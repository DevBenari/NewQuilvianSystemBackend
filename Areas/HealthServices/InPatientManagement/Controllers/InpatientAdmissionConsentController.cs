using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Controllers
{
    /// <summary>
    /// Formulir Persetujuan Rawat Inap & Tanda Tangan Digital (BE-RWI-205, RWI-DEC-272, RWI-DEC-273).
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/inpatient-admissions/{episodeId:guid}/consent")]
    [Route("api/v1/health-services/inpatient-management/episodes/{episodeId:guid}/admission-consents")]
    [Route("api/v1/health-services/inpatient-management/episodes/{episodeId:guid}/consent")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_INPATIENT",
        moduleName: "Health Service Inpatient",
        displayName: "Inpatient Admission Consent",
        AreaName = "HealthServices",
        ControllerName = "InpatientAdmissionConsent",
        Description = "Formulir persetujuan rawat inap dan tanda tangan digital pasien / penanggung jawab",
        SortOrder = 20
    )]
    [Tags("InpatientAdmissionConsent")]
    public class InpatientAdmissionConsentController : ControllerBase
    {
        private readonly InpatientAdmissionConsentService _consentService;

        public InpatientAdmissionConsentController(InpatientAdmissionConsentService consentService)
        {
            _consentService = consentService;
        }

        /// <summary>
        /// Menyimpan formulir persetujuan rawat inap dan tanda tangan digital base64 ke episode rawat inap.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<InpatientAdmissionConsentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<InpatientAdmissionConsentResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Create", "Save Inpatient Admission Consent", Description = "Menyimpan form persetujuan dan TTD digital admisi", AccessType = AccessTypes.Create, SortOrder = 1)]
        [AccessPermission("InpatientEpisode", "Create")]
        public async Task<IActionResult> SaveConsent(
            [FromRoute] Guid episodeId,
            [FromBody] InpatientAdmissionConsentCreateDto request,
            CancellationToken cancellationToken = default)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            Guid? actorUserId = Guid.TryParse(userIdStr, out var parsedGuid) ? parsedGuid : null;

            var result = await _consentService.SaveConsentAsync(episodeId, request, actorUserId, cancellationToken);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, ApiResponse<object>.Fail(
                    result.StatusCode,
                    result.Message,
                    result.Code != null ? new { code = result.Code } : null));
            }

            return StatusCode(StatusCodes.Status201Created,
                ApiResponse<InpatientAdmissionConsentResponse>.Ok(result.Data, result.Message));
        }

        /// <summary>
        /// Mengambil data formulir persetujuan rawat inap dan tanda tangan digital yang telah tersimpan.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<InpatientAdmissionConsentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Inpatient Admission Consent", Description = "Membaca form persetujuan dan TTD digital admisi", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("InpatientEpisode", "Read")]
        public async Task<IActionResult> GetConsent(
            [FromRoute] Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            var result = await _consentService.GetConsentAsync(episodeId, cancellationToken);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, ApiResponse<object>.Fail(
                    result.StatusCode, result.Message));
            }

            return Ok(ApiResponse<InpatientAdmissionConsentResponse>.Ok(result.Data, result.Message));
        }
    }
}
