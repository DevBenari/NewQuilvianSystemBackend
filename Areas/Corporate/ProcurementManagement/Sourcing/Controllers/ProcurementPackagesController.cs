using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.DTOs;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Services;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Request.DTOs;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Request.Services;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Sourcing.DTOs;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Sourcing.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Sourcing.Controllers;

/// <summary>
/// Paket pengadaan — bagian draf (BE-PBJ-PS-005; PBJ-PS-API 1.0 §Procurement Package): metadata saringan, ringkasan, daftar,
/// baris PR siap paket, buat, detail, riwayat, tambah baris, lepas baris; persiapan (BE-PBJ-PS-007): ubah persiapan, saran
/// metode, kriteria teknis, evaluator; persetujuan persiapan (BE-PBJ-PS-008): ajukan, tarik, buka ulang, linimasa, sinkron
/// ulang <c>Wfl</c>. Pecah, batal, dan nomor PO menyusul di BE-PBJ-PS-014..015.
/// </summary>
/// <remarks>
/// Transaksi ber-lifecycle, bukan master data: tidak ada <c>GET /options</c>, <c>PATCH /{id}/status</c>, atau
/// <c>DELETE /{id}</c>; paket dibatalkan lewat aksi <c>cancel</c> (BE-PBJ-PS-015). <c>DELETE /{id}/sources/{sourceId}</c>
/// melepas satu baris PR dari paket, bukan menghapus paket. Paket di luar cakupan unit pengguna dijawab 404. Controller hanya
/// meneruskan ke <see cref="ProcurementPackageService"/> (QBE-SVC-001).
/// </remarks>
[ApiController]
[Authorize]
[Route("api/v1/corporate/procurement-management/procurement-packages")]
[AccessController(ProcurementPermissions.SourcingModuleCode, ProcurementPermissions.SourcingModuleName, "Procurement Package",
    AreaName = ProcurementPermissions.AreaName, ControllerName = ProcurementPermissions.PackageResource,
    Description = "Paket pengadaan dari baris permintaan yang disetujui", SortOrder = 1)]
[Tags("Corporate / Procurement Management / Sourcing / Procurement Package")]
public sealed class ProcurementPackagesController : ControllerBase
{
    private const string Resource = ProcurementPermissions.PackageResource;
    private readonly ProcurementPackageService _service;
    private readonly ProcurementPackagePreparationService _preparation;
    private readonly ProcurementPackageApprovalService _approval;
    private readonly ProcurementApprovalTimelineService _approvalTimeline;

    public ProcurementPackagesController(
        ProcurementPackageService service,
        ProcurementPackagePreparationService preparation,
        ProcurementPackageApprovalService approval,
        ProcurementApprovalTimelineService approvalTimeline)
    {
        _service = service;
        _preparation = preparation;
        _approval = approval;
        _approvalTimeline = approvalTimeline;
    }

