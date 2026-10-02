using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models
{
    /// <summary>
    /// Satu pilihan hasil yang sah untuk sebuah batas nilai berbentuk pilihan terbatas
    /// (<c>LAB-DEC-021</c>, BR-17).
    ///
    /// Keberadaan tabel ini adalah syarat <c>AC-28</c>: analis memilih dari daftar, bukan
    /// mengetik bebas. Tanpa daftar yang sah, "+4", "Positif kuat (4+)", dan "protein +4"
    /// tersimpan sebagai tiga hal berbeda dan nilai kritis tidak pernah terdeteksi.
    ///
    /// <see cref="IsOutOfReference"/> dan <see cref="IsCritical"/> dipisah karena keduanya
    /// menjawab pertanyaan berbeda: yang pertama menandai hasil di luar rujukan, yang kedua
    /// memicu pelaporan nilai kritis. Keduanya boleh sama-sama kosong — golongan darah dan
    /// tes kehamilan berbentuk pilihan tetapi tidak punya nilai kritis, dan itu sah.
    /// </summary>
    public class LabValueOption : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Batas nilai induk yang memiliki pilihan ini.</summary>
        [Required]
        public Guid ValueBoundId { get; set; }

        /// <summary>Kode pilihan, misalnya <c>P3</c>. Unik dalam satu batas nilai.</summary>
        [Required]
        [MaxLength(20)]
        public string OptionCode { get; set; } = string.Empty;

        /// <summary>Nama pilihan sebagaimana dibaca petugas, misalnya <c>+3</c>.</summary>
        [Required]
        [MaxLength(100)]
        public string OptionName { get; set; } = string.Empty;

        /// <summary>Pilihan ini berada di luar nilai rujukan.</summary>
        public bool IsOutOfReference { get; set; }

        /// <summary>
        /// Pilihan ini tergolong kritis. Perubahannya memerlukan persetujuan klinis, sama
        /// seperti batas kritis berbentuk angka (<c>LAB-DEC-023</c>).
        /// </summary>
        public bool IsCritical { get; set; }

        /// <summary>
        /// Urutan pilihan pada skala hasilnya — Negatif, +1, +2, +3, +4. Urutan ini bermakna
        /// bisnis, bukan sekadar tampilan: ia menyatakan tingkatan skala ordinal pemeriksaan.
        /// </summary>
        public int SortOrder { get; set; }

        public LabValueBound? ValueBound { get; set; }
    }
}
