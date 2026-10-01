using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    /// <summary>
    /// Standar Luaran Keperawatan Indonesia (SLKI) resmi PPNI.
    /// Menentukan kriteria evaluasi dan target hasil asuhan yang diharapkan.
    /// </summary>
    [Table("MstNursingDiagnosisOutcome", Schema = "public")]
    public class MstNursingDiagnosisOutcome : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid NursingDiagnosisId { get; set; }

        [Required]
        [MaxLength(50)]
        public string OutcomeCode { get; set; } = string.Empty; // misal: L.08066

        [Required]
        [MaxLength(300)]
        public string OutcomeName { get; set; } = string.Empty; // misal: Tingkat Nyeri Menurun

        public string? Expectation { get; set; } // misal: Keluhan nyeri menurun, meringis menurun, gelisah menurun

        [Required]
        [MaxLength(50)]
        public string TerminologySystem { get; set; } = "SLKI";

        public bool IsActive { get; set; } = true;

        public MstNursingDiagnosis? NursingDiagnosis { get; set; }
    }
}
