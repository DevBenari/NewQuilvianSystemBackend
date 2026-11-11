using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs
{
    public class OvertimeBiometricVerificationResultDto
    {
        public Guid VerificationId { get; set; }
        public Guid RealizationId { get; set; }
        public string RealizationNumber { get; set; } = string.Empty;
        public string VerificationStatus { get; set; } = string.Empty;
        public int ClaimedMinutes { get; set; }
        public int BiometricExcessMinutes { get; set; }
        public int EligibleMinutes { get; set; }
        public int VerifiedPayableMinutes { get; set; }
        public decimal VerifiedPayableHours => (decimal)VerifiedPayableMinutes / 60m;
        public bool IsDiscrepancyFlagged { get; set; }
        public string? DiscrepancyReason { get; set; }
        public DateTime? BiometricCheckIn { get; set; }
        public DateTime? BiometricCheckOut { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class OvertimeDiscrepancyQueueItemDto
    {
        public Guid VerificationId { get; set; }
        public Guid RealizationId { get; set; }
        public string RealizationNumber { get; set; } = string.Empty;
        public Guid WorkforceProfileId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string UnitName { get; set; } = string.Empty;
        public DateOnly WorkDate { get; set; }
        public int ClaimedMinutes { get; set; }
        public int BiometricExcessMinutes { get; set; }
        public int VarianceMinutes { get; set; }
        public DateTime? BiometricCheckIn { get; set; }
        public DateTime? BiometricCheckOut { get; set; }
        public string? DiscrepancyReason { get; set; }
        public string VerificationStatus { get; set; } = string.Empty;
        public string? EmergencyClinicalReferenceNo { get; set; }
        public DateTime FlaggedAt { get; set; }
    }

    public class ResolveOvertimeDiscrepancyCommand
    {
        [Range(0, 1440, ErrorMessage = "Durasi penyesuaian harus antara 0 hingga 1440 menit.")]
        public int AdjustedMinutes { get; set; }

        [Required(ErrorMessage = "Nomor referensi logbook fisik ruangan / surat pengantar wajib diisi.")]
        [MaxLength(100)]
        public string PhysicalLogbookReferenceNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Catatan justifikasi tertulis penyesuaian manual HR wajib diisi.")]
        [MaxLength(2000)]
        public string ManualAdjustmentNotes { get; set; } = string.Empty;

        [Required(ErrorMessage = "Aksi penyelesaian wajib dipilih (ApproveAdjusted / RejectDiscrepancy).")]
        [MaxLength(30)]
        public string ResolutionAction { get; set; } = "ApproveAdjusted"; // ApproveAdjusted, RejectDiscrepancy
    }

    public class ResolveOvertimeDiscrepancyResultDto
    {
        public Guid VerificationId { get; set; }
        public Guid RealizationId { get; set; }
        public string RealizationNumber { get; set; } = string.Empty;
        public string PreviousVerificationStatus { get; set; } = string.Empty;
        public string NewVerificationStatus { get; set; } = string.Empty;
        public int AdjustedMinutes { get; set; }
        public int VerifiedPayableMinutes { get; set; }
        public decimal VerifiedPayableHours => (decimal)VerifiedPayableMinutes / 60m;
        public string RealizationStatus { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
