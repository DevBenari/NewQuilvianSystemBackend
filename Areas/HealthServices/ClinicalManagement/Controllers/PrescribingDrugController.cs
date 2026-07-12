using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
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
        private readonly LoggerService _loggerService;

        public PrescribingDrugController(
            ApplicationDbContext dbContext,
            EncounterInsuranceService encounterInsuranceService,
            InsuranceCoverageService insuranceCoverageService,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _encounterInsuranceService = encounterInsuranceService;
            _insuranceCoverageService = insuranceCoverageService;
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

            var query = ApplyFilters(
                BuildDrugQuery(),
                drugCategoryId,
                isFormulary,
                isGeneric,
                isAntibiotic,
                isHighAlert,
                isCompoundIngredientAllowed,
                search
            );

            var totalData = await query.CountAsync(cancellationToken);

            var drugs = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var items = new List<PrescribingDrugResponse>();

            foreach (var drug in drugs)
            {
                var coverage = await _insuranceCoverageService.ResolveDrugAsync(
                    encounterId,
                    drug.Id,
                    quantity: 1,
                    cancellationToken: cancellationToken
                );

                items.Add(MapResponse(drug, coverage));
            }

            var result = new PrescribingDrugPagedResponse
            {
                EncounterId = encounterId,
                PaymentTypeName = context.PaymentTypeName,
                InsuranceProviderName = context.InsuranceProviderName,
                BenefitPlanName = context.BenefitPlanName,
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

            if (drug == null)
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
                    result.UnitPrice,
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
            string? sortDirection)
        {
            var isDesc = string.Equals(
                sortDirection,
                "desc",
                StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "drugName").ToLowerInvariant() switch
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
                DispenseUnitMeasurementId = drug.DispenseUnitMeasurementId,
                DispenseUnitName = drug.DispenseUnitMeasurement?.MeasurementName,
                DispenseUnitSymbol = drug.DispenseUnitMeasurement?.MeasurementSymbol,
                DefaultDoseUnitMeasurementId = drug.DefaultDoseUnitMeasurementId,
                DefaultDoseUnitName = drug.DefaultDoseUnitMeasurement?.MeasurementName,
                DefaultDoseUnitSymbol = drug.DefaultDoseUnitMeasurement?.MeasurementSymbol,
                IsFormulary = drug.IsFormulary,
                IsGeneric = drug.IsGeneric,
                IsAntibiotic = drug.IsAntibiotic,
                IsNarcotic = drug.IsNarcotic,
                IsPsychotropic = drug.IsPsychotropic,
                IsHighAlert = drug.IsHighAlert,
                IsCompoundIngredientAllowed = drug.IsCompoundIngredientAllowed,
                IsAllowFractionalDispense = drug.IsAllowFractionalDispense,
                IsStockManaged = drug.IsStockManaged,
                IsNeedPrescription = drug.IsNeedPrescription,
                IsNeedApprovalFromDrug = drug.IsNeedApproval,
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
                HospitalUnitPrice = coverage.HospitalUnitPrice,
                ContractUnitPrice = coverage.ContractUnitPrice,
                UnitPrice = coverage.UnitPrice,
                CoveredAmount = coverage.CoveredAmount,
                PatientPayAmount = coverage.PatientPayAmount,
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
                HospitalUnitPrice = baseResponse.HospitalUnitPrice,
                ContractUnitPrice = baseResponse.ContractUnitPrice,
                UnitPrice = baseResponse.UnitPrice,
                CoveredAmount = baseResponse.CoveredAmount,
                PatientPayAmount = baseResponse.PatientPayAmount,
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
