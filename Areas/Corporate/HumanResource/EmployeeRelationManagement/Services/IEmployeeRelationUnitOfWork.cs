using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Services
{
    public interface IEmployeeRelationUnitOfWork : IDisposable
    {
        IQueryable<TrxDisciplinaryIncidentReport> IncidentReports { get; }
        IQueryable<TrxDisciplinaryInvestigationRecord> InvestigationRecords { get; }
        IQueryable<TrxDisciplinarySanction> Sanctions { get; }
        IQueryable<TrxDisciplinarySuspension> Suspensions { get; }
        IQueryable<TrxEmployeeGrievance> Grievances { get; }
        IQueryable<TrxBipartiteNegotiationForum> BipartiteForums { get; }
        IQueryable<TrxEmployeeSeparation> Separations { get; }
        IQueryable<TrxSeveranceCalculation> SeveranceCalculations { get; }
        IQueryable<TrxExitClearanceProcess> ExitClearances { get; }
        IQueryable<TrxExitClearanceChecklist> ExitClearanceChecklists { get; }
        IQueryable<MstEmployee> Employees { get; }
        IQueryable<MstWorkforceProfile> WorkforceProfiles { get; }

        /// <summary>
        /// Memetakan email petugas yang terautentikasi ke ID akun pengguna, untuk stempel aktor pada
        /// deret nomor dokumen. Mengembalikan <c>null</c> bila email tidak dikenal.
        /// </summary>
        Task<Guid?> FindUserIdByEmailAsync(string? email, CancellationToken cancellationToken = default);

        Task AddIncidentReportAsync(TrxDisciplinaryIncidentReport incidentReport, CancellationToken cancellationToken = default);
        Task AddInvestigationRecordAsync(TrxDisciplinaryInvestigationRecord record, CancellationToken cancellationToken = default);
        Task AddSanctionAsync(TrxDisciplinarySanction sanction, CancellationToken cancellationToken = default);
        Task AddSuspensionAsync(TrxDisciplinarySuspension suspension, CancellationToken cancellationToken = default);
        Task AddGrievanceAsync(TrxEmployeeGrievance grievance, CancellationToken cancellationToken = default);
        Task AddBipartiteForumAsync(TrxBipartiteNegotiationForum forum, CancellationToken cancellationToken = default);
        Task AddSeparationAsync(TrxEmployeeSeparation separation, CancellationToken cancellationToken = default);
        Task AddSeveranceCalculationAsync(TrxSeveranceCalculation calculation, CancellationToken cancellationToken = default);
        Task AddExitClearanceProcessAsync(TrxExitClearanceProcess process, CancellationToken cancellationToken = default);
        Task AddExitClearanceChecklistAsync(TrxExitClearanceChecklist checklist, CancellationToken cancellationToken = default);
        Task AddExitClearanceChecklistsRangeAsync(IEnumerable<TrxExitClearanceChecklist> checklists, CancellationToken cancellationToken = default);

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    }
}
