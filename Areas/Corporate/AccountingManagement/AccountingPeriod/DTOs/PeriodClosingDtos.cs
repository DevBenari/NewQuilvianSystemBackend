using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.DTOs
{
    /// <summary>
    /// Daftar periksa penutupan sebuah periode: apa yang menahan penutupan, dan apa yang perlu
    /// diperhatikan tetapi boleh dilewati (<c>ACC-DEC-051</c>).
    /// </summary>
    /// <remarks>
    /// <b>Isinya dihitung saat diminta dan tidak pernah disimpan.</b> Menyimpan hasilnya akan
    /// membuat penutupan ditolak berdasarkan keadaan yang sudah berubah — misalnya jurnal
    /// terakhir baru saja disahkan rekan sebelah, tetapi daftar periksa masih menampilkan angka
    /// setengah jam lalu. <see cref="EvaluatedAt"/> ada supaya layar dapat menunjukkan kapan
    /// angka ini diambil.
    /// </remarks>
    public class PeriodClosingChecklistResponse
    {
        public Guid AccountingPeriodId { get; set; }

        public string PeriodName { get; set; } = string.Empty;

        public AccountingPeriodStatus PeriodStatus { get; set; }

        /// <summary>
        /// Waktu perhitungan ini dijalankan. Selalu berubah setiap kali endpoint dipanggil.
        /// </summary>
        public DateTime EvaluatedAt { get; set; }

        /// <summary>
        /// Benar bila tidak ada satu pun penghalang **yang sudah diperiksa** bernilai lebih dari
        /// nol, dan periode masih berstatus <c>Open</c>.
        /// </summary>
        /// <remarks>
        /// Bernilai benar <b>tidak</b> berarti seluruh pemeriksaan sudah berjalan — periksa
        /// <see cref="IsComplete"/> untuk itu.
        /// </remarks>
        public bool CanSubmitClosing { get; set; }

        /// <summary>
        /// Benar hanya bila **seluruh** butir sudah dapat diperiksa. Selama gelombang `P2-1`
        /// belum berdiri, nilainya <c>false</c>, dan layar sebaiknya menyatakan bahwa daftar
        /// periksa masih sebagian.
        /// </summary>
        public bool IsComplete { get; set; }

        /// <summary>Jumlah penghalang yang benar-benar menahan penutupan saat ini.</summary>
        public int BlockingCount { get; set; }

        /// <summary>Jumlah peringatan yang muncul; tidak menahan apa pun.</summary>
        public int WarningCount { get; set; }

        /// <summary>Jumlah butir yang belum dapat diperiksa, penghalang maupun peringatan.</summary>
        public int NotYetAvailableCount { get; set; }

        /// <summary>
        /// Tiga penghalang <c>ACC-DEC-051</c> beserta perluasannya <c>ACC-DEC-065</c>. Selalu
        /// berisi ketiganya, walaupun sebagiannya belum dapat diperiksa.
        /// </summary>
        public List<PeriodClosingBlockerResponse> Blockers { get; set; } = new();

        /// <summary>
        /// Peringatan yang boleh dilewati. Selalu berisi seluruh butirnya, walaupun sebagiannya
        /// belum dapat diperiksa.
        /// </summary>
        public List<PeriodClosingBlockerResponse> Warnings { get; set; } = new();
    }

    /// <summary>
    /// Satu butir daftar periksa, penghalang maupun peringatan.
    /// </summary>
    public class PeriodClosingBlockerResponse
    {
        /// <summary>
        /// Kode tetap yang dapat dibaca layar, misalnya <c>UNPOSTED_JOURNALS</c>. Sengaja bukan
        /// nomor urut, supaya penambahan butir baru tidak menggeser arti butir yang sudah ada.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        /// <summary>Kalimat yang siap ditampilkan kepada pengguna.</summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Jumlah temuan. Bernilai nol bila bersih, dan **juga** nol bila
        /// <see cref="State"/> bernilai <c>NotYetAvailable</c>.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Benar bila butir ini menahan penutupan. Peringatan selalu <c>false</c>.
        /// </summary>
        public bool IsBlocking { get; set; }

        public PeriodChecklistItemState State { get; set; } = PeriodChecklistItemState.Evaluated;

        /// <summary>
        /// Diisi hanya bila <see cref="State"/> bernilai <c>NotYetAvailable</c>: alasan mengapa
        /// butir ini belum dapat diperiksa, beserta gelombang yang akan menyediakannya.
        /// </summary>
        public string? UnavailableReason { get; set; }
    }

    /// <summary>
    /// Satu baris riwayat penutupan periode. Barisnya tidak pernah diubah maupun dihapus.
    /// </summary>
    public class PeriodClosingApprovalResponse
    {
        public Guid Id { get; set; }

        public Guid AccountingPeriodId { get; set; }

        /// <summary>Nomor urut tindakan dalam satu periode, mulai dari 1.</summary>
        public int ActionSequence { get; set; }

        public PeriodClosingAction Action { get; set; }

        public Guid ActionBy { get; set; }

        public DateTime ActionAt { get; set; }

        public string? ActionNote { get; set; }
    }

    /// <summary>Pengajuan penutupan periode oleh Manajer Akuntansi.</summary>
    public class SubmitPeriodClosingRequest
    {
        /// <summary>Catatan pengajuan; tidak wajib.</summary>
        [MaxLength(500)]
        public string? Note { get; set; }
    }

    /// <summary>Persetujuan penutupan oleh penyetuju yang bukan pengaju.</summary>
    public class ApprovePeriodClosingRequest
    {
        [MaxLength(500)]
        public string? Note { get; set; }
    }

    /// <summary>
    /// Penolakan penutupan. <see cref="Reason"/> <b>wajib</b> — periode kembali terbuka, dan
    /// tanpa alasan tertulis tidak ada yang tahu apa yang harus diperbaiki.
    /// </summary>
    public class RejectPeriodClosingRequest
    {
        [MaxLength(500)]
        public string? Reason { get; set; }
    }
}
