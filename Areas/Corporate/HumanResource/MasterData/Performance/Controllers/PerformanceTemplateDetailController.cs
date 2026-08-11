using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Performance.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Performance.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Controllers;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Performance.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/performance-templates/{performanceTemplateId:guid}/details")]
    [Tags("Corporate / Human Resource / Master Data / Performance Template Detail")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Performance Template Detail",
        AreaName = "Corporate",
        ControllerName = "PerformanceTemplateDetail",
        Description = "Master performance template detail",
        SortOrder = 34
    )]
    public class PerformanceTemplateDetailController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly LoggerService _log;
        private static readonly string[]
        Types =
        {
            "Section", "KPI", "Competency", "Behavior", "Goal", "Custom"
        };
        private static readonly string[]
        Methods =
        {
            "RatingScale", "PercentageAchievement", "Binary", "Manual", "Formula"
        };
        public PerformanceTemplateDetailController(ApplicationDbContext db, LoggerService log)
        {
            _db = db;
            _log = log;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Performance Template Detail", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PerformanceTemplateDetail", "Read")]
        public IActionResult Metadata() => Ok(ApiResponse<PerformanceTemplateDetailFilterMetadataResponse>.Ok(new()
        {
            DefaultFilter = new(), CustomPeriods = BuildPeriodOptions(), DetailTypeOptions = Types.Select(x => new PerformanceStringOptionResponse
            {
                Value = x, Label = x
            }
            ).ToList(), ScoreMethodOptions = Methods.Select(x => new PerformanceStringOptionResponse
            {
                Value = x, Label = x
            }
            ).ToList(), SortOptions = new()
            {
                new()
                {
                    Value = "sortOrder", Label = "Urutan"
                }, new()
                {
                    Value = "detailName", Label = "Nama detail"
                }, new()
                {
                    Value = "weight", Label = "Bobot"
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
        [AccessAction("Read", "Read Performance Template Detail", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PerformanceTemplateDetail", "Read")]
        public async Task<IActionResult> Summary(Guid performanceTemplateId, CancellationToken ct)
        {
            if (!await TemplateExists(performanceTemplateId, ct)) return NF();
            var q = Q(performanceTemplateId);
            return Ok(ApiResponse<PerformanceTemplateDetailSummaryResponse>.Ok(new()
            {
                TotalData = await q.CountAsync(ct), ActiveData = await q.CountAsync(x => x.IsActive, ct), RequiredData = await q.CountAsync(x => x.IsRequired, ct), TotalWeight = await q.Where(x => x.IsActive).SumAsync(x => x.Weight, ct)
            }, "Ringkasan berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read Performance Template Detail", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PerformanceTemplateDetail", "Read")]
        public async Task<IActionResult> List(Guid performanceTemplateId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string? customPeriod, [FromQuery] string? detailType, [FromQuery] string? scoreMethod, [FromQuery] bool? isRequired, [FromQuery] bool? isActive, [FromQuery] string? search, [FromQuery] string? sortBy = "sortOrder", [FromQuery] string? sortDirection = "asc", [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 25, CancellationToken ct = default)
        {
            if (!await TemplateExists(performanceTemplateId, ct)) return NF();
            Norm(ref pageNumber, ref pageSize);
            var q = Filter(Q(performanceTemplateId), detailType, scoreMethod, isRequired, isActive, search);
            q = WorkflowMasterDataSupport.ApplyDateFilter(q, startDate, endDate, customPeriod);
            var total = await q.CountAsync(ct);
            var rows = await Sort(q, sortBy, sortDirection).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);
            return Ok(ApiResponse<PagedResult<PerformanceTemplateDetailResponse>>.Ok(new()
            {
                PageNumber = pageNumber, PageSize = pageSize, TotalData = total, TotalPage = (int) Math.Ceiling(total / (double) pageSize), Items = rows.Select(Map).ToList()
            }, "Data berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Performance Template Detail", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PerformanceTemplateDetail", "Read")]
        public async Task<IActionResult> Detail(Guid performanceTemplateId, Guid id, CancellationToken ct)
        {
            var x = await Q(performanceTemplateId).FirstOrDefaultAsync(x => x.Id == id, ct);
            if (x == null) return NotFound(ApiResponse<object>.Fail(404, "Template detail tidak ditemukan."));
            var r = new PerformanceTemplateDetailDetailResponse
            {
                Id = x.Id, PerformanceTemplateId = x.PerformanceTemplateId, ParentDetailId = x.ParentDetailId, KpiCatalogId = x.KpiCatalogId,
                CompetencyId = x.CompetencyId, RatingScaleId = x.RatingScaleId, DetailCode = x.DetailCode, DetailName = x.DetailName,
                DetailType = x.DetailType, Weight = x.Weight, TargetValue = x.TargetValue, MinimumTargetValue = x.MinimumTargetValue,
                MaximumTargetValue = x.MaximumTargetValue, MeasurementUnit = x.MeasurementUnit, ScoreMethod = x.ScoreMethod, TargetDirection = x.TargetDirection,
                IsRequired = x.IsRequired, SortOrder = x.SortOrder, IsActive = x.IsActive, Description = x.Description, EvidenceRequirement = x.EvidenceRequirement,
                AllowEmployeeComment = x.AllowEmployeeComment, AllowReviewerComment = x.AllowReviewerComment, CreateDateTime = x.CreateDateTime,
                CreateBy = N(x.CreateBy), UpdateDateTime = x.UpdateDateTime, UpdateBy = N(x.UpdateBy)
            };
            return Ok(ApiResponse<PerformanceTemplateDetailDetailResponse>.Ok(r, "Detail berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Create", "Create Performance Template Detail", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("PerformanceTemplateDetail", "Create")]
        public async Task<IActionResult> Create(Guid performanceTemplateId, [FromBody] CreatePerformanceTemplateDetailRequest r, CancellationToken ct)
        {
            if (!await TemplateExists(performanceTemplateId, ct)) return NF();
            var e = await Validate(performanceTemplateId, r, null, ct);
            if (e != null) return BadRequest(ApiResponse<object>.Fail(400, e));
            var x = new MstPerformanceTemplateDetail
            {
                Id = Guid.NewGuid(), PerformanceTemplateId = performanceTemplateId, ParentDetailId = NG(r.ParentDetailId), KpiCatalogId = NG(r.KpiCatalogId),
                CompetencyId = NG(r.CompetencyId), RatingScaleId = NG(r.RatingScaleId), DetailCode = r.DetailCode.Trim(), DetailName = r.DetailName.Trim(),
                DetailType = Canon(r.DetailType, Types), Description = T(r.Description), Weight = r.Weight, TargetValue = r.TargetValue,
                MinimumTargetValue = r.MinimumTargetValue, MaximumTargetValue = r.MaximumTargetValue, MeasurementUnit = T(r.MeasurementUnit),
                ScoreMethod = Canon(r.ScoreMethod, Methods), TargetDirection = T(r.TargetDirection), EvidenceRequirement = T(r.EvidenceRequirement),
                IsRequired = r.IsRequired, AllowEmployeeComment = r.AllowEmployeeComment, AllowReviewerComment = r.AllowReviewerComment,
                SortOrder = r.SortOrder, IsActive = true, CreateDateTime = DateTime.UtcNow, CreateBy = Actor(), IsDelete = false,
                IsCancel = false
            };
            _db.Set<MstPerformanceTemplateDetail> ().Add(x);
            await _db.SaveChangesAsync(ct);
            return await Detail(performanceTemplateId, x.Id, ct);
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Performance Template Detail", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("PerformanceTemplateDetail", "Update")]
        public async Task<IActionResult> Update(Guid performanceTemplateId, Guid id, [FromBody] UpdatePerformanceTemplateDetailRequest r, CancellationToken ct)
        {
            var x = await _db.Set<MstPerformanceTemplateDetail> ().FirstOrDefaultAsync(x => x.Id == id && x.PerformanceTemplateId == performanceTemplateId && !x.IsDelete, ct);
            if (x == null) return NotFound(ApiResponse<object>.Fail(404, "Template detail tidak ditemukan."));
            var e = await Validate(performanceTemplateId, r, id, ct);
            if (e != null) return BadRequest(ApiResponse<object>.Fail(400, e));
            x.ParentDetailId = NG(r.ParentDetailId);
            x.KpiCatalogId = NG(r.KpiCatalogId);
            x.CompetencyId = NG(r.CompetencyId);
            x.RatingScaleId = NG(r.RatingScaleId);
            x.DetailCode = r.DetailCode.Trim();
            x.DetailName = r.DetailName.Trim();
            x.DetailType = Canon(r.DetailType, Types);
            x.Description = T(r.Description);
            x.Weight = r.Weight;
            x.TargetValue = r.TargetValue;
            x.MinimumTargetValue = r.MinimumTargetValue;
            x.MaximumTargetValue = r.MaximumTargetValue;
            x.MeasurementUnit = T(r.MeasurementUnit);
            x.ScoreMethod = Canon(r.ScoreMethod, Methods);
            x.TargetDirection = T(r.TargetDirection);
            x.EvidenceRequirement = T(r.EvidenceRequirement);
            x.IsRequired = r.IsRequired;
            x.AllowEmployeeComment = r.AllowEmployeeComment;
            x.AllowReviewerComment = r.AllowReviewerComment;
            x.SortOrder = r.SortOrder;
            x.IsActive = r.IsActive;
            x.UpdateDateTime = DateTime.UtcNow;
            x.UpdateBy = Actor();
            await _db.SaveChangesAsync(ct);
            return await Detail(performanceTemplateId, id, ct);
        }

        [HttpPatch("{id:guid}/status")]
        [AccessAction("Update", "Update Performance Template Detail Status", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("PerformanceTemplateDetail", "Update")]
        public async Task<IActionResult> Status(Guid performanceTemplateId, Guid id, [FromBody] UpdatePerformanceTemplateDetailStatusRequest r, CancellationToken ct)
        {
            var x = await _db.Set<MstPerformanceTemplateDetail> ().FirstOrDefaultAsync(x => x.Id == id && x.PerformanceTemplateId == performanceTemplateId && !x.IsDelete, ct);
            if (x == null) return NotFound(ApiResponse<object>.Fail(404, "Template detail tidak ditemukan."));
            x.IsActive = r.IsActive;
            x.UpdateDateTime = DateTime.UtcNow;
            x.UpdateBy = Actor();
            await _db.SaveChangesAsync(ct);
            return Ok(ApiResponse<object>.Ok(null, "Status berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete Performance Template Detail", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("PerformanceTemplateDetail", "Delete")]
        public async Task<IActionResult> Delete(Guid performanceTemplateId, Guid id, CancellationToken ct)
        {
            var x = await _db.Set<MstPerformanceTemplateDetail> ().FirstOrDefaultAsync(x => x.Id == id && x.PerformanceTemplateId == performanceTemplateId && !x.IsDelete, ct);
            if (x == null) return NotFound(ApiResponse<object>.Fail(404, "Template detail tidak ditemukan."));
            if (await _db.Set<MstPerformanceTemplateDetail> ().AnyAsync(d => d.ParentDetailId == id && !d.IsDelete, ct)) return BadRequest(ApiResponse<object>.Fail(400, "Template detail masih memiliki child detail."));
            var now = DateTime.UtcNow;
            x.IsDelete = true;
            x.IsActive = false;
            x.DeleteDateTime = now;
            x.DeleteBy = Actor();
            x.UpdateDateTime = now;
            x.UpdateBy = Actor();
            await _db.SaveChangesAsync(ct);
            return Ok(ApiResponse<object>.Ok(null, "Template detail berhasil dihapus."));
        }
        IQueryable<MstPerformanceTemplateDetail> Q(Guid id) => _db.Set<MstPerformanceTemplateDetail> ().AsNoTracking().Where(x => x.PerformanceTemplateId == id && !x.IsDelete);
        static IQueryable<MstPerformanceTemplateDetail> Filter(IQueryable<MstPerformanceTemplateDetail> q, string? t, string? m, bool? r, bool? a, string? s)
        {
            if (!string.IsNullOrWhiteSpace(t)) q = q.Where(x => x.DetailType == t);
            if (!string.IsNullOrWhiteSpace(m)) q = q.Where(x => x.ScoreMethod == m);
            if (r.HasValue) q = q.Where(x => x.IsRequired == r);
            if (a.HasValue) q = q.Where(x => x.IsActive == a);
            if (!string.IsNullOrWhiteSpace(s))
            {
                var k = s.Trim().ToLower();
                q = q.Where(x => x.DetailCode.ToLower().Contains(k) || x.DetailName.ToLower().Contains(k) || (x.Description != null && x.Description.ToLower().Contains(k)));
            }
            return q;
        }
        static IOrderedQueryable<MstPerformanceTemplateDetail> Sort(IQueryable<MstPerformanceTemplateDetail> q, string? b, string? d)
        {
            var z = string.Equals(d, "desc", StringComparison.OrdinalIgnoreCase);
            return(b ?? "").ToLowerInvariant() switch
            {
                "detailname" => z? q.OrderByDescending(x => x.DetailName): q.OrderBy(x => x.DetailName), "weight" => z? q.OrderByDescending(x => x.Weight): q.OrderBy(x => x.Weight),
                "createdatetime" => z? q.OrderByDescending(x => x.CreateDateTime): q.OrderBy(x => x.CreateDateTime), _ => z? q.OrderByDescending(x => x.SortOrder): q.OrderBy(x => x.SortOrder)
            };
        }
        PerformanceTemplateDetailResponse Map(MstPerformanceTemplateDetail x) => new()
        {
            Id = x.Id, PerformanceTemplateId = x.PerformanceTemplateId, ParentDetailId = x.ParentDetailId, KpiCatalogId = x.KpiCatalogId,
            CompetencyId = x.CompetencyId, RatingScaleId = x.RatingScaleId, DetailCode = x.DetailCode, DetailName = x.DetailName,
            DetailType = x.DetailType, Weight = x.Weight, TargetValue = x.TargetValue, MinimumTargetValue = x.MinimumTargetValue,
            MaximumTargetValue = x.MaximumTargetValue, MeasurementUnit = x.MeasurementUnit, ScoreMethod = x.ScoreMethod, TargetDirection = x.TargetDirection,
            IsRequired = x.IsRequired, SortOrder = x.SortOrder, IsActive = x.IsActive, Description = x.Description, CreateDateTime = x.CreateDateTime,
            CreateBy = N(x.CreateBy)
        };
        async Task<string ?> Validate(Guid templateId, CreatePerformanceTemplateDetailRequest r, Guid? id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(r.DetailCode) || string.IsNullOrWhiteSpace(r.DetailName)) return "Kode dan nama detail wajib diisi.";
            if (!Types.Contains(r.DetailType, StringComparer.OrdinalIgnoreCase)) return "Detail type tidak valid.";
            if (!Methods.Contains(r.ScoreMethod, StringComparer.OrdinalIgnoreCase)) return "Score method tidak valid.";
            if (r.Weight<0 || r.Weight> 100) return "Weight harus 0 sampai 100.";
            if (r.MinimumTargetValue.HasValue && r.MaximumTargetValue.HasValue && r.MaximumTargetValue<r.MinimumTargetValue) return "Rentang target tidak valid.";
            if (r.ParentDetailId.HasValue && r.ParentDetailId == id) return "Parent detail tidak boleh dirinya sendiri.";
            if (r.ParentDetailId.HasValue && !await _db.Set<MstPerformanceTemplateDetail> ().AnyAsync(x => x.Id == r.ParentDetailId && x.PerformanceTemplateId == templateId && !x.IsDelete, ct)) return "Parent detail tidak valid.";
            if (r.KpiCatalogId.HasValue && !await _db.Set<MstKpiCatalog> ().AnyAsync(x => x.Id == r.KpiCatalogId && x.IsActive && !x.IsDelete, ct)) return "KPI catalog tidak valid.";
            if (r.CompetencyId.HasValue && !await _db.Set<MstCompetency> ().AnyAsync(x => x.Id == r.CompetencyId && x.IsActive && !x.IsDelete, ct)) return "Competency tidak valid.";
            if (r.RatingScaleId.HasValue && !await _db.Set<MstPerformanceRatingScale> ().AnyAsync(x => x.Id == r.RatingScaleId && x.IsActive && !x.IsDelete, ct)) return "Rating scale tidak valid.";
            var c = r.DetailCode.Trim().ToLower();
            if (await _db.Set<MstPerformanceTemplateDetail> ().AnyAsync(x => x.PerformanceTemplateId == templateId && !x.IsDelete && x.DetailCode.ToLower() == c && (!id.HasValue || x.Id != id), ct)) return "Detail code sudah digunakan pada template.";
            return null;
        }
        async Task<bool> TemplateExists(Guid id, CancellationToken ct) => await _db.Set<MstPerformanceTemplate> ().AnyAsync(x => x.Id == id && !x.IsDelete, ct);
        IActionResult NF() => NotFound(ApiResponse<object>.Fail(404, "Performance template tidak ditemukan."));
        Guid Actor()
        {
            var s = User.FindFirstValue("user_id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(s, out var g)? g : Guid.Empty;
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

        private static List<PerformanceCustomPeriodOptionResponse> BuildPeriodOptions()
        {
            return new List<PerformanceCustomPeriodOptionResponse>
            {
                new() { Value = "today", Label = "Hari ini" },
                new() { Value = "last7days", Label = "7 hari terakhir" },
                new() { Value = "thismonth", Label = "Bulan ini" },
                new() { Value = "lastmonth", Label = "Bulan lalu" }
            };
        }
    }
}
