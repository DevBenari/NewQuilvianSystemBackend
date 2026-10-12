using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;

/// <summary>
/// Pembuat dokumen PDF read-only untuk Report AR Created (Laporan Pembuatan AR).
/// Menghasilkan format Landscape A4 dengan pengelompokan perusahaan, subtotal tanggal,
/// subtotal perusahaan, dan grand total.
/// </summary>
public static class ReportArCreatedReportDocuments
{
    public static byte[] BuildPdf(
        List<ReportArCreatedItemResponse> rows,
        ReportArCreatedSummaryResponse summary)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontSize(8).FontFamily("Arial"));

                // Header Laporan
                page.Header().Column(headerCol =>
                {
                    headerCol.Item().Row(r =>
                    {
                        r.RelativeItem().Column(c =>
                        {
                            c.Item().Text("RUMAH SAKIT METROPOLITAN MEDICAL CENTRE").FontSize(11).Bold().FontColor("#0f172a");
                            c.Item().Text("Jl. H.R. Rasuna Said Kav. C-21, Kuningan, Jakarta Selatan 12920 | Telp: (021) 520-3435").FontSize(7.5f).FontColor("#64748b");
                        });

                        r.RelativeItem().AlignRight().Column(c =>
                        {
                            c.Item().Text("LAPORAN PEMBUATAN AR (REPORT AR CREATED)").FontSize(11).Bold().FontColor("#087d91");
                            c.Item().Text($"Dicetak pada: {FinanceBusinessDate.ToBusinessTime(DateTimeOffset.UtcNow):dd/MM/yyyy HH:mm} WIB").FontSize(7.5f).FontColor("#64748b");
                        });
                    });

                    headerCol.Item().PaddingTop(4).PaddingBottom(6).LineHorizontal(1).LineColor("#cbd5e1");

                    // Filter & Ringkasan Laporan
                    headerCol.Item().Background("#f8fafc").Border(1).BorderColor("#e2e8f0").Padding(6).Row(filterRow =>
                    {
                        filterRow.RelativeItem().Text(t =>
                        {
                            t.Span("Periode: ").Bold();
                            t.Span(string.IsNullOrWhiteSpace(summary.PeriodLabel) ? "Semua Periode" : summary.PeriodLabel);
                            t.Span("   |   Penjamin: ").Bold();
                            t.Span(summary.SelectedCompanyName ?? "Semua Perusahaan");
                        });

                        filterRow.RelativeItem().AlignRight().Text(t =>
                        {
                            t.Span($"Total Tagihan: ").Bold();
                            t.Span($"Rp {summary.TotalBillingAmount:N0}").Bold().FontColor("#087d91");
                            t.Span($"   |   Jumlah Detail: {summary.TotalDetailRows} Tagihan ({summary.TotalInvoiceCount} Invoice)");
                        });
                    });

                    headerCol.Item().PaddingBottom(4);
                });

                // Konten Tabel
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.ConstantColumn(24);   // NO
                        cols.ConstantColumn(75);   // TANGGAL INVOICE
                        cols.ConstantColumn(75);   // TANGGAL REGISTRASI
                        cols.ConstantColumn(65);   // NO. BILL
                        cols.ConstantColumn(65);   // NO. REG
                        cols.ConstantColumn(65);   // NO. RM
                        cols.RelativeColumn(3);    // NAMA PASIEN
                        cols.RelativeColumn(2.5f); // NO. INVOICE
                        cols.ConstantColumn(75);   // JUMLAH TAGIHAN
                    });

                    // Table Header
                    table.Header(h =>
                    {
                        string[] headers =
                        [
                            "NO", "TGL INVOICE", "TGL REGISTRASI", "NO. BILL",
                            "NO. REG", "NO. RM", "NAMA PASIEN", "NO. INVOICE", "TAGIHAN (RP)"
                        ];

                        for (var idx = 0; idx < headers.Length; idx++)
                        {
                            var headerCell = h.Cell().Background("#087d91").Padding(4);
                            if (idx == 0 || idx == 1 || idx == 2 || idx == 4 || idx == 5)
                            {
                                headerCell.AlignCenter().Text(headers[idx]).Bold().FontColor(Colors.White);
                            }
                            else if (idx == 8)
                            {
                                headerCell.AlignRight().Text(headers[idx]).Bold().FontColor(Colors.White);
                            }
                            else
                            {
                                headerCell.AlignLeft().Text(headers[idx]).Bold().FontColor(Colors.White);
                            }
                        }
                    });

                    if (rows.Count == 0)
                    {
                        table.Cell().ColumnSpan(9).Padding(16).AlignCenter()
                            .Text("Tidak ada data Invoice AR yang sesuai dengan filter.").FontColor("#64748b");
                        return;
                    }

                    // Kelompokkan baris berdasarkan Perusahaan
                    var companyGroups = rows.GroupBy(x => x.CompanyName).ToList();
                    var globalRowIndex = 1;

                    foreach (var cGroup in companyGroups)
                    {
                        // Baris Header Perusahaan (Colspan 9)
                        table.Cell().ColumnSpan(9)
                            .Background("#d9d9d9")
                            .Padding(4)
                            .Text(cGroup.Key.ToUpperInvariant())
                            .Bold()
                            .FontColor("#071323");

                        // Kelompokkan per tanggal dalam perusahaan
                        var dateGroups = cGroup.GroupBy(x =>
                            x.InvoiceDate.HasValue
                                ? FinanceBusinessDate.ToBusinessTime(x.InvoiceDate.Value).ToString("dd MMMM yyyy")
                                : "Tanpa Tanggal"
                        ).ToList();

                        decimal companyTotal = 0m;

                        foreach (var dGroup in dateGroups)
                        {
                            decimal dateSubtotal = 0m;

                            foreach (var item in dGroup)
                            {
                                dateSubtotal += item.BillingAmount;
                                companyTotal += item.BillingAmount;

                                var isEven = globalRowIndex % 2 == 0;
                                var rowBg = isEven ? "#f8fafc" : "#ffffff";

                                table.Cell().Background(rowBg).Padding(3).AlignCenter().Text(globalRowIndex.ToString());

                                var invDateStr = item.InvoiceDate.HasValue
                                    ? FinanceBusinessDate.ToBusinessTime(item.InvoiceDate.Value).ToString("dd/MM/yyyy HH:mm")
                                    : "-";
                                table.Cell().Background(rowBg).Padding(3).AlignCenter().Text(invDateStr);

                                var regDateStr = item.RegistrationDate.HasValue
                                    ? FinanceBusinessDate.ToBusinessTime(item.RegistrationDate.Value).ToString("dd/MM/yyyy HH:mm")
                                    : "-";
                                table.Cell().Background(rowBg).Padding(3).AlignCenter().Text(regDateStr);

                                table.Cell().Background(rowBg).Padding(3).Text(item.BillingNumber ?? "-");
                                table.Cell().Background(rowBg).Padding(3).AlignCenter().Text(item.RegistrationNumber ?? "-");
                                table.Cell().Background(rowBg).Padding(3).AlignCenter().Text(item.MedicalRecordNumber ?? "-");
                                table.Cell().Background(rowBg).Padding(3).Text(item.PatientName ?? "-").Bold();
                                table.Cell().Background(rowBg).Padding(3).Text(item.InvoiceNumber ?? "-");
                                table.Cell().Background(rowBg).Padding(3).AlignRight().Text(item.BillingAmount.ToString("N0"));

                                globalRowIndex++;
                            }

                            // Subtotal Tanggal
                            table.Cell().ColumnSpan(8)
                                .Background("#e6f4ea")
                                .Padding(3)
                                .AlignRight()
                                .Text($"Total {dGroup.Key}:")
                                .Bold()
                                .FontColor("#1e4620");

                            table.Cell()
                                .Background("#e6f4ea")
                                .Padding(3)
                                .AlignRight()
                                .Text(dateSubtotal.ToString("N0"))
                                .Bold()
                                .FontColor("#1e4620");
                        }

                        // Total Perusahaan
                        table.Cell().ColumnSpan(8)
                            .Background("#e0f2fe")
                            .Padding(4)
                            .AlignRight()
                            .Text($"Total {cGroup.Key}:")
                            .Bold()
                            .FontColor("#0369a1");

                        table.Cell()
                            .Background("#e0f2fe")
                            .Padding(4)
                            .AlignRight()
                            .Text(companyTotal.ToString("N0"))
                            .Bold()
                            .FontColor("#0369a1");
                    }

                    // Grand Total
                    table.Cell().ColumnSpan(8)
                        .Background("#0f172a")
                        .Padding(5)
                        .AlignRight()
                        .Text("GRAND TOTAL:")
                        .Bold()
                        .FontColor(Colors.White);

                    table.Cell()
                        .Background("#0f172a")
                        .Padding(5)
                        .AlignRight()
                        .Text(summary.TotalBillingAmount.ToString("N0"))
                        .Bold()
                        .FontColor(Colors.White);
                });

                // Footer
                page.Footer().Row(footerRow =>
                {
                    footerRow.RelativeItem().Text("Dokumen Resmi Rumah Sakit Metropolitan Medical Centre — Confidential").FontSize(7.5f).FontColor("#94a3b8");
                    footerRow.RelativeItem().AlignRight().Text(x =>
                    {
                        x.Span("Halaman ");
                        x.CurrentPageNumber();
                        x.Span(" dari ");
                        x.TotalPages();
                    });
                });
            });
        }).GeneratePdf();
    }
}

