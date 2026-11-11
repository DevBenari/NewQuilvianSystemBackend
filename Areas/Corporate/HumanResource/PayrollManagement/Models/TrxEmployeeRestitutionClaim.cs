using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models
{
    /// <summary>
    /// Pengajuan klaim restitusi mandiri oleh pegawai di ESS (BE-HRD-161, BE-HRD-167, HRD-DEC-130).
    /// </summary>
    [Table("TrxEmployeeRestitutionClaim", Schema = "public")]
    public class TrxEmployeeRestitutionClaim : IdentityModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(100)]
        public string ClaimNumber { get; set; } = string.Empty;

        [Required]
        public Guid EmployeeId { get; set; }

        [Required, MaxLength(100)]
        public string ClaimCategory { get; set; } = "KacamataKerja";
        // 'KacamataKerja', 'FaskesLuarDarurat', 'DinasCitoMedis'

        public DateTime ExpenseDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ClaimAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ApprovedAmount { get; set; } = 0.00m;

        [Required]
        public string ReceiptAttachmentUrl { get; set; } = string.Empty;

        public string? MedicalResumeAttachmentUrl { get; set; }

        [Required, MaxLength(50)]
        public string ClaimStatus { get; set; } = "Draft";
        // 'Draft', 'Submitted', 'SupervisorApproved', 'HrReviewed', 'Approved', 'Rejected', 'QueuedForPayroll', 'ProcessedInPayroll', 'VoucherIssued', 'DisbursedViaCashier', 'Closed'

        [Required]
        public string ReasonNotes { get; set; } = string.Empty;

        public string? RejectionReason { get; set; }

        public Guid? SupervisorId { get; set; }
        public DateTime? SupervisorApprovedAt { get; set; }

        public Guid? HrReviewerId { get; set; }
        public DateTime? HrReviewedAt { get; set; }

        public Guid? FinalApproverId { get; set; }
        public DateTime? FinalApprovedAt { get; set; }

        public bool IsCitoEmergency { get; set; } = false;

        public MstEmployee? Employee { get; set; }
        public MstEmployee? Supervisor { get; set; }
        public ApplicationUser? HrReviewer { get; set; }
        public ApplicationUser? FinalApprover { get; set; }
    }
}
