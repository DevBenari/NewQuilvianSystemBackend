using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using TrainingRecordPagedResult = QuilvianSystemBackend.Responses.PagedResult<QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.DTOs.WfpTrainingRecordResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/training-records")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_LEARNING_DEVELOPMENT",
        moduleName: "Human Resource Learning and Development",
        displayName: "Workforce Training Record",
        AreaName = "Corporate",
        ControllerName = "WorkforceTrainingRecord",
        Description = "Corporate human resource workforce training record",
        SortOrder = 1)]
    [Tags("Corporate / Human Resource / Learning and Development / Training Record")]
    public class WfpTrainingRecordController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.LearningAndDevelopment";
        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public WfpTrainingRecordController(ApplicationDbContext dbContext, LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<WfpTrainingRecordFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Training Record", Description = "Melihat metadata filter riwayat pelatihan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceTrainingRecord", "Read")]
        public async Task<IActionResult> GetFilterMetadata(Guid workforceProfileId, CancellationToken cancellationToken)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
                return WorkforceProfileNotFound();

            var trainingTypes = await _dbContext.Set<WfpTrainingRecord>()
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete && x.TrainingType != string.Empty)
                .Select(x => x.TrainingType)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync(cancellationToken);

            var trainingCategories = await _dbContext.Set<MstTrainingCategory>()
                .AsNoTracking()
                .Where(x => x.IsActive && !x.IsDelete)
                .OrderBy(x => x.TrainingCategoryName)
                .Select(x => new WfpTrainingCategoryOptionResponse
                {
                    Id = x.Id,
                    TrainingCategoryCode = x.TrainingCategoryCode,
                    TrainingCategoryName = x.TrainingCategoryName,
                    IsMandatoryCategory = x.IsMandatoryCategory
                })
                .ToListAsync(cancellationToken);

            var trainingCatalogs = await _dbContext.Set<MstTrainingCatalog>()
                .AsNoTracking()
                .Include(x => x.TrainingCategory)
                .Where(x => x.IsActive && !x.IsDelete)
                .OrderBy(x => x.TrainingName)
                .Select(x => new WfpTrainingCatalogOptionResponse
                {
                    Id = x.Id,
                    TrainingCode = x.TrainingCode,
                    TrainingName = x.TrainingName,
                    TrainingCategoryId = x.TrainingCategoryId,
                    TrainingCategoryName = x.TrainingCategory != null ? x.TrainingCategory.TrainingCategoryName : string.Empty,
                    TrainingType = x.TrainingType,
                    DeliveryMethod = x.DeliveryMethod,
                    DefaultProviderName = x.DefaultProviderName,
                    DurationHours = x.DurationHours,
                    ValidityMonths = x.ValidityMonths,
                    IsMandatory = x.IsMandatory,
                    RequiresAssessment = x.RequiresAssessment,
                    MinimumPassingScore = x.MinimumPassingScore,
                    IssuesCertificate = x.IssuesCertificate
                })
                .ToListAsync(cancellationToken);

            var today = DateTime.UtcNow.Date;
            var mandatoryRules = await _dbContext.Set<MstMandatoryTrainingRule>()
                .AsNoTracking()
                .Include(x => x.TrainingCatalog)
                .Where(x => x.IsActive && !x.IsDelete
                    && (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value <= today)
                    && (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= today))
                .OrderByDescending(x => x.Priority)
                .ThenBy(x => x.RuleName)
                .Select(x => new WfpMandatoryTrainingRuleOptionResponse
                {
                    Id = x.Id,
                    TrainingCatalogId = x.TrainingCatalogId,
                    TrainingCatalogName = x.TrainingCatalog != null ? x.TrainingCatalog.TrainingName : string.Empty,
                    RuleCode = x.RuleCode,
                    RuleName = x.RuleName,
                    CompletionDueDaysFromJoin = x.CompletionDueDaysFromJoin,
                    RecurrenceMonths = x.RecurrenceMonths,
                    GracePeriodDays = x.GracePeriodDays,
                    RequiresPassingResult = x.RequiresPassingResult,
                    MinimumPassingScore = x.MinimumPassingScore,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    Priority = x.Priority
                })
                .ToListAsync(cancellationToken);

            var result = new WfpTrainingRecordFilterMetadataResponse
            {
                DefaultFilter = new WfpTrainingRecordDefaultFilterResponse(),
                CustomPeriods = BuildPeriods(),
                TrainingTypeOptions = trainingTypes
                    .Concat(trainingCatalogs.Select(x => x.TrainingType))
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(x => x)
                    .Select(x => new WfpTrainingRecordStringOptionResponse { Value = x, Label = x })
                    .ToList(),
                TrainingCatalogOptions = trainingCatalogs,
                TrainingCategoryOptions = trainingCategories,
                MandatoryTrainingRuleOptions = mandatoryRules,
                SortOptions = new List<WfpTrainingRecordSortOptionResponse>
                {
                    new() { Value = "startDate", Label = "Tanggal mulai" },
                    new() { Value = "endDate", Label = "Tanggal selesai" },
                    new() { Value = "trainingName", Label = "Nama pelatihan" },
                    new() { Value = "trainingType", Label = "Tipe pelatihan" },
                    new() { Value = "creditPoint", Label = "Credit point" },
                    new() { Value = "isVerified", Label = "Status verifikasi" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            return Ok(ApiResponse<WfpTrainingRecordFilterMetadataResponse>.Ok(result, "Metadata filter riwayat pelatihan berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<WfpTrainingRecordSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Training Record", Description = "Melihat ringkasan riwayat pelatihan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceTrainingRecord", "Read")]
        public async Task<IActionResult> GetSummary(Guid workforceProfileId, CancellationToken cancellationToken)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
                return WorkforceProfileNotFound();

            var query = _dbContext.Set<WfpTrainingRecord>().AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            var result = new WfpTrainingRecordSummaryResponse
            {
                TotalTraining = await query.CountAsync(cancellationToken),
                ActiveTraining = await query.CountAsync(x => x.IsActive, cancellationToken),
                InactiveTraining = await query.CountAsync(x => !x.IsActive, cancellationToken),
                VerifiedTraining = await query.CountAsync(x => x.IsVerified, cancellationToken),
                UnverifiedTraining = await query.CountAsync(x => !x.IsVerified, cancellationToken),
                MandatoryTraining = await query.CountAsync(x => x.IsMandatory, cancellationToken),
                ExternalTraining = await query.CountAsync(x => x.IsExternalTraining, cancellationToken),
                TotalCreditPoint = await query.SumAsync(x => x.CreditPoint, cancellationToken)
            };

            return Ok(ApiResponse<WfpTrainingRecordSummaryResponse>.Ok(result, "Ringkasan riwayat pelatihan berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<TrainingRecordPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Training Record", Description = "Melihat data riwayat pelatihan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceTrainingRecord", "Read")]
        public async Task<IActionResult> GetTrainingRecords(
            Guid workforceProfileId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? trainingCatalogId,
            [FromQuery] Guid? trainingCategoryId,
            [FromQuery] string? trainingType,
            [FromQuery] bool? isVerified,
            [FromQuery] bool? isMandatory,
            [FromQuery] bool? isExternalTraining,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "startDate",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
                return WorkforceProfileNotFound();

            NormalizePaging(ref pageNumber, ref pageSize);
            var query = BuildBaseQuery(workforceProfileId);
            query = ApplyFilters(query, startDate, endDate, customPeriod, trainingCatalogId, trainingCategoryId, trainingType, isVerified, isMandatory, isExternalTraining, isActive, search);

            var totalData = await query.CountAsync(cancellationToken);
            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var actorMap = await BuildActorMapAsync(entities.SelectMany(x => new[] { x.CreateBy, x.VerifiedByUserId ?? Guid.Empty }), cancellationToken);
            var items = entities.Select(x => MapResponse(x, actorMap)).ToList();

            return Ok(ApiResponse<TrainingRecordPagedResult>.Ok(new TrainingRecordPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            }, "Data riwayat pelatihan berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WfpTrainingRecordDetailResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Training Record", Description = "Melihat detail riwayat pelatihan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceTrainingRecord", "Read")]
        public async Task<IActionResult> GetTrainingRecordById(Guid workforceProfileId, Guid id, CancellationToken cancellationToken)
        {
            var entity = await BuildBaseQuery(workforceProfileId).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Riwayat pelatihan tidak ditemukan."));

            var actorMap = await BuildActorMapAsync(new[] { entity.CreateBy, entity.UpdateBy, entity.VerifiedByUserId ?? Guid.Empty }, cancellationToken);
            var baseResponse = MapResponse(entity, actorMap);
            var detail = new WfpTrainingRecordDetailResponse
            {
                Id = baseResponse.Id,
                WorkforceProfileId = baseResponse.WorkforceProfileId,
                WorkforceProfileCode = baseResponse.WorkforceProfileCode,
                WorkforceDisplayName = baseResponse.WorkforceDisplayName,
                TrainingCatalogId = baseResponse.TrainingCatalogId,
                TrainingCatalogCode = baseResponse.TrainingCatalogCode,
                TrainingCatalogName = baseResponse.TrainingCatalogName,
                CatalogTrainingType = baseResponse.CatalogTrainingType,
                CatalogDeliveryMethod = baseResponse.CatalogDeliveryMethod,
                CatalogDurationHours = baseResponse.CatalogDurationHours,
                CatalogValidityMonths = baseResponse.CatalogValidityMonths,
                CatalogRequiresAssessment = baseResponse.CatalogRequiresAssessment,
                CatalogMinimumPassingScore = baseResponse.CatalogMinimumPassingScore,
                CatalogIssuesCertificate = baseResponse.CatalogIssuesCertificate,
                TrainingCategoryId = baseResponse.TrainingCategoryId,
                TrainingCategoryCode = baseResponse.TrainingCategoryCode,
                TrainingCategoryName = baseResponse.TrainingCategoryName,
                MandatoryTrainingRuleId = baseResponse.MandatoryTrainingRuleId,
                MandatoryTrainingRuleCode = baseResponse.MandatoryTrainingRuleCode,
                MandatoryTrainingRuleName = baseResponse.MandatoryTrainingRuleName,
                TrainingParticipantId = baseResponse.TrainingParticipantId,
                RequirementCode = baseResponse.RequirementCode,
                TrainingType = baseResponse.TrainingType,
                TrainingName = baseResponse.TrainingName,
                Organizer = baseResponse.Organizer,
                Location = baseResponse.Location,
                StartDate = baseResponse.StartDate,
                EndDate = baseResponse.EndDate,
                CertificateNumber = baseResponse.CertificateNumber,
                CreditPoint = baseResponse.CreditPoint,
                FilePath = baseResponse.FilePath,
                FileContentType = baseResponse.FileContentType,
                HasFile = baseResponse.HasFile,
                IsVerified = baseResponse.IsVerified,
                VerifiedByUserId = baseResponse.VerifiedByUserId,
                VerifiedByUserName = baseResponse.VerifiedByUserName,
                VerifiedAt = baseResponse.VerifiedAt,
                IsMandatory = baseResponse.IsMandatory,
                IsExternalTraining = baseResponse.IsExternalTraining,
                IsActive = baseResponse.IsActive,
                Description = baseResponse.Description,
                CreateDateTime = baseResponse.CreateDateTime,
                CreateBy = baseResponse.CreateBy,
                CreateByName = baseResponse.CreateByName,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy,
                UpdateByName = GetActorName(actorMap, entity.UpdateBy)
            };

            return Ok(ApiResponse<WfpTrainingRecordDetailResponse>.Ok(detail, "Detail riwayat pelatihan berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<WfpTrainingRecordDetailResponse>), StatusCodes.Status200OK)]
        [AccessAction("Create", "Create Workforce Training Record", Description = "Membuat riwayat pelatihan", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("WorkforceTrainingRecord", "Create")]
        public async Task<IActionResult> CreateTrainingRecord(Guid workforceProfileId, [FromBody] CreateWfpTrainingRecordRequest request, CancellationToken cancellationToken)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
                return WorkforceProfileNotFound();

            var validation = await ValidateRequestAsync(workforceProfileId, null, request, cancellationToken);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(400, validation.ErrorMessage!));

            var master = await ResolveTrainingMasterAsync(request, cancellationToken);
            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();
            var entity = new WfpTrainingRecord
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = workforceProfileId,
                TrainingCatalogId = master.Catalog?.Id,
                TrainingCategoryId = master.CategoryId,
                MandatoryTrainingRuleId = master.Rule?.Id,
                TrainingParticipantId = NormalizeGuid(request.TrainingParticipantId),
                RequirementCode = NormalizeText(request.RequirementCode) ?? master.Rule?.RuleCode,
                TrainingType = master.Catalog?.TrainingType ?? request.TrainingType!.Trim(),
                TrainingName = master.Catalog?.TrainingName ?? request.TrainingName!.Trim(),
                Organizer = NormalizeText(request.Organizer) ?? master.Catalog?.DefaultProviderName,
                Location = NormalizeText(request.Location),
                StartDate = NormalizeUtc(request.StartDate),
                EndDate = NormalizeUtcNullable(request.EndDate),
                CertificateNumber = NormalizeText(request.CertificateNumber),
                CreditPoint = request.CreditPoint,
                FilePath = NormalizeText(request.FilePath),
                FileContentType = NormalizeText(request.FileContentType),
                IsVerified = request.IsVerified,
                VerifiedByUserId = request.IsVerified ? actor : null,
                VerifiedAt = request.IsVerified ? now : null,
                IsMandatory = request.IsMandatory || master.Catalog?.IsMandatory == true || master.Rule != null,
                IsExternalTraining = request.IsExternalTraining || string.Equals(master.Catalog?.TrainingType, "External", StringComparison.OrdinalIgnoreCase),
                IsActive = request.IsActive,
                Description = NormalizeText(request.Description),
                CreateDateTime = now,
                CreateBy = actor,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<WfpTrainingRecord>().Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await _loggerService.InfoAsync(LogCategory, "WorkforceTrainingRecord.Create", "Membuat riwayat pelatihan workforce.", new { entity.Id, entity.WorkforceProfileId, entity.TrainingName });
            return await GetTrainingRecordById(workforceProfileId, entity.Id, cancellationToken);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WfpTrainingRecordDetailResponse>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Update Workforce Training Record", Description = "Mengubah riwayat pelatihan", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceTrainingRecord", "Update")]
        public async Task<IActionResult> UpdateTrainingRecord(Guid workforceProfileId, Guid id, [FromBody] UpdateWfpTrainingRecordRequest request, CancellationToken cancellationToken)
        {
            var entity = await FindEntityAsync(workforceProfileId, id, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Riwayat pelatihan tidak ditemukan."));

            var validation = await ValidateRequestAsync(workforceProfileId, id, request, cancellationToken);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(400, validation.ErrorMessage!));

            var master = await ResolveTrainingMasterAsync(request, cancellationToken);
            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();
            entity.TrainingCatalogId = master.Catalog?.Id;
            entity.TrainingCategoryId = master.CategoryId;
            entity.MandatoryTrainingRuleId = master.Rule?.Id;
            entity.TrainingParticipantId = NormalizeGuid(request.TrainingParticipantId);
            entity.RequirementCode = NormalizeText(request.RequirementCode) ?? master.Rule?.RuleCode;
            entity.TrainingType = master.Catalog?.TrainingType ?? request.TrainingType!.Trim();
            entity.TrainingName = master.Catalog?.TrainingName ?? request.TrainingName!.Trim();
            entity.Organizer = NormalizeText(request.Organizer) ?? master.Catalog?.DefaultProviderName;
            entity.Location = NormalizeText(request.Location);
            entity.StartDate = NormalizeUtc(request.StartDate);
            entity.EndDate = NormalizeUtcNullable(request.EndDate);
            entity.CertificateNumber = NormalizeText(request.CertificateNumber);
            entity.CreditPoint = request.CreditPoint;
            entity.FilePath = NormalizeText(request.FilePath);
            entity.FileContentType = NormalizeText(request.FileContentType);
            entity.IsVerified = request.IsVerified;
            entity.VerifiedByUserId = request.IsVerified ? entity.VerifiedByUserId ?? actor : null;
            entity.VerifiedAt = request.IsVerified ? entity.VerifiedAt ?? now : null;
            entity.IsMandatory = request.IsMandatory || master.Catalog?.IsMandatory == true || master.Rule != null;
            entity.IsExternalTraining = request.IsExternalTraining || string.Equals(master.Catalog?.TrainingType, "External", StringComparison.OrdinalIgnoreCase);
            entity.IsActive = request.IsActive;
            entity.Description = NormalizeText(request.Description);
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetTrainingRecordById(workforceProfileId, id, cancellationToken);
        }

        [HttpPatch("{id:guid}/status")]
        [AccessAction("Update", "Update Workforce Training Record", Description = "Mengubah status riwayat pelatihan", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceTrainingRecord", "Update")]
        public async Task<IActionResult> UpdateStatus(Guid workforceProfileId, Guid id, [FromBody] UpdateWfpTrainingRecordStatusRequest request, CancellationToken cancellationToken)
        {
            var entity = await FindEntityAsync(workforceProfileId, id, cancellationToken);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Riwayat pelatihan tidak ditemukan."));
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();
            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetTrainingRecordById(workforceProfileId, id, cancellationToken);
        }

        [HttpPatch("{id:guid}/verify")]
        [AccessAction("Update", "Verify Workforce Training Record", Description = "Memverifikasi riwayat pelatihan", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceTrainingRecord", "Update")]
        public async Task<IActionResult> Verify(Guid workforceProfileId, Guid id, [FromBody] VerifyWfpTrainingRecordRequest request, CancellationToken cancellationToken)
        {
            var entity = await FindEntityAsync(workforceProfileId, id, cancellationToken);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Riwayat pelatihan tidak ditemukan."));
            if (request.IsVerified && entity.EndDate.HasValue && entity.EndDate.Value > DateTime.UtcNow)
                return BadRequest(ApiResponse<object>.Fail(400, "Pelatihan yang belum selesai tidak dapat diverifikasi."));
            var now = DateTime.UtcNow;
            entity.IsVerified = request.IsVerified;
            entity.VerifiedByUserId = request.IsVerified ? GetCurrentUserId() : null;
            entity.VerifiedAt = request.IsVerified ? now : null;
            entity.UpdateDateTime = now;
            entity.UpdateBy = GetCurrentUserId();
            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetTrainingRecordById(workforceProfileId, id, cancellationToken);
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete Workforce Training Record", Description = "Menghapus riwayat pelatihan", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("WorkforceTrainingRecord", "Delete")]
        public async Task<IActionResult> Delete(Guid workforceProfileId, Guid id, CancellationToken cancellationToken)
        {
            var entity = await FindEntityAsync(workforceProfileId, id, cancellationToken);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Riwayat pelatihan tidak ditemukan."));
            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();
            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actor;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Ok(ApiResponse<object>.Ok(null, "Riwayat pelatihan berhasil dihapus."));
        }

        private IQueryable<WfpTrainingRecord> BuildBaseQuery(Guid workforceProfileId) =>
            _dbContext.Set<WfpTrainingRecord>()
                .AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.TrainingCatalog)
                    .ThenInclude(x => x!.TrainingCategory)
                .Include(x => x.TrainingCategory)
                .Include(x => x.MandatoryTrainingRule)
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

        private static IQueryable<WfpTrainingRecord> ApplyFilters(IQueryable<WfpTrainingRecord> query, DateTime? startDate, DateTime? endDate, string? customPeriod, Guid? trainingCatalogId, Guid? trainingCategoryId, string? trainingType, bool? isVerified, bool? isMandatory, bool? isExternalTraining, bool? isActive, string? search)
        {
            var range = ResolveRange(startDate, endDate, customPeriod);
            if (range.Start.HasValue) query = query.Where(x => x.StartDate >= range.Start.Value);
            if (range.EndExclusive.HasValue) query = query.Where(x => x.StartDate < range.EndExclusive.Value);
            if (trainingCatalogId.HasValue && trainingCatalogId.Value != Guid.Empty) query = query.Where(x => x.TrainingCatalogId == trainingCatalogId);
            if (trainingCategoryId.HasValue && trainingCategoryId.Value != Guid.Empty) query = query.Where(x => x.TrainingCategoryId == trainingCategoryId);
            if (!string.IsNullOrWhiteSpace(trainingType)) query = query.Where(x => x.TrainingType == trainingType.Trim());
            if (isVerified.HasValue) query = query.Where(x => x.IsVerified == isVerified);
            if (isMandatory.HasValue) query = query.Where(x => x.IsMandatory == isMandatory);
            if (isExternalTraining.HasValue) query = query.Where(x => x.IsExternalTraining == isExternalTraining);
            if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x => x.TrainingName.ToLower().Contains(keyword) || x.TrainingType.ToLower().Contains(keyword) ||
                    (x.Organizer != null && x.Organizer.ToLower().Contains(keyword)) || (x.CertificateNumber != null && x.CertificateNumber.ToLower().Contains(keyword)) ||
                    (x.RequirementCode != null && x.RequirementCode.ToLower().Contains(keyword)));
            }
            return query;
        }

        private static IOrderedQueryable<WfpTrainingRecord> ApplySorting(IQueryable<WfpTrainingRecord> query, string? sortBy, string? direction)
        {
            var desc = string.Equals(direction, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "startDate").Trim().ToLowerInvariant() switch
            {
                "enddate" => desc ? query.OrderByDescending(x => x.EndDate) : query.OrderBy(x => x.EndDate),
                "trainingname" => desc ? query.OrderByDescending(x => x.TrainingName) : query.OrderBy(x => x.TrainingName),
                "trainingtype" => desc ? query.OrderByDescending(x => x.TrainingType) : query.OrderBy(x => x.TrainingType),
                "creditpoint" => desc ? query.OrderByDescending(x => x.CreditPoint) : query.OrderBy(x => x.CreditPoint),
                "isverified" => desc ? query.OrderByDescending(x => x.IsVerified) : query.OrderBy(x => x.IsVerified),
                "createdatetime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "isactive" => desc ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
                _ => desc ? query.OrderByDescending(x => x.StartDate) : query.OrderBy(x => x.StartDate)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(Guid workforceProfileId, Guid? excludeId, CreateWfpTrainingRecordRequest request, CancellationToken ct)
        {
            if (!request.TrainingCatalogId.HasValue && string.IsNullOrWhiteSpace(request.TrainingType))
                return (false, "Training type wajib diisi jika training catalog tidak dipilih.");

            if (!request.TrainingCatalogId.HasValue && string.IsNullOrWhiteSpace(request.TrainingName))
                return (false, "Nama pelatihan wajib diisi jika training catalog tidak dipilih.");

            if (request.StartDate == default)
                return (false, "Tanggal mulai wajib diisi.");

            if (request.EndDate.HasValue && request.EndDate.Value < request.StartDate)
                return (false, "Tanggal selesai tidak boleh lebih kecil dari tanggal mulai.");

            if (request.CreditPoint < 0)
                return (false, "Credit point tidak boleh negatif.");

            MstTrainingCatalog? catalog = null;
            if (request.TrainingCatalogId.HasValue && request.TrainingCatalogId.Value != Guid.Empty)
            {
                catalog = await _dbContext.Set<MstTrainingCatalog>()
                    .AsNoTracking()
                    .Include(x => x.TrainingCategory)
                    .FirstOrDefaultAsync(x => x.Id == request.TrainingCatalogId.Value && x.IsActive && !x.IsDelete, ct);

                if (catalog == null)
                    return (false, "Training catalog tidak ditemukan atau tidak aktif.");

                if (request.TrainingCategoryId.HasValue && request.TrainingCategoryId.Value != Guid.Empty
                    && request.TrainingCategoryId.Value != catalog.TrainingCategoryId)
                    return (false, "Training category tidak sesuai dengan training catalog yang dipilih.");
            }
            else if (request.TrainingCategoryId.HasValue && request.TrainingCategoryId.Value != Guid.Empty
                && !await ActiveExistsAsync<MstTrainingCategory>(request.TrainingCategoryId.Value, ct))
            {
                return (false, "Training category tidak ditemukan atau tidak aktif.");
            }

            if (request.MandatoryTrainingRuleId.HasValue && request.MandatoryTrainingRuleId.Value != Guid.Empty)
            {
                var rule = await _dbContext.Set<MstMandatoryTrainingRule>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == request.MandatoryTrainingRuleId.Value && x.IsActive && !x.IsDelete, ct);

                if (rule == null)
                    return (false, "Mandatory training rule tidak ditemukan atau tidak aktif.");

                var today = DateTime.UtcNow.Date;
                if (rule.EffectiveStartDate.HasValue && rule.EffectiveStartDate.Value.Date > today)
                    return (false, "Mandatory training rule belum berlaku.");

                if (rule.EffectiveEndDate.HasValue && rule.EffectiveEndDate.Value.Date < today)
                    return (false, "Mandatory training rule sudah berakhir.");

                if (catalog != null && rule.TrainingCatalogId != catalog.Id)
                    return (false, "Mandatory training rule tidak sesuai dengan training catalog yang dipilih.");

                if (catalog == null)
                    catalog = await _dbContext.Set<MstTrainingCatalog>()
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == rule.TrainingCatalogId && x.IsActive && !x.IsDelete, ct);

                if (catalog == null)
                    return (false, "Training catalog pada mandatory training rule tidak ditemukan atau tidak aktif.");

                if (rule.RequiresPassingResult && !rule.MinimumPassingScore.HasValue)
                    return (false, "Mandatory training rule membutuhkan hasil lulus tetapi minimum passing score belum ditentukan.");
            }

            if (request.TrainingParticipantId.HasValue && request.TrainingParticipantId.Value != Guid.Empty)
            {
                var exists = await _dbContext.Set<TrxTrainingParticipant>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == request.TrainingParticipantId.Value && !x.IsDelete, ct);

                if (!exists)
                    return (false, "Training participant tidak ditemukan.");

                var duplicate = await _dbContext.Set<WfpTrainingRecord>()
                    .AsNoTracking()
                    .AnyAsync(x => x.TrainingParticipantId == request.TrainingParticipantId
                        && !x.IsDelete
                        && (!excludeId.HasValue || x.Id != excludeId.Value), ct);

                if (duplicate)
                    return (false, "Training participant tersebut sudah memiliki training record.");
            }

            return (true, null);
        }

        private async Task<(MstTrainingCatalog? Catalog, MstMandatoryTrainingRule? Rule, Guid? CategoryId)> ResolveTrainingMasterAsync(
            CreateWfpTrainingRecordRequest request,
            CancellationToken ct)
        {
            MstTrainingCatalog? catalog = null;
            MstMandatoryTrainingRule? rule = null;

            if (request.MandatoryTrainingRuleId.HasValue && request.MandatoryTrainingRuleId.Value != Guid.Empty)
            {
                rule = await _dbContext.Set<MstMandatoryTrainingRule>()
                    .AsNoTracking()
                    .FirstAsync(x => x.Id == request.MandatoryTrainingRuleId.Value, ct);
            }

            var catalogId = NormalizeGuid(request.TrainingCatalogId) ?? rule?.TrainingCatalogId;
            if (catalogId.HasValue)
            {
                catalog = await _dbContext.Set<MstTrainingCatalog>()
                    .AsNoTracking()
                    .FirstAsync(x => x.Id == catalogId.Value, ct);
            }

            var categoryId = catalog?.TrainingCategoryId ?? NormalizeGuid(request.TrainingCategoryId);
            return (catalog, rule, categoryId);
        }

        private async Task<bool> ActiveExistsAsync<TEntity>(Guid id, CancellationToken ct) where TEntity : QuilvianSystemBackend.Models.IdentityModel =>
            await _dbContext.Set<TEntity>().AsNoTracking().AnyAsync(x => EF.Property<Guid>(x, "Id") == id && EF.Property<bool>(x, "IsActive") && !EF.Property<bool>(x, "IsDelete"), ct);

        private async Task<bool> WorkforceProfileExistsAsync(Guid id, CancellationToken ct) =>
            await _dbContext.Set<MstWorkforceProfile>().AsNoTracking().AnyAsync(x => x.Id == id && x.IsActive && !x.IsDelete, ct);

        private IActionResult WorkforceProfileNotFound() => NotFound(ApiResponse<object>.Fail(404, "Workforce profile tidak ditemukan atau tidak aktif."));

        private async Task<WfpTrainingRecord?> FindEntityAsync(Guid workforceProfileId, Guid id, CancellationToken ct) =>
            await _dbContext.Set<WfpTrainingRecord>().FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete, ct);

        private static WfpTrainingRecordResponse MapResponse(WfpTrainingRecord x, IReadOnlyDictionary<Guid, string> actors) => new()
        {
            Id = x.Id,
            WorkforceProfileId = x.WorkforceProfileId,
            WorkforceProfileCode = x.WorkforceProfile?.ProfileCode ?? string.Empty,
            WorkforceDisplayName = x.WorkforceProfile?.DisplayName ?? string.Empty,
            TrainingCatalogId = x.TrainingCatalogId,
            TrainingCatalogCode = x.TrainingCatalog?.TrainingCode,
            TrainingCatalogName = x.TrainingCatalog?.TrainingName,
            CatalogTrainingType = x.TrainingCatalog?.TrainingType,
            CatalogDeliveryMethod = x.TrainingCatalog?.DeliveryMethod,
            CatalogDurationHours = x.TrainingCatalog?.DurationHours,
            CatalogValidityMonths = x.TrainingCatalog?.ValidityMonths,
            CatalogRequiresAssessment = x.TrainingCatalog?.RequiresAssessment,
            CatalogMinimumPassingScore = x.TrainingCatalog?.MinimumPassingScore,
            CatalogIssuesCertificate = x.TrainingCatalog?.IssuesCertificate,
            TrainingCategoryId = x.TrainingCategoryId,
            TrainingCategoryCode = x.TrainingCategory?.TrainingCategoryCode ?? x.TrainingCatalog?.TrainingCategory?.TrainingCategoryCode,
            TrainingCategoryName = x.TrainingCategory?.TrainingCategoryName ?? x.TrainingCatalog?.TrainingCategory?.TrainingCategoryName,
            MandatoryTrainingRuleId = x.MandatoryTrainingRuleId,
            MandatoryTrainingRuleCode = x.MandatoryTrainingRule?.RuleCode,
            MandatoryTrainingRuleName = x.MandatoryTrainingRule?.RuleName,
            TrainingParticipantId = x.TrainingParticipantId,
            RequirementCode = x.RequirementCode,
            TrainingType = x.TrainingType,
            TrainingName = x.TrainingName,
            Organizer = x.Organizer,
            Location = x.Location,
            StartDate = x.StartDate,
            EndDate = x.EndDate,
            CertificateNumber = x.CertificateNumber,
            CreditPoint = x.CreditPoint,
            FilePath = x.FilePath,
            FileContentType = x.FileContentType,
            HasFile = !string.IsNullOrWhiteSpace(x.FilePath),
            IsVerified = x.IsVerified,
            VerifiedByUserId = x.VerifiedByUserId,
            VerifiedByUserName = x.VerifiedByUserId.HasValue ? GetActorName(actors, x.VerifiedByUserId.Value) : null,
            VerifiedAt = x.VerifiedAt,
            IsMandatory = x.IsMandatory,
            IsExternalTraining = x.IsExternalTraining,
            IsActive = x.IsActive,
            Description = x.Description,
            CreateDateTime = x.CreateDateTime,
            CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
            CreateByName = GetActorName(actors, x.CreateBy)
        };

        private async Task<Dictionary<Guid, string>> BuildActorMapAsync(IEnumerable<Guid> ids, CancellationToken ct)
        {
            var valid = ids.Where(x => x != Guid.Empty).Distinct().ToList();
            return await _dbContext.Users.AsNoTracking().Where(x => valid.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode, ct);
        }

        private static string? GetActorName(IReadOnlyDictionary<Guid, string> map, Guid id) => id == Guid.Empty ? null : map.GetValueOrDefault(id);
        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
        private static string? NormalizeText(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        private static Guid? NormalizeGuid(Guid? value) => !value.HasValue || value.Value == Guid.Empty ? null : value;
        private static DateTime NormalizeUtc(DateTime value) => value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);
        private static DateTime? NormalizeUtcNullable(DateTime? value) => value.HasValue ? NormalizeUtc(value.Value) : null;
        private static void NormalizePaging(ref int pageNumber, ref int pageSize) { pageNumber = Math.Max(1, pageNumber); pageSize = pageSize <= 0 ? 25 : Math.Min(pageSize, 100); }
        private static List<WfpTrainingRecordStringOptionResponse> BuildPeriods() => new()
        {
            new() { Value = "today", Label = "Hari ini" }, new() { Value = "last7days", Label = "7 hari terakhir" },
            new() { Value = "last30days", Label = "30 hari terakhir" }, new() { Value = "thismonth", Label = "Bulan ini" }, new() { Value = "lastmonth", Label = "Bulan lalu" }
        };
        private static (DateTime? Start, DateTime? EndExclusive) ResolveRange(DateTime? start, DateTime? end, string? period)
        {
            if (start.HasValue || end.HasValue) return (start.HasValue ? DateTime.SpecifyKind(start.Value.Date, DateTimeKind.Utc) : null, end.HasValue ? DateTime.SpecifyKind(end.Value.Date.AddDays(1), DateTimeKind.Utc) : null);
            var today = DateTime.UtcNow.Date;
            return period?.Trim().ToLowerInvariant() switch
            {
                "today" => (today, today.AddDays(1)), "last7days" => (today.AddDays(-6), today.AddDays(1)), "last30days" => (today.AddDays(-29), today.AddDays(1)),
                "thismonth" => (new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1)),
                "lastmonth" => (new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-1), new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc)),
                _ => (null, null)
            };
        }
    }
}
