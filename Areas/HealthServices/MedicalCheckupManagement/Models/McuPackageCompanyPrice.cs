using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Models
{
    /// <summary>
    /// Harga khusus sebuah paket untuk satu perusahaan penjamin, dalam rentang tanggal tertentu.
    /// Rentang per pasangan paket–perusahaan tidak boleh tumpang tindih (<c>VAL-MCU-05</c>, dijaga service).
    /// </summary>
    [Table("McuPackageCompanyPrice", Schema = "public")]
    public class McuPackageCompanyPrice : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PackageId { get; set; }

        public McuPackage? Package { get; set; }

        [Required]
        public Guid CompanyGuarantorId { get; set; }

        public MstCompanyGuarantor? CompanyGuarantor { get; set; }

        /// <summary>Tarif khusus ber-<c>IsPackageTariff</c>.</summary>
        [Required]
        public Guid TariffId { get; set; }

        public MstTariff? Tariff { get; set; }

        /// <summary>Tanggal kalender WIB.</summary>
        public DateOnly EffectiveStartDate { get; set; }

        /// <summary>Kosong = tanpa batas akhir.</summary>
        public DateOnly? EffectiveEndDate { get; set; }
    }
}
