using System;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Services
{
    public interface IEmployeeEmploymentRecordService
    {
        Task SyncFromContractAsync(WfpContractHistory contract, Guid userId, CancellationToken cancellationToken = default);
    }
}
