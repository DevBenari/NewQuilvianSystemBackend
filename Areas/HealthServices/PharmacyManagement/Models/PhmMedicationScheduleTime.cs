using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models
{
    /// <summary>
    /// Jam standar satu slot pemberian per kode frekuensi, bawaan atau per unit — <c>BE-RWI-114</c>, kamus data 0.4 bagian 11.14.
    /// </summary>
    [Table("PhmMedicationScheduleTime", Schema = "public")]
    public class PhmMedicationScheduleTime : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Sama dengan <c>PhmPrescriptionItem.FrequencyCode</c>.</summary>
        [Required]
        [MaxLength(50)]
        public string FrequencyCode { get; set; } = string.Empty;

        /// <summary><c>null</c> = bawaan seluruh unit.</summary>
        public Guid? ServiceUnitId { get; set; }

        public int SlotNumber { get; set; }

        /// <summary>Jam lokal rumah sakit (Asia/Jakarta).</summary>
        public TimeOnly TimeOfDay { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
