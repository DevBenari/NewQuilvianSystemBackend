using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs
{
    public class WfpEmploymentHistorySummaryResponse
    {
        public int TotalData { get; set; }
        public int ActiveData { get; set; }
        public int ApprovedData { get; set; }
        public int PendingApprovalData { get; set; }
        public int TransferData { get; set; }
        public int PromotionData { get; set; }
        public int SeparationData { get; set; }
    }

    public class WfpEmploymentHistoryResponse
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceProfileCode { get; set; } = string.Empty;
        public string WorkforceDisplayName { get; set; } = string.Empty;
        public string HistoryType { get; set; } = string.Empty;
        public string? OldStatus { get; set; }
        public string? NewStatus { get; set; }
        public Guid? OldEmploymentStatusId { get; set; }
        public string? OldEmploymentStatusName { get; set; }
        public Guid? NewEmploymentStatusId { get; set; }
        public string? NewEmploymentStatusName { get; set; }
        public Guid? OldEmploymentTypeId { get; set; }
        public string? OldEmploymentTypeName { get; set; }
        public Guid? NewEmploymentTypeId { get; set; }
        public string? NewEmploymentTypeName { get; set; }
        public Guid? OldDepartmentId { get; set; }
        public string? OldDepartmentName { get; set; }
        public Guid? NewDepartmentId { get; set; }
        public string? NewDepartmentName { get; set; }
        public Guid? OldPositionId { get; set; }
        public string? OldPositionName { get; set; }
        public Guid? NewPositionId { get; set; }
        public string? NewPositionName { get; set; }
        public Guid? OldOrganizationUnitId { get; set; }
        public Guid? NewOrganizationUnitId { get; set; }
        public Guid? OldEmployeeGradeId { get; set; }
        public Guid? NewEmployeeGradeId { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Reason { get; set; }
        public string? ReferenceType { get; set; }
        public Guid? ReferenceId { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public string? ApprovedByUserName { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public bool IsApproved { get; set; }
        public string? FilePath { get; set; }
        public string? FileContentType { get; set; }
        public string? FileName { get; set; }
        public string? FileDownloadUrl { get; set; }
        public bool HasFile { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class WfpEmploymentHistoryOptionResponse
    {
        public Guid Id { get; set; }
        public string HistoryType { get; set; } = string.Empty;
        public DateTime EffectiveDate { get; set; }
        public string? NewDepartmentName { get; set; }
        public string? NewPositionName { get; set; }
        public string? NewStatus { get; set; }
        public bool IsApproved { get; set; }
        public bool IsActive { get; set; }
    }

    public class WfpEmploymentHistoryFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string ResetButtonLabel { get; set; } = "Reset";
        public WfpEmploymentHistoryDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<WfpEmploymentHistoryStringOptionResponse> CustomPeriods { get; set; } = new();
        public List<WfpEmploymentHistoryStringOptionResponse> HistoryTypeOptions { get; set; } = new();
        public List<WfpEmploymentHistoryStringOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public WfpEmploymentHistoryFileUploadInfoResponse FileUploadInfo { get; set; } = new();
    }

    public class WfpEmploymentHistoryDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Period { get; set; }
        public string? HistoryType { get; set; }
        public Guid? NewDepartmentId { get; set; }
        public Guid? NewPositionId { get; set; }
        public bool? IsApproved { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "effectiveDate";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class WfpEmploymentHistoryStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class WfpEmploymentHistoryFileUploadInfoResponse
    {
        public int MaxFileSizeMb { get; set; } = 10;
        public List<string> AllowedExtensions { get; set; } = new()
        {
            ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx", ".xls", ".xlsx"
        };
    }

    public class CreateWfpEmploymentHistoryRequest
    {
        [Required, MaxLength(50)]
        public string HistoryType { get; set; } = "StatusChange";

        [MaxLength(100)]
        public string? OldStatus { get; set; }

        [MaxLength(100)]
        public string? NewStatus { get; set; }

        public Guid? OldEmploymentStatusId { get; set; }
        public Guid? NewEmploymentStatusId { get; set; }
        public Guid? OldEmploymentTypeId { get; set; }
        public Guid? NewEmploymentTypeId { get; set; }
        public Guid? OldDepartmentId { get; set; }
        public Guid? NewDepartmentId { get; set; }
        public Guid? OldPositionId { get; set; }
        public Guid? NewPositionId { get; set; }
        public Guid? OldOrganizationUnitId { get; set; }
        public Guid? NewOrganizationUnitId { get; set; }
        public Guid? OldEmployeeGradeId { get; set; }
        public Guid? NewEmployeeGradeId { get; set; }

        [Required]
        public DateTime EffectiveDate { get; set; }

        public DateTime? EndDate { get; set; }

        [MaxLength(250)]
        public string? Reason { get; set; }

        [MaxLength(100)]
        public string? ReferenceType { get; set; }

        public Guid? ReferenceId { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public IFormFile? File { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateWfpEmploymentHistoryRequest : CreateWfpEmploymentHistoryRequest
    {
        public bool ReplaceExistingFile { get; set; }
    }

    public class ApproveWfpEmploymentHistoryRequest
    {
        public bool IsApproved { get; set; } = true;
    }

    public class UpdateWfpEmploymentHistoryStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class DeleteWfpEmploymentHistoryFileRequest
    {
        public bool DeletePhysicalFile { get; set; } = true;
    }
}
