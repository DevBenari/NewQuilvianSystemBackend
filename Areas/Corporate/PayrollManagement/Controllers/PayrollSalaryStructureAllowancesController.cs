using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Controllers;

/// <summary>
/// Tunjangan tetap per Struktur Gaji (PAY-BE-16, kontrak PGJ-C1). Sub-resource master: baris selalu dikelola dari dalam struktur
/// induknya, sehingga sembilan endpoint baseline master data tidak dipakai (ringkasan, opsi, dan saringan dipegang endpoint struktur).
/// Seluruh akses data lewat <see cref="PayrollSalaryStructureAllowanceService"/>.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/payroll-management/master-data/salary-structures/{salaryStructureId:guid}/allowances")]
[AccessController(
    moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
    moduleName: "Human Resource Master Data",
    displayName: "Salary Structure Allowance",
    AreaName = "Corporate",
    ControllerName = "SalaryStructureAllowance",
    Description = "Tunjangan tetap per struktur gaji (nominal tetap atau persentase gaji pokok)",
    SortOrder = 46)]
[Tags("Corporate / Payroll Management / Master Data / Salary Structure Allowance")]
public sealed class PayrollSalaryStructureAllowancesController : ControllerBase
{
    private const string Resource = "SalaryStructureAllowance";

    private readonly PayrollSalaryStructureAllowanceService _service;
    private readonly AccessPermissionService _accessPermissionService;

    public PayrollSalaryStructureAllowancesController(
        PayrollSalaryStructureAllowanceService service,
        AccessPermissionService accessPermissionService)
    {
        _service = service;
        _accessPermissionService = accessPermissionService;
    }

    /// <summary>Izin nominal menurut Akses Role. Tanpa izin ini nominal, persen, dan batas maksimum null di setiap respons (bukan nol).</summary>
    private Task<bool> CanViewAmountAsync() => _accessPermissionService.HasAccessAsync(User, Resource, "ViewAmount");

    [HttpGet]
    [AccessAction("Read", "Read Salary Structure Allowance", Description = "Melihat tunjangan tetap satu struktur gaji", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("SalaryStructureAllowance", "Read")]
    public Task<IActionResult> List(Guid salaryStructureId, CancellationToken cancellationToken) =>
        PayrollControllerSupport.RunAsync(
            async () =>
            {
                var canViewAmount = await CanViewAmountAsync();
                var items = await _service.ListAsync(salaryStructureId, cancellationToken);
                return PayrollSalaryStructureAllowanceService.ApplyAmountVisibility(items, canViewAmount);
            },
            "Tunjangan struktur gaji berhasil dimuat.");

    /// <summary>
    /// Pintu izin nominal (pola ViewAmount di repo): endpoint ini sengaja ada agar kemampuan "melihat nominal tunjangan struktur" muncul
    /// di layar Akses Role. Pemilik izin ini juga melihat nominal pada daftar dan pada respons tambah/ubah.
    /// </summary>
    [HttpGet("{id:guid}/amount")]
    [AccessAction("ViewAmount", "View Salary Structure Allowance Amount", Description = "Melihat nominal dan persentase tunjangan tetap struktur gaji", AccessType = AccessTypes.Read, SortOrder = 5)]
    [AccessPermission("SalaryStructureAllowance", "ViewAmount")]
    public Task<IActionResult> GetAmount(Guid salaryStructureId, Guid id, CancellationToken cancellationToken) =>
        PayrollControllerSupport.RunAsync(
            async () => PayrollSalaryStructureAllowanceService.ApplyAmountVisibility(
                await _service.GetAmountAsync(salaryStructureId, id, cancellationToken), canViewAmount: true),
            "Nominal tunjangan struktur gaji berhasil dimuat.");

    [HttpPost]
    [AccessAction("Create", "Create Salary Structure Allowance", Description = "Menambah jenis tunjangan ke struktur gaji", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("SalaryStructureAllowance", "Create")]
    public Task<IActionResult> Create(
        Guid salaryStructureId,
        [FromBody] CreateSalaryStructureAllowanceRequest request,
        CancellationToken cancellationToken) =>
        PayrollControllerSupport.RunAsync(
            async () => PayrollSalaryStructureAllowanceService.ApplyAmountVisibility(
                await _service.CreateAsync(salaryStructureId, request, PayrollControllerSupport.CurrentUserId(User), cancellationToken),
                await CanViewAmountAsync()),
            "Tunjangan berhasil ditambahkan ke struktur gaji.");

    [HttpPut("{id:guid}")]
    [AccessAction("Update", "Update Salary Structure Allowance", Description = "Mengubah cara hitung, nilai, atau status tunjangan struktur", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("SalaryStructureAllowance", "Update")]
    public Task<IActionResult> Update(
        Guid salaryStructureId,
        Guid id,
        [FromBody] UpdateSalaryStructureAllowanceRequest request,
        CancellationToken cancellationToken) =>
        PayrollControllerSupport.RunAsync(
            async () => PayrollSalaryStructureAllowanceService.ApplyAmountVisibility(
                await _service.UpdateAsync(salaryStructureId, id, request, PayrollControllerSupport.CurrentUserId(User), cancellationToken),
                await CanViewAmountAsync()),
            "Tunjangan struktur gaji berhasil diperbarui.");

    [HttpDelete("{id:guid}")]
    [AccessAction("Delete", "Delete Salary Structure Allowance", Description = "Menghapus tunjangan dari struktur gaji (ditandai)", AccessType = AccessTypes.Delete, SortOrder = 4)]
    [AccessPermission("SalaryStructureAllowance", "Delete")]
    public Task<IActionResult> Delete(
        Guid salaryStructureId,
        Guid id,
        [FromQuery] string? expectedRowVersion,
        [FromQuery] bool acknowledgeUsedInPayroll,
        CancellationToken cancellationToken) =>
        PayrollControllerSupport.RunAsync(
            () => _service.DeleteAsync(salaryStructureId, id, expectedRowVersion, acknowledgeUsedInPayroll, PayrollControllerSupport.CurrentUserId(User), cancellationToken),
            "Tunjangan berhasil dihapus dari struktur gaji.");
}
