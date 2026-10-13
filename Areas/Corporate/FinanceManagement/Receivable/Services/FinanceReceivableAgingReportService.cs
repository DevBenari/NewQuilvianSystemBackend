using System.Globalization;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;

/// <summary>
/// Service read-only untuk Laporan Aging AR (Halaman A: Daftar Penjamin dan Halaman B: Detail Aging per Penjamin).
/// Mengikuti format V1 dengan 5 bucket keterlambatan (≤30, 31-60, 61-90, 91-120, >120 Hari) + Belum Jatuh Tempo.
/// Mendukung saldo historis AsOfDate, partial payment, reversal, dan ekspor Excel dan PDF.
/// </summary>
public sealed class FinanceReceivableAgingReportService
{
    private static readonly CultureInfo IdCulture = new("id-ID");
    private readonly ApplicationDbContext _dbContext;

    public FinanceReceivableAgingReportService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Mengambil daftar penjamin/perusahaan terpaginasi (Halaman A).
    /// </summary>
    public async Task<ReceivableAgingPayerPagedResponse> GetAgingPayersPagedAsync(
        FinanceReceivableAgingPayerQuery query, CancellationToken cancellationToken)
    {
        var asOfDate = query.AsOfDate ?? FinanceBusinessDate.Today();
        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = Math.Clamp(query.PageSize, 1, 50);

        // 1. Ambil master penjamin (Perusahaan & Asuransi)
        var companies = await _dbContext.MstCompanyGuarantors.AsNoTracking()
            .Where(x => !x.IsDelete && x.IsActive)
            .Select(x => new
            {
                Id = x.Id,
                Code = x.CompanyGuarantorCode,
                Name = x.CompanyGuarantorName,
                Type = "Perusahaan"
            })
            .ToListAsync(cancellationToken);

        var insurances = await _dbContext.MstInsuranceProviders.AsNoTracking()
            .Where(x => !x.IsDelete && x.IsActive)
            .Select(x => new
            {
                Id = x.Id,
                Code = x.InsuranceProviderCode,
                Name = x.InsuranceProviderName,
                Type = "Asuransi"
            })
            .ToListAsync(cancellationToken);

        var allPayers = companies.Concat(insurances).ToList();

        // 2. Sertakan penjamin yang mungkin ada di transaksi piutang tetapi belum masuk daftar di atas
        var distinctPayerIdsInTx = await _dbContext.FinReceivables.AsNoTracking()
            .Where(r => !r.IsDelete && r.DebtorReferenceId.HasValue)
            .Select(r => r.DebtorReferenceId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        var existingPayerIdSet = allPayers.Select(x => x.Id).ToHashSet();
        foreach (var extraId in distinctPayerIdsInTx)
        {
            if (!existingPayerIdSet.Contains(extraId))
            {
                allPayers.Add(new
                {
                    Id = extraId,
                    Code = "EXT-" + extraId.ToString()[..6].ToUpperInvariant(),
                    Name = "Penjamin " + extraId.ToString()[..8].ToUpperInvariant(),
                    Type = "Penjamin"
                });
                existingPayerIdSet.Add(extraId);
            }
        }

        // 3. Filter berdasarkan pencarian nama atau kode
        var search = query.Search?.Trim();
        if (!string.IsNullOrWhiteSpace(search))
        {
            allPayers = allPayers.Where(p =>
                p.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                p.Code.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        // 4. Pengurutan Nama Penjamin ASC
        allPayers = allPayers.OrderBy(p => p.Name).ToList();

        var totalCount = allPayers.Count;
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
        var pagedPayers = allPayers.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        // 5. Hitung metrik invoice count & outstanding per penjamin aktif pada halaman ini
        var pagedPayerIds = pagedPayers.Select(p => p.Id).ToList();

        var batches = await _dbContext.FinReceivableInvoiceBatches.AsNoTracking()
            .Where(b => !b.IsDelete && pagedPayerIds.Contains(b.DebtorReferenceId))
            .Where(b => (b.InvoiceDate ?? DateOnly.FromDateTime(b.CreateDateTime.Date)) <= asOfDate)
            .Select(b => new { b.DebtorReferenceId, b.Id, b.Status })
            .ToListAsync(cancellationToken);

        var receivables = await _dbContext.FinReceivables.AsNoTracking()
            .Where(r => !r.IsDelete && r.DebtorReferenceId.HasValue && pagedPayerIds.Contains(r.DebtorReferenceId.Value))
            .Where(r => DateOnly.FromDateTime(r.RecognizedAt.Date) <= asOfDate)
            .Select(r => new { r.DebtorReferenceId, r.OutstandingAmount, r.Status })
            .ToListAsync(cancellationToken);

        var items = pagedPayers.Select(p =>
        {
            var pBatches = batches.Where(b => b.DebtorReferenceId == p.Id && b.Status != FinReceivableInvoiceBatchStatuses.Cancelled).ToList();
            var pReceivables = receivables.Where(r => r.DebtorReferenceId == p.Id && r.Status != FinReceivableStatuses.Cancelled).ToList();

            return new ReceivableAgingPayerItemResponse
            {
                PayerId = p.Id,
                PayerCode = p.Code,
                PayerName = p.Name,
                PayerType = p.Type,
                TotalInvoiceCount = pBatches.Select(b => b.Id).Distinct().Count(),
                TotalOutstanding = pReceivables.Sum(r => r.OutstandingAmount)
            };
        }).ToList();

        return new ReceivableAgingPayerPagedResponse
        {
            Items = items,
            TotalPenjamin = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = totalPages
        };
    }

    /// <summary>
    /// Mengambil ringkasan detail umur piutang per penjamin (Halaman B).
    /// </summary>
    public async Task<ReceivableAgingDetailSummaryResponse> GetAgingPayerSummaryAsync(
        Guid payerId, DateOnly? asOfDate, CancellationToken cancellationToken)
    {
        var resolvedDate = asOfDate ?? FinanceBusinessDate.Today();
        var (_, summary) = await LoadPayerInvoiceDataAsync(payerId, resolvedDate, cancellationToken);
        return summary;
    }

    /// <summary>
    /// Mengambil tabel detail invoice umur piutang per penjamin terpaginasi (Halaman B).
    /// </summary>
    public async Task<ReceivableAgingInvoicePagedResponse> GetAgingPayerInvoicesPagedAsync(
        Guid payerId, ReceivableAgingDetailQuery query, CancellationToken cancellationToken)
    {
        var resolvedDate = query.AsOfDate ?? FinanceBusinessDate.Today();
        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = Math.Clamp(query.PageSize, 1, 10); // Maksimum 10 baris per halaman

        var (allInvoices, summary) = await LoadPayerInvoiceDataAsync(payerId, resolvedDate, cancellationToken);

        // Filter status pembayaran
        var filtered = allInvoices.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(query.Status) && query.Status != "ALL")
        {
            var normalizedStatus = query.Status.Trim().ToUpperInvariant();
            filtered = normalizedStatus switch
            {
                "LUNAS" => filtered.Where(x => x.PaymentStatus.Equals("Lunas", StringComparison.OrdinalIgnoreCase)),
                "BELUM_LUNAS" => filtered.Where(x => x.PaymentStatus.Equals("Belum Lunas", StringComparison.OrdinalIgnoreCase)),
                "DIBAYAR_SEBAGIAN" => filtered.Where(x => x.PaymentStatus.Equals("Dibayar Sebagian", StringComparison.OrdinalIgnoreCase)),
                "DIBATALKAN" => filtered.Where(x => x.PaymentStatus.Equals("Dibatalkan", StringComparison.OrdinalIgnoreCase)),
                _ => filtered
            };
        }

        // Filter pencarian invoice number
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            filtered = filtered.Where(x => x.InvoiceNumber.Contains(s, StringComparison.OrdinalIgnoreCase));
        }

        // Sorting
        if (query.SortBy?.Equals("TotalAmount", StringComparison.OrdinalIgnoreCase) == true)
        {
            filtered = query.SortDirection?.Equals("DESC", StringComparison.OrdinalIgnoreCase) == true
                ? filtered.OrderByDescending(x => x.TotalAmount)
                : filtered.OrderBy(x => x.TotalAmount);
        }
        else if (query.SortBy?.Equals("OutstandingAmount", StringComparison.OrdinalIgnoreCase) == true)
        {
            filtered = query.SortDirection?.Equals("DESC", StringComparison.OrdinalIgnoreCase) == true
                ? filtered.OrderByDescending(x => x.OutstandingAmount)
                : filtered.OrderBy(x => x.OutstandingAmount);
        }
        else
        {
            filtered = filtered.OrderBy(x => x.DueDate).ThenBy(x => x.InvoiceNumber);
        }

        var filteredList = filtered.ToList();
        var totalCount = filteredList.Count;
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
        var pagedItems = filteredList.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        return new ReceivableAgingInvoicePagedResponse
        {
            Items = pagedItems,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            Summary = summary
        };
    }

    /// <summary>
    /// Ekspor detail invoice umur piutang per penjamin ke format Excel (.xlsx).
    /// </summary>
    public async Task<byte[]> ExportAgingPayerExcelAsync(
        Guid payerId, ReceivableAgingDetailQuery query, CancellationToken cancellationToken)
    {
        var resolvedDate = query.AsOfDate ?? FinanceBusinessDate.Today();
        var (allInvoices, summary) = await LoadPayerInvoiceDataAsync(payerId, resolvedDate, cancellationToken);

        // Filter sesuai query
        var filtered = allInvoices.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(query.Status) && query.Status != "ALL")
        {
            var normalizedStatus = query.Status.Trim().ToUpperInvariant();
            filtered = normalizedStatus switch
            {
                "LUNAS" => filtered.Where(x => x.PaymentStatus.Equals("Lunas", StringComparison.OrdinalIgnoreCase)),
                "BELUM_LUNAS" => filtered.Where(x => x.PaymentStatus.Equals("Belum Lunas", StringComparison.OrdinalIgnoreCase)),
                "DIBAYAR_SEBAGIAN" => filtered.Where(x => x.PaymentStatus.Equals("Dibayar Sebagian", StringComparison.OrdinalIgnoreCase)),
                "DIBATALKAN" => filtered.Where(x => x.PaymentStatus.Equals("Dibatalkan", StringComparison.OrdinalIgnoreCase)),
                _ => filtered
            };
        }
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            filtered = filtered.Where(x => x.InvoiceNumber.Contains(s, StringComparison.OrdinalIgnoreCase));
        }

