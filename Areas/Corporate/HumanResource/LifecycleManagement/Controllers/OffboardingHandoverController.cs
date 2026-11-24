using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;
using QuilvianSystemBackend.Shared.HumanResource.Helpers;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/lifecycle-management/handovers")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_LIFECYCLE_MANAGEMENT",
        moduleName: "Human Resource Lifecycle Management",
        displayName: "Serah Terima Tugas Offboarding",
        AreaName = "Corporate",
        ControllerName = "OffboardingHandoverAdministration",
        Description = "Pengelolaan daftar tugas serah terima pekerjaan, penyerahan berkas dan verifikasi atasan",
        SortOrder = 23)]
    [Tags("Corporate / Human Resource / Lifecycle Management / Offboarding Handover")]
    public class OffboardingHandoverController : ControllerBase
    {
        private readonly OffboardingHandoverService _service;

        public OffboardingHandoverController(OffboardingHandoverService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        [HttpGet("summary")]
        [AccessAction("Read", "Melihat Daftar Serah Terima", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OffboardingHandoverAdministration", "Read")]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
        {
            var result = await _service.GetSummaryAsync(cancellationToken);
            return Ok(ApiResponse<HandoverSummaryResponse>.Ok(result, "Ringkasan serah terima berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Melihat Daftar Serah Terima", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OffboardingHandoverAdministration", "Read")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] string? status,
            [FromQuery] string? search,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var result = await _service.GetPagedAsync(status, search, pageNumber, pageSize, cancellationToken);
            return Ok(ApiResponse<PagedResult<HandoverChecklistListResponse>>.Ok(result, "Daftar berkas serah terima berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Melihat Detail Serah Terima", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OffboardingHandoverAdministration", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _service.GetByIdAsync(id, cancellationToken);
            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Berkas serah terima tidak ditemukan."));
            }
            return Ok(ApiResponse<HandoverChecklistDetailResponse>.Ok(result, "Detail serah terima berhasil diambil."));
        }

        [HttpPost("{id:guid}/tasks")]
        [AccessAction("ManageTasks", "Menambah Tugas Serah Terima", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("OffboardingHandoverAdministration", "ManageTasks")]
        public async Task<IActionResult> AddTask(Guid id, [FromBody] AddHandoverTaskRequest request, CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var actorUserId)) return UnauthorizedResponse();
            try
            {
                var result = await _service.AddTaskAsync(id, request, actorUserId, cancellationToken);
                return StatusCode(StatusCodes.Status201Created, ApiResponse<HandoverTaskResponse>.Ok(result, "Tugas serah terima berhasil ditambahkan."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
            catch (Exception ex) when (HrSafeError.IsDomain(ex))
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }

        [HttpPut("{id:guid}/tasks/{taskId:guid}/submit")]
        [AccessAction("SubmitTask", "Menyerahkan Tugas Serah Terima", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("OffboardingHandoverAdministration", "SubmitTask")]
        public async Task<IActionResult> SubmitTask(Guid id, Guid taskId, [FromBody] SubmitHandoverTaskRequest request, CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var actorUserId)) return UnauthorizedResponse();
            try
            {
                var result = await _service.SubmitTaskAsync(id, taskId, request, actorUserId, User, cancellationToken);
                return Ok(ApiResponse<HandoverTaskResponse>.Ok(result, "Tugas serah terima berhasil diserahkan untuk verifikasi."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail(StatusCodes.Status403Forbidden, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status409Conflict, ApiResponse<object>.Fail(StatusCodes.Status409Conflict, ex.Message));
            }
            catch (Exception ex) when (HrSafeError.IsDomain(ex))
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }

        [HttpPut("{id:guid}/tasks/{taskId:guid}/verify")]
        [AccessAction("VerifyTask", "Memverifikasi Tugas Serah Terima", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("OffboardingHandoverAdministration", "VerifyTask")]
        public async Task<IActionResult> VerifyTask(Guid id, Guid taskId, [FromBody] VerifyHandoverTaskRequest request, CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var actorUserId)) return UnauthorizedResponse();
            try
            {
                var result = await _service.VerifyTaskAsync(id, taskId, request, actorUserId, User, cancellationToken);
                var actionText = request.IsApproved ? "disetujui" : "dikembalikan untuk perbaikan";
                return Ok(ApiResponse<HandoverTaskResponse>.Ok(result, $"Tugas serah terima berhasil {actionText}."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail(StatusCodes.Status403Forbidden, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status409Conflict, ApiResponse<object>.Fail(StatusCodes.Status409Conflict, ex.Message));
            }
            catch (Exception ex) when (HrSafeError.IsDomain(ex))
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }

        [HttpPost("{id:guid}/complete")]
        [AccessAction("Finalize", "Menyelesaikan Berkas Serah Terima", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("OffboardingHandoverAdministration", "Finalize")]
        public async Task<IActionResult> Complete(Guid id, CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var actorUserId)) return UnauthorizedResponse();
            try
            {
                var result = await _service.CompleteChecklistAsync(id, actorUserId, cancellationToken);
                return Ok(ApiResponse<HandoverChecklistDetailResponse>.Ok(result, "Berkas serah terima selesai 100%."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status422UnprocessableEntity, ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, ex.Message));
            }
            catch (Exception ex) when (HrSafeError.IsDomain(ex))
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }

        private bool TryGetUserId(out Guid userId)
        {
            var value = User.FindFirstValue("user_id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out userId) && userId != Guid.Empty;
        }

        private IActionResult UnauthorizedResponse()
        {
            return Unauthorized(ApiResponse<object>.Fail(StatusCodes.Status401Unauthorized, "Identitas user login tidak valid."));
        }
    }
}
