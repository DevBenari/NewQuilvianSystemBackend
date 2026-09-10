using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Services;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Controllers
{
    /// <summary>
    /// Tutup tahun: pratinjau perhitungan, dan penyusunan jurnal penutup tahun
    /// (<c>ACC-DEC-053</c>, <c>ACC-DEC-054</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Pengesahannya memakai endpoint yang sudah ada</b>, yaitu
    /// <c>POST /journals/{id}/post</c>. Tidak ada jalur pengesahan khusus tutup tahun di sini,
    /// karena jurnal penutup adalah jurnal biasa (<c>ACC-STATE-0.2</c> bagian 4). Pembalikannya
    /// pun memakai <c>POST /journals/{id}/reverse</c> yang sudah ada (<c>ACC-DEC-029</c>).
    /// </para>
    /// <para>
    /// <b>Kenapa controller tersendiri, bukan menumpang <c>AccountingPeriodController</c>.</b>
    /// <c>ACC-PERMISSION-0.4</c> menetapkan hak aksesnya <c>YearEndClosing : Read</c> dan
    /// <c>YearEndClosing : Generate</c>, sementara argumen pertama <c>[AccessPermission]</c>
    /// wajib sama persis dengan <c>ControllerName</c> pada <c>[AccessController]</c>. Menumpang
    /// controller periode akan memaksa hak aksesnya bernama <c>AccountingPeriod</c> — menyatukan
    /// kewenangan menutup tahun dengan kewenangan mengelola periode, padahal matriks hak akses
    /// sengaja memisahkan keduanya.
    /// </para>
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/accounting/year-end-closing")]
    [AccessController(
        moduleCode: "ACCOUNTING_PERIOD",
        moduleName: "Accounting Period",
        displayName: "Year End Closing",
        AreaName = "Corporate",
        ControllerName = "YearEndClosing",
        Description = "Corporate accounting year end closing",
        SortOrder = 4)]
    [Tags("Corporate / Accounting / Year End Closing")]
    public class YearEndClosingController : ControllerBase
    {
        private const string LogCategory = "Corporate.Accounting.YearEndClosing";

        private readonly AccYearEndClosingService _service;
        private readonly LoggerService _loggerService;

        public YearEndClosingController(
            AccYearEndClosingService service,
            LoggerService loggerService)
        {
            _service = service;
            _loggerService = loggerService;
        }

        /// <summary>
        /// Pratinjau perhitungan tutup tahun: saldo tiap akun pendapatan dan beban, serta selisih
        /// yang akan masuk laba ditahan.
        /// </summary>
        /// <remarks>
        /// <b>Tidak membuat apa pun.</b> Dipanggil berapa kali pun, nol jurnal terbentuk dan nol
        /// nomor jurnal terpakai. Ditolak <c>409</c> bila masih ada periode tahun itu yang belum
        /// ditutup, dan <c>422</c> bila akun laba ditahan belum ditetapkan atau tidak ada saldo
        /// yang perlu ditutup.
        /// </remarks>
        [HttpGet("preview")]
        [AccessAction("Read", "Read Year End Closing", Description = "Melihat pratinjau tutup tahun", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("YearEndClosing", "Read")]
        public async Task<IActionResult> Preview(
            [FromQuery] Guid legalEntityId,
            [FromQuery] int fiscalYear,
            CancellationToken ct)
            => ToActionResult(await _service.PreviewAsync(legalEntityId, fiscalYear, ct));

        /// <summary>
        /// Menyusun jurnal penutup tahun berstatus <c>Draft</c> berjenis <c>JT</c>.
        /// </summary>
        /// <remarks>
        /// Ditolak <c>409</c> bila masih ada periode tahun itu yang belum ditutup, atau bila
        /// jurnal penutup tahun itu sudah pernah disusun. Ditolak <c>422</c> bila akun laba
        /// ditahan belum ditetapkan, akunnya tidak lagi layak, atau tidak ada saldo yang perlu
        /// ditutup.
        /// </remarks>
        [HttpPost("generate")]
        [AccessAction("Generate", "Generate Year End Closing", Description = "Menyusun jurnal penutup tahun", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("YearEndClosing", "Generate")]
        public async Task<IActionResult> Generate(
            [FromBody] GenerateYearEndClosingRequest request,
            CancellationToken ct)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return IdentitasTidakValid();

            var hasil = await _service.GenerateAsync(request, actor, ct);

            await CatatAsync("YearEndClosing.Generate", hasil, request, actor);

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
        /// Mencatat jejak tanpa membawa rahasia bisnis.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Pesan keberhasilan sengaja tidak dicatat apa adanya.</b> Kalimat yang dikembalikan
        /// ke layar menyebut nilai laba atau rugi tahun itu — angka yang paling rahasia di
        /// seluruh modul — sementara <c>ACC-PERMISSION-0.3</c> bagian 4 melarang nominal masuk
        /// log. Karena itu yang dicatat adalah kalimat tetap beserta identitas jurnalnya, persis
        /// pola <c>JournalController</c>.
        /// </para>
        /// <para>
        /// Pesan <b>kegagalan</b> dicatat apa adanya: seluruhnya menyebut periode, nomor jurnal,
        /// atau pengaturan yang keliru, dan tidak satu pun memuat nominal.
        /// </para>
        /// </remarks>
        private Task CatatAsync<T>(
            string aksi,
            AccountingServiceResult<T> hasil,
            GenerateYearEndClosingRequest request,
            Guid actor)
        {
            var jurnal = hasil.Data as JournalDetailResponse;

            var muatan = new
            {
                request.LegalEntityId,
                request.FiscalYear,
                EntityId = jurnal?.Id,
                jurnal?.JournalNumber,
                JournalStatus = jurnal?.JournalStatus,
                hasil.StatusCode,
                actor
            };

            return hasil.Success
                ? _loggerService.InfoAsync(
                    LogCategory,
                    aksi,
                    $"Jurnal penutup tahun buku {request.FiscalYear} berhasil disusun sebagai draft.",
                    muatan)
                : _loggerService.WarningAsync(LogCategory, aksi, TanpaNominal(hasil.Message), muatan);
        }

        /// <summary>
        /// Menghapus nominal dari kalimat yang hendak dicatat logger.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>ACC-PERMISSION-0.4</c> bagian 4 melarang <c>DebitAmount</c>, <c>CreditAmount</c>,
        /// <c>TotalDebit</c>, dan <c>TotalCredit</c> masuk catatan logger. Sebagian pesan
        /// penolakan justru menyebut nominal supaya berguna bagi petugas — misalnya
        /// <i>"selisih Rp 250.000"</i> — dan pesan itulah yang diteruskan ke logger.
        /// </para>
        /// <para>
        /// Yang dibuang hanya angkanya; kalimatnya tetap utuh sehingga sebab kegagalan masih
        /// dapat ditelusuri dari log. Controller lama meneruskan pesannya apa adanya, dan itu
        /// dicatat sebagai temuan di luar cakupan — bukan alasan untuk menirunya pada kode baru.
        /// </para>
        /// </remarks>
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
