using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using static QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services.OperatingRoomCommandSupport;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services;

/// <summary>
/// Outbox penyerahan data operasi ke Inventory/Farmasi dan Billing (BE-OPR-009,
/// `OPR-INT-001`/`OPR-INT-002`).
///
/// Adapter nyata ke kedua consumer <b>belum dibangun</b> karena owner API-nya belum
/// ditetapkan. Yang berjalan di sini adalah pencatatan lokal: baris delivery ditulis dalam
/// transaksi yang sama dengan perubahan bisnis, berkunci idempotency, dan dapat direkonsiliasi
/// atau diretry. Hasil pengiriman dicatat lewat <see cref="RecordAttemptAsync"/> sehingga
/// ketika adapter tersedia ia hanya perlu memanggil metode itu.
/// </summary>
public sealed class OperatingRoomIntegrationService
{
    public const string InventoryDestination = "Inventory";
    public const string BillingDestination = "Billing";
    public const string MaterialMessageType = "OPR-INT-001";
    public const string ChargeMessageType = "OPR-INT-002";

    /// <summary>Nama kejadian bisnisnya. Berbeda dari `MessageType`, yang menunjuk kontrak.</summary>
    public const string MaterialEventType = "operating-room.material-usage.recorded";

    public const string ChargeEventType = "operating-room.charge.registered";

    /// <summary>Versi bentuk amplop. Naik hanya bila bentuknya berubah tidak kompatibel.</summary>
    public const string EventVersion = "1.0";

    private static readonly JsonSerializerOptions PayloadOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private const string AttemptAction = "IntegrationAttempt";
    private const string RetryAction = "IntegrationRetry";

    /// <summary>
    /// Tujuan yang belum memiliki consumer yang siap menerima, sehingga pesannya menumpuk di
    /// antrean dan rekonsiliasinya masih dilakukan orang.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Daftar ini menyatakan kesiapan <b>penerima</b>, bukan kesiapan modul Operasi. Pesannya
    /// sendiri sudah terbentuk lengkap dan tersimpan; yang belum ada adalah pihak yang
    /// mengambilnya. Begitu consumer tersedia, tidak ada yang perlu diubah di sini selain
    /// mengeluarkan namanya dari daftar.
    /// </para>
    /// <para>
    /// <see cref="InventoryDestination"/> sudah keluar dari daftar: pemakaian material dibukukan
    /// ke kartu stok Farmasi oleh <see cref="OperatingRoomInventoryDispatchService"/>. Billing
    /// masih di dalamnya karena tarif dan tagihan adalah milik Billing.
    /// </para>
    /// </remarks>
    private static readonly string[] BlockedDestinations = [BillingDestination];

    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LoggerService _loggerService;

