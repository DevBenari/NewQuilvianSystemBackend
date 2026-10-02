using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models
{
    /// <summary>
    /// Salinan satu versi butir rencana asuhan sebelum ia diperbarui - <c>CAP-013</c> aturan 5,
    /// <c>BE-RWI-060</c>, <c>AC-CAP013-02</c>, <c>RWI-AC-177</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Ini mesin versi, bukan mesin addendum.</b> Perubahan rencana asuhan adalah
    /// <b>perkembangan klinis</b> - pasien membaik, sehingga tujuannya berubah - dan bukan
    /// pembetulan kesalahan. <c>RWI-DEC-091</c> secara tegas tidak menyeret rencana asuhan ke
    /// mesin addendum milik <c>MedicalRecordManagement</c>, karena menyamakan keduanya akan
    /// mengaburkan perbedaan antara <i>pasien membaik</i> dan <i>perawat salah tulis</i>.
    /// </para>
    /// <para>
    /// <b>Dua kolom terakhir adalah inti tabel ini.</b> <c>OriginalAuthorEmployeeId</c> dan
    /// <c>OriginalAuthoredAt</c> menyimpan penulis dan waktu <b>versi yang diarsipkan</b>, bukan
    /// perawat yang memperbaruinya.
    /// </para>
    /// <para>
    /// <b>Contoh berangka.</b> Butir "risiko jatuh tinggi" ditulis Ns. Sari pukul 08.00, lalu
    /// diperbarui Ns. Dewi pukul 15.00 menjadi "risiko jatuh sedang". Yang tersimpan pada tabel
    /// ini adalah versi 1 berbunyi "risiko jatuh tinggi" atas nama <b>Ns. Sari pukul 08.00</b>.
    /// Bila tersalin atas nama Ns. Dewi, rekam medis kehilangan bukti siapa yang menilai pertama
    /// kali - dan itulah yang diuji <c>AC-CAP013-02</c>.
    /// </para>
    /// </remarks>
    [Table("CliNursingCarePlanItemRevision", Schema = "public")]
    public class CliNursingCarePlanItemRevision : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Butir yang versinya diarsipkan.</summary>
        [Required]
        public Guid CarePlanItemId { get; set; }

        /// <summary>
        /// Nomor versi yang diarsipkan. Unique bersama <c>CarePlanItemId</c>, sehingga satu butir
        /// tidak pernah memiliki dua baris untuk versi yang sama.
        /// </summary>
        [Required]
        public int VersionNumber { get; set; }

        /// <summary>Masalah keperawatan pada versi ini. Sensitif.</summary>
        [Required]
        [MaxLength(500)]
        public string ProblemStatement { get; set; } = string.Empty;

        /// <summary>Tujuan asuhan pada versi ini. Sensitif.</summary>
        [MaxLength(500)]
        public string? GoalStatement { get; set; }

        /// <summary>Rencana tindakan pada versi ini. Sensitif.</summary>
        public string? PlannedIntervention { get; set; }

        /// <summary>Catatan evaluasi pada versi ini. Sensitif.</summary>
        public string? EvaluationNote { get; set; }

        /// <summary>Saat versi ini diarsipkan, yaitu saat butirnya diperbarui.</summary>
        public DateTime RevisedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Perawat yang menulis versi ini - <b>bukan</b> perawat yang memperbaruinya.
        /// </summary>
        [Required]
        public Guid OriginalAuthorEmployeeId { get; set; }

        /// <summary>Waktu penulisan versi ini - <b>bukan</b> waktu pembaruannya.</summary>
        [Required]
        public DateTime OriginalAuthoredAt { get; set; }

        public CliNursingCarePlanItem? CarePlanItem { get; set; }

        public MstEmployee? OriginalAuthorEmployee { get; set; }
    }
}
