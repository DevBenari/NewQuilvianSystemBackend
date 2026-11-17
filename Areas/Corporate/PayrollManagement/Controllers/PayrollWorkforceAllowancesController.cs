using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Controllers;

/// <summary>
/// Tunjangan Pegawai pada Detail Profil Penggajian. Arketipe: sub-proses ter-scope induk (standar transaksi 2.3) dengan lifecycle
/// penetapan: <c>POST</c> membuat, <c>PUT</c> menyunting isi, dan setiap perpindahan status adalah aksi bernama (<c>submit</c>,
/// <c>approve</c>, <c>reject</c>, <c>deactivate</c>). Tidak ada <c>DELETE</c>: penetapan tidak dihapus, dinonaktifkan dengan alasan,
/// supaya riwayat dan komponen payroll yang sudah memakainya tetap dapat ditelusuri. Tidak ada <c>GET /options</c>: pilihan jenis
/// tunjangan dibaca dari endpoint options Master Tunjangan yang sudah ada.
///
/// Induknya WorkforceProfileId (identitas HRD), bukan profil payroll: tunjangan dapat disiapkan sebelum PrlPayroll ada, dan membuka
/// layar ini tidak pernah membuat profil payroll. Izin dipisah per kemampuan di Akses Role: Read, ViewAmount (nominal dan persen),
/// Create, Update, Submit, Approve, Reject, Deactivate. Pemilik izin ReadDetail pada Profil Penggajian TIDAK otomatis boleh mengubah
/// tunjangan. Nominal dan persen null bagi pemanggil tanpa ViewAmount.
///
/// Seluruh akses data lewat <see cref="PayrollWorkforceAllowanceService"/>.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/payroll-management/workforce-profiles/{workforceProfileId:guid}/allowance-assignments")]
[AccessController(
    moduleCode: "CORPORATE_PAYROLL_COMPENSATION",
    moduleName: "Corporate Payroll Compensation",
    displayName: "Workforce Allowance Assignment",
    AreaName = "Corporate",
    ControllerName = "WorkforceAllowanceAssignment",
    Description = "Tunjangan tetap khusus pegawai berbasis Master Tunjangan pada Detail Profil Penggajian",
    SortOrder = 66)]
[Tags("Corporate / Payroll Management / Workforce Allowance Assignment")]
public sealed class PayrollWorkforceAllowancesController : ControllerBase
{
    private const string Resource = "WorkforceAllowanceAssignment";

    private readonly PayrollWorkforceAllowanceService _service;
    private readonly AccessPermissionService _accessPermissionService;

    public PayrollWorkforceAllowancesController(PayrollWorkforceAllowanceService service, AccessPermissionService accessPermissionService)
    {
        _service = service;
        _accessPermissionService = accessPermissionService;
    }

    private Task<bool> HasAsync(string action) => _accessPermissionService.HasAccessAsync(User, Resource, action);

    /// <summary>Kemampuan pemanggil menurut Akses Role; dipakai menyembunyikan nominal dan membentuk AvailableActions (bantuan tampilan, bukan pengaman).</summary>
    private async Task<WorkforceAllowanceCaller> ResolveCallerAsync() => new(
        CanViewAmount: await HasAsync("ViewAmount"),
        CanUpdate: await HasAsync("Update"),
        CanSubmit: await HasAsync("Submit"),
        CanApprove: await HasAsync("Approve"),
        CanReject: await HasAsync("Reject"),
        CanDeactivate: await HasAsync("Deactivate"));

