using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Services
{
    /// <summary>
    /// Implementasi Layanan Surveilans Bahaya Ruangan & Pemantauan Dosis Radiasi TLD (BE-HRD-298, VR-OHS-17)
    /// </summary>
    /// <remarks>
    /// Remediasi 10 November 2026: radar dosis kumulatif dihitung dari catatan dosis yang disimpan endpoint
    /// <c>surveillance/radiation-dose</c> (kolom <c>MeasuredDoseMsv</c>), bukan lagi angka tetap 4,8 mSv / 1,2 mSv
    /// untuk semua pekerja. Nama unit kerja diambil dari departemen utama pegawai.
    /// </remarks>
    public class HazardSurveillanceService : IHazardSurveillanceService
    {
        private const decimal QuarterlyThresholdMsv = 5.0m;   // Triwulanan > 5 mSv memicu alarm Bapeten (VR-OHS-17)
        private const decimal YearlyThresholdMsv = 20.0m;     // NBD tahunan pekerja radiasi 20 mSv
        private const decimal NearThresholdYearlyMsv = 15.0m;

        // Cadangan untuk catatan lama yang dibuat sebelum kolom MeasuredDoseMsv ada: dosis dibaca dari teks hasil administratif.
        private static readonly Regex LegacyDosePattern = new(@"Dosis Tercatat:\s*([\d]+(?:[.,]\d+)?)\s*mSv", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly IOccupationalHealthUnitOfWork _uow;
        private readonly NumberSeriesAllocator _numberAllocator;

        public HazardSurveillanceService(IOccupationalHealthUnitOfWork uow, NumberSeriesAllocator numberAllocator)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _numberAllocator = numberAllocator ?? throw new ArgumentNullException(nameof(numberAllocator));
        }

        public async Task<HealthSurveillanceResponseDto> RecordRadiationDoseAsync(RecordRadiationDoseRequestDto request, Guid recordedByUserId, CancellationToken cancellationToken = default)
        {
            var employee = await _uow.Employees
                .FirstOrDefaultAsync(x => x.Id == request.EmployeeId, cancellationToken);
            if (employee == null)
                throw new KeyNotFoundException($"Pegawai dengan ID {request.EmployeeId} tidak ditemukan.");

            var isOverThreshold = request.DoseMsv > QuarterlyThresholdMsv;

            var surveillance = new TrxEmployeeHealthSurveillance
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = employee.WorkforceProfileId,
                EmployeeId = employee.Id,
                SurveillanceNumber = await _numberAllocator.AllocateAsync(new NumberAllocationRequest(
                SequenceKey: OccupationalHealthNumberSeries.RadiationKey,
                Prefix: OccupationalHealthNumberSeries.RadiationPrefix,
                ResetPolicy: OccupationalHealthNumberSeries.RadiationResetPolicy,
                SequenceDigits: OccupationalHealthNumberSeries.SequenceDigits,
                ActorUserId: recordedByUserId == Guid.Empty ? OccupationalHealthNumberSeries.SystemFallbackUserId : recordedByUserId,
                Instant: DateTimeOffset.UtcNow), cancellationToken),
                SurveillanceProgram = "Pemantauan Dosis Radiasi Pekerja Radiasi (TLD Badge)",
                RiskFactor = "Ionizing Radiation (X-Ray / CT / Cath Lab)",
                ScheduledDate = request.PeriodEnd,
                CompletedDate = DateTime.UtcNow,
                SurveillanceStatus = "Completed",
                MeasuredDoseMsv = request.DoseMsv,
                AdministrativeOutcome = $"Dosis Tercatat: {request.DoseMsv.ToString(CultureInfo.InvariantCulture)} mSv. Badge: {request.BadgeNumber ?? "TLD-STD"}. {(isOverThreshold ? "[PERINGATAN]: Melebihi ambang batas triwulanan 5 mSv!" : "Dalam batas aman.")}",
                ClinicalFindingsRestricted = AesClinicalEncryptionHelper.Encrypt($"Pembacaan Lab: {request.ReaderLaboratoryName ?? "BATAN/BRIN"}. Dosis: {request.DoseMsv} mSv. Periode: {request.PeriodStart:dd/MM/yyyy} s.d. {request.PeriodEnd:dd/MM/yyyy}."),
                IsCompliant = !isOverThreshold,
                IsActive = true,
                CreateBy = recordedByUserId,
                CreateDateTime = DateTime.UtcNow,
                UpdateBy = recordedByUserId,
                UpdateDateTime = DateTime.UtcNow
            };

            await _uow.AddAsync(surveillance, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            return new HealthSurveillanceResponseDto
            {
                Id = surveillance.Id,
                EmployeeId = employee.Id,
                SurveillanceNumber = surveillance.SurveillanceNumber,
                SurveillanceProgram = surveillance.SurveillanceProgram,
                ScheduledDate = surveillance.ScheduledDate,
                CompletedDate = surveillance.CompletedDate,
                SurveillanceStatus = surveillance.SurveillanceStatus,
                AdministrativeOutcome = surveillance.AdministrativeOutcome,
                IsCompliant = surveillance.IsCompliant
            };
        }

        public async Task<List<RadiationSurveillanceRadarDto>> GetRadiationRadarAsync(int? year, bool? isNearThresholdOnly, CancellationToken cancellationToken = default)
        {
            var targetYear = year ?? DateTime.UtcNow.Year;

            // Cari seluruh data surveilans radiasi tahun berjalan
            var surveillances = await _uow.HealthSurveillances
                .Where(x => !x.IsDelete && x.SurveillanceProgram.Contains("Radiasi") && x.ScheduledDate.Year == targetYear)
                .ToListAsync(cancellationToken);

            var empIds = surveillances.Select(x => x.EmployeeId).Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();
            var employees = await _uow.Employees
                .Include(x => x.PrimaryDepartment)
                .Where(x => empIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, cancellationToken);

            var list = new List<RadiationSurveillanceRadarDto>();

            foreach (var empId in empIds)
            {
                employees.TryGetValue(empId, out var emp);

                var records = surveillances
                    .Where(x => x.EmployeeId == empId)
                    .Select(x => new { x.ScheduledDate, Dose = ResolveDose(x) })
                    .Where(x => x.Dose.HasValue)
                    .ToList();

                // Dosis kumulatif tahunan = jumlah seluruh pembacaan TLD pada tahun target.
                var yearlyDose = records.Sum(x => x.Dose!.Value);

                // Dosis triwulanan = jumlah pembacaan pada triwulan terakhir yang punya catatan di tahun target.
                var latestQuarter = records.Count > 0 ? records.Max(x => QuarterOf(x.ScheduledDate)) : 0;
                var quarterlyDose = records.Where(x => QuarterOf(x.ScheduledDate) == latestQuarter).Sum(x => x.Dose!.Value);

                var isOverQuarter = quarterlyDose > QuarterlyThresholdMsv;
                var isOverYear = yearlyDose > YearlyThresholdMsv;

                if (isNearThresholdOnly == true && yearlyDose < NearThresholdYearlyMsv && !isOverQuarter)
                    continue;

                list.Add(new RadiationSurveillanceRadarDto
                {
                    EmployeeId = empId,
                    EmployeeName = emp != null ? emp.FullName : "Pekerja Radiasi",
                    EmployeeNumber = emp?.EmployeeNumber ?? string.Empty,
                    DepartmentName = emp?.PrimaryDepartment?.DepartmentName ?? "-",
                    QuarterlyDoseMsv = Math.Round(quarterlyDose, 3),
                    CumulativeYearlyDoseMsv = Math.Round(yearlyDose, 3),
                    MaxPermissibleYearlyDoseMsv = YearlyThresholdMsv,
                    IsOverQuarterlyThreshold = isOverQuarter,
                    IsOverYearlyThreshold = isOverYear,
                    RotationRecommendation = isOverYear ? "RotasiWajibKeluarZonaRadiasi" : (isOverQuarter ? "EvaluasiTeknikPenyinaran" : "NormalDuty")
                });
            }

            return list
                .OrderByDescending(x => x.CumulativeYearlyDoseMsv)
                .ToList();
        }

        private static int QuarterOf(DateTime date) => ((date.Month - 1) / 3) + 1;

        /// <summary>
        /// Dosis dari kolom terukur; untuk catatan lama tanpa kolom itu, dibaca dari teks "Dosis Tercatat: x mSv".
        /// </summary>
        private static decimal? ResolveDose(TrxEmployeeHealthSurveillance record)
        {
            if (record.MeasuredDoseMsv.HasValue) return record.MeasuredDoseMsv.Value;
            if (string.IsNullOrWhiteSpace(record.AdministrativeOutcome)) return null;

            var match = LegacyDosePattern.Match(record.AdministrativeOutcome);
            if (!match.Success) return null;

            var raw = match.Groups[1].Value.Replace(',', '.');
            return decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var dose) ? dose : null;
        }
    }
}
