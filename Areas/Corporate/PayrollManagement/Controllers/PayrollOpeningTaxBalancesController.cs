using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Controllers;

/// <summary>
/// Saldo awal PPh historis (Opening Tax Balance, PAY-BE-FIX-016): jalur sah melengkapi riwayat tahun pajak yang tidak punya payroll run di
/// Quilvian, untuk rekonsiliasi masa pajak terakhir. Arketipe: master dengan lifecycle persetujuan, sehingga perpindahan status adalah
/// aksi bernama (<c>submit</c>, <c>approve</c>, <c>reject</c>, <c>revise</c>), bukan <c>PATCH status</c> generik, dan tidak ada
/// <c>GET /options</c> karena saldo awal bukan isi dropdown; satu-satunya opsi adalah <c>GET workforce/options</c> (pegawai dan dokter pemilik saldo). Izin dipisah per kemampuan di Akses Role: Read, Create, Update, Submit,
/// Approve, Reject, Delete, dan ViewAmount (nominal sensitif). Pengaju boleh sekaligus penyetuju bila memegang izin Approve; penyetuju wajib punya ViewAmount.
/// Tidak pernah mengubah payroll run yang sudah final.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/payroll-management/opening-tax-balances")]
[AccessController(
    moduleCode: "CORPORATE_PAYROLL_COMPENSATION",
    moduleName: "Corporate Payroll Compensation",
    displayName: "Payroll Opening Tax Balance",
    AreaName = "Corporate",
    ControllerName = "PayrollOpeningTaxBalance",
    Description = "Saldo awal PPh historis (penghasilan dan PPh bulan sebelum cutover) yang disetujui untuk rekonsiliasi masa pajak terakhir",
    SortOrder = 71)]
[Tags("Corporate / Payroll Management / Opening Tax Balance")]
public sealed class PayrollOpeningTaxBalancesController : ControllerBase
{
    private const string Resource = "PayrollOpeningTaxBalance";

    private readonly PayrollOpeningTaxBalanceService _service;
    private readonly AccessPermissionService _accessPermissionService;

    public PayrollOpeningTaxBalancesController(PayrollOpeningTaxBalanceService service, AccessPermissionService accessPermissionService)
    {
        _service = service;
        _accessPermissionService = accessPermissionService;
    }

    private Task<bool> HasAsync(string action) => _accessPermissionService.HasAccessAsync(User, Resource, action);

    private async Task<PayrollOpeningTaxBalanceCaller> ResolveCallerAsync() => new(
        CanViewAmount: await HasAsync("ViewAmount"),
        CanUpdate: await HasAsync("Update"),
        CanSubmit: await HasAsync("Submit"),
        CanApprove: await HasAsync("Approve"),
        CanReject: await HasAsync("Reject"),
        CanCreate: await HasAsync("Create"),
        CanDelete: await HasAsync("Delete"));

