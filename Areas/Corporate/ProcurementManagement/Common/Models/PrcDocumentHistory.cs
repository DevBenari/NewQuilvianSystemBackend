using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Models
{
    /// <summary>
    /// Riwayat aksi dan perpindahan status seluruh dokumen Pengadaan —
    /// <c>perencanaan-permintaan/data/data-dictionary.md</c> bagian 5 (BE-PBJ-002).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Rujukan ke dokumen bersifat polimorfik (<see cref="DocumentType"/> +
    /// <see cref="DocumentId"/>), tanpa FK, sehingga satu tabel melayani PR, rencana, rencana
    /// paket, pagu, dan katalog. Sub-modul lain menulis lewat
    /// <c>ProcurementDocumentHistoryWriter</c>, tidak langsung ke tabel ini.
    /// </para>
    /// <para>
    /// Baris hanya ditambah, tidak pernah diubah atau dihapus. Contoh: PR-000031 yang diajukan,
    /// dikembalikan, lalu diajukan ulang menghasilkan tiga baris: <c>SUBMIT</c>
    /// (DRAFT → SUBMITTED), <c>RETURN</c> (SUBMITTED → RETURNED, beserta alasannya), dan
    /// <c>SUBMIT</c> (RETURNED → SUBMITTED).
    /// </para>
    /// </remarks>
    [Table("PrcDocumentHistory", Schema = "public")]
    public class PrcDocumentHistory : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Jenis dokumen dari <c>ProcurementDocumentTypes</c>, contoh <c>PBJ_PR</c>.</summary>
        [Required]
        [MaxLength(40)]
        public string DocumentType { get; set; } = string.Empty;

        [Required]
        public Guid DocumentId { get; set; }

        /// <summary>Nomor dokumen saat aksi terjadi (bila sudah ada), untuk pencarian riwayat.</summary>
        [MaxLength(30)]
        public string? DocumentNumber { get; set; }

        /// <summary>Kode aksi dari <c>ProcurementDocumentHistoryActions</c>, contoh <c>SUBMIT</c>.</summary>
        [Required]
        [MaxLength(40)]
        public string ActionCode { get; set; } = string.Empty;

        [MaxLength(30)]
        public string? FromStatus { get; set; }

        [MaxLength(30)]
        public string? ToStatus { get; set; }

        /// <summary>SENSITIF. Alasan kembali/tolak/batal/revisi; tidak boleh masuk logger.</summary>
        [MaxLength(1000)]
        public string? Reason { get; set; }

        /// <summary>SENSITIF. Cuplikan nominal dan kuantitas saat aksi (jsonb).</summary>
        public string? SnapshotJson { get; set; }

        [Required]
        public Guid ActorUserId { get; set; }

        public DateTimeOffset OccurredAt { get; set; }

        /// <summary>Instance workflow (Wfl) bila aksi berasal dari keputusan approval.</summary>
        public Guid? WorkflowInstanceId { get; set; }
    }
}
