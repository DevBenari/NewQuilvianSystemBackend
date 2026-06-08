using Microsoft.AspNetCore.Http;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
using QuilvianSystemBackend.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs
{
    public class WorkforceEmploymentHistorySummaryResponse
    {
        public Guid WorkforceProfileId { get; set; }
        public string ProfileCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public UserType UserType { get; set; }
        public int TotalHistory { get; set; }
        public int ActiveHistory { get; set; }
        public int InactiveHistory { get; set; }
        public int JoinHistory { get; set; }
        public int MutationHistory { get; set; }
        public int PromotionHistory { get; set; }
        public int StatusChangeHistory { get; set; }
        public int ResignOrTerminationHistory { get; set; }
        public int WithFileHistory { get; set; }
        public int ApprovedHistory { get; set; }
    }

    public class WorkforceEmploymentHistoryResponse
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string ProfileCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public UserType UserType { get; set; }
        public EmploymentHistoryType HistoryType { get; set; }
        public Guid? OldDepartmentId { get; set; }
        public string? OldDepartmentCode { get; set; }
        public string? OldDepartmentName { get; set; }
        public Guid? NewDepartmentId { get; set; }
        public string? NewDepartmentCode { get; set; }
        public string? NewDepartmentName { get; set; }
        public Guid? OldPositionId { get; set; }
        public string? OldPositionCode { get; set; }
        public string? OldPositionName { get; set; }
        public Guid? NewPositionId { get; set; }
        public string? NewPositionCode { get; set; }
        public string? NewPositionName { get; set; }
        public string? OldStatus { get; set; }
        public string? NewStatus { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Description { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public string? ApprovedByUserName { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? FilePath { get; set; }
        public string? FileContentType { get; set; }
        public string? FileName { get; set; }
        public string? FilePreviewUrl { get; set; }
        public string? FileDownloadUrl { get; set; }
        public bool HasFile { get; set; }
        public bool IsApproved { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class WorkforceEmploymentHistoryDetailResponse : WorkforceEmploymentHistoryResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class WorkforceEmploymentHistoryOptionResponse
    {
        public Guid Id { get; set; }
        public EmploymentHistoryType HistoryType { get; set; }
        public string HistoryTypeName { get; set; } = string.Empty;
        public string? NewDepartmentName { get; set; }
        public string? NewPositionName { get; set; }
        public string? OldStatus { get; set; }
        public string? NewStatus { get; set; }
        public DateTime EffectiveDate { get; set; }
        public bool HasFile { get; set; }
        public bool IsApproved { get; set; }
    }

    public class WorkforceEmploymentHistoryOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<WorkforceEmploymentHistoryOptionResponse> Items { get; set; } = new();
    }

    public class WorkforceEmploymentHistoryFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string TimeZoneInfo { get; set; } = "Date filter memakai CreateDateTime dalam UTC.";
        public string ResetButtonLabel { get; set; } = "Reset";
        public string ModuleInfo { get; set; } = "EmploymentHistory hanya menangani riwayat mutasi/status/jabatan. ContractHistory tidak digunakan di endpoint ini.";
        public string RelationFilterInfo { get; set; } = "Filter relasi dibatasi 2 untuk menjaga UI tetap ringkas: newDepartmentId dan newPositionId.";
        public WorkforceEmploymentHistoryDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<WorkforceEmploymentHistoryCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<WorkforceEmploymentHistorySortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<WorkforceEmploymentHistoryTypeOptionResponse> HistoryTypeOptions { get; set; } = new();
        public List<WorkforceEmploymentStatusExampleResponse> StatusExamples { get; set; } = new();
        public List<string> PreviewSupportedContentTypes { get; set; } = new()
        {
            "application/pdf",
            "image/jpeg",
            "image/png"
        };
        public WorkforceEmploymentHistoryFileUploadInfoResponse FileUploadInfo { get; set; } = new();
        public List<string> FrontendGuide { get; set; } = new();
    }

    public class WorkforceEmploymentHistoryDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public EmploymentHistoryType? HistoryType { get; set; }
        public Guid? NewDepartmentId { get; set; }
        public Guid? NewPositionId { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "effectiveDate";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class WorkforceEmploymentHistoryCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class WorkforceEmploymentHistorySortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class WorkforceEmploymentHistoryTypeOptionResponse
    {
        public EmploymentHistoryType Value { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class WorkforceEmploymentStatusExampleResponse
    {
        public string Category { get; set; } = string.Empty;
        public List<string> Values { get; set; } = new();
    }

    public class WorkforceEmploymentHistoryFileUploadInfoResponse
    {
        public int MaxFileSizeMb { get; set; } = 10;
        public List<string> AllowedExtensions { get; set; } = new()
        {
            ".pdf",
            ".jpg",
            ".jpeg",
            ".png",
            ".doc",
            ".docx",
            ".xls",
            ".xlsx"
        };
        public string PreviewInfo { get; set; } = "PDF dan image bisa dipreview langsung. DOC/DOCX/XLS/XLSX biasanya perlu viewer khusus atau fallback download.";
    }

    public class CreateWorkforceEmploymentHistoryRequest
    {
        [Required]
        public EmploymentHistoryType HistoryType { get; set; } = EmploymentHistoryType.Unknown;

        public Guid? OldDepartmentId { get; set; }
        public Guid? NewDepartmentId { get; set; }
        public Guid? OldPositionId { get; set; }
        public Guid? NewPositionId { get; set; }

        [MaxLength(100)]
        public string? OldStatus { get; set; }

        [MaxLength(100)]
        public string? NewStatus { get; set; }

        [Required]
        public DateTime EffectiveDate { get; set; }

        public DateTime? EndDate { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public Guid? ApprovedByUserId { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public IFormFile? File { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceEmploymentHistoryRequest : CreateWorkforceEmploymentHistoryRequest
    {
        public bool ReplaceExistingFile { get; set; } = false;
    }

    public class UpdateWorkforceEmploymentHistoryStatusRequest
    {
        public bool IsActive { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class DeleteWorkforceEmploymentHistoryFileRequest
    {
        public bool DeletePhysicalFile { get; set; } = true;
    }
}
