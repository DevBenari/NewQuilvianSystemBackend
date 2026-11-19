using System;
using System.Linq;
using QuilvianSystemBackend.Services.Security;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Services;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Shared.HumanResource.Helpers;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/hrd/travel/settlements")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_BUSINESS_TRAVEL",
        moduleName: "Human Resource Business Travel",
        displayName: "Pertanggungjawaban Perjalanan Dinas",
        AreaName = "Corporate",
        ControllerName = "BusinessTravelSettlement",
        Description = "Laporan pertanggungjawaban biaya, verifikasi kuitansi, dan penutupan selisih kas",
        SortOrder = 12)]
    [Tags("Corporate - HRD Business Travel Settlements")]
    public class BusinessTravelSettlementsController : ControllerBase
    {
        private readonly ITravelSettlementService _settlementService;
        private readonly IMissingReceiptDeclarationService _missingReceiptService;

        public BusinessTravelSettlementsController(ITravelSettlementService settlementService, IMissingReceiptDeclarationService missingReceiptService)
        {
            _settlementService = settlementService;
            _missingReceiptService = missingReceiptService;
        }

        /// <summary>
        /// Daftar deklarasi biaya darurat tanpa nota pada satu settlement.
        /// </summary>
        [HttpGet("{id}/missing-receipts")]
        [AccessAction("Read", "Melihat Settlement Dinas", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BusinessTravelSettlement", "Read")]
        [ProducesResponseType(typeof(ApiResponse<List<MissingReceiptDeclarationDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMissingReceipts([FromRoute] Guid id)
        {
            try
            {
                var result = await _missingReceiptService.GetDeclarationsAsync(id);
                return Ok(ApiResponse<List<MissingReceiptDeclarationDto>>.Ok(result, "Daftar deklarasi tanpa nota berhasil diambil"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<List<MissingReceiptDeclarationDto>>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
        }

        /// <summary>
        /// Mengajukan deklarasi biaya darurat tanpa nota (VR-TRV-08, VR-TRV-09).
        /// </summary>
        [HttpPost("{id}/missing-receipts")]
        [AccessAction("DeclareMissingReceipt", "Mengajukan Deklarasi Tanpa Nota", AccessType = AccessTypes.Create, SortOrder = 6)]
        [AccessPermission("BusinessTravelSettlement", "DeclareMissingReceipt")]
        [ProducesResponseType(typeof(ApiResponse<MissingReceiptDeclarationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeclareMissingReceipt([FromRoute] Guid id, [FromBody] MissingReceiptItemDto dto)
        {
            try
            {
                var result = await _missingReceiptService.DeclareAsync(id, dto, GetCurrentUserId());
                return Ok(ApiResponse<MissingReceiptDeclarationDto>.Ok(result, "Deklarasi tanpa nota berhasil diajukan"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<MissingReceiptDeclarationDto>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<MissingReceiptDeclarationDto>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
        }

        /// <summary>
        /// Menyetujui deklarasi biaya darurat tanpa nota; pengaju tidak boleh menyetujui deklarasinya sendiri.
        /// </summary>
        [HttpPost("missing-receipts/{itemId}/approve")]
        [AccessAction("ApproveMissingReceipt", "Menyetujui Deklarasi Tanpa Nota", AccessType = AccessTypes.Update, SortOrder = 7)]
        [AccessPermission("BusinessTravelSettlement", "ApproveMissingReceipt")]
        [ProducesResponseType(typeof(ApiResponse<MissingReceiptDeclarationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ApproveMissingReceipt([FromRoute] Guid itemId)
        {
            try
            {
                var result = await _missingReceiptService.ApproveAsync(itemId, GetCurrentUserId());
                return Ok(ApiResponse<MissingReceiptDeclarationDto>.Ok(result, "Deklarasi tanpa nota berhasil disetujui"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<MissingReceiptDeclarationDto>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<MissingReceiptDeclarationDto>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
        }

        private Guid GetCurrentUserId()
        {
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userClaim, out var userId)) return userId;
            return Guid.Empty;
        }

        /// <summary>
        /// Mengambil daftar pertanggungjawaban settlement biaya perjalanan dinas (Kontrak v25)
        /// </summary>
        [HttpGet]
        [AccessAction("Read", "Melihat Pertanggungjawaban Dinas", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BusinessTravelSettlement", "Read")]
        [ProducesResponseType(typeof(ApiResponse<List<TravelSettlementResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSettlements(
            [FromQuery] string? status,
            [FromQuery] bool? isOverdue,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var (items, totalCount) = await _settlementService.GetSettlementsAsync(status, isOverdue, pageNumber, pageSize);
            return Ok(ApiResponse<List<TravelSettlementResponseDto>>.Ok(items, $"Total {totalCount} berkas settlement ditemukan"));
        }

        /// <summary>
        /// Mengambil lembar komputasi rekonsiliasi uang muka vs pengeluaran riil nakes (Kontrak v25)
        /// </summary>
        [HttpGet("{id}")]
        [AccessAction("Read", "Melihat Pertanggungjawaban Dinas", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BusinessTravelSettlement", "Read")]
        [ProducesResponseType(typeof(ApiResponse<TravelSettlementDetailResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSettlementById([FromRoute] Guid id)
        {
            try
            {
                var result = await _settlementService.GetSettlementByIdAsync(id);
                return Ok(ApiResponse<TravelSettlementDetailResponseDto>.Ok(result));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<TravelSettlementDetailResponseDto>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
        }

        /// <summary>
        /// Mengajukan settlement biaya perjalanan dinas dan melampirkan kuitansi riil (Kontrak v25)
        /// </summary>
        [HttpPost]
        [AccessAction("Submit", "Menyerahkan Pertanggungjawaban Dinas", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("BusinessTravelSettlement", "Submit")]
        [ProducesResponseType(typeof(ApiResponse<TravelSettlementResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SubmitSettlement([FromBody] SubmitTravelSettlementDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _settlementService.SubmitSettlementAsync(dto, userId);
                return StatusCode(StatusCodes.Status201Created, ApiResponse<TravelSettlementResponseDto>.Ok(result, "Pertanggungjawaban settlement berhasil diserahkan"));
            }
            catch (Exception ex) when (HrSafeError.IsDomain(ex))
            {
                return BadRequest(ApiResponse<TravelSettlementResponseDto>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
        }

        /// <summary>
        /// Verifikasi fisik bukti kuitansi oleh Tim Auditor Keuangan RS (Kontrak v25)
        /// </summary>
        [HttpPost("{id}/verify")]
        [AccessAction("Verify", "Memverifikasi Kuitansi Dinas", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("BusinessTravelSettlement", "Verify")]
        [ProducesResponseType(typeof(ApiResponse<TravelSettlementResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> VerifySettlement([FromRoute] Guid id, [FromBody] VerifySettlementDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _settlementService.VerifySettlementAsync(id, dto, userId);
                return Ok(ApiResponse<TravelSettlementResponseDto>.Ok(result, "Verifikasi kuitansi settlement berhasil disimpan"));
            }
            catch (Exception ex) when (HrSafeError.IsDomain(ex))
            {
                return BadRequest(ApiResponse<TravelSettlementResponseDto>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
        }

        /// <summary>
        /// Penyelesaian akhir kas: pencairan selisih kurang atau setor balik selisih lebih (Kontrak v25)
        /// </summary>
        [HttpPost("{id}/close")]
        [AccessAction("Close", "Menutup Selisih Settlement Dinas", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("BusinessTravelSettlement", "Close")]
        [ProducesResponseType(typeof(ApiResponse<TravelSettlementResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CloseSettlement([FromRoute] Guid id, [FromBody] CloseSettlementDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _settlementService.CloseSettlementAsync(id, dto, userId);
                return Ok(ApiResponse<TravelSettlementResponseDto>.Ok(result, "Penyelesaian kas settlement berhasil disahkan"));
            }
            catch (Exception ex) when (HrSafeError.IsDomain(ex))
            {
                return BadRequest(ApiResponse<TravelSettlementResponseDto>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
        }

        /// <summary>
        /// Unggah struk/bukti pengeluaran perjalanan dinas (MSL-15, DEC-FER-02 opsi A, 18 Nov 2026). Multipart: <c>file</c>; query <c>category</c>
        /// ("travel-receipt"). Jawaban berisi <c>key</c> yang disimpan di kolom lampiran fitur. PDF/JPG/PNG/WEBP/Word/Excel, maks 10 MB.
        /// </summary>
        [HttpPost("attachments")]
        [AccessAction("Submit", "Mengajukan Settlement Dinas", AccessType = AccessTypes.Create, SortOrder = 30)]
        [AccessPermission("BusinessTravelSettlement", "Submit")]
        [Tags("Corporate - HRD Business Travel Settlements")]
        [RequestSizeLimit(HrAttachmentStorage.DefaultMaximumFileSizeBytes + 1024 * 1024)]
        [ProducesResponseType(typeof(ApiResponse<HrStoredAttachment>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadAttachment(
            [FromForm] IFormFile file,
            [FromQuery] string? category,
            [FromServices] HrAttachmentStorage storage,
            CancellationToken cancellationToken = default)
        {
            var resolvedCategory = string.IsNullOrWhiteSpace(category) ? "travel-receipt" : category.Trim();
            if (!new[] { "travel-receipt" }.Contains(resolvedCategory))
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Jenis lampiran tidak dikenal untuk fitur ini."));
            }

            try
            {
                var stored = await storage.SaveAsync(resolvedCategory, User.GetUserId(), file, cancellationToken);
                return StatusCode(StatusCodes.Status201Created, ApiResponse<HrStoredAttachment>.Ok(stored, "Lampiran berhasil diunggah."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
        }

        /// <summary>
        /// Unduh struk/bukti pengeluaran perjalanan dinas (MSL-15). Boleh untuk pengunggah, atau pemegang hak Verify, Close pada fitur ini.
        /// Lampiran milik orang lain tanpa hak dijawab 404 agar keberadaannya tidak bocor.
        /// </summary>
        [HttpGet("attachments/download")]
        [AccessAction("Read", "Melihat Settlement Dinas", AccessType = AccessTypes.Read, SortOrder = 31)]
        [AccessPermission("BusinessTravelSettlement", "Read")]
        [Tags("Corporate - HRD Business Travel Settlements")]
        [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DownloadAttachment(
            [FromQuery] string key,
            [FromServices] HrAttachmentStorage storage,
            [FromServices] AccessPermissionService accessPermissionService)
        {
            var category = HrAttachmentStorage.CategoryOf(key);
            if (category == null || !new[] { "travel-receipt" }.Contains(category))
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Lampiran tidak ditemukan."));
            }

            try
            {
                var attachment = storage.Open(key, category);
                var allowed = attachment.OwnerUserId == User.GetUserId() || await accessPermissionService.HasAccessAsync(User, "BusinessTravelSettlement", "Verify") || await accessPermissionService.HasAccessAsync(User, "BusinessTravelSettlement", "Close");
                if (!allowed)
                {
                    return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Lampiran tidak ditemukan."));
                }

                return PhysicalFile(attachment.PhysicalPath, attachment.ContentType, attachment.DownloadName);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
        }
    }
}
