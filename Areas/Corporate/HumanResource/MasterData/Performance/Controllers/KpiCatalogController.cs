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

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Performance.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/kpi-catalogs")]
    [Tags("Corporate / Human Resource / Master Data / KPI Catalog")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "KPI Catalog",
        AreaName = "Corporate",
        ControllerName = "KpiCatalog",
        Description = "Master KPI catalog",
        SortOrder = 32
    )]
    public class KpiCatalogController : ControllerBase
    {
        private const string Prefix = "KPI-RSMMC-";
        private readonly ApplicationDbContext _db;
        private readonly LoggerService _log;
        private static readonly string[]
        Directions =
        {
            "HigherIsBetter", "LowerIsBetter", "ExactTarget", "RangeTarget", "Milestone"
        };
        private static readonly string[]
        Frequencies =
        {
            "Daily", "Weekly", "Monthly", "Quarter", "Semester", "Annual", "OnDemand"
        };
        public KpiCatalogController(ApplicationDbContext db, LoggerService log)
        {
            _db = db;
            _log = log;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read KPI Catalog", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("KpiCatalog", "Read")]
        public IActionResult Metadata() => Ok(ApiResponse<KpiCatalogFilterMetadataResponse>.Ok(new()
        {
            DefaultFilter = new(), TargetDirectionOptions = Directions.Select(x => new PerformanceStringOptionResponse
            {
                Value = x, Label = x
            }
            ).ToList(), MeasurementFrequencyOptions = Frequencies.Select(x => new PerformanceStringOptionResponse
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
                    Value = "kpiName", Label = "Nama KPI"
                }, new()
                {
                    Value = "defaultWeight", Label = "Bobot default"
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
        [AccessAction("Read", "Read KPI Catalog", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("KpiCatalog", "Read")]
        public async Task<IActionResult> Summary(CancellationToken ct)
        {
            var q = Q();
            return Ok(ApiResponse<KpiCatalogSummaryResponse>.Ok(new()
            {
                TotalData = await q.CountAsync(ct), ActiveData = await q.CountAsync(x => x.IsActive, ct), InactiveData = await q.CountAsync(x => !x.IsActive, ct), QuantitativeData = await q.CountAsync(x => x.IsQuantitative, ct), CascadableData = await q.CountAsync(x => x.IsCascadable, ct)
            }, "Ringkasan berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read KPI Catalog", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("KpiCatalog", "Read")]
        public async Task<IActionResult> List([FromQuery] Guid? organizationUnitId, [FromQuery] Guid? departmentId, [FromQuery] Guid? positionId, [FromQuery] string? kpiCategory, [FromQuery] string? targetDirection, [FromQuery] string? measurementFrequency, [FromQuery] bool? isQuantitative, [FromQuery] bool? isCascadable, [FromQuery] bool? isActive, [FromQuery] string? search, [FromQuery] string? sortBy = "sortOrder", [FromQuery] string? sortDirection = "asc", [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 25, CancellationToken ct = default)
        {
            Norm(ref pageNumber, ref pageSize);
            var q = Filter(Q(), organizationUnitId, departmentId, positionId, kpiCategory, targetDirection, measurementFrequency, isQuantitative, isCascadable, isActive, search);
            var total = await q.CountAsync(ct);
            var rows = await Sort(q, sortBy, sortDirection).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);
            return Ok(ApiResponse<PagedResult<KpiCatalogResponse>>.Ok(new()
            {
                PageNumber = pageNumber, PageSize = pageSize, TotalData = total, TotalPage = (int) Math.Ceiling(total / (double) pageSize), Items = rows.Select(Map).ToList()
            }, "Data berhasil diambil."));
        }

        [HttpGet("options")]
        [AccessAction("Read", "Read KPI Catalog", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("KpiCatalog", "Read")]
        public async Task<IActionResult> Options([FromQuery] string? search, [FromQuery] bool onlyActive = true, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 25, CancellationToken ct = default)
        {
            Norm(ref pageNumber, ref pageSize);
            var q = Filter(Q(), null, null, null, null, null, null, null, null, onlyActive? true : null, search);
            var total = await q.CountAsync(ct);
            var items = await q.OrderBy(x => x.SortOrder).ThenBy(x => x.KpiName).Skip((pageNumber - 1) * pageSize).Take(pageSize).Select(x => new KpiCatalogOptionResponse
            {
                Id = x.Id, Code = x.KpiCode, Name = x.KpiName, Category = x.KpiCategory, TargetDirection = x.TargetDirection, MeasurementFrequency = x.MeasurementFrequency, MeasurementUnit = x.MeasurementUnit, DefaultTargetValue = x.DefaultTargetValue, DefaultWeight = x.DefaultWeight
            }
            ).ToListAsync(ct);
            return Ok(ApiResponse<KpiCatalogOptionPagedResponse>.Ok(new()
            {
                PageNumber = pageNumber, PageSize = pageSize, TotalData = total, TotalPage = (int) Math.Ceiling(total / (double) pageSize), Items = items
            }, "Pilihan berhasil diambil."));
        }

        [HttpGet("{id : guid}")]
        [AccessAction("Read", "Read KPI Catalog", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("KpiCatalog", "Read")]
        public async Task<IActionResult> Detail(Guid id, CancellationToken ct)
        {
            var x = await Q().FirstOrDefaultAsync(x => x.Id == id, ct);
            if (x == null) return NotFound(ApiResponse<object>.Fail(404, "KPI catalog tidak ditemukan."));
            var r = new KpiCatalogDetailResponse
            {
                Id = x.Id, OrganizationUnitId = x.OrganizationUnitId, DepartmentId = x.DepartmentId, PositionId = x.PositionId,
                KpiCode = x.KpiCode, KpiName = x.KpiName, KpiCategory = x.KpiCategory, MeasurementUnit = x.MeasurementUnit, TargetDirection = x.TargetDirection,
                MeasurementFrequency = x.MeasurementFrequency, DefaultTargetValue = x.DefaultTargetValue, MinimumTargetValue = x.MinimumTargetValue,
                MaximumTargetValue = x.MaximumTargetValue, DefaultWeight = x.DefaultWeight, IsQuantitative = x.IsQuantitative, IsCascadable = x.IsCascadable,
                SortOrder = x.SortOrder, IsActive = x.IsActive, TemplateDetailCount = x.TemplateDetails.Count(t => !t.IsDelete),
                Description = x.Description, DataSource = x.DataSource, CalculationFormula = x.CalculationFormula, CreateDateTime = x.CreateDateTime,
                CreateBy = N(x.CreateBy), UpdateDateTime = x.UpdateDateTime, UpdateBy = N(x.UpdateBy)
            };
            return Ok(ApiResponse<KpiCatalogDetailResponse>.Ok(r, "Detail berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Create", "Create KPI Catalog", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("KpiCatalog", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateKpiCatalogRequest r, CancellationToken ct)
        {
            var e = await Validate(r, null, ct);
            if (e != null) return BadRequest(ApiResponse<object>.Fail(400, e));
            var x = new MstKpiCatalog
            {
                Id = Guid.NewGuid(), OrganizationUnitId = NG(r.OrganizationUnitId), DepartmentId = NG(r.DepartmentId), PositionId = NG(r.PositionId),
                KpiCode = await Code(ct), KpiName = r.KpiName.Trim(), KpiCategory = r.KpiCategory.Trim(), Description = T(r.Description),
                MeasurementUnit = T(r.MeasurementUnit), TargetDirection = Canon(r.TargetDirection, Directions), MeasurementFrequency = Canon(r.MeasurementFrequency, Frequencies),
                DataSource = T(r.DataSource), CalculationFormula = T(r.CalculationFormula), DefaultTargetValue = r.DefaultTargetValue,
                MinimumTargetValue = r.MinimumTargetValue, MaximumTargetValue = r.MaximumTargetValue, DefaultWeight = r.DefaultWeight,
                IsQuantitative = r.IsQuantitative, IsCascadable = r.IsCascadable, SortOrder = r.SortOrder, IsActive = true, CreateDateTime = DateTime.UtcNow,
                CreateBy = Actor(), IsDelete = false, IsCancel = false
            };
            _db.Set<MstKpiCatalog> ().Add(x);
            await _db.SaveChangesAsync(ct);
            return await Detail(x.Id, ct);
        }

        [HttpPut("{id : guid}")]
        [AccessAction("Update", "Update KPI Catalog", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("KpiCatalog", "Update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateKpiCatalogRequest r, CancellationToken ct)
        {
            var x = await _db.Set<MstKpiCatalog> ().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);
            if (x == null) return NotFound(ApiResponse<object>.Fail(404, "KPI catalog tidak ditemukan."));
            var e = await Validate(r, id, ct);
            if (e != null) return BadRequest(ApiResponse<object>.Fail(400, e));
            x.OrganizationUnitId = NG(r.OrganizationUnitId);
            x.DepartmentId = NG(r.DepartmentId);
            x.PositionId = NG(r.PositionId);
            x.KpiName = r.KpiName.Trim();
            x.KpiCategory = r.KpiCategory.Trim();
            x.Description = T(r.Description);
            x.MeasurementUnit = T(r.MeasurementUnit);
            x.TargetDirection = Canon(r.TargetDirection, Directions);
            x.MeasurementFrequency = Canon(r.MeasurementFrequency, Frequencies);
            x.DataSource = T(r.DataSource);
            x.CalculationFormula = T(r.CalculationFormula);
            x.DefaultTargetValue = r.DefaultTargetValue;
            x.MinimumTargetValue = r.MinimumTargetValue;
            x.MaximumTargetValue = r.MaximumTargetValue;
            x.DefaultWeight = r.DefaultWeight;
            x.IsQuantitative = r.IsQuantitative;
            x.IsCascadable = r.IsCascadable;
            x.SortOrder = r.SortOrder;
            x.IsActive = r.IsActive;
            x.UpdateDateTime = DateTime.UtcNow;
            x.UpdateBy = Actor();
            await _db.SaveChangesAsync(ct);
            return await Detail(id, ct);
        }

        [HttpPatch("{id : guid}/status")]
        [AccessAction("Update", "Update KPI Catalog Status", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("KpiCatalog", "Update")]
        public async Task<IActionResult> Status(Guid id, [FromBody] UpdateKpiCatalogStatusRequest r, CancellationToken ct)
        {
            var x = await _db.Set<MstKpiCatalog> ().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);
            if (x == null) return NotFound(ApiResponse<object>.Fail(404, "KPI catalog tidak ditemukan."));
            x.IsActive = r.IsActive;
            x.UpdateDateTime = DateTime.UtcNow;
            x.UpdateBy = Actor();
            await _db.SaveChangesAsync(ct);
            return Ok(ApiResponse<object>.Ok(null, "Status berhasil diperbarui."));
        }

        [HttpDelete("{id : guid}")]
        [AccessAction("Delete", "Delete KPI Catalog", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("KpiCatalog", "Delete")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var x = await _db.Set<MstKpiCatalog> ().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);
            if (x == null) return NotFound(ApiResponse<object>.Fail(404, "KPI catalog tidak ditemukan."));
            if (await _db.Set<MstPerformanceTemplateDetail> ().AnyAsync(t => t.KpiCatalogId == id && !t.IsDelete, ct)) return BadRequest(ApiResponse<object>.Fail(400, "KPI catalog sudah digunakan template detail."));
            var now = DateTime.UtcNow;
            x.IsDelete = true;
            x.IsActive = false;
            x.DeleteDateTime = now;
            x.DeleteBy = Actor();
            x.UpdateDateTime = now;
            x.UpdateBy = Actor();
            await _db.SaveChangesAsync(ct);
            return Ok(ApiResponse<object>.Ok(null, "KPI catalog berhasil dihapus."));
        }
        IQueryable<MstKpiCatalog> Q() => _db.Set<MstKpiCatalog> ().AsNoTracking().Include(x => x.TemplateDetails).Where(x => !x.IsDelete);
        static IQueryable<MstKpiCatalog> Filter(IQueryable<MstKpiCatalog> q, Guid? o, Guid? d, Guid? p, string? c, string? td, string? f, bool? iq, bool? ic, bool? a, string? s)
        {
            if (o.HasValue && o != Guid.Empty) q = q.Where(x => x.OrganizationUnitId == o);
            if (d.HasValue && d != Guid.Empty) q = q.Where(x => x.DepartmentId == d);
            if (p.HasValue && p != Guid.Empty) q = q.Where(x => x.PositionId == p);
            if (!string.IsNullOrWhiteSpace(c)) q = q.Where(x => x.KpiCategory == c);
            if (!string.IsNullOrWhiteSpace(td)) q = q.Where(x => x.TargetDirection == td);
            if (!string.IsNullOrWhiteSpace(f)) q = q.Where(x => x.MeasurementFrequency == f);
            if (iq.HasValue) q = q.Where(x => x.IsQuantitative == iq);
            if (ic.HasValue) q = q.Where(x => x.IsCascadable == ic);
            if (a.HasValue) q = q.Where(x => x.IsActive == a);
            if (!string.IsNullOrWhiteSpace(s))
            {
                var k = s.Trim().ToLower();
                q = q.Where(x => x.KpiCode.ToLower().Contains(k) || x.KpiName.ToLower().Contains(k) || x.KpiCategory.ToLower().Contains(k));
            }
            return q;
        }
        static IOrderedQueryable<MstKpiCatalog> Sort(IQueryable<MstKpiCatalog> q, string? b, string? d)
        {
            var z = string.Equals(d, "desc", StringComparison.OrdinalIgnoreCase);
            return(b ?? "").ToLowerInvariant() switch
            {
                "kpiname" => z? q.OrderByDescending(x => x.KpiName): q.OrderBy(x => x.KpiName), "defaultweight" => z? q.OrderByDescending(x => x.DefaultWeight): q.OrderBy(x => x.DefaultWeight),
                "createdatetime" => z? q.OrderByDescending(x => x.CreateDateTime): q.OrderBy(x => x.CreateDateTime), _ => z? q.OrderByDescending(x => x.SortOrder): q.OrderBy(x => x.SortOrder)
            };
        }
        KpiCatalogResponse Map(MstKpiCatalog x) => new()
        {
            Id = x.Id, OrganizationUnitId = x.OrganizationUnitId, DepartmentId = x.DepartmentId, PositionId = x.PositionId,
            KpiCode = x.KpiCode, KpiName = x.KpiName, KpiCategory = x.KpiCategory, MeasurementUnit = x.MeasurementUnit, TargetDirection = x.TargetDirection,
            MeasurementFrequency = x.MeasurementFrequency, DefaultTargetValue = x.DefaultTargetValue, MinimumTargetValue = x.MinimumTargetValue,
            MaximumTargetValue = x.MaximumTargetValue, DefaultWeight = x.DefaultWeight, IsQuantitative = x.IsQuantitative, IsCascadable = x.IsCascadable,
            SortOrder = x.SortOrder, IsActive = x.IsActive, TemplateDetailCount = x.TemplateDetails.Count(t => !t.IsDelete),
            Description = x.Description, CreateDateTime = x.CreateDateTime, CreateBy = N(x.CreateBy)
        };
        async Task<string ?> Validate(CreateKpiCatalogRequest r, Guid? id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(r.KpiName) || string.IsNullOrWhiteSpace(r.KpiCategory)) return "Nama dan kategori KPI wajib diisi.";
            if (!Directions.Contains(r.TargetDirection, StringComparer.OrdinalIgnoreCase)) return "Target direction tidak valid.";
            if (!Frequencies.Contains(r.MeasurementFrequency, StringComparer.OrdinalIgnoreCase)) return "Measurement frequency tidak valid.";
            if (r.DefaultWeight<0 || r.DefaultWeight> 100) return "Default weight harus 0 sampai 100.";
            if (r.MinimumTargetValue.HasValue && r.MaximumTargetValue.HasValue && r.MaximumTargetValue<r.MinimumTargetValue) return "Rentang target tidak valid.";
            var n = r.KpiName.Trim().ToLower();
            if (await _db.Set<MstKpiCatalog> ().AnyAsync(x => !x.IsDelete && x.KpiName.ToLower() == n && x.PositionId == NG(r.PositionId) && (!id.HasValue || x.Id != id), ct)) return "Nama KPI sudah digunakan pada scope position tersebut.";
            return null;
        }
        async Task<string> Code(CancellationToken ct)
        {
            var c = await _db.Set<MstKpiCatalog> ().Where(x => x.KpiCode.StartsWith(Prefix)).Select(x => x.KpiCode).ToListAsync(ct);
            return Next(c, Prefix);
        }
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
