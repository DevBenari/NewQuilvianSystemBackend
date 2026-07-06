using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using System.Text;

namespace QuilvianSystemBackend.Seeders
{
    /// <summary>
    /// Seeder master ICD resmi yang dipakai RS.
    ///
    /// Supported input:
    /// 1. Generated seed CSV:
    ///    - MstDiagnosisChapter.csv
    ///    - MstDiagnosis.csv
    ///
    /// 2. Raw SATUSEHAT/e-klaim public CSV:
    ///    - [PUBLIC] ICD-10 e-klaim.xlsx - ICD10.csv
    ///    - [PUBLIC] ICD-9CM e-klaim.xlsx - ICD9 CM.csv
    ///
    /// ICD-PM dan ICD-MM tidak di-import ke MstDiagnosis karena keduanya adalah mapping/klasifikasi
    /// maternal-perinatal mortality, bukan master diagnosis utama untuk SOAP dokter.
    /// </summary>
    public class Icd10DiagnosisSeeder
    {
        private const string Icd10Version = "ICD-10";
        private const string Icd9Version = "ICD-9";
        private static readonly Guid SystemUserId = Guid.Empty;

        public static async Task SeedAsync(IServiceProvider serviceProvider, string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
                throw new ArgumentException("Folder path ICD wajib diisi.", nameof(folderPath));

            if (!Directory.Exists(folderPath))
                throw new DirectoryNotFoundException($"Folder ICD tidak ditemukan: {folderPath}");

            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var now = DateTime.UtcNow;

            var generatedChapterPath = Path.Combine(folderPath, "MstDiagnosisChapter.csv");
            var generatedDiagnosisPath = Path.Combine(folderPath, "MstDiagnosis.csv");

            if (File.Exists(generatedChapterPath) && File.Exists(generatedDiagnosisPath))
            {
                await ImportGeneratedSeedCsvAsync(dbContext, generatedChapterPath, generatedDiagnosisPath, now);
                return;
            }

            var icd10Path = FindFirstCsv(folderPath, "ICD10", "ICD-10 e-klaim");
            var icd9Path = FindFirstCsv(folderPath, "ICD9", "ICD-9CM", "ICD9 CM");

            if (icd10Path == null && icd9Path == null)
            {
                throw new FileNotFoundException(
                    "File ICD tidak ditemukan. Letakkan MstDiagnosisChapter.csv + MstDiagnosis.csv, atau raw ICD10/ICD9 e-klaim CSV di folder seed."
                );
            }

            await ImportRawSatuSehatCsvAsync(dbContext, icd10Path, icd9Path, now);
        }

