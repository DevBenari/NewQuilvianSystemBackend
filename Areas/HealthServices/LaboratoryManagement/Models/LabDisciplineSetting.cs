using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models
{
    /// <summary>
    /// Apa yang tercetak pada footer satu disiplin (<c>LAB-DEC-119</c>, <c>LAB-DEC-127</c>).
    ///
    /// <b>Tiga barisnya tetap tiga.</b> Satu per disiplin, dan hanya isinya yang berubah — itu
    /// sebabnya endpoint-nya nol <c>POST</c> dan nol <c>DELETE</c> (<c>LAB-API-v1</c> <c>r27</c>
    /// bagian 22.7).
    ///
    /// <b>Satu tabel, bukan tiga pengaturan terpisah.</b> Ketiganya menjawab pertanyaan yang
    /// sama — <i>apa yang tercetak di footer disiplin ini</i> — dan memisahkannya berarti tiga
    /// tempat yang harus diingat bersamaan ketika kop rumah sakit berubah.
    /// </summary>
    public class LabDisciplineSetting : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Disiplin yang diatur. Unik di antara baris yang belum terhapus.</summary>
        [Required]
        public LabDiscipline Discipline { get; set; }

        /// <summary>
        /// Label yang tercetak mendahului nama konsultan, dan <b>berbeda per disiplin</b>:
        /// <c>Konsultan Mikrobiologi Klinik</c>, <c>Spesialis Patologi Anatomi</c>, dan
        /// <c>Konsultan</c> pada Patologi Klinik (<c>LAB-EVD-005</c>).
        /// </summary>
        [Required]
        [MaxLength(150)]
        public string ConsultantLabel { get; set; } = string.Empty;

        /// <summary>
        /// Nama konsultan yang tercetak.
        ///
        /// <b>Ia BUKAN diturunkan dari pemegang wewenang klinis.</b> Footer Mikrobiologi
        /// mencetak <i>Usman Chatib Warsa</i> sedangkan <c>DR-LAB-002</c> adalah
        /// <i>dr. Nabila Rahmawati</i> — dua peran berbeda yang kebetulan sama-sama
        /// Mikrobiologi Klinik. Menurunkan satu dari yang lain akan mencetak nama yang salah
        /// pada dokumen yang dipegang pasien (<c>LAB-DEC-119</c>).
        /// </summary>
        [MaxLength(200)]
        public string? ConsultantName { get; set; }

        /// <summary>
        /// Kalimat penjelas baku yang tercetak sendiri pada setiap hasil disiplin ini —
        /// <c>LEBAR ZONA ANTIBIOTIK TIDAK MEMPENGARUHI TINGKAT KEPEKAAN BAKTERI.</c> pada
        /// Mikrobiologi.
        ///
        /// <b>Terpisah dari catatan bebas analis, yang tetap ada di bawahnya.</b> Diketik ulang
        /// setiap kali, suatu hari ia terlupa pada hasil yang justru membutuhkannya — dan salah
        /// ketik pada kalimat klinis nol akan tertangkap siapa pun (<c>LAB-DEC-127</c>).
        /// </summary>
        [MaxLength(1000)]
        public string? StandingNote { get; set; }

        /// <summary>
        /// Teks yang mendahului nomor cetak. Kosong pada ketiga baris awal.
        ///
        /// <b>Ia nol menentukan pemisah maupun lebar nomor</b> — lihat
        /// <see cref="Services.LabReportNumberService"/> dan <c>LAB-OPEN-043</c>.
        /// </summary>
        [MaxLength(20)]
        public string? ReportNumberPrefix { get; set; }

        /// <summary>Pengaturan yang tidak dipakai dinonaktifkan, bukan dihapus.</summary>
        public bool IsActive { get; set; } = true;
    }
}
