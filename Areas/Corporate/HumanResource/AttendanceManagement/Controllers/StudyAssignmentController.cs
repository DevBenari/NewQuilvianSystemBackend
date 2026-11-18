using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/attendance/study-assignments")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_ATTENDANCE",
        moduleName: "Human Resource Attendance",
        displayName: "Study Assignment",
        AreaName = "Corporate",
        ControllerName = "StudyAssignment",
        Description = "Tugas belajar penuh waktu dari SK: diblokir dari roster dan rekap Tugas Belajar",
        SortOrder = 17)]
    [Tags("Corporate / Human Resource / Attendance Management / Study Assignment")]
    public class StudyAssignmentController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.AttendanceManagement";
        private readonly StudyAssignmentService _service;
        private readonly LoggerService _loggerService;

        public StudyAssignmentController(StudyAssignmentService service, LoggerService loggerService)
        {
            _service = service;
            _loggerService = loggerService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<StudyAssignmentResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Study Assignment", Description = "Melihat tugas belajar pegawai", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("StudyAssignment", "Read")]
        public async Task<IActionResult> GetList([FromQuery] StudyAssignmentQueryRequest request, CancellationToken cancellationToken = default) =>
            ToResult(await _service.ListAsync(request, cancellationToken));

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<StudyAssignmentResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Study Assignment", Description = "Melihat tugas belajar pegawai", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("StudyAssignment", "Read")]
        public async Task<IActionResult> GetDetail(Guid id, CancellationToken cancellationToken = default) =>
            ToResult(await _service.GetAsync(id, cancellationToken));

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<StudyAssignmentResponse>), StatusCodes.Status200OK)]
        [AccessAction("Create", "Create Study Assignment", Description = "Mencatat tugas belajar dari SK", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("StudyAssignment", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateStudyAssignmentRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _service.CreateAsync(request, CurrentUserId(), cancellationToken);
            if (result.Success) await AuditAsync("StudyAssignment.Create", result.Data!, "dicatat");
            return ToResult(result);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<StudyAssignmentResponse>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Update Study Assignment", Description = "Mengubah tanggal atau data SK tugas belajar dengan alasan", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("StudyAssignment", "Update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStudyAssignmentRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _service.UpdateAsync(id, request, CurrentUserId(), cancellationToken);
            if (result.Success) await AuditAsync("StudyAssignment.Update", result.Data!, "diubah");
            return ToResult(result);
        }

        [HttpPost("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<StudyAssignmentResponse>), StatusCodes.Status200OK)]
        [AccessAction("Cancel", "Cancel Study Assignment", Description = "Membatalkan tugas belajar dengan alasan", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("StudyAssignment", "Cancel")]
        public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelStudyAssignmentRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _service.CancelAsync(id, request, CurrentUserId(), cancellationToken);
            if (result.Success) await AuditAsync("StudyAssignment.Cancel", result.Data!, "dibatalkan");
            return ToResult(result);
        }

        // Matriks 30.5: ID, ID pegawai, tanggal mulai/selesai; pelaku dari sesi.
        private Task AuditAsync(string action, StudyAssignmentResponse d, string verb) =>
            _loggerService.AuditAsync(LogCategory, action,
                $"Tugas belajar {d.Id} pegawai {d.WorkforceProfileId} {d.StartDate:yyyy-MM-dd} s.d. {d.EndDate:yyyy-MM-dd} {verb}; status {d.Status}; {d.ReprocessScheduled} rekap dijadwalkan hitung ulang.");

        private IActionResult ToResult<T>(StudyAssignmentServiceResult<T> result) =>
            result.Success
                ? Ok(ApiResponse<T>.Ok(result.Data!, result.Message))
                : StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));

        private Guid CurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}
