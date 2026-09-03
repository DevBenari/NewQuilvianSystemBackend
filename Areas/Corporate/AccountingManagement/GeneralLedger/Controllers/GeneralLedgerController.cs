using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.GeneralLedger.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.GeneralLedger.Services;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.GeneralLedger.Controllers
{
    /// <summary>
    /// Buku besar, neraca saldo, dan saldo per akun. Seluruhnya baca saja.
    /// </summary>
    /// <remarks>
    /// Ketiga endpoint hanya menghitung jurnal berstatus disahkan. Laporan tidak pernah mencampur
    /// yang sudah dan belum disahkan.
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/accounting/general-ledger")]
    [AccessController(
        moduleCode: "ACCOUNTING_GENERAL_LEDGER",
        moduleName: "Accounting General Ledger",
        displayName: "General Ledger",
        AreaName = "Corporate",
        ControllerName = "GeneralLedger",
        Description = "Corporate accounting general ledger and trial balance",
        SortOrder = 1)]
    [Tags("Corporate / Accounting / General Ledger")]
    public class GeneralLedgerController : ControllerBase
    {
        private const string LogCategory = "Corporate.Accounting.GeneralLedger";

        private readonly AccGeneralLedgerService _service;
        private readonly LoggerService _loggerService;

        public GeneralLedgerController(
            AccGeneralLedgerService service,
            LoggerService loggerService)
        {
            _service = service;
            _loggerService = loggerService;
        }

        [HttpGet("movements")]
        [AccessAction("Read", "Read General Ledger", Description = "Melihat mutasi buku besar", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("GeneralLedger", "Read")]
        public async Task<IActionResult> GetMovements([FromQuery] LedgerMovementQuery query, CancellationToken ct)
            => ToActionResult(await _service.GetMovementsAsync(query, ct));

        /// <remarks>
        /// <b>Satu-satunya endpoint grup ini yang dicatat logger</b>, sesuai `ACC-PERMISSION-0.3`
        /// bagian 3. Neraca saldo adalah laporan yang dipakai menutup buku, sehingga siapa
        /// membacanya dan kapan menjadi pertanyaan audit. Dua endpoint lain adalah penelusuran
        /// sehari-hari; mencatat semuanya menghasilkan ratusan baris yang justru menyulitkan
        /// penelusuran saat benar-benar dibutuhkan.
        ///
        /// Muatan log sengaja tidak memuat satu pun angka — `ACC-PERMISSION-0.3` bagian 4
        /// melarang nilai uang masuk payload log.
        /// </remarks>
        [HttpGet("trial-balance")]
        [AccessAction("Read", "Read General Ledger", Description = "Melihat neraca saldo", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("GeneralLedger", "Read")]
        public async Task<IActionResult> GetTrialBalance([FromQuery] TrialBalanceQuery query, CancellationToken ct)
        {
            var hasil = await _service.GetTrialBalanceAsync(query, ct);

            var muatan = new
            {
                query.LegalEntityId,
                query.PeriodCode,
                hasil.StatusCode,
                JumlahBaris = hasil.Data?.Rows.Count
            };

            if (hasil.Success)
            {
                await _loggerService.InfoAsync(
                    LogCategory, "GeneralLedger.TrialBalance", hasil.Message, muatan);
            }
            else
            {
                await _loggerService.WarningAsync(
                    LogCategory, "GeneralLedger.TrialBalance", hasil.Message, muatan);
            }

            return ToActionResult(hasil);
        }

        [HttpGet("account-balance/{accountId:guid}")]
        [AccessAction("Read", "Read General Ledger", Description = "Melihat saldo satu akun", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("GeneralLedger", "Read")]
        public async Task<IActionResult> GetAccountBalance(
            Guid accountId,
            [FromQuery] string? periodCode,
            CancellationToken ct)
            => ToActionResult(await _service.GetAccountBalanceAsync(accountId, periodCode, ct));

        // ------------------------------------------------------------------
        // Pembantu
        // ------------------------------------------------------------------

        private IActionResult ToActionResult<T>(AccountingServiceResult<T> result)
        {
            return result.Success
                ? StatusCode(result.StatusCode, ApiResponse<T>.Ok(result.Data, result.Message))
                : StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));
        }
    }
}
