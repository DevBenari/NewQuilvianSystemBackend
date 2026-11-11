using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Services
{
    /// <summary>
    /// Layanan Sidang Pleno Kalibrasi Manajemen & Guiding Distribution (BE-HRD-261, PRF-B6, VR-PRF-05, VR-PRF-12)
    /// </summary>
    public class PerformanceCalibrationService : IPerformanceCalibrationService
    {
        private readonly IPerformanceUnitOfWork _uow;

        public PerformanceCalibrationService(IPerformanceUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task<CalibrationDistributionMatrixDto> GetDistributionMatrixAsync(Guid cycleId, CancellationToken cancellationToken = default)
        {
            var appraisals = await _uow.PerformanceAppraisals
                .Where(x => x.PerformanceCycleId == cycleId)
                .ToListAsync(cancellationToken);

            int total = appraisals.Count;
            if (total == 0)
            {
                return new CalibrationDistributionMatrixDto { TotalStaff = 0, IsWithinGuidingDistribution = true };
            }

            int countA = appraisals.Count(x => x.FinalScore >= 90.0m);
            int countB = appraisals.Count(x => x.FinalScore >= 75.0m && x.FinalScore < 90.0m);
            int countC = appraisals.Count(x => x.FinalScore >= 65.0m && x.FinalScore < 75.0m);
            int countD = appraisals.Count(x => x.FinalScore >= 50.0m && x.FinalScore < 65.0m);
            int countE = appraisals.Count(x => x.FinalScore < 50.0m);

            decimal pctA = Math.Round(((decimal)countA / total) * 100m, 1);
            decimal pctB = Math.Round(((decimal)countB / total) * 100m, 1);
            decimal pctC = Math.Round(((decimal)countC / total) * 100m, 1);
            decimal pctD = Math.Round(((decimal)countD / total) * 100m, 1);
            decimal pctE = Math.Round(((decimal)countE / total) * 100m, 1);

            // Guiding Distribution PRF-B6: A (10-15%), B (70-80%), C/D/E (5-10%)
            bool isWithin = (pctA >= 10m && pctA <= 20m) && (pctB >= 65m && pctB <= 85m);

            return new CalibrationDistributionMatrixDto
            {
                TotalStaff = total,
                CountGradeA = countA,
                PercentageGradeA = pctA,
                CountGradeB = countB,
                PercentageGradeB = pctB,
                CountGradeC = countC,
                PercentageGradeC = pctC,
                CountGradeD = countD,
                PercentageGradeD = pctD,
                CountGradeE = countE,
                PercentageGradeE = pctE,
                IsWithinGuidingDistribution = isWithin
            };
        }

        public async Task<CalibrationSessionDto> CreateSessionAsync(CreateCalibrationSessionRequestDto dto, Guid createdBy, CancellationToken cancellationToken = default)
        {
            var session = new TrxPerformanceCalibrationSession
            {
                Id = Guid.NewGuid(),
                PerformanceCycleId = dto.PerformanceCycleId,
                SessionCode = dto.SessionCode,
                SessionDate = dto.SessionDate,
                CommitteeChairpersonId = dto.CommitteeChairpersonId,
                Status = "SCHEDULED",
                MinutesOfMeeting = dto.Notes,
                CreateBy = createdBy,
                CreateDateTime = DateTime.UtcNow,
                UpdateBy = createdBy,
                UpdateDateTime = DateTime.UtcNow
            };

            await _uow.AddAsync(session, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            return new CalibrationSessionDto
            {
                Id = session.Id,
                PerformanceCycleId = session.PerformanceCycleId,
                SessionCode = session.SessionCode,
                SessionDate = session.SessionDate,
                CommitteeChairpersonId = session.CommitteeChairpersonId,
                Status = session.Status,
                MinutesOfMeeting = session.MinutesOfMeeting
            };
        }

        public async Task<bool> OverrideScoreAsync(OverrideCalibrationScoreRequestDto dto, Guid committeeMemberId, CancellationToken cancellationToken = default)
        {
            // VR-PRF-05, VR-PRF-12: Kewajiban Nota Justifikasi Tertulis Minimal 50 Karakter
            if (string.IsNullOrWhiteSpace(dto.OverrideJustificationNotes) || dto.OverrideJustificationNotes.Trim().Length < 50)
            {
                throw new ArgumentException("Penyesuaian skor kalibrasi wajib menyertakan nota pertimbangan tertulis minimal 50 karakter (VR-PRF-05, VR-PRF-12).");
            }

            var appraisal = await _uow.PerformanceAppraisals.FirstOrDefaultAsync(x => x.Id == dto.AppraisalId, cancellationToken);
            if (appraisal == null)
                throw new KeyNotFoundException($"Lembar evaluasi kinerja ID {dto.AppraisalId} tidak ditemukan.");

            appraisal.CalibratedScore = dto.CalibratedScore;
            appraisal.FinalScore = dto.CalibratedScore;
            appraisal.FinalGrade = DetermineGrade(dto.CalibratedScore);
            appraisal.CalibrationNotes = dto.OverrideJustificationNotes;
            appraisal.Status = "CALIBRATED";
            appraisal.UpdateBy = committeeMemberId;
            appraisal.UpdateDateTime = DateTime.UtcNow;

            _uow.Update(appraisal);
            await _uow.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> FinalizeSessionAsync(Guid sessionId, FinalizeCalibrationSessionRequestDto dto, Guid finalizedBy, CancellationToken cancellationToken = default)
        {
            var session = await _uow.PerformanceCalibrationSessions.FirstOrDefaultAsync(x => x.Id == sessionId, cancellationToken);
            if (session == null)
                throw new KeyNotFoundException($"Sesi kalibrasi ID {sessionId} tidak ditemukan.");

            session.Status = "FINALIZED";
            session.MinutesOfMeeting = dto.MinutesOfMeeting ?? session.MinutesOfMeeting;
            session.UpdateBy = finalizedBy;
            session.UpdateDateTime = DateTime.UtcNow;
            _uow.Update(session);

            if (dto.ReleaseImmediatelyToEss)
            {
                var appraisals = await _uow.PerformanceAppraisals
                    .Where(x => x.PerformanceCycleId == session.PerformanceCycleId)
                    .ToListAsync(cancellationToken);

                foreach (var app in appraisals)
                {
                    app.Status = "RELEASED";
                    app.PublishedDate = DateTime.UtcNow;
                    app.UpdateBy = finalizedBy;
                    app.UpdateDateTime = DateTime.UtcNow;
                    _uow.Update(app);

                    // VR-PRF-08 & PRF-B7: Pemicu Otomatis Program PIP untuk Skor Akhir < 65.00
                    if (app.FinalScore < 65.00m)
                    {
                        var hasPip = await _uow.PerformanceImprovementPlans
                            .AnyAsync(p => p.PerformanceAppraisalId == app.Id, cancellationToken);

                        if (!hasPip)
                        {
                            var pip = new TrxPerformanceImprovementPlan
                            {
                                Id = Guid.NewGuid(),
                                PerformanceAppraisalId = app.Id,
                                WorkforceProfileId = app.WorkforceProfileId,
                                MentorStaffId = app.ManagerId ?? finalizedBy,
                                ImprovementObjectives = "Perbaikan kinerja klinis dan kepatuhan standar mutu RS berdasarkan hasil evaluasi kinerja di bawah ambang batas minimal 65.00.",
                                StartDate = DateTime.UtcNow.Date,
                                EndDate = DateTime.UtcNow.Date.AddDays(60), // PRF-B7: 60 hari kalender
                                Status = "ACTIVE",
                                BaselineScore = app.FinalScore,
                                ExtensionCount = 0,
                                CreateBy = finalizedBy,
                                CreateDateTime = DateTime.UtcNow,
                                UpdateBy = finalizedBy,
                                UpdateDateTime = DateTime.UtcNow
                            };
                            await _uow.AddAsync(pip, cancellationToken);
                        }
                    }
                }
            }

            await _uow.SaveChangesAsync(cancellationToken);
            return true;
        }

        private static string DetermineGrade(decimal score)
        {
            if (score >= 90.0m) return "SANGAT_BAIK";
            if (score >= 75.0m) return "BAIK";
            if (score >= 65.0m) return "CUKUP";
            if (score >= 50.0m) return "KURANG";
            return "SANGAT_KURANG";
        }
    }
}
