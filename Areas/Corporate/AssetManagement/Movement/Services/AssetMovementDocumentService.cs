using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Movement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Movement.Services;

public sealed class AssetMovementDocumentService
{
    private static readonly TimeZoneInfo ReportTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Jakarta");
    private static readonly CultureInfo ReportCulture = CultureInfo.GetCultureInfo("id-ID");

    public byte[] RenderTransferReport(AssetTransferResponse transfer, string legalEntityName, string hospitalSiteName)
    {
        ArgumentNullException.ThrowIfNull(transfer);

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
                    header.Item().PaddingTop(8).AlignCenter().Text("BERITA ACARA SERAH TERIMA MUTASI ASET").FontSize(13).Bold();
                    header.Item().AlignCenter().Text(transfer.TransferNumber).FontSize(10);
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

                        Row("Unit asal", transfer.SourceOrganizationUnitName);
                        Row("Unit tujuan", transfer.TargetOrganizationUnitName);
                        Row("Ruangan tujuan", transfer.TargetLocationName);
                        Row("Penanggung jawab", transfer.TargetCustodianEmployeeName);
                        Row("Alasan", $"{transfer.ReasonName} — {transfer.ReasonNote}");
                        Row("Mutasi yang dikoreksi", transfer.CorrectsTransferNumber);
                        Row("Jumlah aset", transfer.ItemCount.ToString(ReportCulture));
                    });

                    content.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(10, Unit.Millimetre);
                            columns.ConstantColumn(38, Unit.Millimetre);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.ConstantColumn(26, Unit.Millimetre);
                            columns.ConstantColumn(26, Unit.Millimetre);
                        });

                        table.Header(head =>
                        {
                            foreach (var title in new[] { "No", "No. Inventaris", "Nama Aset", "Kelompok", "Kondisi Diajukan", "Kondisi Diterima" })
                            {
                                head.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text(title).Bold();
                            }
                        });

                        var number = 1;
                        foreach (var item in transfer.Items)
                        {
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(number.ToString(ReportCulture));
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(item.InventoryNumber ?? "-");
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(item.AssetName);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(item.GroupName ?? "-");
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(item.ConditionAtSubmitName);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(item.ConditionAtReceiptName ?? "-");
                            number++;
                        }
                    });

                    content.Item().PaddingTop(6).Row(row =>
                    {
                        row.Spacing(6, Unit.Millimetre);
                        row.RelativeItem().Element(cell => ComposeSignature(cell, "Diajukan oleh", transfer.Requested));
                        row.RelativeItem().Element(cell => ComposeSignature(cell, "Disetujui Kepala Unit asal", transfer.UnitApproved));
                        row.RelativeItem().Element(cell => ComposeSignature(cell, "Disetujui Bagian Aset", transfer.AssetHeadApproved));
                        row.RelativeItem().Element(cell => ComposeSignature(cell, "Diterima Kepala Unit tujuan", transfer.Received));
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

    public byte[] RenderLoanReport(AssetLoanResponse loan, string legalEntityName, string hospitalSiteName)
    {
        ArgumentNullException.ThrowIfNull(loan);
        var returned = loan.ReturnConfirmed != null;
        var title = returned ? "BERITA ACARA PEMINJAMAN DAN PENGEMBALIAN ASET" : "BERITA ACARA PEMINJAMAN ASET";

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
                    header.Item().PaddingTop(8).AlignCenter().Text(title).FontSize(13).Bold();
                    header.Item().AlignCenter().Text(loan.LoanNumber).FontSize(10);
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

                        Row("Unit pemilik", loan.OwnerOrganizationUnitName);
                        Row("Unit peminjam", loan.BorrowerOrganizationUnitName);
                        Row("Tujuan peminjaman", loan.Purpose);
                        Row("Tanggal mulai", FormatDate(loan.StartDate));
                        Row("Rencana tanggal kembali", loan.PlannedReturnDate == loan.OriginalPlannedReturnDate
                            ? FormatDate(loan.PlannedReturnDate)
                            : $"{FormatDate(loan.PlannedReturnDate)} (semula {FormatDate(loan.OriginalPlannedReturnDate)})");
                        Row("Jumlah aset", loan.ItemCount.ToString(ReportCulture));
                    });

                    content.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(10, Unit.Millimetre);
                            columns.ConstantColumn(38, Unit.Millimetre);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.ConstantColumn(26, Unit.Millimetre);
                            columns.ConstantColumn(26, Unit.Millimetre);
                        });

                        table.Header(head =>
                        {
                            foreach (var heading in new[] { "No", "No. Inventaris", "Nama Aset", "Kelompok", "Kondisi Diambil", "Kondisi Kembali" })
                            {
                                head.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text(heading).Bold();
                            }
                        });

                        var number = 1;
                        foreach (var item in loan.Items)
                        {
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(number.ToString(ReportCulture));
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(item.InventoryNumber ?? "-");
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(item.AssetName);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(item.GroupName ?? "-");
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(item.ConditionAtPickupName);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(item.ReturnConditionName ?? "-");
                            number++;
                        }
                    });

                    content.Item().PaddingTop(6).Row(row =>
                    {
                        row.Spacing(6, Unit.Millimetre);
                        row.RelativeItem().Element(cell => ComposeSignature(cell, "Diajukan oleh", loan.Requested));
                        row.RelativeItem().Element(cell => ComposeSignature(cell, "Disetujui Kepala Unit pemilik", loan.Approved));
                        row.RelativeItem().Element(cell => ComposeSignature(cell, "Diambil Kepala Unit peminjam", loan.PickedUp));
                    });

                    content.Item().Row(row =>
                    {
                        row.Spacing(6, Unit.Millimetre);
                        row.RelativeItem().Element(cell => ComposeSignature(cell, "Diserahkan kembali Kepala Unit peminjam", loan.Returned));
                        row.RelativeItem().Element(cell => ComposeSignature(cell, "Diterima kembali Kepala Unit pemilik", loan.ReturnConfirmed));
                        row.RelativeItem();
                    });

                    content.Item().PaddingTop(4)
                        .Text("Persetujuan di atas dilakukan di sistem dan berlaku sebagai bukti serah terima. Aset tetap tercatat milik unit pemilik selama dipinjam. Tanda tangan basah tidak diwajibkan.")
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

    private static string FormatDate(DateOnly value) => value.ToString("dd MMMM yyyy", ReportCulture);

    private static void ComposeSignature(IContainer container, string title, AssetMovementActorResponse? step) =>
        container.Border(0.75f).BorderColor(Colors.Grey.Lighten1).Padding(6).Column(column =>
        {
            column.Spacing(3);
            column.Item().Text(title).FontSize(8).FontColor(Colors.Grey.Darken2);
            column.Item().PaddingTop(4).Text(step?.Name ?? "-").Bold();
            column.Item().Text(step?.PositionName ?? "Jabatan tidak tercatat").FontSize(8);
            column.Item().Text(step is { } value ? FormatReportTime(value.At) : "-").FontSize(8);
        });

    private static string FormatReportTime(DateTime utc)
    {
        var value = utc.Kind == DateTimeKind.Utc ? utc : DateTime.SpecifyKind(utc, DateTimeKind.Utc);
        var local = TimeZoneInfo.ConvertTimeFromUtc(value, ReportTimeZone);
        return $"{local.ToString("dd MMMM yyyy, HH.mm", ReportCulture)} WIB";
    }
}
