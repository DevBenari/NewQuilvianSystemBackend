using System;
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
            catch (Exception ex)
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
            catch (Exception ex)
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
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<TravelSettlementResponseDto>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
        }
    }
}
