using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Models
{
    /// <summary>
    /// Riwayat tindakan pada sebuah jurnal. Barisnya <b>tidak pernah</b> diubah maupun dihapus.
    ///
    /// Karena itu relasinya ke jurnal memakai <c>Restrict</c>, bukan <c>Cascade</c>: riwayat
    /// persetujuan adalah bukti audit dan tidak boleh ikut terhapus bersama jurnalnya.
    /// </summary>
    [Table("AccJournalApproval", Schema = "public")]
    public class AccJournalApproval : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid JournalId { get; set; }

        public JournalApprovalAction ApprovalAction { get; set; }

        /// <summary>
        /// Pelaku tindakan.
        /// </summary>
        [Required]
        public Guid ActionBy { get; set; }

        public DateTime ActionAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Wajib untuk <c>Rejected</c> dan <c>Reversed</c>; penegakannya di service.
        /// </summary>
        [MaxLength(500)]
        public string? Reason { get; set; }

        public AccJournal? Journal { get; set; }
    }
}
