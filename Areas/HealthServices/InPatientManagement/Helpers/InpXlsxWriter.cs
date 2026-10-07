using System.IO.Compression;
using System.Text;
using System.Xml;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Helpers
{
    /// <summary>
    /// Penulis berkas <c>.xlsx</c> satu lembar yang minimal (<c>BE-RWI-184</c>): seluruh sel ditulis
    /// sebagai teks <c>inlineStr</c> lewat <see cref="ZipArchive"/> dan <see cref="XmlWriter"/>.
    /// </summary>
    /// <remarks>
    /// Sengaja tidak menambah paket NuGet: repository belum memiliki pustaka Excel, dan menambah
    /// dependensi adalah keputusan tersendiri. Format yang ditulis adalah SpreadsheetML (Office Open
    /// XML) standar yang dibuka Excel, LibreOffice, dan Google Sheets.
    /// </remarks>
    internal static class InpXlsxWriter
    {
        private const string MainNs = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        private const string RelNs = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        private const string PackageRelNs = "http://schemas.openxmlformats.org/package/2006/relationships";

        public const string ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        public static byte[] Write(string sheetName, IReadOnlyList<string> header, IEnumerable<IReadOnlyList<string?>> rows)
        {
            using var stream = new MemoryStream();
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
            {
                WriteEntry(archive, "[Content_Types].xml",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                    "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">" +
                    "<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>" +
                    "<Default Extension=\"xml\" ContentType=\"application/xml\"/>" +
                    "<Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/>" +
                    "<Override PartName=\"/xl/worksheets/sheet1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>" +
                    "</Types>");
                WriteEntry(archive, "_rels/.rels",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                    $"<Relationships xmlns=\"{PackageRelNs}\">" +
                    "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/>" +
                    "</Relationships>");
                WriteEntry(archive, "xl/_rels/workbook.xml.rels",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                    $"<Relationships xmlns=\"{PackageRelNs}\">" +
                    "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet1.xml\"/>" +
                    "</Relationships>");

                var workbook = archive.CreateEntry("xl/workbook.xml", CompressionLevel.Fastest);
                using (var writer = XmlWriter.Create(workbook.Open(), new XmlWriterSettings { Encoding = new UTF8Encoding(false) }))
                {
                    writer.WriteStartDocument(true);
                    writer.WriteStartElement("workbook", MainNs);
                    writer.WriteAttributeString("xmlns", "r", null, RelNs);
                    writer.WriteStartElement("sheets", MainNs);
                    writer.WriteStartElement("sheet", MainNs);
                    writer.WriteAttributeString("name", SafeSheetName(sheetName));
                    writer.WriteAttributeString("sheetId", "1");
                    writer.WriteAttributeString("id", RelNs, "rId1");
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }

                var sheet = archive.CreateEntry("xl/worksheets/sheet1.xml", CompressionLevel.Fastest);
                using (var writer = XmlWriter.Create(sheet.Open(), new XmlWriterSettings { Encoding = new UTF8Encoding(false) }))
                {
                    writer.WriteStartDocument(true);
                    writer.WriteStartElement("worksheet", MainNs);
                    writer.WriteStartElement("sheetData", MainNs);
                    var rowNumber = 1;
                    WriteRow(writer, rowNumber++, header);
                    foreach (var row in rows)
                        WriteRow(writer, rowNumber++, row);
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
            }

            return stream.ToArray();
        }

        private static void WriteRow(XmlWriter writer, int rowNumber, IReadOnlyList<string?> cells)
        {
            writer.WriteStartElement("row", MainNs);
            writer.WriteAttributeString("r", rowNumber.ToString());
            for (var i = 0; i < cells.Count; i++)
            {
                writer.WriteStartElement("c", MainNs);
                writer.WriteAttributeString("r", $"{ColumnName(i)}{rowNumber}");
                writer.WriteAttributeString("t", "inlineStr");
                writer.WriteStartElement("is", MainNs);
                writer.WriteStartElement("t", MainNs);
                writer.WriteAttributeString("xml", "space", null, "preserve");
                writer.WriteString(CleanXml(cells[i]));
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        private static void WriteEntry(ZipArchive archive, string path, string content)
        {
            var entry = archive.CreateEntry(path, CompressionLevel.Fastest);
            using var writer = new StreamWriter(entry.Open(), new UTF8Encoding(false));
            writer.Write(content);
        }

        private static string ColumnName(int index)
        {
            var name = string.Empty;
            index++;
            while (index > 0)
            {
                var modulo = (index - 1) % 26;
                name = (char)('A' + modulo) + name;
                index = (index - modulo) / 26;
            }
            return name;
        }

        /// <summary>Membuang karakter yang tidak sah di XML 1.0 supaya berkas tidak rusak.</summary>
        private static string CleanXml(string? value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            var builder = new StringBuilder(value.Length);
            foreach (var ch in value)
            {
                if (XmlConvert.IsXmlChar(ch)) builder.Append(ch);
            }
            return builder.ToString();
        }

        private static string SafeSheetName(string name)
        {
            var cleaned = new string(name.Where(ch => ch is not ('\\' or '/' or '?' or '*' or '[' or ']' or ':')).ToArray());
            if (string.IsNullOrWhiteSpace(cleaned)) cleaned = "Sheet1";
            return cleaned.Length > 31 ? cleaned[..31] : cleaned;
        }
    }
}
