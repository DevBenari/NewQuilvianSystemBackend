using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services
{
    public class HrdEmploymentLifecycleSchedulerService : IHrdEmploymentLifecycleSchedulerService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IHrdOnboardingCaseService _caseService;
        private readonly ILogger<HrdEmploymentLifecycleSchedulerService> _logger;

        public HrdEmploymentLifecycleSchedulerService(
            ApplicationDbContext dbContext,
            IHrdOnboardingCaseService caseService,
            ILogger<HrdEmploymentLifecycleSchedulerService> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _caseService = caseService ?? throw new ArgumentNullException(nameof(caseService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<int> ExecuteDailyTickAsync(
            DateOnly? testDate = null,
            CancellationToken cancellationToken = default)
        {
            var today = testDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
            var todayDateTime = today.ToDateTime(TimeOnly.MinValue);
            var processedCount = 0;

            _logger.LogInformation("Menjalankan langkah harian siklus hidup kepegawaian untuk tanggal {Today}", today);

            // 1. Pengaktifan Kontrak Lanjutan Berjalan pada Tanggal Mulai
            var contractsToActivate = await _dbContext.WfpContractHistories
                .Where(c => !c.IsDelete &&
                            c.ContractStatus == EmploymentLifecycleValueConstants.ContractStatusSigned &&
                            !c.IsCurrent &&
                            c.StartDate <= todayDateTime)
                .ToListAsync(cancellationToken);

            foreach (var contract in contractsToActivate)
            {
                contract.IsCurrent = true;
                contract.UpdateDateTime = DateTime.UtcNow;

                if (contract.PreviousContractHistoryId.HasValue)
                {
                    var previous = await _dbContext.WfpContractHistories
                        .FirstOrDefaultAsync(p => p.Id == contract.PreviousContractHistoryId.Value, cancellationToken);

                    if (previous != null)
                    {
                        previous.IsCurrent = false;
                        previous.ContractStatus = "Completed";
                        previous.UpdateDateTime = DateTime.UtcNow;
                    }
                }

                processedCount++;
            }

            // 2. Pengakhiran Kontrak yang Lewat Tanggal Berakhir (Sehari Sesudah EndDate)
            var expiredContracts = await _dbContext.WfpContractHistories
                .Where(c => !c.IsDelete &&
                            c.ContractStatus == EmploymentLifecycleValueConstants.ContractStatusSigned &&
                            c.EndDate.HasValue &&
                            c.EndDate.Value < todayDateTime)
                .ToListAsync(cancellationToken);

            foreach (var contract in expiredContracts)
            {
                // Cek apakah ada kontrak lanjutan yang sudah Signed
                var hasSignedRenewal = await _dbContext.WfpContractHistories
                    .AnyAsync(r => r.PreviousContractHistoryId == contract.Id &&
                                   r.ContractStatus == EmploymentLifecycleValueConstants.ContractStatusSigned &&
                                   !r.IsDelete, cancellationToken);

                if (!hasSignedRenewal)
                {
                    // Cek keputusan kontrak
                    var decision = await _dbContext.HrdContractDecisions
                        .FirstOrDefaultAsync(d => d.ContractHistoryId == contract.Id && !d.IsDelete, cancellationToken);

                    if (decision != null && decision.DecisionStatus == ContractDecisionStatus.Approved &&
                        decision.DecisionType == ContractDecisionType.NotRenew)
                    {
                        contract.ContractStatus = "Ended";
                        contract.IsCurrent = false;
                        contract.UpdateDateTime = DateTime.UtcNow;
                    }
                    else if (decision == null || decision.DecisionStatus != ContractDecisionStatus.Approved)
                    {
                        // Kontrak berakhir tanpa keputusan disetujui
                        contract.ContractStatus = "LapsedWithoutDecision";
                        contract.IsCurrent = false;
                        contract.UpdateDateTime = DateTime.UtcNow;
                    }

                    processedCount++;
                }
            }

            // 3. Evaluasi Otomatis Masa Percobaan yang Lewat Tanggal Berakhir (Lulus Otomatis & Penutupan Penilaian Terbuka)
            var expiredProbations = await _dbContext.HrdProbations
                .Include(p => p.Assessments)
                .Where(p => !p.IsDelete &&
                            (p.ProbationStatus == ProbationStatus.Running || p.ProbationStatus == ProbationStatus.AwaitingDecision) &&
                            p.EndDate < today)
                .ToListAsync(cancellationToken);

            foreach (var probation in expiredProbations)
            {
                probation.ProbationStatus = ProbationStatus.PassedAutomatically;
                probation.ClosedAt = DateTime.UtcNow;
                probation.RowVersion = Guid.NewGuid();
                probation.UpdateDateTime = DateTime.UtcNow;

                // Tutup penilaian terbuka yang belum sempat diputus (ClosedOnExpiry)
                foreach (var assessment in probation.Assessments.Where(a => !a.IsDelete &&
                    (a.AssessmentStatus == ProbationAssessmentStatus.Submitted || a.AssessmentStatus == ProbationAssessmentStatus.Returned)))
                {
                    assessment.AssessmentStatus = ProbationAssessmentStatus.ClosedOnExpiry;
                    assessment.DecisionNote = "Penilaian ditutup otomatis karena masa percobaan telah melewati tanggal akhir tanpa persetujuan (Lulus Otomatis).";
                    assessment.DecidedAt = DateTime.UtcNow;
                    assessment.RowVersion = Guid.NewGuid();
                    assessment.UpdateDateTime = DateTime.UtcNow;
                }

                processedCount++;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            // 4. Evaluasi Butir Otomatis Kasus Onboarding Berjalan
            var activeCases = await _dbContext.HrdOnboardingCases
                .Where(c => !c.IsDelete && c.CaseStatus == OnboardingCaseStatus.InProgress)
                .Select(c => c.Id)
                .ToListAsync(cancellationToken);

            foreach (var caseId in activeCases)
            {
                try
                {
                    await _caseService.CheckAutomaticItemsAsync(caseId, new System.Security.Claims.ClaimsPrincipal(), cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Gagal mengevaluasi butir otomatis untuk kasus {CaseId}", caseId);
                }
            }

            _logger.LogInformation("Selesai menjalankan scheduler siklus kepegawaian. Total {Count} entitas dimutakhirkan.", processedCount);
            return processedCount;
        }
    }
}
