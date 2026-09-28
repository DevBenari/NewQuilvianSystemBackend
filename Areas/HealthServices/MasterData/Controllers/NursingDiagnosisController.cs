using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/master-data/nursing-diagnoses")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MASTER_DATA",
        moduleName: "Health Service Master Data",
        displayName: "Nursing Diagnosis",
        AreaName = "HealthServices",
        ControllerName = "NursingDiagnosis",
        Description = "Health service master data nursing diagnosis (SDKI, SLKI, SIKI)",
        SortOrder = 12
    )]
    [Tags("Health Services / Master Data / Nursing Diagnosis (SDKI)")]
    public class NursingDiagnosisController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MasterData.NursingDiagnosis";

        private readonly NursingDiagnosisService _service;
        private readonly LoggerService _loggerService;

        public NursingDiagnosisController(
            NursingDiagnosisService service,
            LoggerService loggerService)
        {
            _service = service;
            _loggerService = loggerService;
        }

        private Guid GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(claim, out var uid) ? uid : Guid.Empty;
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<NursingDiagnosisSummaryResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSummary(CancellationToken ct)
        {
            var summary = await _service.GetSummaryAsync(ct);
            return Ok(ApiResponse<NursingDiagnosisSummaryResponse>.Ok(summary, "Ringkasan diagnosis keperawatan berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<NursingDiagnosisListItemResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPagedList(
            [FromQuery] string? search,
            [FromQuery] Guid? groupId,
            [FromQuery] bool? isActive,
            [FromQuery] int page = 1,
            [FromQuery] int perPage = 25,
            CancellationToken ct = default)
        {
            var paged = await _service.GetPagedListAsync(search, groupId, isActive, page, perPage, ct);
            return Ok(ApiResponse<PagedResult<NursingDiagnosisListItemResponse>>.Ok(paged, "Daftar diagnosis keperawatan berhasil diambil."));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<NursingDiagnosisOptionResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOptions(
            [FromQuery] string? search,
            [FromQuery] int limit = 50,
            CancellationToken ct = default)
        {
            var options = await _service.GetOptionsAsync(search, limit, ct);
            return Ok(ApiResponse<List<NursingDiagnosisOptionResponse>>.Ok(options, "Pilihan diagnosis keperawatan berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<NursingDiagnosisDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDetailById([FromRoute] Guid id, CancellationToken ct)
        {
            var detail = await _service.GetDetailByIdAsync(id, ct);
            if (detail == null)
            {
                return NotFound(ApiResponse<object>.Error("Diagnosis keperawatan tidak ditemukan."));
            }

            return Ok(ApiResponse<NursingDiagnosisDetailResponse>.Ok(detail, "Detail diagnosis keperawatan berhasil diambil."));
        }

        [HttpGet("{id:guid}/bundle")]
        [ProducesResponseType(typeof(ApiResponse<NursingDiagnosisBundleResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBundleById([FromRoute] Guid id, CancellationToken ct)
        {
            var bundle = await _service.GetBundleByIdAsync(id, ct);
            if (bundle == null)
            {
                return NotFound(ApiResponse<object>.Error("Diagnosis keperawatan tidak ditemukan."));
            }

            return Ok(ApiResponse<NursingDiagnosisBundleResponse>.Ok(bundle, "Bundel 3S SDKI/SLKI/SIKI berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<NursingDiagnosisDetailResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(
            [FromBody] CreateNursingDiagnosisRequest request,
            CancellationToken ct)
        {
            try
            {
                var actorUserId = GetCurrentUserId();
                var result = await _service.CreateDiagnosisAsync(request, actorUserId, ct);
                await _loggerService.InfoAsync(LogCategory, "Create", $"Menambahkan diagnosis keperawatan: {request.Code} - {request.Name}", result);
                return StatusCode(StatusCodes.Status201Created, ApiResponse<NursingDiagnosisDetailResponse>.Ok(result, "Diagnosis keperawatan berhasil ditambahkan."));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message));
            }
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<NursingDiagnosisDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(
            [FromRoute] Guid id,
            [FromBody] UpdateNursingDiagnosisRequest request,
            CancellationToken ct)
        {
            try
            {
                var actorUserId = GetCurrentUserId();
                var result = await _service.UpdateDiagnosisAsync(id, request, actorUserId, ct);
                await _loggerService.InfoAsync(LogCategory, "Update", $"Memperbarui diagnosis keperawatan {id}: {request.Code}", result);
                return Ok(ApiResponse<NursingDiagnosisDetailResponse>.Ok(result, "Diagnosis keperawatan berhasil diperbarui."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Error(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message));
            }
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
        {
            var actorUserId = GetCurrentUserId();
            var success = await _service.DeleteDiagnosisAsync(id, actorUserId, ct);
            if (!success)
            {
                return NotFound(ApiResponse<object>.Error("Diagnosis keperawatan tidak ditemukan."));
            }

            await _loggerService.InfoAsync(LogCategory, "Delete", $"Menghapus diagnosis keperawatan {id}", null);
            return Ok(ApiResponse<bool>.Ok(true, "Diagnosis keperawatan berhasil dinonaktifkan."));
        }

        [HttpPost("interventions")]
        [ProducesResponseType(typeof(ApiResponse<NursingDiagnosisInterventionResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddIntervention(
            [FromBody] CreateNursingDiagnosisInterventionRequest request,
            CancellationToken ct)
        {
            try
            {
                var actorUserId = GetCurrentUserId();
                var result = await _service.AddInterventionAsync(request, actorUserId, ct);
                return StatusCode(StatusCodes.Status201Created, ApiResponse<NursingDiagnosisInterventionResponse>.Ok(result, "Intervensi keperawatan SIKI berhasil ditambahkan."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Error(ex.Message));
            }
        }

        [HttpPost("outcomes")]
        [ProducesResponseType(typeof(ApiResponse<NursingDiagnosisOutcomeResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddOutcome(
            [FromBody] CreateNursingDiagnosisOutcomeRequest request,
            CancellationToken ct)
        {
            try
            {
                var actorUserId = GetCurrentUserId();
                var result = await _service.AddOutcomeAsync(request, actorUserId, ct);
                return StatusCode(StatusCodes.Status201Created, ApiResponse<NursingDiagnosisOutcomeResponse>.Ok(result, "Luaran keperawatan SLKI berhasil ditambahkan."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Error(ex.Message));
            }
        }

        [HttpPost("etiologies")]
        [ProducesResponseType(typeof(ApiResponse<NursingDiagnosisEtiologyResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddEtiology(
            [FromBody] CreateNursingDiagnosisEtiologyRequest request,
            CancellationToken ct)
        {
            try
            {
                var actorUserId = GetCurrentUserId();
                var result = await _service.AddEtiologyAsync(request, actorUserId, ct);
                return StatusCode(StatusCodes.Status201Created, ApiResponse<NursingDiagnosisEtiologyResponse>.Ok(result, "Etiologi diagnosis berhasil ditambahkan."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Error(ex.Message));
            }
        }
    }
}
