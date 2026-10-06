using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Responses;
namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
public partial class InpDischargeService
{
    /// <summary>
    /// Mencatat pasien sudah meninggalkan ruangan: melepas tempat tidur seketika <b>tanpa</b>
    /// menutup episode, dan menyimpan status kasir yang terbaca saat itu (<c>BE-RWI-153</c>).
    /// </summary>
    /// <remarks>
    /// Kepergian fisik bukan perubahan status, sehingga tidak menulis <c>InpStatusHistory</c>;
    /// episode tetap <c>DischargePending</c> (<c>RWI-DEC-009</c>). Kasir tidak menahan
    /// kepergian (<c>RWI-DEC-186</c>): bila status bukan <c>CLEARED</c> atau tidak terbaca,
    /// layar wajib menampilkan peringatan lalu mengirim ulang dengan pengakuan, selain itu 409
    /// <c>INP-DEP-001</c>. Penutupan pemakaian alat berjalan sesudah commit dan kegagalannya
    /// tidak membatalkan kepergian. Tidak dapat dibatalkan (<c>RWI-RULE-036</c>).
    /// </remarks>
    public async Task<InpEpisodeOperationResult> RecordPatientDepartureAsync(Guid episodeId,
        RecordDepartureRequest? request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        await using var tx = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        var episode = await _dbContext.Set<InpEpisode>().FromSqlInterpolated($"SELECT * FROM public.\"InpEpisode\" WHERE \"Id\" = {episodeId} AND \"IsDelete\" = false FOR UPDATE").FirstOrDefaultAsync(cancellationToken);
        if (episode == null) return InpEpisodeOperationResult.NotFound("Episode tidak ditemukan.");
        if (episode.EpisodeStatus != InpEpisodeStatus.DischargePending) return InpEpisodeOperationResult.BusinessRuleRejected("Keputusan pulang DPJP belum ada.", episode);
        if (episode.PhysicallyLeftAt.HasValue) return InpEpisodeOperationResult.Conflict("Kepergian pasien sudah dicatat.", episode);
        var now = DateTime.UtcNow;
        var departedAt = request?.DepartedAt ?? now;
        if (departedAt > now || departedAt < episode.DischargeDecidedAt) return InpEpisodeOperationResult.Invalid("Waktu kepergian tidak sah.");
        var placement = await _dbContext.Set<InpBedPlacement>().Include(x => x.Bed).FirstOrDefaultAsync(x => x.EpisodeId == episodeId && x.EndDateTime == null && !x.IsDelete, cancellationToken);
        if (placement != null && departedAt < placement.StartDateTime) return InpEpisodeOperationResult.Invalid("Waktu keluar mendahului penempatan tempat tidur.");
        if (await _dbContext.Set<QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models.CliEquipmentUsage>().AnyAsync(x => x.InpEpisodeId == episodeId && x.Status == QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums.CliEquipmentUsageStatus.Running && !x.IsDelete && x.StartedAt > departedAt, cancellationToken))
            return InpEpisodeOperationResult.Invalid("Waktu keluar mendahului pemakaian alat yang masih berjalan.");
        var observed = await _billingClearanceAdapter.GetStatusAsync(episode.EncounterId, cancellationToken);
        if ((!observed.IsReadable || observed.Status != "CLEARED") && request?.ClearanceWarningAcknowledged != true)
            return InpEpisodeOperationResult.FromStatus(InpEpisodeOperationStatus.Conflict,
                observed.IsReadable
                    ? "Kasir belum memberi izin pulang. Pastikan keluarga sudah diarahkan ke kasir, lalu konfirmasi bila pasien tetap meninggalkan ruangan."
                    : "Status kasir tidak dapat dibaca saat ini. Kepergian tetap dapat dicatat, dan episode ini akan masuk daftar pulang sebelum izin kasir.", "INP-DEP-001");
        var response = new InpatientDepartureResponse { EpisodeId = episode.Id, EpisodeStatus = episode.EpisodeStatus,
            PhysicallyLeftAt = departedAt, ClearanceObserved = observed.ToObservation(), ClearanceObservedAt = now,
            ReleasedBedCode = placement?.Bed?.BedCode,
            PhysicallyLeftByUserName = await _dbContext.Users.Where(x => x.Id == actorUserId).Select(x => x.DisplayName).FirstOrDefaultAsync(cancellationToken) };
        await _bedOccupancyService.ReleaseActivePlacementAsync(episodeId, InpBedPlacementEndReason.PatientDeparted, actorUserId, departedAt, cancellationToken);
        if (placement != null)
        {
            placement.PhysicallyLeftAt = departedAt;
            await _outboxService.EnqueueEventAsync("BED_RELEASED", episode.Id, episode.EncounterId, "DISCHARGE", placement.Id, placement.Version, departedAt, cancellationToken);
        }
        episode.PhysicallyLeftAt = departedAt; episode.PhysicallyLeftByUserId = actorUserId;
        episode.DepartureClearanceObserved = observed.ToObservation(); episode.DepartureClearanceObservedAt = now;
        episode.DepartureClearanceWarningAcknowledged = request?.ClearanceWarningAcknowledged == true;
        episode.UpdateBy = actorUserId; episode.UpdateDateTime = now;
        await _dbContext.SaveChangesAsync(cancellationToken); await tx.CommitAsync(cancellationToken); await tx.DisposeAsync();
        // Clinical departure is durable. Follow-up uses a separate context and cannot undo it.
        try
        {
            using var scope = _scopeFactory.CreateScope();
            response.RunningEquipmentUsageClosedCount = await scope.ServiceProvider.GetRequiredService<CliEquipmentUsageService>()
                .CloseRunningForDepartureAsync(episodeId, departedAt, actorUserId, cancellationToken);
        }
        catch (Exception ex)
        {
            _dischargeLogger.LogError(ex, "Penutupan alat sesudah keluar ruangan gagal untuk {EpisodeId}.", episodeId);
            response.FollowUpWarnings.Add("Keluar ruangan tersimpan. Penutupan pemakaian alat perlu ditindaklanjuti; sistem akan mencoba ulang.");
        }
        var result = InpEpisodeOperationResult.Success(episode, "Kepergian pasien tersimpan. Tempat tidur sudah dilepas; episode tetap DischargePending.");
        result.Departure = response; return result;
    }

