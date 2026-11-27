using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services;
using QuilvianSystemBackend.Areas.SelfServices.HumanResource.DTOs;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Shared.HumanResource.Helpers;
using QuilvianSystemBackend.Shared.HumanResource.Services;

namespace QuilvianSystemBackend.Areas.SelfServices.HumanResource.Controllers
{
    // Layanan Mandiri surat tugas dinas luar (HRD-DEC-252, kontrak v31 API 37.5). Identitas pegawai hanya dari sesi (VR-PK-34);
    // aturan pengajuan sama dengan layar HR lewat OfficialDutyRequestService. Persetujuan tetap di layar HR (HRD-DEC-256).
    [ApiController]
    [Authorize]
    [Route("api/v1/self-services/human-resource/official-duties")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_EMPLOYEE_SELF_SERVICE",
        moduleName: "Human Resource Employee Self Service",
        displayName: "My Official Duty",
        AreaName = "SelfServices",
        ControllerName = "MyOfficialDuty",
        Description = "Pegawai mengajukan, melihat, dan membatalkan surat tugas dinas luar miliknya",
        SortOrder = 14)]
    [Tags("Self Services / Human Resource / Official Duty")]
    public class OfficialDutySelfServiceController : ControllerBase
    {
        private const string LogCategory = "SelfServices.HumanResource.OfficialDuty";

        private readonly OfficialDutyRequestService _service;
        private readonly HumanResourceContextService _contextService;
        private readonly HrAttachmentStorage _attachmentStorage;
        private readonly LoggerService _loggerService;

        public OfficialDutySelfServiceController(
            OfficialDutyRequestService service,
            HumanResourceContextService contextService,
            HrAttachmentStorage attachmentStorage,
            LoggerService loggerService)
        {
            _service = service;
            _contextService = contextService;
            _attachmentStorage = attachmentStorage;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<OfficialDutySelfFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read My Official Duty", Description = "Melihat surat tugas dinas luar milik sendiri", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MyOfficialDuty", "Read")]
        public IActionResult GetFilterMetadata()
        {
            return Ok(ApiResponse<OfficialDutySelfFilterMetadataResponse>.Ok(
                new OfficialDutySelfFilterMetadataResponse
                {
                    DutyCategoryOptions = OfficialDutySelfCategory.All.Select(x => new OfficialDutySelfOptionResponse { Value = x.Value, Label = x.Label }).ToList(),
                    StatusOptions = OfficialDutySelfStatus.All.Select(x => new OfficialDutySelfOptionResponse { Value = x.Value, Label = x.Label }).ToList(),
                    PageSizeOptions = new List<int> { 10, 25, 50, 100 }
                },
                "Metadata surat tugas dinas luar berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<OfficialDutySelfResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read My Official Duty", Description = "Melihat surat tugas dinas luar milik sendiri", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MyOfficialDuty", "Read")]
        public async Task<IActionResult> GetMine([FromQuery] OfficialDutySelfQuery query, CancellationToken cancellationToken = default)
        {
            var (profileId, error) = await ResolveProfileAsync(cancellationToken);
            if (error != null) return error;

            var result = await _service.GetPagedForSelfAsync(profileId, query.FromDate, query.ToDate, query.Status, query.PageNumber, query.PageSize, cancellationToken);
            if (!result.Success) return Fail(result.StatusCode, result.Message);

            var page = result.Data!;
            return Ok(ApiResponse<PagedResult<OfficialDutySelfResponse>>.Ok(
                new PagedResult<OfficialDutySelfResponse>
                {
                    PageNumber = page.PageNumber,
                    PageSize = page.PageSize,
                    TotalData = page.TotalData,
                    TotalPage = page.TotalPage,
                    Items = page.Items.Select(OfficialDutySelfResponse.From).ToList()
                },
                result.Message));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<OfficialDutySelfResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read My Official Duty", Description = "Melihat surat tugas dinas luar milik sendiri", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MyOfficialDuty", "Read")]
        public async Task<IActionResult> GetMineById(Guid id, CancellationToken cancellationToken = default)
        {
            var (profileId, error) = await ResolveProfileAsync(cancellationToken);
            if (error != null) return error;

            return ToResult(await _service.GetByIdForSelfAsync(id, profileId, cancellationToken));
        }

        [HttpPost("attachments")]
        [RequestSizeLimit(HrAttachmentStorage.DefaultMaximumFileSizeBytes + 1024 * 1024)]
        [ProducesResponseType(typeof(ApiResponse<HrStoredAttachment>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Create", "Create My Official Duty", Description = "Mengajukan surat tugas dinas luar untuk diri sendiri", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("MyOfficialDuty", "Create")]
        public async Task<IActionResult> UploadAttachment([FromForm] IFormFile file, CancellationToken cancellationToken = default)
        {
            var (profileId, error) = await ResolveProfileAsync(cancellationToken);
            if (error != null) return error;

            var profile = await _service.EnsureSelfProfileAsync(profileId, cancellationToken);
            if (!profile.Success) return Fail(profile.StatusCode, profile.Message);

            try
            {
                var stored = await _attachmentStorage.SaveAsync(OfficialDutyRequestService.SelfAttachmentCategory, User.GetUserId(), file, cancellationToken);
                return StatusCode(StatusCodes.Status201Created, ApiResponse<HrStoredAttachment>.Ok(stored, "Lampiran berhasil diunggah."));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Fail(StatusCodes.Status401Unauthorized, ex.Message);
            }
            catch (ArgumentException ex)
            {
                return Fail(StatusCodes.Status400BadRequest, ex.Message);
            }
        }

        [HttpGet("attachments/download")]
        [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read My Official Duty", Description = "Melihat surat tugas dinas luar milik sendiri", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MyOfficialDuty", "Read")]
        public async Task<IActionResult> DownloadAttachment([FromQuery] string? url, CancellationToken cancellationToken = default)
        {
            var (profileId, error) = await ResolveProfileAsync(cancellationToken);
            if (error != null) return error;

            var own = await _service.EnsureOwnAttachmentForSelfAsync(profileId, url, cancellationToken);
            if (!own.Success) return Fail(own.StatusCode, own.Message);

            try
            {
                var attachment = _attachmentStorage.Open(url!.Trim(), OfficialDutyRequestService.SelfAttachmentCategory);
                return PhysicalFile(attachment.PhysicalPath, attachment.ContentType, attachment.DownloadName);
            }
            catch (KeyNotFoundException ex)
            {
                return Fail(StatusCodes.Status404NotFound, ex.Message);
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<OfficialDutySelfResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Create", "Create My Official Duty", Description = "Mengajukan surat tugas dinas luar untuk diri sendiri", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("MyOfficialDuty", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateOfficialDutySelfRequest request, CancellationToken cancellationToken = default)
        {
            var (profileId, error) = await ResolveProfileAsync(cancellationToken);
            if (error != null) return error;
            if (!request.StartAt.HasValue || !request.EndAt.HasValue)
            {
                return Fail(StatusCodes.Status400BadRequest, "Waktu mulai dan selesai dinas luar wajib diisi.");
            }

            var result = await _service.CreateForSelfAsync(profileId, new CreateOfficialDutyRequest
            {
                DutyCategory = request.DutyCategory,
                AssignmentOrderNumber = request.AssignmentOrderNumber,
                StartAt = request.StartAt.Value,
                EndAt = request.EndAt.Value,
                Destination = request.Destination,
                Purpose = request.Purpose,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                AttachmentUrl = request.AttachmentUrl
            }, User.GetUserId(), cancellationToken);
            if (result.Success)
            {
                // Matriks audit 35.4: Id surat tugas dan rentang. Tujuan dan uraian tugas tidak dicatat.
                var d = result.Data!;
                await _loggerService.AuditAsync(LogCategory, "OfficialDuty.SelfCreate",
                    $"Surat tugas dinas luar {d.Id} rentang {d.StartAt:yyyy-MM-ddTHH:mm:ss} s.d. {d.EndAt:yyyy-MM-ddTHH:mm:ss} diajukan pemohon; status {d.Status}.");
            }

            return ToResult(result);
        }

        [HttpPost("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<OfficialDutySelfResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Cancel", "Cancel My Official Duty", Description = "Membatalkan surat tugas dinas luar sendiri yang masih diajukan", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("MyOfficialDuty", "Cancel")]
        public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelSelfRequest request, CancellationToken cancellationToken = default)
        {
            var (profileId, error) = await ResolveProfileAsync(cancellationToken);
            if (error != null) return error;

            var result = await _service.CancelBySelfAsync(id, profileId, request.Reason, User.GetUserId(), cancellationToken);
            if (result.Success)
            {
                // Matriks audit 35.4: Id surat tugas dan rentang.
                var d = result.Data!;
                await _loggerService.AuditAsync(LogCategory, "OfficialDuty.SelfCancel",
                    $"Surat tugas dinas luar {d.Id} rentang {d.StartAt:yyyy-MM-ddTHH:mm:ss} s.d. {d.EndAt:yyyy-MM-ddTHH:mm:ss} dibatalkan pemohon; status sebelum Submitted, status sekarang {d.Status}.");
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

        private IActionResult ToResult(OfficialDutyRequestServiceResult<OfficialDutyResponse> result) =>
            result.Success
                ? Ok(ApiResponse<OfficialDutySelfResponse>.Ok(OfficialDutySelfResponse.From(result.Data!), result.Message))
                : Fail(result.StatusCode, result.Message);

        private ObjectResult Fail(int statusCode, string message) =>
            StatusCode(statusCode, ApiResponse<object>.Fail(statusCode, message));
    }
}
