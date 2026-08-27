using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.DTOs
{
    // =====================================================================
    // Permintaan masuk
    // =====================================================================

    public class CreateFinancialActionRequest
    {
        [Required]
        public BillingFinancialActionType ActionType { get; set; }

        [Required]
        public Guid FolioId { get; set; }

        public Guid? ChargeLineId { get; set; }

        public Guid? ChargeComponentId { get; set; }

        /// <summary>
        /// Encounter tujuan pada koreksi lintas encounter. Mengisinya membuat permintaan ini
        /// selalu high-risk, tanpa memandang nominal.
        /// </summary>
        public Guid? TargetEncounterId { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Nominal tidak boleh negatif.")]
        public decimal RequestedAmount { get; set; }

        [MaxLength(10)]
        public string Currency { get; set; } = "IDR";

        [Required(ErrorMessage = "Alasan wajib diisi. Tindakan finansial tanpa alasan tidak dapat ditelusuri.")]
        [MaxLength(60)]
        public string ReasonCode { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? ReasonNote { get; set; }

        /// <summary>
        /// Kunci idempotensi pengajuan. Bila diisi, pengiriman ulang dengan kunci yang sama
        /// mengembalikan permintaan yang sudah ada alih-alih membuat yang kedua.
        /// </summary>
        [MaxLength(120)]
        public string? IdempotencyKey { get; set; }
    }

    /// <summary>
    /// Revisi permintaan. Tidak menyunting baris lama: sebuah permintaan baru diterbitkan dengan
    /// nomor revisi berikutnya, dan yang lama dibekukan sebagai riwayat.
    /// </summary>
    public class ReviseFinancialActionRequest
    {
        public Guid? ChargeLineId { get; set; }

        public Guid? ChargeComponentId { get; set; }

        public Guid? TargetEncounterId { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Nominal tidak boleh negatif.")]
        public decimal RequestedAmount { get; set; }

        [MaxLength(10)]
        public string Currency { get; set; } = "IDR";

        [Required]
        [MaxLength(60)]
        public string ReasonCode { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? ReasonNote { get; set; }
    }

    public class DecideFinancialActionRequest
    {
        [Required]
        public BillingApprovalDecision Decision { get; set; }

        /// <summary>
        /// Nominal yang benar-benar disetujui. Bila kosong, nominal yang diajukan yang berlaku.
        /// Tidak boleh melebihi nominal yang diajukan.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal? ApprovedAmount { get; set; }

        [MaxLength(1000)]
        public string? DecisionNote { get; set; }

        /// <summary>
        /// Sidik isi permintaan yang dilihat checker.
        ///
        /// Bila diisi dan tidak sama dengan sidik isi permintaan saat ini, keputusan ditolak —
        /// karena berarti isi yang disetujui bukan isi yang sedang berlaku.
        /// </summary>
        [MaxLength(64)]
        public string? ExpectedContentHash { get; set; }
    }

    public class ExecuteFinancialActionRequest
    {
        [MaxLength(1000)]
        public string? ExecutionNote { get; set; }
    }

    public class CloseFolioRequest
    {
        [MaxLength(1000)]
        public string? Note { get; set; }
    }

    public class ReopenFolioRequest
    {
        /// <summary>
        /// Permintaan <c>FolioReopen</c> yang sudah disetujui. Wajib: membuka kembali folio
        /// tanpa persetujuan bukan pilihan yang disediakan.
        /// </summary>
        [Required]
        public Guid FinancialActionRequestId { get; set; }

        [MaxLength(1000)]
        public string? Note { get; set; }
    }

    // =====================================================================
    // Jawaban keluar
    // =====================================================================

    public class FinancialActionRequestResponse
    {
        public Guid Id { get; set; }

        public string RequestNumber { get; set; } = string.Empty;

        public BillingFinancialActionType ActionType { get; set; }

        public string ActionTypeName { get; set; } = string.Empty;

        public BillingFinancialActionStatus Status { get; set; }

        public string StatusName { get; set; } = string.Empty;

        public BillingFinancialRiskLevel RiskLevel { get; set; }

        public bool RequiresApproval { get; set; }

        public Guid FolioId { get; set; }

        public Guid EncounterId { get; set; }

        public Guid? ChargeLineId { get; set; }

        public Guid? ChargeComponentId { get; set; }

        public Guid? TargetEncounterId { get; set; }

        public decimal RequestedAmount { get; set; }

        public decimal? ExecutedAmount { get; set; }

        public string Currency { get; set; } = "IDR";

        public string ReasonCode { get; set; } = string.Empty;

        public string? ReasonNote { get; set; }

        public Guid MakerUserId { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public DateTime? ExpiresAt { get; set; }

        public DateTime? ExecutedAt { get; set; }

        public Guid? ExecutedByUserId { get; set; }

        public int RevisionNumber { get; set; }

        public Guid? SupersedesRequestId { get; set; }

        public string ContentHash { get; set; } = string.Empty;

        public Guid? ApprovalPolicyId { get; set; }

        public int? ApprovalPolicyVersion { get; set; }

        public string? PolicyBlockReason { get; set; }

        public int Version { get; set; }

        /// <summary>
        /// Kalimat lugas tentang apa yang harus dilakukan berikutnya. Ada supaya petugas tidak
        /// perlu menerjemahkan sendiri arti sebuah status.
        /// </summary>
        public string NextAction { get; set; } = string.Empty;

        public List<FinancialApprovalResponse> Approvals { get; set; } = new();
    }

    public class FinancialApprovalResponse
    {
        public Guid Id { get; set; }

        public BillingApprovalDecision Decision { get; set; }

        public string DecisionName { get; set; } = string.Empty;

        public Guid CheckerUserId { get; set; }

        public DateTime DecidedAt { get; set; }

        public decimal? ApprovedAmount { get; set; }

        public string? DecisionNote { get; set; }

        public string RequestContentHash { get; set; } = string.Empty;

        public BillingFinancialActionStatus PriorStatus { get; set; }

        public BillingFinancialActionStatus ResultingStatus { get; set; }
    }

    public class FolioClosureResponse
    {
        public Guid FolioId { get; set; }

        public Guid EncounterId { get; set; }

        public BillingFolioStatus Status { get; set; }

        public string StatusName { get; set; } = string.Empty;

        public int Version { get; set; }

        public DateTime PerformedAt { get; set; }

        public Guid PerformedByUserId { get; set; }

        public string Message { get; set; } = string.Empty;
    }

    public class FolioClosureHistoryResponse
    {
        public Guid Id { get; set; }

        public Guid FolioId { get; set; }

        public BillingFolioClosureAction Action { get; set; }

        public string ActionName { get; set; } = string.Empty;

        public BillingFolioStatus PriorStatus { get; set; }

        public BillingFolioStatus NewStatus { get; set; }

        public Guid PerformedByUserId { get; set; }

        public DateTime PerformedAt { get; set; }

        public string? Note { get; set; }

        public Guid? FinancialActionRequestId { get; set; }

        public string? ClosureEvidence { get; set; }
    }
}
