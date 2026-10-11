using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;

/// <summary>
/// Service read-only untuk Laporan AR (Billing AR Report).
/// Menghasilkan laporan tagihan AR berdasarkan invoice Billing yang telah lunas di sisi Billing,
/// dikelompokkan menurut perusahaan/asuransi/penjamin secara ascending, dengan pagination 30 baris/halaman
/// serta ekspor Excel dan PDF.
///
/// HANYA MEMBACA. Service ini tidak mengubah status transaksi dan tidak menimbulkan side effect pada database.
/// </summary>
public sealed class FinanceBillingArReportService
{
    private static readonly CultureInfo IdCulture = new("id-ID");
    private readonly ApplicationDbContext _dbContext;

    public FinanceBillingArReportService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Mengambil data Laporan AR terpaginasi (maksimal 30 baris per halaman) beserta pengelompokan dan ringkasan global.
    /// </summary>
    public async Task<BillingArReportPagedResponse> GetReportAsync(
        BillingArReportQuery query, CancellationToken cancellationToken)
    {
        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = Math.Clamp(query.PageSize, 1, 30); // Enforce strictly max 30 server-side

        var resolvedDate = ResolveDateRange(query);
        var (fromWhereClause, parameters) = BuildFromWhereClause(query, resolvedDate);

        // 1. Eksekusi ringkasan global (Summary) atas SELURUH hasil saringan, bukan hanya 30 baris halaman
        var summary = await ExecuteSummaryQueryAsync(fromWhereClause, parameters, query, resolvedDate.PeriodLabel, cancellationToken);

        // 2. Eksekusi 30 baris data halaman aktif
        var items = await ExecutePagedItemsQueryAsync(fromWhereClause, parameters, (pageNumber - 1) * pageSize, pageSize, cancellationToken);

        // 3. Bangun kelompok (Groups) dan subtotal tanggal untuk 30 baris pada halaman ini
        var groups = BuildPageGroups(items);

        var totalCount = summary.InvoiceCount;
        var totalPage = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

        return new BillingArReportPagedResponse
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = totalCount,
            TotalPage = totalPage,
            Items = items,
            Groups = groups,
            Summary = summary
        };
    }

    /// <summary>
    /// Ekspor seluruh hasil filter Laporan AR ke format Excel (.xlsx) dengan grouping, subtotal tanggal, dan total penjamin.
    /// </summary>
    public async Task<byte[]> ExportExcelAsync(BillingArReportQuery query, CancellationToken cancellationToken)
    {
        var resolvedDate = ResolveDateRange(query);
        var (fromWhereClause, parameters) = BuildFromWhereClause(query, resolvedDate);

        var allItems = await ExecutePagedItemsQueryAsync(fromWhereClause, parameters, 0, int.MaxValue, cancellationToken);
        var groups = BuildPageGroups(allItems);
        var summary = await ExecuteSummaryQueryAsync(fromWhereClause, parameters, query, resolvedDate.PeriodLabel, cancellationToken);

        using var workbook = new ClosedXML.Excel.XLWorkbook();
        var ws = workbook.Worksheets.Add("Laporan Billing AR");

        // Judul Laporan
        ws.Cell(1, 1).Value = "LAPORAN BILLING KE FINANCE AR";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;
        ws.Cell(1, 1).Style.Font.FontColor = ClosedXML.Excel.XLColor.FromHtml("#0d7f89");

        // Metadata Filter & Ringkasan Monitoring
        ws.Cell(2, 1).Value = $"Periode: {summary.PeriodLabel}  |  Penjamin: {summary.PayerName}  |  Jenis Kunjungan: {FormatVisitTypeLabel(query.VisitType)}";
        ws.Cell(2, 1).Style.Font.FontSize = 10;
        ws.Cell(2, 1).Style.Font.FontColor = ClosedXML.Excel.XLColor.FromHtml("#4b5563");

        var printTimeWib = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, FinanceBusinessDate.BusinessTimeZone);
        ws.Cell(3, 1).Value = $"Dicetak: {printTimeWib:dd/MM/yyyy HH:mm} WIB  |  Total: {summary.InvoiceCount} Invoice ({summary.PatientCount} Pasien)  |  Billing On Process: {summary.BillingOnProcessCount}  |  Handoff Berhasil: {summary.HandoffSuccessCount}  |  Invoice AR Terbit: {summary.InvoiceArCreatedCount}";
        ws.Cell(3, 1).Style.Font.FontSize = 9;
        ws.Cell(3, 1).Style.Font.Italic = true;
        ws.Cell(3, 1).Style.Font.FontColor = ClosedXML.Excel.XLColor.FromHtml("#6b7280");

        ws.Cell(4, 1).Value = $"Estimasi Belum Final: Rp {summary.TotalEstimatedAmount:N0}  |  Billing Final: Rp {summary.TotalFinalBillingAmount:N0}  |  Piutang Terbentuk: Rp {summary.TotalFinanceReceivableAmount:N0}";
        ws.Cell(4, 1).Style.Font.FontSize = 9;
        ws.Cell(4, 1).Style.Font.Bold = true;
        ws.Cell(4, 1).Style.Font.FontColor = ClosedXML.Excel.XLColor.FromHtml("#0d7f89");

        // Table Header di baris 6
        var headerRow = 6;
        string[] headers = [
            "NO", "TGL MASUK", "TGL KELUAR", "NO. REG", "NO. BILL", "NO. RM", "NAMA PASIEN",
            "UMUR", "LAYANAN", "POLIKLINIK / KAMAR", "TAGIHAN PENJAMIN", "TAGIHAN EXCESS",
            "STATUS BILLING", "STATUS FINANCE AR", "STATUS INVOICE AR"
        ];
        for (var c = 0; c < headers.Length; c++)
        {
            var cell = ws.Cell(headerRow, c + 1);
            cell.Value = headers[c];
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontSize = 10;
            cell.Style.Font.FontColor = ClosedXML.Excel.XLColor.FromHtml("#0d7f89");
            cell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#dff8fb");
            cell.Style.Alignment.Horizontal = (c == 0 || c == 1 || c == 2 || c == 7 || c == 8 || c >= 12)
                ? ClosedXML.Excel.XLAlignmentHorizontalValues.Center
                : (c == 10 || c == 11)
                ? ClosedXML.Excel.XLAlignmentHorizontalValues.Right
                : ClosedXML.Excel.XLAlignmentHorizontalValues.Left;
            cell.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
            cell.Style.Border.OutsideBorderColor = ClosedXML.Excel.XLColor.FromHtml("#8adce4");
        }

        var r = headerRow + 1;
        var globalIndex = 1;

        foreach (var group in groups)
        {
            // Baris Header Kelompok (Group Header) - Colspan 15
            var groupHeaderRange = ws.Range(r, 1, r, 15);
            groupHeaderRange.Merge();
            groupHeaderRange.Value = group.PayerName.ToUpperInvariant();
            groupHeaderRange.Style.Font.Bold = true;
            groupHeaderRange.Style.Font.FontSize = 10;
            groupHeaderRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#d9d9d9");
            groupHeaderRange.Style.Font.FontColor = ClosedXML.Excel.XLColor.FromHtml("#071323");
            groupHeaderRange.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
            groupHeaderRange.Style.Border.OutsideBorderColor = ClosedXML.Excel.XLColor.FromHtml("#b5bec7");
            r++;

            // Kelompokkan item di dalam grup berdasarkan tanggal WIB
            var dateMap = new Dictionary<string, List<BillingArReportItemResponse>>();
            foreach (var item in group.Items)
            {
                var dKey = item.BillingDate.HasValue
                    ? TimeZoneInfo.ConvertTime(item.BillingDate.Value, FinanceBusinessDate.BusinessTimeZone).ToString("yyyy-MM-dd")
                    : "Lainnya";
                if (!dateMap.TryGetValue(dKey, out var dList))
                {
                    dList = new List<BillingArReportItemResponse>();
                    dateMap[dKey] = dList;
                }
                dList.Add(item);
            }

            foreach (var (dateKey, dateItems) in dateMap)
            {
                decimal dateSubtotal = 0m;
                string dateLabel = dateKey;

                foreach (var item in dateItems)
                {
                    dateSubtotal += item.CoveredBillingAmount;

                    ws.Cell(r, 1).Value = globalIndex++;
                    ws.Cell(r, 1).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

                    ws.Cell(r, 2).Value = item.VisitDate.HasValue ? item.VisitDate.Value.ToString("dd/MM/yyyy") : "-";
                    ws.Cell(r, 2).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

                    ws.Cell(r, 3).Value = item.DischargeDate.HasValue ? item.DischargeDate.Value.ToString("dd/MM/yyyy HH:mm") : "-";
                    ws.Cell(r, 3).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

                    ws.Cell(r, 4).Value = item.RegistrationNumber;
                    ws.Cell(r, 5).Value = item.InvoiceNumber;
                    ws.Cell(r, 6).Value = item.MedicalRecordNumber;
                    ws.Cell(r, 7).Value = item.PatientName;
                    ws.Cell(r, 8).Value = item.PatientAgeText;
                    ws.Cell(r, 9).Value = item.VisitTypeLabel;
                    ws.Cell(r, 10).Value = item.ClinicOrRoomName;

                    ws.Cell(r, 11).Value = item.CoveredBillingAmount;
                    ws.Cell(r, 11).Style.NumberFormat.Format = "#,##0";
                    ws.Cell(r, 11).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Right;

                    ws.Cell(r, 12).Value = item.ExcessAmount;
                    ws.Cell(r, 12).Style.NumberFormat.Format = "#,##0";
                    ws.Cell(r, 12).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Right;

                    ws.Cell(r, 13).Value = item.BillingStatusLabel + (item.IsEstimated ? " (Estimasi)" : "");
                    ws.Cell(r, 13).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

                    ws.Cell(r, 14).Value = item.FinanceHandoffStatusLabel;
                    ws.Cell(r, 14).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

                    ws.Cell(r, 15).Value = item.ArInvoiceStatusLabel;
                    ws.Cell(r, 15).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

                    for (var c = 1; c <= 15; c++)
                    {
                        ws.Cell(r, c).Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                        ws.Cell(r, c).Style.Border.OutsideBorderColor = ClosedXML.Excel.XLColor.FromHtml("#e5e7eb");
                    }
                    r++;

                    if (item.BillingDate.HasValue)
                    {
                        dateLabel = TimeZoneInfo.ConvertTime(item.BillingDate.Value, FinanceBusinessDate.BusinessTimeZone).ToString("dd/MM/yyyy");
                    }
                }

                // Subtotal per Tanggal
                var subtotalLabelRange = ws.Range(r, 1, r, 10);
                subtotalLabelRange.Merge();
                subtotalLabelRange.Value = $"Total {dateLabel} :";
                subtotalLabelRange.Style.Font.Bold = true;
                subtotalLabelRange.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Right;
                subtotalLabelRange.Style.Font.FontColor = ClosedXML.Excel.XLColor.FromHtml("#0d7f89");
                subtotalLabelRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#dff8fb");

                var subtotalValCell = ws.Cell(r, 11);
                subtotalValCell.Value = dateSubtotal;
                subtotalValCell.Style.Font.Bold = true;
                subtotalValCell.Style.NumberFormat.Format = "#,##0";
                subtotalValCell.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Right;
                subtotalValCell.Style.Font.FontColor = ClosedXML.Excel.XLColor.FromHtml("#0d7f89");
                subtotalValCell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#dff8fb");

                for (var c = 1; c <= 15; c++)
                {
                    ws.Cell(r, c).Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                    ws.Cell(r, c).Style.Border.OutsideBorderColor = ClosedXML.Excel.XLColor.FromHtml("#8adce4");
                }
                r++;
            }

            // Total Kelompok (Group Total)
            var groupTotalLabelRange = ws.Range(r, 1, r, 10);
            groupTotalLabelRange.Merge();
            groupTotalLabelRange.Value = $"Total {group.PayerName} :";
            groupTotalLabelRange.Style.Font.Bold = true;
            groupTotalLabelRange.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Right;
            groupTotalLabelRange.Style.Font.FontColor = ClosedXML.Excel.XLColor.FromHtml("#071323");
            groupTotalLabelRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#e6f7f8");

            var groupTotalValCell = ws.Cell(r, 11);
            groupTotalValCell.Value = group.GroupTotal;
            groupTotalValCell.Style.Font.Bold = true;
            groupTotalValCell.Style.NumberFormat.Format = "#,##0";
            groupTotalValCell.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Right;
            groupTotalValCell.Style.Font.FontColor = ClosedXML.Excel.XLColor.FromHtml("#071323");
            groupTotalValCell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#e6f7f8");

            for (var c = 1; c <= 15; c++)
            {
                ws.Cell(r, c).Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                ws.Cell(r, c).Style.Border.OutsideBorderColor = ClosedXML.Excel.XLColor.FromHtml("#8adce4");
            }
            r++;
        }

        // Baris Grand Total di akhir laporan
        if (summary.InvoiceCount > 0)
        {
            var grandTotalLabelRange = ws.Range(r, 1, r, 10);
            grandTotalLabelRange.Merge();
            grandTotalLabelRange.Value = "GRAND TOTAL :";
            grandTotalLabelRange.Style.Font.Bold = true;
            grandTotalLabelRange.Style.Font.FontSize = 11;
            grandTotalLabelRange.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Right;
            grandTotalLabelRange.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
            grandTotalLabelRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#0d7f89");

            var grandTotalValCell = ws.Cell(r, 11);
            grandTotalValCell.Value = summary.TotalAmount;
            grandTotalValCell.Style.Font.Bold = true;
            grandTotalValCell.Style.Font.FontSize = 11;
            grandTotalValCell.Style.NumberFormat.Format = "#,##0";
            grandTotalValCell.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Right;
            grandTotalValCell.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
            grandTotalValCell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#0d7f89");

            for (var c = 1; c <= 15; c++)
            {
                ws.Cell(r, c).Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Medium;
                ws.Cell(r, c).Style.Border.OutsideBorderColor = ClosedXML.Excel.XLColor.FromHtml("#085b63");
            }
        }

        ws.Columns().AdjustToContents();
        ws.Column(1).Width = 6;
        ws.Column(2).Width = 14;
        ws.Column(3).Width = 16;
        ws.Column(4).Width = 18;
        ws.Column(5).Width = 20;
        ws.Column(6).Width = 14;
        ws.Column(7).Width = 24;
        ws.Column(8).Width = 10;
        ws.Column(9).Width = 14;
        ws.Column(10).Width = 20;
        ws.Column(11).Width = 18;
        ws.Column(12).Width = 16;
        ws.Column(13).Width = 16;
        ws.Column(14).Width = 18;
        ws.Column(15).Width = 18;

        ws.SheetView.FreezeRows(headerRow);

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    /// <summary>
    /// Ekspor seluruh hasil filter Laporan Billing ke Finance AR ke format PDF (A4 Landscape) dengan grouping, subtotal tanggal, dan total penjamin.
    /// </summary>
    public async Task<byte[]> ExportPdfAsync(BillingArReportQuery query, CancellationToken cancellationToken)
    {
        var resolvedDate = ResolveDateRange(query);
        var (fromWhereClause, parameters) = BuildFromWhereClause(query, resolvedDate);

        var allItems = await ExecutePagedItemsQueryAsync(fromWhereClause, parameters, 0, int.MaxValue, cancellationToken);
        var groups = BuildPageGroups(allItems);
        var summary = await ExecuteSummaryQueryAsync(fromWhereClause, parameters, query, resolvedDate.PeriodLabel, cancellationToken);

        var printTimeWib = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, FinanceBusinessDate.BusinessTimeZone);

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(18);
                page.DefaultTextStyle(x => x.FontSize(7.5f).FontFamily("Arial"));

                page.Header().Column(col =>
                {
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Column(c =>
                        {
                            c.Item().Text("RS MMC - LAPORAN BILLING KE FINANCE AR").FontSize(12).Bold().FontColor("#0d7f89");
                            c.Item().Text($"Monitoring Siklus Tagihan Billing ke Piutang & Invoice AR  |  Periode: {summary.PeriodLabel}  |  Penjamin: {summary.PayerName}")
                                .FontSize(7.5f).FontColor("#4b5563");
                        });
                        r.ConstantItem(260).AlignRight().Column(c =>
                        {
                            c.Item().Text($"Dicetak: {printTimeWib:dd/MM/yyyy HH:mm} WIB  |  Total: {summary.InvoiceCount} Inv ({summary.PatientCount} Pasien)").FontSize(7.5f).FontColor("#6b7280");
                            c.Item().Text($"On Process: {summary.BillingOnProcessCount} | Handoff Sukses: {summary.HandoffSuccessCount} | Inv AR: {summary.InvoiceArCreatedCount}").FontSize(7.5f).Bold().FontColor("#0d7f89");
                        });
                    });

                    col.Item().PaddingTop(3).PaddingBottom(5).LineHorizontal(1).LineColor("#8adce4");
                });

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.ConstantColumn(22);   // NO
                        cols.ConstantColumn(58);   // TGL MASUK
                        cols.RelativeColumn(1.3f); // NO. REG
                        cols.ConstantColumn(54);   // NO. RM
                        cols.RelativeColumn(1.7f); // PASIEN
                        cols.ConstantColumn(40);   // UMUR
                        cols.ConstantColumn(52);   // LAYANAN
                        cols.RelativeColumn(1.3f); // POLI/KAMAR
                        cols.ConstantColumn(72);   // TAGIHAN
                        cols.ConstantColumn(55);   // ST. BILLING
                        cols.ConstantColumn(68);   // ST. FINANCE
                        cols.ConstantColumn(65);   // ST. INV AR
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background("#dff8fb").Padding(3).AlignCenter().Text("NO").Bold().FontColor("#0d7f89");
                        header.Cell().Background("#dff8fb").Padding(3).AlignCenter().Text("TGL MASUK").Bold().FontColor("#0d7f89");
                        header.Cell().Background("#dff8fb").Padding(3).Text("NO. REG").Bold().FontColor("#0d7f89");
                        header.Cell().Background("#dff8fb").Padding(3).Text("NO. RM").Bold().FontColor("#0d7f89");
                        header.Cell().Background("#dff8fb").Padding(3).Text("PASIEN").Bold().FontColor("#0d7f89");
                        header.Cell().Background("#dff8fb").Padding(3).AlignCenter().Text("UMUR").Bold().FontColor("#0d7f89");
                        header.Cell().Background("#dff8fb").Padding(3).AlignCenter().Text("LAYANAN").Bold().FontColor("#0d7f89");
                        header.Cell().Background("#dff8fb").Padding(3).Text("POLI/KAMAR").Bold().FontColor("#0d7f89");
                        header.Cell().Background("#dff8fb").Padding(3).AlignRight().Text("TAGIHAN").Bold().FontColor("#0d7f89");
                        header.Cell().Background("#dff8fb").Padding(3).AlignCenter().Text("ST. BILLING").Bold().FontColor("#0d7f89");
                        header.Cell().Background("#dff8fb").Padding(3).AlignCenter().Text("ST. FINANCE").Bold().FontColor("#0d7f89");
                        header.Cell().Background("#dff8fb").Padding(3).AlignCenter().Text("ST. INV AR").Bold().FontColor("#0d7f89");
                    });

                    var globalIndex = 1;

                    foreach (var group in groups)
                    {
                        // Group Header (colspan 12)
                        table.Cell().ColumnSpan(12).Background("#d9d9d9").PaddingVertical(3).PaddingHorizontal(5)
                            .Text(group.PayerName.ToUpperInvariant()).Bold().FontSize(8f).FontColor("#071323");

                        // Kelompokkan per tanggal
                        var dateMap = new Dictionary<string, List<BillingArReportItemResponse>>();
                        foreach (var item in group.Items)
                        {
                            var dKey = item.BillingDate.HasValue
                                ? TimeZoneInfo.ConvertTime(item.BillingDate.Value, FinanceBusinessDate.BusinessTimeZone).ToString("yyyy-MM-dd")
                                : "Lainnya";
                            if (!dateMap.TryGetValue(dKey, out var dList))
                            {
                                dList = new List<BillingArReportItemResponse>();
                                dateMap[dKey] = dList;
                            }
                            dList.Add(item);
                        }

                        foreach (var (dateKey, dateItems) in dateMap)
                        {
                            decimal dateSubtotal = 0m;
                            string dateLabel = dateKey;

                            foreach (var item in dateItems)
                            {
                                dateSubtotal += item.CoveredBillingAmount;
                                var isEven = globalIndex % 2 == 0;
                                var bg = isEven ? "#f9fafb" : "#ffffff";

                                table.Cell().Background(bg).Padding(2.5f).AlignCenter().Text(globalIndex.ToString());
                                var visitDisplay = item.VisitDate.HasValue ? item.VisitDate.Value.ToString("dd/MM/yyyy") : "-";
                                table.Cell().Background(bg).Padding(2.5f).AlignCenter().Text(visitDisplay);
                                table.Cell().Background(bg).Padding(2.5f).Text(item.RegistrationNumber);
                                table.Cell().Background(bg).Padding(2.5f).Text(item.MedicalRecordNumber);
                                table.Cell().Background(bg).Padding(2.5f).Text(item.PatientName).Bold();
                                table.Cell().Background(bg).Padding(2.5f).AlignCenter().Text(item.PatientAgeText);
                                table.Cell().Background(bg).Padding(2.5f).AlignCenter().Text(item.VisitTypeLabel);
                                table.Cell().Background(bg).Padding(2.5f).Text(item.ClinicOrRoomName);
                                
                                var amtText = item.CoveredBillingAmount.ToString("N0", IdCulture);
                                if (item.IsEstimated) amtText += "*";
                                table.Cell().Background(bg).Padding(2.5f).AlignRight().Text(amtText).Bold();

                                table.Cell().Background(bg).Padding(2.5f).AlignCenter().Text(item.BillingStatusLabel);
                                table.Cell().Background(bg).Padding(2.5f).AlignCenter().Text(item.FinanceHandoffStatusLabel);
                                table.Cell().Background(bg).Padding(2.5f).AlignCenter().Text(item.ArInvoiceStatusLabel);

                                globalIndex++;
                                if (item.BillingDate.HasValue)
                                {
                                    dateLabel = TimeZoneInfo.ConvertTime(item.BillingDate.Value, FinanceBusinessDate.BusinessTimeZone).ToString("dd/MM/yyyy");
                                }
                            }

                            // Subtotal per Tanggal
                            table.Cell().ColumnSpan(8).Background("#dff8fb").Padding(2.5f).AlignRight()
                                .Text($"Total {dateLabel} :").Bold().FontColor("#0d7f89");
                            table.Cell().Background("#dff8fb").Padding(2.5f).AlignRight()
                                .Text(dateSubtotal.ToString("N0", IdCulture)).Bold().FontColor("#0d7f89");
                            table.Cell().ColumnSpan(3).Background("#dff8fb").Padding(2.5f).Text("");
                        }

                        // Total Kelompok (Group Total)
                        table.Cell().ColumnSpan(8).Background("#e6f7f8").Padding(2.5f).AlignRight()
                            .Text($"Total {group.PayerName} :").Bold().FontColor("#071323");
                        table.Cell().Background("#e6f7f8").Padding(2.5f).AlignRight()
                            .Text(group.GroupTotal.ToString("N0", IdCulture)).Bold().FontColor("#071323");
                        table.Cell().ColumnSpan(3).Background("#e6f7f8").Padding(2.5f).Text("");
                    }

                    // Grand Total
                    if (summary.InvoiceCount > 0)
                    {
                        table.Cell().ColumnSpan(8).Background("#0d7f89").Padding(3.5f).AlignRight()
                            .Text("GRAND TOTAL :").Bold().FontSize(8.5f).FontColor("#ffffff");
                        table.Cell().Background("#0d7f89").Padding(3.5f).AlignRight()
                            .Text(summary.TotalAmount.ToString("N0", IdCulture)).Bold().FontSize(8.5f).FontColor("#ffffff");
                        table.Cell().ColumnSpan(3).Background("#0d7f89").Padding(3.5f).Text("");
                    }
                    else
                    {
                        table.Cell().ColumnSpan(12).Padding(12).AlignCenter()
                            .Text("Tidak ada data tagihan yang memenuhi saringan.").Italic().FontColor("#6b7280");
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Halaman ");
                    x.CurrentPageNumber();
                    x.Span(" dari ");
                    x.TotalPages();
                });
            });
        }).GeneratePdf();
    }

    /// <summary>
    /// Pilihan opsi penjamin (perusahaan atau asuransi) untuk dropdown saringan Laporan AR.
    /// </summary>
    public async Task<List<BillingDataPayerOptionResponse>> GetPayerOptionsAsync(
        string? search, int limit, CancellationToken cancellationToken)
    {
        var clampedLimit = Math.Clamp(limit, 1, 50);
        var s = search?.Trim();

        var insurance = _dbContext.MstInsuranceProviders.AsNoTracking().Where(x => !x.IsDelete && x.IsActive);
        var companies = _dbContext.MstCompanyGuarantors.AsNoTracking().Where(x => !x.IsDelete && x.IsActive);

        if (!string.IsNullOrEmpty(s))
        {
            var pattern = $"%{EscapeLike(s)}%";
            insurance = insurance.Where(x => EF.Functions.ILike(x.InsuranceProviderName, pattern, "\\"));
            companies = companies.Where(x => EF.Functions.ILike(x.CompanyGuarantorName, pattern, "\\"));
        }

        var insuranceList = await insurance
            .OrderBy(x => x.InsuranceProviderName)
            .Take(clampedLimit)
            .Select(x => new BillingDataPayerOptionResponse
            {
                Id = x.Id,
                Name = x.InsuranceProviderName,
                Kind = BillingDataPayerKinds.Insurance
            })
            .ToListAsync(cancellationToken);

        var companyList = await companies
            .OrderBy(x => x.CompanyGuarantorName)
            .Take(clampedLimit)
            .Select(x => new BillingDataPayerOptionResponse
            {
                Id = x.Id,
                Name = x.CompanyGuarantorName,
                Kind = BillingDataPayerKinds.Company
            })
            .ToListAsync(cancellationToken);

        return insuranceList.Concat(companyList)
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.Kind, StringComparer.Ordinal)
            .Take(clampedLimit)
            .ToList();
    }

    // ------------------------------------------------------------------------------------
    // Helper Query SQL & Pemetaan
    // ------------------------------------------------------------------------------------

    private (string FromWhereClause, List<NpgsqlParameter> Parameters) BuildFromWhereClause(
        BillingArReportQuery query, ResolvedDateRange resolvedDate)
    {
        var sb = new StringBuilder();
        var parameters = new List<NpgsqlParameter>();

        sb.AppendLine(@"
FROM ""BilInvoice"" i
JOIN ""RegPatientEncounter"" e ON i.""EncounterId"" = e.""Id""
LEFT JOIN ""MstPatient"" p ON e.""PatientId"" = p.""Id""
LEFT JOIN ""MstClinic"" cl ON e.""ClinicId"" = cl.""Id"" AND NOT cl.""IsDelete""
LEFT JOIN ""MstRoom"" rm ON e.""RoomId"" = rm.""Id"" AND NOT rm.""IsDelete""
LEFT JOIN ""MstServiceUnit"" su ON e.""ServiceUnitId"" = su.""Id"" AND NOT su.""IsDelete""
LEFT JOIN LATERAL (
    SELECT h2.""Id"", h2.""DebtorReferenceId"", h2.""Amount"", h2.""Status""
    FROM ""BilArHandoff"" h2
    WHERE h2.""InvoiceId"" = i.""Id"" AND h2.""DebtorType"" = 'PAYER' AND NOT h2.""IsDelete""
    LIMIT 1
) h ON true
LEFT JOIN LATERAL (
    SELECT intake2.""Id"", intake2.""Status"", intake2.""ErrorMessage""
    FROM ""FinBillingHandoffIntake"" intake2
    WHERE intake2.""SourceHandoffId"" = h.""Id"" AND intake2.""HandoffType"" = 'AR' AND NOT intake2.""IsDelete""
    ORDER BY intake2.""CreateDateTime"" DESC, intake2.""Id"" DESC
    LIMIT 1
) intake ON true
LEFT JOIN LATERAL (
    SELECT r2.""Id"", r2.""ReceivableNumber"", r2.""DebtorReferenceId"", r2.""OriginalAmount""
    FROM ""FinReceivable"" r2
    WHERE r2.""InvoiceId"" = i.""Id"" AND r2.""DebtorType"" = 'PAYER' AND NOT r2.""IsDelete""
    LIMIT 1
) r ON true
LEFT JOIN LATERAL (
    SELECT cv2.""PrimaryAmount"", cv2.""ExcessAmount""
    FROM ""BilCalculationVersion"" cv2
    WHERE cv2.""InvoiceId"" = i.""Id"" AND cv2.""VersionNo"" = i.""CurrentCalculationVersion""
    LIMIT 1
) cv ON true
LEFT JOIN LATERAL (
    SELECT COALESCE(SUM(bi_item.""Quantity"" * bi_item.""UnitPrice""), 0) AS ""ItemsTotal""
    FROM ""BilInvoiceItem"" bi_item
    WHERE bi_item.""InvoiceId"" = i.""Id"" AND NOT bi_item.""IsDelete"" AND bi_item.""Status"" <> 'VOIDED'
) items_sum ON true
LEFT JOIN LATERAL (
    SELECT g2.""InsuranceProviderId"", g2.""CompanyGuarantorId"", g2.""PaymentSourceNameSnapshot""
    FROM ""RegPatientEncounterGuarantor"" g2
    WHERE g2.""EncounterId"" = i.""EncounterId"" AND g2.""IsActive"" AND NOT g2.""IsDelete"" AND NOT g2.""IsCancel""
    ORDER BY g2.""IsPrimary"" DESC, g2.""Priority"" ASC
    LIMIT 1
) g ON true
LEFT JOIN LATERAL (
    SELECT bi2.""BatchId"", batch2.""Status"" AS ""BatchStatus"", batch2.""BatchNumber""
    FROM ""FinReceivableInvoiceBatchItem"" bi2
    JOIN ""FinReceivableInvoiceBatch"" batch2 ON bi2.""BatchId"" = batch2.""Id"" AND NOT batch2.""IsDelete""
    WHERE bi2.""ReceivableId"" = r.""Id"" AND NOT bi2.""IsDelete""
    ORDER BY bi2.""IsActiveMembership"" DESC, batch2.""CreateDateTime"" DESC, batch2.""Id"" DESC
    LIMIT 1
) batch_info ON true
WHERE NOT i.""IsDelete""
  AND (i.""Status"" = 'OPEN' OR COALESCE(r.""OriginalAmount"", h.""Amount"", cv.""PrimaryAmount"" + cv.""ExcessAmount"", items_sum.""ItemsTotal"", 0) > 0)");

        // 1. Filter Penjamin (PayerId)
        if (query.PayerId.HasValue && query.PayerId.Value != Guid.Empty)
        {
            sb.AppendLine(@"  AND COALESCE(r.""DebtorReferenceId"", h.""DebtorReferenceId"", g.""InsuranceProviderId"", g.""CompanyGuarantorId"") = @payerId");
            parameters.Add(new NpgsqlParameter("payerId", query.PayerId.Value));
        }

        // 2. Filter Jenis Kunjungan (VisitType)
        if (!string.IsNullOrWhiteSpace(query.VisitType) && !query.VisitType.Equals("ALL", StringComparison.OrdinalIgnoreCase))
        {
            var vType = query.VisitType.Trim().ToUpperInvariant();
            if (vType == "OP" || vType == "OUTPATIENT")
            {
                sb.AppendLine(@"  AND e.""EncounterType"" = 1");
            }
            else if (vType == "IP" || vType == "INPATIENT")
            {
                sb.AppendLine(@"  AND e.""EncounterType"" = 3");
            }
            else if (vType == "IGD" || vType == "EMERGENCY")
            {
                sb.AppendLine(@"  AND e.""EncounterType"" = 2");
            }
        }

        // 3. Filter Status Billing (BillingStatus)
        if (!string.IsNullOrWhiteSpace(query.BillingStatus) && !query.BillingStatus.Equals("ALL", StringComparison.OrdinalIgnoreCase))
        {
            var bStatus = query.BillingStatus.Trim().ToUpperInvariant();
            sb.AppendLine(@"  AND i.""Status"" = @billingStatus");
            parameters.Add(new NpgsqlParameter("billingStatus", bStatus));
        }

        // 4. Filter Status Integrasi Finance AR (FinanceHandoffStatus)
        if (!string.IsNullOrWhiteSpace(query.FinanceHandoffStatus) && !query.FinanceHandoffStatus.Equals("ALL", StringComparison.OrdinalIgnoreCase))
        {
            var fStatus = query.FinanceHandoffStatus.Trim().ToUpperInvariant();
            if (fStatus == "SUCCESS")
            {
                sb.AppendLine(@"  AND r.""Id"" IS NOT NULL");
            }
            else if (fStatus == "FAILED")
            {
                sb.AppendLine(@"  AND r.""Id"" IS NULL AND intake.""Status"" = 'ERROR'");
            }
            else if (fStatus == "PENDING")
            {
                sb.AppendLine(@"  AND r.""Id"" IS NULL AND intake.""Status"" != 'ERROR' AND (h.""Status"" = 'CREATED' OR intake.""Status"" = 'NEW')");
            }
            else if (fStatus == "NOT_HANDED_OFF" || fStatus == "NOTHANDEDOFF")
            {
                sb.AppendLine(@"  AND r.""Id"" IS NULL AND i.""Status"" = 'OPEN'");
            }
            else if (fStatus == "NOT_APPLICABLE" || fStatus == "NOTAPPLICABLE")
            {
                sb.AppendLine(@"  AND r.""Id"" IS NULL AND intake.""Id"" IS NULL AND h.""Id"" IS NULL AND i.""Status"" != 'OPEN'");
            }
        }

        // 5. Filter Status Pembuatan Invoice AR (ArInvoiceStatus)
        if (!string.IsNullOrWhiteSpace(query.ArInvoiceStatus) && !query.ArInvoiceStatus.Equals("ALL", StringComparison.OrdinalIgnoreCase))
        {
            var status = query.ArInvoiceStatus.Trim().ToUpperInvariant();
            if (status == "CREATED" || status == "ISSUED")
            {
                sb.AppendLine(@"  AND batch_info.""BatchId"" IS NOT NULL AND batch_info.""BatchStatus"" IN ('ISSUED', 'PARTIALLY_PAID', 'PAID')");
            }
            else if (status == "DRAFT")
            {
                sb.AppendLine(@"  AND batch_info.""BatchId"" IS NOT NULL AND batch_info.""BatchStatus"" = 'DRAFT'");
            }
            else if (status == "CANCELLED")
            {
                sb.AppendLine(@"  AND batch_info.""BatchId"" IS NOT NULL AND batch_info.""BatchStatus"" = 'CANCELLED'");
            }
            else if (status == "NOT_CREATED" || status == "NOTCREATED")
            {
                sb.AppendLine(@"  AND (r.""Id"" IS NULL OR batch_info.""BatchId"" IS NULL OR batch_info.""BatchStatus"" NOT IN ('ISSUED', 'PARTIALLY_PAID', 'PAID', 'DRAFT'))");
            }
        }

        // 6. Filter Rentang Tanggal
        if (resolvedDate.FromUtc.HasValue)
        {
            sb.AppendLine(@"  AND COALESCE(i.""InvoiceDate"", e.""EncounterDate"") >= @fromUtc");
            parameters.Add(new NpgsqlParameter("fromUtc", resolvedDate.FromUtc.Value));
        }
        if (resolvedDate.ToExclusiveUtc.HasValue)
        {
            sb.AppendLine(@"  AND COALESCE(i.""InvoiceDate"", e.""EncounterDate"") < @toExclusiveUtc");
            parameters.Add(new NpgsqlParameter("toExclusiveUtc", resolvedDate.ToExclusiveUtc.Value));
        }

        // 7. Filter Pencarian (Search)
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = $"%{EscapeLike(query.Search.Trim())}%";
            sb.AppendLine(@"  AND (
      i.""InvoiceNumber"" ILIKE @searchPattern
      OR e.""EncounterNumber"" ILIKE @searchPattern
      OR p.""FullName"" ILIKE @searchPattern
      OR p.""MedicalRecordNumber"" ILIKE @searchPattern
      OR COALESCE(
          (SELECT ins.""InsuranceProviderName"" FROM ""MstInsuranceProvider"" ins WHERE ins.""Id"" = COALESCE(r.""DebtorReferenceId"", h.""DebtorReferenceId"", g.""InsuranceProviderId"", g.""CompanyGuarantorId"") AND NOT ins.""IsDelete"" LIMIT 1),
          (SELECT comp.""CompanyGuarantorName"" FROM ""MstCompanyGuarantor"" comp WHERE comp.""Id"" = COALESCE(r.""DebtorReferenceId"", h.""DebtorReferenceId"", g.""InsuranceProviderId"", g.""CompanyGuarantorId"") AND NOT comp.""IsDelete"" LIMIT 1),
          g.""PaymentSourceNameSnapshot"",
          'Lainnya'
      ) ILIKE @searchPattern
  )");
            parameters.Add(new NpgsqlParameter("searchPattern", pattern));
        }

        return (sb.ToString(), parameters);
    }

    private async Task<BillingArReportSummaryResponse> ExecuteSummaryQueryAsync(
        string fromWhereClause,
        List<NpgsqlParameter> parameters,
        BillingArReportQuery query,
        string periodLabel,
        CancellationToken cancellationToken)
    {
        var sql = $@"
SELECT 
    COUNT(*)::int AS total_count,
    COUNT(DISTINCT p.""Id"")::int AS patient_count,
    COUNT(*) FILTER (WHERE i.""Status"" = 'OPEN')::int AS billing_on_process_count,
    COUNT(*) FILTER (WHERE i.""Status"" IN ('CLOSED', 'FINAL', 'SETTLED_BY_WRITE_OFF'))::int AS billing_closed_count,
    COUNT(*) FILTER (WHERE r.""Id"" IS NULL AND (intake.""Status"" = 'NEW' OR h.""Status"" = 'CREATED'))::int AS handoff_pending_count,
    COUNT(*) FILTER (WHERE r.""Id"" IS NOT NULL)::int AS handoff_success_count,
    COUNT(*) FILTER (WHERE r.""Id"" IS NULL OR batch_info.""BatchId"" IS NULL OR batch_info.""BatchStatus"" NOT IN ('ISSUED', 'PARTIALLY_PAID', 'PAID', 'DRAFT'))::int AS invoice_ar_not_created_count,
    COUNT(*) FILTER (WHERE batch_info.""BatchId"" IS NOT NULL AND batch_info.""BatchStatus"" IN ('ISSUED', 'PARTIALLY_PAID', 'PAID', 'DRAFT'))::int AS invoice_ar_created_count,
    COALESCE(SUM(CASE WHEN i.""Status"" = 'OPEN' THEN COALESCE(cv.""PrimaryAmount"", items_sum.""ItemsTotal"", 0) ELSE 0 END), 0) AS total_estimated_amount,
    COALESCE(SUM(CASE WHEN i.""Status"" IN ('FINAL', 'CLOSED', 'SETTLED_BY_WRITE_OFF') THEN COALESCE(r.""OriginalAmount"", h.""Amount"", cv.""PrimaryAmount"", 0) ELSE 0 END), 0) AS total_final_billing_amount,
    COALESCE(SUM(COALESCE(r.""OriginalAmount"", 0)), 0) AS total_finance_receivable_amount,
    COALESCE(SUM(COALESCE(r.""OriginalAmount"", h.""Amount"", cv.""PrimaryAmount"", items_sum.""ItemsTotal"", 0)), 0) AS total_amount
{fromWhereClause};";

        var conn = _dbContext.Database.GetDbConnection();
        var wasClosed = conn.State != ConnectionState.Open;
        if (wasClosed) await conn.OpenAsync(cancellationToken);

        try
        {
            // Baca dan simpan seluruh nilai kolom ke variabel lokal di dalam cakupan eksplisit ini,
            // supaya cmd/reader benar-benar di-dispose (await using) sebelum ResolvePayerNameAsync
            // (query EF Core pada _dbContext yang memakai koneksi ADO.NET yang sama) dipanggil di
            // bawah. Memanggilnya selagi reader masih terbuka berisiko
            // NpgsqlOperationInProgressException pada koneksi yang tidak mendukung command ganda.
            bool hasRow;
            int invCount = 0, patCount = 0, onProcessCount = 0, closedCount = 0,
                handoffPending = 0, handoffSuccess = 0, arNotCreated = 0, arCreated = 0;
            decimal estAmount = 0m, finalAmount = 0m, recAmount = 0m, totAmount = 0m;

            {
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                foreach (var p in parameters)
                {
                    cmd.Parameters.Add(p.Clone());
                }

                await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
                hasRow = await reader.ReadAsync(cancellationToken);
                if (hasRow)
                {
                    invCount = Convert.ToInt32(reader.GetValue(0));
                    patCount = Convert.ToInt32(reader.GetValue(1));
                    onProcessCount = Convert.ToInt32(reader.GetValue(2));
                    closedCount = Convert.ToInt32(reader.GetValue(3));
                    handoffPending = Convert.ToInt32(reader.GetValue(4));
                    handoffSuccess = Convert.ToInt32(reader.GetValue(5));
                    arNotCreated = Convert.ToInt32(reader.GetValue(6));
                    arCreated = Convert.ToInt32(reader.GetValue(7));
                    estAmount = Convert.ToDecimal(reader.GetValue(8));
                    finalAmount = Convert.ToDecimal(reader.GetValue(9));
                    recAmount = Convert.ToDecimal(reader.GetValue(10));
                    totAmount = Convert.ToDecimal(reader.GetValue(11));
                }
            }

            if (!hasRow)
            {
                return new BillingArReportSummaryResponse
                {
                    PayerName = "Semua Perusahaan / Asuransi",
                    PeriodLabel = periodLabel
                };
            }

            string payerDisplayName = "Semua Perusahaan / Asuransi";
            if (query.PayerId.HasValue && query.PayerId.Value != Guid.Empty)
            {
                payerDisplayName = await ResolvePayerNameAsync(query.PayerId.Value, cancellationToken);
            }

            return new BillingArReportSummaryResponse
            {
                InvoiceCount = invCount,
                PatientCount = patCount,
                BillingOnProcessCount = onProcessCount,
                BillingClosedCount = closedCount,
                HandoffPendingCount = handoffPending,
                HandoffSuccessCount = handoffSuccess,
                InvoiceArNotCreatedCount = arNotCreated,
                InvoiceArCreatedCount = arCreated,
                TotalEstimatedAmount = estAmount,
                TotalFinalBillingAmount = finalAmount,
                TotalFinanceReceivableAmount = recAmount,
                TotalAmount = totAmount,
                PayerName = payerDisplayName,
                PeriodLabel = periodLabel,
                EntityTotalAmount = totAmount,
                EntityPatientCount = patCount
            };
        }
        finally
        {
            if (wasClosed) await conn.CloseAsync();
        }
    }

    private async Task<List<BillingArReportItemResponse>> ExecutePagedItemsQueryAsync(
        string fromWhereClause,
        List<NpgsqlParameter> parameters,
        int offset,
        int limit,
        CancellationToken cancellationToken)
    {
        var sql = $@"
SELECT 
    i.""Id"" AS invoice_id,
    i.""InvoiceNumber"",
    i.""Status"" AS invoice_status,
    COALESCE(i.""InvoiceDate"", e.""EncounterDate"") AS billing_date,
    e.""EncounterDate"" AS visit_date,
    COALESCE(e.""CompletedAt"", i.""ClosedAt"") AS discharge_date,
    e.""Id"" AS encounter_id,
    e.""EncounterNumber"",
    e.""EncounterType"",
    COALESCE(e.""AgeTextAtEncounter"", CASE WHEN e.""AgeYearAtEncounter"" IS NOT NULL THEN e.""AgeYearAtEncounter"" || ' Thn' ELSE '-' END) AS patient_age_text,
    p.""Id"" AS patient_id,
    p.""MedicalRecordNumber"",
    p.""FullName"" AS patient_name,
    COALESCE(r.""DebtorReferenceId"", h.""DebtorReferenceId"", g.""InsuranceProviderId"", g.""CompanyGuarantorId"") AS payer_id,
    CASE 
        WHEN (SELECT ins.""Id"" FROM ""MstInsuranceProvider"" ins WHERE ins.""Id"" = COALESCE(r.""DebtorReferenceId"", h.""DebtorReferenceId"", g.""InsuranceProviderId"", g.""CompanyGuarantorId"") AND NOT ins.""IsDelete"" LIMIT 1) IS NOT NULL THEN 'INSURANCE'
        WHEN (SELECT comp.""Id"" FROM ""MstCompanyGuarantor"" comp WHERE comp.""Id"" = COALESCE(r.""DebtorReferenceId"", h.""DebtorReferenceId"", g.""InsuranceProviderId"", g.""CompanyGuarantorId"") AND NOT comp.""IsDelete"" LIMIT 1) IS NOT NULL THEN 'COMPANY'
        ELSE 'OTHER'
    END AS payer_type,
    COALESCE(
        (SELECT ins.""InsuranceProviderName"" FROM ""MstInsuranceProvider"" ins WHERE ins.""Id"" = COALESCE(r.""DebtorReferenceId"", h.""DebtorReferenceId"", g.""InsuranceProviderId"", g.""CompanyGuarantorId"") AND NOT ins.""IsDelete"" LIMIT 1),
        (SELECT comp.""CompanyGuarantorName"" FROM ""MstCompanyGuarantor"" comp WHERE comp.""Id"" = COALESCE(r.""DebtorReferenceId"", h.""DebtorReferenceId"", g.""InsuranceProviderId"", g.""CompanyGuarantorId"") AND NOT comp.""IsDelete"" LIMIT 1),
        g.""PaymentSourceNameSnapshot"",
        'Lainnya'
    ) AS payer_name,
    COALESCE(cl.""ClinicName"", rm.""RoomName"", su.""ServiceUnitName"", '-') AS clinic_or_room_name,
    COALESCE(r.""OriginalAmount"", h.""Amount"", cv.""PrimaryAmount"", items_sum.""ItemsTotal"", 0) AS covered_amount,
    COALESCE(cv.""ExcessAmount"", 0) AS excess_amount,
    r.""Id"" AS receivable_id,
    r.""ReceivableNumber"",
    batch_info.""BatchId"" AS batch_id,
    batch_info.""BatchNumber"",
    batch_info.""BatchStatus"",
    CASE
        WHEN r.""Id"" IS NOT NULL THEN 'SUCCESS'
        WHEN intake.""Status"" = 'ERROR' THEN 'FAILED'
        WHEN h.""Status"" = 'CREATED' OR intake.""Status"" = 'NEW' THEN 'PENDING'
        WHEN i.""Status"" = 'OPEN' THEN 'NOT_HANDED_OFF'
        ELSE 'NOT_APPLICABLE'
    END AS finance_handoff_status,
    CASE
        WHEN r.""Id"" IS NULL THEN 'NOT_CREATED'
        WHEN batch_info.""BatchId"" IS NOT NULL AND batch_info.""BatchStatus"" = 'DRAFT' THEN 'DRAFT'
        WHEN batch_info.""BatchId"" IS NOT NULL AND batch_info.""BatchStatus"" IN ('ISSUED', 'PARTIALLY_PAID', 'PAID') THEN 'ISSUED'
        WHEN batch_info.""BatchId"" IS NOT NULL AND batch_info.""BatchStatus"" = 'CANCELLED' THEN 'CANCELLED'
        ELSE 'NOT_CREATED'
    END AS ar_invoice_status
{fromWhereClause}
ORDER BY payer_name ASC, billing_date ASC, invoice_id ASC
LIMIT @pageSize OFFSET @offset;";

        var conn = _dbContext.Database.GetDbConnection();
        var wasClosed = conn.State != ConnectionState.Open;
        if (wasClosed) await conn.OpenAsync(cancellationToken);

        var result = new List<BillingArReportItemResponse>();

        try
        {
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            foreach (var p in parameters)
            {
                cmd.Parameters.Add(p.Clone());
            }
            cmd.Parameters.Add(new NpgsqlParameter("pageSize", limit));
            cmd.Parameters.Add(new NpgsqlParameter("offset", offset));

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var invId = reader.GetGuid(0);
                var invNumber = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                var invStatus = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                DateTimeOffset? billingDate = ReadDateTimeOffset(reader, 3);
                DateTime? visitDate = ReadDateTime(reader, 4);
                DateTimeOffset? dischargeDate = ReadDateTimeOffset(reader, 5);
                Guid? encId = reader.IsDBNull(6) ? null : reader.GetGuid(6);
                var encNumber = reader.IsDBNull(7) ? string.Empty : reader.GetString(7);
                var encTypeVal = reader.IsDBNull(8) ? 0 : Convert.ToInt32(reader.GetValue(8));
                var ageText = reader.IsDBNull(9) ? "-" : reader.GetString(9);
                var mrn = reader.IsDBNull(11) ? string.Empty : reader.GetString(11);
                var patName = reader.IsDBNull(12) ? string.Empty : reader.GetString(12);
                Guid? payerId = reader.IsDBNull(13) ? null : reader.GetGuid(13);
                var payerType = reader.IsDBNull(14) ? "OTHER" : reader.GetString(14);
                var payerName = reader.IsDBNull(15) ? "Lainnya" : reader.GetString(15);
                var clinicOrRoom = reader.IsDBNull(16) ? "-" : reader.GetString(16);
                var coveredAmount = reader.IsDBNull(17) ? 0m : Convert.ToDecimal(reader.GetValue(17));
                var excessAmount = reader.IsDBNull(18) ? 0m : Convert.ToDecimal(reader.GetValue(18));
                Guid? recId = reader.IsDBNull(19) ? null : reader.GetGuid(19);
                var recNumber = reader.IsDBNull(20) ? null : reader.GetString(20);
                Guid? batchId = reader.IsDBNull(21) ? null : reader.GetGuid(21);
                var batchNumber = reader.IsDBNull(22) ? null : reader.GetString(22);
                var financeHandoffStatus = reader.IsDBNull(24) ? "NOT_HANDED_OFF" : reader.GetString(24);
                var arInvoiceStatus = reader.IsDBNull(25) ? "NOT_CREATED" : reader.GetString(25);

                var visitTypeStr = encTypeVal switch
                {
                    1 => "OP",
                    3 => "IP",
                    2 => "IGD",
                    _ => "OTHER"
                };

                var visitTypeLabel = encTypeVal switch
                {
                    1 => "Rawat Jalan",
                    3 => "Rawat Inap",
                    2 => "IGD",
                    _ => "Lainnya"
                };

                var billingStatusLabel = invStatus switch
                {
                    "OPEN" => "On Process",
                    "FINAL" => "Final",
                    "CLOSED" => "Closed",
                    "SETTLED_BY_WRITE_OFF" => "Write-off",
                    _ => invStatus
                };

                var handoffLabel = financeHandoffStatus switch
                {
                    "SUCCESS" => "Berhasil",
                    "FAILED" => "Gagal",
                    "PENDING" => "Menunggu Proses",
                    "NOT_HANDED_OFF" => "Belum Handoff",
                    "NOT_APPLICABLE" => "Tidak Berlaku",
                    _ => financeHandoffStatus
                };

                var arStatusLabel = arInvoiceStatus switch
                {
                    "NOT_CREATED" => "Belum Dibuat",
                    "DRAFT" => "Draft",
                    "ISSUED" => "Sudah Dibuat",
                    "CANCELLED" => "Dibatalkan",
                    _ => arInvoiceStatus
                };

                result.Add(new BillingArReportItemResponse
                {
                    Id = invId.ToString(),
                    BillingInvoiceId = invId,
                    InvoiceNumber = invNumber,
                    EncounterId = encId,
                    RegistrationNumber = encNumber,
                    MedicalRecordNumber = mrn,
                    PatientName = patName,
                    PatientAgeText = ageText,
                    BillingDate = billingDate,
                    VisitDate = visitDate,
                    DischargeDate = dischargeDate,
                    ClinicOrRoomName = clinicOrRoom,
                    PayerId = payerId,
                    PayerType = payerType,
                    PayerName = payerName,
                    VisitType = visitTypeStr,
                    VisitTypeLabel = visitTypeLabel,
                    CoveredBillingAmount = coveredAmount,
                    ExcessAmount = excessAmount,
                    IsEstimated = invStatus == "OPEN",
                    BillingStatus = invStatus,
                    BillingStatusLabel = billingStatusLabel,
                    FinanceHandoffStatus = financeHandoffStatus,
                    FinanceHandoffStatusLabel = handoffLabel,
                    FinanceReceivableId = recId,
                    FinanceReceivableNumber = recNumber,
                    ArInvoiceStatus = arInvoiceStatus,
                    ArInvoiceStatusLabel = arStatusLabel,
                    ArInvoiceBatchId = batchId,
                    ArInvoiceNumber = batchNumber
                });
            }

            return result;
        }
        finally
        {
            if (wasClosed) await conn.CloseAsync();
        }
    }

    private static List<BillingArReportGroupResponse> BuildPageGroups(List<BillingArReportItemResponse> items)
    {
        var groups = new List<BillingArReportGroupResponse>();
        if (items.Count == 0) return groups;

        // Kelompokkan berdasarkan PayerId + PayerName (ASC)
        var groupedByPayer = items
            .GroupBy(x => new { PayerKey = x.PayerId?.ToString() ?? x.PayerName, x.PayerId, x.PayerName, x.PayerType })
            .OrderBy(g => g.Key.PayerName, StringComparer.OrdinalIgnoreCase);

        foreach (var pGroup in groupedByPayer)
        {
            var pItems = pGroup.ToList();
            var groupTotal = pItems.Sum(x => x.CoveredBillingAmount);

            // Kelompokkan per tanggal kalender WIB
            var dateTotals = pItems
                .GroupBy(x => x.BillingDate.HasValue
                    ? TimeZoneInfo.ConvertTime(x.BillingDate.Value, FinanceBusinessDate.BusinessTimeZone).ToString("yyyy-MM-dd")
                    : "Lainnya")
                .OrderBy(dg => dg.Key)
                .Select(dg =>
                {
                    var dKey = dg.Key;
                    var label = dKey;
                    if (DateTime.TryParseExact(dKey, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                    {
                        label = parsedDate.ToString("dd/MM/yyyy");
                    }

                    return new BillingArReportDateTotalResponse
                    {
                        Date = dKey,
                        DateLabel = label,
                        Total = dg.Sum(it => it.CoveredBillingAmount),
                        ItemCount = dg.Count()
                    };
                })
                .ToList();

            groups.Add(new BillingArReportGroupResponse
            {
                PayerId = pGroup.Key.PayerId,
                GroupId = pGroup.Key.PayerKey,
                PayerName = pGroup.Key.PayerName,
                PayerType = pGroup.Key.PayerType,
                ItemCount = pItems.Count,
                GroupTotal = groupTotal,
                DateTotals = dateTotals,
                Items = pItems
            });
        }

        return groups;
    }

    private async Task<string> ResolvePayerNameAsync(Guid payerId, CancellationToken cancellationToken)
    {
        var ins = await _dbContext.MstInsuranceProviders.AsNoTracking()
            .Where(x => x.Id == payerId && !x.IsDelete)
            .Select(x => x.InsuranceProviderName)
            .FirstOrDefaultAsync(cancellationToken);
        if (!string.IsNullOrEmpty(ins)) return ins;

        var comp = await _dbContext.MstCompanyGuarantors.AsNoTracking()
            .Where(x => x.Id == payerId && !x.IsDelete)
            .Select(x => x.CompanyGuarantorName)
            .FirstOrDefaultAsync(cancellationToken);
        if (!string.IsNullOrEmpty(comp)) return comp;

        return "Penjamin Terpilih";
    }

    private static ResolvedDateRange ResolveDateRange(BillingArReportQuery query)
    {
        var tz = FinanceBusinessDate.BusinessTimeZone;
        var todayWib = FinanceBusinessDate.Today();

        // 1. Prioritas Preset
        if (!string.IsNullOrWhiteSpace(query.PeriodPreset))
        {
            var preset = query.PeriodPreset.Trim().ToLowerInvariant();
            switch (preset)
            {
                case "today":
                    return new ResolvedDateRange(
                        ToUtcStartOfDay(todayWib, tz),
                        ToUtcStartOfDay(todayWib.AddDays(1), tz),
                        $"Hari ini ({todayWib:dd/MM/yyyy})");

                case "thisweek":
                    // Monday based
                    var diff = (7 + (todayWib.DayOfWeek - DayOfWeek.Monday)) % 7;
                    var mon = todayWib.AddDays(-diff);
                    var nextMon = mon.AddDays(7);
                    var sun = mon.AddDays(6);
                    return new ResolvedDateRange(
                        ToUtcStartOfDay(mon, tz),
                        ToUtcStartOfDay(nextMon, tz),
                        $"Minggu Ini ({mon:dd/MM/yyyy} - {sun:dd/MM/yyyy})");

                case "thismonth":
                    var firstDayMonth = new DateOnly(todayWib.Year, todayWib.Month, 1);
                    var nextMonthFirst = firstDayMonth.AddMonths(1);
                    var lastDayMonth = nextMonthFirst.AddDays(-1);
                    return new ResolvedDateRange(
                        ToUtcStartOfDay(firstDayMonth, tz),
                        ToUtcStartOfDay(nextMonthFirst, tz),
                        $"Bulan Ini ({firstDayMonth:dd/MM/yyyy} - {lastDayMonth:dd/MM/yyyy})");

                case "lastmonth":
                    var curMonthFirst = new DateOnly(todayWib.Year, todayWib.Month, 1);
                    var prevMonthFirst = curMonthFirst.AddMonths(-1);
                    var prevMonthLast = curMonthFirst.AddDays(-1);
                    return new ResolvedDateRange(
                        ToUtcStartOfDay(prevMonthFirst, tz),
                        ToUtcStartOfDay(curMonthFirst, tz),
                        $"Bulan Lalu ({prevMonthFirst:dd/MM/yyyy} - {prevMonthLast:dd/MM/yyyy})");

                case "thisyear":
                    var firstDayYear = new DateOnly(todayWib.Year, 1, 1);
                    var nextYearFirst = firstDayYear.AddYears(1);
                    var lastDayYear = nextYearFirst.AddDays(-1);
                    return new ResolvedDateRange(
                        ToUtcStartOfDay(firstDayYear, tz),
                        ToUtcStartOfDay(nextYearFirst, tz),
                        $"Tahun Ini ({firstDayYear:dd/MM/yyyy} - {lastDayYear:dd/MM/yyyy})");

                case "all":
                    return new ResolvedDateRange(null, null, "Semua Periode");
            }
        }

        // 2. Custom Date Range
        if (query.StartDate.HasValue || query.EndDate.HasValue)
        {
            var s = query.StartDate ?? query.EndDate!.Value;
            var e = query.EndDate ?? query.StartDate!.Value;
            if (e < s) e = s;

            return new ResolvedDateRange(
                ToUtcStartOfDay(s, tz),
                ToUtcStartOfDay(e.AddDays(1), tz),
                $"{s:dd/MM/yyyy} s/d {e:dd/MM/yyyy}");
        }

        return new ResolvedDateRange(null, null, "Semua Periode");
    }

    private static DateTime ToUtcStartOfDay(DateOnly date, TimeZoneInfo tz)
    {
        var localDateTime = date.ToDateTime(TimeOnly.MinValue);
        return TimeZoneInfo.ConvertTimeToUtc(localDateTime, tz);
    }

    private static string FormatVisitTypeLabel(string? visitType)
    {
        if (string.IsNullOrWhiteSpace(visitType) || visitType.Equals("ALL", StringComparison.OrdinalIgnoreCase))
            return "Semua Kunjungan";
        return visitType.Trim().ToUpperInvariant() switch
        {
            "OP" or "OUTPATIENT" => "Rawat Jalan (OP)",
            "IP" or "INPATIENT" => "Rawat Inap (IP)",
            "IGD" or "EMERGENCY" => "IGD",
            _ => visitType
        };
    }

    private static DateTimeOffset? ReadDateTimeOffset(DbDataReader reader, int index)
    {
        if (reader.IsDBNull(index)) return null;
        var val = reader.GetValue(index);
        if (val is DateTimeOffset dto) return dto;
        if (val is DateTime dt) return new DateTimeOffset(dt, TimeSpan.Zero);
        return null;
    }

    private static DateTime? ReadDateTime(DbDataReader reader, int index)
    {
        if (reader.IsDBNull(index)) return null;
        var val = reader.GetValue(index);
        if (val is DateTime dt) return dt;
        if (val is DateTimeOffset dto) return dto.DateTime;
        return null;
    }

    private static string EscapeLike(string input) =>
        input.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");

    private sealed record ResolvedDateRange(DateTime? FromUtc, DateTime? ToExclusiveUtc, string PeriodLabel);
}
