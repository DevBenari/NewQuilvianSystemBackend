using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models
{
    [Table("WfpTrainingRecord", Schema = "public")]
    public class WfpTrainingRecord : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        [MaxLength(100)]
        public string? RequirementCode { get; set; }

        [Required]
        [MaxLength(100)]
        public string TrainingType { get; set; } = string.Empty;
        // Seminar, Workshop, Course, Webinar, InHouseTraining

        [Required]
        [MaxLength(200)]
        public string TrainingName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Organizer { get; set; }

        [MaxLength(200)]
        public string? Location { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [MaxLength(100)]
        public string? CertificateNumber { get; set; }

        public decimal CreditPoint { get; set; } = 0;
        // SKP / credit point

        [MaxLength(500)]
        public string? FilePath { get; set; }

        [MaxLength(100)]
        public string? FileContentType { get; set; }

        public bool IsVerified { get; set; } = false;

        public bool IsActive { get; set; } = true;

        [MaxLength(250)]
        public string? Description { get; set; }

        public MstWorkforceProfile? WorkforceProfile { get; set; }
    }
}