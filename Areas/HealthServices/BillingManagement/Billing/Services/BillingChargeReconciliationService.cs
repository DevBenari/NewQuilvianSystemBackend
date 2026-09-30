using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Constants;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Enums;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

/// <summary>
/// Antrean rekonsiliasi penagihan Rawat Jalan (<c>BE-RJE-012</c>, <c>RJ-E2E-DEC-009</c>): fakta
/// klinis yang penyerahannya belum pasti, dan baris folio yang belum masuk invoice.
/// </summary>
/// <remarks>
/// Kirim ulang tidak membangun jalur baru: fakta lewat
/// <see cref="ClinicalMilestoneFactProducer.RedispatchAsync"/>, baris folio lewat
/// <see cref="BillingClinicalChargeBridgeService.TrySyncEffectAsync"/> — identitas dan kunci
/// idempotency sama dengan pengiriman otomatis. Pemegang item dikunci <c>FOR UPDATE NOWAIT</c>
/// dan dijaga token konkurensi, sehingga item yang sedang dipegang pekerja langsung ditolak
/// <c>RJE-VAL-025</c> tanpa menunggu.
/// </remarks>
public sealed class BillingChargeReconciliationService
{
    private const string LogCategory = "BillingManagement";
    private const int MaxPageSize = 100;

    private readonly ApplicationDbContext _dbContext;
    private readonly ClinicalMilestoneFactProducer _factProducer;
    private readonly BillingClinicalChargeBridgeService _bridge;
    private readonly LoggerService _loggerService;

    public BillingChargeReconciliationService(
        ApplicationDbContext dbContext,
        ClinicalMilestoneFactProducer factProducer,
        BillingClinicalChargeBridgeService bridge,
        LoggerService loggerService)
    {
        _dbContext = dbContext;
        _factProducer = factProducer;
        _bridge = bridge;
        _loggerService = loggerService;
    }

    // ================================================================= daftar

    public async Task<PagedResult<ChargeReconciliationItemResponse>> ListAsync(
        ChargeReconciliationQuery query,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);
        var status = Normalize(query.Status)?.ToUpperInvariant();
        if (status is not (null or ChargeReconciliationStatuses.Failed or ChargeReconciliationStatuses.ReconciliationRequired
            or ChargeReconciliationStatuses.Resolved))
            throw new ChargeReconciliationException(null, 422, "Status antrean tidak dikenali.");

        var rows = new List<ChargeReconciliationItemResponse>();
        rows.AddRange(await LoadFactRowsAsync(query, status, cancellationToken));
        rows.AddRange(await LoadEffectRowsAsync(query, status, cancellationToken));

        var domain = Normalize(query.SourceDomain)?.ToUpperInvariant();
        var errorCode = Normalize(query.ErrorCode);
        var search = Normalize(query.Search);
        var filtered = rows
            .Where(x => domain == null || string.Equals(x.SourceDomain, domain, StringComparison.OrdinalIgnoreCase))
            .Where(x => errorCode == null || string.Equals(x.ErrorCode, errorCode, StringComparison.OrdinalIgnoreCase))
            .Where(x => search == null
                || (x.InvoiceNumber?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                || (x.EncounterNumber?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false))
            .OrderByDescending(x => x.ReconciliationRequiredAt ?? x.LastAttemptAt)
            .ThenBy(x => x.Id)
            .ToList();

        return new PagedResult<ChargeReconciliationItemResponse>
        {
            PageNumber = page,
            PageSize = pageSize,
            TotalData = filtered.Count,
            TotalPage = (int)Math.Ceiling(filtered.Count / (double)pageSize),
            Items = filtered.Skip((page - 1) * pageSize).Take(pageSize).ToList()
        };
    }

