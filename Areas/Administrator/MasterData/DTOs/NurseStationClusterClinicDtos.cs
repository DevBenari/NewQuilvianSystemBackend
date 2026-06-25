using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Administrator.MasterData.DTOs
{
    public class NurseStationClusterClinicSummaryResponse
    {
        public int TotalMapping { get; set; }
        public int ActiveMapping { get; set; }
        public int InactiveMapping { get; set; }
        public int PrimaryMapping { get; set; }
    }

    public class NurseStationClusterClinicResponse
    {
        public Guid Id { get; set; }
        public Guid NurseStationClusterId { get; set; }
        public string? ClusterCode { get; set; }
        public string? ClusterName { get; set; }
        public Guid ClinicId { get; set; }
        public string? ClinicCode { get; set; }
        public string? ClinicName { get; set; }
        public bool IsPrimary { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class NurseStationClusterClinicDetailResponse : NurseStationClusterClinicResponse
    {
        public string? Description { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class NurseStationClusterClinicOptionResponse
    {
        public Guid Id { get; set; }
        public Guid NurseStationClusterId { get; set; }
        public string? ClusterName { get; set; }
        public Guid ClinicId { get; set; }
        public string? ClinicCode { get; set; }
        public string? ClinicName { get; set; }
        public bool IsPrimary { get; set; }
        public int SortOrder { get; set; }
    }

    public class NurseStationClusterClinicOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<NurseStationClusterClinicOptionResponse> Items { get; set; } = new();
    }

    public class NurseStationClusterClinicFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string ResetButtonLabel { get; set; } = "Reset";
        public NurseStationClusterClinicDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<NurseStationClusterClinicSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class NurseStationClusterClinicDefaultFilterResponse
    {
        public Guid? NurseStationClusterId { get; set; }
        public Guid? ClinicId { get; set; }
        public string? Search { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsPrimary { get; set; }
        public string SortBy { get; set; } = "sortOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class NurseStationClusterClinicSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateNurseStationClusterClinicRequest
    {
        [Required]
        public Guid NurseStationClusterId { get; set; }
        [Required]
        public Guid ClinicId { get; set; }
        public bool IsPrimary { get; set; } = false;
        public int SortOrder { get; set; } = 0;
        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateNurseStationClusterClinicRequest : CreateNurseStationClusterClinicRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateNurseStationClusterClinicStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class DeleteNurseStationClusterClinicRequest
    {
        [MaxLength(250)]
        public string? DeleteReason { get; set; }
    }

    public class NurseStationClusterClinicCreateResponse
    {
        public Guid Id { get; set; }
        public Guid NurseStationClusterId { get; set; }
        public Guid ClinicId { get; set; }
        public bool IsActive { get; set; }
    }
}
