using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Services
{
    public class PayrollDeductionEngineService : IPayrollDeductionEngineService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IRestitutionSettingService _settingService;
        private readonly ILogger<PayrollDeductionEngineService> _logger;

        public PayrollDeductionEngineService(
            ApplicationDbContext dbContext,
            IRestitutionSettingService settingService,
            ILogger<PayrollDeductionEngineService> logger)
        {
            _dbContext = dbContext;
            _settingService = settingService;
            _logger = logger;
        }

        public decimal CalculateSalaryCapLimit(decimal grossPay, decimal maxDeductionPercentage = 50.0m)
        {
            if (grossPay <= 0) return 0m;
            return Math.Round(grossPay * (maxDeductionPercentage / 100m), 2, MidpointRounding.AwayFromZero);
        }

        public async Task<DeductionSimulationResultDto> SimulateEmployeeDeductionsAsync(
            Guid employeeId,
            decimal grossPay,
            CancellationToken cancellationToken = default)
        {
            var maxPercentage = await _settingService.GetMaxDeductionPercentageAsync(null, cancellationToken);
            var capLimit = CalculateSalaryCapLimit(grossPay, maxPercentage);

            var activeSchedules = await _dbContext.TrxPayrollDeductionSchedules
                .AsNoTracking()
                .Where(s => s.EmployeeId == employeeId &&
                            s.ScheduleStatus == "Active" &&
                            s.RemainingPrincipal > 0 &&
                            !s.IsDelete)
                .OrderBy(s => s.PriorityLevel) // Mutlak hirarki 1 -> 2 -> 3 -> 4 (DED-B2)
                .ThenBy(s => s.StartDate)
                .ToListAsync(cancellationToken);

            var result = new DeductionSimulationResultDto
            {
                EmployeeId = employeeId,
                GrossPay = grossPay,
                MaxAllowedDeduction = capLimit,
                TotalScheduledDeduction = 0m,
                TotalActualDeduction = 0m,
                TotalDeferredDeduction = 0m,
                IsCapped = false
            };

            decimal remainingQuota = capLimit;

            foreach (var schedule in activeSchedules)
            {
                decimal scheduledAmount = Math.Min(schedule.MonthlyInstallmentAmount, schedule.RemainingPrincipal);
                result.TotalScheduledDeduction += scheduledAmount;

                var itemResult = new DeductionItemResultDto
                {
                    DeductionScheduleId = schedule.Id,
                    DeductionCategory = schedule.DeductionCategory,
                    PriorityLevel = schedule.PriorityLevel,
                    ScheduledAmount = scheduledAmount
                };

                if (scheduledAmount <= remainingQuota)
                {
                    itemResult.ActualDeductedAmount = scheduledAmount;
                    itemResult.DeferredAmount = 0m;
                    itemResult.ExecutionStatus = "DeductedInFull";
                    remainingQuota -= scheduledAmount;
                }
                else if (remainingQuota > 0m)
                {
                    itemResult.ActualDeductedAmount = remainingQuota;
                    itemResult.DeferredAmount = scheduledAmount - remainingQuota;
                    itemResult.ExecutionStatus = "PartiallyDeducted";
                    itemResult.DeferralReason = $"Sisa potongan Rp {itemResult.DeferredAmount:N2} ditunda (carry-over) ke bulan berikutnya karena mentok batas 50% Gross Pay (PP 36/2021).";
                    remainingQuota = 0m;
                    result.IsCapped = true;
                }
                else
                {
                    itemResult.ActualDeductedAmount = 0m;
                    itemResult.DeferredAmount = scheduledAmount;
                    itemResult.ExecutionStatus = "DeferredDueToSalaryCap";
                    itemResult.DeferralReason = "Ditunda otomatis ke periode berikutnya tanpa denda bunga karena batas 50% telah habis teralokasi (DED-B2, DED-B3).";
                    result.IsCapped = true;
                }

                result.TotalActualDeduction += itemResult.ActualDeductedAmount;
                result.TotalDeferredDeduction += itemResult.DeferredAmount;
                result.Items.Add(itemResult);
            }

            return result;
        }

        public async Task<DeductionSimulationResultDto> ExecutePayrollDeductionsAsync(
            Guid payrollRunId,
            Guid employeeId,
            decimal grossPay,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var simulation = await SimulateEmployeeDeductionsAsync(employeeId, grossPay, cancellationToken);

            foreach (var item in simulation.Items)
            {
                var existingItem = await _dbContext.TrxPayrollDeductionItems
                    .FirstOrDefaultAsync(x => x.PayrollRunId == payrollRunId &&
                                              x.DeductionScheduleId == item.DeductionScheduleId, cancellationToken);

                if (existingItem != null)
                {
                    existingItem.ActualDeductedAmount = item.ActualDeductedAmount;
                    existingItem.DeferredAmount = item.DeferredAmount;
                    existingItem.ExecutionStatus = item.ExecutionStatus;
                    existingItem.DeferralReason = item.DeferralReason;
                    existingItem.ExecutedAt = DateTime.UtcNow;
                    existingItem.UpdateDateTime = DateTime.UtcNow;
                    existingItem.UpdateBy = userId;
                }
                else
                {
                    var newItem = new TrxPayrollDeductionItem
                    {
                        Id = Guid.NewGuid(),
                        PayrollRunId = payrollRunId,
                        DeductionScheduleId = item.DeductionScheduleId,
                        EmployeeId = employeeId,
                        DeductionCategory = item.DeductionCategory,
                        PriorityLevel = item.PriorityLevel,
                        ScheduledAmount = item.ScheduledAmount,
                        ActualDeductedAmount = item.ActualDeductedAmount,
                        DeferredAmount = item.DeferredAmount,
                        ExecutionStatus = item.ExecutionStatus,
                        DeferralReason = item.DeferralReason,
                        ExecutedAt = DateTime.UtcNow,
                        CreateDateTime = DateTime.UtcNow,
                        CreateBy = userId
                    };
                    await _dbContext.TrxPayrollDeductionItems.AddAsync(newItem, cancellationToken);
                }

                // Update saldo pokok pada master cicilan
                var schedule = await _dbContext.TrxPayrollDeductionSchedules
                    .FirstOrDefaultAsync(s => s.Id == item.DeductionScheduleId, cancellationToken);

                if (schedule != null)
                {
                    schedule.RemainingPrincipal -= item.ActualDeductedAmount;
                    if (item.ActualDeductedAmount > 0)
                    {
                        schedule.PaidInstallmentCount++;
                    }

                    if (schedule.RemainingPrincipal <= 0m)
                    {
                        schedule.RemainingPrincipal = 0m;
                        schedule.ScheduleStatus = "Completed";
                    }

                    schedule.UpdateDateTime = DateTime.UtcNow;
                    schedule.UpdateBy = userId;
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Eksekusi pemotongan gaji selesai untuk Pegawai {Emp}: Terpotong Rp {Actual:N2}, Ditunda Rp {Deferred:N2}.",
                employeeId, simulation.TotalActualDeduction, simulation.TotalDeferredDeduction);

            return simulation;
        }

        public async Task<List<DeductionSimulationResultDto>> ExecuteBatchPayrollDeductionsAsync(
            Guid payrollRunId,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var runEmployees = await _dbContext.TrxPayrollRunEmployees
                .Where(e => e.PayrollRunId == payrollRunId && !e.IsDelete)
                .ToListAsync(cancellationToken);

            var results = new List<DeductionSimulationResultDto>();

            foreach (var runEmp in runEmployees)
            {
                // Ambil ID profil atau mapping ID pegawai
                var emp = await _dbContext.MstEmployees
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.WorkforceProfileId == runEmp.WorkforceProfileId || e.Id == runEmp.WorkforceProfileId, cancellationToken);

                var employeeId = emp?.Id ?? runEmp.WorkforceProfileId;

                var res = await ExecutePayrollDeductionsAsync(payrollRunId, employeeId, runEmp.GrossPay, userId, cancellationToken);
                results.Add(res);
            }

            return results;
        }
    }
}
