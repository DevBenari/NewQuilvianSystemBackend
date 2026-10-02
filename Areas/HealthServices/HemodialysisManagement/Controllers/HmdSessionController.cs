using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using P = QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Constants.HemodialysisPermissions;

namespace QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Controllers
{
    /// <summary>
    /// Pelaksanaan sesi HD — aggregate ber-lifecycle dua belas status (<c>BE-HMD-12</c> sampai
    /// <c>BE-HMD-16</c>, <c>BE-HMD-18</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Setiap perpindahan status punya perintah <c>POST /{id}/&lt;aksi&gt;</c>; dokumen tunggal milik
    /// sesi — checklist, Pra-HD, Pasca-HD — memakai <c>PUT</c> pada path tetap. Sesi yang sudah
    /// disahkan menolak setiap penulisan dengan <c>423</c>.
    /// </para>
    /// <para>
    /// <b>Pembacaan konteks sesi ikut dicatat logger</b> — pengecualian bernama pada
    /// <c>permission-audit-matrix.md</c> bagian 5, karena membuka ruang kerja sesi berarti membuka
    /// catatan klinis lengkap seorang pasien.
    /// </para>
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/hemodialysis-management/hemodialysis-sessions")]
    [AccessController(
        moduleCode: P.ModuleCode,
        moduleName: P.ModuleName,
        displayName: "Hemodialysis Session",
        AreaName = P.AreaName,
        ControllerName = P.Session.Resource,
        Description = "Pelaksanaan sesi hemodialisa",
        SortOrder = 9
    )]
    [Tags("Health Services / Hemodialysis Management / Hemodialysis Session")]
    public class HmdSessionController : ControllerBase
    {
        private readonly HmdSessionService _sessionService;
        private readonly HmdBillingHandoffService _billingHandoffService;
        private readonly LoggerService _logger;

        public HmdSessionController(
            HmdSessionService sessionService,
            HmdBillingHandoffService billingHandoffService,
            LoggerService logger)
        {
            _sessionService = sessionService;
            _billingHandoffService = billingHandoffService;
            _logger = logger;
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<HmdSessionDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, "Read Hemodialysis Session", Description = "Membuka konteks lengkap satu sesi HD", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Session.Resource, P.Read)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var detail = await _sessionService.GetDetailAsync(id, cancellationToken);
            var result = detail == null
                ? HmdResult<HmdSessionDetailResponse>.NotFound("Sesi hemodialisa tidak ditemukan atau sudah dihapus.")
                : HmdResult<HmdSessionDetailResponse>.Ok(detail);

            return await HmdHttp.RespondAsync(this, result, "Konteks sesi HD berhasil diambil.", _logger, P.Session.Resource, P.Read,
                x => new { x.Id, x.SessionNumber, x.SessionStatus });
        }

