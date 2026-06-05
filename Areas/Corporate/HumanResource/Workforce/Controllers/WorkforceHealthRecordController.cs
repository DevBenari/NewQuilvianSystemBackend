using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponseWorkforceHealthRecordPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs.WorkforceHealthRecordResponse>;

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
        private const string CodePrefix = "HLR-RSMMC-";
        private const int CodeNumberLength = 5;
        private const long MaxFileSizeBytes = 10 * 1024 * 1024;

        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
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

        private static readonly HashSet<string> PreviewSupportedContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "application/pdf",
            "image/jpeg",
            "image/png"
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

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceHealthRecordFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Health Record", Description = "Melihat metadata filter health record workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceHealthRecord", "Read")]
        public async Task<IActionResult> GetFilterMetadata(Guid workforceProfileId)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var result = new WorkforceHealthRecordFilterMetadataResponse
            {
                DefaultFilter = new WorkforceHealthRecordDefaultFilterResponse(),
                CustomPeriods = new List<WorkforceHealthRecordCustomPeriodOptionResponse>
                {
                    new() { Value = "today", Label = "Hari ini" },
                    new() { Value = "last7days", Label = "7 hari terakhir" },
                    new() { Value = "thismonth", Label = "Bulan ini" },
                    new() { Value = "lastmonth", Label = "Bulan lalu" }
                },
                SortOptions = new List<WorkforceHealthRecordSortOptionResponse>
                {
                    new() { Value = "recordDate", Label = "Tanggal record" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "requirementCode", Label = "Kode health record" },
                    new() { Value = "healthRecordType", Label = "Tipe health record" },
                    new() { Value = "resultStatus", Label = "Status hasil" },
                    new() { Value = "providerName", Label = "Provider" },
                    new() { Value = "expiredDate", Label = "Tanggal expired" },
                    new() { Value = "isFitToWork", Label = "Fit to work" },
                    new() { Value = "isVerified", Label = "Terverifikasi" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                HealthRecordTypes = BuildHealthRecordTypeOptions(),
                ResultStatuses = BuildResultStatusOptions(),
                FitToWorkOptions = new List<WorkforceHealthRecordBooleanOptionResponse>
                {
                    new() { Value = null, Label = "Semua", Description = "Tampilkan semua status fit to work." },
                    new() { Value = true, Label = "Fit To Work", Description = "Pegawai dinyatakan layak bekerja." },
                    new() { Value = false, Label = "Not Fit To Work", Description = "Pegawai tidak/ belum layak bekerja atau perlu pembatasan kerja." }
                },
                CodeInfo = new WorkforceHealthRecordCodeInfoResponse(),
                FileUploadInfo = new WorkforceHealthRecordFileUploadInfoResponse
                {
                    MaxFileSizeMb = (int)(MaxFileSizeBytes / 1024 / 1024),
                    AllowedExtensions = AllowedExtensions.OrderBy(x => x).ToList(),
                    FormFieldName = "File",
                    ContentType = "multipart/form-data"
                },
                PreviewSupportedContentTypes = PreviewSupportedContentTypes.OrderBy(x => x).ToList(),
                FrontendGuide = new List<string>
                {
                    "RequirementCode tidak perlu dikirim dari frontend karena dibuat otomatis oleh backend.",
                    "HealthRecordType wajib dipilih dari enum agar user tidak mengisi text bebas.",
                    "Gunakan RecordDate untuk tanggal pemeriksaan/kejadian kesehatan.",
                    "ExpiredDate diisi jika record punya masa berlaku, misalnya MCU, vaksin, screening, atau fit to work.",
                    "Jika IsFitToWork = false, FitToWorkRestrictionNote wajib diisi agar HR/K3RS tahu pembatasan kerja.",
                    "File dikirim dengan multipart/form-data melalui field File.",
                    "Untuk preview dokumen, frontend cukup memakai FilePreviewUrl. Untuk download, pakai FileDownloadUrl."
                }
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceHealthRecord.GetFilterMetadata",
                "Mengambil metadata filter health record workforce.",
                new { workforceProfileId, profile.ProfileCode }
            );

            return Ok(ApiResponse<WorkforceHealthRecordFilterMetadataResponse>.Ok(
                result,
                "Metadata filter health record workforce berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceHealthRecordSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Health Record", Description = "Melihat ringkasan health record workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceHealthRecord", "Read")]
        public async Task<IActionResult> GetSummary(Guid workforceProfileId)
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
            var query = BuildBaseQuery(workforceProfileId);

            var result = new WorkforceHealthRecordSummaryResponse
            {
                WorkforceProfileId = profile.Id,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                TotalHealthRecord = await query.CountAsync(),
                ActiveHealthRecord = await query.CountAsync(x => x.IsActive),
                InactiveHealthRecord = await query.CountAsync(x => !x.IsActive),
                VerifiedHealthRecord = await query.CountAsync(x => x.IsVerified),
                UnverifiedHealthRecord = await query.CountAsync(x => !x.IsVerified),
                ExpiredHealthRecord = await query.CountAsync(x => x.ExpiredDate.HasValue && x.ExpiredDate.Value < today),
                CurrentlyValidHealthRecord = await query.CountAsync(x => x.IsActive && x.IsVerified && (!x.ExpiredDate.HasValue || x.ExpiredDate.Value >= today)),
                FitToWorkHealthRecord = await query.CountAsync(x => x.IsFitToWork == true),
                NotFitToWorkHealthRecord = await query.CountAsync(x => x.IsFitToWork == false),
                CompliantForWorkHealthRecord = await query.CountAsync(x => x.IsActive && x.IsVerified && (!x.ExpiredDate.HasValue || x.ExpiredDate.Value >= today) && (!x.IsFitToWork.HasValue || x.IsFitToWork.Value)),
                HealthRecordWithFile = await query.CountAsync(x => x.FilePath != null && x.FilePath != string.Empty),
                HealthRecordWithoutFile = await query.CountAsync(x => x.FilePath == null || x.FilePath == string.Empty)
            };

            return Ok(ApiResponse<WorkforceHealthRecordSummaryResponse>.Ok(
                result,
                "Ringkasan health record workforce berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseWorkforceHealthRecordPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Health Record", Description = "Melihat health record workforce profile", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceHealthRecord", "Read")]
        public async Task<IActionResult> GetHealthRecords(
            Guid workforceProfileId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] HealthRecordType? healthRecordType,
            [FromQuery] HealthRecordResultStatus? resultStatus,
            [FromQuery] bool? isFitToWork,
            [FromQuery] bool? isVerified,
            [FromQuery] bool? isActive,
            [FromQuery] bool? isExpired,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "recordDate",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery(workforceProfileId);
            query = ApplyDateFilter(query, startDate, endDate, customPeriod);
            query = ApplyStandardFilter(query, healthRecordType, resultStatus, isFitToWork, isVerified, isActive, isExpired, search);

            var totalData = await query.CountAsync();

            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = entities.Select(x => MapResponse(x, profile)).ToList();

            var result = new ResponseWorkforceHealthRecordPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseWorkforceHealthRecordPagedResult>.Ok(
                result,
                "Data health record workforce berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceHealthRecordOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Health Record", Description = "Melihat pilihan health record workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceHealthRecord", "Read")]
        public async Task<IActionResult> GetOptions(
            Guid workforceProfileId,
            [FromQuery] HealthRecordType? healthRecordType,
            [FromQuery] HealthRecordResultStatus? resultStatus,
            [FromQuery] bool? isFitToWork,
            [FromQuery] bool? isVerified,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var profileExists = await ProfileExistsAsync(workforceProfileId);

            if (!profileExists)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery(workforceProfileId);
            query = ApplyStandardFilter(query, healthRecordType, resultStatus, isFitToWork, isVerified, onlyActive ? true : null, null, search);

            var totalData = await query.CountAsync();
            var today = DateTime.UtcNow.Date;

            var items = await query
                .OrderByDescending(x => x.IsVerified)
                .ThenByDescending(x => x.RecordDate)
                .ThenBy(x => x.HealthRecordType)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new WorkforceHealthRecordOptionResponse
                {
                    Id = x.Id,
                    RequirementCode = x.RequirementCode,
                    HealthRecordType = x.HealthRecordType,
                    RecordDate = x.RecordDate,
                    ResultStatus = x.ResultStatus,
                    ProviderName = x.ProviderName,
                    ExpiredDate = x.ExpiredDate,
                    IsFitToWork = x.IsFitToWork,
                    IsVerified = x.IsVerified,
                    IsExpired = x.ExpiredDate.HasValue && x.ExpiredDate.Value < today,
                    IsCurrentlyValid = x.IsActive && x.IsVerified && (!x.ExpiredDate.HasValue || x.ExpiredDate.Value >= today),
                    IsCompliantForWork = x.IsActive && x.IsVerified && (!x.ExpiredDate.HasValue || x.ExpiredDate.Value >= today) && (!x.IsFitToWork.HasValue || x.IsFitToWork.Value),
                    HasFile = x.FilePath != null && x.FilePath != string.Empty
                })
                .ToListAsync();

            var result = new WorkforceHealthRecordOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<WorkforceHealthRecordOptionPagedResponse>.Ok(
                result,
                "Data pilihan health record workforce berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceHealthRecordDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Health Record", Description = "Melihat detail health record workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceHealthRecord", "Read")]
        public async Task<IActionResult> GetHealthRecordById(Guid workforceProfileId, Guid id)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var entity = await BuildBaseQuery(workforceProfileId).FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Health record workforce tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<WorkforceHealthRecordDetailResponse>.Ok(
                MapDetailResponse(entity, profile),
                "Detail health record workforce berhasil diambil."
            ));
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceHealthRecordDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Workforce Health Record", Description = "Menambah health record workforce profile", AccessType = AccessTypes.Create, SortOrder = 2)]
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

            var validation = await ValidateRequestAsync(workforceProfileId, null, request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data health record tidak valid."
                ));
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
                RequirementCode = await GenerateHealthRecordCodeAsync(),
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

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceHealthRecord.CreateHealthRecord",
                "Health record workforce berhasil dibuat.",
                new { workforceProfileId, entity.Id, entity.RequirementCode, entity.HealthRecordType }
            );

            return Ok(ApiResponse<WorkforceHealthRecordDetailResponse>.Ok(
                MapDetailResponse(entity, profile),
                "Health record workforce berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceHealthRecordDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Workforce Health Record", Description = "Mengubah health record workforce profile", AccessType = AccessTypes.Update, SortOrder = 3)]
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
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

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

            var validation = await ValidateRequestAsync(workforceProfileId, id, request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data health record tidak valid."
                ));
            }

            if (request.ReplaceExistingFile && request.File == null)
            {
                DeletePhysicalFileIfExists(entity.FilePath);
                entity.FilePath = null;
                entity.FileContentType = null;
            }

            if (request.File != null && request.ReplaceExistingFile)
            {
                DeletePhysicalFileIfExists(entity.FilePath);

                var savedFile = await SaveHealthRecordFileAsync(workforceProfileId, request.File);
                entity.FilePath = savedFile.FilePath;
                entity.FileContentType = savedFile.ContentType;
            }
            else if (request.File != null && string.IsNullOrWhiteSpace(entity.FilePath))
            {
                var savedFile = await SaveHealthRecordFileAsync(workforceProfileId, request.File);
                entity.FilePath = savedFile.FilePath;
                entity.FileContentType = savedFile.ContentType;
            }

            entity.HealthRecordType = request.HealthRecordType;
            entity.RecordDate = request.RecordDate.Date;
            entity.ResultStatus = request.ResultStatus;
            entity.ProviderName = NormalizeNullableText(request.ProviderName);
            entity.ExpiredDate = request.ExpiredDate?.Date;
            entity.IsFitToWork = request.IsFitToWork;
            entity.FitToWorkRestrictionNote = NormalizeNullableText(request.FitToWorkRestrictionNote);
            entity.Notes = NormalizeNullableText(request.Notes);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<WorkforceHealthRecordDetailResponse>.Ok(
                MapDetailResponse(entity, profile),
                "Health record workforce berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Workforce Health Record", Description = "Mengubah status health record workforce", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("WorkforceHealthRecord", "Update")]
        public async Task<IActionResult> UpdateHealthRecordStatus(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWorkforceHealthRecordStatusRequest request)
        {
            var entity = await _dbContext.Set<WfpHealthRecord>()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

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

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status health record workforce berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/verify")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceHealthRecordDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Workforce Health Record", Description = "Verifikasi health record workforce", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("WorkforceHealthRecord", "Update")]
        public async Task<IActionResult> VerifyHealthRecord(
            Guid workforceProfileId,
            Guid id,
            [FromBody] VerifyWorkforceHealthRecordRequest request)
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
                .Include(x => x.VerifiedByUser)
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

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

            return Ok(ApiResponse<WorkforceHealthRecordDetailResponse>.Ok(
                MapDetailResponse(entity, profile),
                "Health record workforce berhasil diverifikasi."
            ));
        }

        [HttpPatch("{id:guid}/unverify")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceHealthRecordDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Workforce Health Record", Description = "Membatalkan verifikasi health record workforce", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("WorkforceHealthRecord", "Update")]
        public async Task<IActionResult> UnverifyHealthRecord(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UnverifyWorkforceHealthRecordRequest request)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            if (string.IsNullOrWhiteSpace(request.UnverifyReason))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Alasan unverify wajib diisi."
                ));
            }

            var entity = await _dbContext.Set<WfpHealthRecord>()
                .Include(x => x.VerifiedByUser)
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

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

            return Ok(ApiResponse<WorkforceHealthRecordDetailResponse>.Ok(
                MapDetailResponse(entity, profile),
                "Verifikasi health record workforce berhasil dibatalkan."
            ));
        }

        [HttpGet("{id:guid}/preview")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Health Record", Description = "Preview file health record workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceHealthRecord", "Read")]
        public async Task<IActionResult> PreviewHealthRecordFile(Guid workforceProfileId, Guid id)
        {
            var file = await GetHealthRecordFileAsync(workforceProfileId, id);

            if (!file.IsValid)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    file.ErrorMessage ?? "File health record tidak ditemukan."
                ));
            }

            Response.Headers[HeaderNames.ContentDisposition] = new ContentDispositionHeaderValue("inline")
            {
                FileNameStar = file.FileName
            }.ToString();

            var bytes = await System.IO.File.ReadAllBytesAsync(file.PhysicalPath!);
            return File(bytes, file.ContentType ?? "application/octet-stream");
        }

        [HttpGet("{id:guid}/download")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Health Record", Description = "Download file health record workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceHealthRecord", "Read")]
        public async Task<IActionResult> DownloadHealthRecordFile(Guid workforceProfileId, Guid id)
        {
            var file = await GetHealthRecordFileAsync(workforceProfileId, id);

            if (!file.IsValid)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    file.ErrorMessage ?? "File health record tidak ditemukan."
                ));
            }

            var bytes = await System.IO.File.ReadAllBytesAsync(file.PhysicalPath!);
            return File(bytes, file.ContentType ?? "application/octet-stream", file.DownloadName ?? file.FileName);
        }

        [HttpDelete("{id:guid}/file")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Workforce Health Record File", Description = "Menghapus file health record workforce", AccessType = AccessTypes.Delete, SortOrder = 7)]
        [AccessPermission("WorkforceHealthRecord", "Delete")]
        public async Task<IActionResult> DeleteHealthRecordFile(
            Guid workforceProfileId,
            Guid id,
            [FromBody] DeleteWorkforceHealthRecordFileRequest? request = null)
        {
            var entity = await _dbContext.Set<WfpHealthRecord>()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Health record workforce tidak ditemukan."
                ));
            }

            if (string.IsNullOrWhiteSpace(entity.FilePath))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Health record belum memiliki file."
                ));
            }

            if (request?.DeletePhysicalFile ?? true)
            {
                DeletePhysicalFileIfExists(entity.FilePath);
            }

            entity.FilePath = null;
            entity.FileContentType = null;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "File health record workforce berhasil dihapus."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Workforce Health Record", Description = "Menghapus health record workforce", AccessType = AccessTypes.Delete, SortOrder = 8)]
        [AccessPermission("WorkforceHealthRecord", "Delete")]
        public async Task<IActionResult> DeleteHealthRecord(Guid workforceProfileId, Guid id)
        {
            var entity = await _dbContext.Set<WfpHealthRecord>()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

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

        private IQueryable<WfpHealthRecord> BuildBaseQuery(Guid workforceProfileId)
        {
            return _dbContext.Set<WfpHealthRecord>()
                .AsNoTracking()
                .Include(x => x.VerifiedByUser)
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);
        }

        private static IQueryable<WfpHealthRecord> ApplyDateFilter(
            IQueryable<WfpHealthRecord> query,
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            var today = DateTime.UtcNow.Date;
            DateTime? effectiveStartDate = startDate?.Date;
            DateTime? effectiveEndDate = endDate?.Date;

            if (!string.IsNullOrWhiteSpace(customPeriod) &&
                !string.Equals(customPeriod, "custom", StringComparison.OrdinalIgnoreCase))
            {
                switch (customPeriod.Trim().ToLowerInvariant())
                {
                    case "today":
                        effectiveStartDate = today;
                        effectiveEndDate = today;
                        break;
                    case "last7days":
                        effectiveStartDate = today.AddDays(-6);
                        effectiveEndDate = today;
                        break;
                    case "thismonth":
                        effectiveStartDate = new DateTime(today.Year, today.Month, 1);
                        effectiveEndDate = effectiveStartDate.Value.AddMonths(1).AddDays(-1);
                        break;
                    case "lastmonth":
                        var firstDayThisMonth = new DateTime(today.Year, today.Month, 1);
                        effectiveStartDate = firstDayThisMonth.AddMonths(-1);
                        effectiveEndDate = firstDayThisMonth.AddDays(-1);
                        break;
                }
            }

            if (effectiveStartDate.HasValue)
            {
                query = query.Where(x => x.RecordDate >= effectiveStartDate.Value);
            }

            if (effectiveEndDate.HasValue)
            {
                var endExclusive = effectiveEndDate.Value.AddDays(1);
                query = query.Where(x => x.RecordDate < endExclusive);
            }

            return query;
        }

        private static IQueryable<WfpHealthRecord> ApplyStandardFilter(
            IQueryable<WfpHealthRecord> query,
            HealthRecordType? healthRecordType,
            HealthRecordResultStatus? resultStatus,
            bool? isFitToWork,
            bool? isVerified,
            bool? isActive,
            bool? isExpired,
            string? search)
        {
            var today = DateTime.UtcNow.Date;

            if (healthRecordType.HasValue)
            {
                query = query.Where(x => x.HealthRecordType == healthRecordType.Value);
            }

            if (resultStatus.HasValue)
            {
                query = query.Where(x => x.ResultStatus == resultStatus.Value);
            }

            if (isFitToWork.HasValue)
            {
                query = query.Where(x => x.IsFitToWork == isFitToWork.Value);
            }

            if (isVerified.HasValue)
            {
                query = query.Where(x => x.IsVerified == isVerified.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            if (isExpired.HasValue)
            {
                query = isExpired.Value
                    ? query.Where(x => x.ExpiredDate.HasValue && x.ExpiredDate.Value < today)
                    : query.Where(x => !x.ExpiredDate.HasValue || x.ExpiredDate.Value >= today);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    (x.RequirementCode != null && x.RequirementCode.ToLower().Contains(keyword)) ||
                    (x.ProviderName != null && x.ProviderName.ToLower().Contains(keyword)) ||
                    (x.FitToWorkRestrictionNote != null && x.FitToWorkRestrictionNote.ToLower().Contains(keyword)) ||
                    (x.Notes != null && x.Notes.ToLower().Contains(keyword)) ||
                    (x.VerificationNote != null && x.VerificationNote.ToLower().Contains(keyword))
                );
            }

            return query;
        }

        private static IOrderedQueryable<WfpHealthRecord> ApplySorting(
            IQueryable<WfpHealthRecord> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            var normalizedSortBy = string.IsNullOrWhiteSpace(sortBy)
                ? "recorddate"
                : sortBy.Trim().ToLowerInvariant();

            return normalizedSortBy switch
            {
                "createdatetime" or "created" => isDesc
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),
                "requirementcode" or "code" => isDesc
                    ? query.OrderByDescending(x => x.RequirementCode)
                    : query.OrderBy(x => x.RequirementCode),
                "healthrecordtype" or "type" => isDesc
                    ? query.OrderByDescending(x => x.HealthRecordType).ThenByDescending(x => x.RecordDate)
                    : query.OrderBy(x => x.HealthRecordType).ThenByDescending(x => x.RecordDate),
                "resultstatus" or "status" => isDesc
                    ? query.OrderByDescending(x => x.ResultStatus).ThenByDescending(x => x.RecordDate)
                    : query.OrderBy(x => x.ResultStatus).ThenByDescending(x => x.RecordDate),
                "providername" or "provider" => isDesc
                    ? query.OrderByDescending(x => x.ProviderName).ThenByDescending(x => x.RecordDate)
                    : query.OrderBy(x => x.ProviderName).ThenByDescending(x => x.RecordDate),
                "expireddate" or "expired" => isDesc
                    ? query.OrderByDescending(x => x.ExpiredDate).ThenByDescending(x => x.RecordDate)
                    : query.OrderBy(x => x.ExpiredDate).ThenByDescending(x => x.RecordDate),
                "isfittowork" or "fittowork" => isDesc
                    ? query.OrderByDescending(x => x.IsFitToWork).ThenByDescending(x => x.RecordDate)
                    : query.OrderBy(x => x.IsFitToWork).ThenByDescending(x => x.RecordDate),
                "isverified" => isDesc
                    ? query.OrderByDescending(x => x.IsVerified).ThenByDescending(x => x.RecordDate)
                    : query.OrderBy(x => x.IsVerified).ThenByDescending(x => x.RecordDate),
                "isactive" => isDesc
                    ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.RecordDate)
                    : query.OrderBy(x => x.IsActive).ThenByDescending(x => x.RecordDate),
                _ => isDesc
                    ? query.OrderByDescending(x => x.RecordDate).ThenByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.RecordDate).ThenBy(x => x.CreateDateTime)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid workforceProfileId,
            Guid? excludeId,
            CreateWorkforceHealthRecordRequest request)
        {
            if (request.HealthRecordType == HealthRecordType.Unknown)
            {
                return (false, "HealthRecordType wajib dipilih dan tidak boleh Unknown.");
            }

            if (!Enum.IsDefined(typeof(HealthRecordType), request.HealthRecordType))
            {
                return (false, "HealthRecordType tidak valid.");
            }

            if (!Enum.IsDefined(typeof(HealthRecordResultStatus), request.ResultStatus))
            {
                return (false, "ResultStatus tidak valid.");
            }

            if (request.RecordDate == default)
            {
                return (false, "RecordDate wajib diisi.");
            }

            if (request.ExpiredDate.HasValue && request.RecordDate.Date > request.ExpiredDate.Value.Date)
            {
                return (false, "RecordDate tidak boleh lebih besar dari ExpiredDate.");
            }

            if (request.IsFitToWork == false && string.IsNullOrWhiteSpace(request.FitToWorkRestrictionNote))
            {
                return (false, "FitToWorkRestrictionNote wajib diisi jika IsFitToWork = false.");
            }

            if (request.File != null)
            {
                var fileValidation = ValidateFile(request.File);

                if (!fileValidation.IsValid)
                {
                    return fileValidation;
                }
            }

            var normalizedProviderName = NormalizeNullableText(request.ProviderName);

            var duplicate = await _dbContext.Set<WfpHealthRecord>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id != excludeId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    x.HealthRecordType == request.HealthRecordType &&
                    x.RecordDate == request.RecordDate.Date &&
                    x.ProviderName == normalizedProviderName &&
                    !x.IsDelete);

            if (duplicate)
            {
                return (false, "Health record dengan tipe, tanggal, dan provider yang sama sudah terdaftar pada workforce profile ini.");
            }

            return (true, null);
        }

        private static (bool IsValid, string? ErrorMessage) ValidateFile(IFormFile file)
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

            return (true, null);
        }

        private async Task<string> GenerateHealthRecordCodeAsync()
        {
            var codes = await _dbContext.Set<WfpHealthRecord>()
                .AsNoTracking()
                .Where(x => x.RequirementCode != null && x.RequirementCode.StartsWith(CodePrefix))
                .Select(x => x.RequirementCode!)
                .ToListAsync();

            var maxNumber = 0;

            foreach (var code in codes)
            {
                var numberText = code[CodePrefix.Length..];

                if (int.TryParse(numberText, out var number) && number > maxNumber)
                {
                    maxNumber = number;
                }
            }

            return $"{CodePrefix}{(maxNumber + 1).ToString().PadLeft(CodeNumberLength, '0')}";
        }

        private async Task<(string FilePath, string? ContentType)> SaveHealthRecordFileAsync(
            Guid workforceProfileId,
            IFormFile file)
        {
            var storage = GetFileStoragePaths();
            var relativeFolder = Path.Combine("uploads", "workforce-health-records", workforceProfileId.ToString());
            var physicalFolder = Path.Combine(storage.RootPath, relativeFolder);

            Directory.CreateDirectory(physicalFolder);

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

        private async Task<FileResolveResult> GetHealthRecordFileAsync(Guid workforceProfileId, Guid id)
        {
            var record = await _dbContext.Set<WfpHealthRecord>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            if (record == null)
            {
                return FileResolveResult.Fail("Health record workforce tidak ditemukan.");
            }

            if (string.IsNullOrWhiteSpace(record.FilePath))
            {
                return FileResolveResult.Fail("Health record belum memiliki file.");
            }

            var physicalPath = ResolvePhysicalPath(record.FilePath);

            if (!System.IO.File.Exists(physicalPath))
            {
                return FileResolveResult.Fail("File health record tidak ditemukan di server.");
            }

            var provider = new FileExtensionContentTypeProvider();

            if (!provider.TryGetContentType(physicalPath, out var contentType))
            {
                contentType = record.FileContentType ?? "application/octet-stream";
            }

            var fileName = Path.GetFileName(physicalPath);
            var downloadName = $"{SanitizeFileName(record.HealthRecordType.ToString())}_{record.RecordDate:yyyyMMdd}{Path.GetExtension(physicalPath)}";

            return FileResolveResult.Ok(physicalPath, contentType, fileName, downloadName);
        }

        private void DeletePhysicalFileIfExists(string? filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return;
            }

            var physicalPath = ResolvePhysicalPath(filePath);

            if (System.IO.File.Exists(physicalPath))
            {
                System.IO.File.Delete(physicalPath);
            }
        }

        private string ResolvePhysicalPath(string filePath)
        {
            var normalizedPath = filePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var storage = GetFileStoragePaths();

            return Path.Combine(storage.RootPath, normalizedPath);
        }

        private (string RootPath, string PublicRequestPath) GetFileStoragePaths()
        {
            var rootPath = _environment.WebRootPath;

            if (string.IsNullOrWhiteSpace(rootPath))
            {
                rootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
            }

            return (rootPath, "/uploads");
        }

        private WorkforceHealthRecordResponse MapResponse(WfpHealthRecord entity, MstWorkforceProfile profile)
        {
            var today = DateTime.UtcNow.Date;
            var hasFile = !string.IsNullOrWhiteSpace(entity.FilePath);
            var isExpired = entity.ExpiredDate.HasValue && entity.ExpiredDate.Value.Date < today;
            var isCurrentlyValid = entity.IsActive && entity.IsVerified && !isExpired;
            var isCompliantForWork = isCurrentlyValid && (!entity.IsFitToWork.HasValue || entity.IsFitToWork.Value);

            return new WorkforceHealthRecordResponse
            {
                Id = entity.Id,
                WorkforceProfileId = entity.WorkforceProfileId,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                RequirementCode = entity.RequirementCode,
                HealthRecordType = entity.HealthRecordType,
                RecordDate = entity.RecordDate,
                ResultStatus = entity.ResultStatus,
                ProviderName = entity.ProviderName,
                ExpiredDate = entity.ExpiredDate,
                IsFitToWork = entity.IsFitToWork,
                FitToWorkRestrictionNote = entity.FitToWorkRestrictionNote,
                IsVerified = entity.IsVerified,
                VerifiedByUserId = entity.VerifiedByUserId,
                VerifiedByUserName = entity.VerifiedByUser?.DisplayName,
                VerifiedAt = entity.VerifiedAt,
                VerificationNote = entity.VerificationNote,
                FilePath = entity.FilePath,
                FileContentType = entity.FileContentType,
                FileName = hasFile ? Path.GetFileName(entity.FilePath) : null,
                FilePreviewUrl = hasFile ? BuildEndpointUrl(entity.WorkforceProfileId, entity.Id, "preview") : null,
                FileDownloadUrl = hasFile ? BuildEndpointUrl(entity.WorkforceProfileId, entity.Id, "download") : null,
                HasFile = hasFile,
                Notes = entity.Notes,
                IsExpired = isExpired,
                IsCurrentlyValid = isCurrentlyValid,
                IsCompliantForWork = isCompliantForWork,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime
            };
        }

        private WorkforceHealthRecordDetailResponse MapDetailResponse(WfpHealthRecord entity, MstWorkforceProfile profile)
        {
            var baseResponse = MapResponse(entity, profile);

            return new WorkforceHealthRecordDetailResponse
            {
                Id = baseResponse.Id,
                WorkforceProfileId = baseResponse.WorkforceProfileId,
                ProfileCode = baseResponse.ProfileCode,
                DisplayName = baseResponse.DisplayName,
                RequirementCode = baseResponse.RequirementCode,
                HealthRecordType = baseResponse.HealthRecordType,
                RecordDate = baseResponse.RecordDate,
                ResultStatus = baseResponse.ResultStatus,
                ProviderName = baseResponse.ProviderName,
                ExpiredDate = baseResponse.ExpiredDate,
                IsFitToWork = baseResponse.IsFitToWork,
                FitToWorkRestrictionNote = baseResponse.FitToWorkRestrictionNote,
                IsVerified = baseResponse.IsVerified,
                VerifiedByUserId = baseResponse.VerifiedByUserId,
                VerifiedByUserName = baseResponse.VerifiedByUserName,
                VerifiedAt = baseResponse.VerifiedAt,
                VerificationNote = baseResponse.VerificationNote,
                FilePath = baseResponse.FilePath,
                FileContentType = baseResponse.FileContentType,
                FileName = baseResponse.FileName,
                FilePreviewUrl = baseResponse.FilePreviewUrl,
                FileDownloadUrl = baseResponse.FileDownloadUrl,
                HasFile = baseResponse.HasFile,
                Notes = baseResponse.Notes,
                IsExpired = baseResponse.IsExpired,
                IsCurrentlyValid = baseResponse.IsCurrentlyValid,
                IsCompliantForWork = baseResponse.IsCompliantForWork,
                IsActive = baseResponse.IsActive,
                CreateDateTime = baseResponse.CreateDateTime,
                UpdateDateTime = entity.UpdateDateTime,
                CreateBy = entity.CreateBy,
                UpdateBy = entity.UpdateBy
            };
        }

        private string BuildEndpointUrl(Guid workforceProfileId, Guid id, string action)
        {
            return CombineUrlPath(
                Request.PathBase.Value,
                "api/v1/corporate/human-resource/workforce-profiles",
                workforceProfileId.ToString(),
                "health-records",
                id.ToString(),
                action
            );
        }

        private async Task<MstWorkforceProfile?> GetProfileAsync(Guid workforceProfileId)
        {
            return await _dbContext.Set<MstWorkforceProfile>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == workforceProfileId && !x.IsDelete);
        }

        private async Task<bool> ProfileExistsAsync(Guid workforceProfileId)
        {
            return await _dbContext.Set<MstWorkforceProfile>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == workforceProfileId && !x.IsDelete);
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0)
            {
                pageNumber = 1;
            }

            if (pageSize <= 0)
            {
                pageSize = 25;
            }

            if (pageSize > 100)
            {
                pageSize = 100;
            }

            return (pageNumber, pageSize);
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

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private static string SanitizeFileName(string value)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = new string(value.Select(ch => invalidChars.Contains(ch) ? '_' : ch).ToArray());

            return string.IsNullOrWhiteSpace(sanitized)
                ? "health-record"
                : sanitized;
        }

        private static string CombineUrlPath(params string?[] parts)
        {
            return "/" + string.Join(
                "/",
                parts
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x!.Trim('/'))
            );
        }

        private static List<WorkforceHealthRecordEnumOptionResponse> BuildHealthRecordTypeOptions()
        {
            return Enum.GetValues(typeof(HealthRecordType))
                .Cast<HealthRecordType>()
                .Where(x => !string.Equals(x.ToString(), "Unknown", StringComparison.OrdinalIgnoreCase))
                .Select(x => new WorkforceHealthRecordEnumOptionResponse
                {
                    Value = Convert.ToInt32(x),
                    Name = x.ToString(),
                    Label = ToDisplayLabel(x.ToString()),
                    Description = ResolveHealthRecordTypeDescription(x.ToString())
                })
                .ToList();
        }

        private static List<WorkforceHealthRecordEnumOptionResponse> BuildResultStatusOptions()
        {
            return Enum.GetValues(typeof(HealthRecordResultStatus))
                .Cast<HealthRecordResultStatus>()
                .Where(x => !string.Equals(x.ToString(), "Unknown", StringComparison.OrdinalIgnoreCase))
                .Select(x => new WorkforceHealthRecordEnumOptionResponse
                {
                    Value = Convert.ToInt32(x),
                    Name = x.ToString(),
                    Label = ToDisplayLabel(x.ToString()),
                    Description = ResolveResultStatusDescription(x.ToString())
                })
                .ToList();
        }

        private static string ToDisplayLabel(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var result = value.Replace("_", " ");

            for (var i = result.Length - 1; i > 0; i--)
            {
                if (char.IsUpper(result[i]) && !char.IsWhiteSpace(result[i - 1]) && !char.IsUpper(result[i - 1]))
                {
                    result = result.Insert(i, " ");
                }
            }

            return result;
        }

        private static string ResolveHealthRecordTypeDescription(string value)
        {
            return value switch
            {
                "MCU" => "Medical Check Up berkala atau prakerja.",
                "Vaccination" => "Data vaksinasi pegawai.",
                "HepatitisScreening" => "Screening hepatitis untuk tenaga kesehatan atau risiko kerja tertentu.",
                "TbScreening" => "Screening TB/TBC untuk tenaga kesehatan atau risiko kerja tertentu.",
                "FitToWork" => "Pernyataan layak kerja atau pembatasan kerja.",
                "OccupationalInjury" => "Cedera/kecelakaan akibat kerja.",
                "ExposureIncident" => "Insiden paparan kerja, misalnya needle stick injury atau paparan cairan tubuh.",
                _ => "Jenis health record workforce."
            };
        }

        private static string ResolveResultStatusDescription(string value)
        {
            return value switch
            {
                "Normal" => "Hasil pemeriksaan normal.",
                "Abnormal" => "Hasil pemeriksaan abnormal.",
                "Passed" => "Hasil dinyatakan lulus/memenuhi.",
                "Failed" => "Hasil dinyatakan tidak lulus/tidak memenuhi.",
                "Fit" => "Dinyatakan fit/layak.",
                "Unfit" => "Dinyatakan tidak fit/tidak layak.",
                "ConditionalFit" => "Dinyatakan fit dengan catatan atau pembatasan.",
                _ => "Status hasil health record."
            };
        }

        private sealed class FileResolveResult
        {
            public bool IsValid { get; private set; }
            public string? ErrorMessage { get; private set; }
            public string? PhysicalPath { get; private set; }
            public string? ContentType { get; private set; }
            public string? FileName { get; private set; }
            public string? DownloadName { get; private set; }

            public static FileResolveResult Ok(string physicalPath, string? contentType, string fileName, string downloadName)
            {
                return new FileResolveResult
                {
                    IsValid = true,
                    PhysicalPath = physicalPath,
                    ContentType = contentType,
                    FileName = fileName,
                    DownloadName = downloadName
                };
            }

            public static FileResolveResult Fail(string errorMessage)
            {
                return new FileResolveResult
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
        }
    }
}
