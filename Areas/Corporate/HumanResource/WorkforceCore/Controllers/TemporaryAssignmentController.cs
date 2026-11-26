using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Controllers
{
    // Daftar baca penempatan sementara lintas pegawai (HRD-DEC-251, kontrak v31 API 37.6). Hanya baca; pencatatan dan
    // perubahan tetap lewat WfpOrganizationAssignmentController (Profil Pegawai 360).
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-core/temporary-assignments")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFORCE_CORE",
        moduleName: "Human Resource Workforce Core",
        displayName: "Temporary Assignment",
        AreaName = "Corporate",
        ControllerName = "TemporaryAssignment",
        Description = "Daftar baca penempatan sementara lintas pegawai dalam cakupan unit",
        SortOrder = 27)]
    [Tags("Corporate / Human Resource / Workforce Core / Temporary Assignment")]
    public class TemporaryAssignmentController : ControllerBase
    {
        private readonly TemporaryAssignmentQueryService _service;

        public TemporaryAssignmentController(TemporaryAssignmentQueryService service)
        {
            _service = service;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<TemporaryAssignmentFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Temporary Assignment", Description = "Melihat daftar penempatan sementara lintas pegawai", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("TemporaryAssignment", "Read")]
        public IActionResult GetFilterMetadata()
        {
            return Ok(ApiResponse<TemporaryAssignmentFilterMetadataResponse>.Ok(
                _service.GetFilterMetadata(),
                "Metadata penempatan sementara berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<TemporaryAssignmentResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [AccessAction("Read", "Read Temporary Assignment", Description = "Melihat daftar penempatan sementara lintas pegawai", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("TemporaryAssignment", "Read")]
        public async Task<IActionResult> GetPaged([FromQuery] TemporaryAssignmentPagedQuery query, CancellationToken cancellationToken)
        {
            var result = await _service.GetPagedAsync(query, cancellationToken);
            return result.Success
                ? Ok(ApiResponse<PagedResult<TemporaryAssignmentResponse>>.Ok(result.Data, result.Message))
                : StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));
        }
    }
}
