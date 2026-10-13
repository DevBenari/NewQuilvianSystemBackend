using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Dtos;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Services;

/// <summary>
/// Pembuat dokumen PDF read-only untuk Receivable AR Canceled (Canceled AR Payment Sessions).
/// Dipanggil oleh FinanceReceiptService.ExportCanceledArPaymentsPdfAsync.
/// Menampilkan hasil filter grouped session yang identik dengan tampilan tabel.
/// </summary>
public static class CanceledArPaymentReportDocuments
{
    public static byte[] BuildListPdf(List<CanceledArPaymentRowResponse> rows, CanceledArPaymentSummaryResponse summary)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(24);
                page.DefaultTextStyle(x => x.FontSize(8));

                page.Header().Column(header =>
                {
                    header.Item().Text("Daftar Pembatalan Receivable AR").FontSize(14).Bold();
                    header.Item().Text("Riwayat sesi pembayaran piutang AR yang telah dibatalkan / dibalik").FontSize(8).FontColor(Colors.Grey.Darken2);
                });

                page.Content().Column(col =>
                {
                    col.Item().PaddingVertical(8).Text(
                        $"Transaksi: {summary.TransactionCount}   |   " +
                        $"Total Tagihan: Rp {summary.TotalTagihan:N0}   |   " +
                        $"Total Bayar: Rp {summary.TotalBayar:N0}   |   " +
                        $"Total Sisa: Rp {summary.TotalSisa:N0}")
                        .FontSize(9).Bold();

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(24);
                            columns.RelativeColumn(2.5f);
                            columns.RelativeColumn(3f);
                            columns.RelativeColumn(2f);
                            columns.RelativeColumn(2f);
                            columns.RelativeColumn(2f);
                            columns.RelativeColumn(2f);
                            columns.RelativeColumn(3f);
                            columns.RelativeColumn(2.5f);
                        });

                        table.Header(h =>
                        {
                            string[] heads =
                            [
                                "No", "No. Invoice", "Nama Perusahaan", "Tgl Pembayaran",
                                "Total Tagihan", "Total Diterima", "Sisa Pembayaran", "Keterangan", "Status"
                            ];
                            foreach (var title in heads)
                            {
                                h.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text(title).Bold();
                            }
                        });

                        var index = 1;
                        foreach (var row in rows)
                        {
                            table.Cell().Padding(4).Text(index.ToString());
                            table.Cell().Padding(4).Text(row.InvoiceNumber);
                            table.Cell().Padding(4).Text(row.CompanyName);
                            table.Cell().Padding(4).Text(FinanceBusinessDate.ToBusinessTime(row.PaymentDate).ToString("dd/MM/yyyy"));
                            table.Cell().Padding(4).AlignRight().Text(row.TotalTagihan.ToString("N0"));
                            table.Cell().Padding(4).AlignRight().Text(row.TotalDiterima.ToString("N0"));
                            table.Cell().Padding(4).AlignRight().Text(row.HistoricalRemaining.ToString("N0"));
                            table.Cell().Padding(4).Text(row.Keterangan ?? "-");
                            table.Cell().Padding(4).Column(c =>
                            {
                                c.Item().Text(row.Status).Bold().FontColor(Colors.Red.Medium);
                                c.Item().Text(row.ReversedByName ?? "-").FontSize(7).FontColor(Colors.Grey.Darken1);
                            });

                            index++;
                        }

                        if (rows.Count == 0)
                        {
                            table.Cell().ColumnSpan(9).Padding(12).AlignCenter()
                                .Text("Tidak ada data pembatalan pembayaran AR yang sesuai dengan filter.");
                        }
                    });
                });

                page.Footer().AlignRight().Text(x =>
                {
                    x.Span("Halaman ");
                    x.CurrentPageNumber();
                    x.Span(" dari ");
                    x.TotalPages();
                });
            });
        }).GeneratePdf();
    }
}
