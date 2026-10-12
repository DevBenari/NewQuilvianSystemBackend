using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;

/// <summary>Semua writer memakai lock hierarchy dan bed yang sama, serta satu commit receipt/audit.</summary>
public sealed class InpBedMutationCoordinator(ApplicationDbContext db, InpBedAccessService access, IHttpContextAccessor http, Microsoft.Extensions.Options.IOptionsMonitor<QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Options.BedManagementOptions> options)
{
    public BedOperationMeta? LastOperation { get; private set; } public string? LastOperationName { get; private set; }
    public string RequireKey()
    {
        var key = _httpKey();
        if (string.IsNullOrWhiteSpace(key) || key.Length > 100)
            throw new InpBedOperationException(400, "INVALID_INPUT", "Idempotency-Key wajib, maksimum 100 karakter.");
        return key;
    }
    private string? _httpKey() => http.HttpContext?.Request.Headers["Idempotency-Key"].ToString();
    

    public async Task<BedOperationMeta> ExecuteAsync(
        string operation, object request, IEnumerable<Guid> episodeIds, IEnumerable<Guid> bedIds,
        IEnumerable<BedVersionExpectation> versions,
        Func<CancellationToken, Task<(string Kind, Guid Id)>> mutate, CancellationToken ct = default,
        IEnumerable<Guid>? extraUnitIds = null, IEnumerable<Guid>? extraRoomIds = null,
        IEnumerable<Guid>? extraClassIds = null)
    {
        var key = RequireKey();
        var actor = access.ActorId;
        if (actor == Guid.Empty || !await db.Users.AnyAsync(x => x.Id == actor && x.IsActive, ct) || !await access.HasPermissionAsync(operation))
            throw new InpBedOperationException(403, "ACCESS_DENIED", "Hak operasi tidak tersedia.");
        var ids = bedIds.Distinct().OrderBy(x => x).ToArray();
        foreach (var id in ids) await access.RequireBedAsync(id, operation, ct);
        var hash = Hash(operation, request);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        try
        {
            foreach (var id in episodeIds.Distinct().OrderBy(x => x)) await LockAsync("InpEpisode", id, ct);
            await LockHierarchyAsync(ids, extraUnitIds, extraRoomIds, extraClassIds, ct);
            // Mengikat key bahkan ketika dua request mempunyai set bed berbeda.
            await db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock(hashtextextended({actor.ToString() + ":" + key}, 0))", ct);
            var receipt = await db.Set<InpBedOperationReceipt>().AsNoTracking()
                .FirstOrDefaultAsync(x => x.ActorUserId == actor && x.IdempotencyKey == key, ct);
            if (receipt != null)
            {
                if (receipt.OperationName != operation || receipt.RequestHash != hash)
                    throw new InpBedOperationException(409, "IDEMPOTENCY_KEY_REUSED", "Kunci ini sudah digunakan untuk isian berbeda.");
                var receiptBeds = await db.Set<InpBedLifecycleEvent>().AsNoTracking().Where(x => x.OperationReceiptId == receipt.Id)
                    .Join(db.Set<InpBedReadiness>(), e => e.ReadinessId, r => r.Id, (e,r) => r.BedId).Distinct().ToListAsync(ct);
                foreach (var receiptBed in receiptBeds) await access.RequireBedAsync(receiptBed, receipt.OperationName, ct); if (receiptBeds.Count == 0 && receipt.ResultKind is "Room" or "ServiceUnit" or "PatientClass") await access.RequireMasterResultAsync(receipt.ResultKind, receipt.ResultEntityId, receipt.OperationName, ct);
                var meta = Meta(receipt, true);
                await tx.CommitAsync(ct);
                LastOperation = meta; LastOperationName = operation;
                return meta;
            }
            var expected = versions.ToList();
            if (expected.Count != ids.Length || expected.Select(x => x.BedId).Distinct().Count() != expected.Count ||
                !expected.Select(x => x.BedId).OrderBy(x => x).SequenceEqual(ids) || expected.Any(x => x.ExpectedVersion < 1))
                throw new InpBedOperationException(400, "INVALID_INPUT", "Kirim versi seluruh bed terdampak, tanpa duplikasi atau tambahan.");
            var roots = new List<(InpBedReadiness Root, InpBedReadinessState Before)>();
            foreach (var id in ids)
            {
                var r = await RootAsync(id, actor, ct);
                await ExpireLockedAsync(id, r, actor, ct);
                if (r.Version != expected.Single(x => x.BedId == id).ExpectedVersion)
                    throw new InpBedOperationException(409, "STALE_BED_VERSION", "Versi bed berubah. Muat ulang sebelum menyimpan.");
                roots.Add((r, r.State));
            }
            var result = await mutate(ct);
            var committedAt = DateTime.UtcNow;
            receipt = new InpBedOperationReceipt
            {
                ActorUserId = actor, IdempotencyKey = key, OperationName = operation, RequestHash = hash,
                ResultKind = result.Kind, ResultEntityId = result.Id, CommittedAtUtc = committedAt,
                CreateBy = actor
            };
            foreach (var (r, before) in roots)
            {
                r.Version++; r.UpdateBy = actor; r.UpdateDateTime = committedAt;
                db.Set<InpBedLifecycleEvent>().Add(new()
                {
                    ReadinessId = r.Id, CycleId = r.CycleId, OperationReceiptId = receipt.Id,
                    Action = operation, BeforeState = before, AfterState = r.State, ActorUserId = actor,
                    OccurredAtUtc = committedAt, RecordedAtUtc = committedAt, CreateBy = actor,
                    PlacementId = result.Kind == "Placement" ? result.Id : null,
                    ReservationId = result.Kind == "Reservation" ? result.Id : null,
                    CleaningAttemptId = result.Kind == "CleaningAttempt" ? result.Id : null,
                    Reason = OperationReason(request),
                    EvidenceReference = OperationText(request, "EvidenceReference")
                });
            }
            receipt.ResultVersion = roots.Count == 0 ? db.Set<InpBedReadiness>().Local.SingleOrDefault(x => result.Kind == "Bed" && x.BedId == result.Id)?.Version : roots.Max(x => x.Root.Version);
            foreach (var audit in db.Set<InpBedLifecycleEvent>().Local.Where(x => x.OperationReceiptId == null && x.ActorUserId == actor && x.Action != "ReservationExpiryEvaluated" && db.Entry(x).State == EntityState.Added)) audit.OperationReceiptId = receipt.Id;
            db.Set<InpBedOperationReceipt>().Add(receipt);
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            LastOperation = Meta(receipt, false); LastOperationName = operation;
            return LastOperation;
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync(ct); db.ChangeTracker.Clear();
            throw new InpBedOperationException(409, "BED_HOLD_CONFLICT", "Data bed berubah bersamaan. Muat ulang data: " + ex.GetType().Name);
        }
        catch
        {
            await tx.RollbackAsync(ct); db.ChangeTracker.Clear(); throw;
        }
    }
    public async Task LockHierarchyAsync(Guid[] ids, IEnumerable<Guid>? extraUnits, IEnumerable<Guid>? extraRooms, IEnumerable<Guid>? extraClasses, CancellationToken ct)
    {
        var initial = await db.Set<MstBed>().AsNoTracking().Where(x => ids.Contains(x.Id))
            .Select(x => new { x.Id, x.RoomId, UnitId = x.Room!.ServiceUnitId, ClassId = x.Room.PatientClassId }).ToListAsync(ct);
        var roomIds = initial.Select(x => x.RoomId).Concat(extraRooms ?? []).Distinct().ToArray();
        var parents = await db.Set<MstRoom>().AsNoTracking().Where(x => roomIds.Contains(x.Id))
            .Select(x => new { x.Id, x.ServiceUnitId, x.PatientClassId }).ToListAsync(ct);
        foreach (var id in parents.Select(x => x.ServiceUnitId).Concat(extraUnits ?? []).Distinct().OrderBy(x => x))
            await LockAsync("MstServiceUnit", id, ct);
        foreach (var id in roomIds.OrderBy(x => x)) await LockAsync("MstRoom", id, ct);
        foreach (var id in parents.Select(x => x.PatientClassId).Where(x => x.HasValue).Select(x => x!.Value).Concat(extraClasses ?? []).Distinct().OrderBy(x => x))
            await LockAsync("MstPatientClass", id, ct);
        foreach (var id in ids.OrderBy(x => x)) await LockAsync("MstBed", id, ct);
        var current = await db.Set<MstBed>().AsNoTracking().Where(x => ids.Contains(x.Id))
            .Select(x => new { x.Id, x.RoomId, UnitId = x.Room!.ServiceUnitId, ClassId = x.Room.PatientClassId }).ToListAsync(ct);
        if (initial.Count != ids.Length || !initial.OrderBy(x => x.Id).SequenceEqual(current.OrderBy(x => x.Id)))
            throw new InpBedOperationException(409, "STALE_BED_VERSION", "Hierarchy bed berubah. Muat ulang data.");
        var currentParents = await db.Set<MstRoom>().AsNoTracking().Where(x => roomIds.Contains(x.Id))
            .Select(x => new { x.Id, x.ServiceUnitId, x.PatientClassId }).ToListAsync(ct);
        if (!parents.OrderBy(x => x.Id).SequenceEqual(currentParents.OrderBy(x => x.Id)))
            throw new InpBedOperationException(409, "STALE_BED_VERSION", "Induk kamar berubah.");
        // Core legacy dapat memuat entity sebelum lock. Jangan memakai salinan tracking itu.
        foreach (var entry in db.ChangeTracker.Entries().Where(x =>
            x.State == EntityState.Unchanged && (x.Entity is MstBed || x.Entity is MstRoom || x.Entity is MstPatientClass || x.Entity is InpEpisode || x.Entity is InpBedPlacement || x.Entity is InpBedReservation)).ToList())
            await entry.ReloadAsync(ct);
    }
    private async Task LockAsync(string table, Guid id, CancellationToken ct)
    {
        // table hanya konstanta internal, id selalu parameter.
        await db.Database.ExecuteSqlRawAsync($"SELECT 1 FROM public.\"{table}\" WHERE \"Id\" = {{0}} FOR UPDATE", new object[] { id }, ct);
    }
    public async Task<InpBedReadiness> RootAsync(Guid bedId, Guid actor, CancellationToken ct)
    {
        var root = db.Set<InpBedReadiness>().Local.SingleOrDefault(x => x.BedId == bedId) ?? await db.Set<InpBedReadiness>().SingleOrDefaultAsync(x => x.BedId == bedId, ct);
        if (root != null) return root;
        root = new() { BedId = bedId, CreateBy = actor };
        db.Set<InpBedReadiness>().Add(root);
        return root;
    }
    public async Task<int> ExpireScopedAsync(string operation, CancellationToken ct)
    {
        var units = await access.UnitsAsync(operation, ct);
        var due = await db.Set<InpBedReservation>().AsNoTracking().Where(x => !x.IsDelete && x.ReservationStatus == InpBedReservationStatus.Active &&
            x.ExpiresAt <= DateTime.UtcNow && x.Bed != null && x.Bed.Room != null && units.Contains(x.Bed.Room.ServiceUnitId))
            .Select(x => new { x.BedId, x.EpisodeId }).ToListAsync(ct);
        if (due.Count == 0) return 0;
        // Semua episode lebih dulu; hierarchy dan bed terurut memakai primitive yang sama.
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        foreach (var id in due.Select(x => x.EpisodeId).Distinct().OrderBy(x => x)) await LockAsync("InpEpisode", id, ct);
        var ids = due.Select(x => x.BedId).Distinct().OrderBy(x => x).ToArray();
        await LockHierarchyAsync(ids, null, null, null, ct);
        foreach (var id in ids) { var root = await RootAsync(id, options.CurrentValue.ReservationExpiryActorUserId ?? access.ActorId, ct); await ExpireLockedAsync(id, root, access.ActorId, ct); }
        await db.SaveChangesAsync(ct); await transaction.CommitAsync(ct);
        return due.Count;
    }
    public async Task RequireEmptyAsync(Guid bedId, CancellationToken ct)
    {
        if (await db.Set<InpBedPlacement>().AnyAsync(x => x.BedId == bedId && x.EndDateTime == null && !x.IsDelete, ct) ||
            await db.Set<InpBedReservation>().AnyAsync(x => x.BedId == bedId && x.ReservationStatus == InpBedReservationStatus.Active && !x.IsDelete && x.ExpiresAt > DateTime.UtcNow, ct))
            throw new InpBedOperationException(409, "BED_HOLD_CONFLICT", "Bed masih ditempati atau dipesan.");
    }
    public async Task ExpireLockedAsync(Guid bedId, InpBedReadiness root, Guid actor, CancellationToken ct)
    {
        var due = await db.Set<InpBedReservation>().Where(x => x.BedId == bedId && !x.IsDelete && x.ReservationStatus == InpBedReservationStatus.Active && x.ExpiresAt <= DateTime.UtcNow).ToListAsync(ct);
        if (due.Count == 0) return;
        var systemActor = options.CurrentValue.ReservationExpiryActorUserId;
        if (!systemActor.HasValue || string.IsNullOrWhiteSpace(options.CurrentValue.ReservationExpiryAssignmentReference) || !await db.Users.AnyAsync(x => x.Id == systemActor.Value && x.IsActive, ct))
            throw new InpBedOperationException(503, "DEPENDENCY_UNAVAILABLE", "Akun sistem expiry dan bukti penugasannya belum dapat diverifikasi.");
        actor = systemActor.Value;
        foreach (var r in due)
        {
            r.ReservationStatus = InpBedReservationStatus.Expired; r.IsActive = false;
            r.ReleasedAt = DateTime.UtcNow; r.UpdateBy = actor; r.UpdateDateTime = r.ReleasedAt;
            root.Version++; root.UpdateBy = actor; root.UpdateDateTime = DateTime.UtcNow;
            db.Set<InpBedLifecycleEvent>().Add(new()
            {
                ReadinessId = root.Id, CycleId = root.CycleId, ReservationId = r.Id,
                Action = "ReservationExpiryEvaluated", BeforeState = root.State, AfterState = root.State,
                ActorUserId = actor, CreateBy = actor, OccurredAtUtc = r.ExpiresAt, RecordedAtUtc = DateTime.UtcNow
            });
        }
        if (due.Count > 0)
        {
            var bed = await db.Set<MstBed>().SingleAsync(x => x.Id == bedId, ct);
            if (bed.BedStatus == BedStatus.Reserved && !await db.Set<InpBedPlacement>().AnyAsync(x => x.BedId == bedId && x.EndDateTime == null && !x.IsDelete, ct))
                bed.BedStatus = root.State == InpBedReadinessState.Ready ? BedStatus.Available : BedStatus.Cleaning;
            await db.SaveChangesAsync(ct);
        }
    }
    public async Task InvalidateAsync(Guid bedId, Guid actor, Guid? releasedPlacement, CancellationToken ct)
    {
        var r = await RootAsync(bedId, actor, ct);
        var attempts = await db.Set<InpBedCleaningAttempt>().Where(x => x.ReadinessId == r.Id && (x.State == InpBedCleaningAttemptState.Started || x.State == InpBedCleaningAttemptState.AwaitingVerification)).ToListAsync(ct);
        foreach (var a in attempts) { a.State = InpBedCleaningAttemptState.Interrupted; a.UpdateBy = actor; a.UpdateDateTime = DateTime.UtcNow; }
        var dirty = releasedPlacement.HasValue || r.State is InpBedReadinessState.WaitingCleaning or InpBedReadinessState.Cleaning or InpBedReadinessState.AwaitingVerification; r.CycleId = Guid.NewGuid(); r.State = dirty ? InpBedReadinessState.WaitingCleaning : InpBedReadinessState.Unverified;
        r.LastReleasedPlacementId = releasedPlacement ?? r.LastReleasedPlacementId;
        r.VerifiedAtUtc = null; r.VerifiedByUserId = null; r.VerificationReference = null;
    }
    public static BedOperationMeta Meta(InpBedOperationReceipt r, bool replay) => new()
    { ReceiptId = r.Id, ResultEntityId = r.ResultEntityId, ResultKind = r.ResultKind, CommittedVersion = r.ResultVersion, CommittedAtUtc = r.CommittedAtUtc, IsReplay = replay };
    private static string? OperationReason(object request)
    {
        foreach (var name in new[] { "OperationReason", "Reason", "TransferReason" })
        {
            var value = OperationText(request, name);
            if (value != null) return value;
        }
        return null;
    }
    private static string? OperationText(object request, string name)
    {
        var doc = JsonSerializer.SerializeToElement(request);
        while (doc.ValueKind == JsonValueKind.Object)
        {
            if (doc.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String)
                return value.GetString()?.Trim();
            if (!doc.TryGetProperty("request", out doc)) break;
        }
        return null;
    }
    private static string Hash(string operation, object request)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            void Write(JsonElement e)
            {
                if (e.ValueKind == JsonValueKind.Object)
                {
                    writer.WriteStartObject();
                    foreach (var p in e.EnumerateObject().OrderBy(x => x.Name, StringComparer.Ordinal)) { writer.WritePropertyName(p.Name); Write(p.Value); }
                    writer.WriteEndObject();
                }
                else if (e.ValueKind == JsonValueKind.Array)
                { writer.WriteStartArray(); foreach (var x in e.EnumerateArray()) Write(x); writer.WriteEndArray(); }
                else e.WriteTo(writer);
            }
            Write(JsonSerializer.SerializeToElement(new { operation, request }));
        }
        return Convert.ToHexString(SHA256.HashData(stream.ToArray())).ToLowerInvariant();
    }
}

