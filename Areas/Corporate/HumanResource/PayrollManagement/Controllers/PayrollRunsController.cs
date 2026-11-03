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
/// Siklus payroll run milik HRD: buka, hitung, ajukan, kembalikan, sahkan, batalkan. Potongan cicilan piutang RS dihitung saat run
/// dihitung dan hasilnya dikonfirmasi ke Finance hanya saat run disahkan. Tidak ada eksekusi potongan manual per pegawai.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/human-resource/payroll-runs")]
[AccessController(
    moduleCode: "CORPORATE_HUMAN_RESOURCE_PAYROLL_RUN",
    moduleName: "Corporate Human Resource Payroll Run",
    displayName: "Payroll Run",
    AreaName = "Corporate",
    ControllerName = "PayrollRun",
    Description = "Siklus payroll run HRD: buka, hitung, ajukan, sahkan, batalkan",
    SortOrder = 55)]
[Tags("Corporate / Human Resource / Payroll Management / Payroll Run")]
public sealed class PayrollRunsController : ControllerBase
{
    private readonly PayrollRunService _service;

    public PayrollRunsController(PayrollRunService service)
    {
        _service = service;
    }

    [HttpGet]
    [AccessAction("Read", "Read Payroll Run", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("PayrollRun", "Read")]
    public async Task<IActionResult> GetPaged([FromQuery] PayrollRunQuery query, CancellationToken cancellationToken)
    {
        var result = await _service.GetPagedAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<PayrollRunListItemResponse>>.Ok(result, "Daftar payroll run berhasil dimuat."));
    }

    [HttpGet("period-options")]
    [AccessAction("ReadPeriods", "Read Payroll Run Periods", AccessType = AccessTypes.Read, SortOrder = 9)]
    [AccessPermission("PayrollRun", "ReadPeriods")]
    public async Task<IActionResult> GetPeriodOptions(CancellationToken cancellationToken) =>
        Ok(ApiResponse<List<PayrollPeriodOptionResponse>>.Ok(await _service.GetPeriodOptionsAsync(cancellationToken), "Periode payroll berhasil dimuat."));

