using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Controllers;

/// <summary>
/// BE-FIN-099, FIN-API-1.9 O.3. Aggregate ber-lifecycle (DRAF→DITERBITKAN/DIBATALKAN) —
/// preview MUST NOT menuntut hak Create (L.5.13); post dan cancel memakai RowVersion eksplisit.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/finance-management/benefit-settlements")]
[AccessController("CORPORATE_FINANCE_MANAGEMENT_BENEFIT_SETTLEMENT", "Corporate Finance Management Benefit Settlement", "Benefit Settlement",
    AreaName = "Corporate", ControllerName = "FinanceBenefitSettlement", Description = "Pelunasan internal porsi benefit rumah sakit", SortOrder = 35)]
[Tags("Corporate / Finance Management / Receivable / Benefit Settlement")]
public sealed class FinanceBenefitSettlementsController : ControllerBase
{
    private readonly FinanceBenefitSettlementService _service;
    public FinanceBenefitSettlementsController(FinanceBenefitSettlementService service) => _service = service;

    [HttpPost("preview")]
    [AccessAction("Read", "Read Benefit Settlement", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceBenefitSettlement", "Read")]
    [ProducesResponseType(typeof(ApiResponse<BenefitSettlementPreviewResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Preview([FromBody] PreviewBenefitSettlementRequest request, CancellationToken cancellationToken) =>
        Ok(ApiResponse<BenefitSettlementPreviewResponse>.Ok(
            await _service.PreviewAsync(request, cancellationToken), "Hitung awal pelunasan berhasil diambil."));

    [HttpGet]
    [AccessAction("Read", "Read Benefit Settlement", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceBenefitSettlement", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<BenefitSettlementListResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaged([FromQuery] BenefitSettlementQuery request, CancellationToken cancellationToken) =>
        Ok(ApiResponse<PagedResult<BenefitSettlementListResponse>>.Ok(
            await _service.GetPagedAsync(request, cancellationToken), "Daftar pelunasan internal berhasil diambil."));

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Benefit Settlement", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceBenefitSettlement", "Read")]
    [ProducesResponseType(typeof(ApiResponse<BenefitSettlementDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDetail(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(ApiResponse<BenefitSettlementDetailResponse>.Ok(
                await _service.GetDetailAsync(id, cancellationToken), "Rincian pelunasan berhasil diambil."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpPost]
    [AccessAction("Create", "Create Benefit Settlement", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("FinanceBenefitSettlement", "Create")]
    [ProducesResponseType(typeof(ApiResponse<BenefitSettlementDetailResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreateBenefitSettlementRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.CreateAsync(request, CurrentUserId(), cancellationToken);
            return StatusCode(StatusCodes.Status201Created, ApiResponse<BenefitSettlementDetailResponse>.Ok(result, "Draf pelunasan internal berhasil dibuat."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpPost("{id:guid}/post")]
    [AccessAction("Post", "Post Benefit Settlement", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("FinanceBenefitSettlement", "Post")]
    [ProducesResponseType(typeof(ApiResponse<BenefitSettlementDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Post(Guid id, [FromBody] PostBenefitSettlementRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.PostAsync(id, request, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<BenefitSettlementDetailResponse>.Ok(result, "Pelunasan internal berhasil diterbitkan — seluruh kartu piutang ditutup."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpPost("{id:guid}/cancel")]
    [AccessAction("Cancel", "Cancel Benefit Settlement", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("FinanceBenefitSettlement", "Cancel")]
    [ProducesResponseType(typeof(ApiResponse<BenefitSettlementResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelBenefitSettlementRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.CancelAsync(id, request, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<BenefitSettlementResponse>.Ok(result, "Pelunasan internal berhasil dibatalkan."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    private IActionResult Failure(Exception exception) => exception switch
    {
        KeyNotFoundException => NotFound(ApiResponse<object>.Fail(404, exception.Message)),
        BenefitSettlementConflictException => Conflict(ApiResponse<object>.Fail(409, exception.Message)),
        BenefitSettlementValidationException => UnprocessableEntity(ApiResponse<object>.Fail(422, exception.Message)),
        _ => throw exception
    };

    private static bool IsHandled(Exception exception) =>
        exception is KeyNotFoundException or BenefitSettlementConflictException or BenefitSettlementValidationException;

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }
}
