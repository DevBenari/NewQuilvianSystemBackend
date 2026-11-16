using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;

/// <summary>
/// Kelengkapan data payroll per pegawai dan dokter aktif beserta impor profil payroll. Daftar bersumber dari POPULASI pegawai dan
/// dokter (master HRD), bukan dari profil payroll, sehingga orang yang belum punya profil tetap tampil.
/// Batas kepemilikan: Payroll hanya MEMBACA data HRD (penetapan gaji utama dan rekening); satu-satunya yang ditulis di sini adalah
/// profil payroll (PrlPayroll), milik Payroll. Impor mengambil struktur, grade, penetapan gaji, dan rekening dari data HRD, dan
/// menolak baris yang data HRD-nya belum ada. Semua atau tidak sama sekali dalam satu SaveChanges (satu transaksi database) dan
/// idempoten: profil yang sudah ada dilewati. Nominal pajak dan iuran tidak pernah ditulis ke log.
/// </summary>
public sealed class PayrollDataReadinessService
{
    private const string LogCategory = "Corporate.PayrollManagement.PayrollDataReadiness";
    private const string PersonEmployee = "Karyawan";
    private const string PersonDoctor = "Dokter";
    private const string CompleteFilter = "Lengkap";
    private const string IncompleteFilter = "Belum lengkap";
    private const string BankNone = "Belum ada";
    private const string BankUnverified = "Belum terverifikasi";
    private const string BankVerified = "Terverifikasi";
    private const int MaxPageSize = 100;
    private const int MaxImportRows = 200;
    private const string ImportDescription = "Dimuat lewat Kelengkapan Data Payroll.";

    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;

    public PayrollDataReadinessService(ApplicationDbContext dbContext, LoggerService loggerService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
    }

    private sealed record Person(Guid WorkforceProfileId, Guid? EmployeeId, string Type, string Number, string Name);

    private sealed record AssignmentInfo(Guid Id, Guid SalaryStructureId, Guid SalaryGradeId, DateTime EffectiveStartDate, bool Approved);

    private sealed record BankInfo(Guid Id, bool Verified);

    private sealed record RowPlan(int RowNumber, Person Person, Guid? CostCenterId, ImportPayrollDataRow Row);

    // ------------------------------------------------------------------------------------
    // Baca
    // ------------------------------------------------------------------------------------

    public async Task<PagedResult<PayrollDataReadinessItemResponse>> ListAsync(
        PayrollDataReadinessQuery query, CancellationToken cancellationToken)
    {
        IEnumerable<PayrollDataReadinessItemResponse> rows = await LoadReadinessAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(query.PersonType))
        {
            var type = query.PersonType.Trim();
            rows = rows.Where(x => string.Equals(x.PersonType, type, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            rows = rows.Where(x =>
                x.Name.Contains(term, StringComparison.OrdinalIgnoreCase)
                || x.Number.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        var completeness = query.Completeness?.Trim();
        if (string.Equals(completeness, CompleteFilter, StringComparison.OrdinalIgnoreCase))
            rows = rows.Where(x => x.IsComplete);
        else if (string.Equals(completeness, IncompleteFilter, StringComparison.OrdinalIgnoreCase))
            rows = rows.Where(x => !x.IsComplete);

        var filtered = rows
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.Number, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);

        return new PagedResult<PayrollDataReadinessItemResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = filtered.Count,
            TotalPage = (int)Math.Ceiling(filtered.Count / (double)pageSize),
            Items = filtered.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList()
        };
    }

    public async Task<PayrollDataReadinessSummaryResponse> SummaryAsync(CancellationToken cancellationToken)
    {
        var all = await LoadReadinessAsync(cancellationToken);
        return new PayrollDataReadinessSummaryResponse
        {
            TotalPeople = all.Count,
            WithPayrollProfile = all.Count(x => x.HasPayrollProfile),
            WithApprovedSalaryAssignment = all.Count(x => x.HasApprovedSalaryAssignment),
            WithVerifiedBankAccount = all.Count(x => x.BankStatus == BankVerified),
            Complete = all.Count(x => x.IsComplete)
        };
    }

    /// <summary>
    /// Populasi (pegawai dan dokter aktif dari master HRD) dipertemukan dengan tiga set kelengkapan di memori. Jumlahnya dibatasi
    /// jumlah SDM rumah sakit, dan penyaringan lewat kelengkapan tidak dapat dipaginasi di database tanpa menggabungkan empat tabel.
    /// </summary>
    private async Task<List<PayrollDataReadinessItemResponse>> LoadReadinessAsync(CancellationToken cancellationToken)
    {
        var people = await LoadPeopleAsync(cancellationToken);

        var profiles = (await _dbContext.PrlPayrolls.AsNoTracking()
                .Where(x => !x.IsDelete)
                .Select(x => new { x.WorkforceProfileId, x.IsActive, x.IsPayrollEligible, x.PayrollStatus })
                .ToListAsync(cancellationToken))
            .GroupBy(x => x.WorkforceProfileId)
            .ToDictionary(g => g.Key, g => g.First());

        var assignments = (await _dbContext.WfpSalaryAssignments.AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && x.IsPrimary)
                .Select(x => new { x.WorkforceProfileId, Approved = x.ApprovedAt != null })
                .ToListAsync(cancellationToken))
            .GroupBy(x => x.WorkforceProfileId)
            .ToDictionary(g => g.Key, g => (Approved: g.Any(a => a.Approved), Pending: g.Any(a => !a.Approved)));

        var banks = (await _dbContext.WfpBankAccounts.AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && x.IsPayrollAccount)
                .Select(x => new { x.WorkforceProfileId, x.IsVerified })
                .ToListAsync(cancellationToken))
            .GroupBy(x => x.WorkforceProfileId)
            .ToDictionary(g => g.Key, g => g.Any(b => b.IsVerified) ? BankVerified : BankUnverified);

