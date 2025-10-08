namespace dotnetfashionassistant.Configuration
{
    /// <summary>
    /// Speechサービスへの接続設定を保持するためのオプションクラス。
    /// </summary>
    public class SpeechOptions
    {
        public const string SectionName = "Speech";

        /// <summary>
        /// Speechリソースのエンドポイント URI。
        /// </summary>
        public string? Endpoint { get; set; }

        /// <summary>
        /// Speechリソースのキー。
        /// </summary>
        public string? Key { get; set; }

        /// <summary>
        /// Speechリソースのリージョン（省略可能だが、MediaRecorderなどから送る場合に明示的指定があると便利）。
        /// </summary>
        public string? Region { get; set; }

        /// <summary>
        /// 音声認識を行う言語 (例: "ja-JP").
        /// </summary>
        public string RecognitionLanguage { get; set; } = "ja-JP";
    }
}
