using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs
{
    /// <summary>
    /// Keterangan tentang pembukaan berkas yang baru saja terjadi.
    ///
    /// Ikut dikembalikan pada SETIAP balasan endpoint rekam medis, dan itu disengaja. Pengguna
    /// berhak tahu bahwa pembukaannya tercatat, dan bila aksesnya ditandai untuk ditinjau, ia
    /// berhak tahu itu sekarang — bukan baru mengetahuinya saat ditanya unit rekam medis.
    /// </summary>
    public class MedicalRecordAccessInfoResponse
    {
        public Guid? AccessLogId { get; set; }

        public MedicalRecordAccessType AccessType { get; set; }

        public string AccessTypeName { get; set; } = string.Empty;

        /// <summary>Pasien sedang punya kunjungan yang belum ditutup.</summary>
        public bool HasActiveEncounter { get; set; }

        /// <summary>Pembukaan ini akan ditelaah unit rekam medis.</summary>
        public bool IsFlaggedForReview { get; set; }
    }

    /// <summary>Identitas pasien secukupnya untuk kepala berkas rekam medis.</summary>
    public class MedicalRecordPatientIdentityResponse
    {
        public Guid PatientId { get; set; }

        public string MedicalRecordNumber { get; set; } = string.Empty;

        public string PatientCode { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public DateTime? BirthDate { get; set; }

        public int? AgeYear { get; set; }

        public string? GenderName { get; set; }
    }

    /// <summary>Satu alergi aktif. Ditampilkan menonjol karena menyangkut keselamatan pasien.</summary>
    public class MedicalRecordAllergyBriefResponse
    {
        public Guid DocumentId { get; set; }

        public string AllergenName { get; set; } = string.Empty;

        public string? AllergenGroupName { get; set; }

        public string? ReactionType { get; set; }

        public string SeverityName { get; set; } = string.Empty;

        public bool IsLifeThreatening { get; set; }

        public bool IsHighRisk { get; set; }

        public DateTime ReportedDateTime { get; set; }
    }

    /// <summary>Satu diagnosis aktif.</summary>
    public class MedicalRecordDiagnosisBriefResponse
    {
        public Guid DocumentId { get; set; }

        public string DiagnosisCode { get; set; } = string.Empty;

        public string DiagnosisName { get; set; } = string.Empty;

        public bool IsPrimary { get; set; }

        public bool IsChronic { get; set; }

        public DateTime DiagnosisDateTime { get; set; }
    }

    /// <summary>Jumlah dokumen pasien pada satu jenis.</summary>
    public class MedicalRecordDocumentCountResponse
    {
        public ClinicalDocumentKind DocumentKind { get; set; }

        public string DocumentKindName { get; set; } = string.Empty;

        public int Total { get; set; }

        /// <summary>
        /// Jenis ini sudah tunduk aturan keutuhan pada rilis sekarang.
        ///
        /// Bernilai `false` untuk dua belas jenis dari tiga belas (RM-DEC-019). Layar WAJIB
        /// menyatakannya terbuka sesuai RM-FE-009.
        /// </summary>
        public bool IsIntegrityEnforced { get; set; }
    }

    /// <summary>
    /// Ringkasan berkas rekam medis seorang pasien.
    ///
    /// Isinya sengaja dibatasi pada yang perlu dilihat lebih dulu: identitas, alergi aktif,
    /// diagnosis aktif, dan jumlah dokumen per jenis. Isi dokumen tidak ada di sini.
    /// </summary>
    public class MedicalRecordSummaryResponse
    {
        public MedicalRecordPatientIdentityResponse Patient { get; set; } = new();

        public MedicalRecordAccessInfoResponse Access { get; set; } = new();

        public List<MedicalRecordAllergyBriefResponse> ActiveAllergies { get; set; } = new();

        public List<MedicalRecordDiagnosisBriefResponse> ActiveDiagnoses { get; set; } = new();

        public List<MedicalRecordDocumentCountResponse> DocumentCounts { get; set; } = new();

        public int TotalDocument { get; set; }

        /// <summary>Sumber yang gagal dihitung. Kosong berarti seluruh angka di atas lengkap.</summary>
        public List<MedicalRecordTimelineSourceFailure> FailedSources { get; set; } = new();

        public bool IsComplete => FailedSources.Count == 0;
    }

    /// <summary>
    /// Balasan endpoint riwayat.
    ///
    /// Memuat halaman **beserta** keterangan kelengkapannya. Lihat catatan delta kontrak pada
    /// `contracts/api-contract.md` bagian 2: bentuk `PagedResult` saja tidak punya tempat untuk
    /// menyatakan sumber yang gagal dibaca, padahal acceptance criteria `BE-13` nomor 4
    /// mewajibkan kekurangan dinyatakan.
    /// </summary>
    public class MedicalRecordTimelineResponse
    {
        public PagedResult<MedicalRecordTimelineItemResponse> Page { get; set; } = new();

        public MedicalRecordAccessInfoResponse Access { get; set; } = new();

        public List<ClinicalDocumentKind> RequestedKinds { get; set; } = new();

        public List<MedicalRecordTimelineSourceFailure> FailedSources { get; set; } = new();

        public bool IsTruncated { get; set; }

        public bool IsComplete { get; set; }
    }

    /// <summary>Satu bagian isi dokumen, berbentuk label dan nilai supaya seragam untuk 13 jenis.</summary>
    public class MedicalRecordDocumentSectionResponse
    {
        public string Label { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;
    }

    /// <summary>
    /// Detail satu dokumen klinis beserta addendumnya.
    ///
    /// PENTING — `PrivateNote` TIDAK PERNAH ada di sini, walaupun dokumennya memilikinya.
    /// Kolom itu hanya dapat dibuka lewat endpoint tersendiri dengan izin terpisah
    /// (`BE-15`, RM-DEC-022). Yang ada di sini hanya penanda <see cref="HasPrivateNote"/>,
    /// supaya pembaca tahu catatan itu ada tanpa ikut membacanya.
    /// </summary>
    public class MedicalRecordDocumentDetailResponse
    {
        public ClinicalDocumentKind DocumentKind { get; set; }

        public string DocumentKindName { get; set; } = string.Empty;

        public Guid DocumentId { get; set; }

        public Guid PatientId { get; set; }

        public Guid? EncounterId { get; set; }

        public string? DocumentNumber { get; set; }

        public string? Title { get; set; }

        public DateTime OccurredAt { get; set; }

        public bool IsCancelled { get; set; }

        public MedicalRecordAccessInfoResponse Access { get; set; } = new();

        // ---- Keutuhan dokumen ----

        public bool IsIntegrityEnforced { get; set; }

        public ClinicalDocumentIntegrityStatus? IntegrityStatus { get; set; }

        public string? IntegrityStatusName { get; set; }

        public DateTime? SignedAt { get; set; }

        public Guid? AuthorUserId { get; set; }

        public string? AuthorName { get; set; }

        public int AddendumCount { get; set; }

        // ---- Isi ----

        public List<MedicalRecordDocumentSectionResponse> Sections { get; set; } = new();

        public List<ClinicalNoteAddendumResponse> Addendums { get; set; } = new();

        /// <summary>
        /// Dokumen ini memiliki catatan pribadi yang tidak ditampilkan di sini.
        ///
        /// Penanda, bukan isinya. Membukanya menuntut izin `MedicalRecord : ReadPrivateNote`
        /// dan selalu menuntut alasan (`BE-15`).
        /// </summary>
        public bool HasPrivateNote { get; set; }
    }

    /// <summary>
    /// Isi catatan pribadi klinisi pada sebuah dokumen klinis.
    ///
    /// **Inilah satu-satunya bentuk balasan pada modul ini yang benar-benar memuat isi
    /// `PrivateNote`.** Endpoint yang mengembalikannya memakai izin tersendiri
    /// `MedicalRecord : ReadPrivateNote`, terpisah dari izin baca rekam medis biasa, dan
    /// **selalu** menuntut keperluan akses — bahkan untuk pasien yang sedang dirawat pengguna
    /// (`RM-DEC-022`).
    ///
    /// Alasannya: kolom ini ditulis dengan harapan bersifat pribadi, sehingga membukanya selalu
    /// merupakan tindakan yang perlu dipertanggungjawabkan, bukan bagian pekerjaan sehari-hari.
    /// </summary>
    public class MedicalRecordPrivateNoteResponse
    {
        public ClinicalDocumentKind DocumentKind { get; set; }

        public string DocumentKindName { get; set; } = string.Empty;

        public Guid DocumentId { get; set; }

        public Guid PatientId { get; set; }

        public Guid? EncounterId { get; set; }

        public string? DocumentNumber { get; set; }

        public DateTime OccurredAt { get; set; }

        /// <summary>
        /// Penulis catatan. Pembaca berhak tahu catatan pribadi siapa yang sedang ia buka.
        /// </summary>
        public Guid? AuthorUserId { get; set; }

        public string? AuthorName { get; set; }

        /// <summary>
        /// Isi catatan pribadi. Bernilai kosong bila dokumennya memang tidak memuat catatan
        /// pribadi — bukan karena disembunyikan.
        /// </summary>
        public string? PrivateNote { get; set; }

        /// <summary>
        /// Membedakan "catatannya memang tidak ada" dari "catatannya ada tetapi kosong".
        /// Tanpa penanda ini, pembaca tidak dapat tahu mana yang benar.
        /// </summary>
        public bool HasPrivateNote { get; set; }

        public MedicalRecordAccessInfoResponse Access { get; set; } = new();
    }

    /// <summary>Satu pilihan jenis dokumen pada penyaring riwayat.</summary>
    public class MedicalRecordDocumentKindOptionResponse
    {
        public ClinicalDocumentKind Value { get; set; }

        public string Name { get; set; } = string.Empty;

        public bool IsIntegrityEnforced { get; set; }
    }

    /// <summary>Satu pilihan keperluan akses.</summary>
    public class MedicalRecordAccessPurposeOptionResponse
    {
        public Guid Id { get; set; }

        public string PurposeCode { get; set; } = string.Empty;

        public string PurposeName { get; set; } = string.Empty;

        /// <summary>Keperluan ini menuntut penjelasan tambahan berupa teks bebas.</summary>
        public bool IsFreeTextRequired { get; set; }

        /// <summary>Pemakaian keperluan ini akan ditandai untuk ditelaah unit rekam medis.</summary>
        public bool RequiresReview { get; set; }

        public string? Description { get; set; }
    }

    /// <summary>
    /// Daftar pilihan yang dibutuhkan layar rekam medis sebelum pengguna dapat menyaring atau
    /// membuka berkas.
    ///
    /// Endpoint ini sengaja tidak menyentuh data pasien mana pun, sehingga tidak menghasilkan
    /// jejak akses. Ia hanya berisi daftar pilihan.
    /// </summary>
    public class MedicalRecordFilterMetadataResponse
    {
        public List<MedicalRecordDocumentKindOptionResponse> DocumentKinds { get; set; } = new();

        public List<MedicalRecordAccessPurposeOptionResponse> AccessPurposes { get; set; } = new();

        public int PageSizeDefault { get; set; }

        public int PageSizeMax { get; set; }

        /// <summary>
        /// Peringatan yang WAJIB ditampilkan bila daftar keperluan akses kosong.
        ///
        /// Tanpa satu pun keperluan terdaftar, pembukaan rekam medis pasien di luar rawatan akan
        /// selalu ditolak — bukan karena kesalahan pengguna, melainkan karena master-nya belum
        /// diisi (`BE-09`, menunggu SOP rekam medis).
        /// </summary>
        public bool IsAccessPurposeMasterEmpty { get; set; }
    }
}
