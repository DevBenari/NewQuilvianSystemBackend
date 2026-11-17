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
/// Jenis pengadaan — <b>hanya baca</b> (BE-PBJ-003, PBJ-DEC-024, PBJ-DEC-037). Varian yang
/// dibenarkan dari sembilan endpoint baku: tidak ada create/update/status/delete, karena jenis
/// menentukan alur serah terima dan hanya diisi lewat data awal.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/procurement-management/master-data/procurement-types")]
[AccessController(ProcurementPermissions.MasterDataModuleCode, ProcurementPermissions.MasterDataModuleName, "Procurement Type",
    AreaName = ProcurementPermissions.AreaName, ControllerName = ProcurementPermissions.ProcurementTypeResource,
    Description = "Jenis pengadaan (hanya baca)", SortOrder = 4)]
[Tags("Corporate / Procurement Management / Master Data / Procurement Type")]
public sealed class ProcurementTypesController : ControllerBase
{
    private const string Resource = ProcurementPermissions.ProcurementTypeResource;
    private readonly ProcurementTypeService _service;

    public ProcurementTypesController(ProcurementTypeService service) => _service = service;

    [HttpGet]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Type", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ProcurementTypeResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] ProcurementReferenceQuery request, CancellationToken cancellationToken) =>
        Ok(ApiResponse<PagedResult<ProcurementTypeResponse>>.Ok(
            await _service.GetPagedAsync(request, cancellationToken), "Daftar jenis pengadaan berhasil diambil."));

    [HttpGet("options")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Type", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<List<ProcurementTypeResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOptions([FromQuery] bool onlyActive = true, CancellationToken cancellationToken = default) =>
        Ok(ApiResponse<List<ProcurementTypeResponse>>.Ok(
            await _service.GetOptionsAsync(onlyActive, cancellationToken), "Pilihan jenis pengadaan berhasil diambil."));

    [HttpGet("{id:guid}")]
    [AccessAction(ProcurementPermissions.ReadAction, "Read Procurement Type", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission(Resource, ProcurementPermissions.ReadAction)]
    [ProducesResponseType(typeof(ApiResponse<ProcurementTypeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(ApiResponse<ProcurementTypeResponse>.Ok(
                await _service.GetByIdAsync(id, cancellationToken), "Detail jenis pengadaan berhasil diambil."));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<object>.Fail(404, exception.Message));
        }
    }
}
