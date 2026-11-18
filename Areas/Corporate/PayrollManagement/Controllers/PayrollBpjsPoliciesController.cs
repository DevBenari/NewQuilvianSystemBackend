using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Options;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Controllers;

/// <summary>
/// Kebijakan tarif dan batas upah iuran BPJS berversi (PrlBpjsPolicy). Nilai bawaan Pengaturan Penggajian bukan kebijakan resmi:
/// HRD menyalinnya menjadi draf (<c>from-setting</c>), melengkapi klasifikasi risiko JKK beserta dasarnya, mengajukannya, dan Finance
/// mengesahkannya. Izin dipisah per kemampuan di Akses Role (Read, Create, Update, Submit, Approve, Reject, Retire, Delete); pengesah
/// selalu berbeda dari pengaju. Hanya kebijakan yang disahkan yang dipakai mesin payroll.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/payroll-management/bpjs-policies")]
[AccessController(
    moduleCode: "CORPORATE_PAYROLL_COMPENSATION",
    moduleName: "Corporate Payroll Compensation",
    displayName: "Payroll BPJS Policy",
    AreaName = "Corporate",
    ControllerName = "PayrollBpjsPolicy",
    Description = "Kebijakan tarif dan plafon iuran BPJS berversi (klasifikasi risiko JKK, JHT, JP, JKM, BPJS Kesehatan) yang diverifikasi HRD dan disahkan Finance",
    SortOrder = 71)]
[Tags("Corporate / Payroll Management / BPJS Policy")]
public sealed class PayrollBpjsPoliciesController : ControllerBase
{
    private const string Resource = "PayrollBpjsPolicy";

    private readonly PayrollBpjsPolicyService _service;
    private readonly AccessPermissionService _accessPermissionService;
    private readonly PayrollPolicyOptions _policyOptions;

    public PayrollBpjsPoliciesController(
        PayrollBpjsPolicyService service,
        AccessPermissionService accessPermissionService,
        IOptions<PayrollPolicyOptions> policyOptions)
    {
        _service = service;
        _accessPermissionService = accessPermissionService;
        _policyOptions = policyOptions.Value;
    }

    private Task<bool> HasAsync(string action) => _accessPermissionService.HasAccessAsync(User, Resource, action);

    private async Task<PayrollStatutoryPolicyCaller> ResolveCallerAsync() => new(
        CanUpdate: await HasAsync("Update"),
        CanSubmit: await HasAsync("Submit"),
        CanApprove: await HasAsync("Approve"),
        CanReject: await HasAsync("Reject"),
        CanRetire: await HasAsync("Retire"),
        CanDelete: await HasAsync("Delete"));

