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
    /// Implementasi Layanan Asesmen Kembali Bekerja (Return to Work / RTW) & Pembuka Gembok Roster Sub-06 (BE-HRD-293, VR-OHS-07, INT-HR-89)
    /// </summary>
    public class ReturnToWorkService : IReturnToWorkService
    {
        private readonly IOccupationalHealthUnitOfWork _uow;

        public ReturnToWorkService(IOccupationalHealthUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task<ReturnToWorkAssessmentDto> AssessReturnToWorkAsync(Guid restrictionId, RecordReturnToWorkRequestDto request, Guid doctorUserId, CancellationToken cancellationToken = default)
        {
            var restriction = await _uow.WorkRestrictions
                .FirstOrDefaultAsync(x => x.Id == restrictionId, cancellationToken);
            if (restriction == null)
                throw new KeyNotFoundException($"Surat pembatasan tugas dengan ID {restrictionId} tidak ditemukan.");

            var employeeId = restriction.EmployeeId ?? restriction.WorkforceProfileId;

            var rtw = new TrxReturnToWorkAssessment
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = restriction.WorkforceProfileId,
                EmployeeId = restriction.EmployeeId,
                AssessmentNumber = $"RTW-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}",
                AssessmentDate = DateTime.UtcNow,
                ExpectedReturnDate = request.ActualReturnDate,
                ActualReturnDate = request.ActualReturnDate,
                FitnessStatus = request.ConclusionStatus,
                IsPhasedReturn = request.ConclusionStatus == "FitWithModifiedDuty",
                PhasedReturnStartDate = request.ConclusionStatus == "FitWithModifiedDuty" ? request.ActualReturnDate : null,
                PhasedReturnEndDate = request.ConclusionStatus == "FitWithModifiedDuty" ? request.ActualReturnDate.AddDays(30) : null,
                AdministrativeRestrictionSummary = request.DoctorNotes ?? (request.IsRestrictionLifted ? "Restriksi dicabut penuh. Staf layak dinas normal." : "Masih dalam evaluasi pemulihan."),
                IsSchedulingAllowed = request.ConclusionStatus != "RemainUnfit",
                IsClinicalDutyAllowed = request.ConclusionStatus == "FitForFullDuty",
                AssessedByProvider = !string.IsNullOrWhiteSpace(request.DoctorHiperkesRegNumber) 
                    ? $"Dokter Hiperkes RS (No. Reg: {request.DoctorHiperkesRegNumber})" 
                    : "Dokter Hiperkes RS",
                ApprovedByUserId = doctorUserId,
                ApprovedAt = DateTime.UtcNow,
                AssessmentStatus = "Completed",
                IsActive = true,
                CreateBy = doctorUserId,
                CreateDateTime = DateTime.UtcNow,
                UpdateBy = doctorUserId,
                UpdateDateTime = DateTime.UtcNow
            };

            await _uow.AddAsync(rtw, cancellationToken);

            // Aturan VR-OHS-07 & INT-HR-89: Jika dinyatakan FitForFullDuty / IsRestrictionLifted, cabut restriksi dan buka gembok Sub-06
            if (request.IsRestrictionLifted || request.ConclusionStatus == "FitForFullDuty")
            {
                restriction.RestrictionStatus = "Lifted";
                restriction.IsSchedulingBlocked = false;
                restriction.IsClinicalServiceBlocked = false;
                restriction.EffectiveEndDate = request.ActualReturnDate;
                restriction.UpdateDateTime = DateTime.UtcNow;
                restriction.UpdateBy = doctorUserId;
                _uow.Update(restriction);

                // Update juga latest fitness to work jika ada
                var latestFitness = await _uow.FitnessToWorks
                    .Where(x => x.EmployeeId == employeeId && x.EffectiveEndDate >= DateTime.UtcNow)
                    .OrderByDescending(x => x.AssessmentDate)
                    .FirstOrDefaultAsync(cancellationToken);

                if (latestFitness != null)
                {
                    latestFitness.FitnessStatus = "Fit";
                    latestFitness.WorkRestrictionRequired = false;
                    latestFitness.IsSchedulingAllowed = true;
                    latestFitness.IsClinicalDutyAllowed = true;
                    latestFitness.UpdateDateTime = DateTime.UtcNow;
                    latestFitness.UpdateBy = doctorUserId;
                    _uow.Update(latestFitness);
                }
            }

            await _uow.SaveChangesAsync(cancellationToken);

            return new ReturnToWorkAssessmentDto
            {
                Id = rtw.Id,
                EmployeeId = employeeId,
                AssessmentNumber = rtw.AssessmentNumber,
                AssessmentDate = rtw.AssessmentDate,
                ActualReturnDate = rtw.ActualReturnDate,
                FitnessStatus = rtw.FitnessStatus,
                IsSchedulingAllowed = rtw.IsSchedulingAllowed,
                IsClinicalDutyAllowed = rtw.IsClinicalDutyAllowed,
                AssessmentStatus = rtw.AssessmentStatus
            };
        }

        public async Task<List<ReturnToWorkAssessmentDto>> GetRtwHistoryByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default)
        {
            var assessments = await _uow.ReturnToWorkAssessments
                .Where(x => x.EmployeeId == employeeId)
                .OrderByDescending(x => x.AssessmentDate)
                .ToListAsync(cancellationToken);

            return assessments.Select(x => new ReturnToWorkAssessmentDto
            {
                Id = x.Id,
                EmployeeId = employeeId,
                AssessmentNumber = x.AssessmentNumber,
                AssessmentDate = x.AssessmentDate,
                ActualReturnDate = x.ActualReturnDate,
                FitnessStatus = x.FitnessStatus,
                IsSchedulingAllowed = x.IsSchedulingAllowed,
                IsClinicalDutyAllowed = x.IsClinicalDutyAllowed,
                AssessmentStatus = x.AssessmentStatus
            }).ToList();
        }
    }
}
