using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models
{
    /// <summary>
    /// Riwayat keputusan dokter atas satu obat bawaan — <c>BE-RWI-101</c>, migration <c>R5</c>,
    /// kamus data 0.5 bagian 13.3.
    /// </summary>
    /// <remarks>
    /// <b>Baris tidak pernah diubah.</b> Mengganti keputusan menambah baris baru yang menunjuk
    /// keputusan lama lewat <see cref="SupersedesDecisionId"/>, sehingga seluruh riwayat terapi obat
    /// bawaan tetap dapat ditelusuri (permission-audit-matrix 0.6.0 bagian 9.1).
    /// </remarks>
    [Table("PhmMedicationReconciliationDecision", Schema = "public")]
    public class PhmMedicationReconciliationDecision : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ReconciliationItemId { get; set; }

        /// <summary>Nomor urut keputusan pada satu obat bawaan, mulai 1.</summary>
        public int SequenceNumber { get; set; }

        public ReconciliationDecisionType DecisionType { get; set; }

        /// <summary>Alasan klinis keputusan. SENSITIF.</summary>
        [MaxLength(500)]
        public string? DecisionNote { get; set; }

        [Required]
        public Guid DecidedByDoctorId { get; set; }

        [Required]
        public Guid DecidedByUserId { get; set; }

        public DateTime DecidedAt { get; set; }

        public Guid? ResultPrescriptionId { get; set; }

        public Guid? ResultPrescriptionItemId { get; set; }

        public Guid? SupersedesDecisionId { get; set; }

        public bool IsActive { get; set; } = true;

        public PhmMedicationReconciliationItem? ReconciliationItem { get; set; }

        public MstDoctor? DecidedByDoctor { get; set; }

        public ApplicationUser? DecidedByUser { get; set; }

        public PhmPrescription? ResultPrescription { get; set; }

        public PhmPrescriptionItem? ResultPrescriptionItem { get; set; }

        public PhmMedicationReconciliationDecision? SupersedesDecision { get; set; }
    }
}
