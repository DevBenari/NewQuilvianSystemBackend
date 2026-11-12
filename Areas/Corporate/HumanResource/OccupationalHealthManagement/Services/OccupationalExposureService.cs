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

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Services
{
    /// <summary>
    /// Implementasi Layanan Fast-Track Pajanan Okupasional & Surveilans Serologi Serial 1-3-6 Bulan (BE-HRD-295, VR-OHS-09..11, INT-HR-91)
    /// </summary>
    public class OccupationalExposureService : IOccupationalExposureService
    {
        private readonly IOccupationalHealthUnitOfWork _uow;
        private readonly NumberSeriesAllocator _numberAllocator;

        public OccupationalExposureService(IOccupationalHealthUnitOfWork uow, NumberSeriesAllocator numberAllocator)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _numberAllocator = numberAllocator ?? throw new ArgumentNullException(nameof(numberAllocator));
        }

        public async Task<OccupationalExposureDto> RecordSerologyFollowupAsync(Guid exposureId, RecordSerologyFollowupRequestDto request, Guid doctorUserId, CancellationToken cancellationToken = default)
        {
            var exposure = await _uow.OccupationalExposures
                .FirstOrDefaultAsync(x => x.Id == exposureId, cancellationToken);
            if (exposure == null)
                throw new KeyNotFoundException($"Pajanan kerja dengan ID {exposureId} tidak ditemukan.");

            // Validasi bulan serial: 1, 3, atau 6 (VR-OHS-11)
            if (request.FollowUpMonth != 1 && request.FollowUpMonth != 3 && request.FollowUpMonth != 6)
            {
                throw new ArgumentException("Bulan tindak lanjut surveilans serologi serial wajib pada Bulan 1, 3, atau 6 (VR-OHS-11).");
            }

            // Catat ke Health Surveillance
            var surveillance = new TrxEmployeeHealthSurveillance
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = exposure.WorkforceProfileId,
                EmployeeId = exposure.EmployeeId,
                OccupationalExposureId = exposure.Id,
                SurveillanceNumber = await _numberAllocator.AllocateAsync(new NumberAllocationRequest(
                SequenceKey: OccupationalHealthNumberSeries.SerologyKey,
                Prefix: OccupationalHealthNumberSeries.SerologyPrefix,
                ResetPolicy: OccupationalHealthNumberSeries.SerologyResetPolicy,
                SequenceDigits: OccupationalHealthNumberSeries.SequenceDigits,
                ActorUserId: doctorUserId == Guid.Empty ? OccupationalHealthNumberSeries.SystemFallbackUserId : doctorUserId,
                Instant: DateTimeOffset.UtcNow), cancellationToken),
                SurveillanceProgram = $"Surveilans Serologi Pasca Pajanan Darah Bulan ke-{request.FollowUpMonth}",
                RiskFactor = "Biological Blood-Borne Pathogens (HIV/HBV/HCV)",
                ScheduledDate = request.TestDate,
                CompletedDate = DateTime.UtcNow,
                SurveillanceStatus = "Completed",
                AdministrativeOutcome = $"Hasil Bulan {request.FollowUpMonth}: {request.OutcomeStatus}",
                ClinicalFindingsRestricted = AesClinicalEncryptionHelper.Encrypt($"Hasil Lab Serologi Bulan {request.FollowUpMonth}: {request.OutcomeStatus}. Dokumen: {request.LabDocumentUrl}. Catatan: {request.ClinicalNotesEncrypted}"),
                IsCompliant = true,
                IsActive = true,
                CreateBy = doctorUserId,
                CreateDateTime = DateTime.UtcNow,
                UpdateBy = doctorUserId,
                UpdateDateTime = DateTime.UtcNow
            };

            await _uow.AddAsync(surveillance, cancellationToken);

            // Update status exposure
            if (request.FollowUpMonth == 6 && request.OutcomeStatus.Equals("NonReactive", StringComparison.OrdinalIgnoreCase))
            {
                // Kasus ditutup bebas infeksi (AT-HRD-OHS-11)
                exposure.ExposureStatus = "ClosedSeronegative";
                exposure.FollowUpRequired = false;
                exposure.FollowUpDate = null;
            }
            else if (request.OutcomeStatus.Equals("Reactive", StringComparison.OrdinalIgnoreCase))
            {
                exposure.ExposureStatus = "SeroconversionDetected";
                exposure.FollowUpRequired = true;
            }
            else
            {
                // Set jadwal surveilans berikutnya
                var nextMonth = request.FollowUpMonth == 1 ? 3 : 6;
                exposure.FollowUpDate = exposure.ExposureDateTime.AddMonths(nextMonth);
                exposure.ExposureStatus = "UnderSurveillance";
            }

            exposure.UpdateDateTime = DateTime.UtcNow;
            exposure.UpdateBy = doctorUserId;
            _uow.Update(exposure);

            await _uow.SaveChangesAsync(cancellationToken);

            var emp = exposure.EmployeeId.HasValue 
                ? await _uow.Employees.FirstOrDefaultAsync(x => x.Id == exposure.EmployeeId.Value, cancellationToken)
                : null;

            return new OccupationalExposureDto
            {
                Id = exposure.Id,
                EmployeeId = exposure.EmployeeId ?? Guid.Empty,
                EmployeeName = emp != null ? emp.FullName : "Pegawai RS",
                ExposureNumber = exposure.ExposureNumber,
                ExposureDateTime = exposure.ExposureDateTime,
                ExposureType = exposure.ExposureType,
                RiskLevel = exposure.RiskLevel,
                ExposureStatus = exposure.ExposureStatus,
                FollowUpRequired = exposure.FollowUpRequired,
                FollowUpDate = exposure.FollowUpDate
            };
        }

        public async Task<List<ActiveExposureRadarSummaryDto>> GetActiveExposureRadarAsync(CancellationToken cancellationToken = default)
        {
            var activeExposures = await _uow.OccupationalExposures
                .Where(x => x.ExposureStatus == "UnderSurveillance" || x.ExposureStatus == "PendingPEP")
                .OrderBy(x => x.ExposureDateTime)
                .ToListAsync(cancellationToken);

            var employeeIds = activeExposures.Select(x => x.EmployeeId).Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();
            var employees = await _uow.Employees.Include(x => x.PrimaryDepartment).Where(x => employeeIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, cancellationToken);

            var results = new List<ActiveExposureRadarSummaryDto>();

            foreach (var exp in activeExposures)
            {
                var monthsElapsed = (int)Math.Floor((DateTime.UtcNow - exp.ExposureDateTime).TotalDays / 30.0);
                var nextMilestone = monthsElapsed < 1 ? 1 : (monthsElapsed < 3 ? 3 : 6);
                var scheduledTestDate = exp.ExposureDateTime.AddMonths(nextMilestone);

                employees.TryGetValue(exp.EmployeeId ?? Guid.Empty, out var emp);

                results.Add(new ActiveExposureRadarSummaryDto
                {
                    ExposureId = exp.Id,
                    EmployeeId = exp.EmployeeId ?? Guid.Empty,
                    EmployeeName = emp != null ? emp.FullName : "Pegawai RS",
                    DepartmentName = emp?.PrimaryDepartment?.DepartmentName ?? "-",
                    ExposureDate = exp.ExposureDateTime,
                    MonthsElapsed = monthsElapsed,
                    NextMilestoneMonth = nextMilestone,
                    NextScheduledTestDate = scheduledTestDate,
                    Month1Status = monthsElapsed >= 1 ? "Completed" : "Scheduled",
                    Month3Status = monthsElapsed >= 3 ? "Completed" : (nextMilestone == 3 ? "NextDue" : "Pending"),
                    Month6Status = monthsElapsed >= 6 ? "Completed" : (nextMilestone == 6 ? "NextDue" : "Pending"),
                    OverallStatus = exp.ExposureStatus
                });
            }

            return results;
        }

        public async Task<OccupationalExposureDto> GetExposureDetailAsync(Guid exposureId, CancellationToken cancellationToken = default)
        {
            var exposure = await _uow.OccupationalExposures
                .FirstOrDefaultAsync(x => x.Id == exposureId, cancellationToken);
            if (exposure == null)
                throw new KeyNotFoundException($"Pajanan kerja dengan ID {exposureId} tidak ditemukan.");

            var emp = exposure.EmployeeId.HasValue 
                ? await _uow.Employees.FirstOrDefaultAsync(x => x.Id == exposure.EmployeeId.Value, cancellationToken)
                : null;

            return new OccupationalExposureDto
            {
                Id = exposure.Id,
                EmployeeId = exposure.EmployeeId ?? Guid.Empty,
                EmployeeName = emp != null ? emp.FullName : "Pegawai RS",
                ExposureNumber = exposure.ExposureNumber,
                ExposureDateTime = exposure.ExposureDateTime,
                ExposureType = exposure.ExposureType,
                RiskLevel = exposure.RiskLevel,
                ExposureStatus = exposure.ExposureStatus,
                FollowUpRequired = exposure.FollowUpRequired,
                FollowUpDate = exposure.FollowUpDate
            };
        }
    }
}
