using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models
{
    /// <summary>
    /// Satu aturan yang menyatakan kombinasi mana pada antibiogram tergolong <b>kritis</b>
    /// (<c>LAB-DEC-103</c>).
    ///
    /// <b>Bentuknya diputuskan pemilik modul; ISINYA diisi wewenang klinis Mikrobiologi</b>
    /// (<c>DR-LAB-002</c>). Pemisahan itu disengaja: bentuk dan isi adalah dua wewenang
    /// berbeda, dan memisahkannya membuat <c>S4b</c> dapat berjalan tanpa menunggu daftar
    /// klinisnya selesai.
    ///
    /// <b>Kenapa BUKAN "semua <c>R</c> berarti kritis".</b> Resistensi terhadap satu
    /// antibiotik cadangan bukan kegawatan; sebaliknya MRSA yang masih peka terhadap banyak
    /// obat justru wajib dikabarkan segera. Aturan "semua <c>R</c>" membuat penanda menyala
    /// hampir pada setiap hasil, dan <b>alarm yang berbunyi terus-menerus berhenti dibaca
    /// orang</b>.
    ///
    /// <b>Ketiga ruas penilainya boleh kosong, dan kosong berarti "apa saja":</b>
    /// <list type="table">
    /// <item><term>MRSA, —, —</term><description>kuman itu selalu kritis</description></item>
    /// <item><term>—, Meropenem, R</term><description>resisten Meropenem selalu kritis, kuman apa pun</description></item>
    /// <item><term>E. coli, Ceftriaxone, R</term><description>hanya kombinasi itu</description></item>
    /// </list>
    /// Baris yang <b>ketiganya kosong</b> ditolak (<c>VAL-106</c>) — ia berarti seluruh hasil
    /// kritis, dan itu mematikan guna penandanya.
    /// </summary>
    public class LabMicrobiologyCriticalRule : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Kosong berarti <b>kuman apa saja</b>.</summary>
        public Guid? LabOrganismId { get; set; }

        /// <summary>Kosong berarti <b>antibiotik apa saja</b>.</summary>
        public Guid? LabAntibioticId { get; set; }

        /// <summary>Kosong berarti <b>hasil apa saja</b>.</summary>
        public LabSusceptibilityResult? SusceptibilityResult { get; set; }

        /// <summary>
        /// Alasan klinis aturan ini. Bukan wajib, tetapi mengisinya membuat pertanyaan
        /// <i>"kenapa kombinasi ini dianggap kritis"</i> dapat dijawab tanpa bertanya kepada
        /// orang.
        /// </summary>
        [MaxLength(500)]
        public string? RuleNote { get; set; }

        /// <summary>
        /// Aturan yang tidak lagi berlaku <b>dinonaktifkan</b>, bukan dihapus — riwayat
        /// perubahan aturan keselamatan harus dapat ditelusuri.
        /// </summary>
        public bool IsActive { get; set; } = true;

        public LabOrganism? LabOrganism { get; set; }

        public LabAntibiotic? LabAntibiotic { get; set; }
    }
}
