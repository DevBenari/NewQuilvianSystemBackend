using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using QuilvianSystemBackend.Services.Security;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Services;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Shared.HumanResource.Helpers;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Controllers
{
    /// <summary>
    /// REST API Controller untuk Sidang Kalibrasi Manajemen & Resolusi Sengketa Nilai (Kontrak v21 - Tag 27.3)
    /// </summary>
    [ApiController]
    [Route("api/v1/corporate/hrd/performance")]
    [Authorize]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_PERFORMANCE",
        moduleName: "Human Resource Performance",
        displayName: "Kalibrasi & Sengketa Nilai Kinerja",
        AreaName = "Corporate",
        ControllerName = "PerformanceCalibration",
        Description = "Kurva distribusi, sidang kalibrasi, override skor, dan adjudikasi sanggahan",
        SortOrder = 15)]
    [Tags("Corporate - HRD Performance Calibration & Dispute Resolution")]
    public class PerformanceCalibrationController : ControllerBase
    {
        private readonly IPerformanceCalibrationService _calibrationService;
        private readonly IPerformanceDisputeService _disputeService;

        public PerformanceCalibrationController(
            IPerformanceCalibrationService calibrationService,
            IPerformanceDisputeService disputeService)
        {
            _calibrationService = calibrationService ?? throw new ArgumentNullException(nameof(calibrationService));
            _disputeService = disputeService ?? throw new ArgumentNullException(nameof(disputeService));
        }

        #region CALIBRATION
        /// <summary>
        /// Menampilkan sebaran kurva nilai aktual RS terhadap pedoman distribusi fleksibel (10-15% A, 70-80% B, 5-10% C/D) (PRF-B6)
        /// </summary>
        [HttpGet("calibration/distribution-matrix")]
        [AccessAction("ViewCalibration", "Melihat Kurva Kalibrasi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PerformanceCalibration", "ViewCalibration")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<CalibrationDistributionMatrixDto>), 200)]
        public async Task<IActionResult> GetDistributionMatrix(
            [FromQuery] Guid cycleId,
            CancellationToken cancellationToken = default)
        {
            var result = await _calibrationService.GetDistributionMatrixAsync(cycleId, cancellationToken);
            return Ok(ApiResponse<CalibrationDistributionMatrixDto>.Ok(result, "Matriks distribusi kurva nilai kalibrasi berhasil diambil."));
        }

        /// <summary>
        /// Menjadwalkan dan membuka sidang pleno kalibrasi manajemen rumah sakit
        /// </summary>
        [HttpPost("calibration/sessions")]
        [AccessAction("ManageCalibration", "Mengelola Sidang Kalibrasi", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("PerformanceCalibration", "ManageCalibration")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<CalibrationSessionDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        public async Task<IActionResult> CreateCalibrationSession(
            [FromBody] CreateCalibrationSessionRequestDto dto,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _calibrationService.CreateSessionAsync(dto, Guid.Empty, cancellationToken);
                return StatusCode(201, ApiResponse<CalibrationSessionDto>.Ok(result, "Sidang pleno kalibrasi berhasil dijadwalkan."));
            }
            catch (Exception ex) when (HrSafeError.IsDomain(ex))
            {
                return BadRequest(ApiResponse<string>.Fail(400, ex.Message));
            }
        }

        /// <summary>
        /// Penyesuaian skor kinerja pegawai oleh Komite Kalibrasi dengan kewajiban nota pertimbangan tertulis min 50 karakter (VR-PRF-05, VR-PRF-12)
        /// </summary>
        [HttpPost("calibration/override-score")]
        [AccessAction("ManageCalibration", "Mengelola Sidang Kalibrasi", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("PerformanceCalibration", "ManageCalibration")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        public async Task<IActionResult> OverrideScore(
            [FromBody] OverrideCalibrationScoreRequestDto dto,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _calibrationService.OverrideScoreAsync(dto, Guid.Empty, cancellationToken);
                return Ok(ApiResponse<bool>.Ok(result, "Penyesuaian skor hasil kalibrasi berhasil disimpan dengan nota justifikasi komite."));
            }
            catch (Exception ex) when (HrSafeError.IsDomain(ex))
            {
                return BadRequest(ApiResponse<string>.Fail(400, ex.Message));
            }
        }

        /// <summary>
        /// Pengesahan final hasil kalibrasi nilai RS dan instruksi rilis massal ke portal ESS pegawai
        /// </summary>
        [HttpPost("calibration/sessions/{id}/finalize")]
        [AccessAction("ManageCalibration", "Mengelola Sidang Kalibrasi", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("PerformanceCalibration", "ManageCalibration")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        public async Task<IActionResult> FinalizeSession(
            [FromRoute] Guid id,
            [FromBody] FinalizeCalibrationSessionRequestDto dto,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _calibrationService.FinalizeSessionAsync(id, dto, Guid.Empty, cancellationToken);
                return Ok(ApiResponse<bool>.Ok(result, "Sidang pleno kalibrasi disahkan; nilai resmi dirilis serentak ke portal ESS."));
            }
            catch (Exception ex) when (HrSafeError.IsDomain(ex))
            {
                return BadRequest(ApiResponse<string>.Fail(400, ex.Message));
            }
        }
        #endregion

        #region DISPUTES
        /// <summary>
        /// Pengajuan sanggah banding nilai kinerja resmi oleh pegawai (maksimal H+7 pasca rilis nilai, VR-PRF-12, HRD-DEC-169)
        /// </summary>
        [HttpPost("disputes/submit")]
        [AccessAction("SubmitDispute", "Mengajukan Sanggahan Nilai", AccessType = AccessTypes.Create, SortOrder = 3)]
        [AccessPermission("PerformanceCalibration", "SubmitDispute")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<AppraisalDisputeDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        public async Task<IActionResult> SubmitDispute(
            [FromQuery] Guid? staffId = null,
            [FromBody] SubmitAppraisalDisputeRequestDto dto = null!,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var sId = staffId ?? Guid.Empty;
                var result = await _disputeService.SubmitDisputeAsync(dto, sId, cancellationToken);
                return StatusCode(201, ApiResponse<AppraisalDisputeDto>.Ok(result, "Permohonan sanggah banding nilai resmi berhasil diregistrasi."));
            }
            catch (Exception ex) when (HrSafeError.IsDomain(ex))
            {
                return BadRequest(ApiResponse<string>.Fail(400, ex.Message));
            }
        }

        /// <summary>
        /// Menerbitkan putusan final majelis banding Direktur SDM (maksimal H+14 kalender, VR-PRF-13)
        /// </summary>
        [HttpPost("disputes/{id}/adjudicate")]
        [AccessAction("ManageDispute", "Mengadjudikasi Sanggahan Nilai", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("PerformanceCalibration", "ManageDispute")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<AppraisalDisputeDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        public async Task<IActionResult> AdjudicateDispute(
            [FromRoute] Guid id,
            [FromQuery] Guid? directorId = null,
            [FromBody] AdjudicateDisputeRequestDto dto = null!,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var dId = directorId ?? Guid.Empty;
                var result = await _disputeService.AdjudicateDisputeAsync(id, dto, dId, cancellationToken);
                return Ok(ApiResponse<AppraisalDisputeDto>.Ok(result, "Putusan majelis sanggah banding Direktur SDM berhasil disahkan."));
            }
            catch (Exception ex) when (HrSafeError.IsDomain(ex))
            {
                return BadRequest(ApiResponse<string>.Fail(400, ex.Message));
            }
        }
        #endregion

        /// <summary>
        /// Daftar sanggahan nilai kinerja seluruh pegawai. Filter: search, status, pageNumber, pageSize (MSL-14, DEC-FER-05 opsi A, 18 Nov 2026).
        /// </summary>
        [HttpGet("disputes")]
        [AccessAction("ManageDispute", "Melihat Daftar Sanggahan Seluruh Pegawai", AccessType = AccessTypes.Read, SortOrder = 20)]
        [AccessPermission("PerformanceCalibration", "ManageDispute")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<AppraisalDisputeListItemDto>>), 200)]
        public async Task<IActionResult> GetDisputeList(
            [FromQuery] PerformanceListQuery query,
            [FromServices] IPerformanceListQueryService listService,
            CancellationToken cancellationToken = default)
        {
            var result = await listService.GetDisputesAsync(query, cancellationToken);
            return Ok(ApiResponse<PagedResult<AppraisalDisputeListItemDto>>.Ok(result, "Daftar berhasil diambil."));
        }

        /// <summary>
        /// Unggah bukti pendukung sanggahan nilai (MSL-15, DEC-FER-02 opsi A, 18 Nov 2026). Multipart: <c>file</c>; query <c>category</c>
        /// ("performance-dispute-evidence"). Jawaban berisi <c>key</c> yang disimpan di kolom lampiran fitur. PDF/JPG/PNG/WEBP/Word/Excel, maks 10 MB.
        /// </summary>
        [HttpPost("disputes/evidence")]
        [AccessAction("SubmitDispute", "Mengajukan Sanggahan Nilai", AccessType = AccessTypes.Create, SortOrder = 30)]
        [AccessPermission("PerformanceCalibration", "SubmitDispute")]
        [Tags("Corporate - HRD Performance Calibration & Dispute Resolution")]
        [RequestSizeLimit(HrAttachmentStorage.DefaultMaximumFileSizeBytes + 1024 * 1024)]
        [ProducesResponseType(typeof(ApiResponse<HrStoredAttachment>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadAttachment(
            [FromForm] IFormFile file,
            [FromQuery] string? category,
            [FromServices] HrAttachmentStorage storage,
            CancellationToken cancellationToken = default)
        {
            var resolvedCategory = string.IsNullOrWhiteSpace(category) ? "performance-dispute-evidence" : category.Trim();
            if (!new[] { "performance-dispute-evidence" }.Contains(resolvedCategory))
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Jenis lampiran tidak dikenal untuk fitur ini."));
            }

            try
            {
                var stored = await storage.SaveAsync(resolvedCategory, User.GetUserId(), file, cancellationToken);
                return StatusCode(StatusCodes.Status201Created, ApiResponse<HrStoredAttachment>.Ok(stored, "Lampiran berhasil diunggah."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
        }

        /// <summary>
        /// Unduh bukti pendukung sanggahan nilai (MSL-15). Boleh untuk pengunggah, atau pemegang hak ManageDispute pada fitur ini.
        /// Lampiran milik orang lain tanpa hak dijawab 404 agar keberadaannya tidak bocor.
        /// </summary>
        [HttpGet("disputes/evidence/download")]
        [AccessAction("SubmitDispute", "Mengajukan Sanggahan Nilai", AccessType = AccessTypes.Create, SortOrder = 31)]
        [AccessPermission("PerformanceCalibration", "SubmitDispute")]
        [Tags("Corporate - HRD Performance Calibration & Dispute Resolution")]
        [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DownloadAttachment(
            [FromQuery] string key,
            [FromServices] HrAttachmentStorage storage,
            [FromServices] AccessPermissionService accessPermissionService)
        {
            var category = HrAttachmentStorage.CategoryOf(key);
            if (category == null || !new[] { "performance-dispute-evidence" }.Contains(category))
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Lampiran tidak ditemukan."));
            }

            try
            {
                var attachment = storage.Open(key, category);
                var allowed = attachment.OwnerUserId == User.GetUserId() || await accessPermissionService.HasAccessAsync(User, "PerformanceCalibration", "ManageDispute");
                if (!allowed)
                {
                    return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Lampiran tidak ditemukan."));
                }

                return PhysicalFile(attachment.PhysicalPath, attachment.ContentType, attachment.DownloadName);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
        }
    }
}
