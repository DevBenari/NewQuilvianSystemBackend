using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs
{
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
}
