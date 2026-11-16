using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Request.DTOs;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Request.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Request.Controllers;

/// <summary>
/// Verifikasi PR oleh Tim Pengadaan (BE-PBJ-008; PBJ-PP-API bagian Verification).
/// </summary>
/// <remarks>
/// Arketipe worklist: antrean PR <c>SUBMITTED</c> beserta aksi per PR, tanpa <c>POST /</c> maupun
/// <c>DELETE</c>. Persetujuan PR tidak punya endpoint di sini — penyetuju bertindak di kotak masuk Wfl,
/// dan hasilnya disinkronkan ke PR oleh penangan lifecycle.
/// </remarks>
[ApiController]
[Authorize]
[Route("api/v1/corporate/procurement-management/purchase-request-verifications")]
[AccessController(ProcurementPermissions.RequestModuleCode, ProcurementPermissions.RequestModuleName, "Procurement Request Verification",
    AreaName = ProcurementPermissions.AreaName, ControllerName = ProcurementPermissions.PurchaseRequestVerificationResource,
    Description = "Verifikasi dan cek stok permintaan pengadaan oleh Tim Pengadaan", SortOrder = 2)]
[Tags("Corporate / Procurement Management / Request / Verification")]
public sealed class PurchaseRequestVerificationsController : ControllerBase
{
    private const string Resource = ProcurementPermissions.PurchaseRequestVerificationResource;
    private readonly ProcurementPurchaseRequestVerificationService _service;

    public PurchaseRequestVerificationsController(ProcurementPurchaseRequestVerificationService service) => _service = service;

    [HttpGet]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Request Verification", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PurchaseRequestListItem>>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetQueue([FromQuery] PurchaseRequestQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetQueueAsync(User, request, cancellationToken), "Antrean verifikasi berhasil diambil.");

    [HttpGet("stock-checks")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Request Verification", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<StockCheckListItem>>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetStockChecks([FromQuery] StockCheckQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetStockChecksAsync(User, request, cancellationToken), "Daftar cek stok berhasil diambil.");

    [HttpPut("{id:guid}/stock-checks")]
    [AccessAction(ProcurementPermissions.VerifyAction, "Verify Procurement Request", Description = "Mencatat cek stok dan memverifikasi permintaan", AccessType = AccessTypes.Update, SortOrder = 2)]
    [AccessPermission(Resource, ProcurementPermissions.VerifyAction)]
    [ProducesResponseType(typeof(ApiResponse<PurchaseRequestResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> RecordStockCheck(Guid id, [FromBody] RecordStockCheckRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.RecordStockCheckAsync(User, id, request, cancellationToken), "Hasil cek stok berhasil disimpan.");

    [HttpPost("{id:guid}/complete")]
    [AccessAction(ProcurementPermissions.VerifyAction, "Verify Procurement Request", Description = "Mencatat cek stok dan memverifikasi permintaan", AccessType = AccessTypes.Update, SortOrder = 2)]
    [AccessPermission(Resource, ProcurementPermissions.VerifyAction)]
    [ProducesResponseType(typeof(ApiResponse<PurchaseRequestResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> Complete(
        Guid id,
        [FromBody] CompleteVerificationRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.CompleteAsync(User, id, request, idempotencyKey, cancellationToken), "Verifikasi berhasil diselesaikan.");

    [HttpPost("{id:guid}/return")]
    [AccessAction(ProcurementPermissions.VerifyAction, "Verify Procurement Request", Description = "Mencatat cek stok dan memverifikasi permintaan", AccessType = AccessTypes.Update, SortOrder = 2)]
    [AccessPermission(Resource, ProcurementPermissions.VerifyAction)]
    [ProducesResponseType(typeof(ApiResponse<PurchaseRequestResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> Return(Guid id, [FromBody] ReturnPurchaseRequestRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.ReturnAsync(User, id, request, cancellationToken), "Permintaan berhasil dikembalikan ke pengusul.");

    [HttpPost("{id:guid}/reject")]
    [AccessAction(ProcurementPermissions.VerifyAction, "Verify Procurement Request", Description = "Mencatat cek stok dan memverifikasi permintaan", AccessType = AccessTypes.Update, SortOrder = 2)]
    [AccessPermission(Resource, ProcurementPermissions.VerifyAction)]
    [ProducesResponseType(typeof(ApiResponse<PurchaseRequestResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> Reject(Guid id, [FromBody] RejectPurchaseRequestRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.RejectAsync(User, id, request, cancellationToken), "Permintaan berhasil ditolak.");

    [HttpPost("{id:guid}/cancel-approved")]
    [AccessAction(ProcurementPermissions.CancelApprovedAction, "Cancel Approved Procurement Request", Description = "Membatalkan permintaan yang sudah disetujui dan belum masuk paket", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission(Resource, ProcurementPermissions.CancelApprovedAction)]
    [ProducesResponseType(typeof(ApiResponse<PurchaseRequestResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> CancelApproved(Guid id, [FromBody] CancelPurchaseRequestRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.CancelApprovedAsync(User, id, request, cancellationToken), "Permintaan yang disetujui berhasil dibatalkan.");

    /// <remarks>
    /// Delta kontrak (BE-PBJ-008): sinkron ulang yang diasumsikan arsitektur bagian 8 belum ada sebagai
    /// endpoint generik Wfl, sehingga disediakan di sini mengikuti pola
    /// <c>AttendanceCorrectionController</c> <c>POST /{id}/workflow/synchronize</c>, memakai izin
    /// <c>Verify</c> yang sudah ada.
    /// </remarks>
    [HttpPost("{id:guid}/workflow/synchronize")]
    [AccessAction(ProcurementPermissions.VerifyAction, "Verify Procurement Request", Description = "Mencatat cek stok dan memverifikasi permintaan", AccessType = AccessTypes.Update, SortOrder = 2)]
    [AccessPermission(Resource, ProcurementPermissions.VerifyAction)]
    [ProducesResponseType(typeof(ApiResponse<PurchaseRequestResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public Task<IActionResult> SynchronizeWorkflow(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.SynchronizeWorkflowAsync(User, id, cancellationToken), "Status pengajuan berhasil disinkronkan dengan alur persetujuan.");

    private async Task<IActionResult> ExecuteAsync<T>(Func<Task<T>> command, string message)
    {
        try { return Ok(ApiResponse<T>.Ok(await command(), message)); }
        catch (KeyNotFoundException exception) { return NotFound(ApiResponse<object>.Fail(404, exception.Message)); }
        catch (ProcurementRequestForbiddenException exception) { return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail(403, exception.Message)); }
        catch (ProcurementRequestConflictException exception) { return Conflict(ApiResponse<object>.Fail(409, exception.Message, new { code = exception.Code })); }
        catch (ProcurementRequestValidationException exception)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(422, exception.Message, new { code = exception.Code, lineNo = exception.LineNo }));
        }
    }
}
