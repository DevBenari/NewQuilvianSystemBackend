using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services
{
    public class HrdClinicalPreceptorshipService : IHrdClinicalPreceptorshipService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IHrdLifecycleAttachmentService _attachmentService;

        public HrdClinicalPreceptorshipService(
            ApplicationDbContext dbContext,
            IHrdLifecycleAttachmentService attachmentService)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _attachmentService = attachmentService ?? throw new ArgumentNullException(nameof(attachmentService));
        }

        private static Guid GetUserId(ClaimsPrincipal user)
        {
            var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? user.FindFirst("sub")?.Value;
            return Guid.TryParse(sub, out var guid) ? guid : Guid.Empty;
        }

        private static bool IsHrOrAdmin(ClaimsPrincipal user)
        {
            return user.IsInRole("Admin") ||
                   user.IsInRole("Administrator") ||
                   user.IsInRole("SuperAdmin") ||
                   user.IsInRole("HR") ||
                   user.IsInRole("HRAdmin") ||
                   user.IsInRole("HRManager");
        }

        private static Guid? GetUserUnitId(ClaimsPrincipal user)
        {
            var unitClaim = user.FindFirst("OrganizationUnitId")?.Value
                         ?? user.FindFirst("UnitId")?.Value
                         ?? user.FindFirst("unit_id")?.Value;
            return Guid.TryParse(unitClaim, out var guid) ? guid : null;
        }

        public async Task<RecruitmentResult<PagedResult<ClinicalPreceptorshipListItemResponse>>> GetPagedAsync(
            ClinicalPreceptorshipQuery query,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var dbQuery = _dbContext.HrdClinicalPreceptorships
                .Include(p => p.OnboardingCase)
                .Include(p => p.WorkforceProfile)
                .Include(p => p.OrganizationUnit)
                .Include(p => p.Preceptors.Where(pr => !pr.IsDelete && !pr.AssignedUntil.HasValue))
                    .ThenInclude(pr => pr.PreceptorWorkforceProfile)
                .Where(p => !p.IsDelete)
                .AsQueryable();

            if (!IsHrOrAdmin(user))
            {
                var userUnitId = GetUserUnitId(user);
                if (userUnitId.HasValue)
                {
                    dbQuery = dbQuery.Where(p => p.OrganizationUnitId == userUnitId.Value);
                }
            }

            if (query.PreceptorshipStatus.HasValue)
            {
                dbQuery = dbQuery.Where(p => p.PreceptorshipStatus == query.PreceptorshipStatus.Value);
            }

            if (query.OrganizationUnitId.HasValue)
            {
                dbQuery = dbQuery.Where(p => p.OrganizationUnitId == query.OrganizationUnitId.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var term = query.Search.Trim().ToLower();
                dbQuery = dbQuery.Where(p =>
                    (p.OnboardingCase != null && p.OnboardingCase.CaseNumber.ToLower().Contains(term)) ||
                    (p.WorkforceProfile != null && (p.WorkforceProfile.ProfileCode.ToLower().Contains(term) || p.WorkforceProfile.DisplayName.ToLower().Contains(term))));
            }

            var totalCount = await dbQuery.CountAsync(cancellationToken);
            var pageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
            var pageSize = query.PageSize > 0 ? query.PageSize : 10;

            var items = await dbQuery
                .OrderByDescending(p => p.CreateDateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ClinicalPreceptorshipListItemResponse
                {
                    Id = p.Id,
                    OnboardingCaseId = p.OnboardingCaseId,
                    CaseNumber = p.OnboardingCase != null ? p.OnboardingCase.CaseNumber : "-",
                    WorkforceProfileId = p.WorkforceProfileId,
                    WorkforceProfileCode = p.WorkforceProfile != null ? p.WorkforceProfile.ProfileCode : "-",
                    WorkforceDisplayName = p.WorkforceProfile != null ? p.WorkforceProfile.DisplayName : "-",
                    OrganizationUnitId = p.OrganizationUnitId,
                    OrganizationUnitName = p.OrganizationUnit != null ? p.OrganizationUnit.UnitName : "-",
                    PreceptorshipStatus = p.PreceptorshipStatus,
                    CurrentPreceptorDisplayName = p.Preceptors.Where(pr => !pr.IsDelete && !pr.AssignedUntil.HasValue)
                        .Select(pr => pr.PreceptorWorkforceProfile != null ? pr.PreceptorWorkforceProfile.DisplayName : "-")
                        .FirstOrDefault() ?? "-",
                    AccompaniedSince = p.AccompaniedSince,
                    IndependentOn = p.IndependentOn
                })
                .ToListAsync(cancellationToken);

            var totalPages = pageSize > 0 ? (int)Math.Ceiling((double)totalCount / pageSize) : 0;

            var result = new PagedResult<ClinicalPreceptorshipListItemResponse>
            {
                Items = items,
                TotalData = totalCount,
                TotalPage = totalPages,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return RecruitmentResult<PagedResult<ClinicalPreceptorshipListItemResponse>>.Ok(result, "Daftar pendampingan klinis berhasil diambil.");
        }

        public async Task<RecruitmentResult<ClinicalPreceptorshipResponse>> GetByIdAsync(
            Guid id,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var p = await _dbContext.HrdClinicalPreceptorships
                .Include(x => x.OnboardingCase)
                .Include(x => x.WorkforceProfile)
                .Include(x => x.OrganizationUnit)
                .Include(x => x.DeclaredByUser)
                .Include(x => x.Preceptors.Where(pr => !pr.IsDelete))
                    .ThenInclude(pr => pr.PreceptorWorkforceProfile)
                .Include(x => x.Preceptors.Where(pr => !pr.IsDelete))
                    .ThenInclude(pr => pr.AssignedByUser)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (p == null)
            {
                return RecruitmentResult<ClinicalPreceptorshipResponse>.NotFound("Data pendampingan klinis tidak ditemukan.");
            }

            if (!IsHrOrAdmin(user))
            {
                var userUnitId = GetUserUnitId(user);
                if (userUnitId.HasValue && p.OrganizationUnitId != userUnitId.Value)
                {
                    return RecruitmentResult<ClinicalPreceptorshipResponse>.Forbidden(
                        "Anda tidak memiliki wewenang untuk melihat pendampingan klinis di luar unit kerja Anda.");
                }
            }

            return RecruitmentResult<ClinicalPreceptorshipResponse>.Ok(MapToResponse(p), "Detail pendampingan klinis berhasil diambil.");
        }

        public async Task<RecruitmentResult<ClinicalPreceptorshipResponse>> CreateForCaseAsync(
            Guid onboardingCaseId,
            Guid preceptorWorkforceProfileId,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var onboardingCase = await _dbContext.HrdOnboardingCases
                .FirstOrDefaultAsync(c => c.Id == onboardingCaseId && !c.IsDelete, cancellationToken);

            if (onboardingCase == null)
            {
                return RecruitmentResult<ClinicalPreceptorshipResponse>.NotFound("Kasus onboarding tidak ditemukan.");
            }

            // Validasi VR-ONB-08: Preceptor bukan pegawai itu sendiri
            if (preceptorWorkforceProfileId == onboardingCase.WorkforceProfileId)
            {
                return RecruitmentResult<ClinicalPreceptorshipResponse>.Unprocessable(
                    "Preceptor tidak boleh diri pegawai itu sendiri (VR-ONB-08).");
            }

            // Validasi VR-ONB-08: Preceptor harus tenaga klinis aktif di unit yang sama
            var preceptorProfile = await _dbContext.MstWorkforceProfiles
                .FirstOrDefaultAsync(w => w.Id == preceptorWorkforceProfileId && !w.IsDelete, cancellationToken);

            if (preceptorProfile == null)
            {
                return RecruitmentResult<ClinicalPreceptorshipResponse>.NotFound("Profil tenaga kesehatan preceptor tidak ditemukan.");
            }

            // Cek apakah pendampingan klinis sudah pernah dibuat untuk kasus ini
            var existing = await _dbContext.HrdClinicalPreceptorships
                .Include(p => p.Preceptors)
                .FirstOrDefaultAsync(p => p.OnboardingCaseId == onboardingCaseId && !p.IsDelete, cancellationToken);

            if (existing != null)
            {
                // Jika sudah ada, tambahkan atau ubah preceptor bila berbeda
                var currentPreceptor = existing.Preceptors.FirstOrDefault(pr => !pr.IsDelete && !pr.AssignedUntil.HasValue);
                if (currentPreceptor == null || currentPreceptor.PreceptorWorkforceProfileId != preceptorWorkforceProfileId)
                {
                    if (currentPreceptor != null)
                    {
                        currentPreceptor.AssignedUntil = DateOnly.FromDateTime(DateTime.UtcNow);
                        currentPreceptor.UpdateDateTime = DateTime.UtcNow;
                        currentPreceptor.UpdateBy = actorUserId;
                    }

                    var newPr = new HrdClinicalPreceptorshipPreceptor
                    {
                        Id = Guid.NewGuid(),
                        PreceptorshipId = existing.Id,
                        PreceptorWorkforceProfileId = preceptorWorkforceProfileId,
                        AssignedFrom = DateOnly.FromDateTime(DateTime.UtcNow),
                        AssignedByUserId = actorUserId,
                        ChangeReason = "Penetapan awal preceptor dari checklist orientasi.",
                        CreateBy = actorUserId,
                        CreateDateTime = DateTime.UtcNow
                    };
                    await _dbContext.HrdClinicalPreceptorshipPreceptors.AddAsync(newPr, cancellationToken);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }

                return await GetByIdInternalAsync(existing.Id, cancellationToken);
            }

            var preceptorship = new HrdClinicalPreceptorship
            {
                Id = Guid.NewGuid(),
                OnboardingCaseId = onboardingCase.Id,
                WorkforceProfileId = onboardingCase.WorkforceProfileId,
                OrganizationUnitId = onboardingCase.OrganizationUnitId,
                PreceptorshipStatus = PreceptorshipStatus.PreceptorAssigned,
                RowVersion = Guid.NewGuid(),
                CreateBy = actorUserId,
                CreateDateTime = DateTime.UtcNow
            };

            var initialPreceptor = new HrdClinicalPreceptorshipPreceptor
            {
                Id = Guid.NewGuid(),
                PreceptorshipId = preceptorship.Id,
                PreceptorWorkforceProfileId = preceptorWorkforceProfileId,
                AssignedFrom = DateOnly.FromDateTime(DateTime.UtcNow),
                AssignedByUserId = actorUserId,
                ChangeReason = "Penetapan pertama preceptor pada saat onboarding.",
                CreateBy = actorUserId,
                CreateDateTime = DateTime.UtcNow
            };

            preceptorship.Preceptors.Add(initialPreceptor);
            await _dbContext.HrdClinicalPreceptorships.AddAsync(preceptorship, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return await GetByIdInternalAsync(preceptorship.Id, cancellationToken);
        }

        public async Task<RecruitmentResult<ClinicalPreceptorshipResponse>> ChangePreceptorAsync(
            Guid id,
            ChangePreceptorRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId(user);

            var p = await _dbContext.HrdClinicalPreceptorships
                .Include(x => x.Preceptors)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (p == null)
            {
                return RecruitmentResult<ClinicalPreceptorshipResponse>.NotFound("Data pendampingan klinis tidak ditemukan.");
            }

            if (!IsHrOrAdmin(user))
            {
                var userUnitId = GetUserUnitId(user);
                if (userUnitId.HasValue && p.OrganizationUnitId != userUnitId.Value)
                {
                    return RecruitmentResult<ClinicalPreceptorshipResponse>.Forbidden(
                        "Anda hanya dapat mengganti preceptor untuk unit kerja Anda.");
                }
            }

            // Validasi VR-ONB-08: Preceptor bukan pegawai itu sendiri
            if (request.NewPreceptorWorkforceProfileId == p.WorkforceProfileId)
            {
                return RecruitmentResult<ClinicalPreceptorshipResponse>.Unprocessable(
                    "Preceptor tidak boleh diri pegawai itu sendiri (VR-ONB-08).");
            }

            var preceptorProfile = await _dbContext.MstWorkforceProfiles
                .FirstOrDefaultAsync(w => w.Id == request.NewPreceptorWorkforceProfileId && !w.IsDelete, cancellationToken);

            if (preceptorProfile == null)
            {
                return RecruitmentResult<ClinicalPreceptorshipResponse>.NotFound("Profil preceptor baru tidak ditemukan.");
            }

            // Tutup preceptor aktif sebelumnya
            var currentPreceptor = p.Preceptors.FirstOrDefault(pr => !pr.IsDelete && !pr.AssignedUntil.HasValue);
            if (currentPreceptor != null)
            {
                currentPreceptor.AssignedUntil = DateOnly.FromDateTime(DateTime.UtcNow);
                currentPreceptor.UpdateDateTime = DateTime.UtcNow;
                currentPreceptor.UpdateBy = userId;
            }

            var newPreceptor = new HrdClinicalPreceptorshipPreceptor
            {
                Id = Guid.NewGuid(),
                PreceptorshipId = p.Id,
                PreceptorWorkforceProfileId = request.NewPreceptorWorkforceProfileId,
                AssignedFrom = DateOnly.FromDateTime(DateTime.UtcNow),
                AssignedByUserId = userId,
                ChangeReason = request.ChangeReason.Trim(),
                CreateBy = userId,
                CreateDateTime = DateTime.UtcNow
            };

            await _dbContext.HrdClinicalPreceptorshipPreceptors.AddAsync(newPreceptor, cancellationToken);
            p.RowVersion = Guid.NewGuid();
            p.UpdateDateTime = DateTime.UtcNow;
            p.UpdateBy = userId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetByIdInternalAsync(p.Id, cancellationToken);
        }

        public async Task<RecruitmentResult<ClinicalPreceptorshipResponse>> DeclareIndependentAsync(
            Guid id,
            DeclareClinicalIndependentRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId(user);

            var p = await _dbContext.HrdClinicalPreceptorships
                .Include(x => x.Preceptors)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (p == null)
            {
                return RecruitmentResult<ClinicalPreceptorshipResponse>.NotFound("Data pendampingan klinis tidak ditemukan.");
            }

            if (!IsHrOrAdmin(user))
            {
                var userUnitId = GetUserUnitId(user);
                if (userUnitId.HasValue && p.OrganizationUnitId != userUnitId.Value)
                {
                    return RecruitmentResult<ClinicalPreceptorshipResponse>.Forbidden(
                        "Anda hanya dapat menyatakan siap mandiri untuk unit kerja Anda.");
                }
            }

            // Validasi VR-ONB-60: Siap Mandiri hanya sah saat status Accompanied (didampingi)
            if (p.PreceptorshipStatus != PreceptorshipStatus.Accompanied)
            {
                return RecruitmentResult<ClinicalPreceptorshipResponse>.Unprocessable(
                    $"Pernyataan Siap Mandiri hanya dapat dilakukan saat tenaga kesehatan berstatus 'Accompanied' (Didampingi). Status saat ini: '{p.PreceptorshipStatus}' (VR-ONB-60).");
            }

            // Validasi VR-ONB-61: Wajib lampiran berkas bukti asesmen kompetensi klinis
            if (request.EvidenceFile == null || request.EvidenceFile.Length == 0)
            {
                return RecruitmentResult<ClinicalPreceptorshipResponse>.Unprocessable(
                    "Pernyataan Siap Mandiri wajib menyertakan berkas bukti hasil asesmen kompetensi klinis (VR-ONB-61).");
            }

            var saveResult = await _attachmentService.ValidateAndSaveAttachmentAsync(
                request.EvidenceFile,
                "preceptorship-evaluations",
                cancellationToken);

            if (!saveResult.IsSuccess)
            {
                return RecruitmentResult<ClinicalPreceptorshipResponse>.Fail(saveResult.ErrorMessage ?? "Gagal menyimpan berkas bukti.");
            }

            p.EvidenceStoragePath = saveResult.StoragePath;
            p.EvidenceFileName = saveResult.FileName;
            p.EvidenceContentType = saveResult.ContentType;

            // Tutup penugasan preceptor aktif
            var currentPreceptor = p.Preceptors.FirstOrDefault(pr => !pr.IsDelete && !pr.AssignedUntil.HasValue);
            if (currentPreceptor != null)
            {
                currentPreceptor.AssignedUntil = request.IndependentOn;
                currentPreceptor.UpdateDateTime = DateTime.UtcNow;
                currentPreceptor.UpdateBy = userId;
            }

            p.PreceptorshipStatus = PreceptorshipStatus.Independent;
            p.IndependentOn = request.IndependentOn;
            p.DeclaredByUserId = userId;
            p.AssessmentNote = request.AssessmentNote.Trim();
            p.EvidenceStoragePath = saveResult.StoragePath;
            p.EvidenceFileName = saveResult.FileName;
            p.EvidenceContentType = saveResult.ContentType;
            p.RowVersion = Guid.NewGuid();
            p.UpdateDateTime = DateTime.UtcNow;
            p.UpdateBy = userId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetByIdInternalAsync(p.Id, cancellationToken);
        }

        public async Task<bool> SetAccompaniedAsync(
            Guid onboardingCaseId,
            CancellationToken cancellationToken = default)
        {
            var p = await _dbContext.HrdClinicalPreceptorships
                .FirstOrDefaultAsync(x => x.OnboardingCaseId == onboardingCaseId && !x.IsDelete, cancellationToken);

            if (p == null) return false;

            if (p.PreceptorshipStatus == PreceptorshipStatus.PreceptorAssigned)
            {
                p.PreceptorshipStatus = PreceptorshipStatus.Accompanied;
                p.AccompaniedSince = DateOnly.FromDateTime(DateTime.UtcNow);
                p.RowVersion = Guid.NewGuid();
                p.UpdateDateTime = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            return true;
        }

        public async Task<bool> CancelForCaseAsync(
            Guid onboardingCaseId,
            CancellationToken cancellationToken = default)
        {
            var p = await _dbContext.HrdClinicalPreceptorships
                .FirstOrDefaultAsync(x => x.OnboardingCaseId == onboardingCaseId && !x.IsDelete, cancellationToken);

            if (p == null) return false;

            p.PreceptorshipStatus = PreceptorshipStatus.Cancelled;
            p.RowVersion = Guid.NewGuid();
            p.UpdateDateTime = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task<(bool Exists, Stream? Stream, string? ContentType, string? FileName)> DownloadEvidenceAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var p = await _dbContext.HrdClinicalPreceptorships
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (p == null || string.IsNullOrWhiteSpace(p.EvidenceStoragePath))
            {
                return (false, null, null, null);
            }

            return await _attachmentService.GetAttachmentStreamAsync(
                p.EvidenceStoragePath,
                cancellationToken);
        }

        private async Task<RecruitmentResult<ClinicalPreceptorshipResponse>> GetByIdInternalAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            var p = await _dbContext.HrdClinicalPreceptorships
                .Include(x => x.OnboardingCase)
                .Include(x => x.WorkforceProfile)
                .Include(x => x.OrganizationUnit)
                .Include(x => x.DeclaredByUser)
                .Include(x => x.Preceptors.Where(pr => !pr.IsDelete))
                    .ThenInclude(pr => pr.PreceptorWorkforceProfile)
                .Include(x => x.Preceptors.Where(pr => !pr.IsDelete))
                    .ThenInclude(pr => pr.AssignedByUser)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (p == null)
            {
                return RecruitmentResult<ClinicalPreceptorshipResponse>.NotFound("Data pendampingan klinis tidak ditemukan.");
            }

            return RecruitmentResult<ClinicalPreceptorshipResponse>.Ok(MapToResponse(p), "Detail pendampingan klinis berhasil diambil.");
        }

        private static ClinicalPreceptorshipResponse MapToResponse(HrdClinicalPreceptorship p)
        {
            return new ClinicalPreceptorshipResponse
            {
                Id = p.Id,
                OnboardingCaseId = p.OnboardingCaseId,
                CaseNumber = p.OnboardingCase?.CaseNumber ?? "-",
                WorkforceProfileId = p.WorkforceProfileId,
                WorkforceProfileCode = p.WorkforceProfile?.ProfileCode ?? "-",
                WorkforceDisplayName = p.WorkforceProfile?.DisplayName ?? "-",
                OrganizationUnitId = p.OrganizationUnitId,
                OrganizationUnitName = p.OrganizationUnit?.UnitName ?? "-",
                PreceptorshipStatus = p.PreceptorshipStatus,
                AccompaniedSince = p.AccompaniedSince,
                IndependentOn = p.IndependentOn,
                DeclaredByUserName = p.DeclaredByUser?.UserName,
                EvidenceStoragePath = p.EvidenceStoragePath,
                EvidenceFileName = p.EvidenceFileName,
                AssessmentNote = p.AssessmentNote,
                RowVersion = p.RowVersion,
                Preceptors = p.Preceptors.Where(pr => !pr.IsDelete).OrderBy(pr => pr.AssignedFrom).Select(pr => new ClinicalPreceptorHistoryItemResponse
                {
                    Id = pr.Id,
                    PreceptorWorkforceProfileId = pr.PreceptorWorkforceProfileId,
                    PreceptorProfileCode = pr.PreceptorWorkforceProfile?.ProfileCode ?? "-",
                    PreceptorDisplayName = pr.PreceptorWorkforceProfile?.DisplayName ?? "-",
                    AssignedFrom = pr.AssignedFrom,
                    AssignedUntil = pr.AssignedUntil,
                    ChangeReason = pr.ChangeReason,
                    AssignedByUserName = pr.AssignedByUser?.UserName ?? "-"
                }).ToList()
            };
        }
    }
}
