using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.Services;

public sealed class StockTakeDocumentService
{
    private static readonly TimeZoneInfo ReportTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Jakarta");
    private static readonly CultureInfo ReportCulture = CultureInfo.GetCultureInfo("id-ID");

    public byte[] RenderReport(
        StockTakeSessionResponse session,
        IReadOnlyList<StockTakeResultResponse> discrepancies,
        string legalEntityName,
        string hospitalSiteName)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(discrepancies);
        var counts = session.ResultCounts ?? new StockTakeResultCountsResponse();

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
                    header.Item().PaddingTop(8).AlignCenter().Text("BERITA ACARA INVENTARISASI FISIK ASET").FontSize(13).Bold();
                    header.Item().AlignCenter().Text(session.SessionNumber).FontSize(10);
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

                        Row("Unit", session.OrganizationUnitName);
                        Row("Ruangan", session.LocationName ?? "Seluruh ruangan unit");
                        Row("Rencana pelaksanaan", $"{FormatDate(session.PlannedStartDate)} s.d. {FormatDate(session.PlannedEndDate)}");
                        Row("Dimulai", Describe(session.Started));
                        Row("Pemindaian ditutup", Describe(session.CountingClosed));
                        Row("Aset diharapkan", session.ExpectedCount.ToString(ReportCulture));
                        Row("Tim", string.Join(", ", session.TeamMembers.Select(x => $"{x.EmployeeName}{(x.IsLeader ? " (ketua)" : string.Empty)}")));
                    });

                    content.Item().Text("Ringkasan hasil").Bold();
                    content.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.ConstantColumn(28, Unit.Millimetre);
                        });

                        void Count(string label, int value)
                        {
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(label);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).AlignRight().Text(value.ToString(ReportCulture));
                        }

                        Count("Sesuai", counts.Sesuai);
                        Count("Sesuai, sedang dipinjam", counts.SesuaiSedangDipinjam);
                        Count("Tidak ditemukan", counts.TidakDitemukan);
                        Count("Beda lokasi", counts.BedaLokasi);
                        Count("Beda kondisi", counts.BedaKondisi);
                        Count("Tidak terdaftar (temuan)", counts.TidakTerdaftar);
                        table.Cell().Padding(3).Text("Jumlah hasil").Bold();
                        table.Cell().Padding(3).AlignRight().Text(counts.Total.ToString(ReportCulture)).Bold();
                    });

                    content.Item().Text($"Selisih ({discrepancies.Count})").Bold();
                    if (discrepancies.Count == 0)
                    {
                        content.Item().Text("Tidak ada selisih pada sesi ini.").Italic();
                    }
                    else
                    {
                        content.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(8, Unit.Millimetre);
                                columns.ConstantColumn(24, Unit.Millimetre);
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(3);
                            });

                            table.Header(head =>
                            {
                                foreach (var title in new[] { "No", "Kategori", "Aset / Temuan", "Tercatat", "Ditemukan", "Catatan Tim" })
                                {
                                    head.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text(title).Bold();
                                }
                            });

                            var number = 1;
                            foreach (var item in discrepancies)
                            {
                                var asset = item.AssetId.HasValue ? $"{item.InventoryNumber ?? "-"} · {item.AssetName}" : $"Temuan: {item.FindingDescription}";
                                Cell(table, number.ToString(ReportCulture));
                                Cell(table, item.CategoryName);
                                Cell(table, asset);
                                Cell(table, $"{item.ExpectedLocationName ?? "-"} · {item.ExpectedConditionName ?? "-"}");
                                Cell(table, $"{item.FoundLocationName ?? "-"} · {item.ObservedConditionName ?? "-"}");
                                Cell(table, item.TeamNote ?? "-");
                                number++;
                            }
                        });
                    }

                    content.Item().PaddingTop(6).Row(row =>
                    {
                        row.Spacing(8, Unit.Millimetre);
                        row.RelativeItem().Element(cell => ComposeSignature(cell, "Diajukan ketua tim", session.Submitted));
                        row.RelativeItem().Element(cell => ComposeSignature(cell, "Diverifikasi Kepala Bagian Aset", session.Verified));
                    });

                    content.Item().PaddingTop(4)
                        .Text("Verifikasi dilakukan di sistem dan berlaku sebagai persetujuan berita acara. Hasil opname tidak mengubah data aset; setiap selisih ditindaklanjuti lewat usulan.")
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

    private static void Cell(TableDescriptor table, string? value) =>
        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(string.IsNullOrWhiteSpace(value) ? "-" : value);

    private static void ComposeSignature(IContainer container, string title, StockTakeActorResponse? actor) =>
        container.Column(column =>
        {
            column.Item().Text(title).FontColor(Colors.Grey.Darken2);
            column.Item().PaddingTop(4).Text(actor?.Name ?? "-").Bold();
            column.Item().Text(actor?.PositionName ?? string.Empty).FontSize(8);
            column.Item().Text(actor != null ? FormatDateTime(actor.At) : "-").FontSize(8);
        });

    private static string Describe(StockTakeActorResponse? actor) =>
        actor == null ? "-" : $"{actor.Name ?? "-"} · {FormatDateTime(actor.At)}";

    private static string FormatDate(DateOnly date) => date.ToString("d MMMM yyyy", ReportCulture);

    private static string FormatDateTime(DateTime utc) =>
        $"{TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), ReportTimeZone).ToString("d MMMM yyyy HH.mm", ReportCulture)} WIB";
}
