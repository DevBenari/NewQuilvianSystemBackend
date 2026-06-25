using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponseWorkforceDocumentPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs.WorkforceDocumentResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/documents")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFORCE",
        moduleName: "Human Resource Workforce",
        displayName: "Workforce Document",
        AreaName = "Corporate",
        ControllerName = "WorkforceDocument",
        Description = "Workforce document management",
        SortOrder = 23
    )]
    [Tags("Corporate / Human Resource / Workforce / Document")]
    public class WorkforceDocumentController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.Workforce.Document";
        private const string CodePrefix = "DOC-RSMMC-";
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

        public WorkforceDocumentController(
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
        [ProducesResponseType(typeof(ApiResponse<WorkforceDocumentFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Document", Description = "Melihat metadata filter dokumen workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceDocument", "Read")]
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

            var result = new WorkforceDocumentFilterMetadataResponse
            {
                DefaultFilter = new WorkforceDocumentDefaultFilterResponse(),
                CustomPeriods = new List<WorkforceDocumentCustomPeriodOptionResponse>
                {
                    new() { Value = "today", Label = "Hari ini" },
                    new() { Value = "last7days", Label = "7 hari terakhir" },
                    new() { Value = "thismonth", Label = "Bulan ini" },
                    new() { Value = "lastmonth", Label = "Bulan lalu" }
                },
                SortOptions = new List<WorkforceDocumentSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "requirementCode", Label = "Kode dokumen" },
                    new() { Value = "documentType", Label = "Tipe dokumen" },
                    new() { Value = "documentName", Label = "Nama dokumen" },
                    new() { Value = "documentNumber", Label = "Nomor dokumen" },
                    new() { Value = "issueDate", Label = "Tanggal terbit" },
                    new() { Value = "expiredDate", Label = "Tanggal expired" },
                    new() { Value = "isVerified", Label = "Terverifikasi" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                DocumentTypeOptions = BuildDocumentTypeOptions(),
                FrontendGuide = new List<string>
                {
                    "DocumentType adalah kategori dokumen dan dipilih dari enum/dropdown, bukan diketik bebas.",
                    "DocumentName adalah nama tampilan dokumen, contoh: KTP Budi, NPWP Budi, Kontrak Kerja 2026.",
                    "RequirementCode dibuat otomatis oleh backend dengan format DOC-RSMMC-00001, jadi frontend tidak perlu mengirim RequirementCode.",
                    "Gunakan FilePreviewUrl untuk preview modal/iframe dan FileDownloadUrl untuk download.",
                    "Gunakan Account/identity number pada DocumentNumber bila dokumen memiliki nomor resmi seperti KTP, NPWP, passport, atau nomor kontrak."
                }
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceDocument.GetFilterMetadata",
                "Mengambil metadata filter dokumen workforce.",
                new { workforceProfileId, profile.ProfileCode }
            );

            return Ok(ApiResponse<WorkforceDocumentFilterMetadataResponse>.Ok(
                result,
                "Metadata filter dokumen workforce berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceDocumentSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Document", Description = "Melihat ringkasan dokumen workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceDocument", "Read")]
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

            var result = new WorkforceDocumentSummaryResponse
            {
                WorkforceProfileId = profile.Id,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                TotalDocument = await query.CountAsync(),
                ActiveDocument = await query.CountAsync(x => x.IsActive),
                InactiveDocument = await query.CountAsync(x => !x.IsActive),
                VerifiedDocument = await query.CountAsync(x => x.IsVerified),
                UnverifiedDocument = await query.CountAsync(x => !x.IsVerified),
                ExpiredDocument = await query.CountAsync(x => x.ExpiredDate.HasValue && x.ExpiredDate.Value.Date < today),
                DocumentWithFile = await query.CountAsync(x => x.FilePath != null && x.FilePath != string.Empty),
                DocumentWithoutFile = await query.CountAsync(x => x.FilePath == null || x.FilePath == string.Empty),
                KtpDocument = await query.CountAsync(x => x.DocumentType == "KTP"),
                NpwpDocument = await query.CountAsync(x => x.DocumentType == "NPWP"),
                ContractDocument = await query.CountAsync(x => x.DocumentType == "CONTRACT"),
                PassportDocument = await query.CountAsync(x => x.DocumentType == "PASSPORT")
            };

            return Ok(ApiResponse<WorkforceDocumentSummaryResponse>.Ok(
                result,
                "Ringkasan dokumen workforce berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseWorkforceDocumentPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Document", Description = "Melihat dokumen workforce profile", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceDocument", "Read")]
        public async Task<IActionResult> GetDocuments(
            Guid workforceProfileId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] WorkforceDocumentType? documentType,
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

            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery(workforceProfileId);
            query = ApplyDateFilter(query, startDate, endDate, customPeriod);
            query = ApplyStandardFilter(query, documentType, isVerified, isActive, isExpired, search);

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

            var result = new ResponseWorkforceDocumentPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseWorkforceDocumentPagedResult>.Ok(
                result,
                "Data dokumen workforce berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceDocumentOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Document", Description = "Melihat pilihan dokumen workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceDocument", "Read")]
        public async Task<IActionResult> GetOptions(
            Guid workforceProfileId,
            [FromQuery] WorkforceDocumentType? documentType,
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
            query = ApplyStandardFilter(
                query,
                documentType,
                isVerified,
                onlyActive ? true : null,
                isExpired: null,
                search
            );

            var totalData = await query.CountAsync();
            var today = AppDateTimeHelper.OperationalDate();

            var items = await query
                .OrderByDescending(x => x.IsVerified)
                .ThenBy(x => x.DocumentType)
                .ThenBy(x => x.DocumentName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new WorkforceDocumentOptionResponse
                {
                    Id = x.Id,
                    RequirementCode = x.RequirementCode,
                    DocumentType = x.DocumentType,
                    DocumentName = x.DocumentName,
                    DocumentNumber = x.DocumentNumber,
                    IssueDate = x.IssueDate,
                    ExpiredDate = x.ExpiredDate,
                    HasFile = x.FilePath != null && x.FilePath != string.Empty,
                    IsVerified = x.IsVerified,
                    IsExpired = x.ExpiredDate.HasValue && x.ExpiredDate.Value.Date < today
                })
                .ToListAsync();

            var result = new WorkforceDocumentOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<WorkforceDocumentOptionPagedResponse>.Ok(
                result,
                "Data pilihan dokumen workforce berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceDocumentDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Document", Description = "Melihat detail dokumen workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceDocument", "Read")]
        public async Task<IActionResult> GetDocumentById(Guid workforceProfileId, Guid id)
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
                    "Dokumen workforce tidak ditemukan."
                ));
            }

            var actorNames = await GetActorNameMapAsync(new[]
            {
                entity.CreateBy,
                entity.UpdateBy
            });

            var data = MapDetailResponse(entity, profile, actorNames);
            NormalizeAudit(data);

            return Ok(ApiResponse<WorkforceDocumentDetailResponse>.Ok(
                data,
                "Detail dokumen workforce berhasil diambil."
            ));
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceDocumentDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Workforce Document", Description = "Menambah dokumen workforce profile", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("WorkforceDocument", "Create")]
        public async Task<IActionResult> CreateDocument(
            Guid workforceProfileId,
            [FromForm] CreateWorkforceDocumentRequest request)
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
                    validation.ErrorMessage ?? "Data dokumen tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            string? filePath = null;
            string? fileContentType = null;

            if (request.File != null)
            {
                var savedFile = await SaveDocumentFileAsync(workforceProfileId, request.File);
                filePath = savedFile.FilePath;
                fileContentType = savedFile.ContentType;
            }

            var entity = new WfpDocument
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = workforceProfileId,
                RequirementCode = await GenerateDocumentCodeAsync(),
                DocumentType = NormalizeDocumentType(request.DocumentType),
                DocumentName = NormalizeRequiredText(request.DocumentName),
                DocumentNumber = NormalizeNullableText(request.DocumentNumber),
                IssueDate = request.IssueDate?.Date,
                ExpiredDate = request.ExpiredDate?.Date,
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

            _dbContext.Set<WfpDocument>().Add(entity);
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceDocument.CreateDocument",
                "Dokumen workforce berhasil dibuat.",
                new { workforceProfileId, entity.Id, entity.RequirementCode }
            );

            var actorNames = await GetActorNameMapAsync(new[]
            {
                entity.CreateBy,
                entity.UpdateBy
            });

            var data = MapDetailResponse(entity, profile, actorNames);
            NormalizeAudit(data);

            return Ok(ApiResponse<WorkforceDocumentDetailResponse>.Ok(
                data,
                "Dokumen workforce berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceDocumentDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Workforce Document", Description = "Mengubah dokumen workforce profile", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceDocument", "Update")]
        public async Task<IActionResult> UpdateDocument(
            Guid workforceProfileId,
            Guid id,
            [FromForm] UpdateWorkforceDocumentRequest request)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var entity = await _dbContext.Set<WfpDocument>()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Dokumen workforce tidak ditemukan."
                ));
            }

            var validation = await ValidateRequestAsync(workforceProfileId, id, request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data dokumen tidak valid."
                ));
            }

            if (request.ReplaceExistingFile && request.File == null)
            {
                DeletePhysicalFileIfExists(entity.FilePath);
                entity.FilePath = null;
                entity.FileContentType = null;
            }
            else if (request.File != null && request.ReplaceExistingFile)
            {
                DeletePhysicalFileIfExists(entity.FilePath);

                var savedFile = await SaveDocumentFileAsync(workforceProfileId, request.File);
                entity.FilePath = savedFile.FilePath;
                entity.FileContentType = savedFile.ContentType;
            }
            else if (request.File != null && string.IsNullOrWhiteSpace(entity.FilePath))
            {
                var savedFile = await SaveDocumentFileAsync(workforceProfileId, request.File);
                entity.FilePath = savedFile.FilePath;
                entity.FileContentType = savedFile.ContentType;
            }

            entity.DocumentType = NormalizeDocumentType(request.DocumentType);
            entity.DocumentName = NormalizeRequiredText(request.DocumentName);
            entity.DocumentNumber = NormalizeNullableText(request.DocumentNumber);
            entity.IssueDate = request.IssueDate?.Date;
            entity.ExpiredDate = request.ExpiredDate?.Date;
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
            NormalizeAudit(data);

            return Ok(ApiResponse<WorkforceDocumentDetailResponse>.Ok(
                data,
                "Dokumen workforce berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Workforce Document", Description = "Mengubah status dokumen workforce", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("WorkforceDocument", "Update")]
        public async Task<IActionResult> UpdateDocumentStatus(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWorkforceDocumentStatusRequest request)
        {
            var entity = await _dbContext.Set<WfpDocument>()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Dokumen workforce tidak ditemukan."
                ));
            }

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status dokumen workforce berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/verify")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Verify Workforce Document", Description = "Verifikasi dokumen workforce", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("WorkforceDocument", "Update")]
        public async Task<IActionResult> VerifyDocument(
            Guid workforceProfileId,
            Guid id,
            [FromBody] VerifyWorkforceDocumentRequest request)
        {
            var entity = await _dbContext.Set<WfpDocument>()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Dokumen workforce tidak ditemukan."
                ));
            }

            entity.IsVerified = request.IsVerified;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                request.IsVerified
                    ? "Dokumen workforce berhasil diverifikasi."
                    : "Verifikasi dokumen workforce berhasil dibatalkan."
            ));
        }

        [HttpGet("{id:guid}/preview")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Document", Description = "Preview dokumen workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceDocument", "Read")]
        public async Task<IActionResult> PreviewDocument(Guid workforceProfileId, Guid id)
        {
            var fileValidation = await GetDocumentFileAsync(workforceProfileId, id);

            if (!fileValidation.IsValid)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    fileValidation.ErrorMessage ?? "File dokumen workforce tidak ditemukan."
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
        [AccessAction("Read", "Read Workforce Document", Description = "Download dokumen workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceDocument", "Read")]
        public async Task<IActionResult> DownloadDocument(Guid workforceProfileId, Guid id)
        {
            var fileValidation = await GetDocumentFileAsync(workforceProfileId, id);

            if (!fileValidation.IsValid)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    fileValidation.ErrorMessage ?? "File dokumen workforce tidak ditemukan."
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
        [AccessAction("Delete", "Delete Workforce Document File", Description = "Menghapus file dokumen workforce", AccessType = AccessTypes.Delete, SortOrder = 6)]
        [AccessPermission("WorkforceDocument", "Delete")]
        public async Task<IActionResult> DeleteDocumentFile(
            Guid workforceProfileId,
            Guid id,
            [FromBody] DeleteWorkforceDocumentFileRequest? request = null)
        {
            var entity = await _dbContext.Set<WfpDocument>()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Dokumen workforce tidak ditemukan."
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
                "File dokumen workforce berhasil dihapus."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Workforce Document", Description = "Menghapus dokumen workforce", AccessType = AccessTypes.Delete, SortOrder = 7)]
        [AccessPermission("WorkforceDocument", "Delete")]
        public async Task<IActionResult> DeleteDocument(Guid workforceProfileId, Guid id)
        {
            var entity = await _dbContext.Set<WfpDocument>()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Dokumen workforce tidak ditemukan."
                ));
            }

            entity.IsActive = false;
            entity.IsDelete = true;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = GetCurrentUserId();
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Dokumen workforce berhasil dihapus."
            ));
        }

        private IQueryable<WfpDocument> BuildBaseQuery(Guid workforceProfileId)
        {
            return _dbContext.Set<WfpDocument>()
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);
        }

        private static IQueryable<WfpDocument> ApplyDateFilter(
            IQueryable<WfpDocument> query,
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

        private static IQueryable<WfpDocument> ApplyStandardFilter(
            IQueryable<WfpDocument> query,
            WorkforceDocumentType? documentType,
            bool? isVerified,
            bool? isActive,
            bool? isExpired,
            string? search)
        {
            if (documentType.HasValue && documentType.Value != WorkforceDocumentType.Unknown)
            {
                var selectedType = NormalizeDocumentType(documentType.Value);
                query = query.Where(x => x.DocumentType == selectedType);
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
                var today = AppDateTimeHelper.OperationalDate();
                query = isExpired.Value
                    ? query.Where(x => x.ExpiredDate.HasValue && x.ExpiredDate.Value.Date < today)
                    : query.Where(x => !x.ExpiredDate.HasValue || x.ExpiredDate.Value.Date >= today);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    (x.RequirementCode != null && x.RequirementCode.ToLower().Contains(keyword)) ||
                    x.DocumentType.ToLower().Contains(keyword) ||
                    x.DocumentName.ToLower().Contains(keyword) ||
                    (x.DocumentNumber != null && x.DocumentNumber.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<WfpDocument> ApplySorting(
            IQueryable<WfpDocument> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "createDateTime").Trim().ToLowerInvariant() switch
            {
                "requirementcode" => isDescending ? query.OrderByDescending(x => x.RequirementCode) : query.OrderBy(x => x.RequirementCode),
                "documenttype" => isDescending ? query.OrderByDescending(x => x.DocumentType).ThenBy(x => x.DocumentName) : query.OrderBy(x => x.DocumentType).ThenBy(x => x.DocumentName),
                "documentname" => isDescending ? query.OrderByDescending(x => x.DocumentName) : query.OrderBy(x => x.DocumentName),
                "documentnumber" => isDescending ? query.OrderByDescending(x => x.DocumentNumber) : query.OrderBy(x => x.DocumentNumber),
                "issuedate" => isDescending ? query.OrderByDescending(x => x.IssueDate) : query.OrderBy(x => x.IssueDate),
                "expireddate" => isDescending ? query.OrderByDescending(x => x.ExpiredDate) : query.OrderBy(x => x.ExpiredDate),
                "isverified" => isDescending ? query.OrderByDescending(x => x.IsVerified).ThenBy(x => x.DocumentName) : query.OrderBy(x => x.IsVerified).ThenBy(x => x.DocumentName),
                "isactive" => isDescending ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.DocumentName) : query.OrderBy(x => x.IsActive).ThenBy(x => x.DocumentName),
                _ => isDescending ? query.OrderByDescending(x => x.CreateDateTime).ThenBy(x => x.DocumentName) : query.OrderBy(x => x.CreateDateTime).ThenBy(x => x.DocumentName)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid workforceProfileId,
            Guid? excludeId,
            CreateWorkforceDocumentRequest request)
        {
            var profileExists = await ProfileExistsAsync(workforceProfileId);

            if (!profileExists)
            {
                return (false, "Workforce profile tidak ditemukan.");
            }

            if (!Enum.IsDefined(typeof(WorkforceDocumentType), request.DocumentType) ||
                request.DocumentType == WorkforceDocumentType.Unknown)
            {
                return (false, "DocumentType wajib dipilih dan tidak boleh Unknown.");
            }

            if (string.IsNullOrWhiteSpace(request.DocumentName))
            {
                return (false, "DocumentName wajib diisi.");
            }

            if (request.IssueDate.HasValue &&
                request.ExpiredDate.HasValue &&
                request.ExpiredDate.Value.Date < request.IssueDate.Value.Date)
            {
                return (false, "ExpiredDate tidak boleh lebih kecil dari IssueDate.");
            }

            if (request.File != null)
            {
                var fileValidation = ValidateFile(request.File);

                if (!fileValidation.IsValid)
                {
                    return fileValidation;
                }
            }

            var normalizedDocumentType = NormalizeDocumentType(request.DocumentType);
            var normalizedDocumentNumber = NormalizeNullableText(request.DocumentNumber);

            if (!string.IsNullOrWhiteSpace(normalizedDocumentNumber))
            {
                var duplicateQuery = _dbContext.Set<WfpDocument>()
                    .AsNoTracking()
                    .Where(x =>
                        x.WorkforceProfileId == workforceProfileId &&
                        x.DocumentType == normalizedDocumentType &&
                        x.DocumentNumber == normalizedDocumentNumber &&
                        !x.IsDelete);

                if (excludeId.HasValue)
                {
                    duplicateQuery = duplicateQuery.Where(x => x.Id != excludeId.Value);
                }

                if (await duplicateQuery.AnyAsync())
                {
                    return (false, "DocumentNumber dengan DocumentType tersebut sudah terdaftar pada workforce profile ini.");
                }
            }

            return (true, null);
        }

        private static (bool IsValid, string? ErrorMessage) ValidateFile(IFormFile file)
        {
            if (file.Length <= 0)
            {
                return (false, "File dokumen kosong.");
            }

            if (file.Length > MaxFileSizeBytes)
            {
                return (false, "Ukuran file dokumen maksimal 10 MB.");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!AllowedExtensions.Contains(extension))
            {
                return (false, "Format file tidak didukung. Gunakan PDF, JPG, PNG, DOC, DOCX, XLS, atau XLSX.");
            }

            return (true, null);
        }

        private async Task<string> GenerateDocumentCodeAsync()
        {
            var existingCodes = await _dbContext.Set<WfpDocument>()
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

        private async Task<(string FilePath, string? ContentType)> SaveDocumentFileAsync(
            Guid workforceProfileId,
            IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var storage = GetFileStoragePaths();
            var relativeFolder = Path.Combine("workforce-documents", workforceProfileId.ToString());
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

        private async Task<FileResolveResult> GetDocumentFileAsync(Guid workforceProfileId, Guid id)
        {
            var document = await _dbContext.Set<WfpDocument>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            if (document == null)
            {
                return FileResolveResult.Invalid("Dokumen workforce tidak ditemukan.");
            }

            if (string.IsNullOrWhiteSpace(document.FilePath))
            {
                return FileResolveResult.Invalid("File dokumen workforce belum tersedia.");
            }

            var physicalPath = ResolvePhysicalPath(document.FilePath);

            if (!System.IO.File.Exists(physicalPath))
            {
                return FileResolveResult.Invalid("File fisik dokumen tidak ditemukan di server.");
            }

            var provider = new FileExtensionContentTypeProvider();

            if (!provider.TryGetContentType(physicalPath, out var contentType))
            {
                contentType = document.FileContentType ?? "application/octet-stream";
            }

            var extension = Path.GetExtension(physicalPath);
            var safeDocumentName = SanitizeFileName(document.DocumentName);
            var fileName = Path.GetFileName(physicalPath);
            var downloadName = $"{document.RequirementCode ?? "DOCUMENT"}_{safeDocumentName}{extension}";

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

        private WorkforceDocumentResponse MapResponse(
            WfpDocument entity,
            MstWorkforceProfile profile,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            var today = AppDateTimeHelper.OperationalDate();
            var hasFile = !string.IsNullOrWhiteSpace(entity.FilePath);

            return new WorkforceDocumentResponse
            {
                Id = entity.Id,
                WorkforceProfileId = entity.WorkforceProfileId,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                RequirementCode = entity.RequirementCode,
                DocumentType = entity.DocumentType,
                DocumentName = entity.DocumentName,
                DocumentNumber = entity.DocumentNumber,
                IssueDate = entity.IssueDate,
                ExpiredDate = entity.ExpiredDate,
                HasFile = hasFile,
                FilePath = entity.FilePath,
                FileContentType = entity.FileContentType,
                FileName = hasFile ? Path.GetFileName(entity.FilePath) : null,
                FilePreviewUrl = hasFile ? BuildEndpointUrl(entity.WorkforceProfileId, entity.Id, "preview") : null,
                FileDownloadUrl = hasFile ? BuildEndpointUrl(entity.WorkforceProfileId, entity.Id, "download") : null,
                IsVerified = entity.IsVerified,
                IsExpired = entity.ExpiredDate.HasValue && entity.ExpiredDate.Value.Date < today,
                IsActive = entity.IsActive,
                Description = entity.Description,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy)
            };
        }

        private WorkforceDocumentDetailResponse MapDetailResponse(
            WfpDocument entity,
            MstWorkforceProfile profile,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            var response = MapResponse(entity, profile, actorNames);

            return new WorkforceDocumentDetailResponse
            {
                Id = response.Id,
                WorkforceProfileId = response.WorkforceProfileId,
                ProfileCode = response.ProfileCode,
                DisplayName = response.DisplayName,
                RequirementCode = response.RequirementCode,
                DocumentType = response.DocumentType,
                DocumentName = response.DocumentName,
                DocumentNumber = response.DocumentNumber,
                IssueDate = response.IssueDate,
                ExpiredDate = response.ExpiredDate,
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

        private static void NormalizeAudit(WorkforceDocumentDetailResponse data)
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

        private string BuildEndpointUrl(Guid workforceProfileId, Guid id, string action)
        {
            var pathBase = Request.PathBase.HasValue ? Request.PathBase.Value : string.Empty;
            var path = $"{pathBase}/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/documents/{id}/{action}";

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

        private static string NormalizeDocumentType(WorkforceDocumentType value)
        {
            return value.ToString().Trim().ToUpperInvariant();
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
                ? "document"
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

        private static List<WorkforceDocumentTypeOptionResponse> BuildDocumentTypeOptions()
        {
            return new List<WorkforceDocumentTypeOptionResponse>
            {
                new()
                {
                    Value = WorkforceDocumentType.KTP,
                    Code = "KTP",
                    Label = "KTP",
                    Description = "Kartu Tanda Penduduk.",
                    UsuallyRequiresDocumentNumber = true,
                    UsuallyHasExpiryDate = false
                },
                new()
                {
                    Value = WorkforceDocumentType.NPWP,
                    Code = "NPWP",
                    Label = "NPWP",
                    Description = "Nomor Pokok Wajib Pajak.",
                    UsuallyRequiresDocumentNumber = true,
                    UsuallyHasExpiryDate = false
                },
                new()
                {
                    Value = WorkforceDocumentType.KK,
                    Code = "KK",
                    Label = "Kartu Keluarga",
                    Description = "Dokumen kartu keluarga.",
                    UsuallyRequiresDocumentNumber = true,
                    UsuallyHasExpiryDate = false
                },
                new()
                {
                    Value = WorkforceDocumentType.CONTRACT,
                    Code = "CONTRACT",
                    Label = "Kontrak Kerja",
                    Description = "Dokumen kontrak kerja atau perjanjian kerja.",
                    UsuallyRequiresDocumentNumber = true,
                    UsuallyHasExpiryDate = true
                },
                new()
                {
                    Value = WorkforceDocumentType.NDA,
                    Code = "NDA",
                    Label = "NDA",
                    Description = "Non-disclosure agreement atau perjanjian kerahasiaan.",
                    UsuallyRequiresDocumentNumber = false,
                    UsuallyHasExpiryDate = false
                },
                new()
                {
                    Value = WorkforceDocumentType.PASSPORT,
                    Code = "PASSPORT",
                    Label = "Passport",
                    Description = "Dokumen passport.",
                    UsuallyRequiresDocumentNumber = true,
                    UsuallyHasExpiryDate = true
                },
                new()
                {
                    Value = WorkforceDocumentType.IJAZAH,
                    Code = "IJAZAH",
                    Label = "Ijazah",
                    Description = "Dokumen ijazah pendidikan.",
                    UsuallyRequiresDocumentNumber = false,
                    UsuallyHasExpiryDate = false
                },
                new()
                {
                    Value = WorkforceDocumentType.SKCK,
                    Code = "SKCK",
                    Label = "SKCK",
                    Description = "Surat Keterangan Catatan Kepolisian.",
                    UsuallyRequiresDocumentNumber = true,
                    UsuallyHasExpiryDate = true
                },
                new()
                {
                    Value = WorkforceDocumentType.BPJS_KESEHATAN,
                    Code = "BPJS_KESEHATAN",
                    Label = "BPJS Kesehatan",
                    Description = "Dokumen atau kartu BPJS Kesehatan.",
                    UsuallyRequiresDocumentNumber = true,
                    UsuallyHasExpiryDate = false
                },
                new()
                {
                    Value = WorkforceDocumentType.BPJS_KETENAGAKERJAAN,
                    Code = "BPJS_KETENAGAKERJAAN",
                    Label = "BPJS Ketenagakerjaan",
                    Description = "Dokumen atau kartu BPJS Ketenagakerjaan.",
                    UsuallyRequiresDocumentNumber = true,
                    UsuallyHasExpiryDate = false
                },
                new()
                {
                    Value = WorkforceDocumentType.OTHER,
                    Code = "OTHER",
                    Label = "Lainnya",
                    Description = "Dokumen lainnya yang belum masuk kategori utama.",
                    UsuallyRequiresDocumentNumber = false,
                    UsuallyHasExpiryDate = false
                }
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
