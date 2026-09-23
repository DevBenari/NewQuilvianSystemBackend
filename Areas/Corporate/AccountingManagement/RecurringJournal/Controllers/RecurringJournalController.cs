using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.RecurringJournal.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.RecurringJournal.Services;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.RecurringJournal.Controllers
{
    /// <summary>
    /// Template jurnal berulang — penyusutan bulanan, sewa dibayar di muka, amortisasi, dan
    /// sejenisnya (<c>ACC-DEC-050</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Template baru selalu lahir tidak aktif.</b> Begitu aktif, ia menerbitkan jurnal
    /// sendiri setiap bulan tanpa ada yang menekan tombol, jadi pengaktifannya adalah tindakan
    /// tersendiri dengan hak akses tersendiri — bukan sebuah bidang pada form penambahan.
    /// </para>
    /// <para>
    /// Jurnal yang dihasilkannya lahir <c>Draft</c> dan pengesahannya tetap manual. Sistem tidak
    /// pernah mengesahkan jurnal berulang sendiri: template yang sudah tidak sesuai — misalnya
    /// aset yang sudah dijual — akan terus mencatat beban yang tidak ada, dan karena angkanya
    /// wajar tidak ada yang curiga sampai audit.
    /// </para>
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/accounting/recurring-journals")]
    [AccessController(
        moduleCode: "ACCOUNTING_JOURNAL",
        moduleName: "Accounting Journal",
        displayName: "Recurring Journal",
        AreaName = "Corporate",
        ControllerName = "RecurringJournal",
        Description = "Corporate accounting recurring journal templates",
        SortOrder = 2)]
    [Tags("Corporate / Accounting / Recurring Journal")]
    public class RecurringJournalController : ControllerBase
    {
        private const string LogCategory = "Corporate.Accounting.RecurringJournal";

        private readonly AccRecurringJournalService _service;
        private readonly LoggerService _loggerService;

        public RecurringJournalController(
            AccRecurringJournalService service,
            LoggerService loggerService)
        {
            _service = service;
            _loggerService = loggerService;
        }

        // ------------------------------------------------------------------
        // Baca
        // ------------------------------------------------------------------

        /// <summary>Daftar template berhalaman.</summary>
        [HttpGet]
        [AccessAction("Read", "Read Recurring Journal", Description = "Melihat daftar template jurnal berulang", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RecurringJournal", "Read")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] RecurringJournalPagedQuery query, CancellationToken ct)
            => ToActionResult(await _service.GetPagedAsync(query, ct));

        /// <summary>Rincian satu template beserta seluruh barisnya.</summary>
        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Recurring Journal", Description = "Melihat rincian template", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RecurringJournal", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
            => ToActionResult(await _service.GetByIdAsync(id, ct));

        /// <summary>
        /// Riwayat penerbitan template per periode beserta jurnal yang dihasilkannya.
        /// </summary>
        /// <remarks>
        /// Inilah layar yang menjawab <i>"penyusutan bulan Agustus sudah terbit belum"</i>.
        /// </remarks>
        [HttpGet("{id:guid}/runs")]
        [AccessAction("Read", "Read Recurring Journal", Description = "Melihat riwayat penerbitan template", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RecurringJournal", "Read")]
        public async Task<IActionResult> GetRuns(Guid id, CancellationToken ct)
            => ToActionResult(await _service.GetRunsAsync(id, ct));

        // ------------------------------------------------------------------
        // Tulis
        // ------------------------------------------------------------------

        /// <summary>
        /// Menambah template beserta seluruh barisnya. Template baru <b>selalu tidak aktif</b>.
        /// </summary>
        /// <remarks>
        /// Ditolak <c>400</c> bila baris kurang dari dua, tidak seimbang, berisi debit dan kredit
        /// sekaligus, atau berakun beban tanpa unit biaya. Ditolak <c>409</c> bila kode template
        /// sudah dipakai, atau ada baris berakun induk maupun berakun badan hukum lain.
        /// </remarks>
        [HttpPost]
        [AccessAction("Create", "Create Recurring Journal", Description = "Menambah template jurnal berulang", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("RecurringJournal", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateRecurringJournalRequest request, CancellationToken ct)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return IdentitasTidakValid();

            var hasil = await _service.CreateAsync(request, actor, ct);

            await CatatAsync("RecurringJournal.Create", hasil, null, actor);

            return ToActionResult(hasil);
        }

        /// <summary>Mengubah template beserta seluruh barisnya.</summary>
        /// <remarks>Baris dikirim <b>utuh</b> dan menggantikan seluruh baris sebelumnya.</remarks>
        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Recurring Journal", Description = "Mengubah template jurnal berulang", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("RecurringJournal", "Update")]
        public async Task<IActionResult> Update(
            Guid id, [FromBody] UpdateRecurringJournalRequest request, CancellationToken ct)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return IdentitasTidakValid();

            var hasil = await _service.UpdateAsync(id, request, actor, ct);

            await CatatAsync("RecurringJournal.Update", hasil, id, actor);

            return ToActionResult(hasil);
        }

        /// <summary>Mengaktifkan template sehingga penjadwal mulai menerbitkannya.</summary>
        /// <remarks>
        /// Barisnya diperiksa <b>ulang</b> di sini. Akun dapat dinonaktifkan dan unit biaya dapat
        /// dipindahkan sesudah template tersimpan; template yang diaktifkan dalam keadaan itu
        /// akan gagal terbit setiap bulan di dalam penjadwal, tempat tidak seorang pun melihatnya.
        /// </remarks>
        [HttpPatch("{id:guid}/activate")]
        [AccessAction("Activate", "Activate Recurring Journal", Description = "Mengaktifkan template jurnal berulang", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("RecurringJournal", "Activate")]
        public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return IdentitasTidakValid();

            var hasil = await _service.ActivateAsync(id, actor, ct);

            await CatatAsync("RecurringJournal.Activate", hasil, id, actor);

            return ToActionResult(hasil);
        }

        /// <summary>Menonaktifkan template. Jurnal yang sudah terbit tidak ikut dibatalkan.</summary>
        [HttpPatch("{id:guid}/deactivate")]
        [AccessAction("Activate", "Activate Recurring Journal", Description = "Menonaktifkan template jurnal berulang", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("RecurringJournal", "Activate")]
        public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return IdentitasTidakValid();

            var hasil = await _service.DeactivateAsync(id, actor, ct);

            await CatatAsync("RecurringJournal.Deactivate", hasil, id, actor);

            return ToActionResult(hasil);
        }

        /// <summary>
        /// Menerbitkan jurnal template untuk satu periode secara manual, di luar jadwal.
        /// </summary>
        /// <remarks>
        /// Ditolak <c>409</c> bila template sedang tidak aktif, sudah berakhir, barisnya tidak
        /// lagi layak, atau <b>sudah pernah terbit untuk periode itu</b>. Ditolak <c>422</c> bila
        /// periode tujuan tidak menerima pencatatan baru.
        /// </remarks>
        [HttpPost("{id:guid}/generate")]
        [AccessAction("Generate", "Generate Recurring Journal", Description = "Menerbitkan jurnal dari template di luar jadwal", AccessType = AccessTypes.Create, SortOrder = 5)]
        [AccessPermission("RecurringJournal", "Generate")]
        public async Task<IActionResult> Generate(
            Guid id, [FromBody] GenerateRecurringJournalRequest request, CancellationToken ct)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return IdentitasTidakValid();

            var hasil = await _service.GenerateAsync(id, request ?? new(), actor, ct);

            await CatatPenerbitanAsync(hasil, id, actor);

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

        /// <summary>Mencatat jejak tanpa membawa rahasia bisnis.</summary>
        /// <remarks>
        /// Muatan permintaan <b>tidak</b> ikut dicatat, mengikuti <c>JournalController</c>:
        /// <c>ACC-PERMISSION-0.3</c> bagian 4 melarang <c>DebitAmount</c>, <c>CreditAmount</c>,
        /// dan isi <c>Description</c> baris masuk payload log — dan seluruhnya ada di dalam
        /// <c>CreateRecurringJournalRequest</c>. Yang dicatat hanya identitas template, hasilnya,
        /// dan statusnya.
        /// </remarks>
        private Task CatatAsync<T>(
            string aksi, AccountingServiceResult<T> hasil, Guid? id, Guid actor)
        {
            var template = hasil.Data as RecurringJournalDetailResponse;

            var muatan = new
            {
                EntityId = template?.Id ?? id,
                template?.TemplateCode,
                IsActive = template?.IsActive,
                hasil.StatusCode,
                actor
            };

            var pesan = TanpaNominal(hasil.Message);

            return hasil.Success
                ? _loggerService.InfoAsync(LogCategory, aksi, pesan, muatan)
                : _loggerService.WarningAsync(LogCategory, aksi, pesan, muatan);
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

        /// <summary>
        /// Mencatat penerbitan tanpa membawa nominal jurnalnya.
        /// </summary>
        private Task CatatPenerbitanAsync(
            AccountingServiceResult<RecurringJournalRunResponse> hasil, Guid templateId, Guid actor)
        {
            var muatan = new
            {
                EntityId = templateId,
                RunId = hasil.Data?.Id,
                hasil.Data?.JournalId,
                hasil.Data?.JournalNumber,
                hasil.Data?.PeriodCode,
                hasil.StatusCode,
                actor
            };

            var pesan = TanpaNominal(hasil.Message);

            return hasil.Success
                ? _loggerService.InfoAsync(LogCategory, "RecurringJournal.Generate", pesan, muatan)
                : _loggerService.WarningAsync(LogCategory, "RecurringJournal.Generate", pesan, muatan);
        }

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}
