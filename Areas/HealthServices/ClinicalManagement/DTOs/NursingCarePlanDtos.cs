using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs
{
    // =========================================================================
    // BE-RWI-059 - rencana asuhan keperawatan beserta butirnya
    // =========================================================================

    /// <summary>
    /// Permintaan membuka rencana asuhan keperawatan bagi satu perawatan rawat inap.
    /// </summary>
    /// <remarks>
    /// Tidak memuat penanda perawat. Perawat yang membuka rencana diturunkan dari pengguna yang
    /// terautentikasi; menerimanya dari klien berarti siapa pun dapat mengaku membuka rencana
    /// atas nama perawat lain.
    /// </remarks>
    public class CreateNursingCarePlanRequest
    {
        /// <summary>Kunjungan yang menaungi perawatan rawat inap pasien.</summary>
        [Required]
        public Guid EncounterId { get; set; }

        /// <summary>
        /// Perawatan rawat inap yang dituju. Boleh kosong; bila diisi dan tidak cocok dengan
        /// perawatan milik kunjungan itu, permintaan ditolak.
        /// </summary>
        public Guid? InpEpisodeId { get; set; }
    }

    /// <summary>Permintaan menambah satu masalah keperawatan pada rencana asuhan.</summary>
    public class CreateCarePlanItemRequest
    {
        /// <summary>Masalah keperawatan pasien, ditulis sebagai teks.</summary>
        [Required]
        [MaxLength(500)]
        public string ProblemStatement { get; set; } = string.Empty;

        /// <summary>Tujuan asuhan yang hendak dicapai.</summary>
        [MaxLength(500)]
        public string? GoalStatement { get; set; }

        /// <summary>Rencana tindakan yang disusun perawat.</summary>
        public string? PlannedIntervention { get; set; }

        /// <summary>
        /// Penunjuk katalog terminologi keperawatan, bila kelak rumah sakit memakainya.
        /// </summary>
        public Guid? NursingDiagnosisId { get; set; }

        /// <summary>Pengkajian yang menjadi asal masalah ini — <c>AC-CAP013-01</c>.</summary>
        public Guid? SourceAssessmentId { get; set; }
    }

    /// <summary>
    /// Permintaan memperbarui isi satu butir masalah keperawatan.
    /// </summary>
    /// <remarks>
    /// <c>BE-RWI-060</c>. Pembaruan <b>tidak menimpa</b> isi lama: versi sebelumnya disalin lebih
    /// dulu ke riwayat versi beserta penulis dan waktu aslinya - <c>AC-CAP013-02</c>.
    /// </remarks>
    public class UpdateCarePlanItemRequest
    {
        /// <summary>Masalah keperawatan pada versi baru.</summary>
        [Required]
        [MaxLength(500)]
        public string ProblemStatement { get; set; } = string.Empty;

        /// <summary>Tujuan asuhan pada versi baru.</summary>
        [MaxLength(500)]
        public string? GoalStatement { get; set; }

        /// <summary>Rencana tindakan pada versi baru.</summary>
        public string? PlannedIntervention { get; set; }

        /// <summary>Penunjuk katalog terminologi keperawatan pada versi baru.</summary>
        public Guid? NursingDiagnosisId { get; set; }
    }

    /// <summary>Permintaan mencatat evaluasi hasil asuhan pada satu butir.</summary>
    public class EvaluateCarePlanItemRequest
    {
        /// <summary>Catatan evaluasi. Wajib diisi.</summary>
        [Required]
        public string EvaluationNote { get; set; } = string.Empty;
    }

    /// <summary>
    /// Permintaan menutup satu masalah keperawatan.
    /// </summary>
    /// <remarks>
    /// <c>TargetStatus</c> membedakan dua penutupan yang artinya sama sekali berbeda:
    /// <c>Resolved</c> berarti masalahnya teratasi dan menuntut evaluasi
    /// (<c>VAL-KEP-16</c>); <c>Discontinued</c> berarti masalahnya tidak lagi relevan.
    /// </remarks>
    public class CloseCarePlanItemRequest
    {
        /// <summary>Keadaan tujuan: <c>Resolved</c> atau <c>Discontinued</c>.</summary>
        [Required]
        public NursingCarePlanItemStatus TargetStatus { get; set; } = NursingCarePlanItemStatus.Resolved;

        /// <summary>Alasan penutupan. Wajib diisi.</summary>
        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>Rencana asuhan keperawatan satu perawatan beserta seluruh butirnya.</summary>
    public class NursingCarePlanResponse
    {
        public Guid Id { get; set; }

        public Guid EncounterId { get; set; }

        public Guid InpEpisodeId { get; set; }

        public string? EpisodeNumber { get; set; }

        public Guid PatientId { get; set; }

        public string? PatientName { get; set; }

        public DateTime OpenedAt { get; set; }

        public Guid OpenedByEmployeeId { get; set; }

        public string? OpenedByEmployeeName { get; set; }

        public bool IsActive { get; set; }

        /// <summary>Jumlah butir yang masih dikerjakan.</summary>
        public int ActiveItemCount { get; set; }

        public int TotalItemCount { get; set; }

        public List<CarePlanItemResponse> Items { get; set; } = [];
    }

    /// <summary>Satu butir masalah keperawatan.</summary>
    public class CarePlanItemResponse
    {
        public Guid Id { get; set; }

        public Guid CarePlanId { get; set; }

        public Guid? NursingDiagnosisId { get; set; }

        public Guid? SourceAssessmentId { get; set; }

        /// <summary>Nomor pengkajian asal, bila butir ini lahir dari sebuah pengkajian.</summary>
        public string? SourceAssessmentNumber { get; set; }

        public string ProblemStatement { get; set; } = string.Empty;

        public string? GoalStatement { get; set; }

        public string? PlannedIntervention { get; set; }

        public string? EvaluationNote { get; set; }

        public DateTime? LastEvaluatedAt { get; set; }

        public NursingCarePlanItemStatus ItemStatus { get; set; }

        /// <summary>Nama keadaan butir dalam Bahasa Indonesia, siap ditampilkan.</summary>
        public string ItemStatusLabel { get; set; } = string.Empty;

        public DateTime? ResolvedAt { get; set; }

        public string? CloseReason { get; set; }

        public int VersionNumber { get; set; }

        /// <summary>Perawat yang menulis versi butir yang sedang berlaku.</summary>
        public Guid AuthoredByEmployeeId { get; set; }

        public string? AuthoredByEmployeeName { get; set; }

        /// <summary>Saat versi butir yang sedang berlaku ditulis.</summary>
        public DateTime AuthoredAt { get; set; }

        public DateTime CreateDateTime { get; set; }

        public DateTime? UpdateDateTime { get; set; }
    }

    /// <summary>
    /// Satu versi butir yang sudah diarsipkan - <c>BE-RWI-060</c>, <c>AC-CAP013-02</c>.
    /// </summary>
    /// <remarks>
    /// <c>OriginalAuthorEmployeeId</c> dan <c>OriginalAuthoredAt</c> menyebut penulis dan waktu
    /// <b>versi ini</b>, bukan perawat yang memperbaruinya. Itulah yang membuat rekam medis tetap
    /// dapat menunjukkan siapa yang menilai pertama kali.
    /// </remarks>
    public class CarePlanItemRevisionResponse
    {
        public Guid Id { get; set; }

        public Guid CarePlanItemId { get; set; }

        public int VersionNumber { get; set; }

        public string ProblemStatement { get; set; } = string.Empty;

        public string? GoalStatement { get; set; }

        public string? PlannedIntervention { get; set; }

        public string? EvaluationNote { get; set; }

        /// <summary>Saat versi ini diarsipkan.</summary>
        public DateTime RevisedAt { get; set; }

        /// <summary>Perawat yang menulis versi ini.</summary>
        public Guid OriginalAuthorEmployeeId { get; set; }

        public string? OriginalAuthorEmployeeName { get; set; }

        /// <summary>Waktu penulisan versi ini.</summary>
        public DateTime OriginalAuthoredAt { get; set; }
    }
}
