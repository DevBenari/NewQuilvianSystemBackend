using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.DTOs
{
    public class WfpWorkScheduleAssignmentSummaryResponse
    {
        public int TotalData
        {
            get;
            set;
        }
        public int ActiveData
        {
            get;
            set;
        }
        public int PrimaryData
        {
            get;
            set;
        }
        public int RotatingData
        {
            get;
            set;
        }
        public int TemporaryData
        {
            get;
            set;
        }
    }
    public class WfpWorkScheduleAssignmentResponse
    {
        public Guid Id
        {
            get;
            set;
        }
        public Guid WorkforceProfileId
        {
            get;
            set;
        }
        public string WorkforceProfileCode
        {
            get;
            set;
        }
        =string.Empty;
        public string WorkforceDisplayName
        {
            get;
            set;
        }
        =string.Empty;
        public Guid? OrganizationAssignmentId
        {
            get;
            set;
        }
        public Guid? HospitalSiteId
        {
            get;
            set;
        }
        public string? HospitalSiteName
        {
            get;
            set;
        }
        public Guid? OrganizationUnitId
        {
            get;
            set;
        }
        public string? OrganizationUnitName
        {
            get;
            set;
        }
        public Guid? DepartmentId
        {
            get;
            set;
        }
        public string? DepartmentName
        {
            get;
            set;
        }
        public Guid? PositionId
        {
            get;
            set;
        }
        public string? PositionName
        {
            get;
            set;
        }
        public Guid? WorkLocationId
        {
            get;
            set;
        }
        public string? WorkLocationName
        {
            get;
            set;
        }
        public Guid WorkScheduleId
        {
            get;
            set;
        }
        public string WorkScheduleCode
        {
            get;
            set;
        }
        =string.Empty;
        public string WorkScheduleName
        {
            get;
            set;
        }
        =string.Empty;
        public Guid? ShiftGroupId
        {
            get;
            set;
        }
        public string? ShiftGroupName
        {
            get;
            set;
        }
        public Guid? ShiftPatternId
        {
            get;
            set;
        }
        public string? ShiftPatternName
        {
            get;
            set;
        }
        public Guid? RosterPolicyId
        {
            get;
            set;
        }
        public Guid? MinimumRestPolicyId
        {
            get;
            set;
        }
        public string AssignmentType
        {
            get;
            set;
        }
        =string.Empty;
        public DateOnly EffectiveStartDate
        {
            get;
            set;
        }
        public DateOnly? EffectiveEndDate
        {
            get;
            set;
        }
        public int WeekStartDay
        {
            get;
            set;
        }
        public bool IsPrimary
        {
            get;
            set;
        }
        public bool IsRotating
        {
            get;
            set;
        }
        public bool IsTemporary
        {
            get;
            set;
        }
        public bool IsActive
        {
            get;
            set;
        }
        public string? Notes
        {
            get;
            set;
        }
        public DateTime CreateDateTime
        {
            get;
            set;
        }
        public Guid? CreateBy
        {
            get;
            set;
        }
        public string? CreateByName
        {
            get;
            set;
        }
    }
    public class WfpWorkScheduleAssignmentDetailResponse:WfpWorkScheduleAssignmentResponse
    {
        public DateTime? UpdateDateTime
        {
            get;
            set;
        }
        public Guid? UpdateBy
        {
            get;
            set;
        }
        public string? UpdateByName
        {
            get;
            set;
        }
    }
    public class WfpWorkScheduleAssignmentFilterMetadataResponse
    {
        public List<string> AssignmentTypeOptions
        {
            get;
            set;
        }
        =new();
        public List<int> WeekStartDayOptions
        {
            get;
            set;
        }
        =new();
        public List<string> SortDirections
        {
            get;
            set;
        }
        =new();
        public List<int> PageSizeOptions
        {
            get;
            set;
        }
        =new();
    }
    public class CreateWfpWorkScheduleAssignmentRequest
    {
        public Guid? OrganizationAssignmentId
        {
            get;
            set;
        }
        public Guid? HospitalSiteId
        {
            get;
            set;
        }
        public Guid? OrganizationUnitId
        {
            get;
            set;
        }
        public Guid? DepartmentId
        {
            get;
            set;
        }
        public Guid? PositionId
        {
            get;
            set;
        }
        public Guid? WorkLocationId
        {
            get;
            set;
        }
        [Required] public Guid WorkScheduleId
        {
            get;
            set;
        }
        public Guid? ShiftGroupId
        {
            get;
            set;
        }
        public Guid? ShiftPatternId
        {
            get;
            set;
        }
        public Guid? RosterPolicyId
        {
            get;
            set;
        }
        public Guid? MinimumRestPolicyId
        {
            get;
            set;
        }
        [Required,MaxLength(30)] public string AssignmentType
        {
            get;
            set;
        }
        ="Primary";
        [Required] public DateOnly EffectiveStartDate
        {
            get;
            set;
        }
        public DateOnly? EffectiveEndDate
        {
            get;
            set;
        }
        [Range(0,6)] public int WeekStartDay
        {
            get;
            set;
        }
        =1;
        public bool IsPrimary
        {
            get;
            set;
        }
        =true;
        public bool IsRotating
        {
            get;
            set;
        }
        public bool IsTemporary
        {
            get;
            set;
        }
        public bool IsActive
        {
            get;
            set;
        }
        =true;
        [MaxLength(500)] public string? Notes
        {
            get;
            set;
        }
    }
    public class UpdateWfpWorkScheduleAssignmentRequest:CreateWfpWorkScheduleAssignmentRequest
    {
    }
    public class UpdateWfpWorkScheduleAssignmentStatusRequest
    {
        public bool IsActive
        {
            get;
            set;
        }
    }
}
