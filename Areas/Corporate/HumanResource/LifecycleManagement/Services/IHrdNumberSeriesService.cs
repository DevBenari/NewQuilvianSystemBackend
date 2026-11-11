using System;
using System.Threading;
using System.Threading.Tasks;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services
{
    public interface IHrdNumberSeriesService
    {
        Task<string> GenerateOnboardingCaseNumberAsync(CancellationToken cancellationToken = default);
        Task<string> GenerateContractNumberAsync(CancellationToken cancellationToken = default);
        Task<string> GenerateAssessmentNumberAsync(CancellationToken cancellationToken = default);
        Task<string> GenerateDecisionNumberAsync(CancellationToken cancellationToken = default);
        Task<string> GenerateCorrectionNumberAsync(CancellationToken cancellationToken = default);
        Task<string> GenerateBatchNumberAsync(CancellationToken cancellationToken = default);
    }
}
