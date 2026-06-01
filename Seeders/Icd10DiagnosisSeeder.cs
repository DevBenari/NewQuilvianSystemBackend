using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using System.Text;

namespace QuilvianSystemBackend.Seeders
{
    public class Icd10DiagnosisSeeder
    {
        private const string IcdVersion = "ICD-10 2019";

        public static async Task SeedAsync(IServiceProvider serviceProvider, string folderPath)
        {
            using var scope = serviceProvider.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var chaptersPath = Path.Combine(folderPath, "icd102019syst_chapters.txt");
            var groupsPath = Path.Combine(folderPath, "icd102019syst_groups.txt");
            var codesPath = Path.Combine(folderPath, "icd102019syst_codes.txt");

            if (!File.Exists(chaptersPath))
                throw new FileNotFoundException("File ICD-10 chapters tidak ditemukan.", chaptersPath);

            if (!File.Exists(groupsPath))
                throw new FileNotFoundException("File ICD-10 groups tidak ditemukan.", groupsPath);

            if (!File.Exists(codesPath))
                throw new FileNotFoundException("File ICD-10 codes tidak ditemukan.", codesPath);

            var existingDiagnosisCount = await dbContext.Set<MstDiagnosis>()
                .CountAsync(x => x.IcdVersion == IcdVersion && !x.IsDelete);

            if (existingDiagnosisCount > 0)
            {
                return;
            }

            var now = DateTime.UtcNow;

            var chapterRows = ReadChapters(chaptersPath);
            var groupRows = ReadGroups(groupsPath);
            var codeRows = ReadCodes(codesPath);

            var chapterByCode = await ImportChaptersAsync(
                dbContext,
                chapterRows,
                groupRows,
                now
            );

            await ImportDiagnosesAsync(
                dbContext,
                codeRows,
                groupRows,
                chapterByCode,
                now
            );
        }

        private static List<ChapterImportRow> ReadChapters(string path)
        {
            var rows = new List<ChapterImportRow>();

            foreach (var rawLine in File.ReadLines(path, Encoding.UTF8))
            {
                var line = CleanLine(rawLine);

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Split(';');

                if (parts.Length < 2)
                    continue;

                var chapterCode = NormalizeChapterCode(parts[0]);
                var chapterName = parts[1].Trim();

                if (string.IsNullOrWhiteSpace(chapterCode) || string.IsNullOrWhiteSpace(chapterName))
                    continue;

                rows.Add(new ChapterImportRow
                {
                    ChapterCode = chapterCode,
                    ChapterName = chapterName
                });
            }

            return rows;
        }

        private static List<GroupImportRow> ReadGroups(string path)
        {
            var rows = new List<GroupImportRow>();

            foreach (var rawLine in File.ReadLines(path, Encoding.UTF8))
            {
                var line = CleanLine(rawLine);

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Split(';');

                if (parts.Length < 4)
                    continue;

                var startCode = parts[0].Trim().ToUpperInvariant();
                var endCode = parts[1].Trim().ToUpperInvariant();
                var chapterCode = NormalizeChapterCode(parts[2]);
                var groupName = parts[3].Trim();

                if (string.IsNullOrWhiteSpace(startCode) ||
                    string.IsNullOrWhiteSpace(endCode) ||
                    string.IsNullOrWhiteSpace(chapterCode) ||
                    string.IsNullOrWhiteSpace(groupName))
                {
                    continue;
                }

                rows.Add(new GroupImportRow
                {
                    StartCode = startCode,
                    EndCode = endCode,
                    ChapterCode = chapterCode,
                    GroupName = groupName
                });
            }

            return rows;
        }

        private static List<DiagnosisImportRow> ReadCodes(string path)
        {
            var rows = new List<DiagnosisImportRow>();

            foreach (var rawLine in File.ReadLines(path, Encoding.UTF8))
            {
                var line = CleanLine(rawLine);

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Split(';');

                if (parts.Length < 9)
                    continue;

                var levelText = parts[0].Trim();
                var terminalFlag = parts[1].Trim().ToUpperInvariant();
                var chapterCode = NormalizeChapterCode(parts[3]);

                var rawCode = parts[5].Trim();
                var compactCode = parts[7].Trim();
                var diagnosisName = parts[8].Trim();

                var diagnosisCode = NormalizeDiagnosisCode(rawCode, compactCode);

                if (string.IsNullOrWhiteSpace(chapterCode) ||
                    string.IsNullOrWhiteSpace(diagnosisCode) ||
                    string.IsNullOrWhiteSpace(diagnosisName))
                {
                    continue;
                }

                var isTerminal = terminalFlag == "T";
                var isSelectable = isTerminal && !rawCode.EndsWith(".-", StringComparison.OrdinalIgnoreCase);

                rows.Add(new DiagnosisImportRow
                {
                    LevelText = levelText,
                    TerminalFlag = terminalFlag,
                    ChapterCode = chapterCode,
                    RawCode = rawCode,
                    CompactCode = compactCode,
                    DiagnosisCode = diagnosisCode,
                    DiagnosisName = diagnosisName,
                    IsTerminal = isTerminal,
                    IsSelectableForClinicalUse = isSelectable
                });
            }

            return rows;
        }

