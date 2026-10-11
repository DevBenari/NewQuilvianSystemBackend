using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;

/// <summary>
/// Kontrak generator nomor piutang resmi Finance V2 (AR-YYYYMMDD0001).
/// </summary>
public interface IFinanceReceivableNumberGenerator
{
    /// <summary>
    /// Menerbitkan nomor piutang baru format AR-YYYYMMDD0001 (harian, 4 digit dengan pertumbuhan 5 digit tanpa batas).
    /// Bersifat thread-safe, atomic (terpisah dari transaksi pemanggil), dan aman terhadap concurrent requests.
    /// </summary>
    Task<string> GenerateAsync(Guid actorUserId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Generator nomor piutang resmi Finance V2 yang memanfaatkan NumberSeriesAllocator platform.
/// Memenuhi seluruh kriteria Bagian D (nomor unik, thread-safe, multi-instance lock via pg_advisory_xact_lock,
/// reset harian WIB, tidak ada duplicate number, tidak memakai COUNT/MAX+1 yang rawan race condition).
/// </summary>
public sealed class FinanceReceivableNumberGenerator : IFinanceReceivableNumberGenerator
{
    private const string SequenceKey = "FIN_RECEIVABLE_AR";
    private const string Prefix = "AR";
    private const int SequenceDigits = 4;
    private static readonly Guid SystemFallbackUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    private readonly NumberSeriesAllocator _allocator;

    public FinanceReceivableNumberGenerator(NumberSeriesAllocator allocator)
    {
        _allocator = allocator;
    }

    public async Task<string> GenerateAsync(Guid actorUserId, CancellationToken cancellationToken = default)
    {
        var effectiveUserId = actorUserId == Guid.Empty ? SystemFallbackUserId : actorUserId;
        var request = new NumberAllocationRequest(
            SequenceKey: SequenceKey,
            Prefix: Prefix,
            ResetPolicy: NumberSeriesResetPolicies.Daily,
            SequenceDigits: SequenceDigits,
            ActorUserId: effectiveUserId,
            Instant: DateTimeOffset.UtcNow,
            MinimumValue: 0,
            CompactScopeFormat: true,
            AllowSequenceOverflow: true);

        return await _allocator.AllocateAsync(request, cancellationToken);
    }
}
