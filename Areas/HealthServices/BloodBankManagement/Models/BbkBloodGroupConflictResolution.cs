using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Models
{
    /// <summary>
    /// Catatan penutupan satu keadaan konflik golongan darah pasien. Entity mandiri di luar
    /// batas <c>BD-AGG-04</c> (<c>BD-DOM-22</c>).
    /// </summary>
    /// <remarks>
    /// <b>Append-only.</b> Baris di sini tidak pernah disunting dan tidak pernah dihapus. Ia
    /// adalah jawaban permanen atas pertanyaan "siapa yang memutuskan golongan darah pasien ini,
    /// kapan, dengan alasan apa, dan berdasarkan pemeriksaan yang mana".
    ///
    /// <b>Kenapa <see cref="ResolvingExamId"/> wajib</b> (<c>DEC-BD-031</c>, Model C). Konflik
    /// tidak dapat ditutup dengan memilih salah satu hasil lama, dan tidak dapat ditutup dengan
    /// menghitung mayoritas (<c>INV-BD-022</c>, <c>AC-BD-054</c>). Satu-satunya jalan adalah
    /// pemeriksaan ulang yang tervalidasi, lalu validator klinis menyatakan hasil itu yang
    /// berlaku. Hasil ulang yang nilainya <b>berbeda dari kedua</b> hasil bentrok tetap boleh
    /// menjadi sah bila validator menyatakannya (<c>AC-BD-053</c>) — sistem tidak memaksa hasil
    /// baru cocok dengan salah satu hasil lama.
    ///
    /// <b>Kenapa entity ini di luar aggregate pemeriksaan.</b> Konflik milik <i>pasien</i>, bukan
    /// milik satu pemeriksaan; ia menyentuh beberapa pemeriksaan sekaligus. Menempatkannya di
    /// dalam salah satu pemeriksaan akan menjadikan satu pihak konflik sebagai pemilik
    /// penyelesaiannya sendiri.
    /// </remarks>
    [Table("BbkBloodGroupConflictResolution", Schema = "public")]
    public class BbkBloodGroupConflictResolution : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Pasien yang konfliknya ditutup.</summary>
        [Required]
        public Guid PatientId { get; set; }

        [ForeignKey(nameof(PatientId))]
        public MstPatient? Patient { get; set; }

        /// <summary>
        /// Pemeriksaan ulang tervalidasi yang dinyatakan validator sebagai hasil yang berlaku.
        /// </summary>
        /// <remarks>
        /// <b>Wajib</b>, dan wajib menunjuk pemeriksaan yang bukan salah satu pihak konflik —
        /// pemeriksaan yang sedang bertentangan bukan "pemeriksaan ulang" (<c>VAL-BD-051</c>).
        /// </remarks>
        [Required]
        public Guid ResolvingExamId { get; set; }

        [ForeignKey(nameof(ResolvingExamId))]
        public BbkBloodGroupExam? ResolvingExam { get; set; }

        /// <summary>Validator klinis yang memutuskan.</summary>
        [Required]
        public Guid ResolvedByUserId { get; set; }

        /// <summary>
        /// Alasan terkendali dari <c>MstBloodBankReason.ReasonCode</c>.
        /// </summary>
        /// <remarks>
        /// Alasan <b>tidak boleh teks bebas</b> (<c>INV-BD-016</c>). Kontrak <c>v4</c> belum
        /// menetapkan kategori alasan khusus untuk penyelesaian konflik golongan darah —
        /// kesepuluh kategori yang ada seluruhnya menyangkut order dan kantong. Service karena
        /// itu menuntut kode alasan yang <b>ada dan aktif</b>, tanpa memaksakan kategori yang
        /// belum diputuskan siapa pun. Gap ini dilaporkan pada laporan task <c>BE-BD-011</c>.
        /// </remarks>
        [Required]
        [MaxLength(30)]
        public string ReasonCode { get; set; } = string.Empty;

        [Required]
        public DateTime ResolvedAt { get; set; }
    }
}
