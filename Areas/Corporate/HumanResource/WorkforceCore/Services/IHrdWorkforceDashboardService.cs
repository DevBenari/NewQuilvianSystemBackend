using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services
{
    /// <summary>
    /// Dashboard SDM baca-saja (kontrak v30 bagian 36.1, <c>INT-HR-115</c> s.d. <c>INT-HR-117</c>). Seluruh angka
    /// dihitung saat dipanggil dari data pemilik masing-masing; tidak menulis ke modul sumber dan tidak menyimpan salinan.
    /// </summary>
    public interface IHrdWorkforceDashboardService
    {
        Task<WorkforceDashboardSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken = default);

        Task<WorkforceDashboardDrilldownResult> GetDrilldownAsync(
            WorkforceDashboardDrilldownQuery query,
            CancellationToken cancellationToken = default);
    }

    public class WorkforceDashboardDrilldownResult
    {
        public bool Success { get; private set; }
        public string? Message { get; private set; }
        public PagedResult<WorkforceDashboardDrilldownItem>? Data { get; private set; }

        public static WorkforceDashboardDrilldownResult Ok(PagedResult<WorkforceDashboardDrilldownItem> data) =>
            new() { Success = true, Data = data };

        public static WorkforceDashboardDrilldownResult Invalid(string message) =>
            new() { Success = false, Message = message };
    }
}
