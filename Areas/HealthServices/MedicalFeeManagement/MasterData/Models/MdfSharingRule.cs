using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.MasterData.Models
{
    /// <summary>
    /// Baris aturan tarif sharing jasa medis — MDF-DES-009, MF-DEC-004.
    /// Menyatakan persentase hak jasa untuk satu peran (<see cref="RoleId"/>) pada satu lingkup
    /// layanan (<see cref="TariffId"/> atau <see cref="TariffCategoryId"/>).
    /// Perubahan tarif melahirkan baris baru dan mengisi <see cref="SupersededByRuleId"/> pada baris lama;
    /// baris lama tidak pernah diubah agar riwayat perhitungan masa lalu tidak bergeser.
    /// </summary>
    [Table("MdfSharingRule", Schema = "public")]
    public class MdfSharingRule : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>FK ke kesepakatan induk (MdfSharingAgreement.Id), DeleteBehavior.Cascade.</summary>
        [Required]
        public Guid AgreementId { get; set; }

        /// <summary>FK ke tarif layanan tertentu (MstTariff.Id), DeleteBehavior.Restrict. Null = seluruh layanan.</summary>
        public Guid? TariffId { get; set; }

        /// <summary>FK ke kategori tarif tertentu (MstTariffCategory.Id), DeleteBehavior.Restrict. Null = seluruh kategori.</summary>
        public Guid? TariffCategoryId { get; set; }

        /// <summary>FK ke peran pelaksana jasa medis (MstMedicalFeeRole.Id), DeleteBehavior.Restrict.</summary>
        [Required]
        public Guid RoleId { get; set; }

        /// <summary>Persentase pembagian hak jasa (0.00 .. 100.00).</summary>
        [Required]
        public decimal SharingPercentage { get; set; }

        /// <summary>Tanggal awal berlakunya baris tarif ini.</summary>
        [Required]
        public DateOnly EffectiveStart { get; set; }

        /// <summary>Tanggal akhir berlakunya baris tarif ini. Diisi saat digantikan baris baru.</summary>
        public DateOnly? EffectiveEnd { get; set; }

        /// <summary>FK ke baris aturan baru yang menggantikan baris ini (self-referencing), DeleteBehavior.Restrict.</summary>
        public Guid? SupersededByRuleId { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public MdfSharingAgreement? Agreement { get; set; }
        public MstTariff? Tariff { get; set; }
        public MstTariffCategory? TariffCategory { get; set; }
        public MstMedicalFeeRole? Role { get; set; }
        public MdfSharingRule? SupersededByRule { get; set; }
    }
}

