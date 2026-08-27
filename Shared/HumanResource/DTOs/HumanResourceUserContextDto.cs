namespace QuilvianSystemBackend.Shared.HumanResource.DTOs
{
    public class HumanResourceUserContextDto
    {
        public Guid UserId { get; set; }

        public string UserCode { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string UserType { get; set; } = string.Empty;

        public bool IsUserActive { get; set; }

        public DateTime? AccessValidUntil { get; set; }

        public Guid? WorkforceProfileId { get; set; }

        public string? WorkforceProfileCode { get; set; }

        public string? WorkforceDisplayName { get; set; }

        public Guid? EmployeeId { get; set; }

        public Guid? DoctorId { get; set; }

        public Guid? ExternalUserId { get; set; }

        public string ProfileType { get; set; } = "AccountOnly";

        public bool HasWorkforceProfile { get; set; }

        public bool HasValidWorkforceSubtype { get; set; }

        public Guid? OrganizationAssignmentId { get; set; }

        public Guid? LegalEntityId { get; set; }

        public Guid? HospitalSiteId { get; set; }

        public Guid? OrganizationUnitId { get; set; }

        public Guid? DepartmentId { get; set; }

        public Guid? PositionId { get; set; }

        public Guid? CostCenterId { get; set; }

        public Guid? WorkLocationId { get; set; }

        public Guid? EmployeeGradeId { get; set; }

        public string? HospitalSiteName { get; set; }

        public string? OrganizationUnitName { get; set; }

        public string? DepartmentName { get; set; }

        public string? PositionName { get; set; }

        public string? WorkLocationName { get; set; }

        public string? AssignmentType { get; set; }

        public bool HasOrganizationAssignment { get; set; }

        public Guid? ManagerAssignmentId { get; set; }

        public Guid? ManagerWorkforceProfileId { get; set; }

        public string? ManagerProfileCode { get; set; }

        public string? ManagerDisplayName { get; set; }

        public string? ManagerType { get; set; }

        public bool ManagerCanApproveRequests { get; set; }

        public bool HasManager { get; set; }

        public bool IsManager { get; set; }

        public bool CanApproveRequests { get; set; }

        public int DirectReportCount { get; set; }

        public List<HumanResourceDirectReportDto> DirectReports { get; set; } = new();

        public List<string> Roles { get; set; } = new();

        public bool IsContextComplete { get; set; }

        public List<string> Warnings { get; set; } = new();
    }

    public class HumanResourceDirectReportDto
    {
        public Guid ManagerAssignmentId { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public string WorkforceProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string ManagerType { get; set; } = string.Empty;

        public bool IsPrimaryManager { get; set; }

        public bool CanApproveRequests { get; set; }
    }
}
