using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Controllers;

/// <summary>
/// Controller jembatan manual potong gaji dan jadwal potongan cicilan piutang pegawai
/// (BE-FIN-116, BE-FIN-117, FIN-DEC-219..221, FR-FIN-253..256, FIN-INTEGRATION-1.10 §S.1..S.2, FIN-API-1.11 §S.1).
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/finance-management/payroll-deduction")]
[AccessController("CORPORATE_FINANCE_MANAGEMENT_RECEIVABLE_PAYROLL_DEDUCTION_BRIDGE",
    "Corporate Finance Management Receivable Payroll Deduction Bridge", "Potong Gaji Manual",
    AreaName = "Corporate", ControllerName = "FinancePayrollDeductionBatch",
    Description = "Jadwal potongan per periode dan batch hasil potongan manual", SortOrder = 76)]
[Tags("Corporate / Finance Management / Receivable / Payroll Deduction Bridge")]
public sealed class FinancePayrollDeductionBatchController : ControllerBase
{
    private static readonly Regex PeriodRegex = new(@"^\d{4}-(0[1-9]|1[0-2])$", RegexOptions.Compiled);
    private readonly FinancePayrollDeductionBatchService _service;

    public FinancePayrollDeductionBatchController(FinancePayrollDeductionBatchService service)
    {
        _service = service;
    }

    // ------------------------------------------------------------------------------------
    // BE-FIN-116: Jadwal & Ekspor
    // ------------------------------------------------------------------------------------

