using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs
{
    public class ProcedureSummaryResponse
    {
        public int TotalProcedure { get; set; }
        public int ActiveProcedure { get; set; }
        public int InactiveProcedure { get; set; }
        public int DoctorActionProcedure { get; set; }
        public int NursingActionProcedure { get; set; }
        public int SurgeryProcedure { get; set; }
        public int LaboratoryProcedure { get; set; }
        public int RadiologyProcedure { get; set; }
        public int TherapyProcedure { get; set; }
        public int NeedDoctorProcedure { get; set; }
        public int NeedApprovalProcedure { get; set; }
        public int CoveredByInsuranceDefaultProcedure { get; set; }
        public int AvailableForOutpatientProcedure { get; set; }
        public int AvailableForInpatientProcedure { get; set; }
        public int AvailableForEmergencyProcedure { get; set; }
    }

    public class ProcedureResponse
    {
        public Guid Id { get; set; }
        public string ProcedureCode { get; set; } = string.Empty;
        public string ProcedureName { get; set; } = string.Empty;
        public string? ProcedureGroupName { get; set; }
        public string? ProcedureCategoryName { get; set; }
        public string ProcedureType { get; set; } = string.Empty;
        public bool IsDoctorAction { get; set; }
        public bool IsNursingAction { get; set; }
        public bool IsSurgery { get; set; }
        public bool IsLaboratory { get; set; }
        public bool IsRadiology { get; set; }
        public bool IsTherapy { get; set; }
        public bool IsNeedDoctor { get; set; }
        public bool IsNeedApproval { get; set; }
        public bool IsCoveredByInsuranceDefault { get; set; }
        public bool IsAvailableForOutpatient { get; set; }
        public bool IsAvailableForInpatient { get; set; }
        public bool IsAvailableForEmergency { get; set; }
        public int EstimatedDurationMinutes { get; set; }
        public string? ExternalProcedureCode { get; set; }
        public string? IntegrationCode { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class ProcedureDetailResponse : ProcedureResponse
    {
        public string? ClinicalNoteTemplate { get; set; }
        public string? Description { get; set; }
    }

    public class ProcedureOptionResponse
    {
        public Guid Id { get; set; }
        public string ProcedureCode { get; set; } = string.Empty;
        public string ProcedureName { get; set; } = string.Empty;
        public string? ProcedureGroupName { get; set; }
        public string? ProcedureCategoryName { get; set; }
        public string ProcedureType { get; set; } = string.Empty;
        public bool IsDoctorAction { get; set; }
        public bool IsNursingAction { get; set; }
        public bool IsSurgery { get; set; }
        public bool IsLaboratory { get; set; }
        public bool IsRadiology { get; set; }
        public bool IsTherapy { get; set; }
        public bool IsNeedDoctor { get; set; }
        public bool IsNeedApproval { get; set; }
        public bool IsCoveredByInsuranceDefault { get; set; }
        public bool IsAvailableForOutpatient { get; set; }
        public bool IsAvailableForInpatient { get; set; }
        public bool IsAvailableForEmergency { get; set; }
    }

    public class ProcedureOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<ProcedureOptionResponse> Items { get; set; } = new();
    }

    public class ProcedureFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string CustomPeriodPriorityInfo { get; set; } =
            "Jika customPeriod diisi selain custom, maka startDate dan endDate akan diabaikan.";

        public ProcedureDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<ProcedureCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<ProcedureSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<string> ProcedureTypeOptions { get; set; } = new();
        public List<ProcedureQueryParameterInfoResponse> QueryParameters { get; set; } = new();
        public List<ProcedureFormFieldMetadataResponse> CreateFields { get; set; } = new();
        public List<ProcedureFormFieldMetadataResponse> UpdateFields { get; set; } = new();
    }

    public class ProcedureDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "sortOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class ProcedureCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool UsesStartDate { get; set; }
        public bool UsesEndDate { get; set; }
    }

    public class ProcedureSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class ProcedureQueryParameterInfoResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Required { get; set; } = "No";
        public string Description { get; set; } = string.Empty;
        public string? Example { get; set; }
    }

    public class ProcedureFormFieldMetadataResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public string InputType { get; set; } = string.Empty;
        public bool Required { get; set; }
        public bool IsReadonly { get; set; }
        public string? Placeholder { get; set; }
        public string? Description { get; set; }
    }

    public class CreateProcedureRequest
    {
        [Required]
        [MaxLength(200)]
        public string ProcedureName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ProcedureGroupName { get; set; }

        [MaxLength(100)]
        public string? ProcedureCategoryName { get; set; }

        [Required]
        [MaxLength(50)]
        public string ProcedureType { get; set; } = "General";

        public bool IsDoctorAction { get; set; } = true;
        public bool IsNursingAction { get; set; } = false;
        public bool IsSurgery { get; set; } = false;
        public bool IsLaboratory { get; set; } = false;
        public bool IsRadiology { get; set; } = false;
        public bool IsTherapy { get; set; } = false;
        public bool IsNeedDoctor { get; set; } = true;
        public bool IsNeedApproval { get; set; } = false;
        public bool IsCoveredByInsuranceDefault { get; set; } = true;
        public bool IsAvailableForOutpatient { get; set; } = true;
        public bool IsAvailableForInpatient { get; set; } = true;
        public bool IsAvailableForEmergency { get; set; } = true;

        [Range(0, 1440)]
        public int EstimatedDurationMinutes { get; set; } = 0;

        [MaxLength(50)]
        public string? ExternalProcedureCode { get; set; }

        [MaxLength(50)]
        public string? IntegrationCode { get; set; }

        public int SortOrder { get; set; } = 0;

        [MaxLength(500)]
        public string? ClinicalNoteTemplate { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateProcedureRequest : CreateProcedureRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class ProcedureCreateResponse
    {
        public Guid Id { get; set; }
        public string ProcedureCode { get; set; } = string.Empty;
        public string ProcedureName { get; set; } = string.Empty;
        public string ProcedureType { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class ProcedureUpdateResponse : ProcedureCreateResponse
    {
    }
}
