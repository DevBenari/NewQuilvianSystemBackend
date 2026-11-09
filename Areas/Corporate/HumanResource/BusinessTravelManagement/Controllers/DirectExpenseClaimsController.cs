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
    [Route("api/v1/hrd/expenses/claims")]
    [Tags("Corporate - HRD Direct Non-Trip Expenses")]
    public class DirectExpenseClaimsController : ControllerBase
    {
        private readonly IDirectExpenseClaimService _claimService;
        private readonly IExpenseVerificationAndPaymentService _paymentService;

        public DirectExpenseClaimsController(
            IDirectExpenseClaimService claimService,
            IExpenseVerificationAndPaymentService paymentService)
        {
            _claimService = claimService;
            _paymentService = paymentService;
        }

        private (Guid UserId, Guid EmployeeId) GetCurrentUserContext()
        {
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var empClaim = User.FindFirst("EmployeeId")?.Value;

            Guid.TryParse(userClaim, out var userId);
            Guid.TryParse(empClaim, out var empId);

            return (userId, empId != Guid.Empty ? empId : userId);
        }

        /// <summary>
        /// Mengambil daftar pengajuan klaim biaya operasional non-trip staf (Kontrak v25)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<ExpenseClaimResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetClaims(
            [FromQuery] string? status,
            [FromQuery] Guid? categoryId,
            [FromQuery] DateOnly? startDate,
            [FromQuery] DateOnly? endDate,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var (items, totalCount) = await _claimService.GetExpenseClaimsAsync(
                null, status, categoryId, startDate, endDate, pageNumber, pageSize);
            return Ok(ApiResponse<List<ExpenseClaimResponseDto>>.Ok(items, $"Total {totalCount} klaim operasional non-trip ditemukan"));
        }

        /// <summary>
        /// Mengajukan klaim penggantian biaya operasional langsung lapangan non-trip via ESS (Kontrak v25)
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ExpenseClaimResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateClaim([FromBody] CreateExpenseClaimDto dto)
        {
            try
            {
                var (userId, empId) = GetCurrentUserContext();
                var result = await _claimService.CreateExpenseClaimAsync(dto, userId, empId);
                return StatusCode(StatusCodes.Status201Created, ApiResponse<ExpenseClaimResponseDto>.Ok(result, "Klaim operasional non-trip berhasil diajukan"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ExpenseClaimResponseDto>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
        }

        /// <summary>
        /// Mengambil detail rincian klaim non-trip dan bukti struk kuitansi (Kontrak v25)
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<ExpenseClaimDetailResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetClaimById([FromRoute] Guid id)
        {
            try
            {
                var result = await _claimService.GetExpenseClaimByIdAsync(id);
                return Ok(ApiResponse<ExpenseClaimDetailResponseDto>.Ok(result));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<ExpenseClaimDetailResponseDto>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
        }

        /// <summary>
        /// Verifikasi audit keabsahan kuitansi pembelian darurat oleh Finance (Kontrak v25)
        /// </summary>
        [HttpPost("{id}/verify")]
        [ProducesResponseType(typeof(ApiResponse<ExpenseClaimResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> VerifyClaim([FromRoute] Guid id, [FromBody] VerifyExpenseClaimDto dto)
        {
            try
            {
                var (userId, _) = GetCurrentUserContext();
                var result = await _paymentService.VerifyExpenseClaimAsync(id, dto, userId);
                return Ok(ApiResponse<ExpenseClaimResponseDto>.Ok(result, "Verifikasi klaim non-trip berhasil disimpan"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ExpenseClaimResponseDto>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
        }

        /// <summary>
        /// Otorisasi pembayaran pencairan uang klaim ke rekening staf oleh Kasir RS (Kontrak v25)
        /// </summary>
        [HttpPost("{id}/pay")]
        [ProducesResponseType(typeof(ApiResponse<ExpenseClaimResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PayClaim([FromRoute] Guid id, [FromBody] PayExpenseClaimDto dto)
        {
            try
            {
                var (userId, _) = GetCurrentUserContext();
                var result = await _paymentService.PayExpenseClaimAsync(id, dto, userId);
                return Ok(ApiResponse<ExpenseClaimResponseDto>.Ok(result, "Pembayaran klaim operasional non-trip berhasil dicairkan"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<ExpenseClaimResponseDto>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
        }
    }
}
