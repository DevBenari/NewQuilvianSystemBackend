using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Models
{
    /// <summary>Angka bawaan unit Optik (<c>OPT-DEC-009</c>, <c>OPT-DEC-040</c>, <c>OPT-DEC-041</c>). Nilainya dibekukan ke pesanan/resep saat dibuat; batas diskon tidak di sini (milik kebijakan diskon Billing).</summary>
    /// <remarks>OPT-BP-001, BE-OPT-002 — kolom mengikuti <c>erd/data-dictionary.md</c> r1.</remarks>
    [Table("OptSetting", Schema = "public")]
    public class OptSetting : IdentityModel
    {
        /// <summary>Kunci utama</summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Unit Optik</summary>
        [Required]
        public Guid ServiceUnitId { get; set; }

        /// <summary>0..100 (OPT-DEC-009)</summary>
        public decimal DownPaymentMinPercent { get; set; } = 50m;

        /// <summary>1..365 (OPT-DEC-040)</summary>
        public int PickupReminderDays { get; set; } = 30;

        /// <summary>&gt; PickupReminderDays</summary>
        public int NotCollectedCloseDays { get; set; } = 90;

        /// <summary>1..60</summary>
        public int PrescriptionValidityMonths { get; set; } = 12;

        /// <summary>Garansi ukuran (OPT-DEC-041)</summary>
        public int WarrantyFitDays { get; set; } = 30;

        /// <summary>Garansi cacat lensa</summary>
        public int WarrantyLensDefectMonths { get; set; } = 6;

        /// <summary>Garansi cacat frame</summary>
        public int WarrantyFrameDefectMonths { get; set; } = 6;

        /// <summary>PROPOSED, dikonfirmasi RO/Sp.M</summary>
        public decimal ContactLensBaseCurveMin { get; set; } = 7.50m;

        /// <summary>PROPOSED</summary>
        public decimal ContactLensBaseCurveMax { get; set; } = 9.50m;

        /// <summary>PROPOSED</summary>
        public decimal ContactLensDiameterMin { get; set; } = 13.00m;

        /// <summary>PROPOSED</summary>
        public decimal ContactLensDiameterMax { get; set; } = 15.00m;

        /// <summary>Penanda concurrency (IsConcurrencyToken); naik setiap perubahan</summary>
        public int Version { get; set; } = 0;

        public ICollection<OptSettingRevision> Revisions { get; set; } = new List<OptSettingRevision>();
    }
}
