using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;

/// <summary>
/// Pembuat dokumen PDF read-only untuk Report Canceled Invoice (bagian G task revamp). Dipanggil
/// oleh FinanceReceivableInvoiceBatchService.ExportCanceledListPdfAsync/ExportCanceledDetailPdfAsync.
/// Tidak pernah mengubah status invoice — murni membaca DTO yang sudah dihitung oleh service.
/// Lisensi QuestPDF Community dikonfigurasi sekali di Program.cs (QuestPDF.Settings.License).
/// </summary>
public static class CanceledInvoiceReportDocuments
{
    public static byte[] BuildListPdf(List<CanceledInvoiceRowResponse> rows, CanceledInvoiceSummaryResponse summary)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(24);
                page.DefaultTextStyle(x => x.FontSize(8));

                page.Header().Text("Report Canceled Invoice").FontSize(14).Bold();

                page.Content().Column(col =>
                {
                    col.Item().PaddingVertical(6).Text(
                        $"Target: {summary.TargetName}   |   Total Filter: Rp {summary.TotalAmount:N0}   |   " +
                        $"Total Diskon: Rp {summary.TotalDiscount:N0}   |   Total Akhir: Rp {summary.NetAmount:N0}   |   " +
                        $"Total Target: Rp {summary.TargetAmount:N0}   |   Jumlah Invoice: {summary.InvoiceCount}")
                        .FontSize(9);

                    if (!summary.CategorySupported)
                    {
                        col.Item().PaddingBottom(6)
                            .Text(summary.CategoryLimitationReason ?? "Kategori ini belum didukung Report Canceled Invoice V2.")
                            .FontColor(Colors.Red.Medium).FontSize(8);
                    }

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(22);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2.2f);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1.6f);
                            columns.RelativeColumn(1.6f);
                            columns.RelativeColumn(1.6f);
                            columns.RelativeColumn(3);
                        });

                        table.Header(header =>
                        {
                            string[] heads =
                            [
                                "No", "No. Invoice", "Perusahaan/Penjamin", "Jenis", "Tgl Invoice",
                                "Tgl Cancel", "Oleh", "Total", "Diskon", "Akhir", "Alasan Cancel"
                            ];
                            foreach (var h in heads)
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text(h).Bold();
                        });

                        var i = 1;
                        foreach (var row in rows)
                        {
                            table.Cell().Padding(3).Text(i.ToString());
                            table.Cell().Padding(3).Text(row.InvoiceNumber);
                            table.Cell().Padding(3).Text(row.DebtorName ?? "-");
                            table.Cell().Padding(3).Text(row.ServiceTypeName ?? "-");
                            table.Cell().Padding(3).Text(row.InvoiceDate.HasValue ? row.InvoiceDate.Value.ToString("dd/MM/yyyy") : "-");
                            table.Cell().Padding(3).Text(row.CancelledAt.HasValue
                                ? FinanceBusinessDate.ToBusinessTime(row.CancelledAt.Value).ToString("dd/MM/yyyy HH:mm")
                                : "-");
                            table.Cell().Padding(3).Text(row.CancelledByName ?? "-");
                            table.Cell().Padding(3).AlignRight().Text(row.TotalAmount.ToString("N0"));
                            table.Cell().Padding(3).AlignRight().Text(row.TotalDiscount.ToString("N0"));
                            table.Cell().Padding(3).AlignRight().Text(row.NetAmount.ToString("N0"));
                            table.Cell().Padding(3).Text(row.CancelReason ?? "-");
                            i++;
                        }

                        if (rows.Count == 0)
                        {
                            table.Cell().ColumnSpan(11).Padding(8).AlignCenter()
                                .Text("Tidak ada Canceled Invoice yang cocok dengan filter ini.");
                        }
                    });
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });
        }).GeneratePdf();
    }

    public static byte[] BuildDetailPdf(CanceledInvoiceDetailResponse detail)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(24);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Text($"Canceled Invoice — {detail.InvoiceNumber}").FontSize(14).Bold();

                page.Content().Column(col =>
                {
                    col.Item().PaddingVertical(8).Column(h =>
                    {
                        h.Item().Text($"Perusahaan/Penjamin: {detail.DebtorName ?? "-"}");
                        h.Item().Text($"Status: {detail.Status}");
                        h.Item().Text("Tanggal Cancel: " + (detail.CancelledAt.HasValue
                            ? FinanceBusinessDate.ToBusinessTime(detail.CancelledAt.Value).ToString("dd/MM/yyyy HH:mm")
                            : "-"));
                        h.Item().Text($"Oleh: {detail.CancelledByName ?? "-"}");
                        h.Item().Text($"Alasan Cancel: {detail.CancelReason ?? "-"}");
                        h.Item().Text("Tanggal Invoice: " + (detail.InvoiceDate.HasValue ? detail.InvoiceDate.Value.ToString("dd/MM/yyyy") : "-"));
                        h.Item().Text("Tanggal Jatuh Tempo: " + (detail.DueDate.HasValue ? detail.DueDate.Value.ToString("dd/MM/yyyy") : "-"));
                        h.Item().Text($"Jenis Invoice: {detail.ServiceTypeName ?? "-"}");
                    });

                    col.Item().PaddingTop(8).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(22);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            string[] heads = ["No", "Nama Pasien", "No. RM", "No. Bill", "No. Reg", "Tgl Kunjungan", "Total Tagihan", "Diskon", "Total Akhir"];
                            foreach (var h in heads)
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text(h).Bold();
                        });

                        var i = 1;
                        foreach (var m in detail.Items)
                        {
                            table.Cell().Padding(3).Text(i.ToString());
                            table.Cell().Padding(3).Text(m.PatientName);
                            table.Cell().Padding(3).Text(m.MedicalRecordNumber);
                            table.Cell().Padding(3).Text(m.BillingNumber);
                            table.Cell().Padding(3).Text(m.RegistrationNumber);
                            table.Cell().Padding(3).Text(m.VisitDate.HasValue ? m.VisitDate.Value.ToString("dd/MM/yyyy") : "-");
                            table.Cell().Padding(3).AlignRight().Text(m.EditableTotalAmount.ToString("N0"));
                            table.Cell().Padding(3).AlignRight().Text(m.DiscountAmount.ToString("N0"));
                            table.Cell().Padding(3).AlignRight().Text(m.FinalAmount.ToString("N0"));
                            i++;
                        }

                        if (detail.Items.Count == 0)
                        {
                            table.Cell().ColumnSpan(9).Padding(8).AlignCenter()
                                .Text("Tidak ada anggota tagihan pada Canceled Invoice ini.");
                        }
                    });

                    col.Item().PaddingTop(12).AlignRight().Column(s =>
                    {
                        s.Item().Text($"Jumlah Detail: {detail.Summary.ItemCount}");
                        s.Item().Text($"Total Piutang: Rp {detail.Summary.TotalAmount:N0}");
                        s.Item().Text($"Total Diskon: Rp {detail.Summary.TotalDiscount:N0}");
                        s.Item().Text($"Total Akhir: Rp {detail.Summary.NetAmount:N0}").Bold();
                    });
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });
        }).GeneratePdf();
    }
}
