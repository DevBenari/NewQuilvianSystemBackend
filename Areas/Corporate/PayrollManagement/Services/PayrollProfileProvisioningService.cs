using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;

/// <summary>Hasil penyiapan profil payroll otomatis pada satu pemanggilan.</summary>
/// <param name="Created">Profil baru yang dibuat.</param>
/// <param name="AlreadyExisting">Orang yang sudah punya profil (tidak disentuh, apa pun statusnya).</param>
/// <param name="SkippedIncompleteHrd">Orang tanpa profil yang belum dapat disiapkan karena penetapan gaji atau rekening payroll belum ada di HRD.</param>
public sealed record PayrollProvisioningResult(int Created, int AlreadyExisting, int SkippedIncompleteHrd);

/// <summary>
/// Menyiapkan profil operasional payroll (PrlPayroll) secara otomatis dari data HRD, tanpa impor CSV. Profil dibutuhkan karena populasi
/// payroll run dibentuk dari profil payroll aktif (PayrollRunService.SeedEmployeesAsync).
///
/// Aturan:
/// - Sumber kebenaran tetap HRD: identitas, penetapan gaji utama, struktur dan grade, rekening payroll, dan penempatan organisasi
///   (Cost Center) hanya DIBACA. Persetujuan gaji dan verifikasi rekening tidak pernah diberikan di sini.
/// - Hanya MEMBUAT profil yang belum ada. Profil yang sudah ada tidak disentuh sama sekali: tidak ditimpa, tidak diaktifkan kembali
///   (Suspended, OnHold, Terminated tetap), tidak diperbarui referensinya. Indeks unik WorkforceProfileId (IsDelete = false) di database
///   menjamin tidak pernah ada dua profil aktif untuk satu orang.
/// - Orang yang penetapan gaji utamanya atau rekening payrollnya belum ada di HRD tidak dibuatkan profil: tidak ada gaji atau rekening
///   dummy. Mereka tampil sebagai kekurangan di Kelengkapan Data Payroll.
/// - Tetapan PPh dan iuran per orang tidak punya sumber di HRD dan rumus PPh/BPJS belum disahkan, sehingga keduanya dibiarkan nol dan
///   dianggap BELUM ditetapkan (PayrollProfileRules.IsTaxInsuranceUnset); mesin hitung dan Kelengkapan Data menandainya.
/// - Idempoten: pemanggilan ulang tidak membuat apa pun bila semua profil sudah ada.
///
/// Wajib dipanggil di dalam transaksi pemanggil (payroll run memakai transaksi Serializable dengan kunci advisory per periode atau run),
/// sehingga profil dan baris pegawai run tersimpan atau dibatalkan bersama. Layanan ini menyimpan profil baru sendiri supaya pembacaan
/// berikutnya dalam transaksi yang sama melihatnya. Tidak pernah dipanggil dari GET.
/// </summary>
public sealed class PayrollProfileProvisioningService
{
    private const string LogCategory = "Corporate.PayrollManagement.PayrollProfileProvisioning";

    private readonly ApplicationDbContext _dbContext;
    private readonly PayrollHrdSourceReader _hrdSource;
    private readonly LoggerService _loggerService;

    public PayrollProfileProvisioningService(
        ApplicationDbContext dbContext,
        PayrollHrdSourceReader hrdSource,
        LoggerService loggerService)
    {
        _dbContext = dbContext;
        _hrdSource = hrdSource;
        _loggerService = loggerService;
    }

