using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Options;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;

/// <summary>
/// Memeriksa lalu mengimpor susunan kamar dan bed dari denah (BE-RWI-237, backend 15.4,
/// VAL-DNH-01, 05, 06, 18, 19, 23).
/// </summary>
/// <remarks>
/// <b>Urutan kerja.</b> Seluruh baris divalidasi lebih dulu dan setiap kunci denah mendapat hasil
/// <c>Created</c>, <c>Updated</c>, <c>Unchanged</c>, atau <c>Rejected</c> beserta kodenya. Dry run
/// berhenti di sini tanpa menulis apa pun dan tanpa receipt. Impor sungguhan menulis hanya baris
/// yang lolos, di bawah satu koordinator, satu receipt <c>FloorPlanImport</c>, dan satu
/// <see cref="MstFloorPlanImportRun"/>; kegagalan di tengah jalan membatalkan seluruhnya.
///
/// <para>
/// <b>Kamar existing tidak diubah</b> (INV-DNH-10): kelas, unit, dan bed-nya tetap; perbedaannya
/// dilaporkan dengan kode <c>FLOOR_PLAN_DRIFT</c>. Impor kedua dengan manifest yang sama
/// menghasilkan nol kamar dan nol bed baru.
/// </para>
/// </remarks>
public sealed class FloorPlanImportService(
    ApplicationDbContext db,
    BedMutationService bedMutation,
    InpBedMutationCoordinator coordinator,
    InpBedAccessService access,
    IOptionsMonitor<BedManagementOptions> options,
    IHostEnvironment environment)
{
    private const string Operation = "Room:ImportFloorPlan";
    private const int MaxRooms = 300;
    private const int MaxBedsPerRoom = 20;

    private static readonly Dictionary<string, string> CodeMessages = new()
    {
        ["ROOM_UNIT_MAPPING_MISSING"] = "Kamar belum dipetakan ke unit layanan.",
        ["ROOM_UNIT_INVALID"] = "Unit layanan tidak aktif atau bukan unit rawat inap.",
        ["ACCESS_DENIED"] = "Unit layanan kamar ini berada di luar kewenangan Anda.",
        ["PATIENT_CLASS_NOT_FOUND"] = "Kelas kamar tidak ditemukan atau nonaktif. Terapkan kelas resmi lebih dulu.",
        ["FLOOR_PLAN_KEY_DUPLICATE"] = "Kunci denah sudah dipakai kamar lain atau muncul lebih dari sekali pada manifest.",
        ["FLOOR_PLAN_KEY_IMMUTABLE"] = "Kamar yang dipilih sudah memiliki kunci atau lantai lain.",
        ["NOT_FOUND"] = "Kamar existing yang dipilih tidak ditemukan.",
        ["BED_LABEL_DUPLICATE"] = "Label bed sudah dipakai bed lain di lantai yang sama.",
        ["FLOOR_PLAN_DRIFT"] = "Kelas, unit, atau jumlah bed kamar existing berbeda dari denah; kamar tidak diubah."
    };

    private enum RowAction { None, Create, AttachExisting, RefreshExisting }

    private sealed class RowPlan
    {
        public required FloorPlanImportRoomRequest Source { get; init; }
        public required string Key { get; init; }
        public required string FloorCode { get; init; }
        public required string RoomNumber { get; init; }
        public RowAction Action { get; set; }
        public string Outcome { get; set; } = "Unchanged";
        public List<string> Codes { get; } = [];
        public Guid? ExistingRoomId { get; set; }
        public MstPatientClass? PatientClass { get; set; }
    }

    public async Task<FloorPlanImportResult> ImportAsync(FloorPlanImportRequest request, CancellationToken ct)
    {
        var startedAt = DateTime.UtcNow;
        ValidateFormat(request);
        var dryRun = request.DryRun!.Value;
        var manifestHash = ComputeManifestHash(request);
        var plans = await PlanAsync(request, ct);

        var result = BuildResult(plans, dryRun, manifestHash, bedsMarkedReady: 0);
        if (dryRun) return result;

        var importOptions = options.CurrentValue.FloorPlanImport;
        // Validator startup sudah menolak opsi ini di Production; diperiksa ulang supaya tidak pernah lolos.
        var markReady = importOptions.MarkImportedBedsReady && !environment.IsProduction() &&
            !string.IsNullOrWhiteSpace(importOptions.ReadyEvidenceReference);
        var evidence = markReady ? importOptions.ReadyEvidenceReference!.Trim() : null;

        var writable = plans.Where(x => x.Action != RowAction.None).ToList();
        var unitIds = writable.Where(x => x.Source.ServiceUnitId.HasValue).Select(x => x.Source.ServiceUnitId!.Value).Distinct().ToList();
        var classIds = writable.Where(x => x.PatientClass != null).Select(x => x.PatientClass!.Id).Distinct().ToList();
        var roomIds = writable.Where(x => x.ExistingRoomId.HasValue).Select(x => x.ExistingRoomId!.Value).Distinct().ToList();

        var meta = await bedMutation.ExecuteFloorPlanAsync(Operation, new { request.ManifestVersion, ManifestHash = manifestHash },
            unitIds, roomIds, classIds, async token =>
            {
                foreach (var floor in writable.Select(x => x.FloorCode.ToLowerInvariant()).Distinct().OrderBy(x => x, StringComparer.Ordinal))
                    await BedMutationService.LockFloorBedLabelsAsync(db, floor, token);

                // Kunci baru yang mendadak dipakai sesudah pemeriksaan → seluruh impor dibatalkan.
                var newKeys = writable.Where(x => x.Action == RowAction.Create).Select(x => x.Key).ToList();
                if (newKeys.Count > 0 && await db.Set<MstRoom>().AnyAsync(x => !x.IsDelete && x.FloorPlanRoomKey != null && newKeys.Contains(x.FloorPlanRoomKey), token))
                    throw new InpBedOperationException(409, "FLOOR_PLAN_KEY_DUPLICATE", "Sebagian kunci denah baru saja dipakai proses lain. Ulangi pemeriksaan impor.");

                var createdBeds = new List<MstBed>();
                var now = DateTime.UtcNow;
                foreach (var plan in writable)
                {
                    if (plan.Action == RowAction.Create)
                        createdBeds.AddRange(await CreateRoomWithBedsAsync(plan, now, token));
                    else
                        await LabelExistingRoomAsync(plan, now, token);
                }

                var bedsMarkedReady = 0;
                if (markReady)
                {
                    foreach (var bed in createdBeds)
                    {
                        var root = await coordinator.RootAsync(bed.Id, access.ActorId, token);
                        var before = root.State;
                        root.State = InpBedReadinessState.Ready;
                        root.VerifiedAtUtc = now;
                        root.VerifiedByUserId = access.ActorId;
                        root.VerificationReference = evidence;
                        db.Set<InpBedLifecycleEvent>().Add(new()
                        {
                            ReadinessId = root.Id, CycleId = root.CycleId, Action = "ImportReadyForTest",
                            BeforeState = before, AfterState = root.State, ActorUserId = access.ActorId, CreateBy = access.ActorId,
                            OccurredAtUtc = now, RecordedAtUtc = now, EvidenceReference = evidence,
                            Reason = "Penetapan siap bed hasil impor denah lingkungan " + environment.EnvironmentName
                        });
                        bedsMarkedReady++;
                    }
                }

                var summary = BuildResult(plans, dryRun: false, manifestHash, bedsMarkedReady);
                var run = new MstFloorPlanImportRun
                {
                    ManifestVersion = request.ManifestVersion!.Trim(),
                    ManifestHash = manifestHash,
                    EnvironmentName = Truncate(environment.EnvironmentName, 32),
                    MarkedBedsReady = markReady,
                    ReadyEvidenceReference = evidence,
                    SummaryJson = SerializeSummary(summary),
                    ActorUserId = access.ActorId,
                    StartedAtUtc = startedAt,
                    CompletedAtUtc = DateTime.UtcNow,
                    CreateDateTime = now,
                    CreateBy = access.ActorId
                };
                db.Set<MstFloorPlanImportRun>().Add(run);
                return ("FloorPlanImport", run.Id);
            }, ct);

        return await ReadRunAsync(meta, ct);
    }

    // ---------------------------------------------------------------- validasi format (400)

    /// <summary>
    /// Format dan batas manifest (VAL-DNH-01, 03, 04, 23). Kesalahan format menggagalkan seluruh
    /// permintaan dengan 400; penolakan bisnis per kamar dilaporkan di <c>Rows</c> dengan HTTP 200.
    /// </summary>
    private static void ValidateFormat(FloorPlanImportRequest? request)
    {
        if (request == null) throw new InpBedOperationException(400, "INVALID_INPUT", "Manifest impor belum dikirim.");
        if (!request.DryRun.HasValue) throw new InpBedOperationException(400, "INVALID_INPUT", "Pilih pemeriksaan (dry run) atau impor.");
        if (string.IsNullOrWhiteSpace(request.ManifestVersion) || request.ManifestVersion.Trim().Length > 50)
            throw new InpBedOperationException(400, "INVALID_INPUT", "Versi manifest wajib diisi, maksimum 50 karakter.");
        if (request.Rooms == null || request.Rooms.Count is < 1 or > MaxRooms)
            throw new InpBedOperationException(400, "INVALID_INPUT", $"Manifest harus berisi 1–{MaxRooms} kamar.");

        var errors = new List<(string Code, string Message)>();
        for (var i = 0; i < request.Rooms.Count; i++)
        {
            var row = request.Rooms[i];
            var label = string.IsNullOrWhiteSpace(row?.FloorPlanRoomKey) ? $"baris {i + 1}" : row!.FloorPlanRoomKey!.Trim();
            if (row == null) { errors.Add(("INVALID_INPUT", $"Baris {i + 1} kosong.")); continue; }
            if (string.IsNullOrWhiteSpace(row.FloorPlanRoomKey)) errors.Add(("INVALID_INPUT", $"{label}: kunci denah wajib diisi."));
            if (string.IsNullOrWhiteSpace(row.FloorCode)) errors.Add(("FLOOR_CODE_REQUIRED", $"{label}: kode lantai wajib diisi."));
            else if (FloorPlanRoomNaming.ValidateRoomKey(row.FloorPlanRoomKey, row.FloorCode) is { } keyError)
                errors.Add((keyError.Code, $"{label}: {keyError.Message}"));
            if (string.IsNullOrWhiteSpace(row.RoomNumber) || row.RoomNumber.Trim().Length > 50)
                errors.Add(("INVALID_INPUT", $"{label}: nomor kamar wajib diisi, maksimum 50 karakter."));
            else if (!FloorPlanRoomNaming.IsNumericRoomNumber(row.RoomNumber) && string.IsNullOrWhiteSpace(row.RoomName))
                errors.Add(("INVALID_INPUT", $"{label}: kamar tanpa nomor angka wajib memiliki nama ruang."));
            if (row.RoomName?.Trim().Length > 150) errors.Add(("INVALID_INPUT", $"{label}: nama ruang maksimum 150 karakter."));
            if (row.PatientClassId == Guid.Empty) errors.Add(("INVALID_INPUT", $"{label}: kelas kamar wajib dipilih."));
            if (!row.WingDirection.HasValue || !Enum.IsDefined(row.WingDirection.Value))
                errors.Add(("WING_DIRECTION_INVALID", $"{label}: arah wing harus salah satu dari delapan arah (nilai 1–8)."));
            var beds = row.Beds ?? [];
            if (beds.Count > MaxBedsPerRoom) errors.Add(("INVALID_INPUT", $"{label}: maksimum {MaxBedsPerRoom} bed per kamar."));
            var letters = beds.Select(x => FloorPlanRoomNaming.NormalizeBedLetter(x?.BedNumber)).ToList();
            if (letters.Any(x => x == null)) errors.Add(("INVALID_INPUT", $"{label}: huruf bed wajib berupa huruf, mis. A, B, C."));
            else if (letters.Distinct().Count() != letters.Count) errors.Add(("INVALID_INPUT", $"{label}: huruf bed harus unik per kamar."));
        }

        if (errors.Count > 0)
        {
            var shown = string.Join(" ", errors.Take(10).Select(x => x.Message));
            var more = errors.Count > 10 ? $" (dan {errors.Count - 10} kesalahan lain)" : string.Empty;
            throw new InpBedOperationException(400, errors[0].Code, "Manifest impor tidak valid. " + shown + more);
        }
    }

    // ---------------------------------------------------------------- rencana per kamar

    private async Task<List<RowPlan>> PlanAsync(FloorPlanImportRequest request, CancellationToken ct)
    {
        var rows = request.Rooms;
        var keys = rows.Select(x => x.FloorPlanRoomKey!.Trim()).Distinct().ToList();
        var unitIds = rows.Where(x => x.ServiceUnitId.HasValue).Select(x => x.ServiceUnitId!.Value).Distinct().ToList();
        var classIds = rows.Select(x => x.PatientClassId).Distinct().ToList();
        var existingIds = rows.Where(x => x.ExistingRoomId.HasValue).Select(x => x.ExistingRoomId!.Value).Distinct().ToList();
        var floors = rows.Select(x => x.FloorCode!.Trim().ToLower()).Distinct().ToList();

        var units = await db.Set<MstServiceUnit>().AsNoTracking().Where(x => unitIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);
        var classes = await db.Set<MstPatientClass>().AsNoTracking().Where(x => classIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);
        var roomsByKey = await db.Set<MstRoom>().AsNoTracking()
            .Where(x => !x.IsDelete && x.FloorPlanRoomKey != null && keys.Contains(x.FloorPlanRoomKey))
            .ToDictionaryAsync(x => x.FloorPlanRoomKey!, ct);
        var roomsById = await db.Set<MstRoom>().AsNoTracking()
            .Where(x => existingIds.Contains(x.Id) || (x.FloorPlanRoomKey != null && keys.Contains(x.FloorPlanRoomKey) && !x.IsDelete))
            .ToDictionaryAsync(x => x.Id, ct);
        var touchedRoomIds = roomsById.Keys.ToList();
        var activeBedCounts = await db.Set<MstBed>().AsNoTracking()
            .Where(x => !x.IsDelete && x.IsActive && touchedRoomIds.Contains(x.RoomId))
            .GroupBy(x => x.RoomId).Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, ct);
        var roomClassIds = roomsById.Values.Where(x => x.PatientClassId.HasValue).Select(x => x.PatientClassId!.Value).Distinct().ToList();
        var roomClassNames = await db.Set<MstPatientClass>().AsNoTracking()
            .Where(x => roomClassIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.PatientClassName, ct);
        var floorLabels = await db.Set<MstBed>().AsNoTracking()
            .Where(x => !x.IsDelete && x.Room != null && !x.Room.IsDelete && x.Room.FloorCode != null && floors.Contains(x.Room.FloorCode.ToLower()))
            .Select(x => new { Floor = x.Room!.FloorCode!.ToLower(), x.BedName })
            .ToListAsync(ct);
        var labelsByFloor = floorLabels.GroupBy(x => x.Floor)
            .ToDictionary(g => g.Key, g => g.Select(x => x.BedName.Trim().ToLowerInvariant()).ToHashSet());
        var scope = (await access.UnitsAsync(Operation, ct)).ToHashSet();

        var plans = new List<RowPlan>();
        var seenKeys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var row in rows)
        {
            var plan = new RowPlan
            {
                Source = row,
                Key = row.FloorPlanRoomKey!.Trim(),
                FloorCode = row.FloorCode!.Trim(),
                RoomNumber = row.RoomNumber!.Trim()
            };
            plans.Add(plan);

            if (!seenKeys.Add(plan.Key)) { Reject(plan, "FLOOR_PLAN_KEY_DUPLICATE"); continue; }

            // VAL-DNH-05: unit wajib, aktif, bertipe rawat inap, dalam scope.
            if (!row.ServiceUnitId.HasValue || row.ServiceUnitId.Value == Guid.Empty) { Reject(plan, "ROOM_UNIT_MAPPING_MISSING"); continue; }
            if (!units.TryGetValue(row.ServiceUnitId.Value, out var unit) || unit.IsDelete || !unit.IsActive || unit.ServiceUnitType != ServiceUnitType.Inpatient)
            { Reject(plan, "ROOM_UNIT_INVALID"); continue; }
            if (!scope.Contains(unit.Id)) { Reject(plan, "ACCESS_DENIED"); continue; }

            // VAL-DNH-06: kelas wajib, ada, dan aktif.
            if (!classes.TryGetValue(row.PatientClassId, out var patientClass) || patientClass.IsDelete || !patientClass.IsActive)
            { Reject(plan, "PATIENT_CLASS_NOT_FOUND"); continue; }
            plan.PatientClass = patientClass;

            roomsByKey.TryGetValue(plan.Key, out var keyedRoom);
            var requestedExisting = row.ExistingRoomId is { } existingId && existingId != Guid.Empty ? existingId : (Guid?)null;

            if (keyedRoom != null)
            {
                if (requestedExisting.HasValue && requestedExisting.Value != keyedRoom.Id) { Reject(plan, "FLOOR_PLAN_KEY_DUPLICATE"); continue; }
                if (!scope.Contains(keyedRoom.ServiceUnitId)) { Reject(plan, "ACCESS_DENIED"); continue; }
                plan.ExistingRoomId = keyedRoom.Id;
                AddDriftCode(plan, keyedRoom, activeBedCounts);
                var currentClassName = keyedRoom.PatientClassId.HasValue && roomClassNames.TryGetValue(keyedRoom.PatientClassId.Value, out var className) ? className : null;
                var changed = keyedRoom.WingDirection == null ||
                    keyedRoom.FloorName != FloorPlanRoomNaming.FloorName(keyedRoom.FloorCode!) ||
                    keyedRoom.LocationName != FloorPlanRoomNaming.LocationName(keyedRoom.FloorCode!, keyedRoom.WingDirection ?? row.WingDirection) ||
                    keyedRoom.RoomName != FloorPlanRoomNaming.RoomName(currentClassName, keyedRoom.RoomNumber, keyedRoom.RoomName);
                plan.Action = changed ? RowAction.RefreshExisting : RowAction.None;
                plan.Outcome = changed ? "Updated" : "Unchanged";
                continue;
            }

            if (requestedExisting.HasValue)
            {
                if (!roomsById.TryGetValue(requestedExisting.Value, out var existing) || existing.IsDelete) { Reject(plan, "NOT_FOUND"); continue; }
                if (existing.FloorPlanRoomKey != null ||
                    (existing.FloorCode != null && !string.Equals(existing.FloorCode, plan.FloorCode, StringComparison.OrdinalIgnoreCase)))
                { Reject(plan, "FLOOR_PLAN_KEY_IMMUTABLE"); continue; }
                if (!scope.Contains(existing.ServiceUnitId)) { Reject(plan, "ACCESS_DENIED"); continue; }
                plan.ExistingRoomId = existing.Id;
                AddDriftCode(plan, existing, activeBedCounts);
                plan.Action = RowAction.AttachExisting;
                plan.Outcome = "Updated";
                continue;
            }

            // Kamar baru: label bed unik per lantai, termasuk terhadap baris lain pada manifest ini.
            var floorKey = plan.FloorCode.ToLowerInvariant();
            if (!labelsByFloor.TryGetValue(floorKey, out var labels)) labelsByFloor[floorKey] = labels = [];
            var newLabels = (row.Beds ?? []).Select(x => FloorPlanRoomNaming.BedLabel(plan.RoomNumber, FloorPlanRoomNaming.NormalizeBedLetter(x.BedNumber)!).ToLowerInvariant()).ToList();
            if (newLabels.Any(labels.Contains)) { Reject(plan, "BED_LABEL_DUPLICATE"); continue; }
            foreach (var label in newLabels) labels.Add(label);
            plan.Action = RowAction.Create;
            plan.Outcome = "Created";
        }
        return plans;
    }

    private static void Reject(RowPlan plan, string code)
    {
        plan.Action = RowAction.None;
        plan.Outcome = "Rejected";
        plan.Codes.Add(code);
    }

    /// <summary>VAL-DNH-19: kamar existing yang kelas, unit, atau jumlah bed aktifnya berbeda dari denah.</summary>
    private static void AddDriftCode(RowPlan plan, MstRoom room, IReadOnlyDictionary<Guid, int> activeBedCounts)
    {
        var bedCount = activeBedCounts.TryGetValue(room.Id, out var count) ? count : 0;
        if (room.PatientClassId != plan.Source.PatientClassId || room.ServiceUnitId != plan.Source.ServiceUnitId ||
            bedCount != (plan.Source.Beds?.Count ?? 0))
            plan.Codes.Add("FLOOR_PLAN_DRIFT");
    }

    // ---------------------------------------------------------------- tulis (di dalam koordinator)

    private async Task<List<MstBed>> CreateRoomWithBedsAsync(RowPlan plan, DateTime now, CancellationToken ct)
    {
        var row = plan.Source;
        var patientClass = plan.PatientClass!;
        var room = new MstRoom
        {
            Id = Guid.NewGuid(),
            ServiceUnitId = row.ServiceUnitId!.Value,
            PatientClassId = patientClass.Id,
            RoomCode = await bedMutation.AllocateRoomCodeAsync(ct),
            RoomNumber = plan.RoomNumber,
            RoomName = FloorPlanRoomNaming.RoomName(patientClass.PatientClassName, plan.RoomNumber, row.RoomName),
            FloorCode = plan.FloorCode,
            WingDirection = row.WingDirection,
            FloorPlanRoomKey = plan.Key,
            FloorName = FloorPlanRoomNaming.FloorName(plan.FloorCode),
            LocationName = FloorPlanRoomNaming.LocationName(plan.FloorCode, row.WingDirection),
            Capacity = Math.Max(1, row.Beds?.Count ?? 0),
            IsAvailableForAdmission = true,
            IsActive = true,
            CreateDateTime = now,
            CreateBy = access.ActorId,
            IsDelete = false,
            IsCancel = false
        };
        FloorPlanRoomNaming.ApplyRoomFlags(room, patientClass.PatientClassType);
        db.Set<MstRoom>().Add(room);

        var beds = new List<MstBed>();
        foreach (var bed in (row.Beds ?? []).OrderBy(x => FloorPlanRoomNaming.LetterToNumber(FloorPlanRoomNaming.NormalizeBedLetter(x.BedNumber)!)))
            beds.Add(await bedMutation.AddFloorPlanBedAsync(room, patientClass.PatientClassType,
                FloorPlanRoomNaming.NormalizeBedLetter(bed.BedNumber)!, bed.IsForNewborn, ct));
        return beds;
    }

    /// <summary>
    /// Melekatkan kunci ke kamar existing atau menyegarkan label kamar berkunci. Kelas, unit, dan
    /// bed tidak pernah diubah (INV-DNH-10); arah wing hanya diisi bila masih kosong, sehingga
    /// koreksi admin tidak tertimpa impor ulang.
    /// </summary>
    private async Task LabelExistingRoomAsync(RowPlan plan, DateTime now, CancellationToken ct)
    {
        var room = await db.Set<MstRoom>().Include(x => x.PatientClass)
            .SingleAsync(x => x.Id == plan.ExistingRoomId!.Value, ct);
        if (plan.Action == RowAction.AttachExisting)
        {
            room.FloorPlanRoomKey = plan.Key;
            room.FloorCode ??= plan.FloorCode;
            room.RoomNumber ??= plan.RoomNumber;
        }
        room.WingDirection ??= plan.Source.WingDirection;
        room.FloorName = FloorPlanRoomNaming.FloorName(room.FloorCode!);
        room.LocationName = FloorPlanRoomNaming.LocationName(room.FloorCode!, room.WingDirection);
        room.RoomName = FloorPlanRoomNaming.RoomName(room.PatientClass?.PatientClassName, room.RoomNumber, room.RoomName);
        room.UpdateDateTime = now;
        room.UpdateBy = access.ActorId;
    }

    // ---------------------------------------------------------------- hasil

    private static FloorPlanImportResult BuildResult(List<RowPlan> plans, bool dryRun, string manifestHash, int bedsMarkedReady)
    {
        var result = new FloorPlanImportResult { DryRun = dryRun, ManifestHash = manifestHash };
        foreach (var plan in plans)
        {
            var bedCount = plan.Source.Beds?.Count ?? 0;
            switch (plan.Outcome)
            {
                case "Created": result.Summary.Rooms.Created++; result.Summary.Beds.Created += bedCount; break;
                case "Updated": result.Summary.Rooms.Updated++; result.Summary.Beds.Unchanged += bedCount; break;
                case "Rejected": result.Summary.Rooms.Rejected++; result.Summary.Beds.Rejected += bedCount; break;
                default: result.Summary.Rooms.Unchanged++; result.Summary.Beds.Unchanged += bedCount; break;
            }
            result.Rows.Add(new FloorPlanImportRowResponse
            {
                FloorPlanRoomKey = plan.Key,
                Outcome = plan.Outcome,
                Codes = plan.Codes.ToList(),
                Messages = plan.Codes.Select(MessageFor).ToList()
            });
        }
        result.Summary.BedsMarkedReady = bedsMarkedReady;
        return result;
    }

    private static string MessageFor(string code) => CodeMessages.TryGetValue(code, out var message) ? message : code;

    /// <summary>Ringkasan run sesuai skema <c>SummaryJson</c> kamus data 22.3 (schemaVersion 1).</summary>
    private static string SerializeSummary(FloorPlanImportResult result) => JsonSerializer.Serialize(new
    {
        schemaVersion = 1,
        rooms = new { created = result.Summary.Rooms.Created, updated = result.Summary.Rooms.Updated, unchanged = result.Summary.Rooms.Unchanged, rejected = result.Summary.Rooms.Rejected },
        beds = new { created = result.Summary.Beds.Created, updated = result.Summary.Beds.Updated, unchanged = result.Summary.Beds.Unchanged, rejected = result.Summary.Beds.Rejected },
        bedsMarkedReady = result.Summary.BedsMarkedReady,
        rows = result.Rows.Select(x => new { floorPlanRoomKey = x.FloorPlanRoomKey, outcome = x.Outcome, codes = x.Codes })
    });

    /// <summary>Membaca hasil impor dari catatan run, termasuk saat permintaan diulang dengan kunci idempotency yang sama.</summary>
    private async Task<FloorPlanImportResult> ReadRunAsync(BedOperationMeta meta, CancellationToken ct)
    {
        var run = await db.Set<MstFloorPlanImportRun>().AsNoTracking().SingleOrDefaultAsync(x => x.Id == meta.ResultEntityId, ct)
            ?? throw new InpBedOperationException(404, "NOT_FOUND", "Catatan run impor tidak ditemukan.");
        using var doc = JsonDocument.Parse(run.SummaryJson);
        var root = doc.RootElement;
        FloorPlanImportCountResponse Count(string name) => new()
        {
            Created = root.GetProperty(name).GetProperty("created").GetInt32(),
            Updated = root.GetProperty(name).GetProperty("updated").GetInt32(),
            Unchanged = root.GetProperty(name).GetProperty("unchanged").GetInt32(),
            Rejected = root.GetProperty(name).GetProperty("rejected").GetInt32()
        };
        return new FloorPlanImportResult
        {
            DryRun = false,
            RunId = run.Id,
            ManifestHash = run.ManifestHash,
            OperationMeta = meta,
            Summary = new FloorPlanImportSummaryResponse
            {
                Rooms = Count("rooms"),
                Beds = Count("beds"),
                BedsMarkedReady = root.GetProperty("bedsMarkedReady").GetInt32()
            },
            Rows = root.GetProperty("rows").EnumerateArray().Select(x =>
            {
                var codes = x.GetProperty("codes").EnumerateArray().Select(c => c.GetString() ?? string.Empty).ToList();
                return new FloorPlanImportRowResponse
                {
                    FloorPlanRoomKey = x.GetProperty("floorPlanRoomKey").GetString() ?? string.Empty,
                    Outcome = x.GetProperty("outcome").GetString() ?? string.Empty,
                    Codes = codes,
                    Messages = codes.Select(MessageFor).ToList()
                };
            }).ToList()
        };
    }

    /// <summary>
    /// SHA-256 manifest kanonis: kunci terurut, isian dirapikan, tanpa penanda dry run — sehingga
    /// pemeriksaan dan impor manifest yang sama menghasilkan hash yang sama.
    /// </summary>
    private static string ComputeManifestHash(FloorPlanImportRequest request)
    {
        var canonical = new
        {
            manifestVersion = request.ManifestVersion!.Trim(),
            rooms = request.Rooms
                .OrderBy(x => x.FloorPlanRoomKey!.Trim(), StringComparer.Ordinal)
                .Select(x => new
                {
                    floorPlanRoomKey = x.FloorPlanRoomKey!.Trim(),
                    floorCode = x.FloorCode!.Trim(),
                    roomNumber = x.RoomNumber!.Trim(),
                    roomName = string.IsNullOrWhiteSpace(x.RoomName) ? null : x.RoomName.Trim(),
                    patientClassId = x.PatientClassId,
                    serviceUnitId = x.ServiceUnitId,
                    wingDirection = (int?)x.WingDirection,
                    existingRoomId = x.ExistingRoomId,
                    beds = (x.Beds ?? []).Select(b => new { bedNumber = FloorPlanRoomNaming.NormalizeBedLetter(b.BedNumber), isForNewborn = b.IsForNewborn })
                        .OrderBy(b => b.bedNumber, StringComparer.Ordinal)
                })
        };
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(canonical)))).ToLowerInvariant();
    }

    private static string Truncate(string value, int max) => value.Length <= max ? value : value[..max];
}
