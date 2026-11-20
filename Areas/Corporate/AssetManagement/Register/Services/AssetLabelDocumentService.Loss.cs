using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;

public sealed partial class AssetLabelDocumentService
{
    public byte[] RenderLossReport(AssetConditionReportResponse report, string legalEntityName, string hospitalSiteName)
    {
        ArgumentNullException.ThrowIfNull(report);

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.PageColor(Colors.White);
                page.Margin(18, Unit.Millimetre);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Column(header =>
                {
                    header.Item().Text(legalEntityName).FontSize(11).Bold();
                    header.Item().Text(hospitalSiteName).FontSize(9).FontColor(Colors.Grey.Darken1);
                    header.Item().PaddingTop(8).AlignCenter().Text("BERITA ACARA KEHILANGAN ASET").FontSize(13).Bold();
                    header.Item().AlignCenter().Text(report.ReportNumber).FontSize(10);
                    header.Item().PaddingTop(6).LineHorizontal(0.75f).LineColor(Colors.Grey.Medium);
                });

                page.Content().PaddingTop(10).Column(content =>
                {
                    content.Spacing(10);
                    content.Item().Text("Dengan ini dinyatakan bahwa aset berikut tercatat hilang dan statusnya ditetapkan di sistem:");
                    content.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(48, Unit.Millimetre);
                            columns.RelativeColumn();
                        });

                        void Row(string label, string? value)
                        {
                            table.Cell().PaddingVertical(2).Text(label).FontColor(Colors.Grey.Darken2);
                            table.Cell().PaddingVertical(2).Text(string.IsNullOrWhiteSpace(value) ? "-" : value);
                        }

                        Row("Nomor inventaris", report.InventoryNumber);
                        Row("Nama aset", report.AssetName);
                        Row("Kelompok", report.GroupName);
                        Row("Unit", report.OrganizationUnitName);
                        Row("Ruangan", report.LocationName);
                        Row("Penanggung jawab", report.CustodianEmployeeName);
                        Row("Kondisi sebelumnya", report.PreviousConditionName);
                        Row("Sumber laporan", report.SourceName);
                        Row("Keterangan pelapor", report.Notes);
                        Row("Catatan persetujuan", report.DecisionReason);
                        Row("Berita acara dari unit", report.LossReport?.FileName);
                    });

                    content.Item().PaddingTop(6).Row(row =>
                    {
                        row.Spacing(12, Unit.Millimetre);
                        row.RelativeItem().Element(cell => ComposeSignature(cell, "Dilaporkan oleh", report.Reported?.Name, report.Reported?.PositionName, report.Reported?.At));
                        row.RelativeItem().Element(cell => ComposeSignature(cell, "Disetujui oleh", report.Decided?.Name, report.Decided?.PositionName, report.Decided?.At));
                    });

                    content.Item().PaddingTop(4)
                        .Text("Persetujuan di atas dilakukan di sistem dan berlaku sebagai bukti penetapan status hilang. Tanda tangan basah tidak diwajibkan.")
                        .FontSize(8)
                        .Italic()
                        .FontColor(Colors.Grey.Darken1);
                });

                page.Footer().AlignRight().Text(text =>
                {
                    text.Span("Halaman ").FontSize(8);
                    text.CurrentPageNumber().FontSize(8);
                    text.Span(" dari ").FontSize(8);
                    text.TotalPages().FontSize(8);
                });
            });
        }).GeneratePdf();
    }
}
