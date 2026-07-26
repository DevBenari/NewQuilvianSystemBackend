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
    [Table("TrxPerformanceCheckIn", Schema = "public")]
    public class TrxPerformanceCheckIn : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid PerformanceCycleId { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public Guid ManagerUserId { get; set; }

        public Guid? EmployeeGoalId { get; set; }

        public DateTime CheckInDate { get; set; }

        [Required]
        [MaxLength(40)]
        public string CheckInStatus { get; set; } = "Completed";

        public decimal? ProgressPercentage { get; set; }

        [MaxLength(3000)]
        public string? DiscussionTopics { get; set; }

        [MaxLength(3000)]
        public string? ManagerFeedback { get; set; }

        [MaxLength(3000)]
        public string? EmployeeFeedback { get; set; }

        public string? ActionItemsJson { get; set; }

        public DateTime? NextCheckInDate { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxPerformanceCycle? PerformanceCycle { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public ApplicationUser? ManagerUser { get; set; }
        public TrxEmployeeGoal? EmployeeGoal { get; set; }
    }
}
