using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.Configuration.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.Configuration.Services;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.Configuration.Controllers
{
    /// <summary>
    /// Pengaturan akuntansi per badan hukum. Untuk sekarang isinya satu hal: akun laba ditahan
    /// yang dituju jurnal penutup tahun (<c>ACC-DEC-054</c>).
    /// </summary>
    /// <remarks>
    /// Memakai <c>Update</c>, bukan <c>Create</c>: satu badan hukum hanya punya satu pengaturan,
    /// sehingga menetapkannya selalu berupa penetapan, bukan penambahan baris baru.
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/accounting/configuration")]
    [AccessController(
        moduleCode: "ACCOUNTING_MASTER_DATA",
        moduleName: "Accounting Master Data",
        displayName: "Accounting Configuration",
        AreaName = "Corporate",
        ControllerName = "AccountingConfiguration",
        Description = "Corporate accounting configuration per legal entity",
        SortOrder = 3)]
    [Tags("Corporate / Accounting / Master Data / Configuration")]
    public class AccountingConfigurationController : ControllerBase
    {
        private const string LogCategory = "Corporate.Accounting.MasterData";

        private readonly AccAccountingConfigurationService _service;
        private readonly LoggerService _loggerService;

        public AccountingConfigurationController(
            AccAccountingConfigurationService service,
            LoggerService loggerService)
        {
            _service = service;
            _loggerService = loggerService;
        }

        /// <summary>
        /// Menampilkan pengaturan akuntansi satu badan hukum.
        /// </summary>
        /// <remarks>
        /// Badan hukum yang belum punya pengaturan tetap menjawab <c>200</c> dengan
        /// <c>isConfigured</c> bernilai salah — belum diisi bukan kegagalan.
        /// </remarks>
        [HttpGet("{legalEntityId:guid}")]
        [AccessAction("Read", "Read Accounting Configuration", Description = "Melihat pengaturan akuntansi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AccountingConfiguration", "Read")]
        public async Task<IActionResult> Get(Guid legalEntityId, CancellationToken ct)
            => ToActionResult(await _service.GetAsync(legalEntityId, ct));

        /// <summary>
        /// Menetapkan akun laba ditahan sebuah badan hukum.
        /// </summary>
        /// <remarks>
        /// Ditolak <c>422</c> bila akun yang ditunjuk bukan berjenis Ekuitas, bukan akun yang
        /// menerima transaksi, milik badan hukum lain, atau sudah tidak aktif.
        /// </remarks>
        [HttpPut("{legalEntityId:guid}")]
        [AccessAction("Update", "Update Accounting Configuration", Description = "Menetapkan akun laba ditahan", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("AccountingConfiguration", "Update")]
        public async Task<IActionResult> Set(
            Guid legalEntityId,
            [FromBody] UpdateAccountingConfigurationRequest request,
            CancellationToken ct)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return IdentitasTidakValid();

            var hasil = await _service.SetAsync(legalEntityId, request, actor, ct);
            await CatatAsync("AccountingConfiguration.Set", hasil, new { legalEntityId, request, actor });

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