    private async Task<List<ChargeReconciliationItemResponse>> LoadFactRowsAsync(
        ChargeReconciliationQuery query,
        string? status,
        CancellationToken cancellationToken)
    {
        var facts = _dbContext.Set<CliClinicalMilestoneFact>().AsNoTracking().Where(x => !x.IsDelete);
        facts = status switch
        {
            ChargeReconciliationStatuses.Resolved => facts.Where(x => x.ReconciliationResolvedAt != null),
            ChargeReconciliationStatuses.ReconciliationRequired =>
                facts.Where(x => x.ReconciliationRequiredAt != null && x.ReconciliationResolvedAt == null),
            ChargeReconciliationStatuses.Failed =>
                facts.Where(x => x.ReconciliationRequiredAt == null && x.DispatchStatus == ClinicalFactDispatchStatus.OutcomeUnknown),
            _ => facts.Where(x => x.ReconciliationResolvedAt == null
                && (x.ReconciliationRequiredAt != null || x.DispatchStatus == ClinicalFactDispatchStatus.OutcomeUnknown))
        };
        if (query.EncounterId is { } encounterId) facts = facts.Where(x => x.EncounterId == encounterId);
        if (query.DateFrom is { } from) facts = facts.Where(x => (x.DispatchedAt ?? x.CreateDateTime) >= from.ToUniversalTime());
        if (query.DateTo is { } to) facts = facts.Where(x => (x.DispatchedAt ?? x.CreateDateTime) <= to.ToUniversalTime());

        var list = await (
                from fact in facts
                join encounter in _dbContext.RegPatientEncounters.AsNoTracking() on fact.EncounterId equals encounter.Id
                join user in _dbContext.Users.AsNoTracking() on fact.ReconciliationResolvedByUserId equals (Guid?)user.Id into users
                from resolver in users.DefaultIfEmpty()
                select new
                {
                    fact.Id, fact.EncounterId, encounter.EncounterNumber, fact.SourceContext, fact.SourceAggregateId,
                    fact.SourceItemId, fact.MilestoneFactVersion, fact.BillingOutcomeCode, fact.BillingOutcomeMessage,
                    fact.DispatchAttemptCount, fact.DispatchedAt, fact.NextDispatchAttemptAt, fact.ReconciliationRequiredAt,
                    fact.ReconciliationResolvedAt, ResolverName = resolver != null ? resolver.DisplayName : null,
                    fact.ReconciliationResolutionNote
                })
            .ToListAsync(cancellationToken);

        var invoiceNumbers = await InvoiceNumbersAsync(list.Select(x => x.EncounterId), cancellationToken);
        return list.Select(x => new ChargeReconciliationItemResponse
        {
            ItemType = ChargeReconciliationItemTypes.ClinicalFact,
            Id = x.Id,
            Status = x.ReconciliationResolvedAt != null ? ChargeReconciliationStatuses.Resolved
                : x.ReconciliationRequiredAt != null ? ChargeReconciliationStatuses.ReconciliationRequired
                : ChargeReconciliationStatuses.Failed,
            EncounterId = x.EncounterId,
            EncounterNumber = x.EncounterNumber,
            SourceDomain = DomainOf(x.SourceContext),
            SourceDetailId = (x.SourceItemId ?? x.SourceAggregateId).ToString("D"),
            SourceVersion = x.MilestoneFactVersion,
            ErrorCode = x.BillingOutcomeCode,
            ErrorMessage = x.BillingOutcomeMessage,
            AttemptCount = x.DispatchAttemptCount,
            LastAttemptAt = x.DispatchedAt,
            NextAttemptAt = x.NextDispatchAttemptAt,
            ReconciliationRequiredAt = x.ReconciliationRequiredAt,
            ResolvedAt = x.ReconciliationResolvedAt,
            ResolvedByUserName = x.ResolverName,
            ResolutionNote = x.ReconciliationResolutionNote,
            InvoiceNumber = invoiceNumbers.GetValueOrDefault(x.EncounterId)
        }).ToList();
    }