        var invoiceList = filtered.OrderBy(x => x.DueDate).ThenBy(x => x.InvoiceNumber).ToList();

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Laporan Aging AR");

        // Judul & Header Penjamin
        ws.Cell(1, 1).Value = "LAPORAN AGING AR (UMUR PIUTANG)";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;
        ws.Cell(1, 1).Style.Font.FontColor = XLColor.FromHtml("#0d7f89");

        ws.Cell(2, 1).Value = $"Perusahaan/Penjamin: {summary.PayerName} ({summary.PayerCode})  |  Kategori: {summary.PayerType}  |  Posisi Saldo: {resolvedDate:dd-MM-yyyy}";
        ws.Cell(2, 1).Style.Font.FontSize = 10;
        ws.Cell(2, 1).Style.Font.Italic = true;

        // Blok Ringkasan
        int r = 4;
        ws.Cell(r, 1).Value = "RINGKASAN PORTFOLIO";
        ws.Cell(r, 1).Style.Font.Bold = true;
        r++;
        ws.Cell(r, 1).Value = "Total Invoice";
        ws.Cell(r, 2).Value = summary.TotalInvoice;
        ws.Cell(r, 3).Value = "Total Tagihan";
        ws.Cell(r, 4).Value = summary.TotalInvoiceAmount;
        ws.Cell(r, 4).Style.NumberFormat.Format = "#,##0";
        ws.Cell(r, 5).Value = "Total Pembayaran";
        ws.Cell(r, 6).Value = summary.TotalEffectivePayment;
        ws.Cell(r, 6).Style.NumberFormat.Format = "#,##0";
        ws.Cell(r, 7).Value = "Sisa Piutang";
        ws.Cell(r, 8).Value = summary.TotalOutstanding;
        ws.Cell(r, 8).Style.NumberFormat.Format = "#,##0";
        r++;
        ws.Cell(r, 1).Value = "Belum Jatuh Tempo";
        ws.Cell(r, 2).Value = summary.NotDueAmount;
        ws.Cell(r, 2).Style.NumberFormat.Format = "#,##0";
        ws.Cell(r, 3).Value = "≤30 Hari";
        ws.Cell(r, 4).Value = summary.Bucket1_0_30;
        ws.Cell(r, 4).Style.NumberFormat.Format = "#,##0";
        ws.Cell(r, 5).Value = "31-60 Hari";
        ws.Cell(r, 6).Value = summary.Bucket2_31_60;
        ws.Cell(r, 6).Style.NumberFormat.Format = "#,##0";
        ws.Cell(r, 7).Value = "61-90 Hari";
        ws.Cell(r, 8).Value = summary.Bucket3_61_90;
        ws.Cell(r, 8).Style.NumberFormat.Format = "#,##0";
        r++;
        ws.Cell(r, 1).Value = "91-120 Hari";
        ws.Cell(r, 2).Value = summary.Bucket4_91_120;
        ws.Cell(r, 2).Style.NumberFormat.Format = "#,##0";
        ws.Cell(r, 3).Value = ">120 Hari";
        ws.Cell(r, 4).Value = summary.Bucket5_Over120;
        ws.Cell(r, 4).Style.NumberFormat.Format = "#,##0";
        r += 2;

