using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models
{
    /// <summary>
    /// Satu ruas isian pada laporan Patologi Anatomi — <c>Makroskopik</c>, <c>Kesimpulan</c>,
    /// <c>HER2</c>, dan seterusnya (<c>LAB-DEC-086</c>, <c>LAB-DC-045</c>).
    ///
    /// <b>Kenapa ruas laporan menjadi data induk, bukan kolom.</b> Rancangan sebelumnya
    /// menempatkan makroskopik, mikroskopik, dan kesimpulan sebagai tiga kolom pada
    /// <see cref="LabExamination"/>. <c>LAB-EVD-003</c> membatalkannya: Imunohistokimia memakai
    /// sepuluh ruas yang seluruhnya berbeda, dan Sitologi Ginekologi memakai tiga ruas yang juga
    /// berbeda. Sebagai kolom, setiap golongan baru menuntut migration; sebagai data induk, ia
    /// menuntut satu baris.
    ///
    /// <b>Satu parameter dapat dipakai lebih dari satu golongan, dan itu bukan kebetulan.</b>
    /// <c>Anjuran</c> muncul pada Sitologi Ginekologi <i>dan</i> Imunohistokimia; makroskopik,
    /// mikroskopik, serta kesimpulan muncul pada Histologi <i>dan</i> Sitologi Non-Ginekologi.
    /// Justru inilah sebab keberlakuannya dipisahkan ke
    /// <see cref="LabPathologyParameterCategory"/> alih-alih ditulis sebagai kolom golongan di
    /// sini — kolom golongan tunggal akan memaksa <c>Anjuran</c> disalin menjadi dua baris, dan
    /// dua baris berarti dua kode berbeda untuk satu ruas yang sama.
    ///
    /// <b>Nol penghapusan.</b> Menghapus parameter yang sudah dipakai berarti menghapus isi
    /// laporan diagnostik pasien. Parameter dinonaktifkan; nilai lama yang menunjuk ke sini
    /// tetap terbaca utuh (<c>INV-37</c>, <c>VAL-99</c>).
    /// </summary>
    public class LabPathologyParameter : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Kode ruas. Dinormalkan menjadi huruf kapital dan unik di antara baris yang belum
        /// ditandai terhapus (<c>VAL-101</c>).
        /// </summary>
        [Required]
        public string ParameterCode { get; set; } = string.Empty;

        /// <summary>Label yang dilihat patolog pada formulir laporan.</summary>
        [Required]
        public string ParameterName { get; set; } = string.Empty;

        /// <summary>
        /// Urutan ruas pada formulir. Berlaku lintas golongan: <c>Anjuran</c> berurutan
        /// paling akhir pada kedua golongan yang memakainya.
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// Parameter nonaktif nol dapat dipakai pada nilai <b>baru</b> (<c>VAL-99</c>), dan
        /// nilai lama yang sudah menunjuk ke sini tetap terbaca (<c>INV-37</c>).
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
