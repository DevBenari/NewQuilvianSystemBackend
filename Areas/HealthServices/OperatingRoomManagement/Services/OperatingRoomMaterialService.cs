using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Options;
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
/// Ledger pemakaian material dan implant milik kasus operasi (BE-OPR-008, OPS-REQ-007,
/// OPS-DEC-009/020). Service ini hanya mencatat secara lokal dan tidak pernah memutasi stok;
/// pengiriman ke Inventory/Farmasi ditangani `OperatingRoomIntegrationService`.
/// </summary>
public sealed class OperatingRoomMaterialService
{
    private const string MaterialAction = "RecordMaterial";

    private static readonly OprCaseStatus[] RecordableStatuses =
        [OprCaseStatus.InProgress, OprCaseStatus.Completed];

    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LoggerService _loggerService;
    private readonly OperatingRoomIntegrationService _integrationService;

    private readonly OperatingRoomRuleRelaxation _relaxation;

    public OperatingRoomMaterialService(ApplicationDbContext dbContext,
        IHttpContextAccessor httpContextAccessor, LoggerService loggerService,
        OperatingRoomIntegrationService integrationService, OperatingRoomRuleRelaxation relaxation)
    {
        _relaxation = relaxation;
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _loggerService = loggerService;
        _integrationService = integrationService;
    }

