using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models
{
    /// <summary>
    /// Katalog butir gerbang keselamatan radiologi.
    ///
    /// Ini adalah **kosakata**, bukan kebijakan. Isinya menjawab *"pertanyaan keselamatan apa
    /// saja yang dikenal sistem"* — misalnya skrining kehamilan, skrining implan logam, atau
    /// riwayat alergi kontras. Ia **tidak** menjawab *"pemeriksaan mana yang wajib melewati
    /// pertanyaan yang mana"*; jawaban itu ada pada <see cref="MstRadModalitySafetyRule"/> dan
    /// sengaja dibiarkan kosong.
    ///
    /// Pemisahan itu disengaja. Menamai sebuah pertanyaan keselamatan tidak membahayakan siapa
    /// pun; menetapkan bahwa sebuah pemeriksaan boleh berjalan tanpa pertanyaan itu — atau
    /// sebaliknya — adalah keputusan klinis yang tidak boleh lahir dari migration.
    ///
    /// Daftar bawaan yang diisi migration adalah baseline mengikuti praktik umum rumah sakit di
    /// Indonesia sesuai <c>RJ-BIL-DEC-014</c>. Ia **belum diverifikasi** terhadap SOP rumah
    /// sakit ini dan tidak boleh diperlakukan sebagai SOP yang disetujui.
    /// </summary>
    public class MstRadSafetyRequirement : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string RequirementCode { get; set; } = string.Empty;

        [Required]
        public string RequirementName { get; set; } = string.Empty;

        public string? Description { get; set; }

        /// <summary>
        /// Pengelompokan butir, misalnya identitas, radiasi, kontras, implan, atau sedasi.
        /// Hanya untuk penyajian; tidak menentukan perilaku apa pun.
        /// </summary>
        public string? Category { get; set; }

        /// <summary>Menandai butir yang mewajibkan petugas mengisi catatan saat menjawab.</summary>
        public bool RequiresNote { get; set; }

        /// <summary>
        /// Asal-usul butir ini, ditulis apa adanya supaya tidak ada yang mengira baseline
        /// implementasi adalah SOP yang sudah disahkan.
        /// </summary>
        public string? SourceNote { get; set; }

        public bool IsActive { get; set; } = true;

        public int SortOrder { get; set; }
    }
}
