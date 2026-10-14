using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Controllers;

/// <summary>
/// Controller eksekusi pemotongan cicilan piutang RS di dalam mesin payroll run HR (BE-FIN-110).
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/human-resource/payroll-runs")]
[AccessController(
    moduleCode: "CORPORATE_HUMAN_RESOURCE_PAYROLL_RUN_INSTALLMENT_DEDUCTION",
    moduleName: "Corporate Human Resource Payroll Run Installment Deduction",
    displayName: "Payroll Run Installment Deduction",
    AreaName = "Corporate",
    ControllerName = "PayrollRunInstallmentDeduction",
    Description = "Eksekusi pemotongan cicilan piutang RS di dalam mesin payroll run HR",
    SortOrder = 55)]
[Tags("Corporate / Human Resource / Payroll Management / Installment Deduction")]
public sealed class PayrollRunInstallmentDeductionController : ControllerBase
{
    private readonly PayrollInstallmentDeductionService _service;

    public PayrollRunInstallmentDeductionController(PayrollInstallmentDeductionService service)
    {
        _service = service;
    }

    /// <summary>
    /// Eksekusi pemotongan cicilan piutang RS untuk seluruh pegawai pada suatu payroll run.
    /// </summary>
    [HttpPost("{runId:guid}/installment-deductions")]
    [AccessAction("ExecuteRunDeductions", "Execute Run Installment Deductions", AccessType = AccessTypes.Create, SortOrder = 1)]
    [AccessPermission("PayrollRunInstallmentDeduction", "ExecuteRunDeductions")]
    [ProducesResponseType(typeof(ApiResponse<PayrollRunInstallmentDeductionSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ExecuteRunDeductions(Guid runId, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.ProcessRunInstallmentDeductionsAsync(runId, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<PayrollRunInstallmentDeductionSummaryResponse>.Ok(
                result, $"Pemotongan cicilan untuk {result.TotalInstallmentsProcessed} angsuran berhasil dieksekusi."));
        }
        catch (Exception ex) when (IsHandled(ex))
        {
            return Failure(ex);
        }
    }

    /// <summary>
    /// Eksekusi pemotongan cicilan piutang RS untuk satu baris pegawai pada payroll run.
    /// </summary>
    [HttpPost("employees/{runEmployeeId:guid}/installment-deductions")]
    [AccessAction("ExecuteEmployeeDeduction", "Execute Employee Installment Deduction", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("PayrollRunInstallmentDeduction", "ExecuteEmployeeDeduction")]
    [ProducesResponseType(typeof(ApiResponse<PayrollEmployeeInstallmentDeductionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ExecuteEmployeeDeduction(Guid runEmployeeId, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.ProcessEmployeeInstallmentDeductionAsync(runEmployeeId, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<PayrollEmployeeInstallmentDeductionResponse>.Ok(result, result.Message ?? "Pemotongan cicilan berhasil dieksekusi."));
        }
        catch (Exception ex) when (IsHandled(ex))
        {
            return Failure(ex);
        }
    }

    private IActionResult Failure(Exception exception) => exception switch
    {
        KeyNotFoundException => NotFound(ApiResponse<object>.Fail(404, exception.Message)),
        PayrollResultValidationException => UnprocessableEntity(ApiResponse<object>.Fail(422, exception.Message)),
        _ => throw exception
    };

    private static bool IsHandled(Exception exception) =>
        exception is KeyNotFoundException or PayrollResultValidationException;

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }
}

