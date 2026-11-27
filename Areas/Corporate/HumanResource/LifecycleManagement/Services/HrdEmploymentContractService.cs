using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services
{
    public class HrdEmploymentContractService : IHrdEmploymentContractService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IWfpContractHistoryWriteService _contractWriteService;
        private readonly IWfpNewHireAssignmentService _newHireAssignmentService;
        private readonly IHrdProbationService _probationService;
        private readonly IHrdLifecycleAttachmentService _attachmentService;
        private readonly IHrdNumberSeriesService _numberSeriesService;
        private readonly ResignationLifecycleHandoffService _resignationHandoffService;
        private readonly IHrdEmploymentLifecycleWorkflowIntegrationService _workflowIntegrationService;
        private readonly IHrdFixedTermPolicyService _fixedTermPolicyService;
        private readonly AccessPermissionService _permissionService;
        private readonly QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services.HrdUnitScopeService _unitScopeService;

        public HrdEmploymentContractService(
            ApplicationDbContext dbContext,
            IWfpContractHistoryWriteService contractWriteService,
            IWfpNewHireAssignmentService newHireAssignmentService,
            IHrdProbationService probationService,
            IHrdLifecycleAttachmentService attachmentService,
            IHrdNumberSeriesService numberSeriesService,
            ResignationLifecycleHandoffService resignationHandoffService,
            IHrdEmploymentLifecycleWorkflowIntegrationService workflowIntegrationService,
            IHrdFixedTermPolicyService fixedTermPolicyService,
            AccessPermissionService permissionService,
            QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services.HrdUnitScopeService unitScopeService)
        {
            _unitScopeService = unitScopeService ?? throw new ArgumentNullException(nameof(unitScopeService));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _contractWriteService = contractWriteService ?? throw new ArgumentNullException(nameof(contractWriteService));
            _newHireAssignmentService = newHireAssignmentService ?? throw new ArgumentNullException(nameof(newHireAssignmentService));
            _probationService = probationService ?? throw new ArgumentNullException(nameof(probationService));
            _attachmentService = attachmentService ?? throw new ArgumentNullException(nameof(attachmentService));
            _numberSeriesService = numberSeriesService ?? throw new ArgumentNullException(nameof(numberSeriesService));
            _resignationHandoffService = resignationHandoffService ?? throw new ArgumentNullException(nameof(resignationHandoffService));
            _workflowIntegrationService = workflowIntegrationService ?? throw new ArgumentNullException(nameof(workflowIntegrationService));
            _fixedTermPolicyService = fixedTermPolicyService ?? throw new ArgumentNullException(nameof(fixedTermPolicyService));
            _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
        }

        private static Guid GetUserId(ClaimsPrincipal user)
        {
            var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? user.FindFirst("sub")?.Value;
            return Guid.TryParse(sub, out var guid) ? guid : Guid.Empty;
        }

        public async Task<WfpContractHistory> CreateDraftFromOfferAsync(
            Guid workforceProfileId,
            HrdJobOffer offer,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            if (offer == null) throw new ArgumentNullException(nameof(offer));

            var existingContract = await _dbContext.WfpContractHistories
                .FirstOrDefaultAsync(c => c.SourceDocumentType == EmploymentLifecycleValueConstants.SourceDocOffer &&
                                          c.SourceDocumentId == offer.Id &&
                                          !c.IsDelete, cancellationToken);

            if (existingContract != null)
            {
                return existingContract;
            }

            var contractType = await _dbContext.MstContractTypes
                .FirstOrDefaultAsync(ct => ct.Id == offer.ContractTypeId && !ct.IsDelete, cancellationToken);

            var contractNumber = await _numberSeriesService.GenerateContractNumberAsync(cancellationToken);

            var startDate = offer.ProposedJoinDate.ToDateTime(TimeOnly.MinValue);
            DateTime? endDate = null;
            DateTime? seriesStartDate = null;

            if (contractType != null && contractType.RequiresEndDate && offer.ContractDurationMonths.HasValue)
            {
                endDate = startDate.AddMonths(offer.ContractDurationMonths.Value);
                seriesStartDate = startDate;
            }

            DateTime? probationEndDate = null;
            // UU 27/2026 Pasal 86: Masa percobaan DILARANG untuk PKWT (RequiresEndDate)
            if (contractType != null && contractType.IsProbationApplicable && !contractType.RequiresEndDate)
            {
                probationEndDate = startDate.AddMonths(contractType.DefaultDurationMonths ?? 3);
            }

            var newContract = new WfpContractHistory
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = workforceProfileId,
                ContractTypeId = offer.ContractTypeId,
                ContractNumber = contractNumber,
                HistoryType = "Initial",
                ContractStatus = EmploymentLifecycleValueConstants.ContractStatusDraft,
                StartDate = startDate,
                EndDate = endDate,
                FixedTermSeriesStartDate = seriesStartDate,
                ProbationEndDate = probationEndDate,
                RenewalSequence = 0,
                IsCurrent = false,
                SourceDocumentType = EmploymentLifecycleValueConstants.SourceDocOffer,
                SourceDocumentId = offer.Id,
                Description = $"Kontrak pertama draf dari tawaran pekerjaan nomor {offer.OfferNumber}",
                CreateDateTime = DateTime.UtcNow,
                CreateBy = userId,
                IsActive = true
            };

            await _contractWriteService.CreateInitialContractAsync(newContract, userId, cancellationToken);
            return newContract;
        }

        public async Task<RecruitmentResult<PagedResult<EmploymentContractResponse>>> GetPagedAsync(
            EmploymentContractQuery query,
            CancellationToken cancellationToken = default,
            ClaimsPrincipal? user = null)
        {
            query ??= new EmploymentContractQuery();
            var pageNumber = query.PageNumber <= 0 ? 1 : query.PageNumber;
            var pageSize = query.PageSize <= 0 ? 10 : (query.PageSize > 100 ? 100 : query.PageSize);

            var dbQuery = _dbContext.WfpContractHistories
                .Include(c => c.WorkforceProfile)
                .Include(c => c.ContractType)
                .AsNoTracking()
                .Where(c => !c.IsDelete)
                .AsQueryable();

            // BE-HRD-424: daftar kontrak dibatasi cakupan pemanggil.
            dbQuery = await ApplyContractReadScopeAsync(dbQuery, user, cancellationToken);

            if (!string.IsNullOrWhiteSpace(query.ContractStatus))
            {
                dbQuery = dbQuery.Where(c => c.ContractStatus == query.ContractStatus);
            }

            if (query.WorkforceProfileId.HasValue)
            {
                dbQuery = dbQuery.Where(c => c.WorkforceProfileId == query.WorkforceProfileId.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var s = query.Search.Trim().ToLower();
                dbQuery = dbQuery.Where(c =>
                    c.ContractNumber.ToLower().Contains(s) ||
                    (c.WorkforceProfile != null && (
                        c.WorkforceProfile.ProfileCode.ToLower().Contains(s) ||
                        c.WorkforceProfile.DisplayName.ToLower().Contains(s)
                    )));
            }

            if (query.ExpiringInDays.HasValue && query.ExpiringInDays.Value > 0)
            {
                var today = DateTime.UtcNow.Date;
                var horizon = today.AddDays(query.ExpiringInDays.Value);
                dbQuery = dbQuery.Where(c => c.EndDate.HasValue && c.EndDate.Value >= today && c.EndDate.Value <= horizon);
            }

            var totalCount = await dbQuery.CountAsync(cancellationToken);

            var items = await dbQuery
                .OrderByDescending(c => c.StartDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new EmploymentContractResponse
                {
                    Id = c.Id,
                    WorkforceProfileId = c.WorkforceProfileId,
                    WorkforceCode = c.WorkforceProfile != null ? c.WorkforceProfile.ProfileCode : string.Empty,
                    FullName = c.WorkforceProfile != null ? c.WorkforceProfile.DisplayName : string.Empty,
                    ContractNumber = c.ContractNumber,
                    HistoryType = c.HistoryType,
                    ContractStatus = c.ContractStatus,
                    ContractTypeId = c.ContractTypeId,
                    ContractTypeName = c.ContractType != null ? c.ContractType.ContractTypeName : string.Empty,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    SignedDate = c.SignedDate,
                    ProbationEndDate = c.ProbationEndDate,
                    IsCurrent = c.IsCurrent,
                    SourceDocumentType = c.SourceDocumentType,
                    SourceDocumentId = c.SourceDocumentId,
                    DocumentPath = c.DocumentPath,
                    DocumentFileName = c.DocumentFileName,
                    FixedTermSeriesStartDate = c.FixedTermSeriesStartDate,
                    RowVersion = c.RowVersion
                })
                .ToListAsync(cancellationToken);

            var pagedResult = new PagedResult<EmploymentContractResponse>
            {
                Items = items,
                TotalData = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPage = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            return RecruitmentResult<PagedResult<EmploymentContractResponse>>.Ok(pagedResult);
        }

        public async Task<RecruitmentResult<EmploymentContractResponse>> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default,
            ClaimsPrincipal? user = null)
        {
            var scoped = await ApplyContractReadScopeAsync(_dbContext.WfpContractHistories.AsQueryable(), user, cancellationToken);
            var c = await scoped
                .Include(x => x.WorkforceProfile)
                .Include(x => x.ContractType)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (c == null)
            {
                return RecruitmentResult<EmploymentContractResponse>.NotFound("Kontrak kerja tidak ditemukan.");
            }

            return RecruitmentResult<EmploymentContractResponse>.Ok(new EmploymentContractResponse
            {
                Id = c.Id,
                WorkforceProfileId = c.WorkforceProfileId,
                WorkforceCode = c.WorkforceProfile?.ProfileCode ?? string.Empty,
                FullName = c.WorkforceProfile?.DisplayName ?? string.Empty,
                ContractNumber = c.ContractNumber,
                HistoryType = c.HistoryType,
                ContractStatus = c.ContractStatus,
                ContractTypeId = c.ContractTypeId,
                ContractTypeName = c.ContractType?.ContractTypeName ?? string.Empty,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                SignedDate = c.SignedDate,
                ProbationEndDate = c.ProbationEndDate,
                IsCurrent = c.IsCurrent,
                SourceDocumentType = c.SourceDocumentType,
                SourceDocumentId = c.SourceDocumentId,
                DocumentPath = c.DocumentPath,
                DocumentFileName = c.DocumentFileName,
                FixedTermSeriesStartDate = c.FixedTermSeriesStartDate,
                RowVersion = c.RowVersion
            });
        }

        public async Task<RecruitmentResult<EmploymentContractPrintDataResponse>> GetPrintDataAsync(
            Guid id,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var scopedPrint = await ApplyContractReadScopeAsync(_dbContext.WfpContractHistories.AsQueryable(), user, cancellationToken);
            var contract = await scopedPrint
                .Include(c => c.WorkforceProfile)
                    .ThenInclude(p => p!.Employee)
                .Include(c => c.ContractType)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDelete, cancellationToken);

            if (contract == null)
            {
                return RecruitmentResult<EmploymentContractPrintDataResponse>.NotFound("Kontrak kerja tidak ditemukan.");
            }

            var offer = await _dbContext.HrdJobOffers
                .Include(o => o.Application)
                    .ThenInclude(a => a!.Vacancy)
                        .ThenInclude(v => v!.Requisition)
                            .ThenInclude(r => r!.HospitalSite)
                .Include(o => o.Application)
                    .ThenInclude(a => a!.Vacancy)
                        .ThenInclude(v => v!.Requisition)
                            .ThenInclude(r => r!.Position)
                .Include(o => o.Application)
                    .ThenInclude(a => a!.Vacancy)
                        .ThenInclude(v => v!.Requisition)
                            .ThenInclude(r => r!.OrganizationUnit)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == contract.SourceDocumentId, cancellationToken);

            var requisition = offer?.Application?.Vacancy?.Requisition;

            // FIN-HRD-08-BE-002: Pemeriksaan hak akses nominal di backend
            var canViewSalary = await _permissionService.HasAccessAsync(user, "WorkforceEmploymentContract", "ViewAmount");

            // AUD-08-10: Masking NIK / National ID jika user bukan admin atau tidak memiliki izin full identity
            var canViewFullIdentity = await _permissionService.HasAccessAsync(user, "WorkforceEmploymentContract", "ViewFullIdentity") ||
                                      await _permissionService.HasAccessAsync(user, "WorkforceEmploymentContract", "Manage");
            var rawNik = contract.WorkforceProfile?.Employee?.IdentityNumber;
            var maskedNik = "-";
            if (!string.IsNullOrWhiteSpace(rawNik) && rawNik != "-")
            {
                if (canViewFullIdentity)
                {
                    maskedNik = rawNik;
                }
                else
                {
                    maskedNik = rawNik.Length > 8
                        ? string.Concat(rawNik.AsSpan(0, 4), new string('*', rawNik.Length - 8), rawNik.AsSpan(rawNik.Length - 4))
                        : new string('*', rawNik.Length);
                }
            }

            var printData = new EmploymentContractPrintDataResponse
            {
                ContractId = contract.Id,
                ContractNumber = contract.ContractNumber,
                HospitalName = requisition?.HospitalSite?.SiteName ?? "-",
                EmployeeFullName = contract.WorkforceProfile?.DisplayName ?? string.Empty,
                EmployeeNationalId = maskedNik,
                PositionTitle = offer?.OfferedPositionTitle ?? requisition?.Position?.PositionName ?? "-",
                DepartmentName = requisition?.OrganizationUnit?.UnitName ?? "-",
                ContractTypeName = contract.ContractType?.ContractTypeName ?? "-",
                StartDate = contract.StartDate,
                EndDate = contract.EndDate,
                BasicSalary = canViewSalary ? offer?.OfferedBasicSalary : null,
                IsSalaryMasked = !canViewSalary
            };

            return RecruitmentResult<EmploymentContractPrintDataResponse>.Ok(printData);
        }

        public async Task<RecruitmentResult<ContractSalaryDetailResponse>> GetSalaryDetailAsync(
            Guid id,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var contract = await _dbContext.WfpContractHistories
                .Include(c => c.WorkforceProfile)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDelete, cancellationToken);

            if (contract == null)
            {
                return RecruitmentResult<ContractSalaryDetailResponse>.NotFound("Kontrak kerja tidak ditemukan.");
            }

            var canViewSalary = await _permissionService.HasAccessAsync(user, "WorkforceEmploymentContract", "ViewAmount");

            var offer = await _dbContext.HrdJobOffers
                .Include(o => o.SalaryGrade)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == contract.SourceDocumentId, cancellationToken);

            var salaryGradeId = offer?.SalaryGradeId;
            string? salaryGradeName = offer?.SalaryGrade?.SalaryGradeName;
            string? salaryStructureName = null;
            decimal? basicSalary = offer?.OfferedBasicSalary;

            if (salaryGradeId.HasValue)
            {
                var structure = await _dbContext.Set<MstSalaryStructure>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.SalaryGradeId == salaryGradeId.Value && s.IsActive && !s.IsDelete, cancellationToken);
                salaryStructureName = structure?.SalaryStructureName;
            }

            var response = new ContractSalaryDetailResponse
            {
                ContractId = contract.Id,
                BasicSalary = canViewSalary ? basicSalary : null,
                Currency = "IDR",
                SalaryGradeName = salaryGradeName,
                SalaryStructureName = salaryStructureName,
                IsMasked = !canViewSalary
            };

            return RecruitmentResult<ContractSalaryDetailResponse>.Ok(response);
        }

        public async Task<RecruitmentResult<EmploymentContractResponse>> RecordSignedAsync(
            Guid id,
            RecordContractSignedRequest request,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var contract = await _dbContext.WfpContractHistories
                .Include(c => c.ContractType)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDelete, cancellationToken);

            if (contract == null)
            {
                return RecruitmentResult<EmploymentContractResponse>.NotFound("Kontrak kerja tidak ditemukan.");
            }

            if (contract.ContractStatus != EmploymentLifecycleValueConstants.ContractStatusDraft)
            {
                return RecruitmentResult<EmploymentContractResponse>.BadRequest(
                    $"Kontrak dengan nomor {contract.ContractNumber} sudah berstatus {contract.ContractStatus} dan tidak dapat ditandatangani ulang.");
            }

            // Preflight validation sebelum simpan berkas (AUD-08-09 / P0-A / REG-HRD-08-001..003)
            var offer = await _dbContext.HrdJobOffers
                .Include(o => o.Application)
                    .ThenInclude(a => a!.Vacancy)
                        .ThenInclude(v => v!.Requisition)
                .Include(o => o.Approvals)
                .FirstOrDefaultAsync(o => o.Id == contract.SourceDocumentId, cancellationToken);

            Guid? offerApproverUserId = null;
            if (contract.HistoryType == "Initial" || contract.PreviousContractHistoryId == null)
            {
                if (offer?.Application?.Vacancy?.Requisition == null)
                {
                    return RecruitmentResult<EmploymentContractResponse>.BadRequest(
                        "Job offer dan requisition untuk kontrak awal tidak ditemukan. Penandatanganan kontrak ditolak.");
                }

                var req = offer.Application.Vacancy.Requisition;

                // Preflight Department FK
                var orgUnit = await _dbContext.MstOrganizationUnits
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == req.OrganizationUnitId && !u.IsDelete, cancellationToken);
                var actualDeptId = orgUnit?.DepartmentId ?? Guid.Empty;

                var pos = await _dbContext.MstPositions
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == req.PositionId && !p.IsDelete, cancellationToken);
                if (actualDeptId == Guid.Empty && pos?.DepartmentId != null)
                {
                    actualDeptId = pos.DepartmentId;
                }

                if (actualDeptId == Guid.Empty || !await _dbContext.MstDepartments.AnyAsync(d => d.Id == actualDeptId && !d.IsDelete, cancellationToken))
                {
                    return RecruitmentResult<EmploymentContractResponse>.BadRequest(
                        "Departemen valid (MstDepartment) tidak dapat ditentukan dari Unit Organisasi atau Posisi jabatan. Konfigurasi master data belum lengkap (P0-B/REG-HRD-08-003).");
                }

                // Preflight SalaryGrade & SalaryStructure
                var salaryGrade = await _dbContext.MstSalaryGrades
                    .AsNoTracking()
                    .FirstOrDefaultAsync(g => g.Id == offer.SalaryGradeId && !g.IsDelete, cancellationToken);
                if (salaryGrade == null)
                {
                    return RecruitmentResult<EmploymentContractResponse>.BadRequest(
                        $"Master golongan gaji (MstSalaryGrade) dengan ID '{offer.SalaryGradeId}' tidak ditemukan.");
                }

                var salaryStructure = await _dbContext.Set<MstSalaryStructure>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.SalaryGradeId == offer.SalaryGradeId && s.IsActive && !s.IsDelete, cancellationToken);
                if (salaryStructure == null)
                {
                    return RecruitmentResult<EmploymentContractResponse>.BadRequest(
                        $"Struktur penggajian aktif (MstSalaryStructure) untuk SalaryGrade '{salaryGrade.SalaryGradeName}' tidak ditemukan. Penandatanganan kontrak ditolak (REG-HRD-08-001/P0-A).");
                }

                offerApproverUserId = offer.Approvals
                    .FirstOrDefault(a => a.Status == ApprovalStatus.Approved)?
                    .ApproverUserId ?? offer.UpdateBy;
            }

            var saveResult = await _attachmentService.ValidateAndSaveAttachmentAsync(
                request.SignedScanFile,
                "employment-contracts",
                cancellationToken);

            if (!saveResult.IsSuccess)
            {
                return RecruitmentResult<EmploymentContractResponse>.BadRequest(
                    saveResult.ErrorMessage ?? "Gagal menyimpan berkas scan kontrak.");
            }

            await using var tx = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                await _contractWriteService.ActivateContractAsync(
                    contract.Id,
                    request.SignedDate,
                    saveResult.StoragePath!,
                    saveResult.FileName!,
                    saveResult.ContentType!,
                    userId,
                    cancellationToken);

                var onboardingCase = await _dbContext.HrdOnboardingCases
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.WorkforceProfileId == contract.WorkforceProfileId && !c.IsDelete, cancellationToken);

                if (offer?.Application?.Vacancy?.Requisition != null)
                {
                    var req = offer.Application.Vacancy.Requisition;
                    await _newHireAssignmentService.AssignNewHirePlacementAndSalaryAsync(
                        contract.WorkforceProfileId,
                        req.HospitalSiteId,
                        req.OrganizationUnitId,
                        req.PositionId,
                        offer.SalaryGradeId,
                        offer.OfferedBasicSalary,
                        contract.StartDate,
                        userId,
                        offerApproverUserId,
                        cancellationToken);
                }

                if (onboardingCase != null && contract.ProbationEndDate.HasValue)
                {
                    await _probationService.CreateForContractAsync(
                        contract,
                        onboardingCase.HospitalSiteId,
                        userId,
                        cancellationToken);
                }

                if (onboardingCase != null)
                {
                    var contractItem = onboardingCase.Items
                        .FirstOrDefault(i => i.SystemItemCode == EmploymentLifecycleValueConstants.ItemContractSigned && !i.IsDelete);

                    if (contractItem != null && contractItem.ItemStatus == OnboardingItemStatus.Pending)
                    {
                        contractItem.ItemStatus = OnboardingItemStatus.Completed;
                        contractItem.CompletedAt = DateTime.UtcNow;
                        contractItem.CompletedByUserId = userId;
                        contractItem.CompletionNote = "Kontrak kerja telah ditandatangani dan diaktifkan.";
                        contractItem.AutomaticCheckedAt = DateTime.UtcNow;
                        contractItem.AutomaticResultNote = $"Aktif via kontrak {contract.ContractNumber}";
                        contractItem.UpdateDateTime = DateTime.UtcNow;
                        contractItem.UpdateBy = userId;
                        contractItem.RowVersion = Guid.NewGuid();
                    }
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
                await tx.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync(cancellationToken);
                await _attachmentService.DeleteAttachmentAsync(saveResult.StoragePath, cancellationToken);
                return RecruitmentResult<EmploymentContractResponse>.BadRequest(
                    $"Gagal memproses penandatanganan kontrak: {ex.Message}");
            }

            return await GetByIdAsync(contract.Id, cancellationToken);
        }

        public async Task<(bool Exists, Stream? Stream, string? ContentType, string? FileName)> DownloadScanAsync(
            Guid id,
            CancellationToken cancellationToken = default,
            ClaimsPrincipal? user = null)
        {
            var scopedDownload = await ApplyContractReadScopeAsync(_dbContext.WfpContractHistories.AsQueryable(), user, cancellationToken);
            var contract = await scopedDownload
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDelete, cancellationToken);

            if (contract == null || string.IsNullOrWhiteSpace(contract.DocumentPath))
            {
                return (false, null, null, null);
            }

            return await _attachmentService.GetAttachmentStreamAsync(contract.DocumentPath, cancellationToken);
        }

        /// <summary>
        /// BE-HRD-424: cakupan baca kontrak. Pemegang <c>WorkforceEmploymentContract : ReadAllUnits</c> melihat
        /// seluruh RS; selain itu hanya kontrak milik sendiri dan pegawai yang penempatan organisasinya (hari ini)
        /// berada di unit yang dipimpin pengguna beserta turunannya. Pemanggil internal tanpa principal
        /// (worker/service lain) tidak dibatasi.
        /// </summary>
        private async Task<IQueryable<WfpContractHistory>> ApplyContractReadScopeAsync(
            IQueryable<WfpContractHistory> query,
            ClaimsPrincipal? user,
            CancellationToken cancellationToken)
        {
            if (user == null)
            {
                return query;
            }

            if (await _permissionService.HasAccessAsync(user, "WorkforceEmploymentContract", EmploymentContractPermissions.ReadAllUnits))
            {
                return query;
            }

            var callerUserId = GetUserId(user);
            var callerProfileId = await _dbContext.Users
                .AsNoTracking()
                .Where(x => x.Id == callerUserId)
                .Select(x => x.WorkforceProfileId)
                .FirstOrDefaultAsync(cancellationToken);

            var scope = await _unitScopeService.GetManagerialReadScopeAsync(cancellationToken);
            var unitIds = scope.UnitIds.ToList();
            var today = DateTime.UtcNow.Date;
            var profileIdsInScope = unitIds.Count == 0
                ? new List<Guid>()
                : await _dbContext.WfpOrganizationAssignments
                    .AsNoTracking()
                    .Where(a => a.OrganizationUnitId.HasValue && unitIds.Contains(a.OrganizationUnitId.Value) &&
                                a.IsActive && !a.IsDelete &&
                                a.EffectiveStartDate <= today &&
                                (a.EffectiveEndDate == null || a.EffectiveEndDate >= today))
                    .Select(a => a.WorkforceProfileId)
                    .Distinct()
                    .ToListAsync(cancellationToken);

            return query.Where(c =>
                (callerProfileId.HasValue && c.WorkforceProfileId == callerProfileId.Value) ||
                profileIdsInScope.Contains(c.WorkforceProfileId));
        }

        // --- BE-HRD-080: Keputusan Kontrak T16 & Batas PKWT UU 27/2026 ---

        public async Task<RecruitmentResult<FixedTermCheckResponse>> FixedTermCheckAsync(
            Guid contractId,
            CancellationToken cancellationToken = default)
        {
            var contract = await _dbContext.WfpContractHistories
                .Include(c => c.ContractType)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == contractId && !c.IsDelete, cancellationToken);

            if (contract == null)
            {
                return RecruitmentResult<FixedTermCheckResponse>.NotFound("Kontrak kerja tidak ditemukan.");
            }

            // FIN-HRD-08-BE-001/003: Evaluasi melalui service rule PKWT terpusat
            var checkResponse = _fixedTermPolicyService.EvaluateCompliance(contract);
            return RecruitmentResult<FixedTermCheckResponse>.Ok(checkResponse);
        }

        public async Task<RecruitmentResult<ContractDecisionResponse>> SubmitDecisionAsync(
            Guid contractId,
            SubmitContractDecisionRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var actorId = GetUserId(user);

            var contract = await _dbContext.WfpContractHistories
                .Include(c => c.ContractType)
                .Include(c => c.WorkforceProfile)
                .FirstOrDefaultAsync(c => c.Id == contractId && !c.IsDelete, cancellationToken);

            if (contract == null)
            {
                return RecruitmentResult<ContractDecisionResponse>.NotFound("Kontrak kerja tidak ditemukan.");
            }

            // VR-ONB-35: Hanya untuk kontrak Signed
            if (contract.ContractStatus != EmploymentLifecycleValueConstants.ContractStatusSigned &&
                contract.ContractStatus != "Active")
            {
                return RecruitmentResult<ContractDecisionResponse>.BadRequest(
                    $"VR-ONB-35: Keputusan hanya dapat diajukan untuk kontrak yang sedang aktif bertanda tangan. Status saat ini: {contract.ContractStatus}.");
            }

            // VR-ONB-37: Satu keputusan terbuka (Diajukan/Dikembalikan) atau disetujui per kontrak
            var existingDecision = await _dbContext.HrdContractDecisions
                .FirstOrDefaultAsync(d => d.ContractHistoryId == contractId &&
                                         (d.DecisionStatus == ContractDecisionStatus.Submitted ||
                                          d.DecisionStatus == ContractDecisionStatus.Returned ||
                                          d.DecisionStatus == ContractDecisionStatus.Approved) &&
                                         !d.IsDelete, cancellationToken);

            if (existingDecision != null)
            {
                return RecruitmentResult<ContractDecisionResponse>.BadRequest(
                    $"VR-ONB-37: Kontrak ini sudah memiliki keputusan yang berstatus {existingDecision.DecisionStatus} (Nomor: {existingDecision.DecisionNumber}).");
            }

            // VR-ONB-36: Validasi perpanjangan PKWT berdasarkan UU No. 27/2026 Pasal 82 (FIN-HRD-08-BE-001/003)
            if (request.DecisionType == ContractDecisionType.Extend)
            {
                var validation = _fixedTermPolicyService.ValidateRenewalProposal(
                    contract,
                    request.ProposedStartDate,
                    request.ProposedEndDate,
                    request.ExtensionMonths);

                if (!validation.IsValid)
                {
                    return RecruitmentResult<ContractDecisionResponse>.BadRequest(validation.ErrorMessage!);
                }
            }

            // Generate nomor keputusan
            var decisionNumber = await _numberSeriesService.GenerateDecisionNumberAsync(cancellationToken);

            var decision = new HrdContractDecision
            {
                Id = Guid.NewGuid(),
                DecisionNumber = decisionNumber,
                ContractHistoryId = contractId,
                WorkforceProfileId = contract.WorkforceProfileId,
                DecisionType = request.DecisionType,
                ExtensionMonths = request.ExtensionMonths,
                ProposedStartDate = request.ProposedStartDate,
                ProposedEndDate = request.ProposedEndDate,
                ReasonText = request.ReasonText,
                DecisionStatus = ContractDecisionStatus.Submitted,
                SubmittedByUserId = actorId,
                SubmittedAt = DateTime.UtcNow,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorId
            };

            _dbContext.HrdContractDecisions.Add(decision);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Pemicu alur persetujuan T16 (B6). Gagal tertutup: bila mesin menolak (belum ada langkah/penyetuju),
            // usulan dihapus lunak agar tidak tertinggal "Diajukan" tanpa penyetuju dan VR-ONB-37 tidak menahan pengajuan ulang.
            var workflow = await _workflowIntegrationService.StartContractDecisionWorkflowAsync(
                decision.Id,
                actorId,
                cancellationToken);

            if (!workflow.Success)
            {
                decision.IsDelete = true;
                decision.DeleteDateTime = DateTime.UtcNow;
                decision.DeleteBy = actorId;
                await _dbContext.SaveChangesAsync(cancellationToken);
                return RecruitmentResult<ContractDecisionResponse>.Fail(workflow.Message, workflow.StatusCode);
            }

            decision.WorkflowInstanceId = workflow.WorkflowInstanceId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return RecruitmentResult<ContractDecisionResponse>.Created(MapToDecisionResponse(decision, contract, user.Identity?.Name));
        }

        public async Task<RecruitmentResult<ContractDecisionResponse>> GetDecisionByIdAsync(
            Guid contractId,
            Guid decisionId,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var decision = await _dbContext.HrdContractDecisions
                .Include(d => d.ContractHistory)
                .Include(d => d.WorkforceProfile)
                .Include(d => d.SubmittedByUser)
                .Include(d => d.DecidedByUser)
                .Include(d => d.FollowUpContractHistory)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == decisionId && d.ContractHistoryId == contractId && !d.IsDelete, cancellationToken);

            if (decision == null)
            {
                return RecruitmentResult<ContractDecisionResponse>.NotFound("Keputusan kontrak tidak ditemukan.");
            }

            return RecruitmentResult<ContractDecisionResponse>.Ok(MapToDecisionResponse(decision, decision.ContractHistory, decision.SubmittedByUser?.DisplayName));
        }

        public async Task<RecruitmentResult<ContractDecisionResponse>> UpdateDecisionAsync(
            Guid contractId,
            Guid decisionId,
            SubmitContractDecisionRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var actorId = GetUserId(user);

            var decision = await _dbContext.HrdContractDecisions
                .Include(d => d.ContractHistory)
                .FirstOrDefaultAsync(d => d.Id == decisionId && d.ContractHistoryId == contractId && !d.IsDelete, cancellationToken);

            if (decision == null)
            {
                return RecruitmentResult<ContractDecisionResponse>.NotFound("Keputusan kontrak tidak ditemukan.");
            }

            // B6: usulan diperbaiki saat Dikembalikan (Minta Revisi), lalu instance yang sama diajukan ulang. Usulan lama
            // berstatus Diajukan yang dibuat sebelum integrasi (GUID tanpa instance) juga boleh diperbaiki lalu dikirim.
            var hasLiveInstance = await _workflowIntegrationService.InstanceExistsAsync(decision.WorkflowInstanceId, cancellationToken);
            var isReturned = decision.DecisionStatus == ContractDecisionStatus.Returned;
            var isUnsentSubmitted = decision.DecisionStatus == ContractDecisionStatus.Submitted && !hasLiveInstance;

            if (!isReturned && !isUnsentSubmitted)
            {
                return RecruitmentResult<ContractDecisionResponse>.BadRequest(
                    decision.DecisionStatus == ContractDecisionStatus.Submitted
                        ? "Usulan sedang menunggu persetujuan dan tidak dapat diubah. Tarik usulan bila perlu perubahan."
                        : $"Keputusan tidak dapat diubah karena berstatus {decision.DecisionStatus}.");
            }

            if (decision.SubmittedByUserId != actorId)
            {
                return RecruitmentResult<ContractDecisionResponse>.Forbidden(
                    "Hanya pengaju yang dapat memperbaiki dan mengajukan ulang usulan ini.");
            }

            // AUD-08-04: Validasi ulang kebijakan sesuai tipe keputusan yang dimutakhirkan
            if (request.DecisionType == ContractDecisionType.Extend)
            {
                if (decision.ContractHistory != null)
                {
                    var validation = _fixedTermPolicyService.ValidateRenewalProposal(
                        decision.ContractHistory,
                        request.ProposedStartDate,
                        request.ProposedEndDate,
                        request.ExtensionMonths);

                    if (!validation.IsValid)
                    {
                        return RecruitmentResult<ContractDecisionResponse>.BadRequest(validation.ErrorMessage!);
                    }
                }
            }
            else if (request.DecisionType == ContractDecisionType.ConvertToPermanent)
            {
                var permanentType = await _dbContext.MstContractTypes
                    .FirstOrDefaultAsync(ct => !ct.RequiresEndDate && ct.IsActive && !ct.IsDelete, cancellationToken);

                if (permanentType == null)
                {
                    return RecruitmentResult<ContractDecisionResponse>.BadRequest(
                        "Master jenis kontrak PKWTT aktif (RequiresEndDate == false) tidak ditemukan. Konversi ke pegawai tetap tidak dapat diproses.");
                }
                request.ProposedEndDate = null;
            }
            else if (request.DecisionType == ContractDecisionType.NotRenew)
            {
                if (string.IsNullOrWhiteSpace(request.ReasonText))
                {
                    return RecruitmentResult<ContractDecisionResponse>.BadRequest(
                        "Alasan tidak memperpanjang kontrak (ReasonText) wajib diisi untuk keputusan tidak perpanjang.");
                }
            }

            decision.DecisionType = request.DecisionType;
            decision.ExtensionMonths = request.ExtensionMonths;
            decision.ProposedStartDate = request.ProposedStartDate;
            decision.ProposedEndDate = request.ProposedEndDate;
            decision.ReasonText = request.ReasonText;
            decision.UpdateDateTime = DateTime.UtcNow;
            decision.UpdateBy = actorId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            var workflow = isReturned && hasLiveInstance
                ? await _workflowIntegrationService.ResubmitAsync(
                    decision.WorkflowInstanceId!.Value,
                    "Usulan diperbaiki dan diajukan ulang.",
                    cancellationToken)
                : await _workflowIntegrationService.StartContractDecisionWorkflowAsync(decision.Id, actorId, cancellationToken);

            if (!workflow.Success)
            {
                // Isian perbaikan tetap tersimpan; status tidak berubah sehingga pengaju dapat mencoba lagi.
                return RecruitmentResult<ContractDecisionResponse>.Fail(workflow.Message, workflow.StatusCode);
            }

            decision.WorkflowInstanceId = workflow.WorkflowInstanceId ?? decision.WorkflowInstanceId;
            decision.DecisionStatus = ContractDecisionStatus.Submitted;
            decision.SubmittedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return RecruitmentResult<ContractDecisionResponse>.Ok(
                MapToDecisionResponse(decision, decision.ContractHistory, user.Identity?.Name),
                "Usulan diperbaiki dan diajukan ulang ke alur persetujuan.");
        }

        public async Task<RecruitmentResult<bool>> WithdrawDecisionAsync(
            Guid contractId,
            Guid decisionId,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var actorId = GetUserId(user);

            var decision = await _dbContext.HrdContractDecisions
                .FirstOrDefaultAsync(d => d.Id == decisionId && d.ContractHistoryId == contractId && !d.IsDelete, cancellationToken);

            if (decision == null)
            {
                return RecruitmentResult<bool>.NotFound("Keputusan kontrak tidak ditemukan.");
            }

            if (decision.DecisionStatus != ContractDecisionStatus.Submitted &&
                decision.DecisionStatus != ContractDecisionStatus.Returned)
            {
                return RecruitmentResult<bool>.BadRequest(
                    $"Keputusan tidak dapat ditarik karena berstatus {decision.DecisionStatus}.");
            }

            // B6: instance ikut ditarik agar tugas tidak tertinggal di Kotak Masuk penyetuju.
            var workflow = await _workflowIntegrationService.WithdrawAsync(
                decision.WorkflowInstanceId,
                actorId,
                "Usulan keputusan kontrak ditarik oleh pengaju.",
                cancellationToken);
            if (!workflow.Success)
            {
                return RecruitmentResult<bool>.Fail(workflow.Message, workflow.StatusCode);
            }

            decision.DecisionStatus = ContractDecisionStatus.Withdrawn;
            decision.UpdateDateTime = DateTime.UtcNow;
            decision.UpdateBy = actorId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return RecruitmentResult<bool>.Ok(true, "Keputusan kontrak berhasil ditarik.");
        }

        public async Task<RecruitmentResult<ContractDecisionResponse>> RecordEmployeeNotifiedAsync(
            Guid contractId,
            Guid decisionId,
            RecordEmployeeNotifiedRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var actorId = GetUserId(user);

            var decision = await _dbContext.HrdContractDecisions
                .Include(d => d.ContractHistory)
                .FirstOrDefaultAsync(d => d.Id == decisionId && d.ContractHistoryId == contractId && !d.IsDelete, cancellationToken);

            if (decision == null)
            {
                return RecruitmentResult<ContractDecisionResponse>.NotFound("Keputusan kontrak tidak ditemukan.");
            }

            // AUD-08-07: Tolak tanggal pemberitahuan di masa depan
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (request.NotifiedOn > today)
            {
                return RecruitmentResult<ContractDecisionResponse>.BadRequest("Tanggal pemberitahuan tidak boleh lebih besar dari hari ini.");
            }

            var channel = !string.IsNullOrWhiteSpace(request.DeliveryChannel) ? request.DeliveryChannel : "Surat Tertulis";
            decision.EmployeeNotifiedOn = request.NotifiedOn;
            decision.NotificationRecordedByUserId = actorId;

            // BE-HRD-419: status kepatuhan H-7 hanya dari artefak bukti; catatan teks tidak pernah menghasilkan ON_TIME.
            var (complianceStatus, reqDate) = ComputeNoticeCompliance(decision, decision.ContractHistory, today);
            var noteDetail = $"[Pemberitahuan: Status={complianceStatus}, Kanal={channel}, Tgl={request.NotifiedOn:dd-MM-yyyy}, Catatan={request.EvidenceNotes ?? "-"}]";
            decision.DecisionNote = string.IsNullOrWhiteSpace(decision.DecisionNote)
                ? noteDetail
                : $"{decision.DecisionNote} | {noteDetail}";
            decision.UpdateDateTime = DateTime.UtcNow;
            decision.UpdateBy = actorId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            var response = MapToDecisionResponse(decision, decision.ContractHistory, user.Identity?.Name);
            response.NotificationChannel = channel;
            response.NotificationEvidenceNotes = request.EvidenceNotes;
            response.NotificationComplianceStatus = complianceStatus;
            response.RequiredNoticeDate = reqDate;
            return RecruitmentResult<ContractDecisionResponse>.Ok(response);
        }

        /// <summary>
        /// BE-HRD-419: unggah artefak bukti pemberitahuan tertulis (surat bertanda terima, tangkapan kanal resmi).
        /// Artefak tidak dapat diganti setelah tersimpan (immutable); salah unggah dikoreksi lewat keputusan baru.
        /// </summary>
        public async Task<RecruitmentResult<ContractDecisionResponse>> UploadNoticeEvidenceAsync(
            Guid contractId,
            Guid decisionId,
            Microsoft.AspNetCore.Http.IFormFile? file,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var actorId = GetUserId(user);

            var decision = await _dbContext.HrdContractDecisions
                .Include(d => d.ContractHistory)
                .FirstOrDefaultAsync(d => d.Id == decisionId && d.ContractHistoryId == contractId && !d.IsDelete, cancellationToken);

            if (decision == null)
            {
                return RecruitmentResult<ContractDecisionResponse>.NotFound("Keputusan kontrak tidak ditemukan.");
            }

            if (!string.IsNullOrWhiteSpace(decision.NoticeEvidencePath))
            {
                return RecruitmentResult<ContractDecisionResponse>.Conflict("Bukti pemberitahuan sudah tersimpan dan tidak dapat diganti.");
            }

            if (file == null || file.Length == 0)
            {
                return RecruitmentResult<ContractDecisionResponse>.BadRequest("Berkas bukti pemberitahuan wajib diunggah.");
            }

            var saveResult = await _attachmentService.ValidateAndSaveAttachmentAsync(file, "contract-notice-evidence", cancellationToken);
            if (!saveResult.IsSuccess)
            {
                return RecruitmentResult<ContractDecisionResponse>.BadRequest(saveResult.ErrorMessage ?? "Berkas bukti pemberitahuan tidak valid.");
            }

            decision.NoticeEvidencePath = saveResult.StoragePath;
            decision.NoticeEvidenceFileName = saveResult.FileName;
            decision.NoticeEvidenceUploadedAt = DateTime.UtcNow;
            decision.NoticeEvidenceUploadedByUserId = actorId;
            decision.UpdateDateTime = DateTime.UtcNow;
            decision.UpdateBy = actorId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return RecruitmentResult<ContractDecisionResponse>.Ok(
                MapToDecisionResponse(decision, decision.ContractHistory, user.Identity?.Name));
        }

        /// <summary>
        /// BE-HRD-419: status pemberitahuan tertulis H-7 (UU 27/2026 Pasal 82).
        /// <list type="bullet">
        /// <item><c>ON_TIME</c>/<c>LATE</c> — tanggal pemberitahuan tercatat <b>dan</b> artefak bukti tersimpan.</item>
        /// <item><c>LATE</c> — tanpa artefak, tenggat H-7 sudah lewat (ditandai; worker mengeskalasi).</item>
        /// <item><c>MISSING</c> — tanpa artefak, tenggat belum lewat.</item>
        /// <item><c>NOT_APPLICABLE</c> — kontrak tanpa tanggal berakhir.</item>
        /// </list>
        /// </summary>
        internal static (string Status, DateOnly? RequiredNoticeDate) ComputeNoticeCompliance(
            HrdContractDecision decision,
            WfpContractHistory? contract,
            DateOnly today)
        {
            if (contract?.EndDate == null)
            {
                return ("NOT_APPLICABLE", null);
            }

            var requiredDate = DateOnly.FromDateTime(contract.EndDate.Value).AddDays(-7);
            var hasArtifact = !string.IsNullOrWhiteSpace(decision.NoticeEvidencePath);

            if (decision.EmployeeNotifiedOn.HasValue && hasArtifact)
            {
                return (decision.EmployeeNotifiedOn.Value <= requiredDate ? "ON_TIME" : "LATE", requiredDate);
            }

            return (today > requiredDate ? "LATE" : "MISSING", requiredDate);
        }

        public async Task<RecruitmentResult<bool>> ApplyDecisionAsync(
            Guid decisionId,
            Guid deciderUserId,
            bool isApproved,
            string? decisionNote,
            CancellationToken cancellationToken = default)
        {
            var decision = await _dbContext.HrdContractDecisions
                .Include(d => d.ContractHistory)
                .FirstOrDefaultAsync(d => d.Id == decisionId && !d.IsDelete, cancellationToken);

            if (decision == null)
            {
                return RecruitmentResult<bool>.NotFound("Keputusan kontrak tidak ditemukan.");
            }

            // HRD-DEC-031 / FIN-HRD-08-BE-005: Maker-Checker guard di backend
            if (decision.SubmittedByUserId == deciderUserId)
            {
                return RecruitmentResult<bool>.BadRequest(
                    "Pembuat pengajuan keputusan tidak dapat menyetujui atau menolak keputusannya sendiri (Prinsip Maker-Checker HRD-DEC-031).");
            }

            // AUD-08-06: Idempotency lengkap untuk Approved, Rejected, dan Withdrawn
            if (decision.DecisionStatus == ContractDecisionStatus.Approved)
            {
                if (decision.DecisionType == ContractDecisionType.NotRenew && decision.EmployeeSeparationId.HasValue)
                {
                    return RecruitmentResult<bool>.Ok(true, "Keputusan pengakhiran kontrak sudah pernah disetujui sebelumnya.");
                }
                if ((decision.DecisionType == ContractDecisionType.Extend || decision.DecisionType == ContractDecisionType.ConvertToPermanent) && decision.FollowUpContractHistoryId.HasValue)
                {
                    return RecruitmentResult<bool>.Ok(true, "Keputusan perpanjangan/konversi kontrak sudah pernah disetujui sebelumnya.");
                }
            }
            if (decision.DecisionStatus == ContractDecisionStatus.Rejected)
            {
                return RecruitmentResult<bool>.Ok(true, "Keputusan kontrak sudah berstatus ditolak sebelumnya.");
            }
            if (decision.DecisionStatus == ContractDecisionStatus.Withdrawn)
            {
                return RecruitmentResult<bool>.BadRequest("Keputusan kontrak berstatus ditarik (Withdrawn) dan tidak dapat diproses.");
            }

            await using var tx = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                decision.DecisionStatus = isApproved ? ContractDecisionStatus.Approved : ContractDecisionStatus.Rejected;
                decision.DecidedByUserId = deciderUserId;
                decision.DecidedAt = DateTime.UtcNow;
                decision.DecisionNote = decisionNote;
                decision.UpdateDateTime = DateTime.UtcNow;
                decision.UpdateBy = deciderUserId;

                if (isApproved && decision.ContractHistory != null)
                {
                    if (decision.DecisionType == ContractDecisionType.Extend || decision.DecisionType == ContractDecisionType.ConvertToPermanent)
                    {
                        var followUp = await CreateFollowUpDraftAsync(
                            decision.ContractHistoryId,
                            decision.DecisionType,
                            decision.ProposedStartDate,
                            decision.ProposedEndDate,
                            deciderUserId,
                            cancellationToken);

                        decision.FollowUpContractHistoryId = followUp.Id;
                    }
                    else if (decision.DecisionType == ContractDecisionType.NotRenew)
                    {
                        var lastWorkingDate = decision.ContractHistory.EndDate.HasValue
                            ? DateOnly.FromDateTime(decision.ContractHistory.EndDate.Value)
                            : DateOnly.FromDateTime(DateTime.UtcNow);

                        var handoff = await _resignationHandoffService.HandoffLifecycleSeparationAsync(
                            decision.WorkforceProfileId,
                            null,
                            "ContractNonRenewal",
                            lastWorkingDate,
                            decision.ReasonText ?? "Kontrak kerja berakhir dan tidak diperpanjang.",
                            decision.DecisionNote,
                            deciderUserId,
                            cancellationToken);

                        if (handoff.Success && handoff.Data != null)
                        {
                            decision.EmployeeSeparationId = handoff.Data.EmployeeSeparationId;
                        }
                    }
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
                await tx.CommitAsync(cancellationToken);
                return RecruitmentResult<bool>.Ok(true, "Penerapan keputusan kontrak berhasil dieksekusi.");
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync(cancellationToken);
                return RecruitmentResult<bool>.BadRequest($"Gagal menerapkan keputusan kontrak: {ex.Message}");
            }
        }

        public async Task<WfpContractHistory> CreateFollowUpDraftAsync(
            Guid parentContractId,
            ContractDecisionType decisionType,
            DateOnly startDate,
            DateOnly? endDate,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var parent = await _dbContext.WfpContractHistories
                .Include(c => c.ContractType)
                .FirstAsync(c => c.Id == parentContractId, cancellationToken);

            Guid targetContractTypeId = parent.ContractTypeId ?? Guid.Empty;
            string historyType = "Renewal";

            if (decisionType == ContractDecisionType.ConvertToPermanent)
            {
                historyType = "Conversion";
                var permanentType = await _dbContext.MstContractTypes
                    .FirstOrDefaultAsync(ct => !ct.RequiresEndDate && ct.IsActive && !ct.IsDelete, cancellationToken);

                if (permanentType == null)
                {
                    throw new InvalidOperationException("Master jenis kontrak PKWTT aktif (RequiresEndDate == false) tidak ditemukan. Konversi ke pegawai tetap tidak dapat diproses (FIN-HRD-08-BE-007).");
                }

                targetContractTypeId = permanentType.Id;
                endDate = null; // PKWTT tidak memiliki tanggal akhir
            }

            var nextSeq = parent.RenewalSequence + 1;
            var contractNumber = $"{parent.ContractNumber}-R{nextSeq}";
            if (contractNumber.Length > 50) contractNumber = contractNumber[..50];

            var startDateTime = startDate.ToDateTime(TimeOnly.MinValue);
            var endDateTime = endDate?.ToDateTime(TimeOnly.MinValue);

            var followUp = new WfpContractHistory
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = parent.WorkforceProfileId,
                PreviousContractHistoryId = parent.Id,
                RenewalSequence = nextSeq,
                HistoryType = historyType,
                ContractStatus = EmploymentLifecycleValueConstants.ContractStatusDraft,
                ContractTypeId = targetContractTypeId,
                EmploymentTypeId = parent.EmploymentTypeId,
                ContractNumber = contractNumber,
                StartDate = startDateTime,
                EndDate = endDateTime,
                FixedTermSeriesStartDate = decisionType == ContractDecisionType.ConvertToPermanent ? null : (parent.FixedTermSeriesStartDate ?? parent.StartDate),
                ProbationEndDate = null, // Conversion / Renewal tanpa masa percobaan baru
                IsCurrent = false,
                Description = $"Kontrak lanjutan ({historyType}) dari kontrak {parent.ContractNumber}",
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };

            await _contractWriteService.CreateRenewalContractAsync(followUp, actorUserId, cancellationToken);
            return followUp;
        }

        // --- BE-HRD-081: Koreksi Kontrak T17 ---

        public async Task<RecruitmentResult<ContractCorrectionResponse>> SubmitCorrectionAsync(
            Guid contractId,
            SubmitContractCorrectionRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var actorId = GetUserId(user);

            var contract = await _dbContext.WfpContractHistories
                .Include(c => c.ContractType)
                .Include(c => c.WorkforceProfile)
                .FirstOrDefaultAsync(c => c.Id == contractId && !c.IsDelete, cancellationToken);

            if (contract == null)
            {
                return RecruitmentResult<ContractCorrectionResponse>.NotFound("Kontrak kerja tidak ditemukan.");
            }

            // VR-ONB-41: Hanya untuk kontrak Signed
            if (contract.ContractStatus != EmploymentLifecycleValueConstants.ContractStatusSigned &&
                contract.ContractStatus != "Active")
            {
                return RecruitmentResult<ContractCorrectionResponse>.BadRequest(
                    $"VR-ONB-41: Koreksi hanya dapat diajukan untuk kontrak yang sudah bertanda tangan. Status saat ini: {contract.ContractStatus}.");
            }

            // VR-ONB-42: Larangan koreksi jenis kontrak jika sudah ada keputusan atau masa percobaan
            if (contract.ContractTypeId.HasValue && contract.ContractTypeId.Value != request.AfterContractTypeId)
            {
                var hasDecisions = await _dbContext.HrdContractDecisions
                    .AnyAsync(d => d.ContractHistoryId == contractId && d.DecisionStatus == ContractDecisionStatus.Approved && !d.IsDelete, cancellationToken);

                if (hasDecisions)
                {
                    return RecruitmentResult<ContractCorrectionResponse>.BadRequest(
                        "VR-ONB-42: Jenis kontrak tidak dapat dikoreksi karena sudah memiliki keputusan kontrak yang telah disetujui.");
                }
            }

            // VR-ONB-43: Wajib bukti
            if (request.EvidenceFile == null || request.EvidenceFile.Length == 0)
            {
                return RecruitmentResult<ContractCorrectionResponse>.BadRequest(
                    "VR-ONB-43: Pengajuan koreksi kontrak wajib melampirkan berkas bukti dokumen perbaikan.");
            }

            var saveResult = await _attachmentService.ValidateAndSaveAttachmentAsync(
                request.EvidenceFile,
                "contract-corrections",
                cancellationToken);

            if (!saveResult.IsSuccess)
            {
                return RecruitmentResult<ContractCorrectionResponse>.BadRequest(
                    saveResult.ErrorMessage ?? "Gagal menyimpan berkas bukti koreksi kontrak.");
            }

            // Cek 1 koreksi terbuka
            var openCorrection = await _dbContext.HrdContractCorrections
                .FirstOrDefaultAsync(c => c.ContractHistoryId == contractId &&
                                         (c.CorrectionStatus == ContractCorrectionStatus.Submitted || c.CorrectionStatus == ContractCorrectionStatus.Returned) &&
                                         !c.IsDelete, cancellationToken);

            if (openCorrection != null)
            {
                return RecruitmentResult<ContractCorrectionResponse>.BadRequest(
                    $"Sudah ada pengajuan koreksi yang terbuka untuk kontrak ini (Nomor: {openCorrection.CorrectionNumber}).");
            }

            var correctionNumber = await _numberSeriesService.GenerateCorrectionNumberAsync(cancellationToken);

            var afterType = await _dbContext.MstContractTypes
                .FirstOrDefaultAsync(ct => ct.Id == request.AfterContractTypeId, cancellationToken);

            var correction = new HrdContractCorrection
            {
                Id = Guid.NewGuid(),
                CorrectionNumber = correctionNumber,
                ContractHistoryId = contractId,
                BeforeContractNumber = contract.ContractNumber,
                BeforeContractTypeId = contract.ContractTypeId ?? Guid.Empty,
                BeforeStartDate = DateOnly.FromDateTime(contract.StartDate),
                BeforeEndDate = contract.EndDate.HasValue ? DateOnly.FromDateTime(contract.EndDate.Value) : null,
                BeforeDocumentPath = contract.DocumentPath,
                AfterContractNumber = request.AfterContractNumber,
                AfterContractTypeId = request.AfterContractTypeId,
                AfterStartDate = request.AfterStartDate,
                AfterEndDate = request.AfterEndDate,
                ReasonText = request.ReasonText,
                EvidenceStoragePath = saveResult.StoragePath!,
                EvidenceFileName = saveResult.FileName!,
                EvidenceContentType = saveResult.ContentType!,
                CorrectionStatus = ContractCorrectionStatus.Submitted,
                SubmittedByUserId = actorId,
                SubmittedAt = DateTime.UtcNow,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorId
            };

            _dbContext.HrdContractCorrections.Add(correction);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Pemicu alur persetujuan T17 (B6), gagal tertutup seperti T16.
            var workflow = await _workflowIntegrationService.StartContractCorrectionWorkflowAsync(
                correction.Id,
                actorId,
                cancellationToken);

            if (!workflow.Success)
            {
                correction.IsDelete = true;
                correction.DeleteDateTime = DateTime.UtcNow;
                correction.DeleteBy = actorId;
                await _dbContext.SaveChangesAsync(cancellationToken);
                return RecruitmentResult<ContractCorrectionResponse>.Fail(workflow.Message, workflow.StatusCode);
            }

            correction.WorkflowInstanceId = workflow.WorkflowInstanceId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return RecruitmentResult<ContractCorrectionResponse>.Created(new ContractCorrectionResponse
            {
                Id = correction.Id,
                CorrectionNumber = correction.CorrectionNumber,
                ContractHistoryId = contractId,
                BeforeContractNumber = correction.BeforeContractNumber,
                BeforeContractTypeId = correction.BeforeContractTypeId,
                BeforeContractTypeName = contract.ContractType?.ContractTypeName ?? string.Empty,
                BeforeStartDate = correction.BeforeStartDate,
                BeforeEndDate = correction.BeforeEndDate,
                AfterContractNumber = correction.AfterContractNumber,
                AfterContractTypeId = correction.AfterContractTypeId,
                AfterContractTypeName = afterType?.ContractTypeName ?? string.Empty,
                AfterStartDate = correction.AfterStartDate,
                AfterEndDate = correction.AfterEndDate,
                ReasonText = correction.ReasonText,
                EvidenceFileName = correction.EvidenceFileName,
                CorrectionStatus = correction.CorrectionStatus,
                SubmittedByUserId = actorId,
                SubmittedByUserName = user.Identity?.Name ?? string.Empty,
                SubmittedAt = correction.SubmittedAt
            });
        }

        public async Task<RecruitmentResult<ContractCorrectionResponse>> GetCorrectionByIdAsync(
            Guid contractId,
            Guid correctionId,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var correction = await _dbContext.HrdContractCorrections
                .Include(c => c.BeforeContractType)
                .Include(c => c.AfterContractType)
                .Include(c => c.SubmittedByUser)
                .Include(c => c.DecidedByUser)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == correctionId && c.ContractHistoryId == contractId && !c.IsDelete, cancellationToken);

            if (correction == null)
            {
                return RecruitmentResult<ContractCorrectionResponse>.NotFound("Pengajuan koreksi kontrak tidak ditemukan.");
            }

            return RecruitmentResult<ContractCorrectionResponse>.Ok(new ContractCorrectionResponse
            {
                Id = correction.Id,
                CorrectionNumber = correction.CorrectionNumber,
                ContractHistoryId = contractId,
                BeforeContractNumber = correction.BeforeContractNumber,
                BeforeContractTypeId = correction.BeforeContractTypeId,
                BeforeContractTypeName = correction.BeforeContractType?.ContractTypeName ?? string.Empty,
                BeforeStartDate = correction.BeforeStartDate,
                BeforeEndDate = correction.BeforeEndDate,
                AfterContractNumber = correction.AfterContractNumber,
                AfterContractTypeId = correction.AfterContractTypeId,
                AfterContractTypeName = correction.AfterContractType?.ContractTypeName ?? string.Empty,
                AfterStartDate = correction.AfterStartDate,
                AfterEndDate = correction.AfterEndDate,
                ReasonText = correction.ReasonText,
                EvidenceFileName = correction.EvidenceFileName,
                CorrectionStatus = correction.CorrectionStatus,
                SubmittedByUserId = correction.SubmittedByUserId,
                SubmittedByUserName = correction.SubmittedByUser?.DisplayName ?? string.Empty,
                SubmittedAt = correction.SubmittedAt,
                DecidedByUserId = correction.DecidedByUserId,
                DecidedByUserName = correction.DecidedByUser?.DisplayName,
                DecidedAt = correction.DecidedAt,
                DecisionNote = correction.DecisionNote,
                AppliedAt = correction.AppliedAt
            });
        }

        public async Task<RecruitmentResult<bool>> WithdrawCorrectionAsync(
            Guid contractId,
            Guid correctionId,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var actorId = GetUserId(user);

            var correction = await _dbContext.HrdContractCorrections
                .FirstOrDefaultAsync(c => c.Id == correctionId && c.ContractHistoryId == contractId && !c.IsDelete, cancellationToken);

            if (correction == null)
            {
                return RecruitmentResult<bool>.NotFound("Pengajuan koreksi kontrak tidak ditemukan.");
            }

            if (correction.CorrectionStatus != ContractCorrectionStatus.Submitted &&
                correction.CorrectionStatus != ContractCorrectionStatus.Returned)
            {
                return RecruitmentResult<bool>.BadRequest(
                    $"Koreksi tidak dapat ditarik karena berstatus {correction.CorrectionStatus}.");
            }

            // B6: instance T17 ikut ditarik dari Kotak Masuk penyetuju.
            var workflow = await _workflowIntegrationService.WithdrawAsync(
                correction.WorkflowInstanceId,
                actorId,
                "Pengajuan koreksi kontrak ditarik oleh pengaju.",
                cancellationToken);
            if (!workflow.Success)
            {
                return RecruitmentResult<bool>.Fail(workflow.Message, workflow.StatusCode);
            }

            correction.CorrectionStatus = ContractCorrectionStatus.Withdrawn;
            correction.UpdateDateTime = DateTime.UtcNow;
            correction.UpdateBy = actorId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return RecruitmentResult<bool>.Ok(true, "Pengajuan koreksi kontrak berhasil ditarik.");
        }

        public async Task<RecruitmentResult<bool>> ApplyCorrectionAsync(
            Guid correctionId,
            Guid deciderUserId,
            bool isApproved,
            string? decisionNote,
            CancellationToken cancellationToken = default)
        {
            var correction = await _dbContext.HrdContractCorrections
                .FirstOrDefaultAsync(c => c.Id == correctionId && !c.IsDelete, cancellationToken);

            if (correction == null)
            {
                return RecruitmentResult<bool>.NotFound("Pengajuan koreksi tidak ditemukan.");
            }

            // HRD-DEC-089 butir 11: petugas HRD lain yang menyetujui (B6-5).
            if (correction.SubmittedByUserId == deciderUserId)
            {
                return RecruitmentResult<bool>.BadRequest(
                    "Pembuat pengajuan koreksi tidak dapat menyetujui atau menolak koreksinya sendiri (Prinsip Maker-Checker HRD-DEC-031).");
            }

            // Idempoten: keputusan yang sudah diterapkan tidak dijalankan ulang (B6-5).
            if (correction.CorrectionStatus is ContractCorrectionStatus.Approved or ContractCorrectionStatus.Rejected)
            {
                return RecruitmentResult<bool>.Ok(true, "Keputusan koreksi kontrak sudah pernah diterapkan sebelumnya.");
            }

            if (correction.CorrectionStatus == ContractCorrectionStatus.Withdrawn)
            {
                return RecruitmentResult<bool>.BadRequest("Pengajuan koreksi berstatus ditarik dan tidak dapat diproses.");
            }

            correction.CorrectionStatus = isApproved ? ContractCorrectionStatus.Approved : ContractCorrectionStatus.Rejected;
            correction.DecidedByUserId = deciderUserId;
            correction.DecidedAt = DateTime.UtcNow;
            correction.DecisionNote = decisionNote;
            correction.UpdateDateTime = DateTime.UtcNow;
            correction.UpdateBy = deciderUserId;

            if (isApproved)
            {
                var contract = await _dbContext.WfpContractHistories
                    .FirstOrDefaultAsync(c => c.Id == correction.ContractHistoryId && !c.IsDelete, cancellationToken);

                if (contract != null)
                {
                    contract.ContractNumber = correction.AfterContractNumber;
                    contract.ContractTypeId = correction.AfterContractTypeId;
                    contract.StartDate = correction.AfterStartDate.ToDateTime(TimeOnly.MinValue);
                    contract.EndDate = correction.AfterEndDate?.ToDateTime(TimeOnly.MinValue);
                    contract.DocumentPath = correction.EvidenceStoragePath;
                    contract.DocumentFileName = correction.EvidenceFileName;
                    contract.DocumentContentType = correction.EvidenceContentType;
                    contract.RowVersion = Guid.NewGuid();
                    contract.UpdateDateTime = DateTime.UtcNow;
                    contract.UpdateBy = deciderUserId;

                    correction.AppliedAt = DateTime.UtcNow;
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return RecruitmentResult<bool>.Ok(true, "Penerapan koreksi kontrak berhasil dieksekusi.");
        }

        // --- Scan Susulan Kontrak Data Awal (VR-ONB-45) ---

        public async Task<RecruitmentResult<EmploymentContractResponse>> UploadLegacyDocumentAsync(
            Guid contractId,
            UploadLegacyContractDocumentRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var actorId = GetUserId(user);

            var contract = await _dbContext.WfpContractHistories
                .Include(c => c.ContractType)
                .Include(c => c.WorkforceProfile)
                .FirstOrDefaultAsync(c => c.Id == contractId && !c.IsDelete, cancellationToken);

            if (contract == null)
            {
                return RecruitmentResult<EmploymentContractResponse>.NotFound("Kontrak kerja tidak ditemukan.");
            }

            // VR-ONB-45: Scan susulan hanya untuk kontrak data awal yang belum punya scan
            if (contract.HistoryType != "InitialData" && contract.SourceDocumentType != "InitialData")
            {
                return RecruitmentResult<EmploymentContractResponse>.BadRequest(
                    "VR-ONB-45: Unggah dokumen susulan hanya diperuntukkan bagi kontrak dari migrasi data awal (InitialData).");
            }

            if (!string.IsNullOrWhiteSpace(contract.DocumentPath))
            {
                return RecruitmentResult<EmploymentContractResponse>.BadRequest(
                    "VR-ONB-45: Kontrak ini sudah memiliki berkas scan dokumen yang tersimpan.");
            }

            var saveResult = await _attachmentService.ValidateAndSaveAttachmentAsync(
                request.DocumentFile,
                "legacy-contracts",
                cancellationToken);

            if (!saveResult.IsSuccess)
            {
                return RecruitmentResult<EmploymentContractResponse>.BadRequest(
                    saveResult.ErrorMessage ?? "Gagal menyimpan berkas scan dokumen kontrak.");
            }

            contract.DocumentPath = saveResult.StoragePath;
            contract.DocumentFileName = saveResult.FileName;
            contract.DocumentContentType = saveResult.ContentType;
            contract.UpdateDateTime = DateTime.UtcNow;
            contract.UpdateBy = actorId;
            contract.RowVersion = Guid.NewGuid();

            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetByIdAsync(contract.Id, cancellationToken);
        }

        private static ContractDecisionResponse MapToDecisionResponse(
            HrdContractDecision d,
            WfpContractHistory? c,
            string? submitterName)
        {
            var (complianceStatus, reqDate) = ComputeNoticeCompliance(d, c, DateOnly.FromDateTime(DateTime.UtcNow));

            return new ContractDecisionResponse
            {
                Id = d.Id,
                DecisionNumber = d.DecisionNumber,
                ContractHistoryId = d.ContractHistoryId,
                ContractNumber = c?.ContractNumber ?? string.Empty,
                WorkforceProfileId = d.WorkforceProfileId,
                WorkforceCode = d.WorkforceProfile?.ProfileCode ?? string.Empty,
                FullName = d.WorkforceProfile?.DisplayName ?? string.Empty,
                RenewalSequence = c?.RenewalSequence ?? 0,
                DecisionType = d.DecisionType,
                ExtensionMonths = d.ExtensionMonths,
                ProposedStartDate = d.ProposedStartDate,
                ProposedEndDate = d.ProposedEndDate,
                ReasonText = d.ReasonText,
                DecisionStatus = d.DecisionStatus,
                SubmittedByUserId = d.SubmittedByUserId,
                SubmittedByUserName = d.SubmittedByUser?.DisplayName ?? submitterName ?? string.Empty,
                SubmittedAt = d.SubmittedAt,
                DecidedByUserId = d.DecidedByUserId,
                DecidedByUserName = d.DecidedByUser?.DisplayName,
                DecidedAt = d.DecidedAt,
                DecisionNote = d.DecisionNote,
                FollowUpContractHistoryId = d.FollowUpContractHistoryId,
                EmployeeNotifiedOn = d.EmployeeNotifiedOn,
                EmployeeSeparationId = d.EmployeeSeparationId,
                NotificationComplianceStatus = complianceStatus,
                RequiredNoticeDate = reqDate,
                NoticeEvidenceFileName = d.NoticeEvidenceFileName,
                FollowUpContractNumber = d.FollowUpContractHistory?.ContractNumber
            };
        }

        // --- FIN-HRD-08: Worklist Keputusan & Ringkasan Dashboard ---

        public async Task<RecruitmentResult<PagedResult<ContractDecisionResponse>>> GetDecisionsPagedAsync(
            ContractDecisionQuery query,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            query ??= new ContractDecisionQuery();
            var dbQuery = _dbContext.HrdContractDecisions
                .AsNoTracking()
                .Include(d => d.ContractHistory)
                .Include(d => d.WorkforceProfile)
                .Include(d => d.SubmittedByUser)
                .Include(d => d.DecidedByUser)
                .Include(d => d.FollowUpContractHistory)
                .Where(d => !d.IsDelete);

            if (query.ContractHistoryId.HasValue)
            {
                dbQuery = dbQuery.Where(d => d.ContractHistoryId == query.ContractHistoryId.Value);
            }

            if (query.DecisionType.HasValue)
            {
                dbQuery = dbQuery.Where(d => d.DecisionType == query.DecisionType.Value);
            }

            if (query.DecisionStatus.HasValue)
            {
                dbQuery = dbQuery.Where(d => d.DecisionStatus == query.DecisionStatus.Value);
            }

            if (query.WorkforceProfileId.HasValue)
            {
                dbQuery = dbQuery.Where(d => d.WorkforceProfileId == query.WorkforceProfileId.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var term = query.Search.Trim().ToLower();
                dbQuery = dbQuery.Where(d =>
                    (d.DecisionNumber != null && d.DecisionNumber.ToLower().Contains(term)) ||
                    (d.ContractHistory != null && d.ContractHistory.ContractNumber.ToLower().Contains(term)) ||
                    (d.WorkforceProfile != null && d.WorkforceProfile.DisplayName.ToLower().Contains(term)) ||
                    (d.WorkforceProfile != null && d.WorkforceProfile.ProfileCode.ToLower().Contains(term)));
            }

            var totalData = await dbQuery.CountAsync(cancellationToken);
            var pageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
            var pageSize = query.PageSize > 0 ? query.PageSize : 10;

            var items = await dbQuery
                .OrderByDescending(d => d.SubmittedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var responseItems = items.Select(d => MapToDecisionResponse(d, d.ContractHistory, null)).ToList();

            var paged = new PagedResult<ContractDecisionResponse>
            {
                Items = responseItems,
                TotalData = totalData,
                TotalPage = pageSize > 0 ? (int)Math.Ceiling((double)totalData / pageSize) : 0,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return RecruitmentResult<PagedResult<ContractDecisionResponse>>.Ok(paged, "Daftar keputusan kontrak berhasil diambil.");
        }

        public async Task<RecruitmentResult<EmploymentContractSummaryResponse>> GetContractSummaryAsync(
            Guid? hospitalSiteId,
            CancellationToken cancellationToken = default)
        {
            // BE-HRD-457: tanggal acuan dan jendela 60 hari berasal dari ContractRenewalBuckets (HRD-DEC-245).
            var today = ContractRenewalBuckets.ReferenceDate();

            var query = _dbContext.WfpContractHistories.AsNoTracking().Where(c => !c.IsDelete);

            if (hospitalSiteId.HasValue && hospitalSiteId.Value != Guid.Empty)
            {
                var siteProfileIds = await _dbContext.Set<QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models.WfpPositionAssignment>()
                    .Where(p => p.HospitalSiteId == hospitalSiteId.Value && !p.IsDelete)
                    .Select(p => p.WorkforceProfileId)
                    .Distinct()
                    .ToListAsync(cancellationToken);

                query = query.Where(c => siteProfileIds.Contains(c.WorkforceProfileId));
            }

            var activeCount = await query.CountAsync(c => c.IsCurrent && (c.ContractStatus == "Active" || c.ContractStatus == EmploymentLifecycleValueConstants.ContractStatusSigned), cancellationToken);
            var pendingSignedCount = await query.CountAsync(c => c.ContractStatus == EmploymentLifecycleValueConstants.ContractStatusDraft, cancellationToken);
            var expiringSoonCount = await query.CountAsync(ContractRenewalBuckets.ExpiringWithinWindow(today), cancellationToken);
            var needsReviewCount = await query.CountAsync(c => c.ContractStatus == "LapsedWithoutDecision" || (c.EndDate.HasValue && c.EndDate.Value < today && (c.ContractStatus == "Active" || c.ContractStatus == EmploymentLifecycleValueConstants.ContractStatusSigned)), cancellationToken);

            return RecruitmentResult<EmploymentContractSummaryResponse>.Ok(new EmploymentContractSummaryResponse
            {
                ActiveContractsCount = activeCount,
                PendingSignedCount = pendingSignedCount,
                ExpiringSoonCount = expiringSoonCount,
                NeedsReviewCount = needsReviewCount
            });
        }

        public async Task<RecruitmentResult<ContractRenewalSummaryResponse>> GetRenewalSummaryAsync(
            Guid? hospitalSiteId,
            CancellationToken cancellationToken = default)
        {
            // BE-HRD-457: definisi tahapan berasal dari ContractRenewalBuckets agar Dashboard SDM memakai aturan yang sama (HRD-DEC-245).
            var today = ContractRenewalBuckets.ReferenceDate();

            var query = _dbContext.WfpContractHistories
                .AsNoTracking()
                .Where(c => !c.IsDelete && c.IsCurrent && c.EndDate.HasValue && c.EndDate.Value >= today);

            var decisionQuery = _dbContext.HrdContractDecisions
                .AsNoTracking()
                .Where(ContractRenewalBuckets.PendingDecisionPredicate());

            var overdueQuery = _dbContext.WfpContractHistories
                .AsNoTracking()
                .Where(ContractRenewalBuckets.OverduePredicate(today));

            if (hospitalSiteId.HasValue && hospitalSiteId.Value != Guid.Empty)
            {
                var siteProfileIds = await _dbContext.Set<QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models.WfpPositionAssignment>()
                    .Where(p => p.HospitalSiteId == hospitalSiteId.Value && !p.IsDelete)
                    .Select(p => p.WorkforceProfileId)
                    .Distinct()
                    .ToListAsync(cancellationToken);

                query = query.Where(c => siteProfileIds.Contains(c.WorkforceProfileId));
                decisionQuery = decisionQuery.Where(d => siteProfileIds.Contains(d.WorkforceProfileId));
                overdueQuery = overdueQuery.Where(c => siteProfileIds.Contains(c.WorkforceProfileId));
            }

            var reminderH60Count = await query.CountAsync(ContractRenewalBuckets.Stage(ContractRenewalBuckets.H60, today), cancellationToken);
            var escalationH14Count = await query.CountAsync(ContractRenewalBuckets.Stage(ContractRenewalBuckets.H14, today), cancellationToken);
            var noticeDueH7Count = await query.CountAsync(ContractRenewalBuckets.Stage(ContractRenewalBuckets.H7, today), cancellationToken);
            var pendingDecisionCount = await decisionQuery.CountAsync(cancellationToken);
            var overdueCount = await overdueQuery.CountAsync(cancellationToken);

            return RecruitmentResult<ContractRenewalSummaryResponse>.Ok(new ContractRenewalSummaryResponse
            {
                ReminderH60Count = reminderH60Count,
                EscalationH14Count = escalationH14Count,
                NoticeDueH7Count = noticeDueH7Count,
                PendingDecisionCount = pendingDecisionCount,
                OverdueCount = overdueCount
            });
        }

        public async Task<RecruitmentResult<EmploymentStatusChangeSummaryResponse>> GetConversionSummaryAsync(
            Guid? hospitalSiteId,
            CancellationToken cancellationToken = default)
        {
            var conversionDecisions = _dbContext.HrdContractDecisions
                .AsNoTracking()
                .Where(d => !d.IsDelete && d.DecisionType == ContractDecisionType.ConvertToPermanent);

            if (hospitalSiteId.HasValue && hospitalSiteId.Value != Guid.Empty)
            {
                var siteProfileIds = await _dbContext.Set<QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models.WfpPositionAssignment>()
                    .Where(p => p.HospitalSiteId == hospitalSiteId.Value && !p.IsDelete)
                    .Select(p => p.WorkforceProfileId)
                    .Distinct()
                    .ToListAsync(cancellationToken);

                conversionDecisions = conversionDecisions.Where(d => siteProfileIds.Contains(d.WorkforceProfileId));
            }

            var submittedCount = await conversionDecisions.CountAsync(d => d.DecisionStatus == ContractDecisionStatus.Submitted, cancellationToken);
            var pendingSignedCount = await conversionDecisions.CountAsync(d => d.DecisionStatus == ContractDecisionStatus.Approved && d.FollowUpContractHistory != null && d.FollowUpContractHistory.ContractStatus == EmploymentLifecycleValueConstants.ContractStatusDraft, cancellationToken);
            var scheduledOrEffectiveCount = await conversionDecisions.CountAsync(d => d.DecisionStatus == ContractDecisionStatus.Approved && d.FollowUpContractHistory != null && (d.FollowUpContractHistory.ContractStatus == "Active" || d.FollowUpContractHistory.ContractStatus == EmploymentLifecycleValueConstants.ContractStatusSigned), cancellationToken);
            var rejectedCount = await conversionDecisions.CountAsync(d => d.DecisionStatus == ContractDecisionStatus.Rejected, cancellationToken);

            return RecruitmentResult<EmploymentStatusChangeSummaryResponse>.Ok(new EmploymentStatusChangeSummaryResponse
            {
                SubmittedCount = submittedCount,
                PendingSignedCount = pendingSignedCount,
                ScheduledOrEffectiveCount = scheduledOrEffectiveCount,
                RejectedCount = rejectedCount
            });
        }
    }
}
