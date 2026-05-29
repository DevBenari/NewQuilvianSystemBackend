using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
using QuilvianSystemBackend.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs
{
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

        public bool HasFile { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforceEmploymentHistoryListResponse
    {
        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int TotalData { get; set; }

        public int ActiveData { get; set; }

        public int JoinData { get; set; }

        public int MutationData { get; set; }

        public int PromotionData { get; set; }

        public int StatusChangeData { get; set; }

        public int ResignOrTerminationData { get; set; }

        public List<WorkforceEmploymentHistoryResponse> Items { get; set; } = new();
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

        [MaxLength(500)]
        public string? FilePath { get; set; }

        [MaxLength(100)]
        public string? FileContentType { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceEmploymentHistoryRequest
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

        [MaxLength(500)]
        public string? FilePath { get; set; }

        [MaxLength(100)]
        public string? FileContentType { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceEmploymentHistoryStatusRequest
    {
        public bool IsActive { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }
}
