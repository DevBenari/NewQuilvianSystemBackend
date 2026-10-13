using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.DTOs;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Controllers;

/// <summary>
/// Controller Pengajuan Pemutihan Tagihan AR V2 (PER_INVOICE dan BATCH).
/// Seluruh sumber invoice berasal dari Invoice AR Finance V2 (FinReceivableInvoiceBatch).
/// Mendukung lifecycle: DRAFT -> SUBMITTED -> APPROVED -> POSTED (atau SUBMITTED -> REJECTED).
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/finance-management/write-off-proposals")]
[AccessController("CORPORATE_FINANCE_MANAGEMENT_RECEIVABLE_WRITE_OFF_PROPOSAL",
    "Corporate Finance Management Receivable Write Off Proposal",
    "Receivable Write Off Proposal",
    AreaName = "Corporate",
    ControllerName = "FinanceReceivableWriteOffProposal",
    Description = "Pengajuan Pemutihan Tagihan AR V2 — alur proposal per-invoice dan batch berbasis Invoice AR Finance V2",
    SortOrder = 32)]
[Tags("Corporate / Finance Management / Receivable Write Off Proposal")]
public sealed class FinanceReceivableWriteOffProposalsController : ControllerBase
{
    private readonly FinanceReceivableWriteOffProposalService _service;
    private readonly ILogger<FinanceReceivableWriteOffProposalsController> _logger;

