using System;
using System.Threading;
using System.Threading.Tasks;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services
{
    public interface IWfpNewHireAssignmentService
    {
        Task AssignNewHirePlacementAndSalaryAsync(
            Guid workforceProfileId,
            Guid hospitalSiteId,
            Guid organizationUnitId,
            Guid positionId,
            Guid salaryGradeId,
            decimal basicSalary,
            DateTime startDate,
            Guid userId,
            CancellationToken cancellationToken = default);
    }
}
