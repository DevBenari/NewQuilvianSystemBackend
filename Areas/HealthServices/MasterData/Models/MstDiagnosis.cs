using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    [Table("MstDiagnosis", Schema = "public")]
    public class MstDiagnosis : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? DiagnosisChapterId { get; set; }

        public Guid? ParentDiagnosisId { get; set; }
        // Opsional untuk grouping diagnosis, contoh A00 sebagai parent A00.0

        [Required]
        [MaxLength(50)]
        public string DiagnosisCode { get; set; } = string.Empty;
        // Contoh: A00, A00.0, I10, E11.9

        [Required]
        [MaxLength(500)]
        public string DiagnosisName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? DiagnosisNameEnglish { get; set; }

        [MaxLength(200)]
        public string? ShortName { get; set; }

        [MaxLength(300)]
        public string? DiagnosisGroupName { get; set; }

        [MaxLength(300)]
        public string? DiagnosisCategoryName { get; set; }

        [Required]
        [MaxLength(50)]
        public string DiagnosisType { get; set; } = "ICD10";
        // ICD10, Local, Custom

        [MaxLength(100)]
        public string IcdVersion { get; set; } = "ICD-10";

        public bool IsSelectableForClinicalUse { get; set; } = true;
        // Jika false, kode hanya untuk group/category, bukan dipilih dokter.

        public bool IsBillable { get; set; } = true;
        // Untuk kebutuhan billing/claim/reporting.

        public bool IsPrimaryDiagnosisAllowed { get; set; } = true;

        public bool IsSecondaryDiagnosisAllowed { get; set; } = true;

        public bool IsChronicDisease { get; set; } = false;

        public bool IsInfectiousDisease { get; set; } = false;

        public bool IsExternalCause { get; set; } = false;

        public bool IsPregnancyRelated { get; set; } = false;

        public bool IsMentalHealthRelated { get; set; } = false;

        public bool IsRareDisease { get; set; } = false;

        [MaxLength(50)]
        public string? GenderRestriction { get; set; }
        // None, Male, Female

        public int? MinimumAgeYear { get; set; }

        public int? MaximumAgeYear { get; set; }

        [MaxLength(50)]
        public string? ExternalDiagnosisCode { get; set; }

        [MaxLength(50)]
        public string? IntegrationCode { get; set; }

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstDiagnosisChapter? DiagnosisChapter { get; set; }

        public MstDiagnosis? ParentDiagnosis { get; set; }

        public ICollection<MstDiagnosis> ChildDiagnoses { get; set; } = new List<MstDiagnosis>();
    }
}