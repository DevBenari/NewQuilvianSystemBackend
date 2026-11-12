using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/overtime/verifications")]
    [Route("api/overtime/verifications")]
    [Tags("HR Overtime - Biometric Verification & Discrepancies")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_OVERTIME",
        moduleName: "Human Resource Overtime",
        displayName: "Overtime Biometric Verification & Discrepancies",
        AreaName = "Corporate",
        ControllerName = "OvertimeVerificationModern",
        Description = "Verifikasi presensi biometrik, penegakan Min-Duration Rule, dan investigasi logbook fisik HR untuk anomali selisih waktu",
        SortOrder = 4)]
    public class OvertimeVerificationModernController : ControllerBase
    {
        private readonly IOvertimeBiometricVerificationService _biometricVerificationService;
        private readonly IOvertimeDiscrepancyService _discrepancyService;

        public OvertimeVerificationModernController(
            IOvertimeBiometricVerificationService biometricVerificationService,
            IOvertimeDiscrepancyService discrepancyService)
        {
            _biometricVerificationService = biometricVerificationService ?? throw new ArgumentNullException(nameof(biometricVerificationService));
            _discrepancyService = discrepancyService ?? throw new ArgumentNullException(nameof(discrepancyService));
        }

        [HttpPost("{realizationId:guid}/verify-biometric")]
        [AccessAction("Verify", "Menjalankan Verifikasi Biometrik", Description = "Verifikasi silang kehadiran log mesin biometrik presensi dengan aturan min-duration dan pembulatan 30 menit", AccessType = AccessTypes.Update, SortOrder = 1)]
        [AccessPermission("OvertimeVerificationModern", "Verify")]
        [ProducesResponseType(typeof(ApiResponse<OvertimeBiometricVerificationResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> VerifyBiometric(
            Guid realizationId,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            try
            {
                var result = await _biometricVerificationService.VerifyBiometricAsync(realizationId, actorUserId, cancellationToken);
                return Ok(ApiResponse<OvertimeBiometricVerificationResultDto>.Ok(result, result.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
        }

        [HttpGet("discrepancies")]
        [AccessAction("View", "Melihat Antrean Anomali Presensi", Description = "Melihat antrean lembur yang terdeteksi memiliki selisih waktu > 30 menit atau lupa tap", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("OvertimeVerificationModern", "View")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<OvertimeDiscrepancyQueueItemDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDiscrepancyQueue(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var result = await _discrepancyService.GetDiscrepancyQueueAsync(page, pageSize, cancellationToken);
            return Ok(ApiResponse<PagedResult<OvertimeDiscrepancyQueueItemDto>>.Ok(result, "Antrean anomali discrepancy presensi berhasil diambil."));
        }

        [HttpPost("discrepancies/{verificationId:guid}/resolve")]
        [AccessAction("Verify", "Menyelesaikan Anomali Presensi dengan Logbook", Description = "Menyelesaikan anomali selisih waktu menggunakan bukti fisik logbook ruangan atau surat pengantar", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("OvertimeVerificationModern", "Verify")]
        [ProducesResponseType(typeof(ApiResponse<ResolveOvertimeDiscrepancyResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ResolveDiscrepancy(
            Guid verificationId,
            [FromBody] ResolveOvertimeDiscrepancyCommand command,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var result = await _discrepancyService.ResolveDiscrepancyAsync(verificationId, command, actorUserId, cancellationToken);

            return StatusCode(
                result.StatusCode,
                result.Success
                    ? ApiResponse<ResolveOvertimeDiscrepancyResultDto>.Ok(result.Data!, result.Message)
                    : ApiResponse<ResolveOvertimeDiscrepancyResultDto>.Fail(result.StatusCode, result.Message));
        }

        private Guid GetCurrentUserId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(raw, out var parsed) ? parsed : Guid.Empty;
        }
    }
}
