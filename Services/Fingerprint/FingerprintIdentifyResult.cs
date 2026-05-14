namespace QuilvianSystemBackend.Services.Fingerprint
{
    public class FingerprintIdentifyResult
    {
        public bool IsMatched { get; set; }

        public Guid? UserId { get; set; }

        public Guid? FingerprintCredentialId { get; set; }

        public int? Score { get; set; }

        public string Message { get; set; } = string.Empty;

        public static FingerprintIdentifyResult NotMatched(string message)
        {
            return new FingerprintIdentifyResult
            {
                IsMatched = false,
                Message = message
            };
        }

        public static FingerprintIdentifyResult Matched(
            Guid userId,
            Guid fingerprintCredentialId,
            int score)
        {
            return new FingerprintIdentifyResult
            {
                IsMatched = true,
                UserId = userId,
                FingerprintCredentialId = fingerprintCredentialId,
                Score = score,
                Message = "Fingerprint match."
            };
        }
    }
}
