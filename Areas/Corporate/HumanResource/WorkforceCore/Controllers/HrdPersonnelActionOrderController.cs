using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/personnel-action-orders")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFORCE_CORE",
        moduleName: "Human Resource Workforce Core",
        displayName: "Personnel Action Order Administration",
        AreaName = "Corporate",
        ControllerName = "HrdPersonnelActionOrder",
        Description = "Unified SK Kepegawaian (Mutasi, Promosi, Rotasi, Demosi, Penugasan Sementara)",
        SortOrder = 21)]
    [Tags("Corporate / Human Resource / Workforce Core / Personnel Action Order")]
    public class HrdPersonnelActionOrderController : ControllerBase
    {
        private readonly IHrdPersonnelActionOrderService _orderService;
        private static readonly string[] AllowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png" };
        private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

        public HrdPersonnelActionOrderController(IHrdPersonnelActionOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<PersonnelActionOrderFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Personnel Action Order",
            Description = "Melihat metadata filter SK kepegawaian",
            AccessType = AccessTypes.Read,
            SortOrder = 1)]
        [AccessPermission("HrdPersonnelActionOrder", "Read")]
        public async Task<IActionResult> GetFilterMetadata(CancellationToken ct)
        {
            var result = await _orderService.GetFilterMetadataAsync(ct);
            return Ok(ApiResponse<PersonnelActionOrderFilterMetadataResponse>.Ok(result, "Metadata filter berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<PersonnelActionOrderSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Personnel Action Order",
            Description = "Melihat ringkasan metrik SK kepegawaian",
            AccessType = AccessTypes.Read,
            SortOrder = 2)]
        [AccessPermission("HrdPersonnelActionOrder", "Read")]
        public async Task<IActionResult> GetSummary(CancellationToken ct)
        {
            var result = await _orderService.GetSummaryAsync(ct);
            return Ok(ApiResponse<PersonnelActionOrderSummaryResponse>.Ok(result, "Ringkasan SK berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<PersonnelActionOrderListResponse>>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Personnel Action Order",
            Description = "Melihat daftar SK kepegawaian",
            AccessType = AccessTypes.Read,
            SortOrder = 3)]
        [AccessPermission("HrdPersonnelActionOrder", "Read")]
        public async Task<IActionResult> GetOrders([FromQuery] PersonnelActionOrderQuery query, CancellationToken ct)
        {
            var result = await _orderService.GetOrdersAsync(
                query.PageNumber,
                query.PageSize,
                query.ActionCategory,
                query.OrderStatus,
                query.OrganizationUnitId,
                query.StartDate,
                query.EndDate,
                query.Search,
                ct);
            return Ok(ApiResponse<PagedResult<PersonnelActionOrderListResponse>>.Ok(result, "Daftar SK berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PersonnelActionOrderDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Personnel Action Order",
            Description = "Melihat detail SK kepegawaian",
            AccessType = AccessTypes.Read,
            SortOrder = 4)]
        [AccessPermission("HrdPersonnelActionOrder", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var result = await _orderService.GetOrderByIdAsync(id, ct);
            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Dokumen SK tidak ditemukan."));
            }

            return Ok(ApiResponse<PersonnelActionOrderDetailResponse>.Ok(result, "Detail SK berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<PersonnelActionOrderDetailResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(
            "Create",
            "Create Personnel Action Order",
            Description = "Membuat draft SK kepegawaian baru",
            AccessType = AccessTypes.Create,
            SortOrder = 5)]
        [AccessPermission("HrdPersonnelActionOrder", "Create")]
        public async Task<IActionResult> CreateDraft([FromBody] CreatePersonnelActionOrderRequest request, CancellationToken ct)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized(ApiResponse<object>.Fail(StatusCodes.Status401Unauthorized, "Pengguna tidak terautentikasi."));
            }

            try
            {
                var result = await _orderService.CreateDraftOrderAsync(request, userId, ct);
                return StatusCode(StatusCodes.Status201Created,
                    ApiResponse<PersonnelActionOrderDetailResponse>.Ok(result, "Draft SK berhasil dibuat."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PersonnelActionOrderDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(
            "Update",
            "Update Personnel Action Order",
            Description = "Memperbarui draft SK kepegawaian",
            AccessType = AccessTypes.Update,
            SortOrder = 6)]
        [AccessPermission("HrdPersonnelActionOrder", "Update")]
        public async Task<IActionResult> UpdateDraft(Guid id, [FromBody] UpdatePersonnelActionOrderRequest request, CancellationToken ct)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized(ApiResponse<object>.Fail(StatusCodes.Status401Unauthorized, "Pengguna tidak terautentikasi."));
            }

            try
            {
                var result = await _orderService.UpdateDraftOrderAsync(id, request, userId, ct);
                return Ok(ApiResponse<PersonnelActionOrderDetailResponse>.Ok(result, "Draft SK berhasil diperbarui."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
        }

        [HttpPost("{id:guid}/attachments")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<AttachmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(
            "Update",
            "Update Personnel Action Order",
            Description = "Mengunggah pindaian fisik dokumen SK fisik bertanda tangan",
            AccessType = AccessTypes.Update,
            SortOrder = 7)]
        [AccessPermission("HrdPersonnelActionOrder", "Update")]
        public async Task<IActionResult> UploadAttachment(Guid id, IFormFile? file, CancellationToken ct)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized(ApiResponse<object>.Fail(StatusCodes.Status401Unauthorized, "Pengguna tidak terautentikasi."));
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Berkas fisik SK wajib diunggah."));
            }

            if (file.Length > MaxFileSizeBytes)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Ukuran berkas melebihi batas maksimum 10 MB."));
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest,
                    $"Format berkas tidak diizinkan. Ekstensi yang didukung: {string.Join(", ", AllowedExtensions)}"));
            }

            var relativeDir = Path.Combine("Uploads", "PersonnelActionOrders", id.ToString("N"));
            var targetDir = Path.Combine(Directory.GetCurrentDirectory(), relativeDir);
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            var uniqueFileName = $"{Guid.NewGuid():N}{extension}";
            var physicalPath = Path.Combine(targetDir, uniqueFileName);
            var storagePath = Path.Combine(relativeDir, uniqueFileName).Replace('\\', '/');

            using (var stream = new FileStream(physicalPath, FileMode.Create))
            {
                await file.CopyToAsync(stream, ct);
            }

            try
            {
                var result = await _orderService.UploadAttachmentAsync(
                    id,
                    uniqueFileName,
                    file.FileName,
                    file.ContentType,
                    file.Length,
                    storagePath,
                    userId,
                    ct);

                return Ok(ApiResponse<AttachmentResponse>.Ok(result, "Pindaian fisik SK berhasil diunggah."));
            }
            catch (Exception ex)
            {
                if (System.IO.File.Exists(physicalPath))
                {
                    System.IO.File.Delete(physicalPath);
                }
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
        }

        [HttpGet("{id:guid}/attachments/{attachmentId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Personnel Action Order",
            Description = "Mengunduh pindaian fisik dokumen SK",
            AccessType = AccessTypes.Read,
            SortOrder = 8)]
        [AccessPermission("HrdPersonnelActionOrder", "Read")]
        public async Task<IActionResult> DownloadAttachment(Guid id, Guid attachmentId, CancellationToken ct)
        {
            var fileResult = await _orderService.GetAttachmentFileAsync(id, attachmentId, ct);
            if (fileResult == null || !System.IO.File.Exists(fileResult.Value.PhysicalPath))
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Berkas pindaian SK tidak ditemukan."));
            }

            return PhysicalFile(fileResult.Value.PhysicalPath, fileResult.Value.ContentType, fileResult.Value.FileName);
        }

        [HttpPost("{id:guid}/submit")]
        [ProducesResponseType(typeof(ApiResponse<PersonnelActionOrderDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(
            "Update",
            "Update Personnel Action Order",
            Description = "Mengajukan draft SK untuk persetujuan berjenjang",
            AccessType = AccessTypes.Update,
            SortOrder = 9)]
        [AccessPermission("HrdPersonnelActionOrder", "Update")]
        public async Task<IActionResult> SubmitOrder(Guid id, CancellationToken ct)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized(ApiResponse<object>.Fail(StatusCodes.Status401Unauthorized, "Pengguna tidak terautentikasi."));
            }

            try
            {
                var result = await _orderService.SubmitOrderAsync(id, userId, ct);
                return Ok(ApiResponse<PersonnelActionOrderDetailResponse>.Ok(result, "SK kepegawaian berhasil diajukan untuk persetujuan."));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
        }

        [HttpPost("{id:guid}/approve")]
        [ProducesResponseType(typeof(ApiResponse<PersonnelActionOrderDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(
            "Approve",
            "Approve Personnel Action Order",
            Description = "Menyetujui SK kepegawaian dan menerapkan penempatan atomik",
            AccessType = AccessTypes.Update,
            SortOrder = 10)]
        [AccessPermission("HrdPersonnelActionOrder", "Update")]
        public async Task<IActionResult> ApproveOrder(Guid id, CancellationToken ct)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized(ApiResponse<object>.Fail(StatusCodes.Status401Unauthorized, "Pengguna tidak terautentikasi."));
            }

            try
            {
                var result = await _orderService.ApproveOrderAsync(id, userId, ct);
                return Ok(ApiResponse<PersonnelActionOrderDetailResponse>.Ok(result, "SK kepegawaian berhasil disetujui dan riwayat penempatan diperbarui."));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
        }

        [HttpPost("{id:guid}/reject")]
        [ProducesResponseType(typeof(ApiResponse<PersonnelActionOrderDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(
            "Reject",
            "Reject Personnel Action Order",
            Description = "Menolak pengajuan SK kepegawaian",
            AccessType = AccessTypes.Update,
            SortOrder = 11)]
        [AccessPermission("HrdPersonnelActionOrder", "Update")]
        public async Task<IActionResult> RejectOrder(Guid id, [FromBody] RejectOrderRequest request, CancellationToken ct)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized(ApiResponse<object>.Fail(StatusCodes.Status401Unauthorized, "Pengguna tidak terautentikasi."));
            }

            try
            {
                var result = await _orderService.RejectOrderAsync(id, request.Reason, userId, ct);
                return Ok(ApiResponse<PersonnelActionOrderDetailResponse>.Ok(result, "SK kepegawaian berhasil ditolak."));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
        }

        [HttpPost("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<PersonnelActionOrderDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(
            "Cancel",
            "Cancel Personnel Action Order",
            Description = "Membatalkan SK kepegawaian",
            AccessType = AccessTypes.Update,
            SortOrder = 12)]
        [AccessPermission("HrdPersonnelActionOrder", "Update")]
        public async Task<IActionResult> CancelOrder(Guid id, [FromBody] CancelOrderRequest request, CancellationToken ct)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized(ApiResponse<object>.Fail(StatusCodes.Status401Unauthorized, "Pengguna tidak terautentikasi."));
            }

            try
            {
                var result = await _orderService.CancelOrderAsync(id, request.Reason, request.CounterOrderNumber, userId, ct);
                return Ok(ApiResponse<PersonnelActionOrderDetailResponse>.Ok(result, "SK kepegawaian berhasil dibatalkan."));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
        }

        [HttpPost("{id:guid}/cancel-request")]
        [ProducesResponseType(typeof(ApiResponse<PersonnelActionOrderDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(
            "Cancel",
            "Cancel Personnel Action Order",
            Description = "Membatalkan permohonan SK kepegawaian",
            AccessType = AccessTypes.Update,
            SortOrder = 13)]
        [AccessPermission("HrdPersonnelActionOrder", "Update")]
        public Task<IActionResult> CancelRequest(Guid id, [FromBody] CancelOrderRequest request, CancellationToken ct)
        {
            return CancelOrder(id, request, ct);
        }

        private bool TryGetCurrentUserId(out Guid userId)
        {
            var value = User.FindFirstValue("user_id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out userId) && userId != Guid.Empty;
        }
    }
}
