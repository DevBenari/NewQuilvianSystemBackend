using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
public class CliTransfusionMonitoringService(ApplicationDbContext db, NursingEpisodeWriteGuard guard, IConfiguration config, LoggerService logger, CliTransfusionReactionDeliveryService delivery)
{
    private int Tolerance => Math.Clamp(config.GetValue("Clinical:TransfusionMonitoring:LateToleranceMinutes", 10), 0, 120);
    private IQueryable<BbkBloodUnit> Selectable(InpEpisode ep) => db.Set<BbkBloodUnit>().AsNoTracking().Where(x => !x.IsDelete && x.IssuedToPatientId == ep.PatientId && x.IssuedAt >= ep.AdmittedAt &&
        !db.Set<CliTransfusionMonitoring>().Any(m => m.BloodUnitId == x.Id && m.Status != CliTransfusionMonitoringStatus.Cancelled));
    public async Task<NursingResult<List<SelectableBloodUnit>>> SelectableAsync(Guid episodeId, CancellationToken ct)
    {
        var ep = await db.Set<InpEpisode>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == episodeId && !x.IsDelete, ct);
        if (ep == null) return NursingResult<List<SelectableBloodUnit>>.Fail(404, "Episode tidak ditemukan.");
        return NursingResult<List<SelectableBloodUnit>>.Ok(await Selectable(ep).OrderBy(x => x.IssuedAt).Select(x => new SelectableBloodUnit { BloodUnitId = x.Id, PmiBagNumber = x.PmiBagNumber, ComponentName = x.BloodComponent != null ? x.BloodComponent.ComponentName : string.Empty, IssuedAt = x.IssuedAt }).ToListAsync(ct), "Kantong yang diserahkan kepada pasien.");
    }
    public async Task<NursingResult<List<TransfusionMonitoringResponse>>> ListAsync(Guid episodeId, CancellationToken ct)
    {
        var rows = await db.Set<CliTransfusionMonitoring>().AsNoTracking().Where(x => x.InpEpisodeId == episodeId && !x.IsDelete).OrderByDescending(x => x.TransfusionStartedAt).ToListAsync(ct);
        var results = new List<TransfusionMonitoringResponse>(); foreach (var row in rows) results.Add(await ResponseAsync(row, ct));
        return NursingResult<List<TransfusionMonitoringResponse>>.Ok(results, "Monitoring transfusi berhasil diambil.");
    }
    public async Task<NursingResult<TransfusionMonitoringResponse>> StartAsync(StartTransfusionMonitoringRequest request, ClaimsPrincipal user, Guid actor, CancellationToken ct)
    {
        var access = await guard.EnsureCanWriteAsync(request.EpisodeId, user, actor, ct); if (!access.IsSuccess) return access.Cast<TransfusionMonitoringResponse>();
        var ep = await db.Set<InpEpisode>().AsNoTracking().FirstAsync(x => x.Id == request.EpisodeId, ct);
        if (ep.PhysicallyLeftAt.HasValue) return NursingResult<TransfusionMonitoringResponse>.Fail(422, "Pasien sudah keluar ruangan.");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        // Lock Clinical's key without writing the Blood Bank aggregate.
        await db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock(hashtextextended({"TRF:" + request.BloodUnitId}, 0))", ct);
        if (!await db.Set<BbkBloodUnit>().AnyAsync(x => x.Id == request.BloodUnitId && !x.IsDelete && x.IssuedToPatientId == ep.PatientId && x.IssuedAt >= ep.AdmittedAt, ct))
            return NursingResult<TransfusionMonitoringResponse>.Fail(422, "Kantong belum diserahkan kepada pasien ini.", "CLI-TRF-001");
        if (await db.Set<CliTransfusionMonitoring>().AnyAsync(x => x.BloodUnitId == request.BloodUnitId && x.Status != CliTransfusionMonitoringStatus.Cancelled, ct)) return NursingResult<TransfusionMonitoringResponse>.Fail(409, "Kantong sudah dipantau.", "CLI-TRF-002");
        var unit = await Selectable(ep).FirstOrDefaultAsync(x => x.Id == request.BloodUnitId, ct);
        if (unit == null) return NursingResult<TransfusionMonitoringResponse>.Fail(422, "Kantong belum diserahkan kepada pasien ini.", "CLI-TRF-001");
        if (request.ReceivedAtWardAt < unit.IssuedAt || request.TransfusionStartedAt < request.ReceivedAtWardAt || request.TransfusionStartedAt > DateTime.UtcNow || request.ReceivedAtWardAt == default)
            return NursingResult<TransfusionMonitoringResponse>.Fail(422, "Urutan waktu penerimaan dan mulai transfusi tidak sah.");
        var row = new CliTransfusionMonitoring { InpEpisodeId = ep.Id, EncounterId = ep.EncounterId, PatientId = ep.PatientId, BloodUnitId = unit.Id,
            ReceivedAtWardAt = request.ReceivedAtWardAt, TransfusionStartedAt = request.TransfusionStartedAt, PerformedByUserId = actor, CreateBy = actor };
        db.Add(row);
        foreach (var point in Enum.GetValues<CliTransfusionPointType>()) db.Add(new CliTransfusionMonitoringPoint { MonitoringId = row.Id, PointType = point, DueAt = row.TransfusionStartedAt.AddMinutes(point switch { CliTransfusionPointType.Minute15 => 15, CliTransfusionPointType.Hour1 => 60, CliTransfusionPointType.Hour4 => 240, _ => 0 }), CreateBy = actor });
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct); await LogAsync("Start", row.Id, actor);
        return NursingResult<TransfusionMonitoringResponse>.Ok(await ResponseAsync(row, ct), "Monitoring transfusi dimulai.", 201);
    }
    public async Task<NursingResult<TransfusionPointResponse>> PutPointAsync(Guid id, CliTransfusionPointType type, PutTransfusionPointRequest request, ClaimsPrincipal user, Guid actor, CancellationToken ct)
    {
        if (!Enum.IsDefined(type)) return NursingResult<TransfusionPointResponse>.Fail(400, "Titik ukur tidak dikenal.");
        await using var tx = await db.Database.BeginTransactionAsync(ct); var row = await LockAsync(id, ct);
        if (row == null) return NursingResult<TransfusionPointResponse>.Fail(404, "Monitoring tidak ditemukan.");
        var access = await guard.EnsureCanWriteAsync(row.InpEpisodeId, user, actor, ct); if (!access.IsSuccess) return access.Cast<TransfusionPointResponse>();
        if (await DepartedAsync(row.InpEpisodeId, ct)) return NursingResult<TransfusionPointResponse>.Fail(422, "Pasien sudah keluar ruangan.");
        var point = await db.Set<CliTransfusionMonitoringPoint>().FirstAsync(x => x.MonitoringId == id && x.PointType == type, ct);
        if (row.Status == CliTransfusionMonitoringStatus.Cancelled || row.Status == CliTransfusionMonitoringStatus.Completed || point.IsStopped || request.MeasuredAt > row.StoppedAt)
            return NursingResult<TransfusionPointResponse>.Fail(422, "Titik sesudah transfusi dihentikan tidak dapat diisi.", "CLI-TRF-004");
        if (request.MeasuredAt == default || request.MeasuredAt > DateTime.UtcNow || request.MeasuredAt < row.ReceivedAtWardAt || (type != CliTransfusionPointType.BeforeTransfusion && request.MeasuredAt < row.TransfusionStartedAt))
            return NursingResult<TransfusionPointResponse>.Fail(422, "Waktu pengukuran tidak sah.");
        var late = request.MeasuredAt > point.DueAt.AddMinutes(Tolerance);
        if (late && string.IsNullOrWhiteSpace(request.LateNote)) return NursingResult<TransfusionPointResponse>.Fail(422, "Keterangan terlambat wajib diisi.", "CLI-TRF-003");
        if (point.MeasuredAt.HasValue && string.IsNullOrWhiteSpace(request.CorrectionReason)) return NursingResult<TransfusionPointResponse>.Fail(400, "Alasan koreksi titik ukur wajib diisi.");
        if (request.ExpectedRevision.HasValue && request.ExpectedRevision != point.RevisionNumber) return NursingResult<TransfusionPointResponse>.Fail(409, "Revisi berubah.");
        point.MeasuredAt = request.MeasuredAt; point.SystolicBp = request.SystolicBp; point.DiastolicBp = request.DiastolicBp; point.TemperatureCelsius = request.TemperatureCelsius; point.PulseRate = request.PulseRate;
        point.IsLate = late; point.LateNote = request.LateNote?.Trim(); point.CorrectionReason = request.CorrectionReason?.Trim(); point.RecordedByUserId = actor; point.RevisionNumber++; point.UpdateBy = actor; point.UpdateDateTime = DateTime.UtcNow;
        row.Version++; row.UpdateBy = actor; row.UpdateDateTime = DateTime.UtcNow; await db.SaveChangesAsync(ct); await tx.CommitAsync(ct); await LogAsync("Point", id, actor);
        return NursingResult<TransfusionPointResponse>.Ok(Point(point), "Titik ukur tersimpan.");
    }
    public async Task<NursingResult<TransfusionReactionResponse>> RecordReactionAsync(Guid id, RecordTransfusionReactionRequest request, ClaimsPrincipal user, Guid actor, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct); var row = await LockAsync(id, ct);
        if (row == null) return NursingResult<TransfusionReactionResponse>.Fail(404, "Monitoring tidak ditemukan.");
        var access = await guard.EnsureCanWriteAsync(row.InpEpisodeId, user, actor, ct); if (!access.IsSuccess) return access.Cast<TransfusionReactionResponse>();
        if (row.Status == CliTransfusionMonitoringStatus.Cancelled || request.OccurredAt < row.ReceivedAtWardAt || request.OccurredAt > DateTime.UtcNow || string.IsNullOrWhiteSpace(request.ReactionSummary) || (request.PointType.HasValue && !Enum.IsDefined(request.PointType.Value)))
            return NursingResult<TransfusionReactionResponse>.Fail(422, "Reaksi dan waktu kejadian wajib sah.");
        var reaction = new CliTransfusionReaction { MonitoringId = id, OccurredAt = request.OccurredAt, PointType = request.PointType, ReactionSummary = request.ReactionSummary.Trim(), ReactionDetail = request.ReactionDetail?.Trim(), RecordedByUserId = actor, CreateBy = actor };
        db.Add(reaction); await db.SaveChangesAsync(ct); await tx.CommitAsync(ct); await LogAsync("Reaction", reaction.Id, actor);
        await tx.DisposeAsync();
        await delivery.DeliverAsync(reaction.Id, ct);
        return NursingResult<TransfusionReactionResponse>.Ok(Reaction(reaction), "Reaksi tersimpan; status pemberitahuan Bank Darah tersedia.", 201);
    }
    public async Task<NursingResult<TransfusionMonitoringResponse>> StopAsync(Guid id, StopTransfusionRequest request, ClaimsPrincipal user, Guid actor, CancellationToken ct) => await EndAsync(id, CliTransfusionMonitoringStatus.Stopped, request.StoppedAt, request.Reason, user, actor, ct);
    public async Task<NursingResult<TransfusionMonitoringResponse>> CompleteAsync(Guid id, CompleteTransfusionRequest request, ClaimsPrincipal user, Guid actor, CancellationToken ct) => await EndAsync(id, CliTransfusionMonitoringStatus.Completed, request.CompletedAt, null, user, actor, ct);
    public async Task<NursingResult<TransfusionMonitoringResponse>> CancelAsync(Guid id, CancelClinicalMeasurementRequest request, ClaimsPrincipal user, Guid actor, CancellationToken ct) => await EndAsync(id, CliTransfusionMonitoringStatus.Cancelled, DateTime.UtcNow, request.Reason, user, actor, ct);
    private async Task<NursingResult<TransfusionMonitoringResponse>> EndAsync(Guid id, CliTransfusionMonitoringStatus status, DateTime at, string? reason, ClaimsPrincipal user, Guid actor, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct); var row = await LockAsync(id, ct);
        if (row == null) return NursingResult<TransfusionMonitoringResponse>.Fail(404, "Monitoring tidak ditemukan.");
        var access = await guard.EnsureCanWriteAsync(row.InpEpisodeId, user, actor, ct); if (!access.IsSuccess) return access.Cast<TransfusionMonitoringResponse>();
        if (row.Status != CliTransfusionMonitoringStatus.InProgress) return NursingResult<TransfusionMonitoringResponse>.Fail(409, "Monitoring sudah diakhiri.");
        if (at < row.TransfusionStartedAt || at > DateTime.UtcNow || (status != CliTransfusionMonitoringStatus.Completed && string.IsNullOrWhiteSpace(reason))) return NursingResult<TransfusionMonitoringResponse>.Fail(400, "Waktu dan alasan wajib sah.");
        var points = await db.Set<CliTransfusionMonitoringPoint>().Where(x => x.MonitoringId == id).ToListAsync(ct);
        if (status == CliTransfusionMonitoringStatus.Cancelled && (points.Any(x => x.MeasuredAt.HasValue) || await db.Set<CliTransfusionReaction>().AnyAsync(x => x.MonitoringId == id, ct))) return NursingResult<TransfusionMonitoringResponse>.Fail(422, "Monitoring dengan titik ukur atau reaksi tidak dapat dibatalkan.", "CLI-TRF-005");
        if (points.Any(x => x.MeasuredAt > at)) return NursingResult<TransfusionMonitoringResponse>.Fail(422, "Waktu akhir mendahului titik ukur yang sudah dicatat.");
        row.Status = status; row.Version++; row.UpdateBy = actor; row.UpdateDateTime = DateTime.UtcNow;
        if (status == CliTransfusionMonitoringStatus.Stopped) { row.StoppedAt = at; row.StopReason = reason!.Trim(); foreach (var p in points.Where(x => !x.MeasuredAt.HasValue && x.DueAt > at)) { p.IsStopped = true; p.UpdateBy = actor; p.UpdateDateTime = DateTime.UtcNow; } }
        if (status == CliTransfusionMonitoringStatus.Completed) row.CompletedAt = at;
        if (status == CliTransfusionMonitoringStatus.Cancelled) { row.CancelReason = reason!.Trim(); row.IsCancel = true; row.CancelBy = actor; row.CancelDateTime = at; }
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct); await LogAsync(status.ToString(), id, actor);
        return NursingResult<TransfusionMonitoringResponse>.Ok(await ResponseAsync(row, ct), "Status monitoring tersimpan.");
    }
    private Task<bool> DepartedAsync(Guid id, CancellationToken ct) => db.Set<InpEpisode>().AnyAsync(x => x.Id == id && x.PhysicallyLeftAt != null, ct);
    private Task<CliTransfusionMonitoring?> LockAsync(Guid id, CancellationToken ct) => db.Set<CliTransfusionMonitoring>().FromSqlInterpolated($"SELECT * FROM public.\"CliTransfusionMonitoring\" WHERE \"Id\" = {id} AND \"IsDelete\" = false FOR UPDATE").FirstOrDefaultAsync(ct);
    private async Task<TransfusionMonitoringResponse> ResponseAsync(CliTransfusionMonitoring x, CancellationToken ct)
    {
        var bag = await db.Set<BbkBloodUnit>().AsNoTracking().Include(b => b.BloodComponent).FirstAsync(b => b.Id == x.BloodUnitId, ct);
        var points = await db.Set<CliTransfusionMonitoringPoint>().AsNoTracking().Where(p => p.MonitoringId == x.Id).OrderBy(p => p.PointType).ToListAsync(ct);
        var reactions = await db.Set<CliTransfusionReaction>().AsNoTracking().Where(r => r.MonitoringId == x.Id).OrderBy(r => r.OccurredAt).ToListAsync(ct);
        return new() { Id = x.Id, EpisodeId = x.InpEpisodeId, BloodUnitId = x.BloodUnitId, PmiBagNumber = bag.PmiBagNumber, ComponentName = bag.BloodComponent?.ComponentName ?? string.Empty, ReceivedAtWardAt = x.ReceivedAtWardAt,
            TransfusionStartedAt = x.TransfusionStartedAt, Status = x.Status, StoppedAt = x.StoppedAt, StopReason = x.StopReason, CompletedAt = x.CompletedAt, Version = x.Version, LateToleranceMinutes = Tolerance, Points = points.Select(Point).ToList(), Reactions = reactions.Select(Reaction).ToList() };
    }
    private TransfusionPointResponse Point(CliTransfusionMonitoringPoint x) => new() { Id = x.Id, PointType = x.PointType, DueAt = x.DueAt, MeasuredAt = x.MeasuredAt, SystolicBp = x.SystolicBp, DiastolicBp = x.DiastolicBp, TemperatureCelsius = x.TemperatureCelsius, PulseRate = x.PulseRate, IsLate = x.IsLate || (!x.MeasuredAt.HasValue && !x.IsStopped && DateTime.UtcNow > x.DueAt.AddMinutes(Tolerance)), IsStopped = x.IsStopped, LateNote = x.LateNote, RevisionNumber = x.RevisionNumber };
    public static TransfusionReactionResponse Reaction(CliTransfusionReaction x) => new() { Id = x.Id, OccurredAt = x.OccurredAt, PointType = x.PointType, ReactionSummary = x.ReactionSummary, ReactionDetail = x.ReactionDetail, NoticeDelivery = x.NoticeDelivery, NoticeAttemptCount = x.NoticeAttemptCount, BloodBankNoticeId = x.BloodBankNoticeId };
    private Task LogAsync(string action, Guid id, Guid actor) => logger.InfoAsync("Clinical.TransfusionMonitoring", action, "Monitoring transfusi berubah.", new { Id = id, ActorUserId = actor });
}
