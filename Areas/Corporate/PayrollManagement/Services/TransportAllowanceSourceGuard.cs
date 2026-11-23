using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;

/// <summary>
/// Penjaga satu sumber pembayaran Transportasi (PAY-BE-FIX-022). Transportasi bulanan kini dapat berasal dari dua tempat: penetapan
/// transportasi lama (PrlTransportAllowance, metode FixedMonthly) dan penetapan Tunjangan Pegawai berbasis Kebijakan Tunjangan umum
/// (PrlWorkforceAllowance, AssignmentSource = Policy, kategori Transport). Keduanya dibayar mesin hitung, sehingga yang satu tidak boleh
/// dibuat selama yang lain berlaku pada rentang tanggal yang beririsan. Pemeriksaan ini dipakai kedua jalur penulisan dan mesin hitung
/// menjadi jaring pengaman terakhir (menggagalkan pegawai bila data tetap beririsan).
/// </summary>
internal static class TransportAllowanceSourceGuard
{
    private const string FixedMonthly = "FixedMonthly";

    /// <summary>Ada penetapan Transportasi berbasis kebijakan umum (Draft, menunggu persetujuan, atau disetujui) yang beririsan.</summary>
    public static Task<bool> HasUnifiedTransportAsync(
        ApplicationDbContext dbContext, Guid workforceProfileId, DateTime? start, DateTime? end, CancellationToken cancellationToken) =>
        dbContext.PrlWorkforceAllowances.AsNoTracking().AnyAsync(x =>
            !x.IsDelete
            && x.WorkforceProfileId == workforceProfileId
            && x.AssignmentSource == WorkforceAllowanceAssignmentSources.Policy
            && x.AllowanceType != null && x.AllowanceType.AllowanceCategory != null
            && x.AllowanceType.AllowanceCategory.ToLower() == "transport"
            && (x.AllowanceStatus == WorkforceAllowanceStatuses.Draft
                || x.AllowanceStatus == WorkforceAllowanceStatuses.PendingApproval
                || x.AllowanceStatus == WorkforceAllowanceStatuses.Approved)
            && (end == null || x.EffectiveStartDate <= end)
            && (x.EffectiveEndDate == null || start == null || x.EffectiveEndDate >= start),
            cancellationToken);

    /// <summary>
    /// Ada penetapan transportasi lama yang aktif, bermetode FixedMonthly, dan ikut payroll pada rentang yang beririsan (yaitu yang
    /// dibayar mesin hitung). Metode lain (per kehadiran, dst.) tidak dibayar mesin hitung sehingga tidak dihitung ganda.
    /// </summary>
    public static Task<bool> HasLegacyMonthlyTransportAsync(
        ApplicationDbContext dbContext, Guid workforceProfileId, DateTime start, DateTime? end, Guid? excludeId, CancellationToken cancellationToken) =>
        dbContext.PrlTransportAllowances.AsNoTracking().AnyAsync(x =>
            !x.IsDelete && x.IsActive
            && x.WorkforceProfileId == workforceProfileId
            && (excludeId == null || x.Id != excludeId)
            && x.AllowanceStatus == "Active"
            && x.TransportAllowancePolicy != null
            && !x.TransportAllowancePolicy.IsDelete && x.TransportAllowancePolicy.IsActive
            && x.TransportAllowancePolicy.IsIncludedInPayroll
            && x.TransportAllowancePolicy.CalculationMethod == FixedMonthly
            && (end == null || x.EffectiveStartDate == null || x.EffectiveStartDate <= end)
            && (x.EffectiveEndDate == null || x.EffectiveEndDate >= start),
            cancellationToken);

    /// <summary>Apakah kebijakan transportasi lama ini dibayar mesin hitung sebagai transport bulanan tetap.</summary>
    public static Task<bool> IsPaidMonthlyPolicyAsync(ApplicationDbContext dbContext, Guid? policyId, CancellationToken cancellationToken) =>
        policyId.HasValue && policyId.Value != Guid.Empty
            ? dbContext.PrlTransportAllowancePolicies.AsNoTracking().AnyAsync(p =>
                p.Id == policyId.Value && !p.IsDelete && p.IsActive && p.IsIncludedInPayroll && p.CalculationMethod == FixedMonthly,
                cancellationToken)
            : Task.FromResult(false);
}
