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
/// Master peralihan penanganan potongan cicilan ke mesin payroll (BE-FIN-115, FIN-DEC-222, FR-FIN-258, FIN-API-1.11 §S.2).
/// Mengikuti pola MstDirectPaymentThreshold / DirectPaymentThresholdController (satu nilai aktif berversi).
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/finance-management/master-data/payroll-deduction-cutovers")]
[AccessController("CORPORATE_FINANCE_MANAGEMENT_MASTER_DATA_PAYROLL_DEDUCTION_CUTOVER",
    "Corporate Finance Management Payroll Deduction Cutover", "Peralihan Potong Gaji ke Mesin Payroll",
    AreaName = "Corporate", ControllerName = "PayrollDeductionCutover",
    Description = "Periode pertama yang ditangani mesin payroll", SortOrder = 75)]
[Tags("Corporate / Finance Management / Master Data / Payroll Deduction Cutover")]
public sealed class PayrollDeductionCutoverController : ControllerBase
{
    private readonly PayrollDeductionCutoverService _service;

    public PayrollDeductionCutoverController(PayrollDeductionCutoverService service)
    {
        _service = service;
    }

    /// <summary>
    /// Nilai peralihan yang aktif. Mengembalikan null dalam payload data bila belum pernah ditetapkan (fail-open ke jembatan manual, AC 1).
    /// </summary>
    [HttpGet("current")]
    [AccessAction("Read", "Read Current Payroll Deduction Cutover", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("PayrollDeductionCutover", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PayrollDeductionCutoverResponse?>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrent(CancellationToken cancellationToken)
    {
        var result = await _service.GetCurrentAsync(cancellationToken);
        return Ok(ApiResponse<PayrollDeductionCutoverResponse?>.Ok(result, "Periode peralihan aktif berhasil diambil."));
    }

    /// <summary>
    /// Riwayat nilai peralihan pemotongan gaji (FIN-API-1.11 §S.2).
    /// </summary>
    [HttpGet]
    [AccessAction("Read", "Read Payroll Deduction Cutover History", AccessType = AccessTypes.Read, SortOrder = 2)]
    [AccessPermission("PayrollDeductionCutover", "Read")]
    [ProducesResponseType(typeof(ApiResponse<List<PayrollDeductionCutoverResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistory(CancellationToken cancellationToken)
    {
        var result = await _service.GetHistoryAsync(cancellationToken);
        return Ok(ApiResponse<List<PayrollDeductionCutoverResponse>>.Ok(result, "Riwayat periode peralihan berhasil diambil."));
    }

    /// <summary>
    /// Menetapkan periode pertama yang ditangani mesin payroll; versi lama dinonaktifkan (FIN-API-1.11 §S.2, AC 2, AC 3, AC 4).
    /// </summary>
    [HttpPost]
    [AccessAction("Create", "Set Payroll Deduction Cutover", AccessType = AccessTypes.Create, SortOrder = 3)]
    [AccessPermission("PayrollDeductionCutover", "Create")]
    [ProducesResponseType(typeof(ApiResponse<PayrollDeductionCutoverResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> SetCutover(
        [FromBody] SetPayrollDeductionCutoverRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.SetCutoverAsync(request, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<PayrollDeductionCutoverResponse>.Ok(result, "Periode peralihan berhasil ditetapkan."));
        }
        catch (PayrollDeductionCutoverValidationException exception)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, exception.Message));
        }
        catch (PayrollDeductionCutoverBadRequestException exception)
        {
            return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, exception.Message));
        }
        catch (PayrollDeductionCutoverConflictException exception)
        {
            return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, exception.Message));
        }
    }

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }
}

