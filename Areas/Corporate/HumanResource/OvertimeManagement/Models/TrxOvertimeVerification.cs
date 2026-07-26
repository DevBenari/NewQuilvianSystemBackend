using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models
{
    [Table("TrxOvertimeVerification", Schema = "public")]
    public class TrxOvertimeVerification : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid OvertimeRealizationId { get; set; }

        public Guid? WorkflowStepId { get; set; }
        public int VerificationOrder { get; set; } = 1;

        [Required, MaxLength(40)]
        public string VerificationType { get; set; } = "Supervisor";
        // Supervisor, Manager, HR, Payroll.

        public Guid? VerifierUserId { get; set; }
        public Guid? VerifierWorkforceProfileId { get; set; }
        public Guid? RejectionReasonId { get; set; }

        [Required, MaxLength(30)]
        public string VerificationStatus { get; set; } = "Pending";
        // Pending, Approved, Rejected, NeedRevision, Skipped.

        public DateTime? ActionAt { get; set; }
        public int SubmittedMinutes { get; set; } = 0;
        public int EligibleMinutes { get; set; } = 0;
        public int VerifiedMinutes { get; set; } = 0;
        public decimal VerifiedAmount { get; set; } = 0;

        public bool IsAttendanceMatched { get; set; } = false;
        public bool IsPolicyCompliant { get; set; } = false;
        public bool HasVariance { get; set; } = false;
        public bool RequiresRevision { get; set; } = false;
        public bool IsFinalVerification { get; set; } = false;

        [Column(TypeName = "jsonb")]
        public string? VerificationResultJson { get; set; }

        [MaxLength(2000)]
        public string? Comments { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxOvertimeRealization? OvertimeRealization { get; set; }
        public MstWorkflowStep? WorkflowStep { get; set; }
        public ApplicationUser? VerifierUser { get; set; }
        public MstWorkforceProfile? VerifierWorkforceProfile { get; set; }
        public MstRejectionReason? RejectionReason { get; set; }

        public ICollection<TrxCompensatoryTimeOff> CompensatoryTimeOffs { get; set; }
            = new List<TrxCompensatoryTimeOff>();
    }
}
