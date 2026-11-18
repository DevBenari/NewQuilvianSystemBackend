using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Controllers;

/// <summary>
/// Kebijakan tarif PPh berversi (PrlTaxPolicy): sumber tunggal tarif untuk skema AUTO_REGULATORY. Arketipe: master dengan lifecycle
/// persetujuan, sehingga perpindahan status adalah aksi bernama (<c>submit</c>, <c>approve</c>, <c>reject</c>, <c>retire</c>) dan bukan
/// <c>PATCH status</c> generik. Izin dipisah per kemampuan di Akses Role: Read, Create, Update, Submit (HRD menyatakan isi sudah
/// diverifikasi), Approve dan Reject (Finance mengesahkan; pengesah selalu berbeda dari pengaju), Retire, Delete. Tidak ada tarif
/// bawaan: seluruh angka dimasukkan HRD dan disahkan Finance.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/payroll-management/tax-policies")]
[AccessController(
    moduleCode: "CORPORATE_PAYROLL_COMPENSATION",
    moduleName: "Corporate Payroll Compensation",
    displayName: "Payroll Tax Policy",
    AreaName = "Corporate",
    ControllerName = "PayrollTaxPolicy",
    Description = "Kebijakan tarif PPh berversi (TER, PTKP, lapisan Pasal 17, dasar peraturan) yang diverifikasi HRD dan disahkan Finance",
    SortOrder = 70)]
[Tags("Corporate / Payroll Management / Tax Policy")]
public sealed class PayrollTaxPoliciesController : ControllerBase
{
    private const string Resource = "PayrollTaxPolicy";

    private readonly PayrollTaxPolicyService _service;
    private readonly PayrollStatutoryPolicyResolver _resolver;
    private readonly AccessPermissionService _accessPermissionService;

