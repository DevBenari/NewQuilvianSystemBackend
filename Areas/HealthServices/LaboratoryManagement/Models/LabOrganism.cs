using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models
{
    /// <summary>
    /// Organisme yang dapat ditemukan pada biakan Mikrobiologi — <c>Escherichia coli</c>,
    /// <c>Staphylococcus aureus</c>, dan seterusnya (<c>LAB-DEC-084</c>, <c>LAB-DC-041</c>).
    ///
    /// <b>Kenapa ini data induk terkendali, bukan teks bebas.</b> Sebagai teks bebas,
    /// "E. coli", "E.coli", dan "Escherichia coli" terhitung tiga kuman berbeda — dan laporan
    /// pola kuman rumah sakit, yang menjadi dasar pemilihan antibiotik empiris, tidak pernah
    /// dapat dipercaya. Pola resistensi yang salah hitung berakhir pada terapi yang salah pilih.
    ///
    /// <b>Kenapa milik Laboratorium, bukan master-data.</b> <c>LAB-DEC-084</c> memilihnya atas
    /// dasar bukti modul ini sendiri: <c>LAB-COORD-006</c> dan <c>MST-POS-WRITE</c> dua-duanya
    /// membuktikan data induk yang dititipkan ke modul lain dapat berdiri <b>tanpa satu pun cara
    /// mengisinya</b>. Dua kali pola yang sama sudah cukup; yang ketiga tidak perlu dicoba.
    ///
    /// <b>Prefix <c>Lab</c>, bukan <c>Mst</c></b>, mengikuti baris registry 2026-09-02 yang sama
    /// dengan <see cref="LabValueBound"/> dan <see cref="LabSpecimenType"/>.
    ///
    /// <b>Nol penghapusan.</b> Menghapus organisme yang sudah dipakai berarti menghapus temuan
    /// pasien. Ia dinonaktifkan lewat <see cref="IsActive"/>, dan <c>INV-31</c> menjamin isolat
    /// lama yang menunjuk ke sini tetap sah dan tetap terbaca.
    /// </summary>
    public class LabOrganism : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Kode organisme. Dinormalkan menjadi huruf kapital dan unik di antara baris yang belum
        /// ditandai terhapus (<c>VAL-91</c>). Dipakai integrasi dan pelaporan.
        /// </summary>
        [Required]
        public string OrganismCode { get; set; } = string.Empty;

        /// <summary>Nama kuman yang dilihat analis, misalnya <c>Escherichia coli</c>.</summary>
        [Required]
        public string OrganismName { get; set; } = string.Empty;

        /// <summary>Urutan tampil pada daftar pilihan analis.</summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// Organisme yang tidak lagi dipakai <b>dinonaktifkan</b>, bukan dihapus. Isolat lama
        /// tetap terbaca (<c>INV-31</c>, <c>VAL-85</c>).
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>Keterangan bagi analis.</summary>
        public string? Description { get; set; }
    }
}
