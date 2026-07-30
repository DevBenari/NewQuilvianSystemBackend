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

using ResponseRequestReasonPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.DTOs.RequestReasonResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/request-reasons")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Request Reason",
        AreaName = "Corporate",
        ControllerName = "RequestReason",
        Description = "Corporate human resource master data request reason",
        SortOrder = 74)]
    [Tags("Corporate / Human Resource / Master Data / Request Reason")]
    public class RequestReasonController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.MasterData";
        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public RequestReasonController(ApplicationDbContext dbContext, LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<RequestReasonFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Request Reason", Description = "Melihat metadata filter request reason", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RequestReason", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new RequestReasonFilterMetadataResponse
            {
                DefaultFilter = new RequestReasonDefaultFilterResponse(),
                CustomPeriods = WorkflowMasterDataSupport.BuildPeriodOptions(),
                SortOptions = new List<WorkflowMasterSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "reasonCode", Label = "Kode alasan" },
                    new() { Value = "reasonName", Label = "Nama alasan" },
                    new() { Value = "requestType", Label = "Tipe permintaan" },
                    new() { Value = "reasonCategory", Label = "Kategori alasan" },
                    new() { Value = "sortOrder", Label = "Urutan" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            await _loggerService.InfoAsync(LogCategory, "RequestReason.GetFilterMetadata", "Mengambil metadata filter request reason.", result);
            return Ok(ApiResponse<RequestReasonFilterMetadataResponse>.Ok(result, "Metadata filter request reason berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<RequestReasonSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Request Reason", Description = "Melihat ringkasan request reason", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RequestReason", "Read")]
        public async Task<IActionResult> GetSummary([FromQuery] Guid? workflowDefinitionId, [FromQuery] string? requestType)
        {
            var query = BuildBaseQuery();
            if (workflowDefinitionId.HasValue && workflowDefinitionId.Value != Guid.Empty) query = query.Where(x => x.WorkflowDefinitionId == workflowDefinitionId.Value);
            if (!string.IsNullOrWhiteSpace(requestType)) query = query.Where(x => x.RequestType == requestType.Trim());

            var result = new RequestReasonSummaryResponse
            {
                TotalRequestReason = await query.CountAsync(),
                ActiveRequestReason = await query.CountAsync(x => x.IsActive),
                InactiveRequestReason = await query.CountAsync(x => !x.IsActive),
                CommentRequiredReason = await query.CountAsync(x => x.IsCommentRequired),
                AttachmentRequiredReason = await query.CountAsync(x => x.IsAttachmentRequired),
                EmployeeSelectableReason = await query.CountAsync(x => x.IsEmployeeSelectable),
                ManagerSelectableReason = await query.CountAsync(x => x.IsManagerSelectable),
                GlobalReason = await query.CountAsync(x => x.WorkflowDefinitionId == null),
                WorkflowSpecificReason = await query.CountAsync(x => x.WorkflowDefinitionId != null)
            };

            return Ok(ApiResponse<RequestReasonSummaryResponse>.Ok(result, "Ringkasan request reason berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseRequestReasonPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Request Reason", Description = "Melihat data request reason", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RequestReason", "Read")]
        public async Task<IActionResult> GetRequestReasons(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? workflowDefinitionId,
            [FromQuery] string? requestType,
            [FromQuery] string? reasonCategory,
            [FromQuery] bool? isCommentRequired,
            [FromQuery] bool? isAttachmentRequired,
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
            query = ApplyStandardFilter(query, workflowDefinitionId, requestType, reasonCategory, isCommentRequired, isAttachmentRequired, isActive, search);
            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new RequestReasonResponse
                {
                    Id = x.Id,
                    WorkflowDefinitionId = x.WorkflowDefinitionId,
                    WorkflowCode = x.WorkflowDefinition != null ? x.WorkflowDefinition.WorkflowCode : null,
                    WorkflowName = x.WorkflowDefinition != null ? x.WorkflowDefinition.WorkflowName : null,
                    RequestType = x.RequestType,
                    ReasonCode = x.ReasonCode,
                    ReasonName = x.ReasonName,
                    ReasonCategory = x.ReasonCategory,
                    IsCommentRequired = x.IsCommentRequired,
                    IsAttachmentRequired = x.IsAttachmentRequired,
                    IsEmployeeSelectable = x.IsEmployeeSelectable,
                    IsManagerSelectable = x.IsManagerSelectable,
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

            return Ok(ApiResponse<ResponseRequestReasonPagedResult>.Ok(
                new ResponseRequestReasonPagedResult
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = items
                },
                "Data request reason berhasil diambil."));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<RequestReasonOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Request Reason", Description = "Melihat pilihan request reason", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RequestReason", "Read")]
        public async Task<IActionResult> GetRequestReasonOptions(
            [FromQuery] Guid? workflowDefinitionId,
            [FromQuery] string? requestType,
            [FromQuery] bool employeeSelectableOnly = false,
            [FromQuery] bool managerSelectableOnly = false,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = WorkflowMasterDataSupport.NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = ApplyStandardFilter(BuildBaseQuery(), workflowDefinitionId, requestType, null, null, null, onlyActive ? true : null, search);
            if (employeeSelectableOnly) query = query.Where(x => x.IsEmployeeSelectable);
            if (managerSelectableOnly) query = query.Where(x => x.IsManagerSelectable);

            var totalData = await query.CountAsync();
            var items = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.ReasonName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new RequestReasonOptionResponse
                {
                    Id = x.Id,
                    WorkflowDefinitionId = x.WorkflowDefinitionId,
                    RequestType = x.RequestType,
                    ReasonCode = x.ReasonCode,
                    ReasonName = x.ReasonName,
                    ReasonCategory = x.ReasonCategory,
                    IsCommentRequired = x.IsCommentRequired,
                    IsAttachmentRequired = x.IsAttachmentRequired
                })
                .ToListAsync();

            return Ok(ApiResponse<RequestReasonOptionPagedResponse>.Ok(
                new RequestReasonOptionPagedResponse
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = items
                },
                "Data pilihan request reason berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<RequestReasonDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Request Reason", Description = "Melihat detail request reason", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RequestReason", "Read")]
        public async Task<IActionResult> GetRequestReasonById(Guid id)
        {
            var data = await BuildBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => new RequestReasonDetailResponse
                {
                    Id = x.Id,
                    WorkflowDefinitionId = x.WorkflowDefinitionId,
                    WorkflowCode = x.WorkflowDefinition != null ? x.WorkflowDefinition.WorkflowCode : null,
                    WorkflowName = x.WorkflowDefinition != null ? x.WorkflowDefinition.WorkflowName : null,
                    RequestType = x.RequestType,
                    ReasonCode = x.ReasonCode,
                    ReasonName = x.ReasonName,
                    ReasonCategory = x.ReasonCategory,
                    IsCommentRequired = x.IsCommentRequired,
                    IsAttachmentRequired = x.IsAttachmentRequired,
                    IsEmployeeSelectable = x.IsEmployeeSelectable,
                    IsManagerSelectable = x.IsManagerSelectable,
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

            if (data == null) return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Request reason tidak ditemukan."));
            return Ok(ApiResponse<RequestReasonDetailResponse>.Ok(data, "Detail request reason berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<RequestReasonCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Request Reason", Description = "Membuat request reason", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("RequestReason", "Create")]
        public async Task<IActionResult> CreateRequestReason([FromBody] CreateRequestReasonRequest request)
        {
            var validation = await ValidateRequestAsync(null, request);
            if (!validation.IsValid) return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage ?? "Data request reason tidak valid."));

            var actor = WorkflowMasterDataSupport.GetCurrentUserId(User);
            var entity = new MstRequestReason
            {
                Id = Guid.NewGuid(),
                WorkflowDefinitionId = WorkflowMasterDataSupport.NormalizeGuid(request.WorkflowDefinitionId),
                RequestType = request.RequestType.Trim(),
                ReasonCode = WorkflowMasterDataSupport.NormalizeCode(request.ReasonCode),
                ReasonName = request.ReasonName.Trim(),
                ReasonCategory = WorkflowMasterDataSupport.NormalizeNullableString(request.ReasonCategory),
                IsCommentRequired = request.IsCommentRequired,
                IsAttachmentRequired = request.IsAttachmentRequired,
                IsEmployeeSelectable = request.IsEmployeeSelectable,
                IsManagerSelectable = request.IsManagerSelectable,
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

            _dbContext.Set<MstRequestReason>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var result = new RequestReasonCreateResponse
            {
                Id = entity.Id,
                RequestType = entity.RequestType,
                ReasonCode = entity.ReasonCode,
                ReasonName = entity.ReasonName,
                IsActive = entity.IsActive
            };

            await _loggerService.InfoAsync(LogCategory, "RequestReason.CreateRequestReason", "Membuat request reason.", result);
            return Ok(ApiResponse<RequestReasonCreateResponse>.Ok(result, "Request reason berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Update", "Update Request Reason", Description = "Mengubah request reason", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("RequestReason", "Update")]
        public async Task<IActionResult> UpdateRequestReason(Guid id, [FromBody] UpdateRequestReasonRequest request)
        {
            var entity = await _dbContext.Set<MstRequestReason>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Request reason tidak ditemukan."));

            var validation = await ValidateRequestAsync(id, request);
            if (!validation.IsValid) return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage ?? "Data request reason tidak valid."));

            entity.WorkflowDefinitionId = WorkflowMasterDataSupport.NormalizeGuid(request.WorkflowDefinitionId);
            entity.RequestType = request.RequestType.Trim();
            entity.ReasonCode = WorkflowMasterDataSupport.NormalizeCode(request.ReasonCode);
            entity.ReasonName = request.ReasonName.Trim();
            entity.ReasonCategory = WorkflowMasterDataSupport.NormalizeNullableString(request.ReasonCategory);
            entity.IsCommentRequired = request.IsCommentRequired;
            entity.IsAttachmentRequired = request.IsAttachmentRequired;
            entity.IsEmployeeSelectable = request.IsEmployeeSelectable;
            entity.IsManagerSelectable = request.IsManagerSelectable;
            entity.SortOrder = request.SortOrder;
            entity.EffectiveStartDate = request.EffectiveStartDate?.Date;
            entity.EffectiveEndDate = request.EffectiveEndDate?.Date;
            entity.Description = WorkflowMasterDataSupport.NormalizeNullableString(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = WorkflowMasterDataSupport.GetCurrentUserId(User);
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(LogCategory, "RequestReason.UpdateRequestReason", "Mengubah request reason.", new { entity.Id, entity.ReasonCode, entity.ReasonName, entity.IsActive });
            return Ok(ApiResponse<object>.Ok(null, "Request reason berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Request Reason Status", Description = "Mengubah status request reason", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("RequestReason", "Update")]
        public async Task<IActionResult> UpdateRequestReasonStatus(Guid id, [FromBody] UpdateWorkflowMasterStatusRequest request)
        {
            var entity = await _dbContext.Set<MstRequestReason>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Request reason tidak ditemukan."));

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = WorkflowMasterDataSupport.GetCurrentUserId(User);
            await _dbContext.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(null, "Status request reason berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Request Reason", Description = "Menghapus request reason", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("RequestReason", "Delete")]
        public async Task<IActionResult> DeleteRequestReason(Guid id)
        {
            var entity = await _dbContext.Set<MstRequestReason>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Request reason tidak ditemukan."));

            var now = DateTime.UtcNow;
            var actor = WorkflowMasterDataSupport.GetCurrentUserId(User);
            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actor;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor;
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(LogCategory, "RequestReason.DeleteRequestReason", "Menghapus request reason.", new { entity.Id, entity.ReasonCode, entity.ReasonName, entity.DeleteDateTime });
            return Ok(ApiResponse<object>.Ok(null, "Request reason berhasil dihapus."));
        }

        private IQueryable<MstRequestReason> BuildBaseQuery() =>
            _dbContext.Set<MstRequestReason>().AsNoTracking().Where(x => !x.IsDelete);

        private static IQueryable<MstRequestReason> ApplyStandardFilter(
            IQueryable<MstRequestReason> query,
            Guid? workflowDefinitionId,
            string? requestType,
            string? reasonCategory,
            bool? isCommentRequired,
            bool? isAttachmentRequired,
            bool? isActive,
            string? search)
        {
            if (workflowDefinitionId.HasValue && workflowDefinitionId.Value != Guid.Empty) query = query.Where(x => x.WorkflowDefinitionId == workflowDefinitionId.Value);
            if (!string.IsNullOrWhiteSpace(requestType)) query = query.Where(x => x.RequestType == requestType.Trim());
            if (!string.IsNullOrWhiteSpace(reasonCategory)) query = query.Where(x => x.ReasonCategory == reasonCategory.Trim());
            if (isCommentRequired.HasValue) query = query.Where(x => x.IsCommentRequired == isCommentRequired.Value);
            if (isAttachmentRequired.HasValue) query = query.Where(x => x.IsAttachmentRequired == isAttachmentRequired.Value);
            if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.ReasonCode.ToLower().Contains(keyword) ||
                    x.ReasonName.ToLower().Contains(keyword) ||
                    x.RequestType.ToLower().Contains(keyword) ||
                    x.ReasonCategory != null && x.ReasonCategory.ToLower().Contains(keyword) ||
                    x.Description != null && x.Description.ToLower().Contains(keyword));
            }

            return query;
        }

        private static IOrderedQueryable<MstRequestReason> ApplySorting(IQueryable<MstRequestReason> query, string? sortBy, string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "sortOrder").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "reasoncode" => desc ? query.OrderByDescending(x => x.ReasonCode) : query.OrderBy(x => x.ReasonCode),
                "reasonname" => desc ? query.OrderByDescending(x => x.ReasonName) : query.OrderBy(x => x.ReasonName),
                "requesttype" => desc ? query.OrderByDescending(x => x.RequestType) : query.OrderBy(x => x.RequestType),
                "reasoncategory" => desc ? query.OrderByDescending(x => x.ReasonCategory) : query.OrderBy(x => x.ReasonCategory),
                "isactive" => desc ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
                _ => desc ? query.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.ReasonName) : query.OrderBy(x => x.SortOrder).ThenBy(x => x.ReasonName)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(Guid? excludeId, CreateRequestReasonRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RequestType)) return (false, "Request type wajib diisi.");
            if (string.IsNullOrWhiteSpace(request.ReasonCode)) return (false, "Kode alasan wajib diisi.");
            if (string.IsNullOrWhiteSpace(request.ReasonName)) return (false, "Nama alasan wajib diisi.");
            if (!WorkflowMasterDataSupport.IsEffectiveDateValid(request.EffectiveStartDate, request.EffectiveEndDate)) return (false, "Effective end date tidak boleh lebih kecil dari effective start date.");

            var definitionId = WorkflowMasterDataSupport.NormalizeGuid(request.WorkflowDefinitionId);
            if (definitionId.HasValue)
            {
                var definition = await _dbContext.Set<MstWorkflowDefinition>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == definitionId.Value && !x.IsDelete);
                if (definition == null) return (false, "Workflow definition tidak ditemukan.");
                if (!string.Equals(definition.RequestType, request.RequestType.Trim(), StringComparison.OrdinalIgnoreCase)) return (false, "Request type harus sama dengan request type workflow definition yang dipilih.");
            }

            var code = WorkflowMasterDataSupport.NormalizeCode(request.ReasonCode);
            var requestType = request.RequestType.Trim().ToLower();
            var duplicateQuery = _dbContext.Set<MstRequestReason>().AsNoTracking().Where(x =>
                !x.IsDelete &&
                x.WorkflowDefinitionId == definitionId &&
                x.RequestType.ToLower() == requestType &&
                x.ReasonCode == code);
            if (excludeId.HasValue) duplicateQuery = duplicateQuery.Where(x => x.Id != excludeId.Value);
            if (await duplicateQuery.AnyAsync()) return (false, "Kode alasan tersebut sudah digunakan pada request type dan scope workflow yang sama.");

            return (true, null);
        }
    }
}
