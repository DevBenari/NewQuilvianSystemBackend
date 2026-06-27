using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs
{
    public static class QueueVoiceCallTypes
    {
        public const string Nurse = "nurse";
        public const string Doctor = "doctor";
        public const string Display = "display";
        public const string Preview = "preview";
        public const string General = "general";
    }

    public class QueueVoiceGenerateResponse
    {
        public bool Enabled { get; set; }
        public bool Generated { get; set; }
        public bool FromCache { get; set; }
        public string CallType { get; set; } = QueueVoiceCallTypes.General;
        public string? VoiceCode { get; set; }
        public string? Text { get; set; }
        public string? AudioUrl { get; set; }
        public string? DownloadUrl { get; set; }
        public string? FileName { get; set; }
        public string? DateKey { get; set; }
        public string ContentType { get; set; } = "audio/mpeg";
        public string? ErrorMessage { get; set; }
    }

    public class QueueVoicePreviewRequest
    {
        [MaxLength(600)]
        public string? Text { get; set; }

        [MaxLength(60)]
        public string? VoiceCode { get; set; }

        [MaxLength(30)]
        public string? CallType { get; set; }
    }

    public class QueueVoiceRegenerateRequest
    {
        [MaxLength(30)]
        public string? CallType { get; set; }

        public bool ForceRegenerate { get; set; } = true;
    }

    public class QueueVoiceCacheCleanupResponse
    {
        public string CacheFolder { get; set; } = string.Empty;
        public int RetentionDays { get; set; }
        public int DeletedFileCount { get; set; }
        public long DeletedBytes { get; set; }
        public DateTime ExecutedAtUtc { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
