namespace QuilvianSystemBackend.DTOs.Auth
{
    public class FingerprintLoginRequest
    {
        public string FingerprintSampleBase64 { get; set; } = string.Empty;

        public string? DeviceId { get; set; }

        public string? DeviceModel { get; set; }

        public int? SampleFormat { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public double? AccuracyMeters { get; set; }
    }
}
