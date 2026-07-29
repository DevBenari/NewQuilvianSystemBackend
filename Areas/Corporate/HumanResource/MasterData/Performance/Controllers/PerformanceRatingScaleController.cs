using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;
using System.Text.Json;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Performance.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Performance.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Performance.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/performance-rating-scales")]
    [Tags("Corporate / Human Resource / Master Data / Performance Rating Scale")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Performance Rating Scale",
        AreaName = "Corporate",
        ControllerName = "PerformanceRatingScale",
        Description = "Master performance rating scale",
        SortOrder = 31
    )]
    public class PerformanceRatingScaleController : ControllerBase
    {
        private const string Prefix = "PRS-RSMMC-";
        private readonly ApplicationDbContext _db;
        private readonly LoggerService _log;
        private static readonly string[]
        Types =
        {
            "Numeric", "Percentage", "Descriptive", "FivePoint", "Custom"
        };
        public PerformanceRatingScaleController(ApplicationDbContext db, LoggerService log)
        {
            _db = db;
            _log = log;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Performance Rating Scale", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PerformanceRatingScale", "Read")]
        public IActionResult Metadata() => Ok(ApiResponse<PerformanceRatingScaleFilterMetadataResponse>.Ok(new()
        {
            DefaultFilter = new(), ScaleTypeOptions = Types.Select(x => new PerformanceStringOptionResponse
            {
                Value = x, Label = x
            }
            ).ToList(), SortOptions = new()
            {
                new()
                {
                    Value = "scaleName", Label = "Nama skala"
                }, new()
                {
                    Value = "minimumScore", Label = "Nilai minimum"
                }, new()
                {
                    Value = "maximumScore", Label = "Nilai maksimum"
                }, new()
                {
                    Value = "isDefault", Label = "Default"
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
        [AccessAction("Read", "Read Performance Rating Scale", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PerformanceRatingScale", "Read")]
        public async Task<IActionResult> Summary(CancellationToken ct)
        {
            var q = Q();
            return Ok(ApiResponse<PerformanceRatingScaleSummaryResponse>.Ok(new()
            {
                TotalData = await q.CountAsync(ct), ActiveData = await q.CountAsync(x => x.IsActive, ct), InactiveData = await q.CountAsync(x => !x.IsActive, ct), DefaultData = await q.CountAsync(x => x.IsDefault, ct), NumericData = await q.CountAsync(x => x.ScaleType == "Numeric" || x.ScaleType == "FivePoint", ct)
            }, "Ringkasan berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read Performance Rating Scale", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PerformanceRatingScale", "Read")]
        public async Task<IActionResult> List([FromQuery] string? scaleType, [FromQuery] bool? isDefault, [FromQuery] bool? isActive, [FromQuery] string? search, [FromQuery] string? sortBy = "scaleName", [FromQuery] string? sortDirection = "asc", [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 25, CancellationToken ct = default)
        {
            Norm(ref pageNumber, ref pageSize);
            var q = Filter(Q(), scaleType, isDefault, isActive, search);
            var total = await q.CountAsync(ct);
            var rows = await Sort(q, sortBy, sortDirection).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);
            return Ok(ApiResponse<PagedResult<PerformanceRatingScaleResponse>>.Ok(new()
            {
                PageNumber = pageNumber, PageSize = pageSize, TotalData = total, TotalPage = (int) Math.Ceiling(total / (double) pageSize), Items = rows.Select(Map).ToList()
            }, "Data berhasil diambil."));
        }

        [HttpGet("options")]
        [AccessAction("Read", "Read Performance Rating Scale", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PerformanceRatingScale", "Read")]
        public async Task<IActionResult> Options([FromQuery] string? search, [FromQuery] bool onlyActive = true, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 25, CancellationToken ct = default)
        {
            Norm(ref pageNumber, ref pageSize);
            var q = Filter(Q(), null, null, onlyActive? true : null, search);
            var total = await q.CountAsync(ct);
            var items = await q.OrderBy(x => x.ScaleName).Skip((pageNumber - 1) * pageSize).Take(pageSize).Select(x => new PerformanceRatingScaleOptionResponse
            {
                Id = x.Id, Code = x.ScaleCode, Name = x.ScaleName, ScaleType = x.ScaleType, MinimumScore = x.MinimumScore, MaximumScore = x.MaximumScore, PassingScore = x.PassingScore
            }
            ).ToListAsync(ct);
            return Ok(ApiResponse<PerformanceRatingScaleOptionPagedResponse>.Ok(new()
            {
                PageNumber = pageNumber, PageSize = pageSize, TotalData = total, TotalPage = (int) Math.Ceiling(total / (double) pageSize), Items = items
            }, "Pilihan berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Performance Rating Scale", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PerformanceRatingScale", "Read")]
        public async Task<IActionResult> Detail(Guid id, CancellationToken ct)
        {
            var x = await Q().FirstOrDefaultAsync(x => x.Id == id, ct);
            if (x == null) return NotFound(ApiResponse<object>.Fail(404, "Rating scale tidak ditemukan."));
            var r = new PerformanceRatingScaleDetailResponse
            {
                Id = x.Id, ScaleCode = x.ScaleCode, ScaleName = x.ScaleName, ScaleType = x.ScaleType, MinimumScore = x.MinimumScore,
                MaximumScore = x.MaximumScore, PassingScore = x.PassingScore, DecimalPlaces = x.DecimalPlaces, IsHigherScoreBetter = x.IsHigherScoreBetter,
                IsDefault = x.IsDefault, IsActive = x.IsActive, TemplateCount = x.PerformanceTemplates.Count(t => !t.IsDelete),
                TemplateDetailCount = x.TemplateDetails.Count(t => !t.IsDelete), Description = x.Description, RatingDefinitionJson = x.RatingDefinitionJson,
                CreateDateTime = x.CreateDateTime, CreateBy = N(x.CreateBy), UpdateDateTime = x.UpdateDateTime, UpdateBy = N(x.UpdateBy)
            };
            return Ok(ApiResponse<PerformanceRatingScaleDetailResponse>.Ok(r, "Detail berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Create", "Create Performance Rating Scale", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("PerformanceRatingScale", "Create")]
        public async Task<IActionResult> Create([FromBody] CreatePerformanceRatingScaleRequest r, CancellationToken ct)
        {
            var e = await Validate(r, null, ct);
            if (e != null) return BadRequest(ApiResponse<object>.Fail(400, e));
            var now = DateTime.UtcNow;
            if (r.IsDefault) await ClearDefault(null, now, ct);
            var x = new MstPerformanceRatingScale
            {
                Id = Guid.NewGuid(), ScaleCode = await Code(ct), ScaleName = r.ScaleName.Trim(), ScaleType = Canon(r.ScaleType),
                MinimumScore = r.MinimumScore, MaximumScore = r.MaximumScore, PassingScore = r.PassingScore, DecimalPlaces = r.DecimalPlaces,
                IsHigherScoreBetter = r.IsHigherScoreBetter, RatingDefinitionJson = T(r.RatingDefinitionJson), IsDefault = r.IsDefault,
                Description = T(r.Description), IsActive = true, CreateDateTime = now, CreateBy = Actor(), IsDelete = false, IsCancel = false
            };
            _db.Set<MstPerformanceRatingScale> ().Add(x);
            await _db.SaveChangesAsync(ct);
            return await Detail(x.Id, ct);
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Performance Rating Scale", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("PerformanceRatingScale", "Update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePerformanceRatingScaleRequest r, CancellationToken ct)
        {
            var x = await _db.Set<MstPerformanceRatingScale> ().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);
            if (x == null) return NotFound(ApiResponse<object>.Fail(404, "Rating scale tidak ditemukan."));
            var e = await Validate(r, id, ct);
            if (e != null) return BadRequest(ApiResponse<object>.Fail(400, e));
            var now = DateTime.UtcNow;
            if (r.IsDefault) await ClearDefault(id, now, ct);
            x.ScaleName = r.ScaleName.Trim();
            x.ScaleType = Canon(r.ScaleType);
            x.MinimumScore = r.MinimumScore;
            x.MaximumScore = r.MaximumScore;
            x.PassingScore = r.PassingScore;
            x.DecimalPlaces = r.DecimalPlaces;
            x.IsHigherScoreBetter = r.IsHigherScoreBetter;
            x.RatingDefinitionJson = T(r.RatingDefinitionJson);
            x.IsDefault = r.IsDefault;
            x.Description = T(r.Description);
            x.IsActive = r.IsActive;
            x.UpdateDateTime = now;
            x.UpdateBy = Actor();
            await _db.SaveChangesAsync(ct);
            return await Detail(id, ct);
        }

        [HttpPatch("{id:guid}/status")]
        [AccessAction("Update", "Update Performance Rating Scale Status", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("PerformanceRatingScale", "Update")]
        public async Task<IActionResult> Status(Guid id, [FromBody] UpdatePerformanceRatingScaleStatusRequest r, CancellationToken ct)
        {
            var x = await _db.Set<MstPerformanceRatingScale> ().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);
            if (x == null) return NotFound(ApiResponse<object>.Fail(404, "Rating scale tidak ditemukan."));
            var now = DateTime.UtcNow;
            if (r.IsDefault == true) await ClearDefault(id, now, ct);
            x.IsActive = r.IsActive;
            if (r.IsDefault.HasValue) x.IsDefault = r.IsDefault.Value && r.IsActive;
            x.UpdateDateTime = now;
            x.UpdateBy = Actor();
            await _db.SaveChangesAsync(ct);
            return Ok(ApiResponse<object>.Ok(null, "Status berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete Performance Rating Scale", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("PerformanceRatingScale", "Delete")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var x = await _db.Set<MstPerformanceRatingScale> ().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);
            if (x == null) return NotFound(ApiResponse<object>.Fail(404, "Rating scale tidak ditemukan."));
            if (await _db.Set<MstPerformanceTemplate> ().AnyAsync(t => t.RatingScaleId == id && !t.IsDelete, ct) || await _db.Set<MstPerformanceTemplateDetail> ().AnyAsync(t => t.RatingScaleId == id && !t.IsDelete, ct)) return BadRequest(ApiResponse<object>.Fail(400, "Rating scale sudah digunakan."));
            var now = DateTime.UtcNow;
            x.IsDelete = true;
            x.IsActive = false;
            x.IsDefault = false;
            x.DeleteDateTime = now;
            x.DeleteBy = Actor();
            x.UpdateDateTime = now;
            x.UpdateBy = Actor();
            await _db.SaveChangesAsync(ct);
            return Ok(ApiResponse<object>.Ok(null, "Rating scale berhasil dihapus."));
        }
        IQueryable<MstPerformanceRatingScale> Q() => _db.Set<MstPerformanceRatingScale> ().AsNoTracking().Include(x => x.PerformanceTemplates).Include(x => x.TemplateDetails).Where(x => !x.IsDelete);
        static IQueryable<MstPerformanceRatingScale> Filter(IQueryable<MstPerformanceRatingScale> q, string? t, bool? d, bool? a, string? s)
        {
            if (!string.IsNullOrWhiteSpace(t)) q = q.Where(x => x.ScaleType == t);
            if (d.HasValue) q = q.Where(x => x.IsDefault == d);
            if (a.HasValue) q = q.Where(x => x.IsActive == a);
            if (!string.IsNullOrWhiteSpace(s))
            {
                var k = s.Trim().ToLower();
                q = q.Where(x => x.ScaleCode.ToLower().Contains(k) || x.ScaleName.ToLower().Contains(k) || (x.Description != null && x.Description.ToLower().Contains(k)));
            }
            return q;
        }
        static IOrderedQueryable<MstPerformanceRatingScale> Sort(IQueryable<MstPerformanceRatingScale> q, string? b, string? d)
        {
            var z = string.Equals(d, "desc", StringComparison.OrdinalIgnoreCase);
            return(b ?? "").ToLowerInvariant() switch
            {
                "minimumscore" => z? q.OrderByDescending(x => x.MinimumScore): q.OrderBy(x => x.MinimumScore), "maximumscore" => z? q.OrderByDescending(x => x.MaximumScore): q.OrderBy(x => x.MaximumScore),
                "isdefault" => z? q.OrderByDescending(x => x.IsDefault): q.OrderBy(x => x.IsDefault), "createdatetime" => z? q.OrderByDescending(x => x.CreateDateTime): q.OrderBy(x => x.CreateDateTime),
                _ => z? q.OrderByDescending(x => x.ScaleName): q.OrderBy(x => x.ScaleName)
            };
        }
        PerformanceRatingScaleResponse Map(MstPerformanceRatingScale x) => new()
        {
            Id = x.Id, ScaleCode = x.ScaleCode, ScaleName = x.ScaleName, ScaleType = x.ScaleType, MinimumScore = x.MinimumScore,
            MaximumScore = x.MaximumScore, PassingScore = x.PassingScore, DecimalPlaces = x.DecimalPlaces, IsHigherScoreBetter = x.IsHigherScoreBetter,
            IsDefault = x.IsDefault, IsActive = x.IsActive, TemplateCount = x.PerformanceTemplates.Count(t => !t.IsDelete),
            TemplateDetailCount = x.TemplateDetails.Count(t => !t.IsDelete), Description = x.Description, CreateDateTime = x.CreateDateTime,
            CreateBy = N(x.CreateBy)
        };
        async Task<string ?> Validate(CreatePerformanceRatingScaleRequest r, Guid? id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(r.ScaleName)) return "Nama skala wajib diisi.";
            if (!Types.Contains(r.ScaleType, StringComparer.OrdinalIgnoreCase)) return "Scale type tidak valid.";
            if (r.MaximumScore<= r.MinimumScore) return "Maximum score harus lebih besar dari minimum score.";
            if (r.PassingScore.HasValue && (r.PassingScore<r.MinimumScore || r.PassingScore> r.MaximumScore)) return "Passing score berada di luar rentang.";
            if (r.DecimalPlaces<0 || r.DecimalPlaces> 4) return "Decimal places harus 0 sampai 4.";
            if (!string.IsNullOrWhiteSpace(r.RatingDefinitionJson))
            {
                try
                {
                    JsonDocument.Parse(r.RatingDefinitionJson);
                } catch
                {
                    return "RatingDefinitionJson bukan JSON valid.";
                }
            }
            var n = r.ScaleName.Trim().ToLower();
            if (await _db.Set<MstPerformanceRatingScale> ().AnyAsync(x => !x.IsDelete && x.ScaleName.ToLower() == n && (!id.HasValue || x.Id != id), ct)) return "Nama skala sudah digunakan.";
            return null;
        }
        async Task ClearDefault(Guid? except, DateTime now, CancellationToken ct)
        {
            var rows = await _db.Set<MstPerformanceRatingScale> ().Where(x => x.IsDefault && !x.IsDelete && (!except.HasValue || x.Id != except)).ToListAsync(ct);
            foreach (var x in rows)
            {
                x.IsDefault = false;
                x.UpdateDateTime = now;
                x.UpdateBy = Actor();
            }
        }
        async Task<string> Code(CancellationToken ct)
        {
            var c = await _db.Set<MstPerformanceRatingScale> ().Where(x => x.ScaleCode.StartsWith(Prefix)).Select(x => x.ScaleCode).ToListAsync(ct);
            return Next(c, Prefix);
        }
        string Canon(string v) => Types.First(x => x.Equals(v.Trim(), StringComparison.OrdinalIgnoreCase));
        Guid Actor()
        {
            var s = User.FindFirstValue("user_id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(s, out var g)? g : Guid.Empty;
        }
        static string Next(IEnumerable<string> c, string p)
        {
            var u = c.Select(x => x.Replace(p, "")).Where(x => int.TryParse(x, out _)).Select(int.Parse).ToHashSet();
            var n = 1;
            while (u.Contains(n)) n++;
            return p + n.ToString("D5");
        }
        static Guid? N(Guid g) => g == Guid.Empty? null : g;
        static string? T(string? s) => string.IsNullOrWhiteSpace(s)? null : s.Trim();
        static void Norm(ref int p, ref int s)
        {
            p = Math.Max(1, p);
            s = Math.Min(100, Math.Max(1, s));
        }
    }
}
