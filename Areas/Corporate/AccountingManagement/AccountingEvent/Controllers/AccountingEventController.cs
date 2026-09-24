using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingEvent.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingEvent.Services;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingEvent.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/accounting/accounting-events")]
    [AccessController(
        moduleCode: "ACCOUNTING_EVENT",
        moduleName: "Accounting Event",
        displayName: "Accounting Event",
        AreaName = "Corporate",
        ControllerName = "AccountingEvent",
        Description = "Corporate accounting financial event inbox",
        SortOrder = 1)]
    [Tags("Corporate - Accounting - Accounting Event")]
    public class AccountingEventController : ControllerBase
    {
        private const string LogCategory = "Corporate.Accounting.AccountingEvent";

        private readonly AccAccountingEventService _service;
        private readonly LoggerService _loggerService;

        public AccountingEventController(
            AccAccountingEventService service,
            LoggerService loggerService)
        {
            _service = service;
            _loggerService = loggerService;
        }

        [HttpGet]
        [AccessAction("Read", "Read Accounting Event", Description = "Melihat kotak masuk kejadian keuangan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AccountingEvent", "Read")]
        public async Task<IActionResult> GetPaged([FromQuery] AccountingEventPagedQuery query, CancellationToken ct)
            => ToActionResult(await _service.GetPagedAsync(query, ct));

        [HttpGet("summary")]
        [AccessAction("Read", "Read Accounting Event", Description = "Melihat jumlah kejadian keuangan per status", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AccountingEvent", "Read")]
        public async Task<IActionResult> GetSummary([FromQuery] Guid? legalEntityId, CancellationToken ct)
            => ToActionResult(await _service.GetSummaryAsync(legalEntityId, ct));

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Accounting Event", Description = "Melihat rincian kejadian keuangan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AccountingEvent", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
            => ToActionResult(await _service.GetByIdAsync(id, ct));

        [HttpPost("{id:guid}/retry")]
        [AccessAction("Retry", "Retry Accounting Event", Description = "Mencoba ulang kejadian keuangan yang gagal atau tertahan", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("AccountingEvent", "Retry")]
        public async Task<IActionResult> Retry(Guid id, CancellationToken ct)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return IdentitasTidakValid();

            var hasil = await _service.CobaUlangAsync(id, actor, ct);

            await CatatAsync("AccountingEvent.Retry", hasil, new
            {
                EntityId = id,
                hasil.StatusCode,
                EventStatus = hasil.Data?.EventStatus,
                hasil.Data?.HoldReasonCode,
                hasil.Data?.JournalNumber
            });

            return ToActionResult(hasil);
        }

        [HttpPatch("{id:guid}/ignore")]
        [AccessAction("Ignore", "Ignore Accounting Event", Description = "Menandai kejadian keuangan yang gagal sebagai diabaikan", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("AccountingEvent", "Ignore")]
        public async Task<IActionResult> Ignore(Guid id, [FromBody] IgnoreAccountingEventRequest request, CancellationToken ct)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return IdentitasTidakValid();

            var hasil = await _service.AbaikanAsync(id, request, actor, ct);

            await CatatAsync("AccountingEvent.Ignore", hasil, new
            {
                EntityId = id,
                hasil.StatusCode,
                EventStatus = hasil.Data?.EventStatus
            });

            return ToActionResult(hasil);
        }

        [HttpPost]
        [AccessAction("Receive", "Receive Accounting Event", Description = "Menerima kejadian keuangan dari Finance lewat akun layanan", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("AccountingEvent", "Receive")]
        [ProducesResponseType(typeof(ApiResponse<AccountingEventReceiptDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<AccountingEventReceiptDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AccountingEventReceiptDto>), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Receive([FromBody] ReceiveAccountingEventRequest request, CancellationToken ct)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return IdentitasTidakValid();

            var hasil = await _service.TerimaAsync(request, actor, ct);

            await CatatAsync("AccountingEvent.Receive", hasil, new
            {
                request.EventNumber,
                request.EventTypeCode,
                request.SourceModule,
                hasil.StatusCode,
                EventStatus = hasil.Data?.EventStatus,
                hasil.Data?.HoldReasonCode,
                hasil.Data?.JournalNumber
            });

            return ToActionResult(hasil);
        }

        private IActionResult ToActionResult<T>(AccountingServiceResult<T> result)
        {
            if (result.Success)
            {
                var berhasil = ApiResponse<T>.Ok(result.Data, result.Message);
                berhasil.StatusCode = result.StatusCode;
                return StatusCode(result.StatusCode, berhasil);
            }

            if (result.Data is not null)
            {
                var tertahan = ApiResponse<T>.Fail(result.StatusCode, result.Message);
                tertahan.Data = result.Data;
                return StatusCode(result.StatusCode, tertahan);
            }

            return StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));
        }

        private IActionResult IdentitasTidakValid()
            => Unauthorized(ApiResponse<object>.Fail(
                StatusCodes.Status401Unauthorized, "Identitas user login tidak valid."));

        private Task CatatAsync<T>(string aksi, AccountingServiceResult<T> hasil, object muatan)
        {
            var pesan = TanpaNominal(hasil.Message);

            return hasil.Success
                ? _loggerService.InfoAsync(LogCategory, aksi, pesan, muatan)
                : _loggerService.WarningAsync(LogCategory, aksi, pesan, muatan);
        }

        private static string TanpaNominal(string pesan)
            => System.Text.RegularExpressions.Regex.Replace(
                pesan, @"Rp\s?[\d.,]+", "Rp ***");

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}