    private async Task<List<ChargeReconciliationItemResponse>> LoadEffectRowsAsync(
        ChargeReconciliationQuery query,
        string? status,
        CancellationToken cancellationToken)
    {
        var statuses = status switch
        {
            ChargeReconciliationStatuses.Resolved => new[] { BillingInvoiceSyncStatus.Resolved },
            ChargeReconciliationStatuses.ReconciliationRequired => new[] { BillingInvoiceSyncStatus.ReconciliationRequired },
            ChargeReconciliationStatuses.Failed => new[] { BillingInvoiceSyncStatus.Failed },
            _ => new[] { BillingInvoiceSyncStatus.Failed, BillingInvoiceSyncStatus.ReconciliationRequired }
        };

        var effects =
            from effect in _dbContext.Set<BilProcessingEffect>().AsNoTracking()
            join folio in _dbContext.Set<BilFolio>().AsNoTracking() on effect.FolioId equals folio.Id
            join encounter in _dbContext.RegPatientEncounters.AsNoTracking() on folio.EncounterId equals encounter.Id
            where !effect.IsDelete && statuses.Contains(effect.InvoiceSyncStatus)
            select new { effect, folio.EncounterId, encounter.EncounterNumber };
        if (query.EncounterId is { } encounterId) effects = effects.Where(x => x.EncounterId == encounterId);
        if (query.DateFrom is { } from) effects = effects.Where(x => (x.effect.UpdateDateTime ?? x.effect.CreateDateTime) >= from.ToUniversalTime());
        if (query.DateTo is { } to) effects = effects.Where(x => (x.effect.UpdateDateTime ?? x.effect.CreateDateTime) <= to.ToUniversalTime());

        var list = await (
                from row in effects
                join user in _dbContext.Users.AsNoTracking() on row.effect.ReconciliationResolvedByUserId equals (Guid?)user.Id into users
                from resolver in users.DefaultIfEmpty()
                select new
                {
                    row.effect.Id, row.EncounterId, row.EncounterNumber, row.effect.SourceContext, row.effect.InvoiceSourceDomain,
                    row.effect.InvoiceSourceDetailId, row.effect.MilestoneFactVersion, row.effect.InvoiceSyncStatus,
                    row.effect.InvoiceSyncErrorCode, row.effect.InvoiceSyncErrorMessage, row.effect.InvoiceSyncAttemptCount,
                    LastAttemptAt = row.effect.UpdateDateTime ?? row.effect.CreateDateTime, row.effect.InvoiceSyncNextAttemptAt,
                    row.effect.ReconciliationResolvedAt, ResolverName = resolver != null ? resolver.DisplayName : null,
                    row.effect.ReconciliationResolutionNote
                })
            .ToListAsync(cancellationToken);

        var invoiceNumbers = await InvoiceNumbersAsync(list.Select(x => x.EncounterId), cancellationToken);
        return list.Select(x => new ChargeReconciliationItemResponse
        {
            ItemType = ChargeReconciliationItemTypes.ChargeLine,
            Id = x.Id,
            Status = x.InvoiceSyncStatus switch
            {
                BillingInvoiceSyncStatus.Resolved => ChargeReconciliationStatuses.Resolved,
                BillingInvoiceSyncStatus.ReconciliationRequired => ChargeReconciliationStatuses.ReconciliationRequired,
                _ => ChargeReconciliationStatuses.Failed
            },
            EncounterId = x.EncounterId,
            EncounterNumber = x.EncounterNumber,
            SourceDomain = x.InvoiceSourceDomain ?? DomainOf(x.SourceContext),
            SourceDetailId = x.InvoiceSourceDetailId,
            SourceVersion = x.MilestoneFactVersion,
            ErrorCode = x.InvoiceSyncErrorCode,
            ErrorMessage = x.InvoiceSyncErrorMessage,
            AttemptCount = x.InvoiceSyncAttemptCount,
            LastAttemptAt = x.LastAttemptAt,
            NextAttemptAt = x.InvoiceSyncNextAttemptAt,
            ReconciliationRequiredAt = x.InvoiceSyncStatus == BillingInvoiceSyncStatus.Failed ? null : x.LastAttemptAt,
            ResolvedAt = x.ReconciliationResolvedAt,
            ResolvedByUserName = x.ResolverName,
            ResolutionNote = x.ReconciliationResolutionNote,
            InvoiceNumber = invoiceNumbers.GetValueOrDefault(x.EncounterId)
        }).ToList();
    }

