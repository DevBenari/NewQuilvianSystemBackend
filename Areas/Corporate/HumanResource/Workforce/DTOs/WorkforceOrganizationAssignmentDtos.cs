using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs
{
    public class WorkforceOrganizationAssignmentSummaryResponse
    {
        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int TotalOrganizationAssignment { get; set; }

        public int ActiveOrganizationAssignment { get; set; }

        public int InactiveOrganizationAssignment { get; set; }

        public int PrimaryOrganizationAssignment { get; set; }

        public int CurrentlyValidOrganizationAssignment { get; set; }

        public int ExpiredOrganizationAssignment { get; set; }
    }

    public class WorkforceOrganizationAssignmentResponse
    {
        public Guid Id { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public Guid DepartmentId { get; set; }

        public string DepartmentCode { get; set; } = string.Empty;

        public string DepartmentName { get; set; } = string.Empty;

        public Guid PositionId { get; set; }

        public string PositionCode { get; set; } = string.Empty;

        public string PositionName { get; set; } = string.Empty;

        public bool IsPrimary { get; set; }

        public bool IsActive { get; set; }

        public DateTime EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public bool IsExpired { get; set; }

        public bool IsCurrentlyValid { get; set; }

        public string? Description { get; set; }

        public DateTime CreateDateTime { get; set; }

        public Guid? CreateBy { get; set; }

        public string? CreateByName { get; set; }
    }

    public class WorkforceOrganizationAssignmentDetailResponse : WorkforceOrganizationAssignmentResponse
    {
        public DateTime? UpdateDateTime { get; set; }

        public Guid? UpdateBy { get; set; }

        public string? UpdateByName { get; set; }
    }

    public class WorkforceOrganizationAssignmentOptionResponse
    {
        public Guid Id { get; set; }

        public Guid DepartmentId { get; set; }

        public string DepartmentCode { get; set; } = string.Empty;

        public string DepartmentName { get; set; } = string.Empty;

        public Guid PositionId { get; set; }

        public string PositionCode { get; set; } = string.Empty;

        public string PositionName { get; set; } = string.Empty;

        public bool IsPrimary { get; set; }

        public bool IsActive { get; set; }

        public DateTime EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }
    }

    public class WorkforceOrganizationAssignmentOptionPagedResponse
    {
        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public int TotalData { get; set; }

        public int TotalPage { get; set; }

        public List<WorkforceOrganizationAssignmentOptionResponse> Items { get; set; } = new();
    }

    public class WorkforceOrganizationAssignmentFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";

        public string DateFilterInfo { get; set; } =
            "Filter tanggal menggunakan EffectiveStartDate. Jika customPeriod diisi selain custom, startDate dan endDate boleh dikosongkan.";

        public string ResetButtonLabel { get; set; } = "Reset";

        public WorkforceOrganizationAssignmentDefaultFilterResponse DefaultFilter { get; set; } = new();

        public List<WorkforceOrganizationAssignmentCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();

        public List<WorkforceOrganizationAssignmentSortOptionResponse> SortOptions { get; set; } = new();

        public List<string> SortDirections { get; set; } = new();

        public List<int> PageSizeOptions { get; set; } = new();

        public WorkforceOrganizationAssignmentRelationFilterInfoResponse RelationFilterInfo { get; set; } = new();

        public WorkforceOrganizationAssignmentFrontendGuideResponse FrontendGuide { get; set; } = new();
    }

    public class WorkforceOrganizationAssignmentDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string? CustomPeriod { get; set; }

        public Guid? DepartmentId { get; set; }

        public Guid? PositionId { get; set; }

        public bool? IsPrimary { get; set; }

        public bool? IsActive { get; set; }

        public bool? IsExpired { get; set; }

        public string? Search { get; set; }

        public string SortBy { get; set; } = "effectiveStartDate";

        public string SortDirection { get; set; } = "desc";

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 25;
    }

    public class WorkforceOrganizationAssignmentCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;
    }

    public class WorkforceOrganizationAssignmentSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;
    }

    public class WorkforceOrganizationAssignmentRelationFilterInfoResponse
    {
        public string FirstRelationFilter { get; set; } =
            "DepartmentId digunakan untuk filter assignment berdasarkan department.";

        public string SecondRelationFilter { get; set; } =
            "PositionId digunakan untuk filter assignment berdasarkan position.";

        public string PositionDependencyInfo { get; set; } =
            "PositionId harus sesuai dengan DepartmentId pada saat create/update.";
    }

    public class WorkforceOrganizationAssignmentFrontendGuideResponse
    {
        public string Purpose { get; set; } =
            "Organization assignment digunakan untuk menyimpan multi jabatan atau multi department pada satu workforce profile.";

        public string PrimaryInstruction { get; set; } =
            "Hanya boleh ada satu assignment primary aktif per workforce profile. Primary assignment akan disinkronkan ke MstWorkforceProfile, MstEmployee/MstDoctor, ApplicationUser, dan ApplicationUserOrganization.";

        public string CreateInstruction { get; set; } =
            "Jika assignment pertama dibuat dan aktif, backend otomatis menjadikannya primary walaupun IsPrimary dikirim false.";

        public string UpdateInstruction { get; set; } =
            "Primary assignment tidak boleh dinonaktifkan atau diberi EffectiveEndDate. Jadikan assignment lain sebagai primary terlebih dahulu.";

        public string FilterInstruction { get; set; } =
            "Untuk UI filter gunakan maksimal 2 dropdown relasi: Department dan Position. Field lain gunakan status aktif, primary, expired, search, sorting, dan paging.";
    }

    public class CreateWorkforceOrganizationAssignmentRequest
    {
        [Required]
        public Guid DepartmentId { get; set; }

        [Required]
        public Guid PositionId { get; set; }

        public bool IsPrimary { get; set; } = false;

        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateWorkforceOrganizationAssignmentRequest
    {
        [Required]
        public Guid DepartmentId { get; set; }

        [Required]
        public Guid PositionId { get; set; }

        public bool IsPrimary { get; set; } = false;

        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateWorkforceOrganizationAssignmentStatusRequest
    {
        public bool IsActive { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class SetWorkforceOrganizationAssignmentPrimaryRequest
    {
        public bool IsPrimary { get; set; } = true;
    }
}
