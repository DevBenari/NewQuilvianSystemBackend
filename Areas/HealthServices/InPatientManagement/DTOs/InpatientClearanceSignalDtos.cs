namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs
{
    /// <summary>
    /// Payload webhook sinyal kelayakan pemulangan dari kasir (ClearanceApproved / ClearanceRevoked).
    /// </summary>
    public class ClearanceSignalWebhookDto
    {
        public string EncounterId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public string? RevokedByCashierName { get; set; }
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    }
}
