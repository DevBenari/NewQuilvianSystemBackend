using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models
{
    /// <summary>
    /// Satu versi isi hasil bacaan.
    ///
    /// <b>Versi yang sudah dirilis tidak pernah diubah dan tidak pernah dihapus.</b> Koreksi
    /// membuat versi baru yang menunjuk pendahulunya lewat <see cref="PreviousVersionId"/>,
    /// dan versi lama berpindah menjadi <c>Superseded</c> dengan isinya tetap utuh.
    ///
    /// Alasannya bukan kerapian data. Sebuah bacaan yang sudah dirilis mungkin sudah dipakai
    /// dokter lain untuk mengambil keputusan — memberi obat, menjadwalkan operasi, memulangkan
    /// pasien. Ketika bacaan itu kemudian dikoreksi, pertanyaan yang harus dapat dijawab bukan
    /// hanya "apa yang benar sekarang", melainkan juga <b>"apa yang dibaca dokter itu waktu
    /// itu"</b>. Versi yang tertimpa menghapus jawaban kedua.
    ///
    /// Tiga kolom isinya — <see cref="Findings"/>, <see cref="Impression"/>, dan
    /// <see cref="Recommendation"/> — beserta <see cref="AmendmentReason"/> adalah kesimpulan
    /// klinis atas seorang pasien. Keempatnya <b>haram masuk application log</b>
    /// (<c>RAD-PERM-001</c> bagian 7).
    /// </summary>
    public class RadReportVersion : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid RadReportId { get; set; }

        /// <summary>Mulai dari <c>1</c>. Tidak boleh kembar dalam satu bacaan.</summary>
        public int VersionNumber { get; set; }

        /// <summary>
        /// Versi yang digantikan versi ini. Kosong berarti versi pertama. Rantai inilah yang
        /// membuat riwayat koreksi dapat ditelusuri mundur sampai bacaan aslinya.
        /// </summary>
        public Guid? PreviousVersionId { get; set; }

        public RadReportVersionStatus VersionStatus { get; set; } =
            RadReportVersionStatus.Drafted;

        /// <summary>Bernilai <c>true</c> untuk versi kedua dan seterusnya.</summary>
        public bool IsAmendment { get; set; }

        /// <summary><b>Sensitif.</b> Uraian temuan pada citra.</summary>
        public string? Findings { get; set; }

        /// <summary>
        /// <b>Sensitif.</b> Kesimpulan bacaan — inilah yang benar-benar dibaca dokter pengirim,
        /// dan karena itu wajib diisi.
        /// </summary>
        [Required]
        public string Impression { get; set; } = string.Empty;

        /// <summary><b>Sensitif.</b> Saran tindak lanjut.</summary>
        public string? Recommendation { get; set; }

        [Required]
        public Guid AuthorUserId { get; set; }

        /// <summary>
        /// Peran penulis <b>pada saat draf ini ditulis</b>, dibekukan.
        ///
        /// Tidak pernah dibaca ulang dari peran pengguna saat pengesahan. Residen yang
        /// kemudian menjadi dokter radiolog tetap tidak boleh mengesahkan draf yang ia tulis
        /// semasa menjadi residen — <c>RAD-DEC-003</c>.
        /// </summary>
        public RadReportAuthorRole AuthorRoleSnapshot { get; set; }

        public DateTime DraftedAt { get; set; }

        /// <summary>Pengesah. Kosong selama versi ini masih draf.</summary>
        public Guid? ValidatorUserId { get; set; }

        public DateTime? ValidatedAt { get; set; }

        /// <summary>Setelah terisi, isi versi ini <b>tidak boleh berubah</b>.</summary>
        public DateTime? ReleasedAt { get; set; }

        /// <summary>
        /// <b>Sensitif.</b> Alasan koreksi. <b>Wajib</b> diisi bila
        /// <see cref="IsAmendment"/> bernilai <c>true</c> — tanpa itu, pembaca riwayat tidak
        /// tahu mengapa bacaan sebelumnya diganti.
        /// </summary>
        public string? AmendmentReason { get; set; }

        /// <summary>Token konkurensi.</summary>
        public int Version { get; set; }

        public RadReport? RadReport { get; set; }

        public RadReportVersion? PreviousVersion { get; set; }
    }
}
