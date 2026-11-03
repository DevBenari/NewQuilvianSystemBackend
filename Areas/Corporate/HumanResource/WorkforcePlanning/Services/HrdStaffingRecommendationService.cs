using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Services
{
    /// <summary>
    /// Layanan kalkulasi rekomendasi kebutuhan tenaga kerja per baris standar (BE-HRD-035).
    /// Mengimplementasikan rumus Backend Architecture Bagian 16.9 dan kriteria 4:
    /// - FixedMinimum: MinimumHeadcount
    /// - WorkloadRatio: (WorkloadValue ÷ RatioWorkloadQuantity × RatioStaffQuantity) per shift × ShiftsPerDay × ReliefFactor
    /// - ServiceCoverage: ShiftsPerDay × StaffPerShift × ReliefFactor
    /// </summary>
    public sealed class HrdStaffingRecommendationService : IHrdStaffingRecommendationService
    {
        private static readonly CultureInfo IdCulture = new("id-ID");
        private readonly ApplicationDbContext _dbContext;

        public HrdStaffingRecommendationService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<StaffingRecommendationResponse> CalculateRecommendationAsync(
            HrdStaffingStandard standard,
            DateOnly periodStartDate,
            CancellationToken cancellationToken = default)
        {
            if (standard == null)
            {
                throw new ArgumentNullException(nameof(standard));
            }

            // Ambil seluruh angka beban kerja yang berlaku untuk unit ini pada tanggal periode tersebut
            var workloads = await _dbContext.HrdStaffingWorkloadInputs.AsNoTracking()
                .Where(w => w.OrganizationUnitId == standard.OrganizationUnitId &&
                            !w.IsDelete &&
                            w.PeriodStartDate <= periodStartDate &&
                            w.PeriodEndDate >= periodStartDate)
                .ToListAsync(cancellationToken);

            var workloadMap = workloads
                .GroupBy(w => w.PositionId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(w => w.PeriodStartDate).First());

            var response = new StaffingRecommendationResponse
            {
                StaffingStandardId = standard.Id,
                StandardNumber = standard.StandardNumber,
                VersionNumber = standard.VersionNumber,
                OrganizationUnitId = standard.OrganizationUnitId,
                OrganizationUnitName = standard.OrganizationUnit?.UnitName,
                PeriodStartDate = periodStartDate,
                Lines = new List<StaffingLineRecommendationResponse>()
            };

            foreach (var line in standard.Lines.Where(l => !l.IsDelete))
            {
                var posCode = line.Position?.PositionCode;
                var posName = line.Position?.PositionName;

                decimal? recommendedQuantity = null;
                string? breakdown = null;
                string? missingInput = null;

                switch (line.CalculationMethod)
                {
                    case StaffingCalculationMethod.FixedMinimum:
                        var fixedMin = line.MinimumHeadcount ?? 0m;
                        recommendedQuantity = ApplyRounding(fixedMin, line.RoundingMethod);
                        breakdown = $"Jumlah tetap: {FormatNumber(fixedMin)} → {FormatNumber(recommendedQuantity.Value)}";
                        break;

                    case StaffingCalculationMethod.WorkloadRatio:
                        if (!workloadMap.TryGetValue(line.PositionId, out var workloadInput))
                        {
                            // Kriteria 4: Angka beban belum diisi → rekomendasi null dan missingInput "angka beban belum diisi", BUKAN 0
                            recommendedQuantity = null;
                            breakdown = null;
                            missingInput = "angka beban belum diisi";
                        }
                        else
                        {
                            var workload = workloadInput.WorkloadValue;
                            var ratioWorkload = (line.RatioWorkloadQuantity.HasValue && line.RatioWorkloadQuantity.Value > 0)
                                ? line.RatioWorkloadQuantity.Value
                                : 1m;
                            var ratioStaff = (line.RatioStaffQuantity.HasValue && line.RatioStaffQuantity.Value > 0)
                                ? line.RatioStaffQuantity.Value
                                : 1m;
                            var shifts = (line.ShiftsPerDay.HasValue && line.ShiftsPerDay.Value > 0)
                                ? line.ShiftsPerDay.Value
                                : 1;
                            var relief = (line.ReliefFactor.HasValue && line.ReliefFactor.Value >= 1m)
                                ? line.ReliefFactor.Value
                                : 1m;

                            var perShift = (workload / ratioWorkload) * ratioStaff;
                            var subtotal = perShift * shifts;
                            var beforeRounding = subtotal * relief;
                            recommendedQuantity = ApplyRounding(beforeRounding, line.RoundingMethod);

                            breakdown = $"{FormatNumber(workload)} ÷ {FormatNumber(ratioWorkload)} × {FormatNumber(ratioStaff)} = {FormatNumber(perShift)} per shift; {FormatNumber(perShift)} × {shifts} = {FormatNumber(subtotal)}; {FormatNumber(subtotal)} × {FormatNumber(relief)} = {FormatNumber(beforeRounding)} → {FormatNumber(recommendedQuantity.Value)}";
                        }
                        break;

                    case StaffingCalculationMethod.ServiceCoverage:
                        var coverageShifts = (line.ShiftsPerDay.HasValue && line.ShiftsPerDay.Value > 0)
                            ? line.ShiftsPerDay.Value
                            : 1;
                        var staffPerShift = line.StaffPerShift ?? 1m;
                        var coverageRelief = (line.ReliefFactor.HasValue && line.ReliefFactor.Value >= 1m)
                            ? line.ReliefFactor.Value
                            : 1m;

                        var coverageSubtotal = coverageShifts * staffPerShift;
                        var coverageBeforeRounding = coverageSubtotal * coverageRelief;
                        recommendedQuantity = ApplyRounding(coverageBeforeRounding, line.RoundingMethod);

                        breakdown = $"{coverageShifts} × {FormatNumber(staffPerShift)} = {FormatNumber(coverageSubtotal)}; {FormatNumber(coverageSubtotal)} × {FormatNumber(coverageRelief)} = {FormatNumber(coverageBeforeRounding)} → {FormatNumber(recommendedQuantity.Value)}";
                        break;

                    case StaffingCalculationMethod.WorkloadAnalysis:
                        missingInput = "Metode Analisis Beban Kerja belum didukung";
                        break;
                }

                response.Lines.Add(new StaffingLineRecommendationResponse
                {
                    PositionId = line.PositionId,
                    PositionCode = posCode,
                    PositionName = posName,
                    CalculationMethod = line.CalculationMethod,
                    RecommendedQuantity = recommendedQuantity,
                    Breakdown = breakdown,
                    MissingInput = missingInput
                });
            }

            return response;
        }

        public static decimal ApplyRounding(decimal value, StaffingRoundingMethod method)
        {
            return method switch
            {
                StaffingRoundingMethod.Ceiling => Math.Ceiling(value),
                StaffingRoundingMethod.Floor => Math.Floor(value),
                StaffingRoundingMethod.Nearest => Math.Round(value, MidpointRounding.AwayFromZero),
                StaffingRoundingMethod.NoRounding => value,
                _ => Math.Ceiling(value)
            };
        }

        private static string FormatNumber(decimal value)
        {
            return value.ToString("0.##", IdCulture);
        }
    }
}
