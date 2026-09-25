using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

/// <summary>
/// Mesin kalkulasi sewa kamar rawat inap bertingkat, penalti late checkout, dan pro-rata transfer menit riil.
/// Mengimplementasikan aturan bisnis BKC-DEC-112, BKC-DES-043, BKC-DES-050, BIL-VAL-118, BIL-VAL-119, dan BIL-API-1.4.
/// </summary>
public sealed class InpatientRoomChargeCalculationService : IInpatientRoomChargeCalculationService
{
    private static readonly TimeSpan DefaultHospitalOffset = TimeSpan.FromHours(7); // WIB (UTC+7)
    private static readonly TimeSpan AdmissionFullTierCutoff = TimeSpan.FromHours(18); // 18:00:00
    private static readonly TimeSpan AdmissionHalfTierCutoff = TimeSpan.FromHours(22); // 22:00:00
    private static readonly TimeSpan LateCheckoutThreshold = TimeSpan.FromHours(12); // 12:00:00 siang

    private readonly ApplicationDbContext _dbContext;

    public InpatientRoomChargeCalculationService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <summary>
    /// Mengonversi waktu DateTimeOffset ke waktu lokal rumah sakit (WIB / UTC+7).
    /// </summary>
    public static DateTimeOffset ToHospitalLocalTime(DateTimeOffset dateTimeOffset)
    {
        try
        {
            var wibZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            return TimeZoneInfo.ConvertTime(dateTimeOffset, wibZone);
        }
        catch
        {
            return dateTimeOffset.ToOffset(DefaultHospitalOffset);
        }
    }

    /// <summary>
    /// Mengonversi waktu DateTime UTC ke waktu lokal rumah sakit (WIB / UTC+7).
    /// </summary>
    public static DateTimeOffset ToHospitalLocalTime(DateTime dateTimeUtc)
    {
        var offset = dateTimeUtc.Kind == DateTimeKind.Utc
            ? new DateTimeOffset(dateTimeUtc)
            : new DateTimeOffset(DateTime.SpecifyKind(dateTimeUtc, DateTimeKind.Utc));
        return ToHospitalLocalTime(offset);
    }

    /// <inheritdoc />
    public (decimal Multiplier, string Policy) CalculateAdmissionTierMultiplier(TimeSpan timeOfDay)
    {
        // BIL-VAL-118:
        // - Masuk < 18:00:00 -> 100% (1.00m) - TARIF_PENUH_SEBELUM_18
        // - Masuk 18:00:00 s/d < 22:00:00 -> 50% (0.50m) - POTONGAN_50_PERSEN_JAM_18_SD_22
        // - Masuk 22:00:00 s/d < 24:00:00 -> 20% (0.20m) - POTONGAN_80_PERSEN_JAM_22_SD_24
        // - Masuk >= 00:00:00 (hari baru) -> beban hari sebelumnya 0% - HARI_BARU_TIDAK_DITAGIH

        if (timeOfDay < TimeSpan.Zero)
            timeOfDay = TimeSpan.Zero;

        if (timeOfDay >= TimeSpan.FromDays(1))
            timeOfDay = TimeSpan.FromTicks(timeOfDay.Ticks % TimeSpan.FromDays(1).Ticks);

        if (timeOfDay < AdmissionFullTierCutoff)
        {
            return (1.00m, RoomChargeTierPolicies.TarifPenuhSebelum18);
        }

        if (timeOfDay < AdmissionHalfTierCutoff)
        {
            return (0.50m, RoomChargeTierPolicies.Potongan50PersenJam18Sd22);
        }

        return (0.20m, RoomChargeTierPolicies.Potongan80PersenJam22Sd24);
    }

    /// <inheritdoc />
    public (bool IsLate, decimal LateFee, string Policy) CalculateLateCheckoutFee(TimeSpan checkoutTime, decimal dailyTariff)
    {
        if (dailyTariff <= 0)
            return (false, 0.00m, string.Empty);

        if (checkoutTime > LateCheckoutThreshold)
        {
            var fee = Math.Round(dailyTariff * 0.50m, 2, MidpointRounding.AwayFromZero);
            return (true, fee, RoomChargeTierPolicies.PenaltiLateCheckout50Persen);
        }

        return (false, 0.00m, string.Empty);
    }

