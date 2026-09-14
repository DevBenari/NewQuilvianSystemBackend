namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.DTOs
{
    /// <summary>Satu pilihan penyaring bernilai enum pada metadata daftar kerja Bank Darah.</summary>
    public class BloodBankOptionItemResponse
    {
        public int Value { get; set; }
        public string Label { get; set; } = string.Empty;
    }

    /// <summary>Satu pilihan pengurutan pada metadata daftar kerja Bank Darah.</summary>
    public class BloodBankSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    /// <summary>
    /// Satu baris riwayat perpindahan status dari <c>BbkTransitionHistory</c>, untuk scope
    /// permintaan PMI dan kantong darah.
    /// </summary>
    /// <remarks>
    /// <see cref="CorrelationId"/> mengikat baris-baris yang lahir dari satu kejadian: satu
    /// penerimaan yang melahirkan tiga kantong meninggalkan satu baris per kantong, dan ketiganya
    /// membawa id penerimaan yang sama.
    /// </remarks>
    public class BloodBankTransitionDto
    {
        public Guid Id { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? FromStatus { get; set; }
        public string ToStatus { get; set; } = string.Empty;
        public string? ReasonCode { get; set; }
        public string? ReasonNote { get; set; }
        public Guid ActorUserId { get; set; }
        public DateTime OccurredAt { get; set; }
        public Guid? CorrelationId { get; set; }
    }
}
