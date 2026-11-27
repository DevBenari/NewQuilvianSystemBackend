using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Shared.HumanResource.Helpers;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Services
{
    public class CandidateEvaluationService : ICandidateEvaluationService
    {
        private readonly ApplicationDbContext _dbContext;

        public CandidateEvaluationService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private const string OtherEvaluatorMessage = "Evaluasi ini ditugaskan kepada penilai lain.";
        private const string NotAssignedEvaluatorMessage = "Hanya penilai yang ditugaskan yang dapat mengirim nilai uji ini.";
        private const string UnknownTrackMessage = "Jalur evaluasi tidak dikenal.";
        private const string InvalidActorMessage = "Identitas pengguna tidak valid.";

        public async Task<RecruitmentResult<PagedResult<CandidateEvaluationListItemResponse>>> GetPagedAsync(
            CandidateEvaluationQuery query,
            ClaimsPrincipal user,
            bool canViewAll,
            CancellationToken cancellationToken = default)
        {
            // VR-PKB-06: jalur hanya HR atau CLINICAL.
            var track = string.IsNullOrWhiteSpace(query.Track) ? null : query.Track.Trim().ToUpperInvariant();
            if (track != null && track != CandidateEvaluationTracks.Hr && track != CandidateEvaluationTracks.Clinical)
            {
                return RecruitmentResult<PagedResult<CandidateEvaluationListItemResponse>>.Fail(UnknownTrackMessage);
            }

            var dbQuery = _dbContext.HrdCandidateEvaluations
                .AsNoTracking()
                .Include(e => e.Application)
                    .ThenInclude(a => a!.Candidate)
                .Include(e => e.Application)
                    .ThenInclude(a => a!.Vacancy)
                .Where(e => !e.IsDelete);

            // VR-PKB-07 (HRD-DEC-241): tanpa CandidateEvaluation : ViewAll hanya evaluasi milik penilai yang login.
            if (!canViewAll)
            {
                var actorUserId = user.GetUserId();
                if (actorUserId == Guid.Empty)
                {
                    return RecruitmentResult<PagedResult<CandidateEvaluationListItemResponse>>.Fail(InvalidActorMessage, 401);
                }

                dbQuery = dbQuery.Where(e => e.EvaluatorUserId == actorUserId);
            }

            if (track == CandidateEvaluationTracks.Hr)
            {
                dbQuery = dbQuery.Where(e => e.EvaluationType == EvaluationType.HrInterview);
            }
            else if (track == CandidateEvaluationTracks.Clinical)
            {
                dbQuery = dbQuery.Where(e =>
                    e.EvaluationType == EvaluationType.ClinicalAssessment ||
                    e.EvaluationType == EvaluationType.UserInterview);
            }

            // Belum dinilai = nilai belum pernah dikirim. Penjadwalan mengisi rekomendasi bawaan Considered dan skor 0,
            // sehingga penandanya adalah baris yang belum pernah diperbarui oleh submit-hr / submit-clinical.
            if (query.PendingOnly == true)
            {
                dbQuery = dbQuery.Where(e => e.UpdateDateTime == null);
            }

            if (query.ApplicationId.HasValue && query.ApplicationId.Value != Guid.Empty)
            {
                dbQuery = dbQuery.Where(e => e.ApplicationId == query.ApplicationId.Value);
            }

            if (query.EvaluationType.HasValue)
            {
                dbQuery = dbQuery.Where(e => e.EvaluationType == query.EvaluationType.Value);
            }

            if (query.Recommendation.HasValue)
            {
                dbQuery = dbQuery.Where(e => e.Recommendation == query.Recommendation.Value);
            }

            if (query.IsClinicalVetoApplied.HasValue)
            {
                dbQuery = dbQuery.Where(e => e.IsClinicalVetoApplied == query.IsClinicalVetoApplied.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var term = query.Search.Trim().ToLower();
                dbQuery = dbQuery.Where(e =>
                    (e.Application != null && e.Application.ApplicationNumber.ToLower().Contains(term)) ||
                    (e.Application != null && e.Application.Candidate != null && e.Application.Candidate.FullName.ToLower().Contains(term)) ||
                    e.EvaluatorName.ToLower().Contains(term));
            }

            var totalData = await dbQuery.CountAsync(cancellationToken);
            var pageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
            var pageSize = query.PageSize > 0 ? query.PageSize : 10;

            var items = await dbQuery
                .OrderByDescending(e => e.EvaluationDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new CandidateEvaluationListItemResponse
                {
                    Id = e.Id,
                    ApplicationId = e.ApplicationId,
                    ApplicationNumber = e.Application != null ? e.Application.ApplicationNumber : string.Empty,
                    CandidateId = e.Application != null ? e.Application.CandidateId : Guid.Empty,
                    CandidateCode = e.Application != null && e.Application.Candidate != null ? e.Application.Candidate.CandidateCode : string.Empty,
                    CandidateName = e.Application != null && e.Application.Candidate != null ? e.Application.Candidate.FullName : string.Empty,
                    VacancyTitle = e.Application != null && e.Application.Vacancy != null ? e.Application.Vacancy.Title : string.Empty,
                    IsClinicalVacancy = e.Application != null && e.Application.Vacancy != null && e.Application.Vacancy.IsClinical,
                    EvaluationType = e.EvaluationType,
                    EvaluatorName = e.EvaluatorName,
                    EvaluationDate = e.EvaluationDate,
                    Score = e.Score,
                    Recommendation = e.Recommendation,
                    IsClinicalVetoApplied = e.IsClinicalVetoApplied,
                    IsSubmitted = e.UpdateDateTime.HasValue
                })
                .ToListAsync(cancellationToken);

            var result = new PagedResult<CandidateEvaluationListItemResponse>
            {
                Items = items,
                TotalData = totalData,
                TotalPage = pageSize > 0 ? (int)Math.Ceiling((double)totalData / pageSize) : 0,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return RecruitmentResult<PagedResult<CandidateEvaluationListItemResponse>>.Ok(result, "Daftar evaluasi kandidat berhasil diambil.");
        }

        public async Task<RecruitmentResult<PagedResult<EvaluationEligibleApplicationResponse>>> GetEligibleApplicationsAsync(
            EvaluationEligibleQuery query,
            ClaimsPrincipal user,
            bool canViewAll,
            CancellationToken cancellationToken = default)
        {
            var track = string.IsNullOrWhiteSpace(query.Track) ? null : query.Track.Trim().ToUpperInvariant();
            if (track != null && track != CandidateEvaluationTracks.Hr && track != CandidateEvaluationTracks.Clinical)
            {
                return RecruitmentResult<PagedResult<EvaluationEligibleApplicationResponse>>.Fail(UnknownTrackMessage);
            }

            var actorUserId = user.GetUserId();
            if (!canViewAll && actorUserId == Guid.Empty)
            {
                return RecruitmentResult<PagedResult<EvaluationEligibleApplicationResponse>>.Fail(InvalidActorMessage, 401);
            }

            var dbQuery = _dbContext.HrdCandidateApplications
                .AsNoTracking()
                .Include(a => a.Candidate)
                .Include(a => a.Vacancy)
                .Include(a => a.Screening)
                .Include(a => a.Evaluations)
                .Where(a => !a.IsDelete
                            && a.Candidate != null && !a.Candidate.IsDelete && a.Candidate.Status != CandidateStatus.Blacklisted
                            && a.CurrentStage != ApplicationStage.Rejected
                            && a.CurrentStage != ApplicationStage.Withdrawn
                            && a.Screening != null && !a.Screening.IsDelete && a.Screening.Status == ScreeningStatus.Passed);

            if (track == CandidateEvaluationTracks.Clinical)
            {
                dbQuery = dbQuery.Where(a => a.Vacancy != null && a.Vacancy.IsClinical);
            }

            if (!canViewAll)
            {
                dbQuery = dbQuery.Where(a => a.Evaluations.Any(e => !e.IsDelete && e.EvaluatorUserId == actorUserId));
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var term = query.Search.Trim().ToLower();
                dbQuery = dbQuery.Where(a =>
                    a.ApplicationNumber.ToLower().Contains(term) ||
                    (a.Candidate != null && (a.Candidate.FullName.ToLower().Contains(term) || a.Candidate.CandidateCode.ToLower().Contains(term))) ||
                    (a.Vacancy != null && a.Vacancy.Title.ToLower().Contains(term)));
            }

            var apps = await dbQuery
                .OrderByDescending(a => a.ApplicationDate)
                .ToListAsync(cancellationToken);

            var projected = new List<EvaluationEligibleApplicationResponse>();
            foreach (var app in apps)
            {
                if (RecruitmentJourneyPolicy.IsTerminalApplication(app, app.Candidate))
                {
                    continue;
                }

                var evals = app.Evaluations.Where(e => !e.IsDelete).ToList();
                var hrEval = evals
                    .Where(e => e.EvaluationType == EvaluationType.HrInterview)
                    .OrderByDescending(e => RecruitmentJourneyPolicy.IsEvaluationSubmitted(e))
                    .ThenByDescending(e => e.UpdateDateTime ?? e.CreateDateTime)
                    .FirstOrDefault();

                var clinEval = evals
                    .Where(e => e.EvaluationType == EvaluationType.ClinicalAssessment || e.EvaluationType == EvaluationType.UserInterview)
                    .OrderByDescending(e => RecruitmentJourneyPolicy.IsEvaluationSubmitted(e))
                    .ThenByDescending(e => e.UpdateDateTime ?? e.CreateDateTime)
                    .FirstOrDefault();

                var isClinical = app.Vacancy?.IsClinical ?? false;
                var hrSubmitted = hrEval != null && RecruitmentJourneyPolicy.IsEvaluationSubmitted(hrEval);
                var clinSubmitted = clinEval != null && RecruitmentJourneyPolicy.IsEvaluationSubmitted(clinEval);

                if (query.PendingOnly == true)
                {
                    var isTrackPending = track == CandidateEvaluationTracks.Hr
                        ? !hrSubmitted
                        : track == CandidateEvaluationTracks.Clinical
                            ? isClinical && !clinSubmitted
                            : !hrSubmitted || (isClinical && !clinSubmitted);
                    if (!isTrackPending)
                    {
                        continue;
                    }
                }

                var offerPrereq = RecruitmentJourneyPolicy.EvaluateOfferPrerequisites(app, app.Vacancy, app.Screening, evals, app.Candidate);
                projected.Add(new EvaluationEligibleApplicationResponse
                {
                    ApplicationId = app.Id,
                    ApplicationNumber = app.ApplicationNumber,
                    CandidateId = app.CandidateId,
                    CandidateCode = app.Candidate?.CandidateCode ?? string.Empty,
                    CandidateName = app.Candidate?.FullName ?? string.Empty,
                    VacancyId = app.VacancyId,
                    VacancyTitle = app.Vacancy?.Title ?? string.Empty,
                    IsClinicalVacancy = isClinical,
                    CurrentStageName = app.CurrentStage.ToString(),
                    StageStatus = app.StageStatus,
                    HasHrSession = hrEval != null,
                    HrEvaluationId = hrEval?.Id,
                    HrIsSubmitted = hrSubmitted,
                    HrScore = hrSubmitted ? hrEval!.Score : null,
                    HrRecommendation = hrSubmitted ? hrEval!.Recommendation.ToString() : null,
                    HasClinicalSession = clinEval != null,
                    ClinicalEvaluationId = clinEval?.Id,
                    ClinicalIsSubmitted = clinSubmitted,
                    ClinicalScore = clinSubmitted ? clinEval!.Score : null,
                    ClinicalRecommendation = clinSubmitted ? clinEval!.Recommendation.ToString() : null,
                    HasClinicalVeto = offerPrereq.HasClinicalVeto,
                    CanCreateOffer = offerPrereq.CanCreateOffer
                });
            }

            var totalData = projected.Count;
            var pageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
            var pageSize = query.PageSize > 0 ? query.PageSize : 10;
            var pagedItems = projected.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            var pageResult = new PagedResult<EvaluationEligibleApplicationResponse>
            {
                Items = pagedItems,
                TotalData = totalData,
                TotalPage = pageSize > 0 ? (int)Math.Ceiling((double)totalData / pageSize) : 0,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return RecruitmentResult<PagedResult<EvaluationEligibleApplicationResponse>>.Ok(
                pageResult,
                "Daftar antrean evaluasi berdasarkan kelayakan lamaran berhasil diambil.");
        }

        public async Task<RecruitmentResult<CandidateEvaluationResponse>> GetByIdAsync(
            Guid id,
            ClaimsPrincipal user,
            bool canViewAll,
            CancellationToken cancellationToken = default)
        {
            var e = await _dbContext.HrdCandidateEvaluations
                .AsNoTracking()
                .Include(x => x.Application)
                    .ThenInclude(a => a!.Candidate)
                .Include(x => x.Application)
                    .ThenInclude(a => a!.Vacancy)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (e == null)
            {
                return RecruitmentResult<CandidateEvaluationResponse>.NotFound("Data evaluasi tidak ditemukan.");
            }

            // VR-PKB-07 (HRD-DEC-241): tanpa CandidateEvaluation : ViewAll, detail milik penilai lain ditolak.
            if (!canViewAll)
            {
                var actorUserId = user.GetUserId();
                if (actorUserId == Guid.Empty || e.EvaluatorUserId != actorUserId)
                {
                    return RecruitmentResult<CandidateEvaluationResponse>.Fail(OtherEvaluatorMessage, 403);
                }
            }

            return RecruitmentResult<CandidateEvaluationResponse>.Ok(MapToResponse(e), "Detail evaluasi kandidat berhasil diambil.");
        }

        /// <summary>Muat ulang detail sesudah penjadwalan atau pengiriman nilai oleh pemanggil yang sudah lolos pemeriksaan.</summary>
        private async Task<RecruitmentResult<CandidateEvaluationResponse>> LoadResponseAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            var e = await _dbContext.HrdCandidateEvaluations
                .AsNoTracking()
                .Include(x => x.Application)
                    .ThenInclude(a => a!.Candidate)
                .Include(x => x.Application)
                    .ThenInclude(a => a!.Vacancy)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (e == null)
            {
                return RecruitmentResult<CandidateEvaluationResponse>.NotFound("Data evaluasi tidak ditemukan.");
            }

            var response = MapToResponse(e);
            return RecruitmentResult<CandidateEvaluationResponse>.Ok(response, "Detail evaluasi kandidat berhasil diambil.");
        }

        public async Task<RecruitmentResult<CandidateEvaluationResponse>> ScheduleAsync(
            ScheduleEvaluationRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId(user);

            var application = await _dbContext.HrdCandidateApplications
                .Include(a => a.Candidate)
                .Include(a => a.Vacancy)
                .Include(a => a.Screening)
                .FirstOrDefaultAsync(a => a.Id == request.ApplicationId && !a.IsDelete, cancellationToken);

            if (application == null)
            {
                return RecruitmentResult<CandidateEvaluationResponse>.NotFound("Lamaran tidak ditemukan.");
            }

            // Validasi urutan & terminal: tidak boleh menjadwalkan jika lamaran gugur atau belum lolos screening (HRD-DEC-076)
            if (RecruitmentJourneyPolicy.IsTerminalApplication(application, application.Candidate))
            {
                return RecruitmentResult<CandidateEvaluationResponse>.Unprocessable("Lamaran telah dinyatakan gugur atau terminal dan tidak dapat dijadwalkan sesi evaluasi baru.");
            }

            if (application.Screening == null || application.Screening.IsDelete || application.Screening.Status != ScreeningStatus.Passed)
            {
                return RecruitmentResult<CandidateEvaluationResponse>.Unprocessable(
                    "Sesi evaluasi belum dapat dijadwalkan sebelum Seleksi Administrasi (Screening) dinyatakan Lulus (Passed) (HRD-DEC-076).");
            }

            var isClinical = application.Vacancy?.IsClinical ?? false;
            if (!isClinical && request.EvaluationType == EvaluationType.ClinicalAssessment)
            {
                return RecruitmentResult<CandidateEvaluationResponse>.Unprocessable(
                    "Sesi Uji Kompetensi Klinis hanya berlaku untuk lowongan posisi klinis.");
            }

            if (request.EvaluatorUserId == Guid.Empty)
            {
                return RecruitmentResult<CandidateEvaluationResponse>.Unprocessable("EvaluatorUserId wajib diisi dengan identitas penilai yang sah.");
            }

            var evaluation = new HrdCandidateEvaluation
            {
                Id = Guid.NewGuid(),
                ApplicationId = request.ApplicationId,
                EvaluationType = request.EvaluationType,
                EvaluatorUserId = request.EvaluatorUserId,
                EvaluatorName = request.EvaluatorName.Trim(),
                EvaluatorRole = request.EvaluatorRole.Trim(),
                EvaluationDate = request.EvaluationDate,
                Score = 0m,
                Recommendation = EvaluationRecommendation.Considered,
                IsClinicalVetoApplied = false,
                Notes = request.Notes?.Trim(),
                CreateDateTime = DateTime.UtcNow,
                CreateBy = userId
            };

            await _dbContext.HrdCandidateEvaluations.AddAsync(evaluation, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return await LoadResponseAsync(evaluation.Id, cancellationToken);
        }

        public async Task<RecruitmentResult<CandidateEvaluationResponse>> SubmitHrEvaluationAsync(
            Guid evaluationId,
            SubmitHrEvaluationRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId(user);
            if (userId == Guid.Empty)
            {
                return RecruitmentResult<CandidateEvaluationResponse>.Fail(InvalidActorMessage, 401);
            }

            var evaluation = await _dbContext.HrdCandidateEvaluations
                .Include(e => e.Application)
                    .ThenInclude(a => a!.Vacancy)
                .Include(e => e.Application)
                    .ThenInclude(a => a!.Candidate)
                .Include(e => e.Application)
                    .ThenInclude(a => a!.Screening)
                .FirstOrDefaultAsync(e => e.Id == evaluationId && !e.IsDelete, cancellationToken);

            if (evaluation == null)
            {
                return RecruitmentResult<CandidateEvaluationResponse>.NotFound("Data evaluasi tidak ditemukan.");
            }

            if (evaluation.EvaluationType != EvaluationType.HrInterview)
            {
                return RecruitmentResult<CandidateEvaluationResponse>.Unprocessable("Sesi evaluasi ini bukan tipe Wawancara HR.");
            }

            var app = evaluation.Application;
            if (app == null)
            {
                return RecruitmentResult<CandidateEvaluationResponse>.NotFound("Lamaran terkait tidak ditemukan.");
            }

            if (RecruitmentJourneyPolicy.IsTerminalApplication(app, app.Candidate))
            {
                return RecruitmentResult<CandidateEvaluationResponse>.Conflict("Lamaran ini telah dikunci karena gugur/terminal pada tahap sebelumnya.");
            }

            if (app.Screening == null || app.Screening.IsDelete || app.Screening.Status != ScreeningStatus.Passed)
            {
                return RecruitmentResult<CandidateEvaluationResponse>.Unprocessable(
                    "Nilai Wawancara HR tidak dapat dikirim sebelum Seleksi Administrasi dinyatakan Lulus.");
            }

            if (RecruitmentJourneyPolicy.IsEvaluationSubmitted(evaluation))
            {
                return RecruitmentResult<CandidateEvaluationResponse>.Conflict("Nilai evaluasi pada sesi ini sudah dikirim sebelumnya.");
            }

            evaluation.Score = request.Score;
            evaluation.Recommendation = request.Recommendation;
            evaluation.EvaluationDate = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(request.Notes))
            {
                evaluation.Notes = request.Notes.Trim();
            }
            evaluation.UpdateDateTime = DateTime.UtcNow;
            evaluation.UpdateBy = userId;

            if (request.Recommendation == EvaluationRecommendation.NotRecommended)
            {
                app.StageStatus = "ASSESSMENT_FAILED";
                app.Notes = "Tidak direkomendasikan pada hasil Wawancara HR.";
                app.UpdateDateTime = DateTime.UtcNow;
                app.UpdateBy = userId;
            }
            else
            {
                // Evaluasi kesiapan melangkah ke offering hanya jika seluruh syarat lulus telah dikirim
                await CheckAndAdvanceApplicationStageAsync(app, cancellationToken);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return await LoadResponseAsync(evaluation.Id, cancellationToken);
        }

        public async Task<RecruitmentResult<CandidateEvaluationResponse>> SubmitClinicalEvaluationAsync(
            Guid evaluationId,
            SubmitClinicalEvaluationRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId(user);

            var evaluation = await _dbContext.HrdCandidateEvaluations
                .Include(e => e.Application)
                    .ThenInclude(a => a!.Vacancy)
                .Include(e => e.Application)
                    .ThenInclude(a => a!.Candidate)
                .Include(e => e.Application)
                    .ThenInclude(a => a!.Screening)
                .FirstOrDefaultAsync(e => e.Id == evaluationId && !e.IsDelete, cancellationToken);

            if (evaluation == null)
            {
                return RecruitmentResult<CandidateEvaluationResponse>.NotFound("Data evaluasi tidak ditemukan.");
            }

            // VR-PKB-08 (HRD-ASM-98, HRD-DEC-241): hanya penilai yang ditugaskan, termasuk bagi pemegang ViewAll.
            var actorUserId = user.GetUserId();
            if (actorUserId == Guid.Empty || evaluation.EvaluatorUserId != actorUserId)
            {
                return RecruitmentResult<CandidateEvaluationResponse>.Fail(NotAssignedEvaluatorMessage, 403);
            }

            if (evaluation.EvaluationType != EvaluationType.ClinicalAssessment && evaluation.EvaluationType != EvaluationType.UserInterview)
            {
                return RecruitmentResult<CandidateEvaluationResponse>.Unprocessable("Sesi evaluasi ini bukan tipe Uji Kompetensi Klinis atau Wawancara User.");
            }

            var app = evaluation.Application;
            if (app == null)
            {
                return RecruitmentResult<CandidateEvaluationResponse>.NotFound("Lamaran terkait tidak ditemukan.");
            }

            if (RecruitmentJourneyPolicy.IsTerminalApplication(app, app.Candidate))
            {
                return RecruitmentResult<CandidateEvaluationResponse>.Conflict("Lamaran ini telah dikunci karena gugur atau terminal pada tahap sebelumnya.");
            }

            if (app.Screening == null || app.Screening.IsDelete || app.Screening.Status != ScreeningStatus.Passed)
            {
                return RecruitmentResult<CandidateEvaluationResponse>.Unprocessable(
                    "Asesmen klinis tidak dapat dikirim sebelum Seleksi Administrasi dinyatakan Lulus.");
            }

            if (RecruitmentJourneyPolicy.IsEvaluationSubmitted(evaluation))
            {
                return RecruitmentResult<CandidateEvaluationResponse>.Conflict("Nilai asesmen klinis pada sesi ini sudah dikirim sebelumnya.");
            }

            evaluation.Score = request.Score;
            evaluation.Recommendation = request.Recommendation;
            evaluation.EvaluationDate = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(request.Notes))
            {
                evaluation.Notes = request.Notes.Trim();
            }

            // Aturan Bisnis VR-REC-11, HRD-DEC-077: Hak Veto Klinis Mutlak jika NotRecommended
            if (request.Recommendation == EvaluationRecommendation.NotRecommended)
            {
                evaluation.IsClinicalVetoApplied = true;
                evaluation.VetoReason = string.IsNullOrWhiteSpace(request.VetoReason)
                    ? "VETO KLINIS MUTLAK: Kandidat tidak memenuhi standar kompetensi klinis rumah sakit (VR-REC-11)."
                    : request.VetoReason.Trim();

                // Kunci lamaran yang bersangkutan saja; jangan memutus lamaran aktif lain milik kandidat yang sama
                app.StageStatus = "ASSESSMENT_FAILED";
                app.Notes = $"Gugur evaluasi klinis. Alasan veto: {evaluation.VetoReason}";
                app.UpdateDateTime = DateTime.UtcNow;
                app.UpdateBy = userId;

                var hasOtherActiveApplications = await _dbContext.HrdCandidateApplications
                    .AnyAsync(a => a.CandidateId == app.CandidateId
                                   && a.Id != app.Id
                                   && !a.IsDelete
                                   && a.CurrentStage != ApplicationStage.Rejected
                                   && a.CurrentStage != ApplicationStage.Withdrawn
                                   && a.StageStatus != "REJECTED_ADMINISTRATION"
                                   && a.StageStatus != "ASSESSMENT_FAILED"
                                   && a.StageStatus != "MCU_UNFIT_OFFER_REVOKED", cancellationToken);

                if (!hasOtherActiveApplications && app.Candidate != null)
                {
                    app.Candidate.Status = CandidateStatus.Rejected;
                    app.Candidate.UpdateDateTime = DateTime.UtcNow;
                    app.Candidate.UpdateBy = userId;
                }
            }
            else
            {
                evaluation.IsClinicalVetoApplied = false;
                evaluation.VetoReason = null;

                evaluation.UpdateDateTime = DateTime.UtcNow;
                evaluation.UpdateBy = userId;

                // Evaluasi kesiapan melangkah ke offering hanya jika kedua evaluasi telah dikirim dan lulus
                await CheckAndAdvanceApplicationStageAsync(app, cancellationToken);
            }

            evaluation.UpdateDateTime = DateTime.UtcNow;
            evaluation.UpdateBy = userId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return await LoadResponseAsync(evaluation.Id, cancellationToken);
        }

        private async Task CheckAndAdvanceApplicationStageAsync(HrdCandidateApplication app, CancellationToken cancellationToken)
        {
            var screening = app.Screening ?? await _dbContext.HrdCandidateScreenings
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ApplicationId == app.Id && !s.IsDelete, cancellationToken);

            var allEvals = await _dbContext.HrdCandidateEvaluations
                .Where(e => e.ApplicationId == app.Id && !e.IsDelete)
                .ToListAsync(cancellationToken);

            var prereq = RecruitmentJourneyPolicy.EvaluateOfferPrerequisites(app, app.Vacancy, screening, allEvals, app.Candidate);
            if (prereq.CanCreateOffer)
            {
                app.CurrentStage = ApplicationStage.Offering;
                app.StageStatus = "EvaluationPassedReadyForOffer";
                app.UpdateDateTime = DateTime.UtcNow;
            }
        }

        private static CandidateEvaluationResponse MapToResponse(HrdCandidateEvaluation e)
        {
            return new CandidateEvaluationResponse
            {
                Id = e.Id,
                ApplicationId = e.ApplicationId,
                ApplicationNumber = e.Application?.ApplicationNumber ?? string.Empty,
                CandidateId = e.Application?.CandidateId ?? Guid.Empty,
                CandidateCode = e.Application?.Candidate?.CandidateCode ?? string.Empty,
                CandidateName = e.Application?.Candidate?.FullName ?? string.Empty,
                VacancyTitle = e.Application?.Vacancy?.Title ?? string.Empty,
                IsClinicalVacancy = e.Application?.Vacancy?.IsClinical ?? false,
                EvaluationType = e.EvaluationType,
                EvaluatorUserId = e.EvaluatorUserId,
                EvaluatorName = e.EvaluatorName,
                EvaluatorRole = e.EvaluatorRole,
                EvaluationDate = e.EvaluationDate,
                Score = e.Score,
                Recommendation = e.Recommendation,
                IsClinicalVetoApplied = e.IsClinicalVetoApplied,
                VetoReason = e.VetoReason,
                Notes = e.Notes,
                IsSubmitted = RecruitmentJourneyPolicy.IsEvaluationSubmitted(e),
                CreateDateTime = e.CreateDateTime
            };
        }

        private static Guid GetUserId(ClaimsPrincipal user)
        {
            var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? user.FindFirst("sub")?.Value;
            return Guid.TryParse(sub, out var guid) ? guid : Guid.Empty;
        }
    }
}
