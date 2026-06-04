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

using ResponseTariffPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs.TariffResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/master-data/tariffs")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MASTER_DATA",
        moduleName: "Health Service Master Data",
        displayName: "Tariff",
        AreaName = "HealthServices",
        ControllerName = "Tariff",
        Description = "Health service master data tariff",
        SortOrder = 8
    )]
    [Tags("Health Services / Master Data / Tariff")]
    public class TariffController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MasterData";
        private const string TariffCodePrefix = "TF-RSMMC-";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public TariffController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<TariffFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Tariff", Description = "Melihat data tariff", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Tariff", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new TariffFilterMetadataResponse
            {
                DefaultFilter = new TariffDefaultFilterResponse(),
                CustomPeriods = BuildCustomPeriodOptions(),
                SortOptions = new List<TariffSortOptionResponse>
                {
                    new() { Value = "sortOrder", Label = "Urutan" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "tariffCode", Label = "Kode tariff" },
                    new() { Value = "tariffName", Label = "Nama tariff" },
                    new() { Value = "tariffCategoryName", Label = "Kategori tariff" },
                    new() { Value = "serviceUnitName", Label = "Service unit" },
                    new() { Value = "normalPrice", Label = "Harga normal" },
                    new() { Value = "effectiveStartDate", Label = "Tanggal mulai berlaku" },
                    new() { Value = "effectiveEndDate", Label = "Tanggal akhir berlaku" },
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
                "Tariff.GetFilterMetadata",
                "Mengambil metadata filter tariff.",
                result
            );

            return Ok(ApiResponse<TariffFilterMetadataResponse>.Ok(
                result,
                "Metadata filter tariff berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<TariffSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Tariff", Description = "Melihat data tariff", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Tariff", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var now = DateTime.UtcNow;

            var query = _dbContext.Set<MstTariff>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            var result = new TariffSummaryResponse
            {
                TotalTariff = await query.CountAsync(),
                ActiveTariff = await query.CountAsync(x => x.IsActive),
                InactiveTariff = await query.CountAsync(x => !x.IsActive),
                SurgeryRelatedTariff = await query.CountAsync(x => x.IsSurgeryRelated),
                RoomChargeTariff = await query.CountAsync(x => x.IsRoomCharge),
                AdministrationFeeTariff = await query.CountAsync(x => x.IsAdministrationFee),
                RegistrationFeeTariff = await query.CountAsync(x => x.IsRegistrationFee),
                ConsultationFeeTariff = await query.CountAsync(x => x.IsConsultationFee),
                PackageTariff = await query.CountAsync(x => x.IsPackageTariff),
                NeedDoctorTariff = await query.CountAsync(x => x.IsNeedDoctor),
                NeedApprovalTariff = await query.CountAsync(x => x.IsNeedApproval),
                TaxableTariff = await query.CountAsync(x => x.IsTaxable),
                EffectiveTariff = await query.CountAsync(x =>
                    (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value <= now) &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= now)),
                ExpiredTariff = await query.CountAsync(x =>
                    x.EffectiveEndDate.HasValue && x.EffectiveEndDate.Value < now)
            };

            return Ok(ApiResponse<TariffSummaryResponse>.Ok(
                result,
                "Ringkasan tariff berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseTariffPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Tariff", Description = "Melihat data tariff", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Tariff", "Read")]
        public async Task<IActionResult> GetTariffs(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? tariffCategoryId,
            [FromQuery] Guid? serviceUnitId,
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

            var dateRange = ResolveDateRange(startDate, endDate, customPeriod);

            if (!dateRange.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    dateRange.ErrorMessage ?? "Filter tanggal tidak valid."
                ));
            }

            var query = BuildBaseQuery();

            if (dateRange.Start.HasValue)
                query = query.Where(x => x.CreateDateTime >= dateRange.Start.Value);

            if (dateRange.EndExclusive.HasValue)
                query = query.Where(x => x.CreateDateTime < dateRange.EndExclusive.Value);

            if (tariffCategoryId.HasValue && tariffCategoryId.Value != Guid.Empty)
                query = query.Where(x => x.TariffCategoryId == tariffCategoryId.Value);

            if (serviceUnitId.HasValue && serviceUnitId.Value != Guid.Empty)
                query = query.Where(x => x.ServiceUnitId == serviceUnitId.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.TariffCode.ToLower().Contains(keyword) ||
                    x.TariffName.ToLower().Contains(keyword) ||
                    (x.ExternalServiceCode != null && x.ExternalServiceCode.ToLower().Contains(keyword)) ||
                    (x.ExternalClassCode != null && x.ExternalClassCode.ToLower().Contains(keyword)) ||
                    (x.ProviderName != null && x.ProviderName.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)) ||
                    (x.TariffCategory != null && x.TariffCategory.TariffCategoryCode.ToLower().Contains(keyword)) ||
                    (x.TariffCategory != null && x.TariffCategory.TariffCategoryName.ToLower().Contains(keyword)) ||
                    (x.ServiceUnit != null && x.ServiceUnit.ServiceUnitCode.ToLower().Contains(keyword)) ||
                    (x.ServiceUnit != null && x.ServiceUnit.ServiceUnitName.ToLower().Contains(keyword)) ||
                    (x.Clinic != null && x.Clinic.ClinicCode.ToLower().Contains(keyword)) ||
                    (x.Clinic != null && x.Clinic.ClinicName.ToLower().Contains(keyword)) ||
                    (x.PatientClass != null && x.PatientClass.PatientClassCode.ToLower().Contains(keyword)) ||
                    (x.PatientClass != null && x.PatientClass.PatientClassName.ToLower().Contains(keyword)) ||
                    (x.Procedure != null && x.Procedure.ProcedureCode.ToLower().Contains(keyword)) ||
                    (x.Procedure != null && x.Procedure.ProcedureName.ToLower().Contains(keyword)) ||
                    (x.Drug != null && x.Drug.DrugCode.ToLower().Contains(keyword)) ||
                    (x.Drug != null && x.Drug.DrugName.ToLower().Contains(keyword)));
            }

            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new TariffResponse
                {
                    Id = x.Id,
                    TariffCode = x.TariffCode,
                    TariffName = x.TariffName,
                    TariffCategoryId = x.TariffCategoryId,
                    TariffCategoryCode = x.TariffCategory != null ? x.TariffCategory.TariffCategoryCode : string.Empty,
                    TariffCategoryName = x.TariffCategory != null ? x.TariffCategory.TariffCategoryName : string.Empty,
                    TariffGroupName = x.TariffCategory != null ? x.TariffCategory.TariffGroupName : null,
                    ServiceUnitId = x.ServiceUnitId,
                    ServiceUnitCode = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitCode : null,
                    ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null,
                    ClinicId = x.ClinicId,
                    ClinicCode = x.Clinic != null ? x.Clinic.ClinicCode : null,
                    ClinicName = x.Clinic != null ? x.Clinic.ClinicName : null,
                    PatientClassId = x.PatientClassId,
                    PatientClassCode = x.PatientClass != null ? x.PatientClass.PatientClassCode : null,
                    PatientClassName = x.PatientClass != null ? x.PatientClass.PatientClassName : null,
                    ProcedureId = x.ProcedureId,
                    ProcedureCode = x.Procedure != null ? x.Procedure.ProcedureCode : null,
                    ProcedureName = x.Procedure != null ? x.Procedure.ProcedureName : null,
                    DrugId = x.DrugId,
                    DrugCode = x.Drug != null ? x.Drug.DrugCode : null,
                    DrugName = x.Drug != null ? x.Drug.DrugName : null,
                    ExternalServiceCode = x.ExternalServiceCode,
                    ExternalClassCode = x.ExternalClassCode,
                    ProviderName = x.ProviderName,
                    IsSurgeryRelated = x.IsSurgeryRelated,
                    IsRoomCharge = x.IsRoomCharge,
                    IsAdministrationFee = x.IsAdministrationFee,
                    IsRegistrationFee = x.IsRegistrationFee,
                    IsConsultationFee = x.IsConsultationFee,
                    IsPackageTariff = x.IsPackageTariff,
                    IsNeedDoctor = x.IsNeedDoctor,
                    IsNeedApproval = x.IsNeedApproval,
                    NormalPrice = x.NormalPrice,
                    MemberPrice = x.MemberPrice,
                    InsurancePrice = x.InsurancePrice,
                    CompanyPrice = x.CompanyPrice,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    IsTaxable = x.IsTaxable,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

            var result = new ResponseTariffPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseTariffPagedResult>.Ok(
                result,
                "Data tariff berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<TariffOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Tariff", Description = "Melihat data tariff", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Tariff", "Read")]
        public async Task<IActionResult> GetTariffOptions(
    [FromQuery] Guid? tariffCategoryId,
    [FromQuery] Guid? serviceUnitId,
    [FromQuery] bool onlyActive = true,
    [FromQuery] string? search = null,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();

            if (onlyActive)
                query = query.Where(x => x.IsActive);

            if (tariffCategoryId.HasValue && tariffCategoryId.Value != Guid.Empty)
                query = query.Where(x => x.TariffCategoryId == tariffCategoryId.Value);

            if (serviceUnitId.HasValue && serviceUnitId.Value != Guid.Empty)
                query = query.Where(x => x.ServiceUnitId == serviceUnitId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.TariffCode.ToLower().Contains(keyword) ||
                    x.TariffName.ToLower().Contains(keyword) ||
                    (x.TariffCategory != null && x.TariffCategory.TariffCategoryName.ToLower().Contains(keyword)) ||
                    (x.ServiceUnit != null && x.ServiceUnit.ServiceUnitName.ToLower().Contains(keyword)) ||
                    (x.Procedure != null && x.Procedure.ProcedureCode.ToLower().Contains(keyword)) ||
                    (x.Procedure != null && x.Procedure.ProcedureName.ToLower().Contains(keyword)) ||
                    (x.Drug != null && x.Drug.DrugCode.ToLower().Contains(keyword)) ||
                    (x.Drug != null && x.Drug.DrugName.ToLower().Contains(keyword)));
            }

            var totalData = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.TariffName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new TariffOptionResponse
                {
                    Id = x.Id,
                    TariffCode = x.TariffCode,
                    TariffName = x.TariffName,

                    TariffCategoryId = x.TariffCategoryId,
                    TariffCategoryName = x.TariffCategory != null
                        ? x.TariffCategory.TariffCategoryName
                        : string.Empty,

                    ServiceUnitId = x.ServiceUnitId,
                    ServiceUnitName = x.ServiceUnit != null
                        ? x.ServiceUnit.ServiceUnitName
                        : null,

                    ClinicId = x.ClinicId,
                    ClinicName = x.Clinic != null
                        ? x.Clinic.ClinicName
                        : null,

                    PatientClassId = x.PatientClassId,
                    PatientClassName = x.PatientClass != null
                        ? x.PatientClass.PatientClassName
                        : null,

                    ProcedureId = x.ProcedureId,
                    ProcedureName = x.Procedure != null
                        ? x.Procedure.ProcedureName
                        : null,

                    DrugId = x.DrugId,
                    DrugName = x.Drug != null
                        ? x.Drug.DrugName
                        : null,

                    NormalPrice = x.NormalPrice,
                    MemberPrice = x.MemberPrice,
                    InsurancePrice = x.InsurancePrice,
                    CompanyPrice = x.CompanyPrice,

                    IsNeedDoctor = x.IsNeedDoctor,
                    IsNeedApproval = x.IsNeedApproval,
                    IsTaxable = x.IsTaxable
                })
                .ToListAsync();

            var result = new TariffOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<TariffOptionPagedResponse>.Ok(
                result,
                "Data pilihan tariff berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<TariffDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Tariff", Description = "Melihat data tariff", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Tariff", "Read")]
        public async Task<IActionResult> GetTariffById(Guid id)
        {
            var data = await BuildBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => new TariffDetailResponse
                {
                    Id = x.Id,
                    TariffCode = x.TariffCode,
                    TariffName = x.TariffName,
                    TariffCategoryId = x.TariffCategoryId,
                    TariffCategoryCode = x.TariffCategory != null ? x.TariffCategory.TariffCategoryCode : string.Empty,
                    TariffCategoryName = x.TariffCategory != null ? x.TariffCategory.TariffCategoryName : string.Empty,
                    TariffGroupName = x.TariffCategory != null ? x.TariffCategory.TariffGroupName : null,
                    ServiceUnitId = x.ServiceUnitId,
                    ServiceUnitCode = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitCode : null,
                    ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null,
                    ClinicId = x.ClinicId,
                    ClinicCode = x.Clinic != null ? x.Clinic.ClinicCode : null,
                    ClinicName = x.Clinic != null ? x.Clinic.ClinicName : null,
                    PatientClassId = x.PatientClassId,
                    PatientClassCode = x.PatientClass != null ? x.PatientClass.PatientClassCode : null,
                    PatientClassName = x.PatientClass != null ? x.PatientClass.PatientClassName : null,
                    ProcedureId = x.ProcedureId,
                    ProcedureCode = x.Procedure != null ? x.Procedure.ProcedureCode : null,
                    ProcedureName = x.Procedure != null ? x.Procedure.ProcedureName : null,
                    DrugId = x.DrugId,
                    DrugCode = x.Drug != null ? x.Drug.DrugCode : null,
                    DrugName = x.Drug != null ? x.Drug.DrugName : null,
                    ExternalServiceCode = x.ExternalServiceCode,
                    ExternalClassCode = x.ExternalClassCode,
                    ProviderName = x.ProviderName,
                    IsSurgeryRelated = x.IsSurgeryRelated,
                    IsRoomCharge = x.IsRoomCharge,
                    IsAdministrationFee = x.IsAdministrationFee,
                    IsRegistrationFee = x.IsRegistrationFee,
                    IsConsultationFee = x.IsConsultationFee,
                    IsPackageTariff = x.IsPackageTariff,
                    IsNeedDoctor = x.IsNeedDoctor,
                    IsNeedApproval = x.IsNeedApproval,
                    NormalPrice = x.NormalPrice,
                    MemberPrice = x.MemberPrice,
                    InsurancePrice = x.InsurancePrice,
                    CompanyPrice = x.CompanyPrice,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    IsTaxable = x.IsTaxable,
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
                    "Tariff tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<TariffDetailResponse>.Ok(
                data,
                "Detail tariff berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<TariffCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Tariff", Description = "Membuat data tariff", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("Tariff", "Create")]
        public async Task<IActionResult> CreateTariff([FromBody] CreateTariffRequest request)
        {
            var validation = await ValidateRequestAsync(
                excludeId: null,
                tariffCategoryId: request.TariffCategoryId,
                serviceUnitId: request.ServiceUnitId,
                clinicId: request.ClinicId,
                patientClassId: request.PatientClassId,
                procedureId: request.ProcedureId,
                drugId: request.DrugId,
                tariffName: request.TariffName,
                normalPrice: request.NormalPrice,
                memberPrice: request.MemberPrice,
                insurancePrice: request.InsurancePrice,
                companyPrice: request.CompanyPrice,
                effectiveStartDate: request.EffectiveStartDate,
                effectiveEndDate: request.EffectiveEndDate
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data tariff tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            var entity = new MstTariff
            {
                Id = Guid.NewGuid(),
                TariffCode = await GenerateTariffCodeAsync(),
                TariffName = request.TariffName.Trim(),
                TariffCategoryId = request.TariffCategoryId,
                ServiceUnitId = NormalizeNullableGuid(request.ServiceUnitId),
                ClinicId = NormalizeNullableGuid(request.ClinicId),
                PatientClassId = NormalizeNullableGuid(request.PatientClassId),
                ProcedureId = NormalizeNullableGuid(request.ProcedureId),
                DrugId = NormalizeNullableGuid(request.DrugId),
                ExternalServiceCode = NormalizeNullableText(request.ExternalServiceCode),
                ExternalClassCode = NormalizeNullableText(request.ExternalClassCode),
                ProviderName = NormalizeNullableText(request.ProviderName),
                IsSurgeryRelated = request.IsSurgeryRelated,
                IsRoomCharge = request.IsRoomCharge,
                IsAdministrationFee = request.IsAdministrationFee,
                IsRegistrationFee = request.IsRegistrationFee,
                IsConsultationFee = request.IsConsultationFee,
                IsPackageTariff = request.IsPackageTariff,
                IsNeedDoctor = request.IsNeedDoctor,
                IsNeedApproval = request.IsNeedApproval,
                NormalPrice = request.NormalPrice,
                MemberPrice = request.MemberPrice,
                InsurancePrice = request.InsurancePrice,
                CompanyPrice = request.CompanyPrice,
                EffectiveStartDate = request.EffectiveStartDate,
                EffectiveEndDate = request.EffectiveEndDate,
                IsTaxable = request.IsTaxable,
                SortOrder = request.SortOrder,
                Description = NormalizeNullableText(request.Description),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstTariff>().Add(entity);
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            var response = ToCreateUpdateResponse(entity);

            await _loggerService.InfoAsync(
                LogCategory,
                "Tariff.CreateTariff",
                "Membuat data tariff.",
                response
            );

            return Ok(ApiResponse<TariffCreateResponse>.Ok(
                response,
                "Tariff berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<TariffUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Tariff", Description = "Mengubah data tariff", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("Tariff", "Update")]
        public async Task<IActionResult> UpdateTariff(Guid id, [FromBody] UpdateTariffRequest request)
        {
            var entity = await _dbContext.Set<MstTariff>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Tariff tidak ditemukan."
                ));
            }

            var validation = await ValidateRequestAsync(
                excludeId: id,
                tariffCategoryId: request.TariffCategoryId,
                serviceUnitId: request.ServiceUnitId,
                clinicId: request.ClinicId,
                patientClassId: request.PatientClassId,
                procedureId: request.ProcedureId,
                drugId: request.DrugId,
                tariffName: request.TariffName,
                normalPrice: request.NormalPrice,
                memberPrice: request.MemberPrice,
                insurancePrice: request.InsurancePrice,
                companyPrice: request.CompanyPrice,
                effectiveStartDate: request.EffectiveStartDate,
                effectiveEndDate: request.EffectiveEndDate
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data tariff tidak valid."
                ));
            }

            entity.TariffName = request.TariffName.Trim();
            entity.TariffCategoryId = request.TariffCategoryId;
            entity.ServiceUnitId = NormalizeNullableGuid(request.ServiceUnitId);
            entity.ClinicId = NormalizeNullableGuid(request.ClinicId);
            entity.PatientClassId = NormalizeNullableGuid(request.PatientClassId);
            entity.ProcedureId = NormalizeNullableGuid(request.ProcedureId);
            entity.DrugId = NormalizeNullableGuid(request.DrugId);
            entity.ExternalServiceCode = NormalizeNullableText(request.ExternalServiceCode);
            entity.ExternalClassCode = NormalizeNullableText(request.ExternalClassCode);
            entity.ProviderName = NormalizeNullableText(request.ProviderName);
            entity.IsSurgeryRelated = request.IsSurgeryRelated;
            entity.IsRoomCharge = request.IsRoomCharge;
            entity.IsAdministrationFee = request.IsAdministrationFee;
            entity.IsRegistrationFee = request.IsRegistrationFee;
            entity.IsConsultationFee = request.IsConsultationFee;
            entity.IsPackageTariff = request.IsPackageTariff;
            entity.IsNeedDoctor = request.IsNeedDoctor;
            entity.IsNeedApproval = request.IsNeedApproval;
            entity.NormalPrice = request.NormalPrice;
            entity.MemberPrice = request.MemberPrice;
            entity.InsurancePrice = request.InsurancePrice;
            entity.CompanyPrice = request.CompanyPrice;
            entity.EffectiveStartDate = request.EffectiveStartDate;
            entity.EffectiveEndDate = request.EffectiveEndDate;
            entity.IsTaxable = request.IsTaxable;
            entity.SortOrder = request.SortOrder;
            entity.Description = NormalizeNullableText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            var response = ToUpdateResponse(entity);

            return Ok(ApiResponse<TariffUpdateResponse>.Ok(
                response,
                "Tariff berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Tariff", Description = "Menghapus data tariff", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("Tariff", "Delete")]
        public async Task<IActionResult> DeleteTariff(Guid id)
        {
            var entity = await _dbContext.Set<MstTariff>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Tariff tidak ditemukan."
                ));
            }

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Tariff berhasil dihapus."
            ));
        }

        private IQueryable<MstTariff> BuildBaseQuery()
        {
            return _dbContext.Set<MstTariff>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            Guid tariffCategoryId,
            Guid? serviceUnitId,
            Guid? clinicId,
            Guid? patientClassId,
            Guid? procedureId,
            Guid? drugId,
            string tariffName,
            decimal normalPrice,
            decimal? memberPrice,
            decimal? insurancePrice,
            decimal? companyPrice,
            DateTime? effectiveStartDate,
            DateTime? effectiveEndDate)
        {
            if (tariffCategoryId == Guid.Empty)
                return (false, "Kategori tariff wajib dipilih.");

            if (string.IsNullOrWhiteSpace(tariffName))
                return (false, "Nama tariff wajib diisi.");

            if (normalPrice < 0)
                return (false, "Harga normal tidak boleh kurang dari 0.");

            if (memberPrice.HasValue && memberPrice.Value < 0)
                return (false, "Harga member tidak boleh kurang dari 0.");

            if (insurancePrice.HasValue && insurancePrice.Value < 0)
                return (false, "Harga insurance tidak boleh kurang dari 0.");

            if (companyPrice.HasValue && companyPrice.Value < 0)
                return (false, "Harga company tidak boleh kurang dari 0.");

            if (effectiveStartDate.HasValue && effectiveEndDate.HasValue && effectiveEndDate.Value < effectiveStartDate.Value)
                return (false, "Tanggal akhir berlaku tidak boleh lebih kecil dari tanggal mulai berlaku.");

            var tariffCategoryExists = await _dbContext.Set<MstTariffCategory>()
                .AnyAsync(x => x.Id == tariffCategoryId && x.IsActive && !x.IsDelete);

            if (!tariffCategoryExists)
                return (false, "Kategori tariff tidak valid atau tidak aktif.");

            var normalizedServiceUnitId = NormalizeNullableGuid(serviceUnitId);

            if (normalizedServiceUnitId.HasValue)
            {
                var serviceUnitExists = await _dbContext.Set<MstServiceUnit>()
                    .AnyAsync(x => x.Id == normalizedServiceUnitId.Value && x.IsActive && !x.IsDelete);

                if (!serviceUnitExists)
                    return (false, "Service unit tidak valid atau tidak aktif.");
            }

            var normalizedClinicId = NormalizeNullableGuid(clinicId);

            if (normalizedClinicId.HasValue)
            {
                var clinic = await _dbContext.Set<MstClinic>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == normalizedClinicId.Value && x.IsActive && !x.IsDelete);

                if (clinic == null)
                    return (false, "Clinic tidak valid atau tidak aktif.");

                if (normalizedServiceUnitId.HasValue && clinic.ServiceUnitId != normalizedServiceUnitId.Value)
                    return (false, "Clinic tidak sesuai dengan service unit yang dipilih.");
            }

            var normalizedPatientClassId = NormalizeNullableGuid(patientClassId);

            if (normalizedPatientClassId.HasValue)
            {
                var patientClassExists = await _dbContext.Set<MstPatientClass>()
                    .AnyAsync(x => x.Id == normalizedPatientClassId.Value && x.IsActive && !x.IsDelete);

                if (!patientClassExists)
                    return (false, "Patient class tidak valid atau tidak aktif.");
            }

            var normalizedProcedureId = NormalizeNullableGuid(procedureId);
            var normalizedDrugId = NormalizeNullableGuid(drugId);

            if (normalizedProcedureId.HasValue && normalizedDrugId.HasValue)
                return (false, "Tariff tidak boleh terhubung ke procedure dan drug sekaligus.");

            if (normalizedProcedureId.HasValue)
            {
                var procedureExists = await _dbContext.Set<MstProcedure>()
                    .AnyAsync(x => x.Id == normalizedProcedureId.Value && x.IsActive && !x.IsDelete);

                if (!procedureExists)
                    return (false, "Procedure tidak valid atau tidak aktif.");
            }

            if (normalizedDrugId.HasValue)
            {
                var drugExists = await _dbContext.Set<MstDrug>()
                    .AnyAsync(x => x.Id == normalizedDrugId.Value && x.IsActive && !x.IsDelete);

                if (!drugExists)
                    return (false, "Drug tidak valid atau tidak aktif.");
            }

            var normalizedName = tariffName.Trim().ToLower();

            var duplicateNameInScope = await _dbContext.Set<MstTariff>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.TariffCategoryId == tariffCategoryId &&
                    x.ServiceUnitId == normalizedServiceUnitId &&
                    x.ClinicId == normalizedClinicId &&
                    x.PatientClassId == normalizedPatientClassId &&
                    x.ProcedureId == normalizedProcedureId &&
                    x.DrugId == normalizedDrugId &&
                    x.TariffName.ToLower() == normalizedName &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateNameInScope)
                return (false, "Nama tariff dengan kategori, service unit, clinic, patient class, procedure, dan drug tersebut sudah digunakan.");

            return (true, null);
        }

        private async Task<string> GenerateTariffCodeAsync()
        {
            var existingCodes = await _dbContext.Set<MstTariff>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.TariffCode.StartsWith(TariffCodePrefix))
                .Select(x => x.TariffCode)
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

            return $"{TariffCodePrefix}{nextNumber:00000}";
        }

        private static int? ExtractCodeNumber(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return null;

            if (!code.StartsWith(TariffCodePrefix, StringComparison.OrdinalIgnoreCase))
                return null;

            var numberText = code[TariffCodePrefix.Length..];

            return int.TryParse(numberText, out var number)
                ? number
                : null;
        }

        private static TariffCreateResponse ToCreateUpdateResponse(MstTariff entity)
        {
            return new TariffCreateResponse
            {
                Id = entity.Id,
                TariffCode = entity.TariffCode,
                TariffName = entity.TariffName,
                TariffCategoryId = entity.TariffCategoryId,
                ServiceUnitId = entity.ServiceUnitId,
                ClinicId = entity.ClinicId,
                PatientClassId = entity.PatientClassId,
                ProcedureId = entity.ProcedureId,
                DrugId = entity.DrugId,
                NormalPrice = entity.NormalPrice,
                IsActive = entity.IsActive
            };
        }

        private static TariffUpdateResponse ToUpdateResponse(MstTariff entity)
        {
            return new TariffUpdateResponse
            {
                Id = entity.Id,
                TariffCode = entity.TariffCode,
                TariffName = entity.TariffName,
                TariffCategoryId = entity.TariffCategoryId,
                ServiceUnitId = entity.ServiceUnitId,
                ClinicId = entity.ClinicId,
                PatientClassId = entity.PatientClassId,
                ProcedureId = entity.ProcedureId,
                DrugId = entity.DrugId,
                NormalPrice = entity.NormalPrice,
                IsActive = entity.IsActive
            };
        }

        private static IQueryable<MstTariff> ApplySorting(
            IQueryable<MstTariff> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "sortOrder").ToLowerInvariant() switch
            {
                "createdatetime" => isDesc
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),

                "tariffcode" => isDesc
                    ? query.OrderByDescending(x => x.TariffCode)
                    : query.OrderBy(x => x.TariffCode),

                "tariffname" => isDesc
                    ? query.OrderByDescending(x => x.TariffName)
                    : query.OrderBy(x => x.TariffName),

                "tariffcategoryname" => isDesc
                    ? query.OrderByDescending(x => x.TariffCategory != null ? x.TariffCategory.TariffCategoryName : "")
                    : query.OrderBy(x => x.TariffCategory != null ? x.TariffCategory.TariffCategoryName : ""),

                "serviceunitname" => isDesc
                    ? query.OrderByDescending(x => x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : "")
                    : query.OrderBy(x => x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : ""),

                "normalprice" => isDesc
                    ? query.OrderByDescending(x => x.NormalPrice)
                    : query.OrderBy(x => x.NormalPrice),

                "effectivestartdate" => isDesc
                    ? query.OrderByDescending(x => x.EffectiveStartDate)
                    : query.OrderBy(x => x.EffectiveStartDate),

                "effectiveenddate" => isDesc
                    ? query.OrderByDescending(x => x.EffectiveEndDate)
                    : query.OrderBy(x => x.EffectiveEndDate),

                "isactive" => isDesc
                    ? query.OrderByDescending(x => x.IsActive)
                    : query.OrderBy(x => x.IsActive),

                _ => isDesc
                    ? query.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.TariffName)
                    : query.OrderBy(x => x.SortOrder).ThenBy(x => x.TariffName)
            };
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            return (pageNumber, pageSize);
        }

        private static DateRangeResult ResolveDateRange(
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            var today = DateTime.UtcNow.Date;
            DateTime? start = startDate?.Date;
            DateTime? endExclusive = endDate?.Date.AddDays(1);

            if (!string.IsNullOrWhiteSpace(customPeriod) &&
                !string.Equals(customPeriod, "custom", StringComparison.OrdinalIgnoreCase))
            {
                var period = customPeriod.Trim().ToLowerInvariant();

                switch (period)
                {
                    case "today":
                        start = today;
                        endExclusive = today.AddDays(1);
                        break;

                    case "last7days":
                        start = today.AddDays(-6);
                        endExclusive = today.AddDays(1);
                        break;

                    case "thismonth":
                        start = new DateTime(today.Year, today.Month, 1);
                        endExclusive = start.Value.AddMonths(1);
                        break;

                    case "thisyear":
                        start = new DateTime(today.Year, 1, 1);
                        endExclusive = start.Value.AddYears(1);
                        break;

                    case "all":
                        start = null;
                        endExclusive = null;
                        break;

                    default:
                        return new DateRangeResult(false, null, null, "Custom period tidak valid.");
                }
            }

            if (start.HasValue && endExclusive.HasValue && endExclusive.Value <= start.Value)
                return new DateRangeResult(false, null, null, "EndDate harus lebih besar atau sama dengan StartDate.");

            return new DateRangeResult(true, start, endExclusive, null);
        }

        private static List<TariffCustomPeriodOptionResponse> BuildCustomPeriodOptions()
        {
            return new List<TariffCustomPeriodOptionResponse>
            {
                new() { Value = "all", Label = "Semua", Description = "Tidak membatasi tanggal dibuat.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "today", Label = "Hari ini", Description = "Data yang dibuat hari ini.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last7Days", Label = "7 hari terakhir", Description = "Data yang dibuat dalam 7 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "thisMonth", Label = "Bulan ini", Description = "Data yang dibuat pada bulan berjalan.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "thisYear", Label = "Tahun ini", Description = "Data yang dibuat pada tahun berjalan.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "custom", Label = "Custom", Description = "Gunakan startDate dan endDate.", UsesStartDate = true, UsesEndDate = true }
            };
        }

        private static List<TariffQueryParameterInfoResponse> BuildQueryParameterInfo()
        {
            return new List<TariffQueryParameterInfoResponse>
            {
                new() { Name = "startDate", Type = "DateTime?", Description = "Tanggal awal berdasarkan CreateDateTime.", Example = "2026-01-01" },
                new() { Name = "endDate", Type = "DateTime?", Description = "Tanggal akhir berdasarkan CreateDateTime.", Example = "2026-01-31" },
                new() { Name = "customPeriod", Type = "string", Description = "Preset periode. Jika bukan custom, startDate dan endDate diabaikan.", Example = "thisMonth" },
                new() { Name = "tariffCategoryId", Type = "Guid?", Description = "Filter relasi kategori tariff.", Example = "3fa85f64-5717-4562-b3fc-2c963f66afa6" },
                new() { Name = "serviceUnitId", Type = "Guid?", Description = "Filter relasi service unit.", Example = "3fa85f64-5717-4562-b3fc-2c963f66afa6" },
                new() { Name = "isActive", Type = "bool?", Description = "Filter status aktif.", Example = "true" },
                new() { Name = "search", Type = "string", Description = "Pencarian kode, nama, kategori, unit, clinic, procedure, drug, provider, dan deskripsi.", Example = "konsultasi" },
                new() { Name = "sortBy", Type = "string", Description = "Kolom sorting.", Example = "sortOrder" },
                new() { Name = "sortDirection", Type = "string", Description = "Arah sorting asc atau desc.", Example = "asc" },
                new() { Name = "pageNumber", Type = "int", Description = "Nomor halaman.", Example = "1" },
                new() { Name = "pageSize", Type = "int", Description = "Jumlah data per halaman maksimal 100.", Example = "25" }
            };
        }

        private static List<TariffFormFieldMetadataResponse> BuildCreateFieldMetadata()
        {
            return new List<TariffFormFieldMetadataResponse>
            {
                new() { Name = "tariffCode", Label = "Kode tariff", DataType = "string", InputType = "text", Required = false, IsReadonly = true, Placeholder = "Auto generated", Description = "Dibuat otomatis oleh sistem." },
                new() { Name = "tariffName", Label = "Nama tariff", DataType = "string", InputType = "text", Required = true, IsReadonly = false, Placeholder = "Contoh: Konsultasi Dokter Umum" },
                new() { Name = "tariffCategoryId", Label = "Kategori tariff", DataType = "Guid", InputType = "select", Required = true, IsReadonly = false },
                new() { Name = "serviceUnitId", Label = "Service unit", DataType = "Guid?", InputType = "select", Required = false, IsReadonly = false },
                new() { Name = "clinicId", Label = "Clinic", DataType = "Guid?", InputType = "select", Required = false, IsReadonly = false },
                new() { Name = "patientClassId", Label = "Kelas pasien", DataType = "Guid?", InputType = "select", Required = false, IsReadonly = false },
                new() { Name = "procedureId", Label = "Procedure", DataType = "Guid?", InputType = "select", Required = false, IsReadonly = false },
                new() { Name = "drugId", Label = "Drug", DataType = "Guid?", InputType = "select", Required = false, IsReadonly = false },
                new() { Name = "normalPrice", Label = "Harga normal", DataType = "decimal", InputType = "number", Required = true, IsReadonly = false },
                new() { Name = "memberPrice", Label = "Harga member", DataType = "decimal?", InputType = "number", Required = false, IsReadonly = false },
                new() { Name = "insurancePrice", Label = "Harga insurance", DataType = "decimal?", InputType = "number", Required = false, IsReadonly = false },
                new() { Name = "companyPrice", Label = "Harga company", DataType = "decimal?", InputType = "number", Required = false, IsReadonly = false },
                new() { Name = "effectiveStartDate", Label = "Mulai berlaku", DataType = "DateTime?", InputType = "date", Required = false, IsReadonly = false },
                new() { Name = "effectiveEndDate", Label = "Akhir berlaku", DataType = "DateTime?", InputType = "date", Required = false, IsReadonly = false },
                new() { Name = "sortOrder", Label = "Urutan", DataType = "int", InputType = "number", Required = false, IsReadonly = false }
            };
        }

        private static List<TariffFormFieldMetadataResponse> BuildUpdateFieldMetadata()
        {
            var fields = BuildCreateFieldMetadata();
            fields.Add(new TariffFormFieldMetadataResponse { Name = "isActive", Label = "Status aktif", DataType = "bool", InputType = "switch", Required = false, IsReadonly = false });
            return fields;
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

        private static Guid? NormalizeNullableGuid(Guid? value)
        {
            if (!value.HasValue || value.Value == Guid.Empty)
                return null;

            return value.Value;
        }

        private sealed class DateRangeResult
        {
            public DateRangeResult(bool isValid, DateTime? start, DateTime? endExclusive, string? errorMessage)
            {
                IsValid = isValid;
                Start = start;
                EndExclusive = endExclusive;
                ErrorMessage = errorMessage;
            }

            public bool IsValid { get; }
            public DateTime? Start { get; }
            public DateTime? EndExclusive { get; }
            public string? ErrorMessage { get; }
        }
    }
}
