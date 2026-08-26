using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.DTOs
{
    public class LegalEntitySummaryResponse
    {
        public int TotalData { get; set; }
        public int ActiveData { get; set; }
        public int InactiveData { get; set; }
        public int DefaultData { get; set; }
    }

    public class LegalEntityResponse
    {
        public Guid Id { get; set; }
        public string LegalEntityCode { get; set; } = string.Empty;
        public string LegalEntityName { get; set; } = string.Empty;
        public string? ShortName { get; set; }
        public string? TaxIdentificationNumber { get; set; }
        public string? BusinessRegistrationNumber { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public int HospitalSiteCount { get; set; }
        public int OrganizationUnitCount { get; set; }
        public int CostCenterCount { get; set; }
        public int WorkLocationCount { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class LegalEntityDetailResponse : LegalEntityResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class LegalEntityOptionResponse
    {
        public Guid Id { get; set; }
        public string LegalEntityCode { get; set; } = string.Empty;
        public string LegalEntityName { get; set; } = string.Empty;
        public string? ShortName { get; set; }
        public bool IsDefault { get; set; }
    }

    public class LegalEntityOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<LegalEntityOptionResponse> Items { get; set; } = new();
    }

    public class LegalEntityFilterMetadataResponse
    {
        public LegalEntityDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<LegalEntityCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<LegalEntitySortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class LegalEntityDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public bool? IsDefault { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "legalEntityName";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class LegalEntityCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class LegalEntitySortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateLegalEntityRequest
    {
        [Required, MaxLength(200)]
        public string LegalEntityName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ShortName { get; set; }

        [MaxLength(100)]
        public string? TaxIdentificationNumber { get; set; }

        [MaxLength(100)]
        public string? BusinessRegistrationNumber { get; set; }

        [EmailAddress, MaxLength(200)]
        public string? Email { get; set; }

        [MaxLength(30)]
        public string? PhoneNumber { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public bool IsDefault { get; set; }
    }

    public class UpdateLegalEntityRequest : CreateLegalEntityRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateLegalEntityStatusRequest
    {
        public bool IsActive { get; set; }
        public bool? IsDefault { get; set; }
    }
}
