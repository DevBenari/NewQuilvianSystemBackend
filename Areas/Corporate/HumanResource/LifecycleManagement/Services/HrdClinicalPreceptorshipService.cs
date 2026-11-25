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
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services
{
    public class HrdClinicalPreceptorshipService : IHrdClinicalPreceptorshipService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IHrdLifecycleAttachmentService _attachmentService;
        private readonly AccessPermissionService? _permissionService;

        public HrdClinicalPreceptorshipService(
            ApplicationDbContext dbContext,
            IHrdLifecycleAttachmentService attachmentService,
            AccessPermissionService? permissionService = null)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _attachmentService = attachmentService ?? throw new ArgumentNullException(nameof(attachmentService));
            _permissionService = permissionService;
        }

        private static Guid GetUserId(ClaimsPrincipal? user)
        {
            var sub = user?.FindFirst("user_id")?.Value
                   ?? user?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? user?.FindFirst("sub")?.Value;
            return Guid.TryParse(sub, out var guid) ? guid : Guid.Empty;
        }

        private async Task<bool> HasAccessAsync(ClaimsPrincipal? user, string controller, string action)
        {
            if (user?.Identity?.IsAuthenticated != true) return false;
            if (_permissionService != null)
            {
                return await _permissionService.HasAccessAsync(user, controller, action);
            }
            return user.HasClaim("Permission", $"{controller}:{action}");
        }

        private async Task<HashSet<Guid>> GetManagedUnitIdsAsync(ClaimsPrincipal? user, CancellationToken cancellationToken)
        {
            var userId = GetUserId(user);
            if (userId == Guid.Empty) return new HashSet<Guid>();

            var callerProfileId = await _dbContext.Users
                .AsNoTracking()
                .Where(u => u.Id == userId && u.IsActive)
                .Select(u => u.WorkforceProfileId)
                .FirstOrDefaultAsync(cancellationToken);

            if (!callerProfileId.HasValue || callerProfileId.Value == Guid.Empty)
            {
                return new HashSet<Guid>();
            }

            var today = OnboardingCasePolicy.OperationalToday().ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var roots = await _dbContext.WfpOrganizationAssignments
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == callerProfileId.Value &&
                            x.IsManagerialAssignment &&
                            x.IsActive &&
                            !x.IsDelete &&
                            !x.IsCancel &&
                            x.OrganizationUnitId.HasValue &&
                            x.EffectiveStartDate <= today &&
                            (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= today))
                .Select(x => x.OrganizationUnitId!.Value)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (roots.Count == 0) return new HashSet<Guid>();

            var units = await _dbContext.MstOrganizationUnits
                .AsNoTracking()
                .Where(u => !u.IsDelete)
                .Select(u => new { u.Id, u.ParentOrganizationUnitId })
                .ToListAsync(cancellationToken);

            var childrenByParent = units
                .Where(u => u.ParentOrganizationUnitId.HasValue)
                .GroupBy(u => u.ParentOrganizationUnitId!.Value)
                .ToDictionary(g => g.Key, g => g.Select(c => c.Id).ToList());

            var result = new HashSet<Guid>(roots);
            var queue = new Queue<Guid>(roots);
            while (queue.Count > 0)
            {
                var cur = queue.Dequeue();
                if (!childrenByParent.TryGetValue(cur, out var children)) continue;
                foreach (var ch in children)
                {
                    if (result.Add(ch)) queue.Enqueue(ch);
                }
            }
            return result;
        }

        private async Task<bool> CanViewAllUnitsAsync(ClaimsPrincipal? user)
        {
            return await HasAccessAsync(user, "WorkforceOnboardingCase", OnboardingCasePermissions.ReadAllUnits)
                || await HasAccessAsync(user, "WorkforceEmploymentContract", EmploymentContractPermissions.ReadAllUnits)
                || await HasAccessAsync(user, "WorkforceOnboardingCase", "DeclareReady");
        }

        public async Task<RecruitmentResult<PagedResult<ClinicalPreceptorshipListItemResponse>>> GetPagedAsync(
            ClinicalPreceptorshipQuery query,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            query ??= new ClinicalPreceptorshipQuery();
            var dbQuery = _dbContext.HrdClinicalPreceptorships
                .Include(p => p.OnboardingCase)
                .Include(p => p.WorkforceProfile)
                .Include(p => p.OrganizationUnit)
                .Include(p => p.Preceptors.Where(pr => !pr.IsDelete && !pr.AssignedUntil.HasValue))
                    .ThenInclude(pr => pr.PreceptorWorkforceProfile)
                .Where(p => !p.IsDelete)
                .AsNoTracking()
                .AsQueryable();

            if (!await CanViewAllUnitsAsync(user))
            {
                var managedUnits = await GetManagedUnitIdsAsync(user, cancellationToken);
                var userId = GetUserId(user);
                var callerProfileId = userId == Guid.Empty
                    ? null
                    : await _dbContext.Users.AsNoTracking().Where(u => u.Id == userId).Select(u => u.WorkforceProfileId).FirstOrDefaultAsync(cancellationToken);

                var unitList = managedUnits.ToList();
                dbQuery = dbQuery.Where(p =>
                    (unitList.Count > 0 && unitList.Contains(p.OrganizationUnitId)) ||
                    (callerProfileId.HasValue && p.WorkforceProfileId == callerProfileId.Value) ||
                    (callerProfileId.HasValue && p.Preceptors.Any(pr => !pr.IsDelete && !pr.AssignedUntil.HasValue && pr.PreceptorWorkforceProfileId == callerProfileId.Value)));
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

            if (!await CanViewAllUnitsAsync(user))
            {
                var managedUnits = await GetManagedUnitIdsAsync(user, cancellationToken);
                var userId = GetUserId(user);
                var callerProfileId = userId == Guid.Empty
                    ? null
                    : await _dbContext.Users.AsNoTracking().Where(u => u.Id == userId).Select(u => u.WorkforceProfileId).FirstOrDefaultAsync(cancellationToken);

                var canReadThis = managedUnits.Contains(p.OrganizationUnitId)
                    || (callerProfileId.HasValue && p.WorkforceProfileId == callerProfileId.Value)
                    || (callerProfileId.HasValue && p.Preceptors.Any(pr => !pr.IsDelete && !pr.AssignedUntil.HasValue && pr.PreceptorWorkforceProfileId == callerProfileId.Value));

                if (!canReadThis)
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

            // Validasi VR-ONB-08: Preceptor harus profil aktif
            var preceptorProfile = await _dbContext.MstWorkforceProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == preceptorWorkforceProfileId && !w.IsDelete, cancellationToken);

            if (preceptorProfile == null || !preceptorProfile.IsActive)
            {
                return RecruitmentResult<ClinicalPreceptorshipResponse>.Unprocessable("Profil tenaga kesehatan preceptor tidak ditemukan atau belum aktif (VR-ONB-08).");
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

            var managedUnits = await GetManagedUnitIdsAsync(user, cancellationToken);
            if (!managedUnits.Contains(p.OrganizationUnitId))
            {
                return RecruitmentResult<ClinicalPreceptorshipResponse>.Forbidden(
                    "Penggantian preceptor hanya dapat dilakukan oleh Kepala Unit terkait (VR-ONB-08 / HRD-DEC-087).");
            }

            if (p.PreceptorshipStatus == PreceptorshipStatus.Independent || p.PreceptorshipStatus == PreceptorshipStatus.Cancelled)
            {
                return RecruitmentResult<ClinicalPreceptorshipResponse>.Unprocessable(
                    $"Preceptor tidak dapat diganti karena status pendampingan saat ini adalah '{p.PreceptorshipStatus}'.");
            }

            // Validasi VR-ONB-08: Preceptor bukan pegawai itu sendiri
            if (request.NewPreceptorWorkforceProfileId == p.WorkforceProfileId)
            {
                return RecruitmentResult<ClinicalPreceptorshipResponse>.Unprocessable(
                    "Preceptor tidak boleh diri pegawai itu sendiri (VR-ONB-08).");
            }

            var preceptorProfile = await _dbContext.MstWorkforceProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == request.NewPreceptorWorkforceProfileId && !w.IsDelete, cancellationToken);

            if (preceptorProfile == null || !preceptorProfile.IsActive)
            {
                return RecruitmentResult<ClinicalPreceptorshipResponse>.Unprocessable("Profil preceptor baru tidak ditemukan atau belum aktif (VR-ONB-08).");
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

            var managedUnits = await GetManagedUnitIdsAsync(user, cancellationToken);
            if (!managedUnits.Contains(p.OrganizationUnitId))
            {
                return RecruitmentResult<ClinicalPreceptorshipResponse>.Forbidden(
                    "Pernyataan Siap Mandiri hanya dapat dilakukan oleh Kepala Unit terkait tanpa persetujuan HRD (HRD-DEC-087).");
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
