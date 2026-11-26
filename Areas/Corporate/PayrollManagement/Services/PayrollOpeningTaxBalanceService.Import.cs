using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Models;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;

/// <summary>
/// Impor massal saldo awal PPh (PAY-BE-FIX-036). Berkas diuraikan klien menjadi baris teks mentah; SELURUH validasi bisnis ada di sini dan
/// memakai aturan yang sama dengan input manual (<c>ValidateFields</c>, <c>Apply</c>), sehingga kedua jalur menghasilkan jenis data dan
/// aturan yang sama. Validate bersifat baca-saja. Commit memvalidasi ulang di dalam transaksi serializable dengan advisory lock yang sama
/// dengan Create/Approve, menyimpan SEMUA baris sebagai DRAFT atau tidak sama sekali, dan tidak pernah menimpa saldo yang sudah ada,
/// mengajukan, menyetujui, mengubah payroll run, menghitung ulang PPh, atau menyentuh Finance.
/// </summary>
public sealed partial class PayrollOpeningTaxBalanceService
{
    /// <summary>Kolom template (urutan dan nama stabil). Dicocokkan klien tanpa membedakan huruf besar/kecil.</summary>
    internal static readonly string[] ImportColumns =
    {
        "WorkforceCode", "WorkforceType", "TaxYear", "EmployerSource", "CoveredMonths",
        "TaxableGross", "EmployerPremium", "PensionDeduction", "TaxWithheld"
    };

