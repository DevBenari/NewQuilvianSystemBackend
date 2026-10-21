using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;

/// <summary>Adapter transaksi writer master existing; semua guard berjalan sesudah lock, sebelum writer dipanggil.</summary>
public sealed class BedMutationService(ApplicationDbContext db, InpBedMutationCoordinator coordinator, InpBedAccessService access, QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services.NumberSeriesAllocator numbers)
{
    public async Task<Guid> CycleAsync(Guid bedId, CancellationToken ct = default) =>
        await db.Set<InpBedReadiness>().AsNoTracking().Where(x => x.BedId == bedId).Select(x => x.CycleId).SingleOrDefaultAsync(ct);
    public async Task<List<BedVersionExpectation>> VersionsAsync(string kind, Guid id, CancellationToken ct = default)
    {
        var ids = await AffectedAsync(kind, id, ct);
        var roots = await db.Set<InpBedReadiness>().AsNoTracking().Where(x => ids.Contains(x.BedId)).ToDictionaryAsync(x => x.BedId, x => x.Version, ct);
        return ids.OrderBy(x => x).Select(x => new BedVersionExpectation { BedId = x, ExpectedVersion = roots.GetValueOrDefault(x, 1) }).ToList();
    }
    public async Task<Guid[]> AffectedAsync(string kind, Guid id, CancellationToken ct)
    {
        var q = db.Set<MstBed>().AsNoTracking().Where(x => !x.IsDelete);
        q = kind switch
        {
            "Bed" => q.Where(x => x.Id == id),
            "Room" => q.Where(x => x.RoomId == id),
            "ServiceUnit" => q.Where(x => x.Room != null && x.Room.ServiceUnitId == id),
            "PatientClass" => q.Where(x => x.Room != null && x.Room.PatientClassId == id),
            _ => throw new InpBedOperationException(400, "INVALID_INPUT", "Jenis master tidak dikenal.")
        };
        return await q.Select(x => x.Id).OrderBy(x => x).ToArrayAsync(ct);
    }
    public async Task<IActionResult> ExecuteAsync(string kind, string action, Guid id, object request,
        Func<Task<IActionResult>> writer, CancellationToken ct)
    {
        var operation = kind + ":" + (action == "Create" ? "Create" : action == "Delete" ? "Delete" : "Update");
        var json = JsonSerializer.SerializeToElement(request);
        if (kind == "Bed" && json.TryGetProperty("BedStatus", out var status) &&
            (status.GetInt32() is not (1 or 5 or 6 or 7)))
            throw new InpBedOperationException(422, "BED_UNAVAILABLE", "Status manual hanya Available, Maintenance, Blocked atau Inactive.");
        var ids = action == "Create" ? Array.Empty<Guid>() : await AffectedAsync(kind, id, ct);
        var versions = kind == "Bed" && action != "Create"
            ? new List<BedVersionExpectation> { new() { BedId = id, ExpectedVersion = json.TryGetProperty("ExpectedBedVersion", out var v) ? v.GetInt64() : 0 } }
            : json.TryGetProperty("AffectedBedVersions", out var list) ? list.Deserialize<List<BedVersionExpectation>>() ?? [] : [];
        var newRoom = json.TryGetProperty("RoomId", out var room) && room.ValueKind == JsonValueKind.String ? room.GetGuid() : (Guid?)null;
        var newUnit = json.TryGetProperty("ServiceUnitId", out var unit) && unit.ValueKind == JsonValueKind.String ? unit.GetGuid() : (Guid?)null;
        var newClass = json.TryGetProperty("PatientClassId", out var cls) && cls.ValueKind == JsonValueKind.String ? cls.GetGuid() : (Guid?)null;
        IActionResult? response = null;
        var meta = await coordinator.ExecuteAsync(operation, new { action, id, request }, [], ids, versions, async token =>
        {
            var actual = action == "Create" ? Array.Empty<Guid>() : await AffectedAsync(kind, id, token);
            if (!ids.SequenceEqual(actual)) throw new InpBedOperationException(409, "STALE_BED_VERSION", "Set bed terdampak berubah. Muat ulang.");
            var old = await CurrentMasterAsync(kind, id, token);
            var allowedUnits = await access.UnitsAsync(operation, token);
            if (allowedUnits.Count == 0 || (old is MstRoom scopedRoom && !allowedUnits.Contains(scopedRoom.ServiceUnitId)) ||
                (kind == "ServiceUnit" && !allowedUnits.Contains(id)) || (newUnit.HasValue && !allowedUnits.Contains(newUnit.Value)))
                throw new InpBedOperationException(403, "ACCESS_DENIED", "Master berada di luar scope unit.");
            var dangerous = action is "Delete" or "Availability" || HasAvailabilityChange(kind, old, json);
            if (dangerous)
            {
                if (!json.TryGetProperty("OperationReason", out var reason) || string.IsNullOrWhiteSpace(reason.GetString()) || reason.GetString()!.Trim().Length > 500)
                    throw new InpBedOperationException(422, "REASON_REQUIRED", "Alasan perubahan operasional wajib, maksimum 500 karakter.");
                // BE-RWI-235 (VAL-DNH-07): penolakan perubahan kamar menyebut jumlah penghuni dan pesanan.
                if (kind == "Room" && old is MstRoom heldRoom && ids.Length > 0)
                    await RequireRoomEmptyAsync(heldRoom.RoomName, ids, token);
                foreach (var bedId in ids) await coordinator.RequireEmptyAsync(bedId, token);
            }
            if (newRoom.HasValue)
            {
                var target = await db.Set<MstRoom>().AsNoTracking().SingleOrDefaultAsync(x => x.Id == newRoom && !x.IsDelete && x.IsActive, token)
                    ?? throw new InpBedOperationException(422, "BED_UNAVAILABLE", "Kamar tujuan tidak aktif.");
                if (!(await access.UnitsAsync(operation, token)).Contains(target.ServiceUnitId))
                    throw new InpBedOperationException(403, "ACCESS_DENIED", "Kamar tujuan di luar scope.");
            }
            response = await writer();
            if (response is not ObjectResult objectResult || (objectResult.StatusCode ?? 200) >= 300)
                throw new MasterWriteRejectedException(response);
            var resultId = id;
            if (action == "Create")
            {
                var envelope = JsonSerializer.SerializeToElement(objectResult.Value);
                if (!envelope.TryGetProperty("Data", out var data) || !data.TryGetProperty("Id", out var createdId))
                    throw new InvalidOperationException("Respons master create tidak memuat Data.Id.");
                resultId = createdId.GetGuid();
                var root = await coordinator.RootAsync(resultId, access.ActorId, token);
                db.Set<InpBedLifecycleEvent>().Add(new() { ReadinessId = root.Id, CycleId = root.CycleId,
                    Action = "BedCreated", ActorUserId = access.ActorId, CreateBy = access.ActorId,
                    BeforeState = root.State, AfterState = root.State, OccurredAtUtc = DateTime.UtcNow, RecordedAtUtc = DateTime.UtcNow });
            }
            else if (dangerous)
            {
                foreach (var bedId in ids)
                {
                    await coordinator.InvalidateAsync(bedId, access.ActorId, null, token);
                    var bed = await db.Set<MstBed>().SingleAsync(x => x.Id == bedId, token);
                    if (bed.BedStatus == BedStatus.Available) bed.BedStatus = BedStatus.Cleaning;
                }
            }
            return (kind, resultId);
        }, ct, extraUnitIds: (kind == "ServiceUnit" ? new[] { id } : Array.Empty<Guid>()).Concat(newUnit.HasValue ? [newUnit.Value] : []),
            extraRoomIds: (kind == "Room" ? new[] { id } : Array.Empty<Guid>()).Concat(newRoom.HasValue ? [newRoom.Value] : []),
            extraClassIds: (kind == "PatientClass" ? new[] { id } : Array.Empty<Guid>()).Concat(newClass.HasValue ? [newClass.Value] : []));
        if (meta.IsReplay)
        {
            // Replay hanya membaca DTO hasil terkini, tanpa menjalankan writer lagi.
            response = await ReadResultAsync(kind, action, meta.ResultEntityId, ct);
        }
        if (response is ObjectResult result && result.Value != null)
        {
            var data = result.Value.GetType().GetProperty("Data")?.GetValue(result.Value);
            data?.GetType().GetProperty("OperationMeta")?.SetValue(data, meta);
            if (kind == "Bed")
            {
                var root = await db.Set<InpBedReadiness>().AsNoTracking().SingleAsync(x => x.BedId == meta.ResultEntityId, ct);
                data?.GetType().GetProperty("OperationalVersion")?.SetValue(data, root.Version);
                data?.GetType().GetProperty("CycleId")?.SetValue(data, root.CycleId);
            }
        }
        return response!;
    }
    public Task<IActionResult> CreateBedAsync(QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs.CreateBedRequest request,
        Func<string, Task<IActionResult>> writer, CancellationToken ct) =>
        ExecuteAsync("Bed", "Create", Guid.Empty, request, async () => await writer(await AllocateBedCodeAsync(ct)), ct);

