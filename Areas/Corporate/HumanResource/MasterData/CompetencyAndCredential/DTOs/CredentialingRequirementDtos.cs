using QuilvianSystemBackend.Enums.HumanResource;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.DTOs
{
    public class CredentialingRequirementSummaryResponse
    {
        public int TotalRequirement { get; set; }
        public int ActiveRequirement { get; set; }
        public int InactiveRequirement { get; set; }
        public int MandatoryRequirement { get; set; }
        public int DocumentRequiredRequirement { get; set; }
        public int VerificationRequiredRequirement { get; set; }
        public int ExpiryRequiredRequirement { get; set; }
        public int CurrentlyEffectiveRequirement { get; set; }
    }

    public class CredentialingRequirementResponse
    {
        public Guid Id { get; set; }
        public Guid? ProfessionId { get; set; }
        public string? ProfessionCode { get; set; }
        public string? ProfessionName { get; set; }
        public Guid? SpecializationId { get; set; }
        public string? SpecializationCode { get; set; }
        public string? SpecializationName { get; set; }
        public Guid? PositionId { get; set; }
        public string? PositionCode { get; set; }
        public string? PositionName { get; set; }
        public Guid? CompetencyId { get; set; }
        public string? CompetencyCode { get; set; }
        public string? CompetencyName { get; set; }
        public Guid? TrainingCatalogId { get; set; }
        public Guid? CertificationTypeId { get; set; }
        public string? CertificationTypeCode { get; set; }
        public string? CertificationTypeName { get; set; }
        public Guid? LicenseTypeId { get; set; }
        public string? LicenseTypeCode { get; set; }
        public string? LicenseTypeName { get; set; }
        public Guid? ClinicalPrivilegeCatalogId { get; set; }
        public string? ClinicalPrivilegeCode { get; set; }
        public string? ClinicalPrivilegeName { get; set; }
        public string RequirementCode { get; set; } = string.Empty;
        public string RequirementName { get; set; } = string.Empty;
        public string RequirementType { get; set; } = string.Empty;
        public CompetencyLevel? MinimumCompetencyLevel { get; set; }
        public int MinimumExperienceMonths { get; set; }
        public int RequiredQuantity { get; set; }
        public int? ValidityMonths { get; set; }
        public bool IsMandatory { get; set; }
        public bool RequiresDocument { get; set; }
        public bool RequiresVerification { get; set; }
        public bool RequiresExpiryDate { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public bool IsCurrentlyEffective { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int WorkforceCertificationCount { get; set; }
        public int WorkforceLicenseCount { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class CredentialingRequirementDetailResponse : CredentialingRequirementResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class CredentialingRequirementOptionResponse
    {
        public Guid Id { get; set; }
        public string RequirementCode { get; set; } = string.Empty;
        public string RequirementName { get; set; } = string.Empty;
        public string RequirementType { get; set; } = string.Empty;
        public Guid? ProfessionId { get; set; }
        public Guid? SpecializationId { get; set; }
        public Guid? PositionId { get; set; }
        public bool IsMandatory { get; set; }
        public bool RequiresDocument { get; set; }
        public bool RequiresVerification { get; set; }
        public bool RequiresExpiryDate { get; set; }
        public bool IsCurrentlyEffective { get; set; }
    }

    public class CredentialingRequirementOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<CredentialingRequirementOptionResponse> Items { get; set; } = new();
    }

    public class CredentialingRequirementFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public CredentialingRequirementDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<CredentialingRequirementCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<CredentialingRequirementStringOptionResponse> RequirementTypeOptions { get; set; } = new();
        public List<CredentialingRequirementEnumOptionResponse> CompetencyLevelOptions { get; set; } = new();
        public List<CredentialingRequirementSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class CredentialingRequirementDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? ProfessionId { get; set; }
        public Guid? SpecializationId { get; set; }
        public Guid? PositionId { get; set; }
        public string? RequirementType { get; set; }
        public bool? IsMandatory { get; set; }
        public bool? RequiresVerification { get; set; }
        public bool? IsCurrentlyEffective { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "requirementName";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class CredentialingRequirementCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CredentialingRequirementStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CredentialingRequirementEnumOptionResponse
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CredentialingRequirementSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateCredentialingRequirementRequest
    {
        public Guid? ProfessionId { get; set; }
        public Guid? SpecializationId { get; set; }
        public Guid? PositionId { get; set; }
        public Guid? CompetencyId { get; set; }
        public Guid? TrainingCatalogId { get; set; }
        public Guid? CertificationTypeId { get; set; }
        public Guid? LicenseTypeId { get; set; }
        public Guid? ClinicalPrivilegeCatalogId { get; set; }

        [Required, MaxLength(200)]
        public string RequirementName { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string RequirementType { get; set; } = "Document";

        public CompetencyLevel? MinimumCompetencyLevel { get; set; }

        [Range(0, int.MaxValue)]
        public int MinimumExperienceMonths { get; set; }

        [Range(1, int.MaxValue)]
        public int RequiredQuantity { get; set; } = 1;

        [Range(1, int.MaxValue)]
        public int? ValidityMonths { get; set; }

        public bool IsMandatory { get; set; } = true;
        public bool RequiresDocument { get; set; } = true;
        public bool RequiresVerification { get; set; } = true;
        public bool RequiresExpiryDate { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }
    }

    public class UpdateCredentialingRequirementRequest : CreateCredentialingRequirementRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateCredentialingRequirementStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class CredentialingRequirementCreateResponse
    {
        public Guid Id { get; set; }
        public string RequirementCode { get; set; } = string.Empty;
        public string RequirementName { get; set; } = string.Empty;
        public string RequirementType { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
