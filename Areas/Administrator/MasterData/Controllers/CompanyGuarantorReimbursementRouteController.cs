using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Administrator.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Administrator.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/administrator/master-data/company-guarantor-reimbursement-routes")]
    [AccessController(
        moduleCode: "ADMINISTRATOR_MASTER_DATA",
        moduleName: "Administrator Master Data",
        displayName: "Company Guarantor Reimbursement Route",
        AreaName = "Administrator",
        ControllerName = "CompanyGuarantorReimbursementRoute",
        Description = "Administrator master data company guarantor reimbursement route",
        SortOrder = 16
    )]
    [Tags("Administrator / Master Data / Company Guarantor Reimbursement Route")]
    public class CompanyGuarantorReimbursementRouteController : ControllerBase
    {
        private readonly ICompanyGuarantorReimbursementRouteService _service;

        public CompanyGuarantorReimbursementRouteController(ICompanyGuarantorReimbursementRouteService service)
        {
            _service = service;
        }

        [HttpGet("filters/metadata")]
        [HttpGet("filter-metadata")]
        [ProducesResponseType(typeof(ApiResponse<CompanyGuarantorReimbursementRouteFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Company Guarantor Reimbursement Route",
            Description = "Melihat metadata filter rute reimbursement perusahaan penjamin",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("CompanyGuarantorReimbursementRoute", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = await _service.GetFilterMetadataAsync();
            return Ok(ApiResponse<CompanyGuarantorReimbursementRouteFilterMetadataResponse>.Ok(
                result,
                "Metadata filter rute reimbursement berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<CompanyGuarantorReimbursementRouteSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Company Guarantor Reimbursement Route",
            Description = "Melihat ringkasan rute reimbursement perusahaan penjamin",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("CompanyGuarantorReimbursementRoute", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var result = await _service.GetSummaryAsync();
            return Ok(ApiResponse<CompanyGuarantorReimbursementRouteSummaryResponse>.Ok(
                result,
                "Ringkasan rute reimbursement berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<CompanyGuarantorReimbursementRouteResponse>>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Company Guarantor Reimbursement Route",
            Description = "Melihat daftar rute reimbursement perusahaan penjamin",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("CompanyGuarantorReimbursementRoute", "Read")]
        public async Task<IActionResult> GetRoutes(
            [FromQuery] Guid? companyGuarantorId,
            [FromQuery] Guid? insuranceProviderId,
            [FromQuery] string? routeType,
            [FromQuery] bool? isDefault,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] string? sortBy = "priority",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var result = await _service.GetRoutesPagedAsync(
                companyGuarantorId,
                insuranceProviderId,
                routeType,
                isDefault,
                isActive,
                search,
                startDate,
                endDate,
                customPeriod,
                sortBy,
                sortDirection,
                pageNumber,
                pageSize
            );

            return Ok(ApiResponse<PagedResult<CompanyGuarantorReimbursementRouteResponse>>.Ok(
                result,
                "Daftar rute reimbursement berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<CompanyGuarantorReimbursementRouteOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Company Guarantor Reimbursement Route",
            Description = "Melihat data pilihan rute reimbursement perusahaan penjamin",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("CompanyGuarantorReimbursementRoute", "Read")]
        public async Task<IActionResult> GetRouteOptions(
            [FromQuery] Guid? companyGuarantorId,
            [FromQuery] string? routeType,
            [FromQuery] string? search,
            [FromQuery] bool onlyActive = true,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50)
        {
            var result = await _service.GetRouteOptionsAsync(
                companyGuarantorId,
                routeType,
                search,
                onlyActive,
                pageNumber,
                pageSize
            );

            return Ok(ApiResponse<List<CompanyGuarantorReimbursementRouteOptionResponse>>.Ok(
                result,
                "Data pilihan rute reimbursement berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<CompanyGuarantorReimbursementRouteDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Company Guarantor Reimbursement Route",
            Description = "Melihat detail rute reimbursement perusahaan penjamin",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("CompanyGuarantorReimbursementRoute", "Read")]
        public async Task<IActionResult> GetRouteById(Guid id)
        {
            var result = await _service.GetRouteByIdAsync(id);
            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Data rute reimbursement tidak ditemukan atau sudah dihapus."
                ));
            }

            return Ok(ApiResponse<CompanyGuarantorReimbursementRouteDetailResponse>.Ok(
                result,
                "Detail rute reimbursement berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<CompanyGuarantorReimbursementRouteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(
            "Create",
            "Create Company Guarantor Reimbursement Route",
            Description = "Menambah rute reimbursement perusahaan penjamin",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("CompanyGuarantorReimbursementRoute", "Create")]
        public async Task<IActionResult> CreateRoute([FromBody] CreateCompanyGuarantorReimbursementRouteRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Permintaan tidak valid."
                ));
            }

            try
            {
                var result = await _service.CreateRouteAsync(request, GetCurrentUserId());
                return Ok(ApiResponse<CompanyGuarantorReimbursementRouteResponse>.Ok(
                    result,
                    "Rute reimbursement berhasil dibuat."
                ));
            }
            catch (CompanyGuarantorReimbursementRouteValidationException ex)
            {
                return StatusCode(ex.StatusCode, ApiResponse<object>.Fail(
                    ex.StatusCode,
                    ex.Message
                ));
            }
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<CompanyGuarantorReimbursementRouteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(
            "Update",
            "Update Company Guarantor Reimbursement Route",
            Description = "Mengubah rute reimbursement perusahaan penjamin",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("CompanyGuarantorReimbursementRoute", "Update")]
        public async Task<IActionResult> UpdateRoute(
            Guid id,
            [FromBody] UpdateCompanyGuarantorReimbursementRouteRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Permintaan tidak valid."
                ));
            }

            try
            {
                var result = await _service.UpdateRouteAsync(id, request, GetCurrentUserId());
                return Ok(ApiResponse<CompanyGuarantorReimbursementRouteResponse>.Ok(
                    result,
                    "Rute reimbursement berhasil diperbarui."
                ));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    ex.Message
                ));
            }
            catch (CompanyGuarantorReimbursementRouteValidationException ex)
            {
                return StatusCode(ex.StatusCode, ApiResponse<object>.Fail(
                    ex.StatusCode,
                    ex.Message
                ));
            }
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<CompanyGuarantorReimbursementRouteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(
            "Update",
            "Update Company Guarantor Reimbursement Route Status",
            Description = "Mengubah status rute reimbursement perusahaan penjamin",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("CompanyGuarantorReimbursementRoute", "Update")]
        public async Task<IActionResult> UpdateRouteStatus(
            Guid id,
            [FromBody] UpdateCompanyGuarantorReimbursementRouteStatusRequest request)
        {
            try
            {
                var result = await _service.UpdateRouteStatusAsync(id, request, GetCurrentUserId());
                return Ok(ApiResponse<CompanyGuarantorReimbursementRouteResponse>.Ok(
                    result,
                    "Status rute reimbursement berhasil diperbarui."
                ));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    ex.Message
                ));
            }
            catch (CompanyGuarantorReimbursementRouteValidationException ex)
            {
                return StatusCode(ex.StatusCode, ApiResponse<object>.Fail(
                    ex.StatusCode,
                    ex.Message
                ));
            }
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Delete",
            "Delete Company Guarantor Reimbursement Route",
            Description = "Menghapus rute reimbursement perusahaan penjamin",
            AccessType = AccessTypes.Delete,
            SortOrder = 4
        )]
        [AccessPermission("CompanyGuarantorReimbursementRoute", "Delete")]
        public async Task<IActionResult> DeleteRoute(Guid id)
        {
            try
            {
                var result = await _service.DeleteRouteAsync(id, GetCurrentUserId());
                return Ok(ApiResponse<bool>.Ok(
                    result,
                    "Rute reimbursement berhasil dihapus."
                ));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    ex.Message
                ));
            }
        }

        private Guid GetCurrentUserId()
        {
            var userIdText =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("user_id");

            return Guid.TryParse(userIdText, out var userId)
                ? userId
                : Guid.Empty;
        }
    }
}
