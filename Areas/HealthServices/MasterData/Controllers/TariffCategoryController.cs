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
using System.Data;
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
        private const string TariffCategoryCodePrefix = "TC-RSMMC-";
        private const int TariffCategoryCodeDigitLength = 5;

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
                CustomPeriods = BuildCustomPeriodOptions(),
                SortOptions = new List<TariffCategorySortOptionResponse>
                {
                    new() { Value = "sortOrder", Label = "Urutan" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "tariffCategoryCode", Label = "Kode kategori tarif" },
                    new() { Value = "tariffCategoryName", Label = "Nama kategori tarif" },
                    new() { Value = "tariffGroupName", Label = "Grup tarif" },
                    new() { Value = "isRegistrationFee", Label = "Biaya registrasi" },
                    new() { Value = "isAdministrationFee", Label = "Biaya administrasi" },
                    new() { Value = "isConsultationFee", Label = "Biaya konsultasi" },
                    new() { Value = "isRoomCharge", Label = "Biaya kamar" },
                    new() { Value = "isProcedure", Label = "Procedure" },
                    new() { Value = "isLaboratory", Label = "Laboratorium" },
                    new() { Value = "isRadiology", Label = "Radiologi" },
                    new() { Value = "isPharmacy", Label = "Farmasi" },
                    new() { Value = "isSurgery", Label = "Operasi" },
                    new() { Value = "isPackage", Label = "Paket" },
                    new() { Value = "isCoveredByInsuranceDefault", Label = "Default ditanggung asuransi" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                QueryParameters = BuildQueryParameterInfo(),
                CreateFields = BuildCreateFieldMetadata(),
                UpdateFields = BuildUpdateFieldMetadata()
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
            var query = BuildBaseQuery();

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
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Read", "Read Tariff Category", Description = "Melihat data tariff category", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("TariffCategory", "Read")]
        public async Task<IActionResult> GetTariffCategories(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
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

            var dateRange = ResolveDateRange(startDate, endDate, customPeriod);
            if (!dateRange.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    dateRange.ErrorMessage ?? "Filter tanggal tidak valid."
                ));
            }

            var query = BuildBaseQuery();
            query = ApplyDateFilter(query, dateRange.Start, dateRange.EndExclusive);
            query = ApplyStandardFilter(
                query,
                search,
                isActive,
                isRegistrationFee,
                isAdministrationFee,
                isConsultationFee,
                isRoomCharge,
                isProcedure,
                isLaboratory,
                isRadiology,
                isPharmacy,
                isSurgery,
                isPackage,
                isCoveredByInsuranceDefault
            );

            var totalData = await query.CountAsync();
            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var actorNames = await GetActorNameMapAsync(entities.Select(x => x.CreateBy));

            var result = new ResponseTariffCategoryPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = entities.Select(x => MapResponse(x, actorNames)).ToList()
            };

            return Ok(ApiResponse<ResponseTariffCategoryPagedResult>.Ok(
                result,
                "Data tariff category berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<TariffCategoryOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Tariff Category", Description = "Melihat data pilihan tariff category", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("TariffCategory", "Read")]
        public async Task<IActionResult> GetTariffCategoryOptions(
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            [FromQuery] bool? isRegistrationFee = null,
            [FromQuery] bool? isAdministrationFee = null,
            [FromQuery] bool? isConsultationFee = null,
            [FromQuery] bool? isRoomCharge = null,
            [FromQuery] bool? isProcedure = null,
            [FromQuery] bool? isLaboratory = null,
            [FromQuery] bool? isRadiology = null,
            [FromQuery] bool? isPharmacy = null,
            [FromQuery] bool? isSurgery = null,
            [FromQuery] bool? isPackage = null,
            [FromQuery] bool? isCoveredByInsuranceDefault = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();
            query = ApplyStandardFilter(
                query,
                search,
                onlyActive ? true : null,
                isRegistrationFee,
                isAdministrationFee,
                isConsultationFee,
                isRoomCharge,
                isProcedure,
                isLaboratory,
                isRadiology,
                isPharmacy,
                isSurgery,
                isPackage,
                isCoveredByInsuranceDefault
            );

            var totalData = await query.CountAsync();
            var entities = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.TariffCategoryName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new TariffCategoryOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = entities.Select(MapOptionResponse).ToList()
            };

            return Ok(ApiResponse<TariffCategoryOptionPagedResponse>.Ok(
                result,
                "Data pilihan tariff category berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<TariffCategoryDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Tariff Category", Description = "Melihat detail tariff category", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("TariffCategory", "Read")]
        public async Task<IActionResult> GetTariffCategoryById(Guid id)
        {
            var entity = await BuildBaseQuery()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Tariff category tidak ditemukan."
                ));
            }

            var actorNames = await GetActorNameMapAsync(new[] { entity.CreateBy, entity.UpdateBy });
            var result = MapDetailResponse(entity, actorNames);

            return Ok(ApiResponse<TariffCategoryDetailResponse>.Ok(
                result,
                "Detail tariff category berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<TariffCategoryCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Tariff Category", Description = "Membuat data tariff category", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("TariffCategory", "Create")]
        public async Task<IActionResult> CreateTariffCategory([FromBody] CreateTariffCategoryRequest request)
        {
            var validation = await ValidateRequestAsync(null, request.TariffCategoryName);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data tariff category tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            var entity = new MstTariffCategory
            {
                Id = Guid.NewGuid(),
                TariffCategoryCode = await GenerateTariffCategoryCodeAsync(),
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
            await transaction.CommitAsync();

            var response = MapCreateUpdateResponse(entity);

            await _loggerService.InfoAsync(
                LogCategory,
                "TariffCategory.CreateTariffCategory",
                "Membuat data tariff category.",
                response
            );

            return Ok(ApiResponse<TariffCategoryCreateResponse>.Ok(
                response,
                "Tariff category berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<TariffCategoryUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
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

            var validation = await ValidateRequestAsync(id, request.TariffCategoryName);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data tariff category tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

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
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var response = new TariffCategoryUpdateResponse
            {
                Id = entity.Id,
                TariffCategoryCode = entity.TariffCategoryCode,
                TariffCategoryName = entity.TariffCategoryName,
                TariffGroupName = entity.TariffGroupName,
                IsCoveredByInsuranceDefault = entity.IsCoveredByInsuranceDefault,
                IsActive = entity.IsActive
            };

            return Ok(ApiResponse<TariffCategoryUpdateResponse>.Ok(
                response,
                "Tariff category berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<TariffCategoryUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Tariff Category Status", Description = "Mengubah status aktif tariff category", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("TariffCategory", "Update")]
        public async Task<IActionResult> UpdateTariffCategoryStatus(Guid id, [FromBody] UpdateTariffCategoryStatusRequest request)
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

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            var response = new TariffCategoryUpdateResponse
            {
                Id = entity.Id,
                TariffCategoryCode = entity.TariffCategoryCode,
                TariffCategoryName = entity.TariffCategoryName,
                TariffGroupName = entity.TariffGroupName,
                IsCoveredByInsuranceDefault = entity.IsCoveredByInsuranceDefault,
                IsActive = entity.IsActive
            };

            return Ok(ApiResponse<TariffCategoryUpdateResponse>.Ok(
                response,
                request.IsActive
                    ? "Tariff category berhasil diaktifkan."
                    : "Tariff category berhasil dinonaktifkan."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<TariffCategoryDeleteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Tariff Category", Description = "Menghapus data tariff category", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("TariffCategory", "Delete")]
        public async Task<IActionResult> DeleteTariffCategory(Guid id, [FromBody] DeleteTariffCategoryRequest? request = null)
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
                .AsNoTracking()
                .AnyAsync(x => x.TariffCategoryId == id && !x.IsDelete);

            if (isUsedByTariff)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Tariff category tidak dapat dihapus karena sudah digunakan oleh tariff."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            var deleteReason = NormalizeNullableText(request?.DeleteReason);
            if (!string.IsNullOrWhiteSpace(deleteReason))
            {
                entity.Description = string.IsNullOrWhiteSpace(entity.Description)
                    ? $"Delete reason: {deleteReason}"
                    : $"{entity.Description} | Delete reason: {deleteReason}";
            }

            await _dbContext.SaveChangesAsync();

            var response = new TariffCategoryDeleteResponse
            {
                Id = entity.Id,
                TariffCategoryCode = entity.TariffCategoryCode,
                TariffCategoryName = entity.TariffCategoryName,
                IsDelete = entity.IsDelete,
                DeleteDateTime = entity.DeleteDateTime
            };

            return Ok(ApiResponse<TariffCategoryDeleteResponse>.Ok(
                response,
                "Tariff category berhasil dihapus."
            ));
        }

        private IQueryable<MstTariffCategory> BuildBaseQuery()
        {
            return _dbContext.Set<MstTariffCategory>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstTariffCategory> ApplyDateFilter(
            IQueryable<MstTariffCategory> query,
            DateTime? start,
            DateTime? endExclusive)
        {
            if (start.HasValue)
                query = query.Where(x => x.CreateDateTime >= start.Value);

            if (endExclusive.HasValue)
                query = query.Where(x => x.CreateDateTime < endExclusive.Value);

            return query;
        }

        private static IQueryable<MstTariffCategory> ApplyStandardFilter(
            IQueryable<MstTariffCategory> query,
            string? search,
            bool? isActive,
            bool? isRegistrationFee,
            bool? isAdministrationFee,
            bool? isConsultationFee,
            bool? isRoomCharge,
            bool? isProcedure,
            bool? isLaboratory,
            bool? isRadiology,
            bool? isPharmacy,
            bool? isSurgery,
            bool? isPackage,
            bool? isCoveredByInsuranceDefault)
        {
            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

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
                    (x.TariffGroupName != null && x.TariffGroupName.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            return query;
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            string tariffCategoryName)
        {
            if (string.IsNullOrWhiteSpace(tariffCategoryName))
                return (false, "Nama tariff category wajib diisi.");

            var normalizedName = tariffCategoryName.Trim().ToLower();

            var duplicateNameQuery = _dbContext.Set<MstTariffCategory>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.TariffCategoryName.ToLower() == normalizedName);

            if (excludeId.HasValue)
                duplicateNameQuery = duplicateNameQuery.Where(x => x.Id != excludeId.Value);

            if (await duplicateNameQuery.AnyAsync())
                return (false, "Nama tariff category sudah digunakan.");

            return (true, null);
        }

        private async Task<string> GenerateTariffCategoryCodeAsync()
        {
            var existingCodes = await _dbContext.Set<MstTariffCategory>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.TariffCategoryCode.StartsWith(TariffCategoryCodePrefix))
                .Select(x => x.TariffCategoryCode)
                .ToListAsync();

            var usedNumbers = existingCodes
                .Select(ExtractCodeNumber)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .ToHashSet();

            var nextNumber = 1;
            while (usedNumbers.Contains(nextNumber))
            {
                nextNumber++;
            }

            return $"{TariffCategoryCodePrefix}{nextNumber.ToString().PadLeft(TariffCategoryCodeDigitLength, '0')}";
        }

        private static int? ExtractCodeNumber(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return null;

            if (!code.StartsWith(TariffCategoryCodePrefix, StringComparison.OrdinalIgnoreCase))
                return null;

            var numberText = code[TariffCategoryCodePrefix.Length..];
            return int.TryParse(numberText, out var number) ? number : null;
        }

        private static IQueryable<MstTariffCategory> ApplySorting(
            IQueryable<MstTariffCategory> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "sortOrder").ToLowerInvariant() switch
            {
                "createdatetime" => isDesc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "tariffcategorycode" => isDesc ? query.OrderByDescending(x => x.TariffCategoryCode) : query.OrderBy(x => x.TariffCategoryCode),
                "tariffcategoryname" => isDesc ? query.OrderByDescending(x => x.TariffCategoryName) : query.OrderBy(x => x.TariffCategoryName),
                "tariffgroupname" => isDesc ? query.OrderByDescending(x => x.TariffGroupName) : query.OrderBy(x => x.TariffGroupName),
                "isregistrationfee" => isDesc ? query.OrderByDescending(x => x.IsRegistrationFee) : query.OrderBy(x => x.IsRegistrationFee),
                "isadministrationfee" => isDesc ? query.OrderByDescending(x => x.IsAdministrationFee) : query.OrderBy(x => x.IsAdministrationFee),
                "isconsultationfee" => isDesc ? query.OrderByDescending(x => x.IsConsultationFee) : query.OrderBy(x => x.IsConsultationFee),
                "isroomcharge" => isDesc ? query.OrderByDescending(x => x.IsRoomCharge) : query.OrderBy(x => x.IsRoomCharge),
                "isprocedure" => isDesc ? query.OrderByDescending(x => x.IsProcedure) : query.OrderBy(x => x.IsProcedure),
                "islaboratory" => isDesc ? query.OrderByDescending(x => x.IsLaboratory) : query.OrderBy(x => x.IsLaboratory),
                "isradiology" => isDesc ? query.OrderByDescending(x => x.IsRadiology) : query.OrderBy(x => x.IsRadiology),
                "ispharmacy" => isDesc ? query.OrderByDescending(x => x.IsPharmacy) : query.OrderBy(x => x.IsPharmacy),
                "issurgery" => isDesc ? query.OrderByDescending(x => x.IsSurgery) : query.OrderBy(x => x.IsSurgery),
                "ispackage" => isDesc ? query.OrderByDescending(x => x.IsPackage) : query.OrderBy(x => x.IsPackage),
                "iscoveredbyinsurancedefault" => isDesc ? query.OrderByDescending(x => x.IsCoveredByInsuranceDefault) : query.OrderBy(x => x.IsCoveredByInsuranceDefault),
                "isactive" => isDesc ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
                _ => isDesc
                    ? query.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.TariffCategoryName)
                    : query.OrderBy(x => x.SortOrder).ThenBy(x => x.TariffCategoryName)
            };
        }

        private static DateRangeResult ResolveDateRange(
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            if (!string.IsNullOrWhiteSpace(customPeriod) &&
                !string.Equals(customPeriod, "custom", StringComparison.OrdinalIgnoreCase))
            {
                var today = DateTime.UtcNow.Date;
                var period = customPeriod.Trim().ToLowerInvariant();

                return period switch
                {
                    "today" => DateRangeResult.Valid(today, today.AddDays(1)),
                    "yesterday" => DateRangeResult.Valid(today.AddDays(-1), today),
                    "last7days" => DateRangeResult.Valid(today.AddDays(-6), today.AddDays(1)),
                    "last30days" => DateRangeResult.Valid(today.AddDays(-29), today.AddDays(1)),
                    "thismonth" => DateRangeResult.Valid(new DateTime(today.Year, today.Month, 1), new DateTime(today.Year, today.Month, 1).AddMonths(1)),
                    "lastmonth" => DateRangeResult.Valid(new DateTime(today.Year, today.Month, 1).AddMonths(-1), new DateTime(today.Year, today.Month, 1)),
                    "thisyear" => DateRangeResult.Valid(new DateTime(today.Year, 1, 1), new DateTime(today.Year + 1, 1, 1)),
                    _ => DateRangeResult.Invalid("Custom period tidak dikenali.")
                };
            }

            var start = startDate?.Date;
            var endExclusive = endDate?.Date.AddDays(1);

            if (start.HasValue && endExclusive.HasValue && start.Value >= endExclusive.Value)
                return DateRangeResult.Invalid("StartDate tidak boleh lebih besar dari EndDate.");

            return DateRangeResult.Valid(start, endExclusive);
        }

        private static List<TariffCategoryCustomPeriodOptionResponse> BuildCustomPeriodOptions()
        {
            return new List<TariffCategoryCustomPeriodOptionResponse>
            {
                new() { Value = "today", Label = "Hari ini", Description = "Data yang dibuat hari ini.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "yesterday", Label = "Kemarin", Description = "Data yang dibuat kemarin.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last7days", Label = "7 hari terakhir", Description = "Data yang dibuat dalam 7 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last30days", Label = "30 hari terakhir", Description = "Data yang dibuat dalam 30 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "thisMonth", Label = "Bulan ini", Description = "Data yang dibuat pada bulan berjalan.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "lastMonth", Label = "Bulan lalu", Description = "Data yang dibuat pada bulan sebelumnya.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "thisYear", Label = "Tahun ini", Description = "Data yang dibuat pada tahun berjalan.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "custom", Label = "Custom", Description = "Gunakan startDate dan endDate.", UsesStartDate = true, UsesEndDate = true }
            };
        }

        private static List<TariffCategoryQueryParameterInfoResponse> BuildQueryParameterInfo()
        {
            return new List<TariffCategoryQueryParameterInfoResponse>
            {
                new() { Name = "startDate", Type = "date", Description = "Tanggal awal filter berdasarkan CreateDateTime.", Example = "2026-06-01" },
                new() { Name = "endDate", Type = "date", Description = "Tanggal akhir filter berdasarkan CreateDateTime.", Example = "2026-06-30" },
                new() { Name = "customPeriod", Type = "string", Description = "Preset periode. Jika bukan custom, startDate dan endDate diabaikan.", Example = "thisMonth" },
                new() { Name = "isActive", Type = "boolean", Description = "Filter status aktif.", Example = "true" },
                new() { Name = "isRegistrationFee", Type = "boolean", Description = "Filter kategori biaya registrasi.", Example = "true" },
                new() { Name = "isAdministrationFee", Type = "boolean", Description = "Filter kategori biaya administrasi.", Example = "true" },
                new() { Name = "isConsultationFee", Type = "boolean", Description = "Filter kategori biaya konsultasi.", Example = "true" },
                new() { Name = "isRoomCharge", Type = "boolean", Description = "Filter kategori biaya kamar.", Example = "true" },
                new() { Name = "isProcedure", Type = "boolean", Description = "Filter kategori procedure.", Example = "true" },
                new() { Name = "isLaboratory", Type = "boolean", Description = "Filter kategori laboratorium.", Example = "true" },
                new() { Name = "isRadiology", Type = "boolean", Description = "Filter kategori radiologi.", Example = "true" },
                new() { Name = "isPharmacy", Type = "boolean", Description = "Filter kategori farmasi.", Example = "true" },
                new() { Name = "isSurgery", Type = "boolean", Description = "Filter kategori operasi.", Example = "true" },
                new() { Name = "isPackage", Type = "boolean", Description = "Filter kategori paket.", Example = "true" },
                new() { Name = "isCoveredByInsuranceDefault", Type = "boolean", Description = "Filter default ditanggung asuransi.", Example = "true" },
                new() { Name = "search", Type = "string", Description = "Pencarian kode, nama, grup tarif, atau deskripsi.", Example = "konsultasi" },
                new() { Name = "sortBy", Type = "string", Description = "Kolom sorting.", Example = "sortOrder" },
                new() { Name = "sortDirection", Type = "string", Description = "asc atau desc.", Example = "asc" },
                new() { Name = "pageNumber", Type = "integer", Description = "Nomor halaman.", Example = "1" },
                new() { Name = "pageSize", Type = "integer", Description = "Jumlah data per halaman.", Example = "25" }
            };
        }

        private static List<TariffCategoryFormFieldMetadataResponse> BuildCreateFieldMetadata()
        {
            return BuildFieldMetadata(false);
        }

        private static List<TariffCategoryFormFieldMetadataResponse> BuildUpdateFieldMetadata()
        {
            return BuildFieldMetadata(true);
        }

        private static List<TariffCategoryFormFieldMetadataResponse> BuildFieldMetadata(bool isUpdate)
        {
            var fields = new List<TariffCategoryFormFieldMetadataResponse>
            {
                new() { Name = "tariffCategoryCode", Label = "Kode kategori tarif", Section = "Basic", InputType = "readonly", RequiredType = "AutoGenerated", MaxLength = 50, Description = "Dibuat otomatis oleh sistem dengan format TC-RSMMC-00001.", Example = "TC-RSMMC-00001", SortOrder = 1 },
                new() { Name = "tariffCategoryName", Label = "Nama kategori tarif", Section = "Basic", InputType = "text", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", MaxLength = 150, Example = "Konsultasi", SortOrder = 2 },
                new() { Name = "tariffGroupName", Label = "Grup tarif", Section = "Basic", InputType = "text", MaxLength = 100, Example = "Rawat Jalan", SortOrder = 3 },
                new() { Name = "isRegistrationFee", Label = "Biaya registrasi", Section = "Rule", InputType = "switch", SortOrder = 4 },
                new() { Name = "isAdministrationFee", Label = "Biaya administrasi", Section = "Rule", InputType = "switch", SortOrder = 5 },
                new() { Name = "isConsultationFee", Label = "Biaya konsultasi", Section = "Rule", InputType = "switch", SortOrder = 6 },
                new() { Name = "isRoomCharge", Label = "Biaya kamar", Section = "Rule", InputType = "switch", SortOrder = 7 },
                new() { Name = "isProcedure", Label = "Procedure", Section = "Rule", InputType = "switch", SortOrder = 8 },
                new() { Name = "isLaboratory", Label = "Laboratorium", Section = "Rule", InputType = "switch", SortOrder = 9 },
                new() { Name = "isRadiology", Label = "Radiologi", Section = "Rule", InputType = "switch", SortOrder = 10 },
                new() { Name = "isPharmacy", Label = "Farmasi", Section = "Rule", InputType = "switch", SortOrder = 11 },
                new() { Name = "isSurgery", Label = "Operasi", Section = "Rule", InputType = "switch", SortOrder = 12 },
                new() { Name = "isPackage", Label = "Paket", Section = "Rule", InputType = "switch", SortOrder = 13 },
                new() { Name = "isCoveredByInsuranceDefault", Label = "Default ditanggung asuransi", Section = "Rule", InputType = "switch", SortOrder = 14 },
                new() { Name = "sortOrder", Label = "Urutan", Section = "Display", InputType = "number", SortOrder = 15 },
                new() { Name = "description", Label = "Deskripsi", Section = "Additional", InputType = "textarea", MaxLength = 250, SortOrder = 16 }
            };

            if (isUpdate)
            {
                fields.Add(new TariffCategoryFormFieldMetadataResponse
                {
                    Name = "isActive",
                    Label = "Status aktif",
                    Section = "Status",
                    InputType = "switch",
                    SortOrder = 99
                });
            }

            return fields.OrderBy(x => x.SortOrder).ToList();
        }

        private async Task<Dictionary<Guid, string?>> GetActorNameMapAsync(IEnumerable<Guid> actorIds)
        {
            var ids = actorIds.Where(x => x != Guid.Empty).Distinct().ToList();
            if (!ids.Any())
                return new Dictionary<Guid, string?>();

            return await _dbContext.Users
                .AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    Name = x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode
                })
                .ToDictionaryAsync(x => x.Id, x => x.Name);
        }

        private static TariffCategoryResponse MapResponse(
            MstTariffCategory entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new TariffCategoryResponse
            {
                Id = entity.Id,
                TariffCategoryCode = entity.TariffCategoryCode,
                TariffCategoryName = entity.TariffCategoryName,
                TariffGroupName = entity.TariffGroupName,
                IsRegistrationFee = entity.IsRegistrationFee,
                IsAdministrationFee = entity.IsAdministrationFee,
                IsConsultationFee = entity.IsConsultationFee,
                IsRoomCharge = entity.IsRoomCharge,
                IsProcedure = entity.IsProcedure,
                IsLaboratory = entity.IsLaboratory,
                IsRadiology = entity.IsRadiology,
                IsPharmacy = entity.IsPharmacy,
                IsSurgery = entity.IsSurgery,
                IsPackage = entity.IsPackage,
                IsCoveredByInsuranceDefault = entity.IsCoveredByInsuranceDefault,
                SortOrder = entity.SortOrder,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy)
            };
        }

        private static TariffCategoryDetailResponse MapDetailResponse(
            MstTariffCategory entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new TariffCategoryDetailResponse
            {
                Id = entity.Id,
                TariffCategoryCode = entity.TariffCategoryCode,
                TariffCategoryName = entity.TariffCategoryName,
                TariffGroupName = entity.TariffGroupName,
                IsRegistrationFee = entity.IsRegistrationFee,
                IsAdministrationFee = entity.IsAdministrationFee,
                IsConsultationFee = entity.IsConsultationFee,
                IsRoomCharge = entity.IsRoomCharge,
                IsProcedure = entity.IsProcedure,
                IsLaboratory = entity.IsLaboratory,
                IsRadiology = entity.IsRadiology,
                IsPharmacy = entity.IsPharmacy,
                IsSurgery = entity.IsSurgery,
                IsPackage = entity.IsPackage,
                IsCoveredByInsuranceDefault = entity.IsCoveredByInsuranceDefault,
                SortOrder = entity.SortOrder,
                Description = entity.Description,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy),
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };
        }

        private static TariffCategoryOptionResponse MapOptionResponse(MstTariffCategory entity)
        {
            return new TariffCategoryOptionResponse
            {
                Id = entity.Id,
                TariffCategoryCode = entity.TariffCategoryCode,
                TariffCategoryName = entity.TariffCategoryName,
                TariffGroupName = entity.TariffGroupName,
                IsRegistrationFee = entity.IsRegistrationFee,
                IsAdministrationFee = entity.IsAdministrationFee,
                IsConsultationFee = entity.IsConsultationFee,
                IsRoomCharge = entity.IsRoomCharge,
                IsProcedure = entity.IsProcedure,
                IsLaboratory = entity.IsLaboratory,
                IsRadiology = entity.IsRadiology,
                IsPharmacy = entity.IsPharmacy,
                IsSurgery = entity.IsSurgery,
                IsPackage = entity.IsPackage,
                IsCoveredByInsuranceDefault = entity.IsCoveredByInsuranceDefault,
                SortOrder = entity.SortOrder
            };
        }

        private static TariffCategoryCreateResponse MapCreateUpdateResponse(MstTariffCategory entity)
        {
            return new TariffCategoryCreateResponse
            {
                Id = entity.Id,
                TariffCategoryCode = entity.TariffCategoryCode,
                TariffCategoryName = entity.TariffCategoryName,
                TariffGroupName = entity.TariffGroupName,
                IsCoveredByInsuranceDefault = entity.IsCoveredByInsuranceDefault,
                IsActive = entity.IsActive
            };
        }

        private static string? GetActorName(
            IReadOnlyDictionary<Guid, string?> actorNames,
            Guid actorId)
        {
            if (actorId == Guid.Empty)
                return null;

            return actorNames.TryGetValue(actorId, out var name)
                ? name
                : null;
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
            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("user_id");

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

        private sealed class DateRangeResult
        {
            public bool IsValid { get; private init; }
            public DateTime? Start { get; private init; }
            public DateTime? EndExclusive { get; private init; }
            public string? ErrorMessage { get; private init; }

            public static DateRangeResult Valid(DateTime? start, DateTime? endExclusive)
            {
                return new DateRangeResult
                {
                    IsValid = true,
                    Start = start,
                    EndExclusive = endExclusive
                };
            }

            public static DateRangeResult Invalid(string errorMessage)
            {
                return new DateRangeResult
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
        }
    }
}
