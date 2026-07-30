using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

using ResponseApprovalMatrixPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.DTOs.ApprovalMatrixResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/approval-matrices")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Approval Matrix",
        AreaName = "Corporate",
        ControllerName = "ApprovalMatrix",
        Description = "Corporate human resource master data approval matrix",
        SortOrder = 72)]
    [Tags("Corporate / Human Resource / Master Data / Approval Matrix")]
    public class ApprovalMatrixController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.MasterData";

        private static readonly HashSet<string> ApproverSources = new(StringComparer.OrdinalIgnoreCase)
        {
            "RequesterManager", "DirectManager", "ManagerLevel", "Position",
            "OrganizationUnit", "Role", "SpecificRole", "SpecificUser",
            "OrganizationHead", "DepartmentHead", "SiteHr", "CorporateHr",
            "PayrollOfficer", "FinanceOfficer", "CostCenterOwner",
            "CredentialingCommittee"
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public ApprovalMatrixController(ApplicationDbContext dbContext, LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<ApprovalMatrixFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Approval Matrix", Description = "Melihat metadata filter approval matrix", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ApprovalMatrix", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new ApprovalMatrixFilterMetadataResponse
            {
                DefaultFilter = new ApprovalMatrixDefaultFilterResponse(),
                CustomPeriods = WorkflowMasterDataSupport.BuildPeriodOptions(),
                SortOptions = new List<WorkflowMasterSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "approvalMatrixCode", Label = "Kode approval matrix" },
                    new() { Value = "approvalMatrixName", Label = "Nama approval matrix" },
                    new() { Value = "workflowName", Label = "Nama workflow" },
                    new() { Value = "stepName", Label = "Nama step" },
                    new() { Value = "priority", Label = "Prioritas" },
                    new() { Value = "isFallback", Label = "Fallback" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                ApproverSources = ApproverSources
                    .OrderBy(x => x)
                    .Select(x => new WorkflowMasterLookupOptionResponse { Value = x, Label = x })
                    .ToList()
            };

            await _loggerService.InfoAsync(LogCategory, "ApprovalMatrix.GetFilterMetadata", "Mengambil metadata filter approval matrix.", result);
            return Ok(ApiResponse<ApprovalMatrixFilterMetadataResponse>.Ok(result, "Metadata filter approval matrix berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<ApprovalMatrixSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Approval Matrix", Description = "Melihat ringkasan approval matrix", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ApprovalMatrix", "Read")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] Guid? workflowDefinitionId,
            [FromQuery] Guid? workflowStepId)
        {
            var query = BuildBaseQuery();
            if (workflowDefinitionId.HasValue && workflowDefinitionId.Value != Guid.Empty)
                query = query.Where(x => x.WorkflowDefinitionId == workflowDefinitionId.Value);
            if (workflowStepId.HasValue && workflowStepId.Value != Guid.Empty)
                query = query.Where(x => x.WorkflowStepId == workflowStepId.Value);

            var result = new ApprovalMatrixSummaryResponse
            {
                TotalApprovalMatrix = await query.CountAsync(),
                ActiveApprovalMatrix = await query.CountAsync(x => x.IsActive),
                InactiveApprovalMatrix = await query.CountAsync(x => !x.IsActive),
                FallbackApprovalMatrix = await query.CountAsync(x => x.IsFallback),
                ScopedApprovalMatrix = await query.CountAsync(x =>
                    x.LegalEntityId != null || x.HospitalSiteId != null ||
                    x.OrganizationUnitId != null || x.DepartmentId != null ||
                    x.RequesterPositionId != null || x.EmployeeCategoryId != null ||
                    x.EmploymentTypeId != null),
                AmountBasedApprovalMatrix = await query.CountAsync(x => x.MinimumAmount != null || x.MaximumAmount != null),
                DurationBasedApprovalMatrix = await query.CountAsync(x =>
                    x.MinimumDurationHours != null || x.MaximumDurationHours != null ||
                    x.MinimumDurationDays != null || x.MaximumDurationDays != null),
                JsonConditionApprovalMatrix = await query.CountAsync(x => x.ConditionDefinitionJson != null)
            };

            return Ok(ApiResponse<ApprovalMatrixSummaryResponse>.Ok(result, "Ringkasan approval matrix berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseApprovalMatrixPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Approval Matrix", Description = "Melihat data approval matrix", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ApprovalMatrix", "Read")]
        public async Task<IActionResult> GetApprovalMatrices(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? workflowDefinitionId,
            [FromQuery] Guid? workflowStepId,
            [FromQuery] Guid? legalEntityId,
            [FromQuery] Guid? hospitalSiteId,
            [FromQuery] Guid? organizationUnitId,
            [FromQuery] Guid? departmentId,
            [FromQuery] string? approverSourceType,
            [FromQuery] bool? isFallback,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "priority",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = WorkflowMasterDataSupport.NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = WorkflowMasterDataSupport.ApplyDateFilter(BuildBaseQuery(), startDate, endDate, customPeriod);
            query = ApplyStandardFilter(
                query, workflowDefinitionId, workflowStepId, legalEntityId,
                hospitalSiteId, organizationUnitId, departmentId,
                approverSourceType, isFallback, isActive, search);

            var totalData = await query.CountAsync();
            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ApprovalMatrixResponse
                {
                    Id = x.Id,
                    WorkflowDefinitionId = x.WorkflowDefinitionId,
                    WorkflowCode = x.WorkflowDefinition != null ? x.WorkflowDefinition.WorkflowCode : null,
                    WorkflowName = x.WorkflowDefinition != null ? x.WorkflowDefinition.WorkflowName : null,
                    WorkflowStepId = x.WorkflowStepId,
                    WorkflowStepCode = x.WorkflowStep != null ? x.WorkflowStep.StepCode : null,
                    WorkflowStepName = x.WorkflowStep != null ? x.WorkflowStep.StepName : null,
                    ApprovalMatrixCode = x.ApprovalMatrixCode,
                    ApprovalMatrixName = x.ApprovalMatrixName,
                    LegalEntityId = x.LegalEntityId,
                    HospitalSiteId = x.HospitalSiteId,
                    OrganizationUnitId = x.OrganizationUnitId,
                    DepartmentId = x.DepartmentId,
                    RequesterPositionId = x.RequesterPositionId,
                    EmployeeCategoryId = x.EmployeeCategoryId,
                    EmploymentTypeId = x.EmploymentTypeId,
                    MinimumAmount = x.MinimumAmount,
                    MaximumAmount = x.MaximumAmount,
                    CurrencyCode = x.CurrencyCode,
                    MinimumDurationHours = x.MinimumDurationHours,
                    MaximumDurationHours = x.MaximumDurationHours,
                    MinimumDurationDays = x.MinimumDurationDays,
                    MaximumDurationDays = x.MaximumDurationDays,
                    ApproverSourceType = x.ApproverSourceType,
                    ApproverPositionId = x.ApproverPositionId,
                    ApproverOrganizationUnitId = x.ApproverOrganizationUnitId,
                    SpecificApproverUserId = x.SpecificApproverUserId,
                    SpecificApproverUserName = x.SpecificApproverUserId == null
                        ? null
                        : _dbContext.Users.Where(u => u.Id == x.SpecificApproverUserId.Value)
                            .Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode)
                            .FirstOrDefault(),
                    ApproverRoleCode = x.ApproverRoleCode,
                    ManagerLevel = x.ManagerLevel,
                    Priority = x.Priority,
                    IsFallback = x.IsFallback,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    ConditionDefinitionJson = x.ConditionDefinitionJson,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    StepInstanceCount = _dbContext.Set<TrxWorkflowStepInstance>().Count(y => y.ApprovalMatrixId == x.Id && !y.IsDelete),
                    ApproverAssignmentCount = _dbContext.Set<TrxWorkflowApproverAssignment>().Count(y => y.ApprovalMatrixId == x.Id && !y.IsDelete),
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                    CreateByName = x.CreateBy == Guid.Empty ? null : _dbContext.Users.Where(u => u.Id == x.CreateBy).Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode).FirstOrDefault(),
                    UpdateDateTime = x.UpdateDateTime,
                    UpdateBy = x.UpdateBy == Guid.Empty ? null : x.UpdateBy,
                    UpdateByName = x.UpdateBy == Guid.Empty ? null : _dbContext.Users.Where(u => u.Id == x.UpdateBy).Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode).FirstOrDefault()
                })
                .ToListAsync();

            return Ok(ApiResponse<ResponseApprovalMatrixPagedResult>.Ok(
                new ResponseApprovalMatrixPagedResult
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = items
                },
                "Data approval matrix berhasil diambil."));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<ApprovalMatrixOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Approval Matrix", Description = "Melihat pilihan approval matrix", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ApprovalMatrix", "Read")]
        public async Task<IActionResult> GetApprovalMatrixOptions(
            [FromQuery] Guid? workflowDefinitionId,
            [FromQuery] Guid? workflowStepId,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = WorkflowMasterDataSupport.NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = ApplyStandardFilter(
                BuildBaseQuery(), workflowDefinitionId, workflowStepId,
                null, null, null, null, null, null,
                onlyActive ? true : null, search);

            var totalData = await query.CountAsync();
            var items = await query
                .OrderByDescending(x => x.Priority)
                .ThenBy(x => x.ApprovalMatrixName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ApprovalMatrixOptionResponse
                {
                    Id = x.Id,
                    WorkflowDefinitionId = x.WorkflowDefinitionId,
                    WorkflowCode = x.WorkflowDefinition != null ? x.WorkflowDefinition.WorkflowCode : null,
                    WorkflowStepId = x.WorkflowStepId,
                    WorkflowStepCode = x.WorkflowStep != null ? x.WorkflowStep.StepCode : null,
                    ApprovalMatrixCode = x.ApprovalMatrixCode,
                    ApprovalMatrixName = x.ApprovalMatrixName,
                    ApproverSourceType = x.ApproverSourceType,
                    Priority = x.Priority,
                    IsFallback = x.IsFallback
                })
                .ToListAsync();

            return Ok(ApiResponse<ApprovalMatrixOptionPagedResponse>.Ok(
                new ApprovalMatrixOptionPagedResponse
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = items
                },
                "Data pilihan approval matrix berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ApprovalMatrixDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Approval Matrix", Description = "Melihat detail approval matrix", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ApprovalMatrix", "Read")]
        public async Task<IActionResult> GetApprovalMatrixById(Guid id)
        {
            var data = await BuildBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => new ApprovalMatrixDetailResponse
                {
                    Id = x.Id,
                    WorkflowDefinitionId = x.WorkflowDefinitionId,
                    WorkflowCode = x.WorkflowDefinition != null ? x.WorkflowDefinition.WorkflowCode : null,
                    WorkflowName = x.WorkflowDefinition != null ? x.WorkflowDefinition.WorkflowName : null,
                    WorkflowStepId = x.WorkflowStepId,
                    WorkflowStepCode = x.WorkflowStep != null ? x.WorkflowStep.StepCode : null,
                    WorkflowStepName = x.WorkflowStep != null ? x.WorkflowStep.StepName : null,
                    ApprovalMatrixCode = x.ApprovalMatrixCode,
                    ApprovalMatrixName = x.ApprovalMatrixName,
                    LegalEntityId = x.LegalEntityId,
                    HospitalSiteId = x.HospitalSiteId,
                    OrganizationUnitId = x.OrganizationUnitId,
                    DepartmentId = x.DepartmentId,
                    RequesterPositionId = x.RequesterPositionId,
                    EmployeeCategoryId = x.EmployeeCategoryId,
                    EmploymentTypeId = x.EmploymentTypeId,
                    MinimumAmount = x.MinimumAmount,
                    MaximumAmount = x.MaximumAmount,
                    CurrencyCode = x.CurrencyCode,
                    MinimumDurationHours = x.MinimumDurationHours,
                    MaximumDurationHours = x.MaximumDurationHours,
                    MinimumDurationDays = x.MinimumDurationDays,
                    MaximumDurationDays = x.MaximumDurationDays,
                    ApproverSourceType = x.ApproverSourceType,
                    ApproverPositionId = x.ApproverPositionId,
                    ApproverOrganizationUnitId = x.ApproverOrganizationUnitId,
                    SpecificApproverUserId = x.SpecificApproverUserId,
                    SpecificApproverUserName = x.SpecificApproverUserId == null ? null : _dbContext.Users.Where(u => u.Id == x.SpecificApproverUserId.Value).Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode).FirstOrDefault(),
                    ApproverRoleCode = x.ApproverRoleCode,
                    ManagerLevel = x.ManagerLevel,
                    Priority = x.Priority,
                    IsFallback = x.IsFallback,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    ConditionDefinitionJson = x.ConditionDefinitionJson,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    StepInstanceCount = _dbContext.Set<TrxWorkflowStepInstance>().Count(y => y.ApprovalMatrixId == x.Id && !y.IsDelete),
                    ApproverAssignmentCount = _dbContext.Set<TrxWorkflowApproverAssignment>().Count(y => y.ApprovalMatrixId == x.Id && !y.IsDelete),
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                    CreateByName = x.CreateBy == Guid.Empty ? null : _dbContext.Users.Where(u => u.Id == x.CreateBy).Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode).FirstOrDefault(),
                    UpdateDateTime = x.UpdateDateTime,
                    UpdateBy = x.UpdateBy == Guid.Empty ? null : x.UpdateBy,
                    UpdateByName = x.UpdateBy == Guid.Empty ? null : _dbContext.Users.Where(u => u.Id == x.UpdateBy).Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode).FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (data == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Approval matrix tidak ditemukan."));

            return Ok(ApiResponse<ApprovalMatrixDetailResponse>.Ok(data, "Detail approval matrix berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ApprovalMatrixCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Approval Matrix", Description = "Membuat approval matrix", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("ApprovalMatrix", "Create")]
        public async Task<IActionResult> CreateApprovalMatrix([FromBody] CreateApprovalMatrixRequest request)
        {
            var validation = await ValidateRequestAsync(null, request);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage ?? "Data approval matrix tidak valid."));

            var actor = WorkflowMasterDataSupport.GetCurrentUserId(User);
            var entity = new MstApprovalMatrix
            {
                Id = Guid.NewGuid(),
                WorkflowDefinitionId = request.WorkflowDefinitionId,
                WorkflowStepId = request.WorkflowStepId,
                ApprovalMatrixCode = WorkflowMasterDataSupport.NormalizeCode(request.ApprovalMatrixCode),
                ApprovalMatrixName = request.ApprovalMatrixName.Trim(),
                LegalEntityId = WorkflowMasterDataSupport.NormalizeGuid(request.LegalEntityId),
                HospitalSiteId = WorkflowMasterDataSupport.NormalizeGuid(request.HospitalSiteId),
                OrganizationUnitId = WorkflowMasterDataSupport.NormalizeGuid(request.OrganizationUnitId),
                DepartmentId = WorkflowMasterDataSupport.NormalizeGuid(request.DepartmentId),
                RequesterPositionId = WorkflowMasterDataSupport.NormalizeGuid(request.RequesterPositionId),
                EmployeeCategoryId = WorkflowMasterDataSupport.NormalizeGuid(request.EmployeeCategoryId),
                EmploymentTypeId = WorkflowMasterDataSupport.NormalizeGuid(request.EmploymentTypeId),
                MinimumAmount = request.MinimumAmount,
                MaximumAmount = request.MaximumAmount,
                CurrencyCode = string.IsNullOrWhiteSpace(request.CurrencyCode) ? "IDR" : request.CurrencyCode.Trim().ToUpperInvariant(),
                MinimumDurationHours = request.MinimumDurationHours,
                MaximumDurationHours = request.MaximumDurationHours,
                MinimumDurationDays = request.MinimumDurationDays,
                MaximumDurationDays = request.MaximumDurationDays,
                ApproverSourceType = CanonicalSource(request.ApproverSourceType),
                ApproverPositionId = WorkflowMasterDataSupport.NormalizeGuid(request.ApproverPositionId),
                ApproverOrganizationUnitId = WorkflowMasterDataSupport.NormalizeGuid(request.ApproverOrganizationUnitId),
                SpecificApproverUserId = WorkflowMasterDataSupport.NormalizeGuid(request.SpecificApproverUserId),
                ApproverRoleCode = WorkflowMasterDataSupport.NormalizeNullableString(request.ApproverRoleCode),
                ManagerLevel = request.ManagerLevel,
                Priority = request.Priority,
                IsFallback = request.IsFallback,
                EffectiveStartDate = request.EffectiveStartDate?.Date,
                EffectiveEndDate = request.EffectiveEndDate?.Date,
                ConditionDefinitionJson = WorkflowMasterDataSupport.NormalizeNullableString(request.ConditionDefinitionJson),
                Description = WorkflowMasterDataSupport.NormalizeNullableString(request.Description),
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actor,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstApprovalMatrix>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var result = new ApprovalMatrixCreateResponse
            {
                Id = entity.Id,
                ApprovalMatrixCode = entity.ApprovalMatrixCode,
                ApprovalMatrixName = entity.ApprovalMatrixName,
                Priority = entity.Priority,
                IsActive = entity.IsActive
            };

            await _loggerService.InfoAsync(LogCategory, "ApprovalMatrix.CreateApprovalMatrix", "Membuat approval matrix.", result);
            return Ok(ApiResponse<ApprovalMatrixCreateResponse>.Ok(result, "Approval matrix berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Update", "Update Approval Matrix", Description = "Mengubah approval matrix", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("ApprovalMatrix", "Update")]
        public async Task<IActionResult> UpdateApprovalMatrix(Guid id, [FromBody] UpdateApprovalMatrixRequest request)
        {
            var entity = await _dbContext.Set<MstApprovalMatrix>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Approval matrix tidak ditemukan."));

            var validation = await ValidateRequestAsync(id, request);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage ?? "Data approval matrix tidak valid."));

            entity.WorkflowDefinitionId = request.WorkflowDefinitionId;
            entity.WorkflowStepId = request.WorkflowStepId;
            entity.ApprovalMatrixCode = WorkflowMasterDataSupport.NormalizeCode(request.ApprovalMatrixCode);
            entity.ApprovalMatrixName = request.ApprovalMatrixName.Trim();
            entity.LegalEntityId = WorkflowMasterDataSupport.NormalizeGuid(request.LegalEntityId);
            entity.HospitalSiteId = WorkflowMasterDataSupport.NormalizeGuid(request.HospitalSiteId);
            entity.OrganizationUnitId = WorkflowMasterDataSupport.NormalizeGuid(request.OrganizationUnitId);
            entity.DepartmentId = WorkflowMasterDataSupport.NormalizeGuid(request.DepartmentId);
            entity.RequesterPositionId = WorkflowMasterDataSupport.NormalizeGuid(request.RequesterPositionId);
            entity.EmployeeCategoryId = WorkflowMasterDataSupport.NormalizeGuid(request.EmployeeCategoryId);
            entity.EmploymentTypeId = WorkflowMasterDataSupport.NormalizeGuid(request.EmploymentTypeId);
            entity.MinimumAmount = request.MinimumAmount;
            entity.MaximumAmount = request.MaximumAmount;
            entity.CurrencyCode = string.IsNullOrWhiteSpace(request.CurrencyCode) ? "IDR" : request.CurrencyCode.Trim().ToUpperInvariant();
            entity.MinimumDurationHours = request.MinimumDurationHours;
            entity.MaximumDurationHours = request.MaximumDurationHours;
            entity.MinimumDurationDays = request.MinimumDurationDays;
            entity.MaximumDurationDays = request.MaximumDurationDays;
            entity.ApproverSourceType = CanonicalSource(request.ApproverSourceType);
            entity.ApproverPositionId = WorkflowMasterDataSupport.NormalizeGuid(request.ApproverPositionId);
            entity.ApproverOrganizationUnitId = WorkflowMasterDataSupport.NormalizeGuid(request.ApproverOrganizationUnitId);
            entity.SpecificApproverUserId = WorkflowMasterDataSupport.NormalizeGuid(request.SpecificApproverUserId);
            entity.ApproverRoleCode = WorkflowMasterDataSupport.NormalizeNullableString(request.ApproverRoleCode);
            entity.ManagerLevel = request.ManagerLevel;
            entity.Priority = request.Priority;
            entity.IsFallback = request.IsFallback;
            entity.EffectiveStartDate = request.EffectiveStartDate?.Date;
            entity.EffectiveEndDate = request.EffectiveEndDate?.Date;
            entity.ConditionDefinitionJson = WorkflowMasterDataSupport.NormalizeNullableString(request.ConditionDefinitionJson);
            entity.Description = WorkflowMasterDataSupport.NormalizeNullableString(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = WorkflowMasterDataSupport.GetCurrentUserId(User);
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(LogCategory, "ApprovalMatrix.UpdateApprovalMatrix", "Mengubah approval matrix.", new { entity.Id, entity.ApprovalMatrixCode, entity.ApprovalMatrixName, entity.Priority, entity.IsActive });
            return Ok(ApiResponse<object>.Ok(null, "Approval matrix berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Approval Matrix Status", Description = "Mengubah status approval matrix", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("ApprovalMatrix", "Update")]
        public async Task<IActionResult> UpdateApprovalMatrixStatus(Guid id, [FromBody] UpdateWorkflowMasterStatusRequest request)
        {
            var entity = await _dbContext.Set<MstApprovalMatrix>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Approval matrix tidak ditemukan."));

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = WorkflowMasterDataSupport.GetCurrentUserId(User);
            await _dbContext.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(null, "Status approval matrix berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Delete", "Delete Approval Matrix", Description = "Menghapus approval matrix", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("ApprovalMatrix", "Delete")]
        public async Task<IActionResult> DeleteApprovalMatrix(Guid id)
        {
            var entity = await _dbContext.Set<MstApprovalMatrix>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Approval matrix tidak ditemukan."));

            var isUsed =
                await _dbContext.Set<TrxWorkflowStepInstance>().AsNoTracking().AnyAsync(x => x.ApprovalMatrixId == id && !x.IsDelete) ||
                await _dbContext.Set<TrxWorkflowApproverAssignment>().AsNoTracking().AnyAsync(x => x.ApprovalMatrixId == id && !x.IsDelete);

            if (isUsed)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Approval matrix tidak dapat dihapus karena sudah digunakan oleh transaksi workflow."));

            var now = DateTime.UtcNow;
            var actor = WorkflowMasterDataSupport.GetCurrentUserId(User);
            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actor;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor;
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(LogCategory, "ApprovalMatrix.DeleteApprovalMatrix", "Menghapus approval matrix.", new { entity.Id, entity.ApprovalMatrixCode, entity.ApprovalMatrixName, entity.DeleteDateTime });
            return Ok(ApiResponse<object>.Ok(null, "Approval matrix berhasil dihapus."));
        }

        private IQueryable<MstApprovalMatrix> BuildBaseQuery() =>
            _dbContext.Set<MstApprovalMatrix>().AsNoTracking().Where(x => !x.IsDelete);

        private static IQueryable<MstApprovalMatrix> ApplyStandardFilter(
            IQueryable<MstApprovalMatrix> query,
            Guid? workflowDefinitionId,
            Guid? workflowStepId,
            Guid? legalEntityId,
            Guid? hospitalSiteId,
            Guid? organizationUnitId,
            Guid? departmentId,
            string? approverSourceType,
            bool? isFallback,
            bool? isActive,
            string? search)
        {
            if (workflowDefinitionId.HasValue && workflowDefinitionId.Value != Guid.Empty) query = query.Where(x => x.WorkflowDefinitionId == workflowDefinitionId.Value);
            if (workflowStepId.HasValue && workflowStepId.Value != Guid.Empty) query = query.Where(x => x.WorkflowStepId == workflowStepId.Value);
            if (legalEntityId.HasValue && legalEntityId.Value != Guid.Empty) query = query.Where(x => x.LegalEntityId == legalEntityId.Value);
            if (hospitalSiteId.HasValue && hospitalSiteId.Value != Guid.Empty) query = query.Where(x => x.HospitalSiteId == hospitalSiteId.Value);
            if (organizationUnitId.HasValue && organizationUnitId.Value != Guid.Empty) query = query.Where(x => x.OrganizationUnitId == organizationUnitId.Value);
            if (departmentId.HasValue && departmentId.Value != Guid.Empty) query = query.Where(x => x.DepartmentId == departmentId.Value);
            if (!string.IsNullOrWhiteSpace(approverSourceType)) query = query.Where(x => x.ApproverSourceType == approverSourceType.Trim());
            if (isFallback.HasValue) query = query.Where(x => x.IsFallback == isFallback.Value);
            if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.ApprovalMatrixCode.ToLower().Contains(keyword) ||
                    x.ApprovalMatrixName.ToLower().Contains(keyword) ||
                    x.ApproverSourceType.ToLower().Contains(keyword) ||
                    x.Description != null && x.Description.ToLower().Contains(keyword) ||
                    x.WorkflowDefinition != null && x.WorkflowDefinition.WorkflowName.ToLower().Contains(keyword) ||
                    x.WorkflowStep != null && x.WorkflowStep.StepName.ToLower().Contains(keyword));
            }

            return query;
        }

        private static IOrderedQueryable<MstApprovalMatrix> ApplySorting(IQueryable<MstApprovalMatrix> query, string? sortBy, string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "priority").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "approvalmatrixcode" => desc ? query.OrderByDescending(x => x.ApprovalMatrixCode) : query.OrderBy(x => x.ApprovalMatrixCode),
                "approvalmatrixname" => desc ? query.OrderByDescending(x => x.ApprovalMatrixName) : query.OrderBy(x => x.ApprovalMatrixName),
                "workflowname" => desc ? query.OrderByDescending(x => x.WorkflowDefinition != null ? x.WorkflowDefinition.WorkflowName : string.Empty) : query.OrderBy(x => x.WorkflowDefinition != null ? x.WorkflowDefinition.WorkflowName : string.Empty),
                "stepname" => desc ? query.OrderByDescending(x => x.WorkflowStep != null ? x.WorkflowStep.StepName : string.Empty) : query.OrderBy(x => x.WorkflowStep != null ? x.WorkflowStep.StepName : string.Empty),
                "isfallback" => desc ? query.OrderByDescending(x => x.IsFallback) : query.OrderBy(x => x.IsFallback),
                "isactive" => desc ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
                _ => desc ? query.OrderByDescending(x => x.Priority).ThenBy(x => x.ApprovalMatrixName) : query.OrderBy(x => x.Priority).ThenBy(x => x.ApprovalMatrixName)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(Guid? excludeId, CreateApprovalMatrixRequest request)
        {
            if (request.WorkflowDefinitionId == Guid.Empty) return (false, "Workflow definition wajib dipilih.");
            if (request.WorkflowStepId == Guid.Empty) return (false, "Workflow step wajib dipilih.");
            if (string.IsNullOrWhiteSpace(request.ApprovalMatrixCode)) return (false, "Kode approval matrix wajib diisi.");
            if (string.IsNullOrWhiteSpace(request.ApprovalMatrixName)) return (false, "Nama approval matrix wajib diisi.");
            if (!ApproverSources.Contains(request.ApproverSourceType)) return (false, "Approver source type tidak valid.");
            if (!WorkflowMasterDataSupport.IsEffectiveDateValid(request.EffectiveStartDate, request.EffectiveEndDate)) return (false, "Effective end date tidak boleh lebih kecil dari effective start date.");
            if (request.MinimumAmount.HasValue && request.MaximumAmount.HasValue && request.MinimumAmount.Value > request.MaximumAmount.Value) return (false, "Minimum amount tidak boleh lebih besar dari maximum amount.");
            if (request.MinimumDurationHours.HasValue && request.MaximumDurationHours.HasValue && request.MinimumDurationHours.Value > request.MaximumDurationHours.Value) return (false, "Minimum duration hours tidak boleh lebih besar dari maximum duration hours.");
            if (request.MinimumDurationDays.HasValue && request.MaximumDurationDays.HasValue && request.MinimumDurationDays.Value > request.MaximumDurationDays.Value) return (false, "Minimum duration days tidak boleh lebih besar dari maximum duration days.");
            if (!WorkflowMasterDataSupport.IsValidJson(request.ConditionDefinitionJson)) return (false, "Condition definition JSON tidak valid.");

            var step = await _dbContext.Set<MstWorkflowStep>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.WorkflowStepId && !x.IsDelete);
            if (step == null) return (false, "Workflow step tidak ditemukan.");
            if (step.WorkflowDefinitionId != request.WorkflowDefinitionId) return (false, "Workflow step tidak berasal dari workflow definition yang dipilih.");

            var definitionExists = await _dbContext.Set<MstWorkflowDefinition>().AsNoTracking().AnyAsync(x => x.Id == request.WorkflowDefinitionId && !x.IsDelete);
            if (!definitionExists) return (false, "Workflow definition tidak ditemukan.");

            var code = WorkflowMasterDataSupport.NormalizeCode(request.ApprovalMatrixCode);
            var duplicateQuery = _dbContext.Set<MstApprovalMatrix>().AsNoTracking().Where(x => !x.IsDelete && x.ApprovalMatrixCode == code);
            if (excludeId.HasValue) duplicateQuery = duplicateQuery.Where(x => x.Id != excludeId.Value);
            if (await duplicateQuery.AnyAsync()) return (false, "Kode approval matrix tersebut sudah digunakan.");

            var approverPositionId = WorkflowMasterDataSupport.NormalizeGuid(request.ApproverPositionId);
            var approverOrganizationUnitId = WorkflowMasterDataSupport.NormalizeGuid(request.ApproverOrganizationUnitId);
            var specificApproverUserId = WorkflowMasterDataSupport.NormalizeGuid(request.SpecificApproverUserId);

            if (string.Equals(request.ApproverSourceType, "Position", StringComparison.OrdinalIgnoreCase) && !approverPositionId.HasValue) return (false, "Approver position wajib dipilih untuk approver source Position.");
            if (string.Equals(request.ApproverSourceType, "OrganizationUnit", StringComparison.OrdinalIgnoreCase) && !approverOrganizationUnitId.HasValue) return (false, "Approver organization unit wajib dipilih untuk approver source OrganizationUnit.");
            if (string.Equals(request.ApproverSourceType, "SpecificUser", StringComparison.OrdinalIgnoreCase) && !specificApproverUserId.HasValue) return (false, "Specific approver user wajib dipilih untuk approver source SpecificUser.");
            if ((string.Equals(request.ApproverSourceType, "Role", StringComparison.OrdinalIgnoreCase) || string.Equals(request.ApproverSourceType, "SpecificRole", StringComparison.OrdinalIgnoreCase)) && string.IsNullOrWhiteSpace(request.ApproverRoleCode)) return (false, "Approver role code wajib diisi untuk approver source role.");
            if (string.Equals(request.ApproverSourceType, "ManagerLevel", StringComparison.OrdinalIgnoreCase) && (!request.ManagerLevel.HasValue || request.ManagerLevel.Value <= 0)) return (false, "Manager level wajib diisi untuk approver source ManagerLevel.");

            if (approverPositionId.HasValue && !await _dbContext.Set<MstPosition>().AsNoTracking().AnyAsync(x => x.Id == approverPositionId.Value && !x.IsDelete)) return (false, "Approver position tidak ditemukan.");
            if (approverOrganizationUnitId.HasValue && !await _dbContext.Set<MstOrganizationUnit>().AsNoTracking().AnyAsync(x => x.Id == approverOrganizationUnitId.Value && !x.IsDelete)) return (false, "Approver organization unit tidak ditemukan.");
            if (specificApproverUserId.HasValue && !await _dbContext.Users.AsNoTracking().AnyAsync(x => x.Id == specificApproverUserId.Value)) return (false, "Specific approver user tidak ditemukan.");

            var relatedValidation = await ValidateRelatedMasterIdsAsync(request);
            if (!relatedValidation.IsValid) return relatedValidation;

            return (true, null);
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRelatedMasterIdsAsync(CreateApprovalMatrixRequest request)
        {
            var legalEntityId = WorkflowMasterDataSupport.NormalizeGuid(request.LegalEntityId);
            var hospitalSiteId = WorkflowMasterDataSupport.NormalizeGuid(request.HospitalSiteId);
            var organizationUnitId = WorkflowMasterDataSupport.NormalizeGuid(request.OrganizationUnitId);
            var departmentId = WorkflowMasterDataSupport.NormalizeGuid(request.DepartmentId);
            var requesterPositionId = WorkflowMasterDataSupport.NormalizeGuid(request.RequesterPositionId);
            var employeeCategoryId = WorkflowMasterDataSupport.NormalizeGuid(request.EmployeeCategoryId);
            var employmentTypeId = WorkflowMasterDataSupport.NormalizeGuid(request.EmploymentTypeId);

            if (legalEntityId.HasValue && !await _dbContext.Set<MstLegalEntity>().AsNoTracking().AnyAsync(x => x.Id == legalEntityId.Value && !x.IsDelete)) return (false, "Legal entity tidak ditemukan.");
            if (hospitalSiteId.HasValue && !await _dbContext.Set<MstHospitalSite>().AsNoTracking().AnyAsync(x => x.Id == hospitalSiteId.Value && !x.IsDelete)) return (false, "Hospital site tidak ditemukan.");
            if (organizationUnitId.HasValue && !await _dbContext.Set<MstOrganizationUnit>().AsNoTracking().AnyAsync(x => x.Id == organizationUnitId.Value && !x.IsDelete)) return (false, "Organization unit tidak ditemukan.");
            if (departmentId.HasValue && !await _dbContext.Set<MstDepartment>().AsNoTracking().AnyAsync(x => x.Id == departmentId.Value && !x.IsDelete)) return (false, "Department tidak ditemukan.");
            if (requesterPositionId.HasValue && !await _dbContext.Set<MstPosition>().AsNoTracking().AnyAsync(x => x.Id == requesterPositionId.Value && !x.IsDelete)) return (false, "Requester position tidak ditemukan.");
            if (employeeCategoryId.HasValue && !await _dbContext.Set<MstEmployeeCategory>().AsNoTracking().AnyAsync(x => x.Id == employeeCategoryId.Value && !x.IsDelete)) return (false, "Employee category tidak ditemukan.");
            if (employmentTypeId.HasValue && !await _dbContext.Set<MstEmploymentType>().AsNoTracking().AnyAsync(x => x.Id == employmentTypeId.Value && !x.IsDelete)) return (false, "Employment type tidak ditemukan.");

            return (true, null);
        }

        private static string CanonicalSource(string value) => ApproverSources.First(x => string.Equals(x, value.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}
