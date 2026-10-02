using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.EventType.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.EventType.Services;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.EventType.Controllers
{
    /// <summary>
    /// Master jenis kejadian keuangan (<c>BE-ACC-P2-017</c>). Seluruh aturan bisnisnya berada di
    /// <see cref="AccEventTypeService"/>; controller hanya memetakan hasilnya ke kode status HTTP.
    /// </summary>
    /// <remarks>
    /// Base URL mengikuti <c>ACC-API-0.10</c> grup Event Type apa adanya —
    /// <c>api/v1/corporate/accounting/event-types</c>, tanpa segmen <c>master-data</c>.
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/accounting/event-types")]
    [AccessController(
        moduleCode: "ACCOUNTING_MASTER_DATA",
        moduleName: "Accounting Master Data",
        displayName: "Event Type",
        AreaName = "Corporate",
        ControllerName = "EventType",
        Description = "Corporate accounting master data event type",
        SortOrder = 4)]
    [Tags("Corporate / Accounting / Master Data / Event Type")]
    public class EventTypeController : ControllerBase
    {
        private const string LogCategory = "Corporate.Accounting.MasterData";

        private readonly AccEventTypeService _service;
        private readonly LoggerService _loggerService;

        public EventTypeController(
            AccEventTypeService service,
            LoggerService loggerService)
        {
            _service = service;
            _loggerService = loggerService;
        }

        [HttpGet]
        [AccessAction("Read", "Read Event Type", Description = "Melihat daftar jenis kejadian", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EventType", "Read")]
        public async Task<IActionResult> GetPaged([FromQuery] EventTypePagedQuery query, CancellationToken ct)
            => ToActionResult(await _service.GetPagedAsync(query, ct));

        [HttpGet("options")]
        [AccessAction("Read", "Read Event Type", Description = "Melihat pilihan jenis kejadian untuk form aturan posting", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EventType", "Read")]
        public async Task<IActionResult> GetOptions([FromQuery] string? search, CancellationToken ct)
            => ToActionResult(await _service.GetOptionsAsync(search, ct));

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Event Type", Description = "Melihat rincian jenis kejadian", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EventType", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
            => ToActionResult(await _service.GetByIdAsync(id, ct));

        [HttpPost]
        [AccessAction("Create", "Create Event Type", Description = "Menambah jenis kejadian", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("EventType", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateEventTypeRequest request, CancellationToken ct)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return IdentitasTidakValid();

            var hasil = await _service.CreateAsync(request, actor, ct);
            await CatatAsync("EventType.Create", hasil, request);

            return ToActionResult(hasil);
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Event Type", Description = "Mengubah nama dan modul asal jenis kejadian", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("EventType", "Update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEventTypeRequest request, CancellationToken ct)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return IdentitasTidakValid();

            var hasil = await _service.UpdateAsync(id, request, actor, ct);
            await CatatAsync("EventType.Update", hasil, new { EntityId = id, request });

            return ToActionResult(hasil);
        }

        [HttpPatch("{id:guid}/deactivate")]
        [AccessAction("Update", "Update Event Type", Description = "Menonaktifkan jenis kejadian", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("EventType", "Update")]
        public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return IdentitasTidakValid();

            var hasil = await _service.DeactivateAsync(id, actor, ct);
            await CatatAsync("EventType.Deactivate", hasil, new { EntityId = id });

            return ToActionResult(hasil);
        }

        /// <summary>
        /// Mengaktifkan kembali jenis kejadian.
        /// </summary>
        /// <remarks>
        /// Endpoint di luar enam yang tercantum <c>ACC-API-0.10</c> grup Event Type, dicatat sebagai
        /// delta kontrak pada laporan <c>BE-ACC-P2-017</c>: tanpa pasangan <c>activate</c>, jenis
        /// yang pernah dinonaktifkan tidak akan pernah dapat dipakai lagi. Hak aksesnya sama dengan
        /// <c>deactivate</c>, mengikuti <c>ChartOfAccountController</c>.
        /// </remarks>
        [HttpPatch("{id:guid}/activate")]
        [AccessAction("Update", "Update Event Type", Description = "Mengaktifkan kembali jenis kejadian", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("EventType", "Update")]
        public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return IdentitasTidakValid();

            var hasil = await _service.ActivateAsync(id, actor, ct);
            await CatatAsync("EventType.Activate", hasil, new { EntityId = id });

            return ToActionResult(hasil);
        }

        // ------------------------------------------------------------------
        // Pembantu
        // ------------------------------------------------------------------

        private IActionResult ToActionResult<T>(AccountingServiceResult<T> result)
        {
            return result.Success
                ? StatusCode(result.StatusCode, ApiResponse<T>.Ok(result.Data, result.Message))
                : StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));
        }

        private IActionResult IdentitasTidakValid()
            => Unauthorized(ApiResponse<object>.Fail(
                StatusCodes.Status401Unauthorized, "Identitas user login tidak valid."));

        /// <summary>
        /// Master jenis kejadian tidak memuat nominal, tetapi pesannya tetap disaring supaya
        /// seluruh pencatat Accounting berperilaku sama (<c>NFR-004</c>, <c>BE-ACC-P2-033</c>).
        /// </summary>
        /// <remarks>
        /// Id jenis kejadian dikirim sebagai <c>EntityId</c>, <b>bukan</b> <c>id</c>:
        /// <c>LoggerService</c> membaca properti <c>UserId</c> atau <c>Id</c> pada muatan dan
        /// memakainya menggantikan pengguna yang login, sehingga <c>new { id }</c> membuat log
        /// mencatat id jenis kejadian sebagai pelakunya (<c>QBE-LOG-001</c>).
        /// </remarks>
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
