using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponseTariffCategoryPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs.TariffCategoryResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/master-data/tariff-categories")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MASTER_DATA",
        moduleName: "Health Service Master Data",
        displayName: "Tariff Category",
        AreaName = "HealthServices",
        ControllerName = "TariffCategory",
        Description = "Health service master data tariff category",
        SortOrder = 7
    )]
    [Tags("Health Services / Master Data / Tariff Category")]
    public class TariffCategoryController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MasterData";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public TariffCategoryController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<TariffCategoryFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Tariff Category", Description = "Melihat data tariff category", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("TariffCategory", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new TariffCategoryFilterMetadataResponse
            {
                DefaultFilter = new TariffCategoryDefaultFilterResponse(),
                SortOptions = new List<TariffCategorySortOptionResponse>
                {
                    new() { Value = "sortOrder", Label = "Urutan" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "tariffCategoryCode", Label = "Kode kategori tarif" },
                    new() { Value = "tariffCategoryName", Label = "Nama kategori tarif" },
                    new() { Value = "tariffGroupName", Label = "Grup tarif" },
                    new() { Value = "isCoveredByInsuranceDefault", Label = "Default ditanggung asuransi" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "TariffCategory.GetFilterMetadata",
                "Mengambil metadata filter tariff category.",
                result
            );

            return Ok(ApiResponse<TariffCategoryFilterMetadataResponse>.Ok(
                result,
                "Metadata filter tariff category berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<TariffCategorySummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Tariff Category", Description = "Melihat data tariff category", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("TariffCategory", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = _dbContext.Set<MstTariffCategory>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            var result = new TariffCategorySummaryResponse
            {
                TotalTariffCategory = await query.CountAsync(),
                ActiveTariffCategory = await query.CountAsync(x => x.IsActive),
                InactiveTariffCategory = await query.CountAsync(x => !x.IsActive),
                RegistrationFeeCategory = await query.CountAsync(x => x.IsRegistrationFee),
                AdministrationFeeCategory = await query.CountAsync(x => x.IsAdministrationFee),
                ConsultationFeeCategory = await query.CountAsync(x => x.IsConsultationFee),
                RoomChargeCategory = await query.CountAsync(x => x.IsRoomCharge),
                ProcedureCategory = await query.CountAsync(x => x.IsProcedure),
                LaboratoryCategory = await query.CountAsync(x => x.IsLaboratory),
                RadiologyCategory = await query.CountAsync(x => x.IsRadiology),
                PharmacyCategory = await query.CountAsync(x => x.IsPharmacy),
                SurgeryCategory = await query.CountAsync(x => x.IsSurgery),
                PackageCategory = await query.CountAsync(x => x.IsPackage),
                InsuranceCoveredDefaultCategory = await query.CountAsync(x => x.IsCoveredByInsuranceDefault)
            };

            return Ok(ApiResponse<TariffCategorySummaryResponse>.Ok(
                result,
                "Ringkasan tariff category berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseTariffCategoryPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Tariff Category", Description = "Melihat data tariff category", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("TariffCategory", "Read")]
        public async Task<IActionResult> GetTariffCategories(
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            [FromQuery] string? tariffGroupName,
            [FromQuery] bool? isRegistrationFee,
            [FromQuery] bool? isAdministrationFee,
            [FromQuery] bool? isConsultationFee,
            [FromQuery] bool? isRoomCharge,
            [FromQuery] bool? isProcedure,
            [FromQuery] bool? isLaboratory,
            [FromQuery] bool? isRadiology,
            [FromQuery] bool? isPharmacy,
            [FromQuery] bool? isSurgery,
            [FromQuery] bool? isPackage,
            [FromQuery] bool? isCoveredByInsuranceDefault,
            [FromQuery] string? sortBy = "sortOrder",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = _dbContext.Set<MstTariffCategory>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.TariffCategoryCode.ToLower().Contains(keyword) ||
                    x.TariffCategoryName.ToLower().Contains(keyword) ||
                    (x.TariffGroupName != null && x.TariffGroupName.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(tariffGroupName))
            {
                var groupKeyword = tariffGroupName.Trim().ToLower();
                query = query.Where(x => x.TariffGroupName != null && x.TariffGroupName.ToLower().Contains(groupKeyword));
            }

            if (isRegistrationFee.HasValue)
                query = query.Where(x => x.IsRegistrationFee == isRegistrationFee.Value);

            if (isAdministrationFee.HasValue)
                query = query.Where(x => x.IsAdministrationFee == isAdministrationFee.Value);

            if (isConsultationFee.HasValue)
                query = query.Where(x => x.IsConsultationFee == isConsultationFee.Value);

            if (isRoomCharge.HasValue)
                query = query.Where(x => x.IsRoomCharge == isRoomCharge.Value);

            if (isProcedure.HasValue)
                query = query.Where(x => x.IsProcedure == isProcedure.Value);

            if (isLaboratory.HasValue)
                query = query.Where(x => x.IsLaboratory == isLaboratory.Value);

            if (isRadiology.HasValue)
                query = query.Where(x => x.IsRadiology == isRadiology.Value);

            if (isPharmacy.HasValue)
                query = query.Where(x => x.IsPharmacy == isPharmacy.Value);

            if (isSurgery.HasValue)
                query = query.Where(x => x.IsSurgery == isSurgery.Value);

            if (isPackage.HasValue)
                query = query.Where(x => x.IsPackage == isPackage.Value);

            if (isCoveredByInsuranceDefault.HasValue)
                query = query.Where(x => x.IsCoveredByInsuranceDefault == isCoveredByInsuranceDefault.Value);

            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new TariffCategoryResponse
                {
                    Id = x.Id,
                    TariffCategoryCode = x.TariffCategoryCode,
                    TariffCategoryName = x.TariffCategoryName,
                    TariffGroupName = x.TariffGroupName,
                    IsRegistrationFee = x.IsRegistrationFee,
                    IsAdministrationFee = x.IsAdministrationFee,
                    IsConsultationFee = x.IsConsultationFee,
                    IsRoomCharge = x.IsRoomCharge,
                    IsProcedure = x.IsProcedure,
                    IsLaboratory = x.IsLaboratory,
                    IsRadiology = x.IsRadiology,
                    IsPharmacy = x.IsPharmacy,
                    IsSurgery = x.IsSurgery,
                    IsPackage = x.IsPackage,
                    IsCoveredByInsuranceDefault = x.IsCoveredByInsuranceDefault,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

            var result = new ResponseTariffCategoryPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseTariffCategoryPagedResult>.Ok(
                result,
                "Data tariff category berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<TariffCategoryOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Tariff Category", Description = "Melihat data tariff category", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("TariffCategory", "Read")]
        public async Task<IActionResult> GetTariffCategoryOptions(
            [FromQuery] string? tariffGroupName,
            [FromQuery] bool? isRegistrationFee,
            [FromQuery] bool? isAdministrationFee,
            [FromQuery] bool? isConsultationFee,
            [FromQuery] bool? isRoomCharge,
            [FromQuery] bool? isProcedure,
            [FromQuery] bool? isLaboratory,
            [FromQuery] bool? isRadiology,
            [FromQuery] bool? isPharmacy,
            [FromQuery] bool? isSurgery,
            [FromQuery] bool? isPackage,
            [FromQuery] bool? isCoveredByInsuranceDefault,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null)
        {
            var query = _dbContext.Set<MstTariffCategory>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (onlyActive)
                query = query.Where(x => x.IsActive);

            if (!string.IsNullOrWhiteSpace(tariffGroupName))
            {
                var groupKeyword = tariffGroupName.Trim().ToLower();
                query = query.Where(x => x.TariffGroupName != null && x.TariffGroupName.ToLower().Contains(groupKeyword));
            }

            if (isRegistrationFee.HasValue)
                query = query.Where(x => x.IsRegistrationFee == isRegistrationFee.Value);

            if (isAdministrationFee.HasValue)
                query = query.Where(x => x.IsAdministrationFee == isAdministrationFee.Value);

            if (isConsultationFee.HasValue)
                query = query.Where(x => x.IsConsultationFee == isConsultationFee.Value);

            if (isRoomCharge.HasValue)
                query = query.Where(x => x.IsRoomCharge == isRoomCharge.Value);

            if (isProcedure.HasValue)
                query = query.Where(x => x.IsProcedure == isProcedure.Value);

            if (isLaboratory.HasValue)
                query = query.Where(x => x.IsLaboratory == isLaboratory.Value);

            if (isRadiology.HasValue)
                query = query.Where(x => x.IsRadiology == isRadiology.Value);

            if (isPharmacy.HasValue)
                query = query.Where(x => x.IsPharmacy == isPharmacy.Value);

            if (isSurgery.HasValue)
                query = query.Where(x => x.IsSurgery == isSurgery.Value);

            if (isPackage.HasValue)
                query = query.Where(x => x.IsPackage == isPackage.Value);

            if (isCoveredByInsuranceDefault.HasValue)
                query = query.Where(x => x.IsCoveredByInsuranceDefault == isCoveredByInsuranceDefault.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.TariffCategoryCode.ToLower().Contains(keyword) ||
                    x.TariffCategoryName.ToLower().Contains(keyword) ||
                    (x.TariffGroupName != null && x.TariffGroupName.ToLower().Contains(keyword)));
            }

            var data = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.TariffCategoryName)
                .Select(x => new TariffCategoryOptionResponse
                {
                    Id = x.Id,
                    TariffCategoryCode = x.TariffCategoryCode,
                    TariffCategoryName = x.TariffCategoryName,
                    TariffGroupName = x.TariffGroupName,
                    IsRegistrationFee = x.IsRegistrationFee,
                    IsAdministrationFee = x.IsAdministrationFee,
                    IsConsultationFee = x.IsConsultationFee,
                    IsRoomCharge = x.IsRoomCharge,
                    IsProcedure = x.IsProcedure,
                    IsLaboratory = x.IsLaboratory,
                    IsRadiology = x.IsRadiology,
                    IsPharmacy = x.IsPharmacy,
                    IsSurgery = x.IsSurgery,
                    IsPackage = x.IsPackage,
                    IsCoveredByInsuranceDefault = x.IsCoveredByInsuranceDefault
                })
                .ToListAsync();

            return Ok(ApiResponse<List<TariffCategoryOptionResponse>>.Ok(
                data,
                "Data pilihan tariff category berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<TariffCategoryDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Tariff Category", Description = "Melihat data tariff category", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("TariffCategory", "Read")]
        public async Task<IActionResult> GetTariffCategoryById(Guid id)
        {
            var data = await _dbContext.Set<MstTariffCategory>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new TariffCategoryDetailResponse
                {
                    Id = x.Id,
                    TariffCategoryCode = x.TariffCategoryCode,
                    TariffCategoryName = x.TariffCategoryName,
                    TariffGroupName = x.TariffGroupName,
                    IsRegistrationFee = x.IsRegistrationFee,
                    IsAdministrationFee = x.IsAdministrationFee,
                    IsConsultationFee = x.IsConsultationFee,
                    IsRoomCharge = x.IsRoomCharge,
                    IsProcedure = x.IsProcedure,
                    IsLaboratory = x.IsLaboratory,
                    IsRadiology = x.IsRadiology,
                    IsPharmacy = x.IsPharmacy,
                    IsSurgery = x.IsSurgery,
                    IsPackage = x.IsPackage,
                    IsCoveredByInsuranceDefault = x.IsCoveredByInsuranceDefault,
                    SortOrder = x.SortOrder,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Tariff category tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<TariffCategoryDetailResponse>.Ok(
                data,
                "Detail tariff category berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<TariffCategoryCreateResponse>), StatusCodes.Status200OK)]
        [AccessAction("Create", "Create Tariff Category", Description = "Membuat data tariff category", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("TariffCategory", "Create")]
        public async Task<IActionResult> CreateTariffCategory([FromBody] CreateTariffCategoryRequest request)
        {
            var validation = await ValidateRequestAsync(
                excludeId: null,
                tariffCategoryCode: request.TariffCategoryCode,
                tariffCategoryName: request.TariffCategoryName
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data tariff category tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new MstTariffCategory
            {
                Id = Guid.NewGuid(),
                TariffCategoryCode = request.TariffCategoryCode.Trim().ToUpperInvariant(),
                TariffCategoryName = request.TariffCategoryName.Trim(),
                TariffGroupName = NormalizeNullableText(request.TariffGroupName),
                IsRegistrationFee = request.IsRegistrationFee,
                IsAdministrationFee = request.IsAdministrationFee,
                IsConsultationFee = request.IsConsultationFee,
                IsRoomCharge = request.IsRoomCharge,
                IsProcedure = request.IsProcedure,
                IsLaboratory = request.IsLaboratory,
                IsRadiology = request.IsRadiology,
                IsPharmacy = request.IsPharmacy,
                IsSurgery = request.IsSurgery,
                IsPackage = request.IsPackage,
                IsCoveredByInsuranceDefault = request.IsCoveredByInsuranceDefault,
                SortOrder = request.SortOrder,
                Description = NormalizeNullableText(request.Description),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstTariffCategory>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var response = new TariffCategoryCreateResponse
            {
                Id = entity.Id,
                TariffCategoryCode = entity.TariffCategoryCode,
                TariffCategoryName = entity.TariffCategoryName,
                TariffGroupName = entity.TariffGroupName,
                IsCoveredByInsuranceDefault = entity.IsCoveredByInsuranceDefault,
                IsActive = entity.IsActive
            };

            return Ok(ApiResponse<TariffCategoryCreateResponse>.Ok(
                response,
                "Tariff category berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Tariff Category", Description = "Mengubah data tariff category", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("TariffCategory", "Update")]
        public async Task<IActionResult> UpdateTariffCategory(Guid id, [FromBody] UpdateTariffCategoryRequest request)
        {
            var entity = await _dbContext.Set<MstTariffCategory>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Tariff category tidak ditemukan."
                ));
            }

            var validation = await ValidateRequestAsync(
                excludeId: id,
                tariffCategoryCode: request.TariffCategoryCode,
                tariffCategoryName: request.TariffCategoryName
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data tariff category tidak valid."
                ));
            }

            entity.TariffCategoryCode = request.TariffCategoryCode.Trim().ToUpperInvariant();
            entity.TariffCategoryName = request.TariffCategoryName.Trim();
            entity.TariffGroupName = NormalizeNullableText(request.TariffGroupName);
            entity.IsRegistrationFee = request.IsRegistrationFee;
            entity.IsAdministrationFee = request.IsAdministrationFee;
            entity.IsConsultationFee = request.IsConsultationFee;
            entity.IsRoomCharge = request.IsRoomCharge;
            entity.IsProcedure = request.IsProcedure;
            entity.IsLaboratory = request.IsLaboratory;
            entity.IsRadiology = request.IsRadiology;
            entity.IsPharmacy = request.IsPharmacy;
            entity.IsSurgery = request.IsSurgery;
            entity.IsPackage = request.IsPackage;
            entity.IsCoveredByInsuranceDefault = request.IsCoveredByInsuranceDefault;
            entity.SortOrder = request.SortOrder;
            entity.Description = NormalizeNullableText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Tariff category berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Tariff Category", Description = "Menghapus data tariff category", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("TariffCategory", "Delete")]
        public async Task<IActionResult> DeleteTariffCategory(Guid id)
        {
            var entity = await _dbContext.Set<MstTariffCategory>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Tariff category tidak ditemukan."
                ));
            }

            var isUsedByTariff = await _dbContext.Set<MstTariff>()
                .AnyAsync(x => x.TariffCategoryId == id && !x.IsDelete);

            if (isUsedByTariff)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Tariff category tidak dapat dihapus karena sudah digunakan oleh tariff."
                ));
            }

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Tariff category berhasil dihapus."
            ));
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            string tariffCategoryCode,
            string tariffCategoryName)
        {
            if (string.IsNullOrWhiteSpace(tariffCategoryCode))
                return (false, "Kode tariff category wajib diisi.");

            if (string.IsNullOrWhiteSpace(tariffCategoryName))
                return (false, "Nama tariff category wajib diisi.");

            var normalizedCode = tariffCategoryCode.Trim().ToUpperInvariant();
            var normalizedName = tariffCategoryName.Trim().ToLower();

            var duplicateCode = await _dbContext.Set<MstTariffCategory>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.TariffCategoryCode.ToUpper() == normalizedCode &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateCode)
                return (false, "Kode tariff category sudah digunakan.");

            var duplicateName = await _dbContext.Set<MstTariffCategory>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.TariffCategoryName.ToLower() == normalizedName &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateName)
                return (false, "Nama tariff category sudah digunakan.");

            return (true, null);
        }

        private static IQueryable<MstTariffCategory> ApplySorting(
            IQueryable<MstTariffCategory> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "sortOrder").ToLowerInvariant() switch
            {
                "createdatetime" => isDesc
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),

                "tariffcategorycode" => isDesc
                    ? query.OrderByDescending(x => x.TariffCategoryCode)
                    : query.OrderBy(x => x.TariffCategoryCode),

                "tariffcategoryname" => isDesc
                    ? query.OrderByDescending(x => x.TariffCategoryName)
                    : query.OrderBy(x => x.TariffCategoryName),

                "tariffgroupname" => isDesc
                    ? query.OrderByDescending(x => x.TariffGroupName)
                    : query.OrderBy(x => x.TariffGroupName),

                "iscoveredbyinsurancedefault" => isDesc
                    ? query.OrderByDescending(x => x.IsCoveredByInsuranceDefault)
                    : query.OrderBy(x => x.IsCoveredByInsuranceDefault),

                "isactive" => isDesc
                    ? query.OrderByDescending(x => x.IsActive)
                    : query.OrderBy(x => x.IsActive),

                _ => isDesc
                    ? query.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.TariffCategoryName)
                    : query.OrderBy(x => x.SortOrder).ThenBy(x => x.TariffCategoryName)
            };
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            return (pageNumber, pageSize);
        }

        private Guid GetCurrentUserId()
        {
            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userIdText, out var userId)
                ? userId
                : Guid.Empty;
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }
    }
}