    [HttpGet("filters/metadata")]
    [AccessAction("Read", "Read Payroll BPJS Policy", Description = "Melihat kebijakan iuran BPJS berversi", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("PayrollBpjsPolicy", "Read")]
    public IActionResult GetMetadata() =>
        Ok(ApiResponse<PayrollStatutoryPolicyMetadataResponse>.Ok(
            PayrollStatutoryPolicyMetadataFactory.Build(_policyOptions.EnableBpjsInPayroll), "Metadata kebijakan iuran BPJS berhasil dimuat."));

    [HttpGet]
    [AccessAction("Read", "Read Payroll BPJS Policy", Description = "Melihat kebijakan iuran BPJS berversi", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("PayrollBpjsPolicy", "Read")]
    public Task<IActionResult> List([FromQuery] PayrollStatutoryPolicyQuery query, CancellationToken cancellationToken) =>
        PayrollControllerSupport.RunAsync(() => _service.ListAsync(query, cancellationToken), "Daftar kebijakan iuran BPJS berhasil dimuat.");

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Payroll BPJS Policy", Description = "Melihat kebijakan iuran BPJS berversi", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("PayrollBpjsPolicy", "Read")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var caller = await ResolveCallerAsync();
        return await PayrollControllerSupport.RunAsync(() => _service.GetAsync(id, caller, cancellationToken), "Kebijakan iuran BPJS berhasil dimuat.");
    }

    [HttpPost]
    [AccessAction("Create", "Create Payroll BPJS Policy", Description = "Menyusun draf kebijakan iuran BPJS", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("PayrollBpjsPolicy", "Create")]
    public async Task<IActionResult> Create([FromBody] SavePayrollBpjsPolicyRequest request, CancellationToken cancellationToken)
    {
        var caller = await ResolveCallerAsync();
        return await PayrollControllerSupport.RunAsync(
            () => _service.CreateAsync(request, PayrollControllerSupport.CurrentUserId(User), caller, cancellationToken), "Draf kebijakan iuran BPJS berhasil disimpan.");
    }

    /// <summary>Menyalin nilai Pengaturan Penggajian saat ini menjadi draf kebijakan (nilai itu belum resmi; klasifikasi risiko JKK dikosongkan).</summary>
    [HttpPost("from-setting")]
    [AccessAction("Create", "Create Payroll BPJS Policy", Description = "Menyusun draf kebijakan iuran BPJS", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("PayrollBpjsPolicy", "Create")]
    public async Task<IActionResult> CreateFromSetting([FromBody] CreatePayrollBpjsPolicyFromSettingRequest request, CancellationToken cancellationToken)
    {
        var caller = await ResolveCallerAsync();
        return await PayrollControllerSupport.RunAsync(
            () => _service.CreateFromSettingAsync(request, PayrollControllerSupport.CurrentUserId(User), caller, cancellationToken),
            "Draf kebijakan iuran BPJS dari Pengaturan Penggajian berhasil dibuat; lengkapi klasifikasi risiko JKK dan verifikasi tarifnya.");
    }

    [HttpPut("{id:guid}")]
    [AccessAction("Update", "Update Payroll BPJS Policy", Description = "Mengubah draf kebijakan iuran BPJS yang belum diajukan", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("PayrollBpjsPolicy", "Update")]
    public async Task<IActionResult> Update(Guid id, [FromBody] SavePayrollBpjsPolicyRequest request, CancellationToken cancellationToken)
    {
        var caller = await ResolveCallerAsync();
        return await PayrollControllerSupport.RunAsync(
            () => _service.UpdateAsync(id, request, PayrollControllerSupport.CurrentUserId(User), caller, cancellationToken), "Draf kebijakan iuran BPJS berhasil diperbarui.");
    }

    [HttpPost("{id:guid}/revise")]
    [AccessAction("Create", "Create Payroll BPJS Policy", Description = "Menyusun draf kebijakan iuran BPJS", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("PayrollBpjsPolicy", "Create")]
    public async Task<IActionResult> Revise(Guid id, [FromBody] PayrollStatutoryPolicyReviseRequest request, CancellationToken cancellationToken)
    {
        var caller = await ResolveCallerAsync();
        return await PayrollControllerSupport.RunAsync(
            () => _service.ReviseAsync(id, request, PayrollControllerSupport.CurrentUserId(User), caller, cancellationToken), "Revisi kebijakan iuran BPJS berhasil dibuat sebagai draf.");
    }

    [HttpPost("{id:guid}/submit")]
    [AccessAction("Submit", "Submit Payroll BPJS Policy", Description = "Menyatakan kebijakan iuran BPJS sudah diverifikasi terhadap ketentuan BPJS dan mengajukannya untuk disahkan", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("PayrollBpjsPolicy", "Submit")]
    public async Task<IActionResult> Submit(Guid id, CancellationToken cancellationToken)
    {
        var caller = await ResolveCallerAsync();
        return await PayrollControllerSupport.RunAsync(
            () => _service.SubmitAsync(id, PayrollControllerSupport.CurrentUserId(User), caller, cancellationToken), "Kebijakan iuran BPJS berhasil diajukan.");
    }

    [HttpPost("{id:guid}/approve")]
    [AccessAction("Approve", "Approve Payroll BPJS Policy", Description = "Mengesahkan kebijakan iuran BPJS; pengesah harus berbeda dari pengaju", AccessType = AccessTypes.Update, SortOrder = 5)]
    [AccessPermission("PayrollBpjsPolicy", "Approve")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        var caller = await ResolveCallerAsync();
        return await PayrollControllerSupport.RunAsync(
            () => _service.ApproveAsync(id, PayrollControllerSupport.CurrentUserId(User), caller, cancellationToken), "Kebijakan iuran BPJS berhasil disahkan.");
    }

    [HttpPost("{id:guid}/reject")]
    [AccessAction("Reject", "Reject Payroll BPJS Policy", Description = "Menolak kebijakan iuran BPJS dengan alasan", AccessType = AccessTypes.Update, SortOrder = 6)]
    [AccessPermission("PayrollBpjsPolicy", "Reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] PayrollStatutoryPolicyActionRequest request, CancellationToken cancellationToken)
    {
        var caller = await ResolveCallerAsync();
        return await PayrollControllerSupport.RunAsync(
            () => _service.RejectAsync(id, request.Reason, PayrollControllerSupport.CurrentUserId(User), caller, cancellationToken), "Kebijakan iuran BPJS berhasil ditolak.");
    }

    [HttpPost("{id:guid}/retire")]
    [AccessAction("Retire", "Retire Payroll BPJS Policy", Description = "Mempensiunkan kebijakan iuran BPJS yang sudah disahkan (riwayat tetap tersimpan)", AccessType = AccessTypes.Update, SortOrder = 7)]
    [AccessPermission("PayrollBpjsPolicy", "Retire")]
    public async Task<IActionResult> Retire(Guid id, [FromBody] PayrollStatutoryPolicyActionRequest request, CancellationToken cancellationToken)
    {
        var caller = await ResolveCallerAsync();
        return await PayrollControllerSupport.RunAsync(
            () => _service.RetireAsync(id, request.Reason, PayrollControllerSupport.CurrentUserId(User), caller, cancellationToken), "Kebijakan iuran BPJS berhasil dipensiunkan.");
    }

    [HttpDelete("{id:guid}")]
    [AccessAction("Delete", "Delete Payroll BPJS Policy", Description = "Menghapus draf kebijakan iuran BPJS yang belum pernah disahkan", AccessType = AccessTypes.Delete, SortOrder = 8)]
    [AccessPermission("PayrollBpjsPolicy", "Delete")]
    public Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        PayrollControllerSupport.RunAsync(
            () => _service.DeleteAsync(id, PayrollControllerSupport.CurrentUserId(User), cancellationToken), "Draf kebijakan iuran BPJS berhasil dihapus.");
}
