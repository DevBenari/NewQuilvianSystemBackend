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
        displayName: "Terminasi & Pesangon",
        AreaName = "Corporate",
        ControllerName = "EmployeeSeparation",
        Description = "Pengajuan terminasi kerja, kalkulasi pesangon PP 35/2021, dan pengesahan keuangan",
        SortOrder = 13)]
    [Tags("Corporate - HRD Employee Separation & PP35 Severance")]
    [Route("api/v1/hrd/separations")]
    public class EmployeeSeparationController : ControllerBase
    {
        private readonly IEmployeeSeparationService _separationService;
        private readonly ISeveranceCalculationEngine _severanceEngine;

        public EmployeeSeparationController(
            IEmployeeSeparationService separationService,
            ISeveranceCalculationEngine severanceEngine)
        {
            _separationService = separationService ?? throw new ArgumentNullException(nameof(separationService));
            _severanceEngine = severanceEngine ?? throw new ArgumentNullException(nameof(severanceEngine));
        }

        [HttpPost]
        [AccessAction("Initiate", "Mengajukan Terminasi Kerja", AccessType = AccessTypes.Create, SortOrder = 1)]
        [AccessPermission("EmployeeSeparation", "Initiate")]
        public async Task<IActionResult> InitiateSeparation([FromBody] InitiateSeparationRequestDto request, CancellationToken cancellationToken)
        {
            try
            {
                var actorEmail = ResolveActorName();
                var result = await _separationService.InitiateSeparationAsync(request, actorEmail, cancellationToken);
                return StatusCode(201, ApiResponse<SeparationResponseDto>.Ok(result, "Proses terminasi dan checklist exit clearance 5 departemen berhasil diinisiasi."));
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(ApiResponse<SeparationResponseDto>.Fail(422, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<SeparationResponseDto>.Fail(500, ex.Message));
            }
        }

        [HttpGet]
        [AccessAction("Read", "Melihat Daftar Proses Terminasi", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("EmployeeSeparation", "Read")]
        public async Task<IActionResult> GetSeparations(
            [FromQuery] string? status,
            [FromQuery] string? separationReason,
            [FromQuery] Guid? departmentId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _separationService.GetSeparationsAsync(status, separationReason, departmentId, page, pageSize, cancellationToken);
                return Ok(ApiResponse<PagedResult<SeparationResponseDto>>.Ok(result, "Daftar proses terminasi kerja berhasil diambil."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PagedResult<SeparationResponseDto>>.Fail(500, ex.Message));
            }
        }

        [HttpPost("{separationId}/calculate-severance")]
        [AccessAction("CalculateSeverance", "Menghitung Pesangon", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("EmployeeSeparation", "CalculateSeverance")]
        public async Task<IActionResult> CalculateSeverance(Guid separationId, [FromBody] CalculateSeveranceRequestDto request, CancellationToken cancellationToken)
        {
            try
            {
                var actorEmail = ResolveActorName();
                request.SeparationId = separationId;
                var result = await _severanceEngine.CalculateSeveranceAsync(request, actorEmail, cancellationToken);
                return Ok(ApiResponse<SeveranceCalculationResponseDto>.Ok(result, "Kompensasi pesangon berhasil dihitung secara deterministik berdasar regulasi PP 35/2021."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<SeveranceCalculationResponseDto>.Fail(404, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<SeveranceCalculationResponseDto>.Fail(500, ex.Message));
            }
        }

        [HttpGet("{separationId}/severance-breakdown")]
        [AccessAction("ViewSeverance", "Melihat Rincian Pesangon", AccessType = AccessTypes.Read, SortOrder = 4)]
        [AccessPermission("EmployeeSeparation", "ViewSeverance")]
        public async Task<IActionResult> GetSeveranceBreakdown(Guid separationId, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _severanceEngine.GetSeveranceBreakdownAsync(separationId, cancellationToken);
                if (result == null)
                {
                    return NotFound(ApiResponse<SeveranceCalculationDetailDto>.Fail(404, $"Lembar rincian pesangon untuk proses terminasi {separationId} belum dihitung."));
                }
                return Ok(ApiResponse<SeveranceCalculationDetailDto>.Ok(result, "Lembar transparansi kalkulasi pesangon PP 35/2021 berhasil dimuat."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<SeveranceCalculationDetailDto>.Fail(500, ex.Message));
            }
        }

        [HttpPost("{separationId}/approve-severance")]
        [AccessAction("ApproveSeveranceFinance", "Mengesahkan Pencairan Pesangon", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("EmployeeSeparation", "ApproveSeveranceFinance")]
        public async Task<IActionResult> ApproveSeverance(Guid separationId, [FromBody] ApproveSeveranceRequestDto request, CancellationToken cancellationToken)
        {
            try
            {
                var actorEmail = ResolveActorName();
                var result = await _severanceEngine.ApproveSeveranceAsync(separationId, request, actorEmail, cancellationToken);
                return Ok(ApiResponse<SeveranceCalculationResponseDto>.Ok(result, "Pengesahan lembar pesangon oleh Direktur Keuangan berhasil dicatat."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<SeveranceCalculationResponseDto>.Fail(404, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<SeveranceCalculationResponseDto>.Fail(500, ex.Message));
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
