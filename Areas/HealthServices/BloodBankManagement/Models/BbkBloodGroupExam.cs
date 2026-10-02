using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Models
{
    /// <summary>
    /// Satu pemeriksaan golongan darah milik Bank Darah, dengan status validasinya sendiri.
    /// Aggregate root <c>BD-AGG-04</c> / <c>BD-DOM-09</c>.
    /// </summary>
    /// <remarks>
    /// <b>Inilah satu-satunya sumber sah golongan darah pasien</b> (<c>DEC-BD-015</c>).
    /// <c>MstPatient.BloodType</c> adalah data induk administratif — hasil wawancara pendaftaran,
    /// bukan hasil pemeriksaan — dan <b>tidak pernah</b> menjadi sumber klinis
    /// (<c>INV-BD-014</c>). Kekeliruan itulah yang dicatat sebagai <c>CONF-BD-005</c>.
    ///
    /// <b>"Golongan darah sah pasien" bukan kolom.</b> Ia turunan (<c>BD-DOM-21</c>) yang
    /// dihitung dari kumpulan pemeriksaan milik pasien itu, sehingga tidak ada satu nilai pun
    /// yang dapat disunting seseorang untuk mengubah golongan darah pasien tanpa jejak
    /// pemeriksaan.
    ///
    /// <b>Hasil tervalidasi tidak pernah ditimpa</b> (<c>DEC-BD-026</c>). Ketika hasil
    /// tervalidasi baru berbeda dari hasil sah sebelumnya, sistem <b>tidak memilih</b> yang
    /// mana yang benar: keduanya ditandai <see cref="IsConflictHeld"/>, pasien kehilangan
    /// golongan darah sah, dan gerbang klinis tertutup sampai validator menyelesaikannya lewat
    /// pemeriksaan ulang (<c>DEC-BD-031</c>, <c>BD-XINV-04</c>).
    ///
    /// <b>Nol field-nya dialokasikan number-series.</b> Entity ini memuat <c>PatientId</c>,
    /// bukan <c>BloodOrderId</c>, dan tidak punya nomor dokumen — itulah sebabnya slice ini
    /// dapat dikerjakan tanpa menunggu provider nomor bisnis.
    /// </remarks>
    [Table("BbkBloodGroupExam", Schema = "public")]
    public class BbkBloodGroupExam : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Pasien yang diperiksa.</summary>
        [Required]
        public Guid PatientId { get; set; }

        [ForeignKey(nameof(PatientId))]
        public MstPatient? Patient { get; set; }

        /// <summary>
        /// Hasil ABO dan Rhesus. Kosong sebelum hasilnya dicatat.
        /// </summary>
        /// <remarks>
        /// Nilai ini <b>data klinis</b> dan <b>MUST NOT</b> ikut tertulis ke log
        /// (<c>contracts/permission-audit-matrix.md</c>). Enum <c>BloodType</c> dipakai apa
        /// adanya dari Platform (<c>BD-CAP-016</c>), tidak dibuat ulang.
        /// </remarks>
        public BloodType? AboRhesusResult { get; set; }

        public BbkBloodGroupExamStatus ExamStatus { get; set; } = BbkBloodGroupExamStatus.SampleTaken;

        /// <summary>Petugas yang mencatat hasil. Wajib terisi begitu hasil tercatat.</summary>
        public Guid? ExaminedByUserId { get; set; }

        /// <summary>Waktu hasil dicatat. Wajib terisi begitu hasil tercatat.</summary>
        /// <remarks>
        /// Pasangan pemeriksa dan waktu ini yang dijaga <c>VAL-BD-030</c>. Keduanya diturunkan
        /// dari pengguna terautentikasi dan jam server, bukan dari isian pemanggil, supaya
        /// tidak ada hasil yang mengaku diperiksa orang lain.
        /// </remarks>
        public DateTime? ExaminedAt { get; set; }

        /// <summary>Validator yang memvalidasi hasil (<c>DEF-BD-004</c>).</summary>
        public Guid? ValidatedByUserId { get; set; }

        public DateTime? ValidatedAt { get; set; }

        /// <summary>
        /// Penanda hasil sah yang <b>sedang berlaku</b> bagi pasien.
        /// </summary>
        /// <remarks>
        /// Paling banyak satu pemeriksaan per pasien boleh bernilai benar (<c>INV-BD-018</c>).
        /// Bernilai benar <b>bukan</b> hal yang sama dengan berstatus <c>Validated</c>: sebuah
        /// hasil tervalidasi berhenti berlaku ketika muncul hasil tervalidasi lain yang
        /// berselisih, dan ketika itu terjadi pasien tidak punya hasil sah sama sekali sampai
        /// perbedaannya diselesaikan.
        /// </remarks>
        public bool IsValidResult { get; set; }

        /// <summary>
        /// Penanda pemeriksaan ini menjadi pihak dalam perbedaan hasil yang belum diselesaikan
        /// (<c>DEC-BD-026</c>).
        /// </summary>
        /// <remarks>
        /// Selama ada satu saja pemeriksaan milik pasien yang bernilai benar di sini, pasien
        /// dianggap <b>tidak punya golongan darah sah</b> dan seluruh gerbang klinis yang
        /// menuntutnya tertutup (<c>VAL-BD-034</c>). Penanda ini dipadamkan hanya lewat
        /// penyelesaian konflik oleh validator klinis, tidak pernah oleh proses otomatis.
        /// </remarks>
        public bool IsConflictHeld { get; set; }

        /// <summary>Token pencegah tulis-bersamaan pada validasi dan penyelesaian konflik.</summary>
        public int Version { get; set; }

        public ICollection<BbkBloodGroupSample> Samples { get; set; } = new List<BbkBloodGroupSample>();
    }
}
