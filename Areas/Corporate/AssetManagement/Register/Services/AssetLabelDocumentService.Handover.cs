using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;

public sealed partial class AssetLabelDocumentService
{
    private static readonly TimeZoneInfo ReportTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Jakarta");
    private static readonly CultureInfo ReportCulture = CultureInfo.GetCultureInfo("id-ID");

    public byte[] RenderHandoverReport(AssetHandoverResponse handover, string legalEntityName, string hospitalSiteName)
    {
        ArgumentNullException.ThrowIfNull(handover);

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
                    header.Item().PaddingTop(8).AlignCenter().Text("BERITA ACARA PENYERAHAN ASET").FontSize(13).Bold();
                    header.Item().AlignCenter().Text(handover.HandoverNumber).FontSize(10);
                    header.Item().PaddingTop(6).LineHorizontal(0.75f).LineColor(Colors.Grey.Medium);
                });

                page.Content().PaddingTop(10).Column(content =>
                {
                    content.Spacing(10);
                    content.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(42, Unit.Millimetre);
                            columns.RelativeColumn();
                        });

                        void Row(string label, string? value)
                        {
                            table.Cell().PaddingVertical(2).Text(label).FontColor(Colors.Grey.Darken2);
                            table.Cell().PaddingVertical(2).Text(string.IsNullOrWhiteSpace(value) ? "-" : value);
                        }

                        Row("Tujuan", handover.PurposeName);
                        Row("Unit tujuan", handover.TargetOrganizationUnitName);
                        Row("Ruangan", handover.TargetLocationName);
                        Row("Penanggung jawab", handover.TargetCustodianEmployeeName);
                        Row("Jumlah aset", handover.ItemCount.ToString(ReportCulture));
                        Row("Catatan", handover.Notes);
                    });

                    content.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(10, Unit.Millimetre);
                            columns.ConstantColumn(40, Unit.Millimetre);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.ConstantColumn(28, Unit.Millimetre);
                        });

                        table.Header(head =>
                        {
                            foreach (var title in new[] { "No", "No. Inventaris", "Nama Aset", "Kelompok", "Kondisi" })
                            {
                                head.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text(title).Bold();
                            }
                        });

                        var number = 1;
                        foreach (var item in handover.Items)
                        {
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(number.ToString(ReportCulture));
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(item.InventoryNumber ?? "-");
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(item.AssetName);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(item.GroupName ?? "-");
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(item.ConditionAtHandoverName);
                            number++;
                        }
                    });

                    content.Item().PaddingTop(6).Row(row =>
                    {
                        row.Spacing(12, Unit.Millimetre);
                        row.RelativeItem().Element(cell => ComposeApproval(cell, "Diserahkan oleh", handover.HandedOver));
                        row.RelativeItem().Element(cell => ComposeApproval(cell, "Diterima dan dikonfirmasi oleh", handover.Confirmed));
                    });

                    content.Item().PaddingTop(4)
                        .Text("Persetujuan di atas dilakukan di sistem dan berlaku sebagai bukti serah terima. Tanda tangan basah tidak diwajibkan.")
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

    private static void ComposeApproval(IContainer container, string title, AssetHandoverActorResponse? step) =>
        ComposeSignature(container, title, step?.Name, step?.PositionName, step?.At);

    private static void ComposeSignature(IContainer container, string title, string? name, string? positionName, DateTime? at) =>
        container.Border(0.75f).BorderColor(Colors.Grey.Lighten1).Padding(8).Column(column =>
        {
            column.Spacing(3);
            column.Item().Text(title).FontColor(Colors.Grey.Darken2);
            column.Item().PaddingTop(4).Text(name ?? "-").Bold();
            column.Item().Text(positionName ?? "Jabatan tidak tercatat");
            column.Item().Text(at is { } value ? FormatReportTime(value) : "-");
        });

    private static string FormatReportTime(DateTime utc)
    {
        var value = utc.Kind == DateTimeKind.Utc ? utc : DateTime.SpecifyKind(utc, DateTimeKind.Utc);
        var local = TimeZoneInfo.ConvertTimeFromUtc(value, ReportTimeZone);
        return $"{local.ToString("dd MMMM yyyy, HH.mm", ReportCulture)} WIB";
    }
}
