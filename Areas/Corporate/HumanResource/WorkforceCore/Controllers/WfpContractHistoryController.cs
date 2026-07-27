using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/contract-histories")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFORCE_CORE",
        moduleName: "Human Resource Workforce Core",
        displayName: "Workforce Contract History",
        AreaName = "Corporate",
        ControllerName = "WorkforceContractHistory",
        Description = "Corporate human resource workforce contract history",
        SortOrder = 3
    )]
    [Tags("Corporate / Human Resource / Workforce Core / Contract History")]
    public class WfpContractHistoryController : ControllerBase
    {
        private static readonly HashSet<string> AllowedHistoryTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "Initial", "Renewal", "Amendment", "Extension", "Suspension", "Termination", "Expiry"
        };

        private static readonly HashSet<string> AllowedContractStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "Draft", "Active", "Suspended", "Ended", "Expired", "Terminated", "Cancelled"
        };

        private const string LogCategory = "Corporate.HumanResource.WorkforceCore";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public WfpContractHistoryController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<WfpContractHistoryFilterMetadataResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Contract History", Description = "Melihat metadata filter riwayat kontrak workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceContractHistory", "Read")]
        public async Task<IActionResult> GetFilterMetadata(
            Guid workforceProfileId,
            CancellationToken cancellationToken)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Profil tenaga kerja tidak ditemukan."));
            }

            var result = new WfpContractHistoryFilterMetadataResponse
            {
                DefaultFilter = new WfpContractHistoryDefaultFilterResponse(),
                HistoryTypeOptions = AllowedHistoryTypes
                    .OrderBy(x => x)
                    .Select(x => new WfpContractHistoryStringOptionResponse
                    {
                        Value = x,
                        Label = BuildHistoryTypeLabel(x)
                    })
                    .ToList(),
                ContractStatusOptions = AllowedContractStatuses
                    .OrderBy(x => x)
                    .Select(x => new WfpContractHistoryStringOptionResponse
                    {
                        Value = x,
                        Label = BuildContractStatusLabel(x)
                    })
                    .ToList(),
                ContractTypeOptions = await _dbContext.MstContractTypes
                    .AsNoTracking()
                    .Where(x => x.IsActive && !x.IsDelete)
                    .OrderBy(x => x.SortOrder)
                    .ThenBy(x => x.ContractTypeName)
                    .Select(x => new WfpMasterOptionResponse
                    {
                        Id = x.Id,
                        Code = x.ContractTypeCode,
                        Name = x.ContractTypeName,
                        Label = x.ContractTypeName
                    })
                    .ToListAsync(cancellationToken),
                EmploymentTypeOptions = await _dbContext.MstEmploymentTypes
                    .AsNoTracking()
                    .Where(x => x.IsActive && !x.IsDelete)
                    .OrderBy(x => x.SortOrder)
                    .ThenBy(x => x.EmploymentTypeName)
                    .Select(x => new WfpMasterOptionResponse
                    {
                        Id = x.Id,
                        Code = x.EmploymentTypeCode,
                        Name = x.EmploymentTypeName,
                        Label = x.EmploymentTypeName
                    })
                    .ToListAsync(cancellationToken),
                WorkerSourceOptions = await _dbContext.MstWorkerSources
                    .AsNoTracking()
                    .Where(x => x.IsActive && !x.IsDelete)
                    .OrderBy(x => x.SortOrder)
                    .ThenBy(x => x.WorkerSourceName)
                    .Select(x => new WfpMasterOptionResponse
                    {
                        Id = x.Id,
                        Code = x.WorkerSourceCode,
                        Name = x.WorkerSourceName,
                        Label = x.WorkerSourceName
                    })
                    .ToListAsync(cancellationToken),
                TerminationReasonOptions = await _dbContext.MstTerminationReasons
                    .AsNoTracking()
                    .Where(x => x.IsActive && !x.IsDelete)
                    .OrderBy(x => x.SortOrder)
                    .ThenBy(x => x.TerminationReasonName)
                    .Select(x => new WfpMasterOptionResponse
                    {
                        Id = x.Id,
                        Code = x.TerminationReasonCode,
                        Name = x.TerminationReasonName,
                        Label = x.TerminationReasonName
                    })
                    .ToListAsync(cancellationToken),
                PreviousContractOptions = await _dbContext.Set<WfpContractHistory>()
                    .AsNoTracking()
                    .Where(x =>
                        x.WorkforceProfileId == workforceProfileId &&
                        !x.IsDelete)
                    .OrderByDescending(x => x.StartDate)
                    .ThenByDescending(x => x.CreateDateTime)
                    .Select(x => new WfpPreviousContractOptionResponse
                    {
                        Id = x.Id,
                        ContractNumber = x.ContractNumber,
                        ContractStatus = x.ContractStatus,
                        StartDate = x.StartDate,
                        EndDate = x.EndDate,
                        Label = x.ContractNumber + " - " + x.ContractStatus
                    })
                    .ToListAsync(cancellationToken),
                SortOptions = new List<WfpContractHistorySortOptionResponse>
                {
                    new() { Value = "startDate", Label = "Tanggal mulai" },
                    new() { Value = "contractNumber", Label = "Nomor kontrak" },
                    new() { Value = "historyType", Label = "Jenis riwayat" },
                    new() { Value = "contractStatus", Label = "Status kontrak" },
                    new() { Value = "isCurrent", Label = "Kontrak berjalan" },
                    new() { Value = "isActive", Label = "Status aktif" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            return Ok(ApiResponse<WfpContractHistoryFilterMetadataResponse>.Ok(
                result,
                "Metadata filter riwayat kontrak workforce berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<WfpContractHistorySummaryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Contract History", Description = "Melihat ringkasan riwayat kontrak workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceContractHistory", "Read")]
        public async Task<IActionResult> GetSummary(
            Guid workforceProfileId,
            CancellationToken cancellationToken)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Profil tenaga kerja tidak ditemukan."));
            }

            var query = _dbContext.Set<WfpContractHistory>()
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            var result = new WfpContractHistorySummaryResponse
            {
                TotalContractHistory = await query.CountAsync(cancellationToken),
                ActiveContractHistory = await query.CountAsync(x => x.IsActive, cancellationToken),
                InactiveContractHistory = await query.CountAsync(x => !x.IsActive, cancellationToken),
                CurrentContract = await query.CountAsync(x => x.IsCurrent, cancellationToken),
                DraftContract = await query.CountAsync(x => x.ContractStatus == "Draft", cancellationToken),
                ActiveContract = await query.CountAsync(x => x.ContractStatus == "Active", cancellationToken),
                EndedContract = await query.CountAsync(x => x.ContractStatus == "Ended", cancellationToken),
                ExpiredContract = await query.CountAsync(x => x.ContractStatus == "Expired", cancellationToken),
                TerminatedContract = await query.CountAsync(x => x.ContractStatus == "Terminated", cancellationToken)
            };

            return Ok(ApiResponse<WfpContractHistorySummaryResponse>.Ok(
                result,
                "Ringkasan riwayat kontrak workforce berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<WfpContractHistoryResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Contract History", Description = "Melihat data riwayat kontrak workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceContractHistory", "Read")]
        public async Task<IActionResult> GetContractHistories(
            Guid workforceProfileId,
            [FromQuery] string? historyType,
            [FromQuery] string? contractStatus,
            [FromQuery] Guid? contractTypeId,
            [FromQuery] bool? isCurrent,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "startDate",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Profil tenaga kerja tidak ditemukan."));
            }

            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery(workforceProfileId);
            query = ApplyFilter(
                query,
                historyType,
                contractStatus,
                contractTypeId,
                isCurrent,
                isActive,
                search);

            var totalData = await query.CountAsync(cancellationToken);

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new WfpContractHistoryResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    WorkforceProfileCode = x.WorkforceProfile != null ? x.WorkforceProfile.ProfileCode : string.Empty,
                    WorkforceDisplayName = x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : string.Empty,
                    PreviousContractHistoryId = x.PreviousContractHistoryId,
                    PreviousContractNumber = x.PreviousContractHistory != null ? x.PreviousContractHistory.ContractNumber : null,
                    ContractTypeId = x.ContractTypeId,
                    ContractTypeCode = x.ContractType != null ? x.ContractType.ContractTypeCode : null,
                    ContractTypeName = x.ContractType != null ? x.ContractType.ContractTypeName : null,
                    EmploymentTypeId = x.EmploymentTypeId,
                    EmploymentTypeCode = x.EmploymentType != null ? x.EmploymentType.EmploymentTypeCode : null,
                    EmploymentTypeName = x.EmploymentType != null ? x.EmploymentType.EmploymentTypeName : null,
                    WorkerSourceId = x.WorkerSourceId,
                    WorkerSourceCode = x.WorkerSource != null ? x.WorkerSource.WorkerSourceCode : null,
                    WorkerSourceName = x.WorkerSource != null ? x.WorkerSource.WorkerSourceName : null,
                    TerminationReasonId = x.TerminationReasonId,
                    TerminationReasonCode = x.TerminationReason != null ? x.TerminationReason.TerminationReasonCode : null,
                    TerminationReasonName = x.TerminationReason != null ? x.TerminationReason.TerminationReasonName : null,
                    ContractNumber = x.ContractNumber,
                    HistoryType = x.HistoryType,
                    ContractStatus = x.ContractStatus,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    SignedDate = x.SignedDate,
                    ProbationEndDate = x.ProbationEndDate,
                    TerminatedAt = x.TerminatedAt,
                    RenewalSequence = x.RenewalSequence,
                    IsCurrent = x.IsCurrent,
                    DocumentPath = x.DocumentPath,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                    CreateByName = x.CreateBy == Guid.Empty
                        ? null
                        : _dbContext.Users
                            .Where(u => u.Id == x.CreateBy)
                            .Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode)
                            .FirstOrDefault()
                })
                .ToListAsync(cancellationToken);

            var result = new PagedResult<WfpContractHistoryResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<PagedResult<WfpContractHistoryResponse>>.Ok(
                result,
                "Data riwayat kontrak workforce berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WfpContractHistoryDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Contract History", Description = "Melihat detail riwayat kontrak workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceContractHistory", "Read")]
        public async Task<IActionResult> GetContractHistoryById(
            Guid workforceProfileId,
            Guid id,
            CancellationToken cancellationToken)
        {
            var data = await BuildBaseQuery(workforceProfileId)
                .Where(x => x.Id == id)
                .Select(x => new WfpContractHistoryDetailResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    WorkforceProfileCode = x.WorkforceProfile != null ? x.WorkforceProfile.ProfileCode : string.Empty,
                    WorkforceDisplayName = x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : string.Empty,
                    PreviousContractHistoryId = x.PreviousContractHistoryId,
                    PreviousContractNumber = x.PreviousContractHistory != null ? x.PreviousContractHistory.ContractNumber : null,
                    ContractTypeId = x.ContractTypeId,
                    ContractTypeCode = x.ContractType != null ? x.ContractType.ContractTypeCode : null,
                    ContractTypeName = x.ContractType != null ? x.ContractType.ContractTypeName : null,
                    EmploymentTypeId = x.EmploymentTypeId,
                    EmploymentTypeCode = x.EmploymentType != null ? x.EmploymentType.EmploymentTypeCode : null,
                    EmploymentTypeName = x.EmploymentType != null ? x.EmploymentType.EmploymentTypeName : null,
                    WorkerSourceId = x.WorkerSourceId,
                    WorkerSourceCode = x.WorkerSource != null ? x.WorkerSource.WorkerSourceCode : null,
                    WorkerSourceName = x.WorkerSource != null ? x.WorkerSource.WorkerSourceName : null,
                    TerminationReasonId = x.TerminationReasonId,
                    TerminationReasonCode = x.TerminationReason != null ? x.TerminationReason.TerminationReasonCode : null,
                    TerminationReasonName = x.TerminationReason != null ? x.TerminationReason.TerminationReasonName : null,
                    ContractNumber = x.ContractNumber,
                    HistoryType = x.HistoryType,
                    ContractStatus = x.ContractStatus,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    SignedDate = x.SignedDate,
                    ProbationEndDate = x.ProbationEndDate,
                    TerminatedAt = x.TerminatedAt,
                    RenewalSequence = x.RenewalSequence,
                    IsCurrent = x.IsCurrent,
                    DocumentPath = x.DocumentPath,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    RenewalCount = x.Renewals.Count(r => !r.IsDelete),
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                    CreateByName = x.CreateBy == Guid.Empty
                        ? null
                        : _dbContext.Users
                            .Where(u => u.Id == x.CreateBy)
                            .Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode)
                            .FirstOrDefault(),
                    UpdateDateTime = x.UpdateDateTime,
                    UpdateBy = x.UpdateBy == Guid.Empty ? null : x.UpdateBy,
                    UpdateByName = x.UpdateBy == Guid.Empty
                        ? null
                        : _dbContext.Users
                            .Where(u => u.Id == x.UpdateBy)
                            .Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode)
                            .FirstOrDefault()
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (data == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Riwayat kontrak workforce tidak ditemukan."));
            }

            return Ok(ApiResponse<WfpContractHistoryDetailResponse>.Ok(
                data,
                "Detail riwayat kontrak workforce berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<WfpContractHistoryDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Create", "Create Workforce Contract History", Description = "Membuat riwayat kontrak workforce", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("WorkforceContractHistory", "Create")]
        public async Task<IActionResult> CreateContractHistory(
            Guid workforceProfileId,
            [FromBody] CreateWfpContractHistoryRequest request,
            CancellationToken cancellationToken)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Profil tenaga kerja tidak ditemukan."));
            }

            var validation = await ValidateRequestAsync(
                workforceProfileId,
                null,
                request,
                cancellationToken);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data riwayat kontrak workforce tidak valid."));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var normalizedStatus = NormalizeContractStatus(request.ContractStatus);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                if (request.IsCurrent)
                {
                    await UnsetOtherCurrentAsync(workforceProfileId, null, now, actorUserId, cancellationToken);
                }

                var entity = new WfpContractHistory
                {
                    Id = Guid.NewGuid(),
                    WorkforceProfileId = workforceProfileId,
                    PreviousContractHistoryId = NormalizeNullableGuid(request.PreviousContractHistoryId),
                    ContractTypeId = NormalizeNullableGuid(request.ContractTypeId),
                    EmploymentTypeId = NormalizeNullableGuid(request.EmploymentTypeId),
                    WorkerSourceId = NormalizeNullableGuid(request.WorkerSourceId),
                    TerminationReasonId = NormalizeNullableGuid(request.TerminationReasonId),
                    ContractNumber = NormalizeContractNumber(request.ContractNumber),
                    HistoryType = NormalizeHistoryType(request.HistoryType),
                    ContractStatus = normalizedStatus,
                    StartDate = request.StartDate.Date,
                    EndDate = request.EndDate?.Date,
                    SignedDate = request.SignedDate?.Date,
                    ProbationEndDate = request.ProbationEndDate?.Date,
                    TerminatedAt = request.TerminatedAt,
                    RenewalSequence = request.RenewalSequence,
                    IsCurrent = request.IsCurrent,
                    DocumentPath = NormalizeNullableText(request.DocumentPath),
                    Description = NormalizeNullableText(request.Description),
                    IsActive = request.IsActive,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                _dbContext.Set<WfpContractHistory>().Add(entity);
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                await _loggerService.InfoAsync(
                    LogCategory,
                    "WorkforceContractHistory.CreateContractHistory",
                    "Membuat riwayat kontrak workforce.",
                    new { entity.Id, entity.WorkforceProfileId, entity.ContractNumber, entity.ContractStatus, entity.IsCurrent });

                return await GetContractHistoryById(workforceProfileId, entity.Id, cancellationToken);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "WorkforceContractHistory.CreateContractHistory",
                    "Gagal membuat riwayat kontrak workforce.",
                    ex);

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat membuat riwayat kontrak workforce."));
            }
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WfpContractHistoryDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Workforce Contract History", Description = "Mengubah riwayat kontrak workforce", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceContractHistory", "Update")]
        public async Task<IActionResult> UpdateContractHistory(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWfpContractHistoryRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<WfpContractHistory>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete,
                    cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Riwayat kontrak workforce tidak ditemukan."));
            }

            var validation = await ValidateRequestAsync(
                workforceProfileId,
                id,
                request,
                cancellationToken);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data riwayat kontrak workforce tidak valid."));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var normalizedStatus = NormalizeContractStatus(request.ContractStatus);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                if (request.IsCurrent)
                {
                    await UnsetOtherCurrentAsync(workforceProfileId, id, now, actorUserId, cancellationToken);
                }

                entity.PreviousContractHistoryId = NormalizeNullableGuid(request.PreviousContractHistoryId);
                entity.ContractTypeId = NormalizeNullableGuid(request.ContractTypeId);
                entity.EmploymentTypeId = NormalizeNullableGuid(request.EmploymentTypeId);
                entity.WorkerSourceId = NormalizeNullableGuid(request.WorkerSourceId);
                entity.TerminationReasonId = NormalizeNullableGuid(request.TerminationReasonId);
                entity.ContractNumber = NormalizeContractNumber(request.ContractNumber);
                entity.HistoryType = NormalizeHistoryType(request.HistoryType);
                entity.ContractStatus = normalizedStatus;
                entity.StartDate = request.StartDate.Date;
                entity.EndDate = request.EndDate?.Date;
                entity.SignedDate = request.SignedDate?.Date;
                entity.ProbationEndDate = request.ProbationEndDate?.Date;
                entity.TerminatedAt = request.TerminatedAt;
                entity.RenewalSequence = request.RenewalSequence;
                entity.IsCurrent = request.IsCurrent && request.IsActive && normalizedStatus == "Active";
                entity.DocumentPath = NormalizeNullableText(request.DocumentPath);
                entity.Description = NormalizeNullableText(request.Description);
                entity.IsActive = request.IsActive;
                entity.UpdateDateTime = now;
                entity.UpdateBy = actorUserId;

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                await _loggerService.InfoAsync(
                    LogCategory,
                    "WorkforceContractHistory.UpdateContractHistory",
                    "Mengubah riwayat kontrak workforce.",
                    new { entity.Id, entity.WorkforceProfileId, entity.ContractNumber, entity.ContractStatus, entity.IsCurrent });

                return await GetContractHistoryById(workforceProfileId, entity.Id, cancellationToken);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "WorkforceContractHistory.UpdateContractHistory",
                    "Gagal mengubah riwayat kontrak workforce.",
                    ex);

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat mengubah riwayat kontrak workforce."));
            }
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Workforce Contract History", Description = "Mengubah status riwayat kontrak workforce", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceContractHistory", "Update")]
        public async Task<IActionResult> UpdateContractStatus(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWfpContractHistoryStatusRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<WfpContractHistory>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete,
                    cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Riwayat kontrak workforce tidak ditemukan."));
            }

            var normalizedStatus = NormalizeContractStatus(request.ContractStatus);

            if (!AllowedContractStatuses.Contains(normalizedStatus))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Status kontrak tidak valid."));
            }

            if (request.EndDate.HasValue && request.EndDate.Value.Date < entity.StartDate.Date)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "EndDate tidak boleh lebih kecil dari StartDate."));
            }

            if (normalizedStatus == "Terminated")
            {
                if (!request.TerminatedAt.HasValue)
                {
                    return BadRequest(ApiResponse<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        "TerminatedAt wajib diisi untuk kontrak berstatus Terminated."));
                }

                var reasonId = NormalizeNullableGuid(request.TerminationReasonId);
                if (!reasonId.HasValue)
                {
                    return BadRequest(ApiResponse<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        "TerminationReasonId wajib diisi untuk kontrak berstatus Terminated."));
                }

                var reasonExists = await _dbContext.MstTerminationReasons
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id == reasonId.Value &&
                        x.IsActive &&
                        !x.IsDelete,
                        cancellationToken);

                if (!reasonExists)
                {
                    return BadRequest(ApiResponse<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        "Alasan terminasi tidak ditemukan atau sudah tidak aktif."));
                }
            }

            entity.ContractStatus = normalizedStatus;
            entity.IsActive = request.IsActive;
            entity.EndDate = request.EndDate?.Date ?? entity.EndDate;
            entity.TerminatedAt = normalizedStatus == "Terminated" ? request.TerminatedAt : entity.TerminatedAt;
            entity.TerminationReasonId = normalizedStatus == "Terminated"
                ? NormalizeNullableGuid(request.TerminationReasonId)
                : entity.TerminationReasonId;
            entity.Description = NormalizeNullableText(request.Description) ?? entity.Description;

            if (!request.IsActive || normalizedStatus != "Active")
                entity.IsCurrent = false;

            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status riwayat kontrak workforce berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/current")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Workforce Contract History", Description = "Menetapkan kontrak berjalan workforce", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceContractHistory", "Update")]
        public async Task<IActionResult> SetCurrentContract(
            Guid workforceProfileId,
            Guid id,
            [FromBody] SetWfpContractHistoryCurrentRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<WfpContractHistory>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete,
                    cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Riwayat kontrak workforce tidak ditemukan."));
            }

            if (request.IsCurrent && (!entity.IsActive || entity.ContractStatus != "Active"))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Hanya kontrak aktif dengan status Active yang dapat dijadikan kontrak berjalan."));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                if (request.IsCurrent)
                {
                    await UnsetOtherCurrentAsync(workforceProfileId, id, now, actorUserId, cancellationToken);
                }

                entity.IsCurrent = request.IsCurrent;
                entity.UpdateDateTime = now;
                entity.UpdateBy = actorUserId;

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return Ok(ApiResponse<object>.Ok(
                    null,
                    request.IsCurrent
                        ? "Kontrak berjalan workforce berhasil ditetapkan."
                        : "Status kontrak berjalan workforce berhasil dilepas."));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "WorkforceContractHistory.SetCurrentContract",
                    "Gagal menetapkan kontrak berjalan workforce.",
                    ex);

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat menetapkan kontrak berjalan workforce."));
            }
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Workforce Contract History", Description = "Menghapus riwayat kontrak workforce", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("WorkforceContractHistory", "Delete")]
        public async Task<IActionResult> DeleteContractHistory(
            Guid workforceProfileId,
            Guid id,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<WfpContractHistory>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete,
                    cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Riwayat kontrak workforce tidak ditemukan."));
            }

            var hasRenewals = await _dbContext.Set<WfpContractHistory>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.PreviousContractHistoryId == id &&
                    !x.IsDelete,
                    cancellationToken);

            if (hasRenewals)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Riwayat kontrak tidak dapat dihapus karena sudah menjadi referensi kontrak lanjutan."));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.IsCurrent = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceContractHistory.DeleteContractHistory",
                "Menghapus riwayat kontrak workforce.",
                new { entity.Id, entity.WorkforceProfileId, entity.ContractNumber });

            return Ok(ApiResponse<object>.Ok(
                null,
                "Riwayat kontrak workforce berhasil dihapus."));
        }

        private IQueryable<WfpContractHistory> BuildBaseQuery(Guid workforceProfileId)
        {
            return _dbContext.Set<WfpContractHistory>()
                .AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.PreviousContractHistory)
                .Include(x => x.ContractType)
                .Include(x => x.EmploymentType)
                .Include(x => x.WorkerSource)
                .Include(x => x.TerminationReason)
                .Include(x => x.Renewals)
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);
        }

        private static IQueryable<WfpContractHistory> ApplyFilter(
            IQueryable<WfpContractHistory> query,
            string? historyType,
            string? contractStatus,
            Guid? contractTypeId,
            bool? isCurrent,
            bool? isActive,
            string? search)
        {
            if (!string.IsNullOrWhiteSpace(historyType))
            {
                var normalizedType = NormalizeHistoryType(historyType);
                query = query.Where(x => x.HistoryType == normalizedType);
            }

            if (!string.IsNullOrWhiteSpace(contractStatus))
            {
                var normalizedStatus = NormalizeContractStatus(contractStatus);
                query = query.Where(x => x.ContractStatus == normalizedStatus);
            }

            contractTypeId = NormalizeNullableGuid(contractTypeId);
            if (contractTypeId.HasValue)
                query = query.Where(x => x.ContractTypeId == contractTypeId.Value);

            if (isCurrent.HasValue)
                query = query.Where(x => x.IsCurrent == isCurrent.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.ContractNumber.ToLower().Contains(keyword) ||
                    x.HistoryType.ToLower().Contains(keyword) ||
                    x.ContractStatus.ToLower().Contains(keyword) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)) ||
                    (x.ContractType != null && x.ContractType.ContractTypeCode.ToLower().Contains(keyword)) ||
                    (x.ContractType != null && x.ContractType.ContractTypeName.ToLower().Contains(keyword)) ||
                    (x.EmploymentType != null && x.EmploymentType.EmploymentTypeName.ToLower().Contains(keyword)) ||
                    (x.WorkerSource != null && x.WorkerSource.WorkerSourceName.ToLower().Contains(keyword)) ||
                    (x.TerminationReason != null && x.TerminationReason.TerminationReasonName.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<WfpContractHistory> ApplySorting(
            IQueryable<WfpContractHistory> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDescending = !string.Equals(
                sortDirection?.Trim(),
                "asc",
                StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "startDate").Trim().ToLowerInvariant() switch
            {
                "contractnumber" => isDescending
                    ? query.OrderByDescending(x => x.ContractNumber)
                    : query.OrderBy(x => x.ContractNumber),

                "historytype" => isDescending
                    ? query.OrderByDescending(x => x.HistoryType).ThenByDescending(x => x.StartDate)
                    : query.OrderBy(x => x.HistoryType).ThenBy(x => x.StartDate),

                "contractstatus" => isDescending
                    ? query.OrderByDescending(x => x.ContractStatus).ThenByDescending(x => x.StartDate)
                    : query.OrderBy(x => x.ContractStatus).ThenBy(x => x.StartDate),

                "iscurrent" => isDescending
                    ? query.OrderByDescending(x => x.IsCurrent).ThenByDescending(x => x.StartDate)
                    : query.OrderBy(x => x.IsCurrent).ThenBy(x => x.StartDate),

                "isactive" => isDescending
                    ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.StartDate)
                    : query.OrderBy(x => x.IsActive).ThenBy(x => x.StartDate),

                "createdatetime" => isDescending
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),

                _ => isDescending
                    ? query.OrderByDescending(x => x.StartDate).ThenByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.StartDate).ThenBy(x => x.CreateDateTime)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid workforceProfileId,
            Guid? excludeId,
            CreateWfpContractHistoryRequest request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.ContractNumber))
                return (false, "Nomor kontrak wajib diisi.");

            if (string.IsNullOrWhiteSpace(request.HistoryType) ||
                !AllowedHistoryTypes.Contains(request.HistoryType.Trim()))
            {
                return (false, "HistoryType tidak valid.");
            }

            if (string.IsNullOrWhiteSpace(request.ContractStatus) ||
                !AllowedContractStatuses.Contains(request.ContractStatus.Trim()))
            {
                return (false, "ContractStatus tidak valid.");
            }

            if (request.StartDate == default)
                return (false, "StartDate wajib diisi.");

            if (request.EndDate.HasValue && request.EndDate.Value.Date < request.StartDate.Date)
                return (false, "EndDate tidak boleh lebih kecil dari StartDate.");

            if (request.SignedDate.HasValue && request.SignedDate.Value.Date > request.StartDate.Date.AddYears(1))
                return (false, "SignedDate tidak valid terhadap StartDate.");

            if (request.ProbationEndDate.HasValue && request.ProbationEndDate.Value.Date < request.StartDate.Date)
                return (false, "ProbationEndDate tidak boleh lebih kecil dari StartDate.");

            var normalizedHistoryType = NormalizeHistoryType(request.HistoryType);
            var normalizedStatus = NormalizeContractStatus(request.ContractStatus);

            if (request.IsCurrent && (!request.IsActive || normalizedStatus != "Active"))
            {
                return (false, "Kontrak berjalan harus aktif dan memiliki status Active.");
            }

            if (normalizedHistoryType == "Termination" || normalizedStatus == "Terminated")
            {
                if (!request.TerminatedAt.HasValue)
                    return (false, "TerminatedAt wajib diisi untuk terminasi kontrak.");

                if (!NormalizeNullableGuid(request.TerminationReasonId).HasValue)
                    return (false, "TerminationReasonId wajib diisi untuk terminasi kontrak.");
            }

            var normalizedContractNumber = NormalizeContractNumber(request.ContractNumber);

            var duplicateQuery = _dbContext.Set<WfpContractHistory>()
                .AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.ContractNumber == normalizedContractNumber &&
                    !x.IsDelete);

            if (excludeId.HasValue)
                duplicateQuery = duplicateQuery.Where(x => x.Id != excludeId.Value);

            if (await duplicateQuery.AnyAsync(cancellationToken))
                return (false, "Nomor kontrak sudah digunakan pada profil tenaga kerja ini.");

            var previousContractId = NormalizeNullableGuid(request.PreviousContractHistoryId);
            if (previousContractId.HasValue)
            {
                if (excludeId.HasValue && previousContractId.Value == excludeId.Value)
                    return (false, "Kontrak tidak dapat menjadi kontrak sebelumnya untuk dirinya sendiri.");

                var previousExists = await _dbContext.Set<WfpContractHistory>()
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id == previousContractId.Value &&
                        x.WorkforceProfileId == workforceProfileId &&
                        !x.IsDelete,
                        cancellationToken);

                if (!previousExists)
                    return (false, "Kontrak sebelumnya tidak ditemukan atau tidak berasal dari profil tenaga kerja yang sama.");
            }
            else if (normalizedHistoryType is "Renewal" or "Extension" or "Amendment")
            {
                return (false, "PreviousContractHistoryId wajib diisi untuk Renewal, Extension, atau Amendment.");
            }

            var contractTypeId = NormalizeNullableGuid(request.ContractTypeId);
            if (contractTypeId.HasValue)
            {
                var contractType = await _dbContext.MstContractTypes
                    .AsNoTracking()
                    .Where(x =>
                        x.Id == contractTypeId.Value &&
                        x.IsActive &&
                        !x.IsDelete)
                    .Select(x => new { x.RequiresEndDate })
                    .FirstOrDefaultAsync(cancellationToken);

                if (contractType == null)
                    return (false, "Jenis kontrak tidak ditemukan atau sudah tidak aktif.");

                if (contractType.RequiresEndDate && !request.EndDate.HasValue)
                    return (false, "EndDate wajib diisi untuk jenis kontrak yang dipilih.");
            }

            var employmentTypeId = NormalizeNullableGuid(request.EmploymentTypeId);
            if (employmentTypeId.HasValue)
            {
                var employmentType = await _dbContext.MstEmploymentTypes
                    .AsNoTracking()
                    .Where(x =>
                        x.Id == employmentTypeId.Value &&
                        x.IsActive &&
                        !x.IsDelete)
                    .Select(x => new { x.RequiresContractEndDate })
                    .FirstOrDefaultAsync(cancellationToken);

                if (employmentType == null)
                    return (false, "Jenis kepegawaian tidak ditemukan atau sudah tidak aktif.");

                if (employmentType.RequiresContractEndDate && !request.EndDate.HasValue)
                    return (false, "EndDate wajib diisi untuk jenis kepegawaian yang dipilih.");
            }

            var workerSourceId = NormalizeNullableGuid(request.WorkerSourceId);
            if (workerSourceId.HasValue &&
                !await _dbContext.MstWorkerSources.AsNoTracking().AnyAsync(x =>
                    x.Id == workerSourceId.Value && x.IsActive && !x.IsDelete,
                    cancellationToken))
            {
                return (false, "Sumber tenaga kerja tidak ditemukan atau sudah tidak aktif.");
            }

            var terminationReasonId = NormalizeNullableGuid(request.TerminationReasonId);
            if (terminationReasonId.HasValue &&
                !await _dbContext.MstTerminationReasons.AsNoTracking().AnyAsync(x =>
                    x.Id == terminationReasonId.Value && x.IsActive && !x.IsDelete,
                    cancellationToken))
            {
                return (false, "Alasan terminasi tidak ditemukan atau sudah tidak aktif.");
            }

            return (true, null);
        }

        private async Task UnsetOtherCurrentAsync(
            Guid workforceProfileId,
            Guid? exceptId,
            DateTime now,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var query = _dbContext.Set<WfpContractHistory>()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.IsCurrent &&
                    x.IsActive &&
                    !x.IsDelete);

            if (exceptId.HasValue)
                query = query.Where(x => x.Id != exceptId.Value);

            var currentContracts = await query.ToListAsync(cancellationToken);

            foreach (var item in currentContracts)
            {
                item.IsCurrent = false;
                item.UpdateDateTime = now;
                item.UpdateBy = actorUserId;
            }
        }

        private async Task<bool> WorkforceProfileExistsAsync(
            Guid workforceProfileId,
            CancellationToken cancellationToken)
        {
            return workforceProfileId != Guid.Empty &&
                   await _dbContext.MstWorkforceProfiles
                       .AsNoTracking()
                       .AnyAsync(x =>
                           x.Id == workforceProfileId &&
                           x.IsActive &&
                           !x.IsDelete,
                           cancellationToken);
        }

        private Guid GetCurrentUserId()
        {
            var userIdText =
                User.FindFirstValue("user_id") ??
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userIdText, out var userId)
                ? userId
                : Guid.Empty;
        }

        private static (int PageNumber, int PageSize) NormalizePaging(
            int pageNumber,
            int pageSize)
        {
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 25 : pageSize;
            pageSize = pageSize > 100 ? 100 : pageSize;
            return (pageNumber, pageSize);
        }

        private static Guid? NormalizeNullableGuid(Guid? value)
        {
            return !value.HasValue || value.Value == Guid.Empty
                ? null
                : value.Value;
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private static string NormalizeContractNumber(string value)
        {
            return value.Trim().ToUpperInvariant();
        }

        private static string NormalizeHistoryType(string value)
        {
            var selected = AllowedHistoryTypes
                .FirstOrDefault(x => x.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));

            return selected ?? value.Trim();
        }

        private static string NormalizeContractStatus(string value)
        {
            var selected = AllowedContractStatuses
                .FirstOrDefault(x => x.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));

            return selected ?? value.Trim();
        }

        private static string BuildHistoryTypeLabel(string value)
        {
            return value switch
            {
                "Initial" => "Kontrak Awal",
                "Renewal" => "Perpanjangan Kontrak",
                "Amendment" => "Amandemen",
                "Extension" => "Ekstensi",
                "Suspension" => "Penangguhan",
                "Termination" => "Terminasi",
                "Expiry" => "Kedaluwarsa",
                _ => value
            };
        }

        private static string BuildContractStatusLabel(string value)
        {
            return value switch
            {
                "Draft" => "Draft",
                "Active" => "Aktif",
                "Suspended" => "Ditangguhkan",
                "Ended" => "Berakhir",
                "Expired" => "Kedaluwarsa",
                "Terminated" => "Dihentikan",
                "Cancelled" => "Dibatalkan",
                _ => value
            };
        }
    }
}
