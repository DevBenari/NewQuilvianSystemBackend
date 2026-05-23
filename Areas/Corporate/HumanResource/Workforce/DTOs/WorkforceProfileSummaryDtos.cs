using QuilvianSystemBackend.Enum;

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

        public WorkforceMainIdentitySummaryResponse MainIdentity { get; set; } = new();

        public WorkforceUserAccountSummaryResponse UserAccount { get; set; } = new();

        public WorkforceOrganizationSummaryResponse Organization { get; set; } = new();

        public WorkforceDocumentComplianceSummaryResponse DocumentCompliance { get; set; } = new();

        public WorkforceHealthSummaryResponse Health { get; set; } = new();

        public WorkforceScheduleAttendanceSummaryResponse ScheduleAttendance { get; set; } = new();

        public WorkforcePayrollBenefitSummaryResponse PayrollBenefit { get; set; } = new();

        public WorkforceLeaveOvertimeSummaryResponse LeaveOvertime { get; set; } = new();
    }

    public class WorkforceMainIdentitySummaryResponse
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

    public class WorkforceUserAccountSummaryResponse
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

    public class WorkforceOrganizationSummaryResponse
    {
        public int TotalAssignment { get; set; }

        public int ActiveAssignment { get; set; }

        public bool HasPrimaryAssignment { get; set; }

        public Guid? PrimaryDepartmentId { get; set; }

        public string? PrimaryDepartmentCode { get; set; }

        public string? PrimaryDepartmentName { get; set; }

        public Guid? PrimaryPositionId { get; set; }

        public string? PrimaryPositionCode { get; set; }

        public string? PrimaryPositionName { get; set; }
    }

    public class WorkforceDocumentComplianceSummaryResponse
    {
        public int DocumentTotal { get; set; }

        public int DocumentVerified { get; set; }

        public int DocumentUnverified { get; set; }

        public int DocumentExpired { get; set; }

        public int EducationTotal { get; set; }

        public int EducationVerified { get; set; }

        public int TrainingTotal { get; set; }

        public int TrainingVerified { get; set; }

        public int CertificationTotal { get; set; }

        public int CertificationVerified { get; set; }

        public int CertificationExpired { get; set; }

        public int CredentialLicenseTotal { get; set; }

        public int CredentialLicenseVerified { get; set; }

        public int CredentialLicenseExpired { get; set; }

        public int ClinicalPrivilegeTotal { get; set; }

        public int ClinicalPrivilegeActive { get; set; }

        public bool HasExpiredCompliance { get; set; }

        public bool HasPendingVerification { get; set; }
    }

    public class WorkforceHealthSummaryResponse
    {
        public int HealthRecordTotal { get; set; }

        public int HealthRecordVerified { get; set; }

        public int HealthRecordExpired { get; set; }

        public bool? LatestFitToWork { get; set; }

        public DateTime? LatestHealthRecordDate { get; set; }

        public DateTime? NearestHealthRecordExpiredDate { get; set; }
    }

    public class WorkforceScheduleAttendanceSummaryResponse
    {
        public int WorkScheduleAssignmentTotal { get; set; }

        public int ActiveWorkScheduleAssignment { get; set; }

        public DateTime? LatestScheduleDate { get; set; }

        public int AttendanceThisMonth { get; set; }

        public int LateThisMonth { get; set; }

        public int AttendanceAllTime { get; set; }
    }

    public class WorkforcePayrollBenefitSummaryResponse
    {
        public bool HasPrimaryBankAccount { get; set; }

        public int BankAccountTotal { get; set; }

        public bool HasPayrollProfile { get; set; }

        public bool IsPayrollActive { get; set; }

        public bool HasTaxProfile { get; set; }

        public bool HasInsuranceProfile { get; set; }

        public bool HasTransportAllowanceProfile { get; set; }

        public bool IsTransportEligible { get; set; }

        public bool IsNightTransportEligible { get; set; }
    }

    public class WorkforceLeaveOvertimeSummaryResponse
    {
        public int LeaveBalanceTotal { get; set; }

        public decimal TotalRemainingLeave { get; set; }

        public int LeaveRequestTotal { get; set; }

        public int PendingLeaveRequest { get; set; }

        public int ApprovedLeaveRequest { get; set; }

        public int OvertimeRequestTotal { get; set; }

        public int PendingOvertimeRequest { get; set; }

        public int ApprovedOvertimeRequest { get; set; }

        public int ApprovedOvertimeMinutes { get; set; }

        public decimal ApprovedOvertimeHours { get; set; }
    }
}
