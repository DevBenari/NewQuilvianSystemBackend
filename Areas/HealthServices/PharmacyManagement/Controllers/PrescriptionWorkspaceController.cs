using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/pharmacy-management/prescription-workspaces")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_PHARMACY",
        moduleName: "Health Service Pharmacy",
        displayName: "Prescription Workspace",
        AreaName = "HealthServices",
        ControllerName = "PrescriptionWorkspace",
        Description = "Workspace terpadu penyusunan resep dokter",
        SortOrder = 3)]
    [Tags("Health Services / Pharmacy Management / Prescription Workspace")]
    public class PrescriptionWorkspaceController : ControllerBase
    {
        private const string LogCategory = "HealthServices.Pharmacy";
        private readonly PrescriptionWorkspaceService _workspaceService;
        private readonly LoggerService _loggerService;

        public PrescriptionWorkspaceController(
            PrescriptionWorkspaceService workspaceService,
            LoggerService loggerService)
        {
            _workspaceService = workspaceService;
            _loggerService = loggerService;
        }

        [HttpGet("{prescriptionId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PrescriptionWorkspaceResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Prescription Workspace", Description = "Melihat workspace resep", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PrescriptionWorkspace", "Read")]
        public async Task<IActionResult> GetWorkspace(
            Guid prescriptionId,
            CancellationToken cancellationToken = default)
        {
            var result = await _workspaceService.GetAsync(prescriptionId, cancellationToken);
            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workspace resep tidak ditemukan."));
            }

            return Ok(ApiResponse<PrescriptionWorkspaceResponse>.Ok(
                result,
                "Workspace resep berhasil diambil."));
        }

        [HttpGet("by-consultation/{consultationId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PrescriptionWorkspaceResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Prescription Workspace", Description = "Melihat workspace resep berdasarkan konsultasi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PrescriptionWorkspace", "Read")]
        public async Task<IActionResult> GetWorkspaceByConsultation(
            Guid consultationId,
            CancellationToken cancellationToken = default)
        {
            var result = await _workspaceService.GetByConsultationAsync(
                consultationId,
                cancellationToken);

            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Resep aktif untuk konsultasi ini belum tersedia."));
            }

            return Ok(ApiResponse<PrescriptionWorkspaceResponse>.Ok(
                result,
                "Workspace resep berhasil diambil."));
        }

        [HttpPatch("{prescriptionId:guid}/autosave")]
        [ProducesResponseType(typeof(ApiResponse<AutosavePrescriptionWorkspaceResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Autosave Prescription Workspace", Description = "Menyimpan otomatis seluruh draft resep dalam satu transaksi", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("PrescriptionWorkspace", "Update")]
        public async Task<IActionResult> Autosave(
            Guid prescriptionId,
            [FromBody] AutosavePrescriptionWorkspaceRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _workspaceService.AutosaveAsync(
                    prescriptionId,
                    request,
                    GetCurrentUserId(),
                    cancellationToken);

                await _loggerService.InfoAsync(
                    LogCategory,
                    "PrescriptionWorkspace.Autosave",
                    "Menyimpan otomatis workspace resep.",
                    new
                    {
                        prescriptionId,
                        result.SavedAt,
                        result.Summary.TotalItemCount,
                        result.Summary.TotalPrice
                    });

                return Ok(ApiResponse<AutosavePrescriptionWorkspaceResponse>.Ok(
                    result,
                    "Draft resep berhasil disimpan otomatis."));
            }
            catch (PrescriptionAutosaveConflictException ex)
            {
                return Conflict(ApiResponse<object>.Fail(
                    StatusCodes.Status409Conflict,
                    ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    ex.Message));
            }
        }

        private Guid GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userId, out var id) ? id : Guid.Empty;
        }
    }
}
