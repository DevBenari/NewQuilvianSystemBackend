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

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/hrd/travel/settlements")]
    [Tags("Corporate - HRD Business Travel Settlements")]
    public class BusinessTravelSettlementsController : ControllerBase
    {
        private readonly ITravelSettlementService _settlementService;

        public BusinessTravelSettlementsController(ITravelSettlementService settlementService)
        {
            _settlementService = settlementService;
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
