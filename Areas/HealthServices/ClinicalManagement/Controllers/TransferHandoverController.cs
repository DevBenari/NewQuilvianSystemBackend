using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers
{
    /// <summary>
    /// Serah terima klinis saat transfer antarunit (<c>BE-RWI-183</c>, API 11.8, <c>P2</c>).
    /// </summary>
    /// <remarks>
    /// Pembuatan dokumen tidak punya endpoint: dokumen lahir dari transfer antarunit. Permission terima
    /// sengaja terpisah dari kirim (<c>RWI-DEC-189</c>); hak <c>TransferHandover : Receive</c>
    /// diberikan admin hak akses pada perawat unit tujuan.
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/clinical-management/transfer-handovers")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_CLINICAL",
        moduleName: "Health Service Clinical",
        displayName: "Transfer Handover",
        AreaName = "HealthServices",
        ControllerName = "TransferHandover",
        Description = "Serah terima klinis pasien saat transfer antarunit rawat inap",
        SortOrder = 40
    )]
    [Tags("Health Services / Clinical Management / Transfer Handover")]
    public class TransferHandoverController : ControllerBase
    {
        private readonly CliTransferHandoverService _service;

        public TransferHandoverController(CliTransferHandoverService service)
        {
            _service = service;
        }

        /// <summary>Dokumen per episode atau per unit, dengan saringan status.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<TransferHandoverResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Transfer Handover", Description = "Melihat serah terima klinis transfer antarunit", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("TransferHandover", "Read")]
        public async Task<IActionResult> GetList([FromQuery] TransferHandoverQuery query,
            CancellationToken cancellationToken = default)
        {
            var result = await _service.GetListAsync(query, GetCurrentUserId(), cancellationToken);
            return Ok(ApiResponse<List<TransferHandoverResponse>>.Ok(result,
                "Daftar serah terima transfer berhasil diambil."));
        }

        /// <summary>Detail sembilan bagian.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<TransferHandoverResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Transfer Handover", Description = "Melihat detail serah terima klinis transfer", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("TransferHandover", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _service.GetByIdAsync(id, cancellationToken);
            return result == null
                ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Dokumen serah terima transfer tidak ditemukan."))
                : Ok(ApiResponse<TransferHandoverResponse>.Ok(result, "Detail serah terima transfer berhasil diambil."));
        }

        /// <summary>Melengkapi bagian yang diisi pengirim.</summary>
        [HttpPut("{id:guid}/draft")]
        [ProducesResponseType(typeof(ApiResponse<TransferHandoverResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Send", "Send Transfer Handover", Description = "Mengisi dan mengirim serah terima klinis transfer", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("TransferHandover", "Send")]
        public async Task<IActionResult> SaveDraft(Guid id, [FromBody] SaveTransferHandoverDraftRequest request,
            CancellationToken cancellationToken = default)
            => ToResult(await _service.SaveDraftAsync(id, request, GetCurrentUserId(), cancellationToken));

        /// <summary>Mengirim; server membekukan potret klinis.</summary>
        [HttpPatch("{id:guid}/send")]
        [ProducesResponseType(typeof(ApiResponse<TransferHandoverResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Send", "Send Transfer Handover", Description = "Mengisi dan mengirim serah terima klinis transfer", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("TransferHandover", "Send")]
        public async Task<IActionResult> Send(Guid id, [FromBody] SendTransferHandoverRequest request,
            CancellationToken cancellationToken = default)
            => ToResult(await _service.SendAsync(id, request, GetCurrentUserId(), cancellationToken));

        /// <summary>Terima, atau tolak beralasan.</summary>
        /// <remarks>
        /// Penerima sama dengan pengirim → 422 <c>CLI-TRH-001</c>; pasien belum menempati bed aktif di
        /// unit tujuan → 422 <c>CLI-TRH-002</c>; tolak tanpa alasan → 400; versi berubah → 409.
        /// </remarks>
        [HttpPatch("{id:guid}/accept")]
        [ProducesResponseType(typeof(ApiResponse<TransferHandoverResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Receive", "Receive Transfer Handover", Description = "Menerima atau menolak serah terima klinis transfer di unit tujuan", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("TransferHandover", "Receive")]
        public async Task<IActionResult> Accept(Guid id, [FromBody] AcceptTransferHandoverRequest request,
            CancellationToken cancellationToken = default)
            => ToResult(await _service.AcceptAsync(id, request, GetCurrentUserId(), cancellationToken));

        private IActionResult ToResult(CliTransferHandoverResult result)
        {
            if (result.IsSuccess)
                return Ok(ApiResponse<TransferHandoverResponse>.Ok(result.Data, result.Message));

            return StatusCode(result.StatusCode, ApiResponse<object>.Fail(
                result.StatusCode,
                result.Message,
                result.Code != null ? new { result.Code } : null));
        }

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}
