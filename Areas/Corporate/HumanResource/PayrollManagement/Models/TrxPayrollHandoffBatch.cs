using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models
{
    /// <summary>
    /// Paket serah terima final data penggajian terkunci ke Bagian Keuangan (Finance) dengan verifikasi SHA-256 (BE-HRD-146, BE-HRD-158, HRD-DEC-126).
    /// </summary>
    [Table("TrxPayrollHandoffBatch", Schema = "public")]
    public class TrxPayrollHandoffBatch : IdentityModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PayrollRunId { get; set; }

        [Required, MaxLength(60)]
        public string BatchNumber { get; set; } = string.Empty;

        public int PeriodYear { get; set; }
        public int PeriodMonth { get; set; }

        public int TotalEmployees { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalDisbursementAmount { get; set; }

        [Required, MaxLength(64)]
        public string ChecksumSha256 { get; set; } = string.Empty;

        [Required, MaxLength(40)]
        public string HandoffStatus { get; set; } = "PendingTransfer";
        // PendingTransfer, Transferred, AcknowledgedByFinance, RejectedByFinance, Completed

        public DateTime TransferredAt { get; set; }
        public Guid TransferredByUserId { get; set; }

        [MaxLength(100)]
        public string? FinanceReferenceNo { get; set; }

        [MaxLength(2000)]
        public string? FinanceNotes { get; set; }

        [Column(TypeName = "jsonb")]
        public string? PayloadSnapshotJson { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxPayrollRun? PayrollRun { get; set; }
        public ApplicationUser? TransferredByUser { get; set; }
    }
}
