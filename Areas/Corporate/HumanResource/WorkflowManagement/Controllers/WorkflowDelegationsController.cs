using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/hrd/workflow/delegations")]
    [Tags("Corporate - HRD Workflow Delegations")]
    public class WorkflowDelegationsController : ControllerBase
    {
        private readonly IApprovalDelegationV2Service _delegationService;

        public WorkflowDelegationsController(IApprovalDelegationV2Service delegationService)
        {
            _delegationService = delegationService;
        }

        private Guid GetCurrentEmployeeId()
        {
            var claim = User.FindFirst("EmployeeId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(claim, out var empId)) return empId;
            return Guid.Empty;
        }

        private Guid GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(claim, out var userId)) return userId;
            return Guid.Empty;
        }

        /// <summary>
        /// Mengambil daftar pendelegasian wewenang aktif dan riwayat masa lalu (FR-WF-07)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<ApprovalDelegationV2ResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDelegations(
            [FromQuery] bool? isActive,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var result = await _delegationService.GetDelegationsAsync(isActive, pageNumber, pageSize);
            return Ok(ApiResponse<PagedResult<ApprovalDelegationV2ResponseDto>>.Ok(result));
        }

        /// <summary>
        /// Mendaftarkan surat kuasa penunjukan Pelaksana Harian (Plh) berbatas waktu (FR-WF-07, HRD-DEC-184)
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ApprovalDelegationV2ResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateDelegation([FromBody] CreateApprovalDelegationRequestDto request)
        {
            try
            {
                var empId = GetCurrentEmployeeId();
                var result = await _delegationService.CreateDelegationAsync(empId, request);
                return StatusCode(StatusCodes.Status201Created, ApiResponse<ApprovalDelegationV2ResponseDto>.Ok(result, "Surat kuasa penunjukan Plh berhasil didaftarkan"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<ApprovalDelegationV2ResponseDto>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<ApprovalDelegationV2ResponseDto>.Fail(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }

        /// <summary>
        /// Mencabut wewenang delegasi Plh lebih awal sebelum tanggal berakhir (FR-WF-08)
        /// </summary>
        [HttpPut("{id}/revoke")]
        [ProducesResponseType(typeof(ApiResponse<ApprovalDelegationV2ResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RevokeDelegation([FromRoute] Guid id, [FromBody] RevokeDelegationRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _delegationService.RevokeDelegationAsync(id, request, userId);
                return Ok(ApiResponse<ApprovalDelegationV2ResponseDto>.Ok(result, "Wewenang Plh berhasil dicabut"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<ApprovalDelegationV2ResponseDto>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<ApprovalDelegationV2ResponseDto>.Fail(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }

        /// <summary>
        /// Mengambil daftar wewenang Plh yang sedang dipegang oleh staf login (FR-WF-07)
        /// </summary>
        [HttpGet("my-acting-roles")]
        [ProducesResponseType(typeof(ApiResponse<List<ApprovalDelegationV2ResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyActingRoles()
        {
            var empId = GetCurrentEmployeeId();
            var result = await _delegationService.GetMyActingRolesAsync(empId);
            return Ok(ApiResponse<List<ApprovalDelegationV2ResponseDto>>.Ok(result));
        }
    }
}
