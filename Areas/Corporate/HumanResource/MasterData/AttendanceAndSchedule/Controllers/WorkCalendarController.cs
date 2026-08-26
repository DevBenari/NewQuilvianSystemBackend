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
    [Route("api/v1/corporate/human-resource/master-data/workcalendars")]
    [Tags("Corporate / Human Resource / Master Data / WorkCalendar")]
    [AccessController(moduleCode:"HUMAN_RESOURCE_MASTER_DATA",moduleName:"Human Resource Master Data",displayName:"WorkCalendar",AreaName="Corporate",ControllerName="WorkCalendar",Description="Master WorkCalendar",SortOrder=20)]
    public class WorkCalendarController : ControllerBase
    {
        private const string CodePrefix = "WCL-RSMMC-";
        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        public WorkCalendarController(ApplicationDbContext dbContext, LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
    [AccessAction("Read","Read WorkCalendar",AccessType=AccessTypes.Read,SortOrder=1)]
    [AccessPermission("WorkCalendar","Read")]
        public IActionResult GetFilterMetadata()=>Ok(ApiResponse<WorkCalendarFilterMetadataResponse>.Ok(new()
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
    [AccessAction("Read","Read WorkCalendar",AccessType=AccessTypes.Read,SortOrder=1)]
    [AccessPermission("WorkCalendar","Read")]
        public async Task<IActionResult> GetSummary(CancellationToken ct)
        {
            var q=BuildBaseQuery();
            return Ok(ApiResponse<WorkCalendarSummaryResponse>.Ok(new()
            {
                TotalData=await q.CountAsync(ct),ActiveData=await q.CountAsync(x=>x.IsActive,ct),InactiveData=await q.CountAsync(x=>!x.IsActive,ct),DefaultData=await q.CountAsync(x=>x.IsDefault,ct)
            }
            ,"Ringkasan berhasil diambil."));
        }

        [HttpGet]
    [AccessAction("Read","Read WorkCalendar",AccessType=AccessTypes.Read,SortOrder=1)]
    [AccessPermission("WorkCalendar","Read")]
        public async Task<IActionResult> GetWorkCalendars([FromQuery]DateTime? startDate,[FromQuery]DateTime? endDate,[FromQuery]string? customPeriod,[FromQuery]bool? isActive,[FromQuery]string? search,[FromQuery]int pageNumber=1,[FromQuery]int pageSize=25,CancellationToken ct=default)
        {
            pageNumber=Math.Max(1,pageNumber);
            pageSize=Math.Min(100,Math.Max(1,pageSize));
            var q=BuildBaseQuery();
            if(isActive.HasValue)q=q.Where(x=>x.IsActive==isActive);
            if(!string.IsNullOrWhiteSpace(search))
            {
                var k=search.Trim().ToLower();
                q=q.Where(x=>x.WorkCalendarCode.ToLower().Contains(k)||x.WorkCalendarName.ToLower().Contains(k));
            }
            q=WorkflowMasterDataSupport.ApplyDateFilter(q,startDate,endDate,customPeriod);
            var total=await q.CountAsync(ct);
            var rows=await q.OrderBy(x=>x.WorkCalendarName).Skip((pageNumber-1)*pageSize).Take(pageSize).ToListAsync(ct);
            var ids=rows.Select(x=>x.CreateBy).Where(x=>x!=Guid.Empty).Distinct().ToList();
            var users=await _dbContext.Users.Where(x=>ids.Contains(x.Id)).ToDictionaryAsync(x=>x.Id,x=>x.DisplayName??x.UserName??x.Email??x.UserCode,ct);
            var items=rows.Select(x=>Map(x,users)).ToList();
            return Ok(ApiResponse<PagedResult<WorkCalendarResponse>>.Ok(new()
            {
                PageNumber=pageNumber,PageSize=pageSize,TotalData=total,TotalPage=(int)Math.Ceiling(total/(double)pageSize),Items=items
            }
            ,"Data berhasil diambil."));
        }

        [HttpGet("options")]
    [AccessAction("Read","Read WorkCalendar",AccessType=AccessTypes.Read,SortOrder=1)]
    [AccessPermission("WorkCalendar","Read")]
        public async Task<IActionResult> GetWorkCalendarOptions([FromQuery]string? search,[FromQuery]int pageNumber=1,[FromQuery]int pageSize=25,CancellationToken ct=default)
        {
            pageNumber=Math.Max(1,pageNumber);
            pageSize=Math.Min(100,Math.Max(1,pageSize));
            var q=BuildBaseQuery().Where(x=>x.IsActive);
            if(!string.IsNullOrWhiteSpace(search))
            {
                var k=search.Trim().ToLower();
                q=q.Where(x=>x.WorkCalendarCode.ToLower().Contains(k)||x.WorkCalendarName.ToLower().Contains(k));
            }
            var total=await q.CountAsync(ct);
            var items=await q.OrderBy(x=>x.WorkCalendarName).Skip((pageNumber-1)*pageSize).Take(pageSize).Select(x=>new WorkCalendarOptionResponse
            {
                Id=x.Id,Code=x.WorkCalendarCode,Name=x.WorkCalendarName
            }
            ).ToListAsync(ct);
            return Ok(ApiResponse<WorkCalendarOptionPagedResponse>.Ok(new()
            {
                PageNumber=pageNumber,PageSize=pageSize,TotalData=total,TotalPage=(int)Math.Ceiling(total/(double)pageSize),Items=items
            }
            ,"Pilihan berhasil diambil."));
        }

        [HttpGet("{id:guid}")] public async Task<IActionResult> GetWorkCalendarById(Guid id,CancellationToken ct)
        {
            var x=await BuildBaseQuery().FirstOrDefaultAsync(x=>x.Id==id,ct);
            if(x==null)return NotFound(ApiResponse<object>.Fail(404,"Data tidak ditemukan."));
            var m=Map(x,new Dictionary<Guid,string>());
            var d=new WorkCalendarDetailResponse();
            foreach(var prop in typeof(WorkCalendarResponse).GetProperties())prop.SetValue(d,prop.GetValue(m));
            d.UpdateDateTime=x.UpdateDateTime;
            d.UpdateBy=x.UpdateBy==Guid.Empty?null:x.UpdateBy;
            return Ok(ApiResponse<WorkCalendarDetailResponse>.Ok(d,"Detail berhasil diambil."));
        }

        [HttpPost]
    [AccessAction("Create","Create WorkCalendar",AccessType=AccessTypes.Create,SortOrder=2)]
    [AccessPermission("WorkCalendar","Create")]
        public async Task<IActionResult> CreateWorkCalendar([FromBody]CreateWorkCalendarRequest request,CancellationToken ct)
        {
            var err=await Validate(request,null,ct);
            if(err!=null)return BadRequest(ApiResponse<object>.Fail(400,err));
            var now=DateTime.UtcNow;
            var e=new MstWorkCalendar
            {
                Id=Guid.NewGuid(),HospitalSiteId=NG(request.HospitalSiteId), WorkCalendarCode=await GenerateCodeAsync(ct), WorkCalendarName=request.WorkCalendarName.Trim(), CalendarYear=request.CalendarYear, StartDate=request.StartDate.Date, EndDate=request.EndDate.Date, TimeZoneId=request.TimeZoneId.Trim(), Description=N(request.Description), IsDefault=request.IsDefault,IsActive=true,CreateDateTime=now,CreateBy=Actor(),IsDelete=false,IsCancel=false
            }
            ;
            _dbContext.MstWorkCalendars.Add(e);
            await _dbContext.SaveChangesAsync(ct);
            return await GetWorkCalendarById(e.Id,ct);
        }

        [HttpPut("{id:guid}")] public async Task<IActionResult> UpdateWorkCalendar(Guid id,[FromBody]UpdateWorkCalendarRequest request,CancellationToken ct)
        {
            var e=await _dbContext.MstWorkCalendars.FirstOrDefaultAsync(x=>x.Id==id&&!x.IsDelete,ct);
            if(e==null)return NotFound(ApiResponse<object>.Fail(404,"Data tidak ditemukan."));
            var err=await Validate(request,id,ct);
            if(err!=null)return BadRequest(ApiResponse<object>.Fail(400,err));
            e.HospitalSiteId=NG(request.HospitalSiteId);
            e.WorkCalendarName=request.WorkCalendarName.Trim();
            e.CalendarYear=request.CalendarYear;
            e.StartDate=request.StartDate.Date;
            e.EndDate=request.EndDate.Date;
            e.TimeZoneId=request.TimeZoneId.Trim();
            e.Description=N(request.Description);
            e.IsDefault=request.IsDefault;
            e.IsActive=request.IsActive;
            e.UpdateDateTime=DateTime.UtcNow;
            e.UpdateBy=Actor();
            await _dbContext.SaveChangesAsync(ct);
            return await GetWorkCalendarById(id,ct);
        }

        [HttpPatch("{id:guid}/status")] public async Task<IActionResult> UpdateWorkCalendarStatus(Guid id,[FromBody]UpdateWorkCalendarStatusRequest request,CancellationToken ct)
        {
            var e=await _dbContext.MstWorkCalendars.FirstOrDefaultAsync(x=>x.Id==id&&!x.IsDelete,ct);
            if(e==null)return NotFound(ApiResponse<object>.Fail(404,"Data tidak ditemukan."));
            e.IsActive=request.IsActive;
            e.UpdateDateTime=DateTime.UtcNow;
            e.UpdateBy=Actor();
            await _dbContext.SaveChangesAsync(ct);
            return Ok(ApiResponse<object>.Ok(null,"Status berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")] public async Task<IActionResult> DeleteWorkCalendar(Guid id,CancellationToken ct)
        {
            var e=await _dbContext.MstWorkCalendars.FirstOrDefaultAsync(x=>x.Id==id&&!x.IsDelete,ct);
            if(e==null)return NotFound(ApiResponse<object>.Fail(404,"Data tidak ditemukan."));
            if(await _dbContext.Set<MstHoliday>().AnyAsync(x=>x.WorkCalendarId==id&&!x.IsDelete,ct))return BadRequest(ApiResponse<object>.Fail(400,"Data tidak dapat dihapus karena sudah digunakan."));
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
        IQueryable<MstWorkCalendar> BuildBaseQuery()=>_dbContext.MstWorkCalendars.AsNoTracking().Include(x=>x.HospitalSite).Where(x=>!x.IsDelete);
        WorkCalendarResponse Map(MstWorkCalendar x,IReadOnlyDictionary<Guid,string> u)=>new()
        {
            Id=x.Id,HospitalSiteId=x.HospitalSiteId, HospitalSiteCode=x.HospitalSite!=null?x.HospitalSite.SiteCode:null, HospitalSiteName=x.HospitalSite!=null?x.HospitalSite.SiteName:null, WorkCalendarCode=x.WorkCalendarCode, WorkCalendarName=x.WorkCalendarName, CalendarYear=x.CalendarYear, StartDate=x.StartDate, EndDate=x.EndDate, TimeZoneId=x.TimeZoneId, Description=x.Description, IsDefault=x.IsDefault,IsActive=x.IsActive,CreateDateTime=x.CreateDateTime,CreateBy=x.CreateBy==Guid.Empty?null:x.CreateBy,CreateByName=u.GetValueOrDefault(x.CreateBy)
        }
        ;
        async Task<string?> Validate(CreateWorkCalendarRequest request,Guid? exclude,CancellationToken ct)
        {
            if(string.IsNullOrWhiteSpace(request.WorkCalendarName)) return "Nama work calendar wajib diisi.";
            if(request.StartDate==default||request.EndDate==default||request.EndDate.Date<request.StartDate.Date) return "Periode work calendar tidak valid.";
            if(request.CalendarYear<2000||request.CalendarYear>2200) return "Calendar year tidak valid.";
            if(request.HospitalSiteId.HasValue && !await _dbContext.MstHospitalSites.AnyAsync(x=>x.Id==request.HospitalSiteId.Value&&x.IsActive&&!x.IsDelete,ct)) return "Hospital site tidak ditemukan atau tidak aktif.";
            var n=request.WorkCalendarName.Trim().ToLower();
            if(await _dbContext.MstWorkCalendars.AnyAsync(x=>!x.IsDelete&&x.WorkCalendarName.ToLower()==n&&(!exclude.HasValue||x.Id!=exclude),ct))return "Nama sudah digunakan.";
            return null;
        }
        async Task<string> GenerateCodeAsync(CancellationToken ct)
        {
            var codes=await _dbContext.MstWorkCalendars.Where(x=>!x.IsDelete&&x.WorkCalendarCode.StartsWith(CodePrefix)).Select(x=>x.WorkCalendarCode).ToListAsync(ct);
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

        private static List<WorkCalendarCustomPeriodOptionResponse> BuildPeriodOptions()
        {
            return new List<WorkCalendarCustomPeriodOptionResponse>
            {
                new() { Value = "today", Label = "Hari ini" },
                new() { Value = "last7days", Label = "7 hari terakhir" },
                new() { Value = "thismonth", Label = "Bulan ini" },
                new() { Value = "lastmonth", Label = "Bulan lalu" }
            };
        }
    }
}