    internal async Task<string> AllocateBedCodeAsync(CancellationToken ct)
    {
        var codes = await db.Set<MstBed>().IgnoreQueryFilters().AsNoTracking().Where(x => x.BedCode.StartsWith("BD-RSMMC-")).Select(x => x.BedCode).ToListAsync(ct);
        var floor = codes.Select(x => long.TryParse(x[9..], out var n) ? n : 0).DefaultIfEmpty(0).Max();
        return await numbers.AllocateAsync(new("MST_BED", "BD-RSMMC", "NEVER", 5, access.ActorId, DateTimeOffset.UtcNow, floor), ct);
    }

    /// <summary>
    /// Kode kamar sistem <c>RM-RSMMC-#####</c> dari pengalokasi nomor bersama (BE-RWI-232, <c>DNH-DES-01</c>).
    /// Batas bawahnya nomor terbesar yang pernah dipakai, termasuk kamar yang sudah dihapus lunak,
    /// sehingga kode tidak pernah bentrok dengan index unik <c>RoomCode</c> yang tidak berfilter.
    /// </summary>
    /// <example>Kamar RM-RSMMC-00001..00003 ada dan 00003 dihapus lunak → kamar baru RM-RSMMC-00004, bukan 00003.</example>
    public async Task<string> AllocateRoomCodeAsync(CancellationToken ct)
    {
        var codes = await db.Set<MstRoom>().IgnoreQueryFilters().AsNoTracking().Where(x => x.RoomCode.StartsWith("RM-RSMMC-")).Select(x => x.RoomCode).ToListAsync(ct);
        var floor = codes.Select(x => long.TryParse(x[9..], out var n) ? n : 0).DefaultIfEmpty(0).Max();
        return await numbers.AllocateAsync(new("MST_ROOM", "RM-RSMMC", "NEVER", 5, access.ActorId, DateTimeOffset.UtcNow, floor), ct);
    }
    /// <summary>
    /// Menolak perubahan berbahaya pada kamar yang masih dihuni atau dipesan, dengan pesan yang
    /// menyebut jumlahnya (BE-RWI-235, API 14.4 <c>BED_HOLD_CONFLICT</c>).
    /// </summary>
    /// <example>Kamar "Kelas 2 329" dengan dua penempatan aktif → "Kelas 2 329 masih dihuni 2 pasien. …".</example>
    private async Task RequireRoomEmptyAsync(string roomName, Guid[] bedIds, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var occupied = await db.Set<InpBedPlacement>().CountAsync(x => bedIds.Contains(x.BedId) && x.EndDateTime == null && !x.IsDelete, ct);
        var reserved = await db.Set<InpBedReservation>().CountAsync(x => bedIds.Contains(x.BedId) &&
            x.ReservationStatus == InpBedReservationStatus.Active && !x.IsDelete && x.ExpiresAt > now, ct);
        if (occupied == 0 && reserved == 0) return;
        var holders = occupied > 0 && reserved > 0 ? $"masih dihuni {occupied} pasien dan memiliki {reserved} pesanan aktif"
            : occupied > 0 ? $"masih dihuni {occupied} pasien" : $"masih memiliki {reserved} pesanan aktif";
        throw new InpBedOperationException(409, "BED_HOLD_CONFLICT", $"{roomName} {holders}. Perubahan ini baru dapat disimpan setelah kamar kosong.");
    }

