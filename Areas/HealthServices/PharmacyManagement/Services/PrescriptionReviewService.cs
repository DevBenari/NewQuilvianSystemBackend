using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Repositories;
using System.Security.Cryptography;
using System.Text;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services
{
    public class PrescriptionReviewService
    {
        private readonly ApplicationDbContext _dbContext;

        public PrescriptionReviewService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PrescriptionReviewResponse?> GetActiveAsync(
            Guid prescriptionId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxPrescriptionReview>()
                .AsNoTracking()
                .Include(x => x.Items.Where(i => !i.IsDelete && i.IsActive))
                .Include(x => x.Clarifications.Where(c => !c.IsDelete && c.IsActive))
                .Where(x => x.PrescriptionId == prescriptionId && !x.IsDelete && x.IsActive)
                .OrderByDescending(x => x.ReviewVersion)
                .FirstOrDefaultAsync(cancellationToken);

            return entity == null ? null : Map(entity);
        }

        public async Task<PrescriptionReviewResponse> StartAsync(
            Guid prescriptionId,
            Guid pharmacistUserId,
            string? generalNote,
            CancellationToken cancellationToken = default)
        {
            var prescription = await _dbContext.Set<TrxPrescription>()
                .Include(x => x.Items.Where(i => !i.IsDelete && !i.IsCancel && i.IsActive))
                .Include(x => x.Compounds.Where(c => !c.IsDelete && !c.IsCancel && c.IsActive))
                    .ThenInclude(x => x.Items.Where(i => !i.IsDelete && !i.IsCancel && i.IsActive))
                .FirstOrDefaultAsync(x => x.Id == prescriptionId && !x.IsDelete, cancellationToken)
                ?? throw new InvalidOperationException("Resep tidak ditemukan.");

            if (prescription.PrescriptionStatus != PrescriptionStatus.Submitted)
                throw new InvalidOperationException("Telaah farmasi hanya dapat dimulai untuk resep yang sudah diajukan dokter.");

            if (prescription.FulfillmentStatus is not
                (PrescriptionFulfillmentStatus.ReadyForPharmacy or
                 PrescriptionFulfillmentStatus.QueuedAtPharmacy))
            {
                throw new InvalidOperationException("Status resep belum siap untuk proses telaah farmasi.");
            }

            var active = await _dbContext.Set<TrxPrescriptionReview>()
                .Include(x => x.Items)
                .Where(x => x.PrescriptionId == prescriptionId && !x.IsDelete && x.IsActive)
                .OrderByDescending(x => x.ReviewVersion)
                .FirstOrDefaultAsync(cancellationToken);

            var signature = BuildPrescriptionSignature(prescription);
            if (active != null && active.Status is PrescriptionReviewStatus.Pending or PrescriptionReviewStatus.InReview or PrescriptionReviewStatus.ClarificationRequired or PrescriptionReviewStatus.RevisedByDoctor)
            {
                active.Status = PrescriptionReviewStatus.InReview;
                active.ReviewedByPharmacistId = pharmacistUserId;
                active.StartedAt ??= DateTime.UtcNow;
                active.GeneralNote = Normalize(generalNote);
                active.UpdateDateTime = DateTime.UtcNow;
                active.UpdateBy = pharmacistUserId;
                prescription.FulfillmentStatus = PrescriptionFulfillmentStatus.QueuedAtPharmacy;
                await _dbContext.SaveChangesAsync(cancellationToken);
                return Map(active);
            }

            var currentMaxVersion = await _dbContext.Set<TrxPrescriptionReview>()
                .Where(x => x.PrescriptionId == prescriptionId)
                .MaxAsync(x => (int?)x.ReviewVersion, cancellationToken) ?? 0;
            var nextVersion = currentMaxVersion + 1;

            var review = new TrxPrescriptionReview
            {
                Id = Guid.NewGuid(),
                PrescriptionId = prescriptionId,
                ReviewVersion = nextVersion,
                Status = PrescriptionReviewStatus.InReview,
                ReviewedByPharmacistId = pharmacistUserId,
                StartedAt = DateTime.UtcNow,
                GeneralNote = Normalize(generalNote),
                PrescriptionSignatureSnapshot = signature,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = pharmacistUserId,
                IsActive = true
            };

            var hasRegular = prescription.Items.Count > 0;
            var hasCompound = prescription.Compounds.Count > 0;
            var criteria = await _dbContext.Set<MstPrescriptionReviewCriterion>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive &&
                    ((hasRegular && x.IsApplicableToRegular) || (hasCompound && x.IsApplicableToCompound)))
                .OrderBy(x => x.Category)
                .ThenBy(x => x.SortOrder)
                .ToListAsync(cancellationToken);

            foreach (var criterion in criteria)
            {
                review.Items.Add(new TrxPrescriptionReviewItem
                {
                    Id = Guid.NewGuid(),
                    PrescriptionReviewId = review.Id,
                    CriterionId = criterion.Id,
                    Category = criterion.Category,
                    CriterionCodeSnapshot = criterion.CriterionCode,
                    CriterionNameSnapshot = criterion.CriterionName,
                    Result = PrescriptionReviewResult.NotReviewed,
                    Severity = criterion.DefaultSeverity,
                    SortOrder = criterion.SortOrder,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = pharmacistUserId,
                    IsActive = true
                });
            }

            _dbContext.Set<TrxPrescriptionReview>().Add(review);
            prescription.FulfillmentStatus = PrescriptionFulfillmentStatus.QueuedAtPharmacy;
            prescription.UpdateDateTime = DateTime.UtcNow;
            prescription.UpdateBy = pharmacistUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Map(review);
        }

        public async Task<PrescriptionReviewResponse> UpdateItemsAsync(
            Guid reviewId,
            UpdatePrescriptionReviewItemsRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var review = await LoadEditableReviewAsync(reviewId, cancellationToken);
            var itemsById = review.Items.ToDictionary(x => x.Id);
            var now = DateTime.UtcNow;

            foreach (var input in request.Items)
            {
                if (!itemsById.TryGetValue(input.ReviewItemId, out var item))
                    throw new InvalidOperationException("Salah satu kriteria telaah tidak ditemukan.");

                item.Result = input.Result;
                item.Severity = input.Severity ?? item.Severity;
                item.Finding = Normalize(input.Finding);
                item.Recommendation = Normalize(input.Recommendation);
                item.ReviewedByUserId = actorUserId;
                item.ReviewedAt = now;
                item.UpdateDateTime = now;
                item.UpdateBy = actorUserId;
            }

            review.GeneralNote = Normalize(request.GeneralNote) ?? review.GeneralNote;
            review.Status = PrescriptionReviewStatus.InReview;
            RecalculateFlags(review);
            review.UpdateDateTime = now;
            review.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Map(review);
        }

        public async Task<PrescriptionReviewResponse> CompleteAsync(
            Guid reviewId,
            bool approve,
            string? generalNote,
            Guid pharmacistUserId,
            CancellationToken cancellationToken = default)
        {
            var review = await _dbContext.Set<TrxPrescriptionReview>()
                .Include(x => x.Prescription)
                .Include(x => x.Items.Where(i => !i.IsDelete && i.IsActive))
                .Include(x => x.Clarifications.Where(c => !c.IsDelete && c.IsActive))
                .FirstOrDefaultAsync(x => x.Id == reviewId && !x.IsDelete, cancellationToken)
                ?? throw new InvalidOperationException("Telaah resep tidak ditemukan.");

            if (review.Items.Any(x => x.Result == PrescriptionReviewResult.NotReviewed))
                throw new InvalidOperationException("Seluruh kriteria wajib ditelaah sebelum proses diselesaikan.");

            RecalculateFlags(review);
            var hasHardStop = review.Items.Any(x =>
                x.Result == PrescriptionReviewResult.NotCompliant &&
                x.Severity == PrescriptionIssueSeverity.HardStop);
            var hasOpenClarification = review.Clarifications.Any(x =>
                x.Status is PrescriptionClarificationStatus.Open or
                    PrescriptionClarificationStatus.AcknowledgedByDoctor or
                    PrescriptionClarificationStatus.RevisedByDoctor);

            if (approve && (hasHardStop || hasOpenClarification))
                throw new InvalidOperationException("Telaah belum dapat disetujui karena masih ada hard stop atau klarifikasi terbuka.");

            var now = DateTime.UtcNow;
            review.GeneralNote = Normalize(generalNote) ?? review.GeneralNote;
            review.ReviewedByPharmacistId = pharmacistUserId;
            review.CompletedAt = now;
            review.Status = approve ? PrescriptionReviewStatus.Approved : PrescriptionReviewStatus.Rejected;
            review.UpdateDateTime = now;
            review.UpdateBy = pharmacistUserId;

            if (review.Prescription != null)
            {
                review.Prescription.FulfillmentStatus = approve
                    ? PrescriptionFulfillmentStatus.VerifiedByPharmacy
                    : PrescriptionFulfillmentStatus.QueuedAtPharmacy;
                review.Prescription.UpdateDateTime = now;
                review.Prescription.UpdateBy = pharmacistUserId;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return Map(review);
        }

        public async Task<PrescriptionClarificationResponse> CreateClarificationAsync(
            Guid reviewId,
            CreatePrescriptionClarificationRequest request,
            Guid pharmacistUserId,
            CancellationToken cancellationToken = default)
        {
            var review = await LoadEditableReviewAsync(reviewId, cancellationToken);
            var now = DateTime.UtcNow;
            var entity = new TrxPrescriptionClarification
            {
                Id = Guid.NewGuid(),
                PrescriptionId = review.PrescriptionId,
                PrescriptionReviewId = review.Id,
                PrescriptionReviewItemId = request.PrescriptionReviewItemId,
                PrescriptionItemId = request.PrescriptionItemId,
                PrescriptionCompoundId = request.PrescriptionCompoundId,
                PrescriptionCompoundItemId = request.PrescriptionCompoundItemId,
                ProblemCode = request.ProblemCode.Trim(),
                ProblemDescription = request.ProblemDescription.Trim(),
                PharmacistRecommendation = Normalize(request.PharmacistRecommendation),
                Severity = request.Severity,
                Status = PrescriptionClarificationStatus.Open,
                RequestedByPharmacistId = pharmacistUserId,
                RequestedAt = now,
                CreateDateTime = now,
                CreateBy = pharmacistUserId,
                IsActive = true
            };

            review.Status = PrescriptionReviewStatus.ClarificationRequired;
            review.RequiresDoctorClarification = true;
            review.UpdateDateTime = now;
            review.UpdateBy = pharmacistUserId;
            _dbContext.Set<TrxPrescriptionClarification>().Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Map(entity);
        }

        public async Task<PrescriptionClarificationResponse> RespondClarificationAsync(
            Guid clarificationId,
            DoctorClarificationResponseRequest request,
            Guid doctorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxPrescriptionClarification>()
                .Include(x => x.PrescriptionReview)
                .FirstOrDefaultAsync(x => x.Id == clarificationId && !x.IsDelete, cancellationToken)
                ?? throw new InvalidOperationException("Klarifikasi tidak ditemukan.");

            if (entity.Status is PrescriptionClarificationStatus.Closed or PrescriptionClarificationStatus.Cancelled)
                throw new InvalidOperationException("Klarifikasi sudah ditutup.");

            var now = DateTime.UtcNow;
            entity.DoctorResponse = request.DoctorResponse.Trim();
            entity.RespondedByDoctorId = doctorUserId;
            entity.RespondedAt = now;
            entity.Status = request.PrescriptionWasRevised
                ? PrescriptionClarificationStatus.RevisedByDoctor
                : PrescriptionClarificationStatus.AcknowledgedByDoctor;
            entity.UpdateDateTime = now;
            entity.UpdateBy = doctorUserId;

            if (entity.PrescriptionReview != null)
            {
                entity.PrescriptionReview.Status = PrescriptionReviewStatus.RevisedByDoctor;
                entity.PrescriptionReview.UpdateDateTime = now;
                entity.PrescriptionReview.UpdateBy = doctorUserId;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return Map(entity);
        }

        public async Task<PrescriptionClarificationResponse> CloseClarificationAsync(
            Guid clarificationId,
            ClosePrescriptionClarificationRequest request,
            Guid pharmacistUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxPrescriptionClarification>()
                .Include(x => x.PrescriptionReview)
                .ThenInclude(x => x!.Clarifications)
                .FirstOrDefaultAsync(x => x.Id == clarificationId && !x.IsDelete, cancellationToken)
                ?? throw new InvalidOperationException("Klarifikasi tidak ditemukan.");

            var now = DateTime.UtcNow;
            entity.Status = request.Accepted
                ? PrescriptionClarificationStatus.AcceptedByPharmacist
                : PrescriptionClarificationStatus.Rejected;
            entity.ClosedByUserId = pharmacistUserId;
            entity.ClosedAt = now;
            entity.CoverageNoteSafeAppend(request.ClosingNote);
            entity.UpdateDateTime = now;
            entity.UpdateBy = pharmacistUserId;

            if (entity.PrescriptionReview != null)
            {
                var stillOpen = entity.PrescriptionReview.Clarifications
                    .Where(x => x.Id != entity.Id && !x.IsDelete && x.IsActive)
                    .Any(x => x.Status is PrescriptionClarificationStatus.Open or
                        PrescriptionClarificationStatus.AcknowledgedByDoctor or
                        PrescriptionClarificationStatus.RevisedByDoctor);
                entity.PrescriptionReview.RequiresDoctorClarification = stillOpen;
                entity.PrescriptionReview.Status = stillOpen
                    ? PrescriptionReviewStatus.ClarificationRequired
                    : PrescriptionReviewStatus.InReview;
                entity.PrescriptionReview.UpdateDateTime = now;
                entity.PrescriptionReview.UpdateBy = pharmacistUserId;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return Map(entity);
        }

        private async Task<TrxPrescriptionReview> LoadEditableReviewAsync(Guid reviewId, CancellationToken cancellationToken)
        {
            var review = await _dbContext.Set<TrxPrescriptionReview>()
                .Include(x => x.Items.Where(i => !i.IsDelete && i.IsActive))
                .Include(x => x.Clarifications.Where(c => !c.IsDelete && c.IsActive))
                .FirstOrDefaultAsync(x => x.Id == reviewId && !x.IsDelete && x.IsActive, cancellationToken)
                ?? throw new InvalidOperationException("Telaah resep tidak ditemukan.");

            if (review.Status is PrescriptionReviewStatus.Approved or PrescriptionReviewStatus.Rejected or PrescriptionReviewStatus.Cancelled)
                throw new InvalidOperationException("Telaah yang sudah selesai tidak dapat diubah.");
            return review;
        }

        private static void RecalculateFlags(TrxPrescriptionReview review)
        {
            bool HasProblem(PrescriptionReviewCategory category) => review.Items.Any(x =>
                x.Category == category && x.Result == PrescriptionReviewResult.NotCompliant);
            review.HasAdministrativeProblem = HasProblem(PrescriptionReviewCategory.Administrative);
            review.HasPharmaceuticalProblem = HasProblem(PrescriptionReviewCategory.Pharmaceutical);
            review.HasClinicalProblem = HasProblem(PrescriptionReviewCategory.Clinical);
            review.HasCompoundFormulaProblem = HasProblem(PrescriptionReviewCategory.CompoundFormula);
        }

        private static string BuildPrescriptionSignature(TrxPrescription prescription)
        {
            var raw = string.Join("|", new[]
            {
                prescription.Id.ToString(),
                prescription.UpdateDateTime?.Ticks.ToString() ?? prescription.CreateDateTime.Ticks.ToString(),
                prescription.TotalItemCount.ToString(),
                prescription.TotalPrice.ToString("0.####")
            });
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));
        }

        private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static PrescriptionReviewResponse Map(TrxPrescriptionReview x) => new()
        {
            Id = x.Id,
            PrescriptionId = x.PrescriptionId,
            ReviewVersion = x.ReviewVersion,
            Status = x.Status,
            ReviewedByPharmacistId = x.ReviewedByPharmacistId,
            StartedAt = x.StartedAt,
            CompletedAt = x.CompletedAt,
            HasAdministrativeProblem = x.HasAdministrativeProblem,
            HasPharmaceuticalProblem = x.HasPharmaceuticalProblem,
            HasClinicalProblem = x.HasClinicalProblem,
            HasCompoundFormulaProblem = x.HasCompoundFormulaProblem,
            RequiresDoctorClarification = x.RequiresDoctorClarification,
            GeneralNote = x.GeneralNote,
            Items = x.Items.OrderBy(i => i.Category).ThenBy(i => i.SortOrder).Select(i => new PrescriptionReviewItemResponse
            {
                Id = i.Id,
                CriterionId = i.CriterionId,
                Category = i.Category,
                CriterionCode = i.CriterionCodeSnapshot,
                CriterionName = i.CriterionNameSnapshot,
                Result = i.Result,
                Severity = i.Severity,
                Finding = i.Finding,
                Recommendation = i.Recommendation,
                IsSystemDetected = i.IsSystemDetected,
                SystemRuleCode = i.SystemRuleCode,
                PrescriptionItemId = i.PrescriptionItemId,
                PrescriptionCompoundId = i.PrescriptionCompoundId,
                PrescriptionCompoundItemId = i.PrescriptionCompoundItemId,
                ReviewedByUserId = i.ReviewedByUserId,
                ReviewedAt = i.ReviewedAt,
                SortOrder = i.SortOrder
            }).ToList(),
            Clarifications = x.Clarifications.OrderByDescending(c => c.RequestedAt).Select(Map).ToList()
        };

        private static PrescriptionClarificationResponse Map(TrxPrescriptionClarification x) => new()
        {
            Id = x.Id,
            PrescriptionId = x.PrescriptionId,
            PrescriptionReviewId = x.PrescriptionReviewId,
            PrescriptionReviewItemId = x.PrescriptionReviewItemId,
            ProblemCode = x.ProblemCode,
            ProblemDescription = x.ProblemDescription,
            PharmacistRecommendation = x.PharmacistRecommendation,
            Severity = x.Severity,
            Status = x.Status,
            RequestedByPharmacistId = x.RequestedByPharmacistId,
            RequestedAt = x.RequestedAt,
            RespondedByDoctorId = x.RespondedByDoctorId,
            RespondedAt = x.RespondedAt,
            DoctorResponse = x.DoctorResponse,
            ClosedByUserId = x.ClosedByUserId,
            ClosedAt = x.ClosedAt
        };
    }

    internal static class PrescriptionClarificationExtensions
    {
        public static void CoverageNoteSafeAppend(this TrxPrescriptionClarification entity, string? note)
        {
            if (string.IsNullOrWhiteSpace(note)) return;
            entity.DoctorResponse = string.IsNullOrWhiteSpace(entity.DoctorResponse)
                ? $"Catatan penutupan farmasi: {note.Trim()}"
                : $"{entity.DoctorResponse}\nCatatan penutupan farmasi: {note.Trim()}";
        }
    }
}
