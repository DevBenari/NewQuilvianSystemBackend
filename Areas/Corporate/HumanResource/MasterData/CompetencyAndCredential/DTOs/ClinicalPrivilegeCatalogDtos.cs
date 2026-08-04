using QuilvianSystemBackend.Enums.HumanResource;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.DTOs
{
    public class ClinicalPrivilegeCatalogSummaryResponse
    {
        public int TotalPrivilegeCatalog { get; set; }
        public int ActivePrivilegeCatalog { get; set; }
        public int InactivePrivilegeCatalog { get; set; }
        public int HighRiskPrivilegeCatalog { get; set; }
        public int SupervisionRequiredPrivilegeCatalog { get; set; }
        public int IndependentPracticePrivilegeCatalog { get; set; }
        public int PrivilegeCatalogWithCompetency { get; set; }
    }

    public class ClinicalPrivilegeCatalogResponse
    {
        public Guid Id { get; set; }
        public Guid? ProfessionId { get; set; }
        public string? ProfessionCode { get; set; }
        public string? ProfessionName { get; set; }
        public Guid? SpecializationId { get; set; }
        public string? SpecializationCode { get; set; }
        public string? SpecializationName { get; set; }
        public Guid? RequiredCompetencyId { get; set; }
        public string? RequiredCompetencyCode { get; set; }
        public string? RequiredCompetencyName { get; set; }
        public string PrivilegeCode { get; set; } = string.Empty;
        public string PrivilegeName { get; set; } = string.Empty;
        public string PrivilegeCategory { get; set; } = string.Empty;
        public string? ReferenceProcedureCode { get; set; }
        public CompetencyLevel? MinimumCompetencyLevel { get; set; }
        public int MinimumExperienceMonths { get; set; }
        public bool RequiresSupervision { get; set; }
        public bool AllowsIndependentPractice { get; set; }
        public bool IsHighRisk { get; set; }
        public int? DefaultValidityMonths { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int WorkforceClinicalPrivilegeCount { get; set; }
        public int CredentialingRequirementCount { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class ClinicalPrivilegeCatalogDetailResponse : ClinicalPrivilegeCatalogResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class ClinicalPrivilegeCatalogOptionResponse
    {
        public Guid Id { get; set; }
        public Guid? ProfessionId { get; set; }
        public Guid? SpecializationId { get; set; }
        public string PrivilegeCode { get; set; } = string.Empty;
        public string PrivilegeName { get; set; } = string.Empty;
        public string PrivilegeCategory { get; set; } = string.Empty;
        public bool RequiresSupervision { get; set; }
        public bool AllowsIndependentPractice { get; set; }
        public bool IsHighRisk { get; set; }
        public int? DefaultValidityMonths { get; set; }
    }

    public class ClinicalPrivilegeCatalogOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<ClinicalPrivilegeCatalogOptionResponse> Items { get; set; } = new();
    }

    public class ClinicalPrivilegeCatalogFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public ClinicalPrivilegeCatalogDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<ClinicalPrivilegeCatalogCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<ClinicalPrivilegeCatalogStringOptionResponse> PrivilegeCategoryOptions { get; set; } = new();
        public List<ClinicalPrivilegeCatalogEnumOptionResponse> CompetencyLevelOptions { get; set; } = new();
        public List<ClinicalPrivilegeCatalogSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class ClinicalPrivilegeCatalogDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? ProfessionId { get; set; }
        public Guid? SpecializationId { get; set; }
        public Guid? RequiredCompetencyId { get; set; }
        public string? PrivilegeCategory { get; set; }
        public bool? RequiresSupervision { get; set; }
        public bool? AllowsIndependentPractice { get; set; }
        public bool? IsHighRisk { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "privilegeName";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class ClinicalPrivilegeCatalogCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class ClinicalPrivilegeCatalogStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class ClinicalPrivilegeCatalogEnumOptionResponse
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class ClinicalPrivilegeCatalogSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateClinicalPrivilegeCatalogRequest
    {
        public Guid? ProfessionId { get; set; }
        public Guid? SpecializationId { get; set; }
        public Guid? RequiredCompetencyId { get; set; }

        [Required, MaxLength(250)]
        public string PrivilegeName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string PrivilegeCategory { get; set; } = "ClinicalProcedure";

        [MaxLength(100)]
        public string? ReferenceProcedureCode { get; set; }

        public CompetencyLevel? MinimumCompetencyLevel { get; set; }

        [Range(0, int.MaxValue)]
        public int MinimumExperienceMonths { get; set; }

        public bool RequiresSupervision { get; set; }
        public bool AllowsIndependentPractice { get; set; } = true;
        public bool IsHighRisk { get; set; }

        [Range(1, int.MaxValue)]
        public int? DefaultValidityMonths { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }
    }

    public class UpdateClinicalPrivilegeCatalogRequest : CreateClinicalPrivilegeCatalogRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateClinicalPrivilegeCatalogStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class ClinicalPrivilegeCatalogCreateResponse
    {
        public Guid Id { get; set; }
        public string PrivilegeCode { get; set; } = string.Empty;
        public string PrivilegeName { get; set; } = string.Empty;
        public string PrivilegeCategory { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
