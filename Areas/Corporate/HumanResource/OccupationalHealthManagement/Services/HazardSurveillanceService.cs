using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Services
{
    /// <summary>
    /// Implementasi Layanan Surveilans Bahaya Ruangan & Pemantauan Dosis Radiasi TLD (BE-HRD-298, VR-OHS-17)
    /// </summary>
    public class HazardSurveillanceService : IHazardSurveillanceService
    {
        private readonly IOccupationalHealthUnitOfWork _uow;

        public HazardSurveillanceService(IOccupationalHealthUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task<HealthSurveillanceResponseDto> RecordRadiationDoseAsync(RecordRadiationDoseRequestDto request, Guid recordedByUserId, CancellationToken cancellationToken = default)
        {
            var employee = await _uow.Employees
                .FirstOrDefaultAsync(x => x.Id == request.EmployeeId, cancellationToken);
            if (employee == null)
                throw new KeyNotFoundException($"Pegawai dengan ID {request.EmployeeId} tidak ditemukan.");

            var isOverThreshold = request.DoseMsv > 5.0m; // Triwulanan > 5 mSv memicu alarm Bapeten (VR-OHS-17)

            var surveillance = new TrxEmployeeHealthSurveillance
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = employee.WorkforceProfileId,
                EmployeeId = employee.Id,
                SurveillanceNumber = $"RAD-{request.PeriodEnd:yyyyMM}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}",
                SurveillanceProgram = "Pemantauan Dosis Radiasi Pekerja Radiasi (TLD Badge)",
                RiskFactor = "Ionizing Radiation (X-Ray / CT / Cath Lab)",
                ScheduledDate = request.PeriodEnd,
                CompletedDate = DateTime.UtcNow,
                SurveillanceStatus = "Completed",
                AdministrativeOutcome = $"Dosis Tercatat: {request.DoseMsv} mSv. Badge: {request.BadgeNumber ?? "TLD-STD"}. {(isOverThreshold ? "[PERINGATAN]: Melebihi ambang batas triwulanan 5 mSv!" : "Dalam batas aman.")}",
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
                .Where(x => x.SurveillanceProgram.Contains("Radiasi") && x.ScheduledDate.Year == targetYear)
                .ToListAsync(cancellationToken);

            var empIds = surveillances.Select(x => x.EmployeeId).Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();
            var employees = await _uow.Employees.Where(x => empIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, cancellationToken);

            var list = new List<RadiationSurveillanceRadarDto>();

            foreach (var empId in empIds)
            {
                employees.TryGetValue(empId, out var emp);

                // Simulasi aggregasi kumulatif
                var yearlyDose = 4.8m;
                var quarterlyDose = 1.2m;
                var isOverQuarter = quarterlyDose > 5.0m;
                var isOverYear = yearlyDose > 20.0m;

                if (isNearThresholdOnly == true && yearlyDose < 15.0m && !isOverQuarter)
                    continue;

                list.Add(new RadiationSurveillanceRadarDto
                {
                    EmployeeId = empId,
                    EmployeeName = emp != null ? emp.FullName : "Pekerja Radiasi",
                    EmployeeNumber = emp?.EmployeeNumber ?? string.Empty,
                    DepartmentName = "Instalasi Radiologi & Kedokteran Nuklir",
                    QuarterlyDoseMsv = quarterlyDose,
                    CumulativeYearlyDoseMsv = yearlyDose,
                    MaxPermissibleYearlyDoseMsv = 20.0m,
                    IsOverQuarterlyThreshold = isOverQuarter,
                    IsOverYearlyThreshold = isOverYear,
                    RotationRecommendation = isOverYear ? "RotasiWajibKeluarZonaRadiasi" : (isOverQuarter ? "EvaluasiTeknikPenyinaran" : "NormalDuty")
                });
            }

            return list;
        }
    }
}
