using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Services
{
    /// <summary>
    /// Kontrak Service Siklus Kinerja & Master Metrik KPI Rumah Sakit (BE-HRD-257, PRF-B1, VR-PRF-01)
    /// </summary>
    public interface IPerformanceCycleService
    {
        Task<List<PerformanceCycleDto>> GetCyclesAsync(CancellationToken cancellationToken = default);
        Task<PerformanceCycleDto> GetCycleByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<PerformanceCycleDto> CreateCycleAsync(CreatePerformanceCycleRequestDto dto, Guid createdBy, CancellationToken cancellationToken = default);
        Task<bool> TransitionCycleStatusAsync(Guid cycleId, string newStatus, Guid updatedBy, CancellationToken cancellationToken = default);

        Task<List<KpiMetricCatalogDto>> GetKpiCatalogsAsync(string? category = null, string? targetProfession = null, CancellationToken cancellationToken = default);
        Task<KpiMetricCatalogDto> CreateKpiCatalogAsync(CreateKpiMetricCatalogRequestDto dto, Guid createdBy, CancellationToken cancellationToken = default);
    }
}
