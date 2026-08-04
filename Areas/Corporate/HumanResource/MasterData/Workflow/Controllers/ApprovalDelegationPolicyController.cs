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

using ResponseApprovalDelegationPolicyPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.DTOs.ApprovalDelegationPolicyResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/approval-delegation-policies")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Approval Delegation Policy",
        AreaName = "Corporate",
        ControllerName = "ApprovalDelegationPolicy",
        Description = "Corporate human resource master data approval delegation policy",
        SortOrder = 73)]
    [Tags("Corporate / Human Resource / Master Data / Approval Delegation Policy")]
    public class ApprovalDelegationPolicyController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.MasterData";

        private static readonly HashSet<string> DelegationTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "Temporary", "Permanent", "AutomaticOutOfOffice"
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public ApprovalDelegationPolicyController(ApplicationDbContext dbContext, LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<ApprovalDelegationPolicyFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Approval Delegation Policy", Description = "Melihat metadata filter approval delegation policy", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ApprovalDelegationPolicy", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new ApprovalDelegationPolicyFilterMetadataResponse
            {
                DefaultFilter = new ApprovalDelegationPolicyDefaultFilterResponse(),
                CustomPeriods = WorkflowMasterDataSupport.BuildPeriodOptions(),
                SortOptions = new List<WorkflowMasterSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "delegationPolicyCode", Label = "Kode policy" },
                    new() { Value = "delegationPolicyName", Label = "Nama policy" },
                    new() { Value = "delegationType", Label = "Tipe delegation" },
                    new() { Value = "maximumDelegationDays", Label = "Maksimum hari delegation" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                DelegationTypes = DelegationTypes
                    .OrderBy(x => x)
                    .Select(x => new WorkflowMasterLookupOptionResponse { Value = x, Label = x })
                    .ToList()
            };

            await _loggerService.InfoAsync(LogCategory, "ApprovalDelegationPolicy.GetFilterMetadata", "Mengambil metadata filter approval delegation policy.", result);
            return Ok(ApiResponse<ApprovalDelegationPolicyFilterMetadataResponse>.Ok(result, "Metadata filter approval delegation policy berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<ApprovalDelegationPolicySummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Approval Delegation Policy", Description = "Melihat ringkasan approval delegation policy", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ApprovalDelegationPolicy", "Read")]
        public async Task<IActionResult> GetSummary([FromQuery] Guid? workflowDefinitionId)
        {
            var query = BuildBaseQuery();
            if (workflowDefinitionId.HasValue && workflowDefinitionId.Value != Guid.Empty)
                query = query.Where(x => x.WorkflowDefinitionId == workflowDefinitionId.Value);

            var result = new ApprovalDelegationPolicySummaryResponse
            {
                TotalPolicy = await query.CountAsync(),
                ActivePolicy = await query.CountAsync(x => x.IsActive),
                InactivePolicy = await query.CountAsync(x => !x.IsActive),
                TemporaryPolicy = await query.CountAsync(x => x.DelegationType == "Temporary"),
                PermanentPolicy = await query.CountAsync(x => x.DelegationType == "Permanent"),
                AutomaticOutOfOfficePolicy = await query.CountAsync(x => x.DelegationType == "AutomaticOutOfOffice"),
                ManagerApprovalRequiredPolicy = await query.CountAsync(x => x.RequireManagerApproval),
                HrVerificationRequiredPolicy = await query.CountAsync(x => x.RequireHrVerification)
            };

            return Ok(ApiResponse<ApprovalDelegationPolicySummaryResponse>.Ok(result, "Ringkasan approval delegation policy berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseApprovalDelegationPolicyPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Approval Delegation Policy", Description = "Melihat data approval delegation policy", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ApprovalDelegationPolicy", "Read")]
        public async Task<IActionResult> GetApprovalDelegationPolicies(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? workflowDefinitionId,
            [FromQuery] Guid? workflowStepId,
            [FromQuery] Guid? legalEntityId,
            [FromQuery] Guid? hospitalSiteId,
            [FromQuery] Guid? organizationUnitId,
            [FromQuery] string? delegationType,
            [FromQuery] bool? requireManagerApproval,
            [FromQuery] bool? requireHrVerification,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "delegationPolicyName",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = WorkflowMasterDataSupport.NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = WorkflowMasterDataSupport.ApplyDateFilter(BuildBaseQuery(), startDate, endDate, customPeriod);
            query = ApplyStandardFilter(
                query, workflowDefinitionId, workflowStepId, legalEntityId,
                hospitalSiteId, organizationUnitId, delegationType,
                requireManagerApproval, requireHrVerification, isActive, search);

            var totalData = await query.CountAsync();
            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
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
                    CreateByName = x.CreateBy == Guid.Empty ? null : _dbContext.Users.Where(u => u.Id == x.CreateBy).Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode).FirstOrDefault(),
                    UpdateDateTime = x.UpdateDateTime,
                    UpdateBy = x.UpdateBy == Guid.Empty ? null : x.UpdateBy,
                    UpdateByName = x.UpdateBy == Guid.Empty ? null : _dbContext.Users.Where(u => u.Id == x.UpdateBy).Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode).FirstOrDefault()
                })
                .ToListAsync();

            return Ok(ApiResponse<ResponseApprovalDelegationPolicyPagedResult>.Ok(
                new ResponseApprovalDelegationPolicyPagedResult
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = items
                },
                "Data approval delegation policy berhasil diambil."));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<ApprovalDelegationPolicyOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Approval Delegation Policy", Description = "Melihat pilihan approval delegation policy", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ApprovalDelegationPolicy", "Read")]
        public async Task<IActionResult> GetApprovalDelegationPolicyOptions(
            [FromQuery] Guid? workflowDefinitionId,
            [FromQuery] Guid? workflowStepId,
            [FromQuery] string? delegationType,
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
                null, null, null, delegationType, null, null,
                onlyActive ? true : null, search);

            var totalData = await query.CountAsync();
            var items = await query
                .OrderBy(x => x.DelegationPolicyName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ApprovalDelegationPolicyOptionResponse
                {
                    Id = x.Id,
                    DelegationPolicyCode = x.DelegationPolicyCode,
                    DelegationPolicyName = x.DelegationPolicyName,
                    DelegationType = x.DelegationType,
                    MaximumDelegationDays = x.MaximumDelegationDays,
                    WorkflowDefinitionId = x.WorkflowDefinitionId,
                    WorkflowStepId = x.WorkflowStepId
                })
                .ToListAsync();

            return Ok(ApiResponse<ApprovalDelegationPolicyOptionPagedResponse>.Ok(
                new ApprovalDelegationPolicyOptionPagedResponse
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = items
                },
                "Data pilihan approval delegation policy berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ApprovalDelegationPolicyDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Approval Delegation Policy", Description = "Melihat detail approval delegation policy", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ApprovalDelegationPolicy", "Read")]
        public async Task<IActionResult> GetApprovalDelegationPolicyById(Guid id)
        {
            var data = await BuildBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => new ApprovalDelegationPolicyDetailResponse
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
                    CreateByName = x.CreateBy == Guid.Empty ? null : _dbContext.Users.Where(u => u.Id == x.CreateBy).Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode).FirstOrDefault(),
                    UpdateDateTime = x.UpdateDateTime,
                    UpdateBy = x.UpdateBy == Guid.Empty ? null : x.UpdateBy,
                    UpdateByName = x.UpdateBy == Guid.Empty ? null : _dbContext.Users.Where(u => u.Id == x.UpdateBy).Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode).FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (data == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Approval delegation policy tidak ditemukan."));

            return Ok(ApiResponse<ApprovalDelegationPolicyDetailResponse>.Ok(data, "Detail approval delegation policy berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ApprovalDelegationPolicyCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Approval Delegation Policy", Description = "Membuat approval delegation policy", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("ApprovalDelegationPolicy", "Create")]
        public async Task<IActionResult> CreateApprovalDelegationPolicy([FromBody] CreateApprovalDelegationPolicyRequest request)
        {
            var validation = await ValidateRequestAsync(null, request);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage ?? "Data approval delegation policy tidak valid."));

            var actor = WorkflowMasterDataSupport.GetCurrentUserId(User);
            var entity = new MstApprovalDelegationPolicy
            {
                Id = Guid.NewGuid(),
                WorkflowDefinitionId = WorkflowMasterDataSupport.NormalizeGuid(request.WorkflowDefinitionId),
                WorkflowStepId = WorkflowMasterDataSupport.NormalizeGuid(request.WorkflowStepId),
                LegalEntityId = WorkflowMasterDataSupport.NormalizeGuid(request.LegalEntityId),
                HospitalSiteId = WorkflowMasterDataSupport.NormalizeGuid(request.HospitalSiteId),
                OrganizationUnitId = WorkflowMasterDataSupport.NormalizeGuid(request.OrganizationUnitId),
                DelegationPolicyCode = WorkflowMasterDataSupport.NormalizeCode(request.DelegationPolicyCode),
                DelegationPolicyName = request.DelegationPolicyName.Trim(),
                DelegationType = CanonicalType(request.DelegationType),
                MaximumDelegationDays = request.MaximumDelegationDays,
                MinimumNoticeHours = request.MinimumNoticeHours,
                RequireManagerApproval = request.RequireManagerApproval,
                RequireHrVerification = request.RequireHrVerification,
                AllowCrossOrganizationUnit = request.AllowCrossOrganizationUnit,
                AllowCrossHospitalSite = request.AllowCrossHospitalSite,
                AllowCrossLegalEntity = request.AllowCrossLegalEntity,
                AllowSubDelegation = request.AllowSubDelegation,
                AllowSelfDelegation = request.AllowSelfDelegation,
                PreserveDelegatorAccountability = request.PreserveDelegatorAccountability,
                ApprovalWorkflowCode = NormalizeOptionalCode(request.ApprovalWorkflowCode),
                EffectiveStartDate = request.EffectiveStartDate?.Date,
                EffectiveEndDate = request.EffectiveEndDate?.Date,
                Description = WorkflowMasterDataSupport.NormalizeNullableString(request.Description),
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actor,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstApprovalDelegationPolicy>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var result = new ApprovalDelegationPolicyCreateResponse
            {
                Id = entity.Id,
                DelegationPolicyCode = entity.DelegationPolicyCode,
                DelegationPolicyName = entity.DelegationPolicyName,
                DelegationType = entity.DelegationType,
                IsActive = entity.IsActive
            };

            await _loggerService.InfoAsync(LogCategory, "ApprovalDelegationPolicy.CreateApprovalDelegationPolicy", "Membuat approval delegation policy.", result);
            return Ok(ApiResponse<ApprovalDelegationPolicyCreateResponse>.Ok(result, "Approval delegation policy berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Update", "Update Approval Delegation Policy", Description = "Mengubah approval delegation policy", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("ApprovalDelegationPolicy", "Update")]
        public async Task<IActionResult> UpdateApprovalDelegationPolicy(Guid id, [FromBody] UpdateApprovalDelegationPolicyRequest request)
        {
            var entity = await _dbContext.Set<MstApprovalDelegationPolicy>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Approval delegation policy tidak ditemukan."));

            var validation = await ValidateRequestAsync(id, request);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage ?? "Data approval delegation policy tidak valid."));

            entity.WorkflowDefinitionId = WorkflowMasterDataSupport.NormalizeGuid(request.WorkflowDefinitionId);
            entity.WorkflowStepId = WorkflowMasterDataSupport.NormalizeGuid(request.WorkflowStepId);
            entity.LegalEntityId = WorkflowMasterDataSupport.NormalizeGuid(request.LegalEntityId);
            entity.HospitalSiteId = WorkflowMasterDataSupport.NormalizeGuid(request.HospitalSiteId);
            entity.OrganizationUnitId = WorkflowMasterDataSupport.NormalizeGuid(request.OrganizationUnitId);
            entity.DelegationPolicyCode = WorkflowMasterDataSupport.NormalizeCode(request.DelegationPolicyCode);
            entity.DelegationPolicyName = request.DelegationPolicyName.Trim();
            entity.DelegationType = CanonicalType(request.DelegationType);
            entity.MaximumDelegationDays = request.MaximumDelegationDays;
            entity.MinimumNoticeHours = request.MinimumNoticeHours;
            entity.RequireManagerApproval = request.RequireManagerApproval;
            entity.RequireHrVerification = request.RequireHrVerification;
            entity.AllowCrossOrganizationUnit = request.AllowCrossOrganizationUnit;
            entity.AllowCrossHospitalSite = request.AllowCrossHospitalSite;
            entity.AllowCrossLegalEntity = request.AllowCrossLegalEntity;
            entity.AllowSubDelegation = request.AllowSubDelegation;
            entity.AllowSelfDelegation = request.AllowSelfDelegation;
            entity.PreserveDelegatorAccountability = request.PreserveDelegatorAccountability;
            entity.ApprovalWorkflowCode = NormalizeOptionalCode(request.ApprovalWorkflowCode);
            entity.EffectiveStartDate = request.EffectiveStartDate?.Date;
            entity.EffectiveEndDate = request.EffectiveEndDate?.Date;
            entity.Description = WorkflowMasterDataSupport.NormalizeNullableString(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = WorkflowMasterDataSupport.GetCurrentUserId(User);
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(LogCategory, "ApprovalDelegationPolicy.UpdateApprovalDelegationPolicy", "Mengubah approval delegation policy.", new { entity.Id, entity.DelegationPolicyCode, entity.DelegationPolicyName, entity.IsActive });
            return Ok(ApiResponse<object>.Ok(null, "Approval delegation policy berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Approval Delegation Policy Status", Description = "Mengubah status approval delegation policy", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("ApprovalDelegationPolicy", "Update")]
        public async Task<IActionResult> UpdateApprovalDelegationPolicyStatus(Guid id, [FromBody] UpdateWorkflowMasterStatusRequest request)
        {
            var entity = await _dbContext.Set<MstApprovalDelegationPolicy>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Approval delegation policy tidak ditemukan."));

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = WorkflowMasterDataSupport.GetCurrentUserId(User);
            await _dbContext.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(null, "Status approval delegation policy berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Delete", "Delete Approval Delegation Policy", Description = "Menghapus approval delegation policy", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("ApprovalDelegationPolicy", "Delete")]
        public async Task<IActionResult> DeleteApprovalDelegationPolicy(Guid id)
        {
            var entity = await _dbContext.Set<MstApprovalDelegationPolicy>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Approval delegation policy tidak ditemukan."));

            var isUsed = await _dbContext.Set<TrxApprovalDelegation>().AsNoTracking().AnyAsync(x => x.ApprovalDelegationPolicyId == id && !x.IsDelete);
            if (isUsed)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Approval delegation policy tidak dapat dihapus karena sudah digunakan oleh transaksi delegation."));

            var now = DateTime.UtcNow;
            var actor = WorkflowMasterDataSupport.GetCurrentUserId(User);
            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actor;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor;
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(LogCategory, "ApprovalDelegationPolicy.DeleteApprovalDelegationPolicy", "Menghapus approval delegation policy.", new { entity.Id, entity.DelegationPolicyCode, entity.DelegationPolicyName, entity.DeleteDateTime });
            return Ok(ApiResponse<object>.Ok(null, "Approval delegation policy berhasil dihapus."));
        }

        private IQueryable<MstApprovalDelegationPolicy> BuildBaseQuery() =>
            _dbContext.Set<MstApprovalDelegationPolicy>().AsNoTracking().Where(x => !x.IsDelete);

        private static IQueryable<MstApprovalDelegationPolicy> ApplyStandardFilter(
            IQueryable<MstApprovalDelegationPolicy> query,
            Guid? workflowDefinitionId,
            Guid? workflowStepId,
            Guid? legalEntityId,
            Guid? hospitalSiteId,
            Guid? organizationUnitId,
            string? delegationType,
            bool? requireManagerApproval,
            bool? requireHrVerification,
            bool? isActive,
            string? search)
        {
            if (workflowDefinitionId.HasValue && workflowDefinitionId.Value != Guid.Empty) query = query.Where(x => x.WorkflowDefinitionId == workflowDefinitionId.Value);
            if (workflowStepId.HasValue && workflowStepId.Value != Guid.Empty) query = query.Where(x => x.WorkflowStepId == workflowStepId.Value);
            if (legalEntityId.HasValue && legalEntityId.Value != Guid.Empty) query = query.Where(x => x.LegalEntityId == legalEntityId.Value);
            if (hospitalSiteId.HasValue && hospitalSiteId.Value != Guid.Empty) query = query.Where(x => x.HospitalSiteId == hospitalSiteId.Value);
            if (organizationUnitId.HasValue && organizationUnitId.Value != Guid.Empty) query = query.Where(x => x.OrganizationUnitId == organizationUnitId.Value);
            if (!string.IsNullOrWhiteSpace(delegationType)) query = query.Where(x => x.DelegationType == delegationType.Trim());
            if (requireManagerApproval.HasValue) query = query.Where(x => x.RequireManagerApproval == requireManagerApproval.Value);
            if (requireHrVerification.HasValue) query = query.Where(x => x.RequireHrVerification == requireHrVerification.Value);
            if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.DelegationPolicyCode.ToLower().Contains(keyword) ||
                    x.DelegationPolicyName.ToLower().Contains(keyword) ||
                    x.DelegationType.ToLower().Contains(keyword) ||
                    x.ApprovalWorkflowCode != null && x.ApprovalWorkflowCode.ToLower().Contains(keyword) ||
                    x.Description != null && x.Description.ToLower().Contains(keyword));
            }

            return query;
        }

        private static IOrderedQueryable<MstApprovalDelegationPolicy> ApplySorting(IQueryable<MstApprovalDelegationPolicy> query, string? sortBy, string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "delegationPolicyName").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "delegationpolicycode" => desc ? query.OrderByDescending(x => x.DelegationPolicyCode) : query.OrderBy(x => x.DelegationPolicyCode),
                "delegationtype" => desc ? query.OrderByDescending(x => x.DelegationType) : query.OrderBy(x => x.DelegationType),
                "maximumdelegationdays" => desc ? query.OrderByDescending(x => x.MaximumDelegationDays) : query.OrderBy(x => x.MaximumDelegationDays),
                "isactive" => desc ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
                _ => desc ? query.OrderByDescending(x => x.DelegationPolicyName) : query.OrderBy(x => x.DelegationPolicyName)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(Guid? excludeId, CreateApprovalDelegationPolicyRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.DelegationPolicyCode)) return (false, "Kode delegation policy wajib diisi.");
            if (string.IsNullOrWhiteSpace(request.DelegationPolicyName)) return (false, "Nama delegation policy wajib diisi.");
            if (!DelegationTypes.Contains(request.DelegationType)) return (false, "Delegation type tidak valid.");
            if (request.MaximumDelegationDays <= 0) return (false, "Maximum delegation days harus lebih besar dari 0.");
            if (request.MinimumNoticeHours < 0) return (false, "Minimum notice hours tidak boleh negatif.");
            if (!WorkflowMasterDataSupport.IsEffectiveDateValid(request.EffectiveStartDate, request.EffectiveEndDate)) return (false, "Effective end date tidak boleh lebih kecil dari effective start date.");

            var definitionId = WorkflowMasterDataSupport.NormalizeGuid(request.WorkflowDefinitionId);
            var stepId = WorkflowMasterDataSupport.NormalizeGuid(request.WorkflowStepId);
            if (stepId.HasValue && !definitionId.HasValue) return (false, "Workflow definition wajib dipilih jika workflow step diisi.");

            if (definitionId.HasValue && !await _dbContext.Set<MstWorkflowDefinition>().AsNoTracking().AnyAsync(x => x.Id == definitionId.Value && !x.IsDelete)) return (false, "Workflow definition tidak ditemukan.");
            if (stepId.HasValue)
            {
                var step = await _dbContext.Set<MstWorkflowStep>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == stepId.Value && !x.IsDelete);
                if (step == null) return (false, "Workflow step tidak ditemukan.");
                if (step.WorkflowDefinitionId != definitionId) return (false, "Workflow step tidak berasal dari workflow definition yang dipilih.");
            }

            var legalEntityId = WorkflowMasterDataSupport.NormalizeGuid(request.LegalEntityId);
            var hospitalSiteId = WorkflowMasterDataSupport.NormalizeGuid(request.HospitalSiteId);
            var organizationUnitId = WorkflowMasterDataSupport.NormalizeGuid(request.OrganizationUnitId);
            if (legalEntityId.HasValue && !await _dbContext.Set<MstLegalEntity>().AsNoTracking().AnyAsync(x => x.Id == legalEntityId.Value && !x.IsDelete)) return (false, "Legal entity tidak ditemukan.");
            if (hospitalSiteId.HasValue && !await _dbContext.Set<MstHospitalSite>().AsNoTracking().AnyAsync(x => x.Id == hospitalSiteId.Value && !x.IsDelete)) return (false, "Hospital site tidak ditemukan.");
            if (organizationUnitId.HasValue && !await _dbContext.Set<MstOrganizationUnit>().AsNoTracking().AnyAsync(x => x.Id == organizationUnitId.Value && !x.IsDelete)) return (false, "Organization unit tidak ditemukan.");

            var approvalWorkflowCode = NormalizeOptionalCode(request.ApprovalWorkflowCode);
            if (approvalWorkflowCode != null)
            {
                var exists = await _dbContext.Set<MstWorkflowDefinition>().AsNoTracking().AnyAsync(x => !x.IsDelete && x.WorkflowCode == approvalWorkflowCode);
                if (!exists) return (false, "Approval workflow code tidak ditemukan pada workflow definition.");
            }

            var code = WorkflowMasterDataSupport.NormalizeCode(request.DelegationPolicyCode);
            var duplicateQuery = _dbContext.Set<MstApprovalDelegationPolicy>().AsNoTracking().Where(x => !x.IsDelete && x.DelegationPolicyCode == code);
            if (excludeId.HasValue) duplicateQuery = duplicateQuery.Where(x => x.Id != excludeId.Value);
            if (await duplicateQuery.AnyAsync()) return (false, "Kode delegation policy tersebut sudah digunakan.");

            return (true, null);
        }

        private static string CanonicalType(string value) => DelegationTypes.First(x => string.Equals(x, value.Trim(), StringComparison.OrdinalIgnoreCase));
        private static string? NormalizeOptionalCode(string? value) => string.IsNullOrWhiteSpace(value) ? null : WorkflowMasterDataSupport.NormalizeCode(value);
    }
}
