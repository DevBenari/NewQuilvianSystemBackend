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
/// BE-FIN-057 (FIN-DEC-099..104, 02-backend-architecture.md AMENDMENT REVISI 13 §K.6). Aggregate
/// ber-lifecycle (transaksi, bukan master data) — perpindahan status lewat aksi POST
/// /{id}/&lt;aksi&gt;, bukan PATCH /{id}/status generik (transaction-endpoint-standard.md). Sepuluh
/// endpoint, nol DELETE /{id} (pembatalan memakai aksi cancel), nol GET /options.
/// FIN-DEC-103: write-off dan cancel TANPA jenjang persetujuan — satu resource Update saja
/// menjaga keduanya, berbeda sengaja dari FinanceReceivable yang memakai maker-checker.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/finance-management/non-patient-receivables")]
[AccessController("CORPORATE_FINANCE_MANAGEMENT_NON_PATIENT_RECEIVABLE", "Corporate Finance Management Non Patient Receivable", "Non Patient Receivable",
    AreaName = "Corporate", ControllerName = "FinanceNonPatientReceivable", Description = "Piutang sewa non-pasien — parkir dan tenant", SortOrder = 32)]
[Tags("Corporate / Finance Management / Non Patient Receivable")]
public sealed class FinanceNonPatientReceivablesController : ControllerBase
{
    private readonly FinanceNonPatientReceivableService _service;
    public FinanceNonPatientReceivablesController(FinanceNonPatientReceivableService service) => _service = service;

    [HttpGet("filters/metadata")]
    [AccessAction("Read", "Read Non Patient Receivable", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceNonPatientReceivable", "Read")]
    [ProducesResponseType(typeof(ApiResponse<NonPatientReceivableFilterMetadataResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFilterMetadata(CancellationToken cancellationToken) =>
        Ok(ApiResponse<NonPatientReceivableFilterMetadataResponse>.Ok(
            await _service.GetFilterMetadataAsync(cancellationToken), "Metadata filter piutang sewa berhasil diambil."));

    [HttpGet("summary")]
    [AccessAction("Read", "Read Non Patient Receivable", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceNonPatientReceivable", "Read")]
    [ProducesResponseType(typeof(ApiResponse<NonPatientReceivableSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary([FromQuery] NonPatientReceivableSummaryQuery request, CancellationToken cancellationToken) =>
        Ok(ApiResponse<NonPatientReceivableSummaryResponse>.Ok(
            await _service.GetSummaryAsync(request.Category, cancellationToken), "Ringkasan piutang sewa berhasil diambil."));

    [HttpGet("aging")]
    [AccessAction("Read", "Read Non Patient Receivable", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceNonPatientReceivable", "Read")]
    [ProducesResponseType(typeof(ApiResponse<List<ReceivableAgingBucketResult>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAging([FromQuery] NonPatientReceivableAgingQuery request, CancellationToken cancellationToken) =>
        Ok(ApiResponse<List<ReceivableAgingBucketResult>>.Ok(
            await _service.GetAgingAsync(request.AsOfDate, request.Category, cancellationToken), "Umur piutang sewa berhasil diambil."));

    [HttpGet]
    [AccessAction("Read", "Read Non Patient Receivable", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceNonPatientReceivable", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<NonPatientReceivableResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] NonPatientReceivableQuery request, CancellationToken cancellationToken) =>
        Ok(ApiResponse<PagedResult<NonPatientReceivableResponse>>.Ok(
            await _service.GetPagedAsync(request, cancellationToken), "Daftar piutang sewa berhasil diambil."));

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Non Patient Receivable", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceNonPatientReceivable", "Read")]
    [ProducesResponseType(typeof(ApiResponse<NonPatientReceivableDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        try { return Ok(ApiResponse<NonPatientReceivableDetailResponse>.Ok(await _service.GetByIdAsync(id, cancellationToken), "Rincian tagihan sewa berhasil diambil.")); }
        catch (KeyNotFoundException exception) { return NotFound(ApiResponse<object>.Fail(404, exception.Message)); }
    }

    [HttpPost]
    [AccessAction("Create", "Create Non Patient Receivable", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("FinanceNonPatientReceivable", "Create")]
    public async Task<IActionResult> Create([FromBody] CreateNonPatientReceivableRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.CreateAsync(request, CurrentUserId(), cancellationToken);
            return StatusCode(201, ApiResponse<NonPatientReceivableResponse>.Ok(FinanceNonPatientReceivableService.Map(result), "Tagihan sewa berhasil dicatat."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpPut("{id:guid}")]
    [AccessAction("Update", "Update Non Patient Receivable", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("FinanceNonPatientReceivable", "Update")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateNonPatientReceivableRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.UpdateAsync(id, request, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<NonPatientReceivableResponse>.Ok(FinanceNonPatientReceivableService.Map(result), "Tagihan sewa berhasil dikoreksi."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpPost("{id:guid}/settlements")]
    [AccessAction("Update", "Update Non Patient Receivable", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("FinanceNonPatientReceivable", "Update")]
    public async Task<IActionResult> AddSettlement(Guid id, [FromBody] CreateNonPatientReceivableSettlementRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.AddSettlementAsync(id, request, CurrentUserId(), cancellationToken);
            return StatusCode(201, ApiResponse<NonPatientReceivableResponse>.Ok(FinanceNonPatientReceivableService.Map(result), "Pelunasan berhasil dicatat."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpPost("{id:guid}/write-off")]
    [AccessAction("Update", "Update Non Patient Receivable", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("FinanceNonPatientReceivable", "Update")]
    public async Task<IActionResult> WriteOff(Guid id, [FromBody] WriteOffNonPatientReceivableRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.WriteOffAsync(id, request, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<NonPatientReceivableResponse>.Ok(FinanceNonPatientReceivableService.Map(result), "Piutang sewa berhasil dihapusbukukan."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpPost("{id:guid}/cancel")]
    [AccessAction("Update", "Update Non Patient Receivable", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("FinanceNonPatientReceivable", "Update")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelNonPatientReceivableRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.CancelAsync(id, request, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<NonPatientReceivableResponse>.Ok(FinanceNonPatientReceivableService.Map(result), "Tagihan sewa berhasil dibatalkan."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    private IActionResult Failure(Exception exception) => exception switch
    {
        KeyNotFoundException => NotFound(ApiResponse<object>.Fail(404, exception.Message)),
        NonPatientReceivableConflictException => Conflict(ApiResponse<object>.Fail(409, exception.Message)),
        NonPatientReceivableValidationException => UnprocessableEntity(ApiResponse<object>.Fail(422, exception.Message)),
        NonPatientReceivableBadRequestException => BadRequest(ApiResponse<object>.Fail(400, exception.Message)),
        _ => throw exception
    };

    private static bool IsHandled(Exception exception) => exception is
        KeyNotFoundException or NonPatientReceivableConflictException or NonPatientReceivableValidationException or NonPatientReceivableBadRequestException;

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }
}