    /// <summary>
    /// Operasi multi-entitas denah — aktivasi ruang cadangan (BE-RWI-236) dan impor (BE-RWI-237) —
    /// di bawah satu <c>coordinator.ExecuteAsync</c>: satu transaksi, satu receipt, satu commit.
    /// </summary>
    public Task<BedOperationMeta> ExecuteFloorPlanAsync(string operation, object request,
        IEnumerable<Guid> unitIds, IEnumerable<Guid> roomIds, IEnumerable<Guid> classIds,
        Func<CancellationToken, Task<(string Kind, Guid Id)>> mutate, CancellationToken ct) =>
        coordinator.ExecuteAsync(operation, request, [], [], [], mutate, ct,
            extraUnitIds: unitIds, extraRoomIds: roomIds, extraClassIds: classIds);

    /// <summary>
    /// Menambah satu bed denah berkode sistem <c>BD-RSMMC-#####</c> dan berlabel <c>&lt;nomor kamar&gt;-&lt;huruf&gt;</c>,
    /// dengan penanda turunan kelas, kesiapan <c>Unverified</c>, dan event <c>BedCreated</c>.
    /// Dipanggil di dalam <see cref="ExecuteFloorPlanAsync"/>; tidak menyimpan sendiri.
    /// </summary>
    internal async Task<MstBed> AddFloorPlanBedAsync(MstRoom room, PatientClassType? classType, string bedLetter, bool isCrib, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var bed = new MstBed
        {
            Id = Guid.NewGuid(), RoomId = room.Id, BedCode = await AllocateBedCodeAsync(ct),
            BedNumber = bedLetter, BedName = FloorPlanRoomNaming.BedLabel(room.RoomNumber!, bedLetter),
            BedStatus = BedStatus.Available, IsReservable = true, IsActive = true,
            CreateDateTime = now, CreateBy = access.ActorId, IsDelete = false, IsCancel = false
        };
        FloorPlanRoomNaming.ApplyBedFlags(bed, classType, isCrib);
        db.Set<MstBed>().Add(bed);
        var root = await coordinator.RootAsync(bed.Id, access.ActorId, ct);
        db.Set<InpBedLifecycleEvent>().Add(new() { ReadinessId = root.Id, CycleId = root.CycleId,
            Action = "BedCreated", ActorUserId = access.ActorId, CreateBy = access.ActorId,
            BeforeState = root.State, AfterState = root.State, OccurredAtUtc = now, RecordedAtUtc = now });
        return bed;
    }

