using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Models;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Services
{
    /// <summary>
    /// Implementasi Unit of Work & Data Access terisolasi untuk modul K3RS & Occupational Health (BE-HRD-287)
    /// </summary>
    public class OccupationalHealthUnitOfWork : IOccupationalHealthUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private bool _disposed;

        public OccupationalHealthUnitOfWork(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IQueryable<WfpHealthRecord> HealthRecords =>
            _context.WfpHealthRecords.AsNoTracking().Where(x => !x.IsDelete);

        public IQueryable<TrxEmployeeMedicalExamination> MedicalExaminations =>
            _context.TrxEmployeeMedicalExaminations.AsNoTracking().Where(x => !x.IsDelete);

        public IQueryable<TrxEmployeeFitnessToWork> FitnessToWorks =>
            _context.TrxEmployeeFitnessToWorks.AsNoTracking().Where(x => !x.IsDelete);

        public IQueryable<TrxWorkRestriction> WorkRestrictions =>
            _context.TrxWorkRestrictions.AsNoTracking().Where(x => !x.IsDelete);

        public IQueryable<TrxReturnToWorkAssessment> ReturnToWorkAssessments =>
            _context.TrxReturnToWorkAssessments.AsNoTracking().Where(x => !x.IsDelete);

        public IQueryable<TrxEmployeeVaccination> Vaccinations =>
            _context.TrxEmployeeVaccinations.AsNoTracking().Where(x => !x.IsDelete);

        public IQueryable<TrxEmployeeHealthSurveillance> HealthSurveillances =>
            _context.TrxEmployeeHealthSurveillances.AsNoTracking().Where(x => !x.IsDelete);

        public IQueryable<TrxNeedleStickIncident> NeedleStickIncidents =>
            _context.TrxNeedleStickIncidents.AsNoTracking().Where(x => !x.IsDelete);

        public IQueryable<TrxOccupationalExposure> OccupationalExposures =>
            _context.TrxOccupationalExposures.AsNoTracking().Where(x => !x.IsDelete);

        public IQueryable<TrxEmployeeInjury> EmployeeInjuries =>
            _context.TrxEmployeeInjuries.AsNoTracking().Where(x => !x.IsDelete);

        public IQueryable<MstEmployee> Employees =>
            _context.MstEmployees.AsNoTracking().Where(x => !x.IsDelete);

        public IQueryable<MstWorkforceProfile> WorkforceProfiles =>
            _context.MstWorkforceProfiles.AsNoTracking().Where(x => !x.IsDelete);

        public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(IDbContextTransaction transaction, CancellationToken cancellationToken = default)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }

        public async Task RollbackTransactionAsync(IDbContextTransaction transaction, CancellationToken cancellationToken = default)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));
            await transaction.RollbackAsync(cancellationToken);
        }

        public async Task AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class
        {
            await _context.Set<TEntity>().AddAsync(entity, cancellationToken);
        }

        public void Update<TEntity>(TEntity entity) where TEntity : class
        {
            _context.Set<TEntity>().Update(entity);
        }

        public void SoftDelete<TEntity>(TEntity entity, Guid deletedBy) where TEntity : class
        {
            if (entity is IdentityModel identity)
            {
                identity.IsDelete = true;
                identity.DeleteDateTime = DateTime.UtcNow;
                identity.DeleteBy = deletedBy;
                _context.Set<TEntity>().Update(entity);
            }
            else
            {
                _context.Set<TEntity>().Remove(entity);
            }
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Context lifecycle is managed by DI container
                }
                _disposed = true;
            }
        }
    }
}