        private static async Task ImportGeneratedSeedCsvAsync(
            ApplicationDbContext dbContext,
            string chapterPath,
            string diagnosisPath,
            DateTime now)
        {
            var chapterRows = ReadCsvAsDictionary(chapterPath);
            var diagnosisRows = ReadCsvAsDictionary(diagnosisPath);

            var csvChapterIdToActualId = new Dictionary<Guid, Guid>();
            var chapterByKey = await dbContext.Set<MstDiagnosisChapter>()
                .Where(x => !x.IsDelete)
                .ToDictionaryAsync(x => BuildKey(x.IcdVersion, x.ChapterCode));

            foreach (var row in chapterRows)
            {
                var csvId = GetGuid(row, "Id") ?? Guid.NewGuid();
                var icdVersion = NormalizeIcdVersion(GetString(row, "IcdVersion"));
                var chapterCode = NormalizeCode(GetString(row, "ChapterCode"));
                var chapterName = NormalizeRequiredText(GetString(row, "ChapterName"));

                if (string.IsNullOrWhiteSpace(icdVersion) || string.IsNullOrWhiteSpace(chapterCode) || string.IsNullOrWhiteSpace(chapterName))
                    continue;

                var key = BuildKey(icdVersion, chapterCode);

                if (!chapterByKey.TryGetValue(key, out var chapter))
                {
                    chapter = new MstDiagnosisChapter
                    {
                        Id = csvId,
                        ChapterCode = chapterCode,
                        IcdVersion = icdVersion,
                        CreateDateTime = now,
                        CreateBy = SystemUserId,
                        IsCancel = false,
                        IsDelete = false
                    };

                    dbContext.Set<MstDiagnosisChapter>().Add(chapter);
                    chapterByKey[key] = chapter;
                }

                chapter.ChapterName = chapterName;
                chapter.DiagnosisCodeRangeStart = NormalizeNullableCode(GetString(row, "DiagnosisCodeRangeStart"));
                chapter.DiagnosisCodeRangeEnd = NormalizeNullableCode(GetString(row, "DiagnosisCodeRangeEnd"));
                chapter.IsActive = GetBool(row, "IsActive") ?? true;

                if (chapter.CreateDateTime == default)
                {
                    chapter.CreateDateTime = now;
                    chapter.CreateBy = SystemUserId;
                }
                else
                {
                    chapter.UpdateDateTime = now;
                    chapter.UpdateBy = SystemUserId;
                }

                csvChapterIdToActualId[csvId] = chapter.Id;
            }

            await dbContext.SaveChangesAsync();

            var diagnosisByKey = await dbContext.Set<MstDiagnosis>()
                .Where(x => !x.IsDelete)
                .ToDictionaryAsync(x => BuildKey(x.IcdVersion, x.DiagnosisCode));

            var csvDiagnosisIdToActualId = new Dictionary<Guid, Guid>();
            var pendingParentRows = new List<(Guid ActualDiagnosisId, Guid? CsvParentId, string IcdVersion, string DiagnosisCode)>();

            foreach (var row in diagnosisRows)
            {
                var csvId = GetGuid(row, "Id") ?? Guid.NewGuid();
                var csvChapterId = GetGuid(row, "DiagnosisChapterId");
                var csvParentId = GetGuid(row, "ParentDiagnosisId");
                var icdVersion = NormalizeIcdVersion(GetString(row, "IcdVersion"));
                var diagnosisCode = NormalizeCode(GetString(row, "DiagnosisCode"));
                var diagnosisName = NormalizeRequiredText(GetString(row, "DiagnosisName"));
                var diagnosisType = NormalizeDiagnosisType(GetString(row, "DiagnosisType"), icdVersion);

                if (string.IsNullOrWhiteSpace(icdVersion) || string.IsNullOrWhiteSpace(diagnosisCode) || string.IsNullOrWhiteSpace(diagnosisName))
                    continue;

                Guid? actualChapterId = null;
                if (csvChapterId.HasValue && csvChapterIdToActualId.TryGetValue(csvChapterId.Value, out var mappedChapterId))
                    actualChapterId = mappedChapterId;

                var key = BuildKey(icdVersion, diagnosisCode);

                if (!diagnosisByKey.TryGetValue(key, out var diagnosis))
                {
                    diagnosis = new MstDiagnosis
                    {
                        Id = csvId,
                        DiagnosisCode = diagnosisCode,
                        IcdVersion = icdVersion,
                        CreateDateTime = now,
                        CreateBy = SystemUserId,
                        IsCancel = false,
                        IsDelete = false
                    };

                    dbContext.Set<MstDiagnosis>().Add(diagnosis);
                    diagnosisByKey[key] = diagnosis;
                }

                diagnosis.DiagnosisChapterId = actualChapterId;
                diagnosis.ParentDiagnosisId = null;
                diagnosis.DiagnosisCode = diagnosisCode;
                diagnosis.DiagnosisName = diagnosisName;
                diagnosis.DiagnosisType = diagnosisType;
                diagnosis.IcdVersion = icdVersion;
                diagnosis.IsSelectableForClinicalUse = GetBool(row, "IsSelectableForClinicalUse") ?? true;
                diagnosis.IsPrimaryDiagnosisAllowed = GetBool(row, "IsPrimaryDiagnosisAllowed") ?? true;
                diagnosis.IsSecondaryDiagnosisAllowed = GetBool(row, "IsSecondaryDiagnosisAllowed") ?? true;
                diagnosis.IsActive = GetBool(row, "IsActive") ?? true;

                if (diagnosis.CreateDateTime == default)
                {
                    diagnosis.CreateDateTime = now;
                    diagnosis.CreateBy = SystemUserId;
                }
                else
                {
                    diagnosis.UpdateDateTime = now;
                    diagnosis.UpdateBy = SystemUserId;
                }

                csvDiagnosisIdToActualId[csvId] = diagnosis.Id;
                pendingParentRows.Add((diagnosis.Id, csvParentId, icdVersion, diagnosisCode));
            }

            await dbContext.SaveChangesAsync();

            var allDiagnosisById = await dbContext.Set<MstDiagnosis>()
                .Where(x => !x.IsDelete)
                .ToDictionaryAsync(x => x.Id);

            diagnosisByKey = await dbContext.Set<MstDiagnosis>()
                .Where(x => !x.IsDelete)
                .ToDictionaryAsync(x => BuildKey(x.IcdVersion, x.DiagnosisCode));

            foreach (var pending in pendingParentRows)
            {
                if (!allDiagnosisById.TryGetValue(pending.ActualDiagnosisId, out var diagnosis))
                    continue;

                Guid? parentId = null;

                if (pending.CsvParentId.HasValue && csvDiagnosisIdToActualId.TryGetValue(pending.CsvParentId.Value, out var mappedParentId))
                {
                    parentId = mappedParentId;
                }
                else
                {
                    var parentCode = GetParentDiagnosisCode(pending.DiagnosisCode);
                    if (!string.IsNullOrWhiteSpace(parentCode) && diagnosisByKey.TryGetValue(BuildKey(pending.IcdVersion, parentCode), out var parent))
                        parentId = parent.Id;
                }

                if (parentId.HasValue && parentId.Value != diagnosis.Id)
                {
                    diagnosis.ParentDiagnosisId = parentId;
                    diagnosis.UpdateDateTime = now;
                    diagnosis.UpdateBy = SystemUserId;
                }
            }

            await dbContext.SaveChangesAsync();
        }

