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
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/shiftgroups")]
    [Tags("Corporate / Human Resource / Master Data / ShiftGroup")]
    [AccessController(moduleCode:"HUMAN_RESOURCE_MASTER_DATA",moduleName:"Human Resource Master Data",displayName:"ShiftGroup",AreaName="Corporate",ControllerName="ShiftGroup",Description="Master ShiftGroup",SortOrder=20)]
    public class ShiftGroupController : ControllerBase
    {
        private const string CodePrefix = "SHG-RSMMC-";
        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        public ShiftGroupController(ApplicationDbContext dbContext, LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
    [AccessAction("Read","Read ShiftGroup",AccessType=AccessTypes.Read,SortOrder=1)]
    [AccessPermission("ShiftGroup","Read")]
        public IActionResult GetFilterMetadata()=>Ok(ApiResponse<ShiftGroupFilterMetadataResponse>.Ok(new()
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
    [AccessAction("Read","Read ShiftGroup",AccessType=AccessTypes.Read,SortOrder=1)]
    [AccessPermission("ShiftGroup","Read")]
        public async Task<IActionResult> GetSummary(CancellationToken ct)
        {
            var q=BuildBaseQuery();
            return Ok(ApiResponse<ShiftGroupSummaryResponse>.Ok(new()
            {
                TotalData=await q.CountAsync(ct),ActiveData=await q.CountAsync(x=>x.IsActive,ct),InactiveData=await q.CountAsync(x=>!x.IsActive,ct),DefaultData=await q.CountAsync(x=>x.IsRotating,ct)
            }
            ,"Ringkasan berhasil diambil."));
        }

        [HttpGet]
    [AccessAction("Read","Read ShiftGroup",AccessType=AccessTypes.Read,SortOrder=1)]
    [AccessPermission("ShiftGroup","Read")]
        public async Task<IActionResult> GetShiftGroups([FromQuery]bool? isActive,[FromQuery]string? search,[FromQuery]int pageNumber=1,[FromQuery]int pageSize=25,CancellationToken ct=default)
        {
            pageNumber=Math.Max(1,pageNumber);
            pageSize=Math.Min(100,Math.Max(1,pageSize));
            var q=BuildBaseQuery();
            if(isActive.HasValue)q=q.Where(x=>x.IsActive==isActive);
            if(!string.IsNullOrWhiteSpace(search))
            {
                var k=search.Trim().ToLower();
                q=q.Where(x=>x.ShiftGroupCode.ToLower().Contains(k)||x.ShiftGroupName.ToLower().Contains(k));
            }
            var total=await q.CountAsync(ct);
            var rows=await q.OrderBy(x=>x.ShiftGroupName).Skip((pageNumber-1)*pageSize).Take(pageSize).ToListAsync(ct);
            var ids=rows.Select(x=>x.CreateBy).Where(x=>x!=Guid.Empty).Distinct().ToList();
            var users=await _dbContext.Users.Where(x=>ids.Contains(x.Id)).ToDictionaryAsync(x=>x.Id,x=>x.DisplayName??x.UserName??x.Email??x.UserCode,ct);
            var items=rows.Select(x=>Map(x,users)).ToList();
            return Ok(ApiResponse<PagedResult<ShiftGroupResponse>>.Ok(new()
            {
                PageNumber=pageNumber,PageSize=pageSize,TotalData=total,TotalPage=(int)Math.Ceiling(total/(double)pageSize),Items=items
            }
            ,"Data berhasil diambil."));
        }

        [HttpGet("options")]
    [AccessAction("Read","Read ShiftGroup",AccessType=AccessTypes.Read,SortOrder=1)]
    [AccessPermission("ShiftGroup","Read")]
        public async Task<IActionResult> GetShiftGroupOptions([FromQuery]string? search,[FromQuery]int pageNumber=1,[FromQuery]int pageSize=25,CancellationToken ct=default)
        {
            pageNumber=Math.Max(1,pageNumber);
            pageSize=Math.Min(100,Math.Max(1,pageSize));
            var q=BuildBaseQuery().Where(x=>x.IsActive);
            if(!string.IsNullOrWhiteSpace(search))
            {
                var k=search.Trim().ToLower();
                q=q.Where(x=>x.ShiftGroupCode.ToLower().Contains(k)||x.ShiftGroupName.ToLower().Contains(k));
            }
            var total=await q.CountAsync(ct);
            var items=await q.OrderBy(x=>x.ShiftGroupName).Skip((pageNumber-1)*pageSize).Take(pageSize).Select(x=>new ShiftGroupOptionResponse
            {
                Id=x.Id,Code=x.ShiftGroupCode,Name=x.ShiftGroupName
            }
            ).ToListAsync(ct);
            return Ok(ApiResponse<ShiftGroupOptionPagedResponse>.Ok(new()
            {
                PageNumber=pageNumber,PageSize=pageSize,TotalData=total,TotalPage=(int)Math.Ceiling(total/(double)pageSize),Items=items
            }
            ,"Pilihan berhasil diambil."));
        }

        [HttpGet("{id:guid}")] public async Task<IActionResult> GetShiftGroupById(Guid id,CancellationToken ct)
        {
            var x=await BuildBaseQuery().FirstOrDefaultAsync(x=>x.Id==id,ct);
            if(x==null)return NotFound(ApiResponse<object>.Fail(404,"Data tidak ditemukan."));
            var m=Map(x,new Dictionary<Guid,string>());
            var d=new ShiftGroupDetailResponse();
            foreach(var prop in typeof(ShiftGroupResponse).GetProperties())prop.SetValue(d,prop.GetValue(m));
            d.UpdateDateTime=x.UpdateDateTime;
            d.UpdateBy=x.UpdateBy==Guid.Empty?null:x.UpdateBy;
            return Ok(ApiResponse<ShiftGroupDetailResponse>.Ok(d,"Detail berhasil diambil."));
        }

        [HttpPost]
    [AccessAction("Create","Create ShiftGroup",AccessType=AccessTypes.Create,SortOrder=2)]
    [AccessPermission("ShiftGroup","Create")]
        public async Task<IActionResult> CreateShiftGroup([FromBody]CreateShiftGroupRequest request,CancellationToken ct)
        {
            var err=await Validate(request,null,ct);
            if(err!=null)return BadRequest(ApiResponse<object>.Fail(400,err));
            var now=DateTime.UtcNow;
            var e=new MstShiftGroup
            {
                Id=Guid.NewGuid(),ShiftGroupCode=await GenerateCodeAsync(ct), ShiftGroupName=request.ShiftGroupName.Trim(), Description=N(request.Description), IsRotating=request.IsRotating,IsActive=true,CreateDateTime=now,CreateBy=Actor(),IsDelete=false,IsCancel=false
            }
            ;
            _dbContext.MstShiftGroups.Add(e);
            await _dbContext.SaveChangesAsync(ct);
            return await GetShiftGroupById(e.Id,ct);
        }

        [HttpPut("{id:guid}")] public async Task<IActionResult> UpdateShiftGroup(Guid id,[FromBody]UpdateShiftGroupRequest request,CancellationToken ct)
        {
            var e=await _dbContext.MstShiftGroups.FirstOrDefaultAsync(x=>x.Id==id&&!x.IsDelete,ct);
            if(e==null)return NotFound(ApiResponse<object>.Fail(404,"Data tidak ditemukan."));
            var err=await Validate(request,id,ct);
            if(err!=null)return BadRequest(ApiResponse<object>.Fail(400,err));
            e.ShiftGroupName=request.ShiftGroupName.Trim();
            e.Description=N(request.Description);
            e.IsRotating=request.IsRotating;
            e.IsActive=request.IsActive;
            e.UpdateDateTime=DateTime.UtcNow;
            e.UpdateBy=Actor();
            await _dbContext.SaveChangesAsync(ct);
            return await GetShiftGroupById(id,ct);
        }

        [HttpPatch("{id:guid}/status")] public async Task<IActionResult> UpdateShiftGroupStatus(Guid id,[FromBody]UpdateShiftGroupStatusRequest request,CancellationToken ct)
        {
            var e=await _dbContext.MstShiftGroups.FirstOrDefaultAsync(x=>x.Id==id&&!x.IsDelete,ct);
            if(e==null)return NotFound(ApiResponse<object>.Fail(404,"Data tidak ditemukan."));
            e.IsActive=request.IsActive;
            e.UpdateDateTime=DateTime.UtcNow;
            e.UpdateBy=Actor();
            await _dbContext.SaveChangesAsync(ct);
            return Ok(ApiResponse<object>.Ok(null,"Status berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")] public async Task<IActionResult> DeleteShiftGroup(Guid id,CancellationToken ct)
        {
            var e=await _dbContext.MstShiftGroups.FirstOrDefaultAsync(x=>x.Id==id&&!x.IsDelete,ct);
            if(e==null)return NotFound(ApiResponse<object>.Fail(404,"Data tidak ditemukan."));
            if(await _dbContext.MstShifts.AnyAsync(x=>x.ShiftGroupId==id&&!x.IsDelete,ct) || await _dbContext.MstShiftPatterns.AnyAsync(x=>x.ShiftGroupId==id&&!x.IsDelete,ct) || await _dbContext.WfpWorkScheduleAssignments.AnyAsync(x=>x.ShiftGroupId==id&&!x.IsDelete,ct))return BadRequest(ApiResponse<object>.Fail(400,"Data tidak dapat dihapus karena sudah digunakan."));
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
        IQueryable<MstShiftGroup> BuildBaseQuery()=>_dbContext.MstShiftGroups.AsNoTracking().Where(x=>!x.IsDelete);
        ShiftGroupResponse Map(MstShiftGroup x,IReadOnlyDictionary<Guid,string> u)=>new()
        {
            Id=x.Id,ShiftGroupCode=x.ShiftGroupCode, ShiftGroupName=x.ShiftGroupName, Description=x.Description, IsRotating=x.IsRotating,IsActive=x.IsActive,CreateDateTime=x.CreateDateTime,CreateBy=x.CreateBy==Guid.Empty?null:x.CreateBy,CreateByName=u.GetValueOrDefault(x.CreateBy)
        }
        ;
        async Task<string?> Validate(CreateShiftGroupRequest request,Guid? exclude,CancellationToken ct)
        {
            if(string.IsNullOrWhiteSpace(request.ShiftGroupName)) return "Nama shift group wajib diisi.";
            var n=request.ShiftGroupName.Trim().ToLower();
            if(await _dbContext.MstShiftGroups.AnyAsync(x=>!x.IsDelete&&x.ShiftGroupName.ToLower()==n&&(!exclude.HasValue||x.Id!=exclude),ct))return "Nama sudah digunakan.";
            return null;
        }
        async Task<string> GenerateCodeAsync(CancellationToken ct)
        {
            var codes=await _dbContext.MstShiftGroups.Where(x=>!x.IsDelete&&x.ShiftGroupCode.StartsWith(CodePrefix)).Select(x=>x.ShiftGroupCode).ToListAsync(ct);
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
