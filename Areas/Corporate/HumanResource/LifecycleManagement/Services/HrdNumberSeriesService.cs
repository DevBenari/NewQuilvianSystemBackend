using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

        private static string NextSequence(string prefix, IEnumerable<string> existingNumbers)
        {
            var maxSeq = 0;
            foreach (var code in existingNumbers)
            {
                if (string.IsNullOrWhiteSpace(code) || code.Length <= prefix.Length) continue;
                var tail = code.AsSpan(prefix.Length);
                if (int.TryParse(tail, NumberStyles.Integer, CultureInfo.InvariantCulture, out var num) && num > maxSeq)
                {
                    maxSeq = num;
                }
            }
            return $"{prefix}{(maxSeq + 1):D6}";
        }

        public async Task<string> GenerateOnboardingCaseNumberAsync(CancellationToken cancellationToken = default)
        {
            var prefix = $"ONB-{DateTime.UtcNow.Year}-";
            var existing = await _dbContext.HrdOnboardingCases
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.CaseNumber.StartsWith(prefix))
                .Select(x => x.CaseNumber)
                .ToListAsync(cancellationToken);
            return NextSequence(prefix, existing);
        }

        public async Task<string> GenerateContractNumberAsync(CancellationToken cancellationToken = default)
        {
            var prefix = $"CTR-{DateTime.UtcNow.Year}-";
            var existing = await _dbContext.WfpContractHistories
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.ContractNumber.StartsWith(prefix))
                .Select(x => x.ContractNumber)
                .ToListAsync(cancellationToken);
            return NextSequence(prefix, existing);
        }

        public async Task<string> GenerateAssessmentNumberAsync(CancellationToken cancellationToken = default)
        {
            var prefix = $"PBA-{DateTime.UtcNow.Year}-";
            var existing = await _dbContext.HrdProbationAssessments
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.AssessmentNumber.StartsWith(prefix))
                .Select(x => x.AssessmentNumber)
                .ToListAsync(cancellationToken);
            return NextSequence(prefix, existing);
        }

        public async Task<string> GenerateDecisionNumberAsync(CancellationToken cancellationToken = default)
        {
            var prefix = $"CDA-{DateTime.UtcNow.Year}-";
            var existing = await _dbContext.HrdContractDecisions
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.DecisionNumber.StartsWith(prefix))
                .Select(x => x.DecisionNumber)
                .ToListAsync(cancellationToken);
            return NextSequence(prefix, existing);
        }

        public async Task<string> GenerateCorrectionNumberAsync(CancellationToken cancellationToken = default)
        {
            var prefix = $"CCA-{DateTime.UtcNow.Year}-";
            var existing = await _dbContext.HrdContractCorrections
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.CorrectionNumber.StartsWith(prefix))
                .Select(x => x.CorrectionNumber)
                .ToListAsync(cancellationToken);
            return NextSequence(prefix, existing);
        }

        public async Task<string> GenerateBatchNumberAsync(CancellationToken cancellationToken = default)
        {
            var prefix = $"ICB-{DateTime.UtcNow.Year}-";
            var existing = await _dbContext.HrdInitialContractBatches
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.BatchNumber.StartsWith(prefix))
                .Select(x => x.BatchNumber)
                .ToListAsync(cancellationToken);
            return NextSequence(prefix, existing);
        }
    }
}
