namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs
{
    /// <summary>
    /// Ringkasan status operasional kasir untuk perawat bangsal (steril dari nominal rupiah).
    /// </summary>
    public class InpatientBillingStatusResponseDto
    {
        public Guid EpisodeId { get; set; }
        public string EncounterId { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string MedicalRecordNumber { get; set; } = string.Empty;
        public string FolioStatus { get; set; } = string.Empty;
        public string ClearanceStatus { get; set; } = string.Empty;
        public string OperationalStatusText { get; set; } = string.Empty;
        public string StatusColor { get; set; } = string.Empty;
        public bool CanPhysicallyDischarge { get; set; }
        public List<string> BlockerReasons { get; set; } = new();
        public DateTime LastCheckedAtUtc { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Rincian akumulasi biaya finansial lengkap beserta nominal rupiah (khusus staf berizin InpatientBilling:View).
    /// </summary>
    public class InpatientBillingDetailsResponseDto
    {
        public Guid EpisodeId { get; set; }
        public string EncounterId { get; set; } = string.Empty;
        public decimal TotalCharges { get; set; }
        public decimal CoveredAmount { get; set; }
        public decimal PatientExcess { get; set; }
        public decimal DepositPaid { get; set; }
        public decimal OutstandingAmount { get; set; }
        public string ClearanceStatus { get; set; } = string.Empty;
        public List<BillingDetailItemDto> Items { get; set; } = new();
    }

    /// <summary>
    /// Butir rincian tagihan kasir berdasarkan kategori.
    /// </summary>
    public class BillingDetailItemDto
    {
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
