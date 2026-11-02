using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Models
{
    /// <summary>
    /// Kategori kelayakan kerja pada resume, contoh <c>FIT</c> atau <c>UNFIT_SEMENTARA</c>.
    /// Daftar awalnya usulan dan wajib diratifikasi dokter MCU (<c>MCU-OQ-003</c>).
    /// </summary>
    [Table("McuFitnessCategory", Schema = "public")]
    public class McuFitnessCategory : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(30)]
        public string FitnessCategoryCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FitnessCategoryName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
