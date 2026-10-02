using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;

/// <summary>
/// Pantauan kejadian Accounting, baca saja (BE-FIN-012, FR-FIN-074). Membaca
/// FinAccountingEventOutbox/FinAccountingEventAttempt yang ditulis FinanceAccountingOutboxService
/// (BE-FIN-011) — TIDAK ADA method yang mengubah data di kelas ini. Endpoint pengirim/retry
/// sengaja tidak ada di sini; itu wewenang EPIC FIN-12 yang menunggu endpoint penerima Accounting
/// (FIN-CAP-018, erd/accounting-integration.md §6).
/// </summary>
public sealed class FinanceAccountingEventService
{
    private readonly ApplicationDbContext _dbContext;
    public FinanceAccountingEventService(ApplicationDbContext dbContext) => _dbContext = dbContext;

    public async Task<PagedResult<AccountingEventResponse>> GetPagedAsync(AccountingEventQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.FinAccountingEventOutboxes.AsNoTracking().Where(x => !x.IsDelete);

        if (!string.IsNullOrWhiteSpace(request.DeliveryStatus)) query = query.Where(x => x.DeliveryStatus == request.DeliveryStatus);
        if (!string.IsNullOrWhiteSpace(request.EventTypeCode)) query = query.Where(x => x.EventTypeCode == request.EventTypeCode);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(x => EF.Functions.ILike(x.EventNumber, $"%{term}%") || EF.Functions.ILike(x.SourceTransactionId, $"%{term}%"));
        }

