using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Services
{
    /// <summary>
    /// Kontrak Unit of Work & Data Access terisolasi untuk modul Performance Management & Clinical Career (BE-HRD-255)
    /// Menjamin transaksi atomik ACID, isolasi query terfilter soft-delete, dan penanganan konkurensi optimistik.
    /// </summary>
    public interface IPerformanceUnitOfWork : IDisposable
    {
        IQueryable<HrdPerformanceCycle> PerformanceCycles { get; }
        IQueryable<HrdPerformanceGoal> PerformanceGoals { get; }
        IQueryable<HrdPerformanceAppraisal> PerformanceAppraisals { get; }
        IQueryable<HrdPerformanceAppraisalDetail> PerformanceAppraisalDetails { get; }
        IQueryable<HrdPerformanceCalibrationSession> PerformanceCalibrationSessions { get; }
        IQueryable<HrdPerformanceImprovementPlan> PerformanceImprovementPlans { get; }
        IQueryable<HrdPerformancePipActionLog> PerformancePipActionLogs { get; }
        IQueryable<HrdClinicalCareerLadderReview> ClinicalCareerLadderReviews { get; }
        IQueryable<HrdPerformanceAppraisalDispute> PerformanceAppraisalDisputes { get; }
        IQueryable<MstKpiMetricCatalog> KpiMetricCatalogs { get; }

        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(IDbContextTransaction transaction, CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(IDbContextTransaction transaction, CancellationToken cancellationToken = default);

        Task AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class;
        void Update<TEntity>(TEntity entity) where TEntity : class;
        void SoftDelete<TEntity>(TEntity entity, Guid deletedBy) where TEntity : class;
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
