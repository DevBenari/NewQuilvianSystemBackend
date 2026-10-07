using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Constants;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
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
/// Baris delivery ditulis dalam transaksi yang sama dengan perubahan bisnis, berkunci
/// idempotency, dan dapat direkonsiliasi atau diretry. Sejak <c>BE-RWI-179</c> tujuan Billing
/// punya adapter nyata: <see cref="DeliverToBillingAsync"/> mengirim komponen anestesi, sewa
/// kamar operasi, dan bahan sebagai fakta klinis <c>OPERATING_ROOM</c> lewat
/// <c>ClinicalMilestoneFactProducer</c> → <c>BillingFolioService</c>. Hasil pengiriman manual
/// tetap dapat dicatat lewat <see cref="RecordAttemptAsync"/>.
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

    /// <summary>Komponen biaya OK yang dikirim ke Billing (<c>BE-RWI-179</c>, backend 12.8).</summary>
    public const string AnesthesiaComponent = "ANESTHESIA";
    public const string OperatingRoomRentComponent = "OR_RENT";
    public const string MaterialComponentPrefix = "MATERIAL-";

    /// <summary>
    /// Komponen lama <c>procedure</c> yang dulu disiapkan saat laporan operasi difinalkan. Sejak
    /// <c>INV-RWF-29</c> tindakan operasi hanya ditagih lewat order tindakan, sehingga baris lama
    /// seperti ini tidak pernah dikirim ke Billing.
    /// </summary>
    public const string LegacyProcedureComponent = "procedure";

    /// <summary>
    /// Tujuan yang belum memiliki consumer yang siap menerima, sehingga pesannya menumpuk di
    /// antrean dan rekonsiliasinya masih dilakukan orang.
    /// </summary>
    /// <remarks>
    /// <see cref="InventoryDestination"/> sudah keluar dari daftar ini: pemakaian material kini
    /// dibukukan ke kartu stok Farmasi oleh
    /// <see cref="OperatingRoomInventoryDispatchService"/>. <see cref="BillingDestination"/> keluar
    /// sejak <c>BE-RWI-179</c> (<c>RWI-DEC-196</c>): komponen biaya dikirim lewat
    /// <see cref="DeliverToBillingAsync"/> dengan <c>SourceContext = OPERATING_ROOM</c>.
    /// </remarks>
    private static readonly string[] BlockedDestinations = [];

    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LoggerService _loggerService;
    private readonly ClinicalMilestoneFactProducer _factProducer;

    public OperatingRoomIntegrationService(ApplicationDbContext dbContext,
        IHttpContextAccessor httpContextAccessor, LoggerService loggerService,
        ClinicalMilestoneFactProducer factProducer)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _loggerService = loggerService;
        _factProducer = factProducer;
    }

    /// <summary>
    /// Mengirim setiap delivery Billing kasus ini yang masih <c>Pending</c>. Dipanggil
    /// <see cref="OperatingRoomCompletionEffects"/> sesudah komponen disiapkan, di luar transaksi.
    /// </summary>
    /// <returns>Jumlah delivery yang diterima Billing pada pemanggilan ini.</returns>
    public async Task<int> DeliverPendingBillingAsync(Guid caseId, Guid actorUserId,
        CancellationToken cancellationToken = default)
    {
        var pendingIds = await _dbContext.OprIntegrationDeliveries.AsNoTracking()
            .Where(x => x.OprCaseId == caseId && x.Destination == BillingDestination &&
                x.Status == OprDeliveryStatus.Pending && !x.IsDelete)
            .OrderBy(x => x.CreateDateTime)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var accepted = 0;
        foreach (var deliveryId in pendingIds)
        {
            if (await DeliverToBillingAsync(deliveryId, actorUserId, cancellationToken) == OprDeliveryStatus.Accepted)
                accepted++;
        }
        return accepted;
    }

    /// <summary>
    /// Mengirim satu komponen biaya ke Billing sebagai fakta klinis <c>OPERATING_ROOM</c>
    /// (<c>BE-RWI-179</c>, <c>RWI-DEC-196</c>, <c>RWI-DEC-207</c>: tetap dengan <c>EncounterId</c>
    /// kasus OK). Fakta dengan isi sama tidak pernah menghasilkan revisi kedua, sehingga kirim ulang
    /// tidak menggandakan baris tagihan (<c>INV-RWF-29</c>).
    /// </summary>
    /// <remarks>
    /// Kegagalan tidak pernah membatalkan penyelesaian kasus: delivery menjadi <c>Failed</c> beserta
    /// kodenya, lalu dapat diantrekan ulang lewat <see cref="RetryAsync"/>. Kasus yang tidak
    /// <c>Completed</c> tidak pernah ditagih (<c>INV-RWF-30</c>).
    /// </remarks>
    public async Task<OprDeliveryStatus?> DeliverToBillingAsync(Guid deliveryId, Guid actorUserId,
        CancellationToken cancellationToken = default)
    {
        var delivery = await _dbContext.OprIntegrationDeliveries
            .FirstOrDefaultAsync(x => x.Id == deliveryId && !x.IsDelete, cancellationToken);
        if (delivery == null || delivery.Destination != BillingDestination) return null;
        if (delivery.Status != OprDeliveryStatus.Pending) return delivery.Status;

        var now = DateTime.UtcNow;
        delivery.Status = OprDeliveryStatus.Processing;
        delivery.LastAttemptAt = now;
        delivery.RetryCount++;
        delivery.UpdateDateTime = now;
        delivery.UpdateBy = actorUserId;
        await SaveAsync(cancellationToken);

        string? failureCode = null;
        string? acceptedReference = null;
        try
        {
            var build = await BuildBillingFactAsync(delivery, cancellationToken);
            if (build.FailureCode != null)
            {
                failureCode = build.FailureCode;
            }
            else
            {
                var emission = await _factProducer.EmitChargeEligibilityAsync(build.Request!, actorUserId, cancellationToken);
                if (emission.Kind is ClinicalFactEmissionKind.Emitted or ClinicalFactEmissionKind.Replayed)
                    acceptedReference = emission.ClinicalMilestoneFactId?.ToString("N");
                else
                    failureCode = emission.Code ?? emission.Kind.ToString();
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            failureCode = "BILLING_DELIVERY_ERROR";
            await _loggerService.AuditAsync(LogCategory, "OperatingRoomIntegration.DeliverToBilling.Error",
                "Pengiriman komponen biaya operasi ke Billing gagal dan akan dicoba ulang.",
                new { delivery.OprCaseId, DeliveryId = delivery.Id, ExceptionType = exception.GetType().Name });
            // Konteks dapat tertinggal dengan entity setengah jadi; baca ulang delivery dari database.
            _dbContext.ChangeTracker.Clear();
        }

        var tracked = await _dbContext.OprIntegrationDeliveries
            .FirstAsync(x => x.Id == deliveryId, cancellationToken);
        tracked.Status = failureCode == null ? OprDeliveryStatus.Accepted : OprDeliveryStatus.Failed;
        tracked.AcceptedReference = failureCode == null ? acceptedReference : null;
        tracked.LastErrorCode = failureCode;
        tracked.UpdateDateTime = DateTime.UtcNow;
        tracked.UpdateBy = actorUserId;
        await SaveAsync(cancellationToken);

        await _loggerService.AuditAsync(LogCategory, "OperatingRoomIntegration.DeliverToBilling",
            "Mengirim komponen biaya operasi ke Billing.",
            new
            {
                tracked.OprCaseId, DeliveryId = tracked.Id, tracked.IdempotencyKey,
                Status = tracked.Status.ToString(), tracked.LastErrorCode, tracked.RetryCount
            });
        return tracked.Status;
    }

    /// <summary>
    /// Menyusun fakta klinis dari kunci delivery <c>case:charge:component:revision</c>. Data
    /// klinisnya dibaca saat dikirim — anestesi dari catatan anestesi final, sewa kamar dari durasi
    /// catatan operasi, bahan dari pemakaian <c>Used</c>.
    /// </summary>
    private async Task<(ClinicalMilestoneFactRequest? Request, string? FailureCode)> BuildBillingFactAsync(
        OprIntegrationDelivery delivery, CancellationToken cancellationToken)
    {
        var parts = delivery.IdempotencyKey.Split(':');
        if (parts.Length < 4 || parts[1] != "charge") return (null, "UNKNOWN_COMPONENT");
        var component = parts[2];

        // INV-RWF-29: komponen tindakan lama tidak pernah dikirim; tindakan ditagih lewat order.
        if (string.Equals(component, LegacyProcedureComponent, StringComparison.OrdinalIgnoreCase))
            return (null, "SUPERSEDED_BY_ORDER");

        var opCase = await _dbContext.OprCases.AsNoTracking()
            .Where(x => x.Id == delivery.OprCaseId && !x.IsDelete)
            .Select(x => new { x.Id, x.CaseNumber, x.EncounterId, x.Status })
            .FirstOrDefaultAsync(cancellationToken);
        if (opCase == null) return (null, "CASE_NOT_FOUND");
        // INV-RWF-30: kasus Rejected, Cancelled, atau belum selesai tidak menimbulkan biaya.
        if (opCase.Status != OprCaseStatus.Completed) return (null, "CASE_NOT_COMPLETED");

        var execution = await _dbContext.OprExecutionRecords.AsNoTracking()
            .Where(x => x.OprCaseId == opCase.Id && !x.IsDelete)
            .Select(x => new { x.Id, x.StartedAt, x.FinishedAt })
            .FirstOrDefaultAsync(cancellationToken);
        var durationMinutes = execution?.FinishedAt is { } finishedAt
            ? Math.Max(1m, decimal.Round((decimal)(finishedAt - execution.StartedAt).TotalMinutes, 0, MidpointRounding.AwayFromZero))
            : (decimal?)null;
        var serviceAt = execution?.FinishedAt ?? DateTime.UtcNow;

        Guid sourceItemId;
        decimal? quantity;
        string? unit;
        DateTime occurredAt;

        if (component == AnesthesiaComponent)
        {
            var anesthesia = await _dbContext.OprAnesthesiaRecords.AsNoTracking()
                .Where(x => x.OprCaseId == opCase.Id && !x.IsDelete && x.Status == OprRecordStatus.Final)
                .Select(x => (Guid?)x.Id)
                .FirstOrDefaultAsync(cancellationToken);
            if (!anesthesia.HasValue) return (null, "ANESTHESIA_RECORD_NOT_FINAL");
            sourceItemId = anesthesia.Value;
            quantity = durationMinutes;
            unit = durationMinutes.HasValue ? "MINUTE" : null;
            occurredAt = serviceAt;
        }
        else if (component == OperatingRoomRentComponent)
        {
            if (execution == null || durationMinutes == null) return (null, "OPERATION_DURATION_MISSING");
            sourceItemId = execution.Id;
            quantity = durationMinutes;
            unit = "MINUTE";
            occurredAt = serviceAt;
        }
        else if (component.StartsWith(MaterialComponentPrefix, StringComparison.Ordinal) &&
                 Guid.TryParseExact(component[MaterialComponentPrefix.Length..], "N", out var usageId))
        {
            var usage = await _dbContext.OprMaterialUsages.AsNoTracking()
                .Where(x => x.Id == usageId && x.OprCaseId == opCase.Id && !x.IsDelete)
                .Select(x => new { x.Id, x.Quantity, x.UnitCode, x.Outcome, x.OccurredAt })
                .FirstOrDefaultAsync(cancellationToken);
            if (usage == null) return (null, "MATERIAL_USAGE_NOT_FOUND");
            if (usage.Outcome != OprMaterialOutcome.Used) return (null, "MATERIAL_NOT_USED");
            sourceItemId = usage.Id;
            var hasUnit = usage.Quantity > 0 && !string.IsNullOrWhiteSpace(usage.UnitCode);
            quantity = hasUnit ? usage.Quantity : null;
            unit = hasUnit ? usage.UnitCode.Trim() : null;
            occurredAt = usage.OccurredAt;
        }
        else
        {
            return (null, "UNKNOWN_COMPONENT");
        }

        return (new ClinicalMilestoneFactRequest
        {
            SourceContext = BillingSourceContract.OperatingRoomSourceContext,
            SourceAggregateId = opCase.Id,
            SourceItemId = sourceItemId,
            EffectType = BillingSourceContract.OperatingRoomChargeEffectType,
            // RWI-DEC-207: biaya OK tetap pada kunjungan kasus OK; Billing yang menautkannya ke RANAP.
            EncounterId = opCase.EncounterId,
            OccurredAt = occurredAt,
            Quantity = quantity,
            Unit = unit,
            // Rujukan saja — harga selalu ditetapkan Billing dari MstTariff.
            TariffSnapshot = JsonSerializer.Serialize(new
            {
                source = "OperatingRoom",
                component = component.StartsWith(MaterialComponentPrefix, StringComparison.Ordinal) ? "MATERIAL" : component,
                caseNumber = opCase.CaseNumber
            }),
            CorrelationId = opCase.Id,
            CausationId = delivery.Id
        }, null);
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

        // BE-RWI-179: Billing kini punya consumer; delivery yang diantrekan ulang langsung dicoba
        // kirim. Gagal lagi → kembali Failed beserta kodenya, kasus tetap Completed.
        if (delivery.Destination == BillingDestination)
            await DeliverToBillingAsync(deliveryId, actorUserId, cancellationToken);

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
