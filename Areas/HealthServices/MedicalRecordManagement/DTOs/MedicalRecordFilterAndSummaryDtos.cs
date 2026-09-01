namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs
{
    // =========================================================================
    // Bentuk bersama
    //
    // Mengikuti pola metadata dan ringkasan yang sudah dipakai grup Master Data,
    // supaya layar rekam medis dapat memakai komponen penyaring yang sama tanpa
    // penyesuaian khusus.
    // =========================================================================

    /// <summary>Satu pilihan pengurutan pada layar daftar.</summary>
    public class MedicalRecordSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    /// <summary>
    /// Satu pilihan yang berasal dari enum.
    ///
    /// <see cref="Value"/> adalah angka yang dikirim balik ke API, <see cref="Name"/> nama
    /// teknisnya, dan <see cref="Label"/> teks siap tampil dalam Bahasa Indonesia.
    /// </summary>
    public class MedicalRecordEnumOptionResponse
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    /// <summary>Keterangan satu parameter query, supaya layar tahu apa yang boleh dikirim.</summary>
    public class MedicalRecordQueryParameterInfoResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Required { get; set; } = "No";
        public string Description { get; set; } = string.Empty;
        public string? Example { get; set; }
    }

    // =========================================================================
    // Clinical Document Integrity
    // =========================================================================

    public class ClinicalDocumentIntegrityFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";

        /// <summary>
        /// Tiga belas jenis dokumen beserta penanda mana yang sudah tunduk aturan keutuhan.
        ///
        /// Penanda itu WAJIB ditampilkan layar. Rilis pertama hanya menegakkan CPPT
        /// (`RM-DEC-019`); menampilkan jenis lain seolah-olah sudah terlindungi akan
        /// menyesatkan pembacanya.
        /// </summary>
        public List<MedicalRecordDocumentKindOptionResponse> DocumentKinds { get; set; } = new();

        public List<MedicalRecordEnumOptionResponse> IntegrityStatuses { get; set; } = new();

        public List<MedicalRecordEnumOptionResponse> LockTriggers { get; set; } = new();

        public List<MedicalRecordSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<MedicalRecordQueryParameterInfoResponse> QueryParameters { get; set; } = new();
    }

    public class ClinicalDocumentIntegritySummaryResponse
    {
        public int TotalDocument { get; set; }

        public int DraftDocument { get; set; }
        public int SignedDocument { get; set; }
        public int LockedUnsignedDocument { get; set; }
        public int CancelledDocument { get; set; }

        /// <summary>
        /// Dokumen yang penulisnya tidak diketahui, umumnya hasil pengisian data lama.
        /// </summary>
        public int UnknownAuthorDocument { get; set; }

        public int TotalAddendum { get; set; }

        /// <summary>Jumlah jenis dokumen yang sudah tunduk aturan keutuhan. Rilis pertama: satu.</summary>
        public int EnforcedDocumentKind { get; set; }

        /// <summary>Jumlah jenis dokumen yang belum tunduk. Rilis pertama: dua belas.</summary>
        public int NotEnforcedDocumentKind { get; set; }
    }

    // =========================================================================
    // Clinical Note Addendum
    // =========================================================================

    public class ClinicalNoteAddendumFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";

        public List<MedicalRecordDocumentKindOptionResponse> DocumentKinds { get; set; } = new();

        public List<MedicalRecordSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<MedicalRecordQueryParameterInfoResponse> QueryParameters { get; set; } = new();

        /// <summary>
        /// Addendum tidak dapat diubah maupun dihapus setelah dibuat.
        ///
        /// Dinyatakan di metadata supaya layar tidak menyediakan tombol yang tidak akan pernah
        /// bekerja.
        /// </summary>
        public bool IsEditable { get; set; } = false;

        public bool IsDeletable { get; set; } = false;
    }

    public class ClinicalNoteAddendumSummaryResponse
    {
        public int TotalAddendum { get; set; }

        /// <summary>Addendum yang dibuat penulis aslinya sendiri.</summary>
        public int ByOriginalAuthor { get; set; }

        /// <summary>Addendum yang dibuat pengganti karena penulisnya berhalangan.</summary>
        public int BySubstituteAuthor { get; set; }

        /// <summary>Dokumen yang memiliki sekurang-kurangnya satu addendum.</summary>
        public int DocumentWithAddendum { get; set; }
    }

    // =========================================================================
    // Clinical Note Author Delegation
    // =========================================================================

    public class ClinicalNoteAuthorDelegationFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";

        public List<MedicalRecordEnumOptionResponse> Triggers { get; set; } = new();

        public List<MedicalRecordSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<MedicalRecordQueryParameterInfoResponse> QueryParameters { get; set; } = new();

        /// <summary>
        /// Penetapan wajib berbatas waktu.
        ///
        /// Dinyatakan di metadata karena inilah aturan yang paling mudah dilanggar layar:
        /// penetapan tanpa batas waktu adalah pintu belakang permanen (`RM-DEC-020`).
        /// </summary>
        public bool IsValidUntilRequired { get; set; } = true;

        public bool IsGrantReasonRequired { get; set; } = true;
    }

    public class ClinicalNoteAuthorDelegationSummaryResponse
    {
        public int TotalDelegation { get; set; }

        /// <summary>Penetapan yang masih membuka jalur koreksi hari ini.</summary>
        public int ActiveDelegation { get; set; }

        /// <summary>Penetapan yang batas waktunya sudah lewat.</summary>
        public int ExpiredDelegation { get; set; }

        /// <summary>Penetapan yang dicabut sebelum batas waktunya.</summary>
        public int RevokedDelegation { get; set; }

        public int ByUnitHeadGrant { get; set; }
        public int ByInactiveAccount { get; set; }
    }

    // =========================================================================
    // Medical Record Access Log
    // =========================================================================

    public class MedicalRecordAccessLogFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";

        public List<MedicalRecordEnumOptionResponse> AccessTypes { get; set; } = new();
        public List<MedicalRecordEnumOptionResponse> AccessScopes { get; set; } = new();
        public List<MedicalRecordSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<MedicalRecordQueryParameterInfoResponse> QueryParameters { get; set; } = new();

        /// <summary>
        /// Jejak tidak dapat diubah maupun dihapus. Satu-satunya perubahan yang diizinkan adalah
        /// menandainya sudah ditinjau.
        ///
        /// Dinyatakan di metadata supaya layar tidak menyediakan tombol ubah maupun hapus —
        /// jejak yang dapat diubah bukan jejak.
        /// </summary>
        public bool IsEditable { get; set; } = false;

        public bool IsDeletable { get; set; } = false;

        /// <summary>
        /// PERINGATAN PRIVASI. Daftar ini memuat alasan akses yang dapat mengungkap keadaan
        /// pasien, misalnya "konsultasi kejiwaan". Hak aksesnya tidak boleh diberikan seluas
        /// hak baca rekam medis.
        /// </summary>
        public bool ContainsSensitiveReason { get; set; } = true;
    }

    // =========================================================================
    // Medical Record Backfill
    // =========================================================================

    public class MedicalRecordBackfillFilterMetadataResponse
    {
        public List<int> BatchSizeOptions { get; set; } = new();

        public int BatchSizeDefault { get; set; }

        public int BatchSizeMax { get; set; }

        public List<MedicalRecordQueryParameterInfoResponse> QueryParameters { get; set; } = new();

        /// <summary>
        /// Penjalanan bawaan bersifat percobaan, tidak menyimpan apa pun.
        ///
        /// Dinyatakan di metadata supaya layar menampilkan pilihan itu apa adanya, dan pengguna
        /// tahu bahwa menyimpan sungguhan menuntut tindakan sadar.
        /// </summary>
        public bool IsDryRunDefault { get; set; } = true;

        /// <summary>
        /// Ringkasan keadaan catatan lama disediakan endpoint `survey`, bukan `summary`.
        ///
        /// Dinyatakan di sini supaya layar tidak mencari endpoint yang tidak ada.
        /// </summary>
        public string SummaryEndpoint { get; set; } = "survey";
    }
}
