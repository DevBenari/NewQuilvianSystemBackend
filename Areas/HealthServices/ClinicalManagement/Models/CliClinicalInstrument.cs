using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models
{
    /// <summary>
    /// Instrumen atau formulir klinis — <c>BE-RWI-107</c>, kamus data 0.4 bagian 11.4.
    /// </summary>
    [Table("CliClinicalInstrument", Schema = "public")]
    public class CliClinicalInstrument : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Kode unik, contoh <c>FALL_RISK_ADULT</c>.</summary>
        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        /// <summary>Nama tampil, contoh "Risiko Jatuh Dewasa".</summary>
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>Jenis instrumen.</summary>
        public ClinicalInstrumentKind InstrumentKind { get; set; }

        /// <summary>Batas usia bawah inklusif dalam bulan; <c>null</c> = tanpa batas bawah.</summary>
        public int? TargetMinAgeMonths { get; set; }

        /// <summary>Batas usia atas eksklusif dalam bulan; <c>null</c> = terbuka.</summary>
        public int? TargetMaxAgeMonths { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
