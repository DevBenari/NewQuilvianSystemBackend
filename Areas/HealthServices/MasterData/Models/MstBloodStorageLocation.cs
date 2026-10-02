using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    /// <summary>
    /// Master lokasi penyimpanan darah milik Bank Darah — kulkas darah dan tempat simpan
    /// lain yang benar-benar ada di BDRS.
    /// </summary>
    /// <remarks>
    /// <b>Bukan cold storage farmasi.</b> <c>MstDrugStorageLocation</c> sudah ada dan punya
    /// tipe <c>ColdStorage</c>, sehingga tampak seperti kandidat pakai-ulang. <c>DEC-BD-035</c>
    /// menolaknya secara sadar: master itu berorientasi Farmasi, dimiliki tim lain, dan
    /// membawa atribut obat yang tidak berlaku untuk darah. Penolakan itu dicatat di sini
    /// supaya tidak tersambung tanpa sengaja di kemudian hari.
    ///
    /// <b>Master ini menentukan apakah modul Bank Darah berguna atau tidak.</b> Selama tidak
    /// ada satu pun lokasi aktif, tidak ada kantong yang dapat disimpan, dialokasikan, maupun
    /// diberikan (<c>INV-BD-025</c>). Itu konsekuensi <i>fail-closed</i> yang disengaja, dan
    /// menjadikan pengisian master ini prasyarat go-live — bukan pekerjaan yang bisa menyusul.
    ///
    /// <b>Apa yang sengaja TIDAK ada di sini.</b> Nol kolom suhu, nol kapasitas, nol hierarki
    /// gudang, nol rak/laci. MVP tidak memantau rantai dingin (<c>DEC-BD-035</c>,
    /// <c>AC-BD-064</c>). Status <c>Stored</c> pada kantong menyatakan kantong punya tempat
    /// yang tercatat — <b>bukan</b> menyatakan rantai dinginnya terjaga.
    /// </remarks>
    [Table("MstBloodStorageLocation", Schema = "public")]
    public class MstBloodStorageLocation : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Kode lokasi yang dikenali petugas, unik di seluruh tabel. Contoh <c>KLK-BSR</c>.
        /// </summary>
        /// <remarks>
        /// Kode ini <b>ditulis pengguna</b>, bukan dialokasikan sistem — ia penanda fisik yang
        /// sudah dipakai BDRS sehari-hari, bukan nomor urut dokumen.
        /// </remarks>
        [Required]
        [MaxLength(30)]
        public string StorageLocationCode { get; set; } = string.Empty;

        /// <summary>Nama yang dikenali petugas, misalnya <c>Kulkas Besar</c>.</summary>
        [Required]
        [MaxLength(150)]
        public string StorageLocationName { get; set; } = string.Empty;

        /// <summary>
        /// Penanda yang menutup dua gerbang sekaligus ketika bernilai salah.
        /// </summary>
        /// <remarks>
        /// Ketika lokasi dinonaktifkan, dua hal terjadi ke depan: lokasi itu tidak lagi dapat
        /// menjadi tujuan penyimpanan maupun tujuan perpindahan (<c>INV-BD-027</c>), dan
        /// kantong yang penempatan terakhirnya menunjuk lokasi itu tidak dapat dialokasikan
        /// (<c>INV-BD-028</c>).
        ///
        /// <b>Yang TIDAK terjadi, dan ini penting.</b> Penonaktifan <b>tidak</b> memindahkan
        /// kantong dan <b>tidak</b> mengubah status kantong mana pun (<c>DEC-BD-037</c>).
        /// Perpindahan tetap tindakan manusia yang dikerjakan petugas BDRS.
        ///
        /// <b>Contoh.</b> Kulkas Besar rusak dan dinonaktifkan Selasa siang. Dua belas kantong
        /// yang ada di dalamnya tetap tercatat di sana dengan status yang sama persis. Yang
        /// berubah hanya ini: keduabelasnya tidak dapat dialokasikan sampai petugas
        /// memindahkannya ke kulkas yang aktif.
        ///
        /// Nilai ini <b>dibaca saat gerbang dinilai</b> dan tidak pernah disalin ke kantong.
        /// Menyalinnya akan menuntut penyuntingan massal setiap kali satu kulkas dinonaktifkan.
        /// </remarks>
        public bool IsActive { get; set; } = true;

        [MaxLength(250)]
        public string? Description { get; set; }
    }
}
