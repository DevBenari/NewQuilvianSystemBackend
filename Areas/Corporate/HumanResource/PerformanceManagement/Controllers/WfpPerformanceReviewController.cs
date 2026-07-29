using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Performance.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/performance-reviews")]
    [Tags("Corporate / Human Resource / Performance Management / Performance Review")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_PERFORMANCE",
        moduleName: "Human Resource Performance",
        displayName: "Performance Review",
        AreaName = "Corporate",
        ControllerName = "PerformanceReview",
        Description = "Workforce performance review",
        SortOrder = 1
    )]
    public class WfpPerformanceReviewController : ControllerBase
    {
        private const string Prefix = "PRV-RSMMC-";
        private readonly ApplicationDbContext _db;
        private readonly LoggerService _log;
        private static readonly string[]
        Types =
        {
            "Annual", "Semester", "Quarter", "Probation", "Project", "Custom"
        };
        private static readonly string[]
        Statuses =
        {
            "Draft", "SelfAssessment", "ManagerAssessment", "Calibration", "Finalized", "Acknowledged", "Cancelled"
        };
        public WfpPerformanceReviewController(ApplicationDbContext db, LoggerService log)
        {
            _db = db;
            _log = log;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Performance Review", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PerformanceReview", "Read")]
        public IActionResult Metadata() => Ok(ApiResponse<WfpPerformanceReviewFilterMetadataResponse>.Ok(new()
        {
            DefaultFilter = new(), ReviewTypeOptions = Types.Select(x => new WfpPerformanceStringOptionResponse
            {
                Value = x, Label = x
            }
            ).ToList(), ReviewStatusOptions = Statuses.Select(x => new WfpPerformanceStringOptionResponse
            {
                Value = x, Label = x
            }
            ).ToList(), SortOptions = new()
            {
                new()
                {
                    Value = "periodStartDate", Label = "Tanggal periode"
                }, new()
                {
                    Value = "reviewNumber", Label = "Nomor review"
                }, new()
                {
                    Value = "reviewStatus", Label = "Status"
                }, new()
                {
                    Value = "finalScore", Label = "Nilai akhir"
                }, new()
                {
                    Value = "createDateTime", Label = "Tanggal dibuat"
                }
            }, SortDirections = new()
            {
                "asc", "desc"
            }, PageSizeOptions = new()
            {
                10, 25, 50, 100
            }
        }, "Metadata berhasil diambil."));

        [HttpGet("summary")]
        [AccessAction("Read", "Read Performance Review", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PerformanceReview", "Read")]
        public async Task<IActionResult> Summary(Guid workforceProfileId, CancellationToken ct)
        {
            if (!await WorkforceExists(workforceProfileId, ct)) return WorkforceNotFound();
            var q = _db.Set<WfpPerformanceReview> ().AsNoTracking().Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);
            var finalized = q.Where(x => x.IsFinalized);
            return Ok(ApiResponse<WfpPerformanceReviewSummaryResponse>.Ok(new()
            {
                TotalReview = await q.CountAsync(ct), ActiveReview = await q.CountAsync(x => x.IsActive, ct), DraftReview = await q.CountAsync(x => x.ReviewStatus == "Draft", ct), FinalizedReview = await q.CountAsync(x => x.IsFinalized, ct), AcknowledgedReview = await q.CountAsync(x => x.IsAcknowledged, ct), AverageFinalScore = await finalized.AnyAsync(ct)? await finalized.AverageAsync(x => x.FinalScore, ct): 0m
            }, "Ringkasan berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read Performance Review", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PerformanceReview", "Read")]
        public async Task<IActionResult> List(Guid workforceProfileId, [FromQuery] string? reviewType, [FromQuery] string? reviewStatus, [FromQuery] bool? isAcknowledged, [FromQuery] bool? isFinalized, [FromQuery] bool? isActive, [FromQuery] string? search, [FromQuery] string? sortBy = "periodStartDate", [FromQuery] string? sortDirection = "desc", [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 25, CancellationToken ct = default)
        {
            if (!await WorkforceExists(workforceProfileId, ct)) return WorkforceNotFound();
            Norm(ref pageNumber, ref pageSize);
            var q = Filter(Q(workforceProfileId), reviewType, reviewStatus, isAcknowledged, isFinalized, isActive, search);
            var total = await q.CountAsync(ct);
            var rows = await Sort(q, sortBy, sortDirection).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);
            return Ok(ApiResponse<PagedResult<WfpPerformanceReviewResponse>>.Ok(new()
            {
                PageNumber = pageNumber, PageSize = pageSize, TotalData = total, TotalPage = (int) Math.Ceiling(total / (double) pageSize), Items = rows.Select(Map).ToList()
            }, "Data berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Performance Review", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PerformanceReview", "Read")]
        public async Task<IActionResult> Detail(Guid workforceProfileId, Guid id, CancellationToken ct)
        {
            var x = await Q(workforceProfileId).FirstOrDefaultAsync(x => x.Id == id, ct);
            if (x == null) return NotFound(ApiResponse<object>.Fail(404, "Performance review tidak ditemukan."));
            var r = new WfpPerformanceReviewDetailResponse
            {
                Id = x.Id, WorkforceProfileId = x.WorkforceProfileId, WorkforceProfileCode = x.WorkforceProfile?.ProfileCode ?? string.Empty,
                WorkforceDisplayName = x.WorkforceProfile?.DisplayName ?? string.Empty, EmployeeId = x.EmployeeId, OrganizationAssignmentId = x.OrganizationAssignmentId,
                PerformanceCycleId = x.PerformanceCycleId, MasterPerformanceCycleId = x.MasterPerformanceCycleId, MasterPerformanceCycleName = x.MasterPerformanceCycle?.CycleName,
                PerformanceTemplateId = x.PerformanceTemplateId, PerformanceTemplateName = x.PerformanceTemplate?.TemplateName,
                RatingScaleId = x.RatingScaleId, RatingScaleName = x.RatingScale?.ScaleName, ReviewerUserId = x.ReviewerUserId,
                ReviewerUserName = UserName(x.ReviewerUser), ManagerUserId = x.ManagerUserId, ManagerUserName = UserName(x.ManagerUser),
                ReviewNumber = x.ReviewNumber, ReviewType = x.ReviewType, ReviewPeriod = x.ReviewPeriod, PeriodStartDate = x.PeriodStartDate,
                PeriodEndDate = x.PeriodEndDate, ReviewDate = x.ReviewDate, ReviewStatus = x.ReviewStatus, OverallScore = x.OverallScore,
                FinalScore = x.FinalScore, FinalRating = x.FinalRating, IsAcknowledged = x.IsAcknowledged, AcknowledgedAt = x.AcknowledgedAt,
                IsFinalized = x.IsFinalized, FinalizedAt = x.FinalizedAt, IsActive = x.IsActive, DetailCount = x.Details.Count(d => !d.IsDelete),
                Strengths = x.Strengths, ImprovementAreas = x.ImprovementAreas, EmployeeComments = x.EmployeeComments, ReviewerComments = x.ReviewerComments,
                FinalComments = x.FinalComments, FinalizedByUserId = x.FinalizedByUserId, FinalizedByUserName = UserName(x.FinalizedByUser),
                CreateDateTime = x.CreateDateTime, CreateBy = N(x.CreateBy), UpdateDateTime = x.UpdateDateTime, UpdateBy = N(x.UpdateBy)
            };
            return Ok(ApiResponse<WfpPerformanceReviewDetailResponse>.Ok(r, "Detail berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Create", "Create Performance Review", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("PerformanceReview", "Create")]
        public async Task<IActionResult> Create(Guid workforceProfileId, [FromBody] CreateWfpPerformanceReviewRequest r, CancellationToken ct)
        {
            if (!await WorkforceExists(workforceProfileId, ct)) return WorkforceNotFound();
            var e = await Validate(workforceProfileId, r, null, ct);
            if (e != null) return BadRequest(ApiResponse<object>.Fail(400, e));
            var now = DateTime.UtcNow;
            var x = new WfpPerformanceReview
            {
                Id = Guid.NewGuid(), WorkforceProfileId = workforceProfileId, EmployeeId = await ResolveEmployee(workforceProfileId, r.EmployeeId, ct),
                OrganizationAssignmentId = NG(r.OrganizationAssignmentId), PerformanceCycleId = NG(r.PerformanceCycleId), MasterPerformanceCycleId = NG(r.MasterPerformanceCycleId),
                PerformanceTemplateId = NG(r.PerformanceTemplateId), RatingScaleId = NG(r.RatingScaleId), ReviewerUserId = NG(r.ReviewerUserId),
                ManagerUserId = NG(r.ManagerUserId), ReviewNumber = await Code(ct), ReviewType = Canon(r.ReviewType, Types), ReviewPeriod = r.ReviewPeriod.Trim(),
                PeriodStartDate = r.PeriodStartDate, PeriodEndDate = r.PeriodEndDate, ReviewDate = r.ReviewDate, ReviewStatus = Canon(r.ReviewStatus, Statuses),
                OverallScore = r.OverallScore, FinalScore = r.FinalScore, FinalRating = T(r.FinalRating), Strengths = T(r.Strengths),
                ImprovementAreas = T(r.ImprovementAreas), EmployeeComments = T(r.EmployeeComments), ReviewerComments = T(r.ReviewerComments),
                FinalComments = T(r.FinalComments), IsAcknowledged = false, IsFinalized = false, IsActive = r.IsActive, CreateDateTime = now,
                CreateBy = Actor(), IsDelete = false, IsCancel = false
            };
            _db.Set<WfpPerformanceReview> ().Add(x);
            await _db.SaveChangesAsync(ct);
            if (x.PerformanceTemplateId.HasValue) await SeedDetailsFromTemplate(x.Id, x.PerformanceTemplateId.Value, now, ct);
            return await Detail(workforceProfileId, x.Id, ct);
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Performance Review", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("PerformanceReview", "Update")]
        public async Task<IActionResult> Update(Guid workforceProfileId, Guid id, [FromBody] UpdateWfpPerformanceReviewRequest r, CancellationToken ct)
        {
            var x = await _db.Set<WfpPerformanceReview> ().FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete, ct);
            if (x == null) return NotFound(ApiResponse<object>.Fail(404, "Performance review tidak ditemukan."));
            if (x.IsFinalized) return BadRequest(ApiResponse<object>.Fail(400, "Review yang sudah finalized tidak dapat diubah."));
            var e = await Validate(workforceProfileId, r, id, ct);
            if (e != null) return BadRequest(ApiResponse<object>.Fail(400, e));
            x.EmployeeId = await ResolveEmployee(workforceProfileId, r.EmployeeId, ct);
            x.OrganizationAssignmentId = NG(r.OrganizationAssignmentId);
            x.PerformanceCycleId = NG(r.PerformanceCycleId);
            x.MasterPerformanceCycleId = NG(r.MasterPerformanceCycleId);
            x.PerformanceTemplateId = NG(r.PerformanceTemplateId);
            x.RatingScaleId = NG(r.RatingScaleId);
            x.ReviewerUserId = NG(r.ReviewerUserId);
            x.ManagerUserId = NG(r.ManagerUserId);
            x.ReviewType = Canon(r.ReviewType, Types);
            x.ReviewPeriod = r.ReviewPeriod.Trim();
            x.PeriodStartDate = r.PeriodStartDate;
            x.PeriodEndDate = r.PeriodEndDate;
            x.ReviewDate = r.ReviewDate;
            x.ReviewStatus = Canon(r.ReviewStatus, Statuses);
            x.OverallScore = r.OverallScore;
            x.FinalScore = r.FinalScore;
            x.FinalRating = T(r.FinalRating);
            x.Strengths = T(r.Strengths);
            x.ImprovementAreas = T(r.ImprovementAreas);
            x.EmployeeComments = T(r.EmployeeComments);
            x.ReviewerComments = T(r.ReviewerComments);
            x.FinalComments = T(r.FinalComments);
            x.IsActive = r.IsActive;
            x.UpdateDateTime = DateTime.UtcNow;
            x.UpdateBy = Actor();
            await _db.SaveChangesAsync(ct);
            return await Detail(workforceProfileId, id, ct);
        }

        [HttpPatch("{id:guid}/status")]
        [AccessAction("Update", "Update Performance Review Status", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("PerformanceReview", "Update")]
        public async Task<IActionResult> Status(Guid workforceProfileId, Guid id, [FromBody] UpdateWfpPerformanceReviewStatusRequest r, CancellationToken ct)
        {
            var x = await _db.Set<WfpPerformanceReview> ().FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete, ct);
            if (x == null) return NotFound(ApiResponse<object>.Fail(404, "Performance review tidak ditemukan."));
            if (x.IsFinalized) return BadRequest(ApiResponse<object>.Fail(400, "Review finalized tidak dapat diubah status biasa."));
            if (!Statuses.Contains(r.ReviewStatus, StringComparer.OrdinalIgnoreCase)) return BadRequest(ApiResponse<object>.Fail(400, "Review status tidak valid."));
            x.ReviewStatus = Canon(r.ReviewStatus, Statuses);
            if (r.IsActive.HasValue) x.IsActive = r.IsActive.Value;
            x.UpdateDateTime = DateTime.UtcNow;
            x.UpdateBy = Actor();
            await _db.SaveChangesAsync(ct);
            return Ok(ApiResponse<object>.Ok(null, "Status berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/finalize")]
        [AccessAction("Update", "Finalize Performance Review", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("PerformanceReview", "Update")]
        public async Task<IActionResult> Finalize(Guid workforceProfileId, Guid id, [FromBody] FinalizeWfpPerformanceReviewRequest r, CancellationToken ct)
        {
            var x = await _db.Set<WfpPerformanceReview> ().FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete, ct);
            if (x == null) return NotFound(ApiResponse<object>.Fail(404, "Performance review tidak ditemukan."));
            if (x.IsFinalized) return BadRequest(ApiResponse<object>.Fail(400, "Performance review sudah finalized."));
            var details = await _db.Set<WfpPerformanceReviewDetail> ().Where(d => d.PerformanceReviewId == id && d.IsActive && !d.IsDelete).ToListAsync(ct);
            if (details.Count == 0) return BadRequest(ApiResponse<object>.Fail(400, "Performance review belum memiliki detail."));
            if (details.Any(d => !d.FinalScore.HasValue && !d.Score.HasValue)) return BadRequest(ApiResponse<object>.Fail(400, "Seluruh detail aktif harus memiliki final score atau score."));
            var weighted = details.Sum(d => (d.FinalScore ?? d.Score ?? 0m) * d.Weight);
            var totalWeight = details.Sum(d => d.Weight);
            x.OverallScore = totalWeight> 0? weighted / totalWeight : details.Average(d => d.FinalScore ?? d.Score ?? 0m);
            x.FinalScore = r.FinalScore != 0? r.FinalScore : x.OverallScore;
            x.FinalRating = T(r.FinalRating);
            x.FinalComments = T(r.FinalComments);
            x.ReviewStatus = "Finalized";
            x.IsFinalized = true;
            x.FinalizedAt = DateTime.UtcNow;
            x.FinalizedByUserId = Actor();
            x.UpdateDateTime = DateTime.UtcNow;
            x.UpdateBy = Actor();
            await _db.SaveChangesAsync(ct);
            return await Detail(workforceProfileId, id, ct);
        }

        [HttpPatch("{id:guid}/acknowledge")]
        [AccessAction("Update", "Acknowledge Performance Review", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("PerformanceReview", "Update")]
        public async Task<IActionResult> Acknowledge(Guid workforceProfileId, Guid id, [FromBody] AcknowledgeWfpPerformanceReviewRequest r, CancellationToken ct)
        {
            var x = await _db.Set<WfpPerformanceReview> ().FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete, ct);
            if (x == null) return NotFound(ApiResponse<object>.Fail(404, "Performance review tidak ditemukan."));
            if (!x.IsFinalized) return BadRequest(ApiResponse<object>.Fail(400, "Review harus finalized sebelum diakui."));
            x.IsAcknowledged = true;
            x.AcknowledgedAt = DateTime.UtcNow;
            x.EmployeeComments = T(r.EmployeeComments) ?? x.EmployeeComments;
            x.ReviewStatus = "Acknowledged";
            x.UpdateDateTime = DateTime.UtcNow;
            x.UpdateBy = Actor();
            await _db.SaveChangesAsync(ct);
            return Ok(ApiResponse<object>.Ok(null, "Performance review berhasil diakui."));
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete Performance Review", AccessType = AccessTypes.Delete, SortOrder = 7)]
        [AccessPermission("PerformanceReview", "Delete")]
        public async Task<IActionResult> Delete(Guid workforceProfileId, Guid id, CancellationToken ct)
        {
            var x = await _db.Set<WfpPerformanceReview> ().FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete, ct);
            if (x == null) return NotFound(ApiResponse<object>.Fail(404, "Performance review tidak ditemukan."));
            if (x.IsFinalized) return BadRequest(ApiResponse<object>.Fail(400, "Review finalized tidak dapat dihapus."));
            var now = DateTime.UtcNow;
            var details = await _db.Set<WfpPerformanceReviewDetail> ().Where(d => d.PerformanceReviewId == id && !d.IsDelete).ToListAsync(ct);
            foreach (var d in details)
            {
                d.IsDelete = true;
                d.IsActive = false;
                d.DeleteDateTime = now;
                d.DeleteBy = Actor();
                d.UpdateDateTime = now;
                d.UpdateBy = Actor();
            }
            x.IsDelete = true;
            x.IsActive = false;
            x.DeleteDateTime = now;
            x.DeleteBy = Actor();
            x.UpdateDateTime = now;
            x.UpdateBy = Actor();
            await _db.SaveChangesAsync(ct);
            return Ok(ApiResponse<object>.Ok(null, "Performance review berhasil dihapus."));
        }
        IQueryable<WfpPerformanceReview> Q(Guid wf) => _db.Set<WfpPerformanceReview> ().AsNoTracking().Include(x => x.WorkforceProfile).Include(x => x.MasterPerformanceCycle).Include(x => x.PerformanceTemplate).Include(x => x.RatingScale).Include(x => x.ReviewerUser).Include(x => x.ManagerUser).Include(x => x.FinalizedByUser).Include(x => x.Details).Where(x => x.WorkforceProfileId == wf && !x.IsDelete);
        static IQueryable<WfpPerformanceReview> Filter(IQueryable<WfpPerformanceReview> q, string? t, string? s, bool? a, bool? f, bool? active, string? search)
        {
            if (!string.IsNullOrWhiteSpace(t)) q = q.Where(x => x.ReviewType == t);
            if (!string.IsNullOrWhiteSpace(s)) q = q.Where(x => x.ReviewStatus == s);
            if (a.HasValue) q = q.Where(x => x.IsAcknowledged == a);
            if (f.HasValue) q = q.Where(x => x.IsFinalized == f);
            if (active.HasValue) q = q.Where(x => x.IsActive == active);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var k = search.Trim().ToLower();
                q = q.Where(x => x.ReviewNumber.ToLower().Contains(k) || x.ReviewPeriod.ToLower().Contains(k) || (x.FinalRating != null && x.FinalRating.ToLower().Contains(k)));
            }
            return q;
        }
        static IOrderedQueryable<WfpPerformanceReview> Sort(IQueryable<WfpPerformanceReview> q, string? b, string? d)
        {
            var z = string.Equals(d, "desc", StringComparison.OrdinalIgnoreCase);
            return(b ?? "").ToLowerInvariant() switch
            {
                "reviewnumber" => z? q.OrderByDescending(x => x.ReviewNumber): q.OrderBy(x => x.ReviewNumber), "reviewstatus" => z? q.OrderByDescending(x => x.ReviewStatus): q.OrderBy(x => x.ReviewStatus),
                "finalscore" => z? q.OrderByDescending(x => x.FinalScore): q.OrderBy(x => x.FinalScore), "createdatetime" => z? q.OrderByDescending(x => x.CreateDateTime): q.OrderBy(x => x.CreateDateTime),
                _ => z? q.OrderByDescending(x => x.PeriodStartDate): q.OrderBy(x => x.PeriodStartDate)
            };
        }
        WfpPerformanceReviewResponse Map(WfpPerformanceReview x) => new()
        {
            Id = x.Id, WorkforceProfileId = x.WorkforceProfileId, WorkforceProfileCode = x.WorkforceProfile?.ProfileCode ?? string.Empty,
            WorkforceDisplayName = x.WorkforceProfile?.DisplayName ?? string.Empty, EmployeeId = x.EmployeeId, OrganizationAssignmentId = x.OrganizationAssignmentId,
            PerformanceCycleId = x.PerformanceCycleId, MasterPerformanceCycleId = x.MasterPerformanceCycleId, MasterPerformanceCycleName = x.MasterPerformanceCycle?.CycleName,
            PerformanceTemplateId = x.PerformanceTemplateId, PerformanceTemplateName = x.PerformanceTemplate?.TemplateName,
            RatingScaleId = x.RatingScaleId, RatingScaleName = x.RatingScale?.ScaleName, ReviewerUserId = x.ReviewerUserId,
            ReviewerUserName = UserName(x.ReviewerUser), ManagerUserId = x.ManagerUserId, ManagerUserName = UserName(x.ManagerUser),
            ReviewNumber = x.ReviewNumber, ReviewType = x.ReviewType, ReviewPeriod = x.ReviewPeriod, PeriodStartDate = x.PeriodStartDate,
            PeriodEndDate = x.PeriodEndDate, ReviewDate = x.ReviewDate, ReviewStatus = x.ReviewStatus, OverallScore = x.OverallScore,
            FinalScore = x.FinalScore, FinalRating = x.FinalRating, IsAcknowledged = x.IsAcknowledged, AcknowledgedAt = x.AcknowledgedAt,
            IsFinalized = x.IsFinalized, FinalizedAt = x.FinalizedAt, IsActive = x.IsActive, DetailCount = x.Details.Count(d => !d.IsDelete),
            CreateDateTime = x.CreateDateTime, CreateBy = N(x.CreateBy)
        };
        async Task<string ?> Validate(Guid wf, CreateWfpPerformanceReviewRequest r, Guid? id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(r.ReviewPeriod)) return "Review period wajib diisi.";
            if (!Types.Contains(r.ReviewType, StringComparer.OrdinalIgnoreCase)) return "Review type tidak valid.";
            if (!Statuses.Contains(r.ReviewStatus, StringComparer.OrdinalIgnoreCase)) return "Review status tidak valid.";
            if (r.PeriodEndDate<r.PeriodStartDate) return "Periode review tidak valid.";
            if (r.EmployeeId.HasValue && !await _db.Set<MstEmployee> ().AnyAsync(x => x.Id == r.EmployeeId && x.WorkforceProfileId == wf && x.IsActive && !x.IsDelete, ct)) return "Employee tidak sesuai workforce profile.";
            if (r.OrganizationAssignmentId.HasValue && !await _db.Set<WfpOrganizationAssignment> ().AnyAsync(x => x.Id == r.OrganizationAssignmentId && x.WorkforceProfileId == wf && !x.IsDelete, ct)) return "Organization assignment tidak sesuai workforce profile.";
            if (r.MasterPerformanceCycleId.HasValue && !await _db.Set<MstPerformanceCycle> ().AnyAsync(x => x.Id == r.MasterPerformanceCycleId && x.IsActive && !x.IsDelete, ct)) return "Master performance cycle tidak valid.";
            if (r.PerformanceTemplateId.HasValue && !await _db.Set<MstPerformanceTemplate> ().AnyAsync(x => x.Id == r.PerformanceTemplateId && x.IsActive && !x.IsDelete, ct)) return "Performance template tidak valid.";
            if (r.RatingScaleId.HasValue && !await _db.Set<MstPerformanceRatingScale> ().AnyAsync(x => x.Id == r.RatingScaleId && x.IsActive && !x.IsDelete, ct)) return "Rating scale tidak valid.";
            if (r.PerformanceCycleId.HasValue && await _db.Set<WfpPerformanceReview> ().AnyAsync(x => x.WorkforceProfileId == wf && x.PerformanceCycleId == r.PerformanceCycleId && !x.IsDelete && (!id.HasValue || x.Id != id), ct)) return "Review untuk performance cycle tersebut sudah tersedia.";
            return null;
        }
        async Task SeedDetailsFromTemplate(Guid reviewId, Guid templateId, DateTime now, CancellationToken ct)
        {
            var rows = await _db.Set<MstPerformanceTemplateDetail> ().AsNoTracking().Where(x => x.PerformanceTemplateId == templateId && x.IsActive && !x.IsDelete).OrderBy(x => x.SortOrder).ToListAsync(ct);
            foreach (var t in rows) _db.Set<WfpPerformanceReviewDetail> ().Add(new()
            {
                Id = Guid.NewGuid(), PerformanceReviewId = reviewId, KpiCatalogId = t.KpiCatalogId, PerformanceTemplateDetailId = t.Id, DetailType = t.DetailType, Category = null, IndicatorCode = t.DetailCode, IndicatorName = t.DetailName, Description = t.Description, Weight = t.Weight, TargetValue = t.TargetValue, Sequence = t.SortOrder + 1, IsActive = true, CreateDateTime = now, CreateBy = Actor(), IsDelete = false, IsCancel = false
            }
            );
            if (rows.Count> 0) await _db.SaveChangesAsync(ct);
        }
        async Task<Guid ?> ResolveEmployee(Guid wf, Guid? value, CancellationToken ct)
        {
            if (value.HasValue && value != Guid.Empty) return value;
            return await _db.Set<MstEmployee> ().Where(x => x.WorkforceProfileId == wf && x.IsActive && !x.IsDelete).Select(x => (Guid? ) x.Id).FirstOrDefaultAsync(ct);
        }
        async Task<bool> WorkforceExists(Guid id, CancellationToken ct) => await _db.Set<MstWorkforceProfile> ().AnyAsync(x => x.Id == id && x.IsActive && !x.IsDelete, ct);
        IActionResult WorkforceNotFound() => NotFound(ApiResponse<object>.Fail(404, "Profil tenaga kerja tidak ditemukan."));
        async Task<string> Code(CancellationToken ct)
        {
            var c = await _db.Set<WfpPerformanceReview> ().Where(x => x.ReviewNumber.StartsWith(Prefix)).Select(x => x.ReviewNumber).ToListAsync(ct);
            return Next(c, Prefix);
        }
        Guid Actor()
        {
            var s = User.FindFirstValue("user_id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(s, out var g)? g : Guid.Empty;
        }
        static string UserName(QuilvianSystemBackend.Models.ApplicationUser? u) => u?.DisplayName ?? u?.UserName ?? u?.Email ?? u?.UserCode ?? string.Empty;
        static string Next(IEnumerable<string> c, string p)
        {
            var u = c.Select(x => x.Replace(p, "")).Where(x => int.TryParse(x, out _)).Select(int.Parse).ToHashSet();
            var n = 1;
            while (u.Contains(n)) n++;
            return p + n.ToString("D5");
        }
        static string Canon(string v, string[] a) => a.First(x => x.Equals(v.Trim(), StringComparison.OrdinalIgnoreCase));
        static Guid? NG(Guid? g) => !g.HasValue || g == Guid.Empty? null : g;
        static Guid? N(Guid g) => g == Guid.Empty? null : g;
        static string? T(string? s) => string.IsNullOrWhiteSpace(s)? null : s.Trim();
        static void Norm(ref int p, ref int s)
        {
            p = Math.Max(1, p);
            s = Math.Min(100, Math.Max(1, s));
        }
    }
}
