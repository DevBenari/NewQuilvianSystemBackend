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
                SortOptions = new List<TariffSortOptionResponse>
                {
                    new() { Value = "sortOrder", Label = "Urutan" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "tariffCode", Label = "Kode tariff" },
                    new() { Value = "tariffName", Label = "Nama tariff" },
                    new() { Value = "tariffCategoryName", Label = "Kategori tariff" },
                    new() { Value = "serviceUnitName", Label = "Service unit" },
                    new() { Value = "clinicName", Label = "Clinic" },
                    new() { Value = "patientClassName", Label = "Kelas pasien" },
                    new() { Value = "procedureName", Label = "Procedure" },
                    new() { Value = "drugName", Label = "Drug" },
                    new() { Value = "normalPrice", Label = "Harga normal" },
                    new() { Value = "effectiveStartDate", Label = "Tanggal mulai berlaku" },
                    new() { Value = "effectiveEndDate", Label = "Tanggal akhir berlaku" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
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
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            [FromQuery] Guid? tariffCategoryId,
            [FromQuery] Guid? serviceUnitId,
            [FromQuery] Guid? clinicId,
            [FromQuery] Guid? patientClassId,
            [FromQuery] Guid? procedureId,
            [FromQuery] Guid? drugId,
            [FromQuery] string? providerName,
            [FromQuery] bool? isSurgeryRelated,
            [FromQuery] bool? isRoomCharge,
            [FromQuery] bool? isAdministrationFee,
            [FromQuery] bool? isRegistrationFee,
            [FromQuery] bool? isConsultationFee,
            [FromQuery] bool? isPackageTariff,
            [FromQuery] bool? isNeedDoctor,
            [FromQuery] bool? isNeedApproval,
            [FromQuery] bool? isTaxable,
            [FromQuery] DateTime? effectiveDate,
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

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (tariffCategoryId.HasValue && tariffCategoryId.Value != Guid.Empty)
                query = query.Where(x => x.TariffCategoryId == tariffCategoryId.Value);

            if (serviceUnitId.HasValue && serviceUnitId.Value != Guid.Empty)
                query = query.Where(x => x.ServiceUnitId == serviceUnitId.Value);

            if (clinicId.HasValue && clinicId.Value != Guid.Empty)
                query = query.Where(x => x.ClinicId == clinicId.Value);

            if (patientClassId.HasValue && patientClassId.Value != Guid.Empty)
                query = query.Where(x => x.PatientClassId == patientClassId.Value);

            if (procedureId.HasValue && procedureId.Value != Guid.Empty)
                query = query.Where(x => x.ProcedureId == procedureId.Value);

            if (drugId.HasValue && drugId.Value != Guid.Empty)
                query = query.Where(x => x.DrugId == drugId.Value);

            if (!string.IsNullOrWhiteSpace(providerName))
            {
                var providerKeyword = providerName.Trim().ToLower();
                query = query.Where(x => x.ProviderName != null && x.ProviderName.ToLower().Contains(providerKeyword));
            }

            if (isSurgeryRelated.HasValue)
                query = query.Where(x => x.IsSurgeryRelated == isSurgeryRelated.Value);

            if (isRoomCharge.HasValue)
                query = query.Where(x => x.IsRoomCharge == isRoomCharge.Value);

            if (isAdministrationFee.HasValue)
                query = query.Where(x => x.IsAdministrationFee == isAdministrationFee.Value);

            if (isRegistrationFee.HasValue)
                query = query.Where(x => x.IsRegistrationFee == isRegistrationFee.Value);

            if (isConsultationFee.HasValue)
                query = query.Where(x => x.IsConsultationFee == isConsultationFee.Value);

            if (isPackageTariff.HasValue)
                query = query.Where(x => x.IsPackageTariff == isPackageTariff.Value);

            if (isNeedDoctor.HasValue)
                query = query.Where(x => x.IsNeedDoctor == isNeedDoctor.Value);

            if (isNeedApproval.HasValue)
                query = query.Where(x => x.IsNeedApproval == isNeedApproval.Value);

            if (isTaxable.HasValue)
                query = query.Where(x => x.IsTaxable == isTaxable.Value);

            if (effectiveDate.HasValue)
            {
                var selectedDate = effectiveDate.Value;

                query = query.Where(x =>
                    (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value <= selectedDate) &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= selectedDate));
            }

            if (minimumPrice.HasValue)
                query = query.Where(x => x.NormalPrice >= minimumPrice.Value);

            if (maximumPrice.HasValue)
                query = query.Where(x => x.NormalPrice <= maximumPrice.Value);

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
        [ProducesResponseType(typeof(ApiResponse<List<TariffOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Tariff", Description = "Melihat data tariff", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Tariff", "Read")]
        public async Task<IActionResult> GetTariffOptions(
            [FromQuery] Guid? tariffCategoryId,
            [FromQuery] Guid? serviceUnitId,
            [FromQuery] Guid? clinicId,
            [FromQuery] Guid? patientClassId,
            [FromQuery] Guid? procedureId,
            [FromQuery] Guid? drugId,
            [FromQuery] bool? isNeedDoctor,
            [FromQuery] bool? isNeedApproval,
            [FromQuery] bool? isTaxable,
            [FromQuery] DateTime? effectiveDate,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null)
        {
            var query = BuildBaseQuery();

            if (onlyActive)
                query = query.Where(x => x.IsActive);

            if (tariffCategoryId.HasValue && tariffCategoryId.Value != Guid.Empty)
                query = query.Where(x => x.TariffCategoryId == tariffCategoryId.Value);

            if (serviceUnitId.HasValue && serviceUnitId.Value != Guid.Empty)
                query = query.Where(x => x.ServiceUnitId == serviceUnitId.Value);

            if (clinicId.HasValue && clinicId.Value != Guid.Empty)
                query = query.Where(x => x.ClinicId == clinicId.Value);

            if (patientClassId.HasValue && patientClassId.Value != Guid.Empty)
                query = query.Where(x => x.PatientClassId == patientClassId.Value);

            if (procedureId.HasValue && procedureId.Value != Guid.Empty)
                query = query.Where(x => x.ProcedureId == procedureId.Value);

            if (drugId.HasValue && drugId.Value != Guid.Empty)
                query = query.Where(x => x.DrugId == drugId.Value);

            if (isNeedDoctor.HasValue)
                query = query.Where(x => x.IsNeedDoctor == isNeedDoctor.Value);

            if (isNeedApproval.HasValue)
                query = query.Where(x => x.IsNeedApproval == isNeedApproval.Value);

            if (isTaxable.HasValue)
                query = query.Where(x => x.IsTaxable == isTaxable.Value);

            if (effectiveDate.HasValue)
            {
                var selectedDate = effectiveDate.Value;

                query = query.Where(x =>
                    (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value <= selectedDate) &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= selectedDate));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.TariffCode.ToLower().Contains(keyword) ||
                    x.TariffName.ToLower().Contains(keyword) ||
                    (x.TariffCategory != null && x.TariffCategory.TariffCategoryName.ToLower().Contains(keyword)) ||
                    (x.Procedure != null && x.Procedure.ProcedureCode.ToLower().Contains(keyword)) ||
                    (x.Procedure != null && x.Procedure.ProcedureName.ToLower().Contains(keyword)) ||
                    (x.Drug != null && x.Drug.DrugCode.ToLower().Contains(keyword)) ||
                    (x.Drug != null && x.Drug.DrugName.ToLower().Contains(keyword)));
            }

            var data = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.TariffName)
                .Select(x => new TariffOptionResponse
                {
                    Id = x.Id,
                    TariffCode = x.TariffCode,
                    TariffName = x.TariffName,
                    TariffCategoryId = x.TariffCategoryId,
                    TariffCategoryName = x.TariffCategory != null ? x.TariffCategory.TariffCategoryName : string.Empty,
                    ServiceUnitId = x.ServiceUnitId,
                    ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null,
                    ClinicId = x.ClinicId,
                    ClinicName = x.Clinic != null ? x.Clinic.ClinicName : null,
                    PatientClassId = x.PatientClassId,
                    PatientClassName = x.PatientClass != null ? x.PatientClass.PatientClassName : null,
                    ProcedureId = x.ProcedureId,
                    ProcedureName = x.Procedure != null ? x.Procedure.ProcedureName : null,
                    DrugId = x.DrugId,
                    DrugName = x.Drug != null ? x.Drug.DrugName : null,
                    NormalPrice = x.NormalPrice,
                    MemberPrice = x.MemberPrice,
                    InsurancePrice = x.InsurancePrice,
                    CompanyPrice = x.CompanyPrice,
                    IsNeedDoctor = x.IsNeedDoctor,
                    IsNeedApproval = x.IsNeedApproval,
                    IsTaxable = x.IsTaxable
                })
                .ToListAsync();

            return Ok(ApiResponse<List<TariffOptionResponse>>.Ok(
                data,
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
                tariffCode: request.TariffCode,
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

            var entity = new MstTariff
            {
                Id = Guid.NewGuid(),
                TariffCode = request.TariffCode.Trim().ToUpperInvariant(),
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

            var response = new TariffCreateResponse
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

            return Ok(ApiResponse<TariffCreateResponse>.Ok(
                response,
                "Tariff berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
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
                tariffCode: request.TariffCode,
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

            entity.TariffCode = request.TariffCode.Trim().ToUpperInvariant();
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

            return Ok(ApiResponse<object>.Ok(
                null,
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
            string tariffCode,
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

            if (string.IsNullOrWhiteSpace(tariffCode))
                return (false, "Kode tariff wajib diisi.");

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
                var clinicExists = await _dbContext.Set<MstClinic>()
                    .AnyAsync(x => x.Id == normalizedClinicId.Value && x.IsActive && !x.IsDelete);

                if (!clinicExists)
                    return (false, "Clinic tidak valid atau tidak aktif.");
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

            var normalizedCode = tariffCode.Trim().ToUpperInvariant();
            var normalizedName = tariffName.Trim().ToLower();

            var duplicateCode = await _dbContext.Set<MstTariff>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.TariffCode.ToUpper() == normalizedCode &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateCode)
                return (false, "Kode tariff sudah digunakan.");

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

                "clinicname" => isDesc
                    ? query.OrderByDescending(x => x.Clinic != null ? x.Clinic.ClinicName : "")
                    : query.OrderBy(x => x.Clinic != null ? x.Clinic.ClinicName : ""),

                "patientclassname" => isDesc
                    ? query.OrderByDescending(x => x.PatientClass != null ? x.PatientClass.PatientClassName : "")
                    : query.OrderBy(x => x.PatientClass != null ? x.PatientClass.PatientClassName : ""),

                "procedurename" => isDesc
                    ? query.OrderByDescending(x => x.Procedure != null ? x.Procedure.ProcedureName : "")
                    : query.OrderBy(x => x.Procedure != null ? x.Procedure.ProcedureName : ""),

                "drugname" => isDesc
                    ? query.OrderByDescending(x => x.Drug != null ? x.Drug.DrugName : "")
                    : query.OrderBy(x => x.Drug != null ? x.Drug.DrugName : ""),

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
    }
}
