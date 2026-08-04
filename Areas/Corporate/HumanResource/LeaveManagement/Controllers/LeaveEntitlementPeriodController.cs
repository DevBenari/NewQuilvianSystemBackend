using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/leave/entitlement-periods")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_LEAVE",
        moduleName: "Human Resource Leave",
        displayName: "Leave Entitlement Period",
        AreaName = "Corporate",
        ControllerName = "LeaveEntitlementPeriod",
        Description = "Corporate human resource leave entitlement period query",
        SortOrder = 1)]
    [Tags("Corporate / Human Resource / Leave Management / Entitlement Period")]
    public class LeaveEntitlementPeriodController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.LeaveManagement";
        private readonly LeaveEntitlementBalanceQueryService _service;
        private readonly LoggerService _loggerService;

        public LeaveEntitlementPeriodController(
            LeaveEntitlementBalanceQueryService service,
            LoggerService loggerService)
        {
            _service = service;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<LeaveEntitlementPeriodFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Leave Entitlement Period", Description = "Melihat metadata filter periode entitlement cuti", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveEntitlementPeriod", "Read")]
        public IActionResult GetFilterMetadata()
        {
            return Ok(ApiResponse<LeaveEntitlementPeriodFilterMetadataResponse>.Ok(
                _service.GetPeriodMetadata(),
                "Metadata filter periode entitlement cuti berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<LeaveEntitlementPeriodSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Leave Entitlement Period", Description = "Melihat ringkasan periode entitlement cuti", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveEntitlementPeriod", "Read")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] LeaveEntitlementPeriodQueryRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetPeriodSummaryAsync(request, cancellationToken);
            return Ok(ApiResponse<LeaveEntitlementPeriodSummaryResponse>.Ok(
                result,
                "Ringkasan periode entitlement cuti berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<LeaveEntitlementPeriodPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Leave Entitlement Period", Description = "Melihat daftar periode entitlement cuti", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveEntitlementPeriod", "Read")]
        public async Task<IActionResult> GetPeriods(
            [FromQuery] LeaveEntitlementPeriodQueryRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetPeriodPagedAsync(request, cancellationToken);
            return Ok(ApiResponse<LeaveEntitlementPeriodPagedResponse>.Ok(
                result,
                "Data periode entitlement cuti berhasil diambil."));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<LeaveEntitlementPeriodOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Leave Entitlement Period", Description = "Melihat pilihan periode entitlement cuti", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveEntitlementPeriod", "Read")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] Guid? leaveTypeId,
            [FromQuery] int? periodYear,
            [FromQuery] bool onlyOpen = false,
            [FromQuery] string? search = null,
            [FromQuery] int take = 25,
            CancellationToken cancellationToken = default)
        {
            var result = await _service.GetPeriodOptionsAsync(
                leaveTypeId,
                periodYear,
                onlyOpen,
                search,
                take,
                cancellationToken);

            return Ok(ApiResponse<List<LeaveEntitlementPeriodOptionResponse>>.Ok(
                result,
                "Pilihan periode entitlement cuti berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<LeaveEntitlementPeriodDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Leave Entitlement Period", Description = "Melihat detail periode entitlement cuti", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveEntitlementPeriod", "Read")]
        public async Task<IActionResult> GetById(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetPeriodDetailAsync(id, cancellationToken);
            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Periode entitlement cuti tidak ditemukan."));
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "LeaveEntitlementPeriod.GetById",
                "Mengambil detail periode entitlement cuti.",
                new { result.Id, result.PeriodCode, result.PeriodStatus });

            return Ok(ApiResponse<LeaveEntitlementPeriodDetailResponse>.Ok(
                result,
                "Detail periode entitlement cuti berhasil diambil."));
        }

        [HttpGet("{id:guid}/balances")]
        [ProducesResponseType(typeof(ApiResponse<LeaveBalancePagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Leave Entitlement Period Balance", Description = "Melihat saldo pada periode entitlement cuti", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveEntitlementPeriod", "Read")]
        public async Task<IActionResult> GetPeriodBalances(
            Guid id,
            [FromQuery] LeaveBalanceQueryRequest request,
            CancellationToken cancellationToken)
        {
            request.LeaveEntitlementPeriodId = id;
            var result = await _service.GetBalancePagedAsync(request, cancellationToken);
            return Ok(ApiResponse<LeaveBalancePagedResponse>.Ok(
                result,
                "Saldo cuti pada periode berhasil diambil."));
        }
    }
}
