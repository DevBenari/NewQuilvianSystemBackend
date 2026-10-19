using System;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

/// <summary>
/// Kontrak layanan peninjauan item tagihan yang keliru di-void oleh cacat rekonsiliasi lama (BE-BKC-088, BKC-DES-089, FR-BKC-284).
/// </summary>
public interface IBillingSupersededItemReviewService
{
    /// <summary>
    /// Mengambil daftar kandidat item VOIDED dengan kode SUPERSEDED_BY_INPATIENT_ADMISSION beserta status keputusannya.
    /// </summary>
    Task<PagedResult<SupersededItemReviewCandidateResponse>> GetCandidatesAsync(
        SupersededItemReviewListQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Mengambil rincian satu kandidat item tinjauan berdasarkan ID item.
    /// </summary>
    Task<SupersededItemReviewCandidateResponse> GetCandidateByIdAsync(
        Guid invoiceItemId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Menetapkan keputusan final Supervisor Kasir atas satu item (REINSTATE, KEEP, atau NEEDS_POST_CLOSE_CORRECTION).
    /// </summary>
    Task<SupersededItemReviewResponse> DecideAsync(
        Guid invoiceItemId,
        Guid idempotencyKey,
        DecideSupersededItemRequest request,
        Guid actorUserId,
        CancellationToken cancellationToken = default);
}