    private static readonly Regex MonthSeparator = new(@"[,;|\s]+", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex MonthToken = new(@"^\d{1,2}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex MonthRange = new(@"^(\d{1,2})-(\d{1,2})$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex AmountPlain = new(@"^\d+(\.\d{1,2})?$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex ThousandDots = new(@"\.(?=\d{3}(\D|$))", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private sealed record ImportIssue(string Code, string Message);

    private sealed class ImportRowState
    {
        public PayrollOpeningTaxBalanceImportRowRequest Row { get; init; } = new();
        public int RowNumber { get; init; }
        public List<ImportIssue> Issues { get; } = new();
        public Guid? ProfileId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string TypeLabel { get; set; } = string.Empty;
        public int? TaxYear { get; set; }
        public string EmployerSource { get; set; } = string.Empty;
        public List<int> Months { get; set; } = new();
        public decimal? TaxableGross { get; set; }
        public decimal? EmployerPremium { get; set; }
        public decimal? PensionDeduction { get; set; }
        public decimal? TaxWithheld { get; set; }
        public ValidInput? Input { get; set; }
        public Guid? CreatedId { get; set; }
        public bool SourceValid => EmployerSources.Contains(EmployerSource);
        public bool HasKey => ProfileId.HasValue && TaxYear.HasValue && SourceValid;
    }

    private sealed record ImportAnalysis(List<ImportRowState> States, PayrollOpeningTaxBalanceImportResult Result, List<PrlOpeningTaxBalance> Existing);

    // ------------------------------------------------------------------------------------------------------------------------
    // Template
    // ------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Template CSV (UTF-8). Baris berawalan # adalah petunjuk dan diabaikan pengurai. Tidak ada data pegawai atau nominal sungguhan.
    /// Kolom wajib kosong BUKAN nol: nol harus ditulis eksplisit.
    /// </summary>
    public PayrollOpeningTaxBalanceImportTemplateResponse GetImportTemplate()
    {
        var lines = new[]
        {
            "# Template Impor Saldo Awal PPh (CSV UTF-8). Baris yang diawali tanda # adalah petunjuk dan diabaikan.",
            "# Semua kolom wajib diisi, kecuali WorkforceType. Sel kosong BUKAN nol: tulis 0 bila memang nol.",
            "# WorkforceCode: NIP/kode pegawai atau nomor/kode dokter yang terdaftar. Pencocokan memakai kode, tidak pernah nama.",
            "# WorkforceType: Pegawai atau Dokter (boleh kosong; isi bila kode yang sama dipakai pegawai dan dokter).",
            $"# EmployerSource: {OpeningTaxBalanceEmployerSources.SameEmployerPreCutover} (dipakai rekonsiliasi PPh setelah disetujui) atau {OpeningTaxBalanceEmployerSources.PreviousEmployer} (arsip).",
            "# CoveredMonths: bulan 1-12 yang dicakup, mis. 1-11 atau 1;2;3 (gunakan titik koma atau rentang agar tidak perlu tanda kutip).",
            "# Nominal: angka polos, titik untuk desimal (maksimal 2 digit), tanpa pemisah ribuan dan tanpa Rp. Nilai negatif ditolak.",
            "# Contoh format (hapus tanda # dan ganti dengan data Anda; jangan impor contoh ini): KODE-UNIK,Pegawai,2026,SAME_EMPLOYER_PRE_CUTOVER,1-11,0,0,0,0",
            string.Join(",", ImportColumns)
        };

        return new PayrollOpeningTaxBalanceImportTemplateResponse
        {
            FileName = "template-impor-saldo-awal-pph.csv",
            ContentType = "text/csv; charset=utf-8",
            Content = string.Join("\r\n", lines) + "\r\n",
            Columns = ImportColumns.ToList(),
            SourceCodes = EmployerSources.ToList(),
            MaxRows = OpeningTaxBalanceImportLimits.MaxRows,
            MaxFileBytes = OpeningTaxBalanceImportLimits.MaxFileBytes
        };
    }

    // ------------------------------------------------------------------------------------------------------------------------
    // Validate (baca-saja) dan Commit (semua atau tidak sama sekali)
    // ------------------------------------------------------------------------------------------------------------------------

    public async Task<PayrollOpeningTaxBalanceImportResult> ValidateImportAsync(
        PayrollOpeningTaxBalanceImportRequest request, PayrollOpeningTaxBalanceCaller caller, CancellationToken cancellationToken)
    {
        await EnsureStorageAsync(cancellationToken);
        RequireAmountPermission(caller);
        var analysis = await AnalyzeImportAsync(request, cancellationToken);
        analysis.Result.Message = analysis.Result.InvalidRows == 0
            ? $"{analysis.Result.TotalRows} baris valid dan siap diimpor sebagai draf."
            : $"{analysis.Result.InvalidRows} dari {analysis.Result.TotalRows} baris bermasalah. Perbaiki berkas lalu ulangi; belum ada yang disimpan.";
        return analysis.Result;
    }

    public Task<PayrollOpeningTaxBalanceImportResult> CommitImportAsync(
        PayrollOpeningTaxBalanceImportRequest request, Guid actorUserId, PayrollOpeningTaxBalanceCaller caller, CancellationToken cancellationToken) =>
        PayrollTransactionRunner.RunAsync(_dbContext, LockKey, async () =>
        {
            await EnsureStorageAsync(cancellationToken);
            RequireAmountPermission(caller);

            // Validasi ULANG di dalam transaksi dan lock: perubahan data di antara preview dan commit terdeteksi di sini.
            var analysis = await AnalyzeImportAsync(request, cancellationToken);
            var result = analysis.Result;
            if (result.InvalidRows > 0)
            {
                result.Message = $"Impor dibatalkan. {result.InvalidRows} dari {result.TotalRows} baris bermasalah; tidak ada baris yang tersimpan.";
                throw new PayrollOpeningTaxBalanceImportException(result.Message, result);
            }

            var batchId = Guid.NewGuid();
            var now = DateTime.UtcNow;
            var created = new List<PrlOpeningTaxBalance>();
            foreach (var state in analysis.States)
            {
                var input = state.Input!;
                var maxVersion = analysis.Existing
                    .Where(x => x.WorkforceProfileId == input.WorkforceProfileId && x.TaxYear == input.TaxYear && x.EmployerSource == input.EmployerSource)
                    .Select(x => x.Version).DefaultIfEmpty(0).Max();
                var entity = new PrlOpeningTaxBalance
                {
                    Id = Guid.NewGuid(),
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    Version = maxVersion + 1,
                    Status = OpeningTaxBalanceStatuses.Draft
                };
                Apply(entity, input);
                created.Add(entity);
                state.CreatedId = entity.Id;
            }

            _dbContext.PrlOpeningTaxBalances.AddRange(created);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.AuditAsync(LogCategory, "OpeningTaxBalance.ImportCommit",
                $"Impor massal saldo awal PPh: {created.Count} draf dibuat.",
                new
                {
                    BatchId = batchId,
                    ActorUserId = actorUserId,
                    Count = created.Count,
                    request.FileName,
                    TaxYears = string.Join(",", created.Select(x => x.TaxYear).Distinct().OrderBy(y => y)),
                    Ids = created.Select(x => x.Id).ToList()
                });

            result.Committed = true;
            result.CreatedCount = created.Count;
            result.BatchId = batchId;
            result.Message = $"{created.Count} saldo awal berhasil disimpan sebagai draf. Ajukan dan setujui lewat alur yang sama dengan input manual.";
            foreach (var row in result.Rows)
                row.CreatedId = analysis.States.First(s => s.RowNumber == row.RowNumber).CreatedId;
            return result;
        }, cancellationToken);

    // ------------------------------------------------------------------------------------------------------------------------
    // Analisis bersama Validate dan Commit
    // ------------------------------------------------------------------------------------------------------------------------

    private async Task<ImportAnalysis> AnalyzeImportAsync(PayrollOpeningTaxBalanceImportRequest request, CancellationToken cancellationToken)
    {
        var rows = request?.Rows ?? new List<PayrollOpeningTaxBalanceImportRowRequest>();
        if (rows.Count == 0) throw new PayrollRunValidationException("Tidak ada baris untuk diimpor.");
        if (rows.Count > OpeningTaxBalanceImportLimits.MaxRows)
            throw new PayrollRunValidationException(
                $"Berkas memuat {rows.Count} baris. Paling banyak {OpeningTaxBalanceImportLimits.MaxRows} baris per impor; pecah menjadi beberapa berkas.");

        var states = rows.Select((row, index) => new ImportRowState { Row = row, RowNumber = row.RowNumber > 0 ? row.RowNumber : index + 2 }).ToList();
        if (states.Select(s => s.RowNumber).Distinct().Count() != states.Count)
            throw new PayrollRunValidationException("Nomor baris pada permintaan tidak unik.");

        await ResolveWorkforceAsync(states, cancellationToken);
        foreach (var state in states) ParseAndValidateFields(state);
        DetectFileConflicts(states);
        var existing = await DetectExistingConflictsAsync(states, cancellationToken);
        await DetectPayrollRunOverlapAsync(states, cancellationToken);

        var result = new PayrollOpeningTaxBalanceImportResult
        {
            TotalRows = states.Count,
            InvalidRows = states.Count(s => s.Issues.Count > 0),
            DuplicateRows = states.Count(s => s.Issues.Any(i => i.Code == "DUPLICATE_IN_FILE")),
            ConflictRows = states.Count(s => s.Issues.Any(i => i.Code is "EXISTING_BALANCE" or "SOURCE_OVERLAP" or "RUN_FINAL_OVERLAP")),
            Rows = states.Select(ToImportRowResult).ToList()
        };
        result.ValidRows = result.TotalRows - result.InvalidRows;
        result.ReadyRows = result.InvalidRows == 0 ? result.ValidRows : 0;
        return new ImportAnalysis(states, result, existing);
    }

    private static PayrollOpeningTaxBalanceImportRowResult ToImportRowResult(ImportRowState s) => new()
    {
        RowNumber = s.RowNumber,
        WorkforceCode = (s.Row.WorkforceCode ?? string.Empty).Trim(),
        WorkforceProfileId = s.ProfileId,
        WorkforceName = s.Name,
        WorkforceType = s.Type,
        WorkforceTypeLabel = s.TypeLabel,
        TaxYear = s.TaxYear,
        EmployerSource = s.EmployerSource,
        CoveredMonths = s.Months.OrderBy(m => m).ToList(),
        TaxableGross = s.TaxableGross,
        EmployerPremium = s.EmployerPremium,
        PensionDeduction = s.PensionDeduction,
        TaxWithheld = s.TaxWithheld,
        Valid = s.Issues.Count == 0,
        Errors = s.Issues.Select(i => new PayrollOpeningTaxBalanceImportIssue { Code = i.Code, Message = i.Message }).ToList()
    };

    private static string NormalizeCode(string? value) => (value ?? string.Empty).Trim().ToLowerInvariant();

    private async Task ResolveWorkforceAsync(List<ImportRowState> states, CancellationToken cancellationToken)
    {
        var codes = states.Select(s => NormalizeCode(s.Row.WorkforceCode)).Where(c => c.Length > 0).Distinct().ToList();
        var found = new List<WorkforceRecord>();
        if (codes.Count > 0)
        {
            // Satu query untuk seluruh kode. Pencocokan hanya pada kode/nomor/ProfileCode, tidak pernah nama. Tidak ada pembuatan master.
            var raw = await _dbContext.MstWorkforceProfiles.AsNoTracking()
                .Where(p => !p.IsDelete && (
                    codes.Contains(p.ProfileCode.ToLower())
                    || (p.Employee != null && !p.Employee.IsDelete
                        && (codes.Contains(p.Employee.EmployeeCode.ToLower()) || codes.Contains(p.Employee.EmployeeNumber.ToLower())))
                    || (p.Doctor != null && !p.Doctor.IsDelete
                        && (codes.Contains(p.Doctor.DoctorCode.ToLower()) || codes.Contains(p.Doctor.DoctorNumber.ToLower())))))
                .Select(p => new
                {
                    p.Id,
                    p.DisplayName,
                    p.ProfileCode,
                    HasEmployee = p.Employee != null && !p.Employee.IsDelete,
                    EmployeeCode = p.Employee != null ? p.Employee.EmployeeCode : null,
                    EmployeeNumber = p.Employee != null ? p.Employee.EmployeeNumber : null,
                    EmployeeName = p.Employee != null ? p.Employee.FullName : null,
                    HasDoctor = p.Doctor != null && !p.Doctor.IsDelete,
                    DoctorCode = p.Doctor != null ? p.Doctor.DoctorCode : null,
                    DoctorNumber = p.Doctor != null ? p.Doctor.DoctorNumber : null,
                    DoctorName = p.Doctor != null ? p.Doctor.FullName : null
                })
                .ToListAsync(cancellationToken);
            found = raw.Select(x => new WorkforceRecord(
                x.Id, x.DisplayName, x.ProfileCode, x.HasEmployee, x.EmployeeCode, x.EmployeeNumber, x.EmployeeName,
                x.HasDoctor, x.DoctorCode, x.DoctorNumber, x.DoctorName)).ToList();
        }

        foreach (var state in states)
        {
            var codeText = (state.Row.WorkforceCode ?? string.Empty).Trim();
            var code = NormalizeCode(codeText);
            if (code.Length == 0)
            {
                state.Issues.Add(new("WORKFORCE_REQUIRED", "Kode pegawai atau dokter wajib diisi."));
                continue;
            }

            var typeText = (state.Row.WorkforceType ?? string.Empty).Trim().ToLowerInvariant();
            string? wantedType = typeText switch
            {
                "" => null,
                "pegawai" or "karyawan" or "employee" => "Employee",
                "dokter" or "doctor" => "Doctor",
                _ => "INVALID"
            };
            if (wantedType == "INVALID")
            {
                state.Issues.Add(new("INVALID_WORKFORCE_TYPE", "Jenis tenaga kerja harus Pegawai atau Dokter (atau dikosongkan)."));
                continue;
            }

            var candidates = found
                .Where(p => p.Matches(code) && (wantedType switch
                {
                    null => p.HasEmployee || p.HasDoctor,
                    "Employee" => p.HasEmployee,
                    _ => p.HasDoctor
                }))
                .GroupBy(p => p.Id).Select(g => g.First()).ToList();

            if (candidates.Count == 0)
            {
                state.Issues.Add(new("WORKFORCE_NOT_FOUND", $"Kode '{codeText}' tidak ditemukan sebagai pegawai atau dokter."));
                continue;
            }

            if (candidates.Count > 1)
            {
                state.Issues.Add(new("AMBIGUOUS_WORKFORCE",
                    $"Kode '{codeText}' cocok dengan lebih dari satu pegawai atau dokter. Gunakan kode yang unik atau isi kolom WorkforceType."));
                continue;
            }

            var profile = candidates[0];
            var isEmployee = wantedType == "Employee" || (wantedType is null && profile.HasEmployee);
            state.ProfileId = profile.Id;
            state.Type = isEmployee ? "Employee" : "Doctor";
            state.TypeLabel = isEmployee ? "Pegawai" : "Dokter";
            var ownName = isEmployee ? profile.EmployeeName : profile.DoctorName;
            state.Name = string.IsNullOrWhiteSpace(ownName) ? profile.DisplayName : ownName;
        }
    }

    private sealed record WorkforceRecord(
        Guid Id, string DisplayName, string ProfileCode, bool HasEmployee, string? EmployeeCode, string? EmployeeNumber, string? EmployeeName,
        bool HasDoctor, string? DoctorCode, string? DoctorNumber, string? DoctorName)
    {
        public bool Matches(string code) =>
            string.Equals(ProfileCode, code, StringComparison.OrdinalIgnoreCase)
            || (HasEmployee && (string.Equals(EmployeeCode, code, StringComparison.OrdinalIgnoreCase) || string.Equals(EmployeeNumber, code, StringComparison.OrdinalIgnoreCase)))
            || (HasDoctor && (string.Equals(DoctorCode, code, StringComparison.OrdinalIgnoreCase) || string.Equals(DoctorNumber, code, StringComparison.OrdinalIgnoreCase)));
    }

    /// <summary>Menguraikan isian teks mentah, lalu menjalankan aturan bidang yang SAMA dengan input manual (ValidateFields).</summary>
    private void ParseAndValidateFields(ImportRowState state)
    {
        var row = state.Row;
        var currentYear = DateTime.UtcNow.Year;

        // Placeholder yang valid dipakai saat penguraian gagal supaya aturan bersama tidak melaporkan galat ganda untuk bidang yang sama.
        var yearText = (row.TaxYear ?? string.Empty).Trim();
        var year = currentYear;
        if (yearText.Length == 0) state.Issues.Add(new("TAX_YEAR_REQUIRED", "Tahun pajak wajib diisi."));
        else if (!int.TryParse(yearText, NumberStyles.None, CultureInfo.InvariantCulture, out year))
        {
            state.Issues.Add(new("TAX_YEAR_INVALID", "Tahun pajak harus berupa angka (mis. 2026)."));
            year = currentYear;
        }
        else state.TaxYear = year;

        var months = ParseMonths(row.CoveredMonths, out var monthsOk);
        if (!monthsOk)
        {
            state.Issues.Add(new("MONTHS_INVALID", "Bulan yang dicakup tidak dapat dibaca. Gunakan angka 1-12 dipisah koma/titik koma atau rentang (mis. 1-11)."));
            months = new List<int> { 1 };
        }
        else state.Months = months;

        state.EmployerSource = (row.EmployerSource ?? string.Empty).Trim().ToUpperInvariant();

        decimal Amount(string? raw, string label, Action<decimal?> assign)
        {
            if (!TryParseAmount(raw, out var value, out var code))
            {
                var message = code switch
                {
                    "AMOUNT_REQUIRED" => $"{label} wajib diisi (isi 0 bila memang nol).",
                    "AMOUNT_NEGATIVE" => $"{label} tidak boleh negatif.",
                    _ => $"{label} harus berupa angka tidak negatif (maksimal dua desimal)."
                };
                state.Issues.Add(new(code!, message));
                assign(null);
                return 0m;
            }

            assign(value);
            return value;
        }

        var gross = Amount(row.TaxableGross, "Penghasilan kena pajak", v => state.TaxableGross = v);
        var premium = Amount(row.EmployerPremium, "Premi pemberi kerja", v => state.EmployerPremium = v);
        var pension = Amount(row.PensionDeduction, "Iuran pensiun", v => state.PensionDeduction = v);
        var withheld = Amount(row.TaxWithheld, "PPh yang sudah dipotong", v => state.TaxWithheld = v);

        var request = new SavePayrollOpeningTaxBalanceRequest
        {
            WorkforceProfileId = state.ProfileId ?? Guid.Empty,
            TaxYear = year,
            CoveredMonths = months,
            EmployerSource = state.EmployerSource,
            TaxableGross = gross,
            EmployerPremium = premium,
            PensionDeduction = pension,
            TaxWithheld = withheld
        };
        var shared = new List<ImportIssue>();
        state.Input = ValidateFields(request, shared);
        state.Issues.AddRange(shared);
    }

    private static List<int> ParseMonths(string? text, out bool ok)
    {
        ok = true;
        var months = new List<int>();
        var trimmed = (text ?? string.Empty).Trim();
        if (trimmed.Length == 0) return months;
        foreach (var token in MonthSeparator.Split(trimmed).Where(t => t.Length > 0))
        {
            var range = MonthRange.Match(token);
            if (MonthToken.IsMatch(token))
            {
                months.Add(int.Parse(token, CultureInfo.InvariantCulture));
            }
            else if (range.Success
                && int.Parse(range.Groups[1].Value, CultureInfo.InvariantCulture) <= int.Parse(range.Groups[2].Value, CultureInfo.InvariantCulture))
            {
                var first = int.Parse(range.Groups[1].Value, CultureInfo.InvariantCulture);
                var last = int.Parse(range.Groups[2].Value, CultureInfo.InvariantCulture);
                for (var month = first; month <= last; month++) months.Add(month);
            }
            else
            {
                ok = false;
                return new List<int>();
            }
        }

        return months;
    }

    /// <summary>Angka polos, titik desimal (maks. 2 digit). Pemisah ribuan titik / format "1.500,50" tetap dibaca seperti layar manual. Kosong BUKAN nol.</summary>
    private static bool TryParseAmount(string? raw, out decimal value, out string? code)
    {
        value = 0m;
        code = null;
        var text = Regex.Replace(raw ?? string.Empty, @"\s", string.Empty);
        if (text.Length == 0) { code = "AMOUNT_REQUIRED"; return false; }
        if (text.StartsWith('-')) { code = "AMOUNT_NEGATIVE"; return false; }
        var normalized = text.Contains(',') ? text.Replace(".", string.Empty).Replace(',', '.') : ThousandDots.Replace(text, string.Empty);
        if (!AmountPlain.IsMatch(normalized)
            || !decimal.TryParse(normalized, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out value))
        {
            code = "AMOUNT_INVALID";
            return false;
        }

        return true;
    }

    /// <summary>Duplikat kunci di dalam berkas, dan dua sumber berbeda untuk orang dan tahun yang sama yang mencakup bulan yang sama.</summary>
    private static void DetectFileConflicts(List<ImportRowState> states)
    {
        var keyed = states.Where(s => s.HasKey).ToList();
        foreach (var group in keyed.GroupBy(s => (s.ProfileId, s.TaxYear, s.EmployerSource)).Where(g => g.Count() > 1))
        {
            var numbers = group.Select(s => s.RowNumber).OrderBy(n => n).ToList();
            foreach (var state in group)
                state.Issues.Add(new("DUPLICATE_IN_FILE",
                    $"Duplikat dalam berkas: pegawai/dokter, tahun pajak, dan sumber yang sama juga ada pada baris {string.Join(", ", numbers.Where(n => n != state.RowNumber))}."));
        }

        foreach (var group in keyed.GroupBy(s => (s.ProfileId, s.TaxYear)))
        {
            var list = group.ToList();
            foreach (var state in list)
            {
                var overlapRows = list.Where(o => o != state && o.EmployerSource != state.EmployerSource && o.Months.Intersect(state.Months).Any())
                    .Select(o => o.RowNumber).OrderBy(n => n).ToList();
                if (overlapRows.Count > 0)
                    state.Issues.Add(new("SOURCE_OVERLAP",
                        $"Bulan yang dicakup beririsan dengan sumber lain untuk pegawai/dokter yang sama pada baris {string.Join(", ", overlapRows)}; penghasilan dan PPh akan terhitung dua kali."));
            }
        }
    }

    /// <summary>Konflik dengan saldo yang sudah tersimpan: impor tidak pernah menimpa draf, pengajuan, maupun saldo yang disetujui.</summary>
    private async Task<List<PrlOpeningTaxBalance>> DetectExistingConflictsAsync(List<ImportRowState> states, CancellationToken cancellationToken)
    {
        var targets = states.Where(s => s.ProfileId.HasValue && s.TaxYear.HasValue).ToList();
        if (targets.Count == 0) return new List<PrlOpeningTaxBalance>();

        var ids = targets.Select(s => s.ProfileId!.Value).Distinct().ToList();
        var years = targets.Select(s => s.TaxYear!.Value).Distinct().ToList();
        var existing = await _dbContext.PrlOpeningTaxBalances.AsNoTracking()
            .Where(x => !x.IsDelete && ids.Contains(x.WorkforceProfileId) && years.Contains(x.TaxYear))
            .ToListAsync(cancellationToken);

        foreach (var state in targets.Where(s => s.SourceValid))
        {
            var sameKey = existing.Where(x => x.WorkforceProfileId == state.ProfileId && x.TaxYear == state.TaxYear && x.EmployerSource == state.EmployerSource).ToList();
            var open = sameKey.FirstOrDefault(x => OpenStatuses.Contains(x.Status));
            if (open is not null)
                state.Issues.Add(new("EXISTING_BALANCE", open.Status == OpeningTaxBalanceStatuses.Approved
                    ? "Sudah ada saldo awal yang disetujui untuk pegawai/dokter, tahun pajak, dan sumber yang sama. Impor tidak menimpa; buat revisi dari saldo yang disetujui."
                    : "Sudah ada saldo awal yang masih berjalan (draf atau menunggu persetujuan) untuk pegawai/dokter, tahun pajak, dan sumber yang sama. Ubah drafnya atau selesaikan lebih dulu."));

            var approvedOther = existing
                .Where(x => x.WorkforceProfileId == state.ProfileId && x.TaxYear == state.TaxYear && x.EmployerSource != state.EmployerSource
                    && x.Status == OpeningTaxBalanceStatuses.Approved)
                .SelectMany(x => x.CoveredMonths).Where(state.Months.Contains).Distinct().OrderBy(m => m).ToList();
            if (approvedOther.Count > 0)
                state.Issues.Add(new("SOURCE_OVERLAP",
                    $"Bulan {string.Join(", ", approvedOther)} sudah tercakup saldo awal lain yang disetujui untuk pegawai/dokter ini."));
        }

        return existing;
    }

    /// <summary>Bulan yang sudah tercakup payroll run final akan terhitung dua kali; aturan yang sama dengan ApproveAsync, diperiksa sejak impor.</summary>
    private async Task DetectPayrollRunOverlapAsync(List<ImportRowState> states, CancellationToken cancellationToken)
    {
        var targets = states.Where(s => s.ProfileId.HasValue && s.TaxYear.HasValue && s.Months.Count > 0).ToList();
        if (targets.Count == 0) return;

        var ids = targets.Select(s => s.ProfileId!.Value).Distinct().ToList();
        var minYear = targets.Min(s => s.TaxYear!.Value);
        var maxYear = targets.Max(s => s.TaxYear!.Value);
        var rangeStart = new DateTime(minYear, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var rangeEnd = new DateTime(maxYear, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddYears(1);
        var runEnds = await _dbContext.PrlPayrollRunEmployees.AsNoTracking()
            .Where(x => !x.IsDelete && ids.Contains(x.WorkforceProfileId) && x.PayrollRun != null && !x.PayrollRun.IsDelete
                && x.PayrollRun.PeriodEndDateSnapshot >= rangeStart && x.PayrollRun.PeriodEndDateSnapshot < rangeEnd
                && PayrollCalculationService.AccumulatedRunStatuses.Contains(x.PayrollRun.RunStatus))
            .Select(x => new { x.WorkforceProfileId, End = x.PayrollRun!.PeriodEndDateSnapshot })
            .ToListAsync(cancellationToken);
        if (runEnds.Count == 0) return;

        foreach (var state in targets)
        {
            var runMonths = runEnds.Where(r => r.WorkforceProfileId == state.ProfileId && r.End.Year == state.TaxYear).Select(r => r.End.Month).ToHashSet();
            var overlap = state.Months.Where(runMonths.Contains).Distinct().OrderBy(m => m).ToList();
            if (overlap.Count > 0)
                state.Issues.Add(new("RUN_FINAL_OVERLAP",
                    $"Bulan {string.Join(", ", overlap)} tahun {state.TaxYear} sudah tercakup payroll run yang disahkan; penghasilan dan PPh akan terhitung dua kali. Koreksi cakupan bulan."));
        }
    }
}
