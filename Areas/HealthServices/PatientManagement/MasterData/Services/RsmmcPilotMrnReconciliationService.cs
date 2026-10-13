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
    public enum RsmmcPilotMrnReconcileOutcome
    {
        Completed,
        ValidationFailed,
        EnvironmentRejected,
        GateRejected
    }

    public sealed class RsmmcPilotMrnReconcileResult
    {
        public RsmmcPilotMrnReconcileOutcome Outcome { get; init; }

        public string Message { get; init; } = string.Empty;

        public string? ErrorCode { get; init; }

        public RsmmcPilotMrnReconcileResponse? Data { get; init; }
    }

    /// <summary>
    /// Rekonsiliasi MRN 715 pasien Pilot V1 RSMMC ke rencana MRN kanonik (`BE-PAT-MIG-001`).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Pilot V1 dibuat lewat API pembuatan pasien biasa, sehingga MRN-nya dibangkitkan Quilvian dan
    /// tidak sama dengan <c>migration.rsmmc_patient_mrn_plan.final_medical_record_number</c>.
    /// Service ini memindahkan setiap pasien ke MRN kanonik dan membuat QR baru untuk nomor itu.
    /// Yang berubah hanya <c>MedicalRecordNumber</c>, <c>QrCodePath</c>, <c>UpdateDateTime</c>, dan
    /// <c>UpdateBy</c>. <c>Id</c>, <c>PatientCode</c>, data lain, relasi, dan QR lama tetap.
    /// </para>
    /// <para>
    /// Satu pasien satu transaksi. Kegagalan pada satu pasien tidak membatalkan pasien yang sudah
    /// selesai, dan proses berhenti pada kegagalan atau konflik pertama. Pembangkit MRN dan
    /// <c>PatientCode</c> normal tidak pernah dipanggil.
    /// </para>
    /// </remarks>
    public class RsmmcPilotMrnReconciliationService
    {
        public const int DefaultLimit = 25;
        public const int MinLimit = 1;
        public const int MaxLimit = 100;
        public const string FinalizedPlanStatus = "FINALIZED";

        /// <summary>
        /// Tahap kegagalan bila <c>CommitAsync</c> melempar galat. Hasil commit tidak dapat
        /// dipastikan: QR baru tidak dihapus, proses berhenti, dan tidak dicoba ulang otomatis.
        /// </summary>
        public const string CommitOutcomeUncertainStage = "COMMIT_OUTCOME_UNCERTAIN";

        private const string CommitStage = "COMMIT";

        private const string LogCategory = "HealthServices.PatientManagement.MasterData";
        private const string LogAction = "Patient.RsmmcPilotMrnReconcile";

        private static readonly Regex FinalMedicalRecordNumberFormat = new(
            "^[0-9]{2}-[0-9]{2}-[0-9]{2}-[0-9]{2}$",
            RegexOptions.CultureInvariant,
            TimeSpan.FromSeconds(1));

        private readonly ApplicationDbContext _dbContext;
        private readonly IRsmmcPilotMigrationSource _migrationSource;
        private readonly IHostEnvironment _environment;
        private readonly LoggerService _loggerService;
        private readonly RsmmcPilotApprovedBatchProfile _approvedBatch;

        public RsmmcPilotMrnReconciliationService(
            ApplicationDbContext dbContext,
            IRsmmcPilotMigrationSource migrationSource,
            IHostEnvironment environment,
            LoggerService loggerService,
            RsmmcPilotApprovedBatchProfile approvedBatch)
        {
            _dbContext = dbContext;
            _migrationSource = migrationSource;
            _environment = environment;
            _loggerService = loggerService;
            _approvedBatch = approvedBatch;
        }

        public async Task<RsmmcPilotMrnReconcileResult> ReconcileAsync(
            RsmmcPilotMrnReconcileRequest request,
            Guid actorUserId,
            RsmmcPilotQrArtifactStore qrStore,
            string? traceId,
            CancellationToken cancellationToken)
        {
            if (request.BatchId is null || request.BatchId.Value == Guid.Empty)
            {
                return Rejected(
                    RsmmcPilotMrnReconcileOutcome.ValidationFailed,
                    "RSMMC_PILOT_BATCH_ID_REQUIRED",
                    "batchId wajib diisi.");
            }

            var limit = request.Limit ?? DefaultLimit;

            if (limit < MinLimit || limit > MaxLimit)
            {
                return Rejected(
                    RsmmcPilotMrnReconcileOutcome.ValidationFailed,
                    "RSMMC_PILOT_LIMIT_OUT_OF_RANGE",
                    $"limit harus di antara {MinLimit} dan {MaxLimit}.");
            }

            // dryRun yang tidak dikirim selalu berarti simulasi (`PAT-OQ-003`).
            var dryRun = request.DryRun ?? true;
            var batchId = request.BatchId.Value;
            var environmentName = _environment.EnvironmentName ?? string.Empty;

            if (!RsmmcPilotEnvironmentGate.IsAllowed(environmentName))
            {
                await _loggerService.WarningAsync(
                    LogCategory,
                    LogAction,
                    "Rekonsiliasi MRN Pilot RSMMC ditolak karena environment tidak diizinkan.",
                    new { BatchId = batchId, DryRun = dryRun, EnvironmentName = environmentName, ActorUserId = actorUserId });

                return Rejected(
                    RsmmcPilotMrnReconcileOutcome.EnvironmentRejected,
                    "RSMMC_PILOT_ENVIRONMENT_NOT_ALLOWED",
                    $"Rekonsiliasi MRN Pilot RSMMC hanya boleh dijalankan di environment '{RsmmcPilotEnvironmentGate.AllowedEnvironmentName}'.");
            }

            if (!dryRun && actorUserId == Guid.Empty)
            {
                return Rejected(
                    RsmmcPilotMrnReconcileOutcome.ValidationFailed,
                    "RSMMC_PILOT_ACTOR_UNKNOWN",
                    "Pengguna yang menjalankan rekonsiliasi tidak dapat dikenali, sehingga perubahan tidak dapat dicatat aktornya.");
            }

            var snapshot = await LoadPilotSnapshotAsync(batchId, qrStore, cancellationToken);
            var gates = await EvaluateGatesAsync(batchId, snapshot, cancellationToken);

            var response = new RsmmcPilotMrnReconcileResponse
            {
                BatchId = batchId,
                DryRun = dryRun,
                Limit = limit,
                EnvironmentName = environmentName,
                TotalPilot = snapshot.Pilot.Count,
                Gates = gates
            };

            if (!dryRun && gates.Any(x => !x.Passed))
            {
                await _loggerService.WarningAsync(
                    LogCategory,
                    LogAction,
                    "Rekonsiliasi MRN Pilot RSMMC ditolak karena keadaan batch tidak sesuai yang disetujui.",
                    new
                    {
                        BatchId = batchId,
                        DryRun = dryRun,
                        ActorUserId = actorUserId,
                        FailedGates = gates.Where(x => !x.Passed).Select(x => x.Code).ToList()
                    });

                return new RsmmcPilotMrnReconcileResult
                {
                    Outcome = RsmmcPilotMrnReconcileOutcome.GateRejected,
                    ErrorCode = "RSMMC_PILOT_BATCH_GATE_FAILED",
                    Message = "Keadaan batch tidak sesuai yang disetujui. Tidak ada data yang diubah.",
                    Data = response
                };
            }

            var invalidFinal = new List<PilotCandidate>();
            var qrInconsistent = new List<PilotCandidate>();
            var eligible = new List<PilotCandidate>();

            foreach (var candidate in snapshot.Pilot)
            {
                if (!candidate.FinalMedicalRecordNumberValid)
                {
                    invalidFinal.Add(candidate);
                }
                else if (string.Equals(candidate.CurrentMedicalRecordNumber, candidate.FinalMedicalRecordNumber, StringComparison.Ordinal))
                {
                    if (string.Equals(candidate.CurrentQrCodePath, candidate.PlannedQrCodePath, StringComparison.Ordinal))
                    {
                        response.AlreadyReconciled++;
                    }
                    else
                    {
                        qrInconsistent.Add(candidate);
                    }
                }
                else
                {
                    eligible.Add(candidate);
                }
            }

            // Hanya pasien layak yang menghabiskan limit (`PAT-OQ-004`).
            var selected = eligible.Take(limit).ToList();
            var ownersByMedicalRecordNumber = await LoadMedicalRecordNumberOwnersAsync(selected, cancellationToken);

            response.QrInconsistent = qrInconsistent.Count;
            response.EligibleBeforeRun = eligible.Count;
            response.Selected = selected.Count;

            foreach (var candidate in qrInconsistent)
            {
                var item = NewItem(candidate, RsmmcPilotMrnReconciliationStatus.QrInconsistent);
                item.Message = "MRN sudah kanonik tetapi path QR tidak sesuai MRN itu. Tidak diperbaiki otomatis.";
                response.Items.Add(item);
            }

            foreach (var candidate in invalidFinal)
            {
                var item = NewItem(candidate, RsmmcPilotMrnReconciliationStatus.Failed);
                item.FailureStage = "CLASSIFY";
                item.Message = "MRN tujuan pada perencana kosong atau formatnya tidak sah.";
                response.Failed++;
                response.Items.Add(item);
            }

            if (dryRun)
            {
                foreach (var candidate in selected)
                {
                    var item = IsOwnedByAnotherPatient(candidate, ownersByMedicalRecordNumber)
                        ? MedicalRecordNumberConflictItem(candidate)
                        : NewItem(candidate, RsmmcPilotMrnReconciliationStatus.Ready);

                    if (item.Status == RsmmcPilotMrnReconciliationStatus.Ready)
                    {
                        response.Ready++;
                    }
                    else
                    {
                        response.Conflict++;
                    }

                    response.Items.Add(item);
                }

                response.Remaining = eligible.Count;
                SortItems(response);

                await _loggerService.InfoAsync(
                    LogCategory,
                    LogAction,
                    "Simulasi rekonsiliasi MRN Pilot RSMMC dijalankan. Tidak ada data yang diubah.",
                    new
                    {
                        BatchId = batchId,
                        DryRun = true,
                        ActorUserId = actorUserId,
                        response.TotalPilot,
                        response.AlreadyReconciled,
                        response.QrInconsistent,
                        response.EligibleBeforeRun,
                        response.Selected,
                        response.Ready,
                        response.Conflict
                    });

                return Completed(response, "Simulasi rekonsiliasi selesai. Tidak ada data yang diubah.");
            }

            foreach (var candidate in selected)
            {
                RsmmcPilotMrnReconcileItemResponse item;

                if (IsOwnedByAnotherPatient(candidate, ownersByMedicalRecordNumber))
                {
                    item = MedicalRecordNumberConflictItem(candidate);
                    await LogAttemptAsync("WRN", batchId, candidate, null, actorUserId, item, null);
                }
                else
                {
                    item = await ReconcileOneAsync(batchId, candidate, actorUserId, qrStore, traceId, cancellationToken);
                }

                response.Items.Add(item);

                if (item.Status == RsmmcPilotMrnReconciliationStatus.Reconciled)
                {
                    response.Reconciled++;
                    continue;
                }

                if (item.Status == RsmmcPilotMrnReconciliationStatus.Conflict)
                {
                    response.Conflict++;
                }
                else
                {
                    response.Failed++;
                }

                response.Stopped = true;
                response.StopReason =
                    $"Berhenti pada legacy_pid {candidate.LegacyPid} dengan status {item.Status}. Pasien sesudahnya tidak diproses.";
                break;
            }

            response.Remaining = eligible.Count - response.Reconciled;
            SortItems(response);

            var message = response.Stopped
                ? $"Rekonsiliasi berhenti. {response.Reconciled} pasien berhasil direkonsiliasi sebelum berhenti."
                : $"Rekonsiliasi selesai. {response.Reconciled} pasien berhasil direkonsiliasi.";

            return Completed(response, message);
        }

        private async Task<RsmmcPilotMrnReconcileItemResponse> ReconcileOneAsync(
            Guid batchId,
            PilotCandidate candidate,
            Guid actorUserId,
            RsmmcPilotQrArtifactStore qrStore,
            string? traceId,
            CancellationToken cancellationToken)
        {
            var stage = "BEGIN_TRANSACTION";
            var finalMedicalRecordNumber = candidate.FinalMedicalRecordNumber!;
            RsmmcPilotCreatedQrArtifact? createdQr = null;
            IDbContextTransaction? transaction = null;

            try
            {
                transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

                // Pemilihan dilakukan di luar transaksi ini; keadaannya dibaca ulang di sini.
                stage = "REVALIDATE_PLAN";
                var rows = await _migrationSource.GetRowsForRevalidationAsync(batchId, candidate.LegacyPid, cancellationToken);
                var planProblem = FindPlanProblem(rows, candidate);

                if (planProblem != null)
                {
                    return await FailWithoutChangeAsync(batchId, candidate, actorUserId, stage, planProblem);
                }

                stage = "REVALIDATE_PATIENT";
                var patient = await _dbContext.Set<MstPatient>()
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(x => x.Id == candidate.PatientId, cancellationToken);

                var patientProblem = FindPatientProblem(patient, candidate);

                if (patientProblem != null)
                {
                    return await FailWithoutChangeAsync(batchId, candidate, actorUserId, stage, patientProblem);
                }

                stage = "REVALIDATE_MRN_OWNERSHIP";
                var ownedByAnotherPatient = await _dbContext.Set<MstPatient>()
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .AnyAsync(
                        x => x.MedicalRecordNumber == finalMedicalRecordNumber && x.Id != candidate.PatientId,
                        cancellationToken);

                if (ownedByAnotherPatient)
                {
                    var conflict = MedicalRecordNumberConflictItem(candidate);
                    await LogAttemptAsync("WRN", batchId, candidate, null, actorUserId, conflict, null);
                    return conflict;
                }

                stage = "CREATE_QR";
                var qrResult = qrStore.CreateNew(finalMedicalRecordNumber, traceId);

                if (qrResult.IsConflict)
                {
                    var conflict = NewItem(candidate, RsmmcPilotMrnReconciliationStatus.Conflict);
                    conflict.ConflictType = RsmmcPilotMrnConflictType.QrArtifactAlreadyExists;
                    conflict.Message = qrResult.IsRaceConflict
                        ? "Artefak QR untuk MRN kanonik muncul saat QR baru akan dibuat. File itu tidak ditimpa dan data pasien tidak diubah."
                        : "Artefak QR untuk MRN kanonik sudah ada. File itu tidak ditimpa dan data pasien tidak diubah.";

                    await LogAttemptAsync("WRN", batchId, candidate, qrResult.PublicPath, actorUserId, conflict, null);
                    return conflict;
                }

                createdQr = qrResult.Created!;

                stage = "UPDATE_PATIENT";
                patient!.MedicalRecordNumber = finalMedicalRecordNumber;
                patient.QrCodePath = createdQr.PublicPath;
                patient.UpdateDateTime = DateTime.UtcNow;
                patient.UpdateBy = actorUserId;

                stage = "SAVE_CHANGES";
                await _dbContext.SaveChangesAsync(cancellationToken);

                stage = CommitStage;
                await transaction.CommitAsync(cancellationToken);

                var item = NewItem(candidate, RsmmcPilotMrnReconciliationStatus.Reconciled);
                item.PlannedQrCodePath = createdQr.PublicPath;
                item.Message = "MRN dan QR pasien sudah dipindahkan ke nomor kanonik. QR lama dipertahankan.";

                await LogAttemptAsync("INF", batchId, candidate, createdQr.PublicPath, actorUserId, item, null);
                return item;
            }
            catch (Exception ex)
            {
                // Galat dari CommitAsync tidak membuktikan bahwa commit gagal: data mungkin sudah
                // tersimpan. Hasilnya diperlakukan tidak pasti dan tidak pernah ditebak.
                var commitOutcomeUncertain = stage == CommitStage;

                if (transaction != null)
                {
                    await TryRollbackAsync(transaction, batchId, candidate);
                }

                _dbContext.ChangeTracker.Clear();

                var item = NewItem(candidate, RsmmcPilotMrnReconciliationStatus.Failed);

                if (commitOutcomeUncertain)
                {
                    item.FailureStage = CommitOutcomeUncertainStage;
                    item.Message =
                        $"Hasil commit tidak dapat dipastikan. QR lama dan QR baru sama-sama dipertahankan untuk rekonsiliasi operator. Jangan diulang otomatis. TraceId {traceId ?? "-"}.";

                    await LogAttemptAsync("ERR", batchId, candidate, createdQr?.PublicPath, actorUserId, item, ex);
                    return item;
                }

                item.FailureStage = stage;
                item.Message = $"Gagal pada tahap {stage}. Data pasien tidak diubah. Rincian tercatat di log aplikasi dengan TraceId {traceId ?? "-"}.";

                await LogAttemptAsync("ERR", batchId, candidate, createdQr?.PublicPath, actorUserId, item, ex);

                if (createdQr != null)
                {
                    // Transaksi pasien ini tidak pernah di-commit, jadi QR baru tidak ditunjuk siapa pun.
                    var cleanup = await CleanupCreatedQrAsync(qrStore, createdQr, batchId, candidate);

                    if (!cleanup.Deleted && cleanup.Outcome != RsmmcPilotQrCleanupOutcome.AlreadyMissing)
                    {
                        item.Message += $" QR baru dipertahankan karena kepemilikannya tidak terbukti ({cleanup.Outcome}).";
                    }
                }

                return item;
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

        private async Task<RsmmcPilotMrnReconcileItemResponse> FailWithoutChangeAsync(
            Guid batchId,
            PilotCandidate candidate,
            Guid actorUserId,
            string stage,
            string reason)
        {
            var item = NewItem(candidate, RsmmcPilotMrnReconciliationStatus.Failed);
            item.FailureStage = stage;
            item.Message = $"{reason} Data pasien tidak diubah.";

            await LogAttemptAsync("ERR", batchId, candidate, null, actorUserId, item, null);
            return item;
        }

        private static string? FindPlanProblem(IReadOnlyList<RsmmcPilotPlanRow> rows, PilotCandidate candidate)
        {
            if (rows.Count == 0)
            {
                return "Baris perencana untuk legacy_pid ini tidak ditemukan lagi.";
            }

            if (rows.Count > 1)
            {
                return "Pemetaan perencana dan crosswalk untuk legacy_pid ini tidak lagi tunggal.";
            }

            var row = rows[0];

            if (!string.Equals(row.PlanStatus, FinalizedPlanStatus, StringComparison.Ordinal))
            {
                return "Status perencana tidak lagi FINALIZED.";
            }

            if (!string.Equals(row.FinalMedicalRecordNumber, candidate.FinalMedicalRecordNumber, StringComparison.Ordinal))
            {
                return "MRN kanonik pada perencana berubah sejak pasien dipilih.";
            }

            if (!IsValidFinalMedicalRecordNumber(row.FinalMedicalRecordNumber))
            {
                return "MRN kanonik pada perencana kosong atau formatnya tidak sah.";
            }

            if (!Guid.TryParse(row.QuilvianPatientId, out var patientId) || patientId != candidate.PatientId)
            {
                return "Pemetaan crosswalk ke pasien berubah sejak pasien dipilih.";
            }

            return null;
        }

        private static string? FindPatientProblem(MstPatient? patient, PilotCandidate candidate)
        {
            if (patient == null)
            {
                return "Pasien tidak ditemukan lagi.";
            }

            if (!string.Equals(patient.MedicalRecordNumber, candidate.CurrentMedicalRecordNumber, StringComparison.Ordinal))
            {
                return "MRN pasien berubah sejak pasien dipilih.";
            }

            if (!string.Equals(patient.PatientCode, candidate.PatientCode, StringComparison.Ordinal))
            {
                return "PatientCode pasien berubah sejak pasien dipilih.";
            }

            if (!string.Equals(patient.QrCodePath, candidate.CurrentQrCodePath, StringComparison.Ordinal))
            {
                return "Path QR pasien berubah sejak pasien dipilih.";
            }

            return null;
        }

        private async Task<RsmmcPilotQrCleanupResult> CleanupCreatedQrAsync(
            RsmmcPilotQrArtifactStore qrStore,
            RsmmcPilotCreatedQrArtifact createdQr,
            Guid batchId,
            PilotCandidate candidate)
        {
            // Hanya artefak yang terbukti dibuat pemanggilan ini. Path QR lama tidak pernah
            // dikirim ke sini, sehingga QR lama tidak mungkin terhapus lewat jalur ini.
            var cleanup = qrStore.TryDeleteCreated(createdQr);

            if (cleanup.Deleted)
            {
                return cleanup;
            }

            var data = new
            {
                BatchId = batchId,
                candidate.LegacyPid,
                PatientId = candidate.PatientId,
                candidate.PatientCode,
                NewQrCodePath = createdQr.PublicPath,
                ExpectedLength = createdQr.Length,
                ExpectedSha256 = createdQr.Sha256,
                CleanupOutcome = cleanup.Outcome.ToString()
            };

            if (cleanup.Outcome == RsmmcPilotQrCleanupOutcome.Failed)
            {
                // Dicatat terpisah, supaya galat pembersihan tidak menutupi galat database aslinya.
                await _loggerService.ErrorAsync(
                    LogCategory,
                    LogAction,
                    "Pembersihan QR baru sesudah kegagalan database gagal. QR baru tidak dipastikan terhapus; QR lama tidak disentuh.",
                    cleanup.Error,
                    data);
            }
            else
            {
                await _loggerService.WarningAsync(
                    LogCategory,
                    LogAction,
                    "QR baru tidak dihapus karena kepemilikannya tidak terbukti. Perlu diperiksa operator; QR lama tidak disentuh.",
                    data);
            }

            return cleanup;
        }

        private async Task TryRollbackAsync(IDbContextTransaction transaction, Guid batchId, PilotCandidate candidate)
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
                    "Rollback transaksi rekonsiliasi gagal.",
                    rollbackEx,
                    new { BatchId = batchId, candidate.LegacyPid, PatientId = candidate.PatientId });
            }
        }

        private async Task<PilotSnapshot> LoadPilotSnapshotAsync(
            Guid batchId,
            RsmmcPilotQrArtifactStore qrStore,
            CancellationToken cancellationToken)
        {
            var rows = await _migrationSource.GetMappedFinalizedRowsAsync(batchId, cancellationToken);

            var mapped = rows
                .Select(row => (Row: row, PatientId: Guid.TryParse(row.QuilvianPatientId, out var id) ? id : (Guid?)null))
                .ToList();

            var patientIds = mapped
                .Where(x => x.PatientId.HasValue)
                .Select(x => x.PatientId!.Value)
                .Distinct()
                .ToList();

            var patients = await _dbContext.Set<MstPatient>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => patientIds.Contains(x.Id))
                .Select(x => new PatientState(x.Id, x.PatientCode, x.FullName, x.MedicalRecordNumber, x.QrCodePath))
                .ToListAsync(cancellationToken);

            var patientsById = patients.ToDictionary(x => x.Id);

            var pilot = mapped
                .Where(x => x.PatientId.HasValue && patientsById.ContainsKey(x.PatientId.Value))
                .Select(x =>
                {
                    var patient = patientsById[x.PatientId!.Value];
                    var finalMedicalRecordNumber = x.Row.FinalMedicalRecordNumber;
                    var finalValid = IsValidFinalMedicalRecordNumber(finalMedicalRecordNumber);

                    return new PilotCandidate(
                        x.Row.LegacyPid,
                        patient.Id,
                        patient.PatientCode,
                        patient.FullName,
                        patient.MedicalRecordNumber,
                        patient.QrCodePath,
                        finalMedicalRecordNumber,
                        finalValid,
                        finalValid ? qrStore.BuildPublicPath(finalMedicalRecordNumber!) : null);
                })
                .OrderBy(x => x.LegacyPid, LegacyPidComparer.Instance)
                .ToList();

            return new PilotSnapshot(
                MappedRowCount: rows.Count,
                InvalidPatientIdCount: mapped.Count(x => !x.PatientId.HasValue),
                Pilot: pilot);
        }

        private async Task<List<RsmmcPilotMrnGateCheckResponse>> EvaluateGatesAsync(
            Guid batchId,
            PilotSnapshot snapshot,
            CancellationToken cancellationToken)
        {
            var profile = _approvedBatch;
            var pilot = snapshot.Pilot;
            var plannerFinalized = await _migrationSource.CountFinalizedPlanRowsAsync(batchId, cancellationToken);
            var backupRows = await _migrationSource.CountBackupRowsAsync(batchId, cancellationToken);
            var mappedWithPatientId = snapshot.MappedRowCount - snapshot.InvalidPatientIdCount;
            var finalPresent = pilot.Count(x => !string.IsNullOrWhiteSpace(x.FinalMedicalRecordNumber));
            var finalValid = pilot.Count(x => x.FinalMedicalRecordNumberValid);
            var finalDistinct = pilot
                .Where(x => !string.IsNullOrWhiteSpace(x.FinalMedicalRecordNumber))
                .Select(x => x.FinalMedicalRecordNumber)
                .Distinct(StringComparer.Ordinal)
                .Count();

            return
            [
                Gate("APPROVED_BATCH", "batchId sama dengan batch kanonik yang disetujui",
                    profile.BatchId.ToString("D"), batchId.ToString("D"), batchId == profile.BatchId),
                Gate("PLANNER_FINALIZED_ROWS", "Jumlah baris perencana FINALIZED pada batch ini",
                    profile.ExpectedFinalizedPlannerRows, plannerFinalized),
                Gate("BACKUP_ROWS", "Jumlah baris tabel cadangan rollback pada batch ini",
                    profile.ExpectedBackupRows, backupRows),
                Gate("CROSSWALK_PATIENT_ID_VALID", "quilvian_patient_id pada crosswalk yang tidak berbentuk UUID",
                    0, snapshot.InvalidPatientIdCount),
                Gate("MAPPED_ROWS_HAVE_PATIENT", "Baris terpetakan yang MstPatient-nya ada",
                    mappedWithPatientId, pilot.Count),
                Gate("PILOT_ROWS", "Jumlah pasien Pilot terpetakan",
                    profile.ExpectedPilotRows, pilot.Count),
                Gate("PILOT_UNIQUE_LEGACY_PID", "legacy_pid unik pada set Pilot",
                    pilot.Count, pilot.Select(x => x.LegacyPid).Distinct(StringComparer.Ordinal).Count()),
                Gate("PILOT_UNIQUE_PATIENT_ID", "MstPatient.Id unik pada set Pilot",
                    pilot.Count, pilot.Select(x => x.PatientId).Distinct().Count()),
                Gate("FINAL_MRN_PRESENT", "MRN kanonik terisi",
                    pilot.Count, finalPresent),
                Gate("FINAL_MRN_FORMAT", "MRN kanonik berformat 00-00-00-00",
                    pilot.Count, finalValid),
                Gate("FINAL_MRN_UNIQUE", "MRN kanonik unik pada set Pilot",
                    pilot.Count, finalDistinct)
            ];
        }

        private async Task<Dictionary<string, List<Guid>>> LoadMedicalRecordNumberOwnersAsync(
            IReadOnlyCollection<PilotCandidate> selected,
            CancellationToken cancellationToken)
        {
            if (selected.Count == 0)
            {
                return new Dictionary<string, List<Guid>>(StringComparer.Ordinal);
            }

            var finals = selected
                .Select(x => x.FinalMedicalRecordNumber!)
                .Distinct(StringComparer.Ordinal)
                .ToList();

            // Termasuk baris yang sudah ditandai terhapus: indeks unik MedicalRecordNumber tidak
            // bersyarat, jadi nomor itu tetap terpakai.
            var owners = await _dbContext.Set<MstPatient>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => finals.Contains(x.MedicalRecordNumber))
                .Select(x => new { x.Id, x.MedicalRecordNumber })
                .ToListAsync(cancellationToken);

            return owners
                .GroupBy(x => x.MedicalRecordNumber, StringComparer.Ordinal)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Id).ToList(), StringComparer.Ordinal);
        }

        private static bool IsOwnedByAnotherPatient(
            PilotCandidate candidate,
            IReadOnlyDictionary<string, List<Guid>> ownersByMedicalRecordNumber)
        {
            return ownersByMedicalRecordNumber.TryGetValue(candidate.FinalMedicalRecordNumber!, out var owners) &&
                owners.Any(x => x != candidate.PatientId);
        }

        private static RsmmcPilotMrnReconcileItemResponse MedicalRecordNumberConflictItem(PilotCandidate candidate)
        {
            var item = NewItem(candidate, RsmmcPilotMrnReconciliationStatus.Conflict);
            item.ConflictType = RsmmcPilotMrnConflictType.MedicalRecordNumberOwnedByAnotherPatient;
            item.Message = "MRN kanonik sudah dipakai pasien lain. Data pasien tidak diubah.";
            return item;
        }

        private static RsmmcPilotMrnReconcileItemResponse NewItem(PilotCandidate candidate, string status)
        {
            return new RsmmcPilotMrnReconcileItemResponse
            {
                LegacyPid = candidate.LegacyPid,
                PatientId = candidate.PatientId,
                PatientCode = candidate.PatientCode,
                FullName = candidate.FullName,
                CurrentMedicalRecordNumber = candidate.CurrentMedicalRecordNumber,
                FinalMedicalRecordNumber = candidate.FinalMedicalRecordNumber ?? string.Empty,
                CurrentQrCodePath = candidate.CurrentQrCodePath,
                PlannedQrCodePath = candidate.PlannedQrCodePath,
                Status = status
            };
        }

        private static void SortItems(RsmmcPilotMrnReconcileResponse response)
        {
            response.Items = response.Items
                .OrderBy(x => x.LegacyPid, LegacyPidComparer.Instance)
                .ToList();
        }

        private Task LogAttemptAsync(
            string level,
            Guid batchId,
            PilotCandidate candidate,
            string? newQrCodePath,
            Guid actorUserId,
            RsmmcPilotMrnReconcileItemResponse item,
            Exception? exception)
        {
            // Hanya metadata aman. Nama properti sengaja bukan "Id"/"UserId"/"Name", karena
            // LoggerService membaca properti dengan nama itu sebagai identitas pengguna log.
            var data = new
            {
                BatchId = batchId,
                candidate.LegacyPid,
                PatientId = candidate.PatientId,
                candidate.PatientCode,
                OldMedicalRecordNumber = candidate.CurrentMedicalRecordNumber,
                FinalMedicalRecordNumber = candidate.FinalMedicalRecordNumber,
                OldQrCodePath = candidate.CurrentQrCodePath,
                NewQrCodePath = newQrCodePath,
                ActorUserId = actorUserId,
                DryRun = false,
                item.Status,
                item.ConflictType,
                item.FailureStage
            };

            return level switch
            {
                "ERR" => _loggerService.ErrorAsync(LogCategory, LogAction, "Rekonsiliasi MRN Pilot RSMMC gagal untuk satu pasien.", exception, data),
                "WRN" => _loggerService.WarningAsync(LogCategory, LogAction, "Rekonsiliasi MRN Pilot RSMMC menemukan konflik.", data),
                _ => _loggerService.InfoAsync(LogCategory, LogAction, "Rekonsiliasi MRN Pilot RSMMC berhasil untuk satu pasien.", data)
            };
        }

        private static RsmmcPilotMrnGateCheckResponse Gate(string code, string description, int expected, int actual) =>
            Gate(
                code,
                description,
                expected.ToString(CultureInfo.InvariantCulture),
                actual.ToString(CultureInfo.InvariantCulture),
                expected == actual);

        private static RsmmcPilotMrnGateCheckResponse Gate(
            string code,
            string description,
            string expected,
            string actual,
            bool passed) =>
            new()
            {
                Code = code,
                Description = description,
                Expected = expected,
                Actual = actual,
                Passed = passed
            };

        private static bool IsValidFinalMedicalRecordNumber(string? value) =>
            value != null && FinalMedicalRecordNumberFormat.IsMatch(value);

        private static RsmmcPilotMrnReconcileResult Rejected(
            RsmmcPilotMrnReconcileOutcome outcome,
            string errorCode,
            string message) =>
            new() { Outcome = outcome, ErrorCode = errorCode, Message = message };

        private static RsmmcPilotMrnReconcileResult Completed(RsmmcPilotMrnReconcileResponse response, string message) =>
            new() { Outcome = RsmmcPilotMrnReconcileOutcome.Completed, Message = message, Data = response };

        private sealed record PatientState(
            Guid Id,
            string PatientCode,
            string FullName,
            string MedicalRecordNumber,
            string? QrCodePath);

        private sealed record PilotCandidate(
            string LegacyPid,
            Guid PatientId,
            string PatientCode,
            string FullName,
            string CurrentMedicalRecordNumber,
            string? CurrentQrCodePath,
            string? FinalMedicalRecordNumber,
            bool FinalMedicalRecordNumberValid,
            string? PlannedQrCodePath);

        private sealed record PilotSnapshot(
            int MappedRowCount,
            int InvalidPatientIdCount,
            List<PilotCandidate> Pilot);

        /// <summary>
        /// Urutan <c>legacy_pid</c> naik yang deterministik: dibandingkan sebagai angka bila
        /// keduanya angka, selain itu sebagai teks ordinal.
        /// </summary>
        private sealed class LegacyPidComparer : IComparer<string>
        {
            public static readonly LegacyPidComparer Instance = new();

            public int Compare(string? x, string? y)
            {
                var xIsNumber = long.TryParse(x, NumberStyles.None, CultureInfo.InvariantCulture, out var xNumber);
                var yIsNumber = long.TryParse(y, NumberStyles.None, CultureInfo.InvariantCulture, out var yNumber);

                if (xIsNumber && yIsNumber)
                {
                    var byNumber = xNumber.CompareTo(yNumber);
                    return byNumber != 0 ? byNumber : string.CompareOrdinal(x, y);
                }

                if (xIsNumber)
                {
                    return -1;
                }

                if (yIsNumber)
                {
                    return 1;
                }

                return string.CompareOrdinal(x, y);
            }
        }
    }
}
