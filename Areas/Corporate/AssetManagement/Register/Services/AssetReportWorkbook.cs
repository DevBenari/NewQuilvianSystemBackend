using ClosedXML.Excel;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;

public sealed record AssetReportColumn(string Header, string? DateFormat = null);

public sealed record AssetReportSheet(
    string Title,
    IReadOnlyList<string> InfoLines,
    IReadOnlyList<AssetReportColumn> Columns,
    IReadOnlyList<object?[]> Rows);

public sealed record AssetReportFile(byte[] Content, string FileName);

public static class AssetReportWorkbook
{
    public const string ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public static byte[] Build(AssetReportSheet sheet)
    {
        ArgumentNullException.ThrowIfNull(sheet);
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Laporan");

        worksheet.Cell(1, 1).Value = sheet.Title;
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 14;

        var row = 2;
        foreach (var line in sheet.InfoLines)
        {
            worksheet.Cell(row, 1).Value = line;
            row++;
        }

        var headerRow = row + 1;
        for (var index = 0; index < sheet.Columns.Count; index++)
        {
            var header = worksheet.Cell(headerRow, index + 1);
            header.Value = sheet.Columns[index].Header;
            header.Style.Font.Bold = true;
            header.Style.Fill.BackgroundColor = XLColor.LightGray;
            header.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        }

        var dataRow = headerRow + 1;
        foreach (var values in sheet.Rows)
        {
            for (var index = 0; index < sheet.Columns.Count && index < values.Length; index++)
            {
                var cell = worksheet.Cell(dataRow, index + 1);
                switch (values[index])
                {
                    case null:
                        break;
                    case int number:
                        cell.Value = number;
                        break;
                    case DateTime dateTime:
                        cell.Value = dateTime;
                        cell.Style.DateFormat.Format = sheet.Columns[index].DateFormat ?? "dd/mm/yyyy hh:mm";
                        break;
                    case DateOnly date:
                        cell.Value = date.ToDateTime(TimeOnly.MinValue);
                        cell.Style.DateFormat.Format = sheet.Columns[index].DateFormat ?? "dd/mm/yyyy";
                        break;
                    default:
                        cell.Value = values[index]!.ToString();
                        break;
                }
            }

            dataRow++;
        }

        if (sheet.Rows.Count > 0)
        {
            worksheet.Range(headerRow, 1, dataRow - 1, sheet.Columns.Count).SetAutoFilter();
        }

        worksheet.SheetView.FreezeRows(headerRow);
        worksheet.Columns(1, sheet.Columns.Count).AdjustToContents(headerRow, Math.Min(dataRow, headerRow + 500));

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
