using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services
{
    /// <summary>
    /// Layanan implementasi sinkronisasi kartu penjamin pasien "Benefit Karyawan RS" dari HR Benefit ke Patient Management
    /// (BE-FIN-135; FIN-DEC-236, 237, 244, 250, 251; FIN-DES-113, 115, 116; arsitektur V.8, V.9).
    /// Menggantikan NoOpEmployeeBenefitPatientCardSync.
    /// </summary>
    public sealed class EmployeeBenefitCardSyncService : IEmployeeBenefitPatientCardSync
    {
        private const string LogCategory = "HealthServices.PatientManagement.MasterData";

        private readonly ApplicationDbContext _dbContext;
        private readonly WorkforceBenefitOwnerResolver _workforceResolver;
        private readonly EmployeeBenefitEligibilityService _eligibilityService;
        private readonly InternalBenefitCardService _internalBenefitCardService;
        private readonly LoggerService _loggerService;

        public EmployeeBenefitCardSyncService(
            ApplicationDbContext dbContext,
            WorkforceBenefitOwnerResolver workforceResolver,
            EmployeeBenefitEligibilityService eligibilityService,
            InternalBenefitCardService internalBenefitCardService,
            LoggerService loggerService)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _workforceResolver = workforceResolver ?? throw new ArgumentNullException(nameof(workforceResolver));
            _eligibilityService = eligibilityService ?? throw new ArgumentNullException(nameof(eligibilityService));
            _internalBenefitCardService = internalBenefitCardService ?? throw new ArgumentNullException(nameof(internalBenefitCardService));
            _loggerService = loggerService ?? throw new ArgumentNullException(nameof(loggerService));
        }

        /// <summary>
        /// Dipanggil sesudah aktivasi enrollment berhasil di-commit (baik lewat Setujui, Aktifkan manual, atau penjadwal).
        /// </summary>
        public async Task<PatientCardSyncSummaryResponse> OnEnrollmentActivatedAsync(
            Guid enrollmentId,
            Guid actorUserId,
            string triggerSource,
            CancellationToken cancellationToken)
        {
            var enrollment = await _dbContext.TrxEmployeeBenefitEnrollments.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == enrollmentId && !x.IsDelete, cancellationToken);

            if (enrollment == null)
            {
                throw new KeyNotFoundException($"Enrollment benefit dengan ID {enrollmentId} tidak ditemukan.");
            }

            var actualTrigger = string.IsNullOrWhiteSpace(triggerSource)
                ? EmployeeBenefitCardSyncTriggerSources.Activation
                : triggerSource;

            return await ExecuteSyncCoreAsync(enrollment, actorUserId, actualTrigger, cancellationToken);
        }

        /// <summary>
        /// Dipanggil sesudah pembatalan enrollment berhasil di-commit untuk menonaktifkan kartu penjamin terkait (BE-FIN-141).
        /// </summary>
        public async Task OnEnrollmentCancelledAsync(
            Guid enrollmentId,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var lockKey = $"EMPLOYEE_BENEFIT_CARD_SYNC:{enrollmentId}";
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
            if (_dbContext.Database.IsNpgsql())
            {
                await _dbContext.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(hashtext({0}));", [lockKey], cancellationToken);
            }

            var syncRecord = await _dbContext.PatEmployeeBenefitCardSyncs
                .FirstOrDefaultAsync(x => x.EmployeeBenefitEnrollmentId == enrollmentId && !x.IsDelete, cancellationToken);

            if (syncRecord == null)
            {
                await transaction.CommitAsync(cancellationToken);
                return;
            }

            var now = DateTime.UtcNow;

            if (syncRecord.SyncStatus == EmployeeBenefitCardSyncStatuses.Synced && syncRecord.PatientCompanyGuarantorId.HasValue)
            {
                var card = await _dbContext.Set<MstPatientCompanyGuarantor>()
                    .FirstOrDefaultAsync(x => x.Id == syncRecord.PatientCompanyGuarantorId.Value && !x.IsDelete, cancellationToken);

                if (card != null)
                {
                    card.IsActive = false;
                    card.IsPrimary = false;
                    card.UpdateDateTime = now;
                    card.UpdateBy = actorUserId;
                }

                syncRecord.SyncStatus = EmployeeBenefitCardSyncStatuses.Revoked;
                syncRecord.RevokedAt = now;
                syncRecord.ReasonCode = EmployeeBenefitCardSyncReasonCodes.CardRevokedByStaff;
                syncRecord.NextAttemptAt = null; // stop retry
                syncRecord.LastTriggerSource = EmployeeBenefitCardSyncTriggerSources.Cancel;
                syncRecord.LastTriggeredByUserId = actorUserId;
                syncRecord.RowVersion = Guid.NewGuid();
                syncRecord.UpdateDateTime = now;
                syncRecord.UpdateBy = actorUserId;
            }
            else
            {
                // Bila sebelumnya menunggu atau gagal, bersihkan NextAttemptAt agar tidak diulang oleh penjadwal
                syncRecord.NextAttemptAt = null;
                syncRecord.LastTriggerSource = EmployeeBenefitCardSyncTriggerSources.Cancel;
                syncRecord.LastTriggeredByUserId = actorUserId;
                syncRecord.RowVersion = Guid.NewGuid();
                syncRecord.UpdateDateTime = now;
                syncRecord.UpdateBy = actorUserId;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "EmployeeBenefitCardSyncService.OnEnrollmentCancelled",
                $"Catatan sinkronisasi dibatalkan untuk enrollment {enrollmentId}.",
                new { enrollmentId, syncRecord.PatientCompanyGuarantorId, syncRecord.SyncStatus });
        }

        /// <summary>
        /// Permintaan sinkronisasi ulang manual oleh HRD (POST /{id}/patient-card-sync, BE-FIN-138).
        /// </summary>
        public async Task<PatientCardSyncSummaryResponse> RequestResyncAsync(
            Guid enrollmentId,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var enrollment = await _dbContext.TrxEmployeeBenefitEnrollments.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == enrollmentId && !x.IsDelete, cancellationToken);

            if (enrollment == null)
            {
                throw new KeyNotFoundException($"Enrollment benefit dengan ID {enrollmentId} tidak ditemukan.");
            }

            if (!string.Equals(enrollment.EnrollmentStatus, EmployeeBenefitEnrollmentStatuses.Active, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Sinkronisasi ulang hanya dapat diproses pada enrollment benefit yang berstatus Aktif.");
            }

            return await ExecuteSyncCoreAsync(
                enrollment,
                actorUserId,
                EmployeeBenefitCardSyncTriggerSources.ManualResync,
                cancellationToken);
        }

        /// <summary>
        /// Mengambil ringkasan status sinkronisasi kartu untuk sekumpulan enrollment dalam satu batch (hanya baca).
        /// </summary>
        public async Task<Dictionary<Guid, PatientCardSyncSummaryResponse>> GetSummariesAsync(
            IReadOnlyCollection<Guid> enrollmentIds,
            CancellationToken cancellationToken)
        {
            if (enrollmentIds == null || enrollmentIds.Count == 0)
            {
                return new Dictionary<Guid, PatientCardSyncSummaryResponse>();
            }

            var distinctIds = enrollmentIds.Distinct().ToList();

            var records = await _dbContext.PatEmployeeBenefitCardSyncs.AsNoTracking()
                .Where(x => distinctIds.Contains(x.EmployeeBenefitEnrollmentId) && !x.IsDelete)
                .ToListAsync(cancellationToken);

            var recordMap = records.ToDictionary(x => x.EmployeeBenefitEnrollmentId);

            var result = new Dictionary<Guid, PatientCardSyncSummaryResponse>();

            foreach (var id in distinctIds)
            {
                if (recordMap.TryGetValue(id, out var record))
                {
                    result[id] = new PatientCardSyncSummaryResponse
                    {
                        Status = record.SyncStatus,
                        ReasonCode = record.ReasonCode,
                        ReasonLabel = GetReasonLabel(record.ReasonCode),
                        AttemptCount = record.AttemptCount,
                        LastAttemptAt = record.LastAttemptAt,
                        NextAttemptAt = record.NextAttemptAt,
                        SyncedAt = record.SyncedAt
                    };
                }
                else
                {
                    result[id] = new PatientCardSyncSummaryResponse
                    {
                        Status = EmployeeBenefitCardSyncStatuses.NotStarted,
                        ReasonCode = null,
                        ReasonLabel = "Belum ada riwayat sinkronisasi kartu.",
                        AttemptCount = 0
                    };
                }
            }

            return result;
        }

        // ====================================================================================
        // Siklus Rekonsiliasi Penjadwal Otomatis (BE-FIN-136, FIN-INTEGRATION-1.11 V.3)
        // ====================================================================================

        /// <summary>
        /// Menjalankan satu siklus rekonsiliasi penjadwal otomatis sinkronisasi kartu penjamin internal pasien
        /// (BE-FIN-136; FR-FIN-265, FR-FIN-268; FIN-DEC-253; FIN-DES-113, 117, 125; FIN-INTEGRATION-1.11 V.3).
        /// Mencakup 4 langkah rekonsiliasi deterministik dan penjagaan batas pemasangan massal (ActivationCutoffUtc).
        /// </summary>
        public async Task<EmployeeBenefitCardSyncReconciliationResult> RunReconciliationAsync(
            EmployeeBenefitCardSyncSchedulerOptions options,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(options);
            var result = new EmployeeBenefitCardSyncReconciliationResult();

            // AC 1 & FIN-DES-125: Batas pemasangan massal
            // Bila ActivationCutoffUtc kosong/null, penjadwal TIDAK MEMPROSES APA PUN dan mencatat peringatan
            if (!options.ActivationCutoffUtc.HasValue)
            {
                result.WarningCutoffMissing = true;
                await _loggerService.WarningAsync(
                    LogCategory,
                    "EmployeeBenefitCardSyncService.RunReconciliation",
                    "Penjadwal sinkronisasi kartu pasien aktif (Enabled=true) tetapi ActivationCutoffUtc belum ditentukan. " +
                    "Sesuai FIN-DES-125, pemrosesan massal tidak dijalankan demi mencegah pemasangan massal tanpa batas.");
                return result;
            }

            var cutoffUtc = options.ActivationCutoffUtc.Value.UtcDateTime;
            var actorUserId = options.SystemActorUserId ?? Guid.Empty;
            var maxPerCycle = Math.Clamp(options.MaxPerCycle, 1, 500);
            var now = DateTime.UtcNow;

            // ====================================================================================
            // Langkah 1: Enrollment Medical Active, pemilik karyawan atau dokter tetap,
            // ActivatedAt >= ActivationCutoffUtc, belum punya catatan sinkronisasi
            // (AC 2: enrollment sebelum cutoff tidak disentuh)
            // ====================================================================================
            var remainingQuota = maxPerCycle;

            var existingSyncEnrollmentIds = _dbContext.PatEmployeeBenefitCardSyncs.AsNoTracking()
                .Where(x => !x.IsDelete)
                .Select(x => x.EmployeeBenefitEnrollmentId);

            var candidateEnrollments = await (
                from e in _dbContext.TrxEmployeeBenefitEnrollments.AsNoTracking()
                join p in _dbContext.MstBenefitPlans.AsNoTracking() on e.BenefitPlanId equals p.Id
                join t in _dbContext.MstBenefitTypes.AsNoTracking() on p.BenefitTypeId equals t.Id
                where !e.IsDelete
                      && e.EnrollmentStatus == EmployeeBenefitEnrollmentStatuses.Active
                      && t.BenefitCategory == BenefitServiceCategories.Medical
                      && !t.IsClaimBased
                      && e.ActivatedAt.HasValue
                      && e.ActivatedAt.Value >= cutoffUtc
                      && !existingSyncEnrollmentIds.Contains(e.Id)
                orderby e.ActivatedAt
                select e
            ).Take(remainingQuota).ToListAsync(cancellationToken);

            result.Step1NewDiscovered = candidateEnrollments.Count;

            foreach (var enrollment in candidateEnrollments)
            {
                if (cancellationToken.IsCancellationRequested) break;

                try
                {
                    var syncRes = await ExecuteSyncCoreAsync(
                        enrollment,
                        actorUserId,
                        EmployeeBenefitCardSyncTriggerSources.Scheduler,
                        cancellationToken);

                    result.Step1NewProcessed++;
                    if (syncRes.Status == EmployeeBenefitCardSyncStatuses.Synced)
                    {
                        result.Step1NewSuccess++;
                    }
                }
                catch (Exception ex)
                {
                    await _loggerService.ErrorAsync(
                        LogCategory,
                        "EmployeeBenefitCardSyncService.RunReconciliation.Step1",
                        $"Gagal memproses sinkronisasi baru untuk enrollment {enrollment.Id}",
                        ex);
                }

                remainingQuota--;
                if (remainingQuota <= 0) break;
            }

            // ====================================================================================
            // Langkah 2: Catatan FAILED_RETRYABLE atau WAITING_PATIENT yang NextAttemptAt jatuh tempo
            // (AC 3: FAILED_RETRYABLE / WAITING_PATIENT jatuh tempo diulang)
            // (AC 5: Catatan ON_HOLD dan REVOKED TIDAK PERNAH diproses otomatis)
            // ====================================================================================
            if (remainingQuota > 0 && !cancellationToken.IsCancellationRequested)
            {
                var retryRecords = await (
                    from s in _dbContext.PatEmployeeBenefitCardSyncs.AsNoTracking()
                    join e in _dbContext.TrxEmployeeBenefitEnrollments.AsNoTracking() on s.EmployeeBenefitEnrollmentId equals e.Id
                    where !s.IsDelete
                          && (s.SyncStatus == EmployeeBenefitCardSyncStatuses.FailedRetryable || s.SyncStatus == EmployeeBenefitCardSyncStatuses.WaitingPatient)
                          && s.NextAttemptAt.HasValue
                          && s.NextAttemptAt.Value <= now
                          && !e.IsDelete
                          && e.EnrollmentStatus == EmployeeBenefitEnrollmentStatuses.Active
                    orderby s.NextAttemptAt
                    select new { SyncRecord = s, Enrollment = e }
                ).Take(remainingQuota).ToListAsync(cancellationToken);

                result.Step2RetryDiscovered = retryRecords.Count;

                foreach (var item in retryRecords)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    try
                    {
                        var syncRes = await ExecuteSyncCoreAsync(
                            item.Enrollment,
                            actorUserId,
                            EmployeeBenefitCardSyncTriggerSources.Scheduler,
                            cancellationToken);

                        result.Step2RetryProcessed++;
                        if (syncRes.Status == EmployeeBenefitCardSyncStatuses.Synced)
                        {
                            result.Step2RetrySuccess++;
                        }
                    }
                    catch (Exception ex)
                    {
                        await _loggerService.ErrorAsync(
                            LogCategory,
                            "EmployeeBenefitCardSyncService.RunReconciliation.Step2",
                            $"Gagal memproses retry sinkronisasi untuk enrollment {item.Enrollment.Id}",
                            ex);
                    }

                    remainingQuota--;
                    if (remainingQuota <= 0) break;
                }
            }

            // ====================================================================================
            // Langkah 3: Catatan SYNCED yang kartunya kini nonaktif atau terhapus, enrollment masih Active
            // (AC 4: Ubah status menjadi REVOKED, NextAttemptAt null agar tidak dipasang ulang)
            // ====================================================================================
            if (!cancellationToken.IsCancellationRequested)
            {
                var revokedCandidates = await (
                    from s in _dbContext.PatEmployeeBenefitCardSyncs
                    join c in _dbContext.MstPatientCompanyGuarantors on s.PatientCompanyGuarantorId equals c.Id
                    join e in _dbContext.TrxEmployeeBenefitEnrollments on s.EmployeeBenefitEnrollmentId equals e.Id
                    where !s.IsDelete
                          && s.SyncStatus == EmployeeBenefitCardSyncStatuses.Synced
                          && s.PatientCompanyGuarantorId.HasValue
                          && !e.IsDelete
                          && e.EnrollmentStatus == EmployeeBenefitEnrollmentStatuses.Active
                          && (c.IsDelete || !c.IsActive)
                    select new { SyncId = s.Id, s.EmployeeBenefitEnrollmentId, CardIsDelete = c.IsDelete }
                ).Take(50).ToListAsync(cancellationToken);

                foreach (var item in revokedCandidates)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    var lockKey = $"EMPLOYEE_BENEFIT_CARD_SYNC:{item.EmployeeBenefitEnrollmentId}";
                    await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
                    if (_dbContext.Database.IsNpgsql())
                    {
                        await _dbContext.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(hashtext({0}));", [lockKey], cancellationToken);
                    }

                    var syncRecord = await _dbContext.PatEmployeeBenefitCardSyncs
                        .FirstOrDefaultAsync(x => x.Id == item.SyncId && !x.IsDelete, cancellationToken);

                    if (syncRecord != null && syncRecord.SyncStatus == EmployeeBenefitCardSyncStatuses.Synced)
                    {
                        syncRecord.SyncStatus = EmployeeBenefitCardSyncStatuses.Revoked;
                        syncRecord.RevokedAt = now;
                        syncRecord.ReasonCode = item.CardIsDelete
                            ? EmployeeBenefitCardSyncReasonCodes.CardDeleted
                            : EmployeeBenefitCardSyncReasonCodes.CardRevokedByStaff;
                        syncRecord.NextAttemptAt = null; // Penjadwal tidak menyentuhnya lagi (AC 4)
                        syncRecord.LastTriggerSource = EmployeeBenefitCardSyncTriggerSources.Scheduler;
                        syncRecord.LastTriggeredByUserId = actorUserId;
                        syncRecord.RowVersion = Guid.NewGuid();
                        syncRecord.UpdateDateTime = now;
                        syncRecord.UpdateBy = actorUserId;

                        await _dbContext.SaveChangesAsync(cancellationToken);
                        await transaction.CommitAsync(cancellationToken);

                        result.Step3RevokedCount++;

                        await _loggerService.InfoAsync(
                            LogCategory,
                            "EmployeeBenefitCardSyncService.RunReconciliation.Step3",
                            $"Kartu penjamin internal nonaktif/terhapus terdeteksi. Catatan sinkronisasi diubah menjadi REVOKED untuk enrollment {item.EmployeeBenefitEnrollmentId}.",
                            new { item.SyncId, item.EmployeeBenefitEnrollmentId, syncRecord.ReasonCode });
                    }
                    else
                    {
                        await transaction.RollbackAsync(cancellationToken);
                    }
                }
            }

            // ====================================================================================
            // Langkah 4: Catatan SYNCED yang enrollment-nya Cancelled tetapi kartunya masih aktif
            // ====================================================================================
            if (!cancellationToken.IsCancellationRequested)
            {
                var cancelledEnrollmentSyncs = await (
                    from s in _dbContext.PatEmployeeBenefitCardSyncs
                    join c in _dbContext.MstPatientCompanyGuarantors on s.PatientCompanyGuarantorId equals c.Id
                    join e in _dbContext.TrxEmployeeBenefitEnrollments on s.EmployeeBenefitEnrollmentId equals e.Id
                    where !s.IsDelete
                          && s.SyncStatus == EmployeeBenefitCardSyncStatuses.Synced
                          && s.PatientCompanyGuarantorId.HasValue
                          && (e.IsDelete || e.EnrollmentStatus == EmployeeBenefitEnrollmentStatuses.Cancelled)
                          && !c.IsDelete
                          && c.IsActive
                    select s.EmployeeBenefitEnrollmentId
                ).Take(50).ToListAsync(cancellationToken);

                foreach (var enrollmentId in cancelledEnrollmentSyncs)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    try
                    {
                        await OnEnrollmentCancelledAsync(enrollmentId, actorUserId, cancellationToken);
                        result.Step4CancelledCount++;
                    }
                    catch (Exception ex)
                    {
                        await _loggerService.ErrorAsync(
                            LogCategory,
                            "EmployeeBenefitCardSyncService.RunReconciliation.Step4",
                            $"Gagal membatalkan kartu untuk enrollment nonaktif {enrollmentId}",
                            ex);
                    }
                }
            }

            return result;
        }

        // ====================================================================================
        // Inti Algoritma Pencocokan dan Sinkronisasi (V.8)
        // ====================================================================================

        private async Task<PatientCardSyncSummaryResponse> ExecuteSyncCoreAsync(
            TrxEmployeeBenefitEnrollment enrollment,
            Guid actorUserId,
            string triggerSource,
            CancellationToken cancellationToken)
        {
            var lockKey = $"EMPLOYEE_BENEFIT_CARD_SYNC:{enrollment.Id}";
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
            if (_dbContext.Database.IsNpgsql())
            {
                await _dbContext.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(hashtext({0}));", [lockKey], cancellationToken);
            }

            var syncRecord = await _dbContext.PatEmployeeBenefitCardSyncs
                .FirstOrDefaultAsync(x => x.EmployeeBenefitEnrollmentId == enrollment.Id && !x.IsDelete, cancellationToken);

            var attemptCount = (syncRecord?.AttemptCount ?? 0) + 1;
            var now = DateTime.UtcNow;

            // 1. Pemilik: Resolusi profil workforce pemilik benefit
            WorkforceOwnerResolution ownerResolution;
            try
            {
                ownerResolution = await _workforceResolver.ResolveByWorkforceProfileAsync(
                    enrollment.WorkforceProfileId, cancellationToken);
            }
            catch (Exception ex)
            {
                await _loggerService.ErrorAsync(LogCategory, "EmployeeBenefitCardSyncService.ResolveOwner", "Galat pembacaan profil HR", ex);
                return await SaveSyncRecordAsync(syncRecord, enrollment, EmployeeBenefitCardSyncStatuses.FailedRetryable,
                    EmployeeBenefitCardSyncReasonCodes.HrUnavailable, null, null, attemptCount, now, triggerSource, actorUserId, transaction, cancellationToken);
            }

            if (!ownerResolution.IsResolved || ownerResolution.Owner == null || !ownerResolution.Owner.IsActive)
            {
                var reason = ownerResolution.ReasonCode == BenefitEligibilityReasonCodes.OwnerTypeNotAllowed
                    ? EmployeeBenefitCardSyncReasonCodes.CardValidationRejected
                    : EmployeeBenefitCardSyncReasonCodes.HrUnavailable;

                return await SaveSyncRecordAsync(syncRecord, enrollment, EmployeeBenefitCardSyncStatuses.FailedRetryable,
                    reason, null, null, attemptCount, now, triggerSource, actorUserId, transaction, cancellationToken);
            }

            var owner = ownerResolution.Owner;

            // 2. Kelayakan HR pada hari ini untuk hubungan SELF
            var operationalDate = AppDateTimeHelper.OperationalDate();
            BenefitEligibilityResult eligibility;
            try
            {
                eligibility = await _eligibilityService.CheckAsync(new BenefitEligibilityRequest
                {
                    WorkforceProfileId = owner.WorkforceProfileId,
                    EmployeeId = owner.EmployeeId ?? Guid.Empty,
                    FamilyMemberId = null,
                    ServiceDate = operationalDate
                }, cancellationToken);
            }
            catch (Exception ex)
            {
                await _loggerService.ErrorAsync(LogCategory, "EmployeeBenefitCardSyncService.CheckEligibility", "Galat pemeriksaan kelayakan HR", ex);
                return await SaveSyncRecordAsync(syncRecord, enrollment, EmployeeBenefitCardSyncStatuses.FailedRetryable,
                    EmployeeBenefitCardSyncReasonCodes.HrUnavailable, null, null, attemptCount, now, triggerSource, actorUserId, transaction, cancellationToken);
            }

            if (!eligibility.IsEligible)
            {
                var reason = eligibility.ReasonCode == BenefitEligibilityReasonCodes.HrUnavailable
                    ? EmployeeBenefitCardSyncReasonCodes.HrUnavailable
                    : EmployeeBenefitCardSyncReasonCodes.CardValidationRejected;

                return await SaveSyncRecordAsync(syncRecord, enrollment, EmployeeBenefitCardSyncStatuses.FailedRetryable,
                    reason, null, null, attemptCount, now, triggerSource, actorUserId, transaction, cancellationToken);
            }

            // 3. Normalisasi NIK dan Tanggal Lahir Pemilik
            var rawTargetNik = eligibility.TargetPersonNationalId ?? owner.NationalId;
            var targetNik = NormalizeDigits(rawTargetNik);
            var targetDob = eligibility.TargetPersonBirthDate;

            if (string.IsNullOrWhiteSpace(targetNik) || !targetDob.HasValue)
            {
                return await SaveSyncRecordAsync(syncRecord, enrollment, EmployeeBenefitCardSyncStatuses.WaitingPatient,
                    EmployeeBenefitCardSyncReasonCodes.IdentityDataIncomplete, null, null, attemptCount, now, triggerSource, actorUserId, transaction, cancellationToken);
            }

            // 4. Pencarian Pasien Kandidat: Tanggal lahir kalender harus sama persis (V.8 Butir 4)
            var targetCalendarDate = targetDob.Value.Date;

            var candidates = await _dbContext.Set<MstPatient>().AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && x.BirthDate.HasValue && x.BirthDate.Value.Date == targetCalendarDate)
                .Select(x => new
                {
                    x.Id,
                    x.FullName,
                    x.IdentityType,
                    x.IdentityNumber,
                    x.BirthDate,
                    x.MergedToPatientId
                })
                .ToListAsync(cancellationToken);

            // Periksa apakah ada pasien yang NIK dan tanggal lahirnya sama, tapi jenis identitas bukan KTP/NIK
            var sameNikCandidates = candidates
                .Where(x => NormalizeDigits(x.IdentityNumber) == targetNik)
                .ToList();

            if (sameNikCandidates.Count == 0)
            {
                // Tidak ada pasien cocok sama sekali
                return await SaveSyncRecordAsync(syncRecord, enrollment, EmployeeBenefitCardSyncStatuses.WaitingPatient,
                    EmployeeBenefitCardSyncReasonCodes.NoMatch, null, null, attemptCount, now, triggerSource, actorUserId, transaction, cancellationToken);
            }

            // Saring jenis tanda pengenal: KTP atau NIK saja (V.8 Butir 2)
            var validIdentityCandidates = sameNikCandidates
                .Where(x => IsValidIdentityType(x.IdentityType))
                .ToList();

            if (validIdentityCandidates.Count == 0)
            {
                // Pasien ditemukan dengan NIK dan DOB sama, tetapi jenis tanda pengenal bukan KTP/NIK (misal "SIM")
                return await SaveSyncRecordAsync(syncRecord, enrollment, EmployeeBenefitCardSyncStatuses.WaitingPatient,
                    EmployeeBenefitCardSyncReasonCodes.IdentityTypeNotAllowed, null, null, attemptCount, now, triggerSource, actorUserId, transaction, cancellationToken);
            }

            // Pasien kanonik: resolve MergedToPatientId bila ada (V.8 Butir 5)
            var distinctPatientIds = new HashSet<Guid>();
            foreach (var cand in validIdentityCandidates)
            {
                var canonicalId = cand.MergedToPatientId.HasValue && cand.MergedToPatientId.Value != Guid.Empty
                    ? cand.MergedToPatientId.Value
                    : cand.Id;

                // Pastikan pasien kanonik masih aktif dan tidak dihapus
                var isAlive = await _dbContext.Set<MstPatient>().AsNoTracking()
                    .AnyAsync(p => p.Id == canonicalId && p.IsActive && !p.IsDelete, cancellationToken);

                if (isAlive)
                {
                    distinctPatientIds.Add(canonicalId);
                }
            }

            // Evaluasi jumlah kecocokan (V.8 Butir 6)
            if (distinctPatientIds.Count == 0)
            {
                return await SaveSyncRecordAsync(syncRecord, enrollment, EmployeeBenefitCardSyncStatuses.WaitingPatient,
                    EmployeeBenefitCardSyncReasonCodes.NoMatch, null, null, attemptCount, now, triggerSource, actorUserId, transaction, cancellationToken);
            }

            if (distinctPatientIds.Count > 1)
            {
                return await SaveSyncRecordAsync(syncRecord, enrollment, EmployeeBenefitCardSyncStatuses.OnHold,
                    EmployeeBenefitCardSyncReasonCodes.MultipleMatch, null, null, attemptCount, now, triggerSource, actorUserId, transaction, cancellationToken);
            }

            var matchedPatientId = distinctPatientIds.First();

            // Pasang kunci advisory pada pasien untuk mencegah dua proses kartu paralel pada pasien yang sama
            var patientLockKey = $"PATIENT_CARD_SYNC:{matchedPatientId}";
            if (_dbContext.Database.IsNpgsql())
            {
                await _dbContext.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(hashtext({0}));", [patientLockKey], cancellationToken);
            }

            // 7. Evaluasi kartu penjamin internal aktif yang sudah ada pada pasien (FIN-DEC-251, V.8 Butir 7)
            var existingInternalCards = await _internalBenefitCardService.FindActiveInternalCardsAsync(
                matchedPatientId, cancellationToken);

            if (existingInternalCards.Count > 0)
            {
                // Cek apakah ada kartu aktif milik pemilik yang sama dan berelasi SELF
                var sameOwnerSelfCard = existingInternalCards.FirstOrDefault(x =>
                    x.BenefitOwnerWorkforceProfileId == owner.WorkforceProfileId &&
                    string.Equals(x.BenefitRelationship, BenefitRelationshipCodes.Self, StringComparison.OrdinalIgnoreCase));

                if (sameOwnerSelfCard != null)
                {
                    // Pakai ulang kartu yang sudah ada (SYNCED)
                    if (sameOwnerSelfCard.LastEligibilityReferenceNumber != enrollment.EnrollmentNumber)
                    {
                        sameOwnerSelfCard.LastEligibilityReferenceNumber = enrollment.EnrollmentNumber;
                        sameOwnerSelfCard.UpdateDateTime = now;
                        sameOwnerSelfCard.UpdateBy = actorUserId;
                        await _dbContext.SaveChangesAsync(cancellationToken);
                    }

                    return await SaveSyncRecordAsync(syncRecord, enrollment, EmployeeBenefitCardSyncStatuses.Synced,
                        null, matchedPatientId, sameOwnerSelfCard.Id, attemptCount, now, triggerSource, actorUserId, transaction, cancellationToken);
                }

                // Cek apakah ada kartu aktif milik pemilik lain
                var otherOwnerCard = existingInternalCards.FirstOrDefault(x =>
                    x.BenefitOwnerWorkforceProfileId.HasValue && x.BenefitOwnerWorkforceProfileId.Value != owner.WorkforceProfileId);

                if (otherOwnerCard != null)
                {
                    return await SaveSyncRecordAsync(syncRecord, enrollment, EmployeeBenefitCardSyncStatuses.OnHold,
                        EmployeeBenefitCardSyncReasonCodes.OtherInternalCardExists, null, null, attemptCount, now, triggerSource, actorUserId, transaction, cancellationToken);
                }

                // Kartu aktif tanpa tautan HR (legacy unlinked)
                return await SaveSyncRecordAsync(syncRecord, enrollment, EmployeeBenefitCardSyncStatuses.OnHold,
                    EmployeeBenefitCardSyncReasonCodes.LegacyUnlinkedCard, null, null, attemptCount, now, triggerSource, actorUserId, transaction, cancellationToken);
            }

            // 8. Buat kartu internal baru lewat InternalBenefitCardService.CreateSelfCardAsync (V.8 Butir 8)
            var internalGuarantor = await _internalBenefitCardService.FindOrProvisionInternalGuarantorAsync(
                actorUserId, cancellationToken);

            if (!internalGuarantor.IsActive || internalGuarantor.IsCancel)
            {
                return await SaveSyncRecordAsync(syncRecord, enrollment, EmployeeBenefitCardSyncStatuses.FailedRetryable,
                    EmployeeBenefitCardSyncReasonCodes.InternalGuarantorUnavailable, null, null, attemptCount, now, triggerSource, actorUserId, transaction, cancellationToken);
            }

            static string? Cut(string? val, int max) =>
                string.IsNullOrWhiteSpace(val) ? null : (val.Length <= max ? val.Trim() : val[..max].Trim());

            var createArgs = new CreateSelfBenefitCardArgs
            {
                PatientId = matchedPatientId,
                CompanyGuarantorId = internalGuarantor.Id,
                BenefitOwnerWorkforceProfileId = owner.WorkforceProfileId,
                EmployeeId = owner.EmployeeId,
                EmployeeNumber = Cut(eligibility.EmployeeNumber ?? owner.StaffNumber, 100) ?? string.Empty,
                EmployeeName = Cut(eligibility.EmployeeName ?? owner.StaffName, 200),
                DepartmentName = Cut(owner.OrganizationUnitName, 100),
                PositionName = Cut(owner.PositionName, 100),
                BenefitPlanCode = Cut(eligibility.PlanCode, 100),
                BenefitPlanName = Cut(eligibility.PlanName, 150),
                LastEligibilityReferenceNumber = Cut(enrollment.EnrollmentNumber, 100),
                EligibilityNote = "Dipasang otomatis dari sinkronisasi benefit aktif HR.",
                ForcePrimary = null, // IsPrimary dihitung otomatis: true hanya bila belum ada kartu primary aktif
                ActorUserId = actorUserId
            };

            var creationResult = await _internalBenefitCardService.CreateSelfCardAsync(createArgs, cancellationToken);
            if (!creationResult.IsSuccess || creationResult.Card == null)
            {
                return await SaveSyncRecordAsync(syncRecord, enrollment, EmployeeBenefitCardSyncStatuses.FailedRetryable,
                    EmployeeBenefitCardSyncReasonCodes.CardValidationRejected, null, null, attemptCount, now, triggerSource, actorUserId, transaction, cancellationToken);
            }

            return await SaveSyncRecordAsync(syncRecord, enrollment, EmployeeBenefitCardSyncStatuses.Synced,
                null, matchedPatientId, creationResult.Card.Id, attemptCount, now, triggerSource, actorUserId, transaction, cancellationToken);
        }

        // ====================================================================================
        // Penyimpanan Hasil dan Pembantu
        // ====================================================================================

        private async Task<PatientCardSyncSummaryResponse> SaveSyncRecordAsync(
            PatEmployeeBenefitCardSync? syncRecord,
            TrxEmployeeBenefitEnrollment enrollment,
            string syncStatus,
            string? reasonCode,
            Guid? patientId,
            Guid? patientCompanyGuarantorId,
            int attemptCount,
            DateTime now,
            string triggerSource,
            Guid actorUserId,
            IDbContextTransaction transaction,
            CancellationToken cancellationToken)
        {
            DateTime? nextAttemptAt = null;
            if (syncStatus == EmployeeBenefitCardSyncStatuses.WaitingPatient)
            {
                nextAttemptAt = now.AddDays(1);
            }
            else if (syncStatus == EmployeeBenefitCardSyncStatuses.FailedRetryable)
            {
                nextAttemptAt = attemptCount switch
                {
                    1 => now.AddMinutes(15),
                    2 => now.AddHours(1),
                    3 => now.AddHours(6),
                    _ => now.AddHours(24)
                };
            }

            DateTime? syncedAt = syncStatus == EmployeeBenefitCardSyncStatuses.Synced
                ? (syncRecord?.SyncedAt ?? now)
                : syncRecord?.SyncedAt;

            DateTime? revokedAt = syncStatus == EmployeeBenefitCardSyncStatuses.Revoked
                ? (syncRecord?.RevokedAt ?? now)
                : syncRecord?.RevokedAt;

            // Kepatuhan constraint CK_PatEmployeeBenefitCardSync_CardLink:
            // Hanya SYNCED dan REVOKED yang memiliki PatientId & PatientCompanyGuarantorId
            var validPatientId = (syncStatus is EmployeeBenefitCardSyncStatuses.Synced or EmployeeBenefitCardSyncStatuses.Revoked)
                ? patientId
                : null;

            var validCardId = (syncStatus is EmployeeBenefitCardSyncStatuses.Synced or EmployeeBenefitCardSyncStatuses.Revoked)
                ? patientCompanyGuarantorId
                : null;

            // Kepatuhan constraint CK_PatEmployeeBenefitCardSync_Reason:
            // SYNCED wajib memiliki reasonCode == null
            var validReasonCode = syncStatus == EmployeeBenefitCardSyncStatuses.Synced
                ? null
                : reasonCode;

            if (syncRecord == null)
            {
                syncRecord = new PatEmployeeBenefitCardSync
                {
                    Id = Guid.NewGuid(),
                    EmployeeBenefitEnrollmentId = enrollment.Id,
                    WorkforceProfileId = enrollment.WorkforceProfileId,
                    SyncStatus = syncStatus,
                    ReasonCode = validReasonCode,
                    AttemptCount = attemptCount,
                    LastAttemptAt = now,
                    NextAttemptAt = nextAttemptAt,
                    PatientId = validPatientId,
                    PatientCompanyGuarantorId = validCardId,
                    SyncedAt = syncedAt,
                    RevokedAt = revokedAt,
                    LastTriggerSource = triggerSource,
                    LastTriggeredByUserId = actorUserId,
                    RowVersion = Guid.NewGuid(),
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };
                _dbContext.PatEmployeeBenefitCardSyncs.Add(syncRecord);
            }
            else
            {
                syncRecord.SyncStatus = syncStatus;
                syncRecord.ReasonCode = validReasonCode;
                syncRecord.AttemptCount = attemptCount;
                syncRecord.LastAttemptAt = now;
                syncRecord.NextAttemptAt = nextAttemptAt;
                syncRecord.PatientId = validPatientId;
                syncRecord.PatientCompanyGuarantorId = validCardId;
                syncRecord.SyncedAt = syncedAt;
                syncRecord.RevokedAt = revokedAt;
                syncRecord.LastTriggerSource = triggerSource;
                syncRecord.LastTriggeredByUserId = actorUserId;
                syncRecord.RowVersion = Guid.NewGuid();
                syncRecord.UpdateDateTime = now;
                syncRecord.UpdateBy = actorUserId;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "EmployeeBenefitCardSyncService.SaveSyncRecord",
                $"Catatan sinkronisasi benefit enrollment {enrollment.EnrollmentNumber}: {syncStatus} ({validReasonCode ?? "OK"})",
                new { enrollment.Id, syncStatus, ReasonCode = validReasonCode, attemptCount, syncRecord.PatientCompanyGuarantorId });

            return new PatientCardSyncSummaryResponse
            {
                Status = syncStatus,
                ReasonCode = validReasonCode,
                ReasonLabel = GetReasonLabel(validReasonCode),
                AttemptCount = attemptCount,
                LastAttemptAt = now,
                NextAttemptAt = nextAttemptAt,
                SyncedAt = syncedAt
            };
        }

        private static string NormalizeDigits(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            return new string(value.Where(char.IsDigit).ToArray());
        }

        private static bool IsValidIdentityType(string? identityType)
        {
            if (string.IsNullOrWhiteSpace(identityType)) return false;
            var trimmed = identityType.Trim();
            return string.Equals(trimmed, "KTP", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(trimmed, "NIK", StringComparison.OrdinalIgnoreCase);
        }

        private static string? GetReasonLabel(string? reasonCode) => reasonCode switch
        {
            EmployeeBenefitCardSyncReasonCodes.NoMatch => "Data pasien belum ditemukan di SIMRS.",
            EmployeeBenefitCardSyncReasonCodes.IdentityTypeNotAllowed => "Jenis tanda pengenal pasien bukan KTP/NIK.",
            EmployeeBenefitCardSyncReasonCodes.IdentityDataIncomplete => "Data identitas pegawai di HR belum lengkap.",
            EmployeeBenefitCardSyncReasonCodes.MultipleMatch => "Ditemukan lebih dari satu pasien dengan identitas yang sama.",
            EmployeeBenefitCardSyncReasonCodes.OtherInternalCardExists => "Pasien sudah memiliki kartu penjamin internal untuk pemilik lain.",
            EmployeeBenefitCardSyncReasonCodes.LegacyUnlinkedCard => "Pasien memiliki kartu internal lama tanpa tautan HR.",
            EmployeeBenefitCardSyncReasonCodes.HrUnavailable => "Layanan data HR tidak dapat diakses sementara.",
            EmployeeBenefitCardSyncReasonCodes.InternalGuarantorUnavailable => "Penjamin internal sistem tidak aktif atau kedaluwarsa.",
            EmployeeBenefitCardSyncReasonCodes.CardValidationRejected => "Validasi pembuatan kartu penjamin internal ditolak.",
            EmployeeBenefitCardSyncReasonCodes.UnexpectedError => "Terjadi kesalahan internal saat sinkronisasi.",
            EmployeeBenefitCardSyncReasonCodes.CardRevokedByStaff => "Kartu penjamin telah dinonaktifkan oleh petugas.",
            EmployeeBenefitCardSyncReasonCodes.CardDeleted => "Kartu penjamin telah dihapus.",
            _ => reasonCode
        };
    }
}