    public OperatingRoomIntegrationService(ApplicationDbContext dbContext,
        IHttpContextAccessor httpContextAccessor, LoggerService loggerService)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _loggerService = loggerService;
    }

    /// <summary>
    /// Menyiapkan delivery pemakaian material tanpa menyimpan; pemanggil menyimpannya dalam
    /// transaksi yang sama dengan ledger agar tidak ada pemakaian tanpa outbox.
    /// Kunci idempotency mengikuti kontrak: `case:usage:revision`.
    /// </summary>
    public async Task StageMaterialDeliveryAsync(Guid caseId, OprMaterialUsage usage, Guid actorUserId,
        DateTime now, CancellationToken cancellationToken = default) =>
        await StageAsync(caseId, InventoryDestination, MaterialMessageType, MaterialEventType,
            $"{caseId:N}:usage:{usage.Id:N}:{usage.Revision}", $"OprMaterialUsage/{usage.Id:N}",
            peristiwa => peristiwa.Material = new OprIntegrationEventMaterial
            {
                UsageId = usage.Id, ExternalItemId = usage.ExternalItemId,
                ItemType = usage.ItemType.ToString(), Quantity = usage.Quantity,
                UnitMeasurementId = usage.UnitMeasurementId, UnitCode = usage.UnitCode,
                Outcome = usage.Outcome.ToString(), BatchNumber = usage.BatchNumber,
                SerialNumber = usage.SerialNumber, Revision = usage.Revision,
                RecordedBy = usage.RecordedBy
            },
            actorUserId, now, cancellationToken);

    /// <summary>
    /// Menyiapkan delivery komponen tagihan. Kunci idempotency mengikuti kontrak:
    /// `case:charge:component:revision`.
    /// </summary>
    public async Task StageChargeDeliveryAsync(Guid caseId, string component, int revision, Guid actorUserId,
        DateTime now, CancellationToken cancellationToken = default) =>
        await StageAsync(caseId, BillingDestination, ChargeMessageType, ChargeEventType,
            $"{caseId:N}:charge:{component}:{revision}", $"OprCase/{caseId:N}/charge/{component}",
            peristiwa => peristiwa.Charge = new OprIntegrationEventCharge
            {
                Component = component, Revision = revision
            },
            actorUserId, now, cancellationToken);

    /// <remarks>
    /// <para>
    /// Baris outbox ditulis ke change tracker dan <b>tidak</b> disimpan di sini. Pemanggilnya
    /// menyimpan bersama perubahan bisnisnya, sehingga tidak pernah ada pemakaian material tanpa
    /// pesan, dan tidak pernah ada pesan tentang pemakaian yang gagal tersimpan.
    /// </para>
    /// <para>
    /// Pengiriman ke consumer sengaja tidak terjadi di sini. Itulah inti pola outbox: kegagalan
    /// jaringan atau kegagalan consumer tidak boleh sampai membatalkan tindakan klinis yang sudah
    /// benar-benar dilakukan pada pasien.
    /// </para>
    /// </remarks>
    private async Task StageAsync(Guid caseId, string destination, string messageType, string eventType,
        string idempotencyKey, string payloadReference, Action<OprIntegrationEvent> lengkapi,
        Guid actorUserId, DateTime now, CancellationToken cancellationToken)
    {
        // Duplicate key hanya boleh menghasilkan satu efek, termasuk ketika baris kembarannya
        // masih tertahan di change tracker pada perintah yang sama.
        var stagedLocally = _dbContext.OprIntegrationDeliveries.Local
            .Any(x => x.Destination == destination && x.IdempotencyKey == idempotencyKey && !x.IsDelete);
        if (stagedLocally) return;
        var exists = await _dbContext.OprIntegrationDeliveries.AsNoTracking()
            .AnyAsync(x => x.Destination == destination && x.IdempotencyKey == idempotencyKey && !x.IsDelete,
                cancellationToken);
        if (exists) return;

        var eventId = DeterministicEventId(destination, idempotencyKey);
        var peristiwa = await BuildEventAsync(caseId, eventId, eventType, now, cancellationToken);
        lengkapi(peristiwa);

        _dbContext.OprIntegrationDeliveries.Add(new OprIntegrationDelivery
        {
            OprCaseId = caseId, Destination = destination, MessageType = messageType,
            EventId = eventId, EventType = eventType, EventVersion = EventVersion, OccurredAt = now,
            PayloadJson = JsonSerializer.Serialize(peristiwa, PayloadOptions),
            IdempotencyKey = idempotencyKey, CorrelationId = caseId.ToString("N"),
            PayloadReference = payloadReference, Status = OprDeliveryStatus.Pending, RetryCount = 0,
            CreateDateTime = now, CreateBy = actorUserId
        });
    }

    /// <summary>
    /// Menyusun amplop kejadian dari keadaan kasus saat ini.
    /// </summary>
    /// <remarks>
    /// Tindakan dan pelaksana dibaca di sini, bukan dibiarkan sebagai rujukan, supaya pesan tetap
    /// menggambarkan keadaan saat kejadian terjadi meski timnya berganti kemudian.
    /// </remarks>
    private async Task<OprIntegrationEvent> BuildEventAsync(Guid caseId, Guid eventId, string eventType,
        DateTime now, CancellationToken cancellationToken)
    {
        var kasus = await _dbContext.OprCases.AsNoTracking()
            .Where(x => x.Id == caseId && !x.IsDelete)
            .Select(x => new
            {
                x.Id, x.CaseNumber, x.PatientId, x.EncounterId, x.Status, x.Outcome,
                x.CaseType, x.Priority, x.Laterality, x.RequestedAt
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException("Kasus operasi tidak ditemukan.");

        var tindakan = await _dbContext.OprCaseProcedures.AsNoTracking()
            .Where(x => x.OprCaseId == caseId && !x.IsDelete)
            .OrderBy(x => x.Sequence)
            .Select(x => new OprIntegrationEventProcedure
            {
                PatientProcedureId = x.PatientProcedureId,
                ProcedureCode = x.PatientProcedure != null ? x.PatientProcedure.ProcedureCodeSnapshot : string.Empty,
                ProcedureName = x.PatientProcedure != null ? x.PatientProcedure.ProcedureNameSnapshot : string.Empty,
                IsPrimary = x.IsPrimary,
                Sequence = x.Sequence
            })
            .ToListAsync(cancellationToken);

        var pelaksana = await _dbContext.OprTeamMembers.AsNoTracking()
            .Where(x => x.OprCaseId == caseId && x.IsCurrent && !x.IsDelete)
            .OrderBy(x => x.Role)
            .Select(x => new OprIntegrationEventPerformer
            {
                WorkforceId = x.WorkforceId, Role = x.Role.ToString(), IsLead = x.IsLead
            })
            .ToListAsync(cancellationToken);

        return new OprIntegrationEvent
        {
            EventId = eventId,
            EventType = eventType,
            EventVersion = EventVersion,
            OccurredAt = now,
            CaseId = kasus.Id,
            CaseNumber = kasus.CaseNumber,
            PatientId = kasus.PatientId,
            EncounterId = kasus.EncounterId,
            ServiceRequestId = tindakan.FirstOrDefault(x => x.IsPrimary)?.PatientProcedureId
                ?? tindakan.FirstOrDefault()?.PatientProcedureId,
            Case = new OprCaseStatusSnapshot
            {
                Status = kasus.Status.ToString(),
                Outcome = kasus.Outcome?.ToString(),
                CaseType = kasus.CaseType.ToString(),
                Priority = kasus.Priority.ToString(),
                Laterality = kasus.Laterality,
                RequestedAt = kasus.RequestedAt
            },
            Procedures = tindakan,
            Performers = pelaksana
        };
    }

    /// <summary>
    /// Identitas kejadian diturunkan dari tujuan dan kunci idempotency-nya, bukan diacak.
    /// </summary>
    /// <remarks>
    /// Akibatnya kejadian bisnis yang sama selalu memperoleh `EventId` yang sama, berapa kali pun
    /// permintaannya diulang dan dari proses mana pun. Indeks unik pada kolom itu karena itu
    /// benar-benar menjaga keesaan pesan, bukan sekadar mencatat angka acak.
    /// </remarks>
    public static Guid DeterministicEventId(string destination, string idempotencyKey) =>
        new(SHA256.HashData(Encoding.UTF8.GetBytes($"opr-event|{destination}|{idempotencyKey.Trim()}"))[..16]);

    public async Task<OprReconciliationResponse?> GetReconciliationAsync(Guid caseId,
        CancellationToken cancellationToken = default)
    {
        var caseInfo = await _dbContext.OprCases.AsNoTracking()
            .Where(x => x.Id == caseId && !x.IsDelete)
            .Select(x => new { x.Id, x.CaseNumber })
            .FirstOrDefaultAsync(cancellationToken);
        if (caseInfo == null) return null;

        var deliveries = await _dbContext.OprIntegrationDeliveries.AsNoTracking()
            .Where(x => x.OprCaseId == caseId && !x.IsDelete)
            .OrderBy(x => x.Destination).ThenBy(x => x.IdempotencyKey)
            .Select(x => new OprIntegrationDeliveryResponse
            {
                Id = x.Id, OprCaseId = x.OprCaseId, EventId = x.EventId, EventType = x.EventType,
                EventVersion = x.EventVersion, OccurredAt = x.OccurredAt,
                Destination = x.Destination, MessageType = x.MessageType,
                IdempotencyKey = x.IdempotencyKey, CorrelationId = x.CorrelationId,
                PayloadReference = x.PayloadReference, Status = x.Status, RetryCount = x.RetryCount,
                LastAttemptAt = x.LastAttemptAt, LastErrorCode = x.LastErrorCode,
                AcceptedReference = x.AcceptedReference
            })
            .ToListAsync(cancellationToken);

        return new OprReconciliationResponse
        {
            OprCaseId = caseInfo.Id,
            CaseNumber = caseInfo.CaseNumber,
            Deliveries = deliveries,
            PendingCount = deliveries.Count(x => x.Status is OprDeliveryStatus.Pending or OprDeliveryStatus.Processing),
            FailedCount = deliveries.Count(x => x.Status == OprDeliveryStatus.Failed),
            AcceptedCount = deliveries.Count(x => x.Status == OprDeliveryStatus.Accepted),
            BlockedDestinations = [.. BlockedDestinations]
        };
    }

    /// <summary>
    /// Mencatat hasil satu upaya pengiriman. Dipakai adapter downstream ketika kontraknya
    /// sudah disahkan, dan sementara ini oleh operator rekonsiliasi manual.
    /// </summary>
    public async Task<OprIntegrationDeliveryResponse> RecordAttemptAsync(Guid caseId, Guid deliveryId,
        RecordOprDeliveryAttemptRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.IdempotencyKey)) throw new ArgumentException("Idempotency key wajib diisi.");
        var actorUserId = GetUserId(_httpContextAccessor);
        var fingerprint = Hash(string.Join('|', deliveryId, request.Accepted, Normalize(request.AcceptedReference),
            Normalize(request.ErrorCode)));

        var prior = await FindIdempotentAsync(request.IdempotencyKey, cancellationToken);
        if (prior != null)
        {
            EnsureSameCase(prior, caseId);
            EnsureSameFingerprint(prior.Source, fingerprint);
            return (await GetDeliveryAsync(deliveryId, cancellationToken))!;
        }

        var caseStatus = await _dbContext.OprCases.AsNoTracking()
            .Where(x => x.Id == caseId && !x.IsDelete).Select(x => (OprCaseStatus?)x.Status)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException("Kasus operasi tidak ditemukan.");

        var delivery = await _dbContext.OprIntegrationDeliveries
            .FirstOrDefaultAsync(x => x.Id == deliveryId && x.OprCaseId == caseId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Data pengiriman tidak ditemukan.");
        // Pesan yang sudah diterima consumer tidak boleh dikirim ulang; downstream
        // authoritative dan pengiriman kedua berisiko menggandakan efek.
        if (delivery.Status == OprDeliveryStatus.Accepted)
            throw new OperatingRoomConflictException("InvalidStateTransition",
                "Pengiriman ini sudah diterima consumer.");

        var now = DateTime.UtcNow;
        delivery.RetryCount++;
        delivery.LastAttemptAt = now;
        delivery.UpdateDateTime = now;
        delivery.UpdateBy = actorUserId;
        if (request.Accepted)
        {
            delivery.Status = OprDeliveryStatus.Accepted;
            delivery.AcceptedReference = Normalize(request.AcceptedReference);
            delivery.LastErrorCode = null;
        }
        else
        {
            delivery.Status = OprDeliveryStatus.Failed;
            delivery.LastErrorCode = Normalize(request.ErrorCode) ?? "UNKNOWN";
        }

        _dbContext.OprStatusHistories.Add(NewHistory(caseId, caseStatus, caseStatus, AttemptAction,
            $"{delivery.Destination}:{delivery.Status}", request.IdempotencyKey, fingerprint, actorUserId, now));
        await SaveAsync(cancellationToken);

        await _loggerService.AuditAsync(LogCategory, "OperatingRoomIntegration.RecordAttempt",
            "Mencatat hasil pengiriman integrasi operasi.",
            new
            {
                OprCaseId = caseId, ActorUserId = actorUserId, DeliveryId = deliveryId, delivery.Destination,
                Status = delivery.Status.ToString(), delivery.RetryCount, delivery.LastErrorCode,
                CorrelationId = request.IdempotencyKey.Trim()
            });
        return (await GetDeliveryAsync(deliveryId, cancellationToken))!;
    }

    /// <summary>
    /// Membaca amplop kejadian yang dibekukan pada satu baris outbox, apa adanya.
    /// </summary>
    /// <remarks>
    /// Inilah yang dibaca integration layer ketika hendak mengirim atau mengirim ulang. Ia
    /// tidak menyusun ulang isinya dari keadaan kasus sekarang, sehingga pengiriman ulang
    /// membawa isi yang persis sama dengan pengiriman pertama.
    /// </remarks>
    public async Task<OprIntegrationEvent?> GetEventAsync(Guid caseId, Guid deliveryId,
        CancellationToken cancellationToken = default)
    {
        var payload = await _dbContext.OprIntegrationDeliveries.AsNoTracking()
            .Where(x => x.Id == deliveryId && x.OprCaseId == caseId && !x.IsDelete)
            .Select(x => x.PayloadJson)
            .FirstOrDefaultAsync(cancellationToken);

        return string.IsNullOrWhiteSpace(payload)
            ? null
            : JsonSerializer.Deserialize<OprIntegrationEvent>(payload, PayloadOptions);
    }

    /// <summary>
    /// Pesan yang masih menunggu dikirim, paling lama menunggu lebih dulu.
    /// </summary>
    /// <remarks>
    /// Disediakan untuk integration layer yang memproses antrean. Modul Operasi tidak memanggil
    /// consumer mana pun sendiri; ia hanya menyediakan apa yang harus dikirim.
    /// </remarks>
    public Task<List<OprIntegrationDeliveryResponse>> GetPendingAsync(string? destination = null,
        int take = 50, CancellationToken cancellationToken = default) =>
        _dbContext.OprIntegrationDeliveries.AsNoTracking()
            .Where(x => !x.IsDelete && x.Status == OprDeliveryStatus.Pending &&
                (destination == null || x.Destination == destination))
            .OrderBy(x => x.OccurredAt).ThenBy(x => x.Id)
            .Take(take < 1 ? 1 : take > 200 ? 200 : take)
            .Select(x => new OprIntegrationDeliveryResponse
            {
                Id = x.Id, OprCaseId = x.OprCaseId, EventId = x.EventId, EventType = x.EventType,
                EventVersion = x.EventVersion, OccurredAt = x.OccurredAt,
                Destination = x.Destination, MessageType = x.MessageType,
                IdempotencyKey = x.IdempotencyKey, CorrelationId = x.CorrelationId,
                PayloadReference = x.PayloadReference, Status = x.Status, RetryCount = x.RetryCount,
                LastAttemptAt = x.LastAttemptAt, LastErrorCode = x.LastErrorCode,
                AcceptedReference = x.AcceptedReference
            })
            .ToListAsync(cancellationToken);

    /// <summary>Mengembalikan pengiriman gagal ke antrean tanpa menggandakan pesan.</summary>
    public async Task<OprIntegrationDeliveryResponse> RetryAsync(Guid caseId, Guid deliveryId,
        CancellationToken cancellationToken = default)
    {
        var actorUserId = GetUserId(_httpContextAccessor);
        var caseStatus = await _dbContext.OprCases.AsNoTracking()
            .Where(x => x.Id == caseId && !x.IsDelete).Select(x => (OprCaseStatus?)x.Status)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException("Kasus operasi tidak ditemukan.");

        var delivery = await _dbContext.OprIntegrationDeliveries
            .FirstOrDefaultAsync(x => x.Id == deliveryId && x.OprCaseId == caseId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Data pengiriman tidak ditemukan.");
        if (delivery.Status != OprDeliveryStatus.Failed)
            throw new OperatingRoomConflictException("InvalidStateTransition",
                "Hanya pengiriman berstatus Failed yang dapat diantrekan ulang.");

        var now = DateTime.UtcNow;
        delivery.Status = OprDeliveryStatus.Pending;
        delivery.UpdateDateTime = now;
        delivery.UpdateBy = actorUserId;
        _dbContext.OprStatusHistories.Add(NewHistory(caseId, caseStatus, caseStatus, RetryAction,
            $"{delivery.Destination}:Requeued", $"{deliveryId:N}:{delivery.RetryCount}",
            Hash($"{deliveryId}:{delivery.RetryCount}"), actorUserId, now));
        await SaveAsync(cancellationToken);

        await _loggerService.AuditAsync(LogCategory, "OperatingRoomIntegration.Retry",
            "Mengantrekan ulang pengiriman integrasi operasi.",
            new { OprCaseId = caseId, ActorUserId = actorUserId, DeliveryId = deliveryId, delivery.RetryCount });
        return (await GetDeliveryAsync(deliveryId, cancellationToken))!;
    }

    private Task<OprIntegrationDeliveryResponse?> GetDeliveryAsync(Guid deliveryId, CancellationToken cancellationToken) =>
        _dbContext.OprIntegrationDeliveries.AsNoTracking()
            .Where(x => x.Id == deliveryId)
            .Select(x => new OprIntegrationDeliveryResponse
            {
                Id = x.Id, OprCaseId = x.OprCaseId, EventId = x.EventId, EventType = x.EventType,
                EventVersion = x.EventVersion, OccurredAt = x.OccurredAt,
                Destination = x.Destination, MessageType = x.MessageType,
                IdempotencyKey = x.IdempotencyKey, CorrelationId = x.CorrelationId,
                PayloadReference = x.PayloadReference, Status = x.Status, RetryCount = x.RetryCount,
                LastAttemptAt = x.LastAttemptAt, LastErrorCode = x.LastErrorCode,
                AcceptedReference = x.AcceptedReference
            })
            .FirstOrDefaultAsync(cancellationToken);

    private Task<OprStatusHistory?> FindIdempotentAsync(string idempotencyKey, CancellationToken cancellationToken) =>
        _dbContext.OprStatusHistories.AsNoTracking().FirstOrDefaultAsync(x =>
            x.Action == AttemptAction && x.CorrelationId == idempotencyKey.Trim() && !x.IsDelete, cancellationToken);

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