        var items = new List<PayrollDataReadinessItemResponse>(people.Count);
        foreach (var person in people)
        {
            var issues = new List<string>();

            var profileExists = profiles.TryGetValue(person.WorkforceProfileId, out var profile);
            var profileReady = profileExists
                && profile!.IsActive
                && profile.IsPayrollEligible
                && string.Equals(profile.PayrollStatus, "Active", StringComparison.OrdinalIgnoreCase);
            if (!profileExists) issues.Add("Profil payroll belum ada.");
            else if (!profileReady) issues.Add("Profil payroll tidak aktif, tidak eligible, atau tidak berstatus Active.");

            // Penetapan gaji dan rekening milik HRD: pesan menyebut HRD supaya jelas siapa yang melengkapinya.
            var hasAssignment = assignments.TryGetValue(person.WorkforceProfileId, out var assignment);
            var approved = hasAssignment && assignment.Approved;
            var pending = hasAssignment && assignment.Pending;
            if (!hasAssignment) issues.Add("Belum ada penetapan gaji utama di HRD.");
            else if (!approved) issues.Add("Penetapan gaji utama belum disetujui HRD.");

            var bankStatus = banks.GetValueOrDefault(person.WorkforceProfileId) ?? BankNone;
            if (bankStatus == BankNone) issues.Add("Belum ada rekening payroll di HRD.");
            else if (bankStatus == BankUnverified) issues.Add("Rekening payroll belum terverifikasi HRD.");

            items.Add(new PayrollDataReadinessItemResponse
            {
                WorkforceProfileId = person.WorkforceProfileId,
                PersonType = person.Type,
                Number = person.Number,
                Name = person.Name,
                HasPayrollProfile = profileReady,
                HasApprovedSalaryAssignment = approved,
                HasPendingSalaryAssignment = pending,
                BankStatus = bankStatus,
                IsComplete = issues.Count == 0,
                Issues = issues
            });
        }

