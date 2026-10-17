using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Controllers;

/// <summary>
/// Endpoint peninjauan dan penetapan keputusan void biaya administrasi yang keliru (BE-BKC-088, BKC-DES-089, BIL-API-2.0 Bagian B).
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/health-services/billing-management/billing/superseded-item-reviews")]
[AccessController(
    "HEALTH_SERVICE_BILLING_MANAGEMENT",
    "Health Service Billing Management",
    "Billing Superseded Item Review",
    AreaName = "HealthServices",
    ControllerName = "BillingSupersededItemReview",
    Description = "Supervisor review for superseded administration fee items",
    SortOrder = 10)]
[Tags("Health Services / Billing Management / Billing / Superseded Item Reviews")]
public sealed class BillingSupersededItemReviewsController : ControllerBase
{
    private readonly IBillingSupersededItemReviewService _reviewService;

    public BillingSupersededItemReviewsController(IBillingSupersededItemReviewService reviewService)
    {
        _reviewService = reviewService ?? throw new ArgumentNullException(nameof(reviewService));
    }

    /// <summary>
    /// Mengambil daftar item yang di-void dengan kode SUPERSEDED_BY_INPATIENT_ADMISSION beserta status keputusannya.
    /// </summary>
    [HttpGet]
    [AccessAction("Read", "Read Superseded Item Reviews", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("BillingSupersededItemReview", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<SupersededItemReviewCandidateResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCandidates(
        [FromQuery] SupersededItemReviewListQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _reviewService.GetCandidatesAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<SupersededItemReviewCandidateResponse>>.Ok(
            result, "Daftar kandidat tinjauan void item berhasil diambil."));
    }

    /// <summary>
    /// Mengambil rincian satu kandidat item tinjauan berdasarkan ID item.
    /// </summary>
    [HttpGet("{invoiceItemId:guid}")]
    [AccessAction("Read", "Read Superseded Item Review Detail", AccessType = AccessTypes.Read, SortOrder = 2)]
    [AccessPermission("BillingSupersededItemReview", "Read")]
    [ProducesResponseType(typeof(ApiResponse<SupersededItemReviewCandidateResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCandidateById(
        Guid invoiceItemId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _reviewService.GetCandidateByIdAsync(invoiceItemId, cancellationToken);
            return Ok(ApiResponse<SupersededItemReviewCandidateResponse>.Ok(
                result, "Detail kandidat tinjauan void item berhasil diambil."));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, exception.Message));
        }
    }

    /// <summary>
    /// Menetapkan keputusan final Supervisor Kasir atas satu item yang keliru di-void (REINSTATE, KEEP, atau NEEDS_POST_CLOSE_CORRECTION).
    /// </summary>
    [HttpPost("{invoiceItemId:guid}/decision")]
    [AccessAction("Decide", "Decide Superseded Item Review", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("BillingSupersededItemReview", "Decide")]
    [ProducesResponseType(typeof(ApiResponse<SupersededItemReviewResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<SupersededItemReviewResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Decide(
        [FromRoute] Guid invoiceItemId,
        [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
        [FromBody] DecideSupersededItemRequest request,
        CancellationToken cancellationToken)
    {
        // BIL-VAL-158: Header Idempotency-Key wajib
        if (idempotencyKey == Guid.Empty)
        {
            return BadRequest(ApiResponse<object>.Fail(
                StatusCodes.Status400BadRequest, "Idempotency-Key wajib diisi."));
        }

        try
        {
            var result = await _reviewService.DecideAsync(
                invoiceItemId, idempotencyKey, request, CurrentUserId(), cancellationToken);

            if (result.IsReplay)
            {
                return Ok(ApiResponse<SupersededItemReviewResponse>.Ok(
                    result, "Permintaan replay; keputusan sebelumnya dikembalikan."));
            }

            var successMessage = result.Decision == BilSupersededItemReviewDecisions.Reinstate
                ? "Item dipulihkan. Tagihan akan memuatnya pada kalkulasi berikutnya."
                : "Keputusan tinjauan item berhasil disimpan.";

            return StatusCode(StatusCodes.Status201Created, Success(
                result, StatusCodes.Status201Created, successMessage));
        }
        catch (BillingSupersededItemReviewBadRequestException exception)
        {
            return BadRequest(ApiResponse<object>.Fail(
                StatusCodes.Status400BadRequest, exception.Message));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<object>.Fail(
                StatusCodes.Status404NotFound, exception.Message));
        }
        catch (BillingSupersededItemReviewConflictException exception)
        {
            return Conflict(ApiResponse<object>.Fail(
                StatusCodes.Status409Conflict, exception.Message));
        }
        catch (BillingSupersededItemReviewValidationException exception)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(
                StatusCodes.Status422UnprocessableEntity, exception.Message));
        }
    }

    private static ApiResponse<T> Success<T>(T data, int statusCode, string message) => new()
    {
        Success = true,
        StatusCode = statusCode,
        Message = message,
        Data = data
    };

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }
}
