using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Constants;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Models
{
    /// <summary>
    /// Kebijakan tarif PPh berversi (DEC-PRL-TAX-008): satu-satunya sumber tarif untuk skema AUTO_REGULATORY. HRD mengelola profil pajak
    /// per orang tetapi tidak menentukan tarif per individu; payroll mengambil kebijakan yang berstatus APPROVED dan berlaku pada
    /// periode. Memuat dasar peraturan, masa berlaku, parameter perhitungan, tabel PTKP, tabel TER, dan lapisan Pasal 17.
    ///
    /// Tidak ada angka regulasi yang ditanam di kode atau diisi otomatis: seluruh nilai dimasukkan dan diverifikasi HRD, lalu disahkan
    /// Finance. Kebijakan yang sudah disahkan tidak disunting; perubahan dibuat sebagai versi baru.
    /// </summary>
    [Table("PrlTaxPolicy", Schema = "public")]
    public class PrlTaxPolicy : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Nomor versi kebijakan (unik), mis. nomor peraturan beserta urutan revisi. Dicatat pada snapshot setiap hasil hitung.</summary>
        [Required, MaxLength(100)]
        public string PolicyVersion { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        /// <summary>Dasar peraturan (mis. PP 58/2023 dan PMK 168/2023 beserta pasal/lampiran yang dipakai).</summary>
        [Required, MaxLength(1000)]
        public string LegalBasis { get; set; } = string.Empty;

        public DateTime EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        /// <summary>DRAFT | PENDING_APPROVAL | APPROVED | REJECTED | RETIRED (<see cref="StatutoryPolicyStatuses"/>).</summary>
        [Required, MaxLength(30)]
        public string PolicyStatus { get; set; } = StatutoryPolicyStatuses.Draft;

        // ---- Parameter perhitungan (semua dari peraturan; diisi HRD) ----

        /// <summary>Biaya jabatan: persen dari penghasilan bruto.</summary>
        [Column(TypeName = "decimal(9,4)")]
        public decimal PositionCostRatePercent { get; set; }

        /// <summary>Biaya jabatan: batas per bulan.</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal PositionCostMonthlyCap { get; set; }

        /// <summary>Biaya jabatan: batas setahun.</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal PositionCostAnnualCap { get; set; }

        /// <summary>Tambahan persen untuk penerima tanpa NPWP (mis. 20 berarti 120% dari tarif).</summary>
        [Column(TypeName = "decimal(9,4)")]
        public decimal NoNpwpSurchargePercent { get; set; }

        /// <summary>Kelipatan pembulatan ke bawah penghasilan kena pajak (PKP).</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal PkpRoundingUnit { get; set; }

        /// <summary>Bila true, PPh dibulatkan ke rupiah penuh; bila false, dua desimal.</summary>
        public bool RoundTaxToWholeRupiah { get; set; }

        /// <summary>Program iuran pemberi kerja yang termasuk penghasilan bruto (daftar dipisah koma; kosong = tidak ada), mis. premi asuransi.</summary>
        [MaxLength(50)]
        public string? EmployerPremiumPrograms { get; set; }

        /// <summary>Program iuran pegawai yang menjadi pengurang (iuran pensiun/JHT) pada perhitungan tahunan (daftar dipisah koma; kosong = tidak ada).</summary>
        [MaxLength(50)]
        public string? PensionDeductionPrograms { get; set; }

        /// <summary>Persen dasar pengenaan untuk penerima bukan pegawai; kosong = formula bukan pegawai belum tersedia pada kebijakan ini.</summary>
        [Column(TypeName = "decimal(9,4)")]
        public decimal? NonEmployeeBasisPercent { get; set; }

        /// <summary>PER_PAYMENT atau CUMULATIVE (<see cref="TaxNonEmployeeMethods"/>); wajib bila NonEmployeeBasisPercent diisi.</summary>
        [MaxLength(30)]
        public string? NonEmployeeMethod { get; set; }

        /// <summary>Tarif PPh Pasal 26 umum (persen); kosong = PPh 26 belum tersedia pada kebijakan ini.</summary>
        [Column(TypeName = "decimal(9,4)")]
        public decimal? Pph26RatePercent { get; set; }

        /// <summary>Tabel PTKP: [{status, annualAmount, terCategory}]. Kategori TER per status ditentukan kebijakan.</summary>
        [Column(TypeName = "text")]
        public string PtkpJson { get; set; } = "[]";

        /// <summary>Tabel TER: [{category, periodKind, lowerBound, upperBound, ratePercent}], batas bawah tidak termasuk (kecuali 0), batas atas termasuk.</summary>
        [Column(TypeName = "text")]
        public string TerRatesJson { get; set; } = "[]";

        /// <summary>Lapisan tarif Pasal 17: [{lowerBound, upperBound, ratePercent}].</summary>
        [Column(TypeName = "text")]
        public string BracketsJson { get; set; } = "[]";

        // ---- Siklus verifikasi dan pengesahan ----

        /// <summary>Alasan penolakan atau pensiun kebijakan.</summary>
        [MaxLength(500)]
        public string? StatusReason { get; set; }

        /// <summary>Diajukan = HRD menyatakan isi sudah diverifikasi terhadap dasar peraturan.</summary>
        public DateTime? SubmittedAt { get; set; }
        public Guid? SubmittedByUserId { get; set; }

        /// <summary>Disahkan atau ditolak oleh Finance; selalu pengguna yang berbeda dari pengaju.</summary>
        public DateTime? DecidedAt { get; set; }
        public Guid? DecidedByUserId { get; set; }

        public DateTime? RetiredAt { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
