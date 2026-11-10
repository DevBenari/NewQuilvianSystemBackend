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
    [Route("api/v1/hrd/travel/advances")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_BUSINESS_TRAVEL",
        moduleName: "Human Resource Business Travel",
        displayName: "Uang Muka Perjalanan Dinas",
        AreaName = "Corporate",
        ControllerName = "BusinessTravelAdvance",
        Description = "Pengajuan, persetujuan, dan pencairan uang muka perjalanan dinas",
        SortOrder = 11)]
    [Tags("Corporate - HRD Business Travel Advances")]
    public class BusinessTravelAdvancesController : ControllerBase
    {
        private readonly ITravelAdvanceRequestService _advanceRequestService;
        private readonly IAdvanceApprovalAndVoucherService _approvalService;
        private readonly IAdvanceDisbursementService _disbursementService;

        public BusinessTravelAdvancesController(
            ITravelAdvanceRequestService advanceRequestService,
            IAdvanceApprovalAndVoucherService approvalService,
            IAdvanceDisbursementService disbursementService)
        {
            _advanceRequestService = advanceRequestService;
            _approvalService = approvalService;
            _disbursementService = disbursementService;
        }

        private (Guid UserId, Guid EmployeeId, bool IsFinance) GetCurrentUserContext()
        {
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var empClaim = User.FindFirst("EmployeeId")?.Value;

            Guid.TryParse(userClaim, out var userId);
            Guid.TryParse(empClaim, out var empId);

            var isFinance = User.IsInRole("Finance") || User.IsInRole("FinanceManager") || User.IsInRole("Admin");

            return (userId, empId != Guid.Empty ? empId : userId, isFinance);
        }

        /// <summary>
        /// Mengambil daftar permohonan uang muka kerja SPPD (Kontrak v25)
        /// </summary>
        [HttpGet]
        [AccessAction("Read", "Melihat Uang Muka Dinas", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BusinessTravelAdvance", "Read")]
        [ProducesResponseType(typeof(ApiResponse<List<TravelAdvanceResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAdvances([FromQuery] string? status, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var (items, totalCount) = await _advanceRequestService.GetAdvancesAsync(status, pageNumber, pageSize);
            return Ok(ApiResponse<List<TravelAdvanceResponseDto>>.Ok(items, $"Total {totalCount} permohonan uang muka ditemukan"));
        }

        /// <summary>
        /// Mengajukan uang muka perjalanan dinas (maksimal 80% dari pagu yang disetujui) (Kontrak v25)
        /// </summary>
        [HttpPost]
        [AccessAction("Request", "Mengajukan Uang Muka Dinas", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("BusinessTravelAdvance", "Request")]
        [ProducesResponseType(typeof(ApiResponse<TravelAdvanceResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateAdvance([FromBody] CreateTravelAdvanceRequestDto dto)
        {
            try
            {
                var (userId, empId, _) = GetCurrentUserContext();
                var result = await _advanceRequestService.CreateAdvanceRequestAsync(dto, userId, empId);
                return StatusCode(StatusCodes.Status201Created, ApiResponse<TravelAdvanceResponseDto>.Ok(result, "Permohonan uang muka berhasil diajukan"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<TravelAdvanceResponseDto>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
        }

        /// <summary>
        /// Persetujuan uang muka oleh Supervisor &amp; Finance Manager (Alur 2-Pintu VR-TRV-03) (Kontrak v25)
        /// </summary>
        [HttpPost("{id}/approve")]
        [AccessAction("Approve", "Menyetujui Uang Muka Dinas", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("BusinessTravelAdvance", "Approve")]
        [ProducesResponseType(typeof(ApiResponse<TravelAdvanceResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ApproveAdvance([FromRoute] Guid id, [FromBody] ApproveAdvanceRequestDto dto)
        {
            try
            {
                var (userId, _, isFinance) = GetCurrentUserContext();
                var result = await _approvalService.ApproveAdvanceAsync(id, dto, userId, isFinance);
                return Ok(ApiResponse<TravelAdvanceResponseDto>.Ok(result, "Persetujuan uang muka berhasil dicatat"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<TravelAdvanceResponseDto>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
        }

        /// <summary>
        /// Konfirmasi pencairan dana uang muka oleh Kasir RS (voucher / transfer bank) (Kontrak v25)
        /// </summary>
        [HttpPost("{id}/disburse")]
        [AccessAction("Disburse", "Mencairkan Uang Muka Dinas", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("BusinessTravelAdvance", "Disburse")]
        [ProducesResponseType(typeof(ApiResponse<TravelAdvanceResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DisburseAdvance([FromRoute] Guid id, [FromBody] DisburseAdvancePaymentDto dto)
        {
            try
            {
                var (userId, _, _) = GetCurrentUserContext();
                var result = await _disbursementService.DisburseAdvanceAsync(id, dto, userId);
                return Ok(ApiResponse<TravelAdvanceResponseDto>.Ok(result, "Pencairan dana uang muka berhasil dikonfirmasi kasir"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<TravelAdvanceResponseDto>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
        }
    }
}
