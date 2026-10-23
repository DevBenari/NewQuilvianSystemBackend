namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs
{
    /// <summary>
    /// Mode pemilihan pasien worker migrasi massal RSMMC (`BE-PAT-MIG-002`, `PAT-DEC-045`).
    /// </summary>
    public static class RsmmcBulkSelectionMode
    {
        /// <summary>Satu-satunya mode build pertama: manifest eksekusi <c>RSMMC-CANARY-A</c> v1.</summary>
        public const string CanaryA = "CANARY_A";
    }

    /// <summary>Status satu pasien manifest — kontrak `1.3.0` B3.2.</summary>
    public static class RsmmcBulkPatientMigrationStatus
    {
        /// <summary>Simulasi: pasien akan dibuat bila dijalankan sungguhan. Menghabiskan <c>limit</c>.</summary>
        public const string Ready = "READY";

        /// <summary>Eksekusi: <c>MstPatient</c>, crosswalk, dan QR baru sudah ter-<i>commit</i>.</summary>
        public const string Migrated = "MIGRATED";

        /// <summary>Crosswalk sudah menunjuk pasien yang cocok. Tidak menghabiskan <c>limit</c>.</summary>
        public const string AlreadyMigrated = "ALREADY_MIGRATED";

        /// <summary>Sudah dimigrasi, tetapi <c>QrCodePath</c> atau berkas QR tidak sesuai. Dilaporkan, tidak diperbaiki.</summary>
        public const string MigratedQrInconsistent = "MIGRATED_QR_INCONSISTENT";

        public const string Conflict = "CONFLICT";

        public const string Failed = "FAILED";

        /// <summary>Tidak diproses karena eksekusi berhenti, <c>limit</c> tercapai, atau dibatalkan pemanggil.</summary>
        public const string NotProcessed = "NOT_PROCESSED";
    }

    /// <summary>Jenis konflik — kontrak `1.3.0` B3.2.</summary>
    public static class RsmmcBulkPatientConflictType
    {
        public const string CrosswalkExistsWithoutTarget = "CROSSWALK_EXISTS_WITHOUT_TARGET";

        public const string OrphanedCrosswalk = "ORPHANED_CROSSWALK";

        public const string TargetMappingMismatch = "TARGET_MAPPING_MISMATCH";

        public const string DuplicateTargetMapping = "DUPLICATE_TARGET_MAPPING";

        public const string PreviewStateChanged = "PREVIEW_STATE_CHANGED";

        public const string PlannerMismatch = "PLANNER_MISMATCH";

        public const string PatientCodeAlreadyUsed = "PATIENT_CODE_ALREADY_USED";

        public const string MedicalRecordNumberAlreadyUsed = "MRN_ALREADY_USED";

        public const string IdentityNumberAlreadyUsed = "IDENTITY_NUMBER_ALREADY_USED";

        public const string PhoneAlreadyUsed = "PHONE_ALREADY_USED";

        public const string QrArtifactAlreadyExists = "QR_ARTIFACT_ALREADY_EXISTS";

        public const string PreviewValueInvalid = "PREVIEW_VALUE_INVALID";

        public const string DeferredLegacyPid = "DEFERRED_LEGACY_PID";

        public const string ConcurrentRun = "CONCURRENT_RUN";
    }

    /// <summary>Tahap kegagalan per pasien — kontrak `1.3.0` B3.2, B4.3, B4.5.</summary>
    public static class RsmmcBulkPatientFailureStage
    {
        /// <summary><c>legacy_pid</c> di luar Int32, sehingga kunci dua bagian tidak dapat dibentuk.</summary>
        public const string LockKeyOutOfRange = "LOCK_KEY_OUT_OF_RANGE";

        public const string BeginTransaction = "BEGIN_TRANSACTION";

        public const string AcquireLock = "ACQUIRE_LOCK";

        public const string RereadPreview = "REREAD_PREVIEW";

        public const string RereadPlanner = "REREAD_PLANNER";

        public const string RereadCrosswalk = "REREAD_CROSSWALK";

        public const string CheckOwnership = "CHECK_OWNERSHIP";

        public const string InsertPatient = "INSERT_PATIENT";

        public const string InsertCrosswalk = "INSERT_CROSSWALK";

        public const string VerifyPersisted = "VERIFY_PERSISTED";

        public const string CreateQr = "CREATE_QR";

        public const string Commit = "COMMIT";

        /// <summary>
        /// <c>CommitAsync</c> melempar galat. Hasil commit tidak dapat dipastikan: QR baru tidak dihapus,
        /// proses berhenti, dan tidak dicoba ulang otomatis (B4.5 kasus J).
        /// </summary>
        public const string CommitOutcomeUncertain = "COMMIT_OUTCOME_UNCERTAIN";
    }

    /// <summary>Keadaan crosswalk saat pasien dibaca — kontrak B2.2 <c>crosswalkStateBefore</c>.</summary>
    public static class RsmmcBulkCrosswalkState
    {
        public const string Missing = "MISSING";

        public const string ExistsWithoutTarget = "EXISTS_WITHOUT_TARGET";

        public const string ExistsWithTarget = "EXISTS_WITH_TARGET";
    }

    /// <summary>
    /// Masukan <c>POST …/admin/migration/rsmmc-bulk-patients/migrate</c> — kontrak `1.3.0` B2.1.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Daftar <c>legacy_pid</c> sengaja tidak ada: server memakai manifest eksekusi yang disimpannya
    /// sendiri (`PAT-DEC-044`).
    /// </para>
    /// <para>
    /// Atribut validasi seperti <c>[Required]</c> dan <c>[Range]</c> sengaja tidak dipasang. Repository
    /// ini tidak punya pembungkus respons validasi, sehingga atribut itu membuat <c>[ApiController]</c>
    /// menjawab <c>400</c> bawaan sebelum action berjalan, dan kode galat kontrak B2.3
    /// (<c>RSMMC_BULK_BATCH_ID_REQUIRED</c>, <c>RSMMC_BULK_SELECTION_MODE_NOT_SUPPORTED</c>,
    /// <c>RSMMC_BULK_LIMIT_OUT_OF_RANGE</c>) tidak pernah sampai ke pengguna. Seluruh aturannya
    /// divalidasi <c>RsmmcBulkPatientMigrationService</c>.
    /// </para>
    /// </remarks>
    public class RsmmcBulkPatientMigrationRequest
    {
        /// <summary>Wajib. Eksekusi hanya untuk batch kanonik <c>45eeefba-dd39-6831-1bd2-d986851f7c1f</c>.</summary>
        public Guid? BatchId { get; set; }

        /// <summary>Wajib. Build pertama hanya <c>CANARY_A</c>.</summary>
        public string? SelectionMode { get; set; }

        /// <summary>Bila tidak dikirim, dianggap <c>true</c>. Tidak pernah ada default implisit <c>false</c>.</summary>
        public bool? DryRun { get; set; }

        /// <summary>Bawaan 25, rentang 1 sampai 25. Hanya pasien yang akan dibuat yang menghabiskan limit.</summary>
        public int? Limit { get; set; }
    }

    /// <summary>Keluaran — kontrak `1.3.0` B2.2.</summary>
    public class RsmmcBulkPatientMigrationResponse
    {
        public Guid BatchId { get; set; }

        public string SelectionMode { get; set; } = string.Empty;

        public bool DryRun { get; set; }

        public int Limit { get; set; }

        public string EnvironmentName { get; set; } = string.Empty;

        public string ManifestId { get; set; } = string.Empty;

        public int ManifestVersion { get; set; }

        /// <summary>SHA-256 berurutan: <c>legacy_pid</c> dalam urutan manifest, dipisah koma, tanpa spasi, UTF-8.</summary>
        public string ManifestSha256 { get; set; } = string.Empty;

        public int ManifestSize { get; set; }

        public int TotalInManifest { get; set; }

        /// <summary>
        /// Pasien manifest yang sudah dimigrasi saat panggilan dimulai: baris berstatus <c>ALREADY_MIGRATED</c>
        /// maupun <c>MIGRATED_QR_INCONSISTENT</c> — keduanya punya pemetaan dan pasien target yang cocok (B3.2).
        /// </summary>
        public int AlreadyMigrated { get; set; }

        /// <summary>Pasien manifest yang belum dimigrasi saat panggilan dimulai: <c>totalInManifest − alreadyMigrated</c>.</summary>
        public int EligibleBeforeRun { get; set; }

        /// <summary>Jumlah pasien yang paling banyak dibuat panggilan ini: <c>min(limit, eligibleBeforeRun)</c>.</summary>
        public int Selected { get; set; }

        public int Ready { get; set; }

        public int Migrated { get; set; }

        public int Conflict { get; set; }

        public int Failed { get; set; }

        public int NotProcessed { get; set; }

        /// <summary>Pasien manifest yang belum dimigrasi sesudah panggilan ini.</summary>
        public int Remaining { get; set; }

        public bool Stopped { get; set; }

        public string? StopReason { get; set; }

        public List<RsmmcBulkGateCheckResponse> Gates { get; set; } = [];

        /// <summary>Satu baris per pasien manifest, dalam urutan manifest (`PAT-DEC-045`).</summary>
        public List<RsmmcBulkPatientMigrationItemResponse> Items { get; set; } = [];
    }

    public class RsmmcBulkGateCheckResponse
    {
        public string Code { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Expected { get; set; } = string.Empty;

        public string Actual { get; set; } = string.Empty;

        public bool Passed { get; set; }
    }

    public class RsmmcBulkPatientMigrationItemResponse
    {
        /// <summary>Teks angka, supaya nilai <c>bigint</c> tidak terpotong di klien JavaScript.</summary>
        public string LegacyPid { get; set; } = string.Empty;

        public string? SourceGroup { get; set; }

        public string CrosswalkStateBefore { get; set; } = string.Empty;

        public Guid? PatientId { get; set; }

        public string? PatientCode { get; set; }

        public string? MedicalRecordNumber { get; set; }

        public string? PlannedQrCodePath { get; set; }

        /// <summary>Nama dari preview (`PAT-DEC-049`). Tidak pernah masuk log aplikasi.</summary>
        public string? FullName { get; set; }

        public bool HasIdentityNumber { get; set; }

        public bool HasPhoneNumber { get; set; }

        public string Status { get; set; } = string.Empty;

        /// <summary>Simulasi: berkas QR historis ada di path bawaan. Informasi saja, bukan konflik.</summary>
        public bool? HistoricalQrAtDefaultPath { get; set; }

        public long? HistoricalQrLength { get; set; }

        public string? HistoricalQrSha256 { get; set; }

        /// <summary>Simulasi: path rencana <c>rsmmc-bulk</c> sudah terisi. <c>true</c> berarti eksekusi akan konflik.</summary>
        public bool? PlannedQrExists { get; set; }

        public string? ConflictType { get; set; }

        public string? FailureStage { get; set; }

        public string? Message { get; set; }
    }
}