    /// <param name="periodStart">Awal periode payroll yang sedang dipersiapkan; penetapan gaji dan penempatan dipilih yang berlaku pada periode ini.</param>
    /// <param name="periodEnd">Akhir periode payroll yang sedang dipersiapkan.</param>
    /// <param name="actorUserId">Pengguna yang memicu pembukaan atau perhitungan run.</param>
    /// <param name="trigger">Nama proses pemicu untuk jejak audit, mis. <c>PayrollRun.Create</c>.</param>
    public async Task<PayrollProvisioningResult> EnsureProfilesAsync(
        DateTime periodStart,
        DateTime periodEnd,
        Guid actorUserId,
        string trigger,
        CancellationToken cancellationToken)
    {
        if (_dbContext.Database.IsRelational() && _dbContext.Database.CurrentTransaction is null)
            throw new InvalidOperationException("Penyiapan profil payroll harus berjalan di dalam transaksi pemanggil.");

        var people = await _hrdSource.LoadPopulationAsync(cancellationToken);

        // Profil yang belum dihapus dalam keadaan apa pun menghalangi pembuatan baru: aktif, tidak aktif, Suspended, OnHold, Terminated.
        var existing = (await _dbContext.PrlPayrolls.AsNoTracking()
                .Where(x => !x.IsDelete)
                .Select(x => x.WorkforceProfileId)
                .ToListAsync(cancellationToken))
            .ToHashSet();

        var missing = people.Where(p => !existing.Contains(p.WorkforceProfileId)).ToList();
        var alreadyExisting = people.Count - missing.Count;
        if (missing.Count == 0) return new PayrollProvisioningResult(0, alreadyExisting, 0);

        var salaries = await _hrdSource.LoadSalariesAsync(periodStart, periodEnd, cancellationToken);
        var banks = await _hrdSource.LoadBanksAsync(periodEnd, cancellationToken);
        var organizations = await _hrdSource.LoadOrganizationsAsync(periodStart, periodEnd, cancellationToken);

        var now = DateTime.UtcNow;
        var created = new List<PrlPayroll>();
        var skipped = 0;
        foreach (var person in missing)
        {
            var hasSalary = salaries.TryGetValue(person.WorkforceProfileId, out var salary);
            var hasBank = banks.TryGetValue(person.WorkforceProfileId, out var bank);
            if (!PayrollProfileRules.CanAutoPrepare(hasSalary, hasBank) || salary is null || bank is null)
            {
                skipped++;
                continue;
            }

            organizations.TryGetValue(person.WorkforceProfileId, out var organization);

            created.Add(new PrlPayroll
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = person.WorkforceProfileId,
                EmployeeId = person.EmployeeId,
                OrganizationAssignmentId = organization?.OrganizationAssignmentId,
                SalaryAssignmentId = salary.AssignmentId,
                BankAccountId = bank.BankAccountId,
                CostCenterId = organization?.CostCenterId,
                SalaryStructureId = salary.SalaryStructureId,
                SalaryGradeId = salary.SalaryGradeId,
                PayrollStatus = PayrollProfileRules.ActiveStatus,
                CurrencyCode = "IDR",
                PaymentFrequency = "Monthly",
                PaymentMethod = "BankTransfer",
                IsPayrollEligible = true,
                IsConfidential = true,
                // timestamptz wajib berkind UTC; tengah malam UTC sama dengan tanggal berlaku penetapan gaji di HRD.
                EffectiveStartDate = DateTime.SpecifyKind(salary.EffectiveStartDate.Date, DateTimeKind.Utc),
                // Gaji dihitung dari penetapan gaji HRD; kolom gaji pada profil hanya kompatibilitas dan sengaja nol. TaxAmount dan
                // InsuranceAmount tetap nol = belum ditetapkan (tidak ada sumber sah untuk mengisinya).
                Description = PayrollProfileRules.AutoProvisionedDescription,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId
            });
        }

        if (created.Count > 0)
        {
            _dbContext.PrlPayrolls.AddRange(created);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Hanya hitungan dan pemicu: tanpa nama, nomor rekening, gaji, pajak, atau iuran.
            await _loggerService.AuditAsync(LogCategory, "PayrollProfile.AutoProvisioned",
                $"{created.Count} profil payroll disiapkan otomatis dari data HRD ({trigger}).",
                new
                {
                    Created = created.Count,
                    AlreadyExisting = alreadyExisting,
                    SkippedIncompleteHrd = skipped,
                    Trigger = trigger,
                    PeriodStart = periodStart,
                    PeriodEnd = periodEnd,
                    Status = "AutoProvisioned",
                    ActorUserId = actorUserId
                });
        }

        return new PayrollProvisioningResult(created.Count, alreadyExisting, skipped);
    }
}
