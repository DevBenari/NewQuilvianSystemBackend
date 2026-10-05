using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using System.Text;
using System.Text.RegularExpressions;

namespace QuilvianSystemBackend.Seeders
{
    /// <summary>
    /// RJ-DOC-REV-BE-006 — kelompok ICD Diagnosa (DTD) dan pemetaannya ke <see cref="MstDiagnosis"/>.
    ///
    /// Sumber (milik rumah sakit, diterima 1 Oktober 2026):
    /// <list type="bullet">
    /// <item><c>SeedData/ICD10/icd_diagnosa_dtd.csv</c> — daftar kelompok DTD. Hanya baris yang
    /// berisi rentang ICD-10 (<c>no_terperinci</c>) yang merupakan kelompok diagnosa; baris tanpa
    /// rentang adalah kode tindakan ICD-9-CM dan tidak diimpor.</item>
    /// <item><c>SeedData/ICD10/icd10_dtd_mapping.csv</c> — pemetaan eksplisit kode ICD-10 → kelompok.</item>
    /// </list>
    ///
    /// Urutan penentuan kelompok sebuah kode: (1) pemetaan eksplisit; (2) rentang kode kelompok,
    /// yang tersempit menang bila lebih dari satu rentang memuatnya. Kode yang tidak masuk rentang
    /// mana pun dibiarkan tanpa kelompok.
    ///
    /// Idempoten: kelompok di-upsert menurut <c>SourceCode</c>; hanya diagnosa yang kelompoknya
    /// masih kosong yang dipetakan, sehingga koreksi manual tidak pernah ditimpa.
    /// </summary>
    public static class IcdDiagnosisGroupSeeder
    {
        private const string GroupFileName = "icd_diagnosa_dtd.csv";
        private const string MappingFileName = "icd10_dtd_mapping.csv";

        public static async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken ct = default)
        {
            var folder = ResolveFolder();
            if (folder == null) return;

            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var now = DateTime.UtcNow;

            var groupRows = ReadCsv(Path.Combine(folder, GroupFileName))
                .Where(x => !string.IsNullOrWhiteSpace(Get(x, "no_terperinci")) && !string.IsNullOrWhiteSpace(Get(x, "icddid")))
                .ToList();

            var existing = await db.MstDiagnosisGroups.ToDictionaryAsync(x => x.SourceCode, ct);
            foreach (var row in groupRows)
            {
                var sourceCode = Get(row, "icddid")!.Trim();
                var name = Truncate(Get(row, "diagnosa_name")?.Trim() ?? string.Empty, 300);
                var dtd = Truncate(Get(row, "no_dtd")?.Trim() ?? string.Empty, 20);
                var range = TruncateOptional(Get(row, "no_terperinci")?.Trim(), 500);
                var isImmunization = IsTrue(Get(row, "is_imunisasi"));
                var isAccident = IsTrue(Get(row, "penyebab_kecelakaan"));

                if (existing.TryGetValue(sourceCode, out var group))
                {
                    if (group.GroupName != name || group.DtdNumber != dtd || group.CodeRangeText != range
                        || group.IsImmunization != isImmunization || group.IsAccidentCause != isAccident)
                    {
                        group.GroupName = name;
                        group.DtdNumber = dtd;
                        group.CodeRangeText = range;
                        group.IsImmunization = isImmunization;
                        group.IsAccidentCause = isAccident;
                        group.UpdateDateTime = now;
                    }
                    continue;
                }

                group = new MstDiagnosisGroup
                {
                    SourceCode = sourceCode,
                    DtdNumber = dtd,
                    CodeRangeText = range,
                    GroupName = name,
                    IsImmunization = isImmunization,
                    IsAccidentCause = isAccident,
                    IsActive = true,
                    CreateDateTime = now
                };
                db.MstDiagnosisGroups.Add(group);
                existing[sourceCode] = group;
            }

            await db.SaveChangesAsync(ct);

            var explicitMap = ReadCsv(Path.Combine(folder, MappingFileName))
                .Select(x => new { Code = NormalizeCode(Get(x, "kode")), Group = Get(x, "icddid")?.Trim() })
                .Where(x => x.Code != null && !string.IsNullOrEmpty(x.Group) && existing.ContainsKey(x.Group!))
                .GroupBy(x => x.Code!)
                .ToDictionary(x => x.Key, x => existing[x.First().Group!].Id);

            var rangeIndex = existing.Values
                .Where(x => x.IsActive && !x.IsDelete)
                .SelectMany(x => ParseRanges(x.CodeRangeText).Select(r => (GroupId: x.Id, Range: r)))
                .ToList();

            var pending = await db.MstDiagnoses
                .Where(x => !x.IsDelete && x.DiagnosisGroupId == null && x.IcdVersion == "ICD-10")
                .ToListAsync(ct);

            foreach (var diagnosis in pending)
            {
                var code = NormalizeCode(diagnosis.DiagnosisCode);
                if (code == null) continue;

                Guid? groupId = explicitMap.TryGetValue(code, out var mapped) ? mapped : null;
                if (groupId == null && ToKey(code) is { } key)
                {
                    groupId = rangeIndex
                        .Where(x => x.Range.Contains(key))
                        .OrderBy(x => x.Range.Span)
                        .Select(x => (Guid?)x.GroupId)
                        .FirstOrDefault();
                }

                if (groupId != null)
                {
                    diagnosis.DiagnosisGroupId = groupId;
                    diagnosis.UpdateDateTime = now;
                }
            }

            await db.SaveChangesAsync(ct);
        }

