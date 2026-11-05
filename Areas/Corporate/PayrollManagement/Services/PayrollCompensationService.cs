using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;

/// <summary>
/// Model baca "Data Penghasilan Pegawai &amp; Dokter" (PAY-BE-03). Menggabungkan profil penggajian, penetapan gaji yang berlaku,
/// rekening, pajak, dan iuran per penerima, beserta masalah kelengkapannya. HRD tetap pemilik identitas, penetapan gaji, dan rekening;
/// layanan ini hanya MEMBACA (tidak membuat identitas ganda dan tidak mengubah kompensasi). Persetujuan penetapan gaji tetap lewat
/// endpoint penetapan gaji yang sudah ada.
///
/// Pengamanan: nominal gaji hanya dikirim bila pemanggil berwenang ViewAmount; rekening dan NPWP selalu disamarkan.
/// </summary>
public sealed class PayrollCompensationService
{
    private const string PersonEmployee = "Karyawan";
    private const string PersonDoctor = "Dokter";

    private readonly ApplicationDbContext _dbContext;

    public PayrollCompensationService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    private sealed record Person(Guid WorkforceProfileId, string Type, string Number, string Name);

    public async Task<PagedResult<PayrollCompensationListItemResponse>> GetPagedAsync(
        PayrollCompensationQuery query, bool includeAmounts, CancellationToken cancellationToken)
    {
        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var today = DateTime.UtcNow.Date;

        var profileQuery = _dbContext.WfpPayrolls.AsNoTracking().Where(x => !x.IsDelete && x.IsActive);
        if (!string.IsNullOrWhiteSpace(query.PayrollStatus))
            profileQuery = profileQuery.Where(x => x.PayrollStatus == query.PayrollStatus);
        var profiles = await profileQuery.ToListAsync(cancellationToken);

        // Satu penerima bisa punya beberapa profil berdasarkan masa berlaku; ambil yang berlaku hari ini, jika tidak ada yang terbaru.
        var chosen = profiles.GroupBy(x => x.WorkforceProfileId)
            .Select(g => g.OrderByDescending(p => IsEffective(p.EffectiveStartDate, p.EffectiveEndDate, today))
                .ThenByDescending(p => p.EffectiveStartDate).First())
            .ToList();
        var workforceIds = chosen.Select(x => x.WorkforceProfileId).ToList();

        var people = await LoadPeopleAsync(workforceIds, cancellationToken);

        var approvedEffective = (await _dbContext.WfpSalaryAssignments.AsNoTracking()
                .Where(a => !a.IsDelete && a.IsActive && a.IsPrimary && a.ApprovedAt != null
                    && workforceIds.Contains(a.WorkforceProfileId)
                    && a.EffectiveStartDate <= today && (a.EffectiveEndDate == null || a.EffectiveEndDate >= today))
                .Select(a => a.WorkforceProfileId).Distinct().ToListAsync(cancellationToken)).ToHashSet();
        var verifiedBank = (await _dbContext.WfpBankAccounts.AsNoTracking()
                .Where(b => !b.IsDelete && b.IsActive && b.IsPayrollAccount && b.IsVerified && workforceIds.Contains(b.WorkforceProfileId))
                .Select(b => b.WorkforceProfileId).Distinct().ToListAsync(cancellationToken)).ToHashSet();

        var rows = chosen
            .Where(p => people.ContainsKey(p.WorkforceProfileId))
            .Select(p => new { Profile = p, Person = people[p.WorkforceProfileId] })
            .Where(r => string.IsNullOrWhiteSpace(query.PersonType)
                || string.Equals(r.Person.Type, query.PersonType, StringComparison.OrdinalIgnoreCase))
            .Where(r => string.IsNullOrWhiteSpace(query.Search)
                || r.Person.Name.Contains(query.Search.Trim(), StringComparison.OrdinalIgnoreCase)
                || r.Person.Number.Contains(query.Search.Trim(), StringComparison.OrdinalIgnoreCase))
            .Where(r =>
            {
                if (string.IsNullOrWhiteSpace(query.Readiness)) return true;
                var ready = r.Profile.IsPayrollEligible
                    && string.Equals(r.Profile.PayrollStatus, "Active", StringComparison.OrdinalIgnoreCase)
                    && approvedEffective.Contains(r.Profile.WorkforceProfileId)
                    && verifiedBank.Contains(r.Profile.WorkforceProfileId);
                return string.Equals(query.Readiness, "Siap", StringComparison.OrdinalIgnoreCase) ? ready : !ready;
            })
            .OrderBy(r => r.Person.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var pageRows = rows.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        var pageWorkforceIds = pageRows.Select(r => r.Profile.WorkforceProfileId).ToList();

        var assignments = (await _dbContext.WfpSalaryAssignments.AsNoTracking()
                .Where(a => !a.IsDelete && pageWorkforceIds.Contains(a.WorkforceProfileId))
                .ToListAsync(cancellationToken))
            .GroupBy(a => a.WorkforceProfileId).ToDictionary(g => g.Key, g => g.ToList());
        var banks = (await _dbContext.WfpBankAccounts.AsNoTracking()
                .Where(b => !b.IsDelete && b.IsActive && pageWorkforceIds.Contains(b.WorkforceProfileId))
                .ToListAsync(cancellationToken))
            .GroupBy(b => b.WorkforceProfileId).ToDictionary(g => g.Key, g => g.ToList());
        var taxIds = (await _dbContext.WfpTaxes.AsNoTracking()
                .Where(t => !t.IsDelete && t.IsActive && pageWorkforceIds.Contains(t.WorkforceProfileId))
                .Select(t => t.WorkforceProfileId).Distinct().ToListAsync(cancellationToken)).ToHashSet();
        var insuranceIds = (await _dbContext.WfpInsurances.AsNoTracking()
                .Where(t => !t.IsDelete && t.IsActive && pageWorkforceIds.Contains(t.WorkforceProfileId))
                .Select(t => t.WorkforceProfileId).Distinct().ToListAsync(cancellationToken)).ToHashSet();

        var items = pageRows.Select(r => BuildItem(new PayrollCompensationListItemResponse(), r.Person, r.Profile,
            assignments.GetValueOrDefault(r.Profile.WorkforceProfileId) ?? new List<WfpSalaryAssignment>(),
            banks.GetValueOrDefault(r.Profile.WorkforceProfileId) ?? new List<WfpBankAccount>(),
            taxIds.Contains(r.Profile.WorkforceProfileId), insuranceIds.Contains(r.Profile.WorkforceProfileId),
            includeAmounts, today)).ToList();

        return new PagedResult<PayrollCompensationListItemResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = rows.Count,
            TotalPage = (int)Math.Ceiling(rows.Count / (double)pageSize),
            Items = items
        };
    }