        private static async Task ImportRawSatuSehatCsvAsync(
            ApplicationDbContext dbContext,
            string? icd10Path,
            string? icd9Path,
            DateTime now)
        {
            var rawRows = new List<RawDiagnosisRow>();

            if (!string.IsNullOrWhiteSpace(icd10Path))
            {
                rawRows.AddRange(ReadRawDiagnosisRows(icd10Path, Icd10Version, "ICD10"));
            }

            if (!string.IsNullOrWhiteSpace(icd9Path))
            {
                rawRows.AddRange(ReadRawDiagnosisRows(icd9Path, Icd9Version, "ICD9"));
            }

            if (rawRows.Count == 0)
                return;

            var parentCodeSet = rawRows
                .Select(x => GetParentDiagnosisCode(x.DiagnosisCode))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var chapterRows = BuildChapterRows(rawRows);

            var chapterByKey = await dbContext.Set<MstDiagnosisChapter>()
                .Where(x => !x.IsDelete)
                .ToDictionaryAsync(x => BuildKey(x.IcdVersion, x.ChapterCode));

            foreach (var row in chapterRows)
            {
                var key = BuildKey(row.IcdVersion, row.ChapterCode);

                if (!chapterByKey.TryGetValue(key, out var chapter))
                {
                    chapter = new MstDiagnosisChapter
                    {
                        Id = Guid.NewGuid(),
                        ChapterCode = row.ChapterCode,
                        IcdVersion = row.IcdVersion,
                        CreateDateTime = now,
                        CreateBy = SystemUserId,
                        IsCancel = false,
                        IsDelete = false
                    };

                    dbContext.Set<MstDiagnosisChapter>().Add(chapter);
                    chapterByKey[key] = chapter;
                }

                chapter.ChapterName = row.ChapterName;
                chapter.DiagnosisCodeRangeStart = row.RangeStart;
                chapter.DiagnosisCodeRangeEnd = row.RangeEnd;
                chapter.IsActive = true;
                chapter.UpdateDateTime = now;
                chapter.UpdateBy = SystemUserId;
            }

            await dbContext.SaveChangesAsync();

            var diagnosisByKey = await dbContext.Set<MstDiagnosis>()
                .Where(x => !x.IsDelete)
                .ToDictionaryAsync(x => BuildKey(x.IcdVersion, x.DiagnosisCode));

            foreach (var row in rawRows)
            {
                var chapter = ResolveChapter(row, chapterByKey);
                var key = BuildKey(row.IcdVersion, row.DiagnosisCode);

                if (!diagnosisByKey.TryGetValue(key, out var diagnosis))
                {
                    diagnosis = new MstDiagnosis
                    {
                        Id = Guid.NewGuid(),
                        DiagnosisCode = row.DiagnosisCode,
                        IcdVersion = row.IcdVersion,
                        CreateDateTime = now,
                        CreateBy = SystemUserId,
                        IsCancel = false,
                        IsDelete = false
                    };

                    dbContext.Set<MstDiagnosis>().Add(diagnosis);
                    diagnosisByKey[key] = diagnosis;
                }

                diagnosis.DiagnosisChapterId = chapter?.Id;
                diagnosis.ParentDiagnosisId = null;
                diagnosis.DiagnosisCode = row.DiagnosisCode;
                diagnosis.DiagnosisName = row.DiagnosisName;
                diagnosis.DiagnosisType = row.DiagnosisType;
                diagnosis.IcdVersion = row.IcdVersion;
                diagnosis.IsSelectableForClinicalUse = !parentCodeSet.Contains(row.DiagnosisCode);
                diagnosis.IsPrimaryDiagnosisAllowed = true;
                diagnosis.IsSecondaryDiagnosisAllowed = true;
                diagnosis.IsActive = true;
                diagnosis.UpdateDateTime = now;
                diagnosis.UpdateBy = SystemUserId;
            }

            await dbContext.SaveChangesAsync();

            var diagnoses = await dbContext.Set<MstDiagnosis>()
                .Where(x => !x.IsDelete && (x.IcdVersion == Icd10Version || x.IcdVersion == Icd9Version))
                .ToListAsync();

            diagnosisByKey = diagnoses.ToDictionary(x => BuildKey(x.IcdVersion, x.DiagnosisCode));

            foreach (var diagnosis in diagnoses)
            {
                var parentCode = GetParentDiagnosisCode(diagnosis.DiagnosisCode);
                if (string.IsNullOrWhiteSpace(parentCode))
                    continue;

                if (!diagnosisByKey.TryGetValue(BuildKey(diagnosis.IcdVersion, parentCode), out var parent))
                    continue;

                if (parent.Id == diagnosis.Id)
                    continue;

                diagnosis.ParentDiagnosisId = parent.Id;
                diagnosis.UpdateDateTime = now;
                diagnosis.UpdateBy = SystemUserId;
            }

            await dbContext.SaveChangesAsync();
        }

