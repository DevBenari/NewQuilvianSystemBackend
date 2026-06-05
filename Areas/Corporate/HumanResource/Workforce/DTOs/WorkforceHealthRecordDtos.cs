using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs
{
    public class WorkforceHealthRecordSummaryResponse
    {
        public Guid WorkforceProfileId { get; set; }
        public string ProfileCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int TotalHealthRecord { get; set; }
        public int ActiveHealthRecord { get; set; }
        public int InactiveHealthRecord { get; set; }
        public int VerifiedHealthRecord { get; set; }
        public int UnverifiedHealthRecord { get; set; }
        public int ExpiredHealthRecord { get; set; }
        public int CurrentlyValidHealthRecord { get; set; }
        public int FitToWorkHealthRecord { get; set; }
        public int NotFitToWorkHealthRecord { get; set; }
        public int CompliantForWorkHealthRecord { get; set; }
        public int HealthRecordWithFile { get; set; }
        public int HealthRecordWithoutFile { get; set; }
    }

    public class WorkforceHealthRecordResponse
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string ProfileCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? RequirementCode { get; set; }
        public HealthRecordType HealthRecordType { get; set; }
        public DateTime RecordDate { get; set; }
        public HealthRecordResultStatus ResultStatus { get; set; }
        public string? ProviderName { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public bool? IsFitToWork { get; set; }
        public string? FitToWorkRestrictionNote { get; set; }
        public bool IsVerified { get; set; }
        public Guid? VerifiedByUserId { get; set; }
        public string? VerifiedByUserName { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public string? VerificationNote { get; set; }
        public string? FilePath { get; set; }
        public string? FileContentType { get; set; }
        public string? FileName { get; set; }
        public string? FilePreviewUrl { get; set; }
        public string? FileDownloadUrl { get; set; }
        public bool HasFile { get; set; }
        public string? Notes { get; set; }
        public bool IsExpired { get; set; }
        public bool IsCurrentlyValid { get; set; }
        public bool IsCompliantForWork { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforceHealthRecordDetailResponse : WorkforceHealthRecordResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid CreateBy { get; set; }
        public Guid UpdateBy { get; set; }
    }

    public class WorkforceHealthRecordOptionResponse
    {
        public Guid Id { get; set; }
        public string? RequirementCode { get; set; }
        public HealthRecordType HealthRecordType { get; set; }
        public DateTime RecordDate { get; set; }
        public HealthRecordResultStatus ResultStatus { get; set; }
        public string? ProviderName { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public bool? IsFitToWork { get; set; }
        public bool IsVerified { get; set; }
        public bool IsExpired { get; set; }
        public bool IsCurrentlyValid { get; set; }
        public bool IsCompliantForWork { get; set; }
        public bool HasFile { get; set; }
    }

    public class WorkforceHealthRecordOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<WorkforceHealthRecordOptionResponse> Items { get; set; } = new();
    }

    public class WorkforceHealthRecordFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string TimeZoneInfo { get; set; } = "Date filter memakai RecordDate dalam UTC.";
        public string ResetButtonLabel { get; set; } = "Reset";
        public WorkforceHealthRecordDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<WorkforceHealthRecordCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<WorkforceHealthRecordSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<WorkforceHealthRecordEnumOptionResponse> HealthRecordTypes { get; set; } = new();
        public List<WorkforceHealthRecordEnumOptionResponse> ResultStatuses { get; set; } = new();
        public List<WorkforceHealthRecordBooleanOptionResponse> FitToWorkOptions { get; set; } = new();
        public WorkforceHealthRecordCodeInfoResponse CodeInfo { get; set; } = new();
        public WorkforceHealthRecordFileUploadInfoResponse FileUploadInfo { get; set; } = new();
        public List<string> PreviewSupportedContentTypes { get; set; } = new();
        public List<string> FrontendGuide { get; set; } = new();
    }

    public class WorkforceHealthRecordDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public HealthRecordType? HealthRecordType { get; set; }
        public HealthRecordResultStatus? ResultStatus { get; set; }
        public bool? IsFitToWork { get; set; }
        public bool? IsVerified { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsExpired { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "recordDate";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class WorkforceHealthRecordCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class WorkforceHealthRecordSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class WorkforceHealthRecordEnumOptionResponse
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class WorkforceHealthRecordBooleanOptionResponse
    {
        public bool? Value { get; set; }
        public string Label { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class WorkforceHealthRecordCodeInfoResponse
    {
        public string FieldName { get; set; } = "RequirementCode";
        public string Format { get; set; } = "HLR-RSMMC-00001";
        public string Description { get; set; } = "RequirementCode dibuat otomatis oleh backend dan tidak perlu dikirim dari frontend.";
    }

    public class WorkforceHealthRecordFileUploadInfoResponse
    {
        public int MaxFileSizeMb { get; set; } = 10;
        public List<string> AllowedExtensions { get; set; } = new();
        public string FormFieldName { get; set; } = "File";
        public string ContentType { get; set; } = "multipart/form-data";
    }

    public class CreateWorkforceHealthRecordRequest
    {
        [Required]
        public HealthRecordType HealthRecordType { get; set; } = HealthRecordType.Unknown;

        [Required]
        public DateTime RecordDate { get; set; }

        public HealthRecordResultStatus ResultStatus { get; set; } = HealthRecordResultStatus.Unknown;

        [MaxLength(200)]
        public string? ProviderName { get; set; }

        public DateTime? ExpiredDate { get; set; }

        public bool? IsFitToWork { get; set; }

        [MaxLength(250)]
        public string? FitToWorkRestrictionNote { get; set; }

        public IFormFile? File { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceHealthRecordRequest : CreateWorkforceHealthRecordRequest
    {
        public bool ReplaceExistingFile { get; set; } = false;
    }

    public class UpdateWorkforceHealthRecordStatusRequest
    {
        public bool IsActive { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class VerifyWorkforceHealthRecordRequest
    {
        [MaxLength(250)]
        public string? VerificationNote { get; set; }
    }

    public class UnverifyWorkforceHealthRecordRequest
    {
        [Required]
        [MaxLength(250)]
        public string UnverifyReason { get; set; } = string.Empty;
    }

    public class DeleteWorkforceHealthRecordFileRequest
    {
        public bool DeletePhysicalFile { get; set; } = true;
    }
}
