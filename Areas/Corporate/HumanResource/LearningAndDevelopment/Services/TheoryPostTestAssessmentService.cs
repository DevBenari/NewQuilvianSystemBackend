using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Services
{
    public class TheoryPostTestAssessmentService : ITheoryPostTestAssessmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITrainingAttendanceService _attendanceService;

        public TheoryPostTestAssessmentService(ApplicationDbContext context, ITrainingAttendanceService attendanceService)
        {
            _context = context;
            _attendanceService = attendanceService;
        }

        public async Task<TheoryPostTestResultDto> SubmitPostTestAsync(Guid sessionId, SubmitTheoryPostTestRequestDto request, string currentUserId)
        {
            var session = await _context.TrxTrainingSessions
                .FirstOrDefaultAsync(s => s.Id == sessionId && !s.IsDelete);

            if (session == null)
            {
                throw new KeyNotFoundException($"Sesi pelatihan dengan ID '{sessionId}' tidak ditemukan.");
            }

            var staff = await _context.MstWorkforceProfiles
                .FirstOrDefaultAsync(w => w.Id == request.WorkforceProfileId && !w.IsDelete);

            if (staff == null)
            {
                throw new KeyNotFoundException($"Profil staf ID '{request.WorkforceProfileId}' tidak ditemukan.");
            }

            var parsedUserGuid = Guid.TryParse(currentUserId, out var uGuid) ? uGuid : Guid.Empty;

            // Validasi kelayakan presensi kehadiran (VR-TRN-03)
            var eligibility = await _attendanceService.EvaluateEligibilityAsync(sessionId, request.WorkforceProfileId);
            if (!eligibility.IsEligibleForExam)
            {
                throw new InvalidOperationException($"Peserta {staff.DisplayName} tidak memenuhi ambang batas kehadiran ({eligibility.ActualAttendancePercentage}% < {eligibility.RequiredAttendancePercentage}%). Akses ujian post-test teori diblokir.");
            }

            var participant = await _context.TrxTrainingParticipants
                .FirstOrDefaultAsync(p => p.TrainingSessionId == sessionId && p.WorkforceProfileId == request.WorkforceProfileId && !p.IsDelete);

            if (participant == null)
            {
                participant = new TrxTrainingParticipant
                {
                    Id = Guid.NewGuid(),
                    TrainingPlanId = session.TrainingPlanId,
                    TrainingSessionId = sessionId,
                    WorkforceProfileId = request.WorkforceProfileId,
                    ParticipantStatus = "Attending",
                    EnrollmentDate = DateTime.UtcNow,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = parsedUserGuid
                };
                await _context.TrxTrainingParticipants.AddAsync(participant);
            }

            var previousAttempts = await _context.TrxTrainingAssessments
                .Where(a => a.TrainingSessionId == sessionId &&
                            a.WorkforceProfileId == request.WorkforceProfileId &&
                            a.AssessmentType == "PostTest" &&
                            !a.IsDelete)
                .OrderBy(a => a.AttemptNumber)
                .ToListAsync();

            if (previousAttempts.Any(a => a.IsPassed))
            {
                throw new InvalidOperationException($"Peserta {staff.DisplayName} telah dinyatakan LULUS pada ujian post-test sebelumnya. Tidak diperkenankan ujian ulang.");
            }

            int currentAttempt = previousAttempts.Count + 1;
            if (currentAttempt > 3)
            {
                throw new InvalidOperationException($"Batas maksimal remedial (2 kali remedial / total 3 kali percobaan) telah habis (VR-TRN-20). Peserta dinyatakan GUGUR dan wajib mengulang kelas pelatihan dari awal.");
            }

            decimal passingScore = request.PassingScore > 0 ? request.PassingScore : 75.00m;
            bool isPassed = request.Score >= passingScore;
            int remainingRemedials = Math.Max(0, 3 - currentAttempt);

            string resultStatus;
            if (isPassed)
            {
                resultStatus = currentAttempt == 1 ? "PASSED_FIRST_ATTEMPT" : $"PASSED_REMEDIAL_{currentAttempt - 1}";
            }
            else
            {
                resultStatus = remainingRemedials > 0
                    ? $"FAILED_ELIGIBLE_FOR_REMEDIAL (Sisa remedial: {remainingRemedials})"
                    : "FAILED_PERMANENT_REMEDIAL_EXHAUSTED";
            }

            var assessment = new TrxTrainingAssessment
            {
                Id = Guid.NewGuid(),
                TrainingSessionId = sessionId,
                TrainingParticipantId = participant.Id,
                WorkforceProfileId = request.WorkforceProfileId,
                CompetencyId = request.CompetencyId,
                AssessmentType = "PostTest",
                AttemptNumber = currentAttempt,
                StartedAt = request.StartedAt ?? DateTime.UtcNow.AddMinutes(-60),
                SubmittedAt = request.SubmittedAt ?? DateTime.UtcNow,
                Score = request.Score,
                MaximumScore = 100.00m,
                PassingScore = passingScore,
                IsPassed = isPassed,
                AnswerSnapshotJson = request.AnswerSnapshotJson,
                AssessmentResultJson = request.AssessmentResultJson,
                AssessedByUserId = parsedUserGuid,
                Notes = request.Notes ?? resultStatus,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = parsedUserGuid
            };

            await _context.TrxTrainingAssessments.AddAsync(assessment);

            if (isPassed)
            {
                participant.ParticipantStatus = "PassedExam";
                participant.FinalScore = request.Score;
                participant.UpdateDateTime = DateTime.UtcNow;
                participant.UpdateBy = parsedUserGuid;
            }
            else if (remainingRemedials == 0)
            {
                participant.ParticipantStatus = "Failed";
                participant.FinalScore = request.Score;
                participant.UpdateDateTime = DateTime.UtcNow;
                participant.UpdateBy = parsedUserGuid;
            }

            await _context.SaveChangesAsync();

            return new TheoryPostTestResultDto
            {
                Id = assessment.Id,
                TrainingSessionId = assessment.TrainingSessionId,
                WorkforceProfileId = assessment.WorkforceProfileId,
                StaffName = staff.DisplayName,
                CompetencyId = assessment.CompetencyId,
                AssessmentType = assessment.AssessmentType,
                AttemptNumber = assessment.AttemptNumber,
                Score = assessment.Score,
                MaximumScore = assessment.MaximumScore,
                PassingScore = assessment.PassingScore,
                IsPassed = assessment.IsPassed,
                RemainingRemedialAttempts = remainingRemedials,
                ResultStatus = resultStatus,
                SubmittedAt = assessment.SubmittedAt,
                Notes = assessment.Notes,
                CreatedAt = assessment.CreateDateTime
            };
        }

        public async Task<ParticipantAssessmentSummaryDto> GetParticipantAssessmentSummaryAsync(Guid sessionId, Guid workforceProfileId)
        {
            var staff = await _context.MstWorkforceProfiles
                .FirstOrDefaultAsync(w => w.Id == workforceProfileId && !w.IsDelete);

            if (staff == null)
            {
                throw new KeyNotFoundException($"Profil staf ID '{workforceProfileId}' tidak ditemukan.");
            }

            var attempts = await _context.TrxTrainingAssessments
                .Include(a => a.Competency)
                .Where(a => a.TrainingSessionId == sessionId &&
                            a.WorkforceProfileId == workforceProfileId &&
                            a.AssessmentType == "PostTest" &&
                            !a.IsDelete)
                .OrderBy(a => a.AttemptNumber)
                .ToListAsync();

            var totalAttempts = attempts.Count;
            var highestScore = attempts.Count > 0 ? attempts.Max(a => a.Score) : 0m;
            var latestScore = attempts.Count > 0 ? attempts.Last().Score : 0m;
            var isPassed = attempts.Any(a => a.IsPassed);
            var remaining = Math.Max(0, 3 - totalAttempts);
            var canRemedial = !isPassed && remaining > 0;

            string verdict;
            if (isPassed)
            {
                verdict = "LULUS Ujian Post-Test Teori";
            }
            else if (canRemedial)
            {
                verdict = $"Belum Lulus (Passing Grade 75.00). Berhak mengikuti remedial (Sisa {remaining} kali).";
            }
            else if (totalAttempts >= 3)
            {
                verdict = "GUGUR PERMANEN. Kuota remedial telah habis (3x percobaan).";
            }
            else
            {
                verdict = "Belum Mengikuti Ujian Post-Test Teori.";
            }

            return new ParticipantAssessmentSummaryDto
            {
                WorkforceProfileId = staff.Id,
                StaffName = staff.DisplayName,
                TrainingSessionId = sessionId,
                TotalAttempts = totalAttempts,
                HighestScore = highestScore,
                LatestScore = latestScore,
                IsPassed = isPassed,
                CanRemedial = canRemedial,
                RemainingRemedials = remaining,
                FinalVerdict = verdict,
                History = attempts.Select(a => new TheoryPostTestResultDto
                {
                    Id = a.Id,
                    TrainingSessionId = a.TrainingSessionId,
                    WorkforceProfileId = a.WorkforceProfileId,
                    StaffName = staff.DisplayName,
                    CompetencyId = a.CompetencyId,
                    CompetencyName = a.Competency?.CompetencyName,
                    AssessmentType = a.AssessmentType,
                    AttemptNumber = a.AttemptNumber,
                    Score = a.Score,
                    MaximumScore = a.MaximumScore,
                    PassingScore = a.PassingScore,
                    IsPassed = a.IsPassed,
                    RemainingRemedialAttempts = Math.Max(0, 3 - a.AttemptNumber),
                    ResultStatus = a.IsPassed ? "Lulus" : "Tidak Lulus",
                    SubmittedAt = a.SubmittedAt,
                    Notes = a.Notes,
                    CreatedAt = a.CreateDateTime
                }).ToList()
            };
        }

        public async Task<List<TheoryPostTestResultDto>> GetSessionPostTestsAsync(Guid sessionId)
        {
            var assessments = await _context.TrxTrainingAssessments
                .Include(a => a.WorkforceProfile)
                .Include(a => a.Competency)
                .Where(a => a.TrainingSessionId == sessionId && a.AssessmentType == "PostTest" && !a.IsDelete)
                .OrderByDescending(a => a.CreateDateTime)
                .ToListAsync();

            return assessments.Select(a => new TheoryPostTestResultDto
            {
                Id = a.Id,
                TrainingSessionId = a.TrainingSessionId,
                WorkforceProfileId = a.WorkforceProfileId,
                StaffName = a.WorkforceProfile?.DisplayName,
                CompetencyId = a.CompetencyId,
                CompetencyName = a.Competency?.CompetencyName,
                AssessmentType = a.AssessmentType,
                AttemptNumber = a.AttemptNumber,
                Score = a.Score,
                MaximumScore = a.MaximumScore,
                PassingScore = a.PassingScore,
                IsPassed = a.IsPassed,
                RemainingRemedialAttempts = Math.Max(0, 3 - a.AttemptNumber),
                ResultStatus = a.IsPassed ? "Lulus" : "Tidak Lulus",
                SubmittedAt = a.SubmittedAt,
                Notes = a.Notes,
                CreatedAt = a.CreateDateTime
            }).ToList();
        }
    }
}
