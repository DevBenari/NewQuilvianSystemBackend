using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Models
{
    /// <summary>
    /// Riwayat tindakan pada penutupan sebuah periode akuntansi. Barisnya <b>tidak pernah</b>
    /// diubah maupun dihapus.
    ///
    /// Bentuknya meniru <c>AccJournalApproval</c>, dan alasannya sama: riwayat ini adalah data
    /// bisnis yang ditampilkan kepada pengguna, bukan log teknis. Ia menjawab pertanyaan audit
    /// "siapa menyatakan angka bulan ini final, dan kapan".
    ///
    /// Relasinya ke periode memakai <c>Restrict</c>, bukan <c>Cascade</c>, karena riwayat
    /// persetujuan adalah bukti audit dan tidak boleh ikut terhapus bersama periodenya.
    ///
    /// Aturan yang ditegakkan <b>service</b>, bukan di sini:
    /// <list type="bullet">
    /// <item>penyetuju tidak boleh sama dengan pengaju (<c>ACC-DEC-052</c>, meneruskan
    /// <c>ACC-DEC-016</c>);</item>
    /// <item><see cref="ActionNote"/> wajib diisi untuk <c>Rejected</c>;</item>
    /// <item><see cref="ActionSequence"/> diisi berurutan mulai 1 per periode.</item>
    /// </list>
    /// </summary>
    [Table("AccPeriodClosingApproval", Schema = "public")]
    public class AccPeriodClosingApproval : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid AccountingPeriodId { get; set; }

        /// <summary>
        /// Nomor urut tindakan di dalam satu periode, mulai dari 1. Satu periode dapat menempuh
        /// beberapa putaran pengajuan bila penutupannya pernah ditolak.
        /// </summary>
        public int ActionSequence { get; set; }

        public PeriodClosingAction Action { get; set; }

        /// <summary>
        /// Pelaku tindakan.
        /// </summary>
        [Required]
        public Guid ActionBy { get; set; }

        public DateTime ActionAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Wajib untuk <c>Rejected</c>; penegakannya di service.
        /// </summary>
        [MaxLength(500)]
        public string? ActionNote { get; set; }

        public AccAccountingPeriod? AccountingPeriod { get; set; }
    }
}
