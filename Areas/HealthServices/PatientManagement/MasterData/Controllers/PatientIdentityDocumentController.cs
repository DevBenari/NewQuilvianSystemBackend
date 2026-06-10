using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponsePatientIdentityDocumentPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs.PatientIdentityDocumentResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/patient-management/master-data/patient-identity-documents")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_PATIENT_MANAGEMENT_MASTER_DATA",
        moduleName: "Health Service Patient Management Master Data",
        displayName: "Patient Identity Document",
        AreaName = "HealthServices",
        ControllerName = "PatientIdentityDocument",
        Description = "Health service patient management master data patient identity document",
        SortOrder = 15
    )]
    [Tags("Health Services / Patient Management / Master Data / Patient Identity Document")]
    public class PatientIdentityDocumentController : ControllerBase
    {
        private const string LogCategory = "HealthServices.PatientManagement.MasterData";

        private static readonly List<string> CommonIdentityTypes = new()
        {
            "KTP",
            "NIK",
            "KIA",
            "Passport",
            "SIM",
            "BirthCertificate",
            "StudentCard",
            "Other"
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public PatientIdentityDocumentController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<PatientIdentityDocumentFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Patient Identity Document",
            Description = "Melihat metadata filter patient identity document",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("PatientIdentityDocument", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new PatientIdentityDocumentFilterMetadataResponse
            {
                DefaultFilter = new PatientIdentityDocumentDefaultFilterResponse(),
                CustomPeriods = new List<PatientIdentityDocumentCustomPeriodOptionResponse>
                {
                    new() { Value = "today", Label = "Hari ini" },
                    new() { Value = "last7days", Label = "7 hari terakhir" },
                    new() { Value = "thismonth", Label = "Bulan ini" },
                    new() { Value = "lastmonth", Label = "Bulan lalu" }
                },
                RelationFilters = new List<PatientIdentityDocumentRelationFilterResponse>
                {
                    new()
                    {
                        Value = "patientId",
                        Label = "Patient",
                        Endpoint = "/api/v1/health-services/patient-management/master-data/patients/options"
                    }
                },
                SortOptions = new List<PatientIdentityDocumentSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "updateDateTime", Label = "Tanggal diperbarui" },
                    new() { Value = "patientName", Label = "Nama pasien" },
                    new() { Value = "medicalRecordNumber", Label = "Nomor rekam medis" },
                    new() { Value = "identityType", Label = "Tipe identitas" },
                    new() { Value = "identityNumber", Label = "Nomor identitas" },
                    new() { Value = "documentName", Label = "Nama dokumen" },
                    new() { Value = "issueDate", Label = "Tanggal terbit" },
                    new() { Value = "expiredDate", Label = "Tanggal kedaluwarsa" },
                    new() { Value = "isPrimary", Label = "Dokumen utama" },
                    new() { Value = "isVerified", Label = "Status verifikasi" },
                    new() { Value = "isFromKioskScan", Label = "Kiosk scan" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                CommonIdentityTypes = CommonIdentityTypes,
                ResetButtonLabel = "Reset"
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "PatientIdentityDocument.GetFilterMetadata",
                "Mengambil metadata filter patient identity document.",
                result
            );

            return Ok(ApiResponse<PatientIdentityDocumentFilterMetadataResponse>.Ok(
                result,
                "Metadata filter patient identity document berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<PatientIdentityDocumentSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Patient Identity Document",
            Description = "Melihat ringkasan patient identity document",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("PatientIdentityDocument", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var today = DateTime.UtcNow.Date;
            var query = BuildBaseQuery();

            var result = new PatientIdentityDocumentSummaryResponse
            {
                TotalDocument = await query.CountAsync(),
                ActiveDocument = await query.CountAsync(x => x.IsActive),
                InactiveDocument = await query.CountAsync(x => !x.IsActive),
                PrimaryDocument = await query.CountAsync(x => x.IsPrimary),
                VerifiedDocument = await query.CountAsync(x => x.IsVerified),
                UnverifiedDocument = await query.CountAsync(x => !x.IsVerified),
                FromKioskScanDocument = await query.CountAsync(x => x.IsFromKioskScan),
                ExpiredDocument = await query.CountAsync(x =>
                    x.ExpiredDate.HasValue &&
                    x.ExpiredDate.Value.Date < today),
                WithFileDocument = await query.CountAsync(x =>
                    x.FilePath != null &&
                    x.FilePath != string.Empty)
            };

            return Ok(ApiResponse<PatientIdentityDocumentSummaryResponse>.Ok(
                result,
                "Ringkasan patient identity document berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponsePatientIdentityDocumentPagedResult>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Patient Identity Document",
            Description = "Melihat data patient identity document",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("PatientIdentityDocument", "Read")]
        public async Task<IActionResult> GetPatientIdentityDocuments(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? patientId,
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
            query = ApplyRelationFilter(query, patientId);
            query = ApplyStandardFilter(query, isActive, search);

            var totalData = await query.CountAsync();

            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var actorIds = entities
                .SelectMany(x => new[] { x.CreateBy, x.UpdateBy })
                .Concat(entities
                    .Where(x => x.VerifiedByUserId.HasValue)
                    .Select(x => x.VerifiedByUserId!.Value));

            var actorNames = await GetActorNameMapAsync(actorIds);

            var items = entities
                .Select(x => MapResponse(x, actorNames))
                .ToList();

            var result = new ResponsePatientIdentityDocumentPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponsePatientIdentityDocumentPagedResult>.Ok(
                result,
                "Data patient identity document berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<PatientIdentityDocumentOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Patient Identity Document",
            Description = "Melihat data pilihan patient identity document",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("PatientIdentityDocument", "Read")]
        public async Task<IActionResult> GetPatientIdentityDocumentOptions(
            [FromQuery] bool onlyActive = true,
            [FromQuery] Guid? patientId = null,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();

            query = ApplyRelationFilter(query, patientId);
            query = ApplyStandardFilter(
                query,
                onlyActive ? true : null,
                search
            );

            var totalData = await query.CountAsync();

            var entities = await query
                .OrderByDescending(x => x.IsPrimary)
                .ThenByDescending(x => x.IsVerified)
                .ThenBy(x => x.Patient != null ? x.Patient.FullName : string.Empty)
                .ThenBy(x => x.IdentityType)
                .ThenBy(x => x.IdentityNumber)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = entities
                .Select(MapOptionResponse)
                .ToList();

            var result = new PatientIdentityDocumentOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<PatientIdentityDocumentOptionPagedResponse>.Ok(
                result,
                "Data pilihan patient identity document berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientIdentityDocumentDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Patient Identity Document",
            Description = "Melihat detail patient identity document",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("PatientIdentityDocument", "Read")]
        public async Task<IActionResult> GetPatientIdentityDocumentById(Guid id)
        {
            var entity = await BuildBaseQuery()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Patient identity document tidak ditemukan."
                ));
            }

            var actorIds = new List<Guid>
            {
                entity.CreateBy,
                entity.UpdateBy
            };

            if (entity.VerifiedByUserId.HasValue)
            {
                actorIds.Add(entity.VerifiedByUserId.Value);
            }

            var actorNames = await GetActorNameMapAsync(actorIds);

            var data = MapDetailResponse(entity, actorNames);

            NormalizeActorInfo(data);

            return Ok(ApiResponse<PatientIdentityDocumentDetailResponse>.Ok(
                data,
                "Detail patient identity document berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<PatientIdentityDocumentCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(
            "Create",
            "Create Patient Identity Document",
            Description = "Membuat data patient identity document",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("PatientIdentityDocument", "Create")]
        public async Task<IActionResult> CreatePatientIdentityDocument(
            [FromBody] CreatePatientIdentityDocumentRequest request)
        {
            var validation = await ValidateRequestAsync(
                excludeId: null,
                request: request
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data patient identity document tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var isPrimary = request.IsPrimary || !await HasAnyActiveDocumentAsync(request.PatientId);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                if (isPrimary)
                {
                    await UnsetOtherPrimaryAsync(
                        patientId: request.PatientId,
                        exceptId: null,
                        now: now,
                        actorUserId: actorUserId
                    );
                }

                var entity = new MstPatientIdentityDocument
                {
                    Id = Guid.NewGuid(),
                    PatientId = request.PatientId,
                    IdentityType = request.IdentityType.Trim(),
                    IdentityNumber = request.IdentityNumber.Trim(),
                    DocumentName = NormalizeNullableString(request.DocumentName),
                    FilePath = NormalizeNullableString(request.FilePath),
                    FileContentType = NormalizeNullableString(request.FileContentType),
                    IssuedBy = NormalizeNullableString(request.IssuedBy),
                    IssueDate = request.IssueDate,
                    ExpiredDate = request.ExpiredDate,
                    IsPrimary = isPrimary,
                    IsVerified = request.IsVerified,
                    VerifiedByUserId = request.IsVerified ? actorUserId : null,
                    VerifiedAt = request.IsVerified ? now : null,
                    VerificationNote = NormalizeNullableString(request.VerificationNote),
                    IsFromKioskScan = request.IsFromKioskScan,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                _dbContext.Set<MstPatientIdentityDocument>().Add(entity);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                var result = await BuildCreateResponseAsync(entity.Id);

                await _loggerService.InfoAsync(
                    LogCategory,
                    "PatientIdentityDocument.CreatePatientIdentityDocument",
                    "Membuat data patient identity document.",
                    result
                );

                return Ok(ApiResponse<PatientIdentityDocumentCreateResponse>.Ok(
                    result,
                    "Patient identity document berhasil dibuat."
                ));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "PatientIdentityDocument.CreatePatientIdentityDocument",
                    "Gagal membuat data patient identity document.",
                    ex
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat membuat patient identity document."
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
            "Update Patient Identity Document",
            Description = "Mengubah data patient identity document",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("PatientIdentityDocument", "Update")]
        public async Task<IActionResult> UpdatePatientIdentityDocument(
            Guid id,
            [FromBody] UpdatePatientIdentityDocumentRequest request)
        {
            var entity = await _dbContext.Set<MstPatientIdentityDocument>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Patient identity document tidak ditemukan."
                ));
            }

            if (request.IsPrimary && !request.IsActive)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Patient identity document primary harus aktif."
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
                    validation.ErrorMessage ?? "Data patient identity document tidak valid."
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

                var wasVerified = entity.IsVerified;

                entity.PatientId = request.PatientId;
                entity.IdentityType = request.IdentityType.Trim();
                entity.IdentityNumber = request.IdentityNumber.Trim();
                entity.DocumentName = NormalizeNullableString(request.DocumentName);
                entity.FilePath = NormalizeNullableString(request.FilePath);
                entity.FileContentType = NormalizeNullableString(request.FileContentType);
                entity.IssuedBy = NormalizeNullableString(request.IssuedBy);
                entity.IssueDate = request.IssueDate;
                entity.ExpiredDate = request.ExpiredDate;
                entity.IsPrimary = request.IsPrimary;
                entity.IsVerified = request.IsVerified;
                entity.VerifiedByUserId = request.IsVerified
                    ? wasVerified && entity.VerifiedByUserId.HasValue ? entity.VerifiedByUserId : actorUserId
                    : null;
                entity.VerifiedAt = request.IsVerified
                    ? wasVerified && entity.VerifiedAt.HasValue ? entity.VerifiedAt : now
                    : null;
                entity.VerificationNote = NormalizeNullableString(request.VerificationNote);
                entity.IsFromKioskScan = request.IsFromKioskScan;
                entity.IsActive = request.IsActive;
                entity.UpdateDateTime = now;
                entity.UpdateBy = actorUserId;

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                await _loggerService.InfoAsync(
                    LogCategory,
                    "PatientIdentityDocument.UpdatePatientIdentityDocument",
                    "Mengubah data patient identity document.",
                    new
                    {
                        entity.Id,
                        entity.PatientId,
                        entity.IdentityType,
                        entity.IdentityNumber,
                        entity.IsPrimary,
                        entity.IsVerified,
                        entity.IsActive
                    }
                );

                return Ok(ApiResponse<object>.Ok(
                    null,
                    "Patient identity document berhasil diperbarui."
                ));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "PatientIdentityDocument.UpdatePatientIdentityDocument",
                    "Gagal mengubah data patient identity document.",
                    ex
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat mengubah patient identity document."
                    )
                );
            }
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Patient Identity Document Status",
            Description = "Mengubah status patient identity document",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("PatientIdentityDocument", "Update")]
        public async Task<IActionResult> UpdatePatientIdentityDocumentStatus(
            Guid id,
            [FromBody] UpdatePatientIdentityDocumentStatusRequest request)
        {
            var entity = await _dbContext.Set<MstPatientIdentityDocument>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Patient identity document tidak ditemukan."
                ));
            }

            if (!request.IsActive && entity.IsPrimary)
            {
                entity.IsPrimary = false;
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status patient identity document berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Delete",
            "Delete Patient Identity Document",
            Description = "Menghapus data patient identity document",
            AccessType = AccessTypes.Delete,
            SortOrder = 4
        )]
        [AccessPermission("PatientIdentityDocument", "Delete")]
        public async Task<IActionResult> DeletePatientIdentityDocument(
            Guid id,
            [FromBody] DeletePatientIdentityDocumentRequest? request = null)
        {
            var entity = await _dbContext.Set<MstPatientIdentityDocument>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Patient identity document tidak ditemukan."
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
                entity.VerificationNote = request.DeleteReason.Trim();
            }

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "PatientIdentityDocument.DeletePatientIdentityDocument",
                "Menghapus data patient identity document.",
                new
                {
                    entity.Id,
                    entity.PatientId,
                    entity.IdentityType,
                    entity.IdentityNumber,
                    entity.DeleteDateTime
                }
            );

            return Ok(ApiResponse<object>.Ok(
                null,
                "Patient identity document berhasil dihapus."
            ));
        }

        private IQueryable<MstPatientIdentityDocument> BuildBaseQuery()
        {
            return _dbContext.Set<MstPatientIdentityDocument>()
                .AsNoTracking()
                .Include(x => x.Patient)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstPatientIdentityDocument> ApplyDateFilter(
            IQueryable<MstPatientIdentityDocument> query,
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

        private static IQueryable<MstPatientIdentityDocument> ApplyRelationFilter(
            IQueryable<MstPatientIdentityDocument> query,
            Guid? patientId)
        {
            var normalizedPatientId = NormalizeNullableGuid(patientId);

            if (normalizedPatientId.HasValue)
            {
                query = query.Where(x => x.PatientId == normalizedPatientId.Value);
            }

            return query;
        }

        private static IQueryable<MstPatientIdentityDocument> ApplyStandardFilter(
            IQueryable<MstPatientIdentityDocument> query,
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
                    x.IdentityType.ToLower().Contains(keyword) ||
                    x.IdentityNumber.ToLower().Contains(keyword) ||
                    (x.DocumentName != null && x.DocumentName.ToLower().Contains(keyword)) ||
                    (x.FileContentType != null && x.FileContentType.ToLower().Contains(keyword)) ||
                    (x.IssuedBy != null && x.IssuedBy.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.PatientCode.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.MedicalRecordNumber.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.FullName.ToLower().Contains(keyword)));
            }

            return query;
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            CreatePatientIdentityDocumentRequest request)
        {
            if (request.PatientId == Guid.Empty)
            {
                return (false, "Patient wajib dipilih.");
            }

            if (string.IsNullOrWhiteSpace(request.IdentityType))
            {
                return (false, "Tipe identitas wajib diisi.");
            }

            if (string.IsNullOrWhiteSpace(request.IdentityNumber))
            {
                return (false, "Nomor identitas wajib diisi.");
            }

            if (request.IssueDate.HasValue &&
                request.ExpiredDate.HasValue &&
                request.ExpiredDate.Value.Date < request.IssueDate.Value.Date)
            {
                return (false, "Tanggal kedaluwarsa tidak boleh lebih kecil dari tanggal terbit.");
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

            var normalizedType = request.IdentityType.Trim().ToLower();
            var normalizedNumber = request.IdentityNumber.Trim().ToLower();

            var duplicateDocument = await _dbContext.Set<MstPatientIdentityDocument>()
                .AsNoTracking()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.IdentityType.ToLower() == normalizedType &&
                    x.IdentityNumber.ToLower() == normalizedNumber &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateDocument)
            {
                return (false, "Dokumen identitas dengan tipe dan nomor tersebut sudah digunakan.");
            }

            return (true, null);
        }

        private async Task<bool> HasAnyActiveDocumentAsync(Guid patientId)
        {
            return await _dbContext.Set<MstPatientIdentityDocument>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.PatientId == patientId &&
                    x.IsActive &&
                    !x.IsDelete);
        }

        private async Task UnsetOtherPrimaryAsync(
            Guid patientId,
            Guid? exceptId,
            DateTime now,
            Guid actorUserId)
        {
            var query = _dbContext.Set<MstPatientIdentityDocument>()
                .Where(x =>
                    !x.IsDelete &&
                    x.PatientId == patientId &&
                    x.IsPrimary);

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

        private async Task<PatientIdentityDocumentCreateResponse> BuildCreateResponseAsync(Guid id)
        {
            var entity = await BuildBaseQuery()
                .FirstAsync(x => x.Id == id);

            return new PatientIdentityDocumentCreateResponse
            {
                Id = entity.Id,
                PatientId = entity.PatientId,
                PatientName = entity.Patient?.FullName ?? string.Empty,
                IdentityType = entity.IdentityType,
                IdentityNumber = entity.IdentityNumber,
                IsPrimary = entity.IsPrimary,
                IsVerified = entity.IsVerified,
                IsActive = entity.IsActive
            };
        }

        private static PatientIdentityDocumentResponse MapResponse(
            MstPatientIdentityDocument entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new PatientIdentityDocumentResponse
            {
                Id = entity.Id,
                PatientId = entity.PatientId,
                PatientCode = entity.Patient?.PatientCode ?? string.Empty,
                MedicalRecordNumber = entity.Patient?.MedicalRecordNumber ?? string.Empty,
                PatientName = entity.Patient?.FullName ?? string.Empty,
                IdentityType = entity.IdentityType,
                IdentityNumber = entity.IdentityNumber,
                DocumentName = entity.DocumentName,
                FilePath = entity.FilePath,
                FileContentType = entity.FileContentType,
                IssuedBy = entity.IssuedBy,
                IssueDate = entity.IssueDate,
                ExpiredDate = entity.ExpiredDate,
                IsPrimary = entity.IsPrimary,
                IsVerified = entity.IsVerified,
                VerifiedByUserId = entity.VerifiedByUserId,
                VerifiedByUserName = entity.VerifiedByUserId.HasValue
                    ? GetActorName(actorNames, entity.VerifiedByUserId.Value)
                    : null,
                VerifiedAt = entity.VerifiedAt,
                IsFromKioskScan = entity.IsFromKioskScan,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy),
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };
        }

        private static PatientIdentityDocumentDetailResponse MapDetailResponse(
            MstPatientIdentityDocument entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            var response = new PatientIdentityDocumentDetailResponse
            {
                Id = entity.Id,
                PatientId = entity.PatientId,
                PatientCode = entity.Patient?.PatientCode ?? string.Empty,
                MedicalRecordNumber = entity.Patient?.MedicalRecordNumber ?? string.Empty,
                PatientName = entity.Patient?.FullName ?? string.Empty,
                IdentityType = entity.IdentityType,
                IdentityNumber = entity.IdentityNumber,
                DocumentName = entity.DocumentName,
                FilePath = entity.FilePath,
                FileContentType = entity.FileContentType,
                IssuedBy = entity.IssuedBy,
                IssueDate = entity.IssueDate,
                ExpiredDate = entity.ExpiredDate,
                IsPrimary = entity.IsPrimary,
                IsVerified = entity.IsVerified,
                VerifiedByUserId = entity.VerifiedByUserId,
                VerifiedByUserName = entity.VerifiedByUserId.HasValue
                    ? GetActorName(actorNames, entity.VerifiedByUserId.Value)
                    : null,
                VerifiedAt = entity.VerifiedAt,
                VerificationNote = entity.VerificationNote,
                IsFromKioskScan = entity.IsFromKioskScan,
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

        private static PatientIdentityDocumentOptionResponse MapOptionResponse(MstPatientIdentityDocument entity)
        {
            return new PatientIdentityDocumentOptionResponse
            {
                Id = entity.Id,
                PatientId = entity.PatientId,
                PatientCode = entity.Patient?.PatientCode ?? string.Empty,
                MedicalRecordNumber = entity.Patient?.MedicalRecordNumber ?? string.Empty,
                PatientName = entity.Patient?.FullName ?? string.Empty,
                IdentityType = entity.IdentityType,
                IdentityNumber = entity.IdentityNumber,
                DocumentName = entity.DocumentName,
                IsPrimary = entity.IsPrimary,
                IsVerified = entity.IsVerified
            };
        }

        private static IOrderedQueryable<MstPatientIdentityDocument> ApplySorting(
            IQueryable<MstPatientIdentityDocument> query,
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

                "identitytype" => isDescending
                    ? query.OrderByDescending(x => x.IdentityType)
                    : query.OrderBy(x => x.IdentityType),

                "identitynumber" => isDescending
                    ? query.OrderByDescending(x => x.IdentityNumber)
                    : query.OrderBy(x => x.IdentityNumber),

                "documentname" => isDescending
                    ? query.OrderByDescending(x => x.DocumentName)
                    : query.OrderBy(x => x.DocumentName),

                "issuedate" => isDescending
                    ? query.OrderByDescending(x => x.IssueDate)
                    : query.OrderBy(x => x.IssueDate),

                "expireddate" => isDescending
                    ? query.OrderByDescending(x => x.ExpiredDate)
                    : query.OrderBy(x => x.ExpiredDate),

                "isprimary" => isDescending
                    ? query.OrderByDescending(x => x.IsPrimary).ThenBy(x => x.Patient != null ? x.Patient.FullName : string.Empty)
                    : query.OrderBy(x => x.IsPrimary).ThenBy(x => x.Patient != null ? x.Patient.FullName : string.Empty),

                "isverified" => isDescending
                    ? query.OrderByDescending(x => x.IsVerified).ThenBy(x => x.Patient != null ? x.Patient.FullName : string.Empty)
                    : query.OrderBy(x => x.IsVerified).ThenBy(x => x.Patient != null ? x.Patient.FullName : string.Empty),

                "isfromkioskscan" => isDescending
                    ? query.OrderByDescending(x => x.IsFromKioskScan).ThenBy(x => x.Patient != null ? x.Patient.FullName : string.Empty)
                    : query.OrderBy(x => x.IsFromKioskScan).ThenBy(x => x.Patient != null ? x.Patient.FullName : string.Empty),

                "isactive" => isDescending
                    ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.Patient != null ? x.Patient.FullName : string.Empty)
                    : query.OrderBy(x => x.IsActive).ThenBy(x => x.Patient != null ? x.Patient.FullName : string.Empty),

                _ => isDescending
                    ? query.OrderByDescending(x => x.CreateDateTime).ThenByDescending(x => x.IdentityType)
                    : query.OrderBy(x => x.CreateDateTime).ThenBy(x => x.IdentityType)
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

        private static void NormalizeActorInfo(PatientIdentityDocumentResponse data)
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

            if (!data.VerifiedByUserId.HasValue || data.VerifiedByUserId.Value == Guid.Empty)
            {
                data.VerifiedByUserId = null;
                data.VerifiedByUserName = null;
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