        private static List<RawDiagnosisRow> ReadRawDiagnosisRows(string path, string icdVersion, string diagnosisType)
        {
            var rows = new List<RawDiagnosisRow>();
            var dictionaries = ReadCsvAsDictionary(path);
            var seenCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var row in dictionaries)
            {
                var code = NormalizeCode(GetString(row, "CODE"));
                var name = NormalizeRequiredText(GetString(row, "DISPLAY"));

                if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
                    continue;

                if (!seenCodes.Add(code))
                    continue;

                rows.Add(new RawDiagnosisRow
                {
                    DiagnosisCode = code,
                    DiagnosisName = name,
                    IcdVersion = icdVersion,
                    DiagnosisType = diagnosisType
                });
            }

            return rows;
        }

        private static List<ChapterRow> BuildChapterRows(List<RawDiagnosisRow> rawRows)
        {
            var rows = new List<ChapterRow>();

            rows.AddRange(BuildIcd10Chapters(rawRows.Where(x => x.IcdVersion == Icd10Version).ToList()));
            rows.AddRange(BuildIcd9Chapters(rawRows.Where(x => x.IcdVersion == Icd9Version).ToList()));

            return rows;
        }

        private static List<ChapterRow> BuildIcd10Chapters(List<RawDiagnosisRow> icd10Rows)
        {
            if (icd10Rows.Count == 0)
                return new List<ChapterRow>();

            return new List<ChapterRow>
            {
                new ChapterRow("I", "Certain infectious and parasitic diseases", "A00", "B99", Icd10Version),
                new ChapterRow("II", "Neoplasms", "C00", "D48", Icd10Version),
                new ChapterRow("III", "Diseases of the blood and blood-forming organs and certain disorders involving the immune mechanism", "D50", "D89", Icd10Version),
                new ChapterRow("IV", "Endocrine, nutritional and metabolic diseases", "E00", "E90", Icd10Version),
                new ChapterRow("V", "Mental and behavioural disorders", "F00", "F99", Icd10Version),
                new ChapterRow("VI", "Diseases of the nervous system", "G00", "G99", Icd10Version),
                new ChapterRow("VII", "Diseases of the eye and adnexa", "H00", "H59", Icd10Version),
                new ChapterRow("VIII", "Diseases of the ear and mastoid process", "H60", "H95", Icd10Version),
                new ChapterRow("IX", "Diseases of the circulatory system", "I00", "I99", Icd10Version),
                new ChapterRow("X", "Diseases of the respiratory system", "J00", "J99", Icd10Version),
                new ChapterRow("XI", "Diseases of the digestive system", "K00", "K93", Icd10Version),
                new ChapterRow("XII", "Diseases of the skin and subcutaneous tissue", "L00", "L99", Icd10Version),
                new ChapterRow("XIII", "Diseases of the musculoskeletal system and connective tissue", "M00", "M99", Icd10Version),
                new ChapterRow("XIV", "Diseases of the genitourinary system", "N00", "N99", Icd10Version),
                new ChapterRow("XV", "Pregnancy, childbirth and the puerperium", "O00", "O99", Icd10Version),
                new ChapterRow("XVI", "Certain conditions originating in the perinatal period", "P00", "P96", Icd10Version),
                new ChapterRow("XVII", "Congenital malformations, deformations and chromosomal abnormalities", "Q00", "Q99", Icd10Version),
                new ChapterRow("XVIII", "Symptoms, signs and abnormal clinical and laboratory findings, not elsewhere classified", "R00", "R99", Icd10Version),
                new ChapterRow("XIX", "Injury, poisoning and certain other consequences of external causes", "S00", "T98", Icd10Version),
                new ChapterRow("XX", "External causes of morbidity and mortality", "V01", "Y98", Icd10Version),
                new ChapterRow("XXI", "Factors influencing health status and contact with health services", "Z00", "Z99", Icd10Version),
                new ChapterRow("XXII", "Codes for special purposes", "U00", "U99", Icd10Version)
            };
        }

