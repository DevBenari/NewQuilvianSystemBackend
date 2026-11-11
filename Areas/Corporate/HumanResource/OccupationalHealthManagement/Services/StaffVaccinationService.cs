using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Models;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Services
{
    /// <summary>
    /// Implementasi Layanan Radar Imunisasi Staf, Log Batch Vaksin & Pemantauan Titer Protektif (BE-HRD-297, VR-OHS-14..16)
    /// </summary>
    public class StaffVaccinationService : IStaffVaccinationService
    {
        private readonly IOccupationalHealthUnitOfWork _uow;
        private readonly NumberSeriesAllocator _numberAllocator;

        public StaffVaccinationService(IOccupationalHealthUnitOfWork uow, NumberSeriesAllocator numberAllocator)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _numberAllocator = numberAllocator ?? throw new ArgumentNullException(nameof(numberAllocator));
        }

        public async Task<VaccinationRecordDto> RecordVaccinationAsync(RecordVaccinationRequestDto request, Guid recordedByUserId, CancellationToken cancellationToken = default)
        {
            var employee = await _uow.Employees
                .FirstOrDefaultAsync(x => x.Id == request.EmployeeId, cancellationToken);
            if (employee == null)
                throw new KeyNotFoundException($"Pegawai dengan ID {request.EmployeeId} tidak ditemukan.");

            // Validasi data batch vaksin & ED (VR-OHS-14)
            if (string.IsNullOrWhiteSpace(request.BatchNumber))
                throw new ArgumentException("Nomor batch vaksin wajib diisi (VR-OHS-14).");

            if (request.ExpiryDate <= request.AdministeredDate)
                throw new InvalidOperationException("Tanggal kedaluwarsa vaksin tidak valid: Vaksin telah melewati tanggal ED saat disuntikkan.");

            var vaccination = new TrxEmployeeVaccination
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = employee.WorkforceProfileId,
                EmployeeId = employee.Id,
                VaccinationNumber = $"VAC-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}",
                VaccineType = request.VaccineType,
                VaccineName = request.VaccineName,
                DiseasePrevented = request.VaccineType == "HepatitisB" ? "Hepatitis B Virus" : request.VaccineType,
                DoseNumber = request.DoseNumber,
                AdministrationDate = request.AdministeredDate,
                NextDoseDate = request.NextRecommendedDoseDate ?? request.AdministeredDate.AddMonths(request.DoseNumber == 1 ? 1 : (request.DoseNumber == 2 ? 5 : 60)),
                ExpiryDate = request.ExpiryDate,
                ProviderName = request.ProviderName ?? "Instalasi K3RS RS MMC",
                BatchNumber = request.BatchNumber,
                VaccinationStatus = "Administered",
                IsRelevantForRole = true,
                IsMandatory = true,
                IsCompliant = true,
                IsVerified = true,
                VerifiedByUserId = recordedByUserId,
                IsActive = true,
                CreateBy = recordedByUserId,
                CreateDateTime = DateTime.UtcNow,
                UpdateBy = recordedByUserId,
                UpdateDateTime = DateTime.UtcNow
            };

            await _uow.AddAsync(vaccination, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            return new VaccinationRecordDto
            {
                Id = vaccination.Id,
                EmployeeId = employee.Id,
                VaccinationNumber = vaccination.VaccinationNumber,
                VaccineName = vaccination.VaccineName,
                VaccineType = vaccination.VaccineType,
                DoseNumber = vaccination.DoseNumber,
                AdministrationDate = vaccination.AdministrationDate,
                NextDoseDate = vaccination.NextDoseDate,
                BatchNumber = vaccination.BatchNumber,
                VaccinationStatus = vaccination.VaccinationStatus,
                IsCompliant = vaccination.IsCompliant
            };
        }

        public async Task<List<VaccinationRecordDto>> GetVaccinationsByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default)
        {
            var vaccinations = await _uow.Vaccinations
                .Where(x => x.EmployeeId == employeeId)
                .OrderBy(x => x.VaccineType)
                .ThenBy(x => x.DoseNumber)
                .ToListAsync(cancellationToken);

            return vaccinations.Select(v => new VaccinationRecordDto
            {
                Id = v.Id,
                EmployeeId = employeeId,
                VaccinationNumber = v.VaccinationNumber,
                VaccineName = v.VaccineName,
                VaccineType = v.VaccineType,
                DoseNumber = v.DoseNumber,
                AdministrationDate = v.AdministrationDate,
                NextDoseDate = v.NextDoseDate,
                BatchNumber = v.BatchNumber,
                VaccinationStatus = v.VaccinationStatus,
                IsCompliant = v.IsCompliant
            }).ToList();
        }

        public async Task<HealthSurveillanceResponseDto> RecordAntibodyTiterAsync(RecordAntibodyTiterRequestDto request, Guid recordedByUserId, CancellationToken cancellationToken = default)
        {
            var employee = await _uow.Employees
                .FirstOrDefaultAsync(x => x.Id == request.EmployeeId, cancellationToken);
            if (employee == null)
                throw new KeyNotFoundException($"Pegawai dengan ID {request.EmployeeId} tidak ditemukan.");

            // Validasi ambang proteksi Anti-HBs kuantitatif 10 mIU/mL (VR-OHS-15)
            var isProtective = request.AntibodyTiterValue >= 10.0m;
            var interpretation = isProtective ? "Protective" : "NonProtective";

            var surveillance = new TrxEmployeeHealthSurveillance
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = employee.WorkforceProfileId,
                EmployeeId = employee.Id,
                SurveillanceNumber = await _numberAllocator.AllocateAsync(new NumberAllocationRequest(
                SequenceKey: OccupationalHealthNumberSeries.TiterKey,
                Prefix: OccupationalHealthNumberSeries.TiterPrefix,
                ResetPolicy: OccupationalHealthNumberSeries.TiterResetPolicy,
                SequenceDigits: OccupationalHealthNumberSeries.SequenceDigits,
                ActorUserId: recordedByUserId == Guid.Empty ? OccupationalHealthNumberSeries.SystemFallbackUserId : recordedByUserId,
                Instant: DateTimeOffset.UtcNow), cancellationToken),
                SurveillanceProgram = "Pemantauan Titer Antibodi Anti-HBs Kuantitatif",
                RiskFactor = "Biological Hazard (Hepatitis B Transmission)",
                ScheduledDate = request.TestDate,
                CompletedDate = DateTime.UtcNow,
                SurveillanceStatus = "Completed",
                AdministrativeOutcome = $"Kadar Titer: {request.AntibodyTiterValue} {request.TiterUnit} ({interpretation}). {(isProtective ? "Kekebalan Protektif Aktif." : "Diperlukan Vaksinasi Booster Segera.")}",
                ClinicalFindingsRestricted = AesClinicalEncryptionHelper.Encrypt($"Uji Laboratorium: {request.LaboratoryName ?? "Lab Sentral RS"}. Nilai Anti-HBs: {request.AntibodyTiterValue} {request.TiterUnit}. Kesimpulan: {interpretation}."),
                IsCompliant = isProtective,
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

        public async Task<PagedResult<NakesVaccinationRecallDto>> GetBoosterRecallListAsync(string? vaccineType, Guid? departmentId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            // Daftar dihitung dari data nyata: dosis lanjutan yang jatuh tempo H+30 dan hasil titer terakhir yang tidak protektif.
            var now = DateTime.UtcNow;
            var horizon = now.AddDays(30);

            var doseQuery = _uow.Vaccinations
                .Where(x => !x.IsDelete && x.IsActive && x.EmployeeId != null && x.NextDoseDate != null && x.NextDoseDate <= horizon && x.VaccinationStatus != "Completed");
            if (!string.IsNullOrWhiteSpace(vaccineType))
            {
                doseQuery = doseQuery.Where(x => x.VaccineType == vaccineType);
            }
            var dueDoses = await doseQuery.ToListAsync(cancellationToken);

            var titerRows = await _uow.HealthSurveillances
                .Where(x => !x.IsDelete && x.IsActive && x.EmployeeId != null && x.SurveillanceNumber.StartsWith(OccupationalHealthNumberSeries.TiterPrefix))
                .ToListAsync(cancellationToken);
            var lowTiters = titerRows
                .GroupBy(x => x.EmployeeId!.Value)
                .Select(g => g.OrderByDescending(x => x.CompletedDate ?? x.CreateDateTime).First())
                .Where(x => !x.IsCompliant)
                .ToList();

            var employeeIds = dueDoses.Select(x => x.EmployeeId!.Value).Concat(lowTiters.Select(x => x.EmployeeId!.Value)).Distinct().ToList();
            var employeeQuery = _uow.Employees.Include(x => x.PrimaryDepartment).Where(x => !x.IsDelete && employeeIds.Contains(x.Id));
            if (departmentId.HasValue)
            {
                employeeQuery = employeeQuery.Where(x => x.PrimaryDepartmentId == departmentId.Value);
            }
            var employeeMap = await employeeQuery.ToDictionaryAsync(x => x.Id, cancellationToken);

            var recalls = new List<NakesVaccinationRecallDto>();
            foreach (var titer in lowTiters.Where(x => employeeMap.ContainsKey(x.EmployeeId!.Value)))
            {
                var e = employeeMap[titer.EmployeeId!.Value];
                var match = System.Text.RegularExpressions.Regex.Match(titer.AdministrativeOutcome ?? string.Empty, @"Kadar Titer:\s*([\d]+(?:[.,]\d+)?)");
                recalls.Add(new NakesVaccinationRecallDto
                {
                    EmployeeId = e.Id,
                    EmployeeName = e.FullName,
                    EmployeeNumber = e.EmployeeNumber ?? string.Empty,
                    DepartmentName = e.PrimaryDepartment?.DepartmentName ?? "-",
                    RecallReason = "LowAntiHbsTiter (< 10 mIU/mL)",
                    LastKnownTiter = match.Success && decimal.TryParse(match.Groups[1].Value.Replace(',', '.'), System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var titerValue) ? titerValue : null,
                    DueDate = titer.CompletedDate,
                    UrgencyLevel = "High"
                });
            }
            foreach (var dose in dueDoses.Where(x => employeeMap.ContainsKey(x.EmployeeId!.Value)))
            {
                var e = employeeMap[dose.EmployeeId!.Value];
                recalls.Add(new NakesVaccinationRecallDto
                {
                    EmployeeId = e.Id,
                    EmployeeName = e.FullName,
                    EmployeeNumber = e.EmployeeNumber ?? string.Empty,
                    DepartmentName = e.PrimaryDepartment?.DepartmentName ?? "-",
                    RecallReason = $"BoosterDue ({dose.VaccineName})",
                    LastKnownTiter = null,
                    DueDate = dose.NextDoseDate,
                    UrgencyLevel = dose.NextDoseDate < now ? "High" : "Medium"
                });
            }

            var totalCount = recalls.Count;
            var dtos = recalls
                .OrderByDescending(x => x.UrgencyLevel == "High")
                .ThenBy(x => x.DueDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var totalPage = (int)Math.Ceiling((double)totalCount / pageSize);

            return new PagedResult<NakesVaccinationRecallDto>
            {
                PageNumber = page,
                PageSize = pageSize,
                TotalData = totalCount,
                TotalPage = totalPage,
                Items = dtos
            };
        }
    }
}
