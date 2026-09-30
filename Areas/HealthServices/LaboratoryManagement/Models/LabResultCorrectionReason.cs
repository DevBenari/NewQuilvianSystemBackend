using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models
{
    /// <summary>
    /// Daftar terkendali alasan <b>mengubah hasil</b> — dipakai <i>Kembalikan ke analis</i>
    /// (<c>LAB-DEC-138</c>) dan kelak koreksi sesudah rilis (<c>LAB-DEC-082</c>, <c>S6</c>).
    ///
    /// <b>Satu daftar untuk keduanya</b>, supaya "berapa kali sampel tertukar bulan ini"
    /// terhitung dengan satu nama.
    ///
    /// <b>Kenapa tabelnya terpisah dari <see cref="LabFourEyesExceptionReason"/>.</b> Yang satu
    /// menjawab <i>kenapa hasil diubah</i>, yang lain <i>kenapa kewenangan dirangkap</i>
    /// (<c>LAB-DA-001</c> A5.4). Satu tabel berpenanda jenis membuat laporan mutu wajib selalu
    /// ingat menyaring — dan kelupaan sekali menghitung "shift tunggal" sebagai kesalahan hasil.
    ///
    /// <b>Nol penghapusan.</b> Alasan yang pernah dipakai tidak boleh hilang dari riwayat.
    /// Ia dinonaktifkan lewat <see cref="IsActive"/>. Riwayat menyimpan <b>kodenya</b> sebagai
    /// teks (<c>LabTransitionHistory.ReasonCode</c>), bukan lewat foreign key.
    /// </summary>
    public class LabResultCorrectionReason : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Kode alasan, misalnya <c>SAMPEL-TERTUKAR</c>. Unik di antara baris yang belum
        /// ditandai terhapus. <b>Tidak dapat diubah sesudah dibuat</b> — laporan mutu menghitung
        /// per kode.
        /// </summary>
        [Required]
        public string ReasonCode { get; set; } = string.Empty;

        /// <summary>Nama yang dilihat petugas, misalnya <i>Sampel tertukar</i>.</summary>
        [Required]
        public string ReasonName { get; set; } = string.Empty;

        /// <summary>Keterangan kapan alasan ini dipakai.</summary>
        public string? Description { get; set; }

        /// <summary>
        /// Bila <c>true</c>, catatan bebas wajib diisi. <b>Hanya admin sistem</b> yang menyetelnya
        /// (pola <c>LAB-DEC-019</c>, diadopsi <c>LAB-DEC-082</c>).
        /// </summary>
        public bool RequiresNote { get; set; }

        /// <summary>
        /// Alasan nonaktif tidak dapat dipilih pada tindakan <b>baru</b>, tetapi tetap terbaca
        /// pada riwayat lama.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>Urutan tampil pada pilihan.</summary>
        public int SortOrder { get; set; }
    }
}
