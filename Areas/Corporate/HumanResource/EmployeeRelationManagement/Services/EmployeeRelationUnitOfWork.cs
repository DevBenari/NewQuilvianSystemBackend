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

        public IQueryable<HrdDisciplinaryIncidentReport> IncidentReports => _context.HrdDisciplinaryIncidentReports;
        public IQueryable<HrdDisciplinaryInvestigationRecord> InvestigationRecords => _context.HrdDisciplinaryInvestigationRecords;
        public IQueryable<HrdDisciplinarySanction> Sanctions => _context.HrdDisciplinarySanctions;
        public IQueryable<HrdDisciplinarySuspension> Suspensions => _context.HrdDisciplinarySuspensions;
        public IQueryable<TrxEmployeeGrievance> Grievances => _context.TrxEmployeeGrievances;
        public IQueryable<HrdBipartiteNegotiationForum> BipartiteForums => _context.HrdBipartiteNegotiationForums;
        public IQueryable<TrxEmployeeSeparation> Separations => _context.TrxEmployeeSeparations;
        public IQueryable<HrdSeveranceCalculation> SeveranceCalculations => _context.HrdSeveranceCalculations;
        public IQueryable<HrdExitClearanceProcess> ExitClearances => _context.HrdExitClearanceProcesses;
        public IQueryable<HrdExitClearanceChecklist> ExitClearanceChecklists => _context.HrdExitClearanceChecklists;
        public IQueryable<MstEmployee> Employees => _context.MstEmployees;
        public IQueryable<MstWorkforceProfile> WorkforceProfiles => _context.MstWorkforceProfiles;

        public async Task<Guid?> FindUserIdByEmailAsync(string? email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            var normalized = email.Trim().ToUpperInvariant();
            var userId = await _context.Users.AsNoTracking()
                .Where(u => u.NormalizedEmail == normalized || u.NormalizedUserName == normalized)
                .Select(u => (Guid?)u.Id)
                .FirstOrDefaultAsync(cancellationToken);
            return userId;
        }

        public async Task AddIncidentReportAsync(HrdDisciplinaryIncidentReport incidentReport, CancellationToken cancellationToken = default)
        {
            await _context.HrdDisciplinaryIncidentReports.AddAsync(incidentReport, cancellationToken);
        }

        public async Task AddInvestigationRecordAsync(HrdDisciplinaryInvestigationRecord record, CancellationToken cancellationToken = default)
        {
            await _context.HrdDisciplinaryInvestigationRecords.AddAsync(record, cancellationToken);
        }

        public async Task AddSanctionAsync(HrdDisciplinarySanction sanction, CancellationToken cancellationToken = default)
        {
            await _context.HrdDisciplinarySanctions.AddAsync(sanction, cancellationToken);
        }

        public async Task AddSuspensionAsync(HrdDisciplinarySuspension suspension, CancellationToken cancellationToken = default)
        {
            await _context.HrdDisciplinarySuspensions.AddAsync(suspension, cancellationToken);
        }

        public async Task AddGrievanceAsync(TrxEmployeeGrievance grievance, CancellationToken cancellationToken = default)
        {
            await _context.TrxEmployeeGrievances.AddAsync(grievance, cancellationToken);
        }

        public async Task AddBipartiteForumAsync(HrdBipartiteNegotiationForum forum, CancellationToken cancellationToken = default)
        {
            await _context.HrdBipartiteNegotiationForums.AddAsync(forum, cancellationToken);
        }

        public async Task AddSeparationAsync(TrxEmployeeSeparation separation, CancellationToken cancellationToken = default)
        {
            await _context.TrxEmployeeSeparations.AddAsync(separation, cancellationToken);
        }

        public async Task AddSeveranceCalculationAsync(HrdSeveranceCalculation calculation, CancellationToken cancellationToken = default)
        {
            await _context.HrdSeveranceCalculations.AddAsync(calculation, cancellationToken);
        }

        public async Task AddExitClearanceProcessAsync(HrdExitClearanceProcess process, CancellationToken cancellationToken = default)
        {
            await _context.HrdExitClearanceProcesses.AddAsync(process, cancellationToken);
        }

        public async Task AddExitClearanceChecklistAsync(HrdExitClearanceChecklist checklist, CancellationToken cancellationToken = default)
        {
            await _context.HrdExitClearanceChecklists.AddAsync(checklist, cancellationToken);
        }

        public async Task AddExitClearanceChecklistsRangeAsync(IEnumerable<HrdExitClearanceChecklist> checklists, CancellationToken cancellationToken = default)
        {
            await _context.HrdExitClearanceChecklists.AddRangeAsync(checklists, cancellationToken);
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