    public PayrollTaxPoliciesController(
        PayrollTaxPolicyService service,
        PayrollStatutoryPolicyResolver resolver,
        AccessPermissionService accessPermissionService)
    {
        _service = service;
        _resolver = resolver;
        _accessPermissionService = accessPermissionService;
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
    [AccessAction("Read", "Read Payroll Tax Policy", Description = "Melihat kebijakan tarif PPh berversi", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("PayrollTaxPolicy", "Read")]
    public IActionResult GetMetadata() =>
        Ok(ApiResponse<PayrollStatutoryPolicyMetadataResponse>.Ok(
            PayrollStatutoryPolicyMetadataFactory.Build(), "Metadata kebijakan tarif PPh berhasil dimuat."));

    /// <summary>Ketersediaan kebijakan yang disahkan pada tanggal tertentu (bawaan hari ini): "Menunggu Verifikasi Kebijakan" bila belum ada.</summary>
    [HttpGet("availability")]
    [AccessAction("Read", "Read Payroll Tax Policy", Description = "Melihat kebijakan tarif PPh berversi", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("PayrollTaxPolicy", "Read")]
    public Task<IActionResult> GetAvailability([FromQuery] DateTime? asOf, CancellationToken cancellationToken) =>
        PayrollControllerSupport.RunAsync(
            () => _resolver.GetAvailabilityResponseAsync(asOf ?? DateTime.UtcNow, cancellationToken), "Ketersediaan kebijakan berhasil dimuat.");

    [HttpGet]
    [AccessAction("Read", "Read Payroll Tax Policy", Description = "Melihat kebijakan tarif PPh berversi", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("PayrollTaxPolicy", "Read")]
    public Task<IActionResult> List([FromQuery] PayrollStatutoryPolicyQuery query, CancellationToken cancellationToken) =>
        PayrollControllerSupport.RunAsync(() => _service.ListAsync(query, cancellationToken), "Daftar kebijakan tarif PPh berhasil dimuat.");

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Payroll Tax Policy", Description = "Melihat kebijakan tarif PPh berversi", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("PayrollTaxPolicy", "Read")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var caller = await ResolveCallerAsync();
        return await PayrollControllerSupport.RunAsync(() => _service.GetAsync(id, caller, cancellationToken), "Kebijakan tarif PPh berhasil dimuat.");
    }

    [HttpPost]
    [AccessAction("Create", "Create Payroll Tax Policy", Description = "Menyusun draf kebijakan tarif PPh", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("PayrollTaxPolicy", "Create")]
    public async Task<IActionResult> Create([FromBody] SavePayrollTaxPolicyRequest request, CancellationToken cancellationToken)
    {
        var caller = await ResolveCallerAsync();
        return await PayrollControllerSupport.RunAsync(
            () => _service.CreateAsync(request, PayrollControllerSupport.CurrentUserId(User), caller, cancellationToken), "Draf kebijakan tarif PPh berhasil disimpan.");
    }

    [HttpPut("{id:guid}")]
    [AccessAction("Update", "Update Payroll Tax Policy", Description = "Mengubah draf kebijakan tarif PPh yang belum diajukan", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("PayrollTaxPolicy", "Update")]
    public async Task<IActionResult> Update(Guid id, [FromBody] SavePayrollTaxPolicyRequest request, CancellationToken cancellationToken)
    {
        var caller = await ResolveCallerAsync();
        return await PayrollControllerSupport.RunAsync(
            () => _service.UpdateAsync(id, request, PayrollControllerSupport.CurrentUserId(User), caller, cancellationToken), "Draf kebijakan tarif PPh berhasil diperbarui.");
    }

    [HttpPost("{id:guid}/revise")]
    [AccessAction("Create", "Create Payroll Tax Policy", Description = "Menyusun draf kebijakan tarif PPh", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("PayrollTaxPolicy", "Create")]
    public async Task<IActionResult> Revise(Guid id, [FromBody] PayrollStatutoryPolicyReviseRequest request, CancellationToken cancellationToken)
    {
        var caller = await ResolveCallerAsync();
        return await PayrollControllerSupport.RunAsync(
            () => _service.ReviseAsync(id, request, PayrollControllerSupport.CurrentUserId(User), caller, cancellationToken), "Revisi kebijakan tarif PPh berhasil dibuat sebagai draf.");
    }

    [HttpPost("{id:guid}/submit")]
    [AccessAction("Submit", "Submit Payroll Tax Policy", Description = "Menyatakan kebijakan tarif PPh sudah diverifikasi terhadap dasar peraturan dan mengajukannya untuk disahkan", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("PayrollTaxPolicy", "Submit")]
    public async Task<IActionResult> Submit(Guid id, CancellationToken cancellationToken)
    {
        var caller = await ResolveCallerAsync();
        return await PayrollControllerSupport.RunAsync(
            () => _service.SubmitAsync(id, PayrollControllerSupport.CurrentUserId(User), caller, cancellationToken), "Kebijakan tarif PPh berhasil diajukan.");
    }

    [HttpPost("{id:guid}/approve")]
    [AccessAction("Approve", "Approve Payroll Tax Policy", Description = "Mengesahkan kebijakan tarif PPh; pengesah harus berbeda dari pengaju", AccessType = AccessTypes.Update, SortOrder = 5)]
    [AccessPermission("PayrollTaxPolicy", "Approve")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        var caller = await ResolveCallerAsync();
        return await PayrollControllerSupport.RunAsync(
            () => _service.ApproveAsync(id, PayrollControllerSupport.CurrentUserId(User), caller, cancellationToken), "Kebijakan tarif PPh berhasil disahkan.");
    }

    [HttpPost("{id:guid}/reject")]
    [AccessAction("Reject", "Reject Payroll Tax Policy", Description = "Menolak kebijakan tarif PPh dengan alasan", AccessType = AccessTypes.Update, SortOrder = 6)]
    [AccessPermission("PayrollTaxPolicy", "Reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] PayrollStatutoryPolicyActionRequest request, CancellationToken cancellationToken)
    {
        var caller = await ResolveCallerAsync();
        return await PayrollControllerSupport.RunAsync(
            () => _service.RejectAsync(id, request.Reason, PayrollControllerSupport.CurrentUserId(User), caller, cancellationToken), "Kebijakan tarif PPh berhasil ditolak.");
    }

    [HttpPost("{id:guid}/retire")]
    [AccessAction("Retire", "Retire Payroll Tax Policy", Description = "Mempensiunkan kebijakan tarif PPh yang sudah disahkan (riwayat tetap tersimpan)", AccessType = AccessTypes.Update, SortOrder = 7)]
    [AccessPermission("PayrollTaxPolicy", "Retire")]
    public async Task<IActionResult> Retire(Guid id, [FromBody] PayrollStatutoryPolicyActionRequest request, CancellationToken cancellationToken)
    {
        var caller = await ResolveCallerAsync();
        return await PayrollControllerSupport.RunAsync(
            () => _service.RetireAsync(id, request.Reason, PayrollControllerSupport.CurrentUserId(User), caller, cancellationToken), "Kebijakan tarif PPh berhasil dipensiunkan.");
    }

    [HttpDelete("{id:guid}")]
    [AccessAction("Delete", "Delete Payroll Tax Policy", Description = "Menghapus draf kebijakan tarif PPh yang belum pernah disahkan", AccessType = AccessTypes.Delete, SortOrder = 8)]
    [AccessPermission("PayrollTaxPolicy", "Delete")]
    public Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        PayrollControllerSupport.RunAsync(
            () => _service.DeleteAsync(id, PayrollControllerSupport.CurrentUserId(User), cancellationToken), "Draf kebijakan tarif PPh berhasil dihapus.");
}

/// <summary>Opsi metadata kebijakan regulasi (kosakata kode; tidak memuat angka regulasi).</summary>
internal static class PayrollStatutoryPolicyMetadataFactory
{
    public static PayrollStatutoryPolicyMetadataResponse Build() => new()
    {
        StatusOptions = new List<PayrollStatutoryPolicyOptionResponse>
        {
            new() { Value = StatutoryPolicyStatuses.Draft, Label = "Draf" },
            new() { Value = StatutoryPolicyStatuses.PendingApproval, Label = "Menunggu Pengesahan" },
            new() { Value = StatutoryPolicyStatuses.Approved, Label = "Disahkan" },
            new() { Value = StatutoryPolicyStatuses.Rejected, Label = "Ditolak" },
            new() { Value = StatutoryPolicyStatuses.Retired, Label = "Dipensiunkan" }
        },
        TerCategoryOptions = new List<PayrollStatutoryPolicyOptionResponse>
        {
            new() { Value = TaxTerCategories.A, Label = "Kategori A" },
            new() { Value = TaxTerCategories.B, Label = "Kategori B" },
            new() { Value = TaxTerCategories.C, Label = "Kategori C" }
        },
        TerPeriodKindOptions = new List<PayrollStatutoryPolicyOptionResponse>
        {
            new() { Value = TaxTerPeriodKinds.Monthly, Label = "Bulanan" },
            new() { Value = TaxTerPeriodKinds.Daily, Label = "Harian", Note = "Opsional; mesin payroll saat ini hanya memakai tabel bulanan." }
        },
        NonEmployeeMethodOptions = new List<PayrollStatutoryPolicyOptionResponse>
        {
            new() { Value = TaxNonEmployeeMethods.PerPayment, Label = "Per pembayaran" },
            new() { Value = TaxNonEmployeeMethods.Cumulative, Label = "Kumulatif tahun berjalan" }
        },
        ProgramOptions = new List<PayrollStatutoryPolicyOptionResponse>
        {
            new() { Value = "KES", Label = "BPJS Kesehatan" },
            new() { Value = "JHT", Label = "JHT" },
            new() { Value = "JP", Label = "JP" },
            new() { Value = "JKK", Label = "JKK" },
            new() { Value = "JKM", Label = "JKM" },
            new() { Value = "PRIVATE", Label = "Asuransi swasta" }
        },
        JkkRiskClassOptions = new List<PayrollStatutoryPolicyOptionResponse>
        {
            new() { Value = "1", Label = "Kelompok risiko 1" },
            new() { Value = "2", Label = "Kelompok risiko 2" },
            new() { Value = "3", Label = "Kelompok risiko 3" },
            new() { Value = "4", Label = "Kelompok risiko 4" },
            new() { Value = "5", Label = "Kelompok risiko 5" }
        }
    };
}
