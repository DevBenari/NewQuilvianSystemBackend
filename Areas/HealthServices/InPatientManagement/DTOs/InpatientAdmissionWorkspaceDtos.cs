using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs
{
    // =====================================================================
    // Workspace PPRI — bacaan ruang kerja, kop, cetakan tanpa siklus, dan log cetak.
    // Kontrak episode-rawat-inap 0.11.0 API 12.2 dan 12.3 (BE-RWI-193 s.d. 198, 200).
    // Respons yang dijaga InpatientAdmissionDocument : Read TIDAK PERNAH memuat rupiah
    // (RWI-DEC-258); rupiah hanya pada DTO berakhiran "Amounts".
    // =====================================================================

    /// <summary>Kop surat dari profil rumah sakit utama (<c>BE-RWI-193</c>). Kosong bila profil tidak tersedia.</summary>
    public class LetterheadResponse
    {
        public bool IsAvailable { get; set; }

        /// <summary>Alasan bila kop tidak tersedia; kop tetap dicetak tanpa identitas.</summary>
        public string? UnavailableReason { get; set; }

        public string? SiteName { get; set; }

        public string? SiteCode { get; set; }

        public List<string> AddressLines { get; set; } = new();

        public string? PhoneNumber { get; set; }

        public string? Email { get; set; }
    }

    /// <summary>Ringkasan ruang kerja tanpa rupiah (<c>BE-RWI-195</c>).</summary>
    public class AdmissionWorkspaceSummaryResponse
    {
        public Guid EpisodeId { get; set; }

        /// <summary><c>Available</c>, <c>NotYetAdmitted</c>, atau <c>ReadOnly</c>.</summary>
        public string Availability { get; set; } = "Available";

        /// <summary><c>EpisodeClosed</c> atau <c>EpisodeCancelled</c> bila hanya-baca.</summary>
        public string? ReadOnlyReason { get; set; }

        public AdmissionWorkspaceHeaderResponse? Header { get; set; }

        public AdmissionWorkspaceSourcesResponse Sources { get; set; } = new();

        public List<AdmissionWorkspaceMenuResponse> Menus { get; set; } = new();

        public AdmissionWorkspaceCompletenessResponse Completeness { get; set; } = new();

        /// <summary>Peringatan tanpa rupiah, misalnya "Dokumen admisi belum lengkap: 1 (Selisih Biaya)".</summary>
        public List<string> Warnings { get; set; } = new();
    }

    public class AdmissionWorkspaceHeaderResponse
    {
        public string? PatientName { get; set; }

        public string? Salutation { get; set; }

        public string? MedicalRecordNumber { get; set; }

        public string? GenderName { get; set; }

        public DateTime? BirthDate { get; set; }

        public string? AgeText { get; set; }

        public string EpisodeNumber { get; set; } = string.Empty;

        public DateTime? AdmittedAt { get; set; }

        public string? PatientClassName { get; set; }

        public string? ServiceUnitName { get; set; }

        public string? RoomName { get; set; }

        public string? BedName { get; set; }

        /// <summary>Pasien sudah menempati bed (bukan sekadar pemesanan).</summary>
        public bool IsOccupyingBed { get; set; }

        public string? AttendingDoctorName { get; set; }

        public string? PaymentTypeName { get; set; }

        public string? GuarantorName { get; set; }

        /// <summary>Nomor kartu penjamin; kosong tetap kosong (<c>RWI-DEC-253</c>).</summary>
        public string? CardNumber { get; set; }

        public AdmissionWorkspaceContactResponse? PrimaryEmergencyContact { get; set; }

        /// <summary>Kosong (null) bila alergi gagal dimuat; daftar kosong bila tidak ada alergi aktif.</summary>
        public List<string>? ActiveAllergyNames { get; set; }

        public bool RequiresIsolation { get; set; }

        /// <summary>Contoh "Nilai kepercayaan: 2 butir; Privasi khusus: hanya 2 kerabat; privasi transportasi: Ya".</summary>
        public string? PatientRightsSummaryText { get; set; }
    }

    public class AdmissionWorkspaceContactResponse
    {
        public string Name { get; set; } = string.Empty;

        public string? RelationshipText { get; set; }
    }

    /// <summary>Keadaan sumber: <c>Available</c>, <c>Failed</c>, atau <c>NotYetAvailable</c>.</summary>
    public class AdmissionWorkspaceSourcesResponse
    {
        public string Patient { get; set; } = "NotYetAvailable";

        public string Guarantor { get; set; } = "NotYetAvailable";

        public string Allergy { get; set; } = "NotYetAvailable";

        public string Deposit { get; set; } = "NotYetAvailable";

        public string OperatingRoom { get; set; } = "NotYetAvailable";

        public string HospitalProfile { get; set; } = "NotYetAvailable";
    }

    public class AdmissionWorkspaceMenuResponse
    {
        /// <summary>Kunci menu, misalnya <c>NewPatientHandover</c>.</summary>
        public string Key { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// <c>Completed</c>, <c>AwaitingSignature</c>, <c>Draft</c>, <c>NotCreated</c>,
        /// <c>NotRequired</c>, <c>PrintOnly</c>, <c>Printed</c>, <c>NotPrinted</c>, <c>Uncountable</c>.
        /// </summary>
        public string Badge { get; set; } = string.Empty;

        public Guid? ActiveDocumentId { get; set; }

        public bool IsRequired { get; set; }
    }

    public class AdmissionWorkspaceCompletenessResponse
    {
        public int CompletedCount { get; set; }

        public int RequiredCount { get; set; }

        public int UncountableCount { get; set; }

        public List<string> MissingNames { get; set; } = new();

        /// <summary>Nama dokumen yang kelengkapannya tidak dapat dihitung karena sumbernya gagal.</summary>
        public List<string> UncountableNames { get; set; } = new();
    }

    /// <summary>Status deposit berupiah — hanya <c>ViewAmount</c> (<c>BE-RWI-195</c>, <c>202</c>).</summary>
    public class AdmissionWorkspaceAmountsResponse
    {
        /// <summary><c>NotRequired</c>, <c>Sufficient</c>, <c>Shortfall</c>, atau <c>Unavailable</c>.</summary>
        public string DepositStatus { get; set; } = "Unavailable";

        public decimal? MinimumPolicyAmount { get; set; }

        public decimal? ReceivedAmount { get; set; }

        public decimal? ShortfallAmount { get; set; }

        public DateTime? ReadAt { get; set; }

        public AdmissionOverdueStatementResponse? OverdueStatement { get; set; }
    }

    public class AdmissionOverdueStatementResponse
    {
        public Guid DocumentId { get; set; }

        public DateTime DueAt { get; set; }

        public decimal? CurrentShortfallAmount { get; set; }
    }

    // ---------------------------------------------------------------------
    // General Consent cetak saja (BE-RWI-198)
    // ---------------------------------------------------------------------

    /// <summary>
    /// Data cetak Surat Persetujuan 12 butir dan Formulir General Consent V1. <b>Tanpa tulis apa pun</b>
    /// (<c>RWI-DEC-230</c>, <c>233</c>).
    /// </summary>
    public class GeneralConsentPrintDataResponse
    {
        public LetterheadResponse Letterhead { get; set; } = new();

        /// <summary>Kode formulir dari pengaturan; kosong dicetak tanpa kode.</summary>
        public string? FormCode { get; set; }

        /// <summary>Kota penandatanganan dari pengaturan.</summary>
        public string? SigningCity { get; set; }

        /// <summary>Tanggal cetak menurut zona waktu rumah sakit.</summary>
        public DateTime PrintDate { get; set; }

        public GeneralConsentPatientResponse Patient { get; set; } = new();

        public GeneralConsentEpisodeResponse Episode { get; set; } = new();

        /// <summary><c>General</c> (Umum) atau <c>Special</c> (Khusus ICU/Isolasi).</summary>
        public string RoomType { get; set; } = "General";

        public string? RoomTypeReason { get; set; }

        public GeneralConsentGuarantorResponse? Guarantor { get; set; }

        public List<GeneralConsentSignerCandidateResponse> SignerCandidates { get; set; } = new();

        /// <summary>Peringatan baca, misalnya data wali tidak dapat dimuat.</summary>
        public List<string> Warnings { get; set; } = new();
    }

    public class GeneralConsentPatientResponse
    {
        public string? FullName { get; set; }

        public string? Salutation { get; set; }

        public string? MedicalRecordNumber { get; set; }

        public DateTime? BirthDate { get; set; }

        public string? AgeText { get; set; }

        public string? GenderName { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }
    }

    public class GeneralConsentEpisodeResponse
    {
        public string EpisodeNumber { get; set; } = string.Empty;

        public string? EncounterNumber { get; set; }

        public DateTime? AdmittedAt { get; set; }

        public string? PatientClassName { get; set; }

        public string? ServiceUnitName { get; set; }

        public string? RoomName { get; set; }

        public string? BedName { get; set; }

        public string? AttendingDoctorName { get; set; }
    }

    public class GeneralConsentGuarantorResponse
    {
        public string? PaymentTypeName { get; set; }

        public string? GuarantorName { get; set; }

        public string? CardNumber { get; set; }

        public string? MemberNumber { get; set; }

        public string? PolicyNumber { get; set; }
    }

    public class GeneralConsentSignerCandidateResponse
    {
        /// <summary><c>Patient</c>, <c>PatientRelationship</c>, atau <c>EmergencyContact</c>.</summary>
        public string Source { get; set; } = string.Empty;

        public Guid? SourceRecordId { get; set; }

        public string Name { get; set; } = string.Empty;

        /// <summary>Jenis hubungan terstruktur (mis. <c>Spouse</c>, <c>Child</c>, <c>Mother</c>); kosong untuk kontak darurat.</summary>
        public string? RelationshipType { get; set; }

        /// <summary>Teks hubungan kontak darurat apa adanya.</summary>
        public string? RelationshipText { get; set; }

        public string? Address { get; set; }
    }

    // ---------------------------------------------------------------------
    // Gelang dan label (BE-RWI-196)
    // ---------------------------------------------------------------------

    public class IdentityLabelResponse
    {
        public IdentityWristbandResponse Wristband { get; set; } = new();

        public IdentityPatientLabelResponse PatientLabel { get; set; } = new();

        public IdentityPrintCountsResponse PrintCounts { get; set; } = new();
    }

    public class IdentityWristbandResponse
    {
        /// <summary><c>Adult</c> atau <c>Infant</c>.</summary>
        public string Kind { get; set; } = "Adult";

        /// <summary>Jenis cetakan yang wajib dicatat (<c>AdultWristband</c> = 1 atau <c>InfantWristband</c> = 2).</summary>
        public int PrintKind { get; set; }

        /// <summary>Contoh "BUDI SANTOSO, Tn." atau "BY. NY. RINA SANTOSO".</summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>Contoh "12 Mar 1981".</summary>
        public string? BirthDateText { get; set; }

        /// <summary>Contoh "45 th".</summary>
        public string? AgeText { get; set; }

        public string MedicalRecordNumber { get; set; } = string.Empty;

        /// <summary>Isi QR = No. RM terformat saja (<c>RWI-DEC-259</c>).</summary>
        public string? QrPayload { get; set; }

        /// <summary>2 untuk Gelang Bayi, 0 untuk dewasa.</summary>
        public int SmallLabelCount { get; set; }
    }

    public class IdentityPatientLabelResponse
    {
        /// <summary>Kode rumah sakit dari pengaturan; kosong → kode situs.</summary>
        public string? HospitalCode { get; set; }

        /// <summary>Contoh "BUDI SANTOSO / RSX".</summary>
        public string NameLine { get; set; } = string.Empty;

        /// <summary>Contoh "12/03/81".</summary>
        public string? BirthDateShort { get; set; }

        /// <summary>Contoh "L / 45 th".</summary>
        public string? GenderAgeText { get; set; }

        public string MedicalRecordNumber { get; set; } = string.Empty;

        /// <summary>Hanya <c>CardNumberSnapshot</c>; kosong tetap kosong (<c>RWI-DEC-253</c>).</summary>
        public string? CardNumber { get; set; }

        public string? QrPayload { get; set; }
    }

    public class IdentityPrintCountsResponse
    {
        public int AdultWristband { get; set; }

        public int InfantWristband { get; set; }

        public int PatientLabel { get; set; }
    }

    // ---------------------------------------------------------------------
    // Data Dasar Rawat Inap / IPD (BE-RWI-197)
    // ---------------------------------------------------------------------

    /// <summary>
    /// Lembar IPD "DATA DASAR RAWAT INAP/ODC" — PRD Lampiran A.6, <c>RWI-DEC-244</c>, <c>254</c>.
    /// Isian tanpa sumber dikosongkan dan tercantum di <see cref="BlankFields"/> untuk dicetak garis
    /// kosong; tidak ada isian ketik.
    /// </summary>
    public class InpatientBaseDataResponse
    {
        public Guid EpisodeId { get; set; }

        public LetterheadResponse Letterhead { get; set; } = new();

        public string? FormCode { get; set; }

        public bool CanPrint { get; set; }

        public string? CannotPrintReason { get; set; }

        public InpatientBaseDataLeftResponse Left { get; set; } = new();

        public InpatientBaseDataRightResponse Right { get; set; } = new();

        public InpatientBaseDataFooterResponse Footer { get; set; } = new();

        /// <summary>Kunci isian yang dicetak garis kosong, misalnya <c>Occupation</c>, <c>Cashier</c>.</summary>
        public List<string> BlankFields { get; set; } = new();

        /// <summary>"lihat kasir" sampai rupiah dibaca dari <c>/base-data/amounts</c>.</summary>
        public string RoomRateDisplay { get; set; } = "lihat kasir";

        public List<string> Warnings { get; set; } = new();
    }

    public class InpatientBaseDataLeftResponse
    {
        public string? PatientName { get; set; }

        public string? MedicalRecordNumber { get; set; }

        public string? OwnName { get; set; }

        public string? FullName { get; set; }

        /// <summary>Contoh "KTP 3275010101010001".</summary>
        public string? IdentityText { get; set; }

        public DateTime? BirthDate { get; set; }

        public string? KtpAddress { get; set; }

        public string? KtpRtRw { get; set; }

        public string? KtpVillage { get; set; }

        public string? KtpDistrict { get; set; }

        public string? KtpCity { get; set; }

        public string? KtpPostalCode { get; set; }

        public string? DomicileAddress { get; set; }

        public string? Email { get; set; }

        public string? Occupation { get; set; }

        public string? OfficeAddress { get; set; }

        public string? OfficePhone { get; set; }

        public string? Nationality { get; set; }

        public string? Religion { get; set; }

        public string? MutationNumber { get; set; }

        public DateTime? AdmittedAt { get; set; }

        /// <summary>Contoh "Melati 03 / Bed B".</summary>
        public string? RoomText { get; set; }

        public string? PatientClassName { get; set; }

        /// <summary>Dokter penerbit surat pengantar; tanpa surat → dokter perujuk luar (sesudah <c>BE-RWI-190</c>) atau kosong.</summary>
        public string? ReferringDoctor { get; set; }

        public string? AttendingDoctor { get; set; }

        /// <summary>Penerima informasi; kosong selama General Consent <i>fail-closed</i>.</summary>
        public string? InformationRecipient { get; set; }
    }

    public class InpatientBaseDataRightResponse
    {
        public string? GuarantorName { get; set; }

        /// <summary>Perorangan, Perusahaan, atau Asuransi.</summary>
        public string? PaymentCategory { get; set; }

        public string? CompanyOrInsurerName { get; set; }

        public string? MemberNumber { get; set; }

        public string? PolicyNumber { get; set; }

        public InpatientBaseDataResponsiblePersonResponse? ResponsiblePerson { get; set; }

        /// <summary>Diagnosis masuk dan rencana dari surat pengantar terbit; tanpa surat → kosong.</summary>
        public string? AdmissionDiagnosisAndPlan { get; set; }

        public string? PlannedClassName { get; set; }

        public string? PaymentMethod { get; set; }

        public string? DirectorApproval { get; set; }

        public string? Cashier { get; set; }

        /// <summary>Petugas yang mengonfirmasi admisi.</summary>
        public string? AdmissionOfficer { get; set; }

        /// <summary>Perawat penanggung jawab aktif.</summary>
        public string? FloorNurse { get; set; }

        public List<InpatientBaseDataTransferResponse> RoomTransfers { get; set; } = new();

        public string? SpecialAttention { get; set; }

        /// <summary>Butir Nilai Kepercayaan <c>Completed</c>; kosong bila belum lengkap.</summary>
        public List<string> BeliefValues { get; set; } = new();

        /// <summary>Ringkasan Permintaan Privasi <c>Completed</c>; kosong bila belum lengkap.</summary>
        public string? PrivacyText { get; set; }
    }

    public class InpatientBaseDataResponsiblePersonResponse
    {
        public string Name { get; set; } = string.Empty;

        public string? RelationshipText { get; set; }

        public string? IdentityText { get; set; }

        public string? Address { get; set; }

        public string? DomicileAddress { get; set; }

        public string? PhoneNumber { get; set; }
    }

    public class InpatientBaseDataTransferResponse
    {
        public DateTime MovedAt { get; set; }

        public string? ServiceUnitName { get; set; }

        public string? PatientClassName { get; set; }

        public string? RoomName { get; set; }

        public string? BedName { get; set; }
    }

    public class InpatientBaseDataFooterResponse
    {
        public string? HospitalCode { get; set; }

        /// <summary>No. surat = nomor episode.</summary>
        public string LetterNumber { get; set; } = string.Empty;

        public DateTime PrintDate { get; set; }
    }

    /// <summary>"Rencana @ Kamar (Rp)" — hanya <c>ViewAmount</c> (<c>BE-RWI-197</c>; angka sesudah <c>BE-RWI-191</c>).</summary>
    public class InpatientBaseDataAmountsResponse
    {
        public decimal? DailyRoomRate { get; set; }

        /// <summary><c>Available</c>, <c>NotYetAvailable</c> (<c>RWI-OQ-129</c>), atau <c>TariffMissing</c>.</summary>
        public string RoomRateState { get; set; } = "NotYetAvailable";
    }

    // ---------------------------------------------------------------------
    // Ringkasan hak pasien (BE-RWI-200)
    // ---------------------------------------------------------------------

    /// <summary>Ringkasan Nilai Kepercayaan dan Permintaan Privasi <c>Completed</c> untuk modul lain.</summary>
    public class PatientRightsSummaryResponse
    {
        public PatientRightsBeliefResponse? BeliefValues { get; set; }

        public PatientRightsPrivacyResponse? Privacy { get; set; }

        public string? SummaryText { get; set; }
    }

    public class PatientRightsBeliefResponse
    {
        public Guid DocumentId { get; set; }

        public DateTime? CompletedAt { get; set; }

        public List<string> Items { get; set; } = new();
    }

    public class PatientRightsPrivacyResponse
    {
        public Guid DocumentId { get; set; }

        public List<string> AllowedVisitorNames { get; set; } = new();

        public List<string> SpecialRequests { get; set; } = new();

        public bool IsTransportPrivacyRequested { get; set; }
    }

    // ---------------------------------------------------------------------
    // Log cetak (BE-RWI-193)
    // ---------------------------------------------------------------------

    public class RecordPrintRequest
    {
        public InpAdmissionPrintKind? PrintKind { get; set; }

        /// <summary>Wajib untuk cetak dokumen admisi; harus milik episode.</summary>
        public Guid? DocumentId { get; set; }

        /// <summary>1–10, bawaan 1.</summary>
        public int? Copies { get; set; }

        public InpReprintReason? ReprintReason { get; set; }

        /// <summary>Wajib bila alasan <c>Other</c>, 1–200 karakter.</summary>
        public string? ReprintNote { get; set; }
    }

    public class PrintLogResponse
    {
        public Guid Id { get; set; }

        public int PrintKind { get; set; }

        public string PrintKindName { get; set; } = string.Empty;

        public Guid? DocumentId { get; set; }

        public int? DocumentStatusAtPrint { get; set; }

        public int Copies { get; set; }

        public bool IsReprint { get; set; }

        /// <summary>"Cetakan ke-n" untuk kunci cetakan yang sama, dihitung dari urutan baris.</summary>
        public int PrintSequence { get; set; }

        public int? ReprintReason { get; set; }

        public string? ReprintReasonName { get; set; }

        public string? ReprintNote { get; set; }

        public string? PrintedByName { get; set; }

        public DateTime PrintedAt { get; set; }
    }
}
