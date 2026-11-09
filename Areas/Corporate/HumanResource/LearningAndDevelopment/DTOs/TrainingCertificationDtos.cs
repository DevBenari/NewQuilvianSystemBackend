using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.DTOs
{
    public class IssueCertificateRequestDto
    {
        [Required(ErrorMessage = "TrainingParticipantId wajib diisi.")]
        public Guid TrainingParticipantId { get; set; }

        public Guid? CertificationTypeId { get; set; }

        public int ValidityYears { get; set; } = 3;

        [MaxLength(250)]
        public string? IssuerName { get; set; } = "RS Quilvian Learning & Development Center";

        [MaxLength(1000)]
        public string? CertificateFilePath { get; set; }

        public decimal CreditPointEarned { get; set; } = 2.0m;

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public class TrainingCertificateDto
    {
        public Guid Id { get; set; }
        public Guid TrainingParticipantId { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string? StaffName { get; set; }
        public string TrainingSessionName { get; set; } = string.Empty;
        public string CertificateNumber { get; set; } = string.Empty;
        public DateTime IssuedDate { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public string? IssuerName { get; set; }
        public string? CertificateFilePath { get; set; }
        public string? FileChecksum { get; set; }
        public string? VerificationUrl { get; set; }
        public bool IsVerified { get; set; }
        public Guid? TrainingRecordId { get; set; }
        public bool IsSyncedToEdossier { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
