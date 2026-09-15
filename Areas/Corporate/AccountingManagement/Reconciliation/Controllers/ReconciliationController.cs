using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.Reconciliation.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.Reconciliation.Services;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.Reconciliation.Controllers
{
    /// <summary>
    /// Rekonsiliasi control account: membandingkan saldo buku besar dengan catatan rincinya
    /// (<c>ACC-DEC-066</c>).
    /// </summary>
    /// <remarks>
    /// Untuk sekarang hanya sisi buku besarnya yang tersedia. Pembandingan dengan saldo subledger
    /// adalah <c>BE-ACC-P2-014</c>, yang masih terblokir <c>DEC-ACC-P2-011</c>.
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/accounting/reconciliation")]
    [AccessController(
        moduleCode: "ACCOUNTING_RECONCILIATION",
        moduleName: "Accounting Reconciliation",
        displayName: "Control Account Reconciliation",
        AreaName = "Corporate",
        ControllerName = "AccountingReconciliation",
        Description = "Corporate accounting control account reconciliation",
        SortOrder = 5)]
    [Tags("Corporate / Accounting / Reconciliation")]
    public class ReconciliationController : ControllerBase
    {
        private readonly AccControlAccountReconciliationService _service;

        public ReconciliationController(AccControlAccountReconciliationService service)
        {
            _service = service;
        }

        /// <summary>
        /// Saldo buku besar seluruh control account sebuah badan hukum.
        /// </summary>
        /// <remarks>
        /// Dihitung <b>hanya dari baris jurnal berstatus <c>Posted</c></b>. Menghitung dari
        /// status lain menghasilkan saldo yang tidak akan pernah cocok dengan subledger, dan
        /// selisihnya akan disalahartikan sebagai cacat data.
        ///
        /// Hanya membaca; nol perubahan data, sehingga memakai hak akses <c>Read</c>.
        /// </remarks>
        [HttpGet("gl-balances")]
        [AccessAction("Read", "Read Control Account Reconciliation", Description = "Melihat saldo buku besar control account", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AccountingReconciliation", "Read")]
        public async Task<IActionResult> GetGlBalances(
            [FromQuery] ControlAccountBalanceQuery query,
            CancellationToken ct)
            => ToActionResult(await _service.GetGlBalancesAsync(query, ct));

        private IActionResult ToActionResult<T>(AccountingServiceResult<T> result)
        {
            return result.Success
                ? StatusCode(result.StatusCode, ApiResponse<T>.Ok(result.Data, result.Message))
                : StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));
        }
    }
}
