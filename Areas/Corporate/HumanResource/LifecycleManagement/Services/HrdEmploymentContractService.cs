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
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

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

        public HrdEmploymentContractService(
            ApplicationDbContext dbContext,
            IWfpContractHistoryWriteService contractWriteService,
            IWfpNewHireAssignmentService newHireAssignmentService,
            IHrdProbationService probationService,
            IHrdLifecycleAttachmentService attachmentService,
            IHrdNumberSeriesService numberSeriesService,
            ResignationLifecycleHandoffService resignationHandoffService,
            IHrdEmploymentLifecycleWorkflowIntegrationService workflowIntegrationService)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _contractWriteService = contractWriteService ?? throw new ArgumentNullException(nameof(contractWriteService));
            _newHireAssignmentService = newHireAssignmentService ?? throw new ArgumentNullException(nameof(newHireAssignmentService));
            _probationService = probationService ?? throw new ArgumentNullException(nameof(probationService));
            _attachmentService = attachmentService ?? throw new ArgumentNullException(nameof(attachmentService));
            _numberSeriesService = numberSeriesService ?? throw new ArgumentNullException(nameof(numberSeriesService));
            _resignationHandoffService = resignationHandoffService ?? throw new ArgumentNullException(nameof(resignationHandoffService));
            _workflowIntegrationService = workflowIntegrationService ?? throw new ArgumentNullException(nameof(workflowIntegrationService));
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
            if (contractType != null && contractType.IsProbationApplicable)
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
            CancellationToken cancellationToken = default)
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
            CancellationToken cancellationToken = default)
        {
            var c = await _dbContext.WfpContractHistories
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
            var contract = await _dbContext.WfpContractHistories
                .Include(c => c.WorkforceProfile)
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

            var printData = new EmploymentContractPrintDataResponse
            {
                ContractId = contract.Id,
                ContractNumber = contract.ContractNumber,
                HospitalName = requisition?.HospitalSite?.SiteName ?? "Rumah Sakit Quilvian",
                EmployeeFullName = contract.WorkforceProfile?.DisplayName ?? string.Empty,
                EmployeeNationalId = contract.WorkforceProfile?.ProfileCode ?? "-",
                PositionTitle = offer?.OfferedPositionTitle ?? requisition?.Position?.PositionName ?? "-",
                DepartmentName = requisition?.OrganizationUnit?.UnitName ?? "-",
                ContractTypeName = contract.ContractType?.ContractTypeName ?? "PKWT",
                StartDate = contract.StartDate,
                EndDate = contract.EndDate,
                BasicSalary = offer?.OfferedBasicSalary,
                IsSalaryMasked = false
            };

            return RecruitmentResult<EmploymentContractPrintDataResponse>.Ok(printData);
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

                var offer = await _dbContext.HrdJobOffers
                    .Include(o => o.Application)
                        .ThenInclude(a => a!.Vacancy)
                            .ThenInclude(v => v!.Requisition)
                    .FirstOrDefaultAsync(o => o.Id == contract.SourceDocumentId, cancellationToken);

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
            catch (Exception)
            {
                await tx.RollbackAsync(cancellationToken);
                throw;
            }

            return await GetByIdAsync(contract.Id, cancellationToken);
        }

        public async Task<(bool Exists, Stream? Stream, string? ContentType, string? FileName)> DownloadScanAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var contract = await _dbContext.WfpContractHistories
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDelete, cancellationToken);

            if (contract == null || string.IsNullOrWhiteSpace(contract.DocumentPath))
            {
                return (false, null, null, null);
            }

            return await _attachmentService.GetAttachmentStreamAsync(contract.DocumentPath, cancellationToken);
        }

        // --- BE-HRD-080: Keputusan Kontrak T16 & Batas 5 Tahun PKWT ---

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

            var isFixedTerm = contract.ContractType?.RequiresEndDate ?? contract.EndDate.HasValue;
            if (!isFixedTerm || !contract.EndDate.HasValue)
            {
                return RecruitmentResult<FixedTermCheckResponse>.Ok(new FixedTermCheckResponse
                {
                    ContractHistoryId = contract.Id,
                    StartDate = DateOnly.FromDateTime(contract.StartDate),
                    EndDate = contract.EndDate.HasValue ? DateOnly.FromDateTime(contract.EndDate.Value) : null,
                    FixedTermSeriesStartDate = DateOnly.FromDateTime(contract.StartDate),
                    TotalUsedMonths = 0,
                    RemainingMonths = 0,
                    CanExtend = false,
                    MaxExtensionMonths = 0,
                    Message = "Bukan merupakan kontrak PKWT berjangka waktu (Pegawai Tetap/PKWTT)."
                });
            }

            var seriesStart = contract.FixedTermSeriesStartDate.HasValue
                ? DateOnly.FromDateTime(contract.FixedTermSeriesStartDate.Value)
                : DateOnly.FromDateTime(contract.StartDate);

            var end = DateOnly.FromDateTime(contract.EndDate.Value);
            var totalUsedMonths = ((end.Year - seriesStart.Year) * 12) + end.Month - seriesStart.Month;
            if (totalUsedMonths < 0) totalUsedMonths = 0;

            const int maxMonths = 60; // 5 tahun
            var remainingMonths = Math.Max(0, maxMonths - totalUsedMonths);
            var canExtend = remainingMonths > 0;

            return RecruitmentResult<FixedTermCheckResponse>.Ok(new FixedTermCheckResponse
            {
                ContractHistoryId = contract.Id,
                StartDate = DateOnly.FromDateTime(contract.StartDate),
                EndDate = end,
                FixedTermSeriesStartDate = seriesStart,
                TotalUsedMonths = totalUsedMonths,
                RemainingMonths = remainingMonths,
                CanExtend = canExtend,
                MaxExtensionMonths = remainingMonths,
                Message = canExtend
                    ? $"Sisa batas waktu PKWT maksimal 5 tahun adalah {remainingMonths} bulan."
                    : "Masa PKWT kumulatif telah mencapai batas maksimal 5 tahun. Kontrak harus diangkat menjadi pegawai tetap (PKWTT) atau tidak diperpanjang."
            });
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

            // VR-ONB-37: Satu keputusan terbuka (Submitted) atau disetujui per kontrak
            var existingDecision = await _dbContext.HrdContractDecisions
                .FirstOrDefaultAsync(d => d.ContractHistoryId == contractId &&
                                         (d.DecisionStatus == ContractDecisionStatus.Submitted || d.DecisionStatus == ContractDecisionStatus.Approved) &&
                                         !d.IsDelete, cancellationToken);

            if (existingDecision != null)
            {
                return RecruitmentResult<ContractDecisionResponse>.BadRequest(
                    $"VR-ONB-37: Kontrak ini sudah memiliki keputusan yang berstatus {existingDecision.DecisionStatus} (Nomor: {existingDecision.DecisionNumber}).");
            }

            // VR-ONB-36: Validasi batas 5 tahun jika Extend
            if (request.DecisionType == ContractDecisionType.Extend)
            {
                if (!request.ProposedEndDate.HasValue)
                {
                    return RecruitmentResult<ContractDecisionResponse>.BadRequest(
                        "VR-ONB-36: Tanggal berakhir usulan (ProposedEndDate) wajib diisi untuk perpanjangan PKWT.");
                }

                var seriesStart = contract.FixedTermSeriesStartDate.HasValue
                    ? DateOnly.FromDateTime(contract.FixedTermSeriesStartDate.Value)
                    : DateOnly.FromDateTime(contract.StartDate);

                var totalMonths = ((request.ProposedEndDate.Value.Year - seriesStart.Year) * 12) +
                                  request.ProposedEndDate.Value.Month - seriesStart.Month;

                if (totalMonths > 60)
                {
                    return RecruitmentResult<ContractDecisionResponse>.BadRequest(
                        $"VR-ONB-36: Perpanjangan kontrak menghasilkan total durasi PKWT {totalMonths} bulan, yang melebihi batas maksimal 5 tahun (60 bulan). Pertimbangkan pengangkatan Tetap (ConvertToPermanent).");
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

            // Pemicu alur persetujuan T16
            var workflowInstanceId = await _workflowIntegrationService.StartContractDecisionWorkflowAsync(
                decision.Id,
                actorId,
                cancellationToken);

            decision.WorkflowInstanceId = workflowInstanceId;
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

            if (decision.DecisionStatus != ContractDecisionStatus.Submitted)
            {
                return RecruitmentResult<ContractDecisionResponse>.BadRequest(
                    $"Keputusan tidak dapat diubah karena berstatus {decision.DecisionStatus}.");
            }

            decision.DecisionType = request.DecisionType;
            decision.ExtensionMonths = request.ExtensionMonths;
            decision.ProposedStartDate = request.ProposedStartDate;
            decision.ProposedEndDate = request.ProposedEndDate;
            decision.ReasonText = request.ReasonText;
            decision.UpdateDateTime = DateTime.UtcNow;
            decision.UpdateBy = actorId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return RecruitmentResult<ContractDecisionResponse>.Ok(MapToDecisionResponse(decision, decision.ContractHistory, user.Identity?.Name));
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

            if (decision.DecisionStatus != ContractDecisionStatus.Submitted)
            {
                return RecruitmentResult<bool>.BadRequest(
                    $"Keputusan tidak dapat ditarik karena berstatus {decision.DecisionStatus}.");
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

            // VR-ONB-40: Pencatatan tanggal pemberitahuan
            decision.EmployeeNotifiedOn = request.NotifiedOn;
            decision.NotificationRecordedByUserId = actorId;
            decision.UpdateDateTime = DateTime.UtcNow;
            decision.UpdateBy = actorId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return RecruitmentResult<ContractDecisionResponse>.Ok(MapToDecisionResponse(decision, decision.ContractHistory, user.Identity?.Name));
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
            return RecruitmentResult<bool>.Ok(true, "Penerapan keputusan kontrak berhasil dieksekusi.");
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
                    .FirstOrDefaultAsync(ct => !ct.RequiresEndDate && !ct.IsDelete, cancellationToken);

                if (permanentType != null)
                {
                    targetContractTypeId = permanentType.Id;
                }
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
                .FirstOrDefaultAsync(c => c.ContractHistoryId == contractId && c.CorrectionStatus == ContractCorrectionStatus.Submitted && !c.IsDelete, cancellationToken);

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

            var wfId = await _workflowIntegrationService.StartContractCorrectionWorkflowAsync(
                correction.Id,
                actorId,
                cancellationToken);

            correction.WorkflowInstanceId = wfId;
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

            if (correction.CorrectionStatus != ContractCorrectionStatus.Submitted)
            {
                return RecruitmentResult<bool>.BadRequest(
                    $"Koreksi tidak dapat ditarik karena berstatus {correction.CorrectionStatus}.");
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
            return new ContractDecisionResponse
            {
                Id = d.Id,
                DecisionNumber = d.DecisionNumber,
                ContractHistoryId = d.ContractHistoryId,
                ContractNumber = c?.ContractNumber ?? string.Empty,
                WorkforceProfileId = d.WorkforceProfileId,
                WorkforceCode = d.WorkforceProfile?.ProfileCode ?? string.Empty,
                FullName = d.WorkforceProfile?.DisplayName ?? string.Empty,
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
                EmployeeSeparationId = d.EmployeeSeparationId
            };
        }
    }
}
