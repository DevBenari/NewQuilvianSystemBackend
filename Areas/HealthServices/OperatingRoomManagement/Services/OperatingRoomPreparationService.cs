using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using static QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services.OperatingRoomCommandSupport;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services;

/// <summary>
/// Persiapan kasus operasi: consent, checklist keselamatan, sign-off kesiapan, jalur darurat,
/// dan gerbang otomatis menuju `Ready` (BE-OPR-005, OPS-REQ-005, OPS-DEC-005/006/018).
/// </summary>
public sealed class OperatingRoomPreparationService
{
    private const string SignOffAction = "ReadinessSignOff";
    private const string ChecklistAction = "SaveChecklist";
    private const string BypassAction = "EmergencyBypass";
    private const string ReadyAction = "CompleteReadiness";

    private static readonly string[] PreparationActions = [ChecklistAction, SignOffAction, BypassAction];

    private static readonly OprReadinessRole[] RequiredSignOffRoles =
    [
        OprReadinessRole.PrimarySurgeon, OprReadinessRole.Anesthesiologist, OprReadinessRole.Nurse
    ];

    private static readonly PatientConsentType[] RequiredConsentTypes =
    [
        PatientConsentType.Surgery, PatientConsentType.Anesthesia
    ];

    private static readonly PatientConsentStatus[] ValidConsentStatuses =
    [
        PatientConsentStatus.Signed, PatientConsentStatus.Verified, PatientConsentStatus.Approved
    ];

    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LoggerService _loggerService;

