using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Workers;

/// <summary>
/// Melepas penahanan stok rawat jalan yang melewati batas waktu sebelum pembayaran
/// (<c>PHA-DEC-066</c> butir 6).
/// </summary>
/// <remarks>
/// <para>
/// Pola mengikuti <c>BilInvoiceSyncWorker</c>: satu scope per siklus, dan siklus yang gagal
/// dicatat lalu dilanjutkan, tidak menghentikan pekerja.
/// </para>
/// <para>
/// <b>Jeda satu menit, bukan sepuluh detik.</b> Yang dikerjakan di sini bukan pengiriman yang
/// menunggu penerima, melainkan pelepasan yang batasnya dihitung dalam jam. Menyapu lebih
/// sering hanya menambah beban basis data tanpa memperbaiki apa pun yang dirasakan pengguna.
/// </para>
/// <para>
/// <b>Pekerja ini bukan satu-satunya penjaga.</b>
/// <c>PrescriptionStockReservationService.EnsureActiveAsync</c> juga menolak penahanan yang
/// batas waktunya sudah lewat walaupun penyapu belum berjalan. Tanpa itu, pembacaan tepat
/// sebelum sapuan akan memberi jawaban berbeda dari pembacaan sesudahnya untuk keadaan yang
/// sama — dan Farmasi dapat mulai menyiapkan resep yang stoknya sudah tidak lagi dijamin.
/// </para>
/// <para>
/// Penahanan rawat inap dan IGD tidak pernah tersentuh, karena <c>ExpiresAt</c>-nya
/// <c>null</c> (butir 8). Begitu pula penahanan yang pembayarannya sudah valid, yang batas
/// waktunya dihapus saat surat clearance diterapkan (butir 7).
/// </para>
/// </remarks>
public sealed class PrescriptionReservationExpiryWorker : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromMinutes(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PrescriptionReservationExpiryWorker> _logger;

    public PrescriptionReservationExpiryWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<PrescriptionReservationExpiryWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "PrescriptionReservationExpiryWorker aktif. Poll={Poll}s, TTL rawat jalan={Ttl}j.",
            PollInterval.TotalSeconds,
            PrescriptionStockReservationService.OutpatientTimeToLive.TotalHours);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var service = scope.ServiceProvider
                    .GetRequiredService<PrescriptionStockReservationService>();

                var dilepas = await service.ReleaseExpiredAsync(DateTime.UtcNow, stoppingToken);

                if (dilepas > 0)
                {
                    await dbContext.SaveChangesAsync(stoppingToken);

                    _logger.LogInformation(
                        "Melepas {Jumlah} baris penahanan stok resep yang kedaluwarsa sebelum pembayaran.",
                        dilepas);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                // Satu siklus yang gagal tidak boleh mematikan pelepasan untuk selamanya:
                // penahanan yang tidak pernah lepas mengunci stok yang fisiknya ada di rak.
                _logger.LogError(exception,
                    "Siklus pelepasan penahanan stok resep gagal. Dicoba lagi siklus berikutnya.");
            }

            try
            {
                await Task.Delay(PollInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}
