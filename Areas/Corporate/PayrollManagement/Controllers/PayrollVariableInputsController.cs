using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Controllers;

/// <summary>
/// Penghasilan variabel manual per payroll run — insentif, bonus, honor tugas tambahan (PAY-BE-19, kontrak PGJ-C1).
/// Arketipe: sub-proses transaksi yang ter-scope ke run induk. Tidak ada DELETE; pembatalan lewat aksi cancel.
/// Seluruh akses data lewat <see cref="PayrollVariableInputService"/>.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/payroll-management/payroll-runs/{runId:guid}/variable-inputs")]
[AccessController(
    moduleCode: "CORPORATE_PAYROLL_VARIABLE_INPUT",
    moduleName: "Corporate Payroll Variable Input",
    displayName: "Payroll Variable Input",
    AreaName = "Corporate",
    ControllerName = "PayrollVariableInput",
    Description = "Penghasilan variabel manual per run: insentif, bonus, dan honor tugas tambahan beserta persetujuannya",
    SortOrder = 64)]
[Tags("Corporate / Payroll Management / Payroll Variable Input")]
public sealed class PayrollVariableInputsController : ControllerBase
{
    private readonly PayrollVariableInputService _service;

    public PayrollVariableInputsController(PayrollVariableInputService service)
    {
        _service = service;
    }

    private Guid CurrentUserId() => PayrollControllerSupport.CurrentUserId(User);

