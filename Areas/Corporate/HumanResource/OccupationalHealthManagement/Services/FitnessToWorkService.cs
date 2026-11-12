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
    /// Implementasi Evaluasi Kelaikan Kerja Operasional (Fitness to Work) & Validasi Dokter Hiperkes (BE-HRD-290, VR-OHS-03, VR-OHS-13, INT-HR-92)
    /// </summary>
    public class FitnessToWorkService : IFitnessToWorkService
    {
        private readonly IOccupationalHealthUnitOfWork _uow;

        public FitnessToWorkService(IOccupationalHealthUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task<EmployeeFitnessSummaryDto> AssessFitnessAsync(Guid employeeId, UpdateFitnessStatusRequestDto request, Guid doctorUserId, CancellationToken cancellationToken = default)
        {
            // Aturan VR-OHS-03: Dokter pengesah kelaikan kerja wajib memiliki nomor sertifikat Hiperkes aktif
            if (string.IsNullOrWhiteSpace(request.DoctorHiperkesRegNumber))
            {
                throw new InvalidOperationException("Pengesahan kelaikan kerja nakes ditolak: Nomor registrasi sertifikasi Hiperkes / K3RS dokter wajib disertakan (VR-OHS-03).");
            }

            var employee = await _uow.Employees
                .FirstOrDefaultAsync(x => x.Id == employeeId, cancellationToken);
            if (employee == null)
                throw new KeyNotFoundException($"Pegawai dengan ID {employeeId} tidak ditemukan.");

            var activeAssessment = await _uow.FitnessToWorks
                .FirstOrDefaultAsync(x => x.EmployeeId == employeeId && x.ApprovedAt != null && x.EffectiveEndDate >= DateTime.UtcNow, cancellationToken);

            // Aturan VR-OHS-13: Status kelaikan kerja yang telah disahkan bersifat imutabel
            // Untuk pembaruan, buat record asesmen baru dan arsipkan yang lama
            if (activeAssessment != null)
            {
                activeAssessment.IsActive = false;
                activeAssessment.UpdateDateTime = DateTime.UtcNow;
                activeAssessment.UpdateBy = doctorUserId;
                _uow.Update(activeAssessment);
            }

            var encryptedNotes = AesClinicalEncryptionHelper.Encrypt(request.ClinicalNotesEncrypted ?? request.OperationalRestrictionSummary);

            var newAssessment = new TrxEmployeeFitnessToWork
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = employee.WorkforceProfileId,
                EmployeeId = employee.Id,
                AssessmentNumber = $"FTW-HPK-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}",
                AssessmentDate = DateTime.UtcNow,
                FitnessStatus = request.FitnessStatus,
                EffectiveStartDate = DateTime.UtcNow,
                EffectiveEndDate = request.ValidUntil,
                WorkRestrictionRequired = request.FitnessStatus == "FitWithRestriction",
                AdministrativeRestrictionSummary = request.OperationalRestrictionSummary,
                ClinicalBasisRestricted = encryptedNotes,
                IsSchedulingAllowed = request.FitnessStatus != "PermanentlyUnfit" && request.FitnessStatus != "TemporarilyUnfit",
                IsClinicalDutyAllowed = request.FitnessStatus == "Fit",
                AssessedByProvider = $"Dokter Hiperkes RS (No. Reg: {request.DoctorHiperkesRegNumber})",
                ApprovedByUserId = doctorUserId,
                ApprovedAt = DateTime.UtcNow,
                IsActive = true,
                CreateBy = doctorUserId,
                CreateDateTime = DateTime.UtcNow,
                UpdateBy = doctorUserId,
                UpdateDateTime = DateTime.UtcNow
            };

            await _uow.AddAsync(newAssessment, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            return new EmployeeFitnessSummaryDto
            {
                EmployeeId = employee.Id,
                EmployeeName = employee.FullName,
                EmployeeNumber = employee.EmployeeNumber ?? string.Empty,
                DepartmentName = "Unit Kerja Terdaftar",
                FitnessStatus = newAssessment.FitnessStatus,
                ValidUntil = newAssessment.EffectiveEndDate ?? DateTime.UtcNow.AddMonths(12),
                HasActiveRestriction = newAssessment.WorkRestrictionRequired,
                RestrictionDirectives = newAssessment.AdministrativeRestrictionSummary,
                IsSchedulingAllowed = newAssessment.IsSchedulingAllowed,
                IsClinicalDutyAllowed = newAssessment.IsClinicalDutyAllowed,
                AssessedByDoctor = newAssessment.AssessedByProvider ?? "Dokter Hiperkes RS",
                LastAssessedDate = newAssessment.AssessmentDate
            };
        }

        public async Task<EmployeeFitnessSummaryDto> GetLatestAssessmentAsync(Guid employeeId, CancellationToken cancellationToken = default)
        {
            var employee = await _uow.Employees
                .FirstOrDefaultAsync(x => x.Id == employeeId, cancellationToken);
            if (employee == null)
                throw new KeyNotFoundException($"Pegawai dengan ID {employeeId} tidak ditemukan.");

            var latest = await _uow.FitnessToWorks
                .Where(x => x.EmployeeId == employeeId)
                .OrderByDescending(x => x.AssessmentDate)
                .FirstOrDefaultAsync(cancellationToken);

            return new EmployeeFitnessSummaryDto
            {
                EmployeeId = employee.Id,
                EmployeeName = employee.FullName,
                EmployeeNumber = employee.EmployeeNumber ?? string.Empty,
                DepartmentName = "Unit Kerja Terdaftar",
                FitnessStatus = latest?.FitnessStatus ?? "Fit",
                ValidUntil = latest?.EffectiveEndDate ?? DateTime.UtcNow.AddMonths(12),
                HasActiveRestriction = latest?.WorkRestrictionRequired ?? false,
                RestrictionDirectives = latest?.AdministrativeRestrictionSummary,
                IsSchedulingAllowed = latest?.IsSchedulingAllowed ?? true,
                IsClinicalDutyAllowed = latest?.IsClinicalDutyAllowed ?? true,
                AssessedByDoctor = latest?.AssessedByProvider ?? "Dokter Hiperkes RS",
                LastAssessedDate = latest?.AssessmentDate ?? DateTime.UtcNow
            };
        }

        public async Task<List<EmployeeFitnessSummaryDto>> GetAssessmentHistoryAsync(Guid employeeId, CancellationToken cancellationToken = default)
        {
            var employee = await _uow.Employees
                .FirstOrDefaultAsync(x => x.Id == employeeId, cancellationToken);
            if (employee == null)
                throw new KeyNotFoundException($"Pegawai dengan ID {employeeId} tidak ditemukan.");

            var history = await _uow.FitnessToWorks
                .Where(x => x.EmployeeId == employeeId)
                .OrderByDescending(x => x.AssessmentDate)
                .ToListAsync(cancellationToken);

            return history.Select(h => new EmployeeFitnessSummaryDto
            {
                EmployeeId = employee.Id,
                EmployeeName = employee.FullName,
                EmployeeNumber = employee.EmployeeNumber ?? string.Empty,
                DepartmentName = "Unit Kerja Terdaftar",
                FitnessStatus = h.FitnessStatus,
                ValidUntil = h.EffectiveEndDate ?? DateTime.UtcNow,
                HasActiveRestriction = h.WorkRestrictionRequired,
                RestrictionDirectives = h.AdministrativeRestrictionSummary,
                IsSchedulingAllowed = h.IsSchedulingAllowed,
                IsClinicalDutyAllowed = h.IsClinicalDutyAllowed,
                AssessedByDoctor = h.AssessedByProvider ?? "Dokter Hiperkes RS",
                LastAssessedDate = h.AssessmentDate
            }).ToList();
        }
    }
}
