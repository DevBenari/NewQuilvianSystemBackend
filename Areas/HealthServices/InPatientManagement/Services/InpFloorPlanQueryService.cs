using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;

/// <summary>
/// Proyeksi denah satu lantai (backend 15.7): kamar aktif berkunci, status kamar, badge, ringkasan
/// HD/LD, dan bed dengan enam status. Mode Monitoring dipakai Monitoring Bed (BE-RWI-238); mode
/// Pilih Bed menambah kelayakan per bed untuk satu episode beserta outline kamar (BE-RWI-239).
/// </summary>
/// <remarks>
/// <b>Tidak menghitung status bed sendiri.</b> Status bed, penghuni, dan penyamaran identitas
/// berasal dari <see cref="InpBedManagementQueryService.ProjectBatchAsync"/>; kelayakan berasal dari
/// evaluator <see cref="InpBedOccupancyService.EvaluatePlacementEligibilityCore"/> yang sama dengan
/// <c>available-beds</c>. Jumlah query tetap per permintaan, tidak bertambah per bed (NFR-803).
///
/// <para>
/// <b>Status kamar</b> dihitung berurutan atas bed biasa: Tidak Tersedia (kamar ditutup, tanpa bed
/// aktif, atau semua bed Tidak Tersedia) → Tersedia (ada bed Tersedia) → Persiapan (ada bed menunggu
/// atau dalam pembersihan) → Penuh. Contoh: tiga bed Terisi/Dipesan/Tersedia → <c>Available</c>,
/// badge {3, 2, 1}; Terisi/Dipesan/Terisi → <c>Full</c> {3, 3, 0}.
/// </para>
/// </remarks>
public sealed class InpFloorPlanQueryService(
    ApplicationDbContext db,
    InpBedAccessService access,
    InpBedMutationCoordinator coordinator,
    InpBedManagementQueryService bedQueries,
    InpBedOccupancyService occupancy,
    InpBedClassComparisonService classComparison)
{
    private const string MonitoringOperation = "InpatientBedManagement:Read";
    private const string SelectionOperation = "InpatientBedOccupancy:Read";

    private static readonly Dictionary<string, string> RoomStatusLabels = new()
    {
        ["Unavailable"] = "Tidak Tersedia", ["Available"] = "Tersedia", ["Preparing"] = "Persiapan", ["Full"] = "Penuh"
    };

    private sealed record FloorData(
        List<MstRoom> Rooms, List<MstBed> Beds, BedProjectionBatch Projection,
        string? FloorCode, List<FloorPlanFloorSummaryResponse> Floors);

    private sealed record SelectionContext(
        InpEpisode Episode, MstPatient? Patient, InpPlacementContext PlacementContext,
        Guid? MotherRoomId, Guid? SourceClassId);

    /// <summary>Denah Monitoring Bed (GUARD-DNH-05: scope Read Bed Management existing).</summary>
    public async Task<FloorPlanResponse> GetMonitoringAsync(FloorPlanMonitoringQuery? query, CancellationToken ct)
    {
        await coordinator.ExpireScopedAsync(MonitoringOperation, ct);
        var units = await access.UnitsAsync(MonitoringOperation, ct);
        var data = await LoadAsync(units, query?.FloorCode, ct);
        return await ProjectAsync(data, selection: null, ct);
    }

    /// <summary>
    /// Denah langkah Pilih Bed admisi atau tujuan transfer: kelayakan setiap bed untuk episode itu
    /// (VAL-DNH-21, 22). Bed yang tidak layak tetap dikirim beserta alasannya.
    /// </summary>
    public async Task<FloorPlanResponse> GetSelectionAsync(FloorPlanSelectionQuery? query, CancellationToken ct)
    {
        var purpose = query?.Purpose?.Trim();
        if (!string.Equals(purpose, "Admission", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(purpose, "Transfer", StringComparison.OrdinalIgnoreCase))
            throw new InpBedOperationException(400, "INVALID_INPUT", "Tujuan denah harus Admission atau Transfer.");
        if (query!.EpisodeId is not { } episodeId || episodeId == Guid.Empty)
            throw new InpBedOperationException(400, "INVALID_INPUT", "Episode wajib dikirim dari konteks layar.");
        var isTransfer = string.Equals(purpose, "Transfer", StringComparison.OrdinalIgnoreCase);

        await access.RequireEpisodeAsync(episodeId, SelectionOperation, ct);
        var episode = await db.Set<InpEpisode>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == episodeId && !x.IsDelete, ct)
            ?? throw new InpBedOperationException(404, "NOT_FOUND", "Episode rawat inap tidak ditemukan.");

        Guid? sourceClassId = null;
        if (isTransfer)
        {
            var activePlacement = episode.EpisodeStatus == InpEpisodeStatus.Admitted
                ? await db.Set<InpBedPlacement>().AsNoTracking()
                    .FirstOrDefaultAsync(x => x.EpisodeId == episode.Id && x.EndDateTime == null && !x.IsDelete, ct)
                : null;
            if (activePlacement == null)
                throw new InpBedOperationException(422, "FLOOR_PLAN_PURPOSE_MISMATCH",
                    "Denah tujuan transfer hanya untuk pasien yang sedang dirawat dan menempati bed. Kembali ke langkah sebelumnya.");
            sourceClassId = activePlacement.PatientClassId;
        }
        else if (episode.EpisodeStatus != InpEpisodeStatus.Draft)
        {
            throw new InpBedOperationException(422, "FLOOR_PLAN_PURPOSE_MISMATCH",
                "Denah Pilih Bed admisi hanya untuk admisi yang masih disiapkan. Kembali ke langkah sebelumnya.");
        }

        var patient = await db.Set<MstPatient>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == episode.PatientId, ct);
        var motherRoomId = patient?.IsNewborn == true
            ? await InpNewbornAdmissionRules.FindMotherRoomAsync(db, episode.MotherEpisodeId, patient.MotherPatientId, ct)
            : null;

        await coordinator.ExpireScopedAsync(SelectionOperation, ct);
        var units = await access.UnitsAsync(SelectionOperation, ct);
        var data = await LoadAsync(units, query.FloorCode, ct);
        var selection = new SelectionContext(episode, patient,
            isTransfer ? InpPlacementContext.Transfer : InpPlacementContext.Reservation, motherRoomId, sourceClassId);
        return await ProjectAsync(data, selection, ct);
    }

    // ---------------------------------------------------------------- muat data (query tetap)

    private async Task<FloorData> LoadAsync(List<Guid> units, string? requestedFloor, CancellationToken ct)
    {
        // Hanya kamar aktif berkunci dalam scope pembaca; hitungan dihitung sesudah scope (GUARD-DNH-05).
        var rooms = await db.Set<MstRoom>().AsNoTracking()
            .Include(x => x.ServiceUnit)
            .Include(x => x.PatientClass)
            .Where(x => !x.IsDelete && x.IsActive && x.FloorPlanRoomKey != null && x.FloorCode != null && units.Contains(x.ServiceUnitId))
            .ToListAsync(ct);
        var roomIds = rooms.Select(x => x.Id).ToList();
        var beds = await db.Set<MstBed>().AsNoTracking()
            .Where(x => !x.IsDelete && x.IsActive && roomIds.Contains(x.RoomId))
            .ToListAsync(ct);
        var roomById = rooms.ToDictionary(x => x.Id);
        foreach (var bed in beds) bed.Room = roomById[bed.RoomId];

        var projection = await bedQueries.ProjectBatchAsync(beds, ct);
        var status = projection.Rows.ToDictionary(x => x.BedId, x => x.StatusCode);

        var floors = rooms
            .GroupBy(x => x.FloorCode!.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(g => new FloorPlanFloorSummaryResponse
            {
                FloorCode = g.Key,
                FloorName = FloorPlanRoomNaming.FloorName(g.Key),
                RoomCount = g.Count(),
                AvailableBedCount = beds.Count(b => g.Any(r => r.Id == b.RoomId) && status.GetValueOrDefault(b.Id) == "Available")
            })
            .OrderBy(x => FloorOrder(x.FloorCode))
            .ThenBy(x => x.FloorCode, StringComparer.OrdinalIgnoreCase)
            .ToList();

        // Lantai bawaan: bed Tersedia terbanyak; seri → urutan lantai terkecil (backend 15.7).
        string? floorCode = string.IsNullOrWhiteSpace(requestedFloor)
            ? floors.OrderByDescending(x => x.AvailableBedCount).ThenBy(x => FloorOrder(x.FloorCode))
                .ThenBy(x => x.FloorCode, StringComparer.OrdinalIgnoreCase).FirstOrDefault()?.FloorCode
            : floors.FirstOrDefault(x => string.Equals(x.FloorCode, requestedFloor.Trim(), StringComparison.OrdinalIgnoreCase))?.FloorCode
                ?? requestedFloor.Trim();

        var floorRooms = floorCode == null ? [] : rooms
            .Where(x => string.Equals(x.FloorCode!.Trim(), floorCode, StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.FloorPlanRoomKey, StringComparer.Ordinal)
            .ToList();
        var floorRoomIds = floorRooms.Select(x => x.Id).ToHashSet();
        return new FloorData(floorRooms, beds.Where(x => floorRoomIds.Contains(x.RoomId)).ToList(), projection, floorCode, floors);
    }

    /// <summary>Lantai berangka diurutkan menurut angkanya; lantai berhuruf sesudahnya.</summary>
    private static (int Group, int Number) FloorOrder(string floorCode) =>
        int.TryParse(floorCode, out var number) ? (0, number) : (1, 0);

    // ---------------------------------------------------------------- proyeksi kamar dan bed

    private async Task<FloorPlanResponse> ProjectAsync(FloorData data, SelectionContext? selection, CancellationToken ct)
    {
        var rows = data.Projection.Rows.ToDictionary(x => x.BedId);
        var identityByUnit = new Dictionary<Guid, bool>();
        var response = new FloorPlanResponse
        {
            AsOfUtc = DateTime.UtcNow,
            FloorCode = data.FloorCode,
            FloorName = data.FloorCode == null ? null : FloorPlanRoomNaming.FloorName(data.FloorCode),
            Floors = data.Floors
        };

        foreach (var room in data.Rooms)
        {
            var roomBeds = data.Beds.Where(x => x.RoomId == room.Id)
                .OrderBy(x => FloorPlanRoomNaming.NormalizeBedLetter(x.BedNumber) is { } l ? FloorPlanRoomNaming.LetterToNumber(l) : int.MaxValue)
                .ThenBy(x => x.BedName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            // Boks rawat gabung dihitung terpisah, kecuali seluruh bed kamar adalah boks (mis. Ruang Bayi).
            var allCribs = roomBeds.Count > 0 && roomBeds.All(x => x.IsForNewborn);
            var regular = allCribs ? roomBeds : roomBeds.Where(x => !x.IsForNewborn).ToList();
            var cribs = allCribs ? [] : roomBeds.Where(x => x.IsForNewborn).ToList();
            string StatusOf(MstBed bed) => rows.TryGetValue(bed.Id, out var row) ? row.StatusCode : "Unavailable";

            var roomStatus = !room.IsAvailableForAdmission || regular.Count == 0 || regular.All(x => StatusOf(x) == "Unavailable")
                ? "Unavailable"
                : regular.Any(x => StatusOf(x) == "Available") ? "Available"
                : regular.Any(x => StatusOf(x) is "WaitingCleaning" or "Cleaning") ? "Preparing"
                : "Full";

            if (!identityByUnit.TryGetValue(room.ServiceUnitId, out var canReadIdentity))
            {
                // Profil OperationalOnly (Housekeeping) tidak menerima ringkasan HD/LD (GUARD-DNH-01).
                canReadIdentity = await access.CanReadLoadedIdentityAsync(room.ServiceUnitId, room.ServiceUnitId, false, ct);
                identityByUnit[room.ServiceUnitId] = canReadIdentity;
            }

            var roomResponse = new FloorPlanRoomResponse
            {
                RoomId = room.Id,
                FloorPlanRoomKey = room.FloorPlanRoomKey!,
                RoomName = room.RoomName,
                RoomNumber = room.RoomNumber,
                WingDirection = room.WingDirection.HasValue
                    ? new FloorPlanWingResponse { Code = (int)room.WingDirection.Value, Label = FloorPlanRoomNaming.WingLabel(room.WingDirection.Value) }
                    : null,
                LocationName = room.LocationName,
                ServiceUnitName = room.ServiceUnit?.ServiceUnitName,
                PatientClassName = room.PatientClass?.PatientClassName,
                IsSpecialClass = room.PatientClass != null && InpBedClassComparisonService.IsSpecialClass(room.PatientClass.PatientClassType),
                RoomStatus = new FloorPlanCodeLabelResponse { Code = roomStatus, Label = RoomStatusLabels[roomStatus] },
                Counts = Count(regular, StatusOf),
                CribCounts = Count(cribs, StatusOf),
                HasAvailableCrib = cribs.Any(x => StatusOf(x) == "Available"),
                DependencySummary = canReadIdentity ? Dependency(roomBeds, data.Projection) : null
            };

            foreach (var bed in roomBeds)
            {
                var bedResponse = ToBedResponse(bed, rows.TryGetValue(bed.Id, out var row) ? row : null);
                if (selection != null)
                    await ApplySelectionAsync(bedResponse, bed, room, row, data.Projection, selection, ct);
                roomResponse.Beds.Add(bedResponse);
            }

            // Outline hijau: Monitoring = ada bed biasa Tersedia; Pilih Bed = ada bed Tersedia yang layak.
            roomResponse.IsSelectable = selection == null
                ? regular.Any(x => StatusOf(x) == "Available")
                : roomResponse.Beds.Any(x => x.StatusCode == "Available" && x.Eligibility?.IsEligible == true);
            response.Rooms.Add(roomResponse);
        }
        return response;
    }

    private static FloorPlanCountsResponse Count(List<MstBed> beds, Func<MstBed, string> statusOf) => new()
    {
        Capacity = beds.Count(x => statusOf(x) != "Unavailable"),
        Filled = beds.Count(x => statusOf(x) is "Occupied" or "Reserved"),
        Available = beds.Count(x => statusOf(x) == "Available")
    };

    /// <summary>Ringkasan HD/LD episode penghuni bed Terisi, mis. 1 HD + 1 LD → {1, 1, 0}, High Dependency.</summary>
    private static FloorPlanDependencySummaryResponse Dependency(List<MstBed> beds, BedProjectionBatch projection)
    {
        var summary = new FloorPlanDependencySummaryResponse();
        foreach (var bed in beds)
        {
            if (!projection.Placements.TryGetValue(bed.Id, out var placement)) continue;
            var level = projection.EpisodeCareDependency.GetValueOrDefault(placement.EpisodeId);
            if (level == InpCareDependencyLevel.High) summary.HighCount++;
            else if (level == InpCareDependencyLevel.Low) summary.LowCount++;
            else summary.UnassessedCount++;
        }
        summary.IsHighDependency = summary.HighCount >= 1;
        return summary;
    }

    private static FloorPlanBedResponse ToBedResponse(MstBed bed, BedOperationalResponse? row)
    {
        row ??= new BedOperationalResponse { BedId = bed.Id, BedCode = bed.BedCode, BedName = bed.BedName, RoomId = bed.RoomId };
        return new FloorPlanBedResponse
        {
            BedId = row.BedId, RawBedStatus = row.RawBedStatus, RawRoomCode = row.RawRoomCode, RawRoomCapacity = row.RawRoomCapacity,
            BedCode = row.BedCode, BedName = row.BedName, RoomId = row.RoomId, RoomName = row.RoomName,
            ServiceUnitId = row.ServiceUnitId, PatientClassId = row.PatientClassId, PatientClassName = row.PatientClassName,
            IsIsolationBed = bed.IsIsolationBed, IsForNewborn = bed.IsForNewborn, IsIntensiveCareBed = bed.IsIntensiveCareBed,
            BedNumber = bed.BedNumber,
            StatusCode = row.StatusCode, StatusLabel = row.StatusLabel, CleaningPhase = row.CleaningPhase, IsBookable = row.IsBookable,
            OperationalVersion = row.OperationalVersion, CycleId = row.CycleId, ConflictCode = row.ConflictCode,
            BlockReasonCodes = row.BlockReasonCodes, AvailableActions = row.AvailableActions, HoldingContext = row.HoldingContext
        };
    }

    /// <summary>
    /// Kelayakan satu bed untuk episode Pilih Bed, dievaluasi di memori dengan aturan isolasi dan boks
    /// bayi existing; transfer ditambah hasil perbandingan kelas dan kategori yang boleh dipilih.
    /// HD/LD tidak dibaca di sini (INV-DNH-05).
    /// </summary>
    private async Task ApplySelectionAsync(FloorPlanBedResponse response, MstBed bed, MstRoom room, BedOperationalResponse? row,
        BedProjectionBatch projection, SelectionContext selection, CancellationToken ct)
    {
        projection.Roots.TryGetValue(bed.Id, out var root);
        IReadOnlyList<InpBedReservation>? reservations = projection.Reservations.TryGetValue(bed.Id, out var reservation) ? [reservation] : null;
        IReadOnlyList<InpBedPlacement>? placements = projection.Placements.TryGetValue(bed.Id, out var placement) ? [placement] : null;
        var evaluation = occupancy.EvaluatePlacementEligibilityCore(selection.Episode, selection.Patient, bed, room,
            selection.PlacementContext, row ?? new BedOperationalResponse(), root, reservations, placements, selection.MotherRoomId);
        response.Eligibility = new FloorPlanBedEligibilityResponse
        {
            IsEligible = evaluation.IsEligible,
            Failures = evaluation.Failures.Select(x => new FloorPlanEligibilityFailureResponse { Code = x.Code, Message = x.Message }).ToList()
        };

        if (selection.SourceClassId.HasValue && room.PatientClassId.HasValue)
        {
            response.ComparisonResult = await classComparison.CompareAsync(selection.SourceClassId.Value, room.PatientClassId.Value, ct);
            response.AllowedManualCategories = InpBedClassComparisonService.AllowedCategories(response.ComparisonResult);
        }
    }
}
