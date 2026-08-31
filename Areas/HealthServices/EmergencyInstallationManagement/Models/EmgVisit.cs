using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models
{
    [Table("EmgVisit", Schema = "public")]
    public class EmgVisit : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string EmergencyVisitNumber { get; set; } = string.Empty;

        public Guid? EncounterId { get; set; }

        public Guid? PatientId { get; set; }

        [Required]
        public Guid ServiceUnitId { get; set; }

        public Guid? ArrivalModeId { get; set; }

        public Guid? CaseTypeId { get; set; }

        public DateTime ArrivalDateTime { get; set; } = DateTime.UtcNow;

        [MaxLength(1000)]
        public string? ChiefComplaint { get; set; }

        [MaxLength(250)]
        public string? ArrivalLocation { get; set; }

        [MaxLength(250)]
        public string? FoundLocation { get; set; }

        [MaxLength(250)]
        public string? TraumaLocation { get; set; }

        public DateTime? TraumaDateTime { get; set; }

        public bool IsUnknownPatient { get; set; }

        [MaxLength(100)]
        public string? TemporaryPatientAlias { get; set; }

        public bool IsImmediateCareAllowed { get; set; }

        public EmergencyRegistrationStatus RegistrationStatus { get; set; }
            = EmergencyRegistrationStatus.Pending;

        public EmergencyVisitStatus VisitStatus { get; set; }
            = EmergencyVisitStatus.Arrived;

        public DateTime? RegistrationCompletedAt { get; set; }

        public Guid? RegistrationCompletedByUserId { get; set; }

        public DateTime? TreatmentStartedAt { get; set; }

        public DateTime? VisitCompletedAt { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Alasan tertulis ketika petugas menembus penjagaan satu pasien satu episode IGD
        /// aktif. Kosong berarti kunjungan ini tidak pernah menembusnya.
        /// </summary>
        /// <remarks>
        /// <c>BE-IGD-025</c>, keputusan <c>IGD-DEC-084</c>. Jalan keluarnya sengaja <b>tidak</b>
        /// dibuat diam-diam: setiap penembusan menyimpan alasan, pelaku, waktu, dan kunjungan
        /// mana yang ditembus, sehingga pemakaiannya dapat ditinjau — bukan hanya diizinkan.
        /// </remarks>
        [MaxLength(1000)]
        public string? DuplicateEpisodeOverrideReason { get; set; }

        /// <summary>Petugas yang menembus penjagaan episode ganda.</summary>
        public Guid? DuplicateEpisodeOverrideByUserId { get; set; }

        /// <summary>Waktu server saat penjagaan episode ganda ditembus.</summary>
        public DateTime? DuplicateEpisodeOverrideAt { get; set; }

        /// <summary>
        /// Kunjungan IGD yang masih berjalan saat penjagaan ditembus. Menyimpannya membuat
        /// dua episode yang sebenarnya satu peristiwa dapat ditelusuri kembali.
        /// </summary>
        public Guid? DuplicateEpisodeOverrideOfVisitId { get; set; }

        public TrxPatientEncounter? Encounter { get; set; }

        public MstPatient? Patient { get; set; }

        public MstServiceUnit? ServiceUnit { get; set; }

        public EmgArrivalMode? ArrivalMode { get; set; }

        public EmgCaseType? CaseType { get; set; }

        public ApplicationUser? RegistrationCompletedByUser { get; set; }

        public ICollection<EmgTriage> Triages { get; set; }
            = new List<EmgTriage>();

        public ICollection<EmgResuscitation> Resuscitations { get; set; }
            = new List<EmgResuscitation>();

        public ICollection<EmgObservation> Observations { get; set; }
            = new List<EmgObservation>();

        public ICollection<EmgDisposition> Dispositions { get; set; }
            = new List<EmgDisposition>();

        public ICollection<EmgDeparture> Departures { get; set; }
            = new List<EmgDeparture>();

        public ICollection<EmgProcedureDetail> ProcedureDetails { get; set; }
            = new List<EmgProcedureDetail>();
    }
}
