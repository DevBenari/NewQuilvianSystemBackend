using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Controllers;

/// <summary>
/// BE-FIN-095, INT-HR-FIN-001 §P.3 (HR -> Finance). Kontrak menetapkan "Autentikasi:
/// Service-to-Service Token / System Worker Identity" — repository ini TIDAK memiliki skema
/// autentikasi service-to-service terpisah di mana pun (diperiksa, nol preseden). Mengarang skema
/// baru untuk satu endpoint ini akan jadi arsitektur keamanan paralel. Endpoint ini karena itu
/// memakai pola [Authorize] + AccessPermission yang sama persis dengan seluruh endpoint lain —
/// akun integrasi HR diberi hak `FinanceReceivablePayroll : SubmitResult` lewat layar Akses Role
/// seperti akun service lainnya. Dicatat sebagai keputusan desain, bukan penyimpangan senyap.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/finance-management/receivables/installments")]
[AccessController("CORPORATE_FINANCE_MANAGEMENT_RECEIVABLE_PAYROLL", "Corporate Finance Management Receivable Payroll", "Receivable Payroll Result",
    AreaName = "Corporate", ControllerName = "FinanceReceivablePayroll", Description = "Penerimaan hasil potongan gaji dari HR Payroll", SortOrder = 33)]
[Tags("Corporate / Finance Management / Receivable / Payroll Result")]
public sealed class FinanceReceivablesPayrollController : ControllerBase
{
    private readonly FinanceReceivablePayrollSyncService _service;
    public FinanceReceivablesPayrollController(FinanceReceivablePayrollSyncService service) => _service = service;

    [HttpPost("payroll-results")]
    [AccessAction("SubmitResult", "Submit Payroll Result", AccessType = AccessTypes.Create, SortOrder = 1)]
    [AccessPermission("FinanceReceivablePayroll", "SubmitResult")]
    [ProducesResponseType(typeof(ApiResponse<PayrollResultResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> SubmitPayrollResult([FromBody] PayrollResultRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.ProcessResultAsync(request, CurrentUserId(), cancellationToken);
            var message = result.IsIdempotentReplay
                ? "Hasil potongan untuk periode ini sudah tercatat. Kiriman ini diabaikan."
                : "Hasil potongan gaji berhasil dicatat.";
            return Ok(ApiResponse<PayrollResultResponse>.Ok(result, message));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
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