    public async Task<PayrollCompensationDetailResponse> GetDetailAsync(
        Guid workforceProfileId, bool includeAmounts, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var profiles = await _dbContext.WfpPayrolls.AsNoTracking()
            .Where(x => !x.IsDelete && x.IsActive && x.WorkforceProfileId == workforceProfileId)
            .ToListAsync(cancellationToken);
        var profile = profiles
            .OrderByDescending(p => IsEffective(p.EffectiveStartDate, p.EffectiveEndDate, today))
            .ThenByDescending(p => p.EffectiveStartDate)
            .FirstOrDefault()
            ?? throw new KeyNotFoundException("Profil penggajian penerima tidak ditemukan.");

        var people = await LoadPeopleAsync(new List<Guid> { workforceProfileId }, cancellationToken);
        if (!people.TryGetValue(workforceProfileId, out var person))
            throw new KeyNotFoundException("Identitas karyawan/dokter pemilik profil penggajian tidak ditemukan di HRD.");

        var assignments = await _dbContext.WfpSalaryAssignments.AsNoTracking()
            .Include(a => a.SalaryStructure).Include(a => a.SalaryGrade)
            .Where(a => !a.IsDelete && a.WorkforceProfileId == workforceProfileId)
            .OrderByDescending(a => a.EffectiveStartDate)
            .ToListAsync(cancellationToken);
        var banks = await _dbContext.WfpBankAccounts.AsNoTracking()
            .Where(b => !b.IsDelete && b.IsActive && b.WorkforceProfileId == workforceProfileId)
            .ToListAsync(cancellationToken);
        var tax = await _dbContext.WfpTaxes.AsNoTracking()
            .Where(t => !t.IsDelete && t.IsActive && t.WorkforceProfileId == workforceProfileId)
            .OrderByDescending(t => t.EffectiveStartDate)
            .FirstOrDefaultAsync(cancellationToken);
        var hasInsurance = await _dbContext.WfpInsurances.AsNoTracking()
            .AnyAsync(t => !t.IsDelete && t.IsActive && t.WorkforceProfileId == workforceProfileId, cancellationToken);

        var detail = new PayrollCompensationDetailResponse();
        BuildItem(detail, person, profile, assignments, banks, tax is not null, hasInsurance, includeAmounts, today);
        detail.TaxStatus = tax?.TaxStatus;
        detail.IsNpwpRegistered = tax?.IsNpwpRegistered ?? false;
        detail.NpwpMasked = MaskTail(tax?.NpwpNumber, 5);
        detail.SalaryHistory = assignments.Select(a => new PayrollCompensationSalaryHistoryResponse
        {
            Id = a.Id,
            SalaryStructureName = a.SalaryStructure?.SalaryStructureName,
            SalaryGradeName = a.SalaryGrade?.SalaryGradeName,
            EffectiveStartDate = a.EffectiveStartDate,
            EffectiveEndDate = a.EffectiveEndDate,
            IsPrimary = a.IsPrimary,
            IsActive = a.IsActive,
            IsApproved = a.ApprovedAt.HasValue,
            ApprovedAt = a.ApprovedAt,
            BaseSalary = includeAmounts ? (decimal?)a.BaseSalary : null
        }).ToList();
        detail.BankAccounts = banks.Select(b => new PayrollCompensationBankResponse
        {
            Id = b.Id,
            BankName = b.BankName,
            AccountMasked = MaskTail(b.AccountNumber, 4) ?? string.Empty,
            AccountHolderName = b.AccountHolderName,
            IsPayrollAccount = b.IsPayrollAccount,
            IsPrimary = b.IsPrimary,
            IsVerified = b.IsVerified,
            VerifiedAt = b.VerifiedAt
        }).ToList();
        return detail;
    }

