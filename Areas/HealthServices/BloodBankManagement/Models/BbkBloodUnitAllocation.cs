using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Models
{
    /// <summary>
    /// Pengikatan satu kantong darah pada satu baris kebutuhan order — jawaban "kantong ini
    /// disiapkan untuk pasien yang mana, atas permintaan baris order yang mana, sejak kapan, dan
    /// oleh siapa" (<c>BD-AGG-03</c>, <c>DEC-BD-003</c>, <c>DEC-BD-007</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Hanya bertambah, sama seperti riwayat penempatan.</b> Tidak ada jalur bisnis yang
    /// menghapus baris ini. Pembatalan alokasi memindahkan
    /// <see cref="BbkAllocationStatus.Active"/> → <see cref="BbkAllocationStatus.Cancelled"/> lalu
    /// mengisi <see cref="CancelReasonCode"/>, <see cref="CancelReasonNote"/>,
    /// <see cref="CancelledByUserId"/>, dan <see cref="CancelledAt"/> — barisnya tetap ada
    /// (<c>ARCH-BD-POS-03</c>, <c>AC-BD-043</c>).
    /// </para>
    ///
    /// <para>
    /// <b>Contoh yang menjelaskan kenapa riwayatnya wajib utuh.</b> Kantong PRC dialokasikan ke
    /// baris order Pasien A pukul 09.00. Pukul 09.40 petugas menyadari kantong itu keliru dan
    /// membatalkannya dengan alasan <c>SEED-BATAL-ALOKASI</c>. Pukul 10.00 kantong yang sama
    /// dialokasikan ke baris order Pasien B. Tiga bulan kemudian ada pertanyaan audit: "kantong ini
    /// pernah disiapkan untuk siapa saja?" Jawabannya dua baris — A yang dibatalkan beserta
    /// alasannya, dan B yang berlaku. Bila baris pertama dihapus saat pembatalan, pertanyaan itu
    /// tidak dapat dijawab sama sekali.
    /// </para>
    ///
    /// <para>
    /// <b>Paling banyak satu baris <see cref="BbkAllocationStatus.Active"/> per kantong</b>
    /// (<c>INV-BD-019</c>). Penjaganya <b>dua lapis</b>, dan keduanya perlu: index unik terfilter
    /// <c>IX_BbkBloodUnitAllocation_ActiveUnit</c> di database, ditambah token
    /// <c>BbkBloodUnit.Version</c>. Validasi aplikasi saja tidak cukup — dua permintaan yang
    /// datang pada milidetik yang sama sama-sama membaca "belum ada alokasi aktif" sebelum salah
    /// satu menulis. Yang menolak permintaan kedua adalah database, bukan pembacaan itu
    /// (<c>VAL-BD-018c</c>).
    /// </para>
    ///
    /// <para>
    /// <b><see cref="CancelReasonNote"/> adalah salinan, bukan penunjuk.</b> Teks alasan pada
    /// <c>MstBloodBankReason</c> boleh disunting kelak, sedangkan rekam pembatalan wajib tetap
    /// berbunyi persis seperti saat keputusannya diambil (<c>INV-BD-035</c>) — pola yang sama
    /// dengan <c>BbkTransitionHistory.ReasonNote</c>.
    /// </para>
    ///
    /// <para>
    /// <b>Pasien tidak disalin ke sini.</b> Pasien tujuan dibaca lewat
    /// <see cref="BloodOrderLineId"/> → <c>BbkBloodOrderLine</c> → <c>BbkBloodOrder.PatientId</c>.
    /// Menyalinnya akan membuka jalan bagi dua catatan yang berselisih (<c>QBE-ENT-003</c>,
    /// <c>BD-DOM-20</c>).
    /// </para>
    /// </remarks>
    [Table("BbkBloodUnitAllocation", Schema = "public")]
    public class BbkBloodUnitAllocation : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Kantong yang dialokasikan.</summary>
        [Required]
        public Guid BloodUnitId { get; set; }

        public BbkBloodUnit? BloodUnit { get; set; }

        /// <summary>Baris kebutuhan order yang menjadi tujuan alokasi.</summary>
        [Required]
        public Guid BloodOrderLineId { get; set; }

        public BbkBloodOrderLine? BloodOrderLine { get; set; }

        /// <summary>
        /// Keadaan baris alokasi. Lahir <see cref="BbkAllocationStatus.Active"/>; paling banyak satu
        /// yang aktif per kantong.
        /// </summary>
        public BbkAllocationStatus AllocationStatus { get; set; } = BbkAllocationStatus.Active;

        /// <summary>Petugas yang mengalokasikan. <b>Selalu manusia</b>, dari akun yang login.</summary>
        [Required]
        public Guid AllocatedByUserId { get; set; }

        /// <summary>Sejak kapan kantong terikat pada baris kebutuhan ini.</summary>
        public DateTime AllocatedAt { get; set; }

        /// <summary>
        /// Kode alasan terkendali pembatalan, dari <c>MstBloodBankReason.ReasonCode</c> berkategori
        /// <c>AllocationCancellation</c>. Kosong selama alokasi masih aktif.
        /// </summary>
        [MaxLength(30)]
        public string? CancelReasonCode { get; set; }

        /// <summary>Salinan teks alasan pada saat pembatalan, bukan penunjuk ke master.</summary>
        [MaxLength(500)]
        public string? CancelReasonNote { get; set; }

        /// <summary>Petugas yang membatalkan. Kosong selama alokasi masih aktif.</summary>
        public Guid? CancelledByUserId { get; set; }

        /// <summary>Waktu pembatalan. Kosong selama alokasi masih aktif.</summary>
        public DateTime? CancelledAt { get; set; }
    }
}
