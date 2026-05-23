using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Enum;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/performance-reviews")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFORCE",
        moduleName: "Human Resource Workforce",
        displayName: "Workforce Performance Review",
        AreaName = "Corporate",
        ControllerName = "WorkforcePerformanceReview",
        Description = "Corporate human resource workforce performance review",
        SortOrder = 13
    )]
    [Tags("Corporate / Human Resource / Workforce / Performance Review")]
    public class WorkforcePerformanceReviewController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.Workforce";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public WorkforcePerformanceReviewController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<WorkforcePerformanceReviewListResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Workforce Performance Review",
            Description = "Melihat data performance review workforce",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforcePerformanceReview", "Read")]
        public async Task<IActionResult> GetPerformanceReviews(Guid workforceProfileId)
        {
            var profile = await GetWorkforceProfileHeaderAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var items = await _dbContext.WfpPerformanceReviews
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete)
                .OrderByDescending(x => x.ReviewDate)
                .ThenByDescending(x => x.CreateDateTime)
                .Select(x => new WorkforcePerformanceReviewResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    ProfileCode = profile.ProfileCode,
                    DisplayName = profile.DisplayName,
                    UserType = profile.UserType,
                    ReviewPeriod = x.ReviewPeriod,
                    ReviewDate = x.ReviewDate,
                    ReviewerUserId = x.ReviewerUserId,
                    ReviewerUserName = x.ReviewerUser != null ? x.ReviewerUser.DisplayName : null,
                    ReviewType = x.ReviewType,
                    TotalScore = x.TotalScore,
                    FinalRating = x.FinalRating,
                    ReviewStatus = x.ReviewStatus,
                    StrengthNotes = x.StrengthNotes,
                    ImprovementNotes = x.ImprovementNotes,
                    RecommendationNotes = x.RecommendationNotes,
                    IsFinalized = x.IsFinalized,
                    FinalizedAt = x.FinalizedAt,
                    IsActive = x.IsActive,
                    DetailCount = x.Details.Count(d => !d.IsDelete),
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

            var finalizedScores = items
                .Where(x => x.IsFinalized)
                .Select(x => x.TotalScore)
                .ToList();

            var result = new WorkforcePerformanceReviewListResponse
            {
                WorkforceProfileId = workforceProfileId,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                TotalData = items.Count,
                ActiveData = items.Count(x => x.IsActive),
                DraftData = items.Count(x => x.ReviewStatus == PerformanceReviewStatus.Draft),
                InProgressData = items.Count(x => x.ReviewStatus == PerformanceReviewStatus.InProgress),
                CompletedData = items.Count(x => x.ReviewStatus == PerformanceReviewStatus.Completed),
                FinalizedData = items.Count(x => x.ReviewStatus == PerformanceReviewStatus.Finalized || x.IsFinalized),
                AverageScore = finalizedScores.Any()
                    ? Math.Round(finalizedScores.Average(), 2)
                    : 0,
                Items = items
            };

            return Ok(ApiResponse<WorkforcePerformanceReviewListResponse>.Ok(
                result,
                "Data performance review workforce berhasil diambil."
            ));
        }

        [HttpGet("{performanceReviewId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforcePerformanceReviewResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Workforce Performance Review",
            Description = "Melihat detail performance review workforce",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforcePerformanceReview", "Read")]
        public async Task<IActionResult> GetPerformanceReviewById(
            Guid workforceProfileId,
            Guid performanceReviewId)
        {
            var profile = await GetWorkforceProfileHeaderAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var data = await _dbContext.WfpPerformanceReviews
                .AsNoTracking()
                .Where(x =>
                    x.Id == performanceReviewId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete)
                .Select(x => new WorkforcePerformanceReviewResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    ProfileCode = profile.ProfileCode,
                    DisplayName = profile.DisplayName,
                    UserType = profile.UserType,
                    ReviewPeriod = x.ReviewPeriod,
                    ReviewDate = x.ReviewDate,
                    ReviewerUserId = x.ReviewerUserId,
                    ReviewerUserName = x.ReviewerUser != null ? x.ReviewerUser.DisplayName : null,
                    ReviewType = x.ReviewType,
                    TotalScore = x.TotalScore,
                    FinalRating = x.FinalRating,
                    ReviewStatus = x.ReviewStatus,
                    StrengthNotes = x.StrengthNotes,
                    ImprovementNotes = x.ImprovementNotes,
                    RecommendationNotes = x.RecommendationNotes,
                    IsFinalized = x.IsFinalized,
                    FinalizedAt = x.FinalizedAt,
                    IsActive = x.IsActive,
                    DetailCount = x.Details.Count(d => !d.IsDelete),
                    CreateDateTime = x.CreateDateTime,
                    Details = x.Details
                        .Where(d => !d.IsDelete)
                        .OrderBy(d => d.CriteriaCode)
                        .Select(d => new WorkforcePerformanceReviewDetailResponse
                        {
                            Id = d.Id,
                            PerformanceReviewId = d.PerformanceReviewId,
                            CriteriaCode = d.CriteriaCode,
                            CriteriaName = d.CriteriaName,
                            Score = d.Score,
                            Weight = d.Weight,
                            WeightedScore = d.WeightedScore,
                            Notes = d.Notes,
                            CreateDateTime = d.CreateDateTime
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Performance review tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<WorkforcePerformanceReviewResponse>.Ok(
                data,
                "Detail performance review workforce berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<WorkforcePerformanceReviewResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Create",
            "Create Workforce Performance Review",
            Description = "Membuat performance review workforce",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("WorkforcePerformanceReview", "Create")]
        public async Task<IActionResult> CreatePerformanceReview(
            Guid workforceProfileId,
            [FromBody] CreateWorkforcePerformanceReviewRequest request)
        {
            var validation = await ValidateHeaderRequestAsync(
                workforceProfileId,
                request.ReviewPeriod,
                request.ReviewDate,
                request.ReviewerUserId,
                request.ReviewType
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.Message
                ));
            }

            var detailValidation = ValidateDetailRequests(request.Details);

            if (!detailValidation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    detailValidation.Message
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var entity = new WfpPerformanceReview
                {
                    Id = Guid.NewGuid(),
                    WorkforceProfileId = workforceProfileId,
                    ReviewPeriod = request.ReviewPeriod.Trim(),
                    ReviewDate = request.ReviewDate.Date,
                    ReviewerUserId = request.ReviewerUserId,
                    ReviewType = request.ReviewType,
                    TotalScore = 0,
                    FinalRating = request.FinalRating,
                    ReviewStatus = request.ReviewStatus,
                    StrengthNotes = NormalizeNullableText(request.StrengthNotes),
                    ImprovementNotes = NormalizeNullableText(request.ImprovementNotes),
                    RecommendationNotes = NormalizeNullableText(request.RecommendationNotes),
                    IsFinalized = false,
                    FinalizedAt = null,
                    IsActive = request.IsActive,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                _dbContext.WfpPerformanceReviews.Add(entity);
                await _dbContext.SaveChangesAsync();

                if (request.Details.Any())
                {
                    var details = request.Details
                        .Select(x => BuildDetailEntity(entity.Id, x, now, actorUserId))
                        .ToList();

                    _dbContext.WfpPerformanceReviewDetails.AddRange(details);
                    await _dbContext.SaveChangesAsync();

                    await RecalculateTotalScoreAsync(entity.Id, actorUserId);
                }

                await transaction.CommitAsync();

                await _loggerService.InfoAsync(
                    LogCategory,
                    "WorkforcePerformanceReview.CreatePerformanceReview",
                    "Performance review workforce berhasil dibuat.",
                    new
                    {
                        entity.Id,
                        entity.WorkforceProfileId,
                        entity.ReviewPeriod,
                        entity.ReviewType
                    }
                );

                return await GetPerformanceReviewById(workforceProfileId, entity.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "WorkforcePerformanceReview.CreatePerformanceReview",
                    "Gagal membuat performance review workforce.",
                    ex,
                    new { workforceProfileId }
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        $"Gagal membuat performance review workforce: {ex.Message}"
                    )
                );
            }
        }

        [HttpPut("{performanceReviewId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforcePerformanceReviewResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Workforce Performance Review",
            Description = "Mengubah performance review workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforcePerformanceReview", "Update")]
        public async Task<IActionResult> UpdatePerformanceReview(
            Guid workforceProfileId,
            Guid performanceReviewId,
            [FromBody] UpdateWorkforcePerformanceReviewRequest request)
        {
            var entity = await _dbContext.WfpPerformanceReviews
                .FirstOrDefaultAsync(x =>
                    x.Id == performanceReviewId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Performance review tidak ditemukan."
                ));
            }

            if (entity.IsFinalized)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Performance review sudah finalized dan tidak bisa diubah."
                ));
            }

            var validation = await ValidateHeaderRequestAsync(
                workforceProfileId,
                request.ReviewPeriod,
                request.ReviewDate,
                request.ReviewerUserId,
                request.ReviewType
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.Message
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.ReviewPeriod = request.ReviewPeriod.Trim();
            entity.ReviewDate = request.ReviewDate.Date;
            entity.ReviewerUserId = request.ReviewerUserId;
            entity.ReviewType = request.ReviewType;
            entity.FinalRating = request.FinalRating;
            entity.ReviewStatus = request.ReviewStatus;
            entity.StrengthNotes = NormalizeNullableText(request.StrengthNotes);
            entity.ImprovementNotes = NormalizeNullableText(request.ImprovementNotes);
            entity.RecommendationNotes = NormalizeNullableText(request.RecommendationNotes);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return await GetPerformanceReviewById(workforceProfileId, entity.Id);
        }

        [HttpPatch("{performanceReviewId:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<WorkforcePerformanceReviewResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Update Workforce Performance Review Status",
            Description = "Mengubah status performance review workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 4
        )]
        [AccessPermission("WorkforcePerformanceReview", "Update")]
        public async Task<IActionResult> UpdatePerformanceReviewStatus(
            Guid workforceProfileId,
            Guid performanceReviewId,
            [FromBody] UpdateWorkforcePerformanceReviewStatusRequest request)
        {
            var entity = await _dbContext.WfpPerformanceReviews
                .FirstOrDefaultAsync(x =>
                    x.Id == performanceReviewId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Performance review tidak ditemukan."
                ));
            }

            if (entity.IsFinalized)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Performance review sudah finalized dan status tidak bisa diubah manual."
                ));
            }

            entity.ReviewStatus = request.ReviewStatus;
            entity.IsActive = request.IsActive;
            entity.RecommendationNotes = NormalizeNullableText(request.RecommendationNotes) ?? entity.RecommendationNotes;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return await GetPerformanceReviewById(workforceProfileId, entity.Id);
        }

        [HttpPatch("{performanceReviewId:guid}/finalize")]
        [ProducesResponseType(typeof(ApiResponse<WorkforcePerformanceReviewResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Finalize Workforce Performance Review",
            Description = "Finalisasi performance review workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 5
        )]
        [AccessPermission("WorkforcePerformanceReview", "Update")]
        public async Task<IActionResult> FinalizePerformanceReview(
            Guid workforceProfileId,
            Guid performanceReviewId,
            [FromBody] FinalizeWorkforcePerformanceReviewRequest request)
        {
            var entity = await _dbContext.WfpPerformanceReviews
                .Include(x => x.Details)
                .FirstOrDefaultAsync(x =>
                    x.Id == performanceReviewId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Performance review tidak ditemukan."
                ));
            }

            if (entity.IsFinalized)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Performance review sudah finalized."
                ));
            }

            var activeDetails = entity.Details
                .Where(x => !x.IsDelete)
                .ToList();

            if (!activeDetails.Any())
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Performance review tidak bisa finalized karena belum memiliki detail penilaian."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.TotalScore = activeDetails.Sum(x => x.WeightedScore);
            entity.FinalRating = request.FinalRating;
            entity.ReviewStatus = PerformanceReviewStatus.Finalized;
            entity.RecommendationNotes = NormalizeNullableText(request.RecommendationNotes) ?? entity.RecommendationNotes;
            entity.IsFinalized = true;
            entity.FinalizedAt = now;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return await GetPerformanceReviewById(workforceProfileId, entity.Id);
        }

        [HttpDelete("{performanceReviewId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction(
            "Delete",
            "Delete Workforce Performance Review",
            Description = "Menghapus performance review workforce",
            AccessType = AccessTypes.Delete,
            SortOrder = 6
        )]
        [AccessPermission("WorkforcePerformanceReview", "Delete")]
        public async Task<IActionResult> DeletePerformanceReview(
            Guid workforceProfileId,
            Guid performanceReviewId)
        {
            var entity = await _dbContext.WfpPerformanceReviews
                .Include(x => x.Details)
                .FirstOrDefaultAsync(x =>
                    x.Id == performanceReviewId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Performance review tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;

            foreach (var detail in entity.Details.Where(x => !x.IsDelete))
            {
                detail.IsDelete = true;
                detail.DeleteDateTime = now;
                detail.DeleteBy = actorUserId;
            }

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Performance review berhasil dihapus."
            ));
        }

        [HttpPost("{performanceReviewId:guid}/details")]
        [ProducesResponseType(typeof(ApiResponse<WorkforcePerformanceReviewResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Create",
            "Create Workforce Performance Review Detail",
            Description = "Menambah detail performance review workforce",
            AccessType = AccessTypes.Create,
            SortOrder = 7
        )]
        [AccessPermission("WorkforcePerformanceReview", "Create")]
        public async Task<IActionResult> CreateDetail(
            Guid workforceProfileId,
            Guid performanceReviewId,
            [FromBody] CreateWorkforcePerformanceReviewDetailRequest request)
        {
            var review = await _dbContext.WfpPerformanceReviews
                .FirstOrDefaultAsync(x =>
                    x.Id == performanceReviewId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (review == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Performance review tidak ditemukan."
                ));
            }

            if (review.IsFinalized)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Performance review sudah finalized dan detail tidak bisa ditambah."
                ));
            }

            var detailValidation = ValidateDetailRequest(request.CriteriaCode, request.CriteriaName, request.Score, request.Weight);

            if (!detailValidation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    detailValidation.Message
                ));
            }

            var criteriaCode = NormalizeRequiredCode(request.CriteriaCode);

            var duplicate = await _dbContext.WfpPerformanceReviewDetails
                .AnyAsync(x =>
                    x.PerformanceReviewId == performanceReviewId &&
                    x.CriteriaCode.ToLower() == criteriaCode.ToLower() &&
                    !x.IsDelete);

            if (duplicate)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "CriteriaCode sudah digunakan pada performance review ini."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var detail = BuildDetailEntity(performanceReviewId, request, now, actorUserId);

            _dbContext.WfpPerformanceReviewDetails.Add(detail);
            await _dbContext.SaveChangesAsync();

            await RecalculateTotalScoreAsync(performanceReviewId, actorUserId);

            return await GetPerformanceReviewById(workforceProfileId, performanceReviewId);
        }

        [HttpPut("{performanceReviewId:guid}/details/{detailId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforcePerformanceReviewResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Update Workforce Performance Review Detail",
            Description = "Mengubah detail performance review workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 8
        )]
        [AccessPermission("WorkforcePerformanceReview", "Update")]
        public async Task<IActionResult> UpdateDetail(
            Guid workforceProfileId,
            Guid performanceReviewId,
            Guid detailId,
            [FromBody] UpdateWorkforcePerformanceReviewDetailRequest request)
        {
            var review = await _dbContext.WfpPerformanceReviews
                .FirstOrDefaultAsync(x =>
                    x.Id == performanceReviewId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (review == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Performance review tidak ditemukan."
                ));
            }

            if (review.IsFinalized)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Performance review sudah finalized dan detail tidak bisa diubah."
                ));
            }

            var detail = await _dbContext.WfpPerformanceReviewDetails
                .FirstOrDefaultAsync(x =>
                    x.Id == detailId &&
                    x.PerformanceReviewId == performanceReviewId &&
                    !x.IsDelete);

            if (detail == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Detail performance review tidak ditemukan."
                ));
            }

            var detailValidation = ValidateDetailRequest(request.CriteriaCode, request.CriteriaName, request.Score, request.Weight);

            if (!detailValidation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    detailValidation.Message
                ));
            }

            var criteriaCode = NormalizeRequiredCode(request.CriteriaCode);

            var duplicate = await _dbContext.WfpPerformanceReviewDetails
                .AnyAsync(x =>
                    x.Id != detailId &&
                    x.PerformanceReviewId == performanceReviewId &&
                    x.CriteriaCode.ToLower() == criteriaCode.ToLower() &&
                    !x.IsDelete);

            if (duplicate)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "CriteriaCode sudah digunakan pada performance review ini."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            detail.CriteriaCode = criteriaCode;
            detail.CriteriaName = request.CriteriaName.Trim();
            detail.Score = request.Score;
            detail.Weight = request.Weight;
            detail.WeightedScore = CalculateWeightedScore(request.Score, request.Weight);
            detail.Notes = NormalizeNullableText(request.Notes);
            detail.UpdateDateTime = now;
            detail.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            await RecalculateTotalScoreAsync(performanceReviewId, actorUserId);

            return await GetPerformanceReviewById(workforceProfileId, performanceReviewId);
        }

        [HttpDelete("{performanceReviewId:guid}/details/{detailId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforcePerformanceReviewResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Delete",
            "Delete Workforce Performance Review Detail",
            Description = "Menghapus detail performance review workforce",
            AccessType = AccessTypes.Delete,
            SortOrder = 9
        )]
        [AccessPermission("WorkforcePerformanceReview", "Delete")]
        public async Task<IActionResult> DeleteDetail(
            Guid workforceProfileId,
            Guid performanceReviewId,
            Guid detailId)
        {
            var review = await _dbContext.WfpPerformanceReviews
                .FirstOrDefaultAsync(x =>
                    x.Id == performanceReviewId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (review == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Performance review tidak ditemukan."
                ));
            }

            if (review.IsFinalized)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Performance review sudah finalized dan detail tidak bisa dihapus."
                ));
            }

            var detail = await _dbContext.WfpPerformanceReviewDetails
                .FirstOrDefaultAsync(x =>
                    x.Id == detailId &&
                    x.PerformanceReviewId == performanceReviewId &&
                    !x.IsDelete);

            if (detail == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Detail performance review tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            detail.IsDelete = true;
            detail.DeleteDateTime = now;
            detail.DeleteBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            await RecalculateTotalScoreAsync(performanceReviewId, actorUserId);

            return await GetPerformanceReviewById(workforceProfileId, performanceReviewId);
        }

        private async Task RecalculateTotalScoreAsync(Guid performanceReviewId, Guid actorUserId)
        {
            var review = await _dbContext.WfpPerformanceReviews
                .FirstOrDefaultAsync(x => x.Id == performanceReviewId && !x.IsDelete);

            if (review == null)
            {
                return;
            }

            var totalScore = await _dbContext.WfpPerformanceReviewDetails
                .Where(x =>
                    x.PerformanceReviewId == performanceReviewId &&
                    !x.IsDelete)
                .SumAsync(x => x.WeightedScore);

            review.TotalScore = totalScore;
            review.UpdateDateTime = DateTime.UtcNow;
            review.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();
        }

        private async Task<(bool IsValid, string Message)> ValidateHeaderRequestAsync(
            Guid workforceProfileId,
            string reviewPeriod,
            DateTime reviewDate,
            Guid reviewerUserId,
            PerformanceReviewType reviewType)
        {
            var profileExists = await _dbContext.MstWorkforceProfiles
                .AnyAsync(x => x.Id == workforceProfileId && !x.IsDelete);

            if (!profileExists)
            {
                return (false, "Workforce profile tidak ditemukan.");
            }

            if (string.IsNullOrWhiteSpace(reviewPeriod))
            {
                return (false, "ReviewPeriod wajib diisi.");
            }

            if (reviewDate == default)
            {
                return (false, "ReviewDate wajib diisi.");
            }

            if (reviewerUserId == Guid.Empty)
            {
                return (false, "ReviewerUserId wajib diisi.");
            }

            var reviewerExists = await _dbContext.Users
                .AnyAsync(x => x.Id == reviewerUserId);

            if (!reviewerExists)
            {
                return (false, "Reviewer user tidak ditemukan.");
            }

            if (reviewType == PerformanceReviewType.Unknown)
            {
                return (false, "ReviewType wajib dipilih.");
            }

            return (true, string.Empty);
        }

        private static (bool IsValid, string Message) ValidateDetailRequests(
            List<CreateWorkforcePerformanceReviewDetailRequest> details)
        {
            if (details == null || !details.Any())
            {
                return (true, string.Empty);
            }

            var duplicateCodes = details
                .Select(x => NormalizeRequiredCode(x.CriteriaCode))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .GroupBy(x => x)
                .Where(x => x.Count() > 1)
                .Select(x => x.Key)
                .ToList();

            if (duplicateCodes.Any())
            {
                return (false, $"CriteriaCode duplicate: {string.Join(", ", duplicateCodes)}.");
            }

            foreach (var detail in details)
            {
                var validation = ValidateDetailRequest(
                    detail.CriteriaCode,
                    detail.CriteriaName,
                    detail.Score,
                    detail.Weight
                );

                if (!validation.IsValid)
                {
                    return validation;
                }
            }

            return (true, string.Empty);
        }

        private static (bool IsValid, string Message) ValidateDetailRequest(
            string criteriaCode,
            string criteriaName,
            decimal score,
            decimal weight)
        {
            if (string.IsNullOrWhiteSpace(criteriaCode))
            {
                return (false, "CriteriaCode wajib diisi.");
            }

            if (string.IsNullOrWhiteSpace(criteriaName))
            {
                return (false, "CriteriaName wajib diisi.");
            }

            if (score < 0)
            {
                return (false, "Score tidak boleh kurang dari 0.");
            }

            if (weight < 0)
            {
                return (false, "Weight tidak boleh kurang dari 0.");
            }

            if (score > 100)
            {
                return (false, "Score maksimal 100.");
            }

            if (weight > 100)
            {
                return (false, "Weight maksimal 100.");
            }

            return (true, string.Empty);
        }

        private static WfpPerformanceReviewDetail BuildDetailEntity(
            Guid performanceReviewId,
            CreateWorkforcePerformanceReviewDetailRequest request,
            DateTime now,
            Guid actorUserId)
        {
            return new WfpPerformanceReviewDetail
            {
                Id = Guid.NewGuid(),
                PerformanceReviewId = performanceReviewId,
                CriteriaCode = NormalizeRequiredCode(request.CriteriaCode),
                CriteriaName = request.CriteriaName.Trim(),
                Score = request.Score,
                Weight = request.Weight,
                WeightedScore = CalculateWeightedScore(request.Score, request.Weight),
                Notes = NormalizeNullableText(request.Notes),
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };
        }

        private async Task<WorkforceProfileHeader?> GetWorkforceProfileHeaderAsync(Guid workforceProfileId)
        {
            return await _dbContext.MstWorkforceProfiles
                .AsNoTracking()
                .Where(x => x.Id == workforceProfileId && !x.IsDelete)
                .Select(x => new WorkforceProfileHeader
                {
                    Id = x.Id,
                    ProfileCode = x.ProfileCode,
                    DisplayName = x.DisplayName,
                    UserType = x.UserType
                })
                .FirstOrDefaultAsync();
        }

        private Guid GetCurrentUserId()
        {
            var userIdText =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("user_id");

            return Guid.TryParse(userIdText, out var userId)
                ? userId
                : Guid.Empty;
        }

        private static decimal CalculateWeightedScore(decimal score, decimal weight)
        {
            return Math.Round(score * weight / 100m, 2);
        }

        private static string NormalizeRequiredCode(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Trim().ToUpperInvariant();
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private class WorkforceProfileHeader
        {
            public Guid Id { get; set; }

            public string ProfileCode { get; set; } = string.Empty;

            public string DisplayName { get; set; } = string.Empty;

            public UserType UserType { get; set; }
        }
    }
}