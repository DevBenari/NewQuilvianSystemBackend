using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Services
{
    public interface IOppeDataAggregationService
    {
        // 1. Metric Catalog
        Task<List<OppeMetricCatalogDto>> GetMetricCatalogsAsync(string? specialtyGroup = null, string? competencyDomain = null, CancellationToken cancellationToken = default);
        Task<OppeMetricCatalogDto> CreateMetricCatalogAsync(CreateOppeMetricCatalogRequestDto request, Guid creatorId, CancellationToken cancellationToken = default);

        // 2. Evaluation Period
        Task<List<OppeEvaluationPeriodDto>> GetEvaluationPeriodsAsync(int? year = null, CancellationToken cancellationToken = default);
        Task<OppeEvaluationPeriodDto> CreateEvaluationPeriodAsync(CreateOppeEvaluationPeriodRequestDto request, Guid creatorId, CancellationToken cancellationToken = default);
        Task<int> InitiateStaffEvaluationsForPeriodAsync(Guid periodId, Guid initiatorId, CancellationToken cancellationToken = default);

        // 3. Quantitative Aggregation 60%
        Task<OppeStaffEvaluationDto> AggregateSimrsQuantitativeMetricsAsync(Guid staffEvaluationId, Guid actorId, CancellationToken cancellationToken = default);

        // 4. Qualitative Peer Review 40%
        Task<List<AssignedPeerReviewDto>> GetAssignedPeerReviewsAsync(Guid evaluatorWorkforceProfileId, CancellationToken cancellationToken = default);
        Task<bool> SubmitPeerReviewScoresAsync(Guid staffEvaluationId, SubmitPeerReviewEvaluationRequestDto request, CancellationToken cancellationToken = default);

        // 5. Composite Score & Lifecycle Finalization
        Task<OppeStaffEvaluationDto> GetStaffEvaluationByIdAsync(Guid staffEvaluationId, CancellationToken cancellationToken = default);
        Task<OppeStaffEvaluationDto> FinalizeStaffEvaluationAsync(Guid staffEvaluationId, FinalizeOppeStaffEvaluationDto request, CancellationToken cancellationToken = default);
        Task<OppeStaffEvaluationDto> AcknowledgeStaffEvaluationAsync(Guid staffEvaluationId, AcknowledgeOppeStaffEvaluationDto request, CancellationToken cancellationToken = default);
    }
}
