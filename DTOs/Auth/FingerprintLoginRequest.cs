namespace QuilvianSystemBackend.DTOs.Auth
{
    public class FingerprintLoginRequest
    {
        public Guid CredentialId { get; set; }
        public Guid UserId { get; set; }
        public int? Score { get; set; }
        public string? DeviceId { get; set; }
        public string? DeviceModel { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? AccuracyMeters { get; set; }
    }
}
