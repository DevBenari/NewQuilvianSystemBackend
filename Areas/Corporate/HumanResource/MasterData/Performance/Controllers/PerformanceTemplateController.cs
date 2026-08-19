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
    [Route("api/v1/corporate/human-resource/master-data/performance-templates")]
    [Tags("Corporate / Human Resource / Master Data / Performance Template")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Performance Template",
        AreaName = "Corporate",
        ControllerName = "PerformanceTemplate",
        Description = "Master performance template",
        SortOrder = 33
    )]
    public class PerformanceTemplateController : ControllerBase
    {
        private const string Prefix = "PFT-RSMMC-";
        private readonly ApplicationDbContext _db;
        private readonly LoggerService _log;
        private static readonly string[]
        Types =
        {
            "EmployeePerformance", "Probation", "Leadership", "Clinical", "Project", "Custom"
        };
        public PerformanceTemplateController(ApplicationDbContext db, LoggerService log)
        {
            _db = db;
            _log = log;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Performance Template", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PerformanceTemplate", "Read")]
        public IActionResult Metadata() => Ok(ApiResponse<PerformanceTemplateFilterMetadataResponse>.Ok(new()
        {
            DefaultFilter = new(), CustomPeriods = BuildPeriodOptions(),
            TemplateTypeOptions = Types.Select(x => new PerformanceStringOptionResponse
            {
                Value = x,
                Label = x
            }
            ).ToList(),
            SortOptions = new()
            {
                new()
                {
                    Value = "templateName", Label = "Nama template"
                }, new()
                {
                    Value = "totalWeight", Label = "Total bobot"
                }, new()
                {
                    Value = "effectiveStartDate", Label = "Tanggal berlaku"
                }, new()
                {
                    Value = "isDefault", Label = "Default"
                }, new()
                {
                    Value = "createDateTime", Label = "Tanggal dibuat"
                }
            },
            SortDirections = new()
            {
                "asc", "desc"
            },
            PageSizeOptions = new()
            {
                10, 25, 50, 100
            }
        }, "Metadata berhasil diambil."));

        [HttpGet("summary")]
        [AccessAction("Read", "Read Performance Template", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PerformanceTemplate", "Read")]
        public async Task<IActionResult> Summary(CancellationToken ct)
        {
            var q = Q();
            return Ok(ApiResponse<PerformanceTemplateSummaryResponse>.Ok(new()
            {
                TotalData = await q.CountAsync(ct),
                ActiveData = await q.CountAsync(x => x.IsActive, ct),
                DefaultData = await q.CountAsync(x => x.IsDefault, ct),
                SelfAssessmentRequiredData = await q.CountAsync(x => x.IsSelfAssessmentRequired, ct),
                CalibrationRequiredData = await q.CountAsync(x => x.IsCalibrationRequired, ct)
            }, "Ringkasan berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read Performance Template", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PerformanceTemplate", "Read")]
        public async Task<IActionResult> List([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string? customPeriod, [FromQuery] Guid? performanceCycleId, [FromQuery] Guid? ratingScaleId, [FromQuery] string? templateType, [FromQuery] bool? isDefault, [FromQuery] bool? isActive, [FromQuery] string? search, [FromQuery] string? sortBy = "templateName", [FromQuery] string? sortDirection = "asc", [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 25, CancellationToken ct = default)
        {
            Norm(ref pageNumber, ref pageSize);
            var q = Filter(Q(), performanceCycleId, ratingScaleId, templateType, isDefault, isActive, search);
            q = WorkflowMasterDataSupport.ApplyDateFilter(q, startDate, endDate, customPeriod);
            var total = await q.CountAsync(ct);
            var rows = await Sort(q, sortBy, sortDirection).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);
            return Ok(ApiResponse<PagedResult<PerformanceTemplateResponse>>.Ok(new()
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = total,
                TotalPage = (int)Math.Ceiling(total / (double)pageSize),
                Items = rows.Select(Map).ToList()
            }, "Data berhasil diambil."));
        }

        [HttpGet("options")]
        [AccessAction("Read", "Read Performance Template", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PerformanceTemplate", "Read")]
        public async Task<IActionResult> Options([FromQuery] Guid? performanceCycleId, [FromQuery] string? templateType, [FromQuery] string? search, [FromQuery] bool onlyActive = true, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 25, CancellationToken ct = default)
        {
            Norm(ref pageNumber, ref pageSize);
            var q = Filter(Q(), performanceCycleId, null, templateType, null, onlyActive ? true : null, search);
            var total = await q.CountAsync(ct);
            var items = await q.OrderBy(x => x.TemplateName).Skip((pageNumber - 1) * pageSize).Take(pageSize).Select(x => new PerformanceTemplateOptionResponse
            {
                Id = x.Id,
                Code = x.TemplateCode,
                Name = x.TemplateName,
                TemplateType = x.TemplateType,
                RatingScaleId = x.RatingScaleId,
                TotalWeight = x.TotalWeight,
                MinimumPassingScore = x.MinimumPassingScore
            }
            ).ToListAsync(ct);
            return Ok(ApiResponse<PerformanceTemplateOptionPagedResponse>.Ok(new()
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = total,
                TotalPage = (int)Math.Ceiling(total / (double)pageSize),
                Items = items
            }, "Pilihan berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Performance Template", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PerformanceTemplate", "Read")]
        public async Task<IActionResult> Detail(Guid id, CancellationToken ct)
        {
            var x = await Q().FirstOrDefaultAsync(x => x.Id == id, ct);
            if (x == null) return NotFound(ApiResponse<object>.Fail(404, "Performance template tidak ditemukan."));
            var r = new PerformanceTemplateByIdResponse
            {
                Id = x.Id,
                PerformanceCycleId = x.PerformanceCycleId,
                RatingScaleId = x.RatingScaleId,
                CycleName = x.PerformanceCycle != null ? x.PerformanceCycle.CycleName : null,
                RatingScaleName = x.RatingScale != null ? x.RatingScale.ScaleName : null,
                TemplateCode = x.TemplateCode,
                TemplateName = x.TemplateName,
                TemplateType = x.TemplateType,
                TotalWeight = x.TotalWeight,
                MinimumPassingScore = x.MinimumPassingScore,
                IsSelfAssessmentRequired = x.IsSelfAssessmentRequired,
                IsManagerAssessmentRequired = x.IsManagerAssessmentRequired,
                IsCalibrationRequired = x.IsCalibrationRequired,
                EffectiveStartDate = x.EffectiveStartDate,
                EffectiveEndDate = x.EffectiveEndDate,
                IsDefault = x.IsDefault,
                IsActive = x.IsActive,
                DetailCount = x.Details.Count(d => !d.IsDelete),
                Description = x.Description,
                CreateDateTime = x.CreateDateTime,
                CreateBy = N(x.CreateBy),
                LegalEntityId = x.LegalEntityId,
                HospitalSiteId = x.HospitalSiteId,
                OrganizationUnitId = x.OrganizationUnitId,
                DepartmentId = x.DepartmentId,
                PositionId = x.PositionId,
                EmployeeCategoryId = x.EmployeeCategoryId,
                EmploymentTypeId = x.EmploymentTypeId,
                ProfessionId = x.ProfessionId,
                IsPeerAssessmentAllowed = x.IsPeerAssessmentAllowed,
                IsSubordinateAssessmentAllowed = x.IsSubordinateAssessmentAllowed,
                EmployeeInstructions = x.EmployeeInstructions,
                ReviewerInstructions = x.ReviewerInstructions,
                UpdateDateTime = x.UpdateDateTime,
                UpdateBy = N(x.UpdateBy)
            };
            return Ok(ApiResponse<PerformanceTemplateByIdResponse>.Ok(r, "Detail berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Create", "Create Performance Template", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("PerformanceTemplate", "Create")]
        public async Task<IActionResult> Create([FromBody] CreatePerformanceTemplateRequest r, CancellationToken ct)
        {
            var e = await Validate(r, null, ct);
            if (e != null) return BadRequest(ApiResponse<object>.Fail(400, e));
            var now = DateTime.UtcNow;
            if (r.IsDefault) await ClearDefault(null, r.TemplateType, now, ct);
            var x = new MstPerformanceTemplate
            {
                Id = Guid.NewGuid(),
                PerformanceCycleId = NG(r.PerformanceCycleId),
                RatingScaleId = r.RatingScaleId,
                LegalEntityId = NG(r.LegalEntityId),
                HospitalSiteId = NG(r.HospitalSiteId),
                OrganizationUnitId = NG(r.OrganizationUnitId),
                DepartmentId = NG(r.DepartmentId),
                PositionId = NG(r.PositionId),
                EmployeeCategoryId = NG(r.EmployeeCategoryId),
                EmploymentTypeId = NG(r.EmploymentTypeId),
                ProfessionId = NG(r.ProfessionId),
                TemplateCode = await Code(ct),
                TemplateName = r.TemplateName.Trim(),
                TemplateType = Canon(r.TemplateType),
                TotalWeight = r.TotalWeight,
                MinimumPassingScore = r.MinimumPassingScore,
                IsSelfAssessmentRequired = r.IsSelfAssessmentRequired,
                IsManagerAssessmentRequired = r.IsManagerAssessmentRequired,
                IsPeerAssessmentAllowed = r.IsPeerAssessmentAllowed,
                IsSubordinateAssessmentAllowed = r.IsSubordinateAssessmentAllowed,
                IsCalibrationRequired = r.IsCalibrationRequired,
                EmployeeInstructions = T(r.EmployeeInstructions),
                ReviewerInstructions = T(r.ReviewerInstructions),
                EffectiveStartDate = D(r.EffectiveStartDate),
                EffectiveEndDate = D(r.EffectiveEndDate),
                IsDefault = r.IsDefault,
                Description = T(r.Description),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = Actor(),
                IsDelete = false,
                IsCancel = false
            };
            _db.Set<MstPerformanceTemplate>().Add(x);
            await _db.SaveChangesAsync(ct);
            return await Detail(x.Id, ct);
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Performance Template", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("PerformanceTemplate", "Update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePerformanceTemplateRequest r, CancellationToken ct)
        {
            var x = await _db.Set<MstPerformanceTemplate>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);
            if (x == null) return NotFound(ApiResponse<object>.Fail(404, "Performance template tidak ditemukan."));
            var e = await Validate(r, id, ct);
            if (e != null) return BadRequest(ApiResponse<object>.Fail(400, e));
            var now = DateTime.UtcNow;
            if (r.IsDefault) await ClearDefault(id, r.TemplateType, now, ct);
            x.PerformanceCycleId = NG(r.PerformanceCycleId);
            x.RatingScaleId = r.RatingScaleId;
            x.LegalEntityId = NG(r.LegalEntityId);
            x.HospitalSiteId = NG(r.HospitalSiteId);
            x.OrganizationUnitId = NG(r.OrganizationUnitId);
            x.DepartmentId = NG(r.DepartmentId);
            x.PositionId = NG(r.PositionId);
            x.EmployeeCategoryId = NG(r.EmployeeCategoryId);
            x.EmploymentTypeId = NG(r.EmploymentTypeId);
            x.ProfessionId = NG(r.ProfessionId);
            x.TemplateName = r.TemplateName.Trim();
            x.TemplateType = Canon(r.TemplateType);
            x.TotalWeight = r.TotalWeight;
            x.MinimumPassingScore = r.MinimumPassingScore;
            x.IsSelfAssessmentRequired = r.IsSelfAssessmentRequired;
            x.IsManagerAssessmentRequired = r.IsManagerAssessmentRequired;
            x.IsPeerAssessmentAllowed = r.IsPeerAssessmentAllowed;
            x.IsSubordinateAssessmentAllowed = r.IsSubordinateAssessmentAllowed;
            x.IsCalibrationRequired = r.IsCalibrationRequired;
            x.EmployeeInstructions = T(r.EmployeeInstructions);
            x.ReviewerInstructions = T(r.ReviewerInstructions);
            x.EffectiveStartDate = D(r.EffectiveStartDate);
            x.EffectiveEndDate = D(r.EffectiveEndDate);
            x.IsDefault = r.IsDefault;
            x.Description = T(r.Description);
            x.IsActive = r.IsActive;
            x.UpdateDateTime = now;
            x.UpdateBy = Actor();
            await _db.SaveChangesAsync(ct);
            return await Detail(id, ct);
        }

        [HttpPatch("{id:guid}/status")]
        [AccessAction("Update", "Update Performance Template Status", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("PerformanceTemplate", "Update")]
        public async Task<IActionResult> Status(Guid id, [FromBody] UpdatePerformanceTemplateStatusRequest r, CancellationToken ct)
        {
            var x = await _db.Set<MstPerformanceTemplate>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);
            if (x == null) return NotFound(ApiResponse<object>.Fail(404, "Performance template tidak ditemukan."));
            var now = DateTime.UtcNow;
            if (r.IsDefault == true) await ClearDefault(id, x.TemplateType, now, ct);
            x.IsActive = r.IsActive;
            if (r.IsDefault.HasValue) x.IsDefault = r.IsDefault.Value && r.IsActive;
            x.UpdateDateTime = now;
            x.UpdateBy = Actor();
            await _db.SaveChangesAsync(ct);
            return Ok(ApiResponse<object>.Ok(null, "Status berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete Performance Template", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("PerformanceTemplate", "Delete")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var x = await _db.Set<MstPerformanceTemplate>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);
            if (x == null) return NotFound(ApiResponse<object>.Fail(404, "Performance template tidak ditemukan."));
            if (await _db.Set<MstPerformanceTemplateDetail>().AnyAsync(d => d.PerformanceTemplateId == id && !d.IsDelete, ct)) return BadRequest(ApiResponse<object>.Fail(400, "Template masih memiliki detail."));
            var now = DateTime.UtcNow;
            x.IsDelete = true;
            x.IsActive = false;
            x.IsDefault = false;
            x.DeleteDateTime = now;
            x.DeleteBy = Actor();
            x.UpdateDateTime = now;
            x.UpdateBy = Actor();
            await _db.SaveChangesAsync(ct);
            return Ok(ApiResponse<object>.Ok(null, "Performance template berhasil dihapus."));
        }
        IQueryable<MstPerformanceTemplate> Q() => _db.Set<MstPerformanceTemplate>().AsNoTracking().Include(x => x.PerformanceCycle).Include(x => x.RatingScale).Include(x => x.Details).Where(x => !x.IsDelete);
        static IQueryable<MstPerformanceTemplate> Filter(IQueryable<MstPerformanceTemplate> q, Guid? c, Guid? r, string? t, bool? d, bool? a, string? s)
        {
            if (c.HasValue && c != Guid.Empty) q = q.Where(x => x.PerformanceCycleId == c);
            if (r.HasValue && r != Guid.Empty) q = q.Where(x => x.RatingScaleId == r);
            if (!string.IsNullOrWhiteSpace(t)) q = q.Where(x => x.TemplateType == t);
            if (d.HasValue) q = q.Where(x => x.IsDefault == d);
            if (a.HasValue) q = q.Where(x => x.IsActive == a);
            if (!string.IsNullOrWhiteSpace(s))
            {
                var k = s.Trim().ToLower();
                q = q.Where(x => x.TemplateCode.ToLower().Contains(k) || x.TemplateName.ToLower().Contains(k) || (x.Description != null && x.Description.ToLower().Contains(k)));
            }
            return q;
        }
        static IOrderedQueryable<MstPerformanceTemplate> Sort(IQueryable<MstPerformanceTemplate> q, string? b, string? d)
        {
            var z = string.Equals(d, "desc", StringComparison.OrdinalIgnoreCase);
            return (b ?? "").ToLowerInvariant() switch
            {
                "totalweight" => z ? q.OrderByDescending(x => x.TotalWeight) : q.OrderBy(x => x.TotalWeight),
                "effectivestartdate" => z ? q.OrderByDescending(x => x.EffectiveStartDate) : q.OrderBy(x => x.EffectiveStartDate),
                "isdefault" => z ? q.OrderByDescending(x => x.IsDefault) : q.OrderBy(x => x.IsDefault),
                "createdatetime" => z ? q.OrderByDescending(x => x.CreateDateTime) : q.OrderBy(x => x.CreateDateTime),
                _ => z ? q.OrderByDescending(x => x.TemplateName) : q.OrderBy(x => x.TemplateName)
            };
        }
        PerformanceTemplateResponse Map(MstPerformanceTemplate x) => new()
        {
            Id = x.Id,
            PerformanceCycleId = x.PerformanceCycleId,
            RatingScaleId = x.RatingScaleId,
            CycleName = x.PerformanceCycle != null ? x.PerformanceCycle.CycleName : null,
            RatingScaleName = x.RatingScale != null ? x.RatingScale.ScaleName : null,
            TemplateCode = x.TemplateCode,
            TemplateName = x.TemplateName,
            TemplateType = x.TemplateType,
            TotalWeight = x.TotalWeight,
            MinimumPassingScore = x.MinimumPassingScore,
            IsSelfAssessmentRequired = x.IsSelfAssessmentRequired,
            IsManagerAssessmentRequired = x.IsManagerAssessmentRequired,
            IsCalibrationRequired = x.IsCalibrationRequired,
            EffectiveStartDate = x.EffectiveStartDate,
            EffectiveEndDate = x.EffectiveEndDate,
            IsDefault = x.IsDefault,
            IsActive = x.IsActive,
            DetailCount = x.Details.Count(d => !d.IsDelete),
            Description = x.Description,
            CreateDateTime = x.CreateDateTime,
            CreateBy = N(x.CreateBy)
        };
        async Task<string?> Validate(CreatePerformanceTemplateRequest r, Guid? id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(r.TemplateName)) return "Nama template wajib diisi.";
            if (!Types.Contains(r.TemplateType, StringComparer.OrdinalIgnoreCase)) return "Template type tidak valid.";
            if (r.RatingScaleId == Guid.Empty || !await _db.Set<MstPerformanceRatingScale>().AnyAsync(x => x.Id == r.RatingScaleId && x.IsActive && !x.IsDelete, ct)) return "Rating scale tidak ditemukan atau tidak aktif.";
            if (r.PerformanceCycleId.HasValue && !await _db.Set<MstPerformanceCycle>().AnyAsync(x => x.Id == r.PerformanceCycleId && x.IsActive && !x.IsDelete, ct)) return "Performance cycle tidak ditemukan atau tidak aktif.";
            if (r.TotalWeight < 0 || r.TotalWeight > 100) return "Total weight harus 0 sampai 100.";
            if (r.EffectiveEndDate.HasValue && r.EffectiveStartDate.HasValue && r.EffectiveEndDate.Value.Date < r.EffectiveStartDate.Value.Date) return "Periode efektif tidak valid.";
            var n = r.TemplateName.Trim().ToLower();
            if (await _db.Set<MstPerformanceTemplate>().AnyAsync(x => !x.IsDelete && x.TemplateName.ToLower() == n && x.TemplateType == r.TemplateType && (!id.HasValue || x.Id != id), ct)) return "Nama template sudah digunakan untuk tipe tersebut.";
            return null;
        }
        async Task ClearDefault(Guid? except, string type, DateTime now, CancellationToken ct)
        {
            var rows = await _db.Set<MstPerformanceTemplate>().Where(x => x.IsDefault && x.TemplateType == type && !x.IsDelete && (!except.HasValue || x.Id != except)).ToListAsync(ct);
            foreach (var x in rows)
            {
                x.IsDefault = false;
                x.UpdateDateTime = now;
                x.UpdateBy = Actor();
            }
        }
        async Task<string> Code(CancellationToken ct)
        {
            var c = await _db.Set<MstPerformanceTemplate>().Where(x => x.TemplateCode.StartsWith(Prefix)).Select(x => x.TemplateCode).ToListAsync(ct);
            return Next(c, Prefix);
        }
        string Canon(string v) => Types.First(x => x.Equals(v.Trim(), StringComparison.OrdinalIgnoreCase));
        Guid Actor()
        {
            var s = User.FindFirstValue("user_id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(s, out var g) ? g : Guid.Empty;
        }
        static string Next(IEnumerable<string> c, string p)
        {
            var u = c.Select(x => x.Replace(p, "")).Where(x => int.TryParse(x, out _)).Select(int.Parse).ToHashSet();
            var n = 1;
            while (u.Contains(n)) n++;
            return p + n.ToString("D5");
        }
        static Guid? NG(Guid? g) => !g.HasValue || g == Guid.Empty ? null : g;
        static Guid? N(Guid g) => g == Guid.Empty ? null : g;
        static string? T(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
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
