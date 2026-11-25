using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services
{
    public class HrdOnboardingCaseService : IHrdOnboardingCaseService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IHrdNumberSeriesService _numberSeriesService;
        private readonly IHrdEmploymentContractService _contractService;
        private readonly IEmployeeUserAccountService _userAccountService;
        private readonly IHrdLifecycleAttachmentService _attachmentService;
        private readonly IHrdClinicalPreceptorshipService _preceptorshipService;
        private readonly AccessPermissionService? _permissionService;
        private readonly HrdUnitScopeService? _unitScopeService;

        public HrdOnboardingCaseService(
            ApplicationDbContext dbContext,
            IHrdNumberSeriesService numberSeriesService,
            IHrdEmploymentContractService contractService,
            IEmployeeUserAccountService userAccountService,
            IHrdLifecycleAttachmentService attachmentService,
            IHrdClinicalPreceptorshipService preceptorshipService,
            AccessPermissionService? permissionService = null,
            HrdUnitScopeService? unitScopeService = null)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _numberSeriesService = numberSeriesService ?? throw new ArgumentNullException(nameof(numberSeriesService));
            _contractService = contractService ?? throw new ArgumentNullException(nameof(contractService));
            _userAccountService = userAccountService ?? throw new ArgumentNullException(nameof(userAccountService));
            _attachmentService = attachmentService ?? throw new ArgumentNullException(nameof(attachmentService));
            _preceptorshipService = preceptorshipService ?? throw new ArgumentNullException(nameof(preceptorshipService));
            _permissionService = permissionService;
            _unitScopeService = unitScopeService;
        }

        private sealed record ActorCapabilities(
            Guid UserId,
            Guid? WorkforceProfileId,
            bool IsHrOperator,
            bool CanViewAllUnits,
            IReadOnlySet<Guid> ManagedUnitIds,
            bool CanCompleteItem,
            bool CanReassignCoordinator,
            bool CanDeclareReady,
            bool CanCancelJoin,
            bool CanViewContractAmount,
            bool CanRetryAccountCreation,
            bool CanCompleteWorkAccess,
            bool CanAssignPreceptor,
            bool CanReadEmployeeFull,
            bool CanReadBankAccount,
            bool CanReadDocuments)
        {
            public bool IsUnitHeadFor(Guid organizationUnitId) =>
                organizationUnitId != Guid.Empty && ManagedUnitIds.Contains(organizationUnitId);

            public bool CanReadCase(HrdOnboardingCase c) =>
                CanViewAllUnits
                || IsUnitHeadFor(c.OrganizationUnitId)
                || (UserId != Guid.Empty && c.CoordinatorUserId == UserId)
                || (WorkforceProfileId.HasValue && c.WorkforceProfileId == WorkforceProfileId.Value);
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

        private async Task<ActorCapabilities> ResolveActorAsync(ClaimsPrincipal? user, CancellationToken cancellationToken)
        {
            if (user == null || user.Identity?.IsAuthenticated != true)
            {
                return new ActorCapabilities(
                    Guid.Empty, null, false, false, new HashSet<Guid>(),
                    false, false, false, false, false, false, false, false, false, false, false);
            }

            var userId = GetUserId(user);
            var callerProfileId = userId == Guid.Empty
                ? null
                : await _dbContext.Users
                    .AsNoTracking()
                    .Where(x => x.Id == userId && x.IsActive)
                    .Select(x => x.WorkforceProfileId)
                    .FirstOrDefaultAsync(cancellationToken);

            var canRead = await HasAccessAsync(user, "WorkforceOnboardingCase", "Read");
            var canCompleteItem = await HasAccessAsync(user, "WorkforceOnboardingCase", "CompleteItem");
            var canReassign = await HasAccessAsync(user, "WorkforceOnboardingCase", "Reassign");
            var canDeclareReady = await HasAccessAsync(user, "WorkforceOnboardingCase", "DeclareReady");
            var canCancelJoin = await HasAccessAsync(user, "WorkforceOnboardingCase", "CancelJoin");
            var hasReadAllFlag = await HasAccessAsync(user, "WorkforceOnboardingCase", OnboardingCasePermissions.ReadAllUnits)
                              || await HasAccessAsync(user, "WorkforceEmploymentContract", EmploymentContractPermissions.ReadAllUnits);

            var isHrOperator = canDeclareReady || canReassign || canCancelJoin;
            var canViewAllUnits = (canRead && hasReadAllFlag) || isHrOperator;

            var canViewContractAmount = await HasAccessAsync(user, "WorkforceEmploymentContract", "ViewAmount");
            var canRetryAccount = await HasAccessAsync(user, "WorkforceOnboardingAccessRequest", "Retry");
            var canCompleteWorkAccess = await HasAccessAsync(user, "WorkforceOnboardingAccessRequest", "Complete");
            var canAssignPreceptor = await HasAccessAsync(user, "WorkforcePreceptorship", "Manage");
            var canReadEmployeeFull = await HasAccessAsync(user, "HrdEmployee360", "ReadFull")
                                   || await HasAccessAsync(user, "Employee", "Read")
                                   || await HasAccessAsync(user, "Doctor", "Read");
            var canReadBankAccount = await HasAccessAsync(user, "WorkforceBankAccount", "Read")
                                  || await HasAccessAsync(user, "WorkforcePayrollData", "Read")
                                  || canViewContractAmount;
            var canReadDocuments = await HasAccessAsync(user, "WorkforceDocument", "Read")
                                || canReadEmployeeFull;

            var managedUnits = await ResolveManagerialUnitScopeAsync(callerProfileId, cancellationToken);

            return new ActorCapabilities(
                userId,
                callerProfileId,
                isHrOperator,
                canViewAllUnits,
                managedUnits,
                canCompleteItem,
                canReassign,
                canDeclareReady,
                canCancelJoin,
                canViewContractAmount,
                canRetryAccount,
                canCompleteWorkAccess,
                canAssignPreceptor,
                canReadEmployeeFull,
                canReadBankAccount,
                canReadDocuments);
        }

        private async Task<HashSet<Guid>> ResolveManagerialUnitScopeAsync(Guid? workforceProfileId, CancellationToken cancellationToken)
        {
            if (!workforceProfileId.HasValue || workforceProfileId.Value == Guid.Empty)
            {
                return new HashSet<Guid>();
            }

            var today = OnboardingCasePolicy.OperationalToday().ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var rootUnitIds = await _dbContext.WfpOrganizationAssignments
                .AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId.Value &&
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

            if (rootUnitIds.Count == 0)
            {
                return new HashSet<Guid>();
            }

            var units = await _dbContext.MstOrganizationUnits
                .AsNoTracking()
                .Where(u => !u.IsDelete)
                .Select(u => new { u.Id, u.ParentOrganizationUnitId })
                .ToListAsync(cancellationToken);

            var childrenByParent = units
                .Where(u => u.ParentOrganizationUnitId.HasValue)
                .GroupBy(u => u.ParentOrganizationUnitId!.Value)
                .ToDictionary(g => g.Key, g => g.Select(c => c.Id).ToList());

            var readable = new HashSet<Guid>(rootUnitIds);
            var queue = new Queue<Guid>(rootUnitIds);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (!childrenByParent.TryGetValue(current, out var children)) continue;
                foreach (var childId in children)
                {
                    if (readable.Add(childId))
                    {
                        queue.Enqueue(childId);
                    }
                }
            }

            return readable;
        }

        public async Task<RecruitmentResult<OnboardingCaseDetailResponse>> CreateFromHandoffAsync(
            Guid handoffId,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var existingCase = await LoadFullCaseAsync(c => c.HiringHandoffId == handoffId && !c.IsDelete, cancellationToken);
            if (existingCase != null)
            {
                var actor = await ResolveActorAsync(null, cancellationToken);
                return RecruitmentResult<OnboardingCaseDetailResponse>.Ok(
                    await MapToDetailResponseAsync(existingCase, actor, cancellationToken),
                    "Kasus onboarding sudah ada untuk serah terima ini (idempoten).");
            }

            var handoff = await _dbContext.HrdCandidateHiringHandoffs
                .Include(h => h.Offer)
                    .ThenInclude(o => o!.Application)
                        .ThenInclude(a => a!.Candidate)
                .Include(h => h.Offer)
                    .ThenInclude(o => o!.Application)
                        .ThenInclude(a => a!.Vacancy)
                            .ThenInclude(v => v!.Requisition)
                .Include(h => h.WorkforceProfile)
                .FirstOrDefaultAsync(h => h.Id == handoffId && !h.IsDelete, cancellationToken);

            if (handoff == null)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.NotFound("Dokumen serah terima hiring handoff tidak ditemukan.");
            }

            var offer = handoff.Offer;
            var app = offer?.Application;
            var candidate = app?.Candidate;
            var vacancy = app?.Vacancy;
            var profile = handoff.WorkforceProfile;

            if (offer == null || app == null || candidate == null || vacancy == null || profile == null)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Unprocessable(
                    "Data serah terima, penawaran kerja, kandidat, lowongan, atau profil tenaga kerja tidak lengkap.");
            }

            var contractTypeExists = await _dbContext.MstContractTypes
                .AsNoTracking()
                .AnyAsync(ct => ct.Id == offer.ContractTypeId && !ct.IsDelete && ct.IsActive, cancellationToken);
            if (!contractTypeExists)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Unprocessable(
                    "Jenis kontrak pada penawaran kerja tidak ditemukan atau tidak aktif; pembentukan kasus onboarding dibatalkan.");
            }

            var requisition = vacancy.Requisition;
            var siteId = requisition?.HospitalSiteId ?? Guid.Empty;
            var orgUnitId = requisition?.OrganizationUnitId ?? Guid.Empty;
            var positionId = requisition?.PositionId ?? Guid.Empty;
            var isClinical = vacancy.IsClinical;
            var plannedStartDate = offer.ProposedJoinDate;

            var caseNumber = await _numberSeriesService.GenerateOnboardingCaseNumberAsync(cancellationToken);

            var onboardingCase = new HrdOnboardingCase
            {
                Id = Guid.NewGuid(),
                CaseNumber = caseNumber,
                HiringHandoffId = handoff.Id,
                WorkforceProfileId = profile.Id,
                HospitalSiteId = siteId,
                OrganizationUnitId = orgUnitId,
                PositionId = positionId,
                IsClinical = isClinical,
                PlannedStartDate = plannedStartDate,
                CoordinatorUserId = actorUserId,
                CaseStatus = OnboardingCaseStatus.InProgress,
                HasGeneralTemplate = false,
                RowVersion = Guid.NewGuid(),
                CreateBy = actorUserId,
                CreateDateTime = DateTime.UtcNow
            };

            var mcu = await _dbContext.HrdPreEmploymentMedicalChecks
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.OfferId == offer.Id && !m.IsDelete, cancellationToken);
            var isFit = mcu != null && (mcu.ClearanceResult == McuClearanceResult.Fit || mcu.ClearanceResult == McuClearanceResult.FitWithRestrictions);

            var generalTemplate = await _dbContext.MstOnboardingTemplates
                .Include(t => t.Tasks.Where(task => task.IsActive && !task.IsDelete))
                .FirstOrDefaultAsync(t => t.TemplateLayer == OnboardingTemplateLayer.General && t.IsActive && !t.IsDelete, cancellationToken);

            if (generalTemplate != null)
            {
                onboardingCase.HasGeneralTemplate = true;
            }

            MstOnboardingTemplate? clinicalTemplate = null;
            if (isClinical)
            {
                clinicalTemplate = await _dbContext.MstOnboardingTemplates
                    .Include(t => t.Tasks.Where(task => task.IsActive && !task.IsDelete))
                    .FirstOrDefaultAsync(t => t.TemplateLayer == OnboardingTemplateLayer.Clinical && t.IsActive && !t.IsDelete, cancellationToken);
            }

            MstOnboardingTemplate? unitTemplate = null;
            if (orgUnitId != Guid.Empty)
            {
                unitTemplate = await _dbContext.MstOnboardingTemplates
                    .Include(t => t.Tasks.Where(task => task.IsActive && !task.IsDelete))
                    .FirstOrDefaultAsync(t => t.TemplateLayer == OnboardingTemplateLayer.Unit && t.OrganizationUnitId == orgUnitId && t.IsActive && !t.IsDelete, cancellationToken);
            }

            var items = new List<HrdOnboardingCaseItem>();
            var registeredCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            void AddTaskItem(MstOnboardingTemplateTask task, OnboardingTemplateLayer layer)
            {
                var codeKey = !string.IsNullOrWhiteSpace(task.SystemItemCode) ? task.SystemItemCode : task.TaskCode;
                if (!registeredCodes.Add(codeKey)) return;

                var item = new HrdOnboardingCaseItem
                {
                    Id = Guid.NewGuid(),
                    OnboardingCaseId = onboardingCase.Id,
                    TemplateLayer = layer,
                    SourceTemplateTaskId = task.Id,
                    ItemCode = task.TaskCode,
                    ItemName = task.TaskName,
                    TaskCategory = task.TaskCategory,
                    Description = task.Description,
                    SystemItemCode = task.SystemItemCode,
                    ResponsibleRole = task.ResponsibleRole,
                    ItemKind = task.ItemKind,
                    AutomaticSource = task.AutomaticSource,
                    CertificationTypeId = task.CertificationTypeId,
                    IsRequired = task.IsRequired,
                    DueDayOffset = task.DueDayOffset,
                    DueDate = plannedStartDate.AddDays(task.DueDayOffset),
                    ItemStatus = OnboardingItemStatus.Pending,
                    RowVersion = Guid.NewGuid(),
                    CreateBy = actorUserId,
                    CreateDateTime = DateTime.UtcNow
                };

                if (item.SystemItemCode == EmploymentLifecycleValueConstants.ItemFitnessClearance && isFit)
                {
                    item.ItemStatus = OnboardingItemStatus.Completed;
                    item.CompletedAt = DateTime.UtcNow;
                    item.CompletedByUserId = actorUserId;
                    item.AutomaticCheckedAt = DateTime.UtcNow;
                    item.AutomaticResultNote = $"Kelaikan medis terverifikasi ({OnboardingCasePolicy.GetSafeMcuLabel(mcu?.ClearanceResult)}).";
                }

                items.Add(item);
            }

            if (generalTemplate != null)
            {
                foreach (var task in generalTemplate.Tasks.OrderBy(t => t.DueDayOffset).ThenBy(t => t.TaskCode))
                {
                    AddTaskItem(task, OnboardingTemplateLayer.General);
                }
            }

            if (clinicalTemplate != null)
            {
                foreach (var task in clinicalTemplate.Tasks.OrderBy(t => t.DueDayOffset).ThenBy(t => t.TaskCode))
                {
                    AddTaskItem(task, OnboardingTemplateLayer.Clinical);
                }
            }

            if (unitTemplate != null)
            {
                foreach (var task in unitTemplate.Tasks.OrderBy(t => t.DueDayOffset).ThenBy(t => t.TaskCode))
                {
                    AddTaskItem(task, OnboardingTemplateLayer.Unit);
                }
            }

            void EnsureSystemItem(string sysCode, string name, OnboardingTemplateLayer layer, OnboardingResponsibleRole role, OnboardingItemKind kind, OnboardingAutomaticSource? autoSource, int offset)
            {
                if (!registeredCodes.Add(sysCode)) return;

                var fallbackItem = new HrdOnboardingCaseItem
                {
                    Id = Guid.NewGuid(),
                    OnboardingCaseId = onboardingCase.Id,
                    TemplateLayer = layer,
                    ItemCode = sysCode,
                    ItemName = name,
                    TaskCategory = "System",
                    Description = $"Butir wajib sistem: {name}",
                    SystemItemCode = sysCode,
                    ResponsibleRole = role,
                    ItemKind = kind,
                    AutomaticSource = autoSource,
                    IsRequired = true,
                    DueDayOffset = offset,
                    DueDate = plannedStartDate.AddDays(offset),
                    ItemStatus = OnboardingItemStatus.Pending,
                    RowVersion = Guid.NewGuid(),
                    CreateBy = actorUserId,
                    CreateDateTime = DateTime.UtcNow
                };

                if (sysCode == EmploymentLifecycleValueConstants.ItemFitnessClearance && isFit)
                {
                    fallbackItem.ItemStatus = OnboardingItemStatus.Completed;
                    fallbackItem.CompletedAt = DateTime.UtcNow;
                    fallbackItem.CompletedByUserId = actorUserId;
                    fallbackItem.AutomaticCheckedAt = DateTime.UtcNow;
                    fallbackItem.AutomaticResultNote = $"Kelaikan medis terverifikasi ({OnboardingCasePolicy.GetSafeMcuLabel(mcu?.ClearanceResult)}).";
                }

                items.Add(fallbackItem);
            }

            EnsureSystemItem(EmploymentLifecycleValueConstants.ItemFirstDayAttendance, "Pencatatan Kehadiran Hari Pertama", OnboardingTemplateLayer.General, OnboardingResponsibleRole.Hrd, OnboardingItemKind.Manual, null, 0);
            EnsureSystemItem(EmploymentLifecycleValueConstants.ItemContractSigned, "Penandatanganan Kontrak Kerja", OnboardingTemplateLayer.General, OnboardingResponsibleRole.Hrd, OnboardingItemKind.Automatic, OnboardingAutomaticSource.ContractSigned, 0);
            EnsureSystemItem(EmploymentLifecycleValueConstants.ItemFitnessClearance, "Pemeriksaan Kelaikan Medis / MCU Pra-Kerja", OnboardingTemplateLayer.General, OnboardingResponsibleRole.Hrd, OnboardingItemKind.Automatic, OnboardingAutomaticSource.FitnessClearance, 0);
            EnsureSystemItem(EmploymentLifecycleValueConstants.ItemDirectSupervisorAssigned, "Penetapan Atasan Langsung", OnboardingTemplateLayer.Unit, OnboardingResponsibleRole.UnitHead, OnboardingItemKind.PersonSelection, null, 7);

            if (isClinical)
            {
                EnsureSystemItem(EmploymentLifecycleValueConstants.ItemPreceptorAssigned, "Penetapan Preceptor / Pendamping Klinis", OnboardingTemplateLayer.Clinical, OnboardingResponsibleRole.UnitHead, OnboardingItemKind.PersonSelection, null, 7);
            }

            onboardingCase.Items = items;

            // Draf kontrak pertama (BE-HRD-072)
            var createdDraft = await _contractService.CreateDraftFromOfferAsync(profile.Id, offer, actorUserId, cancellationToken);
            if (createdDraft == null)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Unprocessable(
                    "Gagal menyusun draf kontrak kerja pertama dari penawaran.");
            }

            // Permintaan akun pengguna mandiri (BE-HRD-066 / VR-ONB-75)
            var accessRequest = new HrdOnboardingAccessRequest
            {
                Id = Guid.NewGuid(),
                OnboardingCaseId = onboardingCase.Id,
                WorkforceProfileId = profile.Id,
                RequestType = OnboardingAccessRequestType.AccountCreation,
                RequestStatus = OnboardingAccessRequestStatus.Requested,
                RequestedAt = DateTime.UtcNow,
                CreateBy = actorUserId,
                CreateDateTime = DateTime.UtcNow
            };

            try
            {
                var accountResult = await _userAccountService.CreateForWorkforceProfileAsync(
                    profile.Id,
                    actorUserId,
                    isFingerprintRegistrationEnabled: false,
                    fingerprintRegistrationReason: null,
                    cancellationToken: cancellationToken);

                if (accountResult.Success && accountResult.User != null)
                {
                    accessRequest.RequestStatus = OnboardingAccessRequestStatus.Completed;
                    accessRequest.ApplicationUserId = accountResult.User.Id;
                    accessRequest.CompletedAt = DateTime.UtcNow;
                    accessRequest.CompletedByUserId = actorUserId;
                    accessRequest.CompletionNote = "Akun pengguna layanan mandiri berhasil dibuat otomatis.";
                }
                else
                {
                    accessRequest.RequestStatus = OnboardingAccessRequestStatus.Failed;
                    accessRequest.FailureMessage = OnboardingCasePolicy.SanitizeAccountFailureMessage(accountResult.Message);
                }
            }
            catch (Exception)
            {
                accessRequest.RequestStatus = OnboardingAccessRequestStatus.Failed;
                accessRequest.FailureMessage = OnboardingCasePolicy.SanitizeAccountFailureMessage("Exception");
            }

            onboardingCase.AccessRequests.Add(accessRequest);

            await _dbContext.HrdOnboardingCases.AddAsync(onboardingCase, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var sysActor = await ResolveActorAsync(null, cancellationToken);
            return await GetByIdInternalAsync(onboardingCase.Id, sysActor, cancellationToken);
        }

        public async Task<RecruitmentResult<PagedResult<OnboardingCaseListItemResponse>>> GetPagedAsync(
            OnboardingCaseQuery query,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            query ??= new OnboardingCaseQuery();
            var actor = await ResolveActorAsync(user, cancellationToken);

            var dbQuery = ApplyCaseReadScope(_dbContext.HrdOnboardingCases
                .Include(c => c.WorkforceProfile)
                .Include(c => c.OrganizationUnit)
                .Include(c => c.Position)
                .Include(c => c.Items)
                .Where(c => !c.IsDelete)
                .AsNoTracking(), actor);

            if (query.CaseStatus.HasValue)
            {
                dbQuery = dbQuery.Where(c => c.CaseStatus == query.CaseStatus.Value);
            }

            if (query.OrganizationUnitId.HasValue)
            {
                dbQuery = dbQuery.Where(c => c.OrganizationUnitId == query.OrganizationUnitId.Value);
            }

            if (query.IsClinical.HasValue)
            {
                dbQuery = dbQuery.Where(c => c.IsClinical == query.IsClinical.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var term = query.Search.Trim().ToLower();
                dbQuery = dbQuery.Where(c =>
                    c.CaseNumber.ToLower().Contains(term) ||
                    (c.WorkforceProfile != null && (c.WorkforceProfile.DisplayName.ToLower().Contains(term) || c.WorkforceProfile.ProfileCode.ToLower().Contains(term))) ||
                    (c.Position != null && c.Position.PositionName.ToLower().Contains(term)));
            }

            var totalCount = await dbQuery.CountAsync(cancellationToken);
            var pageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
            var pageSize = query.PageSize > 0 ? Math.Min(query.PageSize, 100) : 10;

            var items = await dbQuery
                .OrderByDescending(c => c.CreateDateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new OnboardingCaseListItemResponse
                {
                    Id = c.Id,
                    CaseNumber = c.CaseNumber,
                    WorkforceProfileId = c.WorkforceProfileId,
                    WorkforceCode = c.WorkforceProfile != null ? c.WorkforceProfile.ProfileCode : "-",
                    FullName = c.WorkforceProfile != null ? c.WorkforceProfile.DisplayName : "-",
                    PositionTitle = c.Position != null ? c.Position.PositionName : "-",
                    OrganizationUnitName = c.OrganizationUnit != null ? c.OrganizationUnit.UnitName : "-",
                    IsClinical = c.IsClinical,
                    PlannedStartDate = c.PlannedStartDate,
                    CaseStatus = c.CaseStatus,
                    CompletedItemCount = c.Items.Count(i => !i.IsDelete && i.ItemStatus == OnboardingItemStatus.Completed),
                    TotalItemCount = c.Items.Count(i => !i.IsDelete)
                })
                .ToListAsync(cancellationToken);

            var totalPages = pageSize > 0 ? (int)Math.Ceiling((double)totalCount / pageSize) : 0;

            return RecruitmentResult<PagedResult<OnboardingCaseListItemResponse>>.Ok(new PagedResult<OnboardingCaseListItemResponse>
            {
                Items = items,
                TotalData = totalCount,
                TotalPage = totalPages,
                PageNumber = pageNumber,
                PageSize = pageSize
            }, "Daftar kasus onboarding berhasil diambil.");
        }

        public async Task<RecruitmentResult<OnboardingCaseSummaryResponse>> GetSummaryAsync(
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var actor = await ResolveActorAsync(user, cancellationToken);

            var dbQuery = ApplyCaseReadScope(_dbContext.HrdOnboardingCases
                .Where(c => !c.IsDelete)
                .AsNoTracking(), actor);

            var total = await dbQuery.CountAsync(cancellationToken);
            var inProgress = await dbQuery.CountAsync(c => c.CaseStatus == OnboardingCaseStatus.InProgress, cancellationToken);
            var ready = await dbQuery.CountAsync(c => c.CaseStatus == OnboardingCaseStatus.Completed, cancellationToken);
            var cancelled = await dbQuery.CountAsync(c => c.CaseStatus == OnboardingCaseStatus.Cancelled, cancellationToken);

            return RecruitmentResult<OnboardingCaseSummaryResponse>.Ok(new OnboardingCaseSummaryResponse
            {
                TotalCases = total,
                InProgressCases = inProgress,
                ReadyForWorkCases = ready,
                CancelledCases = cancelled
            }, "Ringkasan metrik kasus onboarding berhasil diambil.");
        }

        public async Task<RecruitmentResult<OnboardingCaseDetailResponse>> GetByIdAsync(
            Guid id,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var actor = await ResolveActorAsync(user, cancellationToken);
            var c = await LoadFullCaseAsync(x => x.Id == id && !x.IsDelete, cancellationToken, asNoTracking: true);

            if (c == null)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.NotFound("Kasus onboarding tidak ditemukan.");
            }

            if (!actor.CanReadCase(c))
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Forbidden(
                    "Anda tidak memiliki wewenang untuk melihat kasus onboarding di luar unit kerja Anda.");
            }

            return RecruitmentResult<OnboardingCaseDetailResponse>.Ok(
                await MapToDetailResponseAsync(c, actor, cancellationToken),
                "Detail kasus onboarding berhasil diambil.");
        }

        public async Task<RecruitmentResult<List<OnboardingCoordinatorOptionDto>>> GetCoordinatorOptionsAsync(
            string? search,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var actor = await ResolveActorAsync(user, cancellationToken);
            if (!actor.CanReassignCoordinator && !actor.IsHrOperator && !actor.CanViewAllUnits)
            {
                return RecruitmentResult<List<OnboardingCoordinatorOptionDto>>.Forbidden(
                    "Hanya petugas onboarding berwenang yang dapat melihat daftar kandidat koordinator.");
            }

            var query = _dbContext.Users
                .Include(u => u.WorkforceProfile)
                    .ThenInclude(w => w!.PrimaryDepartment)
                .Include(u => u.WorkforceProfile)
                    .ThenInclude(w => w!.PrimaryPosition)
                .AsNoTracking()
                .Where(u => u.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(u =>
                    (u.DisplayName != null && u.DisplayName.ToLower().Contains(term)) ||
                    (u.UserName != null && u.UserName.ToLower().Contains(term)) ||
                    (u.WorkforceProfile != null && u.WorkforceProfile.ProfileCode.ToLower().Contains(term)));
            }

            var list = await query
                .OrderBy(u => u.DisplayName ?? u.UserName)
                .Take(30)
                .Select(u => new OnboardingCoordinatorOptionDto
                {
                    UserId = u.Id,
                    UserName = u.UserName ?? "-",
                    DisplayName = u.DisplayName ?? u.UserName ?? "-",
                    WorkforceProfileId = u.WorkforceProfileId,
                    ProfileCode = u.WorkforceProfile != null ? u.WorkforceProfile.ProfileCode : null,
                    DepartmentName = u.WorkforceProfile != null && u.WorkforceProfile.PrimaryDepartment != null
                        ? u.WorkforceProfile.PrimaryDepartment.DepartmentName
                        : null,
                    PositionName = u.WorkforceProfile != null && u.WorkforceProfile.PrimaryPosition != null
                        ? u.WorkforceProfile.PrimaryPosition.PositionName
                        : null
                })
                .ToListAsync(cancellationToken);

            return RecruitmentResult<List<OnboardingCoordinatorOptionDto>>.Ok(list);
        }

        public async Task<RecruitmentResult<OnboardingCaseDetailResponse>> RecordFirstDayAttendanceAsync(
            Guid caseId,
            RecordFirstDayAttendanceRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Unprocessable("Data kehadiran wajib dikirim.");
            }

            var actor = await ResolveActorAsync(user, cancellationToken);
            if (!actor.CanCompleteItem || !actor.IsHrOperator)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Forbidden(
                    "Pencatatan hadir hari pertama hanya berhak dilakukan oleh petugas HRD berwenang (VR-ONB-03).");
            }

            var c = await _dbContext.HrdOnboardingCases
                .Include(x => x.Items)
                .Include(x => x.AccessRequests)
                .FirstOrDefaultAsync(x => x.Id == caseId && !x.IsDelete, cancellationToken);

            if (c == null)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.NotFound("Kasus onboarding tidak ditemukan.");
            }

            if (request.RowVersion.HasValue && request.RowVersion.Value != Guid.Empty && request.RowVersion.Value != c.RowVersion)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Conflict(
                    "Data kasus onboarding telah berubah oleh pengguna lain. Muat ulang data lalu coba kembali.");
            }

            if (c.CaseStatus != OnboardingCaseStatus.InProgress)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Unprocessable(
                    $"Pencatatan hadir hanya dapat dilakukan saat kasus berstatus InProgress. Status saat ini: {c.CaseStatus}.");
            }

            if (c.FirstDayAttendedOn.HasValue)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Unprocessable(
                    "Kehadiran hari pertama sudah pernah dicatat untuk pegawai ini.");
            }

            if (request.AttendedDate < c.PlannedStartDate)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Unprocessable(
                    $"Tanggal kehadiran hari pertama ({request.AttendedDate:yyyy-MM-dd}) tidak boleh mendahului tanggal mulai rencana kerja ({c.PlannedStartDate:yyyy-MM-dd}) (VR-ONB-10).");
            }

            await using var tx = _dbContext.Database.IsRelational()
                ? await _dbContext.Database.BeginTransactionAsync(cancellationToken)
                : null;

            c.FirstDayAttendedOn = request.AttendedDate;
            c.FirstDayRecordedByUserId = actor.UserId;
            c.RowVersion = Guid.NewGuid();
            c.UpdateDateTime = DateTime.UtcNow;
            c.UpdateBy = actor.UserId;

            var firstDayItem = c.Items.FirstOrDefault(i => !i.IsDelete && i.SystemItemCode == EmploymentLifecycleValueConstants.ItemFirstDayAttendance);
            if (firstDayItem != null)
            {
                firstDayItem.ItemStatus = OnboardingItemStatus.Completed;
                firstDayItem.CompletedAt = DateTime.UtcNow;
                firstDayItem.CompletedByUserId = actor.UserId;
                firstDayItem.CompletionNote = $"Dicatat hadir hari pertama pada tanggal {request.AttendedDate:yyyy-MM-dd}.";
                firstDayItem.RowVersion = Guid.NewGuid();
                firstDayItem.UpdateDateTime = DateTime.UtcNow;
                firstDayItem.UpdateBy = actor.UserId;
            }

            var accessReq = c.AccessRequests.FirstOrDefault(a => !a.IsDelete && a.RequestType == OnboardingAccessRequestType.AccountCreation);
            if (accessReq != null && accessReq.ApplicationUserId.HasValue)
            {
                var activateRes = await _userAccountService.ActivateAsync(accessReq.ApplicationUserId.Value, actor.UserId, cancellationToken);
                if (activateRes.Success)
                {
                    accessReq.SelfServiceActivatedAt = DateTime.UtcNow;
                    accessReq.UpdateDateTime = DateTime.UtcNow;
                    accessReq.UpdateBy = actor.UserId;
                }
            }

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                if (tx != null) await tx.CommitAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (tx != null) await tx.RollbackAsync(cancellationToken);
                return RecruitmentResult<OnboardingCaseDetailResponse>.Conflict(
                    "Konflik pembaruan konkuren terdeteksi saat mencatat hadir hari pertama. Muat ulang halaman lalu coba kembali.");
            }

            return await GetByIdInternalAsync(c.Id, actor, cancellationToken);
        }

        public async Task<RecruitmentResult<OnboardingCaseDetailResponse>> CancelFirstDayAttendanceAsync(
            Guid caseId,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var actor = await ResolveActorAsync(user, cancellationToken);
            if (!actor.CanCompleteItem || !actor.IsHrOperator)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Forbidden(
                    "Pembatalan hadir hari pertama hanya berhak dilakukan oleh petugas HRD berwenang.");
            }

            var c = await _dbContext.HrdOnboardingCases
                .Include(x => x.Items)
                .Include(x => x.AccessRequests)
                .FirstOrDefaultAsync(x => x.Id == caseId && !x.IsDelete, cancellationToken);

            if (c == null)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.NotFound("Kasus onboarding tidak ditemukan.");
            }

            if (c.CaseStatus != OnboardingCaseStatus.InProgress)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Unprocessable(
                    $"Kehadiran hari pertama tidak dapat dibatalkan karena status kasus saat ini adalah {c.CaseStatus}.");
            }

            if (!c.FirstDayAttendedOn.HasValue)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Unprocessable(
                    "Belum ada catatan kehadiran hari pertama yang dapat dibatalkan.");
            }

            var handoff = await _dbContext.HrdCandidateHiringHandoffs
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.Id == c.HiringHandoffId && !h.IsDelete, cancellationToken);

            var firstContract = await ResolveHandoffContractAsync(c, handoff, cancellationToken);
            if (firstContract != null &&
                !string.Equals(firstContract.ContractStatus, EmploymentLifecycleValueConstants.ContractStatusDraft, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(firstContract.ContractStatus, "Cancelled", StringComparison.OrdinalIgnoreCase))
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Unprocessable(
                    "Kehadiran hari pertama tidak dapat dibatalkan karena kontrak kerja pertama kasus ini sudah disahkan (VR-ONB-17).");
            }

            await using var tx = _dbContext.Database.IsRelational()
                ? await _dbContext.Database.BeginTransactionAsync(cancellationToken)
                : null;

            c.FirstDayAttendedOn = null;
            c.FirstDayRecordedByUserId = null;
            c.RowVersion = Guid.NewGuid();
            c.UpdateDateTime = DateTime.UtcNow;
            c.UpdateBy = actor.UserId;

            var firstDayItem = c.Items.FirstOrDefault(i => !i.IsDelete && i.SystemItemCode == EmploymentLifecycleValueConstants.ItemFirstDayAttendance);
            if (firstDayItem != null)
            {
                firstDayItem.ItemStatus = OnboardingItemStatus.Pending;
                firstDayItem.CompletedAt = null;
                firstDayItem.CompletedByUserId = null;
                firstDayItem.CompletionNote = null;
                firstDayItem.RowVersion = Guid.NewGuid();
                firstDayItem.UpdateDateTime = DateTime.UtcNow;
                firstDayItem.UpdateBy = actor.UserId;
            }

            // Cabut aktivasi self-service apabila sebelumnya diaktifkan saat pencatatan hadir kasus ini
            var accountReq = c.AccessRequests.FirstOrDefault(a => !a.IsDelete && a.RequestType == OnboardingAccessRequestType.AccountCreation);
            if (accountReq != null && accountReq.SelfServiceActivatedAt.HasValue)
            {
                accountReq.SelfServiceActivatedAt = null;
                accountReq.UpdateDateTime = DateTime.UtcNow;
                accountReq.UpdateBy = actor.UserId;

                if (accountReq.ApplicationUserId.HasValue)
                {
                    var appUser = await _dbContext.Users
                        .FirstOrDefaultAsync(u => u.Id == accountReq.ApplicationUserId.Value, cancellationToken);
                    if (appUser != null && appUser.WorkforceProfileId == c.WorkforceProfileId)
                    {
                        appUser.IsActive = false;
                        appUser.UpdateDateTime = DateTime.UtcNow;
                    }
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            if (tx != null) await tx.CommitAsync(cancellationToken);

            return await GetByIdInternalAsync(c.Id, actor, cancellationToken);
        }

        public async Task<RecruitmentResult<OnboardingCaseDetailResponse>> CompleteItemAsync(
            Guid caseId,
            Guid itemId,
            CompleteChecklistItemRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            request ??= new CompleteChecklistItemRequest();
            var actor = await ResolveActorAsync(user, cancellationToken);
            if (!actor.CanCompleteItem)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Forbidden(
                    "Akun ini tidak memiliki izin menyelesaikan butir checklist onboarding.");
            }

            var c = await _dbContext.HrdOnboardingCases
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == caseId && !x.IsDelete, cancellationToken);

            if (c == null)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.NotFound("Kasus onboarding tidak ditemukan.");
            }

            if (c.CaseStatus == OnboardingCaseStatus.Cancelled)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Unprocessable(
                    "Butir checklist tidak dapat diubah karena kasus onboarding sudah dibatalkan (Cancelled).");
            }

            var item = c.Items.FirstOrDefault(i => i.Id == itemId && !i.IsDelete);
            if (item == null)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.NotFound("Butir checklist onboarding tidak ditemukan.");
            }

            if (c.CaseStatus == OnboardingCaseStatus.Completed && !OnboardingCasePolicy.IsPostReadyOptionalEditableItem(item))
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Unprocessable(
                    "Kasus sudah berstatus Siap Kerja. Hanya butir checklist opsional non-sistem yang masih boleh diselesaikan (HRD-DEC-088).");
            }

            if (item.ItemKind == OnboardingItemKind.Automatic)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Unprocessable(
                    "Butir otomatis dilarang dicentang tangan. Gunakan aksi Periksa Butir Otomatis (VR-ONB-06).");
            }

            if (item.SystemItemCode == EmploymentLifecycleValueConstants.ItemFirstDayAttendance)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Unprocessable(
                    "Kehadiran hari pertama wajib dicatat melalui aksi resmi Catat Hadir Hari Pertama.");
            }

            // Pemisahan peran ketat HRD vs Kepala Unit tanpa bypass (HRD-DEC-086, VR-ONB-03/04, ONB-G04)
            if (item.ResponsibleRole == OnboardingResponsibleRole.Hrd)
            {
                if (!actor.IsHrOperator)
                {
                    return RecruitmentResult<OnboardingCaseDetailResponse>.Forbidden(
                        "Butir ini menjadi tanggung jawab HRD dan hanya dapat dicentang oleh petugas HRD berwenang (VR-ONB-03).");
                }
            }
            else if (item.ResponsibleRole == OnboardingResponsibleRole.UnitHead)
            {
                if (!actor.IsUnitHeadFor(c.OrganizationUnitId))
                {
                    return RecruitmentResult<OnboardingCaseDetailResponse>.Forbidden(
                        "Butir ini menjadi tanggung jawab Kepala Unit tujuan dan hanya dapat dicentang oleh Kepala Unit terkait tanpa bypass HRD (VR-ONB-04).");
                }
            }

            // Enforce bukti wajib pada butir EvidenceRequired
            if (item.ItemKind == OnboardingItemKind.EvidenceRequired &&
                (request.EvidenceFile == null || request.EvidenceFile.Length == 0) &&
                string.IsNullOrWhiteSpace(item.EvidenceStoragePath))
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Unprocessable(
                    "Butir checklist wajib bukti harus disertai unggahan dokumen PDF/JPG/PNG (HRD-DEC-086).");
            }

            // Validasi penetapan atasan langsung & preceptor klinis
            if (item.SystemItemCode == EmploymentLifecycleValueConstants.ItemDirectSupervisorAssigned)
            {
                var (isValidSup, supErr) = await ValidateDirectSupervisorCandidateAsync(
                    c.WorkforceProfileId,
                    request.SelectedWorkforceProfileId,
                    cancellationToken);

                if (!isValidSup)
                {
                    return RecruitmentResult<OnboardingCaseDetailResponse>.Unprocessable(supErr!);
                }

                item.SelectedWorkforceProfileId = request.SelectedWorkforceProfileId!.Value;
            }
            else if (item.SystemItemCode == EmploymentLifecycleValueConstants.ItemPreceptorAssigned)
            {
                if (!request.SelectedWorkforceProfileId.HasValue || request.SelectedWorkforceProfileId.Value == Guid.Empty)
                {
                    return RecruitmentResult<OnboardingCaseDetailResponse>.Unprocessable(
                        "Penetapan preceptor klinis wajib memilih profil tenaga kesehatan preceptor (VR-ONB-08).");
                }

                var preceptorCheck = await ValidatePreceptorCandidateAsync(
                    c,
                    request.SelectedWorkforceProfileId.Value,
                    cancellationToken);
                if (!preceptorCheck.IsValid)
                {
                    return RecruitmentResult<OnboardingCaseDetailResponse>.Unprocessable(preceptorCheck.ErrorMessage!);
                }

                var preceptorshipResult = await _preceptorshipService.CreateForCaseAsync(
                    c.Id,
                    request.SelectedWorkforceProfileId.Value,
                    actor.UserId,
                    cancellationToken);

                if (!preceptorshipResult.Success)
                {
                    return RecruitmentResult<OnboardingCaseDetailResponse>.Unprocessable(preceptorshipResult.Message);
                }

                item.SelectedWorkforceProfileId = request.SelectedWorkforceProfileId.Value;
            }
            else if (item.ItemKind == OnboardingItemKind.PersonSelection)
            {
                if (!request.SelectedWorkforceProfileId.HasValue || request.SelectedWorkforceProfileId.Value == Guid.Empty)
                {
                    return RecruitmentResult<OnboardingCaseDetailResponse>.Unprocessable(
                        "Butir pemilihan personel wajib memilih profil pegawai.");
                }

                var personExists = await _dbContext.MstWorkforceProfiles
                    .AsNoTracking()
                    .AnyAsync(p => p.Id == request.SelectedWorkforceProfileId.Value && !p.IsDelete && p.IsActive, cancellationToken);
                if (!personExists)
                {
                    return RecruitmentResult<OnboardingCaseDetailResponse>.Unprocessable("Profil pegawai yang dipilih tidak ditemukan atau nonaktif.");
                }

                item.SelectedWorkforceProfileId = request.SelectedWorkforceProfileId.Value;
            }

            string? newlySavedStoragePath = null;
            if (request.EvidenceFile != null && request.EvidenceFile.Length > 0)
            {
                var saveResult = await _attachmentService.ValidateAndSaveAttachmentAsync(
                    request.EvidenceFile,
                    "onboarding-checklist-evidence",
                    cancellationToken);

                if (!saveResult.IsSuccess)
                {
                    return RecruitmentResult<OnboardingCaseDetailResponse>.Unprocessable(saveResult.ErrorMessage ?? "Gagal menyimpan berkas bukti.");
                }

                newlySavedStoragePath = saveResult.StoragePath;
                item.EvidenceStoragePath = saveResult.StoragePath;
                item.EvidenceFileName = saveResult.FileName;
                item.EvidenceContentType = saveResult.ContentType;
                item.EvidenceFileSize = saveResult.FileSize;
            }

            item.ItemStatus = OnboardingItemStatus.Completed;
            item.CompletedAt = DateTime.UtcNow;
            item.CompletedByUserId = actor.UserId;
            item.CompletionNote = request.Note?.Trim();
            item.RowVersion = Guid.NewGuid();
            item.UpdateDateTime = DateTime.UtcNow;
            item.UpdateBy = actor.UserId;

            c.RowVersion = Guid.NewGuid();
            c.UpdateDateTime = DateTime.UtcNow;
            c.UpdateBy = actor.UserId;

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (Exception)
            {
                if (!string.IsNullOrWhiteSpace(newlySavedStoragePath))
                {
                    await _attachmentService.DeleteAttachmentAsync(newlySavedStoragePath, cancellationToken);
                }
                throw;
            }

            return await GetByIdInternalAsync(c.Id, actor, cancellationToken);
        }

        public async Task<RecruitmentResult<OnboardingCaseDetailResponse>> UncompleteItemAsync(
            Guid caseId,
            Guid itemId,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var actor = await ResolveActorAsync(user, cancellationToken);
            if (!actor.CanCompleteItem)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Forbidden(
                    "Akun ini tidak memiliki izin membatalkan status butir checklist onboarding.");
            }

            var c = await _dbContext.HrdOnboardingCases
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == caseId && !x.IsDelete, cancellationToken);

            if (c == null)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.NotFound("Kasus onboarding tidak ditemukan.");
            }

            if (c.CaseStatus == OnboardingCaseStatus.Cancelled)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Unprocessable(
                    "Butir checklist tidak dapat dibatalkan karena kasus onboarding berstatus Cancelled.");
            }

            var item = c.Items.FirstOrDefault(i => i.Id == itemId && !i.IsDelete);
            if (item == null)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.NotFound("Butir checklist onboarding tidak ditemukan.");
            }

            if (c.CaseStatus == OnboardingCaseStatus.Completed && !OnboardingCasePolicy.IsPostReadyOptionalEditableItem(item))
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Unprocessable(
                    "Butir wajib atau sistem tidak dapat dibatalkan setelah pegawai dinyatakan Siap Kerja (HRD-DEC-088).");
            }

            if (item.ItemKind == OnboardingItemKind.Automatic)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Unprocessable(
                    "Butir otomatis dilarang dibatalkan tangan.");
            }

            if (item.SystemItemCode == EmploymentLifecycleValueConstants.ItemFirstDayAttendance)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Unprocessable(
                    "Pembatalan hadir hari pertama wajib dilakukan melalui aksi pembatalan kehadiran resmi.");
            }

            // Enforces ONB-G05: Role separation tetap berlaku saat membatalkan centang
            if (item.ResponsibleRole == OnboardingResponsibleRole.Hrd && !actor.IsHrOperator)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Forbidden(
                    "Hanya petugas HRD berwenang yang dapat membatalkan butir milik HRD (VR-ONB-03).");
            }
            if (item.ResponsibleRole == OnboardingResponsibleRole.UnitHead && !actor.IsUnitHeadFor(c.OrganizationUnitId))
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Forbidden(
                    "Hanya Kepala Unit tujuan yang dapat membatalkan butir milik Kepala Unit (VR-ONB-04).");
            }

            item.ItemStatus = OnboardingItemStatus.Pending;
            item.CompletedAt = null;
            item.CompletedByUserId = null;
            item.CompletionNote = null;
            item.RowVersion = Guid.NewGuid();
            item.UpdateDateTime = DateTime.UtcNow;
            item.UpdateBy = actor.UserId;

            c.RowVersion = Guid.NewGuid();
            c.UpdateDateTime = DateTime.UtcNow;
            c.UpdateBy = actor.UserId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetByIdInternalAsync(c.Id, actor, cancellationToken);
        }

        public async Task<RecruitmentResult<OnboardingCaseDetailResponse>> CheckAutomaticItemsAsync(
            Guid caseId,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var actor = await ResolveActorAsync(user, cancellationToken);
            if (!actor.CanCompleteItem)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Forbidden(
                    "Akun ini tidak memiliki izin memeriksa butir otomatis.");
            }

            var c = await _dbContext.HrdOnboardingCases
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == caseId && !x.IsDelete, cancellationToken);

            if (c == null)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.NotFound("Kasus onboarding tidak ditemukan.");
            }

            if (!actor.CanReadCase(c))
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Forbidden(
                    "Anda tidak memiliki wewenang pada kasus onboarding di luar unit kerja Anda.");
            }

            if (c.CaseStatus != OnboardingCaseStatus.InProgress)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Unprocessable(
                    $"Pemeriksaan ulang butir otomatis hanya diizinkan pada kasus berstatus InProgress. Status saat ini: {c.CaseStatus}.");
            }

            var handoff = await _dbContext.HrdCandidateHiringHandoffs
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.Id == c.HiringHandoffId && !h.IsDelete, cancellationToken);

            var now = DateTime.UtcNow;
            var today = OnboardingCasePolicy.OperationalToday().ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

            // 1. CONTRACT_SIGNED (wajib terhubung ke offer handoff + status Signed/Active + scan DocumentPath ada)
            var contractItem = c.Items.FirstOrDefault(i => !i.IsDelete && i.SystemItemCode == EmploymentLifecycleValueConstants.ItemContractSigned);
            if (contractItem != null)
            {
                var firstContract = await ResolveHandoffContractAsync(c, handoff, cancellationToken);
                var validSigned = firstContract != null
                    && (string.Equals(firstContract.ContractStatus, "Signed", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(firstContract.ContractStatus, EmploymentLifecycleValueConstants.ContractStatusActive, StringComparison.OrdinalIgnoreCase))
                    && !string.IsNullOrWhiteSpace(firstContract.DocumentPath);

                contractItem.AutomaticCheckedAt = now;
                contractItem.UpdateDateTime = now;
                contractItem.UpdateBy = actor.UserId;
                contractItem.RowVersion = Guid.NewGuid();

                if (validSigned)
                {
                    contractItem.ItemStatus = OnboardingItemStatus.Completed;
                    contractItem.CompletedAt ??= now;
                    contractItem.CompletedByUserId ??= actor.UserId;
                    contractItem.AutomaticResultNote = $"Kontrak {firstContract!.ContractNumber} ({firstContract.ContractStatus}) sah beserta pindaian bertanda tangan.";
                }
                else
                {
                    contractItem.ItemStatus = OnboardingItemStatus.Pending;
                    contractItem.CompletedAt = null;
                    contractItem.CompletedByUserId = null;
                    contractItem.AutomaticResultNote = firstContract == null
                        ? "Kontrak pertama belum ditemukan."
                        : $"Kontrak {firstContract.ContractNumber} berstatus {firstContract.ContractStatus}; pastikan scan bertanda tangan sudah diunggah dan disahkan.";
                }
            }

            // 2. FITNESS_CLEARANCE
            var fitItem = c.Items.FirstOrDefault(i => !i.IsDelete && i.SystemItemCode == EmploymentLifecycleValueConstants.ItemFitnessClearance);
            if (fitItem != null)
            {
                var mcu = handoff == null
                    ? null
                    : await _dbContext.HrdPreEmploymentMedicalChecks
                        .AsNoTracking()
                        .FirstOrDefaultAsync(m => m.OfferId == handoff.OfferId && !m.IsDelete, cancellationToken);

                var isFit = mcu != null && (mcu.ClearanceResult == McuClearanceResult.Fit || mcu.ClearanceResult == McuClearanceResult.FitWithRestrictions);
                fitItem.AutomaticCheckedAt = now;
                fitItem.UpdateDateTime = now;
                fitItem.UpdateBy = actor.UserId;
                fitItem.RowVersion = Guid.NewGuid();

                if (isFit)
                {
                    fitItem.ItemStatus = OnboardingItemStatus.Completed;
                    fitItem.CompletedAt ??= now;
                    fitItem.CompletedByUserId ??= actor.UserId;
                    fitItem.AutomaticResultNote = $"Kelaikan medis terverifikasi ({OnboardingCasePolicy.GetSafeMcuLabel(mcu?.ClearanceResult)}).";
                }
                else
                {
                    fitItem.ItemStatus = OnboardingItemStatus.Pending;
                    fitItem.CompletedAt = null;
                    fitItem.CompletedByUserId = null;
                    fitItem.AutomaticResultNote = $"Kelaikan medis belum memenuhi syarat ({OnboardingCasePolicy.GetSafeMcuLabel(mcu?.ClearanceResult)}).";
                }
            }

            // 3. Butir Sertifikasi Wajib (wajib terverifikasi + aktif + belum kedaluwarsa)
            var certItems = c.Items
                .Where(i => !i.IsDelete && i.ItemKind == OnboardingItemKind.Automatic && i.CertificationTypeId.HasValue)
                .ToList();

            foreach (var certItem in certItems)
            {
                var validCert = await _dbContext.Set<WfpCertification>()
                    .AsNoTracking()
                    .Where(w => w.WorkforceProfileId == c.WorkforceProfileId &&
                                w.CertificationTypeId == certItem.CertificationTypeId!.Value &&
                                !w.IsDelete &&
                                w.IsActive &&
                                w.IsVerified &&
                                (w.IsLifetime || !w.ExpiredDate.HasValue || w.ExpiredDate.Value.Date >= today.Date))
                    .OrderByDescending(w => w.CreateDateTime)
                    .FirstOrDefaultAsync(cancellationToken);

                certItem.AutomaticCheckedAt = now;
                certItem.UpdateDateTime = now;
                certItem.UpdateBy = actor.UserId;
                certItem.RowVersion = Guid.NewGuid();

                if (validCert != null)
                {
                    certItem.ItemStatus = OnboardingItemStatus.Completed;
                    certItem.CompletedAt ??= now;
                    certItem.CompletedByUserId ??= actor.UserId;
                    certItem.AutomaticResultNote = $"Sertifikasi terverifikasi dan berlaku ({validCert.CertificationName}).";
                }
                else
                {
                    certItem.ItemStatus = OnboardingItemStatus.Pending;
                    certItem.CompletedAt = null;
                    certItem.CompletedByUserId = null;
                    certItem.AutomaticResultNote = "Sertifikasi wajib belum terverifikasi atau sudah kedaluwarsa.";
                }
            }

            c.RowVersion = Guid.NewGuid();
            c.UpdateDateTime = now;
            c.UpdateBy = actor.UserId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetByIdInternalAsync(c.Id, actor, cancellationToken);
        }

        public async Task<RecruitmentResult<OnboardingCaseDetailResponse>> ReassignCoordinatorAsync(
            Guid caseId,
            ReassignCoordinatorRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            if (request == null || request.NewCoordinatorUserId == Guid.Empty)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Unprocessable("Koordinator pengganti wajib dipilih.");
            }

            var actor = await ResolveActorAsync(user, cancellationToken);
            if (!actor.CanReassignCoordinator && !actor.IsHrOperator)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Forbidden(
                    "Pengalihan koordinator kasus hanya dapat dilakukan oleh petugas HRD berwenang.");
            }

            var c = await _dbContext.HrdOnboardingCases
                .FirstOrDefaultAsync(x => x.Id == caseId && !x.IsDelete, cancellationToken);

            if (c == null)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.NotFound("Kasus onboarding tidak ditemukan.");
            }

            if (c.CaseStatus != OnboardingCaseStatus.InProgress)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Unprocessable(
                    $"Koordinator hanya dapat diubah pada kasus berstatus InProgress. Status saat ini: {c.CaseStatus}.");
            }

            var coordinatorExists = await _dbContext.Users
                .AsNoTracking()
                .AnyAsync(u => u.Id == request.NewCoordinatorUserId && u.IsActive, cancellationToken);

            if (!coordinatorExists)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Unprocessable(
                    "Akun koordinator tujuan tidak ditemukan atau berstatus nonaktif.");
            }

            c.CoordinatorUserId = request.NewCoordinatorUserId;
            c.RowVersion = Guid.NewGuid();
            c.UpdateDateTime = DateTime.UtcNow;
            c.UpdateBy = actor.UserId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetByIdInternalAsync(c.Id, actor, cancellationToken);
        }

        public async Task<RecruitmentResult<OnboardingCaseDetailResponse>> DeclareReadyForWorkAsync(
            Guid caseId,
            DeclareReadyForWorkRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var actor = await ResolveActorAsync(user, cancellationToken);
            if (!actor.CanDeclareReady && !actor.IsHrOperator)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Forbidden(
                    "Pernyataan Siap Kerja hanya berhak disahkan oleh petugas HRD berwenang.");
            }

            var c = await LoadFullCaseAsync(x => x.Id == caseId && !x.IsDelete, cancellationToken);
            if (c == null)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.NotFound("Kasus onboarding tidak ditemukan.");
            }

            if (request?.RowVersion.HasValue == true && request.RowVersion.Value != Guid.Empty && request.RowVersion.Value != c.RowVersion)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Conflict(
                    "Data kasus onboarding sudah diperbarui oleh pengguna lain. Muat ulang halaman sebelum menyatakan Siap Kerja.");
            }

            if (c.CaseStatus != OnboardingCaseStatus.InProgress)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Unprocessable(
                    $"Siap kerja hanya dapat dinyatakan saat kasus berstatus InProgress. Status saat ini: {c.CaseStatus}.");
            }

            var facts = await GatherLiveFactsAsync(c, cancellationToken);
            var readiness = OnboardingCasePolicy.EvaluateReadiness(c, actor.UserId, facts);

            if (readiness.FourEyesViolation)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Unprocessable(
                    readiness.FourEyesMessage ?? "Pernyataan Siap Kerja melanggar aturan empat mata (VR-ONB-12).");
            }

            if (!readiness.Eligible)
            {
                var firstBlocker = readiness.Blockers.FirstOrDefault();
                return RecruitmentResult<OnboardingCaseDetailResponse>.Unprocessable(
                    firstBlocker?.Message ?? "Pernyataan Siap Kerja ditolak karena syarat belum terpenuhi (VR-ONB-11).");
            }

            await using var tx = _dbContext.Database.IsRelational()
                ? await _dbContext.Database.BeginTransactionAsync(cancellationToken)
                : null;

            var now = DateTime.UtcNow;

            c.CaseStatus = OnboardingCaseStatus.Completed;
            c.ReadyForWorkAt = now;
            c.ReadyForWorkByUserId = actor.UserId;
            c.RowVersion = Guid.NewGuid();
            c.UpdateDateTime = now;
            c.UpdateBy = actor.UserId;

            if (c.WorkforceProfile != null)
            {
                c.WorkforceProfile.IsActive = true;
                c.WorkforceProfile.UpdateDateTime = now;
                c.WorkforceProfile.UpdateBy = actor.UserId;
            }

            var supervisorItem = c.Items.FirstOrDefault(i => !i.IsDelete && i.SystemItemCode == EmploymentLifecycleValueConstants.ItemDirectSupervisorAssigned);
            if (supervisorItem?.SelectedWorkforceProfileId != null)
            {
                var existingPrimary = await _dbContext.Set<WfpManagerAssignment>()
                    .FirstOrDefaultAsync(m =>
                        m.WorkforceProfileId == c.WorkforceProfileId &&
                        m.IsActive &&
                        !m.IsDelete &&
                        m.IsPrimaryManager, cancellationToken);

                if (existingPrimary == null)
                {
                    await _dbContext.Set<WfpManagerAssignment>().AddAsync(new WfpManagerAssignment
                    {
                        Id = Guid.NewGuid(),
                        WorkforceProfileId = c.WorkforceProfileId,
                        ManagerWorkforceProfileId = supervisorItem.SelectedWorkforceProfileId.Value,
                        OrganizationUnitId = c.OrganizationUnitId,
                        ManagerType = "Direct",
                        EffectiveStartDate = now,
                        IsPrimaryManager = true,
                        CanApproveRequests = true,
                        IsActive = true,
                        CreateBy = actor.UserId,
                        CreateDateTime = now
                    }, cancellationToken);
                }
                else if (existingPrimary.ManagerWorkforceProfileId != supervisorItem.SelectedWorkforceProfileId.Value)
                {
                    if (tx != null) await tx.RollbackAsync(cancellationToken);
                    return RecruitmentResult<OnboardingCaseDetailResponse>.Conflict(
                        "Terdapat konflik penugasan atasan langsung aktif yang berbeda pada profil pegawai.");
                }
            }

            if (c.IsClinical)
            {
                await _preceptorshipService.SetAccompaniedAsync(c.Id, cancellationToken);
            }

            var hasWorkAccess = await _dbContext.HrdOnboardingAccessRequests
                .AnyAsync(a =>
                    a.OnboardingCaseId == c.Id &&
                    !a.IsDelete &&
                    a.RequestType == OnboardingAccessRequestType.WorkAccess &&
                    a.RequestStatus != OnboardingAccessRequestStatus.Cancelled, cancellationToken);

            if (!hasWorkAccess)
            {
                await _dbContext.HrdOnboardingAccessRequests.AddAsync(new HrdOnboardingAccessRequest
                {
                    Id = Guid.NewGuid(),
                    OnboardingCaseId = c.Id,
                    WorkforceProfileId = c.WorkforceProfileId,
                    RequestType = OnboardingAccessRequestType.WorkAccess,
                    RequestStatus = OnboardingAccessRequestStatus.Requested,
                    RequestedAt = now,
                    CreateBy = actor.UserId,
                    CreateDateTime = now
                }, cancellationToken);
            }

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                if (tx != null) await tx.CommitAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (tx != null) await tx.RollbackAsync(cancellationToken);
                return RecruitmentResult<OnboardingCaseDetailResponse>.Conflict(
                    "Pernyataan Siap Kerja gagal karena konflik pembaruan paralel. Muat ulang kasus onboarding terlebih dahulu.");
            }

            return await GetByIdInternalAsync(c.Id, actor, cancellationToken);
        }

        public async Task<RecruitmentResult<OnboardingCaseDetailResponse>> CancelJoinAsync(
            Guid caseId,
            CancelOnboardingJoinRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.CancelNote))
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Unprocessable(
                    "Catatan alasan pembatalan bergabung wajib diisi (VR-ONB-13).");
            }

            var actor = await ResolveActorAsync(user, cancellationToken);
            if (!actor.CanCancelJoin && !actor.IsHrOperator)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Forbidden(
                    "Pembatalan bergabung hanya berhak diproses oleh petugas HRD berwenang.");
            }

            var c = await _dbContext.HrdOnboardingCases
                .Include(x => x.WorkforceProfile)
                .Include(x => x.HiringHandoff)
                .Include(x => x.AccessRequests)
                .FirstOrDefaultAsync(x => x.Id == caseId && !x.IsDelete, cancellationToken);

            if (c == null)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.NotFound("Kasus onboarding tidak ditemukan.");
            }

            if (request.RowVersion.HasValue && request.RowVersion.Value != Guid.Empty && request.RowVersion.Value != c.RowVersion)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Conflict(
                    "Data kasus onboarding telah berubah. Muat ulang halaman lalu coba kembali.");
            }

            if (c.CaseStatus != OnboardingCaseStatus.InProgress)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Unprocessable(
                    $"Pembatalan bergabung hanya dapat dilakukan pada kasus berstatus InProgress. Status saat ini: {c.CaseStatus}.");
            }

            if (c.FirstDayAttendedOn.HasValue)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Unprocessable(
                    "Pembatalan bergabung hanya dapat diproses sebelum pegawai hadir hari pertama. Pegawai yang sudah hadir harus melalui proses pemisahan resmi (VR-ONB-16).");
            }

            await using var tx = _dbContext.Database.IsRelational()
                ? await _dbContext.Database.BeginTransactionAsync(cancellationToken)
                : null;

            var now = DateTime.UtcNow;

            c.CaseStatus = OnboardingCaseStatus.Cancelled;
            c.CancelReason = request.CancelReason;
            c.CancelNote = request.CancelNote.Trim();
            c.CancelledAt = now;
            c.CancelledByUserId = actor.UserId;
            c.RowVersion = Guid.NewGuid();
            c.UpdateDateTime = now;
            c.UpdateBy = actor.UserId;

            if (c.WorkforceProfile != null)
            {
                c.WorkforceProfile.IsActive = false;
                c.WorkforceProfile.UpdateDateTime = now;
                c.WorkforceProfile.UpdateBy = actor.UserId;
            }

            var offerId = c.HiringHandoff?.OfferId;
            var draftContracts = await _dbContext.WfpContractHistories
                .Where(h => h.WorkforceProfileId == c.WorkforceProfileId &&
                            h.ContractStatus == EmploymentLifecycleValueConstants.ContractStatusDraft &&
                            !h.IsDelete &&
                            (!offerId.HasValue || (h.SourceDocumentType == EmploymentLifecycleValueConstants.SourceDocOffer && h.SourceDocumentId == offerId.Value)))
                .ToListAsync(cancellationToken);

            foreach (var ctr in draftContracts)
            {
                ctr.ContractStatus = "Cancelled";
                ctr.RowVersion = Guid.NewGuid();
                ctr.UpdateDateTime = now;
                ctr.UpdateBy = actor.UserId;
            }

            foreach (var req in c.AccessRequests.Where(a => !a.IsDelete && a.RequestStatus != OnboardingAccessRequestStatus.Completed))
            {
                req.RequestStatus = OnboardingAccessRequestStatus.Cancelled;
                req.UpdateDateTime = now;
                req.UpdateBy = actor.UserId;
            }

            if (c.HiringHandoff != null)
            {
                c.HiringHandoff.Status = HandoffStatus.Cancelled;
                c.HiringHandoff.Notes = $"{c.HiringHandoff.Notes} | Pembatalan: {request.CancelNote.Trim()}";
                c.HiringHandoff.RowVersion = Guid.NewGuid();
                c.HiringHandoff.UpdateDateTime = now;
                c.HiringHandoff.UpdateBy = actor.UserId;

                if (c.HiringHandoff.HeadcountRequestSeatId.HasValue)
                {
                    var seat = await _dbContext.HrdHeadcountRequestSeats
                        .FirstOrDefaultAsync(s => s.Id == c.HiringHandoff.HeadcountRequestSeatId.Value && !s.IsDelete, cancellationToken);

                    if (seat != null)
                    {
                        seat.SeatStatus = HeadcountSeatStatus.Open;
                        seat.FulfilledAt = null;
                        seat.LinkedAt = null;
                        seat.LinkedByUserId = null;
                        seat.UpdateDateTime = now;
                        seat.UpdateBy = actor.UserId;
                    }
                }
            }

            if (c.IsClinical)
            {
                await _preceptorshipService.CancelForCaseAsync(c.Id, cancellationToken);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            if (tx != null) await tx.CommitAsync(cancellationToken);

            return await GetByIdInternalAsync(c.Id, actor, cancellationToken);
        }

        public async Task<(bool Exists, Stream? Stream, string? ContentType, string? FileName)> DownloadItemEvidenceAsync(
            Guid caseId,
            Guid itemId,
            CancellationToken cancellationToken = default)
        {
            var item = await _dbContext.HrdOnboardingCaseItems
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == itemId && i.OnboardingCaseId == caseId && !i.IsDelete, cancellationToken);

            if (item == null || string.IsNullOrWhiteSpace(item.EvidenceStoragePath))
            {
                return (false, null, null, null);
            }

            return await _attachmentService.GetAttachmentStreamAsync(item.EvidenceStoragePath, cancellationToken);
        }

        public async Task<(bool Exists, Stream? Stream, string? ContentType, string? FileName, bool IsForbidden)> DownloadItemEvidenceScopedAsync(
            Guid caseId,
            Guid itemId,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var actor = await ResolveActorAsync(user, cancellationToken);
            var c = await _dbContext.HrdOnboardingCases
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == caseId && !x.IsDelete, cancellationToken);

            if (c == null)
            {
                return (false, null, null, null, false);
            }

            if (!actor.CanReadCase(c))
            {
                return (false, null, null, null, true);
            }

            var res = await DownloadItemEvidenceAsync(caseId, itemId, cancellationToken);
            return (res.Exists, res.Stream, res.ContentType, res.FileName, false);
        }

        // --- Private Helper Methods ---

        private static IQueryable<HrdOnboardingCase> ApplyCaseReadScope(IQueryable<HrdOnboardingCase> query, ActorCapabilities actor)
        {
            if (actor.CanViewAllUnits)
            {
                return query;
            }

            var managedUnits = actor.ManagedUnitIds.ToList();
            return query.Where(c =>
                (managedUnits.Count > 0 && managedUnits.Contains(c.OrganizationUnitId)) ||
                (actor.UserId != Guid.Empty && c.CoordinatorUserId == actor.UserId) ||
                (actor.WorkforceProfileId.HasValue && c.WorkforceProfileId == actor.WorkforceProfileId.Value));
        }

        private async Task<HrdOnboardingCase?> LoadFullCaseAsync(
            System.Linq.Expressions.Expression<Func<HrdOnboardingCase, bool>> predicate,
            CancellationToken cancellationToken,
            bool asNoTracking = false)
        {
            var query = _dbContext.HrdOnboardingCases
                .Include(x => x.WorkforceProfile)
                .Include(x => x.HospitalSite)
                .Include(x => x.OrganizationUnit)
                    .ThenInclude(u => u!.Department)
                .Include(x => x.Position)
                .Include(x => x.CoordinatorUser)
                .Include(x => x.HiringHandoff)
                    .ThenInclude(h => h!.Offer)
                        .ThenInclude(o => o!.Application)
                            .ThenInclude(a => a!.Candidate)
                .Include(x => x.HiringHandoff)
                    .ThenInclude(h => h!.Offer)
                        .ThenInclude(o => o!.Application)
                            .ThenInclude(a => a!.Vacancy)
                                .ThenInclude(v => v!.Requisition)
                .Include(x => x.Items)
                    .ThenInclude(i => i.CertificationType)
                .Include(x => x.Items)
                    .ThenInclude(i => i.CompletedByUser)
                .Include(x => x.Items)
                    .ThenInclude(i => i.SelectedWorkforceProfile)
                .Include(x => x.AccessRequests)
                .AsQueryable();

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync(predicate, cancellationToken);
        }

        private async Task<RecruitmentResult<OnboardingCaseDetailResponse>> GetByIdInternalAsync(
            Guid id,
            ActorCapabilities actor,
            CancellationToken cancellationToken)
        {
            var c = await LoadFullCaseAsync(x => x.Id == id && !x.IsDelete, cancellationToken, asNoTracking: true);
            if (c == null)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.NotFound("Kasus onboarding tidak ditemukan.");
            }

            return RecruitmentResult<OnboardingCaseDetailResponse>.Ok(
                await MapToDetailResponseAsync(c, actor, cancellationToken),
                "Detail kasus onboarding berhasil diambil.");
        }

        private async Task<OnboardingContractSnapshot?> ResolveHandoffContractAsync(
            HrdOnboardingCase c,
            HrdCandidateHiringHandoff? handoff,
            CancellationToken cancellationToken)
        {
            var offerId = handoff?.OfferId;
            var contracts = await _dbContext.WfpContractHistories
                .Include(h => h.ContractType)
                .AsNoTracking()
                .Where(h => h.WorkforceProfileId == c.WorkforceProfileId && !h.IsDelete)
                .OrderByDescending(h => h.CreateDateTime)
                .ToListAsync(cancellationToken);

            WfpContractHistory? matched = null;
            if (offerId.HasValue)
            {
                matched = contracts.FirstOrDefault(h =>
                    string.Equals(h.SourceDocumentType, EmploymentLifecycleValueConstants.SourceDocOffer, StringComparison.OrdinalIgnoreCase) &&
                    h.SourceDocumentId == offerId.Value);
            }

            matched ??= contracts.FirstOrDefault(h =>
                string.Equals(h.HistoryType, "Initial", StringComparison.OrdinalIgnoreCase) &&
                h.CreateDateTime >= c.CreateDateTime.AddMinutes(-10));

            if (matched == null) return null;

            return new OnboardingContractSnapshot(
                matched.Id,
                matched.ContractNumber,
                matched.ContractType?.ContractTypeName,
                matched.HistoryType,
                matched.ContractStatus,
                matched.StartDate,
                matched.EndDate,
                matched.ProbationEndDate,
                matched.ContractType?.IsProbationApplicable ?? false,
                matched.DocumentPath,
                matched.DocumentFileName,
                handoff?.Offer?.OfferedBaseSalary);
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateDirectSupervisorCandidateAsync(
            Guid targetWorkforceProfileId,
            Guid? selectedManagerProfileId,
            CancellationToken cancellationToken)
        {
            if (!selectedManagerProfileId.HasValue || selectedManagerProfileId.Value == Guid.Empty)
            {
                return (false, "Penetapan atasan langsung wajib memilih profil pegawai atasan.");
            }

            if (selectedManagerProfileId.Value == targetWorkforceProfileId)
            {
                return (false, "Atasan langsung tidak boleh diri pegawai itu sendiri.");
            }

            var managerExists = await _dbContext.MstWorkforceProfiles
                .AsNoTracking()
                .AnyAsync(w => w.Id == selectedManagerProfileId.Value && !w.IsDelete && w.IsActive, cancellationToken);

            if (!managerExists)
            {
                return (false, "Profil pegawai atasan langsung yang dipilih tidak ditemukan atau berstatus nonaktif.");
            }

            // Cek konflik atasan aktif saat ini
            var existingMgr = await _dbContext.Set<WfpManagerAssignment>()
                .AsNoTracking()
                .FirstOrDefaultAsync(m =>
                    m.WorkforceProfileId == targetWorkforceProfileId &&
                    m.IsPrimaryManager &&
                    m.IsActive &&
                    !m.IsDelete, cancellationToken);

            if (existingMgr != null && existingMgr.ManagerWorkforceProfileId != selectedManagerProfileId.Value)
            {
                return (false, "Profil pegawai sudah memiliki penugasan atasan langsung aktif yang berbeda (konflik WfpManagerAssignment).");
            }

            // Cek cycle reporting chain (maksimal 16 tingkat)
            var visited = new HashSet<Guid> { targetWorkforceProfileId };
            var cursor = selectedManagerProfileId.Value;
            for (var depth = 0; depth < 16; depth++)
            {
                if (!visited.Add(cursor))
                {
                    return (false, "Penetapan atasan langsung ditolak karena membentuk rantai pelaporan melingkar (cycle).");
                }

                var nextManager = await _dbContext.Set<WfpManagerAssignment>()
                    .AsNoTracking()
                    .Where(m => m.WorkforceProfileId == cursor && m.IsPrimaryManager && m.IsActive && !m.IsDelete)
                    .Select(m => (Guid?)m.ManagerWorkforceProfileId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (!nextManager.HasValue) break;
                cursor = nextManager.Value;
            }

            return (true, null);
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidatePreceptorCandidateAsync(
            HrdOnboardingCase c,
            Guid preceptorProfileId,
            CancellationToken cancellationToken)
        {
            if (preceptorProfileId == c.WorkforceProfileId)
            {
                return (false, "Preceptor tidak boleh diri pegawai itu sendiri (VR-ONB-08).");
            }

            var profile = await _dbContext.MstWorkforceProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == preceptorProfileId && !w.IsDelete, cancellationToken);

            if (profile == null || !profile.IsActive)
            {
                return (false, "Profil preceptor klinis tidak ditemukan atau masih berstatus nonaktif (VR-ONB-08).");
            }

            // Pastikan preceptor bukan pegawai yang onboarding-nya sendiri masih InProgress
            var preceptorStillOnboarding = await _dbContext.HrdOnboardingCases
                .AsNoTracking()
                .AnyAsync(oc => oc.WorkforceProfileId == preceptorProfileId &&
                                oc.CaseStatus == OnboardingCaseStatus.InProgress &&
                                !oc.IsDelete, cancellationToken);
            if (preceptorStillOnboarding)
            {
                return (false, "Preceptor yang dipilih masih dalam masa onboarding dan belum Siap Kerja (VR-ONB-08).");
            }

            return (true, null);
        }

        private async Task<OnboardingLiveFacts> GatherLiveFactsAsync(HrdOnboardingCase c, CancellationToken cancellationToken)
        {
            var handoff = c.HiringHandoff ?? await _dbContext.HrdCandidateHiringHandoffs
                .Include(h => h.Offer)
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.Id == c.HiringHandoffId && !h.IsDelete, cancellationToken);

            var firstContract = await ResolveHandoffContractAsync(c, handoff, cancellationToken);
            var hasValidSignedContract = firstContract != null
                && (string.Equals(firstContract.ContractStatus, "Signed", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(firstContract.ContractStatus, EmploymentLifecycleValueConstants.ContractStatusActive, StringComparison.OrdinalIgnoreCase))
                && !string.IsNullOrWhiteSpace(firstContract.DocumentPath);

            var mcu = handoff == null
                ? null
                : await _dbContext.HrdPreEmploymentMedicalChecks
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.OfferId == handoff.OfferId && !m.IsDelete, cancellationToken);
            var hasValidMcu = mcu != null && (mcu.ClearanceResult == McuClearanceResult.Fit || mcu.ClearanceResult == McuClearanceResult.FitWithRestrictions);

            var today = OnboardingCasePolicy.OperationalToday().ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var validCertTypes = await _dbContext.Set<WfpCertification>()
                .AsNoTracking()
                .Where(w => w.WorkforceProfileId == c.WorkforceProfileId &&
                            w.CertificationTypeId.HasValue &&
                            !w.IsDelete &&
                            w.IsActive &&
                            w.IsVerified &&
                            (w.IsLifetime || !w.ExpiredDate.HasValue || w.ExpiredDate.Value.Date >= today.Date))
                .Select(w => w.CertificationTypeId!.Value)
                .Distinct()
                .ToListAsync(cancellationToken);

            var activeManager = await _dbContext.Set<WfpManagerAssignment>()
                .Include(m => m.ManagerWorkforceProfile)
                .AsNoTracking()
                .FirstOrDefaultAsync(m =>
                    m.WorkforceProfileId == c.WorkforceProfileId &&
                    m.IsPrimaryManager &&
                    m.IsActive &&
                    !m.IsDelete, cancellationToken);

            var supervisorItem = c.Items.FirstOrDefault(i => !i.IsDelete && i.SystemItemCode == EmploymentLifecycleValueConstants.ItemDirectSupervisorAssigned);
            var directSupValid = false;
            string? directSupReason = null;
            var hasManagerConflict = false;
            string? managerConflictNote = null;

            if (supervisorItem == null || supervisorItem.ItemStatus != OnboardingItemStatus.Completed || !supervisorItem.SelectedWorkforceProfileId.HasValue)
            {
                directSupReason = "Atasan langsung belum ditetapkan pada checklist tahap Penempatan.";
            }
            else
            {
                var (supOk, supMsg) = await ValidateDirectSupervisorCandidateAsync(
                    c.WorkforceProfileId,
                    supervisorItem.SelectedWorkforceProfileId,
                    cancellationToken);
                directSupValid = supOk;
                directSupReason = supMsg;

                if (activeManager != null && activeManager.ManagerWorkforceProfileId != supervisorItem.SelectedWorkforceProfileId.Value)
                {
                    hasManagerConflict = true;
                    managerConflictNote = $"Terdapat penugasan atasan aktif sebelumnya ({activeManager.ManagerWorkforceProfile?.DisplayName ?? activeManager.ManagerWorkforceProfileId.ToString()}) yang berbeda dengan pilihan onboarding.";
                }
            }

            HrdClinicalPreceptorship? preceptorship = null;
            var preceptorValid = !c.IsClinical;
            string? preceptorReason = null;
            Guid? currentPreceptorProfileId = null;
            string? currentPreceptorName = null;

            if (c.IsClinical)
            {
                preceptorship = await _dbContext.HrdClinicalPreceptorships
                    .Include(p => p.Preceptors.Where(pr => !pr.IsDelete && !pr.AssignedUntil.HasValue))
                        .ThenInclude(pr => pr.PreceptorWorkforceProfile)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.OnboardingCaseId == c.Id && !p.IsDelete, cancellationToken);

                var currentPr = preceptorship?.Preceptors.FirstOrDefault();
                currentPreceptorProfileId = currentPr?.PreceptorWorkforceProfileId;
                currentPreceptorName = currentPr?.PreceptorWorkforceProfile?.DisplayName;

                var preceptorItem = c.Items.FirstOrDefault(i => !i.IsDelete && i.SystemItemCode == EmploymentLifecycleValueConstants.ItemPreceptorAssigned);
                if (preceptorItem == null || preceptorItem.ItemStatus != OnboardingItemStatus.Completed || !preceptorItem.SelectedWorkforceProfileId.HasValue)
                {
                    preceptorReason = "Butir wajib Penetapan Preceptor Klinis belum diselesaikan oleh Kepala Unit.";
                }
                else if (preceptorship == null || preceptorship.PreceptorshipStatus == PreceptorshipStatus.Cancelled || !currentPreceptorProfileId.HasValue)
                {
                    preceptorReason = "Rekam pendampingan klinis (Preceptorship) aktif tidak ditemukan untuk kasus ini.";
                }
                else
                {
                    var prCheck = await ValidatePreceptorCandidateAsync(c, currentPreceptorProfileId.Value, cancellationToken);
                    preceptorValid = prCheck.IsValid;
                    preceptorReason = prCheck.ErrorMessage;
                }
            }

            var hasCredentialLicense = await _dbContext.Set<WfpCredentialLicense>()
                .AsNoTracking()
                .AnyAsync(l => l.WorkforceProfileId == c.WorkforceProfileId && !l.IsDelete && l.IsActive, cancellationToken);

            return new OnboardingLiveFacts(
                firstContract,
                hasValidSignedContract,
                mcu?.ClearanceResult,
                hasValidMcu,
                new HashSet<Guid>(validCertTypes),
                directSupValid,
                directSupReason,
                hasManagerConflict,
                managerConflictNote,
                activeManager?.ManagerWorkforceProfileId,
                activeManager?.ManagerWorkforceProfile?.DisplayName,
                preceptorValid,
                preceptorReason,
                preceptorship?.Id,
                preceptorship?.PreceptorshipStatus,
                currentPreceptorProfileId,
                currentPreceptorName,
                preceptorship?.AccompaniedSince,
                preceptorship?.IndependentOn,
                hasCredentialLicense,
                0,
                0);
        }

        private async Task<OnboardingEmployeeDataSummaryDto> BuildEmployeeDataSummaryAsync(
            HrdOnboardingCase c,
            HrdCandidate? candidate,
            ActorCapabilities actor,
            CancellationToken cancellationToken)
        {
            var wfpId = c.WorkforceProfileId;
            var profile = c.WorkforceProfile;

            var employee = await _dbContext.MstEmployees
                .AsNoTracking()
                .Where(e => e.WorkforceProfileId == wfpId && !e.IsDelete)
                .Select(e => new { e.Id, e.EmployeeCode, e.IdentityNumber, e.WorkLocation })
                .FirstOrDefaultAsync(cancellationToken);

            var doctor = await _dbContext.MstDoctors
                .AsNoTracking()
                .Where(d => d.WorkforceProfileId == wfpId && !d.IsDelete)
                .Select(d => new { d.Id, d.DoctorCode, d.IdentityNumber, d.WorkLocation })
                .FirstOrDefaultAsync(cancellationToken);

            var addrCount = await _dbContext.Set<WfpAddress>().AsNoTracking().CountAsync(x => x.WorkforceProfileId == wfpId && !x.IsDelete, cancellationToken);
            var famCount = await _dbContext.Set<WfpFamilyMember>().AsNoTracking().CountAsync(x => x.WorkforceProfileId == wfpId && !x.IsDelete, cancellationToken);
            var depCount = await _dbContext.Set<WfpDependent>().AsNoTracking().CountAsync(x => x.WorkforceProfileId == wfpId && !x.IsDelete, cancellationToken);
            var emgCount = await _dbContext.Set<WfpEmergencyContact>().AsNoTracking().CountAsync(x => x.WorkforceProfileId == wfpId && !x.IsDelete, cancellationToken);
            var eduCount = await _dbContext.Set<WfpEducation>().AsNoTracking().CountAsync(x => x.WorkforceProfileId == wfpId && !x.IsDelete, cancellationToken);
            var empHistCount = await _dbContext.Set<WfpEmploymentHistory>().AsNoTracking().CountAsync(x => x.WorkforceProfileId == wfpId && !x.IsDelete, cancellationToken);
            var docCount = await _dbContext.Set<WfpDocument>().AsNoTracking().CountAsync(x => x.WorkforceProfileId == wfpId && !x.IsDelete, cancellationToken);
            var verifiedDocCount = await _dbContext.Set<WfpDocument>().AsNoTracking().CountAsync(x => x.WorkforceProfileId == wfpId && !x.IsDelete && x.IsVerified, cancellationToken);
            var bankInfo = await _dbContext.Set<WfpBankAccount>()
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == wfpId && !x.IsDelete && x.IsActive)
                .Select(x => new { x.BankName, x.AccountNumber, x.IsVerified })
                .ToListAsync(cancellationToken);
            var certCount = await _dbContext.Set<WfpCertification>().AsNoTracking().CountAsync(x => x.WorkforceProfileId == wfpId && !x.IsDelete && x.IsActive, cancellationToken);
            var verifiedCertCount = await _dbContext.Set<WfpCertification>().AsNoTracking().CountAsync(x => x.WorkforceProfileId == wfpId && !x.IsDelete && x.IsActive && x.IsVerified, cancellationToken);
            var credCount = await _dbContext.Set<WfpCredentialLicense>().AsNoTracking().CountAsync(x => x.WorkforceProfileId == wfpId && !x.IsDelete && x.IsActive, cancellationToken);

            var primaryWorkforceRoute = employee != null
                ? $"/hr/master-data/employee/{employee.Id}/workforce"
                : doctor != null
                    ? $"/hr/master-data/doctor/{doctor.Id}/workforce"
                    : $"/hr/workforce-core/employee-360/{wfpId}";

            string? ResourceRoute(string key) => employee != null
                ? $"/hr/master-data/employee/{employee.Id}/workforce/{key}"
                : doctor != null
                    ? $"/hr/master-data/doctor/{doctor.Id}/workforce/{key}"
                    : $"/hr/workforce-core/employee-360/{wfpId}";

            var rawNik = employee?.IdentityNumber ?? doctor?.IdentityNumber ?? candidate?.NationalIdNumber;
            var maskedNik = OnboardingCasePolicy.MaskIdentityNumber(rawNik);
            var maskedEmail = OnboardingCasePolicy.MaskEmail(profile?.Email ?? candidate?.Email);
            var maskedPhone = OnboardingCasePolicy.MaskPhone(profile?.PhoneNumber ?? candidate?.PhoneNumber);

            var primaryBank = bankInfo.FirstOrDefault();
            var bankSummaryText = !actor.CanReadBankAccount
                ? (bankInfo.Count > 0 ? $"{bankInfo.Count} rekening tercatat (disembunyikan)" : "Belum ada rekening (akses terbatas)")
                : bankInfo.Count > 0
                    ? $"{primaryBank!.BankName ?? "Bank"} {OnboardingCasePolicy.MaskBankAccount(primaryBank.AccountNumber)}"
                    : "Belum ada rekening payroll";

            var groups = new List<OnboardingEmployeeDataGroupDto>
            {
                new()
                {
                    Key = "identity",
                    Title = "Identitas & Kontak Inti",
                    Status = (employee != null || doctor != null) ? "Complete" : "NeedsCompletion",
                    OwnerLabel = "HRD Data Pegawai",
                    ItemCount = employee != null || doctor != null ? 1 : 0,
                    Summary = employee != null
                        ? $"Master Pegawai {employee.EmployeeCode} terhubung (NIK: {maskedNik})."
                        : doctor != null
                            ? $"Master Dokter {doctor.DoctorCode} terhubung (NIK: {maskedNik})."
                            : $"Profil tenaga kerja {profile?.ProfileCode ?? "-"} tersedia (NIK kandidat: {maskedNik}); rekam master pegawai/dokter belum dibuat.",
                    IsMasked = true,
                    TargetRoute = employee != null
                        ? $"/hr/master-data/employee/{employee.Id}"
                        : doctor != null
                            ? $"/hr/master-data/doctor/{doctor.Id}"
                            : "/hr/master-data/employee/create"
                },
                new()
                {
                    Key = "addresses",
                    Title = "Alamat Domisili & KTP",
                    Status = addrCount > 0 ? "Complete" : "NeedsCompletion",
                    OwnerLabel = "HRD / Pegawai",
                    ItemCount = addrCount,
                    Summary = addrCount > 0 ? $"{addrCount} alamat tercatat." : "Alamat KTP/domisili belum ditambahkan ke profil.",
                    TargetRoute = ResourceRoute("addresses")
                },
                new()
                {
                    Key = "familyMembers",
                    Title = "Keluarga & Tanggungan",
                    Status = (famCount + depCount) > 0 ? "Complete" : "NeedsCompletion",
                    OwnerLabel = "HRD / Pegawai",
                    ItemCount = famCount + depCount,
                    Summary = (famCount + depCount) > 0 ? $"{famCount} anggota keluarga, {depCount} tanggungan." : "Data keluarga / tanggungan pajak belum diisi.",
                    TargetRoute = ResourceRoute("familyMembers")
                },
                new()
                {
                    Key = "emergencyContacts",
                    Title = "Kontak Darurat",
                    Status = emgCount > 0 ? "Complete" : "NeedsCompletion",
                    OwnerLabel = "HRD / Pegawai",
                    ItemCount = emgCount,
                    Summary = emgCount > 0 ? $"{emgCount} kontak darurat tercatat." : "Belum ada kontak darurat.",
                    TargetRoute = ResourceRoute("emergencyContacts")
                },
                new()
                {
                    Key = "educations",
                    Title = "Riwayat Pendidikan",
                    Status = eduCount > 0 ? "Complete" : "NeedsCompletion",
                    OwnerLabel = "HRD / Pegawai",
                    ItemCount = eduCount,
                    Summary = eduCount > 0 ? $"{eduCount} riwayat pendidikan tercatat." : "Riwayat pendidikan belum dilengkapi.",
                    TargetRoute = ResourceRoute("educations")
                },
                new()
                {
                    Key = "employmentHistories",
                    Title = "Riwayat Pekerjaan Sebelumnya",
                    Status = empHistCount > 0 ? "Complete" : "NeedsCompletion",
                    OwnerLabel = "HRD / Pegawai",
                    ItemCount = empHistCount,
                    Summary = empHistCount > 0 ? $"{empHistCount} riwayat pengalaman kerja tercatat." : "Belum ada catatan riwayat pekerjaan sebelumnya.",
                    TargetRoute = ResourceRoute("employmentHistories")
                },
                new()
                {
                    Key = "documents",
                    Title = "Dokumen Personal",
                    Status = !actor.CanReadDocuments ? "AccessRestricted" : (docCount == 0 ? "NeedsCompletion" : verifiedDocCount > 0 ? "Complete" : "Unverified"),
                    OwnerLabel = "HRD Administrasi",
                    ItemCount = actor.CanReadDocuments ? docCount : 0,
                    VerifiedCount = actor.CanReadDocuments ? verifiedDocCount : 0,
                    Summary = !actor.CanReadDocuments
                        ? "Status dokumen dibatasi oleh hak akses WorkforceDocument."
                        : docCount > 0 ? $"{docCount} dokumen diunggah ({verifiedDocCount} terverifikasi)." : "Berkas KTP/KK/Ijazah belum diunggah.",
                    IsMasked = !actor.CanReadDocuments,
                    TargetRoute = ResourceRoute("documents")
                },
                new()
                {
                    Key = "bankAccounts",
                    Title = "Rekening Bank Payroll",
                    Status = !actor.CanReadBankAccount && bankInfo.Count == 0 ? "AccessRestricted" : bankInfo.Count > 0 ? "Complete" : "NeedsCompletion",
                    OwnerLabel = "HRD / Payroll",
                    ItemCount = bankInfo.Count,
                    VerifiedCount = bankInfo.Count(b => b.IsVerified),
                    Summary = bankSummaryText,
                    IsMasked = true,
                    TargetRoute = ResourceRoute("bankAccounts")
                },
                new()
                {
                    Key = "certifications",
                    Title = "Sertifikasi & Kredensial Klinis",
                    Status = !c.IsClinical && certCount == 0 && credCount == 0
                        ? "NotApplicable"
                        : (certCount > 0 && verifiedCertCount == certCount && (!c.IsClinical || credCount > 0))
                            ? "Complete"
                            : (certCount > 0 || credCount > 0) ? "Unverified" : "NeedsCompletion",
                    OwnerLabel = c.IsClinical ? "HRD / Komite Klinis" : "HRD",
                    ItemCount = certCount + credCount,
                    VerifiedCount = verifiedCertCount,
                    Summary = c.IsClinical
                        ? $"{certCount} sertifikasi ({verifiedCertCount} terverifikasi), {credCount} kredensial klinis (STR/SIP/SPK)."
                        : $"{certCount} sertifikasi kompetensi tercatat.",
                    TargetRoute = ResourceRoute("certifications")
                }
            };

            var applicable = groups.Count(g => g.Status != "NotApplicable");
            var completed = groups.Count(g => g.Status == "Complete");

            return new OnboardingEmployeeDataSummaryDto
            {
                WorkforceProfileId = wfpId,
                ProfileCode = profile?.ProfileCode ?? "-",
                UserTypeName = profile?.UserType.ToString() ?? "Employee",
                IsProfileActive = profile?.IsActive ?? false,
                EmployeeId = employee?.Id,
                EmployeeCode = employee?.EmployeeCode,
                DoctorId = doctor?.Id,
                DoctorCode = doctor?.DoctorCode,
                MaskedIdentityNumber = maskedNik,
                MaskedEmail = maskedEmail,
                MaskedPhoneNumber = maskedPhone,
                PrimaryWorkforceRoute = primaryWorkforceRoute,
                Employee360Route = $"/hr/workforce-core/employee-360/{wfpId}",
                CompletedGroupCount = completed,
                ApplicableGroupCount = applicable,
                Groups = groups
            };
        }

        private async Task<OnboardingCaseDetailResponse> MapToDetailResponseAsync(
            HrdOnboardingCase c,
            ActorCapabilities actor,
            CancellationToken cancellationToken)
        {
            var handoff = c.HiringHandoff;
            var offer = handoff?.Offer;
            var app = offer?.Application;
            var candidate = app?.Candidate;
            var vacancy = app?.Vacancy;
            var requisition = vacancy?.Requisition;

            var facts = await GatherLiveFactsAsync(c, cancellationToken);
            var progress = OnboardingCasePolicy.ComputeProgress(c);
            var readiness = OnboardingCasePolicy.EvaluateReadiness(c, actor.UserId, facts);
            var employeeData = await BuildEmployeeDataSummaryAsync(c, candidate, actor, cancellationToken);

            var acceptanceSummary = new OnboardingAcceptanceSummaryDto
            {
                HandoffId = c.HiringHandoffId,
                HandoffNumber = handoff?.HandoffNumber ?? "-",
                HandoffDate = handoff?.HandoffDate,
                HandoffStatus = handoff?.Status.ToString() ?? "-",
                OfferId = offer?.Id,
                OfferNumber = offer?.OfferNumber,
                OfferStatus = offer?.Status.ToString(),
                CandidateId = candidate?.Id,
                CandidateNumber = candidate?.CandidateNumber,
                VacancyTitle = vacancy?.Title,
                RequisitionNumber = requisition?.RequisitionNumber,
                IsRehire = candidate?.IsRehire ?? false,
                PreviousWorkforceProfileId = candidate?.PreviousWorkforceProfileId,
                McuClearanceStatus = facts.McuResult?.ToString() ?? "Missing",
                McuClearanceLabel = OnboardingCasePolicy.GetSafeMcuLabel(facts.McuResult),
                HasMedicalRestrictions = facts.McuResult == McuClearanceResult.FitWithRestrictions,
                CandidateRoute = candidate != null ? $"/hr/recruitment/candidates/{candidate.Id}" : null
            };

            var contractSummary = new OnboardingContractSummaryDto
            {
                ContractId = facts.FirstContract?.ContractId,
                ContractNumber = facts.FirstContract?.ContractNumber,
                ContractTypeName = facts.FirstContract?.ContractTypeName,
                HistoryType = facts.FirstContract?.HistoryType,
                ContractStatus = facts.FirstContract?.ContractStatus,
                StartDate = facts.FirstContract?.StartDate,
                EndDate = facts.FirstContract?.EndDate,
                ProbationEndDate = facts.FirstContract?.ProbationEndDate,
                IsProbationApplicable = facts.FirstContract?.IsProbationApplicable ?? false,
                HasSignedDocument = !string.IsNullOrWhiteSpace(facts.FirstContract?.DocumentPath),
                DocumentFileName = facts.FirstContract?.DocumentFileName,
                IsContractValidForOnboarding = facts.HasValidSignedContract,
                CanViewAmount = actor.CanViewContractAmount,
                SalaryDisplay = OnboardingCasePolicy.FormatSalaryOrMask(facts.FirstContract?.BaseSalaryAmount, actor.CanViewContractAmount),
                BaseSalaryAmount = actor.CanViewContractAmount ? facts.FirstContract?.BaseSalaryAmount : null,
                ContractRoute = facts.FirstContract != null ? $"/hr/lifecycle/employment-contracts/{facts.FirstContract.ContractId}" : "/hr/lifecycle/employment-contracts"
            };

            var hasOrgAssign = await _dbContext.Set<WfpOrganizationAssignment>()
                .AsNoTracking()
                .AnyAsync(o => o.WorkforceProfileId == c.WorkforceProfileId && o.IsActive && !o.IsDelete, cancellationToken);
            var hasPosAssign = await _dbContext.Set<WfpPositionAssignment>()
                .AsNoTracking()
                .AnyAsync(p => p.WorkforceProfileId == c.WorkforceProfileId && p.IsActive && !p.IsDelete, cancellationToken);
            var jobDesc = await _dbContext.MstJobDescriptions
                .AsNoTracking()
                .Where(j => j.PositionId == c.PositionId && !j.IsDelete && j.IsActive && j.Status == "Active")
                .OrderByDescending(j => j.VersionNumber)
                .Select(j => new { j.Id, j.JobDescriptionCode, j.Purpose })
                .FirstOrDefaultAsync(cancellationToken);

            var supItem = c.Items.FirstOrDefault(i => !i.IsDelete && i.SystemItemCode == EmploymentLifecycleValueConstants.ItemDirectSupervisorAssigned);
            var placementSummary = new OnboardingPlacementSummaryDto
            {
                HospitalSiteId = c.HospitalSiteId,
                HospitalSiteName = c.HospitalSite?.SiteName ?? "-",
                OrganizationUnitId = c.OrganizationUnitId,
                OrganizationUnitName = c.OrganizationUnit?.UnitName ?? "-",
                DepartmentName = c.OrganizationUnit?.Department?.DepartmentName,
                PositionId = c.PositionId,
                PositionTitle = c.Position?.PositionName ?? "-",
                WorkLocation = requisition?.WorkLocation ?? c.HospitalSite?.SiteName ?? "-",
                HasActiveOrganizationAssignment = hasOrgAssign,
                HasActivePositionAssignment = hasPosAssign,
                JobDescriptionId = jobDesc?.Id,
                JobDescriptionCode = jobDesc?.JobDescriptionCode,
                JobDescriptionPurpose = jobDesc?.Purpose,
                DirectSupervisorProfileId = supItem?.SelectedWorkforceProfileId ?? facts.ActiveManagerProfileId,
                DirectSupervisorName = supItem?.SelectedWorkforceProfile?.DisplayName ?? facts.ActiveManagerName,
                HasActiveManagerAssignment = facts.ActiveManagerProfileId.HasValue,
                ActiveManagerName = facts.ActiveManagerName,
                HasManagerConflict = facts.HasManagerConflict,
                ManagerConflictNote = facts.ManagerConflictNote
            };

            OnboardingPreceptorshipSummaryDto? preceptorshipSummary = null;
            if (c.IsClinical)
            {
                preceptorshipSummary = new OnboardingPreceptorshipSummaryDto
                {
                    PreceptorshipId = facts.PreceptorshipId,
                    PreceptorshipStatus = facts.PreceptorshipStatusValue?.ToString() ?? "NotAssigned",
                    CurrentPreceptorProfileId = facts.CurrentPreceptorProfileId,
                    CurrentPreceptorName = facts.CurrentPreceptorName,
                    AccompaniedSince = facts.AccompaniedSince,
                    IndependentOn = facts.IndependentOn,
                    HasCredentialLicense = facts.HasCredentialLicense,
                    CredentialWarning = facts.HasCredentialLicense ? null : "Kewenangan klinis (SPK/RKK) belum tercatat pada profil pegawai.",
                    PreceptorshipRoute = facts.PreceptorshipId.HasValue ? $"/hr/lifecycle/clinical-preceptorships/{facts.PreceptorshipId.Value}" : null
                };
            }

            var actorPerms = new OnboardingActorPermissionsDto
            {
                CanViewAllUnits = actor.CanViewAllUnits,
                IsHrOperator = actor.IsHrOperator,
                IsUnitHeadForCase = actor.IsUnitHeadFor(c.OrganizationUnitId),
                CanRecordAttendance = actor.CanCompleteItem && actor.IsHrOperator,
                CanCancelAttendance = actor.CanCompleteItem && actor.IsHrOperator,
                CanCompleteHrItems = actor.CanCompleteItem && actor.IsHrOperator,
                CanCompleteUnitItems = actor.CanCompleteItem && actor.IsUnitHeadFor(c.OrganizationUnitId),
                CanCheckAutomaticItems = actor.CanCompleteItem && (actor.IsHrOperator || actor.IsUnitHeadFor(c.OrganizationUnitId)),
                CanReassignCoordinator = actor.CanReassignCoordinator && actor.IsHrOperator,
                CanDeclareReady = actor.CanDeclareReady && actor.IsHrOperator,
                CanCancelJoin = actor.CanCancelJoin && actor.IsHrOperator,
                CanViewContractAmount = actor.CanViewContractAmount,
                CanRetryAccountCreation = actor.CanRetryAccountCreation,
                CanCompleteWorkAccess = actor.CanCompleteWorkAccess,
                CanAssignPreceptor = actor.CanAssignPreceptor || actor.IsUnitHeadFor(c.OrganizationUnitId)
            };

            var stages = OnboardingCasePolicy.BuildStages(
                c, progress, readiness, acceptanceSummary, employeeData, contractSummary, placementSummary, preceptorshipSummary, actorPerms);

            var availableActions = new List<string>();
            if (c.CaseStatus == OnboardingCaseStatus.InProgress)
            {
                if (!c.FirstDayAttendedOn.HasValue)
                {
                    if (actorPerms.CanRecordAttendance) availableActions.Add("RecordFirstDayAttendance");
                    if (actorPerms.CanCancelJoin) availableActions.Add("CancelJoin");
                }
                else
                {
                    if (actorPerms.CanCancelAttendance && !facts.HasValidSignedContract) availableActions.Add("CancelFirstDayAttendance");
                }

                if (actorPerms.CanCheckAutomaticItems) availableActions.Add("CheckAutomaticItems");
                if (actorPerms.CanReassignCoordinator) availableActions.Add("ReassignCoordinator");
                if (actorPerms.CanDeclareReady && readiness.Eligible) availableActions.Add("DeclareReady");
            }

            var itemResponses = c.Items
                .Where(i => !i.IsDelete)
                .OrderBy(i => i.DueDate)
                .ThenBy(i => i.ItemCode)
                .Select(i =>
                {
                    var roleMatch = i.ResponsibleRole == OnboardingResponsibleRole.Hrd
                        ? actorPerms.CanCompleteHrItems
                        : actorPerms.CanCompleteUnitItems;

                    var editableState = c.CaseStatus == OnboardingCaseStatus.InProgress
                        || (c.CaseStatus == OnboardingCaseStatus.Completed && OnboardingCasePolicy.IsPostReadyOptionalEditableItem(i));

                    var isManualSystemAttendance = i.SystemItemCode == EmploymentLifecycleValueConstants.ItemFirstDayAttendance;
                    var canCompleteItem = editableState && roleMatch && i.ItemKind != OnboardingItemKind.Automatic && !isManualSystemAttendance && i.ItemStatus != OnboardingItemStatus.Completed;
                    var canUncompleteItem = editableState && roleMatch && i.ItemKind != OnboardingItemKind.Automatic && !isManualSystemAttendance && i.ItemStatus == OnboardingItemStatus.Completed;

                    return new OnboardingCaseItemResponse
                    {
                        Id = i.Id,
                        OnboardingCaseId = i.OnboardingCaseId,
                        ItemCode = i.ItemCode,
                        ItemName = i.ItemName,
                        TemplateLayer = i.TemplateLayer,
                        TaskCategory = i.TaskCategory,
                        Description = i.Description,
                        SystemItemCode = i.SystemItemCode,
                        ResponsibleRole = i.ResponsibleRole,
                        ItemKind = i.ItemKind,
                        AutomaticSource = i.AutomaticSource,
                        CertificationTypeId = i.CertificationTypeId,
                        CertificationTypeName = i.CertificationType?.CertificationTypeName,
                        IsRequired = i.IsRequired,
                        DueDayOffset = i.DueDayOffset,
                        DueDate = i.DueDate,
                        ItemStatus = i.ItemStatus,
                        CompletedAt = i.CompletedAt,
                        CompletedByUserId = i.CompletedByUserId,
                        CompletedByUserName = i.CompletedByUser?.DisplayName ?? i.CompletedByUser?.UserName,
                        CompletionNote = i.CompletionNote,
                        SelectedWorkforceProfileId = i.SelectedWorkforceProfileId,
                        SelectedWorkforceFullName = i.SelectedWorkforceProfile?.DisplayName,
                        EvidenceStoragePath = i.EvidenceStoragePath,
                        EvidenceFileName = i.EvidenceFileName,
                        AutomaticCheckedAt = i.AutomaticCheckedAt,
                        AutomaticResultNote = i.AutomaticResultNote,
                        CanComplete = canCompleteItem,
                        CanUncomplete = canUncompleteItem,
                        IsBlockedByRole = !roleMatch && i.ItemKind != OnboardingItemKind.Automatic && !isManualSystemAttendance,
                        RowVersion = i.RowVersion
                    };
                }).ToList();

            return new OnboardingCaseDetailResponse
            {
                Id = c.Id,
                CaseNumber = c.CaseNumber,
                HiringHandoffId = c.HiringHandoffId,
                WorkforceProfileId = c.WorkforceProfileId,
                WorkforceCode = c.WorkforceProfile?.ProfileCode ?? "-",
                FullName = c.WorkforceProfile?.DisplayName ?? "-",
                HospitalSiteId = c.HospitalSiteId,
                HospitalSiteName = c.HospitalSite?.SiteName ?? "-",
                OrganizationUnitId = c.OrganizationUnitId,
                OrganizationUnitName = c.OrganizationUnit?.UnitName ?? "-",
                PositionId = c.PositionId,
                PositionTitle = c.Position?.PositionName ?? "-",
                IsClinical = c.IsClinical,
                PlannedStartDate = c.PlannedStartDate,
                CoordinatorUserId = c.CoordinatorUserId,
                CoordinatorUserName = c.CoordinatorUser?.DisplayName ?? c.CoordinatorUser?.UserName ?? "-",
                CaseStatus = c.CaseStatus,
                FirstDayAttendedOn = c.FirstDayAttendedOn,
                ReadyForWorkAt = c.ReadyForWorkAt,
                CancelReason = c.CancelReason,
                CancelNote = c.CancelNote,
                CancelledAt = c.CancelledAt,
                HasGeneralTemplate = c.HasGeneralTemplate,
                AvailableActions = availableActions,
                RowVersion = c.RowVersion,
                Items = itemResponses,
                AccessRequests = c.AccessRequests
                    .Where(a => !a.IsDelete)
                    .OrderByDescending(a => a.RequestedAt)
                    .Select(a => new OnboardingAccessRequestResponse
                    {
                        Id = a.Id,
                        RequestType = a.RequestType,
                        RequestStatus = a.RequestStatus,
                        ApplicationUserId = a.ApplicationUserId,
                        FailureMessage = a.FailureMessage,
                        SelfServiceActivatedAt = a.SelfServiceActivatedAt,
                        RequestedAt = a.RequestedAt,
                        CompletedAt = a.CompletedAt
                    }).ToList(),
                Progress = progress,
                Readiness = readiness,
                Stages = stages,
                AcceptanceSummary = acceptanceSummary,
                EmployeeDataSummary = employeeData,
                ContractSummary = contractSummary,
                PlacementSummary = placementSummary,
                PreceptorshipSummary = preceptorshipSummary,
                ActorContext = actorPerms
            };
        }
    }
}
