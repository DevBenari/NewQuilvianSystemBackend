using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models
{
    /// <summary>
    /// Rentang breakpoint uji kepekaan bagi satu pasangan <b>organisme dan antibiotik</b>
    /// (<c>LAB-DEC-122</c>, bukti <c>LAB-EVD-006</c> kolom <c>R-S</c>).
    ///
    /// <b>Inilah yang membuat interpretasi dapat dihitung.</b> Zona di bawah
    /// <see cref="LowerMm"/> menghasilkan <c>Resistant</c>, di dalam rentang menghasilkan
    /// <c>Intermediate</c>, di atas <see cref="UpperMm"/> menghasilkan <c>Sensitive</c>
    /// (<c>LAB-DEC-123</c>).
    ///
    /// <b>Per ORGANISME, bukan per antibiotik saja.</b> CLSI menetapkannya begitu: rentang
    /// Ampicillin terhadap <c>Branhamella catarrhalis</c> tidak sama dengan terhadap
    /// <c>Escherichia coli</c>. Menyederhanakannya menjadi per antibiotik berarti melaporkan
    /// kepekaan yang salah untuk sebagian kuman, tanpa satu pun galat terlihat.
    ///
    /// <b>Isinya diisi wewenang klinis Mikrobiologi</b> (<c>DR-LAB-002</c>), bukan kepala
    /// instalasi dan bukan implementer (<c>LAB-PERM-v1</c> rev 9 bagian 11.2). Menggeser batas
    /// bawah dari <c>13</c> menjadi <c>12</c> mengubah sebagian hasil dari <c>R</c> menjadi
    /// <c>I</c> — tanpa satu pun hasil disunting.
    ///
    /// <b>Angka cutoff NOL boleh dihardcode ke kode</b> (<c>LAB-DEC-039</c>). Tabel inilah
    /// tempatnya, dan <see cref="GuidelineVersion"/> mencatat versi panduan yang menjadi
    /// dasarnya.
    /// </summary>
    public class LabSusceptibilityBreakpoint : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Organisme yang dibatasi rentang ini.</summary>
        [Required]
        public Guid LabOrganismId { get; set; }

        /// <summary>Antibiotik yang dibatasi rentang ini.</summary>
        [Required]
        public Guid LabAntibioticId { get; set; }

        /// <summary>
        /// Batas bawah dalam milimeter. Zona <b>di bawah</b> nilai ini menghasilkan
        /// <c>Resistant</c>.
        /// </summary>
        [Required]
        public int LowerMm { get; set; }

        /// <summary>
        /// Batas atas dalam milimeter. Zona <b>di atas</b> nilai ini menghasilkan
        /// <c>Sensitive</c>; di antara keduanya menghasilkan <c>Intermediate</c>.
        ///
        /// Tidak boleh lebih kecil daripada <see cref="LowerMm"/> (<c>VAL-115</c>).
        /// </summary>
        [Required]
        public int UpperMm { get; set; }

        /// <summary>
        /// Versi panduan yang menjadi dasar rentang ini — misalnya <c>CLSI M100 ED34</c>.
        ///
        /// Boleh kosong, tetapi mengisinya membuat pertanyaan <i>"rentang ini dari mana"</i>
        /// dapat dijawab tanpa bertanya kepada orang.
        /// </summary>
        [MaxLength(100)]
        public string? GuidelineVersion { get; set; }

        /// <summary>
        /// Rentang yang tidak lagi dipakai <b>dinonaktifkan</b>, bukan dihapus — baris hasil
        /// lama menyimpan snapshot-nya, dan riwayat perubahan rentang adalah riwayat
        /// keselamatan.
        /// </summary>
        public bool IsActive { get; set; } = true;

        public LabOrganism? LabOrganism { get; set; }

        public LabAntibiotic? LabAntibiotic { get; set; }
    }
}
