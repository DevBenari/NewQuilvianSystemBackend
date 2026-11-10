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
    /// Handoff billing sesi terapi — <c>BE-RHB-013</c>, <c>contracts/api-contract.md</c> v1 grup
    /// Rehab Billing Handoff, <c>state-transition-matrix.md</c> §6. Disetujui pemilik
    /// ClinicalManagement/Billing lewat <c>RHB-OQ-015</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Sub-resource milik sesi (<c>/{id}/billing-handoff</c>) dengan butir hak akses sendiri
    /// (<c>RehabBillingHandoff</c>), supaya kasir/petugas billing dapat diberi hak melihat dan mengulang
    /// penyerahan tanpa hak mengubah catatan sesi. Tindakan bertarif dibentuk otomatis saat sesi
    /// ditandatangani; endpoint ini hanya membaca status dan mengulang penyerahan (idempoten).
    /// </para>
    /// <para>Logger hanya memuat id, nomor, dan status; nama tindakan dan data klinis tidak dicatat.</para>
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/rehabilitation-management/rehab-sessions")]
    [AccessController(
        moduleCode: P.ModuleCode,
        moduleName: P.ModuleName,
        displayName: "Rehab Billing Handoff",
        AreaName = P.AreaName,
        ControllerName = P.BillingHandoff.Resource,
        Description = "Status dan pengulangan penyerahan tindakan sesi terapi rehab ke Billing",
        SortOrder = 11
    )]
    [Tags("Health Services / Rehabilitation Management / Rehab Billing Handoff")]
    public class RehabSessionBillingController : ControllerBase
    {
        private const string Resource = P.BillingHandoff.Resource;
        private const string ReadName = "Read Rehab Billing Handoff";
        private const string RetryName = "Retry Rehab Billing Handoff";

        private readonly RhbBillingHandoffService _service;
        private readonly LoggerService _logger;

        public RehabSessionBillingController(RhbBillingHandoffService service, LoggerService logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("{id:guid}/billing-handoff")]
        [ProducesResponseType(typeof(ApiResponse<RehabBillingHandoffResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, ReadName, Description = "Melihat status penyerahan tindakan sesi rehab ke Billing", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(Resource, P.Read)]
        public async Task<IActionResult> GetStatus(Guid id, CancellationToken cancellationToken = default) =>
            RhbHttp.Map(this, await _service.GetStatusAsync(id, cancellationToken), "Status handoff billing sesi berhasil diambil.");

        [HttpPost("{id:guid}/billing-handoff/retry")]
        [ProducesResponseType(typeof(ApiResponse<RehabBillingHandoffResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.BillingHandoff.Retry, RetryName, Description = "Mengulang penyerahan tindakan sesi rehab ke Billing (idempoten)", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission(Resource, P.BillingHandoff.Retry)]
        public async Task<IActionResult> Retry(Guid id, CancellationToken cancellationToken = default) =>
            await RhbHttp.RespondAsync(this, await _service.RetryAsync(id, RhbHttp.ActorId(User), cancellationToken),
                "Penyerahan ke Billing diulang.", _logger, Resource, P.BillingHandoff.Retry, Trace);

        private static object Trace(RehabBillingHandoffResponse x) =>
            new { x.SessionId, x.SessionNumber, Billing = x.BillingHandoffStatus.ToString(), Items = x.Items.Count };
    }
}
