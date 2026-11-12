using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using P = QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Constants.RehabilitationPermissions;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Controllers
{
    /// <summary>
    /// Permintaan rehab beserta disposisinya — <c>BE-RHB-007</c>, <c>contracts/api-contract.md</c>
    /// v1 grup Rehab Request, <c>state-transition-matrix.md</c> §1.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Arketipe <b>aggregate ber-lifecycle</b>: perpindahan status lewat <c>POST /{id}/&lt;aksi&gt;</c>,
    /// pembatalan lewat <c>cancel</c>; tidak ada <c>DELETE</c>, <c>options</c>, atau
    /// <c>PATCH status</c>.
    /// </para>
    /// <para>
    /// <b>Pengembalian administratif dan disposisi klinis dijaga butir yang berbeda</b>
    /// (RHB-INV-02): <c>return</c> memakai <c>RehabRequest : Return</c>, sedangkan
    /// <c>request-clarification</c>, <c>accept</c>, <c>redirect</c>, dan <c>reject</c> memakai
    /// <c>RehabRequest : Disposition</c>. Pemegang <c>Return</c> yang memanggil <c>reject</c>
    /// dijawab 403 oleh filter hak akses.
    /// </para>
    /// <para>
    /// Logger hanya memuat nomor dan status; indikasi, nama perujuk, dan alasan tidak pernah
    /// dicatat (<c>permission-audit-matrix.md</c> bagian 5–6).
    /// </para>
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/rehabilitation-management/rehab-requests")]
    [AccessController(
        moduleCode: P.ModuleCode,
        moduleName: P.ModuleName,
        displayName: "Rehab Request",
        AreaName = P.AreaName,
        ControllerName = P.Request.Resource,
        Description = "Permintaan layanan Rehab Medik dan disposisinya",
        SortOrder = 3
    )]
    [Tags("Health Services / Rehabilitation Management / Rehab Request")]
    public class RehabRequestController : ControllerBase
    {
        private const string ReadName = "Read Rehab Request";
        private const string CreateName = "Create Rehab Request";
        private const string UpdateName = "Update Rehab Request";
        private const string ReturnName = "Return Rehab Request";
        private const string DispositionName = "Disposition Rehab Request";
        private const string CancelName = "Cancel Rehab Request";

        private readonly RhbRequestService _service;
        private readonly LoggerService _logger;

        public RehabRequestController(RhbRequestService service, LoggerService logger)
        {
            _service = service;
            _logger = logger;
        }

        // =====================================================================
        // Baca
        // =====================================================================

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<RehabRequestFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, ReadName, Description = "Melihat permintaan rehab", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Request.Resource, P.Read)]
        public async Task<IActionResult> GetFilterMetadata(CancellationToken cancellationToken = default) =>
            Ok(ApiResponse<RehabRequestFilterMetadataResponse>.Ok(
                await _service.BuildFilterMetadataAsync(cancellationToken), "Metadata filter permintaan rehab berhasil diambil."));

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<RehabRequestSummaryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(P.Read, ReadName, Description = "Melihat permintaan rehab", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Request.Resource, P.Read)]
        public async Task<IActionResult> GetSummary([FromQuery] RehabRequestQuery query, CancellationToken cancellationToken = default) =>
            RhbHttp.Map(this, await _service.GetSummaryAsync(query, cancellationToken), "Ringkasan permintaan rehab berhasil diambil.");

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<RehabRequestListItem>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(P.Read, ReadName, Description = "Melihat permintaan rehab", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Request.Resource, P.Read)]
        public async Task<IActionResult> GetList([FromQuery] RehabRequestQuery query, CancellationToken cancellationToken = default) =>
            RhbHttp.Map(this, await _service.GetListAsync(query, cancellationToken), "Daftar permintaan rehab berhasil diambil.");

        [HttpGet("duplicate-check")]
        [ProducesResponseType(typeof(ApiResponse<List<RehabDuplicateCandidate>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(P.Read, ReadName, Description = "Melihat permintaan rehab", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Request.Resource, P.Read)]
        public async Task<IActionResult> CheckDuplicates([FromQuery] RehabDuplicateCheckQuery query, CancellationToken cancellationToken = default) =>
            RhbHttp.Map(this, await _service.CheckDuplicatesAsync(query, cancellationToken), "Pemeriksaan duplikasi selesai.");

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<RehabRequestDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, ReadName, Description = "Melihat permintaan rehab", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Request.Resource, P.Read)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var detail = await _service.GetDetailAsync(id, cancellationToken);
            return detail == null
                ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Permintaan rehab tidak ditemukan."))
                : Ok(ApiResponse<RehabRequestDetailResponse>.Ok(detail, "Rincian permintaan rehab berhasil diambil."));
        }

        [HttpGet("{id:guid}/status-history")]
        [ProducesResponseType(typeof(ApiResponse<List<RehabStatusHistoryItem>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, ReadName, Description = "Melihat permintaan rehab", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Request.Resource, P.Read)]
        public async Task<IActionResult> GetStatusHistory(Guid id, CancellationToken cancellationToken = default) =>
            RhbHttp.Map(this, await _service.GetStatusHistoryAsync(id, cancellationToken), "Riwayat status permintaan rehab berhasil diambil.");

        // =====================================================================
        // Draft
        // =====================================================================

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<RehabRequestDetailResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Create, CreateName, Description = "Membuat draf permintaan rehab", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission(P.Request.Resource, P.Create)]
        public async Task<IActionResult> Create([FromBody] CreateRehabRequestRequest request, CancellationToken cancellationToken = default) =>
            await RhbHttp.RespondAsync(this,
                await _service.CreateAsync(request, RhbHttp.ActorId(User), cancellationToken),
                "Draf permintaan rehab berhasil dibuat.", _logger, P.Request.Resource, P.Create, Trace);

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<RehabRequestDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Update, UpdateName, Description = "Mengubah dan mengajukan permintaan rehab", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(P.Request.Resource, P.Update)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRehabRequestRequest request, CancellationToken cancellationToken = default) =>
            await RhbHttp.RespondAsync(this,
                await _service.UpdateAsync(id, request, RhbHttp.ActorId(User), cancellationToken),
                "Permintaan rehab berhasil diubah.", _logger, P.Request.Resource, P.Update, Trace);

        // =====================================================================
        // Aksi status
        // =====================================================================

        [HttpPost("{id:guid}/submit")]
        [ProducesResponseType(typeof(ApiResponse<RehabRequestDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Update, UpdateName, Description = "Mengubah dan mengajukan permintaan rehab", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(P.Request.Resource, P.Update)]
        public async Task<IActionResult> Submit(Guid id, [FromBody] SubmitRehabRequestRequest request, CancellationToken cancellationToken = default) =>
            await RhbHttp.RespondAsync(this,
                await _service.SubmitAsync(id, request, RhbHttp.ActorId(User), cancellationToken),
                "Permintaan rehab berhasil diajukan.", _logger, P.Request.Resource, "Submit", Trace);

        [HttpPost("{id:guid}/return")]
        [ProducesResponseType(typeof(ApiResponse<RehabRequestDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Request.Return, ReturnName, Description = "Mengembalikan permintaan rehab karena kelengkapan administrasi", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission(P.Request.Resource, P.Request.Return)]
        public async Task<IActionResult> Return(Guid id, [FromBody] RehabReasonRequest request, CancellationToken cancellationToken = default) =>
            await RhbHttp.RespondAsync(this,
                await _service.ReturnAsync(id, request, RhbHttp.ActorId(User), cancellationToken),
                "Permintaan rehab dikembalikan untuk dilengkapi.", _logger, P.Request.Resource, P.Request.Return, Trace);

        [HttpPost("{id:guid}/request-clarification")]
        [ProducesResponseType(typeof(ApiResponse<RehabRequestDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Request.Disposition, DispositionName, Description = "Menelaah permintaan rehab: klarifikasi, terima, alihkan, atau tolak", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission(P.Request.Resource, P.Request.Disposition)]
        public async Task<IActionResult> RequestClarification(Guid id, [FromBody] RehabReasonRequest request, CancellationToken cancellationToken = default) =>
            await RhbHttp.RespondAsync(this,
                await _service.RequestClarificationAsync(id, request, RhbHttp.ActorId(User), cancellationToken),
                "Klarifikasi permintaan rehab berhasil diminta.", _logger, P.Request.Resource, "RequestClarification", Trace);

        [HttpPost("{id:guid}/accept")]
        [ProducesResponseType(typeof(ApiResponse<RehabRequestDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Request.Disposition, DispositionName, Description = "Menelaah permintaan rehab: klarifikasi, terima, alihkan, atau tolak", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission(P.Request.Resource, P.Request.Disposition)]
        public async Task<IActionResult> Accept(Guid id, [FromBody] AcceptRehabRequestRequest request, CancellationToken cancellationToken = default) =>
            await RhbHttp.RespondAsync(this,
                await _service.AcceptAsync(id, request, RhbHttp.ActorId(User), cancellationToken),
                "Permintaan rehab diterima dan episode dibuka.", _logger, P.Request.Resource, "Accept",
                x => new { x.Id, x.RequestNumber, Status = x.RequestStatus.ToString(), x.EpisodeId, x.EpisodeNumber });

        [HttpPost("{id:guid}/redirect")]
        [ProducesResponseType(typeof(ApiResponse<RehabRequestDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Request.Disposition, DispositionName, Description = "Menelaah permintaan rehab: klarifikasi, terima, alihkan, atau tolak", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission(P.Request.Resource, P.Request.Disposition)]
        public async Task<IActionResult> Redirect(Guid id, [FromBody] RedirectRehabRequestRequest request, CancellationToken cancellationToken = default) =>
            await RhbHttp.RespondAsync(this,
                await _service.RedirectAsync(id, request, RhbHttp.ActorId(User), cancellationToken),
                "Permintaan rehab dialihkan.", _logger, P.Request.Resource, "Redirect", Trace);

        [HttpPost("{id:guid}/reject")]
        [ProducesResponseType(typeof(ApiResponse<RehabRequestDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Request.Disposition, DispositionName, Description = "Menelaah permintaan rehab: klarifikasi, terima, alihkan, atau tolak", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission(P.Request.Resource, P.Request.Disposition)]
        public async Task<IActionResult> Reject(Guid id, [FromBody] RehabReasonRequest request, CancellationToken cancellationToken = default) =>
            await RhbHttp.RespondAsync(this,
                await _service.RejectAsync(id, request, RhbHttp.ActorId(User), cancellationToken),
                "Permintaan rehab ditolak.", _logger, P.Request.Resource, "Reject", Trace);

        [HttpPost("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<RehabRequestDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Request.Cancel, CancelName, Description = "Membatalkan permintaan rehab", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission(P.Request.Resource, P.Request.Cancel)]
        public async Task<IActionResult> Cancel(Guid id, [FromBody] RehabReasonRequest request, CancellationToken cancellationToken = default) =>
            await RhbHttp.RespondAsync(this,
                await _service.CancelAsync(id, request, RhbHttp.ActorId(User), cancellationToken),
                "Permintaan rehab dibatalkan.", _logger, P.Request.Resource, P.Request.Cancel, Trace);

        /// <summary>Isi logger: nomor dan status saja, tanpa kolom sensitif.</summary>
        private static object Trace(RehabRequestDetailResponse x) =>
            new { x.Id, x.RequestNumber, Status = x.RequestStatus.ToString() };
    }
}
