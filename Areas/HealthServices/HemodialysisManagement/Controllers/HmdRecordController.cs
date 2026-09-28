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
    /// Pengesahan dan penguncian catatan sesi HD oleh dokter penanggung jawab sesi (<c>BE-HMD-17</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>HemodialysisRecord : Finalize</c> sengaja dipisahkan dari <c>HemodialysisSession : Update</c>:
    /// yang satu mengisi catatan, yang lain menyatakan catatan itu sah dan menguncinya.
    /// </para>
    /// <para>
    /// Koreksi setelah pengesahan <b>tidak</b> dilayani controller ini. Ia memakai endpoint addendum
    /// Rekam Medis yang sudah ada — <c>POST /api/v1/health-services/medical-record-management/
    /// clinical-note-addendums/by-document/HemodialysisSession/{sessionId}</c> — sehingga catatan
    /// asli tetap utuh dan koreksinya tercatat berdampingan.
    /// </para>
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/hemodialysis-management/hemodialysis-sessions")]
    [AccessController(
        moduleCode: P.ModuleCode,
        moduleName: P.ModuleName,
        displayName: "Hemodialysis Record",
        AreaName = P.AreaName,
        ControllerName = P.Record.Resource,
        Description = "Pengesahan dan penguncian catatan sesi hemodialisa",
        SortOrder = 13
    )]
    [Tags("Health Services / Hemodialysis Management / Hemodialysis Session")]
    public class HmdRecordController : ControllerBase
    {
        private readonly HmdSessionFinalizationService _service;
        private readonly LoggerService _logger;

        public HmdRecordController(HmdSessionFinalizationService service, LoggerService logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Dokter penanggung jawab sesi mengesahkan dan mengunci catatan. Dokter lain ditolak
        /// <c>403</c>; penyerahan ke Billing dijalankan setelah pengesahan tersimpan.
        /// </summary>
        [HttpPost("{id:guid}/finalize")]
        [ProducesResponseType(typeof(ApiResponse<HmdSessionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Record.Finalize, "Finalize Hemodialysis Record", Description = "Dokter penanggung jawab mengesahkan dan mengunci catatan sesi HD", AccessType = AccessTypes.Update, SortOrder = 1)]
        [AccessPermission(P.Record.Resource, P.Record.Finalize)]
        public async Task<IActionResult> Finalize(Guid id, [FromBody] FinalizeHmdSessionRequest? request, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _service.FinalizeAsync(
                    id,
                    request ?? new FinalizeHmdSessionRequest(),
                    HmdHttp.Actor(this),
                    Request.Headers.UserAgent.ToString(),
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    cancellationToken),
                "Catatan sesi hemodialisa berhasil disahkan dan dikunci.", _logger, P.Record.Resource, P.Record.Finalize,
                x => new { x.Id, x.SessionNumber, x.SessionStatus, x.SignedByUserId, x.SignedAt, x.BillingHandoffStatus });

        /// <summary>
        /// Dokter penanggung jawab mengembalikan dokumentasi kepada perawat untuk dilengkapi
        /// (<c>AwaitingFinalization → Completed/Stopped</c>, <c>HMD-VAL-071</c>).
        /// </summary>
        [HttpPost("{id:guid}/return-for-completion")]
        [ProducesResponseType(typeof(ApiResponse<HmdSessionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status423Locked)]
        [AccessAction(P.Record.Finalize, "Finalize Hemodialysis Record", Description = "Dokter penanggung jawab mengembalikan dokumentasi sesi HD untuk dilengkapi", AccessType = AccessTypes.Update, SortOrder = 1)]
        [AccessPermission(P.Record.Resource, P.Record.Finalize)]
        public async Task<IActionResult> ReturnForCompletion(Guid id, [FromBody] ReturnHmdSessionRequest request, CancellationToken cancellationToken = default) =>
            await HmdHttp.RespondAsync(this,
                await _service.ReturnForCompletionAsync(id, request, HmdHttp.Actor(this), cancellationToken),
                "Dokumentasi sesi dikembalikan untuk dilengkapi.", _logger, P.Record.Resource, "ReturnForCompletion",
                x => new { x.Id, x.SessionNumber, x.SessionStatus });
    }
}
