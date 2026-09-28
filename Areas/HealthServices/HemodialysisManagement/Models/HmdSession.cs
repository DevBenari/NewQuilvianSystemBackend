using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models
{
    /// <summary>
    /// Satu kali pelaksanaan cuci darah, dari dijadwalkan sampai catatannya dikunci — aggregate root.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Dua pelaku disimpan terpisah</b> (syarat 2): <see cref="DocumentedByUserId"/> yang
    /// menyelesaikan dokumentasi dan <see cref="SignedByUserId"/> yang mengesahkan.
    /// </para>
    /// <para>
    /// Sesi <c>Finalized</c> tidak dapat diubah. Penguncian ditegakkan di dua tempat: status sesi
    /// ini, dan baris keutuhan dokumen Rekam Medis berjenis <c>HemodialysisSession</c>.
    /// </para>
    /// </remarks>
    [Table("HmdSession", Schema = "public")]
    public class HmdSession : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Nomor sesi dari <c>NumberSeriesAllocator</c>, deret <c>HMD_SESSION</c>.</summary>
        [Required]
        [MaxLength(30)]
        public string SessionNumber { get; set; } = string.Empty;

        [Required]
        public Guid EpisodeId { get; set; }

        public HmdEpisode? Episode { get; set; }

        /// <summary>Resep aktif yang dikunci saat sesi dijadwalkan.</summary>
        [Required]
        public Guid PrescriptionId { get; set; }

        public HmdPrescription? Prescription { get; set; }

        /// <summary>Boleh kosong saat dijadwalkan; wajib sebelum sesi dimulai.</summary>
        public Guid? EncounterId { get; set; }

        public RegPatientEncounter? Encounter { get; set; }

        public Guid? InpEpisodeId { get; set; }

        public InpEpisode? InpEpisode { get; set; }

        public Guid? OrderId { get; set; }

        public HmdOrder? Order { get; set; }

        [Required]
        public Guid MachineId { get; set; }

        public HmdMachine? Machine { get; set; }

        [Required]
        public Guid StationId { get; set; }

        public HmdStation? Station { get; set; }

        /// <summary>Dokter penanggung jawab sesi; wajib sebelum sesi dinyatakan siap (<c>HMD-DEC-009</c>).</summary>
        public Guid? ResponsibleDoctorId { get; set; }

        public MstDoctor? ResponsibleDoctor { get; set; }

        public DateOnly ScheduledDate { get; set; }

        public HmdShift Shift { get; set; }

        public DateTime ScheduledStartAt { get; set; }

        public DateTime ScheduledEndAt { get; set; }

        public DateTime? CheckedInAt { get; set; }

        public Guid? CheckedInByUserId { get; set; }

        public DateTime? ReadyAt { get; set; }

        public Guid? ReadyByUserId { get; set; }

        /// <summary>Waktu server saat sesi dimulai, bukan waktu perangkat pengguna.</summary>
        public DateTime? StartedAt { get; set; }

        public Guid? StartedByUserId { get; set; }

        public DateTime? EndedAt { get; set; }

        public Guid? EndedByUserId { get; set; }

        public int? ActualDurationMinutes { get; set; }

        public int? ActualUltrafiltrationMl { get; set; }

        public HmdSessionStatus SessionStatus { get; set; } = HmdSessionStatus.Planned;

        [MaxLength(500)]
        public string? HoldReason { get; set; }

        public HmdSessionStopReason? StopReason { get; set; }

        /// <summary>Keterangan penghentian. SENSITIF.</summary>
        [MaxLength(1000)]
        public string? StopNote { get; set; }

        /// <summary>Penyimpangan dari resep beserta alasannya. SENSITIF.</summary>
        [MaxLength(1000)]
        public string? DeviationNote { get; set; }

        public HmdDisposition? Disposition { get; set; }

        /// <summary>Instruksi tindak lanjut. SENSITIF.</summary>
        [MaxLength(1000)]
        public string? DispositionNote { get; set; }

        [MaxLength(500)]
        public string? CancelReason { get; set; }

        /// <summary>Alasan dokter mengembalikan dokumentasi untuk dilengkapi.</summary>
        [MaxLength(500)]
        public string? ReturnReason { get; set; }

        public Guid? DocumentedByUserId { get; set; }

        public DateTime? DocumentedAt { get; set; }

        public Guid? SignedByUserId { get; set; }

        public DateTime? SignedAt { get; set; }

        /// <summary>SHA-256 isi klinis sesi pada saat disahkan, sebagai bukti keutuhan.</summary>
        [MaxLength(64)]
        public string? RecordHash { get; set; }

        /// <summary>Tindakan pasien yang dibuat saat sesi dimulai; unik per sesi.</summary>
        public Guid? PatientProcedureId { get; set; }

        public TrxPatientProcedure? PatientProcedure { get; set; }

        /// <summary>Kunci idempotency tombol Mulai.</summary>
        [MaxLength(100)]
        public string? IdempotencyKey { get; set; }

        public HmdBillingHandoffStatus BillingHandoffStatus { get; set; } = HmdBillingHandoffStatus.NotRequired;

        public DateTime? BillingHandoffAt { get; set; }

        [MaxLength(1000)]
        public string? BillingHandoffError { get; set; }

        public int Version { get; set; }

        public ICollection<HmdSessionChecklist> Checklists { get; set; } = new List<HmdSessionChecklist>();

        public ICollection<HmdSessionAssessment> Assessments { get; set; } = new List<HmdSessionAssessment>();

        public ICollection<HmdSessionObservation> Observations { get; set; } = new List<HmdSessionObservation>();

        public ICollection<HmdSessionMedication> Medications { get; set; } = new List<HmdSessionMedication>();

        public ICollection<HmdSessionComplication> Complications { get; set; } = new List<HmdSessionComplication>();

        public ICollection<HmdSessionStaffAssignment> StaffAssignments { get; set; } = new List<HmdSessionStaffAssignment>();
    }
}