        private static async Task<Dictionary<string, MstDiagnosisChapter>> ImportChaptersAsync(
            ApplicationDbContext dbContext,
            List<ChapterImportRow> chapterRows,
            List<GroupImportRow> groupRows,
            DateTime now)
        {
            var existingChapters = await dbContext.Set<MstDiagnosisChapter>()
                .Where(x => x.IcdVersion == IcdVersion && !x.IsDelete)
                .ToListAsync();

            var chapterByCode = existingChapters
                .ToDictionary(x => x.ChapterCode, StringComparer.OrdinalIgnoreCase);

            foreach (var row in chapterRows)
            {
                if (!chapterByCode.TryGetValue(row.ChapterCode, out var chapter))
                {
                    chapter = new MstDiagnosisChapter
                    {
                        Id = Guid.NewGuid(),
                        ChapterCode = row.ChapterCode,
                        ChapterName = row.ChapterName,
                        IcdVersion = IcdVersion,
                        SortOrder = SafeParseInt(row.ChapterCode),
                        IsActive = true,
                        CreateDateTime = now,
                        CreateBy = Guid.Empty,
                        IsDelete = false,
                        IsCancel = false
                    };

                    dbContext.Set<MstDiagnosisChapter>().Add(chapter);
                    chapterByCode[row.ChapterCode] = chapter;
                }
                else
                {
                    chapter.ChapterName = row.ChapterName;
                    chapter.IcdVersion = IcdVersion;
                    chapter.UpdateDateTime = now;
                    chapter.UpdateBy = Guid.Empty;
                }
            }

            foreach (var group in groupRows)
            {
                if (!chapterByCode.TryGetValue(group.ChapterCode, out var chapter))
                    continue;

                if (string.IsNullOrWhiteSpace(chapter.DiagnosisCodeRangeStart))
                    chapter.DiagnosisCodeRangeStart = group.StartCode;

                chapter.DiagnosisCodeRangeEnd = group.EndCode;
            }

            await dbContext.SaveChangesAsync();

            return chapterByCode;
        }

        private static async Task ImportDiagnosesAsync(
            ApplicationDbContext dbContext,
            List<DiagnosisImportRow> codeRows,
            List<GroupImportRow> groupRows,
            Dictionary<string, MstDiagnosisChapter> chapterByCode,
            DateTime now)
        {
            var existingDiagnoses = await dbContext.Set<MstDiagnosis>()
                .Where(x => x.IcdVersion == IcdVersion && !x.IsDelete)
                .ToListAsync();

            var diagnosisByCode = existingDiagnoses
                .ToDictionary(x => x.DiagnosisCode, StringComparer.OrdinalIgnoreCase);

            foreach (var row in codeRows)
            {
                if (!chapterByCode.TryGetValue(row.ChapterCode, out var chapter))
                    continue;

                var groupName = FindGroupName(row.DiagnosisCode, row.ChapterCode, groupRows);

                if (!diagnosisByCode.TryGetValue(row.DiagnosisCode, out var diagnosis))
                {
                    diagnosis = new MstDiagnosis
                    {
                        Id = Guid.NewGuid(),
                        DiagnosisChapterId = chapter.Id,
                        DiagnosisCode = row.DiagnosisCode,
                        DiagnosisName = row.DiagnosisName,
                        DiagnosisNameEnglish = row.DiagnosisName,
                        ShortName = row.DiagnosisName.Length > 200
                            ? row.DiagnosisName.Substring(0, 200)
                            : row.DiagnosisName,
                        DiagnosisGroupName = groupName,
                        DiagnosisCategoryName = groupName,
                        DiagnosisType = "ICD10",
                        IcdVersion = IcdVersion,
                        IsSelectableForClinicalUse = row.IsSelectableForClinicalUse,
                        IsBillable = row.IsTerminal,
                        IsPrimaryDiagnosisAllowed = row.IsSelectableForClinicalUse,
                        IsSecondaryDiagnosisAllowed = row.IsSelectableForClinicalUse,
                        IsChronicDisease = false,
                        IsInfectiousDisease = row.ChapterCode == "01",
                        IsExternalCause = row.ChapterCode == "19" || row.ChapterCode == "20",
                        IsPregnancyRelated = row.ChapterCode == "15",
                        IsMentalHealthRelated = row.ChapterCode == "05",
                        IsRareDisease = false,
                        SortOrder = SafeParseInt(row.LevelText),
                        IsActive = true,
                        CreateDateTime = now,
                        CreateBy = Guid.Empty,
                        IsDelete = false,
                        IsCancel = false
                    };

                    dbContext.Set<MstDiagnosis>().Add(diagnosis);
                    diagnosisByCode[row.DiagnosisCode] = diagnosis;
                }
                else
                {
                    diagnosis.DiagnosisChapterId = chapter.Id;
                    diagnosis.DiagnosisName = row.DiagnosisName;
                    diagnosis.DiagnosisNameEnglish = row.DiagnosisName;
                    diagnosis.ShortName = row.DiagnosisName.Length > 200
                        ? row.DiagnosisName.Substring(0, 200)
                        : row.DiagnosisName;
                    diagnosis.DiagnosisGroupName = groupName;
                    diagnosis.DiagnosisCategoryName = groupName;
                    diagnosis.DiagnosisType = "ICD10";
                    diagnosis.IcdVersion = IcdVersion;
                    diagnosis.IsSelectableForClinicalUse = row.IsSelectableForClinicalUse;
                    diagnosis.IsBillable = row.IsTerminal;
                    diagnosis.IsPrimaryDiagnosisAllowed = row.IsSelectableForClinicalUse;
                    diagnosis.IsSecondaryDiagnosisAllowed = row.IsSelectableForClinicalUse;
                    diagnosis.IsActive = true;
                    diagnosis.UpdateDateTime = now;
                    diagnosis.UpdateBy = Guid.Empty;
                }
            }

            await dbContext.SaveChangesAsync();

            var allDiagnoses = await dbContext.Set<MstDiagnosis>()
                .Where(x => x.IcdVersion == IcdVersion && !x.IsDelete)
                .ToListAsync();

            diagnosisByCode = allDiagnoses
                .ToDictionary(x => x.DiagnosisCode, StringComparer.OrdinalIgnoreCase);

            foreach (var diagnosis in allDiagnoses)
            {
                var parentCode = GetParentDiagnosisCode(diagnosis.DiagnosisCode);

                if (string.IsNullOrWhiteSpace(parentCode))
                    continue;

                if (!diagnosisByCode.TryGetValue(parentCode, out var parentDiagnosis))
                    continue;

                if (diagnosis.Id == parentDiagnosis.Id)
                    continue;

                diagnosis.ParentDiagnosisId = parentDiagnosis.Id;
                diagnosis.UpdateDateTime = now;
                diagnosis.UpdateBy = Guid.Empty;
            }

            await dbContext.SaveChangesAsync();
        }

