using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;

public sealed class InpatientBillingQueryService(ApplicationDbContext db, IInpBillingClearanceAdapter clearance)
    : IInpatientBillingQueryService
{
    public async Task<InpatientBillingStatusResponseDto?> GetOperationalBillingStatusAsync(
        Guid episodeId, CancellationToken cancellationToken = default)
    {
        var episode = await db.Set<InpEpisode>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == episodeId && !x.IsDelete, cancellationToken);
        if (episode == null) return null;
        var current = await clearance.GetStatusAsync(episode.EncounterId, cancellationToken);
        return new()
        {
            EpisodeId = episode.Id, EncounterId = episode.EncounterId, IsReadable = current.IsReadable,
            ClearanceStatus = current.Status, Reasons = current.Reasons, EvaluatedAt = current.EvaluatedAt,
            InvoiceStatus = current.InvoiceStatus,
            IsClosedWithoutFinancialClearance = episode.IsClosedWithoutFinancialClearance
        };
    }
}
