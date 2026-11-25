using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services;
using QuilvianSystemBackend.Areas.SelfServices.HumanResource.DTOs;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Shared.HumanResource.Services;

namespace QuilvianSystemBackend.Areas.SelfServices.HumanResource.Controllers
{
    // Layanan Mandiri izin pulang cepat (HRD-DEC-241, kontrak v30 API 36.4, INT-HR-115). Identitas pegawai hanya dari sesi
    // (VR-PK-34); aturan pengajuan sama dengan layar HR lewat EarlyLeavePermissionService. Persetujuan tetap di layar HR.
    [ApiController]
    [Authorize]
    [Route("api/v1/self-services/human-resource/early-leave-permissions")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_EMPLOYEE_SELF_SERVICE",
        moduleName: "Human Resource Employee Self Service",
        displayName: "My Early Leave Permission",
        AreaName = "SelfServices",
        ControllerName = "MyEarlyLeavePermission",
        Description = "Pegawai mengajukan, melihat, dan membatalkan izin pulang cepat miliknya",
        SortOrder = 13)]
    [Tags("Self Services / Human Resource / Early Leave Permission")]
    public class EarlyLeavePermissionSelfServiceController : ControllerBase
    {
        private const string LogCategory = "SelfServices.HumanResource.EarlyLeavePermission";

        private readonly EarlyLeavePermissionService _service;
        private readonly HumanResourceContextService _contextService;
        private readonly LoggerService _loggerService;

        public EarlyLeavePermissionSelfServiceController(
            EarlyLeavePermissionService service,
            HumanResourceContextService contextService,
            LoggerService loggerService)
        {
            _service = service;
            _contextService = contextService;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<EarlyLeaveSelfFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read My Early Leave Permission", Description = "Melihat izin pulang cepat milik sendiri", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MyEarlyLeavePermission", "Read")]
        public IActionResult GetFilterMetadata()
        {
            return Ok(ApiResponse<EarlyLeaveSelfFilterMetadataResponse>.Ok(
                new EarlyLeaveSelfFilterMetadataResponse
                {
                    StatusOptions = EarlyLeaveSelfStatus.All.Select(x => new EarlyLeaveSelfOptionResponse { Value = x.Value, Label = x.Label }).ToList(),
                    PageSizeOptions = new List<int> { 10, 25, 50, 100 }
                },
                "Metadata izin pulang cepat berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<EarlyLeaveSelfResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read My Early Leave Permission", Description = "Melihat izin pulang cepat milik sendiri", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MyEarlyLeavePermission", "Read")]
        public async Task<IActionResult> GetMine([FromQuery] EarlyLeaveSelfQuery query, CancellationToken cancellationToken = default)
        {
            var (profileId, error) = await ResolveProfileAsync(cancellationToken);
            if (error != null) return error;

            var result = await _service.GetPagedForSelfAsync(profileId, query.FromDate, query.ToDate, query.Status, query.PageNumber, query.PageSize, cancellationToken);
            if (!result.Success) return Fail(result.StatusCode, result.Message);

            var page = result.Data!;
            return Ok(ApiResponse<PagedResult<EarlyLeaveSelfResponse>>.Ok(
                new PagedResult<EarlyLeaveSelfResponse>
                {
                    PageNumber = page.PageNumber,
                    PageSize = page.PageSize,
                    TotalData = page.TotalData,
                    TotalPage = page.TotalPage,
                    Items = page.Items.Select(EarlyLeaveSelfResponse.From).ToList()
                },
                result.Message));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<EarlyLeaveSelfResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read My Early Leave Permission", Description = "Melihat izin pulang cepat milik sendiri", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MyEarlyLeavePermission", "Read")]
        public async Task<IActionResult> GetMineById(Guid id, CancellationToken cancellationToken = default)
        {
            var (profileId, error) = await ResolveProfileAsync(cancellationToken);
            if (error != null) return error;

            return ToResult(await _service.GetByIdForSelfAsync(id, profileId, cancellationToken));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<EarlyLeaveSelfResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Create", "Create My Early Leave Permission", Description = "Mengajukan izin pulang cepat untuk diri sendiri", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("MyEarlyLeavePermission", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateEarlyLeaveSelfRequest request, CancellationToken cancellationToken = default)
        {
            var (profileId, error) = await ResolveProfileAsync(cancellationToken);
            if (error != null) return error;
            if (!request.AttendanceDate.HasValue || !request.RequestedEarlyLeaveAt.HasValue)
            {
                return Fail(StatusCodes.Status400BadRequest, "Tanggal dan jam izin pulang cepat wajib diisi.");
            }

            var result = await _service.CreateForSelfAsync(
                profileId, request.AttendanceDate.Value, request.RequestedEarlyLeaveAt.Value, request.Reason, CurrentUserId(), cancellationToken);
            if (result.Success)
            {
                // Matriks audit 34: Id izin, tanggal, jam izin. Alasan tidak dicatat (data sensitif).
                var d = result.Data!;
                await _loggerService.AuditAsync(LogCategory, "EarlyLeavePermission.SelfCreate",
                    $"Izin pulang cepat {d.Id} tanggal {d.AttendanceDate:yyyy-MM-dd} jam izin {d.RequestedEarlyLeaveAt:yyyy-MM-ddTHH:mm:ss} diajukan pemohon; status {d.Status}.");
            }

            return ToResult(result);
        }

        [HttpPost("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<EarlyLeaveSelfResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Cancel", "Cancel My Early Leave Permission", Description = "Membatalkan izin pulang cepat sendiri yang masih diajukan", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("MyEarlyLeavePermission", "Cancel")]
        public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelSelfRequest request, CancellationToken cancellationToken = default)
        {
            var (profileId, error) = await ResolveProfileAsync(cancellationToken);
            if (error != null) return error;

            var result = await _service.CancelBySelfAsync(id, profileId, request.Reason, CurrentUserId(), cancellationToken);
            if (result.Success)
            {
                // Matriks audit 34: Id izin, status sebelum.
                await _loggerService.AuditAsync(LogCategory, "EarlyLeavePermission.SelfCancel",
                    $"Izin pulang cepat {result.Data!.Id} dibatalkan pemohon; status sebelum Submitted, status sekarang {result.Data.Status}.");
            }

            return ToResult(result);
        }

        // VR-PK-34: profil pegawai dari akun login. Akun tanpa profil diteruskan sebagai null; service menjawab 404.
        private async Task<(Guid? ProfileId, IActionResult? Error)> ResolveProfileAsync(CancellationToken cancellationToken)
        {
            try
            {
                var context = await _contextService.GetCurrentAsync(cancellationToken);
                return (context.WorkforceProfileId, null);
            }
            catch (UnauthorizedAccessException ex)
            {
                return (null, Unauthorized(ApiResponse<object>.Fail(StatusCodes.Status401Unauthorized, ex.Message)));
            }
        }

        private IActionResult ToResult(EarlyLeavePermissionServiceResult<EarlyLeavePermissionResponse> result) =>
            result.Success
                ? Ok(ApiResponse<EarlyLeaveSelfResponse>.Ok(EarlyLeaveSelfResponse.From(result.Data!), result.Message))
                : Fail(result.StatusCode, result.Message);

        private ObjectResult Fail(int statusCode, string message) =>
            StatusCode(statusCode, ApiResponse<object>.Fail(statusCode, message));

        private Guid CurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}