        private static string? FindGroupName(
            string diagnosisCode,
            string chapterCode,
            List<GroupImportRow> groupRows)
        {
            var rootCode = GetRootDiagnosisCode(diagnosisCode);

            var group = groupRows.FirstOrDefault(x =>
                x.ChapterCode.Equals(chapterCode, StringComparison.OrdinalIgnoreCase) &&
                string.Compare(rootCode, x.StartCode, StringComparison.OrdinalIgnoreCase) >= 0 &&
                string.Compare(rootCode, x.EndCode, StringComparison.OrdinalIgnoreCase) <= 0);

            return group?.GroupName;
        }

        private static string GetRootDiagnosisCode(string diagnosisCode)
        {
            var code = diagnosisCode.Trim().ToUpperInvariant();

            if (code.Contains('.'))
                code = code.Split('.')[0];

            return code.Length >= 3
                ? code.Substring(0, 3)
                : code;
        }

        private static string? GetParentDiagnosisCode(string diagnosisCode)
        {
            var code = diagnosisCode.Trim().ToUpperInvariant();

            if (!code.Contains('.'))
                return null;

            var index = code.LastIndexOf('.');

            if (index <= 0)
                return null;

            return code.Substring(0, index);
        }

        private static string NormalizeDiagnosisCode(string rawCode, string compactCode)
        {
            var code = !string.IsNullOrWhiteSpace(rawCode)
                ? rawCode.Trim()
                : compactCode.Trim();

            code = code.ToUpperInvariant();

            if (code.EndsWith(".-", StringComparison.OrdinalIgnoreCase))
                code = code.Replace(".-", string.Empty);

            return code;
        }

        private static string NormalizeChapterCode(string value)
        {
            var code = value.Trim();

            if (int.TryParse(code, out var number))
                return number.ToString("00");

            return code;
        }

        private static string CleanLine(string rawLine)
        {
            return rawLine
                .Replace("\uFEFF", string.Empty)
                .Trim();
        }

        private static int SafeParseInt(string value)
        {
            return int.TryParse(value, out var result)
                ? result
                : 0;
        }

        private class ChapterImportRow
        {
            public string ChapterCode { get; set; } = string.Empty;
            public string ChapterName { get; set; } = string.Empty;
        }

        private class GroupImportRow
        {
            public string StartCode { get; set; } = string.Empty;
            public string EndCode { get; set; } = string.Empty;
            public string ChapterCode { get; set; } = string.Empty;
            public string GroupName { get; set; } = string.Empty;
        }

        private class DiagnosisImportRow
        {
            public string LevelText { get; set; } = string.Empty;
            public string TerminalFlag { get; set; } = string.Empty;
            public string ChapterCode { get; set; } = string.Empty;
            public string RawCode { get; set; } = string.Empty;
            public string CompactCode { get; set; } = string.Empty;
            public string DiagnosisCode { get; set; } = string.Empty;
            public string DiagnosisName { get; set; } = string.Empty;
            public bool IsTerminal { get; set; }
            public bool IsSelectableForClinicalUse { get; set; }
        }
    }
}
