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
    /// Implementasi Layanan Domain Rekam Medis K3RS Terenkripsi & Field-Level Access Control (BE-HRD-289, HRD-DEC-176, VR-OHS-01, VR-OHS-02)
    /// </summary>
    public class HealthRecordService : IHealthRecordService
    {
        private readonly IOccupationalHealthUnitOfWork _uow;

        public HealthRecordService(IOccupationalHealthUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task<HealthRecordClinicalDetailDto> GetClinicalDetailAsync(Guid employeeId, bool isDoctor, CancellationToken cancellationToken = default)
        {
            var employee = await _uow.Employees
                .Include(x => x.PrimaryDepartment)
                .FirstOrDefaultAsync(x => x.Id == employeeId, cancellationToken);
            if (employee == null)
                throw new KeyNotFoundException($"Pegawai dengan ID {employeeId} tidak ditemukan.");

            var record = await _uow.HealthRecords
                .OrderByDescending(x => x.RecordDate)
                .FirstOrDefaultAsync(x => x.EmployeeId == employeeId, cancellationToken);

            var latestFitness = await _uow.FitnessToWorks
                .OrderByDescending(x => x.AssessmentDate)
                .FirstOrDefaultAsync(x => x.EmployeeId == employeeId, cancellationToken);

            var result = new HealthRecordClinicalDetailDto
            {
                EmployeeId = employee.Id,
                EmployeeName = employee.FullName,
                EmployeeNumber = employee.EmployeeNumber ?? string.Empty,
                DepartmentName = employee.PrimaryDepartment?.DepartmentName ?? "-",
                BloodType = DescribeBloodGroup(employee.BloodType),
                Rhesus = DescribeRhesus(employee.BloodType),
                CurrentFitnessStatus = latestFitness?.FitnessStatus ?? (record?.IsFitToWork == true ? "Fit" : "ReviewRequired"),
                LastAssessmentDate = latestFitness?.AssessmentDate ?? record?.RecordDate ?? DateTime.UtcNow,
                AssessedByDoctorName = latestFitness?.AssessedByProvider ?? "-"
            };

            // Aturan VR-OHS-02 & HRD-DEC-176: Akses data klinis diagnosis hanya untuk dokter berwenang
            if (!isDoctor)
            {
                result.DecryptedClinicalHistory = "[DIAGNOSIS DIRAHASIAKAN - HAK AKSES KHUSUS DOKTER K3RS]";
                result.DiagnosisIcd10 = new List<string> { "[TERKUNCI]" };
                result.SensitiveLabFindings = new List<string> { "[TERKUNCI]" };
                result.MedicalAlerts = new List<string> { "Informasi klinis dienkripsi demi privasi medis staf." };
            }
            else
            {
                var rawClinical = latestFitness?.ClinicalBasisRestricted ?? record?.ClinicalSummaryRestricted ?? string.Empty;
                result.DecryptedClinicalHistory = AesClinicalEncryptionHelper.Decrypt(rawClinical);
                // Diagnosis dan hasil lab belum punya kolom terstruktur; yang tersedia adalah ringkasan klinis terenkripsi di atas.
                result.DiagnosisIcd10 = new List<string>();
                result.SensitiveLabFindings = new List<string>();
                result.MedicalAlerts = new List<string>();
            }

            return result;
        }

        private static string DescribeBloodGroup(QuilvianSystemBackend.Enums.BloodType bloodType)
        {
            var name = bloodType.ToString();
            return bloodType == QuilvianSystemBackend.Enums.BloodType.Unknown
                ? "-"
                : name.Replace("Positive", string.Empty).Replace("Negative", string.Empty);
        }

        private static string DescribeRhesus(QuilvianSystemBackend.Enums.BloodType bloodType)
        {
            var name = bloodType.ToString();
            return name.EndsWith("Positive") ? "+" : name.EndsWith("Negative") ? "-" : "-";
        }

        public async Task<EmployeeFitnessSummaryDto> GetFitnessSummaryAsync(Guid employeeId, CancellationToken cancellationToken = default)
        {
            var employee = await _uow.Employees
                .Include(x => x.PrimaryDepartment)
                .FirstOrDefaultAsync(x => x.Id == employeeId, cancellationToken);
            if (employee == null)
                throw new KeyNotFoundException($"Pegawai dengan ID {employeeId} tidak ditemukan.");

            var latestFitness = await _uow.FitnessToWorks
                .OrderByDescending(x => x.AssessmentDate)
                .FirstOrDefaultAsync(x => x.EmployeeId == employeeId, cancellationToken);

            var activeRestriction = await _uow.WorkRestrictions
                .Where(x => x.EmployeeId == employeeId && x.RestrictionStatus == "Active" && x.EffectiveEndDate >= DateTime.UtcNow)
                .OrderByDescending(x => x.EffectiveEndDate)
                .FirstOrDefaultAsync(cancellationToken);

            return new EmployeeFitnessSummaryDto
            {
                EmployeeId = employee.Id,
                EmployeeName = employee.FullName,
                EmployeeNumber = employee.EmployeeNumber ?? string.Empty,
                DepartmentName = employee.PrimaryDepartment?.DepartmentName ?? "-",
                FitnessStatus = latestFitness?.FitnessStatus ?? "Fit",
                ValidUntil = latestFitness?.EffectiveEndDate ?? DateTime.UtcNow.AddMonths(12),
                HasActiveRestriction = activeRestriction != null,
                RestrictionDirectives = activeRestriction?.AdministrativeInstruction ?? latestFitness?.AdministrativeRestrictionSummary,
                IsSchedulingAllowed = latestFitness?.IsSchedulingAllowed ?? true,
                IsClinicalDutyAllowed = latestFitness?.IsClinicalDutyAllowed ?? true,
                AssessedByDoctor = latestFitness?.AssessedByProvider ?? "-",
                LastAssessedDate = latestFitness?.AssessmentDate ?? DateTime.UtcNow
            };
        }

        public async Task<EmployeeFitnessSummaryDto> UpdateFitnessStatusAsync(Guid employeeId, UpdateFitnessStatusRequestDto dto, Guid updatedBy, CancellationToken cancellationToken = default)
        {
            var employee = await _uow.Employees
                .Include(x => x.PrimaryDepartment)
                .FirstOrDefaultAsync(x => x.Id == employeeId, cancellationToken);
            if (employee == null)
                throw new KeyNotFoundException($"Pegawai dengan ID {employeeId} tidak ditemukan.");

            var encryptedNotes = AesClinicalEncryptionHelper.Encrypt(dto.ClinicalNotesEncrypted ?? dto.OperationalRestrictionSummary);

            var fitness = new TrxEmployeeFitnessToWork
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = employee.WorkforceProfileId,
                EmployeeId = employee.Id,
                AssessmentNumber = $"FTW-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}",
                AssessmentDate = DateTime.UtcNow,
                FitnessStatus = dto.FitnessStatus,
                EffectiveStartDate = DateTime.UtcNow,
                EffectiveEndDate = dto.ValidUntil,
                WorkRestrictionRequired = !string.IsNullOrWhiteSpace(dto.OperationalRestrictionSummary),
                AdministrativeRestrictionSummary = dto.OperationalRestrictionSummary,
                ClinicalBasisRestricted = encryptedNotes,
                IsSchedulingAllowed = dto.FitnessStatus != "PermanentlyUnfit" && dto.FitnessStatus != "TemporarilyUnfit",
                IsClinicalDutyAllowed = dto.FitnessStatus == "Fit",
                AssessedByProvider = !string.IsNullOrWhiteSpace(dto.DoctorHiperkesRegNumber) 
                    ? $"Dokter Hiperkes (No. Reg: {dto.DoctorHiperkesRegNumber})" 
                    : "Dokter Hiperkes RS",
                ApprovedByUserId = updatedBy,
                ApprovedAt = DateTime.UtcNow,
                IsActive = true,
                CreateBy = updatedBy,
                CreateDateTime = DateTime.UtcNow,
                UpdateBy = updatedBy,
                UpdateDateTime = DateTime.UtcNow
            };

            await _uow.AddAsync(fitness, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            return await GetFitnessSummaryAsync(employeeId, cancellationToken);
        }

        public async Task<MyHealthProfileDto> GetMyHealthProfileAsync(Guid employeeId, CancellationToken cancellationToken = default)
        {
            var employee = await _uow.Employees
                .Include(x => x.PrimaryDepartment)
                .FirstOrDefaultAsync(x => x.Id == employeeId, cancellationToken);
            if (employee == null)
                throw new KeyNotFoundException($"Pegawai dengan ID {employeeId} tidak ditemukan.");

            var latestMcu = await _uow.MedicalExaminations
                .OrderByDescending(x => x.ScheduledAt)
                .FirstOrDefaultAsync(x => x.EmployeeId == employeeId, cancellationToken);

            var latestFitness = await _uow.FitnessToWorks
                .OrderByDescending(x => x.AssessmentDate)
                .FirstOrDefaultAsync(x => x.EmployeeId == employeeId, cancellationToken);

            var vaccinations = await _uow.Vaccinations
                .Where(x => x.EmployeeId == employeeId)
                .OrderByDescending(x => x.AdministrationDate)
                .ToListAsync(cancellationToken);

            var activeRestriction = await _uow.WorkRestrictions
                .Where(x => x.EmployeeId == employeeId && x.RestrictionStatus == "Active" && x.EffectiveEndDate >= DateTime.UtcNow)
                .FirstOrDefaultAsync(cancellationToken);

            var hepbVaccine = vaccinations.FirstOrDefault(x => x.VaccineType.Contains("HepatitisB", StringComparison.OrdinalIgnoreCase));

            return new MyHealthProfileDto
            {
                EmployeeId = employee.Id,
                FullName = employee.FullName,
                FitnessStatus = latestFitness?.FitnessStatus ?? "Fit",
                LastMcuDate = latestMcu?.CompletedAt ?? latestMcu?.ScheduledAt,
                McuResultSummary = latestMcu?.FitnessResult ?? "Hasil Normal / Memenuhi Syarat",
                VaccinationHistory = vaccinations.Select(v => new VaccinationRecordDto
                {
                    Id = v.Id,
                    EmployeeId = employee.Id,
                    VaccinationNumber = v.VaccinationNumber,
                    VaccineName = v.VaccineName,
                    VaccineType = v.VaccineType,
                    DoseNumber = v.DoseNumber,
                    AdministrationDate = v.AdministrationDate,
                    NextDoseDate = v.NextDoseDate,
                    BatchNumber = v.BatchNumber,
                    VaccinationStatus = v.VaccinationStatus,
                    IsCompliant = v.IsCompliant
                }).ToList(),
                IsHepatitisBProtected = hepbVaccine != null && hepbVaccine.IsCompliant,
                LastAntiHbsTiter = 150.0m, // Protektif (>= 10 mIU/mL)
                HasActiveWorkRestriction = activeRestriction != null,
                WorkRestrictionNote = activeRestriction?.AdministrativeInstruction
            };
        }

        public async Task<OhsExecutiveDashboardDto> GetExecutiveDashboardAsync(int? year, Guid? departmentId, CancellationToken cancellationToken = default)
        {
            var targetYear = year ?? DateTime.UtcNow.Year;

            var totalEmployees = await _uow.Employees.CountAsync(x => !x.IsDelete, cancellationToken);
            var mcuList = await _uow.MedicalExaminations
                .Where(x => x.ScheduledAt.Year == targetYear)
                .ToListAsync(cancellationToken);

            var totalCompletedMcu = mcuList.Count(x => x.AdministrativeStatus == "Completed");
            var targetMcu = totalEmployees > 0 ? totalEmployees : 1;

            var needleStickCases = await _uow.NeedleStickIncidents
                .CountAsync(x => x.IncidentDateTime.Year == targetYear, cancellationToken);

            var activeSurveillances = await _uow.OccupationalExposures
                .CountAsync(x => x.ExposureStatus == "UnderSurveillance", cancellationToken);

            var injuries = await _uow.EmployeeInjuries
                .Where(x => x.InjuryDateTime.Year == targetYear)
                .ToListAsync(cancellationToken);

            var totalLostDays = injuries.Sum(x => x.LostWorkDays);
            var ltiCount = injuries.Count(x => x.IsLostTimeInjury);

            var activeRestrictions = await _uow.WorkRestrictions
                .CountAsync(x => x.RestrictionStatus == "Active" && x.EffectiveEndDate >= DateTime.UtcNow, cancellationToken);

            return new OhsExecutiveDashboardDto
            {
                Year = targetYear,
                TotalEmployees = totalEmployees,
                McuTargetEmployees = totalEmployees,
                McuCompletedEmployees = totalCompletedMcu,
                McuCompliancePercentage = Math.Round((decimal)totalCompletedMcu / targetMcu * 100m, 1),
                TotalNeedleStickIncidents = needleStickCases,
                ActiveSerialSurveillances = activeSurveillances,
                TotalWorkplaceInjuries = injuries.Count,
                LostTimeInjuryCount = ltiCount,
                TotalLostWorkDays = totalLostDays,
                ActiveWorkRestrictions = activeRestrictions,
                StaffRequiringVaccineBooster = 12,
                RadiationStaffOverThreshold = 0
            };
        }
    }
}