    /// <summary>Kosakata status dan sumber, serta apakah penyimpanan saldo awal sudah tersedia di database.</summary>
    [HttpGet("filters/metadata")]
    [AccessAction("Read", "Read Payroll Opening Tax Balance", Description = "Melihat saldo awal PPh historis", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("PayrollOpeningTaxBalance", "Read")]
    public Task<IActionResult> GetMetadata(CancellationToken cancellationToken) =>
        PayrollControllerSupport.RunAsync(() => _service.GetMetadataAsync(cancellationToken), "Metadata saldo awal PPh berhasil dimuat.");

    [HttpGet]
    [AccessAction("Read", "Read Payroll Opening Tax Balance", Description = "Melihat saldo awal PPh historis", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("PayrollOpeningTaxBalance", "Read")]
    public async Task<IActionResult> List([FromQuery] PayrollOpeningTaxBalanceQuery query, CancellationToken cancellationToken)
    {
        var caller = await ResolveCallerAsync();
        return await PayrollControllerSupport.RunAsync(() => _service.ListAsync(query, caller, cancellationToken), "Daftar saldo awal PPh berhasil dimuat.");
    }

    /// <summary>
    /// PAY-BE-FIX-035: opsi pemilik saldo (pegawai dan dokter) dengan value = WorkforceProfileId, berpencarian dan berhalaman. Dipakai form
    /// tambah saldo awal; izin Create karena hanya penyusun draf yang memilih pemilik. Murni baca.
    /// </summary>
    [HttpGet("workforce/options")]
    [AccessAction("Create", "Create Payroll Opening Tax Balance", Description = "Menyusun draf saldo awal PPh historis", AccessType = AccessTypes.Create, SortOrder = 3)]
    [AccessPermission("PayrollOpeningTaxBalance", "Create")]
    public Task<IActionResult> GetWorkforceOptions([FromQuery] PayrollOpeningTaxBalanceWorkforceQuery query, CancellationToken cancellationToken) =>
        PayrollControllerSupport.RunAsync(() => _service.GetWorkforceOptionsAsync(query, cancellationToken), "Pilihan pegawai dan dokter berhasil dimuat.");

    /// <summary>PAY-BE-FIX-036: template impor CSV (kolom, petunjuk, kode sumber, batas baris/ukuran). Tanpa data pegawai atau nominal.</summary>
    [HttpGet("import/template")]
    [AccessAction("Create", "Create Payroll Opening Tax Balance", Description = "Menyusun draf saldo awal PPh historis", AccessType = AccessTypes.Create, SortOrder = 3)]
    [AccessPermission("PayrollOpeningTaxBalance", "Create")]
    public Task<IActionResult> GetImportTemplate() =>
        PayrollControllerSupport.RunAsync(() => Task.FromResult(_service.GetImportTemplate()), "Template impor saldo awal PPh berhasil dimuat.");

    /// <summary>
    /// PAY-BE-FIX-036: validasi seluruh baris (baca-saja, tidak menulis). Selalu 200 dengan hasil per baris; baris bermasalah ditandai.
    /// Membutuhkan Create dan ViewAmount (nominal tampil pada hasil).
    /// </summary>
    [HttpPost("import/validate")]
    [AccessAction("Create", "Create Payroll Opening Tax Balance", Description = "Menyusun draf saldo awal PPh historis", AccessType = AccessTypes.Create, SortOrder = 3)]
    [AccessPermission("PayrollOpeningTaxBalance", "Create")]
    public async Task<IActionResult> ValidateImport([FromBody] PayrollOpeningTaxBalanceImportRequest request, CancellationToken cancellationToken)
    {
        var caller = await ResolveCallerAsync();
        return await PayrollControllerSupport.RunAsync(() => _service.ValidateImportAsync(request, caller, cancellationToken), "Validasi impor saldo awal PPh selesai.");
    }

    /// <summary>
    /// PAY-BE-FIX-036: simpan semua baris sebagai DRAFT atau tidak sama sekali. Validasi diulang di dalam transaksi; bila ada baris bermasalah
    /// respons 422 membawa hasil per baris di <c>errors</c> dan tidak ada yang tersimpan. Tidak mengajukan, menyetujui, atau menimpa apa pun.
    /// </summary>
    [HttpPost("import/commit")]
    [AccessAction("Create", "Create Payroll Opening Tax Balance", Description = "Menyusun draf saldo awal PPh historis", AccessType = AccessTypes.Create, SortOrder = 3)]
    [AccessPermission("PayrollOpeningTaxBalance", "Create")]
    public async Task<IActionResult> CommitImport([FromBody] PayrollOpeningTaxBalanceImportRequest request, CancellationToken cancellationToken)
    {
        var caller = await ResolveCallerAsync();
        try
        {
            var result = await _service.CommitImportAsync(request, PayrollControllerSupport.CurrentUserId(User), caller, cancellationToken);
            return Ok(ApiResponse<PayrollOpeningTaxBalanceImportResult>.Ok(result, result.Message));
        }
        catch (PayrollOpeningTaxBalanceImportException ex)
        {
            return new ObjectResult(ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, ex.Message, ex.Result))
            {
                StatusCode = StatusCodes.Status422UnprocessableEntity
            };
        }
        catch (Exception ex) when (PayrollControllerSupport.IsHandled(ex))
        {
            return PayrollControllerSupport.Failure(ex);
        }
    }

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Payroll Opening Tax Balance", Description = "Melihat saldo awal PPh historis", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("PayrollOpeningTaxBalance", "Read")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var caller = await ResolveCallerAsync();
        return await PayrollControllerSupport.RunAsync(() => _service.GetAsync(id, caller, cancellationToken), "Saldo awal PPh berhasil dimuat.");
    }

    /// <summary>
    /// Pintu izin nominal (pola ViewAmount di repo): endpoint ini sengaja ada agar kemampuan "melihat dan mengisi nominal saldo awal PPh"
    /// muncul di layar Akses Role. Tanpa izin ini nominal pada respons bernilai null (bukan nol), dan nominal tidak dapat diisi, disetujui, atau ditolak.
    /// </summary>
    [HttpGet("{id:guid}/amount")]
    [AccessAction("ViewAmount", "View Payroll Opening Tax Balance Amount", Description = "Melihat dan mengisi nominal saldo awal PPh historis; juga prasyarat menyetujui atau menolak", AccessType = AccessTypes.Read, SortOrder = 2)]
    [AccessPermission("PayrollOpeningTaxBalance", "ViewAmount")]
    public async Task<IActionResult> GetAmount(Guid id, CancellationToken cancellationToken)
    {
        var caller = await ResolveCallerAsync();
        return await PayrollControllerSupport.RunAsync(() => _service.GetAsync(id, caller, cancellationToken), "Saldo awal PPh berhasil dimuat.");
    }

