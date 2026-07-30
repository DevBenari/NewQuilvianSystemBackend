using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Shared.HumanResource.DTOs;
using QuilvianSystemBackend.Shared.HumanResource.Services;
using System.Data;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services
{
    public class ApprovalDelegationService
    {
        private const string LogCategory = "Corporate.HumanResource.WorkflowManagement";
        private const string DelegationCodePrefix = "DLG-RSMMC-";
        private const string ApprovalReferenceType = "TrxApprovalDelegation";

        private readonly ApplicationDbContext _dbContext;
        private readonly HumanResourceContextService _humanResourceContextService;
        private readonly WorkflowService _workflowService;
        private readonly LoggerService _loggerService;

        public ApprovalDelegationService(
            ApplicationDbContext dbContext,
            HumanResourceContextService humanResourceContextService,
            WorkflowService workflowService,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _humanResourceContextService = humanResourceContextService;
            _workflowService = workflowService;
            _loggerService = loggerService;
        }

        public ApprovalDelegationFilterMetadataResponse GetFilterMetadata()
        {
            return new ApprovalDelegationFilterMetadataResponse
            {
                DefaultFilter = new ApprovalDelegationDefaultFilterResponse(),
                PeriodOptions = BuildOptions(new[]
                {
                    ("today", "Hari Ini"),
                    ("last7days", "7 Hari Terakhir"),
                    ("last30days", "30 Hari Terakhir"),
                    ("thismonth", "Bulan Ini"),
                    ("lastmonth", "Bulan Lalu"),
                    ("custom", "Rentang Tanggal")
                }),
                StatusOptions = BuildOptions(new[]
                {
                    (WorkflowValueConstants.DelegationStatus.Draft, "Draft"),
                    (WorkflowValueConstants.DelegationStatus.Submitted, "Diajukan"),
                    (WorkflowValueConstants.DelegationStatus.Approved, "Disetujui"),
                    (WorkflowValueConstants.DelegationStatus.Active, "Aktif"),
                    (WorkflowValueConstants.DelegationStatus.Expired, "Berakhir"),
                    (WorkflowValueConstants.DelegationStatus.Rejected, "Ditolak"),
                    (WorkflowValueConstants.DelegationStatus.Revoked, "Dicabut"),
                    (WorkflowValueConstants.DelegationStatus.Cancelled, "Dibatalkan")
                }),
                SortOptions = new List<ApprovalDelegationSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal Dibuat" },
                    new() { Value = "delegationNumber", Label = "Nomor Delegasi" },
                    new() { Value = "delegatorName", Label = "Pemberi Delegasi" },
                    new() { Value = "delegateName", Label = "Penerima Delegasi" },
                    new() { Value = "effectiveStartAt", Label = "Mulai Berlaku" },
                    new() { Value = "effectiveEndAt", Label = "Selesai Berlaku" },
                    new() { Value = "delegationStatus", Label = "Status" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };
        }

        public async Task<WorkflowServiceResult<ApprovalDelegationSummaryResponse>> GetSummaryAsync(
            DateTime? startDate,
            DateTime? endDate,
            string? period,
            Guid? delegatorUserId,
            Guid? delegateUserId,
            Guid? approvalDelegationPolicyId,
            Guid? workflowDefinitionId,
            Guid? workflowStepId,
            string? delegationStatus,
            bool? appliesToAllWorkflows,
            bool? isActive,
            string? search,
            CancellationToken cancellationToken = default)
        {
            var actorResult = await GetActorContextAsync(cancellationToken);
            if (!actorResult.Success)
            {
                return WorkflowServiceResult<ApprovalDelegationSummaryResponse>.Fail(
                    actorResult.StatusCode,
                    actorResult.Message);
            }

            await SynchronizeStatusesAsync(cancellationToken);

            var range = ResolveDateRange(startDate, endDate, period);
            if (!range.IsValid)
            {
                return WorkflowServiceResult<ApprovalDelegationSummaryResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    range.ErrorMessage!);
            }

            var query = ApplyFilters(
                BuildBaseQuery(),
                range,
                delegatorUserId,
                delegateUserId,
                approvalDelegationPolicyId,
                workflowDefinitionId,
                workflowStepId,
                delegationStatus,
                appliesToAllWorkflows,
                isActive,
                search);

            var now = DateTime.UtcNow;
            var actor = actorResult.Data!;

            var result = new ApprovalDelegationSummaryResponse
            {
                TotalData = await query.CountAsync(cancellationToken),
                Draft = await query.CountAsync(
                    x => x.DelegationStatus == WorkflowValueConstants.DelegationStatus.Draft,
                    cancellationToken),
                Submitted = await query.CountAsync(
                    x => x.DelegationStatus == WorkflowValueConstants.DelegationStatus.Submitted,
                    cancellationToken),
                Approved = await query.CountAsync(
                    x => x.DelegationStatus == WorkflowValueConstants.DelegationStatus.Approved,
                    cancellationToken),
                Active = await query.CountAsync(
                    x => x.DelegationStatus == WorkflowValueConstants.DelegationStatus.Active,
                    cancellationToken),
                Expired = await query.CountAsync(
                    x => x.DelegationStatus == WorkflowValueConstants.DelegationStatus.Expired,
                    cancellationToken),
                Rejected = await query.CountAsync(
                    x => x.DelegationStatus == WorkflowValueConstants.DelegationStatus.Rejected,
                    cancellationToken),
                Revoked = await query.CountAsync(
                    x => x.DelegationStatus == WorkflowValueConstants.DelegationStatus.Revoked,
                    cancellationToken),
                Cancelled = await query.CountAsync(
                    x => x.DelegationStatus == WorkflowValueConstants.DelegationStatus.Cancelled,
                    cancellationToken),
                DelegatedByCurrentUser = await query.CountAsync(
                    x => x.DelegatorUserId == actor.UserId,
                    cancellationToken),
                DelegatedToCurrentUser = await query.CountAsync(
                    x => x.DelegateUserId == actor.UserId,
                    cancellationToken),
                EffectiveToday = await query.CountAsync(
                    x => x.IsActive &&
                         x.EffectiveStartAt <= now &&
                         x.EffectiveEndAt >= now &&
                         (x.DelegationStatus == WorkflowValueConstants.DelegationStatus.Active ||
                          x.DelegationStatus == WorkflowValueConstants.DelegationStatus.Approved),
                    cancellationToken)
            };

            return WorkflowServiceResult<ApprovalDelegationSummaryResponse>.Ok(
                result,
                "Ringkasan approval delegation berhasil diambil.");
        }

        public async Task<WorkflowServiceResult<PagedResult<ApprovalDelegationListResponse>>> GetPagedAsync(
            DateTime? startDate,
            DateTime? endDate,
            string? period,
            Guid? delegatorUserId,
            Guid? delegateUserId,
            Guid? approvalDelegationPolicyId,
            Guid? workflowDefinitionId,
            Guid? workflowStepId,
            string? delegationStatus,
            bool? appliesToAllWorkflows,
            bool? isActive,
            string? search,
            string? sortBy,
            string? sortDirection,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            await SynchronizeStatusesAsync(cancellationToken);

            var range = ResolveDateRange(startDate, endDate, period);
            if (!range.IsValid)
            {
                return WorkflowServiceResult<PagedResult<ApprovalDelegationListResponse>>.Fail(
                    StatusCodes.Status400BadRequest,
                    range.ErrorMessage!);
            }

            var paging = NormalizePaging(pageNumber, pageSize);
            var query = ApplyFilters(
                BuildBaseQuery(),
                range,
                delegatorUserId,
                delegateUserId,
                approvalDelegationPolicyId,
                workflowDefinitionId,
                workflowStepId,
                delegationStatus,
                appliesToAllWorkflows,
                isActive,
                search);

            var totalData = await query.CountAsync(cancellationToken);
            var now = DateTime.UtcNow;

            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((paging.PageNumber - 1) * paging.PageSize)
                .Take(paging.PageSize)
                .ToListAsync(cancellationToken);

            var items = new List<ApprovalDelegationListResponse>();
            foreach (var entity in entities)
            {
                items.Add(await MapListResponseAsync(entity, now, cancellationToken));
            }

            var result = new PagedResult<ApprovalDelegationListResponse>
            {
                PageNumber = paging.PageNumber,
                PageSize = paging.PageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)paging.PageSize),
                Items = items
            };

            return WorkflowServiceResult<PagedResult<ApprovalDelegationListResponse>>.Ok(
                result,
                "Data approval delegation berhasil diambil.");
        }

        public async Task<WorkflowServiceResult<ApprovalDelegationDetailResponse>> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty)
            {
                return WorkflowServiceResult<ApprovalDelegationDetailResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Approval delegation id tidak valid.");
            }

            var actorResult = await GetActorContextAsync(cancellationToken);
            if (!actorResult.Success)
            {
                return WorkflowServiceResult<ApprovalDelegationDetailResponse>.Fail(
                    actorResult.StatusCode,
                    actorResult.Message);
            }

            await SynchronizeStatusesAsync(cancellationToken);

            var entity = await BuildBaseQuery()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (entity == null)
            {
                return WorkflowServiceResult<ApprovalDelegationDetailResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Approval delegation tidak ditemukan.");
            }

            var response = await MapDetailResponseAsync(
                entity,
                actorResult.Data!,
                cancellationToken);

            return WorkflowServiceResult<ApprovalDelegationDetailResponse>.Ok(
                response,
                "Detail approval delegation berhasil diambil.");
        }

        public async Task<WorkflowServiceResult<ApprovalDelegationDetailResponse>> CreateDraftAsync(
            CreateApprovalDelegationRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                return WorkflowServiceResult<ApprovalDelegationDetailResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Request approval delegation wajib diisi.");
            }

            var actorResult = await GetActorContextAsync(cancellationToken);
            if (!actorResult.Success)
            {
                return WorkflowServiceResult<ApprovalDelegationDetailResponse>.Fail(
                    actorResult.StatusCode,
                    actorResult.Message);
            }

            var actor = actorResult.Data!;
            var validation = await ValidateRequestAsync(
                null,
                actor,
                request,
                validateNoticePeriod: false,
                cancellationToken);

            if (!validation.IsValid)
            {
                return WorkflowServiceResult<ApprovalDelegationDetailResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage!);
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            try
            {
                var delegateUser = validation.DelegateUser!;
                var policy = validation.Policy;
                var now = DateTime.UtcNow;

                var entity = new TrxApprovalDelegation
                {
                    Id = Guid.NewGuid(),
                    DelegatorUserId = actor.UserId,
                    DelegatorWorkforceProfileId = actor.WorkforceProfileId,
                    DelegateUserId = delegateUser.Id,
                    DelegateWorkforceProfileId = delegateUser.WorkforceProfileId,
                    ApprovalDelegationPolicyId = NormalizeGuid(request.ApprovalDelegationPolicyId),
                    WorkflowDefinitionId = NormalizeGuid(request.WorkflowDefinitionId),
                    WorkflowStepId = NormalizeGuid(request.WorkflowStepId),
                    DelegationNumber = await GenerateDelegationNumberAsync(cancellationToken),
                    DelegationStatus = WorkflowValueConstants.DelegationStatus.Draft,
                    EffectiveStartAt = NormalizeUtc(request.EffectiveStartAt),
                    EffectiveEndAt = NormalizeUtc(request.EffectiveEndAt),
                    DelegationReason = NormalizeNullableString(request.DelegationReason),
                    AppliesToAllWorkflows = request.AppliesToAllWorkflows,
                    AllowSubDelegation = policy?.AllowSubDelegation == true && request.AllowSubDelegation,
                    PreserveDelegatorAccountability = policy?.PreserveDelegatorAccountability ??
                                                     request.PreserveDelegatorAccountability,
                    ScopeDefinitionJson = NormalizeJson(request.ScopeDefinitionJson),
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actor.UserId,
                    IsDelete = false,
                    IsCancel = false
                };

                _dbContext.Set<TrxApprovalDelegation>().Add(entity);
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                await _loggerService.InfoAsync(
                    LogCategory,
                    "ApprovalDelegation.CreateDraft",
                    "Membuat draft approval delegation.",
                    new { entity.Id, entity.DelegationNumber, entity.DelegatorUserId, entity.DelegateUserId });

                return await GetByIdAsync(entity.Id, cancellationToken);
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync(cancellationToken);
                return WorkflowServiceResult<ApprovalDelegationDetailResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Approval delegation gagal dibuat karena terjadi konflik data. Silakan ulangi proses.");
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<WorkflowServiceResult<ApprovalDelegationDetailResponse>> UpdateDraftAsync(
            Guid id,
            UpdateApprovalDelegationRequest request,
            CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty || request == null)
            {
                return WorkflowServiceResult<ApprovalDelegationDetailResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Approval delegation id dan request wajib diisi.");
            }

            var actorResult = await GetActorContextAsync(cancellationToken);
            if (!actorResult.Success)
            {
                return WorkflowServiceResult<ApprovalDelegationDetailResponse>.Fail(
                    actorResult.StatusCode,
                    actorResult.Message);
            }

            var actor = actorResult.Data!;
            var entity = await _dbContext.Set<TrxApprovalDelegation>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
            {
                return WorkflowServiceResult<ApprovalDelegationDetailResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Approval delegation tidak ditemukan.");
            }

            if (entity.DelegatorUserId != actor.UserId)
            {
                return WorkflowServiceResult<ApprovalDelegationDetailResponse>.Fail(
                    StatusCodes.Status403Forbidden,
                    "Hanya pemberi delegasi yang dapat mengubah draft.");
            }

            if (!string.Equals(
                    entity.DelegationStatus,
                    WorkflowValueConstants.DelegationStatus.Draft,
                    StringComparison.OrdinalIgnoreCase))
            {
                return WorkflowServiceResult<ApprovalDelegationDetailResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Hanya approval delegation berstatus Draft yang dapat diubah.");
            }

            var validation = await ValidateRequestAsync(
                id,
                actor,
                request,
                validateNoticePeriod: false,
                cancellationToken);

            if (!validation.IsValid)
            {
                return WorkflowServiceResult<ApprovalDelegationDetailResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage!);
            }

            var delegateUser = validation.DelegateUser!;
            var policy = validation.Policy;
            var now = DateTime.UtcNow;

            entity.DelegateUserId = delegateUser.Id;
            entity.DelegateWorkforceProfileId = delegateUser.WorkforceProfileId;
            entity.ApprovalDelegationPolicyId = NormalizeGuid(request.ApprovalDelegationPolicyId);
            entity.WorkflowDefinitionId = NormalizeGuid(request.WorkflowDefinitionId);
            entity.WorkflowStepId = NormalizeGuid(request.WorkflowStepId);
            entity.EffectiveStartAt = NormalizeUtc(request.EffectiveStartAt);
            entity.EffectiveEndAt = NormalizeUtc(request.EffectiveEndAt);
            entity.DelegationReason = NormalizeNullableString(request.DelegationReason);
            entity.AppliesToAllWorkflows = request.AppliesToAllWorkflows;
            entity.AllowSubDelegation = policy?.AllowSubDelegation == true && request.AllowSubDelegation;
            entity.PreserveDelegatorAccountability = policy?.PreserveDelegatorAccountability ??
                                                     request.PreserveDelegatorAccountability;
            entity.ScopeDefinitionJson = NormalizeJson(request.ScopeDefinitionJson);
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor.UserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "ApprovalDelegation.UpdateDraft",
                "Mengubah draft approval delegation.",
                new { entity.Id, entity.DelegationNumber });

            return await GetByIdAsync(entity.Id, cancellationToken);
        }

        public async Task<WorkflowServiceResult<ApprovalDelegationDetailResponse>> SubmitAsync(
            Guid id,
            SubmitApprovalDelegationRequest? request,
            CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty)
            {
                return WorkflowServiceResult<ApprovalDelegationDetailResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Approval delegation id tidak valid.");
            }

            var actorResult = await GetActorContextAsync(cancellationToken);
            if (!actorResult.Success)
            {
                return WorkflowServiceResult<ApprovalDelegationDetailResponse>.Fail(
                    actorResult.StatusCode,
                    actorResult.Message);
            }

            var actor = actorResult.Data!;
            string? approvalWorkflowCode;
            bool startGenericWorkflow;
            bool activateImmediately;

            await using (var transaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken))
            {
                try
                {
                    var entity = await _dbContext.Set<TrxApprovalDelegation>()
                        .Include(x => x.ApprovalDelegationPolicy)
                        .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

                    if (entity == null)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return WorkflowServiceResult<ApprovalDelegationDetailResponse>.Fail(
                            StatusCodes.Status404NotFound,
                            "Approval delegation tidak ditemukan.");
                    }

                    if (entity.DelegatorUserId != actor.UserId)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return WorkflowServiceResult<ApprovalDelegationDetailResponse>.Fail(
                            StatusCodes.Status403Forbidden,
                            "Hanya pemberi delegasi yang dapat mengajukan approval delegation.");
                    }

                    if (!string.Equals(
                            entity.DelegationStatus,
                            WorkflowValueConstants.DelegationStatus.Draft,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return WorkflowServiceResult<ApprovalDelegationDetailResponse>.Fail(
                            StatusCodes.Status409Conflict,
                            "Hanya approval delegation berstatus Draft yang dapat diajukan.");
                    }

                    var entityRequest = ToRequest(entity);
                    var validation = await ValidateRequestAsync(
                        entity.Id,
                        actor,
                        entityRequest,
                        validateNoticePeriod: true,
                        cancellationToken);

                    if (!validation.IsValid)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return WorkflowServiceResult<ApprovalDelegationDetailResponse>.Fail(
                            StatusCodes.Status400BadRequest,
                            validation.ErrorMessage!);
                    }

                    var overlapMessage = await ValidateNoOverlapAsync(
                        entity,
                        cancellationToken);

                    if (overlapMessage != null)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return WorkflowServiceResult<ApprovalDelegationDetailResponse>.Fail(
                            StatusCodes.Status409Conflict,
                            overlapMessage);
                    }

                    var loopMessage = await ValidateNoDelegationLoopAsync(
                        entity,
                        cancellationToken);

                    if (loopMessage != null)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return WorkflowServiceResult<ApprovalDelegationDetailResponse>.Fail(
                            StatusCodes.Status409Conflict,
                            loopMessage);
                    }

                    var subDelegationMessage = await ValidateSubDelegationAsync(
                        entity,
                        cancellationToken);

                    if (subDelegationMessage != null)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return WorkflowServiceResult<ApprovalDelegationDetailResponse>.Fail(
                            StatusCodes.Status409Conflict,
                            subDelegationMessage);
                    }

                    var now = DateTime.UtcNow;
                    var policy = validation.Policy;
                    approvalWorkflowCode = NormalizeNullableString(policy?.ApprovalWorkflowCode);
                    startGenericWorkflow = !string.IsNullOrWhiteSpace(approvalWorkflowCode);
                    var requiresManualApproval = policy?.RequireManagerApproval == true ||
                                                 policy?.RequireHrVerification == true;

                    entity.SubmittedAt = now;
                    entity.UpdateDateTime = now;
                    entity.UpdateBy = actor.UserId;
                    entity.RevocationReason = null;

                    if (startGenericWorkflow || requiresManualApproval)
                    {
                        entity.DelegationStatus = WorkflowValueConstants.DelegationStatus.Submitted;
                        entity.IsActive = true;
                        activateImmediately = false;
                    }
                    else
                    {
                        entity.ApprovedAt = now;
                        entity.ApprovedByUserId = null;
                        activateImmediately = entity.EffectiveStartAt <= now &&
                                              entity.EffectiveEndAt >= now;
                        entity.DelegationStatus = activateImmediately
                            ? WorkflowValueConstants.DelegationStatus.Active
                            : WorkflowValueConstants.DelegationStatus.Approved;
                        entity.IsActive = true;
                    }

                    await _dbContext.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                }
                catch
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            }

            if (startGenericWorkflow)
            {
                var workflowResult = await StartGenericApprovalWorkflowAsync(
                    id,
                    approvalWorkflowCode!,
                    request?.Comment,
                    cancellationToken);

                if (!workflowResult.Success)
                {
                    await RevertSubmittedToDraftAsync(id, actor.UserId, cancellationToken);
                    return WorkflowServiceResult<ApprovalDelegationDetailResponse>.Fail(
                        workflowResult.StatusCode,
                        workflowResult.Message);
                }
            }
            else if (activateImmediately)
            {
                await ApplyDelegationToOpenAssignmentsAsync(id, cancellationToken);
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "ApprovalDelegation.Submit",
                "Mengajukan approval delegation.",
                new { Id = id, ApprovalWorkflowCode = approvalWorkflowCode });

            return await GetByIdAsync(id, cancellationToken);
        }

        public async Task<WorkflowServiceResult<ApprovalDelegationDetailResponse>> ApproveAsync(
            Guid id,
            ApproveApprovalDelegationRequest? request,
            CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty)
            {
                return WorkflowServiceResult<ApprovalDelegationDetailResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Approval delegation id tidak valid.");
            }

            var actorResult = await GetActorContextAsync(cancellationToken);
            if (!actorResult.Success)
            {
                return WorkflowServiceResult<ApprovalDelegationDetailResponse>.Fail(
                    actorResult.StatusCode,
                    actorResult.Message);
            }

            var actor = actorResult.Data!;
            bool activateImmediately;

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            try
            {
                var entity = await _dbContext.Set<TrxApprovalDelegation>()
                    .Include(x => x.ApprovalDelegationPolicy)
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

                if (entity == null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return WorkflowServiceResult<ApprovalDelegationDetailResponse>.Fail(
                        StatusCodes.Status404NotFound,
                        "Approval delegation tidak ditemukan.");
                }

                if (!string.Equals(
                        entity.DelegationStatus,
                        WorkflowValueConstants.DelegationStatus.Submitted,
                        StringComparison.OrdinalIgnoreCase))
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return WorkflowServiceResult<ApprovalDelegationDetailResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        "Hanya approval delegation berstatus Submitted yang dapat disetujui.");
                }

                if (!string.IsNullOrWhiteSpace(entity.ApprovalDelegationPolicy?.ApprovalWorkflowCode))
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return WorkflowServiceResult<ApprovalDelegationDetailResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        "Approval delegation ini menggunakan Workflow Engine generik. Lakukan approval melalui approval assignment workflow terkait.");
                }

                if (actor.UserId == entity.DelegatorUserId || actor.UserId == entity.DelegateUserId)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return WorkflowServiceResult<ApprovalDelegationDetailResponse>.Fail(
                        StatusCodes.Status403Forbidden,
                        "Pemberi atau penerima delegasi tidak dapat menyetujui pengajuan delegasinya sendiri.");
                }

                var overlapMessage = await ValidateNoOverlapAsync(entity, cancellationToken);
                if (overlapMessage != null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return WorkflowServiceResult<ApprovalDelegationDetailResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        overlapMessage);
                }

                var loopMessage = await ValidateNoDelegationLoopAsync(entity, cancellationToken);
                if (loopMessage != null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return WorkflowServiceResult<ApprovalDelegationDetailResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        loopMessage);
                }

                var now = DateTime.UtcNow;
                if (entity.EffectiveEndAt < now)
                {
                    entity.DelegationStatus = WorkflowValueConstants.DelegationStatus.Expired;
                    entity.IsActive = false;
                    entity.UpdateDateTime = now;
                    entity.UpdateBy = actor.UserId;
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    return WorkflowServiceResult<ApprovalDelegationDetailResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        "Periode approval delegation sudah berakhir.");
                }

                activateImmediately = entity.EffectiveStartAt <= now;
                entity.DelegationStatus = activateImmediately
                    ? WorkflowValueConstants.DelegationStatus.Active
                    : WorkflowValueConstants.DelegationStatus.Approved;
                entity.ApprovedAt = now;
                entity.ApprovedByUserId = actor.UserId;
                entity.IsActive = true;
                entity.UpdateDateTime = now;
                entity.UpdateBy = actor.UserId;

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }

            if (activateImmediately)
            {
                await ApplyDelegationToOpenAssignmentsAsync(id, cancellationToken);
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "ApprovalDelegation.Approve",
                "Menyetujui approval delegation.",
                new { Id = id, ApprovedBy = actor.UserId, Comment = request?.Comment });

            return await GetByIdAsync(id, cancellationToken);
        }

        public async Task<WorkflowServiceResult<ApprovalDelegationDetailResponse>> RejectAsync(
            Guid id,
            RejectApprovalDelegationRequest request,
            CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty || request == null || string.IsNullOrWhiteSpace(request.Reason))
            {
                return WorkflowServiceResult<ApprovalDelegationDetailResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Approval delegation id dan alasan penolakan wajib diisi.");
            }

            var actorResult = await GetActorContextAsync(cancellationToken);
            if (!actorResult.Success)
            {
                return WorkflowServiceResult<ApprovalDelegationDetailResponse>.Fail(
                    actorResult.StatusCode,
                    actorResult.Message);
            }

            var actor = actorResult.Data!;
            var entity = await _dbContext.Set<TrxApprovalDelegation>()
                .Include(x => x.ApprovalDelegationPolicy)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
            {
                return WorkflowServiceResult<ApprovalDelegationDetailResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Approval delegation tidak ditemukan.");
            }

            if (!string.Equals(
                    entity.DelegationStatus,
                    WorkflowValueConstants.DelegationStatus.Submitted,
                    StringComparison.OrdinalIgnoreCase))
            {
                return WorkflowServiceResult<ApprovalDelegationDetailResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Hanya approval delegation berstatus Submitted yang dapat ditolak.");
            }

            if (!string.IsNullOrWhiteSpace(entity.ApprovalDelegationPolicy?.ApprovalWorkflowCode))
            {
                return WorkflowServiceResult<ApprovalDelegationDetailResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Approval delegation ini menggunakan Workflow Engine generik. Lakukan reject melalui approval assignment workflow terkait.");
            }

            if (actor.UserId == entity.DelegatorUserId || actor.UserId == entity.DelegateUserId)
            {
                return WorkflowServiceResult<ApprovalDelegationDetailResponse>.Fail(
                    StatusCodes.Status403Forbidden,
                    "Pemberi atau penerima delegasi tidak dapat menolak pengajuan delegasinya sendiri.");
            }

            var now = DateTime.UtcNow;
            entity.DelegationStatus = WorkflowValueConstants.DelegationStatus.Rejected;
            entity.RevocationReason = request.Reason.Trim();
            entity.IsActive = false;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor.UserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "ApprovalDelegation.Reject",
                "Menolak approval delegation.",
                new { entity.Id, entity.DelegationNumber, RejectedBy = actor.UserId, request.Reason });

            return await GetByIdAsync(id, cancellationToken);
        }

        public async Task<WorkflowServiceResult<ApprovalDelegationDetailResponse>> ActivateAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty)
            {
                return WorkflowServiceResult<ApprovalDelegationDetailResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Approval delegation id tidak valid.");
            }

            var actorResult = await GetActorContextAsync(cancellationToken);
            if (!actorResult.Success)
            {
                return WorkflowServiceResult<ApprovalDelegationDetailResponse>.Fail(
                    actorResult.StatusCode,
                    actorResult.Message);
            }

            var actor = actorResult.Data!;
            var entity = await _dbContext.Set<TrxApprovalDelegation>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
            {
                return WorkflowServiceResult<ApprovalDelegationDetailResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Approval delegation tidak ditemukan.");
            }

            if (!string.Equals(
                    entity.DelegationStatus,
                    WorkflowValueConstants.DelegationStatus.Approved,
                    StringComparison.OrdinalIgnoreCase))
            {
                return WorkflowServiceResult<ApprovalDelegationDetailResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Hanya approval delegation berstatus Approved yang dapat diaktifkan.");
            }

            var now = DateTime.UtcNow;
            if (entity.EffectiveStartAt > now)
            {
                return WorkflowServiceResult<ApprovalDelegationDetailResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Approval delegation belum memasuki waktu mulai berlaku.");
            }

            if (entity.EffectiveEndAt < now)
            {
                entity.DelegationStatus = WorkflowValueConstants.DelegationStatus.Expired;
                entity.IsActive = false;
                entity.UpdateDateTime = now;
                entity.UpdateBy = actor.UserId;
                await _dbContext.SaveChangesAsync(cancellationToken);

                return WorkflowServiceResult<ApprovalDelegationDetailResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Periode approval delegation sudah berakhir.");
            }

            entity.DelegationStatus = WorkflowValueConstants.DelegationStatus.Active;
            entity.IsActive = true;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor.UserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            await ApplyDelegationToOpenAssignmentsAsync(id, cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "ApprovalDelegation.Activate",
                "Mengaktifkan approval delegation.",
                new { entity.Id, entity.DelegationNumber, ActivatedBy = actor.UserId });

            return await GetByIdAsync(id, cancellationToken);
        }

        public async Task<WorkflowServiceResult<ApprovalDelegationDetailResponse>> RevokeAsync(
            Guid id,
            RevokeApprovalDelegationRequest request,
            CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty || request == null || string.IsNullOrWhiteSpace(request.Reason))
            {
                return WorkflowServiceResult<ApprovalDelegationDetailResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Approval delegation id dan alasan pencabutan wajib diisi.");
            }

            var actorResult = await GetActorContextAsync(cancellationToken);
            if (!actorResult.Success)
            {
                return WorkflowServiceResult<ApprovalDelegationDetailResponse>.Fail(
                    actorResult.StatusCode,
                    actorResult.Message);
            }

            var actor = actorResult.Data!;
            var entity = await _dbContext.Set<TrxApprovalDelegation>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
            {
                return WorkflowServiceResult<ApprovalDelegationDetailResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Approval delegation tidak ditemukan.");
            }

            if (!string.Equals(
                    entity.DelegationStatus,
                    WorkflowValueConstants.DelegationStatus.Active,
                    StringComparison.OrdinalIgnoreCase))
            {
                return WorkflowServiceResult<ApprovalDelegationDetailResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Hanya approval delegation berstatus Active yang dapat dicabut.");
            }

            var now = DateTime.UtcNow;
            entity.DelegationStatus = WorkflowValueConstants.DelegationStatus.Revoked;
            entity.RevokedAt = now;
            entity.RevokedByUserId = actor.UserId;
            entity.RevocationReason = request.Reason.Trim();
            entity.IsActive = false;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor.UserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            await RestoreOpenAssignmentsAsync(id, actor.UserId, cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "ApprovalDelegation.Revoke",
                "Mencabut approval delegation.",
                new { entity.Id, entity.DelegationNumber, RevokedBy = actor.UserId, request.Reason });

            return await GetByIdAsync(id, cancellationToken);
        }

        public async Task<WorkflowServiceResult<ApprovalDelegationDetailResponse>> CancelAsync(
            Guid id,
            CancelApprovalDelegationRequest request,
            CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty || request == null || string.IsNullOrWhiteSpace(request.Reason))
            {
                return WorkflowServiceResult<ApprovalDelegationDetailResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Approval delegation id dan alasan pembatalan wajib diisi.");
            }

            var actorResult = await GetActorContextAsync(cancellationToken);
            if (!actorResult.Success)
            {
                return WorkflowServiceResult<ApprovalDelegationDetailResponse>.Fail(
                    actorResult.StatusCode,
                    actorResult.Message);
            }

            var actor = actorResult.Data!;
            var entity = await _dbContext.Set<TrxApprovalDelegation>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
            {
                return WorkflowServiceResult<ApprovalDelegationDetailResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Approval delegation tidak ditemukan.");
            }

            if (entity.DelegatorUserId != actor.UserId)
            {
                return WorkflowServiceResult<ApprovalDelegationDetailResponse>.Fail(
                    StatusCodes.Status403Forbidden,
                    "Hanya pemberi delegasi yang dapat membatalkan approval delegation.");
            }

            var allowed = string.Equals(
                              entity.DelegationStatus,
                              WorkflowValueConstants.DelegationStatus.Draft,
                              StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(
                              entity.DelegationStatus,
                              WorkflowValueConstants.DelegationStatus.Submitted,
                              StringComparison.OrdinalIgnoreCase) ||
                          (string.Equals(
                               entity.DelegationStatus,
                               WorkflowValueConstants.DelegationStatus.Approved,
                               StringComparison.OrdinalIgnoreCase) &&
                           entity.EffectiveStartAt > DateTime.UtcNow);

            if (!allowed)
            {
                return WorkflowServiceResult<ApprovalDelegationDetailResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Approval delegation tidak berada pada status yang dapat dibatalkan. Gunakan revoke untuk delegasi yang sudah aktif.");
            }

            var now = DateTime.UtcNow;
            entity.DelegationStatus = WorkflowValueConstants.DelegationStatus.Cancelled;
            entity.RevocationReason = request.Reason.Trim();
            entity.CancelDateTime = now;
            entity.CancelBy = actor.UserId;
            entity.IsCancel = true;
            entity.IsActive = false;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor.UserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            await CancelRelatedGenericWorkflowAsync(entity.Id, request.Reason, cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "ApprovalDelegation.Cancel",
                "Membatalkan approval delegation.",
                new { entity.Id, entity.DelegationNumber, CancelledBy = actor.UserId, request.Reason });

            return await GetByIdAsync(id, cancellationToken);
        }

        public async Task<WorkflowServiceResult<object>> DeleteAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty)
            {
                return WorkflowServiceResult<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Approval delegation id tidak valid.");
            }

            var actorResult = await GetActorContextAsync(cancellationToken);
            if (!actorResult.Success)
            {
                return WorkflowServiceResult<object>.Fail(
                    actorResult.StatusCode,
                    actorResult.Message);
            }

            var actor = actorResult.Data!;
            var entity = await _dbContext.Set<TrxApprovalDelegation>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
            {
                return WorkflowServiceResult<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Approval delegation tidak ditemukan.");
            }

            if (entity.DelegatorUserId != actor.UserId)
            {
                return WorkflowServiceResult<object>.Fail(
                    StatusCodes.Status403Forbidden,
                    "Hanya pemberi delegasi yang dapat menghapus approval delegation.");
            }

            var deletable = string.Equals(
                                entity.DelegationStatus,
                                WorkflowValueConstants.DelegationStatus.Draft,
                                StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(
                                entity.DelegationStatus,
                                WorkflowValueConstants.DelegationStatus.Rejected,
                                StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(
                                entity.DelegationStatus,
                                WorkflowValueConstants.DelegationStatus.Cancelled,
                                StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(
                                entity.DelegationStatus,
                                WorkflowValueConstants.DelegationStatus.Expired,
                                StringComparison.OrdinalIgnoreCase);

            if (!deletable)
            {
                return WorkflowServiceResult<object>.Fail(
                    StatusCodes.Status409Conflict,
                    "Approval delegation hanya dapat dihapus ketika berstatus Draft, Rejected, Cancelled, atau Expired.");
            }

            var isUsed = await _dbContext.Set<TrxWorkflowApproverAssignment>()
                .AsNoTracking()
                .AnyAsync(
                    x => x.ApprovalDelegationId == id && !x.IsDelete,
                    cancellationToken);

            if (isUsed)
            {
                return WorkflowServiceResult<object>.Fail(
                    StatusCodes.Status409Conflict,
                    "Approval delegation tidak dapat dihapus karena sudah pernah digunakan pada workflow assignment.");
            }

            var now = DateTime.UtcNow;
            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actor.UserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor.UserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "ApprovalDelegation.Delete",
                "Menghapus approval delegation secara soft delete.",
                new { entity.Id, entity.DelegationNumber, DeletedBy = actor.UserId });

            return WorkflowServiceResult<object>.Ok(
                new { entity.Id, entity.DelegationNumber },
                "Approval delegation berhasil dihapus.");
        }

        private IQueryable<TrxApprovalDelegation> BuildBaseQuery()
        {
            return _dbContext.Set<TrxApprovalDelegation>()
                .AsNoTracking()
                .Where(x => !x.IsDelete)
                .Include(x => x.DelegatorUser)
                .Include(x => x.DelegatorWorkforceProfile)
                .Include(x => x.DelegateUser)
                .Include(x => x.DelegateWorkforceProfile)
                .Include(x => x.ApprovalDelegationPolicy)
                .Include(x => x.WorkflowDefinition)
                .Include(x => x.WorkflowStep)
                .Include(x => x.ApprovedByUser)
                .Include(x => x.RevokedByUser);
        }

        private static IQueryable<TrxApprovalDelegation> ApplyFilters(
            IQueryable<TrxApprovalDelegation> query,
            DateRangeResult range,
            Guid? delegatorUserId,
            Guid? delegateUserId,
            Guid? approvalDelegationPolicyId,
            Guid? workflowDefinitionId,
            Guid? workflowStepId,
            string? delegationStatus,
            bool? appliesToAllWorkflows,
            bool? isActive,
            string? search)
        {
            if (range.Start.HasValue)
            {
                query = query.Where(x => x.CreateDateTime >= range.Start.Value);
            }

            if (range.EndExclusive.HasValue)
            {
                query = query.Where(x => x.CreateDateTime < range.EndExclusive.Value);
            }

            if (delegatorUserId.HasValue && delegatorUserId.Value != Guid.Empty)
            {
                query = query.Where(x => x.DelegatorUserId == delegatorUserId.Value);
            }

            if (delegateUserId.HasValue && delegateUserId.Value != Guid.Empty)
            {
                query = query.Where(x => x.DelegateUserId == delegateUserId.Value);
            }

            if (approvalDelegationPolicyId.HasValue && approvalDelegationPolicyId.Value != Guid.Empty)
            {
                query = query.Where(x => x.ApprovalDelegationPolicyId == approvalDelegationPolicyId.Value);
            }

            if (workflowDefinitionId.HasValue && workflowDefinitionId.Value != Guid.Empty)
            {
                query = query.Where(x => x.WorkflowDefinitionId == workflowDefinitionId.Value);
            }

            if (workflowStepId.HasValue && workflowStepId.Value != Guid.Empty)
            {
                query = query.Where(x => x.WorkflowStepId == workflowStepId.Value);
            }

            if (!string.IsNullOrWhiteSpace(delegationStatus))
            {
                var normalizedStatus = delegationStatus.Trim();
                query = query.Where(x => x.DelegationStatus == normalizedStatus);
            }

            if (appliesToAllWorkflows.HasValue)
            {
                query = query.Where(x => x.AppliesToAllWorkflows == appliesToAllWorkflows.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.DelegationNumber.ToLower().Contains(keyword) ||
                    x.DelegationReason != null && x.DelegationReason.ToLower().Contains(keyword) ||
                    x.DelegatorUser != null &&
                    ((x.DelegatorUser.DisplayName != null && x.DelegatorUser.DisplayName.ToLower().Contains(keyword)) ||
                     (x.DelegatorUser.UserName != null && x.DelegatorUser.UserName.ToLower().Contains(keyword)) ||
                     (x.DelegatorUser.Email != null && x.DelegatorUser.Email.ToLower().Contains(keyword))) ||
                    x.DelegateUser != null &&
                    ((x.DelegateUser.DisplayName != null && x.DelegateUser.DisplayName.ToLower().Contains(keyword)) ||
                     (x.DelegateUser.UserName != null && x.DelegateUser.UserName.ToLower().Contains(keyword)) ||
                     (x.DelegateUser.Email != null && x.DelegateUser.Email.ToLower().Contains(keyword))) ||
                    x.ApprovalDelegationPolicy != null &&
                    x.ApprovalDelegationPolicy.DelegationPolicyName.ToLower().Contains(keyword) ||
                    x.WorkflowDefinition != null &&
                    (x.WorkflowDefinition.WorkflowCode.ToLower().Contains(keyword) ||
                     x.WorkflowDefinition.WorkflowName.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<TrxApprovalDelegation> ApplySorting(
            IQueryable<TrxApprovalDelegation> query,
            string? sortBy,
            string? sortDirection)
        {
            var descending = string.Equals(
                sortDirection,
                "desc",
                StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "createDateTime").Trim().ToLowerInvariant() switch
            {
                "delegationnumber" => descending
                    ? query.OrderByDescending(x => x.DelegationNumber)
                    : query.OrderBy(x => x.DelegationNumber),
                "delegatorname" => descending
                    ? query.OrderByDescending(x => x.DelegatorWorkforceProfile != null
                        ? x.DelegatorWorkforceProfile.DisplayName
                        : x.DelegatorUser != null ? x.DelegatorUser.DisplayName : string.Empty)
                    : query.OrderBy(x => x.DelegatorWorkforceProfile != null
                        ? x.DelegatorWorkforceProfile.DisplayName
                        : x.DelegatorUser != null ? x.DelegatorUser.DisplayName : string.Empty),
                "delegatename" => descending
                    ? query.OrderByDescending(x => x.DelegateWorkforceProfile != null
                        ? x.DelegateWorkforceProfile.DisplayName
                        : x.DelegateUser != null ? x.DelegateUser.DisplayName : string.Empty)
                    : query.OrderBy(x => x.DelegateWorkforceProfile != null
                        ? x.DelegateWorkforceProfile.DisplayName
                        : x.DelegateUser != null ? x.DelegateUser.DisplayName : string.Empty),
                "effectivestartat" => descending
                    ? query.OrderByDescending(x => x.EffectiveStartAt)
                    : query.OrderBy(x => x.EffectiveStartAt),
                "effectiveendat" => descending
                    ? query.OrderByDescending(x => x.EffectiveEndAt)
                    : query.OrderBy(x => x.EffectiveEndAt),
                "delegationstatus" => descending
                    ? query.OrderByDescending(x => x.DelegationStatus)
                    : query.OrderBy(x => x.DelegationStatus),
                _ => descending
                    ? query.OrderByDescending(x => x.CreateDateTime).ThenByDescending(x => x.Id)
                    : query.OrderBy(x => x.CreateDateTime).ThenBy(x => x.Id)
            };
        }

        private async Task<ApprovalDelegationListResponse> MapListResponseAsync(
            TrxApprovalDelegation entity,
            DateTime now,
            CancellationToken cancellationToken)
        {
            var appliedAssignmentCount = await _dbContext.Set<TrxWorkflowApproverAssignment>()
                .AsNoTracking()
                .CountAsync(
                    x => x.ApprovalDelegationId == entity.Id && !x.IsDelete,
                    cancellationToken);

            var createByName = await GetUserDisplayNameAsync(entity.CreateBy, cancellationToken);

            return new ApprovalDelegationListResponse
            {
                Id = entity.Id,
                DelegationNumber = entity.DelegationNumber,
                DelegationStatus = entity.DelegationStatus,
                DelegatorUserId = entity.DelegatorUserId,
                DelegatorWorkforceProfileId = entity.DelegatorWorkforceProfileId,
                DelegatorProfileCode = entity.DelegatorWorkforceProfile?.ProfileCode,
                DelegatorName = ResolveUserName(entity.DelegatorWorkforceProfile?.DisplayName, entity.DelegatorUser),
                DelegateUserId = entity.DelegateUserId,
                DelegateWorkforceProfileId = entity.DelegateWorkforceProfileId,
                DelegateProfileCode = entity.DelegateWorkforceProfile?.ProfileCode,
                DelegateName = ResolveUserName(entity.DelegateWorkforceProfile?.DisplayName, entity.DelegateUser),
                ApprovalDelegationPolicyId = entity.ApprovalDelegationPolicyId,
                DelegationPolicyCode = entity.ApprovalDelegationPolicy?.DelegationPolicyCode,
                DelegationPolicyName = entity.ApprovalDelegationPolicy?.DelegationPolicyName,
                WorkflowDefinitionId = entity.WorkflowDefinitionId,
                WorkflowCode = entity.WorkflowDefinition?.WorkflowCode,
                WorkflowName = entity.WorkflowDefinition?.WorkflowName,
                WorkflowStepId = entity.WorkflowStepId,
                WorkflowStepCode = entity.WorkflowStep?.StepCode,
                WorkflowStepName = entity.WorkflowStep?.StepName,
                EffectiveStartAt = entity.EffectiveStartAt,
                EffectiveEndAt = entity.EffectiveEndAt,
                DelegationDurationDays = CalculateDurationDays(entity.EffectiveStartAt, entity.EffectiveEndAt),
                AppliesToAllWorkflows = entity.AppliesToAllWorkflows,
                AllowSubDelegation = entity.AllowSubDelegation,
                PreserveDelegatorAccountability = entity.PreserveDelegatorAccountability,
                IsCurrentlyEffective = entity.IsActive &&
                                       entity.EffectiveStartAt <= now &&
                                       entity.EffectiveEndAt >= now &&
                                       (entity.DelegationStatus == WorkflowValueConstants.DelegationStatus.Active ||
                                        entity.DelegationStatus == WorkflowValueConstants.DelegationStatus.Approved),
                IsActive = entity.IsActive,
                AppliedAssignmentCount = appliedAssignmentCount,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : entity.CreateBy,
                CreateByName = createByName
            };
        }

        private async Task<ApprovalDelegationDetailResponse> MapDetailResponseAsync(
            TrxApprovalDelegation entity,
            HumanResourceUserContextDto actor,
            CancellationToken cancellationToken)
        {
            var list = await MapListResponseAsync(entity, DateTime.UtcNow, cancellationToken);
            var relatedWorkflow = await GetRelatedWorkflowAsync(entity.Id, cancellationToken);

            return new ApprovalDelegationDetailResponse
            {
                Id = list.Id,
                DelegationNumber = list.DelegationNumber,
                DelegationStatus = list.DelegationStatus,
                DelegatorUserId = list.DelegatorUserId,
                DelegatorWorkforceProfileId = list.DelegatorWorkforceProfileId,
                DelegatorProfileCode = list.DelegatorProfileCode,
                DelegatorName = list.DelegatorName,
                DelegateUserId = list.DelegateUserId,
                DelegateWorkforceProfileId = list.DelegateWorkforceProfileId,
                DelegateProfileCode = list.DelegateProfileCode,
                DelegateName = list.DelegateName,
                ApprovalDelegationPolicyId = list.ApprovalDelegationPolicyId,
                DelegationPolicyCode = list.DelegationPolicyCode,
                DelegationPolicyName = list.DelegationPolicyName,
                WorkflowDefinitionId = list.WorkflowDefinitionId,
                WorkflowCode = list.WorkflowCode,
                WorkflowName = list.WorkflowName,
                WorkflowStepId = list.WorkflowStepId,
                WorkflowStepCode = list.WorkflowStepCode,
                WorkflowStepName = list.WorkflowStepName,
                EffectiveStartAt = list.EffectiveStartAt,
                EffectiveEndAt = list.EffectiveEndAt,
                DelegationDurationDays = list.DelegationDurationDays,
                AppliesToAllWorkflows = list.AppliesToAllWorkflows,
                AllowSubDelegation = list.AllowSubDelegation,
                PreserveDelegatorAccountability = list.PreserveDelegatorAccountability,
                IsCurrentlyEffective = list.IsCurrentlyEffective,
                IsActive = list.IsActive,
                AppliedAssignmentCount = list.AppliedAssignmentCount,
                CreateDateTime = list.CreateDateTime,
                CreateBy = list.CreateBy,
                CreateByName = list.CreateByName,
                DelegationReason = entity.DelegationReason,
                ApprovalWorkflowCode = entity.ApprovalDelegationPolicy?.ApprovalWorkflowCode,
                RequiresManagerApproval = entity.ApprovalDelegationPolicy?.RequireManagerApproval == true,
                RequiresHrVerification = entity.ApprovalDelegationPolicy?.RequireHrVerification == true,
                ScopeDefinitionJson = entity.ScopeDefinitionJson,
                SubmittedAt = entity.SubmittedAt,
                ApprovedAt = entity.ApprovedAt,
                ApprovedByUserId = entity.ApprovedByUserId,
                ApprovedByName = ResolveUserName(null, entity.ApprovedByUser),
                RevokedAt = entity.RevokedAt,
                RevokedByUserId = entity.RevokedByUserId,
                RevokedByName = ResolveUserName(null, entity.RevokedByUser),
                DecisionOrRevocationReason = entity.RevocationReason,
                ApprovalWorkflowInstanceId = relatedWorkflow?.Id,
                ApprovalWorkflowRequestNumber = relatedWorkflow?.RequestNumber,
                ApprovalWorkflowStatus = relatedWorkflow?.WorkflowStatus,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy,
                UpdateByName = await GetUserDisplayNameAsync(entity.UpdateBy, cancellationToken),
                AvailableActions = BuildAvailableActions(entity, actor, relatedWorkflow?.WorkflowStatus)
            };
        }

        private async Task<RequestValidationResult> ValidateRequestAsync(
            Guid? excludeId,
            HumanResourceUserContextDto actor,
            CreateApprovalDelegationRequest request,
            bool validateNoticePeriod,
            CancellationToken cancellationToken)
        {
            if (request.DelegateUserId == Guid.Empty)
            {
                return RequestValidationResult.Fail("Penerima delegasi wajib dipilih.");
            }

            if (request.DelegateUserId == actor.UserId)
            {
                return RequestValidationResult.Fail("Pemberi delegasi dan penerima delegasi tidak boleh sama.");
            }

            var effectiveStartAt = NormalizeUtc(request.EffectiveStartAt);
            var effectiveEndAt = NormalizeUtc(request.EffectiveEndAt);
            if (effectiveEndAt <= effectiveStartAt)
            {
                return RequestValidationResult.Fail("Waktu selesai delegasi harus lebih besar dari waktu mulai delegasi.");
            }

            if (!request.AppliesToAllWorkflows &&
                (!request.WorkflowDefinitionId.HasValue || request.WorkflowDefinitionId.Value == Guid.Empty) &&
                string.IsNullOrWhiteSpace(request.ScopeDefinitionJson))
            {
                return RequestValidationResult.Fail(
                    "Pilih berlaku untuk seluruh workflow, pilih workflow definition, atau isi scope definition JSON.");
            }

            if (request.WorkflowStepId.HasValue && request.WorkflowStepId.Value != Guid.Empty &&
                (!request.WorkflowDefinitionId.HasValue || request.WorkflowDefinitionId.Value == Guid.Empty))
            {
                return RequestValidationResult.Fail(
                    "Workflow definition wajib dipilih ketika workflow step diisi.");
            }

            if (!string.IsNullOrWhiteSpace(request.ScopeDefinitionJson) &&
                !IsValidJson(request.ScopeDefinitionJson))
            {
                return RequestValidationResult.Fail(
                    "Scope definition JSON tidak valid.");
            }

            var delegateUser = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == request.DelegateUserId && x.IsActive,
                    cancellationToken);

            if (delegateUser == null)
            {
                return RequestValidationResult.Fail(
                    "Penerima delegasi tidak ditemukan atau tidak aktif.");
            }

            MstApprovalDelegationPolicy? policy = null;
            var policyId = NormalizeGuid(request.ApprovalDelegationPolicyId);
            if (policyId.HasValue)
            {
                policy = await _dbContext.Set<MstApprovalDelegationPolicy>()
                    .AsNoTracking()
                    .Include(x => x.WorkflowDefinition)
                    .Include(x => x.WorkflowStep)
                    .FirstOrDefaultAsync(
                        x => x.Id == policyId.Value && x.IsActive && !x.IsDelete && !x.IsCancel,
                        cancellationToken);

                if (policy == null)
                {
                    return RequestValidationResult.Fail(
                        "Approval delegation policy tidak ditemukan atau tidak aktif.");
                }

                var policyValidation = await ValidatePolicyAsync(
                    policy,
                    actor,
                    delegateUser.WorkforceProfileId,
                    request,
                    validateNoticePeriod,
                    cancellationToken);

                if (policyValidation != null)
                {
                    return RequestValidationResult.Fail(policyValidation);
                }
            }

            var workflowDefinitionId = NormalizeGuid(request.WorkflowDefinitionId);
            MstWorkflowDefinition? workflowDefinition = null;
            if (workflowDefinitionId.HasValue)
            {
                workflowDefinition = await _dbContext.Set<MstWorkflowDefinition>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x => x.Id == workflowDefinitionId.Value &&
                             x.IsActive &&
                             !x.IsDelete &&
                             !x.IsCancel,
                        cancellationToken);

                if (workflowDefinition == null)
                {
                    return RequestValidationResult.Fail(
                        "Workflow definition tidak ditemukan atau tidak aktif.");
                }
            }

            var workflowStepId = NormalizeGuid(request.WorkflowStepId);
            if (workflowStepId.HasValue)
            {
                var workflowStepExists = await _dbContext.Set<MstWorkflowStep>()
                    .AsNoTracking()
                    .AnyAsync(
                        x => x.Id == workflowStepId.Value &&
                             x.WorkflowDefinitionId == workflowDefinitionId &&
                             x.IsActive &&
                             !x.IsDelete &&
                             !x.IsCancel,
                        cancellationToken);

                if (!workflowStepExists)
                {
                    return RequestValidationResult.Fail(
                        "Workflow step tidak ditemukan, tidak aktif, atau bukan bagian dari workflow definition yang dipilih.");
                }
            }

            if (!string.IsNullOrWhiteSpace(policy?.ApprovalWorkflowCode))
            {
                var nowDate = DateTime.UtcNow.Date;
                var approvalWorkflowExists = await _dbContext.Set<MstWorkflowDefinition>()
                    .AsNoTracking()
                    .AnyAsync(
                        x => x.WorkflowCode == policy.ApprovalWorkflowCode &&
                             x.IsActive &&
                             x.WorkflowStatus == "Active" &&
                             !x.IsDelete &&
                             !x.IsCancel &&
                             (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value <= nowDate) &&
                             (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= nowDate),
                        cancellationToken);

                if (!approvalWorkflowExists)
                {
                    return RequestValidationResult.Fail(
                        $"Approval workflow dengan kode {policy.ApprovalWorkflowCode} tidak ditemukan atau tidak aktif.");
                }
            }

            return RequestValidationResult.Ok(delegateUser, policy);
        }

        private async Task<string?> ValidatePolicyAsync(
            MstApprovalDelegationPolicy policy,
            HumanResourceUserContextDto actor,
            Guid? delegateWorkforceProfileId,
            CreateApprovalDelegationRequest request,
            bool validateNoticePeriod,
            CancellationToken cancellationToken)
        {
            var startAt = NormalizeUtc(request.EffectiveStartAt);
            var endAt = NormalizeUtc(request.EffectiveEndAt);
            var durationDays = CalculateDurationDays(startAt, endAt);

            if (policy.MaximumDelegationDays > 0 && durationDays > policy.MaximumDelegationDays)
            {
                return $"Durasi delegasi melebihi batas maksimum policy, yaitu {policy.MaximumDelegationDays} hari.";
            }

            if (validateNoticePeriod && policy.MinimumNoticeHours > 0)
            {
                var minimumStartAt = DateTime.UtcNow.AddHours(policy.MinimumNoticeHours);
                if (startAt < minimumStartAt)
                {
                    return $"Delegasi harus diajukan minimal {policy.MinimumNoticeHours} jam sebelum waktu mulai.";
                }
            }

            if (policy.EffectiveStartDate.HasValue && startAt.Date < policy.EffectiveStartDate.Value.Date)
            {
                return "Waktu mulai delegasi berada sebelum periode efektif policy.";
            }

            if (policy.EffectiveEndDate.HasValue && endAt.Date > policy.EffectiveEndDate.Value.Date)
            {
                return "Waktu selesai delegasi berada setelah periode efektif policy.";
            }

            if (policy.WorkflowDefinitionId.HasValue &&
                NormalizeGuid(request.WorkflowDefinitionId) != policy.WorkflowDefinitionId)
            {
                return "Workflow definition harus mengikuti scope workflow pada delegation policy.";
            }

            if (policy.WorkflowStepId.HasValue &&
                NormalizeGuid(request.WorkflowStepId) != policy.WorkflowStepId)
            {
                return "Workflow step harus mengikuti scope workflow step pada delegation policy.";
            }

            if (request.AllowSubDelegation && !policy.AllowSubDelegation)
            {
                return "Delegation policy tidak mengizinkan sub-delegation.";
            }

            if (!actor.WorkforceProfileId.HasValue || !delegateWorkforceProfileId.HasValue)
            {
                if (!policy.AllowCrossLegalEntity ||
                    !policy.AllowCrossHospitalSite ||
                    !policy.AllowCrossOrganizationUnit)
                {
                    return "Data workforce profile pemberi atau penerima delegasi belum tersedia untuk validasi scope organisasi.";
                }

                return null;
            }

            var delegatorAssignment = await GetCurrentOrganizationAssignmentAsync(
                actor.WorkforceProfileId.Value,
                cancellationToken);
            var delegateAssignment = await GetCurrentOrganizationAssignmentAsync(
                delegateWorkforceProfileId.Value,
                cancellationToken);

            if (delegatorAssignment == null || delegateAssignment == null)
            {
                if (!policy.AllowCrossLegalEntity ||
                    !policy.AllowCrossHospitalSite ||
                    !policy.AllowCrossOrganizationUnit)
                {
                    return "Organization assignment aktif pemberi atau penerima delegasi belum tersedia.";
                }

                return null;
            }

            if (policy.LegalEntityId.HasValue &&
                delegatorAssignment.LegalEntityId != policy.LegalEntityId)
            {
                return "Pemberi delegasi berada di luar legal entity scope delegation policy.";
            }

            if (policy.HospitalSiteId.HasValue &&
                delegatorAssignment.HospitalSiteId != policy.HospitalSiteId)
            {
                return "Pemberi delegasi berada di luar hospital site scope delegation policy.";
            }

            if (policy.OrganizationUnitId.HasValue &&
                delegatorAssignment.OrganizationUnitId != policy.OrganizationUnitId)
            {
                return "Pemberi delegasi berada di luar organization unit scope delegation policy.";
            }

            if (!policy.AllowCrossLegalEntity &&
                delegatorAssignment.LegalEntityId != delegateAssignment.LegalEntityId)
            {
                return "Delegation policy tidak mengizinkan delegasi lintas legal entity.";
            }

            if (!policy.AllowCrossHospitalSite &&
                delegatorAssignment.HospitalSiteId != delegateAssignment.HospitalSiteId)
            {
                return "Delegation policy tidak mengizinkan delegasi lintas hospital site.";
            }

            if (!policy.AllowCrossOrganizationUnit &&
                delegatorAssignment.OrganizationUnitId != delegateAssignment.OrganizationUnitId)
            {
                return "Delegation policy tidak mengizinkan delegasi lintas organization unit.";
            }

            return null;
        }

        private async Task<string?> ValidateNoOverlapAsync(
            TrxApprovalDelegation entity,
            CancellationToken cancellationToken)
        {
            var candidates = await _dbContext.Set<TrxApprovalDelegation>()
                .AsNoTracking()
                .Where(x =>
                    x.Id != entity.Id &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.IsActive &&
                    x.DelegatorUserId == entity.DelegatorUserId &&
                    (x.DelegationStatus == WorkflowValueConstants.DelegationStatus.Submitted ||
                     x.DelegationStatus == WorkflowValueConstants.DelegationStatus.Approved ||
                     x.DelegationStatus == WorkflowValueConstants.DelegationStatus.Active) &&
                    x.EffectiveStartAt < entity.EffectiveEndAt &&
                    x.EffectiveEndAt > entity.EffectiveStartAt)
                .ToListAsync(cancellationToken);

            var proposed = ToScope(entity);
            var conflict = candidates.FirstOrDefault(x => ScopesOverlap(ToScope(x), proposed));
            if (conflict == null)
            {
                return null;
            }

            return $"Periode dan scope delegasi bertabrakan dengan delegation {conflict.DelegationNumber}.";
        }

        private async Task<string?> ValidateNoDelegationLoopAsync(
            TrxApprovalDelegation entity,
            CancellationToken cancellationToken)
        {
            var candidates = await _dbContext.Set<TrxApprovalDelegation>()
                .AsNoTracking()
                .Where(x =>
                    x.Id != entity.Id &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.IsActive &&
                    (x.DelegationStatus == WorkflowValueConstants.DelegationStatus.Submitted ||
                     x.DelegationStatus == WorkflowValueConstants.DelegationStatus.Approved ||
                     x.DelegationStatus == WorkflowValueConstants.DelegationStatus.Active) &&
                    x.EffectiveStartAt < entity.EffectiveEndAt &&
                    x.EffectiveEndAt > entity.EffectiveStartAt)
                .ToListAsync(cancellationToken);

            var proposedScope = ToScope(entity);
            var relevant = candidates
                .Where(x => ScopesOverlap(ToScope(x), proposedScope))
                .ToList();

            var graph = relevant
                .GroupBy(x => x.DelegatorUserId)
                .ToDictionary(
                    x => x.Key,
                    x => x.Select(item => item.DelegateUserId).Distinct().ToList());

            if (!graph.TryGetValue(entity.DelegatorUserId, out var proposedEdges))
            {
                proposedEdges = new List<Guid>();
                graph[entity.DelegatorUserId] = proposedEdges;
            }

            if (!proposedEdges.Contains(entity.DelegateUserId))
            {
                proposedEdges.Add(entity.DelegateUserId);
            }

            var queue = new Queue<Guid>();
            var visited = new HashSet<Guid>();
            queue.Enqueue(entity.DelegateUserId);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (!visited.Add(current))
                {
                    continue;
                }

                if (current == entity.DelegatorUserId)
                {
                    return "Delegasi membentuk delegation loop. Contoh loop yang dilarang: A ke B, B ke C, lalu C ke A.";
                }

                if (!graph.TryGetValue(current, out var nextUsers))
                {
                    continue;
                }

                foreach (var next in nextUsers)
                {
                    queue.Enqueue(next);
                }
            }

            return null;
        }

        private async Task<string?> ValidateSubDelegationAsync(
            TrxApprovalDelegation entity,
            CancellationToken cancellationToken)
        {
            var incoming = await _dbContext.Set<TrxApprovalDelegation>()
                .AsNoTracking()
                .Where(x =>
                    x.Id != entity.Id &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.IsActive &&
                    x.DelegateUserId == entity.DelegatorUserId &&
                    (x.DelegationStatus == WorkflowValueConstants.DelegationStatus.Active ||
                     x.DelegationStatus == WorkflowValueConstants.DelegationStatus.Approved) &&
                    x.EffectiveStartAt < entity.EffectiveEndAt &&
                    x.EffectiveEndAt > entity.EffectiveStartAt)
                .ToListAsync(cancellationToken);

            var proposedScope = ToScope(entity);
            var blocked = incoming.FirstOrDefault(x =>
                ScopesOverlap(ToScope(x), proposedScope) && !x.AllowSubDelegation);

            if (blocked == null)
            {
                return null;
            }

            return $"Delegasi tidak dapat diteruskan karena delegation {blocked.DelegationNumber} tidak mengizinkan sub-delegation.";
        }

        private async Task<WfpOrganizationAssignment?> GetCurrentOrganizationAssignmentAsync(
            Guid workforceProfileId,
            CancellationToken cancellationToken)
        {
            var today = DateTime.UtcNow.Date;
            return await _dbContext.Set<WfpOrganizationAssignment>()
                .AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.IsActive &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.EffectiveStartDate.Date <= today &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= today))
                .OrderByDescending(x => x.IsPrimary)
                .ThenByDescending(x => x.EffectiveStartDate)
                .FirstOrDefaultAsync(cancellationToken);
        }

        private async Task ApplyDelegationToOpenAssignmentsAsync(
            Guid delegationId,
            CancellationToken cancellationToken)
        {
            var delegation = await _dbContext.Set<TrxApprovalDelegation>()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == delegationId &&
                         x.IsActive &&
                         !x.IsDelete &&
                         !x.IsCancel &&
                         x.DelegationStatus == WorkflowValueConstants.DelegationStatus.Active,
                    cancellationToken);

            if (delegation == null)
            {
                return;
            }

            var assignments = await _dbContext.Set<TrxWorkflowApproverAssignment>()
                .Include(x => x.WorkflowInstance)
                .Include(x => x.WorkflowStepInstance)
                .Where(x =>
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.IsActive &&
                    !x.IsDelegated &&
                    x.AssignedApproverUserId == delegation.DelegatorUserId &&
                    (x.AssignmentStatus == WorkflowValueConstants.AssignmentStatus.Pending ||
                     x.AssignmentStatus == WorkflowValueConstants.AssignmentStatus.Available ||
                     x.AssignmentStatus == WorkflowValueConstants.AssignmentStatus.InProgress))
                .ToListAsync(cancellationToken);

            var now = DateTime.UtcNow;
            var changed = false;

            foreach (var assignment in assignments)
            {
                if (assignment.WorkflowInstance == null || assignment.WorkflowStepInstance == null)
                {
                    continue;
                }

                if (!DelegationMatchesWorkflow(
                        delegation,
                        assignment.WorkflowInstance.WorkflowDefinitionId,
                        assignment.WorkflowStepInstance.WorkflowStepId))
                {
                    continue;
                }

                assignment.OriginalApproverUserId = assignment.AssignedApproverUserId;
                assignment.OriginalApproverWorkforceProfileId =
                    assignment.AssignedApproverWorkforceProfileId;
                assignment.AssignedApproverUserId = delegation.DelegateUserId;
                assignment.AssignedApproverWorkforceProfileId =
                    delegation.DelegateWorkforceProfileId;
                assignment.ApprovalDelegationId = delegation.Id;
                assignment.DelegatedAt = now;
                assignment.IsDelegated = true;
                assignment.UpdateDateTime = now;
                assignment.UpdateBy = delegation.DelegatorUserId;
                changed = true;
            }

            if (changed)
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        private async Task RestoreOpenAssignmentsAsync(
            Guid delegationId,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var assignments = await _dbContext.Set<TrxWorkflowApproverAssignment>()
                .Where(x =>
                    x.ApprovalDelegationId == delegationId &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.IsActive &&
                    x.IsDelegated &&
                    (x.AssignmentStatus == WorkflowValueConstants.AssignmentStatus.Pending ||
                     x.AssignmentStatus == WorkflowValueConstants.AssignmentStatus.Available ||
                     x.AssignmentStatus == WorkflowValueConstants.AssignmentStatus.InProgress))
                .ToListAsync(cancellationToken);

            var now = DateTime.UtcNow;
            foreach (var assignment in assignments)
            {
                if (assignment.OriginalApproverUserId.HasValue)
                {
                    assignment.AssignedApproverUserId = assignment.OriginalApproverUserId.Value;
                    assignment.AssignedApproverWorkforceProfileId =
                        assignment.OriginalApproverWorkforceProfileId;
                }

                assignment.OriginalApproverUserId = null;
                assignment.OriginalApproverWorkforceProfileId = null;
                assignment.ApprovalDelegationId = null;
                assignment.DelegatedAt = null;
                assignment.IsDelegated = false;
                assignment.UpdateDateTime = now;
                assignment.UpdateBy = actorUserId;
            }

            if (assignments.Count > 0)
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        private async Task SynchronizeStatusesAsync(
            CancellationToken cancellationToken)
        {
            await SynchronizeGenericWorkflowDecisionsAsync(cancellationToken);

            var now = DateTime.UtcNow;
            var candidates = await _dbContext.Set<TrxApprovalDelegation>()
                .Where(x =>
                    !x.IsDelete &&
                    !x.IsCancel &&
                    (x.DelegationStatus == WorkflowValueConstants.DelegationStatus.Approved ||
                     x.DelegationStatus == WorkflowValueConstants.DelegationStatus.Active ||
                     x.DelegationStatus == WorkflowValueConstants.DelegationStatus.Submitted))
                .ToListAsync(cancellationToken);

            var activatedIds = new List<Guid>();
            var expired = new List<(Guid Id, Guid ActorUserId)>();
            var changed = false;

            foreach (var entity in candidates)
            {
                if (entity.EffectiveEndAt < now)
                {
                    entity.DelegationStatus = WorkflowValueConstants.DelegationStatus.Expired;
                    entity.IsActive = false;
                    entity.UpdateDateTime = now;
                    entity.UpdateBy = entity.DelegatorUserId;
                    expired.Add((entity.Id, entity.DelegatorUserId));
                    changed = true;
                    continue;
                }

                if (entity.DelegationStatus == WorkflowValueConstants.DelegationStatus.Approved &&
                    entity.EffectiveStartAt <= now)
                {
                    entity.DelegationStatus = WorkflowValueConstants.DelegationStatus.Active;
                    entity.IsActive = true;
                    entity.UpdateDateTime = now;
                    entity.UpdateBy = entity.ApprovedByUserId ?? entity.DelegatorUserId;
                    activatedIds.Add(entity.Id);
                    changed = true;
                }
            }

            if (changed)
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            foreach (var id in activatedIds)
            {
                await ApplyDelegationToOpenAssignmentsAsync(id, cancellationToken);
            }

            foreach (var item in expired)
            {
                await RestoreOpenAssignmentsAsync(item.Id, item.ActorUserId, cancellationToken);
            }
        }

        private async Task SynchronizeGenericWorkflowDecisionsAsync(
            CancellationToken cancellationToken)
        {
            var submitted = await _dbContext.Set<TrxApprovalDelegation>()
                .Include(x => x.ApprovalDelegationPolicy)
                .Where(x =>
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.IsActive &&
                    x.DelegationStatus == WorkflowValueConstants.DelegationStatus.Submitted &&
                    x.ApprovalDelegationPolicy != null &&
                    x.ApprovalDelegationPolicy.ApprovalWorkflowCode != null)
                .ToListAsync(cancellationToken);

            if (submitted.Count == 0)
            {
                return;
            }

            var ids = submitted.Select(x => x.Id).ToList();
            var workflowInstances = await _dbContext.Set<TrxWorkflowInstance>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.ReferenceType == ApprovalReferenceType &&
                    ids.Contains(x.ReferenceId))
                .OrderByDescending(x => x.CreateDateTime)
                .ToListAsync(cancellationToken);

            var latestByReference = workflowInstances
                .GroupBy(x => x.ReferenceId)
                .ToDictionary(x => x.Key, x => x.First());

            var now = DateTime.UtcNow;
            var activated = new List<Guid>();
            var changed = false;

            foreach (var entity in submitted)
            {
                if (!latestByReference.TryGetValue(entity.Id, out var workflow))
                {
                    continue;
                }

                if (workflow.WorkflowStatus == WorkflowValueConstants.WorkflowStatus.Completed ||
                    workflow.WorkflowStatus == WorkflowValueConstants.WorkflowStatus.Approved)
                {
                    entity.ApprovedAt = workflow.CompletedAt ?? workflow.LastActionAt ?? now;
                    entity.ApprovedByUserId = null;
                    entity.DelegationStatus = entity.EffectiveStartAt <= now && entity.EffectiveEndAt >= now
                        ? WorkflowValueConstants.DelegationStatus.Active
                        : WorkflowValueConstants.DelegationStatus.Approved;
                    entity.IsActive = entity.EffectiveEndAt >= now;
                    entity.UpdateDateTime = now;
                    entity.UpdateBy = entity.DelegatorUserId;
                    if (entity.DelegationStatus == WorkflowValueConstants.DelegationStatus.Active)
                    {
                        activated.Add(entity.Id);
                    }
                    changed = true;
                }
                else if (workflow.WorkflowStatus == WorkflowValueConstants.WorkflowStatus.Rejected)
                {
                    entity.DelegationStatus = WorkflowValueConstants.DelegationStatus.Rejected;
                    entity.IsActive = false;
                    entity.RevocationReason = workflow.CompletionNote ?? "Ditolak melalui Workflow Engine.";
                    entity.UpdateDateTime = now;
                    entity.UpdateBy = entity.DelegatorUserId;
                    changed = true;
                }
                else if (workflow.WorkflowStatus == WorkflowValueConstants.WorkflowStatus.Cancelled ||
                         workflow.WorkflowStatus == WorkflowValueConstants.WorkflowStatus.Withdrawn)
                {
                    entity.DelegationStatus = WorkflowValueConstants.DelegationStatus.Cancelled;
                    entity.IsActive = false;
                    entity.IsCancel = true;
                    entity.CancelDateTime = workflow.CancelledAt ?? workflow.WithdrawnAt ?? now;
                    entity.CancelBy = entity.DelegatorUserId;
                    entity.UpdateDateTime = now;
                    entity.UpdateBy = entity.DelegatorUserId;
                    changed = true;
                }
            }

            if (changed)
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            foreach (var id in activated)
            {
                await ApplyDelegationToOpenAssignmentsAsync(id, cancellationToken);
            }
        }

        private async Task<WorkflowServiceResult<object>> StartGenericApprovalWorkflowAsync(
            Guid delegationId,
            string approvalWorkflowCode,
            string? comment,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<TrxApprovalDelegation>()
                .AsNoTracking()
                .FirstAsync(x => x.Id == delegationId, cancellationToken);

            var existing = await _dbContext.Set<TrxWorkflowInstance>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.ReferenceType == ApprovalReferenceType &&
                    x.ReferenceId == delegationId)
                .OrderByDescending(x => x.CreateDateTime)
                .FirstOrDefaultAsync(cancellationToken);

            Guid workflowInstanceId;
            if (existing == null)
            {
                var createResult = await _workflowService.CreateAsync(
                    new CreateWorkflowInstanceRequest
                    {
                        WorkflowDefinitionCode = approvalWorkflowCode,
                        ReferenceType = ApprovalReferenceType,
                        ReferenceId = delegationId,
                        ExternalReferenceNumber = entity.DelegationNumber,
                        SourceChannel = WorkflowValueConstants.SourceChannel.Web,
                        RequestCorrelationId = $"approval-delegation-{delegationId:N}",
                        IdempotencyKey = $"approval-delegation-create-{delegationId:N}",
                        RequestContext = JsonSerializer.SerializeToElement(new
                        {
                            entity.DelegationNumber,
                            entity.DelegatorUserId,
                            entity.DelegateUserId,
                            entity.EffectiveStartAt,
                            entity.EffectiveEndAt,
                            entity.AppliesToAllWorkflows,
                            entity.WorkflowDefinitionId,
                            entity.WorkflowStepId,
                            entity.ScopeDefinitionJson
                        })
                    },
                    cancellationToken);

                if (!createResult.Success || createResult.Data == null)
                {
                    return WorkflowServiceResult<object>.Fail(
                        createResult.StatusCode,
                        $"Workflow approval delegation gagal dibuat. {createResult.Message}");
                }

                workflowInstanceId = createResult.Data.Id;
            }
            else
            {
                workflowInstanceId = existing.Id;
            }

            var submitResult = await _workflowService.SubmitAsync(
                workflowInstanceId,
                new WorkflowSubmitRequest
                {
                    Comment = string.IsNullOrWhiteSpace(comment)
                        ? "Pengajuan approval delegation."
                        : comment.Trim(),
                    IdempotencyKey = $"approval-delegation-submit-{delegationId:N}"
                },
                cancellationToken);

            if (!submitResult.Success)
            {
                return WorkflowServiceResult<object>.Fail(
                    submitResult.StatusCode,
                    $"Workflow approval delegation gagal diajukan. {submitResult.Message}");
            }

            return WorkflowServiceResult<object>.Ok(
                new { WorkflowInstanceId = workflowInstanceId },
                "Workflow approval delegation berhasil dibuat dan diajukan.");
        }

        private async Task CancelRelatedGenericWorkflowAsync(
            Guid delegationId,
            string reason,
            CancellationToken cancellationToken)
        {
            var workflow = await _dbContext.Set<TrxWorkflowInstance>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.ReferenceType == ApprovalReferenceType &&
                    x.ReferenceId == delegationId)
                .OrderByDescending(x => x.CreateDateTime)
                .FirstOrDefaultAsync(cancellationToken);

            if (workflow == null ||
                workflow.WorkflowStatus == WorkflowValueConstants.WorkflowStatus.Completed ||
                workflow.WorkflowStatus == WorkflowValueConstants.WorkflowStatus.Rejected ||
                workflow.WorkflowStatus == WorkflowValueConstants.WorkflowStatus.Cancelled ||
                workflow.WorkflowStatus == WorkflowValueConstants.WorkflowStatus.Withdrawn)
            {
                return;
            }

            if (workflow.WorkflowStatus == WorkflowValueConstants.WorkflowStatus.InProgress)
            {
                await _workflowService.WithdrawAsync(
                    workflow.Id,
                    new WorkflowWithdrawRequest
                    {
                        Reason = reason,
                        IdempotencyKey = $"approval-delegation-withdraw-{delegationId:N}"
                    },
                    cancellationToken);
            }
            else
            {
                await _workflowService.CancelAsync(
                    workflow.Id,
                    new WorkflowCancelRequest
                    {
                        Reason = reason,
                        IdempotencyKey = $"approval-delegation-cancel-{delegationId:N}"
                    },
                    cancellationToken);
            }
        }

        private async Task RevertSubmittedToDraftAsync(
            Guid id,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<TrxApprovalDelegation>()
                .FirstOrDefaultAsync(
                    x => x.Id == id &&
                         !x.IsDelete &&
                         x.DelegationStatus == WorkflowValueConstants.DelegationStatus.Submitted,
                    cancellationToken);

            if (entity == null)
            {
                return;
            }

            entity.DelegationStatus = WorkflowValueConstants.DelegationStatus.Draft;
            entity.SubmittedAt = null;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        private async Task<RelatedWorkflowInfo?> GetRelatedWorkflowAsync(
            Guid delegationId,
            CancellationToken cancellationToken)
        {
            return await _dbContext.Set<TrxWorkflowInstance>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.ReferenceType == ApprovalReferenceType &&
                    x.ReferenceId == delegationId)
                .OrderByDescending(x => x.CreateDateTime)
                .Select(x => new RelatedWorkflowInfo
                {
                    Id = x.Id,
                    RequestNumber = x.RequestNumber,
                    WorkflowStatus = x.WorkflowStatus
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        private static List<string> BuildAvailableActions(
            TrxApprovalDelegation entity,
            HumanResourceUserContextDto actor,
            string? relatedWorkflowStatus)
        {
            var actions = new List<string> { "Read" };
            var isOwner = entity.DelegatorUserId == actor.UserId;
            var usesGenericWorkflow = !string.IsNullOrWhiteSpace(
                entity.ApprovalDelegationPolicy?.ApprovalWorkflowCode);

            if (isOwner && entity.DelegationStatus == WorkflowValueConstants.DelegationStatus.Draft)
            {
                actions.Add("Update");
                actions.Add("Submit");
                actions.Add("Cancel");
                actions.Add("Delete");
            }

            if (entity.DelegationStatus == WorkflowValueConstants.DelegationStatus.Submitted &&
                !usesGenericWorkflow &&
                actor.UserId != entity.DelegatorUserId &&
                actor.UserId != entity.DelegateUserId)
            {
                actions.Add("Approve");
                actions.Add("Reject");
            }

            if (isOwner && entity.DelegationStatus == WorkflowValueConstants.DelegationStatus.Submitted)
            {
                actions.Add("Cancel");
            }

            if (entity.DelegationStatus == WorkflowValueConstants.DelegationStatus.Approved)
            {
                actions.Add("Activate");
                if (isOwner && entity.EffectiveStartAt > DateTime.UtcNow)
                {
                    actions.Add("Cancel");
                }
            }

            if (entity.DelegationStatus == WorkflowValueConstants.DelegationStatus.Active)
            {
                actions.Add("Revoke");
            }

            if (isOwner &&
                (entity.DelegationStatus == WorkflowValueConstants.DelegationStatus.Rejected ||
                 entity.DelegationStatus == WorkflowValueConstants.DelegationStatus.Cancelled ||
                 entity.DelegationStatus == WorkflowValueConstants.DelegationStatus.Expired))
            {
                actions.Add("Delete");
            }

            if (usesGenericWorkflow && !string.IsNullOrWhiteSpace(relatedWorkflowStatus))
            {
                actions.Add("OpenApprovalWorkflow");
            }

            return actions.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        private static bool DelegationMatchesWorkflow(
            TrxApprovalDelegation delegation,
            Guid workflowDefinitionId,
            Guid workflowStepId)
        {
            if (delegation.AppliesToAllWorkflows)
            {
                return true;
            }

            if (delegation.WorkflowDefinitionId.HasValue &&
                delegation.WorkflowDefinitionId.Value != workflowDefinitionId)
            {
                return false;
            }

            if (delegation.WorkflowStepId.HasValue &&
                delegation.WorkflowStepId.Value != workflowStepId)
            {
                return false;
            }

            return delegation.WorkflowDefinitionId.HasValue ||
                   !string.IsNullOrWhiteSpace(delegation.ScopeDefinitionJson);
        }

        private async Task<string?> GetUserDisplayNameAsync(
            Guid userId,
            CancellationToken cancellationToken)
        {
            if (userId == Guid.Empty)
            {
                return null;
            }

            return await _dbContext.Users
                .AsNoTracking()
                .Where(x => x.Id == userId)
                .Select(x => x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode)
                .FirstOrDefaultAsync(cancellationToken);
        }

        private async Task<string> GenerateDelegationNumberAsync(
            CancellationToken cancellationToken)
        {
            var period = DateTime.UtcNow.ToString("yyyyMM");
            var prefix = $"{DelegationCodePrefix}{period}-";
            var codes = await _dbContext.Set<TrxApprovalDelegation>()
                .AsNoTracking()
                .Where(x => x.DelegationNumber.StartsWith(prefix))
                .Select(x => x.DelegationNumber)
                .ToListAsync(cancellationToken);

            var maximum = codes
                .Select(x => x[prefix.Length..])
                .Where(x => int.TryParse(x, out _))
                .Select(int.Parse)
                .DefaultIfEmpty(0)
                .Max();

            return $"{prefix}{maximum + 1:00000}";
        }

        private async Task<WorkflowServiceResult<HumanResourceUserContextDto>> GetActorContextAsync(
            CancellationToken cancellationToken)
        {
            try
            {
                var actor = await _humanResourceContextService.GetCurrentAsync(cancellationToken);
                return WorkflowServiceResult<HumanResourceUserContextDto>.Ok(
                    actor,
                    "Konteks user berhasil diambil.");
            }
            catch (UnauthorizedAccessException ex)
            {
                return WorkflowServiceResult<HumanResourceUserContextDto>.Fail(
                    StatusCodes.Status401Unauthorized,
                    ex.Message);
            }
        }

        private static CreateApprovalDelegationRequest ToRequest(
            TrxApprovalDelegation entity)
        {
            return new CreateApprovalDelegationRequest
            {
                DelegateUserId = entity.DelegateUserId,
                ApprovalDelegationPolicyId = entity.ApprovalDelegationPolicyId,
                WorkflowDefinitionId = entity.WorkflowDefinitionId,
                WorkflowStepId = entity.WorkflowStepId,
                EffectiveStartAt = entity.EffectiveStartAt,
                EffectiveEndAt = entity.EffectiveEndAt,
                DelegationReason = entity.DelegationReason,
                AppliesToAllWorkflows = entity.AppliesToAllWorkflows,
                AllowSubDelegation = entity.AllowSubDelegation,
                PreserveDelegatorAccountability = entity.PreserveDelegatorAccountability,
                ScopeDefinitionJson = entity.ScopeDefinitionJson
            };
        }

        private static DelegationScopeDescriptor ToScope(
            TrxApprovalDelegation entity)
        {
            return new DelegationScopeDescriptor
            {
                AppliesToAllWorkflows = entity.AppliesToAllWorkflows,
                WorkflowDefinitionId = entity.WorkflowDefinitionId,
                WorkflowStepId = entity.WorkflowStepId,
                ScopeDefinitionJson = NormalizeJson(entity.ScopeDefinitionJson)
            };
        }

        private static bool ScopesOverlap(
            DelegationScopeDescriptor left,
            DelegationScopeDescriptor right)
        {
            if (left.AppliesToAllWorkflows || right.AppliesToAllWorkflows)
            {
                return true;
            }

            if (left.WorkflowDefinitionId.HasValue && right.WorkflowDefinitionId.HasValue)
            {
                if (left.WorkflowDefinitionId.Value != right.WorkflowDefinitionId.Value)
                {
                    return false;
                }

                return !left.WorkflowStepId.HasValue ||
                       !right.WorkflowStepId.HasValue ||
                       left.WorkflowStepId.Value == right.WorkflowStepId.Value;
            }

            if (!left.WorkflowDefinitionId.HasValue && !right.WorkflowDefinitionId.HasValue)
            {
                if (string.IsNullOrWhiteSpace(left.ScopeDefinitionJson) ||
                    string.IsNullOrWhiteSpace(right.ScopeDefinitionJson))
                {
                    return true;
                }

                return string.Equals(
                    left.ScopeDefinitionJson,
                    right.ScopeDefinitionJson,
                    StringComparison.Ordinal);
            }

            // A JSON scope may still cover a definition-based scope. Until a dedicated
            // condition evaluator is available, use the conservative overlap rule.
            return true;
        }

        private static string? ResolveUserName(
            string? workforceDisplayName,
            ApplicationUser? user)
        {
            if (!string.IsNullOrWhiteSpace(workforceDisplayName))
            {
                return workforceDisplayName;
            }

            return user?.DisplayName ??
                   user?.UserName ??
                   user?.Email ??
                   user?.UserCode;
        }

        private static int CalculateDurationDays(
            DateTime startAt,
            DateTime endAt)
        {
            return Math.Max(1, (int)Math.Ceiling((endAt - startAt).TotalDays));
        }

        private static DateTime NormalizeUtc(DateTime value)
        {
            return value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
            };
        }

        private static Guid? NormalizeGuid(Guid? value)
        {
            return !value.HasValue || value.Value == Guid.Empty
                ? null
                : value.Value;
        }

        private static string? NormalizeNullableString(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static string? NormalizeJson(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            using var document = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(document.RootElement);
        }

        private static bool IsValidJson(string json)
        {
            try
            {
                using var document = JsonDocument.Parse(json);
                return document.RootElement.ValueKind == JsonValueKind.Object ||
                       document.RootElement.ValueKind == JsonValueKind.Array;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        private static (int PageNumber, int PageSize) NormalizePaging(
            int pageNumber,
            int pageSize)
        {
            return (
                pageNumber < 1 ? 1 : pageNumber,
                pageSize < 1 ? 25 : Math.Min(pageSize, 100));
        }

        private static DateRangeResult ResolveDateRange(
            DateTime? startDate,
            DateTime? endDate,
            string? period)
        {
            if (startDate.HasValue && endDate.HasValue && startDate.Value.Date > endDate.Value.Date)
            {
                return DateRangeResult.Fail(
                    "Tanggal mulai tidak boleh lebih besar daripada tanggal selesai.");
            }

            if (startDate.HasValue || endDate.HasValue)
            {
                return DateRangeResult.Ok(
                    startDate.HasValue
                        ? DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc)
                        : null,
                    endDate.HasValue
                        ? DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc)
                        : null);
            }

            var today = DateTime.UtcNow.Date;
            return period?.Trim().ToLowerInvariant() switch
            {
                "today" => DateRangeResult.Ok(today, today.AddDays(1)),
                "last7days" => DateRangeResult.Ok(today.AddDays(-6), today.AddDays(1)),
                "last30days" => DateRangeResult.Ok(today.AddDays(-29), today.AddDays(1)),
                "thismonth" => DateRangeResult.Ok(
                    new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc),
                    new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1)),
                "lastmonth" => DateRangeResult.Ok(
                    new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-1),
                    new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc)),
                null or "" or "custom" => DateRangeResult.Ok(null, null),
                _ => DateRangeResult.Fail("Period filter tidak dikenali.")
            };
        }

        private static List<ApprovalDelegationStringOptionResponse> BuildOptions(
            IEnumerable<(string Value, string Label)> items)
        {
            return items
                .Select(x => new ApprovalDelegationStringOptionResponse
                {
                    Value = x.Value,
                    Label = x.Label
                })
                .ToList();
        }

        private sealed class RequestValidationResult
        {
            public bool IsValid { get; private set; }
            public string? ErrorMessage { get; private set; }
            public ApplicationUser? DelegateUser { get; private set; }
            public MstApprovalDelegationPolicy? Policy { get; private set; }

            public static RequestValidationResult Ok(
                ApplicationUser delegateUser,
                MstApprovalDelegationPolicy? policy)
            {
                return new RequestValidationResult
                {
                    IsValid = true,
                    DelegateUser = delegateUser,
                    Policy = policy
                };
            }

            public static RequestValidationResult Fail(string message)
            {
                return new RequestValidationResult
                {
                    IsValid = false,
                    ErrorMessage = message
                };
            }
        }

        private sealed class DelegationScopeDescriptor
        {
            public bool AppliesToAllWorkflows { get; set; }
            public Guid? WorkflowDefinitionId { get; set; }
            public Guid? WorkflowStepId { get; set; }
            public string? ScopeDefinitionJson { get; set; }
        }

        private sealed class DateRangeResult
        {
            public bool IsValid { get; private set; }
            public string? ErrorMessage { get; private set; }
            public DateTime? Start { get; private set; }
            public DateTime? EndExclusive { get; private set; }

            public static DateRangeResult Ok(
                DateTime? start,
                DateTime? endExclusive)
            {
                return new DateRangeResult
                {
                    IsValid = true,
                    Start = start,
                    EndExclusive = endExclusive
                };
            }

            public static DateRangeResult Fail(string message)
            {
                return new DateRangeResult
                {
                    IsValid = false,
                    ErrorMessage = message
                };
            }
        }

        private sealed class RelatedWorkflowInfo
        {
            public Guid Id { get; set; }
            public string RequestNumber { get; set; } = string.Empty;
            public string WorkflowStatus { get; set; } = string.Empty;
        }
    }
}
