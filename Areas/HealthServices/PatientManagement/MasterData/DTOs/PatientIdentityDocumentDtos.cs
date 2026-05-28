using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs
{
    public class PatientIdentityDocumentSummaryResponse
    {
        public int TotalDocument { get; set; }
        public int ActiveDocument { get; set; }
        public int InactiveDocument { get; set; }
        public int PrimaryDocument { get; set; }
        public int VerifiedDocument { get; set; }
        public int UnverifiedDocument { get; set; }
        public int FromKioskScanDocument { get; set; }
        public int ExpiredDocument { get; set; }
    }

    public class PatientIdentityDocumentResponse
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string PatientCode { get; set; } = string.Empty;
        public string MedicalRecordNumber { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string IdentityType { get; set; } = string.Empty;
        public string IdentityNumber { get; set; } = string.Empty;
        public string? DocumentName { get; set; }
        public string? FilePath { get; set; }
        public string? FileContentType { get; set; }
        public string? IssuedBy { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsVerified { get; set; }
        public Guid? VerifiedByUserId { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public bool IsFromKioskScan { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class PatientIdentityDocumentDetailResponse : PatientIdentityDocumentResponse
    {
        public string? VerificationNote { get; set; }
    }

    public class PatientIdentityDocumentOptionResponse
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string MedicalRecordNumber { get; set; } = string.Empty;
        public string IdentityType { get; set; } = string.Empty;
        public string IdentityNumber { get; set; } = string.Empty;
        public string? DocumentName { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsVerified { get; set; }
    }

    public class PatientIdentityDocumentFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public PatientIdentityDocumentDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<PatientIdentityDocumentSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<string> CommonIdentityTypes { get; set; } = new();
    }

    public class PatientIdentityDocumentDefaultFilterResponse
    {
        public string? Search { get; set; }
        public Guid? PatientId { get; set; }
        public string? IdentityType { get; set; }
        public bool? IsPrimary { get; set; }
        public bool? IsVerified { get; set; }
        public bool? IsFromKioskScan { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? ExpiredFrom { get; set; }
        public DateTime? ExpiredTo { get; set; }
        public string SortBy { get; set; } = "createDateTime";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class PatientIdentityDocumentSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreatePatientIdentityDocumentRequest
    {
        [Required]
        public Guid PatientId { get; set; }

        [Required]
        [MaxLength(50)]
        public string IdentityType { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string IdentityNumber { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? DocumentName { get; set; }

        [MaxLength(500)]
        public string? FilePath { get; set; }

        [MaxLength(100)]
        public string? FileContentType { get; set; }

        [MaxLength(100)]
        public string? IssuedBy { get; set; }

        public DateTime? IssueDate { get; set; }

        public DateTime? ExpiredDate { get; set; }

        public bool IsPrimary { get; set; } = false;

        public bool IsVerified { get; set; } = false;

        [MaxLength(250)]
        public string? VerificationNote { get; set; }

        public bool IsFromKioskScan { get; set; } = false;
    }

    public class UpdatePatientIdentityDocumentRequest : CreatePatientIdentityDocumentRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class VerifyPatientIdentityDocumentRequest
    {
        public bool IsVerified { get; set; } = true;

        [MaxLength(250)]
        public string? VerificationNote { get; set; }
    }

    public class PatientIdentityDocumentCreateResponse
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string IdentityType { get; set; } = string.Empty;
        public string IdentityNumber { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public bool IsVerified { get; set; }
        public bool IsActive { get; set; }
    }
}
