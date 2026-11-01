using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/clinical-management/prescribing-drugs")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_CLINICAL",
        moduleName: "Health Service Clinical",
        displayName: "Prescribing Drug",
        AreaName = "HealthServices",
        ControllerName = "PrescribingDrug",
        Description = "Katalog obat untuk proses resep dokter berdasarkan encounter",
        SortOrder = 5
    )]
    [Tags("Health Services / Clinical Management / Prescribing Drug")]
    public class PrescribingDrugController : ControllerBase
    {
        private const string LogCategory = "HealthServices.Clinical";

        private readonly ApplicationDbContext _dbContext;
        private readonly EncounterInsuranceService _encounterInsuranceService;
        private readonly InsuranceCoverageService _insuranceCoverageService;
        private readonly PharmacyDepotRoutingService _depotRoutingService;
        private readonly DrugStockService _drugStockService;
        private readonly LoggerService _loggerService;

        public PrescribingDrugController(
            ApplicationDbContext dbContext,
            EncounterInsuranceService encounterInsuranceService,
            InsuranceCoverageService insuranceCoverageService,
            PharmacyDepotRoutingService depotRoutingService,
            DrugStockService drugStockService,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _encounterInsuranceService = encounterInsuranceService;
            _insuranceCoverageService = insuranceCoverageService;
            _depotRoutingService = depotRoutingService;
            _drugStockService = drugStockService;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<PrescribingDrugFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Prescribing Drug", Description = "Melihat metadata filter katalog obat resep", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PrescribingDrug", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new PrescribingDrugFilterMetadataResponse
            {
                DefaultFilter = new PrescribingDrugDefaultFilterResponse(),
                SortOptions = new List<PrescribingDrugSortOptionResponse>
                {
                    new() { Value = "drugName", Label = "Nama obat" },
                    new() { Value = "drugCode", Label = "Kode obat" },
                    new() { Value = "genericName", Label = "Nama generik" },
                    new() { Value = "drugCategoryName", Label = "Kategori obat" },
                    new() { Value = "isFormulary", Label = "Formularium" },
                    new() { Value = "isGeneric", Label = "Generik" },
                    new() { Value = "isHighAlert", Label = "High alert" },
                    new() { Value = "sortOrder", Label = "Urutan" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                QueryParameters = BuildQueryParameterInfo()
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "PrescribingDrug.GetFilterMetadata",
                "Mengambil metadata filter katalog obat resep.",
                result
            );

            return Ok(ApiResponse<PrescribingDrugFilterMetadataResponse>.Ok(
                result,
                "Metadata filter katalog obat resep berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PrescribingDrugPagedResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Read", "Read Prescribing Drug", Description = "Melihat katalog obat resep", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PrescribingDrug", "Read")]
        public async Task<IActionResult> GetDrugs(
            [FromQuery] Guid encounterId,
            [FromQuery] Guid? drugCategoryId,
            [FromQuery] bool? isFormulary,
            [FromQuery] bool? isGeneric,
            [FromQuery] bool? isAntibiotic,
            [FromQuery] bool? isHighAlert,
            [FromQuery] bool? isCompoundIngredientAllowed,
            [FromQuery] bool? isConsumable,
            [FromQuery] bool? fromPharmacyStock,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "drugName",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            if (encounterId == Guid.Empty)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "EncounterId wajib diisi."
                ));
            }

            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var context = await _encounterInsuranceService.GetContextAsync(
                encounterId,
                cancellationToken: cancellationToken
            );

            if (!context.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    context.ErrorMessage ?? "Konteks encounter tidak valid."
                ));
            }

            // Katalog berbasis stok (RJ-DOC-DEC-087): depo ditentukan routing encounter yang sama
            // dengan reservasi resep, sehingga obat yang tampil adalah obat yang kelak ditahan.
            Guid? stockLocationId = null;
            string? stockLocationName = null;

            if (fromPharmacyStock == true)
            {
                var depot = await _depotRoutingService.ResolveAsync(encounterId, cancellationToken);

                if (!depot.IsSuccess || !depot.StorageLocationId.HasValue)
                {
                    return BadRequest(ApiResponse<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        depot.Message
                    ));
                }

                stockLocationId = depot.StorageLocationId.Value;
                stockLocationName = await _drugStockService.GetStorageLocationNameAsync(
                    stockLocationId.Value,
                    cancellationToken
                );
            }

            var query = ApplyFilters(
                BuildDrugQuery(),
                drugCategoryId,
                isFormulary,
                isGeneric,
                isAntibiotic,
                isHighAlert,
                isCompoundIngredientAllowed,
                isConsumable,
                search
            );

            if (stockLocationId.HasValue)
            {
                var stockedDrugIds = _drugStockService.QueryStockedDrugIds(stockLocationId.Value);
                query = query.Where(x => stockedDrugIds.Contains(x.Id));
            }

            var totalData = await query.CountAsync(cancellationToken);

            var drugs = await ApplySorting(query, sortBy, sortDirection, search)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var availableStock = stockLocationId.HasValue
                ? await _drugStockService.GetAvailableQuantitiesAsync(
                    stockLocationId.Value,
                    drugs.Select(x => x.Id).ToList(),
                    cancellationToken)
                : null;

            var items = new List<PrescribingDrugResponse>();

            foreach (var drug in drugs)
            {
                var coverage = await _insuranceCoverageService.ResolveDrugAsync(
                    encounterId,
                    drug.Id,
                    quantity: 1,
                    cancellationToken: cancellationToken
                );

                var item = MapResponse(drug, coverage);

                if (availableStock != null)
                {
                    ApplyStock(
                        item,
                        stockLocationId!.Value,
                        stockLocationName,
                        availableStock.GetValueOrDefault(drug.Id));
                }

                items.Add(item);
            }

            var result = new PrescribingDrugPagedResponse
            {
                EncounterId = encounterId,
                PaymentTypeName = context.PaymentTypeName,
                InsuranceProviderName = context.InsuranceProviderName ?? context.CompanyGuarantorName ?? context.PaymentSourceName,
                BenefitPlanName = context.BenefitPlanName,
                ServiceUnitId = context.ServiceUnitId,
                ClinicId = context.ClinicId,
                PatientClassId = context.PatientClassId,
                InsuranceProviderId = context.InsuranceProviderId ?? context.CompanyGuarantorId,
                BenefitPlanCode = context.BenefitPlanCode,
                StockStorageLocationId = stockLocationId,
                StockStorageLocationName = stockLocationName,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "PrescribingDrug.GetDrugs",
                "Mengambil katalog obat resep berdasarkan encounter.",
                new
                {
                    encounterId,
                    drugCategoryId,
                    isFormulary,
                    isGeneric,
                    isAntibiotic,
                    isHighAlert,
                    isCompoundIngredientAllowed,
                    isConsumable,
                    fromPharmacyStock,
                    stockLocationId,
                    search,
                    sortBy,
                    sortDirection,
                    pageNumber,
                    pageSize,
                    totalData
                }
            );

            return Ok(ApiResponse<PrescribingDrugPagedResponse>.Ok(
                result,
                "Katalog obat resep berhasil diambil."
            ));
        }

        [HttpGet("{drugId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PrescribingDrugDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Read", "Read Prescribing Drug Detail", Description = "Melihat detail obat resep", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PrescribingDrug", "Read")]
        public async Task<IActionResult> GetDrugById(
            Guid drugId,
            [FromQuery] Guid encounterId,
            [FromQuery] decimal quantity = 1,
            CancellationToken cancellationToken = default)
        {
            if (encounterId == Guid.Empty)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "EncounterId wajib diisi."
                ));
            }

            if (drugId == Guid.Empty)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "DrugId wajib diisi."
                ));
            }

            if (quantity <= 0)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Quantity harus lebih dari 0."
                ));
            }

            var context = await _encounterInsuranceService.GetContextAsync(
                encounterId,
                cancellationToken: cancellationToken
            );

            if (!context.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    context.ErrorMessage ?? "Konteks encounter tidak valid."
                ));
            }

            var drug = await BuildDrugQuery()
                .FirstOrDefaultAsync(x => x.Id == drugId, cancellationToken);

            if (drug == null ||
                drug.DrugCode.ToUpper().StartsWith("ALK") ||
                !drug.DrugCode.ToUpper().StartsWith("OBT"))
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Obat tidak ditemukan atau tidak dapat diresepkan."
                ));
            }

            var coverage = await _insuranceCoverageService.ResolveDrugAsync(
                encounterId,
                drug.Id,
                quantity,
                cancellationToken: cancellationToken
            );

            var result = MapDetailResponse(drug, coverage);

            await _loggerService.InfoAsync(
                LogCategory,
                "PrescribingDrug.GetDrugById",
                "Mengambil detail obat resep berdasarkan encounter.",
                new
                {
                    encounterId,
                    drugId,
                    quantity,
                    result.CoverageStatus,
                    result.PricingQuantity,
                    result.UnitPrice,
                    result.TotalPrice,
                    result.CoveredAmount,
                    result.PatientPayAmount
                }
            );

            return Ok(ApiResponse<PrescribingDrugDetailResponse>.Ok(
                result,
                "Detail obat resep berhasil diambil."
            ));
        }

        private IQueryable<MstDrug> BuildDrugQuery()
        {
            return _dbContext.Set<MstDrug>()
                .AsNoTracking()
                .Include(x => x.DrugCategory)
                .Include(x => x.BaseUnitMeasurement)
                .Include(x => x.DispenseUnitMeasurement)
                .Include(x => x.DefaultDoseUnitMeasurement)
                .Where(x =>
                    !x.IsDelete &&
                    x.IsActive &&
                    x.IsPrescribable);
        }

        private static IQueryable<MstDrug> ApplyFilters(
            IQueryable<MstDrug> query,
            Guid? drugCategoryId,
            bool? isFormulary,
            bool? isGeneric,
            bool? isAntibiotic,
            bool? isHighAlert,
            bool? isCompoundIngredientAllowed,
            bool? isConsumable,
            string? search)
        {
            if (drugCategoryId.HasValue && drugCategoryId.Value != Guid.Empty)
                query = query.Where(x => x.DrugCategoryId == drugCategoryId.Value);

            if (isFormulary.HasValue)
                query = query.Where(x => x.IsFormulary == isFormulary.Value);

            if (isGeneric.HasValue)
                query = query.Where(x => x.IsGeneric == isGeneric.Value);

            if (isAntibiotic.HasValue)
                query = query.Where(x => x.IsAntibiotic == isAntibiotic.Value);

            if (isHighAlert.HasValue)
                query = query.Where(x => x.IsHighAlert == isHighAlert.Value);

            if (isCompoundIngredientAllowed.HasValue)
            {
                query = query.Where(x =>
                    x.IsCompoundIngredientAllowed == isCompoundIngredientAllowed.Value);
            }

            if (isConsumable.HasValue && isConsumable.Value)
            {
                query = query.Where(x => x.IsConsumable || x.DrugCode.ToUpper().StartsWith("ALK"));
            }
            else
            {
                // Resep obat dokter: tidak boleh ALK (alat kesehatan), hanya obat (OBT) yang boleh muncul
                query = query.Where(x => !x.IsConsumable && !x.DrugCode.ToUpper().StartsWith("ALK") && x.DrugCode.ToUpper().StartsWith("OBT"));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.DrugCode.ToLower().Contains(keyword) ||
                    x.DrugName.ToLower().Contains(keyword) ||
                    (x.GenericName != null && x.GenericName.ToLower().Contains(keyword)) ||
                    (x.BrandName != null && x.BrandName.ToLower().Contains(keyword)) ||
                    (x.DrugForm != null && x.DrugForm.ToLower().Contains(keyword)) ||
                    (x.Strength != null && x.Strength.ToLower().Contains(keyword)) ||
                    (x.DrugCategory != null &&
                     x.DrugCategory.DrugCategoryName.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IQueryable<MstDrug> ApplySorting(
            IQueryable<MstDrug> query,
            string? sortBy,
            string? sortDirection,
            string? search = null)
        {
            var isDesc = string.Equals(
                sortDirection,
                "desc",
                StringComparison.OrdinalIgnoreCase);

            var normalizedSortBy = (sortBy ?? "drugName").ToLowerInvariant();

            // Jika ada kata kunci pencarian dan pengurutan default (nama obat atau urutan standar),
            // prioritaskan obat yang paling relevan dengan kata kunci nama obat:
            if (!string.IsNullOrWhiteSpace(search) && (normalizedSortBy == "drugname" || normalizedSortBy == "sortorder"))
            {
                var keyword = search.Trim().ToLower();

                query = query.OrderBy(x =>
                    x.DrugName.ToLower() == keyword ? 1 :
                    x.DrugName.ToLower().StartsWith(keyword) ? 2 :
                    x.DrugName.ToLower().Contains(keyword) ? 3 :
                    (x.GenericName != null && x.GenericName.ToLower().StartsWith(keyword)) ? 4 :
                    (x.BrandName != null && x.BrandName.ToLower().Contains(keyword)) ? 5 :
                    (x.GenericName != null && x.GenericName.ToLower().Contains(keyword)) ? 6 :
                    7);

                return isDesc
                    ? ((IOrderedQueryable<MstDrug>)query).ThenByDescending(x => x.DrugName)
                    : ((IOrderedQueryable<MstDrug>)query).ThenBy(x => x.DrugName);
            }

            return normalizedSortBy switch
            {
                "drugcode" => isDesc
                    ? query.OrderByDescending(x => x.DrugCode)
                    : query.OrderBy(x => x.DrugCode),

                "genericname" => isDesc
                    ? query.OrderByDescending(x => x.GenericName)
                    : query.OrderBy(x => x.GenericName),

                "drugcategoryname" => isDesc
                    ? query.OrderByDescending(x => x.DrugCategory != null
                        ? x.DrugCategory.DrugCategoryName
                        : string.Empty)
                    : query.OrderBy(x => x.DrugCategory != null
                        ? x.DrugCategory.DrugCategoryName
                        : string.Empty),

                "isformulary" => isDesc
                    ? query.OrderByDescending(x => x.IsFormulary)
                    : query.OrderBy(x => x.IsFormulary),

                "isgeneric" => isDesc
                    ? query.OrderByDescending(x => x.IsGeneric)
                    : query.OrderBy(x => x.IsGeneric),

                "ishighalert" => isDesc
                    ? query.OrderByDescending(x => x.IsHighAlert)
                    : query.OrderBy(x => x.IsHighAlert),

                "sortorder" => isDesc
                    ? query.OrderByDescending(x => x.SortOrder)
                    : query.OrderBy(x => x.SortOrder),

                _ => isDesc
                    ? query.OrderByDescending(x => x.DrugName)
                    : query.OrderBy(x => x.DrugName)
            };
        }

        private static PrescribingDrugResponse MapResponse(
            MstDrug drug,
            InsuranceCoverageResult coverage)
        {
            var warnings = coverage.Warnings.ToList();

            if (!coverage.IsValid &&
                !string.IsNullOrWhiteSpace(coverage.ErrorMessage))
            {
                warnings.Add(coverage.ErrorMessage);
            }

            var readiness = BuildReadiness(drug, coverage);
            var pricingQuantity = coverage.Quantity > 0 ? coverage.Quantity : 1m;
            var hospitalTotalPrice = RoundMoney(
                coverage.HospitalUnitPrice * pricingQuantity);

            return new PrescribingDrugResponse
            {
                DrugId = drug.Id,
                DrugCode = drug.DrugCode,
                DrugName = drug.DrugName,
                GenericName = drug.GenericName,
                BrandName = drug.BrandName,
                ManufacturerName = drug.ManufacturerName,
                DrugForm = drug.DrugForm,
                Strength = drug.Strength,
                StrengthValue = drug.StrengthValue,
                Route = drug.Route,
                DrugCategoryId = drug.DrugCategoryId,
                DrugCategoryName = drug.DrugCategory?.DrugCategoryName ?? string.Empty,
                BaseUnitMeasurementId = drug.BaseUnitMeasurementId,
                BaseUnitName = drug.BaseUnitMeasurement?.MeasurementName,
                BaseUnitSymbol = drug.BaseUnitMeasurement?.MeasurementSymbol,
                DispenseUnitMeasurementId = drug.DispenseUnitMeasurementId ?? drug.BaseUnitMeasurementId,
                DispenseUnitName = drug.DispenseUnitMeasurement?.MeasurementName ?? drug.BaseUnitMeasurement?.MeasurementName,
                DispenseUnitSymbol = drug.DispenseUnitMeasurement?.MeasurementSymbol ?? drug.BaseUnitMeasurement?.MeasurementSymbol,
                DefaultDoseUnitMeasurementId = drug.DefaultDoseUnitMeasurementId ?? drug.BaseUnitMeasurementId ?? drug.DispenseUnitMeasurementId,
                DefaultDoseUnitName = drug.DefaultDoseUnitMeasurement?.MeasurementName ?? drug.BaseUnitMeasurement?.MeasurementName ?? drug.DispenseUnitMeasurement?.MeasurementName,
                DefaultDoseUnitSymbol = drug.DefaultDoseUnitMeasurement?.MeasurementSymbol ?? drug.BaseUnitMeasurement?.MeasurementSymbol ?? drug.DispenseUnitMeasurement?.MeasurementSymbol,
                IsFormulary = drug.IsFormulary,
                IsGeneric = drug.IsGeneric,
                IsAntibiotic = drug.IsAntibiotic,
                IsNarcotic = drug.IsNarcotic,
                IsPsychotropic = drug.IsPsychotropic,
                IsHighAlert = drug.IsHighAlert,
                IsCompoundIngredientAllowed = drug.IsCompoundIngredientAllowed,
                IsAllowFractionalDispense = drug.IsAllowFractionalDispense,
                IsStockManaged = drug.IsStockManaged,
                IsConsumable = drug.IsConsumable,
                IsNeedPrescription = drug.IsNeedPrescription,
                IsNeedApprovalFromDrug = drug.IsNeedApproval,
                HasTariff = readiness.HasTariff,
                HasDispenseUnit = readiness.HasDispenseUnit,
                HasDoseUnit = readiness.HasDoseUnit,
                HasClinicalInformation = readiness.HasClinicalInformation,
                CanSelect = readiness.CanSelect,
                ReadinessStatus = readiness.Status,
                ReadinessMessages = readiness.Messages,
                CompositionText = BuildCompositionText(drug),
                IsFallbackTariff = coverage.IsFallbackTariff,
                PricingWarning = coverage.PricingWarning,
                TariffId = coverage.TariffId,
                TariffCode = coverage.TariffCode,
                TariffName = coverage.TariffName,
                InsuranceTariffId = coverage.InsuranceTariffId,
                InsuranceCoverageRuleId = coverage.InsuranceCoverageRuleId,
                PaymentTypeName = coverage.PaymentTypeName,
                PricingSource = coverage.PricingSource,
                IsCoverageApplicable = coverage.IsCoverageApplicable,
                IsCovered = coverage.IsCovered,
                CoverageStatus = coverage.CoverageStatus,
                CoveragePercent = coverage.CoveragePercent,
                PricingQuantity = pricingQuantity,
                HospitalUnitPrice = coverage.HospitalUnitPrice,
                HospitalTotalPrice = hospitalTotalPrice,
                ContractUnitPrice = coverage.ContractUnitPrice,
                UnitPrice = coverage.UnitPrice,
                TotalPrice = coverage.TotalPrice,
                CoveredAmount = coverage.CoveredAmount,
                PatientPayAmount = coverage.PatientPayAmount,
                CoPaymentAmount = coverage.CoPaymentAmount,
                IsNeedApproval = coverage.IsNeedApproval || drug.IsNeedApproval,
                IsNeedGuaranteeLetter = coverage.IsNeedGuaranteeLetter,
                CoverageNote = coverage.CoverageNote,
                Warnings = warnings
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList()
            };
        }

        private static PrescribingDrugDetailResponse MapDetailResponse(
            MstDrug drug,
            InsuranceCoverageResult coverage)
        {
            var baseResponse = MapResponse(drug, coverage);

            return new PrescribingDrugDetailResponse
            {
                DrugId = baseResponse.DrugId,
                DrugCode = baseResponse.DrugCode,
                DrugName = baseResponse.DrugName,
                GenericName = baseResponse.GenericName,
                BrandName = baseResponse.BrandName,
                ManufacturerName = baseResponse.ManufacturerName,
                DrugForm = baseResponse.DrugForm,
                Strength = baseResponse.Strength,
                StrengthValue = baseResponse.StrengthValue,
                Route = baseResponse.Route,
                DrugCategoryId = baseResponse.DrugCategoryId,
                DrugCategoryName = baseResponse.DrugCategoryName,
                BaseUnitMeasurementId = baseResponse.BaseUnitMeasurementId,
                BaseUnitName = baseResponse.BaseUnitName,
                BaseUnitSymbol = baseResponse.BaseUnitSymbol,
                DispenseUnitMeasurementId = baseResponse.DispenseUnitMeasurementId,
                DispenseUnitName = baseResponse.DispenseUnitName,
                DispenseUnitSymbol = baseResponse.DispenseUnitSymbol,
                DefaultDoseUnitMeasurementId = baseResponse.DefaultDoseUnitMeasurementId,
                DefaultDoseUnitName = baseResponse.DefaultDoseUnitName,
                DefaultDoseUnitSymbol = baseResponse.DefaultDoseUnitSymbol,
                IsFormulary = baseResponse.IsFormulary,
                IsGeneric = baseResponse.IsGeneric,
                IsAntibiotic = baseResponse.IsAntibiotic,
                IsNarcotic = baseResponse.IsNarcotic,
                IsPsychotropic = baseResponse.IsPsychotropic,
                IsHighAlert = baseResponse.IsHighAlert,
                IsCompoundIngredientAllowed = baseResponse.IsCompoundIngredientAllowed,
                IsAllowFractionalDispense = baseResponse.IsAllowFractionalDispense,
                IsStockManaged = baseResponse.IsStockManaged,
                IsNeedPrescription = baseResponse.IsNeedPrescription,
                IsNeedApprovalFromDrug = baseResponse.IsNeedApprovalFromDrug,
                HasTariff = baseResponse.HasTariff,
                HasDispenseUnit = baseResponse.HasDispenseUnit,
                HasDoseUnit = baseResponse.HasDoseUnit,
                HasClinicalInformation = baseResponse.HasClinicalInformation,
                CanSelect = baseResponse.CanSelect,
                ReadinessStatus = baseResponse.ReadinessStatus,
                ReadinessMessages = baseResponse.ReadinessMessages,
                CompositionText = baseResponse.CompositionText,
                IsFallbackTariff = baseResponse.IsFallbackTariff,
                PricingWarning = baseResponse.PricingWarning,
                TariffId = baseResponse.TariffId,
                TariffCode = baseResponse.TariffCode,
                TariffName = baseResponse.TariffName,
                InsuranceTariffId = baseResponse.InsuranceTariffId,
                InsuranceCoverageRuleId = baseResponse.InsuranceCoverageRuleId,
                PaymentTypeName = baseResponse.PaymentTypeName,
                PricingSource = baseResponse.PricingSource,
                IsCoverageApplicable = baseResponse.IsCoverageApplicable,
                IsCovered = baseResponse.IsCovered,
                CoverageStatus = baseResponse.CoverageStatus,
                CoveragePercent = baseResponse.CoveragePercent,
                PricingQuantity = baseResponse.PricingQuantity,
                HospitalUnitPrice = baseResponse.HospitalUnitPrice,
                HospitalTotalPrice = baseResponse.HospitalTotalPrice,
                ContractUnitPrice = baseResponse.ContractUnitPrice,
                UnitPrice = baseResponse.UnitPrice,
                TotalPrice = baseResponse.TotalPrice,
                CoveredAmount = baseResponse.CoveredAmount,
                PatientPayAmount = baseResponse.PatientPayAmount,
                CoPaymentAmount = baseResponse.CoPaymentAmount,
                IsNeedApproval = baseResponse.IsNeedApproval,
                IsNeedGuaranteeLetter = baseResponse.IsNeedGuaranteeLetter,
                CoverageNote = baseResponse.CoverageNote,
                Warnings = baseResponse.Warnings,
                Indication = drug.Indication,
                Contraindication = drug.Contraindication,
                SideEffect = drug.SideEffect,
                WarningPrecaution = drug.WarningPrecaution,
                DosageInformation = drug.DosageInformation,
                DrugInteraction = drug.DrugInteraction,
                AdministrationInstruction = drug.AdministrationInstruction,
                StorageInstruction = drug.StorageInstruction,
                PregnancyCategory = drug.PregnancyCategory,
                LactationNote = drug.LactationNote,
                PediatricNote = drug.PediatricNote,
                GeriatricNote = drug.GeriatricNote
            };
        }

        private static DrugReadinessResult BuildReadiness(
            MstDrug drug,
            InsuranceCoverageResult coverage)
        {
            var messages = new List<string>();

            var hasTariff = coverage.IsValid && coverage.TariffId.HasValue;
            var effectiveDispenseUnit =
                drug.DispenseUnitMeasurement ?? drug.BaseUnitMeasurement;
            var effectiveDispenseUnitId =
                drug.DispenseUnitMeasurementId ?? drug.BaseUnitMeasurementId;

            var effectiveDoseUnit =
                drug.DefaultDoseUnitMeasurement ?? drug.BaseUnitMeasurement ?? drug.DispenseUnitMeasurement;
            var effectiveDoseUnitId =
                drug.DefaultDoseUnitMeasurementId ?? drug.BaseUnitMeasurementId ?? drug.DispenseUnitMeasurementId;

            var hasDispenseUnit =
                effectiveDispenseUnitId.HasValue &&
                effectiveDispenseUnit != null;
            var hasDoseUnit =
                drug.IsConsumable ||
                (effectiveDoseUnitId.HasValue && effectiveDoseUnit != null);
            var hasClinicalInformation = HasClinicalInformation(drug);

            if (!hasTariff)
            {
                messages.Add(coverage.CoverageStatus == "ConfigurationMissing"
                    ? "Tarif sesuai kelas kamar maupun tarif umum obat belum tersedia. Lengkapi tarif sebelum menyimpan resep."
                    : coverage.ErrorMessage ?? "Tarif obat belum dapat digunakan.");
            }

            if (!hasDispenseUnit)
            {
                messages.Add("Satuan penyerahan obat belum dikonfigurasi pada master obat.");
            }

            if (!hasDoseUnit)
            {
                messages.Add("Satuan dosis default belum dikonfigurasi pada master obat.");
            }

            if (!hasClinicalInformation)
            {
                messages.Add("Informasi klinis obat belum dilengkapi pada master obat.");
            }

            if (coverage.IsFallbackTariff &&
                !string.IsNullOrWhiteSpace(coverage.PricingWarning))
            {
                messages.Add(coverage.PricingWarning);
            }

            var canSelect = hasTariff && hasDispenseUnit && hasDoseUnit;

            return new DrugReadinessResult
            {
                HasTariff = hasTariff,
                HasDispenseUnit = hasDispenseUnit,
                HasDoseUnit = hasDoseUnit,
                HasClinicalInformation = hasClinicalInformation,
                CanSelect = canSelect,
                Status = canSelect ? "Ready" : "ConfigurationIncomplete",
                Messages = messages
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList()
            };
        }

        /// <summary>
        /// Menempelkan stok depo pada satu baris katalog dan menutup pemilihan bila stoknya habis.
        /// </summary>
        /// <remarks>
        /// Pesan stok habis ditaruh paling depan karena layar dokter menampilkan pesan kesiapan
        /// pertama sebagai alasan tombol Pilih nonaktif.
        /// </remarks>
        private static void ApplyStock(
            PrescribingDrugResponse item,
            Guid storageLocationId,
            string? storageLocationName,
            decimal availableQuantity)
        {
            item.StockStorageLocationId = storageLocationId;
            item.StockStorageLocationName = storageLocationName;
            item.AvailableStockQuantity = availableQuantity;
            item.IsStockAvailable = availableQuantity > 0;

            if (availableQuantity > 0)
                return;

            item.CanSelect = false;
            item.ReadinessMessages.Insert(
                0,
                $"Stok obat di {storageLocationName ?? "depo Farmasi"} habis.");

            if (item.ReadinessStatus == "Ready")
                item.ReadinessStatus = "OutOfStock";
        }

        private static bool HasClinicalInformation(MstDrug drug)
        {
            return new[]
            {
                drug.Indication,
                drug.Contraindication,
                drug.SideEffect,
                drug.WarningPrecaution,
                drug.DosageInformation,
                drug.DrugInteraction,
                drug.AdministrationInstruction,
                drug.StorageInstruction,
                drug.PregnancyCategory,
                drug.LactationNote,
                drug.PediatricNote,
                drug.GeriatricNote
            }.Any(x => !string.IsNullOrWhiteSpace(x));
        }

        private static string? BuildCompositionText(MstDrug drug)
        {
            var values = new[]
            {
                drug.GenericName,
                drug.Strength
            }
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

            return values.Count == 0 ? null : string.Join(" ", values);
        }

        private sealed class DrugReadinessResult
        {
            public bool HasTariff { get; set; }
            public bool HasDispenseUnit { get; set; }
            public bool HasDoseUnit { get; set; }
            public bool HasClinicalInformation { get; set; }
            public bool CanSelect { get; set; }
            public string Status { get; set; } = string.Empty;
            public List<string> Messages { get; set; } = new();
        }

        private static List<PrescribingDrugQueryParameterResponse>
            BuildQueryParameterInfo()
        {
            return new List<PrescribingDrugQueryParameterResponse>
            {
                new()
                {
                    Name = "encounterId",
                    Type = "Guid",
                    IsRequired = true,
                    Description = "Encounter pasien sebagai dasar payment dan coverage."
                },
                new()
                {
                    Name = "drugCategoryId",
                    Type = "Guid?",
                    IsRequired = false,
                    Description = "Filter berdasarkan kategori obat."
                },
                new()
                {
                    Name = "isFormulary",
                    Type = "bool?",
                    IsRequired = false,
                    Description = "Filter status formularium."
                },
                new()
                {
                    Name = "isGeneric",
                    Type = "bool?",
                    IsRequired = false,
                    Description = "Filter obat generik."
                },
                new()
                {
                    Name = "isAntibiotic",
                    Type = "bool?",
                    IsRequired = false,
                    Description = "Filter antibiotik."
                },
                new()
                {
                    Name = "isHighAlert",
                    Type = "bool?",
                    IsRequired = false,
                    Description = "Filter obat high alert."
                },
                new()
                {
                    Name = "isCompoundIngredientAllowed",
                    Type = "bool?",
                    IsRequired = false,
                    Description = "Filter obat yang dapat digunakan sebagai bahan racikan."
                },
                new()
                {
                    Name = "isConsumable",
                    Type = "bool?",
                    IsRequired = false,
                    Description = "Filter barang habis pakai / alkes. Default false untuk peresepan obat."
                },
                new()
                {
                    Name = "fromPharmacyStock",
                    Type = "bool?",
                    IsRequired = false,
                    Description = "Bila true, hanya obat yang punya saldo di depo Farmasi hasil routing encounter, beserta stok tersedia per obat."
                },
                new()
                {
                    Name = "search",
                    Type = "string?",
                    IsRequired = false,
                    Description = "Pencarian kode, nama, generik, merek, bentuk, kekuatan, atau kategori obat."
                },
                new()
                {
                    Name = "sortBy",
                    Type = "string?",
                    IsRequired = false,
                    Description = "Kolom sorting."
                },
                new()
                {
                    Name = "sortDirection",
                    Type = "string?",
                    IsRequired = false,
                    Description = "Arah sorting: asc atau desc."
                },
                new()
                {
                    Name = "pageNumber",
                    Type = "int",
                    IsRequired = false,
                    Description = "Nomor halaman."
                },
                new()
                {
                    Name = "pageSize",
                    Type = "int",
                    IsRequired = false,
                    Description = "Jumlah data per halaman."
                }
            };
        }

        private static decimal RoundMoney(decimal value)
        {
            return Math.Round(value, 2, MidpointRounding.AwayFromZero);
        }

        private static (int PageNumber, int PageSize) NormalizePaging(
            int pageNumber,
            int pageSize)
        {
            if (pageNumber <= 0)
                pageNumber = 1;

            if (pageSize <= 0)
                pageSize = 25;

            if (pageSize > 100)
                pageSize = 100;

            return (pageNumber, pageSize);
        }
    }
}
