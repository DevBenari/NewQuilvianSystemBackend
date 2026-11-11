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
    public class CandidateService : ICandidateService
    {
        private readonly ApplicationDbContext _dbContext;

        public CandidateService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<RecruitmentResult<PagedResult<CandidateListItemResponse>>> GetPagedAsync(
            CandidateQuery query,
            CancellationToken cancellationToken = default)
        {
            var dbQuery = _dbContext.HrdCandidates
                .AsNoTracking()
                .Where(c => !c.IsDelete);

            if (query.IsRehire.HasValue)
            {
                dbQuery = dbQuery.Where(c => c.IsRehire == query.IsRehire.Value);
            }

            if (query.Status.HasValue)
            {
                dbQuery = dbQuery.Where(c => c.Status == query.Status.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.SearchNik))
            {
                var nik = query.SearchNik.Trim();
                dbQuery = dbQuery.Where(c => c.NationalIdNumber.Contains(nik));
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var term = query.Search.Trim().ToLower();
                dbQuery = dbQuery.Where(c =>
                    c.CandidateCode.ToLower().Contains(term) ||
                    c.FullName.ToLower().Contains(term) ||
                    c.Email.ToLower().Contains(term) ||
                    c.PhoneNumber.Contains(term));
            }

            var totalData = await dbQuery.CountAsync(cancellationToken);

            var pageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
            var pageSize = query.PageSize > 0 ? query.PageSize : 10;

            var items = await dbQuery
                .OrderByDescending(c => c.CreateDateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CandidateListItemResponse
                {
                    Id = c.Id,
                    CandidateCode = c.CandidateCode,
                    NationalIdNumber = c.NationalIdNumber,
                    FullName = c.FullName,
                    Email = c.Email,
                    PhoneNumber = c.PhoneNumber,
                    IsRehire = c.IsRehire,
                    Status = c.Status,
                    CreateDateTime = c.CreateDateTime
                })
                .ToListAsync(cancellationToken);

            var result = new PagedResult<CandidateListItemResponse>
            {
                Items = items,
                TotalData = totalData,
                TotalPage = pageSize > 0 ? (int)Math.Ceiling((double)totalData / pageSize) : 0,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return RecruitmentResult<PagedResult<CandidateListItemResponse>>.Ok(result, "Daftar pelamar berhasil diambil.");
        }

        public async Task<RecruitmentResult<CandidateResponse>> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var c = await _dbContext.HrdCandidates
                .AsNoTracking()
                .Include(x => x.Applications)
                    .ThenInclude(a => a.Vacancy)
                .Include(x => x.Documents)
                .Include(x => x.PreviousWorkforceProfile)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (c == null)
            {
                return RecruitmentResult<CandidateResponse>.NotFound("Kandidat tidak ditemukan.");
            }

            string? previousEmployeeNumber = null;
            if (c.PreviousWorkforceProfileId.HasValue)
            {
                var emp = await _dbContext.MstEmployees
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.WorkforceProfileId == c.PreviousWorkforceProfileId.Value && !e.IsDelete, cancellationToken);
                previousEmployeeNumber = emp?.EmployeeNumber;
            }

            var response = MapToResponse(c, previousEmployeeNumber);
            return RecruitmentResult<CandidateResponse>.Ok(response, "Detail kandidat berhasil diambil.");
        }

        public async Task<RecruitmentResult<CandidateResponse>> RegisterAsync(
            RegisterCandidateRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId(user);

            var cleanNik = request.NationalIdNumber.Trim();
            if (cleanNik.Length != 16 || !cleanNik.All(char.IsDigit))
            {
                return RecruitmentResult<CandidateResponse>.Fail("NIK KTP harus terdiri dari tepat 16 digit angka.", 400);
            }

            // Anti-duplikasi kandidat berdasarkan NIK
            var exists = await _dbContext.HrdCandidates
                .AnyAsync(c => c.NationalIdNumber == cleanNik && !c.IsDelete, cancellationToken);

            if (exists)
            {
                return RecruitmentResult<CandidateResponse>.Conflict($"Pelamar dengan NIK '{cleanNik}' sudah terdaftar dalam sistem.");
            }

            // Aturan Bisnis VR-REC-06: Deteksi rehire berdasarkan NIK di master pegawai (MstEmployee)
            var matchedEmployee = await _dbContext.MstEmployees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.IdentityNumber == cleanNik && !e.IsDelete, cancellationToken);

            var isRehire = matchedEmployee != null;
            Guid? previousProfileId = matchedEmployee?.WorkforceProfileId;

            // Generate nomor urut CandidateCode unik
            var todayPrefix = $"CAN-{DateTime.UtcNow:yyyyMMdd}";
            var countToday = await _dbContext.HrdCandidates
                .CountAsync(c => c.CandidateCode.StartsWith(todayPrefix), cancellationToken);
            var candidateCode = $"{todayPrefix}-{(countToday + 1):D4}";

            var candidate = new HrdCandidate
            {
                Id = Guid.NewGuid(),
                CandidateCode = candidateCode,
                NationalIdNumber = cleanNik,
                FullName = request.FullName.Trim(),
                Gender = request.Gender?.Trim() ?? string.Empty,
                BirthDate = request.BirthDate,
                BirthPlace = request.BirthPlace?.Trim() ?? string.Empty,
                Email = request.Email.Trim().ToLower(),
                PhoneNumber = request.PhoneNumber.Trim(),
                Address = request.Address?.Trim() ?? string.Empty,
                IsRehire = isRehire,
                PreviousWorkforceProfileId = previousProfileId,
                Status = CandidateStatus.Registered,
                RowVersion = Guid.NewGuid(),
                CreateDateTime = DateTime.UtcNow,
                CreateBy = userId
            };

            await _dbContext.HrdCandidates.AddAsync(candidate, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return await GetByIdAsync(candidate.Id, cancellationToken);
        }

        public async Task<RecruitmentResult<CandidateResponse>> UpdateAsync(
            Guid id,
            UpdateCandidateRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId(user);

            var entity = await _dbContext.HrdCandidates
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDelete, cancellationToken);

            if (entity == null)
            {
                return RecruitmentResult<CandidateResponse>.NotFound("Kandidat tidak ditemukan.");
            }

            if (entity.RowVersion != request.RowVersion)
            {
                return RecruitmentResult<CandidateResponse>.Conflict("Data kandidat telah diubah oleh pengguna lain. Silakan muat ulang data terbaru.");
            }

            entity.FullName = request.FullName.Trim();
            entity.Gender = request.Gender?.Trim() ?? string.Empty;
            entity.BirthDate = request.BirthDate;
            entity.BirthPlace = request.BirthPlace?.Trim() ?? string.Empty;
            entity.Email = request.Email.Trim().ToLower();
            entity.PhoneNumber = request.PhoneNumber.Trim();
            entity.Address = request.Address?.Trim() ?? string.Empty;
            entity.RowVersion = Guid.NewGuid();
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = userId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return await GetByIdAsync(entity.Id, cancellationToken);
        }

        public async Task<RecruitmentResult<CandidateApplicationResponse>> ApplyVacancyAsync(
            Guid candidateId,
            ApplyVacancyRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId(user);

            var candidate = await _dbContext.HrdCandidates
                .FirstOrDefaultAsync(c => c.Id == candidateId && !c.IsDelete, cancellationToken);

            if (candidate == null)
            {
                return RecruitmentResult<CandidateApplicationResponse>.NotFound("Kandidat tidak ditemukan.");
            }

            // Validasi keberadaan dan status lowongan kerja
            var vacancy = await _dbContext.HrdJobVacancies
                .FirstOrDefaultAsync(v => v.Id == request.VacancyId && !v.IsDelete, cancellationToken);

            if (vacancy == null)
            {
                return RecruitmentResult<CandidateApplicationResponse>.NotFound("Lowongan kerja tidak ditemukan.");
            }

            // Aturan Bisnis VR-REC-07: Lowongan harus berstatus Published
            if (vacancy.Status != JobVacancyStatus.Published)
            {
                return RecruitmentResult<CandidateApplicationResponse>.Unprocessable("Lamaran hanya dapat diajukan pada lowongan kerja yang berstatus Published (Aktif).");
            }

            if (vacancy.ClosingDate.HasValue && vacancy.ClosingDate.Value < DateTime.UtcNow)
            {
                return RecruitmentResult<CandidateApplicationResponse>.Unprocessable("Lowongan kerja ini sudah melewati batas waktu penutupan pendaftaran.");
            }

            // Validasi anti-duplikasi lamaran untuk lowongan yang sama
            var alreadyApplied = await _dbContext.HrdCandidateApplications
                .AnyAsync(a => a.CandidateId == candidateId && a.VacancyId == request.VacancyId && !a.IsDelete, cancellationToken);

            if (alreadyApplied)
            {
                return RecruitmentResult<CandidateApplicationResponse>.Conflict("Kandidat sudah pernah mengajukan lamaran untuk lowongan kerja ini.");
            }

            // Auto-numbering ApplicationNumber
            var todayPrefix = $"APP-{DateTime.UtcNow:yyyyMMdd}";
            var countToday = await _dbContext.HrdCandidateApplications
                .CountAsync(a => a.ApplicationNumber.StartsWith(todayPrefix), cancellationToken);
            var applicationNumber = $"{todayPrefix}-{(countToday + 1):D4}";

            var application = new HrdCandidateApplication
            {
                Id = Guid.NewGuid(),
                ApplicationNumber = applicationNumber,
                CandidateId = candidateId,
                VacancyId = request.VacancyId,
                ApplicationDate = DateTime.UtcNow,
                CurrentStage = ApplicationStage.Screening,
                StageStatus = "Submitted",
                Notes = request.Notes?.Trim(),
                RowVersion = Guid.NewGuid(),
                CreateDateTime = DateTime.UtcNow,
                CreateBy = userId
            };

            // Update status kandidat jika masih Registered -> Active
            if (candidate.Status == CandidateStatus.Registered)
            {
                candidate.Status = CandidateStatus.Active;
                candidate.RowVersion = Guid.NewGuid();
                candidate.UpdateDateTime = DateTime.UtcNow;
                candidate.UpdateBy = userId;
            }

            await _dbContext.HrdCandidateApplications.AddAsync(application, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var response = new CandidateApplicationResponse
            {
                Id = application.Id,
                ApplicationNumber = application.ApplicationNumber,
                VacancyId = application.VacancyId,
                VacancyTitle = vacancy.Title,
                ApplicationDate = application.ApplicationDate,
                CurrentStage = application.CurrentStage,
                StageStatus = application.StageStatus,
                Notes = application.Notes
            };

            return RecruitmentResult<CandidateApplicationResponse>.Ok(response, "Lamaran pekerjaan berhasil diajukan.");
        }

        public async Task<RecruitmentResult<CandidateDocumentResponse>> UploadDocumentAsync(
            Guid candidateId,
            UploadCandidateDocumentRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId(user);

            var candidateExists = await _dbContext.HrdCandidates
                .AnyAsync(c => c.Id == candidateId && !c.IsDelete, cancellationToken);

            if (!candidateExists)
            {
                return RecruitmentResult<CandidateDocumentResponse>.NotFound("Kandidat tidak ditemukan.");
            }

            if (request.ApplicationId.HasValue)
            {
                var appExists = await _dbContext.HrdCandidateApplications
                    .AnyAsync(a => a.Id == request.ApplicationId.Value && a.CandidateId == candidateId && !a.IsDelete, cancellationToken);
                if (!appExists)
                {
                    return RecruitmentResult<CandidateDocumentResponse>.NotFound("Lamaran yang diasosiasikan tidak ditemukan untuk kandidat ini.");
                }
            }

            var doc = new HrdCandidateDocument
            {
                Id = Guid.NewGuid(),
                CandidateId = candidateId,
                ApplicationId = request.ApplicationId,
                DocumentType = request.DocumentType,
                DocumentNumber = request.DocumentNumber?.Trim(),
                DocumentTitle = request.DocumentTitle.Trim(),
                FilePath = request.FilePath.Trim(),
                FileSize = request.FileSize,
                MimeType = string.IsNullOrWhiteSpace(request.MimeType) ? "application/pdf" : request.MimeType.Trim(),
                ValidFrom = request.ValidFrom,
                ValidUntil = request.ValidUntil,
                IsVerified = false,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = userId
            };

            await _dbContext.HrdCandidateDocuments.AddAsync(doc, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var response = new CandidateDocumentResponse
            {
                Id = doc.Id,
                CandidateId = doc.CandidateId,
                ApplicationId = doc.ApplicationId,
                DocumentType = doc.DocumentType,
                DocumentNumber = doc.DocumentNumber,
                DocumentTitle = doc.DocumentTitle,
                FilePath = doc.FilePath,
                FileSize = doc.FileSize,
                MimeType = doc.MimeType,
                ValidFrom = doc.ValidFrom,
                ValidUntil = doc.ValidUntil,
                IsVerified = doc.IsVerified,
                VerifiedBy = doc.VerifiedBy,
                VerifiedAt = doc.VerifiedAt
            };

            return RecruitmentResult<CandidateDocumentResponse>.Ok(response, "Dokumen pelamar berhasil disimpan.");
        }

        private static CandidateResponse MapToResponse(HrdCandidate c, string? previousEmployeeNumber)
        {
            return new CandidateResponse
            {
                Id = c.Id,
                CandidateCode = c.CandidateCode,
                NationalIdNumber = c.NationalIdNumber,
                FullName = c.FullName,
                Gender = c.Gender,
                BirthDate = c.BirthDate,
                BirthPlace = c.BirthPlace,
                Email = c.Email,
                PhoneNumber = c.PhoneNumber,
                Address = c.Address,
                IsRehire = c.IsRehire,
                PreviousWorkforceProfileId = c.PreviousWorkforceProfileId,
                PreviousEmployeeNumber = previousEmployeeNumber,
                Status = c.Status,
                RowVersion = c.RowVersion,
                CreateDateTime = c.CreateDateTime,
                UpdateDateTime = c.UpdateDateTime,
                Applications = c.Applications
                    .Where(a => !a.IsDelete)
                    .OrderByDescending(a => a.ApplicationDate)
                    .Select(a => new CandidateApplicationResponse
                    {
                        Id = a.Id,
                        ApplicationNumber = a.ApplicationNumber,
                        VacancyId = a.VacancyId,
                        VacancyTitle = a.Vacancy?.Title,
                        ApplicationDate = a.ApplicationDate,
                        CurrentStage = a.CurrentStage,
                        StageStatus = a.StageStatus,
                        Notes = a.Notes
                    }).ToList(),
                Documents = c.Documents
                    .Where(d => !d.IsDelete)
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
                    }).ToList()
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
