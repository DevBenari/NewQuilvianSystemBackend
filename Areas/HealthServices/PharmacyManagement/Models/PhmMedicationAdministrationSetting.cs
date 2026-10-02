using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models
{
    /// <summary>
    /// Pengaturan MAR, satu baris aktif — <c>BE-RWI-114</c>, kamus data 0.4 bagian 11.14.
    /// </summary>
    [Table("PhmMedicationAdministrationSetting", Schema = "public")]
    public class PhmMedicationAdministrationSetting : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>1–72 jam.</summary>
        public int DoseGenerationHorizonHours { get; set; }

        /// <summary>Penanda tampilan lewat waktu; <c>null</c> = tanpa penanda.</summary>
        public int? MissedAfterMinutes { get; set; }

        public int? PrnEvaluationMinutes { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
