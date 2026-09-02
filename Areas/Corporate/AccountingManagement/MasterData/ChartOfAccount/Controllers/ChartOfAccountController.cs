using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Services;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Controllers
{
    /// <summary>
    /// Daftar akun per badan hukum. Seluruh aturan bisnisnya berada di
    /// <see cref="AccChartOfAccountService"/>; controller hanya memetakan hasilnya ke kode
    /// status HTTP.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/accounting/master-data/chart-of-accounts")]
    [AccessController(
        moduleCode: "ACCOUNTING_MASTER_DATA",
        moduleName: "Accounting Master Data",
        displayName: "Chart of Account",
        AreaName = "Corporate",
        ControllerName = "ChartOfAccount",
        Description = "Corporate accounting master data chart of account",
        SortOrder = 1)]
    [Tags("Corporate / Accounting / Master Data / Chart of Account")]
    public class ChartOfAccountController : ControllerBase
    {
        private const string LogCategory = "Corporate.Accounting.MasterData";

        private readonly AccChartOfAccountService _service;
        private readonly LoggerService _loggerService;

        public ChartOfAccountController(
            AccChartOfAccountService service,
            LoggerService loggerService)
        {
            _service = service;
            _loggerService = loggerService;
        }

        [HttpGet]
        [AccessAction("Read", "Read Chart of Account", Description = "Melihat daftar akun", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ChartOfAccount", "Read")]
        public async Task<IActionResult> GetPaged([FromQuery] ChartOfAccountPagedQuery query, CancellationToken ct)
            => ToActionResult(await _service.GetPagedAsync(query, ct));

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Chart of Account", Description = "Melihat rincian akun", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ChartOfAccount", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
            => ToActionResult(await _service.GetByIdAsync(id, ct));

        [HttpGet("tree")]
        [AccessAction("Read", "Read Chart of Account", Description = "Melihat susunan induk dan anak akun", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ChartOfAccount", "Read")]
        public async Task<IActionResult> GetTree([FromQuery] Guid legalEntityId, CancellationToken ct)
            => ToActionResult(await _service.GetTreeAsync(legalEntityId, ct));

        [HttpGet("options")]
        [AccessAction("Read", "Read Chart of Account", Description = "Melihat pilihan akun untuk form jurnal", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ChartOfAccount", "Read")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] Guid legalEntityId,
            [FromQuery] string? search,
            CancellationToken ct)
            => ToActionResult(await _service.GetOptionsAsync(legalEntityId, search, ct));

        [HttpPost]
        [AccessAction("Create", "Create Chart of Account", Description = "Menambah akun", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("ChartOfAccount", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateChartOfAccountRequest request, CancellationToken ct)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return IdentitasTidakValid();

            var hasil = await _service.CreateAsync(request, actor, ct);

            await CatatAsync("ChartOfAccount.Create", hasil, request);

            return ToActionResult(hasil);
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Chart of Account", Description = "Mengubah akun", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("ChartOfAccount", "Update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateChartOfAccountRequest request, CancellationToken ct)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return IdentitasTidakValid();

            var hasil = await _service.UpdateAsync(id, request, actor, ct);

            await CatatAsync("ChartOfAccount.Update", hasil, new { id, request });

            return ToActionResult(hasil);
        }

        [HttpPatch("{id:guid}/deactivate")]
        [AccessAction("Update", "Update Chart of Account", Description = "Menonaktifkan akun", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("ChartOfAccount", "Update")]
        public async Task<IActionResult> Deactivate(
            Guid id,
            [FromBody] DeactivateChartOfAccountRequest request,
            CancellationToken ct)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return IdentitasTidakValid();

            var hasil = await _service.DeactivateAsync(id, request, actor, ct);

            await CatatAsync("ChartOfAccount.Deactivate", hasil, new { id, request });

            return ToActionResult(hasil);
        }

        [HttpPatch("{id:guid}/activate")]
        [AccessAction("Update", "Update Chart of Account", Description = "Mengaktifkan kembali akun", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("ChartOfAccount", "Update")]
        public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return IdentitasTidakValid();

            var hasil = await _service.ActivateAsync(id, actor, ct);

            await CatatAsync("ChartOfAccount.Activate", hasil, new { id });

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
