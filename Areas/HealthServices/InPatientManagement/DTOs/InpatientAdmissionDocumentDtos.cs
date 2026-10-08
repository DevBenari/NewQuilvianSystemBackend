using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs
{
    // =====================================================================
    // Workspace PPRI — dokumen admisi bertanda tangan. Kontrak episode-rawat-inap 0.11.0
    // API 12.2 dan 12.3 (BE-RWI-194, 199 s.d. 202).
    //
    // Request TIDAK PERNAH memuat identitas pasien (INV-RWA-05) maupun angka deposit atau harga
    // bertarif (INV-RWA-06). Bentuk dan panjang isian diperiksa service supaya penolakannya
    // membawa kalimat validation matrix 15, bukan pesan bawaan validasi model.
    // =====================================================================

    /// <summary>Penanda tangan atau deklarer yang dinyatakan dokumen.</summary>
    /// <remarks>
    /// Bila <see cref="SourceType"/> bukan <c>Manual</c>, server membaca ulang nama, alamat, dan
    /// telepon dari service pemilik dan mengabaikan isian klien. Contoh: klien mengirim nama "Rina"
    /// untuk relasi <c>Spouse</c> yang tercatat "Rina Santoso" → tersimpan "Rina Santoso".
    /// </remarks>
    public class AdmissionPartyInput
    {
        /// <summary>Kosong berarti <c>Manual</c>.</summary>
        public InpAdmissionPartySource? SourceType { get; set; }

        /// <summary>Id relasi atau kontak darurat; wajib bila sumbernya relasi atau kontak darurat.</summary>
        public Guid? SourceRecordId { get; set; }

        public string? FullName { get; set; }

        public InpAdmissionPartyRelationship? Relationship { get; set; }

        public string? RelationshipText { get; set; }

        public string? Address { get; set; }

        public DateTime? BirthDate { get; set; }

        public Gender? Gender { get; set; }

        public string? Occupation { get; set; }

        public InpAdmissionPartyIdentityType? IdentityType { get; set; }

        public string? IdentityNumber { get; set; }

        /// <summary>Tanda hubung, spasi, dan titik dibuang; maksimal 13 digit.</summary>
        public string? MobilePhone { get; set; }

        public string? OfficePhone { get; set; }
    }

    public class AdmissionHandoverItemInput
    {
        public Guid ClearanceItemId { get; set; }

        public InpHandoverItemChoice? Choice { get; set; }

        public string? Note { get; set; }
    }

    public class AdmissionPrivacyInput
    {
        public bool IsTransportPrivacyRequested { get; set; }

        /// <summary>Maksimal 3 baris; satu nama satu baris.</summary>
        public List<string>? AllowedVisitors { get; set; }

        /// <summary>Maksimal 3 baris.</summary>
        public List<string>? SpecialRequests { get; set; }
    }

    public class AdmissionCostDifferenceInput
    {
        public InpCostDifferenceSubject? Subject { get; set; }

        /// <summary>Wajib saat kunci bila subjek "saudara kandung lainnya".</summary>
        public string? SubjectOtherText { get; set; }
    }

    public class AdmissionDepositInput
    {
        /// <summary>Tanggal jatuh tempo; jamnya selalu 11.00 waktu rumah sakit.</summary>
        public DateTime? DueDate { get; set; }
    }

    /// <summary>Isi dokumen yang dapat disunting selama <c>Draft</c>.</summary>
    public abstract class AdmissionDocumentContentInput
    {
        /// <summary>Bawaan dari pengaturan Rawat Inap saat dibuat.</summary>
        public string? SigningCity { get; set; }

        /// <summary>"Tanggal" formulir; tanggal surat Pelunasan Deposit. Bawaan hari ini.</summary>
        public DateTime? StatementDate { get; set; }

        public string? Note { get; set; }

        public AdmissionPartyInput? Party { get; set; }

        public List<AdmissionHandoverItemInput>? HandoverItems { get; set; }

        public AdmissionPrivacyInput? Privacy { get; set; }

        /// <summary>1–5 butir hal yang bertentangan.</summary>
        public List<string>? BeliefItems { get; set; }

        public AdmissionCostDifferenceInput? CostDifference { get; set; }

        public AdmissionDepositInput? Deposit { get; set; }
    }

    public class CreateAdmissionDocumentRequest : AdmissionDocumentContentInput
    {
        public InpAdmissionDocumentType? DocumentType { get; set; }
    }

    public class UpdateAdmissionDocumentRequest : AdmissionDocumentContentInput
    {
        public Guid? RowVersion { get; set; }
    }

    public class RowVersionRequest
    {
        public Guid? RowVersion { get; set; }
    }

    public class ReasonRequest
    {
        public Guid? RowVersion { get; set; }

        /// <summary>Buang konsep 1–500 karakter; batal 10–500 karakter.</summary>
        public string? Reason { get; set; }
    }

    public class ReviseAdmissionDocumentRequest
    {
        public Guid? RowVersion { get; set; }

        /// <summary>10–500 karakter.</summary>
        public string? CorrectionReason { get; set; }
    }

    /// <summary>Catatan lembar kertas yang sudah ditandatangani pasien atau keluarga.</summary>
    public class RecordPaperSignatureRequest
    {
        public Guid? RowVersion { get; set; }

        /// <summary>1–200 karakter.</summary>
        public string? SignerName { get; set; }

        public InpAdmissionPartyRelationship? SignerRelationship { get; set; }

        public string? SignerRelationshipText { get; set; }

        /// <summary>Tidak sebelum dokumen dikunci dan tidak lebih dari 5 menit di depan waktu server.</summary>
        public DateTime? SignedAt { get; set; }
    }

    // ---------------------------------------------------------------------
    // Respons dokumen
    // ---------------------------------------------------------------------

    public class AdmissionDocumentSummaryResponse
    {
        public Guid Id { get; set; }

        public int DocumentType { get; set; }

        public string DocumentTypeName { get; set; } = string.Empty;

        public string DocumentTypeLabel { get; set; } = string.Empty;

        public int Status { get; set; }

        public string StatusName { get; set; } = string.Empty;

        public string StatusLabel { get; set; } = string.Empty;

        public int VersionNo { get; set; }

        public Guid? PreviousVersionId { get; set; }

        public DateTime CreatedAt { get; set; }

        public string? CreatedByName { get; set; }

        public DateTime? LockedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public DateTime? CancelledAt { get; set; }
    }

    /// <summary>Satu dokumen lengkap tanpa rupiah.</summary>
    public class AdmissionDocumentResponse : AdmissionDocumentSummaryResponse
    {
        public Guid RowVersion { get; set; }

        public bool IsReadOnly { get; set; }

        /// <summary>
        /// Bantuan tampilan, bukan pengaman: <c>Update</c>, <c>Lock</c>, <c>Unlock</c>, <c>Revise</c>,
        /// <c>Discard</c>, <c>Cancel</c>, <c>SignPatientOrFamily</c>, <c>SignAdmissionOfficer</c>,
        /// <c>SignCro</c>, <c>SignReceivingNurse</c>, <c>SignHeadNurse</c>, <c>Print</c>, <c>AmountPrint</c>.
        /// </summary>
        public List<string> AvailableActions { get; set; } = new();

        public string? SigningCity { get; set; }

        public DateTime? StatementDate { get; set; }

        public string? Note { get; set; }

        public string? CorrectionReason { get; set; }

        public string? CancelledReason { get; set; }

        public List<AdmissionSignatureSlotResponse> Slots { get; set; } = new();

        public AdmissionPartyResponse? Party { get; set; }

        public List<AdmissionHandoverItemResponse> HandoverItems { get; set; } = new();

        public AdmissionPrivacyResponse? Privacy { get; set; }

        public List<AdmissionBeliefItemResponse> BeliefItems { get; set; } = new();

        public AdmissionCostDifferenceResponse? CostDifference { get; set; }

        public AdmissionDepositResponse? Deposit { get; set; }

        /// <summary>Data pasien berubah sesudah dokumen dikunci (<c>FR-RWA-124</c>).</summary>
        public bool SourceChangedSinceLock { get; set; }

        public string? SourceChangedMessage { get; set; }
    }

    public class AdmissionSignatureSlotResponse
    {
        public int Slot { get; set; }

        public string SlotName { get; set; } = string.Empty;

        /// <summary>Label cetak V1, misalnya "Kepala Ruangan".</summary>
        public string Label { get; set; } = string.Empty;

        public AdmissionSignatureResponse? Signature { get; set; }
    }

    public class AdmissionSignatureResponse
    {
        public int Method { get; set; }

        public string MethodName { get; set; } = string.Empty;

        public string SignerName { get; set; } = string.Empty;

        public string? SignerPositionName { get; set; }

        public int? SignerRelationship { get; set; }

        public string? SignerRelationshipText { get; set; }

        public DateTime SignedAt { get; set; }

        public string? VerifiedByName { get; set; }
    }

    public class AdmissionPartyResponse
    {
        public int SourceType { get; set; }

        public string SourceTypeName { get; set; } = string.Empty;

        public Guid? SourceRecordId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public int? Relationship { get; set; }

        public string? RelationshipName { get; set; }

        public string? RelationshipText { get; set; }

        public string? Address { get; set; }

        public DateTime? BirthDate { get; set; }

        public int? Gender { get; set; }

        public string? Occupation { get; set; }

        public int? IdentityType { get; set; }

        public string? IdentityNumber { get; set; }

        public string? MobilePhone { get; set; }

        public string? OfficePhone { get; set; }
    }

    public class AdmissionHandoverItemResponse
    {
        public Guid ClearanceItemId { get; set; }

        public int LineNo { get; set; }

        /// <summary>Nomor butir induk tercetak; kosong untuk sub-butir.</summary>
        public int? ItemNumber { get; set; }

        public int? ParentItemNumber { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public int? Choice { get; set; }

        public string? Note { get; set; }

        /// <summary>Saran sistem; hanya selama <c>Draft</c> dan tidak pernah memilih otomatis.</summary>
        public AdmissionSuggestionResponse? Suggestion { get; set; }
    }

    public class AdmissionSuggestionResponse
    {
        /// <summary>Contoh "Sudah — saran sistem (surat pengantar dr. Andika, 07-10-2026)".</summary>
        public string Text { get; set; } = string.Empty;
    }

    public class AdmissionPrivacyResponse
    {
        public bool IsTransportPrivacyRequested { get; set; }

        public List<string> AllowedVisitors { get; set; } = new();

        public List<string> SpecialRequests { get; set; } = new();
    }

    public class AdmissionBeliefItemResponse
    {
        public int ItemNo { get; set; }

        public string Text { get; set; } = string.Empty;
    }

    public class AdmissionCostDifferenceResponse
    {
        public int Subject { get; set; }

        public string SubjectName { get; set; } = string.Empty;

        public string SubjectLabel { get; set; } = string.Empty;

        public string? SubjectOtherText { get; set; }
    }

    /// <summary>Isi Pelunasan Deposit tanpa rupiah; angka hanya lewat <c>/amounts</c>.</summary>
    public class AdmissionDepositResponse
    {
        public DateTime? DueAt { get; set; }

        public bool AmountsHidden { get; set; } = true;
    }

    /// <summary>Angka dokumen — hanya <c>ViewAmount</c>.</summary>
    public class AdmissionDocumentAmountsResponse
    {
        public AdmissionDepositAmountsResponse? Deposit { get; set; }
    }

    public class AdmissionDepositAmountsResponse
    {
        public decimal? MinimumPolicyAmount { get; set; }

        public decimal? ReceivedAmount { get; set; }

        public decimal? ShortfallAmount { get; set; }

        /// <summary>Contoh "Rp 3.000.000 (Rp 5.000.000 − Rp 2.000.000)".</summary>
        public string? CalculationText { get; set; }

        public DateTime? ReadAt { get; set; }

        /// <summary>Benar sesudah dikunci: angka beku, tidak lagi mengikuti Billing (<c>RWI-DEC-263</c>).</summary>
        public bool IsFrozen { get; set; }
    }

    // ---------------------------------------------------------------------
    // Isian bawaan
    // ---------------------------------------------------------------------

    public class AdmissionDocumentPrefillResponse
    {
        public int DocumentType { get; set; }

        public string DocumentTypeName { get; set; } = string.Empty;

        public bool CanCreate { get; set; }

        /// <summary>
        /// <c>NotRequiredForPayer</c>, <c>NoDepositShortfall</c>, <c>DepositUnavailable</c>,
        /// <c>ActiveDocumentExists</c>, <c>GuarantorUnavailable</c>, <c>EpisodeNotWritable</c>,
        /// atau <c>HandoverItemsMissing</c>.
        /// </summary>
        public string? CannotCreateReasonCode { get; set; }

        public string? CannotCreateReason { get; set; }

        public Guid? ActiveDocumentId { get; set; }

        public string? SigningCity { get; set; }

        public DateTime? StatementDate { get; set; }

        public List<AdmissionPartyCandidateResponse> PartyCandidates { get; set; } = new();

        public List<AdmissionHandoverItemResponse> HandoverItems { get; set; } = new();

        public List<string> PreviousBeliefItems { get; set; } = new();

        public Guid? PreviousBeliefDocumentId { get; set; }

        /// <summary>Selisih Biaya "diri saya sendiri": data deklarer dari master pasien.</summary>
        public AdmissionPartyResponse? PatientAsDeclarer { get; set; }

        public DateTime? DefaultDueDate { get; set; }

        public DateTime? MaxDueDate { get; set; }

        public List<string> Warnings { get; set; } = new();
    }

    /// <summary>Satu calon penanda tangan; hanya isian yang dibutuhkan dokumen (<c>RWI-DEC-257</c>).</summary>
    public class AdmissionPartyCandidateResponse
    {
        /// <summary><c>PatientRelationship</c> atau <c>EmergencyContact</c>.</summary>
        public string Source { get; set; } = string.Empty;

        public Guid SourceRecordId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? RelationshipType { get; set; }

        public string? RelationshipText { get; set; }

        public string? Address { get; set; }

        public string? PhoneNumber { get; set; }

        public bool IsPrimary { get; set; }

        public bool IsResponsiblePerson { get; set; }
    }

    // ---------------------------------------------------------------------
    // Data cetak dokumen
    // ---------------------------------------------------------------------

    public class AdmissionDocumentPrintResponse
    {
        public Guid DocumentId { get; set; }

        public int DocumentType { get; set; }

        public string DocumentTypeName { get; set; } = string.Empty;

        public int Status { get; set; }

        public string StatusName { get; set; } = string.Empty;

        public int VersionNo { get; set; }

        /// <summary><c>Draft</c>, <c>SignatureSheet</c>, <c>Final</c>, <c>Superseded</c>, atau <c>Cancelled</c>.</summary>
        public string PrintMarker { get; set; } = string.Empty;

        /// <summary>Contoh "KONSEP — BELUM DITANDATANGANI" atau "Lembar untuk ditandatangani — versi 1".</summary>
        public string? PrintMarkerText { get; set; }

        /// <summary>"ADMISI DIBATALKAN" bila episodenya dibatalkan.</summary>
        public string? EpisodeCancelledMarker { get; set; }

        /// <summary>Urutan cetakan berikutnya untuk status ini; kosong untuk konsep.</summary>
        public int? NextPrintSequence { get; set; }

        public LetterheadResponse Letterhead { get; set; } = new();

        public string? FormCode { get; set; }

        public string? SigningCity { get; set; }

        public DateTime? StatementDate { get; set; }

        public AdmissionPrintPatientResponse Patient { get; set; } = new();

        public AdmissionPrintEpisodeResponse Episode { get; set; } = new();

        public AdmissionPrintGuarantorResponse? Guarantor { get; set; }

        public string? Note { get; set; }

        public AdmissionPartyResponse? Party { get; set; }

        public List<AdmissionHandoverItemResponse> HandoverItems { get; set; } = new();

        public AdmissionPrivacyResponse? Privacy { get; set; }

        public List<AdmissionBeliefItemResponse> BeliefItems { get; set; } = new();

        public AdmissionCostDifferenceResponse? CostDifference { get; set; }

        /// <summary>Hanya jalur berupiah yang mengisi angkanya.</summary>
        public AdmissionPrintDepositResponse? Deposit { get; set; }

        public List<AdmissionSignatureLineResponse> SignatureLines { get; set; } = new();
    }

    public class AdmissionPrintPatientResponse
    {
        public string? FullName { get; set; }

        public string? Salutation { get; set; }

        public string? MedicalRecordNumber { get; set; }

        public DateTime? BirthDate { get; set; }

        public string? GenderName { get; set; }

        public string? Religion { get; set; }

        public string? Address { get; set; }
    }

    public class AdmissionPrintEpisodeResponse
    {
        public string? EpisodeNumber { get; set; }

        public DateTime? AdmittedAt { get; set; }

        public string? PatientClassName { get; set; }

        public string? ServiceUnitName { get; set; }

        public string? RoomName { get; set; }

        public string? BedName { get; set; }

        public string? AttendingDoctorName { get; set; }
    }

    public class AdmissionPrintGuarantorResponse
    {
        public string? PaymentTypeName { get; set; }

        public string? GuarantorName { get; set; }

        public string? PolicyNumber { get; set; }

        public string? MemberNumber { get; set; }

        public string? CardNumber { get; set; }
    }

    public class AdmissionPrintDepositResponse
    {
        public DateTime? DueAt { get; set; }

        /// <summary>Contoh "12 Oktober 2026 pukul 11.00".</summary>
        public string? DueAtText { get; set; }

        public decimal? MinimumPolicyAmount { get; set; }

        public decimal? ReceivedAmount { get; set; }

        public decimal? ShortfallAmount { get; set; }

        public string? CalculationText { get; set; }

        public DateTime? AmountsReadAt { get; set; }
    }

    public class AdmissionSignatureLineResponse
    {
        public int Slot { get; set; }

        public string SlotName { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;

        public bool IsSigned { get; set; }

        /// <summary>
        /// Atestasi: "Ditandatangani secara elektronik oleh <i>nama</i>, <i>jabatan</i>, <i>waktu</i>".
        /// Kertas: "Ditandatangani di kertas oleh <i>nama</i> (<i>hubungan</i>), <i>waktu</i>, diverifikasi <i>petugas</i>".
        /// </summary>
        public string? Text { get; set; }
    }
}
