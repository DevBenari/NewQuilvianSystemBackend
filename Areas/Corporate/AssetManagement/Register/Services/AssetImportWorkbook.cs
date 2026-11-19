using System.Globalization;
using ClosedXML.Excel;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Services;

public sealed record AssetImportColumn(string Key, string Header, bool Required, string Description, string Example);

public sealed class AssetImportParsedRow
{
    public int RowNumber { get; init; }
    public Dictionary<string, string> Values { get; init; } = new(StringComparer.Ordinal);

    public string Text(string key) => Values.TryGetValue(key, out var value) ? value : string.Empty;
}

public sealed record AssetImportParseResult(bool HeaderValid, List<AssetImportParsedRow> Rows);

public sealed record AssetImportReferenceRow(string Code, string Name, string? Extra);

public sealed record AssetImportReferences(
    IReadOnlyList<AssetImportReferenceRow> Groups,
    IReadOnlyList<AssetImportReferenceRow> OwnershipTypes,
    IReadOnlyList<AssetImportReferenceRow> Locations);

public static class AssetImportWorkbook
{
    public const string DataSheetName = "Data Aset";
    public const string GuideSheetName = "Petunjuk";
    public const string ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    public const string TemplateFileName = "templat-impor-aset.xlsx";

    public const string GroupCode = "groupCode";
    public const string OwnershipCode = "ownershipCode";
    public const string AssetName = "assetName";
    public const string Description = "description";
    public const string Brand = "brand";
    public const string ModelType = "modelType";
    public const string SerialNumber = "serialNumber";
    public const string Manufacturer = "manufacturer";
    public const string DistributionPermitNumber = "distributionPermitNumber";
    public const string ManufactureYear = "manufactureYear";
    public const string DistributorName = "distributorName";
    public const string SupplierName = "supplierName";
    public const string AcquisitionDate = "acquisitionDate";
    public const string ReadyForUseDate = "readyForUseDate";
    public const string AcquisitionCost = "acquisitionCost";
    public const string AccumulatedDepreciation = "accumulatedDepreciation";
    public const string UnitCode = "unitCode";
    public const string LocationCode = "locationCode";
    public const string CustodianNumber = "custodianNumber";

    public static readonly IReadOnlyList<AssetImportColumn> Columns =
    [
        new(GroupCode, "Kode Kelompok", true, "Kode inventaris kelompok aset (lihat lembar Kode Kelompok)", "ALKMED"),
        new(OwnershipCode, "Kode Kepemilikan", true, "Kode jenis kepemilikan (lihat lembar Kode Kepemilikan)", "MILIK"),
        new(AssetName, "Nama Aset", true, "Nama aset, paling panjang 250 karakter", "Monitor Pasien"),
        new(Description, "Keterangan", false, "Paling panjang 1000 karakter", "Ruang ICU bed 3"),
        new(Brand, "Merek", false, "Wajib untuk alat medis", "Mindray"),
        new(ModelType, "Tipe/Model", false, "Wajib untuk alat medis", "uMEC 12"),
        new(SerialNumber, "Nomor Seri", false, "Wajib untuk alat medis; merek + nomor seri harus unik", "SN-2019-0001"),
        new(Manufacturer, "Produsen", false, "Wajib untuk alat medis", "Shenzhen Mindray"),
        new(DistributionPermitNumber, "Nomor Izin Edar", false, "AKL/AKD; wajib untuk alat medis", "AKL 20501910123"),
        new(ManufactureYear, "Tahun Pembuatan", false, "Empat angka", "2019"),
        new(DistributorName, "Distributor", false, "Paling panjang 150 karakter", "PT Distributor Medis"),
        new(SupplierName, "Pemasok", false, "Paling panjang 200 karakter", "PT Pemasok Alat"),
        new(AcquisitionDate, "Tanggal Perolehan", true, "Format tanggal Excel atau YYYY-MM-DD", "2019-03-01"),
        new(ReadyForUseDate, "Tanggal Siap Pakai", true, "Tidak boleh sebelum tanggal perolehan dan tidak boleh sesudah tanggal awal", "2019-03-15"),
        new(AcquisitionCost, "Harga Perolehan", true, "Rupiah, lebih dari 0, paling banyak 2 angka desimal", "60000000"),
        new(AccumulatedDepreciation, "Akumulasi Penyusutan", true, "Rupiah sampai tanggal awal; 0 bila tidak disusutkan", "45000000"),
        new(UnitCode, "Kode Unit", true, "Kode unit organisasi penempatan", "U9KOL"),
        new(LocationCode, "Kode Lokasi", true, "Kode lokasi milik unit itu (lihat lembar Kode Lokasi)", "U9KOL-A"),
        new(CustodianNumber, "NIP Penanggung Jawab", true, "Nomor induk atau kode pegawai penanggung jawab", "198706152010011001")
    ];

