using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Services;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Controllers
{
    /// <summary>
    /// Periode akuntansi: dibangkitkan setahun sekaligus, ditutup bertahap, dan dibuka kembali
    /// dengan alasan tertulis.
    /// </summary>
    /// <remarks>
    /// Penutupan dan pembukaan kembali memakai hak akses tersendiri — <c>Close</c> dan
    /// <c>Reopen</c> — bukan <c>Update</c>. `ACC-DEC-026` membatasi keduanya pada Manajer
    /// Akuntansi, dan memisahkannya membuat pembatasan itu dapat ditegakkan matriks hak akses
    /// tanpa pemeriksaan tambahan di kode.
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/accounting/periods")]
    [AccessController(
        moduleCode: "ACCOUNTING_PERIOD",
        moduleName: "Accounting Period",
        displayName: "Accounting Period",
        AreaName = "Corporate",
        ControllerName = "AccountingPeriod",
        Description = "Corporate accounting period lifecycle",
        SortOrder = 3)]
    [Tags("Corporate / Accounting / Accounting Period")]
    public class AccountingPeriodController : ControllerBase
    {
        private const string LogCategory = "Corporate.Accounting.AccountingPeriod";

        private readonly AccAccountingPeriodService _service;
        private readonly AccPeriodClosingService _closingService;
        private readonly LoggerService _loggerService;

        public AccountingPeriodController(
            AccAccountingPeriodService service,
            AccPeriodClosingService closingService,
            LoggerService loggerService)
        {
            _service = service;
            _closingService = closingService;
            _loggerService = loggerService;
        }

        [HttpGet]
        [AccessAction("Read", "Read Accounting Period", Description = "Melihat daftar periode akuntansi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AccountingPeriod", "Read")]
        public async Task<IActionResult> GetPaged([FromQuery] AccountingPeriodPagedQuery query, CancellationToken ct)
            => ToActionResult(await _service.GetPagedAsync(query, ct));

        [HttpGet("current")]
        [AccessAction("Read", "Read Accounting Period", Description = "Melihat periode berjalan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AccountingPeriod", "Read")]
        public async Task<IActionResult> GetCurrent([FromQuery] Guid legalEntityId, CancellationToken ct)
            => ToActionResult(await _service.GetCurrentAsync(legalEntityId, ct));

        [HttpPost("generate")]
        [AccessAction("Create", "Create Accounting Period", Description = "Membangkitkan periode satu tahun buku", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("AccountingPeriod", "Create")]
        public async Task<IActionResult> Generate([FromBody] GenerateAccountingPeriodRequest request, CancellationToken ct)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return IdentitasTidakValid();

            var hasil = await _service.GenerateAsync(request, actor, ct);
            await CatatAsync("AccountingPeriod.Generate", hasil, request);

            return ToActionResult(hasil);
        }

        [HttpPost("{id:guid}/close")]
        [AccessAction("Close", "Close Accounting Period", Description = "Menutup periode akuntansi", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("AccountingPeriod", "Close")]
        public async Task<IActionResult> Close(Guid id, [FromBody] ClosePeriodRequest request, CancellationToken ct)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return IdentitasTidakValid();

            var hasil = await _service.CloseAsync(id, request, actor, ct);
            await CatatAsync("AccountingPeriod.Close", hasil, new { id, request });

            return ToActionResult(hasil);
        }

        [HttpPost("{id:guid}/reopen")]
        [AccessAction("Reopen", "Reopen Accounting Period", Description = "Membuka kembali periode akuntansi", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("AccountingPeriod", "Reopen")]
        public async Task<IActionResult> Reopen(Guid id, [FromBody] ReopenPeriodRequest request, CancellationToken ct)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return IdentitasTidakValid();

            var hasil = await _service.ReopenAsync(id, request, actor, ct);

            // Alasan pembukaan kembali wajib tercatat di jejak audit — bagian dari DoD task ini.
            await CatatAsync("AccountingPeriod.Reopen", hasil, new { id, request.Reason, actor });

            return ToActionResult(hasil);
        }

        /// <summary>
        /// Daftar periksa penutupan periode: apa yang menahan, dan apa yang hanya perlu
        /// diperhatikan.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Dihitung saat diminta, tidak pernah disimpan</b> (`ACC-DEC-051`). Dua panggilan
        /// berturut-turut menghasilkan angka berbeda bila ada jurnal yang disahkan di antaranya,
        /// dan itu memang yang dikehendaki.
        /// </para>
        /// <para>
        /// Memakai hak akses <c>Read</c>, bukan <c>Close</c>: membaca daftar periksa tidak
        /// mengubah apa pun, dan Manajer Akuntansi perlu melihatnya jauh sebelum berhak menutup.
        /// </para>
        /// </remarks>
        [HttpGet("{id:guid}/closing-checklist")]
        [AccessAction("Read", "Read Accounting Period", Description = "Melihat daftar periksa penutupan periode", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AccountingPeriod", "Read")]
        public async Task<IActionResult> GetClosingChecklist(Guid id, CancellationToken ct)
            => ToActionResult(await _closingService.GetChecklistAsync(id, ct));

        /// <summary>
        /// Riwayat penutupan periode: siapa mengajukan, siapa menyetujui atau menolak, dan kapan.
        /// </summary>
        /// <remarks>
        /// Daftar kosong adalah jawaban yang sah untuk periode yang ditutup sebelum Phase 2
        /// berdiri — mereka ditutup ketika aturan persetujuannya memang belum ada.
        /// </remarks>
        [HttpGet("{id:guid}/closing-history")]
        [AccessAction("Read", "Read Accounting Period", Description = "Melihat riwayat penutupan periode", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AccountingPeriod", "Read")]
        public async Task<IActionResult> GetClosingHistory(Guid id, CancellationToken ct)
            => ToActionResult(await _closingService.GetClosingHistoryAsync(id, ct));

        /// <summary>
        /// Mengajukan penutupan periode. Periode menjadi menunggu persetujuan.
        /// </summary>
        /// <remarks>
        /// Memakai hak akses <c>Close</c> yang sudah ada — mengajukan penutupan adalah wewenang
        /// Manajer Akuntansi yang sama, hanya alurnya kini melewati orang kedua
        /// (<c>ACC-DEC-052</c>).
        /// </remarks>
        [HttpPost("{id:guid}/submit-closing")]
        [AccessAction("Close", "Close Accounting Period", Description = "Mengajukan penutupan periode", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("AccountingPeriod", "Close")]
        public async Task<IActionResult> SubmitClosing(Guid id, [FromBody] SubmitPeriodClosingRequest request, CancellationToken ct)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return IdentitasTidakValid();

            var hasil = await _closingService.SubmitClosingAsync(id, request, actor, ct);
            await CatatAsync("AccountingPeriod.SubmitClosing", hasil, new { id, request, actor });

            return ToActionResult(hasil);
        }

        /// <summary>
        /// Menyetujui penutupan periode. Periode menjadi tutup sementara.
        /// </summary>
        /// <remarks>
        /// Hak akses <c>Approve</c> <b>terpisah</b> dari <c>Close</c>, dan itulah yang membuat
        /// prinsip empat mata dapat ditegakkan lewat matriks hak akses: peran yang boleh
        /// mengajukan tidak perlu diberi hak menyetujui. Pemeriksaan "penyetuju bukan pengaju"
        /// tetap dilakukan backend, tidak diserahkan ke pengaturan peran.
        /// </remarks>
        [HttpPost("{id:guid}/approve-closing")]
        [AccessAction("Approve", "Approve Accounting Period Closing", Description = "Menyetujui penutupan periode", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("AccountingPeriod", "Approve")]
        public async Task<IActionResult> ApproveClosing(Guid id, [FromBody] ApprovePeriodClosingRequest request, CancellationToken ct)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return IdentitasTidakValid();

            var hasil = await _closingService.ApproveClosingAsync(id, request, actor, ct);
            await CatatAsync("AccountingPeriod.ApproveClosing", hasil, new { id, request, actor });

            return ToActionResult(hasil);
        }

        /// <summary>
        /// Menolak penutupan periode. Periode kembali terbuka, dan alasan tertulis wajib.
        /// </summary>
        [HttpPost("{id:guid}/reject-closing")]
        [AccessAction("Approve", "Approve Accounting Period Closing", Description = "Menolak penutupan periode", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("AccountingPeriod", "Approve")]
        public async Task<IActionResult> RejectClosing(Guid id, [FromBody] RejectPeriodClosingRequest request, CancellationToken ct)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return IdentitasTidakValid();

            var hasil = await _closingService.RejectClosingAsync(id, request, actor, ct);

            // Alasan penolakan wajib tercatat di jejak audit.
            await CatatAsync("AccountingPeriod.RejectClosing", hasil, new { id, request.Reason, actor });

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