    [HttpPost]
    [AccessAction("Create", "Create Payroll Opening Tax Balance", Description = "Menyusun draf saldo awal PPh historis", AccessType = AccessTypes.Create, SortOrder = 3)]
    [AccessPermission("PayrollOpeningTaxBalance", "Create")]
    public async Task<IActionResult> Create([FromBody] SavePayrollOpeningTaxBalanceRequest request, CancellationToken cancellationToken)
    {
        var caller = await ResolveCallerAsync();
        return await PayrollControllerSupport.RunAsync(
            () => _service.CreateAsync(request, PayrollControllerSupport.CurrentUserId(User), caller, cancellationToken), "Draf saldo awal PPh berhasil disimpan.");
    }

    [HttpPut("{id:guid}")]
    [AccessAction("Update", "Update Payroll Opening Tax Balance", Description = "Mengubah draf saldo awal PPh yang belum diajukan", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("PayrollOpeningTaxBalance", "Update")]
    public async Task<IActionResult> Update(Guid id, [FromBody] SavePayrollOpeningTaxBalanceRequest request, CancellationToken cancellationToken)
    {
        var caller = await ResolveCallerAsync();
        return await PayrollControllerSupport.RunAsync(
            () => _service.UpdateAsync(id, request, PayrollControllerSupport.CurrentUserId(User), caller, cancellationToken), "Draf saldo awal PPh berhasil diperbarui.");
    }

    /// <summary>Membuat draf versi baru dari saldo yang sudah disetujui atau ditolak; versi lama utuh sampai versi baru disetujui.</summary>
    [HttpPost("{id:guid}/revise")]
    [AccessAction("Create", "Create Payroll Opening Tax Balance", Description = "Menyusun draf saldo awal PPh historis", AccessType = AccessTypes.Create, SortOrder = 3)]
    [AccessPermission("PayrollOpeningTaxBalance", "Create")]
    public async Task<IActionResult> Revise(Guid id, CancellationToken cancellationToken)
    {
        var caller = await ResolveCallerAsync();
        return await PayrollControllerSupport.RunAsync(
            () => _service.ReviseAsync(id, PayrollControllerSupport.CurrentUserId(User), caller, cancellationToken), "Revisi saldo awal PPh berhasil dibuat sebagai draf.");
    }

    [HttpPost("{id:guid}/submit")]
    [AccessAction("Submit", "Submit Payroll Opening Tax Balance", Description = "Mengajukan saldo awal PPh untuk disetujui", AccessType = AccessTypes.Update, SortOrder = 5)]
    [AccessPermission("PayrollOpeningTaxBalance", "Submit")]
    public async Task<IActionResult> Submit(Guid id, CancellationToken cancellationToken)
    {
        var caller = await ResolveCallerAsync();
        return await PayrollControllerSupport.RunAsync(
            () => _service.SubmitAsync(id, PayrollControllerSupport.CurrentUserId(User), caller, cancellationToken), "Saldo awal PPh berhasil diajukan.");
    }

    [HttpPost("{id:guid}/approve")]
    [AccessAction("Approve", "Approve Payroll Opening Tax Balance", Description = "Menyetujui saldo awal PPh; penyetuju harus punya izin melihat nominal (pengaju boleh menyetujui sendiri bila memegang izin ini)", AccessType = AccessTypes.Update, SortOrder = 6)]
    [AccessPermission("PayrollOpeningTaxBalance", "Approve")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] PayrollOpeningTaxBalanceDecisionRequest? request, CancellationToken cancellationToken)
    {
        var caller = await ResolveCallerAsync();
        return await PayrollControllerSupport.RunAsync(
            () => _service.ApproveAsync(id, request?.Note, PayrollControllerSupport.CurrentUserId(User), caller, cancellationToken), "Saldo awal PPh berhasil disetujui.");
    }

    [HttpPost("{id:guid}/reject")]
    [AccessAction("Reject", "Reject Payroll Opening Tax Balance", Description = "Menolak saldo awal PPh dengan alasan", AccessType = AccessTypes.Update, SortOrder = 7)]
    [AccessPermission("PayrollOpeningTaxBalance", "Reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] PayrollOpeningTaxBalanceDecisionRequest? request, CancellationToken cancellationToken)
    {
        var caller = await ResolveCallerAsync();
        return await PayrollControllerSupport.RunAsync(
            () => _service.RejectAsync(id, request?.Note, PayrollControllerSupport.CurrentUserId(User), caller, cancellationToken), "Saldo awal PPh berhasil ditolak.");
    }

    [HttpDelete("{id:guid}")]
    [AccessAction("Delete", "Delete Payroll Opening Tax Balance", Description = "Menghapus draf atau saldo awal PPh yang ditolak (yang lain adalah riwayat)", AccessType = AccessTypes.Delete, SortOrder = 8)]
    [AccessPermission("PayrollOpeningTaxBalance", "Delete")]
    public Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        PayrollControllerSupport.RunAsync(
            () => _service.DeleteAsync(id, PayrollControllerSupport.CurrentUserId(User), cancellationToken), "Saldo awal PPh berhasil dihapus.");
}
