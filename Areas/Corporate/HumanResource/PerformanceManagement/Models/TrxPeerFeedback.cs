using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Performance.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models
{
    [Table("TrxPeerFeedback", Schema = "public")]
    public class TrxPeerFeedback : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid PerformanceCycleId { get; set; }

        public Guid SubjectWorkforceProfileId { get; set; }

        public Guid? ReviewerWorkforceProfileId { get; set; }

        public Guid? ReviewerUserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string RelationshipType { get; set; } = "Peer";

        public bool IsAnonymous { get; set; } = false;

        [Required]
        [MaxLength(40)]
        public string FeedbackStatus { get; set; } = "Draft";

        public decimal? OverallRating { get; set; }

        [MaxLength(3000)]
        public string? StrengthFeedback { get; set; }

        [MaxLength(3000)]
        public string? DevelopmentFeedback { get; set; }

        [MaxLength(3000)]
        public string? AdditionalComments { get; set; }

        public string? FeedbackJson { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxPerformanceCycle? PerformanceCycle { get; set; }
        public MstWorkforceProfile? SubjectWorkforceProfile { get; set; }
        public MstWorkforceProfile? ReviewerWorkforceProfile { get; set; }
        public ApplicationUser? ReviewerUser { get; set; }
    }
}
