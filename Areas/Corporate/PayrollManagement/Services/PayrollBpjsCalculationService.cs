using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Repositories;
using System.Text.Json;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Services
{
    public class PayrollBpjsCalculationService : IPayrollBpjsCalculationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPayrollSettingService _settingService;
        private readonly ILogger<PayrollBpjsCalculationService> _logger;

        public PayrollBpjsCalculationService(
            ApplicationDbContext context,
            IPayrollSettingService settingService,
            ILogger<PayrollBpjsCalculationService> logger)
        {
            _context = context;
            _settingService = settingService;
            _logger = logger;
        }

        public async Task<PayrollServiceResult<CalculateBpjsResultDto>> CalculateBpjsContributionsAsync(
            Guid runId,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Memulai kalkulasi iuran BPJS Kesehatan dan BPJS Ketenagakerjaan untuk batch penggajian ID: {RunId}. Actor: {ActorId}",
                runId, actorUserId);

            var run = await _context.PrlPayrollRuns
                .Include(r => r.Employees.Where(e => e.IsActive))
                    .ThenInclude(e => e.Components.Where(c => c.IsActive))
                .FirstOrDefaultAsync(r => r.Id == runId && r.IsActive, cancellationToken);

            if (run == null)
            {
                return PayrollServiceResult<CalculateBpjsResultDto>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Batch penggajian dengan ID {runId} tidak ditemukan.");
            }

            if (run.IsLocked || run.RunStatus is "Approved" or "PaymentProcessing" or "Paid" or "Posted" or "Closed")
            {
                return PayrollServiceResult<CalculateBpjsResultDto>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    $"Batch penggajian {run.RunNumber} telah berstatus '{run.RunStatus}' (Locked={run.IsLocked}) dan tidak dapat dimodifikasi.");
            }

            var setting = await ResolveEffectiveSettingAsync(run.ConfigurationSnapshotJson, cancellationToken);

            var employees = run.Employees.ToList();
            var details = new List<EmployeeBpjsCalculationDto>();

            foreach (var emp in employees)
            {
                var dto = ProcessEmployeeBpjs(emp, setting, actorUserId);
                details.Add(dto);
            }

            // Agregasi Ringkasan Run
            run.TotalEmployeeContribution = employees.Sum(e => e.TotalEmployeeInsuranceContribution);
            run.TotalEmployerContribution = employees.Sum(e => e.TotalEmployerInsuranceContribution);
            run.TotalDeduction = employees.Sum(e => e.TotalDeduction);
            run.TotalNetPay = employees.Sum(e => e.NetPay);
            run.UpdateDateTime = DateTime.UtcNow;
            run.UpdateBy = actorUserId;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Kalkulasi BPJS selesai untuk {Count} pegawai pada batch {RunNumber}. Total Potongan BPJS Pegawai: {TotalEE:N2}, Total Beban RS: {TotalER:N2}",
                employees.Count, run.RunNumber, run.TotalEmployeeContribution, run.TotalEmployerContribution);

            return PayrollServiceResult<CalculateBpjsResultDto>.Ok(
                new CalculateBpjsResultDto
                {
                    RunId = run.Id,
                    RunNumber = run.RunNumber,
                    TotalEmployeesProcessed = employees.Count,
                    TotalEmployeeBpjsContribution = run.TotalEmployeeContribution,
                    TotalEmployerBpjsContribution = run.TotalEmployerContribution,
                    TotalBpjsKesehatan = details.Sum(d => d.BpjsKesehatanEmployee + d.BpjsKesehatanEmployer),
                    TotalBpjsKetenagakerjaan = details.Sum(d => d.BpjsJkkEmployer + d.BpjsJkmEmployer + d.BpjsJhtEmployee + d.BpjsJhtEmployer + d.BpjsJpEmployee + d.BpjsJpEmployer),
                    Message = $"Kalkulasi iuran jaminan sosial BPJS untuk {employees.Count} pegawai berhasil diproses.",
                    Details = details
                },
                "Kalkulasi iuran jaminan sosial BPJS berhasil.");
        }

        public async Task<PayrollServiceResult<EmployeeBpjsCalculationDto>> CalculateEmployeeBpjsAsync(
            Guid runId,
            Guid employeeRunId,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var emp = await _context.PrlPayrollRunEmployees
                .Include(e => e.Components.Where(c => c.IsActive))
                .Include(e => e.PayrollRun)
                .FirstOrDefaultAsync(e => e.Id == employeeRunId && e.PayrollRunId == runId && e.IsActive, cancellationToken);

            if (emp == null || emp.PayrollRun == null)
            {
                return PayrollServiceResult<EmployeeBpjsCalculationDto>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Data pegawai penggajian ID {employeeRunId} pada batch {runId} tidak ditemukan.");
            }

            if (emp.PayrollRun.IsLocked || emp.PayrollRun.RunStatus is "Approved" or "Paid" or "Closed")
            {
                return PayrollServiceResult<EmployeeBpjsCalculationDto>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    $"Batch penggajian telah berstatus final dan tidak dapat diperbarui.");
            }

            var setting = await ResolveEffectiveSettingAsync(emp.PayrollRun.ConfigurationSnapshotJson, cancellationToken);
            var result = ProcessEmployeeBpjs(emp, setting, actorUserId);

            await _context.SaveChangesAsync(cancellationToken);

            return PayrollServiceResult<EmployeeBpjsCalculationDto>.Ok(
                result,
                $"Kalkulasi iuran BPJS untuk {emp.EmployeeNameSnapshot} berhasil.");
        }

        private async Task<HrdPayrollSettingDto> ResolveEffectiveSettingAsync(
            string? snapshotJson,
            CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(snapshotJson))
            {
                try
                {
                    var snapshot = JsonSerializer.Deserialize<HrdPayrollSettingDto>(snapshotJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    if (snapshot != null) return snapshot;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Gagal mem-parsing snapshot setting JSON. Menggunakan setting aktif saat ini.");
                }
            }

            return await _settingService.GetSettingAsync(cancellationToken);
        }

        private EmployeeBpjsCalculationDto ProcessEmployeeBpjs(
            PrlPayrollRunEmployee emp,
            HrdPayrollSettingDto setting,
            Guid actorUserId)
        {
            // 1. Bersihkan komponen asuransi BPJS lama
            var existingInsuranceComponents = emp.Components
                .Where(c => c.SourceType == "Insurance")
                .ToList();

            foreach (var comp in existingInsuranceComponents)
            {
                _context.PrlPayrollEmployeeComponents.Remove(comp);
            }

            // 2. Tentukan Dasar Upah Pengenaan BPJS (Gaji Pokok + Tunjangan Tetap)
            var baseWage = emp.BaseSalary + emp.TotalRecurringEarning;

            // BPJS Kesehatan (Batas Plafon Upah)
            var bpjsKesBase = Math.Min(baseWage, setting.BpjsKesehatanCapAmount);
            var bpjsKesEE = Math.Round(bpjsKesBase * (setting.BpjsKesehatanEmployeeRate / 100m), 2, MidpointRounding.AwayFromZero);
            var bpjsKesER = Math.Round(bpjsKesBase * (setting.BpjsKesehatanEmployerRate / 100m), 2, MidpointRounding.AwayFromZero);

            // BPJS Ketenagakerjaan: JKK (Beban RS 100%)
            var bpjsJkkER = Math.Round(baseWage * (setting.BpjsJkkRate / 100m), 2, MidpointRounding.AwayFromZero);

            // BPJS Ketenagakerjaan: JKM (Beban RS 100%)
            var bpjsJkmER = Math.Round(baseWage * (setting.BpjsJkmRate / 100m), 2, MidpointRounding.AwayFromZero);

            // BPJS Ketenagakerjaan: JHT (Pekerja 2%, RS 3.7%) - Tanpa Plafon
            var bpjsJhtEE = Math.Round(baseWage * (setting.BpjsJhtEmployeeRate / 100m), 2, MidpointRounding.AwayFromZero);
            var bpjsJhtER = Math.Round(baseWage * (setting.BpjsJhtEmployerRate / 100m), 2, MidpointRounding.AwayFromZero);

            // BPJS Ketenagakerjaan: JP (Pekerja 1%, RS 2%) - Berplafon Resmi
            var bpjsJpBase = Math.Min(baseWage, setting.BpjsJpCapAmount);
            var bpjsJpEE = Math.Round(bpjsJpBase * (setting.BpjsJpEmployeeRate / 100m), 2, MidpointRounding.AwayFromZero);
            var bpjsJpER = Math.Round(bpjsJpBase * (setting.BpjsJpEmployerRate / 100m), 2, MidpointRounding.AwayFromZero);

            var newComponents = new List<PrlPayrollEmployeeComponent>();

            // Komponen: Potongan BPJS Kesehatan Pekerja (1%)
            if (bpjsKesEE > 0)
            {
                newComponents.Add(new PrlPayrollEmployeeComponent
                {
                    Id = Guid.NewGuid(),
                    PayrollRunEmployeeId = emp.Id,
                    SourceType = "Insurance",
                    ComponentCodeSnapshot = "DED_BPJS_KES",
                    ComponentNameSnapshot = "Iuran BPJS Kesehatan (1% Pekerja)",
                    ComponentTypeSnapshot = "Deduction",
                    CalculationMethodSnapshot = "Percentage",
                    Percentage = setting.BpjsKesehatanEmployeeRate,
                    BaseAmount = bpjsKesBase,
                    Amount = bpjsKesEE,
                    CurrencyCode = "IDR",
                    IsTaxable = false,
                    IsDisplayedOnPayslip = true,
                    SortOrder = 10,
                    IsActive = true,
                    CreateBy = actorUserId,
                    CreateDateTime = DateTime.UtcNow
                });
            }

            // Komponen: Potongan BPJS Ketenagakerjaan JHT Pekerja (2%)
            if (bpjsJhtEE > 0)
            {
                newComponents.Add(new PrlPayrollEmployeeComponent
                {
                    Id = Guid.NewGuid(),
                    PayrollRunEmployeeId = emp.Id,
                    SourceType = "Insurance",
                    ComponentCodeSnapshot = "DED_BPJS_JHT",
                    ComponentNameSnapshot = "Iuran BPJS TK JHT (2% Pekerja)",
                    ComponentTypeSnapshot = "Deduction",
                    CalculationMethodSnapshot = "Percentage",
                    Percentage = setting.BpjsJhtEmployeeRate,
                    BaseAmount = baseWage,
                    Amount = bpjsJhtEE,
                    CurrencyCode = "IDR",
                    IsTaxable = false,
                    IsDisplayedOnPayslip = true,
                    SortOrder = 11,
                    IsActive = true,
                    CreateBy = actorUserId,
                    CreateDateTime = DateTime.UtcNow
                });
            }

            // Komponen: Potongan BPJS Ketenagakerjaan JP Pekerja (1%)
            if (bpjsJpEE > 0)
            {
                newComponents.Add(new PrlPayrollEmployeeComponent
                {
                    Id = Guid.NewGuid(),
                    PayrollRunEmployeeId = emp.Id,
                    SourceType = "Insurance",
                    ComponentCodeSnapshot = "DED_BPJS_JP",
                    ComponentNameSnapshot = "Iuran BPJS TK JP (1% Pekerja)",
                    ComponentTypeSnapshot = "Deduction",
                    CalculationMethodSnapshot = "Percentage",
                    Percentage = setting.BpjsJpEmployeeRate,
                    BaseAmount = bpjsJpBase,
                    Amount = bpjsJpEE,
                    CurrencyCode = "IDR",
                    IsTaxable = false,
                    IsDisplayedOnPayslip = true,
                    SortOrder = 12,
                    IsActive = true,
                    CreateBy = actorUserId,
                    CreateDateTime = DateTime.UtcNow
                });
            }

            // Komponen Kontribusi RS: BPJS Kesehatan (4%)
            if (bpjsKesER > 0)
            {
                newComponents.Add(new PrlPayrollEmployeeComponent
                {
                    Id = Guid.NewGuid(),
                    PayrollRunEmployeeId = emp.Id,
                    SourceType = "Insurance",
                    ComponentCodeSnapshot = "ER_BPJS_KES",
                    ComponentNameSnapshot = "Kontribusi BPJS Kesehatan RS (4%)",
                    ComponentTypeSnapshot = "EmployerContribution",
                    CalculationMethodSnapshot = "Percentage",
                    Percentage = setting.BpjsKesehatanEmployerRate,
                    BaseAmount = bpjsKesBase,
                    Amount = 0m, // Tidak memotong pegawai
                    EmployerAmount = bpjsKesER,
                    CurrencyCode = "IDR",
                    IsDisplayedOnPayslip = false,
                    SortOrder = 20,
                    IsActive = true,
                    CreateBy = actorUserId,
                    CreateDateTime = DateTime.UtcNow
                });
            }

            // Komponen Kontribusi RS: BPJS TK JKK (0.89%)
            if (bpjsJkkER > 0)
            {
                newComponents.Add(new PrlPayrollEmployeeComponent
                {
                    Id = Guid.NewGuid(),
                    PayrollRunEmployeeId = emp.Id,
                    SourceType = "Insurance",
                    ComponentCodeSnapshot = "ER_BPJS_JKK",
                    ComponentNameSnapshot = "Kontribusi BPJS TK JKK RS (0.89%)",
                    ComponentTypeSnapshot = "EmployerContribution",
                    CalculationMethodSnapshot = "Percentage",
                    Percentage = setting.BpjsJkkRate,
                    BaseAmount = baseWage,
                    Amount = 0m,
                    EmployerAmount = bpjsJkkER,
                    CurrencyCode = "IDR",
                    IsDisplayedOnPayslip = false,
                    SortOrder = 21,
                    IsActive = true,
                    CreateBy = actorUserId,
                    CreateDateTime = DateTime.UtcNow
                });
            }

            // Komponen Kontribusi RS: BPJS TK JKM (0.30%)
            if (bpjsJkmER > 0)
            {
                newComponents.Add(new PrlPayrollEmployeeComponent
                {
                    Id = Guid.NewGuid(),
                    PayrollRunEmployeeId = emp.Id,
                    SourceType = "Insurance",
                    ComponentCodeSnapshot = "ER_BPJS_JKM",
                    ComponentNameSnapshot = "Kontribusi BPJS TK JKM RS (0.30%)",
                    ComponentTypeSnapshot = "EmployerContribution",
                    CalculationMethodSnapshot = "Percentage",
                    Percentage = setting.BpjsJkmRate,
                    BaseAmount = baseWage,
                    Amount = 0m,
                    EmployerAmount = bpjsJkmER,
                    CurrencyCode = "IDR",
                    IsDisplayedOnPayslip = false,
                    SortOrder = 22,
                    IsActive = true,
                    CreateBy = actorUserId,
                    CreateDateTime = DateTime.UtcNow
                });
            }

            // Komponen Kontribusi RS: BPJS TK JHT (3.70%)
            if (bpjsJhtER > 0)
            {
                newComponents.Add(new PrlPayrollEmployeeComponent
                {
                    Id = Guid.NewGuid(),
                    PayrollRunEmployeeId = emp.Id,
                    SourceType = "Insurance",
                    ComponentCodeSnapshot = "ER_BPJS_JHT",
                    ComponentNameSnapshot = "Kontribusi BPJS TK JHT RS (3.70%)",
                    ComponentTypeSnapshot = "EmployerContribution",
                    CalculationMethodSnapshot = "Percentage",
                    Percentage = setting.BpjsJhtEmployerRate,
                    BaseAmount = baseWage,
                    Amount = 0m,
                    EmployerAmount = bpjsJhtER,
                    CurrencyCode = "IDR",
                    IsDisplayedOnPayslip = false,
                    SortOrder = 23,
                    IsActive = true,
                    CreateBy = actorUserId,
                    CreateDateTime = DateTime.UtcNow
                });
            }

            // Komponen Kontribusi RS: BPJS TK JP (2.00%)
            if (bpjsJpER > 0)
            {
                newComponents.Add(new PrlPayrollEmployeeComponent
                {
                    Id = Guid.NewGuid(),
                    PayrollRunEmployeeId = emp.Id,
                    SourceType = "Insurance",
                    ComponentCodeSnapshot = "ER_BPJS_JP",
                    ComponentNameSnapshot = "Kontribusi BPJS TK JP RS (2.00%)",
                    ComponentTypeSnapshot = "EmployerContribution",
                    CalculationMethodSnapshot = "Percentage",
                    Percentage = setting.BpjsJpEmployerRate,
                    BaseAmount = bpjsJpBase,
                    Amount = 0m,
                    EmployerAmount = bpjsJpER,
                    CurrencyCode = "IDR",
                    IsDisplayedOnPayslip = false,
                    SortOrder = 24,
                    IsActive = true,
                    CreateBy = actorUserId,
                    CreateDateTime = DateTime.UtcNow
                });
            }

            _context.PrlPayrollEmployeeComponents.AddRange(newComponents);

            // 3. Update Saldo Pemotongan Pegawai
            emp.TotalEmployeeInsuranceContribution = bpjsKesEE + bpjsJhtEE + bpjsJpEE;
            emp.TotalEmployerInsuranceContribution = bpjsKesER + bpjsJkkER + bpjsJkmER + bpjsJhtER + bpjsJpER;
            emp.TotalDeduction = emp.TotalEmployeeInsuranceContribution; // jika ada potongan lain di slice B6 akan diakumulasi
            emp.NetPay = emp.GrossPay - emp.TotalDeduction - emp.TotalTax;
            emp.UpdateDateTime = DateTime.UtcNow;
            emp.UpdateBy = actorUserId;

            return new EmployeeBpjsCalculationDto
            {
                PayrollRunEmployeeId = emp.Id,
                WorkforceProfileId = emp.WorkforceProfileId,
                EmployeeName = emp.EmployeeNameSnapshot,
                CalculationBaseWage = baseWage,
                BpjsKesehatanBaseWage = bpjsKesBase,
                BpjsJpBaseWage = bpjsJpBase,
                BpjsKesehatanEmployee = bpjsKesEE,
                BpjsKesehatanEmployer = bpjsKesER,
                BpjsJkkEmployer = bpjsJkkER,
                BpjsJkmEmployer = bpjsJkmER,
                BpjsJhtEmployee = bpjsJhtEE,
                BpjsJhtEmployer = bpjsJhtER,
                BpjsJpEmployee = bpjsJpEE,
                BpjsJpEmployer = bpjsJpER,
                TotalEmployeeContribution = emp.TotalEmployeeInsuranceContribution,
                TotalEmployerContribution = emp.TotalEmployerInsuranceContribution
            };
        }
    }
}
