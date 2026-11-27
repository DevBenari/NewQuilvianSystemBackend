using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Options;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;

/// <summary>Pegawai atau dokter aktif di master HRD. Satu orang satu baris menurut WorkforceProfileId.</summary>
public sealed record PayrollHrdPerson(Guid WorkforceProfileId, Guid? EmployeeId, string Type, string Number, string Name);

/// <summary>
/// Penetapan gaji utama HRD yang berlaku pada jendela tanggal. <see cref="AssignmentId"/> dan nilai turunannya milik penetapan yang
/// dipilih (yang sudah disetujui didahulukan, lalu tanggal mulai terbaru). <see cref="HasApproved"/> dan <see cref="HasPending"/>
/// menyatakan apakah ada penetapan yang disetujui dan/atau yang masih menunggu persetujuan pada jendela itu.
/// <see cref="ApprovedCount"/> adalah jumlah penetapan utama yang DISETUJUI dan berlaku bersamaan pada jendela; lebih dari satu berarti
/// konflik data HRD (dua gaji utama berlaku sekaligus), yang tidak boleh dipilih diam-diam.
/// </summary>
public sealed record PayrollHrdSalary(
    Guid AssignmentId,
    Guid SalaryStructureId,
    Guid SalaryGradeId,
    DateTime EffectiveStartDate,
    bool HasApproved,
    bool HasPending,
    int ApprovedCount = 0);

/// <summary>Rekening payroll aktif HRD yang berlaku. <see cref="HasVerified"/> benar bila ada rekening yang sudah diverifikasi HRD.</summary>
public sealed record PayrollHrdBank(Guid BankAccountId, bool HasVerified);

/// <summary>Penempatan organisasi primer aktif HRD yang berlaku; <see cref="CostCenterId"/> kosong bila HRD belum mengisinya.</summary>
public sealed record PayrollHrdOrganization(Guid OrganizationAssignmentId, Guid? CostCenterId);

/// <summary>
/// Aturan profil payroll yang dipakai bersama oleh kelengkapan data, penyiapan profil otomatis, dan mesin hitung, supaya satu
/// aturan tidak ditulis dua kali. Tidak ada akses data di sini.
/// </summary>
public static class PayrollProfileRules
{
    public const string ActiveStatus = "Active";

    /// <summary>Penanda asal pada <c>PrlPayroll.Description</c> untuk profil yang disiapkan otomatis (kolom itu sudah ada; tanpa migration).</summary>
    public const string AutoProvisionedDescription = "Disiapkan otomatis dari data HRD saat payroll run dipersiapkan.";

    /// <summary>Profil ikut payroll run: aktif, eligible, dan berstatus Active (sama dengan SeedEmployeesAsync).</summary>
    public static bool IsOperational(bool isActive, bool isPayrollEligible, string? payrollStatus) =>
        isActive && isPayrollEligible && string.Equals(payrollStatus, ActiveStatus, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Profil baru dapat dibuat otomatis hanya bila HRD sudah punya penetapan gaji utama yang berlaku dan rekening payroll aktif
    /// (persetujuan dan verifikasi tidak disyaratkan di sini: penetapan belum disetujui tetap menahan hitung, rekening belum
    /// terverifikasi tetap menahan pembayaran). Tanpa keduanya, tidak ada data dummy yang dibuat.
    /// </summary>
    public static bool CanAutoPrepare(bool hasSalaryAssignment, bool hasBankAccount) => hasSalaryAssignment && hasBankAccount;

    // Penentu kelengkapan PPh dan iuran BUKAN lagi "TaxAmount == 0 && InsuranceAmount == 0": nol bisa merupakan hasil yang sah. Status
    // dinilai eksplisit dari konfigurasi HRD (PrlTax, PrlInsurance) oleh PayrollTaxInsuranceRules.
}

/// <summary>
/// Pembaca sumber HRD untuk Payroll: populasi pegawai dan dokter aktif, penetapan gaji utama, rekening payroll, dan penempatan
/// organisasi (Cost Center). Hanya membaca (AsNoTracking). Dipakai kelengkapan data (jendela = hari ini) dan penyiapan profil otomatis
/// (jendela = periode payroll), sehingga keduanya memakai pilihan sumber yang sama.
/// </summary>
public sealed class PayrollHrdSourceReader
{
    private readonly ApplicationDbContext _dbContext;
    private readonly PayrollPolicyOptions _policy;

    /// <param name="policyOptions">Opsional supaya pemakai lama (termasuk tes) yang hanya memberi DbContext tetap berjalan; tanpa opsi, bawaan kebijakan dipakai.</param>
    public PayrollHrdSourceReader(ApplicationDbContext dbContext, IOptions<PayrollPolicyOptions>? policyOptions = null)
    {
        _dbContext = dbContext;
        _policy = policyOptions?.Value ?? new PayrollPolicyOptions();
    }

    /// <summary>Pegawai lalu dokter aktif; WorkforceProfileId yang sama tidak digandakan.</summary>
    public Task<List<PayrollHrdPerson>> LoadPopulationAsync(CancellationToken cancellationToken) =>
        LoadPopulationAsync(null, null, cancellationToken);

