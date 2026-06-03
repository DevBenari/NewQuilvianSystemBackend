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
        [AccessAction("Read", "Read Drug", Description = "Melihat data drug", AccessType = AccessTypes.Read, SortOrder = 1)]
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
                SortOptions = new List<DrugSortOptionResponse>
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
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                DrugFormOptions = DrugFormOptions.OrderBy(x => x).ToList(),
                RouteOptions = RouteOptions.OrderBy(x => x).ToList()
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
        [AccessAction("Read", "Read Drug", Description = "Melihat data drug", AccessType = AccessTypes.Read, SortOrder = 1)]
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
            [FromQuery] bool? isActive,
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
            query = ApplyStandardFilter(query, drugCategoryId, baseUnitMeasurementId, isActive, search);

            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new DrugResponse
                {
                    Id = x.Id,
                    DrugCategoryId = x.DrugCategoryId,
                    DrugCategoryCode = x.DrugCategory != null ? x.DrugCategory.DrugCategoryCode : string.Empty,
                    DrugCategoryName = x.DrugCategory != null ? x.DrugCategory.DrugCategoryName : string.Empty,
                    DrugCategoryType = x.DrugCategory != null ? x.DrugCategory.DrugCategoryType : null,
                    DrugGroupName = x.DrugCategory != null ? x.DrugCategory.DrugGroupName : null,
                    DrugCode = x.DrugCode,
                    DrugName = x.DrugName,
                    GenericName = x.GenericName,
                    BrandName = x.BrandName,
                    ManufacturerName = x.ManufacturerName,
                    DrugForm = x.DrugForm,
                    Strength = x.Strength,
                    StrengthValue = x.StrengthValue,
                    StrengthMeasurementId = x.StrengthMeasurementId,
                    StrengthMeasurementCode = x.StrengthMeasurement != null ? x.StrengthMeasurement.MeasurementCode : null,
                    StrengthMeasurementName = x.StrengthMeasurement != null ? x.StrengthMeasurement.MeasurementName : null,
                    StrengthMeasurementSymbol = x.StrengthMeasurement != null ? x.StrengthMeasurement.MeasurementSymbol : null,
                    BaseUnit = x.BaseUnit,
                    DispenseUnit = x.DispenseUnit,
                    BaseUnitMeasurementId = x.BaseUnitMeasurementId,
                    BaseUnitMeasurementCode = x.BaseUnitMeasurement != null ? x.BaseUnitMeasurement.MeasurementCode : null,
                    BaseUnitMeasurementName = x.BaseUnitMeasurement != null ? x.BaseUnitMeasurement.MeasurementName : null,
                    BaseUnitMeasurementSymbol = x.BaseUnitMeasurement != null ? x.BaseUnitMeasurement.MeasurementSymbol : null,
                    DispenseUnitMeasurementId = x.DispenseUnitMeasurementId,
                    DispenseUnitMeasurementCode = x.DispenseUnitMeasurement != null ? x.DispenseUnitMeasurement.MeasurementCode : null,
                    DispenseUnitMeasurementName = x.DispenseUnitMeasurement != null ? x.DispenseUnitMeasurement.MeasurementName : null,
                    DispenseUnitMeasurementSymbol = x.DispenseUnitMeasurement != null ? x.DispenseUnitMeasurement.MeasurementSymbol : null,
                    PurchaseUnitMeasurementId = x.PurchaseUnitMeasurementId,
                    PurchaseUnitMeasurementCode = x.PurchaseUnitMeasurement != null ? x.PurchaseUnitMeasurement.MeasurementCode : null,
                    PurchaseUnitMeasurementName = x.PurchaseUnitMeasurement != null ? x.PurchaseUnitMeasurement.MeasurementName : null,
                    PurchaseUnitMeasurementSymbol = x.PurchaseUnitMeasurement != null ? x.PurchaseUnitMeasurement.MeasurementSymbol : null,
                    StockUnitMeasurementId = x.StockUnitMeasurementId,
                    StockUnitMeasurementCode = x.StockUnitMeasurement != null ? x.StockUnitMeasurement.MeasurementCode : null,
                    StockUnitMeasurementName = x.StockUnitMeasurement != null ? x.StockUnitMeasurement.MeasurementName : null,
                    StockUnitMeasurementSymbol = x.StockUnitMeasurement != null ? x.StockUnitMeasurement.MeasurementSymbol : null,
                    DefaultDoseUnitMeasurementId = x.DefaultDoseUnitMeasurementId,
                    DefaultDoseUnitMeasurementCode = x.DefaultDoseUnitMeasurement != null ? x.DefaultDoseUnitMeasurement.MeasurementCode : null,
                    DefaultDoseUnitMeasurementName = x.DefaultDoseUnitMeasurement != null ? x.DefaultDoseUnitMeasurement.MeasurementName : null,
                    DefaultDoseUnitMeasurementSymbol = x.DefaultDoseUnitMeasurement != null ? x.DefaultDoseUnitMeasurement.MeasurementSymbol : null,
                    Route = x.Route,
                    IsFormulary = x.IsFormulary,
                    IsGeneric = x.IsGeneric,
                    IsAntibiotic = x.IsAntibiotic,
                    IsNarcotic = x.IsNarcotic,
                    IsPsychotropic = x.IsPsychotropic,
                    IsHighAlert = x.IsHighAlert,
                    IsChronicDiseaseDrug = x.IsChronicDiseaseDrug,
                    IsVaccine = x.IsVaccine,
                    IsConsumable = x.IsConsumable,
                    IsCompoundIngredientAllowed = x.IsCompoundIngredientAllowed,
                    IsStockManaged = x.IsStockManaged,
                    IsBatchTracked = x.IsBatchTracked,
                    IsExpiryDateTracked = x.IsExpiryDateTracked,
                    IsAllowFractionalDispense = x.IsAllowFractionalDispense,
                    IsNeedPrescription = x.IsNeedPrescription,
                    IsCoveredByInsuranceDefault = x.IsCoveredByInsuranceDefault,
                    IsNeedApproval = x.IsNeedApproval,
                    DefaultPrice = x.DefaultPrice,
                    InsurancePrice = x.InsurancePrice,
                    MemberPrice = x.MemberPrice,
                    CompanyPrice = x.CompanyPrice,
                    ExternalDrugCode = x.ExternalDrugCode,
                    IntegrationCode = x.IntegrationCode,
                    BpomRegistrationNumber = x.BpomRegistrationNumber,
                    NationalDrugCode = x.NationalDrugCode,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

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
        [ProducesResponseType(typeof(ApiResponse<List<DrugOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Drug", Description = "Melihat data drug", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Drug", "Read")]
        public async Task<IActionResult> GetDrugOptions(
            [FromQuery] Guid? drugCategoryId,
            [FromQuery] Guid? baseUnitMeasurementId,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null)
        {
            var query = BuildBaseQuery();

            query = ApplyStandardFilter(
                query,
                drugCategoryId,
                baseUnitMeasurementId,
                onlyActive ? true : null,
                search
            );

            var data = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.DrugName)
                .Select(x => new DrugOptionResponse
                {
                    Id = x.Id,
                    DrugCategoryId = x.DrugCategoryId,
                    DrugCategoryName = x.DrugCategory != null ? x.DrugCategory.DrugCategoryName : string.Empty,
                    DrugCode = x.DrugCode,
                    DrugName = x.DrugName,
                    GenericName = x.GenericName,
                    BrandName = x.BrandName,
                    DrugForm = x.DrugForm,
                    Strength = x.Strength,
                    StrengthValue = x.StrengthValue,
                    StrengthMeasurementSymbol = x.StrengthMeasurement != null ? x.StrengthMeasurement.MeasurementSymbol : null,
                    BaseUnit = x.BaseUnit,
                    DispenseUnit = x.DispenseUnit,
                    BaseUnitMeasurementSymbol = x.BaseUnitMeasurement != null ? x.BaseUnitMeasurement.MeasurementSymbol : null,
                    DispenseUnitMeasurementSymbol = x.DispenseUnitMeasurement != null ? x.DispenseUnitMeasurement.MeasurementSymbol : null,
                    PurchaseUnitMeasurementSymbol = x.PurchaseUnitMeasurement != null ? x.PurchaseUnitMeasurement.MeasurementSymbol : null,
                    StockUnitMeasurementSymbol = x.StockUnitMeasurement != null ? x.StockUnitMeasurement.MeasurementSymbol : null,
                    DefaultDoseUnitMeasurementSymbol = x.DefaultDoseUnitMeasurement != null ? x.DefaultDoseUnitMeasurement.MeasurementSymbol : null,
                    Route = x.Route,
                    IsFormulary = x.IsFormulary,
                    IsGeneric = x.IsGeneric,
                    IsAntibiotic = x.IsAntibiotic,
                    IsNarcotic = x.IsNarcotic,
                    IsPsychotropic = x.IsPsychotropic,
                    IsHighAlert = x.IsHighAlert,
                    IsChronicDiseaseDrug = x.IsChronicDiseaseDrug,
                    IsVaccine = x.IsVaccine,
                    IsConsumable = x.IsConsumable,
                    IsCompoundIngredientAllowed = x.IsCompoundIngredientAllowed,
                    IsNeedPrescription = x.IsNeedPrescription,
                    IsCoveredByInsuranceDefault = x.IsCoveredByInsuranceDefault,
                    IsNeedApproval = x.IsNeedApproval,
                    DefaultPrice = x.DefaultPrice,
                    InsurancePrice = x.InsurancePrice,
                    MemberPrice = x.MemberPrice,
                    CompanyPrice = x.CompanyPrice
                })
                .ToListAsync();

            return Ok(ApiResponse<List<DrugOptionResponse>>.Ok(
                data,
                "Data pilihan drug berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DrugDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Drug", Description = "Melihat data drug", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Drug", "Read")]
        public async Task<IActionResult> GetDrugById(Guid id)
        {
            var data = await BuildBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => new DrugDetailResponse
                {
                    Id = x.Id,
                    DrugCategoryId = x.DrugCategoryId,
                    DrugCategoryCode = x.DrugCategory != null ? x.DrugCategory.DrugCategoryCode : string.Empty,
                    DrugCategoryName = x.DrugCategory != null ? x.DrugCategory.DrugCategoryName : string.Empty,
                    DrugCategoryType = x.DrugCategory != null ? x.DrugCategory.DrugCategoryType : null,
                    DrugGroupName = x.DrugCategory != null ? x.DrugCategory.DrugGroupName : null,
                    DrugCode = x.DrugCode,
                    DrugName = x.DrugName,
                    GenericName = x.GenericName,
                    BrandName = x.BrandName,
                    ManufacturerName = x.ManufacturerName,
                    DrugForm = x.DrugForm,
                    Strength = x.Strength,
                    StrengthValue = x.StrengthValue,
                    StrengthMeasurementId = x.StrengthMeasurementId,
                    StrengthMeasurementCode = x.StrengthMeasurement != null ? x.StrengthMeasurement.MeasurementCode : null,
                    StrengthMeasurementName = x.StrengthMeasurement != null ? x.StrengthMeasurement.MeasurementName : null,
                    StrengthMeasurementSymbol = x.StrengthMeasurement != null ? x.StrengthMeasurement.MeasurementSymbol : null,
                    BaseUnit = x.BaseUnit,
                    DispenseUnit = x.DispenseUnit,
                    BaseUnitMeasurementId = x.BaseUnitMeasurementId,
                    BaseUnitMeasurementCode = x.BaseUnitMeasurement != null ? x.BaseUnitMeasurement.MeasurementCode : null,
                    BaseUnitMeasurementName = x.BaseUnitMeasurement != null ? x.BaseUnitMeasurement.MeasurementName : null,
                    BaseUnitMeasurementSymbol = x.BaseUnitMeasurement != null ? x.BaseUnitMeasurement.MeasurementSymbol : null,
                    DispenseUnitMeasurementId = x.DispenseUnitMeasurementId,
                    DispenseUnitMeasurementCode = x.DispenseUnitMeasurement != null ? x.DispenseUnitMeasurement.MeasurementCode : null,
                    DispenseUnitMeasurementName = x.DispenseUnitMeasurement != null ? x.DispenseUnitMeasurement.MeasurementName : null,
                    DispenseUnitMeasurementSymbol = x.DispenseUnitMeasurement != null ? x.DispenseUnitMeasurement.MeasurementSymbol : null,
                    PurchaseUnitMeasurementId = x.PurchaseUnitMeasurementId,
                    PurchaseUnitMeasurementCode = x.PurchaseUnitMeasurement != null ? x.PurchaseUnitMeasurement.MeasurementCode : null,
                    PurchaseUnitMeasurementName = x.PurchaseUnitMeasurement != null ? x.PurchaseUnitMeasurement.MeasurementName : null,
                    PurchaseUnitMeasurementSymbol = x.PurchaseUnitMeasurement != null ? x.PurchaseUnitMeasurement.MeasurementSymbol : null,
                    StockUnitMeasurementId = x.StockUnitMeasurementId,
                    StockUnitMeasurementCode = x.StockUnitMeasurement != null ? x.StockUnitMeasurement.MeasurementCode : null,
                    StockUnitMeasurementName = x.StockUnitMeasurement != null ? x.StockUnitMeasurement.MeasurementName : null,
                    StockUnitMeasurementSymbol = x.StockUnitMeasurement != null ? x.StockUnitMeasurement.MeasurementSymbol : null,
                    DefaultDoseUnitMeasurementId = x.DefaultDoseUnitMeasurementId,
                    DefaultDoseUnitMeasurementCode = x.DefaultDoseUnitMeasurement != null ? x.DefaultDoseUnitMeasurement.MeasurementCode : null,
                    DefaultDoseUnitMeasurementName = x.DefaultDoseUnitMeasurement != null ? x.DefaultDoseUnitMeasurement.MeasurementName : null,
                    DefaultDoseUnitMeasurementSymbol = x.DefaultDoseUnitMeasurement != null ? x.DefaultDoseUnitMeasurement.MeasurementSymbol : null,
                    Route = x.Route,
                    IsFormulary = x.IsFormulary,
                    IsGeneric = x.IsGeneric,
                    IsAntibiotic = x.IsAntibiotic,
                    IsNarcotic = x.IsNarcotic,
                    IsPsychotropic = x.IsPsychotropic,
                    IsHighAlert = x.IsHighAlert,
                    IsChronicDiseaseDrug = x.IsChronicDiseaseDrug,
                    IsVaccine = x.IsVaccine,
                    IsConsumable = x.IsConsumable,
                    IsCompoundIngredientAllowed = x.IsCompoundIngredientAllowed,
                    IsStockManaged = x.IsStockManaged,
                    IsBatchTracked = x.IsBatchTracked,
                    IsExpiryDateTracked = x.IsExpiryDateTracked,
                    IsAllowFractionalDispense = x.IsAllowFractionalDispense,
                    IsNeedPrescription = x.IsNeedPrescription,
                    IsCoveredByInsuranceDefault = x.IsCoveredByInsuranceDefault,
                    IsNeedApproval = x.IsNeedApproval,
                    DefaultPrice = x.DefaultPrice,
                    InsurancePrice = x.InsurancePrice,
                    MemberPrice = x.MemberPrice,
                    CompanyPrice = x.CompanyPrice,
                    Indication = x.Indication,
                    Contraindication = x.Contraindication,
                    SideEffect = x.SideEffect,
                    WarningPrecaution = x.WarningPrecaution,
                    DosageInformation = x.DosageInformation,
                    DrugInteraction = x.DrugInteraction,
                    AdministrationInstruction = x.AdministrationInstruction,
                    StorageInstruction = x.StorageInstruction,
                    PregnancyCategory = x.PregnancyCategory,
                    LactationNote = x.LactationNote,
                    PediatricNote = x.PediatricNote,
                    GeriatricNote = x.GeriatricNote,
                    ExternalDrugCode = x.ExternalDrugCode,
                    IntegrationCode = x.IntegrationCode,
                    BpomRegistrationNumber = x.BpomRegistrationNumber,
                    NationalDrugCode = x.NationalDrugCode,
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
                    "Drug tidak ditemukan."
                ));
            }

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
            var validation = await ValidateRequestAsync(
                excludeId: null,
                request: request
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data drug tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new MstDrug
            {
                Id = Guid.NewGuid(),
                DrugCategoryId = request.DrugCategoryId,
                DrugCode = await GenerateDrugCodeAsync(),
                DrugName = request.DrugName.Trim(),
                GenericName = NormalizeNullableString(request.GenericName),
                BrandName = NormalizeNullableString(request.BrandName),
                ManufacturerName = NormalizeNullableString(request.ManufacturerName),
                DrugForm = NormalizeNullableString(request.DrugForm),
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
                Route = NormalizeNullableString(request.Route),
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

            var result = new DrugCreateResponse
            {
                Id = entity.Id,
                DrugCode = entity.DrugCode,
                DrugName = entity.DrugName,
                DrugCategoryId = entity.DrugCategoryId,
                IsActive = entity.IsActive
            };

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
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
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

            var validation = await ValidateRequestAsync(
                excludeId: id,
                request: request
            );

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
            entity.DrugForm = NormalizeNullableString(request.DrugForm);
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
            entity.Route = NormalizeNullableString(request.Route);
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

            await _loggerService.InfoAsync(
                LogCategory,
                "Drug.UpdateDrug",
                "Mengubah data drug.",
                new { entity.Id, entity.DrugCode, entity.DrugName, entity.IsActive }
            );

            return Ok(ApiResponse<object>.Ok(
                null,
                "Drug berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Drug", Description = "Menghapus data drug", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("Drug", "Delete")]
        public async Task<IActionResult> DeleteDrug(Guid id)
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

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "Drug.DeleteDrug",
                "Menghapus data drug.",
                new { entity.Id, entity.DrugCode, entity.DrugName, entity.DeleteDateTime }
            );

            return Ok(ApiResponse<object>.Ok(
                null,
                "Drug berhasil dihapus."
            ));
        }

        private IQueryable<MstDrug> BuildBaseQuery()
        {
            return _dbContext.Set<MstDrug>()
                .AsNoTracking()
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
                var now = DateTime.UtcNow;
                var today = now.Date;

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
            bool? isActive,
            string? search)
        {
            if (drugCategoryId.HasValue && drugCategoryId.Value != Guid.Empty)
                query = query.Where(x => x.DrugCategoryId == drugCategoryId.Value);

            if (baseUnitMeasurementId.HasValue && baseUnitMeasurementId.Value != Guid.Empty)
                query = query.Where(x => x.BaseUnitMeasurementId == baseUnitMeasurementId.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

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

            var categoryExists = await _dbContext.Set<MstDrugCategory>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == request.DrugCategoryId &&
                    !x.IsDelete &&
                    x.IsActive);

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
                    .CountAsync(x =>
                        measurementIds.Contains(x.Id) &&
                        !x.IsDelete &&
                        x.IsActive);

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

            return (true, null);
        }

        private async Task<string> GenerateDrugCodeAsync()
        {
            var existingCodes = await _dbContext.Set<MstDrug>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.DrugCode.StartsWith(CodePrefix))
                .Select(x => x.DrugCode)
                .ToListAsync();

            var usedNumbers = existingCodes
                .Select(x => x.Replace(CodePrefix, string.Empty))
                .Where(x => int.TryParse(x, out _))
                .Select(int.Parse)
                .Where(x => x > 0)
                .ToHashSet();

            var nextNumber = 1;
            while (usedNumbers.Contains(nextNumber))
                nextNumber++;

            return CodePrefix + nextNumber.ToString().PadLeft(CodeNumberLength, '0');
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
                pageNumber = 1;

            if (pageSize < 1)
                pageSize = 25;

            if (pageSize > 100)
                pageSize = 100;

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

        private Guid GetCurrentUserId()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userIdValue, out var userId)
                ? userId
                : Guid.Empty;
        }
    }
}
