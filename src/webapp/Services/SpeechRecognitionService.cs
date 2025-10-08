using System.Buffers;
using dotnetfashionassistant.Configuration;
using dotnetfashionassistant.Models;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace dotnetfashionassistant.Services
{
    /// <summary>
    /// Azure Speech SDK を利用して音声認識を行うサービス。
    /// サーバー側でストリームとして受け取ったマイク音声を <see cref="RecognizeOnceAsync"/> を通してテキスト化する。
    /// </summary>
    public class SpeechRecognitionService
    {
        private readonly SpeechOptions _speechOptions;
        private readonly ILogger<SpeechRecognitionService> _logger;

        public SpeechRecognitionService(IOptions<SpeechOptions> speechOptions, ILogger<SpeechRecognitionService> logger)
        {
            _speechOptions = speechOptions.Value ?? throw new ArgumentNullException(nameof(speechOptions));
            _logger = logger;
        }

        /// <summary>
        /// 1 回分の音声ストリームを同期的に認識する。PCM (16kHz/16bit/mono) を想定。
        /// </summary>
        public async Task<SpeechRecognitionResponse> RecognizeOnceAsync(
            Stream audioStream,
            int sampleRate,
            string? recognitionLanguage = null,
            CancellationToken cancellationToken = default)
        {
            if (audioStream == null)
            {
                throw new ArgumentNullException(nameof(audioStream));
            }

            if (string.IsNullOrWhiteSpace(_speechOptions.Key))
            {
                throw new InvalidOperationException("Speech キーが未設定です。appsettings または環境変数を確認してください。");
            }

            SpeechConfig speechConfig = CreateSpeechConfig();
            speechConfig.SpeechRecognitionLanguage = !string.IsNullOrWhiteSpace(recognitionLanguage)
                ? recognitionLanguage
                : _speechOptions.RecognitionLanguage;

            // クライアント（ブラウザー）側で PCM に変換された音声を送信する前提。
            var normalizedSampleRate = sampleRate > 0 ? sampleRate : 16000;
            var audioFormat = AudioStreamFormat.GetWaveFormatPCM((uint)normalizedSampleRate, 16, 1);
            using var pushStream = AudioInputStream.CreatePushStream(audioFormat);
            using var audioInput = AudioConfig.FromStreamInput(pushStream);

            _logger.LogInformation("Starting speech recognition. SampleRate={SampleRate}, Language={Language}", normalizedSampleRate, speechConfig.SpeechRecognitionLanguage);

            // 入力ストリームを PushAudioInputStream へ書き出し。
            await WriteStreamToPushStreamAsync(audioStream, pushStream, cancellationToken).ConfigureAwait(false);

            using var recognizer = new SpeechRecognizer(speechConfig, audioInput);

            try
            {
                var result = await recognizer.RecognizeOnceAsync().ConfigureAwait(false);
                _logger.LogInformation("Speech recognition completed. Reason={Reason}", result.Reason);
                return MapResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Speech サービス呼び出し中にエラーが発生しました。");
                return new SpeechRecognitionResponse
                {
                    Success = false,
                    Reason = ResultReason.Canceled.ToString(),
                    ErrorDetails = ex.Message,
                    ErrorCode = ex.GetType().Name
                };
            }
        }

        private SpeechConfig CreateSpeechConfig()
        {
            if (!string.IsNullOrWhiteSpace(_speechOptions.Region))
            {
                return SpeechConfig.FromSubscription(_speechOptions.Key!, _speechOptions.Region);
            }

            if (!string.IsNullOrWhiteSpace(_speechOptions.Endpoint))
            {
                return SpeechConfig.FromEndpoint(new Uri(_speechOptions.Endpoint), _speechOptions.Key);
            }

            throw new InvalidOperationException("Speech の構成には Region もしくは Endpoint の指定が必要です。");
        }

        private async Task WriteStreamToPushStreamAsync(Stream source, PushAudioInputStream destination, CancellationToken cancellationToken)
        {
            var rentedBuffer = ArrayPool<byte>.Shared.Rent(8192);
            try
            {
                int bytesRead;
                while ((bytesRead = await source.ReadAsync(rentedBuffer.AsMemory(0, rentedBuffer.Length), cancellationToken).ConfigureAwait(false)) > 0)
                {
                    destination.Write(rentedBuffer, bytesRead);
                }
            }
            finally
            {
                destination.Close();
                ArrayPool<byte>.Shared.Return(rentedBuffer);
            }
        }

        private SpeechRecognitionResponse MapResult(Microsoft.CognitiveServices.Speech.SpeechRecognitionResult sdkResult)
        {
            switch (sdkResult.Reason)
            {
                case ResultReason.RecognizedSpeech:
                    return new SpeechRecognitionResponse
                    {
                        Success = true,
                        RecognizedText = sdkResult.Text,
                        Reason = sdkResult.Reason.ToString()
                    };

                case ResultReason.NoMatch:
                    _logger.LogWarning("音声を認識できませんでした (NoMatch)。");
                    return new SpeechRecognitionResponse
                    {
                        Success = false,
                        Reason = sdkResult.Reason.ToString(),
                        ErrorDetails = "音声を認識できませんでした。"
                    };

                case ResultReason.Canceled:
                    var cancellation = CancellationDetails.FromResult(sdkResult);
                    _logger.LogWarning("音声認識がキャンセルされました。Reason={Reason}, ErrorCode={ErrorCode}, Details={Details}",
                        cancellation.Reason, cancellation.ErrorCode, cancellation.ErrorDetails);
                    return new SpeechRecognitionResponse
                    {
                        Success = false,
                        Reason = sdkResult.Reason.ToString(),
                        ErrorCode = cancellation.ErrorCode.ToString(),
                        ErrorDetails = cancellation.ErrorDetails
                    };

                default:
                    _logger.LogWarning("予期していない認識結果 Reason={Reason}", sdkResult.Reason);
                    return new SpeechRecognitionResponse
                    {
                        Success = false,
                        Reason = sdkResult.Reason.ToString(),
                        ErrorDetails = "未対応の認識結果が返却されました。"
                    };
            }
        }
    }
}