    /// <summary>
    /// Mengantrekan pembentukan label bed pada satu lantai sampai transaksi selesai, supaya dua
    /// penambahan bed bersamaan di lantai yang sama tidak memakai label yang sama (INV-DNH-02).
    /// </summary>
    internal static async Task LockFloorBedLabelsAsync(ApplicationDbContext db, string floorCode, CancellationToken ct)
    {
        if (!db.Database.IsRelational() || db.Database.CurrentTransaction == null) return;
        var key = "MstBedLabel:" + floorCode.Trim().ToLowerInvariant();
        await db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock(hashtextextended({key}, 0))", ct);
    }

    /// <summary>
    /// Huruf bed yang sudah dipakai awalan nomor kamar yang sama di satu lantai, termasuk bed
    /// nonaktif (backend 15.6). Kamar 321 kedua di lantai yang sama melanjutkan huruf kamar pertama.
    /// </summary>
    internal static async Task<List<string?>> UsedBedLettersAsync(ApplicationDbContext db, string floorCode, string roomNumber, CancellationToken ct)
    {
        var floor = floorCode.Trim().ToLower();
        var number = roomNumber.Trim().ToLower();
        var rows = await db.Set<MstBed>().AsNoTracking()
            .Where(x => !x.IsDelete && x.Room != null && !x.Room.IsDelete && x.Room.FloorCode != null &&
                x.Room.FloorCode.ToLower() == floor && x.Room.RoomNumber != null && x.Room.RoomNumber.ToLower() == number)
            .Select(x => new { x.BedNumber, x.BedName })
            .ToListAsync(ct);
        return rows.SelectMany(x => new[] { FloorPlanRoomNaming.NormalizeBedLetter(x.BedNumber), FloorPlanRoomNaming.LetterFromLabel(x.BedName, roomNumber) }).ToList();
    }

