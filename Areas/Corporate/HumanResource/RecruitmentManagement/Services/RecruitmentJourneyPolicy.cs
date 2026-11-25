using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;

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

    internal static class RecruitmentJourneyPolicy
    {
        public static readonly TimeZoneInfo JakartaTimeZone = ResolveJakartaTimeZone();

        public static bool IsFastTrackApplication(HrdCandidateApplication app)
            => string.Equals(app.StageStatus, "FastTrackPromotedFromTalentPool", StringComparison.OrdinalIgnoreCase);

        public static bool IsEvaluationSubmitted(HrdCandidateEvaluation eval)
            => !eval.IsDelete && eval.UpdateDateTime.HasValue;

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
