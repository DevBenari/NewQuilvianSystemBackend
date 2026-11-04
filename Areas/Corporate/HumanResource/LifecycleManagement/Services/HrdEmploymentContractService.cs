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

        public HrdEmploymentContractService(
            ApplicationDbContext dbContext,
            IWfpContractHistoryWriteService contractWriteService,
            IWfpNewHireAssignmentService newHireAssignmentService,
            IHrdProbationService probationService,
            IHrdLifecycleAttachmentService attachmentService,
            IHrdNumberSeriesService numberSeriesService)
        {
            _dbContext = dbContext;
            _contractWriteService = contractWriteService;
            _newHireAssignmentService = newHireAssignmentService;
            _probationService = probationService;
            _attachmentService = attachmentService;
            _numberSeriesService = numberSeriesService;
        }

        public async Task<WfpContractHistory> CreateDraftFromOfferAsync(
            Guid workforceProfileId,
            HrdJobOffer offer,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            if (offer == null) throw new ArgumentNullException(nameof(offer));

            // Periksa apakah draft kontrak untuk offer ini sudah pernah dibuat (Idempoten)
            var existingContract = await _dbContext.WfpContractHistories
                .FirstOrDefaultAsync(c => c.SourceDocumentType == EmploymentLifecycleValueConstants.SourceDocOffer &&
                                          c.SourceDocumentId == offer.Id &&
                                          !c.IsDelete, cancellationToken);

            if (existingContract != null)
            {
                return existingContract;
            }

            var startDate = offer.ProposedJoinDate.ToDateTime(TimeOnly.MinValue);
            DateTime? endDate = null;

            if (offer.ContractDurationMonths.HasValue && offer.ContractDurationMonths.Value > 0)
            {
                endDate = offer.ProposedJoinDate.AddMonths(offer.ContractDurationMonths.Value).AddDays(-1).ToDateTime(TimeOnly.MinValue);
            }

            var contractNumber = await _numberSeriesService.GenerateContractNumberAsync(cancellationToken);

            var contract = new WfpContractHistory
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = workforceProfileId,
                ContractTypeId = offer.ContractTypeId,
                ContractNumber = contractNumber,
                HistoryType = "Initial",
                ContractStatus = "Draft",
                StartDate = startDate,
                EndDate = endDate,
                SourceDocumentType = EmploymentLifecycleValueConstants.SourceDocOffer,
                SourceDocumentId = offer.Id,
                RowVersion = Guid.NewGuid(),
                IsCurrent = false,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = userId
            };

            return await _contractWriteService.CreateInitialContractAsync(contract, userId, cancellationToken);
        }

        public async Task<RecruitmentResult<PagedResult<EmploymentContractResponse>>> GetPagedAsync(
            EmploymentContractQuery query,
            CancellationToken cancellationToken = default)
        {
            var dbQuery = _dbContext.WfpContractHistories
                .AsNoTracking()
                .Include(c => c.WorkforceProfile)
                .Include(c => c.ContractType)
                .Where(c => !c.IsDelete);

            if (query.WorkforceProfileId.HasValue)
            {
                dbQuery = dbQuery.Where(c => c.WorkforceProfileId == query.WorkforceProfileId.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.ContractStatus))
            {
                dbQuery = dbQuery.Where(c => c.ContractStatus == query.ContractStatus);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var term = query.Search.Trim().ToLower();
                dbQuery = dbQuery.Where(c =>
                    c.ContractNumber.ToLower().Contains(term) ||
                    (c.WorkforceProfile != null && (c.WorkforceProfile.ProfileCode.ToLower().Contains(term) || c.WorkforceProfile.DisplayName.ToLower().Contains(term))));
            }

            var totalData = await dbQuery.CountAsync(cancellationToken);
            var pageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
            var pageSize = query.PageSize > 0 ? query.PageSize : 10;

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
                    RowVersion = c.RowVersion
                })
                .ToListAsync(cancellationToken);

            var paged = new PagedResult<EmploymentContractResponse>
            {
                Items = items,
                TotalData = totalData,
                TotalPage = pageSize > 0 ? (int)Math.Ceiling((double)totalData / pageSize) : 0,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return RecruitmentResult<PagedResult<EmploymentContractResponse>>.Ok(paged, "Daftar kontrak kerja berhasil diambil.");
        }

        public async Task<RecruitmentResult<EmploymentContractResponse>> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var c = await _dbContext.WfpContractHistories
                .AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.ContractType)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (c == null)
            {
                return RecruitmentResult<EmploymentContractResponse>.NotFound("Kontrak kerja tidak ditemukan.");
            }

            var response = new EmploymentContractResponse
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
                RowVersion = c.RowVersion
            };

            return RecruitmentResult<EmploymentContractResponse>.Ok(response, "Detail kontrak kerja berhasil diambil.");
        }

        public async Task<RecruitmentResult<EmploymentContractPrintDataResponse>> GetPrintDataAsync(
            Guid id,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var contract = await _dbContext.WfpContractHistories
                .Include(c => c.WorkforceProfile)
                .Include(c => c.ContractType)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDelete, cancellationToken);

            if (contract == null)
            {
                return RecruitmentResult<EmploymentContractPrintDataResponse>.NotFound("Kontrak kerja tidak ditemukan.");
            }

            var employee = await _dbContext.Set<MstEmployee>()
                .Include(e => e.PrimaryDepartment)
                .Include(e => e.PrimaryPosition)
                .FirstOrDefaultAsync(e => e.WorkforceProfileId == contract.WorkforceProfileId && !e.IsDelete, cancellationToken);

            HrdJobOffer? offer = null;
            if (contract.SourceDocumentType == EmploymentLifecycleValueConstants.SourceDocOffer && contract.SourceDocumentId.HasValue)
            {
                offer = await _dbContext.HrdJobOffers
                    .FirstOrDefaultAsync(o => o.Id == contract.SourceDocumentId.Value && !o.IsDelete, cancellationToken);
            }

            // Otorisasi nominal gaji: Hanya pemegang hak akses 'ViewAmount' yang melihat nominal riil (VR-ONB-31, AT-HRD-ONB-40)
            var canViewSalary = user.IsInRole("Administrator") ||
                                user.HasClaim("permission", "WorkforceSalaryAssignment.ViewAmount") ||
                                user.HasClaim("permission", "WfpSalaryAssignment.ViewAmount");

            decimal? salary = null;
            if (canViewSalary && offer != null)
            {
                salary = offer.OfferedBasicSalary;
            }

            var response = new EmploymentContractPrintDataResponse
            {
                ContractId = contract.Id,
                ContractNumber = contract.ContractNumber,
                HospitalName = "Rumah Sakit Quilvian",
                EmployeeFullName = employee?.FullName ?? contract.WorkforceProfile?.DisplayName ?? string.Empty,
                EmployeeNationalId = employee?.IdentityNumber ?? string.Empty,
                PositionTitle = offer?.OfferedPositionTitle ?? employee?.PrimaryPosition?.PositionName ?? string.Empty,
                DepartmentName = employee?.PrimaryDepartment?.DepartmentName ?? string.Empty,
                ContractTypeName = contract.ContractType?.ContractTypeName ?? string.Empty,
                StartDate = contract.StartDate,
                EndDate = contract.EndDate,
                BasicSalary = salary,
                IsSalaryMasked = !canViewSalary
            };

            return RecruitmentResult<EmploymentContractPrintDataResponse>.Ok(response, "Data cetak kontrak kerja berhasil disiapkan.");
        }

        public async Task<RecruitmentResult<EmploymentContractResponse>> RecordSignedAsync(
            Guid id,
            RecordContractSignedRequest request,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var contract = await _dbContext.WfpContractHistories
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDelete, cancellationToken);

            if (contract == null)
            {
                return RecruitmentResult<EmploymentContractResponse>.NotFound("Kontrak kerja tidak ditemukan.");
            }

            if (contract.ContractStatus != "Draft")
            {
                return RecruitmentResult<EmploymentContractResponse>.Unprocessable(
                    $"Hanya kontrak berstatus Draft yang dapat ditandatangani. Status saat ini: '{contract.ContractStatus}'.");
            }

            // Validasi file lampiran VR-ONB-30
            if (request.SignedScanFile == null || request.SignedScanFile.Length == 0)
            {
                return RecruitmentResult<EmploymentContractResponse>.Unprocessable(
                    "Penandatanganan kontrak wajib melampirkan berkas scan kontrak yang telah ditandatangani para pihak (VR-ONB-30).");
            }

            var fileSaveResult = await _attachmentService.ValidateAndSaveAttachmentAsync(
                request.SignedScanFile,
                "contracts",
                cancellationToken);

            if (!fileSaveResult.IsSuccess)
            {
                return RecruitmentResult<EmploymentContractResponse>.Unprocessable(
                    fileSaveResult.ErrorMessage ?? "Berkas lampiran tidak valid.");
            }

            // Buka onboarding case dan offer terkait
            var onboardingCase = await _dbContext.HrdOnboardingCases
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.WorkforceProfileId == contract.WorkforceProfileId && !c.IsDelete, cancellationToken);

            HrdJobOffer? offer = null;
            if (contract.SourceDocumentType == EmploymentLifecycleValueConstants.SourceDocOffer && contract.SourceDocumentId.HasValue)
            {
                offer = await _dbContext.HrdJobOffers
                    .FirstOrDefaultAsync(o => o.Id == contract.SourceDocumentId.Value && !o.IsDelete, cancellationToken);
            }

            // Jalankan transaksi aktivasi satu pintu
            await using var tx = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                // 1. Aktivasi kontrak
                await _contractWriteService.ActivateContractAsync(
                    contract.Id,
                    request.SignedDate,
                    fileSaveResult.StoragePath!,
                    fileSaveResult.FileName!,
                    fileSaveResult.ContentType!,
                    userId,
                    cancellationToken);

                // 2. Buat penempatan dan gaji awal
                if (onboardingCase != null && offer != null)
                {
                    await _newHireAssignmentService.AssignNewHirePlacementAndSalaryAsync(
                        contract.WorkforceProfileId,
                        onboardingCase.HospitalSiteId,
                        onboardingCase.OrganizationUnitId,
                        onboardingCase.PositionId,
                        offer.SalaryGradeId,
                        offer.OfferedBasicSalary,
                        contract.StartDate,
                        userId,
                        cancellationToken);
                }

                // 3. Buat masa percobaan bila PKWTT
                if (onboardingCase != null)
                {
                    await _probationService.CreateForContractAsync(
                        contract,
                        onboardingCase.HospitalSiteId,
                        userId,
                        cancellationToken);
                }

                // 4. Centang butir sistem CONTRACT_SIGNED pada checklist onboarding bila ada
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
    }
}
