using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models
{
    [Table("TrxCandidateInterview", Schema = "public")]
    public class TrxCandidateInterview : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid CandidateApplicationId { get; set; }
        public Guid? InterviewTemplateId { get; set; }
        public Guid? RecruitmentStageId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? WorkLocationId { get; set; }
        public Guid? PanelLeadUserId { get; set; }
        public Guid? PanelLeadWorkforceProfileId { get; set; }

        public int InterviewRound { get; set; } = 1;

        [Required]
        [MaxLength(30)]
        public string InterviewType { get; set; } = "UserInterview";

        [Required]
        [MaxLength(20)]
        public string InterviewMode { get; set; } = "OnSite";
        // OnSite, Online, Phone, Hybrid.

        public DateTime ScheduledStartAt { get; set; }
        public DateTime? ScheduledEndAt { get; set; }
        public DateTime? ActualStartAt { get; set; }
        public DateTime? ActualEndAt { get; set; }

        [MaxLength(500)]
        public string? LocationDescription { get; set; }

        [MaxLength(1000)]
        public string? MeetingUrl { get; set; }

        [MaxLength(30)]
        public string InterviewStatus { get; set; } = "Scheduled";
        // Scheduled, Confirmed, InProgress, Completed, Rescheduled, Cancelled, NoShow.

        public decimal? FinalScore { get; set; }

        [MaxLength(30)]
        public string? FinalRecommendation { get; set; }
        // StrongHire, Hire, Hold, NoHire, StrongNoHire.

        public int RequiredEvaluationCount { get; set; } = 1;
        public int SubmittedEvaluationCount { get; set; } = 0;
        public string? PanelDefinitionJson { get; set; }
        public string? EvaluationSummaryJson { get; set; }

        [MaxLength(1500)]
        public string? FinalNotes { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxCandidateApplication? CandidateApplication { get; set; }
        public MstInterviewTemplate? InterviewTemplate { get; set; }
        public MstRecruitmentStage? RecruitmentStage { get; set; }
        public MstHospitalSite? HospitalSite { get; set; }
        public MstWorkLocation? WorkLocation { get; set; }
        public ApplicationUser? PanelLeadUser { get; set; }
        public MstWorkforceProfile? PanelLeadWorkforceProfile { get; set; }
    }
}
