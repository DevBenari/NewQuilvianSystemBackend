using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;

public sealed class InpBedReadinessService(ApplicationDbContext db, InpBedMutationCoordinator coordinator,
    InpBedAccessService access, InpBedManagementQueryService queries)
{
    public async Task<BedOperationResponse> StartAsync(Guid bedId, StartBedCleaningRequest request, CancellationToken ct)
        => await MutateAsync(bedId, "StartCleaning", request, null, ct);
    public async Task<BedOperationResponse> CompleteAsync(Guid attemptId, CompleteBedCleaningRequest request, CancellationToken ct)
    {
        var bedId = await db.Set<InpBedCleaningAttempt>().AsNoTracking().Where(x => x.Id == attemptId)
            .Join(db.Set<InpBedReadiness>(), a => a.ReadinessId, r => r.Id, (a,r) => (Guid?)r.BedId).SingleOrDefaultAsync(ct);
        if (!bedId.HasValue) throw new InpBedOperationException(404, "NOT_FOUND", "Upaya pembersihan tidak ditemukan.");
        return await MutateAsync(bedId.Value, "CompleteCleaning", request, attemptId, ct);
    }
    public async Task<BedOperationResponse> VerifyAsync(Guid bedId, VerifyBedReadinessRequest request, CancellationToken ct)
        => await MutateAsync(bedId, "VerifyReadiness", request, request.AttemptId, ct);
    private async Task<BedOperationResponse> MutateAsync(Guid bedId, string action, BedCleaningRequest request, Guid? attemptId, CancellationToken ct)
    {
        var operation = "InpatientBedManagement:" + action;
        await access.RequireBedAsync(bedId, operation, ct);
        var meta = await coordinator.ExecuteAsync(operation, new { bedId, attemptId, request }, [],
            [bedId], [new() { BedId = bedId, ExpectedVersion = request.ExpectedBedVersion }], async token =>
            {
                var bed = await db.Set<MstBed>().Include(x => x.Room).SingleAsync(x => x.Id == bedId, token);
                var assignment = access.WorkflowAssignment(bed.Room!.ServiceUnitId, operation);
                await coordinator.RequireEmptyAsync(bedId, token);
                var r = await coordinator.RootAsync(bedId, access.ActorId, token);
                if (request.CycleId == Guid.Empty || r.CycleId != request.CycleId)
                    throw new InpBedOperationException(409, "CLEANING_CYCLE_STALE", "Siklus bed berubah. Muat ulang data.");
                var operational = await queries.ProjectAsync(bed, token);
                if (operational.BlockReasonCodes.Contains("MASTER_INVALID") || !Enum.IsDefined(bed.BedStatus) ||
                    bed.BedStatus is BedStatus.Maintenance or BedStatus.Blocked or BedStatus.Inactive or BedStatus.Unknown)
                    throw new InpBedOperationException(422, "BED_UNAVAILABLE", "Bed atau hierarchy sedang ditutup.");
                var actor = access.ActorId; var now = DateTime.UtcNow;
                InpBedCleaningAttempt? attempt = attemptId.HasValue
                    ? await db.Set<InpBedCleaningAttempt>().SingleOrDefaultAsync(x => x.Id == attemptId && x.ReadinessId == r.Id && x.CycleId == r.CycleId, token)
                    : null;
                if (attemptId.HasValue && attempt == null)
                    throw new InpBedOperationException(409, "CLEANING_CYCLE_STALE", "Upaya bukan bagian dari siklus berjalan.");
                if (action == "StartCleaning")
                {
                    if (r.State != InpBedReadinessState.WaitingCleaning)
                        throw new InpBedOperationException(409, "CLEANING_CYCLE_STALE", "Bed belum menunggu pembersihan.");
                    attempt = new() { ReadinessId = r.Id, CycleId = r.CycleId, State = InpBedCleaningAttemptState.Started,
                        StartedAtUtc = now, StartedByUserId = actor, SopReference = assignment.SopReference!, CreateBy = actor };
                    db.Set<InpBedCleaningAttempt>().Add(attempt); r.State = InpBedReadinessState.Cleaning;
                }
                else if (action == "CompleteCleaning")
                {
                    if (r.State != InpBedReadinessState.Cleaning || attempt?.State != InpBedCleaningAttemptState.Started)
                        throw new InpBedOperationException(409, "CLEANING_CYCLE_STALE", "Upaya sudah selesai atau tidak sedang dikerjakan.");
                    attempt.State = InpBedCleaningAttemptState.AwaitingVerification; attempt.CompletedAtUtc = now; attempt.CompletedByUserId = actor;
                    attempt.UpdateBy = actor; attempt.UpdateDateTime = now; r.State = InpBedReadinessState.AwaitingVerification;
                }
                else
                {
                    var verify = (VerifyBedReadinessRequest)request;
                    if (string.IsNullOrWhiteSpace(verify.EvidenceReference) || verify.EvidenceReference.Trim().Length > 200 ||
                        verify.Reason?.Length > 500 || (!verify.IsReady && string.IsNullOrWhiteSpace(verify.Reason)))
                        throw new InpBedOperationException(422, "REASON_REQUIRED", "Bukti wajib; penolakan juga wajib beralasan.");
                    var initial = r.State == InpBedReadinessState.Unverified && attemptId == null;
                    // Legacy used bed tanpa root tidak boleh menerima verifikasi awal langsung.
                    var latestUse = await db.Set<InpBedPlacement>().Where(x => x.BedId == bedId && !x.IsDelete).OrderByDescending(x => x.EndDateTime).Select(x => x.EndDateTime).FirstOrDefaultAsync(token);
                    var lastAccepted = await db.Set<InpBedCleaningAttempt>().Where(x => x.ReadinessId == r.Id && x.State == InpBedCleaningAttemptState.Accepted).OrderByDescending(x => x.VerifiedAtUtc).Select(x => x.VerifiedAtUtc).FirstOrDefaultAsync(token);
                    if (initial && latestUse.HasValue && (!lastAccepted.HasValue || lastAccepted < latestUse))
                        throw new InpBedOperationException(422, "BED_NOT_READY", "Ada bukti pemakaian lama; lakukan backfill used-dirty dan pembersihan.");
                    if (!initial && (r.State != InpBedReadinessState.AwaitingVerification || attempt?.State != InpBedCleaningAttemptState.AwaitingVerification))
                        throw new InpBedOperationException(409, "CLEANING_CYCLE_STALE", "Pembersihan siklus ini belum menunggu verifikasi.");
                    if (attempt != null)
                    {
                        attempt.State = verify.IsReady ? InpBedCleaningAttemptState.Accepted : InpBedCleaningAttemptState.Rejected;
                        attempt.VerifiedAtUtc = now; attempt.VerifiedByUserId = actor; attempt.RejectionReason = verify.IsReady ? null : verify.Reason!.Trim();
                        attempt.UpdateBy = actor; attempt.UpdateDateTime = now;
                    }
                    r.State = verify.IsReady ? InpBedReadinessState.Ready : InpBedReadinessState.WaitingCleaning;
                    r.VerifiedAtUtc = verify.IsReady ? now : null; r.VerifiedByUserId = verify.IsReady ? actor : null;
                    r.VerificationReference = verify.IsReady ? verify.EvidenceReference.Trim() : null;
                }
                bed.BedStatus = r.State == InpBedReadinessState.Ready ? BedStatus.Available : BedStatus.Cleaning;
                bed.UpdateBy = actor; bed.UpdateDateTime = now;
                return attempt == null ? ("Bed", bedId) : ("CleaningAttempt", attempt.Id);
            }, ct);
        var currentBed = await db.Set<MstBed>().AsNoTracking().SingleAsync(x => x.Id == bedId, ct);
        var current = await queries.ProjectAsync(currentBed, ct);
        current.HoldingContext = null;
        return new() { Bed = current,
            AttemptId = meta.ResultKind == "CleaningAttempt" ? meta.ResultEntityId : null, OperationMeta = meta };
    }
    public async Task<PagedResult<BedCleaningAttemptResponse>> AttemptsAsync(Guid bedId, int pageNumber, int pageSize, CancellationToken ct)
    {
        await access.RequireBedAsync(bedId, "InpatientBedManagement:Read", ct);
        var source = db.Set<InpBedCleaningAttempt>().AsNoTracking().Join(db.Set<InpBedReadiness>().Where(x => x.BedId == bedId),
            a => a.ReadinessId, r => r.Id, (a,r) => a);
        var total = await source.CountAsync(ct); var page = Math.Max(1, pageNumber); var size = Math.Clamp(pageSize, 1, 100);
        var rows = await source.OrderByDescending(x => x.StartedAtUtc).ThenByDescending(x => x.Id).Skip((page - 1) * size).Take(size).ToListAsync(ct);
        return new() { PageNumber = page, PageSize = size, TotalData = total, TotalPage = (int)Math.Ceiling(total / (double)size),
            Items = rows.Select(x => new BedCleaningAttemptResponse { Id = x.Id, CycleId = x.CycleId, State = x.State.ToString(),
                StartedAtUtc = x.StartedAtUtc, StartedByUserId = x.StartedByUserId, CompletedAtUtc = x.CompletedAtUtc,
                CompletedByUserId = x.CompletedByUserId, VerifiedAtUtc = x.VerifiedAtUtc, VerifiedByUserId = x.VerifiedByUserId,
                RejectionReason = x.RejectionReason, SopReference = x.SopReference }).ToList() };
    }
}

