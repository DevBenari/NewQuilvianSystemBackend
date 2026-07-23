using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponsePatientInsurancePagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs.PatientInsuranceResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/patient-management/master-data/patient-insurances")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_PATIENT_MANAGEMENT_MASTER_DATA",
        moduleName: "Health Service Patient Management Master Data",
        displayName: "Patient Insurance",
        AreaName = "HealthServices",
        ControllerName = "PatientInsurance",
        Description = "Health service patient management master data patient insurance",
        SortOrder = 14
    )]
    [Tags("Health Services / Patient Management / Master Data / Patient Insurance")]
    public class PatientInsuranceController : ControllerBase
    {
        private const string LogCategory = "HealthServices.PatientManagement.MasterData";
        private const string KioskReadPolicy = "KioskRead";

        private static readonly HashSet<string> AllowedHolderRelationships = new(StringComparer.OrdinalIgnoreCase)
        {
            "Self",
            "Spouse",
            "Child",
            "Parent",
            "Sibling",
            "Guardian",
            "Employee",
            "Other"
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public PatientInsuranceController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [HttpGet("kiosk/filters/metadata")]
        [Authorize(Policy = KioskReadPolicy)]
        [ProducesResponseType(typeof(ApiResponse<PatientInsuranceFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Patient Insurance",
            Description = "Melihat metadata filter patient insurance",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new PatientInsuranceFilterMetadataResponse
            {
                DefaultFilter = new PatientInsuranceDefaultFilterResponse(),
                CustomPeriods = new List<PatientInsuranceCustomPeriodOptionResponse>
                {
                    new() { Value = "today", Label = "Hari ini" },
                    new() { Value = "last7days", Label = "7 hari terakhir" },
                    new() { Value = "thismonth", Label = "Bulan ini" },
                    new() { Value = "lastmonth", Label = "Bulan lalu" }
                },
                RelationFilters = new List<PatientInsuranceRelationFilterResponse>
                {
                    new()
                    {
                        Value = "patientId",
                        Label = "Patient",
                        Endpoint = "/api/v1/health-services/patient-management/master-data/patients/options"
                    },
                    new()
                    {
                        Value = "insuranceProviderId",
                        Label = "Insurance Provider",
                        Endpoint = "/api/v1/administrator/master-data/insurance-providers/options"
                    }
                },
                SortOptions = new List<PatientInsuranceSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "updateDateTime", Label = "Tanggal diperbarui" },
                    new() { Value = "patientName", Label = "Nama pasien" },
                    new() { Value = "medicalRecordNumber", Label = "Nomor rekam medis" },
                    new() { Value = "insuranceProviderName", Label = "Nama insurance provider" },
                    new() { Value = "policyNumber", Label = "Nomor polis" },
                    new() { Value = "cardNumber", Label = "Nomor kartu" },
                    new() { Value = "memberNumber", Label = "Nomor member" },
                    new() { Value = "planName", Label = "Nama plan" },
                    new() { Value = "className", Label = "Kelas" },
                    new() { Value = "effectiveStartDate", Label = "Tanggal mulai berlaku" },
                    new() { Value = "effectiveEndDate", Label = "Tanggal akhir berlaku" },
                    new() { Value = "isPrimary", Label = "Primary" },
                    new() { Value = "isEligible", Label = "Eligible" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                HolderRelationships = AllowedHolderRelationships.OrderBy(x => x).ToList(),
                ResetButtonLabel = "Reset"
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "PatientInsurance.GetFilterMetadata",
                "Mengambil metadata filter patient insurance.",
                result
            );

            return Ok(ApiResponse<PatientInsuranceFilterMetadataResponse>.Ok(
                result,
                "Metadata filter patient insurance berhasil diambil."
            ));
        }


        [HttpGet("admin/filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<PatientInsuranceFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessPermission("PatientInsurance", "Read")]
        public async Task<IActionResult> GetFilterMetadataForAdmin()
        {
            var actionResult = await GetFilterMetadata();

            if (actionResult is OkObjectResult okResult &&
                okResult.Value is ApiResponse<PatientInsuranceFilterMetadataResponse> response &&
                response.Data != null)
            {
                foreach (var relationFilter in response.Data.RelationFilters)
                {
                    if (string.Equals(relationFilter.Value, "patientId", StringComparison.OrdinalIgnoreCase))
                    {
                        relationFilter.Endpoint = "/api/v1/health-services/patient-management/master-data/patients/admin/options";
                    }
                    else if (string.Equals(relationFilter.Value, "insuranceProviderId", StringComparison.OrdinalIgnoreCase))
                    {
                        relationFilter.Endpoint = "/api/v1/administrator/master-data/insurance-providers/admin/options";
                    }
                }
            }

            return actionResult;
        }

        [HttpGet("summary")]
        [HttpGet("admin/summary")]
        [ProducesResponseType(typeof(ApiResponse<PatientInsuranceSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Patient Insurance",
            Description = "Melihat ringkasan patient insurance",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("PatientInsurance", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var today = AppDateTimeHelper.OperationalDate();
            var query = BuildBaseQuery();

            var result = new PatientInsuranceSummaryResponse
            {
                TotalPatientInsurance = await query.CountAsync(),
                ActivePatientInsurance = await query.CountAsync(x => x.IsActive),
                InactivePatientInsurance = await query.CountAsync(x => !x.IsActive),
                PrimaryPatientInsurance = await query.CountAsync(x => x.IsPrimary),
                EligiblePatientInsurance = await query.CountAsync(x => x.IsEligible),
                IneligiblePatientInsurance = await query.CountAsync(x => !x.IsEligible),
                NeedGuaranteeLetterPatientInsurance = await query.CountAsync(x => x.IsNeedGuaranteeLetter),
                NeedReferralLetterPatientInsurance = await query.CountAsync(x => x.IsNeedReferralLetter),
                AllowExcessPaymentByPatientInsurance = await query.CountAsync(x => x.IsAllowExcessPaymentByPatient),
                EffectivePatientInsurance = await query.CountAsync(x =>
                    (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value.Date <= today) &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= today)),
                ExpiredPatientInsurance = await query.CountAsync(x =>
                    x.EffectiveEndDate.HasValue &&
                    x.EffectiveEndDate.Value.Date < today),
                WithAnnualLimitPatientInsurance = await query.CountAsync(x => x.AnnualLimitAmount.HasValue),
                WithRemainingLimitPatientInsurance = await query.CountAsync(x => x.RemainingLimitAmount.HasValue),
                WithCoPaymentPatientInsurance = await query.CountAsync(x =>
                    x.CoPaymentPercent.HasValue ||
                    x.CoPaymentAmount.HasValue),
                WithCardImagePatientInsurance = await query.CountAsync(x =>
                    x.CardImagePath != null &&
                    x.CardImagePath != string.Empty)
            };

            return Ok(ApiResponse<PatientInsuranceSummaryResponse>.Ok(
                result,
                "Ringkasan patient insurance berhasil diambil."
            ));
        }

        [HttpGet]
        [HttpGet("kiosk")]
        [Authorize(Policy = KioskReadPolicy)]
        [ProducesResponseType(typeof(ApiResponse<ResponsePatientInsurancePagedResult>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Patient Insurance",
            Description = "Melihat data patient insurance",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        public async Task<IActionResult> GetPatientInsurances(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? patientId,
            [FromQuery] Guid? insuranceProviderId,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "createDateTime",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();

            query = ApplyDateFilter(query, startDate, endDate, customPeriod);
            query = ApplyRelationFilter(query, patientId, insuranceProviderId);
            query = ApplyStandardFilter(query, isActive, search);

            var totalData = await query.CountAsync();

            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var actorNames = await GetActorNameMapAsync(
                entities
                    .SelectMany(x => new[] { x.CreateBy, x.UpdateBy })
                    .Where(x => x != Guid.Empty)
            );

            var items = entities
                .Select(x => MapResponse(x, actorNames))
                .ToList();

            var result = new ResponsePatientInsurancePagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponsePatientInsurancePagedResult>.Ok(
                result,
                "Data patient insurance berhasil diambil."
            ));
        }


        [HttpGet("admin")]
        [ProducesResponseType(typeof(ApiResponse<ResponsePatientInsurancePagedResult>), StatusCodes.Status200OK)]
        [AccessPermission("PatientInsurance", "Read")]
        public async Task<IActionResult> GetPatientInsurancesForAdmin(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? patientId,
            [FromQuery] Guid? insuranceProviderId,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "createDateTime",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            return await GetPatientInsurances(
                startDate,
                endDate,
                customPeriod,
                patientId,
                insuranceProviderId,
                isActive,
                search,
                sortBy,
                sortDirection,
                pageNumber,
                pageSize
            );
        }

        [HttpGet("options")]
        [HttpGet("kiosk/options")]
        [Authorize(Policy = KioskReadPolicy)]
        [ProducesResponseType(typeof(ApiResponse<PatientInsuranceOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Patient Insurance",
            Description = "Melihat data pilihan patient insurance",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        public async Task<IActionResult> GetPatientInsuranceOptions(
            [FromQuery] bool onlyActive = true,
            [FromQuery] bool onlyUsable = false,
            [FromQuery] DateTime? serviceDate = null,
            [FromQuery] Guid? patientId = null,
            [FromQuery] Guid? insuranceProviderId = null,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();

            query = ApplyRelationFilter(query, patientId, insuranceProviderId);
            query = ApplyStandardFilter(
                query,
                onlyActive ? true : null,
                search
            );

            if (onlyUsable)
            {
                var effectiveDate = (serviceDate ?? AppDateTimeHelper.OperationalDate()).Date;

                query = query.Where(x =>
                    x.IsActive &&
                    x.IsEligible &&
                    x.Patient != null &&
                    x.Patient.IsActive &&
                    !x.Patient.IsDelete &&
                    x.InsuranceProvider != null &&
                    x.InsuranceProvider.IsActive &&
                    !x.InsuranceProvider.IsDelete &&
                    (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value.Date <= effectiveDate) &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= effectiveDate) &&
                    (!x.InsuranceProvider.ContractStartDate.HasValue || x.InsuranceProvider.ContractStartDate.Value.Date <= effectiveDate) &&
                    (!x.InsuranceProvider.ContractEndDate.HasValue || x.InsuranceProvider.ContractEndDate.Value.Date >= effectiveDate));
            }

            var totalData = await query.CountAsync();

            var entities = await query
                .OrderByDescending(x => x.IsPrimary)
                .ThenBy(x => x.Patient != null ? x.Patient.FullName : string.Empty)
                .ThenBy(x => x.InsuranceProvider != null ? x.InsuranceProvider.InsuranceProviderName : string.Empty)
                .ThenBy(x => x.PolicyNumber)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = entities
                .Select(MapOptionResponse)
                .ToList();

            var result = new PatientInsuranceOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<PatientInsuranceOptionPagedResponse>.Ok(
                result,
                "Data pilihan patient insurance berhasil diambil."
            ));
        }


        [HttpGet("admin/options")]
        [ProducesResponseType(typeof(ApiResponse<PatientInsuranceOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessPermission("PatientInsurance", "Read")]
        public async Task<IActionResult> GetPatientInsuranceOptionsForAdmin(
            [FromQuery] bool onlyActive = true,
            [FromQuery] bool onlyUsable = false,
            [FromQuery] DateTime? serviceDate = null,
            [FromQuery] Guid? patientId = null,
            [FromQuery] Guid? insuranceProviderId = null,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            return await GetPatientInsuranceOptions(
                onlyActive,
                onlyUsable,
                serviceDate,
                patientId,
                insuranceProviderId,
                search,
                pageNumber,
                pageSize
            );
        }

        [HttpGet("{id:guid}")]
        [HttpGet("admin/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientInsuranceDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Patient Insurance",
            Description = "Melihat detail patient insurance",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("PatientInsurance", "Read")]
        public async Task<IActionResult> GetPatientInsuranceById(Guid id)
        {
            var entity = await BuildBaseQuery()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Patient insurance tidak ditemukan."
                ));
            }

            var actorNames = await GetActorNameMapAsync(new[]
            {
                entity.CreateBy,
                entity.UpdateBy
            });

            var data = MapDetailResponse(entity, actorNames);

            NormalizeActorInfo(data);

            return Ok(ApiResponse<PatientInsuranceDetailResponse>.Ok(
                data,
                "Detail patient insurance berhasil diambil."
            ));
        }

        [HttpPost]
        [HttpPost("kiosk")]
        [Authorize(Policy = KioskReadPolicy)]
        [ProducesResponseType(typeof(ApiResponse<PatientInsuranceCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(
            "Create",
            "Create Patient Insurance",
            Description = "Membuat data patient insurance",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        public async Task<IActionResult> CreatePatientInsurance(
            [FromBody] CreatePatientInsuranceRequest request)
        {
            var validation = await ValidateRequestAsync(
                excludeId: null,
                request: request
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data patient insurance tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                if (request.IsPrimary)
                {
                    await UnsetOtherPrimaryAsync(
                        patientId: request.PatientId,
                        exceptId: null,
                        now: now,
                        actorUserId: actorUserId
                    );
                }

                var entity = new MstPatientInsurance
                {
                    Id = Guid.NewGuid(),
                    PatientId = request.PatientId,
                    InsuranceProviderId = request.InsuranceProviderId,
                    PolicyNumber = request.PolicyNumber.Trim(),
                    CardNumber = NormalizeNullableString(request.CardNumber),
                    MemberNumber = NormalizeNullableString(request.MemberNumber),
                    PlanName = NormalizeNullableString(request.PlanName),
                    ClassName = NormalizeNullableString(request.ClassName),
                    BenefitPlanCode = NormalizeNullableString(request.BenefitPlanCode),
                    HolderName = NormalizeNullableString(request.HolderName),
                    HolderRelationship = NormalizeNullableString(request.HolderRelationship),
                    EffectiveStartDate = request.EffectiveStartDate,
                    EffectiveEndDate = request.EffectiveEndDate,
                    IsPrimary = request.IsPrimary,
                    IsEligible = request.IsEligible,
                    LastEligibilityCheckAt = request.LastEligibilityCheckAt,
                    LastEligibilityReferenceNumber = NormalizeNullableString(request.LastEligibilityReferenceNumber),
                    EligibilityNote = NormalizeNullableString(request.EligibilityNote),
                    AnnualLimitAmount = request.AnnualLimitAmount,
                    RemainingLimitAmount = request.RemainingLimitAmount,
                    CoPaymentPercent = request.CoPaymentPercent,
                    CoPaymentAmount = request.CoPaymentAmount,
                    IsNeedGuaranteeLetter = request.IsNeedGuaranteeLetter,
                    IsNeedReferralLetter = request.IsNeedReferralLetter,
                    IsAllowExcessPaymentByPatient = request.IsAllowExcessPaymentByPatient,
                    CardImagePath = NormalizeNullableString(request.CardImagePath),
                    Notes = NormalizeNullableString(request.Notes),
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                _dbContext.Set<MstPatientInsurance>().Add(entity);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                var result = await BuildCreateResponseAsync(entity.Id);

                await _loggerService.InfoAsync(
                    LogCategory,
                    "PatientInsurance.CreatePatientInsurance",
                    "Membuat data patient insurance.",
                    result
                );

                return Ok(ApiResponse<PatientInsuranceCreateResponse>.Ok(
                    result,
                    "Patient insurance berhasil dibuat."
                ));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "PatientInsurance.CreatePatientInsurance",
                    "Gagal membuat data patient insurance.",
                    ex
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat membuat patient insurance."
                    )
                );
            }
        }


        [HttpPost("admin")]
        [ProducesResponseType(typeof(ApiResponse<PatientInsuranceCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessPermission("PatientInsurance", "Create")]
        public async Task<IActionResult> CreatePatientInsuranceForAdmin(
            [FromBody] CreatePatientInsuranceRequest request)
        {
            return await CreatePatientInsurance(request);
        }


        [HttpPatch("kiosk/{id:guid}/primary")]
        [Authorize(Policy = KioskReadPolicy)]
        [ProducesResponseType(typeof(ApiResponse<PatientInsuranceCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SetPatientInsurancePrimaryFromKiosk(Guid id)
        {
            var entity = await _dbContext.Set<MstPatientInsurance>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Patient insurance tidak ditemukan."
                ));
            }

            if (!entity.IsActive)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Patient insurance tidak aktif dan tidak dapat dijadikan utama."
                ));
            }

            var patientIsActive = await _dbContext.Set<MstPatient>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == entity.PatientId &&
                    x.IsActive &&
                    !x.IsDelete);

            if (!patientIsActive)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Patient tidak valid atau tidak aktif."
                ));
            }

            var providerIsActive = await _dbContext.Set<MstInsuranceProvider>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == entity.InsuranceProviderId &&
                    x.IsActive &&
                    !x.IsDelete);

            if (!providerIsActive)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Insurance provider tidak valid atau tidak aktif."
                ));
            }

            if (entity.IsPrimary)
            {
                var currentResult = await BuildCreateResponseAsync(entity.Id);

                return Ok(ApiResponse<PatientInsuranceCreateResponse>.Ok(
                    currentResult,
                    "Patient insurance sudah menjadi asuransi utama."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                await UnsetOtherPrimaryAsync(
                    patientId: entity.PatientId,
                    exceptId: entity.Id,
                    now: now,
                    actorUserId: actorUserId
                );

                entity.IsPrimary = true;
                entity.UpdateDateTime = now;
                entity.UpdateBy = actorUserId;

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                var result = await BuildCreateResponseAsync(entity.Id);

                await _loggerService.InfoAsync(
                    LogCategory,
                    "PatientInsurance.SetPatientInsurancePrimaryFromKiosk",
                    "Mengubah asuransi utama pasien dari kiosk.",
                    result
                );

                return Ok(ApiResponse<PatientInsuranceCreateResponse>.Ok(
                    result,
                    "Asuransi utama pasien berhasil diperbarui."
                ));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "PatientInsurance.SetPatientInsurancePrimaryFromKiosk",
                    "Gagal mengubah asuransi utama pasien dari kiosk.",
                    ex
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat mengubah asuransi utama pasien."
                    )
                );
            }
        }

        [HttpPut("{id:guid}")]
        [HttpPut("admin/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(
            "Update",
            "Update Patient Insurance",
            Description = "Mengubah data patient insurance",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("PatientInsurance", "Update")]
        public async Task<IActionResult> UpdatePatientInsurance(
            Guid id,
            [FromBody] UpdatePatientInsuranceRequest request)
        {
            var entity = await _dbContext.Set<MstPatientInsurance>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Patient insurance tidak ditemukan."
                ));
            }

            if (request.IsPrimary && !request.IsActive)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Patient insurance primary harus aktif."
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
                    validation.ErrorMessage ?? "Data patient insurance tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                if (request.IsPrimary)
                {
                    await UnsetOtherPrimaryAsync(
                        patientId: request.PatientId,
                        exceptId: id,
                        now: now,
                        actorUserId: actorUserId
                    );
                }

                entity.PatientId = request.PatientId;
                entity.InsuranceProviderId = request.InsuranceProviderId;
                entity.PolicyNumber = request.PolicyNumber.Trim();
                entity.CardNumber = NormalizeNullableString(request.CardNumber);
                entity.MemberNumber = NormalizeNullableString(request.MemberNumber);
                entity.PlanName = NormalizeNullableString(request.PlanName);
                entity.ClassName = NormalizeNullableString(request.ClassName);
                entity.BenefitPlanCode = NormalizeNullableString(request.BenefitPlanCode);
                entity.HolderName = NormalizeNullableString(request.HolderName);
                entity.HolderRelationship = NormalizeNullableString(request.HolderRelationship);
                entity.EffectiveStartDate = request.EffectiveStartDate;
                entity.EffectiveEndDate = request.EffectiveEndDate;
                entity.IsPrimary = request.IsPrimary;
                entity.IsEligible = request.IsEligible;
                entity.LastEligibilityCheckAt = request.LastEligibilityCheckAt;
                entity.LastEligibilityReferenceNumber = NormalizeNullableString(request.LastEligibilityReferenceNumber);
                entity.EligibilityNote = NormalizeNullableString(request.EligibilityNote);
                entity.AnnualLimitAmount = request.AnnualLimitAmount;
                entity.RemainingLimitAmount = request.RemainingLimitAmount;
                entity.CoPaymentPercent = request.CoPaymentPercent;
                entity.CoPaymentAmount = request.CoPaymentAmount;
                entity.IsNeedGuaranteeLetter = request.IsNeedGuaranteeLetter;
                entity.IsNeedReferralLetter = request.IsNeedReferralLetter;
                entity.IsAllowExcessPaymentByPatient = request.IsAllowExcessPaymentByPatient;
                entity.CardImagePath = NormalizeNullableString(request.CardImagePath);
                entity.Notes = NormalizeNullableString(request.Notes);
                entity.IsActive = request.IsActive;
                entity.UpdateDateTime = now;
                entity.UpdateBy = actorUserId;

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                await _loggerService.InfoAsync(
                    LogCategory,
                    "PatientInsurance.UpdatePatientInsurance",
                    "Mengubah data patient insurance.",
                    new
                    {
                        entity.Id,
                        entity.PatientId,
                        entity.InsuranceProviderId,
                        entity.PolicyNumber,
                        entity.IsPrimary,
                        entity.IsEligible,
                        entity.IsActive
                    }
                );

                return Ok(ApiResponse<object>.Ok(
                    null,
                    "Patient insurance berhasil diperbarui."
                ));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "PatientInsurance.UpdatePatientInsurance",
                    "Gagal mengubah data patient insurance.",
                    ex
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat mengubah patient insurance."
                    )
                );
            }
        }

        [HttpPatch("{id:guid}/status")]
        [HttpPatch("admin/{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Patient Insurance Status",
            Description = "Mengubah status patient insurance",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("PatientInsurance", "Update")]
        public async Task<IActionResult> UpdatePatientInsuranceStatus(
            Guid id,
            [FromBody] UpdatePatientInsuranceStatusRequest request)
        {
            var entity = await _dbContext.Set<MstPatientInsurance>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Patient insurance tidak ditemukan."
                ));
            }

            if (!request.IsActive && entity.IsPrimary)
            {
                entity.IsPrimary = false;
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status patient insurance berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [HttpDelete("admin/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Delete",
            "Delete Patient Insurance",
            Description = "Menghapus data patient insurance",
            AccessType = AccessTypes.Delete,
            SortOrder = 4
        )]
        [AccessPermission("PatientInsurance", "Delete")]
        public async Task<IActionResult> DeletePatientInsurance(
            Guid id,
            [FromBody] DeletePatientInsuranceRequest? request = null)
        {
            var entity = await _dbContext.Set<MstPatientInsurance>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Patient insurance tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.IsPrimary = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            if (!string.IsNullOrWhiteSpace(request?.DeleteReason))
            {
                entity.Notes = request.DeleteReason.Trim();
            }

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "PatientInsurance.DeletePatientInsurance",
                "Menghapus data patient insurance.",
                new
                {
                    entity.Id,
                    entity.PatientId,
                    entity.InsuranceProviderId,
                    entity.PolicyNumber,
                    entity.DeleteDateTime
                }
            );

            return Ok(ApiResponse<object>.Ok(
                null,
                "Patient insurance berhasil dihapus."
            ));
        }

        private IQueryable<MstPatientInsurance> BuildBaseQuery()
        {
            return _dbContext.Set<MstPatientInsurance>()
                .AsNoTracking()
                .Include(x => x.Patient)
                .Include(x => x.InsuranceProvider)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstPatientInsurance> ApplyDateFilter(
            IQueryable<MstPatientInsurance> query,
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

            if (!startDate.HasValue &&
                !endDate.HasValue &&
                !string.IsNullOrWhiteSpace(customPeriod))
            {
                var today = AppDateTimeHelper.OperationalDate();

                switch (customPeriod.Trim().ToLowerInvariant())
                {
                    case "today":
                        query = query.Where(x =>
                            x.CreateDateTime >= today &&
                            x.CreateDateTime < today.AddDays(1));
                        break;

                    case "last7days":
                        query = query.Where(x =>
                            x.CreateDateTime >= today.AddDays(-6) &&
                            x.CreateDateTime < today.AddDays(1));
                        break;

                    case "thismonth":
                        var thisMonthStart = new DateTime(
                            today.Year,
                            today.Month,
                            1,
                            0,
                            0,
                            0,
                            DateTimeKind.Utc
                        );

                        query = query.Where(x =>
                            x.CreateDateTime >= thisMonthStart &&
                            x.CreateDateTime < thisMonthStart.AddMonths(1));
                        break;

                    case "lastmonth":
                        var currentMonthStart = new DateTime(
                            today.Year,
                            today.Month,
                            1,
                            0,
                            0,
                            0,
                            DateTimeKind.Utc
                        );

                        var lastMonthStart = currentMonthStart.AddMonths(-1);

                        query = query.Where(x =>
                            x.CreateDateTime >= lastMonthStart &&
                            x.CreateDateTime < currentMonthStart);
                        break;
                }
            }

            return query;
        }

        private static IQueryable<MstPatientInsurance> ApplyRelationFilter(
            IQueryable<MstPatientInsurance> query,
            Guid? patientId,
            Guid? insuranceProviderId)
        {
            var normalizedPatientId = NormalizeNullableGuid(patientId);

            if (normalizedPatientId.HasValue)
            {
                query = query.Where(x => x.PatientId == normalizedPatientId.Value);
            }

            var normalizedInsuranceProviderId = NormalizeNullableGuid(insuranceProviderId);

            if (normalizedInsuranceProviderId.HasValue)
            {
                query = query.Where(x => x.InsuranceProviderId == normalizedInsuranceProviderId.Value);
            }

            return query;
        }

        private static IQueryable<MstPatientInsurance> ApplyStandardFilter(
            IQueryable<MstPatientInsurance> query,
            bool? isActive,
            string? search)
        {
            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.PolicyNumber.ToLower().Contains(keyword) ||
                    (x.CardNumber != null && x.CardNumber.ToLower().Contains(keyword)) ||
                    (x.MemberNumber != null && x.MemberNumber.ToLower().Contains(keyword)) ||
                    (x.PlanName != null && x.PlanName.ToLower().Contains(keyword)) ||
                    (x.ClassName != null && x.ClassName.ToLower().Contains(keyword)) ||
                    (x.BenefitPlanCode != null && x.BenefitPlanCode.ToLower().Contains(keyword)) ||
                    (x.HolderName != null && x.HolderName.ToLower().Contains(keyword)) ||
                    (x.HolderRelationship != null && x.HolderRelationship.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.PatientCode.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.MedicalRecordNumber.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.FullName.ToLower().Contains(keyword)) ||
                    (x.InsuranceProvider != null && x.InsuranceProvider.InsuranceProviderCode.ToLower().Contains(keyword)) ||
                    (x.InsuranceProvider != null && x.InsuranceProvider.InsuranceProviderName.ToLower().Contains(keyword)));
            }

            return query;
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            CreatePatientInsuranceRequest request)
        {
            if (request.PatientId == Guid.Empty)
            {
                return (false, "Patient wajib dipilih.");
            }

            if (request.InsuranceProviderId == Guid.Empty)
            {
                return (false, "Insurance provider wajib dipilih.");
            }

            if (string.IsNullOrWhiteSpace(request.PolicyNumber))
            {
                return (false, "Nomor polis wajib diisi.");
            }

            var holderRelationship = NormalizeNullableString(request.HolderRelationship);

            if (!string.IsNullOrWhiteSpace(holderRelationship) &&
                !AllowedHolderRelationships.Contains(holderRelationship))
            {
                return (false, "Hubungan pemegang polis tidak valid. Gunakan nilai dari endpoint filters/metadata.");
            }

            if (request.EffectiveStartDate.HasValue &&
                request.EffectiveEndDate.HasValue &&
                request.EffectiveEndDate.Value.Date < request.EffectiveStartDate.Value.Date)
            {
                return (false, "Tanggal akhir berlaku tidak boleh lebih kecil dari tanggal mulai berlaku.");
            }

            if (request.AnnualLimitAmount.HasValue && request.AnnualLimitAmount.Value < 0)
            {
                return (false, "Limit tahunan tidak boleh kurang dari 0.");
            }

            if (request.RemainingLimitAmount.HasValue && request.RemainingLimitAmount.Value < 0)
            {
                return (false, "Sisa limit tidak boleh kurang dari 0.");
            }

            if (request.AnnualLimitAmount.HasValue &&
                request.RemainingLimitAmount.HasValue &&
                request.RemainingLimitAmount.Value > request.AnnualLimitAmount.Value)
            {
                return (false, "Sisa limit tidak boleh lebih besar dari limit tahunan.");
            }

            if (request.CoPaymentPercent.HasValue &&
                (request.CoPaymentPercent.Value < 0 || request.CoPaymentPercent.Value > 100))
            {
                return (false, "Co-payment percent harus di antara 0 sampai 100.");
            }

            if (request.CoPaymentAmount.HasValue && request.CoPaymentAmount.Value < 0)
            {
                return (false, "Co-payment amount tidak boleh kurang dari 0.");
            }

            var patientExists = await _dbContext.Set<MstPatient>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == request.PatientId &&
                    x.IsActive &&
                    !x.IsDelete);

            if (!patientExists)
            {
                return (false, "Patient tidak valid atau tidak aktif.");
            }

            var insuranceProviderExists = await _dbContext.Set<MstInsuranceProvider>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == request.InsuranceProviderId &&
                    x.IsActive &&
                    !x.IsDelete);

            if (!insuranceProviderExists)
            {
                return (false, "Insurance provider tidak valid atau tidak aktif.");
            }

            var normalizedPolicyNumber = request.PolicyNumber.Trim().ToLower();

            var duplicatePolicy = await _dbContext.Set<MstPatientInsurance>()
                .AsNoTracking()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.PatientId == request.PatientId &&
                    x.InsuranceProviderId == request.InsuranceProviderId &&
                    x.PolicyNumber.ToLower() == normalizedPolicyNumber &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicatePolicy)
            {
                return (false, "Nomor polis untuk patient dan insurance provider tersebut sudah digunakan.");
            }

            return (true, null);
        }

        private async Task UnsetOtherPrimaryAsync(
            Guid patientId,
            Guid? exceptId,
            DateTime now,
            Guid actorUserId)
        {
            var query = _dbContext.Set<MstPatientInsurance>()
                .Where(x =>
                    !x.IsDelete &&
                    x.PatientId == patientId &&
                    x.IsPrimary);

            if (exceptId.HasValue)
            {
                query = query.Where(x => x.Id != exceptId.Value);
            }

            var entities = await query.ToListAsync();

            foreach (var entity in entities)
            {
                entity.IsPrimary = false;
                entity.UpdateDateTime = now;
                entity.UpdateBy = actorUserId;
            }
        }

        private async Task<PatientInsuranceCreateResponse> BuildCreateResponseAsync(Guid id)
        {
            var entity = await BuildBaseQuery()
                .FirstAsync(x => x.Id == id);

            return new PatientInsuranceCreateResponse
            {
                Id = entity.Id,
                PatientId = entity.PatientId,
                PatientName = entity.Patient?.FullName ?? string.Empty,
                InsuranceProviderId = entity.InsuranceProviderId,
                InsuranceProviderName = entity.InsuranceProvider?.InsuranceProviderName ?? string.Empty,
                PolicyNumber = entity.PolicyNumber,
                IsPrimary = entity.IsPrimary,
                IsEligible = entity.IsEligible,
                IsActive = entity.IsActive
            };
        }

        private static PatientInsuranceResponse MapResponse(
            MstPatientInsurance entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            var usability = BuildUsability(entity, AppDateTimeHelper.OperationalDate());

            return new PatientInsuranceResponse
            {
                Id = entity.Id,
                PatientId = entity.PatientId,
                PatientCode = entity.Patient?.PatientCode ?? string.Empty,
                MedicalRecordNumber = entity.Patient?.MedicalRecordNumber ?? string.Empty,
                PatientName = entity.Patient?.FullName ?? string.Empty,
                InsuranceProviderId = entity.InsuranceProviderId,
                InsuranceProviderCode = entity.InsuranceProvider?.InsuranceProviderCode ?? string.Empty,
                InsuranceProviderName = entity.InsuranceProvider?.InsuranceProviderName ?? string.Empty,
                InsuranceGroupName = entity.InsuranceProvider?.InsuranceGroupName,
                ProviderType = entity.InsuranceProvider?.ProviderType,
                ClaimMethod = entity.InsuranceProvider?.ClaimMethod,
                PolicyNumber = entity.PolicyNumber,
                CardNumber = entity.CardNumber,
                MemberNumber = entity.MemberNumber,
                PlanName = entity.PlanName,
                ClassName = entity.ClassName,
                BenefitPlanCode = entity.BenefitPlanCode,
                HolderName = entity.HolderName,
                HolderRelationship = entity.HolderRelationship,
                EffectiveStartDate = entity.EffectiveStartDate,
                EffectiveEndDate = entity.EffectiveEndDate,
                IsPrimary = entity.IsPrimary,
                IsEligible = entity.IsEligible,
                IsCurrentlyEffective = usability.IsCurrentlyEffective,
                IsProviderActive = usability.IsProviderActive,
                IsProviderContractEffective = usability.IsProviderContractEffective,
                IsUsableForEncounter = usability.IsUsableForEncounter,
                UsabilityMessage = usability.UsabilityMessage,
                LastEligibilityCheckAt = entity.LastEligibilityCheckAt,
                LastEligibilityReferenceNumber = entity.LastEligibilityReferenceNumber,
                AnnualLimitAmount = entity.AnnualLimitAmount,
                RemainingLimitAmount = entity.RemainingLimitAmount,
                CoPaymentPercent = entity.CoPaymentPercent,
                CoPaymentAmount = entity.CoPaymentAmount,
                IsNeedGuaranteeLetter = entity.IsNeedGuaranteeLetter,
                IsNeedReferralLetter = entity.IsNeedReferralLetter,
                IsAllowExcessPaymentByPatient = entity.IsAllowExcessPaymentByPatient,
                CardImagePath = entity.CardImagePath,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy),
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };
        }

        private static PatientInsuranceDetailResponse MapDetailResponse(
            MstPatientInsurance entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            var usability = BuildUsability(entity, AppDateTimeHelper.OperationalDate());

            var response = new PatientInsuranceDetailResponse
            {
                Id = entity.Id,
                PatientId = entity.PatientId,
                PatientCode = entity.Patient?.PatientCode ?? string.Empty,
                MedicalRecordNumber = entity.Patient?.MedicalRecordNumber ?? string.Empty,
                PatientName = entity.Patient?.FullName ?? string.Empty,
                InsuranceProviderId = entity.InsuranceProviderId,
                InsuranceProviderCode = entity.InsuranceProvider?.InsuranceProviderCode ?? string.Empty,
                InsuranceProviderName = entity.InsuranceProvider?.InsuranceProviderName ?? string.Empty,
                InsuranceGroupName = entity.InsuranceProvider?.InsuranceGroupName,
                ProviderType = entity.InsuranceProvider?.ProviderType,
                ClaimMethod = entity.InsuranceProvider?.ClaimMethod,
                PolicyNumber = entity.PolicyNumber,
                CardNumber = entity.CardNumber,
                MemberNumber = entity.MemberNumber,
                PlanName = entity.PlanName,
                ClassName = entity.ClassName,
                BenefitPlanCode = entity.BenefitPlanCode,
                HolderName = entity.HolderName,
                HolderRelationship = entity.HolderRelationship,
                EffectiveStartDate = entity.EffectiveStartDate,
                EffectiveEndDate = entity.EffectiveEndDate,
                IsPrimary = entity.IsPrimary,
                IsEligible = entity.IsEligible,
                IsCurrentlyEffective = usability.IsCurrentlyEffective,
                IsProviderActive = usability.IsProviderActive,
                IsProviderContractEffective = usability.IsProviderContractEffective,
                IsUsableForEncounter = usability.IsUsableForEncounter,
                UsabilityMessage = usability.UsabilityMessage,
                LastEligibilityCheckAt = entity.LastEligibilityCheckAt,
                LastEligibilityReferenceNumber = entity.LastEligibilityReferenceNumber,
                EligibilityNote = entity.EligibilityNote,
                AnnualLimitAmount = entity.AnnualLimitAmount,
                RemainingLimitAmount = entity.RemainingLimitAmount,
                CoPaymentPercent = entity.CoPaymentPercent,
                CoPaymentAmount = entity.CoPaymentAmount,
                IsNeedGuaranteeLetter = entity.IsNeedGuaranteeLetter,
                IsNeedReferralLetter = entity.IsNeedReferralLetter,
                IsAllowExcessPaymentByPatient = entity.IsAllowExcessPaymentByPatient,
                CardImagePath = entity.CardImagePath,
                Notes = entity.Notes,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy),
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };

            return response;
        }

        private static PatientInsuranceOptionResponse MapOptionResponse(MstPatientInsurance entity)
        {
            var usability = BuildUsability(entity, AppDateTimeHelper.OperationalDate());

            return new PatientInsuranceOptionResponse
            {
                Id = entity.Id,
                PatientId = entity.PatientId,
                PatientCode = entity.Patient?.PatientCode ?? string.Empty,
                MedicalRecordNumber = entity.Patient?.MedicalRecordNumber ?? string.Empty,
                PatientName = entity.Patient?.FullName ?? string.Empty,
                InsuranceProviderId = entity.InsuranceProviderId,
                InsuranceProviderCode = entity.InsuranceProvider?.InsuranceProviderCode ?? string.Empty,
                InsuranceProviderName = entity.InsuranceProvider?.InsuranceProviderName ?? string.Empty,
                PolicyNumber = entity.PolicyNumber,
                CardNumber = entity.CardNumber,
                MemberNumber = entity.MemberNumber,
                PlanName = entity.PlanName,
                ClassName = entity.ClassName,
                BenefitPlanCode = entity.BenefitPlanCode,
                EffectiveStartDate = entity.EffectiveStartDate,
                EffectiveEndDate = entity.EffectiveEndDate,
                IsPrimary = entity.IsPrimary,
                IsEligible = entity.IsEligible,
                IsCurrentlyEffective = usability.IsCurrentlyEffective,
                IsProviderActive = usability.IsProviderActive,
                IsProviderContractEffective = usability.IsProviderContractEffective,
                IsUsableForEncounter = usability.IsUsableForEncounter,
                UsabilityMessage = usability.UsabilityMessage,
                IsNeedGuaranteeLetter = entity.IsNeedGuaranteeLetter,
                IsNeedReferralLetter = entity.IsNeedReferralLetter,
                IsAllowExcessPaymentByPatient = entity.IsAllowExcessPaymentByPatient,
                AnnualLimitAmount = entity.AnnualLimitAmount,
                RemainingLimitAmount = entity.RemainingLimitAmount,
                CoPaymentPercent = entity.CoPaymentPercent,
                CoPaymentAmount = entity.CoPaymentAmount
            };
        }

        private static (
            bool IsCurrentlyEffective,
            bool IsProviderActive,
            bool IsProviderContractEffective,
            bool IsUsableForEncounter,
            string? UsabilityMessage) BuildUsability(
                MstPatientInsurance entity,
                DateTime serviceDate)
        {
            var date = serviceDate.Date;

            var isCurrentlyEffective =
                (!entity.EffectiveStartDate.HasValue || entity.EffectiveStartDate.Value.Date <= date) &&
                (!entity.EffectiveEndDate.HasValue || entity.EffectiveEndDate.Value.Date >= date);

            var isProviderActive =
                entity.InsuranceProvider != null &&
                entity.InsuranceProvider.IsActive &&
                !entity.InsuranceProvider.IsDelete;

            var isProviderContractEffective =
                entity.InsuranceProvider != null &&
                (!entity.InsuranceProvider.ContractStartDate.HasValue || entity.InsuranceProvider.ContractStartDate.Value.Date <= date) &&
                (!entity.InsuranceProvider.ContractEndDate.HasValue || entity.InsuranceProvider.ContractEndDate.Value.Date >= date);

            var isPatientActive =
                entity.Patient != null &&
                entity.Patient.IsActive &&
                !entity.Patient.IsDelete;

            var isUsable =
                entity.IsActive &&
                entity.IsEligible &&
                isPatientActive &&
                isCurrentlyEffective &&
                isProviderActive &&
                isProviderContractEffective;

            string? message = null;

            if (!entity.IsActive)
                message = "Data asuransi pasien tidak aktif.";
            else if (!entity.IsEligible)
                message = "Asuransi pasien belum eligible.";
            else if (!isPatientActive)
                message = "Data pasien tidak aktif.";
            else if (!isCurrentlyEffective)
                message = entity.EffectiveStartDate.HasValue && entity.EffectiveStartDate.Value.Date > date
                    ? "Polis asuransi belum mulai berlaku."
                    : "Polis asuransi sudah berakhir.";
            else if (!isProviderActive)
                message = "Provider asuransi tidak aktif.";
            else if (!isProviderContractEffective)
                message = entity.InsuranceProvider?.ContractStartDate.HasValue == true &&
                          entity.InsuranceProvider.ContractStartDate.Value.Date > date
                    ? "Kontrak provider asuransi belum mulai berlaku."
                    : "Kontrak provider asuransi sudah berakhir.";

            return (
                isCurrentlyEffective,
                isProviderActive,
                isProviderContractEffective,
                isUsable,
                message);
        }

        private static IOrderedQueryable<MstPatientInsurance> ApplySorting(
            IQueryable<MstPatientInsurance> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDescending = string.Equals(
                sortDirection,
                "desc",
                StringComparison.OrdinalIgnoreCase
            );

            return (sortBy ?? "createDateTime").Trim().ToLowerInvariant() switch
            {
                "updatedatetime" => isDescending
                    ? query.OrderByDescending(x => x.UpdateDateTime).ThenByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.UpdateDateTime).ThenBy(x => x.CreateDateTime),

                "patientname" => isDescending
                    ? query.OrderByDescending(x => x.Patient != null ? x.Patient.FullName : string.Empty)
                    : query.OrderBy(x => x.Patient != null ? x.Patient.FullName : string.Empty),

                "medicalrecordnumber" => isDescending
                    ? query.OrderByDescending(x => x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty)
                    : query.OrderBy(x => x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty),

                "insuranceprovidername" => isDescending
                    ? query.OrderByDescending(x => x.InsuranceProvider != null ? x.InsuranceProvider.InsuranceProviderName : string.Empty)
                    : query.OrderBy(x => x.InsuranceProvider != null ? x.InsuranceProvider.InsuranceProviderName : string.Empty),

                "policynumber" => isDescending
                    ? query.OrderByDescending(x => x.PolicyNumber)
                    : query.OrderBy(x => x.PolicyNumber),

                "cardnumber" => isDescending
                    ? query.OrderByDescending(x => x.CardNumber)
                    : query.OrderBy(x => x.CardNumber),

                "membernumber" => isDescending
                    ? query.OrderByDescending(x => x.MemberNumber)
                    : query.OrderBy(x => x.MemberNumber),

                "planname" => isDescending
                    ? query.OrderByDescending(x => x.PlanName)
                    : query.OrderBy(x => x.PlanName),

                "classname" => isDescending
                    ? query.OrderByDescending(x => x.ClassName)
                    : query.OrderBy(x => x.ClassName),

                "effectivestartdate" => isDescending
                    ? query.OrderByDescending(x => x.EffectiveStartDate)
                    : query.OrderBy(x => x.EffectiveStartDate),

                "effectiveenddate" => isDescending
                    ? query.OrderByDescending(x => x.EffectiveEndDate)
                    : query.OrderBy(x => x.EffectiveEndDate),

                "isprimary" => isDescending
                    ? query.OrderByDescending(x => x.IsPrimary).ThenBy(x => x.Patient != null ? x.Patient.FullName : string.Empty)
                    : query.OrderBy(x => x.IsPrimary).ThenBy(x => x.Patient != null ? x.Patient.FullName : string.Empty),

                "iseligible" => isDescending
                    ? query.OrderByDescending(x => x.IsEligible).ThenBy(x => x.Patient != null ? x.Patient.FullName : string.Empty)
                    : query.OrderBy(x => x.IsEligible).ThenBy(x => x.Patient != null ? x.Patient.FullName : string.Empty),

                "isactive" => isDescending
                    ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.Patient != null ? x.Patient.FullName : string.Empty)
                    : query.OrderBy(x => x.IsActive).ThenBy(x => x.Patient != null ? x.Patient.FullName : string.Empty),

                _ => isDescending
                    ? query.OrderByDescending(x => x.CreateDateTime).ThenByDescending(x => x.PolicyNumber)
                    : query.OrderBy(x => x.CreateDateTime).ThenBy(x => x.PolicyNumber)
            };
        }

        private async Task<Dictionary<Guid, string?>> GetActorNameMapAsync(
            IEnumerable<Guid> actorIds)
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
                    Name =
                        x.DisplayName ??
                        x.UserName ??
                        x.Email ??
                        x.UserCode
                })
                .ToDictionaryAsync(x => x.Id, x => x.Name);
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

        private static void NormalizeActorInfo(PatientInsuranceResponse data)
        {
            if (data.UpdateDateTime.HasValue &&
                data.UpdateDateTime.Value == DateTime.MinValue)
            {
                data.UpdateDateTime = null;
            }

            if (!data.CreateBy.HasValue || data.CreateBy.Value == Guid.Empty)
            {
                data.CreateBy = null;
                data.CreateByName = null;
            }

            if (!data.UpdateBy.HasValue || data.UpdateBy.Value == Guid.Empty)
            {
                data.UpdateBy = null;
                data.UpdateByName = null;
            }
        }

        private static (int PageNumber, int PageSize) NormalizePaging(
            int pageNumber,
            int pageSize)
        {
            if (pageNumber < 1)
            {
                pageNumber = 1;
            }

            if (pageSize < 1)
            {
                pageSize = 25;
            }

            if (pageSize > 100)
            {
                pageSize = 100;
            }

            return (pageNumber, pageSize);
        }

        private static string? NormalizeNullableString(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private static Guid? NormalizeNullableGuid(Guid? value)
        {
            if (!value.HasValue || value.Value == Guid.Empty)
            {
                return null;
            }

            return value.Value;
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
