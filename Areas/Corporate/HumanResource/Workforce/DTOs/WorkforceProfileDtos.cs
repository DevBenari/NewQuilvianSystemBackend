using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
using QuilvianSystemBackend.Enums;
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

        public CredentialVerificationStatus VerificationStatus { get; set; }

        public bool IsVerified { get; set; }

        public Guid? VerifiedByUserId { get; set; }

        public string? VerifiedByUserName { get; set; }

        public DateTime? VerifiedAt { get; set; }

        public string? VerificationNote { get; set; }

        public Guid? RejectedByUserId { get; set; }

        public string? RejectedByUserName { get; set; }

        public DateTime? RejectedAt { get; set; }

        public string? RejectedReason { get; set; }

        public Guid? RevokedByUserId { get; set; }

        public string? RevokedByUserName { get; set; }

        public DateTime? RevokedAt { get; set; }

        public string? RevokedReason { get; set; }

        public bool IsPrimary { get; set; }

        public bool IsExpired { get; set; }

        public bool IsCurrentlyValid { get; set; }

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

        public int PendingVerificationData { get; set; }

        public int RejectedData { get; set; }

        public int RevokedData { get; set; }

        public int ExpiredData { get; set; }

        public int CurrentlyValidData { get; set; }

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

        public bool IsPrimary { get; set; } = false;

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

        public bool IsPrimary { get; set; } = false;

        public bool IsActive { get; set; } = true;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateWorkforceCredentialLicenseStatusRequest
    {
        public bool IsActive { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class VerifyWorkforceCredentialLicenseRequest
    {
        [MaxLength(250)]
        public string? VerificationNote { get; set; }
    }

    public class RejectWorkforceCredentialLicenseRequest
    {
        [Required]
        [MaxLength(250)]
        public string RejectedReason { get; set; } = string.Empty;
    }

    public class RevokeWorkforceCredentialLicenseRequest
    {
        [Required]
        [MaxLength(250)]
        public string RevokedReason { get; set; } = string.Empty;
    }

    public class WorkforceTransportAllowanceResponse
    {
        public Guid Id { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public Guid? TransportAllowancePolicyId { get; set; }

        public string? PolicyCode { get; set; }

        public string? PolicyName { get; set; }

        public bool IsEligible { get; set; }

        public bool IsRegularTransportEligible { get; set; }

        public bool IsNightTransportEligible { get; set; }

        public string AllowanceMode { get; set; } = string.Empty;

        public decimal MonthlyAmount { get; set; }

        public decimal DailyAmount { get; set; }

        public decimal NightAmount { get; set; }

        public bool IsProrated { get; set; }

        public bool IsTaxable { get; set; }

        public bool IsPayrollComponent { get; set; }

        public DateTime EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public string? Description { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforceTransportAllowanceListResponse
    {
        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int TotalData { get; set; }

        public int ActiveData { get; set; }

        public int EligibleData { get; set; }

        public int RegularEligibleData { get; set; }

        public int NightEligibleData { get; set; }

        public List<WorkforceTransportAllowanceResponse> Items { get; set; } = new();
    }

    public class CreateWorkforceTransportAllowanceRequest
    {
        public Guid? TransportAllowancePolicyId { get; set; }

        public bool IsEligible { get; set; } = false;

        public bool IsRegularTransportEligible { get; set; } = false;

        public bool IsNightTransportEligible { get; set; } = false;

        [Required]
        [MaxLength(50)]
        public string AllowanceMode { get; set; } = "None";

        public decimal MonthlyAmount { get; set; } = 0;

        public decimal DailyAmount { get; set; } = 0;

        public decimal NightAmount { get; set; } = 0;

        public bool IsProrated { get; set; } = true;

        public bool IsTaxable { get; set; } = true;

        public bool IsPayrollComponent { get; set; } = true;

        [Required]
        public DateTime EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceTransportAllowanceRequest
    {
        public Guid? TransportAllowancePolicyId { get; set; }

        public bool IsEligible { get; set; } = false;

        public bool IsRegularTransportEligible { get; set; } = false;

        public bool IsNightTransportEligible { get; set; } = false;

        [Required]
        [MaxLength(50)]
        public string AllowanceMode { get; set; } = "None";

        public decimal MonthlyAmount { get; set; } = 0;

        public decimal DailyAmount { get; set; } = 0;

        public decimal NightAmount { get; set; } = 0;

        public bool IsProrated { get; set; } = true;

        public bool IsTaxable { get; set; } = true;

        public bool IsPayrollComponent { get; set; } = true;

        [Required]
        public DateTime EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceTransportAllowanceStatusRequest
    {
        public bool IsActive { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class WorkforceTransportAllowanceTransactionResponse
    {
        public Guid Id { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public Guid? TransportAllowanceId { get; set; }

        public Guid? TransportAllowancePolicyId { get; set; }

        public Guid? AttendanceId { get; set; }

        public DateTime TransactionDate { get; set; }

        public string PeriodYearMonth { get; set; } = string.Empty;

        public string AllowanceType { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public bool IsGeneratedFromAttendance { get; set; }

        public bool IsNightShift { get; set; }

        public string TransactionStatus { get; set; } = string.Empty;

        public string? Notes { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforceTransportAllowanceTransactionListResponse
    {
        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int TotalData { get; set; }

        public int ActiveData { get; set; }

        public int DraftData { get; set; }

        public int CalculatedData { get; set; }

        public int ApprovedData { get; set; }

        public int PostedToPayrollData { get; set; }

        public int CancelledData { get; set; }

        public decimal TotalAmount { get; set; }

        public List<WorkforceTransportAllowanceTransactionResponse> Items { get; set; } = new();
    }

    public class CreateWorkforceTransportAllowanceTransactionRequest
    {
        public Guid? TransportAllowanceId { get; set; }

        public Guid? TransportAllowancePolicyId { get; set; }

        public Guid? AttendanceId { get; set; }

        [Required]
        public DateTime TransactionDate { get; set; }

        [MaxLength(20)]
        public string? PeriodYearMonth { get; set; }

        [Required]
        [MaxLength(50)]
        public string AllowanceType { get; set; } = "Regular";

        public decimal Amount { get; set; } = 0;

        public bool IsGeneratedFromAttendance { get; set; } = false;

        public bool IsNightShift { get; set; } = false;

        [MaxLength(50)]
        public string TransactionStatus { get; set; } = "Draft";

        [MaxLength(250)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceTransportAllowanceTransactionStatusRequest
    {
        [Required]
        [MaxLength(50)]
        public string TransactionStatus { get; set; } = "Draft";

        [MaxLength(250)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class WorkforcePayrollResponse
    {
        public Guid Id { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string PayrollGroup { get; set; } = string.Empty;

        public string PaymentMethod { get; set; } = string.Empty;

        public Guid? PrimaryBankAccountId { get; set; }

        public string? PrimaryBankName { get; set; }

        public string? PrimaryBankAccountNumber { get; set; }

        public string? PrimaryBankAccountHolderName { get; set; }

        public decimal BasicSalary { get; set; }

        public decimal FixedAllowance { get; set; }

        public decimal FixedDeduction { get; set; }

        public decimal NetFixedAmount { get; set; }

        public bool IsOvertimeEligible { get; set; }

        public bool IsPayrollActive { get; set; }

        public DateTime EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public string? Description { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforcePayrollListResponse
    {
        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int TotalData { get; set; }

        public int ActiveData { get; set; }

        public int PayrollActiveData { get; set; }

        public decimal TotalBasicSalary { get; set; }

        public decimal TotalFixedAllowance { get; set; }

        public decimal TotalFixedDeduction { get; set; }

        public List<WorkforcePayrollResponse> Items { get; set; } = new();
    }

    public class CreateWorkforcePayrollRequest
    {
        [MaxLength(50)]
        public string PayrollGroup { get; set; } = "Default";

        [MaxLength(50)]
        public string PaymentMethod { get; set; } = "BankTransfer";

        public Guid? PrimaryBankAccountId { get; set; }

        public decimal BasicSalary { get; set; } = 0;

        public decimal FixedAllowance { get; set; } = 0;

        public decimal FixedDeduction { get; set; } = 0;

        public bool IsOvertimeEligible { get; set; } = false;

        public bool IsPayrollActive { get; set; } = true;

        [Required]
        public DateTime EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforcePayrollRequest
    {
        [MaxLength(50)]
        public string PayrollGroup { get; set; } = "Default";

        [MaxLength(50)]
        public string PaymentMethod { get; set; } = "BankTransfer";

        public Guid? PrimaryBankAccountId { get; set; }

        public decimal BasicSalary { get; set; } = 0;

        public decimal FixedAllowance { get; set; } = 0;

        public decimal FixedDeduction { get; set; } = 0;

        public bool IsOvertimeEligible { get; set; } = false;

        public bool IsPayrollActive { get; set; } = true;

        [Required]
        public DateTime EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforcePayrollStatusRequest
    {
        public bool IsActive { get; set; }

        public bool IsPayrollActive { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class WorkforceTaxResponse
    {
        public Guid Id { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string? NpwpNumber { get; set; }

        public string TaxStatus { get; set; } = string.Empty;

        public bool IsTaxed { get; set; }

        public string TaxCalculationMethod { get; set; } = string.Empty;

        public DateTime EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public string? Description { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforceTaxListResponse
    {
        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int TotalData { get; set; }

        public int ActiveData { get; set; }

        public int TaxedData { get; set; }

        public List<WorkforceTaxResponse> Items { get; set; } = new();
    }

    public class CreateWorkforceTaxRequest
    {
        [MaxLength(30)]
        public string? NpwpNumber { get; set; }

        [MaxLength(50)]
        public string TaxStatus { get; set; } = "TK0";

        public bool IsTaxed { get; set; } = true;

        [MaxLength(50)]
        public string TaxCalculationMethod { get; set; } = "Gross";

        [Required]
        public DateTime EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceTaxRequest
    {
        [MaxLength(30)]
        public string? NpwpNumber { get; set; }

        [MaxLength(50)]
        public string TaxStatus { get; set; } = "TK0";

        public bool IsTaxed { get; set; } = true;

        [MaxLength(50)]
        public string TaxCalculationMethod { get; set; } = "Gross";

        [Required]
        public DateTime EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceTaxStatusRequest
    {
        public bool IsActive { get; set; }

        public bool IsTaxed { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class WorkforceInsuranceResponse
    {
        public Guid Id { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public bool IsBpjsKesehatanEnabled { get; set; }

        public string? BpjsKesehatanNumber { get; set; }

        public bool IsBpjsKetenagakerjaanEnabled { get; set; }

        public string? BpjsKetenagakerjaanNumber { get; set; }

        public bool IsPrivateInsuranceEnabled { get; set; }

        public string? PrivateInsuranceProvider { get; set; }

        public string? PrivateInsuranceNumber { get; set; }

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public string? Description { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforceInsuranceListResponse
    {
        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int TotalData { get; set; }

        public int ActiveData { get; set; }

        public int BpjsKesehatanData { get; set; }

        public int BpjsKetenagakerjaanData { get; set; }

        public int PrivateInsuranceData { get; set; }

        public List<WorkforceInsuranceResponse> Items { get; set; } = new();
    }

    public class CreateWorkforceInsuranceRequest
    {
        public bool IsBpjsKesehatanEnabled { get; set; } = false;

        [MaxLength(50)]
        public string? BpjsKesehatanNumber { get; set; }

        public bool IsBpjsKetenagakerjaanEnabled { get; set; } = false;

        [MaxLength(50)]
        public string? BpjsKetenagakerjaanNumber { get; set; }

        public bool IsPrivateInsuranceEnabled { get; set; } = false;

        [MaxLength(100)]
        public string? PrivateInsuranceProvider { get; set; }

        [MaxLength(100)]
        public string? PrivateInsuranceNumber { get; set; }

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceInsuranceRequest
    {
        public bool IsBpjsKesehatanEnabled { get; set; } = false;

        [MaxLength(50)]
        public string? BpjsKesehatanNumber { get; set; }

        public bool IsBpjsKetenagakerjaanEnabled { get; set; } = false;

        [MaxLength(50)]
        public string? BpjsKetenagakerjaanNumber { get; set; }

        public bool IsPrivateInsuranceEnabled { get; set; } = false;

        [MaxLength(100)]
        public string? PrivateInsuranceProvider { get; set; }

        [MaxLength(100)]
        public string? PrivateInsuranceNumber { get; set; }

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceInsuranceStatusRequest
    {
        public bool IsActive { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class WorkforceTransportAllowancePolicyResponse
    {
        public Guid Id { get; set; }

        public string PolicyCode { get; set; } = string.Empty;

        public string PolicyName { get; set; } = string.Empty;

        public string AllowanceMode { get; set; } = string.Empty;

        public decimal DefaultMonthlyAmount { get; set; }

        public decimal DefaultDailyAmount { get; set; }

        public decimal DefaultNightAmount { get; set; }

        public TimeOnly? NightStartTime { get; set; }

        public TimeOnly? NightEndTime { get; set; }

        public bool RequireAttendance { get; set; }

        public bool ExcludeIfAbsent { get; set; }

        public bool ExcludeIfLeave { get; set; }

        public bool ExcludeIfHoliday { get; set; }

        public bool IsTaxable { get; set; }

        public bool IsPayrollComponent { get; set; }

        public bool IsActive { get; set; }

        public string? Description { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforceTransportAllowancePolicyListResponse
    {
        public int TotalData { get; set; }

        public int ActiveData { get; set; }

        public int PayrollComponentData { get; set; }

        public int TaxableData { get; set; }

        public List<WorkforceTransportAllowancePolicyResponse> Items { get; set; } = new();
    }

    public class WorkforceTransportAllowancePolicyOptionResponse
    {
        public Guid Id { get; set; }

        public string PolicyCode { get; set; } = string.Empty;

        public string PolicyName { get; set; } = string.Empty;

        public string AllowanceMode { get; set; } = string.Empty;

        public decimal DefaultMonthlyAmount { get; set; }

        public decimal DefaultDailyAmount { get; set; }

        public decimal DefaultNightAmount { get; set; }
    }

    public class CreateWorkforceTransportAllowancePolicyRequest
    {
        [Required]
        [MaxLength(50)]
        public string PolicyCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string PolicyName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string AllowanceMode { get; set; } = "DailyAttendance";

        public decimal DefaultMonthlyAmount { get; set; } = 0;

        public decimal DefaultDailyAmount { get; set; } = 0;

        public decimal DefaultNightAmount { get; set; } = 0;

        public TimeOnly? NightStartTime { get; set; }

        public TimeOnly? NightEndTime { get; set; }

        public bool RequireAttendance { get; set; } = true;

        public bool ExcludeIfAbsent { get; set; } = true;

        public bool ExcludeIfLeave { get; set; } = true;

        public bool ExcludeIfHoliday { get; set; } = false;

        public bool IsTaxable { get; set; } = true;

        public bool IsPayrollComponent { get; set; } = true;

        public bool IsActive { get; set; } = true;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateWorkforceTransportAllowancePolicyRequest
    {
        [Required]
        [MaxLength(50)]
        public string PolicyCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string PolicyName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string AllowanceMode { get; set; } = "DailyAttendance";

        public decimal DefaultMonthlyAmount { get; set; } = 0;

        public decimal DefaultDailyAmount { get; set; } = 0;

        public decimal DefaultNightAmount { get; set; } = 0;

        public TimeOnly? NightStartTime { get; set; }

        public TimeOnly? NightEndTime { get; set; }

        public bool RequireAttendance { get; set; } = true;

        public bool ExcludeIfAbsent { get; set; } = true;

        public bool ExcludeIfLeave { get; set; } = true;

        public bool ExcludeIfHoliday { get; set; } = false;

        public bool IsTaxable { get; set; } = true;

        public bool IsPayrollComponent { get; set; } = true;

        public bool IsActive { get; set; } = true;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateWorkforceTransportAllowancePolicyStatusRequest
    {
        public bool IsActive { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class WorkforceLeaveBalanceResponse
    {
        public Guid Id { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int LeaveYear { get; set; }

        public LeaveType LeaveType { get; set; }

        public decimal OpeningBalance { get; set; }

        public decimal EntitledDays { get; set; }

        public decimal UsedDays { get; set; }

        public decimal PendingDays { get; set; }

        public decimal RemainingDays { get; set; }

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public bool IsActive { get; set; }

        public string? Description { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforceLeaveBalanceListResponse
    {
        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int TotalData { get; set; }

        public int ActiveData { get; set; }

        public decimal TotalEntitledDays { get; set; }

        public decimal TotalUsedDays { get; set; }

        public decimal TotalPendingDays { get; set; }

        public decimal TotalRemainingDays { get; set; }

        public List<WorkforceLeaveBalanceResponse> Items { get; set; } = new();
    }

    public class CreateWorkforceLeaveBalanceRequest
    {
        [Required]
        public int LeaveYear { get; set; }

        [Required]
        public LeaveType LeaveType { get; set; } = LeaveType.AnnualLeave;

        public decimal OpeningBalance { get; set; } = 0;

        public decimal EntitledDays { get; set; } = 0;

        public decimal UsedDays { get; set; } = 0;

        public decimal PendingDays { get; set; } = 0;

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public bool IsActive { get; set; } = true;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateWorkforceLeaveBalanceRequest
    {
        [Required]
        public int LeaveYear { get; set; }

        [Required]
        public LeaveType LeaveType { get; set; } = LeaveType.AnnualLeave;

        public decimal OpeningBalance { get; set; } = 0;

        public decimal EntitledDays { get; set; } = 0;

        public decimal UsedDays { get; set; } = 0;

        public decimal PendingDays { get; set; } = 0;

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public bool IsActive { get; set; } = true;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateWorkforceLeaveBalanceStatusRequest
    {
        public bool IsActive { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class WorkforceLeaveRequestResponse
    {
        public Guid Id { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public Guid? LeaveBalanceId { get; set; }

        public LeaveType LeaveType { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public decimal TotalDays { get; set; }

        public bool IsHalfDay { get; set; }

        public bool IsDeductBalance { get; set; }

        public string Reason { get; set; } = string.Empty;

        public LeaveApprovalStatus ApprovalStatus { get; set; }

        public DateTime RequestedAt { get; set; }

        public Guid? ApprovedByUserId { get; set; }

        public string? ApprovedByUserName { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public string? ApprovalNote { get; set; }

        public Guid? RejectedByUserId { get; set; }

        public string? RejectedByUserName { get; set; }

        public DateTime? RejectedAt { get; set; }

        public string? RejectedReason { get; set; }

        public Guid? CancelledByUserId { get; set; }

        public string? CancelledByUserName { get; set; }

        public DateTime? CancelledAt { get; set; }

        public string? CancelReason { get; set; }

        public string? AttachmentPath { get; set; }

        public string? AttachmentContentType { get; set; }

        public bool HasAttachment { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforceLeaveRequestListResponse
    {
        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int TotalData { get; set; }

        public int PendingData { get; set; }

        public int ApprovedData { get; set; }

        public int RejectedData { get; set; }

        public int CancelledData { get; set; }

        public decimal TotalRequestedDays { get; set; }

        public decimal ApprovedDays { get; set; }

        public decimal PendingDays { get; set; }

        public List<WorkforceLeaveRequestResponse> Items { get; set; } = new();
    }

    public class CreateWorkforceLeaveRequestRequest
    {
        [Required]
        public LeaveType LeaveType { get; set; } = LeaveType.AnnualLeave;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public decimal? TotalDays { get; set; }

        public bool IsHalfDay { get; set; } = false;

        public bool IsDeductBalance { get; set; } = true;

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? AttachmentPath { get; set; }

        [MaxLength(100)]
        public string? AttachmentContentType { get; set; }
    }

    public class ApproveWorkforceLeaveRequestRequest
    {
        [MaxLength(250)]
        public string? ApprovalNote { get; set; }
    }

    public class RejectWorkforceLeaveRequestRequest
    {
        [Required]
        [MaxLength(250)]
        public string RejectedReason { get; set; } = string.Empty;
    }

    public class CancelWorkforceLeaveRequestRequest
    {
        [Required]
        [MaxLength(250)]
        public string CancelReason { get; set; } = string.Empty;
    }

    public class WorkforceOvertimeRequestResponse
    {
        public Guid Id { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public Guid? AttendanceId { get; set; }

        public Guid? WorkScheduleAssignmentId { get; set; }

        public Guid? WorkScheduleId { get; set; }

        public string? WorkScheduleCode { get; set; }

        public string? WorkScheduleName { get; set; }

        public DateTime OvertimeDate { get; set; }

        public TimeOnly? ScheduledStartTime { get; set; }

        public TimeOnly? ScheduledEndTime { get; set; }

        public DateTime? ActualCheckInAt { get; set; }

        public DateTime? ActualCheckOutAt { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }

        public bool IsOvernight { get; set; }

        public int TotalMinutes { get; set; }

        public decimal TotalHours { get; set; }

        public string Reason { get; set; } = string.Empty;

        public OvertimeApprovalStatus ApprovalStatus { get; set; }

        public DateTime RequestedAt { get; set; }

        public Guid? ApprovedByUserId { get; set; }

        public string? ApprovedByUserName { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public string? ApprovalNote { get; set; }

        public Guid? RejectedByUserId { get; set; }

        public string? RejectedByUserName { get; set; }

        public DateTime? RejectedAt { get; set; }

        public string? RejectedReason { get; set; }

        public Guid? CancelledByUserId { get; set; }

        public string? CancelledByUserName { get; set; }

        public DateTime? CancelledAt { get; set; }

        public string? CancelReason { get; set; }

        public bool IsPayrollProcessed { get; set; }

        public DateTime? PayrollProcessedAt { get; set; }

        public Guid? PayrollProcessedByUserId { get; set; }

        public string? PayrollProcessedByUserName { get; set; }

        public string? PayrollPeriodCode { get; set; }

        public string? AttachmentPath { get; set; }

        public string? AttachmentContentType { get; set; }

        public bool HasAttachment { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforceOvertimeRequestListResponse
    {
        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int TotalData { get; set; }

        public int PendingData { get; set; }

        public int ApprovedData { get; set; }

        public int RejectedData { get; set; }

        public int CancelledData { get; set; }

        public int PayrollProcessedData { get; set; }

        public int TotalMinutes { get; set; }

        public int ApprovedMinutes { get; set; }

        public int PendingMinutes { get; set; }

        public List<WorkforceOvertimeRequestResponse> Items { get; set; } = new();
    }

    public class CreateWorkforceOvertimeRequestRequest
    {
        public Guid? AttendanceId { get; set; }

        public Guid? WorkScheduleAssignmentId { get; set; }

        public Guid? WorkScheduleId { get; set; }

        [Required]
        public DateTime OvertimeDate { get; set; }

        public TimeOnly? ScheduledStartTime { get; set; }

        public TimeOnly? ScheduledEndTime { get; set; }

        public DateTime? ActualCheckInAt { get; set; }

        public DateTime? ActualCheckOutAt { get; set; }

        [Required]
        public TimeOnly StartTime { get; set; }

        [Required]
        public TimeOnly EndTime { get; set; }

        public bool IsOvernight { get; set; } = false;

        public int? TotalMinutes { get; set; }

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? AttachmentPath { get; set; }

        [MaxLength(100)]
        public string? AttachmentContentType { get; set; }
    }

    public class UpdateWorkforceOvertimeRequestRequest
    {
        public Guid? AttendanceId { get; set; }

        public Guid? WorkScheduleAssignmentId { get; set; }

        public Guid? WorkScheduleId { get; set; }

        [Required]
        public DateTime OvertimeDate { get; set; }

        public TimeOnly? ScheduledStartTime { get; set; }

        public TimeOnly? ScheduledEndTime { get; set; }

        public DateTime? ActualCheckInAt { get; set; }

        public DateTime? ActualCheckOutAt { get; set; }

        [Required]
        public TimeOnly StartTime { get; set; }

        [Required]
        public TimeOnly EndTime { get; set; }

        public bool IsOvernight { get; set; } = false;

        public int? TotalMinutes { get; set; }

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? AttachmentPath { get; set; }

        [MaxLength(100)]
        public string? AttachmentContentType { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class ApproveWorkforceOvertimeRequestRequest
    {
        [MaxLength(250)]
        public string? ApprovalNote { get; set; }
    }

    public class RejectWorkforceOvertimeRequestRequest
    {
        [Required]
        [MaxLength(250)]
        public string RejectedReason { get; set; } = string.Empty;
    }

    public class CancelWorkforceOvertimeRequestRequest
    {
        [Required]
        [MaxLength(250)]
        public string CancelReason { get; set; } = string.Empty;
    }

    public class UpdateWorkforceOvertimePayrollStatusRequest
    {
        public bool IsPayrollProcessed { get; set; }

        [MaxLength(50)]
        public string? PayrollPeriodCode { get; set; }
    }

    public class WorkforceClinicalPrivilegeResponse
    {
        public Guid Id { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public Guid? CredentialLicenseId { get; set; }

        public string? CredentialLicenseType { get; set; }

        public string? CredentialLicenseNumber { get; set; }

        public Guid? DepartmentId { get; set; }

        public string? DepartmentCode { get; set; }

        public string? DepartmentName { get; set; }

        public Guid? PositionId { get; set; }

        public string? PositionCode { get; set; }

        public string? PositionName { get; set; }

        public string PrivilegeCode { get; set; } = string.Empty;

        public string PrivilegeName { get; set; } = string.Empty;

        public ClinicalPrivilegeType PrivilegeType { get; set; }

        public string? ClinicalScope { get; set; }

        public string? SpecialtyName { get; set; }

        public string? SubSpecialtyName { get; set; }

        public string? ProcedureGroup { get; set; }

        public string? ProcedureName { get; set; }

        public string? PracticeLocation { get; set; }

        public DateTime EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public ClinicalPrivilegeStatus PrivilegeStatus { get; set; }

        public bool IsTemporary { get; set; }

        public bool IsEmergencyPrivilege { get; set; }

        public bool IsSupervisionRequired { get; set; }

        public Guid? SupervisorUserId { get; set; }

        public string? SupervisorUserName { get; set; }

        public Guid? GrantedByUserId { get; set; }

        public string? GrantedByUserName { get; set; }

        public DateTime? GrantedAt { get; set; }

        public string? GrantNote { get; set; }

        public Guid? RejectedByUserId { get; set; }

        public string? RejectedByUserName { get; set; }

        public DateTime? RejectedAt { get; set; }

        public string? RejectedReason { get; set; }

        public Guid? SuspendedByUserId { get; set; }

        public string? SuspendedByUserName { get; set; }

        public DateTime? SuspendedAt { get; set; }

        public string? SuspensionReason { get; set; }

        public Guid? RevokedByUserId { get; set; }

        public string? RevokedByUserName { get; set; }

        public DateTime? RevokedAt { get; set; }

        public string? RevokedReason { get; set; }

        public DateTime? LastReviewDate { get; set; }

        public DateTime? NextReviewDate { get; set; }

        public string? SupportingFilePath { get; set; }

        public string? SupportingFileContentType { get; set; }

        public bool HasSupportingFile { get; set; }

        public bool IsExpired { get; set; }

        public bool IsCurrentlyValid { get; set; }

        public bool IsActive { get; set; }

        public string? Description { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforceClinicalPrivilegeListResponse
    {
        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int TotalData { get; set; }

        public int ActiveData { get; set; }

        public int PendingApprovalData { get; set; }

        public int SuspendedData { get; set; }

        public int RejectedData { get; set; }

        public int RevokedData { get; set; }

        public int ExpiredData { get; set; }

        public int CurrentlyValidData { get; set; }

        public int WithCredentialLicenseData { get; set; }

        public int WithSupportingFileData { get; set; }

        public List<WorkforceClinicalPrivilegeResponse> Items { get; set; } = new();
    }

    public class CreateWorkforceClinicalPrivilegeRequest
    {
        public Guid? CredentialLicenseId { get; set; }

        public Guid? DepartmentId { get; set; }

        public Guid? PositionId { get; set; }

        [Required]
        [MaxLength(100)]
        public string PrivilegeCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string PrivilegeName { get; set; } = string.Empty;

        public ClinicalPrivilegeType PrivilegeType { get; set; } = ClinicalPrivilegeType.CorePrivilege;

        [MaxLength(100)]
        public string? ClinicalScope { get; set; }

        [MaxLength(150)]
        public string? SpecialtyName { get; set; }

        [MaxLength(150)]
        public string? SubSpecialtyName { get; set; }

        [MaxLength(150)]
        public string? ProcedureGroup { get; set; }

        [MaxLength(200)]
        public string? ProcedureName { get; set; }

        [MaxLength(200)]
        public string? PracticeLocation { get; set; }

        [Required]
        public DateTime EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public bool IsTemporary { get; set; } = false;

        public bool IsEmergencyPrivilege { get; set; } = false;

        public bool IsSupervisionRequired { get; set; } = false;

        public Guid? SupervisorUserId { get; set; }

        public DateTime? LastReviewDate { get; set; }

        public DateTime? NextReviewDate { get; set; }

        public IFormFile? SupportingFile { get; set; }

        public bool IsActive { get; set; } = true;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateWorkforceClinicalPrivilegeRequest
    {
        public Guid? CredentialLicenseId { get; set; }

        public Guid? DepartmentId { get; set; }

        public Guid? PositionId { get; set; }

        [Required]
        [MaxLength(100)]
        public string PrivilegeCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string PrivilegeName { get; set; } = string.Empty;

        public ClinicalPrivilegeType PrivilegeType { get; set; } = ClinicalPrivilegeType.CorePrivilege;

        [MaxLength(100)]
        public string? ClinicalScope { get; set; }

        [MaxLength(150)]
        public string? SpecialtyName { get; set; }

        [MaxLength(150)]
        public string? SubSpecialtyName { get; set; }

        [MaxLength(150)]
        public string? ProcedureGroup { get; set; }

        [MaxLength(200)]
        public string? ProcedureName { get; set; }

        [MaxLength(200)]
        public string? PracticeLocation { get; set; }

        [Required]
        public DateTime EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public bool IsTemporary { get; set; } = false;

        public bool IsEmergencyPrivilege { get; set; } = false;

        public bool IsSupervisionRequired { get; set; } = false;

        public Guid? SupervisorUserId { get; set; }

        public DateTime? LastReviewDate { get; set; }

        public DateTime? NextReviewDate { get; set; }

        public IFormFile? SupportingFile { get; set; }

        public bool ReplaceExistingFile { get; set; } = false;

        public bool IsActive { get; set; } = true;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateWorkforceClinicalPrivilegeStatusRequest
    {
        public bool IsActive { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class GrantWorkforceClinicalPrivilegeRequest
    {
        [MaxLength(250)]
        public string? GrantNote { get; set; }

        public DateTime? NextReviewDate { get; set; }
    }

    public class RejectWorkforceClinicalPrivilegeRequest
    {
        [Required]
        [MaxLength(250)]
        public string RejectedReason { get; set; } = string.Empty;
    }

    public class SuspendWorkforceClinicalPrivilegeRequest
    {
        [Required]
        [MaxLength(250)]
        public string SuspensionReason { get; set; } = string.Empty;
    }

    public class RevokeWorkforceClinicalPrivilegeRequest
    {
        [Required]
        [MaxLength(250)]
        public string RevokedReason { get; set; } = string.Empty;
    }

    public class WorkforceHealthRecordResponse
    {
        public Guid Id { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string? RequirementCode { get; set; }

        public HealthRecordType HealthRecordType { get; set; }

        public DateTime RecordDate { get; set; }

        public HealthRecordResultStatus ResultStatus { get; set; }

        public string? ProviderName { get; set; }

        public DateTime? ExpiredDate { get; set; }

        public bool? IsFitToWork { get; set; }

        public string? FitToWorkRestrictionNote { get; set; }

        public bool IsVerified { get; set; }

        public Guid? VerifiedByUserId { get; set; }

        public string? VerifiedByUserName { get; set; }

        public DateTime? VerifiedAt { get; set; }

        public string? VerificationNote { get; set; }

        public string? FilePath { get; set; }

        public string? FileContentType { get; set; }

        public bool HasFile { get; set; }

        public string? Notes { get; set; }

        public bool IsExpired { get; set; }

        public bool IsCurrentlyValid { get; set; }

        public bool IsCompliantForWork { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforceHealthRecordListResponse
    {
        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int TotalData { get; set; }

        public int ActiveData { get; set; }

        public int VerifiedData { get; set; }

        public int ExpiredData { get; set; }

        public int CurrentlyValidData { get; set; }

        public int FitToWorkData { get; set; }

        public int NotFitToWorkData { get; set; }

        public int CompliantForWorkData { get; set; }

        public int WithFileData { get; set; }

        public List<WorkforceHealthRecordResponse> Items { get; set; } = new();
    }

    public class CreateWorkforceHealthRecordRequest
    {
        [MaxLength(100)]
        public string? RequirementCode { get; set; }

        [Required]
        public HealthRecordType HealthRecordType { get; set; } = HealthRecordType.Unknown;

        [Required]
        public DateTime RecordDate { get; set; }

        public HealthRecordResultStatus ResultStatus { get; set; } = HealthRecordResultStatus.Unknown;

        [MaxLength(200)]
        public string? ProviderName { get; set; }

        public DateTime? ExpiredDate { get; set; }

        public bool? IsFitToWork { get; set; }

        [MaxLength(250)]
        public string? FitToWorkRestrictionNote { get; set; }

        public IFormFile? File { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceHealthRecordRequest
    {
        [MaxLength(100)]
        public string? RequirementCode { get; set; }

        [Required]
        public HealthRecordType HealthRecordType { get; set; } = HealthRecordType.Unknown;

        [Required]
        public DateTime RecordDate { get; set; }

        public HealthRecordResultStatus ResultStatus { get; set; } = HealthRecordResultStatus.Unknown;

        [MaxLength(200)]
        public string? ProviderName { get; set; }

        public DateTime? ExpiredDate { get; set; }

        public bool? IsFitToWork { get; set; }

        [MaxLength(250)]
        public string? FitToWorkRestrictionNote { get; set; }

        public IFormFile? File { get; set; }

        public bool ReplaceExistingFile { get; set; } = false;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceHealthRecordStatusRequest
    {
        public bool IsActive { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class VerifyWorkforceHealthRecordRequest
    {
        [MaxLength(250)]
        public string? VerificationNote { get; set; }
    }

    public class UnverifyWorkforceHealthRecordRequest
    {
        [Required]
        [MaxLength(250)]
        public string UnverifyReason { get; set; } = string.Empty;
    }

    public class WorkforceOnboardingChecklistResponse
    {
        public Guid Id { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public UserType UserType { get; set; }

        public OnboardingType OnboardingType { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime TargetCompletionDate { get; set; }

        public DateTime? CompletedDate { get; set; }

        public OnboardingStatus Status { get; set; }

        public Guid? AssignedToUserId { get; set; }

        public string? AssignedToUserName { get; set; }

        public string? Notes { get; set; }

        public bool IsActive { get; set; }

        public int TotalTask { get; set; }

        public int RequiredTask { get; set; }

        public int CompletedTask { get; set; }

        public int PendingTask { get; set; }

        public int RequiredPendingTask { get; set; }

        public decimal CompletionPercentage { get; set; }

        public DateTime CreateDateTime { get; set; }

        public List<WorkforceOnboardingTaskResponse> Tasks { get; set; } = new();
    }

    public class WorkforceOnboardingChecklistListResponse
    {
        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int TotalData { get; set; }

        public int ActiveData { get; set; }

        public int DraftData { get; set; }

        public int InProgressData { get; set; }

        public int CompletedData { get; set; }

        public List<WorkforceOnboardingChecklistResponse> Items { get; set; } = new();
    }

    public class WorkforceOnboardingTaskResponse
    {
        public Guid Id { get; set; }

        public Guid OnboardingChecklistId { get; set; }

        public string TaskCode { get; set; } = string.Empty;

        public string TaskName { get; set; } = string.Empty;

        public OnboardingTaskCategory TaskCategory { get; set; }

        public bool IsRequired { get; set; }

        public bool IsCompleted { get; set; }

        public DateTime? CompletedAt { get; set; }

        public Guid? CompletedByUserId { get; set; }

        public string? CompletedByUserName { get; set; }

        public string? Notes { get; set; }

        public int SortOrder { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class CreateWorkforceOnboardingChecklistRequest
    {
        [Required]
        public OnboardingType OnboardingType { get; set; } = OnboardingType.Unknown;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime TargetCompletionDate { get; set; }

        public Guid? AssignedToUserId { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public bool GenerateDefaultTasks { get; set; } = true;
    }

    public class UpdateWorkforceOnboardingChecklistRequest
    {
        [Required]
        public OnboardingType OnboardingType { get; set; } = OnboardingType.Unknown;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime TargetCompletionDate { get; set; }

        public OnboardingStatus Status { get; set; } = OnboardingStatus.InProgress;

        public Guid? AssignedToUserId { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceOnboardingChecklistStatusRequest
    {
        [Required]
        public OnboardingStatus Status { get; set; }

        public DateTime? CompletedDate { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class CreateWorkforceOnboardingTaskRequest
    {
        [Required]
        [MaxLength(100)]
        public string TaskCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string TaskName { get; set; } = string.Empty;

        public OnboardingTaskCategory TaskCategory { get; set; } = OnboardingTaskCategory.Other;

        public bool IsRequired { get; set; } = true;

        public bool IsCompleted { get; set; } = false;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceOnboardingTaskRequest
    {
        [Required]
        [MaxLength(100)]
        public string TaskCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string TaskName { get; set; } = string.Empty;

        public OnboardingTaskCategory TaskCategory { get; set; } = OnboardingTaskCategory.Other;

        public bool IsRequired { get; set; } = true;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;
    }

    public class CompleteWorkforceOnboardingTaskRequest
    {
        public bool IsCompleted { get; set; } = true;

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class UpdateWorkforceOnboardingTaskStatusRequest
    {
        public bool IsActive { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class WorkforceOffboardingChecklistResponse
    {
        public Guid Id { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public UserType UserType { get; set; }

        public OffboardingType OffboardingType { get; set; }

        public DateTime EffectiveEndDate { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? CompletedDate { get; set; }

        public OffboardingStatus Status { get; set; }

        public string Reason { get; set; } = string.Empty;

        public string? Notes { get; set; }

        public bool IsActive { get; set; }

        public int TotalTask { get; set; }

        public int RequiredTask { get; set; }

        public int CompletedTask { get; set; }

        public int PendingTask { get; set; }

        public int RequiredPendingTask { get; set; }

        public decimal CompletionPercentage { get; set; }

        public DateTime CreateDateTime { get; set; }

        public List<WorkforceOffboardingTaskResponse> Tasks { get; set; } = new();
    }

    public class WorkforceOffboardingChecklistListResponse
    {
        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int TotalData { get; set; }

        public int ActiveData { get; set; }

        public int DraftData { get; set; }

        public int InProgressData { get; set; }

        public int CompletedData { get; set; }

        public int CancelledData { get; set; }

        public List<WorkforceOffboardingChecklistResponse> Items { get; set; } = new();
    }

    public class WorkforceOffboardingTaskResponse
    {
        public Guid Id { get; set; }

        public Guid OffboardingChecklistId { get; set; }

        public string TaskCode { get; set; } = string.Empty;

        public string TaskName { get; set; } = string.Empty;

        public OffboardingTaskCategory TaskCategory { get; set; }

        public bool IsRequired { get; set; }

        public bool IsCompleted { get; set; }

        public DateTime? CompletedAt { get; set; }

        public Guid? CompletedByUserId { get; set; }

        public string? CompletedByUserName { get; set; }

        public string? Notes { get; set; }

        public int SortOrder { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class CreateWorkforceOffboardingChecklistRequest
    {
        [Required]
        public OffboardingType OffboardingType { get; set; } = OffboardingType.Unknown;

        [Required]
        public DateTime EffectiveEndDate { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public bool GenerateDefaultTasks { get; set; } = true;
    }

    public class UpdateWorkforceOffboardingChecklistRequest
    {
        [Required]
        public OffboardingType OffboardingType { get; set; } = OffboardingType.Unknown;

        [Required]
        public DateTime EffectiveEndDate { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public OffboardingStatus Status { get; set; } = OffboardingStatus.InProgress;

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceOffboardingChecklistStatusRequest
    {
        [Required]
        public OffboardingStatus Status { get; set; }

        public DateTime? CompletedDate { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class CreateWorkforceOffboardingTaskRequest
    {
        [Required]
        [MaxLength(100)]
        public string TaskCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string TaskName { get; set; } = string.Empty;

        public OffboardingTaskCategory TaskCategory { get; set; } = OffboardingTaskCategory.Other;

        public bool IsRequired { get; set; } = true;

        public bool IsCompleted { get; set; } = false;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceOffboardingTaskRequest
    {
        [Required]
        [MaxLength(100)]
        public string TaskCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string TaskName { get; set; } = string.Empty;

        public OffboardingTaskCategory TaskCategory { get; set; } = OffboardingTaskCategory.Other;

        public bool IsRequired { get; set; } = true;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;
    }

    public class CompleteWorkforceOffboardingTaskRequest
    {
        public bool IsCompleted { get; set; } = true;

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class UpdateWorkforceOffboardingTaskStatusRequest
    {
        public bool IsActive { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class WorkforceEmploymentHistoryResponse
    {
        public Guid Id { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public UserType UserType { get; set; }

        public EmploymentHistoryType HistoryType { get; set; }

        public Guid? OldDepartmentId { get; set; }

        public string? OldDepartmentCode { get; set; }

        public string? OldDepartmentName { get; set; }

        public Guid? NewDepartmentId { get; set; }

        public string? NewDepartmentCode { get; set; }

        public string? NewDepartmentName { get; set; }

        public Guid? OldPositionId { get; set; }

        public string? OldPositionCode { get; set; }

        public string? OldPositionName { get; set; }

        public Guid? NewPositionId { get; set; }

        public string? NewPositionCode { get; set; }

        public string? NewPositionName { get; set; }

        public string? OldStatus { get; set; }

        public string? NewStatus { get; set; }

        public DateTime EffectiveDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string? Description { get; set; }

        public Guid? ApprovedByUserId { get; set; }

        public string? ApprovedByUserName { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public string? FilePath { get; set; }

        public string? FileContentType { get; set; }

        public bool HasFile { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforceEmploymentHistoryListResponse
    {
        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int TotalData { get; set; }

        public int ActiveData { get; set; }

        public int JoinData { get; set; }

        public int MutationData { get; set; }

        public int PromotionData { get; set; }

        public int StatusChangeData { get; set; }

        public int ResignOrTerminationData { get; set; }

        public List<WorkforceEmploymentHistoryResponse> Items { get; set; } = new();
    }

    public class CreateWorkforceEmploymentHistoryRequest
    {
        [Required]
        public EmploymentHistoryType HistoryType { get; set; } = EmploymentHistoryType.Unknown;

        public Guid? OldDepartmentId { get; set; }

        public Guid? NewDepartmentId { get; set; }

        public Guid? OldPositionId { get; set; }

        public Guid? NewPositionId { get; set; }

        [MaxLength(100)]
        public string? OldStatus { get; set; }

        [MaxLength(100)]
        public string? NewStatus { get; set; }

        [Required]
        public DateTime EffectiveDate { get; set; }

        public DateTime? EndDate { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public Guid? ApprovedByUserId { get; set; }

        public DateTime? ApprovedAt { get; set; }

        [MaxLength(500)]
        public string? FilePath { get; set; }

        [MaxLength(100)]
        public string? FileContentType { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceEmploymentHistoryRequest
    {
        [Required]
        public EmploymentHistoryType HistoryType { get; set; } = EmploymentHistoryType.Unknown;

        public Guid? OldDepartmentId { get; set; }

        public Guid? NewDepartmentId { get; set; }

        public Guid? OldPositionId { get; set; }

        public Guid? NewPositionId { get; set; }

        [MaxLength(100)]
        public string? OldStatus { get; set; }

        [MaxLength(100)]
        public string? NewStatus { get; set; }

        [Required]
        public DateTime EffectiveDate { get; set; }

        public DateTime? EndDate { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public Guid? ApprovedByUserId { get; set; }

        public DateTime? ApprovedAt { get; set; }

        [MaxLength(500)]
        public string? FilePath { get; set; }

        [MaxLength(100)]
        public string? FileContentType { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceEmploymentHistoryStatusRequest
    {
        public bool IsActive { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class WorkforceContractHistoryResponse
    {
        public Guid Id { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public UserType UserType { get; set; }

        public string ContractNumber { get; set; } = string.Empty;

        public ContractHistoryType ContractType { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public ContractHistoryStatus ContractStatus { get; set; }

        public bool IsExpired { get; set; }

        public string? FilePath { get; set; }

        public string? FileContentType { get; set; }

        public bool HasFile { get; set; }

        public string? Notes { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforceContractHistoryListResponse
    {
        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int TotalData { get; set; }

        public int ActiveData { get; set; }

        public int ActiveContractData { get; set; }

        public int ExpiredContractData { get; set; }

        public int TerminatedContractData { get; set; }

        public int DraftContractData { get; set; }

        public List<WorkforceContractHistoryResponse> Items { get; set; } = new();
    }

    public class CreateWorkforceContractHistoryRequest
    {
        [Required]
        [MaxLength(100)]
        public string ContractNumber { get; set; } = string.Empty;

        [Required]
        public ContractHistoryType ContractType { get; set; } = ContractHistoryType.Unknown;

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public ContractHistoryStatus ContractStatus { get; set; } = ContractHistoryStatus.Draft;

        [MaxLength(500)]
        public string? FilePath { get; set; }

        [MaxLength(100)]
        public string? FileContentType { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceContractHistoryRequest
    {
        [Required]
        [MaxLength(100)]
        public string ContractNumber { get; set; } = string.Empty;

        [Required]
        public ContractHistoryType ContractType { get; set; } = ContractHistoryType.Unknown;

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public ContractHistoryStatus ContractStatus { get; set; } = ContractHistoryStatus.Draft;

        [MaxLength(500)]
        public string? FilePath { get; set; }

        [MaxLength(100)]
        public string? FileContentType { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceContractHistoryStatusRequest
    {
        [Required]
        public ContractHistoryStatus ContractStatus { get; set; }

        public bool IsActive { get; set; } = true;

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class WorkforceCompetencyAssessmentResponse
    {
        public Guid Id { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public UserType UserType { get; set; }

        public Guid CompetencyId { get; set; }

        public string CompetencyCode { get; set; } = string.Empty;

        public string CompetencyName { get; set; } = string.Empty;

        public CompetencyCategory CompetencyCategory { get; set; }

        public DateTime AssessmentDate { get; set; }

        public CompetencyLevel CompetencyLevel { get; set; }

        public CompetencyAssessmentResultStatus ResultStatus { get; set; }

        public Guid? AssessedByUserId { get; set; }

        public string? AssessedByUserName { get; set; }

        public DateTime? ExpiredDate { get; set; }

        public bool IsExpired { get; set; }

        public string? FilePath { get; set; }

        public string? FileContentType { get; set; }

        public bool HasFile { get; set; }

        public string? Notes { get; set; }

        public bool IsVerified { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforceCompetencyAssessmentListResponse
    {
        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int TotalData { get; set; }

        public int ActiveData { get; set; }

        public int VerifiedData { get; set; }

        public int PassedData { get; set; }

        public int FailedData { get; set; }

        public int NeedTrainingData { get; set; }

        public int ExpiredData { get; set; }

        public List<WorkforceCompetencyAssessmentResponse> Items { get; set; } = new();
    }

    public class CreateWorkforceCompetencyAssessmentRequest
    {
        [Required]
        public Guid CompetencyId { get; set; }

        [Required]
        public DateTime AssessmentDate { get; set; }

        public CompetencyLevel CompetencyLevel { get; set; } = CompetencyLevel.None;

        public CompetencyAssessmentResultStatus ResultStatus { get; set; } = CompetencyAssessmentResultStatus.NotAssessed;

        public Guid? AssessedByUserId { get; set; }

        public DateTime? ExpiredDate { get; set; }

        [MaxLength(500)]
        public string? FilePath { get; set; }

        [MaxLength(100)]
        public string? FileContentType { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsVerified { get; set; } = false;

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceCompetencyAssessmentRequest
    {
        [Required]
        public Guid CompetencyId { get; set; }

        [Required]
        public DateTime AssessmentDate { get; set; }

        public CompetencyLevel CompetencyLevel { get; set; } = CompetencyLevel.None;

        public CompetencyAssessmentResultStatus ResultStatus { get; set; } = CompetencyAssessmentResultStatus.NotAssessed;

        public Guid? AssessedByUserId { get; set; }

        public DateTime? ExpiredDate { get; set; }

        [MaxLength(500)]
        public string? FilePath { get; set; }

        [MaxLength(100)]
        public string? FileContentType { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsVerified { get; set; } = false;

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceCompetencyAssessmentStatusRequest
    {
        public bool IsActive { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class VerifyWorkforceCompetencyAssessmentRequest
    {
        public bool IsVerified { get; set; } = true;

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class WorkforceCompetencyMatrixItemResponse
    {
        public Guid RequirementId { get; set; }

        public Guid PositionId { get; set; }

        public string PositionName { get; set; } = string.Empty;

        public Guid CompetencyId { get; set; }

        public string CompetencyCode { get; set; } = string.Empty;

        public string CompetencyName { get; set; } = string.Empty;

        public CompetencyCategory CompetencyCategory { get; set; }

        public bool IsRequired { get; set; }

        public CompetencyLevel MinimumLevel { get; set; }

        public bool IsCertificationRequired { get; set; }

        public bool IsTrainingRequired { get; set; }

        public Guid? LatestAssessmentId { get; set; }

        public CompetencyLevel? LatestCompetencyLevel { get; set; }

        public CompetencyAssessmentResultStatus? LatestResultStatus { get; set; }

        public DateTime? LatestAssessmentDate { get; set; }

        public DateTime? ExpiredDate { get; set; }

        public bool IsVerified { get; set; }

        public bool IsExpired { get; set; }

        public bool IsPassed { get; set; }

        public bool IsLevelMet { get; set; }

        public string Status { get; set; } = "Missing";
    }

    public class WorkforceCompetencyMatrixResponse
    {
        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public UserType UserType { get; set; }

        public Guid? PrimaryPositionId { get; set; }

        public string? PrimaryPositionName { get; set; }

        public int TotalRequirement { get; set; }

        public int CompletedRequirement { get; set; }

        public int MissingRequirement { get; set; }

        public int NeedTrainingRequirement { get; set; }

        public int ExpiredRequirement { get; set; }

        public List<WorkforceCompetencyMatrixItemResponse> Items { get; set; } = new();
    }

    public class WorkforcePerformanceReviewResponse
    {
        public Guid Id { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public UserType UserType { get; set; }

        public string ReviewPeriod { get; set; } = string.Empty;

        public DateTime ReviewDate { get; set; }

        public Guid ReviewerUserId { get; set; }

        public string? ReviewerUserName { get; set; }

        public PerformanceReviewType ReviewType { get; set; }

        public decimal TotalScore { get; set; }

        public PerformanceFinalRating FinalRating { get; set; }

        public PerformanceReviewStatus ReviewStatus { get; set; }

        public string? StrengthNotes { get; set; }

        public string? ImprovementNotes { get; set; }

        public string? RecommendationNotes { get; set; }

        public bool IsFinalized { get; set; }

        public DateTime? FinalizedAt { get; set; }

        public bool IsActive { get; set; }

        public int DetailCount { get; set; }

        public DateTime CreateDateTime { get; set; }

        public List<WorkforcePerformanceReviewDetailResponse> Details { get; set; } = new();
    }

    public class WorkforcePerformanceReviewListResponse
    {
        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int TotalData { get; set; }

        public int ActiveData { get; set; }

        public int DraftData { get; set; }

        public int InProgressData { get; set; }

        public int CompletedData { get; set; }

        public int FinalizedData { get; set; }

        public decimal AverageScore { get; set; }

        public List<WorkforcePerformanceReviewResponse> Items { get; set; } = new();
    }

    public class WorkforcePerformanceReviewDetailResponse
    {
        public Guid Id { get; set; }

        public Guid PerformanceReviewId { get; set; }

        public string CriteriaCode { get; set; } = string.Empty;

        public string CriteriaName { get; set; } = string.Empty;

        public decimal Score { get; set; }

        public decimal Weight { get; set; }

        public decimal WeightedScore { get; set; }

        public string? Notes { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class CreateWorkforcePerformanceReviewRequest
    {
        [Required]
        [MaxLength(50)]
        public string ReviewPeriod { get; set; } = string.Empty;

        [Required]
        public DateTime ReviewDate { get; set; }

        [Required]
        public Guid ReviewerUserId { get; set; }

        public PerformanceReviewType ReviewType { get; set; } = PerformanceReviewType.Unknown;

        public PerformanceFinalRating FinalRating { get; set; } = PerformanceFinalRating.NotRated;

        public PerformanceReviewStatus ReviewStatus { get; set; } = PerformanceReviewStatus.Draft;

        [MaxLength(1000)]
        public string? StrengthNotes { get; set; }

        [MaxLength(1000)]
        public string? ImprovementNotes { get; set; }

        [MaxLength(1000)]
        public string? RecommendationNotes { get; set; }

        public bool IsActive { get; set; } = true;

        public List<CreateWorkforcePerformanceReviewDetailRequest> Details { get; set; } = new();
    }

    public class UpdateWorkforcePerformanceReviewRequest
    {
        [Required]
        [MaxLength(50)]
        public string ReviewPeriod { get; set; } = string.Empty;

        [Required]
        public DateTime ReviewDate { get; set; }

        [Required]
        public Guid ReviewerUserId { get; set; }

        public PerformanceReviewType ReviewType { get; set; } = PerformanceReviewType.Unknown;

        public PerformanceFinalRating FinalRating { get; set; } = PerformanceFinalRating.NotRated;

        public PerformanceReviewStatus ReviewStatus { get; set; } = PerformanceReviewStatus.Draft;

        [MaxLength(1000)]
        public string? StrengthNotes { get; set; }

        [MaxLength(1000)]
        public string? ImprovementNotes { get; set; }

        [MaxLength(1000)]
        public string? RecommendationNotes { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforcePerformanceReviewStatusRequest
    {
        public PerformanceReviewStatus ReviewStatus { get; set; }

        public bool IsActive { get; set; } = true;

        [MaxLength(1000)]
        public string? RecommendationNotes { get; set; }
    }

    public class FinalizeWorkforcePerformanceReviewRequest
    {
        public PerformanceFinalRating FinalRating { get; set; } = PerformanceFinalRating.NotRated;

        [MaxLength(1000)]
        public string? RecommendationNotes { get; set; }
    }

    public class CreateWorkforcePerformanceReviewDetailRequest
    {
        [Required]
        [MaxLength(100)]
        public string CriteriaCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string CriteriaName { get; set; } = string.Empty;

        public decimal Score { get; set; } = 0;

        public decimal Weight { get; set; } = 0;

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class UpdateWorkforcePerformanceReviewDetailRequest
    {
        [Required]
        [MaxLength(100)]
        public string CriteriaCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string CriteriaName { get; set; } = string.Empty;

        public decimal Score { get; set; } = 0;

        public decimal Weight { get; set; } = 0;

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class WorkforceDisciplinaryActionResponse
    {
        public Guid Id { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public UserType UserType { get; set; }

        public DisciplinaryActionType ActionType { get; set; }

        public DateTime IncidentDate { get; set; }

        public DateTime IssuedDate { get; set; }

        public DisciplinarySeverityLevel SeverityLevel { get; set; }

        public string Reason { get; set; } = string.Empty;

        public string? Description { get; set; }

        public Guid IssuedByUserId { get; set; }

        public string? IssuedByUserName { get; set; }

        public DateTime? EffectiveUntil { get; set; }

        public string? FilePath { get; set; }

        public bool HasFile { get; set; }

        public DisciplinaryActionStatus ActionStatus { get; set; }

        public string? Notes { get; set; }

        public bool IsExpired { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforceDisciplinaryActionListResponse
    {
        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int TotalData { get; set; }

        public int ActiveData { get; set; }

        public int DraftData { get; set; }

        public int IssuedData { get; set; }

        public int AcknowledgedData { get; set; }

        public int UnderReviewData { get; set; }

        public int ResolvedData { get; set; }

        public int CancelledData { get; set; }

        public int ExpiredData { get; set; }

        public int HighSeverityData { get; set; }

        public int CriticalSeverityData { get; set; }

        public int WithFileData { get; set; }

        public List<WorkforceDisciplinaryActionResponse> Items { get; set; } = new();
    }

    public class CreateWorkforceDisciplinaryActionRequest
    {
        public DisciplinaryActionType ActionType { get; set; } = DisciplinaryActionType.Unknown;

        [Required]
        public DateTime IncidentDate { get; set; }

        [Required]
        public DateTime IssuedDate { get; set; }

        public DisciplinarySeverityLevel SeverityLevel { get; set; } = DisciplinarySeverityLevel.Low;

        [Required]
        [MaxLength(250)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public Guid IssuedByUserId { get; set; }

        public DateTime? EffectiveUntil { get; set; }

        public IFormFile? File { get; set; }

        public DisciplinaryActionStatus ActionStatus { get; set; } = DisciplinaryActionStatus.Draft;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceDisciplinaryActionRequest
    {
        public DisciplinaryActionType ActionType { get; set; } = DisciplinaryActionType.Unknown;

        [Required]
        public DateTime IncidentDate { get; set; }

        [Required]
        public DateTime IssuedDate { get; set; }

        public DisciplinarySeverityLevel SeverityLevel { get; set; } = DisciplinarySeverityLevel.Low;

        [Required]
        [MaxLength(250)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public Guid IssuedByUserId { get; set; }

        public DateTime? EffectiveUntil { get; set; }

        public IFormFile? File { get; set; }

        public bool ReplaceExistingFile { get; set; } = false;

        public DisciplinaryActionStatus ActionStatus { get; set; } = DisciplinaryActionStatus.Draft;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceDisciplinaryActionStatusRequest
    {
        public DisciplinaryActionStatus ActionStatus { get; set; } = DisciplinaryActionStatus.Issued;

        public bool IsActive { get; set; } = true;

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class WorkforceComplianceAlertResponse
    {
        public Guid Id { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public UserType UserType { get; set; }

        public string SourceEntityName { get; set; } = string.Empty;

        public Guid SourceEntityId { get; set; }

        public string SourceDisplayName { get; set; } = string.Empty;

        public ComplianceAlertType AlertType { get; set; }

        public string AlertTitle { get; set; } = string.Empty;

        public string AlertMessage { get; set; } = string.Empty;

        public DateTime DueDate { get; set; }

        public int DaysRemaining { get; set; }

        public bool IsOverdue { get; set; }

        public ComplianceAlertStatus AlertStatus { get; set; }

        public ComplianceAlertSeverityLevel SeverityLevel { get; set; }

        public bool IsResolved { get; set; }

        public DateTime? ResolvedAt { get; set; }

        public Guid? ResolvedByUserId { get; set; }

        public string? ResolvedByUserName { get; set; }

        public string? Notes { get; set; }

        public bool IsActive { get; set; }

        public int LogCount { get; set; }

        public DateTime CreateDateTime { get; set; }

        public List<WorkforceComplianceAlertLogResponse> Logs { get; set; } = new();
    }

    public class WorkforceComplianceAlertLogResponse
    {
        public Guid Id { get; set; }

        public Guid ComplianceAlertId { get; set; }

        public ComplianceAlertLogType LogType { get; set; }

        public ComplianceAlertStatus? OldStatus { get; set; }

        public ComplianceAlertStatus? NewStatus { get; set; }

        public string? LogMessage { get; set; }

        public Guid? PerformedByUserId { get; set; }

        public string? PerformedByUserName { get; set; }

        public DateTime PerformedAt { get; set; }

        public string? Notes { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforceComplianceAlertListResponse
    {
        public int TotalData { get; set; }

        public int ActiveData { get; set; }

        public int OpenData { get; set; }

        public int InProgressData { get; set; }

        public int ResolvedData { get; set; }

        public int IgnoredData { get; set; }

        public int CancelledData { get; set; }

        public int ExpiredData { get; set; }

        public int OverdueData { get; set; }

        public int DueTodayData { get; set; }

        public int DueInSevenDaysData { get; set; }

        public int DueInThirtyDaysData { get; set; }

        public int CriticalData { get; set; }

        public int HighData { get; set; }

        public List<WorkforceComplianceAlertResponse> Items { get; set; } = new();
    }

    public class WorkforceComplianceAlertSummaryResponse
    {
        public int TotalAlert { get; set; }

        public int OpenAlert { get; set; }

        public int InProgressAlert { get; set; }

        public int ResolvedAlert { get; set; }

        public int OverdueAlert { get; set; }

        public int DueTodayAlert { get; set; }

        public int DueInSevenDaysAlert { get; set; }

        public int DueInThirtyDaysAlert { get; set; }

        public int CriticalAlert { get; set; }

        public int HighAlert { get; set; }

        public int DocumentAlert { get; set; }

        public int LicenseAlert { get; set; }

        public int CertificationAlert { get; set; }

        public int HealthRecordAlert { get; set; }

        public int ContractAlert { get; set; }

        public int ClinicalPrivilegeAlert { get; set; }

        public int ExternalAccessAlert { get; set; }
    }

    public class CreateWorkforceComplianceAlertRequest
    {
        [Required]
        public Guid WorkforceProfileId { get; set; }

        [Required]
        [MaxLength(100)]
        public string SourceEntityName { get; set; } = string.Empty;

        [Required]
        public Guid SourceEntityId { get; set; }

        public ComplianceAlertType AlertType { get; set; } = ComplianceAlertType.Unknown;

        [Required]
        [MaxLength(200)]
        public string AlertTitle { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string AlertMessage { get; set; } = string.Empty;

        [Required]
        public DateTime DueDate { get; set; }

        public ComplianceAlertStatus AlertStatus { get; set; } = ComplianceAlertStatus.Open;

        public ComplianceAlertSeverityLevel SeverityLevel { get; set; } = ComplianceAlertSeverityLevel.Low;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceComplianceAlertRequest
    {
        public ComplianceAlertType AlertType { get; set; } = ComplianceAlertType.Unknown;

        [Required]
        [MaxLength(200)]
        public string AlertTitle { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string AlertMessage { get; set; } = string.Empty;

        [Required]
        public DateTime DueDate { get; set; }

        public ComplianceAlertStatus AlertStatus { get; set; } = ComplianceAlertStatus.Open;

        public ComplianceAlertSeverityLevel SeverityLevel { get; set; } = ComplianceAlertSeverityLevel.Low;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceComplianceAlertStatusRequest
    {
        public ComplianceAlertStatus AlertStatus { get; set; } = ComplianceAlertStatus.Open;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class ResolveWorkforceComplianceAlertRequest
    {
        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class ReopenWorkforceComplianceAlertRequest
    {
        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class AddWorkforceComplianceAlertLogRequest
    {
        public ComplianceAlertLogType LogType { get; set; } = ComplianceAlertLogType.NoteAdded;

        [MaxLength(1000)]
        public string? LogMessage { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class GenerateWorkforceComplianceAlertRequest
    {
        public Guid? WorkforceProfileId { get; set; }

        [Range(0, 365)]
        public int DaysBeforeDue { get; set; } = 30;

        public bool IncludeExpired { get; set; } = true;

        public bool IncludeWillExpire { get; set; } = true;

        public bool IncludeDocument { get; set; } = true;

        public bool IncludeCertification { get; set; } = true;

        public bool IncludeCredentialLicense { get; set; } = true;

        public bool IncludeClinicalPrivilege { get; set; } = true;

        public bool IncludeHealthRecord { get; set; } = true;

        public bool IncludeEmployeeContract { get; set; } = true;

        public bool IncludeDoctorContract { get; set; } = true;

        public bool IncludeExternalAccess { get; set; } = true;

        public bool IncludeContractHistory { get; set; } = true;
    }

    public class GenerateWorkforceComplianceAlertResponse
    {
        public int DaysBeforeDue { get; set; }

        public Guid? WorkforceProfileId { get; set; }

        public int CandidateData { get; set; }

        public int CreatedData { get; set; }

        public int SkippedDuplicateData { get; set; }

        public int SkippedResolvedData { get; set; }

        public int DocumentCreatedData { get; set; }

        public int CertificationCreatedData { get; set; }

        public int CredentialLicenseCreatedData { get; set; }

        public int ClinicalPrivilegeCreatedData { get; set; }

        public int HealthRecordCreatedData { get; set; }

        public int EmployeeContractCreatedData { get; set; }

        public int DoctorContractCreatedData { get; set; }

        public int ExternalAccessCreatedData { get; set; }

        public int ContractHistoryCreatedData { get; set; }

        public List<WorkforceComplianceAlertResponse> CreatedItems { get; set; } = new();
    }

    public class WorkforceScheduleChangeRequestResponse
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string ProfileCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public UserType UserType { get; set; }

        public Guid? CurrentWorkScheduleAssignmentId { get; set; }
        public DateTime? CurrentScheduleDate { get; set; }
        public Guid? CurrentWorkScheduleId { get; set; }
        public string? CurrentWorkScheduleCode { get; set; }
        public string? CurrentWorkScheduleName { get; set; }

        public DateTime RequestedScheduleDate { get; set; }
        public Guid? RequestedWorkScheduleId { get; set; }
        public string? RequestedWorkScheduleCode { get; set; }
        public string? RequestedWorkScheduleName { get; set; }

        public ScheduleChangeRequestType RequestType { get; set; }
        public string Reason { get; set; } = string.Empty;
        public ScheduleRequestApprovalStatus ApprovalStatus { get; set; }
        public DateTime RequestedAt { get; set; }

        public Guid? ApprovedByUserId { get; set; }
        public string? ApprovedByUserName { get; set; }
        public DateTime? ApprovedAt { get; set; }

        public Guid? RejectedByUserId { get; set; }
        public string? RejectedByUserName { get; set; }
        public DateTime? RejectedAt { get; set; }
        public string? RejectedReason { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforceScheduleChangeRequestListResponse
    {
        public Guid WorkforceProfileId { get; set; }
        public string ProfileCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int TotalData { get; set; }
        public int ActiveData { get; set; }
        public int PendingData { get; set; }
        public int ApprovedData { get; set; }
        public int RejectedData { get; set; }
        public int CancelledData { get; set; }
        public List<WorkforceScheduleChangeRequestResponse> Items { get; set; } = new();
    }

    public class CreateWorkforceScheduleChangeRequest
    {
        public Guid? CurrentWorkScheduleAssignmentId { get; set; }

        [Required]
        public DateTime RequestedScheduleDate { get; set; }

        public Guid? RequestedWorkScheduleId { get; set; }

        [Required]
        public ScheduleChangeRequestType RequestType { get; set; }

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceScheduleChangeRequest
    {
        public Guid? CurrentWorkScheduleAssignmentId { get; set; }

        [Required]
        public DateTime RequestedScheduleDate { get; set; }

        public Guid? RequestedWorkScheduleId { get; set; }

        [Required]
        public ScheduleChangeRequestType RequestType { get; set; }

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceScheduleChangeStatusRequest
    {
        [Required]
        public ScheduleRequestApprovalStatus ApprovalStatus { get; set; }

        [MaxLength(250)]
        public string? RejectedReason { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class ApproveWorkforceScheduleChangeRequest
    {
        public bool ApplyToScheduleAssignment { get; set; } = false;

        [MaxLength(250)]
        public string? Notes { get; set; }
    }

    public class RejectWorkforceScheduleChangeRequest
    {
        [Required]
        [MaxLength(250)]
        public string RejectedReason { get; set; } = string.Empty;
    }

    public class CancelWorkforceScheduleChangeRequest
    {
        [MaxLength(250)]
        public string? CancelReason { get; set; }
    }

    public class WorkforceShiftSwapRequestResponse
    {
        public Guid Id { get; set; }

        public Guid RequesterWorkforceProfileId { get; set; }
        public string RequesterProfileCode { get; set; } = string.Empty;
        public string RequesterDisplayName { get; set; } = string.Empty;
        public UserType RequesterUserType { get; set; }

        public Guid TargetWorkforceProfileId { get; set; }
        public string TargetProfileCode { get; set; } = string.Empty;
        public string TargetDisplayName { get; set; } = string.Empty;
        public UserType TargetUserType { get; set; }

        public Guid RequesterScheduleAssignmentId { get; set; }
        public DateTime? RequesterScheduleDate { get; set; }
        public Guid? RequesterWorkScheduleId { get; set; }
        public string? RequesterWorkScheduleCode { get; set; }
        public string? RequesterWorkScheduleName { get; set; }

        public Guid TargetScheduleAssignmentId { get; set; }
        public DateTime? TargetScheduleDate { get; set; }
        public Guid? TargetWorkScheduleId { get; set; }
        public string? TargetWorkScheduleCode { get; set; }
        public string? TargetWorkScheduleName { get; set; }

        public string Reason { get; set; } = string.Empty;
        public ScheduleRequestApprovalStatus ApprovalStatus { get; set; }
        public DateTime RequestedAt { get; set; }

        public Guid? ApprovedByUserId { get; set; }
        public string? ApprovedByUserName { get; set; }
        public DateTime? ApprovedAt { get; set; }

        public Guid? RejectedByUserId { get; set; }
        public string? RejectedByUserName { get; set; }
        public DateTime? RejectedAt { get; set; }
        public string? RejectedReason { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforceShiftSwapRequestListResponse
    {
        public int TotalData { get; set; }
        public int ActiveData { get; set; }
        public int PendingData { get; set; }
        public int ApprovedData { get; set; }
        public int RejectedData { get; set; }
        public int CancelledData { get; set; }
        public List<WorkforceShiftSwapRequestResponse> Items { get; set; } = new();
    }

    public class CreateWorkforceShiftSwapRequest
    {
        [Required]
        public Guid RequesterWorkforceProfileId { get; set; }

        [Required]
        public Guid TargetWorkforceProfileId { get; set; }

        [Required]
        public Guid RequesterScheduleAssignmentId { get; set; }

        [Required]
        public Guid TargetScheduleAssignmentId { get; set; }

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceShiftSwapRequest
    {
        [Required]
        public Guid RequesterWorkforceProfileId { get; set; }

        [Required]
        public Guid TargetWorkforceProfileId { get; set; }

        [Required]
        public Guid RequesterScheduleAssignmentId { get; set; }

        [Required]
        public Guid TargetScheduleAssignmentId { get; set; }

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceShiftSwapStatusRequest
    {
        [Required]
        public ScheduleRequestApprovalStatus ApprovalStatus { get; set; }

        [MaxLength(250)]
        public string? RejectedReason { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class ApproveWorkforceShiftSwapRequest
    {
        public bool ApplyToScheduleAssignment { get; set; } = false;

        [MaxLength(250)]
        public string? Notes { get; set; }
    }

    public class RejectWorkforceShiftSwapRequest
    {
        [Required]
        [MaxLength(250)]
        public string RejectedReason { get; set; } = string.Empty;
    }

    public class CancelWorkforceShiftSwapRequest
    {
        [MaxLength(250)]
        public string? CancelReason { get; set; }
    }

}