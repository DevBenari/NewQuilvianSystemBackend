using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/emergency-installation-management/emergency-encounter-reconciliations")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_EMERGENCY_INSTALLATION_MANAGEMENT",
        moduleName: "Health Service Emergency Installation Management",
        displayName: "Emergency Encounter Reconciliation",
        AreaName = "HealthServices",
        ControllerName = "EmergencyEncounterReconciliation",
        Description = "Menutup encounter gawat darurat lama yang tertinggal terbuka, berbukti dan dapat dibalik",
        SortOrder = 10
    )]
    [Tags("Health Services / Emergency Installation Management / Emergency Encounter Reconciliation")]
    public class EmergencyEncounterReconciliationController : ControllerBase
    {
        private const string LogCategory = "HealthServices.EmergencyInstallation";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        private readonly EmergencyDocumentNumberService _documentNumberService;

        public EmergencyEncounterReconciliationController(
            ApplicationDbContext dbContext,
            LoggerService loggerService,
            EmergencyDocumentNumberService documentNumberService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
            _documentNumberService = documentNumberService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyEncounterReconciliationFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Emergency Encounter Reconciliation", Description = "Melihat metadata filter rekonsiliasi encounter IGD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmergencyEncounterReconciliation", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = new EmergencyEncounterReconciliationFilterMetadataResponse
            {
                DefaultFilter = new EmergencyEncounterReconciliationDefaultFilterResponse(),
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                SortableFields = new List<string> { "executedat", "runnumber" }
            };

            return Ok(ApiResponse<EmergencyEncounterReconciliationFilterMetadataResponse>.Ok(result, "Metadata filter rekonsiliasi encounter IGD berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyEncounterReconciliationSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Emergency Encounter Reconciliation", Description = "Melihat ringkasan rekonsiliasi encounter IGD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmergencyEncounterReconciliation", "Read")]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Set<EmgEncounterReconciliationRun>().AsNoTracking();

            var summary = new EmergencyEncounterReconciliationSummaryResponse
            {
                TotalRuns = await query.CountAsync(cancellationToken),
                ActiveRuns = await query.CountAsync(x => x.Status == EmergencyReconciliationRunStatus.Executed, cancellationToken),
                ReversedRuns = await query.CountAsync(x => x.Status == EmergencyReconciliationRunStatus.Reversed, cancellationToken),
                TotalReconciledEncounters = await query.SumAsync(x => x.CountK1 + x.CountK1Outpatient, cancellationToken)
            };

            return Ok(ApiResponse<EmergencyEncounterReconciliationSummaryResponse>.Ok(summary, "Ringkasan rekonsiliasi encounter IGD berhasil diambil."));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<EmergencyEncounterReconciliationOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Emergency Encounter Reconciliation Options", Description = "Melihat pilihan rekonsiliasi encounter IGD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmergencyEncounterReconciliation", "Read")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] string? search = null,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Set<EmgEncounterReconciliationRun>().AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x => x.RunNumber.ToLower().Contains(keyword));
            }

            var items = await query.OrderByDescending(x => x.ExecutedAt)
                .Take(100)
                .Select(x => new EmergencyEncounterReconciliationOptionResponse
                {
                    Id = x.Id,
                    RunNumber = x.RunNumber,
                    ExecutedAt = x.ExecutedAt,
                    IsReversed = x.Status == EmergencyReconciliationRunStatus.Reversed
                })
                .ToListAsync(cancellationToken);

            return Ok(ApiResponse<List<EmergencyEncounterReconciliationOptionResponse>>.Ok(items, "Pilihan rekonsiliasi encounter IGD berhasil diambil."));
        }
        [HttpGet("preview")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyEncounterReconciliationPreviewResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Emergency Encounter Reconciliation", Description = "Melihat pratinjau dan riwayat rekonsiliasi encounter IGD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmergencyEncounterReconciliation", "Read")]
        public async Task<IActionResult> Preview(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            (pageNumber, pageSize) = NormalizePaging(pageNumber, pageSize);

            var hasil = await EmergencyEncounterReconciliation.PreviewAsync(
                _dbContext,
                pageNumber,
                pageSize,
                cancellationToken);

            return Ok(ApiResponse<EmergencyEncounterReconciliationPreviewResponse>.Ok(
                hasil,
                "Pratinjau rekonsiliasi encounter IGD berhasil diambil."));
        }

        [HttpPost("runs")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyEncounterReconciliationRunResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Process", "Execute Emergency Encounter Reconciliation", Description = "Menutup encounter gawat darurat lama yang kunjungannya sudah selesai atau batal", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("EmergencyEncounterReconciliation", "Process")]
        public async Task<IActionResult> Execute(
            [FromBody] ExecuteEmergencyEncounterReconciliationRequest request,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();

            var hasil = await EmergencyEncounterReconciliation.ExecuteAsync(
                _dbContext,
                _documentNumberService,
                request,
                actorUserId,
                cancellationToken);

            if (!hasil.Berhasil)
                return StatusCode(hasil.StatusCode, ApiResponse<object>.Fail(hasil.StatusCode, hasil.Penolakan!));

            var run = hasil.Data!;

            await _loggerService.InfoAsync(
                LogCategory,
                "EmergencyEncounterReconciliation.Execute",
                "Menjalankan rekonsiliasi encounter IGD historis.",
                new { EntityId = run.Id, Controller = "EmergencyEncounterReconciliation", Action = "Execute", run.RunNumber, run.CountK1, run.CountK1Outpatient, run.CountK2, run.CountK3, run.CountK4 }
            );

            var respons = ApiResponse<EmergencyEncounterReconciliationRunResponse>.Ok(
                EmergencyEncounterReconciliation.ToRunResponse(run, true),
                $"Rekonsiliasi {run.RunNumber} tercatat; {run.CountK1 + run.CountK1Outpatient} encounter ditutup.");
            respons.StatusCode = hasil.StatusCode;

            return StatusCode(hasil.StatusCode, respons);
        }

        [HttpGet("runs")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<EmergencyEncounterReconciliationRunResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Emergency Encounter Reconciliation", Description = "Melihat pratinjau dan riwayat rekonsiliasi encounter IGD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmergencyEncounterReconciliation", "Read")]
        public async Task<IActionResult> GetRuns(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            (pageNumber, pageSize) = NormalizePaging(pageNumber, pageSize);

            var result = await EmergencyEncounterReconciliation.RiwayatAsync(
                _dbContext,
                pageNumber,
                pageSize,
                cancellationToken);

            return Ok(ApiResponse<PagedResult<EmergencyEncounterReconciliationRunResponse>>.Ok(
                result,
                "Riwayat rekonsiliasi encounter IGD berhasil diambil."));
        }

        [HttpGet("runs/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyEncounterReconciliationRunResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Emergency Encounter Reconciliation", Description = "Melihat pratinjau dan riwayat rekonsiliasi encounter IGD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmergencyEncounterReconciliation", "Read")]
        public async Task<IActionResult> GetRunById(Guid id, CancellationToken cancellationToken = default)
        {
            var run = await EmergencyEncounterReconciliation.FindRunAsync(_dbContext, id, cancellationToken);

            if (run == null)
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Run rekonsiliasi tidak ditemukan."));

            return Ok(ApiResponse<EmergencyEncounterReconciliationRunResponse>.Ok(
                EmergencyEncounterReconciliation.ToRunResponse(run, true),
                "Detail run rekonsiliasi berhasil diambil."));
        }

        [HttpPost("runs/{id:guid}/reverse")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyEncounterReconciliationRunResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Reverse", "Reverse Emergency Encounter Reconciliation", Description = "Membalik satu run rekonsiliasi encounter IGD", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("EmergencyEncounterReconciliation", "Reverse")]
        public async Task<IActionResult> Reverse(
            Guid id,
            [FromBody] ReverseEmergencyEncounterReconciliationRequest request,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();

            var hasil = await EmergencyEncounterReconciliation.ReverseAsync(
                _dbContext,
                id,
                request,
                actorUserId,
                cancellationToken);

            if (!hasil.Berhasil)
                return StatusCode(hasil.StatusCode, ApiResponse<object>.Fail(hasil.StatusCode, hasil.Penolakan!));

            var run = hasil.Data!;
            var dikembalikan = run.Items.Count(x => x.IsReversed);
            var dilewati = run.Items.Count(x => x.ReverseSkipReason != null);

            await _loggerService.InfoAsync(
                LogCategory,
                "EmergencyEncounterReconciliation.Reverse",
                "Membalik run rekonsiliasi encounter IGD.",
                new { EntityId = run.Id, Controller = "EmergencyEncounterReconciliation", Action = "Reverse", run.RunNumber, Dikembalikan = dikembalikan, Dilewati = dilewati }
            );

            return Ok(ApiResponse<EmergencyEncounterReconciliationRunResponse>.Ok(
                EmergencyEncounterReconciliation.ToRunResponse(run, true),
                $"Run {run.RunNumber} dibalik; {dikembalikan} encounter dikembalikan, {dilewati} dilewati."));
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 25 : Math.Min(pageSize, 100);
            return (pageNumber, pageSize);
        }

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}
