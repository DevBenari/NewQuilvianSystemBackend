using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models
{
    /// <summary>
    /// Pencatatan anomali lonjakan atau penurunan gaji bersih staf >= 20% atau >= Rp 2.000.000 (BE-HRD-146, BE-HRD-155, HRD-DEC-124).
    /// </summary>
    [Table("TrxPayrollVarianceAlert", Schema = "public")]
    public class TrxPayrollVarianceAlert : IdentityModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PayrollRunId { get; set; }

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public int PeriodYear { get; set; }
        public int PeriodMonth { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PreviousNetPay { get; set; } = 0m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentNetPay { get; set; } = 0m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal VarianceAmount { get; set; } = 0m;

        [Column(TypeName = "decimal(5,2)")]
        public decimal VariancePercentage { get; set; } = 0m;

        [Required, MaxLength(30)]
        public string AlertSeverity { get; set; } = "Warning"; // Warning, High, Critical

        [Required, MaxLength(500)]
        public string AlertReason { get; set; } = string.Empty;

        [Required, MaxLength(30)]
        public string ResolutionStatus { get; set; } = "PendingReview"; // PendingReview, Justified, Rejected

        [MaxLength(2000)]
        public string? JustificationNotes { get; set; }

        public Guid? ResolvedByUserId { get; set; }
        public DateTime? ResolvedAt { get; set; }

        public bool IsActive { get; set; } = true;

        public PrlPayrollRun? PayrollRun { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public ApplicationUser? ResolvedByUser { get; set; }
    }
}
