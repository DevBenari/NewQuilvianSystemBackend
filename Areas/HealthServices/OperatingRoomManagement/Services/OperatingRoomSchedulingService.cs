using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Options;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using static QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services.OperatingRoomCommandSupport;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services;

/// <summary>
/// Penjadwalan kasus operasi: penetapan jadwal, revisi, reschedule, dan penundaan
/// (BE-OPR-004, OPS-REQ-003/004, OPS-DEC-004/016/017).
/// </summary>
public sealed class OperatingRoomSchedulingService
{
    /// <summary>Batas atas buffer pada DTO; dipakai sebagai prefilter query benturan.</summary>
    private const int MaxBufferMinutes = 480;

    private static readonly string[] ScheduleActions = ["Schedule", "Reschedule"];
    private static readonly string[] PostponeActions = ["Postpone"];

    private static readonly OprTeamRole[] RequiredRoles =
    [
        OprTeamRole.PrimarySurgeon, OprTeamRole.Anesthesiologist,
        OprTeamRole.ScrubNurse, OprTeamRole.CirculatingNurse
    ];

    private static readonly OprCaseStatus[] SchedulableStatuses =
    [
        OprCaseStatus.Requested, OprCaseStatus.Scheduled, OprCaseStatus.Postponed
    ];

    private static readonly OprCaseStatus[] ClosedStatuses =
    [
        OprCaseStatus.Completed, OprCaseStatus.Cancelled
    ];

    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LoggerService _loggerService;
    private readonly OperatingRoomCredentialResolver _credentialResolver;
    private readonly OperatingRoomSchedulingOptions _options;

