using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Services
{
    /// <summary>
    /// Pemilik format dan alokasi kode kebijakan tunjangan (Shift <c>SAP-RSMMC-00001</c>,
    /// On Call <c>OAP-RSMMC-00001</c>, dan Risiko/Hazard <c>HAP-RSMMC-00001</c>). Controller tidak lagi
    /// membentuk kode sendiri (<c>QBE-CODE-002</c>); nomor diterbitkan provider number-series bersama
    /// yang atomik dan durabel (<c>QBE-CODE-006</c>).
    /// </summary>
    /// <remarks>
    /// Format kode tetap sama dengan alokator legacy (awalan 9 karakter + 5 digit). Kode tertinggi
    /// yang sudah terbit dipakai sebagai batas bawah (<c>MinimumValue</c>) sehingga deret baru
    /// melanjutkan dari nomor tertinggi yang ada tanpa bentrok dengan data lama.
    /// </remarks>
    public class AllowancePolicyCodeService
    {
        private const string ShiftSequenceKey = "MST_SHIFT_ALLOWANCE_POLICY";
        private const string ShiftNumberPrefix = "SAP-RSMMC";
        private const string ShiftLegacyCodePrefix = ShiftNumberPrefix + "-";

        private const string OnCallSequenceKey = "MST_ON_CALL_ALLOWANCE_POLICY";
        private const string OnCallNumberPrefix = "OAP-RSMMC";
        private const string OnCallLegacyCodePrefix = OnCallNumberPrefix + "-";

        private const string HazardSequenceKey = "MST_HAZARD_ALLOWANCE_POLICY";
        private const string HazardNumberPrefix = "HAP-RSMMC";
        private const string HazardLegacyCodePrefix = HazardNumberPrefix + "-";

        private const int SequenceDigits = 5;

        private readonly ApplicationDbContext _dbContext;
        private readonly NumberSeriesAllocator _numberSeriesAllocator;

        public AllowancePolicyCodeService(
            ApplicationDbContext dbContext,
            NumberSeriesAllocator numberSeriesAllocator)
        {
            _dbContext = dbContext;
            _numberSeriesAllocator = numberSeriesAllocator;
        }

        /// <exception cref="NumberSeriesAllocationException">Kode kebijakan tunjangan shift gagal diterbitkan.</exception>
        public async Task<string> AllocateShiftPolicyCodeAsync(
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var existingCodes = await _dbContext.Set<MstShiftAllowancePolicy>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.ShiftAllowancePolicyCode.StartsWith(ShiftLegacyCodePrefix))
                .Select(x => x.ShiftAllowancePolicyCode)
                .ToListAsync(cancellationToken);

            var legacyHighWatermark = ResolveLegacyHighWatermark(existingCodes, ShiftLegacyCodePrefix);

            return await _numberSeriesAllocator.AllocateAsync(
                new NumberAllocationRequest(
                    SequenceKey: ShiftSequenceKey,
                    Prefix: ShiftNumberPrefix,
                    ResetPolicy: NumberSeriesResetPolicies.Never,
                    SequenceDigits: SequenceDigits,
                    ActorUserId: actorUserId,
                    Instant: DateTimeOffset.UtcNow,
                    MinimumValue: legacyHighWatermark),
                cancellationToken);
        }

        /// <exception cref="NumberSeriesAllocationException">Kode kebijakan tunjangan on call gagal diterbitkan.</exception>
        public async Task<string> AllocateOnCallPolicyCodeAsync(
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var existingCodes = await _dbContext.Set<MstOnCallAllowancePolicy>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.OnCallAllowancePolicyCode.StartsWith(OnCallLegacyCodePrefix))
                .Select(x => x.OnCallAllowancePolicyCode)
                .ToListAsync(cancellationToken);

            var legacyHighWatermark = ResolveLegacyHighWatermark(existingCodes, OnCallLegacyCodePrefix);

            return await _numberSeriesAllocator.AllocateAsync(
                new NumberAllocationRequest(
                    SequenceKey: OnCallSequenceKey,
                    Prefix: OnCallNumberPrefix,
                    ResetPolicy: NumberSeriesResetPolicies.Never,
                    SequenceDigits: SequenceDigits,
                    ActorUserId: actorUserId,
                    Instant: DateTimeOffset.UtcNow,
                    MinimumValue: legacyHighWatermark),
                cancellationToken);
        }

        /// <exception cref="NumberSeriesAllocationException">Kode kebijakan tunjangan risiko gagal diterbitkan.</exception>
        public async Task<string> AllocateHazardPolicyCodeAsync(
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var existingCodes = await _dbContext.Set<MstHazardAllowancePolicy>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.HazardAllowancePolicyCode.StartsWith(HazardLegacyCodePrefix))
                .Select(x => x.HazardAllowancePolicyCode)
                .ToListAsync(cancellationToken);

            var legacyHighWatermark = ResolveLegacyHighWatermark(existingCodes, HazardLegacyCodePrefix);

            return await _numberSeriesAllocator.AllocateAsync(
                new NumberAllocationRequest(
                    SequenceKey: HazardSequenceKey,
                    Prefix: HazardNumberPrefix,
                    ResetPolicy: NumberSeriesResetPolicies.Never,
                    SequenceDigits: SequenceDigits,
                    ActorUserId: actorUserId,
                    Instant: DateTimeOffset.UtcNow,
                    MinimumValue: legacyHighWatermark),
                cancellationToken);
        }

        private static long ResolveLegacyHighWatermark(IEnumerable<string> existingCodes, string legacyPrefix)
        {
            return existingCodes
                .Select(code => ExtractSequenceNumber(code, legacyPrefix))
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .DefaultIfEmpty(0)
                .Max();
        }

        private static long? ExtractSequenceNumber(string code, string legacyPrefix)
        {
            if (string.IsNullOrWhiteSpace(code)
                || !code.StartsWith(legacyPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return long.TryParse(code[legacyPrefix.Length..], out var number)
                ? number
                : null;
        }
    }
}
