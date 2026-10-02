using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Controllers;

/// <summary>
/// Ambang nilai pembayaran langsung (BE-FIN-076, FIN-API-1.6 F.4). Resource dan atribut controller
/// persis seperti yang sudah dicatat `permission-audit-matrix.md` G.2. Nol resource/action baru.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/finance-management/master-data/direct-payment-threshold")]
[AccessController("CORPORATE_FINANCE_MANAGEMENT_MASTER_DIRECT_PAYMENT_THRESHOLD",
    "Corporate Finance Management Direct Payment Threshold", "Master Data",
    AreaName = "Corporate", ControllerName = "MstDirectPaymentThreshold",
    Description = "Ambang nilai pembayaran langsung", SortOrder = 73)]
[Tags("Corporate / Finance Management / Master Data / Direct Payment Threshold")]
public sealed class DirectPaymentThresholdController : ControllerBase
{
    private readonly DirectPaymentThresholdService _service;

    public DirectPaymentThresholdController(DirectPaymentThresholdService service)
    {
        _service = service;
    }

    /// <summary>Ambang aktif beserta alasan perubahan terakhir. 404 bila belum pernah ditetapkan (fail-closed, FIN-DES-086).</summary>
    [HttpGet]
    [AccessAction("Read", "Read Direct Payment Threshold", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("MstDirectPaymentThreshold", "Read")]
    [ProducesResponseType(typeof(ApiResponse<DirectPaymentThresholdResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.GetActiveAsync(cancellationToken);
            return Ok(ApiResponse<DirectPaymentThresholdResponse>.Ok(result, "Ambang pembayaran langsung berhasil diambil."));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, exception.Message));
        }
    }

    /// <summary>Mengubah (atau menetapkan pertama kali) ambang; alasan perubahan wajib (FIN-DEC-134).</summary>
    [HttpPut]
    [AccessAction("Update", "Update Direct Payment Threshold", AccessType = AccessTypes.Update, SortOrder = 2)]
    [AccessPermission("MstDirectPaymentThreshold", "Update")]
    [ProducesResponseType(typeof(ApiResponse<DirectPaymentThresholdResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update([FromBody] UpdateDirectPaymentThresholdRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.UpdateAsync(request, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<DirectPaymentThresholdResponse>.Ok(result, "Ambang pembayaran langsung berhasil diperbarui."));
        }
        catch (DirectPaymentThresholdValidationException exception)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, exception.Message));
        }
        catch (DirectPaymentThresholdBadRequestException exception)
        {
            return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, exception.Message));
        }
    }

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }
}
