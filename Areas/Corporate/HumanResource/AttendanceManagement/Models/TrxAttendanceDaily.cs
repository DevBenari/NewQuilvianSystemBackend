using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models
{
    [Table("TrxAttendanceDaily", Schema = "public")]
    public class TrxAttendanceDaily : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }

        public Guid? WorkforceProfileId { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? DoctorId { get; set; }
        public UserType UserType { get; set; }

        public Guid? OrganizationAssignmentId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PositionId { get; set; }
        public Guid? WorkLocationId { get; set; }

        public Guid? WorkScheduleId { get; set; }
        public Guid? WorkScheduleAssignmentId { get; set; }
        public Guid? PrimaryShiftAssignmentId { get; set; }
        public Guid? ShiftId { get; set; }
        public Guid? AttendancePolicyId { get; set; }
        public Guid? GracePeriodPolicyId { get; set; }
        public Guid? PayrollPeriodId { get; set; }
        public Guid? AttendancePeriodId { get; set; }

        public DateOnly AttendanceDate { get; set; }

        [Required]
        [MaxLength(40)]
        public string ScheduleSource { get; set; }
            = AttendanceValueConstants.ScheduleSource.Unresolved;

        public string? ScheduleResolutionJson { get; set; }

        public DateTime? ScheduledCheckInAt { get; set; }
        public DateTime? ScheduledCheckOutAt { get; set; }
        public DateTime? FirstCheckInAt { get; set; }
        public DateTime? LastCheckOutAt { get; set; }

        public bool IsOvernightSchedule { get; set; } = false;
        public bool IsHoliday { get; set; } = false;
        public bool IsRestDay { get; set; } = false;
        public bool IsPresent { get; set; } = false;
        public bool IsAbsent { get; set; } = false;
        public bool IsLate { get; set; } = false;
        public bool IsEarlyLeave { get; set; } = false;
        public bool HasMissingPunch { get; set; } = false;
        public bool IsBusinessTrip { get; set; } = false;
        public bool IsRemoteAttendance { get; set; } = false;
        public bool IsCorrected { get; set; } = false;
        public bool IsLocked { get; set; } = false;

        public int ScheduledWorkMinutes { get; set; } = 0;
        public int ActualWorkMinutes { get; set; } = 0;
        public int BreakMinutes { get; set; } = 0;
        public int PayableWorkMinutes { get; set; } = 0;
        public int LateMinutes { get; set; } = 0;
        public int EarlyLeaveMinutes { get; set; } = 0;
        public int OvertimeMinutes { get; set; } = 0;
        public int NightWorkMinutes { get; set; } = 0;
        public int SourceLogCount { get; set; } = 0;
        public int ExceptionCount { get; set; } = 0;

        [Required]
        [MaxLength(50)]
        public string AttendanceStatus { get; set; }
            = AttendanceValueConstants.AttendanceStatus.Unprocessed;

        [Required]
        [MaxLength(30)]
        public string ProcessingStatus { get; set; }
            = AttendanceValueConstants.AttendanceProcessingStatus.Pending;

        public int ProcessingVersion { get; set; } = 1;
        public DateTime? ProcessedAt { get; set; }

        public bool IsPayrollEligible { get; set; } = true;

        [MaxLength(30)]
        public string PayrollInputStatus { get; set; }
            = AttendanceValueConstants.PayrollInputStatus.Pending;

        public DateTime? PayrollProcessedAt { get; set; }

        [MaxLength(1000)]
        public string? ProcessingMessage { get; set; }

        public bool IsActive { get; set; } = true;

        public ApplicationUser? User { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public MstDoctor? Doctor { get; set; }
        public WfpOrganizationAssignment? OrganizationAssignment { get; set; }
        public MstHospitalSite? HospitalSite { get; set; }
        public MstOrganizationUnit? OrganizationUnit { get; set; }
        public MstDepartment? Department { get; set; }
        public MstPosition? Position { get; set; }
        public MstWorkLocation? WorkLocation { get; set; }
        public MstWorkSchedule? WorkSchedule { get; set; }
        public WfpWorkScheduleAssignment? WorkScheduleAssignment { get; set; }
        public TrxShiftAssignment? PrimaryShiftAssignment { get; set; }
        public MstShift? Shift { get; set; }
        public MstAttendancePolicy? AttendancePolicy { get; set; }
        public MstGracePeriodPolicy? GracePeriodPolicy { get; set; }
        public MstPayrollPeriod? PayrollPeriod { get; set; }
        public TrxAttendancePeriod? AttendancePeriod { get; set; }
        public HrdAttendance? Attendance { get; set; }

        public ICollection<TrxAttendanceRawLog> RawLogs { get; set; }
            = new List<TrxAttendanceRawLog>();

        public ICollection<TrxAttendanceDailySegment> Segments { get; set; }
            = new List<TrxAttendanceDailySegment>();

        public ICollection<TrxAttendanceException> Exceptions { get; set; }
            = new List<TrxAttendanceException>();

        public ICollection<TrxAttendanceCorrectionRequest> CorrectionRequests { get; set; }
            = new List<TrxAttendanceCorrectionRequest>();
    }
}
