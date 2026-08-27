using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using static QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services.OperatingRoomCommandSupport;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services;

/// <summary>
/// Pelaksanaan operasi: mulai, catatan operasi, finalisasi, addendum, dan pembatalan sebelum
/// operasi dimulai (BE-OPR-006, OPS-REQ-006/009, OPS-DEC-008/010/011/019/022).
/// </summary>
public sealed class OperatingRoomExecutionService
{
    private const string StartAction = "Start";
    private const string CancelAction = "Cancel";
    private const string RecordAction = "SaveExecutionRecord";
    private const string FinalizeAction = "FinalizeExecutionRecord";
    private const string AddendumAction = "ExecutionAddendum";

    private static readonly string[] ExecutionActions =
        [StartAction, CancelAction, RecordAction, FinalizeAction, AddendumAction];

    private static readonly OprCaseStatus[] CancellableStatuses =
        [OprCaseStatus.Requested, OprCaseStatus.Scheduled, OprCaseStatus.Ready];

    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LoggerService _loggerService;
    private readonly OperatingRoomIntegrationService _integrationService;

    public OperatingRoomExecutionService(ApplicationDbContext dbContext,
        IHttpContextAccessor httpContextAccessor, LoggerService loggerService,
        OperatingRoomIntegrationService integrationService)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _loggerService = loggerService;
        _integrationService = integrationService;
    }

    public async Task<OprCaseStatusResponse> StartAsync(Guid caseId, StartOprCaseRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.IdempotencyKey)) throw new ArgumentException("Idempotency key wajib diisi.");
        var actorUserId = GetUserId(_httpContextAccessor);
        var fingerprint = Hash(string.Join('|', request.ConfirmedPatientIdentity, request.ConfirmedProcedure,
            Normalize(request.Notes)));

        var prior = await FindIdempotentAsync(request.IdempotencyKey, cancellationToken);
        if (prior != null)
        {
            EnsureSameCase(prior, caseId);
            EnsureSameFingerprint(prior.Source, fingerprint);
            return (await GetStatusResponseAsync(caseId, cancellationToken))!;
        }

        var entity = await LoadCaseAsync(caseId, cancellationToken)
            ?? throw new KeyNotFoundException("Kasus operasi tidak ditemukan.");
        if (entity.Status != OprCaseStatus.Ready)
            throw new OperatingRoomConflictException("InvalidStateTransition",
                "Operasi hanya dapat dimulai pada kasus berstatus Ready.");
        EnsureVersion(entity.Version, request.ExpectedVersion);
        await EnsurePrimarySurgeonAsync(entity, actorUserId, cancellationToken);
        if (!request.ConfirmedPatientIdentity || !request.ConfirmedProcedure)
            throw new OperatingRoomUnprocessableException("StartNotConfirmed",
                "Konfirmasi identitas pasien dan tindakan wajib dilakukan sebelum operasi dimulai.");

        var now = DateTime.UtcNow;
        _dbContext.OprExecutionRecords.Add(new OprExecutionRecord
        {
            OprCaseId = entity.Id, Status = OprRecordStatus.Draft, StartedAt = now, Version = 0,
            CreateDateTime = now, CreateBy = actorUserId
        });

        entity.Status = OprCaseStatus.InProgress;
        entity.Version++;
        entity.UpdateDateTime = now;
        entity.UpdateBy = actorUserId;
        _dbContext.OprStatusHistories.Add(NewHistory(entity.Id, OprCaseStatus.InProgress, OprCaseStatus.Ready,
            StartAction, Normalize(request.Notes), request.IdempotencyKey, fingerprint, actorUserId, now));
        await SaveAsync(cancellationToken);

        await _loggerService.AuditAsync(LogCategory, "OperatingRoomExecution.Start", "Memulai operasi.",
            new
            {
                entity.Id, entity.CaseNumber, ActorUserId = actorUserId, Status = entity.Status.ToString(),
                CorrelationId = request.IdempotencyKey.Trim()
            });
        return (await GetStatusResponseAsync(caseId, cancellationToken))!;
    }

    public async Task<OprCaseStatusResponse> CancelAsync(Guid caseId, CancelOprCaseRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.IdempotencyKey)) throw new ArgumentException("Idempotency key wajib diisi.");
        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new OperatingRoomUnprocessableException("CancelReasonRequired",
                "Alasan klinis pembatalan wajib diisi.");

        var actorUserId = GetUserId(_httpContextAccessor);
        var reason = request.Reason.Trim();
        var fingerprint = Hash(reason);

        var prior = await FindIdempotentAsync(request.IdempotencyKey, cancellationToken);
        if (prior != null)
        {
            EnsureSameCase(prior, caseId);
            EnsureSameFingerprint(prior.Source, fingerprint);
            return (await GetStatusResponseAsync(caseId, cancellationToken))!;
        }

        var entity = await LoadCaseAsync(caseId, cancellationToken)
            ?? throw new KeyNotFoundException("Kasus operasi tidak ditemukan.");
        if (!CancellableStatuses.Contains(entity.Status))
            throw new OperatingRoomConflictException("InvalidStateTransition",
                "Kasus hanya dapat dibatalkan sebelum operasi dimulai.");
        EnsureVersion(entity.Version, request.ExpectedVersion);
        await EnsureSurgeonOrAnesthesiologistAsync(entity, actorUserId, cancellationToken);

        var now = DateTime.UtcNow;
        // Jadwal dan tim yang masih berjalan dilepas supaya ruang dan tenaga tidak
        // terus dianggap terpakai oleh pemeriksaan benturan jadwal.
        foreach (var schedule in entity.Schedules.Where(x => x.IsCurrent && !x.IsDelete))
        {
            schedule.IsCurrent = false;
            schedule.ChangeReason = reason;
            schedule.UpdateDateTime = now;
            schedule.UpdateBy = actorUserId;
        }
        foreach (var member in entity.TeamMembers.Where(x => x.IsCurrent && !x.IsDelete))
        {
            member.IsCurrent = false;
            member.UpdateDateTime = now;
            member.UpdateBy = actorUserId;
        }

        var fromStatus = entity.Status;
        entity.Status = OprCaseStatus.Cancelled;
        entity.Version++;
        entity.UpdateDateTime = now;
        entity.UpdateBy = actorUserId;
        _dbContext.OprStatusHistories.Add(NewHistory(entity.Id, OprCaseStatus.Cancelled, fromStatus, CancelAction,
            reason, request.IdempotencyKey, fingerprint, actorUserId, now));
        await SaveAsync(cancellationToken);

        await _loggerService.AuditAsync(LogCategory, "OperatingRoomExecution.Cancel", "Membatalkan kasus operasi.",
            new
            {
                entity.Id, entity.CaseNumber, ActorUserId = actorUserId, From = fromStatus.ToString(),
                Status = entity.Status.ToString(), CorrelationId = request.IdempotencyKey.Trim()
            });
        return (await GetStatusResponseAsync(caseId, cancellationToken))!;
    }

    public async Task<OprExecutionRecordResponse?> GetRecordAsync(Guid caseId, CancellationToken cancellationToken = default) =>
        await BuildRecordResponseAsync(caseId, cancellationToken);

    public async Task<OprExecutionRecordResponse> SaveRecordAsync(Guid caseId,
        SaveOprExecutionRecordRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.IdempotencyKey)) throw new ArgumentException("Idempotency key wajib diisi.");
        var actorUserId = GetUserId(_httpContextAccessor);
        var fingerprint = Hash(string.Join('|', request.PreDiagnosis.Trim(), request.PostDiagnosis.Trim(),
            request.Findings.Trim(), request.Technique.Trim(), Normalize(request.Complications), request.BloodLossMl,
            Normalize(request.SpecimenNote), Normalize(request.ImplantDrainNote), request.PostPlan.Trim(),
            request.Finalize, request.Outcome, request.FinishedAt?.ToUniversalTime().Ticks));

        var prior = await FindIdempotentAsync(request.IdempotencyKey, cancellationToken);
        if (prior != null)
        {
            EnsureSameCase(prior, caseId);
            EnsureSameFingerprint(prior.Source, fingerprint);
            return (await BuildRecordResponseAsync(caseId, cancellationToken))!;
        }

        var entity = await LoadCaseAsync(caseId, cancellationToken)
            ?? throw new KeyNotFoundException("Kasus operasi tidak ditemukan.");
        if (entity.Status != OprCaseStatus.InProgress)
            throw new OperatingRoomConflictException("InvalidStateTransition",
                "Catatan operasi hanya dapat diisi selama operasi berlangsung.");
        await EnsurePrimarySurgeonAsync(entity, actorUserId, cancellationToken);

        var record = await _dbContext.OprExecutionRecords
            .FirstOrDefaultAsync(x => x.OprCaseId == caseId && !x.IsDelete, cancellationToken)
            ?? throw new OperatingRoomConflictException("InvalidStateTransition",
                "Catatan operasi belum dibuat; mulai operasi terlebih dahulu.");
        if (record.Status == OprRecordStatus.Final)
            throw new OperatingRoomUnprocessableException("OPR010",
                "Catatan final hanya dapat diperbaiki melalui addendum.");
        EnsureVersion(record.Version, request.ExpectedRecordVersion);

        var now = DateTime.UtcNow;
        record.PreDiagnosis = request.PreDiagnosis.Trim();
        record.PostDiagnosis = request.PostDiagnosis.Trim();
        record.Findings = request.Findings.Trim();
        record.Technique = request.Technique.Trim();
        record.Complications = Normalize(request.Complications);
        record.BloodLossMl = request.BloodLossMl;
        record.SpecimenNote = Normalize(request.SpecimenNote);
        record.ImplantDrainNote = Normalize(request.ImplantDrainNote);
        record.PostPlan = request.PostPlan.Trim();
        record.Version++;
        record.UpdateDateTime = now;
        record.UpdateBy = actorUserId;

        if (request.Finalize)
        {
            EnsureFinalizable(record, request);
            var finishedAt = request.FinishedAt?.ToUniversalTime() ?? now;
            if (finishedAt < record.StartedAt)
                throw new ArgumentException("Waktu selesai tidak boleh mendahului waktu mulai operasi.");
            record.Status = OprRecordStatus.Final;
            record.FinishedAt = finishedAt;
            record.FinalizedBy = actorUserId;
            record.FinalizedAt = now;
            // Outcome disimpan pada kasus; penghentian dini tetap menuju `Completed`
            // setelah syarat keselamatan pada BE-OPR-007 terpenuhi.
            entity.Outcome = request.Outcome;
            entity.Version++;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            // Layanan aktual selesai memicu penyerahan tagihan ke Billing (`OPR-INT-002`).
            await _integrationService.StageChargeDeliveryAsync(entity.Id, "procedure", record.Version,
                actorUserId, now, cancellationToken);
        }

        _dbContext.OprStatusHistories.Add(NewHistory(entity.Id, entity.Status, entity.Status,
            request.Finalize ? FinalizeAction : RecordAction,
            request.Finalize ? request.Outcome?.ToString() : "Draft",
            request.IdempotencyKey, fingerprint, actorUserId, now));
        await SaveAsync(cancellationToken);

        await _loggerService.AuditAsync(LogCategory, "OperatingRoomExecution.SaveRecord",
            "Menyimpan catatan operasi.",
            new
            {
                entity.Id, entity.CaseNumber, ActorUserId = actorUserId, RecordStatus = record.Status.ToString(),
                Outcome = entity.Outcome?.ToString(), record.Version, CorrelationId = request.IdempotencyKey.Trim()
            });
        return (await BuildRecordResponseAsync(caseId, cancellationToken))!;
    }

    public async Task<OprExecutionAddendumResponse> CreateAddendumAsync(Guid caseId,
        CreateOprExecutionAddendumRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.IdempotencyKey)) throw new ArgumentException("Idempotency key wajib diisi.");
        if (string.IsNullOrWhiteSpace(request.Content) || string.IsNullOrWhiteSpace(request.Reason))
            throw new ArgumentException("Isi dan alasan addendum wajib diisi.");

        var actorUserId = GetUserId(_httpContextAccessor);
        var fingerprint = Hash(string.Join('|', request.Content.Trim(), request.Reason.Trim()));

        var prior = await FindIdempotentAsync(request.IdempotencyKey, cancellationToken);
        if (prior != null)
        {
            EnsureSameCase(prior, caseId);
            EnsureSameFingerprint(prior.Source, fingerprint);
            return (await LatestAddendumAsync(caseId, cancellationToken))!;
        }

        var entity = await LoadCaseAsync(caseId, cancellationToken)
            ?? throw new KeyNotFoundException("Kasus operasi tidak ditemukan.");
        await EnsurePrimarySurgeonAsync(entity, actorUserId, cancellationToken);

        var record = await _dbContext.OprExecutionRecords.AsNoTracking()
            .FirstOrDefaultAsync(x => x.OprCaseId == caseId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Catatan operasi tidak ditemukan.");
        if (record.Status != OprRecordStatus.Final)
            throw new OperatingRoomConflictException("InvalidStateTransition",
                "Addendum hanya dapat ditambahkan pada catatan operasi yang sudah final.");

        var now = DateTime.UtcNow;
        _dbContext.OprExecutionAddenda.Add(new OprExecutionAddendum
        {
            ExecutionRecordId = record.Id, Content = request.Content.Trim(), Reason = request.Reason.Trim(),
            AuthoredBy = actorUserId, AuthoredAt = now, CreateDateTime = now, CreateBy = actorUserId
        });
        _dbContext.OprStatusHistories.Add(NewHistory(entity.Id, entity.Status, entity.Status, AddendumAction,
            "Addendum", request.IdempotencyKey, fingerprint, actorUserId, now));
        await SaveAsync(cancellationToken);

        await _loggerService.AuditAsync(LogCategory, "OperatingRoomExecution.Addendum",
            "Menambah addendum catatan operasi.",
            new
            {
                entity.Id, entity.CaseNumber, ActorUserId = actorUserId, ExecutionRecordId = record.Id,
                CorrelationId = request.IdempotencyKey.Trim()
            });
        return (await LatestAddendumAsync(caseId, cancellationToken))!;
    }

    private static void EnsureFinalizable(OprExecutionRecord record, SaveOprExecutionRecordRequest request)
    {
        if (!request.Outcome.HasValue)
            throw new OperatingRoomUnprocessableException("OutcomeRequired",
                "Tentukan hasil operasi sebelum catatan difinalisasi.");
        var missing = new List<string>();
        if (string.IsNullOrWhiteSpace(record.PreDiagnosis)) missing.Add("diagnosis praoperasi");
        if (string.IsNullOrWhiteSpace(record.PostDiagnosis)) missing.Add("diagnosis pascaoperasi");
        if (string.IsNullOrWhiteSpace(record.Findings)) missing.Add("temuan");
        if (string.IsNullOrWhiteSpace(record.Technique)) missing.Add("teknik");
        if (string.IsNullOrWhiteSpace(record.PostPlan)) missing.Add("rencana pascaoperasi");
        if (missing.Count > 0)
            throw new OperatingRoomUnprocessableException("ExecutionRecordIncomplete",
                $"Lengkapi {string.Join(", ", missing)} sebelum catatan operasi difinalisasi.");
    }

    /// <summary>Hanya dokter bedah utama pada tim berjalan yang boleh memulai dan mencatat operasi.</summary>
    private async Task EnsurePrimarySurgeonAsync(OprCase entity, Guid actorUserId, CancellationToken cancellationToken)
    {
        var workforceId = await ResolveWorkforceAsync(actorUserId, cancellationToken);
        var isSurgeon = entity.TeamMembers.Any(x => x.IsCurrent && !x.IsDelete &&
            x.WorkforceId == workforceId && x.Role == OprTeamRole.PrimarySurgeon);
        if (!isSurgeon)
            throw new OperatingRoomForbiddenException(
                "Hanya dokter bedah utama pada tim operasi yang berwenang atas tindakan ini.");
    }

    private async Task EnsureSurgeonOrAnesthesiologistAsync(OprCase entity, Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var workforceId = await ResolveWorkforceAsync(actorUserId, cancellationToken);
        var isAuthorized = entity.TeamMembers.Any(x => x.IsCurrent && !x.IsDelete && x.WorkforceId == workforceId &&
            (x.Role == OprTeamRole.PrimarySurgeon || x.Role == OprTeamRole.Anesthesiologist));
        // Kasus yang belum dijadwalkan belum punya tim; kewenangan jatuh pada dokter pemohon
        // atau dokter bedah utama yang tercatat pada permintaan.
        if (!isAuthorized && !entity.TeamMembers.Any(x => x.IsCurrent && !x.IsDelete))
        {
            var doctorWorkforceIds = await _dbContext.Set<MstDoctor>().AsNoTracking()
                .Where(x => (x.Id == entity.PrimarySurgeonId || x.Id == entity.RequesterDoctorId) && !x.IsDelete)
                .Select(x => x.WorkforceProfileId).ToListAsync(cancellationToken);
            isAuthorized = doctorWorkforceIds.Contains(workforceId);
        }
        if (!isAuthorized)
            throw new OperatingRoomForbiddenException(
                "Hanya dokter bedah atau dokter anestesi yang boleh membatalkan kasus operasi.");
    }

    private async Task<Guid> ResolveWorkforceAsync(Guid actorUserId, CancellationToken cancellationToken)
    {
        var workforceId = await _dbContext.Users.AsNoTracking()
            .Where(x => x.Id == actorUserId).Select(x => x.WorkforceProfileId)
            .FirstOrDefaultAsync(cancellationToken);
        if (!workforceId.HasValue || workforceId.Value == Guid.Empty)
            throw new OperatingRoomForbiddenException("Akun pengguna tidak terhubung dengan data tenaga.");
        return workforceId.Value;
    }

    private Task<OprCase?> LoadCaseAsync(Guid caseId, CancellationToken cancellationToken) =>
        _dbContext.OprCases
            .Include(x => x.Schedules)
            .Include(x => x.TeamMembers)
            .FirstOrDefaultAsync(x => x.Id == caseId && !x.IsDelete, cancellationToken);

    private Task<OprStatusHistory?> FindIdempotentAsync(string idempotencyKey, CancellationToken cancellationToken) =>
        _dbContext.OprStatusHistories.AsNoTracking().FirstOrDefaultAsync(x =>
            ExecutionActions.Contains(x.Action) && x.CorrelationId == idempotencyKey.Trim() && !x.IsDelete,
            cancellationToken);

    private async Task<OprCaseStatusResponse?> GetStatusResponseAsync(Guid caseId, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.OprCases.AsNoTracking()
            .Where(x => x.Id == caseId && !x.IsDelete)
            .Select(x => new { x.Id, x.CaseNumber, x.Status, x.Version })
            .FirstOrDefaultAsync(cancellationToken);
        return entity == null ? null : new OprCaseStatusResponse
        {
            Id = entity.Id, CaseNumber = entity.CaseNumber, Status = entity.Status, Version = entity.Version,
            AvailableActions = AvailableActions(entity.Status)
        };
    }

    private async Task<OprExecutionRecordResponse?> BuildRecordResponseAsync(Guid caseId, CancellationToken cancellationToken)
    {
        var record = await _dbContext.OprExecutionRecords.AsNoTracking()
            .Include(x => x.Addenda.Where(a => !a.IsDelete))
            .FirstOrDefaultAsync(x => x.OprCaseId == caseId && !x.IsDelete, cancellationToken);
        if (record == null) return null;
        var caseInfo = await _dbContext.OprCases.AsNoTracking()
            .Where(x => x.Id == caseId).Select(x => new { x.Status, x.Outcome })
            .FirstAsync(cancellationToken);

        return new OprExecutionRecordResponse
        {
            Id = record.Id, OprCaseId = record.OprCaseId, Status = record.Status,
            PreDiagnosis = record.PreDiagnosis, PostDiagnosis = record.PostDiagnosis, Findings = record.Findings,
            Technique = record.Technique, Complications = record.Complications, BloodLossMl = record.BloodLossMl,
            SpecimenNote = record.SpecimenNote, ImplantDrainNote = record.ImplantDrainNote, PostPlan = record.PostPlan,
            StartedAt = record.StartedAt, FinishedAt = record.FinishedAt, FinalizedBy = record.FinalizedBy,
            FinalizedAt = record.FinalizedAt, Version = record.Version,
            CaseStatus = caseInfo.Status, CaseOutcome = caseInfo.Outcome,
            Addenda = [.. record.Addenda.OrderBy(x => x.AuthoredAt).Select(x => new OprExecutionAddendumResponse
            {
                Id = x.Id, Content = x.Content, Reason = x.Reason, AuthoredBy = x.AuthoredBy, AuthoredAt = x.AuthoredAt
            })]
        };
    }

    private async Task<OprExecutionAddendumResponse?> LatestAddendumAsync(Guid caseId, CancellationToken cancellationToken) =>
        await _dbContext.OprExecutionAddenda.AsNoTracking()
            .Where(x => !x.IsDelete && x.ExecutionRecord != null && x.ExecutionRecord.OprCaseId == caseId)
            .OrderByDescending(x => x.AuthoredAt)
            .Select(x => new OprExecutionAddendumResponse
            {
                Id = x.Id, Content = x.Content, Reason = x.Reason, AuthoredBy = x.AuthoredBy, AuthoredAt = x.AuthoredAt
            })
            .FirstOrDefaultAsync(cancellationToken);

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            _dbContext.ChangeTracker.Clear();
            throw new OperatingRoomConflictException("OPR012",
                "Data telah diperbarui pengguna lain. Muat ulang lalu coba kembali.");
        }
    }
}
