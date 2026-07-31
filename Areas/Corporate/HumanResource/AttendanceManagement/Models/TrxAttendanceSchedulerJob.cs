using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models
{
    [Table("TrxAttendanceSchedulerJob", Schema = "public")]
    public class TrxAttendanceSchedulerJob : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(60)]
        public string JobNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string JobType { get; set; }
            = AttendanceValueConstants.AttendanceSchedulerJobType.ProcessRange;

        [Required]
        [MaxLength(30)]
        public string JobStatus { get; set; }
            = AttendanceValueConstants.AttendanceSchedulerJobStatus.Pending;

        public Guid? AttendancePeriodId { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        public Guid? WorkforceProfileId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }

        public bool ForceReprocess { get; set; } = false;
        public int Priority { get; set; } = 100;
        public int RetryCount { get; set; } = 0;
        public int MaxRetryCount { get; set; } = 3;

        public DateTime ScheduledAt { get; set; } = DateTime.UtcNow;
        public DateTime AvailableAt { get; set; } = DateTime.UtcNow;
        public DateTime? StartedAt { get; set; }
        public DateTime? HeartbeatAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? FailedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public DateTime? NextRetryAt { get; set; }

        [MaxLength(200)]
        public string? WorkerInstanceId { get; set; }

        public Guid? ProcessingRunId { get; set; }
        public Guid? TriggeredByUserId { get; set; }
        public Guid? CancelledByUserId { get; set; }

        [MaxLength(100)]
        public string? CorrelationId { get; set; }

        public string? ParametersJson { get; set; }

        [MaxLength(4000)]
        public string? LastError { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxAttendancePeriod? AttendancePeriod { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstHospitalSite? HospitalSite { get; set; }
        public MstOrganizationUnit? OrganizationUnit { get; set; }
        public MstDepartment? Department { get; set; }
        public TrxAttendanceProcessingRun? ProcessingRun { get; set; }
        public ApplicationUser? TriggeredByUser { get; set; }
        public ApplicationUser? CancelledByUser { get; set; }
    }
}
