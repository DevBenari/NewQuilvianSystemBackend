using QuilvianSystemBackend.Areas.Administrator.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models
{
    /// <summary>
    /// Satu janji kedatangan pasien prioritas untuk satu tanggal, sebelum poli dan dokter dipilih
    /// (rawat-jalan Amendment RP, RJ-PRI-DEC-009). Reservasi bukan kunjungan: tidak membuat
    /// antrean maupun tagihan. Kunjungan baru terbit saat admisi melengkapi poli dan dokter.
    /// </summary>
    /// <remarks>
    /// Kode link tidak pernah disimpan dalam bentuk asli; yang tersimpan hanya hash SHA-256
    /// (<see cref="ArrivalCodeHash"/>, INV-RP-05). Snapshot klasifikasi diambil saat reservasi
    /// dibuat dan tidak diubah (INV-RP-03).
    /// </remarks>
    [Table("RegPriorityReservation", Schema = "public")]
    public class RegPriorityReservation : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(30)]
        public string ReservationNumber { get; set; } = string.Empty;

        [Required]
        public Guid PatientId { get; set; }

        /// <summary>Tanggal kunjungan, kalender WIB, tanpa jam.</summary>
        [Required]
        public DateTime VisitDate { get; set; }

        [Required]
        public PriorityReservationStatus Status { get; set; } = PriorityReservationStatus.AwaitingArrival;

        /// <summary>Hex SHA-256 dari kode link (64 karakter). Sensitif: tidak masuk log maupun respons.</summary>
        [Required]
        [MaxLength(64)]
        public string ArrivalCodeHash { get; set; } = string.Empty;

        [Required]
        public DateTime ArrivalCodeIssuedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ArrivedAt { get; set; }

        public PriorityReservationArrivalSource? ArrivalSource { get; set; }

        public Guid? ArrivalMarkedByUserId { get; set; }

        /// <summary>Checklist skrining awal (RJ-PRI-DEC-019). Sensitif: data kesehatan.</summary>
        public bool? ScreeningCough { get; set; }

        public bool? ScreeningFever { get; set; }

        public bool? ScreeningFallRisk { get; set; }

        public DateTime? ScreenedAt { get; set; }

        public Guid? ScreenedByUserId { get; set; }

        /// <summary>Kunjungan hasil pelengkapan atau pendaftaran lewat jalur lain.</summary>
        public Guid? EncounterId { get; set; }

        public DateTime? ClosedAt { get; set; }

        public Guid? ClosedByUserId { get; set; }

        /// <summary>Alasan batal (10–500 karakter). Sensitif: bisa memuat keterangan kesehatan.</summary>
        [MaxLength(500)]
        public string? ClosureReason { get; set; }

        /// <summary>Salinan <c>PriorityQueue</c> tingkat saat reservasi dibuat.</summary>
        public bool IsPrioritySnapshot { get; set; }

        public Guid? PatientMembershipIdSnapshot { get; set; }

        public Guid? MembershipTierIdSnapshot { get; set; }

        [MaxLength(50)]
        public string? MembershipTierCodeSnapshot { get; set; }

        public int QueuePriorityLevelSnapshot { get; set; }

        public QueueAudience QueueAudienceSnapshot { get; set; } = QueueAudience.Regular;

        public PublicDisplayMode PublicDisplayModeSnapshot { get; set; } = PublicDisplayMode.Default;

        public Guid RowVersion { get; set; } = Guid.NewGuid();

        public MstPatient? Patient { get; set; }

        public MstPatientMembership? PatientMembershipSnapshot { get; set; }

        public RegPatientEncounter? Encounter { get; set; }

        public ICollection<RegPriorityReservationHistory> Histories { get; set; } = new List<RegPriorityReservationHistory>();
    }
}
