using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models
{
    /// <summary>
    /// Modalitas pencitraan.
    ///
    /// Kode yang diisi migration mengikuti kode modalitas DICOM yang berlaku umum — `CR`, `CT`,
    /// `MR`, `US`, dan seterusnya. Itu **kosakata teknis internasional**, bukan SOP rumah sakit,
    /// sehingga aman diisi sebagai baseline tanpa mengarang kebijakan siapa pun.
    ///
    /// Yang **tidak** diisi di sini adalah aturan keselamatannya. Lihat
    /// <see cref="MstRadModalitySafetyRule"/>: menetapkan pemeriksaan mana yang wajib melewati
    /// gerbang apa adalah kebijakan klinis, dan kebijakan klinis tidak boleh lahir dari
    /// migration.
    /// </summary>
    public class MstRadModality : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Kode modalitas, mengikuti kode DICOM bila tersedia.</summary>
        [Required]
        public string ModalityCode { get; set; } = string.Empty;

        [Required]
        public string ModalityName { get; set; } = string.Empty;

        public string? Description { get; set; }

        /// <summary>
        /// Menandai modalitas yang memakai radiasi pengion.
        ///
        /// Penanda ini **tidak** menetapkan gerbang keselamatan apa pun dengan sendirinya. Ia
        /// hanya membantu admin menyaring modalitas ketika menyusun aturan, dan membuat jejak
        /// audit lebih mudah dibaca. Yang mengikat tetap baris aturan yang benar-benar dibuat.
        /// </summary>
        public bool UsesIonisingRadiation { get; set; }

        /// <summary>Menandai modalitas yang dapat memakai media kontras.</summary>
        public bool SupportsContrast { get; set; }

        public bool IsActive { get; set; } = true;

        public int SortOrder { get; set; }

        public ICollection<MstRadModalitySafetyRule> SafetyRules { get; set; } =
            new List<MstRadModalitySafetyRule>();
    }
}
