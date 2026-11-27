using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Services
{
    internal sealed record OfferPrerequisiteEvaluation(
        bool CanCreateOffer,
        string? BlockingCode,
        string? BlockingMessage,
        bool HrCompleted,
        bool HrPassed,
        bool ClinicalRequired,
        bool ClinicalCompleted,
        bool ClinicalPassed,
        bool HasClinicalVeto);

    internal sealed record HandoffPrerequisiteEvaluation(
        bool IsEligible,
        string? BlockingCode,
        string? BlockingMessage);

    internal sealed record TalentPoolEligibilityEvaluation(
        bool IsEligible,
        int HttpStatusCode,
        string? BlockingCode,
        string? BlockingMessage,
        int QuotaSeats,
        int OccupiedSeats);

    internal static class RecruitmentJourneyPolicy
    {
        public static readonly TimeZoneInfo JakartaTimeZone = ResolveJakartaTimeZone();

        public static bool IsFastTrackApplication(HrdCandidateApplication app)
            => string.Equals(app.StageStatus, "FastTrackPromotedFromTalentPool", StringComparison.OrdinalIgnoreCase);

        public static bool IsEvaluationSubmitted(HrdCandidateEvaluation eval)
            => !eval.IsDelete && eval.UpdateDateTime.HasValue;

        public static HrdCandidateScreening CreateInitialPendingScreening(
            Guid applicationId,
            bool isClinicalVacancy,
            Guid actorUserId,
            string? notes = null)
        {
            return new HrdCandidateScreening
            {
                Id = Guid.NewGuid(),
                ApplicationId = applicationId,
                ScreeningDate = DateTime.UtcNow,
                Status = ScreeningStatus.Pending,
                IsStrRequired = isClinicalVacancy,
                ClinicalScreeningResult = ClinicalScreeningResult.NotApplicable,
                Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
                EvaluatedByUserId = actorUserId,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };
        }

        public static string ResolveClinicalScreeningResultLabel(
            ScreeningStatus status,
            bool isStrRequired,
            ClinicalScreeningResult result)
        {
            if (isStrRequired && status == ScreeningStatus.Pending && result == ClinicalScreeningResult.NotApplicable)
            {
                return "Menunggu Pemeriksaan STR";
            }

            return result switch
            {
                ClinicalScreeningResult.StrValid => "STR Valid",
                ClinicalScreeningResult.StrMissing => "STR Tidak Ditemukan",
                ClinicalScreeningResult.StrExpiredAutoReject => "STR Kedaluwarsa (Auto-Reject)",
                _ => isStrRequired ? "Memerlukan Verifikasi STR" : "Tidak Wajib STR (Non-Klinis)"
            };
        }

        public static List<string> GetMissingMandatoryGeneralDocuments(IEnumerable<HrdCandidateDocument> documents)
        {
            var available = documents
                .Where(d => !d.IsDelete)
                .Select(d => d.DocumentType)
                .ToHashSet();

            var missing = new List<string>();
            if (!available.Contains(DocumentType.IdentityCard))
            {
                missing.Add("KTP (Identitas)");
            }

            if (!available.Contains(DocumentType.CurriculumVitae))
            {
                missing.Add("CV / Daftar Riwayat Hidup");
            }

            if (!available.Contains(DocumentType.DiplomaCertificate))
            {
                missing.Add("Ijazah Pendidikan Terakhir");
            }

            return missing;
        }

        public static (bool HasStr, bool IsExpired, HrdCandidateDocument? SelectedStr) EvaluateClinicalStrDocument(
            IEnumerable<HrdCandidateDocument> documents,
            Guid applicationId,
            DateOnly today)
        {
            var strDoc = documents
                .Where(d => !d.IsDelete && d.DocumentType == DocumentType.StrCertificate)
                .OrderByDescending(d => d.ApplicationId == applicationId)
                .ThenByDescending(d => d.CreateDateTime)
                .ThenByDescending(d => d.Id)
                .FirstOrDefault();

            if (strDoc == null)
            {
                return (false, false, null);
            }

            var isExpired = strDoc.ValidUntil.HasValue && strDoc.ValidUntil.Value < today;
            return (true, isExpired, strDoc);
        }

        public static bool IsTerminalApplication(HrdCandidateApplication app, HrdCandidate? candidate = null)
        {
            if (candidate != null && candidate.Status == CandidateStatus.Blacklisted)
            {
                return true;
            }

            if (app.CurrentStage == ApplicationStage.Rejected || app.CurrentStage == ApplicationStage.Withdrawn)
            {
                return true;
            }

            return string.Equals(app.StageStatus, "REJECTED_ADMINISTRATION", StringComparison.OrdinalIgnoreCase)
                || string.Equals(app.StageStatus, "ASSESSMENT_FAILED", StringComparison.OrdinalIgnoreCase)
                || string.Equals(app.StageStatus, "MCU_UNFIT_OFFER_REVOKED", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsInactiveApplicationForCandidateStatus(HrdCandidateApplication app)
        {
            return IsTerminalApplication(app)
                || string.Equals(app.StageStatus, "OFFER_DECLINED", StringComparison.OrdinalIgnoreCase)
                || string.Equals(app.StageStatus, "HIRING_CANCELLED", StringComparison.OrdinalIgnoreCase);
        }

        public static OfferPrerequisiteEvaluation EvaluateOfferPrerequisites(
            HrdCandidateApplication app,
            HrdJobVacancy? vacancy,
            HrdCandidateScreening? screening,
            IEnumerable<HrdCandidateEvaluation> evaluations,
            HrdCandidate? candidate = null)
        {
            var isClinical = vacancy?.IsClinical ?? (screening?.IsStrRequired ?? false);

            if (candidate != null && candidate.Status == CandidateStatus.Blacklisted)
            {
                return new OfferPrerequisiteEvaluation(
                    false,
                    "CANDIDATE_BLACKLISTED",
                    "Pembuatan penawaran kerja ditolak: status kandidat termasuk daftar hitam (Blacklisted).",
                    false, false, isClinical, false, !isClinical, false);
            }

            var activeEvals = evaluations.Where(e => !e.IsDelete).ToList();
            var hasClinicalVeto = activeEvals.Any(e =>
                e.IsClinicalVetoApplied ||
                (IsEvaluationSubmitted(e)
                 && (e.EvaluationType == EvaluationType.ClinicalAssessment || e.EvaluationType == EvaluationType.UserInterview)
                 && e.Recommendation == EvaluationRecommendation.NotRecommended));

            var latestSubmittedHr = activeEvals
                .Where(e => e.EvaluationType == EvaluationType.HrInterview && IsEvaluationSubmitted(e))
                .OrderByDescending(e => e.UpdateDateTime)
                .ThenByDescending(e => e.CreateDateTime)
                .FirstOrDefault();

            var latestSubmittedClinical = activeEvals
                .Where(e => (e.EvaluationType == EvaluationType.ClinicalAssessment || e.EvaluationType == EvaluationType.UserInterview)
                            && IsEvaluationSubmitted(e))
                .OrderByDescending(e => e.UpdateDateTime)
                .ThenByDescending(e => e.CreateDateTime)
                .FirstOrDefault();

            var hrCompleted = latestSubmittedHr != null;
            var hrPassed = latestSubmittedHr != null && latestSubmittedHr.Recommendation != EvaluationRecommendation.NotRecommended;

            var clinicalCompleted = latestSubmittedClinical != null;
            var clinicalPassed = !isClinical || (latestSubmittedClinical != null
                                                 && !hasClinicalVeto
                                                 && latestSubmittedClinical.Recommendation != EvaluationRecommendation.NotRecommended);

            if (app.CurrentStage == ApplicationStage.Withdrawn)
            {
                return new OfferPrerequisiteEvaluation(
                    false,
                    "APPLICATION_WITHDRAWN",
                    "Lamaran telah ditarik mundur (Withdrawn) dan tidak dapat diproses ke tahap penawaran.",
                    hrCompleted, hrPassed, isClinical, clinicalCompleted, clinicalPassed, hasClinicalVeto);
            }

            if (hasClinicalVeto || string.Equals(app.StageStatus, "ASSESSMENT_FAILED", StringComparison.OrdinalIgnoreCase))
            {
                return new OfferPrerequisiteEvaluation(
                    false,
                    hasClinicalVeto ? "CLINICAL_VETO_APPLIED" : "EVALUATION_FAILED",
                    hasClinicalVeto
                        ? "Pembuatan penawaran ditolak secara permanen karena adanya Hak Veto Klinis Mutlak dari Asesor Medis (VR-REC-11, HRD-DEC-077)."
                        : "Pembuatan penawaran ditolak: kandidat dinyatakan tidak lulus pada tahap evaluasi (ASSESSMENT_FAILED).",
                    hrCompleted, hrPassed, isClinical, clinicalCompleted, clinicalPassed, hasClinicalVeto);
            }

            if (string.Equals(app.StageStatus, "REJECTED_ADMINISTRATION", StringComparison.OrdinalIgnoreCase)
                || app.CurrentStage == ApplicationStage.Rejected)
            {
                return new OfferPrerequisiteEvaluation(
                    false,
                    "SCREENING_REJECTED",
                    "Pembuatan penawaran ditolak: kandidat telah gugur pada tahap penapisan administrasi atau peninjauan sebelumnya.",
                    hrCompleted, hrPassed, isClinical, clinicalCompleted, clinicalPassed, hasClinicalVeto);
            }

            if (IsFastTrackApplication(app))
            {
                return new OfferPrerequisiteEvaluation(
                    true, null, null, hrCompleted, hrPassed, isClinical, clinicalCompleted, clinicalPassed, false);
            }

            if (screening == null || screening.IsDelete || screening.Status != ScreeningStatus.Passed)
            {
                return new OfferPrerequisiteEvaluation(
                    false,
                    "SCREENING_NOT_PASSED",
                    "Pembuatan penawaran ditolak: Penapisan Administrasi (Screening) harus berstatus Lulus (Passed) terlebih dahulu.",
                    hrCompleted, hrPassed, isClinical, clinicalCompleted, clinicalPassed, hasClinicalVeto);
            }

            if (!hrCompleted)
            {
                return new OfferPrerequisiteEvaluation(
                    false,
                    "HR_EVALUATION_NOT_SUBMITTED",
                    "Pembuatan penawaran ditolak: hasil evaluasi Wawancara HR wajib dikirim terlebih dahulu sebelum membuat penawaran (VR-REC-12).",
                    hrCompleted, hrPassed, isClinical, clinicalCompleted, clinicalPassed, hasClinicalVeto);
            }

            if (!hrPassed)
            {
                return new OfferPrerequisiteEvaluation(
                    false,
                    "HR_EVALUATION_NOT_RECOMMENDED",
                    "Pembuatan penawaran ditolak: hasil Wawancara HR menyatakan kandidat Tidak Direkomendasikan (NotRecommended).",
                    hrCompleted, hrPassed, isClinical, clinicalCompleted, clinicalPassed, hasClinicalVeto);
            }

            if (isClinical && !clinicalCompleted)
            {
                return new OfferPrerequisiteEvaluation(
                    false,
                    "CLINICAL_EVALUATION_NOT_SUBMITTED",
                    "Pembuatan penawaran ditolak: lowongan klinis wajib menyelesaikan dan mengirim hasil Asesmen Klinis terlebih dahulu (VR-REC-12, HRD-DEC-077).",
                    hrCompleted, hrPassed, isClinical, clinicalCompleted, clinicalPassed, hasClinicalVeto);
            }

            if (isClinical && !clinicalPassed)
            {
                return new OfferPrerequisiteEvaluation(
                    false,
                    "CLINICAL_EVALUATION_FAILED",
                    "Pembuatan penawaran ditolak: kandidat tidak memenuhi kelulusan Asesmen Klinis.",
                    hrCompleted, hrPassed, isClinical, clinicalCompleted, clinicalPassed, hasClinicalVeto);
            }

            return new OfferPrerequisiteEvaluation(
                true, null, null, hrCompleted, hrPassed, isClinical, clinicalCompleted, clinicalPassed, hasClinicalVeto);
        }

        public static HandoffPrerequisiteEvaluation EvaluateHandoffPrerequisites(
            HrdCandidateApplication app,
            HrdJobOffer offer,
            HrdPreEmploymentMedicalCheck? mcu,
            HrdCandidate? candidate = null)
        {
            if (IsTerminalApplication(app, candidate))
            {
                return new HandoffPrerequisiteEvaluation(
                    false,
                    "APPLICATION_TERMINAL",
                    "Serah terima ditolak: status lamaran atau kandidat berada pada status terminal/gugur.");
            }

            if (offer.Status == JobOfferStatus.Revoked || offer.Status == JobOfferStatus.ConditionalRejected || offer.Status == JobOfferStatus.FinalRejected || offer.Status == JobOfferStatus.RejectedByApprover || offer.Status == JobOfferStatus.Expired)
            {
                return new HandoffPrerequisiteEvaluation(
                    false,
                    "OFFER_NOT_ACCEPTED",
                    $"Serah terima ditolak: status penawaran kerja adalah {offer.Status}.");
            }

            if (mcu == null || mcu.IsDelete || mcu.ClearanceResult == McuClearanceResult.Pending)
            {
                return new HandoffPrerequisiteEvaluation(
                    false,
                    "MCU_PENDING_OR_MISSING",
                    "Serah terima onboarding hanya dapat diproses setelah hasil pemeriksaan kesehatan pra-kerja (MCU) dinyatakan Laik Kerja (FIT) (VR-REC-20).");
            }

            if (mcu.ClearanceResult == McuClearanceResult.Unfit)
            {
                return new HandoffPrerequisiteEvaluation(
                    false,
                    "MCU_UNFIT",
                    "Serah terima onboarding diblokir permanen karena hasil MCU Pra-Kerja menyatakan Tidak Laik Kerja (UNFIT).");
            }

            if (mcu.ClearanceResult == McuClearanceResult.FitWithRestrictions)
            {
                return new HandoffPrerequisiteEvaluation(
                    false,
                    "MCU_FIT_WITH_RESTRICTIONS_CONFIRMATION_REQUIRED",
                    "Hasil MCU menyatakan Fit With Restrictions (Laik dengan Catatan Pembatasan), namun konfirmasi penyesuaian kerja belum tersedia sehingga serah terima onboarding ditahan sementara.");
            }

            if (offer.Status != JobOfferStatus.FinalAccepted)
            {
                return new HandoffPrerequisiteEvaluation(
                    false,
                    "OFFER_NOT_FINAL_ACCEPTED",
                    $"Serah terima onboarding memerlukan persetujuan final penawaran (FinalAccepted). Status saat ini: {offer.Status} (VR-REC-20).");
            }

            return new HandoffPrerequisiteEvaluation(true, null, null);
        }

        public static bool IsOfferOccupyingVacancySeat(JobOfferStatus status)
            => status != JobOfferStatus.RejectedByApprover
               && status != JobOfferStatus.ConditionalRejected
               && status != JobOfferStatus.FinalRejected
               && status != JobOfferStatus.Revoked
               && status != JobOfferStatus.Expired;

        public static async Task<Dictionary<Guid, int>> CountOccupiedSeatsByVacancyAsync(
            ApplicationDbContext dbContext,
            IEnumerable<Guid> vacancyIds,
            CancellationToken cancellationToken = default)
        {
            var idList = vacancyIds.Where(v => v != Guid.Empty).Distinct().ToList();
            if (idList.Count == 0)
            {
                return new Dictionary<Guid, int>();
            }

            var activeOfferAppIds = await dbContext.HrdJobOffers
                .AsNoTracking()
                .Include(o => o.Application)
                .Where(o => !o.IsDelete
                            && o.Application != null
                            && !o.Application.IsDelete
                            && idList.Contains(o.Application.VacancyId)
                            && o.Status != JobOfferStatus.RejectedByApprover
                            && o.Status != JobOfferStatus.ConditionalRejected
                            && o.Status != JobOfferStatus.FinalRejected
                            && o.Status != JobOfferStatus.Revoked
                            && o.Status != JobOfferStatus.Expired)
                .Select(o => new { o.Application!.VacancyId, o.ApplicationId })
                .ToListAsync(cancellationToken);

            var handoffAppIds = await dbContext.HrdCandidateHiringHandoffs
                .AsNoTracking()
                .Include(h => h.Offer)
                    .ThenInclude(o => o!.Application)
                .Where(h => !h.IsDelete
                            && h.Status != HandoffStatus.Cancelled
                            && h.Offer != null
                            && h.Offer.Application != null
                            && !h.Offer.Application.IsDelete
                            && idList.Contains(h.Offer.Application.VacancyId))
                .Select(h => new { h.Offer!.Application!.VacancyId, h.Offer.ApplicationId })
                .ToListAsync(cancellationToken);

            return activeOfferAppIds
                .Concat(handoffAppIds)
                .GroupBy(x => x.VacancyId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.ApplicationId).Distinct().Count());
        }

        public static TalentPoolEligibilityEvaluation EvaluateTalentPoolEligibility(
            HrdCandidate? candidate,
            HrdJobVacancy? vacancy,
            HrdCandidateApplication? app,
            HrdCandidateScreening? screening,
            IEnumerable<HrdCandidateEvaluation> evaluations,
            IEnumerable<HrdJobOffer> offersForApplication,
            HrdPreEmploymentMedicalCheck? mcuForLatestOffer,
            bool hasActiveHandoffOnApplication,
            bool hasActiveTalentPoolRecord,
            int occupiedSeatsOnVacancy,
            bool hasCreatePermission = true)
        {
            var quotaSeats = vacancy?.QuotaSeats ?? 0;

            // 1. Wajib memiliki kandidat, lowongan asal, dan lamaran yang sah pada lowongan asal
            if (candidate == null || candidate.IsDelete)
            {
                return new TalentPoolEligibilityEvaluation(
                    false, 422, "CANDIDATE_NOT_FOUND", "Kandidat tidak ditemukan atau tidak aktif.", quotaSeats, occupiedSeatsOnVacancy);
            }

            if (vacancy == null || vacancy.IsDelete)
            {
                return new TalentPoolEligibilityEvaluation(
                    false, 422, "VACANCY_NOT_FOUND", "Lowongan asal tidak ditemukan atau tidak aktif.", quotaSeats, occupiedSeatsOnVacancy);
            }

            if (app == null || app.IsDelete || app.CandidateId != candidate.Id || app.VacancyId != vacancy.Id)
            {
                return new TalentPoolEligibilityEvaluation(
                    false,
                    422,
                    "ORIGIN_APPLICATION_MISSING",
                    "Kandidat tidak memiliki lamaran yang sah pada lowongan asal untuk dimasukkan ke Talent Pool.",
                    quotaSeats,
                    occupiedSeatsOnVacancy);
            }

            // 6. Kandidat / lamaran tidak berstatus terminal atau sudah masuk proses serah terima onboarding
            if (candidate.Status == CandidateStatus.Blacklisted || candidate.Status == CandidateStatus.Hired)
            {
                return new TalentPoolEligibilityEvaluation(
                    false,
                    422,
                    "CANDIDATE_TERMINAL_STATUS",
                    $"Kandidat berstatus '{candidate.Status}' tidak memenuhi syarat masuk Talent Pool.",
                    quotaSeats,
                    occupiedSeatsOnVacancy);
            }

            if (IsInactiveApplicationForCandidateStatus(app))
            {
                return new TalentPoolEligibilityEvaluation(
                    false,
                    422,
                    "APPLICATION_TERMINAL_STATUS",
                    "Lamaran berada pada status terminal/gugur dan tidak memenuhi syarat masuk Talent Pool.",
                    quotaSeats,
                    occupiedSeatsOnVacancy);
            }

            if (mcuForLatestOffer != null && !mcuForLatestOffer.IsDelete && mcuForLatestOffer.ClearanceResult == McuClearanceResult.Unfit)
            {
                return new TalentPoolEligibilityEvaluation(
                    false,
                    422,
                    "MCU_UNFIT",
                    "Hasil MCU Pra-Kerja menyatakan Tidak Laik Kerja (UNFIT) sehingga kandidat tidak memenuhi syarat Talent Pool.",
                    quotaSeats,
                    occupiedSeatsOnVacancy);
            }

            if (hasActiveHandoffOnApplication || app.CurrentStage == ApplicationStage.Handoff)
            {
                return new TalentPoolEligibilityEvaluation(
                    false,
                    422,
                    "HANDOFF_ACTIVE",
                    "Lamaran sedang atau sudah diproses pada tahap Serah Terima / Onboarding dan tidak dapat didaftarkan ke Talent Pool.",
                    quotaSeats,
                    occupiedSeatsOnVacancy);
            }

            // 2, 3, 4. Lulus Seleksi Administrasi, memenuhi evaluasi wajib, dan bebas veto klinis (HRD-DEC-080)
            var activeOffers = offersForApplication.Where(o => !o.IsDelete).ToList();
            var offerPrereq = EvaluateOfferPrerequisites(app, vacancy, screening, evaluations, candidate);
            if (!offerPrereq.CanCreateOffer || offerPrereq.HasClinicalVeto)
            {
                return new TalentPoolEligibilityEvaluation(
                    false,
                    422,
                    offerPrereq.BlockingCode ?? "PREREQUISITE_NOT_MET",
                    offerPrereq.BlockingMessage ?? "Kandidat belum memenuhi seluruh tahapan seleksi tanpa veto untuk masuk Talent Pool (HRD-DEC-080).",
                    quotaSeats,
                    occupiedSeatsOnVacancy);
            }

            // 7. Tidak sedang memiliki penawaran aktif yang bertentangan (termasuk ConditionalAccepted) atau penawaran terminal
            var conflictingActiveOffer = activeOffers
                .Where(o => IsOfferOccupyingVacancySeat(o.Status))
                .OrderByDescending(o => o.CreateDateTime)
                .FirstOrDefault();
            if (conflictingActiveOffer != null)
            {
                var blockingCode = conflictingActiveOffer.Status == JobOfferStatus.ConditionalAccepted
                    ? "OFFER_CONDITIONAL_ACCEPTED_CONFLICT"
                    : "ACTIVE_OFFER_CONFLICT";
                return new TalentPoolEligibilityEvaluation(
                    false,
                    422,
                    blockingCode,
                    $"Kandidat sedang memiliki surat penawaran aktif ({conflictingActiveOffer.OfferNumber} — {conflictingActiveOffer.Status}) pada lamaran ini sehingga tidak boleh diarahkan ke Talent Pool.",
                    quotaSeats,
                    occupiedSeatsOnVacancy);
            }

            var terminatedOffer = activeOffers
                .Where(o => o.Status == JobOfferStatus.ConditionalRejected
                            || o.Status == JobOfferStatus.FinalRejected
                            || o.Status == JobOfferStatus.Expired
                            || o.Status == JobOfferStatus.Revoked)
                .OrderByDescending(o => o.CreateDateTime)
                .FirstOrDefault();
            if (terminatedOffer != null)
            {
                return new TalentPoolEligibilityEvaluation(
                    false,
                    422,
                    "OFFER_TERMINAL_STATUS",
                    $"Penawaran pada lamaran ini berada pada status terminal ({terminatedOffer.Status}) sehingga kandidat tidak memenuhi syarat Talent Pool.",
                    quotaSeats,
                    occupiedSeatsOnVacancy);
            }

            // 5. Kuota lowongan asal harus sudah teralokasi penuh
            if (quotaSeats > 0 && occupiedSeatsOnVacancy < quotaSeats)
            {
                return new TalentPoolEligibilityEvaluation(
                    false,
                    422,
                    "VACANCY_QUOTA_NOT_FULL",
                    $"Kuota kursi lowongan '{vacancy.Title}' belum terisi penuh ({occupiedSeatsOnVacancy}/{quotaSeats}); proses kandidat melalui Penawaran Kerja terlebih dahulu (HRD-DEC-080).",
                    quotaSeats,
                    occupiedSeatsOnVacancy);
            }

            // 8. Belum memiliki record Talent Pool aktif yang menyebabkan duplikasi
            if (hasActiveTalentPoolRecord)
            {
                return new TalentPoolEligibilityEvaluation(
                    false,
                    409,
                    "TALENT_POOL_ALREADY_ACTIVE",
                    "Kandidat sudah aktif terdaftar di Talent Pool.",
                    quotaSeats,
                    occupiedSeatsOnVacancy);
            }

            // 9. Pengguna memiliki permission yang sesuai
            if (!hasCreatePermission)
            {
                return new TalentPoolEligibilityEvaluation(
                    false,
                    403,
                    "TALENT_POOL_CREATE_FORBIDDEN",
                    "Pengguna tidak memiliki izin TalentPool.Create untuk mendaftarkan kandidat ke Talent Pool.",
                    quotaSeats,
                    occupiedSeatsOnVacancy);
            }

            return new TalentPoolEligibilityEvaluation(true, 200, null, null, quotaSeats, occupiedSeatsOnVacancy);
        }

        public static string MaskNationalIdNumber(string? nik)
        {
            if (string.IsNullOrWhiteSpace(nik))
            {
                return "-";
            }

            var trimmed = nik.Trim();
            if (trimmed.Length <= 8)
            {
                return new string('*', trimmed.Length);
            }

            var visibleStart = trimmed[..4];
            var visibleEnd = trimmed[^4..];
            return $"{visibleStart}{new string('*', trimmed.Length - 8)}{visibleEnd}";
        }

        public static async Task<string> AllocateSequenceAsync(
            NumberSeriesAllocator? allocator,
            IEnumerable<string> existingNumbersForDay,
            string sequenceKey,
            string prefix,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var instant = DateTimeOffset.UtcNow;
            var dateCode = TimeZoneInfo.ConvertTime(instant, JakartaTimeZone).ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            var fullPrefix = $"{prefix}-{dateCode}-";
            var legacyHighWatermark = ExtractMaxSequence(existingNumbersForDay, fullPrefix.Length);

            if (allocator == null)
            {
                return $"{fullPrefix}{(legacyHighWatermark + 1):D4}";
            }

            return await allocator.AllocateAsync(
                new NumberAllocationRequest(
                    SequenceKey: sequenceKey,
                    Prefix: prefix,
                    ResetPolicy: NumberSeriesResetPolicies.Daily,
                    SequenceDigits: 4,
                    ActorUserId: actorUserId,
                    Instant: instant,
                    MinimumValue: legacyHighWatermark),
                cancellationToken);
        }

        public static long ExtractMaxSequence(IEnumerable<string> numbers, int prefixLength)
        {
            return numbers
                .Select(n => !string.IsNullOrWhiteSpace(n)
                             && n.Length > prefixLength
                             && long.TryParse(n[prefixLength..], NumberStyles.None, CultureInfo.InvariantCulture, out var parsed)
                    ? parsed
                    : 0L)
                .DefaultIfEmpty(0L)
                .Max();
        }

        private static TimeZoneInfo ResolveJakartaTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            }
            catch
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById("Asia/Jakarta");
                }
                catch
                {
                    return TimeZoneInfo.Utc;
                }
            }
        }
    }
}
