using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Services
{
    public class EmployeeRelationUnitOfWork : IEmployeeRelationUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public EmployeeRelationUnitOfWork(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IQueryable<TrxDisciplinaryIncidentReport> IncidentReports => _context.TrxDisciplinaryIncidentReports;
        public IQueryable<TrxDisciplinaryInvestigationRecord> InvestigationRecords => _context.TrxDisciplinaryInvestigationRecords;
        public IQueryable<TrxDisciplinarySanction> Sanctions => _context.TrxDisciplinarySanctions;
        public IQueryable<TrxDisciplinarySuspension> Suspensions => _context.TrxDisciplinarySuspensions;
        public IQueryable<TrxEmployeeGrievance> Grievances => _context.TrxEmployeeGrievances;
        public IQueryable<TrxBipartiteNegotiationForum> BipartiteForums => _context.TrxBipartiteNegotiationForums;
        public IQueryable<TrxEmployeeSeparation> Separations => _context.TrxEmployeeSeparations;
        public IQueryable<TrxSeveranceCalculation> SeveranceCalculations => _context.TrxSeveranceCalculations;
        public IQueryable<TrxExitClearanceProcess> ExitClearances => _context.TrxExitClearanceProcesses;
        public IQueryable<TrxExitClearanceChecklist> ExitClearanceChecklists => _context.TrxExitClearanceChecklists;
        public IQueryable<MstEmployee> Employees => _context.MstEmployees;
        public IQueryable<MstWorkforceProfile> WorkforceProfiles => _context.MstWorkforceProfiles;

        public async Task AddIncidentReportAsync(TrxDisciplinaryIncidentReport incidentReport, CancellationToken cancellationToken = default)
        {
            await _context.TrxDisciplinaryIncidentReports.AddAsync(incidentReport, cancellationToken);
        }

        public async Task AddInvestigationRecordAsync(TrxDisciplinaryInvestigationRecord record, CancellationToken cancellationToken = default)
        {
            await _context.TrxDisciplinaryInvestigationRecords.AddAsync(record, cancellationToken);
        }

        public async Task AddSanctionAsync(TrxDisciplinarySanction sanction, CancellationToken cancellationToken = default)
        {
            await _context.TrxDisciplinarySanctions.AddAsync(sanction, cancellationToken);
        }

        public async Task AddSuspensionAsync(TrxDisciplinarySuspension suspension, CancellationToken cancellationToken = default)
        {
            await _context.TrxDisciplinarySuspensions.AddAsync(suspension, cancellationToken);
        }

        public async Task AddGrievanceAsync(TrxEmployeeGrievance grievance, CancellationToken cancellationToken = default)
        {
            await _context.TrxEmployeeGrievances.AddAsync(grievance, cancellationToken);
        }

        public async Task AddBipartiteForumAsync(TrxBipartiteNegotiationForum forum, CancellationToken cancellationToken = default)
        {
            await _context.TrxBipartiteNegotiationForums.AddAsync(forum, cancellationToken);
        }

        public async Task AddSeparationAsync(TrxEmployeeSeparation separation, CancellationToken cancellationToken = default)
        {
            await _context.TrxEmployeeSeparations.AddAsync(separation, cancellationToken);
        }

        public async Task AddSeveranceCalculationAsync(TrxSeveranceCalculation calculation, CancellationToken cancellationToken = default)
        {
            await _context.TrxSeveranceCalculations.AddAsync(calculation, cancellationToken);
        }

        public async Task AddExitClearanceProcessAsync(TrxExitClearanceProcess process, CancellationToken cancellationToken = default)
        {
            await _context.TrxExitClearanceProcesses.AddAsync(process, cancellationToken);
        }

        public async Task AddExitClearanceChecklistAsync(TrxExitClearanceChecklist checklist, CancellationToken cancellationToken = default)
        {
            await _context.TrxExitClearanceChecklists.AddAsync(checklist, cancellationToken);
        }

        public async Task AddExitClearanceChecklistsRangeAsync(IEnumerable<TrxExitClearanceChecklist> checklists, CancellationToken cancellationToken = default)
        {
            await _context.TrxExitClearanceChecklists.AddRangeAsync(checklists, cancellationToken);
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public void Dispose()
        {
            // ApplicationDbContext is managed by DI container
        }
    }
}
