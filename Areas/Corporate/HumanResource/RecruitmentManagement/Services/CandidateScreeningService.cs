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

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Services
{
    public class CandidateScreeningService : ICandidateScreeningService
    {
        private readonly ApplicationDbContext _dbContext;

        public CandidateScreeningService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<RecruitmentResult<PagedResult<CandidateScreeningListItemResponse>>> GetPagedAsync(
            CandidateScreeningQuery query,
            CancellationToken cancellationToken = default)
        {
            var dbQuery = _dbContext.HrdCandidateScreenings
                .AsNoTracking()
                .Include(s => s.Application)
                    .ThenInclude(a => a!.Candidate)
                .Include(s => s.Application)
                    .ThenInclude(a => a!.Vacancy)
                .Where(s => !s.IsDelete);

            if (query.VacancyId.HasValue && query.VacancyId.Value != Guid.Empty)
            {
                dbQuery = dbQuery.Where(s => s.Application != null && s.Application.VacancyId == query.VacancyId.Value);
            }

            if (query.Status.HasValue)
            {
                dbQuery = dbQuery.Where(s => s.Status == query.Status.Value);
            }

            if (query.ClinicalResult.HasValue)
            {
                dbQuery = dbQuery.Where(s => s.ClinicalScreeningResult == query.ClinicalResult.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var term = query.Search.Trim().ToLower();
                dbQuery = dbQuery.Where(s =>
                    (s.Application != null && s.Application.ApplicationNumber.ToLower().Contains(term)) ||
                    (s.Application != null && s.Application.Candidate != null && s.Application.Candidate.FullName.ToLower().Contains(term)) ||
                    (s.StrNumber != null && s.StrNumber.ToLower().Contains(term)));
            }

            var totalData = await dbQuery.CountAsync(cancellationToken);
            var pageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
            var pageSize = query.PageSize > 0 ? query.PageSize : 10;

            var items = await dbQuery
                .OrderByDescending(s => s.ScreeningDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new CandidateScreeningListItemResponse
                {
                    Id = s.Id,
                    ApplicationId = s.ApplicationId,
                    ApplicationNumber = s.Application != null ? s.Application.ApplicationNumber : string.Empty,
                    ApplicationStageStatus = s.Application != null ? s.Application.StageStatus : null,
                    CandidateId = s.Application != null ? s.Application.CandidateId : Guid.Empty,
                    CandidateCode = s.Application != null && s.Application.Candidate != null ? s.Application.Candidate.CandidateCode : string.Empty,
                    CandidateName = s.Application != null && s.Application.Candidate != null ? s.Application.Candidate.FullName : string.Empty,
                    VacancyTitle = s.Application != null && s.Application.Vacancy != null ? s.Application.Vacancy.Title : string.Empty,
                    IsClinicalVacancy = s.Application != null && s.Application.Vacancy != null && s.Application.Vacancy.IsClinical,
                    Status = s.Status,
                    ClinicalScreeningResult = s.ClinicalScreeningResult,
                    StrExpiryDate = s.StrExpiryDate,
                    ScreeningDate = s.ScreeningDate
                })
                .ToListAsync(cancellationToken);

            var candidateIds = items.Where(i => i.CandidateId != Guid.Empty).Select(i => i.CandidateId).Distinct().ToList();
            if (candidateIds.Count > 0)
            {
                var docs = await _dbContext.HrdCandidateDocuments
                    .AsNoTracking()
                    .Where(d => candidateIds.Contains(d.CandidateId) && !d.IsDelete)
                    .Select(d => new
                    {
                        d.CandidateId,
                        d.ApplicationId,
                        d.DocumentType,
                        d.ValidUntil
                    })
                    .ToListAsync(cancellationToken);

                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                var docsByCandidate = docs.GroupBy(d => d.CandidateId).ToDictionary(g => g.Key, g => g.ToList());

                foreach (var item in items)
                {
                    item.ClinicalScreeningResultLabel = RecruitmentJourneyPolicy.ResolveClinicalScreeningResultLabel(
                        item.Status,
                        item.IsClinicalVacancy,
                        item.ClinicalScreeningResult);

                    if (docsByCandidate.TryGetValue(item.CandidateId, out var cDocs))
                    {
                        var relevant = cDocs.Where(d => d.ApplicationId == null || d.ApplicationId == item.ApplicationId).ToList();
                        item.DocumentsCount = relevant.Count;
                        var strDocs = relevant.Where(d => d.DocumentType == DocumentType.StrCertificate).ToList();
                        item.HasStrDocument = strDocs.Count > 0;
                        item.HasExpiredStr = strDocs.Any(d => d.ValidUntil.HasValue && d.ValidUntil.Value < today);
                    }
                }
            }

            var result = new PagedResult<CandidateScreeningListItemResponse>
            {
                Items = items,
                TotalData = totalData,
                TotalPage = pageSize > 0 ? (int)Math.Ceiling((double)totalData / pageSize) : 0,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return RecruitmentResult<PagedResult<CandidateScreeningListItemResponse>>.Ok(result, "Daftar penapisan berkas berhasil diambil.");
        }

        public async Task<RecruitmentResult<CandidateScreeningResponse>> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var s = await _dbContext.HrdCandidateScreenings
                .AsNoTracking()
                .Include(x => x.Application)
                    .ThenInclude(a => a!.Candidate)
                .Include(x => x.Application)
                    .ThenInclude(a => a!.Vacancy)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (s == null)
            {
                return RecruitmentResult<CandidateScreeningResponse>.NotFound("Data penapisan berkas tidak ditemukan.");
            }

            var response = await BuildDetailedResponseAsync(s, cancellationToken);
            return RecruitmentResult<CandidateScreeningResponse>.Ok(response, "Detail penapisan berkas berhasil diambil.");
        }

        public async Task<RecruitmentResult<CandidateScreeningResponse>> GetByApplicationIdAsync(
            Guid applicationId,
            CancellationToken cancellationToken = default)
        {
            if (applicationId == Guid.Empty)
            {
                return RecruitmentResult<CandidateScreeningResponse>.Fail("ApplicationId tidak valid.", 400);
            }

            var s = await _dbContext.HrdCandidateScreenings
                .AsNoTracking()
                .Include(x => x.Application)
                    .ThenInclude(a => a!.Candidate)
                .Include(x => x.Application)
                    .ThenInclude(a => a!.Vacancy)
                .FirstOrDefaultAsync(x => x.ApplicationId == applicationId && !x.IsDelete, cancellationToken);

            if (s == null)
            {
                return RecruitmentResult<CandidateScreeningResponse>.NotFound("Data penapisan berkas untuk lamaran ini belum tersedia.");
            }

            var response = await BuildDetailedResponseAsync(s, cancellationToken);
            return RecruitmentResult<CandidateScreeningResponse>.Ok(response, "Detail penapisan berkas berdasarkan lamaran berhasil diambil.");
        }

        public async Task<RecruitmentResult<ScreeningSummaryResponse>> GetSummaryAsync(
            Guid? vacancyId,
            CancellationToken cancellationToken = default)
        {
            var screeningsQuery = _dbContext.HrdCandidateScreenings
                .AsNoTracking()
                .Include(s => s.Application)
                    .ThenInclude(a => a!.Vacancy)
                .Where(s => !s.IsDelete);

            if (vacancyId.HasValue && vacancyId.Value != Guid.Empty)
            {
                screeningsQuery = screeningsQuery.Where(s => s.Application != null && s.Application.VacancyId == vacancyId.Value);
            }

            var rows = await screeningsQuery
                .Select(s => new
                {
                    s.Status,
                    s.IsStrRequired,
                    IsClinical = s.Application != null && s.Application.Vacancy != null && s.Application.Vacancy.IsClinical
                })
                .ToListAsync(cancellationToken);

            var missingQuery = _dbContext.HrdCandidateApplications
                .AsNoTracking()
                .Where(a => !a.IsDelete
                            && a.CurrentStage == ApplicationStage.Screening
                            && a.StageStatus == "Submitted"
                            && a.Candidate != null && !a.Candidate.IsDelete && a.Candidate.Status != CandidateStatus.Blacklisted
                            && a.Vacancy != null && !a.Vacancy.IsDelete
                            && !_dbContext.HrdCandidateScreenings.Any(s => s.ApplicationId == a.Id && !s.IsDelete));

            if (vacancyId.HasValue && vacancyId.Value != Guid.Empty)
            {
                missingQuery = missingQuery.Where(a => a.VacancyId == vacancyId.Value);
            }

            var missingEligible = await missingQuery.CountAsync(cancellationToken);

            var summary = new ScreeningSummaryResponse
            {
                TotalScreenings = rows.Count,
                PendingCount = rows.Count(r => r.Status == ScreeningStatus.Pending),
                PassedCount = rows.Count(r => r.Status == ScreeningStatus.Passed),
                RejectedCount = rows.Count(r => r.Status == ScreeningStatus.Rejected),
                ClinicalPendingCount = rows.Count(r => r.Status == ScreeningStatus.Pending && (r.IsClinical || r.IsStrRequired)),
                MissingScreeningEligibleCount = missingEligible
            };

            return RecruitmentResult<ScreeningSummaryResponse>.Ok(summary, "Ringkasan seleksi administrasi berhasil diambil.");
        }

        public async Task<RecruitmentResult<ScreeningReconcilePreviewResponse>> GetReconcilePreviewAsync(
            CancellationToken cancellationToken = default)
        {
            var (eligible, skipped) = await InspectMissingScreeningsAsync(cancellationToken);
            var preview = new ScreeningReconcilePreviewResponse
            {
                EligibleCount = eligible.Count,
                SkippedCount = skipped.Count,
                EligibleItems = eligible.Select(x => x.Dto).ToList(),
                SkippedItems = skipped
            };

            return RecruitmentResult<ScreeningReconcilePreviewResponse>.Ok(preview, "Pratinjau rekonsiliasi antrean screening berhasil dimuat.");
        }

        public async Task<RecruitmentResult<ScreeningReconcileResultResponse>> ReconcileMissingScreeningsAsync(
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId(user);
            if (userId == Guid.Empty)
            {
                return RecruitmentResult<ScreeningReconcileResultResponse>.Fail("Identitas user pada token tidak valid.", 401);
            }

            var (eligible, skipped) = await InspectMissingScreeningsAsync(cancellationToken);
            if (eligible.Count == 0)
            {
                return RecruitmentResult<ScreeningReconcileResultResponse>.Ok(
                    new ScreeningReconcileResultResponse
                    {
                        CreatedCount = 0,
                        SkippedCount = skipped.Count,
                        SkippedItems = skipped
                    },
                    "Tidak ada lamaran eligible baru yang memerlukan inisiasi screening.");
            }

            await using var transaction = _dbContext.Database.IsRelational() && _dbContext.Database.CurrentTransaction == null
                ? await _dbContext.Database.BeginTransactionAsync(cancellationToken)
                : null;

            var createdIds = new List<Guid>();
            foreach (var item in eligible)
            {
                var alreadyHas = await _dbContext.HrdCandidateScreenings
                    .AnyAsync(s => s.ApplicationId == item.Application.Id && !s.IsDelete, cancellationToken);
                if (alreadyHas)
                {
                    continue;
                }

                var screening = RecruitmentJourneyPolicy.CreateInitialPendingScreening(
                    item.Application.Id,
                    item.IsClinical,
                    userId,
                    "Inisiasi otomatis melalui rekonsiliasi antrean screening.");
                await _dbContext.HrdCandidateScreenings.AddAsync(screening, cancellationToken);
                createdIds.Add(screening.Id);
            }

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                if (transaction != null)
                {
                    await transaction.CommitAsync(cancellationToken);
                }
            }
            catch (DbUpdateException)
            {
                if (transaction != null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                }
                return RecruitmentResult<ScreeningReconcileResultResponse>.Conflict(
                    "Terjadi konflik konkurensi saat melakukan rekonsiliasi screening. Silakan coba lagi.");
            }

            var result = new ScreeningReconcileResultResponse
            {
                CreatedCount = createdIds.Count,
                SkippedCount = skipped.Count,
                CreatedScreeningIds = createdIds,
                SkippedItems = skipped
            };

            return RecruitmentResult<ScreeningReconcileResultResponse>.Ok(result, $"Rekonsiliasi berhasil membuat {createdIds.Count} baris screening Pending.");
        }

        private async Task<(List<(HrdCandidateApplication Application, bool IsClinical, ScreeningReconcileItemDto Dto)> Eligible, List<ScreeningReconcileItemDto> Skipped)> InspectMissingScreeningsAsync(
            CancellationToken cancellationToken)
        {
            var appsWithoutScreening = await _dbContext.HrdCandidateApplications
                .AsNoTracking()
                .Include(a => a.Candidate)
                .Include(a => a.Vacancy)
                .Where(a => !a.IsDelete && !_dbContext.HrdCandidateScreenings.Any(s => s.ApplicationId == a.Id && !s.IsDelete))
                .OrderByDescending(a => a.ApplicationDate)
                .ToListAsync(cancellationToken);

            var eligible = new List<(HrdCandidateApplication Application, bool IsClinical, ScreeningReconcileItemDto Dto)>();
            var skipped = new List<ScreeningReconcileItemDto>();

            foreach (var app in appsWithoutScreening)
            {
                var isClinical = app.Vacancy?.IsClinical ?? false;
                string? skipReason = null;

                if (app.Candidate == null || app.Candidate.IsDelete)
                {
                    skipReason = "Kandidat tidak ditemukan atau telah dihapus.";
                }
                else if (app.Candidate.Status == CandidateStatus.Blacklisted)
                {
                    skipReason = "Kandidat berstatus daftar hitam (Blacklisted).";
                }
                else if (app.Vacancy == null || app.Vacancy.IsDelete)
                {
                    skipReason = "Data lowongan kerja tidak ditemukan.";
                }
                else if (RecruitmentJourneyPolicy.IsFastTrackApplication(app))
                {
                    skipReason = "Lamaran merupakan jalur Fast-Track Talent Pool (melewati screening).";
                }
                else if (RecruitmentJourneyPolicy.IsTerminalApplication(app, app.Candidate))
                {
                    skipReason = "Lamaran sudah berstatus terminal/gugur.";
                }
                else if (app.CurrentStage != ApplicationStage.Screening || !string.Equals(app.StageStatus, "Submitted", StringComparison.OrdinalIgnoreCase))
                {
                    skipReason = $"Tahap lamaran sudah berada di '{app.CurrentStage}' ({app.StageStatus}), bukan antrean awal Screening/Submitted.";
                }

                var dto = new ScreeningReconcileItemDto
                {
                    ApplicationId = app.Id,
                    ApplicationNumber = app.ApplicationNumber,
                    CandidateId = app.CandidateId,
                    CandidateName = app.Candidate?.FullName ?? "-",
                    VacancyTitle = app.Vacancy?.Title ?? "-",
                    IsClinical = isClinical,
                    CurrentStage = app.CurrentStage.ToString(),
                    StageStatus = app.StageStatus,
                    Eligible = skipReason == null,
                    Reason = skipReason ?? "Memenuhi syarat untuk dibuatkan antrean screening Pending."
                };

                if (skipReason == null)
                {
                    eligible.Add((app, isClinical, dto));
                }
                else
                {
                    skipped.Add(dto);
                }
            }

            return (eligible, skipped);
        }

        public async Task<RecruitmentResult<CandidateScreeningResponse>> InitiateScreeningAsync(
            InitiateScreeningRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId(user);
            if (userId == Guid.Empty)
            {
                return RecruitmentResult<CandidateScreeningResponse>.Fail("Identitas user pada token tidak valid.", 401);
            }

            var application = await _dbContext.HrdCandidateApplications
                .Include(a => a.Candidate)
                .Include(a => a.Vacancy)
                .FirstOrDefaultAsync(a => a.Id == request.ApplicationId && !a.IsDelete, cancellationToken);

            if (application == null)
            {
                return RecruitmentResult<CandidateScreeningResponse>.NotFound("Lamaran tidak ditemukan.");
            }

            if (RecruitmentJourneyPolicy.IsTerminalApplication(application, application.Candidate))
            {
                return RecruitmentResult<CandidateScreeningResponse>.Unprocessable(
                    "Penapisan berkas tidak dapat diinisiasi karena lamaran atau kandidat berada pada status terminal/gugur.");
            }

            var existingScreening = await _dbContext.HrdCandidateScreenings
                .FirstOrDefaultAsync(s => s.ApplicationId == request.ApplicationId && !s.IsDelete, cancellationToken);

            if (existingScreening != null)
            {
                return RecruitmentResult<CandidateScreeningResponse>.Conflict("Penapisan berkas untuk lamaran ini sudah diinisiasi sebelumnya.");
            }

            var isClinical = application.Vacancy?.IsClinical ?? request.IsStrRequired;
            var screening = RecruitmentJourneyPolicy.CreateInitialPendingScreening(
                request.ApplicationId,
                isClinical,
                userId,
                request.Notes);

            await _dbContext.HrdCandidateScreenings.AddAsync(screening, cancellationToken);
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                return RecruitmentResult<CandidateScreeningResponse>.Conflict("Penapisan berkas untuk lamaran ini sudah diinisiasi oleh sesi lain.");
            }

            return await GetByIdAsync(screening.Id, cancellationToken);
        }

        public async Task<RecruitmentResult<CandidateScreeningResponse>> EvaluateScreeningAsync(
            EvaluateScreeningRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId(user);
            if (userId == Guid.Empty)
            {
                return RecruitmentResult<CandidateScreeningResponse>.Fail("Identitas user pada token tidak valid.", 401);
            }

            var screening = await _dbContext.HrdCandidateScreenings
                .Include(s => s.Application)
                    .ThenInclude(a => a!.Vacancy)
                .Include(s => s.Application)
                    .ThenInclude(a => a!.Candidate)
                .FirstOrDefaultAsync(s => s.ApplicationId == request.ApplicationId && !s.IsDelete, cancellationToken);

            if (screening == null)
            {
                return RecruitmentResult<CandidateScreeningResponse>.NotFound("Data penapisan berkas belum diinisiasi untuk lamaran ini.");
            }

            var app = screening.Application;
            if (app == null)
            {
                return RecruitmentResult<CandidateScreeningResponse>.NotFound("Lamaran terkait tidak ditemukan.");
            }

            if (screening.Status != ScreeningStatus.Pending)
            {
                return RecruitmentResult<CandidateScreeningResponse>.Conflict(
                    $"Penapisan berkas untuk lamaran ini sudah selesai diputuskan dengan status '{screening.Status}' dan tidak dapat dievaluasi ulang.");
            }

            if (RecruitmentJourneyPolicy.IsTerminalApplication(app, app.Candidate))
            {
                return RecruitmentResult<CandidateScreeningResponse>.Unprocessable(
                    "Evaluasi penapisan berkas ditolak karena lamaran atau kandidat telah berstatus terminal/gugur.");
            }

            var isClinical = app.Vacancy?.IsClinical ?? screening.IsStrRequired;
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            // Ambil dokumen pelamar untuk lamaran ini (prioritaskan dokumen yang terikat langsung pada ApplicationId)
            var documents = await _dbContext.HrdCandidateDocuments
                .Where(d => d.CandidateId == app.CandidateId
                            && (d.ApplicationId == null || d.ApplicationId == app.Id)
                            && !d.IsDelete)
                .OrderByDescending(d => d.ApplicationId == app.Id)
                .ThenByDescending(d => d.CreateDateTime)
                .ThenByDescending(d => d.Id)
                .ToListAsync(cancellationToken);

            var (_, isExpiredStr, strDoc) = RecruitmentJourneyPolicy.EvaluateClinicalStrDocument(documents, app.Id, today);
            if (strDoc != null)
            {
                screening.StrNumber = strDoc.DocumentNumber;
                screening.StrExpiryDate = strDoc.ValidUntil;
            }

            // Mode 1: Simpan pemeriksaan / minta kelengkapan tanpa menutup keputusan final
            if (request.Decision == ScreeningStatus.Pending)
            {
                screening.Notes = string.IsNullOrWhiteSpace(request.Notes)
                    ? "Pemeriksaan berkas administrasi membutuhkan kelengkapan/tindak lanjut."
                    : request.Notes.Trim();
                screening.EvaluatedByUserId = userId;
                screening.UpdateDateTime = DateTime.UtcNow;
                screening.UpdateBy = userId;

                app.StageStatus = "ScreeningNeedsCompletion";
                app.UpdateDateTime = DateTime.UtcNow;
                app.UpdateBy = userId;

                await _dbContext.SaveChangesAsync(cancellationToken);
                return await GetByIdAsync(screening.Id, cancellationToken);
            }

            // Mode 2: Penolakan manual eksplisit atau hasil klinis manual negatif (tidak bisa bypass)
            var manualFailure = isClinical && (request.ManualClinicalResult == ClinicalScreeningResult.StrMissing ||
                                               request.ManualClinicalResult == ClinicalScreeningResult.StrExpiredAutoReject);
            if (request.Decision == ScreeningStatus.Rejected || manualFailure)
            {
                screening.Status = ScreeningStatus.Rejected;
                if (isClinical)
                {
                    if (request.ManualClinicalResult == ClinicalScreeningResult.StrMissing || strDoc == null)
                    {
                        screening.ClinicalScreeningResult = ClinicalScreeningResult.StrMissing;
                    }
                    else if (request.ManualClinicalResult == ClinicalScreeningResult.StrExpiredAutoReject || isExpiredStr)
                    {
                        screening.ClinicalScreeningResult = ClinicalScreeningResult.StrExpiredAutoReject;
                    }
                    else
                    {
                        screening.ClinicalScreeningResult = ClinicalScreeningResult.NotApplicable;
                    }
                }
                else
                {
                    screening.ClinicalScreeningResult = ClinicalScreeningResult.NotApplicable;
                }

                screening.Notes = string.IsNullOrWhiteSpace(request.Notes)
                    ? "Gugur pada pemeriksaan seleksi administrasi."
                    : request.Notes.Trim();
                screening.EvaluatedByUserId = userId;
                screening.UpdateDateTime = DateTime.UtcNow;
                screening.UpdateBy = userId;

                app.StageStatus = "REJECTED_ADMINISTRATION";
                app.Notes = $"Gugur penapisan administrasi: {screening.Notes}";
                app.UpdateDateTime = DateTime.UtcNow;
                app.UpdateBy = userId;

                await _dbContext.SaveChangesAsync(cancellationToken);
                return await GetByIdAsync(screening.Id, cancellationToken);
            }

            // Mode 3: Evaluasi Kelulusan (Decision == Passed atau Decision == null legacy auto-evaluate)
            // Hard Gate STR untuk lowongan klinis (VR-REC-07, VR-REC-08, HRD-DEC-076)
            if (isClinical)
            {
                if (strDoc == null)
                {
                    if (request.Decision == ScreeningStatus.Passed)
                    {
                        return RecruitmentResult<CandidateScreeningResponse>.Unprocessable(
                            "Hard gate STR klinis: pelamar posisi klinis wajib mengunggah STR aktif sebelum dinyatakan Lolos Administrasi (VR-REC-07).");
                    }

                    screening.Status = ScreeningStatus.Rejected;
                    screening.ClinicalScreeningResult = ClinicalScreeningResult.StrMissing;
                    screening.Notes = "DOKUMEN_STR_WAJIB_TIDAK_DITEMUKAN: Pelamar posisi klinis wajib mengunggah Surat Tanda Registrasi (STR) yang valid (VR-REC-07).";
                    screening.EvaluatedByUserId = userId;
                    screening.UpdateDateTime = DateTime.UtcNow;
                    screening.UpdateBy = userId;

                    app.StageStatus = "REJECTED_ADMINISTRATION";
                    app.Notes = "Gugur penapisan administrasi: Berkas STR tidak ditemukan.";
                    app.UpdateDateTime = DateTime.UtcNow;
                    app.UpdateBy = userId;

                    await _dbContext.SaveChangesAsync(cancellationToken);
                    return await GetByIdAsync(screening.Id, cancellationToken);
                }

                if (isExpiredStr)
                {
                    if (request.Decision == ScreeningStatus.Passed)
                    {
                        return RecruitmentResult<CandidateScreeningResponse>.Unprocessable(
                            $"Hard gate STR klinis: Masa berlaku STR telah kedaluwarsa pada {strDoc.ValidUntil!.Value:yyyy-MM-dd}. Tidak dapat diluluskan (VR-REC-08).");
                    }

                    screening.Status = ScreeningStatus.Rejected;
                    screening.ClinicalScreeningResult = ClinicalScreeningResult.StrExpiredAutoReject;
                    screening.Notes = $"EXPIRED_STR_AUTO_REJECT: Masa berlaku STR telah kedaluwarsa pada {strDoc.ValidUntil!.Value:yyyy-MM-dd}. Pelamar otomatis gugur penapisan (VR-REC-08).";
                    screening.EvaluatedByUserId = userId;
                    screening.UpdateDateTime = DateTime.UtcNow;
                    screening.UpdateBy = userId;

                    app.StageStatus = "REJECTED_ADMINISTRATION";
                    app.Notes = "Gugur penapisan administrasi: STR kedaluwarsa (Auto-Reject).";
                    app.UpdateDateTime = DateTime.UtcNow;
                    app.UpdateBy = userId;

                    await _dbContext.SaveChangesAsync(cancellationToken);
                    return await GetByIdAsync(screening.Id, cancellationToken);
                }
            }

            // Cek kelengkapan dokumen wajib umum (KTP, CV, Ijazah) untuk seluruh lowongan (REC-05, UAT-09, UAT-11)
            var missingMandatoryDocs = RecruitmentJourneyPolicy.GetMissingMandatoryGeneralDocuments(documents);
            if (missingMandatoryDocs.Count > 0)
            {
                return RecruitmentResult<CandidateScreeningResponse>.Unprocessable(
                    $"Tidak dapat meluluskan seleksi administrasi sebelum dokumen wajib lengkap: {string.Join(", ", missingMandatoryDocs)}.");
            }

            // Seluruh prasyarat lulus terpenuhi
            screening.Status = ScreeningStatus.Passed;
            screening.ClinicalScreeningResult = isClinical ? ClinicalScreeningResult.StrValid : ClinicalScreeningResult.NotApplicable;
            screening.Notes = string.IsNullOrWhiteSpace(request.Notes)
                ? (isClinical
                    ? "Penapisan berkas administrasi wajib dan keabsahan STR klinis lulus."
                    : "Penapisan berkas administrasi wajib non-klinis lulus.")
                : request.Notes.Trim();
            screening.EvaluatedByUserId = userId;
            screening.UpdateDateTime = DateTime.UtcNow;
            screening.UpdateBy = userId;

            var verifiedByName = user.Identity?.Name ?? "HR Admin";
            var verifiedAt = DateTime.UtcNow;
            var docsToVerify = documents
                .Where(d => d.DocumentType == DocumentType.IdentityCard
                            || d.DocumentType == DocumentType.CurriculumVitae
                            || d.DocumentType == DocumentType.DiplomaCertificate
                            || (isClinical && strDoc != null && d.Id == strDoc.Id));
            foreach (var doc in docsToVerify)
            {
                doc.IsVerified = true;
                doc.VerifiedAt = verifiedAt;
                doc.VerifiedBy = verifiedByName;
            }

            app.CurrentStage = ApplicationStage.InterviewHr;
            app.StageStatus = "ScreeningPassed";
            app.UpdateDateTime = DateTime.UtcNow;
            app.UpdateBy = userId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetByIdAsync(screening.Id, cancellationToken);
        }

        public async Task<RecruitmentResult<List<CandidateDocumentResponse>>> GetCandidateDocumentsAsync(
            Guid candidateId,
            CancellationToken cancellationToken = default)
        {
            var docs = await _dbContext.HrdCandidateDocuments
                .AsNoTracking()
                .Where(d => d.CandidateId == candidateId && !d.IsDelete)
                .OrderByDescending(d => d.CreateDateTime)
                .Select(d => new CandidateDocumentResponse
                {
                    Id = d.Id,
                    CandidateId = d.CandidateId,
                    ApplicationId = d.ApplicationId,
                    DocumentType = d.DocumentType,
                    DocumentNumber = d.DocumentNumber,
                    DocumentTitle = d.DocumentTitle,
                    FilePath = d.FilePath,
                    FileSize = d.FileSize,
                    MimeType = d.MimeType,
                    ValidFrom = d.ValidFrom,
                    ValidUntil = d.ValidUntil,
                    IsVerified = d.IsVerified,
                    VerifiedBy = d.VerifiedBy,
                    VerifiedAt = d.VerifiedAt
                })
                .ToListAsync(cancellationToken);

            return RecruitmentResult<List<CandidateDocumentResponse>>.Ok(docs, "Daftar berkas dokumen pelamar berhasil diambil.");
        }

        private async Task<CandidateScreeningResponse> BuildDetailedResponseAsync(
            HrdCandidateScreening s,
            CancellationToken cancellationToken)
        {
            var response = MapToResponse(s);
            if (s.Application != null)
            {
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                var docs = await _dbContext.HrdCandidateDocuments
                    .AsNoTracking()
                    .Where(d => d.CandidateId == s.Application.CandidateId
                                && (d.ApplicationId == null || d.ApplicationId == s.ApplicationId)
                                && !d.IsDelete)
                    .OrderByDescending(d => d.ApplicationId == s.ApplicationId)
                    .ThenByDescending(d => d.CreateDateTime)
                    .ThenByDescending(d => d.Id)
                    .Select(d => new CandidateDocumentResponse
                    {
                        Id = d.Id,
                        CandidateId = d.CandidateId,
                        ApplicationId = d.ApplicationId,
                        DocumentType = d.DocumentType,
                        DocumentNumber = d.DocumentNumber,
                        DocumentTitle = d.DocumentTitle,
                        FilePath = d.FilePath,
                        FileSize = d.FileSize,
                        MimeType = d.MimeType,
                        ValidFrom = d.ValidFrom,
                        ValidUntil = d.ValidUntil,
                        IsVerified = d.IsVerified,
                        VerifiedBy = d.VerifiedBy,
                        VerifiedAt = d.VerifiedAt
                    })
                    .ToListAsync(cancellationToken);

                var strDocs = docs.Where(d => d.DocumentType == DocumentType.StrCertificate).ToList();
                response.Documents = docs;
                response.DocumentsCount = docs.Count;
                response.HasStrDocument = strDocs.Count > 0;
                response.HasExpiredStr = strDocs.Any(d => d.ValidUntil.HasValue && d.ValidUntil.Value < today);
            }

            return response;
        }

        private static CandidateScreeningResponse MapToResponse(HrdCandidateScreening s)
        {
            var isClinicalVacancy = s.Application?.Vacancy?.IsClinical ?? s.IsStrRequired;
            return new CandidateScreeningResponse
            {
                Id = s.Id,
                ApplicationId = s.ApplicationId,
                ApplicationNumber = s.Application?.ApplicationNumber ?? string.Empty,
                ApplicationStageStatus = s.Application?.StageStatus,
                CandidateId = s.Application?.CandidateId ?? Guid.Empty,
                CandidateCode = s.Application?.Candidate?.CandidateCode ?? string.Empty,
                CandidateName = s.Application?.Candidate?.FullName ?? string.Empty,
                VacancyId = s.Application?.VacancyId ?? Guid.Empty,
                VacancyTitle = s.Application?.Vacancy?.Title ?? string.Empty,
                IsClinicalVacancy = isClinicalVacancy,
                ScreeningDate = s.ScreeningDate,
                Status = s.Status,
                IsStrRequired = s.IsStrRequired,
                ClinicalScreeningResult = s.ClinicalScreeningResult,
                ClinicalScreeningResultLabel = RecruitmentJourneyPolicy.ResolveClinicalScreeningResultLabel(
                    s.Status,
                    isClinicalVacancy || s.IsStrRequired,
                    s.ClinicalScreeningResult),
                StrNumber = s.StrNumber,
                StrExpiryDate = s.StrExpiryDate,
                Notes = s.Notes,
                EvaluatedByUserId = s.EvaluatedByUserId,
                CreateDateTime = s.CreateDateTime
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
