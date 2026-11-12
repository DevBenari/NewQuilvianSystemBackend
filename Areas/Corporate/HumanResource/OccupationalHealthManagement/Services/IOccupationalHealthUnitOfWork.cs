using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Services
{
    /// <summary>
    /// Kontrak Unit of Work & Data Access terisolasi untuk modul K3RS & Occupational Health (BE-HRD-287)
    /// Menjamin transaksi atomik ACID, isolasi query terfilter soft-delete, dan penanganan konkurensi optimistik.
    /// </summary>
    public interface IOccupationalHealthUnitOfWork : IDisposable
    {
        IQueryable<WfpHealthRecord> HealthRecords { get; }
        IQueryable<TrxEmployeeMedicalExamination> MedicalExaminations { get; }
        IQueryable<TrxEmployeeFitnessToWork> FitnessToWorks { get; }
        IQueryable<TrxWorkRestriction> WorkRestrictions { get; }
        IQueryable<TrxReturnToWorkAssessment> ReturnToWorkAssessments { get; }
        IQueryable<TrxEmployeeVaccination> Vaccinations { get; }
        IQueryable<TrxEmployeeHealthSurveillance> HealthSurveillances { get; }
        IQueryable<TrxNeedleStickIncident> NeedleStickIncidents { get; }
        IQueryable<TrxOccupationalExposure> OccupationalExposures { get; }
        IQueryable<TrxEmployeeInjury> EmployeeInjuries { get; }
        IQueryable<MstEmployee> Employees { get; }
        IQueryable<MstWorkforceProfile> WorkforceProfiles { get; }

        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(IDbContextTransaction transaction, CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(IDbContextTransaction transaction, CancellationToken cancellationToken = default);

        Task AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class;
        void Update<TEntity>(TEntity entity) where TEntity : class;
        void SoftDelete<TEntity>(TEntity entity, Guid deletedBy) where TEntity : class;
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
