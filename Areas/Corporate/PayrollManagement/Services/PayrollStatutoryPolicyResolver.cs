using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Options;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;

/// <summary>
/// Pembaca kebijakan regulasi penggajian yang berlaku (murni baca): kebijakan tarif PPh berstatus APPROVED yang masa berlakunya
/// mencakup akhir periode. Kebijakan DRAFT, PENDING_APPROVAL, REJECTED, dan RETIRED tidak pernah dipakai mesin payroll, termasuk nilai
/// bawaan Pengaturan Penggajian yang belum diverifikasi. Kebijakan iuran BPJS tidak lagi dibaca (PAY-BE-FIX-007).
/// </summary>
public sealed class PayrollStatutoryPolicyResolver
{
    private readonly ApplicationDbContext _dbContext;

    public PayrollStatutoryPolicyResolver(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>Tanggal kalender dicap UTC: kolom kebijakan bertipe timestamptz dan Npgsql menolak Kind=Unspecified.</summary>
    private static DateTime ToUtcDate(DateTime value) => DateTime.SpecifyKind(value.Date, DateTimeKind.Utc);

    public async Task<PayrollTaxPolicy?> GetTaxPolicyAsync(DateTime asOf, CancellationToken cancellationToken)
    {
        var date = ToUtcDate(asOf);
        var entity = await _dbContext.PrlTaxPolicies.AsNoTracking()
            .Where(x => !x.IsDelete && x.IsActive && x.PolicyStatus == StatutoryPolicyStatuses.Approved
                && x.EffectiveStartDate <= date && (x.EffectiveEndDate == null || x.EffectiveEndDate >= date))
            .OrderByDescending(x => x.EffectiveStartDate)
            .ThenByDescending(x => x.DecidedAt)
            .FirstOrDefaultAsync(cancellationToken);
        return entity is null ? null : PayrollTaxPolicy.FromEntity(entity);
    }

    public async Task<PayrollPolicyAvailability> GetAvailabilityAsync(DateTime asOf, CancellationToken cancellationToken)
    {
        var tax = await GetTaxPolicyAsync(asOf, cancellationToken);
        return new PayrollPolicyAvailability(tax is not null, tax?.Version);
    }

    public async Task<PayrollStatutoryPolicyAvailabilityResponse> GetAvailabilityResponseAsync(DateTime asOf, CancellationToken cancellationToken)
    {
        var availability = await GetAvailabilityAsync(asOf, cancellationToken);
        var pendingTax = await _dbContext.PrlTaxPolicies.AsNoTracking()
            .CountAsync(x => !x.IsDelete && (x.PolicyStatus == StatutoryPolicyStatuses.Draft || x.PolicyStatus == StatutoryPolicyStatuses.PendingApproval), cancellationToken);

        return new PayrollStatutoryPolicyAvailabilityResponse
        {
            AsOf = ToUtcDate(asOf),
            TaxPolicyApproved = availability.TaxApproved,
            TaxPolicyVersion = availability.TaxVersion,
            TaxPolicyStatusLabel = availability.TaxApproved ? "Kebijakan tarif PPh aktif" : "Belum ada kebijakan tarif PPh yang berlaku",
            PendingTaxPolicyCount = pendingTax
        };
    }
}
