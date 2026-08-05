using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.SelfServices.HumanResource.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/self-services/human-resource/leave/balances")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_EMPLOYEE_SELF_SERVICE",
        moduleName: "Human Resource Employee Self Service",
        displayName: "My Leave Balance",
        AreaName = "SelfServices",
        ControllerName = "MyLeaveBalance",
        Description = "Employee self-service leave balance, detail, and ledger",
        SortOrder = 4)]
    [Tags("Self Services / Human Resource / Leave Balance")]
    public class LeaveBalanceSelfServiceController : ControllerBase
    {
        private readonly LeaveEntitlementBalanceQueryService _service;

        public LeaveBalanceSelfServiceController(LeaveEntitlementBalanceQueryService service)
        {
            _service = service;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<LeaveBalanceFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read My Leave Balance", Description = "Melihat metadata saldo cuti milik sendiri", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MyLeaveBalance", "Read")]
        public IActionResult GetMetadata()
        {
            return Ok(ApiResponse<LeaveBalanceFilterMetadataResponse>.Ok(
                _service.GetBalanceMetadata(),
                "Metadata saldo cuti berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<LeaveSelfServiceSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read My Leave Balance", Description = "Melihat ringkasan saldo cuti milik sendiri", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MyLeaveBalance", "Read")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] int? year,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetMySummaryAsync(
                GetCurrentUserId(),
                year,
                cancellationToken);

            return result == null
                ? NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile user login tidak ditemukan."))
                : Ok(ApiResponse<LeaveSelfServiceSummaryResponse>.Ok(
                    result,
                    "Ringkasan saldo cuti berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<LeaveBalancePagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read My Leave Balance", Description = "Melihat saldo cuti milik sendiri", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MyLeaveBalance", "Read")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] LeaveBalanceQueryRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetMyBalancesAsync(
                GetCurrentUserId(),
                request,
                cancellationToken);

            return result == null
                ? NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile user login tidak ditemukan."))
                : Ok(ApiResponse<LeaveBalancePagedResponse>.Ok(
                    result,
                    "Saldo cuti berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<LeaveBalanceDetailResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read My Leave Balance", Description = "Melihat detail saldo cuti milik sendiri", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MyLeaveBalance", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _service.GetMyBalanceDetailAsync(
                GetCurrentUserId(),
                id,
                cancellationToken);

            return result == null
                ? NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Saldo cuti tidak ditemukan atau bukan milik user login."))
                : Ok(ApiResponse<LeaveBalanceDetailResponse>.Ok(
                    result,
                    "Detail saldo cuti berhasil diambil."));
        }

        [HttpGet("{id:guid}/ledger")]
        [ProducesResponseType(typeof(ApiResponse<LeaveBalanceTransactionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read My Leave Balance", Description = "Melihat ledger saldo cuti milik sendiri", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MyLeaveBalance", "Read")]
        public async Task<IActionResult> GetLedger(
            Guid id,
            [FromQuery] LeaveBalanceLedgerQueryRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetMyLedgerAsync(
                GetCurrentUserId(),
                id,
                request,
                cancellationToken);

            return result == null
                ? NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Saldo cuti tidak ditemukan atau bukan milik user login."))
                : Ok(ApiResponse<LeaveBalanceTransactionPagedResponse>.Ok(
                    result,
                    "Ledger saldo cuti berhasil diambil."));
        }

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                        User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}
