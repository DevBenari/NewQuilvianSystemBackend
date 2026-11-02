using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using MedicalRecordCompletenessPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs.MedicalRecordCompletenessListItemResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Controllers
{
    /// <remarks>
    /// Daftar kerja kelengkapan berkas untuk petugas rekam medis (`BE-39`, kontrak API 0.2.0 A2).
    /// Berkas yang belum lengkap hanya masuk daftar ini; penutupan, pemulangan, dan tagihan tidak
    /// pernah tertahan (`RM-DEC-046`).
    ///
    /// Keadaan tidak dapat diubah pengguna. Satu-satunya tindakan adalah menilai ulang.
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/medical-record-management/medical-record-completeness")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MEDICAL_RECORD",
        moduleName: "Health Service Medical Record",
        displayName: "Medical Record Completeness",
        AreaName = "HealthServices",
        ControllerName = "MedicalRecordCompleteness",
        Description = "Daftar kerja kelengkapan berkas rekam medis per kunjungan",
        SortOrder = 7
    )]
    [Tags("Health Services / Medical Record Management / Medical Record Completeness")]
    public class MedicalRecordCompletenessController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MedicalRecord";

        private readonly MedicalRecordCompletenessService _completenessService;
        private readonly LoggerService _loggerService;

        public MedicalRecordCompletenessController(
            MedicalRecordCompletenessService completenessService,
            LoggerService loggerService)
        {
            _completenessService = completenessService;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<MedicalRecordCompletenessFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Medical Record Completeness", Description = "Melihat pilihan penyaring daftar kerja kelengkapan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicalRecordCompleteness", "Read")]
        public IActionResult GetFilterMetadata()
        {
            return Ok(ApiResponse<MedicalRecordCompletenessFilterMetadataResponse>.Ok(
                MedicalRecordCompletenessService.BuildFilterMetadata(),
                "Pilihan penyaring daftar kerja kelengkapan berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<MedicalRecordCompletenessSummaryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Read", "Read Medical Record Completeness", Description = "Melihat jumlah kunjungan per keadaan kelengkapan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicalRecordCompleteness", "Read")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] MedicalRecordCompletenessPagedQuery query,
            CancellationToken cancellationToken = default)
        {
            var galat = MedicalRecordCompletenessService.ValidateQuery(query);
            if (galat != null)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, galat));

            return Ok(ApiResponse<MedicalRecordCompletenessSummaryResponse>.Ok(
                await _completenessService.GetSummaryAsync(query, cancellationToken),
                "Ringkasan kelengkapan berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<MedicalRecordCompletenessPagedResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Read", "Read Medical Record Completeness", Description = "Melihat daftar kerja kelengkapan berkas", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicalRecordCompleteness", "Read")]
        public async Task<IActionResult> GetAll(
            [FromQuery] MedicalRecordCompletenessPagedQuery query,
            CancellationToken cancellationToken = default)
        {
            var galat = MedicalRecordCompletenessService.ValidateQuery(query);
            if (galat != null)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, galat));

            return Ok(ApiResponse<MedicalRecordCompletenessPagedResult>.Ok(
                await _completenessService.GetPagedAsync(query, cancellationToken),
                "Daftar kerja kelengkapan berhasil diambil."));
        }

        [HttpGet("encounters/{encounterId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<MedicalRecordCompletenessDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Medical Record Completeness", Description = "Melihat penilaian kelengkapan satu kunjungan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicalRecordCompleteness", "Read")]
        public async Task<IActionResult> GetByEncounter(
            Guid encounterId,
            CancellationToken cancellationToken = default)
        {
            var detail = await _completenessService.GetDetailAsync(encounterId, cancellationToken);

            if (detail == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    MedicalRecordCompletenessService.NotEvaluatedMessage));
            }

            return Ok(ApiResponse<MedicalRecordCompletenessDetailResponse>.Ok(
                detail, "Penilaian kelengkapan berhasil diambil."));
        }

        [HttpPost("encounters/{encounterId:guid}/evaluate")]
        [ProducesResponseType(typeof(ApiResponse<MedicalRecordCompletenessDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Evaluate", "Evaluate Medical Record Completeness", Description = "Menilai ulang kelengkapan berkas satu kunjungan", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("MedicalRecordCompleteness", "Evaluate")]
        public async Task<IActionResult> Evaluate(
            Guid encounterId,
            CancellationToken cancellationToken = default)
        {
            var hasil = await _completenessService.EvaluateAsync(encounterId, GetCurrentUserId(), cancellationToken);

            switch (hasil.Status)
            {
                case MedicalRecordCompletenessStatus.NotFound:
                    return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, hasil.Message));
                case MedicalRecordCompletenessStatus.Running:
                case MedicalRecordCompletenessStatus.Cancelled:
                case MedicalRecordCompletenessStatus.Conflict:
                    return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, hasil.Message));
            }

            var detail = hasil.Detail!;

            // AC7: hanya keadaan dan jumlah. Rincian kekurangan memuat id penulis dan tidak pernah
            // masuk log, baik di teks pesan maupun di payload.
            await _loggerService.AuditAsync(
                LogCategory,
                "MedicalRecordCompleteness.Evaluate",
                $"Menilai ulang kelengkapan kunjungan {detail.EncounterNumber}: {detail.State}, " +
                $"{detail.Deficiencies.Count} kekurangan, {(detail.IsPersisted ? "disimpan" : "tidak disimpan")}.");

            return Ok(ApiResponse<MedicalRecordCompletenessDetailResponse>.Ok(detail, hasil.Message));
        }

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}
