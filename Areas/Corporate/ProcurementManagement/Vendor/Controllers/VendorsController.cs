using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.DTOs;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Services;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Vendor.DTOs;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Vendor.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Vendor.Controllers;

/// <summary>
/// Vendor Pengadaan (BE-PBJ-VD-003, 005, 006; PBJ-VD-API 1.0 §Vendor): daftar, ringkasan, detail, kategori, riwayat,
/// kelayakan per kategori, dan status Dibatasi. Kunci di
/// URL adalah <c>supplierId</c> (<c>MstSupplier.Id</c>); vendor dibuat di Master Data Supplier, bukan di sini
/// (PBJ-DEC-053). Data vendor tidak dibatasi per unit. Controller hanya meneruskan ke
/// <see cref="ProcurementVendorProfileService"/> (QBE-SVC-001).
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/procurement-management/vendors")]
[AccessController(ProcurementPermissions.VendorModuleCode, ProcurementPermissions.VendorModuleName, "Procurement Vendor",
    AreaName = ProcurementPermissions.AreaName, ControllerName = ProcurementPermissions.VendorResource,
    Description = "Daftar, detail, kategori, dan riwayat vendor Pengadaan", SortOrder = 1)]
[Tags("Corporate / Procurement Management / Vendor / Vendor")]
public sealed class VendorsController : ControllerBase
{
    private const string Resource = ProcurementPermissions.VendorResource;
    private readonly ProcurementVendorProfileService _service;
    private readonly ProcurementVendorEligibilityService _eligibilityService;

    public VendorsController(ProcurementVendorProfileService service, ProcurementVendorEligibilityService eligibilityService)
    {
        _service = service;
        _eligibilityService = eligibilityService;
    }

    [HttpGet("filters/metadata")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Vendor", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<VendorFilterMetadataResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetFilterMetadata(CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetFilterMetadataAsync(cancellationToken), "Metadata filter vendor berhasil diambil.");

    [HttpGet("summary")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Vendor", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<VendorSummaryResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetSummary(CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetSummaryAsync(cancellationToken), "Ringkasan vendor berhasil diambil.");

    [HttpGet]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Vendor", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<VendorListItemResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> Get([FromQuery] VendorQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetPagedAsync(request, cancellationToken), "Daftar vendor berhasil diambil.");

    /// <summary>
    /// Kelayakan vendor untuk satu kategori (BE-PBJ-VD-005). <c>itemCategoryId</c> wajib; <c>supplierId</c> opsional
    /// (kosong = semua vendor yang melayani kategori); <c>onlyEligible=true</c> untuk daftar pilihan.
    /// </summary>
    [HttpGet("eligibility")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Vendor", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<List<VendorEligibilityResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetEligibility([FromQuery] VendorEligibilityQuery request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _eligibilityService.EvaluateAsync(
            request.ItemCategoryId!.Value, request.SupplierId, request.OnlyEligible, cancellationToken), "Kelayakan vendor berhasil diambil.");

    [HttpGet("{supplierId:guid}")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Vendor", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<VendorDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetById(Guid supplierId, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetDetailAsync(supplierId, cancellationToken), "Detail vendor berhasil diambil.");

    [HttpGet("{supplierId:guid}/history")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Vendor", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<List<DocumentHistoryResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetHistory(Guid supplierId, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetHistoryAsync(supplierId, cancellationToken), "Riwayat vendor berhasil diambil.");

    [HttpPut("{supplierId:guid}/categories")]
    [AccessAction(ProcurementPermissions.UpdateAction, "Update Procurement Vendor", AccessType = AccessTypes.Update, SortOrder = 2)]
    [AccessPermission(Resource, ProcurementPermissions.UpdateAction)]
    [ProducesResponseType(typeof(ApiResponse<VendorDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> UpdateCategories(
        Guid supplierId, [FromBody] UpdateVendorCategoriesRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.UpdateCategoriesAsync(supplierId, request, CurrentUserId(), cancellationToken), "Kategori vendor berhasil disimpan.");

    [HttpPost("{supplierId:guid}/restriction")]
    [AccessAction(ProcurementPermissions.RestrictAction, "Restrict Procurement Vendor", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission(Resource, ProcurementPermissions.RestrictAction)]
    [ProducesResponseType(typeof(ApiResponse<VendorDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> Restrict(
        Guid supplierId,
        [FromBody] RestrictVendorRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.RestrictAsync(supplierId, request, idempotencyKey, CurrentUserId(), cancellationToken), "Vendor berhasil dibatasi.");

    [HttpPost("{supplierId:guid}/restriction/lift")]
    [AccessAction(ProcurementPermissions.RestrictAction, "Restrict Procurement Vendor", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission(Resource, ProcurementPermissions.RestrictAction)]
    [ProducesResponseType(typeof(ApiResponse<VendorDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> LiftRestriction(Guid supplierId, [FromBody] LiftRestrictionRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.LiftRestrictionAsync(supplierId, request, CurrentUserId(), cancellationToken), "Pembatasan vendor berhasil dicabut.");

    private async Task<IActionResult> ExecuteAsync<T>(Func<Task<T>> command, string message)
    {
        try { return Ok(ApiResponse<T>.Ok(await command(), message)); }
        catch (KeyNotFoundException exception) { return NotFound(ApiResponse<object>.Fail(404, exception.Message)); }
        catch (ProcurementPeriodFilterException exception) { return UnprocessableEntity(ApiResponse<object>.Fail(422, exception.Message, new { code = exception.Code })); }
        catch (ProcurementVendorConflictException exception) { return Conflict(ApiResponse<object>.Fail(409, exception.Message, new { code = exception.Code })); }
        catch (ProcurementVendorValidationException exception)
        {
            return UnprocessableEntity(exception.Code == null
                ? ApiResponse<object>.Fail(422, exception.Message)
                : ApiResponse<object>.Fail(422, exception.Message, new { code = exception.Code }));
        }
    }

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }
}
