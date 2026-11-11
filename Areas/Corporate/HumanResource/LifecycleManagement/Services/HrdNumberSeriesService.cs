using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services
{
    public class HrdNumberSeriesService : IHrdNumberSeriesService
    {
        private readonly ApplicationDbContext _dbContext;

        public HrdNumberSeriesService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string> GenerateOnboardingCaseNumberAsync(CancellationToken cancellationToken = default)
        {
            var year = DateTime.UtcNow.Year;
            var prefix = $"ONB-{year}-";
            var count = await _dbContext.HrdOnboardingCases
                .CountAsync(x => x.CaseNumber.StartsWith(prefix), cancellationToken);
            return $"{prefix}{(count + 1):D6}";
        }

        public async Task<string> GenerateContractNumberAsync(CancellationToken cancellationToken = default)
        {
            var year = DateTime.UtcNow.Year;
            var prefix = $"CTR-{year}-";
            var count = await _dbContext.WfpContractHistories
                .CountAsync(x => x.ContractNumber.StartsWith(prefix), cancellationToken);
            return $"{prefix}{(count + 1):D6}";
        }

        public async Task<string> GenerateAssessmentNumberAsync(CancellationToken cancellationToken = default)
        {
            var year = DateTime.UtcNow.Year;
            var prefix = $"PBA-{year}-";
            var count = await _dbContext.HrdProbationAssessments
                .CountAsync(x => x.AssessmentNumber.StartsWith(prefix), cancellationToken);
            return $"{prefix}{(count + 1):D6}";
        }

        public async Task<string> GenerateDecisionNumberAsync(CancellationToken cancellationToken = default)
        {
            var year = DateTime.UtcNow.Year;
            var prefix = $"CDA-{year}-";
            var count = await _dbContext.HrdContractDecisions
                .CountAsync(x => x.DecisionNumber.StartsWith(prefix), cancellationToken);
            return $"{prefix}{(count + 1):D6}";
        }

        public async Task<string> GenerateCorrectionNumberAsync(CancellationToken cancellationToken = default)
        {
            var year = DateTime.UtcNow.Year;
            var prefix = $"CCA-{year}-";
            var count = await _dbContext.HrdContractCorrections
                .CountAsync(x => x.CorrectionNumber.StartsWith(prefix), cancellationToken);
            return $"{prefix}{(count + 1):D6}";
        }

        public async Task<string> GenerateBatchNumberAsync(CancellationToken cancellationToken = default)
        {
            var year = DateTime.UtcNow.Year;
            var prefix = $"ICB-{year}-";
            var count = await _dbContext.HrdInitialContractBatches
                .CountAsync(x => x.BatchNumber.StartsWith(prefix), cancellationToken);
            return $"{prefix}{(count + 1):D6}";
        }
    }
}
