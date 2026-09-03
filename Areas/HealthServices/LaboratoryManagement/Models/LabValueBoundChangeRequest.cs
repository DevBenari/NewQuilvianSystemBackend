using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models
{
    /// <summary>
    /// Pengajuan perubahan batas kritis sebuah batas nilai (<c>LAB-DEC-023</c>, BR-19).
    ///
    /// Keberadaan entity ini adalah pengaman keselamatan, bukan sekadar formalitas administrasi.
    /// <c>LAB-DEC-018</c> memberi kepala instalasi kebebasan mengubah isi tabel batas nilai;
    /// <c>LAB-DEC-023</c> mempersempitnya untuk dua kolom yang menentukan kapan seorang pasien
    /// dinyatakan terancam — batas kritis berupa angka, dan penanda pilihan yang dianggap kritis.
    ///
    /// Contoh yang dicegah: kepala instalasi menaikkan batas kritis atas Kalium dari 6,0 menjadi
    /// 8,0 karena merasa peringatan terlalu sering muncul. Sejak saat itu pasien dengan Kalium
    /// 7,2 mmol/L tidak lagi memicu kewajiban pelaporan nilai kritis, tanpa satu pun aturan
    /// dilanggar dan tanpa ada yang menyadarinya.
    ///
    /// Selama pengajuan masih <see cref="LabBoundChangeStatus.Submitted"/>, batas yang berlaku
    /// pada <c>LabValueBound</c> <b>tidak berubah sama sekali</b>. Nilai usulan tinggal di sini,
    /// terpisah dari nilai yang berlaku, justru supaya keduanya tidak mungkin tertukar.
    ///
    /// Penegakan transisi status, larangan menyetujui pengajuan sendiri, dan penolakan pengajuan
    /// kedua saat yang pertama belum diputuskan adalah pekerjaan service pada <c>BE-LAB-05</c>.
    /// </summary>
    public class LabValueBoundChangeRequest : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Batas nilai yang diusulkan berubah.</summary>
        [Required]
        public Guid ValueBoundId { get; set; }

        /// <summary>
        /// Status pengajuan. Lahir sebagai <see cref="LabBoundChangeStatus.Submitted"/>;
        /// ketiga status lainnya bersifat terminal.
        /// </summary>
        public LabBoundChangeStatus RequestStatus { get; set; } = LabBoundChangeStatus.Submitted;

        /// <summary>Usulan batas kritis bawah. Kosong bila yang diusulkan hanya batas atas atau daftar pilihan kritis.</summary>
        public decimal? ProposedCriticalLow { get; set; }

        /// <summary>Usulan batas kritis atas.</summary>
        public decimal? ProposedCriticalHigh { get; set; }

        /// <summary>
        /// Usulan daftar pilihan yang dianggap kritis untuk batas berbentuk pilihan, ditulis
        /// sebagai kode yang dipisah koma — misalnya <c>P3,P4</c>. Disimpan sebagai teks usulan,
        /// bukan sebagai relasi, karena baris ini adalah rekaman niat pada satu saat tertentu
        /// dan tidak boleh ikut berubah ketika daftar pilihan yang sesungguhnya diubah.
        /// </summary>
        [MaxLength(500)]
        public string? ProposedCriticalOptionCodes { get; set; }

        /// <summary>Alasan pengajuan. Wajib — pengajuan tanpa alasan ditolak <c>422</c>.</summary>
        [Required]
        [MaxLength(1000)]
        public string RequestReason { get; set; } = string.Empty;

        [Required]
        public Guid RequestedByUserId { get; set; }

        public DateTime RequestedAt { get; set; }

        /// <summary>
        /// Pemutus dari pihak klinis. Kosong selama pengajuan belum diputuskan.
        /// Nilainya tidak boleh sama dengan <see cref="RequestedByUserId"/>; larangan itu
        /// ditegakkan di dalam service <c>BE-LAB-05</c>, karena sistem permission yang ada
        /// hanya menjawab boleh atau tidak dan tidak pernah membandingkan pelaku sebelumnya.
        /// </summary>
        public Guid? DecidedByUserId { get; set; }

        public DateTime? DecidedAt { get; set; }

        [MaxLength(1000)]
        public string? DecisionNote { get; set; }

        /// <summary>
        /// Token konkurensi. Dua pemutus yang menyetujui pengajuan yang sama secara bersamaan
        /// tidak boleh sama-sama berhasil, karena keduanya akan menulis batas kritis yang
        /// berbeda ke batas nilai yang sama.
        /// </summary>
        public int Version { get; set; }

        public LabValueBound? ValueBound { get; set; }
    }
}
