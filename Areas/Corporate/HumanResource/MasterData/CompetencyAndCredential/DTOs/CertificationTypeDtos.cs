using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.DTOs
{
    public class CertificationTypeSummaryResponse
    {
        public int TotalCertificationType { get; set; }
        public int ActiveCertificationType { get; set; }
        public int InactiveCertificationType { get; set; }
        public int ExpiryRequiredType { get; set; }
        public int RenewableType { get; set; }
        public int DocumentRequiredType { get; set; }
        public int VerificationRequiredType { get; set; }
    }

    public class CertificationTypeResponse
    {
        public Guid Id { get; set; }
        public Guid? ProfessionId { get; set; }
        public string? ProfessionCode { get; set; }
        public string? ProfessionName { get; set; }
        public string CertificationTypeCode { get; set; } = string.Empty;
        public string CertificationTypeName { get; set; } = string.Empty;
        public string? IssuingAuthority { get; set; }
        public int? DefaultValidityMonths { get; set; }
        public bool RequiresExpiryDate { get; set; }
        public bool IsRenewable { get; set; }
        public bool RequiresDocument { get; set; }
        public bool RequiresVerification { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int WorkforceCertificationCount { get; set; }
        public int CredentialingRequirementCount { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class CertificationTypeDetailResponse : CertificationTypeResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class CertificationTypeOptionResponse
    {
        public Guid Id { get; set; }
        public Guid? ProfessionId { get; set; }
        public string? ProfessionName { get; set; }
        public string CertificationTypeCode { get; set; } = string.Empty;
        public string CertificationTypeName { get; set; } = string.Empty;
        public string? IssuingAuthority { get; set; }
        public int? DefaultValidityMonths { get; set; }
        public bool RequiresExpiryDate { get; set; }
        public bool RequiresDocument { get; set; }
        public bool RequiresVerification { get; set; }
    }

    public class CertificationTypeOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<CertificationTypeOptionResponse> Items { get; set; } = new();
    }

    public class CertificationTypeFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public CertificationTypeDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<CertificationTypeCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<CertificationTypeSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class CertificationTypeDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? ProfessionId { get; set; }
        public bool? RequiresExpiryDate { get; set; }
        public bool? IsRenewable { get; set; }
        public bool? RequiresDocument { get; set; }
        public bool? RequiresVerification { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "certificationTypeName";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class CertificationTypeCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CertificationTypeSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateCertificationTypeRequest
    {
        public Guid? ProfessionId { get; set; }

        [Required, MaxLength(200)]
        public string CertificationTypeName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? IssuingAuthority { get; set; }

        [Range(1, int.MaxValue)]
        public int? DefaultValidityMonths { get; set; }

        public bool RequiresExpiryDate { get; set; } = true;
        public bool IsRenewable { get; set; } = true;
        public bool RequiresDocument { get; set; } = true;
        public bool RequiresVerification { get; set; } = true;

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class UpdateCertificationTypeRequest : CreateCertificationTypeRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateCertificationTypeStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class CertificationTypeCreateResponse
    {
        public Guid Id { get; set; }
        public string CertificationTypeCode { get; set; } = string.Empty;
        public string CertificationTypeName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
