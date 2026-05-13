using QuilvianSystemBackend.Enum;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs
{
    public class WorkforceOrganizationAssignmentResponse
    {
        public Guid Id { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public Guid DepartmentId { get; set; }

        public string DepartmentCode { get; set; } = string.Empty;

        public string DepartmentName { get; set; } = string.Empty;

        public Guid PositionId { get; set; }

        public string PositionCode { get; set; } = string.Empty;

        public string PositionName { get; set; } = string.Empty;

        public bool IsPrimary { get; set; }

        public bool IsActive { get; set; }

        public DateTime EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public string? Description { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforceOrganizationAssignmentListResponse
    {
        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int TotalData { get; set; }

        public int ActiveData { get; set; }

        public int PrimaryData { get; set; }

        public List<WorkforceOrganizationAssignmentResponse> Items { get; set; } = new();
    }

    public class CreateWorkforceOrganizationAssignmentRequest
    {
        [Required]
        public Guid DepartmentId { get; set; }

        [Required]
        public Guid PositionId { get; set; }

        public bool IsPrimary { get; set; } = false;

        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateWorkforceOrganizationAssignmentRequest
    {
        [Required]
        public Guid DepartmentId { get; set; }

        [Required]
        public Guid PositionId { get; set; }

        public bool IsPrimary { get; set; } = false;

        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateWorkforceOrganizationAssignmentStatusRequest
    {
        public bool IsActive { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class SetWorkforceOrganizationAssignmentPrimaryRequest
    {
        public bool IsPrimary { get; set; } = true;
    }

    public class WorkforceBankAccountResponse
    {
        public Guid Id { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string BankName { get; set; } = string.Empty;

        public string AccountNumber { get; set; } = string.Empty;

        public string AccountHolderName { get; set; } = string.Empty;

        public string? BankBranch { get; set; }

        public bool IsPrimary { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforceBankAccountListResponse
    {
        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int TotalData { get; set; }

        public int ActiveData { get; set; }

        public int PrimaryData { get; set; }

        public List<WorkforceBankAccountResponse> Items { get; set; } = new();
    }

    public class CreateWorkforceBankAccountRequest
    {
        [Required]
        [MaxLength(100)]
        public string BankName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string AccountNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string AccountHolderName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? BankBranch { get; set; }

        public bool IsPrimary { get; set; } = false;

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceBankAccountRequest
    {
        [Required]
        [MaxLength(100)]
        public string BankName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string AccountNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string AccountHolderName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? BankBranch { get; set; }

        public bool IsPrimary { get; set; } = false;

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceBankAccountStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class SetWorkforceBankAccountPrimaryRequest
    {
        public bool IsPrimary { get; set; } = true;
    }

    public class WorkforceRequirementResponse
    {
        public Guid Id { get; set; }

        public UserType UserType { get; set; }

        public string RequirementCategory { get; set; } = string.Empty;

        public string RequirementCode { get; set; } = string.Empty;

        public string RequirementName { get; set; } = string.Empty;

        public bool IsRequired { get; set; }

        public bool IsMultipleAllowed { get; set; }

        public bool IsFileRequired { get; set; }

        public bool IsNumberRequired { get; set; }

        public bool IsIssueDateRequired { get; set; }

        public bool IsExpiredDateRequired { get; set; }

        public bool IsVerificationRequired { get; set; }

        public int SortOrder { get; set; }

        public bool IsActive { get; set; }

        public string? Description { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforceRequirementListResponse
    {
        public int TotalData { get; set; }

        public int ActiveData { get; set; }

        public int RequiredData { get; set; }

        public List<WorkforceRequirementResponse> Items { get; set; } = new();
    }

    public class CreateWorkforceRequirementRequest
    {
        [Required]
        public UserType UserType { get; set; }

        [Required]
        [MaxLength(50)]
        public string RequirementCategory { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string RequirementCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string RequirementName { get; set; } = string.Empty;

        public bool IsRequired { get; set; } = true;

        public bool IsMultipleAllowed { get; set; } = false;

        public bool IsFileRequired { get; set; } = true;

        public bool IsNumberRequired { get; set; } = false;

        public bool IsIssueDateRequired { get; set; } = false;

        public bool IsExpiredDateRequired { get; set; } = false;

        public bool IsVerificationRequired { get; set; } = true;

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateWorkforceRequirementRequest
    {
        [Required]
        public UserType UserType { get; set; }

        [Required]
        [MaxLength(50)]
        public string RequirementCategory { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string RequirementCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string RequirementName { get; set; } = string.Empty;

        public bool IsRequired { get; set; } = true;

        public bool IsMultipleAllowed { get; set; } = false;

        public bool IsFileRequired { get; set; } = true;

        public bool IsNumberRequired { get; set; } = false;

        public bool IsIssueDateRequired { get; set; } = false;

        public bool IsExpiredDateRequired { get; set; } = false;

        public bool IsVerificationRequired { get; set; } = true;

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateWorkforceRequirementStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class WorkforceRequirementChecklistItemResponse
    {
        public Guid RequirementId { get; set; }

        public UserType UserType { get; set; }

        public string RequirementCategory { get; set; } = string.Empty;

        public string RequirementCode { get; set; } = string.Empty;

        public string RequirementName { get; set; } = string.Empty;

        public bool IsRequired { get; set; }

        public bool IsFileRequired { get; set; }

        public bool IsNumberRequired { get; set; }

        public bool IsIssueDateRequired { get; set; }

        public bool IsExpiredDateRequired { get; set; }

        public bool IsVerificationRequired { get; set; }

        public bool IsSubmitted { get; set; }

        public bool IsVerified { get; set; }

        public bool IsExpired { get; set; }

        public Guid? SourceId { get; set; }

        public string? SourceType { get; set; }

        public string? FilePath { get; set; }

        public DateTime? ExpiredDate { get; set; }

        public string Status { get; set; } = "Missing";

        public int SortOrder { get; set; }

        public string? Description { get; set; }
    }

    public class WorkforceRequirementChecklistGroupResponse
    {
        public string RequirementCategory { get; set; } = string.Empty;

        public int TotalData { get; set; }

        public int CompletedData { get; set; }

        public int MissingData { get; set; }

        public int NeedVerificationData { get; set; }

        public int ExpiredData { get; set; }

        public List<WorkforceRequirementChecklistItemResponse> Items { get; set; } = new();
    }

    public class WorkforceProfileRequirementChecklistResponse
    {
        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public UserType UserType { get; set; }

        public int TotalRequirement { get; set; }

        public int CompletedRequirement { get; set; }

        public int MissingRequirement { get; set; }

        public int NeedVerificationRequirement { get; set; }

        public int ExpiredRequirement { get; set; }

        public List<WorkforceRequirementChecklistGroupResponse> Groups { get; set; } = new();
    }

    public class WorkforceDocumentResponse
    {
        public Guid Id { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string? RequirementCode { get; set; }

        public string DocumentType { get; set; } = string.Empty;

        public string DocumentName { get; set; } = string.Empty;

        public string? DocumentNumber { get; set; }

        public DateTime? IssueDate { get; set; }

        public DateTime? ExpiredDate { get; set; }

        public string? FilePath { get; set; }

        public string? FileContentType { get; set; }

        public bool HasFile { get; set; }

        public bool IsVerified { get; set; }

        public bool IsExpired { get; set; }

        public bool IsActive { get; set; }

        public string? Description { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforceDocumentListResponse
    {
        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int TotalData { get; set; }

        public int ActiveData { get; set; }

        public int VerifiedData { get; set; }

        public int ExpiredData { get; set; }

        public int DocumentWithFileData { get; set; }

        public List<WorkforceDocumentResponse> Items { get; set; } = new();
    }

    public class CreateWorkforceDocumentRequest
    {
        [MaxLength(100)]
        public string? RequirementCode { get; set; }

        [Required]
        [MaxLength(100)]
        public string DocumentType { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string DocumentName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? DocumentNumber { get; set; }

        public DateTime? IssueDate { get; set; }

        public DateTime? ExpiredDate { get; set; }

        public IFormFile? File { get; set; }

        public bool IsVerified { get; set; } = false;

        public bool IsActive { get; set; } = true;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateWorkforceDocumentRequest
    {
        [MaxLength(100)]
        public string? RequirementCode { get; set; }

        [Required]
        [MaxLength(100)]
        public string DocumentType { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string DocumentName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? DocumentNumber { get; set; }

        public DateTime? IssueDate { get; set; }

        public DateTime? ExpiredDate { get; set; }

        public IFormFile? File { get; set; }

        public bool ReplaceExistingFile { get; set; } = false;

        public bool IsVerified { get; set; } = false;

        public bool IsActive { get; set; } = true;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateWorkforceDocumentStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class VerifyWorkforceDocumentRequest
    {
        public bool IsVerified { get; set; } = true;
    }

    public class WorkforceEducationResponse
    {
        public Guid Id { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string? RequirementCode { get; set; }

        public string EducationLevel { get; set; } = string.Empty;

        public string InstitutionName { get; set; } = string.Empty;

        public string? Major { get; set; }

        public int? GraduationYear { get; set; }

        public string? CertificateNumber { get; set; }

        public string? FilePath { get; set; }

        public string? FileContentType { get; set; }

        public bool HasFile { get; set; }

        public bool IsVerified { get; set; }

        public bool IsActive { get; set; }

        public string? Description { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforceEducationListResponse
    {
        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int TotalData { get; set; }

        public int ActiveData { get; set; }

        public int VerifiedData { get; set; }

        public int EducationWithFileData { get; set; }

        public List<WorkforceEducationResponse> Items { get; set; } = new();
    }

    public class CreateWorkforceEducationRequest
    {
        [MaxLength(100)]
        public string? RequirementCode { get; set; }

        [Required]
        [MaxLength(100)]
        public string EducationLevel { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string InstitutionName { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? Major { get; set; }

        public int? GraduationYear { get; set; }

        [MaxLength(100)]
        public string? CertificateNumber { get; set; }

        public IFormFile? File { get; set; }

        public bool IsVerified { get; set; } = false;

        public bool IsActive { get; set; } = true;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateWorkforceEducationRequest
    {
        [MaxLength(100)]
        public string? RequirementCode { get; set; }

        [Required]
        [MaxLength(100)]
        public string EducationLevel { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string InstitutionName { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? Major { get; set; }

        public int? GraduationYear { get; set; }

        [MaxLength(100)]
        public string? CertificateNumber { get; set; }

        public IFormFile? File { get; set; }

        public bool ReplaceExistingFile { get; set; } = false;

        public bool IsVerified { get; set; } = false;

        public bool IsActive { get; set; } = true;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateWorkforceEducationStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class VerifyWorkforceEducationRequest
    {
        public bool IsVerified { get; set; } = true;
    }

    public class WorkforceTrainingRecordResponse
    {
        public Guid Id { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string? RequirementCode { get; set; }

        public string TrainingType { get; set; } = string.Empty;

        public string TrainingName { get; set; } = string.Empty;

        public string? Organizer { get; set; }

        public string? Location { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string? CertificateNumber { get; set; }

        public decimal CreditPoint { get; set; }

        public string? FilePath { get; set; }

        public string? FileContentType { get; set; }

        public bool HasFile { get; set; }

        public bool IsVerified { get; set; }

        public bool IsActive { get; set; }

        public string? Description { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforceTrainingRecordListResponse
    {
        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int TotalData { get; set; }

        public int ActiveData { get; set; }

        public int VerifiedData { get; set; }

        public int TrainingWithFileData { get; set; }

        public decimal TotalCreditPoint { get; set; }

        public List<WorkforceTrainingRecordResponse> Items { get; set; } = new();
    }

    public class CreateWorkforceTrainingRecordRequest
    {
        [MaxLength(100)]
        public string? RequirementCode { get; set; }

        [Required]
        [MaxLength(100)]
        public string TrainingType { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string TrainingName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Organizer { get; set; }

        [MaxLength(200)]
        public string? Location { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [MaxLength(100)]
        public string? CertificateNumber { get; set; }

        public decimal CreditPoint { get; set; } = 0;

        public IFormFile? File { get; set; }

        public bool IsVerified { get; set; } = false;

        public bool IsActive { get; set; } = true;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateWorkforceTrainingRecordRequest
    {
        [MaxLength(100)]
        public string? RequirementCode { get; set; }

        [Required]
        [MaxLength(100)]
        public string TrainingType { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string TrainingName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Organizer { get; set; }

        [MaxLength(200)]
        public string? Location { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [MaxLength(100)]
        public string? CertificateNumber { get; set; }

        public decimal CreditPoint { get; set; } = 0;

        public IFormFile? File { get; set; }

        public bool ReplaceExistingFile { get; set; } = false;

        public bool IsVerified { get; set; } = false;

        public bool IsActive { get; set; } = true;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateWorkforceTrainingRecordStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class VerifyWorkforceTrainingRecordRequest
    {
        public bool IsVerified { get; set; } = true;
    }

    public class WorkforceCertificationResponse
    {
        public Guid Id { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string? RequirementCode { get; set; }

        public string CertificationType { get; set; } = string.Empty;

        public string CertificationName { get; set; } = string.Empty;

        public string? Issuer { get; set; }

        public string? CertificateNumber { get; set; }

        public DateTime IssueDate { get; set; }

        public DateTime? ExpiredDate { get; set; }

        public bool IsLifetime { get; set; }

        public string? FilePath { get; set; }

        public string? FileContentType { get; set; }

        public bool HasFile { get; set; }

        public bool IsVerified { get; set; }

        public bool IsExpired { get; set; }

        public bool IsActive { get; set; }

        public string? Description { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforceCertificationListResponse
    {
        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int TotalData { get; set; }

        public int ActiveData { get; set; }

        public int VerifiedData { get; set; }

        public int ExpiredData { get; set; }

        public int CertificationWithFileData { get; set; }

        public List<WorkforceCertificationResponse> Items { get; set; } = new();
    }

    public class CreateWorkforceCertificationRequest
    {
        [MaxLength(100)]
        public string? RequirementCode { get; set; }

        [Required]
        [MaxLength(100)]
        public string CertificationType { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string CertificationName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Issuer { get; set; }

        [MaxLength(100)]
        public string? CertificateNumber { get; set; }

        [Required]
        public DateTime IssueDate { get; set; }

        public DateTime? ExpiredDate { get; set; }

        public bool IsLifetime { get; set; } = false;

        public IFormFile? File { get; set; }

        public bool IsVerified { get; set; } = false;

        public bool IsActive { get; set; } = true;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateWorkforceCertificationRequest
    {
        [MaxLength(100)]
        public string? RequirementCode { get; set; }

        [Required]
        [MaxLength(100)]
        public string CertificationType { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string CertificationName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Issuer { get; set; }

        [MaxLength(100)]
        public string? CertificateNumber { get; set; }

        [Required]
        public DateTime IssueDate { get; set; }

        public DateTime? ExpiredDate { get; set; }

        public bool IsLifetime { get; set; } = false;

        public IFormFile? File { get; set; }

        public bool ReplaceExistingFile { get; set; } = false;

        public bool IsVerified { get; set; } = false;

        public bool IsActive { get; set; } = true;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateWorkforceCertificationStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class VerifyWorkforceCertificationRequest
    {
        public bool IsVerified { get; set; } = true;
    }

    public class WorkforceCredentialLicenseResponse
    {
        public Guid Id { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string? RequirementCode { get; set; }

        public string LicenseType { get; set; } = string.Empty;

        public string LicenseNumber { get; set; } = string.Empty;

        public string? Issuer { get; set; }

        public DateTime IssueDate { get; set; }

        public DateTime ExpiredDate { get; set; }

        public string? PracticeLocation { get; set; }

        public string? FilePath { get; set; }

        public string? FileContentType { get; set; }

        public bool HasFile { get; set; }

        public bool IsVerified { get; set; }

        public bool IsExpired { get; set; }

        public bool IsActive { get; set; }

        public string? Description { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforceCredentialLicenseListResponse
    {
        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int TotalData { get; set; }

        public int ActiveData { get; set; }

        public int VerifiedData { get; set; }

        public int ExpiredData { get; set; }

        public int CredentialLicenseWithFileData { get; set; }

        public List<WorkforceCredentialLicenseResponse> Items { get; set; } = new();
    }

    public class CreateWorkforceCredentialLicenseRequest
    {
        [MaxLength(100)]
        public string? RequirementCode { get; set; }

        [Required]
        [MaxLength(100)]
        public string LicenseType { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LicenseNumber { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Issuer { get; set; }

        [Required]
        public DateTime IssueDate { get; set; }

        [Required]
        public DateTime ExpiredDate { get; set; }

        [MaxLength(200)]
        public string? PracticeLocation { get; set; }

        public IFormFile? File { get; set; }

        public bool IsVerified { get; set; } = false;

        public bool IsActive { get; set; } = true;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateWorkforceCredentialLicenseRequest
    {
        [MaxLength(100)]
        public string? RequirementCode { get; set; }

        [Required]
        [MaxLength(100)]
        public string LicenseType { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LicenseNumber { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Issuer { get; set; }

        [Required]
        public DateTime IssueDate { get; set; }

        [Required]
        public DateTime ExpiredDate { get; set; }

        [MaxLength(200)]
        public string? PracticeLocation { get; set; }

        public IFormFile? File { get; set; }

        public bool ReplaceExistingFile { get; set; } = false;

        public bool IsVerified { get; set; } = false;

        public bool IsActive { get; set; } = true;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateWorkforceCredentialLicenseStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class VerifyWorkforceCredentialLicenseRequest
    {
        public bool IsVerified { get; set; } = true;
    }
}