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
        public string? VoiceName { get; set; }
        public string? Gender { get; set; }
        public string? Language { get; set; }
        public decimal? LengthScale { get; set; }
        public decimal? NoiseScale { get; set; }
        public decimal? NoiseW { get; set; }
        public decimal? Volume { get; set; }
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

        [MaxLength(80)]
        public string? VoiceCode { get; set; }

        [MaxLength(30)]
        public string? CallType { get; set; }
    }

    public class QueueVoiceRegenerateRequest
    {
        [MaxLength(30)]
        public string? CallType { get; set; }

        [MaxLength(80)]
        public string? VoiceCode { get; set; }

        public bool ForceRegenerate { get; set; } = true;
    }

    public class QueueVoiceProfileResponse
    {
        public Guid? Id { get; set; }
        public string VoiceCode { get; set; } = string.Empty;
        public string VoiceName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string ModelPath { get; set; } = string.Empty;
        public decimal LengthScale { get; set; }
        public decimal NoiseScale { get; set; }
        public decimal NoiseW { get; set; }
        public decimal Volume { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
        public string Source { get; set; } = "Configuration";
        public string? Description { get; set; }
    }

    public class QueueVoiceProfileRequest
    {
        [Required]
        [MaxLength(80)]
        public string VoiceCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string VoiceName { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Gender { get; set; } = "Female";

        [MaxLength(20)]
        public string Language { get; set; } = "id-ID";

        [Required]
        [MaxLength(500)]
        public string ModelPath { get; set; } = string.Empty;

        [Range(0.70, 1.50)]
        public decimal LengthScale { get; set; } = 1.08m;

        [Range(0.10, 1.50)]
        public decimal NoiseScale { get; set; } = 0.65m;

        [Range(0.10, 1.50)]
        public decimal NoiseW { get; set; } = 0.80m;

        [Range(0.50, 2.00)]
        public decimal Volume { get; set; } = 1.15m;

        public bool IsDefault { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }
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
