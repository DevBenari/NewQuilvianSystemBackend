using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using TrainingCategoryPagedResult = QuilvianSystemBackend.Responses.PagedResult<QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.DTOs.TrainingCategoryResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Controllers
{
    [ApiController, Authorize]
    [Route("api/v1/corporate/human-resource/master-data/training-categories")]
    [AccessController("HUMAN_RESOURCE_MASTER_DATA", "Human Resource Master Data", "Training Category", AreaName="Corporate", ControllerName="TrainingCategory", Description="Corporate human resource master data training category", SortOrder=21)]
    [Tags("Corporate / Human Resource / Master Data / Training Category")]
    public class TrainingCategoryController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.MasterData";
        private const string CodePrefix = "TCG-RSMMC-";
        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        public TrainingCategoryController(ApplicationDbContext dbContext, LoggerService loggerService) { _dbContext=dbContext; _loggerService=loggerService; }

        [HttpGet("filters/metadata"), AccessAction("Read","Read Training Category",Description="Melihat metadata filter training category",AccessType=AccessTypes.Read,SortOrder=1), AccessPermission("TrainingCategory","Read")]
        public IActionResult GetFilterMetadata() => Ok(ApiResponse<TrainingCategoryFilterMetadataResponse>.Ok(new TrainingCategoryFilterMetadataResponse
        {
            DefaultFilter=new(), CustomPeriods=Periods(), SortDirections=new(){"asc","desc"}, PageSizeOptions=new(){10,25,50,100},
            SortOptions=new(){ new(){Value="createDateTime",Label="Tanggal dibuat"},new(){Value="trainingCategoryCode",Label="Kode kategori"},new(){Value="trainingCategoryName",Label="Nama kategori"},new(){Value="catalogCount",Label="Jumlah katalog"},new(){Value="isActive",Label="Status aktif"}}
        },"Metadata filter training category berhasil diambil."));

        [HttpGet("summary"), AccessAction("Read","Read Training Category",Description="Melihat ringkasan training category",AccessType=AccessTypes.Read,SortOrder=1), AccessPermission("TrainingCategory","Read")]
        public async Task<IActionResult> GetSummary(CancellationToken ct) { var q=Base(); return Ok(ApiResponse<TrainingCategorySummaryResponse>.Ok(new(){TotalTrainingCategory=await q.CountAsync(ct),ActiveTrainingCategory=await q.CountAsync(x=>x.IsActive,ct),InactiveTrainingCategory=await q.CountAsync(x=>!x.IsActive,ct),MandatoryCategory=await q.CountAsync(x=>x.IsMandatoryCategory,ct),NonMandatoryCategory=await q.CountAsync(x=>!x.IsMandatoryCategory,ct)},"Ringkasan training category berhasil diambil.")); }

        [HttpGet, AccessAction("Read","Read Training Category",Description="Melihat data training category",AccessType=AccessTypes.Read,SortOrder=1), AccessPermission("TrainingCategory","Read")]
        public async Task<IActionResult> Get([FromQuery]DateTime? startDate,[FromQuery]DateTime? endDate,[FromQuery]string? customPeriod,[FromQuery]bool? isMandatoryCategory,[FromQuery]bool? isActive,[FromQuery]string? search,[FromQuery]string? sortBy="trainingCategoryName",[FromQuery]string? sortDirection="asc",[FromQuery]int pageNumber=1,[FromQuery]int pageSize=25,CancellationToken ct=default)
        {
            NormalizePaging(ref pageNumber,ref pageSize); var q=DateFilter(Base(),startDate,endDate,customPeriod);
            if(isMandatoryCategory.HasValue) q=q.Where(x=>x.IsMandatoryCategory==isMandatoryCategory.Value); if(isActive.HasValue) q=q.Where(x=>x.IsActive==isActive.Value);
            if(!string.IsNullOrWhiteSpace(search)){var k=search.Trim().ToLower();q=q.Where(x=>x.TrainingCategoryCode.ToLower().Contains(k)||x.TrainingCategoryName.ToLower().Contains(k)||(x.Description!=null&&x.Description.ToLower().Contains(k)));}
            var total=await q.CountAsync(ct); var entities=await Sort(q,sortBy,sortDirection).Skip((pageNumber-1)*pageSize).Take(pageSize).ToListAsync(ct); var actors=await Actors(entities.Select(x=>x.CreateBy),ct);
            var items=entities.Select(x=>Map(x,actors)).ToList(); return Ok(ApiResponse<TrainingCategoryPagedResult>.Ok(new(){PageNumber=pageNumber,PageSize=pageSize,TotalData=total,TotalPage=(int)Math.Ceiling(total/(double)pageSize),Items=items},"Data training category berhasil diambil."));
        }

        [HttpGet("options"), AccessAction("Read","Read Training Category",Description="Melihat pilihan training category",AccessType=AccessTypes.Read,SortOrder=1), AccessPermission("TrainingCategory","Read")]
        public async Task<IActionResult> Options([FromQuery]bool onlyActive=true,[FromQuery]string? search=null,[FromQuery]int pageNumber=1,[FromQuery]int pageSize=25,CancellationToken ct=default)
        { NormalizePaging(ref pageNumber,ref pageSize); var q=Base(); if(onlyActive)q=q.Where(x=>x.IsActive); if(!string.IsNullOrWhiteSpace(search)){var k=search.Trim().ToLower();q=q.Where(x=>x.TrainingCategoryCode.ToLower().Contains(k)||x.TrainingCategoryName.ToLower().Contains(k));} var total=await q.CountAsync(ct); var items=await q.OrderBy(x=>x.TrainingCategoryName).Skip((pageNumber-1)*pageSize).Take(pageSize).Select(x=>new TrainingCategoryOptionResponse{Id=x.Id,TrainingCategoryCode=x.TrainingCategoryCode,TrainingCategoryName=x.TrainingCategoryName,IsMandatoryCategory=x.IsMandatoryCategory}).ToListAsync(ct); return Ok(ApiResponse<TrainingCategoryOptionPagedResponse>.Ok(new(){PageNumber=pageNumber,PageSize=pageSize,TotalData=total,TotalPage=(int)Math.Ceiling(total/(double)pageSize),Items=items},"Pilihan training category berhasil diambil.")); }

        [HttpGet("{id:guid}"), AccessAction("Read","Read Training Category",Description="Melihat detail training category",AccessType=AccessTypes.Read,SortOrder=1), AccessPermission("TrainingCategory","Read")]
        public async Task<IActionResult> Detail(Guid id,CancellationToken ct){var x=await Base().FirstOrDefaultAsync(x=>x.Id==id,ct);if(x==null)return NotFound(ApiResponse<object>.Fail(404,"Training category tidak ditemukan."));var actors=await Actors(new[]{x.CreateBy,x.UpdateBy},ct);var r=Map(x,actors);return Ok(ApiResponse<TrainingCategoryDetailResponse>.Ok(new(){Id=r.Id,TrainingCategoryCode=r.TrainingCategoryCode,TrainingCategoryName=r.TrainingCategoryName,IsMandatoryCategory=r.IsMandatoryCategory,Description=r.Description,IsActive=r.IsActive,TrainingCatalogCount=r.TrainingCatalogCount,CreateDateTime=r.CreateDateTime,CreateBy=r.CreateBy,CreateByName=r.CreateByName,UpdateDateTime=x.UpdateDateTime,UpdateBy=x.UpdateBy==Guid.Empty?null:x.UpdateBy,UpdateByName=Actor(actors,x.UpdateBy)},"Detail training category berhasil diambil."));}

        [HttpPost, AccessAction("Create","Create Training Category",Description="Membuat training category",AccessType=AccessTypes.Create,SortOrder=2), AccessPermission("TrainingCategory","Create")]
        public async Task<IActionResult> Create([FromBody]CreateTrainingCategoryRequest request,CancellationToken ct){var v=await Validate(null,request,ct);if(!v.ok)return BadRequest(ApiResponse<object>.Fail(400,v.error!));var e=new MstTrainingCategory{Id=Guid.NewGuid(),TrainingCategoryCode=await GenerateCode(ct),TrainingCategoryName=request.TrainingCategoryName.Trim(),IsMandatoryCategory=request.IsMandatoryCategory,Description=Norm(request.Description),IsActive=true,CreateDateTime=DateTime.UtcNow,CreateBy=UserId(),IsDelete=false,IsCancel=false};_dbContext.Set<MstTrainingCategory>().Add(e);await _dbContext.SaveChangesAsync(ct);await _loggerService.InfoAsync(LogCategory,"TrainingCategory.Create","Membuat training category.",new{e.Id,e.TrainingCategoryCode,e.TrainingCategoryName});return Ok(ApiResponse<TrainingCategoryCreateResponse>.Ok(new(){Id=e.Id,TrainingCategoryCode=e.TrainingCategoryCode,TrainingCategoryName=e.TrainingCategoryName,IsActive=e.IsActive},"Training category berhasil dibuat."));}

        [HttpPut("{id:guid}"), AccessAction("Update","Update Training Category",Description="Mengubah training category",AccessType=AccessTypes.Update,SortOrder=3), AccessPermission("TrainingCategory","Update")]
        public async Task<IActionResult> Update(Guid id,[FromBody]UpdateTrainingCategoryRequest request,CancellationToken ct){var e=await Find(id,ct);if(e==null)return NotFound(ApiResponse<object>.Fail(404,"Training category tidak ditemukan."));var v=await Validate(id,request,ct);if(!v.ok)return BadRequest(ApiResponse<object>.Fail(400,v.error!));e.TrainingCategoryName=request.TrainingCategoryName.Trim();e.IsMandatoryCategory=request.IsMandatoryCategory;e.Description=Norm(request.Description);e.IsActive=request.IsActive;e.UpdateDateTime=DateTime.UtcNow;e.UpdateBy=UserId();await _dbContext.SaveChangesAsync(ct);return Ok(ApiResponse<object>.Ok(null,"Training category berhasil diperbarui."));}

        [HttpPatch("{id:guid}/status"), AccessAction("Update","Update Training Category Status",Description="Mengubah status training category",AccessType=AccessTypes.Update,SortOrder=4), AccessPermission("TrainingCategory","Update")]
        public async Task<IActionResult> Status(Guid id,[FromBody]UpdateTrainingCategoryStatusRequest request,CancellationToken ct){var e=await Find(id,ct);if(e==null)return NotFound(ApiResponse<object>.Fail(404,"Training category tidak ditemukan."));e.IsActive=request.IsActive;e.UpdateDateTime=DateTime.UtcNow;e.UpdateBy=UserId();await _dbContext.SaveChangesAsync(ct);return Ok(ApiResponse<object>.Ok(null,"Status training category berhasil diperbarui."));}

        [HttpDelete("{id:guid}"), AccessAction("Delete","Delete Training Category",Description="Menghapus training category",AccessType=AccessTypes.Delete,SortOrder=5), AccessPermission("TrainingCategory","Delete")]
        public async Task<IActionResult> Delete(Guid id,CancellationToken ct){var e=await Find(id,ct);if(e==null)return NotFound(ApiResponse<object>.Fail(404,"Training category tidak ditemukan."));if(await _dbContext.Set<MstTrainingCatalog>().AsNoTracking().AnyAsync(x=>x.TrainingCategoryId==id&&!x.IsDelete,ct))return BadRequest(ApiResponse<object>.Fail(400,"Training category tidak dapat dihapus karena sudah digunakan oleh training catalog."));var now=DateTime.UtcNow;var actor=UserId();e.IsDelete=true;e.IsActive=false;e.DeleteDateTime=now;e.DeleteBy=actor;e.UpdateDateTime=now;e.UpdateBy=actor;await _dbContext.SaveChangesAsync(ct);return Ok(ApiResponse<object>.Ok(null,"Training category berhasil dihapus."));}

        private IQueryable<MstTrainingCategory> Base()=>_dbContext.Set<MstTrainingCategory>().AsNoTracking().Where(x=>!x.IsDelete);
        private async Task<MstTrainingCategory?> Find(Guid id,CancellationToken ct)=>await _dbContext.Set<MstTrainingCategory>().FirstOrDefaultAsync(x=>x.Id==id&&!x.IsDelete,ct);
        private async Task<(bool ok,string? error)> Validate(Guid? id,CreateTrainingCategoryRequest r,CancellationToken ct){if(string.IsNullOrWhiteSpace(r.TrainingCategoryName))return(false,"Nama training category wajib diisi.");var name=r.TrainingCategoryName.Trim().ToLower();var q=_dbContext.Set<MstTrainingCategory>().AsNoTracking().Where(x=>!x.IsDelete&&x.TrainingCategoryName.ToLower()==name);if(id.HasValue)q=q.Where(x=>x.Id!=id.Value);if(await q.AnyAsync(ct))return(false,"Nama training category sudah digunakan.");return(true,null);}
        private async Task<string> GenerateCode(CancellationToken ct){var codes=await _dbContext.Set<MstTrainingCategory>().AsNoTracking().Where(x=>!x.IsDelete&&x.TrainingCategoryCode.StartsWith(CodePrefix)).Select(x=>x.TrainingCategoryCode).ToListAsync(ct);return Next(codes,CodePrefix);}
        private TrainingCategoryResponse Map(MstTrainingCategory x,IReadOnlyDictionary<Guid,string> a)=>new(){Id=x.Id,TrainingCategoryCode=x.TrainingCategoryCode,TrainingCategoryName=x.TrainingCategoryName,IsMandatoryCategory=x.IsMandatoryCategory,Description=x.Description,IsActive=x.IsActive,TrainingCatalogCount=_dbContext.Set<MstTrainingCatalog>().Count(c=>c.TrainingCategoryId==x.Id&&!c.IsDelete),CreateDateTime=x.CreateDateTime,CreateBy=x.CreateBy==Guid.Empty?null:x.CreateBy,CreateByName=Actor(a,x.CreateBy)};
        private static IOrderedQueryable<MstTrainingCategory> Sort(IQueryable<MstTrainingCategory> q,string? by,string? dir){var d=string.Equals(dir,"desc",StringComparison.OrdinalIgnoreCase);return(by??"trainingCategoryName").ToLowerInvariant() switch{"createdatetime"=>d?q.OrderByDescending(x=>x.CreateDateTime):q.OrderBy(x=>x.CreateDateTime),"trainingcategorycode"=>d?q.OrderByDescending(x=>x.TrainingCategoryCode):q.OrderBy(x=>x.TrainingCategoryCode),"isactive"=>d?q.OrderByDescending(x=>x.IsActive):q.OrderBy(x=>x.IsActive),_=>d?q.OrderByDescending(x=>x.TrainingCategoryName):q.OrderBy(x=>x.TrainingCategoryName)};}
        private async Task<Dictionary<Guid,string>> Actors(IEnumerable<Guid> ids,CancellationToken ct){var v=ids.Where(x=>x!=Guid.Empty).Distinct().ToList();return await _dbContext.Users.AsNoTracking().Where(x=>v.Contains(x.Id)).ToDictionaryAsync(x=>x.Id,x=>x.DisplayName??x.UserName??x.Email??x.UserCode,ct);} private static string? Actor(IReadOnlyDictionary<Guid,string>a,Guid id)=>id==Guid.Empty?null:a.GetValueOrDefault(id);
        private Guid UserId(){var s=User.FindFirstValue(ClaimTypes.NameIdentifier)??User.FindFirstValue("user_id");return Guid.TryParse(s,out var id)?id:Guid.Empty;} private static string? Norm(string? s)=>string.IsNullOrWhiteSpace(s)?null:s.Trim();
        private static string Next(IEnumerable<string> codes,string prefix){var used=codes.Select(x=>x.Replace(prefix,"")).Where(x=>int.TryParse(x,out _)).Select(int.Parse).ToHashSet();var n=1;while(used.Contains(n))n++;return prefix+n.ToString("D5");}
        private static void NormalizePaging(ref int p,ref int s){p=Math.Max(1,p);s=s<1?25:Math.Min(s,100);} private static List<TrainingCategoryCustomPeriodOptionResponse> Periods()=>new(){new(){Value="today",Label="Hari ini"},new(){Value="last7days",Label="7 hari terakhir"},new(){Value="thismonth",Label="Bulan ini"},new(){Value="lastmonth",Label="Bulan lalu"}};
        private static IQueryable<MstTrainingCategory> DateFilter(IQueryable<MstTrainingCategory> q,DateTime? s,DateTime? e,string? p){var r=Range(s,e,p);if(r.Item1.HasValue)q=q.Where(x=>x.CreateDateTime>=r.Item1.Value);if(r.Item2.HasValue)q=q.Where(x=>x.CreateDateTime<r.Item2.Value);return q;} private static (DateTime?,DateTime?) Range(DateTime? s,DateTime? e,string? p){if(s.HasValue||e.HasValue)return(s?.Date,e?.Date.AddDays(1));var t=DateTime.UtcNow.Date;return p?.ToLowerInvariant() switch{"today"=>(t,t.AddDays(1)),"last7days"=>(t.AddDays(-6),t.AddDays(1)),"thismonth"=>(new DateTime(t.Year,t.Month,1),new DateTime(t.Year,t.Month,1).AddMonths(1)),"lastmonth"=>(new DateTime(t.Year,t.Month,1).AddMonths(-1),new DateTime(t.Year,t.Month,1)),_=>(null,null)};}
    }
}
