using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;

/// <summary>
/// Susunan satu lantai untuk editor Master Bed dan aktivasi ruang cadangan (BE-RWI-236, backend 15.4).
/// </summary>
/// <remarks>
/// <b>Baca tanpa identitas pasien.</b> Editor master hanya menerima jumlah penghuni dan pesanan,
/// tidak pernah nama maupun No. RM (VAL-DNH-20).
///
/// <para>
/// <b>Aktivasi atomik.</b> Kamar dan seluruh bed-nya dibuat di bawah satu
/// <c>BedMutationService.ExecuteFloorPlanAsync</c>: satu transaksi, satu receipt <c>Room</c>, dan
/// event <c>BedCreated</c> per bed. Kegagalan di tengah jalan tidak meninggalkan kamar tanpa bed.
/// </para>
/// </remarks>
public sealed class FloorPlanLayoutService(
    ApplicationDbContext db,
    BedMutationService bedMutation,
    InpBedManagementQueryService bedQueries,
    InpBedAccessService access)
{
    private const string ActivationOperation = "Room:Create";

    public async Task<FloorPlanLayoutResponse> GetLayoutAsync(FloorPlanLayoutQuery query, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(query?.FloorCode))
            throw new InpBedOperationException(400, "FLOOR_CODE_REQUIRED", "Pilih lantai denah lebih dulu.");
        var floorCode = query.FloorCode.Trim();
        var floor = floorCode.ToLower();

        var rooms = await db.Set<MstRoom>().AsNoTracking()
            .Include(x => x.ServiceUnit)
            .Include(x => x.PatientClass)
            .Where(x => !x.IsDelete && x.FloorPlanRoomKey != null && x.FloorCode != null && x.FloorCode.ToLower() == floor)
            .OrderBy(x => x.FloorPlanRoomKey)
            .ToListAsync(ct);
        var roomIds = rooms.Select(x => x.Id).ToList();

        var beds = await db.Set<MstBed>().AsNoTracking()
            .Where(x => !x.IsDelete && roomIds.Contains(x.RoomId))
            .ToListAsync(ct);
        foreach (var bed in beds) bed.Room = rooms.First(x => x.Id == bed.RoomId);
        var bedIds = beds.Select(x => x.Id).ToList();

        // Status bed dari proyeksi enam status yang sama dengan Monitoring; identitas tidak dipakai.
        var projection = await bedQueries.ProjectBatchAsync(beds, ct);
        var rows = projection.Rows.ToDictionary(x => x.BedId);

        var usedPlacements = await db.Set<InpBedPlacement>().IgnoreQueryFilters().AsNoTracking()
            .Where(x => bedIds.Contains(x.BedId)).Select(x => x.BedId).Distinct().ToListAsync(ct);
        var usedReservations = await db.Set<InpBedReservation>().IgnoreQueryFilters().AsNoTracking()
            .Where(x => bedIds.Contains(x.BedId)).Select(x => x.BedId).Distinct().ToListAsync(ct);
        var usedBeds = usedPlacements.Concat(usedReservations).ToHashSet();

        var withoutKey = await db.Set<MstRoom>().AsNoTracking()
            .Where(x => !x.IsDelete && x.FloorPlanRoomKey == null && x.ServiceUnit != null &&
                x.ServiceUnit.ServiceUnitType == ServiceUnitType.Inpatient && !x.ServiceUnit.IsDelete)
            .OrderBy(x => x.RoomName)
            .Select(x => new FloorPlanRoomWithoutKeyResponse
            {
                RoomId = x.Id, RoomCode = x.RoomCode, RoomName = x.RoomName, FloorName = x.FloorName,
                ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null
            })
            .ToListAsync(ct);

        return new FloorPlanLayoutResponse
        {
            FloorCode = floorCode,
            FloorName = FloorPlanRoomNaming.FloorName(floorCode),
            RoomsWithoutFloorPlanKey = withoutKey,
            Rooms = rooms.Select(room =>
            {
                var roomBeds = beds.Where(x => x.RoomId == room.Id)
                    .OrderBy(x => x.BedNumber == null ? int.MaxValue : FloorPlanRoomNaming.NormalizeBedLetter(x.BedNumber) is { } l ? FloorPlanRoomNaming.LetterToNumber(l) : int.MaxValue)
                    .ThenBy(x => x.BedName)
                    .ToList();
                return new FloorPlanLayoutRoomResponse
                {
                    RoomId = room.Id,
                    FloorPlanRoomKey = room.FloorPlanRoomKey!,
                    RoomCode = room.RoomCode,
                    RoomNumber = room.RoomNumber,
                    RoomName = room.RoomName,
                    PatientClassId = room.PatientClassId,
                    PatientClassName = room.PatientClass?.PatientClassName,
                    ServiceUnitId = room.ServiceUnitId,
                    ServiceUnitName = room.ServiceUnit?.ServiceUnitName,
                    WingDirection = room.WingDirection,
                    WingDirectionName = room.WingDirection.HasValue ? FloorPlanRoomNaming.WingLabel(room.WingDirection.Value) : null,
                    IsActive = room.IsActive,
                    IsAvailableForAdmission = room.IsAvailableForAdmission,
                    OccupiedCount = roomBeds.Count(x => projection.Placements.ContainsKey(x.Id)),
                    ReservedCount = roomBeds.Count(x => projection.Reservations.ContainsKey(x.Id)),
                    AffectedBedVersions = roomBeds.OrderBy(x => x.Id).Select(x => new BedVersionExpectation
                    {
                        BedId = x.Id,
                        ExpectedVersion = projection.Roots.TryGetValue(x.Id, out var root) ? root.Version : 1
                    }).ToList(),
                    Beds = roomBeds.Select(x => new FloorPlanLayoutBedResponse
                    {
                        BedId = x.Id,
                        BedCode = x.BedCode,
                        BedName = x.BedName,
                        BedNumber = x.BedNumber,
                        IsForNewborn = x.IsForNewborn,
                        IsActive = x.IsActive,
                        StatusCode = rows.TryGetValue(x.Id, out var row) ? row.StatusCode : "Unavailable",
                        StatusLabel = rows.TryGetValue(x.Id, out var labelRow) ? labelRow.StatusLabel : null,
                        HasUsageHistory = usedBeds.Contains(x.Id),
                        OperationalVersion = rows.TryGetValue(x.Id, out var versionRow) ? versionRow.OperationalVersion : 1
                    }).ToList()
                };
            }).ToList()
        };
    }

    /// <summary>
    /// Mengaktifkan ruang cadangan menjadi kamar berkunci beserta bed dan boks bayinya (VAL-DNH-09).
    /// Bed baru berkesiapan <c>Unverified</c> sampai disahkan perawat.
    /// </summary>
    public async Task<FloorPlanRoomActivationResponse> ActivateRoomAsync(ActivateFloorPlanRoomRequest request, CancellationToken ct)
    {
        if (request == null)
            throw new InpBedOperationException(400, "INVALID_INPUT", "Isian aktivasi ruang cadangan belum dikirim.");
        if (string.IsNullOrWhiteSpace(request.FloorCode))
            throw new InpBedOperationException(400, "FLOOR_CODE_REQUIRED", "Kode lantai wajib diisi.");
        if (!FloorPlanRoomNaming.IsValidFloorCode(request.FloorCode))
            throw new InpBedOperationException(400, "INVALID_INPUT", "Kode lantai hanya boleh angka atau huruf, maksimum 10 karakter.");
        if (string.IsNullOrWhiteSpace(request.FloorPlanRoomKey))
            throw new InpBedOperationException(400, "INVALID_INPUT", "Kunci denah ruang cadangan wajib diisi.");
        var keyError = FloorPlanRoomNaming.ValidateRoomKey(request.FloorPlanRoomKey, request.FloorCode);
        if (keyError != null)
            throw new InpBedOperationException(400, keyError.Value.Code, keyError.Value.Message);
        if (string.IsNullOrWhiteSpace(request.RoomNumber) || request.RoomNumber.Trim().Length > 50)
            throw new InpBedOperationException(400, "INVALID_INPUT", "Nomor kamar wajib diisi, maksimum 50 karakter.");
        if (request.PatientClassId == Guid.Empty || request.ServiceUnitId == Guid.Empty)
            throw new InpBedOperationException(400, "INVALID_INPUT", "Kelas dan unit layanan wajib dipilih.");
        if (!request.WingDirection.HasValue || !Enum.IsDefined(request.WingDirection.Value))
            throw new InpBedOperationException(400, "WING_DIRECTION_INVALID", "Arah wing harus salah satu dari delapan arah (nilai 1–8).");
        if (request.BedCount is < 1 or > 20 || request.CribCount is < 0 or > 10)
            throw new InpBedOperationException(400, "INVALID_INPUT", "Jumlah bed 1–20 dan jumlah boks bayi 0–10.");

        var floorCode = request.FloorCode.Trim();
        var key = request.FloorPlanRoomKey.Trim();
        var roomNumber = request.RoomNumber.Trim();

        var meta = await bedMutation.ExecuteFloorPlanAsync(ActivationOperation, request,
            [request.ServiceUnitId], [], [request.PatientClassId], async token =>
            {
                // GUARD-DNH-06: scope unit existing.
                if (!(await access.UnitsAsync(ActivationOperation, token)).Contains(request.ServiceUnitId))
                    throw new InpBedOperationException(403, "ACCESS_DENIED", "Unit layanan berada di luar kewenangan Anda.");

                var unit = await db.Set<MstServiceUnit>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.ServiceUnitId, token);
                if (unit == null || unit.IsDelete || !unit.IsActive || unit.ServiceUnitType != ServiceUnitType.Inpatient)
                    throw new InpBedOperationException(422, "ROOM_UNIT_INVALID", "Pilih unit rawat inap yang aktif.");

                var patientClass = await db.Set<MstPatientClass>().AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == request.PatientClassId && x.IsActive && !x.IsDelete, token)
                    ?? throw new InpBedOperationException(422, "PATIENT_CLASS_NOT_FOUND", "Kelas kamar tidak ditemukan atau nonaktif. Terapkan kelas resmi lebih dulu.");

                if (await db.Set<MstRoom>().AnyAsync(x => !x.IsDelete && x.FloorPlanRoomKey == key, token))
                    throw new InpBedOperationException(409, "FLOOR_PLAN_KEY_DUPLICATE", "Kunci denah sudah dipakai kamar lain. Periksa pemetaan; jangan membuat kamar kedua.");

                var floor = floorCode.ToLower();
                var number = roomNumber.ToLower();
                if (await db.Set<MstRoom>().AnyAsync(x => !x.IsDelete && x.IsActive && x.FloorCode != null && x.FloorCode.ToLower() == floor &&
                        x.RoomNumber != null && x.RoomNumber.ToLower() == number, token))
                    throw new InpBedOperationException(409, "ROOM_NUMBER_DUPLICATE", $"Nomor kamar {roomNumber} sudah dipakai kamar aktif di lantai ini. Pakai nomor lain.");

                await BedMutationService.LockFloorBedLabelsAsync(db, floorCode, token);
                var now = DateTime.UtcNow;
                var room = new MstRoom
                {
                    Id = Guid.NewGuid(),
                    ServiceUnitId = request.ServiceUnitId,
                    PatientClassId = request.PatientClassId,
                    RoomCode = await bedMutation.AllocateRoomCodeAsync(token),
                    RoomNumber = roomNumber,
                    RoomName = FloorPlanRoomNaming.RoomName(patientClass.PatientClassName, roomNumber, null),
                    FloorCode = floorCode,
                    WingDirection = request.WingDirection,
                    FloorPlanRoomKey = key,
                    FloorName = FloorPlanRoomNaming.FloorName(floorCode),
                    LocationName = FloorPlanRoomNaming.LocationName(floorCode, request.WingDirection),
                    Capacity = request.BedCount + request.CribCount,
                    IsAvailableForAdmission = true,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = access.ActorId,
                    IsDelete = false,
                    IsCancel = false
                };
                FloorPlanRoomNaming.ApplyRoomFlags(room, patientClass.PatientClassType);
                db.Set<MstRoom>().Add(room);

                var existingLabels = await BedMutationService.FloorBedLabelsAsync(db, floorCode, null, token);
                var start = FloorPlanRoomNaming.LetterToNumber(FloorPlanRoomNaming.NextBedLetter(
                    await BedMutationService.UsedBedLettersAsync(db, floorCode, roomNumber, token)));
                for (var i = 0; i < request.BedCount + request.CribCount; i++)
                {
                    var letter = FloorPlanRoomNaming.NumberToLetter(start + i);
                    var label = FloorPlanRoomNaming.BedLabel(roomNumber, letter);
                    if (existingLabels.Contains(label.ToLowerInvariant()))
                        throw new InpBedOperationException(409, "BED_LABEL_DUPLICATE", $"Label bed {label} sudah dipakai bed lain di lantai yang sama.");
                    await bedMutation.AddFloorPlanBedAsync(room, patientClass.PatientClassType, letter, isCrib: i >= request.BedCount, token);
                }
                return ("Room", room.Id);
            }, ct);

        return await ReadActivationAsync(meta, ct);
    }

    private async Task<FloorPlanRoomActivationResponse> ReadActivationAsync(BedOperationMeta meta, CancellationToken ct)
    {
        var room = await db.Set<MstRoom>().AsNoTracking().SingleOrDefaultAsync(x => x.Id == meta.ResultEntityId, ct)
            ?? throw new InpBedOperationException(404, "NOT_FOUND", "Kamar hasil aktivasi tidak ditemukan.");
        var beds = await db.Set<MstBed>().AsNoTracking().Where(x => x.RoomId == room.Id && !x.IsDelete).ToListAsync(ct);
        var creator = room.CreateBy == Guid.Empty ? null
            : await db.Users.AsNoTracking().Where(x => x.Id == room.CreateBy).Select(x => x.DisplayName ?? x.UserName).FirstOrDefaultAsync(ct);
        return new FloorPlanRoomActivationResponse
        {
            OperationMeta = meta,
            Room = new RoomCreateResponse
            {
                Id = room.Id,
                ServiceUnitId = room.ServiceUnitId,
                PatientClassId = room.PatientClassId,
                RoomCode = room.RoomCode,
                RoomName = room.RoomName,
                RoomType = room.RoomType,
                RoomTypeName = string.Concat(room.RoomType.ToString().Select((c, i) => i > 0 && char.IsUpper(c) ? " " + c : c.ToString())),
                RoomNumber = room.RoomNumber,
                FloorCode = room.FloorCode,
                WingDirection = room.WingDirection,
                FloorPlanRoomKey = room.FloorPlanRoomKey,
                IsActive = room.IsActive,
                CreateDateTime = room.CreateDateTime,
                CreateBy = room.CreateBy == Guid.Empty ? null : room.CreateBy,
                CreateByName = creator
            },
            Beds = beds
                .OrderBy(x => FloorPlanRoomNaming.NormalizeBedLetter(x.BedNumber) is { } l ? FloorPlanRoomNaming.LetterToNumber(l) : int.MaxValue)
                .Select(x => new FloorPlanActivatedBedResponse { BedId = x.Id, BedCode = x.BedCode, BedName = x.BedName, IsForNewborn = x.IsForNewborn })
                .ToList()
        };
    }
}
