using System;
using System.Threading;
using System.Threading.Tasks;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services
{
    public interface IHrdEmploymentLifecycleSchedulerService
    {
        Task<int> ExecuteDailyTickAsync(
            DateOnly? testDate = null,
            CancellationToken cancellationToken = default);
    }
}
