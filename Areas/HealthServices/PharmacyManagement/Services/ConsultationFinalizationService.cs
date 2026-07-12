using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services
{
    public class ConsultationFinalizationService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ConsultationValidationService _validationService;
        private readonly PrescriptionAggregateService _prescriptionAggregateService;
        private readonly PrescriptionWorkflowService _prescriptionWorkflowService;

        public ConsultationFinalizationService(
            ApplicationDbContext dbContext,
            ConsultationValidationService validationService,
            PrescriptionAggregateService prescriptionAggregateService,
            PrescriptionWorkflowService prescriptionWorkflowService)
        {
            _dbContext = dbContext;
            _validationService = validationService;
            _prescriptionAggregateService = prescriptionAggregateService;
            _prescriptionWorkflowService = prescriptionWorkflowService;
        }

        public async Task<ConsultationFinalizationOperationResult> FinalizeAsync(
            Guid consultationId,
            FinalizeDoctorConsultationRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            var consultation = await _dbContext.Set<TrxDoctorConsultation>()
                .Include(x => x.Queue)
                .Include(x => x.Encounter)
                .FirstOrDefaultAsync(x => x.Id == consultationId && !x.IsDelete, cancellationToken);

            if (consultation == null)
                return ConsultationFinalizationOperationResult.Fail("Konsultasi dokter tidak ditemukan.");

            if (request.ExpectedUpdatedAt.HasValue && consultation.UpdateDateTime.HasValue &&
                consultation.UpdateDateTime.Value.ToUniversalTime() != request.ExpectedUpdatedAt.Value.ToUniversalTime())
            {
                return ConsultationFinalizationOperationResult.Conflict("Data konsultasi telah berubah. Muat ulang sebelum menyelesaikan konsultasi.");
            }

            if (consultation.ConsultationStatus is DoctorConsultationStatus.Completed or DoctorConsultationStatus.Cancelled)
                return ConsultationFinalizationOperationResult.Fail("Status konsultasi tidak valid untuk finalisasi.");

            ApplyRequest(consultation, request);
            consultation.UpdateDateTime = now;
            consultation.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            var prescriptions = await _dbContext.Set<TrxPrescription>()
                .Where(x => x.ConsultationId == consultationId && !x.IsDelete && !x.IsCancel && x.IsActive)
                .ToListAsync(cancellationToken);

            foreach (var prescription in prescriptions.Where(x => x.PrescriptionStatus == PrescriptionStatus.Draft))
            {
                await _prescriptionAggregateService.RebuildAsync(prescription.Id, actorUserId, now, cancellationToken);
            }

            var validation = await _validationService.ValidateAsync(consultationId, cancellationToken);
            if (validation.ErrorCount > 0)
            {
                await transaction.RollbackAsync(cancellationToken);
                return ConsultationFinalizationOperationResult.ValidationFailed(validation);
            }

            var acknowledged = new HashSet<string>(request.AcknowledgedWarningKeys ?? new List<string>(), StringComparer.OrdinalIgnoreCase);
            var missingWarnings = validation.Sections.SelectMany(x => x.Issues)
                .Where(x => x.Severity == ConsultationValidationSeverity.Warning && !acknowledged.Contains(x.IssueKey))
                .ToList();

            if (missingWarnings.Count > 0)
            {
                await transaction.RollbackAsync(cancellationToken);
                return ConsultationFinalizationOperationResult.WarningAcknowledgementRequired(validation);
            }

            var finalizedPrescriptionCount = 0;
            foreach (var prescription in prescriptions.Where(x => x.PrescriptionStatus == PrescriptionStatus.Draft))
            {
                var workflow = await _prescriptionWorkflowService.FinalizeFromConsultationAsync(prescription, actorUserId, now, cancellationToken);
                if (!workflow.IsSuccess)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return ConsultationFinalizationOperationResult.Fail(workflow.ErrorMessage ?? "Resep gagal difinalkan.");
                }
                finalizedPrescriptionCount++;
            }

            consultation.ConsultationStatus = DoctorConsultationStatus.Completed;
            consultation.CompletedAt = now;
            consultation.CompletedByUserId = actorUserId;
            consultation.DoctorNote = MergeNote(consultation.DoctorNote, request.FinalizationNote);
            consultation.UpdateDateTime = now;
            consultation.UpdateBy = actorUserId;

            if (consultation.Queue != null)
            {
                consultation.Queue.QueueStatus = QueueStatus.Completed;
                consultation.Queue.ConsultationCompletedAt = now;
                consultation.Queue.CompletedAt = now;
                consultation.Queue.CompletedByUserId = actorUserId;
                consultation.Queue.UpdateDateTime = now;
                consultation.Queue.UpdateBy = actorUserId;
            }

            if (consultation.Encounter != null)
            {
                consultation.Encounter.EncounterStatus = EncounterStatus.ConsultationCompleted;
                consultation.Encounter.UpdateDateTime = now;
                consultation.Encounter.UpdateBy = actorUserId;
            }

            var finalizedProcedureCount = await _dbContext.Set<TrxPatientProcedure>()
                .CountAsync(x => x.ConsultationId == consultationId && !x.IsDelete && !x.IsCancel && x.IsActive, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return ConsultationFinalizationOperationResult.Success(new ConsultationFinalizationResponse
            {
                ConsultationId = consultationId,
                CompletedAt = now,
                CompletedByUserId = actorUserId,
                FinalizedPrescriptionCount = finalizedPrescriptionCount,
                FinalizedProcedureCount = finalizedProcedureCount,
                Validation = validation
            });
        }

        private static void ApplyRequest(TrxDoctorConsultation entity, FinalizeDoctorConsultationRequest request)
        {
            entity.ChiefComplaint = Normalize(request.ChiefComplaint) ?? entity.ChiefComplaint;
            entity.HistoryOfPresentIllness = Normalize(request.HistoryOfPresentIllness) ?? entity.HistoryOfPresentIllness;
            entity.PhysicalExamination = Normalize(request.PhysicalExamination) ?? entity.PhysicalExamination;
            entity.Subjective = Normalize(request.Subjective) ?? entity.Subjective;
            entity.Objective = Normalize(request.Objective) ?? entity.Objective;
            entity.Assessment = Normalize(request.Assessment) ?? entity.Assessment;
            entity.Plan = Normalize(request.Plan) ?? entity.Plan;
            entity.ProcedurePlan = Normalize(request.ProcedurePlan) ?? entity.ProcedurePlan;
            entity.PrescriptionPlan = Normalize(request.PrescriptionPlan) ?? entity.PrescriptionPlan;
            entity.SupportingExamPlan = Normalize(request.SupportingExamPlan) ?? entity.SupportingExamPlan;
            entity.ReferralPlan = Normalize(request.ReferralPlan) ?? entity.ReferralPlan;
            entity.EducationPlan = Normalize(request.EducationPlan) ?? entity.EducationPlan;
            entity.FollowUpDate = request.FollowUpDate ?? entity.FollowUpDate;
            entity.FollowUpNote = Normalize(request.FollowUpNote) ?? entity.FollowUpNote;
            entity.DoctorNote = Normalize(request.DoctorNote) ?? entity.DoctorNote;
        }

        private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        private static string? MergeNote(string? current, string? addition)
        {
            var note = Normalize(addition);
            if (note == null) return current;
            return string.IsNullOrWhiteSpace(current) ? note : $"{current}\nFinalisasi: {note}";
        }
    }

    public class ConsultationFinalizationOperationResult
    {
        public bool IsSuccess { get; private set; }
        public bool IsConflict { get; private set; }
        public bool RequiresWarningAcknowledgement { get; private set; }
        public string? ErrorMessage { get; private set; }
        public ConsultationFinalizationValidationResponse? Validation { get; private set; }
        public ConsultationFinalizationResponse? Data { get; private set; }

        public static ConsultationFinalizationOperationResult Success(ConsultationFinalizationResponse data) => new() { IsSuccess = true, Data = data, Validation = data.Validation };
        public static ConsultationFinalizationOperationResult Fail(string message) => new() { ErrorMessage = message };
        public static ConsultationFinalizationOperationResult Conflict(string message) => new() { IsConflict = true, ErrorMessage = message };
        public static ConsultationFinalizationOperationResult ValidationFailed(ConsultationFinalizationValidationResponse validation) => new() { ErrorMessage = "Konsultasi belum dapat diselesaikan.", Validation = validation };
        public static ConsultationFinalizationOperationResult WarningAcknowledgementRequired(ConsultationFinalizationValidationResponse validation) => new() { RequiresWarningAcknowledgement = true, ErrorMessage = "Konfirmasi seluruh peringatan sebelum menyelesaikan konsultasi.", Validation = validation };
    }
}
