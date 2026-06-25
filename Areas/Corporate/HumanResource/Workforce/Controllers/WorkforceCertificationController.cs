using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponseWorkforceCertificationPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs.WorkforceCertificationResponse>;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/certifications")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFORCE",
        moduleName: "Human Resource Workforce",
        displayName: "Workforce Certification",
        AreaName = "Corporate",
        ControllerName = "WorkforceCertification",
        Description = "Workforce certification management",
        SortOrder = 26
    )]
    [Tags("Corporate / Human Resource / Workforce / Certification")]
    public class WorkforceCertificationController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.Workforce.Certification";
        private const string CodePrefix = "CRT-RSMMC-";
        private const int CodeNumberLength = 5;
        private const long MaxFileSizeBytes = 10 * 1024 * 1024;

        private static readonly HashSet<string> AllowedCertificationTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "Clinical",
            "NonClinical",
            "Safety",
            "Quality",
            "IT",
            "Other"
        };

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

        public WorkforceCertificationController(
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
        [ProducesResponseType(typeof(ApiResponse<WorkforceCertificationFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Certification", Description = "Melihat metadata filter certification workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceCertification", "Read")]
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

            var result = new WorkforceCertificationFilterMetadataResponse
            {
                DefaultFilter = new WorkforceCertificationDefaultFilterResponse(),
                CustomPeriods = new List<WorkforceCertificationCustomPeriodOptionResponse>
                {
                    new() { Value = "today", Label = "Hari ini" },
                    new() { Value = "last7days", Label = "7 hari terakhir" },
                    new() { Value = "thismonth", Label = "Bulan ini" },
                    new() { Value = "lastmonth", Label = "Bulan lalu" }
                },
                SortOptions = new List<WorkforceCertificationSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "requirementCode", Label = "Kode certification" },
                    new() { Value = "certificationType", Label = "Tipe certification" },
                    new() { Value = "certificationName", Label = "Nama certification" },
                    new() { Value = "issuer", Label = "Penerbit" },
                    new() { Value = "certificateNumber", Label = "Nomor sertifikat" },
                    new() { Value = "issueDate", Label = "Tanggal terbit" },
                    new() { Value = "expiredDate", Label = "Tanggal expired" },
                    new() { Value = "isLifetime", Label = "Lifetime" },
                    new() { Value = "isVerified", Label = "Terverifikasi" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                CertificationTypes = AllowedCertificationTypes.OrderBy(x => x).ToList()
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceCertification.GetFilterMetadata",
                "Mengambil metadata filter certification workforce.",
                new { workforceProfileId, profile.ProfileCode }
            );

            return Ok(ApiResponse<WorkforceCertificationFilterMetadataResponse>.Ok(
                result,
                "Metadata filter certification workforce berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceCertificationSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Certification", Description = "Melihat ringkasan certification workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceCertification", "Read")]
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

            var today = AppDateTimeHelper.OperationalDate();
            var query = BuildBaseQuery(workforceProfileId);

            var result = new WorkforceCertificationSummaryResponse
            {
                WorkforceProfileId = profile.Id,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                TotalCertification = await query.CountAsync(),
                ActiveCertification = await query.CountAsync(x => x.IsActive),
                InactiveCertification = await query.CountAsync(x => !x.IsActive),
                VerifiedCertification = await query.CountAsync(x => x.IsVerified),
                UnverifiedCertification = await query.CountAsync(x => !x.IsVerified),
                ExpiredCertification = await query.CountAsync(x => !x.IsLifetime && x.ExpiredDate.HasValue && x.ExpiredDate.Value.Date < today),
                LifetimeCertification = await query.CountAsync(x => x.IsLifetime),
                CertificationWithFile = await query.CountAsync(x => x.FilePath != null && x.FilePath != string.Empty),
                CertificationWithoutFile = await query.CountAsync(x => x.FilePath == null || x.FilePath == string.Empty)
            };

            return Ok(ApiResponse<WorkforceCertificationSummaryResponse>.Ok(
                result,
                "Ringkasan certification workforce berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseWorkforceCertificationPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Certification", Description = "Melihat certification workforce profile", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceCertification", "Read")]
        public async Task<IActionResult> GetCertifications(
            Guid workforceProfileId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] string? certificationType,
            [FromQuery] bool? isVerified,
            [FromQuery] bool? isActive,
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

            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery(workforceProfileId);
            query = ApplyDateFilter(query, startDate, endDate, customPeriod);
            query = ApplyStandardFilter(query, certificationType, isVerified, isActive, search);

            var totalData = await query.CountAsync();

            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var actorNames = await GetActorNameMapAsync(
                entities
                    .Select(x => x.CreateBy)
                    .Where(x => x != Guid.Empty)
            );

            var items = entities
                .Select(x => MapResponse(x, profile, actorNames))
                .ToList();

            var result = new ResponseWorkforceCertificationPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseWorkforceCertificationPagedResult>.Ok(
                result,
                "Data certification workforce berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceCertificationOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Certification", Description = "Melihat pilihan certification workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceCertification", "Read")]
        public async Task<IActionResult> GetOptions(
            Guid workforceProfileId,
            [FromQuery] string? certificationType,
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
            query = ApplyStandardFilter(query, certificationType, isVerified, onlyActive ? true : null, search);

            var totalData = await query.CountAsync();
            var today = AppDateTimeHelper.OperationalDate();

            var items = await query
                .OrderByDescending(x => x.IsVerified)
                .ThenBy(x => x.ExpiredDate)
                .ThenBy(x => x.CertificationName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new WorkforceCertificationOptionResponse
                {
                    Id = x.Id,
                    RequirementCode = x.RequirementCode,
                    CertificationType = x.CertificationType,
                    CertificationName = x.CertificationName,
                    Issuer = x.Issuer,
                    CertificateNumber = x.CertificateNumber,
                    IssueDate = x.IssueDate,
                    ExpiredDate = x.ExpiredDate,
                    IsLifetime = x.IsLifetime,
                    HasFile = x.FilePath != null && x.FilePath != string.Empty,
                    IsVerified = x.IsVerified,
                    IsExpired = !x.IsLifetime && x.ExpiredDate.HasValue && x.ExpiredDate.Value.Date < today
                })
                .ToListAsync();

            var result = new WorkforceCertificationOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<WorkforceCertificationOptionPagedResponse>.Ok(
                result,
                "Data pilihan certification workforce berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceCertificationDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Certification", Description = "Melihat detail certification workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceCertification", "Read")]
        public async Task<IActionResult> GetCertificationById(Guid workforceProfileId, Guid id)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var entity = await BuildBaseQuery(workforceProfileId)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Certification workforce tidak ditemukan."
                ));
            }

            var actorNames = await GetActorNameMapAsync(new[]
            {
                entity.CreateBy,
                entity.UpdateBy
            });

            var data = MapDetailResponse(entity, profile, actorNames);
            NormalizeAuditResponse(data);

            return Ok(ApiResponse<WorkforceCertificationDetailResponse>.Ok(
                data,
                "Detail certification workforce berhasil diambil."
            ));
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceCertificationDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Workforce Certification", Description = "Menambah certification workforce profile", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("WorkforceCertification", "Create")]
        public async Task<IActionResult> CreateCertification(
            Guid workforceProfileId,
            [FromForm] CreateWorkforceCertificationRequest request)
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
                    validation.ErrorMessage ?? "Data certification tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            string? filePath = null;
            string? fileContentType = null;

            if (request.File != null)
            {
                var savedFile = await SaveCertificationFileAsync(workforceProfileId, request.File);
                filePath = savedFile.FilePath;
                fileContentType = savedFile.ContentType;
            }

            var entity = new WfpCertification
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = workforceProfileId,
                RequirementCode = await GenerateCertificationCodeAsync(),
                CertificationType = NormalizeRequiredText(request.CertificationType),
                CertificationName = NormalizeRequiredText(request.CertificationName),
                Issuer = NormalizeNullableText(request.Issuer),
                CertificateNumber = NormalizeNullableText(request.CertificateNumber),
                IssueDate = ToUtcDate(request.IssueDate),
                ExpiredDate = request.IsLifetime ? null : ToNullableUtcDate(request.ExpiredDate),
                IsLifetime = request.IsLifetime,
                FilePath = filePath,
                FileContentType = fileContentType,
                IsVerified = request.IsVerified,
                IsActive = request.IsActive,
                Description = NormalizeNullableText(request.Description),
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<WfpCertification>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var actorNames = await GetActorNameMapAsync(new[] { entity.CreateBy });
            var data = MapDetailResponse(entity, profile, actorNames);
            NormalizeAuditResponse(data);

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceCertification.CreateCertification",
                "Certification workforce berhasil dibuat.",
                new { workforceProfileId, entity.Id, entity.RequirementCode }
            );

            return Ok(ApiResponse<WorkforceCertificationDetailResponse>.Ok(
                data,
                "Certification workforce berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceCertificationDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Workforce Certification", Description = "Mengubah certification workforce profile", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceCertification", "Update")]
        public async Task<IActionResult> UpdateCertification(
            Guid workforceProfileId,
            Guid id,
            [FromForm] UpdateWorkforceCertificationRequest request)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var entity = await _dbContext.Set<WfpCertification>()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Certification workforce tidak ditemukan."
                ));
            }

            var validation = await ValidateRequestAsync(workforceProfileId, id, request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data certification tidak valid."
                ));
            }

            if (request.File != null && request.ReplaceExistingFile)
            {
                DeletePhysicalFileIfExists(entity.FilePath);

                var savedFile = await SaveCertificationFileAsync(workforceProfileId, request.File);
                entity.FilePath = savedFile.FilePath;
                entity.FileContentType = savedFile.ContentType;
            }
            else if (request.File != null && string.IsNullOrWhiteSpace(entity.FilePath))
            {
                var savedFile = await SaveCertificationFileAsync(workforceProfileId, request.File);
                entity.FilePath = savedFile.FilePath;
                entity.FileContentType = savedFile.ContentType;
            }

            entity.CertificationType = NormalizeRequiredText(request.CertificationType);
            entity.CertificationName = NormalizeRequiredText(request.CertificationName);
            entity.Issuer = NormalizeNullableText(request.Issuer);
            entity.CertificateNumber = NormalizeNullableText(request.CertificateNumber);
            entity.IssueDate = ToUtcDate(request.IssueDate);
            entity.ExpiredDate = request.IsLifetime ? null : ToNullableUtcDate(request.ExpiredDate);
            entity.IsLifetime = request.IsLifetime;
            entity.IsVerified = request.IsVerified;
            entity.IsActive = request.IsActive;
            entity.Description = NormalizeNullableText(request.Description);
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            var actorNames = await GetActorNameMapAsync(new[]
            {
                entity.CreateBy,
                entity.UpdateBy
            });

            var data = MapDetailResponse(entity, profile, actorNames);
            NormalizeAuditResponse(data);

            return Ok(ApiResponse<WorkforceCertificationDetailResponse>.Ok(
                data,
                "Certification workforce berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Workforce Certification", Description = "Mengubah status certification workforce", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("WorkforceCertification", "Update")]
        public async Task<IActionResult> UpdateCertificationStatus(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWorkforceCertificationStatusRequest request)
        {
            var entity = await _dbContext.Set<WfpCertification>()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Certification workforce tidak ditemukan."
                ));
            }

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status certification workforce berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/verify")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Verify Workforce Certification", Description = "Verifikasi certification workforce", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("WorkforceCertification", "Update")]
        public async Task<IActionResult> VerifyCertification(
            Guid workforceProfileId,
            Guid id,
            [FromBody] VerifyWorkforceCertificationRequest request)
        {
            var entity = await _dbContext.Set<WfpCertification>()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Certification workforce tidak ditemukan."
                ));
            }

            entity.IsVerified = request.IsVerified;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                request.IsVerified
                    ? "Certification workforce berhasil diverifikasi."
                    : "Verifikasi certification workforce berhasil dibatalkan."
            ));
        }

        [HttpGet("{id:guid}/preview")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Certification", Description = "Preview file certification workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceCertification", "Read")]
        public async Task<IActionResult> PreviewCertification(Guid workforceProfileId, Guid id)
        {
            var fileValidation = await GetCertificationFileAsync(workforceProfileId, id);

            if (!fileValidation.IsValid)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    fileValidation.ErrorMessage ?? "File certification workforce tidak ditemukan."
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
        [AccessAction("Read", "Read Workforce Certification", Description = "Download file certification workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceCertification", "Read")]
        public async Task<IActionResult> DownloadCertification(Guid workforceProfileId, Guid id)
        {
            var fileValidation = await GetCertificationFileAsync(workforceProfileId, id);

            if (!fileValidation.IsValid)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    fileValidation.ErrorMessage ?? "File certification workforce tidak ditemukan."
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
        [AccessAction("Delete", "Delete Workforce Certification File", Description = "Menghapus file certification workforce", AccessType = AccessTypes.Delete, SortOrder = 6)]
        [AccessPermission("WorkforceCertification", "Delete")]
        public async Task<IActionResult> DeleteCertificationFile(
            Guid workforceProfileId,
            Guid id,
            [FromBody] DeleteWorkforceCertificationFileRequest? request = null)
        {
            var entity = await _dbContext.Set<WfpCertification>()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Certification workforce tidak ditemukan."
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
                "File certification workforce berhasil dihapus."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Workforce Certification", Description = "Menghapus certification workforce", AccessType = AccessTypes.Delete, SortOrder = 7)]
        [AccessPermission("WorkforceCertification", "Delete")]
        public async Task<IActionResult> DeleteCertification(Guid workforceProfileId, Guid id)
        {
            var entity = await _dbContext.Set<WfpCertification>()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Certification workforce tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsActive = false;
            entity.IsDelete = true;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Certification workforce berhasil dihapus."
            ));
        }

        private IQueryable<WfpCertification> BuildBaseQuery(Guid workforceProfileId)
        {
            return _dbContext.Set<WfpCertification>()
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);
        }

        private static IQueryable<WfpCertification> ApplyDateFilter(
            IQueryable<WfpCertification> query,
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

            if (!startDate.HasValue && !endDate.HasValue && !string.IsNullOrWhiteSpace(customPeriod))
            {
                var today = AppDateTimeHelper.OperationalDate();

                switch (customPeriod.Trim().ToLowerInvariant())
                {
                    case "today":
                        query = query.Where(x => x.CreateDateTime >= today && x.CreateDateTime < today.AddDays(1));
                        break;

                    case "last7days":
                        query = query.Where(x => x.CreateDateTime >= today.AddDays(-6) && x.CreateDateTime < today.AddDays(1));
                        break;

                    case "thismonth":
                        var thisMonthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                        query = query.Where(x => x.CreateDateTime >= thisMonthStart && x.CreateDateTime < thisMonthStart.AddMonths(1));
                        break;

                    case "lastmonth":
                        var currentMonthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                        var lastMonthStart = currentMonthStart.AddMonths(-1);
                        query = query.Where(x => x.CreateDateTime >= lastMonthStart && x.CreateDateTime < currentMonthStart);
                        break;
                }
            }

            return query;
        }

        private static IQueryable<WfpCertification> ApplyStandardFilter(
            IQueryable<WfpCertification> query,
            string? certificationType,
            bool? isVerified,
            bool? isActive,
            string? search)
        {
            if (!string.IsNullOrWhiteSpace(certificationType))
            {
                var selectedType = certificationType.Trim().ToLower();
                query = query.Where(x => x.CertificationType.ToLower() == selectedType);
            }

            if (isVerified.HasValue)
            {
                query = query.Where(x => x.IsVerified == isVerified.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    (x.RequirementCode != null && x.RequirementCode.ToLower().Contains(keyword)) ||
                    x.CertificationType.ToLower().Contains(keyword) ||
                    x.CertificationName.ToLower().Contains(keyword) ||
                    (x.Issuer != null && x.Issuer.ToLower().Contains(keyword)) ||
                    (x.CertificateNumber != null && x.CertificateNumber.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<WfpCertification> ApplySorting(
            IQueryable<WfpCertification> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "createDateTime").Trim().ToLowerInvariant() switch
            {
                "requirementcode" => isDescending ? query.OrderByDescending(x => x.RequirementCode) : query.OrderBy(x => x.RequirementCode),
                "certificationtype" => isDescending ? query.OrderByDescending(x => x.CertificationType).ThenBy(x => x.CertificationName) : query.OrderBy(x => x.CertificationType).ThenBy(x => x.CertificationName),
                "certificationname" => isDescending ? query.OrderByDescending(x => x.CertificationName) : query.OrderBy(x => x.CertificationName),
                "issuer" => isDescending ? query.OrderByDescending(x => x.Issuer) : query.OrderBy(x => x.Issuer),
                "certificatenumber" => isDescending ? query.OrderByDescending(x => x.CertificateNumber) : query.OrderBy(x => x.CertificateNumber),
                "issuedate" => isDescending ? query.OrderByDescending(x => x.IssueDate) : query.OrderBy(x => x.IssueDate),
                "expireddate" => isDescending ? query.OrderByDescending(x => x.ExpiredDate) : query.OrderBy(x => x.ExpiredDate),
                "islifetime" => isDescending ? query.OrderByDescending(x => x.IsLifetime).ThenBy(x => x.CertificationName) : query.OrderBy(x => x.IsLifetime).ThenBy(x => x.CertificationName),
                "isverified" => isDescending ? query.OrderByDescending(x => x.IsVerified).ThenBy(x => x.CertificationName) : query.OrderBy(x => x.IsVerified).ThenBy(x => x.CertificationName),
                "isactive" => isDescending ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.CertificationName) : query.OrderBy(x => x.IsActive).ThenBy(x => x.CertificationName),
                _ => isDescending ? query.OrderByDescending(x => x.CreateDateTime).ThenBy(x => x.CertificationName) : query.OrderBy(x => x.CreateDateTime).ThenBy(x => x.CertificationName)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid workforceProfileId,
            Guid? excludeId,
            CreateWorkforceCertificationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.CertificationType))
                return (false, "CertificationType wajib diisi.");

            if (!AllowedCertificationTypes.Contains(request.CertificationType.Trim()))
                return (false, "CertificationType hanya boleh Clinical, NonClinical, Safety, Quality, IT, atau Other.");

            if (string.IsNullOrWhiteSpace(request.CertificationName))
                return (false, "CertificationName wajib diisi.");

            if (request.IssueDate == default)
                return (false, "IssueDate wajib diisi.");

            if (!request.IsLifetime && request.ExpiredDate.HasValue && request.ExpiredDate.Value.Date < request.IssueDate.Date)
                return (false, "ExpiredDate tidak boleh lebih kecil dari IssueDate.");

            if (request.File != null)
            {
                var fileValidation = ValidateFile(request.File);

                if (!fileValidation.IsValid)
                    return fileValidation;
            }

            var normalizedCertificateNumber = NormalizeNullableText(request.CertificateNumber);

            if (!string.IsNullOrWhiteSpace(normalizedCertificateNumber))
            {
                var duplicateCertificateNumberQuery = _dbContext.Set<WfpCertification>()
                    .AsNoTracking()
                    .Where(x =>
                        x.WorkforceProfileId == workforceProfileId &&
                        x.CertificateNumber == normalizedCertificateNumber &&
                        !x.IsDelete);

                if (excludeId.HasValue)
                    duplicateCertificateNumberQuery = duplicateCertificateNumberQuery.Where(x => x.Id != excludeId.Value);

                if (await duplicateCertificateNumberQuery.AnyAsync())
                    return (false, "Nomor sertifikat certification sudah terdaftar pada workforce profile ini.");
            }

            return (true, null);
        }

        private static (bool IsValid, string? ErrorMessage) ValidateFile(IFormFile file)
        {
            if (file.Length <= 0)
                return (false, "File certification kosong.");

            if (file.Length > MaxFileSizeBytes)
                return (false, "Ukuran file certification maksimal 10 MB.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!AllowedExtensions.Contains(extension))
                return (false, "Format file tidak didukung. Gunakan PDF, JPG, PNG, DOC, DOCX, XLS, atau XLSX.");

            return (true, null);
        }

        private async Task<string> GenerateCertificationCodeAsync()
        {
            var existingCodes = await _dbContext.Set<WfpCertification>()
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
                nextNumber++;

            return CodePrefix + nextNumber.ToString().PadLeft(CodeNumberLength, '0');
        }

        private async Task<(string FilePath, string? ContentType)> SaveCertificationFileAsync(
            Guid workforceProfileId,
            IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var storage = GetFileStoragePaths();
            var relativeFolder = Path.Combine("workforce-certifications", workforceProfileId.ToString());
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

        private async Task<FileResolveResult> GetCertificationFileAsync(Guid workforceProfileId, Guid id)
        {
            var certification = await _dbContext.Set<WfpCertification>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            if (certification == null)
                return FileResolveResult.Invalid("Certification workforce tidak ditemukan.");

            if (string.IsNullOrWhiteSpace(certification.FilePath))
                return FileResolveResult.Invalid("File certification workforce belum tersedia.");

            var physicalPath = ResolvePhysicalPath(certification.FilePath);

            if (!System.IO.File.Exists(physicalPath))
                return FileResolveResult.Invalid("File fisik certification tidak ditemukan di server.");

            var provider = new FileExtensionContentTypeProvider();

            if (!provider.TryGetContentType(physicalPath, out var contentType))
                contentType = certification.FileContentType ?? "application/octet-stream";

            var extension = Path.GetExtension(physicalPath);
            var safeCertificationName = SanitizeFileName(certification.CertificationName);
            var fileName = Path.GetFileName(physicalPath);
            var downloadName = $"{certification.RequirementCode ?? "CERTIFICATION"}_{safeCertificationName}{extension}";

            return FileResolveResult.Valid(physicalPath, contentType, fileName, downloadName);
        }

        private void DeletePhysicalFileIfExists(string? filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            var physicalPath = ResolvePhysicalPath(filePath);

            if (System.IO.File.Exists(physicalPath))
                System.IO.File.Delete(physicalPath);
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
                throw new InvalidOperationException("Path file tidak valid.");

            return fullPath;
        }

        private (string RootPath, string PublicRequestPath) GetFileStoragePaths()
        {
            var publicRequestPath = _configuration["FileStorage:PublicRequestPath"] ?? "/uploads";

            if (!publicRequestPath.StartsWith('/'))
                publicRequestPath = "/" + publicRequestPath;

            publicRequestPath = publicRequestPath.TrimEnd('/');

            var configuredRoot = _configuration["FileStorage:UploadRootPath"];

            if (!string.IsNullOrWhiteSpace(configuredRoot))
            {
                Directory.CreateDirectory(configuredRoot);
                return (configuredRoot, publicRequestPath);
            }

            var webRootPath = _environment.WebRootPath;

            if (string.IsNullOrWhiteSpace(webRootPath))
                webRootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");

            var rootPath = Path.Combine(webRootPath, publicRequestPath.TrimStart('/'));
            Directory.CreateDirectory(rootPath);

            return (rootPath, publicRequestPath);
        }

        private WorkforceCertificationResponse MapResponse(
            WfpCertification entity,
            MstWorkforceProfile profile,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            var today = AppDateTimeHelper.OperationalDate();
            var hasFile = !string.IsNullOrWhiteSpace(entity.FilePath);

            return new WorkforceCertificationResponse
            {
                Id = entity.Id,
                WorkforceProfileId = entity.WorkforceProfileId,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                RequirementCode = entity.RequirementCode,
                CertificationType = entity.CertificationType,
                CertificationName = entity.CertificationName,
                Issuer = entity.Issuer,
                CertificateNumber = entity.CertificateNumber,
                IssueDate = entity.IssueDate,
                ExpiredDate = entity.ExpiredDate,
                IsLifetime = entity.IsLifetime,
                HasFile = hasFile,
                FilePath = entity.FilePath,
                FileContentType = entity.FileContentType,
                FileName = hasFile ? Path.GetFileName(entity.FilePath) : null,
                FilePreviewUrl = hasFile ? BuildEndpointUrl(entity.WorkforceProfileId, entity.Id, "preview") : null,
                FileDownloadUrl = hasFile ? BuildEndpointUrl(entity.WorkforceProfileId, entity.Id, "download") : null,
                IsVerified = entity.IsVerified,
                IsExpired = !entity.IsLifetime && entity.ExpiredDate.HasValue && entity.ExpiredDate.Value.Date < today,
                IsActive = entity.IsActive,
                Description = entity.Description,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy)
            };
        }

        private WorkforceCertificationDetailResponse MapDetailResponse(
            WfpCertification entity,
            MstWorkforceProfile profile,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            var response = MapResponse(entity, profile, actorNames);

            return new WorkforceCertificationDetailResponse
            {
                Id = response.Id,
                WorkforceProfileId = response.WorkforceProfileId,
                ProfileCode = response.ProfileCode,
                DisplayName = response.DisplayName,
                RequirementCode = response.RequirementCode,
                CertificationType = response.CertificationType,
                CertificationName = response.CertificationName,
                Issuer = response.Issuer,
                CertificateNumber = response.CertificateNumber,
                IssueDate = response.IssueDate,
                ExpiredDate = response.ExpiredDate,
                IsLifetime = response.IsLifetime,
                HasFile = response.HasFile,
                FilePath = response.FilePath,
                FileContentType = response.FileContentType,
                FileName = response.FileName,
                FilePreviewUrl = response.FilePreviewUrl,
                FileDownloadUrl = response.FileDownloadUrl,
                IsVerified = response.IsVerified,
                IsExpired = response.IsExpired,
                IsActive = response.IsActive,
                Description = response.Description,
                CreateDateTime = response.CreateDateTime,
                CreateBy = response.CreateBy,
                CreateByName = response.CreateByName,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };
        }

        private string BuildEndpointUrl(Guid workforceProfileId, Guid id, string action)
        {
            var pathBase = Request.PathBase.HasValue ? Request.PathBase.Value : string.Empty;
            var path = $"{pathBase}/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/certifications/{id}/{action}";

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

        private static void NormalizeAuditResponse(WorkforceCertificationDetailResponse data)
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

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
                pageNumber = 1;

            if (pageSize < 1)
                pageSize = 25;

            if (pageSize > 100)
                pageSize = 100;

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

        private static DateTime ToUtcDate(DateTime value)
        {
            return DateTime.SpecifyKind(value.Date, DateTimeKind.Utc);
        }

        private static DateTime? ToNullableUtcDate(DateTime? value)
        {
            return value.HasValue
                ? DateTime.SpecifyKind(value.Value.Date, DateTimeKind.Utc)
                : null;
        }

        private static string SanitizeFileName(string value)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var cleaned = new string(value.Select(ch => invalidChars.Contains(ch) ? '-' : ch).ToArray());

            return string.IsNullOrWhiteSpace(cleaned)
                ? "certification"
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
