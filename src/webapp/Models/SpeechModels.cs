using Microsoft.AspNetCore.Http;

namespace dotnetfashionassistant.Models
{
    public class SpeechRecognitionResponse
    {
        public bool Success { get; set; }
        public string? RecognizedText { get; set; }
        public string? Reason { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorDetails { get; set; }
    }

    public class SpeechTranscriptionRequest
    {
        public IFormFile? Audio { get; set; }
        public int? SampleRate { get; set; }
        public string? RecognitionLanguage { get; set; }
    }
}