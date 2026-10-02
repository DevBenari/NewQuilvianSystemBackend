using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models
{
    /// <summary>
    /// Satu organisme yang ditemukan pada biakan sebuah pemeriksaan Mikrobiologi
    /// (<c>LAB-DC-036</c>, BR-23).
    ///
    /// <b>Melekat pada PEMERIKSAAN, bukan pada pesanan</b> (<c>LAB-DEC-095</c>). Satu pesanan
    /// dapat memuat Kultur Darah dan Kultur Urin sekaligus, dan keduanya bahan berbeda dengan
    /// pola kuman yang berbeda — menyatukannya membuat laporan pola kuman per jenis bahan
    /// mustahil dihitung.
    ///
    /// <b>Nol isolat adalah keadaan yang sah.</b> Biakan yang tidak menumbuhkan apa pun
    /// menyingkirkan dugaan infeksi bakteri, dan itu temuan yang berguna
    /// (<c>AC-164</c>, <c>LAB-DEC-104</c>).
    /// </summary>
    public class LabMicrobiologyIsolate : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Pemeriksaan yang menemukannya.</summary>
        [Required]
        public Guid LabExaminationId { get; set; }

        /// <summary>
        /// Organisme yang dikenal. <b>Pengetikan bebas ditolak</b> (<c>INV-30</c>,
        /// <c>LAB-DEC-084</c>): teks bebas menulis <c>E. coli</c>, <c>E.coli</c>, dan
        /// <c>Escherichia coli</c> sebagai tiga kuman padahal satu, dan pola resistensi
        /// rumah sakit tidak dapat dihitung dari itu.
        /// </summary>
        [Required]
        public Guid LabOrganismId { get; set; }

        /// <summary>
        /// Nama organisme sebagaimana berlaku <b>saat isolat dicatat</b>.
        ///
        /// Snapshot, bukan penunjuk semata, dengan alasan yang sama seperti
        /// <c>ProcedureNameSnapshot</c>: nama yang diperbaiki kepala instalasi enam bulan
        /// kemudian tidak boleh mengubah isi hasil yang sudah tercetak (<c>AC-110</c>).
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string OrganismNameSnapshot { get; set; } = string.Empty;

        /// <summary>
        /// Apakah kepekaan isolat ini diuji (<c>LAB-DEC-126</c>).
        ///
        /// <b>Bernilai salah untuk kuman yang ditemukan tetapi tidak diuji.</b> Bukti
        /// <c>LAB-EVD-006</c> menampilkan catatan yang menyebut dua kuman sekaligus sedangkan
        /// hanya satu yang bertabel antibiogram. Keduanya tetap disimpan sebagai isolat, sebab
        /// laporan pola kuman hanya dapat menghitung kuman yang tersimpan sebagai data —
        /// ditulis sebagai kalimat, pertanyaan <i>"berapa kali kuman ini tumbuh tahun ini"</i>
        /// hanya dapat dijawab dengan membaca ribuan catatan satu per satu.
        ///
        /// Isolat bertanda salah <b>tidak boleh</b> memiliki baris kepekaan (<c>VAL-117</c>).
        /// </summary>
        public bool IsSusceptibilityTested { get; set; } = true;

        /// <summary>Catatan analis atas isolat ini.</summary>
        [MaxLength(500)]
        public string? Note { get; set; }

        public LabExamination? LabExamination { get; set; }

        public LabOrganism? LabOrganism { get; set; }

        public ICollection<LabIsolateSusceptibility> Susceptibilities { get; set; }
            = new List<LabIsolateSusceptibility>();
    }
}
