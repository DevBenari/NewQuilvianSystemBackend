using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Enum;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/health-records")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Workforce Health Record",
        AreaName = "Corporate",
        ControllerName = "WorkforceHealthRecord",
        Description = "Workforce occupational health record management",
        SortOrder = 29
    )]
    [Tags("Corporate / Human Resource / Workforce / Health Record")]
    public class WorkforceHealthRecordController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.Workforce.HealthRecord";
        private const long MaxFileSizeBytes = 10 * 1024 * 1024;

        private static readonly string[] AllowedExtensions =
        {
            ".pdf",
            ".jpg",
            ".jpeg",
            ".png",
            ".doc",
            ".docx",
            ".xls",
            ".xlsx"
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        private readonly IWebHostEnvironment _environment;

        public WorkforceHealthRecordController(
            ApplicationDbContext dbContext,
            LoggerService loggerService,
            IWebHostEnvironment environment)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
            _environment = environment;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<WorkforceHealthRecordListResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Workforce Health Record",
            Description = "Melihat health record workforce profile",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceHealthRecord", "Read")]
        public async Task<IActionResult> GetHealthRecords(
            Guid workforceProfileId,
            [FromQuery] HealthRecordType? healthRecordType,
            [FromQuery] HealthRecordResultStatus? resultStatus,
            [FromQuery] bool? isActive,
            [FromQuery] bool? isVerified,
            [FromQuery] bool? isFitToWork,
            [FromQuery] bool? isExpired,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var today = DateTime.UtcNow.Date;

            var query = _dbContext.Set<WfpHealthRecord>()
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            if (healthRecordType.HasValue)
            {
                query = query.Where(x => x.HealthRecordType == healthRecordType.Value);
            }

            if (resultStatus.HasValue)
            {
                query = query.Where(x => x.ResultStatus == resultStatus.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            if (isVerified.HasValue)
            {
                query = query.Where(x => x.IsVerified == isVerified.Value);
            }

            if (isFitToWork.HasValue)
            {
                query = query.Where(x => x.IsFitToWork == isFitToWork.Value);
            }

            if (isExpired.HasValue)
            {
                query = isExpired.Value
                    ? query.Where(x => x.ExpiredDate.HasValue && x.ExpiredDate.Value.Date < today)
                    : query.Where(x => !x.ExpiredDate.HasValue || x.ExpiredDate.Value.Date >= today);
            }

            if (startDate.HasValue)
            {
                query = query.Where(x => x.RecordDate >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                query = query.Where(x => x.RecordDate <= endDate.Value.Date);
            }

            var rawItems = await query
                .OrderByDescending(x => x.RecordDate)
                .ThenByDescending(x => x.IsActive)
                .ThenByDescending(x => x.IsVerified)
                .ThenBy(x => x.HealthRecordType)
                .Select(x => new
                {
                    x.Id,
                    x.WorkforceProfileId,
                    x.RequirementCode,
                    x.HealthRecordType,
                    x.RecordDate,
                    x.ResultStatus,
                    x.ProviderName,
                    x.ExpiredDate,
                    x.IsFitToWork,
                    x.FitToWorkRestrictionNote,
                    x.IsVerified,
                    x.VerifiedByUserId,
                    VerifiedByUserName = x.VerifiedByUser != null ? x.VerifiedByUser.DisplayName : null,
                    x.VerifiedAt,
                    x.VerificationNote,
                    x.FilePath,
                    x.FileContentType,
                    x.Notes,
                    x.IsActive,
                    x.CreateDateTime
                })
                .ToListAsync();

            var items = rawItems.Select(x => new WorkforceHealthRecordResponse
            {
                Id = x.Id,
                WorkforceProfileId = x.WorkforceProfileId,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                RequirementCode = x.RequirementCode,
                HealthRecordType = x.HealthRecordType,
                RecordDate = x.RecordDate,
                ResultStatus = x.ResultStatus,
                ProviderName = x.ProviderName,
                ExpiredDate = x.ExpiredDate,
                IsFitToWork = x.IsFitToWork,
                FitToWorkRestrictionNote = x.FitToWorkRestrictionNote,
                IsVerified = x.IsVerified,
                VerifiedByUserId = x.VerifiedByUserId,
                VerifiedByUserName = x.VerifiedByUserName,
                VerifiedAt = x.VerifiedAt,
                VerificationNote = x.VerificationNote,
                FilePath = x.FilePath,
                FileContentType = x.FileContentType,
                HasFile = !string.IsNullOrWhiteSpace(x.FilePath),
                Notes = x.Notes,
                IsExpired = x.ExpiredDate.HasValue && x.ExpiredDate.Value.Date < today,
                IsCurrentlyValid = x.IsActive &&
                    x.IsVerified &&
                    (!x.ExpiredDate.HasValue || x.ExpiredDate.Value.Date >= today),
                IsCompliantForWork = x.IsActive &&
                    x.IsVerified &&
                    (!x.ExpiredDate.HasValue || x.ExpiredDate.Value.Date >= today) &&
                    (!x.IsFitToWork.HasValue || x.IsFitToWork.Value),
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime
            }).ToList();

            var result = new WorkforceHealthRecordListResponse
            {
                WorkforceProfileId = profile.Id,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                TotalData = items.Count,
                ActiveData = items.Count(x => x.IsActive),
                VerifiedData = items.Count(x => x.IsVerified),
                ExpiredData = items.Count(x => x.IsExpired),
                CurrentlyValidData = items.Count(x => x.IsCurrentlyValid),
                FitToWorkData = items.Count(x => x.IsFitToWork == true),
                NotFitToWorkData = items.Count(x => x.IsFitToWork == false),
                CompliantForWorkData = items.Count(x => x.IsCompliantForWork),
                WithFileData = items.Count(x => x.HasFile),
                Items = items
            };

            return Ok(ApiResponse<WorkforceHealthRecordListResponse>.Ok(
                result,
                "Data health record workforce berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceHealthRecordResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Workforce Health Record",
            Description = "Melihat detail health record workforce profile",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceHealthRecord", "Read")]
        public async Task<IActionResult> GetHealthRecordById(Guid workforceProfileId, Guid id)
        {
            var response = await BuildHealthRecordResponseAsync(id, workforceProfileId);

            if (response == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Health record workforce tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<WorkforceHealthRecordResponse>.Ok(
                response,
                "Detail health record workforce berhasil diambil."
            ));
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceHealthRecordResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Create",
            "Create Workforce Health Record",
            Description = "Menambah health record workforce profile",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("WorkforceHealthRecord", "Create")]
        public async Task<IActionResult> CreateHealthRecord(
            Guid workforceProfileId,
            [FromForm] CreateWorkforceHealthRecordRequest request)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var validation = ValidateHealthRecordRequest(
                request.HealthRecordType,
                request.RecordDate,
                request.ExpiredDate,
                request.IsFitToWork,
                request.FitToWorkRestrictionNote,
                request.File);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data health record tidak valid."
                ));
            }

            var normalizedRequirementCode = NormalizeRequirementCodeOrNull(request.RequirementCode);

            if (!string.IsNullOrWhiteSpace(normalizedRequirementCode))
            {
                var requirementValid = await ValidateRequirementCodeAsync(
                    profile.UserType,
                    "HealthRecord",
                    normalizedRequirementCode);

                if (!requirementValid.IsValid)
                {
                    return BadRequest(ApiResponse<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        requirementValid.ErrorMessage ?? "RequirementCode tidak valid."
                    ));
                }
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            string? filePath = null;
            string? fileContentType = null;

            if (request.File != null)
            {
                var savedFile = await SaveHealthRecordFileAsync(workforceProfileId, request.File);
                filePath = savedFile.FilePath;
                fileContentType = savedFile.ContentType;
            }

            var entity = new WfpHealthRecord
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = workforceProfileId,
                RequirementCode = normalizedRequirementCode,
                HealthRecordType = request.HealthRecordType,
                RecordDate = request.RecordDate.Date,
                ResultStatus = request.ResultStatus,
                ProviderName = NormalizeNullableText(request.ProviderName),
                ExpiredDate = request.ExpiredDate?.Date,
                IsFitToWork = request.IsFitToWork,
                FitToWorkRestrictionNote = NormalizeNullableText(request.FitToWorkRestrictionNote),
                IsVerified = false,
                VerifiedByUserId = null,
                VerifiedAt = null,
                VerificationNote = null,
                FilePath = filePath,
                FileContentType = fileContentType,
                Notes = NormalizeNullableText(request.Notes),
                IsActive = request.IsActive,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<WfpHealthRecord>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var response = await BuildHealthRecordResponseAsync(entity.Id, workforceProfileId);

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceHealthRecord.CreateHealthRecord",
                "Health record workforce berhasil dibuat.",
                new
                {
                    WorkforceProfileId = workforceProfileId,
                    entity.Id,
                    entity.HealthRecordType,
                    entity.RecordDate,
                    entity.ResultStatus
                }
            );

            return Ok(ApiResponse<WorkforceHealthRecordResponse>.Ok(
                response!,
                "Health record workforce berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceHealthRecordResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Update Workforce Health Record",
            Description = "Mengubah health record workforce profile",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceHealthRecord", "Update")]
        public async Task<IActionResult> UpdateHealthRecord(
            Guid workforceProfileId,
            Guid id,
            [FromForm] UpdateWorkforceHealthRecordRequest request)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var entity = await _dbContext.Set<WfpHealthRecord>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Health record workforce tidak ditemukan."
                ));
            }

            if (entity.IsVerified)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Health record yang sudah verified tidak boleh diubah. Gunakan unverify terlebih dahulu jika perlu koreksi."
                ));
            }

            var validation = ValidateHealthRecordRequest(
                request.HealthRecordType,
                request.RecordDate,
                request.ExpiredDate,
                request.IsFitToWork,
                request.FitToWorkRestrictionNote,
                request.File);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data health record tidak valid."
                ));
            }

            var normalizedRequirementCode = NormalizeRequirementCodeOrNull(request.RequirementCode);

            if (!string.IsNullOrWhiteSpace(normalizedRequirementCode))
            {
                var requirementValid = await ValidateRequirementCodeAsync(
                    profile.UserType,
                    "HealthRecord",
                    normalizedRequirementCode);

                if (!requirementValid.IsValid)
                {
                    return BadRequest(ApiResponse<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        requirementValid.ErrorMessage ?? "RequirementCode tidak valid."
                    ));
                }
            }

            if (request.ReplaceExistingFile && request.File == null)
            {
                entity.FilePath = null;
                entity.FileContentType = null;
            }

            if (request.File != null)
            {
                var savedFile = await SaveHealthRecordFileAsync(workforceProfileId, request.File);
                entity.FilePath = savedFile.FilePath;
                entity.FileContentType = savedFile.ContentType;
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.RequirementCode = normalizedRequirementCode;
            entity.HealthRecordType = request.HealthRecordType;
            entity.RecordDate = request.RecordDate.Date;
            entity.ResultStatus = request.ResultStatus;
            entity.ProviderName = NormalizeNullableText(request.ProviderName);
            entity.ExpiredDate = request.ExpiredDate?.Date;
            entity.IsFitToWork = request.IsFitToWork;
            entity.FitToWorkRestrictionNote = NormalizeNullableText(request.FitToWorkRestrictionNote);
            entity.Notes = NormalizeNullableText(request.Notes);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var response = await BuildHealthRecordResponseAsync(entity.Id, workforceProfileId);

            return Ok(ApiResponse<WorkforceHealthRecordResponse>.Ok(
                response!,
                "Health record workforce berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceHealthRecordResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Update Workforce Health Record",
            Description = "Mengubah status aktif health record workforce profile",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceHealthRecord", "Update")]
        public async Task<IActionResult> UpdateHealthRecordStatus(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWorkforceHealthRecordStatusRequest request)
        {
            var entity = await _dbContext.Set<WfpHealthRecord>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Health record workforce tidak ditemukan."
                ));
            }

            entity.IsActive = request.IsActive;
            entity.Notes = NormalizeNullableText(request.Notes) ?? entity.Notes;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            var response = await BuildHealthRecordResponseAsync(entity.Id, workforceProfileId);

            return Ok(ApiResponse<WorkforceHealthRecordResponse>.Ok(
                response!,
                "Status health record workforce berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/verify")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceHealthRecordResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Update Workforce Health Record",
            Description = "Verifikasi health record workforce profile",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceHealthRecord", "Update")]
        public async Task<IActionResult> VerifyHealthRecord(
            Guid workforceProfileId,
            Guid id,
            [FromBody] VerifyWorkforceHealthRecordRequest request)
        {
            var entity = await _dbContext.Set<WfpHealthRecord>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Health record workforce tidak ditemukan."
                ));
            }

            if (!entity.IsActive)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Health record nonaktif tidak bisa diverifikasi."
                ));
            }

            if (entity.ExpiredDate.HasValue && entity.ExpiredDate.Value.Date < DateTime.UtcNow.Date)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Health record sudah expired dan tidak bisa diverifikasi."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsVerified = true;
            entity.VerifiedByUserId = actorUserId;
            entity.VerifiedAt = now;
            entity.VerificationNote = NormalizeNullableText(request.VerificationNote);
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var response = await BuildHealthRecordResponseAsync(entity.Id, workforceProfileId);

            return Ok(ApiResponse<WorkforceHealthRecordResponse>.Ok(
                response!,
                "Health record workforce berhasil diverifikasi."
            ));
        }

        [HttpPatch("{id:guid}/unverify")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceHealthRecordResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Update Workforce Health Record",
            Description = "Membatalkan verifikasi health record workforce profile",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceHealthRecord", "Update")]
        public async Task<IActionResult> UnverifyHealthRecord(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UnverifyWorkforceHealthRecordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UnverifyReason))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Alasan unverify wajib diisi."
                ));
            }

            var entity = await _dbContext.Set<WfpHealthRecord>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Health record workforce tidak ditemukan."
                ));
            }

            entity.IsVerified = false;
            entity.VerifiedByUserId = null;
            entity.VerifiedAt = null;
            entity.VerificationNote = NormalizeNullableText(request.UnverifyReason);
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            var response = await BuildHealthRecordResponseAsync(entity.Id, workforceProfileId);

            return Ok(ApiResponse<WorkforceHealthRecordResponse>.Ok(
                response!,
                "Verifikasi health record workforce berhasil dibatalkan."
            ));
        }

        [HttpGet("{id:guid}/download")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Workforce Health Record",
            Description = "Download file health record workforce profile",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceHealthRecord", "Read")]
        public async Task<IActionResult> DownloadHealthRecordFile(Guid workforceProfileId, Guid id)
        {
            var record = await _dbContext.Set<WfpHealthRecord>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (record == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Health record workforce tidak ditemukan."
                ));
            }

            if (string.IsNullOrWhiteSpace(record.FilePath))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Health record belum memiliki file."
                ));
            }

            var relativePath = record.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var rootPath = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
            var physicalPath = Path.Combine(rootPath, relativePath);

            if (!System.IO.File.Exists(physicalPath))
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "File health record tidak ditemukan di server."
                ));
            }

            var provider = new FileExtensionContentTypeProvider();

            if (!provider.TryGetContentType(physicalPath, out var contentType))
            {
                contentType = record.FileContentType ?? "application/octet-stream";
            }

            var fileName = Path.GetFileName(physicalPath);
            var bytes = await System.IO.File.ReadAllBytesAsync(physicalPath);

            return File(bytes, contentType, fileName);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction(
            "Delete",
            "Delete Workforce Health Record",
            Description = "Menghapus health record workforce profile",
            AccessType = AccessTypes.Delete,
            SortOrder = 4
        )]
        [AccessPermission("WorkforceHealthRecord", "Delete")]
        public async Task<IActionResult> DeleteHealthRecord(Guid workforceProfileId, Guid id)
        {
            var entity = await _dbContext.Set<WfpHealthRecord>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Health record workforce tidak ditemukan."
                ));
            }

            if (entity.IsVerified)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Health record verified tidak boleh dihapus. Gunakan unverify terlebih dahulu jika perlu koreksi."
                ));
            }

            entity.IsActive = false;
            entity.IsDelete = true;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Health record workforce berhasil dihapus."
            ));
        }

        private async Task<MstWorkforceProfile?> GetProfileAsync(Guid workforceProfileId)
        {
            return await _dbContext.Set<MstWorkforceProfile>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == workforceProfileId && !x.IsDelete);
        }

        private async Task<WorkforceHealthRecordResponse?> BuildHealthRecordResponseAsync(
            Guid id,
            Guid workforceProfileId)
        {
            var today = DateTime.UtcNow.Date;

            var item = await _dbContext.Set<WfpHealthRecord>()
                .AsNoTracking()
                .Where(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete)
                .Select(x => new
                {
                    x.Id,
                    x.WorkforceProfileId,
                    ProfileCode = x.WorkforceProfile != null ? x.WorkforceProfile.ProfileCode : string.Empty,
                    DisplayName = x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : string.Empty,
                    x.RequirementCode,
                    x.HealthRecordType,
                    x.RecordDate,
                    x.ResultStatus,
                    x.ProviderName,
                    x.ExpiredDate,
                    x.IsFitToWork,
                    x.FitToWorkRestrictionNote,
                    x.IsVerified,
                    x.VerifiedByUserId,
                    VerifiedByUserName = x.VerifiedByUser != null ? x.VerifiedByUser.DisplayName : null,
                    x.VerifiedAt,
                    x.VerificationNote,
                    x.FilePath,
                    x.FileContentType,
                    x.Notes,
                    x.IsActive,
                    x.CreateDateTime
                })
                .FirstOrDefaultAsync();

            if (item == null)
            {
                return null;
            }

            return new WorkforceHealthRecordResponse
            {
                Id = item.Id,
                WorkforceProfileId = item.WorkforceProfileId,
                ProfileCode = item.ProfileCode,
                DisplayName = item.DisplayName,
                RequirementCode = item.RequirementCode,
                HealthRecordType = item.HealthRecordType,
                RecordDate = item.RecordDate,
                ResultStatus = item.ResultStatus,
                ProviderName = item.ProviderName,
                ExpiredDate = item.ExpiredDate,
                IsFitToWork = item.IsFitToWork,
                FitToWorkRestrictionNote = item.FitToWorkRestrictionNote,
                IsVerified = item.IsVerified,
                VerifiedByUserId = item.VerifiedByUserId,
                VerifiedByUserName = item.VerifiedByUserName,
                VerifiedAt = item.VerifiedAt,
                VerificationNote = item.VerificationNote,
                FilePath = item.FilePath,
                FileContentType = item.FileContentType,
                HasFile = !string.IsNullOrWhiteSpace(item.FilePath),
                Notes = item.Notes,
                IsExpired = item.ExpiredDate.HasValue && item.ExpiredDate.Value.Date < today,
                IsCurrentlyValid = item.IsActive &&
                    item.IsVerified &&
                    (!item.ExpiredDate.HasValue || item.ExpiredDate.Value.Date >= today),
                IsCompliantForWork = item.IsActive &&
                    item.IsVerified &&
                    (!item.ExpiredDate.HasValue || item.ExpiredDate.Value.Date >= today) &&
                    (!item.IsFitToWork.HasValue || item.IsFitToWork.Value),
                IsActive = item.IsActive,
                CreateDateTime = item.CreateDateTime
            };
        }

        private static (bool IsValid, string? ErrorMessage) ValidateHealthRecordRequest(
            HealthRecordType healthRecordType,
            DateTime recordDate,
            DateTime? expiredDate,
            bool? isFitToWork,
            string? fitToWorkRestrictionNote,
            IFormFile? file)
        {
            if (healthRecordType == HealthRecordType.Unknown)
            {
                return (false, "HealthRecordType wajib dipilih.");
            }

            if (recordDate == default)
            {
                return (false, "RecordDate wajib diisi.");
            }

            if (expiredDate.HasValue && recordDate.Date > expiredDate.Value.Date)
            {
                return (false, "RecordDate tidak boleh lebih besar dari ExpiredDate.");
            }

            if (isFitToWork == false && string.IsNullOrWhiteSpace(fitToWorkRestrictionNote))
            {
                return (false, "FitToWorkRestrictionNote wajib diisi jika IsFitToWork = false.");
            }

            if (file != null)
            {
                if (file.Length <= 0)
                {
                    return (false, "File health record kosong.");
                }

                if (file.Length > MaxFileSizeBytes)
                {
                    return (false, "Ukuran file health record maksimal 10 MB.");
                }

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!AllowedExtensions.Contains(extension))
                {
                    return (false, "Format file tidak didukung. Gunakan PDF, JPG, JPEG, PNG, DOC, DOCX, XLS, atau XLSX.");
                }
            }

            return (true, null);
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequirementCodeAsync(
            UserType userType,
            string category,
            string requirementCode)
        {
            var exists = await _dbContext.Set<MstWorkforceRequirement>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.UserType == userType &&
                    x.RequirementCategory == category &&
                    x.RequirementCode == requirementCode &&
                    x.IsActive &&
                    !x.IsDelete);

            return exists
                ? (true, null)
                : (false, $"RequirementCode '{requirementCode}' tidak ditemukan untuk user type ini.");
        }

        private async Task<(string FilePath, string? ContentType)> SaveHealthRecordFileAsync(
            Guid workforceProfileId,
            IFormFile file)
        {
            var rootPath = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
            var relativeFolder = Path.Combine("uploads", "workforce-health-records", workforceProfileId.ToString());
            var physicalFolder = Path.Combine(rootPath, relativeFolder);

            if (!Directory.Exists(physicalFolder))
            {
                Directory.CreateDirectory(physicalFolder);
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var physicalPath = Path.Combine(physicalFolder, fileName);

            await using (var stream = new FileStream(physicalPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var filePath = "/" + Path.Combine(relativeFolder, fileName).Replace(Path.DirectorySeparatorChar, '/');

            return (filePath, file.ContentType);
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

        private static string? NormalizeRequirementCodeOrNull(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim().ToUpperInvariant();
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }
    }
}