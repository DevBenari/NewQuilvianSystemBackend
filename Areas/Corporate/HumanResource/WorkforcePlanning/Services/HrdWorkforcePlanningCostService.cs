using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.DTOs;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Services
{
    /// <summary>
    /// Layanan estimasi biaya penambahan headcount dari gaji tengah grade dan penyamaran biaya (BE-HRD-038).
    /// </summary>
    public sealed class HrdWorkforcePlanningCostService : IHrdWorkforcePlanningCostService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<HrdWorkforcePlanningCostService> _logger;

        public HrdWorkforcePlanningCostService(
            ApplicationDbContext dbContext,
            ILogger<HrdWorkforcePlanningCostService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<CostEstimationResult> CalculateEstimatedCostAsync(
            Guid? salaryGradeId,
            decimal workloadPortion,
            int months,
            DateOnly asOfDate,
            ClaimsPrincipal? user,
            CancellationToken cancellationToken = default)
        {
            var isCostVisible = HasCostReadPermission(user);

            if (!salaryGradeId.HasValue)
            {
                return new CostEstimationResult
                {
                    EstimatedCost = null,
                    MidpointSalary = null,
                    Months = months,
                    WorkloadPortion = workloadPortion,
                    RateStatus = CostRateStatus.RateUnavailable,
                    Note = "Grade gaji tidak dipilih",
                    CostVisible = isCostVisible
                };
            }

            var asOfDateTime = asOfDate.ToDateTime(TimeOnly.MinValue);

            var salaryGrade = await _dbContext.MstSalaryGrades
                .AsNoTracking()
                .Where(g => !g.IsDelete &&
                            g.Id == salaryGradeId.Value &&
                            g.IsActive &&
                            (!g.EffectiveStartDate.HasValue || g.EffectiveStartDate.Value <= asOfDateTime) &&
                            (!g.EffectiveEndDate.HasValue || g.EffectiveEndDate.Value >= asOfDateTime))
                .FirstOrDefaultAsync(cancellationToken);

            if (salaryGrade == null || salaryGrade.MidpointSalary <= 0m)
            {
                _logger.LogInformation("Tarif gaji tengah tidak tersedia untuk SalaryGradeId={SalaryGradeId}", salaryGradeId);

                return new CostEstimationResult
                {
                    EstimatedCost = null,
                    MidpointSalary = null,
                    Months = months,
                    WorkloadPortion = workloadPortion,
                    RateStatus = CostRateStatus.RateUnavailable,
                    Note = "Tarif gaji tengah belum diatur untuk grade ini",
                    CostVisible = isCostVisible
                };
            }

            var midpoint = salaryGrade.MidpointSalary;
            var estimatedCost = workloadPortion * midpoint * months;

            if (!isCostVisible)
            {
                return new CostEstimationResult
                {
                    EstimatedCost = null,
                    MidpointSalary = null,
                    Months = months,
                    WorkloadPortion = workloadPortion,
                    RateStatus = CostRateStatus.Available,
                    Note = "Biaya disamarkan (tidak memiliki wewenang akses biaya)",
                    CostVisible = false
                };
            }

            return new CostEstimationResult
            {
                EstimatedCost = estimatedCost,
                MidpointSalary = midpoint,
                Months = months,
                WorkloadPortion = workloadPortion,
                RateStatus = CostRateStatus.Available,
                Note = "Estimasi berdasarkan gaji tengah grade",
                CostVisible = true
            };
        }

        public bool HasCostReadPermission(ClaimsPrincipal? user)
        {
            if (user == null) return false;

            // Memeriksa permission WorkforcePlanningCost:Read atau role Finance/SuperAdmin
            if (user.IsInRole("SuperAdmin") || user.IsInRole("Admin") || user.IsInRole("FinanceAdmin") || user.IsInRole("HRManager"))
            {
                return true;
            }

            // Memeriksa claim permission khusus
            return user.HasClaim("permission", "WorkforcePlanningCost:Read") ||
                   user.HasClaim("permission", "WorkforcePlanning:CostRead");
        }

        public decimal? MaskCost(decimal? cost, ClaimsPrincipal? user)
        {
            return HasCostReadPermission(user) ? cost : null;
        }
    }
}
