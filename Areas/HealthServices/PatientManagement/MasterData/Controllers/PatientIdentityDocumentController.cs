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
        moduleCode: "HEALTH_SERVICE_PATIENT_MANAGEMENT",
        moduleName: "Health Service Patient Management",
        displayName: "Patient Identity Document",
        AreaName = "HealthServices",
        ControllerName = "PatientIdentityDocument",
        Description = "Health service patient management master data patient identity document",
        SortOrder = 2
    )]
    [Tags("Health Services / Patient Management / Patient Identity Document")]
    public class PatientIdentityDocumentController : ControllerBase
    {
        private const string LogCategory = "HealthServices.PatientManagement";

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
        [AccessAction("Read", "Read Patient Identity Document", Description = "Melihat data patient identity document", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientIdentityDocument", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new PatientIdentityDocumentFilterMetadataResponse
            {
                DefaultFilter = new PatientIdentityDocumentDefaultFilterResponse(),
                SortOptions = new List<PatientIdentityDocumentSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "patientName", Label = "Nama pasien" },
                    new() { Value = "medicalRecordNumber", Label = "Nomor rekam medis" },
                    new() { Value = "identityType", Label = "Tipe identitas" },
                    new() { Value = "identityNumber", Label = "Nomor identitas" },
                    new() { Value = "documentName", Label = "Nama dokumen" },
                    new() { Value = "issueDate", Label = "Tanggal terbit" },
                    new() { Value = "expiredDate", Label = "Tanggal kedaluwarsa" },
                    new() { Value = "isPrimary", Label = "Dokumen utama" },
                    new() { Value = "isVerified", Label = "Status verifikasi" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                CommonIdentityTypes = new List<string>
                {
                    "KTP",
                    "NIK",
                    "KIA",
                    "Passport",
                    "SIM",
                    "BirthCertificate",
                    "StudentCard",
                    "Other"
                }
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
        [AccessAction("Read", "Read Patient Identity Document", Description = "Melihat data patient identity document", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientIdentityDocument", "Read")]
        public async Task<IActionResult> GetSummary([FromQuery] Guid? patientId)
        {
            var today = DateTime.UtcNow.Date;

            var query = _dbContext.Set<MstPatientIdentityDocument>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (patientId.HasValue && patientId.Value != Guid.Empty)
                query = query.Where(x => x.PatientId == patientId.Value);

            var result = new PatientIdentityDocumentSummaryResponse
            {
                TotalDocument = await query.CountAsync(),
                ActiveDocument = await query.CountAsync(x => x.IsActive),
                InactiveDocument = await query.CountAsync(x => !x.IsActive),
                PrimaryDocument = await query.CountAsync(x => x.IsPrimary),
                VerifiedDocument = await query.CountAsync(x => x.IsVerified),
                UnverifiedDocument = await query.CountAsync(x => !x.IsVerified),
                FromKioskScanDocument = await query.CountAsync(x => x.IsFromKioskScan),
                ExpiredDocument = await query.CountAsync(x => x.ExpiredDate.HasValue && x.ExpiredDate.Value.Date < today)
            };

            return Ok(ApiResponse<PatientIdentityDocumentSummaryResponse>.Ok(
                result,
                "Ringkasan patient identity document berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponsePatientIdentityDocumentPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Identity Document", Description = "Melihat data patient identity document", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientIdentityDocument", "Read")]
        public async Task<IActionResult> GetPatientIdentityDocuments(
            [FromQuery] string? search,
            [FromQuery] Guid? patientId,
            [FromQuery] string? identityType,
            [FromQuery] bool? isPrimary,
            [FromQuery] bool? isVerified,
            [FromQuery] bool? isFromKioskScan,
            [FromQuery] bool? isActive,
            [FromQuery] DateTime? expiredFrom,
            [FromQuery] DateTime? expiredTo,
            [FromQuery] string? sortBy = "createDateTime",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = _dbContext.Set<MstPatientIdentityDocument>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            query = ApplyFilters(
                query,
                search,
                patientId,
                identityType,
                isPrimary,
                isVerified,
                isFromKioskScan,
                isActive,
                expiredFrom,
                expiredTo
            );

            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new PatientIdentityDocumentResponse
                {
                    Id = x.Id,
                    PatientId = x.PatientId,
                    PatientCode = x.Patient != null ? x.Patient.PatientCode : string.Empty,
                    MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty,
                    PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                    IdentityType = x.IdentityType,
                    IdentityNumber = x.IdentityNumber,
                    DocumentName = x.DocumentName,
                    FilePath = x.FilePath,
                    FileContentType = x.FileContentType,
                    IssuedBy = x.IssuedBy,
                    IssueDate = x.IssueDate,
                    ExpiredDate = x.ExpiredDate,
                    IsPrimary = x.IsPrimary,
                    IsVerified = x.IsVerified,
                    VerifiedByUserId = x.VerifiedByUserId,
                    VerifiedAt = x.VerifiedAt,
                    IsFromKioskScan = x.IsFromKioskScan,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

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
        [ProducesResponseType(typeof(ApiResponse<List<PatientIdentityDocumentOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Identity Document", Description = "Melihat data patient identity document", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientIdentityDocument", "Read")]
        public async Task<IActionResult> GetPatientIdentityDocumentOptions(
            [FromQuery] Guid? patientId,
            [FromQuery] string? identityType,
            [FromQuery] bool? isPrimary,
            [FromQuery] bool? isVerified,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null)
        {
            var query = _dbContext.Set<MstPatientIdentityDocument>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (onlyActive)
                query = query.Where(x => x.IsActive);

            if (patientId.HasValue && patientId.Value != Guid.Empty)
                query = query.Where(x => x.PatientId == patientId.Value);

            if (!string.IsNullOrWhiteSpace(identityType))
            {
                var normalizedType = identityType.Trim().ToLower();
                query = query.Where(x => x.IdentityType.ToLower() == normalizedType);
            }

            if (isPrimary.HasValue)
                query = query.Where(x => x.IsPrimary == isPrimary.Value);

            if (isVerified.HasValue)
                query = query.Where(x => x.IsVerified == isVerified.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.IdentityType.ToLower().Contains(keyword) ||
                    x.IdentityNumber.ToLower().Contains(keyword) ||
                    (x.DocumentName != null && x.DocumentName.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.FullName.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.MedicalRecordNumber.ToLower().Contains(keyword)));
            }

            var data = await query
                .OrderByDescending(x => x.IsPrimary)
                .ThenByDescending(x => x.IsVerified)
                .ThenBy(x => x.IdentityType)
                .ThenBy(x => x.IdentityNumber)
                .Take(100)
                .Select(x => new PatientIdentityDocumentOptionResponse
                {
                    Id = x.Id,
                    PatientId = x.PatientId,
                    PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                    MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty,
                    IdentityType = x.IdentityType,
                    IdentityNumber = x.IdentityNumber,
                    DocumentName = x.DocumentName,
                    IsPrimary = x.IsPrimary,
                    IsVerified = x.IsVerified
                })
                .ToListAsync();

            return Ok(ApiResponse<List<PatientIdentityDocumentOptionResponse>>.Ok(
                data,
                "Data pilihan patient identity document berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientIdentityDocumentDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Patient Identity Document", Description = "Melihat data patient identity document", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientIdentityDocument", "Read")]
        public async Task<IActionResult> GetPatientIdentityDocumentById(Guid id)
        {
            var data = await _dbContext.Set<MstPatientIdentityDocument>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new PatientIdentityDocumentDetailResponse
                {
                    Id = x.Id,
                    PatientId = x.PatientId,
                    PatientCode = x.Patient != null ? x.Patient.PatientCode : string.Empty,
                    MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty,
                    PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                    IdentityType = x.IdentityType,
                    IdentityNumber = x.IdentityNumber,
                    DocumentName = x.DocumentName,
                    FilePath = x.FilePath,
                    FileContentType = x.FileContentType,
                    IssuedBy = x.IssuedBy,
                    IssueDate = x.IssueDate,
                    ExpiredDate = x.ExpiredDate,
                    IsPrimary = x.IsPrimary,
                    IsVerified = x.IsVerified,
                    VerifiedByUserId = x.VerifiedByUserId,
                    VerifiedAt = x.VerifiedAt,
                    VerificationNote = x.VerificationNote,
                    IsFromKioskScan = x.IsFromKioskScan,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Patient identity document tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<PatientIdentityDocumentDetailResponse>.Ok(
                data,
                "Detail patient identity document berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<PatientIdentityDocumentCreateResponse>), StatusCodes.Status200OK)]
        [AccessAction("Create", "Create Patient Identity Document", Description = "Membuat data patient identity document", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("PatientIdentityDocument", "Create")]
        public async Task<IActionResult> CreatePatientIdentityDocument([FromBody] CreatePatientIdentityDocumentRequest request)
        {
            var validation = await ValidateRequestAsync(
                excludeId: null,
                request.PatientId,
                request.IdentityType,
                request.IdentityNumber,
                request.IssueDate,
                request.ExpiredDate
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

            if (isPrimary)
                await ClearPrimaryDocumentAsync(request.PatientId, excludeId: null);

            var entity = new MstPatientIdentityDocument
            {
                Id = Guid.NewGuid(),
                PatientId = request.PatientId,
                IdentityType = request.IdentityType.Trim(),
                IdentityNumber = request.IdentityNumber.Trim(),
                DocumentName = NormalizeNullableText(request.DocumentName),
                FilePath = NormalizeNullableText(request.FilePath),
                FileContentType = NormalizeNullableText(request.FileContentType),
                IssuedBy = NormalizeNullableText(request.IssuedBy),
                IssueDate = request.IssueDate,
                ExpiredDate = request.ExpiredDate,
                IsPrimary = isPrimary,
                IsVerified = request.IsVerified,
                VerifiedByUserId = request.IsVerified ? actorUserId : null,
                VerifiedAt = request.IsVerified ? now : null,
                VerificationNote = NormalizeNullableText(request.VerificationNote),
                IsFromKioskScan = request.IsFromKioskScan,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstPatientIdentityDocument>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var response = new PatientIdentityDocumentCreateResponse
            {
                Id = entity.Id,
                PatientId = entity.PatientId,
                IdentityType = entity.IdentityType,
                IdentityNumber = entity.IdentityNumber,
                IsPrimary = entity.IsPrimary,
                IsVerified = entity.IsVerified,
                IsActive = entity.IsActive
            };

            return Ok(ApiResponse<PatientIdentityDocumentCreateResponse>.Ok(
                response,
                "Patient identity document berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Patient Identity Document", Description = "Mengubah data patient identity document", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("PatientIdentityDocument", "Update")]
        public async Task<IActionResult> UpdatePatientIdentityDocument(Guid id, [FromBody] UpdatePatientIdentityDocumentRequest request)
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

            var validation = await ValidateRequestAsync(
                excludeId: id,
                request.PatientId,
                request.IdentityType,
                request.IdentityNumber,
                request.IssueDate,
                request.ExpiredDate
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data patient identity document tidak valid."
                ));
            }

            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;
            var isPrimary = request.IsPrimary;

            if (isPrimary)
                await ClearPrimaryDocumentAsync(request.PatientId, excludeId: id);

            entity.PatientId = request.PatientId;
            entity.IdentityType = request.IdentityType.Trim();
            entity.IdentityNumber = request.IdentityNumber.Trim();
            entity.DocumentName = NormalizeNullableText(request.DocumentName);
            entity.FilePath = NormalizeNullableText(request.FilePath);
            entity.FileContentType = NormalizeNullableText(request.FileContentType);
            entity.IssuedBy = NormalizeNullableText(request.IssuedBy);
            entity.IssueDate = request.IssueDate;
            entity.ExpiredDate = request.ExpiredDate;
            entity.IsPrimary = isPrimary;
            entity.IsVerified = request.IsVerified;
            entity.VerifiedByUserId = request.IsVerified ? entity.VerifiedByUserId ?? actorUserId : null;
            entity.VerifiedAt = request.IsVerified ? entity.VerifiedAt ?? now : null;
            entity.VerificationNote = NormalizeNullableText(request.VerificationNote);
            entity.IsFromKioskScan = request.IsFromKioskScan;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Patient identity document berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/verify")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Verify Patient Identity Document", Description = "Verifikasi data patient identity document", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("PatientIdentityDocument", "Update")]
        public async Task<IActionResult> VerifyPatientIdentityDocument(Guid id, [FromBody] VerifyPatientIdentityDocumentRequest request)
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

            var actorUserId = GetCurrentUserId();

            entity.IsVerified = request.IsVerified;
            entity.VerificationNote = NormalizeNullableText(request.VerificationNote);
            entity.VerifiedByUserId = request.IsVerified ? actorUserId : null;
            entity.VerifiedAt = request.IsVerified ? DateTime.UtcNow : null;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                request.IsVerified
                    ? "Patient identity document berhasil diverifikasi."
                    : "Verifikasi patient identity document berhasil dibatalkan."
            ));
        }

        [HttpPatch("{id:guid}/set-primary")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Set Primary Patient Identity Document", Description = "Mengatur dokumen identitas utama patient", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("PatientIdentityDocument", "Update")]
        public async Task<IActionResult> SetPrimaryPatientIdentityDocument(Guid id)
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

            if (!entity.IsActive)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Dokumen tidak aktif tidak dapat dijadikan dokumen utama."
                ));
            }

            await ClearPrimaryDocumentAsync(entity.PatientId, excludeId: id);

            entity.IsPrimary = true;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Patient identity document berhasil dijadikan dokumen utama."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Patient Identity Document", Description = "Menghapus data patient identity document", AccessType = AccessTypes.Delete, SortOrder = 6)]
        [AccessPermission("PatientIdentityDocument", "Delete")]
        public async Task<IActionResult> DeletePatientIdentityDocument(Guid id)
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

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.IsPrimary = false;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Patient identity document berhasil dihapus."
            ));
        }

        private static IQueryable<MstPatientIdentityDocument> ApplyFilters(
            IQueryable<MstPatientIdentityDocument> query,
            string? search,
            Guid? patientId,
            string? identityType,
            bool? isPrimary,
            bool? isVerified,
            bool? isFromKioskScan,
            bool? isActive,
            DateTime? expiredFrom,
            DateTime? expiredTo)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.IdentityType.ToLower().Contains(keyword) ||
                    x.IdentityNumber.ToLower().Contains(keyword) ||
                    (x.DocumentName != null && x.DocumentName.ToLower().Contains(keyword)) ||
                    (x.IssuedBy != null && x.IssuedBy.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.PatientCode.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.MedicalRecordNumber.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.FullName.ToLower().Contains(keyword)));
            }

            if (patientId.HasValue && patientId.Value != Guid.Empty)
                query = query.Where(x => x.PatientId == patientId.Value);

            if (!string.IsNullOrWhiteSpace(identityType))
            {
                var normalizedType = identityType.Trim().ToLower();
                query = query.Where(x => x.IdentityType.ToLower() == normalizedType);
            }

            if (isPrimary.HasValue)
                query = query.Where(x => x.IsPrimary == isPrimary.Value);

            if (isVerified.HasValue)
                query = query.Where(x => x.IsVerified == isVerified.Value);

            if (isFromKioskScan.HasValue)
                query = query.Where(x => x.IsFromKioskScan == isFromKioskScan.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (expiredFrom.HasValue)
                query = query.Where(x => x.ExpiredDate.HasValue && x.ExpiredDate.Value.Date >= expiredFrom.Value.Date);

            if (expiredTo.HasValue)
                query = query.Where(x => x.ExpiredDate.HasValue && x.ExpiredDate.Value.Date <= expiredTo.Value.Date);

            return query;
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            Guid patientId,
            string identityType,
            string identityNumber,
            DateTime? issueDate,
            DateTime? expiredDate)
        {
            if (patientId == Guid.Empty)
                return (false, "Patient wajib dipilih.");

            if (string.IsNullOrWhiteSpace(identityType))
                return (false, "Tipe identitas wajib diisi.");

            if (string.IsNullOrWhiteSpace(identityNumber))
                return (false, "Nomor identitas wajib diisi.");

            if (issueDate.HasValue && expiredDate.HasValue && expiredDate.Value.Date < issueDate.Value.Date)
                return (false, "Tanggal kedaluwarsa tidak boleh lebih kecil dari tanggal terbit.");

            var patientExists = await _dbContext.Set<MstPatient>()
                .AnyAsync(x => x.Id == patientId && x.IsActive && !x.IsDelete);

            if (!patientExists)
                return (false, "Patient tidak valid atau tidak aktif.");

            var normalizedType = identityType.Trim().ToLower();
            var normalizedNumber = identityNumber.Trim().ToLower();

            var duplicateDocument = await _dbContext.Set<MstPatientIdentityDocument>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.IdentityType.ToLower() == normalizedType &&
                    x.IdentityNumber.ToLower() == normalizedNumber &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateDocument)
                return (false, "Dokumen identitas dengan tipe dan nomor tersebut sudah digunakan.");

            return (true, null);
        }

        private async Task<bool> HasAnyActiveDocumentAsync(Guid patientId)
        {
            return await _dbContext.Set<MstPatientIdentityDocument>()
                .AnyAsync(x => x.PatientId == patientId && x.IsActive && !x.IsDelete);
        }

        private async Task ClearPrimaryDocumentAsync(Guid patientId, Guid? excludeId)
        {
            var existingPrimaries = await _dbContext.Set<MstPatientIdentityDocument>()
                .Where(x =>
                    x.PatientId == patientId &&
                    x.IsPrimary &&
                    !x.IsDelete &&
                    (!excludeId.HasValue || x.Id != excludeId.Value))
                .ToListAsync();

            if (!existingPrimaries.Any())
                return;

            var actorUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            foreach (var item in existingPrimaries)
            {
                item.IsPrimary = false;
                item.UpdateDateTime = now;
                item.UpdateBy = actorUserId;
            }
        }

        private static IQueryable<MstPatientIdentityDocument> ApplySorting(
            IQueryable<MstPatientIdentityDocument> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "createDateTime").ToLowerInvariant() switch
            {
                "patientname" => isDesc
                    ? query.OrderByDescending(x => x.Patient != null ? x.Patient.FullName : "")
                    : query.OrderBy(x => x.Patient != null ? x.Patient.FullName : ""),

                "medicalrecordnumber" => isDesc
                    ? query.OrderByDescending(x => x.Patient != null ? x.Patient.MedicalRecordNumber : "")
                    : query.OrderBy(x => x.Patient != null ? x.Patient.MedicalRecordNumber : ""),

                "identitytype" => isDesc ? query.OrderByDescending(x => x.IdentityType) : query.OrderBy(x => x.IdentityType),
                "identitynumber" => isDesc ? query.OrderByDescending(x => x.IdentityNumber) : query.OrderBy(x => x.IdentityNumber),
                "documentname" => isDesc ? query.OrderByDescending(x => x.DocumentName) : query.OrderBy(x => x.DocumentName),
                "issuedate" => isDesc ? query.OrderByDescending(x => x.IssueDate) : query.OrderBy(x => x.IssueDate),
                "expireddate" => isDesc ? query.OrderByDescending(x => x.ExpiredDate) : query.OrderBy(x => x.ExpiredDate),
                "isprimary" => isDesc ? query.OrderByDescending(x => x.IsPrimary) : query.OrderBy(x => x.IsPrimary),
                "isverified" => isDesc ? query.OrderByDescending(x => x.IsVerified) : query.OrderBy(x => x.IsVerified),
                "isactive" => isDesc ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
                _ => isDesc
                    ? query.OrderByDescending(x => x.CreateDateTime).ThenByDescending(x => x.IdentityType)
                    : query.OrderBy(x => x.CreateDateTime).ThenBy(x => x.IdentityType)
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
    }
}