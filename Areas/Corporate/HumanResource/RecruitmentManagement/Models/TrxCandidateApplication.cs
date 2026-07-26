using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models
{
    [Table("TrxCandidateApplication", Schema = "public")]
    public class TrxCandidateApplication : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string ApplicationNumber { get; set; } = string.Empty;

        [Required]
        public Guid CandidateId { get; set; }

        [Required]
        public Guid JobVacancyId { get; set; }

        public Guid? JobRequisitionId { get; set; }
        public Guid? RecruitmentSourceId { get; set; }
        public Guid? CurrentStageId { get; set; }
        public Guid? CandidateStatusId { get; set; }
        public Guid? AssignedRecruiterUserId { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public Guid? WorkflowInstanceId { get; set; }

        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(30)]
        public string ApplicationStatus { get; set; } = "Submitted";
        // Draft, Submitted, Screening, Assessment, Interview, Offer, Hired, Rejected, Withdrawn, Closed.

        public decimal? OverallScore { get; set; }
        public int CurrentStageOrder { get; set; } = 0;
        public DateTime? LastStageChangedAt { get; set; }
        public DateTime? WithdrawnAt { get; set; }
        public Guid? WithdrawalReasonId { get; set; }
        public DateTime? RejectedAt { get; set; }
        public Guid? RejectionReasonId { get; set; }

        [MaxLength(1000)]
        public string? StatusNotes { get; set; }

        public bool IsInternalCandidate { get; set; } = false;
        public bool IsPriorityCandidate { get; set; } = false;
        public bool IsActive { get; set; } = true;

        public TrxCandidate? Candidate { get; set; }
        public TrxJobVacancy? JobVacancy { get; set; }
        public TrxJobRequisition? JobRequisition { get; set; }
        public MstRecruitmentSource? RecruitmentSource { get; set; }
        public MstRecruitmentStage? CurrentStage { get; set; }
        public MstCandidateStatus? CandidateStatus { get; set; }
        public ApplicationUser? AssignedRecruiterUser { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
        public MstRequestReason? WithdrawalReason { get; set; }
        public MstRejectionReason? RejectionReason { get; set; }
    }
}
