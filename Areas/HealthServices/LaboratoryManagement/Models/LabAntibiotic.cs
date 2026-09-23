using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models
{
    /// <summary>
    /// Antibiotik pada panel uji kepekaan Mikrobiologi — <c>Ceftriaxone</c>,
    /// <c>Meropenem</c>, dan seterusnya (<c>LAB-DEC-084</c>, <c>LAB-DC-042</c>).
    ///
    /// <b>Tabel ini BUKAN data induk obat, dan kemiripan namanya menyesatkan.</b> Ia panel uji
    /// milik laboratorium: daftar antibiotik yang diujikan terhadap sebuah isolat, bukan daftar
    /// obat yang dapat diresepkan. Menautkannya ke formularium farmasi adalah keputusan
    /// tersendiri yang <b>belum</b> diambil, dan nol boleh disimpulkan dari kesamaan nama.
    /// Kamus data bagian 14.2 menulis peringatan itu secara terpisah justru karena ia hal yang
    /// paling mungkin "dirapikan" pemelihara berikutnya.
    ///
    /// <b>Kenapa terkendali.</b> Sama dengan <see cref="LabOrganism"/>: pola resistensi dihitung
    /// dari nama antibiotik, dan ejaan yang beragam membuat hitungannya salah tanpa satu pun
    /// galat terlihat.
    ///
    /// <b>Nol penghapusan.</b> Baris kepekaan yang sudah tercatat menunjuk ke sini; menghapusnya
    /// berarti menghapus hasil uji pasien. Dinonaktifkan lewat <see cref="IsActive"/>, dan
    /// <c>VAL-86</c> hanya menolaknya pada baris <b>baru</b>.
    /// </summary>
    public class LabAntibiotic : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Kode antibiotik pada panel uji. Dinormalkan menjadi huruf kapital dan unik di antara
        /// baris yang belum ditandai terhapus (<c>VAL-91</c>).
        /// </summary>
        [Required]
        public string AntibioticCode { get; set; } = string.Empty;

        /// <summary>Nama antibiotik yang dilihat analis, misalnya <c>Ceftriaxone</c>.</summary>
        [Required]
        public string AntibioticName { get; set; } = string.Empty;

        /// <summary>Urutan tampil pada panel uji.</summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// Antibiotik yang ditarik dari panel <b>dinonaktifkan</b>, bukan dihapus. Baris
        /// kepekaan lama tetap terbaca (<c>VAL-86</c>).
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>Keterangan bagi analis.</summary>
        public string? Description { get; set; }

        /// <summary>
        /// Kandungan cakram dalam mikrogram pada metode difusi — Ampicillin <c>10</c>,
        /// Cefoperazone <c>75</c>, Fosfomycin <c>200</c> (<c>LAB-DEC-122</c>, bukti
        /// <c>LAB-EVD-006</c> kolom <c>UG</c>).
        ///
        /// <b>Ini sifat CAKRAM, bukan nilai hasil.</b> Ia disalin menjadi snapshot pada baris
        /// kepekaan saat baris dibuat, sehingga cetak ulang tahun depan tetap menampilkan
        /// angka yang dipakai saat pengujian.
        ///
        /// Boleh kosong: antibiotik yang hanya diuji dengan metode dilusi nol punya cakram.
        /// </summary>
        public int? DiscContentUg { get; set; }
    }
}
