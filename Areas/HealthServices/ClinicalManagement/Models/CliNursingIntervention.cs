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
    /// Satu tindakan keperawatan yang <b>sudah dilakukan</b> - <c>CAP-014</c>,
    /// <c>BE-RWI-061</c>, <c>FR-KEP-018</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Kenapa tidak memakai <c>TrxPatientProcedure</c> yang sudah ada.</b> Tabel itu
    /// mewajibkan <c>ConsultationId</c> dan <c>DoctorId</c>; melonggarkannya akan melemahkan
    /// penjagaan bagi tindakan dokter yang membutuhkan keduanya untuk penagihan. Tindakan
    /// keperawatan justru sering lahir tanpa konsultasi dokter sama sekali.
    /// </para>
    /// <para>
    /// <b>Tindakan mendadak boleh tanpa rencana asuhan.</b> <c>CAP-014</c> aturan 3:
    /// <c>CarePlanItemId</c> nullable, dan memakai <c>SetNull</c> - menutup butir rencana
    /// <b>tidak boleh</b> menghapus tindakan yang sudah benar-benar dilakukan
    /// (<c>CAP-013</c> aturan 6).
    /// </para>
    /// <para>
    /// <b>Kunci permintaan dijaga database, bukan hanya aplikasi.</b> Idempotency yang dipasang
    /// di controller saja akan bocor ketika ada dua instance aplikasi berjalan: keduanya membaca
    /// "belum ada" pada saat yang sama, lalu keduanya menyimpan. Unique parsial pada
    /// <c>IdempotencyKey</c> adalah penjaga terakhirnya - <c>VAL-KEP-15</c>, <c>QBE-CODE-004</c>.
    /// </para>
    /// <para>
    /// <b>Nama tabelnya bukan <c>Trx*</c></b> - <c>QBE-NAM-001</c>. Entity, berkas,
    /// configuration, <c>DbSet</c>, dan nama tabel satu paket: <c>CliNursingIntervention</c> /
    /// <c>CliNursingIntervention.cs</c> / <c>CliNursingInterventionConfiguration</c> /
    /// <c>CliNursingInterventions</c> / <c>public."CliNursingIntervention"</c>.
    /// </para>
    /// </remarks>
    [Table("CliNursingIntervention", Schema = "public")]
    public class CliNursingIntervention : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Jangkar klinis. Setiap tindakan menempel pada satu kunjungan.</summary>
        [Required]
        public Guid EncounterId { get; set; }

        /// <summary>
        /// Perawatan rawat inap yang menaungi tindakan ini. Boleh kosong supaya tabel ini tetap
        /// dapat dipakai kelak di luar rawat inap tanpa perubahan bentuk.
        /// </summary>
        public Guid? InpEpisodeId { get; set; }

        /// <summary>Pasien yang menerima tindakan. Penjaga salah pasien.</summary>
        [Required]
        public Guid PatientId { get; set; }

        /// <summary>
        /// Butir rencana asuhan yang menjadi dasar tindakan. <b>Boleh kosong</b>: tindakan
        /// mendadak tetap harus dapat dicatat - <c>CAP-014</c> aturan 3.
        /// </summary>
        public Guid? CarePlanItemId { get; set; }

        /// <summary>Apa yang dikerjakan.</summary>
        [Required]
        [MaxLength(300)]
        public string InterventionName { get; set; } = string.Empty;

        /// <summary>
        /// <b>Waktu tindakan dikerjakan</b>, bukan waktu pencatatan.
        /// </summary>
        /// <remarks>
        /// Perawat sering baru sempat mencatat berjam-jam kemudian, dan lini masa pasien harus
        /// membaca waktu yang sebenarnya. Waktu pencatatan sendiri sudah tersimpan pada
        /// <c>CreateDateTime</c>. Waktu di masa depan ditolak <c>VAL-KEP-13</c>; waktu sebelum
        /// pasien masuk kamar ditolak <c>VAL-KEP-14</c>.
        /// </remarks>
        public DateTime PerformedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Perawat yang mengerjakan tindakan.</summary>
        [Required]
        public Guid PerformedByEmployeeId { get; set; }

        /// <summary>Hasil tindakan beserta respons pasien. Sensitif.</summary>
        public string? ResultNote { get; set; }

        /// <summary>Keadaan catatan tindakan.</summary>
        public NursingInterventionStatus RecordStatus { get; set; } = NursingInterventionStatus.Recorded;

        /// <summary>Saat catatan dinyatakan final.</summary>
        public DateTime? FinalizedAt { get; set; }

        /// <summary>Pengguna yang menyatakan catatan final.</summary>
        public Guid? FinalizedByUserId { get; set; }

        /// <summary>
        /// Kunci permintaan. Boleh kosong, dan unique-nya karena itu parsial.
        /// </summary>
        /// <remarks>
        /// Berbeda dari <c>CliPhysicianVisit</c> yang mewajibkannya, kunci di sini opsional
        /// mengikuti kamus data bagian 6: pencatatan dari layar yang tidak mengirim kunci tetap
        /// harus berjalan, karena menahan pencatatan tindakan yang sudah dilakukan lebih
        /// berbahaya daripada risiko duplikat yang dapat dikoreksi.
        /// </remarks>
        [MaxLength(100)]
        public string? IdempotencyKey { get; set; }

        /// <summary>Apakah tindakan ini dapat ditagih.</summary>
        public bool IsBillable { get; set; } = false;

        /// <summary>
        /// Keadaan pengiriman ke Billing. <b>Terpisah</b> dari <see cref="RecordStatus"/>.
        /// </summary>
        public NursingBillingDispatchStatus BillingDispatchStatus { get; set; } =
            NursingBillingDispatchStatus.NotApplicable;

        /// <summary>Saat percobaan pengiriman terakhir dijalankan.</summary>
        public DateTime? BillingDispatchedAt { get; set; }

        /// <summary>Banyaknya percobaan pengiriman yang pernah dijalankan.</summary>
        public int BillingDispatchAttemptCount { get; set; }

        /// <summary>
        /// Sebab kegagalan pengiriman terakhir, untuk rekonsiliasi.
        /// </summary>
        /// <remarks>
        /// Berisi pesan teknis dari sisi integrasi, bukan isi klinis. Ia dibaca petugas yang
        /// menjalankan percobaan ulang.
        /// </remarks>
        [MaxLength(500)]
        public string? BillingDispatchFailureReason { get; set; }

        public TrxPatientEncounter? Encounter { get; set; }

        public InpEpisode? InpEpisode { get; set; }

        public MstPatient? Patient { get; set; }

        public CliNursingCarePlanItem? CarePlanItem { get; set; }

        public MstEmployee? PerformedByEmployee { get; set; }
    }
}
