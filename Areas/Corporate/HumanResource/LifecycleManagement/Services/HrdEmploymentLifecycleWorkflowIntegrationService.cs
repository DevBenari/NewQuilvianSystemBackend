using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services
{
    public class HrdEmploymentLifecycleWorkflowIntegrationService : IHrdEmploymentLifecycleWorkflowIntegrationService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ResignationLifecycleHandoffService _resignationHandoffService;
        private readonly ILogger<HrdEmploymentLifecycleWorkflowIntegrationService> _logger;

        public HrdEmploymentLifecycleWorkflowIntegrationService(
            ApplicationDbContext dbContext,
            ResignationLifecycleHandoffService resignationHandoffService,
            ILogger<HrdEmploymentLifecycleWorkflowIntegrationService> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _resignationHandoffService = resignationHandoffService ?? throw new ArgumentNullException(nameof(resignationHandoffService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Guid?> StartProbationAssessmentWorkflowAsync(
            Guid assessmentId,
            Guid submitterUserId,
            CancellationToken cancellationToken = default)
        {
            var assessment = await _dbContext.HrdProbationAssessments
                .FirstOrDefaultAsync(a => a.Id == assessmentId && !a.IsDelete, cancellationToken);

            if (assessment == null) return null;

            var workflowInstanceId = Guid.NewGuid();
            assessment.WorkflowInstanceId = workflowInstanceId;
            assessment.AssessmentStatus = ProbationAssessmentStatus.Submitted;
            assessment.SubmittedByUserId = submitterUserId;
            assessment.SubmittedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Workflow T15 started for ProbationAssessment {AssessmentId}, instance {WorkflowInstanceId}", assessmentId, workflowInstanceId);
            return workflowInstanceId;
        }

        public async Task<Guid?> StartContractDecisionWorkflowAsync(
            Guid decisionId,
            Guid submitterUserId,
            CancellationToken cancellationToken = default)
        {
            var decision = await _dbContext.HrdContractDecisions
                .FirstOrDefaultAsync(d => d.Id == decisionId && !d.IsDelete, cancellationToken);

            if (decision == null) return null;

            var workflowInstanceId = Guid.NewGuid();
            decision.WorkflowInstanceId = workflowInstanceId;
            decision.DecisionStatus = ContractDecisionStatus.Submitted;
            decision.SubmittedByUserId = submitterUserId;
            decision.SubmittedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Workflow T16 started for ContractDecision {DecisionId}, instance {WorkflowInstanceId}", decisionId, workflowInstanceId);
            return workflowInstanceId;
        }

        public async Task<Guid?> StartContractCorrectionWorkflowAsync(
            Guid correctionId,
            Guid submitterUserId,
            CancellationToken cancellationToken = default)
        {
            var correction = await _dbContext.HrdContractCorrections
                .FirstOrDefaultAsync(c => c.Id == correctionId && !c.IsDelete, cancellationToken);

            if (correction == null) return null;

            var workflowInstanceId = Guid.NewGuid();
            correction.WorkflowInstanceId = workflowInstanceId;
            correction.CorrectionStatus = ContractCorrectionStatus.Submitted;
            correction.SubmittedByUserId = submitterUserId;
            correction.SubmittedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Workflow T17 started for ContractCorrection {CorrectionId}, instance {WorkflowInstanceId}", correctionId, workflowInstanceId);
            return workflowInstanceId;
        }

        public async Task<Guid?> StartInitialContractBatchWorkflowAsync(
            Guid batchId,
            Guid submitterUserId,
            CancellationToken cancellationToken = default)
        {
            var batch = await _dbContext.HrdInitialContractBatches
                .FirstOrDefaultAsync(b => b.Id == batchId && !b.IsDelete, cancellationToken);

            if (batch == null) return null;

            var workflowInstanceId = Guid.NewGuid();
            batch.WorkflowInstanceId = workflowInstanceId;
            batch.BatchStatus = InitialContractBatchStatus.Submitted;
            batch.SubmittedByUserId = submitterUserId;
            batch.SubmittedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Workflow T18 started for InitialContractBatch {BatchId}, instance {WorkflowInstanceId}", batchId, workflowInstanceId);
            return workflowInstanceId;
        }

        public async Task<bool> HandleWorkflowCompletedAsync(
            Guid workflowInstanceId,
            bool isApproved,
            Guid deciderUserId,
            string? decisionNote,
            CancellationToken cancellationToken = default)
        {
            // 1. T15 - ProbationAssessment
            var assessment = await _dbContext.HrdProbationAssessments
                .Include(a => a.Probation)
                .FirstOrDefaultAsync(a => a.WorkflowInstanceId == workflowInstanceId && !a.IsDelete, cancellationToken);
            if (assessment != null)
            {
                var probation = assessment.Probation;
                if (isApproved)
                {
                    // VR-ONB-58: Persetujuan sesudah tanggal akhir ditolak
                    var today = DateOnly.FromDateTime(DateTime.UtcNow);
                    if (probation != null && today > probation.EndDate)
                    {
                        _logger.LogWarning("VR-ONB-58: Persetujuan penilaian masa percobaan dilarang karena tanggal akhir ({EndDate}) telah terlampaui.", probation.EndDate);
                        return false;
                    }

                    assessment.AssessmentStatus = ProbationAssessmentStatus.Approved;
                    assessment.DecidedByUserId = deciderUserId;
                    assessment.DecidedAt = DateTime.UtcNow;
                    assessment.DecisionNote = decisionNote;
                    assessment.UpdateDateTime = DateTime.UtcNow;
                    assessment.UpdateBy = deciderUserId;

                    if (probation != null)
                    {
                        if (assessment.Result == ProbationResult.Pass)
                        {
                            probation.ProbationStatus = ProbationStatus.Passed;
                            probation.ClosedAt = DateTime.UtcNow;
                            probation.ClosedByUserId = deciderUserId;
                            probation.RowVersion = Guid.NewGuid();
                            probation.UpdateDateTime = DateTime.UtcNow;
                            probation.UpdateBy = deciderUserId;
                        }
                        else if (assessment.Result == ProbationResult.Fail)
                        {
                            probation.ProbationStatus = ProbationStatus.Failed;
                            probation.ClosedAt = DateTime.UtcNow;
                            probation.ClosedByUserId = deciderUserId;
                            probation.RowVersion = Guid.NewGuid();
                            probation.UpdateDateTime = DateTime.UtcNow;
                            probation.UpdateBy = deciderUserId;

                            var handoff = await _resignationHandoffService.HandoffLifecycleSeparationAsync(
                                probation.WorkforceProfileId,
                                null,
                                "ProbationFailed",
                                probation.EndDate,
                                assessment.ReasonText,
                                decisionNote,
                                deciderUserId,
                                cancellationToken);

                            if (handoff.Success && handoff.Data != null)
                            {
                                probation.EmployeeSeparationId = handoff.Data.EmployeeSeparationId;
                            }
                        }
                    }
                }
                else
                {
                    assessment.AssessmentStatus = ProbationAssessmentStatus.Rejected;
                    assessment.DecidedByUserId = deciderUserId;
                    assessment.DecidedAt = DateTime.UtcNow;
                    assessment.DecisionNote = decisionNote;
                    assessment.UpdateDateTime = DateTime.UtcNow;
                    assessment.UpdateBy = deciderUserId;

                    if (probation != null)
                    {
                        probation.ProbationStatus = ProbationStatus.Running;
                        probation.RowVersion = Guid.NewGuid();
                        probation.UpdateDateTime = DateTime.UtcNow;
                        probation.UpdateBy = deciderUserId;
                    }
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
                return true;
            }

            // 2. T16 - ContractDecision
            var decision = await _dbContext.HrdContractDecisions
                .FirstOrDefaultAsync(d => d.WorkflowInstanceId == workflowInstanceId && !d.IsDelete, cancellationToken);
            if (decision != null)
            {
                decision.DecisionStatus = isApproved ? ContractDecisionStatus.Approved : ContractDecisionStatus.Rejected;
                decision.DecidedByUserId = deciderUserId;
                decision.DecidedAt = DateTime.UtcNow;
                decision.DecisionNote = decisionNote;
                await _dbContext.SaveChangesAsync(cancellationToken);
                return true;
            }

            // 3. T17 - ContractCorrection
            var correction = await _dbContext.HrdContractCorrections
                .FirstOrDefaultAsync(c => c.WorkflowInstanceId == workflowInstanceId && !c.IsDelete, cancellationToken);
            if (correction != null)
            {
                correction.CorrectionStatus = isApproved ? ContractCorrectionStatus.Approved : ContractCorrectionStatus.Rejected;
                correction.DecidedByUserId = deciderUserId;
                correction.DecidedAt = DateTime.UtcNow;
                correction.DecisionNote = decisionNote;
                await _dbContext.SaveChangesAsync(cancellationToken);
                return true;
            }

            // 4. T18 - InitialContractBatch
            var batch = await _dbContext.HrdInitialContractBatches
                .FirstOrDefaultAsync(b => b.WorkflowInstanceId == workflowInstanceId && !b.IsDelete, cancellationToken);
            if (batch != null)
            {
                batch.BatchStatus = isApproved ? InitialContractBatchStatus.Applied : InitialContractBatchStatus.Rejected;
                batch.DecidedByUserId = deciderUserId;
                batch.DecidedAt = DateTime.UtcNow;
                if (isApproved)
                {
                    batch.AppliedAt = DateTime.UtcNow;
                }
                await _dbContext.SaveChangesAsync(cancellationToken);
                return true;
            }

            return false;
        }
    }
}
