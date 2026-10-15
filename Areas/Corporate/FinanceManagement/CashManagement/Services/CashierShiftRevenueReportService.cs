using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.CashManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.PettyCash.Models;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Globalization;
using System.Text;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.CashManagement.Services;

/// <summary>
/// Service laporan operasional pendapatan kasir per shift (read-only reporting).
/// Mengintegrasikan penerimaan tunai shift, transaksi non-tunai, tagihan/pembayaran asuransi,
/// dan transaksi Petty Cash yang dilayani petugas kasir.
/// </summary>
public sealed class CashierShiftRevenueReportService
{
    private const string LogCategory = "Corporate.FinanceManagement.Reports.CashierShifts";
    private static readonly CultureInfo IdCulture = new("id-ID");

    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;

    public CashierShiftRevenueReportService(
        ApplicationDbContext dbContext,
        LoggerService loggerService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
    }

    // ====================================================================================
    // 1. FILTER DROPDOWNS (SEARCHABLE & SERVER-SIDE)
    // ====================================================================================

    /// <summary>
    /// Mengambil daftar kasir yang dapat dicari berdasarkan nama atau kode kasir.
    /// </summary>
    public async Task<PagedResult<FilterOptionResponse>> GetCashierOptionsAsync(
        CashierFilterQuery query, CancellationToken cancellationToken)
    {
        var usersQuery = _dbContext.Users.AsNoTracking().Where(u => u.IsActive);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            usersQuery = usersQuery.Where(u =>
                u.UserCode.ToLower().Contains(search) ||
                u.DisplayName.ToLower().Contains(search) ||
                (u.UserName != null && u.UserName.ToLower().Contains(search)));
        }

        var total = await usersQuery.CountAsync(cancellationToken);
        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var users = await usersQuery
            .OrderBy(u => u.DisplayName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new
            {
                u.Id,
                u.DisplayName,
                u.UserCode,
                u.UserName
            })
            .ToListAsync(cancellationToken);

        var items = users.Select(u => new FilterOptionResponse
        {
            Value = u.Id.ToString(),
            Label = !string.IsNullOrWhiteSpace(u.DisplayName) ? u.DisplayName : (u.UserName ?? u.UserCode),
            CashierCode = u.UserCode,
            SubLabel = !string.IsNullOrWhiteSpace(u.UserCode) ? $"Kode: {u.UserCode}" : null
        }).ToList();

