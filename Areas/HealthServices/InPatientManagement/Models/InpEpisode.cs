using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models
{
    [Table("InpEpisode", Schema = "public")]
    public class InpEpisode : IdentityModel
    {

        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string EpisodeNumber { get; set; } = string.Empty;

        [Required]
        public Guid EncounterId { get; set; }

        [Required]
        public Guid PatientId { get; set; }

        [Required]
        public Guid ServiceUnitId { get; set; }

        [Required]
        public Guid PatientClassId { get; set; }

        public InpEpisodeStatus EpisodeStatus { get; set; } = InpEpisodeStatus.Draft;

        public DateTime? AdmittedAt { get; set; }

        public DateTime? DischargeDecidedAt { get; set; }

        public DateTime? PhysicallyLeftAt { get; set; }

        public Guid? PhysicallyLeftByUserId { get; set; }

        /// <summary>
        /// Status kasir yang dibaca langsung dari Billing saat keluar ruangan dicatat, atau
        /// <see cref="InpClearanceObservation.Unreadable"/> bila Billing tidak terbaca —
        /// kontrak <c>integrasi-billing</c> <c>1.1.0</c> kamus data 6.3.
        /// </summary>
        /// <remarks>Jejak pengamatan, bukan sumber status kasir (<c>INV-RWF-01</c>).</remarks>
        public InpClearanceObservation? DepartureClearanceObserved { get; set; }

        /// <summary>Waktu bacaan status kasir saat keluar ruangan.</summary>
        public DateTime? DepartureClearanceObservedAt { get; set; }

        /// <summary>
        /// <c>true</c> bila pencatat keluar ruangan mengakui peringatan kasir
        /// (<c>INV-RWF-03</c>, <c>VAL-RWF-01</c>).
        /// </summary>
        public bool DepartureClearanceWarningAcknowledged { get; set; } = false;

        /// <summary>
        /// Status kasir saat episode ditutup. Penutupan normal selalu
        /// <see cref="InpClearanceObservation.Cleared"/>; penutupan dengan override menyimpan
        /// bacaan langsung saat itu.
        /// </summary>
        public InpClearanceObservation? ClosureClearanceObserved { get; set; }

        public Guid? MotherEpisodeId { get; set; }

        public bool RequiresIsolation { get; set; } = false;

        public InpIsolationSource? IsolationSource { get; set; }

        public Guid? IsolationSetByUserId { get; set; }

        public Guid? IsolationSetByDoctorId { get; set; }

        public DateTime? IsolationSetAt { get; set; }

        [MaxLength(500)]
        public string? IsolationNote { get; set; }

        public int Version { get; set; } = 1;

        public DateTime? ClosedAt { get; set; }

        public InpDischargeType DischargeType { get; set; } = InpDischargeType.Unknown;

        public bool IsClosedWithoutFinancialClearance { get; set; } = false;

        [MaxLength(500)]
        public string? ClosedWithoutClearanceReason { get; set; }

        // Enam kolom di bawah DIPENSIUNKAN oleh kontrak integrasi-billing 1.1.0 (RWI-DEC-167
        // butir 4, RWI-DEC-187): kolom tetap ada di database supaya data lama tidak hilang,
        // tetapi kode baru tidak menulis maupun membacanya. Status kasir hanya milik Billing
        // (INV-RWF-01). Pembaca yang masih tersisa dicabut BE-RWI-152 dan BE-RWI-153.
        [Obsolete("Dipensiunkan kontrak integrasi-billing 1.1.0 (INV-RWF-01). Baca status kasir lewat Billing.")]
        public BillingClearanceStatus ClearanceStatus { get; set; } = BillingClearanceStatus.None;

        [Obsolete("Dipensiunkan kontrak integrasi-billing 1.1.0 (INV-RWF-01).")]
        [MaxLength(1000)]
        public string? ClearanceRevokedReason { get; set; }

        [Obsolete("Dipensiunkan kontrak integrasi-billing 1.1.0 (RWI-DEC-187).")]
        public bool IsSupervisorOverridden { get; set; } = false;

        [Obsolete("Dipensiunkan kontrak integrasi-billing 1.1.0 (RWI-DEC-187).")]
        [MaxLength(1000)]
        public string? SupervisorOverrideReason { get; set; }

        [Obsolete("Dipensiunkan kontrak integrasi-billing 1.1.0 (RWI-DEC-187).")]
        public Guid? SupervisorOverriddenByUserId { get; set; }

        [Obsolete("Dipensiunkan kontrak integrasi-billing 1.1.0 (RWI-DEC-187).")]
        public DateTime? SupervisorOverriddenAtUtc { get; set; }

        [MaxLength(500)]
        public string? CancelReason { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public RegPatientEncounter? Encounter { get; set; }

        public MstPatient? Patient { get; set; }

        public MstServiceUnit? ServiceUnit { get; set; }

        public MstPatientClass? PatientClass { get; set; }

        public ApplicationUser? PhysicallyLeftByUser { get; set; }

        public InpEpisode? MotherEpisode { get; set; }

        public ApplicationUser? IsolationSetByUser { get; set; }

        public MstDoctor? IsolationSetByDoctor { get; set; }

        public ICollection<InpDoctorAssignment> DoctorAssignments { get; set; } = new List<InpDoctorAssignment>();

        public ICollection<InpNurseAssignment> NurseAssignments { get; set; } = new List<InpNurseAssignment>();

        public ICollection<InpBedReservation> BedReservations { get; set; } = new List<InpBedReservation>();

        public ICollection<InpBedPlacement> BedPlacements { get; set; } = new List<InpBedPlacement>();

        public ICollection<InpClearanceMark> ClearanceMarks { get; set; } = new List<InpClearanceMark>();

        public ICollection<InpFinancialClearance> FinancialClearances { get; set; } = new List<InpFinancialClearance>();

        public ICollection<InpStatusHistory> StatusHistories { get; set; } = new List<InpStatusHistory>();

        public ICollection<InpCorrectionSession> CorrectionSessions { get; set; } = new List<InpCorrectionSession>();
    }
}
