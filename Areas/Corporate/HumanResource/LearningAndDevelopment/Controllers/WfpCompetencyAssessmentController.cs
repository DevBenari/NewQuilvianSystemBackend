using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Enums.HumanResource;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using CompetencyAssessmentPagedResult = QuilvianSystemBackend.Responses.PagedResult<QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.DTOs.WfpCompetencyAssessmentResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/competency-assessments")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_LEARNING_DEVELOPMENT",
        moduleName: "Human Resource Learning and Development",
        displayName: "Workforce Competency Assessment",
        AreaName = "Corporate",
        ControllerName = "WorkforceCompetencyAssessment",
        Description = "Corporate human resource workforce competency assessment",
        SortOrder = 2)]
    [Tags("Corporate / Human Resource / Learning and Development / Competency Assessment")]
    public class WfpCompetencyAssessmentController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.LearningAndDevelopment";
        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public WfpCompetencyAssessmentController(ApplicationDbContext dbContext, LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<WfpCompetencyAssessmentFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Competency Assessment", Description = "Melihat metadata filter asesmen kompetensi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceCompetencyAssessment", "Read")]
        public async Task<IActionResult> GetFilterMetadata(Guid workforceProfileId, CancellationToken cancellationToken)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
                return WorkforceProfileNotFound();

            var competencyOptions = await _dbContext.Set<MstCompetency>()
                .AsNoTracking()
                .Where(x => x.IsActive && !x.IsDelete)
                .OrderBy(x => x.CompetencyName)
                .Select(x => new WfpCompetencyAssessmentMasterOptionResponse
                {
                    Id = x.Id,
                    Code = x.CompetencyCode,
                    Name = x.CompetencyName
                })
                .ToListAsync(cancellationToken);

            var result = new WfpCompetencyAssessmentFilterMetadataResponse
            {
                DefaultFilter = new WfpCompetencyAssessmentDefaultFilterResponse(),
                CustomPeriods = BuildPeriods(),
                CompetencyLevelOptions = BuildEnumOptions<CompetencyLevel>(),
                ResultStatusOptions = BuildEnumOptions<CompetencyAssessmentResultStatus>(),
                CompetencyOptions = competencyOptions,
                SortOptions = new List<WfpCompetencyAssessmentSortOptionResponse>
                {
                    new() { Value = "assessmentDate", Label = "Tanggal asesmen" },
                    new() { Value = "competencyName", Label = "Nama kompetensi" },
                    new() { Value = "competencyLevel", Label = "Level kompetensi" },
                    new() { Value = "resultStatus", Label = "Hasil asesmen" },
                    new() { Value = "score", Label = "Nilai" },
                    new() { Value = "expiredDate", Label = "Tanggal kedaluwarsa" },
                    new() { Value = "isVerified", Label = "Status verifikasi" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            return Ok(ApiResponse<WfpCompetencyAssessmentFilterMetadataResponse>.Ok(result, "Metadata filter asesmen kompetensi berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<WfpCompetencyAssessmentSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Competency Assessment", Description = "Melihat ringkasan asesmen kompetensi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceCompetencyAssessment", "Read")]
        public async Task<IActionResult> GetSummary(Guid workforceProfileId, CancellationToken cancellationToken)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
                return WorkforceProfileNotFound();

            var now = DateTime.UtcNow;
            var soon = now.AddDays(90);
            var query = _dbContext.Set<WfpCompetencyAssessment>().AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            var scored = query.Where(x => x.Score.HasValue && x.MaximumScore.HasValue && x.MaximumScore > 0);
            decimal? averagePercentage = await scored.AnyAsync(cancellationToken)
                ? await scored.AverageAsync(
                    x => x.Score!.Value / x.MaximumScore!.Value * 100m,
                    cancellationToken)
                : (decimal?)null;

            var result = new WfpCompetencyAssessmentSummaryResponse
            {
                TotalAssessment = await query.CountAsync(cancellationToken),
                ActiveAssessment = await query.CountAsync(x => x.IsActive, cancellationToken),
                InactiveAssessment = await query.CountAsync(x => !x.IsActive, cancellationToken),
                VerifiedAssessment = await query.CountAsync(x => x.IsVerified, cancellationToken),
                UnverifiedAssessment = await query.CountAsync(x => !x.IsVerified, cancellationToken),
                ExpiredAssessment = await query.CountAsync(x => x.ExpiredDate.HasValue && x.ExpiredDate < now, cancellationToken),
                ExpiringSoonAssessment = await query.CountAsync(x => x.ExpiredDate.HasValue && x.ExpiredDate >= now && x.ExpiredDate <= soon, cancellationToken),
                AverageScorePercentage = averagePercentage
            };

            return Ok(ApiResponse<WfpCompetencyAssessmentSummaryResponse>.Ok(result, "Ringkasan asesmen kompetensi berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<CompetencyAssessmentPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Competency Assessment", Description = "Melihat data asesmen kompetensi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceCompetencyAssessment", "Read")]
        public async Task<IActionResult> GetAssessments(
            Guid workforceProfileId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? competencyId,
            [FromQuery] CompetencyLevel? competencyLevel,
            [FromQuery] CompetencyAssessmentResultStatus? resultStatus,
            [FromQuery] bool? isVerified,
            [FromQuery] bool? isExpired,
            [FromQuery] bool? isActive,
            [FromQuery] int? expiringWithinDays,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "assessmentDate",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
                return WorkforceProfileNotFound();

            NormalizePaging(ref pageNumber, ref pageSize);
            var query = BuildBaseQuery(workforceProfileId);
            query = ApplyFilters(query, startDate, endDate, customPeriod, competencyId, competencyLevel, resultStatus, isVerified, isExpired, isActive, expiringWithinDays, search);

            var totalData = await query.CountAsync(cancellationToken);
            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var actorMap = await BuildActorMapAsync(entities.SelectMany(x => new[] { x.CreateBy, x.AssessedByUserId ?? Guid.Empty, x.VerifiedByUserId ?? Guid.Empty }), cancellationToken);
            var items = entities.Select(x => MapResponse(x, actorMap)).ToList();

            return Ok(ApiResponse<CompetencyAssessmentPagedResult>.Ok(new CompetencyAssessmentPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            }, "Data asesmen kompetensi berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WfpCompetencyAssessmentDetailResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Competency Assessment", Description = "Melihat detail asesmen kompetensi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceCompetencyAssessment", "Read")]
        public async Task<IActionResult> GetAssessmentById(Guid workforceProfileId, Guid id, CancellationToken cancellationToken)
        {
            var entity = await BuildBaseQuery(workforceProfileId).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Asesmen kompetensi tidak ditemukan."));

            var actorMap = await BuildActorMapAsync(new[] { entity.CreateBy, entity.UpdateBy, entity.AssessedByUserId ?? Guid.Empty, entity.VerifiedByUserId ?? Guid.Empty }, cancellationToken);
            var baseResponse = MapResponse(entity, actorMap);
            var detail = new WfpCompetencyAssessmentDetailResponse
            {
                Id = baseResponse.Id,
                WorkforceProfileId = baseResponse.WorkforceProfileId,
                WorkforceProfileCode = baseResponse.WorkforceProfileCode,
                WorkforceDisplayName = baseResponse.WorkforceDisplayName,
                CompetencyId = baseResponse.CompetencyId,
                CompetencyCode = baseResponse.CompetencyCode,
                CompetencyName = baseResponse.CompetencyName,
                SourceTrainingAssessmentId = baseResponse.SourceTrainingAssessmentId,
                SourceTrainingResultId = baseResponse.SourceTrainingResultId,
                AssessmentDate = baseResponse.AssessmentDate,
                CompetencyLevel = baseResponse.CompetencyLevel,
                ResultStatus = baseResponse.ResultStatus,
                AssessedByUserId = baseResponse.AssessedByUserId,
                AssessedByUserName = baseResponse.AssessedByUserName,
                ExpiredDate = baseResponse.ExpiredDate,
                IsExpired = baseResponse.IsExpired,
                DaysUntilExpiry = baseResponse.DaysUntilExpiry,
                Score = baseResponse.Score,
                MaximumScore = baseResponse.MaximumScore,
                ScorePercentage = baseResponse.ScorePercentage,
                FilePath = baseResponse.FilePath,
                FileContentType = baseResponse.FileContentType,
                HasFile = baseResponse.HasFile,
                Notes = baseResponse.Notes,
                IsVerified = baseResponse.IsVerified,
                VerifiedByUserId = baseResponse.VerifiedByUserId,
                VerifiedByUserName = baseResponse.VerifiedByUserName,
                VerifiedAt = baseResponse.VerifiedAt,
                IsActive = baseResponse.IsActive,
                CreateDateTime = baseResponse.CreateDateTime,
                CreateBy = baseResponse.CreateBy,
                CreateByName = baseResponse.CreateByName,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy,
                UpdateByName = GetActorName(actorMap, entity.UpdateBy)
            };

            return Ok(ApiResponse<WfpCompetencyAssessmentDetailResponse>.Ok(detail, "Detail asesmen kompetensi berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<WfpCompetencyAssessmentDetailResponse>), StatusCodes.Status200OK)]
        [AccessAction("Create", "Create Workforce Competency Assessment", Description = "Membuat asesmen kompetensi", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("WorkforceCompetencyAssessment", "Create")]
        public async Task<IActionResult> CreateAssessment(Guid workforceProfileId, [FromBody] CreateWfpCompetencyAssessmentRequest request, CancellationToken cancellationToken)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
                return WorkforceProfileNotFound();

            var validation = await ValidateRequestAsync(workforceProfileId, null, request, cancellationToken);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(400, validation.ErrorMessage!));

            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();
            var entity = new WfpCompetencyAssessment
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = workforceProfileId,
                CompetencyId = request.CompetencyId,
                SourceTrainingAssessmentId = NormalizeGuid(request.SourceTrainingAssessmentId),
                SourceTrainingResultId = NormalizeGuid(request.SourceTrainingResultId),
                AssessmentDate = NormalizeUtc(request.AssessmentDate),
                CompetencyLevel = request.CompetencyLevel,
                ResultStatus = request.ResultStatus,
                AssessedByUserId = NormalizeGuid(request.AssessedByUserId),
                ExpiredDate = NormalizeUtcNullable(request.ExpiredDate),
                Score = request.Score,
                MaximumScore = request.MaximumScore,
                FilePath = NormalizeText(request.FilePath),
                FileContentType = NormalizeText(request.FileContentType),
                Notes = NormalizeText(request.Notes),
                IsVerified = request.IsVerified,
                VerifiedByUserId = request.IsVerified ? actor : null,
                VerifiedAt = request.IsVerified ? now : null,
                IsActive = request.IsActive,
                CreateDateTime = now,
                CreateBy = actor,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<WfpCompetencyAssessment>().Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await _loggerService.InfoAsync(LogCategory, "WorkforceCompetencyAssessment.Create", "Membuat asesmen kompetensi workforce.", new { entity.Id, entity.WorkforceProfileId, entity.CompetencyId });
            return await GetAssessmentById(workforceProfileId, entity.Id, cancellationToken);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WfpCompetencyAssessmentDetailResponse>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Update Workforce Competency Assessment", Description = "Mengubah asesmen kompetensi", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceCompetencyAssessment", "Update")]
        public async Task<IActionResult> UpdateAssessment(Guid workforceProfileId, Guid id, [FromBody] UpdateWfpCompetencyAssessmentRequest request, CancellationToken cancellationToken)
        {
            var entity = await FindEntityAsync(workforceProfileId, id, cancellationToken);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Asesmen kompetensi tidak ditemukan."));

            var validation = await ValidateRequestAsync(workforceProfileId, id, request, cancellationToken);
            if (!validation.IsValid) return BadRequest(ApiResponse<object>.Fail(400, validation.ErrorMessage!));

            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();
            entity.CompetencyId = request.CompetencyId;
            entity.SourceTrainingAssessmentId = NormalizeGuid(request.SourceTrainingAssessmentId);
            entity.SourceTrainingResultId = NormalizeGuid(request.SourceTrainingResultId);
            entity.AssessmentDate = NormalizeUtc(request.AssessmentDate);
            entity.CompetencyLevel = request.CompetencyLevel;
            entity.ResultStatus = request.ResultStatus;
            entity.AssessedByUserId = NormalizeGuid(request.AssessedByUserId);
            entity.ExpiredDate = NormalizeUtcNullable(request.ExpiredDate);
            entity.Score = request.Score;
            entity.MaximumScore = request.MaximumScore;
            entity.FilePath = NormalizeText(request.FilePath);
            entity.FileContentType = NormalizeText(request.FileContentType);
            entity.Notes = NormalizeText(request.Notes);
            entity.IsVerified = request.IsVerified;
            entity.VerifiedByUserId = request.IsVerified ? entity.VerifiedByUserId ?? actor : null;
            entity.VerifiedAt = request.IsVerified ? entity.VerifiedAt ?? now : null;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetAssessmentById(workforceProfileId, id, cancellationToken);
        }

        [HttpPatch("{id:guid}/status")]
        [AccessAction("Update", "Update Workforce Competency Assessment", Description = "Mengubah status asesmen kompetensi", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceCompetencyAssessment", "Update")]
        public async Task<IActionResult> UpdateStatus(Guid workforceProfileId, Guid id, [FromBody] UpdateWfpCompetencyAssessmentStatusRequest request, CancellationToken cancellationToken)
        {
            var entity = await FindEntityAsync(workforceProfileId, id, cancellationToken);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Asesmen kompetensi tidak ditemukan."));
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();
            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetAssessmentById(workforceProfileId, id, cancellationToken);
        }

        [HttpPatch("{id:guid}/verify")]
        [AccessAction("Update", "Verify Workforce Competency Assessment", Description = "Memverifikasi asesmen kompetensi", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceCompetencyAssessment", "Update")]
        public async Task<IActionResult> Verify(Guid workforceProfileId, Guid id, [FromBody] VerifyWfpCompetencyAssessmentRequest request, CancellationToken cancellationToken)
        {
            var entity = await FindEntityAsync(workforceProfileId, id, cancellationToken);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Asesmen kompetensi tidak ditemukan."));
            if (request.IsVerified && entity.AssessmentDate > DateTime.UtcNow)
                return BadRequest(ApiResponse<object>.Fail(400, "Asesmen dengan tanggal di masa depan tidak dapat diverifikasi."));
            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();
            entity.IsVerified = request.IsVerified;
            entity.VerifiedByUserId = request.IsVerified ? actor : null;
            entity.VerifiedAt = request.IsVerified ? now : null;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetAssessmentById(workforceProfileId, id, cancellationToken);
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete Workforce Competency Assessment", Description = "Menghapus asesmen kompetensi", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("WorkforceCompetencyAssessment", "Delete")]
        public async Task<IActionResult> Delete(Guid workforceProfileId, Guid id, CancellationToken cancellationToken)
        {
            var entity = await FindEntityAsync(workforceProfileId, id, cancellationToken);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Asesmen kompetensi tidak ditemukan."));
            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();
            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actor;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Ok(ApiResponse<object>.Ok(null, "Asesmen kompetensi berhasil dihapus."));
        }

        private IQueryable<WfpCompetencyAssessment> BuildBaseQuery(Guid workforceProfileId) =>
            _dbContext.Set<WfpCompetencyAssessment>().AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.Competency)
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

        private static IQueryable<WfpCompetencyAssessment> ApplyFilters(IQueryable<WfpCompetencyAssessment> query, DateTime? startDate, DateTime? endDate, string? customPeriod, Guid? competencyId, CompetencyLevel? competencyLevel, CompetencyAssessmentResultStatus? resultStatus, bool? isVerified, bool? isExpired, bool? isActive, int? expiringWithinDays, string? search)
        {
            var range = ResolveRange(startDate, endDate, customPeriod);
            if (range.Start.HasValue) query = query.Where(x => x.AssessmentDate >= range.Start.Value);
            if (range.EndExclusive.HasValue) query = query.Where(x => x.AssessmentDate < range.EndExclusive.Value);
            if (competencyId.HasValue && competencyId.Value != Guid.Empty) query = query.Where(x => x.CompetencyId == competencyId.Value);
            if (competencyLevel.HasValue) query = query.Where(x => x.CompetencyLevel == competencyLevel.Value);
            if (resultStatus.HasValue) query = query.Where(x => x.ResultStatus == resultStatus.Value);
            if (isVerified.HasValue) query = query.Where(x => x.IsVerified == isVerified.Value);
            if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);
            var now = DateTime.UtcNow;
            if (isExpired.HasValue)
                query = isExpired.Value ? query.Where(x => x.ExpiredDate.HasValue && x.ExpiredDate < now) : query.Where(x => !x.ExpiredDate.HasValue || x.ExpiredDate >= now);
            if (expiringWithinDays.HasValue && expiringWithinDays.Value >= 0)
            {
                var until = now.AddDays(expiringWithinDays.Value);
                query = query.Where(x => x.ExpiredDate.HasValue && x.ExpiredDate >= now && x.ExpiredDate <= until);
            }
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x => (x.Competency != null && (x.Competency.CompetencyCode.ToLower().Contains(keyword) || x.Competency.CompetencyName.ToLower().Contains(keyword))) ||
                    (x.Notes != null && x.Notes.ToLower().Contains(keyword)));
            }
            return query;
        }

        private static IOrderedQueryable<WfpCompetencyAssessment> ApplySorting(IQueryable<WfpCompetencyAssessment> query, string? sortBy, string? direction)
        {
            var desc = string.Equals(direction, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "assessmentDate").Trim().ToLowerInvariant() switch
            {
                "competencyname" => desc ? query.OrderByDescending(x => x.Competency != null ? x.Competency.CompetencyName : string.Empty) : query.OrderBy(x => x.Competency != null ? x.Competency.CompetencyName : string.Empty),
                "competencylevel" => desc ? query.OrderByDescending(x => x.CompetencyLevel) : query.OrderBy(x => x.CompetencyLevel),
                "resultstatus" => desc ? query.OrderByDescending(x => x.ResultStatus) : query.OrderBy(x => x.ResultStatus),
                "score" => desc ? query.OrderByDescending(x => x.Score) : query.OrderBy(x => x.Score),
                "expireddate" => desc ? query.OrderByDescending(x => x.ExpiredDate) : query.OrderBy(x => x.ExpiredDate),
                "isverified" => desc ? query.OrderByDescending(x => x.IsVerified) : query.OrderBy(x => x.IsVerified),
                "createdatetime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "isactive" => desc ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
                _ => desc ? query.OrderByDescending(x => x.AssessmentDate) : query.OrderBy(x => x.AssessmentDate)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(Guid workforceProfileId, Guid? excludeId, CreateWfpCompetencyAssessmentRequest request, CancellationToken ct)
        {
            if (request.CompetencyId == Guid.Empty) return (false, "Competency wajib dipilih.");
            if (request.AssessmentDate == default) return (false, "Tanggal asesmen wajib diisi.");
            var competencyExists = await _dbContext.Set<MstCompetency>().AsNoTracking().AnyAsync(x => x.Id == request.CompetencyId && x.IsActive && !x.IsDelete, ct);
            if (!competencyExists) return (false, "Competency tidak ditemukan atau tidak aktif.");
            if (request.ExpiredDate.HasValue && request.ExpiredDate.Value < request.AssessmentDate) return (false, "Tanggal kedaluwarsa tidak boleh lebih kecil dari tanggal asesmen.");
            if (request.Score.HasValue && request.Score.Value < 0) return (false, "Nilai tidak boleh negatif.");
            if (request.MaximumScore.HasValue && request.MaximumScore.Value <= 0) return (false, "Maximum score harus lebih besar dari 0.");
            if (request.Score.HasValue && request.MaximumScore.HasValue && request.Score.Value > request.MaximumScore.Value) return (false, "Nilai tidak boleh lebih besar dari maximum score.");
            if (request.AssessedByUserId.HasValue && !await _dbContext.Users.AsNoTracking().AnyAsync(x => x.Id == request.AssessedByUserId.Value, ct)) return (false, "Assessor user tidak ditemukan.");
            if (request.SourceTrainingAssessmentId.HasValue && !await _dbContext.Set<TrxTrainingAssessment>().AsNoTracking().AnyAsync(x => x.Id == request.SourceTrainingAssessmentId.Value && !x.IsDelete, ct)) return (false, "Source training assessment tidak ditemukan.");
            if (request.SourceTrainingResultId.HasValue && !await _dbContext.Set<TrxTrainingResult>().AsNoTracking().AnyAsync(x => x.Id == request.SourceTrainingResultId.Value && !x.IsDelete, ct)) return (false, "Source training result tidak ditemukan.");
            var duplicate = await _dbContext.Set<WfpCompetencyAssessment>().AsNoTracking().AnyAsync(x => x.WorkforceProfileId == workforceProfileId && x.CompetencyId == request.CompetencyId && x.AssessmentDate == NormalizeUtc(request.AssessmentDate) && !x.IsDelete && (!excludeId.HasValue || x.Id != excludeId.Value), ct);
            if (duplicate) return (false, "Asesmen kompetensi dengan competency dan tanggal yang sama sudah tersedia.");
            return (true, null);
        }

        private async Task<bool> WorkforceProfileExistsAsync(Guid id, CancellationToken ct) =>
            await _dbContext.Set<MstWorkforceProfile>().AsNoTracking().AnyAsync(x => x.Id == id && x.IsActive && !x.IsDelete, ct);
        private IActionResult WorkforceProfileNotFound() => NotFound(ApiResponse<object>.Fail(404, "Workforce profile tidak ditemukan atau tidak aktif."));
        private async Task<WfpCompetencyAssessment?> FindEntityAsync(Guid workforceProfileId, Guid id, CancellationToken ct) =>
            await _dbContext.Set<WfpCompetencyAssessment>().FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete, ct);

        private static WfpCompetencyAssessmentResponse MapResponse(WfpCompetencyAssessment x, IReadOnlyDictionary<Guid, string> actors)
        {
            var now = DateTime.UtcNow;
            decimal? scorePercentage =
                x.Score.HasValue &&
                x.MaximumScore.HasValue &&
                x.MaximumScore.Value > 0
                    ? x.Score.Value / x.MaximumScore.Value * 100m
                    : (decimal?)null;
            return new WfpCompetencyAssessmentResponse
            {
                Id = x.Id,
                WorkforceProfileId = x.WorkforceProfileId,
                WorkforceProfileCode = x.WorkforceProfile?.ProfileCode ?? string.Empty,
                WorkforceDisplayName = x.WorkforceProfile?.DisplayName ?? string.Empty,
                CompetencyId = x.CompetencyId,
                CompetencyCode = x.Competency?.CompetencyCode ?? string.Empty,
                CompetencyName = x.Competency?.CompetencyName ?? string.Empty,
                SourceTrainingAssessmentId = x.SourceTrainingAssessmentId,
                SourceTrainingResultId = x.SourceTrainingResultId,
                AssessmentDate = x.AssessmentDate,
                CompetencyLevel = x.CompetencyLevel,
                ResultStatus = x.ResultStatus,
                AssessedByUserId = x.AssessedByUserId,
                AssessedByUserName = x.AssessedByUserId.HasValue ? GetActorName(actors, x.AssessedByUserId.Value) : null,
                ExpiredDate = x.ExpiredDate,
                IsExpired = x.ExpiredDate.HasValue && x.ExpiredDate.Value < now,
                DaysUntilExpiry = x.ExpiredDate.HasValue ? (int)Math.Ceiling((x.ExpiredDate.Value - now).TotalDays) : null,
                Score = x.Score,
                MaximumScore = x.MaximumScore,
                ScorePercentage = scorePercentage,
                FilePath = x.FilePath,
                FileContentType = x.FileContentType,
                HasFile = !string.IsNullOrWhiteSpace(x.FilePath),
                Notes = x.Notes,
                IsVerified = x.IsVerified,
                VerifiedByUserId = x.VerifiedByUserId,
                VerifiedByUserName = x.VerifiedByUserId.HasValue ? GetActorName(actors, x.VerifiedByUserId.Value) : null,
                VerifiedAt = x.VerifiedAt,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime,
                CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                CreateByName = GetActorName(actors, x.CreateBy)
            };
        }

        private async Task<Dictionary<Guid, string>> BuildActorMapAsync(IEnumerable<Guid> ids, CancellationToken ct)
        {
            var valid = ids.Where(x => x != Guid.Empty).Distinct().ToList();
            return await _dbContext.Users.AsNoTracking().Where(x => valid.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode, ct);
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
        private static List<WfpCompetencyAssessmentStringOptionResponse> BuildPeriods() => new()
        {
            new() { Value = "today", Label = "Hari ini" }, new() { Value = "last7days", Label = "7 hari terakhir" },
            new() { Value = "last30days", Label = "30 hari terakhir" }, new() { Value = "thismonth", Label = "Bulan ini" }, new() { Value = "lastmonth", Label = "Bulan lalu" }
        };
        private static List<WfpCompetencyAssessmentEnumOptionResponse> BuildEnumOptions<TEnum>() where TEnum : struct, Enum =>
            Enum.GetValues<TEnum>().Select(x => new WfpCompetencyAssessmentEnumOptionResponse { Value = Convert.ToInt32(x), Name = x.ToString(), Label = x.ToString() }).ToList();
        private static (DateTime? Start, DateTime? EndExclusive) ResolveRange(DateTime? start, DateTime? end, string? period)
        {
            if (start.HasValue || end.HasValue) return (start.HasValue ? DateTime.SpecifyKind(start.Value.Date, DateTimeKind.Utc) : null, end.HasValue ? DateTime.SpecifyKind(end.Value.Date.AddDays(1), DateTimeKind.Utc) : null);
            var today = DateTime.UtcNow.Date;
            return period?.Trim().ToLowerInvariant() switch
            {
                "today" => (today, today.AddDays(1)),
                "last7days" => (today.AddDays(-6), today.AddDays(1)),
                "last30days" => (today.AddDays(-29), today.AddDays(1)),
                "thismonth" => (new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1)),
                "lastmonth" => (new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-1), new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc)),
                _ => (null, null)
            };
        }
    }
}
