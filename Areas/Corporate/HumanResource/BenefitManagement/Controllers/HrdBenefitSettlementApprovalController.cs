using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/corporate/human-resource/benefit-management/settlement-approvals")]
[AccessController(
    moduleCode: "HUMAN_RESOURCE_BENEFIT_SETTLEMENT_APPROVAL",
    moduleName: "Human Resource Benefit Settlement Approval",
    displayName: "HRD Benefit Settlement Approval",
    AreaName = "Corporate",
    ControllerName = "HrdBenefitSettlementApproval",
    Description = "Persetujuan penggunaan benefit penutupan piutang RS oleh HRD",
    SortOrder = 15)]
[Tags("Corporate / Human Resource / Benefit Management / Settlement Approval")]
public sealed class HrdBenefitSettlementApprovalController : ControllerBase
{
    private readonly HrdBenefitSettlementApprovalService _service;

    public HrdBenefitSettlementApprovalController(HrdBenefitSettlementApprovalService service)
    {
        _service = service;
    }

    [HttpGet]
    [AccessAction("Read", "Read Benefit Settlement Approvals", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("HrdBenefitSettlementApproval", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<HrdBenefitSettlementApprovalItemResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] HrdBenefitSettlementApprovalQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetPagedAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<HrdBenefitSettlementApprovalItemResponse>>.Ok(result, "Daftar permohonan persetujuan penutupan benefit berhasil diambil."));
    }

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Benefit Settlement Approval Detail", AccessType = AccessTypes.Read, SortOrder = 2)]
    [AccessPermission("HrdBenefitSettlementApproval", "Read")]
    [ProducesResponseType(typeof(ApiResponse<HrdBenefitSettlementApprovalDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDetail(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.GetDetailAsync(id, cancellationToken);
            return Ok(ApiResponse<HrdBenefitSettlementApprovalDetailResponse>.Ok(result, "Rincian permohonan persetujuan penutupan benefit berhasil diambil."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(404, ex.Message));
        }
    }

    [HttpPost("{id:guid}/approve")]
    [AccessAction("Approve", "Approve Benefit Settlement", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("HrdBenefitSettlementApproval", "Approve")]
    [ProducesResponseType(typeof(ApiResponse<HrdBenefitSettlementApprovalDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Approve(
        Guid id,
        [FromBody] HrdApproveSettlementRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.ApproveAsync(id, request, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<HrdBenefitSettlementApprovalDetailResponse>.Ok(result, "Permohonan penutupan benefit berhasil disetujui oleh HRD."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(404, ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.Fail(409, ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(400, ex.Message));
        }
    }

    [HttpPost("{id:guid}/reject")]
    [AccessAction("Reject", "Reject Benefit Settlement", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("HrdBenefitSettlementApproval", "Reject")]
    [ProducesResponseType(typeof(ApiResponse<HrdBenefitSettlementApprovalDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Reject(
        Guid id,
        [FromBody] HrdRejectSettlementRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.RejectAsync(id, request, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<HrdBenefitSettlementApprovalDetailResponse>.Ok(result, "Permohonan penutupan benefit berhasil ditolak oleh HRD."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(404, ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.Fail(409, ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(400, ex.Message));
        }
    }

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }
}
