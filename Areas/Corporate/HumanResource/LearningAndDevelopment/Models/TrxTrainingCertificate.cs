using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Performance.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Models
{
    [Table("TrxTrainingCertificate", Schema = "public")]
    public class TrxTrainingCertificate : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid TrainingParticipantId { get; set; }

        public Guid TrainingResultId { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public Guid? CertificationTypeId { get; set; }

        public Guid? TrainingRecordId { get; set; }

        [Required]
        [MaxLength(150)]
        public string CertificateNumber { get; set; } = string.Empty;

        public DateTime IssuedDate { get; set; }

        public DateTime? ExpiredDate { get; set; }

        [MaxLength(250)]
        public string? IssuerName { get; set; }

        [MaxLength(1000)]
        public string? CertificateFilePath { get; set; }

        [MaxLength(150)]
        public string? FileContentType { get; set; }

        [MaxLength(150)]
        public string? FileChecksum { get; set; }

        [MaxLength(1000)]
        public string? VerificationUrl { get; set; }

        public bool IsVerified { get; set; } = false;

        public Guid? VerifiedByUserId { get; set; }

        public DateTime? VerifiedAt { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxTrainingParticipant? TrainingParticipant { get; set; }
        public TrxTrainingResult? TrainingResult { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstCertificationType? CertificationType { get; set; }
        public WfpTrainingRecord? TrainingRecord { get; set; }
        public ApplicationUser? VerifiedByUser { get; set; }
    }
}