    private async Task<Dictionary<Guid, string>> InvoiceNumbersAsync(IEnumerable<Guid> encounterIds, CancellationToken cancellationToken)
    {
        var ids = encounterIds.Distinct().ToList();
        if (ids.Count == 0) return new Dictionary<Guid, string>();
        return await _dbContext.BilInvoices.AsNoTracking()
            .Where(x => ids.Contains(x.EncounterId) && !x.IsDelete)
            .GroupBy(x => x.EncounterId)
            .Select(x => new { EncounterId = x.Key, InvoiceNumber = x.Min(i => i.InvoiceNumber) })
            .ToDictionaryAsync(x => x.EncounterId, x => x.InvoiceNumber ?? string.Empty, cancellationToken);
    }

    // ============================================================== kirim ulang

    public async Task<ChargeReconciliationItemResponse> RetryAsync(
        string itemType,
        Guid id,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var type = ParseItemType(itemType);
        if (type == ChargeReconciliationItemTypes.ClinicalFact)
        {
            await LockAndUpdateFactAsync(id, fact =>
            {
                if (fact.DispatchStatus is not (ClinicalFactDispatchStatus.Pending or ClinicalFactDispatchStatus.OutcomeUnknown))
                    throw AlreadyRecorded();
                // Kirim ulang manual adalah satu percobaan baru; pekerja tidak ikut memegangnya.
                fact.ReconciliationRequiredAt = null;
                fact.NextDispatchAttemptAt = null;
            }, cancellationToken);
            await _factProducer.RedispatchAsync(id, cancellationToken);
        }
        else
        {
            await LockAndUpdateEffectAsync(id, effect =>
            {
                if (effect.InvoiceSyncStatus is not (BillingInvoiceSyncStatus.Pending or BillingInvoiceSyncStatus.Failed
                    or BillingInvoiceSyncStatus.ReconciliationRequired))
                    throw AlreadyRecorded();
                effect.InvoiceSyncStatus = BillingInvoiceSyncStatus.Pending;
                effect.InvoiceSyncNextAttemptAt = null;
            }, cancellationToken);
            await _bridge.TrySyncEffectAsync(id, cancellationToken);
        }

        var result = await GetItemAsync(type, id, cancellationToken);
        await _loggerService.AuditAsync(LogCategory, "BillingChargeReconciliation.Retry",
            "Mengirim ulang item antrean rekonsiliasi.",
            new { ItemType = type, Id = id, ActorUserId = actorUserId, result.Status, result.ErrorCode });
        return result;
    }

    // ============================================================== selesaikan