    public OperatingRoomPreparationService(ApplicationDbContext dbContext,
        IHttpContextAccessor httpContextAccessor, LoggerService loggerService)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _loggerService = loggerService;
    }

    public async Task<OprPreparationResponse?> GetAsync(Guid caseId, CancellationToken cancellationToken = default)
    {
        var entity = await LoadCaseAsync(caseId, cancellationToken);
        return entity == null ? null : await BuildResponseAsync(entity, cancellationToken);
    }

    public async Task<OprChecklistResponse> SaveChecklistAsync(Guid caseId, OprChecklistPhase phase,
        SaveOprChecklistRequest request, CancellationToken cancellationToken = default)
    {
        ValidateChecklistRequest(request);
        var actorUserId = GetUserId(_httpContextAccessor);
        var fingerprint = Hash(string.Join('|', (int)phase, request.TemplateVersion.Trim(), request.Complete,
            string.Join(',', request.Items.OrderBy(x => x.Code, StringComparer.Ordinal)
                .Select(x => $"{x.Code}:{x.IsMandatory}:{x.IsChecked}:{Normalize(x.Note)}"))));

        var prior = await FindIdempotentAsync(request.IdempotencyKey, cancellationToken);
        if (prior != null)
        {
            EnsureSameCaseAndFingerprint(prior, caseId, fingerprint);
            return (await GetChecklistResponseAsync(caseId, phase, cancellationToken))!;
        }

        var entity = await LoadCaseAsync(caseId, cancellationToken)
            ?? throw new KeyNotFoundException("Kasus operasi tidak ditemukan.");
        EnsurePhaseAllowed(entity.Status, phase);
        EnsureVersion(entity.Version, request.ExpectedVersion);

        var now = DateTime.UtcNow;
        var active = CurrentChecklist(entity, phase);
        // Checklist yang sudah final tidak diubah di tempat; perubahan membuat revisi baru
        // agar riwayat pemeriksaan sebelumnya tetap dapat ditelusuri.
        if (active == null || active.Status == OprChecklistStatus.Completed)
        {
            var previousRevision = entity.SafetyChecklists
                .Where(x => x.Phase == phase && !x.IsDelete).Select(x => (int?)x.Revision).Max() ?? 0;
            var replaced = active;
            active = new OprSafetyChecklist
            {
                OprCaseId = entity.Id, Phase = phase, Revision = previousRevision + 1,
                Status = OprChecklistStatus.Draft, CreateDateTime = now, CreateBy = actorUserId,
                IsEmergencyBypass = replaced?.IsEmergencyBypass ?? false,
                BypassReason = replaced?.BypassReason,
                BypassResponsibleUserId = replaced?.BypassResponsibleUserId
            };
            _dbContext.OprSafetyChecklists.Add(active);
            entity.SafetyChecklists.Add(active);
        }
        else
        {
            active.UpdateDateTime = now;
            active.UpdateBy = actorUserId;
        }

        active.TemplateVersion = request.TemplateVersion.Trim();
        active.ItemsJson = SerializeItems(request.Items);

        if (request.Complete)
        {
            var missing = request.Items.Where(x => x.IsMandatory && !x.IsChecked).ToList();
            if (missing.Count > 0 && !active.IsEmergencyBypass)
                throw new OperatingRoomUnprocessableException("OPR006", "Persiapan pasien belum lengkap.");
            active.Status = OprChecklistStatus.Completed;
            active.SignedByUserId = actorUserId;
            active.SignedAt = now;
            // Checklist darurat yang akhirnya dilengkapi menandai kapan pasien sudah stabil.
            if (active.IsEmergencyBypass && missing.Count == 0) active.CompletedAfterStableAt = now;
        }

        entity.Version++;
        entity.UpdateDateTime = now;
        entity.UpdateBy = actorUserId;
        _dbContext.OprStatusHistories.Add(NewHistory(entity.Id, entity.Status, entity.Status, ChecklistAction,
            $"{phase}:{(request.Complete ? "Completed" : "Draft")}", request.IdempotencyKey, fingerprint, actorUserId, now));

        await EvaluateReadinessAsync(entity, actorUserId, now, request.IdempotencyKey, cancellationToken);
        await SaveAsync(cancellationToken);

        await _loggerService.AuditAsync(LogCategory, "OperatingRoomPreparation.SaveChecklist",
            "Menyimpan checklist keselamatan operasi.",
            new
            {
                entity.Id, entity.CaseNumber, ActorUserId = actorUserId, Phase = phase.ToString(),
                active.Revision, Status = active.Status.ToString(), CorrelationId = request.IdempotencyKey.Trim()
            });
        return (await GetChecklistResponseAsync(caseId, phase, cancellationToken))!;
    }

    public async Task<OprPreparationResponse> CreateSignOffAsync(Guid caseId,
        CreateOprReadinessSignOffRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.IdempotencyKey)) throw new ArgumentException("Idempotency key wajib diisi.");
        var actorUserId = GetUserId(_httpContextAccessor);
        var fingerprint = Hash(string.Join('|', (int)request.Role, Normalize(request.Notes)));

        var prior = await FindIdempotentAsync(request.IdempotencyKey, cancellationToken);
        if (prior != null)
        {
            EnsureSameCaseAndFingerprint(prior, caseId, fingerprint);
            return (await GetAsync(caseId, cancellationToken))!;
        }

        var entity = await LoadCaseAsync(caseId, cancellationToken)
            ?? throw new KeyNotFoundException("Kasus operasi tidak ditemukan.");
        if (entity.Status != OprCaseStatus.Scheduled)
            throw new OperatingRoomConflictException("InvalidStateTransition",
                "Sign-off kesiapan hanya dapat diberikan pada kasus berstatus Scheduled.");
        EnsureVersion(entity.Version, request.ExpectedVersion);

        var existing = await ReadSignOffsAsync(caseId, cancellationToken);
        if (existing.Any(x => x.Role == request.Role))
            throw new OperatingRoomConflictException("OPR006", "Sign-off untuk peran tersebut sudah tercatat.");

        await EnsureSignOffAuthorityAsync(entity, request.Role, actorUserId, cancellationToken);

        var now = DateTime.UtcNow;
        entity.Version++;
        entity.UpdateDateTime = now;
        entity.UpdateBy = actorUserId;
        _dbContext.OprStatusHistories.Add(NewHistory(entity.Id, entity.Status, entity.Status, SignOffAction,
            BuildSignOffReason(request.Role, request.Notes), request.IdempotencyKey, fingerprint, actorUserId, now));

        await EvaluateReadinessAsync(entity, actorUserId, now, request.IdempotencyKey, cancellationToken);
        await SaveAsync(cancellationToken);

        await _loggerService.AuditAsync(LogCategory, "OperatingRoomPreparation.SignOff",
            "Mencatat sign-off kesiapan operasi.",
            new
            {
                entity.Id, entity.CaseNumber, ActorUserId = actorUserId, Role = request.Role.ToString(),
                Status = entity.Status.ToString(), CorrelationId = request.IdempotencyKey.Trim()
            });
        return (await GetAsync(caseId, cancellationToken))!;
    }

    public async Task<OprPreparationResponse> CreateEmergencyBypassAsync(Guid caseId,
        CreateOprEmergencyBypassRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.IdempotencyKey)) throw new ArgumentException("Idempotency key wajib diisi.");
        if (string.IsNullOrWhiteSpace(request.Reason) || request.ResponsibleUserId == Guid.Empty)
            throw new OperatingRoomUnprocessableException("OPR007",
                "Lengkapi alasan dan penanggung jawab jalur darurat.");

        var actorUserId = GetUserId(_httpContextAccessor);
        var reason = request.Reason.Trim();
        var fingerprint = Hash(string.Join('|', reason, request.ResponsibleUserId));

        var prior = await FindIdempotentAsync(request.IdempotencyKey, cancellationToken);
        if (prior != null)
        {
            EnsureSameCaseAndFingerprint(prior, caseId, fingerprint);
            return (await GetAsync(caseId, cancellationToken))!;
        }

        var entity = await LoadCaseAsync(caseId, cancellationToken)
            ?? throw new KeyNotFoundException("Kasus operasi tidak ditemukan.");
        if (entity.Status != OprCaseStatus.Scheduled)
            throw new OperatingRoomConflictException("InvalidStateTransition",
                "Jalur darurat hanya dapat dicatat pada kasus berstatus Scheduled.");
        EnsureVersion(entity.Version, request.ExpectedVersion);
        if (entity.CaseType != OprCaseType.Emergency && entity.Priority != OprPriority.Emergency)
            throw new OperatingRoomUnprocessableException("OPR007",
                "Jalur darurat hanya berlaku untuk kasus darurat.");

        var responsibleValid = await _dbContext.Users.AsNoTracking()
            .AnyAsync(x => x.Id == request.ResponsibleUserId, cancellationToken);
        if (!responsibleValid)
            throw new ArgumentException("Penanggung jawab jalur darurat tidak ditemukan.");

        var now = DateTime.UtcNow;
        var checklist = CurrentChecklist(entity, OprChecklistPhase.SignIn);
        if (checklist == null)
        {
            checklist = new OprSafetyChecklist
            {
                OprCaseId = entity.Id, Phase = OprChecklistPhase.SignIn, Revision = 1,
                Status = OprChecklistStatus.Draft, TemplateVersion = "EMERGENCY-BYPASS",
                ItemsJson = SerializeItems([]), CreateDateTime = now, CreateBy = actorUserId
            };
            _dbContext.OprSafetyChecklists.Add(checklist);
            entity.SafetyChecklists.Add(checklist);
        }
        else
        {
            checklist.UpdateDateTime = now;
            checklist.UpdateBy = actorUserId;
        }
        checklist.IsEmergencyBypass = true;
        checklist.BypassReason = reason;
        checklist.BypassResponsibleUserId = request.ResponsibleUserId;

        entity.Version++;
        entity.UpdateDateTime = now;
        entity.UpdateBy = actorUserId;
        _dbContext.OprStatusHistories.Add(NewHistory(entity.Id, entity.Status, entity.Status, BypassAction,
            "EmergencyBypass", request.IdempotencyKey, fingerprint, actorUserId, now));

        await EvaluateReadinessAsync(entity, actorUserId, now, request.IdempotencyKey, cancellationToken);
        await SaveAsync(cancellationToken);

        // Alasan klinis tidak masuk log; cukup penanda jalur darurat dan penanggung jawabnya.
        await _loggerService.AuditAsync(LogCategory, "OperatingRoomPreparation.EmergencyBypass",
            "Mencatat jalur darurat persiapan operasi.",
            new
            {
                entity.Id, entity.CaseNumber, ActorUserId = actorUserId, request.ResponsibleUserId,
                Status = entity.Status.ToString(), CorrelationId = request.IdempotencyKey.Trim()
            });
        return (await GetAsync(caseId, cancellationToken))!;
    }

    /// <summary>
    /// Menaikkan status menjadi `Ready` bila seluruh prasyarat terpenuhi. Dipanggil setiap
    /// perubahan persiapan sehingga transisi terjadi tepat satu kali.
    /// </summary>
    private async Task EvaluateReadinessAsync(OprCase entity, Guid actorUserId, DateTime now,
        string idempotencyKey, CancellationToken cancellationToken)
    {
        if (entity.Status != OprCaseStatus.Scheduled) return;

        var signOffs = await ReadSignOffsAsync(entity.Id, cancellationToken);
        var consents = await ReadConsentsAsync(entity, cancellationToken);
        var outstanding = BuildOutstanding(entity, signOffs, consents);
        if (outstanding.Count > 0) return;

        entity.Status = OprCaseStatus.Ready;
        _dbContext.OprStatusHistories.Add(NewHistory(entity.Id, OprCaseStatus.Ready, OprCaseStatus.Scheduled,
            ReadyAction, null, idempotencyKey, Hash(ReadyAction + entity.Id), actorUserId, now));
    }

    private List<string> BuildOutstanding(OprCase entity, IReadOnlyCollection<OprReadinessSignOffResponse> signOffs,
        IReadOnlyCollection<OprConsentStatusResponse> consents)
    {
        var outstanding = new List<string>();
        if (!entity.Schedules.Any(x => x.IsCurrent && !x.IsDelete))
            outstanding.Add("Jadwal aktif belum tersedia.");

        var bypass = IsBypassActive(entity);
        if (!bypass)
        {
            foreach (var consent in consents.Where(x => !x.IsValid))
                outstanding.Add(consent.ConsentType == PatientConsentType.Surgery
                    ? "Consent tindakan operasi belum sah."
                    : "Consent tindakan anestesi belum sah.");

            var signIn = CurrentChecklist(entity, OprChecklistPhase.SignIn);
            if (signIn == null || signIn.Status != OprChecklistStatus.Completed)
                outstanding.Add("Checklist verifikasi sebelum anestesi belum selesai.");
        }

        foreach (var role in RequiredSignOffRoles.Where(role => signOffs.All(x => x.Role != role)))
            outstanding.Add($"Sign-off {RoleLabel(role)} belum ada.");
        return outstanding;
    }

    private async Task<OprPreparationResponse> BuildResponseAsync(OprCase entity, CancellationToken cancellationToken)
    {
        var signOffs = await ReadSignOffsAsync(entity.Id, cancellationToken);
        var consents = await ReadConsentsAsync(entity, cancellationToken);
        return new OprPreparationResponse
        {
            OprCaseId = entity.Id,
            CaseNumber = entity.CaseNumber,
            Status = entity.Status,
            Version = entity.Version,
            Consents = [.. consents],
            Checklists = [.. Enum.GetValues<OprChecklistPhase>()
                .Select(phase => CurrentChecklist(entity, phase))
                .Where(x => x != null)
                .Select(x => MapChecklist(x!))],
            SignOffs = [.. signOffs],
            OutstandingRequirements = entity.Status == OprCaseStatus.Scheduled
                ? BuildOutstanding(entity, signOffs, consents)
                : [],
            IsEmergencyBypassActive = IsBypassActive(entity),
            AvailableActions = AvailableActions(entity.Status)
        };
    }

    /// <summary>
    /// Sign-off disimpan sebagai histori append-only, bukan tabel tersendiri. Baris yang baru
    /// ditambahkan pada perintah berjalan ikut dihitung supaya sign-off ketiga langsung
    /// menutup gerbang kesiapan dalam satu perintah.
    /// </summary>
    private async Task<List<OprReadinessSignOffResponse>> ReadSignOffsAsync(Guid caseId, CancellationToken cancellationToken)
    {
        var saved = await _dbContext.OprStatusHistories.AsNoTracking()
            .Where(x => x.OprCaseId == caseId && x.Action == SignOffAction && !x.IsDelete)
            .Select(x => new { x.Reason, x.ActorUserId, x.OccurredAt })
            .ToListAsync(cancellationToken);
        var pending = _dbContext.OprStatusHistories.Local
            .Where(x => x.OprCaseId == caseId && x.Action == SignOffAction && !x.IsDelete)
            .Select(x => new { x.Reason, x.ActorUserId, x.OccurredAt });
        var rows = saved.Concat(pending).OrderBy(x => x.OccurredAt).ToList();

        var result = new List<OprReadinessSignOffResponse>();
        foreach (var row in rows)
        {
            var (role, notes) = ParseSignOffReason(row.Reason);
            if (role == null || result.Any(x => x.Role == role.Value)) continue;
            result.Add(new OprReadinessSignOffResponse
            {
                Role = role.Value, ActorUserId = row.ActorUserId, SignedAt = row.OccurredAt, Notes = notes
            });
        }
        return result;
    }

    private async Task<List<OprConsentStatusResponse>> ReadConsentsAsync(OprCase entity, CancellationToken cancellationToken)
    {
        var procedureIds = entity.Procedures.Where(x => !x.IsDelete).Select(x => x.PatientProcedureId).ToList();
        var now = DateTime.UtcNow;
        var consents = await _dbContext.Set<TrxPatientConsent>().AsNoTracking()
            .Where(x => x.PatientId == entity.PatientId && !x.IsDelete &&
                RequiredConsentTypes.Contains(x.ConsentType) &&
                (x.EncounterId == entity.EncounterId ||
                    (x.PatientProcedureId.HasValue && procedureIds.Contains(x.PatientProcedureId.Value))))
            .Select(x => new
            {
                x.Id, x.ConsentNumber, x.ConsentType, x.ConsentStatus, x.ExpiredDate, x.SignedAt
            })
            .ToListAsync(cancellationToken);

        return [.. RequiredConsentTypes.Select(type =>
        {
            var candidate = consents
                .Where(x => x.ConsentType == type)
                .OrderByDescending(x => ValidConsentStatuses.Contains(x.ConsentStatus))
                .ThenByDescending(x => x.SignedAt ?? DateTime.MinValue)
                .FirstOrDefault();
            var valid = candidate != null && ValidConsentStatuses.Contains(candidate.ConsentStatus) &&
                (!candidate.ExpiredDate.HasValue || candidate.ExpiredDate.Value >= now);
            return new OprConsentStatusResponse
            {
                ConsentType = type, ConsentId = candidate?.Id, ConsentNumber = candidate?.ConsentNumber,
                ConsentStatus = candidate?.ConsentStatus, IsValid = valid
            };
        })];
    }

    private async Task EnsureSignOffAuthorityAsync(OprCase entity, OprReadinessRole role, Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var workforceId = await _dbContext.Users.AsNoTracking()
            .Where(x => x.Id == actorUserId).Select(x => x.WorkforceProfileId)
            .FirstOrDefaultAsync(cancellationToken);
        if (!workforceId.HasValue || workforceId.Value == Guid.Empty)
            throw new OperatingRoomForbiddenException("Akun pengguna tidak terhubung dengan data tenaga.");

        var allowedRoles = AllowedTeamRoles(role);
        var isMember = entity.TeamMembers.Any(x => x.IsCurrent && !x.IsDelete &&
            x.WorkforceId == workforceId.Value && allowedRoles.Contains(x.Role));
        if (!isMember)
            throw new OperatingRoomForbiddenException(
                $"Hanya {RoleLabel(role)} pada tim operasi yang boleh memberikan sign-off ini.");
    }

    private static OprTeamRole[] AllowedTeamRoles(OprReadinessRole role) => role switch
    {
        OprReadinessRole.PrimarySurgeon => [OprTeamRole.PrimarySurgeon],
        OprReadinessRole.Anesthesiologist => [OprTeamRole.Anesthesiologist],
        _ => [OprTeamRole.ScrubNurse, OprTeamRole.CirculatingNurse]
    };

    private static string RoleLabel(OprReadinessRole role) => role switch
    {
        OprReadinessRole.PrimarySurgeon => "dokter bedah utama",
        OprReadinessRole.Anesthesiologist => "dokter anestesi",
        _ => "perawat kamar operasi"
    };

    private static void EnsurePhaseAllowed(OprCaseStatus status, OprChecklistPhase phase)
    {
        var allowed = phase switch
        {
            OprChecklistPhase.SignIn => status is OprCaseStatus.Scheduled or OprCaseStatus.Ready,
            OprChecklistPhase.TimeOut => status is OprCaseStatus.Ready or OprCaseStatus.InProgress,
            _ => status == OprCaseStatus.InProgress
        };
        if (!allowed)
            throw new OperatingRoomConflictException("InvalidStateTransition",
                "Fase checklist tidak sesuai dengan status kasus saat ini.");
    }

    private static void ValidateChecklistRequest(SaveOprChecklistRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.IdempotencyKey)) throw new ArgumentException("Idempotency key wajib diisi.");
        if (string.IsNullOrWhiteSpace(request.TemplateVersion))
            throw new ArgumentException("Versi template checklist wajib diisi.");
        if (request.Items.Count == 0) throw new ArgumentException("Item checklist wajib diisi.");
        if (request.Items.Any(x => string.IsNullOrWhiteSpace(x.Code)) ||
            request.Items.Select(x => x.Code.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Count() != request.Items.Count)
            throw new ArgumentException("Kode item checklist tidak valid atau memiliki data ganda.");
    }

    private static bool IsBypassActive(OprCase entity)
    {
        var signIn = CurrentChecklist(entity, OprChecklistPhase.SignIn);
        return signIn is { IsEmergencyBypass: true };
    }

    private static OprSafetyChecklist? CurrentChecklist(OprCase entity, OprChecklistPhase phase) =>
        entity.SafetyChecklists.Where(x => x.Phase == phase && !x.IsDelete)
            .OrderByDescending(x => x.Revision).FirstOrDefault();

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

    private Task<OprCase?> LoadCaseAsync(Guid caseId, CancellationToken cancellationToken) =>
        _dbContext.OprCases
            .Include(x => x.Procedures)
            .Include(x => x.Schedules)
            .Include(x => x.TeamMembers)
            .Include(x => x.SafetyChecklists)
            .FirstOrDefaultAsync(x => x.Id == caseId && !x.IsDelete, cancellationToken);

    private async Task<OprChecklistResponse?> GetChecklistResponseAsync(Guid caseId, OprChecklistPhase phase,
        CancellationToken cancellationToken)
    {
        var checklist = await _dbContext.OprSafetyChecklists.AsNoTracking()
            .Where(x => x.OprCaseId == caseId && x.Phase == phase && !x.IsDelete)
            .OrderByDescending(x => x.Revision).FirstOrDefaultAsync(cancellationToken);
        return checklist == null ? null : MapChecklist(checklist);
    }

    private Task<OprStatusHistory?> FindIdempotentAsync(string idempotencyKey, CancellationToken cancellationToken) =>
        _dbContext.OprStatusHistories.AsNoTracking().FirstOrDefaultAsync(x =>
            PreparationActions.Contains(x.Action) && x.CorrelationId == idempotencyKey.Trim() && !x.IsDelete,
            cancellationToken);

    private static void EnsureSameCaseAndFingerprint(OprStatusHistory prior, Guid caseId, string fingerprint)
    {
        if (prior.OprCaseId != caseId)
            throw new OperatingRoomConflictException("OPR013", "Idempotency key sudah digunakan untuk kasus lain.");
        if (!string.Equals(prior.Source, BuildSource(fingerprint), StringComparison.Ordinal))
            throw new OperatingRoomConflictException("OPR013",
                "Idempotency key digunakan dengan isi permintaan yang berbeda.");
    }

    private static string BuildSignOffReason(OprReadinessRole role, string? notes) =>
        string.IsNullOrWhiteSpace(notes) ? role.ToString() : $"{role}|{notes.Trim()}";

    private static (OprReadinessRole? Role, string? Notes) ParseSignOffReason(string? reason)
    {
        if (string.IsNullOrWhiteSpace(reason)) return (null, null);
        var separator = reason.IndexOf('|');
        var rolePart = separator < 0 ? reason : reason[..separator];
        var notes = separator < 0 ? null : reason[(separator + 1)..];
        return Enum.TryParse<OprReadinessRole>(rolePart, out var role) ? (role, notes) : (null, null);
    }

    private static string SerializeItems(IReadOnlyCollection<OprChecklistItemRequest> items) =>
        JsonSerializer.Serialize(new
        {
            items = items.Select(x => new
            {
                code = x.Code.Trim(), label = x.Label.Trim(), isMandatory = x.IsMandatory,
                isChecked = x.IsChecked, note = Normalize(x.Note)
            })
        });

    private static List<OprChecklistItemResponse> DeserializeItems(string itemsJson)
    {
        try
        {
            using var document = JsonDocument.Parse(itemsJson);
            if (!document.RootElement.TryGetProperty("items", out var items)) return [];
            return [.. items.EnumerateArray().Select(x => new OprChecklistItemResponse
            {
                Code = x.TryGetProperty("code", out var code) ? code.GetString() ?? string.Empty : string.Empty,
                Label = x.TryGetProperty("label", out var label) ? label.GetString() ?? string.Empty : string.Empty,
                IsMandatory = x.TryGetProperty("isMandatory", out var mandatory) && mandatory.GetBoolean(),
                IsChecked = x.TryGetProperty("isChecked", out var check) && check.GetBoolean(),
                Note = x.TryGetProperty("note", out var note) ? note.GetString() : null
            })];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static OprChecklistResponse MapChecklist(OprSafetyChecklist entity) => new()
    {
        Id = entity.Id, Phase = entity.Phase, TemplateVersion = entity.TemplateVersion, Revision = entity.Revision,
        Status = entity.Status, Items = DeserializeItems(entity.ItemsJson), SignedByUserId = entity.SignedByUserId,
        SignedAt = entity.SignedAt, IsEmergencyBypass = entity.IsEmergencyBypass, BypassReason = entity.BypassReason,
        BypassResponsibleUserId = entity.BypassResponsibleUserId, CompletedAfterStableAt = entity.CompletedAfterStableAt
    };
}
