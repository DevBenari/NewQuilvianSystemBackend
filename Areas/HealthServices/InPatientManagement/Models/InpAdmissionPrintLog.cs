using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models
{
    /// <summary>
    /// Satu kejadian cetak gelang, label, IPD, atau dokumen admisi — kamus data 20.13
    /// (<c>BE-RWI-192</c>, <c>INV-RWA-09</c>). Tambah saja; tidak pernah diubah.
    /// </summary>
    /// <remarks>
    /// "Cetakan ke-n" dihitung dari urutan baris, tidak disimpan. Contoh: gelang Budi dicetak Sari
    /// 09.50 (<c>IsReprint = false</c>); pukul 15.10 Andi mencetak lagi beralasan rusak — baris kedua
    /// <c>IsReprint = true</c>, <c>ReprintReason = Damaged</c>, layar menulis "Cetakan ke-2".
    /// </remarks>
    public class InpAdmissionPrintLog : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid EpisodeId { get; set; }

        /// <summary>Wajib bila <see cref="PrintKind"/> = <c>AdmissionDocument</c>.</summary>
        public Guid? DocumentId { get; set; }

        public InpAdmissionPrintKind PrintKind { get; set; }

        /// <summary>Status dokumen saat dicetak; menentukan penanda cetakan.</summary>
        public InpAdmissionDocumentStatus? DocumentStatusAtPrint { get; set; }

        /// <summary>1–10.</summary>
        public int Copies { get; set; } = 1;

        /// <summary>
        /// Ditetapkan server: sudah ada cetakan sebelumnya untuk kunci yang sama, atau episode
        /// <c>Closed</c>/<c>Cancelled</c>.
        /// </summary>
        public bool IsReprint { get; set; }

        public InpReprintReason? ReprintReason { get; set; }

        /// <summary>Wajib bila alasan <c>Other</c>.</summary>
        public string? ReprintNote { get; set; }

        public Guid PrintedByUserId { get; set; }

        /// <summary>Waktu server.</summary>
        public DateTime PrintedAt { get; set; }

        /// <summary>Klik ganda tidak menambah baris.</summary>
        public string? IdempotencyKey { get; set; }

        public InpEpisode? Episode { get; set; }

        public InpAdmissionDocument? Document { get; set; }
    }
}
