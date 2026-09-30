using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs
{
    // =========================================================================
    // BE-RWI-061 - pencatatan tindakan keperawatan
    // =========================================================================

    /// <summary>Permintaan mencatat satu tindakan keperawatan yang sudah dilakukan.</summary>
    public class CreateNursingInterventionRequest
    {
        /// <summary>Kunjungan yang menaungi perawatan rawat inap pasien.</summary>
        [Required]
        public Guid EncounterId { get; set; }

        /// <summary>
        /// Perawatan rawat inap yang dituju. Boleh kosong; bila diisi dan tidak cocok dengan
        /// perawatan milik kunjungan itu, permintaan ditolak.
        /// </summary>
        public Guid? InpEpisodeId { get; set; }

        /// <summary>
        /// Butir rencana asuhan yang mendasari tindakan. <b>Boleh kosong</b> - tindakan mendadak
        /// tetap harus dapat dicatat (<c>CAP-014</c> aturan 3).
        /// </summary>
        public Guid? CarePlanItemId { get; set; }

        /// <summary>Apa yang dikerjakan.</summary>
        [Required]
        [MaxLength(300)]
        public string InterventionName { get; set; } = string.Empty;

        /// <summary>
        /// Waktu tindakan dikerjakan. Kosong berarti sekarang.
        /// </summary>
        public DateTime? PerformedAt { get; set; }

        /// <summary>
        /// Perawat yang mengerjakan tindakan. Kosong berarti perawat yang sedang masuk.
        /// </summary>
        /// <remarks>
        /// Diisi ketika seorang perawat mencatatkan tindakan rekannya yang sedang sibuk.
        /// Pencatatnya sendiri tetap tersimpan terpisah pada <c>CreateBy</c>, sehingga
        /// pertanggungjawaban keduanya tidak tertukar.
        /// </remarks>
        public Guid? PerformedByEmployeeId { get; set; }

        /// <summary>Hasil tindakan beserta respons pasien.</summary>
        public string? ResultNote { get; set; }

        /// <summary>Apakah tindakan ini dapat ditagih.</summary>
        public bool IsBillable { get; set; }

        /// <summary>
        /// Kunci permintaan. Boleh dikirim di sini atau lewat header <c>Idempotency-Key</c>;
        /// bila keduanya terisi, isi badan permintaan yang dipakai.
        /// </summary>
        [MaxLength(100)]
        public string? IdempotencyKey { get; set; }
    }

    /// <summary>Satu catatan tindakan keperawatan.</summary>
    public class NursingInterventionResponse
    {
        public Guid Id { get; set; }

        public Guid EncounterId { get; set; }

        public Guid? InpEpisodeId { get; set; }

        public Guid PatientId { get; set; }

        public Guid? CarePlanItemId { get; set; }

        /// <summary>Masalah keperawatan yang mendasari tindakan, bila ada.</summary>
        public string? CarePlanProblemStatement { get; set; }

        public string InterventionName { get; set; } = string.Empty;

        public DateTime PerformedAt { get; set; }

        public Guid PerformedByEmployeeId { get; set; }

        public string? PerformedByEmployeeName { get; set; }

        public string? ResultNote { get; set; }

        public NursingInterventionStatus RecordStatus { get; set; }

        /// <summary>Nama keadaan catatan dalam Bahasa Indonesia, siap ditampilkan.</summary>
        public string RecordStatusLabel { get; set; } = string.Empty;

        public DateTime? FinalizedAt { get; set; }

        public bool IsBillable { get; set; }

        public NursingBillingDispatchStatus BillingDispatchStatus { get; set; }

        public string BillingDispatchStatusLabel { get; set; } = string.Empty;

        public DateTime CreateDateTime { get; set; }

        public DateTime? UpdateDateTime { get; set; }

        /// <summary>
        /// Benar ketika catatan ini sudah ada sebelumnya dan permintaan tadi adalah kiriman ulang
        /// dengan kunci yang sama - <c>VAL-KEP-15</c>.
        /// </summary>
        public bool IsReplay { get; set; }
    }

    /// <summary>Satu baris pada daftar tindakan satu perawatan.</summary>
    public class NursingInterventionListItem
    {
        public Guid Id { get; set; }

        public string InterventionName { get; set; } = string.Empty;

        public DateTime PerformedAt { get; set; }

        public Guid PerformedByEmployeeId { get; set; }

        public string? PerformedByEmployeeName { get; set; }

        public Guid? CarePlanItemId { get; set; }

        public NursingInterventionStatus RecordStatus { get; set; }

        public string RecordStatusLabel { get; set; } = string.Empty;

        public NursingBillingDispatchStatus BillingDispatchStatus { get; set; }

        public string BillingDispatchStatusLabel { get; set; } = string.Empty;

        public bool IsBillable { get; set; }
    }

    // =========================================================================
    // BE-RWI-062 - finalisasi, koreksi, dan keadaan pengiriman tagihan
    // =========================================================================

    /// <summary>
    /// Permintaan menyunting isi catatan tindakan yang <b>belum</b> final.
    /// </summary>
    /// <remarks>
    /// Sesudah catatan final, penyuntingan ditolak dan yang berlaku adalah koreksi bernomor -
    /// <c>state-transition-matrix.md</c> bagian 3.
    /// </remarks>
    public class UpdateNursingInterventionRequest
    {
        /// <summary>Apa yang dikerjakan.</summary>
        [Required]
        [MaxLength(300)]
        public string InterventionName { get; set; } = string.Empty;

        /// <summary>Hasil tindakan beserta respons pasien.</summary>
        public string? ResultNote { get; set; }
    }

    /// <summary>Permintaan menambah koreksi pada catatan tindakan yang sudah final.</summary>
    public class CreateInterventionAddendumRequest
    {
        /// <summary>Isi koreksi. Wajib diisi.</summary>
        [Required]
        public string Content { get; set; } = string.Empty;

        /// <summary>Alasan koreksi. Wajib diisi - <c>VAL-KEP-12</c>.</summary>
        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>
    /// Keadaan pengiriman tagihan satu tindakan - <c>AC-CAP014-02</c>.
    /// </summary>
    /// <remarks>
    /// Sengaja memuat <b>kedua</b> mesin status sekaligus, supaya pembaca langsung melihat bahwa
    /// keduanya tidak saling mengunci: catatan yang <c>Finalized</c> dapat berdampingan dengan
    /// pengiriman yang <c>Failed</c>.
    /// </remarks>
    public class BillingDispatchResponse
    {
        public Guid InterventionId { get; set; }

        public bool IsBillable { get; set; }

        public NursingBillingDispatchStatus BillingDispatchStatus { get; set; }

        public string BillingDispatchStatusLabel { get; set; } = string.Empty;

        public DateTime? BillingDispatchedAt { get; set; }

        public int BillingDispatchAttemptCount { get; set; }

        public string? BillingDispatchFailureReason { get; set; }

        /// <summary>Keadaan klinis catatan, yang tidak pernah dipengaruhi keadaan tagihan.</summary>
        public NursingInterventionStatus RecordStatus { get; set; }

        public string RecordStatusLabel { get; set; } = string.Empty;

        /// <summary>Kalimat siap tampil yang menjelaskan keadaan pengiriman bagi pengguna.</summary>
        public string Message { get; set; } = string.Empty;
    }

    // =========================================================================
    // BE-RWI-140 - Pencatatan Tindakan Harian Massal (Batch Daily Interventions)
    // =========================================================================

    /// <summary>
    /// Satu butir tindakan keperawatan harian dalam permintaan batch.
    /// </summary>
    public class BatchNursingInterventionItemRequest
    {
        /// <summary>Kode template standar (misal: ACT_O2, ACT_SUCTION).</summary>
        public string? TemplateCode { get; set; }

        /// <summary>Nama tindakan keperawatan yang dilakukan.</summary>
        [Required]
        [MaxLength(300)]
        public string InterventionName { get; set; } = string.Empty;

        /// <summary>Waktu pelaksanaan riil tindakan.</summary>
        public DateTime? PerformedAt { get; set; }

        /// <summary>ID Pegawai perawat yang melaksanakan (opsional, fallback ke perawat login).</summary>
        public Guid? PerformedByEmployeeId { get; set; }

        /// <summary>Nama perawat pelaksana (untuk audit trail / preview tampilan).</summary>
        public string? PerformedByEmployeeName { get; set; }

        /// <summary>Keterangan, hasil tindakan, atau catatan respons pasien.</summary>
        public string? ActionNotes { get; set; }

        /// <summary>Butir rencana asuhan terkait (opsional).</summary>
        public Guid? CarePlanItemId { get; set; }

        /// <summary>Apakah tindakan dapat ditagihkan.</summary>
        public bool IsBillable { get; set; }

        /// <summary>Kunci idempotensi per butir (opsional).</summary>
        [MaxLength(100)]
        public string? IdempotencyKey { get; set; }
    }

    /// <summary>
    /// Permintaan mencatat sekumpulan tindakan keperawatan harian sekaligus (Lembar Keperawatan Harian).
    /// </summary>
    public class CreateBatchNursingInterventionRequest
    {
        /// <summary>Kunjungan pasien rawat inap.</summary>
        [Required]
        public Guid EncounterId { get; set; }

        /// <summary>Episode rawat inap yang menaungi tindakan.</summary>
        public Guid? InpEpisodeId { get; set; }

        /// <summary>Nama shift pelayanan (Pagi / Siang / Malam).</summary>
        public string? Shift { get; set; }

        /// <summary>Tanggal pelaksanaan tindakan (format YYYY-MM-DD).</summary>
        public string? ActionDate { get; set; }

        /// <summary>Kunci idempotensi batch untuk mencegah double-submit jaringan.</summary>
        [MaxLength(100)]
        public string? IdempotencyKey { get; set; }

        /// <summary>Daftar tindakan yang telah dilaksanakan.</summary>
        [Required]
        [MinLength(1, ErrorMessage = "Daftar tindakan wajib berisi minimal 1 butir tindakan.")]
        public List<BatchNursingInterventionItemRequest> Items { get; set; } = new();
    }

    /// <summary>
    /// Respons hasil penyimpanan massal tindakan keperawatan harian.
    /// </summary>
    public class BatchNursingInterventionResponse
    {
        public int TotalRequested { get; set; }

        public int TotalCreated { get; set; }

        public bool IsReplay { get; set; }

        public List<NursingInterventionResponse> SavedInterventions { get; set; } = new();
    }

    /// <summary>
    /// DTO definisi katalog 19 template tindakan keperawatan harian standar rawat inap.
    /// </summary>
    public class NursingDailyActionTemplateDto
    {
        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string? DefaultNotes { get; set; }

        public int SortOrder { get; set; }
    }
}