    /// <summary>
    /// Mengambil jadwal potongan satu periode beserta total dan rincian per baris (AC 1, AC 2).
    /// </summary>
    [HttpGet("schedule")]
    [AccessAction("Read", "Read Deduction Schedule", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinancePayrollDeductionBatch", "Read")]
    [ProducesResponseType(typeof(ApiResponse<DeductionScheduleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSchedule([FromQuery] string period, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(period) || !PeriodRegex.IsMatch(period.Trim()))
        {
            return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest,
                "Format parameter period harus YYYY-MM dengan bulan 01 sampai 12 (contoh: 2026-09)."));
        }

        try
        {
            var result = await _service.GetScheduleAsync(period.Trim(), cancellationToken);
            return Ok(ApiResponse<DeductionScheduleResponse>.Ok(result, "Jadwal potongan gaji berhasil diambil."));
        }
        catch (FinancePayrollDeductionBatchBadRequestException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, ex.Message));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, exception.Message));
        }
    }

    /// <summary>
    /// Mengekspor jadwal potongan ke berkas Excel; sekaligus menjadi templat berkas unggah hasil (AC 3, FIN-DEC-219).
    /// </summary>
    [HttpGet("schedule/export/excel")]
    [AccessAction("Read", "Export Deduction Schedule Excel", AccessType = AccessTypes.Read, SortOrder = 2)]
    [AccessPermission("FinancePayrollDeductionBatch", "Read")]
    public async Task<IActionResult> ExportScheduleExcel([FromQuery] string period, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(period) || !PeriodRegex.IsMatch(period.Trim()))
        {
            return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest,
                "Format parameter period harus YYYY-MM dengan bulan 01 sampai 12 (contoh: 2026-09)."));
        }

        try
        {
            var bytes = await _service.ExportScheduleExcelAsync(period.Trim(), cancellationToken);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"jadwal-potongan-gaji-{period.Trim()}-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx");
        }
        catch (FinancePayrollDeductionBatchBadRequestException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, ex.Message));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, exception.Message));
        }
    }

    /// <summary>
    /// Mengekspor jadwal potongan ke berkas PDF untuk serah terima / arsip cetak (FIN-DEC-219).
    /// </summary>
    [HttpGet("schedule/export/pdf")]
    [AccessAction("Read", "Export Deduction Schedule PDF", AccessType = AccessTypes.Read, SortOrder = 3)]
    [AccessPermission("FinancePayrollDeductionBatch", "Read")]
    public async Task<IActionResult> ExportSchedulePdf([FromQuery] string period, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(period) || !PeriodRegex.IsMatch(period.Trim()))
        {
            return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest,
                "Format parameter period harus YYYY-MM dengan bulan 01 sampai 12 (contoh: 2026-09)."));
        }

        try
        {
            var bytes = await _service.ExportSchedulePdfAsync(period.Trim(), cancellationToken);
            return File(bytes, "application/pdf",
                $"jadwal-potongan-gaji-{period.Trim()}-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf");
        }
        catch (FinancePayrollDeductionBatchBadRequestException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, ex.Message));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, exception.Message));
        }
    }

    // ------------------------------------------------------------------------------------
    // BE-FIN-117: Daftar & Rincian Batch
    // ------------------------------------------------------------------------------------

    /// <summary>
    /// Mengambil daftar batch hasil potongan gaji berpaging (FIN-API-1.11 §S.1).
    /// </summary>
    [HttpGet("batches")]
    [AccessAction("Read", "Read Deduction Batches", AccessType = AccessTypes.Read, SortOrder = 4)]
    [AccessPermission("FinancePayrollDeductionBatch", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<DeductionBatchListResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBatches([FromQuery] DeductionBatchPagedQuery query, CancellationToken cancellationToken)
    {
        var result = await _service.GetBatchesAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<DeductionBatchListResponse>>.Ok(result, "Daftar batch hasil potongan berhasil diambil."));
    }

    /// <summary>
    /// Mengambil rincian batch beserta baris item dan hasil validasinya (FIN-API-1.11 §S.1).
    /// </summary>
    [HttpGet("batches/{id:guid}")]
    [AccessAction("Read", "Read Deduction Batch Detail", AccessType = AccessTypes.Read, SortOrder = 5)]
    [AccessPermission("FinancePayrollDeductionBatch", "Read")]
    [ProducesResponseType(typeof(ApiResponse<DeductionBatchResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBatchDetail([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.GetBatchDetailAsync(id, cancellationToken);
            return Ok(ApiResponse<DeductionBatchResponse>.Ok(result, "Rincian batch hasil potongan berhasil diambil."));
        }
        catch (FinancePayrollDeductionBatchNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
        }
    }

    // ------------------------------------------------------------------------------------
    // BE-FIN-117: Unggah Berkas Batch (POST /batches)
    // ------------------------------------------------------------------------------------

    /// <summary>
    /// Mengunggah berkas hasil potongan; membuat batch DRAFT dan memvalidasi tiap baris (AC 1, AC 7, AC 8).
    /// </summary>
    [HttpPost("batches")]
    [Consumes("multipart/form-data")]
    [AccessAction("Create", "Upload Deduction Batch", AccessType = AccessTypes.Create, SortOrder = 6)]
    [AccessPermission("FinancePayrollDeductionBatch", "Create")]
    [ProducesResponseType(typeof(ApiResponse<DeductionBatchResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UploadBatch([FromForm] UploadDeductionBatchRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var actorUserId = CurrentUserId();
            var result = await _service.UploadBatchAsync(request, actorUserId, cancellationToken);
            return StatusCode(StatusCodes.Status201Created,
                ApiResponse<DeductionBatchResponse>.Ok(result, "Batch hasil potongan berhasil diunggah."));
        }
        catch (FinancePayrollDeductionBatchBadRequestException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, ex.Message));
        }
        catch (FinancePayrollDeductionBatchConflictException ex)
        {
            return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, ex.Message));
        }
        catch (FinancePayrollDeductionBatchValidationException ex)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, ex.Message));
        }
    }

    // ------------------------------------------------------------------------------------
    // BE-FIN-117: Pengajuan Batch (POST /batches/{id}/submit)
    // ------------------------------------------------------------------------------------

    /// <summary>
    /// Mengajukan batch dengan nomor dan bukti laporan penggajian (AC 2, AC 3).
    /// </summary>
    [HttpPost("batches/{id:guid}/submit")]
    [AccessAction("Submit", "Submit Deduction Batch", AccessType = AccessTypes.Update, SortOrder = 7)]
    [AccessPermission("FinancePayrollDeductionBatch", "Submit")]
    [ProducesResponseType(typeof(ApiResponse<DeductionBatchResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> SubmitBatch(
        [FromRoute] Guid id, [FromBody] SubmitDeductionBatchRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var actorUserId = CurrentUserId();
            var result = await _service.SubmitBatchAsync(id, request, actorUserId, cancellationToken);
            return Ok(ApiResponse<DeductionBatchResponse>.Ok(result, "Batch hasil potongan berhasil diajukan."));
        }
        catch (FinancePayrollDeductionBatchNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
        }
        catch (FinancePayrollDeductionBatchConflictException ex)
        {
            return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, ex.Message));
        }
        catch (FinancePayrollDeductionBatchValidationException ex)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, ex.Message));
        }
    }

    // ------------------------------------------------------------------------------------
    // BE-FIN-117: Pengesahan Batch Reguler (POST /batches/{id}/approve)
    // ------------------------------------------------------------------------------------

    /// <summary>
    /// Mengesahkan batch REGULER dan menerapkan seluruh baris dalam satu transaksi atomik (AC 4, AC 5, AC 6).
    /// </summary>
    [HttpPost("batches/{id:guid}/approve")]
    [AccessAction("Approve", "Approve Deduction Batch", AccessType = AccessTypes.Update, SortOrder = 8)]
    [AccessPermission("FinancePayrollDeductionBatch", "Approve")]
    [ProducesResponseType(typeof(ApiResponse<DeductionBatchResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ApproveBatch(
        [FromRoute] Guid id, [FromBody] DeductionBatchTransitionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var actorUserId = CurrentUserId();
            var result = await _service.ApproveBatchAsync(id, request, actorUserId, cancellationToken);
            return Ok(ApiResponse<DeductionBatchResponse>.Ok(result, "Batch hasil potongan berhasil disahkan secara atomik."));
        }
        catch (FinancePayrollDeductionBatchNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
        }
        catch (FinancePayrollDeductionBatchForbiddenException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                ApiResponse<object>.Fail(StatusCodes.Status403Forbidden, ex.Message));
        }
        catch (FinancePayrollDeductionBatchConflictException ex)
        {
            return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, ex.Message));
        }
        catch (FinancePayrollDeductionBatchBadRequestException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, ex.Message));
        }
        catch (FinancePayrollDeductionBatchValidationException ex)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, ex.Message));
        }
    }

    // ------------------------------------------------------------------------------------
    // BE-FIN-117/118: Pengesahan Batch Koreksi (POST /batches/{id}/approve-correction)
    // ------------------------------------------------------------------------------------

    /// <summary>
    /// Mengesahkan batch KOREKSI (FIN-API-1.11 §S.1).
    /// </summary>
    [HttpPost("batches/{id:guid}/approve-correction")]
    [AccessAction("ApproveCorrection", "Approve Correction Deduction Batch", AccessType = AccessTypes.Update, SortOrder = 9)]
    [AccessPermission("FinancePayrollDeductionBatch", "ApproveCorrection")]
    [ProducesResponseType(typeof(ApiResponse<DeductionBatchResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ApproveCorrectionBatch(
        [FromRoute] Guid id, [FromBody] DeductionBatchTransitionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var actorUserId = CurrentUserId();
            var result = await _service.ApproveCorrectionBatchAsync(id, request, actorUserId, cancellationToken);
            return Ok(ApiResponse<DeductionBatchResponse>.Ok(result, "Batch koreksi berhasil disahkan."));
        }
        catch (FinancePayrollDeductionBatchNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
        }
        catch (FinancePayrollDeductionBatchForbiddenException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                ApiResponse<object>.Fail(StatusCodes.Status403Forbidden, ex.Message));
        }
        catch (FinancePayrollDeductionBatchConflictException ex)
        {
            return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, ex.Message));
        }
        catch (FinancePayrollDeductionBatchBadRequestException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, ex.Message));
        }
        catch (FinancePayrollDeductionBatchValidationException ex)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, ex.Message));
        }
    }

    // ------------------------------------------------------------------------------------
    // BE-FIN-117: Penolakan Batch (POST /batches/{id}/reject)
    // ------------------------------------------------------------------------------------

    /// <summary>
    /// Menolak batch yang diajukan; alasan wajib (FIN-API-1.11 §S.1).
    /// </summary>
    [HttpPost("batches/{id:guid}/reject")]
    [AccessAction("Reject", "Reject Deduction Batch", AccessType = AccessTypes.Update, SortOrder = 10)]
    [AccessPermission("FinancePayrollDeductionBatch", "Reject")]
    [ProducesResponseType(typeof(ApiResponse<DeductionBatchResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RejectBatch(
        [FromRoute] Guid id, [FromBody] RejectDeductionBatchRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var actorUserId = CurrentUserId();
            var result = await _service.RejectBatchAsync(id, request, actorUserId, cancellationToken);
            return Ok(ApiResponse<DeductionBatchResponse>.Ok(result, "Batch hasil potongan berhasil ditolak."));
        }
        catch (FinancePayrollDeductionBatchNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
        }
        catch (FinancePayrollDeductionBatchConflictException ex)
        {
            return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, ex.Message));
        }
        catch (FinancePayrollDeductionBatchValidationException ex)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, ex.Message));
        }
    }

    // ------------------------------------------------------------------------------------
    // BE-FIN-117: Pembatalan Batch (POST /batches/{id}/cancel)
    // ------------------------------------------------------------------------------------

    /// <summary>
    /// Membatalkan batch DRAFT atau DIAJUKAN; alasan wajib (FIN-API-1.11 §S.1).
    /// </summary>
    [HttpPost("batches/{id:guid}/cancel")]
    [AccessAction("Cancel", "Cancel Deduction Batch", AccessType = AccessTypes.Update, SortOrder = 11)]
    [AccessPermission("FinancePayrollDeductionBatch", "Cancel")]
    [ProducesResponseType(typeof(ApiResponse<DeductionBatchResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CancelBatch(
        [FromRoute] Guid id, [FromBody] CancelDeductionBatchRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var actorUserId = CurrentUserId();
            var result = await _service.CancelBatchAsync(id, request, actorUserId, cancellationToken);
            return Ok(ApiResponse<DeductionBatchResponse>.Ok(result, "Batch hasil potongan berhasil dibatalkan."));
        }
        catch (FinancePayrollDeductionBatchNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
        }
        catch (FinancePayrollDeductionBatchConflictException ex)
        {
            return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, ex.Message));
        }
        catch (FinancePayrollDeductionBatchValidationException ex)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, ex.Message));
        }
    }

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }
}
