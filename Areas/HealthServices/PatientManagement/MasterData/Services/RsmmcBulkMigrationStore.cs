using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services
{
    /// <summary>
    /// Satu baris <c>migration.rsmmc_patient_target_preview</c> — skema `PAT-FACT-083`, dibaca bertipe.
    /// </summary>
    /// <remarks>
    /// Kolom yang secara fisik <c>NOT NULL</c> tetap <i>nullable</i> di sini supaya nilai yang
    /// menyimpang dari skema yang dilaporkan menjadi konflik terklasifikasi, bukan galat pemetaan.
    /// </remarks>
    public sealed class RsmmcBulkPreviewRow
    {
        public Guid BatchId { get; set; }

        public long LegacyPid { get; set; }

        public string? SourceGroup { get; set; }

        public string? LegacyOldRm { get; set; }

        public string? LegacyNormalizedMrn { get; set; }

        public string? LegacyNormalizedNik { get; set; }

        public string? ProposedPatientCode { get; set; }

        public string? ProposedMedicalRecordNumber { get; set; }

        public int? PatientType { get; set; }

        public int? PatientStatus { get; set; }

        public int? RegistrationSource { get; set; }

        public int? Gender { get; set; }

        public int? Religion { get; set; }

        public int? MaritalStatus { get; set; }

        public int? BloodType { get; set; }

        public string? FullName { get; set; }

        public DateTime? BirthDate { get; set; }

        public string? IdentityType { get; set; }

        public string? IdentityNumber { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }

        public bool? IsMember { get; set; }

        public bool? IsNewborn { get; set; }

        public bool? IsDeceased { get; set; }

        public bool? IsActive { get; set; }

        public string? ValidationStatus { get; set; }
    }

    /// <summary>Satu baris <c>migration.rsmmc_patient_mrn_plan</c> — skema `PAT-FACT-082`.</summary>
    public sealed class RsmmcBulkPlannerRow
    {
        public long LegacyPid { get; set; }

        public string? PlanStatus { get; set; }

        public string? FinalMedicalRecordNumber { get; set; }
    }

    /// <summary>Satu baris <c>migration.rsmmc_patient_crosswalk</c> — skema `PAT-FACT-079`.</summary>
    public sealed class RsmmcBulkCrosswalkRow
    {
        public long LegacyPid { get; set; }

        public Guid? QuilvianPatientId { get; set; }

        public string? MatchStatus { get; set; }

        public string? MigrationStatus { get; set; }

        public Guid? LastBatchId { get; set; }
    }

    /// <summary>Satu kolom tabel migrasi menurut <c>information_schema.columns</c>.</summary>
    public sealed class RsmmcBulkSchemaColumn
    {
        public string TableName { get; set; } = string.Empty;

        public string ColumnName { get; set; } = string.Empty;

        public string DataType { get; set; } = string.Empty;
    }

    public sealed class RsmmcBulkPreviewUniqueness
    {
        public long Rows { get; set; }

        public long DistinctLegacyPids { get; set; }

        public long DistinctPatientCodes { get; set; }

        public long DistinctMedicalRecordNumbers { get; set; }
    }

    public sealed class RsmmcBulkPilotCrosswalkState
    {
        public long Targets { get; set; }

        public long AutoMigrated { get; set; }

        public long ManualMigrated { get; set; }
    }

    /// <summary>
    /// Nilai baris crosswalk baru — tepat kolom `PAT-DEC-047`. <c>canonical_legacy_pid</c>,
    /// <c>review_reason</c>, dan <c>updated_at</c> selalu NULL; <c>created_at</c> waktu database.
    /// </summary>
    public sealed record RsmmcBulkCrosswalkInsert(
        long LegacyPid,
        string? LegacyOldRm,
        string? NormalizedMrn,
        string? NormalizedNik,
        Guid QuilvianPatientId,
        Guid BatchId);

    /// <summary>
    /// Akses tabel <c>migration.rsmmc_*</c> untuk worker migrasi massal (`BE-PAT-MIG-002`).
    /// </summary>
    /// <remarks>
    /// Satu-satunya operasi tulis adalah <see cref="InsertCrosswalkAsync"/>: tidak ada UPDATE atau
    /// DELETE pada tabel migrasi mana pun (kontrak B9, `PAT-DEC-019`, `PAT-DEC-035`).
    /// <see cref="TryAcquirePatientLockAsync"/> dan <see cref="InsertCrosswalkAsync"/> wajib berjalan
    /// di dalam transaksi pasien pemanggil.
    /// </remarks>
    public interface IRsmmcBulkMigrationStore
    {
        Task<IReadOnlyList<RsmmcBulkSchemaColumn>> GetSchemaColumnsAsync(CancellationToken cancellationToken);

        Task<long> CountBatchRowsAsync(Guid batchId, CancellationToken cancellationToken);

        Task<long> CountFinalizedPlannerRowsAsync(Guid batchId, CancellationToken cancellationToken);

        Task<RsmmcBulkPreviewUniqueness> GetPreviewUniquenessAsync(Guid batchId, CancellationToken cancellationToken);

        /// <summary>
        /// <c>legacy_pid</c> perencana <c>FINALIZED</c> yang tidak ada di preview batch ini dan bukan
        /// pemetaan Pilot — himpunan tertunda gerbang <c>DEFERRED_SET</c>.
        /// </summary>
        Task<IReadOnlyList<long>> GetDeferredLegacyPidsAsync(Guid batchId, int maxRows, CancellationToken cancellationToken);

        Task<long> CountWorkerMigratedCrosswalkAsync(Guid batchId, CancellationToken cancellationToken);

        /// <summary><c>MstPatient</c> yang memakai <c>PatientCode</c> preview tetapi tidak dipetakan ke pemilik kode itu.</summary>
        Task<long> CountPreviewCodesUsedByUnmappedPatientsAsync(Guid batchId, CancellationToken cancellationToken);

        /// <summary><c>MstPatient</c> yang memakai MRN usulan preview tetapi tidak dipetakan ke pemilik MRN itu.</summary>
        Task<long> CountPreviewMedicalRecordNumbersUsedByUnmappedPatientsAsync(Guid batchId, CancellationToken cancellationToken);

        /// <summary>Grup <c>quilvian_patient_id</c> yang dipetakan lebih dari satu <c>legacy_pid</c>.</summary>
        Task<long> CountDuplicateTargetGroupsAsync(CancellationToken cancellationToken);

        Task<RsmmcBulkPilotCrosswalkState> GetPilotCrosswalkStateAsync(CancellationToken cancellationToken);

        Task<long> CountPreviewRowsAsync(Guid batchId, CancellationToken cancellationToken);

        /// <summary>Seluruh baris tabel cadangan Pilot — tabel itu seluruhnya milik batch kanonik.</summary>
        Task<long> CountBackupRowsAsync(CancellationToken cancellationToken);

        Task<IReadOnlyList<RsmmcBulkPreviewRow>> GetPreviewRowsAsync(
            Guid batchId,
            IReadOnlyCollection<long> legacyPids,
            CancellationToken cancellationToken);

        Task<IReadOnlyList<RsmmcBulkPlannerRow>> GetPlannerRowsAsync(
            Guid batchId,
            IReadOnlyCollection<long> legacyPids,
            CancellationToken cancellationToken);

        Task<IReadOnlyList<RsmmcBulkCrosswalkRow>> GetCrosswalkRowsAsync(
            IReadOnlyCollection<long> legacyPids,
            CancellationToken cancellationToken);

        Task<long> CountCrosswalkRowsForTargetAsync(Guid patientId, CancellationToken cancellationToken);

        /// <summary>
        /// <c>pg_try_advisory_xact_lock(<see cref="RsmmcBulkApprovedBatchProfile.WorkerLockNamespace"/>, lockKey)</c>
        /// tanpa menunggu. <c>false</c> berarti kunci dipegang sesi lain (`CONCURRENT_RUN`).
        /// </summary>
        Task<bool> TryAcquirePatientLockAsync(int lockKey, CancellationToken cancellationToken);

        Task InsertCrosswalkAsync(RsmmcBulkCrosswalkInsert row, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Implementasi PostgreSQL. SQL berparameter <b>bertipe</b> sesuai skema fisik `PAT-FACT-079`–`083`
    /// — <c>uuid</c>, <c>bigint</c>, <c>integer</c>, <c>text</c> — tanpa cast <c>::text</c> (`PAT-DEC-063`),
    /// sehingga primary key <c>(batch_id, legacy_pid)</c> terpakai.
    /// </summary>
    /// <remarks>
    /// Tabel <c>migration.rsmmc_*</c> tidak dipetakan ke model EF dan tidak ada EF migration (kontrak B9).
    /// Kueri berjalan pada koneksi dan transaksi <see cref="ApplicationDbContext"/> yang sama dengan
    /// insert <c>MstPatient</c>.
    /// </remarks>
    public sealed class RsmmcBulkMigrationStore : IRsmmcBulkMigrationStore
    {
        // Nama tabel adalah konstanta kode, bukan masukan, sehingga ditulis sebagai literal: kolom
        // information_schema bertipe domain sql_identifier, dan literal tidak membutuhkan cast apa pun.
        internal const string SchemaColumnsSql =
            @"SELECT c.table_name AS ""TableName"",
       c.column_name AS ""ColumnName"",
       c.data_type AS ""DataType""
FROM information_schema.columns c
WHERE c.table_schema = 'migration'
  AND c.table_name IN ('rsmmc_patient_target_preview', 'rsmmc_patient_mrn_plan', 'rsmmc_patient_crosswalk',
                       'rsmmc_patient_batch')";

        internal const string CountBatchRowsSql =
            @"SELECT count(*) AS ""Value""
FROM migration.rsmmc_patient_batch b
WHERE b.batch_id = @batch_id";

        internal const string CountFinalizedPlannerRowsSql =
            @"SELECT count(*) AS ""Value""
FROM migration.rsmmc_patient_mrn_plan p
WHERE p.batch_id = @batch_id
  AND p.plan_status = @plan_status";

        internal const string PreviewUniquenessSql =
            @"SELECT count(*) AS ""Rows"",
       count(DISTINCT t.legacy_pid) AS ""DistinctLegacyPids"",
       count(DISTINCT t.proposed_patient_code) AS ""DistinctPatientCodes"",
       count(DISTINCT t.proposed_medical_record_number) AS ""DistinctMedicalRecordNumbers""
FROM migration.rsmmc_patient_target_preview t
WHERE t.batch_id = @batch_id";

        internal const string DeferredLegacyPidsSql =
            @"SELECT p.legacy_pid AS ""Value""
FROM migration.rsmmc_patient_mrn_plan p
WHERE p.batch_id = @batch_id
  AND p.plan_status = @plan_status
  AND NOT EXISTS (
      SELECT 1 FROM migration.rsmmc_patient_target_preview t
      WHERE t.batch_id = @batch_id AND t.legacy_pid = p.legacy_pid)
  AND NOT EXISTS (
      SELECT 1 FROM migration.rsmmc_patient_crosswalk c
      WHERE c.legacy_pid = p.legacy_pid AND c.migration_status = ANY(@pilot_statuses))
ORDER BY p.legacy_pid
LIMIT @max_rows";

        internal const string CountWorkerMigratedCrosswalkSql =
            @"SELECT count(*) AS ""Value""
FROM migration.rsmmc_patient_crosswalk c
WHERE c.last_batch_id = @batch_id
  AND c.migration_status = @migration_status";

        internal const string CountPreviewCodesUsedByUnmappedPatientsSql =
            @"SELECT count(*) AS ""Value""
FROM public.""MstPatient"" m
JOIN migration.rsmmc_patient_target_preview t
  ON t.batch_id = @batch_id AND t.proposed_patient_code = m.""PatientCode""
LEFT JOIN migration.rsmmc_patient_crosswalk c ON c.legacy_pid = t.legacy_pid
WHERE c.quilvian_patient_id IS NULL OR c.quilvian_patient_id <> m.""Id""";

        internal const string CountPreviewMedicalRecordNumbersUsedByUnmappedPatientsSql =
            @"SELECT count(*) AS ""Value""
FROM public.""MstPatient"" m
JOIN migration.rsmmc_patient_target_preview t
  ON t.batch_id = @batch_id AND t.proposed_medical_record_number = m.""MedicalRecordNumber""
LEFT JOIN migration.rsmmc_patient_crosswalk c ON c.legacy_pid = t.legacy_pid
WHERE c.quilvian_patient_id IS NULL OR c.quilvian_patient_id <> m.""Id""";

        internal const string CountDuplicateTargetGroupsSql =
            @"SELECT count(*) AS ""Value""
FROM (
    SELECT c.quilvian_patient_id
    FROM migration.rsmmc_patient_crosswalk c
    WHERE c.quilvian_patient_id IS NOT NULL
    GROUP BY c.quilvian_patient_id
    HAVING count(*) > 1
) d";

        internal const string PilotCrosswalkStateSql =
            @"SELECT count(*) FILTER (WHERE c.quilvian_patient_id IS NOT NULL) AS ""Targets"",
       count(*) FILTER (WHERE c.migration_status = @pilot_auto) AS ""AutoMigrated"",
       count(*) FILTER (WHERE c.migration_status = @pilot_manual) AS ""ManualMigrated""
FROM migration.rsmmc_patient_crosswalk c
WHERE c.migration_status = ANY(@pilot_statuses)";

        internal const string CountPreviewRowsSql =
            @"SELECT count(*) AS ""Value""
FROM migration.rsmmc_patient_target_preview t
WHERE t.batch_id = @batch_id";

        // Seluruh tabel, tanpa filter batch: tipe fisik batch_id tabel cadangan tidak tercatat pada
        // PAT-FACT mana pun, dan cadangan Pilot seluruhnya milik batch kanonik.
        internal const string CountBackupRowsSql =
            @"SELECT count(*) AS ""Value""
FROM migration.rsmmc_patient_pilot_reconcile_backup b";

        internal const string PreviewRowsSql =
            @"SELECT t.batch_id AS ""BatchId"",
       t.legacy_pid AS ""LegacyPid"",
       t.source_group AS ""SourceGroup"",
       t.legacy_old_rm AS ""LegacyOldRm"",
       t.legacy_normalized_mrn AS ""LegacyNormalizedMrn"",
       t.legacy_normalized_nik AS ""LegacyNormalizedNik"",
       t.proposed_patient_code AS ""ProposedPatientCode"",
       t.proposed_medical_record_number AS ""ProposedMedicalRecordNumber"",
       t.patient_type AS ""PatientType"",
       t.patient_status AS ""PatientStatus"",
       t.registration_source AS ""RegistrationSource"",
       t.gender AS ""Gender"",
       t.religion AS ""Religion"",
       t.marital_status AS ""MaritalStatus"",
       t.blood_type AS ""BloodType"",
       t.full_name AS ""FullName"",
       t.birth_date AS ""BirthDate"",
       t.identity_type AS ""IdentityType"",
       t.identity_number AS ""IdentityNumber"",
       t.phone_number AS ""PhoneNumber"",
       t.address AS ""Address"",
       t.is_member AS ""IsMember"",
       t.is_newborn AS ""IsNewborn"",
       t.is_deceased AS ""IsDeceased"",
       t.is_active AS ""IsActive"",
       t.validation_status AS ""ValidationStatus""
FROM migration.rsmmc_patient_target_preview t
WHERE t.batch_id = @batch_id
  AND t.legacy_pid = ANY(@legacy_pids)";

        internal const string PlannerRowsSql =
            @"SELECT p.legacy_pid AS ""LegacyPid"",
       p.plan_status AS ""PlanStatus"",
       p.final_medical_record_number AS ""FinalMedicalRecordNumber""
FROM migration.rsmmc_patient_mrn_plan p
WHERE p.batch_id = @batch_id
  AND p.legacy_pid = ANY(@legacy_pids)";

        internal const string CrosswalkRowsSql =
            @"SELECT c.legacy_pid AS ""LegacyPid"",
       c.quilvian_patient_id AS ""QuilvianPatientId"",
       c.match_status AS ""MatchStatus"",
       c.migration_status AS ""MigrationStatus"",
       c.last_batch_id AS ""LastBatchId""
FROM migration.rsmmc_patient_crosswalk c
WHERE c.legacy_pid = ANY(@legacy_pids)";

        internal const string CountCrosswalkRowsForTargetSql =
            @"SELECT count(*) AS ""Value""
FROM migration.rsmmc_patient_crosswalk c
WHERE c.quilvian_patient_id = @patient_id";

        internal const string TryAcquirePatientLockSql =
            @"SELECT pg_try_advisory_xact_lock(@lock_namespace, @lock_key) AS ""Value""";

        /// <summary>Satu-satunya SQL tulis worker: INSERT crosswalk baru dengan kolom `PAT-DEC-047`.</summary>
        internal const string InsertCrosswalkSql =
            @"INSERT INTO migration.rsmmc_patient_crosswalk
    (legacy_pid, canonical_legacy_pid, legacy_old_rm, normalized_mrn, normalized_nik,
     quilvian_patient_id, match_status, migration_status, review_reason, last_batch_id,
     created_at, updated_at)
VALUES
    (@legacy_pid, NULL, @legacy_old_rm, @normalized_mrn, @normalized_nik,
     @quilvian_patient_id, @match_status, @migration_status, NULL, @last_batch_id,
     CURRENT_TIMESTAMP, NULL)";

        private readonly ApplicationDbContext _dbContext;

        public RsmmcBulkMigrationStore(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<RsmmcBulkSchemaColumn>> GetSchemaColumnsAsync(CancellationToken cancellationToken)
        {
            return await _dbContext.Database
                .SqlQueryRaw<RsmmcBulkSchemaColumn>(SchemaColumnsSql)
                .ToListAsync(cancellationToken);
        }

        public Task<long> CountBatchRowsAsync(Guid batchId, CancellationToken cancellationToken) =>
            ScalarAsync(CountBatchRowsSql, cancellationToken, Uuid("batch_id", batchId));

        public Task<long> CountFinalizedPlannerRowsAsync(Guid batchId, CancellationToken cancellationToken) =>
            ScalarAsync(
                CountFinalizedPlannerRowsSql,
                cancellationToken,
                Uuid("batch_id", batchId),
                Text("plan_status", RsmmcBulkApprovedBatchProfile.PlannerFinalizedStatus));

        public async Task<RsmmcBulkPreviewUniqueness> GetPreviewUniquenessAsync(Guid batchId, CancellationToken cancellationToken)
        {
            return await _dbContext.Database
                .SqlQueryRaw<RsmmcBulkPreviewUniqueness>(PreviewUniquenessSql, Uuid("batch_id", batchId))
                .SingleAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<long>> GetDeferredLegacyPidsAsync(
            Guid batchId,
            int maxRows,
            CancellationToken cancellationToken)
        {
            return await _dbContext.Database
                .SqlQueryRaw<long>(
                    DeferredLegacyPidsSql,
                    Uuid("batch_id", batchId),
                    Text("plan_status", RsmmcBulkApprovedBatchProfile.PlannerFinalizedStatus),
                    PilotStatuses(),
                    Integer("max_rows", maxRows))
                .ToListAsync(cancellationToken);
        }

        public Task<long> CountWorkerMigratedCrosswalkAsync(Guid batchId, CancellationToken cancellationToken) =>
            ScalarAsync(
                CountWorkerMigratedCrosswalkSql,
                cancellationToken,
                Uuid("batch_id", batchId),
                Text("migration_status", RsmmcBulkApprovedBatchProfile.MigrationStatusBulkWorker));

        public Task<long> CountPreviewCodesUsedByUnmappedPatientsAsync(Guid batchId, CancellationToken cancellationToken) =>
            ScalarAsync(CountPreviewCodesUsedByUnmappedPatientsSql, cancellationToken, Uuid("batch_id", batchId));

        public Task<long> CountPreviewMedicalRecordNumbersUsedByUnmappedPatientsAsync(
            Guid batchId,
            CancellationToken cancellationToken) =>
            ScalarAsync(CountPreviewMedicalRecordNumbersUsedByUnmappedPatientsSql, cancellationToken, Uuid("batch_id", batchId));

        public Task<long> CountDuplicateTargetGroupsAsync(CancellationToken cancellationToken) =>
            ScalarAsync(CountDuplicateTargetGroupsSql, cancellationToken);

        public async Task<RsmmcBulkPilotCrosswalkState> GetPilotCrosswalkStateAsync(CancellationToken cancellationToken)
        {
            return await _dbContext.Database
                .SqlQueryRaw<RsmmcBulkPilotCrosswalkState>(
                    PilotCrosswalkStateSql,
                    Text("pilot_auto", RsmmcBulkApprovedBatchProfile.MigrationStatusPilotAuto),
                    Text("pilot_manual", RsmmcBulkApprovedBatchProfile.MigrationStatusPilotManual),
                    PilotStatuses())
                .SingleAsync(cancellationToken);
        }

        public Task<long> CountPreviewRowsAsync(Guid batchId, CancellationToken cancellationToken) =>
            ScalarAsync(CountPreviewRowsSql, cancellationToken, Uuid("batch_id", batchId));

        public Task<long> CountBackupRowsAsync(CancellationToken cancellationToken) =>
            ScalarAsync(CountBackupRowsSql, cancellationToken);

        public async Task<IReadOnlyList<RsmmcBulkPreviewRow>> GetPreviewRowsAsync(
            Guid batchId,
            IReadOnlyCollection<long> legacyPids,
            CancellationToken cancellationToken)
        {
            return await _dbContext.Database
                .SqlQueryRaw<RsmmcBulkPreviewRow>(PreviewRowsSql, Uuid("batch_id", batchId), BigintArray("legacy_pids", legacyPids))
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<RsmmcBulkPlannerRow>> GetPlannerRowsAsync(
            Guid batchId,
            IReadOnlyCollection<long> legacyPids,
            CancellationToken cancellationToken)
        {
            return await _dbContext.Database
                .SqlQueryRaw<RsmmcBulkPlannerRow>(PlannerRowsSql, Uuid("batch_id", batchId), BigintArray("legacy_pids", legacyPids))
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<RsmmcBulkCrosswalkRow>> GetCrosswalkRowsAsync(
            IReadOnlyCollection<long> legacyPids,
            CancellationToken cancellationToken)
        {
            return await _dbContext.Database
                .SqlQueryRaw<RsmmcBulkCrosswalkRow>(CrosswalkRowsSql, BigintArray("legacy_pids", legacyPids))
                .ToListAsync(cancellationToken);
        }

        public Task<long> CountCrosswalkRowsForTargetAsync(Guid patientId, CancellationToken cancellationToken) =>
            ScalarAsync(CountCrosswalkRowsForTargetSql, cancellationToken, Uuid("patient_id", patientId));

        public async Task<bool> TryAcquirePatientLockAsync(int lockKey, CancellationToken cancellationToken)
        {
            EnsureTransaction();

            return await _dbContext.Database
                .SqlQueryRaw<bool>(
                    TryAcquirePatientLockSql,
                    Integer("lock_namespace", RsmmcBulkApprovedBatchProfile.WorkerLockNamespace),
                    Integer("lock_key", lockKey))
                .SingleAsync(cancellationToken);
        }

        public async Task InsertCrosswalkAsync(RsmmcBulkCrosswalkInsert row, CancellationToken cancellationToken)
        {
            EnsureTransaction();

            await _dbContext.Database.ExecuteSqlRawAsync(
                InsertCrosswalkSql,
                [
                    Bigint("legacy_pid", row.LegacyPid),
                    Text("legacy_old_rm", row.LegacyOldRm),
                    Text("normalized_mrn", row.NormalizedMrn),
                    Text("normalized_nik", row.NormalizedNik),
                    Uuid("quilvian_patient_id", row.QuilvianPatientId),
                    Text("match_status", RsmmcBulkApprovedBatchProfile.MatchStatusTargetCreated),
                    Text("migration_status", RsmmcBulkApprovedBatchProfile.MigrationStatusBulkWorker),
                    Uuid("last_batch_id", row.BatchId)
                ],
                cancellationToken);
        }

        private async Task<long> ScalarAsync(string sql, CancellationToken cancellationToken, params NpgsqlParameter[] parameters)
        {
            return await _dbContext.Database
                .SqlQueryRaw<long>(sql, parameters)
                .SingleAsync(cancellationToken);
        }

        // Kunci xact dan insert crosswalk wajib berada di transaksi pasien, supaya lepas dan batal
        // bersama transaksi itu.
        private void EnsureTransaction()
        {
            if (_dbContext.Database.CurrentTransaction is null)
            {
                throw new InvalidOperationException("Operasi migrasi massal ini wajib berjalan di dalam transaksi pasien.");
            }
        }

        // Parameter dibuat baru untuk setiap kueri: satu NpgsqlParameter tidak boleh dipakai dua perintah.
        private static NpgsqlParameter Uuid(string name, Guid value) =>
            new(name, NpgsqlDbType.Uuid) { Value = value };

        private static NpgsqlParameter Bigint(string name, long value) =>
            new(name, NpgsqlDbType.Bigint) { Value = value };

        private static NpgsqlParameter Integer(string name, int value) =>
            new(name, NpgsqlDbType.Integer) { Value = value };

        private static NpgsqlParameter Text(string name, string? value) =>
            new(name, NpgsqlDbType.Text) { Value = (object?)value ?? DBNull.Value };

        private static NpgsqlParameter BigintArray(string name, IReadOnlyCollection<long> values) =>
            new(name, NpgsqlDbType.Array | NpgsqlDbType.Bigint) { Value = values.ToArray() };

        private static NpgsqlParameter TextArray(string name, IReadOnlyCollection<string> values) =>
            new(name, NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = values.ToArray() };

        private static NpgsqlParameter PilotStatuses() =>
            TextArray(
                "pilot_statuses",
                [RsmmcBulkApprovedBatchProfile.MigrationStatusPilotAuto, RsmmcBulkApprovedBatchProfile.MigrationStatusPilotManual]);
    }
}
