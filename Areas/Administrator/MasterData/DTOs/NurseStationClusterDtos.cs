using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Administrator.MasterData.DTOs
{
    public class NurseStationClusterSummaryResponse
    {
        public int TotalCluster { get; set; }
        public int ActiveCluster { get; set; }
        public int InactiveCluster { get; set; }
        public int DefaultCluster { get; set; }
        public int AvailableForRegistrationQueue { get; set; }
        public int AvailableForScreening { get; set; }
        public int AvailableForDisplay { get; set; }
    }

    public class NurseStationClusterClinicOverviewResponse
    {
        public Guid MappingId { get; set; }
        public Guid ClinicId { get; set; }
        public string? ClinicCode { get; set; }
        public string? ClinicName { get; set; }
        public bool IsPrimary { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
    }

    public class NurseStationClusterStaffClinicOverviewResponse
    {
        public Guid ClinicId { get; set; }
        public string? ClinicCode { get; set; }
        public string? ClinicName { get; set; }
        public int SortOrder { get; set; }
    }

    public class NurseStationClusterStaffOverviewResponse
    {
        public Guid StaffMappingId { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? EmployeeNumber { get; set; }
        public string? EmployeeName { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string? WorkforceProfileCode { get; set; }
        public string? WorkforceProfileName { get; set; }
        public int ClinicCount { get; set; }
        public List<NurseStationClusterStaffClinicOverviewResponse> Clinics { get; set; } = new();
        public bool IsPrimary { get; set; }
        public bool CanCallQueue { get; set; }
        public bool CanStartScreening { get; set; }
        public bool CanTransferQueue { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
    }

    public class NurseStationClusterResponse
    {
        public Guid Id { get; set; }
        public Guid ServiceUnitId { get; set; }
        public string? ServiceUnitCode { get; set; }
        public string? ServiceUnitName { get; set; }
        public string ClusterCode { get; set; } = string.Empty;
        public string ClusterName { get; set; } = string.Empty;
        public string? ShortName { get; set; }
        public string? LocationName { get; set; }
        public string? FloorName { get; set; }
        public string? RoomName { get; set; }
        public int ClinicCount { get; set; }
        public int StaffCount { get; set; }
        public bool IsAvailableForRegistrationQueue { get; set; }
        public bool IsAvailableForScreening { get; set; }
        public bool IsAvailableForDisplay { get; set; }
        public bool IsDefault { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class NurseStationClusterDetailResponse : NurseStationClusterResponse
    {
        public string? Description { get; set; }
        public List<NurseStationClusterClinicOverviewResponse> Clinics { get; set; } = new();
        public List<NurseStationClusterStaffOverviewResponse> Staffs { get; set; } = new();
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class NurseStationClusterOptionResponse
    {
        public Guid Id { get; set; }
        public Guid ServiceUnitId { get; set; }
        public string? ServiceUnitCode { get; set; }
        public string? ServiceUnitName { get; set; }
        public string ClusterCode { get; set; } = string.Empty;
        public string ClusterName { get; set; } = string.Empty;
        public string? ShortName { get; set; }
        public bool IsDefault { get; set; }
        public int SortOrder { get; set; }
    }

    public class NurseStationClusterOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<NurseStationClusterOptionResponse> Items { get; set; } = new();
    }

    public class NurseStationClusterFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string CustomPeriodPriorityInfo { get; set; } = "Jika customPeriod diisi selain custom, maka startDate dan endDate akan diabaikan.";
        public string ResetButtonLabel { get; set; } = "Reset";
        public NurseStationClusterDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<NurseStationClusterCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<NurseStationClusterSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class NurseStationClusterDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public string? Search { get; set; }
        public Guid? ServiceUnitId { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDefault { get; set; }
        public string SortBy { get; set; } = "sortOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class NurseStationClusterCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class NurseStationClusterSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateNurseStationClusterRequest
    {
        [Required]
        public Guid ServiceUnitId { get; set; }

        [Required]
        [MaxLength(150)]
        public string ClusterName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? ShortName { get; set; }

        [MaxLength(100)]
        public string? LocationName { get; set; }

        [MaxLength(50)]
        public string? FloorName { get; set; }

        [MaxLength(50)]
        public string? RoomName { get; set; }

        public bool IsAvailableForRegistrationQueue { get; set; } = true;
        public bool IsAvailableForScreening { get; set; } = true;
        public bool IsAvailableForDisplay { get; set; } = true;
        public bool IsDefault { get; set; } = false;
        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateNurseStationClusterRequest : CreateNurseStationClusterRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateNurseStationClusterStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class DeleteNurseStationClusterRequest
    {
        [MaxLength(250)]
        public string? DeleteReason { get; set; }
    }

    public class NurseStationClusterCreateResponse
    {
        public Guid Id { get; set; }
        public string ClusterCode { get; set; } = string.Empty;
        public string ClusterName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