    public static byte[] BuildTemplate(AssetImportReferences references, DateOnly? cutoverDate)
    {
        ArgumentNullException.ThrowIfNull(references);
        using var workbook = new XLWorkbook();

        var data = workbook.Worksheets.Add(DataSheetName);
        for (var index = 0; index < Columns.Count; index++)
        {
            var column = Columns[index];
            var header = data.Cell(1, index + 1);
            header.Value = column.Header;
            header.Style.Font.Bold = true;
            header.Style.Fill.BackgroundColor = column.Required ? XLColor.LightGreen : XLColor.LightGray;
            data.Cell(2, index + 1).Value = column.Example;
        }

        data.SheetView.FreezeRows(1);
        data.Columns().AdjustToContents();

        var guide = workbook.Worksheets.Add(GuideSheetName);
        guide.Cell(1, 1).Value = "Kolom";
        guide.Cell(1, 2).Value = "Wajib";
        guide.Cell(1, 3).Value = "Keterangan";
        guide.Cell(1, 4).Value = "Contoh";
        guide.Row(1).Style.Font.Bold = true;
        for (var index = 0; index < Columns.Count; index++)
        {
            var column = Columns[index];
            guide.Cell(index + 2, 1).Value = column.Header;
            guide.Cell(index + 2, 2).Value = column.Required ? "Ya" : "Tidak";
            guide.Cell(index + 2, 3).Value = column.Description;
            guide.Cell(index + 2, 4).Value = column.Example;
        }

        var noteRow = Columns.Count + 3;
        guide.Cell(noteRow, 1).Value = "Tanggal awal";
        guide.Cell(noteRow, 3).Value = cutoverDate is { } cutover
            ? cutover.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            : "Belum diatur di Pengaturan Aset; impor ditolak sampai diatur.";
        guide.Cell(noteRow + 1, 1).Value = "Aturan";
        guide.Cell(noteRow + 1, 3).Value = "Hapus baris contoh di lembar Data Aset. Satu baris salah membuat seluruh berkas ditolak. Paling banyak 5.000 baris per berkas.";
        guide.Columns().AdjustToContents();

        AddReferenceSheet(workbook, "Kode Kelompok", ["Kode Kelompok", "Nama Kelompok", "Klasifikasi"], references.Groups);
        AddReferenceSheet(workbook, "Kode Kepemilikan", ["Kode Kepemilikan", "Nama", "Milik RS"], references.OwnershipTypes);
        AddReferenceSheet(workbook, "Kode Lokasi", ["Kode Lokasi", "Nama Lokasi", "Kode Unit"], references.Locations);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public static AssetImportParseResult Parse(Stream content)
    {
        ArgumentNullException.ThrowIfNull(content);
        using var workbook = new XLWorkbook(content);
        if (!workbook.TryGetWorksheet(DataSheetName, out var sheet))
        {
            return new AssetImportParseResult(false, []);
        }

        for (var index = 0; index < Columns.Count; index++)
        {
            var header = sheet.Cell(1, index + 1).GetString().Trim();
            if (!string.Equals(header, Columns[index].Header, StringComparison.OrdinalIgnoreCase))
            {
                return new AssetImportParseResult(false, []);
            }
        }

        var rows = new List<AssetImportParsedRow>();
        var lastRow = sheet.LastRowUsed()?.RowNumber() ?? 1;
        for (var rowNumber = 2; rowNumber <= lastRow; rowNumber++)
        {
            var row = new AssetImportParsedRow { RowNumber = rowNumber };
            var hasValue = false;
            for (var index = 0; index < Columns.Count; index++)
            {
                var value = ReadCell(sheet.Cell(rowNumber, index + 1));
                if (value.Length > 0)
                {
                    hasValue = true;
                }

                row.Values[Columns[index].Key] = value;
            }

            if (hasValue)
            {
                rows.Add(row);
            }
        }

        return new AssetImportParseResult(true, rows);
    }

    public static bool TryParseDate(string value, out DateOnly date)
    {
        string[] formats = ["yyyy-MM-dd", "dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy", "d-M-yyyy"];
        return DateOnly.TryParseExact(value.Trim(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
    }

    public static bool TryParseAmount(string value, out decimal amount)
    {
        var text = value.Trim().Replace("Rp", string.Empty, StringComparison.OrdinalIgnoreCase).Replace(" ", string.Empty);
        var indonesian = text.Contains(',') || System.Text.RegularExpressions.Regex.IsMatch(text, @"^-?\d{1,3}(\.\d{3})+$");
        return decimal.TryParse(
            text,
            NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands,
            indonesian ? CultureInfo.GetCultureInfo("id-ID") : CultureInfo.InvariantCulture,
            out amount);
    }

    private static string ReadCell(IXLCell cell)
    {
        if (cell.IsEmpty())
        {
            return string.Empty;
        }

        if (cell.DataType == XLDataType.DateTime)
        {
            return DateOnly.FromDateTime(cell.GetDateTime()).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        if (cell.DataType == XLDataType.Number)
        {
            return cell.GetDouble().ToString("0.##", CultureInfo.InvariantCulture);
        }

        return cell.GetString().Trim();
    }

    private static void AddReferenceSheet(XLWorkbook workbook, string name, string[] headers, IReadOnlyList<AssetImportReferenceRow> rows)
    {
        var sheet = workbook.Worksheets.Add(name);
        for (var index = 0; index < headers.Length; index++)
        {
            sheet.Cell(1, index + 1).Value = headers[index];
        }

        sheet.Row(1).Style.Font.Bold = true;
        for (var index = 0; index < rows.Count; index++)
        {
            sheet.Cell(index + 2, 1).Value = rows[index].Code;
            sheet.Cell(index + 2, 2).Value = rows[index].Name;
            sheet.Cell(index + 2, 3).Value = rows[index].Extra ?? string.Empty;
        }

        sheet.Columns().AdjustToContents();
    }
}
