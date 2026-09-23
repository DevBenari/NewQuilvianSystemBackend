using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models
{
    /// <summary>
    /// Spesifik Specimen — tingkat kedua di bawah <see cref="LabSpecimenType"/>
    /// (<c>LAB-DEC-129</c>, <c>LAB-DEC-131</c>, <c>LAB-DEC-132</c>).
    ///
    /// <b>Datanya bertingkat TIGA, tetapi hanya DUA yang menjadi pilihan.</b>
    /// <c>LAB-EVD-007</c> memuat <c>jenis_specimen</c> (31), <c>subjenis_specimen</c> (85),
    /// dan <c>display</c> (1.767). Yang menjadi tingkat kedua adalah <c>display</c>; subjenis
    /// turun menjadi <see cref="SubTypeName"/> — sebuah <b>atribut</b>, bukan layar pilihan —
    /// sebab <b>21 dari 31 kelompok hanya punya satu subjenis</b>, dan memilihnya berarti
    /// melewati layar yang nol punya alternatif.
    /// </summary>
    public class LabSpecimenDetailType : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Kelompok induknya pada <see cref="LabSpecimenType"/>.</summary>
        [Required]
        public Guid LabSpecimenTypeId { get; set; }

        /// <summary>
        /// Kode milik Laboratorium, dinormalkan huruf kapital dan unik di antara baris yang
        /// belum ditandai terhapus.
        ///
        /// <b>Bukan kode SNOMED.</b> Nilai yang kelak ditambahkan kepala instalasi lewat jalan
        /// keluar <c>Lainnya</c> nol punya kode SNOMED, dan memaksakannya sebagai kode utama
        /// berarti baris lokal diberi kode karangan di dalam kolom yang dianggap standar
        /// internasional (<c>LAB-DEC-132</c>).
        /// </summary>
        [Required]
        [MaxLength(64)]
        public string DetailTypeCode { get; set; } = string.Empty;

        /// <summary>
        /// Nama Bahasa Indonesia. <b>Boleh kosong</b> (<c>LAB-DEC-131</c>).
        ///
        /// Seluruh 1.767 baris hasil impor berbahasa Inggris, dan menahan slice sampai
        /// seluruhnya diterjemahkan tidak sebanding. Yang lebih memberatkan: <b>terjemahan
        /// anatomi yang keliru lebih berbahaya daripada istilah Inggris yang dibiarkan</b> —
        /// petugas yang ragu pada istilah asing akan bertanya, sedangkan terjemahan salah
        /// tampak meyakinkan.
        /// </summary>
        [MaxLength(300)]
        public string? DetailTypeNameId { get; set; }

        /// <summary>
        /// Nama SNOMED CT berbahasa Inggris. <b>Wajib</b> — seluruh baris punya ini, dan
        /// mewajibkan yang belum ada justru akan membuat seeder gagal pada baris pertama.
        /// </summary>
        [Required]
        [MaxLength(300)]
        public string DetailTypeNameEn { get; set; } = string.Empty;

        /// <summary>
        /// <c>subjenis_specimen</c> — <b>atribut pengelompokan, bukan tingkat pilihan</b>.
        ///
        /// Disimpan sebagai teks, bukan penunjuk tabel: ia nol pernah dipilih, dan mendirikan
        /// tabel untuk sesuatu yang nol dipilih berarti tiga tabel untuk dua tingkat. Sebagai
        /// teks ia tetap dapat dikelompokkan pada pelaporan — <i>"berapa banyak bahan dari
        /// kelompok cairan pleura"</i> — dan itu memang satu-satunya kegunaannya.
        /// </summary>
        [MaxLength(200)]
        public string? SubTypeName { get; set; }

        /// <summary>
        /// Kode SNOMED CT. <b>Kosong</b> untuk baris yang ditambahkan lewat <c>Lainnya</c>.
        ///
        /// Disimpan meski tidak menjadi kode utama: datanya ada di tangan hari ini, dan
        /// pemetaan 1.767 baris tidak perlu dikerjakan ulang ketika rumah sakit kelak bertukar
        /// data dengan sistem luar.
        /// </summary>
        [MaxLength(32)]
        public string? SnomedCode { get; set; }

        public int SortOrder { get; set; }

        /// <summary>
        /// Ke-166 baris berkonfidensi <c>Rendah</c> ter-seed bernilai <b>salah</b>
        /// (<c>LAB-DEC-130</c>). Seluruhnya menyebut <b>lokasi tanpa menyebut bahan</b> —
        /// <c>Specimen from abdominal cavity</c> — sehingga sebagai pilihan rutin ia
        /// mengaburkan pelaporan bahan. Dibuang berarti kehilangan kode SNOMED-nya.
        /// </summary>
        public bool IsActive { get; set; } = true;

        public LabSpecimenType? LabSpecimenType { get; set; }
    }

    /// <summary>
    /// Spesifik Specimen yang dipilih pada satu wadah. Satu specimen boleh menunjuk
    /// <b>lebih dari satu</b> (<c>LAB-DEC-098</c>).
    /// </summary>
    public class LabSpecimenDetail : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid LabSpecimenId { get; set; }

        [Required]
        public Guid LabSpecimenDetailTypeId { get; set; }

        /// <summary>
        /// Nama sebagaimana berlaku saat dipilih — Indonesia bila ada, Inggris bila belum.
        ///
        /// Snapshot, dengan alasan yang sama seperti <c>OrganismNameSnapshot</c>: nama yang
        /// diperbaiki kepala instalasi kelak tidak boleh mengubah arti specimen yang sudah
        /// tercatat.
        /// </summary>
        [Required]
        [MaxLength(300)]
        public string DetailNameSnapshot { get; set; } = string.Empty;

        public LabSpecimen? LabSpecimen { get; set; }

        public LabSpecimenDetailType? LabSpecimenDetailType { get; set; }
    }
}
