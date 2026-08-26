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
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Controllers;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Performance.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/performance-cycles")]
    [Tags("Corporate / Human Resource / Master Data / Performance Cycle")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Performance Cycle",
        AreaName = "Corporate",
        ControllerName = "PerformanceCycle",
        Description = "Master performance cycle",
        SortOrder = 30
    )]
    public class PerformanceCycleController : ControllerBase
    {
        private const string Prefix = "PFC-RSMMC-";
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
            "Draft", "Open", "GoalSetting", "MidReview", "FinalReview", "Calibration", "Completed", "Closed", "Cancelled"
        };
        public PerformanceCycleController(ApplicationDbContext db, LoggerService log)
        {
            _db = db;
            _log = log;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Performance Cycle", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PerformanceCycle", "Read")]
        public IActionResult Metadata() => Ok(ApiResponse<PerformanceCycleFilterMetadataResponse>.Ok(new()
        {
            DefaultFilter = new(), CustomPeriods = BuildPeriodOptions(), CycleTypeOptions = Types.Select(x => new PerformanceStringOptionResponse
            {
                Value = x, Label = x
            }
            ).ToList(), CycleStatusOptions = Statuses.Select(x => new PerformanceStringOptionResponse
            {
                Value = x, Label = x
            }
            ).ToList(), SortOptions = new()
            {
                new()
                {
                    Value = "periodStartDate", Label = "Tanggal mulai"
                }, new()
                {
                    Value = "cycleName", Label = "Nama siklus"
                }, new()
                {
                    Value = "cycleStatus", Label = "Status"
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
        [AccessAction("Read", "Read Performance Cycle", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PerformanceCycle", "Read")]
        public async Task<IActionResult> Summary(CancellationToken ct)
        {
            var q = Q();
            return Ok(ApiResponse<PerformanceCycleSummaryResponse>.Ok(new()
            {
                TotalData = await q.CountAsync(ct), ActiveData = await q.CountAsync(x => x.IsActive, ct), CurrentData = await q.CountAsync(x => x.IsCurrent, ct), LockedData = await q.CountAsync(x => x.IsLocked, ct), OpenData = await q.CountAsync(x => x.CycleStatus == "Open", ct), CompletedData = await q.CountAsync(x => x.CycleStatus == "Completed", ct)
            }, "Ringkasan berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read Performance Cycle", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PerformanceCycle", "Read")]
        public async Task<IActionResult> List([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string? customPeriod, [FromQuery] string? cycleType, [FromQuery] string? cycleStatus, [FromQuery] bool? isCurrent, [FromQuery] bool? isLocked, [FromQuery] bool? isActive, [FromQuery] string? search, [FromQuery] string? sortBy = "periodStartDate", [FromQuery] string? sortDirection = "desc", [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 25, CancellationToken ct = default)
        {
            Norm(ref pageNumber, ref pageSize);
            var q = Filter(Q(), cycleType, cycleStatus, isCurrent, isLocked, isActive, search);
            q = WorkflowMasterDataSupport.ApplyDateFilter(q, startDate, endDate, customPeriod);
            var total = await q.CountAsync(ct);
            var rows = await Sort(q, sortBy, sortDirection).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);
            var items = rows.Select(Map).ToList();
            return Ok(ApiResponse<PagedResult<PerformanceCycleResponse>>.Ok(new()
            {
                PageNumber = pageNumber, PageSize = pageSize, TotalData = total, TotalPage = (int) Math.Ceiling(total / (double) pageSize), Items = items
            }, "Data berhasil diambil."));
        }

        [HttpGet("options")]
        [AccessAction("Read", "Read Performance Cycle", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PerformanceCycle", "Read")]
        public async Task<IActionResult> Options([FromQuery] string? search, [FromQuery] bool onlyActive = true, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 25, CancellationToken ct = default)
        {
            Norm(ref pageNumber, ref pageSize);
            var q = Filter(Q(), null, null, null, null, onlyActive? true : null, search);
            var total = await q.CountAsync(ct);
            var items = await q.OrderByDescending(x => x.PeriodStartDate).Skip((pageNumber - 1) * pageSize).Take(pageSize).Select(x => new PerformanceCycleOptionResponse
            {
                Id = x.Id, Code = x.CycleCode, Name = x.CycleName, CycleType = x.CycleType, CycleStatus = x.CycleStatus, PeriodStartDate = x.PeriodStartDate, PeriodEndDate = x.PeriodEndDate
            }
            ).ToListAsync(ct);
            return Ok(ApiResponse<PerformanceCycleOptionPagedResponse>.Ok(new()
            {
                PageNumber = pageNumber, PageSize = pageSize, TotalData = total, TotalPage = (int) Math.Ceiling(total / (double) pageSize), Items = items
            }, "Pilihan berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Performance Cycle", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PerformanceCycle", "Read")]
        public async Task<IActionResult> Detail(Guid id, CancellationToken ct)
        {
            var x = await Q().FirstOrDefaultAsync(x => x.Id == id, ct);
            if (x == null) return NotFound(ApiResponse<object>.Fail(404, "Performance cycle tidak ditemukan."));
            var r = new PerformanceCycleDetailResponse
            {
                Id = x.Id, LegalEntityId = x.LegalEntityId, HospitalSiteId = x.HospitalSiteId, CycleCode = x.CycleCode, CycleName = x.CycleName,
                CycleType = x.CycleType, PeriodYear = x.PeriodYear, PeriodStartDate = x.PeriodStartDate, PeriodEndDate = x.PeriodEndDate,
                CycleStatus = x.CycleStatus, IsCurrent = x.IsCurrent, IsLocked = x.IsLocked, IsActive = x.IsActive, TemplateCount = x.PerformanceTemplates.Count(d => !d.IsDelete),
                Description = x.Description, CreateDateTime = x.CreateDateTime, CreateBy = N(x.CreateBy), GoalSettingStartDate = x.GoalSettingStartDate,
                GoalSettingEndDate = x.GoalSettingEndDate, MidReviewStartDate = x.MidReviewStartDate, MidReviewEndDate = x.MidReviewEndDate,
                FinalReviewStartDate = x.FinalReviewStartDate, FinalReviewEndDate = x.FinalReviewEndDate, CalibrationStartDate = x.CalibrationStartDate,
                CalibrationEndDate = x.CalibrationEndDate, UpdateDateTime = x.UpdateDateTime, UpdateBy = N(x.UpdateBy)
            };
            return Ok(ApiResponse<PerformanceCycleDetailResponse>.Ok(r, "Detail berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Create", "Create Performance Cycle", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("PerformanceCycle", "Create")]
        public async Task<IActionResult> Create([FromBody] CreatePerformanceCycleRequest r, CancellationToken ct)
        {
            var e = await Validate(r, null, ct);
            if (e != null) return BadRequest(ApiResponse<object>.Fail(400, e));
            var now = DateTime.UtcNow;
            var x = new MstPerformanceCycle
            {
                Id = Guid.NewGuid(), LegalEntityId = NG(r.LegalEntityId), HospitalSiteId = NG(r.HospitalSiteId), CycleCode = await Code(ct),
                CycleName = r.CycleName.Trim(), CycleType = Canon(r.CycleType, Types), PeriodYear = r.PeriodYear, PeriodStartDate = r.PeriodStartDate.Date,
                PeriodEndDate = r.PeriodEndDate.Date, GoalSettingStartDate = D(r.GoalSettingStartDate), GoalSettingEndDate = D(r.GoalSettingEndDate),
                MidReviewStartDate = D(r.MidReviewStartDate), MidReviewEndDate = D(r.MidReviewEndDate), FinalReviewStartDate = D(r.FinalReviewStartDate),
                FinalReviewEndDate = D(r.FinalReviewEndDate), CalibrationStartDate = D(r.CalibrationStartDate), CalibrationEndDate = D(r.CalibrationEndDate),
                CycleStatus = Canon(r.CycleStatus, Statuses), IsCurrent = r.IsCurrent, IsLocked = r.IsLocked, Description = T(r.Description),
                IsActive = true, CreateDateTime = now, CreateBy = Actor(), IsDelete = false, IsCancel = false
            };
            if (x.IsCurrent) await ClearCurrent(null, now, ct);
            _db.Set<MstPerformanceCycle> ().Add(x);
            await _db.SaveChangesAsync(ct);
            return await Detail(x.Id, ct);
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Performance Cycle", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("PerformanceCycle", "Update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePerformanceCycleRequest r, CancellationToken ct)
        {
            var x = await _db.Set<MstPerformanceCycle> ().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);
            if (x == null) return NotFound(ApiResponse<object>.Fail(404, "Performance cycle tidak ditemukan."));
            var e = await Validate(r, id, ct);
            if (e != null) return BadRequest(ApiResponse<object>.Fail(400, e));
            var now = DateTime.UtcNow;
            if (r.IsCurrent) await ClearCurrent(id, now, ct);
            x.LegalEntityId = NG(r.LegalEntityId);
            x.HospitalSiteId = NG(r.HospitalSiteId);
            x.CycleName = r.CycleName.Trim();
            x.CycleType = Canon(r.CycleType, Types);
            x.PeriodYear = r.PeriodYear;
            x.PeriodStartDate = r.PeriodStartDate.Date;
            x.PeriodEndDate = r.PeriodEndDate.Date;
            x.GoalSettingStartDate = D(r.GoalSettingStartDate);
            x.GoalSettingEndDate = D(r.GoalSettingEndDate);
            x.MidReviewStartDate = D(r.MidReviewStartDate);
            x.MidReviewEndDate = D(r.MidReviewEndDate);
            x.FinalReviewStartDate = D(r.FinalReviewStartDate);
            x.FinalReviewEndDate = D(r.FinalReviewEndDate);
            x.CalibrationStartDate = D(r.CalibrationStartDate);
            x.CalibrationEndDate = D(r.CalibrationEndDate);
            x.CycleStatus = Canon(r.CycleStatus, Statuses);
            x.IsCurrent = r.IsCurrent;
            x.IsLocked = r.IsLocked;
            x.Description = T(r.Description);
            x.IsActive = r.IsActive;
            x.UpdateDateTime = now;
            x.UpdateBy = Actor();
            await _db.SaveChangesAsync(ct);
            return await Detail(id, ct);
        }

        [HttpPatch("{id:guid}/status")]
        [AccessAction("Update", "Update Performance Cycle Status", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("PerformanceCycle", "Update")]
        public async Task<IActionResult> Status(Guid id, [FromBody] UpdatePerformanceCycleStatusRequest r, CancellationToken ct)
        {
            var x = await _db.Set<MstPerformanceCycle> ().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);
            if (x == null) return NotFound(ApiResponse<object>.Fail(404, "Performance cycle tidak ditemukan."));
            if (!Statuses.Contains(r.CycleStatus, StringComparer.OrdinalIgnoreCase)) return BadRequest(ApiResponse<object>.Fail(400, "Cycle status tidak valid."));
            var now = DateTime.UtcNow;
            if (r.IsCurrent == true) await ClearCurrent(id, now, ct);
            x.CycleStatus = Canon(r.CycleStatus, Statuses);
            if (r.IsCurrent.HasValue) x.IsCurrent = r.IsCurrent.Value;
            if (r.IsLocked.HasValue) x.IsLocked = r.IsLocked.Value;
            if (r.IsActive.HasValue) x.IsActive = r.IsActive.Value;
            x.UpdateDateTime = now;
            x.UpdateBy = Actor();
            await _db.SaveChangesAsync(ct);
            return Ok(ApiResponse<object>.Ok(null, "Status berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete Performance Cycle", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("PerformanceCycle", "Delete")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var x = await _db.Set<MstPerformanceCycle> ().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);
            if (x == null) return NotFound(ApiResponse<object>.Fail(404, "Performance cycle tidak ditemukan."));
            if (await _db.Set<MstPerformanceTemplate> ().AnyAsync(t => t.PerformanceCycleId == id && !t.IsDelete, ct)) return BadRequest(ApiResponse<object>.Fail(400, "Performance cycle sudah digunakan template."));
            var now = DateTime.UtcNow;
            x.IsDelete = true;
            x.IsActive = false;
            x.IsCurrent = false;
            x.DeleteDateTime = now;
            x.DeleteBy = Actor();
            x.UpdateDateTime = now;
            x.UpdateBy = Actor();
            await _db.SaveChangesAsync(ct);
            return Ok(ApiResponse<object>.Ok(null, "Performance cycle berhasil dihapus."));
        }
        IQueryable<MstPerformanceCycle> Q() => _db.Set<MstPerformanceCycle> ().AsNoTracking().Include(x => x.PerformanceTemplates).Where(x => !x.IsDelete);
        static IQueryable<MstPerformanceCycle> Filter(IQueryable<MstPerformanceCycle> q, string? t, string? s, bool? c, bool? l, bool? a, string? search)
        {
            if (!string.IsNullOrWhiteSpace(t)) q = q.Where(x => x.CycleType == t);
            if (!string.IsNullOrWhiteSpace(s)) q = q.Where(x => x.CycleStatus == s);
            if (c.HasValue) q = q.Where(x => x.IsCurrent == c);
            if (l.HasValue) q = q.Where(x => x.IsLocked == l);
            if (a.HasValue) q = q.Where(x => x.IsActive == a);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var k = search.Trim().ToLower();
                q = q.Where(x => x.CycleCode.ToLower().Contains(k) || x.CycleName.ToLower().Contains(k) || (x.Description != null && x.Description.ToLower().Contains(k)));
            }
            return q;
        }
        static IOrderedQueryable<MstPerformanceCycle> Sort(IQueryable<MstPerformanceCycle> q, string? b, string? d)
        {
            var z = string.Equals(d, "desc", StringComparison.OrdinalIgnoreCase);
            return(b ?? "").ToLowerInvariant() switch
            {
                "cyclename" => z? q.OrderByDescending(x => x.CycleName): q.OrderBy(x => x.CycleName), "cyclestatus" => z? q.OrderByDescending(x => x.CycleStatus): q.OrderBy(x => x.CycleStatus),
                "createdatetime" => z? q.OrderByDescending(x => x.CreateDateTime): q.OrderBy(x => x.CreateDateTime), _ => z? q.OrderByDescending(x => x.PeriodStartDate): q.OrderBy(x => x.PeriodStartDate)
            };
        }
        PerformanceCycleResponse Map(MstPerformanceCycle x) => new()
        {
            Id = x.Id, LegalEntityId = x.LegalEntityId, HospitalSiteId = x.HospitalSiteId, CycleCode = x.CycleCode, CycleName = x.CycleName,
            CycleType = x.CycleType, PeriodYear = x.PeriodYear, PeriodStartDate = x.PeriodStartDate, PeriodEndDate = x.PeriodEndDate,
            CycleStatus = x.CycleStatus, IsCurrent = x.IsCurrent, IsLocked = x.IsLocked, IsActive = x.IsActive, TemplateCount = x.PerformanceTemplates.Count(d => !d.IsDelete),
            Description = x.Description, CreateDateTime = x.CreateDateTime, CreateBy = N(x.CreateBy)
        };
        async Task<string ?> Validate(CreatePerformanceCycleRequest r, Guid? id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(r.CycleName)) return "Nama siklus wajib diisi.";
            if (!Types.Contains(r.CycleType, StringComparer.OrdinalIgnoreCase)) return "Cycle type tidak valid.";
            if (!Statuses.Contains(r.CycleStatus, StringComparer.OrdinalIgnoreCase)) return "Cycle status tidak valid.";
            if (r.PeriodEndDate.Date<r.PeriodStartDate.Date) return "Periode siklus tidak valid.";
            var n = r.CycleName.Trim().ToLower();
            if (await _db.Set<MstPerformanceCycle> ().AnyAsync(x => !x.IsDelete && x.CycleName.ToLower() == n && (!id.HasValue || x.Id != id), ct)) return "Nama siklus sudah digunakan.";
            return null;
        }
        async Task ClearCurrent(Guid? except, DateTime now, CancellationToken ct)
        {
            var rows = await _db.Set<MstPerformanceCycle> ().Where(x => x.IsCurrent && !x.IsDelete && (!except.HasValue || x.Id != except)).ToListAsync(ct);
            foreach (var x in rows)
            {
                x.IsCurrent = false;
                x.UpdateDateTime = now;
                x.UpdateBy = Actor();
            }
        }
        async Task<string> Code(CancellationToken ct)
        {
            var c = await _db.Set<MstPerformanceCycle> ().Where(x => x.CycleCode.StartsWith(Prefix)).Select(x => x.CycleCode).ToListAsync(ct);
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
        static DateTime? D(DateTime? d) => d?.Date;
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
