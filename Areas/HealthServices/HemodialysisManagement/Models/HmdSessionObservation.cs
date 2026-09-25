using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models
{
    /// <summary>
    /// Satu baris pemantauan pada satu waktu selama sesi berjalan. Setiap pengamatan adalah baris
    /// baru; tabel ini tidak pernah menyimpan hanya nilai terakhir.
    /// </summary>
    /// <remarks>
    /// Parameter mesin (QB, QD, TMP, tekanan vena dan arteri) sengaja tinggal di sini, tidak
    /// dipaksakan masuk ke <c>TrxPatientVitalSign</c>.
    /// </remarks>
    [Table("HmdSessionObservation", Schema = "public")]
    public class HmdSessionObservation : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid SessionId { get; set; }

        public HmdSession? Session { get; set; }

        /// <summary>Urutan pengamatan dalam sesi; unik per sesi.</summary>
        public int SequenceNumber { get; set; }

        public DateTime ObservedAt { get; set; }

        [Required]
        public Guid RecordedByUserId { get; set; }

        public int? SystolicBp { get; set; }

        public int? DiastolicBp { get; set; }

        public int? PulseRate { get; set; }

        public int? RespiratoryRate { get; set; }

        public decimal? TemperatureC { get; set; }

        public int? OxygenSaturation { get; set; }

        public int? BloodFlowRate { get; set; }

        public int? DialysateFlowRate { get; set; }

        public int? TransmembranePressure { get; set; }

        public int? VenousPressure { get; set; }

        public int? ArterialPressure { get; set; }

        public int? UltrafiltrationVolumeMl { get; set; }

        /// <summary>Catatan pengamatan termasuk keluhan subjektif. SENSITIF.</summary>
        [MaxLength(1000)]
        public string? Note { get; set; }
    }
}
