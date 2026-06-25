using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Administrator.MasterData.DTOs
{
    public class NurseStationClusterStaffSummaryResponse
    {
        public int TotalStaff { get; set; }
        public int ActiveStaff { get; set; }
        public int InactiveStaff { get; set; }
        public int PrimaryStaff { get; set; }
        public int CanCallQueueStaff { get; set; }
        public int CanStartScreeningStaff { get; set; }
        public int CanTransferQueueStaff { get; set; }
    }

    public class NurseStationClusterStaffResponse
    {
        public Guid Id { get; set; }
        public Guid NurseStationClusterId { get; set; }
        public string? ClusterCode { get; set; }
        public string? ClusterName { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? EmployeeNumber { get; set; }
        public string? EmployeeName { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string? WorkforceProfileCode { get; set; }
        public string? WorkforceProfileName { get; set; }
        public bool IsPrimary { get; set; }
        public bool CanCallQueue { get; set; }
        public bool CanStartScreening { get; set; }
        public bool CanTransferQueue { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class NurseStationClusterStaffDetailResponse : NurseStationClusterStaffResponse
    {
        public string? Description { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class NurseStationClusterStaffOptionResponse
    {
        public Guid Id { get; set; }
        public Guid NurseStationClusterId { get; set; }
        public string? ClusterName { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }
        public bool IsPrimary { get; set; }
        public int SortOrder { get; set; }
    }

    public class NurseStationClusterStaffOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<NurseStationClusterStaffOptionResponse> Items { get; set; } = new();
    }

    public class NurseStationClusterStaffFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string ResetButtonLabel { get; set; } = "Reset";
        public NurseStationClusterStaffDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<NurseStationClusterStaffSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class NurseStationClusterStaffDefaultFilterResponse
    {
        public Guid? NurseStationClusterId { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? WorkforceProfileId { get; set; }
        public string? Search { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsPrimary { get; set; }
        public string SortBy { get; set; } = "sortOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class NurseStationClusterStaffSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateNurseStationClusterStaffRequest
    {
        [Required]
        public Guid NurseStationClusterId { get; set; }
        [Required]
        public Guid EmployeeId { get; set; }
        public Guid? WorkforceProfileId { get; set; }
        public bool IsPrimary { get; set; } = false;
        public bool CanCallQueue { get; set; } = true;
        public bool CanStartScreening { get; set; } = true;
        public bool CanTransferQueue { get; set; } = true;
        public int SortOrder { get; set; } = 0;
        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateNurseStationClusterStaffRequest : CreateNurseStationClusterStaffRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateNurseStationClusterStaffStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class DeleteNurseStationClusterStaffRequest
    {
        [MaxLength(250)]
        public string? DeleteReason { get; set; }
    }

    public class NurseStationClusterStaffCreateResponse
    {
        public Guid Id { get; set; }
        public Guid NurseStationClusterId { get; set; }
        public Guid EmployeeId { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public bool IsActive { get; set; }
    }
}
