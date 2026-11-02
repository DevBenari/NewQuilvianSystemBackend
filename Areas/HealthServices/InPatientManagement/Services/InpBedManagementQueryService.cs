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
        var list = await ProjectManyAsync([bed], ct);
        return list[0];
    }

    public async Task<List<BedOperationalResponse>> ProjectManyAsync(IReadOnlyList<MstBed> beds, CancellationToken ct = default)
        => (await ProjectBatchAsync(beds, ct)).Rows;

    /// <summary>
    /// Proyeksi status operasional bed beserta data yang dimuatnya, supaya pemanggil seperti
    /// pencarian bed dan kelayakan penempatan tidak memuat ulang kesiapan, pemesanan,
    /// penempatan, unit, dan kelas yang sama (BE-RWI-228). Isi <see cref="BedProjectionBatch.Rows"/>
    /// identik dengan <see cref="ProjectManyAsync"/>.
    /// </summary>
    internal async Task<BedProjectionBatch> ProjectBatchAsync(IReadOnlyList<MstBed> beds, CancellationToken ct = default)
    {
        if (beds.Count == 0) return new BedProjectionBatch();

        var bedIds = beds.Select(x => x.Id).Distinct().ToList();
        var roomIds = beds.Select(x => x.RoomId).Distinct().ToList();

        // Kamar yang sudah ikut dimuat pemanggil (Include tanpa pelacakan) dipakai langsung;
        // hanya yang belum ada yang dibaca dari database. Kamar terlacak tidak dipakai supaya
        // perubahan yang belum disimpan tidak ikut terbaca.
        var rooms = new Dictionary<Guid, MstRoom>();
        foreach (var bed in beds)
        {
            if (bed.Room != null && bed.Room.Id == bed.RoomId && db.Entry(bed.Room).State == EntityState.Detached)
                rooms[bed.RoomId] = bed.Room;
        }

        var missingRoomIds = roomIds.Where(x => !rooms.ContainsKey(x)).ToList();
        if (missingRoomIds.Count > 0)
        {
            var loadedRooms = await db.Set<MstRoom>().AsNoTracking()
                .Where(x => missingRoomIds.Contains(x.Id))
                .ToListAsync(ct);
            foreach (var room in loadedRooms) rooms[room.Id] = room;
        }

        var unitIds = rooms.Values.Select(x => x.ServiceUnitId).Distinct().ToList();
        var classIds = rooms.Values.Where(x => x.PatientClassId.HasValue).Select(x => x.PatientClassId!.Value).Distinct().ToList();

        var units = await db.Set<MstServiceUnit>().AsNoTracking()
            .Where(x => unitIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);

        var classes = await db.Set<MstPatientClass>().AsNoTracking()
            .Where(x => classIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);

        var roots = await db.Set<InpBedReadiness>().AsNoTracking()
            .Where(x => bedIds.Contains(x.BedId))
            .ToDictionaryAsync(x => x.BedId, ct);

        var placements = await db.Set<InpBedPlacement>().AsNoTracking()
            .Where(x => bedIds.Contains(x.BedId) && x.EndDateTime == null && !x.IsDelete)
            .ToDictionaryAsync(x => x.BedId, ct);

        var nowUtc = DateTime.UtcNow;
        var reservations = await db.Set<InpBedReservation>().AsNoTracking()
            .Where(x => bedIds.Contains(x.BedId) && x.ReservationStatus == InpBedReservationStatus.Active && !x.IsDelete && x.ExpiresAt > nowUtc)
            .ToDictionaryAsync(x => x.BedId, ct);

        var epIds = placements.Values.Select(x => x.EpisodeId)
            .Concat(reservations.Values.Select(x => x.EpisodeId))
            .Distinct().ToList();

        Dictionary<Guid, (Guid Id, string EpisodeNumber, string? Name, Guid ServiceUnitId, bool IsDelete, string? MedicalRecordNumber, InpCareDependencyLevel? CareDependencyLevel)> episodes = [];
        if (epIds.Count > 0)
        {
            // BE-RWI-238: No. RM dan HD/LD ikut dimuat sekali per batch, bukan per bed.
            var epList = await db.Set<InpEpisode>().AsNoTracking()
                .Where(x => epIds.Contains(x.Id))
                .Select(x => new
                {
                    x.Id, x.EpisodeNumber, Name = x.Patient == null ? null : x.Patient.FullName, x.ServiceUnitId, x.IsDelete,
                    MedicalRecordNumber = x.Patient == null ? null : x.Patient.MedicalRecordNumber, x.CareDependencyLevel
                })
                .ToListAsync(ct);
            episodes = epList.ToDictionary(x => x.Id, x => (x.Id, x.EpisodeNumber, x.Name, x.ServiceUnitId, x.IsDelete, x.MedicalRecordNumber, x.CareDependencyLevel));
        }

        // Unit per aksi pembersihan dimuat hanya bila ada bed bebas dalam keadaan kesiapan yang
        // relevan. Pada papan biasa hampir semua bed Ready atau terpakai, sehingga tiga cek hak
        // akses beserta daftar unitnya tidak perlu dijalankan sama sekali (BE-RWI-228).
        var actionUnits = new Dictionary<string, List<Guid>>();

        var responses = new List<BedOperationalResponse>(beds.Count);
        foreach (var bed in beds)
        {
            rooms.TryGetValue(bed.RoomId, out var room);
            MstServiceUnit? unit = null;
            if (room != null) units.TryGetValue(room.ServiceUnitId, out unit);
            MstPatientClass? cls = null;
            if (room?.PatientClassId != null) classes.TryGetValue(room.PatientClassId.Value, out cls);

            roots.TryGetValue(bed.Id, out var root);
            placements.TryGetValue(bed.Id, out var placement);
            reservations.TryGetValue(bed.Id, out var reservation);

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
                IsIsolationBed = bed.IsIsolationBed, IsForNewborn = bed.IsForNewborn,
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
            if (episodeId.HasValue && episodes.TryGetValue(episodeId.Value, out var ep))
            {
                if (await access.CanReadLoadedIdentityAsync(response.ServiceUnitId, ep.ServiceUnitId, ep.IsDelete, ct))
                {
                    // GUARD-DNH-01: No. RM dan HD/LD mengikuti aturan yang sama dengan nama penghuni.
                    response.HoldingContext = new() { EpisodeId = ep.Id, EpisodeNumber = ep.EpisodeNumber, PatientName = ep.Name,
                        MedicalRecordNumber = ep.MedicalRecordNumber, CareDependencyLevel = ep.CareDependencyLevel?.ToString(),
                        ReservationId = reservation?.Id, ReservationExpiresAt = reservation?.ExpiresAt,
                        ReservationReservedAt = reservation?.ReservedAt };
                }
            }
            var freeForHousekeeping = placement == null && reservation == null && !closed && valid;
            foreach (var action in CleaningActions)
            {
                if (!freeForHousekeeping || !ReadinessAllows(action, root?.State)) continue;
                if (!actionUnits.TryGetValue(action, out var allowedUnits))
                {
                    allowedUnits = await access.UnitsAsync("InpatientBedManagement:" + action, ct);
                    actionUnits[action] = allowedUnits;
                }
                if (!allowedUnits.Contains(response.ServiceUnitId)) continue;
                try
                {
                    access.WorkflowAssignment(response.ServiceUnitId, "InpatientBedManagement:" + action);
                    response.AvailableActions.Add(action);
                }
                catch (InpBedOperationException) { }
            }
            responses.Add(response);
        }
        return new BedProjectionBatch
        {
            Rows = responses, Roots = roots, Placements = placements, Reservations = reservations, Units = units, Classes = classes,
            EpisodeCareDependency = episodes.ToDictionary(x => x.Key, x => x.Value.CareDependencyLevel)
        };
    }

    private static readonly string[] CleaningActions = ["StartCleaning", "CompleteCleaning", "VerifyReadiness"];

    private static bool ReadinessAllows(string action, InpBedReadinessState? state) => action switch
    {
        "StartCleaning" => state == InpBedReadinessState.WaitingCleaning,
        "CompleteCleaning" => state == InpBedReadinessState.Cleaning,
        "VerifyReadiness" => state is InpBedReadinessState.Unverified or InpBedReadinessState.AwaitingVerification,
        _ => false
    };

    public async Task<BedMonitoringResponse> MonitoringAsync(BedMonitoringQuery query, string operation = "InpatientBedManagement:Read", CancellationToken ct = default)
    {
        if (query.Status != null && !StatusLabels.ContainsKey(query.Status))
            throw new InpBedOperationException(400, "INVALID_INPUT", "Status monitoring tidak dikenal.");
        await coordinator.ExpireScopedAsync(operation, ct); var units = await access.UnitsAsync(operation, ct);
        var source = db.Set<MstBed>().AsNoTracking().Where(x => !x.IsDelete && x.Room != null && units.Contains(x.Room.ServiceUnitId));
        if (query.IsForNewborn.HasValue) source = source.Where(x => x.IsForNewborn == query.IsForNewborn.Value);
        if (query.ServiceUnitId.HasValue) source = source.Where(x => x.Room!.ServiceUnitId == query.ServiceUnitId);
        if (query.RoomId.HasValue) source = source.Where(x => x.RoomId == query.RoomId);
        if (query.PatientClassId.HasValue) source = source.Where(x => x.Room!.PatientClassId == query.PatientClassId);
        if (!string.IsNullOrWhiteSpace(query.FloorCode))
        {
            // BE-RWI-238: saring satu lantai denah; tanpa parameter jawaban tidak berubah.
            var floor = query.FloorCode.Trim().ToLower();
            source = source.Where(x => x.Room!.FloorCode != null && x.Room.FloorCode.ToLower() == floor);
        }
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLowerInvariant();
            source = source.Where(x => x.BedCode.ToLower().Contains(term) || x.BedName.ToLower().Contains(term) || x.Room!.RoomName.ToLower().Contains(term));
        }
        var beds = await source.OrderBy(x => x.BedCode).ToListAsync(ct);
        var projection = await ProjectBatchAsync(beds, ct);
        var allRows = projection.Rows;
        var rows = query.Status == null ? allRows : allRows.Where(x => x.StatusCode == query.Status).ToList();
        var result = new BedMonitoringResponse { AsOfUtc = DateTime.UtcNow };
        result.Counts = new() { Total = rows.Count, Available = rows.Count(x => x.StatusCode == "Available"),
            Occupied = rows.Count(x => x.StatusCode == "Occupied"), Reserved = rows.Count(x => x.StatusCode == "Reserved"),
            WaitingCleaning = rows.Count(x => x.StatusCode == "WaitingCleaning"), Cleaning = rows.Count(x => x.StatusCode == "Cleaning"),
            Unavailable = rows.Count(x => x.StatusCode == "Unavailable") };
        // Nama unit dari data proyeksi; setiap baris sudah berada pada unit dalam cakupan (BE-RWI-228).
        var unitNames = projection.Units.ToDictionary(x => x.Key, x => x.Value.ServiceUnitName);
        result.ServiceUnits = rows.GroupBy(x => x.ServiceUnitId).Select(g => new BedMonitoringUnit
        {
            Id = g.Key, Name = unitNames.GetValueOrDefault(g.Key),
            Rooms = g.GroupBy(x => x.RoomId).Select(r => new BedMonitoringRoom { Id = r.Key, Name = r.First().RoomName, Beds = r.ToList() }).ToList()
        }).ToList();
        return result;
    }
    /// <param name="p">Penempatan yang lokasinya dibekukan.</param>
    /// <param name="ct">Token pembatalan.</param>
    /// <param name="comparisonResult">
    /// Hasil perbandingan kelas pada transfer tervalidasi (<c>DownGrade</c>/<c>UpGrade</c>/<c>SameGrade</c>/<c>Special</c>);
    /// tidak diisi pada penempatan awal sehingga bentuk snapshot penempatan awal tidak berubah.
    /// </param>
    public async Task<string> SnapshotAsync(InpBedPlacement p, CancellationToken ct = default, string? comparisonResult = null)
    {
        var bed = await db.Set<MstBed>().AsNoTracking().SingleAsync(x => x.Id == p.BedId, ct);
        var room = await db.Set<MstRoom>().AsNoTracking().SingleAsync(x => x.Id == p.RoomId, ct);
        var unit = await db.Set<MstServiceUnit>().AsNoTracking().SingleAsync(x => x.Id == p.ServiceUnitId, ct);
        var cls = await db.Set<MstPatientClass>().AsNoTracking().SingleAsync(x => x.Id == p.PatientClassId, ct);
        var snapshot = new { p.BedId, bed.BedCode, bed.BedName, p.RoomId, room.RoomCode, room.RoomName,
            p.ServiceUnitId, unit.ServiceUnitName, p.PatientClassId, cls.PatientClassCode, cls.PatientClassName, p.TransferCategory, ClassOrderEvidenceReference = options.CurrentValue.ClassOrder.EvidenceReference, ClassOrderSourceDigest = options.CurrentValue.ClassOrder.ApprovedSourceDigest };
        if (comparisonResult == null) return JsonSerializer.Serialize(snapshot);
        // BE-RWI-233: hasil perbandingan (termasuk Special) ikut dibekukan pada snapshot transfer.
        var node = JsonSerializer.SerializeToNode(snapshot)!.AsObject();
        node["ComparisonResult"] = comparisonResult;
        return node.ToJsonString();
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

/// <summary>Hasil <see cref="InpBedManagementQueryService.ProjectBatchAsync"/>: baris status beserta data yang dimuatnya.</summary>
internal sealed class BedProjectionBatch
{
    public List<BedOperationalResponse> Rows { get; init; } = [];
    public Dictionary<Guid, InpBedReadiness> Roots { get; init; } = [];
    public Dictionary<Guid, InpBedPlacement> Placements { get; init; } = [];
    public Dictionary<Guid, InpBedReservation> Reservations { get; init; } = [];
    public Dictionary<Guid, MstServiceUnit> Units { get; init; } = [];
    public Dictionary<Guid, MstPatientClass> Classes { get; init; } = [];
    /// <summary>HD/LD episode penghuni/pemesan per EpisodeId; dipakai ringkasan kamar denah (BE-RWI-238).</summary>
    public Dictionary<Guid, InpCareDependencyLevel?> EpisodeCareDependency { get; init; } = [];
}
