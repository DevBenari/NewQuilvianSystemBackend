using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Controllers;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/shiftpatterns")]
    [Tags("Corporate / Human Resource / Master Data / ShiftPattern")]
    [AccessController(moduleCode:"HUMAN_RESOURCE_MASTER_DATA",moduleName:"Human Resource Master Data",displayName:"ShiftPattern",AreaName="Corporate",ControllerName="ShiftPattern",Description="Master ShiftPattern",SortOrder=20)]
    public class ShiftPatternController : ControllerBase
    {
        private const string CodePrefix = "SHP-RSMMC-";
        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        public ShiftPatternController(ApplicationDbContext dbContext, LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
    [AccessAction("Read","Read ShiftPattern",AccessType=AccessTypes.Read,SortOrder=1)]
    [AccessPermission("ShiftPattern","Read")]
        public IActionResult GetFilterMetadata()=>Ok(ApiResponse<ShiftPatternFilterMetadataResponse>.Ok(new()
        {
            DefaultFilter=new(),CustomPeriods=BuildPeriodOptions(),SortDirections=new()
            {
                "asc","desc"
            }
            ,PageSizeOptions=new()
            {
                10,25,50,100
            }
        }
        ,"Metadata berhasil diambil."));

        [HttpGet("summary")]
    [AccessAction("Read","Read ShiftPattern",AccessType=AccessTypes.Read,SortOrder=1)]
    [AccessPermission("ShiftPattern","Read")]
        public async Task<IActionResult> GetSummary(CancellationToken ct)
        {
            var q=BuildBaseQuery();
            return Ok(ApiResponse<ShiftPatternSummaryResponse>.Ok(new()
            {
                TotalData=await q.CountAsync(ct),ActiveData=await q.CountAsync(x=>x.IsActive,ct),InactiveData=await q.CountAsync(x=>!x.IsActive,ct),DefaultData=await q.CountAsync(x=>x.IsDefault,ct)
            }
            ,"Ringkasan berhasil diambil."));
        }

        [HttpGet]
    [AccessAction("Read","Read ShiftPattern",AccessType=AccessTypes.Read,SortOrder=1)]
    [AccessPermission("ShiftPattern","Read")]
        public async Task<IActionResult> GetShiftPatterns([FromQuery]DateTime? startDate,[FromQuery]DateTime? endDate,[FromQuery]string? customPeriod,[FromQuery]bool? isActive,[FromQuery]string? search,[FromQuery]int pageNumber=1,[FromQuery]int pageSize=25,CancellationToken ct=default)
        {
            pageNumber=Math.Max(1,pageNumber);
            pageSize=Math.Min(100,Math.Max(1,pageSize));
            var q=BuildBaseQuery();
            if(isActive.HasValue)q=q.Where(x=>x.IsActive==isActive);
            if(!string.IsNullOrWhiteSpace(search))
            {
                var k=search.Trim().ToLower();
                q=q.Where(x=>x.ShiftPatternCode.ToLower().Contains(k)||x.ShiftPatternName.ToLower().Contains(k));
            }
            q=WorkflowMasterDataSupport.ApplyDateFilter(q,startDate,endDate,customPeriod);
            var total=await q.CountAsync(ct);
            var rows=await q.OrderBy(x=>x.ShiftPatternName).Skip((pageNumber-1)*pageSize).Take(pageSize).ToListAsync(ct);
            var ids=rows.Select(x=>x.CreateBy).Where(x=>x!=Guid.Empty).Distinct().ToList();
            var users=await _dbContext.Users.Where(x=>ids.Contains(x.Id)).ToDictionaryAsync(x=>x.Id,x=>x.DisplayName??x.UserName??x.Email??x.UserCode,ct);
            var items=rows.Select(x=>Map(x,users)).ToList();
            return Ok(ApiResponse<PagedResult<ShiftPatternResponse>>.Ok(new()
            {
                PageNumber=pageNumber,PageSize=pageSize,TotalData=total,TotalPage=(int)Math.Ceiling(total/(double)pageSize),Items=items
            }
            ,"Data berhasil diambil."));
        }

        [HttpGet("options")]
    [AccessAction("Read","Read ShiftPattern",AccessType=AccessTypes.Read,SortOrder=1)]
    [AccessPermission("ShiftPattern","Read")]
        public async Task<IActionResult> GetShiftPatternOptions([FromQuery]string? search,[FromQuery]int pageNumber=1,[FromQuery]int pageSize=25,CancellationToken ct=default)
        {
            pageNumber=Math.Max(1,pageNumber);
            pageSize=Math.Min(100,Math.Max(1,pageSize));
            var q=BuildBaseQuery().Where(x=>x.IsActive);
            if(!string.IsNullOrWhiteSpace(search))
            {
                var k=search.Trim().ToLower();
                q=q.Where(x=>x.ShiftPatternCode.ToLower().Contains(k)||x.ShiftPatternName.ToLower().Contains(k));
            }
            var total=await q.CountAsync(ct);
            var items=await q.OrderBy(x=>x.ShiftPatternName).Skip((pageNumber-1)*pageSize).Take(pageSize).Select(x=>new ShiftPatternOptionResponse
            {
                Id=x.Id,Code=x.ShiftPatternCode,Name=x.ShiftPatternName
            }
            ).ToListAsync(ct);
            return Ok(ApiResponse<ShiftPatternOptionPagedResponse>.Ok(new()
            {
                PageNumber=pageNumber,PageSize=pageSize,TotalData=total,TotalPage=(int)Math.Ceiling(total/(double)pageSize),Items=items
            }
            ,"Pilihan berhasil diambil."));
        }

        [HttpGet("{id:guid}")] public async Task<IActionResult> GetShiftPatternById(Guid id,CancellationToken ct)
        {
            var x=await BuildBaseQuery().FirstOrDefaultAsync(x=>x.Id==id,ct);
            if(x==null)return NotFound(ApiResponse<object>.Fail(404,"Data tidak ditemukan."));
            var m=Map(x,new Dictionary<Guid,string>());
            var d=new ShiftPatternDetailResponse();
            foreach(var prop in typeof(ShiftPatternResponse).GetProperties())prop.SetValue(d,prop.GetValue(m));
            d.UpdateDateTime=x.UpdateDateTime;
            d.UpdateBy=x.UpdateBy==Guid.Empty?null:x.UpdateBy;
            return Ok(ApiResponse<ShiftPatternDetailResponse>.Ok(d,"Detail berhasil diambil."));
        }

        [HttpPost]
    [AccessAction("Create","Create ShiftPattern",AccessType=AccessTypes.Create,SortOrder=2)]
    [AccessPermission("ShiftPattern","Create")]
        public async Task<IActionResult> CreateShiftPattern([FromBody]CreateShiftPatternRequest request,CancellationToken ct)
        {
            var err=await Validate(request,null,ct);
            if(err!=null)return BadRequest(ApiResponse<object>.Fail(400,err));
            var now=DateTime.UtcNow;
            var e=new MstShiftPattern
            {
                Id=Guid.NewGuid(),ShiftGroupId=request.ShiftGroupId, ShiftPatternCode=await GenerateCodeAsync(ct), ShiftPatternName=request.ShiftPatternName.Trim(), CycleLengthDays=request.CycleLengthDays, PatternDefinitionJson=request.PatternDefinitionJson.Trim(), Description=N(request.Description), IsDefault=request.IsDefault,IsActive=true,CreateDateTime=now,CreateBy=Actor(),IsDelete=false,IsCancel=false
            }
            ;
            _dbContext.MstShiftPatterns.Add(e);
            await _dbContext.SaveChangesAsync(ct);
            return await GetShiftPatternById(e.Id,ct);
        }

        [HttpPut("{id:guid}")] public async Task<IActionResult> UpdateShiftPattern(Guid id,[FromBody]UpdateShiftPatternRequest request,CancellationToken ct)
        {
            var e=await _dbContext.MstShiftPatterns.FirstOrDefaultAsync(x=>x.Id==id&&!x.IsDelete,ct);
            if(e==null)return NotFound(ApiResponse<object>.Fail(404,"Data tidak ditemukan."));
            var err=await Validate(request,id,ct);
            if(err!=null)return BadRequest(ApiResponse<object>.Fail(400,err));
            e.ShiftGroupId=request.ShiftGroupId;
            e.ShiftPatternName=request.ShiftPatternName.Trim();
            e.CycleLengthDays=request.CycleLengthDays;
            e.PatternDefinitionJson=request.PatternDefinitionJson.Trim();
            e.Description=N(request.Description);
            e.IsDefault=request.IsDefault;
            e.IsActive=request.IsActive;
            e.UpdateDateTime=DateTime.UtcNow;
            e.UpdateBy=Actor();
            await _dbContext.SaveChangesAsync(ct);
            return await GetShiftPatternById(id,ct);
        }

        [HttpPatch("{id:guid}/status")] public async Task<IActionResult> UpdateShiftPatternStatus(Guid id,[FromBody]UpdateShiftPatternStatusRequest request,CancellationToken ct)
        {
            var e=await _dbContext.MstShiftPatterns.FirstOrDefaultAsync(x=>x.Id==id&&!x.IsDelete,ct);
            if(e==null)return NotFound(ApiResponse<object>.Fail(404,"Data tidak ditemukan."));
            e.IsActive=request.IsActive;
            e.UpdateDateTime=DateTime.UtcNow;
            e.UpdateBy=Actor();
            await _dbContext.SaveChangesAsync(ct);
            return Ok(ApiResponse<object>.Ok(null,"Status berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")] public async Task<IActionResult> DeleteShiftPattern(Guid id,CancellationToken ct)
        {
            var e=await _dbContext.MstShiftPatterns.FirstOrDefaultAsync(x=>x.Id==id&&!x.IsDelete,ct);
            if(e==null)return NotFound(ApiResponse<object>.Fail(404,"Data tidak ditemukan."));
            if(await _dbContext.WfpWorkScheduleAssignments.AnyAsync(x=>x.ShiftPatternId==id&&!x.IsDelete,ct))return BadRequest(ApiResponse<object>.Fail(400,"Data tidak dapat dihapus karena sudah digunakan."));
            var now=DateTime.UtcNow;
            e.IsDelete=true;
            e.IsActive=false;
            e.DeleteDateTime=now;
            e.DeleteBy=Actor();
            e.UpdateDateTime=now;
            e.UpdateBy=Actor();
            await _dbContext.SaveChangesAsync(ct);
            return Ok(ApiResponse<object>.Ok(null,"Data berhasil dihapus."));
        }
        IQueryable<MstShiftPattern> BuildBaseQuery()=>_dbContext.MstShiftPatterns.AsNoTracking().Include(x=>x.ShiftGroup).Where(x=>!x.IsDelete);
        ShiftPatternResponse Map(MstShiftPattern x,IReadOnlyDictionary<Guid,string> u)=>new()
        {
            Id=x.Id,ShiftGroupId=x.ShiftGroupId, ShiftGroupCode=x.ShiftGroup!=null?x.ShiftGroup.ShiftGroupCode:string.Empty, ShiftGroupName=x.ShiftGroup!=null?x.ShiftGroup.ShiftGroupName:string.Empty, ShiftPatternCode=x.ShiftPatternCode, ShiftPatternName=x.ShiftPatternName, CycleLengthDays=x.CycleLengthDays, PatternDefinitionJson=x.PatternDefinitionJson, Description=x.Description, IsDefault=x.IsDefault,IsActive=x.IsActive,CreateDateTime=x.CreateDateTime,CreateBy=x.CreateBy==Guid.Empty?null:x.CreateBy,CreateByName=u.GetValueOrDefault(x.CreateBy)
        }
        ;
        async Task<string?> Validate(CreateShiftPatternRequest request,Guid? exclude,CancellationToken ct)
        {
            if(request.ShiftGroupId==Guid.Empty) return "Shift group wajib dipilih.";
            if(!await _dbContext.MstShiftGroups.AnyAsync(x=>x.Id==request.ShiftGroupId&&x.IsActive&&!x.IsDelete,ct)) return "Shift group tidak ditemukan atau tidak aktif.";
            if(string.IsNullOrWhiteSpace(request.ShiftPatternName)) return "Nama shift pattern wajib diisi.";
            if(request.CycleLengthDays<1) return "Cycle length minimal 1 hari.";
            try
            {
                System.Text.Json.JsonDocument.Parse(request.PatternDefinitionJson);
            }
            catch
            {
                return "Pattern definition JSON tidak valid.";
            }
            var n=request.ShiftPatternName.Trim().ToLower();
            if(await _dbContext.MstShiftPatterns.AnyAsync(x=>!x.IsDelete&&x.ShiftPatternName.ToLower()==n&&(!exclude.HasValue||x.Id!=exclude),ct))return "Nama sudah digunakan.";
            return null;
        }
        async Task<string> GenerateCodeAsync(CancellationToken ct)
        {
            var codes=await _dbContext.MstShiftPatterns.Where(x=>!x.IsDelete&&x.ShiftPatternCode.StartsWith(CodePrefix)).Select(x=>x.ShiftPatternCode).ToListAsync(ct);
            var used=codes.Select(x=>x.Replace(CodePrefix,"")).Where(x=>int.TryParse(x,out _)).Select(int.Parse).ToHashSet();
            var n=1;
            while(used.Contains(n))n++;
            return CodePrefix+n.ToString("D5");
        }
        Guid Actor()
        {
            var v=User.FindFirstValue(ClaimTypes.NameIdentifier)??User.FindFirstValue("user_id");
            return Guid.TryParse(v,out var id)?id:Guid.Empty;
        }
        static string? N(string? v)=>string.IsNullOrWhiteSpace(v)?null:v.Trim();
        static Guid? NG(Guid? v)=>!v.HasValue||v==Guid.Empty?null:v;

        private static List<ShiftPatternCustomPeriodOptionResponse> BuildPeriodOptions()
        {
            return new List<ShiftPatternCustomPeriodOptionResponse>
            {
                new() { Value = "today", Label = "Hari ini" },
                new() { Value = "last7days", Label = "7 hari terakhir" },
                new() { Value = "thismonth", Label = "Bulan ini" },
                new() { Value = "lastmonth", Label = "Bulan lalu" }
            };
        }
    }
}
