using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models
{
    [Table("TrxAttendanceProcessingRun", Schema = "public")]
    public class TrxAttendanceProcessingRun : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(60)]
        public string RunNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string ProcessingMode { get; set; }
            = AttendanceValueConstants.ProcessingRunMode.Batch;

        [Required]
        [MaxLength(30)]
        public string RunStatus { get; set; }
            = AttendanceValueConstants.ProcessingRunStatus.Pending;

        [Required]
        [MaxLength(30)]
        public string TriggerSource { get; set; }
            = AttendanceValueConstants.ProcessingTriggerSource.System;

        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        public Guid? TargetWorkforceProfileId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }

        public int ProcessingVersion { get; set; } = 1;
        public int TargetCount { get; set; } = 0;
        public int SuccessCount { get; set; } = 0;
        public int FailedCount { get; set; } = 0;
        public int SkippedCount { get; set; } = 0;

        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? CancelledAt { get; set; }

        public Guid? TriggeredByUserId { get; set; }
        public Guid? CancelledByUserId { get; set; }

        [MaxLength(100)]
        public string? CorrelationId { get; set; }

        public string? ParametersJson { get; set; }

        [MaxLength(2000)]
        public string? ErrorSummary { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? TargetWorkforceProfile { get; set; }
        public MstHospitalSite? HospitalSite { get; set; }
        public MstOrganizationUnit? OrganizationUnit { get; set; }
        public MstDepartment? Department { get; set; }
        public ApplicationUser? TriggeredByUser { get; set; }
        public ApplicationUser? CancelledByUser { get; set; }
    }
}
