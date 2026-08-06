namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs
{
    public class OvertimePlanQueryRequest
    {
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? PlanStatus { get; set; }
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? CostCenterId { get; set; }
        public Guid? WorkLocationId { get; set; }
        public Guid? RosterPeriodId { get; set; }
        public Guid? WorkforceProfileId { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "planStartDate";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class OvertimePlanFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string DateTimeFormat { get; set; } = "yyyy-MM-ddTHH:mm:ssZ";
        public List<OvertimePlanStringOptionResponse> PlanStatusOptions { get; set; } = new();
        public List<OvertimePlanStringOptionResponse> DetailStatusOptions { get; set; } = new();
        public List<OvertimePlanStringOptionResponse> DayTypeOptions { get; set; } = new();
        public List<OvertimePlanStringOptionResponse> OvertimeCategoryOptions { get; set; } = new();
        public List<OvertimePlanSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class OvertimePlanStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class OvertimePlanSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class OvertimePlanSummaryResponse
    {
        public int TotalPlan { get; set; }
        public int DraftPlan { get; set; }
        public int ValidatedPlan { get; set; }
        public int PublishedPlan { get; set; }
        public int PartiallyConvertedPlan { get; set; }
        public int ConvertedPlan { get; set; }
        public int CancelledPlan { get; set; }
        public int ClosedPlan { get; set; }
        public int TotalDetail { get; set; }
        public int ValidDetail { get; set; }
        public int ConflictDetail { get; set; }
        public int GeneratedRequest { get; set; }
        public int TotalPlannedMinutes { get; set; }
    }

    public class OvertimePlanListResponse
    {
        public Guid Id { get; set; }
        public string PlanNumber { get; set; } = string.Empty;
        public string PlanTitle { get; set; } = string.Empty;
        public DateOnly PlanStartDate { get; set; }
        public DateOnly PlanEndDate { get; set; }
        public string PlanStatus { get; set; } = string.Empty;
        public Guid? LegalEntityId { get; set; }
        public string? LegalEntityName { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public string? HospitalSiteName { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public string? OrganizationUnitName { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public Guid? CostCenterId { get; set; }
        public string? CostCenterName { get; set; }
        public Guid? WorkLocationId { get; set; }
        public string? WorkLocationName { get; set; }
        public Guid? RosterPeriodId { get; set; }
        public int TotalDetail { get; set; }
        public int ConflictDetail { get; set; }
        public int GeneratedRequest { get; set; }
        public int TotalPlannedMinutes { get; set; }
        public bool IsActive { get; set; }
        public DateTime? ValidatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }


    public class OvertimePlanOptionResponse
    {
        public Guid Id { get; set; }
        public string PlanNumber { get; set; } = string.Empty;
        public string PlanTitle { get; set; } = string.Empty;
        public DateOnly PlanStartDate { get; set; }
        public DateOnly PlanEndDate { get; set; }
        public string PlanStatus { get; set; } = string.Empty;
        public int TotalDetail { get; set; }
    }

}
