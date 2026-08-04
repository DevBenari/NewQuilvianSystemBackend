using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.DTOs
{
    public class WfpHealthRecordSummaryResponse
    {
        public int TotalHealthRecord { get; set; }
        public int ActiveHealthRecord { get; set; }
        public int InactiveHealthRecord { get; set; }
        public int VerifiedHealthRecord { get; set; }
        public int UnverifiedHealthRecord { get; set; }
        public int FitToWorkRecord { get; set; }
        public int NotFitToWorkRecord { get; set; }
        public int WorkRestrictionRecord { get; set; }
        public int SensitiveRecord { get; set; }
        public int ExpiredRecord { get; set; }
        public int ExpiringSoonRecord { get; set; }
    }

    public class WfpHealthRecordResponse
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceProfileCode { get; set; } = string.Empty;
        public string WorkforceDisplayName { get; set; } = string.Empty;

        public Guid? EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }

        public Guid? DoctorId { get; set; }
        public string? DoctorCode { get; set; }
        public string? DoctorName { get; set; }

        public string RecordCode { get; set; } = string.Empty;
        public string RecordType { get; set; } = string.Empty;
        public DateTime RecordDate { get; set; }
        public string? ProviderName { get; set; }
        public string? AdministrativeResultStatus { get; set; }
        public string? AdministrativeSummary { get; set; }
        public string AccessClassification { get; set; } = string.Empty;
        public bool IsSensitive { get; set; }
        public bool? IsFitToWork { get; set; }
        public bool WorkRestrictionRequired { get; set; }
        public DateTime? ReminderDate { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public bool IsExpired { get; set; }
        public int? DaysUntilExpiry { get; set; }
        public string? FilePath { get; set; }
        public string? FileContentType { get; set; }
        public bool HasFile { get; set; }
        public bool IsVerified { get; set; }
        public Guid? VerifiedByUserId { get; set; }
        public string? VerifiedByUserName { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public bool IsActive { get; set; }
        public string? Description { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class WfpHealthRecordDetailResponse : WfpHealthRecordResponse
    {
        public string? ClinicalSummaryRestricted { get; set; }
        public int MedicalExaminationCount { get; set; }
        public int FitnessAssessmentCount { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class WfpHealthRecordFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string ResetButtonLabel { get; set; } = "Reset";
        public WfpHealthRecordDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<WfpHealthRecordStringOptionResponse> CustomPeriods { get; set; } = new();
        public List<WfpHealthRecordStringOptionResponse> RecordTypeOptions { get; set; } = new();
        public List<WfpHealthRecordStringOptionResponse> AccessClassificationOptions { get; set; } = new();
        public List<WfpHealthRecordSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class WfpHealthRecordDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public string? RecordType { get; set; }
        public string? AdministrativeResultStatus { get; set; }
        public string? AccessClassification { get; set; }
        public bool? IsSensitive { get; set; }
        public bool? IsFitToWork { get; set; }
        public bool? WorkRestrictionRequired { get; set; }
        public bool? IsVerified { get; set; }
        public bool? IsExpired { get; set; }
        public bool? IsActive { get; set; }
        public int? ExpiringWithinDays { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "recordDate";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class WfpHealthRecordStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class WfpHealthRecordSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateWfpHealthRecordRequest
    {
        public Guid? EmployeeId { get; set; }
        public Guid? DoctorId { get; set; }

        [Required]
        [MaxLength(50)]
        public string RecordType { get; set; } = "General";

        [Required]
        public DateTime RecordDate { get; set; }

        [MaxLength(200)]
        public string? ProviderName { get; set; }

        [MaxLength(40)]
        public string? AdministrativeResultStatus { get; set; }

        [MaxLength(1500)]
        public string? AdministrativeSummary { get; set; }

        [MaxLength(4000)]
        public string? ClinicalSummaryRestricted { get; set; }

        [Required]
        [MaxLength(30)]
        public string AccessClassification { get; set; } = "Restricted";

        public bool IsSensitive { get; set; } = true;
        public bool? IsFitToWork { get; set; }
        public bool WorkRestrictionRequired { get; set; }
        public DateTime? ReminderDate { get; set; }
        public DateTime? ExpiredDate { get; set; }

        [MaxLength(500)]
        public string? FilePath { get; set; }

        [MaxLength(100)]
        public string? FileContentType { get; set; }

        public bool IsVerified { get; set; }
        public bool IsActive { get; set; } = true;

        [MaxLength(1000)]
        public string? Description { get; set; }
    }

    public class UpdateWfpHealthRecordRequest : CreateWfpHealthRecordRequest
    {
    }

    public class UpdateWfpHealthRecordStatusRequest
    {
        public bool IsActive { get; set; }
        public DateTime? ExpiredDate { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }
    }

    public class VerifyWfpHealthRecordRequest
    {
        public bool IsVerified { get; set; } = true;
    }
}
