using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;

/// <summary>
/// Layanan read-model Laporan Pembayaran AR (Report Payment AR).
/// Menampilkan pembayaran yang dialokasikan terhadap Invoice AR tertentu (FinReceivableInvoiceBatch),
/// bersumber dari Kas/Bank maupun Ayat Silang (FinCrossEntry), dengan grain per kejadian pembayaran (ReceiptId + BatchId).
/// Seluruh kalkulasi berdasar ledger Finance V2, tidak melakukan mutasi data atau SaveChanges.
/// </summary>
public sealed class FinanceReceivablePaymentReportService
{
    private readonly ApplicationDbContext _dbContext;

    public FinanceReceivablePaymentReportService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaymentArReportPagedResponse> GetPaymentArReportAsync(
        PaymentArReportQuery query, CancellationToken cancellationToken)
    {
        var (pagedGroups, totalCount, summary) = await QueryPaymentGroupsAsync(query, query.PageNumber, query.PageSize, cancellationToken);

        var rows = await HydrateReportRowsAsync(pagedGroups, cancellationToken);

        var pageSize = query.PageSize <= 0 ? 10 : query.PageSize;
        var totalPages = totalCount == 0 ? 1 : (int)Math.Ceiling((double)totalCount / pageSize);

        return new PaymentArReportPagedResponse
        {
            Items = rows,
            Summary = summary,
            PageNumber = query.PageNumber <= 0 ? 1 : query.PageNumber,
            PageSize = pageSize,
            TotalData = totalCount,
            TotalPage = totalPages
        };
    }

    public async Task<PaymentArReportSummaryResponse> GetPaymentArSummaryAsync(
        PaymentArReportQuery query, CancellationToken cancellationToken)
    {
        var (_, _, summary) = await QueryPaymentGroupsAsync(query, 1, 0, cancellationToken);
        return summary;
    }

