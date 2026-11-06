using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Persistence;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Services
{
    public class PayrollEarningsCalculationService : IPayrollEarningsCalculationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PayrollEarningsCalculationService> _logger;

        public PayrollEarningsCalculationService(
            ApplicationDbContext context,
            ILogger<PayrollEarningsCalculationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PayrollServiceResult<PayrollEarningsCalculationResultDto>> CalculateFixedEarningsAsync(
            Guid runId,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Memulai kalkulasi penghasilan tetap batch penggajian ID: {RunId}. Actor: {ActorId}",
                runId, actorUserId);

            var run = await _context.TrxPayrollRuns
                .Include(r => r.Employees.Where(e => e.IsActive))
                    .ThenInclude(e => e.Components.Where(c => c.IsActive))
                .FirstOrDefaultAsync(r => r.Id == runId && r.IsActive, cancellationToken);

            if (run == null)
            {
                return PayrollServiceResult<PayrollEarningsCalculationResultDto>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Batch penggajian dengan ID {runId} tidak ditemukan.");
            }

            if (run.IsLocked || run.RunStatus is "Approved" or "PaymentProcessing" or "Paid" or "Posted" or "Closed")
            {
                return PayrollServiceResult<PayrollEarningsCalculationResultDto>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    $"Batch penggajian {run.RunNumber} telah berstatus '{run.RunStatus}' (Locked={run.IsLocked}) dan tidak dapat dihitung ulang.");
            }

            var employees = run.Employees.ToList();
            var employeeResults = new List<PayrollEmployeeEarningsResultDto>();

            foreach (var emp in employees)
            {
                var singleResult = ProcessEmployeeFixedEarnings(emp, actorUserId);
                employeeResults.Add(singleResult);
            }

            // Agregasi Ringkasan Run
            run.TotalBaseSalary = employees.Sum(e => e.BaseSalary);
            run.TotalEarning = employees.Sum(e => e.TotalRecurringEarning + e.TotalVariableEarning + e.TotalOvertimeAmount + e.TotalAttendanceAllowance);
            run.TotalGrossPay = employees.Sum(e => e.GrossPay);
            run.TotalNetPay = employees.Sum(e => e.NetPay);
            run.ProcessedEmployeeCount = employees.Count;
            run.RunStatus = "Calculating";
            run.CalculatedAt = DateTime.UtcNow;
            run.CalculatedByUserId = actorUserId;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Kalkulasi penghasilan tetap selesai untuk {Count} pegawai pada batch {RunNumber}. Total Gross: {TotalGross:N2}",
                employees.Count, run.RunNumber, run.TotalGrossPay);

            return PayrollServiceResult<PayrollEarningsCalculationResultDto>.Ok(
                new PayrollEarningsCalculationResultDto
                {
                    RunId = run.Id,
                    RunNumber = run.RunNumber,
                    TotalEmployeesCalculated = employees.Count,
                    TotalBaseSalary = run.TotalBaseSalary,
                    TotalRecurringAllowance = employees.Sum(e => e.TotalRecurringEarning),
                    TotalGrossPay = run.TotalGrossPay,
                    CalculatedAt = run.CalculatedAt.Value,
                    Message = $"Kalkulasi penghasilan tetap untuk {employees.Count} pegawai berhasil diproses.",
                    EmployeeResults = employeeResults
                },
                "Kalkulasi penghasilan tetap berhasil diproses.");
        }

        public async Task<PayrollServiceResult<PayrollEmployeeEarningsResultDto>> CalculateEmployeeFixedEarningsAsync(
            Guid runId,
            Guid employeeRunId,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var emp = await _context.TrxPayrollRunEmployees
                .Include(e => e.Components.Where(c => c.IsActive))
                .Include(e => e.PayrollRun)
                .FirstOrDefaultAsync(e => e.Id == employeeRunId && e.PayrollRunId == runId && e.IsActive, cancellationToken);

            if (emp == null)
            {
                return PayrollServiceResult<PayrollEmployeeEarningsResultDto>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Data pegawai penggajian dengan ID {employeeRunId} pada batch {runId} tidak ditemukan.");
            }

            if (emp.PayrollRun != null && (emp.PayrollRun.IsLocked || emp.PayrollRun.RunStatus is "Approved" or "Paid" or "Closed"))
            {
                return PayrollServiceResult<PayrollEmployeeEarningsResultDto>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    $"Batch penggajian telah berstatus final dan tidak dapat diperbarui.");
            }

            var result = ProcessEmployeeFixedEarnings(emp, actorUserId);
            await _context.SaveChangesAsync(cancellationToken);

            return PayrollServiceResult<PayrollEmployeeEarningsResultDto>.Ok(
                result,
                $"Kalkulasi penghasilan tetap untuk {emp.EmployeeNameSnapshot} berhasil.");
        }

        private PayrollEmployeeEarningsResultDto ProcessEmployeeFixedEarnings(
            TrxPayrollRunEmployee emp,
            Guid actorUserId)
        {
            // 1. Bersihkan komponen penghasilan tetap terdahulu
            var existingFixedComponents = emp.Components
                .Where(c => c.SourceType is "SalaryAssignment" or "FixedAllowance")
                .ToList();

            foreach (var comp in existingFixedComponents)
            {
                _context.TrxPayrollEmployeeComponents.Remove(comp);
            }

            // 2. Evaluasi Nominal Komponen Tetap
            var structuralAlw = EvaluateStructuralAllowance(emp.PositionNameSnapshot);
            var functionalAlw = EvaluateFunctionalAllowance(emp.PositionNameSnapshot, emp.DepartmentNameSnapshot);
            var hazardAlw = EvaluateHazardAllowance(emp.DepartmentNameSnapshot, emp.PositionNameSnapshot);

            var newComponents = new List<TrxPayrollEmployeeComponent>();
            var componentSummary = new List<PayrollComponentSummaryDto>();

            // Komponen: Gaji Pokok
            if (emp.BaseSalary > 0)
            {
                var baseComp = new TrxPayrollEmployeeComponent
                {
                    Id = Guid.NewGuid(),
                    PayrollRunEmployeeId = emp.Id,
                    SourceType = "SalaryAssignment",
                    ComponentCodeSnapshot = "GP01",
                    ComponentNameSnapshot = "Gaji Pokok",
                    ComponentTypeSnapshot = "Earning",
                    CalculationMethodSnapshot = "Fixed",
                    CurrencyCode = "IDR",
                    Quantity = 1m,
                    Rate = emp.BaseSalary,
                    Amount = emp.BaseSalary,
                    TaxableAmount = emp.BaseSalary,
                    IsTaxable = true,
                    IsDisplayedOnPayslip = true,
                    SortOrder = 1,
                    IsActive = true,
                    CreatedBy = actorUserId.ToString(),
                    CreatedDate = DateTime.UtcNow
                };
                newComponents.Add(baseComp);
                componentSummary.Add(new PayrollComponentSummaryDto
                {
                    ComponentCode = baseComp.ComponentCodeSnapshot,
                    ComponentName = baseComp.ComponentNameSnapshot,
                    ComponentType = baseComp.ComponentTypeSnapshot,
                    Amount = baseComp.Amount,
                    IsTaxable = baseComp.IsTaxable
                });
            }

            // Komponen: Tunjangan Struktural / Jabatan
            if (structuralAlw > 0)
            {
                var structComp = new TrxPayrollEmployeeComponent
                {
                    Id = Guid.NewGuid(),
                    PayrollRunEmployeeId = emp.Id,
                    SourceType = "FixedAllowance",
                    ComponentCodeSnapshot = "TJ01",
                    ComponentNameSnapshot = "Tunjangan Struktural / Jabatan",
                    ComponentTypeSnapshot = "Earning",
                    CalculationMethodSnapshot = "Fixed",
                    CurrencyCode = "IDR",
                    Quantity = 1m,
                    Rate = structuralAlw,
                    Amount = structuralAlw,
                    TaxableAmount = structuralAlw,
                    IsTaxable = true,
                    IsDisplayedOnPayslip = true,
                    SortOrder = 2,
                    IsActive = true,
                    CreatedBy = actorUserId.ToString(),
                    CreatedDate = DateTime.UtcNow
                };
                newComponents.Add(structComp);
                componentSummary.Add(new PayrollComponentSummaryDto
                {
                    ComponentCode = structComp.ComponentCodeSnapshot,
                    ComponentName = structComp.ComponentNameSnapshot,
                    ComponentType = structComp.ComponentTypeSnapshot,
                    Amount = structComp.Amount,
                    IsTaxable = structComp.IsTaxable
                });
            }

            // Komponen: Tunjangan Fungsional Profesi Medis / Paramedis
            if (functionalAlw > 0)
            {
                var funcComp = new TrxPayrollEmployeeComponent
                {
                    Id = Guid.NewGuid(),
                    PayrollRunEmployeeId = emp.Id,
                    SourceType = "FixedAllowance",
                    ComponentCodeSnapshot = "TF01",
                    ComponentNameSnapshot = "Tunjangan Fungsional Profesi Klinis",
                    ComponentTypeSnapshot = "Earning",
                    CalculationMethodSnapshot = "Fixed",
                    CurrencyCode = "IDR",
                    Quantity = 1m,
                    Rate = functionalAlw,
                    Amount = functionalAlw,
                    TaxableAmount = functionalAlw,
                    IsTaxable = true,
                    IsDisplayedOnPayslip = true,
                    SortOrder = 3,
                    IsActive = true,
                    CreatedBy = actorUserId.ToString(),
                    CreatedDate = DateTime.UtcNow
                };
                newComponents.Add(funcComp);
                componentSummary.Add(new PayrollComponentSummaryDto
                {
                    ComponentCode = funcComp.ComponentCodeSnapshot,
                    ComponentName = funcComp.ComponentNameSnapshot,
                    ComponentType = funcComp.ComponentTypeSnapshot,
                    Amount = funcComp.Amount,
                    IsTaxable = funcComp.IsTaxable
                });
            }

            // Komponen: Tunjangan Risiko Medis & Unit Berisiko Tinggi
            if (hazardAlw > 0)
            {
                var hazComp = new TrxPayrollEmployeeComponent
                {
                    Id = Guid.NewGuid(),
                    PayrollRunEmployeeId = emp.Id,
                    SourceType = "FixedAllowance",
                    ComponentCodeSnapshot = "TR01",
                    ComponentNameSnapshot = "Tunjangan Risiko Medis & Unit Khusus",
                    ComponentTypeSnapshot = "Earning",
                    CalculationMethodSnapshot = "Fixed",
                    CurrencyCode = "IDR",
                    Quantity = 1m,
                    Rate = hazardAlw,
                    Amount = hazardAlw,
                    TaxableAmount = hazardAlw,
                    IsTaxable = true,
                    IsDisplayedOnPayslip = true,
                    SortOrder = 4,
                    IsActive = true,
                    CreatedBy = actorUserId.ToString(),
                    CreatedDate = DateTime.UtcNow
                };
                newComponents.Add(hazComp);
                componentSummary.Add(new PayrollComponentSummaryDto
                {
                    ComponentCode = hazComp.ComponentCodeSnapshot,
                    ComponentName = hazComp.ComponentNameSnapshot,
                    ComponentType = hazComp.ComponentTypeSnapshot,
                    Amount = hazComp.Amount,
                    IsTaxable = hazComp.IsTaxable
                });
            }

            _context.TrxPayrollEmployeeComponents.AddRange(newComponents);

            // 3. Akumulasi Total Komponen Tetap & Update Saldo Pegawai
            emp.TotalRecurringEarning = structuralAlw + functionalAlw + hazardAlw;
            emp.GrossPay = emp.BaseSalary + emp.TotalRecurringEarning + emp.TotalVariableEarning + emp.TotalOvertimeAmount + emp.TotalAttendanceAllowance;
            emp.NetPay = emp.GrossPay - emp.TotalDeduction - emp.TotalTax - emp.TotalEmployeeInsuranceContribution;
            emp.EmployeePayrollStatus = "Calculated";

            return new PayrollEmployeeEarningsResultDto
            {
                PayrollRunEmployeeId = emp.Id,
                WorkforceProfileId = emp.WorkforceProfileId,
                EmployeeName = emp.EmployeeNameSnapshot,
                DepartmentName = emp.DepartmentNameSnapshot ?? "-",
                PositionName = emp.PositionNameSnapshot ?? "-",
                BaseSalary = emp.BaseSalary,
                StructuralAllowance = structuralAlw,
                FunctionalAllowance = functionalAlw,
                MedicalHazardAllowance = hazardAlw,
                TotalRecurringEarning = emp.TotalRecurringEarning,
                GrossPay = emp.GrossPay,
                Components = componentSummary
            };
        }

        private static decimal EvaluateStructuralAllowance(string? positionName)
        {
            if (string.IsNullOrWhiteSpace(positionName)) return 0m;

            var pos = positionName.ToLowerInvariant();

            if (pos.Contains("direktur") || pos.Contains("wakil direktur"))
                return 6000000m;
            if (pos.Contains("kepala bidang") || pos.Contains("kabid") || pos.Contains("manager") || pos.Contains("kepala divisi"))
                return 3500000m;
            if (pos.Contains("kepala instalasi") || pos.Contains("kanit") || pos.Contains("kepala unit") || pos.Contains("kepala seksi"))
                return 2500000m;
            if (pos.Contains("kepala ruangan") || pos.Contains("karu") || pos.Contains("supervisor"))
                return 1500000m;
            if (pos.Contains("koordinator") || pos.Contains("pj shift") || pos.Contains("katim"))
                return 750000m;

            return 0m;
        }

        private static decimal EvaluateFunctionalAllowance(string? positionName, string? departmentName)
        {
            var combined = $"{positionName ?? string.Empty} {departmentName ?? string.Empty}".ToLowerInvariant();

            if (combined.Contains("spesialis") || combined.Contains("subspesialis") || combined.Contains("konsultan"))
                return 4500000m;
            if (combined.Contains("dokter umum") || combined.Contains("dokter gigi") || combined.Contains("dokter ruangan"))
                return 2500000m;
            if (combined.Contains("apoteker"))
                return 1500000m;

            if (combined.Contains("perawat") || combined.Contains("nurse") || combined.Contains("bidan"))
            {
                if (combined.Contains("pk iv") || combined.Contains("utama")) return 1750000m;
                if (combined.Contains("pk iii") || combined.Contains("madya")) return 1250000m;
                if (combined.Contains("pk ii") || combined.Contains("muda")) return 900000m;
                if (combined.Contains("pk i") || combined.Contains("pertama")) return 600000m;
                return 750000m;
            }

            if (combined.Contains("radiografer") || combined.Contains("analis kesehatan") || combined.Contains("pranata laboratorium"))
                return 800000m;
            if (combined.Contains("fisioterapis") || combined.Contains("nutrisionis") || combined.Contains("dietisien") || combined.Contains("perekam medis"))
                return 650000m;

            return 0m;
        }

        private static decimal EvaluateHazardAllowance(string? departmentName, string? positionName)
        {
            var combined = $"{departmentName ?? string.Empty} {positionName ?? string.Empty}".ToLowerInvariant();

            if (combined.Contains("icu") || combined.Contains("iccu") || combined.Contains("picu") || combined.Contains("nicu"))
                return 1200000m;
            if (combined.Contains("isolasi") || combined.Contains("pinere") || combined.Contains("tb-mdr") || combined.Contains("infeksius"))
                return 1500000m;
            if (combined.Contains("igd") || combined.Contains("gawat darurat"))
                return 1000000m;
            if (combined.Contains("bedah") || combined.Contains("ibs") || combined.Contains("ok") || combined.Contains("operasi"))
                return 1000000m;
            if (combined.Contains("radiologi") || combined.Contains("radioterapi") || combined.Contains("kedokteran nuklir"))
                return 1250000m;
            if (combined.Contains("laboratorium") || combined.Contains("patologi"))
                return 750000m;
            if (combined.Contains("hemodialisa") || combined.Contains("dialisis"))
                return 600000m;
            if (combined.Contains("bank darah") || combined.Contains("bdrs") || combined.Contains("cssd") || combined.Contains("sterilisasi"))
                return 500000m;

            return 0m;
        }
    }
}
