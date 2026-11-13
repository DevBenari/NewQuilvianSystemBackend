using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Controllers;

/// <summary>
/// Katalog barang/jasa Pengadaan (BE-PBJ-003, PBJ-DEC-022, PBJ-DEC-037) — sembilan endpoint baku
/// master data. Kode <c>KTL-…</c> dibuat server; frontend tidak mengirimnya.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/procurement-management/master-data/items")]
[AccessController(ProcurementPermissions.MasterDataModuleCode, ProcurementPermissions.MasterDataModuleName, "Procurement Item",
    AreaName = ProcurementPermissions.AreaName, ControllerName = ProcurementPermissions.ItemResource,
    Description = "Katalog barang/jasa Pengadaan", SortOrder = 1)]
[Tags("Corporate / Procurement Management / Master Data / Item")]
public sealed class ProcurementItemsController : ControllerBase
{
    private const string Resource = ProcurementPermissions.ItemResource;
    private readonly ProcurementItemService _service;

    public ProcurementItemsController(ProcurementItemService service) => _service = service;

    [HttpGet("filters/metadata")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Item", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<ProcurementItemFilterMetadataResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFilterMetadata(CancellationToken cancellationToken) =>
        Ok(ApiResponse<ProcurementItemFilterMetadataResponse>.Ok(
            await _service.GetFilterMetadataAsync(cancellationToken), "Metadata filter katalog berhasil diambil."));

    [HttpGet("summary")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Item", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<ProcurementItemSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken) =>
        Ok(ApiResponse<ProcurementItemSummaryResponse>.Ok(
            await _service.GetSummaryAsync(cancellationToken), "Ringkasan katalog berhasil diambil."));

    [HttpGet]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Item", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ProcurementItemResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] ProcurementItemQuery request, CancellationToken cancellationToken) =>
        Ok(ApiResponse<PagedResult<ProcurementItemResponse>>.Ok(
            await _service.GetPagedAsync(request, cancellationToken), "Daftar katalog berhasil diambil."));

    [HttpGet("options")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Item", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ProcurementItemOptionResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOptions([FromQuery] ProcurementItemOptionsQuery request, CancellationToken cancellationToken) =>
        Ok(ApiResponse<PagedResult<ProcurementItemOptionResponse>>.Ok(
            await _service.GetOptionsAsync(request, cancellationToken), "Pilihan barang/jasa berhasil diambil."));

    [HttpGet("{id:guid}")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Item", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<ProcurementItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetByIdAsync(id, cancellationToken), "Detail barang/jasa berhasil diambil.");

    [HttpPost]
    [AccessAction(ProcurementPermissions.CreateAction, "Create Procurement Item", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission(Resource, ProcurementPermissions.CreateAction)]
    [ProducesResponseType(typeof(ApiResponse<ProcurementItemResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Create([FromBody] UpsertProcurementItemRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.CreateAsync(request, CurrentUserId(), cancellationToken), "Barang/jasa berhasil ditambahkan ke katalog.");

    [HttpPut("{id:guid}")]
    [AccessAction(ProcurementPermissions.UpdateAction, "Update Procurement Item", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission(Resource, ProcurementPermissions.UpdateAction)]
    [ProducesResponseType(typeof(ApiResponse<ProcurementItemResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Update(Guid id, [FromBody] UpsertProcurementItemRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.UpdateAsync(id, request, CurrentUserId(), cancellationToken), "Barang/jasa berhasil diubah.");

    [HttpPatch("{id:guid}/status")]
    [AccessAction(ProcurementPermissions.UpdateAction, "Update Procurement Item", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission(Resource, ProcurementPermissions.UpdateAction)]
    [ProducesResponseType(typeof(ApiResponse<ProcurementItemResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateProcurementMasterStatusRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.UpdateStatusAsync(id, request, CurrentUserId(), cancellationToken), "Status barang/jasa berhasil diubah.");

    [HttpDelete("{id:guid}")]
    [AccessAction(ProcurementPermissions.DeleteAction, "Delete Procurement Item", AccessType = AccessTypes.Delete, SortOrder = 4)]
    [AccessPermission(Resource, ProcurementPermissions.DeleteAction)]
    [ProducesResponseType(typeof(ApiResponse<ProcurementMasterDeleteResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.DeleteAsync(id, CurrentUserId(), cancellationToken), "Barang/jasa berhasil dihapus.");

    private async Task<IActionResult> ExecuteAsync<T>(Func<Task<T>> command, string message)
    {
        try { return Ok(ApiResponse<T>.Ok(await command(), message)); }
        catch (KeyNotFoundException exception) { return NotFound(ApiResponse<object>.Fail(404, exception.Message)); }
        catch (ProcurementMasterInUseException exception) { return BadRequest(ApiResponse<object>.Fail(400, exception.Message)); }
        catch (ProcurementMasterConflictException exception) { return Conflict(ApiResponse<object>.Fail(409, exception.Message)); }
        catch (ProcurementMasterValidationException exception) { return UnprocessableEntity(ApiResponse<object>.Fail(422, exception.Message)); }
    }

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }
}
