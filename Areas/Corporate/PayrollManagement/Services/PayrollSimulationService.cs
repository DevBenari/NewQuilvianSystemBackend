using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Options;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;

/// <summary>
/// PAY-BE-21 (blueprint payroll-gaji PGJ-C1, PRL-DEC-32): simulasi hitung gaji satu periode tanpa jejak.
///
/// Cara kerja: membuka transaksi database, membuat run sementara berjenis Simulation (nomor dari GUID, bukan number series)
/// beserta baris pegawai dengan aturan yang sama dengan run resmi, menjalankan <see cref="PayrollCalculationService"/> yang sama
/// persis, membaca hasilnya, lalu SELALU membatalkan transaksi (blok finally) dan mengosongkan change tracker.
///
/// Sengaja TIDAK dipanggil (keputusan pemilik 2026-11-12, karena bagian 5.5 arsitektur tidak dapat dipenuhi apa adanya):
/// - impor HR (kehadiran, cuti, lembur): ketiga service HR membuka transaksi sendiri dan mengunci absensi harian di HR, sehingga
///   meninggalkan jejak;
/// - sinkronisasi Finance dan potongan cicilan piutang (kriteria 4);
/// - NumberSeriesAllocator (kriteria 4).
/// Akibatnya simulasi menghitung gaji pokok, tunjangan struktur atau cadangan profil, transport bulanan, serta pajak dan iuran
/// tetapan profil; komponen dari impor HR, input variabel, dan cicilan tidak ada. Hal ini dinyatakan di Notes respons.
/// </summary>
public sealed class PayrollSimulationService
{
    private const string LogCategory = "Corporate.PayrollManagement.PayrollSimulation";
    private const string SimulationRunType = "Simulation";
    private const string EmployeeCalculated = "Calculated";

    private readonly ApplicationDbContext _dbContext;
    private readonly PayrollCalculationService _calculationService;
    private readonly LoggerService _loggerService;
    private readonly PayrollPolicyOptions _policy;

    public PayrollSimulationService(
        ApplicationDbContext dbContext,
        PayrollCalculationService calculationService,
        LoggerService loggerService,
        IOptions<PayrollPolicyOptions> policyOptions)
    {
        _dbContext = dbContext;
        _calculationService = calculationService;
        _loggerService = loggerService;
        _policy = policyOptions.Value;
    }

    private sealed record Candidate(PrlPayroll Profile, Guid? EmployeeId, string Number, string Name);

    public async Task<PayrollSimulationResponse> SimulateAsync(
        PayrollSimulationRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        // SIM-01
        var period = await _dbContext.MstPayrollPeriods.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == request.PayrollPeriodId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Periode penggajian tidak ditemukan.");

        // SIM-02
        var maxEmployees = Math.Max(1, _policy.SimulationMaxEmployees);
        var requestedIds = (request.WorkforceProfileIds ?? new List<Guid>()).Where(x => x != Guid.Empty).Distinct().ToList();
        if (requestedIds.Count > maxEmployees)
            throw new ArgumentException($"Simulasi paling banyak {maxEmployees} pegawai sekali jalan.");

        var notes = new List<string>
        {
            "Simulasi tidak mengimpor kehadiran, lembur, dan cuti dari HR, sehingga komponen dari sumber itu tidak muncul.",
            "Simulasi tidak memuat penghasilan variabel dan potongan cicilan piutang; potongan yang tampil adalah potongan sebelum cicilan.",
            "Hasil simulasi tidak disimpan."
        };

        var candidates = await LoadCandidatesAsync(period, requestedIds, notes, maxEmployees, cancellationToken);

        // SIM-03
        if (candidates.Count == 0)
            throw new ArgumentException("Tidak ada pegawai dengan penetapan gaji yang berlaku di periode ini.");

        if (_dbContext.Database.CurrentTransaction is not null)
            throw new InvalidOperationException("Simulasi payroll harus berjalan di transaksinya sendiri.");

        PayrollSimulationResponse response;
        await using (var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken))
        {
            try
            {
                response = await RunInsideTransactionAsync(period, candidates, actorUserId, cancellationToken);
            }
            finally
            {
                // Selalu dibatalkan, berhasil maupun gagal: tidak ada run, pegawai, atau komponen yang tersisa.
                await transaction.RollbackAsync(CancellationToken.None);
                _dbContext.ChangeTracker.Clear();
            }
        }

