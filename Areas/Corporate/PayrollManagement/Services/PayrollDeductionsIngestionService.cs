using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Repositories;
using System.Text.Json;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Services
{
    public class PayrollDeductionsIngestionService : IPayrollDeductionsIngestionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPayrollSettingService _settingService;
        private readonly ILogger<PayrollDeductionsIngestionService> _logger;

        public PayrollDeductionsIngestionService(
            ApplicationDbContext context,
            IPayrollSettingService settingService,
            ILogger<PayrollDeductionsIngestionService> logger)
        {
            _context = context;
            _settingService = settingService;
            _logger = logger;
        }

        public async Task<PayrollServiceResult<IngestDeductionsResultDto>> IngestDeductionsAsync(
            Guid runId,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Memulai penyerapan potongan pinjaman koperasi & restitusi (Sub-modul 11) untuk batch penggajian ID: {RunId}. Actor: {ActorId}",
                runId, actorUserId);

            var run = await _context.PrlPayrollRuns
                .Include(r => r.PayrollPeriod)
                .Include(r => r.Employees.Where(e => e.IsActive))
                    .ThenInclude(e => e.Components.Where(c => c.IsActive))
                .FirstOrDefaultAsync(r => r.Id == runId && r.IsActive, cancellationToken);

            if (run == null)
            {
                return PayrollServiceResult<IngestDeductionsResultDto>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Batch penggajian dengan ID {runId} tidak ditemukan.");
            }

            if (run.IsLocked || run.RunStatus is "Approved" or "PaymentProcessing" or "Paid" or "Posted" or "Closed")
            {
                return PayrollServiceResult<IngestDeductionsResultDto>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    $"Batch penggajian {run.RunNumber} telah berstatus '{run.RunStatus}' (Locked={run.IsLocked}) dan tidak dapat dimodifikasi.");
            }

            var setting = await ResolveEffectiveSettingAsync(run.ConfigurationSnapshotJson, cancellationToken);
            var maxLoanPercentage = setting.MaxLoanDeductionPercentage > 0 ? setting.MaxLoanDeductionPercentage : 50.00m;

            var employees = run.Employees.ToList();
            var profileIds = employees.Select(e => e.WorkforceProfileId).Distinct().ToList();

            var periodEndDate = run.PeriodEndDateSnapshot != default
                ? run.PeriodEndDateSnapshot
                : (run.PayrollPeriod?.EndDate ?? DateTime.UtcNow);

            // Kueri seluruh angsuran pinjaman aktif yang jatuh tempo sebelum/pada akhir periode
            var activeInstallments = await _context.TrxEmployeeLoanInstallments
                .Include(i => i.EmployeeLoan)
                .Where(i => i.IsActive
                    && profileIds.Contains(i.EmployeeLoan!.WorkforceProfileId)
                    && (i.InstallmentStatus == "Scheduled" || i.InstallmentStatus == "PartiallyPaid")
                    && i.DueDate <= periodEndDate
                    && (i.EmployeeLoan.LoanStatus == "Approved" || i.EmployeeLoan.LoanStatus == "Disbursed" || i.EmployeeLoan.LoanStatus == "Active"))
                .OrderBy(i => i.DueDate)
                .ThenBy(i => i.InstallmentNumber)
                .ToListAsync(cancellationToken);

            var installmentsByProfile = activeInstallments
                .GroupBy(i => i.EmployeeLoan!.WorkforceProfileId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var details = new List<EmployeeDeductionSummaryDto>();
            int totalInstallmentsDeducted = 0;
            decimal totalLoanAmount = 0m;
            int cappedCount = 0;

            foreach (var emp in employees)
            {
                installmentsByProfile.TryGetValue(emp.WorkforceProfileId, out var empInstallments);
                empInstallments ??= new List<TrxEmployeeLoanInstallment>();

                var summary = ProcessEmployeeDeductions(emp, empInstallments, maxLoanPercentage, run.RunNumber, actorUserId);
                details.Add(summary);

                totalInstallmentsDeducted += summary.Deductions.Count(d => d.ComponentCode.StartsWith("DED_LOAN"));
                totalLoanAmount += summary.ActualLoanDeduction;
                if (summary.IsCappedByLimit) cappedCount++;
            }

            // Agregasi Ringkasan Run
            run.TotalDeduction = employees.Sum(e => e.TotalDeduction);
            run.TotalNetPay = employees.Sum(e => e.NetPay);
            run.UpdateDateTime = DateTime.UtcNow;
            run.UpdateBy = actorUserId;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Penyerapan potongan cicilan selesai untuk {Count} pegawai pada batch {RunNumber}. Total Pinjaman Terpotong: Rp {TotalLoan:N2}, Pegawai Capped: {Capped}",
                employees.Count, run.RunNumber, totalLoanAmount, cappedCount);

            return PayrollServiceResult<IngestDeductionsResultDto>.Ok(
                new IngestDeductionsResultDto
                {
                    RunId = run.Id,
                    RunNumber = run.RunNumber,
                    TotalEmployeesProcessed = employees.Count,
                    TotalLoanInstallmentsDeducted = totalInstallmentsDeducted,
                    TotalLoanDeductionAmount = totalLoanAmount,
                    TotalRestitutionsDeducted = 0,
                    TotalRestitutionDeductionAmount = 0m,
                    TotalDeductionsAmount = run.TotalDeduction,
                    TotalEmployeesCappedByLimit = cappedCount,
                    Message = $"Penyerapan {totalInstallmentsDeducted} cicilan pinjaman koperasi/internal untuk {employees.Count} pegawai berhasil diproses.",
                    Details = details
                },
                "Penyerapan potongan pinjaman internal berhasil.");
        }

        public async Task<PayrollServiceResult<EmployeeDeductionSummaryDto>> IngestEmployeeDeductionsAsync(
            Guid runId,
            Guid employeeRunId,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var emp = await _context.PrlPayrollRunEmployees
                .Include(e => e.Components.Where(c => c.IsActive))
                .Include(e => e.PayrollRun)
                    .ThenInclude(r => r!.PayrollPeriod)
                .FirstOrDefaultAsync(e => e.Id == employeeRunId && e.PayrollRunId == runId && e.IsActive, cancellationToken);

            if (emp == null || emp.PayrollRun == null)
            {
                return PayrollServiceResult<EmployeeDeductionSummaryDto>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Data pegawai penggajian ID {employeeRunId} pada batch {runId} tidak ditemukan.");
            }

            if (emp.PayrollRun.IsLocked || emp.PayrollRun.RunStatus is "Approved" or "Paid" or "Closed")
            {
                return PayrollServiceResult<EmployeeDeductionSummaryDto>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    $"Batch penggajian telah berstatus final dan tidak dapat diperbarui.");
            }

            var setting = await ResolveEffectiveSettingAsync(emp.PayrollRun.ConfigurationSnapshotJson, cancellationToken);
            var maxLoanPercentage = setting.MaxLoanDeductionPercentage > 0 ? setting.MaxLoanDeductionPercentage : 50.00m;

            var periodEndDate = emp.PayrollRun.PeriodEndDateSnapshot != default
                ? emp.PayrollRun.PeriodEndDateSnapshot
                : (emp.PayrollRun.PayrollPeriod?.EndDate ?? DateTime.UtcNow);

            var activeInstallments = await _context.TrxEmployeeLoanInstallments
                .Include(i => i.EmployeeLoan)
                .Where(i => i.IsActive
                    && i.EmployeeLoan!.WorkforceProfileId == emp.WorkforceProfileId
                    && (i.InstallmentStatus == "Scheduled" || i.InstallmentStatus == "PartiallyPaid")
                    && i.DueDate <= periodEndDate
                    && (i.EmployeeLoan.LoanStatus == "Approved" || i.EmployeeLoan.LoanStatus == "Disbursed" || i.EmployeeLoan.LoanStatus == "Active"))
                .OrderBy(i => i.DueDate)
                .ThenBy(i => i.InstallmentNumber)
                .ToListAsync(cancellationToken);

            var summary = ProcessEmployeeDeductions(emp, activeInstallments, maxLoanPercentage, emp.PayrollRun.RunNumber, actorUserId);

            // Perbarui agregasi batch
            var run = emp.PayrollRun;
            var allEmps = await _context.PrlPayrollRunEmployees
                .Where(e => e.PayrollRunId == runId && e.IsActive)
                .ToListAsync(cancellationToken);

            run.TotalDeduction = allEmps.Sum(e => e.TotalDeduction);
            run.TotalNetPay = allEmps.Sum(e => e.NetPay);
            run.UpdateDateTime = DateTime.UtcNow;
            run.UpdateBy = actorUserId;

            await _context.SaveChangesAsync(cancellationToken);

            return PayrollServiceResult<EmployeeDeductionSummaryDto>.Ok(
                summary,
                $"Penyerapan potongan pinjaman untuk {emp.EmployeeNameSnapshot} berhasil.");
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

        private EmployeeDeductionSummaryDto ProcessEmployeeDeductions(
            PrlPayrollRunEmployee emp,
            List<TrxEmployeeLoanInstallment> activeInstallments,
            decimal maxLoanPercentage,
            string runNumber,
            Guid actorUserId)
        {
            // 1. Bersihkan komponen potongan pinjaman/restitusi lama
            var existingLoanComponents = emp.Components
                .Where(c => c.SourceType == "Loan" || c.ComponentCodeSnapshot.StartsWith("DED_LOAN"))
                .ToList();

            foreach (var comp in existingLoanComponents)
            {
                _context.PrlPayrollEmployeeComponents.Remove(comp);
            }

            // 2. Evaluasi batas proteksi penghidupan layak pegawai (VR-PAY-25):
            // Maksimal 50% dari penghasilan kotor (GrossPay) dan sisa Take-Home Pay tidak boleh negatif
            var maxAllowedLoan = Math.Round(emp.GrossPay * (maxLoanPercentage / 100m), 2, MidpointRounding.AwayFromZero);
            var deductionsBeforeLoan = emp.Components
                .Where(c => c.IsActive && c.ComponentTypeSnapshot == "Deduction" && !c.ComponentCodeSnapshot.StartsWith("DED_LOAN"))
                .Sum(c => c.Amount);

            var remainingNetBeforeLoan = Math.Max(0m, emp.GrossPay - deductionsBeforeLoan);
            var loanCeiling = Math.Min(maxAllowedLoan, remainingNetBeforeLoan);

            decimal requestedLoanTotal = 0m;
            decimal actualLoanTotal = 0m;
            bool isCapped = false;
            var deductionSummaryList = new List<PayrollComponentSummaryDto>();

            // 3. Proses setiap angsuran pinjaman berjalan
            foreach (var inst in activeInstallments)
            {
                var dueAmount = inst.OutstandingAmount > 0 ? inst.OutstandingAmount : inst.ExpectedAmount;
                requestedLoanTotal += dueAmount;

                var availableBudget = Math.Max(0m, loanCeiling - actualLoanTotal);

                if (availableBudget <= 0)
                {
                    isCapped = true;
                    continue; // Batas pagu terlampaui, sisa cicilan ditunda ke siklus berikutnya
                }

                decimal actualDeduct;
                if (dueAmount <= availableBudget)
                {
                    actualDeduct = dueAmount;
                    inst.InstallmentStatus = "Paid";
                    inst.PaidAmount += actualDeduct;
                    inst.OutstandingAmount = 0m;
                }
                else
                {
                    actualDeduct = availableBudget;
                    inst.InstallmentStatus = "PartiallyPaid";
                    inst.PaidAmount += actualDeduct;
                    inst.OutstandingAmount = dueAmount - actualDeduct;
                    isCapped = true;
                }

                inst.PayrollRunEmployeeId = emp.Id;
                inst.PaidAt = DateTime.UtcNow;
                inst.PaidByUserId = actorUserId;
                inst.PaymentReferenceNumber = $"{runNumber}-LOAN-{inst.InstallmentNumber:D2}";
                inst.UpdateDateTime = DateTime.UtcNow;
                inst.UpdateBy = actorUserId;

                actualLoanTotal += actualDeduct;

                // Tambahkan komponen potongan slip gaji
                var isKoperasi = (inst.EmployeeLoan?.LoanType ?? string.Empty).ToLowerInvariant().Contains("koperasi");
                var compCode = isKoperasi ? "DED_LOAN_KOP" : "DED_LOAN";
                var loanName = inst.EmployeeLoan?.LoanNumber ?? "Pinjaman Karyawan";

                var loanComp = new PrlPayrollEmployeeComponent
                {
                    Id = Guid.NewGuid(),
                    PayrollRunEmployeeId = emp.Id,
                    SourceType = "Loan",
                    ComponentCodeSnapshot = compCode,
                    ComponentNameSnapshot = $"Potongan {loanName} (Angsuran Ke-{inst.InstallmentNumber})",
                    ComponentTypeSnapshot = "Deduction",
                    CalculationMethodSnapshot = "Fixed",
                    BaseAmount = dueAmount,
                    Amount = actualDeduct,
                    CurrencyCode = "IDR",
                    IsTaxable = false,
                    IsDisplayedOnPayslip = true,
                    SortOrder = 40,
                    IsActive = true,
                    CreateBy = actorUserId,
                    CreateDateTime = DateTime.UtcNow
                };

                _context.PrlPayrollEmployeeComponents.Add(loanComp);
                deductionSummaryList.Add(new PayrollComponentSummaryDto
                {
                    ComponentCode = compCode,
                    ComponentName = loanComp.ComponentNameSnapshot,
                    ComponentType = "Deduction",
                    Amount = actualDeduct,
                    IsTaxable = false
                });
            }

            // 4. Update Akumulasi Pegawai
            emp.TotalDeduction = deductionsBeforeLoan + actualLoanTotal;
            emp.NetPay = Math.Max(0m, emp.GrossPay - emp.TotalDeduction);
            emp.PaymentAmount = emp.NetPay;
            emp.UpdateDateTime = DateTime.UtcNow;
            emp.UpdateBy = actorUserId;

            return new EmployeeDeductionSummaryDto
            {
                PayrollRunEmployeeId = emp.Id,
                WorkforceProfileId = emp.WorkforceProfileId,
                EmployeeName = emp.EmployeeNameSnapshot,
                GrossPay = emp.GrossPay,
                MaxAllowedLoanDeduction = loanCeiling,
                RequestedLoanDeduction = requestedLoanTotal,
                ActualLoanDeduction = actualLoanTotal,
                ActualRestitutionDeduction = 0m,
                TotalDeductions = emp.TotalDeduction,
                NetPay = emp.NetPay,
                IsCappedByLimit = isCapped,
                Deductions = deductionSummaryList
            };
        }
    }
}
