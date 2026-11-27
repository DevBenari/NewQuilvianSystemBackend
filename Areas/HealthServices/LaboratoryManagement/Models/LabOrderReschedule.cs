using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models
{
    /// <summary>
    /// Satu penjadwalan ulang pesanan laboratorium yang <b>berhasil</b> (<c>LAB-DC-067</c>,
    /// <c>LAB-DEC-252</c>): tanggal rencana lama, tanggal rencana baru, alasan, pelaku, dan waktunya.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Baris tidak pernah diubah dan tidak dihapus. Penjadwalan ulang yang ditolak tidak meninggalkan
    /// baris (<c>INV-69</c>), dan pengisian tanggal rencana pesanan lama oleh migration juga tidak —
    /// riwayat ini hanya mencatat keputusan manusia (<c>02-backend-architecture.md</c> 33.10 no. 5).
    /// </para>
    /// <para>
    /// Sengaja tidak memakai <see cref="LabTransitionHistory"/>: tabel itu mencatat perpindahan status
    /// dan dibaca Laporan Pembatalan, sedangkan penjadwalan ulang tidak mengubah status
    /// (<c>LAB-DEC-257</c>, 33.10 no. 1).
    /// </para>
    /// </remarks>
    [Table("LabOrderReschedule", Schema = "public")]
    public class LabOrderReschedule : IdentityModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Pesanan yang dijadwalkan ulang.</summary>
        public Guid LabOrderId { get; set; }

        /// <summary>Tanggal rencana sebelum dipindah — tanggal kalender WIB.</summary>
        public DateOnly PreviousPlannedDate { get; set; }

        /// <summary>Tanggal rencana sesudah dipindah — tanggal kalender WIB.</summary>
        public DateOnly NewPlannedDate { get; set; }

        /// <summary>
        /// Alasan penjadwalan ulang, teks bebas yang sudah di-<i>trim</i> (<c>LAB-DEC-256</c>). Dapat
        /// menyebut kondisi pasien — jangan dicatat ke log.
        /// </summary>
        [MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;

        /// <summary>Pengguna terautentikasi yang menjadwalkan ulang.</summary>
        public Guid ActorUserId { get; set; }

        /// <summary>Waktu penjadwalan ulang, UTC.</summary>
        public DateTime OccurredAt { get; set; }

        public LabOrder? LabOrder { get; set; }
    }
}
