using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Services
{
    public class TrainingEnrollmentService : ITrainingEnrollmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TrainingEnrollmentService> _logger;

        public TrainingEnrollmentService(
            ApplicationDbContext context,
            ILogger<TrainingEnrollmentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<TrainingEnrollmentDto>> GetEnrollmentsAsync(
            Guid? sessionId,
            Guid? workforceProfileId,
            string? status,
            CancellationToken cancellationToken = default)
        {
            var query = _context.TrxTrainingEnrollmentRequests
                .AsNoTracking()
                .Include(r => r.TrainingPlan)
                .Include(r => r.TrainingSession)
                .Include(r => r.WorkforceProfile)
                .Where(r => !r.IsDelete);

            if (sessionId.HasValue)
            {
                query = query.Where(r => r.TrainingSessionId == sessionId.Value);
            }

            if (workforceProfileId.HasValue)
            {
                query = query.Where(r => r.WorkforceProfileId == workforceProfileId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(r => r.RequestStatus == status);
            }

            var enrollments = await query
                .OrderByDescending(r => r.CreateDateTime)
                .ToListAsync(cancellationToken);

            return enrollments.Select(MapToDto).ToList();
        }

        public async Task<TrainingEnrollmentDto?> GetEnrollmentByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var enrollment = await _context.TrxTrainingEnrollmentRequests
                .AsNoTracking()
                .Include(r => r.TrainingPlan)
                .Include(r => r.TrainingSession)
                .Include(r => r.WorkforceProfile)
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDelete, cancellationToken);

            return enrollment == null ? null : MapToDto(enrollment);
        }

        public async Task<TrainingEnrollmentDto> SubmitEssEnrollmentAsync(
            SubmitEnrollmentRequestDto request,
            string submittedBy,
            CancellationToken cancellationToken = default)
        {
            // Validasi keberadaan Training Plan
            var plan = await _context.TrxTrainingPlans
                .FirstOrDefaultAsync(p => p.Id == request.TrainingPlanId && !p.IsDelete, cancellationToken);

            if (plan == null)
            {
                throw new KeyNotFoundException($"Training Plan dengan ID {request.TrainingPlanId} tidak ditemukan.");
            }

            // Jika memilih sesi tertentu, validasi kuota kapasitas (VR-TRN-07)
            if (request.TrainingSessionId.HasValue)
            {
                var session = await _context.TrxTrainingSessions
                    .Include(s => s.EnrollmentRequests)
                    .FirstOrDefaultAsync(s => s.Id == request.TrainingSessionId.Value && !s.IsDelete, cancellationToken);

                if (session == null)
                {
                    throw new KeyNotFoundException($"Sesi pelatihan dengan ID {request.TrainingSessionId} tidak ditemukan.");
                }

                if (session.SessionStatus != "RegistrationOpen")
                {
                    throw new InvalidOperationException($"Pendaftaran sesi {session.SessionCode} saat ini sedang {session.SessionStatus}.");
                }

                var registeredCount = session.EnrollmentRequests
                    .Count(r => !r.IsDelete && (r.RequestStatus == "Approved" || r.RequestStatus == "PendingDiklat" || r.RequestStatus == "PendingSupervisor"));

                if (registeredCount >= session.Capacity)
                {
                    throw new InvalidOperationException(
                        $"Kuota kelas pelatihan {session.SessionCode} telah penuh ({registeredCount}/{session.Capacity} peserta) (VR-TRN-07). Silakan pilih sesi lainnya.");
                }

                // Cegah pendaftaran ganda staf pada sesi yang sama
                var isAlreadyEnrolled = session.EnrollmentRequests
                    .Any(r => !r.IsDelete && r.WorkforceProfileId == request.WorkforceProfileId && r.RequestStatus != "Rejected");

                if (isAlreadyEnrolled)
                {
                    throw new InvalidOperationException("Pegawai telah terdaftar pada sesi pelatihan ini sebelumnya.");
                }
            }

            var currentYear = DateTime.UtcNow.Year;
            var countThisYear = await _context.TrxTrainingEnrollmentRequests
                .IgnoreQueryFilters()
                .CountAsync(r => r.RequestDate.Year == currentYear, cancellationToken);

            var requestNumber = $"ENR-TRN-{currentYear}-{(countThisYear + 1):D5}";

            var entity = new TrxTrainingEnrollmentRequest
            {
                Id = Guid.NewGuid(),
                TrainingPlanId = request.TrainingPlanId,
                TrainingSessionId = request.TrainingSessionId,
                WorkforceProfileId = request.WorkforceProfileId,
                EmployeeId = request.EmployeeId,
                RequestNumber = requestNumber,
                RequestDate = DateTime.UtcNow,
                EnrollmentType = string.IsNullOrWhiteSpace(request.EnrollmentType) ? "Internal" : request.EnrollmentType,
                RequestStatus = "PendingSupervisor",
                DevelopmentNeed = request.DevelopmentNeed.Trim(),
                RequestReason = request.RequestReason.Trim(),
                ExternalProviderName = request.ExternalProviderName,
                ExternalTrainingName = request.ExternalTrainingName,
                ExternalStartDate = request.ExternalStartDate,
                ExternalEndDate = request.ExternalEndDate,
                RequestedCost = request.RequestedCost,
                CurrencyCode = string.IsNullOrWhiteSpace(request.CurrencyCode) ? "IDR" : request.CurrencyCode,
                AttachmentPath = request.AttachmentPath,
                SubmittedAt = DateTime.UtcNow,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow
            };

            _context.TrxTrainingEnrollmentRequests.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Permohonan pendaftaran diklat via ESS berhasil diajukan: {RequestNumber} oleh {SubmittedBy}",
                entity.RequestNumber, submittedBy);

            return MapToDto(entity);
        }

        public async Task<TrainingEnrollmentDto> ProcessApprovalWorkflowAsync(
            Guid id,
            ProcessEnrollmentApprovalDto request,
            string reviewerUser,
            CancellationToken cancellationToken = default)
        {
            var enrollment = await _context.TrxTrainingEnrollmentRequests
                .Include(r => r.TrainingPlan)
                .Include(r => r.TrainingSession)
                .Include(r => r.WorkforceProfile)
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDelete, cancellationToken);

            if (enrollment == null)
            {
                throw new KeyNotFoundException($"Permohonan pendaftaran dengan ID {id} tidak ditemukan.");
            }

            if (request.Stage == "Supervisor")
            {
                if (enrollment.RequestStatus != "PendingSupervisor")
                {
                    throw new InvalidOperationException($"Pendaftaran ini tidak berada pada tahap approval atasan (Status: {enrollment.RequestStatus}).");
                }

                if (request.Action == "Approved")
                {
                    enrollment.RequestStatus = "PendingDiklat";
                    enrollment.ManagerActionAt = DateTime.UtcNow;
                    enrollment.ManagerActionNote = string.IsNullOrWhiteSpace(request.ActionNote) ? "Disetujui Kepala Ruangan / Supervisor" : request.ActionNote;
                }
                else
                {
                    enrollment.RequestStatus = "Rejected";
                    enrollment.ManagerActionAt = DateTime.UtcNow;
                    enrollment.ManagerActionNote = string.IsNullOrWhiteSpace(request.ActionNote) ? "Ditolak Atasan Langsung" : request.ActionNote;
                }
            }
            else if (request.Stage == "Diklat")
            {
                if (enrollment.RequestStatus != "PendingDiklat")
                {
                    throw new InvalidOperationException($"Pendaftaran ini tidak berada pada tahap validasi Diklat (Status: {enrollment.RequestStatus}).");
                }

                if (request.Action == "Approved")
                {
                    enrollment.RequestStatus = "Approved";
                    enrollment.ManagerActionNote = string.IsNullOrWhiteSpace(enrollment.ManagerActionNote)
                        ? $"Divalidasi Diklat: {request.ActionNote}"
                        : $"{enrollment.ManagerActionNote} | Diklat: {request.ActionNote}";
                }
                else
                {
                    enrollment.RequestStatus = "Rejected";
                    enrollment.ManagerActionNote = string.IsNullOrWhiteSpace(enrollment.ManagerActionNote)
                        ? $"Ditolak Diklat: {request.ActionNote}"
                        : $"{enrollment.ManagerActionNote} | Diklat: {request.ActionNote}";
                }
            }

            enrollment.UpdateDateTime = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Workflow approval pendaftaran {RequestNumber} ({Stage}) diproses: {Action} oleh {Reviewer}",
                enrollment.RequestNumber, request.Stage, request.Action, reviewerUser);

            return MapToDto(enrollment);
        }

        public async Task<bool> DeleteEnrollmentAsync(Guid id, string deletedBy, CancellationToken cancellationToken = default)
        {
            var enrollment = await _context.TrxTrainingEnrollmentRequests
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDelete, cancellationToken);

            if (enrollment == null) return false;

            if (enrollment.RequestStatus == "Approved")
            {
                throw new InvalidOperationException("Pendaftaran pelatihan yang sudah disetujui resmi oleh Diklat tidak dapat dihapus.");
            }

            enrollment.IsDelete = true;
            enrollment.DeleteDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        private static TrainingEnrollmentDto MapToDto(TrxTrainingEnrollmentRequest entity)
        {
            return new TrainingEnrollmentDto
            {
                Id = entity.Id,
                RequestNumber = entity.RequestNumber,
                RequestDate = entity.RequestDate,
                TrainingPlanId = entity.TrainingPlanId,
                TrainingPlanName = entity.TrainingPlan?.TrainingPlanName,
                TrainingSessionId = entity.TrainingSessionId,
                SessionCode = entity.TrainingSession?.SessionCode,
                SessionName = entity.TrainingSession?.SessionName,
                WorkforceProfileId = entity.WorkforceProfileId,
                StaffName = entity.WorkforceProfile?.DisplayName,
                EnrollmentType = entity.EnrollmentType,
                RequestStatus = entity.RequestStatus,
                DevelopmentNeed = entity.DevelopmentNeed,
                RequestReason = entity.RequestReason,
                ExternalProviderName = entity.ExternalProviderName,
                ExternalTrainingName = entity.ExternalTrainingName,
                ExternalStartDate = entity.ExternalStartDate,
                ExternalEndDate = entity.ExternalEndDate,
                RequestedCost = entity.RequestedCost,
                CurrencyCode = entity.CurrencyCode,
                AttachmentPath = entity.AttachmentPath,
                SubmittedAt = entity.SubmittedAt,
                ManagerActionAt = entity.ManagerActionAt,
                ManagerActionNote = entity.ManagerActionNote,
                CreatedAt = entity.CreateDateTime,
                IsActive = entity.IsActive
            };
        }
    }
}
