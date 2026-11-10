using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;

/// <summary>
/// Generator PDF slip gaji dari snapshot yang dibekukan (PAY-BE-09). Lisensi QuestPDF Community dikonfigurasi sekali di Program.cs.
/// Dokumen dibangun dari <see cref="PayrollPayslipSnapshot"/>, tidak pernah dari data gaji yang berjalan, sehingga hasil unduhan
/// selalu identik dengan slip yang diterbitkan.
/// </summary>
public static class PayrollPayslipDocuments
{
    public static byte[] BuildPdf(PayrollPayslipSnapshot slip)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(32);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Column(headerCol =>
                {
                    headerCol.Item().Row(row =>
                    {
                        row.RelativeItem().Text("SLIP GAJI").FontSize(16).Bold();
                        row.ConstantItem(200).AlignRight().Text($"No. {slip.PayslipNumber}").FontSize(9).SemiBold();
                    });
                    headerCol.Item().PaddingTop(4).Text($"Periode {slip.PeriodName} ({slip.PeriodStartDate:dd/MM/yyyy} - {slip.PeriodEndDate:dd/MM/yyyy})")
                        .FontSize(10);
                    headerCol.Item().PaddingTop(4).PaddingBottom(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                });

                page.Content().Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1.4f);
                            columns.RelativeColumn(3f);
                            columns.RelativeColumn(1.4f);
                            columns.RelativeColumn(3f);
                        });
                        Row(table, "NIP/No. Dokter", slip.EmployeeNumber, "Nama", slip.EmployeeName);
                        Row(table, "Unit", slip.DepartmentName ?? "-", "Jabatan", slip.PositionName ?? "-");
                        Row(table, "Status Pajak", slip.TaxStatus ?? "-", "Rekening", string.IsNullOrWhiteSpace(slip.BankName) ? "-" : $"{slip.BankName} {slip.BankAccountMasked}");
                    });

                    col.Item().PaddingTop(10).Text("Penghasilan").Bold().FontSize(10);
                    col.Item().Element(c => Lines(c, slip.Earnings, slip.TotalEarning, "Total Penghasilan"));

                    col.Item().PaddingTop(10).Text("Potongan").Bold().FontSize(10);
                    col.Item().Element(c => Lines(c, slip.Deductions, slip.TotalDeduction, "Total Potongan"));

                    col.Item().PaddingTop(12).Background(Colors.Grey.Lighten3).Padding(8).Row(row =>
                    {
                        row.RelativeItem().Text("GAJI BERSIH DITERIMA").Bold().FontSize(11);
                        row.ConstantItem(160).AlignRight().Text($"{slip.CurrencyCode} {slip.NetPay:N0}").Bold().FontSize(11);
                    });

                    col.Item().PaddingTop(14).Text(
                        "Dokumen ini dihasilkan sistem dari data yang telah disahkan dan bersifat rahasia. "
                        + "Hubungi bagian Payroll bila terdapat ketidaksesuaian.").FontSize(8).Italic();
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

    private static void Row(TableDescriptor table, string label1, string value1, string label2, string value2)
    {
        table.Cell().Padding(2).Text(label1).SemiBold();
        table.Cell().Padding(2).Text(value1);
        table.Cell().Padding(2).Text(label2).SemiBold();
        table.Cell().Padding(2).Text(value2);
    }

    private static void Lines(IContainer container, List<PayrollPayslipLine> lines, decimal total, string totalLabel)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(4f);
                columns.RelativeColumn(1.6f);
            });

            if (lines.Count == 0)
            {
                table.Cell().ColumnSpan(2).Padding(4).Text("-").Italic();
            }

            foreach (var line in lines)
            {
                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3)
                    .Text(line.IsProrated ? $"{line.Name} (prorata)" : line.Name);
                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).AlignRight().Text(line.Amount.ToString("N0"));
            }

            table.Cell().Padding(3).Text(totalLabel).Bold();
            table.Cell().Padding(3).AlignRight().Text(total.ToString("N0")).Bold();
        });
    }
}
