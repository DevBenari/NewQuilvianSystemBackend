using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Controllers
{
    /// <summary>
    /// Registri dokumen administrasi kepegawaian lintas jenis (BE-HRD-385): SK, Surat Tugas, Surat Keterangan, dan
    /// dokumen personal dalam satu daftar berpaging dengan filter server.
    /// </summary>
    [ApiController]
    [Route("api/v1/corporate/human-resource/administrative-documents")]
    [Authorize]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_SERVICE_MANAGEMENT",
        moduleName: "Human Resource Service Management",
        displayName: "Registri Dokumen Administrasi",
        AreaName = "Corporate",
        ControllerName = "AdministrativeDocumentRegistry",
        Description = "Registri SK, Surat Tugas, Surat Keterangan, dan dokumen personal pegawai",
        SortOrder = 12)]
    [Tags("HR Services - Administrative Document Registry")]
    public class AdministrativeDocumentRegistryController : ControllerBase
    {
        private readonly AdministrativeDocumentRegistryService _service;

        public AdministrativeDocumentRegistryController(AdministrativeDocumentRegistryService service)
        {
            _service = service;
        }

        /// <summary>
        /// Daftar dokumen. Filter: <c>source</c>, <c>documentType</c>, <c>workforceProfileId</c>, <c>organizationUnitId</c>,
        /// <c>startDate</c>/<c>endDate</c>, <c>validity</c> (BERLAKU/TERJADWAL/TIDAK_BERLAKU/KEDALUWARSA), <c>isConfidential</c>, <c>search</c>.
        /// </summary>
        [HttpGet]
        [AccessAction("Read", "Melihat Registri Dokumen Administrasi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AdministrativeDocumentRegistry", "Read")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<AdministrativeDocumentRegistryItem>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPaged([FromQuery] AdministrativeDocumentQuery query, CancellationToken cancellationToken)
        {
            var result = await _service.GetPagedAsync(query, User, cancellationToken);
            return Ok(ApiResponse<PagedResult<AdministrativeDocumentRegistryItem>>.Ok(result, "Registri dokumen administrasi berhasil diambil."));
        }
    }
}
