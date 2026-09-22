using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services
{
    /// <summary>
    /// Tanda vital rawat inap per episode — <c>BE-RWI-110</c> kriteria 5 dan <c>BE-RWI-121</c>,
    /// <c>FR-KEP-049</c>, <c>FR-KEP-056</c>, <c>INV-KEP-04</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Episode diisi server, tidak dipercaya dari klien.</b> Tanda vital untuk kunjungan yang sedang
    /// menaungi perawatan rawat inap berjalan diberi <c>InpEpisodeId</c> episode itu dan sumber
    /// <c>InpatientObservation</c>.
    /// </para>
    /// <para>
    /// <b>Nyeri bukan tempatnya di sini.</b> Pada kunjungan rawat inap, isian nyeri pada tanda vital
    /// ditolak <c>400</c> "Nyeri dicatat pada Monitoring Nyeri, bukan pada tanda vital." — satu fakta klinis,
    /// satu tempat. Poliklinik dan IGD tidak berubah.
    /// </para>
    /// <para>
    /// <b>Deret per episode.</b> Contoh: Budi dirawat sejak Senin; perawat membuka grafik Rabu 10.00 tanpa
    /// rentang → 24 jam terakhir (Selasa 10.00–Rabu 10.00), terurut waktu observasi. Rentang lebih dari
    /// 7 hari ditolak <c>400</c> supaya satu permintaan tidak menarik seluruh riwayat perawatan panjang.
    /// </para>
    /// </remarks>
    public class InpatientVitalSignService
    {
        public const string PenolakanNyeriPadaTandaVital = "Nyeri dicatat pada Monitoring Nyeri, bukan pada tanda vital.";

        private static readonly TimeSpan RentangBawaan = TimeSpan.FromHours(24);
        private static readonly TimeSpan RentangMaksimum = TimeSpan.FromDays(7);

        private readonly ApplicationDbContext _dbContext;
        private readonly InpatientClinicalContextService _contextService;

        public InpatientVitalSignService(ApplicationDbContext dbContext, InpatientClinicalContextService contextService)
        {
            _dbContext = dbContext;
            _contextService = contextService;
        }

        /// <summary>Episode rawat inap berjalan milik kunjungan, atau <c>null</c> bila bukan rawat inap.</summary>
        public Task<Guid?> FindInpatientEpisodeIdAsync(Guid? encounterId, CancellationToken cancellationToken = default) =>
            encounterId.HasValue && encounterId.Value != Guid.Empty
                ? _contextService.FindOpenEpisodeIdAsync(encounterId.Value, cancellationToken)
                : Task.FromResult<Guid?>(null);

        /// <summary><c>VAL-KEP-22c</c>: <c>true</c> bila permintaan membawa isian nyeri apa pun.</summary>
        public static bool HasPainFields(bool hasPain, int? painScale, string? painLocation, string? painNote) =>
            hasPain || painScale.HasValue || !string.IsNullOrWhiteSpace(painLocation) || !string.IsNullOrWhiteSpace(painNote);

        /// <summary>Deret tanda vital satu episode untuk tabel dan grafik.</summary>
        public async Task<NursingResult<List<PatientVitalSignSeriesItem>>> GetSeriesAsync(
            Guid episodeId,
            DateTime? from,
            DateTime? to,
            CancellationToken cancellationToken = default)
        {
            var adaEpisode = await _dbContext.Set<InpEpisode>().AsNoTracking()
                .AnyAsync(x => x.Id == episodeId && !x.IsDelete, cancellationToken);

            if (!adaEpisode)
                return NursingResult<List<PatientVitalSignSeriesItem>>.Fail(StatusCodes.Status404NotFound, "Perawatan rawat inap tidak ditemukan.");

            var sampai = to ?? DateTime.UtcNow;
            var dari = from ?? sampai - RentangBawaan;

            if (dari > sampai)
                return NursingResult<List<PatientVitalSignSeriesItem>>.Fail(StatusCodes.Status400BadRequest, "Waktu awal tidak boleh setelah waktu akhir.");

            if (sampai - dari > RentangMaksimum)
                return NursingResult<List<PatientVitalSignSeriesItem>>.Fail(StatusCodes.Status400BadRequest, "Rentang deret tanda vital paling panjang 7 hari.");

            var baris = await _dbContext.Set<TrxPatientVitalSign>().AsNoTracking()
                .Where(x => x.InpEpisodeId == episodeId && !x.IsDelete &&
                            x.ObservationDateTime >= dari && x.ObservationDateTime <= sampai)
                .OrderBy(x => x.ObservationDateTime)
                .Select(x => new PatientVitalSignSeriesItem
                {
                    Id = x.Id,
                    ObservationDateTime = x.ObservationDateTime,
                    VitalSignStatus = x.VitalSignStatus,
                    BloodPressureSystolic = x.BloodPressureSystolic,
                    BloodPressureDiastolic = x.BloodPressureDiastolic,
                    MeanArterialPressure = x.MeanArterialPressure,
                    PulseRate = x.PulseRate,
                    RespiratoryRate = x.RespiratoryRate,
                    Temperature = x.Temperature,
                    OxygenSaturation = x.OxygenSaturation,
                    IsUsingOxygen = x.IsUsingOxygen,
                    ConsciousnessStatus = x.ConsciousnessStatus,
                    GcsTotal = x.GcsTotal,
                    EarlyWarningScore = x.EarlyWarningScore,
                    EwsRiskLevel = x.EwsRiskLevel,
                    IsAbnormal = x.IsAbnormal,
                    IsCritical = x.IsCritical,
                    IsExcludedFromChart = x.VitalSignStatus == PatientVitalSignStatus.Cancelled ||
                                          x.VitalSignStatus == PatientVitalSignStatus.EnteredInError,
                    ObservedByUserId = x.ObservedByUserId,
                    ObservedByName = x.ObservedByUser != null ? x.ObservedByUser.DisplayName : null,
                    UpdateDateTime = x.UpdateDateTime
                })
                .ToListAsync(cancellationToken);

            return NursingResult<List<PatientVitalSignSeriesItem>>.Ok(baris, "Deret tanda vital berhasil diambil.");
        }
    }
}