        // Header Tabel 13 Kolom
        string[] headers = {
            "No", "Invoice", "Jatuh Tempo", "Dokumen AR", "Total Tagihan",
            "Jumlah Bayar", "Sisa Piutang", "≤30 Hari", "31–60 Hari",
            "61–90 Hari", "91–120 Hari", ">120 Hari", "Status"
        };

        for (int c = 0; c < headers.Length; c++)
        {
            var cell = ws.Cell(r, c + 1);
            cell.Value = headers[c];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#0d7f89");
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = (c >= 4 && c <= 11) ? XLAlignmentHorizontalValues.Right : XLAlignmentHorizontalValues.Left;
        }

        r++;
        int rowNo = 1;
        foreach (var inv in invoiceList)
        {
            ws.Cell(r, 1).Value = rowNo++;
            ws.Cell(r, 2).Value = inv.InvoiceNumber;
            ws.Cell(r, 3).Value = inv.DueDate.HasValue ? inv.DueDate.Value.ToString("dd/MM/yyyy") : "-";
            ws.Cell(r, 4).Value = inv.HasDocument ? "Tersedia" : "Belum Tersedia";
            ws.Cell(r, 5).Value = inv.TotalAmount;
            ws.Cell(r, 6).Value = inv.PaidAmount;
            ws.Cell(r, 7).Value = inv.OutstandingAmount;
            ws.Cell(r, 8).Value = inv.Bucket1_0_30;
            ws.Cell(r, 9).Value = inv.Bucket2_31_60;
            ws.Cell(r, 10).Value = inv.Bucket3_61_90;
            ws.Cell(r, 11).Value = inv.Bucket4_91_120;
            ws.Cell(r, 12).Value = inv.Bucket5_Over120;
            ws.Cell(r, 13).Value = inv.PaymentStatus;

            for (int col = 5; col <= 12; col++)
            {
                ws.Cell(r, col).Style.NumberFormat.Format = "#,##0";
                ws.Cell(r, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            }

            r++;
        }

        // Baris Grand Total
        ws.Cell(r, 1).Value = "TOTAL";
        ws.Cell(r, 1).Style.Font.Bold = true;
        ws.Range(r, 1, r, 4).Merge();
        ws.Cell(r, 5).Value = invoiceList.Sum(x => x.TotalAmount);
        ws.Cell(r, 6).Value = invoiceList.Sum(x => x.PaidAmount);
        ws.Cell(r, 7).Value = invoiceList.Sum(x => x.OutstandingAmount);
        ws.Cell(r, 8).Value = invoiceList.Sum(x => x.Bucket1_0_30);
        ws.Cell(r, 9).Value = invoiceList.Sum(x => x.Bucket2_31_60);
        ws.Cell(r, 10).Value = invoiceList.Sum(x => x.Bucket3_61_90);
        ws.Cell(r, 11).Value = invoiceList.Sum(x => x.Bucket4_91_120);
        ws.Cell(r, 12).Value = invoiceList.Sum(x => x.Bucket5_Over120);

        for (int col = 5; col <= 12; col++)
        {
            ws.Cell(r, col).Style.Font.Bold = true;
            ws.Cell(r, col).Style.NumberFormat.Format = "#,##0";
            ws.Cell(r, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        }

        ws.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    /// <summary>
    /// Ekspor detail invoice umur piutang per penjamin ke format PDF (QuestPDF A4 Landscape).
    /// </summary>
    public async Task<byte[]> ExportAgingPayerPdfAsync(
        Guid payerId, ReceivableAgingDetailQuery query, CancellationToken cancellationToken)
    {
        var resolvedDate = query.AsOfDate ?? FinanceBusinessDate.Today();
        var (allInvoices, summary) = await LoadPayerInvoiceDataAsync(payerId, resolvedDate, cancellationToken);

        var filtered = allInvoices.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(query.Status) && query.Status != "ALL")
        {
            var normalizedStatus = query.Status.Trim().ToUpperInvariant();
            filtered = normalizedStatus switch
            {
                "LUNAS" => filtered.Where(x => x.PaymentStatus.Equals("Lunas", StringComparison.OrdinalIgnoreCase)),
                "BELUM_LUNAS" => filtered.Where(x => x.PaymentStatus.Equals("Belum Lunas", StringComparison.OrdinalIgnoreCase)),
                "DIBAYAR_SEBAGIAN" => filtered.Where(x => x.PaymentStatus.Equals("Dibayar Sebagian", StringComparison.OrdinalIgnoreCase)),
                "DIBATALKAN" => filtered.Where(x => x.PaymentStatus.Equals("Dibatalkan", StringComparison.OrdinalIgnoreCase)),
                _ => filtered
            };
        }
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            filtered = filtered.Where(x => x.InvoiceNumber.Contains(s, StringComparison.OrdinalIgnoreCase));
        }

        var invoiceList = filtered.OrderBy(x => x.DueDate).ThenBy(x => x.InvoiceNumber).ToList();

        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontSize(8).FontFamily("Arial"));

