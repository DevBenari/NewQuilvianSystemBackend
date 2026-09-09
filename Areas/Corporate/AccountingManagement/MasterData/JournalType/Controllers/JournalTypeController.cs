using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.JournalType.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.JournalType.Services;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.JournalType.Controllers
{
    /// <summary>
    /// Jenis jurnal beserta awalan nomornya. Seluruh aturan bisnis berada di
    /// <see cref="AccJournalTypeService"/>.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/accounting/master-data/journal-types")]
    [AccessController(
        moduleCode: "ACCOUNTING_MASTER_DATA",
        moduleName: "Accounting Master Data",
        displayName: "Journal Type",
        AreaName = "Corporate",
        ControllerName = "JournalType",
        Description = "Corporate accounting master data journal type",
        SortOrder = 2)]
    [Tags("Corporate / Accounting / Master Data / Journal Type")]
    public class JournalTypeController : ControllerBase
    {
        private const string LogCategory = "Corporate.Accounting.MasterData";

        private readonly AccJournalTypeService _service;
        private readonly LoggerService _loggerService;

        public JournalTypeController(
            AccJournalTypeService service,
            LoggerService loggerService)
        {
            _service = service;
            _loggerService = loggerService;
        }

        [HttpGet]
        [AccessAction("Read", "Read Journal Type", Description = "Melihat daftar jenis jurnal", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("JournalType", "Read")]
        public async Task<IActionResult> GetPaged([FromQuery] JournalTypePagedQuery query, CancellationToken ct)
            => ToActionResult(await _service.GetPagedAsync(query, ct));

        [HttpGet("options")]
        [AccessAction("Read", "Read Journal Type", Description = "Melihat pilihan jenis jurnal", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("JournalType", "Read")]
        public async Task<IActionResult> GetOptions(CancellationToken ct)
            => ToActionResult(await _service.GetOptionsAsync(ct));

        [HttpPost]
        [AccessAction("Create", "Create Journal Type", Description = "Menambah jenis jurnal", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("JournalType", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateJournalTypeRequest request, CancellationToken ct)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return IdentitasTidakValid();

            var hasil = await _service.CreateAsync(request, actor, ct);
            await CatatAsync("JournalType.Create", hasil, request);

            return ToActionResult(hasil);
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Journal Type", Description = "Mengubah jenis jurnal", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("JournalType", "Update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateJournalTypeRequest request, CancellationToken ct)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return IdentitasTidakValid();

            var hasil = await _service.UpdateAsync(id, request, actor, ct);
            await CatatAsync("JournalType.Update", hasil, new { id, request });

            return ToActionResult(hasil);
        }

        /// <summary>
        /// Mengisi empat jenis jurnal bawaan sesuai `02-backend-architecture.md` bagian 9.1.
        /// </summary>
        /// <remarks>
        /// Endpoint kelima, **di luar empat yang tercantum `ACC-API-0.2`**. Ia adalah call site
        /// seeder `BE-ACC-006` yang selama ini belum ada (`ACC-TD-004`), dan dilaporkan sebagai
        /// delta kontrak pada laporan task bagian 7 — menunggu ratifikasi owner.
        ///
        /// Memakai hak akses `JournalType : Create` karena akibatnya memang menambah baris master.
        /// Aman dipanggil berulang.
        /// </remarks>
        [HttpPost("seed")]
        [AccessAction("Create", "Create Journal Type", Description = "Mengisi jenis jurnal bawaan", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("JournalType", "Create")]
        public async Task<IActionResult> Seed(CancellationToken ct)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return IdentitasTidakValid();

            var hasil = await _service.SeedAsync(actor, ct);
            await CatatAsync("JournalType.Seed", hasil, new { actor });

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

        private Task CatatAsync<T>(string aksi, AccountingServiceResult<T> hasil, object muatan)
        {
            return hasil.Success
                ? _loggerService.InfoAsync(LogCategory, aksi, hasil.Message, muatan)
                : _loggerService.WarningAsync(LogCategory, aksi, hasil.Message, muatan);
        }

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}
