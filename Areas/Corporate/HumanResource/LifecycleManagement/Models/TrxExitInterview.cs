using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models
{
    [Table("TrxExitInterview", Schema = "public")]
    public class TrxExitInterview : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required, MaxLength(50)] public string InterviewNumber { get; set; } = string.Empty;
        [Required] public Guid EmployeeSeparationId { get; set; }
        [Required] public Guid WorkforceProfileId { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? InterviewerUserId { get; set; }
        public Guid? InterviewerWorkforceProfileId { get; set; }
        public DateTime? ScheduledAt { get; set; }
        public DateTime? ConductedAt { get; set; }
        [MaxLength(50)] public string InterviewMode { get; set; } = "InPerson";
        [MaxLength(30)] public string InterviewStatus { get; set; } = "Scheduled";
        public decimal? OverallSatisfactionScore { get; set; }
        [MaxLength(2000)] public string? PrimaryReasonForLeaving { get; set; }
        [MaxLength(3000)] public string? PositiveFeedback { get; set; }
        [MaxLength(3000)] public string? ImprovementFeedback { get; set; }
        [MaxLength(2000)] public string? ManagerFeedback { get; set; }
        [MaxLength(2000)] public string? WorkplaceFeedback { get; set; }
        public bool IsRecommendedForRehire { get; set; } = true;
        public bool IsConfidential { get; set; } = true;
        public bool IsActive { get; set; } = true;
        public TrxEmployeeSeparation? EmployeeSeparation { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public ApplicationUser? InterviewerUser { get; set; }
        public MstWorkforceProfile? InterviewerWorkforceProfile { get; set; }
    }
}