    public FinanceReceivableWriteOffProposalsController(
        FinanceReceivableWriteOffProposalService service,
        ILogger<FinanceReceivableWriteOffProposalsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    // ------------------------------------------------------------------------------------
    // GET: Daftar Proposal
    // ------------------------------------------------------------------------------------
    [HttpGet]
    [AccessAction("Read", "Read Write Off Proposal", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceReceivableWriteOffProposal", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ReceivableWriteOffProposalResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] ReceivableWriteOffProposalQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.GetPagedAsync(request, cancellationToken);
            return Ok(ApiResponse<PagedResult<ReceivableWriteOffProposalResponse>>.Ok(result, "Daftar pengajuan pemutihan tagihan berhasil diambil."));
        }
        catch (Exception ex)
        {
            return HandleException(ex, "GetPagedWriteOffProposals");
        }
    }

    // ------------------------------------------------------------------------------------
    // GET: Ringkasan Proposal
    // ------------------------------------------------------------------------------------
    [HttpGet("summary")]
    [AccessAction("Read", "Read Write Off Proposal", AccessType = AccessTypes.Read, SortOrder = 2)]
    [AccessPermission("FinanceReceivableWriteOffProposal", "Read")]
    [ProducesResponseType(typeof(ApiResponse<ReceivableWriteOffProposalSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary([FromQuery] ReceivableWriteOffProposalQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.GetSummaryAsync(request, cancellationToken);
            return Ok(ApiResponse<ReceivableWriteOffProposalSummaryResponse>.Ok(result, "Ringkasan pengajuan pemutihan tagihan berhasil diambil."));
        }
        catch (Exception ex)
        {
            return HandleException(ex, "GetSummaryWriteOffProposals");
        }
    }

    // ------------------------------------------------------------------------------------
    // GET: Daftar Invoice AR yang Layak Dihapusbukukan (Eligible)
    // ------------------------------------------------------------------------------------
    [HttpGet("eligible-invoices")]
    [AccessAction("Read", "Read Write Off Proposal", AccessType = AccessTypes.Read, SortOrder = 3)]
    [AccessPermission("FinanceReceivableWriteOffProposal", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<EligibleWriteOffInvoiceResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEligibleInvoices([FromQuery] EligibleWriteOffInvoiceQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.GetEligibleInvoicesAsync(request, cancellationToken);
            return Ok(ApiResponse<PagedResult<EligibleWriteOffInvoiceResponse>>.Ok(result, "Daftar Invoice AR yang memenuhi syarat berhasil diambil."));
        }
        catch (Exception ex)
        {
            return HandleException(ex, "GetEligibleInvoices");
        }
    }

    // ------------------------------------------------------------------------------------
    // GET: Pilihan Alasan Pemutihan
    // ------------------------------------------------------------------------------------
    [HttpGet("reasons")]
    [AccessAction("Read", "Read Write Off Proposal", AccessType = AccessTypes.Read, SortOrder = 4)]
    [AccessPermission("FinanceReceivableWriteOffProposal", "Read")]
    [ProducesResponseType(typeof(ApiResponse<List<EligibleWriteOffReasonResponse>>), StatusCodes.Status200OK)]
    public IActionResult GetReasons()
    {
        var result = _service.GetReasonCodes();
        return Ok(ApiResponse<List<EligibleWriteOffReasonResponse>>.Ok(result, "Pilihan alasan pemutihan berhasil diambil."));
    }

    // ------------------------------------------------------------------------------------
    // GET: Rincian Proposal by ID
    // ------------------------------------------------------------------------------------
    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Write Off Proposal", AccessType = AccessTypes.Read, SortOrder = 5)]
    [AccessPermission("FinanceReceivableWriteOffProposal", "Read")]
    [ProducesResponseType(typeof(ApiResponse<ReceivableWriteOffProposalDetailResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.GetByIdAsync(id, cancellationToken);
            return Ok(ApiResponse<ReceivableWriteOffProposalDetailResponse>.Ok(result, "Rincian pengajuan pemutihan tagihan berhasil diambil."));
        }
        catch (Exception ex)
        {
            return HandleException(ex, "GetWriteOffProposalById");
        }
    }

    // ------------------------------------------------------------------------------------
    // POST: Buat Draft Proposal
    // ------------------------------------------------------------------------------------
    [HttpPost]
    [AccessAction("Create", "Create Write Off Proposal", AccessType = AccessTypes.Create, SortOrder = 6)]
    [AccessPermission("FinanceReceivableWriteOffProposal", "Create")]
    [ProducesResponseType(typeof(ApiResponse<ReceivableWriteOffProposalDetailResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateReceivableWriteOffProposalRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = RequireCurrentUserId();
            var result = await _service.CreateDraftAsync(request, userId, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id },
                ApiResponse<ReceivableWriteOffProposalDetailResponse>.Ok(result, "Draft pengajuan pemutihan tagihan berhasil dibuat."));
        }
        catch (Exception ex)
        {
            return HandleException(ex, "CreateWriteOffProposal");
        }
    }

    // ------------------------------------------------------------------------------------
    // PUT: Perbarui Draft Proposal
    // ------------------------------------------------------------------------------------
    [HttpPut("{id:guid}")]
    [AccessAction("Update", "Update Write Off Proposal", AccessType = AccessTypes.Update, SortOrder = 7)]
    [AccessPermission("FinanceReceivableWriteOffProposal", "Update")]
    [ProducesResponseType(typeof(ApiResponse<ReceivableWriteOffProposalDetailResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id, [FromBody] UpdateReceivableWriteOffProposalRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = RequireCurrentUserId();
            var result = await _service.UpdateDraftAsync(id, request, userId, cancellationToken);
            return Ok(ApiResponse<ReceivableWriteOffProposalDetailResponse>.Ok(result, "Draft pengajuan pemutihan tagihan berhasil diperbarui."));
        }
        catch (Exception ex)
        {
            return HandleException(ex, "UpdateWriteOffProposal");
        }
    }

    // ------------------------------------------------------------------------------------
    // DELETE: Batalkan / Hapus Draft Proposal
    // ------------------------------------------------------------------------------------
    [HttpDelete("{id:guid}")]
    [AccessAction("Delete", "Delete Write Off Proposal", AccessType = AccessTypes.Delete, SortOrder = 8)]
    [AccessPermission("FinanceReceivableWriteOffProposal", "Delete")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id, [FromQuery] Guid rowVersion, CancellationToken cancellationToken)
    {
        try
        {
            var userId = RequireCurrentUserId();
            await _service.DeleteDraftAsync(id, rowVersion, userId, cancellationToken);
            return Ok(ApiResponse<object>.Ok(new { Id = id }, "Draft pengajuan pemutihan tagihan berhasil dihapus."));
        }
        catch (Exception ex)
        {
            return HandleException(ex, "DeleteWriteOffProposal");
        }
    }

    // ------------------------------------------------------------------------------------
    // POST: Submit Proposal (DRAFT -> SUBMITTED)
    // ------------------------------------------------------------------------------------
    [HttpPost("{id:guid}/submit")]
    [AccessAction("Submit", "Submit Write Off Proposal", AccessType = AccessTypes.Update, SortOrder = 9)]
    [AccessPermission("FinanceReceivableWriteOffProposal", "Update")]
    [ProducesResponseType(typeof(ApiResponse<ReceivableWriteOffProposalDetailResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Submit(
        [FromRoute] Guid id, [FromBody] SubmitReceivableWriteOffProposalRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = RequireCurrentUserId();
            var result = await _service.SubmitAsync(id, request.ExpectedRowVersion, userId, cancellationToken);
            return Ok(ApiResponse<ReceivableWriteOffProposalDetailResponse>.Ok(result, "Pengajuan pemutihan tagihan berhasil diajukan (SUBMITTED)."));
        }
        catch (Exception ex)
        {
            return HandleException(ex, "SubmitWriteOffProposal");
        }
    }

    // ------------------------------------------------------------------------------------
    // POST: Setujui Proposal (SUBMITTED -> APPROVED) — Maker-Checker
    // ------------------------------------------------------------------------------------
    [HttpPost("{id:guid}/approve")]
    [AccessAction("Approve", "Approve Write Off Proposal", AccessType = AccessTypes.Update, SortOrder = 10)]
    [AccessPermission("FinanceReceivableWriteOffProposal", "Update")]
    [ProducesResponseType(typeof(ApiResponse<ReceivableWriteOffProposalDetailResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Approve(
        [FromRoute] Guid id, [FromBody] ApproveReceivableWriteOffProposalRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = RequireCurrentUserId();
            var result = await _service.ApproveAsync(id, request, userId, cancellationToken);
            return Ok(ApiResponse<ReceivableWriteOffProposalDetailResponse>.Ok(result, "Pengajuan pemutihan tagihan berhasil disetujui (APPROVED)."));
        }
        catch (Exception ex)
        {
            return HandleException(ex, "ApproveWriteOffProposal");
        }
    }

    // ------------------------------------------------------------------------------------
    // POST: Tolak Proposal (SUBMITTED -> REJECTED)
    // ------------------------------------------------------------------------------------
    [HttpPost("{id:guid}/reject")]
    [AccessAction("Reject", "Reject Write Off Proposal", AccessType = AccessTypes.Update, SortOrder = 11)]
    [AccessPermission("FinanceReceivableWriteOffProposal", "Update")]
    [ProducesResponseType(typeof(ApiResponse<ReceivableWriteOffProposalDetailResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Reject(
        [FromRoute] Guid id, [FromBody] RejectReceivableWriteOffProposalRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = RequireCurrentUserId();
            var result = await _service.RejectAsync(id, request, userId, cancellationToken);
            return Ok(ApiResponse<ReceivableWriteOffProposalDetailResponse>.Ok(result, "Pengajuan pemutihan tagihan telah ditolak (REJECTED)."));
        }
        catch (Exception ex)
        {
            return HandleException(ex, "RejectWriteOffProposal");
        }
    }

    // ------------------------------------------------------------------------------------
    // POST: Posting Pemutihan ke Buku Piutang & Outbox (APPROVED -> POSTED)
    // ------------------------------------------------------------------------------------
    [HttpPost("{id:guid}/post")]
    [AccessAction("Post", "Post Write Off Proposal", AccessType = AccessTypes.Update, SortOrder = 12)]
    [AccessPermission("FinanceReceivableWriteOffProposal", "Update")]
    [ProducesResponseType(typeof(ApiResponse<ReceivableWriteOffProposalDetailResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Post(
        [FromRoute] Guid id, [FromBody] PostReceivableWriteOffProposalRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = RequireCurrentUserId();
            var result = await _service.PostAsync(id, request.ExpectedRowVersion, userId, cancellationToken);
            return Ok(ApiResponse<ReceivableWriteOffProposalDetailResponse>.Ok(result, "Pemutihan tagihan berhasil diposting ke saldo piutang dan akuntansi (POSTED)."));
        }
        catch (Exception ex)
        {
            return HandleException(ex, "PostWriteOffProposal");
        }
    }

    // ------------------------------------------------------------------------------------
    // Helper Methods
    // ------------------------------------------------------------------------------------

    private Guid RequireCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
        if (Guid.TryParse(value, out var userId) && userId != Guid.Empty)
        {
            return userId;
        }

        var traceId = HttpContext.TraceIdentifier;
        _logger.LogWarning("Identitas pengguna tidak valid pada Pengajuan Pemutihan Tagihan. TraceId={TraceId}, RawClaim={RawClaim}", traceId, value);
        throw new ReceivableBadRequestException("Identitas pengguna tidak valid untuk transaksi keuangan.");
    }

    private IActionResult HandleException(Exception exception, string actionName)
    {
        var traceId = HttpContext.TraceIdentifier;

        if (exception is KeyNotFoundException)
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, exception.Message, new { traceId }));

        if (exception is ReceivableBadRequestException)
            return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, exception.Message, new { traceId }));

        if (exception is ReceivableValidationException)
            return UnprocessableEntity(ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, exception.Message, new { traceId }));

        if (exception is ReceivableConflictException or DbUpdateConcurrencyException)
            return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, exception.Message, new { traceId }));

        var pgEx = exception as PostgresException ?? exception.InnerException as PostgresException;
        _logger.LogError(exception,
            "Kesalahan internal pada {ActionName}. TraceId={TraceId}, Message={Message}, SqlState={SqlState}",
            actionName, traceId, exception.Message, pgEx?.SqlState);

        return StatusCode(StatusCodes.Status500InternalServerError,
            ApiResponse<object>.Fail(StatusCodes.Status500InternalServerError, "Terjadi kesalahan pada server saat memproses permohonan pemutihan tagihan.", new { traceId }));
    }
}
