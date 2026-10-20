using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.PatientRefund.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.PatientRefund.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.PatientRefund.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/corporate/finance-management/patient-refunds")]
[AccessController("CORPORATE_FINANCE_MANAGEMENT_PATIENT_REFUND", "Corporate Finance Management Patient Refund", "Patient Refund",
    AreaName = "Corporate", ControllerName = "FinancePatientRefund", Description = "Manajemen saldo refund dan antrean pencairan kelebihan pembayaran deposit pasien", SortOrder = 45)]
[Tags("Corporate / Finance Management / Patient Refund")]
public sealed class FinancePatientRefundsController : ControllerBase
{
    private readonly FinancePatientRefundService _service;

    public FinancePatientRefundsController(FinancePatientRefundService service)
    {
        _service = service;
    }

    [HttpGet]
    [AccessAction("Read", "Read Patient Refund List", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinancePatientRefund", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PatientRefundItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] DateTimeOffset? startDate = null,
        [FromQuery] DateTimeOffset? endDate = null,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] string? sourceType = null,
        [FromQuery] decimal? minAmount = null,
        [FromQuery] decimal? maxAmount = null,
        [FromQuery] string? sortField = null,
        [FromQuery] string? sortDirection = null,
        CancellationToken cancellationToken = default)
    {
        var filter = new PatientRefundFilterDto
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            StartDate = startDate,
            EndDate = endDate,
            Search = search,
            Status = status,
            SourceType = sourceType,
            MinAmount = minAmount,
            MaxAmount = maxAmount,
            SortField = sortField,
            SortDirection = sortDirection
        };

        var (items, totalCount) = await _service.GetPagedListAsync(filter, cancellationToken);
        var pagedResult = new PagedResult<PatientRefundItemDto>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = totalCount,
            TotalPage = pageSize > 0 ? (int)Math.Ceiling(totalCount / (double)pageSize) : 0,
            Items = items.ToList()
        };
        return Ok(ApiResponse<PagedResult<PatientRefundItemDto>>.Ok(
            pagedResult, "Daftar saldo refund pasien berhasil diambil."));
    }

    [HttpGet("summary")]
    [AccessAction("Read", "Read Patient Refund Summary", AccessType = AccessTypes.Read, SortOrder = 2)]
    [AccessPermission("FinancePatientRefund", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PatientRefundSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
    {
        var summary = await _service.GetSummaryAsync(cancellationToken);
        return Ok(ApiResponse<PatientRefundSummaryDto>.Ok(summary, "Ringkasan saldo refund pasien berhasil diambil."));
    }

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Patient Refund Detail", AccessType = AccessTypes.Read, SortOrder = 3)]
    [AccessPermission("FinancePatientRefund", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PatientRefundDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.GetByIdAsync(id, cancellationToken);
            return Ok(ApiResponse<PatientRefundDetailDto>.Ok(result, "Detail saldo refund pasien berhasil diambil."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
        }
    }

    [HttpPost]
    [AccessAction("Create", "Submit Patient Refund Request", AccessType = AccessTypes.Create, SortOrder = 4)]
    [AccessPermission("FinancePatientRefund", "Create")]
    [ProducesResponseType(typeof(ApiResponse<PatientRefundDetailDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create(
        [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
        [FromBody] CreatePatientRefundRequestDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            if (idempotencyKey == Guid.Empty)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Header Idempotency-Key wajib diisi."));
            }

            var result = await _service.CreateRequestAsync(dto, idempotencyKey, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<PatientRefundDetailDto>.Ok(result, "Pengajuan refund pasien berhasil dibuat dan masuk antrean persetujuan."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
        }
        catch (FinancePatientRefundValidationException ex)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, ex.Message));
        }
        catch (FinancePatientRefundConflictException ex)
        {
            return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, ex.Message));
        }
    }

    [HttpPost("{id:guid}/approve")]
    [AccessAction("Approve", "Approve Patient Refund", AccessType = AccessTypes.Update, SortOrder = 5)]
    [AccessPermission("FinancePatientRefund", "Approve")]
    [ProducesResponseType(typeof(ApiResponse<PatientRefundDetailDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Approve(
        Guid id,
        [FromBody] ApprovePatientRefundRequestDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.ApproveAsync(id, dto, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<PatientRefundDetailDto>.Ok(result, "Pengajuan refund pasien berhasil disetujui."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
        }
        catch (FinancePatientRefundValidationException ex)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, ex.Message));
        }
        catch (FinancePatientRefundConflictException ex)
        {
            return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, ex.Message));
        }
    }

    [HttpPost("{id:guid}/reject")]
    [AccessAction("Reject", "Reject Patient Refund", AccessType = AccessTypes.Update, SortOrder = 6)]
    [AccessPermission("FinancePatientRefund", "Reject")]
    [ProducesResponseType(typeof(ApiResponse<PatientRefundDetailDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Reject(
        Guid id,
        [FromBody] RejectPatientRefundRequestDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.RejectAsync(id, dto, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<PatientRefundDetailDto>.Ok(result, "Pengajuan refund pasien telah ditolak."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
        }
        catch (FinancePatientRefundValidationException ex)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, ex.Message));
        }
        catch (FinancePatientRefundConflictException ex)
        {
            return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, ex.Message));
        }
    }

    [HttpPost("{id:guid}/execute")]
    [AccessAction("Execute", "Execute Patient Refund Payment", AccessType = AccessTypes.Update, SortOrder = 7)]
    [AccessPermission("FinancePatientRefund", "Execute")]
    [ProducesResponseType(typeof(ApiResponse<PatientRefundDetailDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Execute(
        Guid id,
        [FromBody] ExecutePatientRefundRequestDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.ExecutePaymentAsync(id, dto, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<PatientRefundDetailDto>.Ok(result, "Pencairan pengembalian dana pasien berhasil diproses."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
        }
        catch (FinancePatientRefundValidationException ex)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, ex.Message));
        }
        catch (FinancePatientRefundConflictException ex)
        {
            return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, ex.Message));
        }
    }

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }
}