        return items;
    }

    private async Task<List<Person>> LoadPeopleAsync(CancellationToken cancellationToken)
    {
        var seen = new HashSet<Guid>();
        var people = new List<Person>();

        var employees = await _dbContext.MstEmployees.AsNoTracking()
            .Where(x => !x.IsDelete && x.IsActive)
            .Select(x => new { x.Id, x.WorkforceProfileId, x.EmployeeNumber, x.FullName })
            .ToListAsync(cancellationToken);
        foreach (var e in employees)
            if (seen.Add(e.WorkforceProfileId))
                people.Add(new Person(e.WorkforceProfileId, e.Id, PersonEmployee, e.EmployeeNumber, e.FullName));

        var doctors = await _dbContext.MstDoctors.AsNoTracking()
            .Where(x => !x.IsDelete && x.IsActive)
            .Select(x => new { x.WorkforceProfileId, x.DoctorNumber, x.FullName })
            .ToListAsync(cancellationToken);
        foreach (var d in doctors)
            if (seen.Add(d.WorkforceProfileId))
                people.Add(new Person(d.WorkforceProfileId, null, PersonDoctor, d.DoctorNumber, d.FullName));

        return people;
    }

    // ------------------------------------------------------------------------------------
    // Impor profil payroll
    // ------------------------------------------------------------------------------------

    /// <summary>
    /// Semua baris divalidasi lebih dulu (orang, Cost Center, nilai pajak dan iuran, kelengkapan data HRD). Satu baris gagal
    /// membatalkan impor dan tidak ada data yang tersimpan. Uji kering menjalankan seluruh pemeriksaan dan menghitung hasilnya
    /// tanpa menyimpan apa pun. Data HRD tidak pernah diubah: penetapan gaji dan rekening hanya dibaca.
    /// </summary>
    public async Task<PayrollDataImportResult> ImportAsync(
        ImportPayrollDataRequest request,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var rows = request.Rows ?? new List<ImportPayrollDataRow>();
        if (rows.Count == 0) throw new ArgumentException("Tidak ada baris untuk diimpor.");
        if (rows.Count > MaxImportRows)
            throw new ArgumentException($"Paling banyak {MaxImportRows} baris per impor; pecah menjadi beberapa berkas.");

        var peopleByNumber = await LoadPeopleByNumberAsync(rows, cancellationToken);
        var costCenters = await LoadCostCentersAsync(rows, cancellationToken);

        var failed = new List<PayrollDataImportFailedRow>();
        var plans = new List<RowPlan>();
        var firstRowOfPerson = new Dictionary<Guid, int>();

        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            var rowNumber = i + 1;
            var reasons = new List<string>();

            var number = row.PersonNumber?.Trim();
            Person? person = null;
            if (string.IsNullOrEmpty(number))
                reasons.Add("Nomor pegawai atau dokter kosong.");
            else if (!peopleByNumber.TryGetValue(number, out var matches))
                reasons.Add($"Pegawai atau dokter {number} tidak ditemukan atau tidak aktif di HRD.");
            else if (matches.Count > 1)
                reasons.Add($"Nomor {number} cocok dengan lebih dari satu orang.");
            else
                person = matches[0];

            if (person is not null)
            {
                if (firstRowOfPerson.TryGetValue(person.WorkforceProfileId, out var firstRow))
                    reasons.Add($"Orang ini sudah muncul di baris {firstRow}.");
                else
                    firstRowOfPerson[person.WorkforceProfileId] = rowNumber;
            }

            Guid? costCenterId = null;
            var costCenterCode = row.CostCenterCode?.Trim();
            if (!string.IsNullOrEmpty(costCenterCode))
            {
                if (costCenters.TryGetValue(costCenterCode, out var costCenterFound)) costCenterId = costCenterFound;
                else reasons.Add($"Cost Center {costCenterCode} tidak ditemukan atau tidak aktif.");
            }

            if (row.TaxAmount < 0m || row.InsuranceAmount < 0m)
                reasons.Add("Nilai pajak dan iuran tidak boleh negatif.");

            if (reasons.Count > 0)
            {
                failed.Add(new PayrollDataImportFailedRow { RowNumber = rowNumber, Reason = string.Join(" ", reasons) });
                continue;
            }

            plans.Add(new RowPlan(rowNumber, person!, costCenterId, row));
        }

        // Keadaan Payroll dan HRD untuk orang yang lolos isian. Seluruhnya hanya dibaca (AsNoTracking).
        var profileIds = plans.Select(p => p.Person.WorkforceProfileId).ToList();

        var existingProfileIds = (await _dbContext.PrlPayrolls.AsNoTracking()
                .Where(x => !x.IsDelete && profileIds.Contains(x.WorkforceProfileId))
                .Select(x => x.WorkforceProfileId)
                .ToListAsync(cancellationToken))
            .ToHashSet();

        var assignments = (await _dbContext.WfpSalaryAssignments.AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && x.IsPrimary && profileIds.Contains(x.WorkforceProfileId))
                .Select(x => new
                {
                    x.Id,
                    x.WorkforceProfileId,
                    x.SalaryStructureId,
                    x.SalaryGradeId,
                    x.EffectiveStartDate,
                    Approved = x.ApprovedAt != null
                })
                .ToListAsync(cancellationToken))
            .GroupBy(x => x.WorkforceProfileId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(a => a.EffectiveStartDate)
                    .Select(a => new AssignmentInfo(a.Id, a.SalaryStructureId, a.SalaryGradeId, a.EffectiveStartDate, a.Approved))
                    .First());

        // Rekening payroll aktif milik HRD: yang terverifikasi didahulukan, lalu rekening utama, lalu yang terbaru.
        var banks = (await _dbContext.WfpBankAccounts.AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && x.IsPayrollAccount && profileIds.Contains(x.WorkforceProfileId))
                .Select(x => new { x.Id, x.WorkforceProfileId, x.IsVerified, x.IsPrimary, x.CreateDateTime })
                .ToListAsync(cancellationToken))
            .GroupBy(x => x.WorkforceProfileId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(b => b.IsVerified)
                    .ThenByDescending(b => b.IsPrimary)
                    .ThenByDescending(b => b.CreateDateTime)
                    .Select(b => new BankInfo(b.Id, b.IsVerified))
                    .First());

        // Profil baru butuh penetapan gaji utama dan rekening payroll di HRD. Profil yang sudah ada dilewati tanpa syarat itu.
        foreach (var plan in plans)
        {
            var profileId = plan.Person.WorkforceProfileId;
            if (existingProfileIds.Contains(profileId)) continue;

            var reasons = new List<string>();
            if (!assignments.ContainsKey(profileId))
                reasons.Add("Penetapan gaji utama belum ada di data HRD. Lengkapi di HRD lalu impor lagi.");
            if (!banks.ContainsKey(profileId))
                reasons.Add("Rekening payroll belum ada di data HRD. Lengkapi di HRD lalu impor lagi.");
            if (reasons.Count > 0)
                failed.Add(new PayrollDataImportFailedRow { RowNumber = plan.RowNumber, Reason = string.Join(" ", reasons) });
        }

        if (failed.Count > 0)
        {
            throw new PayrollDataImportException(
                $"Impor dibatalkan. {failed.Count} baris bermasalah; tidak ada data yang tersimpan.",
                new PayrollDataImportResult
                {
                    DryRun = request.DryRun,
                    RowCount = rows.Count,
                    FailedRows = failed.OrderBy(f => f.RowNumber).ToList()
                });
        }

        var now = DateTime.UtcNow;
        var result = new PayrollDataImportResult { DryRun = request.DryRun, RowCount = plans.Count };
        var newProfiles = new List<PrlPayroll>();

        foreach (var plan in plans)
        {
            var profileId = plan.Person.WorkforceProfileId;
            var rowResult = new PayrollDataImportRowResult
            {
                RowNumber = plan.RowNumber,
                PersonNumber = plan.Person.Number,
                PersonName = plan.Person.Name
            };

            if (existingProfileIds.Contains(profileId))
            {
                rowResult.PayrollProfile = "Sudah ada (dilewati)";
                rowResult.Warnings.Add("Profil payroll sudah ada; pajak, iuran, dan Cost Center dari berkas tidak dipakai.");
                result.PayrollProfilesSkipped++;
                result.Rows.Add(rowResult);
                continue;
            }

            var assignment = assignments[profileId];
            var bank = banks[profileId];

            rowResult.SalaryAssignment = assignment.Approved ? "Disetujui HRD" : "Menunggu persetujuan HRD";
            rowResult.BankAccount = bank.Verified ? "Terverifikasi HRD" : "Belum terverifikasi";
            if (!assignment.Approved)
                rowResult.Warnings.Add("Penetapan gaji utama belum disetujui HRD; gaji belum dapat dihitung sampai disetujui.");
            if (!bank.Verified)
                rowResult.Warnings.Add("Rekening payroll belum terverifikasi HRD; pembayaran akan ditahan sampai terverifikasi.");

            newProfiles.Add(new PrlPayroll
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = profileId,
                EmployeeId = plan.Person.EmployeeId,
                SalaryAssignmentId = assignment.Id,
                BankAccountId = bank.Id,
                CostCenterId = plan.CostCenterId,
                SalaryStructureId = assignment.SalaryStructureId,
                SalaryGradeId = assignment.SalaryGradeId,
                PayrollStatus = "Active",
                CurrencyCode = "IDR",
                PaymentFrequency = "Monthly",
                PaymentMethod = "BankTransfer",
                IsPayrollEligible = true,
                IsConfidential = true,
                // timestamptz wajib berkind UTC; tengah malam UTC sama dengan tanggal berlaku penetapan gaji di HRD.
                EffectiveStartDate = DateTime.SpecifyKind(assignment.EffectiveStartDate.Date, DateTimeKind.Utc),
                // Gaji dihitung dari penetapan gaji; kolom gaji pada profil hanya kompatibilitas dan sengaja nol.
                TaxAmount = plan.Row.TaxAmount,
                InsuranceAmount = plan.Row.InsuranceAmount,
                Description = ImportDescription,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId
            });
            rowResult.PayrollProfile = "Dibuat";
            result.PayrollProfilesCreated++;
            result.Rows.Add(rowResult);
        }

        if (!request.DryRun)
        {
            _dbContext.PrlPayrolls.AddRange(newProfiles);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.AuditAsync(LogCategory, "PayrollDataReadiness.Import",
                "Profil payroll pegawai dan dokter dimuat dari data HRD.",
                new
                {
                    RowCount = result.RowCount,
                    result.PayrollProfilesCreated,
                    result.PayrollProfilesSkipped,
                    Status = "Imported",
                    ActorUserId = actorUserId
                });
        }

        return result;
    }

    // ------------------------------------------------------------------------------------
    // Pencarian master untuk impor
    // ------------------------------------------------------------------------------------

    private async Task<Dictionary<string, List<Person>>> LoadPeopleByNumberAsync(
        List<ImportPayrollDataRow> rows, CancellationToken cancellationToken)
    {
        var numbers = DistinctUpper(rows.Select(r => r.PersonNumber));
        var result = new Dictionary<string, List<Person>>(StringComparer.OrdinalIgnoreCase);

        void Add(Person person)
        {
            var key = person.Number.Trim();
            if (!result.TryGetValue(key, out var list)) result[key] = list = new List<Person>();
            if (list.All(p => p.WorkforceProfileId != person.WorkforceProfileId)) list.Add(person);
        }

        var employees = await _dbContext.MstEmployees.AsNoTracking()
            .Where(x => !x.IsDelete && x.IsActive && numbers.Contains(x.EmployeeNumber.ToUpper()))
            .Select(x => new { x.Id, x.WorkforceProfileId, x.EmployeeNumber, x.FullName })
            .ToListAsync(cancellationToken);
        foreach (var e in employees)
            Add(new Person(e.WorkforceProfileId, e.Id, PersonEmployee, e.EmployeeNumber, e.FullName));

        var doctors = await _dbContext.MstDoctors.AsNoTracking()
            .Where(x => !x.IsDelete && x.IsActive && numbers.Contains(x.DoctorNumber.ToUpper()))
            .Select(x => new { x.WorkforceProfileId, x.DoctorNumber, x.FullName })
            .ToListAsync(cancellationToken);
        foreach (var d in doctors)
            Add(new Person(d.WorkforceProfileId, null, PersonDoctor, d.DoctorNumber, d.FullName));

        return result;
    }

    private async Task<Dictionary<string, Guid>> LoadCostCentersAsync(
        List<ImportPayrollDataRow> rows, CancellationToken cancellationToken)
    {
        var codes = DistinctUpper(rows.Select(r => r.CostCenterCode));
        if (codes.Count == 0) return new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);

        return (await _dbContext.MstCostCenters.AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && codes.Contains(x.CostCenterCode.ToUpper()))
                .Select(x => new { x.Id, x.CostCenterCode })
                .ToListAsync(cancellationToken))
            .GroupBy(x => x.CostCenterCode.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Id, StringComparer.OrdinalIgnoreCase);
    }

    private static List<string> DistinctUpper(IEnumerable<string?> values) =>
        values.Select(v => v?.Trim().ToUpperInvariant())
            .Where(v => !string.IsNullOrEmpty(v))
            .Select(v => v!)
            .Distinct()
            .ToList();
}
