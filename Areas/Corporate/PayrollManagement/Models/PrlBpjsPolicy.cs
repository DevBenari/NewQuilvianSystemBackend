using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Constants;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Models
{
    /// <summary>
    /// LEGACY_PERSISTED / BLOCKED_BY_SCHEMA (PAY-BE-FIX-007): tidak ada consumer aktif. Tabel, mapping, DbSet, dan baris historisnya
    /// dipertahankan karena penghapusan butuh migration (DROP TABLE) yang di luar wewenang task; tidak ada layanan, controller, atau
    /// mesin hitung yang membaca atau menulisnya lagi. Pekerjaan database terpisah diperlukan untuk benar-benar menghapusnya.
    ///
    /// Dahulu: kebijakan tarif dan batas upah iuran BPJS berversi (DEC-PRL-BPJS-001): sumber tarif iuran yang dipakai mesin payroll. Nilai
    /// bawaan Pengaturan Penggajian (HrdPayrollSetting) bukan kebijakan resmi dan berstatus "Menunggu Verifikasi Kebijakan" sampai
    /// HRD memverifikasi dan Finance mengesahkannya menjadi versi di sini. Memuat klasifikasi risiko JKK beserta dasarnya, tarif JHT,
    /// JP, JKM, BPJS Kesehatan, plafon upah, dan masa berlaku.
    ///
    /// Kebijakan yang sudah disahkan tidak disunting; perubahan tarif dibuat sebagai versi baru dan disimpan pada snapshot hitung
    /// sehingga payroll historis tidak berubah.
    /// </summary>
    [Table("PrlBpjsPolicy", Schema = "public")]
    public class PrlBpjsPolicy : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Nomor versi kebijakan (unik); dicatat pada snapshot iuran setiap penerima.</summary>
        [Required, MaxLength(100)]
        public string PolicyVersion { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        /// <summary>Dasar penetapan tarif dan plafon (peraturan/surat BPJS yang dipakai).</summary>
        [Required, MaxLength(1000)]
        public string LegalBasis { get; set; } = string.Empty;

        public DateTime EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        /// <summary>DRAFT | PENDING_APPROVAL | APPROVED | REJECTED | RETIRED (<see cref="StatutoryPolicyStatuses"/>).</summary>
        [Required, MaxLength(30)]
        public string PolicyStatus { get; set; } = StatutoryPolicyStatuses.Draft;

        /// <summary>Kelompok risiko JKK pemberi kerja (1 sampai 5) menurut klasifikasi BPJS Ketenagakerjaan; wajib diisi dengan dasarnya.</summary>
        public int? JkkRiskClass { get; set; }

        /// <summary>Dasar penetapan kelompok risiko JKK (nomor surat/klasifikasi usaha rumah sakit).</summary>
        [MaxLength(1000)]
        public string? JkkRiskBasis { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal HealthCapAmount { get; set; }
        [Column(TypeName = "decimal(9,4)")]
        public decimal HealthEmployeeRate { get; set; }
        [Column(TypeName = "decimal(9,4)")]
        public decimal HealthEmployerRate { get; set; }

        [Column(TypeName = "decimal(9,4)")]
        public decimal JhtEmployeeRate { get; set; }
        [Column(TypeName = "decimal(9,4)")]
        public decimal JhtEmployerRate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal JpCapAmount { get; set; }
        [Column(TypeName = "decimal(9,4)")]
        public decimal JpEmployeeRate { get; set; }
        [Column(TypeName = "decimal(9,4)")]
        public decimal JpEmployerRate { get; set; }

        [Column(TypeName = "decimal(9,4)")]
        public decimal JkkRate { get; set; }
        [Column(TypeName = "decimal(9,4)")]
        public decimal JkmRate { get; set; }

        [MaxLength(500)]
        public string? StatusReason { get; set; }

        /// <summary>Diajukan = HRD menyatakan isi sudah diverifikasi terhadap ketentuan BPJS.</summary>
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
