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

using ResponseWorkflowStepPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.DTOs.WorkflowStepResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/workflow-steps")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Workflow Step",
        AreaName = "Corporate",
        ControllerName = "WorkflowStep",
        Description = "Corporate human resource master data workflow step",
        SortOrder = 71)]
    [Tags("Corporate / Human Resource / Master Data / Workflow Step")]
    public class WorkflowStepController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.MasterData";

        private static readonly HashSet<string> StepTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "Approval", "Review", "Verification", "Notification", "SystemAction"
        };

        private static readonly HashSet<string> ApprovalModes = new(StringComparer.OrdinalIgnoreCase)
        {
            "Any", "All", "Sequential", "Percentage"
        };

        private static readonly HashSet<string> ApproverSources = new(StringComparer.OrdinalIgnoreCase)
        {
            "RequesterManager", "DirectManager", "ManagerLevel", "Position",
            "OrganizationUnit", "Role", "SpecificRole", "SpecificUser",
            "ApprovalMatrix", "RequesterSelected", "OrganizationHead",
            "DepartmentHead", "SiteHr", "CorporateHr", "PayrollOfficer",
            "FinanceOfficer", "CostCenterOwner", "CredentialingCommittee"
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public WorkflowStepController(ApplicationDbContext dbContext, LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<WorkflowStepFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workflow Step", Description = "Melihat metadata filter workflow step", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkflowStep", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new WorkflowStepFilterMetadataResponse
            {
                DefaultFilter = new WorkflowStepDefaultFilterResponse(),
                CustomPeriods = WorkflowMasterDataSupport.BuildPeriodOptions(),
                SortOptions = new List<WorkflowMasterSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "workflowName", Label = "Nama workflow" },
                    new() { Value = "stepCode", Label = "Kode step" },
                    new() { Value = "stepName", Label = "Nama step" },
                    new() { Value = "stepOrder", Label = "Urutan step" },
                    new() { Value = "stepType", Label = "Tipe step" },
                    new() { Value = "approvalMode", Label = "Mode approval" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                StepTypes = StepTypes.OrderBy(x => x).Select(ToLookup).ToList(),
                ApprovalModes = ApprovalModes.OrderBy(x => x).Select(ToLookup).ToList(),
                ApproverSources = ApproverSources.OrderBy(x => x).Select(ToLookup).ToList()
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkflowStep.GetFilterMetadata",
                "Mengambil metadata filter workflow step.",
                result);

            return Ok(ApiResponse<WorkflowStepFilterMetadataResponse>.Ok(
                result,
                "Metadata filter workflow step berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<WorkflowStepSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workflow Step", Description = "Melihat ringkasan workflow step", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkflowStep", "Read")]
        public async Task<IActionResult> GetSummary([FromQuery] Guid? workflowDefinitionId)
        {
            var query = BuildBaseQuery();
            if (workflowDefinitionId.HasValue && workflowDefinitionId.Value != Guid.Empty)
                query = query.Where(x => x.WorkflowDefinitionId == workflowDefinitionId.Value);

            var result = new WorkflowStepSummaryResponse
            {
                TotalWorkflowStep = await query.CountAsync(),
                ActiveWorkflowStep = await query.CountAsync(x => x.IsActive),
                InactiveWorkflowStep = await query.CountAsync(x => !x.IsActive),
                ApprovalStep = await query.CountAsync(x => x.StepType == "Approval"),
                VerificationStep = await query.CountAsync(x => x.StepType == "Verification"),
                NotificationStep = await query.CountAsync(x => x.StepType == "Notification"),
                ParallelStep = await query.CountAsync(x => x.IsParallel),
                DelegationAllowedStep = await query.CountAsync(x => x.AllowDelegation)
            };

            return Ok(ApiResponse<WorkflowStepSummaryResponse>.Ok(
                result,
                "Ringkasan workflow step berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseWorkflowStepPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workflow Step", Description = "Melihat data workflow step", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkflowStep", "Read")]
        public async Task<IActionResult> GetWorkflowSteps(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? workflowDefinitionId,
            [FromQuery] string? stepType,
            [FromQuery] string? approvalMode,
            [FromQuery] string? approverSourceType,
            [FromQuery] bool? isParallel,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "stepOrder",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = WorkflowMasterDataSupport.NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = WorkflowMasterDataSupport.ApplyDateFilter(
                BuildBaseQuery(), startDate, endDate, customPeriod);
            query = ApplyStandardFilter(
                query, workflowDefinitionId, stepType, approvalMode,
                approverSourceType, isParallel, isActive, search);

            var totalData = await query.CountAsync();
            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
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
                    SpecificApproverUserName = x.SpecificApproverUserId == null
                        ? null
                        : _dbContext.Users.Where(u => u.Id == x.SpecificApproverUserId.Value)
                            .Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode)
                            .FirstOrDefault(),
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
                    CreateByName = x.CreateBy == Guid.Empty
                        ? null
                        : _dbContext.Users.Where(u => u.Id == x.CreateBy)
                            .Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode)
                            .FirstOrDefault(),
                    UpdateDateTime = x.UpdateDateTime,
                    UpdateBy = x.UpdateBy == Guid.Empty ? null : x.UpdateBy,
                    UpdateByName = x.UpdateBy == Guid.Empty
                        ? null
                        : _dbContext.Users.Where(u => u.Id == x.UpdateBy)
                            .Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode)
                            .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(ApiResponse<ResponseWorkflowStepPagedResult>.Ok(
                new ResponseWorkflowStepPagedResult
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = items
                },
                "Data workflow step berhasil diambil."));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<WorkflowStepOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workflow Step", Description = "Melihat pilihan workflow step", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkflowStep", "Read")]
        public async Task<IActionResult> GetWorkflowStepOptions(
            [FromQuery] Guid? workflowDefinitionId,
            [FromQuery] string? stepType,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = WorkflowMasterDataSupport.NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = ApplyStandardFilter(
                BuildBaseQuery(), workflowDefinitionId, stepType, null,
                null, null, onlyActive ? true : null, search);

            var totalData = await query.CountAsync();
            var items = await query
                .OrderBy(x => x.StepOrder)
                .ThenBy(x => x.StepName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new WorkflowStepOptionResponse
                {
                    Id = x.Id,
                    WorkflowDefinitionId = x.WorkflowDefinitionId,
                    WorkflowCode = x.WorkflowDefinition != null ? x.WorkflowDefinition.WorkflowCode : null,
                    StepCode = x.StepCode,
                    StepName = x.StepName,
                    StepOrder = x.StepOrder,
                    StepType = x.StepType,
                    ApprovalMode = x.ApprovalMode,
                    ApproverSourceType = x.ApproverSourceType
                })
                .ToListAsync();

            return Ok(ApiResponse<WorkflowStepOptionPagedResponse>.Ok(
                new WorkflowStepOptionPagedResponse
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = items
                },
                "Data pilihan workflow step berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkflowStepDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workflow Step", Description = "Melihat detail workflow step", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkflowStep", "Read")]
        public async Task<IActionResult> GetWorkflowStepById(Guid id)
        {
            var data = await BuildBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => new WorkflowStepDetailResponse
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
                    SpecificApproverUserName = x.SpecificApproverUserId == null
                        ? null
                        : _dbContext.Users.Where(u => u.Id == x.SpecificApproverUserId.Value)
                            .Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode)
                            .FirstOrDefault(),
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
                    CreateByName = x.CreateBy == Guid.Empty
                        ? null
                        : _dbContext.Users.Where(u => u.Id == x.CreateBy)
                            .Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode)
                            .FirstOrDefault(),
                    UpdateDateTime = x.UpdateDateTime,
                    UpdateBy = x.UpdateBy == Guid.Empty ? null : x.UpdateBy,
                    UpdateByName = x.UpdateBy == Guid.Empty
                        ? null
                        : _dbContext.Users.Where(u => u.Id == x.UpdateBy)
                            .Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode)
                            .FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workflow step tidak ditemukan."));
            }

            return Ok(ApiResponse<WorkflowStepDetailResponse>.Ok(
                data,
                "Detail workflow step berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<WorkflowStepCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Workflow Step", Description = "Membuat workflow step", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("WorkflowStep", "Create")]
        public async Task<IActionResult> CreateWorkflowStep([FromBody] CreateWorkflowStepRequest request)
        {
            var validation = await ValidateRequestAsync(null, request);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data workflow step tidak valid."));
            }

            var actor = WorkflowMasterDataSupport.GetCurrentUserId(User);
            var entity = new MstWorkflowStep
            {
                Id = Guid.NewGuid(),
                WorkflowDefinitionId = request.WorkflowDefinitionId,
                StepCode = WorkflowMasterDataSupport.NormalizeCode(request.StepCode),
                StepName = request.StepName.Trim(),
                StepOrder = request.StepOrder,
                StepType = CanonicalValue(StepTypes, request.StepType),
                ApprovalMode = CanonicalValue(ApprovalModes, request.ApprovalMode),
                RequiredApprovalCount = request.RequiredApprovalCount,
                RequiredApprovalPercentage = request.RequiredApprovalPercentage,
                ApproverSourceType = CanonicalValue(ApproverSources, request.ApproverSourceType),
                ApproverPositionId = WorkflowMasterDataSupport.NormalizeGuid(request.ApproverPositionId),
                ApproverOrganizationUnitId = WorkflowMasterDataSupport.NormalizeGuid(request.ApproverOrganizationUnitId),
                SpecificApproverUserId = WorkflowMasterDataSupport.NormalizeGuid(request.SpecificApproverUserId),
                ApproverRoleCode = WorkflowMasterDataSupport.NormalizeNullableString(request.ApproverRoleCode),
                ManagerLevel = request.ManagerLevel,
                IsRequired = request.IsRequired,
                IsParallel = request.IsParallel,
                AllowDelegation = request.AllowDelegation,
                AllowSelfApproval = request.AllowSelfApproval,
                ReminderAfterHours = request.ReminderAfterHours,
                EscalationAfterHours = request.EscalationAfterHours,
                AutoApproveAfterHours = request.AutoApproveAfterHours,
                AutoRejectAfterHours = request.AutoRejectAfterHours,
                OnApproveNextStepCode = NormalizeOptionalCode(request.OnApproveNextStepCode),
                OnRejectStepCode = NormalizeOptionalCode(request.OnRejectStepCode),
                Instructions = WorkflowMasterDataSupport.NormalizeNullableString(request.Instructions),
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actor,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstWorkflowStep>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var result = new WorkflowStepCreateResponse
            {
                Id = entity.Id,
                WorkflowDefinitionId = entity.WorkflowDefinitionId,
                StepCode = entity.StepCode,
                StepName = entity.StepName,
                StepOrder = entity.StepOrder,
                IsActive = entity.IsActive
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkflowStep.CreateWorkflowStep",
                "Membuat workflow step.",
                result);

            return Ok(ApiResponse<WorkflowStepCreateResponse>.Ok(
                result,
                "Workflow step berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Update", "Update Workflow Step", Description = "Mengubah workflow step", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkflowStep", "Update")]
        public async Task<IActionResult> UpdateWorkflowStep(
            Guid id,
            [FromBody] UpdateWorkflowStepRequest request)
        {
            var entity = await _dbContext.Set<MstWorkflowStep>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workflow step tidak ditemukan."));
            }

            var validation = await ValidateRequestAsync(id, request);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data workflow step tidak valid."));
            }

            entity.WorkflowDefinitionId = request.WorkflowDefinitionId;
            entity.StepCode = WorkflowMasterDataSupport.NormalizeCode(request.StepCode);
            entity.StepName = request.StepName.Trim();
            entity.StepOrder = request.StepOrder;
            entity.StepType = CanonicalValue(StepTypes, request.StepType);
            entity.ApprovalMode = CanonicalValue(ApprovalModes, request.ApprovalMode);
            entity.RequiredApprovalCount = request.RequiredApprovalCount;
            entity.RequiredApprovalPercentage = request.RequiredApprovalPercentage;
            entity.ApproverSourceType = CanonicalValue(ApproverSources, request.ApproverSourceType);
            entity.ApproverPositionId = WorkflowMasterDataSupport.NormalizeGuid(request.ApproverPositionId);
            entity.ApproverOrganizationUnitId = WorkflowMasterDataSupport.NormalizeGuid(request.ApproverOrganizationUnitId);
            entity.SpecificApproverUserId = WorkflowMasterDataSupport.NormalizeGuid(request.SpecificApproverUserId);
            entity.ApproverRoleCode = WorkflowMasterDataSupport.NormalizeNullableString(request.ApproverRoleCode);
            entity.ManagerLevel = request.ManagerLevel;
            entity.IsRequired = request.IsRequired;
            entity.IsParallel = request.IsParallel;
            entity.AllowDelegation = request.AllowDelegation;
            entity.AllowSelfApproval = request.AllowSelfApproval;
            entity.ReminderAfterHours = request.ReminderAfterHours;
            entity.EscalationAfterHours = request.EscalationAfterHours;
            entity.AutoApproveAfterHours = request.AutoApproveAfterHours;
            entity.AutoRejectAfterHours = request.AutoRejectAfterHours;
            entity.OnApproveNextStepCode = NormalizeOptionalCode(request.OnApproveNextStepCode);
            entity.OnRejectStepCode = NormalizeOptionalCode(request.OnRejectStepCode);
            entity.Instructions = WorkflowMasterDataSupport.NormalizeNullableString(request.Instructions);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = WorkflowMasterDataSupport.GetCurrentUserId(User);
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkflowStep.UpdateWorkflowStep",
                "Mengubah workflow step.",
                new { entity.Id, entity.StepCode, entity.StepName, entity.StepOrder, entity.IsActive });

            return Ok(ApiResponse<object>.Ok(null, "Workflow step berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Workflow Step Status", Description = "Mengubah status workflow step", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("WorkflowStep", "Update")]
        public async Task<IActionResult> UpdateWorkflowStepStatus(
            Guid id,
            [FromBody] UpdateWorkflowMasterStatusRequest request)
        {
            var entity = await _dbContext.Set<MstWorkflowStep>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workflow step tidak ditemukan."));
            }

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = WorkflowMasterDataSupport.GetCurrentUserId(User);
            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(null, "Status workflow step berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Delete", "Delete Workflow Step", Description = "Menghapus workflow step", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("WorkflowStep", "Delete")]
        public async Task<IActionResult> DeleteWorkflowStep(Guid id)
        {
            var entity = await _dbContext.Set<MstWorkflowStep>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workflow step tidak ditemukan."));
            }

            var isUsed =
                await _dbContext.Set<MstApprovalMatrix>().AsNoTracking().AnyAsync(x => x.WorkflowStepId == id && !x.IsDelete) ||
                await _dbContext.Set<MstApprovalDelegationPolicy>().AsNoTracking().AnyAsync(x => x.WorkflowStepId == id && !x.IsDelete) ||
                await _dbContext.Set<MstRejectionReason>().AsNoTracking().AnyAsync(x => x.WorkflowStepId == id && !x.IsDelete) ||
                await _dbContext.Set<TrxWorkflowStepInstance>().AsNoTracking().AnyAsync(x => x.WorkflowStepId == id && !x.IsDelete) ||
                await _dbContext.Set<TrxApprovalDelegation>().AsNoTracking().AnyAsync(x => x.WorkflowStepId == id && !x.IsDelete);

            if (isUsed)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Workflow step tidak dapat dihapus karena sudah digunakan oleh konfigurasi atau transaksi workflow."));
            }

            var now = DateTime.UtcNow;
            var actor = WorkflowMasterDataSupport.GetCurrentUserId(User);
            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actor;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor;
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkflowStep.DeleteWorkflowStep",
                "Menghapus workflow step.",
                new { entity.Id, entity.StepCode, entity.StepName, entity.DeleteDateTime });

            return Ok(ApiResponse<object>.Ok(null, "Workflow step berhasil dihapus."));
        }

        private IQueryable<MstWorkflowStep> BuildBaseQuery() =>
            _dbContext.Set<MstWorkflowStep>().AsNoTracking().Where(x => !x.IsDelete);

        private static IQueryable<MstWorkflowStep> ApplyStandardFilter(
            IQueryable<MstWorkflowStep> query,
            Guid? workflowDefinitionId,
            string? stepType,
            string? approvalMode,
            string? approverSourceType,
            bool? isParallel,
            bool? isActive,
            string? search)
        {
            if (workflowDefinitionId.HasValue && workflowDefinitionId.Value != Guid.Empty)
                query = query.Where(x => x.WorkflowDefinitionId == workflowDefinitionId.Value);
            if (!string.IsNullOrWhiteSpace(stepType))
                query = query.Where(x => x.StepType == stepType.Trim());
            if (!string.IsNullOrWhiteSpace(approvalMode))
                query = query.Where(x => x.ApprovalMode == approvalMode.Trim());
            if (!string.IsNullOrWhiteSpace(approverSourceType))
                query = query.Where(x => x.ApproverSourceType == approverSourceType.Trim());
            if (isParallel.HasValue)
                query = query.Where(x => x.IsParallel == isParallel.Value);
            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.StepCode.ToLower().Contains(keyword) ||
                    x.StepName.ToLower().Contains(keyword) ||
                    x.StepType.ToLower().Contains(keyword) ||
                    x.ApproverSourceType.ToLower().Contains(keyword) ||
                    x.WorkflowDefinition != null &&
                    (x.WorkflowDefinition.WorkflowCode.ToLower().Contains(keyword) ||
                     x.WorkflowDefinition.WorkflowName.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<MstWorkflowStep> ApplySorting(
            IQueryable<MstWorkflowStep> query,
            string? sortBy,
            string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "stepOrder").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "workflowname" => desc
                    ? query.OrderByDescending(x => x.WorkflowDefinition != null ? x.WorkflowDefinition.WorkflowName : string.Empty)
                    : query.OrderBy(x => x.WorkflowDefinition != null ? x.WorkflowDefinition.WorkflowName : string.Empty),
                "stepcode" => desc ? query.OrderByDescending(x => x.StepCode) : query.OrderBy(x => x.StepCode),
                "stepname" => desc ? query.OrderByDescending(x => x.StepName) : query.OrderBy(x => x.StepName),
                "steptype" => desc ? query.OrderByDescending(x => x.StepType) : query.OrderBy(x => x.StepType),
                "approvalmode" => desc ? query.OrderByDescending(x => x.ApprovalMode) : query.OrderBy(x => x.ApprovalMode),
                "isactive" => desc ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
                _ => desc
                    ? query.OrderByDescending(x => x.StepOrder).ThenByDescending(x => x.StepCode)
                    : query.OrderBy(x => x.StepOrder).ThenBy(x => x.StepCode)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            CreateWorkflowStepRequest request)
        {
            if (request.WorkflowDefinitionId == Guid.Empty)
                return (false, "Workflow definition wajib dipilih.");
            if (string.IsNullOrWhiteSpace(request.StepCode))
                return (false, "Kode step wajib diisi.");
            if (string.IsNullOrWhiteSpace(request.StepName))
                return (false, "Nama step wajib diisi.");
            if (request.StepOrder <= 0)
                return (false, "Step order harus lebih besar dari 0.");
            if (!StepTypes.Contains(request.StepType))
                return (false, "Step type tidak valid.");
            if (!ApprovalModes.Contains(request.ApprovalMode))
                return (false, "Approval mode tidak valid.");
            if (!ApproverSources.Contains(request.ApproverSourceType))
                return (false, "Approver source type tidak valid.");
            if (request.RequiredApprovalCount <= 0)
                return (false, "Required approval count harus lebih besar dari 0.");
            if (string.Equals(request.ApprovalMode, "Percentage", StringComparison.OrdinalIgnoreCase) &&
                (!request.RequiredApprovalPercentage.HasValue ||
                 request.RequiredApprovalPercentage.Value <= 0 ||
                 request.RequiredApprovalPercentage.Value > 100))
                return (false, "Approval mode Percentage wajib memiliki required approval percentage antara 0 dan 100.");

            var definitionExists = await _dbContext.Set<MstWorkflowDefinition>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == request.WorkflowDefinitionId && !x.IsDelete);
            if (!definitionExists)
                return (false, "Workflow definition tidak ditemukan.");

            var stepCode = WorkflowMasterDataSupport.NormalizeCode(request.StepCode);
            var duplicateQuery = _dbContext.Set<MstWorkflowStep>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.WorkflowDefinitionId == request.WorkflowDefinitionId &&
                    x.StepCode == stepCode);
            if (excludeId.HasValue)
                duplicateQuery = duplicateQuery.Where(x => x.Id != excludeId.Value);
            if (await duplicateQuery.AnyAsync())
                return (false, "Kode step tersebut sudah digunakan pada workflow definition yang sama.");

            var approverPositionId = WorkflowMasterDataSupport.NormalizeGuid(request.ApproverPositionId);
            var approverOrganizationUnitId = WorkflowMasterDataSupport.NormalizeGuid(request.ApproverOrganizationUnitId);
            var specificApproverUserId = WorkflowMasterDataSupport.NormalizeGuid(request.SpecificApproverUserId);

            if (string.Equals(request.ApproverSourceType, "Position", StringComparison.OrdinalIgnoreCase) &&
                !approverPositionId.HasValue)
                return (false, "Approver position wajib dipilih untuk approver source Position.");

            if (string.Equals(request.ApproverSourceType, "OrganizationUnit", StringComparison.OrdinalIgnoreCase) &&
                !approverOrganizationUnitId.HasValue)
                return (false, "Approver organization unit wajib dipilih untuk approver source OrganizationUnit.");

            if (string.Equals(request.ApproverSourceType, "SpecificUser", StringComparison.OrdinalIgnoreCase) &&
                !specificApproverUserId.HasValue)
                return (false, "Specific approver user wajib dipilih untuk approver source SpecificUser.");

            if ((string.Equals(request.ApproverSourceType, "Role", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(request.ApproverSourceType, "SpecificRole", StringComparison.OrdinalIgnoreCase)) &&
                string.IsNullOrWhiteSpace(request.ApproverRoleCode))
                return (false, "Approver role code wajib diisi untuk approver source role.");

            if (string.Equals(request.ApproverSourceType, "ManagerLevel", StringComparison.OrdinalIgnoreCase) &&
                (!request.ManagerLevel.HasValue || request.ManagerLevel.Value <= 0))
                return (false, "Manager level wajib diisi untuk approver source ManagerLevel.");

            if (approverPositionId.HasValue &&
                !await _dbContext.Set<MstPosition>().AsNoTracking().AnyAsync(x => x.Id == approverPositionId.Value && !x.IsDelete))
                return (false, "Approver position tidak ditemukan.");

            if (approverOrganizationUnitId.HasValue &&
                !await _dbContext.Set<MstOrganizationUnit>().AsNoTracking().AnyAsync(x => x.Id == approverOrganizationUnitId.Value && !x.IsDelete))
                return (false, "Approver organization unit tidak ditemukan.");

            if (specificApproverUserId.HasValue &&
                !await _dbContext.Users.AsNoTracking().AnyAsync(x => x.Id == specificApproverUserId.Value))
                return (false, "Specific approver user tidak ditemukan.");

            var approveNext = NormalizeOptionalCode(request.OnApproveNextStepCode);
            var rejectNext = NormalizeOptionalCode(request.OnRejectStepCode);
            if (approveNext == stepCode || rejectNext == stepCode)
                return (false, "Step tidak boleh mengarahkan hasil action kembali ke step yang sama.");

            return (true, null);
        }

        private static WorkflowMasterLookupOptionResponse ToLookup(string value) =>
            new() { Value = value, Label = value };

        private static string CanonicalValue(HashSet<string> values, string value) =>
            values.First(x => string.Equals(x, value.Trim(), StringComparison.OrdinalIgnoreCase));

        private static string? NormalizeOptionalCode(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : WorkflowMasterDataSupport.NormalizeCode(value);
    }
}
