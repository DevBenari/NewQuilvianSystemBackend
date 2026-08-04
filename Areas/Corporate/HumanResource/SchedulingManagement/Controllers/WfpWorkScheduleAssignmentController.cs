using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/work-schedule-assignments")]
    [Tags("Corporate / Human Resource / Scheduling Management / Work Schedule Assignment")]
    [AccessController(moduleCode:"HUMAN_RESOURCE_SCHEDULING",moduleName:"Human Resource Scheduling",displayName:"Work Schedule Assignment",AreaName="Corporate",ControllerName="WorkScheduleAssignment",Description="Workforce work schedule assignment",SortOrder=1)]
    public class WfpWorkScheduleAssignmentController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        public WfpWorkScheduleAssignmentController(ApplicationDbContext dbContext, LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")] public IActionResult GetFilterMetadata()=>Ok(ApiResponse<WfpWorkScheduleAssignmentFilterMetadataResponse>.Ok(new()
        {
            AssignmentTypeOptions=new()
            {
                "Primary","Temporary","Rotation","Project","OnCall"
            }
            ,WeekStartDayOptions=Enumerable.Range(0,7).ToList(),SortDirections=new()
            {
                "asc","desc"
            }
            ,PageSizeOptions=new()
            {
                10,25,50,100
            }
        }
        ,"Metadata berhasil diambil."));

        [HttpGet("summary")] public async Task<IActionResult> GetSummary(Guid workforceProfileId,CancellationToken ct)
        {
            if(!await Exists(workforceProfileId,ct))return NF();
            var q=_dbContext.WfpWorkScheduleAssignments.AsNoTracking().Where(x=>x.WorkforceProfileId==workforceProfileId&&!x.IsDelete);
            return Ok(ApiResponse<WfpWorkScheduleAssignmentSummaryResponse>.Ok(new()
            {
                TotalData=await q.CountAsync(ct),ActiveData=await q.CountAsync(x=>x.IsActive,ct),PrimaryData=await q.CountAsync(x=>x.IsPrimary,ct),RotatingData=await q.CountAsync(x=>x.IsRotating,ct),TemporaryData=await q.CountAsync(x=>x.IsTemporary,ct)
            }
            ,"Ringkasan berhasil diambil."));
        }

        [HttpGet] public async Task<IActionResult> GetWorkScheduleAssignments(Guid workforceProfileId,[FromQuery]bool? isActive,[FromQuery]int pageNumber=1,[FromQuery]int pageSize=25,CancellationToken ct=default)
        {
            if(!await Exists(workforceProfileId,ct))return NF();
            pageNumber=Math.Max(1,pageNumber);
            pageSize=Math.Min(100,Math.Max(1,pageSize));
            var q=BuildBaseQuery(workforceProfileId);
            if(isActive.HasValue)q=q.Where(x=>x.IsActive==isActive);
            var total=await q.CountAsync(ct);
            var rows=await q.OrderByDescending(x=>x.EffectiveStartDate).Skip((pageNumber-1)*pageSize).Take(pageSize).ToListAsync(ct);
            var items=rows.Select(Map).ToList();
            return Ok(ApiResponse<PagedResult<WfpWorkScheduleAssignmentResponse>>.Ok(new()
            {
                PageNumber=pageNumber,PageSize=pageSize,TotalData=total,TotalPage=(int)Math.Ceiling(total/(double)pageSize),Items=items
            }
            ,"Data berhasil diambil."));
        }

        [HttpGet("{id:guid}")] public async Task<IActionResult> GetWorkScheduleAssignmentById(Guid workforceProfileId,Guid id,CancellationToken ct)
        {
            var e=await BuildBaseQuery(workforceProfileId).FirstOrDefaultAsync(x=>x.Id==id,ct);
            if(e==null)return NotFound(ApiResponse<object>.Fail(404,"Assignment tidak ditemukan."));
            var b=Map(e);
            var d=new WfpWorkScheduleAssignmentDetailResponse();
            foreach(var pr in typeof(WfpWorkScheduleAssignmentResponse).GetProperties())pr.SetValue(d,pr.GetValue(b));
            d.UpdateDateTime=e.UpdateDateTime;
            d.UpdateBy=e.UpdateBy==Guid.Empty?null:e.UpdateBy;
            return Ok(ApiResponse<WfpWorkScheduleAssignmentDetailResponse>.Ok(d,"Detail berhasil diambil."));
        }

        [HttpPost] public async Task<IActionResult> CreateWorkScheduleAssignment(Guid workforceProfileId,[FromBody]CreateWfpWorkScheduleAssignmentRequest r,CancellationToken ct)
        {
            if(!await Exists(workforceProfileId,ct))return NF();
            var err=await Validate(workforceProfileId,r,null,ct);
            if(err!=null)return BadRequest(ApiResponse<object>.Fail(400,err));
            var now=DateTime.UtcNow;
            var e=new WfpWorkScheduleAssignment
            {
                Id=Guid.NewGuid(),WorkforceProfileId=workforceProfileId,OrganizationAssignmentId=NG(r.OrganizationAssignmentId),HospitalSiteId=NG(r.HospitalSiteId),OrganizationUnitId=NG(r.OrganizationUnitId),DepartmentId=NG(r.DepartmentId),PositionId=NG(r.PositionId),WorkLocationId=NG(r.WorkLocationId),WorkScheduleId=r.WorkScheduleId,ShiftGroupId=NG(r.ShiftGroupId),ShiftPatternId=NG(r.ShiftPatternId),RosterPolicyId=NG(r.RosterPolicyId),MinimumRestPolicyId=NG(r.MinimumRestPolicyId),AssignmentType=r.AssignmentType.Trim(),EffectiveStartDate=r.EffectiveStartDate,EffectiveEndDate=r.EffectiveEndDate,WeekStartDay=r.WeekStartDay,IsPrimary=r.IsPrimary,IsRotating=r.IsRotating,IsTemporary=r.IsTemporary,IsActive=r.IsActive,Notes=N(r.Notes),CreateDateTime=now,CreateBy=Actor(),IsDelete=false,IsCancel=false
            }
            ;
            _dbContext.WfpWorkScheduleAssignments.Add(e);
            await _dbContext.SaveChangesAsync(ct);
            return await GetWorkScheduleAssignmentById(workforceProfileId,e.Id,ct);
        }

        [HttpPut("{id:guid}")] public async Task<IActionResult> UpdateWorkScheduleAssignment(Guid workforceProfileId,Guid id,[FromBody]UpdateWfpWorkScheduleAssignmentRequest r,CancellationToken ct)
        {
            var e=await _dbContext.WfpWorkScheduleAssignments.FirstOrDefaultAsync(x=>x.Id==id&&x.WorkforceProfileId==workforceProfileId&&!x.IsDelete,ct);
            if(e==null)return NotFound(ApiResponse<object>.Fail(404,"Assignment tidak ditemukan."));
            var err=await Validate(workforceProfileId,r,id,ct);
            if(err!=null)return BadRequest(ApiResponse<object>.Fail(400,err));
            e.OrganizationAssignmentId=NG(r.OrganizationAssignmentId);
            e.HospitalSiteId=NG(r.HospitalSiteId);
            e.OrganizationUnitId=NG(r.OrganizationUnitId);
            e.DepartmentId=NG(r.DepartmentId);
            e.PositionId=NG(r.PositionId);
            e.WorkLocationId=NG(r.WorkLocationId);
            e.WorkScheduleId=r.WorkScheduleId;
            e.ShiftGroupId=NG(r.ShiftGroupId);
            e.ShiftPatternId=NG(r.ShiftPatternId);
            e.RosterPolicyId=NG(r.RosterPolicyId);
            e.MinimumRestPolicyId=NG(r.MinimumRestPolicyId);
            e.AssignmentType=r.AssignmentType.Trim();
            e.EffectiveStartDate=r.EffectiveStartDate;
            e.EffectiveEndDate=r.EffectiveEndDate;
            e.WeekStartDay=r.WeekStartDay;
            e.IsPrimary=r.IsPrimary;
            e.IsRotating=r.IsRotating;
            e.IsTemporary=r.IsTemporary;
            e.IsActive=r.IsActive;
            e.Notes=N(r.Notes);
            e.UpdateDateTime=DateTime.UtcNow;
            e.UpdateBy=Actor();
            await _dbContext.SaveChangesAsync(ct);
            return await GetWorkScheduleAssignmentById(workforceProfileId,id,ct);
        }

        [HttpPatch("{id:guid}/status")] public async Task<IActionResult> UpdateWorkScheduleAssignmentStatus(Guid workforceProfileId,Guid id,[FromBody]UpdateWfpWorkScheduleAssignmentStatusRequest r,CancellationToken ct)
        {
            var e=await _dbContext.WfpWorkScheduleAssignments.FirstOrDefaultAsync(x=>x.Id==id&&x.WorkforceProfileId==workforceProfileId&&!x.IsDelete,ct);
            if(e==null)return NotFound(ApiResponse<object>.Fail(404,"Assignment tidak ditemukan."));
            e.IsActive=r.IsActive;
            e.UpdateDateTime=DateTime.UtcNow;
            e.UpdateBy=Actor();
            await _dbContext.SaveChangesAsync(ct);
            return await GetWorkScheduleAssignmentById(workforceProfileId,id,ct);
        }

        [HttpDelete("{id:guid}")] public async Task<IActionResult> DeleteWorkScheduleAssignment(Guid workforceProfileId,Guid id,CancellationToken ct)
        {
            var e=await _dbContext.WfpWorkScheduleAssignments.FirstOrDefaultAsync(x=>x.Id==id&&x.WorkforceProfileId==workforceProfileId&&!x.IsDelete,ct);
            if(e==null)return NotFound(ApiResponse<object>.Fail(404,"Assignment tidak ditemukan."));
            var now=DateTime.UtcNow;
            e.IsDelete=true;
            e.IsActive=false;
            e.DeleteDateTime=now;
            e.DeleteBy=Actor();
            e.UpdateDateTime=now;
            e.UpdateBy=Actor();
            await _dbContext.SaveChangesAsync(ct);
            return Ok(ApiResponse<object>.Ok(null,"Assignment berhasil dihapus."));
        }
        IQueryable<WfpWorkScheduleAssignment> BuildBaseQuery(Guid id)=>_dbContext.WfpWorkScheduleAssignments.AsNoTracking().Include(x=>x.WorkforceProfile).Include(x=>x.HospitalSite).Include(x=>x.OrganizationUnit).Include(x=>x.Department).Include(x=>x.Position).Include(x=>x.WorkLocation).Include(x=>x.WorkSchedule).Include(x=>x.ShiftGroup).Include(x=>x.ShiftPattern).Where(x=>x.WorkforceProfileId==id&&!x.IsDelete);
        WfpWorkScheduleAssignmentResponse Map(WfpWorkScheduleAssignment x)=>new()
        {
            Id=x.Id,WorkforceProfileId=x.WorkforceProfileId,WorkforceProfileCode=x.WorkforceProfile?.ProfileCode??"",WorkforceDisplayName=x.WorkforceProfile?.DisplayName??"",OrganizationAssignmentId=x.OrganizationAssignmentId,HospitalSiteId=x.HospitalSiteId,HospitalSiteName=x.HospitalSite?.SiteName,OrganizationUnitId=x.OrganizationUnitId,OrganizationUnitName=x.OrganizationUnit?.UnitName,DepartmentId=x.DepartmentId,DepartmentName=x.Department?.DepartmentName,PositionId=x.PositionId,PositionName=x.Position?.PositionName,WorkLocationId=x.WorkLocationId,WorkLocationName=x.WorkLocation?.LocationName,WorkScheduleId=x.WorkScheduleId,WorkScheduleCode=x.WorkSchedule?.ScheduleCode??"",WorkScheduleName=x.WorkSchedule?.ScheduleName??"",ShiftGroupId=x.ShiftGroupId,ShiftGroupName=x.ShiftGroup?.ShiftGroupName,ShiftPatternId=x.ShiftPatternId,ShiftPatternName=x.ShiftPattern?.ShiftPatternName,RosterPolicyId=x.RosterPolicyId,MinimumRestPolicyId=x.MinimumRestPolicyId,AssignmentType=x.AssignmentType,EffectiveStartDate=x.EffectiveStartDate,EffectiveEndDate=x.EffectiveEndDate,WeekStartDay=x.WeekStartDay,IsPrimary=x.IsPrimary,IsRotating=x.IsRotating,IsTemporary=x.IsTemporary,IsActive=x.IsActive,Notes=x.Notes,CreateDateTime=x.CreateDateTime,CreateBy=x.CreateBy==Guid.Empty?null:x.CreateBy
        }
        ;
        async Task<string?> Validate(Guid wf,CreateWfpWorkScheduleAssignmentRequest r,Guid? exclude,CancellationToken ct)
        {
            if(r.WorkScheduleId==Guid.Empty||!await _dbContext.MstWorkSchedules.AnyAsync(x=>x.Id==r.WorkScheduleId&&x.IsActive&&!x.IsDelete,ct))return "Work schedule tidak ditemukan atau tidak aktif.";
            if(string.IsNullOrWhiteSpace(r.AssignmentType)||!new[]
            {
                "Primary","Temporary","Rotation","Project","OnCall"
            }
            .Contains(r.AssignmentType.Trim(),StringComparer.OrdinalIgnoreCase))return "Assignment type tidak valid.";
            if(r.EffectiveEndDate.HasValue&&r.EffectiveEndDate.Value<r.EffectiveStartDate)return "Periode efektif tidak valid.";
            if(r.ShiftGroupId.HasValue&&!await _dbContext.MstShiftGroups.AnyAsync(x=>x.Id==r.ShiftGroupId.Value&&x.IsActive&&!x.IsDelete,ct))return "Shift group tidak ditemukan.";
            if(r.ShiftPatternId.HasValue&&!await _dbContext.MstShiftPatterns.AnyAsync(x=>x.Id==r.ShiftPatternId.Value&&x.IsActive&&!x.IsDelete,ct))return "Shift pattern tidak ditemukan.";
            if(r.IsPrimary&&await _dbContext.WfpWorkScheduleAssignments.AnyAsync(x=>x.WorkforceProfileId==wf&&x.IsPrimary&&x.IsActive&&!x.IsDelete&&(!exclude.HasValue||x.Id!=exclude)&&(!x.EffectiveEndDate.HasValue||x.EffectiveEndDate>=r.EffectiveStartDate)&&( !r.EffectiveEndDate.HasValue||x.EffectiveStartDate<=r.EffectiveEndDate),ct))return "Primary work schedule assignment aktif pada periode tersebut sudah tersedia.";
            return null;
        }
        async Task<bool> Exists(Guid id,CancellationToken ct)=>await _dbContext.Set<MstWorkforceProfile>().AnyAsync(x=>x.Id==id&&x.IsActive&&!x.IsDelete,ct);
        IActionResult NF()=>NotFound(ApiResponse<object>.Fail(404,"Workforce profile tidak ditemukan atau tidak aktif."));
        Guid Actor()
        {
            var v=User.FindFirstValue(ClaimTypes.NameIdentifier)??User.FindFirstValue("user_id");
            return Guid.TryParse(v,out var id)?id:Guid.Empty;
        }
        static Guid? NG(Guid? v)=>!v.HasValue||v==Guid.Empty?null:v;
        static string? N(string? v)=>string.IsNullOrWhiteSpace(v)?null:v.Trim();
    }
}
