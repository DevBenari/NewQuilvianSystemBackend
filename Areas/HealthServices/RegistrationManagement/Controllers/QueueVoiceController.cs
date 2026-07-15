using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/registration-management/queue-voice")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_REGISTRATION_MANAGEMENT",
        moduleName: "Health Service Registration Management",
        displayName: "Queue Voice",
        AreaName = "HealthServices",
        ControllerName = "QueueVoice",
        Description = "Audio suara panggilan antrean lintas modul",
        SortOrder = 7
    )]
    [Tags("Health Services / Registration Management / Queue Voice")]
    public class QueueVoiceController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly QueueVoiceService _queueVoiceService;

        public QueueVoiceController(
            ApplicationDbContext dbContext,
            QueueVoiceService queueVoiceService)
        {
            _dbContext = dbContext;
            _queueVoiceService = queueVoiceService;
        }

        [HttpGet("audio/{dateKey}/{fileName}")]
        [Produces("audio/mpeg")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Queue Voice Audio", Description = "Memutar audio panggilan antrean", AccessType = AccessTypes.Read, SortOrder = 1)]
        public IActionResult GetAudio(string dateKey, string fileName)
        {
            var filePath = _queueVoiceService.ResolveAudioPath(dateKey, fileName);
            if (filePath == null || !System.IO.File.Exists(filePath))
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "File audio panggilan tidak ditemukan atau sudah dibersihkan oleh sistem."));
            }

            return PhysicalFile(filePath, "audio/mpeg", enableRangeProcessing: true);
        }

        [HttpGet("download/{dateKey}/{fileName}")]
        [Produces("audio/mpeg")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Download Queue Voice Audio", Description = "Mengunduh audio panggilan antrean", AccessType = AccessTypes.Read, SortOrder = 2)]
        public IActionResult DownloadAudio(string dateKey, string fileName)
        {
            var filePath = _queueVoiceService.ResolveAudioPath(dateKey, fileName);
            if (filePath == null || !System.IO.File.Exists(filePath))
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "File audio panggilan tidak ditemukan atau sudah dibersihkan oleh sistem."));
            }

            return PhysicalFile(filePath, "audio/mpeg", fileDownloadName: fileName, enableRangeProcessing: true);
        }

        [HttpGet("profiles")]
        [ProducesResponseType(typeof(ApiResponse<List<QueueVoiceProfileResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Queue Voice Profiles", Description = "Melihat daftar profil suara panggilan antrean", AccessType = AccessTypes.Read, SortOrder = 3)]
        [AccessPermission("QueueVoice", "Read")]
        public async Task<IActionResult> GetProfiles()
        {
            var result = await _queueVoiceService.GetAvailableVoiceProfilesAsync();
            return Ok(ApiResponse<List<QueueVoiceProfileResponse>>.Ok(result, "Daftar profil suara berhasil diambil."));
        }

        [HttpPost("preview")]
        [ProducesResponseType(typeof(ApiResponse<QueueVoiceGenerateResponse>), StatusCodes.Status200OK)]
        [AccessAction("Create", "Preview Queue Voice", Description = "Membuat preview audio suara panggilan antrean", AccessType = AccessTypes.Create, SortOrder = 4)]
        [AccessPermission("QueueVoice", "Create")]
        public async Task<IActionResult> Preview([FromBody] QueueVoicePreviewRequest request)
        {
            var result = await _queueVoiceService.GeneratePreviewAudioAsync(request ?? new QueueVoicePreviewRequest());
            return Ok(ApiResponse<QueueVoiceGenerateResponse>.Ok(
                result,
                result.ErrorMessage ?? "Preview audio panggilan berhasil dibuat."));
        }

        [HttpPost("queues/{queueId:guid}/regenerate")]
        [ProducesResponseType(typeof(ApiResponse<QueueVoiceGenerateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Regenerate Queue Voice", Description = "Generate ulang audio panggilan antrean", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("QueueVoice", "Update")]
        public async Task<IActionResult> RegenerateQueueVoice(Guid queueId, [FromBody] QueueVoiceRegenerateRequest? request = null)
        {
            var queue = await _dbContext.Set<TrxQueue>()
                .Include(x => x.Patient)
                .Include(x => x.ServiceUnit)
                .Include(x => x.Clinic)
                .Include(x => x.Doctor)
                .FirstOrDefaultAsync(x => x.Id == queueId && !x.IsDelete && x.IsActive);

            if (queue == null)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Antrean tidak ditemukan."));
            }

            var callType = string.IsNullOrWhiteSpace(request?.CallType) ? QueueVoiceCallTypes.General : request!.CallType!;
            var forceRegenerate = request?.ForceRegenerate ?? true;
            var result = await _queueVoiceService.GetOrCreateQueueCallAudioAsync(
                queue,
                callType,
                forceRegenerate,
                overrideVoiceCode: request?.VoiceCode);

            return Ok(ApiResponse<QueueVoiceGenerateResponse>.Ok(
                result,
                result.ErrorMessage ?? "Audio panggilan berhasil dibuat ulang."));
        }
    }
}
