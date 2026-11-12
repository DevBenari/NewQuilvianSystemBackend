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
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services
{
    public class HrdInitialContractBatchService : IHrdInitialContractBatchService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IHrdNumberSeriesService _numberSeriesService;
        private readonly IHrdEmploymentLifecycleWorkflowIntegrationService _workflowIntegrationService;

        public HrdInitialContractBatchService(
            ApplicationDbContext dbContext,
            IHrdNumberSeriesService numberSeriesService,
            IHrdEmploymentLifecycleWorkflowIntegrationService workflowIntegrationService)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _numberSeriesService = numberSeriesService ?? throw new ArgumentNullException(nameof(numberSeriesService));
            _workflowIntegrationService = workflowIntegrationService ?? throw new ArgumentNullException(nameof(workflowIntegrationService));
        }

        private static Guid GetUserId(ClaimsPrincipal user)
        {
            var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? user.FindFirst("sub")?.Value;
            return Guid.TryParse(sub, out var guid) ? guid : Guid.Empty;
        }

        public async Task<RecruitmentResult<PagedResult<InitialContractBatchListItemResponse>>> GetPagedAsync(
            InitialContractBatchQuery query,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            query ??= new InitialContractBatchQuery();
            var pageNumber = query.PageNumber <= 0 ? 1 : query.PageNumber;
            var pageSize = query.PageSize <= 0 ? 10 : (query.PageSize > 100 ? 100 : query.PageSize);

            var dbQuery = _dbContext.HrdInitialContractBatches
                .Include(b => b.HospitalSite)
                .Include(b => b.SubmittedByUser)
                .AsNoTracking()
                .Where(b => !b.IsDelete)
                .AsQueryable();

            if (query.HospitalSiteId.HasValue)
            {
                dbQuery = dbQuery.Where(b => b.HospitalSiteId == query.HospitalSiteId.Value);
            }

            if (query.BatchStatus.HasValue)
            {
                dbQuery = dbQuery.Where(b => b.BatchStatus == query.BatchStatus.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var s = query.Search.Trim().ToLower();
                dbQuery = dbQuery.Where(b => b.BatchNumber.ToLower().Contains(s));
            }

            var totalCount = await dbQuery.CountAsync(cancellationToken);

            var items = await dbQuery
                .OrderByDescending(b => b.CreateDateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new InitialContractBatchListItemResponse
                {
                    Id = b.Id,
                    BatchNumber = b.BatchNumber,
                    HospitalSiteId = b.HospitalSiteId,
                    HospitalSiteName = b.HospitalSite != null ? b.HospitalSite.SiteName : string.Empty,
                    BatchStatus = b.BatchStatus,
                    TotalRows = b.TotalRows,
                    ValidRows = b.ValidRows,
                    InvalidRows = b.InvalidRows,
                    SubmittedByUserId = b.SubmittedByUserId,
                    SubmittedByUserName = b.SubmittedByUser != null ? b.SubmittedByUser.DisplayName ?? b.SubmittedByUser.UserName : null,
                    SubmittedAt = b.SubmittedAt,
                    AppliedAt = b.AppliedAt
                })
                .ToListAsync(cancellationToken);

            var pagedResult = new PagedResult<InitialContractBatchListItemResponse>
            {
                Items = items,
                TotalData = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPage = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            return RecruitmentResult<PagedResult<InitialContractBatchListItemResponse>>.Ok(pagedResult);
        }

        public async Task<RecruitmentResult<InitialContractBatchDetailResponse>> GetByIdAsync(
            Guid id,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var batch = await _dbContext.HrdInitialContractBatches
                .Include(b => b.HospitalSite)
                .Include(b => b.SubmittedByUser)
                .Include(b => b.DecidedByUser)
                .Include(b => b.Rows)
                    .ThenInclude(r => r.WorkforceProfile)
                .Include(b => b.Rows)
                    .ThenInclude(r => r.ContractType)
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDelete, cancellationToken);

            if (batch == null)
            {
                return RecruitmentResult<InitialContractBatchDetailResponse>.NotFound("Batch data awal kontrak tidak ditemukan.");
            }

            return RecruitmentResult<InitialContractBatchDetailResponse>.Ok(MapToDetailResponse(batch));
        }

        public async Task<RecruitmentResult<InitialContractBatchDetailResponse>> CreateDraftBatchAsync(
            CreateInitialContractBatchRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var actorId = GetUserId(user);

            var site = await _dbContext.MstHospitalSites
                .FirstOrDefaultAsync(s => s.Id == request.HospitalSiteId && !s.IsDelete, cancellationToken);

            if (site == null)
            {
                return RecruitmentResult<InitialContractBatchDetailResponse>.BadRequest("Rumah sakit / unit site tidak ditemukan.");
            }

            var batchNumber = await _numberSeriesService.GenerateBatchNumberAsync(cancellationToken);

            var batch = new HrdInitialContractBatch
            {
                Id = Guid.NewGuid(),
                BatchNumber = batchNumber,
                HospitalSiteId = request.HospitalSiteId,
                BatchStatus = InitialContractBatchStatus.Draft,
                TotalRows = 0,
                ValidRows = 0,
                InvalidRows = 0,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorId
            };

            _dbContext.HrdInitialContractBatches.Add(batch);
            await _dbContext.SaveChangesAsync(cancellationToken);

            batch.HospitalSite = site;
            return RecruitmentResult<InitialContractBatchDetailResponse>.Created(MapToDetailResponse(batch));
        }

        public async Task<RecruitmentResult<InitialContractBatchRowResponse>> AddOrUpdateRowAsync(
            Guid batchId,
            AddInitialContractBatchRowRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var actorId = GetUserId(user);

            var batch = await _dbContext.HrdInitialContractBatches
                .Include(b => b.Rows)
                .FirstOrDefaultAsync(b => b.Id == batchId && !b.IsDelete, cancellationToken);

            if (batch == null)
            {
                return RecruitmentResult<InitialContractBatchRowResponse>.NotFound("Batch data awal kontrak tidak ditemukan.");
            }

            if (batch.BatchStatus != InitialContractBatchStatus.Draft &&
                batch.BatchStatus != InitialContractBatchStatus.Returned)
            {
                return RecruitmentResult<InitialContractBatchRowResponse>.BadRequest(
                    $"Baris batch tidak dapat diubah karena batch berstatus {batch.BatchStatus}.");
            }

            var profile = await _dbContext.MstWorkforceProfiles
                .FirstOrDefaultAsync(p => p.Id == request.WorkforceProfileId && !p.IsDelete, cancellationToken);

            var contractType = await _dbContext.MstContractTypes
                .FirstOrDefaultAsync(ct => ct.Id == request.ContractTypeId && !ct.IsDelete, cancellationToken);

            // Validasi baris
            bool isValid = true;
            string? validationMsg = null;

            if (profile == null)
            {
                isValid = false;
                validationMsg = "Pegawai tidak ditemukan dalam master data.";
            }
            else if (contractType == null)
            {
                isValid = false;
                validationMsg = "Jenis kontrak tidak ditemukan.";
            }
            else
            {
                // VR-ONB-65: Pegawai hanya sekali masuk batch data awal
                var duplicateRow = batch.Rows.FirstOrDefault(r => r.WorkforceProfileId == request.WorkforceProfileId && !r.IsDelete);
                if (duplicateRow != null)
                {
                    isValid = false;
                    validationMsg = "VR-ONB-65: Pegawai ini sudah terdaftar dalam batch ini.";
                }

                // Cek apakah jenis kontrak PKWT dan batas 5 tahun
                if (contractType.RequiresEndDate)
                {
                    if (!request.EndDate.HasValue)
                    {
                        isValid = false;
                        validationMsg = "Kontrak PKWT wajib memiliki tanggal berakhir (EndDate).";
                    }
                    else
                    {
                        var seriesStart = request.FixedTermSeriesStartDate ?? request.StartDate;
                        var end = request.EndDate.Value;
                        var totalMonths = ((end.Year - seriesStart.Year) * 12) + end.Month - seriesStart.Month;

                        if (totalMonths > 60)
                        {
                            isValid = false;
                            validationMsg = $"Total durasi PKWT kumulatif ({totalMonths} bulan) melebihi batas maksimal 5 tahun.";
                        }
                    }
                }
            }

            var row = new HrdInitialContractBatchRow
            {
                Id = Guid.NewGuid(),
                BatchId = batchId,
                WorkforceProfileId = request.WorkforceProfileId,
                ContractTypeId = request.ContractTypeId,
                ContractNumber = request.ContractNumber.Trim(),
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                FixedTermSeriesStartDate = request.FixedTermSeriesStartDate,
                IsValid = isValid,
                ValidationMessage = validationMsg,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorId
            };

            _dbContext.HrdInitialContractBatchRows.Add(row);

            // Perbarui hitungan total baris
            batch.Rows.Add(row);
            RecalculateBatchCounts(batch);
            batch.UpdateDateTime = DateTime.UtcNow;
            batch.UpdateBy = actorId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            row.WorkforceProfile = profile;
            row.ContractType = contractType;

            return RecruitmentResult<InitialContractBatchRowResponse>.Created(MapToRowResponse(row));
        }

        public async Task<RecruitmentResult<bool>> DeleteRowAsync(
            Guid batchId,
            Guid rowId,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var actorId = GetUserId(user);

            var batch = await _dbContext.HrdInitialContractBatches
                .Include(b => b.Rows)
                .FirstOrDefaultAsync(b => b.Id == batchId && !b.IsDelete, cancellationToken);

            if (batch == null)
            {
                return RecruitmentResult<bool>.NotFound("Batch data awal kontrak tidak ditemukan.");
            }

            if (batch.BatchStatus != InitialContractBatchStatus.Draft &&
                batch.BatchStatus != InitialContractBatchStatus.Returned)
            {
                return RecruitmentResult<bool>.BadRequest(
                    $"Baris batch tidak dapat dihapus karena batch berstatus {batch.BatchStatus}.");
            }

            var row = batch.Rows.FirstOrDefault(r => r.Id == rowId && !r.IsDelete);
            if (row == null)
            {
                return RecruitmentResult<bool>.NotFound("Baris batch tidak ditemukan.");
            }

            row.IsDelete = true;
            row.UpdateDateTime = DateTime.UtcNow;
            row.UpdateBy = actorId;

            RecalculateBatchCounts(batch);
            batch.UpdateDateTime = DateTime.UtcNow;
            batch.UpdateBy = actorId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return RecruitmentResult<bool>.Ok(true, "Baris batch data awal berhasil dihapus.");
        }

        public async Task<RecruitmentResult<InitialContractBatchDetailResponse>> SubmitBatchAsync(
            Guid batchId,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var actorId = GetUserId(user);

            var batch = await _dbContext.HrdInitialContractBatches
                .Include(b => b.HospitalSite)
                .Include(b => b.Rows)
                    .ThenInclude(r => r.WorkforceProfile)
                .Include(b => b.Rows)
                    .ThenInclude(r => r.ContractType)
                .FirstOrDefaultAsync(b => b.Id == batchId && !b.IsDelete, cancellationToken);

            if (batch == null)
            {
                return RecruitmentResult<InitialContractBatchDetailResponse>.NotFound("Batch data awal kontrak tidak ditemukan.");
            }

            if (batch.BatchStatus != InitialContractBatchStatus.Draft &&
                batch.BatchStatus != InitialContractBatchStatus.Returned)
            {
                return RecruitmentResult<InitialContractBatchDetailResponse>.BadRequest(
                    $"Batch tidak dapat diajukan karena berstatus {batch.BatchStatus}.");
            }

            RecalculateBatchCounts(batch);

            if (batch.TotalRows == 0)
            {
                return RecruitmentResult<InitialContractBatchDetailResponse>.BadRequest(
                    "Batch tidak dapat diajukan karena belum memiliki baris data kontrak.");
            }

            if (batch.InvalidRows > 0)
            {
                return RecruitmentResult<InitialContractBatchDetailResponse>.BadRequest(
                    $"VR-ONB-66: Batch memiliki {batch.InvalidRows} baris yang tidak valid. Perbaiki atau hapus baris yang bermasalah terlebih dahulu.");
            }

            batch.BatchStatus = InitialContractBatchStatus.Submitted;
            batch.SubmittedByUserId = actorId;
            batch.SubmittedAt = DateTime.UtcNow;
            batch.UpdateDateTime = DateTime.UtcNow;
            batch.UpdateBy = actorId;

            var wfId = await _workflowIntegrationService.StartInitialContractBatchWorkflowAsync(
                batch.Id,
                actorId,
                cancellationToken);

            batch.WorkflowInstanceId = wfId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return RecruitmentResult<InitialContractBatchDetailResponse>.Ok(MapToDetailResponse(batch));
        }

        public async Task<RecruitmentResult<InitialContractBatchDetailResponse>> CancelBatchAsync(
            Guid batchId,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var actorId = GetUserId(user);

            var batch = await _dbContext.HrdInitialContractBatches
                .Include(b => b.HospitalSite)
                .Include(b => b.Rows)
                .FirstOrDefaultAsync(b => b.Id == batchId && !b.IsDelete, cancellationToken);

            if (batch == null)
            {
                return RecruitmentResult<InitialContractBatchDetailResponse>.NotFound("Batch data awal kontrak tidak ditemukan.");
            }

            if (batch.BatchStatus != InitialContractBatchStatus.Draft &&
                batch.BatchStatus != InitialContractBatchStatus.Returned)
            {
                return RecruitmentResult<InitialContractBatchDetailResponse>.BadRequest(
                    $"Batch tidak dapat dibatalkan karena berstatus {batch.BatchStatus}.");
            }

            batch.BatchStatus = InitialContractBatchStatus.Cancelled;
            batch.UpdateDateTime = DateTime.UtcNow;
            batch.UpdateBy = actorId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return RecruitmentResult<InitialContractBatchDetailResponse>.Ok(MapToDetailResponse(batch));
        }

        public async Task<RecruitmentResult<bool>> ApplyInitialBatchAsync(
            Guid batchId,
            Guid deciderUserId,
            bool isApproved,
            string? decisionNote,
            CancellationToken cancellationToken = default)
        {
            var batch = await _dbContext.HrdInitialContractBatches
                .Include(b => b.Rows)
                .FirstOrDefaultAsync(b => b.Id == batchId && !b.IsDelete, cancellationToken);

            if (batch == null)
            {
                return RecruitmentResult<bool>.NotFound("Batch data awal kontrak tidak ditemukan.");
            }

            batch.BatchStatus = isApproved ? InitialContractBatchStatus.Applied : InitialContractBatchStatus.Rejected;
            batch.DecidedByUserId = deciderUserId;
            batch.DecidedAt = DateTime.UtcNow;
            batch.UpdateDateTime = DateTime.UtcNow;
            batch.UpdateBy = deciderUserId;

            if (isApproved)
            {
                batch.AppliedAt = DateTime.UtcNow;

                var activeRows = batch.Rows.Where(r => r.IsValid && !r.IsDelete).ToList();
                foreach (var row in activeRows)
                {
                    var newContract = new WfpContractHistory
                    {
                        Id = Guid.NewGuid(),
                        WorkforceProfileId = row.WorkforceProfileId,
                        ContractTypeId = row.ContractTypeId,
                        ContractNumber = row.ContractNumber,
                        HistoryType = "InitialData",
                        ContractStatus = "Signed",
                        StartDate = row.StartDate.ToDateTime(TimeOnly.MinValue),
                        EndDate = row.EndDate?.ToDateTime(TimeOnly.MinValue),
                        FixedTermSeriesStartDate = row.FixedTermSeriesStartDate?.ToDateTime(TimeOnly.MinValue),
                        SignedDate = row.StartDate.ToDateTime(TimeOnly.MinValue),
                        IsCurrent = true,
                        ProbationEndDate = null,
                        SourceDocumentType = "InitialData",
                        SourceDocumentId = batch.Id,
                        Description = $"Kontrak migrasi data awal batch {batch.BatchNumber}",
                        RowVersion = Guid.NewGuid(),
                        CreateDateTime = DateTime.UtcNow,
                        CreateBy = deciderUserId,
                        IsActive = true
                    };

                    _dbContext.WfpContractHistories.Add(newContract);
                    row.CreatedContractHistoryId = newContract.Id;
                    row.UpdateDateTime = DateTime.UtcNow;
                    row.UpdateBy = deciderUserId;
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return RecruitmentResult<bool>.Ok(true, "Penerapan batch data awal berhasil dieksekusi.");
        }

        private static void RecalculateBatchCounts(HrdInitialContractBatch batch)
        {
            var activeRows = batch.Rows.Where(r => !r.IsDelete).ToList();
            batch.TotalRows = activeRows.Count;
            batch.ValidRows = activeRows.Count(r => r.IsValid);
            batch.InvalidRows = activeRows.Count(r => !r.IsValid);
        }

        private static InitialContractBatchDetailResponse MapToDetailResponse(HrdInitialContractBatch b)
        {
            return new InitialContractBatchDetailResponse
            {
                Id = b.Id,
                BatchNumber = b.BatchNumber,
                HospitalSiteId = b.HospitalSiteId,
                HospitalSiteName = b.HospitalSite?.SiteName ?? string.Empty,
                BatchStatus = b.BatchStatus,
                TotalRows = b.TotalRows,
                ValidRows = b.ValidRows,
                InvalidRows = b.InvalidRows,
                SubmittedByUserId = b.SubmittedByUserId,
                SubmittedByUserName = b.SubmittedByUser?.DisplayName ?? b.SubmittedByUser?.UserName,
                SubmittedAt = b.SubmittedAt,
                DecidedByUserId = b.DecidedByUserId,
                DecidedByUserName = b.DecidedByUser?.DisplayName ?? b.DecidedByUser?.UserName,
                DecidedAt = b.DecidedAt,
                AppliedAt = b.AppliedAt,
                Rows = b.Rows.Where(r => !r.IsDelete).Select(MapToRowResponse).ToList(),
                RowVersion = b.RowVersion
            };
        }

        private static InitialContractBatchRowResponse MapToRowResponse(HrdInitialContractBatchRow r)
        {
            return new InitialContractBatchRowResponse
            {
                Id = r.Id,
                BatchId = r.BatchId,
                WorkforceProfileId = r.WorkforceProfileId,
                WorkforceCode = r.WorkforceProfile?.ProfileCode ?? string.Empty,
                FullName = r.WorkforceProfile?.DisplayName ?? string.Empty,
                ContractTypeId = r.ContractTypeId,
                ContractTypeName = r.ContractType?.ContractTypeName ?? string.Empty,
                ContractNumber = r.ContractNumber,
                StartDate = r.StartDate,
                EndDate = r.EndDate,
                FixedTermSeriesStartDate = r.FixedTermSeriesStartDate,
                IsValid = r.IsValid,
                ValidationMessage = r.ValidationMessage,
                CreatedContractHistoryId = r.CreatedContractHistoryId
            };
        }
    }
}
