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
                    CandidateName = s.Application != null && s.Application.Candidate != null ? s.Application.Candidate.FullName : string.Empty,
                    VacancyTitle = s.Application != null && s.Application.Vacancy != null ? s.Application.Vacancy.Title : string.Empty,
                    IsClinicalVacancy = s.Application != null && s.Application.Vacancy != null && s.Application.Vacancy.IsClinical,
                    Status = s.Status,
                    ClinicalScreeningResult = s.ClinicalScreeningResult,
                    StrExpiryDate = s.StrExpiryDate,
                    ScreeningDate = s.ScreeningDate
                })
                .ToListAsync(cancellationToken);

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

            var response = MapToResponse(s);
            return RecruitmentResult<CandidateScreeningResponse>.Ok(response, "Detail penapisan berkas berhasil diambil.");
        }

        public async Task<RecruitmentResult<CandidateScreeningResponse>> InitiateScreeningAsync(
            InitiateScreeningRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId(user);

            var application = await _dbContext.HrdCandidateApplications
                .Include(a => a.Vacancy)
                .FirstOrDefaultAsync(a => a.Id == request.ApplicationId && !a.IsDelete, cancellationToken);

            if (application == null)
            {
                return RecruitmentResult<CandidateScreeningResponse>.NotFound("Lamaran tidak ditemukan.");
            }

            var existingScreening = await _dbContext.HrdCandidateScreenings
                .FirstOrDefaultAsync(s => s.ApplicationId == request.ApplicationId && !s.IsDelete, cancellationToken);

            if (existingScreening != null)
            {
                return RecruitmentResult<CandidateScreeningResponse>.Conflict("Penapisan berkas untuk lamaran ini sudah diinisiasi sebelumnya.");
            }

            var isClinical = application.Vacancy?.IsClinical ?? request.IsStrRequired;

            var screening = new HrdCandidateScreening
            {
                Id = Guid.NewGuid(),
                ApplicationId = request.ApplicationId,
                ScreeningDate = DateTime.UtcNow,
                Status = ScreeningStatus.Pending,
                IsStrRequired = isClinical,
                ClinicalScreeningResult = isClinical ? ClinicalScreeningResult.NotApplicable : ClinicalScreeningResult.NotApplicable,
                Notes = request.Notes?.Trim(),
                EvaluatedByUserId = userId,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = userId
            };

            await _dbContext.HrdCandidateScreenings.AddAsync(screening, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return await GetByIdAsync(screening.Id, cancellationToken);
        }

        public async Task<RecruitmentResult<CandidateScreeningResponse>> EvaluateScreeningAsync(
            EvaluateScreeningRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId(user);

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

            var isClinical = app.Vacancy?.IsClinical ?? screening.IsStrRequired;
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            // Ambil seluruh dokumen pelamar untuk evaluasi
            var documents = await _dbContext.HrdCandidateDocuments
                .Where(d => d.CandidateId == app.CandidateId && !d.IsDelete)
                .ToListAsync(cancellationToken);

            var strDoc = documents.FirstOrDefault(d => d.DocumentType == DocumentType.StrCertificate);

            // Aturan Bisnis VR-REC-07: Lowongan klinis wajib memiliki dokumen STR
            if (isClinical)
            {
                if (strDoc == null)
                {
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

                screening.StrNumber = strDoc.DocumentNumber;
                screening.StrExpiryDate = strDoc.ValidUntil;

                // Aturan Bisnis VR-REC-08, AT-HRD-D2-05: STR kedaluwarsa -> EXPIRED_STR_AUTO_REJECT
                if (strDoc.ValidUntil.HasValue && strDoc.ValidUntil.Value < today)
                {
                    screening.Status = ScreeningStatus.Rejected;
                    screening.ClinicalScreeningResult = ClinicalScreeningResult.StrExpiredAutoReject;
                    screening.Notes = $"EXPIRED_STR_AUTO_REJECT: Masa berlaku STR telah kedaluwarsa pada {strDoc.ValidUntil.Value:yyyy-MM-dd}. Pelamar otomatis gugur penapisan (VR-REC-08).";
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

                // STR aktif dan valid
                screening.Status = ScreeningStatus.Passed;
                screening.ClinicalScreeningResult = ClinicalScreeningResult.StrValid;
                screening.Notes = string.IsNullOrWhiteSpace(request.Notes)
                    ? "Penapisan berkas administrasi dan keabsahan STR klinis lulus."
                    : request.Notes.Trim();
                screening.EvaluatedByUserId = userId;
                screening.UpdateDateTime = DateTime.UtcNow;
                screening.UpdateBy = userId;

                strDoc.IsVerified = true;
                strDoc.VerifiedAt = DateTime.UtcNow;
                strDoc.VerifiedBy = user.Identity?.Name ?? "HR Admin";

                app.CurrentStage = ApplicationStage.InterviewHr;
                app.StageStatus = "ScreeningPassed";
                app.UpdateDateTime = DateTime.UtcNow;
                app.UpdateBy = userId;

                await _dbContext.SaveChangesAsync(cancellationToken);
                return await GetByIdAsync(screening.Id, cancellationToken);
            }
            else
            {
                // Aturan Bisnis VR-REC-09: Posisi non-klinis melangkah tanpa kewajiban STR
                screening.Status = ScreeningStatus.Passed;
                screening.ClinicalScreeningResult = ClinicalScreeningResult.NotApplicable;
                screening.Notes = string.IsNullOrWhiteSpace(request.Notes)
                    ? "Penapisan berkas administrasi non-klinis lulus."
                    : request.Notes.Trim();
                screening.EvaluatedByUserId = userId;
                screening.UpdateDateTime = DateTime.UtcNow;
                screening.UpdateBy = userId;

                app.CurrentStage = ApplicationStage.InterviewHr;
                app.StageStatus = "ScreeningPassed";
                app.UpdateDateTime = DateTime.UtcNow;
                app.UpdateBy = userId;

                await _dbContext.SaveChangesAsync(cancellationToken);
                return await GetByIdAsync(screening.Id, cancellationToken);
            }
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

        private static CandidateScreeningResponse MapToResponse(HrdCandidateScreening s)
        {
            return new CandidateScreeningResponse
            {
                Id = s.Id,
                ApplicationId = s.ApplicationId,
                ApplicationNumber = s.Application?.ApplicationNumber ?? string.Empty,
                CandidateId = s.Application?.CandidateId ?? Guid.Empty,
                CandidateCode = s.Application?.Candidate?.CandidateCode ?? string.Empty,
                CandidateName = s.Application?.Candidate?.FullName ?? string.Empty,
                VacancyId = s.Application?.VacancyId ?? Guid.Empty,
                VacancyTitle = s.Application?.Vacancy?.Title ?? string.Empty,
                IsClinicalVacancy = s.Application?.Vacancy?.IsClinical ?? s.IsStrRequired,
                ScreeningDate = s.ScreeningDate,
                Status = s.Status,
                IsStrRequired = s.IsStrRequired,
                ClinicalScreeningResult = s.ClinicalScreeningResult,
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
