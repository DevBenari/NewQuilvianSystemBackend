using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models;
using QuilvianSystemBackend.Repositories;
using System.Data;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services
{
    public class OvertimePeriodService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly OvertimePeriodQueryService _queryService;
        private readonly OvertimeFinalReconciliationService _reconciliationService;

        public OvertimePeriodService(
            ApplicationDbContext dbContext,
            OvertimePeriodQueryService queryService,
            OvertimeFinalReconciliationService reconciliationService)
        {
            _dbContext = dbContext;
            _queryService = queryService;
            _reconciliationService = reconciliationService;
        }

        public async Task<OvertimeClosingServiceResult<OvertimePeriodDetailResponse>> CreateAsync(
            CreateOvertimePeriodRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var error = await ValidateRequestAsync(null, request, cancellationToken);
            if (error != null) return Fail<OvertimePeriodDetailResponse>(StatusCodes.Status400BadRequest, error);

            var now = DateTime.UtcNow;
            var entity = new TrxOvertimePeriod
            {
                Id = Guid.NewGuid(),
                PeriodCode = request.PeriodCode.Trim(),
                PeriodName = request.PeriodName.Trim(),
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                LegalEntityId = NormalizeGuid(request.LegalEntityId),
                HospitalSiteId = NormalizeGuid(request.HospitalSiteId),
                OrganizationUnitId = NormalizeGuid(request.OrganizationUnitId),
                DepartmentId = NormalizeGuid(request.DepartmentId),
                PeriodStatus = OvertimeValueConstants.PeriodStatus.Open,
                RequireAttendanceFinal = request.RequireAttendanceFinal,
                RequireVerificationComplete = request.RequireVerificationComplete,
                RequireSettlementComplete = request.RequireSettlementComplete,
                ScheduledCloseAt = NormalizeUtc(request.ScheduledCloseAt),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                UpdateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<TrxOvertimePeriod>().Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
            var detail = await _queryService.GetDetailAsync(entity.Id, cancellationToken);
            return OvertimeClosingServiceResult<OvertimePeriodDetailResponse>.Ok(
                detail!,
                "Overtime period berhasil dibuat.",
                StatusCodes.Status201Created);
        }

        public async Task<OvertimeClosingServiceResult<OvertimePeriodDetailResponse>> UpdateAsync(
            Guid id,
            UpdateOvertimePeriodRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxOvertimePeriod>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null) return Fail<OvertimePeriodDetailResponse>(StatusCodes.Status404NotFound, "Overtime period tidak ditemukan.");
            if (!IsEditable(entity.PeriodStatus)) return Fail<OvertimePeriodDetailResponse>(StatusCodes.Status409Conflict, "Period Closed, Closing, atau Cancelled tidak dapat diubah.");

            var error = await ValidateRequestAsync(id, request, cancellationToken);
            if (error != null) return Fail<OvertimePeriodDetailResponse>(StatusCodes.Status400BadRequest, error);

            entity.PeriodCode = request.PeriodCode.Trim();
            entity.PeriodName = request.PeriodName.Trim();
            entity.StartDate = request.StartDate;
            entity.EndDate = request.EndDate;
            entity.LegalEntityId = NormalizeGuid(request.LegalEntityId);
            entity.HospitalSiteId = NormalizeGuid(request.HospitalSiteId);
            entity.OrganizationUnitId = NormalizeGuid(request.OrganizationUnitId);
            entity.DepartmentId = NormalizeGuid(request.DepartmentId);
            entity.RequireAttendanceFinal = request.RequireAttendanceFinal;
            entity.RequireVerificationComplete = request.RequireVerificationComplete;
            entity.RequireSettlementComplete = request.RequireSettlementComplete;
            entity.ScheduledCloseAt = NormalizeUtc(request.ScheduledCloseAt);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            var detail = await _queryService.GetDetailAsync(entity.Id, cancellationToken);
            return OvertimeClosingServiceResult<OvertimePeriodDetailResponse>.Ok(detail!, "Overtime period berhasil diperbarui.");
        }

        public async Task<OvertimeClosingServiceResult<OvertimePeriodActionResponse>> DeleteAsync(
            Guid id,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxOvertimePeriod>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null) return Fail<OvertimePeriodActionResponse>(StatusCodes.Status404NotFound, "Overtime period tidak ditemukan.");
            if (entity.PeriodStatus != OvertimeValueConstants.PeriodStatus.Open)
                return Fail<OvertimePeriodActionResponse>(StatusCodes.Status409Conflict, "Hanya period Open yang dapat dihapus.");

            var hasRequest = await ApplyPeriodScope(
                    _dbContext.WfpOvertimeRequests.AsNoTracking().Where(x => !x.IsDelete),
                    entity)
                .AnyAsync(cancellationToken);
            if (hasRequest) return Fail<OvertimePeriodActionResponse>(StatusCodes.Status409Conflict, "Period sudah memiliki transaksi Overtime dan tidak dapat dihapus.");

            var now = DateTime.UtcNow;
            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return OvertimeClosingServiceResult<OvertimePeriodActionResponse>.Ok(
                MapAction(entity, OvertimeValueConstants.PeriodStatus.Open, false, now, null),
                "Overtime period berhasil dihapus.");
        }

        public async Task<OvertimeClosingServiceResult<OvertimeFinalReconciliationResponse>> ValidateAsync(
            Guid id,
            ValidateOvertimePeriodRequest? request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            request ??= new ValidateOvertimePeriodRequest();
            var entity = await _dbContext.Set<TrxOvertimePeriod>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null) return Fail<OvertimeFinalReconciliationResponse>(StatusCodes.Status404NotFound, "Overtime period tidak ditemukan.");

            var result = await _reconciliationService.ReconcileAsync(
                ToReconciliationRequest(entity, request.AllowRepair, request.VerificationOverdueHours),
                actorUserId,
                cancellationToken);
            var now = DateTime.UtcNow;
            entity.LastValidatedAt = now;
            entity.LastReconciledAt = now;
            entity.ValidationSnapshotJson = _reconciliationService.SerializeSnapshot(result);
            entity.ReconciliationSnapshotJson = entity.ValidationSnapshotJson;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return OvertimeClosingServiceResult<OvertimeFinalReconciliationResponse>.Ok(
                result,
                result.IsCloseReady
                    ? "Overtime period valid dan siap ditutup."
                    : "Validasi selesai dan masih menemukan blocking issue.");
        }

        public async Task<OvertimeClosingServiceResult<OvertimePeriodActionResponse>> CloseAsync(
            Guid id,
            CloseOvertimePeriodRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (actorUserId == Guid.Empty) return Fail<OvertimePeriodActionResponse>(StatusCodes.Status401Unauthorized, "Identitas user login tidak valid.");
            if (string.IsNullOrWhiteSpace(request.Reason)) return Fail<OvertimePeriodActionResponse>(StatusCodes.Status400BadRequest, "Alasan closing wajib diisi.");

            var initialPeriod = await _dbContext.Set<TrxOvertimePeriod>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (initialPeriod == null)
                return Fail<OvertimePeriodActionResponse>(StatusCodes.Status404NotFound, "Overtime period tidak ditemukan.");
            if (initialPeriod.PeriodStatus == OvertimeValueConstants.PeriodStatus.Closed)
                return OvertimeClosingServiceResult<OvertimePeriodActionResponse>.Ok(
                    MapAction(initialPeriod, initialPeriod.PeriodStatus, request.ForceClose, DateTime.UtcNow, null),
                    "Overtime period sudah Closed.");
            if (initialPeriod.PeriodStatus != OvertimeValueConstants.PeriodStatus.Open &&
                initialPeriod.PeriodStatus != OvertimeValueConstants.PeriodStatus.Reopened &&
                initialPeriod.PeriodStatus != OvertimeValueConstants.PeriodStatus.Closing)
            {
                return Fail<OvertimePeriodActionResponse>(StatusCodes.Status409Conflict, "Overtime period tidak dapat ditutup dari status saat ini.");
            }

            // Safe repair dijalankan saat period masih writable. Setelah itu status dipindah ke
            // Closing dan disimpan agar seluruh mutation Overtime langsung diblokir oleh period guard.
            if (request.AllowRepair && initialPeriod.PeriodStatus != OvertimeValueConstants.PeriodStatus.Closing)
            {
                await _reconciliationService.ReconcileAsync(
                    ToReconciliationRequest(initialPeriod, true, 24),
                    actorUserId,
                    cancellationToken);
            }

            var previousWritableStatus = initialPeriod.PeriodStatus == OvertimeValueConstants.PeriodStatus.Closing
                ? ResolveWritableStatus(initialPeriod)
                : initialPeriod.PeriodStatus;

            await using (var markClosingTransaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken))
            {
                try
                {
                    await AcquireLockAsync("OVERTIME-PERIOD-CLOSE-" + id, cancellationToken);
                    var periodToMark = await _dbContext.Set<TrxOvertimePeriod>()
                        .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
                    if (periodToMark == null)
                    {
                        await markClosingTransaction.RollbackAsync(cancellationToken);
                        return Fail<OvertimePeriodActionResponse>(StatusCodes.Status404NotFound, "Overtime period tidak ditemukan.");
                    }
                    if (periodToMark.PeriodStatus == OvertimeValueConstants.PeriodStatus.Closed)
                    {
                        await markClosingTransaction.CommitAsync(cancellationToken);
                        return OvertimeClosingServiceResult<OvertimePeriodActionResponse>.Ok(
                            MapAction(periodToMark, periodToMark.PeriodStatus, request.ForceClose, DateTime.UtcNow, null),
                            "Overtime period sudah Closed.");
                    }
                    if (periodToMark.PeriodStatus != OvertimeValueConstants.PeriodStatus.Open &&
                        periodToMark.PeriodStatus != OvertimeValueConstants.PeriodStatus.Reopened &&
                        periodToMark.PeriodStatus != OvertimeValueConstants.PeriodStatus.Closing)
                    {
                        await markClosingTransaction.RollbackAsync(cancellationToken);
                        return Fail<OvertimePeriodActionResponse>(StatusCodes.Status409Conflict, "Overtime period tidak dapat ditutup dari status saat ini.");
                    }

                    if (periodToMark.PeriodStatus != OvertimeValueConstants.PeriodStatus.Closing)
                    {
                        previousWritableStatus = periodToMark.PeriodStatus;
                        periodToMark.PeriodStatus = OvertimeValueConstants.PeriodStatus.Closing;
                        periodToMark.UpdateDateTime = DateTime.UtcNow;
                        periodToMark.UpdateBy = actorUserId;
                        await _dbContext.SaveChangesAsync(cancellationToken);
                    }

                    await markClosingTransaction.CommitAsync(cancellationToken);
                }
                catch
                {
                    await markClosingTransaction.RollbackAsync(cancellationToken);
                    throw;
                }
            }

            // Reconciliation final berjalan saat status Closing sudah committed sehingga mutation baru
            // pada tanggal/scope period ini akan ditolak oleh OvertimePeriodGuardService.
            var closingPeriod = await _dbContext.Set<TrxOvertimePeriod>()
                .AsNoTracking()
                .FirstAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            var reconciliation = await _reconciliationService.ReconcileAsync(
                ToReconciliationRequest(closingPeriod, false, 24),
                actorUserId,
                cancellationToken);

            await using var finalizeTransaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);
            try
            {
                await AcquireLockAsync("OVERTIME-PERIOD-CLOSE-" + id, cancellationToken);
                var entity = await _dbContext.Set<TrxOvertimePeriod>()
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
                if (entity == null)
                {
                    await finalizeTransaction.RollbackAsync(cancellationToken);
                    return Fail<OvertimePeriodActionResponse>(StatusCodes.Status404NotFound, "Overtime period tidak ditemukan.");
                }
                if (entity.PeriodStatus == OvertimeValueConstants.PeriodStatus.Closed)
                {
                    await finalizeTransaction.CommitAsync(cancellationToken);
                    return OvertimeClosingServiceResult<OvertimePeriodActionResponse>.Ok(
                        MapAction(entity, OvertimeValueConstants.PeriodStatus.Closing, request.ForceClose, DateTime.UtcNow, reconciliation),
                        "Overtime period sudah Closed.");
                }
                if (entity.PeriodStatus != OvertimeValueConstants.PeriodStatus.Closing)
                {
                    await finalizeTransaction.RollbackAsync(cancellationToken);
                    return Fail<OvertimePeriodActionResponse>(StatusCodes.Status409Conflict, "Status overtime period berubah selama proses closing.");
                }

                var now = DateTime.UtcNow;
                entity.LastValidatedAt = now;
                entity.LastReconciledAt = now;
                entity.ValidationSnapshotJson = _reconciliationService.SerializeSnapshot(reconciliation);
                entity.ReconciliationSnapshotJson = entity.ValidationSnapshotJson;

                if (!reconciliation.IsCloseReady && !request.ForceClose)
                {
                    entity.PeriodStatus = previousWritableStatus;
                    entity.UpdateDateTime = now;
                    entity.UpdateBy = actorUserId;
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    await finalizeTransaction.CommitAsync(cancellationToken);
                    return Fail<OvertimePeriodActionResponse>(
                        StatusCodes.Status409Conflict,
                        $"Overtime period belum dapat ditutup karena terdapat {reconciliation.BlockingCount} blocking issue. Status period dikembalikan ke {previousWritableStatus}.");
                }

                entity.PeriodStatus = OvertimeValueConstants.PeriodStatus.Closed;
                entity.ClosedAt = now;
                entity.ClosedByUserId = actorUserId;
                entity.CloseReason = request.Reason.Trim();
                entity.CloseVersion += 1;
                entity.ScheduledCloseAt = null;
                entity.UpdateDateTime = now;
                entity.UpdateBy = actorUserId;
                await _dbContext.SaveChangesAsync(cancellationToken);
                await finalizeTransaction.CommitAsync(cancellationToken);

                return OvertimeClosingServiceResult<OvertimePeriodActionResponse>.Ok(
                    MapAction(entity, previousWritableStatus, request.ForceClose, now, reconciliation),
                    request.ForceClose && !reconciliation.IsCloseReady
                        ? "Overtime period ditutup secara paksa dengan blocking issue yang tercatat pada snapshot."
                        : "Overtime period berhasil ditutup.");
            }
            catch
            {
                await finalizeTransaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<OvertimeClosingServiceResult<OvertimePeriodActionResponse>> ReopenAsync(
            Guid id,
            ReopenOvertimePeriodRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (actorUserId == Guid.Empty) return Fail<OvertimePeriodActionResponse>(StatusCodes.Status401Unauthorized, "Identitas user login tidak valid.");
            if (string.IsNullOrWhiteSpace(request.Reason)) return Fail<OvertimePeriodActionResponse>(StatusCodes.Status400BadRequest, "Alasan reopen wajib diisi.");

            var entity = await _dbContext.Set<TrxOvertimePeriod>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null) return Fail<OvertimePeriodActionResponse>(StatusCodes.Status404NotFound, "Overtime period tidak ditemukan.");
            if (entity.PeriodStatus != OvertimeValueConstants.PeriodStatus.Closed &&
                entity.PeriodStatus != OvertimeValueConstants.PeriodStatus.Closing)
            {
                return Fail<OvertimePeriodActionResponse>(StatusCodes.Status409Conflict, "Hanya overtime period Closed atau Closing yang dapat dibuka kembali.");
            }

            var previous = entity.PeriodStatus;
            var now = DateTime.UtcNow;
            entity.PeriodStatus = OvertimeValueConstants.PeriodStatus.Reopened;
            entity.ReopenedAt = now;
            entity.ReopenedByUserId = actorUserId;
            entity.ReopenReason = request.Reason.Trim();
            entity.ReopenCount += 1;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return OvertimeClosingServiceResult<OvertimePeriodActionResponse>.Ok(
                MapAction(entity, previous, false, now, null),
                "Overtime period berhasil dibuka kembali.");
        }

        public async Task<OvertimeClosingServiceResult<OvertimePeriodActionResponse>> CancelAsync(
            Guid id,
            CancelOvertimePeriodRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.Reason)) return Fail<OvertimePeriodActionResponse>(StatusCodes.Status400BadRequest, "Alasan pembatalan wajib diisi.");
            var entity = await _dbContext.Set<TrxOvertimePeriod>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null) return Fail<OvertimePeriodActionResponse>(StatusCodes.Status404NotFound, "Overtime period tidak ditemukan.");
            if (!IsEditable(entity.PeriodStatus)) return Fail<OvertimePeriodActionResponse>(StatusCodes.Status409Conflict, "Overtime period tidak dapat dibatalkan dari status saat ini.");

            var previous = entity.PeriodStatus;
            var now = DateTime.UtcNow;
            entity.PeriodStatus = OvertimeValueConstants.PeriodStatus.Cancelled;
            entity.IsCancel = true;
            entity.IsActive = false;
            entity.CancelDateTime = now;
            entity.CancelBy = actorUserId;
            entity.CloseReason = request.Reason.Trim();
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return OvertimeClosingServiceResult<OvertimePeriodActionResponse>.Ok(
                MapAction(entity, previous, false, now, null),
                "Overtime period berhasil dibatalkan.");
        }

        private async Task<string?> ValidateRequestAsync(
            Guid? id,
            CreateOvertimePeriodRequest request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.PeriodCode)) return "Kode period wajib diisi.";
            if (string.IsNullOrWhiteSpace(request.PeriodName)) return "Nama period wajib diisi.";
            if (request.EndDate < request.StartDate) return "Tanggal akhir tidak boleh lebih kecil dari tanggal mulai.";
            if (request.EndDate.DayNumber - request.StartDate.DayNumber + 1 > 366) return "Rentang satu overtime period maksimal 366 hari.";

            var normalizedCode = request.PeriodCode.Trim().ToLower();
            if (await _dbContext.Set<TrxOvertimePeriod>().AsNoTracking().AnyAsync(x =>
                !x.IsDelete && x.Id != id && x.PeriodCode.ToLower() == normalizedCode, cancellationToken))
                return "Kode overtime period sudah digunakan.";

            var legal = NormalizeGuid(request.LegalEntityId);
            var site = NormalizeGuid(request.HospitalSiteId);
            var unit = NormalizeGuid(request.OrganizationUnitId);
            var dept = NormalizeGuid(request.DepartmentId);
            var overlap = await _dbContext.Set<TrxOvertimePeriod>().AsNoTracking().AnyAsync(x =>
                !x.IsDelete && !x.IsCancel && x.Id != id &&
                x.LegalEntityId == legal && x.HospitalSiteId == site && x.OrganizationUnitId == unit && x.DepartmentId == dept &&
                x.StartDate <= request.EndDate && request.StartDate <= x.EndDate,
                cancellationToken);
            if (overlap) return "Terdapat overtime period lain dengan scope dan rentang tanggal yang overlap.";

            if (legal.HasValue && !await _dbContext.MstLegalEntities.AnyAsync(x => x.Id == legal && x.IsActive && !x.IsDelete && !x.IsCancel, cancellationToken)) return "Legal entity tidak ditemukan atau tidak aktif.";
            if (site.HasValue && !await _dbContext.MstHospitalSites.AnyAsync(x => x.Id == site && x.IsActive && !x.IsDelete && !x.IsCancel, cancellationToken)) return "Hospital site tidak ditemukan atau tidak aktif.";
            if (unit.HasValue && !await _dbContext.MstOrganizationUnits.AnyAsync(x => x.Id == unit && x.IsActive && !x.IsDelete && !x.IsCancel, cancellationToken)) return "Organization unit tidak ditemukan atau tidak aktif.";
            if (dept.HasValue && !await _dbContext.MstDepartments.AnyAsync(x => x.Id == dept && x.IsActive && !x.IsDelete && !x.IsCancel, cancellationToken)) return "Department tidak ditemukan atau tidak aktif.";
            return null;
        }

        private static OvertimeReconciliationRequest ToReconciliationRequest(
            TrxOvertimePeriod period,
            bool allowRepair,
            int overdueHours) => new()
        {
            OvertimePeriodId = period.Id,
            StartDate = period.StartDate,
            EndDate = period.EndDate,
            LegalEntityId = period.LegalEntityId,
            HospitalSiteId = period.HospitalSiteId,
            OrganizationUnitId = period.OrganizationUnitId,
            DepartmentId = period.DepartmentId,
            AllowRepair = allowRepair,
            VerificationOverdueHours = overdueHours
        };

        private static IQueryable<WfpOvertimeRequest> ApplyPeriodScope(
            IQueryable<WfpOvertimeRequest> query,
            TrxOvertimePeriod period)
        {
            query = query.Where(x => x.OvertimeDate >= period.StartDate && x.OvertimeDate <= period.EndDate);
            if (period.HospitalSiteId.HasValue) query = query.Where(x => x.HospitalSiteId == period.HospitalSiteId);
            if (period.OrganizationUnitId.HasValue) query = query.Where(x => x.OrganizationUnitId == period.OrganizationUnitId);
            if (period.DepartmentId.HasValue) query = query.Where(x => x.DepartmentId == period.DepartmentId);
            if (period.LegalEntityId.HasValue) query = query.Where(x => x.HospitalSite != null && x.HospitalSite.LegalEntityId == period.LegalEntityId);
            return query;
        }

        private async Task AcquireLockAsync(string key, CancellationToken cancellationToken) =>
            await _dbContext.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock(hashtext({key}))", cancellationToken);

        private static string ResolveWritableStatus(TrxOvertimePeriod period) =>
            period.ReopenCount > 0 && period.ReopenedAt.HasValue
                ? OvertimeValueConstants.PeriodStatus.Reopened
                : OvertimeValueConstants.PeriodStatus.Open;

        private static bool IsEditable(string status) =>
            status == OvertimeValueConstants.PeriodStatus.Open ||
            status == OvertimeValueConstants.PeriodStatus.Reopened;

        private static Guid? NormalizeGuid(Guid? value) => value.HasValue && value.Value != Guid.Empty ? value.Value : null;
        private static DateTime? NormalizeUtc(DateTime? value) => value.HasValue
            ? value.Value.Kind == DateTimeKind.Utc ? value.Value : value.Value.ToUniversalTime()
            : null;

        private static OvertimePeriodActionResponse MapAction(
            TrxOvertimePeriod entity,
            string previous,
            bool forced,
            DateTime actionAt,
            OvertimeFinalReconciliationResponse? reconciliation) => new()
        {
            OvertimePeriodId = entity.Id,
            PeriodCode = entity.PeriodCode,
            PreviousStatus = previous,
            CurrentStatus = entity.PeriodStatus,
            CloseVersion = entity.CloseVersion,
            ReopenCount = entity.ReopenCount,
            WasForced = forced,
            ActionAt = actionAt,
            Reconciliation = reconciliation
        };

        private static OvertimeClosingServiceResult<T> Fail<T>(int statusCode, string message) =>
            OvertimeClosingServiceResult<T>.Fail(statusCode, message);
    }
}
