using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Models;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Services
{
    public class TrainingAttendanceService : ITrainingAttendanceService
    {
        private readonly ApplicationDbContext _context;

        public TrainingAttendanceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TrainingAttendanceDto> RecordCheckInAsync(Guid sessionId, RecordAttendanceCheckInRequestDto request, string currentUserId)
        {
            var session = await _context.TrxTrainingSessions
                .FirstOrDefaultAsync(s => s.Id == sessionId && !s.IsDelete);

            if (session == null)
            {
                throw new KeyNotFoundException($"Sesi pelatihan dengan ID '{sessionId}' tidak ditemukan.");
            }

            var parsedUserGuid = Guid.TryParse(currentUserId, out var uGuid) ? uGuid : Guid.Empty;

            var participant = await _context.TrxTrainingParticipants
                .Include(p => p.WorkforceProfile)
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
                    IsMandatory = false,
                    IsActive = true,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = parsedUserGuid
                };
                await _context.TrxTrainingParticipants.AddAsync(participant);
            }

            var attDate = request.AttendanceDate?.Date ?? DateTime.UtcNow.Date;
            var checkInTime = request.CheckInTime ?? DateTime.UtcNow;

            var attendance = await _context.TrxTrainingAttendances
                .FirstOrDefaultAsync(a => a.TrainingSessionId == sessionId &&
                                          a.WorkforceProfileId == request.WorkforceProfileId &&
                                          a.AttendanceDate.Date == attDate &&
                                          !a.IsDelete);

            if (attendance == null)
            {
                attendance = new TrxTrainingAttendance
                {
                    Id = Guid.NewGuid(),
                    TrainingSessionId = sessionId,
                    TrainingParticipantId = participant.Id,
                    WorkforceProfileId = request.WorkforceProfileId,
                    AttendanceDate = attDate,
                    CheckInAt = checkInTime,
                    ScheduledMinutes = request.ScheduledMinutes > 0 ? request.ScheduledMinutes : 480,
                    AttendedMinutes = 0,
                    AttendanceStatus = "Present",
                    AttendanceSource = string.IsNullOrWhiteSpace(request.AttendanceSource) ? "QRCode" : request.AttendanceSource,
                    ProofPath = request.ProofPath,
                    Notes = request.Notes,
                    IsActive = true,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = parsedUserGuid
                };
                await _context.TrxTrainingAttendances.AddAsync(attendance);
            }
            else
            {
                attendance.CheckInAt = checkInTime;
                attendance.AttendanceStatus = "Present";
                if (!string.IsNullOrWhiteSpace(request.AttendanceSource))
                {
                    attendance.AttendanceSource = request.AttendanceSource;
                }
                if (!string.IsNullOrWhiteSpace(request.ProofPath))
                {
                    attendance.ProofPath = request.ProofPath;
                }
                if (!string.IsNullOrWhiteSpace(request.Notes))
                {
                    attendance.Notes = request.Notes;
                }
                attendance.UpdateDateTime = DateTime.UtcNow;
                attendance.UpdateBy = parsedUserGuid;
            }

            await _context.SaveChangesAsync();

            var staff = participant.WorkforceProfile ?? await _context.MstWorkforceProfiles.FindAsync(request.WorkforceProfileId);

            return new TrainingAttendanceDto
            {
                Id = attendance.Id,
                TrainingSessionId = attendance.TrainingSessionId,
                WorkforceProfileId = attendance.WorkforceProfileId,
                StaffName = staff?.DisplayName,
                AttendanceDate = attendance.AttendanceDate,
                CheckInAt = attendance.CheckInAt,
                CheckOutAt = attendance.CheckOutAt,
                ScheduledMinutes = attendance.ScheduledMinutes,
                AttendedMinutes = attendance.AttendedMinutes,
                AttendanceStatus = attendance.AttendanceStatus,
                AttendanceSource = attendance.AttendanceSource,
                ProofPath = attendance.ProofPath,
                Notes = attendance.Notes,
                IsActive = attendance.IsActive,
                CreatedAt = attendance.CreateDateTime
            };
        }

        public async Task<TrainingAttendanceDto> RecordCheckOutAsync(Guid sessionId, RecordAttendanceCheckOutRequestDto request, string currentUserId)
        {
            var session = await _context.TrxTrainingSessions
                .FirstOrDefaultAsync(s => s.Id == sessionId && !s.IsDelete);

            if (session == null)
            {
                throw new KeyNotFoundException($"Sesi pelatihan dengan ID '{sessionId}' tidak ditemukan.");
            }

            var parsedUserGuid = Guid.TryParse(currentUserId, out var uGuid) ? uGuid : Guid.Empty;
            var checkOutTime = request.CheckOutTime ?? DateTime.UtcNow;

            var attendance = await _context.TrxTrainingAttendances
                .Include(a => a.WorkforceProfile)
                .OrderByDescending(a => a.AttendanceDate)
                .FirstOrDefaultAsync(a => a.TrainingSessionId == sessionId &&
                                          a.WorkforceProfileId == request.WorkforceProfileId &&
                                          !a.IsDelete);

            if (attendance == null)
            {
                throw new InvalidOperationException("Catatan Check-In untuk peserta ini tidak ditemukan pada sesi terkait.");
            }

            attendance.CheckOutAt = checkOutTime;
            if (request.AttendedMinutes.HasValue && request.AttendedMinutes.Value > 0)
            {
                attendance.AttendedMinutes = request.AttendedMinutes.Value;
            }
            else if (attendance.CheckInAt.HasValue)
            {
                var duration = (int)(checkOutTime - attendance.CheckInAt.Value).TotalMinutes;
                attendance.AttendedMinutes = duration > 0 ? duration : 0;
            }

            if (!string.IsNullOrWhiteSpace(request.Notes))
            {
                attendance.Notes = string.IsNullOrWhiteSpace(attendance.Notes)
                    ? request.Notes
                    : $"{attendance.Notes}; CheckOut: {request.Notes}";
            }

            attendance.UpdateDateTime = DateTime.UtcNow;
            attendance.UpdateBy = parsedUserGuid;

            await _context.SaveChangesAsync();

            // Hitung ulang total persentase kehadiran peserta
            await RecalculateParticipantAttendanceAsync(sessionId, request.WorkforceProfileId);

            return new TrainingAttendanceDto
            {
                Id = attendance.Id,
                TrainingSessionId = attendance.TrainingSessionId,
                WorkforceProfileId = attendance.WorkforceProfileId,
                StaffName = attendance.WorkforceProfile?.DisplayName,
                AttendanceDate = attendance.AttendanceDate,
                CheckInAt = attendance.CheckInAt,
                CheckOutAt = attendance.CheckOutAt,
                ScheduledMinutes = attendance.ScheduledMinutes,
                AttendedMinutes = attendance.AttendedMinutes,
                AttendanceStatus = attendance.AttendanceStatus,
                AttendanceSource = attendance.AttendanceSource,
                ProofPath = attendance.ProofPath,
                Notes = attendance.Notes,
                IsActive = attendance.IsActive,
                CreatedAt = attendance.CreateDateTime
            };
        }

        public async Task<List<TrainingAttendanceDto>> GetSessionAttendancesAsync(Guid sessionId)
        {
            var list = await _context.TrxTrainingAttendances
                .Include(a => a.WorkforceProfile)
                .Where(a => a.TrainingSessionId == sessionId && !a.IsDelete)
                .OrderByDescending(a => a.AttendanceDate)
                .ThenBy(a => a.WorkforceProfile != null ? a.WorkforceProfile.DisplayName : string.Empty)
                .ToListAsync();

            return list.Select(a => new TrainingAttendanceDto
            {
                Id = a.Id,
                TrainingSessionId = a.TrainingSessionId,
                WorkforceProfileId = a.WorkforceProfileId,
                StaffName = a.WorkforceProfile?.DisplayName,
                AttendanceDate = a.AttendanceDate,
                CheckInAt = a.CheckInAt,
                CheckOutAt = a.CheckOutAt,
                ScheduledMinutes = a.ScheduledMinutes,
                AttendedMinutes = a.AttendedMinutes,
                AttendanceStatus = a.AttendanceStatus,
                AttendanceSource = a.AttendanceSource,
                ProofPath = a.ProofPath,
                Notes = a.Notes,
                IsActive = a.IsActive,
                CreatedAt = a.CreateDateTime
            }).ToList();
        }

        public async Task<ParticipantAttendanceEligibilityDto> EvaluateEligibilityAsync(Guid sessionId, Guid workforceProfileId)
        {
            var session = await _context.TrxTrainingSessions
                .FirstOrDefaultAsync(s => s.Id == sessionId && !s.IsDelete);

            if (session == null)
            {
                throw new KeyNotFoundException($"Sesi pelatihan dengan ID '{sessionId}' tidak ditemukan.");
            }

            var staff = await _context.MstWorkforceProfiles
                .Include(w => w.PrimaryPosition)
                .FirstOrDefaultAsync(w => w.Id == workforceProfileId && !w.IsDelete);

            if (staff == null)
            {
                throw new KeyNotFoundException($"Profil tenaga kerja ID '{workforceProfileId}' tidak ditemukan.");
            }

            var attendances = await _context.TrxTrainingAttendances
                .Where(a => a.TrainingSessionId == sessionId && a.WorkforceProfileId == workforceProfileId && !a.IsDelete)
                .ToListAsync();

            var posName = staff.PrimaryPosition?.PositionName ?? string.Empty;
            bool isClinical = staff.UserType == UserType.PermanentDoctor ||
                              staff.UserType == UserType.GuestDoctor ||
                              posName.Contains("Klinis", StringComparison.OrdinalIgnoreCase) ||
                              posName.Contains("Medis", StringComparison.OrdinalIgnoreCase) ||
                              posName.Contains("Dokter", StringComparison.OrdinalIgnoreCase) ||
                              posName.Contains("Perawat", StringComparison.OrdinalIgnoreCase) ||
                              posName.Contains("Bidan", StringComparison.OrdinalIgnoreCase) ||
                              posName.Contains("Clinical", StringComparison.OrdinalIgnoreCase);

            var totalScheduledMinutes = attendances.Sum(a => a.ScheduledMinutes);
            var totalAttendedMinutes = attendances.Sum(a => a.AttendedMinutes);

            if (totalScheduledMinutes == 0)
            {
                var durationHours = (session.EndDateTime - session.StartDateTime).TotalHours;
                totalScheduledMinutes = durationHours > 0 ? (int)(durationHours * 60) : 480;
            }

            decimal actualPercentage = 0m;
            if (totalScheduledMinutes > 0)
            {
                actualPercentage = Math.Round(((decimal)totalAttendedMinutes / (decimal)totalScheduledMinutes) * 100m, 2);
            }
            else if (attendances.Any(a => a.AttendanceStatus == "Present"))
            {
                actualPercentage = 100m;
            }

            decimal requiredPercentage = isClinical ? 100.00m : 80.00m;
            bool isEligible = actualPercentage >= requiredPercentage;

            string statusMsg = isEligible
                ? $"Memenuhi syarat ambang presensi {(isClinical ? "staf klinis (wajib 100%)" : "staf non-klinis (min 80%)")} dengan pencapaian {actualPercentage}%."
                : $"TIDAK memenuhi syarat ambang presensi {(isClinical ? "staf klinis (wajib 100%)" : "staf non-klinis (min 80%)")}. Pencapaian saat ini {actualPercentage}% (kurang dari {requiredPercentage}%). Akses ujian / OSCE diblokir.";

            return new ParticipantAttendanceEligibilityDto
            {
                WorkforceProfileId = staff.Id,
                StaffName = staff.DisplayName,
                IsClinicalStaff = isClinical,
                TotalSessionDays = attendances.Count > 0 ? attendances.Select(a => a.AttendanceDate.Date).Distinct().Count() : 1,
                AttendedDays = attendances.Count(a => a.AttendanceStatus == "Present"),
                TotalScheduledMinutes = totalScheduledMinutes,
                TotalAttendedMinutes = totalAttendedMinutes,
                ActualAttendancePercentage = actualPercentage,
                IsEligibleForExam = isEligible,
                StatusMessage = statusMsg
            };
        }

        public async Task<List<ParticipantAttendanceEligibilityDto>> EvaluateSessionAllEligibilitiesAsync(Guid sessionId)
        {
            var participantStaffIds = await _context.TrxTrainingParticipants
                .Where(p => p.TrainingSessionId == sessionId && !p.IsDelete)
                .Select(p => p.WorkforceProfileId)
                .Distinct()
                .ToListAsync();

            var results = new List<ParticipantAttendanceEligibilityDto>();
            foreach (var staffId in participantStaffIds)
            {
                var eligibility = await EvaluateEligibilityAsync(sessionId, staffId);
                results.Add(eligibility);
            }

            return results;
        }

        private async Task RecalculateParticipantAttendanceAsync(Guid sessionId, Guid workforceProfileId)
        {
            var participant = await _context.TrxTrainingParticipants
                .FirstOrDefaultAsync(p => p.TrainingSessionId == sessionId && p.WorkforceProfileId == workforceProfileId && !p.IsDelete);

            if (participant == null) return;

            var attendances = await _context.TrxTrainingAttendances
                .Where(a => a.TrainingSessionId == sessionId && a.WorkforceProfileId == workforceProfileId && !a.IsDelete)
                .ToListAsync();

            var totalScheduled = attendances.Sum(a => a.ScheduledMinutes);
            var totalAttended = attendances.Sum(a => a.AttendedMinutes);

            if (totalScheduled > 0)
            {
                participant.AttendancePercentage = Math.Round(((decimal)totalAttended / (decimal)totalScheduled) * 100m, 2);
            }
            else if (attendances.Any(a => a.AttendanceStatus == "Present"))
            {
                participant.AttendancePercentage = 100m;
            }

            participant.UpdateDateTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