    /// <inheritdoc />
    public IReadOnlyList<RoomPlacementSegmentCalculation> CalculateDailyProRataTransfers(
        DateOnly date,
        IReadOnlyList<PlacementSegmentInput> segments,
        decimal dayMultiplier = 1.00m)
    {
        if (segments == null || segments.Count == 0)
            return Array.Empty<RoomPlacementSegmentCalculation>();

        if (segments.Count == 1)
        {
            var single = segments[0];
            var durationMinutes = (int)Math.Max(1, Math.Round(((single.EndTime ?? single.StartTime.AddHours(24)) - single.StartTime).TotalMinutes));
            var amount = Math.Round(single.DailyRate * dayMultiplier, 2, MidpointRounding.AwayFromZero);

            string policy;
            if (dayMultiplier == 0.50m)
                policy = RoomChargeTierPolicies.Potongan50PersenJam18Sd22;
            else if (dayMultiplier == 0.20m)
                policy = RoomChargeTierPolicies.Potongan80PersenJam22Sd24;
            else if (dayMultiplier == 0.00m)
                policy = RoomChargeTierPolicies.HariBaruTidakDitagih;
            else if (dayMultiplier < 1.00m)
                policy = RoomChargeTierPolicies.Potongan50PersenJam18Sd22;
            else
                policy = RoomChargeTierPolicies.TarifStandarHarian;

            return new[]
            {
                new RoomPlacementSegmentCalculation
                {
                    PlacementId = single.PlacementId,
                    RoomId = single.RoomId,
                    RoomName = single.RoomName,
                    BedId = single.BedId,
                    BedCode = single.BedCode,
                    PatientClassId = single.PatientClassId,
                    PatientClassName = single.PatientClassName,
                    StartTime = single.StartTime,
                    EndTime = single.EndTime,
                    DurationMinutes = durationMinutes,
                    TotalDayMinutes = durationMinutes,
                    DailyRate = single.DailyRate,
                    ProportionPercentage = 100.00m,
                    CalculatedAmount = amount,
                    AppliedPolicy = policy
                }
            };
        }

        // BIL-VAL-119 / BKC-DES-050: Transfer kamar multipel dalam 1 hari kalender
        // Hitung menit durasi riil setiap kamar
        var durations = new List<int>();
        foreach (var seg in segments)
        {
            var end = seg.EndTime ?? seg.StartTime.AddHours(1);
            var minutes = (int)Math.Max(1, Math.Round((end - seg.StartTime).TotalMinutes));
            durations.Add(minutes);
        }

        var totalMinutes = durations.Sum();
        if (totalMinutes <= 0) totalMinutes = 1440; // 24 jam kalender

        var results = new List<RoomPlacementSegmentCalculation>(segments.Count);
        for (var i = 0; i < segments.Count; i++)
        {
            var seg = segments[i];
            var minutes = durations[i];
            var proportion = (decimal)minutes / (decimal)totalMinutes;
            var proportionPercent = Math.Round(proportion * 100m, 2, MidpointRounding.AwayFromZero);

            // Alokasi proporsional: (MenitKamar / TotalMenitHariItu) * TarifKamar * dayMultiplier
            var segmentAmount = Math.Round(proportion * seg.DailyRate * dayMultiplier, 2, MidpointRounding.AwayFromZero);

            results.Add(new RoomPlacementSegmentCalculation
            {
                PlacementId = seg.PlacementId,
                RoomId = seg.RoomId,
                RoomName = seg.RoomName,
                BedId = seg.BedId,
                BedCode = seg.BedCode,
                PatientClassId = seg.PatientClassId,
                PatientClassName = seg.PatientClassName,
                StartTime = seg.StartTime,
                EndTime = seg.EndTime,
                DurationMinutes = minutes,
                TotalDayMinutes = totalMinutes,
                DailyRate = seg.DailyRate,
                ProportionPercentage = proportionPercent,
                CalculatedAmount = segmentAmount,
                AppliedPolicy = RoomChargeTierPolicies.ProRataTransferMenit
            });
        }

        return results;
    }

