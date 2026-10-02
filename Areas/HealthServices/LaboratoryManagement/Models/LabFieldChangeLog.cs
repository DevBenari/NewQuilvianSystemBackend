using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models
{
    /// <summary>
    /// Jejak perubahan <b>nilai satu ruas</b> (<c>LAB-DEC-112</c>).
    ///
    /// <b>Ia BUKAN pengganti <c>LabTransitionHistory</c>, dan keduanya sengaja terpisah.</b>
    /// Alasannya sama persis dengan yang dipakai <c>LAB-DEC-080</c> menolak menambahkan
    /// <c>Validated</c> ke <c>LabExaminationStatus</c>: <b>satu tabel, dua sumbu</b>.
    /// Perpindahan status dan perubahan nilai ruas menjawab dua pertanyaan berbeda.
    /// Ditumpangkan, ketiga kolom nilai akan kosong pada seluruh baris status lama, dan setiap
    /// laporan riwayat status harus menyaring baris yang bukan miliknya — selamanya.
    ///
    /// <b>Bentuknya sengaja UMUM.</b> <see cref="EntityName"/> beserta <see cref="EntityId"/>
    /// membuatnya dapat dipakai ulang ketika ruas lain kelak perlu dijejaki, tanpa menambah
    /// tabel kelima.
    /// </summary>
    public class LabFieldChangeLog : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Nama entity yang berubah, misalnya <c>LabSpecimen</c>.</summary>
        [Required]
        [MaxLength(100)]
        public string EntityName { get; set; } = string.Empty;

        /// <summary>
        /// Identitas baris yang berubah.
        ///
        /// <b>Sengaja tanpa foreign key</b>, sebab tabel ini melayani lebih dari satu entity —
        /// dan itu justru yang membuatnya dapat dipakai ulang.
        /// </summary>
        [Required]
        public Guid EntityId { get; set; }

        [Required]
        [MaxLength(100)]
        public string FieldName { get; set; } = string.Empty;

        /// <summary>
        /// Nilai <b>sebelum</b> diubah, sudah dibaca-manusiakan. Kosong berarti ruasnya memang
        /// belum terisi.
        ///
        /// <b>Inilah alasan tabel ini ada.</b> Mengubah jenis bahan sesudah pemeriksaan
        /// berjalan mengubah arti hasilnya; tanpa nilai lama, pembaca enam bulan kemudian nol
        /// punya cara tahu bahwa bahannya pernah tercatat lain.
        /// </summary>
        [MaxLength(500)]
        public string? OldValue { get; set; }

        [MaxLength(500)]
        public string? NewValue { get; set; }

        /// <summary>
        /// Siapa yang mengubah. <b>Nullable dan tanpa foreign key</b>, mengikuti pola kolom
        /// pengguna lain pada modul ini.
        /// </summary>
        public Guid? ChangedByUserId { get; set; }

        [Required]
        public DateTime ChangedAt { get; set; }
    }
}
