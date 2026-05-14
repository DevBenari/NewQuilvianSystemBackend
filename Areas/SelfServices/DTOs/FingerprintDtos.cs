namespace QuilvianSystemBackend.Areas.SelfServices.DTOs
{
    public class FingerprintRegisterRequest
    {
        public string FingerPosition { get; set; } = "RightThumb";

        public string TemplateFormat { get; set; } = "DigitalPersona.SampleFormat5";

        public string? TemplateVersion { get; set; }

        public string FingerprintTemplateBase64 { get; set; } = string.Empty;

        // Optional alias kalau frontend masih kirim field fingerprintSample
        public string? FingerprintSample { get; set; }

        public string? DeviceId { get; set; }

        public string? DeviceModel { get; set; }

        public int? SampleFormat { get; set; }

        public int? QualityScore { get; set; }

        public int EnrollmentSampleCount { get; set; } = 1;
    }

    public class FingerprintCredentialResponse
    {
        public Guid Id { get; set; }

        public string FingerPosition { get; set; } = string.Empty;

        public string TemplateFormat { get; set; } = string.Empty;

        public string? TemplateVersion { get; set; }

        public string? DeviceId { get; set; }

        public string? DeviceModel { get; set; }

        public int? SampleFormat { get; set; }

        public int? QualityScore { get; set; }

        public int EnrollmentSampleCount { get; set; }

        public bool IsPrimary { get; set; }

        public DateTime RegisteredAt { get; set; }
    }

    public class FingerprintStatusResponse
    {
        public Guid UserId { get; set; }

        public string UserType { get; set; } = string.Empty;

        public bool CanRegister { get; set; }

        public string Message { get; set; } = string.Empty;

        public bool HasFingerprint { get; set; }

        public int FingerprintCount { get; set; }

        public List<FingerprintCredentialResponse> Fingerprints { get; set; } = new();
    }

    public class FingerprintMetadataResponse
    {
        public List<FingerprintOptionResponse> FingerPositions { get; set; } = new();

        public List<FingerprintOptionResponse> TemplateFormats { get; set; } = new();

        public List<string> AllowedUserTypes { get; set; } = new();

        public string DefaultFingerPosition { get; set; } = "RightThumb";

        public string DefaultTemplateFormat { get; set; } = "DigitalPersona.SampleFormat5";
    }

    public class FingerprintOptionResponse
    {
        public string Value { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;
    }
}
