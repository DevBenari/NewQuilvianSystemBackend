using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models
{
    /// <summary>
    /// Riwayat satu perubahan jadwal (reschedule) appointment Rawat Jalan
    /// (RJ-REG-REV-BE-005, RJ-REG-DEC-007).
    /// </summary>
    /// <remarks>
    /// ID kunjungan tidak berubah saat reschedule; yang berpindah adalah tanggal, jadwal dokter,
    /// dan antrean. Baris ini menyimpan nilai sebelum dan sesudah beserta alasan dan pelakunya
    /// sehingga perubahan dapat ditelusuri. Alasan dapat memuat keterangan pasien, jadi tidak
    /// disalin ke application log.
    /// </remarks>
    [Table("RegEncounterReschedule", Schema = "public")]
    public class RegEncounterReschedule : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EncounterId { get; set; }

        public DateTime OldEncounterDate { get; set; }

        public DateTime NewEncounterDate { get; set; }

        public Guid? OldDoctorScheduleId { get; set; }

        public Guid? NewDoctorScheduleId { get; set; }

        public Guid? OldDoctorId { get; set; }

        public Guid? NewDoctorId { get; set; }

        public Guid? OldQueueId { get; set; }

        public Guid? NewQueueId { get; set; }

        [MaxLength(30)]
        public string? OldQueueCode { get; set; }

        [MaxLength(30)]
        public string? NewQueueCode { get; set; }

        [Required]
        [MaxLength(250)]
        public string Reason { get; set; } = string.Empty;

        public DateTime RescheduledAt { get; set; }

        public Guid RescheduledByUserId { get; set; }

        public RegPatientEncounter? Encounter { get; set; }
    }
}
