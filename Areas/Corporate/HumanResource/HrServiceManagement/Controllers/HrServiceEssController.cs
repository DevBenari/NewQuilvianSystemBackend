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
    [Route("api/v1/self-service/hr/services/tickets")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_SERVICE_MANAGEMENT",
        moduleName: "Human Resource Service Management",
        displayName: "Layanan Kepegawaian Mandiri (ESS)",
        AreaName = "Corporate",
        ControllerName = "HrServiceEss",
        Description = "Pengajuan tiket layanan SDM oleh pegawai, utas klarifikasi, dan lampiran",
        SortOrder = 13)]
    [Tags("HR Services - ESS Requests & My Documents")]
    [Authorize]
    public class HrServiceEssController : ControllerBase
    {
        private readonly IHrServiceTicketService _ticketService;

        public HrServiceEssController(IHrServiceTicketService ticketService)
        {
            _ticketService = ticketService;
        }

        private Guid GetCurrentUserWorkforceProfileId()
        {
            var claim = User.FindFirst("WorkforceProfileId")?.Value 
                     ?? User.FindFirst("EmployeeProfileId")?.Value
                     ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return Guid.TryParse(claim, out var guid) ? guid : Guid.Empty;
        }

        /// <summary>
        /// Mengajukan tiket permohonan layanan SDM mandiri oleh pegawai melalui portal ESS (FR-HRS-06, HRS-B1)
        /// </summary>
        [HttpPost]
        [AccessAction("CreateRequest", "Mengajukan Tiket Layanan Mandiri", AccessType = AccessTypes.Create, SortOrder = 1)]
        [AccessPermission("HrServiceEss", "CreateRequest")]
        public async Task<IActionResult> SubmitTicket([FromBody] CreateHrServiceTicketRequestDto request, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var profileId = GetCurrentUserWorkforceProfileId();
            if (profileId == Guid.Empty)
            {
                return BadRequest(new { message = "Klaim identitas profil pegawai tidak ditemukan dalam token otentikasi" });
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "ESS_USER";

            try
            {
                var created = await _ticketService.SubmitTicketEssAsync(profileId, request, currentUserId, cancellationToken);
                return CreatedAtAction(nameof(GetMyTicketDetail), new { id = created.Id }, created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Mengambil daftar seluruh riwayat tiket permohonan layanan yang diajukan oleh pegawai yang sedang login (FR-HRS-08)
        /// </summary>
        [HttpGet("my-tickets")]
        [AccessAction("ViewOwn", "Melihat Tiket Layanan Sendiri", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("HrServiceEss", "ViewOwn")]
        public async Task<IActionResult> GetMyTickets(CancellationToken cancellationToken = default)
        {
            var profileId = GetCurrentUserWorkforceProfileId();
            if (profileId == Guid.Empty)
            {
                return BadRequest(new { message = "Identitas profil pegawai tidak valid" });
            }

            var tickets = await _ticketService.GetMyTicketsAsync(profileId, cancellationToken);
            return Ok(tickets);
        }

        /// <summary>
        /// Mengambil detail status tiket permohonan milik pegawai
        /// </summary>
        [HttpGet("{id:guid}")]
        [AccessAction("ViewOwn", "Melihat Tiket Layanan Sendiri", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("HrServiceEss", "ViewOwn")]
        public async Task<IActionResult> GetMyTicketDetail(Guid id, CancellationToken cancellationToken = default)
        {
            var profileId = GetCurrentUserWorkforceProfileId();
            var ticket = await _ticketService.GetTicketDetailAsync(id, requestingWorkforceProfileId: profileId, cancellationToken: cancellationToken);

            if (ticket == null) return NotFound(new { message = "Tiket tidak ditemukan atau bukan milik Anda" });
            return Ok(ticket);
        }

        /// <summary>
        /// Mengambil percakapan klarifikasi tiket (catatan internal analis disembunyikan secara otomatis)
        /// </summary>
        [HttpGet("{id:guid}/comments")]
        [AccessAction("ViewOwn", "Melihat Tiket Layanan Sendiri", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("HrServiceEss", "ViewOwn")]
        public async Task<IActionResult> GetComments(Guid id, CancellationToken cancellationToken = default)
        {
            var comments = await _ticketService.GetCommentsAsync(id, isHrStaff: false, cancellationToken);
            return Ok(comments);
        }

        /// <summary>
        /// Mengirimkan pesan atau balasan klarifikasi berkas dari pegawai kepada analis SDM
        /// </summary>
        [HttpPost("{id:guid}/comments")]
        [AccessAction("CreateRequest", "Mengajukan Tiket Layanan Mandiri", AccessType = AccessTypes.Create, SortOrder = 1)]
        [AccessPermission("HrServiceEss", "CreateRequest")]
        public async Task<IActionResult> AddComment(Guid id, [FromBody] AddTicketCommentRequestDto request, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString();
            var authorGuid = Guid.TryParse(userIdString, out var guid) ? guid : Guid.Empty;
            var authorName = ResolveActorName();

            try
            {
                // Pegawai tidak diizinkan mencentang isInternalNote
                request.IsInternalNote = false;

                var comment = await _ticketService.AddCommentAsync(id, request, authorGuid, authorName, isHrStaff: false, cancellationToken);
                return Ok(comment);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Mengambil daftar berkas lampiran pada tiket permohonan milik pegawai (BE-HRD-196)
        /// </summary>
        [HttpGet("{id:guid}/attachments")]
        [AccessAction("ViewOwn", "Melihat Tiket Layanan Sendiri", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("HrServiceEss", "ViewOwn")]
        public async Task<IActionResult> GetAttachments(Guid id, CancellationToken cancellationToken = default)
        {
            var attachments = await _ticketService.GetAttachmentsAsync(id, isHrStaff: false, cancellationToken);
            return Ok(attachments);
        }

        /// <summary>
        /// Mengunggah berkas lampiran pendukung atau berkas klarifikasi oleh pegawai (maks 5MB, format PDF/JPG/PNG/DOCX)
        /// </summary>
        [HttpPost("{id:guid}/attachments")]
        [AccessAction("CreateRequest", "Mengajukan Tiket Layanan Mandiri", AccessType = AccessTypes.Create, SortOrder = 1)]
        [AccessPermission("HrServiceEss", "CreateRequest")]
        public async Task<IActionResult> UploadAttachment(Guid id, IFormFile file, [FromForm] string? description, CancellationToken cancellationToken = default)
        {
            var profileId = GetCurrentUserWorkforceProfileId();
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString();
            var authorGuid = Guid.TryParse(userIdString, out var guid) ? guid : Guid.Empty;

            try
            {
                var attachment = await _ticketService.UploadAttachmentAsync(id, file, description, authorGuid, workforceProfileId: profileId, cancellationToken);
                return Ok(attachment);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Mengunduh berkas lampiran pada tiket milik pegawai
        /// </summary>
        [HttpGet("{id:guid}/attachments/{attachmentId:guid}/download")]
        [AccessAction("ViewOwn", "Melihat Tiket Layanan Sendiri", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("HrServiceEss", "ViewOwn")]
        public async Task<IActionResult> DownloadAttachment(Guid id, Guid attachmentId, CancellationToken cancellationToken = default)
        {
            var profileId = GetCurrentUserWorkforceProfileId();

            try
            {
                var (bytes, contentType, fileName) = await _ticketService.DownloadAttachmentAsync(id, attachmentId, requestingWorkforceProfileId: profileId, isHrStaff: false, cancellationToken);
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
