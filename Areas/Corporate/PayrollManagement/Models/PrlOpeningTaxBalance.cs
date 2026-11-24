using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Models
{
    /// <summary>
    /// Saldo awal PPh historis (Opening Tax Balance, PAY-BE-FIX-016): akumulasi penghasilan dan PPh satu pegawai pada satu tahun pajak untuk
    /// bulan-bulan yang tidak punya payroll run di Quilvian (mis. sebelum cutover). Dipakai rekonsiliasi masa pajak terakhir HANYA bila
    /// berstatus APPROVED dan bersumber SAME_EMPLOYER_PRE_CUTOVER (lihat <c>PayrollCalculationService.BuildYtd</c>).
    ///
    /// Satu baris mewakili BANYAK masa pajak: <see cref="CoveredMonths"/> menyimpan bulan yang dicakup (1..12), bukan jumlah baris. Nilai
    /// adalah kumulatif atas bulan-bulan itu saja. Perubahan sesudah disahkan dibuat sebagai versi baru (<see cref="Version"/>); versi
    /// lama menjadi SUPERSEDED dan tidak dihapus. Tidak pernah mengubah payroll run yang sudah final.
    ///
    /// Tabel dibuat lewat DDL yang disetujui pemilik (PAY-BE-FIX-015 bagian 9.1), bukan migration pada task ini.
    /// </summary>
    [Table("PrlOpeningTaxBalance", Schema = "public")]
    public class PrlOpeningTaxBalance : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid WorkforceProfileId { get; set; }

        public int TaxYear { get; set; }

        /// <summary>Bulan (1..12) yang dicakup saldo ini, terurut dan tanpa duplikat. Kolom <c>integer[]</c>.</summary>
        public List<int> CoveredMonths { get; set; } = new();

        /// <summary>SAME_EMPLOYER_PRE_CUTOVER | PREVIOUS_EMPLOYER (<see cref="OpeningTaxBalanceEmployerSources"/>).</summary>
        [Required, MaxLength(40)]
        public string EmployerSource { get; set; } = OpeningTaxBalanceEmployerSources.SameEmployerPreCutover;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxableGross { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal EmployerPremium { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PensionDeduction { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxWithheld { get; set; }

        /// <summary>Dokumen asal angka (slip, 1721-A1, sistem lama). Wajib sebelum diajukan.</summary>
        [MaxLength(500)]
        public string? SourceReference { get; set; }

        /// <summary>DRAFT | PENDING_APPROVAL | APPROVED | REJECTED | SUPERSEDED (<see cref="OpeningTaxBalanceStatuses"/>).</summary>
        [Required, MaxLength(30)]
        public string Status { get; set; } = OpeningTaxBalanceStatuses.Draft;

        public int Version { get; set; } = 1;

        public DateTime? SubmittedAt { get; set; }
        public Guid? SubmittedByUserId { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public Guid? ApprovedByUserId { get; set; }

        /// <summary>
        /// Pelaku dan waktu penolakan. Pengaju boleh sama dengan penolak (tidak ada larangan; pembeda adalah izin/role di Akses Role),
        /// berbeda dari persetujuan yang wajib oleh pengguna lain.
        /// </summary>
        public DateTime? RejectedAt { get; set; }
        public Guid? RejectedByUserId { get; set; }

        /// <summary>Catatan keputusan; wajib pada penolakan.</summary>
        [MaxLength(1000)]
        public string? DecisionNote { get; set; }

        public MstWorkforceProfile? WorkforceProfile { get; set; }
    }
}
