using System;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Services
{
    /// <summary>
    /// Antarmuka kalkulasi rekomendasi kebutuhan tenaga kerja (BE-HRD-035).
    /// Mengimplementasikan rumus perhitungan 3 metode sesuai Backend Architecture Bagian 16.9.
    /// </summary>
    public interface IHrdStaffingRecommendationService
    {
        Task<StaffingRecommendationResponse> CalculateRecommendationAsync(
            HrdStaffingStandard standard,
            DateOnly periodStartDate,
            CancellationToken cancellationToken = default);
    }
}
