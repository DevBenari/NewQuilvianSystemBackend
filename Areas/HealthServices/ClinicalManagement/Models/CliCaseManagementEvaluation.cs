using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models
{
    /// <summary>
    /// Evaluasi Awal Manajer Pelayanan Pasien — satu dokumen hidup per episode. <c>BE-RWI-113</c>, kamus data 0.4 bagian 11.7.
    /// </summary>
    [Table("CliCaseManagementEvaluation", Schema = "public")]
    public class CliCaseManagementEvaluation : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(30)]
        public string EvaluationNumber { get; set; } = string.Empty;

        public Guid EncounterId { get; set; }

        public Guid InpEpisodeId { get; set; }

        public Guid PatientId { get; set; }

        /// <summary>Unit episode saat konsep dibuat. Kewenangan tetap dibaca dari unit episode saat simpan.</summary>
        public Guid ServiceUnitIdSnapshot { get; set; }

        public CaseManagementEvaluationStatus EvaluationStatus { get; set; } = CaseManagementEvaluationStatus.Draft;

        public Guid AuthorEmployeeId { get; set; }

        public Guid AuthorUserId { get; set; }

        public DateTime ClinicalDateTime { get; set; }

        public DateTime? CompletedAt { get; set; }

        public Guid? CompletedByUserId { get; set; }

        public DateTime? CancelledAt { get; set; }

        public Guid? CancelledByUserId { get; set; }

        /// <summary>Wajib bila dibatalkan. SENSITIF.</summary>
        [MaxLength(500)]
        public string? CancelReason { get; set; }

        [MaxLength(100)]
        public string? IdempotencyKey { get; set; }
    }
}
