using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models
{
    /// <summary>
    /// Satu kejadian kunjungan dokter ke pasien — <c>CAP-025</c>, <c>RWI-DEC-084</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Kenapa visite butuh tabelnya sendiri.</b> Menghitung visite dari catatan yang ditulis
    /// dokter dilarang <c>INV-DOK-07</c>: dokter yang mendatangi pasien tetapi belum sempat
    /// menulis apa pun tetap benar-benar datang, dan dokter yang menulis tiga catatan dalam satu
    /// kunjungan tetap datang sekali. Kejadian dan catatan karena itu dua hal berbeda, dan
    /// ketiga tautan dokumennya nullable.
    /// </para>
    /// <para>
    /// <b>Nama tabelnya bukan <c>Trx*</c>.</b> <c>QBE-NAM-001</c> melarang awalan itu untuk kode
    /// baru; prefix registry milik <c>ClinicalManagement</c> adalah <c>Cli</c>. Entity, berkas,
    /// configuration, <c>DbSet</c>, dan nama tabel adalah satu paket:
    /// <c>CliPhysicianVisit</c> / <c>CliPhysicianVisit.cs</c> /
    /// <c>CliPhysicianVisitConfiguration</c> / <c>CliPhysicianVisits</c> /
    /// <c>public."CliPhysicianVisit"</c>.
    /// </para>
    /// <para>
    /// <b>Tidak ada unique atas pasangan perawatan, dokter, dan tanggal.</b> Dokter yang
    /// benar-benar datang dua kali pada hari yang sama menghasilkan dua baris —
    /// <c>RWI-DEC-085</c>. Yang dijaga unique adalah kunci permintaan, supaya tombol yang
    /// tertekan dua kali tidak melahirkan dua kejadian.
    /// </para>
    /// </remarks>
    [Table("CliPhysicianVisit", Schema = "public")]
    public class CliPhysicianVisit : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Nomor bisnis yang terbaca manusia, dialokasikan service lewat penyedia nomor.
        /// </summary>
        /// <remarks>
        /// <c>QBE-CODE-002</c> dan <c>QBE-CODE-003</c>: controller tidak pernah mengalokasikan
        /// nomor, dan alokasinya tidak boleh memakai Count+1 maupun Max+1.
        /// </remarks>
        [Required]
        [MaxLength(30)]
        public string PhysicianVisitNumber { get; set; } = string.Empty;

        /// <summary>Jangkar klinis. Setiap kejadian menempel pada satu kunjungan.</summary>
        [Required]
        public Guid EncounterId { get; set; }

        /// <summary>
        /// Perawatan rawat inap yang menaungi kejadian ini. Boleh kosong: kunjungan dokter di
        /// luar rawat inap tetap dapat dicatat.
        /// </summary>
        public Guid? InpEpisodeId { get; set; }

        /// <summary>Pasien yang didatangi. Penjaga salah pasien.</summary>
        [Required]
        public Guid PatientId { get; set; }

        /// <summary>Dokter yang datang. Subjek dari fakta ini.</summary>
        [Required]
        public Guid DoctorId { get; set; }

        /// <summary>
        /// <b>Waktu kedatangan</b>, bukan waktu pencatatan.
        /// </summary>
        /// <remarks>
        /// Visite pukul 07.40 yang baru dicatat pukul 07.52 tetap terbaca pada pukul 07.40.
        /// Waktu pencatatan sendiri sudah tersimpan pada <c>CreateDateTime</c>.
        /// </remarks>
        public DateTime VisitDateTime { get; set; } = DateTime.UtcNow;

        /// <summary>Peran dokter pada kejadian ini.</summary>
        public PhysicianVisitRole VisitRole { get; set; } = PhysicianVisitRole.Dpjp;

        /// <summary>Keadaan kejadian; menjaga <c>INV-DOK-08</c>.</summary>
        public PhysicianVisitStatus VisitStatus { get; set; } = PhysicianVisitStatus.Recorded;

        /// <summary>Tautan opsional ke catatan dokter yang lahir dari kunjungan ini.</summary>
        public Guid? ConsultationId { get; set; }

        /// <summary>Tautan opsional ke catatan pada lembar terpadu.</summary>
        public Guid? ProgressNoteId { get; set; }

        /// <summary>Tautan opsional ke tindakan yang dikerjakan saat kunjungan ini.</summary>
        public Guid? PatientProcedureId { get; set; }

        /// <summary>Catatan singkat dokter. Sensitif; tidak boleh masuk custom logger.</summary>
        [MaxLength(1000)]
        public string? Note { get; set; }

        /// <summary>Pengguna yang mencatat kejadian ini.</summary>
        [Required]
        public Guid RecordedByUserId { get; set; }

        /// <summary>
        /// Kunci permintaan. <b>Wajib terisi</b> sejak revision <c>0.2</c> kamus data.
        /// </summary>
        /// <remarks>
        /// <c>INV-DOK-06</c> tidak dapat dijamin bila kuncinya boleh kosong. Unique-nya penuh,
        /// bukan parsial: kunci milik kejadian yang <b>sudah dibatalkan pun tidak boleh dipakai
        /// ulang</b>, karena bila boleh, sebuah kiriman ulang lama dapat menghidupkan kembali
        /// kejadian yang sengaja dibatalkan.
        /// </remarks>
        [Required]
        [MaxLength(100)]
        public string IdempotencyKey { get; set; } = string.Empty;

        /// <summary>Waktu pembatalan kejadian.</summary>
        public DateTime? CancelledAt { get; set; }

        /// <summary>Pengguna yang membatalkan kejadian.</summary>
        public Guid? CancelledByUserId { get; set; }

        /// <summary>Alasan pembatalan. Wajib diisi saat membatalkan. Sensitif.</summary>
        [MaxLength(500)]
        public string? CancelReason { get; set; }

        /// <summary>
        /// Kejadian yang digantikan oleh kejadian ini setelah koreksi.
        /// </summary>
        /// <remarks>
        /// Diisi saat dokter mencatat ulang setelah membatalkan kejadian yang salah, sehingga
        /// jejak koreksinya terbaca tanpa menyunting kejadian lamanya.
        /// </remarks>
        public Guid? CorrectsVisitId { get; set; }

        public TrxPatientEncounter? Encounter { get; set; }

        public InpEpisode? InpEpisode { get; set; }

        public MstPatient? Patient { get; set; }

        public MstDoctor? Doctor { get; set; }

        public TrxDoctorConsultation? Consultation { get; set; }

        public TrxPatientIntegratedProgressNote? ProgressNote { get; set; }

        public TrxPatientProcedure? PatientProcedure { get; set; }

        public ApplicationUser? RecordedByUser { get; set; }

        public ApplicationUser? CancelledByUser { get; set; }

        public CliPhysicianVisit? CorrectsVisit { get; set; }
    }
}
