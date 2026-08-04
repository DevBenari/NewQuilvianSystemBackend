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

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/performance-reviews/{performanceReviewId:guid}/details")]
    [Tags("Corporate / Human Resource / Performance Management / Performance Review Detail")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_PERFORMANCE",
        moduleName: "Human Resource Performance",
        displayName: "Performance Review Detail",
        AreaName = "Corporate",
        ControllerName = "PerformanceReviewDetail",
        Description = "Workforce performance review detail",
        SortOrder = 2
    )]
    public class WfpPerformanceReviewDetailController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly LoggerService _log;
        private static readonly string[]
        Types =
        {
            "Section", "KPI", "Competency", "Behavior", "Goal", "Custom"
        };
        public WfpPerformanceReviewDetailController(ApplicationDbContext db, LoggerService log)
        {
            _db = db;
            _log = log;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Performance Review Detail", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PerformanceReviewDetail", "Read")]
        public IActionResult Metadata() => Ok(ApiResponse<WfpPerformanceReviewDetailFilterMetadataResponse>.Ok(new()
        {
            DefaultFilter = new(), DetailTypeOptions = Types.Select(x => new WfpPerformanceStringOptionResponse
            {
                Value = x, Label = x
            }
            ).ToList(), SortOptions = new()
            {
                new()
                {
                    Value = "sequence", Label = "Urutan"
                }, new()
                {
                    Value = "indicatorName", Label = "Indikator"
                }, new()
                {
                    Value = "weight", Label = "Bobot"
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
        [AccessAction("Read", "Read Performance Review Detail", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PerformanceReviewDetail", "Read")]
        public async Task<IActionResult> Summary(Guid workforceProfileId, Guid performanceReviewId, CancellationToken ct)
        {
            if (!await ReviewExists(workforceProfileId, performanceReviewId, ct)) return NF();
            var q = Q(performanceReviewId);
            var scored = q.Where(x => x.FinalScore.HasValue || x.Score.HasValue);
            return Ok(ApiResponse<WfpPerformanceReviewDetailSummaryResponse>.Ok(new()
            {
                TotalDetail = await q.CountAsync(ct), ActiveDetail = await q.CountAsync(x => x.IsActive, ct), TotalWeight = await q.Where(x => x.IsActive).SumAsync(x => x.Weight, ct), AverageFinalScore = await scored.AnyAsync(ct)? await scored.AverageAsync(x => x.FinalScore ?? x.Score ?? 0m, ct): 0m
            }, "Ringkasan berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read Performance Review Detail", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PerformanceReviewDetail", "Read")]
        public async Task<IActionResult> List(Guid workforceProfileId, Guid performanceReviewId, [FromQuery] string? detailType, [FromQuery] bool? isActive, [FromQuery] string? search, [FromQuery] string? sortBy = "sequence", [FromQuery] string? sortDirection = "asc", [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 25, CancellationToken ct = default)
        {
            if (!await ReviewExists(workforceProfileId, performanceReviewId, ct)) return NF();
            Norm(ref pageNumber, ref pageSize);
            var q = Filter(Q(performanceReviewId), detailType, isActive, search);
            var total = await q.CountAsync(ct);
            var rows = await Sort(q, sortBy, sortDirection).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);
            return Ok(ApiResponse<PagedResult<WfpPerformanceReviewItemResponse>>.Ok(new()
            {
                PageNumber = pageNumber, PageSize = pageSize, TotalData = total, TotalPage = (int) Math.Ceiling(total / (double) pageSize), Items = rows.Select(Map).ToList()
            }, "Data berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Performance Review Detail", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PerformanceReviewDetail", "Read")]
        public async Task<IActionResult> Detail(Guid workforceProfileId, Guid performanceReviewId, Guid id, CancellationToken ct)
        {
            if (!await ReviewExists(workforceProfileId, performanceReviewId, ct)) return NF();
            var x = await Q(performanceReviewId).FirstOrDefaultAsync(x => x.Id == id, ct);
            if (x == null) return NotFound(ApiResponse<object>.Fail(404, "Performance review detail tidak ditemukan."));
            var r = new WfpPerformanceReviewItemDetailResponse
            {
                Id = x.Id, PerformanceReviewId = x.PerformanceReviewId, KpiCatalogId = x.KpiCatalogId, PerformanceTemplateDetailId = x.PerformanceTemplateDetailId,
                DetailType = x.DetailType, Category = x.Category, IndicatorCode = x.IndicatorCode, IndicatorName = x.IndicatorName,
                Weight = x.Weight, TargetValue = x.TargetValue, ActualValue = x.ActualValue, SelfScore = x.SelfScore, ManagerScore = x.ManagerScore,
                FinalScore = x.FinalScore, Score = x.Score, Rating = x.Rating, Sequence = x.Sequence, IsActive = x.IsActive, Description = x.Description,
                EvidencePath = x.EvidencePath, Comments = x.Comments, CreateDateTime = x.CreateDateTime, CreateBy = N(x.CreateBy),
                UpdateDateTime = x.UpdateDateTime, UpdateBy = N(x.UpdateBy)
            };
            return Ok(ApiResponse<WfpPerformanceReviewItemDetailResponse>.Ok(r, "Detail berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Create", "Create Performance Review Detail", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("PerformanceReviewDetail", "Create")]
        public async Task<IActionResult> Create(Guid workforceProfileId, Guid performanceReviewId, [FromBody] CreateWfpPerformanceReviewDetailRequest r, CancellationToken ct)
        {
            var review = await GetEditableReview(workforceProfileId, performanceReviewId, ct);
            if (review == null) return NFOrFinalized(workforceProfileId, performanceReviewId, ct);
            var e = await Validate(performanceReviewId, r, null, ct);
            if (e != null) return BadRequest(ApiResponse<object>.Fail(400, e));
            var normalized = await ResolveMaster(r, ct);
            var x = new WfpPerformanceReviewDetail
            {
                Id = Guid.NewGuid(), PerformanceReviewId = performanceReviewId, KpiCatalogId = NG(r.KpiCatalogId), PerformanceTemplateDetailId = NG(r.PerformanceTemplateDetailId),
                DetailType = Canon(r.DetailType), Category = T(r.Category), IndicatorCode = T(r.IndicatorCode) ?? normalized.Code,
                IndicatorName = string.IsNullOrWhiteSpace(r.IndicatorName)? normalized.Name : r.IndicatorName.Trim(), Description = T(r.Description) ?? normalized.Description,
                Weight = r.Weight != 0? r.Weight : normalized.Weight, TargetValue = r.TargetValue ?? normalized.Target, ActualValue = r.ActualValue,
                SelfScore = r.SelfScore, ManagerScore = r.ManagerScore, FinalScore = r.FinalScore, Score = r.Score, Rating = T(r.Rating),
                EvidencePath = T(r.EvidencePath), Comments = T(r.Comments), Sequence = r.Sequence, IsActive = r.IsActive, CreateDateTime = DateTime.UtcNow,
                CreateBy = Actor(), IsDelete = false, IsCancel = false
            };
            _db.Set<WfpPerformanceReviewDetail> ().Add(x);
            await _db.SaveChangesAsync(ct);
            await RecalculateReview(performanceReviewId, ct);
            return await Detail(workforceProfileId, performanceReviewId, x.Id, ct);
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Performance Review Detail", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("PerformanceReviewDetail", "Update")]
        public async Task<IActionResult> Update(Guid workforceProfileId, Guid performanceReviewId, Guid id, [FromBody] UpdateWfpPerformanceReviewDetailRequest r, CancellationToken ct)
        {
            if (await GetEditableReview(workforceProfileId, performanceReviewId, ct) == null) return NFOrFinalized(workforceProfileId, performanceReviewId, ct);
            var x = await _db.Set<WfpPerformanceReviewDetail> ().FirstOrDefaultAsync(x => x.Id == id && x.PerformanceReviewId == performanceReviewId && !x.IsDelete, ct);
            if (x == null) return NotFound(ApiResponse<object>.Fail(404, "Performance review detail tidak ditemukan."));
            var e = await Validate(performanceReviewId, r, id, ct);
            if (e != null) return BadRequest(ApiResponse<object>.Fail(400, e));
            var normalized = await ResolveMaster(r, ct);
            x.KpiCatalogId = NG(r.KpiCatalogId);
            x.PerformanceTemplateDetailId = NG(r.PerformanceTemplateDetailId);
            x.DetailType = Canon(r.DetailType);
            x.Category = T(r.Category);
            x.IndicatorCode = T(r.IndicatorCode) ?? normalized.Code;
            x.IndicatorName = string.IsNullOrWhiteSpace(r.IndicatorName)? normalized.Name : r.IndicatorName.Trim();
            x.Description = T(r.Description) ?? normalized.Description;
            x.Weight = r.Weight != 0? r.Weight : normalized.Weight;
            x.TargetValue = r.TargetValue ?? normalized.Target;
            x.ActualValue = r.ActualValue;
            x.SelfScore = r.SelfScore;
            x.ManagerScore = r.ManagerScore;
            x.FinalScore = r.FinalScore;
            x.Score = r.Score;
            x.Rating = T(r.Rating);
            x.EvidencePath = T(r.EvidencePath);
            x.Comments = T(r.Comments);
            x.Sequence = r.Sequence;
            x.IsActive = r.IsActive;
            x.UpdateDateTime = DateTime.UtcNow;
            x.UpdateBy = Actor();
            await _db.SaveChangesAsync(ct);
            await RecalculateReview(performanceReviewId, ct);
            return await Detail(workforceProfileId, performanceReviewId, id, ct);
        }

        [HttpPatch("{id:guid}/status")]
        [AccessAction("Update", "Update Performance Review Detail Status", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("PerformanceReviewDetail", "Update")]
        public async Task<IActionResult> Status(Guid workforceProfileId, Guid performanceReviewId, Guid id, [FromBody] UpdateWfpPerformanceReviewDetailStatusRequest r, CancellationToken ct)
        {
            if (await GetEditableReview(workforceProfileId, performanceReviewId, ct) == null) return NFOrFinalized(workforceProfileId, performanceReviewId, ct);
            var x = await _db.Set<WfpPerformanceReviewDetail> ().FirstOrDefaultAsync(x => x.Id == id && x.PerformanceReviewId == performanceReviewId && !x.IsDelete, ct);
            if (x == null) return NotFound(ApiResponse<object>.Fail(404, "Performance review detail tidak ditemukan."));
            x.IsActive = r.IsActive;
            x.UpdateDateTime = DateTime.UtcNow;
            x.UpdateBy = Actor();
            await _db.SaveChangesAsync(ct);
            await RecalculateReview(performanceReviewId, ct);
            return Ok(ApiResponse<object>.Ok(null, "Status berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete Performance Review Detail", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("PerformanceReviewDetail", "Delete")]
        public async Task<IActionResult> Delete(Guid workforceProfileId, Guid performanceReviewId, Guid id, CancellationToken ct)
        {
            if (await GetEditableReview(workforceProfileId, performanceReviewId, ct) == null) return NFOrFinalized(workforceProfileId, performanceReviewId, ct);
            var x = await _db.Set<WfpPerformanceReviewDetail> ().FirstOrDefaultAsync(x => x.Id == id && x.PerformanceReviewId == performanceReviewId && !x.IsDelete, ct);
            if (x == null) return NotFound(ApiResponse<object>.Fail(404, "Performance review detail tidak ditemukan."));
            var now = DateTime.UtcNow;
            x.IsDelete = true;
            x.IsActive = false;
            x.DeleteDateTime = now;
            x.DeleteBy = Actor();
            x.UpdateDateTime = now;
            x.UpdateBy = Actor();
            await _db.SaveChangesAsync(ct);
            await RecalculateReview(performanceReviewId, ct);
            return Ok(ApiResponse<object>.Ok(null, "Performance review detail berhasil dihapus."));
        }
        IQueryable<WfpPerformanceReviewDetail> Q(Guid id) => _db.Set<WfpPerformanceReviewDetail> ().AsNoTracking().Where(x => x.PerformanceReviewId == id && !x.IsDelete);
        static IQueryable<WfpPerformanceReviewDetail> Filter(IQueryable<WfpPerformanceReviewDetail> q, string? t, bool? a, string? s)
        {
            if (!string.IsNullOrWhiteSpace(t)) q = q.Where(x => x.DetailType == t);
            if (a.HasValue) q = q.Where(x => x.IsActive == a);
            if (!string.IsNullOrWhiteSpace(s))
            {
                var k = s.Trim().ToLower();
                q = q.Where(x => (x.IndicatorCode != null && x.IndicatorCode.ToLower().Contains(k)) || x.IndicatorName.ToLower().Contains(k) || (x.Category != null && x.Category.ToLower().Contains(k)));
            }
            return q;
        }
        static IOrderedQueryable<WfpPerformanceReviewDetail> Sort(IQueryable<WfpPerformanceReviewDetail> q, string? b, string? d)
        {
            var z = string.Equals(d, "desc", StringComparison.OrdinalIgnoreCase);
            return(b ?? "").ToLowerInvariant() switch
            {
                "indicatorname" => z? q.OrderByDescending(x => x.IndicatorName): q.OrderBy(x => x.IndicatorName), "weight" => z? q.OrderByDescending(x => x.Weight): q.OrderBy(x => x.Weight),
                "finalscore" => z? q.OrderByDescending(x => x.FinalScore): q.OrderBy(x => x.FinalScore), "createdatetime" => z? q.OrderByDescending(x => x.CreateDateTime): q.OrderBy(x => x.CreateDateTime),
                _ => z? q.OrderByDescending(x => x.Sequence): q.OrderBy(x => x.Sequence)
            };
        }
        WfpPerformanceReviewItemResponse Map(WfpPerformanceReviewDetail x) => new()
        {
            Id = x.Id, PerformanceReviewId = x.PerformanceReviewId, KpiCatalogId = x.KpiCatalogId, PerformanceTemplateDetailId = x.PerformanceTemplateDetailId,
            DetailType = x.DetailType, Category = x.Category, IndicatorCode = x.IndicatorCode, IndicatorName = x.IndicatorName,
            Weight = x.Weight, TargetValue = x.TargetValue, ActualValue = x.ActualValue, SelfScore = x.SelfScore, ManagerScore = x.ManagerScore,
            FinalScore = x.FinalScore, Score = x.Score, Rating = x.Rating, Sequence = x.Sequence, IsActive = x.IsActive, CreateDateTime = x.CreateDateTime,
            CreateBy = N(x.CreateBy)
        };
        async Task<string ?> Validate(Guid reviewId, CreateWfpPerformanceReviewDetailRequest r, Guid? id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(r.IndicatorName) && !r.KpiCatalogId.HasValue && !r.PerformanceTemplateDetailId.HasValue) return "Indicator name atau referensi master wajib diisi.";
            if (!Types.Contains(r.DetailType, StringComparer.OrdinalIgnoreCase)) return "Detail type tidak valid.";
            if (r.Weight<0 || r.Weight> 100) return "Weight harus 0 sampai 100.";
            if (r.Sequence<1) return "Sequence minimal 1.";
            if (r.KpiCatalogId.HasValue && !await _db.Set<MstKpiCatalog> ().AnyAsync(x => x.Id == r.KpiCatalogId && x.IsActive && !x.IsDelete, ct)) return "KPI catalog tidak valid.";
            if (r.PerformanceTemplateDetailId.HasValue && !await _db.Set<MstPerformanceTemplateDetail> ().AnyAsync(x => x.Id == r.PerformanceTemplateDetailId && x.IsActive && !x.IsDelete, ct)) return "Performance template detail tidak valid.";
            if (await _db.Set<WfpPerformanceReviewDetail> ().AnyAsync(x => x.PerformanceReviewId == reviewId && x.Sequence == r.Sequence && !x.IsDelete && (!id.HasValue || x.Id != id), ct)) return "Sequence sudah digunakan pada review.";
            return null;
        }
        async Task<(string? Code, string Name, string? Description, decimal Weight, decimal? Target)> ResolveMaster(CreateWfpPerformanceReviewDetailRequest r, CancellationToken ct)
        {
            if (r.PerformanceTemplateDetailId.HasValue)
            {
                var x = await _db.Set<MstPerformanceTemplateDetail> ().AsNoTracking().FirstAsync(x => x.Id == r.PerformanceTemplateDetailId, ct);
                return(x.DetailCode, x.DetailName, x.Description, x.Weight, x.TargetValue);
            }
            if (r.KpiCatalogId.HasValue)
            {
                var x = await _db.Set<MstKpiCatalog> ().AsNoTracking().FirstAsync(x => x.Id == r.KpiCatalogId, ct);
                return(x.KpiCode, x.KpiName, x.Description, x.DefaultWeight, x.DefaultTargetValue);
            }
            return(null, r.IndicatorName.Trim(), T(r.Description), r.Weight, r.TargetValue);
        }
        async Task RecalculateReview(Guid id, CancellationToken ct)
        {
            var r = await _db.Set<WfpPerformanceReview> ().FirstAsync(x => x.Id == id, ct);
            var rows = await _db.Set<WfpPerformanceReviewDetail> ().Where(x => x.PerformanceReviewId == id && x.IsActive && !x.IsDelete && (x.FinalScore.HasValue || x.Score.HasValue)).ToListAsync(ct);
            if (rows.Count == 0) r.OverallScore = 0m;
            else
            {
                var weight = rows.Sum(x => x.Weight);
                r.OverallScore = weight> 0? rows.Sum(x => (x.FinalScore ?? x.Score ?? 0m) * x.Weight) / weight : rows.Average(x => x.FinalScore ?? x.Score ?? 0m);
            }
            r.UpdateDateTime = DateTime.UtcNow;
            r.UpdateBy = Actor();
            await _db.SaveChangesAsync(ct);
        }
        async Task<bool> ReviewExists(Guid wf, Guid id, CancellationToken ct) => await _db.Set<WfpPerformanceReview> ().AnyAsync(x => x.Id == id && x.WorkforceProfileId == wf && !x.IsDelete, ct);
        async Task<WfpPerformanceReview ?> GetEditableReview(Guid wf, Guid id, CancellationToken ct) => await _db.Set<WfpPerformanceReview> ().FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == wf && !x.IsDelete && !x.IsFinalized, ct);
        IActionResult NF() => NotFound(ApiResponse<object>.Fail(404, "Performance review tidak ditemukan."));
        IActionResult NFOrFinalized(Guid wf, Guid id, CancellationToken ct)
        {
            var exists = _db.Set<WfpPerformanceReview> ().AsNoTracking().Any(x => x.Id == id && x.WorkforceProfileId == wf && !x.IsDelete);
            return exists? BadRequest(ApiResponse<object>.Fail(400, "Performance review finalized tidak dapat diubah.")): NF();
        }
        Guid Actor()
        {
            var s = User.FindFirstValue("user_id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(s, out var g)? g : Guid.Empty;
        }
        static string Canon(string v) => Types.First(x => x.Equals(v.Trim(), StringComparison.OrdinalIgnoreCase));
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
