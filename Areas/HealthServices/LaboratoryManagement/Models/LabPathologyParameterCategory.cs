using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models
{
    /// <summary>
    /// Keberlakuan satu <see cref="LabPathologyParameter"/> pada satu
    /// <see cref="LabPathologyCategory"/>, beserta wajib atau tidaknya ruas itu
    /// (<c>LAB-DEC-086</c>, <c>LAB-DC-046</c>).
    ///
    /// <b>Entity inilah yang menjawab "formulir ini berisi ruas apa saja".</b> Layar hasil
    /// Patologi Anatomi nol menebak bentuk formulirnya: ia membacanya dari sini, sehingga ruas
    /// keenam belas kelak cukup menambah satu baris tanpa rilis frontend.
    ///
    /// <b><see cref="IsRequired"/> adalah penegak <c>INV-34</c>.</b> Kelengkapan laporan diuji
    /// saat finalisasi terhadap kolom ini — bukan terhadap tiga nama kolom yang ditulis di kode.
    /// Perbedaannya menentukan: <c>VAL-88</c> yang menuntut makroskopik, mikroskopik, dan
    /// kesimpulan <b>dicabut</b> pada <c>LAB-VAL-v1</c> <c>r8</c> justru karena ia mengeraskan
    /// tiga nama itu, sedangkan Imunohistokimia nol memakai ketiganya.
    ///
    /// <b>Kenapa entity tersendiri, bukan kolom golongan pada parameter.</b> Satu parameter
    /// dipakai lebih dari satu golongan — <c>Anjuran</c> pada Sitologi Ginekologi dan
    /// Imunohistokimia, dan ketiga ruas Histologi pada Sitologi Non-Ginekologi. Kolom golongan
    /// tunggal akan memaksa ruas yang sama disalin menjadi beberapa baris berkode berbeda.
    /// </summary>
    public class LabPathologyParameterCategory : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Ruas isian yang berlaku.</summary>
        [Required]
        public Guid LabPathologyParameterId { get; set; }

        /// <summary>Golongan pemeriksaan tempat ruas itu berlaku.</summary>
        [Required]
        public Guid LabPathologyCategoryId { get; set; }

        /// <summary>
        /// Apakah ruas ini wajib terisi sebelum laporan dapat difinalkan (<c>INV-34</c>,
        /// <c>VAL-95</c>).
        ///
        /// Bawaannya <c>true</c> mengikuti <c>LAB-EVD-003</c>: seluruh ruas yang tampil sesuai
        /// golongannya dinyatakan wajib. Kolomnya tetap ada supaya kelonggaran per ruas kelak
        /// dapat diberikan tanpa mengubah kode.
        /// </summary>
        public bool IsRequired { get; set; } = true;

        /// <summary>Ruas isian yang berlaku.</summary>
        public LabPathologyParameter? LabPathologyParameter { get; set; }

        /// <summary>Golongan pemeriksaan tempat ruas itu berlaku.</summary>
        public LabPathologyCategory? LabPathologyCategory { get; set; }
    }
}
