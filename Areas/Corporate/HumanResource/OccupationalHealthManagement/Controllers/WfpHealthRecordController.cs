using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/health-records")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_OCCUPATIONAL_HEALTH",
        moduleName: "Human Resource Occupational Health",
        displayName: "Workforce Health Record",
        AreaName = "Corporate",
        ControllerName = "WorkforceHealthRecord",
        Description = "Corporate human resource workforce occupational health record",
        SortOrder = 1
    )]
    [Tags("Corporate / Human Resource / Occupational Health Management / Health Record")]
    public class WfpHealthRecordController : ControllerBase
    {
        private static readonly HashSet<string> AllowedRecordTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "General",
            "MedicalExamination",
            "FitnessToWork",
            "Vaccination",
            "WorkRestriction",
            "OccupationalExposure",
            "Injury",
            "ReturnToWork"
        };

        private static readonly HashSet<string> AllowedAccessClassifications = new(StringComparer.OrdinalIgnoreCase)
        {
            "Restricted",
            "Confidential",
            "Administrative"
        };

        private const string LogCategory = "Corporate.HumanResource.OccupationalHealth";
        private const string CodePrefix = "HLT-RSMMC-";
        private const int CodeNumberLength = 5;

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public WfpHealthRecordController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<WfpHealthRecordFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Health Record", Description = "Melihat metadata filter rekam kesehatan workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceHealthRecord", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = new WfpHealthRecordFilterMetadataResponse
            {
                DefaultFilter = new WfpHealthRecordDefaultFilterResponse(),
                CustomPeriods = BuildPeriodOptions(),
                RecordTypeOptions = AllowedRecordTypes
                    .OrderBy(x => x)
                    .Select(x => new WfpHealthRecordStringOptionResponse
                    {
                        Value = x,
                        Label = BuildLabel(x)
                    })
                    .ToList(),
                AccessClassificationOptions = AllowedAccessClassifications
                    .OrderBy(x => x)
                    .Select(x => new WfpHealthRecordStringOptionResponse
                    {
                        Value = x,
                        Label = BuildLabel(x)
                    })
                    .ToList(),
                SortOptions = new List<WfpHealthRecordSortOptionResponse>
                {
                    new() { Value = "recordDate", Label = "Tanggal catatan" },
                    new() { Value = "recordCode", Label = "Kode catatan" },
                    new() { Value = "recordType", Label = "Jenis catatan" },
                    new() { Value = "administrativeResultStatus", Label = "Status hasil administratif" },
                    new() { Value = "isFitToWork", Label = "Status layak kerja" },
                    new() { Value = "expiredDate", Label = "Tanggal kedaluwarsa" },
                    new() { Value = "isVerified", Label = "Status verifikasi" },
                    new() { Value = "isActive", Label = "Status aktif" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            return Ok(ApiResponse<WfpHealthRecordFilterMetadataResponse>.Ok(
                result,
                "Metadata filter rekam kesehatan workforce berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<WfpHealthRecordSummaryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Health Record", Description = "Melihat ringkasan rekam kesehatan workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceHealthRecord", "Read")]
        public async Task<IActionResult> GetSummary(
            Guid workforceProfileId,
            CancellationToken cancellationToken)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
            {
                return WorkforceProfileNotFound();
            }

            var now = DateTime.UtcNow;
            var expiringSoon = now.AddDays(90);
            var query = _dbContext.Set<WfpHealthRecord>()
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            var result = new WfpHealthRecordSummaryResponse
            {
                TotalHealthRecord = await query.CountAsync(cancellationToken),
                ActiveHealthRecord = await query.CountAsync(x => x.IsActive, cancellationToken),
                InactiveHealthRecord = await query.CountAsync(x => !x.IsActive, cancellationToken),
                VerifiedHealthRecord = await query.CountAsync(x => x.IsVerified, cancellationToken),
                UnverifiedHealthRecord = await query.CountAsync(x => !x.IsVerified, cancellationToken),
                FitToWorkRecord = await query.CountAsync(x => x.IsFitToWork == true, cancellationToken),
                NotFitToWorkRecord = await query.CountAsync(x => x.IsFitToWork == false, cancellationToken),
                WorkRestrictionRecord = await query.CountAsync(x => x.WorkRestrictionRequired, cancellationToken),
                SensitiveRecord = await query.CountAsync(x => x.IsSensitive, cancellationToken),
                ExpiredRecord = await query.CountAsync(x => x.ExpiredDate.HasValue && x.ExpiredDate < now, cancellationToken),
                ExpiringSoonRecord = await query.CountAsync(x =>
                    x.ExpiredDate.HasValue &&
                    x.ExpiredDate >= now &&
                    x.ExpiredDate <= expiringSoon,
                    cancellationToken)
            };

            return Ok(ApiResponse<WfpHealthRecordSummaryResponse>.Ok(
                result,
                "Ringkasan rekam kesehatan workforce berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<WfpHealthRecordResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Health Record", Description = "Melihat data rekam kesehatan workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceHealthRecord", "Read")]
        public async Task<IActionResult> GetHealthRecords(
            Guid workforceProfileId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] string? recordType,
            [FromQuery] string? administrativeResultStatus,
            [FromQuery] string? accessClassification,
            [FromQuery] bool? isSensitive,
            [FromQuery] bool? isFitToWork,
            [FromQuery] bool? workRestrictionRequired,
            [FromQuery] bool? isVerified,
            [FromQuery] bool? isExpired,
            [FromQuery] bool? isActive,
            [FromQuery] int? expiringWithinDays,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "recordDate",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
            {
                return WorkforceProfileNotFound();
            }

            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery(workforceProfileId);
            query = ApplyFilter(
                query,
                startDate,
                endDate,
                customPeriod,
                recordType,
                administrativeResultStatus,
                accessClassification,
                isSensitive,
                isFitToWork,
                workRestrictionRequired,
                isVerified,
                isExpired,
                isActive,
                expiringWithinDays,
                search);

            var totalData = await query.CountAsync(cancellationToken);

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new WfpHealthRecordResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    WorkforceProfileCode = x.WorkforceProfile != null ? x.WorkforceProfile.ProfileCode : string.Empty,
                    WorkforceDisplayName = x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : string.Empty,
                    EmployeeId = x.EmployeeId,
                    EmployeeCode = x.Employee != null ? x.Employee.EmployeeCode : null,
                    EmployeeName = x.Employee != null ? x.Employee.FullName : null,
                    DoctorId = x.DoctorId,
                    DoctorCode = x.Doctor != null ? x.Doctor.DoctorCode : null,
                    DoctorName = x.Doctor != null ? x.Doctor.FullName : null,
                    RecordCode = x.RecordCode,
                    RecordType = x.RecordType,
                    RecordDate = x.RecordDate,
                    ProviderName = x.ProviderName,
                    AdministrativeResultStatus = x.AdministrativeResultStatus,
                    AdministrativeSummary = x.AdministrativeSummary,
                    AccessClassification = x.AccessClassification,
                    IsSensitive = x.IsSensitive,
                    IsFitToWork = x.IsFitToWork,
                    WorkRestrictionRequired = x.WorkRestrictionRequired,
                    ReminderDate = x.ReminderDate,
                    ExpiredDate = x.ExpiredDate,
                    IsExpired = x.ExpiredDate.HasValue && x.ExpiredDate < DateTime.UtcNow,
                    DaysUntilExpiry = x.ExpiredDate.HasValue
                        ? (int?)Math.Ceiling((x.ExpiredDate.Value - DateTime.UtcNow).TotalDays)
                        : null,
                    FilePath = x.FilePath,
                    FileContentType = x.FileContentType,
                    HasFile = x.FilePath != null && x.FilePath != string.Empty,
                    IsVerified = x.IsVerified,
                    VerifiedByUserId = x.VerifiedByUserId,
                    VerifiedByUserName = x.VerifiedByUser != null
                        ? x.VerifiedByUser.DisplayName ?? x.VerifiedByUser.UserName ?? x.VerifiedByUser.Email ?? x.VerifiedByUser.UserCode
                        : null,
                    VerifiedAt = x.VerifiedAt,
                    IsActive = x.IsActive,
                    Description = x.Description,
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                    CreateByName = x.CreateBy == Guid.Empty
                        ? null
                        : _dbContext.Users
                            .Where(u => u.Id == x.CreateBy)
                            .Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode)
                            .FirstOrDefault()
                })
                .ToListAsync(cancellationToken);

            var result = new PagedResult<WfpHealthRecordResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<PagedResult<WfpHealthRecordResponse>>.Ok(
                result,
                "Data rekam kesehatan workforce berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WfpHealthRecordDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Health Record", Description = "Melihat detail rekam kesehatan workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceHealthRecord", "Read")]
        public async Task<IActionResult> GetHealthRecordById(
            Guid workforceProfileId,
            Guid id,
            CancellationToken cancellationToken)
        {
            var data = await BuildBaseQuery(workforceProfileId)
                .Where(x => x.Id == id)
                .Select(x => new WfpHealthRecordDetailResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    WorkforceProfileCode = x.WorkforceProfile != null ? x.WorkforceProfile.ProfileCode : string.Empty,
                    WorkforceDisplayName = x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : string.Empty,
                    EmployeeId = x.EmployeeId,
                    EmployeeCode = x.Employee != null ? x.Employee.EmployeeCode : null,
                    EmployeeName = x.Employee != null ? x.Employee.FullName : null,
                    DoctorId = x.DoctorId,
                    DoctorCode = x.Doctor != null ? x.Doctor.DoctorCode : null,
                    DoctorName = x.Doctor != null ? x.Doctor.FullName : null,
                    RecordCode = x.RecordCode,
                    RecordType = x.RecordType,
                    RecordDate = x.RecordDate,
                    ProviderName = x.ProviderName,
                    AdministrativeResultStatus = x.AdministrativeResultStatus,
                    AdministrativeSummary = x.AdministrativeSummary,
                    ClinicalSummaryRestricted = x.ClinicalSummaryRestricted,
                    AccessClassification = x.AccessClassification,
                    IsSensitive = x.IsSensitive,
                    IsFitToWork = x.IsFitToWork,
                    WorkRestrictionRequired = x.WorkRestrictionRequired,
                    ReminderDate = x.ReminderDate,
                    ExpiredDate = x.ExpiredDate,
                    IsExpired = x.ExpiredDate.HasValue && x.ExpiredDate < DateTime.UtcNow,
                    DaysUntilExpiry = x.ExpiredDate.HasValue
                        ? (int?)Math.Ceiling((x.ExpiredDate.Value - DateTime.UtcNow).TotalDays)
                        : null,
                    FilePath = x.FilePath,
                    FileContentType = x.FileContentType,
                    HasFile = x.FilePath != null && x.FilePath != string.Empty,
                    IsVerified = x.IsVerified,
                    VerifiedByUserId = x.VerifiedByUserId,
                    VerifiedByUserName = x.VerifiedByUser != null
                        ? x.VerifiedByUser.DisplayName ?? x.VerifiedByUser.UserName ?? x.VerifiedByUser.Email ?? x.VerifiedByUser.UserCode
                        : null,
                    VerifiedAt = x.VerifiedAt,
                    IsActive = x.IsActive,
                    Description = x.Description,
                    MedicalExaminationCount = x.MedicalExaminations.Count(m => !m.IsDelete),
                    FitnessAssessmentCount = x.FitnessAssessments.Count(f => !f.IsDelete),
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                    CreateByName = x.CreateBy == Guid.Empty
                        ? null
                        : _dbContext.Users
                            .Where(u => u.Id == x.CreateBy)
                            .Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode)
                            .FirstOrDefault(),
                    UpdateDateTime = x.UpdateDateTime,
                    UpdateBy = x.UpdateBy == Guid.Empty ? null : x.UpdateBy,
                    UpdateByName = x.UpdateBy == Guid.Empty
                        ? null
                        : _dbContext.Users
                            .Where(u => u.Id == x.UpdateBy)
                            .Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode)
                            .FirstOrDefault()
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (data == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Rekam kesehatan workforce tidak ditemukan."));
            }

            return Ok(ApiResponse<WfpHealthRecordDetailResponse>.Ok(
                data,
                "Detail rekam kesehatan workforce berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<WfpHealthRecordDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Create", "Create Workforce Health Record", Description = "Membuat rekam kesehatan workforce", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("WorkforceHealthRecord", "Create")]
        public async Task<IActionResult> CreateHealthRecord(
            Guid workforceProfileId,
            [FromBody] CreateWfpHealthRecordRequest request,
            CancellationToken cancellationToken)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
            {
                return WorkforceProfileNotFound();
            }

            var validation = await ValidateRequestAsync(workforceProfileId, request, cancellationToken);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data rekam kesehatan workforce tidak valid."));
            }

            var subtype = await ResolveWorkforceSubtypeAsync(workforceProfileId, request.EmployeeId, request.DoctorId, cancellationToken);
            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            try
            {
                var entity = new WfpHealthRecord
                {
                    Id = Guid.NewGuid(),
                    WorkforceProfileId = workforceProfileId,
                    EmployeeId = subtype.EmployeeId,
                    DoctorId = subtype.DoctorId,
                    RecordCode = await GenerateCodeAsync(cancellationToken),
                    RecordType = NormalizeRecordType(request.RecordType),
                    RecordDate = NormalizeUtc(request.RecordDate),
                    ProviderName = NormalizeNullableText(request.ProviderName),
                    AdministrativeResultStatus = NormalizeNullableText(request.AdministrativeResultStatus),
                    AdministrativeSummary = NormalizeNullableText(request.AdministrativeSummary),
                    ClinicalSummaryRestricted = NormalizeNullableText(request.ClinicalSummaryRestricted),
                    AccessClassification = NormalizeAccessClassification(request.AccessClassification),
                    IsSensitive = request.IsSensitive,
                    IsFitToWork = request.IsFitToWork,
                    WorkRestrictionRequired = request.WorkRestrictionRequired,
                    ReminderDate = NormalizeUtcNullable(request.ReminderDate),
                    ExpiredDate = NormalizeUtcNullable(request.ExpiredDate),
                    FilePath = NormalizeNullableText(request.FilePath),
                    FileContentType = NormalizeNullableText(request.FileContentType),
                    IsVerified = request.IsVerified,
                    VerifiedByUserId = request.IsVerified ? actorUserId : null,
                    VerifiedAt = request.IsVerified ? now : null,
                    IsActive = request.IsActive,
                    Description = NormalizeNullableText(request.Description),
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                _dbContext.Set<WfpHealthRecord>().Add(entity);
                await _dbContext.SaveChangesAsync(cancellationToken);

                await _loggerService.InfoAsync(
                    LogCategory,
                    "WorkforceHealthRecord.CreateHealthRecord",
                    "Membuat rekam kesehatan workforce.",
                    new { entity.Id, entity.WorkforceProfileId, entity.RecordCode, entity.RecordType });

                return await GetHealthRecordById(workforceProfileId, entity.Id, cancellationToken);
            }
            catch (Exception ex)
            {
                await _loggerService.ErrorAsync(
                    LogCategory,
                    "WorkforceHealthRecord.CreateHealthRecord",
                    "Gagal membuat rekam kesehatan workforce.",
                    ex);

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat membuat rekam kesehatan workforce."));
            }
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WfpHealthRecordDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Workforce Health Record", Description = "Mengubah rekam kesehatan workforce", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceHealthRecord", "Update")]
        public async Task<IActionResult> UpdateHealthRecord(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWfpHealthRecordRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<WfpHealthRecord>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete,
                    cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Rekam kesehatan workforce tidak ditemukan."));
            }

            var validation = await ValidateRequestAsync(workforceProfileId, request, cancellationToken);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data rekam kesehatan workforce tidak valid."));
            }

            var subtype = await ResolveWorkforceSubtypeAsync(workforceProfileId, request.EmployeeId, request.DoctorId, cancellationToken);
            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.EmployeeId = subtype.EmployeeId;
            entity.DoctorId = subtype.DoctorId;
            entity.RecordType = NormalizeRecordType(request.RecordType);
            entity.RecordDate = NormalizeUtc(request.RecordDate);
            entity.ProviderName = NormalizeNullableText(request.ProviderName);
            entity.AdministrativeResultStatus = NormalizeNullableText(request.AdministrativeResultStatus);
            entity.AdministrativeSummary = NormalizeNullableText(request.AdministrativeSummary);
            entity.ClinicalSummaryRestricted = NormalizeNullableText(request.ClinicalSummaryRestricted);
            entity.AccessClassification = NormalizeAccessClassification(request.AccessClassification);
            entity.IsSensitive = request.IsSensitive;
            entity.IsFitToWork = request.IsFitToWork;
            entity.WorkRestrictionRequired = request.WorkRestrictionRequired;
            entity.ReminderDate = NormalizeUtcNullable(request.ReminderDate);
            entity.ExpiredDate = NormalizeUtcNullable(request.ExpiredDate);
            entity.FilePath = NormalizeNullableText(request.FilePath);
            entity.FileContentType = NormalizeNullableText(request.FileContentType);
            entity.IsVerified = request.IsVerified;
            entity.VerifiedByUserId = request.IsVerified
                ? entity.VerifiedByUserId ?? actorUserId
                : null;
            entity.VerifiedAt = request.IsVerified
                ? entity.VerifiedAt ?? now
                : null;
            entity.IsActive = request.IsActive;
            entity.Description = NormalizeNullableText(request.Description);
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceHealthRecord.UpdateHealthRecord",
                "Mengubah rekam kesehatan workforce.",
                new { entity.Id, entity.WorkforceProfileId, entity.RecordCode, entity.RecordType, entity.IsActive });

            return await GetHealthRecordById(workforceProfileId, id, cancellationToken);
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Workforce Health Record", Description = "Mengubah status rekam kesehatan workforce", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceHealthRecord", "Update")]
        public async Task<IActionResult> UpdateHealthRecordStatus(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWfpHealthRecordStatusRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await FindEntityAsync(workforceProfileId, id, cancellationToken);
            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Rekam kesehatan workforce tidak ditemukan."));
            }

            var expiredDate = NormalizeUtcNullable(request.ExpiredDate);
            if (expiredDate.HasValue && expiredDate.Value < entity.RecordDate)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "ExpiredDate tidak boleh lebih kecil dari RecordDate."));
            }

            entity.IsActive = request.IsActive;
            entity.ExpiredDate = expiredDate;
            entity.Description = NormalizeNullableText(request.Description) ?? entity.Description;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status rekam kesehatan workforce berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/verify")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Verify Workforce Health Record", Description = "Memverifikasi rekam kesehatan workforce", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceHealthRecord", "Update")]
        public async Task<IActionResult> VerifyHealthRecord(
            Guid workforceProfileId,
            Guid id,
            [FromBody] VerifyWfpHealthRecordRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await FindEntityAsync(workforceProfileId, id, cancellationToken);
            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Rekam kesehatan workforce tidak ditemukan."));
            }

            if (request.IsVerified && entity.RecordDate > DateTime.UtcNow)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Catatan kesehatan dengan RecordDate di masa depan tidak dapat diverifikasi."));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsVerified = request.IsVerified;
            entity.VerifiedByUserId = request.IsVerified ? actorUserId : null;
            entity.VerifiedAt = request.IsVerified ? now : null;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<object>.Ok(
                null,
                request.IsVerified
                    ? "Rekam kesehatan workforce berhasil diverifikasi."
                    : "Verifikasi rekam kesehatan workforce berhasil dibatalkan."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Workforce Health Record", Description = "Menghapus rekam kesehatan workforce", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("WorkforceHealthRecord", "Delete")]
        public async Task<IActionResult> DeleteHealthRecord(
            Guid workforceProfileId,
            Guid id,
            CancellationToken cancellationToken)
        {
            var entity = await FindEntityAsync(workforceProfileId, id, cancellationToken);
            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Rekam kesehatan workforce tidak ditemukan."));
            }

            var usage = await _dbContext.Set<WfpHealthRecord>()
                .AsNoTracking()
                .Where(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete)
                .Select(x => new
                {
                    HasMedicalExamination = x.MedicalExaminations.Any(m => !m.IsDelete),
                    HasFitnessAssessment = x.FitnessAssessments.Any(f => !f.IsDelete)
                })
                .FirstAsync(cancellationToken);

            if (usage.HasMedicalExamination || usage.HasFitnessAssessment)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Rekam kesehatan tidak dapat dihapus karena sudah digunakan pada pemeriksaan kesehatan atau asesmen fitness to work."));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceHealthRecord.DeleteHealthRecord",
                "Menghapus rekam kesehatan workforce.",
                new { entity.Id, entity.WorkforceProfileId, entity.RecordCode, entity.DeleteDateTime });

            return Ok(ApiResponse<object>.Ok(
                null,
                "Rekam kesehatan workforce berhasil dihapus."));
        }

        private IQueryable<WfpHealthRecord> BuildBaseQuery(Guid workforceProfileId)
        {
            return _dbContext.Set<WfpHealthRecord>()
                .AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.Employee)
                .Include(x => x.Doctor)
                .Include(x => x.VerifiedByUser)
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);
        }

        private static IQueryable<WfpHealthRecord> ApplyFilter(
            IQueryable<WfpHealthRecord> query,
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod,
            string? recordType,
            string? administrativeResultStatus,
            string? accessClassification,
            bool? isSensitive,
            bool? isFitToWork,
            bool? workRestrictionRequired,
            bool? isVerified,
            bool? isExpired,
            bool? isActive,
            int? expiringWithinDays,
            string? search)
        {
            var range = ResolveDateRange(startDate, endDate, customPeriod);
            if (range.Start.HasValue)
                query = query.Where(x => x.RecordDate >= range.Start.Value);
            if (range.EndExclusive.HasValue)
                query = query.Where(x => x.RecordDate < range.EndExclusive.Value);

            if (!string.IsNullOrWhiteSpace(recordType))
            {
                var value = recordType.Trim().ToLower();
                query = query.Where(x => x.RecordType.ToLower() == value);
            }

            if (!string.IsNullOrWhiteSpace(administrativeResultStatus))
            {
                var value = administrativeResultStatus.Trim().ToLower();
                query = query.Where(x =>
                    x.AdministrativeResultStatus != null &&
                    x.AdministrativeResultStatus.ToLower() == value);
            }

            if (!string.IsNullOrWhiteSpace(accessClassification))
            {
                var value = accessClassification.Trim().ToLower();
                query = query.Where(x => x.AccessClassification.ToLower() == value);
            }

            if (isSensitive.HasValue)
                query = query.Where(x => x.IsSensitive == isSensitive.Value);
            if (isFitToWork.HasValue)
                query = query.Where(x => x.IsFitToWork == isFitToWork.Value);
            if (workRestrictionRequired.HasValue)
                query = query.Where(x => x.WorkRestrictionRequired == workRestrictionRequired.Value);
            if (isVerified.HasValue)
                query = query.Where(x => x.IsVerified == isVerified.Value);
            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            var now = DateTime.UtcNow;
            if (isExpired.HasValue)
            {
                query = isExpired.Value
                    ? query.Where(x => x.ExpiredDate.HasValue && x.ExpiredDate < now)
                    : query.Where(x => !x.ExpiredDate.HasValue || x.ExpiredDate >= now);
            }

            if (expiringWithinDays.HasValue && expiringWithinDays.Value >= 0)
            {
                var until = now.AddDays(expiringWithinDays.Value);
                query = query.Where(x =>
                    x.ExpiredDate.HasValue &&
                    x.ExpiredDate >= now &&
                    x.ExpiredDate <= until);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.RecordCode.ToLower().Contains(keyword) ||
                    x.RecordType.ToLower().Contains(keyword) ||
                    (x.ProviderName != null && x.ProviderName.ToLower().Contains(keyword)) ||
                    (x.AdministrativeResultStatus != null && x.AdministrativeResultStatus.ToLower().Contains(keyword)) ||
                    (x.AdministrativeSummary != null && x.AdministrativeSummary.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<WfpHealthRecord> ApplySorting(
            IQueryable<WfpHealthRecord> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDescending = !string.Equals(
                sortDirection?.Trim(),
                "asc",
                StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "recordDate").Trim().ToLowerInvariant() switch
            {
                "recordcode" => isDescending
                    ? query.OrderByDescending(x => x.RecordCode)
                    : query.OrderBy(x => x.RecordCode),

                "recordtype" => isDescending
                    ? query.OrderByDescending(x => x.RecordType).ThenByDescending(x => x.RecordDate)
                    : query.OrderBy(x => x.RecordType).ThenBy(x => x.RecordDate),

                "administrativeresultstatus" => isDescending
                    ? query.OrderByDescending(x => x.AdministrativeResultStatus).ThenByDescending(x => x.RecordDate)
                    : query.OrderBy(x => x.AdministrativeResultStatus).ThenBy(x => x.RecordDate),

                "isfittowork" => isDescending
                    ? query.OrderByDescending(x => x.IsFitToWork).ThenByDescending(x => x.RecordDate)
                    : query.OrderBy(x => x.IsFitToWork).ThenBy(x => x.RecordDate),

                "expireddate" => isDescending
                    ? query.OrderByDescending(x => x.ExpiredDate).ThenByDescending(x => x.RecordDate)
                    : query.OrderBy(x => x.ExpiredDate).ThenBy(x => x.RecordDate),

                "isverified" => isDescending
                    ? query.OrderByDescending(x => x.IsVerified).ThenByDescending(x => x.RecordDate)
                    : query.OrderBy(x => x.IsVerified).ThenBy(x => x.RecordDate),

                "isactive" => isDescending
                    ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.RecordDate)
                    : query.OrderBy(x => x.IsActive).ThenBy(x => x.RecordDate),

                "createdatetime" => isDescending
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),

                _ => isDescending
                    ? query.OrderByDescending(x => x.RecordDate).ThenByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.RecordDate).ThenBy(x => x.CreateDateTime)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid workforceProfileId,
            CreateWfpHealthRecordRequest request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.RecordType))
                return (false, "Jenis rekam kesehatan wajib diisi.");

            if (!AllowedRecordTypes.Contains(request.RecordType.Trim()))
                return (false, "Jenis rekam kesehatan tidak valid.");

            if (request.RecordDate == default)
                return (false, "RecordDate wajib diisi.");

            if (string.IsNullOrWhiteSpace(request.AccessClassification))
                return (false, "AccessClassification wajib diisi.");

            if (!AllowedAccessClassifications.Contains(request.AccessClassification.Trim()))
                return (false, "AccessClassification tidak valid. Gunakan Restricted, Confidential, atau Administrative.");

            var recordDate = NormalizeUtc(request.RecordDate);
            var reminderDate = NormalizeUtcNullable(request.ReminderDate);
            var expiredDate = NormalizeUtcNullable(request.ExpiredDate);

            if (expiredDate.HasValue && expiredDate.Value < recordDate)
                return (false, "ExpiredDate tidak boleh lebih kecil dari RecordDate.");

            if (reminderDate.HasValue && expiredDate.HasValue && reminderDate.Value > expiredDate.Value)
                return (false, "ReminderDate tidak boleh lebih besar dari ExpiredDate.");

            if (request.WorkRestrictionRequired && request.IsFitToWork == true &&
                string.IsNullOrWhiteSpace(request.AdministrativeSummary))
            {
                return (false, "AdministrativeSummary wajib diisi jika terdapat pembatasan kerja.");
            }

            if (!string.IsNullOrWhiteSpace(request.FileContentType) &&
                string.IsNullOrWhiteSpace(request.FilePath))
            {
                return (false, "FilePath wajib diisi jika FileContentType diisi.");
            }

            if (request.EmployeeId.HasValue && request.DoctorId.HasValue)
                return (false, "EmployeeId dan DoctorId tidak boleh diisi bersamaan.");

            if (request.EmployeeId.HasValue)
            {
                var employeeExists = await _dbContext.MstEmployees
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id == request.EmployeeId.Value &&
                        x.WorkforceProfileId == workforceProfileId &&
                        x.IsActive &&
                        !x.IsDelete,
                        cancellationToken);

                if (!employeeExists)
                    return (false, "Employee tidak ditemukan, tidak aktif, atau tidak sesuai workforce profile.");
            }

            if (request.DoctorId.HasValue)
            {
                var doctorExists = await _dbContext.MstDoctors
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id == request.DoctorId.Value &&
                        x.WorkforceProfileId == workforceProfileId &&
                        x.IsActive &&
                        !x.IsDelete,
                        cancellationToken);

                if (!doctorExists)
                    return (false, "Doctor tidak ditemukan, tidak aktif, atau tidak sesuai workforce profile.");
            }

            return (true, null);
        }

        private async Task<(Guid? EmployeeId, Guid? DoctorId)> ResolveWorkforceSubtypeAsync(
            Guid workforceProfileId,
            Guid? requestedEmployeeId,
            Guid? requestedDoctorId,
            CancellationToken cancellationToken)
        {
            if (requestedEmployeeId.HasValue)
                return (requestedEmployeeId.Value, null);

            if (requestedDoctorId.HasValue)
                return (null, requestedDoctorId.Value);

            var employeeId = await _dbContext.MstEmployees
                .AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.IsActive &&
                    !x.IsDelete)
                .Select(x => (Guid?)x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (employeeId.HasValue)
                return (employeeId, null);

            var doctorId = await _dbContext.MstDoctors
                .AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.IsActive &&
                    !x.IsDelete)
                .Select(x => (Guid?)x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            return (null, doctorId);
        }

        private async Task<bool> WorkforceProfileExistsAsync(
            Guid workforceProfileId,
            CancellationToken cancellationToken)
        {
            return workforceProfileId != Guid.Empty &&
                   await _dbContext.MstWorkforceProfiles
                       .AsNoTracking()
                       .AnyAsync(x =>
                           x.Id == workforceProfileId &&
                           x.IsActive &&
                           !x.IsDelete,
                           cancellationToken);
        }

        private async Task<WfpHealthRecord?> FindEntityAsync(
            Guid workforceProfileId,
            Guid id,
            CancellationToken cancellationToken)
        {
            return await _dbContext.Set<WfpHealthRecord>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete,
                    cancellationToken);
        }

        private IActionResult WorkforceProfileNotFound()
        {
            return NotFound(ApiResponse<object>.Fail(
                StatusCodes.Status404NotFound,
                "Profil tenaga kerja tidak ditemukan atau tidak aktif."));
        }

        private async Task<string> GenerateCodeAsync(CancellationToken cancellationToken)
        {
            var codes = await _dbContext.Set<WfpHealthRecord>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.RecordCode.StartsWith(CodePrefix))
                .Select(x => x.RecordCode)
                .ToListAsync(cancellationToken);

            return GenerateNextCode(codes, CodePrefix, CodeNumberLength);
        }

        private Guid GetCurrentUserId()
        {
            var userIdText =
                User.FindFirstValue("user_id") ??
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userIdText, out var userId)
                ? userId
                : Guid.Empty;
        }

        private static string NormalizeRecordType(string value)
        {
            return AllowedRecordTypes.First(x =>
                string.Equals(x, value.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        private static string NormalizeAccessClassification(string value)
        {
            return AllowedAccessClassifications.First(x =>
                string.Equals(x, value.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static DateTime NormalizeUtc(DateTime value)
        {
            return value.Kind == DateTimeKind.Utc
                ? value
                : DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

        private static DateTime? NormalizeUtcNullable(DateTime? value)
        {
            return value.HasValue ? NormalizeUtc(value.Value) : null;
        }

        private static (int PageNumber, int PageSize) NormalizePaging(
            int pageNumber,
            int pageSize)
        {
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 25 : Math.Min(pageSize, 100);
            return (pageNumber, pageSize);
        }

        private static string GenerateNextCode(
            IEnumerable<string> codes,
            string prefix,
            int length)
        {
            var usedNumbers = codes
                .Select(x => x.Replace(prefix, string.Empty))
                .Where(x => int.TryParse(x, out _))
                .Select(int.Parse)
                .Where(x => x > 0)
                .ToHashSet();

            var nextNumber = 1;
            while (usedNumbers.Contains(nextNumber))
                nextNumber++;

            return prefix + nextNumber.ToString().PadLeft(length, '0');
        }

        private static (DateTime? Start, DateTime? EndExclusive) ResolveDateRange(
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            if (startDate.HasValue || endDate.HasValue)
            {
                return (
                    startDate.HasValue
                        ? DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc)
                        : null,
                    endDate.HasValue
                        ? DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc)
                        : null);
            }

            var today = DateTime.UtcNow.Date;
            return customPeriod?.Trim().ToLowerInvariant() switch
            {
                "today" => (today, today.AddDays(1)),
                "last7days" => (today.AddDays(-6), today.AddDays(1)),
                "last30days" => (today.AddDays(-29), today.AddDays(1)),
                "thismonth" => (
                    new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc),
                    new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1)),
                "lastmonth" => (
                    new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-1),
                    new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc)),
                _ => (null, null)
            };
        }

        private static List<WfpHealthRecordStringOptionResponse> BuildPeriodOptions()
        {
            return new List<WfpHealthRecordStringOptionResponse>
            {
                new() { Value = "today", Label = "Hari ini" },
                new() { Value = "last7days", Label = "7 hari terakhir" },
                new() { Value = "last30days", Label = "30 hari terakhir" },
                new() { Value = "thismonth", Label = "Bulan ini" },
                new() { Value = "lastmonth", Label = "Bulan lalu" }
            };
        }

        private static string BuildLabel(string value)
        {
            return value switch
            {
                "MedicalExamination" => "Pemeriksaan kesehatan",
                "FitnessToWork" => "Kelayakan bekerja",
                "Vaccination" => "Vaksinasi",
                "WorkRestriction" => "Pembatasan kerja",
                "OccupationalExposure" => "Paparan kerja",
                "Injury" => "Cedera kerja",
                "ReturnToWork" => "Kembali bekerja",
                "Restricted" => "Terbatas",
                "Confidential" => "Rahasia",
                "Administrative" => "Administratif",
                _ => value
            };
        }
    }
}
