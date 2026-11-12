using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Models;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Services
{
    /// <summary>
    /// Implementasi Layanan Manajemen Pemeriksaan Kesehatan Berkala Staf (MCU) Berbasis Profil Risiko & Kepatuhan STARKES KPS (BE-HRD-299, VR-OHS-12, AT-HRD-OHS-22)
    /// </summary>
    public class MedicalExaminationService : IMedicalExaminationService
    {
        private readonly IOccupationalHealthUnitOfWork _uow;

        public MedicalExaminationService(IOccupationalHealthUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task<MedicalExaminationResponseDto> ScheduleMcuAsync(ScheduleMcuRequestDto request, Guid scheduledByUserId, CancellationToken cancellationToken = default)
        {
            var employee = await _uow.Employees
                .FirstOrDefaultAsync(x => x.Id == request.EmployeeId, cancellationToken);
            if (employee == null)
                throw new KeyNotFoundException($"Pegawai dengan ID {request.EmployeeId} tidak ditemukan.");

            // Penentuan paket MCU berbasis profil bahaya ruangan kerja (VR-OHS-12)
            var riskProfile = request.RiskProfile ?? "Standard";
            var packageNotes = $"Paket MCU: [{riskProfile}] disesuaikan dengan paparan ruangan kerja staf. Catatan: {request.Notes}";

            var mcu = new TrxEmployeeMedicalExamination
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = employee.WorkforceProfileId,
                EmployeeId = employee.Id,
                ExaminationNumber = $"MCU-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}",
                ExaminationType = request.ExamType,
                ScheduledAt = request.ExaminationDate,
                ProviderName = request.ProviderName ?? "Pusat MCU K3RS RS MMC",
                AdministrativeStatus = "Scheduled",
                IsMandatory = true,
                IsClinicalDataRestricted = true,
                AdministrativeNotes = packageNotes,
                IsActive = true,
                CreateBy = scheduledByUserId,
                CreateDateTime = DateTime.UtcNow,
                UpdateBy = scheduledByUserId,
                UpdateDateTime = DateTime.UtcNow
            };

            await _uow.AddAsync(mcu, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            return new MedicalExaminationResponseDto
            {
                Id = mcu.Id,
                EmployeeId = employee.Id,
                EmployeeName = employee.FullName,
                ExaminationNumber = mcu.ExaminationNumber,
                ExaminationType = mcu.ExaminationType,
                ScheduledAt = mcu.ScheduledAt,
                CompletedAt = mcu.CompletedAt,
                AdministrativeStatus = mcu.AdministrativeStatus,
                FitnessResult = mcu.FitnessResult,
                ProviderName = mcu.ProviderName,
                NextExaminationDate = mcu.NextExaminationDate,
                IsClinicalDataRestricted = mcu.IsClinicalDataRestricted
            };
        }

        public async Task<PagedResult<MedicalExaminationSummaryDto>> GetMcuListAsync(string? examType, string? riskProfile, string? status, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var query = _uow.MedicalExaminations.AsQueryable();

            if (!string.IsNullOrWhiteSpace(examType))
                query = query.Where(x => x.ExaminationType == examType);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(x => x.AdministrativeStatus == status);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(x => x.ScheduledAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var empIds = items.Select(x => x.EmployeeId).Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();
            var employees = await _uow.Employees.Include(x => x.PrimaryDepartment).Where(x => empIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, cancellationToken);

            var dtos = items.Select(m => new MedicalExaminationSummaryDto
            {
                Id = m.Id,
                EmployeeId = m.EmployeeId ?? Guid.Empty,
                EmployeeName = m.EmployeeId.HasValue && employees.TryGetValue(m.EmployeeId.Value, out var emp)
                    ? emp.FullName
                    : "Pegawai RS",
                EmployeeNumber = m.EmployeeId.HasValue && employees.TryGetValue(m.EmployeeId.Value, out var e)
                    ? e.EmployeeNumber ?? string.Empty
                    : string.Empty,
                DepartmentName = m.EmployeeId.HasValue && employees.TryGetValue(m.EmployeeId.Value, out var deptEmployee) ? deptEmployee.PrimaryDepartment?.DepartmentName ?? "-" : "-",
                ExaminationNumber = m.ExaminationNumber,
                ExaminationType = m.ExaminationType,
                ScheduledAt = m.ScheduledAt,
                CompletedAt = m.CompletedAt,
                AdministrativeStatus = m.AdministrativeStatus,
                FitnessResult = m.FitnessResult
            }).ToList();

            var totalPage = (int)Math.Ceiling((double)totalCount / pageSize);

            return new PagedResult<MedicalExaminationSummaryDto>
            {
                PageNumber = page,
                PageSize = pageSize,
                TotalData = totalCount,
                TotalPage = totalPage,
                Items = dtos
            };
        }

        public async Task<MedicalExaminationResponseDto> RecordMcuResultsAsync(Guid examinationId, RecordMcuResultsRequestDto request, Guid doctorUserId, CancellationToken cancellationToken = default)
        {
            var mcu = await _uow.MedicalExaminations
                .FirstOrDefaultAsync(x => x.Id == examinationId, cancellationToken);
            if (mcu == null)
                throw new KeyNotFoundException($"Pemeriksaan MCU dengan ID {examinationId} tidak ditemukan.");

            var employeeId = mcu.EmployeeId ?? mcu.WorkforceProfileId;

            // Catat enkripsi catatan klinis
            var rawFindings = $"Fisik: {request.PhysicalFindingsEncrypted}. Lab: {request.LabResultsEncrypted}. Rontgen: {request.ThoraxXrayFindingsEncrypted}";
            var encryptedFindings = AesClinicalEncryptionHelper.Encrypt(rawFindings);

            mcu.CompletedAt = DateTime.UtcNow;
            mcu.AdministrativeStatus = "Completed";
            mcu.FitnessResult = request.FitnessConclusion;
            mcu.NextExaminationDate = request.NextRecommendedMcuDate ?? DateTime.UtcNow.AddYears(1);
            mcu.ClinicalRecordReference = encryptedFindings;
            mcu.AdministrativeNotes = request.AdministrativeNotes ?? "Hasil pemeriksaan MCU telah disahkan oleh dokter pemeriksa K3RS.";
            mcu.UpdateDateTime = DateTime.UtcNow;
            mcu.UpdateBy = doctorUserId;
            _uow.Update(mcu);

            // Sinkronkan status kelaikan kerja
            var fitness = new TrxEmployeeFitnessToWork
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = mcu.WorkforceProfileId,
                EmployeeId = mcu.EmployeeId,
                MedicalExaminationId = mcu.Id,
                AssessmentNumber = $"FTW-MCU-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}",
                AssessmentDate = DateTime.UtcNow,
                FitnessStatus = request.FitnessConclusion,
                EffectiveStartDate = DateTime.UtcNow,
                EffectiveEndDate = mcu.NextExaminationDate,
                WorkRestrictionRequired = request.FitnessConclusion == "FitWithRestriction",
                AdministrativeRestrictionSummary = request.AdministrativeNotes,
                ClinicalBasisRestricted = encryptedFindings,
                IsSchedulingAllowed = request.FitnessConclusion != "PermanentlyUnfit" && request.FitnessConclusion != "TemporarilyUnfit",
                IsClinicalDutyAllowed = request.FitnessConclusion == "Fit",
                AssessedByProvider = !string.IsNullOrWhiteSpace(request.DoctorHiperkesRegNumber)
                    ? $"Dokter Hiperkes MCU (No. Reg: {request.DoctorHiperkesRegNumber})"
                    : "Dokter Hiperkes MCU RS",
                ApprovedByUserId = doctorUserId,
                ApprovedAt = DateTime.UtcNow,
                IsActive = true,
                CreateBy = doctorUserId,
                CreateDateTime = DateTime.UtcNow,
                UpdateBy = doctorUserId,
                UpdateDateTime = DateTime.UtcNow
            };

            await _uow.AddAsync(fitness, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            var emp = mcu.EmployeeId.HasValue 
                ? await _uow.Employees.FirstOrDefaultAsync(x => x.Id == mcu.EmployeeId.Value, cancellationToken)
                : null;

            return new MedicalExaminationResponseDto
            {
                Id = mcu.Id,
                EmployeeId = employeeId,
                EmployeeName = emp != null ? emp.FullName : "Pegawai RS",
                ExaminationNumber = mcu.ExaminationNumber,
                ExaminationType = mcu.ExaminationType,
                ScheduledAt = mcu.ScheduledAt,
                CompletedAt = mcu.CompletedAt,
                AdministrativeStatus = mcu.AdministrativeStatus,
                FitnessResult = mcu.FitnessResult,
                ProviderName = mcu.ProviderName,
                NextExaminationDate = mcu.NextExaminationDate,
                IsClinicalDataRestricted = mcu.IsClinicalDataRestricted
            };
        }
    }
}
