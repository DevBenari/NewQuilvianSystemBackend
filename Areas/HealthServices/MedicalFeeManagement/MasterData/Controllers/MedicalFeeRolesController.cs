using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.MasterData.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.MasterData.Controllers
{
    /// <summary>
    /// Controller pengelola data master peran pelaksana jasa medis (MstMedicalFeeRole).
    /// Menyediakan 9 endpoint baseline sesuai master-data-endpoint-standard.md dan api-contract.md MDF-API-1.0 §2.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/medical-fee-management/master-data/fee-roles")]
    [AccessController(
        moduleCode: "MEDICAL_FEE_MANAGEMENT",
        moduleName: "Medical Fee Management",
        displayName: "Medical Fee Role",
        AreaName = "HealthServices",
        ControllerName = "MedicalFeeRoles",
        Description = "Mengelola data peran pelaksana jasa medis",
        SortOrder = 1
    )]
    [Tags("Health Services / Medical Fee Management / Master Data / Fee Role")]
    public class MedicalFeeRolesController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MedicalFeeManagement.MasterData";

        private readonly MedicalFeeRoleService _service;
        private readonly LoggerService _loggerService;

        public MedicalFeeRolesController(
            MedicalFeeRoleService service,
            LoggerService loggerService)
        {
            _service = service;
            _loggerService = loggerService;
        }

        // =====================================================================
        // 1. GET /filters/metadata
        // =====================================================================

        /// <summary>
        /// Mengambil metadata konfigurasi filter, sort, dan informasi isian form.
        /// </summary>
        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<MedicalFeeRoleFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Medical Fee Role", Description = "Melihat metadata filter peran jasa medis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicalFeeRoles", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var metadata = _service.GetFilterMetadata();
            return Ok(ApiResponse<MedicalFeeRoleFilterMetadataResponse>.Ok(metadata, "Metadata filter peran jasa medis berhasil diambil."));
        }

        // =====================================================================
        // 2. GET /summary
        // =====================================================================

        /// <summary>
        /// Mengambil ringkasan statistik peran (total, aktif, nonaktif, peran utama, pemetaan OK).
        /// </summary>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<MedicalFeeRoleSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Medical Fee Role", Description = "Melihat ringkasan peran jasa medis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicalFeeRoles", "Read")]
        public async Task<IActionResult> GetSummary(CancellationToken ct = default)
        {
            var summary = await _service.GetSummaryAsync(ct);
            return Ok(ApiResponse<MedicalFeeRoleSummaryResponse>.Ok(summary, "Ringkasan data peran jasa medis berhasil diambil."));
        }

        // =====================================================================
        // 3. GET / (List Paged Table)
        // =====================================================================

        /// <summary>
        /// Mengambil daftar peran jasa medis dengan filter pencarian, status aktif, dan pagination.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<MedicalFeeRoleResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Medical Fee Role", Description = "Melihat daftar peran jasa medis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicalFeeRoles", "Read")]
        public async Task<IActionResult> GetPaged([FromQuery] MedicalFeeRolePagedQuery query, CancellationToken ct = default)
        {
            var result = await _service.GetPagedAsync(query, ct);
            return Ok(ApiResponse<PagedResult<MedicalFeeRoleResponse>>.Ok(result, "Daftar peran jasa medis berhasil diambil."));
        }

        // =====================================================================
        // 4. GET /options (Dropdown Feed)
        // =====================================================================

        /// <summary>
        /// Mengambil daftar opsi ringan peran untuk komponen dropdown select pada aturan tarif.
        /// </summary>
        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<MedicalFeeRoleOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Medical Fee Role", Description = "Melihat opsi dropdown peran jasa medis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicalFeeRoles", "Read")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            CancellationToken ct = default)
        {
            var options = await _service.GetOptionsAsync(onlyActive, search, ct);
            return Ok(ApiResponse<List<MedicalFeeRoleOptionResponse>>.Ok(options, "Opsi peran jasa medis berhasil diambil."));
        }

        // =====================================================================
        // 5. GET /{id} (Single Detail)
        // =====================================================================

        /// <summary>
        /// Mengambil detail satu peran pelaksana jasa medis berdasarkan ID.
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<MedicalFeeRoleResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Medical Fee Role", Description = "Melihat detail peran jasa medis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicalFeeRoles", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
        {
            var role = await _service.GetByIdAsync(id, ct);
            if (role == null)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Peran jasa medis tidak ditemukan atau sudah dihapus."));
            }

            return Ok(ApiResponse<MedicalFeeRoleResponse>.Ok(role, "Detail peran jasa medis berhasil diambil."));
        }

        // =====================================================================
        // 6. POST / (Create)
        // =====================================================================

        /// <summary>
        /// Menambahkan peran pelaksana jasa medis baru.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<MedicalFeeRoleResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Create", "Create Medical Fee Role", Description = "Menambah peran jasa medis", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("MedicalFeeRoles", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateMedicalFeeRoleRequest request,
            CancellationToken ct = default)
        {
            var actorUserId = GetCurrentUserId();
            var result = await _service.CreateAsync(request, actorUserId, ct);

            if (!result.Success)
            {
                await _loggerService.WarningAsync(LogCategory, "MedicalFeeRole.Create", result.Message, new { request, actorUserId });
                return ToActionResult(result);
            }

            await _loggerService.InfoAsync(LogCategory, "MedicalFeeRole.Create", result.Message, new { result.Data?.Id, result.Data?.RoleCode, actorUserId });
            return ToActionResult(result);
        }

        // =====================================================================
        // 7. PUT /{id} (Full Update)
        // =====================================================================

        /// <summary>
        /// Mengubah seluruh atribut bisnis peran pelaksana jasa medis.
        /// </summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<MedicalFeeRoleResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Medical Fee Role", Description = "Mengubah peran jasa medis", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("MedicalFeeRoles", "Update")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateMedicalFeeRoleRequest request,
            CancellationToken ct = default)
        {
            var actorUserId = GetCurrentUserId();
            var result = await _service.UpdateAsync(id, request, actorUserId, ct);

            if (!result.Success)
            {
                await _loggerService.WarningAsync(LogCategory, "MedicalFeeRole.Update", result.Message, new { id, request, actorUserId });
                return ToActionResult(result);
            }

            await _loggerService.InfoAsync(LogCategory, "MedicalFeeRole.Update", result.Message, new { id, result.Data?.RoleCode, actorUserId });
            return ToActionResult(result);
        }

        // =====================================================================
        // 8. PATCH /{id}/activation & PATCH /{id}/status (Toggle Status)
        // =====================================================================

        /// <summary>
        /// Mengubah status aktif / nonaktif peran pelaksana jasa medis (rute MDF-API-1.0 /activation).
        /// </summary>
        [HttpPatch("{id:guid}/activation")]
        [ProducesResponseType(typeof(ApiResponse<MedicalFeeRoleResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Medical Fee Role", Description = "Mengubah status aktif peran jasa medis", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("MedicalFeeRoles", "Update")]
        public async Task<IActionResult> ToggleActivation(
            Guid id,
            [FromBody] ToggleMedicalFeeRoleActivationRequest request,
            CancellationToken ct = default)
        {
            return await HandleToggleStatus(id, request.IsActive, ct);
        }

        /// <summary>
        /// Mengubah status aktif / nonaktif peran pelaksana jasa medis (rute standar master data /status).
        /// </summary>
        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<MedicalFeeRoleResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Medical Fee Role", Description = "Mengubah status aktif peran jasa medis", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("MedicalFeeRoles", "Update")]
        public async Task<IActionResult> ToggleStatus(
            Guid id,
            [FromBody] ToggleMedicalFeeRoleActivationRequest request,
            CancellationToken ct = default)
        {
            return await HandleToggleStatus(id, request.IsActive, ct);
        }

        // =====================================================================
        // 9. DELETE /{id} (Soft Delete)
        // =====================================================================

        /// <summary>
        /// Menandai peran sebagai terhapus (soft delete). Ditolak (422) bila sudah digunakan baris tarif sharing.
        /// </summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Delete", "Delete Medical Fee Role", Description = "Menghapus peran jasa medis", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("MedicalFeeRoles", "Delete")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
        {
            var actorUserId = GetCurrentUserId();
            var result = await _service.DeleteAsync(id, actorUserId, ct);

            if (!result.Success)
            {
                await _loggerService.WarningAsync(LogCategory, "MedicalFeeRole.Delete", result.Message, new { id, actorUserId });
                return ToActionResult(result);
            }

            await _loggerService.InfoAsync(LogCategory, "MedicalFeeRole.Delete", result.Message, new { id, actorUserId });
            return ToActionResult(result);
        }

        // =====================================================================
        // Internal Helpers
        // =====================================================================

        private async Task<IActionResult> HandleToggleStatus(Guid id, bool isActive, CancellationToken ct)
        {
            var actorUserId = GetCurrentUserId();
            var result = await _service.ToggleActivationAsync(id, isActive, actorUserId, ct);

            if (!result.Success)
            {
                await _loggerService.WarningAsync(LogCategory, "MedicalFeeRole.ToggleStatus", result.Message, new { id, isActive, actorUserId });
                return ToActionResult(result);
            }

            await _loggerService.InfoAsync(LogCategory, "MedicalFeeRole.ToggleStatus", result.Message, new { id, isActive, actorUserId });
            return ToActionResult(result);
        }

        private IActionResult ToActionResult<T>(MedicalFeeServiceResult<T> result)
        {
            if (result.Success)
            {
                return StatusCode(result.StatusCode, ApiResponse<T>.Ok(result.Data, result.Message));
            }

            object? errorDetails = result.ErrorCode != null ? new { errorCode = result.ErrorCode } : null;
            return StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message, errorDetails));
        }

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}

