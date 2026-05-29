using QuilvianSystemBackend.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs
{
    public class WorkforceRequirementResponse
    {
        public Guid Id { get; set; }

        public UserType UserType { get; set; }

        public string RequirementCategory { get; set; } = string.Empty;

        public string RequirementCode { get; set; } = string.Empty;

        public string RequirementName { get; set; } = string.Empty;

        public bool IsRequired { get; set; }

        public bool IsMultipleAllowed { get; set; }

        public bool IsFileRequired { get; set; }

        public bool IsNumberRequired { get; set; }

        public bool IsIssueDateRequired { get; set; }

        public bool IsExpiredDateRequired { get; set; }

        public bool IsVerificationRequired { get; set; }

        public int SortOrder { get; set; }

        public bool IsActive { get; set; }

        public string? Description { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforceRequirementListResponse
    {
        public int TotalData { get; set; }

        public int ActiveData { get; set; }

        public int RequiredData { get; set; }

        public List<WorkforceRequirementResponse> Items { get; set; } = new();
    }

    public class CreateWorkforceRequirementRequest
    {
        [Required]
        public UserType UserType { get; set; }

        [Required]
        [MaxLength(50)]
        public string RequirementCategory { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string RequirementCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string RequirementName { get; set; } = string.Empty;

        public bool IsRequired { get; set; } = true;

        public bool IsMultipleAllowed { get; set; } = false;

        public bool IsFileRequired { get; set; } = true;

        public bool IsNumberRequired { get; set; } = false;

        public bool IsIssueDateRequired { get; set; } = false;

        public bool IsExpiredDateRequired { get; set; } = false;

        public bool IsVerificationRequired { get; set; } = true;

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateWorkforceRequirementRequest
    {
        [Required]
        public UserType UserType { get; set; }

        [Required]
        [MaxLength(50)]
        public string RequirementCategory { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string RequirementCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string RequirementName { get; set; } = string.Empty;

        public bool IsRequired { get; set; } = true;

        public bool IsMultipleAllowed { get; set; } = false;

        public bool IsFileRequired { get; set; } = true;

        public bool IsNumberRequired { get; set; } = false;

        public bool IsIssueDateRequired { get; set; } = false;

        public bool IsExpiredDateRequired { get; set; } = false;

        public bool IsVerificationRequired { get; set; } = true;

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateWorkforceRequirementStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class WorkforceRequirementChecklistItemResponse
    {
        public Guid RequirementId { get; set; }

        public UserType UserType { get; set; }

        public string RequirementCategory { get; set; } = string.Empty;

        public string RequirementCode { get; set; } = string.Empty;

        public string RequirementName { get; set; } = string.Empty;

        public bool IsRequired { get; set; }

        public bool IsFileRequired { get; set; }

        public bool IsNumberRequired { get; set; }

        public bool IsIssueDateRequired { get; set; }

        public bool IsExpiredDateRequired { get; set; }

        public bool IsVerificationRequired { get; set; }

        public bool IsSubmitted { get; set; }

        public bool IsVerified { get; set; }

        public bool IsExpired { get; set; }

        public Guid? SourceId { get; set; }

        public string? SourceType { get; set; }

        public string? FilePath { get; set; }

        public DateTime? ExpiredDate { get; set; }

        public string Status { get; set; } = "Missing";

        public int SortOrder { get; set; }

        public string? Description { get; set; }
    }

    public class WorkforceRequirementChecklistGroupResponse
    {
        public string RequirementCategory { get; set; } = string.Empty;

        public int TotalData { get; set; }

        public int CompletedData { get; set; }

        public int MissingData { get; set; }

        public int NeedVerificationData { get; set; }

        public int ExpiredData { get; set; }

        public List<WorkforceRequirementChecklistItemResponse> Items { get; set; } = new();
    }

    public class WorkforceProfileRequirementChecklistResponse
    {
        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public UserType UserType { get; set; }

        public int TotalRequirement { get; set; }

        public int CompletedRequirement { get; set; }

        public int MissingRequirement { get; set; }

        public int NeedVerificationRequirement { get; set; }

        public int ExpiredRequirement { get; set; }

        public List<WorkforceRequirementChecklistGroupResponse> Groups { get; set; } = new();
    }
}
