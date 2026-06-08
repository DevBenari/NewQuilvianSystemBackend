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

        public PatientCompanyGuarantorController(ApplicationDbContext dbContext, LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<PatientCompanyGuarantorFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Company Guarantor", Description = "Melihat data patient company guarantor", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientCompanyGuarantor", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new PatientCompanyGuarantorFilterMetadataResponse
            {
                DefaultFilter = new PatientCompanyGuarantorDefaultFilterResponse(),
                SortOptions = new List<PatientCompanyGuarantorSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "patientName", Label = "Nama pasien" },
                    new() { Value = "medicalRecordNumber", Label = "No. rekam medis" },
                    new() { Value = "companyGuarantorName", Label = "Nama penjamin perusahaan" },
                    new() { Value = "employeeNumber", Label = "Nomor karyawan" },
                    new() { Value = "benefitPlanName", Label = "Nama plan" },
                    new() { Value = "className", Label = "Kelas" },
                    new() { Value = "effectiveStartDate", Label = "Tanggal mulai berlaku" },
                    new() { Value = "effectiveEndDate", Label = "Tanggal akhir berlaku" },
                    new() { Value = "lastEligibilityCheckAt", Label = "Tanggal cek eligibility" },
                    new() { Value = "isPrimary", Label = "Status utama" },
                    new() { Value = "isEligible", Label = "Status eligible" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                GradeLevels = new List<string> { "Staff", "Supervisor", "Manager", "Executive", "Director", "Other" }
            };

            await _loggerService.InfoAsync(LogCategory, "PatientCompanyGuarantor.GetFilterMetadata", "Mengambil metadata filter patient company guarantor.", result);

            return Ok(ApiResponse<PatientCompanyGuarantorFilterMetadataResponse>.Ok(result, "Metadata filter patient company guarantor berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<PatientCompanyGuarantorSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Company Guarantor", Description = "Melihat data patient company guarantor", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientCompanyGuarantor", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var today = DateTime.UtcNow.Date;
            var recentEligibilityDate = DateTime.UtcNow.AddDays(-30);

            var query = _dbContext.Set<MstPatientCompanyGuarantor>().AsNoTracking().Where(x => !x.IsDelete);

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
                ExpiredPatientCompanyGuarantor = await query.CountAsync(x => x.EffectiveEndDate.HasValue && x.EffectiveEndDate.Value.Date < today),
                WithAnnualLimitPatientCompanyGuarantor = await query.CountAsync(x => x.AnnualLimitAmount.HasValue),
                WithRemainingLimitPatientCompanyGuarantor = await query.CountAsync(x => x.RemainingLimitAmount.HasValue),
                WithCoPaymentPatientCompanyGuarantor = await query.CountAsync(x => x.CoPaymentPercent.HasValue || x.CoPaymentAmount.HasValue),
                RecentlyEligibilityCheckedPatientCompanyGuarantor = await query.CountAsync(x => x.LastEligibilityCheckAt.HasValue && x.LastEligibilityCheckAt.Value >= recentEligibilityDate),
                WithGuaranteeDocumentPatientCompanyGuarantor = await query.CountAsync(x => x.GuaranteeDocumentPath != null && x.GuaranteeDocumentPath != "")
            };

            return Ok(ApiResponse<PatientCompanyGuarantorSummaryResponse>.Ok(result, "Ringkasan patient company guarantor berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponsePatientCompanyGuarantorPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Company Guarantor", Description = "Melihat data patient company guarantor", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientCompanyGuarantor", "Read")]
        public async Task<IActionResult> GetPatientCompanyGuarantors(
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            [FromQuery] Guid? patientId,
            [FromQuery] Guid? companyGuarantorId,
            [FromQuery] string? benefitPlanName,
            [FromQuery] string? className,
            [FromQuery] string? benefitPlanCode,
            [FromQuery] string? gradeLevel,
            [FromQuery] bool? isPrimary,
            [FromQuery] bool? isEligible,
            [FromQuery] bool? isNeedGuaranteeLetter,
            [FromQuery] bool? isNeedEmployeeVerification,
            [FromQuery] bool? isAllowExcessPaymentByPatient,
            [FromQuery] DateTime? effectiveDate,
            [FromQuery] DateTime? lastEligibilityCheckFrom,
            [FromQuery] DateTime? lastEligibilityCheckTo,
            [FromQuery] decimal? minimumAnnualLimitAmount,
            [FromQuery] decimal? maximumAnnualLimitAmount,
            [FromQuery] decimal? minimumRemainingLimitAmount,
            [FromQuery] decimal? maximumRemainingLimitAmount,
            [FromQuery] string? sortBy = "createDateTime",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();

            query = ApplyFilters(query, search, isActive, patientId, companyGuarantorId, benefitPlanName, className, benefitPlanCode,
                gradeLevel, isPrimary, isEligible, isNeedGuaranteeLetter, isNeedEmployeeVerification,
                isAllowExcessPaymentByPatient, effectiveDate, lastEligibilityCheckFrom, lastEligibilityCheckTo,
                minimumAnnualLimitAmount, maximumAnnualLimitAmount, minimumRemainingLimitAmount, maximumRemainingLimitAmount);

            var totalData = await query.CountAsync();
            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = entities.Select(ToResponse).ToList();

            var result = new ResponsePatientCompanyGuarantorPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponsePatientCompanyGuarantorPagedResult>.Ok(result, "Data patient company guarantor berhasil diambil."));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<PatientCompanyGuarantorOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Company Guarantor", Description = "Melihat data patient company guarantor", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientCompanyGuarantor", "Read")]
        public async Task<IActionResult> GetPatientCompanyGuarantorOptions(
            [FromQuery] Guid? patientId,
            [FromQuery] Guid? companyGuarantorId,
            [FromQuery] bool? isPrimary,
            [FromQuery] bool? isEligible,
            [FromQuery] DateTime? effectiveDate,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null)
        {
            var query = BuildBaseQuery();

            if (onlyActive)
                query = query.Where(x => x.IsActive);
            if (patientId.HasValue && patientId.Value != Guid.Empty)
                query = query.Where(x => x.PatientId == patientId.Value);
            if (companyGuarantorId.HasValue && companyGuarantorId.Value != Guid.Empty)
                query = query.Where(x => x.CompanyGuarantorId == companyGuarantorId.Value);
            if (isPrimary.HasValue)
                query = query.Where(x => x.IsPrimary == isPrimary.Value);
            if (isEligible.HasValue)
                query = query.Where(x => x.IsEligible == isEligible.Value);
            if (effectiveDate.HasValue)
            {
                var selectedDate = effectiveDate.Value.Date;
                query = query.Where(x =>
                    (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value.Date <= selectedDate) &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= selectedDate));
            }
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.EmployeeNumber.ToLower().Contains(keyword) ||
                    (x.EmployeeName != null && x.EmployeeName.ToLower().Contains(keyword)) ||
                    (x.DepartmentName != null && x.DepartmentName.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.FullName.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.MedicalRecordNumber.ToLower().Contains(keyword)) ||
                    (x.CompanyGuarantor != null && x.CompanyGuarantor.CompanyGuarantorName.ToLower().Contains(keyword)));
            }

            var data = await query
                .OrderByDescending(x => x.IsPrimary)
                .ThenBy(x => x.Patient != null ? x.Patient.FullName : string.Empty)
                .ThenBy(x => x.CompanyGuarantor != null ? x.CompanyGuarantor.CompanyGuarantorName : string.Empty)
                .Select(x => new PatientCompanyGuarantorOptionResponse
                {
                    Id = x.Id,
                    PatientId = x.PatientId,
                    PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                    MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty,
                    CompanyGuarantorId = x.CompanyGuarantorId,
                    CompanyGuarantorName = x.CompanyGuarantor != null ? x.CompanyGuarantor.CompanyGuarantorName : string.Empty,
                    EmployeeNumber = x.EmployeeNumber,
                    EmployeeName = x.EmployeeName,
                    DepartmentName = x.DepartmentName,
                    PositionName = x.PositionName,
                    GradeLevel = x.GradeLevel,
                    BenefitPlanName = x.BenefitPlanName,
                    ClassName = x.ClassName,
                    BenefitPlanCode = x.BenefitPlanCode,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    IsPrimary = x.IsPrimary,
                    IsEligible = x.IsEligible,
                    IsNeedGuaranteeLetter = x.IsNeedGuaranteeLetter,
                    IsNeedEmployeeVerification = x.IsNeedEmployeeVerification,
                    IsAllowExcessPaymentByPatient = x.IsAllowExcessPaymentByPatient,
                    AnnualLimitAmount = x.AnnualLimitAmount,
                    RemainingLimitAmount = x.RemainingLimitAmount,
                    CoPaymentPercent = x.CoPaymentPercent,
                    CoPaymentAmount = x.CoPaymentAmount
                })
                .ToListAsync();

            return Ok(ApiResponse<List<PatientCompanyGuarantorOptionResponse>>.Ok(data, "Data pilihan patient company guarantor berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientCompanyGuarantorDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Patient Company Guarantor", Description = "Melihat data patient company guarantor", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientCompanyGuarantor", "Read")]
        public async Task<IActionResult> GetPatientCompanyGuarantorById(Guid id)
        {
            var data = await BuildBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => new PatientCompanyGuarantorDetailResponse
                {
                    Id = x.Id,
                    PatientId = x.PatientId,
                    PatientCode = x.Patient != null ? x.Patient.PatientCode : string.Empty,
                    MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty,
                    PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                    CompanyGuarantorId = x.CompanyGuarantorId,
                    CompanyGuarantorCode = x.CompanyGuarantor != null ? x.CompanyGuarantor.CompanyGuarantorCode : string.Empty,
                    CompanyGuarantorName = x.CompanyGuarantor != null ? x.CompanyGuarantor.CompanyGuarantorName : string.Empty,
                    CompanyGroupName = x.CompanyGuarantor != null ? x.CompanyGuarantor.CompanyGroupName : null,
                    GuarantorType = x.CompanyGuarantor != null ? x.CompanyGuarantor.GuarantorType : null,
                    BillingMethod = x.CompanyGuarantor != null ? x.CompanyGuarantor.BillingMethod : null,
                    EmployeeNumber = x.EmployeeNumber,
                    EmployeeName = x.EmployeeName,
                    DepartmentName = x.DepartmentName,
                    PositionName = x.PositionName,
                    GradeLevel = x.GradeLevel,
                    BenefitPlanName = x.BenefitPlanName,
                    ClassName = x.ClassName,
                    BenefitPlanCode = x.BenefitPlanCode,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    IsPrimary = x.IsPrimary,
                    IsEligible = x.IsEligible,
                    LastEligibilityCheckAt = x.LastEligibilityCheckAt,
                    LastEligibilityReferenceNumber = x.LastEligibilityReferenceNumber,
                    EligibilityNote = x.EligibilityNote,
                    AnnualLimitAmount = x.AnnualLimitAmount,
                    RemainingLimitAmount = x.RemainingLimitAmount,
                    CoPaymentPercent = x.CoPaymentPercent,
                    CoPaymentAmount = x.CoPaymentAmount,
                    IsNeedGuaranteeLetter = x.IsNeedGuaranteeLetter,
                    IsNeedEmployeeVerification = x.IsNeedEmployeeVerification,
                    IsAllowExcessPaymentByPatient = x.IsAllowExcessPaymentByPatient,
                    GuaranteeDocumentPath = x.GuaranteeDocumentPath,
                    Notes = x.Notes,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .FirstOrDefaultAsync();

            if (data == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Patient company guarantor tidak ditemukan."));

            return Ok(ApiResponse<PatientCompanyGuarantorDetailResponse>.Ok(data, "Detail patient company guarantor berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<PatientCompanyGuarantorCreateResponse>), StatusCodes.Status200OK)]
        [AccessAction("Create", "Create Patient Company Guarantor", Description = "Membuat data patient company guarantor", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("PatientCompanyGuarantor", "Create")]
        public async Task<IActionResult> CreatePatientCompanyGuarantor([FromBody] CreatePatientCompanyGuarantorRequest request)
        {
            var validation = await ValidateRequestAsync(null, request.PatientId, request.CompanyGuarantorId, request.EmployeeNumber,
                request.GradeLevel, request.EffectiveStartDate, request.EffectiveEndDate, request.AnnualLimitAmount,
                request.RemainingLimitAmount, request.CoPaymentPercent, request.CoPaymentAmount);

            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage ?? "Data patient company guarantor tidak valid."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            if (request.IsPrimary)
                await ClearPrimaryCompanyGuarantorAsync(request.PatientId, null, now, actorUserId);

            var entity = new MstPatientCompanyGuarantor
            {
                Id = Guid.NewGuid(),
                PatientId = request.PatientId,
                CompanyGuarantorId = request.CompanyGuarantorId,
                EmployeeNumber = request.EmployeeNumber.Trim(),
                EmployeeName = NormalizeNullableText(request.EmployeeName),
                DepartmentName = NormalizeNullableText(request.DepartmentName),
                BenefitPlanName = NormalizeNullableText(request.BenefitPlanName),
                ClassName = NormalizeNullableText(request.ClassName),
                BenefitPlanCode = NormalizeNullableText(request.BenefitPlanCode),
                PositionName = NormalizeNullableText(request.PositionName),
                GradeLevel = NormalizeNullableText(request.GradeLevel),
                EffectiveStartDate = request.EffectiveStartDate,
                EffectiveEndDate = request.EffectiveEndDate,
                IsPrimary = request.IsPrimary,
                IsEligible = request.IsEligible,
                LastEligibilityCheckAt = request.LastEligibilityCheckAt,
                LastEligibilityReferenceNumber = NormalizeNullableText(request.LastEligibilityReferenceNumber),
                EligibilityNote = NormalizeNullableText(request.EligibilityNote),
                AnnualLimitAmount = request.AnnualLimitAmount,
                RemainingLimitAmount = request.RemainingLimitAmount,
                CoPaymentPercent = request.CoPaymentPercent,
                CoPaymentAmount = request.CoPaymentAmount,
                IsNeedGuaranteeLetter = request.IsNeedGuaranteeLetter,
                IsNeedEmployeeVerification = request.IsNeedEmployeeVerification,
                IsAllowExcessPaymentByPatient = request.IsAllowExcessPaymentByPatient,
                GuaranteeDocumentPath = NormalizeNullableText(request.GuaranteeDocumentPath),
                Notes = NormalizeNullableText(request.Notes),
                IsActive = request.IsActive,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.Set<MstPatientCompanyGuarantor>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var response = await BuildCreateUpdateResponse(entity.Id, true);
            return Ok(ApiResponse<PatientCompanyGuarantorCreateResponse>.Ok(response.Create!, "Patient company guarantor berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientCompanyGuarantorUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Patient Company Guarantor", Description = "Mengubah data patient company guarantor", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("PatientCompanyGuarantor", "Update")]
        public async Task<IActionResult> UpdatePatientCompanyGuarantor(Guid id, [FromBody] UpdatePatientCompanyGuarantorRequest request)
        {
            var entity = await _dbContext.Set<MstPatientCompanyGuarantor>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Patient company guarantor tidak ditemukan."));

            var validation = await ValidateRequestAsync(id, request.PatientId, request.CompanyGuarantorId, request.EmployeeNumber,
                request.GradeLevel, request.EffectiveStartDate, request.EffectiveEndDate, request.AnnualLimitAmount,
                request.RemainingLimitAmount, request.CoPaymentPercent, request.CoPaymentAmount);

            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage ?? "Data patient company guarantor tidak valid."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            if (request.IsPrimary)
                await ClearPrimaryCompanyGuarantorAsync(request.PatientId, id, now, actorUserId);

            entity.PatientId = request.PatientId;
            entity.CompanyGuarantorId = request.CompanyGuarantorId;
            entity.EmployeeNumber = request.EmployeeNumber.Trim();
            entity.EmployeeName = NormalizeNullableText(request.EmployeeName);
            entity.DepartmentName = NormalizeNullableText(request.DepartmentName);
            entity.BenefitPlanName = NormalizeNullableText(request.BenefitPlanName);
            entity.ClassName = NormalizeNullableText(request.ClassName);
            entity.BenefitPlanCode = NormalizeNullableText(request.BenefitPlanCode);
            entity.PositionName = NormalizeNullableText(request.PositionName);
            entity.GradeLevel = NormalizeNullableText(request.GradeLevel);
            entity.EffectiveStartDate = request.EffectiveStartDate;
            entity.EffectiveEndDate = request.EffectiveEndDate;
            entity.IsPrimary = request.IsPrimary;
            entity.IsEligible = request.IsEligible;
            entity.LastEligibilityCheckAt = request.LastEligibilityCheckAt;
            entity.LastEligibilityReferenceNumber = NormalizeNullableText(request.LastEligibilityReferenceNumber);
            entity.EligibilityNote = NormalizeNullableText(request.EligibilityNote);
            entity.AnnualLimitAmount = request.AnnualLimitAmount;
            entity.RemainingLimitAmount = request.RemainingLimitAmount;
            entity.CoPaymentPercent = request.CoPaymentPercent;
            entity.CoPaymentAmount = request.CoPaymentAmount;
            entity.IsNeedGuaranteeLetter = request.IsNeedGuaranteeLetter;
            entity.IsNeedEmployeeVerification = request.IsNeedEmployeeVerification;
            entity.IsAllowExcessPaymentByPatient = request.IsAllowExcessPaymentByPatient;
            entity.GuaranteeDocumentPath = NormalizeNullableText(request.GuaranteeDocumentPath);
            entity.Notes = NormalizeNullableText(request.Notes);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var response = await BuildCreateUpdateResponse(entity.Id, false);
            return Ok(ApiResponse<PatientCompanyGuarantorUpdateResponse>.Ok(response.Update!, "Patient company guarantor berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/primary")]
        [ProducesResponseType(typeof(ApiResponse<PatientCompanyGuarantorPrimaryStatusResponse>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Update Patient Company Guarantor", Description = "Mengubah primary patient company guarantor", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("PatientCompanyGuarantor", "Update")]
        public async Task<IActionResult> SetPrimaryStatus(Guid id, [FromQuery] bool isPrimary = true)
        {
            var entity = await _dbContext.Set<MstPatientCompanyGuarantor>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Patient company guarantor tidak ditemukan."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            if (isPrimary)
                await ClearPrimaryCompanyGuarantorAsync(entity.PatientId, id, now, actorUserId);

            entity.IsPrimary = isPrimary;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync();

            var response = await BuildPrimaryStatusResponse(entity.Id);
            return Ok(ApiResponse<PatientCompanyGuarantorPrimaryStatusResponse>.Ok(response, "Status primary patient company guarantor berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/eligibility")]
        [ProducesResponseType(typeof(ApiResponse<PatientCompanyGuarantorEligibilityResponse>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Update Patient Company Guarantor", Description = "Mengubah eligibility patient company guarantor", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("PatientCompanyGuarantor", "Update")]
        public async Task<IActionResult> UpdateEligibility(Guid id, [FromBody] PatientCompanyGuarantorEligibilityRequest request)
        {
            var entity = await _dbContext.Set<MstPatientCompanyGuarantor>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Patient company guarantor tidak ditemukan."));

            if (request.RemainingLimitAmount.HasValue && request.RemainingLimitAmount.Value < 0)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Sisa limit tidak boleh kurang dari 0."));

            entity.IsEligible = request.IsEligible;
            entity.LastEligibilityCheckAt = DateTime.UtcNow;
            entity.LastEligibilityReferenceNumber = NormalizeNullableText(request.LastEligibilityReferenceNumber);
            entity.EligibilityNote = NormalizeNullableText(request.EligibilityNote);
            entity.RemainingLimitAmount = request.RemainingLimitAmount;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();
            await _dbContext.SaveChangesAsync();

            var response = await BuildEligibilityResponse(entity.Id);
            return Ok(ApiResponse<PatientCompanyGuarantorEligibilityResponse>.Ok(response, "Eligibility patient company guarantor berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<PatientCompanyGuarantorStatusResponse>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Update Patient Company Guarantor", Description = "Mengubah status patient company guarantor", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("PatientCompanyGuarantor", "Update")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromQuery] bool isActive)
        {
            var entity = await _dbContext.Set<MstPatientCompanyGuarantor>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Patient company guarantor tidak ditemukan."));

            entity.IsActive = isActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();
            await _dbContext.SaveChangesAsync();

            var response = await BuildStatusResponse(entity.Id);
            return Ok(ApiResponse<PatientCompanyGuarantorStatusResponse>.Ok(response, "Status patient company guarantor berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientCompanyGuarantorDeleteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Patient Company Guarantor", Description = "Menghapus data patient company guarantor", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("PatientCompanyGuarantor", "Delete")]
        public async Task<IActionResult> DeletePatientCompanyGuarantor(Guid id)
        {
            var entity = await BuildBaseQuery().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Patient company guarantor tidak ditemukan."));

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.IsPrimary = false;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = GetCurrentUserId();
            await _dbContext.SaveChangesAsync();

            var response = new PatientCompanyGuarantorDeleteResponse
            {
                Id = entity.Id,
                PatientId = entity.PatientId,
                PatientName = entity.Patient != null ? entity.Patient.FullName : string.Empty,
                CompanyGuarantorId = entity.CompanyGuarantorId,
                CompanyGuarantorName = entity.CompanyGuarantor != null ? entity.CompanyGuarantor.CompanyGuarantorName : string.Empty,
                EmployeeNumber = entity.EmployeeNumber,
                IsDelete = entity.IsDelete
            };

            return Ok(ApiResponse<PatientCompanyGuarantorDeleteResponse>.Ok(response, "Patient company guarantor berhasil dihapus."));
        }

        private IQueryable<MstPatientCompanyGuarantor> BuildBaseQuery()
        {
            return _dbContext.Set<MstPatientCompanyGuarantor>()
                .Include(x => x.Patient)
                .Include(x => x.CompanyGuarantor)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstPatientCompanyGuarantor> ApplyFilters(
            IQueryable<MstPatientCompanyGuarantor> query,
            string? search,
            bool? isActive,
            Guid? patientId,
            Guid? companyGuarantorId,
            string? benefitPlanName,
            string? className,
            string? benefitPlanCode,
            string? gradeLevel,
            bool? isPrimary,
            bool? isEligible,
            bool? isNeedGuaranteeLetter,
            bool? isNeedEmployeeVerification,
            bool? isAllowExcessPaymentByPatient,
            DateTime? effectiveDate,
            DateTime? lastEligibilityCheckFrom,
            DateTime? lastEligibilityCheckTo,
            decimal? minimumAnnualLimitAmount,
            decimal? maximumAnnualLimitAmount,
            decimal? minimumRemainingLimitAmount,
            decimal? maximumRemainingLimitAmount)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.EmployeeNumber.ToLower().Contains(keyword) ||
                    (x.EmployeeName != null && x.EmployeeName.ToLower().Contains(keyword)) ||
                    (x.DepartmentName != null && x.DepartmentName.ToLower().Contains(keyword)) ||
                    (x.BenefitPlanName != null && x.BenefitPlanName.ToLower().Contains(keyword)) ||
                    (x.ClassName != null && x.ClassName.ToLower().Contains(keyword)) ||
                    (x.BenefitPlanCode != null && x.BenefitPlanCode.ToLower().Contains(keyword)) ||
                    (x.PositionName != null && x.PositionName.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.PatientCode.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.MedicalRecordNumber.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.FullName.ToLower().Contains(keyword)) ||
                    (x.CompanyGuarantor != null && x.CompanyGuarantor.CompanyGuarantorCode.ToLower().Contains(keyword)) ||
                    (x.CompanyGuarantor != null && x.CompanyGuarantor.CompanyGuarantorName.ToLower().Contains(keyword)));
            }

            if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);
            if (patientId.HasValue && patientId.Value != Guid.Empty) query = query.Where(x => x.PatientId == patientId.Value);
            if (companyGuarantorId.HasValue && companyGuarantorId.Value != Guid.Empty) query = query.Where(x => x.CompanyGuarantorId == companyGuarantorId.Value);
            if (!string.IsNullOrWhiteSpace(benefitPlanName)) query = query.Where(x => x.BenefitPlanName != null && x.BenefitPlanName.ToLower().Contains(benefitPlanName.Trim().ToLower()));
            if (!string.IsNullOrWhiteSpace(className)) query = query.Where(x => x.ClassName != null && x.ClassName.ToLower().Contains(className.Trim().ToLower()));
            if (!string.IsNullOrWhiteSpace(benefitPlanCode)) query = query.Where(x => x.BenefitPlanCode != null && x.BenefitPlanCode.ToLower().Contains(benefitPlanCode.Trim().ToLower()));
            if (!string.IsNullOrWhiteSpace(gradeLevel)) query = query.Where(x => x.GradeLevel != null && x.GradeLevel == gradeLevel.Trim());
            if (isPrimary.HasValue) query = query.Where(x => x.IsPrimary == isPrimary.Value);
            if (isEligible.HasValue) query = query.Where(x => x.IsEligible == isEligible.Value);
            if (isNeedGuaranteeLetter.HasValue) query = query.Where(x => x.IsNeedGuaranteeLetter == isNeedGuaranteeLetter.Value);
            if (isNeedEmployeeVerification.HasValue) query = query.Where(x => x.IsNeedEmployeeVerification == isNeedEmployeeVerification.Value);
            if (isAllowExcessPaymentByPatient.HasValue) query = query.Where(x => x.IsAllowExcessPaymentByPatient == isAllowExcessPaymentByPatient.Value);

            if (effectiveDate.HasValue)
            {
                var selectedDate = effectiveDate.Value.Date;
                query = query.Where(x =>
                    (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value.Date <= selectedDate) &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= selectedDate));
            }

            if (lastEligibilityCheckFrom.HasValue) query = query.Where(x => x.LastEligibilityCheckAt.HasValue && x.LastEligibilityCheckAt.Value >= lastEligibilityCheckFrom.Value);
            if (lastEligibilityCheckTo.HasValue) query = query.Where(x => x.LastEligibilityCheckAt.HasValue && x.LastEligibilityCheckAt.Value <= lastEligibilityCheckTo.Value);
            if (minimumAnnualLimitAmount.HasValue) query = query.Where(x => x.AnnualLimitAmount.HasValue && x.AnnualLimitAmount.Value >= minimumAnnualLimitAmount.Value);
            if (maximumAnnualLimitAmount.HasValue) query = query.Where(x => x.AnnualLimitAmount.HasValue && x.AnnualLimitAmount.Value <= maximumAnnualLimitAmount.Value);
            if (minimumRemainingLimitAmount.HasValue) query = query.Where(x => x.RemainingLimitAmount.HasValue && x.RemainingLimitAmount.Value >= minimumRemainingLimitAmount.Value);
            if (maximumRemainingLimitAmount.HasValue) query = query.Where(x => x.RemainingLimitAmount.HasValue && x.RemainingLimitAmount.Value <= maximumRemainingLimitAmount.Value);

            return query;
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            Guid patientId,
            Guid companyGuarantorId,
            string employeeNumber,
            string? gradeLevel,
            DateTime? effectiveStartDate,
            DateTime? effectiveEndDate,
            decimal? annualLimitAmount,
            decimal? remainingLimitAmount,
            decimal? coPaymentPercent,
            decimal? coPaymentAmount)
        {
            if (patientId == Guid.Empty) return (false, "Pasien wajib dipilih.");
            if (companyGuarantorId == Guid.Empty) return (false, "Company guarantor wajib dipilih.");
            if (string.IsNullOrWhiteSpace(employeeNumber)) return (false, "Nomor karyawan wajib diisi.");

            if (effectiveStartDate.HasValue && effectiveEndDate.HasValue && effectiveEndDate.Value.Date < effectiveStartDate.Value.Date)
                return (false, "Tanggal akhir berlaku tidak boleh lebih kecil dari tanggal mulai berlaku.");

            if (annualLimitAmount.HasValue && annualLimitAmount.Value < 0) return (false, "Limit tahunan tidak boleh kurang dari 0.");
            if (remainingLimitAmount.HasValue && remainingLimitAmount.Value < 0) return (false, "Sisa limit tidak boleh kurang dari 0.");
            if (coPaymentPercent.HasValue && (coPaymentPercent.Value < 0 || coPaymentPercent.Value > 100)) return (false, "Co-payment percent harus di antara 0 sampai 100.");
            if (coPaymentAmount.HasValue && coPaymentAmount.Value < 0) return (false, "Co-payment amount tidak boleh kurang dari 0.");

            var patientExists = await _dbContext.Set<MstPatient>().AnyAsync(x => x.Id == patientId && !x.IsDelete);
            if (!patientExists) return (false, "Pasien tidak ditemukan.");

            var companyGuarantorExists = await _dbContext.Set<MstCompanyGuarantor>().AnyAsync(x => x.Id == companyGuarantorId && !x.IsDelete);
            if (!companyGuarantorExists) return (false, "Company guarantor tidak ditemukan.");

            var normalizedEmployeeNumber = employeeNumber.Trim().ToLower();
            var duplicateEmployee = await _dbContext.Set<MstPatientCompanyGuarantor>().AnyAsync(x =>
                !x.IsDelete &&
                x.PatientId == patientId &&
                x.CompanyGuarantorId == companyGuarantorId &&
                x.EmployeeNumber.ToLower() == normalizedEmployeeNumber &&
                (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateEmployee)
                return (false, "Nomor karyawan untuk pasien dan company guarantor tersebut sudah digunakan.");

            return (true, null);
        }

        private async Task ClearPrimaryCompanyGuarantorAsync(Guid patientId, Guid? excludeId, DateTime now, Guid actorUserId)
        {
            var existingPrimaries = await _dbContext.Set<MstPatientCompanyGuarantor>()
                .Where(x => !x.IsDelete && x.PatientId == patientId && x.IsPrimary && (!excludeId.HasValue || x.Id != excludeId.Value))
                .ToListAsync();

            foreach (var item in existingPrimaries)
            {
                item.IsPrimary = false;
                item.UpdateDateTime = now;
                item.UpdateBy = actorUserId;
            }
        }

        private async Task<(PatientCompanyGuarantorCreateResponse? Create, PatientCompanyGuarantorUpdateResponse? Update)> BuildCreateUpdateResponse(Guid id, bool isCreate)
        {
            var data = await BuildBaseQuery().AsNoTracking().FirstAsync(x => x.Id == id);

            if (isCreate)
            {
                return (new PatientCompanyGuarantorCreateResponse
                {
                    Id = data.Id,
                    PatientId = data.PatientId,
                    PatientName = data.Patient != null ? data.Patient.FullName : string.Empty,
                    CompanyGuarantorId = data.CompanyGuarantorId,
                    CompanyGuarantorName = data.CompanyGuarantor != null ? data.CompanyGuarantor.CompanyGuarantorName : string.Empty,
                    EmployeeNumber = data.EmployeeNumber,
                    IsPrimary = data.IsPrimary,
                    IsEligible = data.IsEligible,
                    IsActive = data.IsActive
                }, null);
            }

            return (null, new PatientCompanyGuarantorUpdateResponse
            {
                Id = data.Id,
                PatientId = data.PatientId,
                PatientName = data.Patient != null ? data.Patient.FullName : string.Empty,
                CompanyGuarantorId = data.CompanyGuarantorId,
                CompanyGuarantorName = data.CompanyGuarantor != null ? data.CompanyGuarantor.CompanyGuarantorName : string.Empty,
                EmployeeNumber = data.EmployeeNumber,
                IsPrimary = data.IsPrimary,
                IsEligible = data.IsEligible,
                IsActive = data.IsActive
            });
        }

        private async Task<PatientCompanyGuarantorStatusResponse> BuildStatusResponse(Guid id)
        {
            return await BuildBaseQuery().AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new PatientCompanyGuarantorStatusResponse
                {
                    Id = x.Id,
                    PatientId = x.PatientId,
                    PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                    CompanyGuarantorId = x.CompanyGuarantorId,
                    CompanyGuarantorName = x.CompanyGuarantor != null ? x.CompanyGuarantor.CompanyGuarantorName : string.Empty,
                    EmployeeNumber = x.EmployeeNumber,
                    IsActive = x.IsActive
                })
                .FirstAsync();
        }

        private async Task<PatientCompanyGuarantorPrimaryStatusResponse> BuildPrimaryStatusResponse(Guid id)
        {
            return await BuildBaseQuery().AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new PatientCompanyGuarantorPrimaryStatusResponse
                {
                    Id = x.Id,
                    PatientId = x.PatientId,
                    PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                    CompanyGuarantorId = x.CompanyGuarantorId,
                    CompanyGuarantorName = x.CompanyGuarantor != null ? x.CompanyGuarantor.CompanyGuarantorName : string.Empty,
                    EmployeeNumber = x.EmployeeNumber,
                    IsPrimary = x.IsPrimary
                })
                .FirstAsync();
        }

        private async Task<PatientCompanyGuarantorEligibilityResponse> BuildEligibilityResponse(Guid id)
        {
            return await BuildBaseQuery().AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new PatientCompanyGuarantorEligibilityResponse
                {
                    Id = x.Id,
                    PatientId = x.PatientId,
                    PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                    CompanyGuarantorId = x.CompanyGuarantorId,
                    CompanyGuarantorName = x.CompanyGuarantor != null ? x.CompanyGuarantor.CompanyGuarantorName : string.Empty,
                    EmployeeNumber = x.EmployeeNumber,
                    IsEligible = x.IsEligible,
                    LastEligibilityCheckAt = x.LastEligibilityCheckAt,
                    LastEligibilityReferenceNumber = x.LastEligibilityReferenceNumber,
                    EligibilityNote = x.EligibilityNote,
                    RemainingLimitAmount = x.RemainingLimitAmount
                })
                .FirstAsync();
        }

        private static PatientCompanyGuarantorResponse ToResponse(MstPatientCompanyGuarantor x)
        {
            return new PatientCompanyGuarantorResponse
            {
                Id = x.Id,
                PatientId = x.PatientId,
                PatientCode = x.Patient != null ? x.Patient.PatientCode : string.Empty,
                MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty,
                PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                CompanyGuarantorId = x.CompanyGuarantorId,
                CompanyGuarantorCode = x.CompanyGuarantor != null ? x.CompanyGuarantor.CompanyGuarantorCode : string.Empty,
                CompanyGuarantorName = x.CompanyGuarantor != null ? x.CompanyGuarantor.CompanyGuarantorName : string.Empty,
                CompanyGroupName = x.CompanyGuarantor != null ? x.CompanyGuarantor.CompanyGroupName : null,
                GuarantorType = x.CompanyGuarantor != null ? x.CompanyGuarantor.GuarantorType : null,
                BillingMethod = x.CompanyGuarantor != null ? x.CompanyGuarantor.BillingMethod : null,
                EmployeeNumber = x.EmployeeNumber,
                EmployeeName = x.EmployeeName,
                DepartmentName = x.DepartmentName,
                BenefitPlanName = x.BenefitPlanName,
                ClassName = x.ClassName,
                BenefitPlanCode = x.BenefitPlanCode,
                PositionName = x.PositionName,
                GradeLevel = x.GradeLevel,
                EffectiveStartDate = x.EffectiveStartDate,
                EffectiveEndDate = x.EffectiveEndDate,
                IsPrimary = x.IsPrimary,
                IsEligible = x.IsEligible,
                LastEligibilityCheckAt = x.LastEligibilityCheckAt,
                LastEligibilityReferenceNumber = x.LastEligibilityReferenceNumber,
                AnnualLimitAmount = x.AnnualLimitAmount,
                RemainingLimitAmount = x.RemainingLimitAmount,
                CoPaymentPercent = x.CoPaymentPercent,
                CoPaymentAmount = x.CoPaymentAmount,
                IsNeedGuaranteeLetter = x.IsNeedGuaranteeLetter,
                IsNeedEmployeeVerification = x.IsNeedEmployeeVerification,
                IsAllowExcessPaymentByPatient = x.IsAllowExcessPaymentByPatient,
                GuaranteeDocumentPath = x.GuaranteeDocumentPath,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime
            };
        }

        private static IQueryable<MstPatientCompanyGuarantor> ApplySorting(IQueryable<MstPatientCompanyGuarantor> query, string? sortBy, string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "createDateTime").ToLowerInvariant() switch
            {
                "patientname" => isDesc ? query.OrderByDescending(x => x.Patient != null ? x.Patient.FullName : string.Empty) : query.OrderBy(x => x.Patient != null ? x.Patient.FullName : string.Empty),
                "medicalrecordnumber" => isDesc ? query.OrderByDescending(x => x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty) : query.OrderBy(x => x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty),
                "companyguarantorname" => isDesc ? query.OrderByDescending(x => x.CompanyGuarantor != null ? x.CompanyGuarantor.CompanyGuarantorName : string.Empty) : query.OrderBy(x => x.CompanyGuarantor != null ? x.CompanyGuarantor.CompanyGuarantorName : string.Empty),
                "employeenumber" => isDesc ? query.OrderByDescending(x => x.EmployeeNumber) : query.OrderBy(x => x.EmployeeNumber),
                "benefitplanname" => isDesc ? query.OrderByDescending(x => x.BenefitPlanName) : query.OrderBy(x => x.BenefitPlanName),
                "classname" => isDesc ? query.OrderByDescending(x => x.ClassName) : query.OrderBy(x => x.ClassName),
                "effectivestartdate" => isDesc ? query.OrderByDescending(x => x.EffectiveStartDate) : query.OrderBy(x => x.EffectiveStartDate),
                "effectiveenddate" => isDesc ? query.OrderByDescending(x => x.EffectiveEndDate) : query.OrderBy(x => x.EffectiveEndDate),
                "lasteligibilitycheckat" => isDesc ? query.OrderByDescending(x => x.LastEligibilityCheckAt) : query.OrderBy(x => x.LastEligibilityCheckAt),
                "isprimary" => isDesc ? query.OrderByDescending(x => x.IsPrimary) : query.OrderBy(x => x.IsPrimary),
                "iseligible" => isDesc ? query.OrderByDescending(x => x.IsEligible) : query.OrderBy(x => x.IsEligible),
                "isactive" => isDesc ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
                _ => isDesc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime)
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
            return Guid.TryParse(userIdText, out var userId) ? userId : Guid.Empty;
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
