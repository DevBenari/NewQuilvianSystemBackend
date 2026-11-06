using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/corporate/human-resource/leave/balances")]
    [Route("api/v1/corporate/human-resource/leave/balances")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_LEAVE",
        moduleName: "Human Resource Leave",
        displayName: "Leave Balance Ledger",
        AreaName = "Corporate",
        ControllerName = "LeaveBalanceLedger",
        Description = "Corporate human resource leave balance ledger, carry-forward, and audits",
        SortOrder = 3)]
    [Tags("HR Leave - Balances & Ledgers")]
    public class LeaveBalanceLedgerController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.LeaveManagement";

        private readonly LeaveBalanceLedgerService _ledgerService;
        private readonly LeaveCarryForwardProcessorService _carryForwardService;
        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public LeaveBalanceLedgerController(
            LeaveBalanceLedgerService ledgerService,
            LeaveCarryForwardProcessorService carryForwardService,
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _ledgerService = ledgerService;
            _carryForwardService = carryForwardService;
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("my")]
        [ProducesResponseType(typeof(ApiResponse<LeaveBalanceLedgerSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read My Leave Balance", Description = "Melihat saldo cuti mandiri ESS", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveBalanceLedger", "Read")]
        public async Task<IActionResult> GetMyBalance(
            [FromQuery] Guid? leaveTypeId,
            [FromQuery] int? year,
            CancellationToken cancellationToken = default)
        {
            if (!TryGetUserId(out var userId)) return UnauthorizedResponse();

            var workforceProfileId = await GetWorkforceProfileIdFromUserAsync(userId, cancellationToken);
            if (!workforceProfileId.HasValue)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Profil tenaga kerja pegawai tidak ditemukan untuk user login saat ini."));
            }

            var summary = await _ledgerService.GetSummaryAsync(workforceProfileId.Value, leaveTypeId, year, cancellationToken);
            if (summary == null)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Data saldo cuti tidak ditemukan."));
            }

            return Ok(ApiResponse<LeaveBalanceLedgerSummaryDto>.Ok(summary, "Saldo cuti pegawai berhasil diambil."));
        }

        [HttpGet("workforce/{workforceProfileId:guid}/summary")]
        [ProducesResponseType(typeof(ApiResponse<LeaveBalanceLedgerSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Leave Balance", Description = "Melihat ringkasan saldo cuti pegawai spesifik", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("LeaveBalanceLedger", "Read")]
        public async Task<IActionResult> GetWorkforceSummary(
            Guid workforceProfileId,
            [FromQuery] Guid? leaveTypeId,
            [FromQuery] int? year,
            CancellationToken cancellationToken = default)
        {
            var summary = await _ledgerService.GetSummaryAsync(workforceProfileId, leaveTypeId, year, cancellationToken);
            if (summary == null)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Data saldo cuti pegawai tidak ditemukan."));
            }

            return Ok(ApiResponse<LeaveBalanceLedgerSummaryDto>.Ok(summary, "Ringkasan saldo cuti pegawai berhasil diambil."));
        }

        [HttpGet("ledger")]
        [ProducesResponseType(typeof(ApiResponse<LeaveBalanceLedgerPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Leave Balance Ledger", Description = "Melihat daftar riwayat mutasi buku besar cuti", AccessType = AccessTypes.Read, SortOrder = 3)]
        [AccessPermission("LeaveBalanceLedger", "Read")]
        public async Task<IActionResult> GetPagedLedger(
            [FromQuery] TrxLeaveBalanceLedgerQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var data = await _ledgerService.GetPagedLedgerAsync(request, cancellationToken);
            return Ok(ApiResponse<LeaveBalanceLedgerPagedResponse>.Ok(data, "Data riwayat buku besar cuti berhasil diambil."));
        }

        [HttpPost("entitlement")]
        [ProducesResponseType(typeof(ApiResponse<LeaveBalanceLedgerItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Create", "Post Yearly Entitlement", Description = "Mengalokasikan hak cuti tahunan pegawai (12 hari)", AccessType = AccessTypes.Create, SortOrder = 4)]
        [AccessPermission("LeaveBalanceLedger", "Create")]
        public async Task<IActionResult> PostEntitlement(
            [FromBody] PostLeaveEntitlementRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!TryGetUserId(out var userId)) return UnauthorizedResponse();

            var result = await _ledgerService.PostYearlyEntitlementAsync(request, userId, cancellationToken);
            if (result.Success && result.Data != null)
            {
                await _loggerService.InfoAsync(LogCategory, "LeaveBalanceLedger.Entitlement", "Alokasi hak cuti tahunan.", result.Data);
            }
            return ToActionResult(result);
        }

        [HttpPost("adjustment")]
        [ProducesResponseType(typeof(ApiResponse<LeaveBalanceLedgerItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Manual Leave Adjustment", Description = "Melakukan penyesuaian saldo cuti resmi dengan SK Direksi / Dokumen Audit", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("LeaveBalanceLedger", "Update")]
        public async Task<IActionResult> ManualAdjustment(
            [FromBody] ManualLeaveAdjustmentRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!TryGetUserId(out var userId)) return UnauthorizedResponse();

            var result = await _ledgerService.PostManualAdjustmentAsync(request, userId, cancellationToken);
            if (result.Success && result.Data != null)
            {
                await _loggerService.InfoAsync(LogCategory, "LeaveBalanceLedger.Adjustment", "Penyesuaian saldo cuti resmi HR.", result.Data);
            }
            return ToActionResult(result);
        }

        [HttpPost("carry-forward/run-year-end")]
        [ProducesResponseType(typeof(ApiResponse<CarryForwardRunResultDto>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Run Year End Carry Forward", Description = "Menjalankan eksekusi carry-forward akhir tahun max 6 hari", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("LeaveBalanceLedger", "Update")]
        public async Task<IActionResult> RunYearEndCarryForward(
            [FromQuery] int? sourceYear,
            CancellationToken cancellationToken = default)
        {
            if (!TryGetUserId(out var userId)) return UnauthorizedResponse();

            var year = sourceYear ?? DateTime.UtcNow.Year;
            var result = await _carryForwardService.ExecuteYearEndCarryForwardAsync(year, userId, cancellationToken);
            await _loggerService.InfoAsync(LogCategory, "LeaveBalanceLedger.YearEndCarryForward", "Eksekusi carry-forward akhir tahun.", result);

            return Ok(ApiResponse<CarryForwardRunResultDto>.Ok(result, result.Message));
        }

        [HttpPost("carry-forward/run-mid-year-expiry")]
        [ProducesResponseType(typeof(ApiResponse<CarryForwardRunResultDto>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Run Mid Year Carry Forward Expiry", Description = "Menjalankan eksekusi kedaluwarsa kuota bawaan per 30 Juni", AccessType = AccessTypes.Update, SortOrder = 7)]
        [AccessPermission("LeaveBalanceLedger", "Update")]
        public async Task<IActionResult> RunMidYearExpiry(
            [FromQuery] int? currentYear,
            CancellationToken cancellationToken = default)
        {
            if (!TryGetUserId(out var userId)) return UnauthorizedResponse();

            var year = currentYear ?? DateTime.UtcNow.Year;
            var result = await _carryForwardService.ExecuteMidYearCarryForwardExpiryAsync(year, userId, cancellationToken);
            await _loggerService.InfoAsync(LogCategory, "LeaveBalanceLedger.MidYearExpiry", "Eksekusi kedaluwarsa sisa cuti bawaan 30 Juni.", result);

            return Ok(ApiResponse<CarryForwardRunResultDto>.Ok(result, result.Message));
        }

        private IActionResult ToActionResult<T>(LeaveBalanceLedgerServiceResult<T> result)
        {
            return result.Success
                ? StatusCode(result.StatusCode, ApiResponse<T>.Ok(result.Data!, result.Message))
                : StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));
        }

        private bool TryGetUserId(out Guid userId)
        {
            var value = User.FindFirstValue("user_id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out userId) && userId != Guid.Empty;
        }

        private async Task<Guid?> GetWorkforceProfileIdFromUserAsync(Guid userId, CancellationToken cancellationToken)
        {
            var claimVal = User.FindFirstValue("workforce_profile_id");
            if (Guid.TryParse(claimVal, out var wpId) && wpId != Guid.Empty)
            {
                return wpId;
            }

            var user = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
            return user?.WorkforceProfileId;
        }

        private IActionResult UnauthorizedResponse()
        {
            return Unauthorized(ApiResponse<object>.Fail(
                StatusCodes.Status401Unauthorized,
                "Identitas user login tidak valid."));
        }
    }
}
