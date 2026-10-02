using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.PostingRule.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.PostingRule.Services;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.PostingRule.Controllers
{
    /// <summary>
    /// Aturan posting berbaris (<c>BE-ACC-P2-018</c>). Seluruh aturan bisnisnya berada di
    /// <see cref="AccPostingRuleService"/>; controller hanya memetakan hasilnya ke kode status HTTP.
    /// </summary>
    /// <remarks>
    /// Base URL mengikuti <c>ACC-API-0.10</c> grup Posting Rule apa adanya —
    /// <c>api/v1/corporate/accounting/posting-rules</c>. Tidak ada <c>activate</c>: aturan yang
    /// dinonaktifkan disimpan sebagai riwayat, dan pemetaan baru dibuat sebagai aturan baru
    /// (kamus data bagian 12).
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/accounting/posting-rules")]
    [AccessController(
        moduleCode: "ACCOUNTING_MASTER_DATA",
        moduleName: "Accounting Master Data",
        displayName: "Posting Rule",
        AreaName = "Corporate",
        ControllerName = "PostingRule",
        Description = "Corporate accounting master data posting rule",
        SortOrder = 5)]
    [Tags("Corporate / Accounting / Master Data / Posting Rule")]
    public class PostingRuleController : ControllerBase
    {
        private const string LogCategory = "Corporate.Accounting.MasterData";

        private readonly AccPostingRuleService _service;
        private readonly LoggerService _loggerService;

        public PostingRuleController(
            AccPostingRuleService service,
            LoggerService loggerService)
        {
            _service = service;
            _loggerService = loggerService;
        }

        [HttpGet]
        [AccessAction("Read", "Read Posting Rule", Description = "Melihat daftar aturan posting", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PostingRule", "Read")]
        public async Task<IActionResult> GetPaged([FromQuery] PostingRulePagedQuery query, CancellationToken ct)
            => ToActionResult(await _service.GetPagedAsync(query, ct));

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Posting Rule", Description = "Melihat rincian aturan posting", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PostingRule", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
            => ToActionResult(await _service.GetByIdAsync(id, ct));

        [HttpPost]
        [AccessAction("Create", "Create Posting Rule", Description = "Menambah aturan posting", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("PostingRule", "Create")]
        public async Task<IActionResult> Create([FromBody] CreatePostingRuleRequest request, CancellationToken ct)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return IdentitasTidakValid();

            var hasil = await _service.CreateAsync(request, actor, ct);
            await CatatAsync("PostingRule.Create", hasil, null, actor);

            return ToActionResult(hasil);
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Posting Rule", Description = "Mengubah aturan posting", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("PostingRule", "Update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePostingRuleRequest request, CancellationToken ct)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return IdentitasTidakValid();

            var hasil = await _service.UpdateAsync(id, request, actor, ct);
            await CatatAsync("PostingRule.Update", hasil, id, actor);

            return ToActionResult(hasil);
        }

        [HttpPatch("{id:guid}/deactivate")]
        [AccessAction("Update", "Update Posting Rule", Description = "Menonaktifkan aturan posting", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("PostingRule", "Update")]
        public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return IdentitasTidakValid();

            var hasil = await _service.DeactivateAsync(id, actor, ct);
            await CatatAsync("PostingRule.Deactivate", hasil, id, actor);

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
        /// Mencatat identitas aturan, hasil, dan pelakunya — tanpa isi baris dan tanpa nominal
        /// (<c>NFR-004</c>, <c>BE-ACC-P2-033</c>).
        /// </summary>
        private Task CatatAsync<T>(string aksi, AccountingServiceResult<T> hasil, Guid? id, Guid actor)
        {
            var aturan = hasil.Data as PostingRuleDetailResponse;

            var muatan = new
            {
                EntityId = aturan?.Id ?? id,
                aturan?.EventTypeCode,
                aturan?.LegalEntityId,
                IsActive = aturan?.IsActive,
                hasil.StatusCode,
                actor
            };

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
