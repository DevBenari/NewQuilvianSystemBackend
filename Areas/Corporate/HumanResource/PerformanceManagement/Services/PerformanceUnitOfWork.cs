using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Services
{
    /// <summary>
    /// Implementasi Unit of Work & Data Access terisolasi untuk modul Performance Management & Clinical Career (BE-HRD-255)
    /// </summary>
    public class PerformanceUnitOfWork : IPerformanceUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private bool _disposed;

        public PerformanceUnitOfWork(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IQueryable<HrdPerformanceCycle> PerformanceCycles => 
            _context.HrdPerformanceCycles.AsNoTracking().Where(x => !x.IsDelete);

        public IQueryable<HrdPerformanceGoal> PerformanceGoals => 
            _context.HrdPerformanceGoals.AsNoTracking().Where(x => !x.IsDelete);

        public IQueryable<HrdPerformanceAppraisal> PerformanceAppraisals => 
            _context.HrdPerformanceAppraisals.AsNoTracking().Where(x => !x.IsDelete);

        public IQueryable<HrdPerformanceAppraisalDetail> PerformanceAppraisalDetails => 
            _context.HrdPerformanceAppraisalDetails.AsNoTracking().Where(x => !x.IsDelete);

        public IQueryable<HrdPerformanceCalibrationSession> PerformanceCalibrationSessions => 
            _context.HrdPerformanceCalibrationSessions.AsNoTracking().Where(x => !x.IsDelete);

        public IQueryable<HrdPerformanceImprovementPlan> PerformanceImprovementPlans => 
            _context.HrdPerformanceImprovementPlans.AsNoTracking().Where(x => !x.IsDelete);

        public IQueryable<HrdPerformancePipActionLog> PerformancePipActionLogs => 
            _context.HrdPerformancePipActionLogs.AsNoTracking().Where(x => !x.IsDelete);

        public IQueryable<HrdClinicalCareerLadderReview> ClinicalCareerLadderReviews => 
            _context.HrdClinicalCareerLadderReviews.AsNoTracking().Where(x => !x.IsDelete);

        public IQueryable<HrdPerformanceAppraisalDispute> PerformanceAppraisalDisputes => 
            _context.HrdPerformanceAppraisalDisputes.AsNoTracking().Where(x => !x.IsDelete);

        public IQueryable<MstKpiMetricCatalog> KpiMetricCatalogs => 
            _context.MstKpiMetricCatalogs.AsNoTracking().Where(x => !x.IsDelete);

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
