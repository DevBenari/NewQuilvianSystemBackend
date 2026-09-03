using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Services.Security;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Controllers
{
    /// <summary>
    /// Jurnal beserta barisnya. Seluruh aturan bisnisnya berada di
    /// <see cref="AccJournalService"/>; controller hanya memetakan hasilnya ke kode status HTTP.
    /// </summary>
    /// <remarks>
    /// `BE-ACC-010` mengisi lima endpoint CRUD grup Journal pada `ACC-API-0.2`; `BE-ACC-011`
    /// menambahkan <c>submit</c>, <c>approve</c>, <c>reject</c>, dan <c>post</c>. Endpoint
    /// <c>reverse</c> adalah `BE-ACC-013`.
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/accounting/journals")]
    [AccessController(
        moduleCode: "ACCOUNTING_JOURNAL",
        moduleName: "Accounting Journal",
        displayName: "Journal",
        AreaName = "Corporate",
        ControllerName = "Journal",
        Description = "Corporate accounting journal management",
        SortOrder = 1)]
    [Tags("Corporate / Accounting / Journal Management / Journal")]
    public class JournalController : ControllerBase
    {
        private const string LogCategory = "Corporate.Accounting.Journal";

        private readonly AccJournalService _service;
        private readonly LoggerService _loggerService;
        private readonly AccessPermissionService _accessPermissionService;

        public JournalController(
            AccJournalService service,
            LoggerService loggerService,
            AccessPermissionService accessPermissionService)
        {
            _service = service;
            _loggerService = loggerService;
            _accessPermissionService = accessPermissionService;
        }

        [HttpGet]
        [AccessAction("Read", "Read Journal", Description = "Mencari jurnal", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Journal", "Read")]
        public async Task<IActionResult> GetPaged([FromQuery] JournalPagedQuery query, CancellationToken ct)
            => ToActionResult(await _service.GetPagedAsync(query, ct));

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Journal", Description = "Melihat rincian jurnal", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Journal", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
            => ToActionResult(await _service.GetByIdAsync(
                id, GetCurrentUserId(), await AmbilIzinAsync(), ct));

        [HttpPost]
        [AccessAction("Create", "Create Journal", Description = "Membuat jurnal draft", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("Journal", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateJournalRequest request, CancellationToken ct)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return IdentitasTidakValid();

            var hasil = await _service.CreateAsync(request, actor, await AmbilIzinAsync(), ct);

            await CatatAsync("Journal.Create", hasil);

            return ToActionResult(hasil);
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Journal", Description = "Mengubah jurnal draft", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("Journal", "Update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateJournalRequest request, CancellationToken ct)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return IdentitasTidakValid();

            var hasil = await _service.UpdateAsync(id, request, actor, await AmbilIzinAsync(), ct);

            await CatatAsync("Journal.Update", hasil, id);

            return ToActionResult(hasil);
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete Journal", Description = "Menghapus jurnal draft", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("Journal", "Delete")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return IdentitasTidakValid();

            var hasil = await _service.DeleteAsync(id, actor, ct);

            await CatatAsync("Journal.Delete", hasil, id);

            return ToActionResult(hasil);
        }

        // ------------------------------------------------------------------
        // Daur hidup — BE-ACC-011
        // ------------------------------------------------------------------

        [HttpPost("{id:guid}/submit")]
        [AccessAction("Submit", "Submit Journal", Description = "Mengajukan jurnal untuk disetujui", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("Journal", "Submit")]
        public async Task<IActionResult> Submit(Guid id, CancellationToken ct)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return IdentitasTidakValid();

            var hasil = await _service.SubmitAsync(id, actor, await AmbilIzinAsync(), ct);

            await CatatAsync("Journal.Submit", hasil, id);

            return ToActionResult(hasil);
        }

        [HttpPost("{id:guid}/approve")]
        [AccessAction("Approve", "Approve Journal", Description = "Menyetujui jurnal", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("Journal", "Approve")]
        public async Task<IActionResult> Approve(Guid id, CancellationToken ct)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return IdentitasTidakValid();

            var hasil = await _service.ApproveAsync(id, actor, await AmbilIzinAsync(), ct);

            await CatatAsync("Journal.Approve", hasil, id);

            return ToActionResult(hasil);
        }

        /// <remarks>
        /// Memakai permission <c>Journal : Approve</c>, bukan permission tersendiri —
        /// `ACC-PERMISSION-0.3` menyatukan menyetujui dan menolak sebagai satu kewenangan.
        /// </remarks>
        [HttpPost("{id:guid}/reject")]
        [AccessAction("Approve", "Approve Journal", Description = "Menolak jurnal beserta alasannya", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("Journal", "Approve")]
        public async Task<IActionResult> Reject(Guid id, [FromBody] RejectJournalRequest request, CancellationToken ct)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return IdentitasTidakValid();

            var hasil = await _service.RejectAsync(id, request, actor, await AmbilIzinAsync(), ct);

            await CatatAsync("Journal.Reject", hasil, id);

            return ToActionResult(hasil);
        }

        [HttpPost("{id:guid}/post")]
        [AccessAction("Post", "Post Journal", Description = "Mengesahkan jurnal ke buku besar", AccessType = AccessTypes.Update, SortOrder = 7)]
        [AccessPermission("Journal", "Post")]
        public async Task<IActionResult> Post(Guid id, CancellationToken ct)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return IdentitasTidakValid();

            var hasil = await _service.PostAsync(id, actor, await AmbilIzinAsync(), ct);

            await CatatAsync("Journal.Post", hasil, id);

            return ToActionResult(hasil);
        }

        // ------------------------------------------------------------------
        // Pembalikan dan penyesuaian — BE-ACC-013
        // ------------------------------------------------------------------

        [HttpPost("{id:guid}/reverse")]
        [AccessAction("Reverse", "Reverse Journal", Description = "Membuat jurnal pembalik atau penyesuaian", AccessType = AccessTypes.Update, SortOrder = 8)]
        [AccessPermission("Journal", "Reverse")]
        public async Task<IActionResult> Reverse(Guid id, [FromBody] ReverseJournalRequest request, CancellationToken ct)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return IdentitasTidakValid();

            var hasil = await _service.ReverseAsync(id, request, actor, await AmbilIzinAsync(), ct);

            await CatatAsync("Journal.Reverse", hasil, id);

            return ToActionResult(hasil);
        }

        // ------------------------------------------------------------------
        // Pembantu
        // ------------------------------------------------------------------

        /// <summary>
        /// Menilai hak akses pengguna atas ketujuh tindakan jurnal, untuk mengisi
        /// <c>AvailableActions</c>.
        /// </summary>
        /// <remarks>
        /// Penilaiannya memakai <see cref="AccessPermissionService"/> yang sama dengan yang
        /// dipakai <c>[AccessPermission]</c>, sehingga daftar tindakan pada layar tidak pernah
        /// berbeda dari yang benar-benar ditegakkan saat tindakan itu dijalankan.
        ///
        /// Aturan pembuat-bukan-penyetuju <b>tidak</b> dinilai di sini — ia bergantung pada data
        /// jurnalnya, dan tempatnya di service sesuai `ACC-PERMISSION-0.3` bagian 5.
        /// </remarks>
        private async Task<JournalActorPermissions> AmbilIzinAsync()
        {
            async Task<bool> Boleh(string aksi)
                => await _accessPermissionService.HasAccessAsync(User, "Journal", aksi);

            return new JournalActorPermissions(
                CanUpdate: await Boleh("Update"),
                CanDelete: await Boleh("Delete"),
                CanSubmit: await Boleh("Submit"),
                CanApprove: await Boleh("Approve"),
                CanPost: await Boleh("Post"),
                CanReverse: await Boleh("Reverse"));
        }

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
        /// Berbeda dari controller master data, muatan permintaan <b>tidak</b> ikut dicatat.
        /// `ACC-PERMISSION-0.3` bagian 4 melarang <c>TotalDebit</c>, <c>TotalCredit</c>,
        /// <c>DebitAmount</c>, <c>CreditAmount</c>, serta isi <c>Description</c> jurnal maupun
        /// barisnya masuk payload log — dan seluruhnya ada di dalam
        /// <see cref="CreateJournalRequest"/>. Yang dicatat hanya identitas jurnal, hasilnya, dan
        /// statusnya, persis yang didaftar kolom "Yang dicatat" pada bagian itu.
        /// </remarks>
        private Task CatatAsync<T>(string aksi, AccountingServiceResult<T> hasil, Guid? id = null)
        {
            var jurnal = hasil.Data as JournalDetailResponse;

            var muatan = new
            {
                EntityId = jurnal?.Id ?? id,
                jurnal?.JournalNumber,
                JournalStatus = jurnal?.JournalStatus,
                hasil.StatusCode
            };

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
