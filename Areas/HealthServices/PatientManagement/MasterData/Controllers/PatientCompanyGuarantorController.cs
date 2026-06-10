using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponsePatientCompanyGuarantorPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs.PatientCompanyGuarantorResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/patient-management/master-data/patient-company-guarantors")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_PATIENT_MANAGEMENT_MASTER_DATA",
        moduleName: "Health Service Patient Management Master Data",
        displayName: "Patient Company Guarantor",
        AreaName = "HealthServices",
        ControllerName = "PatientCompanyGuarantor",
        Description = "Health service patient management master data patient company guarantor",
        SortOrder = 16
    )]
    [Tags("Health Services / Patient Management / Master Data / Patient Company Guarantor")]
    public class PatientCompanyGuarantorController : ControllerBase
    {
        private const string LogCategory = "HealthServices.PatientManagement.MasterData";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public PatientCompanyGuarantorController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<PatientCompanyGuarantorFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Patient Company Guarantor",
            Description = "Melihat metadata filter patient company guarantor",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("PatientCompanyGuarantor", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new PatientCompanyGuarantorFilterMetadataResponse
            {
                DefaultFilter = new PatientCompanyGuarantorDefaultFilterResponse(),
                CustomPeriods = new List<PatientCompanyGuarantorCustomPeriodOptionResponse>
                {
                    new() { Value = "today", Label = "Hari ini" },
                    new() { Value = "last7days", Label = "7 hari terakhir" },
                    new() { Value = "thismonth", Label = "Bulan ini" },
                    new() { Value = "lastmonth", Label = "Bulan lalu" }
                },
                RelationFilters = new List<PatientCompanyGuarantorRelationFilterResponse>
                {
                    new()
                    {
                        Value = "patientId",
                        Label = "Patient",
                        Endpoint = "/api/v1/health-services/patient-management/master-data/patients/options"
                    },
                    new()
                    {
                        Value = "companyGuarantorId",
                        Label = "Company Guarantor",
                        Endpoint = "/api/v1/health-services/master-data/company-guarantors/options"
                    }
                },
                SortOptions = new List<PatientCompanyGuarantorSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "updateDateTime", Label = "Tanggal diperbarui" },
                    new() { Value = "patientName", Label = "Nama pasien" },
                    new() { Value = "medicalRecordNumber", Label = "Nomor rekam medis" },
                    new() { Value = "companyGuarantorName", Label = "Nama penjamin perusahaan" },
                    new() { Value = "employeeNumber", Label = "Nomor karyawan" },
                    new() { Value = "employeeName", Label = "Nama karyawan" },
                    new() { Value = "benefitPlanName", Label = "Nama benefit plan" },
                    new() { Value = "className", Label = "Kelas" },
                    new() { Value = "effectiveStartDate", Label = "Tanggal mulai berlaku" },
                    new() { Value = "effectiveEndDate", Label = "Tanggal akhir berlaku" },
                    new() { Value = "isPrimary", Label = "Primary" },
                    new() { Value = "isEligible", Label = "Eligible" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                ResetButtonLabel = "Reset"
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "PatientCompanyGuarantor.GetFilterMetadata",
                "Mengambil metadata filter patient company guarantor.",
                result
            );

            return Ok(ApiResponse<PatientCompanyGuarantorFilterMetadataResponse>.Ok(
                result,
                "Metadata filter patient company guarantor berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<PatientCompanyGuarantorSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Patient Company Guarantor",
            Description = "Melihat ringkasan patient company guarantor",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("PatientCompanyGuarantor", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var today = DateTime.UtcNow.Date;
            var query = BuildBaseQuery();

            var result = new PatientCompanyGuarantorSummaryResponse
            {
                TotalPatientCompanyGuarantor = await query.CountAsync(),
                ActivePatientCompanyGuarantor = await query.CountAsync(x => x.IsActive),
                InactivePatientCompanyGuarantor = await query.CountAsync(x => !x.IsActive),
                PrimaryPatientCompanyGuarantor = await query.CountAsync(x => x.IsPrimary),
                EligiblePatientCompanyGuarantor = await query.CountAsync(x => x.IsEligible),
                IneligiblePatientCompanyGuarantor = await query.CountAsync(x => !x.IsEligible),
                NeedGuaranteeLetterPatientCompanyGuarantor = await query.CountAsync(x => x.IsNeedGuaranteeLetter),
                NeedEmployeeVerificationPatientCompanyGuarantor = await query.CountAsync(x => x.IsNeedEmployeeVerification),
                AllowExcessPaymentByPatientCompanyGuarantor = await query.CountAsync(x => x.IsAllowExcessPaymentByPatient),
                EffectivePatientCompanyGuarantor = await query.CountAsync(x =>
                    (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value.Date <= today) &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= today)),
                ExpiredPatientCompanyGuarantor = await query.CountAsync(x =>
                    x.EffectiveEndDate.HasValue &&
                    x.EffectiveEndDate.Value.Date < today),
                WithAnnualLimitPatientCompanyGuarantor = await query.CountAsync(x => x.AnnualLimitAmount.HasValue),
                WithRemainingLimitPatientCompanyGuarantor = await query.CountAsync(x => x.RemainingLimitAmount.HasValue),
                WithCoPaymentPatientCompanyGuarantor = await query.CountAsync(x =>
                    x.CoPaymentPercent.HasValue ||
                    x.CoPaymentAmount.HasValue),
                WithGuaranteeDocumentPatientCompanyGuarantor = await query.CountAsync(x =>
                    x.GuaranteeDocumentPath != null &&
                    x.GuaranteeDocumentPath != string.Empty)
            };

            return Ok(ApiResponse<PatientCompanyGuarantorSummaryResponse>.Ok(
                result,
                "Ringkasan patient company guarantor berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponsePatientCompanyGuarantorPagedResult>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Patient Company Guarantor",
            Description = "Melihat data patient company guarantor",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("PatientCompanyGuarantor", "Read")]
        public async Task<IActionResult> GetPatientCompanyGuarantors(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? patientId,
            [FromQuery] Guid? companyGuarantorId,
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
            query = ApplyRelationFilter(query, patientId, companyGuarantorId);
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

            var result = new ResponsePatientCompanyGuarantorPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponsePatientCompanyGuarantorPagedResult>.Ok(
                result,
                "Data patient company guarantor berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<PatientCompanyGuarantorOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Patient Company Guarantor",
            Description = "Melihat data pilihan patient company guarantor",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("PatientCompanyGuarantor", "Read")]
        public async Task<IActionResult> GetPatientCompanyGuarantorOptions(
            [FromQuery] bool onlyActive = true,
            [FromQuery] Guid? patientId = null,
            [FromQuery] Guid? companyGuarantorId = null,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();

            query = ApplyRelationFilter(query, patientId, companyGuarantorId);
            query = ApplyStandardFilter(
                query,
                onlyActive ? true : null,
                search
            );

            var totalData = await query.CountAsync();

            var entities = await query
                .OrderByDescending(x => x.IsPrimary)
                .ThenBy(x => x.Patient != null ? x.Patient.FullName : string.Empty)
                .ThenBy(x => x.CompanyGuarantor != null ? x.CompanyGuarantor.CompanyGuarantorName : string.Empty)
                .ThenBy(x => x.EmployeeNumber)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = entities
                .Select(MapOptionResponse)
                .ToList();

            var result = new PatientCompanyGuarantorOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<PatientCompanyGuarantorOptionPagedResponse>.Ok(
                result,
                "Data pilihan patient company guarantor berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientCompanyGuarantorDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Patient Company Guarantor",
            Description = "Melihat detail patient company guarantor",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("PatientCompanyGuarantor", "Read")]
        public async Task<IActionResult> GetPatientCompanyGuarantorById(Guid id)
        {
            var entity = await BuildBaseQuery()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Patient company guarantor tidak ditemukan."
                ));
            }

            var actorNames = await GetActorNameMapAsync(new[]
            {
                entity.CreateBy,
                entity.UpdateBy
            });

            var data = MapDetailResponse(entity, actorNames);

            NormalizeActorInfo(data);

            return Ok(ApiResponse<PatientCompanyGuarantorDetailResponse>.Ok(
                data,
                "Detail patient company guarantor berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<PatientCompanyGuarantorCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(
            "Create",
            "Create Patient Company Guarantor",
            Description = "Membuat data patient company guarantor",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("PatientCompanyGuarantor", "Create")]
        public async Task<IActionResult> CreatePatientCompanyGuarantor(
            [FromBody] CreatePatientCompanyGuarantorRequest request)
        {
            var validation = await ValidateRequestAsync(
                excludeId: null,
                request: request
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data patient company guarantor tidak valid."
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

                var entity = new MstPatientCompanyGuarantor
                {
                    Id = Guid.NewGuid(),
                    PatientId = request.PatientId,
                    CompanyGuarantorId = request.CompanyGuarantorId,
                    EmployeeNumber = request.EmployeeNumber.Trim(),
                    EmployeeName = NormalizeNullableString(request.EmployeeName),
                    DepartmentName = NormalizeNullableString(request.DepartmentName),
                    PositionName = NormalizeNullableString(request.PositionName),
                    GradeLevel = NormalizeNullableString(request.GradeLevel),
                    BenefitPlanCode = NormalizeNullableString(request.BenefitPlanCode),
                    BenefitPlanName = NormalizeNullableString(request.BenefitPlanName),
                    ClassName = NormalizeNullableString(request.ClassName),
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
                    IsNeedEmployeeVerification = request.IsNeedEmployeeVerification,
                    IsAllowExcessPaymentByPatient = request.IsAllowExcessPaymentByPatient,
                    GuaranteeDocumentPath = NormalizeNullableString(request.GuaranteeDocumentPath),
                    Notes = NormalizeNullableString(request.Notes),
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                _dbContext.Set<MstPatientCompanyGuarantor>().Add(entity);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                var result = await BuildCreateResponseAsync(entity.Id);

                await _loggerService.InfoAsync(
                    LogCategory,
                    "PatientCompanyGuarantor.CreatePatientCompanyGuarantor",
                    "Membuat data patient company guarantor.",
                    result
                );

                return Ok(ApiResponse<PatientCompanyGuarantorCreateResponse>.Ok(
                    result,
                    "Patient company guarantor berhasil dibuat."
                ));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "PatientCompanyGuarantor.CreatePatientCompanyGuarantor",
                    "Gagal membuat data patient company guarantor.",
                    ex
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat membuat patient company guarantor."
                    )
                );
            }
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(
            "Update",
            "Update Patient Company Guarantor",
            Description = "Mengubah data patient company guarantor",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("PatientCompanyGuarantor", "Update")]
        public async Task<IActionResult> UpdatePatientCompanyGuarantor(
            Guid id,
            [FromBody] UpdatePatientCompanyGuarantorRequest request)
        {
            var entity = await _dbContext.Set<MstPatientCompanyGuarantor>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Patient company guarantor tidak ditemukan."
                ));
            }

            if (request.IsPrimary && !request.IsActive)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Patient company guarantor primary harus aktif."
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
                    validation.ErrorMessage ?? "Data patient company guarantor tidak valid."
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
                entity.CompanyGuarantorId = request.CompanyGuarantorId;
                entity.EmployeeNumber = request.EmployeeNumber.Trim();
                entity.EmployeeName = NormalizeNullableString(request.EmployeeName);
                entity.DepartmentName = NormalizeNullableString(request.DepartmentName);
                entity.PositionName = NormalizeNullableString(request.PositionName);
                entity.GradeLevel = NormalizeNullableString(request.GradeLevel);
                entity.BenefitPlanCode = NormalizeNullableString(request.BenefitPlanCode);
                entity.BenefitPlanName = NormalizeNullableString(request.BenefitPlanName);
                entity.ClassName = NormalizeNullableString(request.ClassName);
                entity.EffectiveStartDate = request.EffectiveStartDate;
                entity.EffectiveEndDate = request.EffectiveEndDate;
                entity.IsPrimary = request.IsActive ? request.IsPrimary : false;
                entity.IsEligible = request.IsEligible;
                entity.LastEligibilityCheckAt = request.LastEligibilityCheckAt;
                entity.LastEligibilityReferenceNumber = NormalizeNullableString(request.LastEligibilityReferenceNumber);
                entity.EligibilityNote = NormalizeNullableString(request.EligibilityNote);
                entity.AnnualLimitAmount = request.AnnualLimitAmount;
                entity.RemainingLimitAmount = request.RemainingLimitAmount;
                entity.CoPaymentPercent = request.CoPaymentPercent;
                entity.CoPaymentAmount = request.CoPaymentAmount;
                entity.IsNeedGuaranteeLetter = request.IsNeedGuaranteeLetter;
                entity.IsNeedEmployeeVerification = request.IsNeedEmployeeVerification;
                entity.IsAllowExcessPaymentByPatient = request.IsAllowExcessPaymentByPatient;
                entity.GuaranteeDocumentPath = NormalizeNullableString(request.GuaranteeDocumentPath);
                entity.Notes = NormalizeNullableString(request.Notes);
                entity.IsActive = request.IsActive;
                entity.UpdateDateTime = now;
                entity.UpdateBy = actorUserId;

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                await _loggerService.InfoAsync(
                    LogCategory,
                    "PatientCompanyGuarantor.UpdatePatientCompanyGuarantor",
                    "Mengubah data patient company guarantor.",
                    new
                    {
                        entity.Id,
                        entity.PatientId,
                        entity.CompanyGuarantorId,
                        entity.EmployeeNumber,
                        entity.IsActive
                    }
                );

                return Ok(ApiResponse<object>.Ok(
                    null,
                    "Patient company guarantor berhasil diperbarui."
                ));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "PatientCompanyGuarantor.UpdatePatientCompanyGuarantor",
                    "Gagal mengubah data patient company guarantor.",
                    ex
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat memperbarui patient company guarantor."
                    )
                );
            }
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Patient Company Guarantor Status",
            Description = "Mengubah status patient company guarantor",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("PatientCompanyGuarantor", "Update")]
        public async Task<IActionResult> UpdatePatientCompanyGuarantorStatus(
            Guid id,
            [FromBody] UpdatePatientCompanyGuarantorStatusRequest request)
        {
            var entity = await _dbContext.Set<MstPatientCompanyGuarantor>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Patient company guarantor tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsActive = request.IsActive;

            if (!request.IsActive)
            {
                entity.IsPrimary = false;
            }

            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status patient company guarantor berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Delete",
            "Delete Patient Company Guarantor",
            Description = "Menghapus data patient company guarantor",
            AccessType = AccessTypes.Delete,
            SortOrder = 4
        )]
        [AccessPermission("PatientCompanyGuarantor", "Delete")]
        public async Task<IActionResult> DeletePatientCompanyGuarantor(
            Guid id,
            [FromBody] DeletePatientCompanyGuarantorRequest? request = null)
        {
            var entity = await _dbContext.Set<MstPatientCompanyGuarantor>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Patient company guarantor tidak ditemukan."
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
                "PatientCompanyGuarantor.DeletePatientCompanyGuarantor",
                "Menghapus data patient company guarantor.",
                new
                {
                    entity.Id,
                    entity.PatientId,
                    entity.CompanyGuarantorId,
                    entity.EmployeeNumber,
                    entity.DeleteDateTime
                }
            );

            return Ok(ApiResponse<object>.Ok(
                null,
                "Patient company guarantor berhasil dihapus."
            ));
        }

        private IQueryable<MstPatientCompanyGuarantor> BuildBaseQuery()
        {
            return _dbContext.Set<MstPatientCompanyGuarantor>()
                .AsNoTracking()
                .Include(x => x.Patient)
                .Include(x => x.CompanyGuarantor)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstPatientCompanyGuarantor> ApplyDateFilter(
            IQueryable<MstPatientCompanyGuarantor> query,
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
                var today = DateTime.UtcNow.Date;

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

        private static IQueryable<MstPatientCompanyGuarantor> ApplyRelationFilter(
            IQueryable<MstPatientCompanyGuarantor> query,
            Guid? patientId,
            Guid? companyGuarantorId)
        {
            var normalizedPatientId = NormalizeNullableGuid(patientId);

            if (normalizedPatientId.HasValue)
            {
                query = query.Where(x => x.PatientId == normalizedPatientId.Value);
            }

            var normalizedCompanyGuarantorId = NormalizeNullableGuid(companyGuarantorId);

            if (normalizedCompanyGuarantorId.HasValue)
            {
                query = query.Where(x => x.CompanyGuarantorId == normalizedCompanyGuarantorId.Value);
            }

            return query;
        }

        private static IQueryable<MstPatientCompanyGuarantor> ApplyStandardFilter(
            IQueryable<MstPatientCompanyGuarantor> query,
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
                    x.EmployeeNumber.ToLower().Contains(keyword) ||
                    (x.EmployeeName != null && x.EmployeeName.ToLower().Contains(keyword)) ||
                    (x.DepartmentName != null && x.DepartmentName.ToLower().Contains(keyword)) ||
                    (x.PositionName != null && x.PositionName.ToLower().Contains(keyword)) ||
                    (x.GradeLevel != null && x.GradeLevel.ToLower().Contains(keyword)) ||
                    (x.BenefitPlanCode != null && x.BenefitPlanCode.ToLower().Contains(keyword)) ||
                    (x.BenefitPlanName != null && x.BenefitPlanName.ToLower().Contains(keyword)) ||
                    (x.ClassName != null && x.ClassName.ToLower().Contains(keyword)) ||
                    (x.LastEligibilityReferenceNumber != null && x.LastEligibilityReferenceNumber.ToLower().Contains(keyword)) ||
                    (x.EligibilityNote != null && x.EligibilityNote.ToLower().Contains(keyword)) ||
                    (x.Notes != null && x.Notes.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.PatientCode.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.MedicalRecordNumber.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.FullName.ToLower().Contains(keyword)) ||
                    (x.CompanyGuarantor != null && x.CompanyGuarantor.CompanyGuarantorCode.ToLower().Contains(keyword)) ||
                    (x.CompanyGuarantor != null && x.CompanyGuarantor.CompanyGuarantorName.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<MstPatientCompanyGuarantor> ApplySorting(
            IQueryable<MstPatientCompanyGuarantor> query,
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

                "companyguarantorname" => isDescending
                    ? query.OrderByDescending(x => x.CompanyGuarantor != null ? x.CompanyGuarantor.CompanyGuarantorName : string.Empty)
                    : query.OrderBy(x => x.CompanyGuarantor != null ? x.CompanyGuarantor.CompanyGuarantorName : string.Empty),

                "employeenumber" => isDescending
                    ? query.OrderByDescending(x => x.EmployeeNumber)
                    : query.OrderBy(x => x.EmployeeNumber),

                "employeename" => isDescending
                    ? query.OrderByDescending(x => x.EmployeeName)
                    : query.OrderBy(x => x.EmployeeName),

                "benefitplanname" => isDescending
                    ? query.OrderByDescending(x => x.BenefitPlanName)
                    : query.OrderBy(x => x.BenefitPlanName),

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
                    ? query.OrderByDescending(x => x.IsPrimary).ThenBy(x => x.EmployeeNumber)
                    : query.OrderBy(x => x.IsPrimary).ThenBy(x => x.EmployeeNumber),

                "iseligible" => isDescending
                    ? query.OrderByDescending(x => x.IsEligible).ThenBy(x => x.EmployeeNumber)
                    : query.OrderBy(x => x.IsEligible).ThenBy(x => x.EmployeeNumber),

                "isactive" => isDescending
                    ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.EmployeeNumber)
                    : query.OrderBy(x => x.IsActive).ThenBy(x => x.EmployeeNumber),

                _ => isDescending
                    ? query.OrderByDescending(x => x.CreateDateTime).ThenByDescending(x => x.EmployeeNumber)
                    : query.OrderBy(x => x.CreateDateTime).ThenBy(x => x.EmployeeNumber)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            CreatePatientCompanyGuarantorRequest request)
        {
            if (request.PatientId == Guid.Empty)
            {
                return (false, "Patient wajib dipilih.");
            }

            if (request.CompanyGuarantorId == Guid.Empty)
            {
                return (false, "Company guarantor wajib dipilih.");
            }

            if (string.IsNullOrWhiteSpace(request.EmployeeNumber))
            {
                return (false, "Nomor karyawan wajib diisi.");
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

            var companyGuarantorExists = await _dbContext.Set<MstCompanyGuarantor>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == request.CompanyGuarantorId &&
                    x.IsActive &&
                    !x.IsDelete);

            if (!companyGuarantorExists)
            {
                return (false, "Company guarantor tidak valid atau tidak aktif.");
            }

            var normalizedEmployeeNumber = request.EmployeeNumber.Trim().ToLower();

            var duplicateEmployeeQuery = _dbContext.Set<MstPatientCompanyGuarantor>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.PatientId == request.PatientId &&
                    x.CompanyGuarantorId == request.CompanyGuarantorId &&
                    x.EmployeeNumber.ToLower() == normalizedEmployeeNumber);

            if (excludeId.HasValue)
            {
                duplicateEmployeeQuery = duplicateEmployeeQuery.Where(x => x.Id != excludeId.Value);
            }

            if (await duplicateEmployeeQuery.AnyAsync())
            {
                return (false, "Nomor karyawan untuk patient dan company guarantor tersebut sudah digunakan.");
            }

            return (true, null);
        }

        private async Task UnsetOtherPrimaryAsync(
            Guid patientId,
            Guid? exceptId,
            DateTime now,
            Guid actorUserId)
        {
            var query = _dbContext.Set<MstPatientCompanyGuarantor>()
                .Where(x =>
                    x.PatientId == patientId &&
                    x.IsPrimary &&
                    !x.IsDelete);

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

        private async Task<PatientCompanyGuarantorCreateResponse> BuildCreateResponseAsync(Guid id)
        {
            var entity = await BuildBaseQuery()
                .FirstAsync(x => x.Id == id);

            return new PatientCompanyGuarantorCreateResponse
            {
                Id = entity.Id,
                PatientId = entity.PatientId,
                PatientName = entity.Patient?.FullName ?? string.Empty,
                CompanyGuarantorId = entity.CompanyGuarantorId,
                CompanyGuarantorName = entity.CompanyGuarantor?.CompanyGuarantorName ?? string.Empty,
                EmployeeNumber = entity.EmployeeNumber,
                IsPrimary = entity.IsPrimary,
                IsEligible = entity.IsEligible,
                IsActive = entity.IsActive
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

        private static PatientCompanyGuarantorResponse MapResponse(
            MstPatientCompanyGuarantor entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new PatientCompanyGuarantorResponse
            {
                Id = entity.Id,
                PatientId = entity.PatientId,
                PatientCode = entity.Patient?.PatientCode ?? string.Empty,
                MedicalRecordNumber = entity.Patient?.MedicalRecordNumber ?? string.Empty,
                PatientName = entity.Patient?.FullName ?? string.Empty,
                CompanyGuarantorId = entity.CompanyGuarantorId,
                CompanyGuarantorCode = entity.CompanyGuarantor?.CompanyGuarantorCode ?? string.Empty,
                CompanyGuarantorName = entity.CompanyGuarantor?.CompanyGuarantorName ?? string.Empty,
                CompanyGroupName = entity.CompanyGuarantor?.CompanyGroupName,
                GuarantorType = entity.CompanyGuarantor?.GuarantorType,
                BillingMethod = entity.CompanyGuarantor?.BillingMethod,
                EmployeeNumber = entity.EmployeeNumber,
                EmployeeName = entity.EmployeeName,
                DepartmentName = entity.DepartmentName,
                PositionName = entity.PositionName,
                GradeLevel = entity.GradeLevel,
                BenefitPlanCode = entity.BenefitPlanCode,
                BenefitPlanName = entity.BenefitPlanName,
                ClassName = entity.ClassName,
                EffectiveStartDate = entity.EffectiveStartDate,
                EffectiveEndDate = entity.EffectiveEndDate,
                IsPrimary = entity.IsPrimary,
                IsEligible = entity.IsEligible,
                LastEligibilityCheckAt = entity.LastEligibilityCheckAt,
                LastEligibilityReferenceNumber = entity.LastEligibilityReferenceNumber,
                AnnualLimitAmount = entity.AnnualLimitAmount,
                RemainingLimitAmount = entity.RemainingLimitAmount,
                CoPaymentPercent = entity.CoPaymentPercent,
                CoPaymentAmount = entity.CoPaymentAmount,
                IsNeedGuaranteeLetter = entity.IsNeedGuaranteeLetter,
                IsNeedEmployeeVerification = entity.IsNeedEmployeeVerification,
                IsAllowExcessPaymentByPatient = entity.IsAllowExcessPaymentByPatient,
                GuaranteeDocumentPath = entity.GuaranteeDocumentPath,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy),
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };
        }

        private static PatientCompanyGuarantorDetailResponse MapDetailResponse(
            MstPatientCompanyGuarantor entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            var response = new PatientCompanyGuarantorDetailResponse
            {
                Id = entity.Id,
                PatientId = entity.PatientId,
                PatientCode = entity.Patient?.PatientCode ?? string.Empty,
                MedicalRecordNumber = entity.Patient?.MedicalRecordNumber ?? string.Empty,
                PatientName = entity.Patient?.FullName ?? string.Empty,
                CompanyGuarantorId = entity.CompanyGuarantorId,
                CompanyGuarantorCode = entity.CompanyGuarantor?.CompanyGuarantorCode ?? string.Empty,
                CompanyGuarantorName = entity.CompanyGuarantor?.CompanyGuarantorName ?? string.Empty,
                CompanyGroupName = entity.CompanyGuarantor?.CompanyGroupName,
                GuarantorType = entity.CompanyGuarantor?.GuarantorType,
                BillingMethod = entity.CompanyGuarantor?.BillingMethod,
                EmployeeNumber = entity.EmployeeNumber,
                EmployeeName = entity.EmployeeName,
                DepartmentName = entity.DepartmentName,
                PositionName = entity.PositionName,
                GradeLevel = entity.GradeLevel,
                BenefitPlanCode = entity.BenefitPlanCode,
                BenefitPlanName = entity.BenefitPlanName,
                ClassName = entity.ClassName,
                EffectiveStartDate = entity.EffectiveStartDate,
                EffectiveEndDate = entity.EffectiveEndDate,
                IsPrimary = entity.IsPrimary,
                IsEligible = entity.IsEligible,
                LastEligibilityCheckAt = entity.LastEligibilityCheckAt,
                LastEligibilityReferenceNumber = entity.LastEligibilityReferenceNumber,
                EligibilityNote = entity.EligibilityNote,
                AnnualLimitAmount = entity.AnnualLimitAmount,
                RemainingLimitAmount = entity.RemainingLimitAmount,
                CoPaymentPercent = entity.CoPaymentPercent,
                CoPaymentAmount = entity.CoPaymentAmount,
                IsNeedGuaranteeLetter = entity.IsNeedGuaranteeLetter,
                IsNeedEmployeeVerification = entity.IsNeedEmployeeVerification,
                IsAllowExcessPaymentByPatient = entity.IsAllowExcessPaymentByPatient,
                GuaranteeDocumentPath = entity.GuaranteeDocumentPath,
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

        private static PatientCompanyGuarantorOptionResponse MapOptionResponse(
            MstPatientCompanyGuarantor entity)
        {
            return new PatientCompanyGuarantorOptionResponse
            {
                Id = entity.Id,
                PatientId = entity.PatientId,
                PatientCode = entity.Patient?.PatientCode ?? string.Empty,
                MedicalRecordNumber = entity.Patient?.MedicalRecordNumber ?? string.Empty,
                PatientName = entity.Patient?.FullName ?? string.Empty,
                CompanyGuarantorId = entity.CompanyGuarantorId,
                CompanyGuarantorCode = entity.CompanyGuarantor?.CompanyGuarantorCode ?? string.Empty,
                CompanyGuarantorName = entity.CompanyGuarantor?.CompanyGuarantorName ?? string.Empty,
                EmployeeNumber = entity.EmployeeNumber,
                EmployeeName = entity.EmployeeName,
                BenefitPlanCode = entity.BenefitPlanCode,
                BenefitPlanName = entity.BenefitPlanName,
                ClassName = entity.ClassName,
                EffectiveStartDate = entity.EffectiveStartDate,
                EffectiveEndDate = entity.EffectiveEndDate,
                IsPrimary = entity.IsPrimary,
                IsEligible = entity.IsEligible,
                IsNeedGuaranteeLetter = entity.IsNeedGuaranteeLetter,
                IsNeedEmployeeVerification = entity.IsNeedEmployeeVerification,
                IsAllowExcessPaymentByPatient = entity.IsAllowExcessPaymentByPatient,
                AnnualLimitAmount = entity.AnnualLimitAmount,
                RemainingLimitAmount = entity.RemainingLimitAmount,
                CoPaymentPercent = entity.CoPaymentPercent,
                CoPaymentAmount = entity.CoPaymentAmount
            };
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

        private static void NormalizeActorInfo(PatientCompanyGuarantorResponse data)
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