        return new PagedResult<FilterOptionResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)pageSize),
            Items = items
        };
    }

    /// <summary>
    /// Mengambil daftar lokasi register kasir yang dapat dicari.
    /// </summary>
    public async Task<PagedResult<FilterOptionResponse>> GetLocationOptionsAsync(
        LocationFilterQuery query, CancellationToken cancellationToken)
    {
        var registersQuery = _dbContext.MstRegisters.AsNoTracking()
            .Where(r => !r.IsDelete && r.IsActive && !string.IsNullOrWhiteSpace(r.Location));

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            registersQuery = registersQuery.Where(r => r.Location!.ToLower().Contains(search));
        }

        var distinctLocations = registersQuery
            .Select(r => r.Location!)
            .Distinct();

        var total = await distinctLocations.CountAsync(cancellationToken);
        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var locations = await distinctLocations
            .OrderBy(loc => loc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = locations.Select(loc => new FilterOptionResponse
        {
            Value = loc,
            Label = loc,
            SubLabel = "Lokasi Register"
        }).ToList();

        return new PagedResult<FilterOptionResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)pageSize),
            Items = items
        };
    }

    /// <summary>
    /// Mengambil daftar shift kasir yang dapat dicari dan difilter berdasarkan tanggal, lokasi, atau kasir.
    /// </summary>
    public async Task<PagedResult<FilterOptionResponse>> GetShiftOptionsAsync(
        ShiftFilterQuery query, CancellationToken cancellationToken)
    {
        var shiftsQuery = _dbContext.BilCashierShifts.AsNoTracking().Where(s => !s.IsDelete);

        if (query.CashierId.HasValue)
        {
            shiftsQuery = shiftsQuery.Where(s => s.CashierId == query.CashierId.Value);
        }

        if (query.DateFrom.HasValue)
        {
            var startUtc = FinanceBusinessDate.GetStartOfDayUtc(query.DateFrom.Value);
            shiftsQuery = shiftsQuery.Where(s => s.OpenedAt >= startUtc);
        }

        if (query.DateTo.HasValue)
        {
            var endUtc = FinanceBusinessDate.GetEndOfDayUtc(query.DateTo.Value);
            shiftsQuery = shiftsQuery.Where(s => s.OpenedAt < endUtc);
        }

        if (!string.IsNullOrWhiteSpace(query.Location))
        {
            var registerIds = _dbContext.MstRegisters.AsNoTracking()
                .Where(r => !r.IsDelete && r.Location == query.Location)
                .Select(r => r.Id);
            shiftsQuery = shiftsQuery.Where(s => registerIds.Contains(s.RegisterId));
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            shiftsQuery = shiftsQuery.Where(s => s.ShiftNumber.ToLower().Contains(search));
        }

        var total = await shiftsQuery.CountAsync(cancellationToken);
        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var shifts = await shiftsQuery
            .OrderByDescending(s => s.OpenedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new
            {
                s.Id,
                s.ShiftNumber,
                s.OpenedAt,
                s.Status
            })
            .ToListAsync(cancellationToken);

        var items = shifts.Select(s => new FilterOptionResponse
        {
            Value = s.Id.ToString(),
            Label = s.ShiftNumber,
            SubLabel = $"{FinanceBusinessDate.ToBusinessTime(s.OpenedAt):dd/MM/yyyy HH:mm} ({s.Status})"
        }).ToList();

        return new PagedResult<FilterOptionResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)pageSize),
            Items = items
        };
    }

    // ====================================================================================
    // 2. LAPORAN UTAMA (SUMMARY, PAGED LIST, & CHARTS)
    // ====================================================================================

    /// <summary>
    /// Mengambil laporan terintegrasi: ringkasan finansial, daftar shift terpaginasi, dan data grafik.
    /// </summary>
    public async Task<CashierShiftRevenueReportResponse> GetReportAsync(
        CashierShiftRevenueReportQuery query, CancellationToken cancellationToken)
    {
        var shiftsQuery = BuildFilteredShiftsQuery(query);

        // --- A. SUMMARY (Dihitung dari SELURUH data hasil filter) ---
        var totalShifts = await shiftsQuery.CountAsync(cancellationToken);
        var totalOpeningCash = await shiftsQuery.SumAsync(s => (decimal?)s.OpeningCash, cancellationToken) ?? 0m;
        var totalCashReceived = await shiftsQuery.SumAsync(s => (decimal?)s.SystemCash, cancellationToken) ?? 0m;
        var totalExpectedCash = checked(totalOpeningCash + totalCashReceived);

        // Hanya hitung PhysicalCash dan Variance dari shift yang sudah selesai/dihitung
        var closedShiftsQuery = shiftsQuery.Where(s =>
            s.Status != CashierShiftStatuses.Open && s.Status != CashierShiftStatuses.Reopened);

        var totalPhysicalCash = await closedShiftsQuery.SumAsync(s => (decimal?)s.PhysicalCash, cancellationToken);
        var totalCashVariance = await closedShiftsQuery.SumAsync(s => (decimal?)s.Variance, cancellationToken);

        // Non-tunai dan komposisi metode pembayaran
        var allFilteredShiftIdsQuery = shiftsQuery.Select(s => s.Id);

        var succeededTenders = _dbContext.BilTenders.AsNoTracking()
            .Where(t => !t.IsDelete && t.Status == BillingTenderStatuses.Succeeded && t.CashierShiftId.HasValue
                && allFilteredShiftIdsQuery.Contains(t.CashierShiftId.Value));

        var tendersWithMethods = await succeededTenders
            .Join(_dbContext.MstPaymentMethods.AsNoTracking(),
                t => t.PaymentMethodId,
                pm => pm.Id,
                (t, pm) => new
                {
                    t.Amount,
                    pm.PaymentMethodType,
                    pm.PaymentMethodName,
                    pm.IsCash
                })
            .ToListAsync(cancellationToken);

        var totalNonCashReceived = tendersWithMethods
            .Where(x => !x.IsCash)
            .Sum(x => x.Amount);

        // Transaksi Petty Cash dalam rentang shift yang difilter
        var cashierIdsInReport = await shiftsQuery.Select(s => s.CashierId).Distinct().ToListAsync(cancellationToken);
        var minOpenedAt = await shiftsQuery.MinAsync(s => (DateTimeOffset?)s.OpenedAt, cancellationToken);
        var maxClosedAt = await shiftsQuery.MaxAsync(s => s.ClosedAt ?? DateTimeOffset.UtcNow, cancellationToken);

        var pettyCashQuery = _dbContext.BilPettyCashVouchers.AsNoTracking()
            .Where(v => !v.IsDelete && !v.IsCancel && v.DisbursedAt.HasValue && v.DisbursedBy.HasValue
                && (v.Status == PettyCashVoucherStatuses.CashReceived || v.Status == PettyCashVoucherStatuses.Completed));

        if (cashierIdsInReport.Count > 0 && minOpenedAt.HasValue)
        {
            pettyCashQuery = pettyCashQuery.Where(v =>
                cashierIdsInReport.Contains(v.DisbursedBy!.Value) &&
                v.DisbursedAt >= minOpenedAt.Value &&
                v.DisbursedAt <= maxClosedAt);
        }
        else
        {
            pettyCashQuery = pettyCashQuery.Where(v => false);
        }

        var matchedVouchers = await pettyCashQuery
            .Select(v => new { v.Amount, v.ReturnedAmount })
            .ToListAsync(cancellationToken);

        var totalPettyCashDisbursed = matchedVouchers.Sum(v => v.Amount);
        var totalPettyCashReturned = matchedVouchers.Sum(v => v.ReturnedAmount);
        var totalPettyCashNet = totalPettyCashDisbursed - totalPettyCashReturned;

        // Tagihan Asuransi (A/R) & Pembayaran Asuransi Diterima
        var insuranceArQuery = _dbContext.FinReceivables.AsNoTracking()
            .Where(r => !r.IsDelete && (r.DebtorType == FinReceivableDebtorTypes.Payer || r.DebtorType == FinReceivableDebtorTypes.PatientGuarantor));

        if (query.DateFrom.HasValue)
        {
            var startUtc = FinanceBusinessDate.GetStartOfDayUtc(query.DateFrom.Value);
            insuranceArQuery = insuranceArQuery.Where(r => r.RecognizedAt >= startUtc);
        }
        if (query.DateTo.HasValue)
        {
            var endUtc = FinanceBusinessDate.GetEndOfDayUtc(query.DateTo.Value);
            insuranceArQuery = insuranceArQuery.Where(r => r.RecognizedAt < endUtc);
        }

        var insuranceArAmount = await insuranceArQuery.SumAsync(r => (decimal?)r.OriginalAmount, cancellationToken) ?? 0m;

        // Pembayaran Asuransi Diterima
        var insuranceReceiptsQuery = _dbContext.FinReceipts.AsNoTracking()
            .Where(rec => !rec.IsDelete && rec.SourceType == FinReceiptSourceTypes.ArCollection && rec.Status != FinReceiptStatuses.Reversed);

        if (query.DateFrom.HasValue)
        {
            var startUtc = FinanceBusinessDate.GetStartOfDayUtc(query.DateFrom.Value);
            insuranceReceiptsQuery = insuranceReceiptsQuery.Where(rec => rec.OccurredAt >= startUtc);
        }
        if (query.DateTo.HasValue)
        {
            var endUtc = FinanceBusinessDate.GetEndOfDayUtc(query.DateTo.Value);
            insuranceReceiptsQuery = insuranceReceiptsQuery.Where(rec => rec.OccurredAt < endUtc);
        }

        var insuranceCollectedAmount = await insuranceReceiptsQuery.SumAsync(rec => (decimal?)rec.Amount, cancellationToken) ?? 0m;
        var unattributedInsuranceAmount = await insuranceReceiptsQuery
            .Where(rec => rec.CashierShiftId == null)
            .SumAsync(rec => (decimal?)rec.Amount, cancellationToken) ?? 0m;

        var summary = new CashierShiftRevenueSummaryDto
        {
            TotalShifts = totalShifts,
            TotalOpeningCash = totalOpeningCash,
            TotalCashReceived = totalCashReceived,
            TotalExpectedCash = totalExpectedCash,
            TotalPhysicalCash = totalPhysicalCash,
            TotalCashVariance = totalCashVariance,
            InsuranceArAmount = insuranceArAmount,
            InsuranceCollectedAmount = insuranceCollectedAmount,
            UnattributedInsuranceAmount = unattributedInsuranceAmount,
            TotalPettyCashDisbursed = totalPettyCashDisbursed,
            TotalPettyCashReturned = totalPettyCashReturned,
            TotalPettyCashNet = totalPettyCashNet,
            TotalNonCashReceived = totalNonCashReceived
        };

        // --- B. PAGED SHIFT ROWS ---
        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var pagedShifts = await shiftsQuery
            .OrderByDescending(s => s.OpenedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var pageShiftIds = pagedShifts.Select(s => s.Id).ToList();
        var registerIdsOnPage = pagedShifts.Select(s => s.RegisterId).Distinct().ToList();
        var cashierIdsOnPage = pagedShifts.Select(s => s.CashierId).Distinct().ToList();

        // Ambil data referensi pendukung
        var registersMap = await _dbContext.MstRegisters.AsNoTracking()
            .Where(r => registerIdsOnPage.Contains(r.Id))
            .ToDictionaryAsync(r => r.Id, r => new { r.RegisterName, Location = r.Location ?? "-" }, cancellationToken);

        var usersMap = await _dbContext.Users.AsNoTracking()
            .Where(u => cashierIdsOnPage.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => !string.IsNullOrWhiteSpace(u.DisplayName) ? u.DisplayName : (u.UserName ?? "Kasir"), cancellationToken);

        // Ambil non-tunai per shift pada halaman ini
        var pageNonCashMap = await _dbContext.BilTenders.AsNoTracking()
            .Where(t => !t.IsDelete && t.Status == BillingTenderStatuses.Succeeded
                && t.CashierShiftId.HasValue && pageShiftIds.Contains(t.CashierShiftId.Value))
            .Join(_dbContext.MstPaymentMethods.AsNoTracking(),
                t => t.PaymentMethodId,
                pm => pm.Id,
                (t, pm) => new { t.CashierShiftId, t.Amount, pm.IsCash })
            .Where(x => !x.IsCash)
            .GroupBy(x => x.CashierShiftId!.Value)
            .Select(g => new { ShiftId = g.Key, NonCashTotal = g.Sum(x => x.Amount) })
            .ToDictionaryAsync(x => x.ShiftId, x => x.NonCashTotal, cancellationToken);

        // Ambil asuransi teratribusi per shift pada halaman ini
        var pageInsuranceMap = await _dbContext.FinReceipts.AsNoTracking()
            .Where(r => !r.IsDelete && r.SourceType == FinReceiptSourceTypes.ArCollection
                && r.Status != FinReceiptStatuses.Reversed && r.CashierShiftId.HasValue
                && pageShiftIds.Contains(r.CashierShiftId.Value))
            .GroupBy(r => r.CashierShiftId!.Value)
            .Select(g => new { ShiftId = g.Key, AttributedTotal = g.Sum(r => r.Amount) })
            .ToDictionaryAsync(x => x.ShiftId, x => x.AttributedTotal, cancellationToken);

        // Petty Cash matching per shift pada halaman ini
        var pageVouchers = await _dbContext.BilPettyCashVouchers.AsNoTracking()
            .Where(v => !v.IsDelete && !v.IsCancel && v.DisbursedAt.HasValue && v.DisbursedBy.HasValue
                && cashierIdsOnPage.Contains(v.DisbursedBy.Value)
                && (v.Status == PettyCashVoucherStatuses.CashReceived || v.Status == PettyCashVoucherStatuses.Completed))
            .Select(v => new
            {
                v.Id,
                v.Amount,
                v.ReturnedAmount,
                DisbursedBy = v.DisbursedBy!.Value,
                DisbursedAt = v.DisbursedAt!.Value
            })
            .ToListAsync(cancellationToken);

        var shiftRows = new List<CashierShiftRevenueItemDto>();
        foreach (var shift in pagedShifts)
        {
            var reg = registersMap.GetValueOrDefault(shift.RegisterId);
            var cashierName = usersMap.GetValueOrDefault(shift.CashierId, "Kasir");
            var nonCash = pageNonCashMap.GetValueOrDefault(shift.Id, 0m);
            var insuranceAmount = pageInsuranceMap.ContainsKey(shift.Id) ? (decimal?)pageInsuranceMap[shift.Id] : null;

            // Match petty cash vouchers for this shift
            var shiftEnd = shift.ClosedAt ?? DateTimeOffset.UtcNow;
            var matchedForThisShift = pageVouchers
                .Where(v => v.DisbursedBy == shift.CashierId && v.DisbursedAt >= shift.OpenedAt && v.DisbursedAt <= shiftEnd)
                .ToList();

            var pcCount = matchedForThisShift.Count;
            var pcNet = matchedForThisShift.Sum(v => v.Amount - v.ReturnedAmount);
            var pcStatus = pcCount > 0 ? "MATCHED" : "NONE";

            var isOpen = shift.Status == CashierShiftStatuses.Open || shift.Status == CashierShiftStatuses.Reopened;

            shiftRows.Add(new CashierShiftRevenueItemDto
            {
                ShiftId = shift.Id,
                ShiftNumber = shift.ShiftNumber,
                RegisterId = shift.RegisterId,
                RegisterName = reg?.RegisterName ?? "-",
                Location = reg?.Location ?? "-",
                CashierId = shift.CashierId,
                CashierName = cashierName,
                OpenedAt = shift.OpenedAt,
                ClosedAt = shift.ClosedAt,
                Status = shift.Status,
                OpeningCash = shift.OpeningCash,
                CashReceived = shift.SystemCash,
                ExpectedCash = checked(shift.OpeningCash + shift.SystemCash),
                PhysicalCash = isOpen ? null : shift.PhysicalCash,
                Variance = isOpen ? null : shift.Variance,
                NonCashReceived = nonCash,
                InsuranceAttributedAmount = insuranceAmount,
                PettyCashMatchedCount = pcCount,
                PettyCashMatchedNet = pcNet,
                PettyCashAttributionStatus = pcStatus
            });
        }

        // --- C. CHARTS DATA ---
        var shiftRevenues = shiftRows
            .Take(10)
            .Select(s => new ShiftRevenueChartItemDto
            {
                ShiftNumber = s.ShiftNumber,
                CashierName = s.CashierName,
                CashReceived = s.CashReceived,
                NonCashReceived = s.NonCashReceived,
                TotalRevenue = s.CashReceived + s.NonCashReceived
            })
            .ToList();

        // Komposisi metode pembayaran
        var methodGroups = tendersWithMethods
            .GroupBy(t => t.PaymentMethodType)
            .Select(g => new
            {
                Type = g.Key,
                Amount = g.Sum(x => x.Amount)
            })
            .ToList();

        var totalRevenueAllMethods = totalCashReceived + totalNonCashReceived + insuranceCollectedAmount;
        var paymentComposition = new List<PaymentMethodCompositionDto>();

        if (totalRevenueAllMethods > 0)
        {
            // Cash
            if (totalCashReceived > 0)
            {
                paymentComposition.Add(new PaymentMethodCompositionDto
                {
                    MethodType = "Cash",
                    Label = "Tunai (Cash)",
                    Amount = totalCashReceived,
                    Percentage = Math.Round((double)(totalCashReceived / totalRevenueAllMethods * 100m), 2)
                });
            }

            // Non-cash groups
            foreach (var mg in methodGroups.Where(x => !string.Equals(x.Type, "Cash", StringComparison.OrdinalIgnoreCase)))
            {
                var label = mg.Type switch
                {
                    "Debit" => "Kartu Debit",
                    "CreditCard" => "Kartu Kredit",
                    "Transfer" => "Transfer Bank",
                    "QRIS" => "QRIS",
                    _ => mg.Type
                };

                paymentComposition.Add(new PaymentMethodCompositionDto
                {
                    MethodType = mg.Type,
                    Label = label,
                    Amount = mg.Amount,
                    Percentage = Math.Round((double)(mg.Amount / totalRevenueAllMethods * 100m), 2)
                });
            }

            // Insurance Collected
            if (insuranceCollectedAmount > 0)
            {
                paymentComposition.Add(new PaymentMethodCompositionDto
                {
                    MethodType = "Insurance",
                    Label = "Asuransi Terbayar",
                    Amount = insuranceCollectedAmount,
                    Percentage = Math.Round((double)(insuranceCollectedAmount / totalRevenueAllMethods * 100m), 2)
                });
            }
        }

        return new CashierShiftRevenueReportResponse
        {
            Summary = summary,
            Shifts = new PagedResult<CashierShiftRevenueItemDto>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalShifts,
                TotalPage = (int)Math.Ceiling(totalShifts / (double)pageSize),
                Items = shiftRows
            },
            Charts = new CashierShiftChartsDto
            {
                ShiftRevenues = shiftRevenues,
                PaymentComposition = paymentComposition
            }
        };
    }

    // ====================================================================================
    // 3. DETAIL TRANSAKSI SHIFT (GET /{shiftId}/transactions)
    // ====================================================================================

    /// <summary>
    /// Mengambil rincian transaksi kasir (pembayaran invoice & deposit) per shift.
    /// </summary>
    public async Task<PagedResult<ShiftTransactionItemDto>> GetShiftTransactionsAsync(
        Guid shiftId, ShiftTransactionQuery query, CancellationToken cancellationToken)
    {
        var shift = await _dbContext.BilCashierShifts.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == shiftId && !s.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Shift kasir tidak ditemukan.");

        var tendersQuery = _dbContext.BilTenders.AsNoTracking()
            .Include(t => t.Settlement)
            .Where(t => !t.IsDelete && t.CashierShiftId == shiftId);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            tendersQuery = tendersQuery.Where(t =>
                (t.KwitansiNumber != null && t.KwitansiNumber.ToLower().Contains(search)) ||
                (t.ProviderReference != null && t.ProviderReference.ToLower().Contains(search)) ||
                (t.CashierReferenceNote != null && t.CashierReferenceNote.ToLower().Contains(search)));
        }

        var total = await tendersQuery.CountAsync(cancellationToken);
        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var tenders = await tendersQuery
            .OrderByDescending(t => t.AttemptedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Join(_dbContext.MstPaymentMethods.AsNoTracking(),
                t => t.PaymentMethodId,
                pm => pm.Id,
                (t, pm) => new
                {
                    t.Id,
                    t.Amount,
                    t.Status,
                    t.AttemptedAt,
                    t.SettledAt,
                    t.KwitansiNumber,
                    t.ProviderReference,
                    t.CashierReferenceNote,
                    IsDeposit = t.Settlement != null && t.Settlement.DepositAccountId.HasValue,
                    pm.PaymentMethodName,
                    pm.PaymentMethodType,
                    pm.IsCash
                })
            .ToListAsync(cancellationToken);

        var items = tenders.Select(t => new ShiftTransactionItemDto
        {
            Id = t.Id,
            OccurredAt = t.SettledAt ?? t.AttemptedAt,
            ReferenceNumber = t.KwitansiNumber ?? t.ProviderReference ?? t.Id.ToString()[..8],
            Category = t.IsDeposit ? "Top-Up Deposit" : "Pembayaran Tagihan",
            PaymentMethodName = t.PaymentMethodName,
            PaymentMethodType = t.PaymentMethodType,
            IsCash = t.IsCash,
            Amount = t.Amount,
            Status = t.Status,
            Notes = t.CashierReferenceNote
        }).ToList();

        return new PagedResult<ShiftTransactionItemDto>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)pageSize),
            Items = items
        };
    }

    // ====================================================================================
    // 4. DETAIL PETTY CASH PER SHIFT (GET /{shiftId}/petty-cash)
    // ====================================================================================

    /// <summary>
    /// Mengambil transaksi Petty Cash yang dilayani petugas kasir pada interval waktu shift.
    /// </summary>
    public async Task<PagedResult<ShiftPettyCashItemDto>> GetShiftPettyCashAsync(
        Guid shiftId, ShiftPettyCashQuery query, CancellationToken cancellationToken)
    {
        var shift = await _dbContext.BilCashierShifts.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == shiftId && !s.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Shift kasir tidak ditemukan.");

        var shiftEnd = shift.ClosedAt ?? DateTimeOffset.UtcNow;

        var vouchersQuery = _dbContext.BilPettyCashVouchers.AsNoTracking()
            .Include(v => v.Category)
            .Where(v => !v.IsDelete && !v.IsCancel && v.DisbursedAt.HasValue && v.DisbursedBy.HasValue
                && v.DisbursedBy == shift.CashierId
                && v.DisbursedAt >= shift.OpenedAt && v.DisbursedAt <= shiftEnd
                && (v.Status == PettyCashVoucherStatuses.CashReceived || v.Status == PettyCashVoucherStatuses.Completed));

        var total = await vouchersQuery.CountAsync(cancellationToken);
        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var vouchers = await vouchersQuery
            .OrderByDescending(v => v.DisbursedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // Ambil nama user kasir
        var cashier = await _dbContext.Users.AsNoTracking()
            .Where(u => u.Id == shift.CashierId)
            .Select(u => !string.IsNullOrWhiteSpace(u.DisplayName) ? u.DisplayName : (u.UserName ?? "Kasir"))
            .FirstOrDefaultAsync(cancellationToken) ?? "Kasir";

        var items = vouchers.Select(v => new ShiftPettyCashItemDto
        {
            Id = v.Id,
            VoucherNumber = v.VoucherNumber,
            DisbursedAt = v.DisbursedAt,
            DisbursedByName = cashier,
            ConfirmedByName = v.ConfirmedByName,
            RecipientName = v.RecipientName,
            CategoryName = v.Category?.CategoryName ?? "-",
            Purpose = v.Purpose,
            Amount = v.Amount,
            ReturnedAmount = v.ReturnedAmount,
            NetAmount = v.Amount - v.ReturnedAmount,
            Status = v.Status,
            AttributionStatus = "Terkait Petugas & Waktu Shift"
        }).ToList();

        return new PagedResult<ShiftPettyCashItemDto>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)pageSize),
            Items = items
        };
    }

    // ====================================================================================
    // 5. LAPORAN ASURANSI (GET /insurance)
    // ====================================================================================

    /// <summary>
    /// Mengambil tagihan A/R asuransi dan penerimaan pelunasan asuransi dengan status atribusi ke shift.
    /// </summary>
    public async Task<PagedResult<InsuranceReportItemDto>> GetInsuranceReportAsync(
        InsuranceReportQuery query, CancellationToken cancellationToken)
    {
        var receivablesQuery = _dbContext.FinReceivables.AsNoTracking()
            .Where(r => !r.IsDelete && (r.DebtorType == FinReceivableDebtorTypes.Payer || r.DebtorType == FinReceivableDebtorTypes.PatientGuarantor));

        if (query.DateFrom.HasValue)
        {
            var startUtc = FinanceBusinessDate.GetStartOfDayUtc(query.DateFrom.Value);
            receivablesQuery = receivablesQuery.Where(r => r.RecognizedAt >= startUtc);
        }
        if (query.DateTo.HasValue)
        {
            var endUtc = FinanceBusinessDate.GetEndOfDayUtc(query.DateTo.Value);
            receivablesQuery = receivablesQuery.Where(r => r.RecognizedAt < endUtc);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            receivablesQuery = receivablesQuery.Where(r => r.Status == query.Status);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            receivablesQuery = receivablesQuery.Where(r =>
                r.ReceivableNumber.ToLower().Contains(search) ||
                (r.BenefitRelationship != null && r.BenefitRelationship.ToLower().Contains(search)));
        }

        var total = await receivablesQuery.CountAsync(cancellationToken);
        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var receivables = await receivablesQuery
            .OrderByDescending(r => r.RecognizedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = receivables.Select(r => new InsuranceReportItemDto
        {
            Id = r.Id,
            PayerName = r.DebtorType,
            ReferenceNumber = r.ReceivableNumber,
            InvoiceNumber = r.InvoiceId?.ToString()[..8],
            ArAmount = r.OriginalAmount,
            CollectedAmount = r.AllocatedAmount,
            OutstandingAmount = r.OutstandingAmount,
            TransactionDate = r.RecognizedAt,
            Status = r.Status,
            AttributionStatus = "BELUM_TERATRIBUSI",
            ShiftNumber = null
        }).ToList();

        return new PagedResult<InsuranceReportItemDto>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)pageSize),
            Items = items
        };
    }

    // ====================================================================================
    // 6. EXPORT LAPORAN (EXCEL & CSV)
    // ====================================================================================

    /// <summary>
    /// Mengekspor seluruh data hasil filter ke format Excel (.xlsx) atau CSV.
    /// </summary>
    public async Task<(byte[] Bytes, string ContentType, string FileName)> ExportAsync(
        CashierShiftExportQuery query, CancellationToken cancellationToken)
    {
        var shiftsQuery = BuildFilteredShiftsQuery(new CashierShiftRevenueReportQuery
        {
            DateFrom = query.DateFrom,
            DateTo = query.DateTo,
            CashierId = query.CashierId,
            Location = query.Location,
            ShiftId = query.ShiftId,
            PaymentMethod = query.PaymentMethod,
            Status = query.Status,
            PageNumber = 1,
            PageSize = int.MaxValue
        });

        var allShifts = await shiftsQuery
            .OrderByDescending(s => s.OpenedAt)
            .ToListAsync(cancellationToken);

        var registerIds = allShifts.Select(s => s.RegisterId).Distinct().ToList();
        var cashierIds = allShifts.Select(s => s.CashierId).Distinct().ToList();
        var allShiftIds = allShifts.Select(s => s.Id).ToList();

        var registersMap = await _dbContext.MstRegisters.AsNoTracking()
            .Where(r => registerIds.Contains(r.Id))
            .ToDictionaryAsync(r => r.Id, r => new { r.RegisterName, Location = r.Location ?? "-" }, cancellationToken);

        var usersMap = await _dbContext.Users.AsNoTracking()
            .Where(u => cashierIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => !string.IsNullOrWhiteSpace(u.DisplayName) ? u.DisplayName : (u.UserName ?? "Kasir"), cancellationToken);

        var nonCashMap = await _dbContext.BilTenders.AsNoTracking()
            .Where(t => !t.IsDelete && t.Status == BillingTenderStatuses.Succeeded
                && t.CashierShiftId.HasValue && allShiftIds.Contains(t.CashierShiftId.Value))
            .Join(_dbContext.MstPaymentMethods.AsNoTracking(),
                t => t.PaymentMethodId,
                pm => pm.Id,
                (t, pm) => new { t.CashierShiftId, t.Amount, pm.IsCash })
            .Where(x => !x.IsCash)
            .GroupBy(x => x.CashierShiftId!.Value)
            .Select(g => new { ShiftId = g.Key, NonCashTotal = g.Sum(x => x.Amount) })
            .ToDictionaryAsync(x => x.ShiftId, x => x.NonCashTotal, cancellationToken);

        var dateFromStr = query.DateFrom?.ToString("yyyyMMdd") ?? "Semua";
        var dateToStr = query.DateTo?.ToString("yyyyMMdd") ?? "Semua";

        if (string.Equals(query.Format, "csv", StringComparison.OrdinalIgnoreCase))
        {
            var sb = new StringBuilder();
            sb.AppendLine("No,Nomor Shift,Lokasi Register,Nama Kasir,Waktu Buka (WIB),Waktu Tutup (WIB),Status,Kas Awal,Penerimaan Tunai,Total Kas Sistem,Kas Fisik,Selisih,Non-Tunai");

            for (int i = 0; i < allShifts.Count; i++)
            {
                var s = allShifts[i];
                var reg = registersMap.GetValueOrDefault(s.RegisterId);
                var cashier = usersMap.GetValueOrDefault(s.CashierId, "Kasir");
                var nonCash = nonCashMap.GetValueOrDefault(s.Id, 0m);
                var expected = s.OpeningCash + s.SystemCash;
                var isOpen = s.Status == CashierShiftStatuses.Open || s.Status == CashierShiftStatuses.Reopened;
                var physStr = isOpen ? "-" : s.PhysicalCash.ToString("F2", CultureInfo.InvariantCulture);
                var varStr = isOpen ? "-" : s.Variance.ToString("F2", CultureInfo.InvariantCulture);

                sb.AppendLine($"{i + 1},\"{s.ShiftNumber}\",\"{reg?.Location}\",\"{cashier}\",\"{FinanceBusinessDate.ToBusinessTime(s.OpenedAt):yyyy-MM-dd HH:mm:ss}\",\"{(s.ClosedAt.HasValue ? FinanceBusinessDate.ToBusinessTime(s.ClosedAt.Value).ToString("yyyy-MM-dd HH:mm:ss") : "-")}\",\"{s.Status}\",{s.OpeningCash:F2},{s.SystemCash:F2},{expected:F2},{physStr},{varStr},{nonCash:F2}");
            }

            var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
            return (csvBytes, "text/csv", $"Laporan_Pendapatan_Kasir_{dateFromStr}_{dateToStr}.csv");
        }

        // Default: Excel .xlsx via ClosedXML
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Laporan Pendapatan Kasir");

        // Palette Styling: Quilvian Teal #0d7f89
        var tealColor = XLColor.FromHtml("#0d7f89");
        var lightTeal = XLColor.FromHtml("#dff8fb");
        var grayBorder = XLColor.FromHtml("#e5e7eb");

        // Title Block
        ws.Cell(1, 1).Value = "LAPORAN PENDAPATAN KASIR PER SHIFT";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;
        ws.Cell(1, 1).Style.Font.FontColor = tealColor;

        ws.Cell(2, 1).Value = $"Periode: {(query.DateFrom.HasValue ? query.DateFrom.Value.ToString("dd MMMM yyyy", IdCulture) : "Awal")} s/d {(query.DateTo.HasValue ? query.DateTo.Value.ToString("dd MMMM yyyy", IdCulture) : "Sekarang")}";
        ws.Cell(2, 1).Style.Font.FontSize = 10;
        ws.Cell(2, 1).Style.Font.FontColor = XLColor.FromHtml("#4b5563");

        ws.Cell(3, 1).Value = $"Tanggal Cetak: {DateTime.Now.ToString("dd MMMM yyyy HH:mm:ss", IdCulture)} WIB";
        ws.Cell(3, 1).Style.Font.FontSize = 9;
        ws.Cell(3, 1).Style.Font.FontColor = XLColor.FromHtml("#6b7280");

        // Headers
        var headers = new[]
        {
            "No", "No. Shift", "Lokasi Register", "Nama Kasir", "Waktu Buka (WIB)",
            "Waktu Tutup (WIB)", "Status", "Kas Awal", "Penerimaan Tunai",
            "Total Kas Sistem", "Kas Fisik", "Selisih", "Non-Tunai"
        };

        var headerRow = 5;
        for (int c = 0; c < headers.Length; c++)
        {
            var cell = ws.Cell(headerRow, c + 1);
            cell.Value = headers[c];
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = tealColor;
            cell.Style.Fill.BackgroundColor = lightTeal;
            cell.Style.Alignment.Horizontal = (c == 0 || c == 4 || c == 5 || c == 6)
                ? XLAlignmentHorizontalValues.Center
                : (c >= 7 ? XLAlignmentHorizontalValues.Right : XLAlignmentHorizontalValues.Left);
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            cell.Style.Border.OutsideBorderColor = XLColor.FromHtml("#8adce4");
        }

        var currentRow = headerRow + 1;
        decimal totalOpening = 0m, totalCash = 0m, totalExpected = 0m, totalNonCash = 0m;
        decimal? totalPhysical = 0m, totalVariance = 0m;

        for (int i = 0; i < allShifts.Count; i++)
        {
            var s = allShifts[i];
            var reg = registersMap.GetValueOrDefault(s.RegisterId);
            var cashier = usersMap.GetValueOrDefault(s.CashierId, "Kasir");
            var nonCash = nonCashMap.GetValueOrDefault(s.Id, 0m);
            var expected = checked(s.OpeningCash + s.SystemCash);
            var isOpen = s.Status == CashierShiftStatuses.Open || s.Status == CashierShiftStatuses.Reopened;

            totalOpening += s.OpeningCash;
            totalCash += s.SystemCash;
            totalExpected += expected;
            totalNonCash += nonCash;
            if (!isOpen)
            {
                totalPhysical = (totalPhysical ?? 0m) + s.PhysicalCash;
                totalVariance = (totalVariance ?? 0m) + s.Variance;
            }

            ws.Cell(currentRow, 1).Value = i + 1;
            ws.Cell(currentRow, 2).Value = s.ShiftNumber;
            ws.Cell(currentRow, 3).Value = reg?.Location ?? "-";
            ws.Cell(currentRow, 4).Value = cashier;
            ws.Cell(currentRow, 5).Value = FinanceBusinessDate.ToBusinessTime(s.OpenedAt).ToString("dd/MM/yyyy HH:mm");
            ws.Cell(currentRow, 6).Value = s.ClosedAt.HasValue ? FinanceBusinessDate.ToBusinessTime(s.ClosedAt.Value).ToString("dd/MM/yyyy HH:mm") : "-";
            ws.Cell(currentRow, 7).Value = s.Status;
            ws.Cell(currentRow, 8).Value = s.OpeningCash;
            ws.Cell(currentRow, 9).Value = s.SystemCash;
            ws.Cell(currentRow, 10).Value = expected;

            if (isOpen)
            {
                ws.Cell(currentRow, 11).Value = "-";
                ws.Cell(currentRow, 12).Value = "-";
            }
            else
            {
                ws.Cell(currentRow, 11).Value = s.PhysicalCash;
                ws.Cell(currentRow, 12).Value = s.Variance;
            }

            ws.Cell(currentRow, 13).Value = nonCash;

            // Alignments & Number Formats
            ws.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(currentRow, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(currentRow, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(currentRow, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(currentRow, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            for (int col = 8; col <= 13; col++)
            {
                var valCell = ws.Cell(currentRow, col);
                valCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                if (valCell.DataType == XLDataType.Number)
                {
                    valCell.Style.NumberFormat.Format = "#,##0";
                }
            }

            for (int c = 1; c <= headers.Length; c++)
            {
                var cell = ws.Cell(currentRow, c);
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                cell.Style.Border.OutsideBorderColor = grayBorder;
            }

            currentRow++;
        }

        // Summary Total Row
        ws.Cell(currentRow, 1).Value = "TOTAL KESELURUHAN";
        var totalLabelRange = ws.Range(currentRow, 1, currentRow, 7);
        totalLabelRange.Merge();
        totalLabelRange.Style.Font.Bold = true;
        totalLabelRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        totalLabelRange.Style.Fill.BackgroundColor = lightTeal;
        totalLabelRange.Style.Font.FontColor = tealColor;

        ws.Cell(currentRow, 8).Value = totalOpening;
        ws.Cell(currentRow, 9).Value = totalCash;
        ws.Cell(currentRow, 10).Value = totalExpected;
        ws.Cell(currentRow, 11).Value = totalPhysical ?? 0m;
        ws.Cell(currentRow, 12).Value = totalVariance ?? 0m;
        ws.Cell(currentRow, 13).Value = totalNonCash;

        for (int col = 8; col <= 13; col++)
        {
            var cell = ws.Cell(currentRow, col);
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = tealColor;
            cell.Style.Fill.BackgroundColor = lightTeal;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            cell.Style.NumberFormat.Format = "#,##0";
        }

        for (int c = 1; c <= headers.Length; c++)
        {
            var cell = ws.Cell(currentRow, c);
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            cell.Style.Border.OutsideBorderColor = XLColor.FromHtml("#8adce4");
        }

        ws.Columns().AdjustToContents();

        using var memoryStream = new MemoryStream();
        workbook.SaveAs(memoryStream);
        var excelBytes = memoryStream.ToArray();

        return (excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Laporan_Pendapatan_Kasir_{dateFromStr}_{dateToStr}.xlsx");
    }

    // ====================================================================================
    // HELPER INTERNAL
    // ====================================================================================

    private IQueryable<BilCashierShift> BuildFilteredShiftsQuery(CashierShiftRevenueReportQuery query)
    {
        var shiftsQuery = _dbContext.BilCashierShifts.AsNoTracking().Where(s => !s.IsDelete);

        if (query.DateFrom.HasValue)
        {
            var startUtc = FinanceBusinessDate.GetStartOfDayUtc(query.DateFrom.Value);
            shiftsQuery = shiftsQuery.Where(s => s.OpenedAt >= startUtc);
        }

        if (query.DateTo.HasValue)
        {
            var endUtc = FinanceBusinessDate.GetEndOfDayUtc(query.DateTo.Value);
            shiftsQuery = shiftsQuery.Where(s => s.OpenedAt < endUtc);
        }

        if (query.CashierId.HasValue)
        {
            shiftsQuery = shiftsQuery.Where(s => s.CashierId == query.CashierId.Value);
        }

        if (query.ShiftId.HasValue)
        {
            shiftsQuery = shiftsQuery.Where(s => s.Id == query.ShiftId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            shiftsQuery = shiftsQuery.Where(s => s.Status == query.Status);
        }

        if (!string.IsNullOrWhiteSpace(query.Location))
        {
            var registerIds = _dbContext.MstRegisters.AsNoTracking()
                .Where(r => !r.IsDelete && r.Location == query.Location)
                .Select(r => r.Id);
            shiftsQuery = shiftsQuery.Where(s => registerIds.Contains(s.RegisterId));
        }

        if (!string.IsNullOrWhiteSpace(query.PaymentMethod))
        {
            var shiftIdsWithMethod = _dbContext.BilTenders.AsNoTracking()
                .Where(t => !t.IsDelete && t.Status == BillingTenderStatuses.Succeeded && t.CashierShiftId.HasValue)
                .Join(_dbContext.MstPaymentMethods.AsNoTracking(),
                    t => t.PaymentMethodId,
                    pm => pm.Id,
                    (t, pm) => new { t.CashierShiftId, pm.PaymentMethodType, pm.PaymentMethodName })
                .Where(j => j.PaymentMethodType == query.PaymentMethod || j.PaymentMethodName == query.PaymentMethod)
                .Select(j => j.CashierShiftId!.Value)
                .Distinct();

            shiftsQuery = shiftsQuery.Where(s => shiftIdsWithMethod.Contains(s.Id));
        }

        return shiftsQuery;
    }
}
