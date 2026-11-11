using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Services;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Controllers
{
    [ApiController]
    [Authorize]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_EMPLOYEE_RELATION",
        moduleName: "Human Resource Employee Relation",
        displayName: "Exit Clearance Multi-Unit",
        AreaName = "Corporate",
        ControllerName = "MultiDeptExitClearance",
        Description = "Matriks serah terima exit clearance lintas unit dan gembok payroll akhir",
        SortOrder = 14)]
    [Tags("Corporate - HRD Multi-Department Exit Clearance & Asset Handover")]
    [Route("api/v1/hrd/exit-clearance")]
    public class MultiDeptExitClearanceController : ControllerBase
    {
        private readonly IExitClearanceService _clearanceService;

        public MultiDeptExitClearanceController(IExitClearanceService clearanceService)
        {
            _clearanceService = clearanceService ?? throw new ArgumentNullException(nameof(clearanceService));
        }

        [HttpGet("{separationId}")]
        [AccessAction("Read", "Melihat Status Exit Clearance", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MultiDeptExitClearance", "Read")]
        public async Task<IActionResult> GetClearanceBySeparation(Guid separationId, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _clearanceService.GetClearanceBySeparationIdAsync(separationId, cancellationToken);
                if (result == null)
                {
                    return NotFound(ApiResponse<ExitClearanceProcessDetailDto>.Fail(404, $"Berkas exit clearance untuk proses terminasi {separationId} tidak ditemukan."));
                }
                return Ok(ApiResponse<ExitClearanceProcessDetailDto>.Ok(result, "Matriks status exit clearance 5 departemen berhasil dimuat."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ExitClearanceProcessDetailDto>.Fail(500, ex.Message));
            }
        }

        [HttpPut("checklists/{checklistId}/signoff")]
        [AccessAction("SignoffChecklist", "Menyetujui Kliring Unit Kerja", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("MultiDeptExitClearance", "SignoffChecklist")]
        public async Task<IActionResult> SignoffChecklist(Guid checklistId, [FromBody] SignoffChecklistRequestDto request, CancellationToken cancellationToken)
        {
            try
            {
                var actorEmail = ResolveActorName();
                var result = await _clearanceService.SignoffChecklistAsync(checklistId, request, actorEmail, cancellationToken);
                return Ok(ApiResponse<ExitClearanceChecklistDto>.Ok(result, "Verifikasi persetujuan serah terima item checklist departemen berhasil disimpan."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<ExitClearanceChecklistDto>.Fail(404, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ExitClearanceChecklistDto>.Fail(500, ex.Message));
            }
        }

        [HttpGet("payroll-lock-status/{employeeId}")]
        [AccessAction("ViewLockStatus", "Melihat Status Gembok Payroll", AccessType = AccessTypes.Read, SortOrder = 3)]
        [AccessPermission("MultiDeptExitClearance", "ViewLockStatus")]
        public async Task<IActionResult> GetPayrollLockStatus(Guid employeeId, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _clearanceService.GetPayrollLockStatusAsync(employeeId, cancellationToken);
                return Ok(ApiResponse<PayrollLockStatusResponseDto>.Ok(result, result.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PayrollLockStatusResponseDto>.Fail(500, ex.Message));
            }
        }

        [HttpPost("{clearanceProcessId}/complete")]
        [AccessAction("Finalize", "Menyelesaikan Exit Clearance", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("MultiDeptExitClearance", "Finalize")]
        public async Task<IActionResult> CompleteExitClearance(Guid clearanceProcessId, [FromBody] FinalizeExitClearanceRequestDto request, CancellationToken cancellationToken)
        {
            try
            {
                var actorEmail = ResolveActorName();
                var result = await _clearanceService.FinalizeExitClearanceAsync(clearanceProcessId, request, actorEmail, cancellationToken);
                return Ok(ApiResponse<ExitClearanceProcessDetailDto>.Ok(result, "Exit clearance selesai 100%. Gembok rilis payroll dan pesangon akhir Sub-10 berhasil dibuka (INT-HR-87)."));
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(ApiResponse<ExitClearanceProcessDetailDto>.Fail(422, ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<ExitClearanceProcessDetailDto>.Fail(404, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ExitClearanceProcessDetailDto>.Fail(500, ex.Message));
            }
        }

        /// <summary>
        /// Nama pengguna yang sedang login untuk jejak aktor. Sengaja tidak memakai aktor cadangan:
        /// permintaan tanpa identitas ditolak, bukan dicatat atas nama orang lain.
        /// </summary>
        private string ResolveActorName()
        {
            var actorName = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(actorName))
            {
                throw new UnauthorizedAccessException("Identitas user pada token tidak valid.");
            }

            return actorName;
        }
    }
}
