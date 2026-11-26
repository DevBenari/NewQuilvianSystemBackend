using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using QuilvianSystemBackend.Services.Security;
using System.Collections.Generic;
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
    /// REST API Controller untuk Evaluasi Kinerja Pegawai & Penilaian Berjenjang Tiga Tingkat (Kontrak v21 - Tag 27.2)
    /// </summary>
    [ApiController]
    [Route("api/v1/corporate/hrd/performance/appraisals")]
    [Authorize]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_PERFORMANCE",
        moduleName: "Human Resource Performance",
        displayName: "Penilaian Kinerja Pegawai",
        AreaName = "Corporate",
        ControllerName = "PerformanceAppraisal",
        Description = "Evaluasi mandiri, penilaian atasan, review lini kedua, dan konfirmasi hasil",
        SortOrder = 14)]
    [Tags("Corporate - HRD Performance Appraisals & Multi-Level Review")]
    public class PerformanceAppraisalsController : ControllerBase
    {
        private readonly IPerformanceAppraisalService _appraisalService;

        public PerformanceAppraisalsController(IPerformanceAppraisalService appraisalService)
        {
            _appraisalService = appraisalService ?? throw new ArgumentNullException(nameof(appraisalService));
        }

        private const string ProfileNotLinkedMessage = "Akun Anda belum terhubung dengan data pegawai, sehingga penilaian kinerja belum dapat dibuka. Hubungi bagian SDM.";

        // MSL-02 (18 Nov 2026): identitas pegawai/atasan selalu dari klaim token workforce_profile_id, tidak pernah dari query string.
        private Guid? CurrentWorkforceProfileId() => User.GetWorkforceProfileId();

        private IActionResult ProfileNotLinked() =>
            StatusCode(StatusCodes.Status403Forbidden, ApiResponse<string>.Fail(StatusCodes.Status403Forbidden, ProfileNotLinkedMessage));

        private IActionResult MapError(Exception ex) => ex switch
        {
            KeyNotFoundException => NotFound(ApiResponse<string>.Fail(StatusCodes.Status404NotFound, ex.Message)),
            UnauthorizedAccessException => StatusCode(StatusCodes.Status403Forbidden, ApiResponse<string>.Fail(StatusCodes.Status403Forbidden, ex.Message)),
            _ => BadRequest(ApiResponse<string>.Fail(StatusCodes.Status400BadRequest, ex.Message))
        };

        /// <summary>
        /// Menampilkan lembar kerja evaluasi kinerja mandiri pegawai yang sedang login (ESS)
        /// </summary>
        [HttpGet("my-appraisal")]
        [AccessAction("ViewOwn", "Melihat Penilaian Kinerja Sendiri", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PerformanceAppraisal", "ViewOwn")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<PerformanceAppraisalDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 403)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        public async Task<IActionResult> GetMyAppraisal(
            [FromQuery] Guid? cycleId = null,
            CancellationToken cancellationToken = default)
        {
            var staffId = CurrentWorkforceProfileId();
            if (staffId == null) return ProfileNotLinked();

            try
            {
                var result = await _appraisalService.GetMyAppraisalAsync(staffId.Value, cycleId, cancellationToken);
                return Ok(ApiResponse<PerformanceAppraisalDto>.Ok(result, "Lembar kerja evaluasi mandiri berhasil diambil."));
            }
            catch (KeyNotFoundException ex)
            {
                return MapError(ex);
            }
        }

        /// <summary>
        /// Menyerahkan lembar refleksi diri evaluasi mandiri beserta unggahan dokumen bukti capaian (VR-PRF-22)
        /// </summary>
        [HttpPost("self-submit")]
        [AccessAction("SelfAssess", "Mengisi Evaluasi Mandiri", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("PerformanceAppraisal", "SelfAssess")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<PerformanceAppraisalDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        public async Task<IActionResult> SubmitSelfAppraisal(
            [FromBody] SubmitSelfAppraisalRequestDto dto = null!,
            CancellationToken cancellationToken = default)
        {
            var staffId = CurrentWorkforceProfileId();
            if (staffId == null) return ProfileNotLinked();

            try
            {
                var result = await _appraisalService.SubmitSelfAppraisalAsync(staffId.Value, dto, cancellationToken);
                return Ok(ApiResponse<PerformanceAppraisalDto>.Ok(result, "Evaluasi mandiri pegawai berhasil diserahkan ke atasan langsung."));
            }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or KeyNotFoundException or UnauthorizedAccessException)
            {
                return MapError(ex);
            }
        }

        /// <summary>
        /// Menampilkan daftar lembar evaluasi staf bawahan yang menunggu penilaian atasan langsung (Karu)
        /// </summary>
        [HttpGet("subordinates")]
        [AccessAction("ViewSubordinates", "Melihat Penilaian Kinerja Bawahan", AccessType = AccessTypes.Read, SortOrder = 3)]
        [AccessPermission("PerformanceAppraisal", "ViewSubordinates")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<List<PerformanceAppraisalDto>>), 200)]
        public async Task<IActionResult> GetSubordinateAppraisals(
            [FromQuery] Guid? cycleId = null,
            CancellationToken cancellationToken = default)
        {
            var managerId = CurrentWorkforceProfileId();
            if (managerId == null) return ProfileNotLinked();

            var result = await _appraisalService.GetSubordinateAppraisalsAsync(managerId.Value, cycleId, cancellationToken);
            return Ok(ApiResponse<List<PerformanceAppraisalDto>>.Ok(result, "Daftar lembar evaluasi staf bawahan berhasil diambil."));
        }

        /// <summary>
        /// Menginput skor resmi 60% KPI hasil kerja dan 40% perilaku Core Values BerAKHLAK oleh Karu (VR-PRF-03, VR-PRF-04, PRF-B4)
        /// </summary>
        [HttpPost("{id}/manager-assess")]
        [AccessAction("ManagerAssess", "Menilai Kinerja Bawahan", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("PerformanceAppraisal", "ManagerAssess")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<PerformanceAppraisalDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        public async Task<IActionResult> SubmitManagerAppraisal(
            [FromRoute] Guid id,
            [FromBody] SubmitManagerAppraisalRequestDto dto = null!,
            CancellationToken cancellationToken = default)
        {
            var managerId = CurrentWorkforceProfileId();
            if (managerId == null) return ProfileNotLinked();

            try
            {
                var result = await _appraisalService.SubmitManagerAppraisalAsync(id, managerId.Value, dto, cancellationToken);
                return Ok(ApiResponse<PerformanceAppraisalDto>.Ok(result, "Penilaian kinerja atasan berhasil disimpan dengan formula 60:40."));
            }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or KeyNotFoundException or UnauthorizedAccessException)
            {
                return MapError(ex);
            }
        }

        /// <summary>
        /// Penelaahan dan otorisasi berkas evaluasi oleh atasan kedua (Kabid) sebelum kalibrasi (VR-PRF-04, PRF-B3)
        /// </summary>
        [HttpPost("{id}/second-line-review")]
        [AccessAction("SecondLineReview", "Mereview Penilaian (Lini Kedua)", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("PerformanceAppraisal", "SecondLineReview")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<PerformanceAppraisalDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        public async Task<IActionResult> ProcessSecondLineReview(
            [FromRoute] Guid id,
            [FromBody] ProcessSecondLineReviewDto dto = null!,
            CancellationToken cancellationToken = default)
        {
            var reviewerId = CurrentWorkforceProfileId();
            if (reviewerId == null) return ProfileNotLinked();

            try
            {
                var result = await _appraisalService.ProcessSecondLineReviewAsync(id, reviewerId.Value, dto, cancellationToken);
                return Ok(ApiResponse<PerformanceAppraisalDto>.Ok(result, "Penelaahan berkas evaluasi atasan kedua berhasil diproses."));
            }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or KeyNotFoundException or UnauthorizedAccessException)
            {
                return MapError(ex);
            }
        }

        /// <summary>
        /// Konfirmasi penerimaan dan penandatanganan skor akhir resmi oleh pegawai via portal ESS
        /// </summary>
        [HttpPost("{id}/acknowledge")]
        [AccessAction("Acknowledge", "Mengonfirmasi Hasil Penilaian", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("PerformanceAppraisal", "Acknowledge")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<PerformanceAppraisalDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        public async Task<IActionResult> AcknowledgeAppraisal(
            [FromRoute] Guid id,
            [FromBody] AcknowledgeAppraisalRequestDto dto = null!,
            CancellationToken cancellationToken = default)
        {
            var staffId = CurrentWorkforceProfileId();
            if (staffId == null) return ProfileNotLinked();

            try
            {
                var result = await _appraisalService.AcknowledgeAppraisalAsync(id, staffId.Value, dto, cancellationToken);
                return Ok(ApiResponse<PerformanceAppraisalDto>.Ok(result, "Penerimaan hasil evaluasi kinerja berhasil dikonfirmasi pegawai."));
            }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or KeyNotFoundException or UnauthorizedAccessException)
            {
                return MapError(ex);
            }
        }

        /// <summary>
        /// Unggah bukti capaian evaluasi mandiri (VR-PRF-22) (MSL-15, DEC-FER-02 opsi A, 18 Nov 2026). Multipart: <c>file</c>; query <c>category</c>
        /// ("performance-evidence"). Jawaban berisi <c>key</c> yang disimpan di kolom lampiran fitur. PDF/JPG/PNG/WEBP/Word/Excel, maks 10 MB.
        /// </summary>
        [HttpPost("evidence")]
        [AccessAction("SelfAssess", "Mengisi Evaluasi Mandiri", AccessType = AccessTypes.Update, SortOrder = 30)]
        [AccessPermission("PerformanceAppraisal", "SelfAssess")]
        [Tags("Corporate - HRD Performance Appraisals & Multi-Level Review")]
        [RequestSizeLimit(HrAttachmentStorage.DefaultMaximumFileSizeBytes + 1024 * 1024)]
        [ProducesResponseType(typeof(ApiResponse<HrStoredAttachment>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadAttachment(
            [FromForm] IFormFile file,
            [FromQuery] string? category,
            [FromServices] HrAttachmentStorage storage,
            CancellationToken cancellationToken = default)
        {
            var resolvedCategory = string.IsNullOrWhiteSpace(category) ? "performance-evidence" : category.Trim();
            if (!new[] { "performance-evidence" }.Contains(resolvedCategory))
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
        /// Unduh bukti capaian evaluasi (MSL-15). Boleh untuk pengunggah, atau pemegang hak ManagerAssess, SecondLineReview, ViewSubordinates pada fitur ini.
        /// Lampiran milik orang lain tanpa hak dijawab 404 agar keberadaannya tidak bocor.
        /// </summary>
        [HttpGet("evidence/download")]
        [AccessAction("ViewOwn", "Melihat Penilaian Kinerja Sendiri", AccessType = AccessTypes.Read, SortOrder = 31)]
        [AccessPermission("PerformanceAppraisal", "ViewOwn")]
        [Tags("Corporate - HRD Performance Appraisals & Multi-Level Review")]
        [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DownloadAttachment(
            [FromQuery] string key,
            [FromServices] HrAttachmentStorage storage,
            [FromServices] AccessPermissionService accessPermissionService)
        {
            var category = HrAttachmentStorage.CategoryOf(key);
            if (category == null || !new[] { "performance-evidence" }.Contains(category))
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Lampiran tidak ditemukan."));
            }

            try
            {
                var attachment = storage.Open(key, category);
                var allowed = attachment.OwnerUserId == User.GetUserId() || await accessPermissionService.HasAccessAsync(User, "PerformanceAppraisal", "ManagerAssess") || await accessPermissionService.HasAccessAsync(User, "PerformanceAppraisal", "SecondLineReview") || await accessPermissionService.HasAccessAsync(User, "PerformanceAppraisal", "ViewSubordinates");
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