        response.PeriodCode = period.PayrollPeriodCode;
        response.PolicyVersion = _policy.PolicyVersion;
        response.Notes.AddRange(notes);

        await _loggerService.AuditAsync(LogCategory, "PayrollRun.Simulate",
            "Simulasi payroll dijalankan (transaksi dibatalkan, tidak ada data tersimpan).",
            new
            {
                PayrollPeriodId = period.Id,
                response.EmployeeCount,
                Calculated = response.Employees.Count(e => e.Errors.Count == 0),
                Errors = response.Employees.Count(e => e.Errors.Count > 0),
                Status = "Simulated",
                ActorUserId = actorUserId
            });

        return response;
    }

    /// <summary>Profil payroll layak hitung di periode (aturan sama dengan SeedEmployeesAsync run resmi) beserta identitasnya.</summary>
    private async Task<List<Candidate>> LoadCandidatesAsync(
        MstPayrollPeriod period, List<Guid> requestedIds, List<string> notes, int maxEmployees, CancellationToken cancellationToken)
    {
        var profilesQuery = _dbContext.PrlPayrolls.AsNoTracking()
            .Where(x => !x.IsDelete && x.IsActive && x.IsPayrollEligible && x.PayrollStatus == "Active"
                && (x.EffectiveStartDate == null || x.EffectiveStartDate <= period.EndDate)
                && (x.EffectiveEndDate == null || x.EffectiveEndDate >= period.StartDate));
        if (requestedIds.Count > 0)
            profilesQuery = profilesQuery.Where(x => requestedIds.Contains(x.WorkforceProfileId));

        var profiles = (await profilesQuery.OrderByDescending(x => x.EffectiveStartDate).ToListAsync(cancellationToken))
            .GroupBy(x => x.WorkforceProfileId).Select(g => g.First()).ToList();
        var workforceIds = profiles.Select(x => x.WorkforceProfileId).ToList();

        var employees = (await _dbContext.MstEmployees.AsNoTracking()
                .Where(x => !x.IsDelete && workforceIds.Contains(x.WorkforceProfileId))
                .Select(x => new { x.WorkforceProfileId, x.Id, x.EmployeeNumber, x.FullName })
                .ToListAsync(cancellationToken))
            .GroupBy(x => x.WorkforceProfileId).ToDictionary(g => g.Key, g => g.First());
        var doctors = (await _dbContext.MstDoctors.AsNoTracking()
                .Where(x => !x.IsDelete && workforceIds.Contains(x.WorkforceProfileId))
                .Select(x => new { x.WorkforceProfileId, x.DoctorNumber, x.FullName })
                .ToListAsync(cancellationToken))
            .GroupBy(x => x.WorkforceProfileId).ToDictionary(g => g.Key, g => g.First());

        var candidates = new List<Candidate>();
        var withoutIdentity = 0;
        foreach (var profile in profiles)
        {
            if (employees.TryGetValue(profile.WorkforceProfileId, out var emp))
                candidates.Add(new Candidate(profile, emp.Id, emp.EmployeeNumber, emp.FullName));
            else if (doctors.TryGetValue(profile.WorkforceProfileId, out var doc))
                candidates.Add(new Candidate(profile, null, doc.DoctorNumber, doc.FullName));
            else
                withoutIdentity++;
        }

        if (requestedIds.Count > 0 && profiles.Count < requestedIds.Count)
            notes.Add($"{requestedIds.Count - profiles.Count} pegawai yang dipilih tidak punya profil payroll aktif di periode ini dan dilewati.");
        if (withoutIdentity > 0)
            notes.Add($"{withoutIdentity} profil payroll dilewati karena identitas pegawai/dokter tidak ditemukan.");

        candidates = candidates.OrderBy(x => x.Name).ThenBy(x => x.Profile.WorkforceProfileId).ToList();
        if (candidates.Count > maxEmployees)
        {
            // Hanya terjadi bila pegawai tidak dipilih: "kosong = seluruh pegawai sampai batas" (api-contract.md).
            notes.Add($"Simulasi dibatasi {maxEmployees} pegawai pertama menurut nama dari {candidates.Count} pegawai layak hitung.");
            candidates = candidates.Take(maxEmployees).ToList();
        }

        return candidates;
    }

    private async Task<PayrollSimulationResponse> RunInsideTransactionAsync(
        MstPayrollPeriod period, List<Candidate> candidates, Guid actorUserId, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var run = new PrlPayrollRun
        {
            PayrollPeriodId = period.Id,
            LegalEntityId = period.LegalEntityId,
            HospitalSiteId = period.HospitalSiteId,
            RunNumber = $"SIM-{Guid.NewGuid():N}",
            RunType = SimulationRunType,
            RunStatus = PayrollRunStatuses.Draft,
            CurrencyCode = "IDR",
            PeriodStartDateSnapshot = period.StartDate,
            PeriodEndDateSnapshot = period.EndDate,
            AttendanceCutoffDateSnapshot = period.AttendanceCutoffDate,
            VariableInputCutoffDateSnapshot = period.VariableInputCutoffDate,
            PaymentDateSnapshot = period.PaymentDate,
            Notes = "Simulasi — transaksi selalu dibatalkan.",
            CreateDateTime = now,
            CreateBy = actorUserId
        };
        _dbContext.PrlPayrollRuns.Add(run);

        foreach (var candidate in candidates)
        {
            var profile = candidate.Profile;
            _dbContext.PrlPayrollRunEmployees.Add(new PrlPayrollRunEmployee
            {
                PayrollRunId = run.Id,
                WorkforceProfileId = profile.WorkforceProfileId,
                EmployeeId = candidate.EmployeeId,
                OrganizationAssignmentId = profile.OrganizationAssignmentId,
                PayrollProfileId = profile.Id,
                SalaryAssignmentId = profile.SalaryAssignmentId,
                BankAccountId = profile.BankAccountId,
                CostCenterId = profile.CostCenterId,
                SalaryStructureId = profile.SalaryStructureId,
                SalaryGradeId = profile.SalaryGradeId,
                EmployeePayrollStatus = "Pending",
                CurrencyCode = string.IsNullOrWhiteSpace(profile.CurrencyCode) ? "IDR" : profile.CurrencyCode,
                EmployeeNumberSnapshot = candidate.Number,
                EmployeeNameSnapshot = candidate.Name,
                CreateDateTime = now,
                CreateBy = actorUserId
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        // Mesin hitung yang sama dengan run resmi; ditulis ke transaksi ini lalu dibaca kembali.
        var calculation = await _calculationService.CalculateRunAsync(run, period, actorUserId, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var rows = await _dbContext.PrlPayrollRunEmployees.AsNoTracking()
            .Where(x => x.PayrollRunId == run.Id && !x.IsDelete)
            .Select(x => new
            {
                x.Id,
                x.WorkforceProfileId,
                x.EmployeeNumberSnapshot,
                x.EmployeeNameSnapshot,
                x.EmployeePayrollStatus,
                x.GrossPay,
                x.TotalDeduction,
                x.NetPay,
                Components = x.Components
                    .Where(c => !c.IsDelete)
                    .OrderBy(c => c.SortOrder)
                    .Select(c => new PayrollSimulationComponentResponse
                    {
                        Code = c.ComponentCodeSnapshot,
                        Name = c.ComponentNameSnapshot,
                        Type = c.ComponentTypeSnapshot,
                        Amount = c.Amount,
                        IsTaxable = c.IsTaxable
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        var response = new PayrollSimulationResponse { EmployeeCount = rows.Count };
        foreach (var row in rows.OrderBy(x => x.EmployeeNameSnapshot).ThenBy(x => x.WorkforceProfileId))
        {
            calculation.EmployeeWarnings.TryGetValue(row.Id, out var warnings);
            calculation.EmployeeErrors.TryGetValue(row.Id, out var errors);
            response.Employees.Add(new PayrollSimulationEmployeeResponse
            {
                WorkforceProfileId = row.WorkforceProfileId,
                EmployeeNumber = row.EmployeeNumberSnapshot ?? string.Empty,
                EmployeeName = row.EmployeeNameSnapshot,
                GrossPay = row.GrossPay,
                TotalDeduction = row.TotalDeduction,
                NetPay = row.NetPay,
                Components = row.Components,
                Warnings = warnings ?? new List<string>(),
                Errors = errors ?? new List<string>()
            });

            if (row.EmployeePayrollStatus == EmployeeCalculated)
            {
                response.TotalGrossPay += row.GrossPay;
                response.TotalDeduction += row.TotalDeduction;
                response.TotalNetPay += row.NetPay;
            }
        }

        return response;
    }
}