        private static string? ResolveFolder()
        {
            foreach (var root in new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() })
            {
                var folder = Path.Combine(root, "SeedData", "ICD10");
                if (File.Exists(Path.Combine(folder, GroupFileName)) && File.Exists(Path.Combine(folder, MappingFileName)))
                    return folder;
            }

            return null;
        }

        // ---- Kode dan rentang ICD-10 -------------------------------------------------------

        /// <summary>Kunci banding kode: huruf, kategori dua digit, sub-kategori (-1 = tingkat kategori).</summary>
        internal readonly record struct CodeKey(char Letter, int Category, int Sub) : IComparable<CodeKey>
        {
            public int CompareTo(CodeKey other)
            {
                var c = Letter.CompareTo(other.Letter);
                if (c != 0) return c;
                c = Category.CompareTo(other.Category);
                return c != 0 ? c : Sub.CompareTo(other.Sub);
            }

            public int Ordinal => ((Letter - 'A') * 100 + Category) * 101 + (Sub + 1);
        }

        internal readonly record struct CodeRange(CodeKey Low, CodeKey High)
        {
            public int Span => High.Ordinal - Low.Ordinal;

            public bool Contains(CodeKey key)
            {
                // Kode tingkat kategori (A00) masuk bila kategorinya berada dalam rentang.
                if (key.Sub < 0)
                {
                    return new CodeKey(Low.Letter, Low.Category, -1).CompareTo(key) <= 0
                        && key.CompareTo(new CodeKey(High.Letter, High.Category, -1)) <= 0;
                }

                return Low.CompareTo(key) <= 0 && key.CompareTo(High) <= 0;
            }
        }

        private static readonly Regex CodePattern = new(@"^([A-Z])(\d{2})(?:\.(\d))?", RegexOptions.Compiled);
        private static readonly Regex PointPattern = new(@"^([A-Z])?(\d{1,2})?((?:\.\d)+)?$", RegexOptions.Compiled);

        private static string? NormalizeCode(string? code)
        {
            var value = code?.Replace(" ", string.Empty).Trim().ToUpperInvariant();
            return string.IsNullOrEmpty(value) ? null : value;
        }

        internal static CodeKey? ToKey(string code)
        {
            var m = CodePattern.Match(code);
            if (!m.Success) return null;
            return new CodeKey(m.Groups[1].Value[0], int.Parse(m.Groups[2].Value),
                m.Groups[3].Success ? int.Parse(m.Groups[3].Value) : -1);
        }

        /// <summary>
        /// Mengurai teks rentang sumber, contoh <c>A 00</c>, <c>C 91 - C 95</c>,
        /// <c>D 10 - D 12.0 - .5, .7 - .9, D 13</c>. Titik tanpa huruf/kategori melanjutkan
        /// kategori sebelumnya. Bagian yang tidak terbaca dilewati.
        /// </summary>
        internal static List<CodeRange> ParseRanges(string? text)
        {
            var result = new List<CodeRange>();
            var source = text?.Replace(" ", string.Empty).ToUpperInvariant();
            if (string.IsNullOrEmpty(source)) return result;

            (char Letter, int Category)? last = null;

            foreach (var part in source.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var points = new List<(char L, int N, int[] Subs)>();
                var valid = true;

                foreach (var end in part.Split('-'))
                {
                    var m = PointPattern.Match(end);
                    if (!m.Success || end.Length == 0) { valid = false; break; }

                    char letter;
                    int category;
                    if (!m.Groups[2].Success)
                    {
                        if (last == null || !m.Groups[3].Success) { valid = false; break; }
                        (letter, category) = last.Value;
                    }
                    else
                    {
                        if (m.Groups[1].Success) letter = m.Groups[1].Value[0];
                        else if (last != null) letter = last.Value.Letter;
                        else { valid = false; break; }
                        category = int.Parse(m.Groups[2].Value);
                    }

                    last = (letter, category);
                    var subs = m.Groups[3].Success
                        ? m.Groups[3].Value.Split('.', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray()
                        : Array.Empty<int>();
                    points.Add((letter, category, subs));
                }

                if (!valid || points.Count == 0) continue;

                var low = points[0];
                var high = points[^1];
                result.Add(new CodeRange(
                    new CodeKey(low.L, low.N, low.Subs.Length > 0 ? low.Subs[0] : -1),
                    new CodeKey(high.L, high.N, high.Subs.Length > 0 ? high.Subs[^1] : 99)));
            }

            return result;
        }

        // ---- CSV ---------------------------------------------------------------------------

        private static List<Dictionary<string, string>> ReadCsv(string path)
        {
            var lines = File.ReadAllLines(path, Encoding.UTF8);
            if (lines.Length == 0) return new();

            var header = SplitLine(lines[0]).Select(x => x.Trim().TrimStart('﻿')).ToArray();
            var rows = new List<Dictionary<string, string>>();
            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var cells = SplitLine(line);
                var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                for (var i = 0; i < header.Length && i < cells.Count; i++) row[header[i]] = cells[i];
                rows.Add(row);
            }

            return rows;
        }

        private static List<string> SplitLine(string line)
        {
            var cells = new List<string>();
            var current = new StringBuilder();
            var quoted = false;
            for (var i = 0; i < line.Length; i++)
            {
                var c = line[i];
                if (c == '"')
                {
                    if (quoted && i + 1 < line.Length && line[i + 1] == '"') { current.Append('"'); i++; }
                    else quoted = !quoted;
                }
                else if (c == ';' && !quoted)
                {
                    cells.Add(current.ToString());
                    current.Clear();
                }
                else current.Append(c);
            }

            cells.Add(current.ToString());
            return cells;
        }

        private static string? Get(Dictionary<string, string> row, string key) =>
            row.TryGetValue(key, out var value) ? value : null;

        private static bool IsTrue(string? value) =>
            string.Equals(value?.Trim(), "TRUE", StringComparison.OrdinalIgnoreCase);

        private static string Truncate(string value, int max) => value.Length <= max ? value : value[..max];

        private static string? TruncateOptional(string? value, int max) =>
            value == null ? null : Truncate(value, max);
    }
}
