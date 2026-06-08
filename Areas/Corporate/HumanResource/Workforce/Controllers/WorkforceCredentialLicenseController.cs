using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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

using ResponseWorkforceCredentialLicensePagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs.WorkforceCredentialLicenseResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/credential-licenses")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFORCE",
        moduleName: "Human Resource Workforce",
        displayName: "Workforce Credential License",
        AreaName = "Corporate",
        ControllerName = "WorkforceCredentialLicense",
        Description = "Workforce credential license management",
        SortOrder = 27
    )]
    [Tags("Corporate / Human Resource / Workforce / Credential License")]
    public class WorkforceCredentialLicenseController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.Workforce.CredentialLicense";
        private const string CodePrefix = "CRL-RSMMC-";
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

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;

        public WorkforceCredentialLicenseController(
            ApplicationDbContext dbContext,
            LoggerService loggerService,
            IWebHostEnvironment environment,
            IConfiguration configuration)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
            _environment = environment;
            _configuration = configuration;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceCredentialLicenseFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Credential License", Description = "Melihat metadata filter credential license workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceCredentialLicense", "Read")]
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

            var result = new WorkforceCredentialLicenseFilterMetadataResponse
            {
                DefaultFilter = new WorkforceCredentialLicenseDefaultFilterResponse(),
                CustomPeriods = BuildCustomPeriodOptions(),
                SortOptions = new List<WorkforceCredentialLicenseSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "requirementCode", Label = "Kode credential license" },
                    new() { Value = "licenseType", Label = "Tipe license" },
                    new() { Value = "licenseNumber", Label = "Nomor license" },
                    new() { Value = "issuer", Label = "Penerbit" },
                    new() { Value = "issueDate", Label = "Tanggal terbit" },
                    new() { Value = "expiredDate", Label = "Tanggal expired" },
                    new() { Value = "verificationStatus", Label = "Status verifikasi" },
                    new() { Value = "isPrimary", Label = "Primary" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                LicenseTypeOptions = BuildLicenseTypeOptions(),
                VerificationStatusOptions = BuildVerificationStatusOptions(),
                FrontendGuide = new List<string>
                {
                    "LicenseType adalah enum/dropdown, bukan text bebas. Pilih STR, SIP, SIK, SIPP, SIPA, SIPB, atau Other.",
                    "RequirementCode tidak dikirim dari frontend. Backend membuat otomatis dengan format CRL-RSMMC-00001.",
                    "LicenseNumber adalah nomor asli dokumen STR/SIP/SIK dan wajib diisi.",
                    "IssueDate dan ExpiredDate gunakan format date-time, contoh 2024-03-20T00:00:00Z.",
                    "Setelah create, status awal menjadi PendingVerification. Gunakan endpoint verify/reject/revoke untuk lifecycle verifikasi.",
                    "File PDF dan image dapat dipreview melalui FilePreviewUrl. DOC/DOCX/XLS/XLSX lebih aman dibuka dengan download."
                }
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceCredentialLicense.GetFilterMetadata",
                "Mengambil metadata filter credential license workforce.",
                new { workforceProfileId, profile.ProfileCode }
            );

            return Ok(ApiResponse<WorkforceCredentialLicenseFilterMetadataResponse>.Ok(
                result,
                "Metadata filter credential license workforce berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceCredentialLicenseSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Credential License Summary", Description = "Melihat ringkasan credential license workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceCredentialLicense", "Read")]
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

            var result = new WorkforceCredentialLicenseSummaryResponse
            {
                WorkforceProfileId = profile.Id,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                TotalCredentialLicense = await query.CountAsync(),
                ActiveCredentialLicense = await query.CountAsync(x => x.IsActive),
                InactiveCredentialLicense = await query.CountAsync(x => !x.IsActive),
                VerifiedCredentialLicense = await query.CountAsync(x => x.IsVerified && x.VerificationStatus == CredentialVerificationStatus.Verified && x.ExpiredDate.Date >= today),
                PendingVerificationCredentialLicense = await query.CountAsync(x => x.VerificationStatus == CredentialVerificationStatus.PendingVerification && x.ExpiredDate.Date >= today),
                RejectedCredentialLicense = await query.CountAsync(x => x.VerificationStatus == CredentialVerificationStatus.Rejected),
                RevokedCredentialLicense = await query.CountAsync(x => x.VerificationStatus == CredentialVerificationStatus.Revoked),
                ExpiredCredentialLicense = await query.CountAsync(x => x.ExpiredDate.Date < today),
                CurrentlyValidCredentialLicense = await query.CountAsync(x => x.IsActive && x.IsVerified && x.VerificationStatus == CredentialVerificationStatus.Verified && x.ExpiredDate.Date >= today),
                PrimaryCredentialLicense = await query.CountAsync(x => x.IsPrimary),
                CredentialLicenseWithFile = await query.CountAsync(x => x.FilePath != null && x.FilePath != string.Empty),
                CredentialLicenseWithoutFile = await query.CountAsync(x => x.FilePath == null || x.FilePath == string.Empty)
            };

            return Ok(ApiResponse<WorkforceCredentialLicenseSummaryResponse>.Ok(
                result,
                "Ringkasan credential license workforce berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseWorkforceCredentialLicensePagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Credential License", Description = "Melihat credential license workforce profile", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceCredentialLicense", "Read")]
        public async Task<IActionResult> GetCredentialLicenses(
            Guid workforceProfileId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] WorkforceCredentialLicenseType? licenseType,
            [FromQuery] CredentialVerificationStatus? verificationStatus,
            [FromQuery] bool? isPrimary,
            [FromQuery] bool? isVerified,
            [FromQuery] bool? isActive,
            [FromQuery] bool? isExpired,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "createDateTime",
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

            var dateRange = ResolveDateRange(startDate, endDate, customPeriod);

            if (!dateRange.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    dateRange.ErrorMessage ?? "Filter tanggal tidak valid."
                ));
            }

            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery(workforceProfileId);
            query = ApplyDateFilter(query, dateRange);
            query = ApplyStandardFilter(query, licenseType, verificationStatus, isPrimary, isVerified, isActive, isExpired, search);

            var totalData = await query.CountAsync();

            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(x => x.VerifiedByUser)
                .Include(x => x.RejectedByUser)
                .Include(x => x.RevokedByUser)
                .ToListAsync();

            var actorNames = await GetActorNameMapAsync(
                entities
                    .Select(x => x.CreateBy)
                    .Where(x => x != Guid.Empty)
            );

            var items = entities
                .Select(x => MapResponse(x, profile, actorNames))
                .ToList();

            var result = new ResponseWorkforceCredentialLicensePagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseWorkforceCredentialLicensePagedResult>.Ok(
                result,
                "Data credential license workforce berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceCredentialLicenseOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Credential License Options", Description = "Melihat pilihan credential license workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceCredentialLicense", "Read")]
        public async Task<IActionResult> GetOptions(
            Guid workforceProfileId,
            [FromQuery] WorkforceCredentialLicenseType? licenseType,
            [FromQuery] CredentialVerificationStatus? verificationStatus,
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
            query = ApplyStandardFilter(
                query,
                licenseType,
                verificationStatus,
                isPrimary: null,
                isVerified: null,
                isActive: onlyActive ? true : null,
                isExpired: null,
                search: search
            );

            var today = DateTime.UtcNow.Date;

            var totalData = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.IsPrimary)
                .ThenBy(x => x.ExpiredDate)
                .ThenBy(x => x.LicenseType)
                .ThenBy(x => x.LicenseNumber)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new WorkforceCredentialLicenseOptionResponse
                {
                    Id = x.Id,
                    RequirementCode = x.RequirementCode,
                    LicenseType = x.LicenseType,
                    LicenseNumber = x.LicenseNumber,
                    Issuer = x.Issuer,
                    IssueDate = x.IssueDate,
                    ExpiredDate = x.ExpiredDate,
                    PracticeLocation = x.PracticeLocation,
                    VerificationStatus = x.ExpiredDate.Date < today
                        ? CredentialVerificationStatus.Expired
                        : x.VerificationStatus,
                    IsVerified = x.IsVerified,
                    IsPrimary = x.IsPrimary,
                    IsExpired = x.ExpiredDate.Date < today,
                    IsCurrentlyValid = x.IsActive && x.IsVerified && x.VerificationStatus == CredentialVerificationStatus.Verified && x.ExpiredDate.Date >= today,
                    HasFile = x.FilePath != null && x.FilePath != string.Empty
                })
                .ToListAsync();

            var result = new WorkforceCredentialLicenseOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<WorkforceCredentialLicenseOptionPagedResponse>.Ok(
                result,
                "Data pilihan credential license workforce berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceCredentialLicenseDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Credential License Detail", Description = "Melihat detail credential license workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceCredentialLicense", "Read")]
        public async Task<IActionResult> GetCredentialLicenseById(Guid workforceProfileId, Guid id)
        {
            var detail = await BuildDetailResponseAsync(workforceProfileId, id);

            if (detail == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Credential license workforce tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<WorkforceCredentialLicenseDetailResponse>.Ok(
                detail,
                "Detail credential license workforce berhasil diambil."
            ));
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceCredentialLicenseDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Workforce Credential License", Description = "Menambah credential license workforce profile", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("WorkforceCredentialLicense", "Create")]
        public async Task<IActionResult> CreateCredentialLicense(
            Guid workforceProfileId,
            [FromForm] CreateWorkforceCredentialLicenseRequest request)
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
                    validation.ErrorMessage ?? "Data credential license tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var licenseTypeText = request.LicenseType.ToString();

            string? filePath = null;
            string? fileContentType = null;

            if (request.File != null)
            {
                var savedFile = await SaveCredentialLicenseFileAsync(workforceProfileId, request.File);
                filePath = savedFile.FilePath;
                fileContentType = savedFile.ContentType;
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var entity = new WfpCredentialLicense
                {
                    Id = Guid.NewGuid(),
                    WorkforceProfileId = workforceProfileId,
                    RequirementCode = await GenerateCredentialLicenseCodeAsync(),
                    LicenseType = licenseTypeText,
                    LicenseNumber = NormalizeRequiredText(request.LicenseNumber),
                    Issuer = NormalizeNullableText(request.Issuer),
                    IssueDate = request.IssueDate.Date,
                    ExpiredDate = request.ExpiredDate.Date,
                    PracticeLocation = NormalizeNullableText(request.PracticeLocation),
                    FilePath = filePath,
                    FileContentType = fileContentType,
                    VerificationStatus = CredentialVerificationStatus.PendingVerification,
                    IsVerified = false,
                    VerifiedByUserId = null,
                    VerifiedAt = null,
                    VerificationNote = null,
                    RejectedByUserId = null,
                    RejectedAt = null,
                    RejectedReason = null,
                    RevokedByUserId = null,
                    RevokedAt = null,
                    RevokedReason = null,
                    IsPrimary = request.IsPrimary,
                    IsActive = request.IsActive,
                    Description = NormalizeNullableText(request.Description),
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                if (entity.IsPrimary)
                {
                    await DeactivateOtherPrimaryAsync(workforceProfileId, licenseTypeText, null, now, actorUserId);
                }

                _dbContext.Set<WfpCredentialLicense>().Add(entity);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                await _loggerService.InfoAsync(
                    LogCategory,
                    "WorkforceCredentialLicense.CreateCredentialLicense",
                    "Credential license workforce berhasil dibuat.",
                    new { workforceProfileId, entity.Id, entity.RequirementCode, entity.LicenseType, entity.LicenseNumber }
                );

                var detail = await BuildDetailResponseAsync(workforceProfileId, entity.Id);

                return Ok(ApiResponse<WorkforceCredentialLicenseDetailResponse>.Ok(
                    detail!,
                    "Credential license workforce berhasil dibuat."
                ));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                DeletePhysicalFileIfExists(filePath);

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "WorkforceCredentialLicense.CreateCredentialLicense",
                    "Gagal membuat credential license workforce.",
                    ex,
                    new { workforceProfileId, request.LicenseType, request.LicenseNumber }
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat membuat credential license workforce."
                    )
                );
            }
        }

        [HttpPut("{id:guid}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceCredentialLicenseDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Workforce Credential License", Description = "Mengubah credential license workforce profile", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceCredentialLicense", "Update")]
        public async Task<IActionResult> UpdateCredentialLicense(
            Guid workforceProfileId,
            Guid id,
            [FromForm] UpdateWorkforceCredentialLicenseRequest request)
        {
            var entity = await _dbContext.Set<WfpCredentialLicense>()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Credential license workforce tidak ditemukan."
                ));
            }

            if (entity.IsVerified || entity.VerificationStatus == CredentialVerificationStatus.Verified)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Credential license yang sudah verified tidak boleh diubah. Gunakan revoke jika license tidak berlaku lagi."
                ));
            }

            if (entity.VerificationStatus == CredentialVerificationStatus.Revoked)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Credential license revoked tidak boleh diubah."
                ));
            }

            var validation = await ValidateRequestAsync(workforceProfileId, id, request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data credential license tidak valid."
                ));
            }

            if (request.File != null && !request.ReplaceExistingFile && !string.IsNullOrWhiteSpace(entity.FilePath))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Credential license sudah memiliki file. Kirim ReplaceExistingFile = true untuk mengganti file."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var licenseTypeText = request.LicenseType.ToString();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                if (request.ReplaceExistingFile)
                {
                    DeletePhysicalFileIfExists(entity.FilePath);
                    entity.FilePath = null;
                    entity.FileContentType = null;
                }

                if (request.File != null)
                {
                    var savedFile = await SaveCredentialLicenseFileAsync(workforceProfileId, request.File);
                    entity.FilePath = savedFile.FilePath;
                    entity.FileContentType = savedFile.ContentType;
                }

                if (request.IsPrimary)
                {
                    await DeactivateOtherPrimaryAsync(workforceProfileId, licenseTypeText, id, now, actorUserId);
                }

                entity.LicenseType = licenseTypeText;
                entity.LicenseNumber = NormalizeRequiredText(request.LicenseNumber);
                entity.Issuer = NormalizeNullableText(request.Issuer);
                entity.IssueDate = request.IssueDate.Date;
                entity.ExpiredDate = request.ExpiredDate.Date;
                entity.PracticeLocation = NormalizeNullableText(request.PracticeLocation);
                entity.IsPrimary = request.IsPrimary;
                entity.IsActive = request.IsActive;
                entity.Description = NormalizeNullableText(request.Description);
                entity.UpdateDateTime = now;
                entity.UpdateBy = actorUserId;

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                var detail = await BuildDetailResponseAsync(workforceProfileId, entity.Id);

                return Ok(ApiResponse<WorkforceCredentialLicenseDetailResponse>.Ok(
                    detail!,
                    "Credential license workforce berhasil diperbarui."
                ));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "WorkforceCredentialLicense.UpdateCredentialLicense",
                    "Gagal mengubah credential license workforce.",
                    ex,
                    new { workforceProfileId, id, request.LicenseType, request.LicenseNumber }
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat mengubah credential license workforce."
                    )
                );
            }
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceCredentialLicenseDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Workforce Credential License Status", Description = "Mengubah status aktif credential license workforce", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("WorkforceCredentialLicense", "Update")]
        public async Task<IActionResult> UpdateCredentialLicenseStatus(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWorkforceCredentialLicenseStatusRequest request)
        {
            var entity = await _dbContext.Set<WfpCredentialLicense>()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Credential license workforce tidak ditemukan."
                ));
            }

            if (request.IsActive && entity.VerificationStatus == CredentialVerificationStatus.Revoked)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Credential license revoked tidak boleh diaktifkan ulang."
                ));
            }

            entity.IsActive = request.IsActive;

            if (!request.IsActive)
            {
                entity.IsPrimary = false;
            }

            entity.Description = NormalizeNullableText(request.Description) ?? entity.Description;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            var detail = await BuildDetailResponseAsync(workforceProfileId, entity.Id);

            return Ok(ApiResponse<WorkforceCredentialLicenseDetailResponse>.Ok(
                detail!,
                "Status credential license workforce berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/primary")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceCredentialLicenseDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Set Workforce Credential License Primary", Description = "Menetapkan credential license primary", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("WorkforceCredentialLicense", "Update")]
        public async Task<IActionResult> SetPrimaryCredentialLicense(
            Guid workforceProfileId,
            Guid id,
            [FromBody] SetWorkforceCredentialLicensePrimaryRequest request)
        {
            if (!request.IsPrimary)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "IsPrimary harus bernilai true."
                ));
            }

            var entity = await _dbContext.Set<WfpCredentialLicense>()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Credential license workforce tidak ditemukan."
                ));
            }

            if (!entity.IsActive)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Credential license nonaktif tidak bisa dijadikan primary."
                ));
            }

            if (entity.ExpiredDate.Date < DateTime.UtcNow.Date)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Credential license expired tidak bisa dijadikan primary."
                ));
            }

            if (entity.VerificationStatus == CredentialVerificationStatus.Rejected ||
                entity.VerificationStatus == CredentialVerificationStatus.Revoked)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Credential license rejected/revoked tidak bisa dijadikan primary."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                await DeactivateOtherPrimaryAsync(workforceProfileId, entity.LicenseType, entity.Id, now, actorUserId);

                entity.IsPrimary = true;
                entity.UpdateDateTime = now;
                entity.UpdateBy = actorUserId;

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                var detail = await BuildDetailResponseAsync(workforceProfileId, entity.Id);

                return Ok(ApiResponse<WorkforceCredentialLicenseDetailResponse>.Ok(
                    detail!,
                    "Credential license primary berhasil diperbarui."
                ));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "WorkforceCredentialLicense.SetPrimaryCredentialLicense",
                    "Gagal menetapkan credential license primary.",
                    ex,
                    new { workforceProfileId, id }
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat menetapkan credential license primary."
                    )
                );
            }
        }

        [HttpPatch("{id:guid}/verify")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceCredentialLicenseDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Verify Workforce Credential License", Description = "Verifikasi credential license workforce", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("WorkforceCredentialLicense", "Update")]
        public async Task<IActionResult> VerifyCredentialLicense(
            Guid workforceProfileId,
            Guid id,
            [FromBody] VerifyWorkforceCredentialLicenseRequest request)
        {
            var entity = await _dbContext.Set<WfpCredentialLicense>()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Credential license workforce tidak ditemukan."
                ));
            }

            if (!entity.IsActive)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Credential license nonaktif tidak bisa diverifikasi."
                ));
            }

            if (entity.ExpiredDate.Date < DateTime.UtcNow.Date)
            {
                entity.IsVerified = false;
                entity.IsPrimary = false;
                entity.VerificationStatus = CredentialVerificationStatus.Expired;
                entity.UpdateDateTime = DateTime.UtcNow;
                entity.UpdateBy = GetCurrentUserId();
                await _dbContext.SaveChangesAsync();

                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Credential license sudah expired dan tidak bisa diverifikasi."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsVerified = true;
            entity.VerificationStatus = CredentialVerificationStatus.Verified;
            entity.VerifiedByUserId = actorUserId;
            entity.VerifiedAt = now;
            entity.VerificationNote = NormalizeNullableText(request.VerificationNote);
            entity.RejectedByUserId = null;
            entity.RejectedAt = null;
            entity.RejectedReason = null;
            entity.RevokedByUserId = null;
            entity.RevokedAt = null;
            entity.RevokedReason = null;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var detail = await BuildDetailResponseAsync(workforceProfileId, entity.Id);

            return Ok(ApiResponse<WorkforceCredentialLicenseDetailResponse>.Ok(
                detail!,
                "Credential license workforce berhasil diverifikasi."
            ));
        }

        [HttpPatch("{id:guid}/reject")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceCredentialLicenseDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Reject Workforce Credential License", Description = "Reject verifikasi credential license workforce", AccessType = AccessTypes.Update, SortOrder = 7)]
        [AccessPermission("WorkforceCredentialLicense", "Update")]
        public async Task<IActionResult> RejectCredentialLicense(
            Guid workforceProfileId,
            Guid id,
            [FromBody] RejectWorkforceCredentialLicenseRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RejectedReason))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Alasan reject wajib diisi."
                ));
            }

            var entity = await _dbContext.Set<WfpCredentialLicense>()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Credential license workforce tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsVerified = false;
            entity.IsPrimary = false;
            entity.VerificationStatus = CredentialVerificationStatus.Rejected;
            entity.RejectedByUserId = actorUserId;
            entity.RejectedAt = now;
            entity.RejectedReason = request.RejectedReason.Trim();
            entity.VerifiedByUserId = null;
            entity.VerifiedAt = null;
            entity.VerificationNote = null;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var detail = await BuildDetailResponseAsync(workforceProfileId, entity.Id);

            return Ok(ApiResponse<WorkforceCredentialLicenseDetailResponse>.Ok(
                detail!,
                "Credential license workforce berhasil di-reject."
            ));
        }

        [HttpPatch("{id:guid}/revoke")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceCredentialLicenseDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Revoke Workforce Credential License", Description = "Revoke credential license workforce", AccessType = AccessTypes.Update, SortOrder = 8)]
        [AccessPermission("WorkforceCredentialLicense", "Update")]
        public async Task<IActionResult> RevokeCredentialLicense(
            Guid workforceProfileId,
            Guid id,
            [FromBody] RevokeWorkforceCredentialLicenseRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RevokedReason))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Alasan revoke wajib diisi."
                ));
            }

            var entity = await _dbContext.Set<WfpCredentialLicense>()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Credential license workforce tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsVerified = false;
            entity.IsActive = false;
            entity.IsPrimary = false;
            entity.VerificationStatus = CredentialVerificationStatus.Revoked;
            entity.RevokedByUserId = actorUserId;
            entity.RevokedAt = now;
            entity.RevokedReason = request.RevokedReason.Trim();
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var detail = await BuildDetailResponseAsync(workforceProfileId, entity.Id);

            return Ok(ApiResponse<WorkforceCredentialLicenseDetailResponse>.Ok(
                detail!,
                "Credential license workforce berhasil di-revoke."
            ));
        }

        [HttpGet("{id:guid}/preview")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Credential License File", Description = "Preview file credential license workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceCredentialLicense", "Read")]
        public async Task<IActionResult> PreviewCredentialLicense(Guid workforceProfileId, Guid id)
        {
            var fileValidation = await GetCredentialLicenseFileAsync(workforceProfileId, id);

            if (!fileValidation.IsValid)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    fileValidation.ErrorMessage ?? "File credential license workforce tidak ditemukan."
                ));
            }

            Response.Headers[HeaderNames.ContentDisposition] = new ContentDispositionHeaderValue("inline")
            {
                FileNameStar = fileValidation.FileName
            }.ToString();

            var stream = new FileStream(
                fileValidation.PhysicalPath!,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read
            );

            return File(stream, fileValidation.ContentType!, enableRangeProcessing: true);
        }

        [HttpGet("{id:guid}/download")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Credential License File", Description = "Download file credential license workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceCredentialLicense", "Read")]
        public async Task<IActionResult> DownloadCredentialLicense(Guid workforceProfileId, Guid id)
        {
            var fileValidation = await GetCredentialLicenseFileAsync(workforceProfileId, id);

            if (!fileValidation.IsValid)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    fileValidation.ErrorMessage ?? "File credential license workforce tidak ditemukan."
                ));
            }

            var stream = new FileStream(
                fileValidation.PhysicalPath!,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read
            );

            return File(
                stream,
                fileValidation.ContentType!,
                fileValidation.DownloadName,
                enableRangeProcessing: true
            );
        }

        [HttpDelete("{id:guid}/file")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Workforce Credential License File", Description = "Menghapus file credential license workforce", AccessType = AccessTypes.Delete, SortOrder = 9)]
        [AccessPermission("WorkforceCredentialLicense", "Delete")]
        public async Task<IActionResult> DeleteCredentialLicenseFile(
            Guid workforceProfileId,
            Guid id,
            [FromBody] DeleteWorkforceCredentialLicenseFileRequest? request = null)
        {
            var entity = await _dbContext.Set<WfpCredentialLicense>()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Credential license workforce tidak ditemukan."
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
                "File credential license workforce berhasil dihapus."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Workforce Credential License", Description = "Menghapus credential license workforce", AccessType = AccessTypes.Delete, SortOrder = 10)]
        [AccessPermission("WorkforceCredentialLicense", "Delete")]
        public async Task<IActionResult> DeleteCredentialLicense(Guid workforceProfileId, Guid id)
        {
            var entity = await _dbContext.Set<WfpCredentialLicense>()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Credential license workforce tidak ditemukan."
                ));
            }

            if (entity.IsVerified || entity.VerificationStatus == CredentialVerificationStatus.Verified)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Credential license verified tidak boleh dihapus. Gunakan revoke agar audit trail tetap aman."
                ));
            }

            entity.IsActive = false;
            entity.IsPrimary = false;
            entity.IsDelete = true;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = GetCurrentUserId();
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Credential license workforce berhasil dihapus."
            ));
        }

        private IQueryable<WfpCredentialLicense> BuildBaseQuery(Guid workforceProfileId)
        {
            return _dbContext.Set<WfpCredentialLicense>()
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);
        }

        private static IQueryable<WfpCredentialLicense> ApplyDateFilter(
            IQueryable<WfpCredentialLicense> query,
            DateRangeResolveResult dateRange)
        {
            if (dateRange.Start.HasValue)
            {
                query = query.Where(x => x.CreateDateTime >= dateRange.Start.Value);
            }

            if (dateRange.EndExclusive.HasValue)
            {
                query = query.Where(x => x.CreateDateTime < dateRange.EndExclusive.Value);
            }

            return query;
        }

        private static IQueryable<WfpCredentialLicense> ApplyStandardFilter(
            IQueryable<WfpCredentialLicense> query,
            WorkforceCredentialLicenseType? licenseType,
            CredentialVerificationStatus? verificationStatus,
            bool? isPrimary,
            bool? isVerified,
            bool? isActive,
            bool? isExpired,
            string? search)
        {
            var today = DateTime.UtcNow.Date;

            if (licenseType.HasValue)
            {
                var selectedLicenseType = licenseType.Value.ToString();
                query = query.Where(x => x.LicenseType == selectedLicenseType);
            }

            if (verificationStatus.HasValue)
            {
                if (verificationStatus.Value == CredentialVerificationStatus.Expired)
                {
                    query = query.Where(x => x.ExpiredDate.Date < today);
                }
                else
                {
                    query = query.Where(x => x.VerificationStatus == verificationStatus.Value);
                }
            }

            if (isPrimary.HasValue)
            {
                query = query.Where(x => x.IsPrimary == isPrimary.Value);
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
                    ? query.Where(x => x.ExpiredDate.Date < today)
                    : query.Where(x => x.ExpiredDate.Date >= today);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    (x.RequirementCode != null && x.RequirementCode.ToLower().Contains(keyword)) ||
                    x.LicenseType.ToLower().Contains(keyword) ||
                    x.LicenseNumber.ToLower().Contains(keyword) ||
                    (x.Issuer != null && x.Issuer.ToLower().Contains(keyword)) ||
                    (x.PracticeLocation != null && x.PracticeLocation.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<WfpCredentialLicense> ApplySorting(
            IQueryable<WfpCredentialLicense> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "createDateTime").Trim().ToLowerInvariant() switch
            {
                "requirementcode" => isDescending ? query.OrderByDescending(x => x.RequirementCode) : query.OrderBy(x => x.RequirementCode),
                "licensetype" => isDescending ? query.OrderByDescending(x => x.LicenseType).ThenBy(x => x.LicenseNumber) : query.OrderBy(x => x.LicenseType).ThenBy(x => x.LicenseNumber),
                "licensenumber" => isDescending ? query.OrderByDescending(x => x.LicenseNumber) : query.OrderBy(x => x.LicenseNumber),
                "issuer" => isDescending ? query.OrderByDescending(x => x.Issuer).ThenBy(x => x.LicenseNumber) : query.OrderBy(x => x.Issuer).ThenBy(x => x.LicenseNumber),
                "issuedate" => isDescending ? query.OrderByDescending(x => x.IssueDate).ThenBy(x => x.LicenseType) : query.OrderBy(x => x.IssueDate).ThenBy(x => x.LicenseType),
                "expireddate" => isDescending ? query.OrderByDescending(x => x.ExpiredDate).ThenBy(x => x.LicenseType) : query.OrderBy(x => x.ExpiredDate).ThenBy(x => x.LicenseType),
                "verificationstatus" => isDescending ? query.OrderByDescending(x => x.VerificationStatus).ThenBy(x => x.ExpiredDate) : query.OrderBy(x => x.VerificationStatus).ThenBy(x => x.ExpiredDate),
                "isprimary" => isDescending ? query.OrderByDescending(x => x.IsPrimary).ThenBy(x => x.LicenseType) : query.OrderBy(x => x.IsPrimary).ThenBy(x => x.LicenseType),
                "isactive" => isDescending ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.LicenseType) : query.OrderBy(x => x.IsActive).ThenBy(x => x.LicenseType),
                _ => isDescending ? query.OrderByDescending(x => x.CreateDateTime).ThenBy(x => x.LicenseType) : query.OrderBy(x => x.CreateDateTime).ThenBy(x => x.LicenseType)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid workforceProfileId,
            Guid? excludeId,
            CreateWorkforceCredentialLicenseRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.LicenseNumber))
            {
                return (false, "LicenseNumber wajib diisi.");
            }

            if (request.IssueDate == default)
            {
                return (false, "IssueDate wajib diisi.");
            }

            if (request.ExpiredDate == default)
            {
                return (false, "ExpiredDate wajib diisi.");
            }

            if (request.IssueDate.Date > request.ExpiredDate.Date)
            {
                return (false, "IssueDate tidak boleh lebih besar dari ExpiredDate.");
            }

            if (request.File != null)
            {
                var fileValidation = ValidateFile(request.File);

                if (!fileValidation.IsValid)
                {
                    return fileValidation;
                }
            }

            var licenseTypeText = request.LicenseType.ToString();
            var licenseNumber = NormalizeRequiredText(request.LicenseNumber);

            var duplicateQuery = _dbContext.Set<WfpCredentialLicense>()
                .AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.LicenseType == licenseTypeText &&
                    x.LicenseNumber == licenseNumber &&
                    !x.IsDelete);

            if (excludeId.HasValue)
            {
                duplicateQuery = duplicateQuery.Where(x => x.Id != excludeId.Value);
            }

            if (await duplicateQuery.AnyAsync())
            {
                return (false, "LicenseType dan LicenseNumber sudah terdaftar pada workforce profile ini.");
            }

            return (true, null);
        }

        private static (bool IsValid, string? ErrorMessage) ValidateFile(IFormFile file)
        {
            if (file.Length <= 0)
            {
                return (false, "File credential license kosong.");
            }

            if (file.Length > MaxFileSizeBytes)
            {
                return (false, "Ukuran file credential license maksimal 10 MB.");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!AllowedExtensions.Contains(extension))
            {
                return (false, "Format file tidak didukung. Gunakan PDF, JPG, JPEG, PNG, DOC, DOCX, XLS, atau XLSX.");
            }

            return (true, null);
        }

        private async Task<string> GenerateCredentialLicenseCodeAsync()
        {
            var existingCodes = await _dbContext.Set<WfpCredentialLicense>()
                .AsNoTracking()
                .Where(x => x.RequirementCode != null && x.RequirementCode.StartsWith(CodePrefix))
                .Select(x => x.RequirementCode!)
                .ToListAsync();

            var usedNumbers = existingCodes
                .Select(x => x.Replace(CodePrefix, string.Empty))
                .Where(x => int.TryParse(x, out _))
                .Select(int.Parse)
                .Where(x => x > 0)
                .ToHashSet();

            var nextNumber = 1;

            while (usedNumbers.Contains(nextNumber))
            {
                nextNumber++;
            }

            return CodePrefix + nextNumber.ToString().PadLeft(CodeNumberLength, '0');
        }

        private async Task<(string FilePath, string? ContentType)> SaveCredentialLicenseFileAsync(
            Guid workforceProfileId,
            IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var storage = GetFileStoragePaths();
            var relativeFolder = Path.Combine("workforce-credential-licenses", workforceProfileId.ToString());
            var absoluteFolder = Path.Combine(storage.RootPath, relativeFolder);

            Directory.CreateDirectory(absoluteFolder);

            var absolutePath = Path.Combine(absoluteFolder, fileName);

            await using (var stream = new FileStream(absolutePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var publicPath = CombineUrlPath(storage.PublicRequestPath, relativeFolder.Replace("\\", "/"), fileName);

            return (publicPath, file.ContentType);
        }

        private async Task<FileResolveResult> GetCredentialLicenseFileAsync(Guid workforceProfileId, Guid id)
        {
            var license = await _dbContext.Set<WfpCredentialLicense>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            if (license == null)
            {
                return FileResolveResult.Invalid("Credential license workforce tidak ditemukan.");
            }

            if (string.IsNullOrWhiteSpace(license.FilePath))
            {
                return FileResolveResult.Invalid("File credential license workforce belum tersedia.");
            }

            var physicalPath = ResolvePhysicalPath(license.FilePath);

            if (!System.IO.File.Exists(physicalPath))
            {
                return FileResolveResult.Invalid("File fisik credential license tidak ditemukan di server.");
            }

            var provider = new FileExtensionContentTypeProvider();

            if (!provider.TryGetContentType(physicalPath, out var contentType))
            {
                contentType = license.FileContentType ?? "application/octet-stream";
            }

            var extension = Path.GetExtension(physicalPath);
            var safeLicenseType = SanitizeFileName(license.LicenseType);
            var safeLicenseNumber = SanitizeFileName(license.LicenseNumber);
            var fileName = Path.GetFileName(physicalPath);
            var downloadName = $"{license.RequirementCode ?? "CREDENTIAL_LICENSE"}_{safeLicenseType}_{safeLicenseNumber}{extension}";

            return FileResolveResult.Valid(physicalPath, contentType, fileName, downloadName);
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
            var storage = GetFileStoragePaths();
            var normalizedFilePath = filePath.Replace("\\", "/").Trim();
            var publicPrefix = storage.PublicRequestPath.TrimEnd('/');

            string relativePath;

            if (normalizedFilePath.StartsWith(publicPrefix + "/", StringComparison.OrdinalIgnoreCase))
            {
                relativePath = normalizedFilePath[(publicPrefix.Length + 1)..];
            }
            else
            {
                relativePath = normalizedFilePath.TrimStart('/');
            }

            var fullPath = Path.GetFullPath(Path.Combine(storage.RootPath, relativePath.Replace("/", Path.DirectorySeparatorChar.ToString())));
            var rootPath = Path.GetFullPath(storage.RootPath);

            if (!fullPath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Path file tidak valid.");
            }

            return fullPath;
        }

        private (string RootPath, string PublicRequestPath) GetFileStoragePaths()
        {
            var publicRequestPath = _configuration["FileStorage:PublicRequestPath"] ?? "/uploads";

            if (!publicRequestPath.StartsWith('/'))
            {
                publicRequestPath = "/" + publicRequestPath;
            }

            publicRequestPath = publicRequestPath.TrimEnd('/');

            var configuredRoot = _configuration["FileStorage:UploadRootPath"];

            if (!string.IsNullOrWhiteSpace(configuredRoot))
            {
                Directory.CreateDirectory(configuredRoot);
                return (configuredRoot, publicRequestPath);
            }

            var webRootPath = _environment.WebRootPath;

            if (string.IsNullOrWhiteSpace(webRootPath))
            {
                webRootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
            }

            var rootPath = Path.Combine(webRootPath, publicRequestPath.TrimStart('/'));
            Directory.CreateDirectory(rootPath);

            return (rootPath, publicRequestPath);
        }

        private WorkforceCredentialLicenseResponse MapResponse(
            WfpCredentialLicense entity,
            MstWorkforceProfile profile,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            var today = DateTime.UtcNow.Date;
            var hasFile = !string.IsNullOrWhiteSpace(entity.FilePath);
            var runtimeStatus = ResolveRuntimeStatus(entity.VerificationStatus, entity.ExpiredDate, entity.IsVerified);

            return new WorkforceCredentialLicenseResponse
            {
                Id = entity.Id,
                WorkforceProfileId = entity.WorkforceProfileId,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                RequirementCode = entity.RequirementCode,
                LicenseType = entity.LicenseType,
                LicenseNumber = entity.LicenseNumber,
                Issuer = entity.Issuer,
                IssueDate = entity.IssueDate,
                ExpiredDate = entity.ExpiredDate,
                PracticeLocation = entity.PracticeLocation,
                HasFile = hasFile,
                FilePath = entity.FilePath,
                FileContentType = entity.FileContentType,
                FileName = hasFile ? Path.GetFileName(entity.FilePath) : null,
                FilePreviewUrl = hasFile ? BuildEndpointUrl(entity.WorkforceProfileId, entity.Id, "preview") : null,
                FileDownloadUrl = hasFile ? BuildEndpointUrl(entity.WorkforceProfileId, entity.Id, "download") : null,
                VerificationStatus = runtimeStatus,
                IsVerified = entity.IsVerified,
                VerifiedByUserId = entity.VerifiedByUserId,
                VerifiedByUserName = entity.VerifiedByUser?.DisplayName,
                VerifiedAt = entity.VerifiedAt,
                VerificationNote = entity.VerificationNote,
                RejectedByUserId = entity.RejectedByUserId,
                RejectedByUserName = entity.RejectedByUser?.DisplayName,
                RejectedAt = entity.RejectedAt,
                RejectedReason = entity.RejectedReason,
                RevokedByUserId = entity.RevokedByUserId,
                RevokedByUserName = entity.RevokedByUser?.DisplayName,
                RevokedAt = entity.RevokedAt,
                RevokedReason = entity.RevokedReason,
                IsPrimary = entity.IsPrimary,
                IsExpired = entity.ExpiredDate.Date < today,
                IsCurrentlyValid = entity.IsActive &&
                    entity.IsVerified &&
                    runtimeStatus == CredentialVerificationStatus.Verified &&
                    entity.ExpiredDate.Date >= today,
                IsActive = entity.IsActive,
                Description = entity.Description,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy)
            };
        }

        private WorkforceCredentialLicenseDetailResponse MapDetailResponse(
            WfpCredentialLicense entity,
            MstWorkforceProfile profile,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            var response = MapResponse(entity, profile, actorNames);

            return new WorkforceCredentialLicenseDetailResponse
            {
                Id = response.Id,
                WorkforceProfileId = response.WorkforceProfileId,
                ProfileCode = response.ProfileCode,
                DisplayName = response.DisplayName,
                RequirementCode = response.RequirementCode,
                LicenseType = response.LicenseType,
                LicenseNumber = response.LicenseNumber,
                Issuer = response.Issuer,
                IssueDate = response.IssueDate,
                ExpiredDate = response.ExpiredDate,
                PracticeLocation = response.PracticeLocation,
                HasFile = response.HasFile,
                FilePath = response.FilePath,
                FileContentType = response.FileContentType,
                FileName = response.FileName,
                FilePreviewUrl = response.FilePreviewUrl,
                FileDownloadUrl = response.FileDownloadUrl,
                VerificationStatus = response.VerificationStatus,
                IsVerified = response.IsVerified,
                VerifiedByUserId = response.VerifiedByUserId,
                VerifiedByUserName = response.VerifiedByUserName,
                VerifiedAt = response.VerifiedAt,
                VerificationNote = response.VerificationNote,
                RejectedByUserId = response.RejectedByUserId,
                RejectedByUserName = response.RejectedByUserName,
                RejectedAt = response.RejectedAt,
                RejectedReason = response.RejectedReason,
                RevokedByUserId = response.RevokedByUserId,
                RevokedByUserName = response.RevokedByUserName,
                RevokedAt = response.RevokedAt,
                RevokedReason = response.RevokedReason,
                IsPrimary = response.IsPrimary,
                IsExpired = response.IsExpired,
                IsCurrentlyValid = response.IsCurrentlyValid,
                IsActive = response.IsActive,
                Description = response.Description,
                CreateDateTime = response.CreateDateTime,
                CreateBy = response.CreateBy,
                CreateByName = response.CreateByName,
                UpdateDateTime = entity.UpdateDateTime.HasValue && entity.UpdateDateTime.Value == DateTime.MinValue
                    ? null
                    : entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };
        }

        private async Task<WorkforceCredentialLicenseDetailResponse?> BuildDetailResponseAsync(Guid workforceProfileId, Guid id)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return null;
            }

            var entity = await _dbContext.Set<WfpCredentialLicense>()
                .AsNoTracking()
                .Include(x => x.VerifiedByUser)
                .Include(x => x.RejectedByUser)
                .Include(x => x.RevokedByUser)
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            if (entity == null)
            {
                return null;
            }

            var actorNames = await GetActorNameMapAsync(new[]
            {
                entity.CreateBy,
                entity.UpdateBy
            });

            return MapDetailResponse(entity, profile, actorNames);
        }

        private string BuildEndpointUrl(Guid workforceProfileId, Guid id, string action)
        {
            var pathBase = Request.PathBase.HasValue ? Request.PathBase.Value : string.Empty;
            var path = $"{pathBase}/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/credential-licenses/{id}/{action}";

            return $"{Request.Scheme}://{Request.Host}{path}";
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

        private async Task<Dictionary<Guid, string?>> GetActorNameMapAsync(IEnumerable<Guid> actorIds)
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

        private async Task DeactivateOtherPrimaryAsync(
            Guid workforceProfileId,
            string licenseType,
            Guid? excludeId,
            DateTime now,
            Guid actorUserId)
        {
            var otherPrimaries = await _dbContext.Set<WfpCredentialLicense>()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.LicenseType == licenseType &&
                    (!excludeId.HasValue || x.Id != excludeId.Value) &&
                    x.IsPrimary &&
                    !x.IsDelete)
                .ToListAsync();

            foreach (var item in otherPrimaries)
            {
                item.IsPrimary = false;
                item.UpdateDateTime = now;
                item.UpdateBy = actorUserId;
            }
        }

        private static CredentialVerificationStatus ResolveRuntimeStatus(
            CredentialVerificationStatus status,
            DateTime expiredDate,
            bool isVerified)
        {
            if (status == CredentialVerificationStatus.Revoked ||
                status == CredentialVerificationStatus.Rejected)
            {
                return status;
            }

            if (expiredDate.Date < DateTime.UtcNow.Date)
            {
                return CredentialVerificationStatus.Expired;
            }

            return isVerified ? CredentialVerificationStatus.Verified : status;
        }

        private static List<WorkforceCredentialLicenseCustomPeriodOptionResponse> BuildCustomPeriodOptions()
        {
            return new List<WorkforceCredentialLicenseCustomPeriodOptionResponse>
            {
                new() { Value = "custom", Label = "Custom Date Range", Description = "Frontend mengirim startDate dan/atau endDate manual." },
                new() { Value = "today", Label = "Hari ini", Description = "Data yang dibuat hari ini berdasarkan UTC." },
                new() { Value = "last7days", Label = "7 hari terakhir", Description = "Data yang dibuat dalam 7 hari terakhir termasuk hari ini." },
                new() { Value = "last30days", Label = "30 hari terakhir", Description = "Data yang dibuat dalam 30 hari terakhir termasuk hari ini." },
                new() { Value = "thismonth", Label = "Bulan ini", Description = "Data yang dibuat pada bulan berjalan." },
                new() { Value = "lastmonth", Label = "Bulan lalu", Description = "Data yang dibuat pada bulan sebelumnya." }
            };
        }

        private static List<WorkforceCredentialLicenseTypeOptionResponse> BuildLicenseTypeOptions()
        {
            return new List<WorkforceCredentialLicenseTypeOptionResponse>
            {
                new()
                {
                    Value = WorkforceCredentialLicenseType.STR,
                    Code = "STR",
                    Label = "STR",
                    Description = "Surat Tanda Registrasi. Umumnya untuk tenaga medis dan tenaga kesehatan.",
                    CommonFor = new List<string> { "Dokter", "Dokter Gigi", "Perawat", "Bidan", "Apoteker", "Tenaga Kesehatan" }
                },
                new()
                {
                    Value = WorkforceCredentialLicenseType.SIP,
                    Code = "SIP",
                    Label = "SIP",
                    Description = "Surat Izin Praktik. Umumnya untuk dokter/dokter gigi dan tenaga medis tertentu.",
                    CommonFor = new List<string> { "Dokter", "Dokter Gigi", "Dokter Spesialis" }
                },
                new()
                {
                    Value = WorkforceCredentialLicenseType.SIK,
                    Code = "SIK",
                    Label = "SIK",
                    Description = "Surat Izin Kerja. Dapat digunakan untuk tenaga kesehatan sesuai kebijakan regulasi/rumah sakit.",
                    CommonFor = new List<string> { "Tenaga Kesehatan", "Tenaga Penunjang Medis" }
                },
                new()
                {
                    Value = WorkforceCredentialLicenseType.SIPP,
                    Code = "SIPP",
                    Label = "SIPP",
                    Description = "Surat Izin Praktik Perawat.",
                    CommonFor = new List<string> { "Perawat" }
                },
                new()
                {
                    Value = WorkforceCredentialLicenseType.SIPA,
                    Code = "SIPA",
                    Label = "SIPA",
                    Description = "Surat Izin Praktik Apoteker.",
                    CommonFor = new List<string> { "Apoteker" }
                },
                new()
                {
                    Value = WorkforceCredentialLicenseType.SIPB,
                    Code = "SIPB",
                    Label = "SIPB",
                    Description = "Surat Izin Praktik Bidan.",
                    CommonFor = new List<string> { "Bidan" }
                },
                new()
                {
                    Value = WorkforceCredentialLicenseType.Other,
                    Code = "Other",
                    Label = "Other",
                    Description = "Jenis credential/license lain yang belum masuk kategori standar.",
                    CommonFor = new List<string> { "Lainnya" }
                }
            };
        }

        private static List<WorkforceCredentialLicenseVerificationStatusOptionResponse> BuildVerificationStatusOptions()
        {
            return new List<WorkforceCredentialLicenseVerificationStatusOptionResponse>
            {
                new() { Value = CredentialVerificationStatus.Unverified, Label = "Unverified", Description = "Data belum diverifikasi." },
                new() { Value = CredentialVerificationStatus.PendingVerification, Label = "Pending Verification", Description = "Data sedang menunggu proses verifikasi." },
                new() { Value = CredentialVerificationStatus.Verified, Label = "Verified", Description = "Data sudah diverifikasi dan masih berlaku." },
                new() { Value = CredentialVerificationStatus.Rejected, Label = "Rejected", Description = "Data ditolak saat verifikasi." },
                new() { Value = CredentialVerificationStatus.Revoked, Label = "Revoked", Description = "Credential/license dicabut atau tidak berlaku lagi." },
                new() { Value = CredentialVerificationStatus.Expired, Label = "Expired", Description = "Credential/license sudah melewati ExpiredDate." }
            };
        }

        private static DateRangeResolveResult ResolveDateRange(
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            var period = customPeriod?.Trim().ToLowerInvariant();
            var today = DateTime.UtcNow.Date;

            DateTime? start = null;
            DateTime? endExclusive = null;

            switch (period)
            {
                case null:
                case "":
                case "custom":
                    if (startDate.HasValue)
                    {
                        start = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
                    }

                    if (endDate.HasValue)
                    {
                        endExclusive = DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc);
                    }
                    break;

                case "today":
                    start = today;
                    endExclusive = today.AddDays(1);
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
                    start = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                    endExclusive = start.Value.AddMonths(1);
                    break;

                case "lastmonth":
                    var thisMonth = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                    start = thisMonth.AddMonths(-1);
                    endExclusive = thisMonth;
                    break;

                default:
                    return DateRangeResolveResult.Invalid(
                        $"customPeriod '{customPeriod}' tidak valid. Gunakan endpoint filters/metadata untuk melihat daftar customPeriod yang tersedia."
                    );
            }

            if (start.HasValue && endExclusive.HasValue && start.Value >= endExclusive.Value)
            {
                return DateRangeResolveResult.Invalid("startDate tidak boleh lebih besar atau sama dengan endDate.");
            }

            return DateRangeResolveResult.Valid(start, endExclusive);
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
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

        private Guid GetCurrentUserId()
        {
            var userIdText =
                User.FindFirstValue("user_id") ??
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userIdText, out var userId)
                ? userId
                : Guid.Empty;
        }

        private static string NormalizeRequiredText(string value)
        {
            return value.Trim();
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
            var cleaned = new string(value.Select(ch => invalidChars.Contains(ch) ? '-' : ch).ToArray());

            return string.IsNullOrWhiteSpace(cleaned)
                ? "credential-license"
                : cleaned.Trim();
        }

        private static string CombineUrlPath(params string[] parts)
        {
            return "/" + string.Join(
                "/",
                parts
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim('/'))
            );
        }

        private sealed class DateRangeResolveResult
        {
            public bool IsValid { get; private set; }
            public string? ErrorMessage { get; private set; }
            public DateTime? Start { get; private set; }
            public DateTime? EndExclusive { get; private set; }

            public static DateRangeResolveResult Valid(DateTime? start, DateTime? endExclusive)
            {
                return new DateRangeResolveResult
                {
                    IsValid = true,
                    Start = start,
                    EndExclusive = endExclusive
                };
            }

            public static DateRangeResolveResult Invalid(string errorMessage)
            {
                return new DateRangeResolveResult
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
        }

        private sealed class FileResolveResult
        {
            public bool IsValid { get; private set; }
            public string? ErrorMessage { get; private set; }
            public string? PhysicalPath { get; private set; }
            public string? ContentType { get; private set; }
            public string? FileName { get; private set; }
            public string? DownloadName { get; private set; }

            public static FileResolveResult Valid(
                string physicalPath,
                string contentType,
                string fileName,
                string downloadName)
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

            public static FileResolveResult Invalid(string errorMessage)
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
