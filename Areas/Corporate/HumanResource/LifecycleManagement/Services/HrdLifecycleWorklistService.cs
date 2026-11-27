using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services
{
    public class HrdLifecycleWorklistService : IHrdLifecycleWorklistService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly AccessPermissionService? _permissionService;

        public HrdLifecycleWorklistService(
            ApplicationDbContext dbContext,
            AccessPermissionService? permissionService = null)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _permissionService = permissionService;
        }

        private async Task<HashSet<Guid>?> ResolveAllowedUnitScopeAsync(ClaimsPrincipal user, CancellationToken cancellationToken)
        {
            var canViewAll = _permissionService != null
                ? await _permissionService.HasAccessAsync(user, "WorkforceEmploymentContract", EmploymentContractPermissions.ReadAllUnits)
                  || await _permissionService.HasAccessAsync(user, "WorkforceOnboardingCase", OnboardingCasePermissions.ReadAllUnits)
                  || await _permissionService.HasAccessAsync(user, "WorkforceOnboardingCase", "DeclareReady")
                : user.HasClaim("Permission", $"WorkforceEmploymentContract:{EmploymentContractPermissions.ReadAllUnits}")
                  || user.HasClaim("Permission", "WorkforceOnboardingCase:DeclareReady");

            if (canViewAll) return null; // all units

            var sub = user.FindFirst("user_id")?.Value
                   ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? user.FindFirst("sub")?.Value;
            if (!Guid.TryParse(sub, out var userId) || userId == Guid.Empty) return new HashSet<Guid>();

            var wfpId = await _dbContext.Users.AsNoTracking().Where(u => u.Id == userId && u.IsActive).Select(u => u.WorkforceProfileId).FirstOrDefaultAsync(cancellationToken);
            if (!wfpId.HasValue) return new HashSet<Guid>();

            var today = OnboardingCasePolicy.OperationalToday().ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var roots = await _dbContext.WfpOrganizationAssignments
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == wfpId.Value && x.IsManagerialAssignment && x.IsActive && !x.IsDelete && !x.IsCancel &&
                            x.OrganizationUnitId.HasValue && x.EffectiveStartDate <= today && (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= today))
                .Select(x => x.OrganizationUnitId!.Value)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (roots.Count == 0) return new HashSet<Guid>();

            var units = await _dbContext.MstOrganizationUnits.AsNoTracking().Where(u => !u.IsDelete).Select(u => new { u.Id, u.ParentOrganizationUnitId }).ToListAsync(cancellationToken);
            var childrenByParent = units.Where(u => u.ParentOrganizationUnitId.HasValue).GroupBy(u => u.ParentOrganizationUnitId!.Value).ToDictionary(g => g.Key, g => g.Select(c => c.Id).ToList());
            var scope = new HashSet<Guid>(roots);
            var q = new Queue<Guid>(roots);
            while (q.Count > 0)
            {
                var p = q.Dequeue();
                if (!childrenByParent.TryGetValue(p, out var chs)) continue;
                foreach (var ch in chs) if (scope.Add(ch)) q.Enqueue(ch);
            }
            return scope;
        }

        public async Task<RecruitmentResult<PagedResult<LifecycleWorklistItemResponse>>> GetWorklistAsync(
            LifecycleWorklistQuery query,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            query ??= new LifecycleWorklistQuery();
            var pageNumber = query.PageNumber <= 0 ? 1 : query.PageNumber;
            var pageSize = query.PageSize <= 0 ? 10 : (query.PageSize > 100 ? 100 : query.PageSize);

            var unitScope = await ResolveAllowedUnitScopeAsync(user, cancellationToken);
            var allItems = await BuildWorklistItemsAsync(unitScope, cancellationToken);

            var filtered = allItems.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(query.Category))
            {
                filtered = filtered.Where(x => x.Category.Equals(query.Category.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var s = query.Search.Trim().ToLower();
                filtered = filtered.Where(x =>
                    x.FullName.ToLower().Contains(s) ||
                    x.WorkforceCode.ToLower().Contains(s) ||
                    x.Description.ToLower().Contains(s));
            }

            var totalCount = filtered.Count();
            var pagedItems = filtered
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var pagedResult = new PagedResult<LifecycleWorklistItemResponse>
            {
                Items = pagedItems,
                TotalData = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPage = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            return RecruitmentResult<PagedResult<LifecycleWorklistItemResponse>>.Ok(pagedResult);
        }

        public async Task<RecruitmentResult<LifecycleWorklistCountResponse>> GetCountsAsync(
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var unitScope = await ResolveAllowedUnitScopeAsync(user, cancellationToken);
            var allItems = await BuildWorklistItemsAsync(unitScope, cancellationToken);

            var categoryCounts = allItems
                .GroupBy(x => x.Category)
                .ToDictionary(g => g.Key, g => g.Count());

            var response = new LifecycleWorklistCountResponse
            {
                TotalCount = allItems.Count,
                CategoryCounts = categoryCounts
            };

            return RecruitmentResult<LifecycleWorklistCountResponse>.Ok(response);
        }

        private async Task<List<LifecycleWorklistItemResponse>> BuildWorklistItemsAsync(HashSet<Guid>? unitScope, CancellationToken cancellationToken)
        {
            HashSet<Guid>? allowedProfiles = null;
            if (unitScope != null)
            {
                if (unitScope.Count == 0) return new List<LifecycleWorklistItemResponse>();

                var unitIds = unitScope.ToList();
                var todayDt = DateTime.UtcNow.Date;
                var orgProfiles = await _dbContext.WfpOrganizationAssignments
                    .AsNoTracking()
                    .Where(a => a.OrganizationUnitId.HasValue && unitIds.Contains(a.OrganizationUnitId.Value) &&
                                a.IsActive && !a.IsDelete && a.EffectiveStartDate <= todayDt &&
                                (a.EffectiveEndDate == null || a.EffectiveEndDate >= todayDt))
                    .Select(a => a.WorkforceProfileId)
                    .ToListAsync(cancellationToken);

                var caseProfiles = await _dbContext.HrdOnboardingCases
                    .AsNoTracking()
                    .Where(c => !c.IsDelete && unitIds.Contains(c.OrganizationUnitId))
                    .Select(c => c.WorkforceProfileId)
                    .ToListAsync(cancellationToken);

                allowedProfiles = new HashSet<Guid>(orgProfiles.Concat(caseProfiles));
            }
            var now = DateTime.UtcNow;
            var today = DateOnly.FromDateTime(now);
            var todayDateTime = today.ToDateTime(TimeOnly.MinValue);
            var in30Days = todayDateTime.AddDays(30);
            var in60Days = todayDateTime.AddDays(60);
            var in14Days = today.AddDays(14);

            var result = new List<LifecycleWorklistItemResponse>();

            // 1 & 2. ContractExpiring & ContractEscalated
            var contracts = await _dbContext.WfpContractHistories
                .Include(c => c.WorkforceProfile)
                .AsNoTracking()
                .Where(c => !c.IsDelete &&
                            c.ContractStatus == "Signed" &&
                            c.EndDate.HasValue &&
                            c.EndDate.Value >= todayDateTime &&
                            c.EndDate.Value <= in60Days)
                .ToListAsync(cancellationToken);

            foreach (var c in contracts)
            {
                var hasDecision = await _dbContext.HrdContractDecisions
                    .AnyAsync(d => d.ContractHistoryId == c.Id && d.DecisionStatus == ContractDecisionStatus.Approved && !d.IsDelete, cancellationToken);

                if (!hasDecision)
                {
                    bool isEscalated = c.EndDate.Value <= in30Days;
                    result.Add(new LifecycleWorklistItemResponse
                    {
                        Id = Guid.NewGuid(),
                        Category = isEscalated ? "ContractEscalated" : "ContractExpiring",
                        CategoryName = isEscalated ? "Eskalasi Kontrak Mendekati Berakhir (H-30)" : "Pemberitahuan Kontrak Berakhir (H-60)",
                        TargetType = "Contract",
                        TargetId = c.Id,
                        WorkforceProfileId = c.WorkforceProfileId,
                        WorkforceCode = c.WorkforceProfile?.ProfileCode ?? string.Empty,
                        FullName = c.WorkforceProfile?.DisplayName ?? string.Empty,
                        Description = $"Kontrak {c.ContractNumber} akan berakhir pada {DateOnly.FromDateTime(c.EndDate.Value):dd-MM-yyyy}.",
                        DueDate = DateOnly.FromDateTime(c.EndDate.Value),
                        UrgencyLevel = isEscalated ? "High" : "Normal"
                    });
                }
            }

            // 3. ContractLapsed
            var lapsedContracts = await _dbContext.WfpContractHistories
                .Include(c => c.WorkforceProfile)
                .AsNoTracking()
                .Where(c => !c.IsDelete && c.ContractStatus == "LapsedWithoutDecision")
                .ToListAsync(cancellationToken);

            foreach (var c in lapsedContracts)
            {
                result.Add(new LifecycleWorklistItemResponse
                {
                    Id = Guid.NewGuid(),
                    Category = "ContractLapsed",
                    CategoryName = "Kontrak Berakhir Tanpa Keputusan",
                    TargetType = "Contract",
                    TargetId = c.Id,
                    WorkforceProfileId = c.WorkforceProfileId,
                    WorkforceCode = c.WorkforceProfile?.ProfileCode ?? string.Empty,
                    FullName = c.WorkforceProfile?.DisplayName ?? string.Empty,
                    Description = $"Kontrak {c.ContractNumber} telah melewati masa berlaku tanpa penetapan keputusan perpanjangan.",
                    DueDate = c.EndDate.HasValue ? DateOnly.FromDateTime(c.EndDate.Value) : null,
                    UrgencyLevel = "Critical"
                });
            }

            // 4. ProbationDue
            var probations = await _dbContext.HrdProbations
                .Include(p => p.WorkforceProfile)
                .AsNoTracking()
                .Where(p => !p.IsDelete &&
                            p.ProbationStatus == ProbationStatus.Running &&
                            p.EndDate <= in14Days &&
                            p.EndDate >= today)
                .ToListAsync(cancellationToken);

            foreach (var p in probations)
            {
                result.Add(new LifecycleWorklistItemResponse
                {
                    Id = Guid.NewGuid(),
                    Category = "ProbationDue",
                    CategoryName = "Masa Percobaan Mendekati Berakhir (H-14)",
                    TargetType = "Probation",
                    TargetId = p.Id,
                    WorkforceProfileId = p.WorkforceProfileId,
                    WorkforceCode = p.WorkforceProfile?.ProfileCode ?? string.Empty,
                    FullName = p.WorkforceProfile?.DisplayName ?? string.Empty,
                    Description = $"Masa percobaan akan berakhir pada {p.EndDate:dd-MM-yyyy}. Lakukan evaluasi kelulusan.",
                    DueDate = p.EndDate,
                    UrgencyLevel = "High"
                });
            }

            // 5. NonRenewalNotificationPending
            var pendingNotifications = await _dbContext.HrdContractDecisions
                .Include(d => d.WorkforceProfile)
                .Include(d => d.ContractHistory)
                .AsNoTracking()
                .Where(d => !d.IsDelete &&
                            d.DecisionStatus == ContractDecisionStatus.Approved &&
                            d.DecisionType == ContractDecisionType.NotRenew &&
                            !d.EmployeeNotifiedOn.HasValue)
                .ToListAsync(cancellationToken);

            foreach (var d in pendingNotifications)
            {
                result.Add(new LifecycleWorklistItemResponse
                {
                    Id = Guid.NewGuid(),
                    Category = "NonRenewalNotificationPending",
                    CategoryName = "Pemberitahuan Tidak Diperpanjang Belum Dikirim",
                    TargetType = "ContractDecision",
                    TargetId = d.Id,
                    WorkforceProfileId = d.WorkforceProfileId,
                    WorkforceCode = d.WorkforceProfile?.ProfileCode ?? string.Empty,
                    FullName = d.WorkforceProfile?.DisplayName ?? string.Empty,
                    Description = $"Keputusan Tidak Diperpanjang ({d.DecisionNumber}) belum dicatat tanggal pemberitahuannya ke pegawai.",
                    DueDate = d.ProposedEndDate,
                    UrgencyLevel = "High"
                });
            }

            // 6. FirstContractUnsigned
            var unsignedContracts = await _dbContext.WfpContractHistories
                .Include(c => c.WorkforceProfile)
                .AsNoTracking()
                .Where(c => !c.IsDelete && c.HistoryType == "Initial" && c.ContractStatus == "Draft")
                .ToListAsync(cancellationToken);

            foreach (var c in unsignedContracts)
            {
                result.Add(new LifecycleWorklistItemResponse
                {
                    Id = Guid.NewGuid(),
                    Category = "FirstContractUnsigned",
                    CategoryName = "Kontrak Pertama Belum Ditandatangani",
                    TargetType = "Contract",
                    TargetId = c.Id,
                    WorkforceProfileId = c.WorkforceProfileId,
                    WorkforceCode = c.WorkforceProfile?.ProfileCode ?? string.Empty,
                    FullName = c.WorkforceProfile?.DisplayName ?? string.Empty,
                    Description = $"Draf kontrak pertama ({c.ContractNumber}) belum dicatat penandatanganannya.",
                    DueDate = DateOnly.FromDateTime(c.StartDate),
                    UrgencyLevel = "Normal"
                });
            }

            // 7. LateStart
            var lateStartCases = await _dbContext.HrdOnboardingCases
                .Include(c => c.WorkforceProfile)
                .AsNoTracking()
                .Where(c => !c.IsDelete &&
                            c.CaseStatus == OnboardingCaseStatus.InProgress &&
                            c.PlannedStartDate < today &&
                            !c.FirstDayAttendedOn.HasValue)
                .ToListAsync(cancellationToken);

            foreach (var c in lateStartCases)
            {
                result.Add(new LifecycleWorklistItemResponse
                {
                    Id = Guid.NewGuid(),
                    Category = "LateStart",
                    CategoryName = "Keterlambatan Hadir Hari Pertama",
                    TargetType = "OnboardingCase",
                    TargetId = c.Id,
                    WorkforceProfileId = c.WorkforceProfileId,
                    WorkforceCode = c.WorkforceProfile?.ProfileCode ?? string.Empty,
                    FullName = c.WorkforceProfile?.DisplayName ?? string.Empty,
                    Description = $"Kasus orientasi {c.CaseNumber} melewati tanggal mulai rencana ({c.PlannedStartDate:dd-MM-yyyy}) tanpa kehadiran.",
                    DueDate = c.PlannedStartDate,
                    UrgencyLevel = "High"
                });
            }

            // 8. LegacyContractDocumentMissing
            var missingScanContracts = await _dbContext.WfpContractHistories
                .Include(c => c.WorkforceProfile)
                .AsNoTracking()
                .Where(c => !c.IsDelete &&
                            (c.HistoryType == "InitialData" || c.SourceDocumentType == "InitialData") &&
                            string.IsNullOrWhiteSpace(c.DocumentPath))
                .ToListAsync(cancellationToken);

            foreach (var c in missingScanContracts)
            {
                result.Add(new LifecycleWorklistItemResponse
                {
                    Id = Guid.NewGuid(),
                    Category = "LegacyContractDocumentMissing",
                    CategoryName = "Dokumen Pindaian Kontrak Lama Belum Lengkap",
                    TargetType = "Contract",
                    TargetId = c.Id,
                    WorkforceProfileId = c.WorkforceProfileId,
                    WorkforceCode = c.WorkforceProfile?.ProfileCode ?? string.Empty,
                    FullName = c.WorkforceProfile?.DisplayName ?? string.Empty,
                    Description = $"Kontrak migrasi data awal ({c.ContractNumber}) belum memiliki unggahan berkas pindaian dokumen fisik.",
                    DueDate = null,
                    UrgencyLevel = "Low"
                });
            }

            return allowedProfiles == null
                ? result
                : result.Where(item => allowedProfiles.Contains(item.WorkforceProfileId)).ToList();
        }
    }
}
