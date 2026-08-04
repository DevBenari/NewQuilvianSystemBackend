using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.EmployeeRelation.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/disciplinary-actions")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_EMPLOYEE_RELATION",
        moduleName: "Human Resource Employee Relation",
        displayName: "Workforce Disciplinary Action",
        AreaName = "Corporate",
        ControllerName = "WorkforceDisciplinaryAction",
        Description = "Restricted workforce disciplinary action",
        SortOrder = 1)]
    [Tags("Corporate / Human Resource / Employee Relation Management / Disciplinary Action")]
    public class WfpDisciplinaryActionController : ControllerBase
    {
        private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "Draft", "Issued", "UnderReview", "Approved", "Rejected", "Effective", "Completed", "Cancelled"
        };

        private static readonly HashSet<string> AllowedAppealStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "Submitted", "UnderReview", "Accepted", "Rejected", "Withdrawn"
        };

        private static readonly HashSet<string> AllowedClassifications = new(StringComparer.OrdinalIgnoreCase)
        {
            "Restricted", "Confidential", "HighlyRestricted"
        };

        private const string LogCategory = "Corporate.HumanResource.EmployeeRelation";
        private const string CodePrefix = "DCA-RSMMC-";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public WfpDisciplinaryActionController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Workforce Disciplinary Action", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceDisciplinaryAction", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = new WfpDisciplinaryActionFilterMetadataResponse
            {
                DefaultFilter = new WfpDisciplinaryActionDefaultFilterResponse(),
                ActionStatusOptions = BuildOptions(AllowedStatuses),
                AppealStatusOptions = BuildOptions(AllowedAppealStatuses),
                AccessClassificationOptions = BuildOptions(AllowedClassifications),
                SortOptions = new List<WfpDisciplinaryActionSortOptionResponse>
                {
                    new() { Value = "actionDate", Label = "Tanggal tindakan" },
                    new() { Value = "actionCode", Label = "Kode tindakan" },
                    new() { Value = "subject", Label = "Subjek" },
                    new() { Value = "actionStatus", Label = "Status" },
                    new() { Value = "isAcknowledged", Label = "Acknowledged" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            return Ok(ApiResponse<WfpDisciplinaryActionFilterMetadataResponse>.Ok(
                result,
                "Metadata filter tindakan disiplin berhasil diambil."));
        }

        [HttpGet("summary")]
        [AccessAction("Read", "Read Workforce Disciplinary Action", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceDisciplinaryAction", "Read")]
        public async Task<IActionResult> GetSummary(Guid workforceProfileId, CancellationToken cancellationToken)
        {
            if (!await WorkforceExistsAsync(workforceProfileId, cancellationToken))
                return NotFound(ApiResponse<object>.Fail(404, "Profil tenaga kerja tidak ditemukan."));

            var query = _dbContext.Set<WfpDisciplinaryAction>()
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            var result = new WfpDisciplinaryActionSummaryResponse
            {
                TotalData = await query.CountAsync(cancellationToken),
                ActiveData = await query.CountAsync(x => x.IsActive, cancellationToken),
                DraftData = await query.CountAsync(x => x.ActionStatus == "Draft", cancellationToken),
                ApprovedData = await query.CountAsync(x => x.ActionStatus == "Approved", cancellationToken),
                AcknowledgedData = await query.CountAsync(x => x.IsAcknowledged, cancellationToken),
                AppealedData = await query.CountAsync(x => x.IsAppealed, cancellationToken),
                ConfidentialData = await query.CountAsync(x => x.IsConfidential, cancellationToken)
            };

            return Ok(ApiResponse<WfpDisciplinaryActionSummaryResponse>.Ok(result, "Ringkasan berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read Workforce Disciplinary Action", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceDisciplinaryAction", "Read")]
        public async Task<IActionResult> GetData(
            Guid workforceProfileId,
            [FromQuery] Guid? disciplinaryActionTypeId,
            [FromQuery] Guid? violationTypeId,
            [FromQuery] Guid? sanctionTypeId,
            [FromQuery] string? actionStatus,
            [FromQuery] bool? isAcknowledged,
            [FromQuery] bool? isAppealed,
            [FromQuery] bool? isConfidential,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "actionDate",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            if (!await WorkforceExistsAsync(workforceProfileId, cancellationToken))
                return NotFound(ApiResponse<object>.Fail(404, "Profil tenaga kerja tidak ditemukan."));

            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = ApplyFilter(
                BuildBaseQuery(workforceProfileId),
                disciplinaryActionTypeId,
                violationTypeId,
                sanctionTypeId,
                actionStatus,
                isAcknowledged,
                isAppealed,
                isConfidential,
                isActive,
                search);

            var totalData = await query.CountAsync(cancellationToken);
            var rows = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var items = rows.Select(MapResponse).ToList();
            return Ok(ApiResponse<PagedResult<WfpDisciplinaryActionResponse>>.Ok(
                new PagedResult<WfpDisciplinaryActionResponse>
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = items
                },
                "Data tindakan disiplin berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Workforce Disciplinary Action", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceDisciplinaryAction", "Read")]
        public async Task<IActionResult> GetById(Guid workforceProfileId, Guid id, CancellationToken cancellationToken)
        {
            var entity = await BuildBaseQuery(workforceProfileId)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Tindakan disiplin tidak ditemukan."));

            return Ok(ApiResponse<WfpDisciplinaryActionDetailResponse>.Ok(
                MapDetailResponse(entity),
                "Detail tindakan disiplin berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Create", "Create Workforce Disciplinary Action", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("WorkforceDisciplinaryAction", "Create")]
        public async Task<IActionResult> Create(
            Guid workforceProfileId,
            [FromBody] CreateWfpDisciplinaryActionRequest request,
            CancellationToken cancellationToken)
        {
            var validation = await ValidateRequestAsync(workforceProfileId, request, cancellationToken);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(400, validation.ErrorMessage!));

            var actionType = await _dbContext.Set<MstDisciplinaryActionType>()
                .AsNoTracking()
                .FirstAsync(x => x.Id == request.DisciplinaryActionTypeId, cancellationToken);

            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();
            var entity = new WfpDisciplinaryAction
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = workforceProfileId,
                EmployeeId = NormalizeGuid(request.EmployeeId),
                OrganizationAssignmentId = NormalizeGuid(request.OrganizationAssignmentId),
                DisciplinaryCaseId = NormalizeGuid(request.DisciplinaryCaseId),
                DisciplinaryDecisionId = NormalizeGuid(request.DisciplinaryDecisionId),
                IncidentReportId = NormalizeGuid(request.IncidentReportId),
                DisciplinaryActionTypeId = request.DisciplinaryActionTypeId,
                ViolationTypeId = NormalizeGuid(request.ViolationTypeId),
                SanctionTypeId = NormalizeGuid(request.SanctionTypeId),
                EmployeeRelationCaseTypeId = NormalizeGuid(request.EmployeeRelationCaseTypeId),
                RequestReasonId = NormalizeGuid(request.RequestReasonId),
                WorkflowDefinitionId = NormalizeGuid(request.WorkflowDefinitionId),
                ActionCode = await GenerateCodeAsync(cancellationToken),
                ActionType = actionType.ActionTypeName,
                ActionLevel = NormalizeNullableText(request.ActionLevel) ?? actionType.DefaultActionLevel,
                ActionDate = request.ActionDate == default ? now : request.ActionDate,
                EffectiveStartDate = request.EffectiveStartDate,
                EffectiveEndDate = request.EffectiveEndDate,
                Subject = request.Subject.Trim(),
                Reason = NormalizeNullableText(request.Reason),
                DecisionSummary = NormalizeNullableText(request.DecisionSummary),
                ConfidentialNotes = NormalizeNullableText(request.ConfidentialNotes),
                ActionStatus = "Draft",
                IsConfidential = request.IsConfidential,
                AccessClassification = NormalizeClassification(request.AccessClassification),
                RequiresEnhancedAudit = request.RequiresEnhancedAudit,
                IssuedByUserId = actor,
                IsActive = true,
                Description = NormalizeNullableText(request.Description),
                CreateDateTime = now,
                CreateBy = actor,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<WfpDisciplinaryAction>().Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(LogCategory, "DisciplinaryAction.Create", "Membuat tindakan disiplin.", new { entity.Id, entity.ActionCode });
            return await GetById(workforceProfileId, entity.Id, cancellationToken);
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Workforce Disciplinary Action", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceDisciplinaryAction", "Update")]
        public async Task<IActionResult> Update(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWfpDisciplinaryActionRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<WfpDisciplinaryAction>()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete, cancellationToken);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Tindakan disiplin tidak ditemukan."));

            if (!string.Equals(entity.ActionStatus, "Draft", StringComparison.OrdinalIgnoreCase))
                return BadRequest(ApiResponse<object>.Fail(400, "Hanya tindakan disiplin berstatus Draft yang dapat diubah penuh."));

            var validation = await ValidateRequestAsync(workforceProfileId, request, cancellationToken);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(400, validation.ErrorMessage!));

            var actionType = await _dbContext.Set<MstDisciplinaryActionType>()
                .AsNoTracking()
                .FirstAsync(x => x.Id == request.DisciplinaryActionTypeId, cancellationToken);

            entity.EmployeeId = NormalizeGuid(request.EmployeeId);
            entity.OrganizationAssignmentId = NormalizeGuid(request.OrganizationAssignmentId);
            entity.DisciplinaryCaseId = NormalizeGuid(request.DisciplinaryCaseId);
            entity.DisciplinaryDecisionId = NormalizeGuid(request.DisciplinaryDecisionId);
            entity.IncidentReportId = NormalizeGuid(request.IncidentReportId);
            entity.DisciplinaryActionTypeId = request.DisciplinaryActionTypeId;
            entity.ViolationTypeId = NormalizeGuid(request.ViolationTypeId);
            entity.SanctionTypeId = NormalizeGuid(request.SanctionTypeId);
            entity.EmployeeRelationCaseTypeId = NormalizeGuid(request.EmployeeRelationCaseTypeId);
            entity.RequestReasonId = NormalizeGuid(request.RequestReasonId);
            entity.WorkflowDefinitionId = NormalizeGuid(request.WorkflowDefinitionId);
            entity.ActionType = actionType.ActionTypeName;
            entity.ActionLevel = NormalizeNullableText(request.ActionLevel) ?? actionType.DefaultActionLevel;
            entity.ActionDate = request.ActionDate;
            entity.EffectiveStartDate = request.EffectiveStartDate;
            entity.EffectiveEndDate = request.EffectiveEndDate;
            entity.Subject = request.Subject.Trim();
            entity.Reason = NormalizeNullableText(request.Reason);
            entity.DecisionSummary = NormalizeNullableText(request.DecisionSummary);
            entity.ConfidentialNotes = NormalizeNullableText(request.ConfidentialNotes);
            entity.IsConfidential = request.IsConfidential;
            entity.AccessClassification = NormalizeClassification(request.AccessClassification);
            entity.RequiresEnhancedAudit = request.RequiresEnhancedAudit;
            entity.IsActive = request.IsActive;
            entity.Description = NormalizeNullableText(request.Description);
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetById(workforceProfileId, id, cancellationToken);
        }

        [HttpPatch("{id:guid}/status")]
        [AccessAction("Update", "Update Workforce Disciplinary Action Status", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("WorkforceDisciplinaryAction", "Update")]
        public async Task<IActionResult> UpdateStatus(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWfpDisciplinaryActionStatusRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await FindEntityAsync(workforceProfileId, id, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Tindakan disiplin tidak ditemukan."));

            if (!AllowedStatuses.Contains(request.ActionStatus))
                return BadRequest(ApiResponse<object>.Fail(400, "Status tindakan disiplin tidak valid."));

            entity.ActionStatus = NormalizeAllowed(request.ActionStatus, AllowedStatuses);
            entity.EffectiveEndDate = request.EffectiveEndDate ?? entity.EffectiveEndDate;
            entity.IsActive = !string.Equals(entity.ActionStatus, "Cancelled", StringComparison.OrdinalIgnoreCase);

            if (string.Equals(entity.ActionStatus, "Approved", StringComparison.OrdinalIgnoreCase))
                entity.ApprovedByUserId = GetCurrentUserId();

            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<object>.Ok(null, "Status tindakan disiplin berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/acknowledge")]
        [AccessAction("Update", "Acknowledge Workforce Disciplinary Action", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("WorkforceDisciplinaryAction", "Update")]
        public async Task<IActionResult> Acknowledge(
            Guid workforceProfileId,
            Guid id,
            [FromBody] AcknowledgeWfpDisciplinaryActionRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await FindEntityAsync(workforceProfileId, id, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Tindakan disiplin tidak ditemukan."));

            entity.IsAcknowledged = request.IsAcknowledged;
            entity.AcknowledgedAt = request.IsAcknowledged ? DateTime.UtcNow : null;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<object>.Ok(null, "Status acknowledge berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/appeal")]
        [AccessAction("Update", "Appeal Workforce Disciplinary Action", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("WorkforceDisciplinaryAction", "Update")]
        public async Task<IActionResult> Appeal(
            Guid workforceProfileId,
            Guid id,
            [FromBody] AppealWfpDisciplinaryActionRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await FindEntityAsync(workforceProfileId, id, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Tindakan disiplin tidak ditemukan."));

            if (request.IsAppealed && string.IsNullOrWhiteSpace(request.AppealStatus))
                request.AppealStatus = "Submitted";

            if (!string.IsNullOrWhiteSpace(request.AppealStatus) && !AllowedAppealStatuses.Contains(request.AppealStatus))
                return BadRequest(ApiResponse<object>.Fail(400, "Status banding tidak valid."));

            entity.IsAppealed = request.IsAppealed;
            entity.AppealStatus = request.IsAppealed ? NormalizeAllowed(request.AppealStatus!, AllowedAppealStatuses) : null;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<object>.Ok(null, "Status banding berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete Workforce Disciplinary Action", AccessType = AccessTypes.Delete, SortOrder = 7)]
        [AccessPermission("WorkforceDisciplinaryAction", "Delete")]
        public async Task<IActionResult> Delete(Guid workforceProfileId, Guid id, CancellationToken cancellationToken)
        {
            var entity = await FindEntityAsync(workforceProfileId, id, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Tindakan disiplin tidak ditemukan."));

            if (!string.Equals(entity.ActionStatus, "Draft", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(entity.ActionStatus, "Cancelled", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(ApiResponse<object>.Fail(400, "Hanya data Draft atau Cancelled yang dapat dihapus."));
            }

            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();
            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actor;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<object>.Ok(null, "Tindakan disiplin berhasil dihapus."));
        }

        private IQueryable<WfpDisciplinaryAction> BuildBaseQuery(Guid workforceProfileId)
        {
            return _dbContext.Set<WfpDisciplinaryAction>()
                .AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.Employee)
                .Include(x => x.DisciplinaryActionType)
                .Include(x => x.ViolationType)
                .Include(x => x.SanctionType)
                .Include(x => x.EmployeeRelationCaseType)
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);
        }

        private static IQueryable<WfpDisciplinaryAction> ApplyFilter(
            IQueryable<WfpDisciplinaryAction> query,
            Guid? disciplinaryActionTypeId,
            Guid? violationTypeId,
            Guid? sanctionTypeId,
            string? actionStatus,
            bool? isAcknowledged,
            bool? isAppealed,
            bool? isConfidential,
            bool? isActive,
            string? search)
        {
            if (disciplinaryActionTypeId.HasValue) query = query.Where(x => x.DisciplinaryActionTypeId == disciplinaryActionTypeId.Value);
            if (violationTypeId.HasValue) query = query.Where(x => x.ViolationTypeId == violationTypeId.Value);
            if (sanctionTypeId.HasValue) query = query.Where(x => x.SanctionTypeId == sanctionTypeId.Value);
            if (!string.IsNullOrWhiteSpace(actionStatus)) query = query.Where(x => x.ActionStatus == actionStatus.Trim());
            if (isAcknowledged.HasValue) query = query.Where(x => x.IsAcknowledged == isAcknowledged.Value);
            if (isAppealed.HasValue) query = query.Where(x => x.IsAppealed == isAppealed.Value);
            if (isConfidential.HasValue) query = query.Where(x => x.IsConfidential == isConfidential.Value);
            if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.ActionCode.ToLower().Contains(keyword) ||
                    x.Subject.ToLower().Contains(keyword) ||
                    x.ActionType.ToLower().Contains(keyword) ||
                    (x.Reason != null && x.Reason.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<WfpDisciplinaryAction> ApplySorting(
            IQueryable<WfpDisciplinaryAction> query,
            string? sortBy,
            string? sortDirection)
        {
            var descending = !string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "actionDate").Trim().ToLowerInvariant() switch
            {
                "actioncode" => descending ? query.OrderByDescending(x => x.ActionCode) : query.OrderBy(x => x.ActionCode),
                "subject" => descending ? query.OrderByDescending(x => x.Subject) : query.OrderBy(x => x.Subject),
                "actionstatus" => descending ? query.OrderByDescending(x => x.ActionStatus) : query.OrderBy(x => x.ActionStatus),
                "isacknowledged" => descending ? query.OrderByDescending(x => x.IsAcknowledged) : query.OrderBy(x => x.IsAcknowledged),
                "createdatetime" => descending ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                _ => descending ? query.OrderByDescending(x => x.ActionDate) : query.OrderBy(x => x.ActionDate)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid workforceProfileId,
            CreateWfpDisciplinaryActionRequest request,
            CancellationToken cancellationToken)
        {
            if (!await WorkforceExistsAsync(workforceProfileId, cancellationToken))
                return (false, "Profil tenaga kerja tidak ditemukan atau tidak aktif.");

            if (request.DisciplinaryActionTypeId == Guid.Empty ||
                !await ActiveMasterExistsAsync<MstDisciplinaryActionType>(request.DisciplinaryActionTypeId, cancellationToken))
                return (false, "Disciplinary action type tidak valid atau tidak aktif.");

            if (request.ViolationTypeId.HasValue &&
                !await ActiveMasterExistsAsync<MstViolationType>(request.ViolationTypeId.Value, cancellationToken))
                return (false, "Violation type tidak valid atau tidak aktif.");

            if (request.SanctionTypeId.HasValue &&
                !await ActiveMasterExistsAsync<MstSanctionType>(request.SanctionTypeId.Value, cancellationToken))
                return (false, "Sanction type tidak valid atau tidak aktif.");

            if (request.EmployeeRelationCaseTypeId.HasValue &&
                !await ActiveMasterExistsAsync<MstEmployeeRelationCaseType>(request.EmployeeRelationCaseTypeId.Value, cancellationToken))
                return (false, "Employee relation case type tidak valid atau tidak aktif.");

            if (string.IsNullOrWhiteSpace(request.Subject))
                return (false, "Subject wajib diisi.");

            if (request.EffectiveStartDate.HasValue && request.EffectiveEndDate.HasValue &&
                request.EffectiveEndDate.Value < request.EffectiveStartDate.Value)
                return (false, "EffectiveEndDate tidak boleh lebih kecil dari EffectiveStartDate.");

            if (!AllowedClassifications.Contains(request.AccessClassification))
                return (false, "Access classification tidak valid.");

            if (request.EmployeeId.HasValue)
            {
                var validEmployee = await _dbContext.MstEmployees.AsNoTracking().AnyAsync(x =>
                    x.Id == request.EmployeeId.Value &&
                    x.WorkforceProfileId == workforceProfileId &&
                    x.IsActive && !x.IsDelete,
                    cancellationToken);
                if (!validEmployee) return (false, "Employee tidak sesuai dengan workforce profile.");
            }

            return (true, null);
        }

        private async Task<bool> ActiveMasterExistsAsync<T>(Guid id, CancellationToken cancellationToken)
            where T : QuilvianSystemBackend.Models.IdentityModel
        {
            return await _dbContext.Set<T>().AsNoTracking().AnyAsync(x => EF.Property<Guid>(x, "Id") == id && !x.IsDelete, cancellationToken);
        }

        private async Task<bool> WorkforceExistsAsync(Guid id, CancellationToken cancellationToken)
        {
            return id != Guid.Empty && await _dbContext.MstWorkforceProfiles.AsNoTracking().AnyAsync(x => x.Id == id && x.IsActive && !x.IsDelete, cancellationToken);
        }

        private async Task<WfpDisciplinaryAction?> FindEntityAsync(Guid workforceProfileId, Guid id, CancellationToken cancellationToken)
        {
            return await _dbContext.Set<WfpDisciplinaryAction>().FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete, cancellationToken);
        }

        private async Task<string> GenerateCodeAsync(CancellationToken cancellationToken)
        {
            var codes = await _dbContext.Set<WfpDisciplinaryAction>().AsNoTracking()
                .Where(x => !x.IsDelete && x.ActionCode.StartsWith(CodePrefix))
                .Select(x => x.ActionCode)
                .ToListAsync(cancellationToken);
            var used = codes.Select(x => x.Replace(CodePrefix, string.Empty)).Where(x => int.TryParse(x, out _)).Select(int.Parse).ToHashSet();
            var next = 1;
            while (used.Contains(next)) next++;
            return CodePrefix + next.ToString("D5");
        }

        private static WfpDisciplinaryActionResponse MapResponse(WfpDisciplinaryAction x)
        {
            return new WfpDisciplinaryActionResponse
            {
                Id = x.Id,
                WorkforceProfileId = x.WorkforceProfileId,
                WorkforceProfileCode = x.WorkforceProfile?.ProfileCode ?? string.Empty,
                WorkforceDisplayName = x.WorkforceProfile?.DisplayName ?? string.Empty,
                EmployeeId = x.EmployeeId,
                EmployeeCode = x.Employee?.EmployeeCode,
                EmployeeName = x.Employee?.FullName,
                DisciplinaryActionTypeId = x.DisciplinaryActionTypeId,
                DisciplinaryActionTypeCode = x.DisciplinaryActionType?.ActionTypeCode,
                DisciplinaryActionTypeName = x.DisciplinaryActionType?.ActionTypeName,
                ViolationTypeId = x.ViolationTypeId,
                ViolationTypeName = x.ViolationType?.ViolationTypeName,
                SanctionTypeId = x.SanctionTypeId,
                SanctionTypeName = x.SanctionType?.SanctionTypeName,
                EmployeeRelationCaseTypeId = x.EmployeeRelationCaseTypeId,
                EmployeeRelationCaseTypeName = x.EmployeeRelationCaseType?.CaseTypeName,
                RequestReasonId = x.RequestReasonId,
                WorkflowDefinitionId = x.WorkflowDefinitionId,
                ActionCode = x.ActionCode,
                ActionType = x.ActionType,
                ActionLevel = x.ActionLevel,
                ActionDate = x.ActionDate,
                EffectiveStartDate = x.EffectiveStartDate,
                EffectiveEndDate = x.EffectiveEndDate,
                Subject = x.Subject,
                ActionStatus = x.ActionStatus,
                IsAcknowledged = x.IsAcknowledged,
                IsAppealed = x.IsAppealed,
                AppealStatus = x.AppealStatus,
                IsConfidential = x.IsConfidential,
                AccessClassification = x.AccessClassification,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime
            };
        }

        private static WfpDisciplinaryActionDetailResponse MapDetailResponse(WfpDisciplinaryAction x)
        {
            var basic = MapResponse(x);
            return new WfpDisciplinaryActionDetailResponse
            {
                Id = basic.Id,
                WorkforceProfileId = basic.WorkforceProfileId,
                WorkforceProfileCode = basic.WorkforceProfileCode,
                WorkforceDisplayName = basic.WorkforceDisplayName,
                EmployeeId = basic.EmployeeId,
                EmployeeCode = basic.EmployeeCode,
                EmployeeName = basic.EmployeeName,
                DisciplinaryActionTypeId = basic.DisciplinaryActionTypeId,
                DisciplinaryActionTypeCode = basic.DisciplinaryActionTypeCode,
                DisciplinaryActionTypeName = basic.DisciplinaryActionTypeName,
                ViolationTypeId = basic.ViolationTypeId,
                ViolationTypeName = basic.ViolationTypeName,
                SanctionTypeId = basic.SanctionTypeId,
                SanctionTypeName = basic.SanctionTypeName,
                EmployeeRelationCaseTypeId = basic.EmployeeRelationCaseTypeId,
                EmployeeRelationCaseTypeName = basic.EmployeeRelationCaseTypeName,
                RequestReasonId = basic.RequestReasonId,
                WorkflowDefinitionId = basic.WorkflowDefinitionId,
                ActionCode = basic.ActionCode,
                ActionType = basic.ActionType,
                ActionLevel = basic.ActionLevel,
                ActionDate = basic.ActionDate,
                EffectiveStartDate = basic.EffectiveStartDate,
                EffectiveEndDate = basic.EffectiveEndDate,
                Subject = basic.Subject,
                ActionStatus = basic.ActionStatus,
                IsAcknowledged = basic.IsAcknowledged,
                IsAppealed = basic.IsAppealed,
                AppealStatus = basic.AppealStatus,
                IsConfidential = basic.IsConfidential,
                AccessClassification = basic.AccessClassification,
                IsActive = basic.IsActive,
                CreateDateTime = basic.CreateDateTime,
                OrganizationAssignmentId = x.OrganizationAssignmentId,
                DisciplinaryCaseId = x.DisciplinaryCaseId,
                DisciplinaryDecisionId = x.DisciplinaryDecisionId,
                IncidentReportId = x.IncidentReportId,
                Reason = x.Reason,
                DecisionSummary = x.DecisionSummary,
                ConfidentialNotes = x.ConfidentialNotes,
                AcknowledgedAt = x.AcknowledgedAt,
                RequiresEnhancedAudit = x.RequiresEnhancedAudit,
                IssuedByUserId = x.IssuedByUserId,
                ApprovedByUserId = x.ApprovedByUserId,
                Description = x.Description,
                UpdateDateTime = x.UpdateDateTime
            };
        }

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue("user_id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }

        private static Guid? NormalizeGuid(Guid? value) => !value.HasValue || value.Value == Guid.Empty ? null : value.Value;
        private static string? NormalizeNullableText(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        private static string NormalizeClassification(string value) => NormalizeAllowed(value, AllowedClassifications);
        private static string NormalizeAllowed(string value, HashSet<string> allowed) => allowed.First(x => x.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize) => (pageNumber < 1 ? 1 : pageNumber, pageSize < 1 ? 25 : Math.Min(pageSize, 100));
        private static List<WfpDisciplinaryActionStringOptionResponse> BuildOptions(IEnumerable<string> values) => values.OrderBy(x => x).Select(x => new WfpDisciplinaryActionStringOptionResponse { Value = x, Label = x }).ToList();
    }
}
