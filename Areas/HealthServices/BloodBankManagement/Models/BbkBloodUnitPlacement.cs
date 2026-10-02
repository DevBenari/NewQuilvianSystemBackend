using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Models
{
    /// <summary>
    /// Satu penempatan kantong darah pada satu lokasi penyimpanan — jawaban "kantong ini ada di
    /// kulkas mana, sejak kapan, dan siapa yang menaruhnya" (<c>BD-DOM-25</c>, <c>DEC-BD-036</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Hanya bertambah (<c>INV-BD-026</c>).</b> Tidak ada jalur bisnis yang mengubah atau
    /// menghapus baris ini, termasuk ketika kantong salah taruh; salah taruh diperbaiki dengan
    /// menambah penempatan baru. Satu-satunya kolom yang berpindah nilai adalah
    /// <see cref="IsCurrent"/>, dari <c>true</c> ke <c>false</c>, ketika penempatan berikutnya lahir.
    /// </para>
    ///
    /// <para>
    /// <b>Penempatan pertama dan perpindahan berbentuk sama.</b> Pembedanya hanya
    /// <see cref="PreviousPlacementId"/>: kosong pada penempatan pertama, terisi pada perpindahan.
    /// Dua tabel untuk satu pertanyaan yang sama sengaja ditolak arsitektur.
    /// </para>
    ///
    /// <para>
    /// <b>Contoh.</b> Kantong PRC diterima Senin pukul 08.00 dan ditaruh di Kulkas Besar pukul
    /// 08.10 — satu baris, <see cref="PreviousPlacementId"/> kosong. Selasa siang Kulkas Besar rusak
    /// dan petugas memindahkannya ke Kulkas Kecil — baris kedua yang menunjuk baris pertama. Baris
    /// pertama tetap ada dengan <see cref="IsCurrent"/> <c>false</c>, sehingga riwayat "Senin
    /// 08.10 sampai Selasa siang di Kulkas Besar" tetap terbaca.
    /// </para>
    ///
    /// <para>
    /// <b>Tidak ada kolom <c>RemovedAt</c></b> — rentang waktu terbaca dari <see cref="PlacedAt"/>
    /// penempatan berikutnya, sesuai kamus data.
    /// </para>
    /// </remarks>
    [Table("BbkBloodUnitPlacement", Schema = "public")]
    public class BbkBloodUnitPlacement : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Kantong yang ditempatkan.</summary>
        [Required]
        public Guid BloodUnitId { get; set; }

        public BbkBloodUnit? BloodUnit { get; set; }

        /// <summary>
        /// Lokasi penyimpanan. Wajib <b>aktif pada saat penempatan dibuat</b> (<c>INV-BD-027</c>);
        /// lokasi yang kelak dinonaktifkan tidak mengubah baris ini.
        /// </summary>
        [Required]
        public Guid StorageLocationId { get; set; }

        public MstBloodStorageLocation? StorageLocation { get; set; }

        /// <summary>Penempatan sebelumnya. Kosong pada penempatan pertama.</summary>
        public Guid? PreviousPlacementId { get; set; }

        public BbkBloodUnitPlacement? PreviousPlacement { get; set; }

        /// <summary>Sejak kapan kantong berada di lokasi ini.</summary>
        public DateTime PlacedAt { get; set; }

        /// <summary>
        /// Petugas yang menaruh atau memindahkan. <b>Selalu manusia</b> — sistem tidak pernah
        /// menjadi pelaku perpindahan (<c>DEC-BD-037</c>).
        /// </summary>
        public Guid PlacedByUserId { get; set; }

        /// <summary>
        /// Penempatan yang sedang berlaku. Paling banyak satu <c>true</c> per kantong, dijaga index
        /// unik terfilter database (<c>INV-BD-026</c>).
        /// </summary>
        public bool IsCurrent { get; set; } = true;

        /// <summary>
        /// Keterangan bebas. <b>Bukan</b> alasan terkendali — kontrak tidak menuntut alasan pada
        /// perpindahan lokasi.
        /// </summary>
        [MaxLength(500)]
        public string? Note { get; set; }
    }
}
