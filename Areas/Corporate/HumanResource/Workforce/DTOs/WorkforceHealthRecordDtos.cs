using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs
{
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

        public bool HasFile { get; set; }

        public string? Notes { get; set; }

        public bool IsExpired { get; set; }

        public bool IsCurrentlyValid { get; set; }

        public bool IsCompliantForWork { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforceHealthRecordListResponse
    {
        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int TotalData { get; set; }

        public int ActiveData { get; set; }

        public int VerifiedData { get; set; }

        public int ExpiredData { get; set; }

        public int CurrentlyValidData { get; set; }

        public int FitToWorkData { get; set; }

        public int NotFitToWorkData { get; set; }

        public int CompliantForWorkData { get; set; }

        public int WithFileData { get; set; }

        public List<WorkforceHealthRecordResponse> Items { get; set; } = new();
    }

    public class CreateWorkforceHealthRecordRequest
    {
        [MaxLength(100)]
        public string? RequirementCode { get; set; }

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

    public class UpdateWorkforceHealthRecordRequest
    {
        [MaxLength(100)]
        public string? RequirementCode { get; set; }

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

        public bool ReplaceExistingFile { get; set; } = false;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
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
}