    public async Task<ChargeReconciliationItemResponse> ResolveAsync(
        string itemType,
        Guid id,
        ResolveChargeReconciliationRequest request,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var type = ParseItemType(itemType);
        var resolution = Normalize(request.Resolution)?.ToUpperInvariant();
        if (resolution == null || !ChargeReconciliationResolutions.All.Contains(resolution))
            throw new ChargeReconciliationException("RJE-VAL-023", 422, "Pilih jenis penyelesaian yang tersedia.");
        var note = Normalize(request.Note);
        if (note == null || note.Length < 10 || note.Length > 500)
            throw new ChargeReconciliationException("RJE-VAL-022", 422, "Tuliskan alasan penyelesaian, minimal 10 karakter.");

        var now = DateTime.UtcNow;
        var resolutionNote = $"{resolution}: {note}";
        if (type == ChargeReconciliationItemTypes.ClinicalFact)
        {
            await LockAndUpdateFactAsync(id, fact =>
            {
                if (fact.DispatchStatus is not (ClinicalFactDispatchStatus.Pending or ClinicalFactDispatchStatus.OutcomeUnknown))
                    throw AlreadyRecorded();
                fact.ReconciliationRequiredAt ??= now;
                fact.NextDispatchAttemptAt = null;
                fact.ReconciliationResolvedAt = now;
                fact.ReconciliationResolvedByUserId = actorUserId;
                fact.ReconciliationResolutionNote = resolutionNote;
            }, cancellationToken);
        }
        else
        {
            await LockAndUpdateEffectAsync(id, effect =>
            {
                if (effect.InvoiceSyncStatus is not (BillingInvoiceSyncStatus.Pending or BillingInvoiceSyncStatus.Failed
                    or BillingInvoiceSyncStatus.ReconciliationRequired))
                    throw AlreadyRecorded();
                effect.InvoiceSyncStatus = BillingInvoiceSyncStatus.Resolved;
                effect.InvoiceSyncNextAttemptAt = null;
                effect.ReconciliationResolvedAt = now;
                effect.ReconciliationResolvedByUserId = actorUserId;
                effect.ReconciliationResolutionNote = resolutionNote;
            }, cancellationToken);
        }

        var result = await GetItemAsync(type, id, cancellationToken);
        await _loggerService.AuditAsync(LogCategory, "BillingChargeReconciliation.Resolve",
            "Menyelesaikan item antrean rekonsiliasi secara manual.",
            new { ItemType = type, Id = id, ActorUserId = actorUserId, Resolution = resolution });
        return result;
    }

    // ================================================================= bantuan

    private async Task LockAndUpdateFactAsync(Guid id, Action<CliClinicalMilestoneFact> apply, CancellationToken cancellationToken)
    {
        await LockAndUpdateAsync<CliClinicalMilestoneFact>(
            """SELECT 1 FROM public."CliClinicalMilestoneFact" WHERE "Id" = {0} FOR UPDATE NOWAIT""",
            id,
            x => x.Id == id && !x.IsDelete,
            fact =>
            {
                if (fact.ReconciliationResolvedAt != null) throw AlreadyResolved();
                apply(fact);
                fact.Version += 1;
                fact.UpdateDateTime = DateTime.UtcNow;
            },
            cancellationToken);
    }

    private async Task LockAndUpdateEffectAsync(Guid id, Action<BilProcessingEffect> apply, CancellationToken cancellationToken)
    {
        await LockAndUpdateAsync<BilProcessingEffect>(
            """SELECT 1 FROM public."BilProcessingEffect" WHERE "Id" = {0} FOR UPDATE NOWAIT""",
            id,
            x => x.Id == id && !x.IsDelete,
            effect =>
            {
                if (effect.InvoiceSyncStatus == BillingInvoiceSyncStatus.Resolved) throw AlreadyResolved();
                apply(effect);
                effect.InvoiceSyncVersion += 1;
                effect.UpdateDateTime = DateTime.UtcNow;
            },
            cancellationToken);
    }

    /// <summary>
    /// Mengunci baris tanpa menunggu, menerapkan perubahan, lalu menyimpan dengan token konkurensi.
    /// Transaksi di-commit sebelum pengiriman ulang, karena producer dan jembatan menolak berjalan di
    /// dalam transaksi pemanggil.
    /// </summary>
    private async Task LockAndUpdateAsync<T>(
        string lockSql,
        Guid id,
        System.Linq.Expressions.Expression<Func<T, bool>> predicate,
        Action<T> apply,
        CancellationToken cancellationToken) where T : class
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await _dbContext.Database.ExecuteSqlRawAsync(lockSql, [id], cancellationToken);
            var entity = await _dbContext.Set<T>().FirstOrDefaultAsync(predicate, cancellationToken)
                ?? throw new KeyNotFoundException("Item antrean rekonsiliasi tidak ditemukan.");
            apply(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception exception) when (IsBusy(exception))
        {
            await transaction.RollbackAsync(CancellationToken.None);
            _dbContext.ChangeTracker.Clear();
            throw new ChargeReconciliationException("RJE-VAL-025", 409,
                "Item sedang diproses sistem. Muat ulang beberapa saat lagi.");
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            _dbContext.ChangeTracker.Clear();
            throw;
        }

