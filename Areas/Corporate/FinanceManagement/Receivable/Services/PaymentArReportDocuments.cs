using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;

public static class PaymentArReportDocuments
{
    public static byte[] BuildListPdf(
        List<PaymentArReportRowResponse> rows,
        PaymentArReportSummaryResponse summary,
        PaymentArReportQuery query)
    {
        return Document.Create(container =>
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
                            c.Item().Text("LAPORAN PEMBAYARAN AR").FontSize(14).Bold().FontColor("#087d91");
                            c.Item().Text("Laporan penerimaan pembayaran atas Invoice AR berdasarkan penjamin, sumber pembayaran, dan periode.").FontSize(8).FontColor(Colors.Grey.Darken1);
                        });

                        r.ConstantItem(200).AlignRight().Column(c =>
                        {
                            var periodStr = query.StartDate.HasValue && query.EndDate.HasValue
                                ? $"{query.StartDate.Value:dd/MM/yyyy} - {query.EndDate.Value:dd/MM/yyyy}"
                                : query.StartDate.HasValue ? $"Mulai {query.StartDate.Value:dd/MM/yyyy}"
                                : query.EndDate.HasValue ? $"Sampai {query.EndDate.Value:dd/MM/yyyy}"
                                : "Semua Periode";
                            c.Item().Text($"Periode: {periodStr}").FontSize(8).Bold();
                            c.Item().Text($"Dicetak: {DateTime.Now:dd/MM/yyyy HH:mm} WIB").FontSize(7).FontColor(Colors.Grey.Darken1);
                        });
                    });

                    col.Item().PaddingVertical(8).Row(r =>
                    {
                        r.RelativeItem().Border(1).BorderColor("#cbd5e1").Background("#f8fafc").Padding(6).Column(c =>
                        {
                            c.Item().Text("Transaksi").FontSize(7).FontColor(Colors.Grey.Darken2);
                            c.Item().Text($"{summary.TotalTransaksi:N0}").FontSize(11).Bold().FontColor("#087d91");
                        });
                        r.Spacing(6);
                        r.RelativeItem().Border(1).BorderColor("#cbd5e1").Background("#f8fafc").Padding(6).Column(c =>
                        {
                            c.Item().Text("Total Tagihan").FontSize(7).FontColor(Colors.Grey.Darken2);
                            c.Item().Text($"Rp {summary.TotalTagihan:N0}").FontSize(11).Bold().FontColor("#0f172a");
                        });
                        r.Spacing(6);
                        r.RelativeItem().Border(1).BorderColor("#cbd5e1").Background("#f8fafc").Padding(6).Column(c =>
                        {
                            c.Item().Text("Total Bayar").FontSize(7).FontColor(Colors.Grey.Darken2);
                            c.Item().Text($"Rp {summary.TotalBayar:N0}").FontSize(11).Bold().FontColor("#16a34a");
                        });
                        r.Spacing(6);
                        r.RelativeItem().Border(1).BorderColor("#cbd5e1").Background("#f8fafc").Padding(6).Column(c =>
                        {
                            c.Item().Text("Total Sisa").FontSize(7).FontColor(Colors.Grey.Darken2);
                            c.Item().Text($"Rp {summary.TotalSisa:N0}").FontSize(11).Bold().FontColor("#dc2626");
                        });
                    });
                });

                page.Content().PaddingVertical(6).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(24); // No
                        columns.RelativeColumn(3.5f); // Penjamin
                        columns.RelativeColumn(1.8f); // Metode
                        columns.RelativeColumn(2.5f); // Bank
                        columns.RelativeColumn(2.2f); // No. Ref
                        columns.RelativeColumn(1.8f); // Tgl Bayar
                        columns.RelativeColumn(2.2f); // Total Tagihan
                        columns.RelativeColumn(2.2f); // Total Bayar
                        columns.RelativeColumn(2.2f); // Sisa
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background("#087d91").Padding(4).AlignCenter().Text("No.").Bold().FontColor(Colors.White);
                        header.Cell().Background("#087d91").Padding(4).Text("Penjamin / Asuransi").Bold().FontColor(Colors.White);
                        header.Cell().Background("#087d91").Padding(4).Text("Metode").Bold().FontColor(Colors.White);
                        header.Cell().Background("#087d91").Padding(4).Text("Bank / Penerima").Bold().FontColor(Colors.White);
                        header.Cell().Background("#087d91").Padding(4).Text("No. Referensi").Bold().FontColor(Colors.White);
                        header.Cell().Background("#087d91").Padding(4).AlignCenter().Text("Tgl. Bayar").Bold().FontColor(Colors.White);
                        header.Cell().Background("#087d91").Padding(4).AlignRight().Text("Total Tagihan").Bold().FontColor(Colors.White);
                        header.Cell().Background("#087d91").Padding(4).AlignRight().Text("Total Bayar").Bold().FontColor(Colors.White);
                        header.Cell().Background("#087d91").Padding(4).AlignRight().Text("Sisa Bayar").Bold().FontColor(Colors.White);
                    });

                    var idx = 1;
                    foreach (var row in rows)
                    {
                        var bg = idx % 2 == 0 ? "#f8fafc" : "#ffffff";

                        table.Cell().Background(bg).Padding(3).AlignCenter().Text(idx.ToString());
                        table.Cell().Background(bg).Padding(3).Column(c =>
                        {
                            c.Item().Text(row.DebtorName).Bold();
                            c.Item().Text($"Inv: {row.InvoiceNumber}").FontSize(6.5f).FontColor(Colors.Grey.Darken1);
                        });
                        table.Cell().Background(bg).Padding(3).Text(row.PaymentMethod);
                        table.Cell().Background(bg).Padding(3).Text(string.IsNullOrWhiteSpace(row.BankRecipient) ? "-" : row.BankRecipient);
                        table.Cell().Background(bg).Padding(3).Column(c =>
                        {
                            c.Item().Text(row.ReferenceNumber);
                            if (!string.IsNullOrWhiteSpace(row.CrossEntryNumber))
                            {
                                c.Item().Text($"AS: {row.CrossEntryNumber}").FontSize(6.5f).FontColor("#087d91");
                            }
                        });
                        table.Cell().Background(bg).Padding(3).AlignCenter().Text(row.PaymentDate.ToString("dd/MM/yyyy"));
                        table.Cell().Background(bg).Padding(3).AlignRight().Text($"Rp {row.TotalTagihan:N0}");
                        table.Cell().Background(bg).Padding(3).AlignRight().Text($"Rp {row.TotalPembayaran:N0}").Bold().FontColor("#16a34a");
                        table.Cell().Background(bg).Padding(3).AlignRight().Text($"Rp {row.SisaPembayaran:N0}").FontColor(row.SisaPembayaran > 0 ? "#dc2626" : "#000000");

                        idx++;
                    }

                    if (rows.Count == 0)
                    {
                        table.Cell().ColumnSpan(9).Padding(16).AlignCenter().Text("Tidak ada data pembayaran yang sesuai kriteria filter.").Italic();
                    }
                });

                page.Footer().Row(r =>
                {
                    r.RelativeItem().Text("Quilvian Hospital System — Modul Finance V2").FontSize(7).FontColor(Colors.Grey.Darken1);
                    r.RelativeItem().AlignRight().Text(x =>
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

    public static byte[] BuildDetailPdf(PaymentArDetailResponse detail)
    {
        return Document.Create(container =>
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
                            c.Item().Text("DETAIL LAPORAN PEMBAYARAN AR").FontSize(14).Bold().FontColor("#087d91");
                            c.Item().Text("Rincian pasien dan Invoice AR pada transaksi pembayaran yang dipilih.").FontSize(8).FontColor(Colors.Grey.Darken1);
                        });

                        r.ConstantItem(200).AlignRight().Column(c =>
                        {
                            c.Item().Text($"Tanggal: {detail.PaymentDate:dd/MM/yyyy}").FontSize(8).Bold();
                            c.Item().Text($"Dicetak: {DateTime.Now:dd/MM/yyyy HH:mm} WIB").FontSize(7).FontColor(Colors.Grey.Darken1);
                        });
                    });

                    // 4 Mini Summary Cards
                    col.Item().PaddingVertical(8).Row(r =>
                    {
                        r.RelativeItem().Border(1).BorderColor("#cbd5e1").Background("#f8fafc").Padding(6).Column(c =>
                        {
                            c.Item().Text("Jumlah Detail").FontSize(7).FontColor(Colors.Grey.Darken2);
                            c.Item().Text($"{detail.Summary.JumlahDetail} Pasien").FontSize(11).Bold().FontColor("#087d91");
                        });
                        r.Spacing(6);
                        r.RelativeItem().Border(1).BorderColor("#cbd5e1").Background("#f8fafc").Padding(6).Column(c =>
                        {
                            c.Item().Text("Total Piutang").FontSize(7).FontColor(Colors.Grey.Darken2);
                            c.Item().Text($"Rp {detail.Summary.TotalPiutang:N0}").FontSize(11).Bold().FontColor("#0f172a");
                        });
                        r.Spacing(6);
                        r.RelativeItem().Border(1).BorderColor("#cbd5e1").Background("#f8fafc").Padding(6).Column(c =>
                        {
                            c.Item().Text("Piutang Terbayar").FontSize(7).FontColor(Colors.Grey.Darken2);
                            c.Item().Text($"Rp {detail.Summary.PiutangTerbayar:N0}").FontSize(11).Bold().FontColor("#16a34a");
                        });
                        r.Spacing(6);
                        r.RelativeItem().Border(1).BorderColor("#cbd5e1").Background("#f8fafc").Padding(6).Column(c =>
                        {
                            c.Item().Text("Sisa Pembayaran").FontSize(7).FontColor(Colors.Grey.Darken2);
                            c.Item().Text($"Rp {detail.Summary.SisaPembayaran:N0}").FontSize(11).Bold().FontColor("#dc2626");
                        });
                    });

                    // Metadata Box
                    col.Item().Border(1).BorderColor("#cbd5e1").Background("#f1f5f9").Padding(8).Row(r =>
                    {
                        r.RelativeItem().Column(c =>
                        {
                            c.Item().Text(t => { t.Span("Penjamin: ").Bold(); t.Span(detail.DebtorName); });
                            c.Item().Text(t => { t.Span("Bank / Metode: ").Bold(); t.Span(detail.BankRecipient); });
                            c.Item().Text(t => { t.Span("Sumber: ").Bold(); t.Span(detail.PaymentMethod); });
                        });
                        r.RelativeItem().Column(c =>
                        {
                            c.Item().Text(t => { t.Span("No. Invoice AR: ").Bold(); t.Span(detail.InvoiceNumber); });
                            c.Item().Text(t => { t.Span("No. Receipt: ").Bold(); t.Span(detail.ReceiptNumber); });
                            c.Item().Text(t => { t.Span("No. Referensi: ").Bold(); t.Span(detail.ReferenceNumber); });
                            if (!string.IsNullOrWhiteSpace(detail.CrossEntryNumber))
                            {
                                c.Item().Text(t => { t.Span("No. Ayat Silang: ").Bold(); t.Span(detail.CrossEntryNumber); });
                            }
                        });
                    });
                });

                page.Content().PaddingVertical(6).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(24); // No
                        columns.RelativeColumn(3.5f); // Pasien
                        columns.RelativeColumn(2.5f); // No Billing
                        columns.RelativeColumn(2.5f); // No Invoice
                        columns.RelativeColumn(2f); // Tgl Tagihan
                        columns.RelativeColumn(2f); // Tgl Jatuh Tempo
                        columns.RelativeColumn(2f); // Pembayaran Ke
                        columns.RelativeColumn(2.5f); // Total Piutang
                        columns.RelativeColumn(2.5f); // Piutang Terbayar
                        columns.RelativeColumn(2.5f); // Sisa Pembayaran
                        columns.RelativeColumn(2f); // Status
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background("#087d91").Padding(4).AlignCenter().Text("No.").Bold().FontColor(Colors.White);
                        header.Cell().Background("#087d91").Padding(4).Text("Nama Pasien").Bold().FontColor(Colors.White);
                        header.Cell().Background("#087d91").Padding(4).Text("No. Billing").Bold().FontColor(Colors.White);
                        header.Cell().Background("#087d91").Padding(4).Text("No. Invoice").Bold().FontColor(Colors.White);
                        header.Cell().Background("#087d91").Padding(4).AlignCenter().Text("Tgl. Tagihan").Bold().FontColor(Colors.White);
                        header.Cell().Background("#087d91").Padding(4).AlignCenter().Text("Tgl. Jth Tempo").Bold().FontColor(Colors.White);
                        header.Cell().Background("#087d91").Padding(4).AlignCenter().Text("Bayar Ke").Bold().FontColor(Colors.White);
                        header.Cell().Background("#087d91").Padding(4).AlignRight().Text("Total Piutang").Bold().FontColor(Colors.White);
                        header.Cell().Background("#087d91").Padding(4).AlignRight().Text("Terbayar").Bold().FontColor(Colors.White);
                        header.Cell().Background("#087d91").Padding(4).AlignRight().Text("Sisa").Bold().FontColor(Colors.White);
                        header.Cell().Background("#087d91").Padding(4).AlignCenter().Text("Status").Bold().FontColor(Colors.White);
                    });

                    var idx = 1;
                    foreach (var item in detail.Items)
                    {
                        var bg = idx % 2 == 0 ? "#f8fafc" : "#ffffff";

                        table.Cell().Background(bg).Padding(3).AlignCenter().Text(idx.ToString());
                        table.Cell().Background(bg).Padding(3).Column(c =>
                        {
                            c.Item().Text(item.PatientName).Bold();
                            c.Item().Text($"RM: {item.MedicalRecordNumber}").FontSize(6.5f).FontColor(Colors.Grey.Darken1);
                        });
                        table.Cell().Background(bg).Padding(3).Text(item.BillingNumber);
                        table.Cell().Background(bg).Padding(3).Text(item.InvoiceNumber);
                        table.Cell().Background(bg).Padding(3).AlignCenter().Text(item.BillingDate?.ToString("dd/MM/yyyy") ?? "-");
                        table.Cell().Background(bg).Padding(3).AlignCenter().Text(item.DueDate?.ToString("dd/MM/yyyy") ?? "-");
                        table.Cell().Background(bg).Padding(3).AlignCenter().Text(item.PaymentSequence);
                        table.Cell().Background(bg).Padding(3).AlignRight().Text($"Rp {item.TotalPiutang:N0}");
                        table.Cell().Background(bg).Padding(3).AlignRight().Text($"Rp {item.PiutangTerbayar:N0}").Bold().FontColor("#16a34a");
                        table.Cell().Background(bg).Padding(3).AlignRight().Text($"Rp {item.SisaPembayaran:N0}").FontColor(item.SisaPembayaran > 0 ? "#dc2626" : "#000000");
                        table.Cell().Background(bg).Padding(3).AlignCenter().Text(item.Status);

                        idx++;
                    }

                    if (detail.Items.Count == 0)
                    {
                        table.Cell().ColumnSpan(11).Padding(16).AlignCenter().Text("Tidak ada rincian pasien pada batch ini.").Italic();
                    }
                });

                page.Footer().Row(r =>
                {
                    r.RelativeItem().Text("Quilvian Hospital System — Modul Finance V2").FontSize(7).FontColor(Colors.Grey.Darken1);
                    r.RelativeItem().AlignRight().Text(x =>
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
