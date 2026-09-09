using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Models
{
    /// <summary>
    /// Satu periode akuntansi beserta statusnya.
    ///
    /// Periode bersifat bulanan dan tahun bukunya mengikuti tahun kalender
    /// (<c>ACC-DEC-013</c>), sehingga satu tahun buku selalu berisi dua belas periode. Tidak ada
    /// periode ke-13 untuk penyesuaian akhir tahun.
    /// </summary>
    [Table("AccAccountingPeriod", Schema = "public")]
    public class AccAccountingPeriod : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Setiap badan hukum menutup bukunya sendiri (<c>ACC-DEC-037</c>).
        /// </summary>
        [Required]
        public Guid LegalEntityId { get; set; }

        /// <summary>
        /// Bentuk <c>{tahun}-{bulan dua digit}</c>, contoh <c>2026-09</c> (<c>ACC-DEC-013</c>).
        /// Panjangnya tepat 7 karakter.
        /// </summary>
        [Required]
        [MaxLength(7)]
        public string PeriodCode { get; set; } = string.Empty;

        /// <summary>
        /// Sama dengan tahun kalender.
        /// </summary>
        public int FiscalYear { get; set; }

        /// <summary>
        /// 1 sampai 12.
        /// </summary>
        public int PeriodMonth { get; set; }

        /// <summary>
        /// Tanggal 1 bulan itu.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Tanggal terakhir bulan itu. Tahun kabisat ditangani perhitungan tanggal.
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Tiga status menurut <c>ACC-DEC-012</c>. Periode yang dibuka kembali dari
        /// <c>Closed</c> menjadi <c>SoftClosed</c>, bukan <c>Open</c> (<c>ACC-DEC-028</c>);
        /// penegakannya di service, bukan di sini.
        /// </summary>
        public AccountingPeriodStatus PeriodStatus { get; set; } = AccountingPeriodStatus.Open;

        public Guid? ClosedBy { get; set; }

        public DateTime? ClosedAt { get; set; }

        public Guid? ReopenedBy { get; set; }

        public DateTime? ReopenedAt { get; set; }

        /// <summary>
        /// Alasan <b>terakhir</b> saja. Wajib diisi saat membuka kembali (<c>ACC-DEC-027</c>);
        /// penegakannya di service, bukan di sini.
        ///
        /// Riwayat lengkap penutupan dan pembukaan kembali disimpan <c>LoggerService</c>, jadi
        /// tabel ini sengaja tidak menduplikasi jejak audit.
        /// </summary>
        [MaxLength(500)]
        public string? LastReasonNote { get; set; }

        public MstLegalEntity? LegalEntity { get; set; }
    }
}
