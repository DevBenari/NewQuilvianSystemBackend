using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models
{
    /// <summary>
    /// Riwayat permanen setiap perubahan batas nilai (<c>LAB-DEC-023</c>, BR-19, <c>AC-34</c>).
    ///
    /// Satu baris menjawab satu pertanyaan lengkap: <b>kolom apa</b> yang berubah, dari
    /// <b>nilai lama</b> apa ke <b>nilai baru</b> apa, oleh <b>siapa</b>, disetujui <b>siapa</b>
    /// bila memang perlu persetujuan, <b>kapan</b>, dan dengan <b>alasan</b> apa.
    ///
    /// Baris pada tabel ini hanya ditambahkan — tidak pernah diubah dan tidak pernah dihapus
    /// oleh alur operasional, mengikuti pola <c>TrxLabTransitionHistory</c> yang sudah berjalan
    /// di modul ini. Bedanya, di sini ketiadaan endpoint yang mengubah tidak dianggap cukup:
    /// kolom faktanya dipasangi tolak-ubah pada <c>LabValueBoundHistoryConfiguration</c>,
    /// sehingga siapa pun yang kelak menulis jalur ubah baru akan ditolak lapisan penyimpanan,
    /// bukan diam-diam berhasil. Riwayat yang dapat diubah bukan riwayat.
    ///
    /// <see cref="OldValue"/> dan <see cref="NewValue"/> disimpan sebagai teks, bukan sebagai
    /// angka, karena satu tabel yang sama merekam perubahan satuan (<c>g/dL</c> menjadi
    /// <c>mmol/L</c>), perubahan batas angka (<c>6,0</c> menjadi <c>8,0</c>), dan perubahan
    /// daftar pilihan kritis (<c>P3,P4</c> menjadi <c>P4</c>).
    /// </summary>
    public class LabValueBoundHistory : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Batas nilai yang berubah.</summary>
        [Required]
        public Guid ValueBoundId { get; set; }

        /// <summary>
        /// Nama kolom yang berubah, misalnya <c>CriticalHigh</c> atau <c>Unit</c>. Disimpan
        /// sebagai teks agar satu baris riwayat tetap terbaca walaupun kolomnya kelak dinamai
        /// ulang atau dihapus.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string ChangedField { get; set; } = string.Empty;

        /// <summary>Nilai lama. Kosong bila kolomnya memang belum pernah terisi.</summary>
        [MaxLength(200)]
        public string? OldValue { get; set; }

        /// <summary>Nilai baru. Kosong bila kolomnya dikosongkan.</summary>
        [MaxLength(200)]
        public string? NewValue { get; set; }

        /// <summary>Pelaku perubahan — kepala instalasi yang mengubah atau yang mengajukan.</summary>
        [Required]
        public Guid ActorUserId { get; set; }

        /// <summary>
        /// Penyetuju. Terisi <b>hanya</b> bila yang berubah adalah batas kritis, karena hanya
        /// perubahan itu yang menempuh jalur persetujuan klinis. Kosong pada perubahan batas
        /// normal, satuan, daftar pilihan sah, dan batas waktu cito — dan kekosongan itu sah,
        /// bukan data yang belum diisi.
        /// </summary>
        public Guid? ApprovedByUserId { get; set; }

        [MaxLength(1000)]
        public string? ChangeReason { get; set; }

        public DateTime OccurredAt { get; set; }

        public LabValueBound? ValueBound { get; set; }
    }
}
