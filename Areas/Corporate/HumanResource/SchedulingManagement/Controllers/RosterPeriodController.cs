using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/corporate/human-resource/scheduling/roster-periods")]
    [Route("api/v1/corporate/human-resource/scheduling-management/roster-periods")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_SCHEDULING_MANAGEMENT",
        moduleName: "Human Resource Scheduling Management",
        displayName: "Roster Period Administration",
        AreaName = "Corporate",
        ControllerName = "RosterPeriod",
        Description = "Pengelolaan periode roster, alokasi shift staf, validasi konflik jadwal dan penutupan roster",
        SortOrder = 22)]
    [Tags("HR Scheduling - Roster Period")]
    public class RosterPeriodController : ControllerBase
    {
        private readonly RosterPeriodService _service;

        public RosterPeriodController(RosterPeriodService service)
        {
            _service = service;
        }

        [HttpGet("summary")]
        [AccessAction("Read", "Read Roster Period Summary", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RosterPeriod", "Read")]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
        {
            var result = await _service.GetSummaryAsync(cancellationToken);
            return Ok(ApiResponse<RosterPeriodSummaryResponse>.Ok(
                result,
                "Ringkasan statistik periode roster berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read Roster Periods", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RosterPeriod", "Read")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] string? status = null,
            [FromQuery] Guid? departmentId = null,
            CancellationToken cancellationToken = default)
        {
            var result = await _service.GetPagedAsync(
                pageNumber,
                pageSize,
                search,
                status,
                departmentId,
                cancellationToken);

            return Ok(ApiResponse<PagedResult<RosterPeriodResponse>>.Ok(
                result,
                "Daftar periode roster berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Roster Period Detail", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RosterPeriod", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            return ToActionResult(await _service.GetByIdAsync(id, cancellationToken));
        }

        [HttpPost]
        [AccessAction("Create", "Create Roster Period", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("RosterPeriod", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateRosterPeriodRequest request,
            CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var userId)) return UnauthorizedResponse();
            return ToActionResult(await _service.CreateAsync(request, userId, cancellationToken));
        }

        [HttpPost("{id:guid}/assignments")]
        [AccessAction("Create", "Add Roster Assignment", AccessType = AccessTypes.Create, SortOrder = 3)]
        [AccessPermission("RosterAssignment", "Create")]
        public async Task<IActionResult> AddAssignment(
            Guid id,
            [FromBody] CreateRosterAssignmentRequest request,
            CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var userId)) return UnauthorizedResponse();
            return ToActionResult(await _service.AddAssignmentAsync(id, request, userId, cancellationToken));
        }

        /// <summary>
        /// Pengisian otomatis daftar staf eligible unit dan status pendampingan (BE-HRD-091).
        /// </summary>
        [HttpPost("{id:guid}/populate-staff")]
        [AccessAction("Create", "Populate Eligible Staff", AccessType = AccessTypes.Create, SortOrder = 4)]
        [AccessPermission("RosterAssignment", "Create")]
        public async Task<IActionResult> PopulateEligibleStaff(
            Guid id,
            CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var userId)) return UnauthorizedResponse();
            return ToActionResult(await _service.PopulateEligibleStaffAsync(id, userId, cancellationToken));
        }

        /// <summary>
        /// Salin matriks penugasan dan shift dari periode roster sebelumnya (BE-HRD-092).
        /// </summary>
        [HttpPost("{id:guid}/copy-previous")]
        [AccessAction("Create", "Copy Previous Roster Period", AccessType = AccessTypes.Create, SortOrder = 5)]
        [AccessPermission("RosterAssignment", "Create")]
        public async Task<IActionResult> CopyPrevious(
            Guid id,
            [FromBody] CopyPreviousRosterPeriodRequest request,
            CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var userId)) return UnauthorizedResponse();
            return ToActionResult(await _service.CopyPreviousPeriodAsync(id, request, userId, cancellationToken));
        }

        /// <summary>
        /// Terapkan pola shift berulang ke kelompok pegawai dalam periode roster (BE-HRD-092).
        /// </summary>
        [HttpPost("{id:guid}/apply-pattern")]
        [AccessAction("Create", "Apply Shift Pattern", AccessType = AccessTypes.Create, SortOrder = 6)]
        [AccessPermission("RosterAssignment", "Create")]
        public async Task<IActionResult> ApplyPattern(
            Guid id,
            [FromBody] ApplyShiftPatternRequest request,
            CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var userId)) return UnauthorizedResponse();
            return ToActionResult(await _service.ApplyShiftPatternAsync(id, request, userId, cancellationToken));
        }

        /// <summary>
        /// Validasi keselamatan jadwal dinas RS (B1-B7 dan W1-W5) (BE-HRD-089, BE-HRD-090).
        /// </summary>
        [HttpPost("{id:guid}/validate")]
        [AccessAction("Update", "Validate Roster Period Conflicts", AccessType = AccessTypes.Update, SortOrder = 7)]
        [AccessPermission("RosterPeriod", "Update")]
        public async Task<IActionResult> Validate(
            Guid id,
            CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var userId)) return UnauthorizedResponse();
            return ToActionResult(await _service.ValidateRosterPeriodAsync(id, userId, cancellationToken));
        }

        /// <summary>
        /// Penerbitan roster mandiri Kepala Unit dengan recheck otomatis (BE-HRD-093).
        /// </summary>
        [HttpPost("{id:guid}/publish")]
        [AccessAction("Update", "Publish Roster Period", AccessType = AccessTypes.Update, SortOrder = 8)]
        [AccessPermission("RosterPeriod", "Update")]
        public async Task<IActionResult> Publish(
            Guid id,
            [FromBody] PublishRosterPeriodRequest? request,
            CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var userId)) return UnauthorizedResponse();
            return ToActionResult(await _service.PublishRosterPeriodAsync(id, request ?? new PublishRosterPeriodRequest(), userId, cancellationToken));
        }

        /// <summary>
        /// Penutupan final dan penguncian periode roster oleh HRD Payroll (BE-HRD-098).
        /// </summary>
        [HttpPost("{id:guid}/close")]
        [AccessAction("Update", "Close Roster Period", AccessType = AccessTypes.Update, SortOrder = 9)]
        [AccessPermission("RosterPeriod", "Update")]
        public async Task<IActionResult> Close(
            Guid id,
            CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var userId)) return UnauthorizedResponse();
            return ToActionResult(await _service.CloseRosterPeriodAsync(id, userId, cancellationToken));
        }

        private IActionResult ToActionResult<T>(SchedulingRequestServiceResult<T> result)
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
    }
}
