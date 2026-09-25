using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using BloodOrderPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.DTOs.BloodOrderListDto>;

namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/blood-bank-management/blood-orders")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_BLOOD_BANK_MANAGEMENT",
        moduleName: "Health Service Blood Bank Management",
        displayName: "Blood Order",
        AreaName = "HealthServices",
        ControllerName = "BloodOrder",
        Description = "Membuat, memantau, dan membatalkan order darah",
        SortOrder = 1
    )]
    [Tags("Health Services / Blood Bank Management / Blood Order")]
    public class BbkBloodOrderController : ControllerBase
    {
        private const string LogCategory = "HealthServices.BloodBankManagement.BloodOrder";

        private readonly BbkBloodOrderService _bloodOrderService;
        private readonly LoggerService _loggerService;

        public BbkBloodOrderController(
            BbkBloodOrderService bloodOrderService,
            LoggerService loggerService)
        {
            _bloodOrderService = bloodOrderService;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<BloodOrderFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Blood Order", Description = "Melihat konfigurasi penyaring order darah", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BloodOrder", "Read")]
        public IActionResult GetFilterMetadata()
        {
            return Ok(ApiResponse<BloodOrderFilterMetadataResponse>.Ok(
                BbkBloodOrderService.BuildFilterMetadata(),
                "Konfigurasi penyaring order darah berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<BloodOrderSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Blood Order", Description = "Melihat ringkasan order darah", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BloodOrder", "Read")]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken = default)
        {
            var summary = await _bloodOrderService.GetSummaryAsync(cancellationToken);
            return Ok(ApiResponse<BloodOrderSummaryResponse>.Ok(
                summary,
                "Ringkasan order darah berhasil diambil."));
        }

        /// <summary>Daftar kerja order darah.</summary>
        /// <remarks>
        /// <para>
        /// <c>startDate</c> dan <c>endDate</c> menyaring <c>BbkBloodOrder.CreateDateTime</c>
        /// (<c>DEC-BD-059</c>). Keduanya <b>tanggal operasional waktu aplikasi</b>, bukan saat
        /// UTC: bagian waktu yang ikut terkirim diabaikan, dan <c>endDate</c> bersifat
        /// <b>inklusif sampai akhir hari</b>.
        /// </para>
        /// <para>
        /// Rentang terbalik — <c>startDate</c> melewati <c>endDate</c> — ditolak
        /// <c>400</c> <c>VAL-BD-086</c>, bukan diserahkan ke database.
        /// </para>
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<BloodOrderPagedResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Read", "Read Blood Order", Description = "Melihat daftar order darah", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BloodOrder", "Read")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] Guid? patientId,
            [FromQuery] Guid? encounterId,
            [FromQuery] Guid? serviceUnitId,
            [FromQuery] Guid? bloodComponentId,
            [FromQuery] BbkBloodOrderStatus? orderStatus,
            [FromQuery] BbkOrderSource? orderSource,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? sortBy,
            [FromQuery] string? sortDirection,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var dateRange = BbkBloodOrderService.ResolveCreateDateRange(startDate, endDate);

            if (!dateRange.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    dateRange.ErrorMessage ?? BbkBloodOrderService.InvalidDateRangeMessage));
            }

            var result = await _bloodOrderService.GetPagedAsync(
                search,
                patientId,
                encounterId,
                serviceUnitId,
                bloodComponentId,
                orderStatus,
                orderSource,
                dateRange.StartUtc,
                dateRange.EndExclusiveUtc,
                sortBy,
                sortDirection,
                pageNumber,
                pageSize,
                cancellationToken);

            return Ok(ApiResponse<BloodOrderPagedResult>.Ok(
                result,
                "Daftar order darah berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<BloodOrderDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Blood Order", Description = "Melihat detail order darah", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BloodOrder", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var detail = await _bloodOrderService.GetDetailAsync(
                id,
                GetCurrentUserId(),
                cancellationToken);

            if (detail == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Order darah tidak ditemukan atau sudah dihapus."));
            }

            return Ok(ApiResponse<BloodOrderDetailDto>.Ok(
                detail,
                "Detail order darah berhasil diambil."));
        }

        [HttpGet("{id:guid}/fulfillment")]
        [ProducesResponseType(typeof(ApiResponse<FulfillmentSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Blood Order", Description = "Melihat ringkasan pemenuhan order darah", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BloodOrder", "Read")]
        public async Task<IActionResult> GetFulfillment(Guid id, CancellationToken cancellationToken = default)
        {
            var fulfillment = await _bloodOrderService.GetFulfillmentAsync(id, cancellationToken);

            if (fulfillment == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Order darah tidak ditemukan atau sudah dihapus."));
            }

            return Ok(ApiResponse<FulfillmentSummaryDto>.Ok(
                fulfillment,
                "Ringkasan pemenuhan order darah berhasil diambil."));
        }

        [HttpGet("{id:guid}/status-history")]
        [ProducesResponseType(typeof(ApiResponse<List<BloodOrderTransitionDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Blood Order", Description = "Melihat riwayat status order darah", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BloodOrder", "Read")]
        public async Task<IActionResult> GetStatusHistory(Guid id, CancellationToken cancellationToken = default)
        {
            var history = await _bloodOrderService.GetStatusHistoryAsync(id, cancellationToken);

            if (history == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Order darah tidak ditemukan atau sudah dihapus."));
            }

            return Ok(ApiResponse<List<BloodOrderTransitionDto>>.Ok(
                history,
                "Riwayat status order darah berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<BloodOrderDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Create", "Create Blood Order", Description = "Membuat order darah", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("BloodOrder", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateBloodOrderRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _bloodOrderService.CreateAsync(
                request,
                GetCurrentUserId(),
                cancellationToken);

            return await RespondToWriteAsync(result, "Create");
        }

        [HttpPost("manual")]
        [ProducesResponseType(typeof(ApiResponse<BloodOrderDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Create", "Create Blood Order", Description = "Membuat order darah manual", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("BloodOrder", "Create")]
        public async Task<IActionResult> CreateManual(
            [FromBody] CreateManualBloodOrderRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _bloodOrderService.CreateManualAsync(
                request,
                GetCurrentUserId(),
                cancellationToken);

            return await RespondToWriteAsync(result, "CreateManual");
        }

        [HttpPost("confirm-duplicate")]
        [ProducesResponseType(typeof(ApiResponse<BloodOrderDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Create", "Create Blood Order", Description = "Melanjutkan order darah ganda dengan alasan tertulis", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("BloodOrder", "Create")]
        public async Task<IActionResult> ConfirmDuplicate(
            [FromBody] ConfirmDuplicateOrderRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _bloodOrderService.ConfirmDuplicateAsync(
                request,
                GetCurrentUserId(),
                cancellationToken);

            return await RespondToWriteAsync(result, "ConfirmDuplicate");
        }

        [HttpPost("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<BloodOrderDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Cancel", "Cancel Blood Order", Description = "Membatalkan order darah dengan alasan terkendali", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("BloodOrder", "Cancel")]
        public async Task<IActionResult> Cancel(
            Guid id,
            [FromBody] CancelBloodOrderRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _bloodOrderService.CancelAsync(
                id,
                request,
                GetCurrentUserId(),
                cancellationToken);

            if (result.Outcome != BloodOrderOutcome.Success)
                return MapFailure(result);

            await _loggerService.InfoAsync(
                LogCategory,
                "BloodOrder.Cancel",
                "Membatalkan order darah.",
                new
                {
                    EntityId = result.Entity!.Id,
                    result.Entity.OrderNumber,
                    request.ReasonCode,
                    Controller = "BloodOrder",
                    Action = "Cancel"
                });

            return Ok(ApiResponse<BloodOrderDetailDto>.Ok(
                await BuildDetailAsync(result.Entity!, cancellationToken),
                result.Message));
        }

        private async Task<IActionResult> RespondToWriteAsync(BloodOrderResult result, string action)
        {
            if (result.Outcome != BloodOrderOutcome.Success)
                return MapFailure(result);

            await _loggerService.InfoAsync(
                LogCategory,
                $"BloodOrder.{action}",
                "Membuat order darah.",
                new
                {
                    EntityId = result.Entity!.Id,
                    result.Entity.OrderNumber,
                    result.Entity.PatientId,
                    result.Entity.OrderSource,
                    Controller = "BloodOrder",
                    Action = action
                });

            return Ok(ApiResponse<BloodOrderDetailDto>.Ok(
                await BuildDetailAsync(result.Entity!, HttpContext.RequestAborted),
                result.Message));
        }

        private async Task<BloodOrderDetailDto> BuildDetailAsync(
            Models.BbkBloodOrder entity,
            CancellationToken cancellationToken)
            => await _bloodOrderService.GetDetailAsync(
                   entity.Id,
                   GetCurrentUserId(),
                   cancellationToken)
               ?? BbkBloodOrderService.ToDetail(entity);

        private IActionResult MapFailure(BloodOrderResult result)
        {
            object? errors = null;

            if (result.Outcome == BloodOrderOutcome.DuplicateOrder)
            {
                errors = new
                {
                    code = "VAL-BD-001",
                    duplicateComponentIds = result.DuplicateComponentIds
                };
            }
            return result.Outcome switch
            {
                BloodOrderOutcome.NotFound => NotFound(
                    ApiResponse<object>.Fail(StatusCodes.Status404NotFound, result.Message, errors)),
                BloodOrderOutcome.UnitNotAuthorized => StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<object>.Fail(StatusCodes.Status403Forbidden, result.Message, errors)),
                BloodOrderOutcome.VersionConflict => Conflict(
                    ApiResponse<object>.Fail(StatusCodes.Status409Conflict, result.Message, errors)),
                BloodOrderOutcome.DuplicateOrder => UnprocessableEntity(
                    ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, result.Message, errors)),
                BloodOrderOutcome.NotAllowedByState => UnprocessableEntity(
                    ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, result.Message, errors)),
                _ => BadRequest(
                    ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, result.Message, errors))
            };
        }

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}