                page.Header().Column(col =>
                {
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Column(c =>
                        {
                            c.Item().Text("LAPORAN AGING AR (UMUR PIUTANG)").Bold().FontSize(13).FontColor("#0d7f89");
                            c.Item().Text($"Penjamin: {summary.PayerName} ({summary.PayerCode}) | Kategori: {summary.PayerType} | Posisi Saldo: {resolvedDate:dd-MM-yyyy}")
                                .FontSize(8).FontColor(Colors.Grey.Darken2);
                        });
                        r.ConstantItem(120).AlignRight().Text($"Dicetak: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(7).FontColor(Colors.Grey.Medium);
                    });

                    col.Item().PaddingVertical(6).LineHorizontal(1).LineColor("#0d7f89");

                    // Summary Block Mini
                    col.Item().PaddingBottom(8).Row(sr =>
                    {
                        sr.RelativeItem().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Column(c =>
                        {
                            c.Item().Text("Total Tagihan").FontSize(7).FontColor(Colors.Grey.Darken1);
                            c.Item().Text(summary.TotalInvoiceAmount.ToString("N0", IdCulture)).Bold().FontSize(8);
                        });
                        sr.RelativeItem().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Column(c =>
                        {
                            c.Item().Text("Total Bayar").FontSize(7).FontColor(Colors.Grey.Darken1);
                            c.Item().Text(summary.TotalEffectivePayment.ToString("N0", IdCulture)).Bold().FontSize(8);
                        });
                        sr.RelativeItem().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Column(c =>
                        {
                            c.Item().Text("Sisa Piutang").FontSize(7).FontColor("#0d7f89");
                            c.Item().Text(summary.TotalOutstanding.ToString("N0", IdCulture)).Bold().FontSize(8).FontColor("#0d7f89");
                        });
                        sr.RelativeItem().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Column(c =>
                        {
                            c.Item().Text("≤30 Hari").FontSize(7).FontColor(Colors.Grey.Darken1);
                            c.Item().Text(summary.Bucket1_0_30.ToString("N0", IdCulture)).Bold().FontSize(8);
                        });
                        sr.RelativeItem().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Column(c =>
                        {
                            c.Item().Text("31–60 Hari").FontSize(7).FontColor(Colors.Grey.Darken1);
                            c.Item().Text(summary.Bucket2_31_60.ToString("N0", IdCulture)).Bold().FontSize(8);
                        });
                        sr.RelativeItem().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Column(c =>
                        {
                            c.Item().Text("61–90 Hari").FontSize(7).FontColor(Colors.Grey.Darken1);
                            c.Item().Text(summary.Bucket3_61_90.ToString("N0", IdCulture)).Bold().FontSize(8);
                        });
                        sr.RelativeItem().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Column(c =>
                        {
                            c.Item().Text("91–120 Hari").FontSize(7).FontColor(Colors.Grey.Darken1);
                            c.Item().Text(summary.Bucket4_91_120.ToString("N0", IdCulture)).Bold().FontSize(8);
                        });
                        sr.RelativeItem().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Column(c =>
                        {
                            c.Item().Text(">120 Hari").FontSize(7).FontColor(Colors.Grey.Darken1);
                            c.Item().Text(summary.Bucket5_Over120.ToString("N0", IdCulture)).Bold().FontSize(8);
                        });
                    });
                });

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(col =>
                    {
                        col.ConstantColumn(20);  // No
                        col.RelativeColumn(3);  // Invoice
                        col.RelativeColumn(2);  // Due Date
                        col.RelativeColumn(2.5f); // Tagihan
                        col.RelativeColumn(2.5f); // Bayar
                        col.RelativeColumn(2.5f); // Sisa
                        col.RelativeColumn(2);  // <=30
                        col.RelativeColumn(2);  // 31-60
                        col.RelativeColumn(2);  // 61-90
                        col.RelativeColumn(2);  // 91-120
                        col.RelativeColumn(2);  // >120
                        col.RelativeColumn(2.2f); // Status
                    });

                    table.Header(h =>
                    {
                        h.Cell().Background("#0d7f89").Padding(3).Text("No").Bold().FontColor(Colors.White);
                        h.Cell().Background("#0d7f89").Padding(3).Text("Invoice").Bold().FontColor(Colors.White);
                        h.Cell().Background("#0d7f89").Padding(3).Text("Jatuh Tempo").Bold().FontColor(Colors.White);
                        h.Cell().Background("#0d7f89").Padding(3).AlignRight().Text("Tagihan").Bold().FontColor(Colors.White);
                        h.Cell().Background("#0d7f89").Padding(3).AlignRight().Text("Bayar").Bold().FontColor(Colors.White);
                        h.Cell().Background("#0d7f89").Padding(3).AlignRight().Text("Sisa Piutang").Bold().FontColor(Colors.White);
                        h.Cell().Background("#0d7f89").Padding(3).AlignRight().Text("≤30").Bold().FontColor(Colors.White);
                        h.Cell().Background("#0d7f89").Padding(3).AlignRight().Text("31–60").Bold().FontColor(Colors.White);
                        h.Cell().Background("#0d7f89").Padding(3).AlignRight().Text("61–90").Bold().FontColor(Colors.White);
                        h.Cell().Background("#0d7f89").Padding(3).AlignRight().Text("91–120").Bold().FontColor(Colors.White);
                        h.Cell().Background("#0d7f89").Padding(3).AlignRight().Text(">120").Bold().FontColor(Colors.White);
                        h.Cell().Background("#0d7f89").Padding(3).Text("Status").Bold().FontColor(Colors.White);
                    });

                    int no = 1;
                    foreach (var inv in invoiceList)
                    {
                        var bg = no % 2 == 0 ? Colors.Grey.Lighten5 : Colors.White;
                        table.Cell().Background(bg).Padding(3).Text(no.ToString());
                        table.Cell().Background(bg).Padding(3).Text(inv.InvoiceNumber);
                        table.Cell().Background(bg).Padding(3).Text(inv.DueDate?.ToString("dd/MM/yyyy") ?? "-");
                        table.Cell().Background(bg).Padding(3).AlignRight().Text(inv.TotalAmount.ToString("N0", IdCulture));
                        table.Cell().Background(bg).Padding(3).AlignRight().Text(inv.PaidAmount.ToString("N0", IdCulture));
                        table.Cell().Background(bg).Padding(3).AlignRight().Text(inv.OutstandingAmount.ToString("N0", IdCulture)).Bold().FontColor("#0d7f89");
                        table.Cell().Background(bg).Padding(3).AlignRight().Text(inv.Bucket1_0_30 > 0 ? inv.Bucket1_0_30.ToString("N0", IdCulture) : "-");
                        table.Cell().Background(bg).Padding(3).AlignRight().Text(inv.Bucket2_31_60 > 0 ? inv.Bucket2_31_60.ToString("N0", IdCulture) : "-");
                        table.Cell().Background(bg).Padding(3).AlignRight().Text(inv.Bucket3_61_90 > 0 ? inv.Bucket3_61_90.ToString("N0", IdCulture) : "-");
                        table.Cell().Background(bg).Padding(3).AlignRight().Text(inv.Bucket4_91_120 > 0 ? inv.Bucket4_91_120.ToString("N0", IdCulture) : "-");
                        table.Cell().Background(bg).Padding(3).AlignRight().Text(inv.Bucket5_Over120 > 0 ? inv.Bucket5_Over120.ToString("N0", IdCulture) : "-");
                        table.Cell().Background(bg).Padding(3).Text(inv.PaymentStatus);
                        no++;
                    }

                    // Grand Total
                    table.Cell().ColumnSpan(3).Background(Colors.Grey.Lighten3).Padding(4).Text("GRAND TOTAL").Bold();
                    table.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text(invoiceList.Sum(x => x.TotalAmount).ToString("N0", IdCulture)).Bold();
                    table.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text(invoiceList.Sum(x => x.PaidAmount).ToString("N0", IdCulture)).Bold();
                    table.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text(invoiceList.Sum(x => x.OutstandingAmount).ToString("N0", IdCulture)).Bold().FontColor("#0d7f89");
                    table.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text(invoiceList.Sum(x => x.Bucket1_0_30).ToString("N0", IdCulture)).Bold();
                    table.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text(invoiceList.Sum(x => x.Bucket2_31_60).ToString("N0", IdCulture)).Bold();
                    table.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text(invoiceList.Sum(x => x.Bucket3_61_90).ToString("N0", IdCulture)).Bold();
                    table.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text(invoiceList.Sum(x => x.Bucket4_91_120).ToString("N0", IdCulture)).Bold();
                    table.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text(invoiceList.Sum(x => x.Bucket5_Over120).ToString("N0", IdCulture)).Bold();
                    table.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("");
                });

                page.Footer().AlignRight().Text(x =>
                {
                    x.Span("Halaman ");
                    x.CurrentPageNumber();
                    x.Span(" dari ");
                    x.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    /// <summary>
    /// Memuat data seluruh invoice AR dan summary untuk penjamin tertentu pada tanggal AsOfDate.
    /// </summary>
    private async Task<(List<ReceivableAgingInvoiceItemResponse> Invoices, ReceivableAgingDetailSummaryResponse Summary)>
        LoadPayerInvoiceDataAsync(Guid payerId, DateOnly asOfDate, CancellationToken cancellationToken)
    {
        // 1. Resolve master penjamin
        string payerCode = string.Empty;
        string payerName = string.Empty;
        string payerType = "Penjamin";

        var company = await _dbContext.MstCompanyGuarantors.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == payerId && !x.IsDelete, cancellationToken);
        if (company != null)
        {
            payerCode = company.CompanyGuarantorCode;
            payerName = company.CompanyGuarantorName;
            payerType = "Perusahaan";
        }
        else
        {
            var ins = await _dbContext.MstInsuranceProviders.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == payerId && !x.IsDelete, cancellationToken);
            if (ins != null)
            {
                payerCode = ins.InsuranceProviderCode;
                payerName = ins.InsuranceProviderName;
                payerType = "Asuransi";
            }
        }

        if (string.IsNullOrWhiteSpace(payerName))
        {
            payerName = "Penjamin " + payerId.ToString()[..8].ToUpperInvariant();
            payerCode = "PEN-" + payerId.ToString()[..6].ToUpperInvariant();
        }

        // 2. Ambil seluruh batch tagihan AR penjamin yang dibuat/diterbitkan <= asOfDate
        var batchEntities = await _dbContext.FinReceivableInvoiceBatches.AsNoTracking()
            .Include(b => b.Items.Where(i => i.IsActiveMembership))
                .ThenInclude(i => i.Receivable)
            .Where(b => !b.IsDelete && b.DebtorReferenceId == payerId)
            .Where(b => (b.InvoiceDate ?? DateOnly.FromDateTime(b.CreateDateTime.Date)) <= asOfDate)
            .ToListAsync(cancellationToken);

        // 3. Ambil pembayaran / alokasi penerimaan aktif untuk piutang anggota batch
        var allMemberRecIds = batchEntities.SelectMany(b => b.Items).Select(i => i.ReceivableId).Distinct().ToList();
        var allocations = await _dbContext.FinReceiptAllocations.AsNoTracking()
            .Where(a => a.ReceivableId.HasValue && allMemberRecIds.Contains(a.ReceivableId.Value) && !a.IsDelete)
            .Where(a => DateOnly.FromDateTime(a.AllocatedAt.Date) <= asOfDate)
            .ToListAsync(cancellationToken);

        // 4. Hitung piutang belum ber-invoice untuk penjamin ini
        var invoicedRecIdSet = allMemberRecIds.ToHashSet();
        var uninvoicedRecs = await _dbContext.FinReceivables.AsNoTracking()
            .Where(r => !r.IsDelete && r.DebtorReferenceId == payerId && r.Status != FinReceivableStatuses.Cancelled && r.OutstandingAmount > 0)
            .Where(r => DateOnly.FromDateTime(r.RecognizedAt.Date) <= asOfDate)
            .Where(r => !invoicedRecIdSet.Contains(r.Id))
            .ToListAsync(cancellationToken);

        var invoices = new List<ReceivableAgingInvoiceItemResponse>();

        foreach (var b in batchEntities)
        {
            var invoiceDate = b.InvoiceDate ?? DateOnly.FromDateTime(b.CreateDateTime.Date);
            var dueDate = b.DueDate ?? (b.InvoiceDate?.AddDays(b.PaymentTermDays ?? 30) ?? b.PeriodEnd);
            var daysPastDue = asOfDate.DayNumber - dueDate.DayNumber;

            // Hitung total tagihan batch
            var totalAmount = b.TotalAmount;

            // Hitung pembayaran efektif per AsOfDate
            decimal paidAmount;
            if (asOfDate >= FinanceBusinessDate.Today())
            {
                paidAmount = b.Items.Where(i => i.Receivable != null).Sum(i => i.Receivable!.AllocatedAmount);
            }
            else
            {
                var batchRecIds = b.Items.Select(i => i.ReceivableId).ToHashSet();
                paidAmount = Math.Max(0m, allocations.Where(a => a.ReceivableId.HasValue && batchRecIds.Contains(a.ReceivableId.Value)).Sum(a => a.IsReversal ? -a.Amount : a.Amount));
            }

            var outstanding = Math.Max(0m, totalAmount - paidAmount);

            // Status pembayaran
            string paymentStatus;
            if (b.Status == FinReceivableInvoiceBatchStatuses.Cancelled)
            {
                paymentStatus = "Dibatalkan";
            }
            else if (outstanding <= 0m)
            {
                paymentStatus = "Lunas";
            }
            else if (paidAmount > 0m)
            {
                paymentStatus = "Dibayar Sebagian";
            }
            else
            {
                paymentStatus = "Belum Lunas";
            }

            // Bucket umur piutang
            decimal b1 = 0m, b2 = 0m, b3 = 0m, b4 = 0m, b5 = 0m;
            if (outstanding > 0m && b.Status != FinReceivableInvoiceBatchStatuses.Cancelled)
            {
                if (daysPastDue > 120) b5 = outstanding;
                else if (daysPastDue > 90) b4 = outstanding;
                else if (daysPastDue > 60) b3 = outstanding;
                else if (daysPastDue > 30) b2 = outstanding;
                else if (daysPastDue > 0) b1 = outstanding;
            }

            invoices.Add(new ReceivableAgingInvoiceItemResponse
            {
                BatchId = b.Id,
                InvoiceNumber = b.BatchNumber,
                InvoiceDate = invoiceDate,
                DueDate = dueDate,
                DaysPastDue = daysPastDue,
                HasDocument = b.Status != FinReceivableInvoiceBatchStatuses.Draft,
                TotalAmount = totalAmount,
                PaidAmount = paidAmount,
                OutstandingAmount = outstanding,
                Bucket1_0_30 = b1,
                Bucket2_31_60 = b2,
                Bucket3_61_90 = b3,
                Bucket4_91_120 = b4,
                Bucket5_Over120 = b5,
                PaymentStatus = paymentStatus,
                BatchStatus = b.Status
            });
        }

        // 5. Agregasi ke Summary Cards (Hanya batch aktif non-cancelled yang masuk kalkulasi portfolio)
        var activeInvoices = invoices.Where(x => x.BatchStatus != FinReceivableInvoiceBatchStatuses.Cancelled).ToList();

        decimal notDueAmount = 0m;
        foreach (var inv in activeInvoices)
        {
            if (inv.OutstandingAmount > 0m && inv.DaysPastDue <= 0)
            {
                notDueAmount += inv.OutstandingAmount;
            }
        }

        var summary = new ReceivableAgingDetailSummaryResponse
        {
            PayerId = payerId,
            PayerCode = payerCode,
            PayerName = payerName,
            PayerType = payerType,
            AsOfDate = asOfDate,

            TotalInvoice = activeInvoices.Count,
            TotalInvoiceAmount = activeInvoices.Sum(x => x.TotalAmount),
            TotalEffectivePayment = activeInvoices.Sum(x => x.PaidAmount),
            TotalOutstanding = activeInvoices.Sum(x => x.OutstandingAmount),
            UnpaidInvoiceCount = activeInvoices.Count(x => x.PaymentStatus == "Belum Lunas"),
            PartiallyPaidInvoiceCount = activeInvoices.Count(x => x.PaymentStatus == "Dibayar Sebagian"),
            PaidInvoiceCount = activeInvoices.Count(x => x.PaymentStatus == "Lunas"),

            NotDueAmount = notDueAmount,
            Bucket1_0_30 = activeInvoices.Sum(x => x.Bucket1_0_30),
            Bucket2_31_60 = activeInvoices.Sum(x => x.Bucket2_31_60),
            Bucket3_61_90 = activeInvoices.Sum(x => x.Bucket3_61_90),
            Bucket4_91_120 = activeInvoices.Sum(x => x.Bucket4_91_120),
            Bucket5_Over120 = activeInvoices.Sum(x => x.Bucket5_Over120),

            UninvoicedReceivableCount = uninvoicedRecs.Count,
            UninvoicedReceivableAmount = uninvoicedRecs.Sum(x => x.OutstandingAmount)
        };

        return (invoices, summary);
    }
}

