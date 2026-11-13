using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Services
{
    public interface IEmployeeRelationUnitOfWork : IDisposable
    {
        IQueryable<HrdDisciplinaryIncidentReport> IncidentReports { get; }
        IQueryable<HrdDisciplinaryInvestigationRecord> InvestigationRecords { get; }
        IQueryable<HrdDisciplinarySanction> Sanctions { get; }
        IQueryable<HrdDisciplinarySuspension> Suspensions { get; }
        IQueryable<TrxEmployeeGrievance> Grievances { get; }
        IQueryable<HrdBipartiteNegotiationForum> BipartiteForums { get; }
        IQueryable<TrxEmployeeSeparation> Separations { get; }
        IQueryable<HrdSeveranceCalculation> SeveranceCalculations { get; }
        IQueryable<HrdExitClearanceProcess> ExitClearances { get; }
        IQueryable<HrdExitClearanceChecklist> ExitClearanceChecklists { get; }
        IQueryable<MstEmployee> Employees { get; }
        IQueryable<MstWorkforceProfile> WorkforceProfiles { get; }

        /// <summary>
        /// Memetakan email petugas yang terautentikasi ke ID akun pengguna, untuk stempel aktor pada
        /// deret nomor dokumen. Mengembalikan <c>null</c> bila email tidak dikenal.
        /// </summary>
        Task<Guid?> FindUserIdByEmailAsync(string? email, CancellationToken cancellationToken = default);

        Task AddIncidentReportAsync(HrdDisciplinaryIncidentReport incidentReport, CancellationToken cancellationToken = default);
        Task AddInvestigationRecordAsync(HrdDisciplinaryInvestigationRecord record, CancellationToken cancellationToken = default);
        Task AddSanctionAsync(HrdDisciplinarySanction sanction, CancellationToken cancellationToken = default);
        Task AddSuspensionAsync(HrdDisciplinarySuspension suspension, CancellationToken cancellationToken = default);
        Task AddGrievanceAsync(TrxEmployeeGrievance grievance, CancellationToken cancellationToken = default);
        Task AddBipartiteForumAsync(HrdBipartiteNegotiationForum forum, CancellationToken cancellationToken = default);
        Task AddSeparationAsync(TrxEmployeeSeparation separation, CancellationToken cancellationToken = default);
        Task AddSeveranceCalculationAsync(HrdSeveranceCalculation calculation, CancellationToken cancellationToken = default);
        Task AddExitClearanceProcessAsync(HrdExitClearanceProcess process, CancellationToken cancellationToken = default);
        Task AddExitClearanceChecklistAsync(HrdExitClearanceChecklist checklist, CancellationToken cancellationToken = default);
        Task AddExitClearanceChecklistsRangeAsync(IEnumerable<HrdExitClearanceChecklist> checklists, CancellationToken cancellationToken = default);

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    }
}
