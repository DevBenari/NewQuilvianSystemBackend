using System.Text;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Readers;

/// <summary>
/// BE-FIN-080, FIN-DES-093: membaca CSV dengan kemampuan bawaan .NET — nol paket (FIN-DEC-140).
/// Penguraian berpegang pada RFC 4180 (tanda kutip ganda membungkus sel berisi koma/baris baru,
/// kutip ganda ganda <c>""</c> adalah escape untuk satu kutip literal) supaya nilai sel apa pun
/// yang sah pada CSV terbaca benar. Nol penguraian angka/tanggal di sini — nilai dipulangkan
/// sebagai teks mentah; penguraian format adalah tanggung jawab lapisan validasi (BE-FIN-081),
/// bukan pembaca ini (02-backend-architecture.md M.8).
/// </summary>
public sealed class CsvOpeningItemFileReader : IOpeningItemFileReader
{
    private static readonly string[] AcceptedMediaTypes =
    [
        "text/csv", "application/csv", "application/vnd.ms-excel", "text/plain"
    ];

    public bool CanRead(string? mediaType, string? fileExtension)
    {
        var extension = fileExtension?.TrimStart('.');
        if (!string.IsNullOrWhiteSpace(extension) && string.Equals(extension, "csv", StringComparison.OrdinalIgnoreCase))
            return true;

        return !string.IsNullOrWhiteSpace(mediaType) &&
               AcceptedMediaTypes.Contains(mediaType.Trim(), StringComparer.OrdinalIgnoreCase);
    }

    public List<OpeningItemRawRow> Read(Stream stream)
    {
        var records = ParseRecords(stream);
        var rows = new List<OpeningItemRawRow>();
        if (records.Count == 0)
            return rows;

        var header = records[0];

        for (var i = 1; i < records.Count; i++)
        {
            var fields = records[i];

            // Baris kosong (umum: baris kosong di tengah berkas, atau baris terakhir yang
            // kosong akibat newline penutup) dilewati tanpa mengganggu penomoran baris lain —
            // nomor barisnya tetap mengikuti posisi fisik pada berkas asal.
            if (fields.Count == 1 && fields[0].Length == 0)
                continue;

            var cells = new Dictionary<string, string>(header.Count, StringComparer.OrdinalIgnoreCase);
            for (var col = 0; col < header.Count; col++)
                cells[header[col]] = col < fields.Count ? fields[col] : string.Empty;

            rows.Add(new OpeningItemRawRow { RowNumber = i + 1, Cells = cells });
        }

        return rows;
    }

    /// <summary>
    /// Pengurai CSV satu-lintasan: mengenali kutip ganda sebagai pembungkus sel (boleh memuat
    /// koma dan baris baru), <c>""</c> di dalam kutip sebagai satu kutip literal, dan CRLF/LF
    /// keduanya sebagai akhir baris.
    /// </summary>
    private static List<List<string>> ParseRecords(Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        var records = new List<List<string>>();
        var currentRecord = new List<string>();
        var field = new StringBuilder();
        var insideQuotes = false;
        var recordHasContent = false;

        void EndField()
        {
            currentRecord.Add(field.ToString());
            field.Clear();
        }

        void EndRecord()
        {
            EndField();
            records.Add(currentRecord);
            currentRecord = [];
            recordHasContent = false;
        }

        int ch;
        while ((ch = reader.Read()) != -1)
        {
            var c = (char)ch;
            recordHasContent = true;

            if (insideQuotes)
            {
                if (c == '"')
                {
                    if (reader.Peek() == '"')
                    {
                        reader.Read();
                        field.Append('"');
                    }
                    else
                    {
                        insideQuotes = false;
                    }
                }
                else
                {
                    field.Append(c);
                }
                continue;
            }

            switch (c)
            {
                case '"':
                    insideQuotes = true;
                    break;
                case ',':
                    EndField();
                    break;
                case '\r':
                    if (reader.Peek() == '\n') reader.Read();
                    EndRecord();
                    break;
                case '\n':
                    EndRecord();
                    break;
                default:
                    field.Append(c);
                    break;
            }
        }

        if (recordHasContent || field.Length > 0 || currentRecord.Count > 0)
            EndRecord();

        return records;
    }
}
