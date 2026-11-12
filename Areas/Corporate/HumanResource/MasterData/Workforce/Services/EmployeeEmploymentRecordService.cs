using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Services
{
    public class EmployeeEmploymentRecordService : IEmployeeEmploymentRecordService
    {
        private readonly ApplicationDbContext _dbContext;

        public EmployeeEmploymentRecordService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task SyncFromContractAsync(
            WfpContractHistory contract,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            if (contract == null) return;

            var employee = await _dbContext.Set<MstEmployee>()
                .FirstOrDefaultAsync(e => e.WorkforceProfileId == contract.WorkforceProfileId && !e.IsDelete, cancellationToken);

            if (employee != null)
            {
                employee.ContractTypeId = contract.ContractTypeId;
                if (contract.EmploymentTypeId.HasValue)
                {
                    employee.EmploymentTypeId = contract.EmploymentTypeId.Value;
                }
                employee.ContractStartDate = contract.StartDate;
                employee.ContractEndDate = contract.EndDate;
                employee.ProbationEndDate = contract.ProbationEndDate;

                if (contract.FixedTermSeriesStartDate.HasValue)
                {
                    employee.JoinDate = contract.FixedTermSeriesStartDate.Value;
                }
                else if (contract.HistoryType == "Initial")
                {
                    employee.JoinDate = contract.StartDate;
                }

                employee.UpdateDateTime = DateTime.UtcNow;
                employee.UpdateBy = userId;
            }

            var doctor = await _dbContext.Set<MstDoctor>()
                .FirstOrDefaultAsync(d => d.WorkforceProfileId == contract.WorkforceProfileId && !d.IsDelete, cancellationToken);

            if (doctor != null)
            {
                doctor.ContractTypeId = contract.ContractTypeId;
                if (contract.EmploymentTypeId.HasValue)
                {
                    doctor.EmploymentTypeId = contract.EmploymentTypeId.Value;
                }
                doctor.ContractStartDate = contract.StartDate;
                doctor.ContractEndDate = contract.EndDate;
                doctor.ProbationEndDate = contract.ProbationEndDate;

                if (contract.FixedTermSeriesStartDate.HasValue)
                {
                    doctor.JoinDate = contract.FixedTermSeriesStartDate.Value;
                }
                else if (contract.HistoryType == "Initial")
                {
                    doctor.JoinDate = contract.StartDate;
                }

                doctor.UpdateDateTime = DateTime.UtcNow;
                doctor.UpdateBy = userId;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
