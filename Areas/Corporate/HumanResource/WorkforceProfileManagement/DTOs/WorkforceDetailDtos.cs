namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceProfileManagement.DTOs
{
    /// <summary>
    /// Aggregated read model for the workforce profile page.
    /// This response does not replace the individual WFP CRUD endpoints.
    /// </summary>
    public class WorkforceDetailResponse
    {
        public WorkforceDetailProfileResponse Profile { get; set; } = new();
        public WorkforceDetailContextResponse Context { get; set; } = new();
        public WorkforceDetailSummaryResponse Summary { get; set; } = new();
    }

    public class WorkforceDetailProfileResponse
    {
        public Guid WorkforceProfileId { get; set; }
        public string ProfileCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? WhatsAppNumber { get; set; }
        public Guid? PrimaryDepartmentId { get; set; }
        public Guid? PrimaryPositionId { get; set; }
        public bool IsActive { get; set; }

        public Guid? EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? EmployeeNumber { get; set; }

        public Guid? DoctorId { get; set; }
        public string? DoctorCode { get; set; }
        public string? DoctorNumber { get; set; }

        public Guid? ExternalUserId { get; set; }
        public string? ExternalCode { get; set; }
    }

    public class WorkforceDetailContextResponse
    {
        public bool IsEmployee { get; set; }
        public bool IsDoctor { get; set; }
        public bool IsExternalUser { get; set; }
        public bool IsClinicalWorkforce { get; set; }
        public bool IsPayrollEligible { get; set; }
        public bool HasActiveOrganizationAssignment { get; set; }
        public bool HasActiveWorkSchedule { get; set; }

        public List<string> AvailableSections { get; set; } = new();
        public List<string> RestrictedSections { get; set; } = new();
    }

    public class WorkforceDetailSummaryResponse
    {
        public int ProfileSummaryCount { get; set; }

        public int AddressCount { get; set; }
        public int BankAccountCount { get; set; }
        public int EducationCount { get; set; }
        public int DocumentCount { get; set; }
        public int EmergencyContactCount { get; set; }
        public int FamilyMemberCount { get; set; }
        public int DependentCount { get; set; }
        public int ContractHistoryCount { get; set; }
        public int EmploymentHistoryCount { get; set; }

        public int OrganizationAssignmentCount { get; set; }
        public int PositionAssignmentCount { get; set; }
        public int ManagerAssignmentCount { get; set; }
        public int SalaryAssignmentCount { get; set; }
        public int WorkScheduleAssignmentCount { get; set; }

        public int TrainingRecordCount { get; set; }
        public int CompetencyAssessmentCount { get; set; }
        public int VerifiedCompetencyAssessmentCount { get; set; }
        public int ExpiredCompetencyAssessmentCount { get; set; }
        public int CertificationCount { get; set; }
        public int CredentialLicenseCount { get; set; }
        public int ClinicalPrivilegeCount { get; set; }
        public int HealthRecordCount { get; set; }
        public int PerformanceReviewCount { get; set; }
        public int DisciplinaryActionCount { get; set; }

        public bool HasPayroll { get; set; }
        public bool HasTax { get; set; }
        public bool HasInsurance { get; set; }
        public bool HasTransportAllowance { get; set; }

        public int ComplianceAlertCount { get; set; }
        public int OpenComplianceAlertCount { get; set; }
        public int CriticalComplianceAlertCount { get; set; }
        public int OverdueComplianceAlertCount { get; set; }
    }
}
