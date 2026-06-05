using QuilvianSystemBackend.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs
{
    public class WorkforceProfileSummaryResponse
    {
        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public UserType UserType { get; set; }

        public string UserTypeName { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        public string? WhatsAppNumber { get; set; }

        public Guid? PrimaryDepartmentId { get; set; }

        public string? PrimaryDepartmentCode { get; set; }

        public string? PrimaryDepartmentName { get; set; }

        public Guid? PrimaryPositionId { get; set; }

        public string? PrimaryPositionCode { get; set; }

        public string? PrimaryPositionName { get; set; }

        public WorkforceProfileMainIdentitySummaryResponse MainIdentity { get; set; } = new();

        public WorkforceProfileUserAccountSummaryResponse UserAccount { get; set; } = new();

        public WorkforceProfileOrganizationAssignmentSummaryResponse OrganizationAssignment { get; set; } = new();

        public WorkforceProfileDocumentComplianceSummaryResponse DocumentCompliance { get; set; } = new();

        public WorkforceProfileHealthSummaryResponse Health { get; set; } = new();

        public WorkforceProfileBankInsuranceSummaryResponse BankInsurance { get; set; } = new();

        public WorkforceProfileCompetencySummaryResponse Competency { get; set; } = new();

        public WorkforceProfileEmploymentHistorySummaryResponse EmploymentHistory { get; set; } = new();

        public WorkforceProfileCompletionSummaryResponse Completion { get; set; } = new();
    }

    public class WorkforceProfileMainIdentitySummaryResponse
    {
        public Guid? EmployeeId { get; set; }

        public Guid? DoctorId { get; set; }

        public Guid? ExternalUserId { get; set; }

        public string? Code { get; set; }

        public string? Number { get; set; }

        public string? Status { get; set; }

        public string? Type { get; set; }

        public string? EmploymentType { get; set; }

        public string? WorkLocation { get; set; }

        public DateTime? JoinDate { get; set; }

        public DateTime? ContractStartDate { get; set; }

        public DateTime? ContractEndDate { get; set; }

        public DateTime? ResignDate { get; set; }

        public DateTime? AccessStartDate { get; set; }

        public DateTime? AccessEndDate { get; set; }
    }

    public class WorkforceProfileUserAccountSummaryResponse
    {
        public bool HasUserAccount { get; set; }

        public Guid? UserId { get; set; }

        public string? UserCode { get; set; }

        public string? UserName { get; set; }

        public string? Email { get; set; }

        public bool IsActive { get; set; }

        public bool MustChangePassword { get; set; }

        public DateTime? LastLoginAt { get; set; }

        public DateTime? AccessValidUntil { get; set; }

        public bool IsAccessExpired { get; set; }

        public bool IsFingerprintRegistrationEnabled { get; set; }

        public int FingerprintCredentialCount { get; set; }
    }

    public class WorkforceProfileOrganizationAssignmentSummaryResponse
    {
        public int TotalAssignment { get; set; }

        public int ActiveAssignment { get; set; }

        public int InactiveAssignment { get; set; }

        public int PrimaryAssignment { get; set; }

        public int CurrentlyValidAssignment { get; set; }

        public int ExpiredAssignment { get; set; }

        public bool HasPrimaryAssignment { get; set; }

        public Guid? PrimaryDepartmentId { get; set; }

        public string? PrimaryDepartmentCode { get; set; }

        public string? PrimaryDepartmentName { get; set; }

        public Guid? PrimaryPositionId { get; set; }

        public string? PrimaryPositionCode { get; set; }

        public string? PrimaryPositionName { get; set; }
    }

    public class WorkforceProfileDocumentComplianceSummaryResponse
    {
        public int DocumentTotal { get; set; }

        public int DocumentVerified { get; set; }

        public int DocumentUnverified { get; set; }

        public int DocumentExpired { get; set; }

        public int DocumentWithFile { get; set; }

        public int EducationTotal { get; set; }

        public int EducationVerified { get; set; }

        public int EducationUnverified { get; set; }

        public int EducationWithFile { get; set; }

        public int TrainingTotal { get; set; }

        public int TrainingVerified { get; set; }

        public int TrainingUnverified { get; set; }

        public int TrainingWithFile { get; set; }

        public decimal TrainingTotalCreditPoint { get; set; }

        public int CertificationTotal { get; set; }

        public int CertificationVerified { get; set; }

        public int CertificationUnverified { get; set; }

        public int CertificationExpired { get; set; }

        public int CertificationWithFile { get; set; }

        public int CredentialLicenseTotal { get; set; }

        public int CredentialLicenseVerified { get; set; }

        public int CredentialLicenseUnverified { get; set; }

        public int CredentialLicenseExpired { get; set; }

        public int CredentialLicenseCurrentlyValid { get; set; }

        public int CredentialLicenseWithFile { get; set; }

        public int ClinicalPrivilegeTotal { get; set; }

        public int ClinicalPrivilegeActive { get; set; }

        public int ClinicalPrivilegeCurrentlyValid { get; set; }

        public int ClinicalPrivilegeExpired { get; set; }

        public int ClinicalPrivilegeWithFile { get; set; }

        public bool HasExpiredCompliance { get; set; }

        public bool HasPendingVerification { get; set; }
    }

    public class WorkforceProfileHealthSummaryResponse
    {
        public int HealthRecordTotal { get; set; }

        public int HealthRecordVerified { get; set; }

        public int HealthRecordUnverified { get; set; }

        public int HealthRecordExpired { get; set; }

        public int HealthRecordCurrentlyValid { get; set; }

        public int HealthRecordCompliantForWork { get; set; }

        public int HealthRecordWithFile { get; set; }

        public bool? LatestFitToWork { get; set; }

        public DateTime? LatestHealthRecordDate { get; set; }

        public DateTime? NearestHealthRecordExpiredDate { get; set; }
    }

    public class WorkforceProfileBankInsuranceSummaryResponse
    {
        public int BankAccountTotal { get; set; }

        public int ActiveBankAccount { get; set; }

        public bool HasPrimaryBankAccount { get; set; }

        public string? PrimaryBankName { get; set; }

        public string? PrimaryAccountNumberMasked { get; set; }

        public int InsuranceTotal { get; set; }

        public int ActiveInsurance { get; set; }

        public bool HasInsuranceProfile { get; set; }

        public bool HasBpjsKesehatan { get; set; }

        public bool HasBpjsKetenagakerjaan { get; set; }

        public bool HasPrivateInsurance { get; set; }

        public bool HasCurrentlyValidInsurance { get; set; }
    }

    public class WorkforceProfileCompetencySummaryResponse
    {
        public int AssessmentTotal { get; set; }

        public int AssessmentVerified { get; set; }

        public int AssessmentUnverified { get; set; }

        public int AssessmentExpired { get; set; }

        public int PassedAssessment { get; set; }

        public int FailedAssessment { get; set; }

        public int NeedTrainingAssessment { get; set; }

        public DateTime? LatestAssessmentDate { get; set; }
    }

    public class WorkforceProfileEmploymentHistorySummaryResponse
    {
        public int HistoryTotal { get; set; }

        public int ActiveHistory { get; set; }

        public DateTime? LatestEffectiveDate { get; set; }

        public string? LatestHistoryType { get; set; }

        public string? LatestOldStatus { get; set; }

        public string? LatestNewStatus { get; set; }
    }

    public class WorkforceProfileCompletionSummaryResponse
    {
        public int TotalRequiredSection { get; set; }

        public int CompletedSection { get; set; }

        public decimal CompletionPercentage { get; set; }

        public List<string> MissingSections { get; set; } = new();

        public List<string> WarningSections { get; set; } = new();
    }
}
