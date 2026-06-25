using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Data;
using System.Security.Claims;

using ResponseDrugPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs.DrugResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/master-data/drugs")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MASTER_DATA",
        moduleName: "Health Service Master Data",
        displayName: "Drug",
        AreaName = "HealthServices",
        ControllerName = "Drug",
        Description = "Health service master data drug",
        SortOrder = 11
    )]
    [Tags("Health Services / Master Data / Drug")]
    public class DrugController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MasterData";
        private const string CodePrefix = "DRG-RSMMC-";
        private const int CodeNumberLength = 5;

        private static readonly HashSet<string> DrugFormOptions = new(StringComparer.OrdinalIgnoreCase)
        {
            "Tablet",
            "Capsule",
            "Syrup",
            "Injection",
            "Cream",
            "Drop",
            "Inhaler",
            "Other"
        };

        private static readonly HashSet<string> RouteOptions = new(StringComparer.OrdinalIgnoreCase)
        {
            "Oral",
            "IV",
            "IM",
            "SC",
            "Topical",
            "Inhalation",
            "Other"
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public DrugController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<DrugFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Drug", Description = "Melihat metadata filter drug", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Drug", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new DrugFilterMetadataResponse
            {
                DefaultFilter = new DrugDefaultFilterResponse(),
                CustomPeriods = new List<DrugCustomPeriodOptionResponse>
                {
                    new() { Value = "today", Label = "Hari ini" },
                    new() { Value = "last7days", Label = "7 hari terakhir" },
                    new() { Value = "thismonth", Label = "Bulan ini" },
                    new() { Value = "lastmonth", Label = "Bulan lalu" }
                },
                SortOptions = BuildSortOptions(),
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                DrugFormOptions = DrugFormOptions
                    .OrderBy(x => x)
                    .Select(x => new DrugStringOptionResponse { Value = x, Label = SplitPascalCase(x) })
                    .ToList(),
                RouteOptions = RouteOptions
                    .OrderBy(x => x)
                    .Select(x => new DrugStringOptionResponse { Value = x, Label = BuildRouteLabel(x) })
                    .ToList(),
                ResetButtonLabel = "Reset"
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "Drug.GetFilterMetadata",
                "Mengambil metadata filter drug.",
                result
            );

            return Ok(ApiResponse<DrugFilterMetadataResponse>.Ok(
                result,
                "Metadata filter drug berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<DrugSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Drug", Description = "Melihat ringkasan drug", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Drug", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = _dbContext.Set<MstDrug>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            var result = new DrugSummaryResponse
            {
                TotalDrug = await query.CountAsync(),
                ActiveDrug = await query.CountAsync(x => x.IsActive),
                InactiveDrug = await query.CountAsync(x => !x.IsActive),
                FormularyDrug = await query.CountAsync(x => x.IsFormulary),
                GenericDrug = await query.CountAsync(x => x.IsGeneric),
                AntibioticDrug = await query.CountAsync(x => x.IsAntibiotic),
                NarcoticDrug = await query.CountAsync(x => x.IsNarcotic),
                PsychotropicDrug = await query.CountAsync(x => x.IsPsychotropic),
                HighAlertDrug = await query.CountAsync(x => x.IsHighAlert),
                ChronicDiseaseDrug = await query.CountAsync(x => x.IsChronicDiseaseDrug),
                VaccineDrug = await query.CountAsync(x => x.IsVaccine),
                ConsumableDrug = await query.CountAsync(x => x.IsConsumable),
                CompoundIngredientAllowedDrug = await query.CountAsync(x => x.IsCompoundIngredientAllowed),
                StockManagedDrug = await query.CountAsync(x => x.IsStockManaged),
                BatchTrackedDrug = await query.CountAsync(x => x.IsBatchTracked),
                ExpiryDateTrackedDrug = await query.CountAsync(x => x.IsExpiryDateTracked),
                FractionalDispenseAllowedDrug = await query.CountAsync(x => x.IsAllowFractionalDispense),
                NeedPrescriptionDrug = await query.CountAsync(x => x.IsNeedPrescription),
                CoveredByInsuranceDefaultDrug = await query.CountAsync(x => x.IsCoveredByInsuranceDefault),
                NeedApprovalDrug = await query.CountAsync(x => x.IsNeedApproval)
            };

            return Ok(ApiResponse<DrugSummaryResponse>.Ok(
                result,
                "Ringkasan drug berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseDrugPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Drug", Description = "Melihat data drug", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Drug", "Read")]
        public async Task<IActionResult> GetDrugs(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? drugCategoryId,
            [FromQuery] Guid? baseUnitMeasurementId,
            [FromQuery] Guid? strengthMeasurementId,
            [FromQuery] string? drugForm,
            [FromQuery] string? route,
            [FromQuery] bool? isActive,
            [FromQuery] bool? isFormulary,
            [FromQuery] bool? isGeneric,
            [FromQuery] bool? isAntibiotic,
            [FromQuery] bool? isNarcotic,
            [FromQuery] bool? isPsychotropic,
            [FromQuery] bool? isHighAlert,
            [FromQuery] bool? isChronicDiseaseDrug,
            [FromQuery] bool? isVaccine,
            [FromQuery] bool? isConsumable,
            [FromQuery] bool? isCompoundIngredientAllowed,
            [FromQuery] bool? isStockManaged,
            [FromQuery] bool? isBatchTracked,
            [FromQuery] bool? isExpiryDateTracked,
            [FromQuery] bool? isAllowFractionalDispense,
            [FromQuery] bool? isNeedPrescription,
            [FromQuery] bool? isCoveredByInsuranceDefault,
            [FromQuery] bool? isNeedApproval,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "sortOrder",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();

            query = ApplyDateFilter(query, startDate, endDate, customPeriod);
            query = ApplyStandardFilter(
                query,
                drugCategoryId,
                baseUnitMeasurementId,
                strengthMeasurementId,
                drugForm,
                route,
                isActive,
                isFormulary,
                isGeneric,
                isAntibiotic,
                isNarcotic,
                isPsychotropic,
                isHighAlert,
                isChronicDiseaseDrug,
                isVaccine,
                isConsumable,
                isCompoundIngredientAllowed,
                isStockManaged,
                isBatchTracked,
                isExpiryDateTracked,
                isAllowFractionalDispense,
                isNeedPrescription,
                isCoveredByInsuranceDefault,
                isNeedApproval,
                search
            );

            var totalData = await query.CountAsync();

            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var actorNames = await GetActorNameMapAsync(
                entities
                    .Select(x => x.CreateBy)
                    .Where(x => x != Guid.Empty)
            );

            var items = entities
                .Select(x => MapResponse(x, actorNames))
                .ToList();

            var result = new ResponseDrugPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseDrugPagedResult>.Ok(
                result,
                "Data drug berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<DrugOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Drug", Description = "Melihat data pilihan drug", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Drug", "Read")]
        public async Task<IActionResult> GetDrugOptions(
            [FromQuery] Guid? drugCategoryId,
            [FromQuery] Guid? baseUnitMeasurementId,
            [FromQuery] string? drugForm = null,
            [FromQuery] string? route = null,
            [FromQuery] bool onlyActive = true,
            [FromQuery] bool? isFormulary = null,
            [FromQuery] bool? isGeneric = null,
            [FromQuery] bool? isAntibiotic = null,
            [FromQuery] bool? isHighAlert = null,
            [FromQuery] bool? isCompoundIngredientAllowed = null,
            [FromQuery] bool? isNeedPrescription = null,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();

            query = ApplyStandardFilter(
                query,
                drugCategoryId,
                baseUnitMeasurementId,
                null,
                drugForm,
                route,
                onlyActive ? true : null,
                isFormulary,
                isGeneric,
                isAntibiotic,
                null,
                null,
                isHighAlert,
                null,
                null,
                null,
                isCompoundIngredientAllowed,
                null,
                null,
                null,
                null,
                isNeedPrescription,
                null,
                null,
                search
            );

            var totalData = await query.CountAsync();

            var entities = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.DrugName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = entities.Select(MapOptionResponse).ToList();

            var result = new DrugOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<DrugOptionPagedResponse>.Ok(
                result,
                "Data pilihan drug berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DrugDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Drug", Description = "Melihat detail drug", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Drug", "Read")]
        public async Task<IActionResult> GetDrugById(Guid id)
        {
            var entity = await BuildBaseQuery()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Drug tidak ditemukan."
                ));
            }

            var actorNames = await GetActorNameMapAsync(new[]
            {
                entity.CreateBy,
                entity.UpdateBy
            });

            var data = MapDetailResponse(entity, actorNames);

            return Ok(ApiResponse<DrugDetailResponse>.Ok(
                data,
                "Detail drug berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<DrugCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Drug", Description = "Membuat data drug", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("Drug", "Create")]
        public async Task<IActionResult> CreateDrug([FromBody] CreateDrugRequest request)
        {
            var validation = await ValidateRequestAsync(null, request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data drug tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            var entity = new MstDrug
            {
                Id = Guid.NewGuid(),
                DrugCategoryId = request.DrugCategoryId,
                DrugCode = await GenerateDrugCodeAsync(),
                DrugName = request.DrugName.Trim(),
                GenericName = NormalizeNullableString(request.GenericName),
                BrandName = NormalizeNullableString(request.BrandName),
                ManufacturerName = NormalizeNullableString(request.ManufacturerName),
                DrugForm = NormalizeDrugForm(request.DrugForm),
                Strength = NormalizeNullableString(request.Strength),
                StrengthValue = request.StrengthValue,
                StrengthMeasurementId = NormalizeNullableGuid(request.StrengthMeasurementId),
                BaseUnit = NormalizeNullableString(request.BaseUnit),
                DispenseUnit = NormalizeNullableString(request.DispenseUnit),
                BaseUnitMeasurementId = NormalizeNullableGuid(request.BaseUnitMeasurementId),
                DispenseUnitMeasurementId = NormalizeNullableGuid(request.DispenseUnitMeasurementId),
                PurchaseUnitMeasurementId = NormalizeNullableGuid(request.PurchaseUnitMeasurementId),
                StockUnitMeasurementId = NormalizeNullableGuid(request.StockUnitMeasurementId),
                DefaultDoseUnitMeasurementId = NormalizeNullableGuid(request.DefaultDoseUnitMeasurementId),
                Route = NormalizeRoute(request.Route),
                IsFormulary = request.IsFormulary,
                IsGeneric = request.IsGeneric,
                IsAntibiotic = request.IsAntibiotic,
                IsNarcotic = request.IsNarcotic,
                IsPsychotropic = request.IsPsychotropic,
                IsHighAlert = request.IsHighAlert,
                IsChronicDiseaseDrug = request.IsChronicDiseaseDrug,
                IsVaccine = request.IsVaccine,
                IsConsumable = request.IsConsumable,
                IsCompoundIngredientAllowed = request.IsCompoundIngredientAllowed,
                IsStockManaged = request.IsStockManaged,
                IsBatchTracked = request.IsBatchTracked,
                IsExpiryDateTracked = request.IsExpiryDateTracked,
                IsAllowFractionalDispense = request.IsAllowFractionalDispense,
                IsNeedPrescription = request.IsNeedPrescription,
                IsCoveredByInsuranceDefault = request.IsCoveredByInsuranceDefault,
                IsNeedApproval = request.IsNeedApproval,
                DefaultPrice = request.DefaultPrice,
                InsurancePrice = request.InsurancePrice,
                MemberPrice = request.MemberPrice,
                CompanyPrice = request.CompanyPrice,
                Indication = NormalizeNullableString(request.Indication),
                Contraindication = NormalizeNullableString(request.Contraindication),
                SideEffect = NormalizeNullableString(request.SideEffect),
                WarningPrecaution = NormalizeNullableString(request.WarningPrecaution),
                DosageInformation = NormalizeNullableString(request.DosageInformation),
                DrugInteraction = NormalizeNullableString(request.DrugInteraction),
                AdministrationInstruction = NormalizeNullableString(request.AdministrationInstruction),
                StorageInstruction = NormalizeNullableString(request.StorageInstruction),
                PregnancyCategory = NormalizeNullableString(request.PregnancyCategory),
                LactationNote = NormalizeNullableString(request.LactationNote),
                PediatricNote = NormalizeNullableString(request.PediatricNote),
                GeriatricNote = NormalizeNullableString(request.GeriatricNote),
                ExternalDrugCode = NormalizeNullableString(request.ExternalDrugCode),
                IntegrationCode = NormalizeNullableString(request.IntegrationCode),
                BpomRegistrationNumber = NormalizeNullableString(request.BpomRegistrationNumber),
                NationalDrugCode = NormalizeNullableString(request.NationalDrugCode),
                SortOrder = request.SortOrder,
                Description = NormalizeNullableString(request.Description),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstDrug>().Add(entity);
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            var result = ToCreateResponse(entity);

            await _loggerService.InfoAsync(
                LogCategory,
                "Drug.CreateDrug",
                "Membuat data drug.",
                result
            );

            return Ok(ApiResponse<DrugCreateResponse>.Ok(
                result,
                "Drug berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DrugUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Drug", Description = "Mengubah data drug", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("Drug", "Update")]
        public async Task<IActionResult> UpdateDrug(Guid id, [FromBody] UpdateDrugRequest request)
        {
            var entity = await _dbContext.Set<MstDrug>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Drug tidak ditemukan."
                ));
            }

            var validation = await ValidateRequestAsync(id, request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data drug tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.DrugCategoryId = request.DrugCategoryId;
            entity.DrugName = request.DrugName.Trim();
            entity.GenericName = NormalizeNullableString(request.GenericName);
            entity.BrandName = NormalizeNullableString(request.BrandName);
            entity.ManufacturerName = NormalizeNullableString(request.ManufacturerName);
            entity.DrugForm = NormalizeDrugForm(request.DrugForm);
            entity.Strength = NormalizeNullableString(request.Strength);
            entity.StrengthValue = request.StrengthValue;
            entity.StrengthMeasurementId = NormalizeNullableGuid(request.StrengthMeasurementId);
            entity.BaseUnit = NormalizeNullableString(request.BaseUnit);
            entity.DispenseUnit = NormalizeNullableString(request.DispenseUnit);
            entity.BaseUnitMeasurementId = NormalizeNullableGuid(request.BaseUnitMeasurementId);
            entity.DispenseUnitMeasurementId = NormalizeNullableGuid(request.DispenseUnitMeasurementId);
            entity.PurchaseUnitMeasurementId = NormalizeNullableGuid(request.PurchaseUnitMeasurementId);
            entity.StockUnitMeasurementId = NormalizeNullableGuid(request.StockUnitMeasurementId);
            entity.DefaultDoseUnitMeasurementId = NormalizeNullableGuid(request.DefaultDoseUnitMeasurementId);
            entity.Route = NormalizeRoute(request.Route);
            entity.IsFormulary = request.IsFormulary;
            entity.IsGeneric = request.IsGeneric;
            entity.IsAntibiotic = request.IsAntibiotic;
            entity.IsNarcotic = request.IsNarcotic;
            entity.IsPsychotropic = request.IsPsychotropic;
            entity.IsHighAlert = request.IsHighAlert;
            entity.IsChronicDiseaseDrug = request.IsChronicDiseaseDrug;
            entity.IsVaccine = request.IsVaccine;
            entity.IsConsumable = request.IsConsumable;
            entity.IsCompoundIngredientAllowed = request.IsCompoundIngredientAllowed;
            entity.IsStockManaged = request.IsStockManaged;
            entity.IsBatchTracked = request.IsBatchTracked;
            entity.IsExpiryDateTracked = request.IsExpiryDateTracked;
            entity.IsAllowFractionalDispense = request.IsAllowFractionalDispense;
            entity.IsNeedPrescription = request.IsNeedPrescription;
            entity.IsCoveredByInsuranceDefault = request.IsCoveredByInsuranceDefault;
            entity.IsNeedApproval = request.IsNeedApproval;
            entity.DefaultPrice = request.DefaultPrice;
            entity.InsurancePrice = request.InsurancePrice;
            entity.MemberPrice = request.MemberPrice;
            entity.CompanyPrice = request.CompanyPrice;
            entity.Indication = NormalizeNullableString(request.Indication);
            entity.Contraindication = NormalizeNullableString(request.Contraindication);
            entity.SideEffect = NormalizeNullableString(request.SideEffect);
            entity.WarningPrecaution = NormalizeNullableString(request.WarningPrecaution);
            entity.DosageInformation = NormalizeNullableString(request.DosageInformation);
            entity.DrugInteraction = NormalizeNullableString(request.DrugInteraction);
            entity.AdministrationInstruction = NormalizeNullableString(request.AdministrationInstruction);
            entity.StorageInstruction = NormalizeNullableString(request.StorageInstruction);
            entity.PregnancyCategory = NormalizeNullableString(request.PregnancyCategory);
            entity.LactationNote = NormalizeNullableString(request.LactationNote);
            entity.PediatricNote = NormalizeNullableString(request.PediatricNote);
            entity.GeriatricNote = NormalizeNullableString(request.GeriatricNote);
            entity.ExternalDrugCode = NormalizeNullableString(request.ExternalDrugCode);
            entity.IntegrationCode = NormalizeNullableString(request.IntegrationCode);
            entity.BpomRegistrationNumber = NormalizeNullableString(request.BpomRegistrationNumber);
            entity.NationalDrugCode = NormalizeNullableString(request.NationalDrugCode);
            entity.SortOrder = request.SortOrder;
            entity.Description = NormalizeNullableString(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var result = ToUpdateResponse(entity);

            await _loggerService.InfoAsync(
                LogCategory,
                "Drug.UpdateDrug",
                "Mengubah data drug.",
                result
            );

            return Ok(ApiResponse<DrugUpdateResponse>.Ok(
                result,
                "Drug berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<DrugUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Drug Status", Description = "Mengubah status drug", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("Drug", "Update")]
        public async Task<IActionResult> UpdateDrugStatus(
            Guid id,
            [FromBody] UpdateDrugStatusRequest request)
        {
            var entity = await _dbContext.Set<MstDrug>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Drug tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var result = ToUpdateResponse(entity);

            return Ok(ApiResponse<DrugUpdateResponse>.Ok(
                result,
                "Status drug berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DrugDeleteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Drug", Description = "Menghapus data drug", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("Drug", "Delete")]
        public async Task<IActionResult> DeleteDrug(
            Guid id,
            [FromBody] DeleteDrugRequest? request = null)
        {
            var entity = await _dbContext.Set<MstDrug>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Drug tidak ditemukan."
                ));
            }

            var usedByTariff = await _dbContext.Set<MstTariff>()
                .AsNoTracking()
                .AnyAsync(x => x.DrugId == id && !x.IsDelete);

            var usedByDrugUnitConversion = await _dbContext.Set<MstDrugUnitConversion>()
                .AsNoTracking()
                .AnyAsync(x => x.DrugId == id && !x.IsDelete);

            var usedInCoverageRule = await _dbContext.Set<MstInsuranceCoverageRule>()
                .AsNoTracking()
                .AnyAsync(x => x.DrugId == id && !x.IsDelete);

            if (usedByTariff || usedByDrugUnitConversion || usedInCoverageRule)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Drug tidak dapat dihapus karena sudah digunakan oleh tariff, drug unit conversion, atau insurance coverage rule."
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

            if (!string.IsNullOrWhiteSpace(request?.DeleteReason))
            {
                entity.Description = request.DeleteReason.Trim();
            }

            await _dbContext.SaveChangesAsync();

            var result = new DrugDeleteResponse
            {
                Id = entity.Id,
                DrugCode = entity.DrugCode,
                DrugName = entity.DrugName,
                DeleteDateTime = entity.DeleteDateTime
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "Drug.DeleteDrug",
                "Menghapus data drug.",
                result
            );

            return Ok(ApiResponse<DrugDeleteResponse>.Ok(
                result,
                "Drug berhasil dihapus."
            ));
        }

        private IQueryable<MstDrug> BuildBaseQuery()
        {
            return _dbContext.Set<MstDrug>()
                .AsNoTracking()
                .Include(x => x.DrugCategory)
                .Include(x => x.StrengthMeasurement)
                .Include(x => x.BaseUnitMeasurement)
                .Include(x => x.DispenseUnitMeasurement)
                .Include(x => x.PurchaseUnitMeasurement)
                .Include(x => x.StockUnitMeasurement)
                .Include(x => x.DefaultDoseUnitMeasurement)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstDrug> ApplyDateFilter(
            IQueryable<MstDrug> query,
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            if (startDate.HasValue)
            {
                var start = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
                query = query.Where(x => x.CreateDateTime >= start);
            }

            if (endDate.HasValue)
            {
                var end = DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc);
                query = query.Where(x => x.CreateDateTime < end);
            }

            if (!startDate.HasValue && !endDate.HasValue && !string.IsNullOrWhiteSpace(customPeriod))
            {
                var today = AppDateTimeHelper.OperationalDate();

                switch (customPeriod.Trim().ToLowerInvariant())
                {
                    case "today":
                        query = query.Where(x => x.CreateDateTime >= today && x.CreateDateTime < today.AddDays(1));
                        break;

                    case "last7days":
                        query = query.Where(x => x.CreateDateTime >= today.AddDays(-6) && x.CreateDateTime < today.AddDays(1));
                        break;

                    case "thismonth":
                        var thisMonthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                        query = query.Where(x => x.CreateDateTime >= thisMonthStart && x.CreateDateTime < thisMonthStart.AddMonths(1));
                        break;

                    case "lastmonth":
                        var currentMonthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                        var lastMonthStart = currentMonthStart.AddMonths(-1);
                        query = query.Where(x => x.CreateDateTime >= lastMonthStart && x.CreateDateTime < currentMonthStart);
                        break;
                }
            }

            return query;
        }

        private static IQueryable<MstDrug> ApplyStandardFilter(
            IQueryable<MstDrug> query,
            Guid? drugCategoryId,
            Guid? baseUnitMeasurementId,
            Guid? strengthMeasurementId,
            string? drugForm,
            string? route,
            bool? isActive,
            bool? isFormulary,
            bool? isGeneric,
            bool? isAntibiotic,
            bool? isNarcotic,
            bool? isPsychotropic,
            bool? isHighAlert,
            bool? isChronicDiseaseDrug,
            bool? isVaccine,
            bool? isConsumable,
            bool? isCompoundIngredientAllowed,
            bool? isStockManaged,
            bool? isBatchTracked,
            bool? isExpiryDateTracked,
            bool? isAllowFractionalDispense,
            bool? isNeedPrescription,
            bool? isCoveredByInsuranceDefault,
            bool? isNeedApproval,
            string? search)
        {
            if (drugCategoryId.HasValue && drugCategoryId.Value != Guid.Empty)
                query = query.Where(x => x.DrugCategoryId == drugCategoryId.Value);

            if (baseUnitMeasurementId.HasValue && baseUnitMeasurementId.Value != Guid.Empty)
                query = query.Where(x => x.BaseUnitMeasurementId == baseUnitMeasurementId.Value);

            if (strengthMeasurementId.HasValue && strengthMeasurementId.Value != Guid.Empty)
                query = query.Where(x => x.StrengthMeasurementId == strengthMeasurementId.Value);

            if (!string.IsNullOrWhiteSpace(drugForm))
            {
                var normalizedDrugForm = NormalizeDrugForm(drugForm);
                query = query.Where(x => x.DrugForm == normalizedDrugForm);
            }

            if (!string.IsNullOrWhiteSpace(route))
            {
                var normalizedRoute = NormalizeRoute(route);
                query = query.Where(x => x.Route == normalizedRoute);
            }

            if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);
            if (isFormulary.HasValue) query = query.Where(x => x.IsFormulary == isFormulary.Value);
            if (isGeneric.HasValue) query = query.Where(x => x.IsGeneric == isGeneric.Value);
            if (isAntibiotic.HasValue) query = query.Where(x => x.IsAntibiotic == isAntibiotic.Value);
            if (isNarcotic.HasValue) query = query.Where(x => x.IsNarcotic == isNarcotic.Value);
            if (isPsychotropic.HasValue) query = query.Where(x => x.IsPsychotropic == isPsychotropic.Value);
            if (isHighAlert.HasValue) query = query.Where(x => x.IsHighAlert == isHighAlert.Value);
            if (isChronicDiseaseDrug.HasValue) query = query.Where(x => x.IsChronicDiseaseDrug == isChronicDiseaseDrug.Value);
            if (isVaccine.HasValue) query = query.Where(x => x.IsVaccine == isVaccine.Value);
            if (isConsumable.HasValue) query = query.Where(x => x.IsConsumable == isConsumable.Value);
            if (isCompoundIngredientAllowed.HasValue) query = query.Where(x => x.IsCompoundIngredientAllowed == isCompoundIngredientAllowed.Value);
            if (isStockManaged.HasValue) query = query.Where(x => x.IsStockManaged == isStockManaged.Value);
            if (isBatchTracked.HasValue) query = query.Where(x => x.IsBatchTracked == isBatchTracked.Value);
            if (isExpiryDateTracked.HasValue) query = query.Where(x => x.IsExpiryDateTracked == isExpiryDateTracked.Value);
            if (isAllowFractionalDispense.HasValue) query = query.Where(x => x.IsAllowFractionalDispense == isAllowFractionalDispense.Value);
            if (isNeedPrescription.HasValue) query = query.Where(x => x.IsNeedPrescription == isNeedPrescription.Value);
            if (isCoveredByInsuranceDefault.HasValue) query = query.Where(x => x.IsCoveredByInsuranceDefault == isCoveredByInsuranceDefault.Value);
            if (isNeedApproval.HasValue) query = query.Where(x => x.IsNeedApproval == isNeedApproval.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.DrugCode.ToLower().Contains(keyword) ||
                    x.DrugName.ToLower().Contains(keyword) ||
                    (x.GenericName != null && x.GenericName.ToLower().Contains(keyword)) ||
                    (x.BrandName != null && x.BrandName.ToLower().Contains(keyword)) ||
                    (x.ManufacturerName != null && x.ManufacturerName.ToLower().Contains(keyword)) ||
                    (x.DrugForm != null && x.DrugForm.ToLower().Contains(keyword)) ||
                    (x.Strength != null && x.Strength.ToLower().Contains(keyword)) ||
                    (x.BaseUnit != null && x.BaseUnit.ToLower().Contains(keyword)) ||
                    (x.DispenseUnit != null && x.DispenseUnit.ToLower().Contains(keyword)) ||
                    (x.Route != null && x.Route.ToLower().Contains(keyword)) ||
                    (x.Indication != null && x.Indication.ToLower().Contains(keyword)) ||
                    (x.Contraindication != null && x.Contraindication.ToLower().Contains(keyword)) ||
                    (x.SideEffect != null && x.SideEffect.ToLower().Contains(keyword)) ||
                    (x.ExternalDrugCode != null && x.ExternalDrugCode.ToLower().Contains(keyword)) ||
                    (x.IntegrationCode != null && x.IntegrationCode.ToLower().Contains(keyword)) ||
                    (x.BpomRegistrationNumber != null && x.BpomRegistrationNumber.ToLower().Contains(keyword)) ||
                    (x.NationalDrugCode != null && x.NationalDrugCode.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)) ||
                    (x.DrugCategory != null && x.DrugCategory.DrugCategoryCode.ToLower().Contains(keyword)) ||
                    (x.DrugCategory != null && x.DrugCategory.DrugCategoryName.ToLower().Contains(keyword)) ||
                    (x.StrengthMeasurement != null && x.StrengthMeasurement.MeasurementName.ToLower().Contains(keyword)) ||
                    (x.BaseUnitMeasurement != null && x.BaseUnitMeasurement.MeasurementName.ToLower().Contains(keyword)) ||
                    (x.DispenseUnitMeasurement != null && x.DispenseUnitMeasurement.MeasurementName.ToLower().Contains(keyword)) ||
                    (x.PurchaseUnitMeasurement != null && x.PurchaseUnitMeasurement.MeasurementName.ToLower().Contains(keyword)) ||
                    (x.StockUnitMeasurement != null && x.StockUnitMeasurement.MeasurementName.ToLower().Contains(keyword)) ||
                    (x.DefaultDoseUnitMeasurement != null && x.DefaultDoseUnitMeasurement.MeasurementName.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<MstDrug> ApplySorting(
            IQueryable<MstDrug> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "sortOrder").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => isDescending ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "drugcode" => isDescending ? query.OrderByDescending(x => x.DrugCode) : query.OrderBy(x => x.DrugCode),
                "drugname" => isDescending ? query.OrderByDescending(x => x.DrugName) : query.OrderBy(x => x.DrugName),
                "genericname" => isDescending ? query.OrderByDescending(x => x.GenericName) : query.OrderBy(x => x.GenericName),
                "brandname" => isDescending ? query.OrderByDescending(x => x.BrandName) : query.OrderBy(x => x.BrandName),
                "manufacturername" => isDescending ? query.OrderByDescending(x => x.ManufacturerName) : query.OrderBy(x => x.ManufacturerName),
                "drugcategoryname" => isDescending
                    ? query.OrderByDescending(x => x.DrugCategory != null ? x.DrugCategory.DrugCategoryName : string.Empty)
                    : query.OrderBy(x => x.DrugCategory != null ? x.DrugCategory.DrugCategoryName : string.Empty),
                "drugform" => isDescending ? query.OrderByDescending(x => x.DrugForm) : query.OrderBy(x => x.DrugForm),
                "route" => isDescending ? query.OrderByDescending(x => x.Route) : query.OrderBy(x => x.Route),
                "defaultprice" => isDescending ? query.OrderByDescending(x => x.DefaultPrice) : query.OrderBy(x => x.DefaultPrice),
                "isformulary" => isDescending ? query.OrderByDescending(x => x.IsFormulary) : query.OrderBy(x => x.IsFormulary),
                "isgeneric" => isDescending ? query.OrderByDescending(x => x.IsGeneric) : query.OrderBy(x => x.IsGeneric),
                "ishighalert" => isDescending ? query.OrderByDescending(x => x.IsHighAlert) : query.OrderBy(x => x.IsHighAlert),
                "isneedprescription" => isDescending ? query.OrderByDescending(x => x.IsNeedPrescription) : query.OrderBy(x => x.IsNeedPrescription),
                "isneedapproval" => isDescending ? query.OrderByDescending(x => x.IsNeedApproval) : query.OrderBy(x => x.IsNeedApproval),
                "isactive" => isDescending ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
                _ => isDescending
                    ? query.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.DrugName)
                    : query.OrderBy(x => x.SortOrder).ThenBy(x => x.DrugName)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            CreateDrugRequest request)
        {
            if (request.DrugCategoryId == Guid.Empty)
                return (false, "Drug category wajib dipilih.");

            if (string.IsNullOrWhiteSpace(request.DrugName))
                return (false, "Nama drug wajib diisi.");

            if (request.DefaultPrice < 0)
                return (false, "Harga default tidak boleh kurang dari 0.");

            if (request.InsurancePrice.HasValue && request.InsurancePrice.Value < 0)
                return (false, "Harga insurance tidak boleh kurang dari 0.");

            if (request.MemberPrice.HasValue && request.MemberPrice.Value < 0)
                return (false, "Harga member tidak boleh kurang dari 0.");

            if (request.CompanyPrice.HasValue && request.CompanyPrice.Value < 0)
                return (false, "Harga company tidak boleh kurang dari 0.");

            if (request.StrengthValue.HasValue && request.StrengthValue.Value < 0)
                return (false, "Nilai strength tidak boleh kurang dari 0.");

            if (!string.IsNullOrWhiteSpace(request.DrugForm) && !DrugFormOptions.Contains(request.DrugForm.Trim()))
                return (false, "Drug form tidak valid. Gunakan nilai dari endpoint filters/metadata.");

            if (!string.IsNullOrWhiteSpace(request.Route) && !RouteOptions.Contains(request.Route.Trim()))
                return (false, "Route tidak valid. Gunakan nilai dari endpoint filters/metadata.");

            var categoryExists = await _dbContext.Set<MstDrugCategory>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == request.DrugCategoryId && !x.IsDelete && x.IsActive);

            if (!categoryExists)
                return (false, "Drug category tidak ditemukan atau tidak aktif.");

            var measurementIds = new List<Guid?>
            {
                request.StrengthMeasurementId,
                request.BaseUnitMeasurementId,
                request.DispenseUnitMeasurementId,
                request.PurchaseUnitMeasurementId,
                request.StockUnitMeasurementId,
                request.DefaultDoseUnitMeasurementId
            }
            .Where(x => x.HasValue && x.Value != Guid.Empty)
            .Select(x => x!.Value)
            .Distinct()
            .ToList();

            if (measurementIds.Count > 0)
            {
                var validMeasurementCount = await _dbContext.Set<MstMeasurement>()
                    .AsNoTracking()
                    .CountAsync(x => measurementIds.Contains(x.Id) && !x.IsDelete && x.IsActive);

                if (validMeasurementCount != measurementIds.Count)
                    return (false, "Measurement yang dipilih tidak valid atau tidak aktif.");
            }

            var normalizedName = request.DrugName.Trim().ToLower();
            var normalizedStrength = NormalizeComparableText(request.Strength);
            var normalizedDrugForm = NormalizeComparableText(request.DrugForm);
            var normalizedBrandName = NormalizeComparableText(request.BrandName);

            var duplicateNameQuery = _dbContext.Set<MstDrug>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.DrugCategoryId == request.DrugCategoryId &&
                    x.DrugName.ToLower() == normalizedName &&
                    (x.Strength ?? string.Empty).Trim().ToLower() == normalizedStrength &&
                    (x.DrugForm ?? string.Empty).Trim().ToLower() == normalizedDrugForm &&
                    (x.BrandName ?? string.Empty).Trim().ToLower() == normalizedBrandName);

            if (excludeId.HasValue)
                duplicateNameQuery = duplicateNameQuery.Where(x => x.Id != excludeId.Value);

            if (await duplicateNameQuery.AnyAsync())
                return (false, "Drug dengan nama, kategori, strength, bentuk, dan brand tersebut sudah digunakan.");

            if (!string.IsNullOrWhiteSpace(request.ExternalDrugCode))
            {
                var externalCode = request.ExternalDrugCode.Trim().ToLower();

                var duplicateExternalCodeQuery = _dbContext.Set<MstDrug>()
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDelete &&
                        x.ExternalDrugCode != null &&
                        x.ExternalDrugCode.ToLower() == externalCode);

                if (excludeId.HasValue)
                    duplicateExternalCodeQuery = duplicateExternalCodeQuery.Where(x => x.Id != excludeId.Value);

                if (await duplicateExternalCodeQuery.AnyAsync())
                    return (false, "External drug code sudah digunakan.");
            }

            if (!string.IsNullOrWhiteSpace(request.IntegrationCode))
            {
                var integrationCode = request.IntegrationCode.Trim().ToLower();

                var duplicateIntegrationCodeQuery = _dbContext.Set<MstDrug>()
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDelete &&
                        x.IntegrationCode != null &&
                        x.IntegrationCode.ToLower() == integrationCode);

                if (excludeId.HasValue)
                    duplicateIntegrationCodeQuery = duplicateIntegrationCodeQuery.Where(x => x.Id != excludeId.Value);

                if (await duplicateIntegrationCodeQuery.AnyAsync())
                    return (false, "Integration code sudah digunakan.");
            }

            return (true, null);
        }

        private async Task<string> GenerateDrugCodeAsync()
        {
            var existingCodes = await _dbContext.Set<MstDrug>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.DrugCode.StartsWith(CodePrefix))
                .Select(x => x.DrugCode)
                .ToListAsync();

            var usedNumbers = existingCodes
                .Select(ExtractDrugSequenceNumber)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Where(x => x > 0)
                .ToHashSet();

            var nextNumber = 1;
            while (usedNumbers.Contains(nextNumber))
                nextNumber++;

            return CodePrefix + nextNumber.ToString().PadLeft(CodeNumberLength, '0');
        }

        private static int? ExtractDrugSequenceNumber(string drugCode)
        {
            if (string.IsNullOrWhiteSpace(drugCode))
                return null;

            if (!drugCode.StartsWith(CodePrefix, StringComparison.OrdinalIgnoreCase))
                return null;

            var numberText = drugCode[CodePrefix.Length..];

            return int.TryParse(numberText, out var number)
                ? number
                : null;
        }

        private async Task<Dictionary<Guid, string?>> GetActorNameMapAsync(IEnumerable<Guid> actorIds)
        {
            var ids = actorIds
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            if (!ids.Any())
            {
                return new Dictionary<Guid, string?>();
            }

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

        private static DrugResponse MapResponse(MstDrug entity, IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new DrugResponse
            {
                Id = entity.Id,
                DrugCategoryId = entity.DrugCategoryId,
                DrugCategoryCode = entity.DrugCategory?.DrugCategoryCode ?? string.Empty,
                DrugCategoryName = entity.DrugCategory?.DrugCategoryName ?? string.Empty,
                DrugCategoryType = entity.DrugCategory?.DrugCategoryType,
                DrugGroupName = entity.DrugCategory?.DrugGroupName,
                DrugCode = entity.DrugCode,
                DrugName = entity.DrugName,
                GenericName = entity.GenericName,
                BrandName = entity.BrandName,
                ManufacturerName = entity.ManufacturerName,
                DrugForm = entity.DrugForm,
                DrugFormName = BuildDrugFormLabel(entity.DrugForm),
                Strength = entity.Strength,
                StrengthValue = entity.StrengthValue,
                StrengthMeasurementId = entity.StrengthMeasurementId,
                StrengthMeasurementCode = entity.StrengthMeasurement?.MeasurementCode,
                StrengthMeasurementName = entity.StrengthMeasurement?.MeasurementName,
                StrengthMeasurementSymbol = entity.StrengthMeasurement?.MeasurementSymbol,
                BaseUnit = entity.BaseUnit,
                DispenseUnit = entity.DispenseUnit,
                BaseUnitMeasurementId = entity.BaseUnitMeasurementId,
                BaseUnitMeasurementCode = entity.BaseUnitMeasurement?.MeasurementCode,
                BaseUnitMeasurementName = entity.BaseUnitMeasurement?.MeasurementName,
                BaseUnitMeasurementSymbol = entity.BaseUnitMeasurement?.MeasurementSymbol,
                DispenseUnitMeasurementId = entity.DispenseUnitMeasurementId,
                DispenseUnitMeasurementCode = entity.DispenseUnitMeasurement?.MeasurementCode,
                DispenseUnitMeasurementName = entity.DispenseUnitMeasurement?.MeasurementName,
                DispenseUnitMeasurementSymbol = entity.DispenseUnitMeasurement?.MeasurementSymbol,
                PurchaseUnitMeasurementId = entity.PurchaseUnitMeasurementId,
                PurchaseUnitMeasurementCode = entity.PurchaseUnitMeasurement?.MeasurementCode,
                PurchaseUnitMeasurementName = entity.PurchaseUnitMeasurement?.MeasurementName,
                PurchaseUnitMeasurementSymbol = entity.PurchaseUnitMeasurement?.MeasurementSymbol,
                StockUnitMeasurementId = entity.StockUnitMeasurementId,
                StockUnitMeasurementCode = entity.StockUnitMeasurement?.MeasurementCode,
                StockUnitMeasurementName = entity.StockUnitMeasurement?.MeasurementName,
                StockUnitMeasurementSymbol = entity.StockUnitMeasurement?.MeasurementSymbol,
                DefaultDoseUnitMeasurementId = entity.DefaultDoseUnitMeasurementId,
                DefaultDoseUnitMeasurementCode = entity.DefaultDoseUnitMeasurement?.MeasurementCode,
                DefaultDoseUnitMeasurementName = entity.DefaultDoseUnitMeasurement?.MeasurementName,
                DefaultDoseUnitMeasurementSymbol = entity.DefaultDoseUnitMeasurement?.MeasurementSymbol,
                Route = entity.Route,
                RouteName = BuildRouteLabel(entity.Route),
                IsFormulary = entity.IsFormulary,
                IsGeneric = entity.IsGeneric,
                IsAntibiotic = entity.IsAntibiotic,
                IsNarcotic = entity.IsNarcotic,
                IsPsychotropic = entity.IsPsychotropic,
                IsHighAlert = entity.IsHighAlert,
                IsChronicDiseaseDrug = entity.IsChronicDiseaseDrug,
                IsVaccine = entity.IsVaccine,
                IsConsumable = entity.IsConsumable,
                IsCompoundIngredientAllowed = entity.IsCompoundIngredientAllowed,
                IsStockManaged = entity.IsStockManaged,
                IsBatchTracked = entity.IsBatchTracked,
                IsExpiryDateTracked = entity.IsExpiryDateTracked,
                IsAllowFractionalDispense = entity.IsAllowFractionalDispense,
                IsNeedPrescription = entity.IsNeedPrescription,
                IsCoveredByInsuranceDefault = entity.IsCoveredByInsuranceDefault,
                IsNeedApproval = entity.IsNeedApproval,
                DefaultPrice = entity.DefaultPrice,
                InsurancePrice = entity.InsurancePrice,
                MemberPrice = entity.MemberPrice,
                CompanyPrice = entity.CompanyPrice,
                ExternalDrugCode = entity.ExternalDrugCode,
                IntegrationCode = entity.IntegrationCode,
                BpomRegistrationNumber = entity.BpomRegistrationNumber,
                NationalDrugCode = entity.NationalDrugCode,
                SortOrder = entity.SortOrder,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy)
            };
        }

        private static DrugDetailResponse MapDetailResponse(MstDrug entity, IReadOnlyDictionary<Guid, string?> actorNames)
        {
            var response = new DrugDetailResponse
            {
                Indication = entity.Indication,
                Contraindication = entity.Contraindication,
                SideEffect = entity.SideEffect,
                WarningPrecaution = entity.WarningPrecaution,
                DosageInformation = entity.DosageInformation,
                DrugInteraction = entity.DrugInteraction,
                AdministrationInstruction = entity.AdministrationInstruction,
                StorageInstruction = entity.StorageInstruction,
                PregnancyCategory = entity.PregnancyCategory,
                LactationNote = entity.LactationNote,
                PediatricNote = entity.PediatricNote,
                GeriatricNote = entity.GeriatricNote,
                Description = entity.Description,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };

            var baseResponse = MapResponse(entity, actorNames);
            CopyDrugResponse(baseResponse, response);

            return response;
        }

        private static DrugOptionResponse MapOptionResponse(MstDrug entity)
        {
            return new DrugOptionResponse
            {
                Id = entity.Id,
                DrugCategoryId = entity.DrugCategoryId,
                DrugCategoryName = entity.DrugCategory?.DrugCategoryName ?? string.Empty,
                DrugCode = entity.DrugCode,
                DrugName = entity.DrugName,
                GenericName = entity.GenericName,
                BrandName = entity.BrandName,
                DrugForm = entity.DrugForm,
                DrugFormName = BuildDrugFormLabel(entity.DrugForm),
                Strength = entity.Strength,
                StrengthValue = entity.StrengthValue,
                StrengthMeasurementSymbol = entity.StrengthMeasurement?.MeasurementSymbol,
                BaseUnit = entity.BaseUnit,
                DispenseUnit = entity.DispenseUnit,
                BaseUnitMeasurementSymbol = entity.BaseUnitMeasurement?.MeasurementSymbol,
                DispenseUnitMeasurementSymbol = entity.DispenseUnitMeasurement?.MeasurementSymbol,
                PurchaseUnitMeasurementSymbol = entity.PurchaseUnitMeasurement?.MeasurementSymbol,
                StockUnitMeasurementSymbol = entity.StockUnitMeasurement?.MeasurementSymbol,
                DefaultDoseUnitMeasurementSymbol = entity.DefaultDoseUnitMeasurement?.MeasurementSymbol,
                Route = entity.Route,
                RouteName = BuildRouteLabel(entity.Route),
                IsFormulary = entity.IsFormulary,
                IsGeneric = entity.IsGeneric,
                IsAntibiotic = entity.IsAntibiotic,
                IsNarcotic = entity.IsNarcotic,
                IsPsychotropic = entity.IsPsychotropic,
                IsHighAlert = entity.IsHighAlert,
                IsChronicDiseaseDrug = entity.IsChronicDiseaseDrug,
                IsVaccine = entity.IsVaccine,
                IsConsumable = entity.IsConsumable,
                IsCompoundIngredientAllowed = entity.IsCompoundIngredientAllowed,
                IsNeedPrescription = entity.IsNeedPrescription,
                IsCoveredByInsuranceDefault = entity.IsCoveredByInsuranceDefault,
                IsNeedApproval = entity.IsNeedApproval,
                DefaultPrice = entity.DefaultPrice,
                InsurancePrice = entity.InsurancePrice,
                MemberPrice = entity.MemberPrice,
                CompanyPrice = entity.CompanyPrice
            };
        }

        private static DrugCreateResponse ToCreateResponse(MstDrug entity)
        {
            return new DrugCreateResponse
            {
                Id = entity.Id,
                DrugCode = entity.DrugCode,
                DrugName = entity.DrugName,
                DrugCategoryId = entity.DrugCategoryId,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime
            };
        }

        private static DrugUpdateResponse ToUpdateResponse(MstDrug entity)
        {
            return new DrugUpdateResponse
            {
                Id = entity.Id,
                DrugCode = entity.DrugCode,
                DrugName = entity.DrugName,
                DrugCategoryId = entity.DrugCategoryId,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                UpdateDateTime = entity.UpdateDateTime
            };
        }

        private static void CopyDrugResponse(DrugResponse source, DrugResponse target)
        {
            target.Id = source.Id;
            target.DrugCategoryId = source.DrugCategoryId;
            target.DrugCategoryCode = source.DrugCategoryCode;
            target.DrugCategoryName = source.DrugCategoryName;
            target.DrugCategoryType = source.DrugCategoryType;
            target.DrugGroupName = source.DrugGroupName;
            target.DrugCode = source.DrugCode;
            target.DrugName = source.DrugName;
            target.GenericName = source.GenericName;
            target.BrandName = source.BrandName;
            target.ManufacturerName = source.ManufacturerName;
            target.DrugForm = source.DrugForm;
            target.DrugFormName = source.DrugFormName;
            target.Strength = source.Strength;
            target.StrengthValue = source.StrengthValue;
            target.StrengthMeasurementId = source.StrengthMeasurementId;
            target.StrengthMeasurementCode = source.StrengthMeasurementCode;
            target.StrengthMeasurementName = source.StrengthMeasurementName;
            target.StrengthMeasurementSymbol = source.StrengthMeasurementSymbol;
            target.BaseUnit = source.BaseUnit;
            target.DispenseUnit = source.DispenseUnit;
            target.BaseUnitMeasurementId = source.BaseUnitMeasurementId;
            target.BaseUnitMeasurementCode = source.BaseUnitMeasurementCode;
            target.BaseUnitMeasurementName = source.BaseUnitMeasurementName;
            target.BaseUnitMeasurementSymbol = source.BaseUnitMeasurementSymbol;
            target.DispenseUnitMeasurementId = source.DispenseUnitMeasurementId;
            target.DispenseUnitMeasurementCode = source.DispenseUnitMeasurementCode;
            target.DispenseUnitMeasurementName = source.DispenseUnitMeasurementName;
            target.DispenseUnitMeasurementSymbol = source.DispenseUnitMeasurementSymbol;
            target.PurchaseUnitMeasurementId = source.PurchaseUnitMeasurementId;
            target.PurchaseUnitMeasurementCode = source.PurchaseUnitMeasurementCode;
            target.PurchaseUnitMeasurementName = source.PurchaseUnitMeasurementName;
            target.PurchaseUnitMeasurementSymbol = source.PurchaseUnitMeasurementSymbol;
            target.StockUnitMeasurementId = source.StockUnitMeasurementId;
            target.StockUnitMeasurementCode = source.StockUnitMeasurementCode;
            target.StockUnitMeasurementName = source.StockUnitMeasurementName;
            target.StockUnitMeasurementSymbol = source.StockUnitMeasurementSymbol;
            target.DefaultDoseUnitMeasurementId = source.DefaultDoseUnitMeasurementId;
            target.DefaultDoseUnitMeasurementCode = source.DefaultDoseUnitMeasurementCode;
            target.DefaultDoseUnitMeasurementName = source.DefaultDoseUnitMeasurementName;
            target.DefaultDoseUnitMeasurementSymbol = source.DefaultDoseUnitMeasurementSymbol;
            target.Route = source.Route;
            target.RouteName = source.RouteName;
            target.IsFormulary = source.IsFormulary;
            target.IsGeneric = source.IsGeneric;
            target.IsAntibiotic = source.IsAntibiotic;
            target.IsNarcotic = source.IsNarcotic;
            target.IsPsychotropic = source.IsPsychotropic;
            target.IsHighAlert = source.IsHighAlert;
            target.IsChronicDiseaseDrug = source.IsChronicDiseaseDrug;
            target.IsVaccine = source.IsVaccine;
            target.IsConsumable = source.IsConsumable;
            target.IsCompoundIngredientAllowed = source.IsCompoundIngredientAllowed;
            target.IsStockManaged = source.IsStockManaged;
            target.IsBatchTracked = source.IsBatchTracked;
            target.IsExpiryDateTracked = source.IsExpiryDateTracked;
            target.IsAllowFractionalDispense = source.IsAllowFractionalDispense;
            target.IsNeedPrescription = source.IsNeedPrescription;
            target.IsCoveredByInsuranceDefault = source.IsCoveredByInsuranceDefault;
            target.IsNeedApproval = source.IsNeedApproval;
            target.DefaultPrice = source.DefaultPrice;
            target.InsurancePrice = source.InsurancePrice;
            target.MemberPrice = source.MemberPrice;
            target.CompanyPrice = source.CompanyPrice;
            target.ExternalDrugCode = source.ExternalDrugCode;
            target.IntegrationCode = source.IntegrationCode;
            target.BpomRegistrationNumber = source.BpomRegistrationNumber;
            target.NationalDrugCode = source.NationalDrugCode;
            target.SortOrder = source.SortOrder;
            target.IsActive = source.IsActive;
            target.CreateDateTime = source.CreateDateTime;
            target.CreateBy = source.CreateBy;
            target.CreateByName = source.CreateByName;
        }

        private static string? GetActorName(
            IReadOnlyDictionary<Guid, string?> actorNames,
            Guid actorId)
        {
            if (actorId == Guid.Empty)
            {
                return null;
            }

            return actorNames.TryGetValue(actorId, out var actorName)
                ? actorName
                : null;
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            return (pageNumber, pageSize);
        }

        private static Guid? NormalizeNullableGuid(Guid? value)
        {
            return value.HasValue && value.Value != Guid.Empty
                ? value.Value
                : null;
        }

        private static string? NormalizeNullableString(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private static string NormalizeComparableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Trim().ToLower();
        }

        private static string? NormalizeDrugForm(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var trimmed = value.Trim();
            var matched = DrugFormOptions.FirstOrDefault(x => string.Equals(x, trimmed, StringComparison.OrdinalIgnoreCase));

            return matched ?? trimmed;
        }

        private static string? NormalizeRoute(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var trimmed = value.Trim();
            var matched = RouteOptions.FirstOrDefault(x => string.Equals(x, trimmed, StringComparison.OrdinalIgnoreCase));

            return matched ?? trimmed;
        }

        private static string? BuildDrugFormLabel(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : SplitPascalCase(value);
        }

        private static string? BuildRouteLabel(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return value switch
            {
                "IV" => "IV",
                "IM" => "IM",
                "SC" => "SC",
                _ => SplitPascalCase(value)
            };
        }

        private static string SplitPascalCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;

            return string.Concat(value.Select((x, i) =>
                i > 0 && char.IsUpper(x) ? " " + x : x.ToString()));
        }

        private static List<DrugSortOptionResponse> BuildSortOptions()
        {
            return new List<DrugSortOptionResponse>
            {
                new() { Value = "sortOrder", Label = "Urutan" },
                new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                new() { Value = "drugCode", Label = "Kode obat" },
                new() { Value = "drugName", Label = "Nama obat" },
                new() { Value = "genericName", Label = "Nama generik" },
                new() { Value = "brandName", Label = "Nama brand" },
                new() { Value = "manufacturerName", Label = "Pabrikan" },
                new() { Value = "drugCategoryName", Label = "Kategori obat" },
                new() { Value = "drugForm", Label = "Bentuk obat" },
                new() { Value = "route", Label = "Rute" },
                new() { Value = "defaultPrice", Label = "Harga default" },
                new() { Value = "isFormulary", Label = "Formularium" },
                new() { Value = "isGeneric", Label = "Generik" },
                new() { Value = "isHighAlert", Label = "High alert" },
                new() { Value = "isNeedPrescription", Label = "Butuh resep" },
                new() { Value = "isNeedApproval", Label = "Butuh approval" },
                new() { Value = "isActive", Label = "Status aktif" }
            };
        }

        private Guid GetCurrentUserId()
        {
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("user_id");

            return Guid.TryParse(userIdValue, out var userId)
                ? userId
                : Guid.Empty;
        }
    }
}
