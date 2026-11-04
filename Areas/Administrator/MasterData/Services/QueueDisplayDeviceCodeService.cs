using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Administrator.MasterData.Services
{
    /// <summary>
    /// Pemilik format dan alokasi kode display antrian (<c>QDD-RSMMC-00001</c>). Controller tidak
    /// lagi membentuk kode sendiri (<c>QBE-CODE-002</c>); nomor diterbitkan provider number-series
    /// bersama yang atomik dan durabel (<c>QBE-CODE-006</c>).
    /// </summary>
    /// <remarks>
    /// Format tetap sama dengan alokator lama di controller: awalan <c>QDD-RSMMC</c> dan 5 digit.
    /// Kode yang sudah terbit dipakai sekali sebagai batas bawah (<c>MinimumValue</c>) supaya deret
    /// baru melanjutkan dari nomor tertinggi yang ada, bukan mulai lagi dari 00001.
    /// </remarks>
    public class QueueDisplayDeviceCodeService
    {
        private const string SequenceKey = "MST_QUEUE_DISPLAY_DEVICE";
        private const string NumberPrefix = "QDD-RSMMC";
        private const string LegacyCodePrefix = NumberPrefix + "-";
        private const int SequenceDigits = 5;

        private readonly ApplicationDbContext _dbContext;
        private readonly NumberSeriesAllocator _numberSeriesAllocator;

        public QueueDisplayDeviceCodeService(
            ApplicationDbContext dbContext,
            NumberSeriesAllocator numberSeriesAllocator)
        {
            _dbContext = dbContext;
            _numberSeriesAllocator = numberSeriesAllocator;
        }

        /// <exception cref="NumberSeriesAllocationException">Nomor gagal diterbitkan.</exception>
        public async Task<string> AllocateDisplayCodeAsync(
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var legacyHighWatermark = await GetLegacyHighWatermarkAsync(cancellationToken);

            return await _numberSeriesAllocator.AllocateAsync(
                new NumberAllocationRequest(
                    SequenceKey: SequenceKey,
                    Prefix: NumberPrefix,
                    ResetPolicy: NumberSeriesResetPolicies.Never,
                    SequenceDigits: SequenceDigits,
                    ActorUserId: actorUserId,
                    Instant: DateTimeOffset.UtcNow,
                    MinimumValue: legacyHighWatermark),
                cancellationToken);
        }

        private async Task<long> GetLegacyHighWatermarkAsync(CancellationToken cancellationToken)
        {
            var existingCodes = await _dbContext.Set<MstQueueDisplayDevice>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.DisplayCode.StartsWith(LegacyCodePrefix))
                .Select(x => x.DisplayCode)
                .ToListAsync(cancellationToken);

            return existingCodes
                .Select(ExtractSequenceNumber)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .DefaultIfEmpty(0)
                .Max();
        }

        private static long? ExtractSequenceNumber(string displayCode)
        {
            if (string.IsNullOrWhiteSpace(displayCode)
                || !displayCode.StartsWith(LegacyCodePrefix, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return long.TryParse(displayCode[LegacyCodePrefix.Length..], out var number)
                ? number
                : null;
        }
    }
}
