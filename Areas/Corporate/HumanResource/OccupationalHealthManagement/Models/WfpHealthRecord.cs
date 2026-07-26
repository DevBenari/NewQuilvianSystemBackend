using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Models
{
    [Table("WfpHealthRecord", Schema = "public")]
    public class WfpHealthRecord : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid WorkforceProfileId { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? DoctorId { get; set; }

        [Required]
        [MaxLength(60)]
        public string RecordCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string RecordType { get; set; } = "General";

        public DateTime RecordDate { get; set; }

        [MaxLength(200)]
        public string? ProviderName { get; set; }

        [MaxLength(40)]
        public string? AdministrativeResultStatus { get; set; }

        [MaxLength(1500)]
        public string? AdministrativeSummary { get; set; }

        [MaxLength(4000)]
        public string? ClinicalSummaryRestricted { get; set; }

        [Required]
        [MaxLength(30)]
        public string AccessClassification { get; set; } = "Restricted";

        public bool IsSensitive { get; set; } = true;
        public bool? IsFitToWork { get; set; }
        public bool WorkRestrictionRequired { get; set; } = false;

        public DateTime? ReminderDate { get; set; }
        public DateTime? ExpiredDate { get; set; }

        [MaxLength(500)]
        public string? FilePath { get; set; }

        [MaxLength(100)]
        public string? FileContentType { get; set; }

        public bool IsVerified { get; set; } = false;
        public Guid? VerifiedByUserId { get; set; }
        public DateTime? VerifiedAt { get; set; }

        public bool IsActive { get; set; } = true;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public MstDoctor? Doctor { get; set; }
        public ApplicationUser? VerifiedByUser { get; set; }

        public ICollection<TrxEmployeeMedicalExamination> MedicalExaminations { get; set; } = new List<TrxEmployeeMedicalExamination>();
        public ICollection<TrxEmployeeFitnessToWork> FitnessAssessments { get; set; } = new List<TrxEmployeeFitnessToWork>();
    }
}
