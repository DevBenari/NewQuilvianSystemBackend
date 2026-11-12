using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Services
{
    public class TrainingSessionService : ITrainingSessionService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TrainingSessionService> _logger;

        public TrainingSessionService(
            ApplicationDbContext context,
            ILogger<TrainingSessionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<TrainingSessionDto>> GetSessionsAsync(
            Guid? trainingPlanId,
            string? status,
            DateTime? fromDate,
            DateTime? toDate,
            CancellationToken cancellationToken = default)
        {
            var query = _context.TrxTrainingSessions
                .AsNoTracking()
                .Include(s => s.TrainingPlan)
                .Include(s => s.EnrollmentRequests)
                .Where(s => !s.IsDelete);

            if (trainingPlanId.HasValue)
            {
                query = query.Where(s => s.TrainingPlanId == trainingPlanId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(s => s.SessionStatus == status);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(s => s.StartDateTime >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(s => s.EndDateTime <= toDate.Value);
            }

            var sessions = await query
                .OrderBy(s => s.StartDateTime)
                .ToListAsync(cancellationToken);

            return sessions.Select(MapToDto).ToList();
        }

        public async Task<TrainingSessionDto?> GetSessionByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var session = await _context.TrxTrainingSessions
                .AsNoTracking()
                .Include(s => s.TrainingPlan)
                .Include(s => s.EnrollmentRequests)
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDelete, cancellationToken);

            return session == null ? null : MapToDto(session);
        }

        public async Task<TrainingSessionDto> CreateSessionAsync(
            CreateTrainingSessionRequestDto request,
            string createdBy,
            CancellationToken cancellationToken = default)
        {
            if (request.EndDateTime <= request.StartDateTime)
            {
                throw new ArgumentException("Waktu berakhir sesi harus lebih besar dari waktu mulai.");
            }

            if (request.Capacity <= 0)
            {
                throw new ArgumentException("Kapasitas kuota peserta minimal 1 orang (VR-TRN-07).");
            }

            // Validasi keberadaan Training Plan
            var plan = await _context.TrxTrainingPlans
                .FirstOrDefaultAsync(p => p.Id == request.TrainingPlanId && !p.IsDelete, cancellationToken);

            if (plan == null)
            {
                throw new KeyNotFoundException($"Training Plan dengan ID {request.TrainingPlanId} tidak ditemukan.");
            }

            // Validasi Pencegahan Bentrok Jadwal (VR-TRN-08)
            await ValidateScheduleConflictAsync(null, request.StartDateTime, request.EndDateTime, request.Venue, request.InstructorUserId, cancellationToken);

            // Generate nomor kode sesi baku: SES-{PlanYear}-B{Count+1}
            string sessionCode = request.SessionCode?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(sessionCode))
            {
                var sessionCountInPlan = await _context.TrxTrainingSessions
                    .IgnoreQueryFilters()
                    .CountAsync(s => s.TrainingPlanId == request.TrainingPlanId, cancellationToken);

                sessionCode = $"SES-{plan.PlanYear}-B{(sessionCountInPlan + 1)}";
            }

            var entity = new TrxTrainingSession
            {
                Id = Guid.NewGuid(),
                TrainingPlanId = request.TrainingPlanId,
                SessionCode = sessionCode,
                SessionName = request.SessionName.Trim(),
                Sequence = request.Sequence > 0 ? request.Sequence : 1,
                StartDateTime = request.StartDateTime,
                EndDateTime = request.EndDateTime,
                RegistrationOpenAt = request.RegistrationOpenAt ?? DateTime.UtcNow,
                RegistrationCloseAt = request.RegistrationCloseAt ?? request.StartDateTime.AddHours(-2),
                DeliveryMode = string.IsNullOrWhiteSpace(request.DeliveryMode) ? "Classroom" : request.DeliveryMode,
                Venue = request.Venue,
                MeetingUrl = request.MeetingUrl,
                InstructorName = request.InstructorName,
                InstructorUserId = request.InstructorUserId,
                Capacity = request.Capacity,
                MinimumParticipant = request.MinimumParticipant,
                SessionStatus = "RegistrationOpen",
                RequiresAttendance = request.RequiresAttendance,
                RequiresPreTest = request.RequiresPreTest,
                RequiresPostTest = request.RequiresPostTest,
                GeneratesCertificate = request.GeneratesCertificate,
                MinimumPassingScore = request.MinimumPassingScore > 0 ? request.MinimumPassingScore : 75.0m,
                AgendaJson = request.AgendaJson,
                Notes = request.Notes,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow
            };

            _context.TrxTrainingSessions.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Sesi pelatihan baru dibuat: {SessionCode} - {SessionName} oleh {CreatedBy}",
                entity.SessionCode, entity.SessionName, createdBy);

            return MapToDto(entity);
        }

        public async Task<TrainingSessionDto> UpdateSessionAsync(
            Guid id,
            UpdateTrainingSessionRequestDto request,
            string updatedBy,
            CancellationToken cancellationToken = default)
        {
            var session = await _context.TrxTrainingSessions
                .Include(s => s.TrainingPlan)
                .Include(s => s.EnrollmentRequests)
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDelete, cancellationToken);

            if (session == null)
            {
                throw new KeyNotFoundException($"Sesi pelatihan dengan ID {id} tidak ditemukan.");
            }

            if (session.SessionStatus == "Completed" || session.SessionStatus == "Cancelled")
            {
                throw new InvalidOperationException($"Sesi pelatihan berstatus {session.SessionStatus} tidak dapat diubah.");
            }

            if (request.EndDateTime <= request.StartDateTime)
            {
                throw new ArgumentException("Waktu berakhir sesi harus lebih besar dari waktu mulai.");
            }

            if (request.Capacity <= 0)
            {
                throw new ArgumentException("Kapasitas kuota peserta minimal 1 orang (VR-TRN-07).");
            }

            // Validasi Pencegahan Bentrok Jadwal (VR-TRN-08)
            await ValidateScheduleConflictAsync(id, request.StartDateTime, request.EndDateTime, request.Venue, request.InstructorUserId, cancellationToken);

            session.SessionName = request.SessionName.Trim();
            session.StartDateTime = request.StartDateTime;
            session.EndDateTime = request.EndDateTime;
            session.RegistrationOpenAt = request.RegistrationOpenAt ?? session.RegistrationOpenAt;
            session.RegistrationCloseAt = request.RegistrationCloseAt ?? session.RegistrationCloseAt;
            session.DeliveryMode = request.DeliveryMode;
            session.Venue = request.Venue;
            session.MeetingUrl = request.MeetingUrl;
            session.InstructorName = request.InstructorName;
            session.InstructorUserId = request.InstructorUserId;
            session.Capacity = request.Capacity;
            session.MinimumParticipant = request.MinimumParticipant;
            session.RequiresAttendance = request.RequiresAttendance;
            session.RequiresPreTest = request.RequiresPreTest;
            session.RequiresPostTest = request.RequiresPostTest;
            session.GeneratesCertificate = request.GeneratesCertificate;
            session.MinimumPassingScore = request.MinimumPassingScore;
            session.AgendaJson = request.AgendaJson ?? session.AgendaJson;
            session.Notes = request.Notes ?? session.Notes;
            session.UpdateDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return MapToDto(session);
        }

        public async Task<TrainingSessionDto> OpenRegistrationAsync(Guid id, string updatedBy, CancellationToken cancellationToken = default)
        {
            var session = await _context.TrxTrainingSessions
                .Include(s => s.TrainingPlan)
                .Include(s => s.EnrollmentRequests)
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDelete, cancellationToken);

            if (session == null)
            {
                throw new KeyNotFoundException($"Sesi pelatihan dengan ID {id} tidak ditemukan.");
            }

            session.SessionStatus = "RegistrationOpen";
            session.RegistrationOpenAt = DateTime.UtcNow;
            session.UpdateDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return MapToDto(session);
        }

        public async Task<TrainingSessionDto> CloseRegistrationAsync(Guid id, string updatedBy, CancellationToken cancellationToken = default)
        {
            var session = await _context.TrxTrainingSessions
                .Include(s => s.TrainingPlan)
                .Include(s => s.EnrollmentRequests)
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDelete, cancellationToken);

            if (session == null)
            {
                throw new KeyNotFoundException($"Sesi pelatihan dengan ID {id} tidak ditemukan.");
            }

            session.SessionStatus = "RegistrationClosed";
            session.RegistrationCloseAt = DateTime.UtcNow;
            session.UpdateDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return MapToDto(session);
        }

        public async Task<TrainingSessionDto> CompleteSessionAsync(Guid id, string updatedBy, CancellationToken cancellationToken = default)
        {
            var session = await _context.TrxTrainingSessions
                .Include(s => s.TrainingPlan)
                .Include(s => s.EnrollmentRequests)
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDelete, cancellationToken);

            if (session == null)
            {
                throw new KeyNotFoundException($"Sesi pelatihan dengan ID {id} tidak ditemukan.");
            }

            session.SessionStatus = "Completed";
            session.UpdateDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return MapToDto(session);
        }

        public async Task<bool> DeleteSessionAsync(Guid id, string deletedBy, CancellationToken cancellationToken = default)
        {
            var session = await _context.TrxTrainingSessions
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDelete, cancellationToken);

            if (session == null) return false;

            if (session.SessionStatus == "InProgress" || session.SessionStatus == "Completed")
            {
                throw new InvalidOperationException($"Sesi pelatihan berstatus {session.SessionStatus} tidak dapat dihapus.");
            }

            session.IsDelete = true;
            session.DeleteDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        private async Task ValidateScheduleConflictAsync(
            Guid? excludeSessionId,
            DateTime start,
            DateTime end,
            string? venue,
            Guid? instructorUserId,
            CancellationToken cancellationToken)
        {
            // Cek konflik ruang venue
            if (!string.IsNullOrWhiteSpace(venue))
            {
                var venueConflict = await _context.TrxTrainingSessions
                    .AsNoTracking()
                    .AnyAsync(s =>
                        (!excludeSessionId.HasValue || s.Id != excludeSessionId.Value) &&
                        !s.IsDelete &&
                        s.SessionStatus != "Cancelled" &&
                        s.Venue != null &&
                        s.Venue.ToLower() == venue.Trim().ToLower() &&
                        start < s.EndDateTime &&
                        end > s.StartDateTime,
                        cancellationToken);

                if (venueConflict)
                {
                    throw new InvalidOperationException(
                        $"Jadwal sesi pelatihan bentrok dengan sesi lain di lokasi/ruang '{venue}' pada rentang waktu yang sama (VR-TRN-08).");
                }
            }

            // Cek konflik instruktur
            if (instructorUserId.HasValue)
            {
                var instructorConflict = await _context.TrxTrainingSessions
                    .AsNoTracking()
                    .AnyAsync(s =>
                        (!excludeSessionId.HasValue || s.Id != excludeSessionId.Value) &&
                        !s.IsDelete &&
                        s.SessionStatus != "Cancelled" &&
                        s.InstructorUserId == instructorUserId.Value &&
                        start < s.EndDateTime &&
                        end > s.StartDateTime,
                        cancellationToken);

                if (instructorConflict)
                {
                    throw new InvalidOperationException(
                        "Jadwal sesi pelatihan bentrok dengan penugasan mengajar instruktur pada sesi lain (VR-TRN-08).");
                }
            }
        }

        private static TrainingSessionDto MapToDto(TrxTrainingSession entity)
        {
            var enrolledCount = entity.EnrollmentRequests?
                .Count(r => !r.IsDelete && (r.RequestStatus == "Approved" || r.RequestStatus == "PendingDiklat" || r.RequestStatus == "PendingSupervisor")) ?? 0;

            return new TrainingSessionDto
            {
                Id = entity.Id,
                TrainingPlanId = entity.TrainingPlanId,
                TrainingPlanName = entity.TrainingPlan?.TrainingPlanName,
                SessionCode = entity.SessionCode,
                SessionName = entity.SessionName,
                Sequence = entity.Sequence,
                StartDateTime = entity.StartDateTime,
                EndDateTime = entity.EndDateTime,
                RegistrationOpenAt = entity.RegistrationOpenAt,
                RegistrationCloseAt = entity.RegistrationCloseAt,
                DeliveryMode = entity.DeliveryMode,
                Venue = entity.Venue,
                MeetingUrl = entity.MeetingUrl,
                InstructorName = entity.InstructorName,
                InstructorUserId = entity.InstructorUserId,
                Capacity = entity.Capacity,
                MinimumParticipant = entity.MinimumParticipant,
                RegisteredParticipantCount = enrolledCount,
                SessionStatus = entity.SessionStatus,
                RequiresAttendance = entity.RequiresAttendance,
                RequiresPreTest = entity.RequiresPreTest,
                RequiresPostTest = entity.RequiresPostTest,
                GeneratesCertificate = entity.GeneratesCertificate,
                MinimumPassingScore = entity.MinimumPassingScore,
                AgendaJson = entity.AgendaJson,
                Notes = entity.Notes,
                IsActive = entity.IsActive,
                CreatedAt = entity.CreateDateTime,
                UpdatedAt = entity.UpdateDateTime
            };
        }
    }
}