    public async Task<PaymentArDetailResponse> GetPaymentArDetailAsync(
        Guid receiptId, Guid batchId, CancellationToken cancellationToken)
    {
        var batch = await _dbContext.FinReceivableInvoiceBatches.AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == batchId && !b.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException($"Invoice AR Batch dengan ID {batchId} tidak ditemukan.");

        var receipt = await _dbContext.FinReceipts.AsNoTracking()
            .Include(r => r.BankAccount)
                .ThenInclude(ba => ba!.Bank)
            .FirstOrDefaultAsync(r => r.Id == receiptId && !r.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException($"Penerimaan pembayaran dengan ID {receiptId} tidak ditemukan.");

        var crossEntry = await _dbContext.FinCrossEntries.AsNoTracking()
            .Include(ce => ce.BankAccount)
                .ThenInclude(ba => ba!.Bank)
            .FirstOrDefaultAsync(ce => ce.ReceiptId == receiptId && !ce.IsDelete, cancellationToken);

        var crossEntryTx = crossEntry == null
            ? await (from cet in _dbContext.FinCrossEntryTransactions.AsNoTracking()
                     join ce in _dbContext.FinCrossEntries.AsNoTracking().Include(x => x.BankAccount).ThenInclude(b => b!.Bank) on cet.CrossEntryId equals ce.Id
                     join a in _dbContext.FinReceiptAllocations.AsNoTracking() on cet.ReceiptAllocationId equals a.Id
                     where a.ReceiptId == receiptId && !cet.IsDelete && !ce.IsDelete && !a.IsDelete
                     select ce).FirstOrDefaultAsync(cancellationToken)
            : null;

        var effectiveCrossEntry = crossEntry ?? crossEntryTx;

        // Ambil penjamin / nama debtor
        var debtorName = await ResolvePayerNameAsync(batch.DebtorReferenceId, cancellationToken);
        var isInsurance = await _dbContext.MstInsuranceProviders.AnyAsync(ip => ip.Id == batch.DebtorReferenceId && !ip.IsDelete, cancellationToken);
        var payerType = isInsurance ? "Asuransi" : "Perusahaan";

        var isCrossEntry = effectiveCrossEntry != null;
        var paymentSource = isCrossEntry ? "AYAT_SILANG" : "KAS_BANK";
        var paymentMethod = isCrossEntry ? "Ayat Silang" : "Kas/Bank";

        var bankAccount = receipt.BankAccount ?? effectiveCrossEntry?.BankAccount;
        var bankRecipient = bankAccount?.Bank != null
            ? $"{bankAccount.Bank.BankName} - {bankAccount.AccountNumber} ({bankAccount.AccountName})"
            : (receipt.BankAccount != null ? receipt.BankAccount.AccountNumber : "-");

        var referenceNumber = !string.IsNullOrWhiteSpace(receipt.ProviderReference)
            ? receipt.ProviderReference
            : (effectiveCrossEntry?.ReferenceNumber ?? receipt.ReceiptNumber);

        var paymentDate = DateOnly.FromDateTime(FinanceBusinessDate.ToBusinessTime(receipt.OccurredAt).DateTime);

        // Ambil seluruh anggota aktif dari batch ini
        var memberRows = await (from bi in _dbContext.FinReceivableInvoiceBatchItems.AsNoTracking()
                                join r in _dbContext.FinReceivables.AsNoTracking() on bi.ReceivableId equals r.Id
                                join inv in _dbContext.BilInvoices.AsNoTracking() on r.InvoiceId equals inv.Id into invJoin
                                from inv in invJoin.DefaultIfEmpty()
                                join enc in _dbContext.RegPatientEncounters.AsNoTracking() on inv.EncounterId equals enc.Id into encJoin
                                from enc in encJoin.DefaultIfEmpty()
                                join pat in _dbContext.MstPatients.AsNoTracking() on enc.PatientId equals pat.Id into patJoin
                                from pat in patJoin.DefaultIfEmpty()
                                where bi.BatchId == batchId && bi.IsActiveMembership && !bi.IsDelete && !r.IsDelete
                                select new
                                {
                                    bi,
                                    r,
                                    inv,
                                    pat
                                }).ToListAsync(cancellationToken);

        var recIds = memberRows.Select(m => m.r.Id).ToList();

        // Alokasi pada transaksi (ReceiptId) ini
        var allocationsThisReceipt = await _dbContext.FinReceiptAllocations.AsNoTracking()
            .Where(a => a.ReceiptId == receiptId
                     && a.ReceivableId != null
                     && recIds.Contains(a.ReceivableId.Value)
                     && !a.IsDelete
                     && !a.IsReversal
                     && !_dbContext.FinReceiptAllocations.Any(rev => rev.ReversalOfAllocationId == a.Id && !rev.IsDelete))
            .ToListAsync(cancellationToken);

        var allocThisReceiptMap = allocationsThisReceipt
            .GroupBy(a => a.ReceivableId!.Value)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

        // Seluruh alokasi historis untuk menghitung urutan "Pembayaran Ke"
        var allHistoricalAllocations = await (from a in _dbContext.FinReceiptAllocations.AsNoTracking()
                                              join rc in _dbContext.FinReceipts.AsNoTracking() on a.ReceiptId equals rc.Id
                                              where a.ReceivableId != null
                                                 && recIds.Contains(a.ReceivableId.Value)
                                                 && !a.IsDelete
                                                 && !a.IsReversal
                                                 && rc.Status != FinReceiptStatuses.Reversed
                                                 && !rc.IsDelete
                                                 && !_dbContext.FinReceiptAllocations.Any(rev => rev.ReversalOfAllocationId == a.Id && !rev.IsDelete)
                                              orderby a.AllocatedAt ascending, a.CreateDateTime ascending, a.Id ascending
                                              select new
                                              {
                                                  a.ReceiptId,
                                                  a.ReceivableId,
                                                  a.Amount
                                              }).ToListAsync(cancellationToken);

        var allocSeqMap = allHistoricalAllocations
            .GroupBy(a => a.ReceivableId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var detailItems = new List<PaymentArDetailItemResponse>();
        foreach (var m in memberRows)
        {
            allocThisReceiptMap.TryGetValue(m.r.Id, out var allocatedInThisPayment);

            var paymentSeq = "-";
            if (allocSeqMap.TryGetValue(m.r.Id, out var history))
            {
                var idx = history.FindIndex(h => h.ReceiptId == receiptId);
                if (idx >= 0)
                {
                    paymentSeq = $"Pembayaran ke-{idx + 1}";
                }
                else if (allocatedInThisPayment > 0)
                {
                    paymentSeq = $"Pembayaran ke-{history.Count + 1}";
                }
            }

            var statusStr = m.r.OutstandingAmount <= 0
                ? "Terbayar"
                : (m.r.AllocatedAmount > 0 ? "Dibayar Sebagian" : "Belum Lunas");

            detailItems.Add(new PaymentArDetailItemResponse
            {
                ReceivableId = m.r.Id,
                PatientName = m.pat?.FullName ?? (!string.IsNullOrWhiteSpace(debtorName) ? debtorName : "Pasien Umum"),
                MedicalRecordNumber = m.pat?.MedicalRecordNumber ?? "-",
                BillingNumber = m.inv?.InvoiceNumber ?? m.r.ReceivableNumber,
                InvoiceNumber = batch.BatchNumber,
                BillingDate = m.inv?.InvoiceDate.HasValue == true
                    ? DateOnly.FromDateTime(m.inv.InvoiceDate.Value.DateTime)
                    : DateOnly.FromDateTime(m.r.RecognizedAt.DateTime),
                DueDate = m.r.DueDate,
                PaymentSequence = paymentSeq,
                TotalPiutang = m.r.OriginalAmount,
                PiutangTerbayar = allocatedInThisPayment,
                SisaPembayaran = m.r.OutstandingAmount,
                Status = statusStr
            });
        }

        var totalPiutangBatch = memberRows.Sum(m => m.r.OriginalAmount);
        var totalTerbayarBatch = allocationsThisReceipt.Sum(a => a.Amount);
        var totalSisaBatch = memberRows.Sum(m => m.r.OutstandingAmount);

        return new PaymentArDetailResponse
        {
            ReceiptId = receiptId,
            BatchId = batchId,
            PaymentKey = $"{receiptId}_{batchId}",
            DebtorName = debtorName,
            PayerType = payerType,
            BankRecipient = bankRecipient,
            PaymentSource = paymentSource,
            PaymentMethod = paymentMethod,
            ReferenceNumber = referenceNumber,
            ReceiptNumber = receipt.ReceiptNumber,
            InvoiceNumber = batch.BatchNumber,
            CrossEntryNumber = effectiveCrossEntry?.CrossEntryNumber,
            PaymentDate = paymentDate,
            Summary = new PaymentArDetailSummaryResponse
            {
                JumlahDetail = memberRows.Count,
                TotalPiutang = totalPiutangBatch,
                PiutangTerbayar = totalTerbayarBatch,
                SisaPembayaran = totalSisaBatch
            },
            Items = detailItems
        };
    }

    public async Task<byte[]> ExportListExcelAsync(PaymentArReportQuery query, CancellationToken cancellationToken)
    {
        var (allGroups, _, _) = await QueryPaymentGroupsAsync(query, 1, int.MaxValue, cancellationToken);
        var rows = await HydrateReportRowsAsync(allGroups, cancellationToken);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Laporan Pembayaran AR");

        // Title & Header Info
        ws.Cell(1, 1).Value = "LAPORAN PEMBAYARAN AR";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;

        var periodStr = query.StartDate.HasValue && query.EndDate.HasValue
            ? $"{query.StartDate.Value:dd/MM/yyyy} s/d {query.EndDate.Value:dd/MM/yyyy}"
            : (query.StartDate.HasValue ? $"Mulai {query.StartDate.Value:dd/MM/yyyy}" : (query.EndDate.HasValue ? $"Sampai {query.EndDate.Value:dd/MM/yyyy}" : "Semua Periode"));
        ws.Cell(2, 1).Value = $"Periode: {periodStr} | Tanggal Unduh: {DateTime.Now:dd/MM/yyyy HH:mm} WIB";
        ws.Cell(2, 1).Style.Font.Italic = true;

        // Table Header
        string[] headers = [
            "No.", "Penjamin / Asuransi", "No. Invoice AR", "Metode", "Bank / Penerima",
            "No. Referensi", "No. Receipt", "No. Ayat Silang", "Tgl. Pembayaran",
            "Total Tagihan", "Total Pembayaran", "Sisa Pembayaran"
        ];

        int headerRow = 4;
        for (int c = 0; c < headers.Length; c++)
        {
            var cell = ws.Cell(headerRow, c + 1);
            cell.Value = headers[c];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#087d91");
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = (c >= 9) ? XLAlignmentHorizontalValues.Right : (c == 0 || c == 8 ? XLAlignmentHorizontalValues.Center : XLAlignmentHorizontalValues.Left);
        }

        int currRow = 5;
        decimal grandTotalTagihan = 0m;
        decimal grandTotalBayar = 0m;
        decimal grandTotalSisa = 0m;

        for (int i = 0; i < rows.Count; i++)
        {
            var r = rows[i];
            ws.Cell(currRow, 1).Value = i + 1;
            ws.Cell(currRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Cell(currRow, 2).Value = r.DebtorName;
            ws.Cell(currRow, 3).Value = r.InvoiceNumber;
            ws.Cell(currRow, 4).Value = r.PaymentMethod;
            ws.Cell(currRow, 5).Value = r.BankRecipient;
            ws.Cell(currRow, 6).Value = r.ReferenceNumber;
            ws.Cell(currRow, 7).Value = r.ReceiptNumber;
            ws.Cell(currRow, 8).Value = r.CrossEntryNumber ?? "-";

            ws.Cell(currRow, 9).Value = r.PaymentDate.ToString("dd/MM/yyyy");
            ws.Cell(currRow, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Cell(currRow, 10).Value = r.TotalTagihan;
            ws.Cell(currRow, 10).Style.NumberFormat.Format = "#,##0";
            ws.Cell(currRow, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            ws.Cell(currRow, 11).Value = r.TotalPembayaran;
            ws.Cell(currRow, 11).Style.NumberFormat.Format = "#,##0";
            ws.Cell(currRow, 11).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            ws.Cell(currRow, 12).Value = r.SisaPembayaran;
            ws.Cell(currRow, 12).Style.NumberFormat.Format = "#,##0";
            ws.Cell(currRow, 12).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            grandTotalTagihan += r.TotalTagihan;
            grandTotalBayar += r.TotalPembayaran;
            grandTotalSisa += r.SisaPembayaran;

            currRow++;
        }

        // Summary row
        ws.Cell(currRow, 1).Value = "TOTAL";
        ws.Range(currRow, 1, currRow, 9).Merge().Style.Font.Bold = true;
        ws.Cell(currRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        ws.Row(currRow).Style.Fill.BackgroundColor = XLColor.FromHtml("#f1f5f9");

        ws.Cell(currRow, 10).Value = grandTotalTagihan;
        ws.Cell(currRow, 10).Style.Font.Bold = true;
        ws.Cell(currRow, 10).Style.NumberFormat.Format = "#,##0";

        ws.Cell(currRow, 11).Value = grandTotalBayar;
        ws.Cell(currRow, 11).Style.Font.Bold = true;
        ws.Cell(currRow, 11).Style.NumberFormat.Format = "#,##0";

        ws.Cell(currRow, 12).Value = grandTotalSisa;
        ws.Cell(currRow, 12).Style.Font.Bold = true;
        ws.Cell(currRow, 12).Style.NumberFormat.Format = "#,##0";

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    public async Task<byte[]> ExportListPdfAsync(PaymentArReportQuery query, CancellationToken cancellationToken)
    {
        var (allGroups, _, summary) = await QueryPaymentGroupsAsync(query, 1, int.MaxValue, cancellationToken);
        var rows = await HydrateReportRowsAsync(allGroups, cancellationToken);

        return PaymentArReportDocuments.BuildListPdf(rows, summary, query);
    }

    public async Task<byte[]> ExportDetailExcelAsync(Guid receiptId, Guid batchId, CancellationToken cancellationToken)
    {
        var detail = await GetPaymentArDetailAsync(receiptId, batchId, cancellationToken);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Detail Pembayaran AR");

        // Title
        ws.Cell(1, 1).Value = "DETAIL LAPORAN PEMBAYARAN AR";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;

        // Metadata
        ws.Cell(3, 1).Value = "Penjamin:";
        ws.Cell(3, 2).Value = detail.DebtorName;
        ws.Cell(4, 1).Value = "Bank / Metode:";
        ws.Cell(4, 2).Value = detail.BankRecipient;
        ws.Cell(5, 1).Value = "Sumber Pembayaran:";
        ws.Cell(5, 2).Value = detail.PaymentMethod;

        ws.Cell(3, 4).Value = "No. Invoice AR:";
        ws.Cell(3, 5).Value = detail.InvoiceNumber;
        ws.Cell(4, 4).Value = "No. Receipt:";
        ws.Cell(4, 5).Value = detail.ReceiptNumber;
        ws.Cell(5, 4).Value = "No. Referensi:";
        ws.Cell(5, 5).Value = detail.ReferenceNumber;
        if (!string.IsNullOrWhiteSpace(detail.CrossEntryNumber))
        {
            ws.Cell(6, 4).Value = "No. Ayat Silang:";
            ws.Cell(6, 5).Value = detail.CrossEntryNumber;
        }

        // Summary Cards
        ws.Cell(8, 1).Value = $"Jumlah Detail: {detail.Summary.JumlahDetail} Pasien";
        ws.Cell(8, 2).Value = $"Total Piutang: Rp {detail.Summary.TotalPiutang:N0}";
        ws.Cell(8, 4).Value = $"Piutang Terbayar: Rp {detail.Summary.PiutangTerbayar:N0}";
        ws.Cell(8, 5).Value = $"Sisa Pembayaran: Rp {detail.Summary.SisaPembayaran:N0}";
        ws.Range(8, 1, 8, 5).Style.Font.Bold = true;

        // Table Header
        string[] headers = [
            "No.", "Nama Pasien", "No. RM", "No. Billing", "No. Invoice",
            "Tgl. Tagihan", "Tgl. Jatuh Tempo", "Pembayaran Ke",
            "Total Piutang", "Piutang Terbayar", "Sisa Pembayaran", "Status"
        ];

        int headerRow = 10;
        for (int c = 0; c < headers.Length; c++)
        {
            var cell = ws.Cell(headerRow, c + 1);
            cell.Value = headers[c];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#087d91");
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = (c >= 8 && c <= 10) ? XLAlignmentHorizontalValues.Right : (c == 0 || c == 5 || c == 6 || c == 7 || c == 11 ? XLAlignmentHorizontalValues.Center : XLAlignmentHorizontalValues.Left);
        }

        int currRow = 11;
        for (int i = 0; i < detail.Items.Count; i++)
        {
            var item = detail.Items[i];
            ws.Cell(currRow, 1).Value = i + 1;
            ws.Cell(currRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Cell(currRow, 2).Value = item.PatientName;
            ws.Cell(currRow, 3).Value = item.MedicalRecordNumber;
            ws.Cell(currRow, 4).Value = item.BillingNumber;
            ws.Cell(currRow, 5).Value = item.InvoiceNumber;

            ws.Cell(currRow, 6).Value = item.BillingDate.HasValue ? item.BillingDate.Value.ToString("dd/MM/yyyy") : "-";
            ws.Cell(currRow, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Cell(currRow, 7).Value = item.DueDate.HasValue ? item.DueDate.Value.ToString("dd/MM/yyyy") : "-";
            ws.Cell(currRow, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Cell(currRow, 8).Value = item.PaymentSequence;
            ws.Cell(currRow, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Cell(currRow, 9).Value = item.TotalPiutang;
            ws.Cell(currRow, 9).Style.NumberFormat.Format = "#,##0";
            ws.Cell(currRow, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            ws.Cell(currRow, 10).Value = item.PiutangTerbayar;
            ws.Cell(currRow, 10).Style.NumberFormat.Format = "#,##0";
            ws.Cell(currRow, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            ws.Cell(currRow, 11).Value = item.SisaPembayaran;
            ws.Cell(currRow, 11).Style.NumberFormat.Format = "#,##0";
            ws.Cell(currRow, 11).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            ws.Cell(currRow, 12).Value = item.Status;
            ws.Cell(currRow, 12).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            currRow++;
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    public async Task<byte[]> ExportDetailPdfAsync(Guid receiptId, Guid batchId, CancellationToken cancellationToken)
    {
        var detail = await GetPaymentArDetailAsync(receiptId, batchId, cancellationToken);
        return PaymentArReportDocuments.BuildDetailPdf(detail);
    }

    // ------------------------------------------------------------------------------------
    // INTERNAL QUERY HELPERS
    // ------------------------------------------------------------------------------------

    private sealed class RawPaymentGroup
    {
        public Guid ReceiptId { get; set; }
        public Guid BatchId { get; set; }
        public decimal TotalAllocatedAmount { get; set; }
        public DateTimeOffset PaymentDateUtc { get; set; }
    }

    private async Task<(List<RawPaymentGroup> PagedGroups, int TotalCount, PaymentArReportSummaryResponse Summary)>
        QueryPaymentGroupsAsync(PaymentArReportQuery query, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        // 1. Base allocation query dengan penegakan integritas data:
        //    - Tidak dihapus
        //    - Bukan baris reversal
        //    - Tidak pernah dibalik oleh baris reversal manapun
        //    - Kuitansi tidak dalam status REVERSED
        //    - Terhubung dengan anggota aktif Invoice AR Batch
        var baseAllocQuery = from alloc in _dbContext.FinReceiptAllocations.AsNoTracking()
                             join r in _dbContext.FinReceipts.AsNoTracking() on alloc.ReceiptId equals r.Id
                             join rec in _dbContext.FinReceivables.AsNoTracking() on alloc.ReceivableId equals rec.Id
                             join bi in _dbContext.FinReceivableInvoiceBatchItems.AsNoTracking() on rec.Id equals bi.ReceivableId
                             join b in _dbContext.FinReceivableInvoiceBatches.AsNoTracking() on bi.BatchId equals b.Id
                             where !alloc.IsDelete
                                && !alloc.IsReversal
                                && alloc.ReceivableId != null
                                && !r.IsDelete
                                && r.Status != FinReceiptStatuses.Reversed
                                && !rec.IsDelete
                                && !bi.IsDelete
                                && bi.IsActiveMembership
                                && !b.IsDelete
                                && !_dbContext.FinReceiptAllocations.Any(rev => rev.ReversalOfAllocationId == alloc.Id && !rev.IsDelete)
                             select new
                             {
                                 alloc.Id,
                                 alloc.ReceiptId,
                                 bi.BatchId,
                                 alloc.Amount,
                                 alloc.AllocatedAt,
                                 r.ReceiptNumber,
                                 r.ProviderReference,
                                 r.BankAccountId,
                                 b.BatchNumber,
                                 b.DebtorReferenceId,
                                 b.TotalAmount
                             };

        // Filter Tanggal Bisnis (WIB)
        if (query.StartDate.HasValue)
        {
            var startUtc = FinanceBusinessDate.GetStartOfDayUtc(query.StartDate.Value);
            baseAllocQuery = baseAllocQuery.Where(x => x.AllocatedAt >= startUtc);
        }

        if (query.EndDate.HasValue)
        {
            var endUtc = FinanceBusinessDate.GetEndOfDayUtc(query.EndDate.Value);
            baseAllocQuery = baseAllocQuery.Where(x => x.AllocatedAt <= endUtc);
        }

        // Filter PayerId eksplisit bila dikirim
        if (query.PayerId.HasValue && query.PayerId != Guid.Empty)
        {
            baseAllocQuery = baseAllocQuery.Where(x => x.DebtorReferenceId == query.PayerId.Value);
        }

        // Filter PayerType (Semua / Asuransi / Perusahaan)
        if (!string.IsNullOrWhiteSpace(query.PayerType) && !string.Equals(query.PayerType, "SEMUA", StringComparison.OrdinalIgnoreCase))
        {
            if (string.Equals(query.PayerType, "ASURANSI", StringComparison.OrdinalIgnoreCase))
            {
                baseAllocQuery = baseAllocQuery.Where(x =>
                    _dbContext.MstInsuranceProviders.Any(ip => ip.Id == x.DebtorReferenceId && !ip.IsDelete));
            }
            else if (string.Equals(query.PayerType, "PERUSAHAAN", StringComparison.OrdinalIgnoreCase))
            {
                baseAllocQuery = baseAllocQuery.Where(x =>
                    _dbContext.MstCompanyGuarantors.Any(cg => cg.Id == x.DebtorReferenceId && !cg.IsDelete));
            }
        }

        // Filter PaymentSource (Semua / Kas/Bank / Ayat Silang)
        if (!string.IsNullOrWhiteSpace(query.PaymentSource) && !string.Equals(query.PaymentSource, "SEMUA", StringComparison.OrdinalIgnoreCase))
        {
            if (string.Equals(query.PaymentSource, "AYAT_SILANG", StringComparison.OrdinalIgnoreCase))
            {
                baseAllocQuery = baseAllocQuery.Where(x =>
                    _dbContext.FinCrossEntries.Any(ce => ce.ReceiptId == x.ReceiptId && !ce.IsDelete) ||
                    _dbContext.FinCrossEntryTransactions.Any(cet => cet.ReceiptAllocationId == x.Id && !cet.IsDelete));
            }
            else if (string.Equals(query.PaymentSource, "KAS_BANK", StringComparison.OrdinalIgnoreCase))
            {
                baseAllocQuery = baseAllocQuery.Where(x =>
                    !_dbContext.FinCrossEntries.Any(ce => ce.ReceiptId == x.ReceiptId && !ce.IsDelete) &&
                    !_dbContext.FinCrossEntryTransactions.Any(cet => cet.ReceiptAllocationId == x.Id && !cet.IsDelete));
            }
        }

        // Filter BankAccountId
        if (query.BankAccountId.HasValue && query.BankAccountId != Guid.Empty)
        {
            baseAllocQuery = baseAllocQuery.Where(x =>
                x.BankAccountId == query.BankAccountId.Value ||
                _dbContext.FinCrossEntries.Any(ce => ce.ReceiptId == x.ReceiptId && ce.BankAccountId == query.BankAccountId.Value && !ce.IsDelete));
        }

        // Filter ReferenceNumber
        if (!string.IsNullOrWhiteSpace(query.ReferenceNumber))
        {
            var refPattern = $"%{query.ReferenceNumber.Trim()}%";
            baseAllocQuery = baseAllocQuery.Where(x =>
                (x.ProviderReference != null && EF.Functions.ILike(x.ProviderReference, refPattern)) ||
                EF.Functions.ILike(x.ReceiptNumber, refPattern) ||
                _dbContext.FinCrossEntries.Any(ce => ce.ReceiptId == x.ReceiptId && !ce.IsDelete && (
                    EF.Functions.ILike(ce.CrossEntryNumber, refPattern) ||
                    EF.Functions.ILike(ce.ReferenceNumber, refPattern)
                )));
        }

        // Filter InvoiceNumber
        if (!string.IsNullOrWhiteSpace(query.InvoiceNumber))
        {
            var invPattern = $"%{query.InvoiceNumber.Trim()}%";
            baseAllocQuery = baseAllocQuery.Where(x => EF.Functions.ILike(x.BatchNumber, invPattern));
        }

        // Global Search
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = $"%{query.Search.Trim()}%";
            baseAllocQuery = baseAllocQuery.Where(x =>
                EF.Functions.ILike(x.BatchNumber, pattern) ||
                EF.Functions.ILike(x.ReceiptNumber, pattern) ||
                (x.ProviderReference != null && EF.Functions.ILike(x.ProviderReference, pattern)) ||
                _dbContext.MstCompanyGuarantors.Any(cg => cg.Id == x.DebtorReferenceId && !cg.IsDelete && EF.Functions.ILike(cg.CompanyGuarantorName, pattern)) ||
                _dbContext.MstInsuranceProviders.Any(ip => ip.Id == x.DebtorReferenceId && !ip.IsDelete && EF.Functions.ILike(ip.InsuranceProviderName, pattern)) ||
                _dbContext.MstBankAccounts.Any(ba => ba.Id == x.BankAccountId && !ba.IsDelete && ba.Bank != null && EF.Functions.ILike(ba.Bank.BankName, pattern)) ||
                _dbContext.FinCrossEntries.Any(ce => ce.ReceiptId == x.ReceiptId && !ce.IsDelete && (
                    EF.Functions.ILike(ce.CrossEntryNumber, pattern) ||
                    EF.Functions.ILike(ce.ReferenceNumber, pattern) ||
                    (ce.BankAccount != null && ce.BankAccount.Bank != null && EF.Functions.ILike(ce.BankAccount.Bank.BankName, pattern))
                )));
        }

        // 2. Grouping berdasarkan (ReceiptId, BatchId)
        var groupedQuery = baseAllocQuery
            .GroupBy(x => new { x.ReceiptId, x.BatchId })
            .Select(g => new RawPaymentGroup
            {
                ReceiptId = g.Key.ReceiptId,
                BatchId = g.Key.BatchId,
                TotalAllocatedAmount = g.Sum(x => x.Amount),
                PaymentDateUtc = g.Max(x => x.AllocatedAt)
            });

        // 3. Hitung Summary
        var totalTransaksi = await groupedQuery.CountAsync(cancellationToken);
        var totalBayar = totalTransaksi > 0 ? await groupedQuery.SumAsync(g => g.TotalAllocatedAmount, cancellationToken) : 0m;

        decimal totalTagihan = 0m;
        decimal totalSisa = 0m;

        if (totalTransaksi > 0)
        {
            var distinctBatchIds = await groupedQuery.Select(g => g.BatchId).Distinct().ToListAsync(cancellationToken);

            totalTagihan = await _dbContext.FinReceivableInvoiceBatches.AsNoTracking()
                .Where(b => distinctBatchIds.Contains(b.Id) && !b.IsDelete)
                .SumAsync(b => b.TotalAmount, cancellationToken);

            totalSisa = await (from bi in _dbContext.FinReceivableInvoiceBatchItems.AsNoTracking()
                               join r in _dbContext.FinReceivables.AsNoTracking() on bi.ReceivableId equals r.Id
                               where distinctBatchIds.Contains(bi.BatchId) && bi.IsActiveMembership && !bi.IsDelete && !r.IsDelete
                               select r.OutstandingAmount)
                               .SumAsync(cancellationToken);
        }

        var summary = new PaymentArReportSummaryResponse
        {
            TotalTransaksi = totalTransaksi,
            TotalTagihan = totalTagihan,
            TotalBayar = totalBayar,
            TotalSisa = totalSisa
        };

        if (pageSize <= 0)
        {
            return (new List<RawPaymentGroup>(), totalTransaksi, summary);
        }

        // 4. Pagination & Sorting
        var pagedQuery = groupedQuery
            .OrderByDescending(g => g.PaymentDateUtc)
            .ThenByDescending(g => g.ReceiptId)
            .ThenByDescending(g => g.BatchId);

        var pagedGroups = pageSize == int.MaxValue
            ? await pagedQuery.ToListAsync(cancellationToken)
            : await pagedQuery.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return (pagedGroups, totalTransaksi, summary);
    }

    private async Task<List<PaymentArReportRowResponse>> HydrateReportRowsAsync(
        List<RawPaymentGroup> groups, CancellationToken cancellationToken)
    {
        if (groups.Count == 0) return [];

        var receiptIds = groups.Select(g => g.ReceiptId).Distinct().ToList();
        var batchIds = groups.Select(g => g.BatchId).Distinct().ToList();

        // 1. Fetch Receipts
        var receipts = await _dbContext.FinReceipts.AsNoTracking()
            .Where(r => receiptIds.Contains(r.Id))
            .Select(r => new
            {
                r.Id,
                r.ReceiptNumber,
                r.ProviderReference,
                r.BankAccountId
            })
            .ToListAsync(cancellationToken);
        var receiptsMap = receipts.GroupBy(r => r.Id).ToDictionary(g => g.Key, g => g.First());

        // 2. Fetch Batches
        var batches = await _dbContext.FinReceivableInvoiceBatches.AsNoTracking()
            .Where(b => batchIds.Contains(b.Id))
            .Select(b => new
            {
                b.Id,
                b.BatchNumber,
                b.DebtorReferenceId,
                b.TotalAmount
            })
            .ToListAsync(cancellationToken);
        var batchesMap = batches.GroupBy(b => b.Id).ToDictionary(g => g.Key, g => g.First());

        var debtorIds = batches.Select(b => b.DebtorReferenceId).Distinct().ToList();

        // 3. Resolve Cross Entries (1:1 per ReceiptId)
        var crossEntries = await _dbContext.FinCrossEntries.AsNoTracking()
            .Include(ce => ce.BankAccount)
                .ThenInclude(ba => ba!.Bank)
            .Where(ce => !ce.IsDelete && receiptIds.Contains(ce.ReceiptId))
            .ToListAsync(cancellationToken);
        var crossEntriesByReceipt = crossEntries.GroupBy(ce => ce.ReceiptId).ToDictionary(g => g.Key, g => g.First());

        // Check cross entries linked via transaction
        var missingCeReceiptIds = receiptIds.Where(id => !crossEntriesByReceipt.ContainsKey(id)).ToList();
        if (missingCeReceiptIds.Count > 0)
        {
            var txCrossEntries = await (from cet in _dbContext.FinCrossEntryTransactions.AsNoTracking()
                                        join ce in _dbContext.FinCrossEntries.AsNoTracking().Include(x => x.BankAccount).ThenInclude(b => b!.Bank) on cet.CrossEntryId equals ce.Id
                                        join a in _dbContext.FinReceiptAllocations.AsNoTracking() on cet.ReceiptAllocationId equals a.Id
                                        where missingCeReceiptIds.Contains(a.ReceiptId) && !cet.IsDelete && !ce.IsDelete && !a.IsDelete
                                        select new { a.ReceiptId, CrossEntry = ce })
                                        .ToListAsync(cancellationToken);

            foreach (var tx in txCrossEntries)
            {
                if (!crossEntriesByReceipt.ContainsKey(tx.ReceiptId))
                {
                    crossEntriesByReceipt[tx.ReceiptId] = tx.CrossEntry;
                }
            }
        }

        // 4. Resolve Bank Accounts
        var bankAccountIds = receipts
            .Where(r => r.BankAccountId.HasValue)
            .Select(r => r.BankAccountId!.Value)
            .Concat(crossEntries.Where(ce => ce.BankAccountId != Guid.Empty).Select(ce => ce.BankAccountId))
            .Distinct()
            .ToList();

        var bankAccountList = bankAccountIds.Count > 0
            ? await _dbContext.MstBankAccounts.AsNoTracking()
                .Include(ba => ba.Bank)
                .Where(ba => !ba.IsDelete && bankAccountIds.Contains(ba.Id))
                .ToListAsync(cancellationToken)
            : [];
        var bankAccounts = bankAccountList.GroupBy(ba => ba.Id).ToDictionary(g => g.Key, g => g.First());

        // 5. Resolve Payers (Company & Insurance)
        var companies = await _dbContext.MstCompanyGuarantors.AsNoTracking()
            .Where(cg => !cg.IsDelete && debtorIds.Contains(cg.Id))
            .ToListAsync(cancellationToken);
        var companyMap = companies.GroupBy(cg => cg.Id).ToDictionary(g => g.Key, g => g.First().CompanyGuarantorName);

        var insurances = await _dbContext.MstInsuranceProviders.AsNoTracking()
            .Where(ip => !ip.IsDelete && debtorIds.Contains(ip.Id))
            .ToListAsync(cancellationToken);
        var insuranceMap = insurances.GroupBy(ip => ip.Id).ToDictionary(g => g.Key, g => g.First().InsuranceProviderName);

        // 6. Resolve Batch Outstandings
        var batchOutstandingList = await (from bi in _dbContext.FinReceivableInvoiceBatchItems.AsNoTracking()
                                          join r in _dbContext.FinReceivables.AsNoTracking() on bi.ReceivableId equals r.Id
                                          where batchIds.Contains(bi.BatchId) && bi.IsActiveMembership && !bi.IsDelete && !r.IsDelete
                                          group r.OutstandingAmount by bi.BatchId into bg
                                          select new { BatchId = bg.Key, Outstanding = bg.Sum() })
                                          .ToListAsync(cancellationToken);
        var batchOutstandingMap = batchOutstandingList.GroupBy(x => x.BatchId).ToDictionary(g => g.Key, g => g.First().Outstanding);

        var result = new List<PaymentArReportRowResponse>(groups.Count);

        foreach (var g in groups)
        {
            receiptsMap.TryGetValue(g.ReceiptId, out var receipt);
            batchesMap.TryGetValue(g.BatchId, out var batch);
            crossEntriesByReceipt.TryGetValue(g.ReceiptId, out var ce);

            var isCrossEntry = ce != null;
            var paymentSource = isCrossEntry ? "AYAT_SILANG" : "KAS_BANK";
            var paymentMethod = isCrossEntry ? "Ayat Silang" : "Kas/Bank";

            // Payer name & type
            var debtorId = batch?.DebtorReferenceId ?? Guid.Empty;
            string debtorName;
            string payerType;
            if (insuranceMap.TryGetValue(debtorId, out var insName))
            {
                debtorName = insName;
                payerType = "Asuransi";
            }
            else if (companyMap.TryGetValue(debtorId, out var compName))
            {
                debtorName = compName;
                payerType = "Perusahaan";
            }
            else
            {
                debtorName = "Penjamin AR";
                payerType = "Perusahaan";
            }

            // Bank recipient
            string bankRecipient = "-";
            var bankAccountId = receipt?.BankAccountId ?? ce?.BankAccountId;
            if (bankAccountId.HasValue && bankAccounts.TryGetValue(bankAccountId.Value, out var ba))
            {
                bankRecipient = ba.Bank != null
                    ? $"{ba.Bank.BankName} - {ba.AccountNumber}"
                    : ba.AccountNumber;
            }
            else if (ce?.BankAccount?.Bank != null)
            {
                bankRecipient = $"{ce.BankAccount.Bank.BankName} - {ce.BankAccount.AccountNumber}";
            }

            var refNumber = !string.IsNullOrWhiteSpace(receipt?.ProviderReference)
                ? receipt.ProviderReference
                : (ce?.ReferenceNumber ?? receipt?.ReceiptNumber ?? "-");

            var paymentDate = DateOnly.FromDateTime(FinanceBusinessDate.ToBusinessTime(g.PaymentDateUtc).DateTime);

            batchOutstandingMap.TryGetValue(g.BatchId, out var batchOutstanding);

            result.Add(new PaymentArReportRowResponse
            {
                RowKey = $"{g.ReceiptId}_{g.BatchId}",
                ReceiptId = g.ReceiptId,
                BatchId = g.BatchId,
                DebtorName = debtorName,
                PayerType = payerType,
                PaymentSource = paymentSource,
                PaymentMethod = paymentMethod,
                BankRecipient = bankRecipient,
                ReferenceNumber = refNumber,
                ReceiptNumber = receipt?.ReceiptNumber ?? "-",
                CrossEntryNumber = ce?.CrossEntryNumber,
                PaymentDate = paymentDate,
                InvoiceNumber = batch?.BatchNumber ?? "-",
                TotalTagihan = batch?.TotalAmount ?? 0m,
                TotalPembayaran = g.TotalAllocatedAmount,
                SisaPembayaran = batchOutstanding
            });
        }

        return result;
    }

    private async Task<string> ResolvePayerNameAsync(Guid debtorReferenceId, CancellationToken cancellationToken)
    {
        var ins = await _dbContext.MstInsuranceProviders.AsNoTracking()
            .Where(x => x.Id == debtorReferenceId && !x.IsDelete)
            .Select(x => x.InsuranceProviderName)
            .FirstOrDefaultAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(ins)) return ins;

        var comp = await _dbContext.MstCompanyGuarantors.AsNoTracking()
            .Where(x => x.Id == debtorReferenceId && !x.IsDelete)
            .Select(x => x.CompanyGuarantorName)
            .FirstOrDefaultAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(comp)) return comp;

        return "Penjamin AR";
    }
}
