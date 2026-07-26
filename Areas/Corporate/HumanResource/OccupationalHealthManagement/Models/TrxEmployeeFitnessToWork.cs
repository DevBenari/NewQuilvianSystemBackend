using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Models
{
    [Table("TrxEmployeeFitnessToWork", Schema = "public")]
    public class TrxEmployeeFitnessToWork : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid WorkforceProfileId { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? HealthRecordId { get; set; }
        public Guid? MedicalExaminationId { get; set; }

        [Required]
        [MaxLength(60)]
        public string AssessmentNumber { get; set; } = string.Empty;

        public DateTime AssessmentDate { get; set; }

        [Required]
        [MaxLength(40)]
        public string FitnessStatus { get; set; } = "ReviewRequired";

        public DateTime EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        public bool WorkRestrictionRequired { get; set; } = false;

        [MaxLength(1500)]
        public string? AdministrativeRestrictionSummary { get; set; }

        [MaxLength(4000)]
        public string? ClinicalBasisRestricted { get; set; }

        public bool IsSchedulingAllowed { get; set; } = false;
        public bool IsClinicalDutyAllowed { get; set; } = false;
        public DateTime? ReviewDate { get; set; }

        [MaxLength(200)]
        public string? AssessedByProvider { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public DateTime? ApprovedAt { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public WfpHealthRecord? HealthRecord { get; set; }
        public TrxEmployeeMedicalExamination? MedicalExamination { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }

        public ICollection<TrxWorkRestriction> WorkRestrictions { get; set; } = new List<TrxWorkRestriction>();
        public ICollection<TrxEmployeeInjury> RelatedInjuries { get; set; } = new List<TrxEmployeeInjury>();
    }
}
