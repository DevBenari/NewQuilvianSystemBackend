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
        private const int TariffCodeDigitLength = 5;

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
                    new() { Value = "clinicName", Label = "Clinic" },
                    new() { Value = "patientClassName", Label = "Kelas pasien" },
                    new() { Value = "procedureName", Label = "Procedure" },
                    new() { Value = "drugName", Label = "Drug" },
                    new() { Value = "normalPrice", Label = "Harga normal" },
                    new() { Value = "memberPrice", Label = "Harga member" },
                    new() { Value = "insurancePrice", Label = "Harga asuransi" },
                    new() { Value = "companyPrice", Label = "Harga company" },
                    new() { Value = "effectiveStartDate", Label = "Tanggal mulai berlaku" },
                    new() { Value = "effectiveEndDate", Label = "Tanggal akhir berlaku" },
                    new() { Value = "isNeedDoctor", Label = "Butuh dokter" },
                    new() { Value = "isNeedApproval", Label = "Butuh approval" },
                    new() { Value = "isTaxable", Label = "Kena pajak" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                EffectiveStatusOptions = new List<string> { "all", "effective", "expired", "future" },
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
            var query = BuildBaseQuery();

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
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Read", "Read Tariff", Description = "Melihat data tariff", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Tariff", "Read")]
        public async Task<IActionResult> GetTariffs(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? tariffCategoryId,
            [FromQuery] Guid? serviceUnitId,
            [FromQuery] Guid? clinicId,
            [FromQuery] Guid? patientClassId,
            [FromQuery] Guid? procedureId,
            [FromQuery] Guid? drugId,
            [FromQuery] bool? isActive,
            [FromQuery] bool? isSurgeryRelated,
            [FromQuery] bool? isRoomCharge,
            [FromQuery] bool? isAdministrationFee,
            [FromQuery] bool? isRegistrationFee,
            [FromQuery] bool? isConsultationFee,
            [FromQuery] bool? isPackageTariff,
            [FromQuery] bool? isNeedDoctor,
            [FromQuery] bool? isNeedApproval,
            [FromQuery] bool? isTaxable,
            [FromQuery] string? effectiveStatus,
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
            query = ApplyDateFilter(query, dateRange.Start, dateRange.EndExclusive);
            query = ApplyStandardFilter(
                query,
                tariffCategoryId,
                serviceUnitId,
                clinicId,
                patientClassId,
                procedureId,
                drugId,
                isActive,
                isSurgeryRelated,
                isRoomCharge,
                isAdministrationFee,
                isRegistrationFee,
                isConsultationFee,
                isPackageTariff,
                isNeedDoctor,
                isNeedApproval,
                isTaxable,
                effectiveStatus,
                search
            );

            var totalData = await query.CountAsync();
            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var actorNames = await GetActorNameMapAsync(entities.Select(x => x.CreateBy));
            var now = DateTime.UtcNow;

            var result = new ResponseTariffPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = entities.Select(x => MapResponse(x, actorNames, now)).ToList()
            };

            return Ok(ApiResponse<ResponseTariffPagedResult>.Ok(
                result,
                "Data tariff berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<TariffOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Tariff", Description = "Melihat data pilihan tariff", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Tariff", "Read")]
        public async Task<IActionResult> GetTariffOptions(
            [FromQuery] Guid? tariffCategoryId,
            [FromQuery] Guid? serviceUnitId,
            [FromQuery] Guid? clinicId,
            [FromQuery] Guid? patientClassId,
            [FromQuery] Guid? procedureId,
            [FromQuery] Guid? drugId,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? effectiveStatus = "effective",
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
                tariffCategoryId,
                serviceUnitId,
                clinicId,
                patientClassId,
                procedureId,
                drugId,
                onlyActive ? true : null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                effectiveStatus,
                search
            );

            var totalData = await query.CountAsync();
            var entities = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.TariffName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var now = DateTime.UtcNow;
            var result = new TariffOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = entities.Select(x => MapOptionResponse(x, now)).ToList()
            };

            return Ok(ApiResponse<TariffOptionPagedResponse>.Ok(
                result,
                "Data pilihan tariff berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<TariffDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Tariff", Description = "Melihat detail tariff", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Tariff", "Read")]
        public async Task<IActionResult> GetTariffById(Guid id)
        {
            var entity = await BuildBaseQuery()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Tariff tidak ditemukan."
                ));
            }

            var actorNames = await GetActorNameMapAsync(new[] { entity.CreateBy, entity.UpdateBy });
            var result = MapDetailResponse(entity, actorNames, DateTime.UtcNow);

            return Ok(ApiResponse<TariffDetailResponse>.Ok(
                result,
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
            var validation = await ValidateRequestAsync(null, request);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data tariff tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

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
                IsSurgeryRelated = request.IsSurgeryRelated,
                IsRoomCharge = request.IsRoomCharge,
                IsAdministrationFee = request.IsAdministrationFee,
                IsRegistrationFee = request.IsRegistrationFee,
                IsConsultationFee = request.IsConsultationFee,
                IsPackageTariff = request.IsPackageTariff,
                IsNeedDoctor = request.IsNeedDoctor,
                IsNeedApproval = request.IsNeedApproval,
                NormalPrice = request.NormalPrice,
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

            var response = MapCreateUpdateResponse(entity);

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

            var validation = await ValidateRequestAsync(id, request);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data tariff tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.TariffName = request.TariffName.Trim();
            entity.TariffCategoryId = request.TariffCategoryId;
            entity.ServiceUnitId = NormalizeNullableGuid(request.ServiceUnitId);
            entity.ClinicId = NormalizeNullableGuid(request.ClinicId);
            entity.PatientClassId = NormalizeNullableGuid(request.PatientClassId);
            entity.ProcedureId = NormalizeNullableGuid(request.ProcedureId);
            entity.DrugId = NormalizeNullableGuid(request.DrugId);
            entity.ExternalServiceCode = NormalizeNullableText(request.ExternalServiceCode);
            entity.ExternalClassCode = NormalizeNullableText(request.ExternalClassCode);
            entity.IsSurgeryRelated = request.IsSurgeryRelated;
            entity.IsRoomCharge = request.IsRoomCharge;
            entity.IsAdministrationFee = request.IsAdministrationFee;
            entity.IsRegistrationFee = request.IsRegistrationFee;
            entity.IsConsultationFee = request.IsConsultationFee;
            entity.IsPackageTariff = request.IsPackageTariff;
            entity.IsNeedDoctor = request.IsNeedDoctor;
            entity.IsNeedApproval = request.IsNeedApproval;
            entity.NormalPrice = request.NormalPrice;
            entity.EffectiveStartDate = request.EffectiveStartDate;
            entity.EffectiveEndDate = request.EffectiveEndDate;
            entity.IsTaxable = request.IsTaxable;
            entity.SortOrder = request.SortOrder;
            entity.Description = NormalizeNullableText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var response = new TariffUpdateResponse
            {
                Id = entity.Id,
                TariffCode = entity.TariffCode,
                TariffName = entity.TariffName,
                TariffCategoryId = entity.TariffCategoryId,
                ProcedureId = entity.ProcedureId,
                DrugId = entity.DrugId,
                ServiceUnitId = entity.ServiceUnitId,
                ClinicId = entity.ClinicId,
                PatientClassId = entity.PatientClassId,
                NormalPrice = entity.NormalPrice,
                IsActive = entity.IsActive
            };

            return Ok(ApiResponse<TariffUpdateResponse>.Ok(
                response,
                "Tariff berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<TariffUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Tariff Status", Description = "Mengubah status aktif tariff", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("Tariff", "Update")]
        public async Task<IActionResult> UpdateTariffStatus(Guid id, [FromBody] UpdateTariffStatusRequest request)
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

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            var response = new TariffUpdateResponse
            {
                Id = entity.Id,
                TariffCode = entity.TariffCode,
                TariffName = entity.TariffName,
                TariffCategoryId = entity.TariffCategoryId,
                ProcedureId = entity.ProcedureId,
                DrugId = entity.DrugId,
                ServiceUnitId = entity.ServiceUnitId,
                ClinicId = entity.ClinicId,
                PatientClassId = entity.PatientClassId,
                NormalPrice = entity.NormalPrice,
                IsActive = entity.IsActive
            };

            return Ok(ApiResponse<TariffUpdateResponse>.Ok(
                response,
                request.IsActive
                    ? "Tariff berhasil diaktifkan."
                    : "Tariff berhasil dinonaktifkan."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<TariffDeleteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Tariff", Description = "Menghapus data tariff", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("Tariff", "Delete")]
        public async Task<IActionResult> DeleteTariff(Guid id, [FromBody] DeleteTariffRequest? request = null)
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

            var response = new TariffDeleteResponse
            {
                Id = entity.Id,
                TariffCode = entity.TariffCode,
                TariffName = entity.TariffName,
                IsDelete = entity.IsDelete,
                DeleteDateTime = entity.DeleteDateTime
            };

            return Ok(ApiResponse<TariffDeleteResponse>.Ok(
                response,
                "Tariff berhasil dihapus."
            ));
        }

        private IQueryable<MstTariff> BuildBaseQuery()
        {
            return _dbContext.Set<MstTariff>()
                .AsNoTracking()
                .Include(x => x.TariffCategory)
                .Include(x => x.ServiceUnit)
                .Include(x => x.Clinic)
                .Include(x => x.PatientClass)
                .Include(x => x.Procedure)
                .Include(x => x.Drug)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstTariff> ApplyDateFilter(
            IQueryable<MstTariff> query,
            DateTime? start,
            DateTime? endExclusive)
        {
            if (start.HasValue)
                query = query.Where(x => x.CreateDateTime >= start.Value);

            if (endExclusive.HasValue)
                query = query.Where(x => x.CreateDateTime < endExclusive.Value);

            return query;
        }

        private static IQueryable<MstTariff> ApplyStandardFilter(
            IQueryable<MstTariff> query,
            Guid? tariffCategoryId,
            Guid? serviceUnitId,
            Guid? clinicId,
            Guid? patientClassId,
            Guid? procedureId,
            Guid? drugId,
            bool? isActive,
            bool? isSurgeryRelated,
            bool? isRoomCharge,
            bool? isAdministrationFee,
            bool? isRegistrationFee,
            bool? isConsultationFee,
            bool? isPackageTariff,
            bool? isNeedDoctor,
            bool? isNeedApproval,
            bool? isTaxable,
            string? effectiveStatus,
            string? search)
        {
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

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

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

            query = ApplyEffectiveStatusFilter(query, effectiveStatus);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.TariffCode.ToLower().Contains(keyword) ||
                    x.TariffName.ToLower().Contains(keyword) ||
                    (x.ExternalServiceCode != null && x.ExternalServiceCode.ToLower().Contains(keyword)) ||
                    (x.ExternalClassCode != null && x.ExternalClassCode.ToLower().Contains(keyword)) ||
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

            return query;
        }

        private static IQueryable<MstTariff> ApplyEffectiveStatusFilter(
            IQueryable<MstTariff> query,
            string? effectiveStatus)
        {
            if (string.IsNullOrWhiteSpace(effectiveStatus) ||
                string.Equals(effectiveStatus, "all", StringComparison.OrdinalIgnoreCase))
            {
                return query;
            }

            var now = DateTime.UtcNow;
            var status = effectiveStatus.Trim().ToLowerInvariant();

            return status switch
            {
                "effective" => query.Where(x =>
                    (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value <= now) &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= now)),

                "expired" => query.Where(x =>
                    x.EffectiveEndDate.HasValue && x.EffectiveEndDate.Value < now),

                "future" => query.Where(x =>
                    x.EffectiveStartDate.HasValue && x.EffectiveStartDate.Value > now),

                _ => query
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            CreateTariffRequest request)
        {
            if (request.TariffCategoryId == Guid.Empty)
                return (false, "Kategori tariff wajib dipilih.");

            if (string.IsNullOrWhiteSpace(request.TariffName))
                return (false, "Nama tariff wajib diisi.");

            if (request.NormalPrice < 0)
                return (false, "Harga normal tidak boleh kurang dari 0.");

            if (request.EffectiveStartDate.HasValue && request.EffectiveEndDate.HasValue &&
                request.EffectiveEndDate.Value < request.EffectiveStartDate.Value)
            {
                return (false, "Tanggal akhir berlaku tidak boleh lebih kecil dari tanggal mulai berlaku.");
            }

            var tariffCategoryExists = await _dbContext.Set<MstTariffCategory>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == request.TariffCategoryId && x.IsActive && !x.IsDelete);

            if (!tariffCategoryExists)
                return (false, "Kategori tariff tidak valid atau tidak aktif.");

            var normalizedServiceUnitId = NormalizeNullableGuid(request.ServiceUnitId);
            if (normalizedServiceUnitId.HasValue)
            {
                var serviceUnitExists = await _dbContext.Set<MstServiceUnit>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == normalizedServiceUnitId.Value && x.IsActive && !x.IsDelete);

                if (!serviceUnitExists)
                    return (false, "Service unit tidak valid atau tidak aktif.");
            }

            var normalizedClinicId = NormalizeNullableGuid(request.ClinicId);
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

            var normalizedPatientClassId = NormalizeNullableGuid(request.PatientClassId);
            if (normalizedPatientClassId.HasValue)
            {
                var patientClassExists = await _dbContext.Set<MstPatientClass>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == normalizedPatientClassId.Value && x.IsActive && !x.IsDelete);

                if (!patientClassExists)
                    return (false, "Patient class tidak valid atau tidak aktif.");
            }

            var normalizedProcedureId = NormalizeNullableGuid(request.ProcedureId);
            var normalizedDrugId = NormalizeNullableGuid(request.DrugId);

            if (normalizedProcedureId.HasValue && normalizedDrugId.HasValue)
                return (false, "Tariff tidak boleh terhubung ke procedure dan drug sekaligus.");

            if (normalizedProcedureId.HasValue)
            {
                var procedureExists = await _dbContext.Set<MstProcedure>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == normalizedProcedureId.Value && x.IsActive && !x.IsDelete);

                if (!procedureExists)
                    return (false, "Procedure tidak valid atau tidak aktif.");
            }

            if (normalizedDrugId.HasValue)
            {
                var drugExists = await _dbContext.Set<MstDrug>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == normalizedDrugId.Value && x.IsActive && !x.IsDelete);

                if (!drugExists)
                    return (false, "Drug tidak valid atau tidak aktif.");
            }

            var normalizedName = request.TariffName.Trim().ToLower();

            var duplicateQuery = _dbContext.Set<MstTariff>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.TariffCategoryId == request.TariffCategoryId &&
                    x.ServiceUnitId == normalizedServiceUnitId &&
                    x.ClinicId == normalizedClinicId &&
                    x.PatientClassId == normalizedPatientClassId &&
                    x.ProcedureId == normalizedProcedureId &&
                    x.DrugId == normalizedDrugId &&
                    x.TariffName.ToLower() == normalizedName);

            if (excludeId.HasValue)
                duplicateQuery = duplicateQuery.Where(x => x.Id != excludeId.Value);

            if (await duplicateQuery.AnyAsync())
            {
                return (false, "Nama tariff dengan kategori, service unit, clinic, patient class, procedure, dan drug tersebut sudah digunakan.");
            }

            return (true, null);
        }

        private async Task<string> GenerateTariffCodeAsync()
        {
            var existingCodes = await _dbContext.Set<MstTariff>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.TariffCode.StartsWith(TariffCodePrefix))
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

            return $"{TariffCodePrefix}{nextNumber.ToString().PadLeft(TariffCodeDigitLength, '0')}";
        }

        private static int? ExtractCodeNumber(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return null;

            if (!code.StartsWith(TariffCodePrefix, StringComparison.OrdinalIgnoreCase))
                return null;

            var numberText = code[TariffCodePrefix.Length..];
            return int.TryParse(numberText, out var number) ? number : null;
        }

        private static IQueryable<MstTariff> ApplySorting(
            IQueryable<MstTariff> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "sortOrder").ToLowerInvariant() switch
            {
                "createdatetime" => isDesc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "tariffcode" => isDesc ? query.OrderByDescending(x => x.TariffCode) : query.OrderBy(x => x.TariffCode),
                "tariffname" => isDesc ? query.OrderByDescending(x => x.TariffName) : query.OrderBy(x => x.TariffName),
                "tariffcategoryname" => isDesc ? query.OrderByDescending(x => x.TariffCategory != null ? x.TariffCategory.TariffCategoryName : "") : query.OrderBy(x => x.TariffCategory != null ? x.TariffCategory.TariffCategoryName : ""),
                "serviceunitname" => isDesc ? query.OrderByDescending(x => x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : "") : query.OrderBy(x => x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : ""),
                "clinicname" => isDesc ? query.OrderByDescending(x => x.Clinic != null ? x.Clinic.ClinicName : "") : query.OrderBy(x => x.Clinic != null ? x.Clinic.ClinicName : ""),
                "patientclassname" => isDesc ? query.OrderByDescending(x => x.PatientClass != null ? x.PatientClass.PatientClassName : "") : query.OrderBy(x => x.PatientClass != null ? x.PatientClass.PatientClassName : ""),
                "procedurename" => isDesc ? query.OrderByDescending(x => x.Procedure != null ? x.Procedure.ProcedureName : "") : query.OrderBy(x => x.Procedure != null ? x.Procedure.ProcedureName : ""),
                "drugname" => isDesc ? query.OrderByDescending(x => x.Drug != null ? x.Drug.DrugName : "") : query.OrderBy(x => x.Drug != null ? x.Drug.DrugName : ""),
                "normalprice" => isDesc ? query.OrderByDescending(x => x.NormalPrice) : query.OrderBy(x => x.NormalPrice),
                "effectivestartdate" => isDesc ? query.OrderByDescending(x => x.EffectiveStartDate) : query.OrderBy(x => x.EffectiveStartDate),
                "effectiveenddate" => isDesc ? query.OrderByDescending(x => x.EffectiveEndDate) : query.OrderBy(x => x.EffectiveEndDate),
                "isneeddoctor" => isDesc ? query.OrderByDescending(x => x.IsNeedDoctor) : query.OrderBy(x => x.IsNeedDoctor),
                "isneedapproval" => isDesc ? query.OrderByDescending(x => x.IsNeedApproval) : query.OrderBy(x => x.IsNeedApproval),
                "istaxable" => isDesc ? query.OrderByDescending(x => x.IsTaxable) : query.OrderBy(x => x.IsTaxable),
                "isactive" => isDesc ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
                _ => isDesc
                    ? query.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.TariffName)
                    : query.OrderBy(x => x.SortOrder).ThenBy(x => x.TariffName)
            };
        }

        private static DateRangeResult ResolveDateRange(
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            var today = AppDateTimeHelper.OperationalDate();
            DateTime? start = startDate?.Date;
            DateTime? endExclusive = endDate?.Date.AddDays(1);

            if (!string.IsNullOrWhiteSpace(customPeriod) &&
                !string.Equals(customPeriod, "custom", StringComparison.OrdinalIgnoreCase))
            {
                var period = customPeriod.Trim().ToLowerInvariant();

                switch (period)
                {
                    case "all":
                        start = null;
                        endExclusive = null;
                        break;
                    case "today":
                        start = today;
                        endExclusive = today.AddDays(1);
                        break;
                    case "yesterday":
                        start = today.AddDays(-1);
                        endExclusive = today;
                        break;
                    case "last7days":
                        start = today.AddDays(-6);
                        endExclusive = today.AddDays(1);
                        break;
                    case "last30days":
                        start = today.AddDays(-29);
                        endExclusive = today.AddDays(1);
                        break;
                    case "thismonth":
                        start = new DateTime(today.Year, today.Month, 1);
                        endExclusive = start.Value.AddMonths(1);
                        break;
                    case "lastmonth":
                        var currentMonthStart = new DateTime(today.Year, today.Month, 1);
                        start = currentMonthStart.AddMonths(-1);
                        endExclusive = currentMonthStart;
                        break;
                    case "thisyear":
                        start = new DateTime(today.Year, 1, 1);
                        endExclusive = start.Value.AddYears(1);
                        break;
                    default:
                        return DateRangeResult.Invalid("Custom period tidak valid.");
                }
            }

            if (start.HasValue && endExclusive.HasValue && start.Value >= endExclusive.Value)
                return DateRangeResult.Invalid("StartDate tidak boleh lebih besar dari EndDate.");

            return DateRangeResult.Valid(start, endExclusive);
        }

        private static List<TariffCustomPeriodOptionResponse> BuildCustomPeriodOptions()
        {
            return new List<TariffCustomPeriodOptionResponse>
            {
                new() { Value = "all", Label = "Semua", Description = "Tidak membatasi tanggal dibuat.", UsesStartDate = false, UsesEndDate = false },
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

        private static List<TariffQueryParameterInfoResponse> BuildQueryParameterInfo()
        {
            return new List<TariffQueryParameterInfoResponse>
            {
                new() { Name = "startDate", Type = "date", Description = "Tanggal awal berdasarkan CreateDateTime.", Example = "2026-01-01" },
                new() { Name = "endDate", Type = "date", Description = "Tanggal akhir berdasarkan CreateDateTime.", Example = "2026-01-31" },
                new() { Name = "customPeriod", Type = "string", Description = "Preset periode. Jika bukan custom, startDate dan endDate diabaikan.", Example = "thisMonth" },
                new() { Name = "tariffCategoryId", Type = "guid", Description = "Filter kategori tariff.", Example = "3fa85f64-5717-4562-b3fc-2c963f66afa6" },
                new() { Name = "serviceUnitId", Type = "guid", Description = "Filter service unit.", Example = "3fa85f64-5717-4562-b3fc-2c963f66afa6" },
                new() { Name = "clinicId", Type = "guid", Description = "Filter clinic.", Example = "3fa85f64-5717-4562-b3fc-2c963f66afa6" },
                new() { Name = "patientClassId", Type = "guid", Description = "Filter kelas pasien.", Example = "3fa85f64-5717-4562-b3fc-2c963f66afa6" },
                new() { Name = "procedureId", Type = "guid", Description = "Filter procedure.", Example = "3fa85f64-5717-4562-b3fc-2c963f66afa6" },
                new() { Name = "drugId", Type = "guid", Description = "Filter drug.", Example = "3fa85f64-5717-4562-b3fc-2c963f66afa6" },
                new() { Name = "effectiveStatus", Type = "string", Description = "all, effective, expired, future.", Example = "effective" },
                new() { Name = "isActive", Type = "bool", Description = "Filter status aktif.", Example = "true" },
                new() { Name = "search", Type = "string", Description = "Pencarian kode, nama, kategori, unit, clinic, procedure, drug, provider, atau deskripsi.", Example = "konsultasi" },
                new() { Name = "sortBy", Type = "string", Description = "Kolom sorting.", Example = "sortOrder" },
                new() { Name = "sortDirection", Type = "string", Description = "asc atau desc.", Example = "asc" },
                new() { Name = "pageNumber", Type = "int", Description = "Nomor halaman.", Example = "1" },
                new() { Name = "pageSize", Type = "int", Description = "Jumlah data per halaman maksimal 100.", Example = "25" }
            };
        }

        private static List<TariffFormFieldMetadataResponse> BuildCreateFieldMetadata()
        {
            return BuildFieldMetadata(false);
        }

        private static List<TariffFormFieldMetadataResponse> BuildUpdateFieldMetadata()
        {
            return BuildFieldMetadata(true);
        }

        private static List<TariffFormFieldMetadataResponse> BuildFieldMetadata(bool isUpdate)
        {
            var fields = new List<TariffFormFieldMetadataResponse>
            {
                new() { Name = "tariffCode", Label = "Kode tariff", Section = "Basic", InputType = "readonly", RequiredType = "AutoGenerated", MaxLength = 50, Description = "Dibuat otomatis oleh sistem dengan format TF-RSMMC-00001.", Example = "TF-RSMMC-00001", SortOrder = 1 },
                new() { Name = "tariffName", Label = "Nama tariff", Section = "Basic", InputType = "text", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", MaxLength = 250, Example = "Konsultasi Dokter Umum", SortOrder = 2 },
                new() { Name = "tariffCategoryId", Label = "Kategori tariff", Section = "Basic", InputType = "select", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", OptionsSource = "/api/v1/health-services/master-data/tariff-categories/options", SortOrder = 3 },
                new() { Name = "serviceUnitId", Label = "Service unit", Section = "Scope", InputType = "select", OptionsSource = "/api/v1/health-services/master-data/service-units/options", SortOrder = 4 },
                new() { Name = "clinicId", Label = "Clinic", Section = "Scope", InputType = "select", OptionsSource = "/api/v1/health-services/master-data/clinics/options", SortOrder = 5 },
                new() { Name = "patientClassId", Label = "Kelas pasien", Section = "Scope", InputType = "select", OptionsSource = "/api/v1/health-services/master-data/patient-classes/options", SortOrder = 6 },
                new() { Name = "procedureId", Label = "Procedure", Section = "Mapping", InputType = "select", OptionsSource = "/api/v1/health-services/master-data/procedures/options", Description = "Opsional. Jangan diisi bersamaan dengan drugId.", SortOrder = 7 },
                new() { Name = "drugId", Label = "Drug", Section = "Mapping", InputType = "select", OptionsSource = "/api/v1/health-services/master-data/drugs/options", Description = "Opsional. Jangan diisi bersamaan dengan procedureId.", SortOrder = 8 },
                new() { Name = "externalServiceCode", Label = "Kode service eksternal", Section = "Integration", InputType = "text", MaxLength = 50, SortOrder = 9 },
                new() { Name = "externalClassCode", Label = "Kode kelas eksternal", Section = "Integration", InputType = "text", MaxLength = 50, SortOrder = 10 },
                new() { Name = "providerName", Label = "Provider", Section = "Integration", InputType = "text", MaxLength = 100, SortOrder = 11 },
                new() { Name = "normalPrice", Label = "Harga normal", Section = "Price", InputType = "number", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", Example = "150000", SortOrder = 12 },
                new() { Name = "memberPrice", Label = "Harga member", Section = "Price", InputType = "number", SortOrder = 13 },
                new() { Name = "insurancePrice", Label = "Harga insurance", Section = "Price", InputType = "number", SortOrder = 14 },
                new() { Name = "companyPrice", Label = "Harga company", Section = "Price", InputType = "number", SortOrder = 15 },
                new() { Name = "effectiveStartDate", Label = "Tanggal mulai berlaku", Section = "Validity", InputType = "date", SortOrder = 16 },
                new() { Name = "effectiveEndDate", Label = "Tanggal akhir berlaku", Section = "Validity", InputType = "date", SortOrder = 17 },
                new() { Name = "isSurgeryRelated", Label = "Terkait operasi", Section = "Rule", InputType = "switch", SortOrder = 18 },
                new() { Name = "isRoomCharge", Label = "Biaya kamar", Section = "Rule", InputType = "switch", SortOrder = 19 },
                new() { Name = "isAdministrationFee", Label = "Biaya administrasi", Section = "Rule", InputType = "switch", SortOrder = 20 },
                new() { Name = "isRegistrationFee", Label = "Biaya registrasi", Section = "Rule", InputType = "switch", SortOrder = 21 },
                new() { Name = "isConsultationFee", Label = "Biaya konsultasi", Section = "Rule", InputType = "switch", SortOrder = 22 },
                new() { Name = "isPackageTariff", Label = "Paket tariff", Section = "Rule", InputType = "switch", SortOrder = 23 },
                new() { Name = "isNeedDoctor", Label = "Butuh dokter", Section = "Rule", InputType = "switch", SortOrder = 24 },
                new() { Name = "isNeedApproval", Label = "Butuh approval", Section = "Rule", InputType = "switch", SortOrder = 25 },
                new() { Name = "isTaxable", Label = "Kena pajak", Section = "Tax", InputType = "switch", SortOrder = 26 },
                new() { Name = "sortOrder", Label = "Urutan", Section = "Display", InputType = "number", SortOrder = 27 },
                new() { Name = "description", Label = "Deskripsi", Section = "Additional", InputType = "textarea", MaxLength = 250, SortOrder = 28 }
            };

            if (isUpdate)
            {
                fields.Add(new TariffFormFieldMetadataResponse
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

        private static TariffResponse MapResponse(
            MstTariff entity,
            IReadOnlyDictionary<Guid, string?> actorNames,
            DateTime now)
        {
            return new TariffResponse
            {
                Id = entity.Id,
                TariffCode = entity.TariffCode,
                TariffName = entity.TariffName,
                TariffCategoryId = entity.TariffCategoryId,
                TariffCategoryCode = entity.TariffCategory?.TariffCategoryCode ?? string.Empty,
                TariffCategoryName = entity.TariffCategory?.TariffCategoryName ?? string.Empty,
                TariffGroupName = entity.TariffCategory?.TariffGroupName,
                ServiceUnitId = entity.ServiceUnitId,
                ServiceUnitCode = entity.ServiceUnit?.ServiceUnitCode,
                ServiceUnitName = entity.ServiceUnit?.ServiceUnitName,
                ClinicId = entity.ClinicId,
                ClinicCode = entity.Clinic?.ClinicCode,
                ClinicName = entity.Clinic?.ClinicName,
                PatientClassId = entity.PatientClassId,
                PatientClassCode = entity.PatientClass?.PatientClassCode,
                PatientClassName = entity.PatientClass?.PatientClassName,
                ProcedureId = entity.ProcedureId,
                ProcedureCode = entity.Procedure?.ProcedureCode,
                ProcedureName = entity.Procedure?.ProcedureName,
                DrugId = entity.DrugId,
                DrugCode = entity.Drug?.DrugCode,
                DrugName = entity.Drug?.DrugName,
                ExternalServiceCode = entity.ExternalServiceCode,
                ExternalClassCode = entity.ExternalClassCode,
                IsSurgeryRelated = entity.IsSurgeryRelated,
                IsRoomCharge = entity.IsRoomCharge,
                IsAdministrationFee = entity.IsAdministrationFee,
                IsRegistrationFee = entity.IsRegistrationFee,
                IsConsultationFee = entity.IsConsultationFee,
                IsPackageTariff = entity.IsPackageTariff,
                IsNeedDoctor = entity.IsNeedDoctor,
                IsNeedApproval = entity.IsNeedApproval,
                NormalPrice = entity.NormalPrice,
                EffectiveStartDate = entity.EffectiveStartDate,
                EffectiveEndDate = entity.EffectiveEndDate,
                IsTaxable = entity.IsTaxable,
                IsCurrentlyEffective = IsCurrentlyEffective(entity, now),
                SortOrder = entity.SortOrder,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy)
            };
        }

        private static TariffDetailResponse MapDetailResponse(
            MstTariff entity,
            IReadOnlyDictionary<Guid, string?> actorNames,
            DateTime now)
        {
            var response = new TariffDetailResponse
            {
                Description = entity.Description,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };

            var baseResponse = MapResponse(entity, actorNames, now);
            response.Id = baseResponse.Id;
            response.TariffCode = baseResponse.TariffCode;
            response.TariffName = baseResponse.TariffName;
            response.TariffCategoryId = baseResponse.TariffCategoryId;
            response.TariffCategoryCode = baseResponse.TariffCategoryCode;
            response.TariffCategoryName = baseResponse.TariffCategoryName;
            response.TariffGroupName = baseResponse.TariffGroupName;
            response.ServiceUnitId = baseResponse.ServiceUnitId;
            response.ServiceUnitCode = baseResponse.ServiceUnitCode;
            response.ServiceUnitName = baseResponse.ServiceUnitName;
            response.ClinicId = baseResponse.ClinicId;
            response.ClinicCode = baseResponse.ClinicCode;
            response.ClinicName = baseResponse.ClinicName;
            response.PatientClassId = baseResponse.PatientClassId;
            response.PatientClassCode = baseResponse.PatientClassCode;
            response.PatientClassName = baseResponse.PatientClassName;
            response.ProcedureId = baseResponse.ProcedureId;
            response.ProcedureCode = baseResponse.ProcedureCode;
            response.ProcedureName = baseResponse.ProcedureName;
            response.DrugId = baseResponse.DrugId;
            response.DrugCode = baseResponse.DrugCode;
            response.DrugName = baseResponse.DrugName;
            response.ExternalServiceCode = baseResponse.ExternalServiceCode;
            response.ExternalClassCode = baseResponse.ExternalClassCode;
            response.IsSurgeryRelated = baseResponse.IsSurgeryRelated;
            response.IsRoomCharge = baseResponse.IsRoomCharge;
            response.IsAdministrationFee = baseResponse.IsAdministrationFee;
            response.IsRegistrationFee = baseResponse.IsRegistrationFee;
            response.IsConsultationFee = baseResponse.IsConsultationFee;
            response.IsPackageTariff = baseResponse.IsPackageTariff;
            response.IsNeedDoctor = baseResponse.IsNeedDoctor;
            response.IsNeedApproval = baseResponse.IsNeedApproval;
            response.NormalPrice = baseResponse.NormalPrice;
            response.EffectiveStartDate = baseResponse.EffectiveStartDate;
            response.EffectiveEndDate = baseResponse.EffectiveEndDate;
            response.IsTaxable = baseResponse.IsTaxable;
            response.IsCurrentlyEffective = baseResponse.IsCurrentlyEffective;
            response.SortOrder = baseResponse.SortOrder;
            response.IsActive = baseResponse.IsActive;
            response.CreateDateTime = baseResponse.CreateDateTime;
            response.CreateBy = baseResponse.CreateBy;
            response.CreateByName = baseResponse.CreateByName;

            return response;
        }

        private static TariffOptionResponse MapOptionResponse(MstTariff entity, DateTime now)
        {
            return new TariffOptionResponse
            {
                Id = entity.Id,
                TariffCode = entity.TariffCode,
                TariffName = entity.TariffName,
                TariffCategoryId = entity.TariffCategoryId,
                TariffCategoryName = entity.TariffCategory?.TariffCategoryName ?? string.Empty,
                ServiceUnitId = entity.ServiceUnitId,
                ServiceUnitName = entity.ServiceUnit?.ServiceUnitName,
                ClinicId = entity.ClinicId,
                ClinicName = entity.Clinic?.ClinicName,
                PatientClassId = entity.PatientClassId,
                PatientClassName = entity.PatientClass?.PatientClassName,
                ProcedureId = entity.ProcedureId,
                ProcedureName = entity.Procedure?.ProcedureName,
                DrugId = entity.DrugId,
                DrugName = entity.Drug?.DrugName,
                NormalPrice = entity.NormalPrice,
                IsNeedDoctor = entity.IsNeedDoctor,
                IsNeedApproval = entity.IsNeedApproval,
                IsTaxable = entity.IsTaxable,
                IsCurrentlyEffective = IsCurrentlyEffective(entity, now)
            };
        }

        private static TariffCreateResponse MapCreateUpdateResponse(MstTariff entity)
        {
            return new TariffCreateResponse
            {
                Id = entity.Id,
                TariffCode = entity.TariffCode,
                TariffName = entity.TariffName,
                TariffCategoryId = entity.TariffCategoryId,
                ProcedureId = entity.ProcedureId,
                DrugId = entity.DrugId,
                ServiceUnitId = entity.ServiceUnitId,
                ClinicId = entity.ClinicId,
                PatientClassId = entity.PatientClassId,
                NormalPrice = entity.NormalPrice,
                IsActive = entity.IsActive
            };
        }

        private static bool IsCurrentlyEffective(MstTariff entity, DateTime now)
        {
            var isAfterStart = !entity.EffectiveStartDate.HasValue || entity.EffectiveStartDate.Value <= now;
            var isBeforeEnd = !entity.EffectiveEndDate.HasValue || entity.EffectiveEndDate.Value >= now;
            return isAfterStart && isBeforeEnd;
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

        private static Guid? NormalizeNullableGuid(Guid? value)
        {
            return value.HasValue && value.Value != Guid.Empty
                ? value.Value
                : null;
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