    public OperatingRoomSchedulingService(ApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor,
        LoggerService loggerService, OperatingRoomCredentialResolver credentialResolver,
        IOptions<OperatingRoomSchedulingOptions> options)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _loggerService = loggerService;
        _credentialResolver = credentialResolver;
        _options = options.Value;
    }

    public async Task<OprScheduleResponse> ScheduleAsync(Guid caseId, ScheduleOprCaseRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateScheduleRequest(request);
        var actorUserId = GetUserId(_httpContextAccessor);
        var fingerprint = BuildFingerprint(request);

        var prior = await FindIdempotentAsync(ScheduleActions, request.IdempotencyKey, cancellationToken);
        if (prior != null)
        {
            if (prior.OprCaseId != caseId)
                throw new OperatingRoomConflictException("OPR013", "Idempotency key sudah digunakan untuk kasus lain.");
            EnsureSameFingerprint(prior.Source, fingerprint);
            return (await GetScheduleResponseAsync(caseId, cancellationToken))!;
        }

        var entity = await LoadCaseAsync(caseId, cancellationToken)
            ?? throw new KeyNotFoundException("Kasus operasi tidak ditemukan.");
        if (!SchedulableStatuses.Contains(entity.Status))
            throw new OperatingRoomConflictException("InvalidStateTransition",
                "Jadwal hanya dapat ditetapkan pada kasus berstatus Requested, Scheduled, atau Postponed.");
        if (entity.Version != request.ExpectedVersion)
            throw new OperatingRoomConflictException("OPR012", "Data telah diperbarui pengguna lain. Muat ulang lalu coba kembali.");

        var isRevision = entity.Schedules.Any(x => !x.IsDelete);
        if (isRevision && string.IsNullOrWhiteSpace(request.ChangeReason))
            throw new ArgumentException("Alasan perubahan jadwal wajib diisi.");

        var startAt = request.StartAt.ToUniversalTime();
        var endAt = request.EndAt.ToUniversalTime();
        var bufferBefore = request.BufferBeforeMinutes ?? _options.DefaultBufferBeforeMinutes;
        var bufferAfter = request.BufferAfterMinutes ?? _options.DefaultBufferAfterMinutes;
        ValidateWindow(startAt, endAt);

        await ValidateRoomAsync(request.RoomId, cancellationToken);
        await ValidateTeamAsync(entity, request.TeamMembers, cancellationToken);
        await EnsureNoOverlapAsync(caseId, request.RoomId, startAt, endAt, bufferBefore, bufferAfter,
            request.TeamMembers.Select(x => x.WorkforceId).ToList(), cancellationToken);

        var credentialStatuses = await _credentialResolver.ResolveAsync(
            request.TeamMembers.Select(x => x.WorkforceId).Distinct().ToList(), startAt, cancellationToken);
        if (credentialStatuses.Any(x => x.Value == OprCredentialCheckStatus.Invalid))
            throw new OperatingRoomUnprocessableException("OPR005",
                "Anggota tim tidak aktif atau tidak memiliki kewenangan yang sesuai.");

        var now = DateTime.UtcNow;
        var previousRevision = entity.Schedules.Where(x => !x.IsDelete).Select(x => (int?)x.Revision).Max() ?? 0;
        RetireCurrentPlan(entity, actorUserId, now, null);

        var newSchedule = new OprSchedule
        {
            OprCaseId = entity.Id, RoomId = request.RoomId, StartAt = startAt, EndAt = endAt,
            BufferBeforeMinutes = bufferBefore, BufferAfterMinutes = bufferAfter,
            Revision = previousRevision + 1, IsCurrent = true,
            ChangeReason = Normalize(request.ChangeReason), ChangedByUserId = actorUserId,
            CreateDateTime = now, CreateBy = actorUserId
        };
        foreach (var member in request.TeamMembers)
            newSchedule.TeamMembers.Add(new OprTeamMember
            {
                OprCaseId = entity.Id, ScheduleId = newSchedule.Id, WorkforceId = member.WorkforceId,
                Role = member.Role, IsLead = member.IsLead, IsCurrent = true,
                CredentialCheckStatus = credentialStatuses[member.WorkforceId], CredentialCheckedAt = now,
                CreateDateTime = now, CreateBy = actorUserId
            });
        // Ditambahkan lewat DbSet, bukan lewat navigasi kasus yang sudah dilacak, agar
        // entity baru pasti berstatus Added walaupun kuncinya diisi dari sisi aplikasi.
        _dbContext.OprSchedules.Add(newSchedule);

        var action = entity.Status == OprCaseStatus.Requested ? "Schedule" : "Reschedule";
        var fromStatus = entity.Status;
        entity.Status = OprCaseStatus.Scheduled;
        entity.Version++;
        entity.UpdateDateTime = now;
        entity.UpdateBy = actorUserId;
        _dbContext.OprStatusHistories.Add(NewHistory(entity.Id, OprCaseStatus.Scheduled, fromStatus, action,
            Normalize(request.ChangeReason), request.IdempotencyKey, fingerprint, actorUserId, now));

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            _dbContext.ChangeTracker.Clear();
            var concurrent = await FindIdempotentAsync(ScheduleActions, request.IdempotencyKey, cancellationToken);
            if (concurrent != null && concurrent.OprCaseId == caseId)
            {
                EnsureSameFingerprint(concurrent.Source, fingerprint);
                return (await GetScheduleResponseAsync(caseId, cancellationToken))!;
            }
            throw new OperatingRoomConflictException("OPR003",
                "Ruang atau anggota tim sudah memiliki jadwal pada waktu tersebut.");
        }

        await _loggerService.AuditAsync(LogCategory, $"OperatingRoomSchedule.{action}",
            "Menetapkan jadwal dan tim kasus operasi.",
            new
            {
                entity.Id,
                entity.CaseNumber,
                ActorUserId = actorUserId,
                newSchedule.Revision,
                Status = entity.Status.ToString(),
                CorrelationId = request.IdempotencyKey.Trim(),
                UnresolvedCredential = credentialStatuses.Count(x => x.Value == OprCredentialCheckStatus.NotAvailable)
            });
        return (await GetScheduleResponseAsync(caseId, cancellationToken))!;
    }

    public async Task<OprCaseStatusResponse> PostponeAsync(Guid caseId, PostponeOprCaseRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.IdempotencyKey)) throw new ArgumentException("Idempotency key wajib diisi.");
        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new OperatingRoomUnprocessableException("MissingPostponeReason", "Alasan penundaan wajib diisi.");

        var actorUserId = GetUserId(_httpContextAccessor);
        var reason = request.Reason.Trim();
        var fingerprint = Hash(string.Join('|', reason, request.ConfirmedByDoctorId));

        var prior = await FindIdempotentAsync(PostponeActions, request.IdempotencyKey, cancellationToken);
        if (prior != null)
        {
            if (prior.OprCaseId != caseId)
                throw new OperatingRoomConflictException("OPR013", "Idempotency key sudah digunakan untuk kasus lain.");
            EnsureSameFingerprint(prior.Source, fingerprint);
            return (await GetStatusResponseAsync(caseId, prior.Reason, cancellationToken))!;
        }

        var entity = await LoadCaseAsync(caseId, cancellationToken)
            ?? throw new KeyNotFoundException("Kasus operasi tidak ditemukan.");
        if (entity.Status != OprCaseStatus.Requested && entity.Status != OprCaseStatus.Scheduled)
            throw new OperatingRoomConflictException("InvalidStateTransition",
                "Penundaan hanya dapat dilakukan pada kasus berstatus Requested atau Scheduled.");
        if (entity.Version != request.ExpectedVersion)
            throw new OperatingRoomConflictException("OPR012", "Data telah diperbarui pengguna lain. Muat ulang lalu coba kembali.");

        var confirmingDoctorValid = await _dbContext.Set<MstDoctor>().AsNoTracking()
            .AnyAsync(x => x.Id == request.ConfirmedByDoctorId && x.IsActive && !x.IsDelete, cancellationToken);
        if (!confirmingDoctorValid)
            throw new ArgumentException("Dokter yang mengonfirmasi penundaan tidak ditemukan atau tidak aktif.");

        var now = DateTime.UtcNow;
        RetireCurrentPlan(entity, actorUserId, now, reason);

        var fromStatus = entity.Status;
        entity.Status = OprCaseStatus.Postponed;
        entity.Version++;
        entity.UpdateDateTime = now;
        entity.UpdateBy = actorUserId;
        _dbContext.OprStatusHistories.Add(NewHistory(entity.Id, OprCaseStatus.Postponed, fromStatus, "Postpone",
            reason, request.IdempotencyKey, fingerprint, actorUserId, now));

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            _dbContext.ChangeTracker.Clear();
            var concurrent = await FindIdempotentAsync(PostponeActions, request.IdempotencyKey, cancellationToken);
            if (concurrent != null && concurrent.OprCaseId == caseId)
                return (await GetStatusResponseAsync(caseId, concurrent.Reason, cancellationToken))!;
            throw new OperatingRoomConflictException("OPR012", "Data telah diperbarui pengguna lain. Muat ulang lalu coba kembali.");
        }

        await _loggerService.AuditAsync(LogCategory, "OperatingRoomSchedule.Postpone", "Menunda kasus operasi.",
            new
            {
                entity.Id,
                entity.CaseNumber,
                ActorUserId = actorUserId,
                Status = entity.Status.ToString(),
                request.ConfirmedByDoctorId,
                CorrelationId = request.IdempotencyKey.Trim()
            });
        return (await GetStatusResponseAsync(caseId, reason, cancellationToken))!;
    }

    public Task<OprScheduleResponse?> GetCurrentScheduleAsync(Guid caseId, CancellationToken cancellationToken = default) =>
        GetScheduleResponseAsync(caseId, cancellationToken);

    /// <summary>
    /// Seluruh revisi jadwal satu kasus, terbaru lebih dulu. Jadwal lama sengaja tidak
    /// dihapus ketika direvisi (OPS-DEC-016), sehingga koordinator dapat menelusuri
    /// jadwal mana yang digeser beserta alasannya.
    /// </summary>
    public async Task<List<OprScheduleResponse>> GetScheduleHistoryAsync(Guid caseId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.OprCases.AsNoTracking()
            .Where(x => x.Id == caseId && !x.IsDelete)
            .Select(x => new { x.Id, x.CaseNumber, x.Status, x.Version })
            .FirstOrDefaultAsync(cancellationToken);
        if (entity == null) return [];

        var schedules = await _dbContext.OprSchedules.AsNoTracking()
            .Where(x => x.OprCaseId == caseId && !x.IsDelete)
            .Include(x => x.Room)
            .Include(x => x.TeamMembers.Where(t => !t.IsDelete)).ThenInclude(t => t.Workforce)
            .OrderByDescending(x => x.Revision)
            .ToListAsync(cancellationToken);

        return [.. schedules.Select(schedule => new OprScheduleResponse
        {
            Id = schedule.Id,
            OprCaseId = entity.Id,
            CaseNumber = entity.CaseNumber,
            RoomId = schedule.RoomId,
            RoomName = schedule.Room?.RoomName ?? string.Empty,
            StartAt = schedule.StartAt,
            EndAt = schedule.EndAt,
            BufferBeforeMinutes = schedule.BufferBeforeMinutes,
            BufferAfterMinutes = schedule.BufferAfterMinutes,
            Revision = schedule.Revision,
            ChangeReason = schedule.ChangeReason,
            IsCurrent = schedule.IsCurrent,
            Status = entity.Status,
            Version = entity.Version,
            TeamMembers = [.. schedule.TeamMembers.OrderBy(x => x.Role)
                .Select(x => new OprTeamMemberResponse
                {
                    WorkforceId = x.WorkforceId,
                    WorkforceName = x.Workforce?.DisplayName ?? string.Empty,
                    Role = x.Role,
                    IsLead = x.IsLead,
                    CredentialCheckStatus = x.CredentialCheckStatus,
                    CredentialCheckedAt = x.CredentialCheckedAt
                })],
            AvailableActions = AvailableActions(entity.Status)
        })];
    }

    /// <summary>Menonaktifkan jadwal dan tim berjalan; histori revisi tetap tersimpan.</summary>
    private static void RetireCurrentPlan(OprCase entity, Guid actorUserId, DateTime now, string? reason)
    {
        foreach (var schedule in entity.Schedules.Where(x => x.IsCurrent && !x.IsDelete))
        {
            schedule.IsCurrent = false;
            if (reason != null) schedule.ChangeReason = reason;
            schedule.UpdateDateTime = now;
            schedule.UpdateBy = actorUserId;
        }
        foreach (var member in entity.TeamMembers.Where(x => x.IsCurrent && !x.IsDelete))
        {
            member.IsCurrent = false;
            member.UpdateDateTime = now;
            member.UpdateBy = actorUserId;
        }
    }

    private static void ValidateScheduleRequest(ScheduleOprCaseRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.IdempotencyKey)) throw new ArgumentException("Idempotency key wajib diisi.");
        if (request.TeamMembers.Count == 0) throw new ArgumentException("Anggota tim wajib diisi.");
        if (request.TeamMembers.Any(x => x.WorkforceId == Guid.Empty))
            throw new ArgumentException("Anggota tim tidak valid.");
        if (request.TeamMembers.Select(x => (x.WorkforceId, x.Role)).Distinct().Count() != request.TeamMembers.Count)
            throw new ArgumentException("Anggota tim memiliki data ganda pada peran yang sama.");
        if (request.TeamMembers.Count(x => x.IsLead) != 1)
            throw new ArgumentException("Tentukan tepat satu ketua tim operasi.");
        if (request.TeamMembers.Single(x => x.IsLead).Role != OprTeamRole.PrimarySurgeon)
            throw new ArgumentException("Ketua tim operasi harus dokter bedah utama.");
    }

    private void ValidateWindow(DateTime startAt, DateTime endAt)
    {
        if (endAt <= startAt) throw new ArgumentException("Waktu selesai harus setelah waktu mulai.");
        var duration = (endAt - startAt).TotalMinutes;
        if (duration < _options.MinimumDurationMinutes)
            throw new ArgumentException($"Durasi jadwal minimal {_options.MinimumDurationMinutes} menit.");
        if (duration > _options.MaximumDurationMinutes)
            throw new ArgumentException($"Durasi jadwal maksimal {_options.MaximumDurationMinutes} menit.");
    }

    private async Task ValidateRoomAsync(Guid roomId, CancellationToken cancellationToken)
    {
        var roomValid = await _dbContext.Set<MstRoom>().AsNoTracking()
            .AnyAsync(x => x.Id == roomId && x.IsActive && !x.IsDelete && x.RoomType == RoomType.OperatingRoom,
                cancellationToken);
        if (!roomValid) throw new ArgumentException("Ruang operasi tidak ditemukan atau tidak aktif.");
    }

    private async Task ValidateTeamAsync(OprCase entity, IReadOnlyCollection<OprTeamMemberRequest> members,
        CancellationToken cancellationToken)
    {
        if (RequiredRoles.Any(role => members.All(x => x.Role != role)))
            throw new OperatingRoomUnprocessableException("OPR004",
                "Lengkapi dokter bedah, dokter anestesi, perawat instrumen, dan perawat sirkuler.");

        var workforceIds = members.Select(x => x.WorkforceId).Distinct().ToList();
        var activeWorkforce = await _dbContext.Set<MstWorkforceProfile>().AsNoTracking()
            .CountAsync(x => workforceIds.Contains(x.Id) && x.IsActive && !x.IsDelete, cancellationToken);
        if (activeWorkforce != workforceIds.Count)
            throw new OperatingRoomUnprocessableException("OPR005",
                "Anggota tim tidak aktif atau tidak memiliki kewenangan yang sesuai.");

        // Ketua tim harus tenaga yang sama dengan dokter bedah utama pada permintaan (OPS-DEC-004).
        // Bila relasi dokter ke workforce belum terisi, konsistensi ini tidak dapat diperiksa.
        var leadWorkforceId = members.Single(x => x.IsLead).WorkforceId;
        var surgeonWorkforceId = await _dbContext.Set<MstDoctor>().AsNoTracking()
            .Where(x => x.Id == entity.PrimarySurgeonId && !x.IsDelete)
            .Select(x => (Guid?)x.WorkforceProfileId).FirstOrDefaultAsync(cancellationToken);
        if (surgeonWorkforceId.HasValue && surgeonWorkforceId.Value != Guid.Empty && surgeonWorkforceId != leadWorkforceId)
            throw new OperatingRoomUnprocessableException("OPR004",
                "Ketua tim harus dokter bedah utama yang tercatat pada permintaan operasi.");
    }

    private async Task EnsureNoOverlapAsync(Guid caseId, Guid roomId, DateTime startAt, DateTime endAt,
        int bufferBefore, int bufferAfter, IReadOnlyCollection<Guid> workforceIds, CancellationToken cancellationToken)
    {
        var windowStart = startAt.AddMinutes(-bufferBefore);
        var windowEnd = endAt.AddMinutes(bufferAfter);
        // Prefilter kasar memakai batas buffer maksimum agar buffer milik jadwal lain ikut
        // terambil; irisan tepatnya dihitung di memori supaya bebas dari perbedaan provider.
        var coarseStart = windowStart.AddMinutes(-MaxBufferMinutes);
        var coarseEnd = windowEnd.AddMinutes(MaxBufferMinutes);

        var candidates = await _dbContext.OprSchedules.AsNoTracking()
            .Where(x => x.IsCurrent && !x.IsDelete && x.OprCaseId != caseId &&
                x.StartAt < coarseEnd && x.EndAt > coarseStart &&
                x.OprCase != null && !x.OprCase.IsDelete && !ClosedStatuses.Contains(x.OprCase.Status))
            .Select(x => new
            {
                x.RoomId,
                x.StartAt,
                x.EndAt,
                x.BufferBeforeMinutes,
                x.BufferAfterMinutes,
                TeamWorkforceIds = x.TeamMembers.Where(t => t.IsCurrent && !t.IsDelete).Select(t => t.WorkforceId).ToList()
            })
            .ToListAsync(cancellationToken);

        var conflicting = candidates.Any(x =>
            x.StartAt.AddMinutes(-x.BufferBeforeMinutes) < windowEnd &&
            x.EndAt.AddMinutes(x.BufferAfterMinutes) > windowStart &&
            (x.RoomId == roomId || x.TeamWorkforceIds.Any(workforceIds.Contains)));
        if (conflicting)
            throw new OperatingRoomConflictException("OPR003",
                "Ruang atau anggota tim sudah memiliki jadwal pada waktu tersebut.");
    }

    private Task<OprCase?> LoadCaseAsync(Guid caseId, CancellationToken cancellationToken) =>
        _dbContext.OprCases
            .Include(x => x.Schedules)
            .Include(x => x.TeamMembers)
            .Include(x => x.StatusHistories)
            .FirstOrDefaultAsync(x => x.Id == caseId && !x.IsDelete, cancellationToken);

    private async Task<OprScheduleResponse?> GetScheduleResponseAsync(Guid caseId, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.OprCases.AsNoTracking()
            .Include(x => x.Schedules.Where(s => s.IsCurrent && !s.IsDelete)).ThenInclude(x => x.Room)
            .Include(x => x.Schedules.Where(s => s.IsCurrent && !s.IsDelete)).ThenInclude(x => x.TeamMembers)
                .ThenInclude(x => x.Workforce)
            .FirstOrDefaultAsync(x => x.Id == caseId && !x.IsDelete, cancellationToken);
        var schedule = entity?.Schedules.FirstOrDefault();
        if (entity == null || schedule == null) return null;

        return new OprScheduleResponse
        {
            Id = schedule.Id,
            OprCaseId = entity.Id,
            CaseNumber = entity.CaseNumber,
            RoomId = schedule.RoomId,
            RoomName = schedule.Room?.RoomName ?? string.Empty,
            StartAt = schedule.StartAt,
            EndAt = schedule.EndAt,
            BufferBeforeMinutes = schedule.BufferBeforeMinutes,
            BufferAfterMinutes = schedule.BufferAfterMinutes,
            Revision = schedule.Revision,
            ChangeReason = schedule.ChangeReason,
            IsCurrent = schedule.IsCurrent,
            Status = entity.Status,
            Version = entity.Version,
            TeamMembers = schedule.TeamMembers.Where(x => !x.IsDelete).OrderBy(x => x.Role)
                .Select(x => new OprTeamMemberResponse
                {
                    WorkforceId = x.WorkforceId,
                    WorkforceName = x.Workforce?.DisplayName ?? string.Empty,
                    Role = x.Role,
                    IsLead = x.IsLead,
                    CredentialCheckStatus = x.CredentialCheckStatus,
                    CredentialCheckedAt = x.CredentialCheckedAt
                }).ToList(),
            AvailableActions = AvailableActions(entity.Status)
        };
    }

    private async Task<OprCaseStatusResponse?> GetStatusResponseAsync(Guid caseId, string? reason,
        CancellationToken cancellationToken)
    {
        var entity = await _dbContext.OprCases.AsNoTracking()
            .Where(x => x.Id == caseId && !x.IsDelete)
            .Select(x => new { x.Id, x.CaseNumber, x.Status, x.Version })
            .FirstOrDefaultAsync(cancellationToken);
        return entity == null ? null : new OprCaseStatusResponse
        {
            Id = entity.Id,
            CaseNumber = entity.CaseNumber,
            Status = entity.Status,
            Version = entity.Version,
            Reason = reason,
            AvailableActions = AvailableActions(entity.Status)
        };
    }

    private Task<OprStatusHistory?> FindIdempotentAsync(string[] actions, string idempotencyKey,
        CancellationToken cancellationToken) =>
        _dbContext.OprStatusHistories.AsNoTracking().FirstOrDefaultAsync(x => actions.Contains(x.Action) &&
            x.CorrelationId == idempotencyKey.Trim() && !x.IsDelete, cancellationToken);

    private static string BuildFingerprint(ScheduleOprCaseRequest r) => Hash(string.Join('|', r.RoomId,
        r.StartAt.ToUniversalTime().Ticks, r.EndAt.ToUniversalTime().Ticks, r.BufferBeforeMinutes, r.BufferAfterMinutes,
        Normalize(r.ChangeReason), string.Join(',', r.TeamMembers.OrderBy(x => x.WorkforceId).ThenBy(x => x.Role)
            .Select(x => $"{x.WorkforceId:N}:{(int)x.Role}:{x.IsLead}"))));
}