    [HttpGet]
    [AccessAction("Read", "Read Workforce Allowance Assignment", Description = "Melihat tunjangan pegawai dari struktur gaji, penetapan khusus, dan kebijakan", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("WorkforceAllowanceAssignment", "Read")]
    public async Task<IActionResult> List(Guid workforceProfileId, [FromQuery] WorkforceAllowanceQuery query, CancellationToken cancellationToken)
    {
        var caller = await ResolveCallerAsync();
        return await PayrollControllerSupport.RunAsync(
            () => _service.GetPagedAsync(workforceProfileId, query, caller, cancellationToken), "Tunjangan pegawai berhasil dimuat.");
    }

    [HttpGet("summary")]
    [AccessAction("Read", "Read Workforce Allowance Assignment", Description = "Melihat tunjangan pegawai dari struktur gaji, penetapan khusus, dan kebijakan", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("WorkforceAllowanceAssignment", "Read")]
    public async Task<IActionResult> Summary(Guid workforceProfileId, CancellationToken cancellationToken)
    {
        var caller = await ResolveCallerAsync();
        return await PayrollControllerSupport.RunAsync(
            () => _service.GetSummaryAsync(workforceProfileId, caller, cancellationToken), "Ringkasan tunjangan pegawai berhasil dimuat.");
    }

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Workforce Allowance Assignment", Description = "Melihat tunjangan pegawai dari struktur gaji, penetapan khusus, dan kebijakan", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("WorkforceAllowanceAssignment", "Read")]
    public async Task<IActionResult> Get(Guid workforceProfileId, Guid id, CancellationToken cancellationToken)
    {
        var caller = await ResolveCallerAsync();
        return await PayrollControllerSupport.RunAsync(
            () => _service.GetAsync(workforceProfileId, id, caller, cancellationToken), "Penetapan tunjangan berhasil dimuat.");
    }

    /// <summary>
    /// Pintu izin nominal (pola ViewAmount di repo): endpoint ini sengaja ada agar kemampuan "melihat nominal tunjangan" muncul di layar
    /// Akses Role. Pemilik izin ini juga melihat nominal dan persen pada daftar, ringkasan, dan detail.
    /// </summary>
    [HttpGet("{id:guid}/amount")]
    [AccessAction("ViewAmount", "View Workforce Allowance Amount", Description = "Melihat nominal dan persentase tunjangan pegawai", AccessType = AccessTypes.Read, SortOrder = 2)]
    [AccessPermission("WorkforceAllowanceAssignment", "ViewAmount")]
    public async Task<IActionResult> GetAmount(Guid workforceProfileId, Guid id, CancellationToken cancellationToken)
    {
        var caller = (await ResolveCallerAsync()) with { CanViewAmount = true };
        return await PayrollControllerSupport.RunAsync(
            () => _service.GetAsync(workforceProfileId, id, caller, cancellationToken), "Nominal tunjangan berhasil dimuat.");
    }

    [HttpPost]
    [AccessAction("Create", "Create Workforce Allowance Assignment", Description = "Menetapkan tunjangan dari Master Tunjangan untuk satu pegawai", AccessType = AccessTypes.Create, SortOrder = 3)]
    [AccessPermission("WorkforceAllowanceAssignment", "Create")]
    public async Task<IActionResult> Create(Guid workforceProfileId, [FromBody] CreateWorkforceAllowanceRequest request, CancellationToken cancellationToken)
    {
        var caller = await ResolveCallerAsync();
        return await PayrollControllerSupport.RunAsync(
            () => _service.CreateAsync(workforceProfileId, request, PayrollControllerSupport.CurrentUserId(User), caller, cancellationToken),
            "Penetapan tunjangan berhasil disimpan.");
    }

    [HttpPut("{id:guid}")]
    [AccessAction("Update", "Update Workforce Allowance Assignment", Description = "Mengubah isi penetapan tunjangan pegawai yang belum dipakai payroll", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("WorkforceAllowanceAssignment", "Update")]
    public async Task<IActionResult> Update(Guid workforceProfileId, Guid id, [FromBody] UpdateWorkforceAllowanceRequest request, CancellationToken cancellationToken)
    {
        var caller = await ResolveCallerAsync();
        return await PayrollControllerSupport.RunAsync(
            () => _service.UpdateAsync(workforceProfileId, id, request, PayrollControllerSupport.CurrentUserId(User), caller, cancellationToken),
            "Penetapan tunjangan berhasil diperbarui.");
    }

    [HttpPost("{id:guid}/submit")]
    [AccessAction("Submit", "Submit Workforce Allowance Assignment", Description = "Mengajukan penetapan tunjangan untuk persetujuan", AccessType = AccessTypes.Update, SortOrder = 5)]
    [AccessPermission("WorkforceAllowanceAssignment", "Submit")]
    public async Task<IActionResult> Submit(Guid workforceProfileId, Guid id, [FromBody] WorkforceAllowanceActionRequest request, CancellationToken cancellationToken)
    {
        var caller = await ResolveCallerAsync();
        return await PayrollControllerSupport.RunAsync(
            () => _service.SubmitAsync(workforceProfileId, id, request, PayrollControllerSupport.CurrentUserId(User), caller, cancellationToken),
            "Penetapan tunjangan berhasil diajukan.");
    }

    [HttpPost("{id:guid}/approve")]
    [AccessAction("Approve", "Approve Workforce Allowance Assignment", Description = "Menyetujui penetapan tunjangan pegawai; pengaju boleh menyetujui sendiri", AccessType = AccessTypes.Update, SortOrder = 6)]
    [AccessPermission("WorkforceAllowanceAssignment", "Approve")]
    public async Task<IActionResult> Approve(Guid workforceProfileId, Guid id, [FromBody] WorkforceAllowanceActionRequest request, CancellationToken cancellationToken)
    {
        var caller = await ResolveCallerAsync();
        return await PayrollControllerSupport.RunAsync(
            () => _service.ApproveAsync(workforceProfileId, id, request, PayrollControllerSupport.CurrentUserId(User), caller, cancellationToken),
            "Penetapan tunjangan berhasil disetujui.");
    }

    [HttpPost("{id:guid}/reject")]
    [AccessAction("Reject", "Reject Workforce Allowance Assignment", Description = "Menolak penetapan tunjangan pegawai dengan alasan", AccessType = AccessTypes.Update, SortOrder = 7)]
    [AccessPermission("WorkforceAllowanceAssignment", "Reject")]
    public async Task<IActionResult> Reject(Guid workforceProfileId, Guid id, [FromBody] WorkforceAllowanceActionRequest request, CancellationToken cancellationToken)
    {
        var caller = await ResolveCallerAsync();
        return await PayrollControllerSupport.RunAsync(
            () => _service.RejectAsync(workforceProfileId, id, request, PayrollControllerSupport.CurrentUserId(User), caller, cancellationToken),
            "Penetapan tunjangan berhasil ditolak.");
    }

    [HttpPost("{id:guid}/deactivate")]
    [AccessAction("Deactivate", "Deactivate Workforce Allowance Assignment", Description = "Menonaktifkan penetapan tunjangan pegawai dengan alasan (riwayat tetap tersimpan)", AccessType = AccessTypes.Update, SortOrder = 8)]
    [AccessPermission("WorkforceAllowanceAssignment", "Deactivate")]
    public async Task<IActionResult> Deactivate(Guid workforceProfileId, Guid id, [FromBody] WorkforceAllowanceActionRequest request, CancellationToken cancellationToken)
    {
        var caller = await ResolveCallerAsync();
        return await PayrollControllerSupport.RunAsync(
            () => _service.DeactivateAsync(workforceProfileId, id, request, PayrollControllerSupport.CurrentUserId(User), caller, cancellationToken),
            "Penetapan tunjangan berhasil dinonaktifkan.");
    }
}
