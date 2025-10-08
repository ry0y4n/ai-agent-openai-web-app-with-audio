using dotnetfashionassistant.Models;
using dotnetfashionassistant.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace dotnetfashionassistant.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SpeechController : ControllerBase
    {
        private readonly SpeechRecognitionService _speechRecognitionService;
        private readonly ILogger<SpeechController> _logger;

        public SpeechController(SpeechRecognitionService speechRecognitionService, ILogger<SpeechController> logger)
        {
            _speechRecognitionService = speechRecognitionService;
            _logger = logger;
        }

        [HttpPost("transcribe")]
        [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB まで許容（短い録音を想定）
        public async Task<ActionResult<SpeechRecognitionResponse>> TranscribeAsync(
            [FromForm] SpeechTranscriptionRequest request,
            CancellationToken cancellationToken)
        {
            if (request.Audio == null || request.Audio.Length == 0)
            {
                return BadRequest("音声ファイルが添付されていません。");
            }

            try
            {
                var sampleRate = request.SampleRate ?? 16000;
                _logger.LogInformation("Speech transcription request received. FileLength={Length} bytes, SampleRate={SampleRate}, Language={Language}",
                    request.Audio.Length, sampleRate, request.RecognitionLanguage ?? "(default)");

                await using var stream = request.Audio.OpenReadStream();
                var result = await _speechRecognitionService.RecognizeOnceAsync(
                    stream,
                    sampleRate,
                    request.RecognitionLanguage,
                    cancellationToken);

                if (!result.Success)
                {
                    _logger.LogWarning("Speech transcription failed. Reason={Reason}, ErrorCode={ErrorCode}, Details={Details}",
                        result.Reason, result.ErrorCode, result.ErrorDetails);
                    return StatusCode(StatusCodes.Status502BadGateway, result);
                }

                _logger.LogInformation("Speech transcription succeeded. Text='{Text}'", result.RecognizedText);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Speech transcription request failed with exception");
                return StatusCode(StatusCodes.Status500InternalServerError, new SpeechRecognitionResponse
                {
                    Success = false,
                    Reason = "Exception",
                    ErrorDetails = ex.Message,
                });
            }
        }
    }
}