    private async Task<Dictionary<Guid, Person>> LoadPeopleAsync(List<Guid> workforceIds, CancellationToken cancellationToken)
    {
        var people = new Dictionary<Guid, Person>();
        var employees = await _dbContext.MstEmployees.AsNoTracking()
            .Where(x => !x.IsDelete && workforceIds.Contains(x.WorkforceProfileId))
            .Select(x => new { x.WorkforceProfileId, x.EmployeeNumber, x.FullName })
            .ToListAsync(cancellationToken);
        foreach (var e in employees) people.TryAdd(e.WorkforceProfileId, new Person(e.WorkforceProfileId, PersonEmployee, e.EmployeeNumber, e.FullName));
        var doctors = await _dbContext.MstDoctors.AsNoTracking()
            .Where(x => !x.IsDelete && workforceIds.Contains(x.WorkforceProfileId))
            .Select(x => new { x.WorkforceProfileId, x.DoctorNumber, x.FullName })
            .ToListAsync(cancellationToken);
        foreach (var d in doctors) people.TryAdd(d.WorkforceProfileId, new Person(d.WorkforceProfileId, PersonDoctor, d.DoctorNumber, d.FullName));
        return people;
    }

    private static T BuildItem<T>(
        T target, Person person, WfpPayroll profile, List<WfpSalaryAssignment> assignments, List<WfpBankAccount> banks,
        bool hasTax, bool hasInsurance, bool includeAmounts, DateTime today) where T : PayrollCompensationListItemResponse
    {
        var primaries = assignments.Where(a => !a.IsDelete && a.IsActive && a.IsPrimary)
            .OrderByDescending(a => a.EffectiveStartDate).ToList();
        var effectiveApproved = primaries
            .Where(a => a.ApprovedAt.HasValue && IsEffective(a.EffectiveStartDate, a.EffectiveEndDate, today)).ToList();
        var current = effectiveApproved.FirstOrDefault() ?? primaries.FirstOrDefault();

        var payrollBanks = banks.Where(b => b.IsPayrollAccount).ToList();
        string bankStatus = payrollBanks.Any(b => b.IsVerified) ? "Terverifikasi"
            : payrollBanks.Count > 0 ? "Belum terverifikasi"
            : "Belum ada";

        target.WorkforceProfileId = profile.WorkforceProfileId;
        target.PayrollProfileId = profile.Id;
        target.PersonType = person.Type;
        target.Number = person.Number;
        target.Name = person.Name;
        target.PayrollStatus = profile.PayrollStatus;
        target.IsPayrollEligible = profile.IsPayrollEligible;
        target.PayrollGroupCode = profile.PayrollGroupCode;
        target.SalaryAssignmentId = current?.Id;
        target.SalaryEffectiveStartDate = current?.EffectiveStartDate;
        target.SalaryEffectiveEndDate = current?.EffectiveEndDate;
        target.SalaryApproved = current?.ApprovedAt.HasValue ?? false;
        target.SalaryApprovedAt = current?.ApprovedAt;
        target.AmountsVisible = includeAmounts;
        target.BaseSalary = includeAmounts ? (decimal?)(current?.BaseSalary ?? profile.BaseSalary) : null;
        target.TotalAllowance = includeAmounts ? (decimal?)profile.TotalAllowance : null;
        target.BankStatus = bankStatus;
        target.HasTaxProfile = hasTax;
        target.HasInsuranceProfile = hasInsurance;

        var issues = new List<string>();
        if (!profile.IsPayrollEligible) issues.Add("Profil payroll tidak eligible.");
        if (!string.Equals(profile.PayrollStatus, "Active", StringComparison.OrdinalIgnoreCase))
            issues.Add($"Status payroll {profile.PayrollStatus}, bukan Active.");
        if (current is null) issues.Add("Belum ada penetapan gaji utama.");
        else if (!current.ApprovedAt.HasValue) issues.Add("Penetapan gaji utama belum disetujui.");
        else if (effectiveApproved.Count == 0) issues.Add("Penetapan gaji utama yang disetujui tidak berlaku hari ini.");
        if (effectiveApproved.Count > 1) issues.Add("Terdapat lebih dari satu penetapan gaji utama yang berlaku bersamaan.");
        if (bankStatus == "Belum ada") issues.Add("Belum ada rekening payroll.");
        else if (bankStatus == "Belum terverifikasi") issues.Add("Rekening payroll belum terverifikasi.");
        if (!hasTax) issues.Add("Profil pajak belum diisi.");
        target.Issues = issues;
        target.IsReady = issues.Count == 0;
        return target;
    }

    private static bool IsEffective(DateTime? start, DateTime? end, DateTime today) =>
        (!start.HasValue || start.Value.Date <= today) && (!end.HasValue || end.Value.Date >= today);

    private static bool IsEffective(DateTime start, DateTime? end, DateTime today) =>
        start.Date <= today && (!end.HasValue || end.Value.Date >= today);

    private static string? MaskTail(string? value, int visible)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var text = value.Trim();
        return text.Length <= visible ? new string('*', text.Length) : new string('*', text.Length - visible) + text[^visible..];
    }
}
