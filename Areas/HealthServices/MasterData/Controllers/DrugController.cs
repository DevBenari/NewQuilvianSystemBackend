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
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            [FromQuery] Guid? drugCategoryId,
            [FromQuery] Guid? defaultTariffId,
            [FromQuery] bool? hasDefaultTariff,
            [FromQuery] string? genericName,
            [FromQuery] string? brandName,
            [FromQuery] string? manufacturerName,
            [FromQuery] string? drugForm,
            [FromQuery] string? baseUnit,
            [FromQuery] string? dispenseUnit,
            [FromQuery] string? route,
            [FromQuery] bool? isFormulary,
            [FromQuery] bool? isGeneric,
            [FromQuery] bool? isAntibiotic,
            [FromQuery] bool? isNarcotic,
            [FromQuery] bool? isPsychotropic,
            [FromQuery] bool? isHighAlert,
            [FromQuery] bool? isChronicDiseaseDrug,
            [FromQuery] bool? isVaccine,
            [FromQuery] bool? isConsumable,
            [FromQuery] bool? isNeedPrescription,
            [FromQuery] bool? isCoveredByInsuranceDefault,
            [FromQuery] bool? isNeedApproval,
            [FromQuery] decimal? minimumPrice,
            [FromQuery] decimal? maximumPrice,
            [FromQuery] string? sortBy = "sortOrder",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();

            query = ApplyFilter(
                query,
                search,
                isActive,
                drugCategoryId,
                defaultTariffId,
                hasDefaultTariff,
                genericName,
                brandName,
                manufacturerName,
                drugForm,
                baseUnit,
                dispenseUnit,
                route,
                isFormulary,
                isGeneric,
                isAntibiotic,
                isNarcotic,
                isPsychotropic,
                isHighAlert,
                isChronicDiseaseDrug,
                isVaccine,
                isConsumable,
                isNeedPrescription,
                isCoveredByInsuranceDefault,
                isNeedApproval,
                minimumPrice,
                maximumPrice
            );

            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => ToResponse(x))
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
            [FromQuery] Guid? defaultTariffId,
            [FromQuery] bool? hasDefaultTariff,
            [FromQuery] string? genericName,
            [FromQuery] string? brandName,
            [FromQuery] string? manufacturerName,
            [FromQuery] string? drugForm,
            [FromQuery] string? baseUnit,
            [FromQuery] string? dispenseUnit,
            [FromQuery] string? route,
            [FromQuery] bool? isFormulary,
            [FromQuery] bool? isGeneric,
            [FromQuery] bool? isAntibiotic,
            [FromQuery] bool? isNarcotic,
            [FromQuery] bool? isPsychotropic,
            [FromQuery] bool? isHighAlert,
            [FromQuery] bool? isChronicDiseaseDrug,
            [FromQuery] bool? isVaccine,
            [FromQuery] bool? isConsumable,
            [FromQuery] bool? isNeedPrescription,
            [FromQuery] bool? isCoveredByInsuranceDefault,
            [FromQuery] bool? isNeedApproval,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null)
        {
            var query = BuildBaseQuery();

            query = ApplyFilter(
                query,
                search,
                onlyActive ? true : null,
                drugCategoryId,
                defaultTariffId,
                hasDefaultTariff,
                genericName,
                brandName,
                manufacturerName,
                drugForm,
                baseUnit,
                dispenseUnit,
                route,
                isFormulary,
                isGeneric,
                isAntibiotic,
                isNarcotic,
                isPsychotropic,
                isHighAlert,
                isChronicDiseaseDrug,
                isVaccine,
                isConsumable,
                isNeedPrescription,
                isCoveredByInsuranceDefault,
                isNeedApproval,
                null,
                null
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
                    BaseUnit = x.BaseUnit,
                    DispenseUnit = x.DispenseUnit,
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
                    BaseUnit = x.BaseUnit,
                    DispenseUnit = x.DispenseUnit,
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
        [AccessAction("Create", "Create Drug", Description = "Membuat data drug", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("Drug", "Create")]
        public async Task<IActionResult> CreateDrug([FromBody] CreateDrugRequest request)
        {
            var validation = await ValidateRequestAsync(
                excludeId: null,
                drugCategoryId: request.DrugCategoryId,
                defaultTariffId: request.DefaultTariffId,
                drugCode: request.DrugCode,
                drugName: request.DrugName,
                defaultPrice: request.DefaultPrice,
                insurancePrice: request.InsurancePrice,
                memberPrice: request.MemberPrice,
                companyPrice: request.CompanyPrice
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
                DrugCode = request.DrugCode.Trim().ToUpperInvariant(),
                DrugName = request.DrugName.Trim(),
                GenericName = NormalizeNullableString(request.GenericName),
                BrandName = NormalizeNullableString(request.BrandName),
                ManufacturerName = NormalizeNullableString(request.ManufacturerName),
                DrugForm = NormalizeNullableString(request.DrugForm),
                Strength = NormalizeNullableString(request.Strength),
                BaseUnit = NormalizeNullableString(request.BaseUnit),
                DispenseUnit = NormalizeNullableString(request.DispenseUnit),
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
                IsActive = request.IsActive,
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
                DrugCategoryId = entity.DrugCategoryId
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
        [ProducesResponseType(typeof(ApiResponse<DrugUpdateResponse>), StatusCodes.Status200OK)]
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
                drugCategoryId: request.DrugCategoryId,
                defaultTariffId: request.DefaultTariffId,
                drugCode: request.DrugCode,
                drugName: request.DrugName,
                defaultPrice: request.DefaultPrice,
                insurancePrice: request.InsurancePrice,
                memberPrice: request.MemberPrice,
                companyPrice: request.CompanyPrice
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
            entity.DrugCode = request.DrugCode.Trim().ToUpperInvariant();
            entity.DrugName = request.DrugName.Trim();
            entity.GenericName = NormalizeNullableString(request.GenericName);
            entity.BrandName = NormalizeNullableString(request.BrandName);
            entity.ManufacturerName = NormalizeNullableString(request.ManufacturerName);
            entity.DrugForm = NormalizeNullableString(request.DrugForm);
            entity.Strength = NormalizeNullableString(request.Strength);
            entity.BaseUnit = NormalizeNullableString(request.BaseUnit);
            entity.DispenseUnit = NormalizeNullableString(request.DispenseUnit);
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

            var result = new DrugUpdateResponse
            {
                Id = entity.Id,
                DrugCode = entity.DrugCode,
                DrugName = entity.DrugName,
                IsActive = entity.IsActive,
                UpdateDateTime = entity.UpdateDateTime
            };

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

        [HttpPatch("{id:guid}/activate")]
        [ProducesResponseType(typeof(ApiResponse<DrugStatusResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Drug", Description = "Mengaktifkan data drug", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("Drug", "Update")]
        public async Task<IActionResult> ActivateDrug(Guid id)
        {
            return await UpdateStatusAsync(id, true, "Drug berhasil diaktifkan.");
        }

        [HttpPatch("{id:guid}/deactivate")]
        [ProducesResponseType(typeof(ApiResponse<DrugStatusResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Drug", Description = "Menonaktifkan data drug", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("Drug", "Update")]
        public async Task<IActionResult> DeactivateDrug(Guid id)
        {
            return await UpdateStatusAsync(id, false, "Drug berhasil dinonaktifkan.");
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DrugDeleteResponse>), StatusCodes.Status200OK)]
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

            var usedInCoverageRule = await _dbContext.Set<MstInsuranceCoverageRule>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.DrugId == id &&
                    !x.IsDelete);

            if (usedInCoverageRule)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Drug tidak dapat dihapus karena sudah digunakan pada insurance coverage rule."
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

        private async Task<IActionResult> UpdateStatusAsync(Guid id, bool isActive, string successMessage)
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

            entity.IsActive = isActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var result = new DrugStatusResponse
            {
                Id = entity.Id,
                DrugCode = entity.DrugCode,
                DrugName = entity.DrugName,
                IsActive = entity.IsActive,
                UpdateDateTime = entity.UpdateDateTime
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "Drug.UpdateStatus",
                successMessage,
                result
            );

            return Ok(ApiResponse<DrugStatusResponse>.Ok(
                result,
                successMessage
            ));
        }

        private IQueryable<MstDrug> BuildBaseQuery()
        {
            return _dbContext.Set<MstDrug>()
                .AsNoTracking()
                .Include(x => x.DrugCategory)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstDrug> ApplyFilter(
            IQueryable<MstDrug> query,
            string? search,
            bool? isActive,
            Guid? drugCategoryId,
            Guid? defaultTariffId,
            bool? hasDefaultTariff,
            string? genericName,
            string? brandName,
            string? manufacturerName,
            string? drugForm,
            string? baseUnit,
            string? dispenseUnit,
            string? route,
            bool? isFormulary,
            bool? isGeneric,
            bool? isAntibiotic,
            bool? isNarcotic,
            bool? isPsychotropic,
            bool? isHighAlert,
            bool? isChronicDiseaseDrug,
            bool? isVaccine,
            bool? isConsumable,
            bool? isNeedPrescription,
            bool? isCoveredByInsuranceDefault,
            bool? isNeedApproval,
            decimal? minimumPrice,
            decimal? maximumPrice)
        {
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
                    (x.DrugCategory != null && x.DrugCategory.DrugCategoryName.ToLower().Contains(keyword)));
            }

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (drugCategoryId.HasValue && drugCategoryId.Value != Guid.Empty)
                query = query.Where(x => x.DrugCategoryId == drugCategoryId.Value);            

            if (!string.IsNullOrWhiteSpace(genericName))
            {
                var keyword = genericName.Trim().ToLower();
                query = query.Where(x => x.GenericName != null && x.GenericName.ToLower().Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(brandName))
            {
                var keyword = brandName.Trim().ToLower();
                query = query.Where(x => x.BrandName != null && x.BrandName.ToLower().Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(manufacturerName))
            {
                var keyword = manufacturerName.Trim().ToLower();
                query = query.Where(x => x.ManufacturerName != null && x.ManufacturerName.ToLower().Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(drugForm))
            {
                var keyword = drugForm.Trim().ToLower();
                query = query.Where(x => x.DrugForm != null && x.DrugForm.ToLower() == keyword);
            }

            if (!string.IsNullOrWhiteSpace(baseUnit))
            {
                var keyword = baseUnit.Trim().ToLower();
                query = query.Where(x => x.BaseUnit != null && x.BaseUnit.ToLower().Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(dispenseUnit))
            {
                var keyword = dispenseUnit.Trim().ToLower();
                query = query.Where(x => x.DispenseUnit != null && x.DispenseUnit.ToLower().Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(route))
            {
                var keyword = route.Trim().ToLower();
                query = query.Where(x => x.Route != null && x.Route.ToLower() == keyword);
            }

            if (isFormulary.HasValue)
                query = query.Where(x => x.IsFormulary == isFormulary.Value);

            if (isGeneric.HasValue)
                query = query.Where(x => x.IsGeneric == isGeneric.Value);

            if (isAntibiotic.HasValue)
                query = query.Where(x => x.IsAntibiotic == isAntibiotic.Value);

            if (isNarcotic.HasValue)
                query = query.Where(x => x.IsNarcotic == isNarcotic.Value);

            if (isPsychotropic.HasValue)
                query = query.Where(x => x.IsPsychotropic == isPsychotropic.Value);

            if (isHighAlert.HasValue)
                query = query.Where(x => x.IsHighAlert == isHighAlert.Value);

            if (isChronicDiseaseDrug.HasValue)
                query = query.Where(x => x.IsChronicDiseaseDrug == isChronicDiseaseDrug.Value);

            if (isVaccine.HasValue)
                query = query.Where(x => x.IsVaccine == isVaccine.Value);

            if (isConsumable.HasValue)
                query = query.Where(x => x.IsConsumable == isConsumable.Value);

            if (isNeedPrescription.HasValue)
                query = query.Where(x => x.IsNeedPrescription == isNeedPrescription.Value);

            if (isCoveredByInsuranceDefault.HasValue)
                query = query.Where(x => x.IsCoveredByInsuranceDefault == isCoveredByInsuranceDefault.Value);

            if (isNeedApproval.HasValue)
                query = query.Where(x => x.IsNeedApproval == isNeedApproval.Value);

            if (minimumPrice.HasValue)
                query = query.Where(x => x.DefaultPrice >= minimumPrice.Value);

            if (maximumPrice.HasValue)
                query = query.Where(x => x.DefaultPrice <= maximumPrice.Value);

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
                "createdatetime" => isDescending
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),

                "drugcode" => isDescending
                    ? query.OrderByDescending(x => x.DrugCode)
                    : query.OrderBy(x => x.DrugCode),

                "drugname" => isDescending
                    ? query.OrderByDescending(x => x.DrugName)
                    : query.OrderBy(x => x.DrugName),

                "genericname" => isDescending
                    ? query.OrderByDescending(x => x.GenericName)
                    : query.OrderBy(x => x.GenericName),

                "brandname" => isDescending
                    ? query.OrderByDescending(x => x.BrandName)
                    : query.OrderBy(x => x.BrandName),

                "manufacturername" => isDescending
                    ? query.OrderByDescending(x => x.ManufacturerName)
                    : query.OrderBy(x => x.ManufacturerName),

                "drugcategoryname" => isDescending
                    ? query.OrderByDescending(x => x.DrugCategory != null ? x.DrugCategory.DrugCategoryName : string.Empty)
                    : query.OrderBy(x => x.DrugCategory != null ? x.DrugCategory.DrugCategoryName : string.Empty),

                "drugform" => isDescending
                    ? query.OrderByDescending(x => x.DrugForm)
                    : query.OrderBy(x => x.DrugForm),

                "route" => isDescending
                    ? query.OrderByDescending(x => x.Route)
                    : query.OrderBy(x => x.Route),

                "defaultprice" => isDescending
                    ? query.OrderByDescending(x => x.DefaultPrice)
                    : query.OrderBy(x => x.DefaultPrice),

                "isformulary" => isDescending
                    ? query.OrderByDescending(x => x.IsFormulary)
                    : query.OrderBy(x => x.IsFormulary),

                "isgeneric" => isDescending
                    ? query.OrderByDescending(x => x.IsGeneric)
                    : query.OrderBy(x => x.IsGeneric),

                "ishighalert" => isDescending
                    ? query.OrderByDescending(x => x.IsHighAlert)
                    : query.OrderBy(x => x.IsHighAlert),

                "isneedprescription" => isDescending
                    ? query.OrderByDescending(x => x.IsNeedPrescription)
                    : query.OrderBy(x => x.IsNeedPrescription),

                "isneedapproval" => isDescending
                    ? query.OrderByDescending(x => x.IsNeedApproval)
                    : query.OrderBy(x => x.IsNeedApproval),

                "isactive" => isDescending
                    ? query.OrderByDescending(x => x.IsActive)
                    : query.OrderBy(x => x.IsActive),

                _ => isDescending
                    ? query.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.DrugName)
                    : query.OrderBy(x => x.SortOrder).ThenBy(x => x.DrugName)
            };
        }

        private static DrugResponse ToResponse(MstDrug x)
        {
            return new DrugResponse
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
                BaseUnit = x.BaseUnit,
                DispenseUnit = x.DispenseUnit,
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
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            Guid drugCategoryId,
            Guid? defaultTariffId,
            string drugCode,
            string drugName,
            decimal defaultPrice,
            decimal? insurancePrice,
            decimal? memberPrice,
            decimal? companyPrice)
        {
            if (drugCategoryId == Guid.Empty)
                return (false, "Drug category wajib dipilih.");

            if (string.IsNullOrWhiteSpace(drugCode))
                return (false, "Kode drug wajib diisi.");

            if (string.IsNullOrWhiteSpace(drugName))
                return (false, "Nama drug wajib diisi.");

            if (defaultPrice < 0)
                return (false, "Harga default tidak boleh kurang dari 0.");

            if (insurancePrice.HasValue && insurancePrice.Value < 0)
                return (false, "Harga insurance tidak boleh kurang dari 0.");

            if (memberPrice.HasValue && memberPrice.Value < 0)
                return (false, "Harga member tidak boleh kurang dari 0.");

            if (companyPrice.HasValue && companyPrice.Value < 0)
                return (false, "Harga company tidak boleh kurang dari 0.");

            var categoryExists = await _dbContext.Set<MstDrugCategory>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == drugCategoryId &&
                    !x.IsDelete &&
                    x.IsActive);

            if (!categoryExists)
                return (false, "Drug category tidak ditemukan atau tidak aktif.");

            var normalizedDefaultTariffId = NormalizeNullableGuid(defaultTariffId);

            if (normalizedDefaultTariffId.HasValue)
            {
                var tariffExists = await _dbContext.Set<MstTariff>()
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id == normalizedDefaultTariffId.Value &&
                        !x.IsDelete &&
                        x.IsActive);

                if (!tariffExists)
                    return (false, "Default tariff tidak ditemukan atau tidak aktif.");
            }

            var normalizedCode = drugCode.Trim().ToUpperInvariant();
            var normalizedName = drugName.Trim().ToLower();

            var duplicateCodeQuery = _dbContext.Set<MstDrug>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.DrugCode.ToUpper() == normalizedCode);

            if (excludeId.HasValue)
                duplicateCodeQuery = duplicateCodeQuery.Where(x => x.Id != excludeId.Value);

            if (await duplicateCodeQuery.AnyAsync())
                return (false, "Kode drug sudah digunakan.");

            var duplicateNameQuery = _dbContext.Set<MstDrug>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.DrugName.ToLower() == normalizedName);

            if (excludeId.HasValue)
                duplicateNameQuery = duplicateNameQuery.Where(x => x.Id != excludeId.Value);

            if (await duplicateNameQuery.AnyAsync())
                return (false, "Nama drug sudah digunakan.");

            return (true, null);
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

        private Guid GetCurrentUserId()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userIdValue, out var userId)
                ? userId
                : Guid.Empty;
        }
    }
}
