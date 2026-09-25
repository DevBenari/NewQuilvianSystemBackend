using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.DTOs;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

/// <summary>
/// Kontrak mesin kalkulasi sewa kamar rawat inap bertingkat, penalti late checkout, dan pro-rata transfer menit riil.
/// (BKC-DEC-112, BKC-DES-043, BKC-DES-050, BIL-VAL-118, BIL-VAL-119, BIL-INTEGRATION-1.2, BIL-API-1.4).
/// </summary>
public interface IInpatientRoomChargeCalculationService
{
    /// <summary>
    /// Menghitung faktor pengali tarif kamar hari pertama berdasarkan jam masuk lokal (BIL-VAL-118).
    /// <para>
    /// - &lt; 18:00:00: 100% (1.00m) - TARIF_PENUH_SEBELUM_18<br/>
    /// - 18:00:00 s/d &lt; 22:00:00: 50% (0.50m) - POTONGAN_50_PERSEN_JAM_18_SD_22<br/>
    /// - 22:00:00 s/d &lt; 24:00:00: 20% (0.20m) - POTONGAN_80_PERSEN_JAM_22_SD_24<br/>
    /// - &gt;= 00:00:00 (hari baru): 0% (0.00m) - HARI_BARU_TIDAK_DITAGIH
    /// </para>
    /// </summary>
    (decimal Multiplier, string Policy) CalculateAdmissionTierMultiplier(TimeSpan timeOfDay);

    /// <summary>
    /// Menghitung penalti keterlambatan checkout (late checkout) jika pasien keluar setelah pukul 12:00 siang waktu lokal.
    /// Dikenakan denda 50% dari tarif harian kamar hari kepulangan (BKC-DEC-112, PENALTI_LATE_CHECKOUT_50_PERSEN).
    /// </summary>
    (bool IsLate, decimal LateFee, string Policy) CalculateLateCheckoutFee(TimeSpan checkoutTime, decimal dailyTariff);

    /// <summary>
    /// Menghitung alokasi tarif kamar pro-rata menit riil untuk pasien yang berpindah kamar multipel pada hari kalender yang sama (BIL-VAL-119, BKC-DES-050).
    /// Rumus: (MenitKamar / TotalMenitHariItu) * TarifKamar, dibulatkan 2 desimal dengan MidpointRounding.AwayFromZero.
    /// </summary>
    IReadOnlyList<RoomPlacementSegmentCalculation> CalculateDailyProRataTransfers(
        DateOnly date,
        IReadOnlyList<PlacementSegmentInput> segments,
        decimal dayMultiplier = 1.00m);

    /// <summary>
    /// Menghitung rincian sewa kamar rawat inap lengkap hari demi hari untuk satu episode perawatan.
    /// </summary>
    Task<InpatientEpisodeRoomChargeSummary> CalculateEpisodeRoomChargesAsync(
        Guid episodeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Menghitung rincian sewa kamar rawat inap lengkap untuk kunjungan rawat inap berdasarkan EncounterId.
    /// </summary>
    Task<InpatientEpisodeRoomChargeSummary> CalculateEncounterRoomChargesAsync(
        Guid encounterId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Memproses event beban sewa kamar harian dari outbox Rawat Inap (BIL-API-1.4, POST /invoices/occupancy-charges).
    /// </summary>
    Task<OccupancyChargeResponse> ProcessOccupancyChargeAsync(
        OccupancyChargeRequest request,
        CancellationToken cancellationToken = default);
}
