using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs
{
    /// <summary>
    /// Bentuk permintaan DPJP menyatakan pasien boleh pulang.
    /// </summary>
    /// <remarks>
    /// <b>Tempat tidur belum dilepas pada langkah ini.</b> Episode berpindah ke
    /// <c>DischargePending</c>, pasien tetap muncul pada census, dan salinan status tempat
    /// tidur tidak berubah. Pelepasannya baru terjadi ketika kepergian fisik dicatat
    /// (<c>BE-RWI-027</c>) atau episode ditutup (<c>BE-RWI-025</c>).
    /// </remarks>
    public class DecideDischargeRequest
    {
        /// <summary>
        /// Cara pulang. Nilai yang berlaku pada revisi ini: 1 <c>DoctorApproved</c>,
        /// 2 <c>AgainstMedicalAdvice</c>, 3 <c>Referred</c>.
        /// </summary>
        public int DischargeType { get; set; }

        /// <summary>
        /// Alasan keputusan pulang. Ikut tersimpan pada baris riwayat status episode.
        /// </summary>
        /// <remarks>
        /// <b>Tujuan rujukan sengaja tidak ada di sini.</b> Ia milik resume pulang, dan
        /// diwajibkan pada saat resume ditandatangani — validation matrix bagian 6. Menyediakan
        /// kolomnya di sini akan membuat dua tempat menyimpan nilai yang sama, dan keduanya
        /// akan berselisih pada kasus pertama yang tujuannya berubah.
        /// </remarks>
        [MaxLength(500)]
        public string? Reason { get; set; }
    }

    /// <summary>
    /// Bentuk permintaan menyusun atau memperbarui resume pulang.
    /// </summary>
    /// <remarks>
    /// Seluruh kolom isi resume bertanda <b>sensitif</b> pada permission matrix bagian 5.4.
    /// Tidak satu pun boleh masuk payload logger, dan tidak satu pun boleh ikut pada endpoint
    /// daftar mana pun.
    /// </remarks>
    public class UpsertDischargeSummaryRequest
    {
        [Required]
        [MaxLength(1000)]
        public string PrimaryDiagnosisText { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? SecondaryDiagnosisText { get; set; }

        [MaxLength(2000)]
        public string? ProcedureSummary { get; set; }

        [MaxLength(2000)]
        public string? DischargeMedicationNote { get; set; }

        [MaxLength(2000)]
        public string? FollowUpInstruction { get; set; }

        [MaxLength(250)]
        public string? ReferralDestination { get; set; }

        [MaxLength(4000)]
        public string? ClinicalSummary { get; set; }
    }

    /// <summary>
    /// Bentuk permintaan DPJP menandatangani resume pulang.
    /// </summary>
    /// <remarks>
    /// <b>Tidak ada kolom penandatangan.</b> Penandatangan diturunkan dari DPJP aktif episode
    /// dan dari pengguna yang terautentikasi, tidak pernah dari isian permintaan. Menerimanya
    /// dari pemanggil membuat <c>GUARD-INP-03</c> dapat dilewati hanya dengan mengirim
    /// identifier dokter lain.
    /// </remarks>
    public class SignDischargeSummaryRequest
    {
        [MaxLength(500)]
        public string? Note { get; set; }
    }

    /// <summary>Resume pulang satu episode.</summary>
    public class DischargeSummaryResponse
    {
        public Guid Id { get; set; }

        public Guid EpisodeId { get; set; }

        public string? EpisodeNumber { get; set; }

        public string PrimaryDiagnosisText { get; set; } = string.Empty;

        public string? SecondaryDiagnosisText { get; set; }

        public string? ProcedureSummary { get; set; }

        public string? DischargeMedicationNote { get; set; }

        public string? FollowUpInstruction { get; set; }

        public string? ReferralDestination { get; set; }

        public string? ClinicalSummary { get; set; }

        public DateTime? SignedAt { get; set; }

        public Guid? SignedByDoctorId { get; set; }

        public string? SignedByDoctorName { get; set; }

        public bool IsSigned { get; set; }

        public int DischargeType { get; set; }

        public string DischargeTypeName { get; set; } = string.Empty;

        public DateTime CreateDateTime { get; set; }

        public DateTime? UpdateDateTime { get; set; }

        /// <summary>
        /// Versi resume yang sudah digantikan, urut waktu. Hanya terisi bila pemanggil
        /// meminta <c>includeRevisions=true</c>.
        /// </summary>
        public List<DischargeSummaryRevisionResponse> Revisions { get; set; } = new();
    }

    /// <summary>
    /// Salinan satu versi resume yang sudah digantikan.
    /// </summary>
    /// <remarks>
    /// Baris versi <b>tidak dapat diubah dan tidak dapat dihapus</b>. Tidak ada endpoint
    /// <c>PUT</c> maupun <c>DELETE</c> yang menunjuk baris ini, dan ketiadaannya disengaja —
    /// api contract bagian 8 dan <c>RWI-DEC-057</c>.
    /// </remarks>
    public class DischargeSummaryRevisionResponse
    {
        public Guid Id { get; set; }

        public Guid DischargeSummaryId { get; set; }

        public int RevisionNumber { get; set; }

        public Guid? CorrectionSessionId { get; set; }

        public string PrimaryDiagnosisText { get; set; } = string.Empty;

        public string? SecondaryDiagnosisText { get; set; }

        public string? ProcedureSummary { get; set; }

        public string? DischargeMedicationNote { get; set; }

        public string? FollowUpInstruction { get; set; }

        public string? ReferralDestination { get; set; }

        public string? ClinicalSummary { get; set; }

        public int PreviousDischargeType { get; set; }

        public string PreviousDischargeTypeName { get; set; } = string.Empty;

        public DateTime PreviousSignedAt { get; set; }

        public Guid PreviousSignedByDoctorId { get; set; }

        public string? PreviousSignedByDoctorName { get; set; }

        public DateTime SupersededAt { get; set; }

        public Guid SupersededByUserId { get; set; }
    }
}
