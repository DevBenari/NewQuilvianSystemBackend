using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;

public sealed class InpBedManagementQueryService(ApplicationDbContext db, InpBedAccessService access, InpBedMutationCoordinator coordinator, Microsoft.Extensions.Options.IOptionsMonitor<QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Options.BedManagementOptions> options)
{
    public static readonly Dictionary<string, string> StatusLabels = new()
    {
        ["Available"] = "Tersedia", ["Occupied"] = "Terisi", ["Reserved"] = "Dipesan",
        ["WaitingCleaning"] = "Menunggu Pembersihan", ["Cleaning"] = "Dalam Pembersihan", ["Unavailable"] = "Tidak Tersedia"
    };
    public async Task<BedOperationalResponse> ProjectAsync(MstBed bed, CancellationToken ct = default)
    {
        var room = await db.Set<MstRoom>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == bed.RoomId, ct);
        var unit = room == null ? null : await db.Set<MstServiceUnit>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == room.ServiceUnitId, ct);
        var cls = room?.PatientClassId == null ? null : await db.Set<MstPatientClass>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == room.PatientClassId, ct);
        var root = await db.Set<InpBedReadiness>().AsNoTracking().SingleOrDefaultAsync(x => x.BedId == bed.Id, ct);
        var placement = await db.Set<InpBedPlacement>().AsNoTracking().FirstOrDefaultAsync(x => x.BedId == bed.Id && x.EndDateTime == null && !x.IsDelete, ct);
        var reservation = await db.Set<InpBedReservation>().AsNoTracking().FirstOrDefaultAsync(x => x.BedId == bed.Id && x.ReservationStatus == InpBedReservationStatus.Active && !x.IsDelete && x.ExpiresAt > DateTime.UtcNow, ct);
        var valid = !bed.IsDelete && bed.IsActive && bed.IsReservable && room is { IsDelete: false, IsActive: true, IsAvailableForAdmission: true } &&
            unit is { IsDelete: false, IsActive: true } && cls is { IsDelete: false, IsActive: true };
        var closed = bed.BedStatus is BedStatus.Maintenance or BedStatus.Blocked or BedStatus.Inactive or BedStatus.Unknown ||
            !Enum.IsDefined(bed.BedStatus);
        var code = placement != null ? "Occupied" : reservation != null ? "Reserved" :
            !valid || closed ? "Unavailable" : root?.State switch
            {
                InpBedReadinessState.Ready when bed.BedStatus == BedStatus.Available => "Available",
                InpBedReadinessState.WaitingCleaning => "WaitingCleaning",
                InpBedReadinessState.Cleaning or InpBedReadinessState.AwaitingVerification => "Cleaning",
                _ => "Unavailable"
            };
        var response = new BedOperationalResponse
        {
            RawBedStatus = (int)bed.BedStatus, BedId = bed.Id, BedCode = bed.BedCode, BedName = bed.BedName, RoomId = bed.RoomId,
            RoomName = room?.RoomName, RawRoomCode = room?.RoomCode, RawRoomCapacity = room?.Capacity ?? 0, ServiceUnitId = room?.ServiceUnitId ?? Guid.Empty,
            PatientClassId = room?.PatientClassId, PatientClassName = cls?.PatientClassName,
            StatusCode = code, StatusLabel = StatusLabels[code], IsBookable = code == "Available",
            OperationalVersion = root?.Version ?? 1, CycleId = root?.CycleId ?? Guid.Empty,
            CleaningPhase = code == "Cleaning" ? root?.State == InpBedReadinessState.AwaitingVerification ? "AwaitingVerification" : "InProgress" : null,
            ConflictCode = (placement != null && reservation != null && placement.EpisodeId != reservation.EpisodeId) || (placement != null && (bed.BedStatus != BedStatus.Occupied || !valid)) ||
                (reservation != null && (bed.BedStatus != BedStatus.Reserved || !valid)) ? "ADMINISTRATIVE_CONFLICT" : null
        };
        if (!valid) response.BlockReasonCodes.Add("MASTER_INVALID");
        if (closed) response.BlockReasonCodes.Add("BED_UNAVAILABLE");
        if (root?.State != InpBedReadinessState.Ready) response.BlockReasonCodes.Add("BED_NOT_READY");
        var episodeId = placement?.EpisodeId ?? reservation?.EpisodeId;
        if (episodeId.HasValue && await access.CanReadIdentityAsync(episodeId.Value, response.ServiceUnitId, ct))
        {
            var ep = await db.Set<InpEpisode>().AsNoTracking().Where(x => x.Id == episodeId).Select(x => new { x.Id, x.EpisodeNumber, Name = x.Patient == null ? null : x.Patient.FullName }).SingleAsync(ct);
            response.HoldingContext = new() { EpisodeId = ep.Id, EpisodeNumber = ep.EpisodeNumber, PatientName = ep.Name, ReservationId = reservation?.Id, ReservationExpiresAt = reservation?.ExpiresAt };
        }
        foreach (var action in new[] { "StartCleaning", "CompleteCleaning", "VerifyReadiness" })
        {
            if (!(await access.UnitsAsync("InpatientBedManagement:" + action, ct)).Contains(response.ServiceUnitId)) continue;
            try
            {
                access.WorkflowAssignment(response.ServiceUnitId, "InpatientBedManagement:" + action);
                if (placement == null && reservation == null && !closed && valid &&
                    (action == "StartCleaning" && root?.State == InpBedReadinessState.WaitingCleaning ||
                     action == "CompleteCleaning" && root?.State == InpBedReadinessState.Cleaning ||
                     action == "VerifyReadiness" && root?.State is InpBedReadinessState.Unverified or InpBedReadinessState.AwaitingVerification))
                    response.AvailableActions.Add(action);
            }
            catch (InpBedOperationException) { }
        }
        return response;
    }
    public async Task<BedMonitoringResponse> MonitoringAsync(BedMonitoringQuery query, string operation = "InpatientBedManagement:Read", CancellationToken ct = default)
    {
        if (query.Status != null && !StatusLabels.ContainsKey(query.Status))
            throw new InpBedOperationException(400, "INVALID_INPUT", "Status monitoring tidak dikenal.");
        await coordinator.ExpireScopedAsync(operation, ct); var units = await access.UnitsAsync(operation, ct);
        var source = db.Set<MstBed>().AsNoTracking().Where(x => !x.IsDelete && x.Room != null && units.Contains(x.Room.ServiceUnitId));
        if (query.ServiceUnitId.HasValue) source = source.Where(x => x.Room!.ServiceUnitId == query.ServiceUnitId);
        if (query.RoomId.HasValue) source = source.Where(x => x.RoomId == query.RoomId);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLowerInvariant();
            source = source.Where(x => x.BedCode.ToLower().Contains(term) || x.BedName.ToLower().Contains(term) || x.Room!.RoomName.ToLower().Contains(term));
        }
        var beds = await source.OrderBy(x => x.BedCode).ToListAsync(ct);
        var rows = new List<BedOperationalResponse>();
        foreach (var b in beds) { var row = await ProjectAsync(b, ct); if (query.Status == null || row.StatusCode == query.Status) rows.Add(row); }
        var result = new BedMonitoringResponse { AsOfUtc = DateTime.UtcNow };
        result.Counts = new() { Total = rows.Count, Available = rows.Count(x => x.StatusCode == "Available"),
            Occupied = rows.Count(x => x.StatusCode == "Occupied"), Reserved = rows.Count(x => x.StatusCode == "Reserved"),
            WaitingCleaning = rows.Count(x => x.StatusCode == "WaitingCleaning"), Cleaning = rows.Count(x => x.StatusCode == "Cleaning"),
            Unavailable = rows.Count(x => x.StatusCode == "Unavailable") };
        var unitNames = await db.Set<MstServiceUnit>().AsNoTracking().Where(x => units.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.ServiceUnitName, ct);
        result.ServiceUnits = rows.GroupBy(x => x.ServiceUnitId).Select(g => new BedMonitoringUnit
        {
            Id = g.Key, Name = unitNames.GetValueOrDefault(g.Key),
            Rooms = g.GroupBy(x => x.RoomId).Select(r => new BedMonitoringRoom { Id = r.Key, Name = r.First().RoomName, Beds = r.ToList() }).ToList()
        }).ToList();
        return result;
    }
    public async Task<string> SnapshotAsync(InpBedPlacement p, CancellationToken ct = default)
    {
        var bed = await db.Set<MstBed>().AsNoTracking().SingleAsync(x => x.Id == p.BedId, ct);
        var room = await db.Set<MstRoom>().AsNoTracking().SingleAsync(x => x.Id == p.RoomId, ct);
        var unit = await db.Set<MstServiceUnit>().AsNoTracking().SingleAsync(x => x.Id == p.ServiceUnitId, ct);
        var cls = await db.Set<MstPatientClass>().AsNoTracking().SingleAsync(x => x.Id == p.PatientClassId, ct);
        return JsonSerializer.Serialize(new { p.BedId, bed.BedCode, bed.BedName, p.RoomId, room.RoomCode, room.RoomName,
            p.ServiceUnitId, unit.ServiceUnitName, p.PatientClassId, cls.PatientClassCode, cls.PatientClassName, p.TransferCategory, ClassOrderEvidenceReference = options.CurrentValue.ClassOrder.EvidenceReference, ClassOrderSourceDigest = options.CurrentValue.ClassOrder.ApprovedSourceDigest });
    }
    public async Task<PagedResult<BedUsageHistoryResponse>> HistoryAsync(BedUsageHistoryQuery query, CancellationToken ct = default)
    {
        if (query.BedId == Guid.Empty || query.FromUtc.Kind != DateTimeKind.Utc || query.ToUtc.Kind != DateTimeKind.Utc || query.FromUtc >= query.ToUtc)
            throw new InpBedOperationException(400, "INVALID_INPUT", "Bed dan rentang UTC FromUtc < ToUtc wajib.");
        await access.RequireBedAsync(query.BedId, "InpatientBedManagement:ReadUsageHistory", ct);
        var units = await access.UnitsAsync("InpatientBedManagement:ReadUsageHistory", ct);
        var src = db.Set<InpBedPlacement>().AsNoTracking().Where(x => x.BedId == query.BedId && units.Contains(x.ServiceUnitId) && !x.IsDelete &&
            x.StartDateTime < query.ToUtc && (x.EndDateTime == null || x.EndDateTime > query.FromUtc));
        if (query.RoomId.HasValue) src = src.Where(x => x.RoomId == query.RoomId);
        var page = Math.Max(1, query.PageNumber); var size = Math.Clamp(query.PageSize, 1, 100);
        var total = await src.CountAsync(ct);
        var rows = await src.OrderByDescending(x => x.StartDateTime).ThenByDescending(x => x.Id).Skip((page - 1) * size).Take(size).ToListAsync(ct);
        var items = new List<BedUsageHistoryResponse>();
        foreach (var p in rows)
        {
            var identity = await access.CanReadIdentityAsync(p.EpisodeId, p.ServiceUnitId, ct);
            items.Add(new()
            {
                PlacementId = p.Id, EpisodeId = identity ? p.EpisodeId : null,
                PatientIdentity = identity ? await db.Set<InpEpisode>().Where(x => x.Id == p.EpisodeId).Select(x => x.Patient == null ? null : x.Patient.FullName).SingleOrDefaultAsync(ct) : null,
                BedId = p.BedId, RoomId = p.RoomId, ServiceUnitId = p.ServiceUnitId, PatientClassId = p.PatientClassId,
                LocationSnapshot = p.LocationSnapshotJson, ContextSource = p.LocationSnapshotJson == null ? "LegacyStoredIds" : "StoredSnapshot",
                SequenceNumber = p.SequenceNumber, StartDateTime = p.StartDateTime, EndDateTime = p.EndDateTime, EndReason = (int?)p.EndReason,
                TransferCategory = (int?)p.TransferCategory, TransferFromPlacementId = p.TransferFromPlacementId,
                Version = p.Version, CorrectsPlacementId = p.CorrectsPlacementId, SupersededByCorrectionId = p.SupersededByCorrectionId,
                IsCurrent = p.EndDateTime == null && p.SupersededByCorrectionId == null, IsCorrection = p.CorrectsPlacementId != null,
                IsEffectiveForBilling = p.SupersededByCorrectionId == null
            });
        }
        return new() { PageNumber = page, PageSize = size, TotalData = total, TotalPage = (int)Math.Ceiling(total / (double)size), Items = items };
    }
    public async Task<OperationOutcomeResponse> OutcomeAsync(string key, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(key) || key.Length > 100) throw new InpBedOperationException(400, "INVALID_INPUT", "Kunci tidak sah.");
        var r = await db.Set<InpBedOperationReceipt>().AsNoTracking().SingleOrDefaultAsync(x => x.ActorUserId == access.ActorId && x.IdempotencyKey == key, ct)
            ?? throw new InpBedOperationException(404, "OPERATION_UNKNOWN", "Hasil commit belum ditemukan.");
        Guid[] beds = r.ResultKind switch
        {
            "Placement" => await db.Set<InpBedPlacement>().Where(x => x.Id == r.ResultEntityId).Select(x => x.BedId).ToArrayAsync(ct),
            "Reservation" => await db.Set<InpBedReservation>().Where(x => x.Id == r.ResultEntityId).Select(x => x.BedId).ToArrayAsync(ct),
            "CleaningAttempt" => await db.Set<InpBedCleaningAttempt>().Where(x => x.Id == r.ResultEntityId).Join(db.Set<InpBedReadiness>(), a => a.ReadinessId, b => b.Id, (a,b) => b.BedId).ToArrayAsync(ct),
            "Bed" => [r.ResultEntityId],
            _ => await db.Set<InpBedLifecycleEvent>().Where(x => x.OperationReceiptId == r.Id).Join(db.Set<InpBedReadiness>(), e => e.ReadinessId, b => b.Id, (e,b) => b.BedId).ToArrayAsync(ct)
        };
        foreach (var bed in beds) await access.RequireBedAsync(bed, r.OperationName, ct);
        if (beds.Length == 0) await access.RequireMasterResultAsync(r.ResultKind, r.ResultEntityId, r.OperationName, ct);
        return new() { ReceiptId = r.Id, ResultEntityId = r.ResultEntityId, ResultKind = r.ResultKind, CommittedVersion = r.ResultVersion,
            CommittedAtUtc = r.CommittedAtUtc, IsReplay = false, OperationName = r.OperationName };
    }
}