        private static List<ChapterRow> BuildIcd9Chapters(List<RawDiagnosisRow> icd9Rows)
        {
            var rows = new List<ChapterRow>();
            var rootRows = icd9Rows
                .Where(x => !x.DiagnosisCode.Contains('.') && x.DiagnosisCode.Length <= 2)
                .OrderBy(x => x.DiagnosisCode)
                .ToList();

            foreach (var root in rootRows)
            {
                rows.Add(new ChapterRow(
                    root.DiagnosisCode,
                    root.DiagnosisName,
                    root.DiagnosisCode,
                    root.DiagnosisCode,
                    Icd9Version
                ));
            }

            return rows;
        }

        private static MstDiagnosisChapter? ResolveChapter(
            RawDiagnosisRow row,
            Dictionary<string, MstDiagnosisChapter> chapterByKey)
        {
            if (row.IcdVersion == Icd9Version)
            {
                var root = GetRootDiagnosisCode(row.DiagnosisCode);
                return chapterByKey.TryGetValue(BuildKey(Icd9Version, root), out var chapter)
                    ? chapter
                    : null;
            }

            var chapterRow = BuildIcd10Chapters(new List<RawDiagnosisRow> { row })
                .FirstOrDefault(x => IsCodeInsideIcd10Range(GetRootDiagnosisCode(row.DiagnosisCode), x.RangeStart, x.RangeEnd));

            if (chapterRow == null)
                return null;

            return chapterByKey.TryGetValue(BuildKey(Icd10Version, chapterRow.ChapterCode), out var found)
                ? found
                : null;
        }

