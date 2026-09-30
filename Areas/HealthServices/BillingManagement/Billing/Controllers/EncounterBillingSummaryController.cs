using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Controllers;

/// <summary>
/// Ringkasan tagihan satu kunjungan Rawat Jalan untuk workspace dokter — <c>BE-RJE-014</c>,
/// <c>FR-RJE-060</c>, <c>RJ-E2E-DEC-008</c>.
/// </summary>
/// <remarks>
/// <para>
/// <b>Arketipe transaksi: monitoring read-only.</b> Satu <c>GET</c>, tanpa endpoint tulis, dan
/// controller tidak menyentuh <c>ApplicationDbContext</c> (<c>QBE-SVC-001</c>).
/// </para>
/// <para>
/// <b>Hak akses <c>EncounterBillingSummary : Read</c> berdiri sendiri</b> dan sengaja tidak
/// menumpang <c>BillingInvoice : Read</c>. Butir itu membuka daftar seluruh invoice dan riwayat
/// pembayaran; dokter yang hanya memegang butir ringkasan tetap ditolak <c>403</c> pada
/// <c>GET /billing/invoices/{id}</c> (<c>AC-RJ-015</c>).
/// </para>
/// </remarks>
[ApiController]
[Authorize]
[Route("api/v1/health-services/billing-management/encounter-billing-summaries")]
[AccessController(
    moduleCode: "HEALTH_SERVICE_BILLING_MANAGEMENT",
    moduleName: "Health Service Billing Management",
    displayName: "Encounter Billing Summary",
    AreaName = "HealthServices",
    ControllerName = "EncounterBillingSummary",
    Description = "Ringkasan tagihan satu kunjungan baca-saja tanpa harga per item",
    SortOrder = 3)]
[Tags("Health Services / Billing Management / Encounter Billing Summary")]
public sealed class EncounterBillingSummaryController : ControllerBase
{
    private readonly EncounterBillingSummaryService _summaryService;

    public EncounterBillingSummaryController(EncounterBillingSummaryService summaryService)
    {
        _summaryService = summaryService;
    }

    /// <summary>Ringkasan tagihan satu kunjungan.</summary>
    /// <remarks>
    /// <c>200</c> ringkasan, termasuk <c>BillingStatus = NO_INVOICE</c> di awal kunjungan;
    /// <c>401</c> sesi tidak berlaku; <c>403</c> tidak memegang <c>EncounterBillingSummary : Read</c>;
    /// <c>404</c> kunjungan tidak ditemukan; <c>422</c> kalkulasi Billing menolak data invoice.
    /// </remarks>
    [HttpGet("{encounterId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<EncounterBillingSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    [AccessAction("Read", "Read Encounter Billing Summary", Description = "Melihat ringkasan tagihan satu kunjungan tanpa harga per item", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("EncounterBillingSummary", "Read")]
    public async Task<IActionResult> GetByEncounter(Guid encounterId, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _summaryService.GetByEncounterAsync(encounterId, CurrentUserId(), cancellationToken);
            if (result is null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Kunjungan tidak ditemukan."));

            return Ok(ApiResponse<EncounterBillingSummaryResponse>.Ok(result,
                result.BillingStatus == EncounterBillingSummaryStatuses.NoInvoice
                    ? "Kunjungan belum memiliki tagihan."
                    : "Ringkasan tagihan kunjungan berhasil dibaca."));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, exception.Message));
        }
        catch (BillingCalculationValidationException exception)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(
                StatusCodes.Status422UnprocessableEntity,
                "Ringkasan tagihan tidak dapat dihitung: " + exception.Message));
        }
    }

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }
}
