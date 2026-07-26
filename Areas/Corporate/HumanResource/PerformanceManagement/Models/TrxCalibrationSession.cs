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
    [Table("TrxCalibrationSession", Schema = "public")]
    public class TrxCalibrationSession : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid PerformanceCycleId { get; set; }

        public Guid? OrganizationUnitId { get; set; }

        public Guid? DepartmentId { get; set; }

        [Required]
        [MaxLength(60)]
        public string CalibrationCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string CalibrationName { get; set; } = string.Empty;

        public DateTime ScheduledStartAt { get; set; }

        public DateTime ScheduledEndAt { get; set; }

        [Required]
        [MaxLength(50)]
        public string CalibrationStatus { get; set; } = "Scheduled";

        public Guid? FacilitatorUserId { get; set; }

        public int ParticipantCount { get; set; } = 0;

        public int EmployeeCount { get; set; } = 0;

        public string? ParticipantSnapshotJson { get; set; }

        public string? CalibrationDecisionJson { get; set; }

        public DateTime? FinalizedAt { get; set; }

        public Guid? FinalizedByUserId { get; set; }

        [MaxLength(3000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxPerformanceCycle? PerformanceCycle { get; set; }
        public MstOrganizationUnit? OrganizationUnit { get; set; }
        public MstDepartment? Department { get; set; }
        public ApplicationUser? FacilitatorUser { get; set; }
        public ApplicationUser? FinalizedByUser { get; set; }
    }
}