    /// <summary>
    /// Populasi aktif dengan penyaringan identitas dilakukan di database: <paramref name="personType"/> ("Karyawan" atau "Dokter")
    /// dan <paramref name="search"/> (nama atau nomor, tanpa membedakan huruf besar kecil). Hanya kolom identitas yang dibaca.
    /// Orang yang aktif sebagai pegawai sekaligus dokter tetap dilabeli Karyawan (pegawai didahulukan), baik populasi disaring
    /// maupun tidak.
    /// </summary>
    public async Task<List<PayrollHrdPerson>> LoadPopulationAsync(
        string? personType, string? search, CancellationToken cancellationToken)
    {
        var wantEmployees = string.IsNullOrWhiteSpace(personType)
            || string.Equals(personType.Trim(), "Karyawan", StringComparison.OrdinalIgnoreCase);
        var wantDoctors = string.IsNullOrWhiteSpace(personType)
            || string.Equals(personType.Trim(), "Dokter", StringComparison.OrdinalIgnoreCase);
        var term = string.IsNullOrWhiteSpace(search) ? null : search.Trim().ToUpper();

        var seen = new HashSet<Guid>();
        var people = new List<PayrollHrdPerson>();

        var employeeQuery = _dbContext.MstEmployees.AsNoTracking().Where(x => !x.IsDelete && x.IsActive);
        if (wantEmployees)
        {
            var filtered = term is null
                ? employeeQuery
                : employeeQuery.Where(x => x.FullName.ToUpper().Contains(term) || x.EmployeeNumber.ToUpper().Contains(term));
            var employees = await filtered
                .Select(x => new { x.Id, x.WorkforceProfileId, x.EmployeeNumber, x.FullName })
                .ToListAsync(cancellationToken);
            foreach (var e in employees)
                if (seen.Add(e.WorkforceProfileId))
                    people.Add(new PayrollHrdPerson(e.WorkforceProfileId, e.Id, "Karyawan", e.EmployeeNumber, e.FullName));
        }
        else
        {
            // Hanya dokter yang diminta: pegawai aktif tetap dibaca (hanya id) supaya dokter yang juga pegawai tidak muncul sebagai Dokter.
            var employeeIds = await employeeQuery.Select(x => x.WorkforceProfileId).ToListAsync(cancellationToken);
            foreach (var id in employeeIds) seen.Add(id);
        }

        if (wantDoctors)
        {
            var doctorQuery = _dbContext.MstDoctors.AsNoTracking().Where(x => !x.IsDelete && x.IsActive);
            var filtered = term is null
                ? doctorQuery
                : doctorQuery.Where(x => x.FullName.ToUpper().Contains(term) || x.DoctorNumber.ToUpper().Contains(term));
            var doctors = await filtered
                .Select(x => new { x.WorkforceProfileId, x.DoctorNumber, x.FullName })
                .ToListAsync(cancellationToken);
            foreach (var d in doctors)
                if (seen.Add(d.WorkforceProfileId))
                    people.Add(new PayrollHrdPerson(d.WorkforceProfileId, null, "Dokter", d.DoctorNumber, d.FullName));
        }

        return people;
    }

