using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

using ResponseWorkflowDefinitionPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.DTOs.WorkflowDefinitionResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/workflow-definitions")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Workflow Definition",
        AreaName = "Corporate",
        ControllerName = "WorkflowDefinition",
        Description = "Corporate human resource master data workflow definition",
        SortOrder = 70)]
    [Tags("Corporate / Human Resource / Master Data / Workflow Definition")]
    public class WorkflowDefinitionController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.MasterData";

        private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "Draft", "Active", "Inactive", "Retired"
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public WorkflowDefinitionController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<WorkflowDefinitionFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workflow Definition", Description = "Melihat metadata filter workflow definition", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkflowDefinition", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new WorkflowDefinitionFilterMetadataResponse
            {
                DefaultFilter = new WorkflowDefinitionDefaultFilterResponse(),
                CustomPeriods = WorkflowMasterDataSupport.BuildPeriodOptions(),
                SortOptions = new List<WorkflowMasterSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "workflowCode", Label = "Kode workflow" },
                    new() { Value = "workflowName", Label = "Nama workflow" },
                    new() { Value = "requestType", Label = "Tipe permintaan" },
                    new() { Value = "version", Label = "Versi" },
                    new() { Value = "workflowStatus", Label = "Status workflow" },
                    new() { Value = "isDefault", Label = "Workflow default" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                WorkflowStatuses = AllowedStatuses
                    .OrderBy(x => x)
                    .Select(x => new WorkflowMasterLookupOptionResponse { Value = x, Label = x })
                    .ToList(),
                WorkflowCategories = new List<WorkflowMasterLookupOptionResponse>
                {
                    new() { Value = "HumanResource", Label = "Human Resource" }
                }
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkflowDefinition.GetFilterMetadata",
                "Mengambil metadata filter workflow definition.",
                result);

            return Ok(ApiResponse<WorkflowDefinitionFilterMetadataResponse>.Ok(
                result,
                "Metadata filter workflow definition berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<WorkflowDefinitionSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workflow Definition", Description = "Melihat ringkasan workflow definition", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkflowDefinition", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = BuildBaseQuery();
            var result = new WorkflowDefinitionSummaryResponse
            {
                TotalWorkflowDefinition = await query.CountAsync(),
                ActiveWorkflowDefinition = await query.CountAsync(x => x.IsActive),
                InactiveWorkflowDefinition = await query.CountAsync(x => !x.IsActive),
                DraftWorkflowDefinition = await query.CountAsync(x => x.WorkflowStatus == "Draft"),
                PublishedWorkflowDefinition = await query.CountAsync(x => x.WorkflowStatus == "Active"),
                RetiredWorkflowDefinition = await query.CountAsync(x => x.WorkflowStatus == "Retired"),
                DefaultWorkflowDefinition = await query.CountAsync(x => x.IsDefault),
                GlobalWorkflowDefinition = await query.CountAsync(x =>
                    x.LegalEntityId == null &&
                    x.HospitalSiteId == null &&
                    x.OrganizationUnitId == null),
                ScopedWorkflowDefinition = await query.CountAsync(x =>
                    x.LegalEntityId != null ||
                    x.HospitalSiteId != null ||
                    x.OrganizationUnitId != null)
            };

            return Ok(ApiResponse<WorkflowDefinitionSummaryResponse>.Ok(
                result,
                "Ringkasan workflow definition berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseWorkflowDefinitionPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workflow Definition", Description = "Melihat data workflow definition", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkflowDefinition", "Read")]
        public async Task<IActionResult> GetWorkflowDefinitions(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? legalEntityId,
            [FromQuery] Guid? hospitalSiteId,
            [FromQuery] Guid? organizationUnitId,
            [FromQuery] string? requestType,
            [FromQuery] string? workflowCategory,
            [FromQuery] string? workflowStatus,
            [FromQuery] bool? isDefault,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "workflowName",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = WorkflowMasterDataSupport.NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = WorkflowMasterDataSupport.ApplyDateFilter(
                BuildBaseQuery(),
                startDate,
                endDate,
                customPeriod);

            query = ApplyStandardFilter(
                query,
                legalEntityId,
                hospitalSiteId,
                organizationUnitId,
                requestType,
                workflowCategory,
                workflowStatus,
                isDefault,
                isActive,
                search);

            var totalData = await query.CountAsync();
            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new WorkflowDefinitionResponse
                {
                    Id = x.Id,
                    LegalEntityId = x.LegalEntityId,
                    HospitalSiteId = x.HospitalSiteId,
                    OrganizationUnitId = x.OrganizationUnitId,
                    WorkflowCode = x.WorkflowCode,
                    WorkflowName = x.WorkflowName,
                    RequestType = x.RequestType,
                    WorkflowCategory = x.WorkflowCategory,
                    Version = x.Version,
                    WorkflowStatus = x.WorkflowStatus,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    AllowRequesterCancel = x.AllowRequesterCancel,
                    AllowRequesterWithdraw = x.AllowRequesterWithdraw,
                    AllowParallelApproval = x.AllowParallelApproval,
                    AllowStepSkip = x.AllowStepSkip,
                    StopOnRejection = x.StopOnRejection,
                    IsDefault = x.IsDefault,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    WorkflowStepCount = _dbContext.Set<MstWorkflowStep>().Count(y => y.WorkflowDefinitionId == x.Id && !y.IsDelete),
                    ApprovalMatrixCount = _dbContext.Set<MstApprovalMatrix>().Count(y => y.WorkflowDefinitionId == x.Id && !y.IsDelete),
                    RequestReasonCount = _dbContext.Set<MstRequestReason>().Count(y => y.WorkflowDefinitionId == x.Id && !y.IsDelete),
                    RejectionReasonCount = _dbContext.Set<MstRejectionReason>().Count(y => y.WorkflowDefinitionId == x.Id && !y.IsDelete),
                    DelegationPolicyCount = _dbContext.Set<MstApprovalDelegationPolicy>().Count(y => y.WorkflowDefinitionId == x.Id && !y.IsDelete),
                    WorkflowInstanceCount = _dbContext.Set<TrxWorkflowInstance>().Count(y => y.WorkflowDefinitionId == x.Id && !y.IsDelete),
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
                .ToListAsync();

            return Ok(ApiResponse<ResponseWorkflowDefinitionPagedResult>.Ok(
                new ResponseWorkflowDefinitionPagedResult
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = items
                },
                "Data workflow definition berhasil diambil."));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<WorkflowDefinitionOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workflow Definition", Description = "Melihat pilihan workflow definition", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkflowDefinition", "Read")]
        public async Task<IActionResult> GetWorkflowDefinitionOptions(
            [FromQuery] string? requestType,
            [FromQuery] string? workflowStatus,
            [FromQuery] Guid? legalEntityId,
            [FromQuery] Guid? hospitalSiteId,
            [FromQuery] Guid? organizationUnitId,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = WorkflowMasterDataSupport.NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = ApplyStandardFilter(
                BuildBaseQuery(),
                legalEntityId,
                hospitalSiteId,
                organizationUnitId,
                requestType,
                null,
                workflowStatus,
                null,
                onlyActive ? true : null,
                search);

            var totalData = await query.CountAsync();
            var items = await query
                .OrderBy(x => x.WorkflowName)
                .ThenByDescending(x => x.Version)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new WorkflowDefinitionOptionResponse
                {
                    Id = x.Id,
                    WorkflowCode = x.WorkflowCode,
                    WorkflowName = x.WorkflowName,
                    RequestType = x.RequestType,
                    Version = x.Version,
                    WorkflowStatus = x.WorkflowStatus,
                    IsDefault = x.IsDefault,
                    LegalEntityId = x.LegalEntityId,
                    HospitalSiteId = x.HospitalSiteId,
                    OrganizationUnitId = x.OrganizationUnitId
                })
                .ToListAsync();

            return Ok(ApiResponse<WorkflowDefinitionOptionPagedResponse>.Ok(
                new WorkflowDefinitionOptionPagedResponse
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = items
                },
                "Data pilihan workflow definition berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkflowDefinitionDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workflow Definition", Description = "Melihat detail workflow definition", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkflowDefinition", "Read")]
        public async Task<IActionResult> GetWorkflowDefinitionById(Guid id)
        {
            var data = await BuildDetailQuery(id).FirstOrDefaultAsync();
            if (data == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workflow definition tidak ditemukan."));
            }

            return Ok(ApiResponse<WorkflowDefinitionDetailResponse>.Ok(
                data,
                "Detail workflow definition berhasil diambil."));
        }

        [HttpGet("{id:guid}/structure")]
        [ProducesResponseType(typeof(ApiResponse<WorkflowDefinitionStructureResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workflow Definition Structure", Description = "Melihat struktur lengkap workflow definition", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkflowDefinition", "Read")]
        public async Task<IActionResult> GetWorkflowDefinitionStructure(Guid id)
        {
            var definition = await BuildDetailQuery(id).FirstOrDefaultAsync();
            if (definition == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workflow definition tidak ditemukan."));
            }

            var steps = await _dbContext.Set<MstWorkflowStep>()
                .AsNoTracking()
                .Where(x => x.WorkflowDefinitionId == id && !x.IsDelete)
                .OrderBy(x => x.StepOrder)
                .ThenBy(x => x.StepCode)
                .Select(x => new WorkflowStepResponse
                {
                    Id = x.Id,
                    WorkflowDefinitionId = x.WorkflowDefinitionId,
                    WorkflowCode = x.WorkflowDefinition != null ? x.WorkflowDefinition.WorkflowCode : null,
                    WorkflowName = x.WorkflowDefinition != null ? x.WorkflowDefinition.WorkflowName : null,
                    WorkflowVersion = x.WorkflowDefinition != null ? x.WorkflowDefinition.Version : 0,
                    StepCode = x.StepCode,
                    StepName = x.StepName,
                    StepOrder = x.StepOrder,
                    StepType = x.StepType,
                    ApprovalMode = x.ApprovalMode,
                    RequiredApprovalCount = x.RequiredApprovalCount,
                    RequiredApprovalPercentage = x.RequiredApprovalPercentage,
                    ApproverSourceType = x.ApproverSourceType,
                    ApproverPositionId = x.ApproverPositionId,
                    ApproverOrganizationUnitId = x.ApproverOrganizationUnitId,
                    SpecificApproverUserId = x.SpecificApproverUserId,
                    ApproverRoleCode = x.ApproverRoleCode,
                    ManagerLevel = x.ManagerLevel,
                    IsRequired = x.IsRequired,
                    IsParallel = x.IsParallel,
                    AllowDelegation = x.AllowDelegation,
                    AllowSelfApproval = x.AllowSelfApproval,
                    ReminderAfterHours = x.ReminderAfterHours,
                    EscalationAfterHours = x.EscalationAfterHours,
                    AutoApproveAfterHours = x.AutoApproveAfterHours,
                    AutoRejectAfterHours = x.AutoRejectAfterHours,
                    OnApproveNextStepCode = x.OnApproveNextStepCode,
                    OnRejectStepCode = x.OnRejectStepCode,
                    Instructions = x.Instructions,
                    IsActive = x.IsActive,
                    ApprovalMatrixCount = _dbContext.Set<MstApprovalMatrix>().Count(y => y.WorkflowStepId == x.Id && !y.IsDelete),
                    RejectionReasonCount = _dbContext.Set<MstRejectionReason>().Count(y => y.WorkflowStepId == x.Id && !y.IsDelete),
                    DelegationPolicyCount = _dbContext.Set<MstApprovalDelegationPolicy>().Count(y => y.WorkflowStepId == x.Id && !y.IsDelete),
                    StepInstanceCount = _dbContext.Set<TrxWorkflowStepInstance>().Count(y => y.WorkflowStepId == x.Id && !y.IsDelete),
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                    UpdateDateTime = x.UpdateDateTime,
                    UpdateBy = x.UpdateBy == Guid.Empty ? null : x.UpdateBy
                })
                .ToListAsync();

            var matrices = await _dbContext.Set<MstApprovalMatrix>()
                .AsNoTracking()
                .Where(x => x.WorkflowDefinitionId == id && !x.IsDelete)
                .OrderByDescending(x => x.Priority)
                .ThenBy(x => x.ApprovalMatrixName)
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
                    ApproverRoleCode = x.ApproverRoleCode,
                    ManagerLevel = x.ManagerLevel,
                    Priority = x.Priority,
                    IsFallback = x.IsFallback,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    ConditionDefinitionJson = x.ConditionDefinitionJson,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                    UpdateDateTime = x.UpdateDateTime,
                    UpdateBy = x.UpdateBy == Guid.Empty ? null : x.UpdateBy
                })
                .ToListAsync();

            var requestReasons = await _dbContext.Set<MstRequestReason>()
                .AsNoTracking()
                .Where(x => x.WorkflowDefinitionId == id && !x.IsDelete)
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.ReasonName)
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
                    UpdateDateTime = x.UpdateDateTime,
                    UpdateBy = x.UpdateBy == Guid.Empty ? null : x.UpdateBy
                })
                .ToListAsync();

            var rejectionReasons = await _dbContext.Set<MstRejectionReason>()
                .AsNoTracking()
                .Where(x => x.WorkflowDefinitionId == id && !x.IsDelete)
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.ReasonName)
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
                    UpdateDateTime = x.UpdateDateTime,
                    UpdateBy = x.UpdateBy == Guid.Empty ? null : x.UpdateBy
                })
                .ToListAsync();

            var policies = await _dbContext.Set<MstApprovalDelegationPolicy>()
                .AsNoTracking()
                .Where(x => x.WorkflowDefinitionId == id && !x.IsDelete)
                .OrderBy(x => x.DelegationPolicyName)
                .Select(x => new ApprovalDelegationPolicyResponse
                {
                    Id = x.Id,
                    WorkflowDefinitionId = x.WorkflowDefinitionId,
                    WorkflowCode = x.WorkflowDefinition != null ? x.WorkflowDefinition.WorkflowCode : null,
                    WorkflowStepId = x.WorkflowStepId,
                    WorkflowStepCode = x.WorkflowStep != null ? x.WorkflowStep.StepCode : null,
                    LegalEntityId = x.LegalEntityId,
                    HospitalSiteId = x.HospitalSiteId,
                    OrganizationUnitId = x.OrganizationUnitId,
                    DelegationPolicyCode = x.DelegationPolicyCode,
                    DelegationPolicyName = x.DelegationPolicyName,
                    DelegationType = x.DelegationType,
                    MaximumDelegationDays = x.MaximumDelegationDays,
                    MinimumNoticeHours = x.MinimumNoticeHours,
                    RequireManagerApproval = x.RequireManagerApproval,
                    RequireHrVerification = x.RequireHrVerification,
                    AllowCrossOrganizationUnit = x.AllowCrossOrganizationUnit,
                    AllowCrossHospitalSite = x.AllowCrossHospitalSite,
                    AllowCrossLegalEntity = x.AllowCrossLegalEntity,
                    AllowSubDelegation = x.AllowSubDelegation,
                    AllowSelfDelegation = x.AllowSelfDelegation,
                    PreserveDelegatorAccountability = x.PreserveDelegatorAccountability,
                    ApprovalWorkflowCode = x.ApprovalWorkflowCode,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    ApprovalDelegationCount = _dbContext.Set<TrxApprovalDelegation>().Count(y => y.ApprovalDelegationPolicyId == x.Id && !y.IsDelete),
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                    UpdateDateTime = x.UpdateDateTime,
                    UpdateBy = x.UpdateBy == Guid.Empty ? null : x.UpdateBy
                })
                .ToListAsync();

            var result = new WorkflowDefinitionStructureResponse
            {
                Definition = definition,
                Steps = steps,
                ApprovalMatrices = matrices,
                RequestReasons = requestReasons,
                RejectionReasons = rejectionReasons,
                DelegationPolicies = policies
            };

            return Ok(ApiResponse<WorkflowDefinitionStructureResponse>.Ok(
                result,
                "Struktur workflow definition berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<WorkflowDefinitionCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Workflow Definition", Description = "Membuat workflow definition", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("WorkflowDefinition", "Create")]
        public async Task<IActionResult> CreateWorkflowDefinition(
            [FromBody] CreateWorkflowDefinitionRequest request)
        {
            var validation = await ValidateRequestAsync(null, request);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data workflow definition tidak valid."));
            }

            var actor = WorkflowMasterDataSupport.GetCurrentUserId(User);
            var entity = new MstWorkflowDefinition
            {
                Id = Guid.NewGuid(),
                LegalEntityId = WorkflowMasterDataSupport.NormalizeGuid(request.LegalEntityId),
                HospitalSiteId = WorkflowMasterDataSupport.NormalizeGuid(request.HospitalSiteId),
                OrganizationUnitId = WorkflowMasterDataSupport.NormalizeGuid(request.OrganizationUnitId),
                WorkflowCode = WorkflowMasterDataSupport.NormalizeCode(request.WorkflowCode),
                WorkflowName = request.WorkflowName.Trim(),
                RequestType = request.RequestType.Trim(),
                WorkflowCategory = request.WorkflowCategory.Trim(),
                Version = request.Version,
                WorkflowStatus = CanonicalStatus(request.WorkflowStatus),
                EffectiveStartDate = request.EffectiveStartDate?.Date,
                EffectiveEndDate = request.EffectiveEndDate?.Date,
                AllowRequesterCancel = request.AllowRequesterCancel,
                AllowRequesterWithdraw = request.AllowRequesterWithdraw,
                AllowParallelApproval = request.AllowParallelApproval,
                AllowStepSkip = request.AllowStepSkip,
                StopOnRejection = request.StopOnRejection,
                IsDefault = request.IsDefault,
                Description = WorkflowMasterDataSupport.NormalizeNullableString(request.Description),
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actor,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstWorkflowDefinition>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var result = new WorkflowDefinitionCreateResponse
            {
                Id = entity.Id,
                WorkflowCode = entity.WorkflowCode,
                WorkflowName = entity.WorkflowName,
                Version = entity.Version,
                WorkflowStatus = entity.WorkflowStatus,
                IsActive = entity.IsActive
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkflowDefinition.CreateWorkflowDefinition",
                "Membuat workflow definition.",
                result);

            return Ok(ApiResponse<WorkflowDefinitionCreateResponse>.Ok(
                result,
                "Workflow definition berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Update", "Update Workflow Definition", Description = "Mengubah workflow definition", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkflowDefinition", "Update")]
        public async Task<IActionResult> UpdateWorkflowDefinition(
            Guid id,
            [FromBody] UpdateWorkflowDefinitionRequest request)
        {
            var entity = await _dbContext.Set<MstWorkflowDefinition>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workflow definition tidak ditemukan."));
            }

            var validation = await ValidateRequestAsync(id, request);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data workflow definition tidak valid."));
            }

            entity.LegalEntityId = WorkflowMasterDataSupport.NormalizeGuid(request.LegalEntityId);
            entity.HospitalSiteId = WorkflowMasterDataSupport.NormalizeGuid(request.HospitalSiteId);
            entity.OrganizationUnitId = WorkflowMasterDataSupport.NormalizeGuid(request.OrganizationUnitId);
            entity.WorkflowCode = WorkflowMasterDataSupport.NormalizeCode(request.WorkflowCode);
            entity.WorkflowName = request.WorkflowName.Trim();
            entity.RequestType = request.RequestType.Trim();
            entity.WorkflowCategory = request.WorkflowCategory.Trim();
            entity.Version = request.Version;
            entity.WorkflowStatus = CanonicalStatus(request.WorkflowStatus);
            entity.EffectiveStartDate = request.EffectiveStartDate?.Date;
            entity.EffectiveEndDate = request.EffectiveEndDate?.Date;
            entity.AllowRequesterCancel = request.AllowRequesterCancel;
            entity.AllowRequesterWithdraw = request.AllowRequesterWithdraw;
            entity.AllowParallelApproval = request.AllowParallelApproval;
            entity.AllowStepSkip = request.AllowStepSkip;
            entity.StopOnRejection = request.StopOnRejection;
            entity.IsDefault = request.IsDefault;
            entity.Description = WorkflowMasterDataSupport.NormalizeNullableString(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = WorkflowMasterDataSupport.GetCurrentUserId(User);

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkflowDefinition.UpdateWorkflowDefinition",
                "Mengubah workflow definition.",
                new
                {
                    entity.Id,
                    entity.WorkflowCode,
                    entity.WorkflowName,
                    entity.Version,
                    entity.WorkflowStatus,
                    entity.IsActive
                });

            return Ok(ApiResponse<object>.Ok(
                null,
                "Workflow definition berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Update", "Update Workflow Definition Status", Description = "Mengubah status workflow definition", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("WorkflowDefinition", "Update")]
        public async Task<IActionResult> UpdateWorkflowDefinitionStatus(
            Guid id,
            [FromBody] UpdateWorkflowDefinitionStatusRequest request)
        {
            if (!AllowedStatuses.Contains(request.WorkflowStatus))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Workflow status tidak valid."));
            }

            var entity = await _dbContext.Set<MstWorkflowDefinition>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workflow definition tidak ditemukan."));
            }

            if (string.Equals(request.WorkflowStatus, "Active", StringComparison.OrdinalIgnoreCase))
            {
                var hasActiveStep = await _dbContext.Set<MstWorkflowStep>()
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.WorkflowDefinitionId == id &&
                        x.IsActive &&
                        !x.IsDelete);

                if (!hasActiveStep)
                {
                    return BadRequest(ApiResponse<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        "Workflow definition tidak dapat diaktifkan karena belum memiliki workflow step aktif."));
                }
            }

            entity.WorkflowStatus = CanonicalStatus(request.WorkflowStatus);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = WorkflowMasterDataSupport.GetCurrentUserId(User);
            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status workflow definition berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Delete", "Delete Workflow Definition", Description = "Menghapus workflow definition", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("WorkflowDefinition", "Delete")]
        public async Task<IActionResult> DeleteWorkflowDefinition(Guid id)
        {
            var entity = await _dbContext.Set<MstWorkflowDefinition>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workflow definition tidak ditemukan."));
            }

            var isUsed =
                await _dbContext.Set<MstWorkflowStep>().AsNoTracking().AnyAsync(x => x.WorkflowDefinitionId == id && !x.IsDelete) ||
                await _dbContext.Set<MstApprovalMatrix>().AsNoTracking().AnyAsync(x => x.WorkflowDefinitionId == id && !x.IsDelete) ||
                await _dbContext.Set<MstApprovalDelegationPolicy>().AsNoTracking().AnyAsync(x => x.WorkflowDefinitionId == id && !x.IsDelete) ||
                await _dbContext.Set<MstRequestReason>().AsNoTracking().AnyAsync(x => x.WorkflowDefinitionId == id && !x.IsDelete) ||
                await _dbContext.Set<MstRejectionReason>().AsNoTracking().AnyAsync(x => x.WorkflowDefinitionId == id && !x.IsDelete) ||
                await _dbContext.Set<TrxWorkflowInstance>().AsNoTracking().AnyAsync(x => x.WorkflowDefinitionId == id && !x.IsDelete) ||
                await _dbContext.Set<TrxApprovalDelegation>().AsNoTracking().AnyAsync(x => x.WorkflowDefinitionId == id && !x.IsDelete);

            if (isUsed)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Workflow definition tidak dapat dihapus karena sudah memiliki konfigurasi turunan atau transaksi workflow."));
            }

            var now = DateTime.UtcNow;
            var actor = WorkflowMasterDataSupport.GetCurrentUserId(User);
            entity.IsDelete = true;
            entity.IsActive = false;
            entity.WorkflowStatus = "Inactive";
            entity.DeleteDateTime = now;
            entity.DeleteBy = actor;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor;
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkflowDefinition.DeleteWorkflowDefinition",
                "Menghapus workflow definition.",
                new
                {
                    entity.Id,
                    entity.WorkflowCode,
                    entity.WorkflowName,
                    entity.DeleteDateTime
                });

            return Ok(ApiResponse<object>.Ok(
                null,
                "Workflow definition berhasil dihapus."));
        }

        private IQueryable<MstWorkflowDefinition> BuildBaseQuery() =>
            _dbContext.Set<MstWorkflowDefinition>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

        private IQueryable<WorkflowDefinitionDetailResponse> BuildDetailQuery(Guid id) =>
            BuildBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => new WorkflowDefinitionDetailResponse
                {
                    Id = x.Id,
                    LegalEntityId = x.LegalEntityId,
                    HospitalSiteId = x.HospitalSiteId,
                    OrganizationUnitId = x.OrganizationUnitId,
                    WorkflowCode = x.WorkflowCode,
                    WorkflowName = x.WorkflowName,
                    RequestType = x.RequestType,
                    WorkflowCategory = x.WorkflowCategory,
                    Version = x.Version,
                    WorkflowStatus = x.WorkflowStatus,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    AllowRequesterCancel = x.AllowRequesterCancel,
                    AllowRequesterWithdraw = x.AllowRequesterWithdraw,
                    AllowParallelApproval = x.AllowParallelApproval,
                    AllowStepSkip = x.AllowStepSkip,
                    StopOnRejection = x.StopOnRejection,
                    IsDefault = x.IsDefault,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    WorkflowStepCount = _dbContext.Set<MstWorkflowStep>().Count(y => y.WorkflowDefinitionId == x.Id && !y.IsDelete),
                    ApprovalMatrixCount = _dbContext.Set<MstApprovalMatrix>().Count(y => y.WorkflowDefinitionId == x.Id && !y.IsDelete),
                    RequestReasonCount = _dbContext.Set<MstRequestReason>().Count(y => y.WorkflowDefinitionId == x.Id && !y.IsDelete),
                    RejectionReasonCount = _dbContext.Set<MstRejectionReason>().Count(y => y.WorkflowDefinitionId == x.Id && !y.IsDelete),
                    DelegationPolicyCount = _dbContext.Set<MstApprovalDelegationPolicy>().Count(y => y.WorkflowDefinitionId == x.Id && !y.IsDelete),
                    WorkflowInstanceCount = _dbContext.Set<TrxWorkflowInstance>().Count(y => y.WorkflowDefinitionId == x.Id && !y.IsDelete),
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
                });

        private static IQueryable<MstWorkflowDefinition> ApplyStandardFilter(
            IQueryable<MstWorkflowDefinition> query,
            Guid? legalEntityId,
            Guid? hospitalSiteId,
            Guid? organizationUnitId,
            string? requestType,
            string? workflowCategory,
            string? workflowStatus,
            bool? isDefault,
            bool? isActive,
            string? search)
        {
            if (legalEntityId.HasValue && legalEntityId.Value != Guid.Empty)
                query = query.Where(x => x.LegalEntityId == legalEntityId.Value);
            if (hospitalSiteId.HasValue && hospitalSiteId.Value != Guid.Empty)
                query = query.Where(x => x.HospitalSiteId == hospitalSiteId.Value);
            if (organizationUnitId.HasValue && organizationUnitId.Value != Guid.Empty)
                query = query.Where(x => x.OrganizationUnitId == organizationUnitId.Value);
            if (!string.IsNullOrWhiteSpace(requestType))
                query = query.Where(x => x.RequestType == requestType.Trim());
            if (!string.IsNullOrWhiteSpace(workflowCategory))
                query = query.Where(x => x.WorkflowCategory == workflowCategory.Trim());
            if (!string.IsNullOrWhiteSpace(workflowStatus))
                query = query.Where(x => x.WorkflowStatus == workflowStatus.Trim());
            if (isDefault.HasValue)
                query = query.Where(x => x.IsDefault == isDefault.Value);
            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.WorkflowCode.ToLower().Contains(keyword) ||
                    x.WorkflowName.ToLower().Contains(keyword) ||
                    x.RequestType.ToLower().Contains(keyword) ||
                    x.WorkflowCategory.ToLower().Contains(keyword) ||
                    x.Description != null && x.Description.ToLower().Contains(keyword));
            }

            return query;
        }

        private static IOrderedQueryable<MstWorkflowDefinition> ApplySorting(
            IQueryable<MstWorkflowDefinition> query,
            string? sortBy,
            string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "workflowName").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "workflowcode" => desc ? query.OrderByDescending(x => x.WorkflowCode) : query.OrderBy(x => x.WorkflowCode),
                "requesttype" => desc ? query.OrderByDescending(x => x.RequestType) : query.OrderBy(x => x.RequestType),
                "version" => desc ? query.OrderByDescending(x => x.Version) : query.OrderBy(x => x.Version),
                "workflowstatus" => desc ? query.OrderByDescending(x => x.WorkflowStatus) : query.OrderBy(x => x.WorkflowStatus),
                "isdefault" => desc ? query.OrderByDescending(x => x.IsDefault) : query.OrderBy(x => x.IsDefault),
                "isactive" => desc ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
                _ => desc
                    ? query.OrderByDescending(x => x.WorkflowName).ThenByDescending(x => x.Version)
                    : query.OrderBy(x => x.WorkflowName).ThenByDescending(x => x.Version)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            CreateWorkflowDefinitionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.WorkflowCode))
                return (false, "Kode workflow wajib diisi.");
            if (string.IsNullOrWhiteSpace(request.WorkflowName))
                return (false, "Nama workflow wajib diisi.");
            if (string.IsNullOrWhiteSpace(request.RequestType))
                return (false, "Request type wajib diisi.");
            if (string.IsNullOrWhiteSpace(request.WorkflowCategory))
                return (false, "Workflow category wajib diisi.");
            if (request.Version <= 0)
                return (false, "Version harus lebih besar dari 0.");
            if (!AllowedStatuses.Contains(request.WorkflowStatus))
                return (false, "Workflow status tidak valid.");
            if (!WorkflowMasterDataSupport.IsEffectiveDateValid(
                    request.EffectiveStartDate,
                    request.EffectiveEndDate))
                return (false, "Effective end date tidak boleh lebih kecil dari effective start date.");

            var legalEntityId = WorkflowMasterDataSupport.NormalizeGuid(request.LegalEntityId);
            var hospitalSiteId = WorkflowMasterDataSupport.NormalizeGuid(request.HospitalSiteId);
            var organizationUnitId = WorkflowMasterDataSupport.NormalizeGuid(request.OrganizationUnitId);

            if (legalEntityId.HasValue &&
                !await _dbContext.Set<MstLegalEntity>().AsNoTracking().AnyAsync(x => x.Id == legalEntityId.Value && !x.IsDelete))
                return (false, "Legal entity tidak ditemukan.");

            if (hospitalSiteId.HasValue &&
                !await _dbContext.Set<MstHospitalSite>().AsNoTracking().AnyAsync(x => x.Id == hospitalSiteId.Value && !x.IsDelete))
                return (false, "Hospital site tidak ditemukan.");

            if (organizationUnitId.HasValue &&
                !await _dbContext.Set<MstOrganizationUnit>().AsNoTracking().AnyAsync(x => x.Id == organizationUnitId.Value && !x.IsDelete))
                return (false, "Organization unit tidak ditemukan.");

            var workflowCode = WorkflowMasterDataSupport.NormalizeCode(request.WorkflowCode);
            var duplicateQuery = _dbContext.Set<MstWorkflowDefinition>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.WorkflowCode == workflowCode &&
                    x.Version == request.Version);

            if (excludeId.HasValue)
                duplicateQuery = duplicateQuery.Where(x => x.Id != excludeId.Value);

            if (await duplicateQuery.AnyAsync())
                return (false, "Kode workflow dan version tersebut sudah digunakan.");

            if (request.IsDefault)
            {
                var requestType = request.RequestType.Trim().ToLower();
                var defaultQuery = _dbContext.Set<MstWorkflowDefinition>()
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDelete &&
                        x.IsActive &&
                        x.IsDefault &&
                        x.RequestType.ToLower() == requestType &&
                        x.LegalEntityId == legalEntityId &&
                        x.HospitalSiteId == hospitalSiteId &&
                        x.OrganizationUnitId == organizationUnitId);

                if (excludeId.HasValue)
                    defaultQuery = defaultQuery.Where(x => x.Id != excludeId.Value);

                if (await defaultQuery.AnyAsync())
                {
                    return (false, "Sudah ada workflow default aktif untuk request type dan scope organisasi yang sama.");
                }
            }

            return (true, null);
        }

        private static string CanonicalStatus(string value) =>
            AllowedStatuses.First(x => string.Equals(x, value.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}
