using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

public sealed class BillingInvoiceService
{
    private const string LogCategory = "HealthServices.BillingManagement.Billing";
    private readonly ApplicationDbContext _dbContext;
    private readonly IBillingChargeSourceAdapter _sourceAdapter;
    private readonly BillingNumberSeriesService _numberSeries;
    private readonly LoggerService _loggerService;

    public BillingInvoiceService(
        ApplicationDbContext dbContext,
        IBillingChargeSourceAdapter sourceAdapter,
        BillingNumberSeriesService numberSeries,
        LoggerService loggerService)
    {
        _dbContext = dbContext;
        _sourceAdapter = sourceAdapter;
        _numberSeries = numberSeries;
        _loggerService = loggerService;
    }

    public async Task<PagedResult<InvoiceSummaryResponse>> GetPagedAsync(BillingInvoiceQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.BilInvoices.AsNoTracking().Where(x => !x.IsDelete);
        if (request.EncounterId.HasValue) query = query.Where(x => x.EncounterId == request.EncounterId.Value);
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var status = request.Status.Trim().ToUpperInvariant();
            query = query.Where(x => x.Status == status);
        }
        if (!string.IsNullOrWhiteSpace(request.ServiceType))
        {
            var serviceType = request.ServiceType.Trim().ToUpperInvariant();
            query = query.Where(x => x.ServiceType == serviceType);
        }
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToUpper();
            query = query.Where(x => x.InvoiceNumber.ToUpper().Contains(search));
        }
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(x => x.CreateDateTime)
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
            .Select(x => new InvoiceSummaryResponse
            {
                Id = x.Id,
                EncounterId = x.EncounterId,
                InvoiceNumber = x.InvoiceNumber,
                ServiceType = x.ServiceType,
                Status = x.Status,
                CurrentCalculationVersion = x.CurrentCalculationVersion,
                RunningGrossAmount = x.Items.Where(i => !i.IsDelete && i.Status != BillingInvoiceItemStatuses.Voided)
                    .Sum(i => i.Quantity * i.UnitPrice),
                ActiveItemCount = x.Items.Count(i => !i.IsDelete && i.Status != BillingInvoiceItemStatuses.Voided),
                CreateDateTime = x.CreateDateTime,
                RowVersion = x.RowVersion
            }).ToListAsync(cancellationToken);
        return new PagedResult<InvoiceSummaryResponse>
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)request.PageSize),
            Items = items
        };
    }

    public async Task<InvoiceDetailResponse> GetDetailAsync(Guid id, CancellationToken cancellationToken)
    {
        var invoice = await _dbContext.BilInvoices.AsNoTracking().Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Invoice Billing tidak ditemukan.");
        return MapDetail(invoice, false);
    }

    public async Task<InvoiceDetailResponse> UpsertChargeAsync(
        UpsertChargeRequest request,
        Guid idempotencyKey,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        ValidateRequest(request, idempotencyKey);
        var source = _sourceAdapter.ValidateAndNormalize(request);
        var payloadHash = ComputePayloadHash(request, source);
        IDbContextTransaction? transaction = null;
        try
        {
            if (_dbContext.Database.IsRelational())
            {
                transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
                await AcquireLockAsync($"BIL_SOURCE_{source.SourceDomain}_{source.SourceDetailId}", cancellationToken);
                await AcquireLockAsync($"BIL_ENCOUNTER_{request.EncounterId:N}", cancellationToken);
            }

            var priorReceipt = await _dbContext.BilChargeReceipts.AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey, cancellationToken);
            if (priorReceipt is not null)
            {
                if (priorReceipt.PayloadHash != payloadHash
                    || priorReceipt.SourceDomain != source.SourceDomain
                    || priorReceipt.SourceDetailId != source.SourceDetailId)
                    throw new BillingInvoiceConflictException("Permintaan yang sama memiliki isi berbeda; gunakan permintaan baru.");
                var replayInvoice = await LoadInvoiceByItemAsync(priorReceipt.InvoiceItemId, cancellationToken);
                if (transaction is not null) await transaction.CommitAsync(cancellationToken);
                return MapDetail(replayInvoice, true);
            }

            var encounter = await _dbContext.TrxPatientEncounters.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.EncounterId && !x.IsDelete && !x.IsCancel, cancellationToken)
                ?? throw new KeyNotFoundException("Encounter tidak ditemukan.");
            var categoryExists = await _dbContext.MstBillingItemCategories.AsNoTracking()
                .AnyAsync(x => x.Id == request.CategoryId && !x.IsDelete && !x.IsCancel && x.IsActive, cancellationToken);
            if (!categoryExists) throw new BillingInvoiceValidationException("Kategori billing tidak ditemukan atau tidak aktif.");

            var invoice = await _dbContext.BilInvoices.Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.EncounterId == request.EncounterId && !x.IsDelete, cancellationToken);
            var createdInvoice = invoice is null;
            if (invoice is null)
            {
                invoice = new BilInvoice
                {
                    EncounterId = request.EncounterId,
                    InvoiceNumber = await _numberSeries.AllocateInvoiceNumberAsync(actorUserId, DateTimeOffset.UtcNow, cancellationToken),
                    ServiceType = MapServiceType(encounter.EncounterType),
                    Status = BillingInvoiceStatuses.Open,
                    CurrentCalculationVersion = 0,
                    RowVersion = Guid.NewGuid(),
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = actorUserId
                };
                _dbContext.BilInvoices.Add(invoice);
            }
            if (invoice.Status != BillingInvoiceStatuses.Open)
                throw new BillingInvoiceValidationException("Invoice final tidak dapat diedit; ajukan adjustment.");

            var existingItem = await _dbContext.BilInvoiceItems.FirstOrDefaultAsync(
                x => x.SourceDomain == source.SourceDomain && x.SourceDetailId == source.SourceDetailId
                    && x.Status != BillingInvoiceItemStatuses.Voided && !x.IsDelete,
                cancellationToken);
            var isReplay = false;
            BilInvoiceItem item;
            if (existingItem is not null)
            {
                if (existingItem.InvoiceId != invoice.Id)
                    throw new BillingInvoiceConflictException("Item pelayanan ini sudah tercatat pada invoice lain.");
                if (request.SourceVersion < existingItem.SourceVersion)
                    throw new BillingInvoiceConflictException("Versi source lebih lama dari data Billing saat ini.");
                if (request.SourceVersion == existingItem.SourceVersion)
                {
                    if (existingItem.SourcePayloadHash != payloadHash)
                        throw new BillingInvoiceConflictException("Source version yang sama memiliki isi berbeda.");
                    isReplay = true;
                }
                else
                {
                    ApplySource(existingItem, request, source, payloadHash, idempotencyKey, actorUserId);
                    invoice.RowVersion = Guid.NewGuid();
                    invoice.UpdateDateTime = DateTime.UtcNow;
                    invoice.UpdateBy = actorUserId;
                }
                item = existingItem;
            }
            else
            {
                item = new BilInvoiceItem
                {
                    InvoiceId = invoice.Id,
                    Invoice = invoice,
                    Status = BillingInvoiceItemStatuses.Active,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = actorUserId
                };
                ApplySource(item, request, source, payloadHash, idempotencyKey, actorUserId, false);
                _dbContext.BilInvoiceItems.Add(item);
                invoice.RowVersion = Guid.NewGuid();
            }

            _dbContext.BilChargeReceipts.Add(new BilChargeReceipt
            {
                IdempotencyKey = idempotencyKey,
                InvoiceItemId = item.Id,
                SourceDomain = source.SourceDomain,
                SourceDetailId = source.SourceDetailId,
                PayloadHash = payloadHash,
                CorrelationId = request.CorrelationId,
                ReceivedAt = DateTimeOffset.UtcNow,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            });

            await _dbContext.SaveChangesAsync(cancellationToken);
            if (transaction is not null) await transaction.CommitAsync(cancellationToken);
            await AuditAsync(createdInvoice ? "BillingInvoice.CreateCharge" : "BillingInvoice.UpsertCharge",
                invoice, item, request.CorrelationId, actorUserId, isReplay);
            return MapDetail(invoice, isReplay);
        }
        catch (DbUpdateException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new BillingInvoiceConflictException("Charge tidak dapat disimpan karena invoice, source, atau idempotency key sudah diproses.", exception);
        }
        catch
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }

    private async Task<BilInvoice> LoadInvoiceByItemAsync(Guid itemId, CancellationToken cancellationToken)
    {
        var invoiceId = await _dbContext.BilInvoiceItems.AsNoTracking()
            .Where(x => x.Id == itemId).Select(x => x.InvoiceId).SingleOrDefaultAsync(cancellationToken);
        if (invoiceId == Guid.Empty) throw new BillingInvoiceConflictException("Receipt idempotency tidak memiliki item Billing yang valid.");
        return await _dbContext.BilInvoices.AsNoTracking().Include(x => x.Items)
            .SingleAsync(x => x.Id == invoiceId && !x.IsDelete, cancellationToken);
    }

    private async Task AcquireLockAsync(string key, CancellationToken cancellationToken) =>
        await _dbContext.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(hashtext({0}));", [key], cancellationToken);

    private Task AuditAsync(string action, BilInvoice invoice, BilInvoiceItem item, Guid correlationId, Guid actorUserId, bool replay) =>
        _loggerService.AuditAsync(LogCategory, action, "Perubahan running invoice dari source pelayanan.", new
        {
            InvoiceId = invoice.Id,
            InvoiceItemId = item.Id,
            item.SourceDomain,
            item.SourceVersion,
            item.SourceStatus,
            item.Quantity,
            item.UnitPrice,
            item.DoctorShare,
            CorrelationId = correlationId,
            ActorUserId = actorUserId,
            IsReplay = replay
        });

    private static void ApplySource(BilInvoiceItem item, UpsertChargeRequest request, BillingChargeSourceSnapshot source,
        string payloadHash, Guid idempotencyKey, Guid actorUserId, bool markUpdate = true)
    {
        item.SourceDomain = source.SourceDomain;
        item.SourceDetailId = source.SourceDetailId;
        item.SourceVersion = request.SourceVersion;
        item.SourceContractVersion = request.ContractVersion.Trim();
        item.SourceStatus = source.SourceStatus;
        item.SourceOccurredAt = request.OccurredAt;
        item.CategoryId = request.CategoryId;
        item.DescriptionSnapshot = request.DescriptionSnapshot.Trim();
        item.Quantity = request.Quantity;
        item.UnitPrice = request.UnitPrice;
        item.DoctorShare = request.DoctorShare;
        item.LastIdempotencyKey = idempotencyKey;
        item.LastCorrelationId = request.CorrelationId;
        item.LastCausationId = request.CausationId;
        item.SourcePayloadHash = payloadHash;
        if (markUpdate)
        {
            item.UpdateDateTime = DateTime.UtcNow;
            item.UpdateBy = actorUserId;
        }
    }

    private static void ValidateRequest(UpsertChargeRequest request, Guid idempotencyKey)
    {
        if (idempotencyKey == Guid.Empty) throw new BillingInvoiceValidationException("Idempotency-Key wajib diisi.");
        if (request.EncounterId == Guid.Empty) throw new BillingInvoiceValidationException("EncounterId wajib diisi.");
        if (request.CategoryId == Guid.Empty) throw new BillingInvoiceValidationException("CategoryId wajib diisi.");
        if (request.SourceVersion <= 0) throw new BillingInvoiceValidationException("SourceVersion harus lebih besar dari nol.");
        if (request.OccurredAt == default) throw new BillingInvoiceValidationException("OccurredAt wajib diisi.");
        if (request.CorrelationId == Guid.Empty || request.CausationId == Guid.Empty)
            throw new BillingInvoiceValidationException("CorrelationId dan CausationId wajib diisi.");
        if (string.IsNullOrWhiteSpace(request.DescriptionSnapshot))
            throw new BillingInvoiceValidationException("DescriptionSnapshot wajib diisi.");
        if (request.Quantity <= 0 || request.UnitPrice < 0 || request.DoctorShare < 0)
            throw new BillingInvoiceValidationException("Quantity harus positif dan nominal tidak boleh negatif.");
        if (request.DoctorShare > request.Quantity * request.UnitPrice)
            throw new BillingInvoiceValidationException("DoctorShare tidak boleh melebihi gross item.");
    }

    private static string ComputePayloadHash(UpsertChargeRequest request, BillingChargeSourceSnapshot source)
    {
        var canonical = string.Join('|', request.EncounterId.ToString("N"), source.SourceDomain, source.SourceDetailId,
            request.SourceVersion.ToString(CultureInfo.InvariantCulture), source.SourceStatus,
            request.OccurredAt.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture), request.CategoryId.ToString("N"),
            request.DescriptionSnapshot.Trim(), request.Quantity.ToString(CultureInfo.InvariantCulture),
            request.UnitPrice.ToString(CultureInfo.InvariantCulture), request.DoctorShare.ToString(CultureInfo.InvariantCulture),
            request.ContractVersion.Trim(), request.CorrelationId.ToString("N"), request.CausationId.ToString("N"));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    }

    private static string MapServiceType(EncounterType encounterType) => encounterType switch
    {
        EncounterType.Outpatient => "RAJAL",
        EncounterType.Emergency => "IGD",
        EncounterType.Inpatient => "RANAP",
        EncounterType.MedicalCheckup => "MCU",
        EncounterType.Telemedicine => "TELEMEDICINE",
        _ => throw new BillingInvoiceValidationException("Jenis encounter belum didukung untuk Billing.")
    };

    private static InvoiceDetailResponse MapDetail(BilInvoice invoice, bool isReplay)
    {
        var activeItems = invoice.Items.Where(x => !x.IsDelete && x.Status != BillingInvoiceItemStatuses.Voided)
            .OrderBy(x => x.CreateDateTime).Select(x => new InvoiceItemResponse
            {
                Id = x.Id,
                SourceDomain = x.SourceDomain,
                SourceDetailId = x.SourceDetailId,
                SourceVersion = x.SourceVersion,
                SourceContractVersion = x.SourceContractVersion,
                SourceStatus = x.SourceStatus,
                SourceOccurredAt = x.SourceOccurredAt,
                CategoryId = x.CategoryId,
                DescriptionSnapshot = x.DescriptionSnapshot,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice,
                DoctorShare = x.DoctorShare,
                GrossAmount = x.Quantity * x.UnitPrice,
                Status = x.Status
            }).ToList();
        return new InvoiceDetailResponse
        {
            Id = invoice.Id,
            EncounterId = invoice.EncounterId,
            InvoiceNumber = invoice.InvoiceNumber,
            ServiceType = invoice.ServiceType,
            Status = invoice.Status,
            CurrentCalculationVersion = invoice.CurrentCalculationVersion,
            RunningGrossAmount = activeItems.Sum(x => x.GrossAmount),
            ActiveItemCount = activeItems.Count,
            CreateDateTime = invoice.CreateDateTime,
            RowVersion = invoice.RowVersion,
            InvoiceDate = invoice.InvoiceDate,
            ClosedAt = invoice.ClosedAt,
            IsReplay = isReplay,
            Items = activeItems
        };
    }
}

public sealed class BillingInvoiceValidationException(string message) : Exception(message);
public sealed class BillingInvoiceConflictException : Exception
{
    public BillingInvoiceConflictException(string message) : base(message) { }
    public BillingInvoiceConflictException(string message, Exception innerException) : base(message, innerException) { }
}
