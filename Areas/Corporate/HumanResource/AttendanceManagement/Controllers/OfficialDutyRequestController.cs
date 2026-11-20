using Microsoft.AspNetCore.Authorization;
using QuilvianSystemBackend.Services.Security;
using QuilvianSystemBackend.Shared.HumanResource.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/corporate/human-resource/attendance/official-duties")]
    [Route("api/v1/corporate/human-resource/attendance-management/official-duties")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_ATTENDANCE",
        moduleName: "Human Resource Attendance",
        displayName: "Official Duty Request Administration",
        AreaName = "Corporate",
        ControllerName = "OfficialDutyRequest",
        Description = "Surat perintah tugas dinas luar rumah sakit dan pengesahan status kehadiran OnDuty staf medis & penunjang",
        SortOrder = 27)]
    [Tags("HR Attendance - Official Duty")]
    public class OfficialDutyRequestController : ControllerBase
    {
        private readonly OfficialDutyRequestService _service;

        public OfficialDutyRequestController(OfficialDutyRequestService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        [HttpGet]
        [AccessAction("Read", "Read Official Duty Requests", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OfficialDutyRequest", "Read")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] OfficialDutyQueryRequest query,
            CancellationToken cancellationToken = default)
        {
            var result = await _service.GetPagedAsync(query, cancellationToken);
            return Ok(ApiResponse<OfficialDutyPagedResponse>.Ok(result, "Data penugasan dinas luar berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Official Duty Request Detail", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("OfficialDutyRequest", "Read")]
        public async Task<IActionResult> GetById(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return ToActionResult(await _service.GetByIdAsync(id, cancellationToken));
        }

        [HttpPost]
        [AccessAction("Create", "Create Official Duty Request", AccessType = AccessTypes.Create, SortOrder = 3)]
        [AccessPermission("OfficialDutyRequest", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateOfficialDutyRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!TryGetUserId(out var userId)) return UnauthorizedResponse();
            return ToActionResult(await _service.CreateAsync(request, userId, cancellationToken));
        }

        [HttpPut("{id:guid}/approve")]
        [HttpPost("{id:guid}/approve")]
        [AccessAction("Approve", "Approve Official Duty Request", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("OfficialDutyRequest", "Approve")]
        public async Task<IActionResult> Approve(
            Guid id,
            [FromBody] ApproveOfficialDutyRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!TryGetUserId(out var userId)) return UnauthorizedResponse();
            return ToActionResult(await _service.ApproveAsync(id, request, userId, cancellationToken));
        }

        [HttpPut("{id:guid}/reject")]
        [HttpPost("{id:guid}/reject")]
        [AccessAction("Reject", "Reject Official Duty Request", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("OfficialDutyRequest", "Reject")]
        public async Task<IActionResult> Reject(
            Guid id,
            [FromBody] RejectOfficialDutyRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!TryGetUserId(out var userId)) return UnauthorizedResponse();
            return ToActionResult(await _service.RejectAsync(id, request, userId, cancellationToken));
        }

        private IActionResult ToActionResult<T>(OfficialDutyRequestServiceResult<T> result)
        {
            return result.Success
                ? StatusCode(result.StatusCode, ApiResponse<T>.Ok(result.Data, result.Message))
                : StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));
        }

        private bool TryGetUserId(out Guid userId)
        {
            var value = User.FindFirstValue("user_id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out userId) && userId != Guid.Empty;
        }

        private IActionResult UnauthorizedResponse()
        {
            return Unauthorized(ApiResponse<object>.Fail(
                StatusCodes.Status401Unauthorized,
                "Identitas user login tidak valid."));
        }

        /// <summary>
        /// Unggah surat tugas/lampiran dinas luar (MSL-15 tahap 2, DEC-FER-02 opsi A, 19 Nov 2026). Multipart <c>file</c>; jawaban berisi <c>key</c>
        /// yang dikirim ke kolom path berkas fitur ini. PDF/JPG/PNG/WEBP/Word/Excel, maksimal 10 MB.
        /// </summary>
        [HttpPost("attachments")]
        [AccessAction("Create", "Create Official Duty Request", AccessType = AccessTypes.Create, SortOrder = 40)]
        [AccessPermission("OfficialDutyRequest", "Create")]
        [Tags("HR Attendance - Official Duty")]
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
                var stored = await storage.SaveAsync("attendance-official-duty", User.GetUserId(), file, cancellationToken);
                return StatusCode(StatusCodes.Status201Created, ApiResponse<HrStoredAttachment>.Ok(stored, "Lampiran berhasil diunggah."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
        }

        /// <summary>
        /// Unduh surat tugas/lampiran dinas luar (MSL-15 tahap 2). Boleh untuk pengunggah atau pemegang hak Read, Approve; selain itu 404.
        /// </summary>
        [HttpGet("attachments/download")]
        [AccessAction("Read", "Read Official Duty Requests", AccessType = AccessTypes.Read, SortOrder = 41)]
        [AccessPermission("OfficialDutyRequest", "Read")]
        [Tags("HR Attendance - Official Duty")]
        [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DownloadHrAttachment(
            [FromQuery] string key,
            [FromServices] HrAttachmentStorage storage,
            [FromServices] AccessPermissionService accessPermissionService)
        {
            try
            {
                var attachment = storage.Open(key, "attendance-official-duty");
                var allowed = attachment.OwnerUserId == User.GetUserId() || await accessPermissionService.HasAccessAsync(User, "OfficialDutyRequest", "Read") || await accessPermissionService.HasAccessAsync(User, "OfficialDutyRequest", "Approve");
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
