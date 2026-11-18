using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/lifecycle-management/retirements")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_LIFECYCLE_MANAGEMENT",
        moduleName: "Human Resource Lifecycle Management",
        displayName: "Administrasi Pensiun Pegawai",
        AreaName = "Corporate",
        ControllerName = "RetirementAdministration",
        Description = "Pengelolaan calon dan kasus pensiun pegawai, persetujuan dan handoff offboarding",
        SortOrder = 22)]
    [Tags("Corporate / Human Resource / Lifecycle Management / Retirement Administration")]
    public class RetirementController : ControllerBase
    {
        private readonly RetirementService _service;

        public RetirementController(RetirementService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Melihat Data Pensiun", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RetirementAdministration", "Read")]
        public IActionResult GetMetadata()
        {
            return Ok(ApiResponse<RetirementFilterMetadataResponse>.Ok(
                _service.GetFilterMetadata(),
                "Metadata pensiun berhasil diambil."));
        }

        [HttpGet("summary")]
        [AccessAction("Read", "Melihat Data Pensiun", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RetirementAdministration", "Read")]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
        {
            var result = await _service.GetSummaryAsync(cancellationToken);
            return Ok(ApiResponse<RetirementSummaryResponse>.Ok(result, "Ringkasan pensiun berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Melihat Data Pensiun", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RetirementAdministration", "Read")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] string? status,
            [FromQuery] string? search,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var result = await _service.GetPagedAsync(status, search, startDate, endDate, pageNumber, pageSize, cancellationToken);
            return Ok(ApiResponse<PagedResult<RetirementListResponse>>.Ok(result, "Daftar kasus pensiun berhasil diambil."));
        }

        [HttpGet("eligible-employees")]
        [AccessAction("Read", "Melihat Data Pensiun", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RetirementAdministration", "Read")]
        public async Task<IActionResult> GetEligibleEmployees(
            [FromQuery] int minAge = 55,
            CancellationToken cancellationToken = default)
        {
            var result = await _service.GetEligibleEmployeesAsync(minAge, cancellationToken);
            return Ok(ApiResponse<List<EligibleRetirementEmployeeResponse>>.Ok(result, "Daftar pegawai mendekati usia pensiun berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Melihat Data Pensiun", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RetirementAdministration", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _service.GetByIdAsync(id, cancellationToken);
            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Kasus pensiun tidak ditemukan."));
            }
            return Ok(ApiResponse<RetirementDetailResponse>.Ok(result, "Detail kasus pensiun berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Create", "Membuat Kasus Pensiun", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("RetirementAdministration", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateRetirementRequest request, CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var actorUserId)) return UnauthorizedResponse();
            try
            {
                var result = await _service.CreateAsync(request, actorUserId, cancellationToken);
                return StatusCode(StatusCodes.Status201Created, ApiResponse<RetirementDetailResponse>.Ok(result, "Kasus pensiun berhasil dibuat."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Mengubah Data Pensiun", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("RetirementAdministration", "Update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRetirementRequest request, CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var actorUserId)) return UnauthorizedResponse();
            try
            {
                var result = await _service.UpdateAsync(id, request, actorUserId, cancellationToken);
                return Ok(ApiResponse<RetirementDetailResponse>.Ok(result, "Data kasus pensiun berhasil diperbarui."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status409Conflict, ApiResponse<object>.Fail(StatusCodes.Status409Conflict, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }

        [HttpPost("{id:guid}/approve")]
        [AccessAction("Approve", "Menyetujui Kasus Pensiun", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("RetirementAdministration", "Approve")]
        public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveRetirementRequest? request, CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var actorUserId)) return UnauthorizedResponse();
            try
            {
                var result = await _service.ApproveAsync(id, request, actorUserId, cancellationToken);
                return Ok(ApiResponse<RetirementDetailResponse>.Ok(result, "Kasus pensiun berhasil disetujui."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }

        [HttpPost("{id:guid}/handoff")]
        [AccessAction("Handoff", "Handoff Pensiun ke Offboarding", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("RetirementAdministration", "Handoff")]
        public async Task<IActionResult> Handoff(Guid id, [FromBody] RetirementHandoffRequest request, CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var actorUserId)) return UnauthorizedResponse();
            try
            {
                var result = await _service.HandoffAsync(id, request, actorUserId, cancellationToken);
                return Ok(ApiResponse<RetirementDetailResponse>.Ok(result, "Kasus pensiun berhasil di-handoff ke terminasi dan offboarding."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
            catch (Exception ex)
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
