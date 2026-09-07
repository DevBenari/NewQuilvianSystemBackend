using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models
{
    /// <summary>
    /// Satu masalah keperawatan beserta tujuan, rencana tindakan, dan evaluasinya —
    /// <c>CAP-013</c>, <c>BE-RWI-059</c>, <c>FR-KEP-013</c>, <c>FR-KEP-015</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Masalah keperawatan ditulis sebagai teks, bukan sebagai kode katalog.</b> Pemakaian
    /// katalog SDKI/SLKI/SIKI belum diputuskan — <c>OQ-RI-011</c>, dibuka kembali lewat
    /// <c>RWI-DEC-090</c>. Karena itu <c>ProblemStatement</c> menampung teksnya dan
    /// <c>NursingDiagnosisId</c> disediakan kosong tanpa foreign key: tabel tujuannya memang
    /// belum ada, dan mengunci bentuknya sekarang berarti memutuskan hal yang bukan wewenang
    /// pembangun.
    /// </para>
    /// <para>
    /// <b>Butir tidak pernah dihapus.</b> <c>CAP-013</c> aturan 6 melarang penutupan butir
    /// menghapus tindakan maupun evaluasi sebelumnya. Butir yang selesai berpindah status
    /// menjadi <c>Resolved</c> atau <c>Discontinued</c>, dan barisnya tetap ada.
    /// </para>
    /// <para>
    /// <b><c>VersionNumber</c> bukan hiasan.</b> Setiap pembaruan butir menyalin keadaan
    /// sebelumnya ke tabel revisi lebih dulu — <c>BE-RWI-060</c>, <c>AC-CAP013-02</c> — sehingga
    /// riwayat asuhan pasien tetap utuh beserta penulis dan waktu aslinya.
    /// </para>
    /// </remarks>
    [Table("CliNursingCarePlanItem", Schema = "public")]
    public class CliNursingCarePlanItem : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Rencana asuhan induk.</summary>
        [Required]
        public Guid CarePlanId { get; set; }

        /// <summary>
        /// Penunjuk ke katalog terminologi keperawatan, bila kelak rumah sakit memakainya.
        /// </summary>
        /// <remarks>
        /// Sengaja <b>tanpa foreign key</b>: keputusan katalog SDKI/SLKI/SIKI masih terbuka
        /// (<c>OQ-RI-011</c>), sehingga tabel tujuannya belum ada. Kolomnya disediakan supaya
        /// pengisian data lama kelak tidak menuntut perubahan bentuk tabel.
        /// </remarks>
        public Guid? NursingDiagnosisId { get; set; }

        /// <summary>
        /// Pengkajian yang menjadi asal masalah keperawatan ini — <c>AC-CAP013-01</c>.
        /// </summary>
        /// <remarks>
        /// Boleh kosong: masalah yang muncul di tengah perawatan tidak selalu lahir dari satu
        /// pengkajian tertentu. Memakai <c>SetNull</c>, supaya penghapusan lunak pengkajian tidak
        /// pernah menyeret butir asuhan ikut hilang.
        /// </remarks>
        public Guid? SourceAssessmentId { get; set; }

        /// <summary>Masalah keperawatan pasien. Sensitif; tidak boleh masuk custom logger.</summary>
        [Required]
        [MaxLength(500)]
        public string ProblemStatement { get; set; } = string.Empty;

        /// <summary>Tujuan asuhan yang hendak dicapai. Sensitif.</summary>
        [MaxLength(500)]
        public string? GoalStatement { get; set; }

        /// <summary>Rencana tindakan yang disusun perawat. Sensitif.</summary>
        public string? PlannedIntervention { get; set; }

        /// <summary>
        /// Catatan evaluasi hasil asuhan terakhir. Sensitif.
        /// </summary>
        /// <remarks>
        /// <c>VAL-KEP-16</c> bersandar pada kolom ini: butir hanya dapat dinyatakan tercapai
        /// bila evaluasinya sudah ada. Riwayat evaluasi sebelumnya tersimpan pada tabel revisi.
        /// </remarks>
        public string? EvaluationNote { get; set; }

        /// <summary>Saat evaluasi terakhir dicatat. Kosong berarti belum pernah dievaluasi.</summary>
        public DateTime? LastEvaluatedAt { get; set; }

        /// <summary>Keadaan butir.</summary>
        public NursingCarePlanItemStatus ItemStatus { get; set; } = NursingCarePlanItemStatus.Active;

        /// <summary>Saat butir dinyatakan tercapai atau dihentikan.</summary>
        public DateTime? ResolvedAt { get; set; }

        /// <summary>Alasan penutupan butir. Wajib saat butir dihentikan. Sensitif.</summary>
        [MaxLength(500)]
        public string? CloseReason { get; set; }

        /// <summary>Nomor versi butir; naik satu setiap kali isinya diperbarui.</summary>
        public int VersionNumber { get; set; } = 1;

        /// <summary>
        /// Perawat yang menulis <b>versi butir yang sedang berlaku</b>.
        /// </summary>
        /// <remarks>
        /// <c>AC-CAP013-02</c> bersandar pada dua kolom ini. Ketika butir diperbarui, keadaan
        /// lamanya disalin ke tabel revisi <b>beserta nilai kedua kolom ini apa adanya</b>,
        /// barulah keduanya diganti dengan perawat dan waktu yang memperbarui. Tanpa keduanya,
        /// versi lama hanya dapat menyebut penulis yang mengubah — dan rekam medis kehilangan
        /// bukti siapa yang menilai pertama kali.
        /// </remarks>
        [Required]
        public Guid AuthoredByEmployeeId { get; set; }

        /// <summary>Saat versi butir yang sedang berlaku ditulis.</summary>
        public DateTime AuthoredAt { get; set; } = DateTime.UtcNow;

        public CliNursingCarePlan? CarePlan { get; set; }

        public TrxPatientAssessment? SourceAssessment { get; set; }

        public MstEmployee? AuthoredByEmployee { get; set; }
    }
}
