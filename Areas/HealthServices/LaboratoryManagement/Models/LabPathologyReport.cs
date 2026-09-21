using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models
{
    /// <summary>
    /// Satu laporan diagnostik Patologi Anatomi, melekat pada satu <b>pesanan</b>
    /// (<c>LAB-DEC-085</c>, <c>LAB-DC-043</c>, <c>INV-32</c>).
    ///
    /// <b>Kenapa per pesanan, sedangkan Patologi Klinik dan Mikrobiologi per pemeriksaan.</b>
    /// Tempat hasil memang berbeda per disiplin, dan itu keputusan sadar. Patolog menulis
    /// <i>satu</i> narasi diagnostik untuk seluruh bahan yang datang bersama — makroskopik
    /// menggambarkan jaringannya, bukan menggambarkan salah satu pemeriksaan yang dipesan atas
    /// jaringan itu. Rancangan sebelumnya menempatkannya sebagai tiga kolom pada
    /// <see cref="LabExamination"/>, dan <c>LAB-EVD-003</c> membatalkannya.
    ///
    /// <b>NOL kolom status lifecycle, dan ini yang paling dijaga pada entity ini</b>
    /// (<c>INV-36</c>). Tidak ada <c>Draft</c>, <c>Final</c>, maupun <c>Released</c>. Selesai
    /// atau belumnya dibaca dari <see cref="FinalizedAt"/> — sebuah <b>fakta yang tercatat</b>,
    /// bukan janji tentang apa berikutnya. Pola yang sama sudah dipakai <c>S4a</c>, yang membaca
    /// hasil-sudah-diisi dari <c>ResultEnteredAt != null</c>.
    ///
    /// <b>Dan sebabnya bukan kerapian.</b> Bila <c>Final</c> diperlakukan sebagai <i>rilis</i>,
    /// maka <c>LAB-DEC-003</c> — yang menuntut empat mata pada rilis — bertabrakan dengan
    /// <c>LAB-DEC-090</c> pada orang yang sama. <b>Final berarti patolog selesai menulis, BUKAN
    /// hasilnya boleh keluar.</b> Rilis adalah <c>S4e</c>, dan ia masih tertahan
    /// <c>DEC-LAB-011</c>.
    ///
    /// <b>NOL kolom <c>IssuedAt</c> dan <c>EffectiveAt</c></b> (<c>INV-38</c>,
    /// <c>LAB-DEC-092</c>). Artifact memintanya sebagai isian manual; keduanya ditolak karena
    /// sistemnya <b>sudah</b> mencatat kejadiannya — waktu terbit diturunkan dari
    /// <see cref="FinalizedAt"/>, dan waktu efektif dari <c>LabSpecimen.CollectedAt</c>.
    /// Menyimpannya berarti dua sumber kebenaran untuk satu kejadian, dan yang satu pasti akan
    /// menyimpang tanpa ada yang tahu.
    ///
    /// <b>NOL kolom gambar</b> — tertahan <c>DEC-LAB-016</c>.
    /// </summary>
    public class LabPathologyReport : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Pesanan yang dilaporkan. Unik di antara baris yang belum ditandai terhapus —
        /// satu pesanan tepat satu laporan (<c>INV-32</c>).
        /// </summary>
        [Required]
        public Guid LabOrderId { get; set; }

        /// <summary>
        /// Tingkat temuan: Normal, Perlu Perhatian, atau Kritis.
        ///
        /// <b>Nilai, bukan status lifecycle</b> (<c>LAB-DEC-094</c>). Dinilai patolog secara
        /// manual — narasi diagnostik nol punya batas atas dan bawah untuk dibandingkan.
        ///
        /// <b>Sensitif:</b> ia menyatakan tingkat bahaya pasien, sehingga dilarang masuk logger
        /// dan dilarang muncul pada layar non-klinis.
        /// </summary>
        public LabPathologyFindingStatus? FindingStatus { get; set; }

        /// <summary>
        /// Penanggung jawab analis (<c>LAB-DEC-093</c>).
        ///
        /// <b>Sengaja tanpa foreign key</b>, mengikuti <c>ResultEnteredByUserId</c> dan
        /// <c>UrgencyMarkedByUserId</c> beserta peringatan <c>REG-ACTOR-FK</c>. Penulisnya tetap
        /// wajib mengisi <c>null</c>, bukan <see cref="System.Guid.Empty"/>, ketika pelakunya
        /// bukan orang — <c>Guid.Empty</c> tidak akan ditolak database melainkan tersimpan
        /// diam-diam sebagai pelaku yang tidak pernah ada.
        /// </summary>
        public Guid? AnalystUserId { get; set; }

        /// <summary>
        /// Kapan patolog menyatakan laporannya <b>selesai ditulis</b>.
        ///
        /// <b>Ini bukan waktu rilis</b> (<c>LAB-DEC-088</c>, <c>INV-35</c>). Kosong berarti
        /// laporannya masih ditulis; terisi berarti patolog sudah menyatakan diagnosisnya
        /// selesai. Dari sinilah <c>issuedAt</c> diturunkan.
        /// </summary>
        public DateTime? FinalizedAt { get; set; }

        /// <summary>
        /// Siapa yang memfinalkan. Nullable dan <b>tanpa foreign key</b>, sebab yang sama dengan
        /// <see cref="AnalystUserId"/>.
        /// </summary>
        public Guid? FinalizedByUserId { get; set; }

        /// <summary>
        /// Berapa kali laporan ini dibuka kembali sesudah difinalkan.
        ///
        /// <b>Angka ini sendiri tidak cukup, dan itu disadari sejak dirancang.</b> Yang
        /// dibutuhkan tata kelola adalah riwayat <i>per kejadian</i> beserta alasannya —
        /// <c>LAB-PERM-v1</c> rev 7 bagian 9.4 mewajibkan <c>PathologyReport.Reopen</c>
        /// beralasan. Riwayat itu disimpan terpisah sebagai jejak audit, bukan di kolom ini;
        /// kolom ini hanya rekapnya.
        /// </summary>
        public int ReopenCount { get; set; }

        /// <summary>Pesanan yang dilaporkan.</summary>
        public LabOrder? LabOrder { get; set; }

        /// <summary>Nilai per parameter pada laporan ini.</summary>
        public ICollection<LabPathologyReportValue> Values { get; set; } = new List<LabPathologyReportValue>();
    }
}
