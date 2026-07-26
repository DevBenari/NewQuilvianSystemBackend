using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models
{
    [Table("WfpContractHistory", Schema = "public")]
    public class WfpContractHistory : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public Guid? PreviousContractHistoryId { get; set; }
        public Guid? ContractTypeId { get; set; }
        public Guid? EmploymentTypeId { get; set; }
        public Guid? WorkerSourceId { get; set; }
        public Guid? TerminationReasonId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ContractNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string HistoryType { get; set; } = "Initial";
        // Initial, Renewal, Amendment, Extension, Suspension, Termination, Expiry.

        [Required]
        [MaxLength(50)]
        public string ContractStatus { get; set; } = "Draft";
        // Draft, Active, Suspended, Ended, Expired, Terminated, Cancelled.

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }
        public DateTime? SignedDate { get; set; }
        public DateTime? ProbationEndDate { get; set; }
        public DateTime? TerminatedAt { get; set; }
        public int RenewalSequence { get; set; } = 0;
        public bool IsCurrent { get; set; } = false;

        [MaxLength(500)]
        public string? DocumentPath { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public WfpContractHistory? PreviousContractHistory { get; set; }
        public ICollection<WfpContractHistory> Renewals { get; set; } = new List<WfpContractHistory>();
        public MstContractType? ContractType { get; set; }
        public MstEmploymentType? EmploymentType { get; set; }
        public MstWorkerSource? WorkerSource { get; set; }
        public MstTerminationReason? TerminationReason { get; set; }
    }
}
