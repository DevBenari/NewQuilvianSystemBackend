using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.Models
{
    /// <summary>Status hasil jasa medis satu penerima — data-dictionary.md bagian 5.</summary>
    public static class MdfServiceFeeStatuses
    {
        public const string Calculated = "Calculated";
        public const string Verified = "Verified";
        public const string Approved = "Approved";
        public const string HandedOff = "HandedOff";
    }

    /// <summary>
    /// Hasil perhitungan jasa medis per penerima per periode (aggregate root) — MDF-DES-010, MDF-DES-012, MF-DEC-005.
    /// Menyimpan akumulasi jasa satu dokter atau staf untuk satu periode kalkulasi.
    /// Pasangan (PeriodId, PayeeType, PayeeReferenceId) wajib unik di antara baris aktif.
    /// FinalAmount = GrossAmount + AdjustmentAmount ditegakkan oleh check constraint basis data.
    /// </summary>
    [Table("MdfServiceFee", Schema = "public")]
    public class MdfServiceFee : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Nomor unik hasil jasa, contoh: 'SF-202610-DOC-0001'.</summary>
        [Required]
        [MaxLength(50)]
        public string FeeNumber { get; set; } = string.Empty;

        /// <summary>FK ke periode kalkulasi (MdfFeePeriod.Id), DeleteBehavior.Restrict.</summary>
        [Required]
        public Guid PeriodId { get; set; }

        /// <summary>Jenis penerima: Doctor atau Workforce (MdfPayeeTypes).</summary>
        [Required]
        [MaxLength(30)]
        public string PayeeType { get; set; } = MdfPayeeTypes.Doctor;

        /// <summary>Rujukan polimorfik ke MstDoctor atau MstWorkforceProfile. Tanpa FK database.</summary>
        [Required]
        public Guid PayeeReferenceId { get; set; }

        /// <summary>Total nilai kotor jasa medis dari penjumlahan CalculatedAmount seluruh rincian.</summary>
        [Required]
        public decimal GrossAmount { get; set; }

        /// <summary>Total penyesuaian/koreksi berstatus disetujui (dapat bertanda positif atau negatif).</summary>
        [Required]
        public decimal AdjustmentAmount { get; set; }

        /// <summary>Nilai akhir jasa kotor (GrossAmount + AdjustmentAmount), sebelum potongan pajak PPh 21.</summary>
        [Required]
        public decimal FinalAmount { get; set; }

        /// <summary>Status hasil jasa: Calculated, Verified, Approved, HandedOff (MdfServiceFeeStatuses).</summary>
        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = MdfServiceFeeStatuses.Calculated;

        /// <summary>Waktu kalkulasi hasil jasa ini dibuat atau terakhir diperbarui.</summary>
        [Required]
        public DateTime CalculatedAt { get; set; }

        /// <summary>User ID pemverifikasi hasil jasa (wajib berbeda dari ApprovedBy).</summary>
        public Guid? VerifiedBy { get; set; }

        /// <summary>Waktu verifikasi hasil jasa dicatat.</summary>
        public DateTime? VerifiedAt { get; set; }

        /// <summary>User ID penyetujui hasil jasa (wajib berbeda dari VerifiedBy).</summary>
        public Guid? ApprovedBy { get; set; }

        /// <summary>Waktu persetujuan hasil jasa dicatat.</summary>
        public DateTime? ApprovedAt { get; set; }

        /// <summary>Token konkurensi optimistik (MDF-DES-005).</summary>
        public Guid RowVersion { get; set; } = Guid.NewGuid();

        public MdfFeePeriod? Period { get; set; }
        public ICollection<MdfServiceFeeDetail> Details { get; set; } = new List<MdfServiceFeeDetail>();
        public ICollection<MdfServiceFeeAdjustment> Adjustments { get; set; } = new List<MdfServiceFeeAdjustment>();
    }
}

