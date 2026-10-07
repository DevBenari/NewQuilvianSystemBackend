using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;

public class CliWsdObservationService(ApplicationDbContext db, NursingEpisodeWriteGuard guard,
    DailyMonitoringService fluids, LoggerService logger)
{
    public static decimal ComputeIncreaseMl(decimal previousResidualMl, decimal currentResidualMl, decimal discardedMl)
        => currentResidualMl + discardedMl - previousResidualMl;
    private static NursingResult<T> Fail<T>(int code, string text, string? reason = null) => NursingResult<T>.Fail(code, text, reason);
    public async Task<NursingResult<List<WsdDrainResponse>>> ListAsync(Guid episodeId, bool includeRemoved, CancellationToken ct)
    {
        var drains = await db.Set<CliWsdDrain>().AsNoTracking().Where(x => x.InpEpisodeId == episodeId && !x.IsDelete &&
            (includeRemoved || x.Status == CliWsdDrainStatus.Active)).OrderBy(x => x.InsertedAt).ToListAsync(ct);
        var ids = drains.Select(x => x.Id).ToList();
        var readings = await db.Set<CliWsdReading>().AsNoTracking().Where(x => ids.Contains(x.WsdDrainId) && !x.IsDelete && x.Status == ClinicalMeasurementStatus.Active).ToListAsync(ct);
        return NursingResult<List<WsdDrainResponse>>.Ok(drains.Select(x => Drain(x, readings.Where(r => r.WsdDrainId == x.Id).OrderByDescending(r => r.PeriodEndAt).FirstOrDefault())).ToList(), "Selang WSD berhasil diambil.");
    }
    public async Task<NursingResult<WsdDrainResponse>> RegisterDrainAsync(RegisterWsdDrainRequest request, ClaimsPrincipal user, Guid actor, CancellationToken ct)
    {
        var access = await guard.EnsureCanWriteAsync(request.EpisodeId, user, actor, ct);
        if (!access.IsSuccess) return access.Cast<WsdDrainResponse>();
        if (request.InsertedAt == default || request.InsertedAt > DateTime.UtcNow || request.InsertedAt < access.Value!.AdmittedAt || string.IsNullOrWhiteSpace(request.DrainLabel))
            return Fail<WsdDrainResponse>(400, "Label dan waktu pemasangan selang tidak sah.");
        if (await DepartedAsync(request.EpisodeId, ct)) return Fail<WsdDrainResponse>(422, "Pasien sudah keluar ruangan.");
        var row = new CliWsdDrain { InpEpisodeId = request.EpisodeId, EncounterId = access.Value!.EncounterId, PatientId = access.Value.PatientId,
            DrainLabel = request.DrainLabel.Trim(), InsertionSite = request.InsertionSite?.Trim(), InsertedAt = request.InsertedAt,
            InitialResidualMl = request.InitialResidualMl ?? 0, RegisteredByUserId = actor, CreateBy = actor };
        db.Add(row);
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException { SqlState: "23505" }) { db.Entry(row).State = EntityState.Detached; return Fail<WsdDrainResponse>(409, "Label selang aktif sudah dipakai."); }
        await LogAsync("Register", row.Id, actor);
        return NursingResult<WsdDrainResponse>.Ok(Drain(row), "Selang WSD terdaftar.", 201);
    }
    public async Task<NursingResult<WsdDrainResponse>> CorrectDrainAsync(Guid id, CorrectWsdDrainRequest request, ClaimsPrincipal user, Guid actor, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var row = await LockDrainAsync(id, ct);
        if (row == null) return Fail<WsdDrainResponse>(404, "Selang tidak ditemukan.");
        var access = await guard.EnsureCanWriteAsync(row.InpEpisodeId, user, actor, ct);
        if (!access.IsSuccess) return access.Cast<WsdDrainResponse>();
        if (row.Version != request.ExpectedVersion) return Fail<WsdDrainResponse>(409, "Versi berubah. Muat ulang selang.");
        if (string.IsNullOrWhiteSpace(request.Reason) || string.IsNullOrWhiteSpace(request.DrainLabel) || request.InsertedAt == default || request.InsertedAt > DateTime.UtcNow || request.InsertedAt < access.Value!.AdmittedAt || request.InsertedAt > row.RemovedAt)
            return Fail<WsdDrainResponse>(400, "Label, waktu, dan alasan koreksi wajib sah.");
        var earliest = await db.Set<CliWsdReading>().Where(x => x.WsdDrainId == id && x.Status == ClinicalMeasurementStatus.Active).MinAsync(x => (DateTime?)x.PeriodStartAt, ct);
        if (earliest.HasValue && request.InsertedAt > earliest) return Fail<WsdDrainResponse>(422, "Waktu pemasangan melewati pembacaan pertama.");
        row.DrainLabel = request.DrainLabel.Trim(); row.InsertionSite = request.InsertionSite?.Trim(); row.InsertedAt = request.InsertedAt;
        row.CorrectionReason = request.Reason.Trim(); row.Version++; row.UpdateBy = actor; row.UpdateDateTime = DateTime.UtcNow;
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct); await LogAsync("CorrectDrain", id, actor);
        return NursingResult<WsdDrainResponse>.Ok(Drain(row), "Koreksi selang tersimpan.");
    }
    public async Task<NursingResult<WsdDrainResponse>> RemoveDrainAsync(Guid id, RemoveWsdDrainRequest request, ClaimsPrincipal user, Guid actor, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var row = await LockDrainAsync(id, ct);
        if (row == null) return Fail<WsdDrainResponse>(404, "Selang tidak ditemukan.");
        var access = await guard.EnsureCanWriteAsync(row.InpEpisodeId, user, actor, ct);
        if (!access.IsSuccess) return access.Cast<WsdDrainResponse>();
        if (row.Status != CliWsdDrainStatus.Active) return Fail<WsdDrainResponse>(422, "Selang sudah dilepas.", "CLI-WSD-002");
        var latest = await LatestAsync(id, ct);
        if (request.RemovedAt == default || request.RemovedAt > DateTime.UtcNow || request.RemovedAt < row.InsertedAt || request.RemovedAt < latest?.PeriodEndAt)
            return Fail<WsdDrainResponse>(400, "Waktu pelepasan tidak sah.");
        row.Status = CliWsdDrainStatus.Removed; row.RemovedAt = request.RemovedAt; row.RemovedByUserId = actor; row.Version++;
        row.UpdateBy = actor; row.UpdateDateTime = DateTime.UtcNow;
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct); await LogAsync("Remove", id, actor);
        return NursingResult<WsdDrainResponse>.Ok(Drain(row, latest), "Selang dilepas.");
    }
    public async Task<NursingResult<List<WsdReadingResponse>>> ReadingsAsync(Guid id, CancellationToken ct)
    {
        if (!await db.Set<CliWsdDrain>().AnyAsync(x => x.Id == id && !x.IsDelete, ct)) return Fail<List<WsdReadingResponse>>(404, "Selang tidak ditemukan.");
        var rows = await db.Set<CliWsdReading>().AsNoTracking().Where(x => x.WsdDrainId == id && !x.IsDelete).OrderBy(x => x.PeriodEndAt).ToListAsync(ct);
        var actors = rows.Select(x => x.RecordedByUserId).Distinct().ToList();
        var names = await db.Set<ApplicationUser>().Where(x => actors.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.UserName, ct);
        return NursingResult<List<WsdReadingResponse>>.Ok(rows.Select(x => Reading(x, names.GetValueOrDefault(x.RecordedByUserId))).ToList(), "Pembacaan WSD berhasil diambil.");
    }
    public async Task<NursingResult<WsdReadingResponse>> RecordReadingAsync(Guid id, RecordWsdReadingRequest request, ClaimsPrincipal user, Guid actor, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var drain = await LockDrainAsync(id, ct);
        if (drain == null) return Fail<WsdReadingResponse>(404, "Selang tidak ditemukan.");
        var access = await guard.EnsureCanWriteAsync(drain.InpEpisodeId, user, actor, ct);
        if (!access.IsSuccess) return access.Cast<WsdReadingResponse>();
        if (drain.Status != CliWsdDrainStatus.Active || await DepartedAsync(drain.InpEpisodeId, ct)) return Fail<WsdReadingResponse>(422, "Selang sudah dilepas atau pasien keluar.", "CLI-WSD-002");
        var latest = await LatestAsync(id, ct);
        if (request.PeriodStartAt < drain.InsertedAt || request.PeriodEndAt <= request.PeriodStartAt || request.PeriodEndAt > DateTime.UtcNow || request.PeriodStartAt < latest?.PeriodEndAt)
            return Fail<WsdReadingResponse>(422, "Periode pembacaan tidak sah atau bertumpang tindih.");
        if (request.ShiftId.HasValue && !await db.Set<CliNursingShift>().AnyAsync(x => x.Id == request.ShiftId && !x.IsDelete && (x.ServiceUnitId == null || x.ServiceUnitId == access.Value!.ServiceUnitId), ct))
            return Fail<WsdReadingResponse>(422, "Shift bukan milik unit pasien.");
        var previous = latest?.CurrentResidualMl ?? drain.InitialResidualMl;
        var increase = ComputeIncreaseMl(previous, request.CurrentResidualMl, request.DiscardedVolumeMl ?? 0);
        if (increase < 0) return Fail<WsdReadingResponse>(422, "Volume bertambah tidak boleh negatif.", "CLI-WSD-001");
        var fluid = new CliFluidBalanceEntry { InpEpisodeId = drain.InpEpisodeId, EncounterId = drain.EncounterId, PatientId = drain.PatientId,
            Direction = FluidDirection.Output, SourceCategory = FluidSourceCategory.DrainOrWsd, SourceDetail = drain.DrainLabel, VolumeMl = increase,
            EntryDateTime = request.PeriodEndAt, RecordedByEmployeeId = access.Value!.ActorEmployeeId, RecordedByUserId = actor, CreateBy = actor };
        var row = new CliWsdReading { WsdDrainId = id, InpEpisodeId = drain.InpEpisodeId, ShiftId = request.ShiftId, PeriodStartAt = request.PeriodStartAt,
            PeriodEndAt = request.PeriodEndAt, PreviousResidualMl = previous, CurrentResidualMl = request.CurrentResidualMl, DiscardedVolumeMl = request.DiscardedVolumeMl ?? 0,
            IncreaseMl = increase, FluidBalanceEntryId = fluid.Id, RecordedByUserId = actor, CreateBy = actor };
        drain.Version++; db.Add(fluid); db.Add(row);
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct); await LogAsync("RecordReading", row.Id, actor);
        return NursingResult<WsdReadingResponse>.Ok(Reading(row), "Pembacaan dan entri cairan tersimpan.", 201);
    }
    public async Task<NursingResult<WsdReadingResponse>> CorrectLatestReadingAsync(Guid id, CorrectWsdReadingRequest request, ClaimsPrincipal user, Guid actor, CancellationToken ct)
        => await ReviseAsync(id, request, null, user, actor, ct);
    public async Task<NursingResult<WsdReadingResponse>> CancelLatestReadingAsync(Guid id, CancelClinicalMeasurementRequest request, ClaimsPrincipal user, Guid actor, CancellationToken ct)
        => await ReviseAsync(id, null, request, user, actor, ct);
    private async Task<NursingResult<WsdReadingResponse>> ReviseAsync(Guid id, CorrectWsdReadingRequest? correction, CancelClinicalMeasurementRequest? cancel, ClaimsPrincipal user, Guid actor, CancellationToken ct)
    {
        var reference = await db.Set<CliWsdReading>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);
        if (reference == null) return Fail<WsdReadingResponse>(404, "Pembacaan tidak ditemukan.");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var drain = await LockDrainAsync(reference.WsdDrainId, ct);
        var row = await db.Set<CliWsdReading>().FirstAsync(x => x.Id == id, ct);
        if ((await LatestAsync(row.WsdDrainId, ct))?.Id != id) return Fail<WsdReadingResponse>(422, "Hanya pembacaan terakhir yang dapat dikoreksi atau dibatalkan.", "CLI-WSD-003");
        var access = await guard.EnsureCanWriteAsync(row.InpEpisodeId, user, actor, ct);
        if (!access.IsSuccess) return access.Cast<WsdReadingResponse>();
        var fluid = await db.Set<CliFluidBalanceEntry>().FirstAsync(x => x.Id == row.FluidBalanceEntryId, ct);
        NursingResult<FluidBalanceEntryResponse> result;
        if (correction != null)
        {
            var increase = ComputeIncreaseMl(row.PreviousResidualMl, correction.CurrentResidualMl, correction.DiscardedVolumeMl ?? 0);
            if (increase < 0) return Fail<WsdReadingResponse>(422, "Volume bertambah tidak boleh negatif.", "CLI-WSD-001");
            result = await fluids.CorrectFluidAsync(fluid.Id, new CorrectFluidBalanceEntryRequest { VolumeMl = increase, SourceCategory = FluidSourceCategory.DrainOrWsd,
                SourceDetail = drain!.DrainLabel, CorrectionReason = correction.Reason, ExpectedRevisionNumber = fluid.RevisionNumber }, user, actor, ct, fromWsd: true);
            if (!result.IsSuccess) return result.Cast<WsdReadingResponse>();
            row.CurrentResidualMl = correction.CurrentResidualMl; row.DiscardedVolumeMl = correction.DiscardedVolumeMl ?? 0; row.IncreaseMl = increase; row.RevisionNumber++;
        }
        else
        {
            result = await fluids.CancelFluidAsync(fluid.Id, new CancelClinicalMeasurementRequest { Reason = cancel!.Reason, ExpectedRevisionNumber = fluid.RevisionNumber }, user, actor, ct, fromWsd: true);
            if (!result.IsSuccess) return result.Cast<WsdReadingResponse>();
            row.Status = ClinicalMeasurementStatus.Cancelled; row.CancelReason = cancel.Reason; row.IsCancel = true; row.CancelBy = actor; row.CancelDateTime = DateTime.UtcNow;
        }
        row.UpdateBy = actor; row.UpdateDateTime = DateTime.UtcNow; drain!.Version++;
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct); await LogAsync(correction == null ? "CancelReading" : "CorrectReading", id, actor);
        return NursingResult<WsdReadingResponse>.Ok(Reading(row), "Revisi pembacaan dan cairan tersimpan.");
    }
    private Task<bool> DepartedAsync(Guid episodeId, CancellationToken ct) => db.Set<InpEpisode>().AnyAsync(x => x.Id == episodeId && x.PhysicallyLeftAt != null, ct);
    private Task<CliWsdDrain?> LockDrainAsync(Guid id, CancellationToken ct) => db.Set<CliWsdDrain>().FromSqlInterpolated($"SELECT * FROM public.\"CliWsdDrain\" WHERE \"Id\" = {id} AND \"IsDelete\" = false FOR UPDATE").FirstOrDefaultAsync(ct);
    private Task<CliWsdReading?> LatestAsync(Guid id, CancellationToken ct) => db.Set<CliWsdReading>().Where(x => x.WsdDrainId == id && !x.IsDelete && x.Status == ClinicalMeasurementStatus.Active).OrderByDescending(x => x.PeriodEndAt).FirstOrDefaultAsync(ct);
    private Task LogAsync(string action, Guid id, Guid actor) => logger.InfoAsync("HealthServices.Clinical.Wsd", action, "Status WSD berubah.", new { Id = id, ActorUserId = actor });
    private static WsdDrainResponse Drain(CliWsdDrain x, CliWsdReading? latest = null) => new() { Id = x.Id, EpisodeId = x.InpEpisodeId, DrainLabel = x.DrainLabel, InsertionSite = x.InsertionSite, InsertedAt = x.InsertedAt, InitialResidualMl = x.InitialResidualMl, RemovedAt = x.RemovedAt, Status = x.Status, Version = x.Version, LatestReading = latest == null ? null : Reading(latest) };
    private static WsdReadingResponse Reading(CliWsdReading x, string? name = null) => new() { Id = x.Id, PeriodStartAt = x.PeriodStartAt, PeriodEndAt = x.PeriodEndAt, PreviousResidualMl = x.PreviousResidualMl, CurrentResidualMl = x.CurrentResidualMl, DiscardedVolumeMl = x.DiscardedVolumeMl, IncreaseMl = x.IncreaseMl, FluidBalanceEntryId = x.FluidBalanceEntryId, Status = x.Status, RevisionNumber = x.RevisionNumber, RecordedByName = name };
}
