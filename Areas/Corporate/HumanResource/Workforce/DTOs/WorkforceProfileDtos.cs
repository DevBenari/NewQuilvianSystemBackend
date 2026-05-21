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
}