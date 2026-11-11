using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Services
{
    public class AnnualTrainingPlanService : IAnnualTrainingPlanService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AnnualTrainingPlanService> _logger;

        public AnnualTrainingPlanService(
            ApplicationDbContext context,
            ILogger<AnnualTrainingPlanService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<AnnualTrainingPlanDto>> GetAnnualPlansAsync(
            int? planYear,
            string? planStatus,
            Guid? departmentId,
            CancellationToken cancellationToken = default)
        {
            var query = _context.TrxTrainingPlans
                .AsNoTracking()
                .Include(p => p.Department)
                .Where(p => !p.IsDelete);

            if (planYear.HasValue)
            {
                query = query.Where(p => p.PlanYear == planYear.Value);
            }

            if (!string.IsNullOrWhiteSpace(planStatus))
            {
                query = query.Where(p => p.PlanStatus == planStatus);
            }

            if (departmentId.HasValue)
            {
                query = query.Where(p => p.DepartmentId == departmentId.Value);
            }

            var plans = await query
                .OrderByDescending(p => p.CreateDateTime)
                .ToListAsync(cancellationToken);

            return plans.Select(MapToDto).ToList();
        }

        public async Task<AnnualTrainingPlanDto?> GetAnnualPlanByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var plan = await _context.TrxTrainingPlans
                .AsNoTracking()
                .Include(p => p.Department)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDelete, cancellationToken);

            return plan == null ? null : MapToDto(plan);
        }

        public async Task<AnnualTrainingPlanDto> CreateAnnualPlanAsync(
            CreateAnnualTrainingPlanRequestDto request,
            string createdBy,
            CancellationToken cancellationToken = default)
        {
            // Validasi VR-TRN-13
            ValidateTrainingPlanCompleteness(
                request.PlannedStartDate,
                request.PlannedEndDate,
                request.EstimatedCost,
                request.PlannedParticipantCount,
                request.Objectives);

            var planYear = request.PlanYear > 0 ? request.PlanYear : DateTime.UtcNow.Year;

            // Generate nomor rencana TNA baku: TNA-{Year}-{Sequence:D4}
            var countThisYear = await _context.TrxTrainingPlans
                .IgnoreQueryFilters()
                .CountAsync(p => p.PlanYear == planYear, cancellationToken);

            var planNumber = $"TNA-{planYear}-{(countThisYear + 1):D4}";

            var entity = new TrxTrainingPlan
            {
                Id = Guid.NewGuid(),
                TrainingPlanNumber = planNumber,
                TrainingPlanName = request.TrainingPlanName.Trim(),
                PlanYear = planYear,
                PlanType = "Annual",
                PlanStatus = "Draft",
                PlannedStartDate = request.PlannedStartDate,
                PlannedEndDate = request.PlannedEndDate,
                DeliveryMode = request.DeliveryMode ?? "InHouseWorkshop",
                ProviderName = request.ProviderName,
                TargetAudience = request.TargetAudience,
                IsMandatory = request.IsMandatory,
                IsExternalTraining = request.IsExternalTraining,
                PlannedParticipantCount = request.PlannedParticipantCount,
                ApprovedParticipantCount = 0,
                EstimatedCost = request.EstimatedCost,
                ApprovedBudget = 0m,
                CurrencyCode = string.IsNullOrWhiteSpace(request.CurrencyCode) ? "IDR" : request.CurrencyCode,
                Objectives = request.Objectives.Trim(),
                Notes = request.Notes,
                DepartmentId = request.DepartmentId,
                MandatoryTrainingRuleId = request.MandatoryTrainingRuleId,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow
            };

            _context.TrxTrainingPlans.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Rencana Program Pelatihan Tahunan TNA baru berhasil dibuat: {PlanNumber} - {PlanName} oleh {CreatedBy}",
                entity.TrainingPlanNumber, entity.TrainingPlanName, createdBy);

            return MapToDto(entity);
        }

        public async Task<AnnualTrainingPlanDto> UpdateAnnualPlanAsync(
            Guid id,
            UpdateAnnualTrainingPlanRequestDto request,
            string updatedBy,
            CancellationToken cancellationToken = default)
        {
            var plan = await _context.TrxTrainingPlans
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDelete, cancellationToken);

            if (plan == null)
            {
                throw new KeyNotFoundException($"Rencana pelatihan dengan ID {id} tidak ditemukan.");
            }

            if (plan.PlanStatus != "Draft")
            {
                throw new InvalidOperationException($"Hanya rencana pelatihan berstatus Draft yang dapat diperbarui. Status saat ini: {plan.PlanStatus}.");
            }

            // Validasi VR-TRN-13
            ValidateTrainingPlanCompleteness(
                request.PlannedStartDate,
                request.PlannedEndDate,
                request.EstimatedCost,
                request.PlannedParticipantCount,
                request.Objectives ?? plan.Objectives ?? "Default");

            plan.TrainingPlanName = request.TrainingPlanName.Trim();
            plan.PlannedStartDate = request.PlannedStartDate;
            plan.PlannedEndDate = request.PlannedEndDate;
            plan.DeliveryMode = request.DeliveryMode;
            plan.ProviderName = request.ProviderName;
            plan.TargetAudience = request.TargetAudience;
            plan.IsMandatory = request.IsMandatory;
            plan.IsExternalTraining = request.IsExternalTraining;
            plan.PlannedParticipantCount = request.PlannedParticipantCount;
            plan.EstimatedCost = request.EstimatedCost;
            plan.Objectives = request.Objectives?.Trim() ?? plan.Objectives;
            plan.Notes = request.Notes ?? plan.Notes;
            plan.UpdateDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return MapToDto(plan);
        }

        public async Task<AnnualTrainingPlanDto> SubmitAnnualPlanAsync(
            Guid id,
            SubmitTrainingPlanRequestDto? request,
            string submittedBy,
            CancellationToken cancellationToken = default)
        {
            var plan = await _context.TrxTrainingPlans
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDelete, cancellationToken);

            if (plan == null)
            {
                throw new KeyNotFoundException($"Rencana pelatihan dengan ID {id} tidak ditemukan.");
            }

            if (plan.PlanStatus != "Draft")
            {
                throw new InvalidOperationException($"Hanya rencana berstatus Draft yang dapat diajukan ke Direksi. Status saat ini: {plan.PlanStatus}.");
            }

            plan.PlanStatus = "Submitted";
            plan.SubmittedAt = DateTime.UtcNow;
            if (request?.SubmissionNotes != null)
            {
                plan.Notes = string.IsNullOrWhiteSpace(plan.Notes)
                    ? $"Pengajuan: {request.SubmissionNotes}"
                    : $"{plan.Notes} | Pengajuan: {request.SubmissionNotes}";
            }
            plan.UpdateDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Rencana TNA {PlanNumber} telah diajukan ke Direksi untuk disetujui.", plan.TrainingPlanNumber);
            return MapToDto(plan);
        }

        public async Task<AnnualTrainingPlanDto> ApproveAnnualPlanAsync(
            Guid id,
            ApproveTrainingPlanRequestDto request,
            string approvedBy,
            CancellationToken cancellationToken = default)
        {
            var plan = await _context.TrxTrainingPlans
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDelete, cancellationToken);

            if (plan == null)
            {
                throw new KeyNotFoundException($"Rencana pelatihan dengan ID {id} tidak ditemukan.");
            }

            if (plan.PlanStatus != "Submitted")
            {
                throw new InvalidOperationException($"Hanya rencana berstatus Submitted yang dapat disahkan oleh Direksi. Status saat ini: {plan.PlanStatus}.");
            }

            plan.PlanStatus = "Approved";
            plan.ApprovedAt = DateTime.UtcNow;
            plan.ApprovedBudget = request.ApprovedBudget > 0 ? request.ApprovedBudget : plan.EstimatedCost;
            plan.ApprovedParticipantCount = request.ApprovedParticipantCount ?? plan.PlannedParticipantCount;

            if (!string.IsNullOrWhiteSpace(request.ApprovalNotes))
            {
                plan.Notes = string.IsNullOrWhiteSpace(plan.Notes)
                    ? $"Persetujuan Direksi: {request.ApprovalNotes}"
                    : $"{plan.Notes} | Persetujuan Direksi: {request.ApprovalNotes}";
            }

            plan.UpdateDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Rencana TNA {PlanNumber} resmi DISAHKAN oleh Direksi ({ApprovedBy}) dengan pagu anggaran Rp {ApprovedBudget:N0}.",
                plan.TrainingPlanNumber, approvedBy, plan.ApprovedBudget);

            return MapToDto(plan);
        }

        public async Task<AnnualTrainingPlanDto> RejectAnnualPlanAsync(
            Guid id,
            RejectTrainingPlanRequestDto request,
            string rejectedBy,
            CancellationToken cancellationToken = default)
        {
            var plan = await _context.TrxTrainingPlans
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDelete, cancellationToken);

            if (plan == null)
            {
                throw new KeyNotFoundException($"Rencana pelatihan dengan ID {id} tidak ditemukan.");
            }

            if (plan.PlanStatus != "Submitted")
            {
                throw new InvalidOperationException($"Hanya rencana berstatus Submitted yang dapat dikembalikan untuk revisi. Status saat ini: {plan.PlanStatus}.");
            }

            // Kembalikan ke Draft untuk revisi
            plan.PlanStatus = "Draft";
            plan.Notes = string.IsNullOrWhiteSpace(plan.Notes)
                ? $"Revisi dari Direksi: {request.RejectionReason}"
                : $"{plan.Notes} | Revisi Direksi: {request.RejectionReason}";
            plan.UpdateDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogWarning(
                "Rencana TNA {PlanNumber} dikembalikan untuk revisi oleh {RejectedBy}. Alasan: {Reason}",
                plan.TrainingPlanNumber, rejectedBy, request.RejectionReason);

            return MapToDto(plan);
        }

        public async Task<bool> DeleteAnnualPlanAsync(Guid id, string deletedBy, CancellationToken cancellationToken = default)
        {
            var plan = await _context.TrxTrainingPlans
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDelete, cancellationToken);

            if (plan == null) return false;

            if (plan.PlanStatus != "Draft")
            {
                throw new InvalidOperationException($"Rencana pelatihan berstatus {plan.PlanStatus} tidak dapat dihapus.");
            }

            plan.IsDelete = true;
            plan.DeleteDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        private static void ValidateTrainingPlanCompleteness(
            DateTime startDate,
            DateTime endDate,
            decimal estimatedCost,
            int plannedParticipants,
            string? objectives)
        {
            if (endDate < startDate)
            {
                throw new ArgumentException("Tanggal akhir rencana pelatihan tidak boleh mendahului tanggal mulai (VR-TRN-13).");
            }

            if (estimatedCost <= 0)
            {
                throw new ArgumentException("Estimasi anggaran pelatihan wajib lebih besar dari 0 (VR-TRN-13).");
            }

            if (plannedParticipants <= 0)
            {
                throw new ArgumentException("Target jumlah peserta pelatihan minimal 1 orang (VR-TRN-13).");
            }

            if (string.IsNullOrWhiteSpace(objectives))
            {
                throw new ArgumentException("Target kompetensi dan tujuan pembelajaran (Objectives) wajib diisi lengkap sesuai standar TNA (VR-TRN-13).");
            }
        }

        private static AnnualTrainingPlanDto MapToDto(TrxTrainingPlan entity)
        {
            return new AnnualTrainingPlanDto
            {
                Id = entity.Id,
                TrainingPlanNumber = entity.TrainingPlanNumber,
                TrainingPlanName = entity.TrainingPlanName,
                PlanYear = entity.PlanYear,
                PlanType = entity.PlanType,
                PlanStatus = entity.PlanStatus,
                PlannedStartDate = entity.PlannedStartDate,
                PlannedEndDate = entity.PlannedEndDate,
                DeliveryMode = entity.DeliveryMode,
                ProviderName = entity.ProviderName,
                TargetAudience = entity.TargetAudience,
                IsMandatory = entity.IsMandatory,
                IsExternalTraining = entity.IsExternalTraining,
                PlannedParticipantCount = entity.PlannedParticipantCount,
                ApprovedParticipantCount = entity.ApprovedParticipantCount,
                EstimatedCost = entity.EstimatedCost,
                ApprovedBudget = entity.ApprovedBudget,
                CurrencyCode = entity.CurrencyCode,
                Objectives = entity.Objectives,
                Notes = entity.Notes,
                DepartmentId = entity.DepartmentId,
                DepartmentName = entity.Department?.DepartmentName,
                MandatoryTrainingRuleId = entity.MandatoryTrainingRuleId,
                SubmittedAt = entity.SubmittedAt,
                SubmittedByName = entity.SubmittedByUser?.UserName,
                ApprovedAt = entity.ApprovedAt,
                ApprovedByName = entity.ApprovedByUser?.UserName,
                CreatedAt = entity.CreateDateTime,
                UpdatedAt = entity.UpdateDateTime,
                IsActive = entity.IsActive
            };
        }
    }
}
