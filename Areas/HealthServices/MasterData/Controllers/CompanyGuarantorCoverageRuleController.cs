using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/master-data/company-guarantor-coverage-rules")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MASTER_DATA",
        moduleName: "Health Service Master Data",
        displayName: "Company Guarantor Coverage Rule",
        AreaName = "HealthServices",
        ControllerName = "CompanyGuarantorCoverageRule",
        Description = "Health service master data company guarantor coverage rule",
        SortOrder = 13
    )]
    [Tags("Health Services / Master Data / Company Guarantor Coverage Rule")]
    public class CompanyGuarantorCoverageRuleController : ControllerBase
    {
        private readonly ICompanyGuarantorCoverageRuleService _service;

        public CompanyGuarantorCoverageRuleController(ICompanyGuarantorCoverageRuleService service)
        {
            _service = service;
        }

        [HttpGet("filters/metadata")]
        [HttpGet("filter-metadata")]
        [ProducesResponseType(typeof(ApiResponse<CompanyGuarantorCoverageRuleFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Company Guarantor Coverage Rule",
            Description = "Melihat metadata filter aturan tanggungan perusahaan penjamin",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("CompanyGuarantorCoverageRule", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = await _service.GetFilterMetadataAsync();
            return Ok(ApiResponse<CompanyGuarantorCoverageRuleFilterMetadataResponse>.Ok(
                result,
                "Metadata filter aturan tanggungan berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<CompanyGuarantorCoverageRuleSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Company Guarantor Coverage Rule",
            Description = "Melihat ringkasan aturan tanggungan perusahaan penjamin",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("CompanyGuarantorCoverageRule", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var result = await _service.GetSummaryAsync();
            return Ok(ApiResponse<CompanyGuarantorCoverageRuleSummaryResponse>.Ok(
                result,
                "Ringkasan aturan tanggungan berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<CompanyGuarantorCoverageRuleResponse>>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Company Guarantor Coverage Rule",
            Description = "Melihat daftar aturan tanggungan perusahaan penjamin",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("CompanyGuarantorCoverageRule", "Read")]
        public async Task<IActionResult> GetRules(
            [FromQuery] Guid? companyGuarantorId,
            [FromQuery] string? itemType,
            [FromQuery] string? coverageStatus,
            [FromQuery] string? benefitPlanCode,
            [FromQuery] string? employeeGrade,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] string? sortBy = "sortOrder",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var result = await _service.GetRulesPagedAsync(
                companyGuarantorId,
                itemType,
                coverageStatus,
                benefitPlanCode,
                employeeGrade,
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

            return Ok(ApiResponse<PagedResult<CompanyGuarantorCoverageRuleResponse>>.Ok(
                result,
                "Daftar aturan tanggungan berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<CompanyGuarantorCoverageRuleOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Company Guarantor Coverage Rule",
            Description = "Melihat pilihan opsi aturan tanggungan perusahaan penjamin",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("CompanyGuarantorCoverageRule", "Read")]
        public async Task<IActionResult> GetRuleOptions(
            [FromQuery] Guid? companyGuarantorId,
            [FromQuery] string? itemType,
            [FromQuery] string? search,
            [FromQuery] bool onlyActive = true,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50)
        {
            var result = await _service.GetRuleOptionsAsync(
                companyGuarantorId,
                itemType,
                search,
                onlyActive,
                pageNumber,
                pageSize
            );

            return Ok(ApiResponse<List<CompanyGuarantorCoverageRuleOptionResponse>>.Ok(
                result,
                "Pilihan aturan tanggungan berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<CompanyGuarantorCoverageRuleDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Company Guarantor Coverage Rule",
            Description = "Melihat detail aturan tanggungan perusahaan penjamin",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("CompanyGuarantorCoverageRule", "Read")]
        public async Task<IActionResult> GetRuleById(Guid id)
        {
            var result = await _service.GetRuleByIdAsync(id);
            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Data aturan tanggungan tidak ditemukan atau sudah dihapus."
                ));
            }

            return Ok(ApiResponse<CompanyGuarantorCoverageRuleDetailResponse>.Ok(
                result,
                "Detail aturan tanggungan berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<CompanyGuarantorCoverageRuleResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(
            "Create",
            "Create Company Guarantor Coverage Rule",
            Description = "Menambah aturan tanggungan perusahaan penjamin",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("CompanyGuarantorCoverageRule", "Create")]
        public async Task<IActionResult> CreateRule([FromBody] CreateCompanyGuarantorCoverageRuleRequest request)
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
                var result = await _service.CreateRuleAsync(request, GetCurrentUserId());
                return StatusCode(StatusCodes.Status201Created, ApiResponse<CompanyGuarantorCoverageRuleResponse>.Ok(
                    result,
                    "Aturan tanggungan perusahaan penjamin berhasil dibuat."
                ));
            }
            catch (CompanyGuarantorCoverageRuleValidationException ex)
            {
                return StatusCode(ex.StatusCode, ApiResponse<object>.Fail(
                    ex.StatusCode,
                    ex.Message
                ));
            }
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<CompanyGuarantorCoverageRuleResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(
            "Update",
            "Update Company Guarantor Coverage Rule",
            Description = "Mengubah aturan tanggungan perusahaan penjamin",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("CompanyGuarantorCoverageRule", "Update")]
        public async Task<IActionResult> UpdateRule(
            Guid id,
            [FromBody] UpdateCompanyGuarantorCoverageRuleRequest request)
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
                var result = await _service.UpdateRuleAsync(id, request, GetCurrentUserId());
                return Ok(ApiResponse<CompanyGuarantorCoverageRuleResponse>.Ok(
                    result,
                    "Aturan tanggungan perusahaan penjamin berhasil diperbarui."
                ));
            }
            catch (CompanyGuarantorCoverageRuleValidationException ex)
            {
                return StatusCode(ex.StatusCode, ApiResponse<object>.Fail(
                    ex.StatusCode,
                    ex.Message
                ));
            }
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<CompanyGuarantorCoverageRuleResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(
            "Update",
            "Update Company Guarantor Coverage Rule Status",
            Description = "Mengubah status aktif aturan tanggungan perusahaan penjamin",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("CompanyGuarantorCoverageRule", "Update")]
        public async Task<IActionResult> UpdateRuleStatus(
            Guid id,
            [FromBody] UpdateCompanyGuarantorCoverageRuleStatusRequest request)
        {
            try
            {
                var result = await _service.UpdateRuleStatusAsync(id, request, GetCurrentUserId());
                return Ok(ApiResponse<CompanyGuarantorCoverageRuleResponse>.Ok(
                    result,
                    "Status aturan tanggungan perusahaan penjamin berhasil diperbarui."
                ));
            }
            catch (CompanyGuarantorCoverageRuleValidationException ex)
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
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(
            "Delete",
            "Delete Company Guarantor Coverage Rule",
            Description = "Menghapus aturan tanggungan perusahaan penjamin",
            AccessType = AccessTypes.Delete,
            SortOrder = 4
        )]
        [AccessPermission("CompanyGuarantorCoverageRule", "Delete")]
        public async Task<IActionResult> DeleteRule(
            Guid id,
            [FromBody] DeleteCompanyGuarantorCoverageRuleRequest? request = null)
        {
            try
            {
                var result = await _service.DeleteRuleAsync(id, request?.DeleteReason, GetCurrentUserId());
                if (!result)
                {
                    return NotFound(ApiResponse<object>.Fail(
                        StatusCodes.Status404NotFound,
                        "Data aturan tanggungan tidak ditemukan atau sudah dihapus."
                    ));
                }

                return Ok(ApiResponse<bool>.Ok(
                    result,
                    "Aturan tanggungan perusahaan penjamin berhasil dihapus."
                ));
            }
            catch (CompanyGuarantorCoverageRuleValidationException ex)
            {
                return StatusCode(ex.StatusCode, ApiResponse<object>.Fail(
                    ex.StatusCode,
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
