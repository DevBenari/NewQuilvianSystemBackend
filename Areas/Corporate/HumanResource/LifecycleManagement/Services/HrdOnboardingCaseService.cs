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
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

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

        public HrdOnboardingCaseService(
            ApplicationDbContext dbContext,
            IHrdNumberSeriesService numberSeriesService,
            IHrdEmploymentContractService contractService,
            IEmployeeUserAccountService userAccountService,
            IHrdLifecycleAttachmentService attachmentService,
            IHrdClinicalPreceptorshipService preceptorshipService)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _numberSeriesService = numberSeriesService ?? throw new ArgumentNullException(nameof(numberSeriesService));
            _contractService = contractService ?? throw new ArgumentNullException(nameof(contractService));
            _userAccountService = userAccountService ?? throw new ArgumentNullException(nameof(userAccountService));
            _attachmentService = attachmentService ?? throw new ArgumentNullException(nameof(attachmentService));
            _preceptorshipService = preceptorshipService ?? throw new ArgumentNullException(nameof(preceptorshipService));
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

        public async Task<RecruitmentResult<OnboardingCaseDetailResponse>> CreateFromHandoffAsync(
            Guid handoffId,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var existingCase = await _dbContext.HrdOnboardingCases
                .Include(c => c.WorkforceProfile)
                .Include(c => c.HospitalSite)
                .Include(c => c.OrganizationUnit)
                .Include(c => c.Position)
                .Include(c => c.CoordinatorUser)
                .Include(c => c.Items)
                    .ThenInclude(i => i.CertificationType)
                .Include(c => c.Items)
                    .ThenInclude(i => i.CompletedByUser)
                .Include(c => c.Items)
                    .ThenInclude(i => i.SelectedWorkforceProfile)
                .Include(c => c.AccessRequests)
                .FirstOrDefaultAsync(c => c.HiringHandoffId == handoffId && !c.IsDelete, cancellationToken);

            if (existingCase != null)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Ok(
                    MapToDetailResponse(existingCase),
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
                    item.AutomaticResultNote = $"Kelaikan medis terverifikasi ({mcu?.ClearanceResult}).";
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
                if (registeredCodes.Add(sysCode))
                {
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
                        fallbackItem.AutomaticResultNote = $"Kelaikan medis terverifikasi ({mcu?.ClearanceResult}).";
                    }

                    items.Add(fallbackItem);
                }
            }

            EnsureSystemItem(EmploymentLifecycleValueConstants.ItemFirstDayAttendance, "Pencatatan Kehadiran Hari Pertama", OnboardingTemplateLayer.General, OnboardingResponsibleRole.Hrd, OnboardingItemKind.Manual, null, 0);
            EnsureSystemItem(EmploymentLifecycleValueConstants.ItemContractSigned, "Penandatanganan Kontrak Kerja", OnboardingTemplateLayer.General, OnboardingResponsibleRole.Hrd, OnboardingItemKind.Automatic, OnboardingAutomaticSource.ContractSigned, 0);
            EnsureSystemItem(EmploymentLifecycleValueConstants.ItemFitnessClearance, "Pemeriksaan Kelaikan Medis / MCU Pra-Kerja", OnboardingTemplateLayer.General, OnboardingResponsibleRole.Hrd, OnboardingItemKind.Automatic, OnboardingAutomaticSource.FitnessClearance, 0);
            EnsureSystemItem(EmploymentLifecycleValueConstants.ItemDirectSupervisorAssigned, "Penetapan Atasan Langsung", OnboardingTemplateLayer.Unit, OnboardingResponsibleRole.UnitHead, OnboardingItemKind.Manual, null, 7);

            if (isClinical)
            {
                EnsureSystemItem(EmploymentLifecycleValueConstants.ItemPreceptorAssigned, "Penetapan Preceptor / Pendamping Klinis", OnboardingTemplateLayer.Clinical, OnboardingResponsibleRole.UnitHead, OnboardingItemKind.Manual, null, 7);
            }

            onboardingCase.Items = items;

            // 6. Draf kontrak pertama (BE-HRD-072)
            await _contractService.CreateDraftFromOfferAsync(profile.Id, offer, actorUserId, cancellationToken);

            // 7. Permintaan akun pengguna mandiri (BE-HRD-066 / VR-ONB-75)
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
                    accessRequest.FailureMessage = accountResult.Message;
                }
            }
            catch (Exception ex)
            {
                accessRequest.RequestStatus = OnboardingAccessRequestStatus.Failed;
                accessRequest.FailureMessage = $"Pengecualian saat pembuatan akun: {ex.Message}";
            }

            onboardingCase.AccessRequests.Add(accessRequest);

            await _dbContext.HrdOnboardingCases.AddAsync(onboardingCase, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return await GetByIdInternalAsync(onboardingCase.Id, cancellationToken);
        }

        public async Task<RecruitmentResult<PagedResult<OnboardingCaseListItemResponse>>> GetPagedAsync(
            OnboardingCaseQuery query,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var dbQuery = _dbContext.HrdOnboardingCases
                .Include(c => c.WorkforceProfile)
                .Include(c => c.OrganizationUnit)
                .Include(c => c.Position)
                .Include(c => c.Items)
                .Where(c => !c.IsDelete)
                .AsQueryable();

            if (!IsHrOrAdmin(user))
            {
                var userUnitId = GetUserUnitId(user);
                if (userUnitId.HasValue)
                {
                    dbQuery = dbQuery.Where(c => c.OrganizationUnitId == userUnitId.Value);
                }
            }

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
            var pageSize = query.PageSize > 0 ? query.PageSize : 10;

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

            var result = new PagedResult<OnboardingCaseListItemResponse>
            {
                Items = items,
                TotalData = totalCount,
                TotalPage = totalPages,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return RecruitmentResult<PagedResult<OnboardingCaseListItemResponse>>.Ok(result, "Daftar kasus onboarding berhasil diambil.");
        }

        public async Task<RecruitmentResult<OnboardingCaseSummaryResponse>> GetSummaryAsync(
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var dbQuery = _dbContext.HrdOnboardingCases
                .Where(c => !c.IsDelete)
                .AsQueryable();

            if (!IsHrOrAdmin(user))
            {
                var userUnitId = GetUserUnitId(user);
                if (userUnitId.HasValue)
                {
                    dbQuery = dbQuery.Where(c => c.OrganizationUnitId == userUnitId.Value);
                }
            }

            var total = await dbQuery.CountAsync(cancellationToken);
            var inProgress = await dbQuery.CountAsync(c => c.CaseStatus == OnboardingCaseStatus.InProgress, cancellationToken);
            var ready = await dbQuery.CountAsync(c => c.CaseStatus == OnboardingCaseStatus.Completed, cancellationToken);
            var cancelled = await dbQuery.CountAsync(c => c.CaseStatus == OnboardingCaseStatus.Cancelled, cancellationToken);

            var summary = new OnboardingCaseSummaryResponse
            {
                TotalCases = total,
                InProgressCases = inProgress,
                ReadyForWorkCases = ready,
                CancelledCases = cancelled
            };

            return RecruitmentResult<OnboardingCaseSummaryResponse>.Ok(summary, "Ringkasan metrik kasus onboarding berhasil diambil.");
        }

        public async Task<RecruitmentResult<OnboardingCaseDetailResponse>> GetByIdAsync(
            Guid id,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var c = await _dbContext.HrdOnboardingCases
                .Include(x => x.WorkforceProfile)
                .Include(x => x.HospitalSite)
                .Include(x => x.OrganizationUnit)
                .Include(x => x.Position)
                .Include(x => x.CoordinatorUser)
                .Include(x => x.Items)
                    .ThenInclude(i => i.CertificationType)
                .Include(x => x.Items)
                    .ThenInclude(i => i.CompletedByUser)
                .Include(x => x.Items)
                    .ThenInclude(i => i.SelectedWorkforceProfile)
                .Include(x => x.AccessRequests)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (c == null)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.NotFound("Kasus onboarding tidak ditemukan.");
            }

            if (!IsHrOrAdmin(user))
            {
                var userUnitId = GetUserUnitId(user);
                if (userUnitId.HasValue && c.OrganizationUnitId != userUnitId.Value)
                {
                    return RecruitmentResult<OnboardingCaseDetailResponse>.Forbidden(
                        "Anda tidak memiliki wewenang untuk melihat kasus onboarding di luar unit kerja Anda.");
                }
            }

            return RecruitmentResult<OnboardingCaseDetailResponse>.Ok(
                MapToDetailResponse(c),
                "Detail kasus onboarding berhasil diambil.");
        }

        public async Task<RecruitmentResult<OnboardingCaseDetailResponse>> RecordFirstDayAttendanceAsync(
            Guid caseId,
            RecordFirstDayAttendanceRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId(user);

            var c = await _dbContext.HrdOnboardingCases
                .Include(x => x.Items)
                .Include(x => x.AccessRequests)
                .FirstOrDefaultAsync(x => x.Id == caseId && !x.IsDelete, cancellationToken);

            if (c == null)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.NotFound("Kasus onboarding tidak ditemukan.");
            }

            if (!IsHrOrAdmin(user))
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Forbidden(
                    "Pencatatan hadir hari pertama hanya berhak dilakukan oleh petugas HRD (VR-ONB-03).");
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

            // Validasi VR-ONB-10: Hadir hari pertama tidak boleh sebelum tanggal mulai rencana
            if (request.AttendedDate < c.PlannedStartDate)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Unprocessable(
                    $"Tanggal kehadiran hari pertama ({request.AttendedDate}) tidak boleh mendahului tanggal mulai rencana kerja ({c.PlannedStartDate}) (VR-ONB-10).");
            }

            c.FirstDayAttendedOn = request.AttendedDate;
            c.FirstDayRecordedByUserId = userId;
            c.RowVersion = Guid.NewGuid();
            c.UpdateDateTime = DateTime.UtcNow;
            c.UpdateBy = userId;

            // Selesaikan otomatis butir sistem FIRST_DAY_ATTENDANCE
            var firstDayItem = c.Items.FirstOrDefault(i => !i.IsDelete && i.SystemItemCode == EmploymentLifecycleValueConstants.ItemFirstDayAttendance);
            if (firstDayItem != null)
            {
                firstDayItem.ItemStatus = OnboardingItemStatus.Completed;
                firstDayItem.CompletedAt = DateTime.UtcNow;
                firstDayItem.CompletedByUserId = userId;
                firstDayItem.CompletionNote = $"Dicatat hadir hari pertama pada tanggal {request.AttendedDate:yyyy-MM-dd}.";
                firstDayItem.RowVersion = Guid.NewGuid();
                firstDayItem.UpdateDateTime = DateTime.UtcNow;
                firstDayItem.UpdateBy = userId;
            }

            // Aktivasi akun layanan mandiri pegawai
            var accessReq = c.AccessRequests.FirstOrDefault(a => !a.IsDelete && a.RequestType == OnboardingAccessRequestType.AccountCreation);
            if (accessReq != null && accessReq.ApplicationUserId.HasValue)
            {
                await _userAccountService.ActivateAsync(accessReq.ApplicationUserId.Value, userId, cancellationToken);
                accessReq.SelfServiceActivatedAt = DateTime.UtcNow;
                accessReq.UpdateDateTime = DateTime.UtcNow;
                accessReq.UpdateBy = userId;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetByIdInternalAsync(c.Id, cancellationToken);
        }

        public async Task<RecruitmentResult<OnboardingCaseDetailResponse>> CancelFirstDayAttendanceAsync(
            Guid caseId,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId(user);

            var c = await _dbContext.HrdOnboardingCases
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == caseId && !x.IsDelete, cancellationToken);

            if (c == null)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.NotFound("Kasus onboarding tidak ditemukan.");
            }

            if (!IsHrOrAdmin(user))
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Forbidden(
                    "Pembatalan hadir hari pertama hanya berhak dilakukan oleh petugas HRD.");
            }

            if (!c.FirstDayAttendedOn.HasValue)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Unprocessable(
                    "Belum ada catatan kehadiran hari pertama yang dapat dibatalkan.");
            }

            // Validasi VR-ONB-17: Pembatalan hanya sah selama kontrak kerja pertama masih Draft
            var contracts = await _dbContext.WfpContractHistories
                .Where(h => h.WorkforceProfileId == c.WorkforceProfileId && !h.IsDelete)
                .ToListAsync(cancellationToken);

            var hasSignedContract = contracts.Any(h => h.ContractStatus != EmploymentLifecycleValueConstants.ContractStatusDraft);
            if (hasSignedContract)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Unprocessable(
                    "Kehadiran hari pertama tidak dapat dibatalkan karena kontrak kerja pertama sudah disahkan bertanda tangan (VR-ONB-17).");
            }

            c.FirstDayAttendedOn = null;
            c.FirstDayRecordedByUserId = null;
            c.RowVersion = Guid.NewGuid();
            c.UpdateDateTime = DateTime.UtcNow;
            c.UpdateBy = userId;

            var firstDayItem = c.Items.FirstOrDefault(i => !i.IsDelete && i.SystemItemCode == EmploymentLifecycleValueConstants.ItemFirstDayAttendance);
            if (firstDayItem != null)
            {
                firstDayItem.ItemStatus = OnboardingItemStatus.Pending;
                firstDayItem.CompletedAt = null;
                firstDayItem.CompletedByUserId = null;
                firstDayItem.CompletionNote = null;
                firstDayItem.RowVersion = Guid.NewGuid();
                firstDayItem.UpdateDateTime = DateTime.UtcNow;
                firstDayItem.UpdateBy = userId;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetByIdInternalAsync(c.Id, cancellationToken);
        }

        public async Task<RecruitmentResult<OnboardingCaseDetailResponse>> CompleteItemAsync(
            Guid caseId,
            Guid itemId,
            CompleteChecklistItemRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId(user);

            var c = await _dbContext.HrdOnboardingCases
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == caseId && !x.IsDelete, cancellationToken);

            if (c == null)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.NotFound("Kasus onboarding tidak ditemukan.");
            }

            var item = c.Items.FirstOrDefault(i => i.Id == itemId && !i.IsDelete);
            if (item == null)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.NotFound("Butir checklist onboarding tidak ditemukan.");
            }

            // Validasi VR-ONB-06: Butir otomatis menolak dicentang manual
            if (item.ItemKind == OnboardingItemKind.Automatic)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Unprocessable(
                    "Butir otomatis dilarang dicentang tangan. Status butir akan terisi otomatis berdasarkan data sistem (VR-ONB-06).");
            }

            // Validasi VR-ONB-03 & VR-ONB-04: Pemisahan jalur HRD vs Kepala Unit
            if (item.ResponsibleRole == OnboardingResponsibleRole.Hrd)
            {
                if (!IsHrOrAdmin(user))
                {
                    return RecruitmentResult<OnboardingCaseDetailResponse>.Forbidden(
                        "Butir ini menjadi tanggung jawab HRD dan hanya dapat dicentang oleh petugas HRD (VR-ONB-03).");
                }
            }
            else if (item.ResponsibleRole == OnboardingResponsibleRole.UnitHead)
            {
                if (!IsHrOrAdmin(user))
                {
                    var userUnitId = GetUserUnitId(user);
                    if (!userUnitId.HasValue || userUnitId.Value != c.OrganizationUnitId)
                    {
                        return RecruitmentResult<OnboardingCaseDetailResponse>.Forbidden(
                            "Butir ini menjadi tanggung jawab Kepala Unit dan hanya dapat dicentang oleh Kepala Unit bersangkutan (VR-ONB-04).");
                    }
                }
            }

            // Penanganan butir wajib atasan langsung & preceptor
            if (item.SystemItemCode == EmploymentLifecycleValueConstants.ItemDirectSupervisorAssigned)
            {
                if (!request.SelectedWorkforceProfileId.HasValue)
                {
                    return RecruitmentResult<OnboardingCaseDetailResponse>.Unprocessable(
                        "Penetapan atasan langsung wajib memilih profil pegawai atasan.");
                }
                item.SelectedWorkforceProfileId = request.SelectedWorkforceProfileId.Value;
            }
            else if (item.SystemItemCode == EmploymentLifecycleValueConstants.ItemPreceptorAssigned)
            {
                if (!request.SelectedWorkforceProfileId.HasValue)
                {
                    return RecruitmentResult<OnboardingCaseDetailResponse>.Unprocessable(
                        "Penetapan preceptor klinis wajib memilih profil tenaga kesehatan preceptor.");
                }

                // Buat entitas pendampingan klinis lewat service BE-HRD-073
                var preceptorshipResult = await _preceptorshipService.CreateForCaseAsync(
                    c.Id,
                    request.SelectedWorkforceProfileId.Value,
                    userId,
                    cancellationToken);

                if (!preceptorshipResult.Success)
                {
                    return RecruitmentResult<OnboardingCaseDetailResponse>.Unprocessable(preceptorshipResult.Message);
                }

                item.SelectedWorkforceProfileId = request.SelectedWorkforceProfileId.Value;
            }

            // Simpan berkas bukti jika dilampirkan
            if (request.EvidenceFile != null && request.EvidenceFile.Length > 0)
            {
                var saveResult = await _attachmentService.ValidateAndSaveAttachmentAsync(
                    request.EvidenceFile,
                    "onboarding-checklist-evidence",
                    cancellationToken);

                if (!saveResult.IsSuccess)
                {
                    return RecruitmentResult<OnboardingCaseDetailResponse>.Fail(saveResult.ErrorMessage ?? "Gagal menyimpan berkas bukti.");
                }

                item.EvidenceStoragePath = saveResult.StoragePath;
                item.EvidenceFileName = saveResult.FileName;
                item.EvidenceContentType = saveResult.ContentType;
                item.EvidenceFileSize = saveResult.FileSize;
            }

            item.ItemStatus = OnboardingItemStatus.Completed;
            item.CompletedAt = DateTime.UtcNow;
            item.CompletedByUserId = userId;
            item.CompletionNote = request.Note?.Trim();
            item.RowVersion = Guid.NewGuid();
            item.UpdateDateTime = DateTime.UtcNow;
            item.UpdateBy = userId;

            c.RowVersion = Guid.NewGuid();
            c.UpdateDateTime = DateTime.UtcNow;
            c.UpdateBy = userId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetByIdInternalAsync(c.Id, cancellationToken);
        }

        public async Task<RecruitmentResult<OnboardingCaseDetailResponse>> UncompleteItemAsync(
            Guid caseId,
            Guid itemId,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId(user);

            var c = await _dbContext.HrdOnboardingCases
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == caseId && !x.IsDelete, cancellationToken);

            if (c == null)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.NotFound("Kasus onboarding tidak ditemukan.");
            }

            var item = c.Items.FirstOrDefault(i => i.Id == itemId && !i.IsDelete);
            if (item == null)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.NotFound("Butir checklist onboarding tidak ditemukan.");
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

            item.ItemStatus = OnboardingItemStatus.Pending;
            item.CompletedAt = null;
            item.CompletedByUserId = null;
            item.CompletionNote = null;
            item.RowVersion = Guid.NewGuid();
            item.UpdateDateTime = DateTime.UtcNow;
            item.UpdateBy = userId;

            c.RowVersion = Guid.NewGuid();
            c.UpdateDateTime = DateTime.UtcNow;
            c.UpdateBy = userId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetByIdInternalAsync(c.Id, cancellationToken);
        }

        public async Task<RecruitmentResult<OnboardingCaseDetailResponse>> CheckAutomaticItemsAsync(
            Guid caseId,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId(user);

            var c = await _dbContext.HrdOnboardingCases
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == caseId && !x.IsDelete, cancellationToken);

            if (c == null)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.NotFound("Kasus onboarding tidak ditemukan.");
            }

            // 1. Evaluasi CONTRACT_SIGNED
            var contractItem = c.Items.FirstOrDefault(i => !i.IsDelete && i.SystemItemCode == EmploymentLifecycleValueConstants.ItemContractSigned);
            if (contractItem != null)
            {
                var hasActiveContract = await _dbContext.WfpContractHistories
                    .AnyAsync(h => h.WorkforceProfileId == c.WorkforceProfileId &&
                                   h.ContractStatus == EmploymentLifecycleValueConstants.ContractStatusActive &&
                                   !h.IsDelete, cancellationToken);

                if (hasActiveContract)
                {
                    contractItem.ItemStatus = OnboardingItemStatus.Completed;
                    contractItem.CompletedAt = DateTime.UtcNow;
                    contractItem.CompletedByUserId = userId;
                    contractItem.AutomaticCheckedAt = DateTime.UtcNow;
                    contractItem.AutomaticResultNote = "Kontrak kerja telah sah bertanda tangan (Active).";
                }
            }

            // 2. Evaluasi FITNESS_CLEARANCE
            var fitItem = c.Items.FirstOrDefault(i => !i.IsDelete && i.SystemItemCode == EmploymentLifecycleValueConstants.ItemFitnessClearance);
            if (fitItem != null)
            {
                var handoff = await _dbContext.HrdCandidateHiringHandoffs
                    .FirstOrDefaultAsync(h => h.Id == c.HiringHandoffId && !h.IsDelete, cancellationToken);

                if (handoff != null)
                {
                    var mcu = await _dbContext.HrdPreEmploymentMedicalChecks
                        .FirstOrDefaultAsync(m => m.OfferId == handoff.OfferId && !m.IsDelete, cancellationToken);

                    var isFit = mcu != null && (mcu.ClearanceResult == McuClearanceResult.Fit || mcu.ClearanceResult == McuClearanceResult.FitWithRestrictions);
                    if (isFit)
                    {
                        fitItem.ItemStatus = OnboardingItemStatus.Completed;
                        fitItem.CompletedAt = DateTime.UtcNow;
                        fitItem.CompletedByUserId = userId;
                        fitItem.AutomaticCheckedAt = DateTime.UtcNow;
                        fitItem.AutomaticResultNote = $"Kelaikan medis terverifikasi ({mcu?.ClearanceResult}).";
                    }
                }
            }

            // 3. Evaluasi Butir Sertifikat Wajib
            var certItems = c.Items
                .Where(i => !i.IsDelete && i.ItemKind == OnboardingItemKind.Automatic && i.CertificationTypeId.HasValue)
                .ToList();

            foreach (var certItem in certItems)
            {
                var validCert = await _dbContext.Set<WfpCertification>()
                    .Where(w => w.WorkforceProfileId == c.WorkforceProfileId &&
                                w.CertificationTypeId == certItem.CertificationTypeId.Value &&
                                !w.IsDelete)
                    .OrderByDescending(w => w.CreateDateTime)
                    .FirstOrDefaultAsync(cancellationToken);

                if (validCert != null)
                {
                    certItem.ItemStatus = OnboardingItemStatus.Completed;
                    certItem.CompletedAt = DateTime.UtcNow;
                    certItem.CompletedByUserId = userId;
                    certItem.AutomaticCheckedAt = DateTime.UtcNow;
                    certItem.AutomaticResultNote = $"Sertifikasi terdaftar ({validCert.CertificationName}).";
                }
            }

            c.RowVersion = Guid.NewGuid();
            c.UpdateDateTime = DateTime.UtcNow;
            c.UpdateBy = userId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetByIdInternalAsync(c.Id, cancellationToken);
        }

        public async Task<RecruitmentResult<OnboardingCaseDetailResponse>> ReassignCoordinatorAsync(
            Guid caseId,
            ReassignCoordinatorRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId(user);

            var c = await _dbContext.HrdOnboardingCases
                .FirstOrDefaultAsync(x => x.Id == caseId && !x.IsDelete, cancellationToken);

            if (c == null)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.NotFound("Kasus onboarding tidak ditemukan.");
            }

            if (!IsHrOrAdmin(user))
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Forbidden(
                    "Pengalihan koordinator kasus hanya dapat dilakukan oleh petugas HRD.");
            }

            c.CoordinatorUserId = request.NewCoordinatorUserId;
            c.RowVersion = Guid.NewGuid();
            c.UpdateDateTime = DateTime.UtcNow;
            c.UpdateBy = userId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetByIdInternalAsync(c.Id, cancellationToken);
        }

        public async Task<RecruitmentResult<OnboardingCaseDetailResponse>> DeclareReadyForWorkAsync(
            Guid caseId,
            DeclareReadyForWorkRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId(user);

            var c = await _dbContext.HrdOnboardingCases
                .Include(x => x.WorkforceProfile)
                .Include(x => x.Items)
                .Include(x => x.AccessRequests)
                .FirstOrDefaultAsync(x => x.Id == caseId && !x.IsDelete, cancellationToken);

            if (c == null)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.NotFound("Kasus onboarding tidak ditemukan.");
            }

            if (!IsHrOrAdmin(user))
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Forbidden(
                    "Pernyataan Siap Kerja hanya berhak disahkan oleh petugas HRD.");
            }

            if (c.CaseStatus != OnboardingCaseStatus.InProgress)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Unprocessable(
                    $"Siap kerja hanya dapat dinyatakan saat kasus berstatus InProgress. Status saat ini: {c.CaseStatus}.");
            }

            if (!c.FirstDayAttendedOn.HasValue)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Unprocessable(
                    "Pernyataan Siap Kerja ditolak karena pegawai belum dicatat hadir hari pertama.");
            }

            // Validasi VR-ONB-11: Seluruh butir checklist wajib (IsRequired) WAJIB berstatus Completed
            var pendingRequiredItems = c.Items
                .Where(i => !i.IsDelete && i.IsRequired && i.ItemStatus != OnboardingItemStatus.Completed)
                .ToList();

            if (pendingRequiredItems.Any())
            {
                var names = string.Join(", ", pendingRequiredItems.Take(3).Select(i => i.ItemName));
                return RecruitmentResult<OnboardingCaseDetailResponse>.Unprocessable(
                    $"Pernyataan Siap Kerja ditolak karena masih terdapat {pendingRequiredItems.Count} butir checklist wajib yang belum diselesaikan (VR-ONB-11): {names}.");
            }

            // Validasi VR-ONB-12: Penyata siap kerja dilarang sama dengan yang memilih atasan langsung
            var supervisorItem = c.Items.FirstOrDefault(i => !i.IsDelete && i.SystemItemCode == EmploymentLifecycleValueConstants.ItemDirectSupervisorAssigned);
            if (supervisorItem != null && supervisorItem.CompletedByUserId.HasValue && supervisorItem.CompletedByUserId.Value == userId)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Unprocessable(
                    "Pernyataan Siap Kerja dilarang dilakukan oleh orang yang sama dengan yang memilih atasan langsung (VR-ONB-12).");
            }

            // Transaksi Siap Kerja:
            // 1. Status kasus menjadi Completed
            c.CaseStatus = OnboardingCaseStatus.Completed;
            c.ReadyForWorkAt = DateTime.UtcNow;
            c.ReadyForWorkByUserId = userId;
            c.RowVersion = Guid.NewGuid();
            c.UpdateDateTime = DateTime.UtcNow;
            c.UpdateBy = userId;

            // 2. Aktifkan profil pegawai
            if (c.WorkforceProfile != null)
            {
                c.WorkforceProfile.IsActive = true;
                c.WorkforceProfile.UpdateDateTime = DateTime.UtcNow;
                c.WorkforceProfile.UpdateBy = userId;
            }

            // 3. Terbitkan penugasan atasan langsung WfpManagerAssignment
            if (supervisorItem != null && supervisorItem.SelectedWorkforceProfileId.HasValue)
            {
                var existingMgr = await _dbContext.Set<WfpManagerAssignment>()
                    .FirstOrDefaultAsync(m => m.WorkforceProfileId == c.WorkforceProfileId && m.IsActive && !m.IsDelete, cancellationToken);

                if (existingMgr == null)
                {
                    var newMgr = new WfpManagerAssignment
                    {
                        Id = Guid.NewGuid(),
                        WorkforceProfileId = c.WorkforceProfileId,
                        ManagerWorkforceProfileId = supervisorItem.SelectedWorkforceProfileId.Value,
                        OrganizationUnitId = c.OrganizationUnitId,
                        ManagerType = "Direct",
                        EffectiveStartDate = DateTime.UtcNow,
                        IsPrimaryManager = true,
                        CanApproveRequests = true,
                        IsActive = true,
                        CreateBy = userId,
                        CreateDateTime = DateTime.UtcNow
                    };
                    await _dbContext.Set<WfpManagerAssignment>().AddAsync(newMgr, cancellationToken);
                }
            }

            // 4. Jika klinis, perbarui status pendampingan menjadi Accompanied (BE-HRD-073)
            if (c.IsClinical)
            {
                await _preceptorshipService.SetAccompaniedAsync(c.Id, cancellationToken);
            }

            // 5. Terbitkan permintaan akses kerja SIMRS penuh
            var workAccessReq = new HrdOnboardingAccessRequest
            {
                Id = Guid.NewGuid(),
                OnboardingCaseId = c.Id,
                WorkforceProfileId = c.WorkforceProfileId,
                RequestType = OnboardingAccessRequestType.WorkAccess,
                RequestStatus = OnboardingAccessRequestStatus.Requested,
                RequestedAt = DateTime.UtcNow,
                CreateBy = userId,
                CreateDateTime = DateTime.UtcNow
            };
            await _dbContext.HrdOnboardingAccessRequests.AddAsync(workAccessReq, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetByIdInternalAsync(c.Id, cancellationToken);
        }

        public async Task<RecruitmentResult<OnboardingCaseDetailResponse>> CancelJoinAsync(
            Guid caseId,
            CancelOnboardingJoinRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId(user);

            var c = await _dbContext.HrdOnboardingCases
                .Include(x => x.WorkforceProfile)
                .Include(x => x.HiringHandoff)
                .FirstOrDefaultAsync(x => x.Id == caseId && !x.IsDelete, cancellationToken);

            if (c == null)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.NotFound("Kasus onboarding tidak ditemukan.");
            }

            if (!IsHrOrAdmin(user))
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Forbidden(
                    "Pembatalan bergabung pegawai hanya berhak diproses oleh petugas HRD.");
            }

            if (c.CaseStatus != OnboardingCaseStatus.InProgress)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Unprocessable(
                    $"Pembatalan bergabung hanya dapat dilakukan saat kasus InProgress. Status saat ini: {c.CaseStatus}.");
            }

            // Validasi VR-ONB-16: Batal bergabung hanya sah SEBELUM hadir hari pertama
            if (c.FirstDayAttendedOn.HasValue)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Unprocessable(
                    "Pembatalan bergabung hanya dapat diproses sebelum pegawai hadir hari pertama. Pegawai yang sudah hadir harus melalui proses pemisahan resmi (VR-ONB-16).");
            }

            if (string.IsNullOrWhiteSpace(request.CancelNote))
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.Unprocessable(
                    "Catatan alasan pembatalan bergabung wajib diisi (VR-ONB-13).");
            }

            c.CaseStatus = OnboardingCaseStatus.Cancelled;
            c.CancelReason = request.CancelReason;
            c.CancelNote = request.CancelNote.Trim();
            c.CancelledAt = DateTime.UtcNow;
            c.CancelledByUserId = userId;
            c.RowVersion = Guid.NewGuid();
            c.UpdateDateTime = DateTime.UtcNow;
            c.UpdateBy = userId;

            // Nonaktifkan profil pegawai
            if (c.WorkforceProfile != null)
            {
                c.WorkforceProfile.IsActive = false;
                c.WorkforceProfile.UpdateDateTime = DateTime.UtcNow;
                c.WorkforceProfile.UpdateBy = userId;
            }

            // Batalkan kontrak kerja draf jika ada (VR-ONB-14)
            var draftContracts = await _dbContext.WfpContractHistories
                .Where(h => h.WorkforceProfileId == c.WorkforceProfileId &&
                            h.ContractStatus == EmploymentLifecycleValueConstants.ContractStatusDraft &&
                            !h.IsDelete)
                .ToListAsync(cancellationToken);

            foreach (var ctr in draftContracts)
            {
                ctr.ContractStatus = "Cancelled";
                ctr.RowVersion = Guid.NewGuid();
                ctr.UpdateDateTime = DateTime.UtcNow;
                ctr.UpdateBy = userId;
            }

            // Batalkan berkas serah terima hiring handoff & buka kembali kursi WFP (VR-ONB-14)
            if (c.HiringHandoff != null)
            {
                c.HiringHandoff.Status = HandoffStatus.Cancelled;
                c.HiringHandoff.Notes = $"{c.HiringHandoff.Notes} | Pembatalan: {request.CancelNote.Trim()}";
                c.HiringHandoff.RowVersion = Guid.NewGuid();
                c.HiringHandoff.UpdateDateTime = DateTime.UtcNow;
                c.HiringHandoff.UpdateBy = userId;

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
                        seat.UpdateDateTime = DateTime.UtcNow;
                        seat.UpdateBy = userId;
                    }
                }
            }

            // Jika klinis, batalkan pendampingan klinis
            if (c.IsClinical)
            {
                await _preceptorshipService.CancelForCaseAsync(c.Id, cancellationToken);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetByIdInternalAsync(c.Id, cancellationToken);
        }

        public async Task<(bool Exists, Stream? Stream, string? ContentType, string? FileName)> DownloadItemEvidenceAsync(
            Guid caseId,
            Guid itemId,
            CancellationToken cancellationToken = default)
        {
            var item = await _dbContext.HrdOnboardingCaseItems
                .FirstOrDefaultAsync(i => i.Id == itemId && i.OnboardingCaseId == caseId && !i.IsDelete, cancellationToken);

            if (item == null || string.IsNullOrWhiteSpace(item.EvidenceStoragePath))
            {
                return (false, null, null, null);
            }

            return await _attachmentService.GetAttachmentStreamAsync(
                item.EvidenceStoragePath,
                cancellationToken);
        }

        private async Task<RecruitmentResult<OnboardingCaseDetailResponse>> GetByIdInternalAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            var c = await _dbContext.HrdOnboardingCases
                .Include(x => x.WorkforceProfile)
                .Include(x => x.HospitalSite)
                .Include(x => x.OrganizationUnit)
                .Include(x => x.Position)
                .Include(x => x.CoordinatorUser)
                .Include(x => x.Items)
                    .ThenInclude(i => i.CertificationType)
                .Include(x => x.Items)
                    .ThenInclude(i => i.CompletedByUser)
                .Include(x => x.Items)
                    .ThenInclude(i => i.SelectedWorkforceProfile)
                .Include(x => x.AccessRequests)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (c == null)
            {
                return RecruitmentResult<OnboardingCaseDetailResponse>.NotFound("Kasus onboarding tidak ditemukan.");
            }

            return RecruitmentResult<OnboardingCaseDetailResponse>.Ok(
                MapToDetailResponse(c),
                "Detail kasus onboarding berhasil diambil.");
        }

        private static OnboardingCaseDetailResponse MapToDetailResponse(HrdOnboardingCase c)
        {
            var actions = new List<string>();
            if (c.CaseStatus == OnboardingCaseStatus.InProgress)
            {
                if (!c.FirstDayAttendedOn.HasValue)
                {
                    actions.Add("RecordFirstDayAttendance");
                    actions.Add("CancelJoin");
                }
                else
                {
                    actions.Add("CancelFirstDayAttendance");
                }
                actions.Add("CheckAutomaticItems");
                actions.Add("ReassignCoordinator");

                var allRequiredCompleted = c.Items
                    .Where(i => !i.IsDelete && i.IsRequired)
                    .All(i => i.ItemStatus == OnboardingItemStatus.Completed);

                if (allRequiredCompleted && c.FirstDayAttendedOn.HasValue)
                {
                    actions.Add("DeclareReady");
                }
            }

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
                CoordinatorUserName = c.CoordinatorUser?.UserName ?? "-",
                CaseStatus = c.CaseStatus,
                FirstDayAttendedOn = c.FirstDayAttendedOn,
                ReadyForWorkAt = c.ReadyForWorkAt,
                CancelReason = c.CancelReason,
                CancelNote = c.CancelNote,
                CancelledAt = c.CancelledAt,
                HasGeneralTemplate = c.HasGeneralTemplate,
                AvailableActions = actions,
                RowVersion = c.RowVersion,
                Items = c.Items.Where(i => !i.IsDelete).OrderBy(i => i.DueDate).ThenBy(i => i.ItemCode).Select(i => new OnboardingCaseItemResponse
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
                    CompletedByUserName = i.CompletedByUser?.UserName,
                    CompletionNote = i.CompletionNote,
                    SelectedWorkforceProfileId = i.SelectedWorkforceProfileId,
                    SelectedWorkforceFullName = i.SelectedWorkforceProfile?.DisplayName,
                    EvidenceStoragePath = i.EvidenceStoragePath,
                    EvidenceFileName = i.EvidenceFileName,
                    AutomaticCheckedAt = i.AutomaticCheckedAt,
                    AutomaticResultNote = i.AutomaticResultNote,
                    RowVersion = i.RowVersion
                }).ToList(),
                AccessRequests = c.AccessRequests.Where(a => !a.IsDelete).OrderByDescending(a => a.RequestedAt).Select(a => new OnboardingAccessRequestResponse
                {
                    Id = a.Id,
                    RequestType = a.RequestType,
                    RequestStatus = a.RequestStatus,
                    ApplicationUserId = a.ApplicationUserId,
                    FailureMessage = a.FailureMessage,
                    SelfServiceActivatedAt = a.SelfServiceActivatedAt,
                    RequestedAt = a.RequestedAt,
                    CompletedAt = a.CompletedAt
                }).ToList()
            };
        }
    }
}
