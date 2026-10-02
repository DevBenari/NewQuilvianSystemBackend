using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models
{
    [Table("InpDischargeSummaryRevision", Schema = "public")]
    public class InpDischargeSummaryRevision : IdentityModel
    {

        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid DischargeSummaryId { get; set; }

        public int RevisionNumber { get; set; }

        public Guid? CorrectionSessionId { get; set; }

        [Required]
        [MaxLength(1000)]
        public string PrimaryDiagnosisText { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? SecondaryDiagnosisText { get; set; }

        [MaxLength(2000)]
        public string? ProcedureSummary { get; set; }

        [MaxLength(2000)]
        public string? DischargeMedicationNote { get; set; }

        [MaxLength(2000)]
        public string? FollowUpInstruction { get; set; }

        [MaxLength(250)]
        public string? ReferralDestination { get; set; }

        [MaxLength(4000)]
        public string? ClinicalSummary { get; set; }

        /// <summary>
        /// Salinan Pemeriksaan Penting versi sebelumnya. Kolom baru <c>BE-RWI-085</c>.
        /// <b>SENSITIF.</b>
        /// </summary>
        [MaxLength(4000)]
        public string? ImportantFindingsSummary { get; set; }

        /// <summary>
        /// Salinan Kondisi Saat Pulang versi sebelumnya. Kolom baru <c>BE-RWI-085</c>.
        /// <b>SENSITIF.</b>
        /// </summary>
        [MaxLength(2000)]
        public string? DischargeConditionNote { get; set; }

        /// <summary>
        /// Salinan Edukasi versi sebelumnya. Kolom baru <c>BE-RWI-085</c>. <b>SENSITIF.</b>
        /// </summary>
        [MaxLength(2000)]
        public string? EducationSummary { get; set; }

        public InpDischargeType PreviousDischargeType { get; set; }

        public DateTime PreviousSignedAt { get; set; }

        [Required]
        public Guid PreviousSignedByDoctorId { get; set; }

        public DateTime SupersededAt { get; set; } = DateTime.UtcNow;

        [Required]
        public Guid SupersededByUserId { get; set; }

        public bool IsActive { get; set; } = true;

        public InpDischargeSummary? DischargeSummary { get; set; }

        public InpCorrectionSession? CorrectionSession { get; set; }

        public MstDoctor? PreviousSignedByDoctor { get; set; }

        public ApplicationUser? SupersededByUser { get; set; }
    }
}