    public async Task<OprMaterialUsageResponse> RecordAsync(Guid caseId, CreateOprMaterialUsageRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.IdempotencyKey)) throw new ArgumentException("Idempotency key wajib diisi.");
        if (string.IsNullOrWhiteSpace(request.UnitCode)) throw new ArgumentException("Satuan item wajib diisi.");
        if (request.ExternalItemId == Guid.Empty) throw new ArgumentException("Item tidak valid.");
        if (request.Quantity <= 0)
            throw new OperatingRoomUnprocessableException("OPR008", "Jumlah pemakaian harus lebih dari nol.");
        if (request.ItemType == OprMaterialItemType.Implant &&
            string.IsNullOrWhiteSpace(request.BatchNumber) && string.IsNullOrWhiteSpace(request.SerialNumber))
            throw new OperatingRoomUnprocessableException("OPR009", "Lengkapi batch atau nomor serial implant.");

        var actorUserId = GetUserId(_httpContextAccessor);
        var fingerprint = Hash(string.Join('|', request.ExternalItemId, (int)request.ItemType, request.Quantity,
            request.UnitCode.Trim(), (int)request.Outcome, Normalize(request.BatchNumber),
            Normalize(request.SerialNumber), request.OccurredAt?.ToUniversalTime().Ticks,
            request.CorrectionOfUsageId, Normalize(request.CorrectionReason)));

        // Retry dengan kunci yang sama tidak boleh menggandakan pemakaian (OPS-DEC-020).
        var prior = await FindIdempotentAsync(request.IdempotencyKey, cancellationToken);
        if (prior != null)
        {
            EnsureSameCase(prior, caseId);
            EnsureSameFingerprint(prior.Source, fingerprint);
            return (await FindByCorrelationAsync(caseId, request.IdempotencyKey, cancellationToken))!;
        }

        var entity = await LoadCaseAsync(caseId, cancellationToken)
            ?? throw new KeyNotFoundException("Kasus operasi tidak ditemukan.");
        if (!RecordableStatuses.Contains(entity.Status))
            throw new OperatingRoomConflictException("InvalidStateTransition",
                "Pemakaian material hanya dapat dicatat setelah operasi dimulai.");
        if (entity.Status == OprCaseStatus.Completed && request.Outcome != OprMaterialOutcome.Corrected)
            throw new OperatingRoomConflictException("InvalidStateTransition",
                "Kasus sudah selesai; perubahan pemakaian hanya melalui koreksi beralasan.");
        await EnsureTeamMemberAsync(entity, actorUserId, cancellationToken);

        var revision = 1;
        if (request.Outcome == OprMaterialOutcome.Corrected)
        {
            if (!request.CorrectionOfUsageId.HasValue || string.IsNullOrWhiteSpace(request.CorrectionReason))
                throw new OperatingRoomUnprocessableException("CorrectionReasonRequired",
                    "Koreksi pemakaian wajib menyebutkan catatan yang dikoreksi dan alasannya.");
            var corrected = await _dbContext.OprMaterialUsages.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.CorrectionOfUsageId.Value && x.OprCaseId == caseId &&
                    !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Catatan pemakaian yang dikoreksi tidak ditemukan.");
            revision = corrected.Revision + 1;
        }

        var resolution = await ResolveItemAsync(request.ExternalItemId, cancellationToken);
        var now = DateTime.UtcNow;
        var usage = new OprMaterialUsage
        {
            OprCaseId = caseId, ExternalItemId = request.ExternalItemId, ItemType = request.ItemType,
            Quantity = request.Quantity, UnitCode = request.UnitCode.Trim(), Outcome = request.Outcome,
            BatchNumber = Normalize(request.BatchNumber), SerialNumber = Normalize(request.SerialNumber),
            OccurredAt = request.OccurredAt?.ToUniversalTime() ?? now, RecordedBy = actorUserId,
            Revision = revision, CorrectionReason = Normalize(request.CorrectionReason),
            CreateDateTime = now, CreateBy = actorUserId
        };
        _dbContext.OprMaterialUsages.Add(usage);
        _dbContext.OprStatusHistories.Add(NewHistory(entity.Id, entity.Status, entity.Status, MaterialAction,
            $"{request.Outcome}:{usage.Id:N}", request.IdempotencyKey, fingerprint, actorUserId, now));
        // Outbox ditulis dalam transaksi yang sama supaya tidak ada pemakaian tanpa
        // rencana penyerahan ke Inventory/Farmasi (`OPR-INT-001`).
        await _integrationService.StageMaterialDeliveryAsync(caseId, usage, actorUserId, now, cancellationToken);
        await SaveAsync(cancellationToken);

        await _loggerService.AuditAsync(LogCategory, "OperatingRoomMaterial.Record",
            "Mencatat pemakaian material atau implant operasi.",
            new
            {
                entity.Id, entity.CaseNumber, ActorUserId = actorUserId, UsageId = usage.Id,
                Outcome = usage.Outcome.ToString(), ItemType = usage.ItemType.ToString(), usage.Revision,
                ItemResolved = resolution.Resolved, CorrelationId = request.IdempotencyKey.Trim()
            });
        return Map(usage, resolution);
    }

    public async Task<OprMaterialLedgerResponse?> GetLedgerAsync(Guid caseId, CancellationToken cancellationToken = default)
    {
        var caseInfo = await _dbContext.OprCases.AsNoTracking()
            .Where(x => x.Id == caseId && !x.IsDelete)
            .Select(x => new { x.Id, x.CaseNumber })
            .FirstOrDefaultAsync(cancellationToken);
        if (caseInfo == null) return null;

        var usages = await _dbContext.OprMaterialUsages.AsNoTracking()
            .Where(x => x.OprCaseId == caseId && !x.IsDelete)
            .OrderBy(x => x.OccurredAt).ThenBy(x => x.Revision)
            .ToListAsync(cancellationToken);
        var resolutions = await ResolveItemsAsync(usages.Select(x => x.ExternalItemId).Distinct().ToList(),
            cancellationToken);

        var entries = usages.Select(x => Map(x, resolutions[x.ExternalItemId])).ToList();
        return new OprMaterialLedgerResponse
        {
            OprCaseId = caseInfo.Id, CaseNumber = caseInfo.CaseNumber, Entries = entries,
            UnresolvedItemCount = entries.Count(x => !x.IsItemResolved)
        };
    }

    /// <summary>
    /// Item dicocokkan ke master obat/bahan yang tersedia. Implant dan bahan bedah dimiliki
    /// Inventory yang ownernya belum ditetapkan, sehingga item yang tidak dikenali tetap
    /// dicatat sebagai belum tervalidasi, bukan ditolak.
    /// </summary>
    private async Task<ItemResolution> ResolveItemAsync(Guid externalItemId, CancellationToken cancellationToken)
    {
        var resolutions = await ResolveItemsAsync([externalItemId], cancellationToken);
        return resolutions[externalItemId];
    }

    private async Task<Dictionary<Guid, ItemResolution>> ResolveItemsAsync(IReadOnlyCollection<Guid> itemIds,
        CancellationToken cancellationToken)
    {
        if (itemIds.Count == 0) return [];
        var drugs = await _dbContext.Set<MstDrug>().AsNoTracking()
            .Where(x => itemIds.Contains(x.Id) && !x.IsDelete)
            .Select(x => new { x.Id, x.DrugName, x.IsActive })
            .ToListAsync(cancellationToken);

        return itemIds.Distinct().ToDictionary(id => id, id =>
        {
            var drug = drugs.FirstOrDefault(x => x.Id == id);
            if (drug == null) return new ItemResolution(false, string.Empty);
            if (!drug.IsActive)
                throw new OperatingRoomUnprocessableException("ItemInactive",
                    "Item yang dipilih sudah tidak aktif pada master farmasi.");
            return new ItemResolution(true, drug.DrugName);
        });
    }

    private async Task EnsureTeamMemberAsync(OprCase entity, Guid actorUserId, CancellationToken cancellationToken)
    {
        // Dilepas saat aturan klinis dilonggarkan: siapa pun boleh mencatat pemakaian
        // material tanpa perlu terdaftar sebagai anggota tim.
        if (_relaxation.IsRelaxed) return;

        var workforceId = await _dbContext.Users.AsNoTracking()
            .Where(x => x.Id == actorUserId).Select(x => x.WorkforceProfileId)
            .FirstOrDefaultAsync(cancellationToken);
        if (!workforceId.HasValue || workforceId.Value == Guid.Empty)
            throw new OperatingRoomForbiddenException("Akun pengguna tidak terhubung dengan data tenaga.");
        if (!entity.TeamMembers.Any(x => x.IsCurrent && !x.IsDelete && x.WorkforceId == workforceId.Value))
            throw new OperatingRoomForbiddenException(
                "Hanya anggota tim operasi yang boleh mencatat pemakaian material.");
    }

    private Task<OprCase?> LoadCaseAsync(Guid caseId, CancellationToken cancellationToken) =>
        _dbContext.OprCases
            .Include(x => x.TeamMembers)
            .FirstOrDefaultAsync(x => x.Id == caseId && !x.IsDelete, cancellationToken);

    private Task<OprStatusHistory?> FindIdempotentAsync(string idempotencyKey, CancellationToken cancellationToken) =>
        _dbContext.OprStatusHistories.AsNoTracking().FirstOrDefaultAsync(x =>
            x.Action == MaterialAction && x.CorrelationId == idempotencyKey.Trim() && !x.IsDelete, cancellationToken);

    /// <summary>Menemukan kembali entri yang dibuat oleh permintaan dengan kunci sama.</summary>
    private async Task<OprMaterialUsageResponse?> FindByCorrelationAsync(Guid caseId, string idempotencyKey,
        CancellationToken cancellationToken)
    {
        var history = await _dbContext.OprStatusHistories.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Action == MaterialAction && x.CorrelationId == idempotencyKey.Trim() &&
                x.OprCaseId == caseId && !x.IsDelete, cancellationToken);
        var marker = history?.Reason?.Split(':', 2);
        if (marker is not { Length: 2 } || !Guid.TryParseExact(marker[1], "N", out var usageId)) return null;

        var usage = await _dbContext.OprMaterialUsages.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == usageId && !x.IsDelete, cancellationToken);
        if (usage == null) return null;
        return Map(usage, await ResolveItemAsync(usage.ExternalItemId, cancellationToken));
    }

    private static OprMaterialUsageResponse Map(OprMaterialUsage usage, ItemResolution resolution) => new()
    {
        Id = usage.Id, OprCaseId = usage.OprCaseId, ExternalItemId = usage.ExternalItemId,
        IsItemResolved = resolution.Resolved, ItemName = resolution.Name, ItemType = usage.ItemType,
        Quantity = usage.Quantity, UnitCode = usage.UnitCode, Outcome = usage.Outcome,
        BatchNumber = usage.BatchNumber, SerialNumber = usage.SerialNumber, OccurredAt = usage.OccurredAt,
        RecordedBy = usage.RecordedBy, Revision = usage.Revision, CorrectionReason = usage.CorrectionReason
    };

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

    private readonly record struct ItemResolution(bool Resolved, string Name);
}
