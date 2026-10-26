using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.Models
{
    /// <summary>Domain sumber layanan medis — data-dictionary.md bagian 6.</summary>
    public static class MdfServiceSourceDomains
    {
        public const string OperatingRoom = "OPERATING_ROOM";
        public const string Procedure = "PROCEDURE";
        public const string Laboratory = "LABORATORY";
        public const string Adhoc = "ADHOC";
        public const string AdhocCatalog = "ADHOC_CATALOG";
    }

    /// <summary>
    /// Rincian perhitungan jasa per baris tindakan/layanan medis — MDF-DES-010, MDF-DES-012, MDF-DES-013, MF-DEC-013, MF-DEC-014.
    /// Menyimpan snapshot nilai kotor sebelum diskon (BaseAmount), persentase tarif (SharingPercentage),
    /// serta referensi wajib ke SharingRuleId agar setiap rupiah dapat ditelusuri ke perjanjian asalnya.
    /// </summary>
    [Table("MdfServiceFeeDetail", Schema = "public")]
    public class MdfServiceFeeDetail : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>FK ke induk hasil jasa (MdfServiceFee.Id), DeleteBehavior.Cascade.</summary>
        [Required]
        public Guid ServiceFeeId { get; set; }

        /// <summary>Domain modul sumber layanan: OPERATING_ROOM, PROCEDURE, LABORATORY, ADHOC, ADHOC_CATALOG.</summary>
        [Required]
        [MaxLength(30)]
        public string SourceDomain { get; set; } = MdfServiceSourceDomains.OperatingRoom;

        /// <summary>ID referensi baris pada modul sumber layanan.</summary>
        [Required]
        [MaxLength(100)]
        public string SourceDetailId { get; set; } = string.Empty;

        /// <summary>Rujukan ke baris tagihan pasien (BilInvoiceItem.Id). Tanpa FK database.</summary>
        public Guid? InvoiceItemId { get; set; }

        /// <summary>FK ke tarif layanan (MstTariff.Id), DeleteBehavior.Restrict. Null jika tindakan non-katalog.</summary>
        public Guid? TariffId { get; set; }

        /// <summary>FK ke peran pelaksana jasa medis (MstMedicalFeeRole.Id), DeleteBehavior.Restrict.</summary>
        [Required]
        public Guid RoleId { get; set; }

        /// <summary>Nilai dasar kotor baris tagihan (Quantity x UnitPrice) sebelum diskon apapun (MDF-DES-012).</summary>
        [Required]
        public decimal BaseAmount { get; set; }

        /// <summary>Snapshot persentase tarif sharing yang berlaku pada tanggal layanan (0.00 .. 100.00).</summary>
        [Required]
        public decimal SharingPercentage { get; set; }

        /// <summary>Hasil kalkulasi jasa: BaseAmount x SharingPercentage / 100 dibulatkan 2 desimal AwayFromZero.</summary>
        [Required]
        public decimal CalculatedAmount { get; set; }

        /// <summary>FK ke baris aturan tarif asal (MdfSharingRule.Id), DeleteBehavior.Restrict. Wajib terisi.</summary>
        [Required]
        public Guid SharingRuleId { get; set; }

        /// <summary>Tanggal pelaksanaan tindakan layanan medis.</summary>
        [Required]
        public DateOnly ServiceDate { get; set; }

        public MdfServiceFee? ServiceFee { get; set; }
        public MstTariff? Tariff { get; set; }
        public MstMedicalFeeRole? Role { get; set; }
        public MdfSharingRule? SharingRule { get; set; }
    }
}

