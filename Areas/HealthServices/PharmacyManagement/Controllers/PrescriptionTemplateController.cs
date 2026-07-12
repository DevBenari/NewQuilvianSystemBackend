using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponsePrescriptionTemplatePagedResult = QuilvianSystemBackend.Responses.PagedResult<QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs.PrescriptionTemplateResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/pharmacy-management/prescription-templates")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_PHARMACY",
        moduleName: "Health Service Pharmacy",
        displayName: "Prescription Template",
        AreaName = "HealthServices",
        ControllerName = "PrescriptionTemplate",
        Description = "Template resep dokter untuk obat umum dan racikan",
        SortOrder = 4)]
    [Tags("Health Services / Pharmacy Management / Prescription Template")]
    public class PrescriptionTemplateController : ControllerBase
    {
        private const string LogCategory = "HealthServices.Pharmacy";
        private readonly ApplicationDbContext _dbContext;
        private readonly PrescriptionTemplateService _templateService;
        private readonly LoggerService _loggerService;

        public PrescriptionTemplateController(
            ApplicationDbContext dbContext,
            PrescriptionTemplateService templateService,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _templateService = templateService;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<PrescriptionTemplateFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Prescription Template", Description = "Melihat metadata filter template resep", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PrescriptionTemplate", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new PrescriptionTemplateFilterMetadataResponse
            {
                DefaultFilter = new PrescriptionTemplateDefaultFilterResponse(),
                SortOptions = new List<PrescriptionTemplateSortOptionResponse>
                {
                    new() { Value = "templateName", Label = "Nama template" },
                    new() { Value = "templateCode", Label = "Kode template" },
                    new() { Value = "templateCategory", Label = "Kategori" },
                    new() { Value = "usageCount", Label = "Jumlah penggunaan" },
                    new() { Value = "lastUsedAt", Label = "Terakhir digunakan" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            await _loggerService.InfoAsync(LogCategory, "PrescriptionTemplate.GetFilterMetadata", "Mengambil metadata filter template resep.", result);
            return Ok(ApiResponse<PrescriptionTemplateFilterMetadataResponse>.Ok(result, "Metadata filter template resep berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponsePrescriptionTemplatePagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Prescription Template", Description = "Melihat daftar template resep", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PrescriptionTemplate", "Read")]
        public async Task<IActionResult> GetTemplates(
            [FromQuery] string? search,
            [FromQuery] Guid? ownerDoctorId,
            [FromQuery] string? templateCategory,
            [FromQuery] bool? isShared,
            [FromQuery] bool? isFavorite,
            [FromQuery] bool? isActive = true,
            [FromQuery] string? sortBy = "templateName",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            (pageNumber, pageSize) = NormalizePaging(pageNumber, pageSize);
            var query = BuildBaseQuery().AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x => x.TemplateCode.ToLower().Contains(keyword) || x.TemplateName.ToLower().Contains(keyword) ||
                    (x.TemplateCategory != null && x.TemplateCategory.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)) ||
                    (x.OwnerDoctor != null && x.OwnerDoctor.FullName.ToLower().Contains(keyword)));
            }
            if (ownerDoctorId.HasValue && ownerDoctorId.Value != Guid.Empty)
                query = query.Where(x => x.OwnerDoctorId == ownerDoctorId.Value || x.IsShared);
            if (!string.IsNullOrWhiteSpace(templateCategory))
                query = query.Where(x => x.TemplateCategory == templateCategory.Trim());
            if (isShared.HasValue) query = query.Where(x => x.IsShared == isShared.Value);
            if (isFavorite.HasValue) query = query.Where(x => x.IsFavorite == isFavorite.Value);
            if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);

            var totalData = await query.CountAsync();
            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return Ok(ApiResponse<ResponsePrescriptionTemplatePagedResult>.Ok(new ResponsePrescriptionTemplatePagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = entities.Select(ToResponse).ToList()
            }, "Data template resep berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PrescriptionTemplateDetailResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Prescription Template", Description = "Melihat detail template resep", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PrescriptionTemplate", "Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await BuildDetailQuery().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Template resep tidak ditemukan."));
            return Ok(ApiResponse<PrescriptionTemplateDetailResponse>.Ok(ToDetailResponse(entity), "Detail template resep berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Create", "Create Prescription Template", Description = "Membuat template resep", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("PrescriptionTemplate", "Create")]
        public async Task<IActionResult> Create([FromBody] CreatePrescriptionTemplateRequest request)
        {
            try
            {
                var entity = await _templateService.CreateAsync(request, GetCurrentUserId());
                var response = ToResponse(await BuildBaseQuery().AsNoTracking().FirstAsync(x => x.Id == entity.Id));
                await _loggerService.InfoAsync(LogCategory, "PrescriptionTemplate.Create", "Membuat template resep.", response);
                return Ok(ApiResponse<PrescriptionTemplateResponse>.Ok(response, "Template resep berhasil dibuat."));
            }
            catch (InvalidOperationException ex) { return BadRequest(ApiResponse<object>.Fail(400, ex.Message)); }
        }

        [HttpPost("from-prescription")]
        [AccessAction("Create", "Create Prescription Template", Description = "Membuat template dari resep", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("PrescriptionTemplate", "Create")]
        public async Task<IActionResult> CreateFromPrescription([FromBody] CreateTemplateFromPrescriptionRequest request)
        {
            try
            {
                var entity = await _templateService.CreateFromPrescriptionAsync(request, GetCurrentUserId());
                var response = ToResponse(await BuildBaseQuery().AsNoTracking().FirstAsync(x => x.Id == entity.Id));
                await _loggerService.InfoAsync(LogCategory, "PrescriptionTemplate.CreateFromPrescription", "Membuat template dari resep.", response);
                return Ok(ApiResponse<PrescriptionTemplateResponse>.Ok(response, "Template dari resep berhasil dibuat."));
            }
            catch (InvalidOperationException ex) { return BadRequest(ApiResponse<object>.Fail(400, ex.Message)); }
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Prescription Template", Description = "Mengubah template resep", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("PrescriptionTemplate", "Update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePrescriptionTemplateRequest request)
        {
            try
            {
                await _templateService.UpdateAsync(id, request, GetCurrentUserId());
                var response = ToDetailResponse(await BuildDetailQuery().AsNoTracking().FirstAsync(x => x.Id == id));
                await _loggerService.InfoAsync(LogCategory, "PrescriptionTemplate.Update", "Mengubah template resep.", response);
                return Ok(ApiResponse<PrescriptionTemplateDetailResponse>.Ok(response, "Template resep berhasil diubah."));
            }
            catch (InvalidOperationException ex) { return BadRequest(ApiResponse<object>.Fail(400, ex.Message)); }
        }

        [HttpPost("{id:guid}/apply")]
        [AccessAction("Create", "Apply Prescription Template", Description = "Menerapkan template ke resep draft", AccessType = AccessTypes.Create, SortOrder = 4)]
        [AccessPermission("PrescriptionTemplate", "Create")]
        public async Task<IActionResult> Apply(Guid id, [FromBody] ApplyPrescriptionTemplateRequest request)
        {
            try
            {
                var response = await _templateService.ApplyAsync(id, request, GetCurrentUserId());
                await _loggerService.InfoAsync(LogCategory, "PrescriptionTemplate.Apply", "Menerapkan template ke resep.", response);
                return Ok(ApiResponse<ApplyPrescriptionTemplateResponse>.Ok(response, "Template berhasil diterapkan ke resep."));
            }
            catch (InvalidOperationException ex) { return BadRequest(ApiResponse<object>.Fail(400, ex.Message)); }
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete Prescription Template", Description = "Menghapus template resep", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("PrescriptionTemplate", "Delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var entity = await _dbContext.Set<MstPrescriptionTemplate>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(404, "Template resep tidak ditemukan."));
            var now = DateTime.UtcNow; var actor = GetCurrentUserId();
            entity.IsDelete = true; entity.IsActive = false; entity.DeleteDateTime = now; entity.DeleteBy = actor; entity.UpdateDateTime = now; entity.UpdateBy = actor;
            await _dbContext.SaveChangesAsync();
            await _loggerService.InfoAsync(LogCategory, "PrescriptionTemplate.Delete", "Menghapus template resep.", new { id });
            return Ok(ApiResponse<object>.Ok(null, "Template resep berhasil dihapus."));
        }

        private IQueryable<MstPrescriptionTemplate> BuildBaseQuery() => _dbContext.Set<MstPrescriptionTemplate>().Include(x => x.OwnerDoctor).Where(x => !x.IsDelete);
        private IQueryable<MstPrescriptionTemplate> BuildDetailQuery() => BuildBaseQuery()
            .Include(x => x.Items.Where(i => !i.IsDelete && i.IsActive)).ThenInclude(x => x.Drug)
            .Include(x => x.Items.Where(i => !i.IsDelete && i.IsActive)).ThenInclude(x => x.DoseUnitMeasurement)
            .Include(x => x.Items.Where(i => !i.IsDelete && i.IsActive)).ThenInclude(x => x.DispenseUnitMeasurement)
            .Include(x => x.Compounds.Where(c => !c.IsDelete && c.IsActive)).ThenInclude(x => x.PackageUnitMeasurement)
            .Include(x => x.Compounds.Where(c => !c.IsDelete && c.IsActive)).ThenInclude(x => x.DoseUnitMeasurement)
            .Include(x => x.Compounds.Where(c => !c.IsDelete && c.IsActive)).ThenInclude(x => x.Items.Where(i => !i.IsDelete && i.IsActive)).ThenInclude(x => x.Drug)
            .Include(x => x.Compounds.Where(c => !c.IsDelete && c.IsActive)).ThenInclude(x => x.Items.Where(i => !i.IsDelete && i.IsActive)).ThenInclude(x => x.QuantityUnitMeasurement);

        private static PrescriptionTemplateResponse ToResponse(MstPrescriptionTemplate x) => new()
        {
            Id = x.Id,
            TemplateCode = x.TemplateCode,
            TemplateName = x.TemplateName,
            TemplateCategory = x.TemplateCategory,
            Description = x.Description,
            OwnerDoctorId = x.OwnerDoctorId,
            OwnerDoctorName = x.OwnerDoctor?.FullName ?? string.Empty,
            IsShared = x.IsShared,
            IsFavorite = x.IsFavorite,
            UsageCount = x.UsageCount,
            LastUsedAt = x.LastUsedAt,
            RegularItemCount = x.RegularItemCount,
            CompoundCount = x.CompoundCount,
            CompoundIngredientCount = x.CompoundIngredientCount,
            TotalItemCount = x.TotalItemCount,
            IsActive = x.IsActive,
            CreateDateTime = x.CreateDateTime
        };

        private static PrescriptionTemplateDetailResponse ToDetailResponse(MstPrescriptionTemplate x)
        {
            var r = new PrescriptionTemplateDetailResponse();
            var b = ToResponse(x);
            r.Id = b.Id; r.TemplateCode = b.TemplateCode; r.TemplateName = b.TemplateName; r.TemplateCategory = b.TemplateCategory; r.Description = b.Description;
            r.OwnerDoctorId = b.OwnerDoctorId; r.OwnerDoctorName = b.OwnerDoctorName; r.IsShared = b.IsShared; r.IsFavorite = b.IsFavorite; r.UsageCount = b.UsageCount;
            r.LastUsedAt = b.LastUsedAt; r.RegularItemCount = b.RegularItemCount; r.CompoundCount = b.CompoundCount; r.CompoundIngredientCount = b.CompoundIngredientCount;
            r.TotalItemCount = b.TotalItemCount; r.IsActive = b.IsActive; r.CreateDateTime = b.CreateDateTime;
            r.Items = x.Items.OrderBy(i => i.SortOrder).Select(i => new PrescriptionTemplateItemResponse
            {
                Id = i.Id,
                DrugId = i.DrugId,
                DrugCode = i.Drug?.DrugCode ?? string.Empty,
                DrugName = i.Drug?.DrugName ?? string.Empty,
                GenericName = i.Drug?.GenericName,
                Strength = i.Drug?.Strength,
                DrugForm = i.Drug?.DrugForm,
                Dose = i.Dose,
                DoseUnitMeasurementId = i.DoseUnitMeasurementId,
                DoseUnitName = i.DoseUnitMeasurement?.MeasurementName,
                FrequencyCode = i.FrequencyCode,
                FrequencyText = i.FrequencyText,
                FrequencyPerDay = i.FrequencyPerDay,
                DurationValue = i.DurationValue,
                DurationUnit = i.DurationUnit,
                IsAsNeeded = i.IsAsNeeded,
                AdministrationTime = i.AdministrationTime,
                Signa = i.Signa,
                AdministrationInstruction = i.AdministrationInstruction,
                DoctorNote = i.DoctorNote,
                Quantity = i.Quantity,
                DispenseUnitMeasurementId = i.DispenseUnitMeasurementId,
                DispenseUnitName = i.DispenseUnitMeasurement?.MeasurementName,
                SortOrder = i.SortOrder
            }).ToList();
            r.Compounds = x.Compounds.OrderBy(c => c.SortOrder).Select(c => new PrescriptionTemplateCompoundResponse
            {
                Id = c.Id,
                CompoundName = c.CompoundName,
                CompoundForm = c.CompoundForm,
                TotalPackage = c.TotalPackage,
                PackageUnitMeasurementId = c.PackageUnitMeasurementId,
                PackageUnitName = c.PackageUnitMeasurement?.MeasurementName,
                DosePerUse = c.DosePerUse,
                DoseUnitMeasurementId = c.DoseUnitMeasurementId,
                DoseUnitName = c.DoseUnitMeasurement?.MeasurementName,
                FrequencyCode = c.FrequencyCode,
                FrequencyText = c.FrequencyText,
                FrequencyPerDay = c.FrequencyPerDay,
                DurationValue = c.DurationValue,
                DurationUnit = c.DurationUnit,
                IsAsNeeded = c.IsAsNeeded,
                AdministrationTime = c.AdministrationTime,
                Signa = c.Signa,
                CompoundingInstruction = c.CompoundingInstruction,
                AdministrationInstruction = c.AdministrationInstruction,
                DoctorNote = c.DoctorNote,
                SortOrder = c.SortOrder,
                Items = c.Items.OrderBy(i => i.SortOrder).Select(i => new PrescriptionTemplateCompoundItemResponse
                {
                    Id = i.Id,
                    DrugId = i.DrugId,
                    DrugCode = i.Drug?.DrugCode ?? string.Empty,
                    DrugName = i.Drug?.DrugName ?? string.Empty,
                    GenericName = i.Drug?.GenericName,
                    Strength = i.Drug?.Strength,
                    AmountPerPackage = i.AmountPerPackage,
                    TotalQuantity = i.TotalQuantity,
                    QuantityUnitMeasurementId = i.QuantityUnitMeasurementId,
                    QuantityUnitName = i.QuantityUnitMeasurement?.MeasurementName,
                    IngredientInstruction = i.IngredientInstruction,
                    SortOrder = i.SortOrder
                }).ToList()
            }).ToList();
            return r;
        }

        private static IQueryable<MstPrescriptionTemplate> ApplySorting(IQueryable<MstPrescriptionTemplate> q, string? sortBy, string? direction)
        {
            var desc = string.Equals(direction, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "templateName").ToLowerInvariant() switch
            {
                "templatecode" => desc ? q.OrderByDescending(x => x.TemplateCode) : q.OrderBy(x => x.TemplateCode),
                "templatecategory" => desc ? q.OrderByDescending(x => x.TemplateCategory) : q.OrderBy(x => x.TemplateCategory),
                "usagecount" => desc ? q.OrderByDescending(x => x.UsageCount) : q.OrderBy(x => x.UsageCount),
                "lastusedat" => desc ? q.OrderByDescending(x => x.LastUsedAt) : q.OrderBy(x => x.LastUsedAt),
                "createdatetime" => desc ? q.OrderByDescending(x => x.CreateDateTime) : q.OrderBy(x => x.CreateDateTime),
                _ => desc ? q.OrderByDescending(x => x.TemplateName) : q.OrderBy(x => x.TemplateName)
            };
        }
        private static (int, int) NormalizePaging(int p, int s) => (p <= 0 ? 1 : p, s <= 0 ? 25 : Math.Min(s, 100));
        private Guid GetCurrentUserId() => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;
    }
}