    /// <summary>Label bed (huruf kecil) milik bed yang belum dihapus di satu lantai (INV-DNH-02).</summary>
    internal static async Task<HashSet<string>> FloorBedLabelsAsync(ApplicationDbContext db, string floorCode, Guid? excludeBedId, CancellationToken ct)
    {
        var floor = floorCode.Trim().ToLower();
        var labels = await db.Set<MstBed>().AsNoTracking()
            .Where(x => !x.IsDelete && (!excludeBedId.HasValue || x.Id != excludeBedId.Value) && x.Room != null && !x.Room.IsDelete &&
                x.Room.FloorCode != null && x.Room.FloorCode.ToLower() == floor)
            .Select(x => x.BedName)
            .ToListAsync(ct);
        return labels.Select(x => x.Trim().ToLowerInvariant()).ToHashSet();
    }

    private async Task<IActionResult> ReadResultAsync(string kind, string action, Guid id, CancellationToken ct)
    {
        var entity = await CurrentMasterAsync(kind, id, ct)
            ?? throw new InpBedOperationException(404, "NOT_FOUND", "Hasil operasi tidak ditemukan.");
        var suffix = action == "Create" ? "CreateResponse" : action == "Delete" ? "DeleteResponse" : "UpdateResponse";
        var dtoType = typeof(QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs.BedUpdateResponse).Assembly.GetType(
            "QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs." + kind + suffix)
            ?? throw new InvalidOperationException("DTO master tidak ditemukan.");
        var data = JsonSerializer.Deserialize(JsonSerializer.Serialize(entity), dtoType)!;
        foreach (var field in new[] { "BedStatus", "RoomType", "ServiceUnitType", "PatientClassType" })
        {
            var value = entity.GetType().GetProperty(field)?.GetValue(entity);
            data.GetType().GetProperty(field + "Name")?.SetValue(data, value?.ToString());
        }
        foreach (var field in new[] { "CreateBy", "UpdateBy", "DeleteBy" })
        {
            if (entity.GetType().GetProperty(field)?.GetValue(entity) is Guid actor && actor != Guid.Empty)
            {
                var name = await db.Users.Where(x => x.Id == actor).Select(x => x.DisplayName).SingleOrDefaultAsync(ct);
                data.GetType().GetProperty(field + "Name")?.SetValue(data, name);
            }
        }
        return new OkObjectResult(QuilvianSystemBackend.Responses.ApiResponse<object>.Ok(data, "Hasil operasi tersimpan."));
    }
    private async Task<object?> CurrentMasterAsync(string kind, Guid id, CancellationToken ct) => kind switch
    {
        "Bed" => await db.Set<MstBed>().AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct),
        "Room" => await db.Set<MstRoom>().AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct),
        "ServiceUnit" => await db.Set<MstServiceUnit>().AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct),
        "PatientClass" => await db.Set<MstPatientClass>().AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct),
        _ => null
    };
    private static bool HasAvailabilityChange(string kind, object? previous, JsonElement request)
    {
        if (previous == null) return false;
        var old = JsonSerializer.SerializeToElement(previous);
        var fields = kind switch
        {
            // BE-RWI-240 (RWI-DEC-315): penanda jenis kelamin dicabut dari bed dan kamar.
            "Bed" => new[] { "RoomId", "BedStatus", "IsActive", "IsReservable", "IsForNewborn", "IsIsolationBed", "IsIntensiveCareBed", "IsOdcBed" },
            "Room" => ["ServiceUnitId", "PatientClassId", "IsActive", "IsAvailableForAdmission", "RoomType", "IsForNewborn", "IsIsolationRoom", "Capacity"],
            _ => ["IsActive"]
        };
        return fields.Any(f => request.TryGetProperty(f, out var next) && next.ValueKind != JsonValueKind.Null &&
            old.TryGetProperty(f, out var value) && value.GetRawText() != next.GetRawText());
    }
}
public sealed class MasterWriteRejectedException(IActionResult result) : Exception
{ public IActionResult Result { get; } = result; }

