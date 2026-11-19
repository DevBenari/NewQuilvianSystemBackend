using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Options;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;

/// <summary>
/// Pembaca kebijakan regulasi penggajian yang berlaku (murni baca): kebijakan PPh dan iuran BPJS berstatus APPROVED yang masa
/// berlakunya mencakup akhir periode. Kebijakan DRAFT, PENDING_APPROVAL, REJECTED, dan RETIRED tidak pernah dipakai mesin payroll,
/// termasuk nilai bawaan Pengaturan Penggajian yang belum diverifikasi.
/// </summary>
public sealed class PayrollStatutoryPolicyResolver
{
    private readonly ApplicationDbContext _dbContext;
    private readonly PayrollPolicyOptions _policyOptions;

    public PayrollStatutoryPolicyResolver(ApplicationDbContext dbContext, IOptions<PayrollPolicyOptions> policyOptions)
    {
        _dbContext = dbContext;
        _policyOptions = policyOptions.Value;
    }

    public bool IsBpjsEnabled => _policyOptions.EnableBpjsInPayroll;

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

    public async Task<PayrollInsurancePolicy?> GetBpjsPolicyAsync(DateTime asOf, CancellationToken cancellationToken)
    {
        var date = ToUtcDate(asOf);
        var entity = await _dbContext.PrlBpjsPolicies.AsNoTracking()
            .Where(x => !x.IsDelete && x.IsActive && x.PolicyStatus == StatutoryPolicyStatuses.Approved
                && x.EffectiveStartDate <= date && (x.EffectiveEndDate == null || x.EffectiveEndDate >= date))
            .OrderByDescending(x => x.EffectiveStartDate)
            .ThenByDescending(x => x.DecidedAt)
            .FirstOrDefaultAsync(cancellationToken);
        return entity is null ? null : PayrollInsurancePolicy.FromEntity(entity);
    }

    public async Task<PayrollPolicyAvailability> GetAvailabilityAsync(DateTime asOf, CancellationToken cancellationToken)
    {
        var tax = await GetTaxPolicyAsync(asOf, cancellationToken);
        var bpjs = _policyOptions.EnableBpjsInPayroll
            ? await GetBpjsPolicyAsync(asOf, cancellationToken)
            : null;
        return new PayrollPolicyAvailability(
            tax is not null,
            tax?.Version,
            bpjs is not null,
            bpjs?.Version,
            _policyOptions.EnableBpjsInPayroll);
    }

    public async Task<PayrollStatutoryPolicyAvailabilityResponse> GetAvailabilityResponseAsync(DateTime asOf, CancellationToken cancellationToken)
    {
        var availability = await GetAvailabilityAsync(asOf, cancellationToken);
        var pendingTax = await _dbContext.PrlTaxPolicies.AsNoTracking()
            .CountAsync(x => !x.IsDelete && (x.PolicyStatus == StatutoryPolicyStatuses.Draft || x.PolicyStatus == StatutoryPolicyStatuses.PendingApproval), cancellationToken);
        var pendingBpjs = _policyOptions.EnableBpjsInPayroll
            ? await _dbContext.PrlBpjsPolicies.AsNoTracking()
                .CountAsync(x => !x.IsDelete && (x.PolicyStatus == StatutoryPolicyStatuses.Draft || x.PolicyStatus == StatutoryPolicyStatuses.PendingApproval), cancellationToken)
            : 0;

        return new PayrollStatutoryPolicyAvailabilityResponse
        {
            AsOf = ToUtcDate(asOf),
            TaxPolicyApproved = availability.TaxApproved,
            TaxPolicyVersion = availability.TaxVersion,
            TaxPolicyStatusLabel = availability.TaxApproved ? "Kebijakan tarif PPh aktif" : "Belum ada kebijakan tarif PPh yang berlaku",
            EnableBpjsInPayroll = _policyOptions.EnableBpjsInPayroll,
            BpjsPolicyApproved = availability.BpjsApproved,
            BpjsPolicyVersion = availability.BpjsVersion,
            BpjsPolicyStatusLabel = !_policyOptions.EnableBpjsInPayroll
                ? StatutoryPolicyLabels.BpjsTestingModeDisabled
                : (availability.BpjsApproved ? "Kebijakan iuran BPJS disahkan" : StatutoryPolicyLabels.AwaitingVerification),
            BpjsTestingModeNotice = _policyOptions.EnableBpjsInPayroll
                ? null
                : "Mode pengujian aktif: kalkulasi BPJS Kesehatan dan BPJS Ketenagakerjaan dinonaktifkan sementara.",
            PendingTaxPolicyCount = pendingTax,
            PendingBpjsPolicyCount = pendingBpjs
        };
    }
}
