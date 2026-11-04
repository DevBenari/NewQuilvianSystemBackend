using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using P = QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Constants.OpticalPermissions;

namespace QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Controllers
{
    /// <summary>
    /// Keluhan garansi — <c>OPT-API-v1</c> grup Opt Warranty Claim (<c>BE-OPT-050</c>): daftar, detail, catat, periksa ulang,
    /// putuskan remake.
    /// </summary>
    /// <remarks>Kalimat log hanya memuat nomor keluhan, nomor pesanan, dan status; isi keluhan dan temuan tidak dicatat. Action sengaja tanpa <c>&lt;summary&gt;</c> XML.</remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/optical-management/warranty-claims")]
    [AccessController(
        moduleCode: P.ModuleCode,
        moduleName: P.ModuleName,
        displayName: "Opt Warranty Claim",
        AreaName = P.AreaName,
        ControllerName = P.WarrantyClaim.Resource,
        Description = "Keluhan garansi kacamata: catat, periksa ulang, setujui remake atau tolak",
        SortOrder = 6
    )]
    [Tags("Health Services / Optical Management / Opt Warranty Claim")]
    public class OptWarrantyClaimController : ControllerBase
    {
        private readonly OptWarrantyClaimService _service;
        private readonly LoggerService _logger;

        public OptWarrantyClaimController(OptWarrantyClaimService service, LoggerService logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<OptWarrantyClaimResponse>>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, "Read Opt Warranty Claim", Description = "Melihat keluhan garansi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.WarrantyClaim.Resource, P.Read)]
        public async Task<IActionResult> GetList([FromQuery] OptWarrantyClaimListQuery query, CancellationToken cancellationToken = default) =>
            Ok(ApiResponse<PagedResult<OptWarrantyClaimResponse>>.Ok(
                await _service.GetListAsync(query, cancellationToken), "Daftar keluhan garansi berhasil diambil."));

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<OptWarrantyClaimResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, "Read Opt Warranty Claim", Description = "Melihat keluhan garansi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.WarrantyClaim.Resource, P.Read)]
        public async Task<IActionResult> GetDetail(Guid id, CancellationToken cancellationToken = default) =>
            OptHttp.Map(this, await _service.GetDetailAsync(id, cancellationToken), "Detail keluhan garansi berhasil diambil.");

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<OptWarrantyClaimResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.Create, "Create Opt Warranty Claim", Description = "Mencatat keluhan garansi pada kunjungan keluhan", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission(P.WarrantyClaim.Resource, P.Create)]
        public async Task<IActionResult> Create([FromBody] OptWarrantyClaimCreateRequest request, CancellationToken cancellationToken = default) =>
            await OptHttp.RespondAsync(this,
                await _service.CreateAsync(request, OptHttp.ActorId(User), cancellationToken),
                "Keluhan garansi tercatat.", _logger, P.WarrantyClaim.Resource, P.Create,
                x => $"{x.ClaimNumber} dicatat atas pesanan {x.OrderNumber} ({x.ComplaintTypeLabel}, {(x.IsWithinWarranty ? "dalam" : "di luar")} garansi).");

        [HttpPost("{id:guid}/examination")]
        [ProducesResponseType(typeof(ApiResponse<OptWarrantyClaimResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status503ServiceUnavailable)]
        [AccessAction(P.WarrantyClaim.Examine, "Examine Opt Warranty Claim", Description = "Mencatat pemeriksaan ulang keluhan (RO/Sp.M)", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(P.WarrantyClaim.Resource, P.WarrantyClaim.Examine)]
        public async Task<IActionResult> Examine(Guid id, [FromBody] OptWarrantyClaimExamineRequest request, CancellationToken cancellationToken = default) =>
            await OptHttp.RespondAsync(this,
                await _service.ExamineAsync(id, request, OptHttp.ActorId(User), cancellationToken),
                "Pemeriksaan ulang tercatat.", _logger, P.WarrantyClaim.Resource, P.WarrantyClaim.Examine,
                x => $"{x.ClaimNumber} diperiksa ulang.");

        [HttpPost("{id:guid}/decision")]
        [ProducesResponseType(typeof(ApiResponse<OptWarrantyClaimResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.WarrantyClaim.Decide, "Decide Opt Warranty Claim", Description = "Menyetujui remake atau menolak keluhan (Kepala Optik)", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission(P.WarrantyClaim.Resource, P.WarrantyClaim.Decide)]
        public async Task<IActionResult> Decide(Guid id, [FromBody] OptWarrantyClaimDecisionRequest request, CancellationToken cancellationToken = default) =>
            await OptHttp.RespondAsync(this,
                await _service.DecideAsync(id, request, OptHttp.ActorId(User), cancellationToken),
                request.Approve ? "Remake disetujui." : "Keluhan ditolak.", _logger, P.WarrantyClaim.Resource, P.WarrantyClaim.Decide,
                x => $"{x.ClaimNumber} {x.StatusLabel}" + (x.RemakeOrderNumber is null ? "." : $"; remake {x.RemakeOrderNumber}."));
    }
}
