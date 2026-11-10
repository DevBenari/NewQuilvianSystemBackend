using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Services;
using System.Security.Claims;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Controllers
{
    [ApiController]
    [Route("api/v1/corporate/hrd/services/tickets")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_SERVICE_MANAGEMENT",
        moduleName: "Human Resource Service Management",
        displayName: "Antrean Tiket Layanan SDM",
        AreaName = "Corporate",
        ControllerName = "HrServiceTicket",
        Description = "Antrean helpdesk SDM, penugasan analis, pemrosesan tiket, dan riwayat perubahan profil",
        SortOrder = 12)]
    [Tags("HR Services - Tickets & Queue")]
    [Authorize(Roles = "SuperAdmin,HRAdmin,HRManager,HRStaff")]
    public class HrServiceTicketController : ControllerBase
    {
        private readonly IHrServiceTicketService _ticketService;
        private readonly IWorkforceProfileSyncService _profileSyncService;

        public HrServiceTicketController(
            IHrServiceTicketService ticketService,
            IWorkforceProfileSyncService profileSyncService)
        {
            _ticketService = ticketService;
            _profileSyncService = profileSyncService;
        }

        /// <summary>
        /// Mengambil antrean seluruh tiket permohonan layanan SDM dengan filter status & prioritas (HRS-W1)
        /// </summary>
        [HttpGet("queue")]
        [AccessAction("Read", "Melihat Antrean Tiket Layanan SDM", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("HrServiceTicket", "Read")]
        public async Task<IActionResult> GetTicketsQueue(
            [FromQuery] string? status = null,
            [FromQuery] string? priorityTier = null,
            [FromQuery] Guid? assignedToUserId = null,
            CancellationToken cancellationToken = default)
        {
            var tickets = await _ticketService.GetAllTicketsQueueAsync(status, priorityTier, assignedToUserId, cancellationToken);
            return Ok(tickets);
        }

        /// <summary>
        /// Mengambil rincian data tiket helpdesk SDM
        /// </summary>
        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Melihat Antrean Tiket Layanan SDM", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("HrServiceTicket", "Read")]
        public async Task<IActionResult> GetTicketDetail(Guid id, CancellationToken cancellationToken = default)
        {
            var ticket = await _ticketService.GetTicketDetailAsync(id, cancellationToken: cancellationToken);
            if (ticket == null) return NotFound(new { message = "Tiket bantuan tidak ditemukan" });
            return Ok(ticket);
        }

        /// <summary>
        /// Menugaskan tiket antrean ke analis SDM spesifik (Claim / Re-assign)
        /// </summary>
        [HttpPost("{id:guid}/assign")]
        [AccessAction("Assign", "Menugaskan Tiket Layanan SDM", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("HrServiceTicket", "Assign")]
        public async Task<IActionResult> AssignTicket(Guid id, [FromBody] AssignTicketRequestDto request, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "SYSTEM";
            try
            {
                var ticket = await _ticketService.AssignTicketAsync(id, request, currentUserId, cancellationToken);
                return Ok(ticket);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Memperbarui status progres tiket SDM (InReview, WaitingStaffResponse, Resolved, Closed, Cancelled)
        /// </summary>
        [HttpPost("{id:guid}/status")]
        [AccessAction("Process", "Memproses Tiket Layanan SDM", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("HrServiceTicket", "Process")]
        public async Task<IActionResult> UpdateTicketStatus(Guid id, [FromBody] UpdateTicketStatusRequestDto request, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "SYSTEM";
            try
            {
                var ticket = await _ticketService.UpdateTicketStatusAsync(id, request, currentUserId, cancellationToken);
                return Ok(ticket);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Mengambil utas percakapan dan catatan klarifikasi pada tiket
        /// </summary>
        [HttpGet("{id:guid}/comments")]
        [AccessAction("Read", "Melihat Antrean Tiket Layanan SDM", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("HrServiceTicket", "Read")]
        public async Task<IActionResult> GetTicketComments(Guid id, CancellationToken cancellationToken = default)
        {
            var comments = await _ticketService.GetCommentsAsync(id, isHrStaff: true, cancellationToken);
            return Ok(comments);
        }

        /// <summary>
        /// Menambahkan komentar atau catatan internal analis HR pada tiket (HRS-W2)
        /// </summary>
        [HttpPost("{id:guid}/comments")]
        [AccessAction("Process", "Memproses Tiket Layanan SDM", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("HrServiceTicket", "Process")]
        public async Task<IActionResult> AddTicketComment(Guid id, [FromBody] AddTicketCommentRequestDto request, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString();
            var authorGuid = Guid.TryParse(userIdString, out var guid) ? guid : Guid.Empty;
            var authorName = ResolveActorName();

            try
            {
                var comment = await _ticketService.AddCommentAsync(id, request, authorGuid, authorName, isHrStaff: true, cancellationToken);
                return Ok(comment);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Mengambil daftar berkas lampiran pendukung pada tiket helpdesk (BE-HRD-196)
        /// </summary>
        [HttpGet("{id:guid}/attachments")]
        [AccessAction("Read", "Melihat Antrean Tiket Layanan SDM", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("HrServiceTicket", "Read")]
        public async Task<IActionResult> GetAttachments(Guid id, CancellationToken cancellationToken = default)
        {
            var attachments = await _ticketService.GetAttachmentsAsync(id, isHrStaff: true, cancellationToken);
            return Ok(attachments);
        }

        /// <summary>
        /// Mengunggah berkas lampiran pendukung oleh analis HR (maks 5MB, format PDF/JPG/PNG/DOCX)
        /// </summary>
        [HttpPost("{id:guid}/attachments")]
        [AccessAction("Process", "Memproses Tiket Layanan SDM", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("HrServiceTicket", "Process")]
        public async Task<IActionResult> UploadAttachment(Guid id, IFormFile file, [FromForm] string? description, CancellationToken cancellationToken = default)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString();
            var authorGuid = Guid.TryParse(userIdString, out var guid) ? guid : Guid.Empty;

            try
            {
                var attachment = await _ticketService.UploadAttachmentAsync(id, file, description, authorGuid, workforceProfileId: null, cancellationToken);
                return Ok(attachment);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Mengunduh berkas fisik lampiran pendukung
        /// </summary>
        [HttpGet("{id:guid}/attachments/{attachmentId:guid}/download")]
        [AccessAction("Read", "Melihat Antrean Tiket Layanan SDM", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("HrServiceTicket", "Read")]
        public async Task<IActionResult> DownloadAttachment(Guid id, Guid attachmentId, CancellationToken cancellationToken = default)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString();
            var userGuid = Guid.TryParse(userIdString, out var guid) ? guid : Guid.Empty;

            try
            {
                var (bytes, contentType, fileName) = await _ticketService.DownloadAttachmentAsync(id, attachmentId, requestingWorkforceProfileId: null, isHrStaff: true, cancellationToken);
                return File(bytes, contentType, fileName);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (FileNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Mengambil riwayat snapshot pemutakhiran profil tenaga kerja yang dieksekusi otomatis dari tiket sah (INT-HR-65)
        /// </summary>
        [HttpGet("{id:guid}/profile-change-history")]
        [AccessAction("Read", "Melihat Antrean Tiket Layanan SDM", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("HrServiceTicket", "Read")]
        public async Task<IActionResult> GetProfileChangeHistoryByTicket(Guid id, CancellationToken cancellationToken = default)
        {
            var history = await _profileSyncService.GetChangeHistoryByTicketAsync(id, cancellationToken);
            return Ok(history);
        }

        /// <summary>
        /// Mengambil riwayat snapshot pemutakhiran profil untuk tenaga kerja spesifik (INT-HR-65)
        /// </summary>
        [HttpGet("employee/{employeeId:guid}/profile-change-history")]
        [AccessAction("Read", "Melihat Antrean Tiket Layanan SDM", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("HrServiceTicket", "Read")]
        public async Task<IActionResult> GetProfileChangeHistoryByEmployee(Guid employeeId, CancellationToken cancellationToken = default)
        {
            var history = await _profileSyncService.GetChangeHistoryByEmployeeAsync(employeeId, cancellationToken);
            return Ok(history);
        }

        /// <summary>
        /// Nama pengguna yang sedang login untuk jejak aktor. Sengaja tidak memakai aktor cadangan:
        /// permintaan tanpa identitas ditolak, bukan dicatat atas nama orang lain.
        /// </summary>
        private string ResolveActorName()
        {
            var actorName = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(actorName))
            {
                throw new UnauthorizedAccessException("Identitas user pada token tidak valid.");
            }

            return actorName;
        }
    }
}