        private static bool IsCodeInsideIcd10Range(string rootCode, string? startCode, string? endCode)
        {
            if (string.IsNullOrWhiteSpace(rootCode) || string.IsNullOrWhiteSpace(startCode) || string.IsNullOrWhiteSpace(endCode))
                return false;

            var start = SplitAlphaNumericCode(startCode);
            var end = SplitAlphaNumericCode(endCode);
            var value = SplitAlphaNumericCode(rootCode);

            if (start.Letter == null || end.Letter == null || value.Letter == null)
                return false;

            var afterStart = CompareAlphaNumeric(value, start) >= 0;
            var beforeEnd = CompareAlphaNumeric(value, end) <= 0;

            return afterStart && beforeEnd;
        }

        private static (char? Letter, int Number) SplitAlphaNumericCode(string code)
        {
            var normalized = NormalizeCode(code);
            if (normalized.Length < 2 || !char.IsLetter(normalized[0]))
                return (null, 0);

            var digitText = new string(normalized.Skip(1).TakeWhile(char.IsDigit).ToArray());
            return int.TryParse(digitText, out var number)
                ? (normalized[0], number)
                : (normalized[0], 0);
        }

        private static int CompareAlphaNumeric((char? Letter, int Number) left, (char? Letter, int Number) right)
        {
            var letterCompare = string.Compare(left.Letter?.ToString(), right.Letter?.ToString(), StringComparison.OrdinalIgnoreCase);
            if (letterCompare != 0)
                return letterCompare;

            return left.Number.CompareTo(right.Number);
        }

        private static string? FindFirstCsv(string folderPath, params string[] keywordOptions)
        {
            return Directory.GetFiles(folderPath, "*.csv", SearchOption.TopDirectoryOnly)
                .FirstOrDefault(path => keywordOptions.Any(keyword =>
                    Path.GetFileName(path).Contains(keyword, StringComparison.OrdinalIgnoreCase)));
        }

        private static List<Dictionary<string, string>> ReadCsvAsDictionary(string path)
        {
            var result = new List<Dictionary<string, string>>();
            using var reader = new StreamReader(path, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

            var headerLine = reader.ReadLine();
            if (headerLine == null)
                return result;

            var headers = ParseCsvLine(headerLine)
                .Select(x => x.Trim().TrimStart('\uFEFF'))
                .ToList();

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var values = ParseCsvLine(line);
                var dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                for (var i = 0; i < headers.Count; i++)
                {
                    dictionary[headers[i]] = i < values.Count ? values[i] : string.Empty;
                }

                result.Add(dictionary);
            }

            return result;
        }

        private static List<string> ParseCsvLine(string line)
        {
            var values = new List<string>();
            var builder = new StringBuilder();
            var inQuotes = false;

            for (var i = 0; i < line.Length; i++)
            {
                var current = line[i];

                if (current == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        builder.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }

                    continue;
                }

                if (current == ',' && !inQuotes)
                {
                    values.Add(builder.ToString());
                    builder.Clear();
                    continue;
                }

                builder.Append(current);
            }

