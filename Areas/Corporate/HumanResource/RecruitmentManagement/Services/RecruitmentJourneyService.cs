using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Security;
using QuilvianSystemBackend.Shared.HumanResource.Helpers;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Services
{
    public sealed class RecruitmentJourneyService : IRecruitmentJourneyService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly AccessPermissionService _permissionService;

        public RecruitmentJourneyService(
            ApplicationDbContext dbContext,
            AccessPermissionService permissionService)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
        }

        public async Task<RecruitmentResult<RecruitmentJourneyResponse>> GetApplicationJourneyAsync(
            Guid applicationId,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            if (applicationId == Guid.Empty)
            {
                return RecruitmentResult<RecruitmentJourneyResponse>.Fail("ID lamaran tidak valid.", 400);
            }

            var actorUserId = user.GetUserId();

            var app = await _dbContext.HrdCandidateApplications
                .AsNoTracking()
                .Include(a => a.Candidate)
                .Include(a => a.Vacancy)
                    .ThenInclude(v => v!.Requisition)
                        .ThenInclude(r => r!.HospitalSite)
                .Include(a => a.Vacancy)
                    .ThenInclude(v => v!.Requisition)
                        .ThenInclude(r => r!.OrganizationUnit)
                .Include(a => a.Vacancy)
                    .ThenInclude(v => v!.Requisition)
                        .ThenInclude(r => r!.Position)
                .FirstOrDefaultAsync(a => a.Id == applicationId && !a.IsDelete, cancellationToken);

            if (app == null || app.Candidate == null || app.Candidate.IsDelete)
            {
                return RecruitmentResult<RecruitmentJourneyResponse>.NotFound("Data lamaran kandidat tidak ditemukan.");
            }

            var candidate = app.Candidate;
            var vacancy = app.Vacancy;
            var requisition = vacancy?.Requisition;
            var isClinical = vacancy?.IsClinical ?? false;
            var isFastTrack = RecruitmentJourneyPolicy.IsFastTrackApplication(app);

            // Hitung jumlah lamaran lain milik kandidat yang sama untuk konteks switcher multi-lamaran
            var otherApplicationsCount = await _dbContext.HrdCandidateApplications
                .AsNoTracking()
                .CountAsync(a => a.CandidateId == candidate.Id && a.Id != app.Id && !a.IsDelete, cancellationToken);

            // Evaluasi seluruh hak akses pengguna berbasis registri permission
            var perms = await ResolvePermissionsAsync(user);

            // Muat domain terkait secara terisolasi untuk ApplicationId ini (AsNoTracking read-only)
            var documents = await _dbContext.HrdCandidateDocuments
                .AsNoTracking()
                .Where(d => d.CandidateId == candidate.Id && (d.ApplicationId == null || d.ApplicationId == app.Id) && !d.IsDelete)
                .ToListAsync(cancellationToken);

            var screening = await _dbContext.HrdCandidateScreenings
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ApplicationId == app.Id && !s.IsDelete, cancellationToken);

            var evaluations = await _dbContext.HrdCandidateEvaluations
                .AsNoTracking()
                .Where(e => e.ApplicationId == app.Id && !e.IsDelete)
                .OrderBy(e => e.CreateDateTime)
                .ToListAsync(cancellationToken);

            var appOffers = await _dbContext.HrdJobOffers
                .AsNoTracking()
                .Include(o => o.SalaryGrade)
                .Include(o => o.ContractType)
                .Include(o => o.Approvals.Where(ap => !ap.IsDelete))
                .Where(o => o.ApplicationId == app.Id && !o.IsDelete)
                .OrderByDescending(o => o.CreateDateTime)
                .ToListAsync(cancellationToken);

            var offer = appOffers.FirstOrDefault();

            var mcu = offer == null
                ? null
                : await _dbContext.HrdPreEmploymentMedicalChecks
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.OfferId == offer.Id && !m.IsDelete, cancellationToken);

            var handoff = offer == null
                ? null
                : await _dbContext.HrdCandidateHiringHandoffs
                    .AsNoTracking()
                    .Include(h => h.WorkforceProfile)
                    .Include(h => h.HeadcountRequestSeat)
                    .FirstOrDefaultAsync(h => h.OfferId == offer.Id && !h.IsDelete, cancellationToken);

            var onboardingCase = handoff == null
                ? null
                : await _dbContext.HrdOnboardingCases
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.HiringHandoffId == handoff.Id && !c.IsDelete, cancellationToken);

            var offerIdsForApp = appOffers.Select(o => o.Id).ToList();
            var hasActiveHandoffOnApp = (handoff != null && !handoff.IsDelete && handoff.Status != HandoffStatus.Cancelled)
                || (offerIdsForApp.Count > 0 && await _dbContext.HrdCandidateHiringHandoffs
                    .AsNoTracking()
                    .AnyAsync(h => !h.IsDelete && h.Status != HandoffStatus.Cancelled && offerIdsForApp.Contains(h.OfferId), cancellationToken));

            // Cek keberadaan record Talent Pool aktif terlepas dari izin baca (memisahkan validasi bisnis vs izin baca UI)
            var hasActiveTalentPool = await _dbContext.HrdTalentPools
                .AsNoTracking()
                .AnyAsync(t => t.CandidateId == candidate.Id
                               && t.Status == TalentPoolStatus.Active
                               && !t.IsDelete, cancellationToken);

            var talentPool = await _dbContext.HrdTalentPools
                .AsNoTracking()
                .Where(t => t.CandidateId == candidate.Id && !t.IsDelete &&
                            ((offer != null && t.PromotedToOfferId == offer.Id) ||
                             (vacancy != null && t.OriginalVacancyId == vacancy.Id)))
                .OrderByDescending(t => t.EnrolledDate)
                .FirstOrDefaultAsync(cancellationToken);

            var occupiedByVacancy = vacancy != null
                ? await RecruitmentJourneyPolicy.CountOccupiedSeatsByVacancyAsync(_dbContext, new[] { vacancy.Id }, cancellationToken)
                : new Dictionary<Guid, int>();
            var occupiedSeatsOnVacancy = vacancy != null && occupiedByVacancy.TryGetValue(vacancy.Id, out var occCount) ? occCount : 0;
            var vacancyQuotaFull = vacancy != null && vacancy.QuotaSeats > 0 && occupiedSeatsOnVacancy >= vacancy.QuotaSeats;

            var offerPrereq = RecruitmentJourneyPolicy.EvaluateOfferPrerequisites(app, vacancy, screening, evaluations, candidate);
            var handoffPrereq = offer == null
                ? new HandoffPrerequisiteEvaluation(false, "OFFER_MISSING", "Surat penawaran kerja belum dibuat.")
                : RecruitmentJourneyPolicy.EvaluateHandoffPrerequisites(app, offer, mcu, candidate);
            var talentPoolEligibility = RecruitmentJourneyPolicy.EvaluateTalentPoolEligibility(
                candidate,
                vacancy,
                app,
                screening,
                evaluations,
                appOffers,
                mcu,
                hasActiveHandoffOnApp,
                hasActiveTalentPool,
                occupiedSeatsOnVacancy,
                perms.CanCreateTalentPool);

            var blockers = new List<JourneyBlockerDto>();
            var allowedActions = new List<JourneyActionDto>();
            var missingOrRestricted = new List<string>();
            var warnings = new List<string>();

            var upstreamBlockedReason = ResolveUpstreamFailureReason(app, candidate, screening, offerPrereq, offer, mcu, handoff, onboardingCase);
            var isTerminalOrFailed = upstreamBlockedReason != null;

            // --- Bangun 6 Tahap Tetap ---
            var registrationStage = BuildRegistrationStage(app, candidate, vacancy, documents, isClinical, isFastTrack, perms, blockers, allowedActions, warnings);
            var screeningStage = BuildScreeningStage(app, screening, isClinical, isFastTrack, isTerminalOrFailed, upstreamBlockedReason, perms, missingOrRestricted, blockers, allowedActions);
            var evaluationStage = BuildEvaluationStage(app, screening, evaluations, offerPrereq, isClinical, isFastTrack, isTerminalOrFailed, upstreamBlockedReason, actorUserId, perms, missingOrRestricted, blockers, allowedActions, warnings);
            var offerStage = BuildOfferStage(app, vacancy, offer, offerPrereq, vacancyQuotaFull, occupiedSeatsOnVacancy, isTerminalOrFailed, upstreamBlockedReason, actorUserId, perms, missingOrRestricted, blockers, allowedActions);
            var mcuStage = BuildMcuStage(app, offer, mcu, isTerminalOrFailed, upstreamBlockedReason, perms, missingOrRestricted, blockers, allowedActions, warnings);
            var handoffStage = BuildHandoffStage(app, candidate, offer, mcu, handoff, onboardingCase, handoffPrereq, isTerminalOrFailed, upstreamBlockedReason, perms, missingOrRestricted, blockers, allowedActions, warnings);

            var stages = new List<JourneyStageDto>
            {
                registrationStage,
                screeningStage,
                evaluationStage,
                offerStage,
                mcuStage,
                handoffStage
            };

            // Tentukan tahap aktif & outcome agregat
            var (currentStageKey, currentStageLabel) = ResolveCurrentVisualStage(
                app,
                screening,
                offerPrereq,
                offer,
                mcu,
                handoff,
                onboardingCase,
                handoffPrereq,
                vacancyQuotaFull,
                isTerminalOrFailed,
                isFastTrack);
            foreach (var stage in stages)
            {
                stage.IsCurrentStage = stage.StageKey == currentStageKey;
            }

            var (journeyOutcome, journeyOutcomeLabel) = ResolveJourneyOutcome(app, candidate, screening, offerPrereq, offer, mcu, handoff, onboardingCase);

            // Hanya tawarkan aksi Talent Pool bila memenuhi seluruh 9 syarat kebijakan Talent Pool (BE-HRD-FIN-REC-021)
            if (talentPoolEligibility.IsEligible)
            {
                allowedActions.Add(new JourneyActionDto
                {
                    ActionCode = "ENROLL_TALENT_POOL",
                    Label = "Masukkan ke Talent Pool",
                    StageKey = currentStageKey,
                    ActionType = "NAVIGATE",
                    Method = "POST",
                    Endpoint = "/api/v1/corporate/human-resource/recruitment/talent-pool/enroll",
                    RoutePath = $"/hr/recruitment/talent-pool/create?candidateId={candidate.Id}&vacancyId={app.VacancyId}&applicationId={app.Id}",
                    RequiredPermission = "TalentPool.Create",
                    Description = $"Kuota lowongan asal terisi penuh ({occupiedSeatsOnVacancy}/{vacancy?.QuotaSeats ?? 0}). Simpan kandidat cadangan berkualifikasi ke Talent Pool."
                });
            }

            if (perms.CanPromoteTalentPool && talentPool != null && talentPool.Status == TalentPoolStatus.Active && !talentPool.IsDelete)
            {
                allowedActions.Add(new JourneyActionDto
                {
                    ActionCode = "FAST_TRACK_PROMOTE_TALENT_POOL",
                    Label = "Promosi Cepat dari Talent Pool",
                    StageKey = currentStageKey,
                    ActionType = "NAVIGATE",
                    RoutePath = $"/hr/recruitment/talent-pool/{talentPool.Id}/promote?candidateId={candidate.Id}&applicationId={app.Id}",
                    RequiredPermission = "TalentPool.Promote",
                    Description = "Promosikan kandidat cadangan langsung ke tahap penawaran lowongan terbuka."
                });
            }

            var nextAction = ResolveNextAction(
                stages,
                allowedActions,
                currentStageKey,
                journeyOutcome,
                screening,
                offerPrereq,
                offer,
                mcu,
                handoff,
                onboardingCase,
                handoffPrereq,
                talentPool,
                talentPoolEligibility,
                vacancyQuotaFull,
                isFastTrack,
                isClinical,
                candidate.Id,
                app.Id,
                actorUserId,
                perms);
            var history = BuildHistory(app, screening, evaluations, offer, mcu, handoff, onboardingCase, talentPool, actorUserId, perms);

            if (vacancy == null)
            {
                missingOrRestricted.Add("Vacancy");
                warnings.Add("Data master lowongan terkait tidak ditemukan atau telah dihapus lunak.");
            }

            var completenessStatus = missingOrRestricted.Count == 0
                ? "COMPLETE"
                : stages.Any(s => s.IsRestricted) ? "RESTRICTED" : "PARTIAL";

            var response = new RecruitmentJourneyResponse
            {
                ApplicationId = app.Id,
                CandidateId = candidate.Id,
                ApplicationNumber = app.ApplicationNumber,
                ApplicationDate = app.ApplicationDate,
                CandidateSummary = new JourneyCandidateSummaryDto
                {
                    CandidateId = candidate.Id,
                    CandidateCode = candidate.CandidateCode,
                    FullName = candidate.FullName,
                    MaskedNationalIdNumber = RecruitmentJourneyPolicy.MaskNationalIdNumber(candidate.NationalIdNumber),
                    Email = candidate.Email,
                    PhoneNumber = candidate.PhoneNumber,
                    IsRehire = candidate.IsRehire,
                    PreviousWorkforceProfileId = candidate.PreviousWorkforceProfileId,
                    Status = candidate.Status.ToString()
                },
                VacancySummary = new JourneyVacancySummaryDto
                {
                    VacancyId = app.VacancyId,
                    VacancyNumber = vacancy?.VacancyNumber ?? "-",
                    Title = vacancy?.Title ?? "-",
                    IsClinical = isClinical,
                    Status = vacancy?.Status.ToString() ?? RecruitmentJourneyStageStatuses.Unknown,
                    QuotaSeats = vacancy?.QuotaSeats ?? 0,
                    PublishDate = vacancy?.PublishDate,
                    ClosingDate = vacancy?.ClosingDate,
                    RequisitionId = vacancy?.RequisitionId,
                    RequisitionNumber = requisition?.RequisitionNumber,
                    HeadcountRequestId = requisition?.HeadcountRequestId,
                    HospitalSiteName = requisition?.HospitalSite?.SiteName,
                    OrganizationUnitName = requisition?.OrganizationUnit?.UnitName,
                    PositionName = requisition?.Position?.PositionName
                },
                CurrentStage = app.CurrentStage.ToString(),
                CurrentStageKey = currentStageKey,
                CurrentStageLabel = currentStageLabel,
                StageStatus = app.StageStatus,
                CandidateStatus = candidate.Status.ToString(),
                JourneyOutcome = journeyOutcome,
                JourneyOutcomeLabel = journeyOutcomeLabel,
                IsFastTrackFromTalentPool = isFastTrack,
                OtherApplicationsCount = otherApplicationsCount,
                Stages = stages,
                AllowedActions = allowedActions,
                Blockers = blockers,
                NextAction = nextAction,
                History = history,
                HandoffReference = perms.CanReadHandoff && handoff != null
                    ? new JourneyHandoffRefDto
                    {
                        HandoffId = handoff.Id,
                        HandoffNumber = handoff.HandoffNumber,
                        Status = handoff.Status.ToString(),
                        HandoffDate = handoff.HandoffDate,
                        WorkforceProfileId = handoff.WorkforceProfileId,
                        WorkforceProfileCode = handoff.WorkforceProfile?.ProfileCode,
                        HeadcountRequestSeatId = handoff.HeadcountRequestSeatId,
                        SeatNumber = handoff.HeadcountRequestSeat?.SeatNumber
                    }
                    : null,
                OnboardingCaseReference = onboardingCase == null
                    ? null
                    : perms.CanReadOnboarding
                        ? new JourneyOnboardingRefDto
                        {
                            OnboardingCaseId = onboardingCase.Id,
                            CaseNumber = onboardingCase.CaseNumber,
                            CaseStatus = onboardingCase.CaseStatus.ToString(),
                            PlannedStartDate = onboardingCase.PlannedStartDate,
                            IsRestricted = false
                        }
                        : new JourneyOnboardingRefDto
                        {
                            OnboardingCaseId = Guid.Empty,
                            CaseNumber = "[TERBATAS]",
                            CaseStatus = RecruitmentJourneyStageStatuses.Restricted,
                            PlannedStartDate = onboardingCase.PlannedStartDate,
                            IsRestricted = true
                        },
                TalentPoolReference = perms.CanReadTalentPool && talentPool != null
                    ? new JourneyTalentPoolRefDto
                    {
                        TalentPoolId = talentPool.Id,
                        Status = talentPool.Status.ToString(),
                        RankingScore = talentPool.RankingScore,
                        RankingPosition = talentPool.RankingPosition,
                        EnrolledDate = talentPool.EnrolledDate,
                        RetentionExpiryDate = talentPool.RetentionExpiryDate,
                        PromotedToOfferId = talentPool.PromotedToOfferId
                    }
                    : null,
                DataCompleteness = new JourneyDataCompletenessDto
                {
                    CompletenessStatus = completenessStatus,
                    MissingOrRestrictedDomains = missingOrRestricted.Distinct().ToList(),
                    Warnings = warnings.Distinct().ToList(),
                    GeneratedAtUtc = DateTime.UtcNow
                },
                Permissions = perms
            };

            return RecruitmentResult<RecruitmentJourneyResponse>.Ok(response, "Ringkasan Recruitment Journey berhasil dimuat.");
        }

        private async Task<JourneyViewerPermissionsDto> ResolvePermissionsAsync(ClaimsPrincipal user)
        {
            return new JourneyViewerPermissionsDto
            {
                CanUpdateCandidate = await _permissionService.HasAccessAsync(user, "Candidate", "Update"),
                CanReadScreening = await _permissionService.HasAccessAsync(user, "CandidateScreening", "Read"),
                CanEvaluateScreening = await _permissionService.HasAccessAsync(user, "CandidateScreening", "Evaluate"),
                CanReadEvaluation = await _permissionService.HasAccessAsync(user, "CandidateEvaluation", "Read"),
                CanViewAllEvaluations = await _permissionService.HasAccessAsync(user, "CandidateEvaluation", "ViewAll"),
                CanEvaluateHr = await _permissionService.HasAccessAsync(user, "CandidateEvaluation", "Evaluate"),
                CanEvaluateClinical = await _permissionService.HasAccessAsync(user, "CandidateEvaluation", "EvaluateClinical"),
                CanReadOffer = await _permissionService.HasAccessAsync(user, "JobOffer", "Read"),
                CanCreateOffer = await _permissionService.HasAccessAsync(user, "JobOffer", "Create"),
                CanUpdateOffer = await _permissionService.HasAccessAsync(user, "JobOffer", "Update"),
                CanReadOfferApproval = await _permissionService.HasAccessAsync(user, "JobOfferApproval", "Read"),
                CanApproveOffer = await _permissionService.HasAccessAsync(user, "JobOfferApproval", "Approve"),
                CanReadMcu = await _permissionService.HasAccessAsync(user, "OccupationalHealthClearance", "Read"),
                CanCreateMcu = await _permissionService.HasAccessAsync(user, "OccupationalHealthClearance", "Create"),
                CanEvaluateMcu = await _permissionService.HasAccessAsync(user, "OccupationalHealthClearance", "Evaluate"),
                CanReadHandoff = await _permissionService.HasAccessAsync(user, "HiringHandoff", "Read"),
                CanExecuteHandoff = await _permissionService.HasAccessAsync(user, "HiringHandoff", "Execute"),
                CanReadTalentPool = await _permissionService.HasAccessAsync(user, "TalentPool", "Read"),
                CanCreateTalentPool = await _permissionService.HasAccessAsync(user, "TalentPool", "Create"),
                CanPromoteTalentPool = await _permissionService.HasAccessAsync(user, "TalentPool", "Promote"),
                CanReadOnboarding = await _permissionService.HasAccessAsync(user, "WorkforceOnboardingCase", "Read")
            };
        }

        private static string? ResolveUpstreamFailureReason(
            HrdCandidateApplication app,
            HrdCandidate candidate,
            HrdCandidateScreening? screening,
            OfferPrerequisiteEvaluation offerPrereq,
            HrdJobOffer? offer,
            HrdPreEmploymentMedicalCheck? mcu,
            HrdCandidateHiringHandoff? handoff,
            HrdOnboardingCase? onboardingCase)
        {
            if (candidate.Status == CandidateStatus.Blacklisted)
                return "Kandidat masuk daftar hitam (Blacklisted).";
            if (app.CurrentStage == ApplicationStage.Withdrawn)
                return "Lamaran ditarik mundur oleh pelamar (Withdrawn).";
            if (screening != null && screening.Status == ScreeningStatus.Rejected)
                return "Lamaran gugur pada Seleksi Administrasi.";
            if (string.Equals(app.StageStatus, "REJECTED_ADMINISTRATION", StringComparison.OrdinalIgnoreCase))
                return "Lamaran gugur pada Seleksi Administrasi.";
            if (offerPrereq.HasClinicalVeto)
                return "Lamaran dihentikan oleh Hak Veto Klinis Mutlak Asesor.";
            if (string.Equals(app.StageStatus, "ASSESSMENT_FAILED", StringComparison.OrdinalIgnoreCase) || (!offerPrereq.HrPassed && offerPrereq.HrCompleted))
                return "Lamaran gugur pada tahap Tes & Wawancara.";
            if (mcu != null && mcu.ClearanceResult == McuClearanceResult.Unfit)
                return "Hasil MCU Pra-Kerja Tidak Laik Kerja (UNFIT); penawaran dibatalkan otomatis.";
            if (offer != null && (offer.Status == JobOfferStatus.ConditionalRejected || offer.Status == JobOfferStatus.FinalRejected || offer.Status == JobOfferStatus.Revoked || offer.Status == JobOfferStatus.Expired))
                return $"Proses berhenti karena penawaran berstatus {offer.Status}.";
            if ((handoff != null && handoff.Status == HandoffStatus.Cancelled) || (onboardingCase != null && onboardingCase.CaseStatus == OnboardingCaseStatus.Cancelled))
                return "Proses bergabung/onboarding dibatalkan (Cancelled).";
            if (app.CurrentStage == ApplicationStage.Rejected)
                return "Lamaran berada pada status Ditolak (Rejected).";
            return null;
        }

        private static JourneyStageDto BuildRegistrationStage(
            HrdCandidateApplication app,
            HrdCandidate candidate,
            HrdJobVacancy? vacancy,
            List<HrdCandidateDocument> documents,
            bool isClinical,
            bool isFastTrack,
            JourneyViewerPermissionsDto perms,
            List<JourneyBlockerDto> blockers,
            List<JourneyActionDto> allowedActions,
            List<string> warnings)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var strDocs = documents.Where(d => d.DocumentType == DocumentType.StrCertificate).ToList();
            var hasStr = strDocs.Count > 0;
            var hasExpiredStr = strDocs.Any(d => d.ValidUntil.HasValue && d.ValidUntil.Value < today);
            var verifiedCount = documents.Count(d => d.IsVerified);

            var stageBlockers = new List<JourneyBlockerDto>();
            var stageActions = new List<JourneyActionDto>();

            if (isClinical && !hasStr && !isFastTrack)
            {
                warnings.Add("Lowongan klinis memerlukan unggahan dokumen STR yang valid sebelum lulus seleksi administrasi.");
            }

            if (perms.CanUpdateCandidate)
            {
                var editAction = new JourneyActionDto
                {
                    ActionCode = "UPLOAD_CANDIDATE_DOCUMENT",
                    Label = "Kelola Dokumen Pelamar",
                    StageKey = RecruitmentJourneyStageKeys.Registration,
                    ActionType = "NAVIGATE",
                    RoutePath = $"/hr/recruitment/candidates/{candidate.Id}?tab=documents&applicationId={app.Id}",
                    RequiredPermission = "Candidate.Update",
                    Description = "Lihat atau unggah berkas dokumen pendukung pelamar pada tab Dokumen."
                };
                stageActions.Add(editAction);
            }

            var summary = isFastTrack
                ? $"Lamaran {app.ApplicationNumber} dibentuk otomatis via Fast-Track Talent Pool untuk lowongan {(vacancy?.Title ?? "-")}."
                : $"Lamaran {app.ApplicationNumber} terdaftar pada {(vacancy?.Title ?? "-")} dengan {documents.Count} berkas dokumen ({verifiedCount} terverifikasi).";

            return new JourneyStageDto
            {
                StageKey = RecruitmentJourneyStageKeys.Registration,
                StageOrder = 1,
                StageTitle = "Pendaftaran",
                Status = RecruitmentJourneyStageStatuses.Completed,
                StatusLabel = "Selesai",
                ResponsibleRole = "Recruiter / Admin SDM",
                Summary = summary,
                StartedAt = app.ApplicationDate,
                CompletedAt = app.ApplicationDate,
                ReferenceId = app.Id,
                ReferenceNumber = app.ApplicationNumber,
                Blockers = stageBlockers,
                AllowedActions = stageActions,
                RegistrationDetail = new JourneyRegistrationStageDetailDto
                {
                    ApplicationNumber = app.ApplicationNumber,
                    ApplicationDate = app.ApplicationDate,
                    TotalDocuments = documents.Count,
                    VerifiedDocuments = verifiedCount,
                    HasStrDocument = hasStr,
                    HasExpiredStrDocument = hasExpiredStr,
                    IsFastTrackFromTalentPool = isFastTrack,
                    Notes = app.Notes
                }
            };
        }

        private static JourneyStageDto BuildScreeningStage(
            HrdCandidateApplication app,
            HrdCandidateScreening? screening,
            bool isClinical,
            bool isFastTrack,
            bool isTerminalOrFailed,
            string? upstreamBlockedReason,
            JourneyViewerPermissionsDto perms,
            List<string> missingOrRestricted,
            List<JourneyBlockerDto> blockers,
            List<JourneyActionDto> allowedActions)
        {
            if (!perms.CanReadScreening)
            {
                missingOrRestricted.Add("CandidateScreening");
                return BuildRestrictedStage(2, RecruitmentJourneyStageKeys.AdministrationScreening, "Seleksi Administrasi", "Tim Seleksi Administrasi HR",
                    "Izin CandidateScreening:Read diperlukan untuk melihat rincian seleksi administrasi.");
            }

            if (isFastTrack && screening == null)
            {
                return new JourneyStageDto
                {
                    StageKey = RecruitmentJourneyStageKeys.AdministrationScreening,
                    StageOrder = 2,
                    StageTitle = "Seleksi Administrasi",
                    Status = RecruitmentJourneyStageStatuses.NotApplicable,
                    StatusLabel = "Dilewati (Fast-Track)",
                    ResponsibleRole = "Tim Seleksi Administrasi HR",
                    Summary = "Tahap seleksi administrasi dilewati karena kandidat dipromosikan langsung dari Talent Pool."
                };
            }

            var stageBlockers = new List<JourneyBlockerDto>();
            var stageActions = new List<JourneyActionDto>();

            if (screening == null)
            {
                var status = isTerminalOrFailed
                    ? RecruitmentJourneyStageStatuses.Blocked
                    : RecruitmentJourneyStageStatuses.Waiting;

                if (isTerminalOrFailed && upstreamBlockedReason != null)
                {
                    stageBlockers.Add(new JourneyBlockerDto
                    {
                        BlockerCode = "UPSTREAM_TERMINAL",
                        StageKey = RecruitmentJourneyStageKeys.AdministrationScreening,
                        Title = "Tahap tidak dilanjutkan",
                        Message = upstreamBlockedReason
                    });
                }
                else if (perms.CanEvaluateScreening)
                {
                    var act = new JourneyActionDto
                    {
                        ActionCode = "INITIATE_SCREENING",
                        Label = "Inisiasi & Evaluasi Screening",
                        StageKey = RecruitmentJourneyStageKeys.AdministrationScreening,
                        ActionType = "POST_ACTION",
                        Method = "POST",
                        Endpoint = "/api/v1/corporate/human-resource/recruitment/screenings",
                        RoutePath = $"/hr/recruitment/screenings?applicationId={app.Id}",
                        RequiredPermission = "CandidateScreening.Evaluate",
                        Description = isClinical
                            ? "Mulai penapisan berkas administrasi dan validasi kelengkapan STR klinis."
                            : "Mulai penapisan berkas administrasi pelamar."
                    };
                    stageActions.Add(act);
                    allowedActions.Add(act);
                }

                return new JourneyStageDto
                {
                    StageKey = RecruitmentJourneyStageKeys.AdministrationScreening,
                    StageOrder = 2,
                    StageTitle = "Seleksi Administrasi",
                    Status = status,
                    StatusLabel = ToIndonesianStageStatusLabel(status),
                    ResponsibleRole = "Tim Seleksi Administrasi HR",
                    Summary = isTerminalOrFailed
                        ? "Seleksi administrasi belum dilakukan saat lamaran dihentikan."
                        : "Menunggu inisiasi seleksi administrasi" + (isClinical ? " dan verifikasi STR klinis." : "."),
                    Blockers = stageBlockers,
                    AllowedActions = stageActions
                };
            }

            string stageStatus;
            string summary;
            if (screening.Status == ScreeningStatus.Passed)
            {
                stageStatus = RecruitmentJourneyStageStatuses.Completed;
                summary = isClinical
                    ? $"Lulus seleksi administrasi & verifikasi STR ({RecruitmentJourneyPolicy.ResolveClinicalScreeningResultLabel(screening.Status, screening.IsStrRequired, screening.ClinicalScreeningResult)})."
                    : "Lulus seleksi administrasi non-klinis.";
            }
            else if (screening.Status == ScreeningStatus.Rejected)
            {
                stageStatus = RecruitmentJourneyStageStatuses.Rejected;
                summary = screening.Notes ?? $"Gugur seleksi administrasi ({RecruitmentJourneyPolicy.ResolveClinicalScreeningResultLabel(screening.Status, screening.IsStrRequired, screening.ClinicalScreeningResult)}).";
                var blk = new JourneyBlockerDto
                {
                    BlockerCode = screening.ClinicalScreeningResult == ClinicalScreeningResult.StrExpiredAutoReject
                        ? "EXPIRED_STR_AUTO_REJECT"
                        : "SCREENING_REJECTED",
                    StageKey = RecruitmentJourneyStageKeys.AdministrationScreening,
                    Title = "Gugur Seleksi Administrasi",
                    Message = summary,
                    ResolutionHint = "Lamaran ini dinyatakan gugur administrasi. Pelamar hanya dapat melamar ulang jika memenuhi persyaratan dokumen terbaru."
                };
                stageBlockers.Add(blk);
                blockers.Add(blk);
            }
            else
            {
                stageStatus = RecruitmentJourneyStageStatuses.InProgress;
                summary = isClinical
                    ? "Penapisan berkas dalam antrean; menunggu pemeriksaan berkas & keabsahan STR."
                    : "Penapisan berkas dalam antrean; menunggu pemeriksaan berkas administrasi HR.";

                if (perms.CanEvaluateScreening && !isTerminalOrFailed)
                {
                    var act = new JourneyActionDto
                    {
                        ActionCode = "EVALUATE_SCREENING",
                        Label = "Periksa Seleksi Administrasi",
                        StageKey = RecruitmentJourneyStageKeys.AdministrationScreening,
                        ActionType = "NAVIGATE",
                        Method = "POST",
                        Endpoint = "/api/v1/corporate/human-resource/recruitment/screenings/evaluate",
                        RoutePath = $"/hr/recruitment/screenings/{screening.Id}?applicationId={app.Id}",
                        RequiredPermission = "CandidateScreening.Evaluate",
                        Description = "Buka halaman pemeriksaan berkas untuk memverifikasi dokumen wajib" + (isClinical ? " dan STR klinis." : ".")
                    };
                    stageActions.Add(act);
                    allowedActions.Add(act);
                }
            }

            return new JourneyStageDto
            {
                StageKey = RecruitmentJourneyStageKeys.AdministrationScreening,
                StageOrder = 2,
                StageTitle = "Seleksi Administrasi",
                Status = stageStatus,
                StatusLabel = ToIndonesianStageStatusLabel(stageStatus),
                ResponsibleRole = "Tim Seleksi Administrasi HR",
                Summary = summary,
                StartedAt = screening.ScreeningDate,
                CompletedAt = screening.Status == ScreeningStatus.Pending ? null : screening.UpdateDateTime ?? screening.ScreeningDate,
                ReferenceId = screening.Id,
                ReferenceNumber = app.ApplicationNumber,
                Blockers = stageBlockers,
                AllowedActions = stageActions,
                ScreeningDetail = new JourneyScreeningStageDetailDto
                {
                    ScreeningId = screening.Id,
                    Status = screening.Status.ToString(),
                    IsStrRequired = screening.IsStrRequired,
                    ClinicalScreeningResult = screening.ClinicalScreeningResult.ToString(),
                    StrNumber = screening.StrNumber,
                    StrExpiryDate = screening.StrExpiryDate,
                    ScreeningDate = screening.ScreeningDate,
                    Notes = screening.Notes
                }
            };
        }

        private static JourneyStageDto BuildEvaluationStage(
            HrdCandidateApplication app,
            HrdCandidateScreening? screening,
            List<HrdCandidateEvaluation> evaluations,
            OfferPrerequisiteEvaluation offerPrereq,
            bool isClinical,
            bool isFastTrack,
            bool isTerminalOrFailed,
            string? upstreamBlockedReason,
            Guid actorUserId,
            JourneyViewerPermissionsDto perms,
            List<string> missingOrRestricted,
            List<JourneyBlockerDto> blockers,
            List<JourneyActionDto> allowedActions,
            List<string> warnings)
        {
            if (!perms.CanReadEvaluation)
            {
                missingOrRestricted.Add("CandidateEvaluation");
                return BuildRestrictedStage(3, RecruitmentJourneyStageKeys.Evaluation, "Tes & Wawancara",
                    isClinical ? "Pewawancara HR & Asesor Klinis" : "Pewawancara HR",
                    "Izin CandidateEvaluation:Read diperlukan untuk melihat detail hasil evaluasi.");
            }

            if (isFastTrack && evaluations.Count == 0)
            {
                return new JourneyStageDto
                {
                    StageKey = RecruitmentJourneyStageKeys.Evaluation,
                    StageOrder = 3,
                    StageTitle = "Tes & Wawancara",
                    Status = RecruitmentJourneyStageStatuses.NotApplicable,
                    StatusLabel = "Dilewati (Fast-Track)",
                    ResponsibleRole = "Pewawancara HR",
                    Summary = "Tahap tes & wawancara dilewati melalui promosi cepat Talent Pool."
                };
            }

            var stageBlockers = new List<JourneyBlockerDto>();
            var stageActions = new List<JourneyActionDto>();

            // Filter sesi sesuai VR-PKB-07 (tanpa ViewAll hanya lihat sesi yang ditugaskan kepada penilai login)
            var visibleEvals = perms.CanViewAllEvaluations
                ? evaluations
                : evaluations.Where(e => e.EvaluatorUserId == actorUserId).ToList();

            var hasHiddenSessions = !perms.CanViewAllEvaluations && evaluations.Count > visibleEvals.Count;
            if (hasHiddenSessions)
            {
                warnings.Add("Sebagian sesi evaluasi kandidat ditugaskan kepada penilai lain dan disembunyikan sesuai kebijakan ViewAll.");
            }

            string stageStatus;
            string summary;
            if (offerPrereq.HasClinicalVeto)
            {
                stageStatus = RecruitmentJourneyStageStatuses.Rejected;
                summary = "Ditolak permanen melalui Hak Veto Klinis Mutlak Asesor Medis.";
                var blk = new JourneyBlockerDto
                {
                    BlockerCode = "CLINICAL_VETO_APPLIED",
                    StageKey = RecruitmentJourneyStageKeys.Evaluation,
                    Title = "Hak Veto Klinis Mutlak",
                    Message = offerPrereq.BlockingMessage ?? summary,
                    ResolutionHint = "Kandidat tidak dapat diproses ke surat penawaran pada lamaran ini."
                };
                stageBlockers.Add(blk);
                blockers.Add(blk);
            }
            else if ((offerPrereq.HrCompleted && !offerPrereq.HrPassed)
                     || string.Equals(app.StageStatus, "ASSESSMENT_FAILED", StringComparison.OrdinalIgnoreCase))
            {
                stageStatus = RecruitmentJourneyStageStatuses.Rejected;
                summary = offerPrereq.BlockingMessage ?? "Kandidat dinyatakan tidak direkomendasikan pada tahap evaluasi.";
                var blk = new JourneyBlockerDto
                {
                    BlockerCode = "EVALUATION_NOT_RECOMMENDED",
                    StageKey = RecruitmentJourneyStageKeys.Evaluation,
                    Title = "Tidak Lulus Tes & Wawancara",
                    Message = summary
                };
                stageBlockers.Add(blk);
                blockers.Add(blk);
            }
            else if (screening == null || screening.Status != ScreeningStatus.Passed)
            {
                stageStatus = RecruitmentJourneyStageStatuses.Blocked;
                summary = upstreamBlockedReason
                    ?? "Tahap Tes & Wawancara belum dapat dijadwalkan sebelum Seleksi Administrasi dinyatakan Lulus.";
                stageBlockers.Add(new JourneyBlockerDto
                {
                    BlockerCode = "SCREENING_PREREQUISITE",
                    StageKey = RecruitmentJourneyStageKeys.Evaluation,
                    Title = "Menunggu Kelulusan Seleksi Administrasi",
                    Message = summary
                });
            }
            else if (offerPrereq.CanCreateOffer)
            {
                stageStatus = RecruitmentJourneyStageStatuses.Completed;
                summary = isClinical
                    ? "Wawancara HR dan Asesmen Klinis telah selesai dikirim serta memenuhi syarat lanjut ke Surat Penawaran."
                    : "Wawancara HR telah selesai dikirim dan lulus untuk lanjut ke Surat Penawaran.";
            }
            else if (evaluations.Count > 0)
            {
                stageStatus = RecruitmentJourneyStageStatuses.InProgress;
                var pendingHr = !offerPrereq.HrCompleted;
                var pendingClinical = isClinical && !offerPrereq.ClinicalCompleted;
                summary = pendingHr && pendingClinical
                    ? "Sesi evaluasi terjadwal; menunggu pengiriman nilai Wawancara HR dan Asesmen Klinis."
                    : pendingHr
                        ? "Menunggu pengiriman nilai Wawancara HR."
                        : "Wawancara HR selesai; menunggu pengiriman nilai Asesmen Klinis.";
            }
            else
            {
                stageStatus = isTerminalOrFailed
                    ? RecruitmentJourneyStageStatuses.Blocked
                    : RecruitmentJourneyStageStatuses.Waiting;
                summary = isTerminalOrFailed
                    ? (upstreamBlockedReason ?? "Evaluasi diblokir karena status lamaran terminal.")
                    : isClinical
                        ? "Seleksi administrasi lulus. Silakan jadwalkan Wawancara HR dan Asesmen Klinis."
                        : "Seleksi administrasi lulus. Silakan jadwalkan Wawancara HR.";
            }

            if (!isTerminalOrFailed && screening != null && screening.Status == ScreeningStatus.Passed && !offerPrereq.CanCreateOffer)
            {
                if (perms.CanEvaluateHr && !offerPrereq.HrCompleted)
                {
                    var scheduleAct = new JourneyActionDto
                    {
                        ActionCode = "SCHEDULE_OR_SUBMIT_HR_EVALUATION",
                        Label = "Jadwalkan / Nilai Wawancara HR",
                        StageKey = RecruitmentJourneyStageKeys.Evaluation,
                        ActionType = "NAVIGATE",
                        RoutePath = $"/hr/recruitment/evaluations/hr/{app.CandidateId}?applicationId={app.Id}",
                        RequiredPermission = "CandidateEvaluation.Evaluate",
                        Description = "Jadwalkan sesi atau kirim hasil Wawancara HR untuk lamaran ini."
                    };
                    stageActions.Add(scheduleAct);
                    allowedActions.Add(scheduleAct);
                }

                if (isClinical && perms.CanEvaluateClinical && !offerPrereq.ClinicalCompleted)
                {
                    var clinAct = new JourneyActionDto
                    {
                        ActionCode = "SUBMIT_CLINICAL_EVALUATION",
                        Label = "Nilai Asesmen Klinis",
                        StageKey = RecruitmentJourneyStageKeys.Evaluation,
                        ActionType = "NAVIGATE",
                        RoutePath = $"/hr/recruitment/evaluations/clinical/{app.CandidateId}?applicationId={app.Id}",
                        RequiredPermission = "CandidateEvaluation.EvaluateClinical",
                        Description = "Kirim penilaian kompetensi klinis oleh asesor medis yang ditugaskan."
                    };
                    stageActions.Add(clinAct);
                    allowedActions.Add(clinAct);
                }

                if (offerPrereq.HrCompleted && isClinical && !offerPrereq.ClinicalCompleted && perms.CanReadEvaluation)
                {
                    stageActions.Add(new JourneyActionDto
                    {
                        ActionCode = "VIEW_EVALUATION_SESSIONS",
                        Label = "Lihat Sesi Evaluasi",
                        StageKey = RecruitmentJourneyStageKeys.Evaluation,
                        ActionType = "NAVIGATE",
                        RoutePath = $"/hr/recruitment/evaluations/hr/{app.CandidateId}?applicationId={app.Id}",
                        RequiredPermission = "CandidateEvaluation.Read",
                        Description = "Lihat ringkasan sesi Wawancara HR yang sudah dikirim sambil menunggu Asesmen Klinis."
                    });
                }
            }

            var submittedDates = visibleEvals.Where(e => e.UpdateDateTime.HasValue).Select(e => e.UpdateDateTime!.Value).ToList();

            return new JourneyStageDto
            {
                StageKey = RecruitmentJourneyStageKeys.Evaluation,
                StageOrder = 3,
                StageTitle = "Tes & Wawancara",
                Status = stageStatus,
                StatusLabel = ToIndonesianStageStatusLabel(stageStatus),
                ResponsibleRole = isClinical ? "Pewawancara HR & Asesor Klinis" : "Pewawancara HR",
                Summary = summary,
                StartedAt = evaluations.FirstOrDefault()?.EvaluationDate ?? evaluations.FirstOrDefault()?.CreateDateTime,
                CompletedAt = stageStatus == RecruitmentJourneyStageStatuses.Completed && submittedDates.Count > 0 ? submittedDates.Max() : null,
                Blockers = stageBlockers,
                AllowedActions = stageActions,
                EvaluationDetail = new JourneyEvaluationStageDetailDto
                {
                    IsClinicalVacancy = isClinical,
                    HrCompleted = offerPrereq.HrCompleted,
                    HrPassed = offerPrereq.HrPassed,
                    ClinicalRequired = offerPrereq.ClinicalRequired,
                    ClinicalCompleted = offerPrereq.ClinicalCompleted,
                    ClinicalPassed = offerPrereq.ClinicalPassed,
                    HasClinicalVeto = offerPrereq.HasClinicalVeto,
                    HasHiddenOtherEvaluatorSessions = hasHiddenSessions,
                    Sessions = visibleEvals.Select(e => new JourneyEvaluationSessionDto
                    {
                        Id = e.Id,
                        EvaluationType = e.EvaluationType.ToString(),
                        EvaluatorUserId = e.EvaluatorUserId,
                        EvaluatorName = e.EvaluatorName,
                        EvaluatorRole = e.EvaluatorRole,
                        EvaluationDate = e.EvaluationDate,
                        IsSubmitted = RecruitmentJourneyPolicy.IsEvaluationSubmitted(e),
                        Score = RecruitmentJourneyPolicy.IsEvaluationSubmitted(e) ? e.Score : null,
                        Recommendation = RecruitmentJourneyPolicy.IsEvaluationSubmitted(e) ? e.Recommendation.ToString() : "Pending",
                        IsClinicalVetoApplied = e.IsClinicalVetoApplied,
                        VetoReason = e.VetoReason,
                        Notes = e.Notes
                    }).ToList()
                }
            };
        }

        private static JourneyStageDto BuildOfferStage(
            HrdCandidateApplication app,
            HrdJobVacancy? vacancy,
            HrdJobOffer? offer,
            OfferPrerequisiteEvaluation offerPrereq,
            bool vacancyQuotaFull,
            int occupiedSeatsOnVacancy,
            bool isTerminalOrFailed,
            string? upstreamBlockedReason,
            Guid actorUserId,
            JourneyViewerPermissionsDto perms,
            List<string> missingOrRestricted,
            List<JourneyBlockerDto> blockers,
            List<JourneyActionDto> allowedActions)
        {
            if (!perms.CanReadOffer && !perms.CanReadOfferApproval)
            {
                missingOrRestricted.Add("JobOffer");
                return BuildRestrictedStage(4, RecruitmentJourneyStageKeys.JobOffer, "Surat Penawaran", "HR & Pejabat Penyetuju",
                    "Izin JobOffer:Read atau JobOfferApproval:Read diperlukan untuk melihat detail Surat Penawaran.");
            }

            var stageBlockers = new List<JourneyBlockerDto>();
            var stageActions = new List<JourneyActionDto>();
            var maskSalary = !(perms.CanReadOffer || perms.CanApproveOffer);

            if (offer == null)
            {
                var status = (isTerminalOrFailed || !offerPrereq.CanCreateOffer || vacancyQuotaFull)
                    ? RecruitmentJourneyStageStatuses.Blocked
                    : RecruitmentJourneyStageStatuses.Waiting;

                string summary;
                if (!offerPrereq.CanCreateOffer)
                {
                    summary = offerPrereq.BlockingMessage ?? "Prasyarat evaluasi belum terpenuhi untuk membuat surat penawaran.";
                    stageBlockers.Add(new JourneyBlockerDto
                    {
                        BlockerCode = offerPrereq.BlockingCode ?? "OFFER_PREREQUISITE",
                        StageKey = RecruitmentJourneyStageKeys.JobOffer,
                        Title = "Surat Penawaran Terkunci",
                        Message = summary
                    });
                }
                else if (isTerminalOrFailed)
                {
                    summary = upstreamBlockedReason ?? "Lamaran dalam status terminal sehingga penawaran tidak dapat dibuat.";
                    stageBlockers.Add(new JourneyBlockerDto
                    {
                        BlockerCode = "UPSTREAM_TERMINAL",
                        StageKey = RecruitmentJourneyStageKeys.JobOffer,
                        Title = "Surat Penawaran Diblokir",
                        Message = summary
                    });
                }
                else if (vacancyQuotaFull)
                {
                    summary = $"Kuota kursi lowongan '{vacancy?.Title ?? "-"}' telah teralokasi penuh ({occupiedSeatsOnVacancy}/{vacancy?.QuotaSeats ?? 0}); kandidat cadangan dapat didaftarkan ke Talent Pool.";
                    stageBlockers.Add(new JourneyBlockerDto
                    {
                        BlockerCode = "VACANCY_QUOTA_FULL",
                        StageKey = RecruitmentJourneyStageKeys.JobOffer,
                        Title = "Kuota Lowongan Terisi Penuh",
                        Message = summary,
                        ResolutionHint = "Daftarkan kandidat ke Talent Pool bila tidak ada penawaran aktif."
                    });
                }
                else
                {
                    summary = "Kandidat memenuhi syarat evaluasi dan siap dibuatkan draf Surat Penawaran Kerja.";
                    if (perms.CanCreateOffer)
                    {
                        var act = new JourneyActionDto
                        {
                            ActionCode = "CREATE_JOB_OFFER",
                            Label = "Buat Surat Penawaran",
                            StageKey = RecruitmentJourneyStageKeys.JobOffer,
                            ActionType = "NAVIGATE",
                            Method = "POST",
                            Endpoint = "/api/v1/corporate/human-resource/recruitment/offers",
                            RoutePath = $"/hr/recruitment/offers/create?applicationId={app.Id}",
                            RequiredPermission = "JobOffer.Create",
                            Description = "Susun surat penawaran kerja dengan skala upah dan jenis kontrak kerja."
                        };
                        stageActions.Add(act);
                        allowedActions.Add(act);
                    }
                }

                return new JourneyStageDto
                {
                    StageKey = RecruitmentJourneyStageKeys.JobOffer,
                    StageOrder = 4,
                    StageTitle = "Surat Penawaran",
                    Status = status,
                    StatusLabel = ToIndonesianStageStatusLabel(status),
                    ResponsibleRole = "HR & Pejabat Penyetuju",
                    Summary = summary,
                    Blockers = stageBlockers,
                    AllowedActions = stageActions
                };
            }

            var isOfferExpired = DateTime.UtcNow > offer.ExpiryDate
                && (offer.Status == JobOfferStatus.SentConditional || offer.Status == JobOfferStatus.FinalOfferSent);

            string stageStatus;
            string offerSummary;

            if (offer.Status == JobOfferStatus.FinalAccepted)
            {
                stageStatus = RecruitmentJourneyStageStatuses.Completed;
                offerSummary = $"Surat penawaran {offer.OfferNumber} telah diterima final oleh kandidat.";
            }
            else if (offer.Status == JobOfferStatus.ConditionalAccepted)
            {
                stageStatus = RecruitmentJourneyStageStatuses.Completed;
                offerSummary = $"Penawaran bersyarat {offer.OfferNumber} telah disetujui kandidat; menunggu kelaikan MCU Pra-Kerja.";
            }
            else if (offer.Status == JobOfferStatus.ConditionalRejected || offer.Status == JobOfferStatus.FinalRejected
                     || offer.Status == JobOfferStatus.Revoked || offer.Status == JobOfferStatus.Expired || isOfferExpired)
            {
                stageStatus = RecruitmentJourneyStageStatuses.Rejected;
                offerSummary = isOfferExpired
                    ? $"Surat penawaran {offer.OfferNumber} telah melampaui batas kedaluwarsa ({offer.ExpiryDate:yyyy-MM-dd})."
                    : $"Surat penawaran {offer.OfferNumber} berstatus {offer.Status}.";
                var blk = new JourneyBlockerDto
                {
                    BlockerCode = isOfferExpired || offer.Status == JobOfferStatus.Expired ? "OFFER_EXPIRED" : "OFFER_NOT_ACCEPTED",
                    StageKey = RecruitmentJourneyStageKeys.JobOffer,
                    Title = "Surat Penawaran Dihentikan",
                    Message = offerSummary
                };
                stageBlockers.Add(blk);
                blockers.Add(blk);
            }
            else if (offer.Status == JobOfferStatus.RejectedByApprover)
            {
                stageStatus = RecruitmentJourneyStageStatuses.Blocked;
                offerSummary = $"Surat penawaran {offer.OfferNumber} ditolak pada jalur persetujuan ({offer.ApprovalTier}).";
                stageBlockers.Add(new JourneyBlockerDto
                {
                    BlockerCode = "OFFER_APPROVAL_REJECTED",
                    StageKey = RecruitmentJourneyStageKeys.JobOffer,
                    Severity = "WARNING",
                    Title = "Penawaran Ditolak Penyetuju",
                    Message = offerSummary,
                    ResolutionHint = "Tinjau catatan penyetuju atau susun penawaran baru bila masih layak diproses."
                });
            }
            else
            {
                stageStatus = RecruitmentJourneyStageStatuses.InProgress;
                offerSummary = offer.Status switch
                {
                    JobOfferStatus.Draft => $"Draf penawaran {offer.OfferNumber} ({offer.ApprovalTier}) siap diajukan persetujuan.",
                    JobOfferStatus.PendingApproval => $"Penawaran {offer.OfferNumber} sedang menunggu keputusan berjenjang ({offer.ApprovalTier}).",
                    JobOfferStatus.Approved => $"Penawaran {offer.OfferNumber} disetujui penuh; siap dikirimkan ke kandidat.",
                    JobOfferStatus.SentConditional => $"Penawaran bersyarat {offer.OfferNumber} terkirim; menunggu respons kandidat.",
                    JobOfferStatus.FinalOfferSent => $"Penawaran final {offer.OfferNumber} terkirim; menunggu konfirmasi final kandidat.",
                    _ => $"Status penawaran: {offer.Status}."
                };
            }

            if (!isTerminalOrFailed && !isOfferExpired)
            {
                if (perms.CanApproveOffer
                    && (offer.Status == JobOfferStatus.Draft || offer.Status == JobOfferStatus.PendingApproval)
                    && (offer.CreateBy != actorUserId || actorUserId == Guid.Empty))
                {
                    var approveAct = new JourneyActionDto
                    {
                        ActionCode = "DECIDE_OFFER_APPROVAL",
                        Label = "Putuskan Persetujuan Penawaran",
                        StageKey = RecruitmentJourneyStageKeys.JobOffer,
                        ActionType = "NAVIGATE",
                        RoutePath = $"/hr/recruitment/offers/{offer.Id}?applicationId={app.Id}",
                        RequiredPermission = "JobOfferApproval.Approve",
                        Description = "Setujui atau tolak langkah persetujuan surat penawaran berjenjang (T13/T14)."
                    };
                    stageActions.Add(approveAct);
                    allowedActions.Add(approveAct);
                }

                if (perms.CanUpdateOffer && offer.Status == JobOfferStatus.Approved)
                {
                    var sendAct = new JourneyActionDto
                    {
                        ActionCode = "SEND_CONDITIONAL_OFFER",
                        Label = "Kirim Surat Penawaran",
                        StageKey = RecruitmentJourneyStageKeys.JobOffer,
                        ActionType = "POST_ACTION",
                        Method = "POST",
                        Endpoint = $"/api/v1/corporate/human-resource/recruitment/offers/{offer.Id}/send-conditional",
                        RoutePath = $"/hr/recruitment/offers/{offer.Id}?applicationId={app.Id}",
                        RequiredPermission = "JobOffer.Update",
                        Description = "Kirim penawaran yang sudah disetujui penuh kepada kandidat."
                    };
                    stageActions.Add(sendAct);
                    allowedActions.Add(sendAct);
                }

                if (perms.CanUpdateOffer && (offer.Status == JobOfferStatus.SentConditional || offer.Status == JobOfferStatus.FinalOfferSent))
                {
                    var respondAct = new JourneyActionDto
                    {
                        ActionCode = "RECORD_CANDIDATE_OFFER_RESPONSE",
                        Label = "Catat Respons Kandidat",
                        StageKey = RecruitmentJourneyStageKeys.JobOffer,
                        ActionType = "POST_ACTION",
                        Method = "POST",
                        Endpoint = $"/api/v1/corporate/human-resource/recruitment/offers/{offer.Id}/candidate-respond",
                        RoutePath = $"/hr/recruitment/offers/{offer.Id}?applicationId={app.Id}",
                        RequiredPermission = "JobOffer.Update",
                        Description = "Catat persetujuan atau penolakan kandidat terhadap surat penawaran yang sudah dikirim."
                    };
                    stageActions.Add(respondAct);
                    allowedActions.Add(respondAct);
                }
            }

            if (perms.CanReadOffer || perms.CanReadOfferApproval)
            {
                stageActions.Add(new JourneyActionDto
                {
                    ActionCode = "VIEW_JOB_OFFER_DETAIL",
                    Label = "Buka Detail Penawaran",
                    StageKey = RecruitmentJourneyStageKeys.JobOffer,
                    ActionType = "NAVIGATE",
                    RoutePath = $"/hr/recruitment/offers/{offer.Id}?applicationId={app.Id}",
                    RequiredPermission = "JobOffer.Read",
                    Description = "Buka rincian surat penawaran dan riwayat persetujuan."
                });
            }

            return new JourneyStageDto
            {
                StageKey = RecruitmentJourneyStageKeys.JobOffer,
                StageOrder = 4,
                StageTitle = "Surat Penawaran",
                Status = stageStatus,
                StatusLabel = ToIndonesianStageStatusLabel(stageStatus),
                ResponsibleRole = "HR & Pejabat Penyetuju",
                Summary = offerSummary,
                StartedAt = offer.OfferDate,
                CompletedAt = stageStatus == RecruitmentJourneyStageStatuses.Completed ? offer.CandidateResponseDate ?? offer.UpdateDateTime : null,
                ReferenceId = offer.Id,
                ReferenceNumber = offer.OfferNumber,
                Blockers = stageBlockers,
                AllowedActions = stageActions,
                OfferDetail = new JourneyOfferStageDetailDto
                {
                    OfferId = offer.Id,
                    OfferNumber = offer.OfferNumber,
                    OfferedPositionTitle = offer.OfferedPositionTitle,
                    Status = offer.Status.ToString(),
                    ApprovalTier = offer.ApprovalTier.ToString(),
                    ContractTypeName = offer.ContractType?.ContractTypeName,
                    ContractDurationMonths = offer.ContractDurationMonths,
                    ProposedJoinDate = offer.ProposedJoinDate,
                    OfferDate = offer.OfferDate,
                    ExpiryDate = offer.ExpiryDate,
                    IsExpired = isOfferExpired || offer.Status == JobOfferStatus.Expired,
                    CandidateResponseDate = offer.CandidateResponseDate,
                    CandidateResponseNotes = offer.CandidateResponseNotes,
                    IsSalaryMasked = maskSalary,
                    OfferedBasicSalary = maskSalary ? null : offer.OfferedBasicSalary,
                    MaximumGradeSalary = maskSalary ? null : offer.MaximumGradeSalary,
                    SalaryGradeName = maskSalary ? null : offer.SalaryGrade?.SalaryGradeName,
                    Approvals = offer.Approvals
                        .Where(a => !a.IsDelete)
                        .OrderBy(a => a.StepNumber)
                        .Select(a => new JourneyOfferApprovalStepDto
                        {
                            Id = a.Id,
                            StepNumber = a.StepNumber,
                            ApproverRole = a.ApproverRole,
                            ApproverName = a.ApproverName,
                            Status = a.Status.ToString(),
                            DecisionDate = a.DecisionDate,
                            Notes = a.Notes
                        }).ToList()
                }
            };
        }

        private static JourneyStageDto BuildMcuStage(
            HrdCandidateApplication app,
            HrdJobOffer? offer,
            HrdPreEmploymentMedicalCheck? mcu,
            bool isTerminalOrFailed,
            string? upstreamBlockedReason,
            JourneyViewerPermissionsDto perms,
            List<string> missingOrRestricted,
            List<JourneyBlockerDto> blockers,
            List<JourneyActionDto> allowedActions,
            List<string> warnings)
        {
            if (!perms.CanReadMcu)
            {
                missingOrRestricted.Add("OccupationalHealthClearance");
                return BuildRestrictedStage(5, RecruitmentJourneyStageKeys.PreEmploymentMcu, "MCU Pra-Kerja", "Tim K3RS / Dokter Pemeriksa",
                    "Izin OccupationalHealthClearance:Read diperlukan untuk melihat status MCU Pra-Kerja.");
            }

            var stageBlockers = new List<JourneyBlockerDto>();
            var stageActions = new List<JourneyActionDto>();
            var maskMedicalDetails = !perms.CanEvaluateMcu;

            if (mcu == null)
            {
                var readyForReferral = !isTerminalOrFailed && offer != null && offer.Status == JobOfferStatus.ConditionalAccepted;
                var status = readyForReferral
                    ? RecruitmentJourneyStageStatuses.Waiting
                    : RecruitmentJourneyStageStatuses.Blocked;

                string summary;
                if (!readyForReferral)
                {
                    summary = upstreamBlockedReason ?? "Rujukan MCU Pra-Kerja baru dapat dibuat setelah kandidat menyetujui penawaran bersyarat (ConditionalAccepted).";
                    stageBlockers.Add(new JourneyBlockerDto
                    {
                        BlockerCode = "CONDITIONAL_ACCEPTANCE_REQUIRED",
                        StageKey = RecruitmentJourneyStageKeys.PreEmploymentMcu,
                        Title = "Menunggu Persetujuan Penawaran Bersyarat",
                        Message = summary
                    });
                }
                else if (perms.CanCreateMcu)
                {
                    summary = "Kandidat telah menyetujui penawaran bersyarat; siap diterbitkan rujukan MCU Pra-Kerja (HRD-DEC-078).";
                    var act = new JourneyActionDto
                    {
                        ActionCode = "REGISTER_MCU_REFERRAL",
                        Label = "Terbitkan Rujukan MCU",
                        StageKey = RecruitmentJourneyStageKeys.PreEmploymentMcu,
                        ActionType = "NAVIGATE",
                        Method = "POST",
                        Endpoint = "/api/v1/corporate/human-resource/recruitment/mcu-clearance/register",
                        RoutePath = $"/hr/recruitment/mcu-clearance/create?applicationId={app.Id}&offerId={offer!.Id}",
                        RequiredPermission = "OccupationalHealthClearance.Create",
                        Description = "Daftarkan jadwal rujukan pemeriksaan kesehatan pra-kerja untuk kandidat."
                    };
                    stageActions.Add(act);
                    allowedActions.Add(act);
                }
                else
                {
                    summary = "Kandidat telah menyetujui penawaran bersyarat. Menunggu petugas berwenang (Tim K3RS / Dokter Pemeriksa dengan izin OccupationalHealthClearance.Create) menerbitkan rujukan MCU Pra-Kerja.";
                }

                return new JourneyStageDto
                {
                    StageKey = RecruitmentJourneyStageKeys.PreEmploymentMcu,
                    StageOrder = 5,
                    StageTitle = "MCU Pra-Kerja",
                    Status = status,
                    StatusLabel = ToIndonesianStageStatusLabel(status),
                    ResponsibleRole = "Tim K3RS / Dokter Pemeriksa",
                    Summary = summary,
                    Blockers = stageBlockers,
                    AllowedActions = stageActions
                };
            }

            string stageStatus;
            string mcuSummary;
            if (mcu.ClearanceResult == McuClearanceResult.Fit)
            {
                stageStatus = RecruitmentJourneyStageStatuses.Completed;
                mcuSummary = $"Hasil MCU {mcu.ReferralNumber}: Laik Kerja (FIT).";
                stageActions.Add(new JourneyActionDto
                {
                    ActionCode = "VIEW_MCU_DETAIL",
                    Label = "Lihat Detail MCU",
                    StageKey = RecruitmentJourneyStageKeys.PreEmploymentMcu,
                    ActionType = "NAVIGATE",
                    RoutePath = $"/hr/recruitment/mcu-clearance/{mcu.Id}?applicationId={app.Id}",
                    RequiredPermission = "OccupationalHealthClearance.Read",
                    Description = "Buka rincian pemeriksaan kesehatan pra-kerja yang telah selesai."
                });
            }
            else if (mcu.ClearanceResult == McuClearanceResult.FitWithRestrictions)
            {
                stageStatus = RecruitmentJourneyStageStatuses.Blocked;
                mcuSummary = $"Hasil MCU {mcu.ReferralNumber}: Fit With Restrictions — menunggu konfirmasi penyesuaian penugasan/kontrak sebelum serah terima.";
                var blk = new JourneyBlockerDto
                {
                    BlockerCode = "MCU_FIT_WITH_RESTRICTIONS_CONFIRMATION_REQUIRED",
                    StageKey = RecruitmentJourneyStageKeys.PreEmploymentMcu,
                    Title = "Fit With Restrictions Memerlukan Konfirmasi Penyesuaian",
                    Message = mcuSummary,
                    ResolutionHint = "Konfirmasikan penyesuaian kontrak/penugasan kerja bersama Tim K3RS dan unit terkait sebelum serah terima ke onboarding dijalankan."
                };
                stageBlockers.Add(blk);
                blockers.Add(blk);
                warnings.Add("Catatan klinis rinci hanya dibuka untuk petugas medis yang berwenang (OccupationalHealthClearance:Evaluate).");
                stageActions.Add(new JourneyActionDto
                {
                    ActionCode = "VIEW_MCU_DETAIL",
                    Label = "Lihat Detail MCU",
                    StageKey = RecruitmentJourneyStageKeys.PreEmploymentMcu,
                    ActionType = "NAVIGATE",
                    RoutePath = $"/hr/recruitment/mcu-clearance/{mcu.Id}?applicationId={app.Id}",
                    RequiredPermission = "OccupationalHealthClearance.Read",
                    Description = "Buka rincian catatan penyesuaian MCU Pra-Kerja."
                });
            }
            else if (mcu.ClearanceResult == McuClearanceResult.Unfit)
            {
                stageStatus = RecruitmentJourneyStageStatuses.Rejected;
                mcuSummary = $"Hasil MCU {mcu.ReferralNumber}: Tidak Laik Kerja (UNFIT). Penawaran dibatalkan otomatis.";
                var blk = new JourneyBlockerDto
                {
                    BlockerCode = "MCU_UNFIT",
                    StageKey = RecruitmentJourneyStageKeys.PreEmploymentMcu,
                    Title = "Tidak Laik Kerja (UNFIT)",
                    Message = mcuSummary
                };
                stageBlockers.Add(blk);
                blockers.Add(blk);
                stageActions.Add(new JourneyActionDto
                {
                    ActionCode = "VIEW_MCU_DETAIL",
                    Label = "Lihat Detail MCU",
                    StageKey = RecruitmentJourneyStageKeys.PreEmploymentMcu,
                    ActionType = "NAVIGATE",
                    RoutePath = $"/hr/recruitment/mcu-clearance/{mcu.Id}?applicationId={app.Id}",
                    RequiredPermission = "OccupationalHealthClearance.Read",
                    Description = "Buka rincian rujukan MCU."
                });
            }
            else
            {
                stageStatus = RecruitmentJourneyStageStatuses.InProgress;
                if (perms.CanEvaluateMcu && !isTerminalOrFailed)
                {
                    mcuSummary = $"Rujukan MCU {mcu.ReferralNumber} telah diterbitkan ke {mcu.ClinicOrFacility}; silakan catat hasil pemeriksaan kelaikan kerja.";
                    var act = new JourneyActionDto
                    {
                        ActionCode = "RECORD_MCU_RESULT",
                        Label = "Catat Hasil MCU",
                        StageKey = RecruitmentJourneyStageKeys.PreEmploymentMcu,
                        ActionType = "NAVIGATE",
                        Method = "POST",
                        Endpoint = $"/api/v1/corporate/human-resource/recruitment/mcu-clearance/{mcu.Id}/record-result",
                        RoutePath = $"/hr/recruitment/mcu-clearance/{mcu.Id}?applicationId={app.Id}",
                        RequiredPermission = "OccupationalHealthClearance.Evaluate",
                        Description = "Catat keputusan kelaikan kerja (Fit, FitWithRestrictions, atau Unfit) beserta catatan medis terisolasi."
                    };
                    stageActions.Add(act);
                    allowedActions.Add(act);
                }
                else
                {
                    mcuSummary = $"Rujukan MCU {mcu.ReferralNumber} telah diterbitkan ke {mcu.ClinicOrFacility}; menunggu pencatatan hasil oleh Dokter Pemeriksa / Tim K3RS.";
                    stageActions.Add(new JourneyActionDto
                    {
                        ActionCode = "VIEW_MCU_DETAIL",
                        Label = "Lihat Detail Rujukan MCU",
                        StageKey = RecruitmentJourneyStageKeys.PreEmploymentMcu,
                        ActionType = "NAVIGATE",
                        RoutePath = $"/hr/recruitment/mcu-clearance/{mcu.Id}?applicationId={app.Id}",
                        RequiredPermission = "OccupationalHealthClearance.Read",
                        Description = "Pantau rincian jadwal rujukan MCU Pra-Kerja yang telah diterbitkan."
                    });
                }
            }

            if (maskMedicalDetails && (!string.IsNullOrWhiteSpace(mcu.DoctorNotes) || !string.IsNullOrWhiteSpace(mcu.CertificateFilePath)))
            {
                warnings.Add("Diagnosa klinis dan berkas sertifikat MCU disembunyikan sesuai kebijakan privasi rekam medis pra-kerja.");
            }

            return new JourneyStageDto
            {
                StageKey = RecruitmentJourneyStageKeys.PreEmploymentMcu,
                StageOrder = 5,
                StageTitle = "MCU Pra-Kerja",
                Status = stageStatus,
                StatusLabel = ToIndonesianStageStatusLabel(stageStatus),
                ResponsibleRole = "Tim K3RS / Dokter Pemeriksa",
                Summary = mcuSummary,
                StartedAt = mcu.ReferralDate,
                CompletedAt = mcu.ClearanceResult == McuClearanceResult.Fit ? mcu.CheckDate ?? mcu.UpdateDateTime : null,
                ReferenceId = mcu.Id,
                ReferenceNumber = mcu.ReferralNumber,
                Blockers = stageBlockers,
                AllowedActions = stageActions,
                McuDetail = new JourneyMcuStageDetailDto
                {
                    McuClearanceId = mcu.Id,
                    ReferralNumber = mcu.ReferralNumber,
                    ReferralDate = mcu.ReferralDate,
                    CheckDate = mcu.CheckDate,
                    ClinicOrFacility = mcu.ClinicOrFacility,
                    ClearanceResult = mcu.ClearanceResult.ToString(),
                    ExaminingDoctorName = mcu.ExaminingDoctorName,
                    IsMedicalDetailsMasked = maskMedicalDetails,
                    DoctorNotes = maskMedicalDetails ? null : mcu.DoctorNotes,
                    CertificateFilePath = maskMedicalDetails ? null : mcu.CertificateFilePath
                }
            };
        }

        private static JourneyStageDto BuildHandoffStage(
            HrdCandidateApplication app,
            HrdCandidate candidate,
            HrdJobOffer? offer,
            HrdPreEmploymentMedicalCheck? mcu,
            HrdCandidateHiringHandoff? handoff,
            HrdOnboardingCase? onboardingCase,
            HandoffPrerequisiteEvaluation handoffPrereq,
            bool isTerminalOrFailed,
            string? upstreamBlockedReason,
            JourneyViewerPermissionsDto perms,
            List<string> missingOrRestricted,
            List<JourneyBlockerDto> blockers,
            List<JourneyActionDto> allowedActions,
            List<string> warnings)
        {
            if (!perms.CanReadHandoff && !perms.CanReadOnboarding)
            {
                missingOrRestricted.Add("HiringHandoff");
                return BuildRestrictedStage(6, RecruitmentJourneyStageKeys.HandoffOnboarding, "Serah Terima", "HR Recruitment & Onboarding",
                    "Izin HiringHandoff:Read atau WorkforceOnboardingCase:Read diperlukan untuk melihat rincian Serah Terima.");
            }

            var stageBlockers = new List<JourneyBlockerDto>();
            var stageActions = new List<JourneyActionDto>();

            if (handoff == null)
            {
                var status = (!isTerminalOrFailed && handoffPrereq.IsEligible)
                    ? RecruitmentJourneyStageStatuses.Waiting
                    : RecruitmentJourneyStageStatuses.Blocked;

                var summary = (!isTerminalOrFailed && handoffPrereq.IsEligible)
                    ? "Kandidat telah memenuhi persetujuan final dan kelaikan MCU FIT; siap diserahkan ke Onboarding (Sub-modul 02)."
                    : (upstreamBlockedReason ?? handoffPrereq.BlockingMessage ?? "Syarat serah terima belum terpenuhi.");

                if (status == RecruitmentJourneyStageStatuses.Blocked)
                {
                    stageBlockers.Add(new JourneyBlockerDto
                    {
                        BlockerCode = handoffPrereq.BlockingCode ?? "HANDOFF_BLOCKED",
                        StageKey = RecruitmentJourneyStageKeys.HandoffOnboarding,
                        Title = "Serah Terima Menunggu Prasyarat",
                        Message = summary
                    });
                }
                else if (perms.CanExecuteHandoff)
                {
                    var act = new JourneyActionDto
                    {
                        ActionCode = "GENERATE_HIRING_HANDOFF",
                        Label = "Proses Serah Terima ke Onboarding",
                        StageKey = RecruitmentJourneyStageKeys.HandoffOnboarding,
                        ActionType = "NAVIGATE",
                        Method = "POST",
                        Endpoint = "/api/v1/corporate/human-resource/recruitment/handoffs/generate",
                        RoutePath = $"/hr/recruitment/handoffs/create?applicationId={app.Id}&offerId={offer!.Id}",
                        RequiredPermission = "HiringHandoff.Execute",
                        Description = "Bentuk atau tautkan profil tenaga kerja serta inisiasi kasus onboarding secara atomik."
                    };
                    stageActions.Add(act);
                    allowedActions.Add(act);
                }

                return new JourneyStageDto
                {
                    StageKey = RecruitmentJourneyStageKeys.HandoffOnboarding,
                    StageOrder = 6,
                    StageTitle = "Serah Terima",
                    Status = status,
                    StatusLabel = ToIndonesianStageStatusLabel(status),
                    ResponsibleRole = "HR Recruitment & Onboarding",
                    Summary = summary,
                    Blockers = stageBlockers,
                    AllowedActions = stageActions
                };
            }

            var needsRecovery = handoff.Status != HandoffStatus.Cancelled && onboardingCase == null;
            var isRehireReused = candidate.PreviousWorkforceProfileId.HasValue
                && handoff.WorkforceProfileId == candidate.PreviousWorkforceProfileId.Value;

            string stageStatus;
            string handoffSummary;
            if (handoff.Status == HandoffStatus.Cancelled || onboardingCase?.CaseStatus == OnboardingCaseStatus.Cancelled)
            {
                stageStatus = RecruitmentJourneyStageStatuses.Rejected;
                handoffSummary = $"Serah terima {handoff.HandoffNumber} dibatalkan (Cancelled).";
                var blk = new JourneyBlockerDto
                {
                    BlockerCode = "HANDOFF_OR_ONBOARDING_CANCELLED",
                    StageKey = RecruitmentJourneyStageKeys.HandoffOnboarding,
                    Title = "Serah Terima / Onboarding Dibatalkan",
                    Message = handoffSummary
                };
                stageBlockers.Add(blk);
                blockers.Add(blk);
            }
            else if (needsRecovery)
            {
                stageStatus = RecruitmentJourneyStageStatuses.Blocked;
                handoffSummary = $"Dokumen serah terima {handoff.HandoffNumber} tercatat, namun kasus onboarding belum terbentuk.";
                var blk = new JourneyBlockerDto
                {
                    BlockerCode = "ONBOARDING_CASE_MISSING_NEEDS_RECOVERY",
                    StageKey = RecruitmentJourneyStageKeys.HandoffOnboarding,
                    Title = "Kasus Onboarding Belum Terbentuk (Partial State)",
                    Message = handoffSummary,
                    ResolutionHint = "Jalankan pemulihan serah terima tanpa membuat ulang WorkforceProfile duplikat."
                };
                stageBlockers.Add(blk);
                blockers.Add(blk);
                warnings.Add("Kasus onboarding belum tersedia untuk handoff ini; pemulihan aman tersedia.");

                if (perms.CanExecuteHandoff && offer != null)
                {
                    var recoverAct = new JourneyActionDto
                    {
                        ActionCode = "RECOVER_HANDOFF_ONBOARDING",
                        Label = "Pulihkan Kasus Onboarding",
                        StageKey = RecruitmentJourneyStageKeys.HandoffOnboarding,
                        ActionType = "POST_ACTION",
                        Method = "POST",
                        Endpoint = "/api/v1/corporate/human-resource/recruitment/handoffs/generate",
                        RoutePath = $"/hr/recruitment/handoffs/create?applicationId={app.Id}&offerId={offer.Id}",
                        RequiredPermission = "HiringHandoff.Execute",
                        Description = "Lanjutkan pembentukan kasus onboarding secara idempoten tanpa menduplikasi profil atau kursi formasi."
                    };
                    stageActions.Add(recoverAct);
                    allowedActions.Add(recoverAct);
                }
            }
            else if (handoff.Status == HandoffStatus.Completed)
            {
                stageStatus = RecruitmentJourneyStageStatuses.Completed;
                handoffSummary = $"Serah terima {handoff.HandoffNumber} selesai; kasus onboarding {onboardingCase?.CaseNumber ?? "-"} aktif/selesai.";
            }
            else
            {
                stageStatus = RecruitmentJourneyStageStatuses.InProgress;
                handoffSummary = $"Serah terima {handoff.HandoffNumber} telah diteruskan ke kasus onboarding {onboardingCase?.CaseNumber ?? "-"}.";
                if (perms.CanExecuteHandoff)
                {
                    var confirmAct = new JourneyActionDto
                    {
                        ActionCode = "CONFIRM_HIRING_HANDOFF",
                        Label = "Konfirmasi Selesai Serah Terima",
                        StageKey = RecruitmentJourneyStageKeys.HandoffOnboarding,
                        ActionType = "POST_ACTION",
                        Method = "POST",
                        Endpoint = $"/api/v1/corporate/human-resource/recruitment/handoffs/{handoff.Id}/confirm-transfer",
                        RoutePath = $"/hr/recruitment/handoffs?applicationId={app.Id}",
                        RequiredPermission = "HiringHandoff.Execute",
                        Description = "Tandai serah terima berkas dari rekrutmen ke onboarding telah terkonfirmasi."
                    };
                    stageActions.Add(confirmAct);
                    allowedActions.Add(confirmAct);
                }
            }

            if (onboardingCase != null && perms.CanReadOnboarding)
            {
                var openOnbAct = new JourneyActionDto
                {
                    ActionCode = "OPEN_ONBOARDING_CASE",
                    Label = "Buka Kasus Onboarding",
                    StageKey = RecruitmentJourneyStageKeys.HandoffOnboarding,
                    ActionType = "NAVIGATE",
                    RoutePath = $"/hr/lifecycle/onboarding-cases/{onboardingCase.Id}",
                    RequiredPermission = "WorkforceOnboardingCase.Read",
                    Description = "Buka ruang kerja kasus onboarding pada Sub-modul Manajemen Siklus Pegawai."
                };
                stageActions.Add(openOnbAct);
                allowedActions.Add(openOnbAct);
            }

            return new JourneyStageDto
            {
                StageKey = RecruitmentJourneyStageKeys.HandoffOnboarding,
                StageOrder = 6,
                StageTitle = "Serah Terima",
                Status = stageStatus,
                StatusLabel = ToIndonesianStageStatusLabel(stageStatus),
                ResponsibleRole = "HR Recruitment & Onboarding",
                Summary = handoffSummary,
                StartedAt = handoff.HandoffDate,
                CompletedAt = handoff.Status == HandoffStatus.Completed ? handoff.UpdateDateTime ?? handoff.HandoffDate : null,
                ReferenceId = handoff.Id,
                ReferenceNumber = handoff.HandoffNumber,
                Blockers = stageBlockers,
                AllowedActions = stageActions,
                HandoffDetail = new JourneyHandoffStageDetailDto
                {
                    HandoffId = handoff.Id,
                    HandoffNumber = handoff.HandoffNumber,
                    HandoffStatus = handoff.Status.ToString(),
                    HandoffDate = handoff.HandoffDate,
                    WorkforceProfileId = handoff.WorkforceProfileId,
                    WorkforceProfileCode = handoff.WorkforceProfile?.ProfileCode,
                    HeadcountRequestSeatId = handoff.HeadcountRequestSeatId,
                    SeatNumber = handoff.HeadcountRequestSeat?.SeatNumber,
                    IsRehireProfileReused = isRehireReused,
                    Notes = handoff.Notes,
                    HasOnboardingCase = onboardingCase != null,
                    NeedsOnboardingRecovery = needsRecovery
                }
            };
        }

        private static JourneyStageDto BuildRestrictedStage(
            int order,
            string stageKey,
            string title,
            string role,
            string reason)
        {
            return new JourneyStageDto
            {
                StageKey = stageKey,
                StageOrder = order,
                StageTitle = title,
                Status = RecruitmentJourneyStageStatuses.Restricted,
                StatusLabel = "Terbatas",
                ResponsibleRole = role,
                Summary = reason,
                IsRestricted = true,
                RestrictionReason = reason
            };
        }

        private static (string StageKey, string StageLabel) ResolveCurrentVisualStage(
            HrdCandidateApplication app,
            HrdCandidateScreening? screening,
            OfferPrerequisiteEvaluation offerPrereq,
            HrdJobOffer? offer,
            HrdPreEmploymentMedicalCheck? mcu,
            HrdCandidateHiringHandoff? handoff,
            HrdOnboardingCase? onboardingCase,
            HandoffPrerequisiteEvaluation handoffPrereq,
            bool vacancyQuotaFull,
            bool isTerminalOrFailed,
            bool isFastTrack)
        {
            if (handoff != null
                || onboardingCase != null
                || app.CurrentStage == ApplicationStage.Handoff
                || (!isTerminalOrFailed && handoffPrereq.IsEligible))
            {
                return (RecruitmentJourneyStageKeys.HandoffOnboarding, "Serah Terima");
            }

            if (mcu != null
                || (offer != null && offer.Status == JobOfferStatus.ConditionalAccepted)
                || app.CurrentStage == ApplicationStage.MedicalCheckup)
            {
                return (RecruitmentJourneyStageKeys.PreEmploymentMcu, "MCU Pra-Kerja");
            }

            if (offer != null
                || app.CurrentStage == ApplicationStage.Offering
                || isFastTrack
                || (!isTerminalOrFailed && offerPrereq.CanCreateOffer && !vacancyQuotaFull))
            {
                return (RecruitmentJourneyStageKeys.JobOffer, "Surat Penawaran");
            }

            if (app.CurrentStage == ApplicationStage.InterviewHr
                || app.CurrentStage == ApplicationStage.ClinicalAssessment
                || (screening != null && screening.Status == ScreeningStatus.Passed))
            {
                return (RecruitmentJourneyStageKeys.Evaluation, "Tes & Wawancara");
            }

            return (RecruitmentJourneyStageKeys.AdministrationScreening, "Seleksi Administrasi");
        }

        private static (string Outcome, string Label) ResolveJourneyOutcome(
            HrdCandidateApplication app,
            HrdCandidate candidate,
            HrdCandidateScreening? screening,
            OfferPrerequisiteEvaluation offerPrereq,
            HrdJobOffer? offer,
            HrdPreEmploymentMedicalCheck? mcu,
            HrdCandidateHiringHandoff? handoff,
            HrdOnboardingCase? onboardingCase)
        {
            if (onboardingCase?.CaseStatus == OnboardingCaseStatus.Cancelled || handoff?.Status == HandoffStatus.Cancelled)
                return (RecruitmentJourneyOutcomeStatuses.CancelledOnboarding, "Dibatalkan pada Tahap Serah Terima/Onboarding");
            if (mcu?.ClearanceResult == McuClearanceResult.Unfit || string.Equals(app.StageStatus, "MCU_UNFIT_OFFER_REVOKED", StringComparison.OrdinalIgnoreCase))
                return (RecruitmentJourneyOutcomeStatuses.McuUnfitRevoked, "Dibatalkan Otomatis — MCU Tidak Laik Kerja (UNFIT)");
            if (app.CurrentStage == ApplicationStage.Withdrawn)
                return (RecruitmentJourneyOutcomeStatuses.Withdrawn, "Lamaran Ditarik Mundur");
            if (candidate.Status == CandidateStatus.Blacklisted || offerPrereq.HasClinicalVeto
                || string.Equals(app.StageStatus, "ASSESSMENT_FAILED", StringComparison.OrdinalIgnoreCase)
                || string.Equals(app.StageStatus, "REJECTED_ADMINISTRATION", StringComparison.OrdinalIgnoreCase)
                || screening?.Status == ScreeningStatus.Rejected
                || app.CurrentStage == ApplicationStage.Rejected)
                return (RecruitmentJourneyOutcomeStatuses.Rejected, "Gugur / Tidak Lolos Seleksi");
            if (offer != null && (offer.Status == JobOfferStatus.ConditionalRejected || offer.Status == JobOfferStatus.FinalRejected))
                return (RecruitmentJourneyOutcomeStatuses.OfferDeclined, "Penawaran Ditolak Kandidat");
            if (offer != null && offer.Status == JobOfferStatus.Expired)
                return (RecruitmentJourneyOutcomeStatuses.OfferExpired, "Penawaran Kedaluwarsa");
            if (handoff != null && onboardingCase != null && (handoff.Status == HandoffStatus.Completed || handoff.Status == HandoffStatus.TransferredToOnboarding))
                return (RecruitmentJourneyOutcomeStatuses.Hired, "Diterima — Masuk Onboarding");
            if (app.CurrentStage == ApplicationStage.TalentPool || candidate.Status == CandidateStatus.TalentPool)
                return (RecruitmentJourneyOutcomeStatuses.TalentPool, "Masuk Talent Pool");
            if (mcu?.ClearanceResult == McuClearanceResult.FitWithRestrictions || (handoff != null && onboardingCase == null))
                return (RecruitmentJourneyOutcomeStatuses.Blocked, "Tertahan — Memerlukan Tindakan Khusus");
            return (RecruitmentJourneyOutcomeStatuses.Active, "Proses Rekrutmen Berjalan");
        }

        private static JourneyNextActionDto? ResolveNextAction(
            List<JourneyStageDto> stages,
            List<JourneyActionDto> globalAllowedActions,
            string currentStageKey,
            string journeyOutcome,
            HrdCandidateScreening? screening,
            OfferPrerequisiteEvaluation offerPrereq,
            HrdJobOffer? offer,
            HrdPreEmploymentMedicalCheck? mcu,
            HrdCandidateHiringHandoff? handoff,
            HrdOnboardingCase? onboardingCase,
            HandoffPrerequisiteEvaluation handoffPrereq,
            HrdTalentPool? talentPool,
            TalentPoolEligibilityEvaluation talentPoolEligibility,
            bool vacancyQuotaFull,
            bool isFastTrack,
            bool isClinical,
            Guid candidateId,
            Guid applicationId,
            Guid actorUserId,
            JourneyViewerPermissionsDto perms)
        {
            if (journeyOutcome is RecruitmentJourneyOutcomeStatuses.Rejected
                or RecruitmentJourneyOutcomeStatuses.Withdrawn
                or RecruitmentJourneyOutcomeStatuses.McuUnfitRevoked
                or RecruitmentJourneyOutcomeStatuses.OfferDeclined
                or RecruitmentJourneyOutcomeStatuses.OfferExpired
                or RecruitmentJourneyOutcomeStatuses.CancelledOnboarding)
            {
                return new JourneyNextActionDto
                {
                    ActionCode = "JOURNEY_TERMINATED",
                    Title = "Proses Lamaran Telah Berakhir",
                    Description = "Tidak ada tindak lanjut aktif pada lamaran ini karena status terminal telah ditetapkan.",
                    StageKey = currentStageKey,
                    ResponsibleRole = "Selesai"
                };
            }

            static JourneyNextActionDto ToNextAction(JourneyActionDto action, string responsibleRole) => new()
            {
                ActionCode = action.ActionCode,
                Title = action.Label,
                Description = action.Description,
                StageKey = action.StageKey,
                ResponsibleRole = responsibleRole,
                TargetRoute = action.RoutePath
            };

            var current = stages.FirstOrDefault(s => s.StageKey == currentStageKey);

            if (handoff != null && onboardingCase != null)
            {
                var confirmAct = current?.AllowedActions.FirstOrDefault(a => a.ActionCode == "CONFIRM_HIRING_HANDOFF");
                if (handoff.Status != HandoffStatus.Completed && confirmAct != null)
                {
                    return ToNextAction(confirmAct, current!.ResponsibleRole);
                }

                if (perms.CanReadOnboarding)
                {
                    return new JourneyNextActionDto
                    {
                        ActionCode = "CONTINUE_ONBOARDING",
                        Title = "Lanjutkan di Kasus Onboarding",
                        Description = $"Pantau kesiapan kerja pegawai melalui kasus onboarding {onboardingCase.CaseNumber}.",
                        StageKey = RecruitmentJourneyStageKeys.HandoffOnboarding,
                        ResponsibleRole = "Koordinator Onboarding Unit/HR",
                        TargetRoute = $"/hr/lifecycle/onboarding-cases/{onboardingCase.Id}"
                    };
                }

                return new JourneyNextActionDto
                {
                    ActionCode = "AWAITING_ONBOARDING_ACCESS",
                    Title = "Menunggu Tim Onboarding Unit/HR",
                    Description = "Kasus onboarding sudah dibuat, namun akun Anda tidak memiliki izin WorkforceOnboardingCase.Read untuk membukanya.",
                    StageKey = RecruitmentJourneyStageKeys.HandoffOnboarding,
                    ResponsibleRole = "Koordinator Onboarding Unit/HR"
                };
            }

            if (handoff != null && onboardingCase == null)
            {
                var recoverAct = current?.AllowedActions.FirstOrDefault(a => a.ActionCode == "RECOVER_HANDOFF_ONBOARDING");
                if (recoverAct != null)
                {
                    return ToNextAction(recoverAct, current!.ResponsibleRole);
                }

                return new JourneyNextActionDto
                {
                    ActionCode = "AWAITING_HANDOFF_PERMISSION",
                    Title = "Menunggu Pemulihan Kasus Onboarding",
                    Description = "Dokumen serah terima tercatat tetapi kasus onboarding belum terbentuk. Menunggu petugas berwenang (izin HiringHandoff.Execute).",
                    StageKey = RecruitmentJourneyStageKeys.HandoffOnboarding,
                    ResponsibleRole = "HR Recruitment & Onboarding"
                };
            }

            if (currentStageKey == RecruitmentJourneyStageKeys.AdministrationScreening
                && !isFastTrack
                && (screening == null || screening.Status == ScreeningStatus.Pending))
            {
                var screenAct = current?.AllowedActions.FirstOrDefault(a =>
                    a.ActionCode is "EVALUATE_SCREENING" or "INITIATE_SCREENING");
                if (screenAct != null)
                {
                    return ToNextAction(screenAct, current!.ResponsibleRole);
                }

                return new JourneyNextActionDto
                {
                    ActionCode = "AWAITING_SCREENING_PERMISSION",
                    Title = "Periksa Seleksi Administrasi",
                    Description = perms.CanReadScreening
                        ? "Lamaran baru menunggu pemeriksaan Seleksi Administrasi oleh Tim Seleksi Administrasi HR (izin CandidateScreening.Evaluate)."
                        : "Tahap aktif berada pada Seleksi Administrasi. Pemeriksaan dibatasi untuk Tim Seleksi Administrasi HR (izin CandidateScreening.Read & CandidateScreening.Evaluate).",
                    StageKey = RecruitmentJourneyStageKeys.AdministrationScreening,
                    ResponsibleRole = "Tim Seleksi Administrasi HR"
                };
            }

            var enrollPoolAct = globalAllowedActions.FirstOrDefault(a => a.ActionCode == "ENROLL_TALENT_POOL");

            if (currentStageKey == RecruitmentJourneyStageKeys.Evaluation)
            {
                if (offerPrereq.CanCreateOffer && vacancyQuotaFull)
                {
                    if (enrollPoolAct != null)
                    {
                        return ToNextAction(enrollPoolAct, "HR Rekrutmen");
                    }

                    if (talentPool != null && talentPool.Status == TalentPoolStatus.Active && !talentPool.IsDelete)
                    {
                        var fastTrackAct = globalAllowedActions.FirstOrDefault(a => a.ActionCode == "FAST_TRACK_PROMOTE_TALENT_POOL");
                        if (fastTrackAct != null)
                        {
                            return ToNextAction(fastTrackAct, "HR Rekrutmen");
                        }

                        return new JourneyNextActionDto
                        {
                            ActionCode = "TALENT_POOL_ENROLLED",
                            Title = "Terdaftar di Talent Pool",
                            Description = "Kandidat cadangan sudah aktif di Talent Pool.",
                            StageKey = currentStageKey,
                            ResponsibleRole = "HR Rekrutmen"
                        };
                    }

                    return new JourneyNextActionDto
                    {
                        ActionCode = "AWAITING_TALENT_POOL_PERMISSION",
                        Title = "Menunggu Pendaftaran ke Talent Pool",
                        Description = "Kandidat berkualifikasi dan kuota lowongan asal penuh. Menunggu petugas berwenang (izin TalentPool.Create) mendaftarkan ke Talent Pool.",
                        StageKey = currentStageKey,
                        ResponsibleRole = "HR Rekrutmen"
                    };
                }

                if (!offerPrereq.HrCompleted)
                {
                    var hrAct = current?.AllowedActions.FirstOrDefault(a => a.ActionCode == "SCHEDULE_OR_SUBMIT_HR_EVALUATION");
                    if (hrAct != null)
                    {
                        return ToNextAction(hrAct, "Pewawancara HR");
                    }

                    return new JourneyNextActionDto
                    {
                        ActionCode = "AWAITING_EVALUATION_PERMISSION",
                        Title = "Menunggu Wawancara HR",
                        Description = "Seleksi administrasi lulus; menunggu Pewawancara HR (izin CandidateEvaluation.Evaluate) menjadwalkan atau mengirimkan nilai.",
                        StageKey = RecruitmentJourneyStageKeys.Evaluation,
                        ResponsibleRole = "Pewawancara HR"
                    };
                }

                if (isClinical && !offerPrereq.ClinicalCompleted)
                {
                    var clinAct = current?.AllowedActions.FirstOrDefault(a => a.ActionCode == "SUBMIT_CLINICAL_EVALUATION");
                    if (clinAct != null)
                    {
                        return ToNextAction(clinAct, "Asesor Klinis");
                    }

                    return new JourneyNextActionDto
                    {
                        ActionCode = "AWAITING_CLINICAL_ASSESSMENT_PERMISSION",
                        Title = "Menunggu Asesmen Klinis",
                        Description = "Wawancara HR selesai; menunggu Asesor Klinis (izin CandidateEvaluation.EvaluateClinical) mengirimkan hasil asesmen klinis.",
                        StageKey = RecruitmentJourneyStageKeys.Evaluation,
                        ResponsibleRole = "Asesor Klinis"
                    };
                }
            }

            if (currentStageKey == RecruitmentJourneyStageKeys.JobOffer)
            {
                if (offer == null)
                {
                    if (vacancyQuotaFull)
                    {
                        if (enrollPoolAct != null)
                        {
                            return ToNextAction(enrollPoolAct, "HR Rekrutmen");
                        }

                        return new JourneyNextActionDto
                        {
                            ActionCode = "AWAITING_TALENT_POOL_PERMISSION",
                            Title = "Menunggu Pendaftaran ke Talent Pool",
                            Description = "Kuota lowongan asal penuh dan kandidat memenuhi syarat. Menunggu petugas berwenang (izin TalentPool.Create).",
                            StageKey = currentStageKey,
                            ResponsibleRole = "HR Rekrutmen"
                        };
                    }

                    var createOfferAct = current?.AllowedActions.FirstOrDefault(a => a.ActionCode == "CREATE_JOB_OFFER");
                    if (createOfferAct != null)
                    {
                        return ToNextAction(createOfferAct, "HR Rekrutmen");
                    }

                    return new JourneyNextActionDto
                    {
                        ActionCode = "AWAITING_OFFER_CREATE_PERMISSION",
                        Title = "Buat Penawaran Kerja",
                        Description = "Evaluasi kandidat memenuhi syarat dan kuota tersedia; menunggu petugas HR berwenang (izin JobOffer.Create) menyusun Surat Penawaran Kerja.",
                        StageKey = RecruitmentJourneyStageKeys.JobOffer,
                        ResponsibleRole = "HR Rekrutmen"
                    };
                }

                if (offer.Status is JobOfferStatus.Draft or JobOfferStatus.PendingApproval)
                {
                    var approveAct = current?.AllowedActions.FirstOrDefault(a => a.ActionCode == "DECIDE_OFFER_APPROVAL");
                    if (approveAct != null)
                    {
                        return ToNextAction(approveAct, "Pejabat Penyetuju Penawaran");
                    }

                    return new JourneyNextActionDto
                    {
                        ActionCode = "AWAITING_OFFER_APPROVAL_PERMISSION",
                        Title = "Proses Approval Penawaran",
                        Description = offer.CreateBy == actorUserId && actorUserId != Guid.Empty
                            ? "Anda adalah pembuat penawaran; sesuai prinsip pemisahan tugas, persetujuan menunggu pejabat penyetuju lain (izin JobOfferApproval.Approve)."
                            : "Surat penawaran sedang menunggu persetujuan berjenjang oleh pejabat penyetuju yang memiliki izin JobOfferApproval.Approve.",
                        StageKey = RecruitmentJourneyStageKeys.JobOffer,
                        ResponsibleRole = "Pejabat Penyetuju Penawaran",
                        TargetRoute = perms.CanReadOffer || perms.CanReadOfferApproval
                            ? $"/hr/recruitment/offers/{offer.Id}?applicationId={applicationId}"
                            : null
                    };
                }

                if (offer.Status == JobOfferStatus.Approved)
                {
                    var sendAct = current?.AllowedActions.FirstOrDefault(a => a.ActionCode == "SEND_CONDITIONAL_OFFER");
                    if (sendAct != null)
                    {
                        return ToNextAction(sendAct, "HR Rekrutmen");
                    }

                    return new JourneyNextActionDto
                    {
                        ActionCode = "AWAITING_OFFER_SEND_PERMISSION",
                        Title = "Kirim Penawaran Kerja",
                        Description = "Surat penawaran telah disetujui penuh; menunggu petugas HR dengan izin JobOffer.Update mengirimkan surat ke kandidat.",
                        StageKey = RecruitmentJourneyStageKeys.JobOffer,
                        ResponsibleRole = "HR Rekrutmen",
                        TargetRoute = perms.CanReadOffer
                            ? $"/hr/recruitment/offers/{offer.Id}?applicationId={applicationId}"
                            : null
                    };
                }

                if (offer.Status is JobOfferStatus.SentConditional or JobOfferStatus.FinalOfferSent)
                {
                    var respondAct = current?.AllowedActions.FirstOrDefault(a => a.ActionCode == "RECORD_CANDIDATE_OFFER_RESPONSE");
                    if (respondAct != null)
                    {
                        return ToNextAction(respondAct, "HR Rekrutmen & Kandidat");
                    }

                    return new JourneyNextActionDto
                    {
                        ActionCode = "AWAITING_CANDIDATE_RESPONSE_PERMISSION",
                        Title = "Catat Respons Kandidat",
                        Description = "Surat penawaran telah dikirim; menunggu pencatatan jawaban kandidat oleh petugas HR dengan izin JobOffer.Update.",
                        StageKey = RecruitmentJourneyStageKeys.JobOffer,
                        ResponsibleRole = "HR Rekrutmen & Kandidat",
                        TargetRoute = perms.CanReadOffer
                            ? $"/hr/recruitment/offers/{offer.Id}?applicationId={applicationId}"
                            : null
                    };
                }

                if (offer.Status == JobOfferStatus.RejectedByApprover && enrollPoolAct != null)
                {
                    return ToNextAction(enrollPoolAct, "HR Rekrutmen");
                }
            }

            if (currentStageKey == RecruitmentJourneyStageKeys.PreEmploymentMcu)
            {
                if (mcu == null)
                {
                    var regMcuAct = current?.AllowedActions.FirstOrDefault(a => a.ActionCode == "REGISTER_MCU_REFERRAL");
                    if (regMcuAct != null)
                    {
                        return ToNextAction(regMcuAct, current!.ResponsibleRole);
                    }

                    if (offer != null && offer.Status == JobOfferStatus.ConditionalAccepted)
                    {
                        return new JourneyNextActionDto
                        {
                            ActionCode = "AWAITING_MCU_REFERRAL_PERMISSION",
                            Title = "Menunggu Penerbitan Rujukan MCU",
                            Description = perms.CanReadMcu
                                ? "Kandidat telah menyetujui penawaran bersyarat dan siap masuk tahap MCU Pra-Kerja. Akun Anda tidak memiliki izin OccupationalHealthClearance.Create; proses menunggu Tim K3RS / petugas yang berwenang."
                                : "Tahap aktif berada pada MCU Pra-Kerja. Rincian dan penerbitan rujukan MCU menunggu petugas yang memiliki izin OccupationalHealthClearance.Read dan OccupationalHealthClearance.Create.",
                            StageKey = RecruitmentJourneyStageKeys.PreEmploymentMcu,
                            ResponsibleRole = "Tim K3RS / Petugas Berwenang MCU"
                        };
                    }
                }
                else if (mcu.ClearanceResult == McuClearanceResult.Pending)
                {
                    var recMcuAct = current?.AllowedActions.FirstOrDefault(a => a.ActionCode == "RECORD_MCU_RESULT");
                    if (recMcuAct != null)
                    {
                        return ToNextAction(recMcuAct, current!.ResponsibleRole);
                    }

                    return new JourneyNextActionDto
                    {
                        ActionCode = "AWAITING_MCU_RESULT_PERMISSION",
                        Title = "Menunggu Pencatatan Hasil MCU",
                        Description = perms.CanReadMcu
                            ? $"Rujukan MCU {mcu.ReferralNumber} sudah diterbitkan. Menunggu Dokter Pemeriksa / Tim K3RS (izin OccupationalHealthClearance.Evaluate) mencatat hasil kelaikan kerja."
                            : "Rujukan MCU Pra-Kerja sudah diterbitkan; menunggu pencatatan hasil oleh Dokter Pemeriksa / Tim K3RS.",
                        StageKey = RecruitmentJourneyStageKeys.PreEmploymentMcu,
                        ResponsibleRole = "Tim K3RS / Dokter Pemeriksa",
                        TargetRoute = perms.CanReadMcu ? $"/hr/recruitment/mcu-clearance/{mcu.Id}?applicationId={applicationId}" : null
                    };
                }
                else if (mcu.ClearanceResult == McuClearanceResult.FitWithRestrictions)
                {
                    return new JourneyNextActionDto
                    {
                        ActionCode = "MCU_FIT_WITH_RESTRICTIONS_CONFIRMATION_REQUIRED",
                        Title = "Konfirmasi Penyesuaian Hasil MCU",
                        Description = "Hasil MCU menyatakan Fit With Restrictions (Laik dengan Catatan Pembatasan). Diperlukan konfirmasi penyesuaian penugasan/kontrak kerja sebelum serah terima dapat dilanjutkan.",
                        StageKey = RecruitmentJourneyStageKeys.PreEmploymentMcu,
                        ResponsibleRole = "Tim HR & Tim K3RS",
                        TargetRoute = perms.CanReadMcu ? $"/hr/recruitment/mcu-clearance/{mcu.Id}?applicationId={applicationId}" : null
                    };
                }
            }

            if (currentStageKey == RecruitmentJourneyStageKeys.HandoffOnboarding)
            {
                var handoffAct = current?.AllowedActions.FirstOrDefault(a =>
                    a.ActionCode is "GENERATE_HIRING_HANDOFF" or "RECOVER_HANDOFF_ONBOARDING" or "CONFIRM_HIRING_HANDOFF");
                if (handoffAct != null)
                {
                    return ToNextAction(handoffAct, current!.ResponsibleRole);
                }

                if (handoff == null && handoffPrereq.IsEligible)
                {
                    return new JourneyNextActionDto
                    {
                        ActionCode = "AWAITING_HANDOFF_PERMISSION",
                        Title = "Menunggu Serah Terima Kandidat",
                        Description = "Kandidat telah dinyatakan FIT pada MCU Pra-Kerja dan penawaran final terpenuhi. Menunggu petugas berwenang (izin HiringHandoff.Execute) menjalankan serah terima ke Onboarding.",
                        StageKey = RecruitmentJourneyStageKeys.HandoffOnboarding,
                        ResponsibleRole = "HR Recruitment & Onboarding"
                    };
                }
            }

            if (enrollPoolAct != null)
            {
                return ToNextAction(enrollPoolAct, "HR Rekrutmen");
            }

            if (current?.Blockers.Count > 0)
            {
                var firstBlocker = current.Blockers[0];
                return new JourneyNextActionDto
                {
                    ActionCode = firstBlocker.BlockerCode,
                    Title = firstBlocker.Title,
                    Description = firstBlocker.Message,
                    StageKey = currentStageKey,
                    ResponsibleRole = current.ResponsibleRole,
                    TargetRoute = offer != null && (perms.CanReadOffer || perms.CanReadOfferApproval)
                        ? $"/hr/recruitment/offers/{offer.Id}?applicationId={applicationId}"
                        : $"/hr/recruitment/candidates/{candidateId}?applicationId={applicationId}"
                };
            }

            return new JourneyNextActionDto
            {
                ActionCode = "VIEW_STAGE_STATUS",
                Title = "Pantau Progres Tahap Aktif",
                Description = current?.Summary ?? "Menunggu tindak lanjut peran yang berwenang.",
                StageKey = currentStageKey,
                ResponsibleRole = current?.ResponsibleRole ?? "Tim HR"
            };
        }

        private static List<JourneyHistoryItemDto> BuildHistory(
            HrdCandidateApplication app,
            HrdCandidateScreening? screening,
            List<HrdCandidateEvaluation> evaluations,
            HrdJobOffer? offer,
            HrdPreEmploymentMedicalCheck? mcu,
            HrdCandidateHiringHandoff? handoff,
            HrdOnboardingCase? onboardingCase,
            HrdTalentPool? talentPool,
            Guid actorUserId,
            JourneyViewerPermissionsDto perms)
        {
            var items = new List<JourneyHistoryItemDto>
            {
                new()
                {
                    Timestamp = app.ApplicationDate,
                    StageKey = RecruitmentJourneyStageKeys.Registration,
                    EventCode = "APPLICATION_REGISTERED",
                    Title = "Lamaran Didaftarkan",
                    Description = $"Lamaran {app.ApplicationNumber} diterima pada sistem.",
                    ActorName = "Recruiter / Pelamar"
                }
            };

            if (perms.CanReadScreening && screening != null)
            {
                items.Add(new JourneyHistoryItemDto
                {
                    Timestamp = screening.ScreeningDate,
                    StageKey = RecruitmentJourneyStageKeys.AdministrationScreening,
                    EventCode = "SCREENING_INITIATED",
                    Title = "Seleksi Administrasi Diinisiasi",
                    Description = screening.IsStrRequired ? "Pemeriksaan administrasi & STR dimulai." : "Pemeriksaan administrasi non-klinis dimulai.",
                    ActorName = "Tim Seleksi Administrasi"
                });

                if (screening.Status != ScreeningStatus.Pending)
                {
                    items.Add(new JourneyHistoryItemDto
                    {
                        Timestamp = screening.UpdateDateTime ?? screening.ScreeningDate,
                        StageKey = RecruitmentJourneyStageKeys.AdministrationScreening,
                        EventCode = $"SCREENING_{screening.Status.ToString().ToUpperInvariant()}",
                        Title = screening.Status == ScreeningStatus.Passed ? "Lulus Seleksi Administrasi" : "Gugur Seleksi Administrasi",
                        Description = screening.Notes ?? screening.ClinicalScreeningResult.ToString(),
                        ActorName = "Tim Seleksi Administrasi"
                    });
                }
            }

            if (perms.CanReadEvaluation)
            {
                var visible = perms.CanViewAllEvaluations
                    ? evaluations
                    : evaluations.Where(e => e.EvaluatorUserId == actorUserId).ToList();

                foreach (var eval in visible)
                {
                    items.Add(new JourneyHistoryItemDto
                    {
                        Timestamp = eval.CreateDateTime,
                        StageKey = RecruitmentJourneyStageKeys.Evaluation,
                        EventCode = "EVALUATION_SCHEDULED",
                        Title = $"Sesi {eval.EvaluationType} Dijadwalkan",
                        Description = $"Penilai: {eval.EvaluatorName} ({eval.EvaluatorRole}).",
                        ActorName = eval.EvaluatorName
                    });

                    if (RecruitmentJourneyPolicy.IsEvaluationSubmitted(eval))
                    {
                        items.Add(new JourneyHistoryItemDto
                        {
                            Timestamp = eval.UpdateDateTime!.Value,
                            StageKey = RecruitmentJourneyStageKeys.Evaluation,
                            EventCode = eval.IsClinicalVetoApplied ? "EVALUATION_CLINICAL_VETO" : "EVALUATION_SUBMITTED",
                            Title = eval.IsClinicalVetoApplied
                                ? $"Hak Veto Klinis pada {eval.EvaluationType}"
                                : $"Nilai {eval.EvaluationType} Dikirim ({eval.Recommendation})",
                            Description = eval.IsClinicalVetoApplied
                                ? (eval.VetoReason ?? "Kandidat tidak direkomendasikan secara klinis.")
                                : $"Skor: {eval.Score:0.##} — Rekomendasi: {eval.Recommendation}.",
                            ActorName = eval.EvaluatorName
                        });
                    }
                }
            }

            if ((perms.CanReadOffer || perms.CanReadOfferApproval) && offer != null)
            {
                items.Add(new JourneyHistoryItemDto
                {
                    Timestamp = offer.CreateDateTime,
                    StageKey = RecruitmentJourneyStageKeys.JobOffer,
                    EventCode = "OFFER_CREATED",
                    Title = $"Draf Penawaran {offer.OfferNumber} Dibuat",
                    Description = $"Posisi: {offer.OfferedPositionTitle} ({offer.ApprovalTier}).",
                    ActorName = "Tim Compensation / HR"
                });

                foreach (var step in offer.Approvals.Where(a => !a.IsDelete && a.DecisionDate.HasValue))
                {
                    items.Add(new JourneyHistoryItemDto
                    {
                        Timestamp = step.DecisionDate!.Value,
                        StageKey = RecruitmentJourneyStageKeys.JobOffer,
                        EventCode = $"OFFER_APPROVAL_{step.Status.ToString().ToUpperInvariant()}",
                        Title = $"Persetujuan Tahap #{step.StepNumber}: {step.Status}",
                        Description = $"{step.ApproverRole} ({step.ApproverName ?? "-"}).",
                        ActorName = step.ApproverName ?? step.ApproverRole
                    });
                }

                if (offer.CandidateResponseDate.HasValue)
                {
                    items.Add(new JourneyHistoryItemDto
                    {
                        Timestamp = offer.CandidateResponseDate.Value,
                        StageKey = RecruitmentJourneyStageKeys.JobOffer,
                        EventCode = $"OFFER_{offer.Status.ToString().ToUpperInvariant()}",
                        Title = $"Respons Kandidat: {offer.Status}",
                        Description = offer.CandidateResponseNotes ?? "Respons kandidat tercatat pada surat penawaran.",
                        ActorName = "Kandidat / HR"
                    });
                }
            }

            if (perms.CanReadMcu && mcu != null)
            {
                items.Add(new JourneyHistoryItemDto
                {
                    Timestamp = mcu.ReferralDate,
                    StageKey = RecruitmentJourneyStageKeys.PreEmploymentMcu,
                    EventCode = "MCU_REFERRED",
                    Title = $"Rujukan MCU {mcu.ReferralNumber} Diterbitkan",
                    Description = $"Fasilitas: {mcu.ClinicOrFacility}.",
                    ActorName = "Tim HR / K3RS"
                });

                if (mcu.ClearanceResult != McuClearanceResult.Pending && (mcu.CheckDate.HasValue || mcu.UpdateDateTime.HasValue))
                {
                    items.Add(new JourneyHistoryItemDto
                    {
                        Timestamp = mcu.UpdateDateTime ?? mcu.CheckDate!.Value,
                        StageKey = RecruitmentJourneyStageKeys.PreEmploymentMcu,
                        EventCode = $"MCU_RESULT_{mcu.ClearanceResult.ToString().ToUpperInvariant()}",
                        Title = $"Hasil MCU: {mcu.ClearanceResult}",
                        Description = $"Dokter Pemeriksa: {mcu.ExaminingDoctorName ?? "-"}.",
                        ActorName = mcu.ExaminingDoctorName ?? "Dokter K3RS"
                    });
                }
            }

            if ((perms.CanReadHandoff || perms.CanReadOnboarding) && handoff != null)
            {
                items.Add(new JourneyHistoryItemDto
                {
                    Timestamp = handoff.HandoffDate,
                    StageKey = RecruitmentJourneyStageKeys.HandoffOnboarding,
                    EventCode = "HANDOFF_GENERATED",
                    Title = $"Serah Terima {handoff.HandoffNumber} Terbentuk",
                    Description = $"Status: {handoff.Status}; Profil: {handoff.WorkforceProfile?.ProfileCode ?? "-"}.",
                    ActorName = "Tim HR"
                });
            }

            if (perms.CanReadOnboarding && onboardingCase != null)
            {
                items.Add(new JourneyHistoryItemDto
                {
                    Timestamp = onboardingCase.CreateDateTime,
                    StageKey = RecruitmentJourneyStageKeys.HandoffOnboarding,
                    EventCode = "ONBOARDING_CASE_CREATED",
                    Title = $"Kasus Onboarding {onboardingCase.CaseNumber} Dibuka",
                    Description = $"Status kasus: {onboardingCase.CaseStatus}; target masuk {onboardingCase.PlannedStartDate:yyyy-MM-dd}.",
                    ActorName = "Sistem Onboarding"
                });
            }

            if (perms.CanReadTalentPool && talentPool != null)
            {
                items.Add(new JourneyHistoryItemDto
                {
                    Timestamp = talentPool.EnrolledDate,
                    StageKey = RecruitmentJourneyStageKeys.Evaluation,
                    EventCode = "TALENT_POOL_ENROLLED",
                    Title = $"Masuk Talent Pool (#{talentPool.RankingPosition})",
                    Description = $"Status: {talentPool.Status}; berlaku sampai {talentPool.RetentionExpiryDate:yyyy-MM-dd}.",
                    ActorName = "Tim Rekrutmen"
                });
            }

            return items.OrderByDescending(i => i.Timestamp).ToList();
        }

        private static string ToIndonesianStageStatusLabel(string status) => status switch
        {
            RecruitmentJourneyStageStatuses.Completed => "Selesai",
            RecruitmentJourneyStageStatuses.InProgress => "Sedang Berjalan",
            RecruitmentJourneyStageStatuses.Waiting => "Menunggu Tindakan",
            RecruitmentJourneyStageStatuses.Blocked => "Terblokir",
            RecruitmentJourneyStageStatuses.Rejected => "Ditolak / Gugur",
            RecruitmentJourneyStageStatuses.NotApplicable => "Tidak Berlaku",
            RecruitmentJourneyStageStatuses.Restricted => "Akses Terbatas",
            _ => "Tidak Diketahui"
        };
    }
}