    /// <inheritdoc />
    public async Task<InpatientEpisodeRoomChargeSummary> CalculateEpisodeRoomChargesAsync(
        Guid episodeId,
        CancellationToken cancellationToken = default)
    {
        var episode = await _dbContext.Set<InpEpisode>()
            .AsNoTracking()
            .Where(x => x.Id == episodeId && !x.IsDelete)
            .Select(x => new
            {
                x.Id,
                x.EncounterId,
                AdmissionDateTime = x.AdmittedAt ?? x.CreateDateTime,
                DischargeDateTime = x.PhysicallyLeftAt ?? x.DischargeDecidedAt,
                x.PatientClassId
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (episode == null)
        {
            return new InpatientEpisodeRoomChargeSummary
            {
                EpisodeId = episodeId,
                CalculatedAt = DateTime.UtcNow
            };
        }

        var placements = await _dbContext.Set<InpBedPlacement>()
            .AsNoTracking()
            .Include(p => p.Room)
            .Include(p => p.Bed)
            .Include(p => p.PatientClass)
            .Where(p => p.EpisodeId == episodeId && !p.IsDelete && !p.IsSuperseded)
            .OrderBy(p => p.StartDateTime)
            .ThenBy(p => p.SequenceNumber)
            .ToListAsync(cancellationToken);

        var roomTariffs = await _dbContext.MstTariffs
            .AsNoTracking()
            .Where(t => t.IsRoomCharge && t.IsActive && !t.IsDelete)
            .ToListAsync(cancellationToken);

        var tariffByPatientClass = roomTariffs
            .Where(t => t.PatientClassId.HasValue)
            .GroupBy(t => t.PatientClassId!.Value)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.NormalPrice).First().NormalPrice);

        var defaultRoomTariff = roomTariffs.Count > 0 ? roomTariffs.Average(t => t.NormalPrice) : 500000m;

        decimal ResolveDailyRate(Guid patientClassId)
        {
            if (tariffByPatientClass.TryGetValue(patientClassId, out var price))
                return price;
            return defaultRoomTariff;
        }

        var admissionLocal = ToHospitalLocalTime(episode.AdmissionDateTime);
        DateTimeOffset? dischargeLocal = episode.DischargeDateTime.HasValue
            ? ToHospitalLocalTime(episode.DischargeDateTime.Value)
            : null;

        var summary = new InpatientEpisodeRoomChargeSummary
        {
            EpisodeId = episode.Id,
            EncounterId = episode.EncounterId,
            AdmissionTime = admissionLocal,
            DischargeTime = dischargeLocal,
            CalculatedAt = DateTime.UtcNow
        };

        if (placements.Count == 0)
        {
            return summary;
        }

        // Kelompokkan penempatan kamar berdasarkan tanggal kalender lokal
        var placementsByDate = new SortedDictionary<DateOnly, List<PlacementSegmentInput>>();

        foreach (var placement in placements)
        {
            var startLocal = ToHospitalLocalTime(placement.StartDateTime);
            DateTimeOffset? endLocal = placement.PhysicallyLeftAt.HasValue
                ? ToHospitalLocalTime(placement.PhysicallyLeftAt.Value)
                : placement.EndDateTime.HasValue
                    ? ToHospitalLocalTime(placement.EndDateTime.Value)
                    : null;

            var dailyRate = ResolveDailyRate(placement.PatientClassId);
            var roomName = placement.Room?.RoomName ?? $"Kamar {placement.RoomId}";
            var bedCode = placement.Bed?.BedCode ?? $"Bed {placement.BedId}";
            var className = placement.PatientClass?.PatientClassName ?? "Kelas Perawatan";

            var curDate = DateOnly.FromDateTime(startLocal.DateTime);
            var finalDate = endLocal.HasValue
                ? DateOnly.FromDateTime(endLocal.Value.DateTime)
                : DateOnly.FromDateTime(DateTime.UtcNow);

            // Jika satu penempatan melintasi beberapa hari kalender, pecah per hari
            for (var d = curDate; d <= finalDate; d = d.AddDays(1))
            {
                if (!placementsByDate.TryGetValue(d, out var dayList))
                {
                    dayList = new List<PlacementSegmentInput>();
                    placementsByDate[d] = dayList;
                }

                DateTimeOffset segStart;
                DateTimeOffset? segEnd;

                if (d == curDate)
                {
                    segStart = startLocal;
                }
                else
                {
                    segStart = new DateTimeOffset(d.ToDateTime(TimeOnly.MinValue), startLocal.Offset);
                }

                if (d == finalDate && endLocal.HasValue)
                {
                    segEnd = endLocal.Value;
                }
                else
                {
                    segEnd = new DateTimeOffset(d.ToDateTime(TimeOnly.MaxValue), startLocal.Offset);
                }

                dayList.Add(new PlacementSegmentInput
                {
                    PlacementId = placement.Id,
                    RoomId = placement.RoomId,
                    RoomName = roomName,
                    BedId = placement.BedId,
                    BedCode = bedCode,
                    PatientClassId = placement.PatientClassId,
                    PatientClassName = className,
                    DailyRate = dailyRate,
                    StartTime = segStart,
                    EndTime = segEnd
                });
            }
        }

        var dates = placementsByDate.Keys.ToList();
        var firstDate = dates.First();
        var lastDate = dates.Last();
        var isDischarged = episode.DischargeDateTime.HasValue;

        foreach (var kvp in placementsByDate)
        {
            var date = kvp.Key;
            var segments = kvp.Value;
            var isFirst = date == firstDate;
            var isDischargeDay = isDischarged && date == lastDate;

            decimal multiplier = 1.00m;
            string primaryPolicy = RoomChargeTierPolicies.TarifStandarHarian;

            if (isFirst)
            {
                var admissionTimeOfDay = admissionLocal.TimeOfDay;
                var tier = CalculateAdmissionTierMultiplier(admissionTimeOfDay);
                multiplier = tier.Multiplier;
                primaryPolicy = tier.Policy;
            }

            var segmentCalcs = CalculateDailyProRataTransfers(date, segments, multiplier);
            var baseAmount = segmentCalcs.Sum(x => x.CalculatedAmount);

            var isLateApplied = false;
            var lateFee = 0.00m;

            if (isDischargeDay && dischargeLocal.HasValue)
            {
                var lastSegmentRate = segments.Last().DailyRate;
                var lateResult = CalculateLateCheckoutFee(dischargeLocal.Value.TimeOfDay, lastSegmentRate);
                if (lateResult.IsLate)
                {
                    isLateApplied = true;
                    lateFee = lateResult.LateFee;
                }
            }

            var dayCharge = new DailyRoomChargeCalculationResult
            {
                Date = date,
                IsFirstDay = isFirst,
                IsDischargeDay = isDischargeDay,
                PrimaryPolicy = primaryPolicy,
                DayMultiplier = multiplier,
                BaseRoomChargeAmount = baseAmount,
                IsLateCheckoutApplied = isLateApplied,
                LateCheckoutFee = lateFee,
                TotalDayChargeAmount = baseAmount + lateFee,
                Segments = segmentCalcs.ToList()
            };

            summary.DailyBreakdowns.Add(dayCharge);
        }

        summary.TotalCareDays = summary.DailyBreakdowns.Count;
        summary.TotalBaseRoomCharge = summary.DailyBreakdowns.Sum(x => x.BaseRoomChargeAmount);
        summary.TotalLateCheckoutFee = summary.DailyBreakdowns.Sum(x => x.LateCheckoutFee);
        summary.GrandTotalRoomCharge = summary.TotalBaseRoomCharge + summary.TotalLateCheckoutFee;

        return summary;
    }

