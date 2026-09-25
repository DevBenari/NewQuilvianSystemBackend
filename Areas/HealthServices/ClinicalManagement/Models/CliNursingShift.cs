using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models
{
    /// <summary>
    /// Jam shift perawat per unit atau bawaan, hanya untuk menghitung total cairan — <c>BE-RWI-120</c>, kamus data 0.4 bagian 11.11. Tidak pernah menentukan kewenangan (<c>RWI-DEC-100</c>).
    /// </summary>
    [Table("CliNursingShift", Schema = "public")]
    public class CliNursingShift : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary><c>null</c> = bawaan seluruh unit.</summary>
        public Guid? ServiceUnitId { get; set; }

        [Required]
        [MaxLength(20)]
        public string ShiftCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string ShiftName { get; set; } = string.Empty;

        public TimeOnly StartTime { get; set; }

        /// <summary>Lebih kecil dari StartTime berarti melewati tengah malam.</summary>
        public TimeOnly EndTime { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
