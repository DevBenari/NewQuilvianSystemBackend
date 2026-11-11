namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Services
{
    public interface IExitClearanceStatusProvider
    {
        Task<bool> IsFinalPayrollUnlockedAsync(Guid employeeId, CancellationToken cancellationToken = default);
        Task<List<string>> GetPendingClearanceDepartmentsAsync(Guid employeeId, CancellationToken cancellationToken = default);
    }
}