    [HttpGet("filters/metadata")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Package", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<PackageFilterMetadataResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetFilterMetadata(CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetFilterMetadataAsync(cancellationToken), "Metadata filter paket berhasil diambil.");

    [HttpGet("summary")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Package", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<PackageSummaryResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetSummary(CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetSummaryAsync(User, cancellationToken), "Ringkasan paket berhasil diambil.");

    [HttpGet]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Package", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PackageListItem>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> Get([FromQuery] PackageQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetPagedAsync(User, request, cancellationToken), "Daftar paket berhasil diambil.");

    /// <summary>Baris PR <c>APPROVED</c> yang belum masuk paket, dalam cakupan unit pengguna (PBJ-PP-INT 1.2 §2).</summary>
    [HttpGet("packageable-request-lines")]
    [AccessAction(ProcurementPermissions.CreateAction, "Create Procurement Package", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission(Resource, ProcurementPermissions.CreateAction)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PackageableRequestLineResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetPackageableRequestLines(
        [FromQuery] PackageableRequestLineQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetPackageableLinesAsync(User, request, cancellationToken), "Baris permintaan siap paket berhasil diambil.");

    [HttpGet("{id:guid}")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Package", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<PackageResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetByIdAsync(User, id, cancellationToken), "Detail paket berhasil diambil.");

    /// <summary>Metode aktif yang rentang HPS-nya mencakup HPS paket (PBJ-DEC-068).</summary>
    [HttpGet("{id:guid}/method-suggestion")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Package", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<MethodSuggestionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetMethodSuggestion(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _preparation.GetMethodSuggestionAsync(User, id, cancellationToken), "Saran metode berhasil diambil.");

    /// <summary>Langkah dan keputusan persetujuan persiapan <c>Wfl</c>; <c>data</c> kosong bila belum diajukan.</summary>
    [HttpGet("{id:guid}/approval")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Package", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<ProcurementApprovalTimelineResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetApproval(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _approvalTimeline.GetAsync(User, ProcurementDocumentTypes.Package, id, cancellationToken),
            "Persetujuan paket berhasil diambil.");

    [HttpGet("{id:guid}/history")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Package", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<List<DocumentHistoryResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetHistory(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetHistoryAsync(User, id, cancellationToken), "Riwayat paket berhasil diambil.");

    [HttpPost]
    [AccessAction(ProcurementPermissions.CreateAction, "Create Procurement Package", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission(Resource, ProcurementPermissions.CreateAction)]
    [ProducesResponseType(typeof(ApiResponse<PackageResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> Create([FromBody] CreatePackageRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.CreateAsync(User, request, cancellationToken), "Draf paket berhasil dibuat.", StatusCodes.Status201Created);

    /// <summary>Ubah persiapan: judul, spesifikasi dan HPS satuan barang, metode, alasan di luar rentang.</summary>
    [HttpPut("{id:guid}")]
    [AccessAction(ProcurementPermissions.UpdateAction, "Update Procurement Package", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission(Resource, ProcurementPermissions.UpdateAction)]
    [ProducesResponseType(typeof(ApiResponse<PackageResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> UpdatePreparation(Guid id, [FromBody] UpdatePackagePreparationRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _preparation.UpdateAsync(User, id, request, cancellationToken), "Persiapan paket berhasil disimpan.");

    [HttpPut("{id:guid}/items/{itemId:guid}/criteria")]
    [AccessAction(ProcurementPermissions.UpdateAction, "Update Procurement Package", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission(Resource, ProcurementPermissions.UpdateAction)]
    [ProducesResponseType(typeof(ApiResponse<PackageResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public Task<IActionResult> UpdateCriteria(
        Guid id, Guid itemId, [FromBody] UpdateTechnicalCriteriaRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _preparation.UpdateCriteriaAsync(User, id, itemId, request, cancellationToken), "Kriteria teknis berhasil disimpan.");

    [HttpPut("{id:guid}/evaluators")]
    [AccessAction(ProcurementPermissions.UpdateAction, "Update Procurement Package", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission(Resource, ProcurementPermissions.UpdateAction)]
    [ProducesResponseType(typeof(ApiResponse<PackageResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public Task<IActionResult> UpdateEvaluators(Guid id, [FromBody] UpdateEvaluatorsRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _preparation.UpdateEvaluatorsAsync(User, id, request, cancellationToken), "Evaluator paket berhasil disimpan.");

    /// <summary>Ajukan persiapan ke <c>Wfl</c>; nomor <c>PKT-…</c> dialokasikan saat pertama diajukan.</summary>
    [HttpPost("{id:guid}/preparation/submit")]
    [AccessAction(ProcurementPermissions.SubmitAction, "Submit Procurement Package", Description = "Mengajukan, menarik, dan menyinkronkan persetujuan persiapan paket", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission(Resource, ProcurementPermissions.SubmitAction)]
    [ProducesResponseType(typeof(ApiResponse<PackageResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> SubmitPreparation(
        Guid id,
        [FromBody] SubmitPackagePreparationRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken) =>
        ExecuteAsync(() => _approval.SubmitAsync(User, id, request, idempotencyKey, cancellationToken), "Persiapan paket berhasil diajukan.");

    [HttpPost("{id:guid}/preparation/withdraw")]
    [AccessAction(ProcurementPermissions.SubmitAction, "Submit Procurement Package", Description = "Mengajukan, menarik, dan menyinkronkan persetujuan persiapan paket", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission(Resource, ProcurementPermissions.SubmitAction)]
    [ProducesResponseType(typeof(ApiResponse<PackageResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public Task<IActionResult> WithdrawPreparation(Guid id, [FromBody] WithdrawPackagePreparationRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _approval.WithdrawAsync(User, id, request, cancellationToken), "Pengajuan persiapan berhasil ditarik.");

    /// <summary>Buka ulang persiapan yang sudah disetujui (HPS/metode dapat diubah lagi); alasan wajib.</summary>
    [HttpPost("{id:guid}/preparation/reopen")]
    [AccessAction(ProcurementPermissions.ReviseAction, "Revise Procurement Package", Description = "Membuka ulang persiapan paket yang sudah disetujui", AccessType = AccessTypes.Update, SortOrder = 5)]
    [AccessPermission(Resource, ProcurementPermissions.ReviseAction)]
    [ProducesResponseType(typeof(ApiResponse<PackageResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> ReopenPreparation(Guid id, [FromBody] ReopenPackagePreparationRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _approval.ReopenAsync(User, id, request, cancellationToken), "Persiapan paket berhasil dibuka ulang.");

    /// <summary>Sinkron ulang hasil <c>Wfl</c> yang gagal diterapkan.</summary>
    [HttpPost("{id:guid}/workflow/synchronize")]
    [AccessAction(ProcurementPermissions.SubmitAction, "Submit Procurement Package", Description = "Mengajukan, menarik, dan menyinkronkan persetujuan persiapan paket", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission(Resource, ProcurementPermissions.SubmitAction)]
    [ProducesResponseType(typeof(ApiResponse<PackageResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public Task<IActionResult> SynchronizeWorkflow(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _approval.SynchronizeWorkflowAsync(User, id, cancellationToken), "Status persetujuan paket berhasil disinkronkan.");

    [HttpPost("{id:guid}/lines")]
    [AccessAction(ProcurementPermissions.UpdateAction, "Update Procurement Package", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission(Resource, ProcurementPermissions.UpdateAction)]
    [ProducesResponseType(typeof(ApiResponse<PackageResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> AddLines(Guid id, [FromBody] AddPackageLinesRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.AddLinesAsync(User, id, request, cancellationToken), "Baris permintaan berhasil ditambahkan ke paket.");

    [HttpDelete("{id:guid}/sources/{sourceId:guid}")]
    [AccessAction(ProcurementPermissions.UpdateAction, "Update Procurement Package", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission(Resource, ProcurementPermissions.UpdateAction)]
    [ProducesResponseType(typeof(ApiResponse<PackageResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public Task<IActionResult> RemoveSource(
        Guid id, Guid sourceId, [FromQuery] RemovePackageSourceQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.RemoveSourceAsync(User, id, sourceId, request, cancellationToken), "Baris permintaan berhasil dilepas dari paket.");

    private async Task<IActionResult> ExecuteAsync<T>(Func<Task<T>> command, string message, int successStatus = StatusCodes.Status200OK)
    {
        try
        {
            var result = ApiResponse<T>.Ok(await command(), message);
            return successStatus == StatusCodes.Status200OK ? Ok(result) : StatusCode(successStatus, result);
        }
        catch (KeyNotFoundException exception) { return NotFound(ApiResponse<object>.Fail(404, exception.Message)); }
        catch (ProcurementSourcingBadRequestException exception) { return BadRequest(ApiResponse<object>.Fail(400, exception.Message)); }
        catch (ProcurementSourcingForbiddenException exception) { return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail(403, exception.Message)); }
        catch (ProcurementPeriodFilterException exception) { return UnprocessableEntity(ApiResponse<object>.Fail(422, exception.Message, new { code = exception.Code })); }
        catch (ProcurementSourcingValidationException exception) { return UnprocessableEntity(ApiResponse<object>.Fail(422, exception.Message, new { code = exception.Code })); }
        catch (ProcurementSourcingConflictException exception) { return Conflict(ApiResponse<object>.Fail(409, exception.Message, new { code = exception.Code })); }
        // Metode baris PR milik perencanaan-permintaan (PBJ-PP-INT 1.2 §2) melempar pengecualian Request.
        catch (ProcurementRequestConflictException exception) { return Conflict(ApiResponse<object>.Fail(409, exception.Message, new { code = exception.Code })); }
    }
}
