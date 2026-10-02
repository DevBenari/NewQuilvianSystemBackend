using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models
{
    /// <summary>
    /// Golongan pemeriksaan Patologi Anatomi yang menentukan <b>bentuk formulir hasilnya</b> —
    /// Histologi, Sitologi Ginekologi, Sitologi Non-Ginekologi, dan Imunohistokimia
    /// (<c>LAB-DEC-086</c>, <c>LAB-DC-044</c>).
    ///
    /// <b>Kenapa tabel, bukan enum.</b> Bentuk laporan Patologi Anatomi berbeda per golongan:
    /// Histologi memakai tiga ruas, sedangkan Imunohistokimia memakai sepuluh. Sebagai enum,
    /// golongan kelima menuntut rilis backend; sebagai tabel, ia cukup menambah satu baris.
    /// <c>LAB-EVD-003</c> sendiri sudah membuktikan daftarnya belum selesai — BR-23 mengira
    /// seluruh Patologi Anatomi berbentuk tiga ruas, dan ternyata tidak.
    ///
    /// <b>Yang tidak diurus tabel ini.</b> Ia nol menyatakan pemeriksaan mana masuk golongan
    /// mana; itu milik <see cref="LabProcedurePathologyCategory"/>. Ia juga nol menyatakan ruas
    /// apa saja yang berlaku; itu milik <see cref="LabPathologyParameterCategory"/>.
    ///
    /// <b>Nol penghapusan.</b> Golongan yang pernah dipakai laporan adalah bagian riwayat
    /// diagnostik pasien; ia dinonaktifkan lewat <see cref="IsActive"/>, bukan dihapus
    /// (<c>INV-37</c>).
    /// </summary>
    public class LabPathologyCategory : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Kode golongan. Dinormalkan menjadi huruf kapital dan unik di antara baris yang belum
        /// ditandai terhapus (<c>VAL-101</c>).
        /// </summary>
        [Required]
        public string CategoryCode { get; set; } = string.Empty;

        /// <summary>Nama yang dilihat petugas saat memilih golongan.</summary>
        [Required]
        public string CategoryName { get; set; } = string.Empty;

        /// <summary>Urutan tampil pada daftar pilihan.</summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// Golongan yang tidak lagi dipakai <b>dinonaktifkan</b>, bukan dihapus. Laporan lama
        /// yang menunjuk ke sini tetap terbaca utuh (<c>INV-37</c>).
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
