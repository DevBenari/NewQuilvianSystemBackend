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

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/workschedules")]
    [Tags("Corporate / Human Resource / Master Data / WorkSchedule")]
    [AccessController(moduleCode:"HUMAN_RESOURCE_MASTER_DATA",moduleName:"Human Resource Master Data",displayName:"WorkSchedule",AreaName="Corporate",ControllerName="WorkSchedule",Description="Master WorkSchedule",SortOrder=20)]
    public class WorkScheduleController : ControllerBase
    {
        private const string CodePrefix = "WKS-RSMMC-";
        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        public WorkScheduleController(ApplicationDbContext dbContext, LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
    [AccessAction("Read","Read WorkSchedule",AccessType=AccessTypes.Read,SortOrder=1)]
    [AccessPermission("WorkSchedule","Read")]
        public IActionResult GetFilterMetadata()=>Ok(ApiResponse<WorkScheduleFilterMetadataResponse>.Ok(new()
        {
            DefaultFilter=new(),SortDirections=new()
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
    [AccessAction("Read","Read WorkSchedule",AccessType=AccessTypes.Read,SortOrder=1)]
    [AccessPermission("WorkSchedule","Read")]
        public async Task<IActionResult> GetSummary(CancellationToken ct)
        {
            var q=BuildBaseQuery();
            return Ok(ApiResponse<WorkScheduleSummaryResponse>.Ok(new()
            {
                TotalData=await q.CountAsync(ct),ActiveData=await q.CountAsync(x=>x.IsActive,ct),InactiveData=await q.CountAsync(x=>!x.IsActive,ct),DefaultData=await q.CountAsync(x=>x.IsDefault,ct)
            }
            ,"Ringkasan berhasil diambil."));
        }

        [HttpGet]
    [AccessAction("Read","Read WorkSchedule",AccessType=AccessTypes.Read,SortOrder=1)]
    [AccessPermission("WorkSchedule","Read")]
        public async Task<IActionResult> GetWorkSchedules([FromQuery]bool? isActive,[FromQuery]string? search,[FromQuery]int pageNumber=1,[FromQuery]int pageSize=25,CancellationToken ct=default)
        {
            pageNumber=Math.Max(1,pageNumber);
            pageSize=Math.Min(100,Math.Max(1,pageSize));
            var q=BuildBaseQuery();
            if(isActive.HasValue)q=q.Where(x=>x.IsActive==isActive);
            if(!string.IsNullOrWhiteSpace(search))
            {
                var k=search.Trim().ToLower();
                q=q.Where(x=>x.ScheduleCode.ToLower().Contains(k)||x.ScheduleName.ToLower().Contains(k));
            }
            var total=await q.CountAsync(ct);
            var rows=await q.OrderBy(x=>x.ScheduleName).Skip((pageNumber-1)*pageSize).Take(pageSize).ToListAsync(ct);
            var ids=rows.Select(x=>x.CreateBy).Where(x=>x!=Guid.Empty).Distinct().ToList();
            var users=await _dbContext.Users.Where(x=>ids.Contains(x.Id)).ToDictionaryAsync(x=>x.Id,x=>x.DisplayName??x.UserName??x.Email??x.UserCode,ct);
            var items=rows.Select(x=>Map(x,users)).ToList();
            return Ok(ApiResponse<PagedResult<WorkScheduleResponse>>.Ok(new()
            {
                PageNumber=pageNumber,PageSize=pageSize,TotalData=total,TotalPage=(int)Math.Ceiling(total/(double)pageSize),Items=items
            }
            ,"Data berhasil diambil."));
        }

        [HttpGet("options")]
    [AccessAction("Read","Read WorkSchedule",AccessType=AccessTypes.Read,SortOrder=1)]
    [AccessPermission("WorkSchedule","Read")]
        public async Task<IActionResult> GetWorkScheduleOptions([FromQuery]string? search,[FromQuery]int pageNumber=1,[FromQuery]int pageSize=25,CancellationToken ct=default)
        {
            pageNumber=Math.Max(1,pageNumber);
            pageSize=Math.Min(100,Math.Max(1,pageSize));
            var q=BuildBaseQuery().Where(x=>x.IsActive);
            if(!string.IsNullOrWhiteSpace(search))
            {
                var k=search.Trim().ToLower();
                q=q.Where(x=>x.ScheduleCode.ToLower().Contains(k)||x.ScheduleName.ToLower().Contains(k));
            }
            var total=await q.CountAsync(ct);
            var items=await q.OrderBy(x=>x.ScheduleName).Skip((pageNumber-1)*pageSize).Take(pageSize).Select(x=>new WorkScheduleOptionResponse
            {
                Id=x.Id,Code=x.ScheduleCode,Name=x.ScheduleName
            }
            ).ToListAsync(ct);
            return Ok(ApiResponse<WorkScheduleOptionPagedResponse>.Ok(new()
            {
                PageNumber=pageNumber,PageSize=pageSize,TotalData=total,TotalPage=(int)Math.Ceiling(total/(double)pageSize),Items=items
            }
            ,"Pilihan berhasil diambil."));
        }

        [HttpGet("{id:guid}")] public async Task<IActionResult> GetWorkScheduleById(Guid id,CancellationToken ct)
        {
            var x=await BuildBaseQuery().FirstOrDefaultAsync(x=>x.Id==id,ct);
            if(x==null)return NotFound(ApiResponse<object>.Fail(404,"Data tidak ditemukan."));
            var m=Map(x,new Dictionary<Guid,string>());
            var d=new WorkScheduleDetailResponse();
            foreach(var prop in typeof(WorkScheduleResponse).GetProperties())prop.SetValue(d,prop.GetValue(m));
            d.UpdateDateTime=x.UpdateDateTime;
            d.UpdateBy=x.UpdateBy==Guid.Empty?null:x.UpdateBy;
            return Ok(ApiResponse<WorkScheduleDetailResponse>.Ok(d,"Detail berhasil diambil."));
        }

        [HttpPost]
    [AccessAction("Create","Create WorkSchedule",AccessType=AccessTypes.Create,SortOrder=2)]
    [AccessPermission("WorkSchedule","Create")]
        public async Task<IActionResult> CreateWorkSchedule([FromBody]CreateWorkScheduleRequest request,CancellationToken ct)
        {
            var err=await Validate(request,null,ct);
            if(err!=null)return BadRequest(ApiResponse<object>.Fail(400,err));
            var now=DateTime.UtcNow;
            var e=new MstWorkSchedule
            {
                Id=Guid.NewGuid(),ScheduleCode = await GenerateCodeAsync(ct), ScheduleName=request.ScheduleName.Trim(), ScheduleType=request.ScheduleType.Trim(), WorkStartTime=request.WorkStartTime, WorkEndTime=request.WorkEndTime, IsOvernight=request.IsOvernight, CheckInToleranceMinutes=request.CheckInToleranceMinutes, CheckOutToleranceMinutes=request.CheckOutToleranceMinutes, IsDefault=request.IsDefault,IsActive=true,CreateDateTime=now,CreateBy=Actor(),IsDelete=false,IsCancel=false
            }
            ;
            _dbContext.MstWorkSchedules.Add(e);
            await _dbContext.SaveChangesAsync(ct);
            return await GetWorkScheduleById(e.Id,ct);
        }

        [HttpPut("{id:guid}")] public async Task<IActionResult> UpdateWorkSchedule(Guid id,[FromBody]UpdateWorkScheduleRequest request,CancellationToken ct)
        {
            var e=await _dbContext.MstWorkSchedules.FirstOrDefaultAsync(x=>x.Id==id&&!x.IsDelete,ct);
            if(e==null)return NotFound(ApiResponse<object>.Fail(404,"Data tidak ditemukan."));
            var err=await Validate(request,id,ct);
            if(err!=null)return BadRequest(ApiResponse<object>.Fail(400,err));
            e.ScheduleName=request.ScheduleName.Trim();
            e.ScheduleType=request.ScheduleType.Trim();
            e.WorkStartTime=request.WorkStartTime;
            e.WorkEndTime=request.WorkEndTime;
            e.IsOvernight=request.IsOvernight;
            e.CheckInToleranceMinutes=request.CheckInToleranceMinutes;
            e.CheckOutToleranceMinutes=request.CheckOutToleranceMinutes;
            e.IsDefault=request.IsDefault;
            e.IsActive=request.IsActive;
            e.UpdateDateTime=DateTime.UtcNow;
            e.UpdateBy=Actor();
            await _dbContext.SaveChangesAsync(ct);
            return await GetWorkScheduleById(id,ct);
        }

        [HttpPatch("{id:guid}/status")] public async Task<IActionResult> UpdateWorkScheduleStatus(Guid id,[FromBody]UpdateWorkScheduleStatusRequest request,CancellationToken ct)
        {
            var e=await _dbContext.MstWorkSchedules.FirstOrDefaultAsync(x=>x.Id==id&&!x.IsDelete,ct);
            if(e==null)return NotFound(ApiResponse<object>.Fail(404,"Data tidak ditemukan."));
            e.IsActive=request.IsActive;
            e.UpdateDateTime=DateTime.UtcNow;
            e.UpdateBy=Actor();
            await _dbContext.SaveChangesAsync(ct);
            return Ok(ApiResponse<object>.Ok(null,"Status berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")] public async Task<IActionResult> DeleteWorkSchedule(Guid id,CancellationToken ct)
        {
            var e=await _dbContext.MstWorkSchedules.FirstOrDefaultAsync(x=>x.Id==id&&!x.IsDelete,ct);
            if(e==null)return NotFound(ApiResponse<object>.Fail(404,"Data tidak ditemukan."));
            if(await _dbContext.WfpWorkScheduleAssignments.AnyAsync(x=>x.WorkScheduleId==id&&!x.IsDelete,ct) || await _dbContext.MstShifts.AnyAsync(x=>x.WorkScheduleId==id&&!x.IsDelete,ct))return BadRequest(ApiResponse<object>.Fail(400,"Data tidak dapat dihapus karena sudah digunakan."));
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
        IQueryable<MstWorkSchedule> BuildBaseQuery()=>_dbContext.MstWorkSchedules.AsNoTracking().Where(x=>!x.IsDelete);
        WorkScheduleResponse Map(MstWorkSchedule x,IReadOnlyDictionary<Guid,string> u)=>new()
        {
            Id=x.Id,ScheduleCode=x.ScheduleCode, ScheduleName=x.ScheduleName, ScheduleType=x.ScheduleType, WorkStartTime=x.WorkStartTime, WorkEndTime=x.WorkEndTime, IsOvernight=x.IsOvernight, CheckInToleranceMinutes=x.CheckInToleranceMinutes, CheckOutToleranceMinutes=x.CheckOutToleranceMinutes, IsDefault=x.IsDefault,IsActive=x.IsActive,CreateDateTime=x.CreateDateTime,CreateBy=x.CreateBy==Guid.Empty?null:x.CreateBy,CreateByName=u.GetValueOrDefault(x.CreateBy)
        }
        ;
        async Task<string?> Validate(CreateWorkScheduleRequest request,Guid? exclude,CancellationToken ct)
        {
            if(string.IsNullOrWhiteSpace(request.ScheduleName)) return "Nama work schedule wajib diisi.";
            if(string.IsNullOrWhiteSpace(request.ScheduleType)) return "Schedule type wajib diisi.";
            if(request.CheckInToleranceMinutes<0||request.CheckOutToleranceMinutes<0) return "Tolerance menit tidak boleh negatif.";
            var n=request.ScheduleName.Trim().ToLower();
            if(await _dbContext.MstWorkSchedules.AnyAsync(x=>!x.IsDelete&&x.ScheduleName.ToLower()==n&&(!exclude.HasValue||x.Id!=exclude),ct))return "Nama sudah digunakan.";
            return null;
        }
        async Task<string> GenerateCodeAsync(CancellationToken ct)
        {
            var codes=await _dbContext.MstWorkSchedules.Where(x=>!x.IsDelete&&x.ScheduleCode.StartsWith(CodePrefix)).Select(x=>x.ScheduleCode).ToListAsync(ct);
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
    }
}