    /// <inheritdoc />
    public async Task<InpatientEpisodeRoomChargeSummary> CalculateEncounterRoomChargesAsync(
        Guid encounterId,
        CancellationToken cancellationToken = default)
    {
        var episodeId = await _dbContext.Set<InpEpisode>()
            .AsNoTracking()
            .Where(x => x.EncounterId == encounterId && !x.IsDelete)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (!episodeId.HasValue)
        {
            return new InpatientEpisodeRoomChargeSummary
            {
                EncounterId = encounterId,
                CalculatedAt = DateTime.UtcNow
            };
        }

        return await CalculateEpisodeRoomChargesAsync(episodeId.Value, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<OccupancyChargeResponse> ProcessOccupancyChargeAsync(
        OccupancyChargeRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Ambil invoice aktif untuk encounter ini jika sudah ada
        var invoice = await _dbContext.BilInvoices
            .AsNoTracking()
            .Where(x => x.EncounterId == request.EncounterId && !x.IsDelete)
            .Select(x => new { x.Id, x.InvoiceNumber })
            .FirstOrDefaultAsync(cancellationToken);

        // Cari tarif harian kamar untuk kelas pasien
        Guid.TryParse(request.PatientClassId, out var classId);
        var tariff = await _dbContext.MstTariffs
            .AsNoTracking()
            .Where(t => t.IsRoomCharge && t.IsActive && !t.IsDelete &&
                        (classId == Guid.Empty || t.PatientClassId == classId))
            .OrderByDescending(t => t.NormalPrice)
            .FirstOrDefaultAsync(cancellationToken);

        var dailyRate = tariff?.NormalPrice ?? 1500000.00m; // Nilai default bila belum ada data

        // Konversi waktu mulai hunian ke waktu lokal rumah sakit
        var localStart = ToHospitalLocalTime(request.OccupancyStartAt);
        var (multiplier, policy) = CalculateAdmissionTierMultiplier(localStart.TimeOfDay);

        var roomChargeAmount = Math.Round(dailyRate * multiplier, 2, MidpointRounding.AwayFromZero);

        return new OccupancyChargeResponse
        {
            InvoiceId = invoice?.Id ?? Guid.Empty,
            InvoiceNumber = invoice?.InvoiceNumber ?? string.Empty,
            CurrentChargeAmount = dailyRate,
            AppliedPolicy = policy,
            RoomChargeAmount = roomChargeAmount,
            VersionNo = request.Version
        };
    }
}