        _dbContext.ChangeTracker.Clear();
    }

    private async Task<ChargeReconciliationItemResponse> GetItemAsync(string type, Guid id, CancellationToken cancellationToken) =>
        (await LoadSingleAsync(type, id, cancellationToken)).FirstOrDefault()
            ?? throw new KeyNotFoundException("Item antrean rekonsiliasi tidak ditemukan.");

    /// <summary>Membaca satu item apa pun statusnya (termasuk yang sudah <c>Synced</c>), untuk respons tindakan.</summary>
    private async Task<List<ChargeReconciliationItemResponse>> LoadSingleAsync(string type, Guid id, CancellationToken cancellationToken)
    {
        if (type == ChargeReconciliationItemTypes.ClinicalFact)
        {
            var fact = await (
                    from x in _dbContext.Set<CliClinicalMilestoneFact>().AsNoTracking()
                    join encounter in _dbContext.RegPatientEncounters.AsNoTracking() on x.EncounterId equals encounter.Id
                    join user in _dbContext.Users.AsNoTracking() on x.ReconciliationResolvedByUserId equals (Guid?)user.Id into users
                    from resolver in users.DefaultIfEmpty()
                    where x.Id == id
                    select new { x, encounter.EncounterNumber, ResolverName = resolver != null ? resolver.DisplayName : null })
                .FirstOrDefaultAsync(cancellationToken);
            if (fact == null) return [];
            var invoice = await InvoiceNumbersAsync([fact.x.EncounterId], cancellationToken);
            return
            [
                new ChargeReconciliationItemResponse
                {
                    ItemType = type,
                    Id = fact.x.Id,
                    Status = fact.x.ReconciliationResolvedAt != null ? ChargeReconciliationStatuses.Resolved
                        : fact.x.ReconciliationRequiredAt != null ? ChargeReconciliationStatuses.ReconciliationRequired
                        : fact.x.DispatchStatus == ClinicalFactDispatchStatus.Dispatched ? "DISPATCHED"
                        : ChargeReconciliationStatuses.Failed,
                    EncounterId = fact.x.EncounterId,
                    EncounterNumber = fact.EncounterNumber,
                    SourceDomain = DomainOf(fact.x.SourceContext),
                    SourceDetailId = (fact.x.SourceItemId ?? fact.x.SourceAggregateId).ToString("D"),
                    SourceVersion = fact.x.MilestoneFactVersion,
                    ErrorCode = fact.x.BillingOutcomeCode,
                    ErrorMessage = fact.x.BillingOutcomeMessage,
                    AttemptCount = fact.x.DispatchAttemptCount,
                    LastAttemptAt = fact.x.DispatchedAt,
                    NextAttemptAt = fact.x.NextDispatchAttemptAt,
                    ReconciliationRequiredAt = fact.x.ReconciliationRequiredAt,
                    ResolvedAt = fact.x.ReconciliationResolvedAt,
                    ResolvedByUserName = fact.ResolverName,
                    ResolutionNote = fact.x.ReconciliationResolutionNote,
                    InvoiceNumber = invoice.GetValueOrDefault(fact.x.EncounterId)
                }
            ];
        }

        var row = await (
                from effect in _dbContext.Set<BilProcessingEffect>().AsNoTracking()
                join folio in _dbContext.Set<BilFolio>().AsNoTracking() on effect.FolioId equals folio.Id
                join encounter in _dbContext.RegPatientEncounters.AsNoTracking() on folio.EncounterId equals encounter.Id
                join user in _dbContext.Users.AsNoTracking() on effect.ReconciliationResolvedByUserId equals (Guid?)user.Id into users
                from resolver in users.DefaultIfEmpty()
                where effect.Id == id
                select new { effect, folio.EncounterId, encounter.EncounterNumber, ResolverName = resolver != null ? resolver.DisplayName : null })
            .FirstOrDefaultAsync(cancellationToken);
        if (row == null) return [];
        var invoices = await InvoiceNumbersAsync([row.EncounterId], cancellationToken);
        return
        [
            new ChargeReconciliationItemResponse
            {
                ItemType = type,
                Id = row.effect.Id,
                Status = row.effect.InvoiceSyncStatus switch
                {
                    BillingInvoiceSyncStatus.Resolved => ChargeReconciliationStatuses.Resolved,
                    BillingInvoiceSyncStatus.ReconciliationRequired => ChargeReconciliationStatuses.ReconciliationRequired,
                    BillingInvoiceSyncStatus.Synced => "SYNCED",
                    BillingInvoiceSyncStatus.Pending => "PENDING",
                    _ => ChargeReconciliationStatuses.Failed
                },
                EncounterId = row.EncounterId,
                EncounterNumber = row.EncounterNumber,
                SourceDomain = row.effect.InvoiceSourceDomain ?? DomainOf(row.effect.SourceContext),
                SourceDetailId = row.effect.InvoiceSourceDetailId,
                SourceVersion = row.effect.MilestoneFactVersion,
                ErrorCode = row.effect.InvoiceSyncErrorCode,
                ErrorMessage = row.effect.InvoiceSyncErrorMessage,
                AttemptCount = row.effect.InvoiceSyncAttemptCount,
                LastAttemptAt = row.effect.UpdateDateTime ?? row.effect.CreateDateTime,
                NextAttemptAt = row.effect.InvoiceSyncNextAttemptAt,
                ResolvedAt = row.effect.ReconciliationResolvedAt,
                ResolvedByUserName = row.ResolverName,
                ResolutionNote = row.effect.ReconciliationResolutionNote,
                InvoiceNumber = invoices.GetValueOrDefault(row.EncounterId)
            }
        ];
    }

    private static string ParseItemType(string? itemType) => itemType?.Trim().ToUpperInvariant() switch
    {
        ChargeReconciliationItemTypes.ClinicalFact => ChargeReconciliationItemTypes.ClinicalFact,
        ChargeReconciliationItemTypes.ChargeLine => ChargeReconciliationItemTypes.ChargeLine,
        _ => throw new ChargeReconciliationException("RJE-VAL-024", 422, "Jenis item tidak dikenali.")
    };

    private static string DomainOf(string sourceContext) => sourceContext switch
    {
        BillingSourceContract.ProcedureSourceContext => BillingBridgeSourceDomains.Procedure,
        BillingSourceContract.LaboratorySourceContext => BillingBridgeSourceDomains.Laboratory,
        BillingSourceContract.RadiologySourceContext => BillingBridgeSourceDomains.Radiology,
        BillingSourceContract.PrescriptionSourceContext => BillingBridgeSourceDomains.Pharmacy,
        BillingSourceContract.ConsultationSourceContext => BillingBridgeSourceDomains.Consultation,
        _ => sourceContext.ToUpperInvariant()
    };

    private static bool IsBusy(Exception exception)
    {
        var postgres = exception as PostgresException ?? exception.InnerException as PostgresException;
        return exception is DbUpdateConcurrencyException || postgres?.SqlState == PostgresErrorCodes.LockNotAvailable;
    }

    private static ChargeReconciliationException AlreadyRecorded() =>
        new("RJE-VAL-020", 422, "Item ini sudah tercatat di tagihan dan tidak perlu dikirim ulang.");

    private static ChargeReconciliationException AlreadyResolved() =>
        new("RJE-VAL-021", 409, "Item ini sudah diselesaikan secara manual.");

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

/// <summary>Penolakan antrean rekonsiliasi beserta kode validation matrix dan status HTTP-nya.</summary>
public sealed class ChargeReconciliationException(string? code, int statusCode, string message) : Exception(message)
{
    public string? Code { get; } = code;
    public int StatusCode { get; } = statusCode;
}
