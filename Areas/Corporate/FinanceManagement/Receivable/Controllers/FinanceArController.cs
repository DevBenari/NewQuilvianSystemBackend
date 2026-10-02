using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.DTOs;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Controllers;

/// <summary>
/// Endpoint API Finance Management V2 - Accounts Receivable (AR).
/// Menyediakan operasi pengelolaan piutang: listing, detail, direct payment, direct write-off, aging, dan report summary.
/// </summary>
[ApiController]
[Authorize]
[Route("api/finance/receivable")]
[AccessController("FINANCE_AR", "Finance AR", "Finance.AR",
    AreaName = "Corporate", ControllerName = "Finance.AR", Description = "Manajemen Piutang Finance V2", SortOrder = 31)]
[Tags("Corporate / Finance Management / AR V2")]
public sealed class FinanceArController : ControllerBase
{
    private readonly FinanceReceivableService _service;

    public FinanceArController(FinanceReceivableService service) => _service = service;

    [HttpGet]
    [AccessAction("View", "View AR", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("Finance.AR", "View")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ReceivableResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] ReceivableQuery request, CancellationToken cancellationToken)
    {
        var result = await _service.GetPagedAsync(request, cancellationToken);
        return Ok(ApiResponse<PagedResult<ReceivableResponse>>.Ok(result, "Daftar piutang berhasil diambil."));
    }

    [HttpGet("{id:guid}")]
    [AccessAction("View", "View AR", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("Finance.AR", "View")]
    [ProducesResponseType(typeof(ApiResponse<ReceivableDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.GetByIdAsync(id, cancellationToken);
            return Ok(ApiResponse<ReceivableDetailResponse>.Ok(result, "Detail piutang berhasil diambil."));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<object>.Fail(404, exception.Message));
        }
    }

    [HttpPost("payment")]
    [AccessAction("Payment", "Payment AR", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("Finance.AR", "Payment")]
    [ProducesResponseType(typeof(ApiResponse<ReceivablePaymentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RecordPayment([FromBody] RecordReceivablePaymentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.RecordPaymentAsync(
                request.ReceivableId,
                request.Amount,
                request.PaymentMethod,
                request.ReferenceNumber,
                request.Notes,
                CurrentUserId(),
                cancellationToken);
            return Ok(ApiResponse<ReceivablePaymentResponse>.Ok(result, "Pembayaran piutang berhasil dicatat."));
        }
        catch (Exception exception) when (IsHandled(exception))
        {
            return Failure(exception);
        }
    }

    [HttpPost("writeoff")]
    [AccessAction("Create", "Create AR WriteOff", AccessType = AccessTypes.Create, SortOrder = 3)]
    [AccessPermission("Finance.AR", "Create")]
    [ProducesResponseType(typeof(ApiResponse<ReceivableWriteOffResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> DirectWriteOff([FromBody] DirectReceivableWriteOffRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.DirectWriteOffAsync(
                request.ReceivableId,
                request.Amount,
                request.Reason,
                CurrentUserId(),
                cancellationToken);
            return Ok(ApiResponse<ReceivableWriteOffResponse>.Ok(
                FinanceReceivableService.MapWriteOff(result),
                "Penghapusan piutang (write-off) berhasil dicatat."));
        }
        catch (Exception exception) when (IsHandled(exception))
        {
            return Failure(exception);
        }
    }

    [HttpGet("aging")]
    [AccessAction("View", "View AR", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("Finance.AR", "View")]
    [ProducesResponseType(typeof(ApiResponse<List<ReceivableAgingBucketResult>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAging([FromQuery] ReceivableAgingQuery request, CancellationToken cancellationToken)
    {
        var result = await _service.GetAgingSummaryAsync(request.AsOfDate, cancellationToken);
        return Ok(ApiResponse<List<ReceivableAgingBucketResult>>.Ok(result, "Rekap umur piutang (aging) berhasil diambil."));
    }

    [HttpGet("report")]
    [AccessAction("View", "View AR", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("Finance.AR", "View")]
    [ProducesResponseType(typeof(ApiResponse<ReceivableReportResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReport([FromQuery] DateOnly? asOfDate, CancellationToken cancellationToken)
    {
        var result = await _service.GetReportSummaryAsync(asOfDate, cancellationToken);
        return Ok(ApiResponse<ReceivableReportResponse>.Ok(result, "Laporan ringkasan piutang berhasil diambil."));
    }

    private IActionResult Failure(Exception exception) => exception switch
    {
        KeyNotFoundException => NotFound(ApiResponse<object>.Fail(404, exception.Message)),
        ReceivableConflictException => Conflict(ApiResponse<object>.Fail(409, exception.Message)),
        ReceivableValidationException => UnprocessableEntity(ApiResponse<object>.Fail(422, exception.Message)),
        ReceivableBadRequestException => BadRequest(ApiResponse<object>.Fail(400, exception.Message)),
        _ => throw exception
    };

    private static bool IsHandled(Exception exception) => exception is
        KeyNotFoundException or ReceivableConflictException or ReceivableValidationException or ReceivableBadRequestException;

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }
}
