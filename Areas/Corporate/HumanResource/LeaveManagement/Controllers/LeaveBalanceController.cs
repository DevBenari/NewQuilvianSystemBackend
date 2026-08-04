using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/leave/balances")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_LEAVE",
        moduleName: "Human Resource Leave",
        displayName: "Leave Balance",
        AreaName = "Corporate",
        ControllerName = "LeaveBalance",
        Description = "Corporate human resource leave balance, ledger, history, and self-service query",
        SortOrder = 2)]
    [Tags("Corporate / Human Resource / Leave Management / Leave Balance")]
    public class LeaveBalanceController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.LeaveManagement";
        private readonly LeaveEntitlementBalanceQueryService _service;
        private readonly LoggerService _loggerService;

        public LeaveBalanceController(
            LeaveEntitlementBalanceQueryService service,
            LoggerService loggerService)
        {
            _service = service;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<LeaveBalanceFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Leave Balance", Description = "Melihat metadata filter saldo cuti", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveBalance", "Read")]
        public IActionResult GetFilterMetadata()
        {
            return Ok(ApiResponse<LeaveBalanceFilterMetadataResponse>.Ok(
                _service.GetBalanceMetadata(),
                "Metadata filter saldo cuti berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<LeaveBalanceSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Leave Balance", Description = "Melihat ringkasan saldo cuti", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveBalance", "Read")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] LeaveBalanceQueryRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetBalanceSummaryAsync(request, cancellationToken);
            return Ok(ApiResponse<LeaveBalanceSummaryResponse>.Ok(
                result,
                "Ringkasan saldo cuti berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<LeaveBalancePagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Leave Balance", Description = "Melihat daftar saldo cuti", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveBalance", "Read")]
        public async Task<IActionResult> GetBalances(
            [FromQuery] LeaveBalanceQueryRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetBalancePagedAsync(request, cancellationToken);
            return Ok(ApiResponse<LeaveBalancePagedResponse>.Ok(
                result,
                "Data saldo cuti berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<LeaveBalanceDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Leave Balance", Description = "Melihat detail saldo cuti", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveBalance", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _service.GetBalanceDetailAsync(id, null, cancellationToken);
            if (result == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Saldo cuti tidak ditemukan."));

            await _loggerService.InfoAsync(
                LogCategory,
                "LeaveBalance.GetById",
                "Mengambil detail saldo cuti.",
                new { result.Id, result.WorkforceProfileId, result.LeaveTypeId, result.Year });

            return Ok(ApiResponse<LeaveBalanceDetailResponse>.Ok(result, "Detail saldo cuti berhasil diambil."));
        }

        [HttpGet("{id:guid}/ledger")]
        [ProducesResponseType(typeof(ApiResponse<LeaveBalanceTransactionPagedResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Leave Balance Ledger", Description = "Melihat ledger saldo cuti", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveBalance", "Read")]
        public async Task<IActionResult> GetLedger(
            Guid id,
            [FromQuery] LeaveBalanceLedgerQueryRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetLedgerAsync(id, request, null, cancellationToken);
            if (result == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Saldo cuti tidak ditemukan."));
            return Ok(ApiResponse<LeaveBalanceTransactionPagedResponse>.Ok(result, "Ledger saldo cuti berhasil diambil."));
        }

        [HttpGet("{id:guid}/entitlements")]
        [ProducesResponseType(typeof(ApiResponse<List<LeaveEntitlementHistoryResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Leave Balance History", Description = "Melihat riwayat entitlement cuti", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveBalance", "Read")]
        public async Task<IActionResult> GetEntitlements(Guid id, CancellationToken cancellationToken)
        {
            var result = await _service.GetEntitlementsAsync(id, null, cancellationToken);
            if (result == null) return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Saldo cuti tidak ditemukan."));
            return Ok(ApiResponse<List<LeaveEntitlementHistoryResponse>>.Ok(result, "Riwayat entitlement cuti berhasil diambil."));
        }

        [HttpGet("{id:guid}/accruals")]
        [ProducesResponseType(typeof(ApiResponse<List<LeaveAccrualHistoryResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Leave Balance History", Description = "Melihat riwayat accrual cuti", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveBalance", "Read")]
        public async Task<IActionResult> GetAccruals(Guid id, CancellationToken cancellationToken)
        {
            var result = await _service.GetAccrualsAsync(id, null, cancellationToken);
            if (result == null) return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Saldo cuti tidak ditemukan."));
            return Ok(ApiResponse<List<LeaveAccrualHistoryResponse>>.Ok(result, "Riwayat accrual cuti berhasil diambil."));
        }

        [HttpGet("{id:guid}/carry-forwards")]
        [ProducesResponseType(typeof(ApiResponse<List<LeaveCarryForwardHistoryResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Leave Balance History", Description = "Melihat riwayat carry forward cuti", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveBalance", "Read")]
        public async Task<IActionResult> GetCarryForwards(Guid id, CancellationToken cancellationToken)
        {
            var result = await _service.GetCarryForwardsAsync(id, null, cancellationToken);
            if (result == null) return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Saldo cuti tidak ditemukan."));
            return Ok(ApiResponse<List<LeaveCarryForwardHistoryResponse>>.Ok(result, "Riwayat carry forward cuti berhasil diambil."));
        }

        [HttpGet("{id:guid}/adjustments")]
        [ProducesResponseType(typeof(ApiResponse<List<LeaveAdjustmentHistoryResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Leave Balance History", Description = "Melihat riwayat adjustment cuti", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveBalance", "Read")]
        public async Task<IActionResult> GetAdjustments(Guid id, CancellationToken cancellationToken)
        {
            var result = await _service.GetAdjustmentsAsync(id, null, cancellationToken);
            if (result == null) return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Saldo cuti tidak ditemukan."));
            return Ok(ApiResponse<List<LeaveAdjustmentHistoryResponse>>.Ok(result, "Riwayat adjustment cuti berhasil diambil."));
        }

        [HttpGet("{id:guid}/reconciliation")]
        [ProducesResponseType(typeof(ApiResponse<LeaveBalanceReconciliationResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Leave Balance Reconciliation", Description = "Melihat rekonsiliasi saldo dan ledger cuti", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveBalance", "Read")]
        public async Task<IActionResult> GetReconciliation(Guid id, CancellationToken cancellationToken)
        {
            var result = await _service.GetReconciliationAsync(id, null, cancellationToken);
            if (result == null) return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Saldo cuti tidak ditemukan."));
            return Ok(ApiResponse<LeaveBalanceReconciliationResponse>.Ok(result, "Rekonsiliasi saldo cuti berhasil diambil."));
        }

        [HttpGet("my-balances/summary")]
        [ProducesResponseType(typeof(ApiResponse<LeaveSelfServiceSummaryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read My Leave Balance", Description = "Melihat ringkasan saldo cuti user login", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveBalanceSelfService", "Read")]
        public async Task<IActionResult> GetMySummary([FromQuery] int? year, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _service.GetMySummaryAsync(userId, year, cancellationToken);
            if (result == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Workforce profile user login tidak ditemukan."));
            return Ok(ApiResponse<LeaveSelfServiceSummaryResponse>.Ok(result, "Ringkasan saldo cuti saya berhasil diambil."));
        }

        [HttpGet("my-balances")]
        [ProducesResponseType(typeof(ApiResponse<LeaveBalancePagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read My Leave Balance", Description = "Melihat saldo cuti user login", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveBalanceSelfService", "Read")]
        public async Task<IActionResult> GetMyBalances(
            [FromQuery] LeaveBalanceQueryRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetMyBalancesAsync(GetCurrentUserId(), request, cancellationToken);
            if (result == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Workforce profile user login tidak ditemukan."));
            return Ok(ApiResponse<LeaveBalancePagedResponse>.Ok(result, "Saldo cuti saya berhasil diambil."));
        }

        [HttpGet("my-balances/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<LeaveBalanceDetailResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read My Leave Balance", Description = "Melihat detail saldo cuti user login", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveBalanceSelfService", "Read")]
        public async Task<IActionResult> GetMyBalanceDetail(Guid id, CancellationToken cancellationToken)
        {
            var result = await _service.GetMyBalanceDetailAsync(GetCurrentUserId(), id, cancellationToken);
            if (result == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Saldo cuti tidak ditemukan atau bukan milik user login."));
            return Ok(ApiResponse<LeaveBalanceDetailResponse>.Ok(result, "Detail saldo cuti saya berhasil diambil."));
        }

        [HttpGet("my-balances/{id:guid}/ledger")]
        [ProducesResponseType(typeof(ApiResponse<LeaveBalanceTransactionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read My Leave Balance", Description = "Melihat ledger saldo cuti user login", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveBalanceSelfService", "Read")]
        public async Task<IActionResult> GetMyLedger(
            Guid id,
            [FromQuery] LeaveBalanceLedgerQueryRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetMyLedgerAsync(GetCurrentUserId(), id, request, cancellationToken);
            if (result == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Saldo cuti tidak ditemukan atau bukan milik user login."));
            return Ok(ApiResponse<LeaveBalanceTransactionPagedResponse>.Ok(result, "Ledger saldo cuti saya berhasil diambil."));
        }

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}
