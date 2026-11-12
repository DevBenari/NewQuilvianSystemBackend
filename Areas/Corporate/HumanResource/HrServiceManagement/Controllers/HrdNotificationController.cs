using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Shared.HumanResource.Helpers;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Controllers
{
    /// <summary>
    /// Pusat pemberitahuan dalam aplikasi milik pengguna yang sedang masuk (BE-HRD-314, WF-B7, INT-HR-95).
    /// </summary>
    /// <remarks>
    /// Worklist pribadi: setiap endpoint hanya menyentuh pemberitahuan milik akun pada token. Tidak ada
    /// parameter identitas dari klien, sehingga pemberitahuan orang lain tidak dapat dibaca dengan mengganti ID.
    /// </remarks>
    [ApiController]
    [Route("api/v1/self-service/hr/notifications")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_SERVICE_MANAGEMENT",
        moduleName: "Human Resource Service Management",
        displayName: "Pusat Pemberitahuan Pegawai",
        AreaName = "Corporate",
        ControllerName = "HrdNotification",
        Description = "Pemberitahuan dalam aplikasi untuk pemohon, approver, dan petugas modul SDM",
        SortOrder = 14)]
    [Tags("HR Services - Notification Center")]
    [Authorize]
    public class HrdNotificationController : ControllerBase
    {
        private readonly IHrdNotificationService _notificationService;

        public HrdNotificationController(IHrdNotificationService notificationService)
        {
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        }

        /// <summary>
        /// Daftar pemberitahuan milik saya, terbaru lebih dulu.
        /// </summary>
        [HttpGet]
        [AccessAction("ViewOwn", "Melihat Pemberitahuan Sendiri", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("HrdNotification", "ViewOwn")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<HrdNotificationDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyNotifications(
            [FromQuery] bool? unreadOnly = null,
            [FromQuery] string? category = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var userId = User.RequireUserId();
            var result = await _notificationService.GetMyNotificationsAsync(userId, unreadOnly, category, pageNumber, pageSize, cancellationToken);
            return Ok(ApiResponse<PagedResult<HrdNotificationDto>>.Ok(result, "Daftar pemberitahuan berhasil diambil."));
        }

        /// <summary>
        /// Jumlah pemberitahuan yang belum dibaca, untuk lencana lonceng.
        /// </summary>
        [HttpGet("unread-count")]
        [AccessAction("ViewOwn", "Melihat Pemberitahuan Sendiri", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("HrdNotification", "ViewOwn")]
        [ProducesResponseType(typeof(ApiResponse<HrdNotificationUnreadCountDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken = default)
        {
            var userId = User.RequireUserId();
            var result = await _notificationService.GetUnreadCountAsync(userId, cancellationToken);
            return Ok(ApiResponse<HrdNotificationUnreadCountDto>.Ok(result, "Jumlah pemberitahuan belum dibaca berhasil diambil."));
        }

        /// <summary>
        /// Menandai satu pemberitahuan milik saya sebagai sudah dibaca.
        /// </summary>
        [HttpPost("{id:guid}/read")]
        [AccessAction("MarkRead", "Menandai Pemberitahuan Dibaca", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("HrdNotification", "MarkRead")]
        [ProducesResponseType(typeof(ApiResponse<HrdNotificationMarkReadResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MarkRead(Guid id, CancellationToken cancellationToken = default)
        {
            var userId = User.RequireUserId();
            try
            {
                var result = await _notificationService.MarkReadAsync(userId, id, cancellationToken);
                return Ok(ApiResponse<HrdNotificationMarkReadResultDto>.Ok(result, "Pemberitahuan ditandai sudah dibaca."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
        }

        /// <summary>
        /// Menandai seluruh pemberitahuan milik saya sebagai sudah dibaca.
        /// </summary>
        [HttpPost("read-all")]
        [AccessAction("MarkRead", "Menandai Pemberitahuan Dibaca", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("HrdNotification", "MarkRead")]
        [ProducesResponseType(typeof(ApiResponse<HrdNotificationMarkReadResultDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken = default)
        {
            var userId = User.RequireUserId();
            var result = await _notificationService.MarkAllReadAsync(userId, cancellationToken);
            return Ok(ApiResponse<HrdNotificationMarkReadResultDto>.Ok(result, "Seluruh pemberitahuan ditandai sudah dibaca."));
        }
    }
}