            values.Add(builder.ToString());
            return values;
        }

        private static string BuildKey(string? version, string? code)
        {
            return $"{NormalizeIcdVersion(version)}|{NormalizeCode(code)}";
        }

        private static string NormalizeIcdVersion(string? value)
        {
            var normalized = NormalizeNullableText(value)?.ToUpperInvariant().Replace("_", "-").Replace(" ", string.Empty);

            return normalized switch
            {
                "ICD9" or "ICD-9" or "ICD9CM" or "ICD-9CM" or "ICD9CM-2010" or "ICD-9CM-2010" => Icd9Version,
                "ICD10" or "ICD-10" or "ICD102010" or "ICD10-2010" or "ICD-10-2010" => Icd10Version,
                _ => string.IsNullOrWhiteSpace(value) ? Icd10Version : value.Trim()
            };
        }

        private static string NormalizeDiagnosisType(string? value, string icdVersion)
        {
            var normalized = NormalizeNullableText(value)?.ToUpperInvariant().Replace("-", string.Empty).Replace("_", string.Empty).Replace(" ", string.Empty);

            return normalized switch
            {
                "ICD9" or "ICD9CM" => "ICD9",
                "ICD10" => "ICD10",
                "LOCAL" => "Local",
                "CUSTOM" => "Custom",
                _ => icdVersion == Icd9Version ? "ICD9" : "ICD10"
            };
        }

        private static string NormalizeCode(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Trim().Trim('\uFEFF').ToUpperInvariant();
        }

        private static string? NormalizeNullableCode(string? value)
        {
            var code = NormalizeCode(value);
            return string.IsNullOrWhiteSpace(code) ? null : code;
        }

        private static string NormalizeRequiredText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Trim().Trim('\uFEFF');
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim().Trim('\uFEFF');
        }

        private static Guid? GetGuid(Dictionary<string, string> row, string key)
        {
            return row.TryGetValue(key, out var value) && Guid.TryParse(value, out var id) && id != Guid.Empty
                ? id
                : null;
        }

        private static bool? GetBool(Dictionary<string, string> row, string key)
        {
            if (!row.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
                return null;

            if (bool.TryParse(value, out var boolValue))
                return boolValue;

            return value.Trim() switch
            {
                "1" => true,
                "0" => false,
                _ => null
            };
        }

        private static string? GetString(Dictionary<string, string> row, string key)
        {
            return row.TryGetValue(key, out var value) ? value : null;
        }

        private static string GetRootDiagnosisCode(string diagnosisCode)
        {
            var code = NormalizeCode(diagnosisCode);

            if (code.Contains('.'))
                code = code.Split('.')[0];

            if (code.Length >= 3 && char.IsLetter(code[0]))
                return code.Substring(0, 3);

            if (code.Length >= 2 && char.IsDigit(code[0]))
                return code.Substring(0, 2);

            return code;
        }

        private static string? GetParentDiagnosisCode(string diagnosisCode)
        {
            var code = NormalizeCode(diagnosisCode);

            if (!code.Contains('.'))
                return null;

            var index = code.LastIndexOf('.');
            if (index <= 0)
                return null;

            return code.Substring(0, index);
        }

        private class RawDiagnosisRow
        {
            public string DiagnosisCode { get; set; } = string.Empty;
            public string DiagnosisName { get; set; } = string.Empty;
            public string DiagnosisType { get; set; } = string.Empty;
            public string IcdVersion { get; set; } = string.Empty;
        }

        private class ChapterRow
        {
            public ChapterRow(string chapterCode, string chapterName, string? rangeStart, string? rangeEnd, string icdVersion)
            {
                ChapterCode = chapterCode;
                ChapterName = chapterName;
                RangeStart = rangeStart;
                RangeEnd = rangeEnd;
                IcdVersion = icdVersion;
            }

            public string ChapterCode { get; }
            public string ChapterName { get; }
            public string? RangeStart { get; }
            public string? RangeEnd { get; }
            public string IcdVersion { get; }
        }
    }
}