        var descending = !string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);
        query = request.SortBy.Trim().ToLowerInvariant() switch
        {
            "deliverystatus" => descending ? query.OrderByDescending(x => x.DeliveryStatus) : query.OrderBy(x => x.DeliveryStatus),
            "amount" => descending ? query.OrderByDescending(x => x.Amount) : query.OrderBy(x => x.Amount),
            "createdatetime" => descending ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
            _ => descending ? query.OrderByDescending(x => x.EventOccurredAt) : query.OrderBy(x => x.EventOccurredAt)
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
            .Select(x => MapListItem(x)).ToListAsync(cancellationToken);
        return new PagedResult<AccountingEventResponse>
        {
            PageNumber = request.PageNumber, PageSize = request.PageSize, TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)request.PageSize), Items = items
        };
    }

    public async Task<AccountingEventDetailResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var outbox = await _dbContext.FinAccountingEventOutboxes.AsNoTracking()
            .Include(x => x.Attempts)
            .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Kejadian tidak ditemukan.");
        return MapDetail(outbox);
    }

    public async Task<AccountingEventSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var counts = await _dbContext.FinAccountingEventOutboxes.AsNoTracking().Where(x => !x.IsDelete)
            .GroupBy(x => x.DeliveryStatus).Select(g => new { Status = g.Key, Count = g.Count() }).ToListAsync(cancellationToken);
        int Count(string status) => counts.FirstOrDefault(c => c.Status == status)?.Count ?? 0;

        return new AccountingEventSummaryResponse
        {
            TotalEvents = counts.Sum(c => c.Count),
            PendingCount = Count(FinAccountingEventDeliveryStatuses.Pending),
            HeldForFinalizationCount = Count(FinAccountingEventDeliveryStatuses.HeldForFinalization),
            SentCount = Count(FinAccountingEventDeliveryStatuses.Sent),
            AcknowledgedCount = Count(FinAccountingEventDeliveryStatuses.Acknowledged),
            HeldCount = Count(FinAccountingEventDeliveryStatuses.Held),
            FailedCount = Count(FinAccountingEventDeliveryStatuses.Failed)
        };
    }

    public Task<AccountingEventFilterMetadataResponse> GetFilterMetadataAsync(CancellationToken cancellationToken) =>
        Task.FromResult(new AccountingEventFilterMetadataResponse
        {
            PageSizeOptions = [10, 25, 50, 100],
            SortableFields = ["eventOccurredAt", "deliveryStatus", "amount", "createDateTime"],
            DeliveryStatusOptions =
            [
                FinAccountingEventDeliveryStatuses.Pending, FinAccountingEventDeliveryStatuses.HeldForFinalization,
                FinAccountingEventDeliveryStatuses.Sent, FinAccountingEventDeliveryStatuses.Acknowledged,
                FinAccountingEventDeliveryStatuses.Held, FinAccountingEventDeliveryStatuses.Failed
            ],
            // FIN-DEC-002 — katalog diusulkan Finance, belum diratifikasi Accounting (BE-FIN-010 bagian 1.1).
            EventTypeCodeOptions =
            [
                FinAccountingEventTypeCodes.PengakuanPiutang, FinAccountingEventTypeCodes.PenerimaanPiutang,
                FinAccountingEventTypeCodes.PenyesuaianPiutang, FinAccountingEventTypeCodes.PemutihanPiutang,
                FinAccountingEventTypeCodes.PengakuanHutangSupplier, FinAccountingEventTypeCodes.PembayaranHutangSupplier,
                FinAccountingEventTypeCodes.PengakuanHutangDokter, FinAccountingEventTypeCodes.PembayaranHutangDokter,
                FinAccountingEventTypeCodes.PenyesuaianHutang, FinAccountingEventTypeCodes.SetoranBank,
                FinAccountingEventTypeCodes.PettyCashTopUp, FinAccountingEventTypeCodes.PettyCashDisbursement,
                FinAccountingEventTypeCodes.PettyCashReturn, FinAccountingEventTypeCodes.PettyCashReversal,
                FinAccountingEventTypeCodes.PettyCashAdjustment, FinAccountingEventTypeCodes.PenerimaanKasir,
                FinAccountingEventTypeCodes.PembalikanPenerimaanKasir
            ]
        });

    private static AccountingEventResponse MapListItem(FinAccountingEventOutbox x) => new()
    {
        Id = x.Id,
        EventNumber = x.EventNumber,
        EventTypeCode = x.EventTypeCode,
        SourceModule = x.SourceModule,
        SourceTransactionId = x.SourceTransactionId,
        SourceVersion = x.SourceVersion,
        EventOccurredAt = x.EventOccurredAt,
        AccountingDate = x.AccountingDate,
        Amount = x.Amount,
        CurrencyCode = x.CurrencyCode,
        DeliveryStatus = x.DeliveryStatus,
        HoldReason = x.HoldReason,
        AttemptCount = x.AttemptCount,
        LastAttemptAt = x.LastAttemptAt,
        LastResponseCode = x.LastResponseCode,
        AccountingReceiptNumber = x.AccountingReceiptNumber,
        AccountingJournalNumber = x.AccountingJournalNumber,
        CorrelationId = x.CorrelationId,
        RowVersion = x.RowVersion,
        CreateDateTime = x.CreateDateTime
    };

    private static AccountingEventDetailResponse MapDetail(FinAccountingEventOutbox x)
    {
        var item = MapListItem(x);
        return new AccountingEventDetailResponse
        {
            Id = item.Id, EventNumber = item.EventNumber, EventTypeCode = item.EventTypeCode, SourceModule = item.SourceModule,
            SourceTransactionId = item.SourceTransactionId, SourceVersion = item.SourceVersion, EventOccurredAt = item.EventOccurredAt,
            AccountingDate = item.AccountingDate, Amount = item.Amount, CurrencyCode = item.CurrencyCode, DeliveryStatus = item.DeliveryStatus,
            HoldReason = item.HoldReason, AttemptCount = item.AttemptCount, LastAttemptAt = item.LastAttemptAt, LastResponseCode = item.LastResponseCode,
            AccountingReceiptNumber = item.AccountingReceiptNumber, AccountingJournalNumber = item.AccountingJournalNumber,
            CorrelationId = item.CorrelationId, RowVersion = item.RowVersion, CreateDateTime = item.CreateDateTime,
            LegalEntityId = x.LegalEntityId,
            CausationId = x.CausationId,
            ComponentsJson = x.ComponentsJson,
            PayloadJson = x.PayloadJson,
            Attempts = x.Attempts
                .OrderByDescending(a => a.AttemptNumber)
                .Select(a => new AccountingEventAttemptResponse
                {
                    Id = a.Id, AttemptNumber = a.AttemptNumber, AttemptedAt = a.AttemptedAt, ResponseCode = a.ResponseCode,
                    ResponseBody = a.ResponseBody, DurationMs = a.DurationMs, ErrorMessage = a.ErrorMessage
                }).ToList()
        };
    }
}