        [HttpPost("{id:guid}/check-in")]
        [ProducesResponseType(typeof(ApiResponse<HmdSessionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status423Locked)]
        [AccessAction(P.Session.CheckIn, "Check In Hemodialysis Session", Description = "Menandai pasien HD sudah datang", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission(P.Session.Resource, P.Session.CheckIn)]
        public async Task<IActionResult> CheckIn(Guid id, [FromBody] CheckInHmdSessionRequest? request, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _sessionService.CheckInAsync(id, request ?? new CheckInHmdSessionRequest(), HmdHttp.ActorId(User), cancellationToken),
                "Kedatangan pasien berhasil dicatat.", _logger, P.Session.Resource, P.Session.CheckIn,
                x => new { x.Id, x.SessionNumber, x.SessionStatus, x.EncounterId });

        [HttpGet("{id:guid}/checklist")]
        [ProducesResponseType(typeof(ApiResponse<List<HmdSessionChecklistResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, "Read Hemodialysis Session", Description = "Melihat checklist Pra-HD beserta hasilnya", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Session.Resource, P.Read)]
        public async Task<IActionResult> GetChecklist(Guid id, CancellationToken cancellationToken = default) =>
            HmdHttp.Map(this, await _sessionService.GetChecklistAsync(id, cancellationToken), "Checklist Pra-HD berhasil diambil.");

        [HttpPut("{id:guid}/checklist")]
        [ProducesResponseType(typeof(ApiResponse<List<HmdSessionChecklistResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status423Locked)]
        [AccessAction(P.Update, "Update Hemodialysis Session", Description = "Menyimpan hasil pemeriksaan checklist Pra-HD", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(P.Session.Resource, P.Update)]
        public async Task<IActionResult> SaveChecklist(Guid id, [FromBody] SaveHmdChecklistRequest request, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _sessionService.SaveChecklistAsync(id, request, HmdHttp.ActorId(User), cancellationToken),
                "Checklist Pra-HD berhasil disimpan.", _logger, P.Session.Resource, "SaveChecklist",
                x => new { SessionId = id, ItemCount = request.Items.Count, Satisfied = x.Count(i => i.IsSatisfied) });

        /// <summary>
        /// Dokter melewati satu butir checklist dengan alasan tertulis. Butir yang masternya tidak
        /// boleh dilewati ditolak <c>422</c>, apa pun alasannya.
        /// </summary>
        [HttpPost("{id:guid}/checklist/{itemId:guid}/override")]
        [ProducesResponseType(typeof(ApiResponse<HmdSessionChecklistResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status423Locked)]
        [AccessAction(P.Session.OverrideChecklist, "Override Hemodialysis Checklist", Description = "Dokter melewati satu butir checklist Pra-HD dengan alasan tertulis", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission(P.Session.Resource, P.Session.OverrideChecklist)]
        public async Task<IActionResult> OverrideChecklist(Guid id, Guid itemId, [FromBody] OverrideHmdChecklistRequest request, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _sessionService.OverrideChecklistAsync(id, itemId, request, HmdHttp.Actor(this), cancellationToken),
                "Butir checklist berhasil dilewati.", _logger, P.Session.Resource, P.Session.OverrideChecklist,
                x => new { SessionId = id, x.ChecklistItemId, x.ItemCode, x.IsOverridden });

        [HttpPut("{id:guid}/pre-hd")]
        [ProducesResponseType(typeof(ApiResponse<HmdSessionAssessmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status423Locked)]
        [AccessAction(P.Update, "Update Hemodialysis Session", Description = "Menyimpan penilaian Pra-HD termasuk berat badan dan tanda vital", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(P.Session.Resource, P.Update)]
        public async Task<IActionResult> SavePreHd(Guid id, [FromBody] SaveHmdPreAssessmentRequest request, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _sessionService.SavePreAssessmentAsync(id, request, HmdHttp.ActorId(User), cancellationToken),
                "Penilaian Pra-HD berhasil disimpan.", _logger, P.Session.Resource, "SavePreHd",
                x => new { SessionId = id, x.Id, x.PatientVitalSignId });

        [HttpPost("{id:guid}/ready")]
        [ProducesResponseType(typeof(ApiResponse<HmdSessionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status423Locked)]
        [AccessAction(P.Session.DeclareReady, "Declare Hemodialysis Session Ready", Description = "Menyatakan sesi HD siap dimulai", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission(P.Session.Resource, P.Session.DeclareReady)]
        public async Task<IActionResult> DeclareReady(Guid id, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _sessionService.DeclareReadyAsync(id, HmdHttp.ActorId(User), cancellationToken),
                "Sesi hemodialisa dinyatakan siap.", _logger, P.Session.Resource, P.Session.DeclareReady,
                x => new { x.Id, x.SessionNumber, x.SessionStatus, x.ReadyAt });

        [HttpPost("{id:guid}/hold")]
        [ProducesResponseType(typeof(ApiResponse<HmdSessionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status423Locked)]
        [AccessAction(P.Session.Hold, "Hold Hemodialysis Session", Description = "Menahan sesi HD karena ada yang belum siap", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission(P.Session.Resource, P.Session.Hold)]
        public async Task<IActionResult> Hold(Guid id, [FromBody] HoldHmdSessionRequest request, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _sessionService.HoldAsync(id, request, HmdHttp.ActorId(User), cancellationToken),
                "Sesi hemodialisa berhasil ditahan.", _logger, P.Session.Resource, P.Session.Hold,
                x => new { x.Id, x.SessionNumber, x.SessionStatus });

        /// <summary>Melanjutkan sesi yang ditahan kembali ke persiapan Pra-HD (<c>Held → PreCheck</c>).</summary>
        [HttpPost("{id:guid}/resume")]
        [ProducesResponseType(typeof(ApiResponse<HmdSessionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status423Locked)]
        [AccessAction(P.Session.Hold, "Hold Hemodialysis Session", Description = "Melanjutkan sesi HD yang ditahan", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission(P.Session.Resource, P.Session.Hold)]
        public async Task<IActionResult> Resume(Guid id, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _sessionService.ResumeAsync(id, HmdHttp.ActorId(User), cancellationToken),
                "Sesi hemodialisa dilanjutkan ke persiapan Pra-HD.", _logger, P.Session.Resource, "Resume",
                x => new { x.Id, x.SessionNumber, x.SessionStatus });

        /// <summary>
        /// Memulai cuci darah. Syarat siap diperiksa ulang saat ini juga; permintaan ulang dengan
        /// kunci idempotency yang sama mengembalikan sesi yang sama tanpa tindakan kedua.
        /// </summary>
        [HttpPost("{id:guid}/start")]
        [ProducesResponseType(typeof(ApiResponse<HmdSessionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status423Locked)]
        [AccessAction(P.Session.Start, "Start Hemodialysis Session", Description = "Memulai cuci darah", AccessType = AccessTypes.Update, SortOrder = 7)]
        [AccessPermission(P.Session.Resource, P.Session.Start)]
        public async Task<IActionResult> Start(Guid id, [FromBody] StartHmdSessionRequest request, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _sessionService.StartAsync(id, request, HmdHttp.ActorId(User), cancellationToken),
                "Sesi hemodialisa dimulai.", _logger, P.Session.Resource, P.Session.Start,
                x => new { x.Id, x.SessionNumber, x.SessionStatus, x.StartedAt, x.PatientProcedureId });

        [HttpPost("{id:guid}/stop")]
        [ProducesResponseType(typeof(ApiResponse<HmdSessionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status423Locked)]
        [AccessAction(P.Session.Stop, "Stop Hemodialysis Session", Description = "Menghentikan sesi HD sebelum selesai dengan alasan wajib", AccessType = AccessTypes.Update, SortOrder = 8)]
        [AccessPermission(P.Session.Resource, P.Session.Stop)]
        public async Task<IActionResult> Stop(Guid id, [FromBody] StopHmdSessionRequest request, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _sessionService.StopAsync(id, request, HmdHttp.ActorId(User), cancellationToken),
                "Sesi hemodialisa dihentikan.", _logger, P.Session.Resource, P.Session.Stop,
                x => new { x.Id, x.SessionNumber, x.SessionStatus, x.StopReason, x.EndedAt });

        [HttpPost("{id:guid}/complete")]
        [ProducesResponseType(typeof(ApiResponse<HmdSessionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status423Locked)]
        [AccessAction(P.Session.Complete, "Complete Hemodialysis Session", Description = "Menyatakan cuci darah selesai secara fisik", AccessType = AccessTypes.Update, SortOrder = 9)]
        [AccessPermission(P.Session.Resource, P.Session.Complete)]
        public async Task<IActionResult> Complete(Guid id, [FromBody] CompleteHmdSessionRequest? request, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _sessionService.CompleteAsync(id, request ?? new CompleteHmdSessionRequest(), HmdHttp.ActorId(User), cancellationToken),
                "Sesi hemodialisa selesai.", _logger, P.Session.Resource, P.Session.Complete,
                x => new { x.Id, x.SessionNumber, x.SessionStatus, x.EndedAt });

        [HttpPut("{id:guid}/post-hd")]
        [ProducesResponseType(typeof(ApiResponse<HmdSessionAssessmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status423Locked)]
        [AccessAction(P.Update, "Update Hemodialysis Session", Description = "Menyimpan penilaian Pasca-HD dan disposisi pasien", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(P.Session.Resource, P.Update)]
        public async Task<IActionResult> SavePostHd(Guid id, [FromBody] SaveHmdPostAssessmentRequest request, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _sessionService.SavePostAssessmentAsync(id, request, HmdHttp.ActorId(User), cancellationToken),
                "Penilaian Pasca-HD berhasil disimpan.", _logger, P.Session.Resource, "SavePostHd",
                x => new { SessionId = id, x.Id, x.PatientVitalSignId, request.Disposition });

        /// <summary>
        /// Perawat menyatakan dokumentasi selesai. Penyelesainya tercatat terpisah dari dokter yang
        /// kelak mengesahkan.
        /// </summary>
        [HttpPost("{id:guid}/submit-documentation")]
        [ProducesResponseType(typeof(ApiResponse<HmdSessionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status423Locked)]
        [AccessAction(P.Session.SubmitDocumentation, "Submit Hemodialysis Documentation", Description = "Perawat menyatakan dokumentasi sesi HD selesai", AccessType = AccessTypes.Update, SortOrder = 10)]
        [AccessPermission(P.Session.Resource, P.Session.SubmitDocumentation)]
        public async Task<IActionResult> SubmitDocumentation(Guid id, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _sessionService.SubmitDocumentationAsync(id, HmdHttp.ActorId(User), cancellationToken),
                "Dokumentasi sesi hemodialisa diajukan untuk disahkan.", _logger, P.Session.Resource, P.Session.SubmitDocumentation,
                x => new { x.Id, x.SessionNumber, x.SessionStatus, x.DocumentedByUserId, x.DocumentedAt });

        [HttpGet("{id:guid}/billing-handoff")]
        [ProducesResponseType(typeof(ApiResponse<HmdBillingHandoffResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, "Read Hemodialysis Session", Description = "Melihat status penyerahan tindakan sesi HD ke Billing", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Session.Resource, P.Read)]
        public async Task<IActionResult> GetBillingHandoff(Guid id, CancellationToken cancellationToken = default) =>
            HmdHttp.Map(this, await _billingHandoffService.GetStatusAsync(id, cancellationToken), "Status penyerahan ke Billing berhasil diambil.");

        /// <summary>
        /// Mengulang penyerahan tindakan yang tertunda atau gagal. Dicatat logger walaupun tidak
        /// mengubah data klinis, karena menyentuh jalur keuangan.
        /// </summary>
        [HttpPost("{id:guid}/billing-handoff/retry")]
        [ProducesResponseType(typeof(ApiResponse<HmdBillingHandoffResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Session.RetryBillingHandoff, "Retry Hemodialysis Billing Handoff", Description = "Mengulang penyerahan tindakan sesi HD ke Billing", AccessType = AccessTypes.Update, SortOrder = 11)]
        [AccessPermission(P.Session.Resource, P.Session.RetryBillingHandoff)]
        public async Task<IActionResult> RetryBillingHandoff(Guid id, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _billingHandoffService.RetryAsync(id, HmdHttp.ActorId(User), cancellationToken),
                "Penyerahan ke Billing berhasil diproses ulang.", _logger, P.Session.Resource, P.Session.RetryBillingHandoff,
                x => new { x.SessionId, x.BillingHandoffStatus });
    }
}
