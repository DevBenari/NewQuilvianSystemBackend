using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models
{
    /// <summary>
    /// Tiket antrean Admisi Rawat Jalan yang diambil pasien reguler di kiosk (blueprint Cetak
    /// Antrean Admisi v1.1). Anonim: diterbitkan sebelum pasien dan kunjungan dikenali, sehingga
    /// sengaja bukan <see cref="RegQueue"/> yang mewajibkan <c>EncounterId</c> dan
    /// <c>PatientId</c>. Nomor antrean poliklinik internal tetap dibentuk oleh
    /// <see cref="RegQueue"/> saat kunjungan disimpan.
    /// </summary>
    [Table("RegAdmissionTicket", Schema = "public")]
    public class RegAdmissionTicket : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Tanggal pelayanan WIB; nomor urut diulang per tanggal dan kelompok.</summary>
        [Required]
        public DateTime ServiceDate { get; set; }

        /// <summary>Kode kelompok antrean admisi dari konfigurasi, contoh <c>REGULER</c>.</summary>
        [Required]
        [MaxLength(30)]
        public string QueueGroupCode { get; set; } = string.Empty;

        /// <summary>Snapshot nama kelompok saat tiket terbit.</summary>
        [Required]
        [MaxLength(100)]
        public string QueueGroupName { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string Prefix { get; set; } = string.Empty;

        [Required]
        public int SequenceNumber { get; set; }

        /// <summary>Nomor yang dicetak dan dipanggil, contoh <c>A-015</c>.</summary>
        [Required]
        [MaxLength(20)]
        public string DisplayNumber { get; set; } = string.Empty;

        [Required]
        public DateTime IssuedAt { get; set; }

        /// <summary>Akun perangkat kiosk yang menerbitkan tiket.</summary>
        public Guid IssuedByUserId { get; set; }

        /// <summary>Kunci idempotensi dari kiosk; ketukan ganda memulangkan tiket yang sama.</summary>
        [MaxLength(100)]
        public string? IdempotencyKey { get; set; }

        public AdmissionTicketStatus Status { get; set; } = AdmissionTicketStatus.Waiting;

        public AdmissionTicketPrintStatus PrintStatus { get; set; } = AdmissionTicketPrintStatus.Unconfirmed;

        public int PrintAttemptCount { get; set; }

        public DateTime? LastPrintedAt { get; set; }

        [MaxLength(250)]
        public string? PrintFailureReason { get; set; }

        [MaxLength(30)]
        public string? CounterCode { get; set; }

        [MaxLength(100)]
        public string? CounterName { get; set; }

        public DateTime? CalledAt { get; set; }

        public Guid? CalledByUserId { get; set; }

        public int CallAttemptCount { get; set; }

        public DateTime? ServiceStartedAt { get; set; }

        public Guid? ServiceStartedByUserId { get; set; }

        public DateTime? ServiceEndedAt { get; set; }

        public Guid? ServiceEndedByUserId { get; set; }

        public int SkipCount { get; set; }

        public DateTime? SkippedAt { get; set; }

        public Guid? SkippedByUserId { get; set; }

        [MaxLength(250)]
        public string? SkipReason { get; set; }

        public DateTime? CancelledAt { get; set; }

        public Guid? CancelledByUserId { get; set; }

        [MaxLength(250)]
        public string? CancelReason { get; set; }

        /// <summary>Catatan penutupan loket, wajib bila selesai tanpa kunjungan.</summary>
        [MaxLength(500)]
        public string? CompletionNote { get; set; }

        /// <summary>Kunjungan hasil registrasi dari tiket ini (audit asal pendaftaran).</summary>
        public Guid? EncounterId { get; set; }

        public DateTime? RegistrationLinkedAt { get; set; }

        public RegPatientEncounter? Encounter { get; set; }

        public ICollection<RegAdmissionTicketHistory> Histories { get; set; } = new List<RegAdmissionTicketHistory>();
    }
}
