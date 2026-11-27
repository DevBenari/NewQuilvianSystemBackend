using System;
using System.Collections.Generic;
using QuilvianSystemBackend.Services.Security;
using QuilvianSystemBackend.Shared.HumanResource.Helpers;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Constants;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/corporate/human-resource/recruitment/mcu-clearance")]
    [Produces(MediaTypeNames.Application.Json)]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_RECRUITMENT",
        moduleName: "Human Resource Recruitment",
        displayName: "MCU Pra-Kerja",
        AreaName = "Corporate",
        ControllerName = "OccupationalHealthClearance",
        Description = "Registrasi dan hasil MCU pra-kerja kandidat",
        SortOrder = 17)]
    [Tags("Corporate / Human Resource / Recruitment Management / MCU Clearance")]
    public sealed class McuClearanceController : ControllerBase
    {
        private readonly IMcuClearanceService _mcuService;

        public McuClearanceController(IMcuClearanceService mcuService)
        {
            _mcuService = mcuService ?? throw new ArgumentNullException(nameof(mcuService));
        }

        [HttpGet]
        [AccessAction("Read", "Melihat MCU Pra-Kerja", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OccupationalHealthClearance", "Read")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<McuClearanceListItemResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPaged([FromQuery] McuClearanceQuery query, CancellationToken cancellationToken)
        {
            var result = await _mcuService.GetPagedAsync(query, cancellationToken);
            return ToActionResult(result);
        }

        [HttpGet("eligible-offers")]
        [AccessAction("Read", "Melihat MCU Pra-Kerja", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OccupationalHealthClearance", "Read")]
        [ProducesResponseType(typeof(ApiResponse<List<EligibleMcuOfferResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEligibleOffers([FromQuery] string? search, CancellationToken cancellationToken)
        {
            var result = await _mcuService.GetEligibleOffersAsync(search, cancellationToken);
            return ToActionResult(result);
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Melihat MCU Pra-Kerja", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OccupationalHealthClearance", "Read")]
        [ProducesResponseType(typeof(ApiResponse<McuClearanceResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mcuService.GetByIdAsync(id, User, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost("register")]
        [AccessAction("Create", "Mendaftarkan MCU Pra-Kerja", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("OccupationalHealthClearance", "Create")]
        [ProducesResponseType(typeof(ApiResponse<McuClearanceResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> RegisterMcu([FromBody] RegisterMcuRequest request, CancellationToken cancellationToken)
        {
            var result = await _mcuService.RegisterMcuAsync(request, User, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/record-result")]
        [AccessAction("Evaluate", "Mencatat Hasil MCU Pra-Kerja", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("OccupationalHealthClearance", "Evaluate")]
        [ProducesResponseType(typeof(ApiResponse<McuClearanceResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> RecordResult(
            Guid id,
            [FromBody] RecordMcuResultRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mcuService.RecordResultAsync(id, request, User, cancellationToken);
            return ToActionResult(result);
        }

        private IActionResult ToActionResult<T>(RecruitmentResult<T> result)
        {
            return result.Success
                ? StatusCode(result.StatusCode, ApiResponse<T>.Ok(result.Data!, result.Message))
                : StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));
        }

        /// <summary>
        /// Unggah sertifikat hasil MCU pra-kerja (MSL-15 tahap 2, DEC-FER-02 opsi A, 19 Nov 2026). Multipart <c>file</c>; jawaban berisi <c>key</c>
        /// yang dikirim ke kolom path berkas fitur ini. PDF/JPG/PNG/WEBP/Word/Excel, maksimal 10 MB.
        /// </summary>
        [HttpPost("certificates/attachments")]
        [AccessAction("Evaluate", "Mencatat Hasil MCU Pra-Kerja", AccessType = AccessTypes.Update, SortOrder = 40)]
        [AccessPermission("OccupationalHealthClearance", "Evaluate")]
        [Tags("Corporate / Human Resource / Recruitment Management / MCU Clearance")]
        [RequestSizeLimit(HrAttachmentStorage.DefaultMaximumFileSizeBytes + 1024 * 1024)]
        [ProducesResponseType(typeof(ApiResponse<HrStoredAttachment>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadHrAttachment(
            [FromForm] IFormFile file,
            [FromServices] HrAttachmentStorage storage,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var stored = await storage.SaveAsync("recruitment-mcu-certificate", User.GetUserId(), file, cancellationToken);
                return StatusCode(StatusCodes.Status201Created, ApiResponse<HrStoredAttachment>.Ok(stored, "Lampiran berhasil diunggah."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
        }

        /// <summary>
        /// Unduh sertifikat hasil MCU pra-kerja (MSL-15 tahap 2, HRD-DEC-078). Hanya pengunggah atau petugas medis
        /// pemegang hak OccupationalHealthClearance : Evaluate; selain itu 404.
        /// </summary>
        [HttpGet("certificates/attachments/download")]
        [AccessAction("Evaluate", "Mencatat Hasil MCU Pra-Kerja", AccessType = AccessTypes.Update, SortOrder = 41)]
        [AccessPermission("OccupationalHealthClearance", "Evaluate")]
        [Tags("Corporate / Human Resource / Recruitment Management / MCU Clearance")]
        [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DownloadHrAttachment(
            [FromQuery] string key,
            [FromServices] HrAttachmentStorage storage,
            [FromServices] AccessPermissionService accessPermissionService)
        {
            try
            {
                var attachment = storage.Open(key, "recruitment-mcu-certificate");
                var allowed = attachment.OwnerUserId == User.GetUserId()
                              || await accessPermissionService.HasAccessAsync(User, "OccupationalHealthClearance", "Evaluate");
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
