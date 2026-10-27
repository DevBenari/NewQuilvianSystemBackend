using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.MasterData.Services;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.MasterData.Controllers
{
    /// <summary>
    /// Controller pengelola kesepakatan tarif sharing (MdfSharingAgreement) dan baris aturan tarif berversi (MdfSharingRule).
    /// Mengimplementasikan kontrak MDF-API-1.0 §3 dan MDF-PERM-1.0 §2.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/medical-fee-management/master-data/sharing-agreements")]
    [AccessController(
        moduleCode: "MEDICAL_FEE_MANAGEMENT",
        moduleName: "Medical Fee Management",
        displayName: "Medical Fee Sharing Agreement",
        AreaName = "HealthServices",
        ControllerName = "MedicalFeeSharingAgreements",
        Description = "Mengelola kesepakatan tarif sharing dan baris aturan tarif berversi",
        SortOrder = 2
    )]
    [Tags("Health Services / Medical Fee Management / Master Data / Sharing Agreement")]
    public class MedicalFeeSharingAgreementsController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MedicalFeeManagement.MasterData.SharingAgreement";

        private readonly MedicalFeeSharingService _service;
        private readonly MedicalFeeAccessGuard _accessGuard;
        private readonly LoggerService _loggerService;

        public MedicalFeeSharingAgreementsController(
            MedicalFeeSharingService service,
            MedicalFeeAccessGuard accessGuard,
            LoggerService loggerService)
        {
            _service = service;
            _accessGuard = accessGuard;
            _loggerService = loggerService;
        }

        // =====================================================================
        // 1. GET /filters/metadata
        // =====================================================================

        /// <summary>
        /// Mengambil konfigurasi metadata filter, sort, dan pilihan enum untuk kesepakatan tarif.
        /// </summary>
        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<SharingAgreementFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Medical Fee Sharing Agreement", Description = "Melihat metadata filter kesepakatan tarif sharing", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicalFeeSharingAgreements", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var metadata = _service.GetFilterMetadata();
            return Ok(ApiResponse<SharingAgreementFilterMetadataResponse>.Ok(metadata, "Metadata filter kesepakatan tarif berhasil diambil."));
        }

        // =====================================================================
        // 2. GET /summary
        // =====================================================================

        /// <summary>
        /// Mengambil ringkasan statistik kesepakatan tarif sharing (total, aktif, draft, terminated, dsb).
        /// </summary>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<SharingAgreementSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Medical Fee Sharing Agreement", Description = "Melihat ringkasan statistik kesepakatan tarif", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicalFeeSharingAgreements", "Read")]
        public async Task<IActionResult> GetSummary(CancellationToken ct = default)
        {
            var summary = await _service.GetSummaryAsync(ct);
            return Ok(ApiResponse<SharingAgreementSummaryResponse>.Ok(summary, "Ringkasan kesepakatan tarif berhasil diambil."));
        }

        // =====================================================================
        // 3. GET / (List Paged Table)
        // =====================================================================

        /// <summary>
        /// Mengambil daftar kesepakatan tarif sharing terpaginasi dengan penyaring.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<SharingAgreementHeaderResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Medical Fee Sharing Agreement", Description = "Melihat daftar kesepakatan tarif sharing", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicalFeeSharingAgreements", "Read")]
        public async Task<IActionResult> GetPaged([FromQuery] SharingAgreementPagedQuery query, CancellationToken ct = default)
        {
            var actorContext = await _accessGuard.ResolveActorContextAsync(User, ct);
            var result = await _service.GetPagedAsync(query, actorContext, ct);
            return Ok(ApiResponse<PagedResult<SharingAgreementHeaderResponse>>.Ok(result, "Daftar kesepakatan tarif sharing berhasil diambil."));
        }

        // =====================================================================
        // 4. GET /options
        // =====================================================================

        /// <summary>
        /// Mengambil opsi kesepakatan tarif untuk dropdown.
        /// </summary>
        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<SharingAgreementOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Medical Fee Sharing Agreement", Description = "Melihat pilihan kesepakatan tarif", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicalFeeSharingAgreements", "Read")]
        public async Task<IActionResult> GetOptions(CancellationToken ct = default)
        {
            var options = await _service.GetOptionsAsync(ct);
            return Ok(ApiResponse<List<SharingAgreementOptionResponse>>.Ok(options, "Pilihan kesepakatan tarif berhasil diambil."));
        }

        // =====================================================================
        // 5. GET /{id} (Single Detail)
        // =====================================================================

        /// <summary>
        /// Mengambil detail satu kesepakatan tarif beserta seluruh baris aturan tarifnya.
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<SharingAgreementDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Medical Fee Sharing Agreement", Description = "Melihat detail kesepakatan tarif sharing", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicalFeeSharingAgreements", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
        {
            try
            {
                var actorContext = await _accessGuard.ResolveActorContextAsync(User, ct);
                var agreement = await _service.GetByIdAsync(id, actorContext, ct);
                if (agreement == null)
                {
                    return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Kesepakatan tarif sharing tidak ditemukan atau sudah dihapus."));
                }

                return Ok(ApiResponse<SharingAgreementDetailResponse>.Ok(agreement, "Detail kesepakatan tarif sharing berhasil diambil."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
        }

        // =====================================================================
        // 6. POST / (Create)
        // =====================================================================

        /// <summary>
        /// Menyusun kesepakatan tarif sharing baru (status awal Draft) beserta aturan tarif awalnya bila disertakan.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<SharingAgreementDetailResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Create", "Create Medical Fee Sharing Agreement", Description = "Menyusun kesepakatan tarif sharing baru", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("MedicalFeeSharingAgreements", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateSharingAgreementRequest request,
            CancellationToken ct = default)
        {
            var actorUserId = GetCurrentUserId();
            var result = await _service.CreateAsync(request, actorUserId, ct);

            if (!result.Success)
            {
                await _loggerService.WarningAsync(LogCategory, "MedicalFeeSharingAgreement.Create", result.Message, new { request, actorUserId });
                return ToActionResult(result);
            }

            await _loggerService.InfoAsync(LogCategory, "MedicalFeeSharingAgreement.Create", result.Message, new { result.Data?.Id, result.Data?.AgreementNumber, actorUserId });
            return ToActionResult(result);
        }

        // =====================================================================
        // 7. PUT /{id} (Update Header)
        // =====================================================================

        /// <summary>
        /// Mengubah kepala kesepakatan tarif sharing.
        /// </summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<SharingAgreementDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Update Medical Fee Sharing Agreement", Description = "Mengubah kesepakatan tarif sharing", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("MedicalFeeSharingAgreements", "Update")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateSharingAgreementRequest request,
            CancellationToken ct = default)
        {
            var actorUserId = GetCurrentUserId();
            var result = await _service.UpdateAsync(id, request, actorUserId, ct);

            if (!result.Success)
            {
                await _loggerService.WarningAsync(LogCategory, "MedicalFeeSharingAgreement.Update", result.Message, new { id, request, actorUserId });
                return ToActionResult(result);
            }

            await _loggerService.InfoAsync(LogCategory, "MedicalFeeSharingAgreement.Update", result.Message, new { id, result.Data?.AgreementNumber, actorUserId });
            return ToActionResult(result);
        }

        // =====================================================================
        // 8. POST /{id}/activation (Activate)
        // =====================================================================

        /// <summary>
        /// Mengaktifkan kesepakatan tarif sharing (memvalidasi status kontrak HR dan kelengkapan aturan tarif).
        /// </summary>
        [HttpPost("{id:guid}/activation")]
        [ProducesResponseType(typeof(ApiResponse<SharingAgreementDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Update Medical Fee Sharing Agreement", Description = "Mengaktifkan kesepakatan tarif sharing", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("MedicalFeeSharingAgreements", "Update")]
        public async Task<IActionResult> Activate(
            Guid id,
            [FromBody] ActivateSharingAgreementRequest request,
            CancellationToken ct = default)
        {
            var actorUserId = GetCurrentUserId();
            var result = await _service.ActivateAsync(id, request, actorUserId, ct);

            if (!result.Success)
            {
                await _loggerService.WarningAsync(LogCategory, "MedicalFeeSharingAgreement.Activate", result.Message, new { id, actorUserId });
                return ToActionResult(result);
            }

            await _loggerService.InfoAsync(LogCategory, "MedicalFeeSharingAgreement.Activate", result.Message, new { id, result.Data?.AgreementNumber, actorUserId });
            return ToActionResult(result);
        }

        // =====================================================================
        // 9. POST /{id}/termination (Terminate)
        // =====================================================================

        /// <summary>
        /// Menghentikan kesepakatan tarif sharing dengan menyertakan alasan.
        /// </summary>
        [HttpPost("{id:guid}/termination")]
        [ProducesResponseType(typeof(ApiResponse<SharingAgreementDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Update Medical Fee Sharing Agreement", Description = "Menghentikan kesepakatan tarif sharing", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("MedicalFeeSharingAgreements", "Update")]
        public async Task<IActionResult> Terminate(
            Guid id,
            [FromBody] TerminateSharingAgreementRequest request,
            CancellationToken ct = default)
        {
            var actorUserId = GetCurrentUserId();
            var result = await _service.TerminateAsync(id, request, actorUserId, ct);

            if (!result.Success)
            {
                await _loggerService.WarningAsync(LogCategory, "MedicalFeeSharingAgreement.Terminate", result.Message, new { id, request, actorUserId });
                return ToActionResult(result);
            }

            await _loggerService.InfoAsync(LogCategory, "MedicalFeeSharingAgreement.Terminate", result.Message, new { id, result.Data?.AgreementNumber, actorUserId });
            return ToActionResult(result);
        }

        // =====================================================================
        // 10. GET /{id}/rules (Get Rules)
        // =====================================================================

        /// <summary>
        /// Mengambil baris aturan tarif dari kesepakatan (opsional filter tanggal berlakunya).
        /// </summary>
        [HttpGet("{id:guid}/rules")]
        [ProducesResponseType(typeof(ApiResponse<List<SharingRuleResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Medical Fee Sharing Agreement", Description = "Melihat baris aturan tarif sharing", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicalFeeSharingAgreements", "Read")]
        public async Task<IActionResult> GetRules(
            Guid id,
            [FromQuery] DateOnly? effectiveOn = null,
            CancellationToken ct = default)
        {
            var rules = await _service.GetRulesAsync(id, effectiveOn, ct);
            return Ok(ApiResponse<List<SharingRuleResponse>>.Ok(rules, "Daftar baris aturan tarif berhasil diambil."));
        }

        // =====================================================================
        // 11. POST /{id}/rules (Add Rule)
        // =====================================================================

        /// <summary>
        /// Menambahkan satu baris aturan tarif baru ke dalam kesepakatan.
        /// </summary>
        [HttpPost("{id:guid}/rules")]
        [ProducesResponseType(typeof(ApiResponse<SharingRuleResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Update Medical Fee Sharing Agreement", Description = "Menambah baris aturan tarif sharing", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("MedicalFeeSharingAgreements", "Update")]
        public async Task<IActionResult> AddRule(
            Guid id,
            [FromBody] AddSharingRuleRequest request,
            CancellationToken ct = default)
        {
            var actorUserId = GetCurrentUserId();
            var result = await _service.AddRuleAsync(id, request, actorUserId, ct);

            if (!result.Success)
            {
                await _loggerService.WarningAsync(LogCategory, "MedicalFeeSharingAgreement.AddRule", result.Message, new { id, request, actorUserId });
                return ToActionResult(result);
            }

            await _loggerService.InfoAsync(LogCategory, "MedicalFeeSharingAgreement.AddRule", result.Message, new { id, result.Data?.Id, actorUserId });
            return ToActionResult(result);
        }

        // =====================================================================
        // 12. POST /{id}/rules/{ruleId}/supersede (Supersede Rule)
        // =====================================================================

        /// <summary>
        /// Mengganti baris aturan tarif dengan versi baru — MDF-DES-009. Menutup baris lama dan membentuk baris baru.
        /// </summary>
        [HttpPost("{id:guid}/rules/{ruleId:guid}/supersede")]
        [ProducesResponseType(typeof(ApiResponse<SharingRuleResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Update Medical Fee Sharing Agreement", Description = "Mengganti baris aturan tarif dengan versi baru", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("MedicalFeeSharingAgreements", "Update")]
        public async Task<IActionResult> SupersedeRule(
            Guid id,
            Guid ruleId,
            [FromBody] SupersedeSharingRuleRequest request,
            CancellationToken ct = default)
        {
            var actorUserId = GetCurrentUserId();
            var result = await _service.SupersedeRuleAsync(id, ruleId, request, actorUserId, ct);

            if (!result.Success)
            {
                await _loggerService.WarningAsync(LogCategory, "MedicalFeeSharingAgreement.SupersedeRule", result.Message, new { id, ruleId, request, actorUserId });
                return ToActionResult(result);
            }

            await _loggerService.InfoAsync(LogCategory, "MedicalFeeSharingAgreement.SupersedeRule", result.Message, new { id, ruleId, NewRuleId = result.Data?.Id, actorUserId });
            return ToActionResult(result);
        }

        // =====================================================================
        // 13. DELETE /{id} (Soft Delete Draft)
        // =====================================================================

        /// <summary>
        /// Menghapus kesepakatan tarif secara soft delete (hanya diperbolehkan untuk status Draft).
        /// </summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Delete", "Delete Medical Fee Sharing Agreement", Description = "Menghapus kesepakatan tarif sharing", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("MedicalFeeSharingAgreements", "Delete")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
        {
            var actorUserId = GetCurrentUserId();
            var result = await _service.DeleteAsync(id, actorUserId, ct);

            if (!result.Success)
            {
                await _loggerService.WarningAsync(LogCategory, "MedicalFeeSharingAgreement.Delete", result.Message, new { id, actorUserId });
                return ToActionResult(result);
            }

            await _loggerService.InfoAsync(LogCategory, "MedicalFeeSharingAgreement.Delete", result.Message, new { id, actorUserId });
            return ToActionResult(result);
        }

        // =====================================================================
        // Internal Helpers
        // =====================================================================

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
