namespace QuilvianSystemBackend.Services.Fingerprint
{
    public interface IFingerprintIdentificationService
    {
        Task<FingerprintIdentifyResult> IdentifyAsync(
            byte[] capturedSample,
            int? sampleFormat,
            string? deviceId,
            CancellationToken cancellationToken = default);
    }
}
