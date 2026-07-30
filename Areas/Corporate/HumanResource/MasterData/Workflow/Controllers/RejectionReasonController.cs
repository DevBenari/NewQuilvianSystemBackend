using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

using ResponseRejectionReasonPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.DTOs.RejectionReasonResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/rejection-reasons")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Rejection Reason",
        AreaName = "Corporate",
        ControllerName = "RejectionReason",
        Description = "Corporate human resource master data rejection reason",
        SortOrder = 75)]
    [Tags("Corporate / Human Resource / Master Data / Rejection Reason")]
    public class RejectionReasonController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.MasterData";

        private static readonly HashSet<string> RejectActions = new(StringComparer.OrdinalIgnoreCase)
        {
            "ReturnToRequester",
            "ReturnToPreviousStep",
            "ReturnToSpecificStep",
            "CancelRequest",
            "CloseRequest"
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public RejectionReasonController(ApplicationDbContext dbContext, LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<RejectionReasonFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Rejection Reason", Description = "Melihat metadata filter rejection reason", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RejectionReason", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new RejectionReasonFilterMetadataResponse
            {
                DefaultFilter = new RejectionReasonDefaultFilterResponse(),
                CustomPeriods = WorkflowMasterDataSupport.BuildPeriodOptions(),
                SortOptions = new List<WorkflowMasterSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "reasonCode", Label = "Kode alasan" },
                    new() { Value = "reasonName", Label = "Nama alasan" },
                    new() { Value = "requestType", Label = "Tipe permintaan" },
                    new() { Value = "reasonCategory", Label = "Kategori alasan" },
                    new() { Value = "rejectAction", Label = "Aksi penolakan" },
                    new() { Value = "sortOrder", Label = "Urutan" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                RejectActions = RejectActions
                    .OrderBy(x => x)
                    .Select(x => new WorkflowMasterLookupOptionResponse { Value = x, Label = x })
                    .ToList()
            };

            await _loggerService.InfoAsync(LogCategory, "RejectionReason.GetFilterMetadata", "Mengambil metadata filter rejection reason.", result);
            return Ok(ApiResponse<RejectionReasonFilterMetadataResponse>.Ok(result, "Metadata filter rejection reason berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<RejectionReasonSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Rejection Reason", Description = "Melihat ringkasan rejection reason", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RejectionReason", "Read")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] Guid? workflowDefinitionId,
            [FromQuery] Guid? workflowStepId,
            [FromQuery] string? requestType)
        {
            var query = BuildBaseQuery();
            if (workflowDefinitionId.HasValue && workflowDefinitionId.Value != Guid.Empty) query = query.Where(x => x.WorkflowDefinitionId == workflowDefinitionId.Value);
            if (workflowStepId.HasValue && workflowStepId.Value != Guid.Empty) query = query.Where(x => x.WorkflowStepId == workflowStepId.Value);
            if (!string.IsNullOrWhiteSpace(requestType)) query = query.Where(x => x.RequestType == requestType.Trim());

            var result = new RejectionReasonSummaryResponse
            {
                TotalRejectionReason = await query.CountAsync(),
                ActiveRejectionReason = await query.CountAsync(x => x.IsActive),
                InactiveRejectionReason = await query.CountAsync(x => !x.IsActive),
                CommentRequiredReason = await query.CountAsync(x => x.IsCommentRequired),
                AttachmentRequiredReason = await query.CountAsync(x => x.IsAttachmentRequired),
                ResubmittableReason = await query.CountAsync(x => x.AllowResubmit),
                GlobalReason = await query.CountAsync(x => x.WorkflowDefinitionId == null && x.WorkflowStepId == null),
                WorkflowSpecificReason = await query.CountAsync(x => x.WorkflowDefinitionId != null && x.WorkflowStepId == null),
                StepSpecificReason = await query.CountAsync(x => x.WorkflowStepId != null)
            };

            return Ok(ApiResponse<RejectionReasonSummaryResponse>.Ok(result, "Ringkasan rejection reason berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseRejectionReasonPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Rejection Reason", Description = "Melihat data rejection reason", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RejectionReason", "Read")]
        public async Task<IActionResult> GetRejectionReasons(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? workflowDefinitionId,
            [FromQuery] Guid? workflowStepId,
            [FromQuery] string? requestType,
            [FromQuery] string? reasonCategory,
            [FromQuery] string? rejectAction,
            [FromQuery] bool? allowResubmit,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "sortOrder",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = WorkflowMasterDataSupport.NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = WorkflowMasterDataSupport.ApplyDateFilter(BuildBaseQuery(), startDate, endDate, customPeriod);
            query = ApplyStandardFilter(query, workflowDefinitionId, workflowStepId, requestType, reasonCategory, rejectAction, allowResubmit, isActive, search);
            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new RejectionReasonResponse
                {
                    Id = x.Id,
                    WorkflowDefinitionId = x.WorkflowDefinitionId,
                    WorkflowCode = x.WorkflowDefinition != null ? x.WorkflowDefinition.WorkflowCode : null,
                    WorkflowName = x.WorkflowDefinition != null ? x.WorkflowDefinition.WorkflowName : null,
                    WorkflowStepId = x.WorkflowStepId,
                    WorkflowStepCode = x.WorkflowStep != null ? x.WorkflowStep.StepCode : null,
                    WorkflowStepName = x.WorkflowStep != null ? x.WorkflowStep.StepName : null,
                    RequestType = x.RequestType,
                    ReasonCode = x.ReasonCode,
                    ReasonName = x.ReasonName,
                    ReasonCategory = x.ReasonCategory,
                    RejectAction = x.RejectAction,
                    ReturnToStepCode = x.ReturnToStepCode,
                    IsCommentRequired = x.IsCommentRequired,
                    IsAttachmentRequired = x.IsAttachmentRequired,
                    AllowResubmit = x.AllowResubmit,
                    SortOrder = x.SortOrder,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                    CreateByName = x.CreateBy == Guid.Empty ? null : _dbContext.Users.Where(u => u.Id == x.CreateBy).Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode).FirstOrDefault(),
                    UpdateDateTime = x.UpdateDateTime,
                    UpdateBy = x.UpdateBy == Guid.Empty ? null : x.UpdateBy,
                    UpdateByName = x.UpdateBy == Guid.Empty ? null : _dbContext.Users.Where(u => u.Id == x.UpdateBy).Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode).FirstOrDefault()
                })
                .ToListAsync();

            return Ok(ApiResponse<ResponseRejectionReasonPagedResult>.Ok(
                new ResponseRejectionReasonPagedResult
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = items
                },
                "Data rejection reason berhasil diambil."));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<RejectionReasonOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Rejection Reason", Description = "Melihat pilihan rejection reason", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RejectionReason", "Read")]
        public async Task<IActionResult> GetRejectionReasonOptions(
            [FromQuery] Guid? workflowDefinitionId,
            [FromQuery] Guid? workflowStepId,
            [FromQuery] string? requestType,
            [FromQuery] string? rejectAction,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = WorkflowMasterDataSupport.NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = ApplyStandardFilter(BuildBaseQuery(), workflowDefinitionId, workflowStepId, requestType, null, rejectAction, null, onlyActive ? true : null, search);
            var totalData = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.ReasonName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new RejectionReasonOptionResponse
                {
                    Id = x.Id,
                    WorkflowDefinitionId = x.WorkflowDefinitionId,
                    WorkflowStepId = x.WorkflowStepId,
                    RequestType = x.RequestType,
                    ReasonCode = x.ReasonCode,
                    ReasonName = x.ReasonName,
                    RejectAction = x.RejectAction,
                    IsCommentRequired = x.IsCommentRequired,
                    IsAttachmentRequired = x.IsAttachmentRequired,
                    AllowResubmit = x.AllowResubmit
                })
                .ToListAsync();

            return Ok(ApiResponse<RejectionReasonOptionPagedResponse>.Ok(
                new RejectionReasonOptionPagedResponse
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = items
                },
                "Data pilihan rejection reason berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<RejectionReasonDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Rejection Reason", Description = "Melihat detail rejection reason", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RejectionReason", "Read")]
        public async Task<IActionResult> GetRejectionReasonById(Guid id)
        {
            var data = await BuildBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => new RejectionReasonDetailResponse
                {
                    Id = x.Id,
                    WorkflowDefinitionId = x.WorkflowDefinitionId,
                    WorkflowCode = x.WorkflowDefinition != null ? x.WorkflowDefinition.WorkflowCode : null,
                    WorkflowName = x.WorkflowDefinition != null ? x.WorkflowDefinition.WorkflowName : null,
                    WorkflowStepId = x.WorkflowStepId,
                    WorkflowStepCode = x.WorkflowStep != null ? x.WorkflowStep.StepCode : null,
                    WorkflowStepName = x.WorkflowStep != null ? x.WorkflowStep.StepName : null,
                    RequestType = x.RequestType,
                    ReasonCode = x.ReasonCode,
                    ReasonName = x.ReasonName,
                    ReasonCategory = x.ReasonCategory,
                    RejectAction = x.RejectAction,
                    ReturnToStepCode = x.ReturnToStepCode,
                    IsCommentRequired = x.IsCommentRequired,
                    IsAttachmentRequired = x.IsAttachmentRequired,
                    AllowResubmit = x.AllowResubmit,
                    SortOrder = x.SortOrder,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                    CreateByName = x.CreateBy == Guid.Empty ? null : _dbContext.Users.Where(u => u.Id == x.CreateBy).Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode).FirstOrDefault(),
                    UpdateDateTime = x.UpdateDateTime,
                    UpdateBy = x.UpdateBy == Guid.Empty ? null : x.UpdateBy,
                    UpdateByName = x.UpdateBy == Guid.Empty ? null : _dbContext.Users.Where(u => u.Id == x.UpdateBy).Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode).FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (data == null) return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Rejection reason tidak ditemukan."));
            return Ok(ApiResponse<RejectionReasonDetailResponse>.Ok(data, "Detail rejection reason berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<RejectionReasonCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Rejection Reason", Description = "Membuat rejection reason", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("RejectionReason", "Create")]
        public async Task<IActionResult> CreateRejectionReason([FromBody] CreateRejectionReasonRequest request)
        {
            var validation = await ValidateRequestAsync(null, request);
            if (!validation.IsValid) return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage ?? "Data rejection reason tidak valid."));

            var actor = WorkflowMasterDataSupport.GetCurrentUserId(User);
            var entity = new MstRejectionReason
            {
                Id = Guid.NewGuid(),
                WorkflowDefinitionId = WorkflowMasterDataSupport.NormalizeGuid(request.WorkflowDefinitionId),
                WorkflowStepId = WorkflowMasterDataSupport.NormalizeGuid(request.WorkflowStepId),
                RequestType = request.RequestType.Trim(),
                ReasonCode = WorkflowMasterDataSupport.NormalizeCode(request.ReasonCode),
                ReasonName = request.ReasonName.Trim(),
                ReasonCategory = WorkflowMasterDataSupport.NormalizeNullableString(request.ReasonCategory),
                RejectAction = CanonicalRejectAction(request.RejectAction),
                ReturnToStepCode = NormalizeOptionalCode(request.ReturnToStepCode),
                IsCommentRequired = request.IsCommentRequired,
                IsAttachmentRequired = request.IsAttachmentRequired,
                AllowResubmit = request.AllowResubmit,
                SortOrder = request.SortOrder,
                EffectiveStartDate = request.EffectiveStartDate?.Date,
                EffectiveEndDate = request.EffectiveEndDate?.Date,
                Description = WorkflowMasterDataSupport.NormalizeNullableString(request.Description),
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actor,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstRejectionReason>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var result = new RejectionReasonCreateResponse
            {
                Id = entity.Id,
                RequestType = entity.RequestType,
                ReasonCode = entity.ReasonCode,
                ReasonName = entity.ReasonName,
                RejectAction = entity.RejectAction,
                IsActive = entity.IsActive
            };

            await _loggerService.InfoAsync(LogCategory, "RejectionReason.CreateRejectionReason", "Membuat rejection reason.", result);
            return Ok(ApiResponse<RejectionReasonCreateResponse>.Ok(result, "Rejection reason berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Update", "Update Rejection Reason", Description = "Mengubah rejection reason", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("RejectionReason", "Update")]
        public async Task<IActionResult> UpdateRejectionReason(Guid id, [FromBody] UpdateRejectionReasonRequest request)
        {
            var entity = await _dbContext.Set<MstRejectionReason>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Rejection reason tidak ditemukan."));

            var validation = await ValidateRequestAsync(id, request);
            if (!validation.IsValid) return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage ?? "Data rejection reason tidak valid."));

            entity.WorkflowDefinitionId = WorkflowMasterDataSupport.NormalizeGuid(request.WorkflowDefinitionId);
            entity.WorkflowStepId = WorkflowMasterDataSupport.NormalizeGuid(request.WorkflowStepId);
            entity.RequestType = request.RequestType.Trim();
            entity.ReasonCode = WorkflowMasterDataSupport.NormalizeCode(request.ReasonCode);
            entity.ReasonName = request.ReasonName.Trim();
            entity.ReasonCategory = WorkflowMasterDataSupport.NormalizeNullableString(request.ReasonCategory);
            entity.RejectAction = CanonicalRejectAction(request.RejectAction);
            entity.ReturnToStepCode = NormalizeOptionalCode(request.ReturnToStepCode);
            entity.IsCommentRequired = request.IsCommentRequired;
            entity.IsAttachmentRequired = request.IsAttachmentRequired;
            entity.AllowResubmit = request.AllowResubmit;
            entity.SortOrder = request.SortOrder;
            entity.EffectiveStartDate = request.EffectiveStartDate?.Date;
            entity.EffectiveEndDate = request.EffectiveEndDate?.Date;
            entity.Description = WorkflowMasterDataSupport.NormalizeNullableString(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = WorkflowMasterDataSupport.GetCurrentUserId(User);
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(LogCategory, "RejectionReason.UpdateRejectionReason", "Mengubah rejection reason.", new { entity.Id, entity.ReasonCode, entity.ReasonName, entity.RejectAction, entity.IsActive });
            return Ok(ApiResponse<object>.Ok(null, "Rejection reason berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Rejection Reason Status", Description = "Mengubah status rejection reason", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("RejectionReason", "Update")]
        public async Task<IActionResult> UpdateRejectionReasonStatus(Guid id, [FromBody] UpdateWorkflowMasterStatusRequest request)
        {
            var entity = await _dbContext.Set<MstRejectionReason>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Rejection reason tidak ditemukan."));

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = WorkflowMasterDataSupport.GetCurrentUserId(User);
            await _dbContext.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(null, "Status rejection reason berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Rejection Reason", Description = "Menghapus rejection reason", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("RejectionReason", "Delete")]
        public async Task<IActionResult> DeleteRejectionReason(Guid id)
        {
            var entity = await _dbContext.Set<MstRejectionReason>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Rejection reason tidak ditemukan."));

            var now = DateTime.UtcNow;
            var actor = WorkflowMasterDataSupport.GetCurrentUserId(User);
            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actor;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor;
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(LogCategory, "RejectionReason.DeleteRejectionReason", "Menghapus rejection reason.", new { entity.Id, entity.ReasonCode, entity.ReasonName, entity.DeleteDateTime });
            return Ok(ApiResponse<object>.Ok(null, "Rejection reason berhasil dihapus."));
        }

        private IQueryable<MstRejectionReason> BuildBaseQuery() =>
            _dbContext.Set<MstRejectionReason>().AsNoTracking().Where(x => !x.IsDelete);

        private static IQueryable<MstRejectionReason> ApplyStandardFilter(
            IQueryable<MstRejectionReason> query,
            Guid? workflowDefinitionId,
            Guid? workflowStepId,
            string? requestType,
            string? reasonCategory,
            string? rejectAction,
            bool? allowResubmit,
            bool? isActive,
            string? search)
        {
            if (workflowDefinitionId.HasValue && workflowDefinitionId.Value != Guid.Empty) query = query.Where(x => x.WorkflowDefinitionId == workflowDefinitionId.Value);
            if (workflowStepId.HasValue && workflowStepId.Value != Guid.Empty) query = query.Where(x => x.WorkflowStepId == workflowStepId.Value);
            if (!string.IsNullOrWhiteSpace(requestType)) query = query.Where(x => x.RequestType == requestType.Trim());
            if (!string.IsNullOrWhiteSpace(reasonCategory)) query = query.Where(x => x.ReasonCategory == reasonCategory.Trim());
            if (!string.IsNullOrWhiteSpace(rejectAction)) query = query.Where(x => x.RejectAction == rejectAction.Trim());
            if (allowResubmit.HasValue) query = query.Where(x => x.AllowResubmit == allowResubmit.Value);
            if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.ReasonCode.ToLower().Contains(keyword) ||
                    x.ReasonName.ToLower().Contains(keyword) ||
                    x.RequestType.ToLower().Contains(keyword) ||
                    x.RejectAction.ToLower().Contains(keyword) ||
                    x.ReasonCategory != null && x.ReasonCategory.ToLower().Contains(keyword) ||
                    x.Description != null && x.Description.ToLower().Contains(keyword));
            }

            return query;
        }

        private static IOrderedQueryable<MstRejectionReason> ApplySorting(IQueryable<MstRejectionReason> query, string? sortBy, string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "sortOrder").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "reasoncode" => desc ? query.OrderByDescending(x => x.ReasonCode) : query.OrderBy(x => x.ReasonCode),
                "reasonname" => desc ? query.OrderByDescending(x => x.ReasonName) : query.OrderBy(x => x.ReasonName),
                "requesttype" => desc ? query.OrderByDescending(x => x.RequestType) : query.OrderBy(x => x.RequestType),
                "reasoncategory" => desc ? query.OrderByDescending(x => x.ReasonCategory) : query.OrderBy(x => x.ReasonCategory),
                "rejectaction" => desc ? query.OrderByDescending(x => x.RejectAction) : query.OrderBy(x => x.RejectAction),
                "isactive" => desc ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
                _ => desc ? query.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.ReasonName) : query.OrderBy(x => x.SortOrder).ThenBy(x => x.ReasonName)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(Guid? excludeId, CreateRejectionReasonRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RequestType)) return (false, "Request type wajib diisi.");
            if (string.IsNullOrWhiteSpace(request.ReasonCode)) return (false, "Kode alasan wajib diisi.");
            if (string.IsNullOrWhiteSpace(request.ReasonName)) return (false, "Nama alasan wajib diisi.");
            if (!RejectActions.Contains(request.RejectAction)) return (false, "Reject action tidak valid.");
            if (!WorkflowMasterDataSupport.IsEffectiveDateValid(request.EffectiveStartDate, request.EffectiveEndDate)) return (false, "Effective end date tidak boleh lebih kecil dari effective start date.");

            var definitionId = WorkflowMasterDataSupport.NormalizeGuid(request.WorkflowDefinitionId);
            var stepId = WorkflowMasterDataSupport.NormalizeGuid(request.WorkflowStepId);
            if (stepId.HasValue && !definitionId.HasValue) return (false, "Workflow definition wajib dipilih jika workflow step diisi.");

            MstWorkflowDefinition? definition = null;
            if (definitionId.HasValue)
            {
                definition = await _dbContext.Set<MstWorkflowDefinition>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == definitionId.Value && !x.IsDelete);
                if (definition == null) return (false, "Workflow definition tidak ditemukan.");
                if (!string.Equals(definition.RequestType, request.RequestType.Trim(), StringComparison.OrdinalIgnoreCase)) return (false, "Request type harus sama dengan request type workflow definition yang dipilih.");
            }

            if (stepId.HasValue)
            {
                var step = await _dbContext.Set<MstWorkflowStep>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == stepId.Value && !x.IsDelete);
                if (step == null) return (false, "Workflow step tidak ditemukan.");
                if (step.WorkflowDefinitionId != definitionId) return (false, "Workflow step tidak berasal dari workflow definition yang dipilih.");
            }

            var rejectAction = CanonicalRejectAction(request.RejectAction);
            var returnToStepCode = NormalizeOptionalCode(request.ReturnToStepCode);
            if (rejectAction == "ReturnToSpecificStep" && string.IsNullOrWhiteSpace(returnToStepCode)) return (false, "Return to step code wajib diisi untuk reject action ReturnToSpecificStep.");
            if (rejectAction != "ReturnToSpecificStep" && !string.IsNullOrWhiteSpace(returnToStepCode)) return (false, "Return to step code hanya boleh diisi untuk reject action ReturnToSpecificStep.");

            if (returnToStepCode != null && definitionId.HasValue)
            {
                var targetExists = await _dbContext.Set<MstWorkflowStep>().AsNoTracking().AnyAsync(x =>
                    x.WorkflowDefinitionId == definitionId.Value &&
                    x.StepCode == returnToStepCode &&
                    !x.IsDelete);
                if (!targetExists) return (false, "Return to step code tidak ditemukan pada workflow definition yang dipilih.");
            }

            var code = WorkflowMasterDataSupport.NormalizeCode(request.ReasonCode);
            var requestType = request.RequestType.Trim().ToLower();
            var duplicateQuery = _dbContext.Set<MstRejectionReason>().AsNoTracking().Where(x =>
                !x.IsDelete &&
                x.WorkflowDefinitionId == definitionId &&
                x.WorkflowStepId == stepId &&
                x.RequestType.ToLower() == requestType &&
                x.ReasonCode == code);
            if (excludeId.HasValue) duplicateQuery = duplicateQuery.Where(x => x.Id != excludeId.Value);
            if (await duplicateQuery.AnyAsync()) return (false, "Kode alasan tersebut sudah digunakan pada request type dan scope workflow yang sama.");

            return (true, null);
        }

        private static string CanonicalRejectAction(string value) => RejectActions.First(x => string.Equals(x, value.Trim(), StringComparison.OrdinalIgnoreCase));
        private static string? NormalizeOptionalCode(string? value) => string.IsNullOrWhiteSpace(value) ? null : WorkflowMasterDataSupport.NormalizeCode(value);
    }
}
