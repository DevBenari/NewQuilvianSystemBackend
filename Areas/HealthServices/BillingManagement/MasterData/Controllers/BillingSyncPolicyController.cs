using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Controllers;

/// <summary>
/// Kebijakan kirim ulang otomatis fakta klinis dan sinkron invoice — <c>BE-RJE-012</c>,
/// <c>FR-RJE-054</c>, <c>RJ-E2E-DEC-009</c>.
/// </summary>
/// <remarks>
/// Hanya dua endpoint sesuai kontrak V2: kedua kebijakan adalah seed tetap, jadi tidak ada
/// <c>POST</c>, <c>DELETE</c>, <c>options</c>, maupun <c>PATCH /status</c>. Aktif/nonaktif diubah
/// lewat <c>PUT</c> bersama angka lainnya dan dijaga <c>RowVersion</c>.
/// </remarks>
[ApiController]
[Authorize]
[Route("api/v1/health-services/billing-management/master-data/billing-sync-policies")]
[AccessController(
    "HEALTH_SERVICE_BILLING_MANAGEMENT_MASTER_DATA",
    "Health Service Billing Management Master Data",
    "Billing Sync Policy",
    AreaName = "HealthServices",
    ControllerName = "BillingSyncPolicy",
    Description = "Kebijakan kirim ulang otomatis fakta klinis dan sinkron invoice",
    SortOrder = 30)]
[Tags("Health Services / Billing Management / Master Data / Billing Sync Policy")]
public sealed class BillingSyncPolicyController : ControllerBase
{
    private readonly BillingSyncPolicyService _service;

    public BillingSyncPolicyController(BillingSyncPolicyService service) => _service = service;

    [HttpGet]
    [AccessAction("Read", "Read Billing Sync Policy", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("BillingSyncPolicy", "Read")]
    [ProducesResponseType(typeof(ApiResponse<List<BillingSyncPolicyResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken) =>
        Ok(ApiResponse<List<BillingSyncPolicyResponse>>.Ok(
            await _service.ListAsync(cancellationToken), "Kebijakan kirim ulang berhasil diambil."));

    /// <remarks><c>422 RJE-VAL-030</c> angka di luar batas; <c>409 RJE-VAL-031</c> <c>RowVersion</c> basi.</remarks>
    [HttpPut("{id:guid}")]
    [AccessAction("Update", "Update Billing Sync Policy", AccessType = AccessTypes.Update, SortOrder = 2)]
    [AccessPermission("BillingSyncPolicy", "Update")]
    [ProducesResponseType(typeof(ApiResponse<BillingSyncPolicyResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBillingSyncPolicyRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(ApiResponse<BillingSyncPolicyResponse>.Ok(
                await _service.UpdateAsync(id, request, CurrentUserId(), cancellationToken),
                "Kebijakan kirim ulang berhasil diubah."));
        }
        catch (BillingSyncPolicyException exception)
        {
            return StatusCode(exception.StatusCode,
                ApiResponse<object>.Fail(exception.StatusCode, exception.Message, new { exception.Code }));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, exception.Message));
        }
    }

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }
}
