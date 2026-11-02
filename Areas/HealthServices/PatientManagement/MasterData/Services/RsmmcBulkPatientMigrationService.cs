using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Hosting;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services
{
    public enum RsmmcBulkPatientMigrationOutcome
    {
        Completed,
        ValidationFailed,
        EnvironmentRejected,
        GateRejected
    }

    public sealed class RsmmcBulkPatientMigrationResult
    {
        public RsmmcBulkPatientMigrationOutcome Outcome { get; init; }

        public string Message { get; init; } = string.Empty;

        public string? ErrorCode { get; init; }

        public RsmmcBulkPatientMigrationResponse? Data { get; init; }
    }

    /// <summary>
    /// Worker migrasi massal pasien RSMMC yang dapat dilanjutkan (`BE-PAT-MIG-002`, kontrak `1.3.0`
    /// Bagian B).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Membuat <c>MstPatient</c> persis dari target preview kanonik yang sudah diaudit — dengan
    /// <c>PatientCode</c> preview, MRN akhir perencana, dan QR baru di namespace <c>rsmmc-bulk</c> —
    /// hanya untuk manifest eksekusi <c>RSMMC-CANARY-A</c> v1, dalam urutan manifest. Tidak ada nomor
    /// yang dibangkitkan, tidak ada pencocokan NIK, dan tidak ada pasien pengganti.
    /// </para>
    /// <para>
    /// Satu pasien satu transaksi, dengan kunci transaksi PostgreSQL dua bagian per pasien yang diambil
    /// tanpa menunggu, lalu seluruh keadaan dibaca ulang sesudah kunci didapat (B4.3, B5). Kegagalan
    /// satu pasien tidak membatalkan pasien yang sudah ter-<i>commit</i>, dan eksekusi berhenti pada
    /// <c>CONFLICT</c> atau <c>FAILED</c> pertama (`PAT-DEC-036`). Simulasi tidak pernah berhenti dan
    /// tidak menulis ke database maupun filesystem (`PAT-DEC-046`).
    /// </para>
    /// <para>
    /// Pembatalan: token pemanggil hanya dipakai pada tahap baca sebelum mutasi. Sejak
    /// <c>INSERT_PATIENT</c>, seluruh langkah sampai <c>COMMIT</c> memakai
    /// <see cref="CancellationToken.None"/>, supaya pembatalan request tidak memotong transaksi di tengah
    /// sesudah QR mungkin dibuat. Pembatalan sebelum mutasi menghentikan eksekusi dan melaporkan pasien
    /// itu serta sisanya sebagai <c>NOT_PROCESSED</c>.
    /// </para>
    /// </remarks>
    public class RsmmcBulkPatientMigrationService
    {
        public const int DefaultLimit = 25;
        public const int MinLimit = 1;
        public const int MaxLimit = 25;

        public const string BatchIdRequiredCode = "RSMMC_BULK_BATCH_ID_REQUIRED";
        public const string SelectionModeNotSupportedCode = "RSMMC_BULK_SELECTION_MODE_NOT_SUPPORTED";
        public const string LimitOutOfRangeCode = "RSMMC_BULK_LIMIT_OUT_OF_RANGE";
        public const string ActorUnknownCode = "RSMMC_BULK_ACTOR_UNKNOWN";
        public const string EnvironmentNotAllowedCode = "RSMMC_BULK_ENVIRONMENT_NOT_ALLOWED";
        public const string GateFailedCode = "RSMMC_BULK_GATE_FAILED";

        public const string GatePreviewSchemaSignature = "PREVIEW_SCHEMA_SIGNATURE";
        public const string GateApprovedBatch = "APPROVED_BATCH";
        public const string GateBatchRowExists = "BATCH_ROW_EXISTS";
        public const string GatePlannerFinalizedRows = "PLANNER_FINALIZED_ROWS";
        public const string GatePreviewRows = "PREVIEW_ROWS";
        public const string GatePreviewUnique = "PREVIEW_UNIQUE";
        public const string GateDeferredSet = "DEFERRED_SET";
        public const string GateDeferredReservationsUnused = "DEFERRED_RESERVATIONS_UNUSED";
        public const string GatePatientBaseline = "PATIENT_BASELINE";
        public const string GateNormalAllocationGuard = "NORMAL_ALLOCATION_GUARD";
        public const string GatePreviewCodeUnmapped = "PREVIEW_CODE_UNMAPPED";
        public const string GatePreviewMrnUnmapped = "PREVIEW_MRN_UNMAPPED";
        public const string GateCrosswalkDuplicateTarget = "CROSSWALK_DUPLICATE_TARGET";
        public const string GatePilotCrosswalkIntact = "PILOT_CROSSWALK_INTACT";
        public const string GatePilotPreviewRows = "PILOT_PREVIEW_ROWS";
        public const string GateBackupRows = "BACKUP_ROWS";
        public const string GateManifestIntegrity = "MANIFEST_INTEGRITY";

        /// <summary>Nilai <c>actual</c> gerbang yang kuerinya tidak dijalankan karena tanda tangan skema gagal.</summary>
        public const string NotEvaluated = "NOT_EVALUATED";

        private const string LogCategory = "HealthServices.PatientManagement.MasterData";
        private const string LogAction = "Patient.RsmmcBulkPatientMigration";

        private const string Uuid = "uuid";
        private const string Bigint = "bigint";
        private const string Integer = "integer";
        private const string Text = "text";
        private const string Varchar = "character varying";
        private const string Boolean = "boolean";
        private const string Date = "date";
        private const string Timestamptz = "timestamp with time zone";

        /// <summary>
        /// Tanda tangan skema fisik yang disetujui (`PAT-DEC-063`; `PAT-FACT-079`, `082`, `083`).
        /// <c>rsmmc_patient_batch.batch_id</c> ikut diwajibkan <c>uuid</c> — sasaran foreign key
        /// <c>last_batch_id</c> (`PAT-FACT-079`) — karena dibaca dengan parameter bertipe <c>uuid</c>.
        /// Tabel cadangan Pilot hanya dihitung seluruh barisnya, sehingga tidak bergantung pada tipe kolom.
        /// </summary>
        internal static readonly IReadOnlyList<(string Table, string Column, string[] Types)> ExpectedSchemaSignature =
        [
            ("rsmmc_patient_target_preview", "batch_id", [Uuid]),
            ("rsmmc_patient_target_preview", "legacy_pid", [Bigint]),
            ("rsmmc_patient_target_preview", "source_group", [Varchar]),
            ("rsmmc_patient_target_preview", "legacy_old_rm", [Text]),
            ("rsmmc_patient_target_preview", "legacy_normalized_mrn", [Text]),
            ("rsmmc_patient_target_preview", "legacy_normalized_nik", [Text]),
            ("rsmmc_patient_target_preview", "proposed_patient_code", [Text]),
            ("rsmmc_patient_target_preview", "proposed_medical_record_number", [Text]),
            ("rsmmc_patient_target_preview", "patient_type", [Integer]),
            ("rsmmc_patient_target_preview", "patient_status", [Integer]),
            ("rsmmc_patient_target_preview", "registration_source", [Integer]),
            ("rsmmc_patient_target_preview", "gender", [Integer]),
            ("rsmmc_patient_target_preview", "religion", [Integer]),
            ("rsmmc_patient_target_preview", "marital_status", [Integer]),
            ("rsmmc_patient_target_preview", "blood_type", [Integer]),
            ("rsmmc_patient_target_preview", "full_name", [Text, Varchar]),
            ("rsmmc_patient_target_preview", "birth_date", [Date]),
            ("rsmmc_patient_target_preview", "identity_type", [Text]),
            ("rsmmc_patient_target_preview", "identity_number", [Text]),
            ("rsmmc_patient_target_preview", "phone_number", [Text]),
            ("rsmmc_patient_target_preview", "address", [Text]),
            ("rsmmc_patient_target_preview", "is_member", [Boolean]),
            ("rsmmc_patient_target_preview", "is_newborn", [Boolean]),
            ("rsmmc_patient_target_preview", "is_deceased", [Boolean]),
            ("rsmmc_patient_target_preview", "is_active", [Boolean]),
            ("rsmmc_patient_target_preview", "validation_status", [Varchar]),
            ("rsmmc_patient_mrn_plan", "batch_id", [Uuid]),
            ("rsmmc_patient_mrn_plan", "legacy_pid", [Bigint]),
            ("rsmmc_patient_mrn_plan", "final_medical_record_number", [Varchar]),
            ("rsmmc_patient_mrn_plan", "plan_status", [Varchar]),
            ("rsmmc_patient_crosswalk", "legacy_pid", [Bigint]),
            ("rsmmc_patient_crosswalk", "canonical_legacy_pid", [Bigint]),
            ("rsmmc_patient_crosswalk", "legacy_old_rm", [Text]),
            ("rsmmc_patient_crosswalk", "normalized_mrn", [Text]),
            ("rsmmc_patient_crosswalk", "normalized_nik", [Text]),
            ("rsmmc_patient_crosswalk", "quilvian_patient_id", [Uuid]),
            ("rsmmc_patient_crosswalk", "match_status", [Varchar]),
            ("rsmmc_patient_crosswalk", "migration_status", [Varchar]),
            ("rsmmc_patient_crosswalk", "review_reason", [Text]),
            ("rsmmc_patient_crosswalk", "last_batch_id", [Uuid]),
            ("rsmmc_patient_crosswalk", "created_at", [Timestamptz]),
            ("rsmmc_patient_crosswalk", "updated_at", [Timestamptz]),
            ("rsmmc_patient_batch", "batch_id", [Uuid])
        ];

        private static readonly Regex FinalMedicalRecordNumberFormat = new(
            "^[0-9]{2}-[0-9]{2}-[0-9]{2}-[0-9]{2}$",
            RegexOptions.CultureInvariant,
            TimeSpan.FromSeconds(1));

        private readonly ApplicationDbContext _dbContext;
        private readonly IRsmmcBulkMigrationStore _store;
        private readonly IHostEnvironment _environment;
        private readonly LoggerService _loggerService;
        private readonly RsmmcBulkApprovedBatchProfile _profile;
        private readonly RsmmcBulkCanaryManifest _manifest;
        private readonly PatientIdentifierAllocationService _identifierAllocationService;

        public RsmmcBulkPatientMigrationService(
            ApplicationDbContext dbContext,
            IRsmmcBulkMigrationStore store,
            IHostEnvironment environment,
            LoggerService loggerService,
            RsmmcBulkApprovedBatchProfile profile,
            RsmmcBulkCanaryManifest manifest,
            PatientIdentifierAllocationService identifierAllocationService)
        {
            _dbContext = dbContext;
            _store = store;
            _environment = environment;
            _loggerService = loggerService;
            _profile = profile;
            _manifest = manifest;
            _identifierAllocationService = identifierAllocationService;
        }

        public async Task<RsmmcBulkPatientMigrationResult> MigrateAsync(
            RsmmcBulkPatientMigrationRequest request,
            Guid actorUserId,
            RsmmcPilotQrArtifactStore qrStore,
            RsmmcQrDestinationProbe qrProbe,
            string? traceId,
            CancellationToken cancellationToken)
        {
            if (request.BatchId is null || request.BatchId.Value == Guid.Empty)
            {
                return Rejected(RsmmcBulkPatientMigrationOutcome.ValidationFailed, BatchIdRequiredCode, "batchId wajib diisi.");
            }

            if (!string.Equals(request.SelectionMode, RsmmcBulkSelectionMode.CanaryA, StringComparison.Ordinal))
            {
                return Rejected(
                    RsmmcBulkPatientMigrationOutcome.ValidationFailed,
                    SelectionModeNotSupportedCode,
                    $"selectionMode wajib diisi dan build ini hanya mendukung '{RsmmcBulkSelectionMode.CanaryA}'.");
            }

            var limit = request.Limit ?? DefaultLimit;

            if (limit < MinLimit || limit > MaxLimit)
            {
                return Rejected(
                    RsmmcBulkPatientMigrationOutcome.ValidationFailed,
                    LimitOutOfRangeCode,
                    $"limit harus di antara {MinLimit} dan {MaxLimit}.");
            }

            // dryRun yang tidak dikirim selalu berarti simulasi; tidak ada default implisit false.
            var dryRun = request.DryRun ?? true;
            var batchId = request.BatchId.Value;
            var environmentName = _environment.EnvironmentName ?? string.Empty;

            // Daftar izin positif Staging persis, dievaluasi sebelum satu tabel pun dibaca.
            if (!RsmmcPilotEnvironmentGate.IsAllowed(environmentName))
            {
                await _loggerService.WarningAsync(
                    LogCategory,
                    LogAction,
                    WithFields(
                        "Migrasi massal pasien RSMMC ditolak karena environment tidak diizinkan.",
                        ("batchId", batchId),
                        ("dryRun", dryRun),
                        ("environmentName", environmentName),
                        ("actorUserId", actorUserId)));

                return Rejected(
                    RsmmcBulkPatientMigrationOutcome.EnvironmentRejected,
                    EnvironmentNotAllowedCode,
                    $"Migrasi massal pasien RSMMC hanya boleh dijalankan di environment '{RsmmcPilotEnvironmentGate.AllowedEnvironmentName}'.");
            }

            if (!dryRun && actorUserId == Guid.Empty)
            {
                return Rejected(
                    RsmmcBulkPatientMigrationOutcome.ValidationFailed,
                    ActorUnknownCode,
                    "Pengguna yang menjalankan migrasi tidak dapat dikenali, sehingga pasien yang dibuat tidak dapat dicatat aktornya.");
            }

            var gateEvaluation = await EvaluateGatesAsync(batchId, cancellationToken);

            var response = new RsmmcBulkPatientMigrationResponse
            {
                BatchId = batchId,
                SelectionMode = RsmmcBulkSelectionMode.CanaryA,
                DryRun = dryRun,
                Limit = limit,
                EnvironmentName = environmentName,
                ManifestId = _manifest.ManifestId,
                ManifestVersion = _manifest.Version,
                ManifestSha256 = _manifest.ComputeOrderedSha256(),
                ManifestSize = _manifest.LegacyPids.Count,
                TotalInManifest = _manifest.LegacyPids.Count,
                Gates = gateEvaluation.Gates
            };

            if (!dryRun && gateEvaluation.Gates.Any(x => !x.Passed))
            {
                await _loggerService.WarningAsync(
                    LogCategory,
                    LogAction,
                    WithFields(
                        "Migrasi massal pasien RSMMC ditolak karena gerbang tingkat panggilan gagal. Tidak ada data yang diubah.",
                        ("batchId", batchId),
                        ("dryRun", false),
                        ("failedGates", gateEvaluation.Gates.Where(x => !x.Passed).Select(x => x.Code).ToList()),
                        ("actorUserId", actorUserId)));

                return new RsmmcBulkPatientMigrationResult
                {
                    Outcome = RsmmcBulkPatientMigrationOutcome.GateRejected,
                    ErrorCode = GateFailedCode,
                    Message = "Satu atau lebih gerbang tingkat panggilan gagal. Tidak ada data yang diubah.",
                    Data = response
                };
            }

            if (!gateEvaluation.SchemaValid)
            {
                // Hanya terjadi pada simulasi: kueri bertipe tidak dijalankan, sehingga pasien tidak diklasifikasikan.
                foreach (var legacyPid in _manifest.LegacyPids)
                {
                    response.Items.Add(new RsmmcBulkPatientMigrationItemResponse
                    {
                        LegacyPid = FormatLegacyPid(legacyPid),
                        CrosswalkStateBefore = RsmmcBulkCrosswalkState.Missing,
                        Status = RsmmcBulkPatientMigrationStatus.NotProcessed,
                        Message = $"Gerbang {GatePreviewSchemaSignature} gagal, sehingga pasien tidak diklasifikasikan."
                    });
                }

                response.NotProcessed = response.Items.Count;

                await LogSimulationAsync(response, actorUserId);
                return Completed(response, $"Simulasi selesai tanpa klasifikasi pasien karena gerbang {GatePreviewSchemaSignature} gagal. Tidak ada data yang diubah.");
            }

            var snapshot = await LoadSnapshotAsync(batchId, gateEvaluation.ManifestPreviews, qrStore, qrProbe, includeHistorical: dryRun, cancellationToken);

            // Field ringkasan hanya yang dikunci B2.2. MIGRATED_QR_INCONSISTENT tetap status per baris, tetapi
            // pasiennya sudah dimigrasi, sehingga ikut alreadyMigrated dan tidak termasuk eligibleBeforeRun.
            response.AlreadyMigrated = snapshot.Values.Count(x =>
                x.Item.Status is RsmmcBulkPatientMigrationStatus.AlreadyMigrated or RsmmcBulkPatientMigrationStatus.MigratedQrInconsistent);
            response.EligibleBeforeRun = response.TotalInManifest - response.AlreadyMigrated;
            response.Selected = Math.Min(limit, response.EligibleBeforeRun);

            if (dryRun)
            {
                response.Items = _manifest.LegacyPids.Select(x => snapshot[x].Item).ToList();
                response.Ready = CountStatus(response, RsmmcBulkPatientMigrationStatus.Ready);
                response.Conflict = CountStatus(response, RsmmcBulkPatientMigrationStatus.Conflict);
                response.Failed = CountStatus(response, RsmmcBulkPatientMigrationStatus.Failed);
                response.Remaining = response.EligibleBeforeRun;

                await LogSimulationAsync(response, actorUserId);
                return Completed(response, "Simulasi migrasi massal selesai. Tidak ada data yang diubah.");
            }

            await ExecuteAsync(batchId, limit, actorUserId, response, snapshot, qrStore, qrProbe, traceId, cancellationToken);

            response.NotProcessed = CountStatus(response, RsmmcBulkPatientMigrationStatus.NotProcessed);
            response.Remaining = Math.Max(0, response.EligibleBeforeRun - response.Migrated);

            var message = response.Stopped
                ? $"Migrasi massal berhenti. {response.Migrated} pasien berhasil dibuat sebelum berhenti."
                : $"Migrasi massal selesai. {response.Migrated} pasien berhasil dibuat.";

            return Completed(response, message);
        }

        private async Task ExecuteAsync(
            Guid batchId,
            int limit,
            Guid actorUserId,
            RsmmcBulkPatientMigrationResponse response,
            IReadOnlyDictionary<long, SnapshotEntry> snapshot,
            RsmmcPilotQrArtifactStore qrStore,
            RsmmcQrDestinationProbe qrProbe,
            string? traceId,
            CancellationToken cancellationToken)
        {
            foreach (var legacyPid in _manifest.LegacyPids)
            {
                var before = snapshot[legacyPid].Item;

                if (response.Stopped)
                {
                    response.Items.Add(NotProcessedItem(before, "Eksekusi sudah berhenti sebelum pasien ini diproses."));
                    continue;
                }

                if (response.Migrated >= limit)
                {
                    response.Items.Add(NotProcessedItem(before, "Batas limit panggilan ini sudah tercapai; pasien ini diproses panggilan berikutnya."));
                    continue;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    response.Stopped = true;
                    response.StopReason = $"Dibatalkan pemanggil sebelum legacy_pid {FormatLegacyPid(legacyPid)} diproses.";
                    response.Items.Add(NotProcessedItem(before, "Dibatalkan pemanggil sebelum pasien ini diproses. Data tidak diubah."));
                    continue;
                }

                var outcome = await MigrateOneAsync(batchId, legacyPid, before, actorUserId, qrStore, qrProbe, traceId, cancellationToken);
                response.Items.Add(outcome.Item);

                if (outcome.Cancelled)
                {
                    response.Stopped = true;
                    response.StopReason = $"Dibatalkan pemanggil saat legacy_pid {FormatLegacyPid(legacyPid)} sedang dibaca; tidak ada perubahan pada pasien itu.";
                    continue;
                }

                switch (outcome.Item.Status)
                {
                    case RsmmcBulkPatientMigrationStatus.Migrated:
                        response.Migrated++;
                        break;

                    case RsmmcBulkPatientMigrationStatus.AlreadyMigrated:
                    case RsmmcBulkPatientMigrationStatus.MigratedQrInconsistent:
                        // Hasil idempoten bukan kegagalan tak terduga dan tidak menghabiskan limit (PAT-DEC-036).
                        break;

                    case RsmmcBulkPatientMigrationStatus.Conflict:
                        response.Conflict++;
                        Stop(response, legacyPid, outcome.Item);
                        break;

                    default:
                        response.Failed++;
                        Stop(response, legacyPid, outcome.Item);
                        break;
                }
            }
        }

        private async Task<OneOutcome> MigrateOneAsync(
            Guid batchId,
            long legacyPid,
            RsmmcBulkPatientMigrationItemResponse before,
            Guid actorUserId,
            RsmmcPilotQrArtifactStore qrStore,
            RsmmcQrDestinationProbe qrProbe,
            string? traceId,
            CancellationToken cancellationToken)
        {
            if (!RsmmcBulkApprovedBatchProfile.TryConvertLockKey(legacyPid, out var lockKey))
            {
                var outOfRange = CopyItem(before, RsmmcBulkPatientMigrationStatus.Failed);
                outOfRange.FailureStage = RsmmcBulkPatientFailureStage.LockKeyOutOfRange;
                outOfRange.Message = "legacy_pid berada di luar rentang Int32, sehingga kunci per pasien tidak dapat dibentuk. Data tidak diubah.";
                await LogItemAsync(batchId, outOfRange, actorUserId, null);
                return new OneOutcome(outOfRange, false);
            }

            var stage = RsmmcBulkPatientFailureStage.BeginTransaction;
            IDbContextTransaction? transaction = null;
            RsmmcPilotCreatedQrArtifact? createdQr = null;
            var mutationStarted = false;

            try
            {
                transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

                stage = RsmmcBulkPatientFailureStage.AcquireLock;

                if (!await _store.TryAcquirePatientLockAsync(lockKey, cancellationToken))
                {
                    await RollbackAsync(transaction, batchId, legacyPid);

                    var busy = CopyItem(before, RsmmcBulkPatientMigrationStatus.Conflict);
                    busy.ConflictType = RsmmcBulkPatientConflictType.ConcurrentRun;
                    busy.Message = "Pasien ini sedang diproses sesi lain. Tidak menunggu; data tidak diubah.";
                    await LogItemAsync(batchId, busy, actorUserId, null);
                    return new OneOutcome(busy, false);
                }

                // B4.3 langkah 3–6: seluruh keadaan dibaca ulang sesudah kunci didapat.
                stage = RsmmcBulkPatientFailureStage.RereadPreview;
                var previews = await _store.GetPreviewRowsAsync(batchId, [legacyPid], cancellationToken);

                stage = RsmmcBulkPatientFailureStage.RereadPlanner;
                var planners = await _store.GetPlannerRowsAsync(batchId, [legacyPid], cancellationToken);

                stage = RsmmcBulkPatientFailureStage.RereadCrosswalk;
                var crosswalks = await _store.GetCrosswalkRowsAsync([legacyPid], cancellationToken);

                stage = RsmmcBulkPatientFailureStage.CheckOwnership;
                var states = await BuildStatesAsync(
                    [legacyPid],
                    previews.ToDictionary(x => x.LegacyPid),
                    planners,
                    crosswalks,
                    qrStore,
                    qrProbe,
                    includeHistorical: false,
                    cancellationToken);

                var state = states[legacyPid];
                var classification = Classify(state);

                if (classification.Status != RsmmcBulkPatientMigrationStatus.Ready)
                {
                    await RollbackAsync(transaction, batchId, legacyPid);

                    var unchanged = BuildItem(state, classification);
                    await LogItemAsync(batchId, unchanged, actorUserId, null);
                    return new OneOutcome(unchanged, false);
                }

                var preview = state.Preview!;
                var finalMedicalRecordNumber = state.Planner!.FinalMedicalRecordNumber!;
                var plannedQrCodePath = state.PlannedQrCodePath!;

                // Sejak titik ini pembatalan request tidak lagi memotong transaksi pasien.
                mutationStarted = true;

                stage = RsmmcBulkPatientFailureStage.InsertPatient;
                var patient = RsmmcBulkPatientMapper.ToPatient(preview, finalMedicalRecordNumber, plannedQrCodePath, actorUserId, DateTime.UtcNow);
                _dbContext.Set<MstPatient>().Add(patient);
                await _dbContext.SaveChangesAsync(CancellationToken.None);

                stage = RsmmcBulkPatientFailureStage.InsertCrosswalk;
                await _store.InsertCrosswalkAsync(
                    new RsmmcBulkCrosswalkInsert(
                        legacyPid,
                        preview.LegacyOldRm,
                        preview.LegacyNormalizedMrn,
                        preview.LegacyNormalizedNik,
                        patient.Id,
                        batchId),
                    CancellationToken.None);

                stage = RsmmcBulkPatientFailureStage.VerifyPersisted;
                var verificationProblem = await VerifyPersistedAsync(patient, legacyPid, batchId);

                if (verificationProblem != null)
                {
                    await RollbackAsync(transaction, batchId, legacyPid);
                    _dbContext.ChangeTracker.Clear();

                    var mismatch = BuildItem(state, Classification.Failure(RsmmcBulkPatientFailureStage.VerifyPersisted, verificationProblem));
                    await LogItemAsync(batchId, mismatch, actorUserId, null);
                    return new OneOutcome(mismatch, false);
                }

                stage = RsmmcBulkPatientFailureStage.CreateQr;
                var qrResult = qrStore.CreateNew(finalMedicalRecordNumber, traceId);

                if (qrResult.IsConflict)
                {
                    await RollbackAsync(transaction, batchId, legacyPid);
                    _dbContext.ChangeTracker.Clear();

                    var conflict = BuildItem(state, Classification.ConflictOf(
                        RsmmcBulkPatientConflictType.QrArtifactAlreadyExists,
                        qrResult.IsRaceConflict
                            ? "Berkas QR muncul di path rsmmc-bulk saat QR baru akan dibuat. Berkas itu tidak ditimpa dan tidak diadopsi; transaksi pasien dibatalkan."
                            : "Berkas QR sudah ada di path rsmmc-bulk. Berkas itu tidak ditimpa dan tidak diadopsi; transaksi pasien dibatalkan."));
                    await LogItemAsync(batchId, conflict, actorUserId, null);
                    return new OneOutcome(conflict, false);
                }

                createdQr = qrResult.Created!;

                stage = RsmmcBulkPatientFailureStage.Commit;
                await transaction.CommitAsync(CancellationToken.None);

                var migrated = BuildItem(state, new Classification(RsmmcBulkPatientMigrationStatus.Migrated, null, null,
                    "Pasien, baris crosswalk, dan QR baru sudah ter-commit. QR historis tidak disentuh."));
                migrated.PatientId = patient.Id;
                migrated.CrosswalkStateBefore = before.CrosswalkStateBefore;

                await LogItemAsync(batchId, migrated, actorUserId, null);
                return new OneOutcome(migrated, false);
            }
            catch (OperationCanceledException) when (!mutationStarted && cancellationToken.IsCancellationRequested)
            {
                if (transaction != null)
                {
                    await RollbackAsync(transaction, batchId, legacyPid);
                }

                return new OneOutcome(NotProcessedItem(before, "Dibatalkan pemanggil sebelum ada perubahan pada pasien ini."), true);
            }
            catch (Exception ex)
            {
                // Galat dari CommitAsync tidak membuktikan bahwa commit gagal; hasilnya tidak ditebak (kasus J).
                var commitOutcomeUncertain = stage == RsmmcBulkPatientFailureStage.Commit;

                if (transaction != null)
                {
                    await RollbackAsync(transaction, batchId, legacyPid);
                }

                _dbContext.ChangeTracker.Clear();

                var failed = CopyItem(before, RsmmcBulkPatientMigrationStatus.Failed);

                if (commitOutcomeUncertain)
                {
                    failed.FailureStage = RsmmcBulkPatientFailureStage.CommitOutcomeUncertain;
                    failed.Message =
                        $"Hasil commit tidak dapat dipastikan. QR baru dipertahankan dan tidak dicoba ulang otomatis; panggilan berikutnya menyelesaikannya lewat crosswalk. TraceId {traceId ?? "-"}.";

                    await LogItemAsync(batchId, failed, actorUserId, ex);
                    return new OneOutcome(failed, false);
                }

                failed.FailureStage = stage;
                failed.Message = stage == RsmmcBulkPatientFailureStage.CreateQr && createdQr is null
                    ? $"Gagal membuat QR baru. Transaksi pasien tidak di-commit; berkas sebagian, bila ada, dipertahankan untuk diperiksa operator. TraceId {traceId ?? "-"}."
                    : $"Gagal pada tahap {stage}. Transaksi pasien dibatalkan. Rincian tercatat di log aplikasi dengan TraceId {traceId ?? "-"}.";

                await LogItemAsync(batchId, failed, actorUserId, ex);

                if (createdQr != null)
                {
                    // Transaksi tidak pernah di-commit, sehingga QR baru tidak ditunjuk pasien mana pun.
                    var cleanup = await CleanupCreatedQrAsync(qrStore, createdQr, batchId, legacyPid);

                    if (!cleanup.Deleted && cleanup.Outcome != RsmmcPilotQrCleanupOutcome.AlreadyMissing)
                    {
                        failed.Message += $" QR baru dipertahankan karena kepemilikannya tidak terbukti ({cleanup.Outcome}).";
                    }
                }

                return new OneOutcome(failed, false);
            }
            finally
            {
                if (transaction != null)
                {
                    await transaction.DisposeAsync();
                }

                _dbContext.ChangeTracker.Clear();
            }
        }

        // B4.3 langkah 10: baca ulang pasien dan crosswalk dari database di transaksi yang sama.
        private async Task<string?> VerifyPersistedAsync(MstPatient expected, long legacyPid, Guid batchId)
        {
            var persisted = await _dbContext.Set<MstPatient>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == expected.Id, CancellationToken.None);

            if (persisted is null)
            {
                return "Pasien yang baru di-insert tidak terbaca kembali. Transaksi pasien dibatalkan.";
            }

            var differences = RsmmcBulkPatientMapper.FindPersistedDifferences(expected, persisted);

            if (differences.Count > 0)
            {
                return $"Nilai tersimpan berbeda dari preview pada kolom: {string.Join(", ", differences)}. Transaksi pasien dibatalkan.";
            }

            var crosswalk = (await _store.GetCrosswalkRowsAsync([legacyPid], CancellationToken.None)).SingleOrDefault();

            if (crosswalk is null ||
                crosswalk.QuilvianPatientId != expected.Id ||
                crosswalk.LastBatchId != batchId ||
                !string.Equals(crosswalk.MatchStatus, RsmmcBulkApprovedBatchProfile.MatchStatusTargetCreated, StringComparison.Ordinal) ||
                !string.Equals(crosswalk.MigrationStatus, RsmmcBulkApprovedBatchProfile.MigrationStatusBulkWorker, StringComparison.Ordinal))
            {
                return "Baris crosswalk baru tidak terbaca kembali sesuai nilai yang disetujui. Transaksi pasien dibatalkan.";
            }

            return null;
        }

        private async Task<Dictionary<long, SnapshotEntry>> LoadSnapshotAsync(
            Guid batchId,
            IReadOnlyDictionary<long, RsmmcBulkPreviewRow> manifestPreviews,
            RsmmcPilotQrArtifactStore qrStore,
            RsmmcQrDestinationProbe qrProbe,
            bool includeHistorical,
            CancellationToken cancellationToken)
        {
            var legacyPids = _manifest.LegacyPids.Distinct().ToList();
            var planners = await _store.GetPlannerRowsAsync(batchId, legacyPids, cancellationToken);
            var crosswalks = await _store.GetCrosswalkRowsAsync(legacyPids, cancellationToken);

            var states = await BuildStatesAsync(
                legacyPids, manifestPreviews, planners, crosswalks, qrStore, qrProbe, includeHistorical, cancellationToken);

            return states.ToDictionary(
                x => x.Key,
                x => new SnapshotEntry(BuildItem(x.Value, Classify(x.Value))));
        }

        private async Task<Dictionary<long, CandidateState>> BuildStatesAsync(
            IReadOnlyCollection<long> legacyPids,
            IReadOnlyDictionary<long, RsmmcBulkPreviewRow> previews,
            IReadOnlyList<RsmmcBulkPlannerRow> planners,
            IReadOnlyList<RsmmcBulkCrosswalkRow> crosswalks,
            RsmmcPilotQrArtifactStore qrStore,
            RsmmcQrDestinationProbe qrProbe,
            bool includeHistorical,
            CancellationToken cancellationToken)
        {
            var plannerByPid = planners.GroupBy(x => x.LegacyPid).ToDictionary(g => g.Key, g => g.ToList());
            var crosswalkByPid = crosswalks.GroupBy(x => x.LegacyPid).ToDictionary(g => g.Key, g => g.ToList());

            var codes = previews.Values
                .Where(x => !string.IsNullOrEmpty(x.ProposedPatientCode))
                .Select(x => x.ProposedPatientCode!)
                .Distinct(StringComparer.Ordinal)
                .ToList();

            var finals = planners
                .Where(x => IsValidFinalMedicalRecordNumber(x.FinalMedicalRecordNumber))
                .Select(x => x.FinalMedicalRecordNumber!)
                .Distinct(StringComparer.Ordinal)
                .ToList();

            var identities = previews.Values
                .Where(x => x.IdentityNumber != null)
                .Select(x => x.IdentityNumber!)
                .Distinct(StringComparer.Ordinal)
                .ToList();

            var phones = previews.Values
                .Where(x => x.PhoneNumber != null)
                .Select(x => x.PhoneNumber!)
                .Distinct(StringComparer.Ordinal)
                .ToList();

            var targetIds = crosswalks
                .Where(x => x.QuilvianPatientId.HasValue)
                .Select(x => x.QuilvianPatientId!.Value)
                .Distinct()
                .ToList();

            var patients = _dbContext.Set<MstPatient>().IgnoreQueryFilters().AsNoTracking();

            // Kode dan MRN: termasuk pasien bertanda terhapus, karena indeks uniknya tanpa syarat.
            var usedCodes = (await patients
                .Where(x => codes.Contains(x.PatientCode))
                .Select(x => x.PatientCode)
                .ToListAsync(cancellationToken)).ToHashSet(StringComparer.Ordinal);

            var usedFinals = (await patients
                .Where(x => finals.Contains(x.MedicalRecordNumber))
                .Select(x => x.MedicalRecordNumber)
                .ToListAsync(cancellationToken)).ToHashSet(StringComparer.Ordinal);

            // Identitas dan telepon: hanya pasien yang tidak dihapus (B3.2, B4.3 langkah 6). Nilai
            // preview dibandingkan apa adanya; tidak ada normalisasi baru di worker.
            var usedIdentities = (await patients
                .Where(x => !x.IsDelete && x.IdentityNumber != null && identities.Contains(x.IdentityNumber))
                .Select(x => x.IdentityNumber!)
                .ToListAsync(cancellationToken)).ToHashSet(StringComparer.Ordinal);

            var usedPhones = (await patients
                .Where(x => !x.IsDelete && x.PhoneNumber != null && phones.Contains(x.PhoneNumber))
                .Select(x => x.PhoneNumber!)
                .ToListAsync(cancellationToken)).ToHashSet(StringComparer.Ordinal);

            var targets = (await patients
                .Where(x => targetIds.Contains(x.Id))
                .Select(x => new TargetPatient(x.Id, x.PatientCode, x.MedicalRecordNumber, x.QrCodePath))
                .ToListAsync(cancellationToken)).ToDictionary(x => x.Id);

            var targetMappingCounts = new Dictionary<Guid, long>();

            foreach (var targetId in targetIds)
            {
                targetMappingCounts[targetId] = await _store.CountCrosswalkRowsForTargetAsync(targetId, cancellationToken);
            }

            var states = new Dictionary<long, CandidateState>();

            foreach (var legacyPid in legacyPids)
            {
                previews.TryGetValue(legacyPid, out var preview);
                plannerByPid.TryGetValue(legacyPid, out var plannerRows);
                crosswalkByPid.TryGetValue(legacyPid, out var crosswalkRows);

                var planner = plannerRows is { Count: 1 } ? plannerRows[0] : null;
                var crosswalk = crosswalkRows?.FirstOrDefault();
                var finalMedicalRecordNumber = planner?.FinalMedicalRecordNumber;
                var finalValid = IsValidFinalMedicalRecordNumber(finalMedicalRecordNumber);
                var plannedQrCodePath = finalValid ? qrStore.BuildPublicPath(finalMedicalRecordNumber!) : null;

                TargetPatient? target = null;
                long targetMappingCount = 0;

                if (crosswalk?.QuilvianPatientId is { } targetId)
                {
                    targets.TryGetValue(targetId, out target);
                    targetMappingCounts.TryGetValue(targetId, out targetMappingCount);
                }

                RsmmcQrDestinationProbeResult? probe = null;

                if (finalValid)
                {
                    probe = includeHistorical
                        ? qrProbe.Probe(finalMedicalRecordNumber!)
                        : new RsmmcQrDestinationProbeResult(qrProbe.PlannedExists(finalMedicalRecordNumber!), false, null, null);
                }

                states[legacyPid] = new CandidateState(
                    legacyPid,
                    preview,
                    planner,
                    plannerRows?.Count ?? 0,
                    crosswalk,
                    crosswalkRows?.Count ?? 0,
                    target,
                    targetMappingCount,
                    preview?.ProposedPatientCode is { } code && usedCodes.Contains(code),
                    finalValid && usedFinals.Contains(finalMedicalRecordNumber!),
                    preview?.IdentityNumber is { } identity && usedIdentities.Contains(identity),
                    preview?.PhoneNumber is { } phone && usedPhones.Contains(phone),
                    plannedQrCodePath,
                    probe,
                    includeHistorical);
            }

            return states;
        }

        /// <summary>Klasifikasi B3.2 dari keadaan yang sudah dibaca — tanpa I/O.</summary>
        private Classification Classify(CandidateState s)
        {
            if (_profile.DeferredLegacyPids.Contains(s.LegacyPid))
            {
                return Classification.ConflictOf(
                    RsmmcBulkPatientConflictType.DeferredLegacyPid,
                    "legacy_pid ini ditunda pemilik dan tidak pernah diproses worker.");
            }

            if (!RsmmcBulkApprovedBatchProfile.TryConvertLockKey(s.LegacyPid, out _))
            {
                return Classification.Failure(
                    RsmmcBulkPatientFailureStage.LockKeyOutOfRange,
                    "legacy_pid berada di luar rentang Int32, sehingga kunci per pasien tidak dapat dibentuk.");
            }

            if (s.Preview is null ||
                s.Preview.BatchId != _profile.BatchId ||
                !string.Equals(s.Preview.ValidationStatus, RsmmcBulkApprovedBatchProfile.PreviewPendingStatus, StringComparison.Ordinal))
            {
                return Classification.ConflictOf(
                    RsmmcBulkPatientConflictType.PreviewStateChanged,
                    "Baris preview tidak ada, bukan milik batch kanonik, atau validation_status bukan PENDING.");
            }

            if (s.Planner is null ||
                !string.Equals(s.Planner.PlanStatus, RsmmcBulkApprovedBatchProfile.PlannerFinalizedStatus, StringComparison.Ordinal) ||
                !IsValidFinalMedicalRecordNumber(s.Planner.FinalMedicalRecordNumber) ||
                !string.Equals(s.Planner.FinalMedicalRecordNumber, s.Preview.ProposedMedicalRecordNumber, StringComparison.Ordinal))
            {
                return Classification.ConflictOf(
                    RsmmcBulkPatientConflictType.PlannerMismatch,
                    s.PlannerRowCount > 1
                        ? "Baris perencana untuk legacy_pid ini tidak tunggal."
                        : "Perencana tidak FINALIZED, MRN akhirnya berbeda dari MRN usulan preview, atau formatnya bukan 00-00-00-00.");
            }

            if (s.Crosswalk is not null)
            {
                if (s.Crosswalk.QuilvianPatientId is null)
                {
                    return Classification.ConflictOf(
                        RsmmcBulkPatientConflictType.CrosswalkExistsWithoutTarget,
                        "Crosswalk historis ada tanpa target. Baris itu tidak disentuh dan tidak dicocokkan otomatis.");
                }

                if (s.TargetMappingCount > 1)
                {
                    return Classification.ConflictOf(
                        RsmmcBulkPatientConflictType.DuplicateTargetMapping,
                        "quilvian_patient_id yang sama dipetakan lebih dari satu legacy_pid.");
                }

                if (s.Target is null)
                {
                    return Classification.ConflictOf(
                        RsmmcBulkPatientConflictType.OrphanedCrosswalk,
                        "Crosswalk menunjuk pasien yang tidak ada. Pasien tidak dibuat ulang.");
                }

                if (!string.Equals(s.Target.PatientCode, s.Preview.ProposedPatientCode, StringComparison.Ordinal) ||
                    !string.Equals(s.Target.MedicalRecordNumber, s.Planner.FinalMedicalRecordNumber, StringComparison.Ordinal))
                {
                    return Classification.ConflictOf(
                        RsmmcBulkPatientConflictType.TargetMappingMismatch,
                        "Pasien target crosswalk punya PatientCode atau MRN yang berbeda dari preview dan perencana.");
                }

                if (!string.Equals(s.Target.QrCodePath, s.PlannedQrCodePath, StringComparison.Ordinal) ||
                    s.Probe is { PlannedExists: false })
                {
                    return new Classification(
                        RsmmcBulkPatientMigrationStatus.MigratedQrInconsistent,
                        null,
                        null,
                        "Pasien sudah dimigrasi, tetapi QrCodePath atau berkas QR di path rsmmc-bulk tidak sesuai. Dilaporkan, tidak diperbaiki.");
                }

                return new Classification(
                    RsmmcBulkPatientMigrationStatus.AlreadyMigrated,
                    null,
                    null,
                    "Pasien sudah dimigrasi dan pemetaannya cocok. Tidak ada perubahan.");
            }

            var invalidValues = RsmmcBulkPatientMapper.FindInvalidValues(s.Preview, s.PlannedQrCodePath!);

            if (invalidValues.Count > 0)
            {
                return Classification.ConflictOf(
                    RsmmcBulkPatientConflictType.PreviewValueInvalid,
                    $"Nilai preview tidak sah: {string.Join("; ", invalidValues)}. Tidak ada nilai pengganti.");
            }

            if (s.PatientCodeUsed)
            {
                return Classification.ConflictOf(
                    RsmmcBulkPatientConflictType.PatientCodeAlreadyUsed,
                    "PatientCode preview sudah dipakai MstPatient lain.");
            }

            if (s.MedicalRecordNumberUsed)
            {
                return Classification.ConflictOf(
                    RsmmcBulkPatientConflictType.MedicalRecordNumberAlreadyUsed,
                    "MRN akhir perencana sudah dipakai MstPatient lain.");
            }

            if (s.IdentityNumberUsed)
            {
                return Classification.ConflictOf(
                    RsmmcBulkPatientConflictType.IdentityNumberAlreadyUsed,
                    "Nomor identitas preview sudah dipakai pasien lain yang tidak dihapus. Pasien tidak pernah digabung.");
            }

            if (s.PhoneNumberUsed)
            {
                return Classification.ConflictOf(
                    RsmmcBulkPatientConflictType.PhoneAlreadyUsed,
                    "Telepon preview sudah dipakai pasien lain yang tidak dihapus. Nilai preview tidak dibuang atau diubah.");
            }

            if (s.Probe is { PlannedExists: true })
            {
                return Classification.ConflictOf(
                    RsmmcBulkPatientConflictType.QrArtifactAlreadyExists,
                    "Berkas sudah ada di path QR rsmmc-bulk pasien ini. Berkas itu tidak akan ditimpa atau diadopsi.");
            }

            return new Classification(
                RsmmcBulkPatientMigrationStatus.Ready,
                null,
                null,
                "Pasien layak dibuat dari preview.");
        }

        private async Task<GateEvaluation> EvaluateGatesAsync(Guid batchId, CancellationToken cancellationToken)
        {
            var gates = new List<RsmmcBulkGateCheckResponse>();

            // PREVIEW_SCHEMA_SIGNATURE dievaluasi pertama, sebelum kueri bertipe lain (PAT-GAP-009).
            var signatureProblems = FindSignatureProblems(await _store.GetSchemaColumnsAsync(cancellationToken));
            var schemaValid = signatureProblems.Count == 0;

            gates.Add(Gate(
                GatePreviewSchemaSignature,
                "Kolom tabel migrasi yang dipakai worker ada dengan tipe fisik yang disetujui",
                "Seluruh kolom sesuai",
                schemaValid ? "Seluruh kolom sesuai" : string.Join("; ", signatureProblems),
                schemaValid));

            gates.Add(Gate(
                GateApprovedBatch,
                "batchId sama dengan batch kanonik yang disetujui",
                _profile.BatchId.ToString("D"),
                batchId.ToString("D"),
                batchId == _profile.BatchId));

            if (!schemaValid)
            {
                gates.Add(NotEvaluatedGate(GateBatchRowExists, "Batch kanonik ada di migration.rsmmc_patient_batch"));
                gates.Add(NotEvaluatedGate(GatePlannerFinalizedRows, "Baris perencana FINALIZED batch kanonik"));
                gates.Add(NotEvaluatedGate(GatePreviewRows, "Baris preview batch kanonik"));
                gates.Add(NotEvaluatedGate(GatePreviewUnique, "legacy_pid / PatientCode / MRN unik pada preview kanonik"));
                gates.Add(NotEvaluatedGate(GateDeferredSet, "Himpunan legacy_pid tertunda"));
                gates.Add(NotEvaluatedGate(GateDeferredReservationsUnused, "Kode dan MRN cadangan pasien tertunda tidak dipakai MstPatient"));
                gates.Add(NotEvaluatedGate(GatePatientBaseline, "Jumlah MstPatient = baseline tetap + crosswalk MIGRATED_BULK_WORKER"));
                gates.Add(await NormalAllocationGuardGateAsync(cancellationToken));
                gates.Add(NotEvaluatedGate(GatePreviewCodeUnmapped, "MstPatient memakai PatientCode preview tanpa pemetaan"));
                gates.Add(NotEvaluatedGate(GatePreviewMrnUnmapped, "MstPatient memakai MRN preview tanpa pemetaan"));
                gates.Add(NotEvaluatedGate(GateCrosswalkDuplicateTarget, "Grup quilvian_patient_id yang dipetakan lebih dari satu legacy_pid"));
                gates.Add(NotEvaluatedGate(GatePilotCrosswalkIntact, "Pemetaan Pilot utuh"));
                gates.Add(NotEvaluatedGate(GatePilotPreviewRows, "Preview Pilot historis"));
                gates.Add(NotEvaluatedGate(GateBackupRows, "Seluruh baris tabel cadangan Pilot"));
                gates.Add(ManifestIntegrityGate(null));

                return new GateEvaluation(gates, false, new Dictionary<long, RsmmcBulkPreviewRow>());
            }

            gates.Add(CountGate(
                GateBatchRowExists,
                "Batch kanonik ada di migration.rsmmc_patient_batch",
                1,
                await _store.CountBatchRowsAsync(batchId, cancellationToken)));

            gates.Add(CountGate(
                GatePlannerFinalizedRows,
                "Baris perencana FINALIZED batch kanonik",
                _profile.ExpectedFinalizedPlannerRows,
                await _store.CountFinalizedPlannerRowsAsync(batchId, cancellationToken)));

            var uniqueness = await _store.GetPreviewUniquenessAsync(batchId, cancellationToken);

            gates.Add(CountGate(GatePreviewRows, "Baris preview batch kanonik", _profile.ExpectedPreviewRows, uniqueness.Rows));

            var expectedUnique = _profile.ExpectedPreviewRows;
            gates.Add(Gate(
                GatePreviewUnique,
                "legacy_pid / PatientCode / MRN unik pada preview kanonik",
                $"{expectedUnique}/{expectedUnique}/{expectedUnique}",
                $"{uniqueness.DistinctLegacyPids}/{uniqueness.DistinctPatientCodes}/{uniqueness.DistinctMedicalRecordNumbers}",
                uniqueness.DistinctLegacyPids == expectedUnique &&
                uniqueness.DistinctPatientCodes == expectedUnique &&
                uniqueness.DistinctMedicalRecordNumbers == expectedUnique));

            var expectedDeferred = _profile.DeferredLegacyPids.OrderBy(x => x).ToList();
            var actualDeferred = await _store.GetDeferredLegacyPidsAsync(batchId, expectedDeferred.Count + 10, cancellationToken);

            gates.Add(Gate(
                GateDeferredSet,
                "legacy_pid perencana FINALIZED yang tidak ada di preview kanonik dan bukan Pilot",
                FormatLegacyPids(expectedDeferred),
                FormatLegacyPids(actualDeferred),
                actualDeferred.OrderBy(x => x).SequenceEqual(expectedDeferred)));

            gates.Add(await DeferredReservationsGateAsync(batchId, cancellationToken));

            // PATIENT_BASELINE (PAT-DEC-077): seluruh baris MstPatient, tanpa filter IsDelete, wajib sama
            // dengan baseline tetap + crosswalk hasil worker pada batch kanonik.
            var patientCount = await _dbContext.Set<MstPatient>().IgnoreQueryFilters().LongCountAsync(cancellationToken);
            var workerCount = await _store.CountWorkerMigratedCrosswalkAsync(batchId, cancellationToken);
            var expectedPatients = _profile.ApprovedPatientBaseline + workerCount;

            gates.Add(Gate(
                GatePatientBaseline,
                "Jumlah seluruh MstPatient = baseline tetap + crosswalk MIGRATED_BULK_WORKER batch kanonik",
                $"{_profile.ApprovedPatientBaseline} + {workerCount} = {expectedPatients}",
                patientCount.ToString(CultureInfo.InvariantCulture),
                patientCount == expectedPatients));

            gates.Add(await NormalAllocationGuardGateAsync(cancellationToken));

            gates.Add(CountGate(
                GatePreviewCodeUnmapped,
                "MstPatient yang memakai PatientCode preview tetapi tidak dipetakan ke legacy_pid pemilik kode itu",
                0,
                await _store.CountPreviewCodesUsedByUnmappedPatientsAsync(batchId, cancellationToken)));

            gates.Add(CountGate(
                GatePreviewMrnUnmapped,
                "MstPatient yang memakai MRN preview tetapi tidak dipetakan ke legacy_pid pemilik MRN itu",
                0,
                await _store.CountPreviewMedicalRecordNumbersUsedByUnmappedPatientsAsync(batchId, cancellationToken)));

            gates.Add(CountGate(
                GateCrosswalkDuplicateTarget,
                "Grup quilvian_patient_id yang dipetakan lebih dari satu legacy_pid",
                0,
                await _store.CountDuplicateTargetGroupsAsync(cancellationToken)));

            var pilot = await _store.GetPilotCrosswalkStateAsync(cancellationToken);

            gates.Add(Gate(
                GatePilotCrosswalkIntact,
                "Pemetaan Pilot: target / MIGRATED_API_AUTO / MIGRATED_API_PILOT",
                $"{_profile.ExpectedPilotTargets}/{_profile.ExpectedPilotAutoMigrated}/{_profile.ExpectedPilotManualMigrated}",
                $"{pilot.Targets}/{pilot.AutoMigrated}/{pilot.ManualMigrated}",
                pilot.Targets == _profile.ExpectedPilotTargets &&
                pilot.AutoMigrated == _profile.ExpectedPilotAutoMigrated &&
                pilot.ManualMigrated == _profile.ExpectedPilotManualMigrated));

            gates.Add(CountGate(
                GatePilotPreviewRows,
                "Preview Pilot historis",
                _profile.ExpectedPilotPreviewRows,
                await _store.CountPreviewRowsAsync(_profile.PilotBatchId, cancellationToken)));

            gates.Add(CountGate(
                GateBackupRows,
                "Seluruh baris tabel cadangan Pilot",
                _profile.ExpectedBackupRows,
                await _store.CountBackupRowsAsync(cancellationToken)));

            var manifestPreviews = (await _store.GetPreviewRowsAsync(batchId, _manifest.LegacyPids.Distinct().ToList(), cancellationToken))
                .GroupBy(x => x.LegacyPid)
                .ToDictionary(g => g.Key, g => g.First());

            gates.Add(ManifestIntegrityGate(manifestPreviews));

            return new GateEvaluation(gates, true, manifestPreviews);
        }

        private async Task<RsmmcBulkGateCheckResponse> DeferredReservationsGateAsync(Guid batchId, CancellationToken cancellationToken)
        {
            var deferredPlanner = await _store.GetPlannerRowsAsync(batchId, _profile.DeferredLegacyPids, cancellationToken);

            var deferredFinals = deferredPlanner
                .Select(x => x.FinalMedicalRecordNumber)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!)
                .Distinct(StringComparer.Ordinal)
                .ToList();

            var reservedCodes = _profile.DeferredReservedPatientCodes.ToList();

            var used = await _dbContext.Set<MstPatient>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .CountAsync(
                    x => reservedCodes.Contains(x.PatientCode) || deferredFinals.Contains(x.MedicalRecordNumber),
                    cancellationToken);

            var allFinalsFound = deferredFinals.Count == _profile.DeferredLegacyPids.Count;

            return Gate(
                GateDeferredReservationsUnused,
                "Kode dan MRN cadangan pasien tertunda tidak dipakai MstPatient",
                $"0 dipakai; MRN tertunda ditemukan {_profile.DeferredLegacyPids.Count}/{_profile.DeferredLegacyPids.Count}",
                $"{used} dipakai; MRN tertunda ditemukan {deferredFinals.Count}/{_profile.DeferredLegacyPids.Count}",
                used == 0 && allFinalsFound);
        }

        // Memakai pemeriksa baca-saja milik BE-PAT-MIG-003/003A apa adanya; algoritma guard tidak disalin.
        private async Task<RsmmcBulkGateCheckResponse> NormalAllocationGuardGateAsync(CancellationToken cancellationToken)
        {
            var status = await _identifierAllocationService.CheckAsync(cancellationToken);

            return Gate(
                GateNormalAllocationGuard,
                "Guard reservasi nomor pendaftaran normal dan bayi aktif dan terverifikasi (overallGuardReady)",
                "overallGuardReady = true",
                $"overallGuardReady = {Bool(status.OverallGuardReady)}; guardState = {status.GuardState}; " +
                $"normal = {status.NormalPatientCreateAllocationMode}; newborn = {status.NewbornPatientCreateAllocationMode}; " +
                $"nextPatientCode = {status.NextPatientCodeFormatted ?? "-"}; nextMrn = {status.NextMrnFormatted ?? "-"}",
                status.OverallGuardReady);
        }

        private RsmmcBulkGateCheckResponse ManifestIntegrityGate(IReadOnlyDictionary<long, RsmmcBulkPreviewRow>? manifestPreviews)
        {
            var pids = _manifest.LegacyPids;
            var distinct = pids.Distinct().Count();
            var sha256 = _manifest.ComputeOrderedSha256();
            var shaMatches = string.Equals(sha256, _manifest.ExpectedOrderedSha256, StringComparison.Ordinal);
            var expectedSize = _manifest.ExpectedSourceGroupCounts.Values.Sum();

            var expected =
                $"{expectedSize} nilai unik; sha256 berurutan {_manifest.ExpectedOrderedSha256}; {expectedSize} ada di preview; " +
                FormatGroups(_manifest.ExpectedSourceGroupCounts);

            if (manifestPreviews is null)
            {
                return Gate(
                    GateManifestIntegrity,
                    "Manifest eksekusi utuh: isi, urutan, sidik jari, keberadaan di preview, dan sebaran kelompok sumber",
                    expected,
                    $"{distinct} nilai unik dari {pids.Count}; sha256 berurutan {sha256}; keberadaan di preview {NotEvaluated}",
                    false);
            }

            var present = pids.Count(manifestPreviews.ContainsKey);
            var groups = pids
                .Where(manifestPreviews.ContainsKey)
                .GroupBy(x => manifestPreviews[x].SourceGroup ?? "(NULL)", StringComparer.Ordinal)
                .ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal);

            var groupsMatch =
                groups.Count == _manifest.ExpectedSourceGroupCounts.Count &&
                _manifest.ExpectedSourceGroupCounts.All(x => groups.TryGetValue(x.Key, out var count) && count == x.Value);

            var passed =
                pids.Count == expectedSize &&
                distinct == pids.Count &&
                shaMatches &&
                present == pids.Count &&
                groupsMatch;

            return Gate(
                GateManifestIntegrity,
                "Manifest eksekusi utuh: isi, urutan, sidik jari, keberadaan di preview, dan sebaran kelompok sumber",
                expected,
                $"{distinct} nilai unik dari {pids.Count}; sha256 berurutan {sha256}; {present} ada di preview; {FormatGroups(groups)}",
                passed);
        }

        internal static IReadOnlyList<string> FindSignatureProblems(IReadOnlyList<RsmmcBulkSchemaColumn> columns)
        {
            var actual = columns
                .GroupBy(x => (x.TableName, x.ColumnName))
                .ToDictionary(g => g.Key, g => g.First().DataType);

            var problems = new List<string>();

            foreach (var (table, column, types) in ExpectedSchemaSignature)
            {
                if (!actual.TryGetValue((table, column), out var dataType))
                {
                    problems.Add($"{table}.{column} tidak ada");
                }
                else if (!types.Contains(dataType, StringComparer.Ordinal))
                {
                    problems.Add($"{table}.{column} bertipe {dataType}, diharapkan {string.Join(" atau ", types)}");
                }
            }

            return problems;
        }

        private async Task<RsmmcPilotQrCleanupResult> CleanupCreatedQrAsync(
            RsmmcPilotQrArtifactStore qrStore,
            RsmmcPilotCreatedQrArtifact createdQr,
            Guid batchId,
            long legacyPid)
        {
            // Hanya artefak yang terbukti dibuat pemanggilan ini; path historis tidak pernah dikirim ke sini.
            var cleanup = qrStore.TryDeleteCreated(createdQr);

            if (cleanup.Deleted)
            {
                return cleanup;
            }

            (string, object?)[] fields =
            [
                ("batchId", batchId),
                ("legacyPid", FormatLegacyPid(legacyPid)),
                ("qrCodePath", createdQr.PublicPath),
                ("expectedLength", createdQr.Length),
                ("expectedSha256", createdQr.Sha256),
                ("cleanupOutcome", cleanup.Outcome.ToString())
            ];

            if (cleanup.Outcome == RsmmcPilotQrCleanupOutcome.Failed)
            {
                await _loggerService.ErrorAsync(
                    LogCategory,
                    LogAction,
                    WithFields(
                        "Pembersihan QR baru sesudah transaksi pasien dibatalkan gagal. QR baru tidak dipastikan terhapus; QR historis tidak disentuh.",
                        fields),
                    cleanup.Error);
            }
            else
            {
                await _loggerService.WarningAsync(
                    LogCategory,
                    LogAction,
                    WithFields(
                        "QR baru tidak dihapus karena kepemilikannya tidak terbukti. Perlu diperiksa operator; QR historis tidak disentuh.",
                        fields));
            }

            return cleanup;
        }

        private async Task RollbackAsync(IDbContextTransaction transaction, Guid batchId, long legacyPid)
        {
            try
            {
                await transaction.RollbackAsync(CancellationToken.None);
            }
            catch (Exception rollbackEx)
            {
                await _loggerService.ErrorAsync(
                    LogCategory,
                    LogAction,
                    WithFields(
                        "Rollback transaksi migrasi massal gagal.",
                        ("batchId", batchId),
                        ("legacyPid", FormatLegacyPid(legacyPid))),
                    rollbackEx);
            }
        }

        private Task LogSimulationAsync(RsmmcBulkPatientMigrationResponse response, Guid actorUserId)
        {
            return _loggerService.InfoAsync(
                LogCategory,
                LogAction,
                WithFields(
                    "Simulasi migrasi massal pasien RSMMC dijalankan. Tidak ada data yang diubah.",
                    ("batchId", response.BatchId),
                    ("dryRun", true),
                    ("manifestId", response.ManifestId),
                    ("manifestVersion", response.ManifestVersion),
                    ("manifestSha256", response.ManifestSha256),
                    ("totalInManifest", response.TotalInManifest),
                    ("alreadyMigrated", response.AlreadyMigrated),
                    ("eligibleBeforeRun", response.EligibleBeforeRun),
                    ("selected", response.Selected),
                    ("ready", response.Ready),
                    ("conflict", response.Conflict),
                    ("failed", response.Failed),
                    ("notProcessed", response.NotProcessed),
                    ("failedGates", response.Gates.Where(x => !x.Passed).Select(x => x.Code).ToList()),
                    ("actorUserId", actorUserId)));
        }

        // Isi minimum B11 per pasien. Hanya metadata aman (PAT-DEC-049): tanpa nama, NIK, telepon,
        // alamat, maupun tanggal lahir.
        private Task LogItemAsync(Guid batchId, RsmmcBulkPatientMigrationItemResponse item, Guid actorUserId, Exception? exception)
        {
            (string, object?)[] fields =
            [
                ("batchId", batchId),
                ("legacyPid", item.LegacyPid),
                ("patientId", item.PatientId),
                ("patientCode", item.PatientCode),
                ("medicalRecordNumber", item.MedicalRecordNumber),
                ("qrCodePath", item.PlannedQrCodePath),
                ("status", item.Status),
                ("conflictType", item.ConflictType),
                ("failureStage", item.FailureStage),
                ("actorUserId", actorUserId)
            ];

            return item.Status switch
            {
                RsmmcBulkPatientMigrationStatus.Failed => _loggerService.ErrorAsync(
                    LogCategory, LogAction, WithFields("Migrasi massal pasien RSMMC gagal untuk satu pasien.", fields), exception),
                RsmmcBulkPatientMigrationStatus.Conflict => _loggerService.WarningAsync(
                    LogCategory, LogAction, WithFields("Migrasi massal pasien RSMMC menemukan konflik.", fields)),
                RsmmcBulkPatientMigrationStatus.Migrated => _loggerService.InfoAsync(
                    LogCategory, LogAction, WithFields("Migrasi massal pasien RSMMC berhasil untuk satu pasien.", fields)),
                _ => Task.CompletedTask
            };
        }

        // LoggerService hanya menulis teks pesan beserta identitas pengguna — bukan objek data — sehingga
        // isi minimum B11 ditulis ke pesan sebagai pasangan nama=nilai. Pemanggil hanya mengirim nilai aman.
        private static string WithFields(string message, params (string Name, object? Value)[] fields) =>
            message + " " + string.Join(" ", fields.Select(x => $"{x.Name}={FormatLogValue(x.Value)}"));

        private static string FormatLogValue(object? value) => value switch
        {
            null => "-",
            Guid guid => guid.ToString("D"),
            bool flag => Bool(flag),
            IEnumerable<string> values => values.Any() ? string.Join(",", values) : "-",
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? "-"
        };

        private static void Stop(RsmmcBulkPatientMigrationResponse response, long legacyPid, RsmmcBulkPatientMigrationItemResponse item)
        {
            response.Stopped = true;
            response.StopReason =
                $"Berhenti pada legacy_pid {FormatLegacyPid(legacyPid)} dengan status {item.Status}" +
                (item.ConflictType != null ? $" / {item.ConflictType}" : string.Empty) +
                (item.FailureStage != null ? $" / {item.FailureStage}" : string.Empty) +
                ". Pasien sesudahnya tidak diproses.";
        }

        private static RsmmcBulkPatientMigrationItemResponse BuildItem(CandidateState s, Classification classification)
        {
            var item = new RsmmcBulkPatientMigrationItemResponse
            {
                LegacyPid = FormatLegacyPid(s.LegacyPid),
                SourceGroup = s.Preview?.SourceGroup,
                CrosswalkStateBefore = s.Crosswalk is null
                    ? RsmmcBulkCrosswalkState.Missing
                    : s.Crosswalk.QuilvianPatientId is null
                        ? RsmmcBulkCrosswalkState.ExistsWithoutTarget
                        : RsmmcBulkCrosswalkState.ExistsWithTarget,
                PatientId = s.Crosswalk?.QuilvianPatientId,
                PatientCode = s.Preview?.ProposedPatientCode,
                MedicalRecordNumber = s.Planner?.FinalMedicalRecordNumber,
                PlannedQrCodePath = s.PlannedQrCodePath,
                FullName = s.Preview?.FullName,
                HasIdentityNumber = !string.IsNullOrEmpty(s.Preview?.IdentityNumber),
                HasPhoneNumber = !string.IsNullOrEmpty(s.Preview?.PhoneNumber),
                Status = classification.Status,
                ConflictType = classification.ConflictType,
                FailureStage = classification.FailureStage,
                Message = classification.Message
            };

            if (s.IncludeHistorical && s.Probe is not null)
            {
                item.PlannedQrExists = s.Probe.PlannedExists;
                item.HistoricalQrAtDefaultPath = s.Probe.HistoricalExists;
                item.HistoricalQrLength = s.Probe.HistoricalLength;
                item.HistoricalQrSha256 = s.Probe.HistoricalSha256;
            }

            return item;
        }

        private static RsmmcBulkPatientMigrationItemResponse CopyItem(RsmmcBulkPatientMigrationItemResponse source, string status)
        {
            return new RsmmcBulkPatientMigrationItemResponse
            {
                LegacyPid = source.LegacyPid,
                SourceGroup = source.SourceGroup,
                CrosswalkStateBefore = source.CrosswalkStateBefore,
                PatientId = source.PatientId,
                PatientCode = source.PatientCode,
                MedicalRecordNumber = source.MedicalRecordNumber,
                PlannedQrCodePath = source.PlannedQrCodePath,
                FullName = source.FullName,
                HasIdentityNumber = source.HasIdentityNumber,
                HasPhoneNumber = source.HasPhoneNumber,
                Status = status
            };
        }

        private static RsmmcBulkPatientMigrationItemResponse NotProcessedItem(RsmmcBulkPatientMigrationItemResponse source, string message)
        {
            var item = CopyItem(source, RsmmcBulkPatientMigrationStatus.NotProcessed);
            item.Message = message;
            return item;
        }

        private static int CountStatus(RsmmcBulkPatientMigrationResponse response, string status) =>
            response.Items.Count(x => x.Status == status);

        private static RsmmcBulkGateCheckResponse CountGate(string code, string description, long expected, long actual) =>
            Gate(
                code,
                description,
                expected.ToString(CultureInfo.InvariantCulture),
                actual.ToString(CultureInfo.InvariantCulture),
                expected == actual);

        private static RsmmcBulkGateCheckResponse NotEvaluatedGate(string code, string description) =>
            Gate(code, description, "Dievaluasi sesudah tanda tangan skema lulus", NotEvaluated, false);

        private static RsmmcBulkGateCheckResponse Gate(string code, string description, string expected, string actual, bool passed) =>
            new()
            {
                Code = code,
                Description = description,
                Expected = expected,
                Actual = actual,
                Passed = passed
            };

        private static string FormatLegacyPid(long legacyPid) => legacyPid.ToString(CultureInfo.InvariantCulture);

        private static string FormatLegacyPids(IEnumerable<long> legacyPids)
        {
            var text = string.Join(",", legacyPids.Select(FormatLegacyPid));
            return text.Length == 0 ? "(kosong)" : text;
        }

        private static string FormatGroups(IReadOnlyDictionary<string, int> groups) =>
            string.Join(",", groups.OrderBy(x => x.Key, StringComparer.Ordinal).Select(x => $"{x.Key}={x.Value}"));

        private static string Bool(bool value) => value ? "true" : "false";

        private static bool IsValidFinalMedicalRecordNumber(string? value) =>
            value != null && FinalMedicalRecordNumberFormat.IsMatch(value);

        private static RsmmcBulkPatientMigrationResult Rejected(
            RsmmcBulkPatientMigrationOutcome outcome,
            string errorCode,
            string message) =>
            new() { Outcome = outcome, ErrorCode = errorCode, Message = message };

        private static RsmmcBulkPatientMigrationResult Completed(RsmmcBulkPatientMigrationResponse response, string message) =>
            new() { Outcome = RsmmcBulkPatientMigrationOutcome.Completed, Message = message, Data = response };

        private sealed record TargetPatient(Guid Id, string PatientCode, string MedicalRecordNumber, string? QrCodePath);

        private sealed record CandidateState(
            long LegacyPid,
            RsmmcBulkPreviewRow? Preview,
            RsmmcBulkPlannerRow? Planner,
            int PlannerRowCount,
            RsmmcBulkCrosswalkRow? Crosswalk,
            int CrosswalkRowCount,
            TargetPatient? Target,
            long TargetMappingCount,
            bool PatientCodeUsed,
            bool MedicalRecordNumberUsed,
            bool IdentityNumberUsed,
            bool PhoneNumberUsed,
            string? PlannedQrCodePath,
            RsmmcQrDestinationProbeResult? Probe,
            bool IncludeHistorical);

        private sealed record Classification(string Status, string? ConflictType, string? FailureStage, string Message)
        {
            public static Classification ConflictOf(string conflictType, string message) =>
                new(RsmmcBulkPatientMigrationStatus.Conflict, conflictType, null, message);

            public static Classification Failure(string failureStage, string message) =>
                new(RsmmcBulkPatientMigrationStatus.Failed, null, failureStage, message);
        }

        private sealed record SnapshotEntry(RsmmcBulkPatientMigrationItemResponse Item);

        private sealed record OneOutcome(RsmmcBulkPatientMigrationItemResponse Item, bool Cancelled);

        private sealed record GateEvaluation(
            List<RsmmcBulkGateCheckResponse> Gates,
            bool SchemaValid,
            IReadOnlyDictionary<long, RsmmcBulkPreviewRow> ManifestPreviews);
    }
}
