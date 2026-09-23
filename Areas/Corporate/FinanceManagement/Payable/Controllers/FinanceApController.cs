using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Controllers;

/// <summary>
/// Endpoint API Finance Management V2 - Accounts Payable (AP).
/// Menyediakan operasi pengelolaan utang: listing, detail, direct payment, aging, dan report summary.
/// </summary>
[ApiController]
[Authorize]
[Route("api/finance/payable")]
[AccessController("FINANCE_AP", "Finance AP", "Finance.AP",
    AreaName = "Corporate", ControllerName = "Finance.AP", Description = "Manajemen Utang Finance V2", SortOrder = 32)]
[Tags("Corporate / Finance Management / AP V2")]
public sealed class FinanceApController : ControllerBase
{
    private readonly FinanceSupplierPayableService _service;

    public FinanceApController(FinanceSupplierPayableService service) => _service = service;

    [HttpGet]
    [AccessAction("View", "View AP", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("Finance.AP", "View")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<SupplierPayableResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] SupplierPayableQuery query, CancellationToken cancellationToken)
    {
        var result = await _service.GetPagedAsync(query, cancellationToken);
        var mapped = new PagedResult<SupplierPayableResponse>
        {
            Items = result.Items.Select(FinanceSupplierPayablesController.MapPayable).ToList(),
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        };
        return Ok(ApiResponse<PagedResult<SupplierPayableResponse>>.Ok(mapped, "Daftar utang supplier berhasil diambil."));
    }

    [HttpGet("{id:guid}")]
    [AccessAction("View", "View AP", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("Finance.AP", "View")]
    [ProducesResponseType(typeof(ApiResponse<SupplierPayableDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var payable = await _service.GetByIdAsync(id, cancellationToken);
            return Ok(ApiResponse<SupplierPayableDetailResponse>.Ok(FinanceSupplierPayablesController.Map(payable), "Detail utang supplier berhasil diambil."));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<object>.Fail(404, exception.Message));
        }
    }

    [HttpPost("payment")]
    [AccessAction("Payment", "Payment AP", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("Finance.AP", "Payment")]
    [ProducesResponseType(typeof(ApiResponse<SupplierPayablePaymentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RecordPayment([FromBody] RecordSupplierPaymentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.RecordDirectPaymentAsync(
                request.SupplierPayableId,
                request.Amount,
                request.BankAccountId,
                request.PaymentMethod,
                request.ReferenceNumber,
                request.Notes,
                CurrentUserId(),
                cancellationToken);
            return Ok(ApiResponse<SupplierPayablePaymentResponse>.Ok(result, "Pembayaran utang supplier berhasil dicatat."));
        }
        catch (Exception exception) when (IsHandled(exception))
        {
            return Failure(exception);
        }
    }

    [HttpGet("aging")]
    [AccessAction("View", "View AP", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("Finance.AP", "View")]
    [ProducesResponseType(typeof(ApiResponse<List<SupplierPayableAgingBucketResult>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAging([FromQuery] SupplierPayableAgingQuery query, CancellationToken cancellationToken)
    {
        var result = await _service.GetAgingSummaryAsync(query.AsOfDate, cancellationToken);
        return Ok(ApiResponse<List<SupplierPayableAgingBucketResult>>.Ok(result, "Rekap umur utang supplier (aging) berhasil diambil."));
    }

    [HttpGet("report")]
    [AccessAction("View", "View AP", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("Finance.AP", "View")]
    [ProducesResponseType(typeof(ApiResponse<SupplierPayableReportResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReport([FromQuery] DateOnly? asOfDate, CancellationToken cancellationToken)
    {
        var result = await _service.GetReportSummaryAsync(asOfDate, cancellationToken);
        return Ok(ApiResponse<SupplierPayableReportResponse>.Ok(result, "Laporan ringkasan utang supplier berhasil diambil."));
    }

    private IActionResult Failure(Exception exception) => exception switch
    {
        KeyNotFoundException => NotFound(ApiResponse<object>.Fail(404, exception.Message)),
        PayableConflictException => Conflict(ApiResponse<object>.Fail(409, exception.Message)),
        PayableValidationException => UnprocessableEntity(ApiResponse<object>.Fail(422, exception.Message)),
        PayableBadRequestException => BadRequest(ApiResponse<object>.Fail(400, exception.Message)),
        _ => throw exception
    };

    private static bool IsHandled(Exception exception) => exception is
        KeyNotFoundException or PayableConflictException or PayableValidationException or PayableBadRequestException;

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }
}
