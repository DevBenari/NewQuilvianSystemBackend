using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models
{
    /// <summary>
    /// Hasil bacaan radiologi atas satu study — wadah identitas dan status, bukan isinya.
    ///
    /// <b>Isi bacaan sengaja tidak disimpan di sini.</b> Yang dibaca dokter pengirim adalah
    /// kesimpulan pada versi yang sedang berlaku, dan bacaan dapat dikoreksi berkali-kali.
    /// Menyimpan isinya di baris ini berarti setiap koreksi menimpa isi sebelumnya — dan
    /// riwayat klinis yang tertimpa tidak dapat dikembalikan. Isi setiap versi tinggal di
    /// <see cref="RadReportVersion"/>.
    ///
    /// <b>Tidak ada kunci asing dari sini ke versi yang berlaku.</b> Kunci semacam itu
    /// melingkar — induk menunjuk versi, versi menunjuk induk — dan membuat penyisipan versi
    /// pertama mustahil dilakukan dalam satu transaksi tanpa kolom sementara yang kosong.
    /// Sebagai gantinya <see cref="CurrentVersionNumber"/> menyimpan nomornya saja.
    ///
    /// Satu study paling banyak punya satu bacaan. Aturan itu dijaga index unik pada database,
    /// bukan hanya oleh service — <c>RAD-ERD-REP-001</c>.
    /// </summary>
    public class RadReport : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Study yang dibaca. Satu study paling banyak satu bacaan.</summary>
        [Required]
        public Guid RadStudyId { get; set; }

        /// <summary>
        /// Disalin dari study supaya pencarian bacaan per pesanan tidak perlu menggabungkan
        /// tabel. Nilainya tidak pernah berubah setelah bacaan dibuat.
        /// </summary>
        [Required]
        public Guid RadOrderId { get; set; }

        /// <summary>
        /// Disalin dari study. Pemiliknya tetap Registration Management; kolom ini hanya
        /// salinan untuk pencarian.
        /// </summary>
        [Required]
        public Guid EncounterId { get; set; }

        /// <summary>Nomor bacaan yang terbaca manusia. Tidak boleh kembar.</summary>
        [Required]
        public string ReportNumber { get; set; } = string.Empty;

        public RadReportStatus ReportStatus { get; set; } = RadReportStatus.Pending;

        /// <summary>
        /// Nomor versi yang sedang berlaku. Bernilai <c>0</c> berarti belum ada draf sama
        /// sekali — bacaan sudah ditunggu, tetapi belum ada yang menuliskannya.
        /// </summary>
        public int CurrentVersionNumber { get; set; }

        /// <summary>
        /// Kapan bacaan pertama kali sampai ke dokter pengirim. Tidak berubah oleh koreksi
        /// berikutnya: pertanyaan "sejak kapan hasilnya tersedia" punya satu jawaban.
        /// </summary>
        public DateTime? FirstReleasedAt { get; set; }

        /// <summary>Kapan versi terakhir dirilis.</summary>
        public DateTime? LastReleasedAt { get; set; }

        /// <summary>Token konkurensi. Dua pengesahan bersamaan tidak boleh sama-sama berhasil.</summary>
        public int Version { get; set; }

        public RadStudy? RadStudy { get; set; }

        public RadOrder? RadOrder { get; set; }

        public ICollection<RadReportVersion> Versions { get; set; } =
            new List<RadReportVersion>();
    }
}
