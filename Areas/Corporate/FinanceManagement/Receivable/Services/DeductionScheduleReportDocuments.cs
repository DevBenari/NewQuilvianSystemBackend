using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;

/// <summary>
/// Generator dokumen PDF jadwal potongan gaji pegawai untuk keperluan serah terima ke tim penggajian (BE-FIN-116, FIN-DEC-219).
/// </summary>
public static class DeductionScheduleReportDocuments
{
    public static byte[] BuildSchedulePdf(DeductionScheduleResponse schedule)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(24);
                page.DefaultTextStyle(x => x.FontSize(8));

                page.Header().Column(headerCol =>
                {
                    headerCol.Item().Row(row =>
                    {
                        row.RelativeItem().Text("Jadwal Potongan Gaji Pegawai — Piutang RS")
                            .FontSize(14).Bold();
                        row.ConstantItem(150).AlignRight()
                            .Text($"Periode: {schedule.Period}").FontSize(10).Bold();
                    });

                    headerCol.Item().PaddingTop(4).PaddingBottom(6).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);

                    headerCol.Item().PaddingBottom(6).Text(
                        $"Total Terutang: Rp {schedule.TotalDueAmount:N0}   |   Jumlah Angsuran: {schedule.RowCount}   |   Dicetak: {DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC")
                        .FontSize(9).SemiBold();
                });

                page.Content().Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(24);         // No
                            columns.RelativeColumn(2.2f);       // Kode Angsuran
                            columns.RelativeColumn(1.8f);       // NIP
                            columns.RelativeColumn(2.8f);       // Nama Pegawai
                            columns.RelativeColumn(2.5f);       // Unit Kerja
                            columns.RelativeColumn(2.2f);       // No. Piutang
                            columns.ConstantColumn(28);         // Ke-
                            columns.RelativeColumn(2.0f);       // Nominal Terjadwal
                            columns.RelativeColumn(2.0f);       // Tunggakan Terbawa
                            columns.RelativeColumn(2.2f);       // Nominal Terutang
                        });

                        table.Header(header =>
                        {
                            string[] heads =
                            [
                                "No", "Kode Angsuran", "NIP", "Nama Pegawai", "Unit Kerja",
                                "No. Piutang", "Ke", "Terjadwal", "Tunggakan", "Terutang"
                            ];
                            foreach (var h in heads)
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text(h).Bold();
                            }
                        });

                        var idx = 1;
                        foreach (var row in schedule.Rows)
                        {
                            table.Cell().Padding(3).Text(idx.ToString());
                            table.Cell().Padding(3).Text(row.InstallmentCode);
                            table.Cell().Padding(3).Text(row.EmployeeNumber);
                            table.Cell().Padding(3).Text(row.EmployeeName);
                            table.Cell().Padding(3).Text(row.OrganizationUnitName);
                            table.Cell().Padding(3).Text(row.ReceivableNumber);
                            table.Cell().Padding(3).AlignCenter().Text(row.InstallmentNumber.ToString());
                            table.Cell().Padding(3).AlignRight().Text(row.ScheduledAmount.ToString("N0"));
                            table.Cell().Padding(3).AlignRight().Text(row.CarriedOverAmount.ToString("N0"));
                            table.Cell().Padding(3).AlignRight().Text(row.DueAmount.ToString("N0"));
                            idx++;
                        }

                        if (schedule.Rows.Count == 0)
                        {
                            table.Cell().ColumnSpan(10).Padding(12).AlignCenter()
                                .Text("Tidak ada jadwal potongan pada periode ini.").Italic();
                        }
                    });
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
}

