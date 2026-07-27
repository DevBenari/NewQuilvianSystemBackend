using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
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
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/dependents")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFORCE_CORE",
        moduleName: "Human Resource Workforce Core",
        displayName: "Workforce Dependent",
        AreaName = "Corporate",
        ControllerName = "WorkforceDependent",
        Description = "Corporate human resource workforce dependent",
        SortOrder = 6
    )]
    [Tags("Corporate / Human Resource / Workforce Core / Dependent")]
    public class WfpDependentController : ControllerBase
    {
        private static readonly string[] DependentTypes =
        {
            "Family", "Tax", "Benefit", "Insurance", "Emergency", "Other"
        };

        private static readonly string[] DependentStatuses =
        {
            "Active", "Inactive", "Suspended", "Ended", "Rejected"
        };

        private const string LogCategory = "Corporate.HumanResource.WorkforceCore";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public WfpDependentController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<WfpDependentFilterMetadataResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Dependent", Description = "Melihat metadata filter tanggungan workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceDependent", "Read")]
        public async Task<IActionResult> GetFilterMetadata(
            Guid workforceProfileId,
            CancellationToken cancellationToken)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Profil tenaga kerja tidak ditemukan."));
            }

            var familyMembers = await _dbContext.Set<WfpFamilyMember>()
                .AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.IsActive &&
                    !x.IsDelete)
                .OrderBy(x => x.FullName)
                .Select(x => new WfpDependentFamilyMemberOptionResponse
                {
                    Id = x.Id,
                    FullName = x.FullName,
                    Relationship = x.Relationship,
                    Label = x.FullName + " - " + x.Relationship
                })
                .ToListAsync(cancellationToken);

            var benefitPlans = await _dbContext.Set<MstBenefitPlan>()
                .AsNoTracking()
                .Where(x => x.IsActive && !x.IsDelete)
                .OrderBy(x => x.BenefitPlanName)
                .Select(x => new WfpDependentBenefitPlanOptionResponse
                {
                    Id = x.Id,
                    BenefitPlanCode = x.BenefitPlanCode,
                    BenefitPlanName = x.BenefitPlanName,
                    Label = x.BenefitPlanName
                })
                .ToListAsync(cancellationToken);

            var result = new WfpDependentFilterMetadataResponse
            {
                DefaultFilter = new WfpDependentDefaultFilterResponse(),
                DependentTypeOptions = DependentTypes
                    .Select(x => new WfpDependentStringOptionResponse { Value = x, Label = BuildDependentTypeLabel(x) })
                    .ToList(),
                DependentStatusOptions = DependentStatuses
                    .Select(x => new WfpDependentStringOptionResponse { Value = x, Label = BuildDependentStatusLabel(x) })
                    .ToList(),
                FamilyMemberOptions = familyMembers,
                BenefitPlanOptions = benefitPlans,
                SortOptions = new List<WfpDependentStringOptionResponse>
                {
                    new() { Value = "effectiveStartDate", Label = "Tanggal mulai berlaku" },
                    new() { Value = "effectiveEndDate", Label = "Tanggal akhir berlaku" },
                    new() { Value = "familyMemberName", Label = "Nama anggota keluarga" },
                    new() { Value = "dependentType", Label = "Jenis tanggungan" },
                    new() { Value = "dependentStatus", Label = "Status tanggungan" },
                    new() { Value = "isBenefitEligible", Label = "Eligible benefit" },
                    new() { Value = "isInsuranceEligible", Label = "Eligible asuransi" },
                    new() { Value = "isActive", Label = "Status aktif" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            return Ok(ApiResponse<WfpDependentFilterMetadataResponse>.Ok(
                result,
                "Metadata filter tanggungan workforce berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<WfpDependentSummaryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Dependent", Description = "Melihat ringkasan tanggungan workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceDependent", "Read")]
        public async Task<IActionResult> GetSummary(
            Guid workforceProfileId,
            CancellationToken cancellationToken)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Profil tenaga kerja tidak ditemukan."));
            }

            var today = DateTime.UtcNow.Date;
            var query = _dbContext.Set<WfpDependent>()
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            var result = new WfpDependentSummaryResponse
            {
                TotalDependent = await query.CountAsync(cancellationToken),
                ActiveDependent = await query.CountAsync(x => x.IsActive, cancellationToken),
                InactiveDependent = await query.CountAsync(x => !x.IsActive, cancellationToken),
                TaxDependent = await query.CountAsync(x => x.IsTaxDependent, cancellationToken),
                BenefitEligibleDependent = await query.CountAsync(x => x.IsBenefitEligible, cancellationToken),
                InsuranceEligibleDependent = await query.CountAsync(x => x.IsInsuranceEligible, cancellationToken),
                CurrentlyEffectiveDependent = await query.CountAsync(x =>
                    x.IsActive &&
                    x.EffectiveStartDate <= today &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= today),
                    cancellationToken),
                EndedDependent = await query.CountAsync(x =>
                    !x.IsActive ||
                    x.DependentStatus == "Ended" ||
                    (x.EffectiveEndDate.HasValue && x.EffectiveEndDate.Value < today),
                    cancellationToken)
            };

            return Ok(ApiResponse<WfpDependentSummaryResponse>.Ok(
                result,
                "Ringkasan tanggungan workforce berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<WfpDependentResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Dependent", Description = "Melihat data tanggungan workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceDependent", "Read")]
        public async Task<IActionResult> GetDependents(
            Guid workforceProfileId,
            [FromQuery] Guid? familyMemberId,
            [FromQuery] Guid? benefitPlanId,
            [FromQuery] string? dependentType,
            [FromQuery] string? dependentStatus,
            [FromQuery] bool? isTaxDependent,
            [FromQuery] bool? isBenefitEligible,
            [FromQuery] bool? isInsuranceEligible,
            [FromQuery] bool? isCurrentlyEffective,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "effectiveStartDate",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Profil tenaga kerja tidak ditemukan."));
            }

            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery(workforceProfileId);
            query = ApplyFilter(
                query,
                familyMemberId,
                benefitPlanId,
                dependentType,
                dependentStatus,
                isTaxDependent,
                isBenefitEligible,
                isInsuranceEligible,
                isCurrentlyEffective,
                isActive,
                search);

            var totalData = await query.CountAsync(cancellationToken);
            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var actorNames = await GetActorNameMapAsync(entities.Select(x => x.CreateBy), cancellationToken);

            return Ok(ApiResponse<PagedResult<WfpDependentResponse>>.Ok(
                new PagedResult<WfpDependentResponse>
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = entities.Select(x => MapResponse(x, actorNames)).ToList()
                },
                "Data tanggungan workforce berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WfpDependentDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Dependent", Description = "Melihat detail tanggungan workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceDependent", "Read")]
        public async Task<IActionResult> GetDependentById(
            Guid workforceProfileId,
            Guid id,
            CancellationToken cancellationToken)
        {
            var entity = await BuildBaseQuery(workforceProfileId)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Tanggungan workforce tidak ditemukan."));
            }

            var actorNames = await GetActorNameMapAsync(new[] { entity.CreateBy, entity.UpdateBy }, cancellationToken);

            return Ok(ApiResponse<WfpDependentDetailResponse>.Ok(
                MapDetailResponse(entity, actorNames),
                "Detail tanggungan workforce berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<WfpDependentDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Create", "Create Workforce Dependent", Description = "Membuat tanggungan workforce", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("WorkforceDependent", "Create")]
        public async Task<IActionResult> CreateDependent(
            Guid workforceProfileId,
            [FromBody] CreateWfpDependentRequest request,
            CancellationToken cancellationToken)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Profil tenaga kerja tidak ditemukan."));
            }

            var validation = await ValidateRequestAsync(workforceProfileId, null, request, cancellationToken);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data tanggungan workforce tidak valid."));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var normalizedStatus = NormalizeDependentStatus(request.DependentStatus);
            var entity = new WfpDependent
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = workforceProfileId,
                FamilyMemberId = NormalizeNullableGuid(request.FamilyMemberId),
                BenefitPlanId = NormalizeNullableGuid(request.BenefitPlanId),
                DependentType = NormalizeDependentType(request.DependentType),
                DependentStatus = normalizedStatus,
                EffectiveStartDate = request.EffectiveStartDate.Date,
                EffectiveEndDate = request.EffectiveEndDate?.Date,
                IsTaxDependent = request.IsTaxDependent,
                IsBenefitEligible = request.IsBenefitEligible,
                IsInsuranceEligible = request.IsInsuranceEligible,
                IsActive = IsTerminalStatus(normalizedStatus) ? false : request.IsActive,
                Description = NormalizeNullableText(request.Description),
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<WfpDependent>().Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceDependent.CreateDependent",
                "Tanggungan workforce berhasil dibuat.",
                new
                {
                    entity.Id,
                    entity.WorkforceProfileId,
                    entity.FamilyMemberId,
                    entity.BenefitPlanId,
                    entity.DependentType,
                    entity.DependentStatus
                });

            return await BuildDetailResultAsync(entity.Id, workforceProfileId, "Tanggungan workforce berhasil dibuat.", cancellationToken);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WfpDependentDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Workforce Dependent", Description = "Mengubah tanggungan workforce", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceDependent", "Update")]
        public async Task<IActionResult> UpdateDependent(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWfpDependentRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<WfpDependent>()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete, cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Tanggungan workforce tidak ditemukan."));
            }

            var validation = await ValidateRequestAsync(workforceProfileId, id, request, cancellationToken);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data tanggungan workforce tidak valid."));
            }

            var normalizedStatus = NormalizeDependentStatus(request.DependentStatus);
            entity.FamilyMemberId = NormalizeNullableGuid(request.FamilyMemberId);
            entity.BenefitPlanId = NormalizeNullableGuid(request.BenefitPlanId);
            entity.DependentType = NormalizeDependentType(request.DependentType);
            entity.DependentStatus = normalizedStatus;
            entity.EffectiveStartDate = request.EffectiveStartDate.Date;
            entity.EffectiveEndDate = request.EffectiveEndDate?.Date;
            entity.IsTaxDependent = request.IsTaxDependent;
            entity.IsBenefitEligible = request.IsBenefitEligible;
            entity.IsInsuranceEligible = request.IsInsuranceEligible;
            entity.IsActive = IsTerminalStatus(normalizedStatus) ? false : request.IsActive;
            entity.Description = NormalizeNullableText(request.Description);
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceDependent.UpdateDependent",
                "Tanggungan workforce berhasil diperbarui.",
                new { entity.Id, entity.WorkforceProfileId, entity.FamilyMemberId, entity.BenefitPlanId, entity.DependentStatus });

            return await BuildDetailResultAsync(entity.Id, workforceProfileId, "Tanggungan workforce berhasil diperbarui.", cancellationToken);
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<WfpDependentDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Workforce Dependent", Description = "Mengubah status tanggungan workforce", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceDependent", "Update")]
        public async Task<IActionResult> UpdateDependentStatus(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWfpDependentStatusRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<WfpDependent>()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete, cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Tanggungan workforce tidak ditemukan."));
            }

            var normalizedStatus = string.IsNullOrWhiteSpace(request.DependentStatus)
                ? entity.DependentStatus
                : NormalizeDependentStatus(request.DependentStatus);

            if (!DependentStatuses.Contains(normalizedStatus, StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Status tanggungan tidak valid."));
            }

            if (request.EffectiveEndDate.HasValue && request.EffectiveEndDate.Value.Date < entity.EffectiveStartDate.Date)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Tanggal akhir berlaku tidak boleh lebih kecil dari tanggal mulai berlaku."));
            }

            entity.DependentStatus = normalizedStatus;
            entity.EffectiveEndDate = request.EffectiveEndDate?.Date;
            entity.IsActive = IsTerminalStatus(normalizedStatus) ? false : request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();
            await _dbContext.SaveChangesAsync(cancellationToken);

            return await BuildDetailResultAsync(entity.Id, workforceProfileId, "Status tanggungan workforce berhasil diperbarui.", cancellationToken);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Workforce Dependent", Description = "Menghapus tanggungan workforce", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("WorkforceDependent", "Delete")]
        public async Task<IActionResult> DeleteDependent(
            Guid workforceProfileId,
            Guid id,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<WfpDependent>()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete, cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Tanggungan workforce tidak ditemukan."));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.IsActive = false;
            entity.IsDelete = true;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceDependent.DeleteDependent",
                "Tanggungan workforce berhasil dihapus.",
                new { entity.Id, entity.WorkforceProfileId, entity.FamilyMemberId, entity.BenefitPlanId });

            return Ok(ApiResponse<object>.Ok(null, "Tanggungan workforce berhasil dihapus."));
        }

        private IQueryable<WfpDependent> BuildBaseQuery(Guid workforceProfileId)
        {
            return _dbContext.Set<WfpDependent>()
                .AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.FamilyMember)
                .Include(x => x.BenefitPlan)
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);
        }

        private static IQueryable<WfpDependent> ApplyFilter(
            IQueryable<WfpDependent> query,
            Guid? familyMemberId,
            Guid? benefitPlanId,
            string? dependentType,
            string? dependentStatus,
            bool? isTaxDependent,
            bool? isBenefitEligible,
            bool? isInsuranceEligible,
            bool? isCurrentlyEffective,
            bool? isActive,
            string? search)
        {
            var today = DateTime.UtcNow.Date;

            if (familyMemberId.HasValue && familyMemberId.Value != Guid.Empty)
                query = query.Where(x => x.FamilyMemberId == familyMemberId.Value);
            if (benefitPlanId.HasValue && benefitPlanId.Value != Guid.Empty)
                query = query.Where(x => x.BenefitPlanId == benefitPlanId.Value);
            if (!string.IsNullOrWhiteSpace(dependentType))
                query = query.Where(x => x.DependentType.ToLower() == dependentType.Trim().ToLower());
            if (!string.IsNullOrWhiteSpace(dependentStatus))
                query = query.Where(x => x.DependentStatus.ToLower() == dependentStatus.Trim().ToLower());
            if (isTaxDependent.HasValue)
                query = query.Where(x => x.IsTaxDependent == isTaxDependent.Value);
            if (isBenefitEligible.HasValue)
                query = query.Where(x => x.IsBenefitEligible == isBenefitEligible.Value);
            if (isInsuranceEligible.HasValue)
                query = query.Where(x => x.IsInsuranceEligible == isInsuranceEligible.Value);
            if (isCurrentlyEffective.HasValue)
            {
                query = isCurrentlyEffective.Value
                    ? query.Where(x => x.IsActive && x.EffectiveStartDate <= today && (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= today))
                    : query.Where(x => !x.IsActive || x.EffectiveStartDate > today || (x.EffectiveEndDate.HasValue && x.EffectiveEndDate.Value < today));
            }
            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.DependentType.ToLower().Contains(keyword) ||
                    x.DependentStatus.ToLower().Contains(keyword) ||
                    (x.FamilyMember != null && x.FamilyMember.FullName.ToLower().Contains(keyword)) ||
                    (x.FamilyMember != null && x.FamilyMember.Relationship.ToLower().Contains(keyword)) ||
                    (x.BenefitPlan != null && x.BenefitPlan.BenefitPlanCode.ToLower().Contains(keyword)) ||
                    (x.BenefitPlan != null && x.BenefitPlan.BenefitPlanName.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<WfpDependent> ApplySorting(
            IQueryable<WfpDependent> query,
            string? sortBy,
            string? sortDirection)
        {
            var desc = !string.Equals(sortDirection?.Trim(), "asc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "effectiveStartDate").Trim().Replace("_", string.Empty).ToLowerInvariant() switch
            {
                "effectiveenddate" => desc ? query.OrderByDescending(x => x.EffectiveEndDate) : query.OrderBy(x => x.EffectiveEndDate),
                "familymembername" => desc
                    ? query.OrderByDescending(x => x.FamilyMember != null ? x.FamilyMember.FullName : string.Empty)
                    : query.OrderBy(x => x.FamilyMember != null ? x.FamilyMember.FullName : string.Empty),
                "dependenttype" => desc ? query.OrderByDescending(x => x.DependentType) : query.OrderBy(x => x.DependentType),
                "dependentstatus" => desc ? query.OrderByDescending(x => x.DependentStatus) : query.OrderBy(x => x.DependentStatus),
                "isbenefiteligible" => desc ? query.OrderByDescending(x => x.IsBenefitEligible) : query.OrderBy(x => x.IsBenefitEligible),
                "isinsuranceeligible" => desc ? query.OrderByDescending(x => x.IsInsuranceEligible) : query.OrderBy(x => x.IsInsuranceEligible),
                "isactive" => desc ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.EffectiveStartDate) : query.OrderBy(x => x.IsActive).ThenByDescending(x => x.EffectiveStartDate),
                "createdatetime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                _ => desc ? query.OrderByDescending(x => x.EffectiveStartDate).ThenByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.EffectiveStartDate).ThenBy(x => x.CreateDateTime)
            };
        }

        private WfpDependentResponse MapResponse(WfpDependent x, IReadOnlyDictionary<Guid, string> actorNames)
        {
            var today = DateTime.UtcNow.Date;
            return new WfpDependentResponse
            {
                Id = x.Id,
                WorkforceProfileId = x.WorkforceProfileId,
                WorkforceProfileCode = x.WorkforceProfile?.ProfileCode ?? string.Empty,
                WorkforceDisplayName = x.WorkforceProfile?.DisplayName ?? string.Empty,
                FamilyMemberId = x.FamilyMemberId,
                FamilyMemberName = x.FamilyMember?.FullName,
                FamilyRelationship = x.FamilyMember?.Relationship,
                BenefitPlanId = x.BenefitPlanId,
                BenefitPlanCode = x.BenefitPlan?.BenefitPlanCode,
                BenefitPlanName = x.BenefitPlan?.BenefitPlanName,
                DependentType = x.DependentType,
                DependentStatus = x.DependentStatus,
                EffectiveStartDate = x.EffectiveStartDate,
                EffectiveEndDate = x.EffectiveEndDate,
                IsCurrentlyEffective = x.IsActive && x.EffectiveStartDate <= today && (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= today),
                IsTaxDependent = x.IsTaxDependent,
                IsBenefitEligible = x.IsBenefitEligible,
                IsInsuranceEligible = x.IsInsuranceEligible,
                IsActive = x.IsActive,
                Description = x.Description,
                CreateDateTime = x.CreateDateTime,
                CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                CreateByName = GetActorName(actorNames, x.CreateBy)
            };
        }

        private WfpDependentDetailResponse MapDetailResponse(WfpDependent x, IReadOnlyDictionary<Guid, string> actorNames)
        {
            var b = MapResponse(x, actorNames);
            return new WfpDependentDetailResponse
            {
                Id = b.Id,
                WorkforceProfileId = b.WorkforceProfileId,
                WorkforceProfileCode = b.WorkforceProfileCode,
                WorkforceDisplayName = b.WorkforceDisplayName,
                FamilyMemberId = b.FamilyMemberId,
                FamilyMemberName = b.FamilyMemberName,
                FamilyRelationship = b.FamilyRelationship,
                BenefitPlanId = b.BenefitPlanId,
                BenefitPlanCode = b.BenefitPlanCode,
                BenefitPlanName = b.BenefitPlanName,
                DependentType = b.DependentType,
                DependentStatus = b.DependentStatus,
                EffectiveStartDate = b.EffectiveStartDate,
                EffectiveEndDate = b.EffectiveEndDate,
                IsCurrentlyEffective = b.IsCurrentlyEffective,
                IsTaxDependent = b.IsTaxDependent,
                IsBenefitEligible = b.IsBenefitEligible,
                IsInsuranceEligible = b.IsInsuranceEligible,
                IsActive = b.IsActive,
                Description = b.Description,
                CreateDateTime = b.CreateDateTime,
                CreateBy = b.CreateBy,
                CreateByName = b.CreateByName,
                UpdateDateTime = x.UpdateDateTime,
                UpdateBy = x.UpdateBy == Guid.Empty ? null : x.UpdateBy,
                UpdateByName = GetActorName(actorNames, x.UpdateBy)
            };
        }

        private async Task<IActionResult> BuildDetailResultAsync(
            Guid id,
            Guid workforceProfileId,
            string message,
            CancellationToken cancellationToken)
        {
            var entity = await BuildBaseQuery(workforceProfileId).FirstAsync(x => x.Id == id, cancellationToken);
            var actorNames = await GetActorNameMapAsync(new[] { entity.CreateBy, entity.UpdateBy }, cancellationToken);
            return Ok(ApiResponse<WfpDependentDetailResponse>.Ok(MapDetailResponse(entity, actorNames), message));
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid workforceProfileId,
            Guid? excludeId,
            CreateWfpDependentRequest request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.DependentType))
                return (false, "Jenis tanggungan wajib diisi.");
            if (!DependentTypes.Contains(request.DependentType.Trim(), StringComparer.OrdinalIgnoreCase))
                return (false, "Jenis tanggungan tidak valid.");
            if (string.IsNullOrWhiteSpace(request.DependentStatus))
                return (false, "Status tanggungan wajib diisi.");
            if (!DependentStatuses.Contains(request.DependentStatus.Trim(), StringComparer.OrdinalIgnoreCase))
                return (false, "Status tanggungan tidak valid.");
            if (request.EffectiveStartDate == default)
                return (false, "Tanggal mulai berlaku wajib diisi.");
            if (request.EffectiveEndDate.HasValue && request.EffectiveEndDate.Value.Date < request.EffectiveStartDate.Date)
                return (false, "Tanggal akhir berlaku tidak boleh lebih kecil dari tanggal mulai berlaku.");

            var familyMemberId = NormalizeNullableGuid(request.FamilyMemberId);
            var benefitPlanId = NormalizeNullableGuid(request.BenefitPlanId);

            if (!familyMemberId.HasValue && !benefitPlanId.HasValue)
                return (false, "FamilyMemberId atau BenefitPlanId minimal salah satu wajib diisi.");

            if (familyMemberId.HasValue)
            {
                var exists = await _dbContext.Set<WfpFamilyMember>()
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id == familyMemberId.Value &&
                        x.WorkforceProfileId == workforceProfileId &&
                        x.IsActive &&
                        !x.IsDelete,
                        cancellationToken);
                if (!exists)
                    return (false, "Anggota keluarga tidak ditemukan, tidak aktif, atau bukan milik workforce ini.");
            }

            if (benefitPlanId.HasValue)
            {
                var exists = await _dbContext.Set<MstBenefitPlan>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == benefitPlanId.Value && x.IsActive && !x.IsDelete, cancellationToken);
                if (!exists)
                    return (false, "Benefit plan tidak ditemukan atau sudah tidak aktif.");
            }

            var newStart = request.EffectiveStartDate.Date;
            var newEnd = request.EffectiveEndDate?.Date;
            var normalizedType = NormalizeDependentType(request.DependentType);

            var overlapExists = await _dbContext.Set<WfpDependent>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.Id != excludeId &&
                    !x.IsDelete &&
                    x.FamilyMemberId == familyMemberId &&
                    x.BenefitPlanId == benefitPlanId &&
                    x.DependentType == normalizedType &&
                    x.EffectiveStartDate <= (newEnd ?? DateTime.MaxValue.Date) &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= newStart),
                    cancellationToken);

            if (overlapExists)
                return (false, "Tanggungan dengan anggota keluarga, benefit plan, jenis, dan periode yang sama atau beririsan sudah tersedia.");

            return (true, null);
        }

        private async Task<bool> WorkforceProfileExistsAsync(Guid workforceProfileId, CancellationToken cancellationToken)
        {
            return workforceProfileId != Guid.Empty && await _dbContext.MstWorkforceProfiles
                .AsNoTracking()
                .AnyAsync(x => x.Id == workforceProfileId && x.IsActive && !x.IsDelete, cancellationToken);
        }

        private async Task<Dictionary<Guid, string>> GetActorNameMapAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
        {
            var userIds = ids.Where(x => x != Guid.Empty).Distinct().ToList();
            if (userIds.Count == 0) return new Dictionary<Guid, string>();
            return await _dbContext.Users.AsNoTracking()
                .Where(x => userIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode, cancellationToken);
        }

        private static string? GetActorName(IReadOnlyDictionary<Guid, string> names, Guid? id)
            => id.HasValue && id.Value != Guid.Empty && names.TryGetValue(id.Value, out var name) ? name : null;

        private Guid GetCurrentUserId()
        {
            var text = User.FindFirstValue("user_id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(text, out var id) ? id : Guid.Empty;
        }

        private static string NormalizeDependentType(string value)
        {
            var match = DependentTypes.FirstOrDefault(x => x.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
            return match ?? value.Trim();
        }

        private static string NormalizeDependentStatus(string value)
        {
            var match = DependentStatuses.FirstOrDefault(x => x.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
            return match ?? value.Trim();
        }

        private static bool IsTerminalStatus(string status)
            => status.Equals("Ended", StringComparison.OrdinalIgnoreCase) || status.Equals("Rejected", StringComparison.OrdinalIgnoreCase);

        private static string BuildDependentTypeLabel(string value) => value switch
        {
            "Family" => "Keluarga",
            "Tax" => "Tanggungan Pajak",
            "Benefit" => "Tanggungan Benefit",
            "Insurance" => "Tanggungan Asuransi",
            "Emergency" => "Kontak Darurat",
            _ => "Lainnya"
        };

        private static string BuildDependentStatusLabel(string value) => value switch
        {
            "Active" => "Aktif",
            "Inactive" => "Tidak Aktif",
            "Suspended" => "Ditangguhkan",
            "Ended" => "Berakhir",
            "Rejected" => "Ditolak",
            _ => value
        };

        private static Guid? NormalizeNullableGuid(Guid? value) => value.HasValue && value.Value != Guid.Empty ? value.Value : null;
        private static string? NormalizeNullableText(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 25 : Math.Min(pageSize, 100);
            return (pageNumber, pageSize);
        }
    }
}
