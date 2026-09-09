using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Options;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using static QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services.OperatingRoomCommandSupport;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services;

/// <summary>
/// Anestesi, recovery, dan serah terima pasien beserta gerbang otomatis menuju `Completed`
/// (BE-OPR-007, OPS-REQ-006/008, OPS-DEC-012/019/021/025, `OPR-INT-005`).
/// </summary>
public sealed class OperatingRoomRecoveryService
{
    private const string AnesthesiaAction = "SaveAnesthesiaRecord";
    private const string RecoveryAction = "SaveRecovery";
    private const string HandoverSendAction = "SendHandover";
    private const string HandoverAcceptAction = "AcceptHandover";
    private const string CompleteAction = "CompleteCase";

    private static readonly string[] RecoveryActions =
        [AnesthesiaAction, RecoveryAction, HandoverSendAction, HandoverAcceptAction];

    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LoggerService _loggerService;

    private readonly OperatingRoomRuleRelaxation _relaxation;

    public OperatingRoomRecoveryService(ApplicationDbContext dbContext,
        IHttpContextAccessor httpContextAccessor, LoggerService loggerService,
        OperatingRoomRuleRelaxation relaxation)
    {
        _relaxation = relaxation;
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _loggerService = loggerService;
    }

    public async Task<OprAnesthesiaRecordResponse> SaveAnesthesiaRecordAsync(Guid caseId,
        SaveOprAnesthesiaRecordRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.IdempotencyKey)) throw new ArgumentException("Idempotency key wajib diisi.");
        var actorUserId = GetUserId(_httpContextAccessor);
        var fingerprint = Hash(string.Join('|', request.AssessmentSummary.Trim(), request.Technique.Trim(),
            request.MedicationFluidSummary.Trim(), request.AirwaySummary.Trim(), request.MonitoringSummary.Trim(),
            Normalize(request.EventSummary), request.FinalCondition.Trim(), request.Finalize));

        var prior = await FindIdempotentAsync(request.IdempotencyKey, cancellationToken);
        if (prior != null)
        {
            EnsureSameCase(prior, caseId);
            EnsureSameFingerprint(prior.Source, fingerprint);
            return (await GetAnesthesiaRecordAsync(caseId, cancellationToken))!;
        }

        var entity = await LoadCaseAsync(caseId, cancellationToken)
            ?? throw new KeyNotFoundException("Kasus operasi tidak ditemukan.");
        if (entity.Status != OprCaseStatus.InProgress)
            throw new OperatingRoomConflictException("InvalidStateTransition",
                "Catatan anestesi hanya dapat diisi selama operasi berlangsung.");
        await EnsureTeamRoleAsync(entity, actorUserId, [OprTeamRole.Anesthesiologist],
            "Hanya dokter anestesi pada tim operasi yang boleh mengisi catatan anestesi.", cancellationToken);

        var now = DateTime.UtcNow;
        var record = await _dbContext.OprAnesthesiaRecords
            .FirstOrDefaultAsync(x => x.OprCaseId == caseId && !x.IsDelete, cancellationToken);
        if (record == null)
        {
            record = new OprAnesthesiaRecord
            {
                OprCaseId = caseId, Status = OprRecordStatus.Draft, Version = 0,
                CreateDateTime = now, CreateBy = actorUserId
            };
            _dbContext.OprAnesthesiaRecords.Add(record);
        }
        else
        {
            if (record.Status == OprRecordStatus.Final)
                throw new OperatingRoomUnprocessableException("OPR010",
                    "Catatan final hanya dapat diperbaiki melalui addendum.");
            EnsureVersion(record.Version, request.ExpectedRecordVersion);
            record.UpdateDateTime = now;
            record.UpdateBy = actorUserId;
        }

        record.AssessmentSummary = request.AssessmentSummary.Trim();
        record.Technique = request.Technique.Trim();
        record.MedicationFluidSummary = request.MedicationFluidSummary.Trim();
        record.AirwaySummary = request.AirwaySummary.Trim();
        record.MonitoringSummary = request.MonitoringSummary.Trim();
        record.EventSummary = Normalize(request.EventSummary);
        record.FinalCondition = request.FinalCondition.Trim();
        record.Version++;

        if (request.Finalize)
        {
            var missing = new List<string>();
            if (string.IsNullOrWhiteSpace(record.AssessmentSummary)) missing.Add("asesmen praanestesi");
            if (string.IsNullOrWhiteSpace(record.Technique)) missing.Add("teknik anestesi");
            if (string.IsNullOrWhiteSpace(record.MedicationFluidSummary)) missing.Add("obat dan cairan");
            if (string.IsNullOrWhiteSpace(record.AirwaySummary)) missing.Add("pengelolaan jalan napas");
            if (string.IsNullOrWhiteSpace(record.MonitoringSummary)) missing.Add("pemantauan");
            if (string.IsNullOrWhiteSpace(record.FinalCondition)) missing.Add("kondisi akhir pasien");
            if (missing.Count > 0)
                throw new OperatingRoomUnprocessableException("AnesthesiaRecordIncomplete",
                    $"Lengkapi {string.Join(", ", missing)} sebelum catatan anestesi difinalisasi.");
            record.Status = OprRecordStatus.Final;
            record.FinalizedBy = actorUserId;
            record.FinalizedAt = now;
        }

        _dbContext.OprStatusHistories.Add(NewHistory(entity.Id, entity.Status, entity.Status, AnesthesiaAction,
            request.Finalize ? "Final" : "Draft", request.IdempotencyKey, fingerprint, actorUserId, now));
        await SaveAsync(cancellationToken);

        await _loggerService.AuditAsync(LogCategory, "OperatingRoomRecovery.SaveAnesthesiaRecord",
            "Menyimpan catatan anestesi.",
            new
            {
                entity.Id, entity.CaseNumber, ActorUserId = actorUserId, RecordStatus = record.Status.ToString(),
                record.Version, CorrelationId = request.IdempotencyKey.Trim()
            });
        return (await GetAnesthesiaRecordAsync(caseId, cancellationToken))!;
    }

    public Task<OprAnesthesiaRecordResponse?> GetAnesthesiaRecordAsync(Guid caseId,
        CancellationToken cancellationToken = default) =>
        _dbContext.OprAnesthesiaRecords.AsNoTracking()
            .Where(x => x.OprCaseId == caseId && !x.IsDelete)
            .Select(x => new OprAnesthesiaRecordResponse
            {
                Id = x.Id, OprCaseId = x.OprCaseId, Status = x.Status, AssessmentSummary = x.AssessmentSummary,
                Technique = x.Technique, MedicationFluidSummary = x.MedicationFluidSummary,
                AirwaySummary = x.AirwaySummary, MonitoringSummary = x.MonitoringSummary,
                EventSummary = x.EventSummary, FinalCondition = x.FinalCondition,
                FinalizedBy = x.FinalizedBy, FinalizedAt = x.FinalizedAt, Version = x.Version
            })
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<OprRecoveryResponse> SaveRecoveryAsync(Guid caseId, SaveOprRecoveryRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.IdempotencyKey)) throw new ArgumentException("Idempotency key wajib diisi.");
        if (string.IsNullOrWhiteSpace(request.ScoreSystem))
            throw new ArgumentException("Sistem penilaian recovery wajib diisi.");
        if (request.Observations.Select(x => x.Code.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Count()
            != request.Observations.Count)
            throw new ArgumentException("Kode pemantauan recovery memiliki data ganda.");

        var actorUserId = GetUserId(_httpContextAccessor);
        var fingerprint = Hash(string.Join('|', request.ScoreSystem.Trim(), request.ScoreValue, request.Status,
            request.Decision, Normalize(request.DecisionNote),
            string.Join(',', request.Observations.OrderBy(x => x.Code, StringComparer.Ordinal)
                .Select(x => $"{x.Code}:{Normalize(x.Value)}"))));

        var prior = await FindIdempotentAsync(request.IdempotencyKey, cancellationToken);
        if (prior != null)
        {
            EnsureSameCase(prior, caseId);
            EnsureSameFingerprint(prior.Source, fingerprint);
            return (await GetRecoveryAsync(caseId, cancellationToken))!;
        }

        var entity = await LoadCaseAsync(caseId, cancellationToken)
            ?? throw new KeyNotFoundException("Kasus operasi tidak ditemukan.");
        if (entity.Status != OprCaseStatus.InProgress)
            throw new OperatingRoomConflictException("InvalidStateTransition",
                "Recovery hanya dapat dicatat selama kasus masih berjalan.");
        // Keputusan pasien keluar recovery adalah kewenangan dokter anestesi (OPS-DEC-019).
        await EnsureTeamRoleAsync(entity, actorUserId, [OprTeamRole.Anesthesiologist],
            "Hanya dokter anestesi pada tim operasi yang boleh memutuskan recovery.", cancellationToken);
        if (request.Status == OprRecoveryStatus.Released && !request.Decision.HasValue)
            throw new OperatingRoomUnprocessableException("RecoveryDecisionRequired",
                "Tentukan tujuan pasien sebelum keluar dari ruang recovery.");

        var now = DateTime.UtcNow;
        var recovery = await _dbContext.OprRecoveries
            .FirstOrDefaultAsync(x => x.OprCaseId == caseId && !x.IsDelete, cancellationToken);
        if (recovery == null)
        {
            recovery = new OprRecovery
            {
                OprCaseId = caseId, Version = 0, CreateDateTime = now, CreateBy = actorUserId
            };
            _dbContext.OprRecoveries.Add(recovery);
        }
        else
        {
            if (recovery.Status == OprRecoveryStatus.Released)
                throw new OperatingRoomConflictException("InvalidStateTransition",
                    "Pasien sudah dinyatakan keluar dari ruang recovery.");
            EnsureVersion(recovery.Version, request.ExpectedRecordVersion);
            recovery.UpdateDateTime = now;
            recovery.UpdateBy = actorUserId;
        }

        recovery.ScoreSystem = request.ScoreSystem.Trim();
        recovery.ScoreValue = request.ScoreValue;
        recovery.ObservationJson = SerializeObservations(request.Observations);
        recovery.Status = request.Status;
        recovery.DecisionNote = Normalize(request.DecisionNote);
        recovery.Version++;
        if (request.Decision.HasValue) recovery.Decision = request.Decision.Value;
        if (request.Status == OprRecoveryStatus.Released)
        {
            recovery.ReleasedBy = actorUserId;
            recovery.ReleasedAt = now;
        }

        _dbContext.OprStatusHistories.Add(NewHistory(entity.Id, entity.Status, entity.Status, RecoveryAction,
            request.Status.ToString(), request.IdempotencyKey, fingerprint, actorUserId, now));
        await SaveAsync(cancellationToken);

        // Ringkasan klinis recovery tidak masuk log; hanya status dan tujuan yang aman.
        await _loggerService.AuditAsync(LogCategory, "OperatingRoomRecovery.SaveRecovery", "Menyimpan data recovery.",
            new
            {
                entity.Id, entity.CaseNumber, ActorUserId = actorUserId, RecoveryStatus = recovery.Status.ToString(),
                Decision = recovery.Decision.ToString(), CorrelationId = request.IdempotencyKey.Trim()
            });
        return (await GetRecoveryAsync(caseId, cancellationToken))!;
    }

    public async Task<OprRecoveryResponse?> GetRecoveryAsync(Guid caseId, CancellationToken cancellationToken = default)
    {
        var recovery = await _dbContext.OprRecoveries.AsNoTracking()
            .FirstOrDefaultAsync(x => x.OprCaseId == caseId && !x.IsDelete, cancellationToken);
        return recovery == null ? null : new OprRecoveryResponse
        {
            Id = recovery.Id, OprCaseId = recovery.OprCaseId, Status = recovery.Status,
            ScoreSystem = recovery.ScoreSystem, ScoreValue = recovery.ScoreValue,
            Observations = DeserializeObservations(recovery.ObservationJson),
            Decision = recovery.Status == OprRecoveryStatus.Monitoring && recovery.Decision == default
                ? null : recovery.Decision,
            DecisionNote = recovery.DecisionNote, ReleasedBy = recovery.ReleasedBy,
            ReleasedAt = recovery.ReleasedAt, Version = recovery.Version
        };
    }

    public async Task<OprHandoverResponse> CreateHandoverAsync(Guid caseId, CreateOprHandoverRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.IdempotencyKey)) throw new ArgumentException("Idempotency key wajib diisi.");
        if (string.IsNullOrWhiteSpace(request.ConditionSummary))
            throw new ArgumentException("Ringkasan kondisi pasien wajib diisi.");

        var actorUserId = GetUserId(_httpContextAccessor);
        var fingerprint = Hash(string.Join('|', request.DestinationUnitId, request.ConditionSummary.Trim(),
            Normalize(request.DeviceTherapySummary), Normalize(request.RiskSummary),
            Normalize(request.InstructionSummary)));

        var prior = await FindIdempotentAsync(request.IdempotencyKey, cancellationToken);
        if (prior != null)
        {
            EnsureSameCase(prior, caseId);
            EnsureSameFingerprint(prior.Source, fingerprint);
            return (await GetCurrentHandoverAsync(caseId, cancellationToken))!;
        }

        var entity = await LoadCaseAsync(caseId, cancellationToken)
            ?? throw new KeyNotFoundException("Kasus operasi tidak ditemukan.");
        if (entity.Status != OprCaseStatus.InProgress)
            throw new OperatingRoomConflictException("InvalidStateTransition",
                "Serah terima hanya dapat dikirim selama kasus masih berjalan.");
        await EnsureTeamRoleAsync(entity, actorUserId,
            [OprTeamRole.PrimarySurgeon, OprTeamRole.Anesthesiologist, OprTeamRole.ScrubNurse, OprTeamRole.CirculatingNurse],
            "Hanya anggota tim operasi yang boleh mengirim serah terima.", cancellationToken);

        var recovery = await _dbContext.OprRecoveries.AsNoTracking()
            .FirstOrDefaultAsync(x => x.OprCaseId == caseId && !x.IsDelete, cancellationToken);
        if (recovery == null || recovery.Status != OprRecoveryStatus.Released)
            throw new OperatingRoomUnprocessableException("RecoveryNotReleased",
                "Pasien belum dinyatakan keluar dari ruang recovery.");

        var destinationValid = await _dbContext.Set<MstServiceUnit>().AsNoTracking()
            .AnyAsync(x => x.Id == request.DestinationUnitId && x.IsActive && !x.IsDelete, cancellationToken);
        if (!destinationValid) throw new ArgumentException("Unit tujuan tidak ditemukan atau tidak aktif.");

        var handovers = await _dbContext.OprHandovers
            .Where(x => x.OprCaseId == caseId && !x.IsDelete).ToListAsync(cancellationToken);
        if (handovers.Any(x => x.Status is OprHandoverStatus.Sent or OprHandoverStatus.Accepted))
            throw new OperatingRoomConflictException("OPR011",
                "Serah terima sedang menunggu penerimaan unit tujuan atau sudah diterima.");

        var now = DateTime.UtcNow;
        var handover = new OprHandover
        {
            OprCaseId = caseId, DestinationUnitId = request.DestinationUnitId, Status = OprHandoverStatus.Sent,
            ConditionSummary = request.ConditionSummary.Trim(),
            DeviceTherapySummary = Normalize(request.DeviceTherapySummary),
            RiskSummary = Normalize(request.RiskSummary),
            InstructionSummary = Normalize(request.InstructionSummary),
            SentBy = actorUserId, SentAt = now,
            Revision = (handovers.Select(x => (int?)x.Revision).Max() ?? 0) + 1,
            CreateDateTime = now, CreateBy = actorUserId
        };
        _dbContext.OprHandovers.Add(handover);
        _dbContext.OprStatusHistories.Add(NewHistory(entity.Id, entity.Status, entity.Status, HandoverSendAction,
            $"Revision:{handover.Revision}", request.IdempotencyKey, fingerprint, actorUserId, now));
        await SaveAsync(cancellationToken);

        await _loggerService.AuditAsync(LogCategory, "OperatingRoomRecovery.SendHandover", "Mengirim serah terima pasien.",
            new
            {
                entity.Id, entity.CaseNumber, ActorUserId = actorUserId, handover.DestinationUnitId,
                handover.Revision, CorrelationId = request.IdempotencyKey.Trim()
            });
        return (await GetCurrentHandoverAsync(caseId, cancellationToken))!;
    }

    public async Task<OprCaseStatusResponse> AcceptHandoverAsync(Guid caseId, Guid handoverId,
        AcceptOprHandoverRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.IdempotencyKey)) throw new ArgumentException("Idempotency key wajib diisi.");
        if (!request.Accept && string.IsNullOrWhiteSpace(request.RejectionReason))
            throw new OperatingRoomUnprocessableException("RejectionReasonRequired",
                "Alasan penolakan serah terima wajib diisi.");

        var actorUserId = GetUserId(_httpContextAccessor);
        var fingerprint = Hash(string.Join('|', handoverId, request.Accept, Normalize(request.RejectionReason)));

        var prior = await FindIdempotentAsync(request.IdempotencyKey, cancellationToken);
        if (prior != null)
        {
            EnsureSameCase(prior, caseId);
            EnsureSameFingerprint(prior.Source, fingerprint);
            return (await GetStatusResponseAsync(caseId, cancellationToken))!;
        }

        var entity = await LoadCaseAsync(caseId, cancellationToken)
            ?? throw new KeyNotFoundException("Kasus operasi tidak ditemukan.");
        var handover = await _dbContext.OprHandovers
            .FirstOrDefaultAsync(x => x.Id == handoverId && x.OprCaseId == caseId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Serah terima tidak ditemukan.");
        if (handover.Status != OprHandoverStatus.Sent)
            throw new OperatingRoomConflictException("InvalidStateTransition",
                "Serah terima ini sudah diproses unit tujuan.");

        var now = DateTime.UtcNow;
        if (request.Accept)
        {
            handover.Status = OprHandoverStatus.Accepted;
            handover.ReceivedBy = actorUserId;
            handover.AcceptedAt = now;
        }
        else
        {
            handover.Status = OprHandoverStatus.Rejected;
            handover.RejectionReason = request.RejectionReason!.Trim();
        }
        handover.UpdateDateTime = now;
        handover.UpdateBy = actorUserId;

        _dbContext.OprStatusHistories.Add(NewHistory(entity.Id, entity.Status, entity.Status, HandoverAcceptAction,
            request.Accept ? "Accepted" : "Rejected", request.IdempotencyKey, fingerprint, actorUserId, now));

        if (request.Accept)
            await EvaluateCompletionAsync(entity, actorUserId, now, request.IdempotencyKey, cancellationToken);
        await SaveAsync(cancellationToken);

        await _loggerService.AuditAsync(LogCategory, "OperatingRoomRecovery.AcceptHandover",
            "Memproses penerimaan serah terima pasien.",
            new
            {
                entity.Id, entity.CaseNumber, ActorUserId = actorUserId, HandoverId = handoverId,
                HandoverStatus = handover.Status.ToString(), Status = entity.Status.ToString(),
                CorrelationId = request.IdempotencyKey.Trim()
            });
        return (await GetStatusResponseAsync(caseId, cancellationToken))!;
    }

    public Task<OprHandoverResponse?> GetCurrentHandoverAsync(Guid caseId, CancellationToken cancellationToken = default) =>
        _dbContext.OprHandovers.AsNoTracking()
            .Where(x => x.OprCaseId == caseId && !x.IsDelete)
            .OrderByDescending(x => x.Revision)
            .Select(x => new OprHandoverResponse
            {
                Id = x.Id, OprCaseId = x.OprCaseId, DestinationUnitId = x.DestinationUnitId,
                DestinationUnitName = string.Empty, Status = x.Status, ConditionSummary = x.ConditionSummary,
                DeviceTherapySummary = x.DeviceTherapySummary, RiskSummary = x.RiskSummary,
                InstructionSummary = x.InstructionSummary, SentBy = x.SentBy, SentAt = x.SentAt,
                ReceivedBy = x.ReceivedBy, AcceptedAt = x.AcceptedAt, RejectionReason = x.RejectionReason,
                Revision = x.Revision,
                CaseStatus = x.OprCase != null ? x.OprCase.Status : OprCaseStatus.InProgress
            })
            .FirstOrDefaultAsync(cancellationToken);

    /// <summary>
    /// `Completed` hanya tercapai setelah catatan operasi final, pasien keluar recovery, dan
    /// serah terima diterima unit tujuan (OPS-DEC-025, `OPR011`).
    /// </summary>
    private async Task EvaluateCompletionAsync(OprCase entity, Guid actorUserId, DateTime now,
        string idempotencyKey, CancellationToken cancellationToken)
    {
        if (entity.Status != OprCaseStatus.InProgress) return;

        var executionFinal = await _dbContext.OprExecutionRecords.AsNoTracking()
            .AnyAsync(x => x.OprCaseId == entity.Id && !x.IsDelete && x.Status == OprRecordStatus.Final,
                cancellationToken);
        if (!executionFinal) return;

        var recoveryReleased = await _dbContext.OprRecoveries.AsNoTracking()
            .AnyAsync(x => x.OprCaseId == entity.Id && !x.IsDelete && x.Status == OprRecoveryStatus.Released,
                cancellationToken);
        if (!recoveryReleased) return;

        // Serah terima yang baru diterima pada perintah ini masih tertahan di change tracker.
        var handoverAccepted = _dbContext.OprHandovers.Local
            .Any(x => x.OprCaseId == entity.Id && !x.IsDelete && x.Status == OprHandoverStatus.Accepted) ||
            await _dbContext.OprHandovers.AsNoTracking()
                .AnyAsync(x => x.OprCaseId == entity.Id && !x.IsDelete && x.Status == OprHandoverStatus.Accepted,
                    cancellationToken);
        if (!handoverAccepted) return;

        entity.Status = OprCaseStatus.Completed;
        entity.Outcome ??= OprCaseOutcome.Completed;
        entity.Version++;
        entity.UpdateDateTime = now;
        entity.UpdateBy = actorUserId;
        _dbContext.OprStatusHistories.Add(NewHistory(entity.Id, OprCaseStatus.Completed, OprCaseStatus.InProgress,
            CompleteAction, entity.Outcome?.ToString(), idempotencyKey, Hash(CompleteAction + entity.Id),
            actorUserId, now));
    }

    private async Task EnsureTeamRoleAsync(OprCase entity, Guid actorUserId, OprTeamRole[] allowedRoles,
        string message, CancellationToken cancellationToken)
    {
        // Dilepas saat aturan klinis dilonggarkan: siapa pun boleh mencatat recovery dan
        // serah terima tanpa perlu terdaftar sebagai anggota tim.
        if (_relaxation.IsRelaxed) return;

        var workforceId = await _dbContext.Users.AsNoTracking()
            .Where(x => x.Id == actorUserId).Select(x => x.WorkforceProfileId)
            .FirstOrDefaultAsync(cancellationToken);
        if (!workforceId.HasValue || workforceId.Value == Guid.Empty)
            throw new OperatingRoomForbiddenException("Akun pengguna tidak terhubung dengan data tenaga.");
        var allowed = entity.TeamMembers.Any(x => x.IsCurrent && !x.IsDelete &&
            x.WorkforceId == workforceId.Value && allowedRoles.Contains(x.Role));
        if (!allowed) throw new OperatingRoomForbiddenException(message);
    }

    private Task<OprCase?> LoadCaseAsync(Guid caseId, CancellationToken cancellationToken) =>
        _dbContext.OprCases
            .Include(x => x.TeamMembers)
            .FirstOrDefaultAsync(x => x.Id == caseId && !x.IsDelete, cancellationToken);

    private Task<OprStatusHistory?> FindIdempotentAsync(string idempotencyKey, CancellationToken cancellationToken) =>
        _dbContext.OprStatusHistories.AsNoTracking().FirstOrDefaultAsync(x =>
            RecoveryActions.Contains(x.Action) && x.CorrelationId == idempotencyKey.Trim() && !x.IsDelete,
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

    private static string SerializeObservations(IReadOnlyCollection<OprRecoveryObservationRequest> observations) =>
        JsonSerializer.Serialize(new
        {
            observations = observations.Select(x => new
            {
                code = x.Code.Trim(), label = x.Label.Trim(), value = Normalize(x.Value), recordedAt = x.RecordedAt
            })
        });

    private static List<OprRecoveryObservationResponse> DeserializeObservations(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            if (!document.RootElement.TryGetProperty("observations", out var items)) return [];
            return [.. items.EnumerateArray().Select(x => new OprRecoveryObservationResponse
            {
                Code = x.TryGetProperty("code", out var code) ? code.GetString() ?? string.Empty : string.Empty,
                Label = x.TryGetProperty("label", out var label) ? label.GetString() ?? string.Empty : string.Empty,
                Value = x.TryGetProperty("value", out var value) ? value.GetString() : null,
                RecordedAt = x.TryGetProperty("recordedAt", out var at) && at.ValueKind != JsonValueKind.Null
                    ? at.GetDateTime() : null
            })];
        }
        catch (JsonException)
        {
            return [];
        }
    }

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