    [HttpGet]
    [AccessAction("Read", "Read Payroll Variable Input", Description = "Melihat penghasilan variabel manual di payroll run", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("PayrollVariableInput", "Read")]
    public Task<IActionResult> List(Guid runId, [FromQuery] PayrollVariableInputQuery query, CancellationToken cancellationToken) =>
        PayrollControllerSupport.RunAsync(
            () => _service.ListAsync(runId, query, cancellationToken),
            "Penghasilan variabel berhasil dimuat.");

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Payroll Variable Input", Description = "Melihat penghasilan variabel manual di payroll run", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("PayrollVariableInput", "Read")]
    public Task<IActionResult> Get(Guid runId, Guid id, CancellationToken cancellationToken) =>
        PayrollControllerSupport.RunAsync(
            () => _service.GetAsync(runId, id, cancellationToken),
            "Detail penghasilan variabel berhasil dimuat.");

    [HttpPost]
    [AccessAction("Create", "Create Payroll Variable Input", Description = "Membuat penghasilan variabel manual berstatus Draft", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("PayrollVariableInput", "Create")]
    public Task<IActionResult> Create(Guid runId, [FromBody] CreatePayrollVariableInputRequest request, CancellationToken cancellationToken) =>
        PayrollControllerSupport.RunAsync(
            () => _service.CreateAsync(runId, request, CurrentUserId(), cancellationToken),
            "Penghasilan variabel berhasil dibuat.");

    /// <summary>Impor semua atau tidak sama sekali (VI-08): bila ada baris gagal, respons 400 membawa daftar baris di <c>errors</c>.</summary>
    [HttpPost("import")]
    [AccessAction("Import", "Import Payroll Variable Input", Description = "Mengimpor banyak penghasilan variabel sekaligus (semua atau tidak sama sekali)", AccessType = AccessTypes.Create, SortOrder = 3)]
    [AccessPermission("PayrollVariableInput", "Import")]
    public async Task<IActionResult> Import(Guid runId, [FromBody] ImportPayrollVariableInputRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.ImportAsync(runId, request, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<PayrollVariableInputImportResult>.Ok(result, $"{result.CreatedCount} penghasilan variabel berhasil diimpor."));
        }
        catch (PayrollVariableInputImportException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, ex.Message, ex.Result));
        }
        catch (Exception ex) when (PayrollControllerSupport.IsHandled(ex))
        {
            return PayrollControllerSupport.Failure(ex);
        }
    }

    [HttpPatch("{id:guid}/content")]
    [AccessAction("Update", "Update Payroll Variable Input", Description = "Mengubah isi penghasilan variabel berstatus Draft", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("PayrollVariableInput", "Update")]
    public Task<IActionResult> UpdateContent(Guid runId, Guid id, [FromBody] UpdatePayrollVariableInputContentRequest request, CancellationToken cancellationToken) =>
        PayrollControllerSupport.RunAsync(
            () => _service.UpdateContentAsync(runId, id, request, CurrentUserId(), cancellationToken),
            "Isi penghasilan variabel berhasil diperbarui.");

    [HttpPost("{id:guid}/submit")]
    [AccessAction("Submit", "Submit Payroll Variable Input", Description = "Mengajukan penghasilan variabel (satu atau massal)", AccessType = AccessTypes.Update, SortOrder = 5)]
    [AccessPermission("PayrollVariableInput", "Submit")]
    public Task<IActionResult> Submit(Guid runId, Guid id, [FromBody] PayrollVariableInputActionRequest request, CancellationToken cancellationToken) =>
        PayrollControllerSupport.RunAsync(
            () => _service.SubmitAsync(runId, id, request, CurrentUserId(), cancellationToken),
            "Penghasilan variabel berhasil diajukan.");

    [HttpPost("{id:guid}/approve")]
    [AccessAction("Approve", "Approve Payroll Variable Input", Description = "Menyetujui penghasilan variabel (satu atau massal); pengaju boleh menyetujui sendiri", AccessType = AccessTypes.Update, SortOrder = 6)]
    [AccessPermission("PayrollVariableInput", "Approve")]
    public Task<IActionResult> Approve(Guid runId, Guid id, [FromBody] PayrollVariableInputActionRequest request, CancellationToken cancellationToken) =>
        PayrollControllerSupport.RunAsync(
            () => _service.ApproveAsync(runId, id, request, CurrentUserId(), cancellationToken),
            "Penghasilan variabel berhasil disetujui.");

    [HttpPost("{id:guid}/reject")]
    [AccessAction("Reject", "Reject Payroll Variable Input", Description = "Menolak penghasilan variabel dengan alasan", AccessType = AccessTypes.Update, SortOrder = 7)]
    [AccessPermission("PayrollVariableInput", "Reject")]
    public Task<IActionResult> Reject(Guid runId, Guid id, [FromBody] PayrollVariableInputDecisionRequest request, CancellationToken cancellationToken) =>
        PayrollControllerSupport.RunAsync(
            () => _service.RejectAsync(runId, id, request, CurrentUserId(), cancellationToken),
            "Penghasilan variabel berhasil ditolak.");

    [HttpPost("{id:guid}/cancel")]
    [AccessAction("Cancel", "Cancel Payroll Variable Input", Description = "Membatalkan penghasilan variabel dengan alasan", AccessType = AccessTypes.Update, SortOrder = 8)]
    [AccessPermission("PayrollVariableInput", "Cancel")]
    public Task<IActionResult> Cancel(Guid runId, Guid id, [FromBody] PayrollVariableInputDecisionRequest request, CancellationToken cancellationToken) =>
        PayrollControllerSupport.RunAsync(
            () => _service.CancelAsync(runId, id, request, CurrentUserId(), cancellationToken),
            "Penghasilan variabel berhasil dibatalkan.");

    [HttpPost("bulk-submit")]
    [AccessAction("Submit", "Submit Payroll Variable Input", Description = "Mengajukan penghasilan variabel (satu atau massal)", AccessType = AccessTypes.Update, SortOrder = 5)]
    [AccessPermission("PayrollVariableInput", "Submit")]
    public Task<IActionResult> BulkSubmit(Guid runId, [FromBody] PayrollVariableInputBulkRequest request, CancellationToken cancellationToken) =>
        PayrollControllerSupport.RunAsync(
            () => _service.BulkSubmitAsync(runId, request, CurrentUserId(), cancellationToken),
            "Pengajuan massal selesai diproses.");

    [HttpPost("bulk-approve")]
    [AccessAction("Approve", "Approve Payroll Variable Input", Description = "Menyetujui penghasilan variabel (satu atau massal); pengaju boleh menyetujui sendiri", AccessType = AccessTypes.Update, SortOrder = 6)]
    [AccessPermission("PayrollVariableInput", "Approve")]
    public Task<IActionResult> BulkApprove(Guid runId, [FromBody] PayrollVariableInputBulkRequest request, CancellationToken cancellationToken) =>
        PayrollControllerSupport.RunAsync(
            () => _service.BulkApproveAsync(runId, request, CurrentUserId(), cancellationToken),
            "Persetujuan massal selesai diproses.");
}