    public async Task<PagedResult<DepartureBeforeClearanceItem>> GetDeparturesBeforeClearanceAsync(DepartureBeforeClearanceQuery query, CancellationToken ct)
    {
        var page = Math.Max(1, query.PageNumber); var size = Math.Clamp(query.PageSize, 1, 100);
        var source = _dbContext.Set<InpEpisode>().AsNoTracking().Where(x => !x.IsDelete && x.PhysicallyLeftAt != null &&
            x.DepartureClearanceObserved != null && x.DepartureClearanceObserved != InpClearanceObservation.Cleared &&
            (query.IncludeClosed || x.EpisodeStatus != InpEpisodeStatus.Closed));
        if (query.ServiceUnitId.HasValue) source = source.Where(x => x.ServiceUnitId == query.ServiceUnitId);
        if (query.From.HasValue) source = source.Where(x => x.PhysicallyLeftAt >= query.From);
        if (query.To.HasValue) source = source.Where(x => x.PhysicallyLeftAt <= query.To);
        var count = await source.CountAsync(ct);
        var rows = await source.OrderByDescending(x => x.PhysicallyLeftAt).Skip((page - 1) * size).Take(size)
            .Select(x => new DepartureBeforeClearanceItem { EpisodeId = x.Id, EpisodeNumber = x.EpisodeNumber, EncounterId = x.EncounterId,
                PatientName = x.Patient == null ? null : x.Patient.FullName, MedicalRecordNumber = x.Patient == null ? null : x.Patient.MedicalRecordNumber,
                ServiceUnitId = x.ServiceUnitId, PhysicallyLeftAt = x.PhysicallyLeftAt, DepartureClearanceObserved = x.DepartureClearanceObserved, EpisodeStatus = x.EpisodeStatus }).ToListAsync(ct);
        foreach (var row in rows) { var current = await _billingClearanceAdapter.GetStatusAsync(row.EncounterId, ct); row.IsReadable = current.IsReadable; row.CurrentClearanceStatus = current.Status; }
        return new() { Items = rows, PageNumber = page, PageSize = size, TotalData = count, TotalPage = (int)Math.Ceiling(count / (double)size) };
    }
}