    /// <summary>
    /// Penetapan gaji utama aktif yang berlaku pada jendela [windowStart, windowEnd], satu hasil per orang. Bila
    /// <paramref name="workforceProfileIds"/> diberikan, hanya orang-orang itu yang dibaca.
    /// </summary>
    public async Task<Dictionary<Guid, PayrollHrdSalary>> LoadSalariesAsync(
        DateTime windowStart, DateTime windowEnd, CancellationToken cancellationToken,
        IReadOnlyCollection<Guid>? workforceProfileIds = null)
    {
        var salaryQuery = _dbContext.WfpSalaryAssignments.AsNoTracking()
            .Where(x => !x.IsDelete && x.IsActive && x.IsPrimary
                && x.EffectiveStartDate <= windowEnd
                && (x.EffectiveEndDate == null || x.EffectiveEndDate >= windowStart));
        if (workforceProfileIds is not null)
            salaryQuery = salaryQuery.Where(x => workforceProfileIds.Contains(x.WorkforceProfileId));

        var rows = await salaryQuery
            .Select(x => new
            {
                x.Id,
                x.WorkforceProfileId,
                x.SalaryStructureId,
                x.SalaryGradeId,
                x.EffectiveStartDate,
                x.IsPrimary,
                // Aturan tunggal "disetujui" (WfpSalaryAssignmentRules.IsApprovedExpression): kedua kolom persetujuan terisi.
                Approved = x.ApprovedAt != null && x.ApprovedByUserId != null
            })
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(x => x.WorkforceProfileId)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var primaries = g.Where(a => a.IsPrimary).ToList();
                    var pool = primaries.Count > 0 ? primaries : g.ToList();
                    var approved = pool.Where(a => a.Approved).ToList();
                    // Yang disetujui didahulukan; bila hanya ada yang belum disetujui, baris itu tetap dikembalikan supaya
                    // pemanggil dapat menyatakan "menunggu persetujuan" (bukan "belum ada") dan tidak memperlakukannya sebagai gaji sah.
                    var chosen = (approved.Count > 0 ? approved : pool)
                        .OrderByDescending(a => a.EffectiveStartDate)
                        .ThenBy(a => a.Id)
                        .First();
                    // Sakelar RequireApprovedSalaryAssignment mati (bawaan): setiap penetapan utama aktif yang efektif dianggap sah tanpa
                    // approval (keputusan "peniadaan approval" 19 Nov 2026), jadi tidak ada yang berstatus menunggu dan semuanya dihitung.
                    // Menyala: hanya yang disetujui sah, yang lain menunggu persetujuan.
                    if (!_policy.RequireApprovedSalaryAssignment)
                        return new PayrollHrdSalary(
                            chosen.Id, chosen.SalaryStructureId, chosen.SalaryGradeId, chosen.EffectiveStartDate,
                            HasApproved: true,
                            HasPending: false,
                            ApprovedCount: pool.Count);

                    return new PayrollHrdSalary(
                        chosen.Id, chosen.SalaryStructureId, chosen.SalaryGradeId, chosen.EffectiveStartDate,
                        HasApproved: approved.Count > 0,
                        HasPending: pool.Any(a => !a.Approved),
                        ApprovedCount: approved.Count);
                });
    }

    /// <summary>
    /// Rekening payroll aktif yang berlaku pada <paramref name="asOf"/>, satu hasil per orang: yang terverifikasi didahulukan, lalu
    /// rekening utama, lalu yang terbaru (urutan sama dengan pemilih rekening mesin hitung). Bila
    /// <paramref name="workforceProfileIds"/> diberikan, hanya orang-orang itu yang dibaca.
    /// </summary>
    public async Task<Dictionary<Guid, PayrollHrdBank>> LoadBanksAsync(
        DateTime asOf, CancellationToken cancellationToken, IReadOnlyCollection<Guid>? workforceProfileIds = null)
    {
        var bankQuery = _dbContext.WfpBankAccounts.AsNoTracking()
            .Where(x => !x.IsDelete && x.IsActive && x.IsPayrollAccount
                && (x.EffectiveStartDate == null || x.EffectiveStartDate <= asOf)
                && (x.EffectiveEndDate == null || x.EffectiveEndDate >= asOf));
        if (workforceProfileIds is not null)
            bankQuery = bankQuery.Where(x => workforceProfileIds.Contains(x.WorkforceProfileId));

        var rows = await bankQuery
            .Select(x => new { x.Id, x.WorkforceProfileId, x.IsVerified, x.IsPrimary, x.CreateDateTime })
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(x => x.WorkforceProfileId)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var chosen = g.OrderByDescending(b => b.IsVerified)
                        .ThenByDescending(b => b.IsPrimary)
                        .ThenByDescending(b => b.CreateDateTime)
                        .ThenBy(b => b.Id)
                        .First();
                    return new PayrollHrdBank(chosen.Id, HasVerified: g.Any(b => b.IsVerified));
                });
    }

    /// <summary>
    /// Penempatan organisasi primer aktif yang berlaku pada jendela, satu hasil per orang (tanggal mulai terbaru). Cost Center hanya
    /// dipakai bila masih aktif di master; selain itu kosong, tidak dikarang.
    /// </summary>
    public async Task<Dictionary<Guid, PayrollHrdOrganization>> LoadOrganizationsAsync(
        DateTime windowStart, DateTime windowEnd, CancellationToken cancellationToken)
    {
        var rows = await _dbContext.WfpOrganizationAssignments.AsNoTracking()
            .Where(x => !x.IsDelete && x.IsActive && x.IsPrimary
                && x.EffectiveStartDate <= windowEnd
                && (x.EffectiveEndDate == null || x.EffectiveEndDate >= windowStart))
            .Select(x => new { x.Id, x.WorkforceProfileId, x.CostCenterId, x.EffectiveStartDate })
            .ToListAsync(cancellationToken);
        if (rows.Count == 0) return new Dictionary<Guid, PayrollHrdOrganization>();

        var costCenterIds = rows.Where(x => x.CostCenterId.HasValue).Select(x => x.CostCenterId!.Value).Distinct().ToList();
        var activeCostCenters = costCenterIds.Count == 0
            ? new HashSet<Guid>()
            : (await _dbContext.MstCostCenters.AsNoTracking()
                    .Where(x => !x.IsDelete && x.IsActive && costCenterIds.Contains(x.Id))
                    .Select(x => x.Id)
                    .ToListAsync(cancellationToken))
                .ToHashSet();

        return rows
            .GroupBy(x => x.WorkforceProfileId)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var chosen = g.OrderByDescending(a => a.EffectiveStartDate).ThenBy(a => a.Id).First();
                    var costCenter = chosen.CostCenterId.HasValue && activeCostCenters.Contains(chosen.CostCenterId.Value)
                        ? chosen.CostCenterId
                        : null;
                    return new PayrollHrdOrganization(chosen.Id, costCenter);
                });
    }
}