    [HttpGet("period-overview")]
    [AccessAction("ReadPeriodOverview", "Read Payroll Run Period Overview", AccessType = AccessTypes.Read, SortOrder = 10)]
    [AccessPermission("PayrollRun", "ReadPeriodOverview")]
    public async Task<IActionResult> GetPeriodOverview([FromQuery] Guid payrollPeriodId, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(ApiResponse<PayrollRunPeriodOverviewResponse>.Ok(
                await _service.GetPeriodOverviewAsync(payrollPeriodId, cancellationToken), "Ringkasan periode payroll berhasil dimuat."));
        }
        catch (Exception ex) when (IsHandled(ex))
        {
            return Failure(ex);
        }
    }

    [HttpGet("{id:guid}")]
    [AccessAction("ReadDetail", "Read Payroll Run Detail", AccessType = AccessTypes.Read, SortOrder = 2)]
    [AccessPermission("PayrollRun", "ReadDetail")]
    public Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        Run(() => _service.GetByIdAsync(id, cancellationToken), "Detail payroll run berhasil dimuat.");

    [HttpPost]
    [AccessAction("Create", "Open Payroll Run", AccessType = AccessTypes.Create, SortOrder = 3)]
    [AccessPermission("PayrollRun", "Create")]
    public Task<IActionResult> Create([FromBody] CreatePayrollRunRequest request, CancellationToken cancellationToken) =>
        Run(() => _service.CreateAsync(request, CurrentUserId(), cancellationToken), "Payroll run berhasil dibuka.");

    [HttpPost("{id:guid}/calculate")]
    [AccessAction("Calculate", "Calculate Payroll Run", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("PayrollRun", "Calculate")]
    public Task<IActionResult> Calculate(Guid id, CancellationToken cancellationToken) =>
        Run(() => _service.CalculateAsync(id, CurrentUserId(), cancellationToken), "Payroll run berhasil dihitung.");

    [HttpPost("{id:guid}/submit")]
    [AccessAction("Submit", "Submit Payroll Run", AccessType = AccessTypes.Update, SortOrder = 5)]
    [AccessPermission("PayrollRun", "Submit")]
    public Task<IActionResult> Submit(Guid id, CancellationToken cancellationToken) =>
        Run(() => _service.SubmitAsync(id, CurrentUserId(), cancellationToken), "Payroll run berhasil diajukan.");

    [HttpPost("{id:guid}/return")]
    [AccessAction("Return", "Return Payroll Run", AccessType = AccessTypes.Update, SortOrder = 6)]
    [AccessPermission("PayrollRun", "Return")]
    public Task<IActionResult> Return(Guid id, [FromBody] PayrollRunDecisionRequest request, CancellationToken cancellationToken) =>
        Run(() => _service.ReturnAsync(id, request?.Reason, CurrentUserId(), cancellationToken), "Payroll run dikembalikan ke tahap hitung.");

    [HttpPost("{id:guid}/approve")]
    [AccessAction("Approve", "Approve Payroll Run", AccessType = AccessTypes.Update, SortOrder = 7)]
    [AccessPermission("PayrollRun", "Approve")]
    public Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken) =>
        Run(() => _service.ApproveAsync(id, CurrentUserId(), cancellationToken), "Payroll run disahkan dan hasil potongan dikonfirmasi ke Finance.");

    [HttpPost("{id:guid}/cancel")]
    [AccessAction("Cancel", "Cancel Payroll Run", AccessType = AccessTypes.Update, SortOrder = 8)]
    [AccessPermission("PayrollRun", "Cancel")]
    public Task<IActionResult> Cancel(Guid id, [FromBody] PayrollRunDecisionRequest request, CancellationToken cancellationToken) =>
        Run(() => _service.CancelAsync(id, request?.Reason, CurrentUserId(), cancellationToken), "Payroll run dibatalkan.");

    [HttpPost("{id:guid}/employees/{runEmployeeId:guid}/reversals")]
    [AccessAction("RequestReversal", "Request Payroll Deduction Reversal", AccessType = AccessTypes.Create, SortOrder = 11)]
    [AccessPermission("PayrollRun", "RequestReversal")]
    public Task<IActionResult> RequestReversal(Guid id, Guid runEmployeeId, [FromBody] RequestPayrollReversalRequest request, CancellationToken cancellationToken) =>
        Run(() => _service.RequestReversalAsync(id, runEmployeeId, request, CurrentUserId(), cancellationToken), "Pembalikan potongan diajukan.");

    [HttpPost("{id:guid}/reversals/{reversalId:guid}/approve")]
    [AccessAction("ApproveReversal", "Approve Payroll Deduction Reversal", AccessType = AccessTypes.Update, SortOrder = 12)]
    [AccessPermission("PayrollRun", "ApproveReversal")]
    public Task<IActionResult> ApproveReversal(Guid id, Guid reversalId, CancellationToken cancellationToken) =>
        Run(() => _service.ApproveReversalAsync(id, reversalId, CurrentUserId(), cancellationToken), "Pembalikan disetujui dan saldo piutang dipulihkan di Finance.");

    [HttpPost("{id:guid}/reversals/{reversalId:guid}/reject")]
    [AccessAction("RejectReversal", "Reject Payroll Deduction Reversal", AccessType = AccessTypes.Update, SortOrder = 13)]
    [AccessPermission("PayrollRun", "RejectReversal")]
    public Task<IActionResult> RejectReversal(Guid id, Guid reversalId, [FromBody] PayrollRunDecisionRequest request, CancellationToken cancellationToken) =>
        Run(() => _service.RejectReversalAsync(id, reversalId, request?.Reason, CurrentUserId(), cancellationToken), "Pembalikan ditolak.");

    private async Task<IActionResult> Run(Func<Task<PayrollRunDetailResponse>> action, string message)
    {
        try
        {
            return Ok(ApiResponse<PayrollRunDetailResponse>.Ok(await action(), message));
        }
        catch (Exception ex) when (IsHandled(ex))
        {
            return Failure(ex);
        }
    }

    private IActionResult Failure(Exception exception) => exception switch
    {
        KeyNotFoundException => NotFound(ApiResponse<object>.Fail(404, exception.Message)),
        PayrollRunConflictException => Conflict(ApiResponse<object>.Fail(409, exception.Message)),
        PayrollRunValidationException or PayrollResultValidationException =>
            UnprocessableEntity(ApiResponse<object>.Fail(422, exception.Message)),
        _ => throw exception
    };

    private static bool IsHandled(Exception exception) =>
        exception is KeyNotFoundException or PayrollRunConflictException or PayrollRunValidationException or PayrollResultValidationException;

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }
}
