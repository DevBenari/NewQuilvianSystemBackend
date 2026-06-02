using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponsePatientClinicalDocumentPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs.PatientClinicalDocumentResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/clinical-management/patient-clinical-documents")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_CLINICAL",
        moduleName: "Health Service Clinical",
        displayName: "Patient Clinical Document",
        AreaName = "HealthServices",
        ControllerName = "PatientClinicalDocument",
        Description = "Dokumen klinis pasien dan berkas rekam medis klinis",
        SortOrder = 9
    )]
    [Tags("Health Services / Clinical Management / Patient Clinical Document")]
    public class PatientClinicalDocumentController : ControllerBase
    {
        private const string LogCategory = "HealthServices.Clinical";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public PatientClinicalDocumentController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<PatientClinicalDocumentFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Clinical Document", Description = "Melihat metadata filter dokumen klinis pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientClinicalDocument", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new PatientClinicalDocumentFilterMetadataResponse
            {
                DefaultFilter = new PatientClinicalDocumentDefaultFilterResponse(),
                SortOptions = new List<PatientClinicalDocumentSortOptionResponse>
                {
                    new() { Value = "documentDateTime", Label = "Tanggal dokumen" },
                    new() { Value = "clinicalDocumentNumber", Label = "Nomor dokumen" },
                    new() { Value = "documentTitle", Label = "Judul dokumen" },
                    new() { Value = "documentType", Label = "Tipe dokumen" },
                    new() { Value = "documentSource", Label = "Sumber dokumen" },
                    new() { Value = "documentStatus", Label = "Status dokumen" },
                    new() { Value = "confidentialityLevel", Label = "Level kerahasiaan" },
                    new() { Value = "isNeedReview", Label = "Butuh review" },
                    new() { Value = "isReviewed", Label = "Sudah direview" },
                    new() { Value = "isVerified", Label = "Terverifikasi" },
                    new() { Value = "isApproved", Label = "Disetujui" },
                    new() { Value = "isArchived", Label = "Diarsipkan" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                DocumentTypeOptions = BuildEnumOptions<PatientClinicalDocumentType>(),
                DocumentSourceOptions = BuildEnumOptions<PatientClinicalDocumentSource>(),
                DocumentStatusOptions = BuildEnumOptions<PatientClinicalDocumentStatus>(),
                ConfidentialityLevelOptions = BuildEnumOptions<PatientClinicalDocumentConfidentialityLevel>()
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "PatientClinicalDocument.GetFilterMetadata",
                "Mengambil metadata filter dokumen klinis pasien.",
                result
            );

            return Ok(ApiResponse<PatientClinicalDocumentFilterMetadataResponse>.Ok(
                result,
                "Metadata filter dokumen klinis pasien berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponsePatientClinicalDocumentPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Clinical Document", Description = "Melihat data dokumen klinis pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientClinicalDocument", "Read")]
        public async Task<IActionResult> GetClinicalDocuments(
            [FromQuery] string? search,
            [FromQuery] Guid? patientId,
            [FromQuery] Guid? encounterId,
            [FromQuery] Guid? queueId,
            [FromQuery] Guid? assessmentId,
            [FromQuery] Guid? consultationId,
            [FromQuery] Guid? patientDiagnosisId,
            [FromQuery] Guid? patientProcedureId,
            [FromQuery] Guid? doctorId,
            [FromQuery] Guid? serviceUnitId,
            [FromQuery] Guid? clinicId,
            [FromQuery] PatientClinicalDocumentType? documentType,
            [FromQuery] PatientClinicalDocumentSource? documentSource,
            [FromQuery] PatientClinicalDocumentStatus? documentStatus,
            [FromQuery] PatientClinicalDocumentConfidentialityLevel? confidentialityLevel,
            [FromQuery] bool? isConfidential,
            [FromQuery] bool? isPatientVisible,
            [FromQuery] bool? isShareable,
            [FromQuery] bool? isExternalDocument,
            [FromQuery] bool? isPartOfMedicalRecord,
            [FromQuery] bool? isLegalDocument,
            [FromQuery] bool? isNeedReview,
            [FromQuery] bool? isReviewed,
            [FromQuery] bool? isVerified,
            [FromQuery] bool? isApproved,
            [FromQuery] bool? isArchived,
            [FromQuery] bool? isActive,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? sortBy = "documentDateTime",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery().AsNoTracking();

            query = ApplyFilters(
                query,
                search,
                patientId,
                encounterId,
                queueId,
                assessmentId,
                consultationId,
                patientDiagnosisId,
                patientProcedureId,
                doctorId,
                serviceUnitId,
                clinicId,
                documentType,
                documentSource,
                documentStatus,
                confidentialityLevel,
                isConfidential,
                isPatientVisible,
                isShareable,
                isExternalDocument,
                isPartOfMedicalRecord,
                isLegalDocument,
                isNeedReview,
                isReviewed,
                isVerified,
                isApproved,
                isArchived,
                isActive,
                startDate,
                endDate
            );

            var totalData = await query.CountAsync();

            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = entities.Select(ToResponse).ToList();

            var result = new ResponsePatientClinicalDocumentPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponsePatientClinicalDocumentPagedResult>.Ok(
                result,
                "Data dokumen klinis pasien berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<PatientClinicalDocumentOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Clinical Document", Description = "Melihat pilihan dokumen klinis pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientClinicalDocument", "Read")]
        public async Task<IActionResult> GetClinicalDocumentOptions(
            [FromQuery] Guid? patientId,
            [FromQuery] Guid? encounterId,
            [FromQuery] Guid? consultationId,
            [FromQuery] PatientClinicalDocumentType? documentType,
            [FromQuery] bool onlyActive = true,
            [FromQuery] bool onlyMedicalRecord = false,
            [FromQuery] string? search = null)
        {
            var query = _dbContext.Set<TrxPatientClinicalDocument>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (onlyActive)
            {
                query = query.Where(x =>
                    x.IsActive &&
                    x.DocumentStatus != PatientClinicalDocumentStatus.Cancelled &&
                    x.DocumentStatus != PatientClinicalDocumentStatus.EnteredInError);
            }

            if (onlyMedicalRecord)
                query = query.Where(x => x.IsPartOfMedicalRecord);

            if (patientId.HasValue && patientId.Value != Guid.Empty)
                query = query.Where(x => x.PatientId == patientId.Value);

            if (encounterId.HasValue && encounterId.Value != Guid.Empty)
                query = query.Where(x => x.EncounterId == encounterId.Value);

            if (consultationId.HasValue && consultationId.Value != Guid.Empty)
                query = query.Where(x => x.ConsultationId == consultationId.Value);

            if (documentType.HasValue)
                query = query.Where(x => x.DocumentType == documentType.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.ClinicalDocumentNumber.ToLower().Contains(keyword) ||
                    x.DocumentTitle.ToLower().Contains(keyword) ||
                    x.FileName.ToLower().Contains(keyword) ||
                    (x.DocumentCode != null && x.DocumentCode.ToLower().Contains(keyword)) ||
                    (x.DocumentCategoryName != null && x.DocumentCategoryName.ToLower().Contains(keyword)) ||
                    (x.ExternalDocumentNumber != null && x.ExternalDocumentNumber.ToLower().Contains(keyword)) ||
                    (x.ExternalProviderName != null && x.ExternalProviderName.ToLower().Contains(keyword)));
            }

            var data = await query
                .OrderByDescending(x => x.DocumentDateTime)
                .Take(100)
                .Select(x => new PatientClinicalDocumentOptionResponse
                {
                    Id = x.Id,
                    ClinicalDocumentNumber = x.ClinicalDocumentNumber,
                    PatientId = x.PatientId,
                    DocumentType = x.DocumentType,
                    DocumentStatus = x.DocumentStatus,
                    DocumentTitle = x.DocumentTitle,
                    DocumentDateTime = x.DocumentDateTime,
                    FileName = x.FileName,
                    FileExtension = x.FileExtension,
                    IsConfidential = x.IsConfidential,
                    IsPartOfMedicalRecord = x.IsPartOfMedicalRecord,
                    IsVerified = x.IsVerified,
                    IsApproved = x.IsApproved,
                    IsArchived = x.IsArchived
                })
                .ToListAsync();

            return Ok(ApiResponse<List<PatientClinicalDocumentOptionResponse>>.Ok(
                data,
                "Data pilihan dokumen klinis pasien berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientClinicalDocumentDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Patient Clinical Document", Description = "Melihat detail dokumen klinis pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientClinicalDocument", "Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await BuildBaseQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Dokumen klinis pasien tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<PatientClinicalDocumentDetailResponse>.Ok(
                ToDetailResponse(entity),
                "Detail dokumen klinis pasien berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<PatientClinicalDocumentCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Patient Clinical Document", Description = "Membuat dokumen klinis pasien", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("PatientClinicalDocument", "Create")]
        public async Task<IActionResult> CreateClinicalDocument([FromBody] CreatePatientClinicalDocumentRequest request)
        {
            var validation = await ValidateCreateRequestAsync(request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data dokumen klinis pasien tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var context = await ResolveClinicalContextAsync(
                request.PatientId,
                request.EncounterId,
                request.QueueId,
                request.AssessmentId,
                request.ConsultationId,
                request.PatientDiagnosisId,
                request.PatientProcedureId,
                request.DoctorId,
                request.ServiceUnitId,
                request.ClinicId
            );

            if (!context.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    context.ErrorMessage ?? "Konteks klinis tidak valid."
                ));
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            var entity = new TrxPatientClinicalDocument
            {
                Id = Guid.NewGuid(),
                ClinicalDocumentNumber = await GenerateClinicalDocumentNumberAsync(now),
                PatientId = request.PatientId,
                EncounterId = context.EncounterId,
                QueueId = context.QueueId,
                AssessmentId = context.AssessmentId,
                ConsultationId = context.ConsultationId,
                PatientDiagnosisId = context.PatientDiagnosisId,
                PatientProcedureId = context.PatientProcedureId,
                DoctorId = context.DoctorId,
                ServiceUnitId = context.ServiceUnitId,
                ClinicId = context.ClinicId,

                DocumentType = request.DocumentType,
                DocumentSource = request.DocumentSource,
                DocumentStatus = request.DocumentStatus,
                ConfidentialityLevel = request.ConfidentialityLevel,
                DocumentTitle = request.DocumentTitle.Trim(),
                DocumentCode = NormalizeNullableText(request.DocumentCode),
                DocumentCategoryName = NormalizeNullableText(request.DocumentCategoryName),
                ExternalDocumentNumber = NormalizeNullableText(request.ExternalDocumentNumber),
                ExternalProviderName = NormalizeNullableText(request.ExternalProviderName),
                ExternalDoctorName = NormalizeNullableText(request.ExternalDoctorName),

                DocumentDateTime = request.DocumentDateTime ?? now,
                ReceivedDateTime = request.ReceivedDateTime,
                UploadedDateTime = request.UploadedDateTime ?? now,
                EffectiveStartDate = request.EffectiveStartDate,
                EffectiveEndDate = request.EffectiveEndDate,
                ExpiredDate = request.ExpiredDate,

                FilePath = request.FilePath.Trim(),
                FileName = request.FileName.Trim(),
                OriginalFileName = NormalizeNullableText(request.OriginalFileName),
                FileExtension = NormalizeFileExtension(request.FileExtension, request.FileName),
                MimeType = NormalizeNullableText(request.MimeType),
                FileSizeBytes = request.FileSizeBytes,
                FileHash = NormalizeNullableText(request.FileHash),
                StorageProvider = NormalizeNullableText(request.StorageProvider),
                ThumbnailPath = NormalizeNullableText(request.ThumbnailPath),
                PreviewPath = NormalizeNullableText(request.PreviewPath),
                PageCount = request.PageCount,

                DocumentSummary = NormalizeNullableText(request.DocumentSummary),
                ClinicalFindingSummary = NormalizeNullableText(request.ClinicalFindingSummary),
                Impression = NormalizeNullableText(request.Impression),
                Recommendation = NormalizeNullableText(request.Recommendation),
                Keywords = NormalizeNullableText(request.Keywords),

                IsConfidential = request.IsConfidential,
                IsPatientVisible = request.IsPatientVisible,
                IsShareable = request.IsShareable,
                IsExternalDocument = request.IsExternalDocument,
                IsPartOfMedicalRecord = request.IsPartOfMedicalRecord,
                IsLegalDocument = request.IsLegalDocument,
                IsNeedReview = request.IsNeedReview,
                IsReviewed = false,
                IsVerified = request.IsVerified,
                VerifiedAt = request.IsVerified ? now : null,
                VerifiedByUserId = request.IsVerified ? actorUserId : null,
                IsApproved = request.IsApproved,
                ApprovedAt = request.IsApproved ? now : null,
                ApprovedByUserId = request.IsApproved ? actorUserId : null,
                UploadedByUserId = actorUserId,
                Notes = NormalizeNullableText(request.Notes),

                IsArchived = false,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            NormalizeClinicalDocumentData(entity);

            _dbContext.Set<TrxPatientClinicalDocument>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var clinicalDocumentCount = await UpdateConsultationClinicalDocumentSummaryAsync(entity.ConsultationId, actorUserId, now);

            await transaction.CommitAsync();

            var response = ToCreateUpdateResponse(entity, clinicalDocumentCount);

            await _loggerService.InfoAsync(
                LogCategory,
                "PatientClinicalDocument.CreateClinicalDocument",
                "Membuat dokumen klinis pasien.",
                response
            );

            return Ok(ApiResponse<PatientClinicalDocumentCreateResponse>.Ok(
                response,
                "Dokumen klinis pasien berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientClinicalDocumentUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Patient Clinical Document", Description = "Mengubah dokumen klinis pasien", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("PatientClinicalDocument", "Update")]
        public async Task<IActionResult> UpdateClinicalDocument(Guid id, [FromBody] UpdatePatientClinicalDocumentRequest request)
        {
            var entity = await _dbContext.Set<TrxPatientClinicalDocument>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Dokumen klinis pasien tidak ditemukan."
                ));
            }

            if (entity.DocumentStatus == PatientClinicalDocumentStatus.Cancelled ||
                entity.DocumentStatus == PatientClinicalDocumentStatus.EnteredInError)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Dokumen yang sudah cancelled atau entered in error tidak dapat diubah."
                ));
            }

            var validation = ValidateUpdateRequest(request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data dokumen klinis pasien tidak valid."
                ));
            }

            var context = await ResolveClinicalContextAsync(
                entity.PatientId,
                request.EncounterId,
                request.QueueId,
                request.AssessmentId,
                request.ConsultationId,
                request.PatientDiagnosisId,
                request.PatientProcedureId,
                request.DoctorId,
                request.ServiceUnitId,
                request.ClinicId
            );

            if (!context.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    context.ErrorMessage ?? "Konteks klinis tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var oldConsultationId = entity.ConsultationId;

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            entity.EncounterId = context.EncounterId;
            entity.QueueId = context.QueueId;
            entity.AssessmentId = context.AssessmentId;
            entity.ConsultationId = context.ConsultationId;
            entity.PatientDiagnosisId = context.PatientDiagnosisId;
            entity.PatientProcedureId = context.PatientProcedureId;
            entity.DoctorId = context.DoctorId;
            entity.ServiceUnitId = context.ServiceUnitId;
            entity.ClinicId = context.ClinicId;

            entity.DocumentType = request.DocumentType;
            entity.DocumentSource = request.DocumentSource;
            entity.DocumentStatus = request.DocumentStatus;
            entity.ConfidentialityLevel = request.ConfidentialityLevel;
            entity.DocumentTitle = request.DocumentTitle.Trim();
            entity.DocumentCode = NormalizeNullableText(request.DocumentCode);
            entity.DocumentCategoryName = NormalizeNullableText(request.DocumentCategoryName);
            entity.ExternalDocumentNumber = NormalizeNullableText(request.ExternalDocumentNumber);
            entity.ExternalProviderName = NormalizeNullableText(request.ExternalProviderName);
            entity.ExternalDoctorName = NormalizeNullableText(request.ExternalDoctorName);

            entity.DocumentDateTime = request.DocumentDateTime ?? entity.DocumentDateTime;
            entity.ReceivedDateTime = request.ReceivedDateTime;
            entity.EffectiveStartDate = request.EffectiveStartDate;
            entity.EffectiveEndDate = request.EffectiveEndDate;
            entity.ExpiredDate = request.ExpiredDate;

            entity.FilePath = request.FilePath.Trim();
            entity.FileName = request.FileName.Trim();
            entity.OriginalFileName = NormalizeNullableText(request.OriginalFileName);
            entity.FileExtension = NormalizeFileExtension(request.FileExtension, request.FileName);
            entity.MimeType = NormalizeNullableText(request.MimeType);
            entity.FileSizeBytes = request.FileSizeBytes;
            entity.FileHash = NormalizeNullableText(request.FileHash);
            entity.StorageProvider = NormalizeNullableText(request.StorageProvider);
            entity.ThumbnailPath = NormalizeNullableText(request.ThumbnailPath);
            entity.PreviewPath = NormalizeNullableText(request.PreviewPath);
            entity.PageCount = request.PageCount;

            entity.DocumentSummary = NormalizeNullableText(request.DocumentSummary);
            entity.ClinicalFindingSummary = NormalizeNullableText(request.ClinicalFindingSummary);
            entity.Impression = NormalizeNullableText(request.Impression);
            entity.Recommendation = NormalizeNullableText(request.Recommendation);
            entity.Keywords = NormalizeNullableText(request.Keywords);

            entity.IsConfidential = request.IsConfidential;
            entity.IsPatientVisible = request.IsPatientVisible;
            entity.IsShareable = request.IsShareable;
            entity.IsExternalDocument = request.IsExternalDocument;
            entity.IsPartOfMedicalRecord = request.IsPartOfMedicalRecord;
            entity.IsLegalDocument = request.IsLegalDocument;
            entity.IsNeedReview = request.IsNeedReview;
            entity.IsActive = request.IsActive;
            entity.Notes = NormalizeNullableText(request.Notes);
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            NormalizeClinicalDocumentData(entity);

            await _dbContext.SaveChangesAsync();

            var clinicalDocumentCount = await UpdateConsultationClinicalDocumentSummaryAsync(oldConsultationId, actorUserId, now);
            if (entity.ConsultationId != oldConsultationId)
                clinicalDocumentCount = await UpdateConsultationClinicalDocumentSummaryAsync(entity.ConsultationId, actorUserId, now);

            await transaction.CommitAsync();

            var response = ToUpdateResponse(entity, clinicalDocumentCount);

            return Ok(ApiResponse<PatientClinicalDocumentUpdateResponse>.Ok(
                response,
                "Dokumen klinis pasien berhasil diubah."
            ));
        }

        [HttpPatch("{id:guid}/review")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Review Patient Clinical Document", Description = "Review dokumen klinis pasien", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("PatientClinicalDocument", "Update")]
        public async Task<IActionResult> ReviewClinicalDocument(Guid id, [FromBody] ReviewPatientClinicalDocumentRequest request)
        {
            var entity = await _dbContext.Set<TrxPatientClinicalDocument>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Dokumen klinis pasien tidak ditemukan."));

            if (entity.DocumentStatus == PatientClinicalDocumentStatus.Cancelled || entity.DocumentStatus == PatientClinicalDocumentStatus.EnteredInError)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Dokumen yang sudah cancelled atau entered in error tidak dapat direview."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsReviewed = true;
            entity.IsNeedReview = false;
            entity.ReviewedAt = now;
            entity.ReviewedByUserId = actorUserId;
            entity.ReviewNote = NormalizeNullableText(request.ReviewNote);
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(null, "Dokumen klinis pasien berhasil direview."));
        }

        [HttpPatch("{id:guid}/verify")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Verify Patient Clinical Document", Description = "Verifikasi dokumen klinis pasien", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("PatientClinicalDocument", "Update")]
        public async Task<IActionResult> VerifyClinicalDocument(Guid id, [FromBody] VerifyPatientClinicalDocumentRequest request)
        {
            var entity = await _dbContext.Set<TrxPatientClinicalDocument>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Dokumen klinis pasien tidak ditemukan."));

            if (entity.DocumentStatus == PatientClinicalDocumentStatus.Cancelled || entity.DocumentStatus == PatientClinicalDocumentStatus.EnteredInError)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Dokumen yang sudah cancelled atau entered in error tidak dapat diverifikasi."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsVerified = true;
            entity.VerifiedAt = now;
            entity.VerifiedByUserId = actorUserId;
            entity.VerificationNote = NormalizeNullableText(request.VerificationNote);
            entity.DocumentStatus = entity.IsApproved ? PatientClinicalDocumentStatus.Approved : PatientClinicalDocumentStatus.Verified;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(null, "Dokumen klinis pasien berhasil diverifikasi."));
        }

        [HttpPatch("{id:guid}/approve")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Approve Patient Clinical Document", Description = "Approve dokumen klinis pasien", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("PatientClinicalDocument", "Update")]
        public async Task<IActionResult> ApproveClinicalDocument(Guid id, [FromBody] ApprovePatientClinicalDocumentRequest request)
        {
            var entity = await _dbContext.Set<TrxPatientClinicalDocument>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Dokumen klinis pasien tidak ditemukan."));

            if (entity.DocumentStatus == PatientClinicalDocumentStatus.Cancelled || entity.DocumentStatus == PatientClinicalDocumentStatus.EnteredInError)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Dokumen yang sudah cancelled atau entered in error tidak dapat disetujui."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsVerified = true;
            entity.VerifiedAt ??= now;
            if (!entity.VerifiedByUserId.HasValue || entity.VerifiedByUserId.Value == Guid.Empty)
                entity.VerifiedByUserId = actorUserId;
            entity.IsApproved = true;
            entity.ApprovedAt = now;
            entity.ApprovedByUserId = actorUserId;
            entity.ApprovalNote = NormalizeNullableText(request.ApprovalNote);
            entity.DocumentStatus = PatientClinicalDocumentStatus.Approved;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(null, "Dokumen klinis pasien berhasil disetujui."));
        }

        [HttpPatch("{id:guid}/archive")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Archive Patient Clinical Document", Description = "Arsipkan dokumen klinis pasien", AccessType = AccessTypes.Update, SortOrder = 7)]
        [AccessPermission("PatientClinicalDocument", "Update")]
        public async Task<IActionResult> ArchiveClinicalDocument(Guid id, [FromBody] ArchivePatientClinicalDocumentRequest request)
        {
            var entity = await _dbContext.Set<TrxPatientClinicalDocument>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Dokumen klinis pasien tidak ditemukan."));

            if (entity.DocumentStatus == PatientClinicalDocumentStatus.Cancelled || entity.DocumentStatus == PatientClinicalDocumentStatus.EnteredInError)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Dokumen yang sudah cancelled atau entered in error tidak dapat diarsipkan."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var consultationId = entity.ConsultationId;

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            entity.IsArchived = true;
            entity.ArchivedAt = now;
            entity.ArchivedByUserId = actorUserId;
            entity.ArchiveReason = request.ArchiveReason.Trim();
            entity.DocumentStatus = PatientClinicalDocumentStatus.Archived;
            entity.IsActive = false;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();
            await UpdateConsultationClinicalDocumentSummaryAsync(consultationId, actorUserId, now);
            await transaction.CommitAsync();

            return Ok(ApiResponse<object>.Ok(null, "Dokumen klinis pasien berhasil diarsipkan."));
        }

        [HttpPatch("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Cancel Patient Clinical Document", Description = "Membatalkan dokumen klinis pasien", AccessType = AccessTypes.Update, SortOrder = 8)]
        [AccessPermission("PatientClinicalDocument", "Update")]
        public async Task<IActionResult> CancelClinicalDocument(Guid id, [FromBody] CancelPatientClinicalDocumentRequest request)
        {
            var entity = await _dbContext.Set<TrxPatientClinicalDocument>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Dokumen klinis pasien tidak ditemukan."));

            if (entity.DocumentStatus == PatientClinicalDocumentStatus.Cancelled)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Dokumen klinis pasien sudah cancelled."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var consultationId = entity.ConsultationId;

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            entity.DocumentStatus = PatientClinicalDocumentStatus.Cancelled;
            entity.IsActive = false;
            entity.CancelledAt = now;
            entity.CancelledByUserId = actorUserId;
            entity.CancelReason = request.CancelReason.Trim();
            entity.IsCancel = true;
            entity.CancelDateTime = now;
            entity.CancelBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();
            await UpdateConsultationClinicalDocumentSummaryAsync(consultationId, actorUserId, now);
            await transaction.CommitAsync();

            return Ok(ApiResponse<object>.Ok(null, "Dokumen klinis pasien berhasil dibatalkan."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Patient Clinical Document", Description = "Menghapus dokumen klinis pasien", AccessType = AccessTypes.Delete, SortOrder = 9)]
        [AccessPermission("PatientClinicalDocument", "Delete")]
        public async Task<IActionResult> DeleteClinicalDocument(Guid id)
        {
            var entity = await _dbContext.Set<TrxPatientClinicalDocument>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Dokumen klinis pasien tidak ditemukan."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var consultationId = entity.ConsultationId;

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            entity.IsDelete = true;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.IsActive = false;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();
            await UpdateConsultationClinicalDocumentSummaryAsync(consultationId, actorUserId, now);
            await transaction.CommitAsync();

            return Ok(ApiResponse<object>.Ok(null, "Dokumen klinis pasien berhasil dihapus."));
        }

        private IQueryable<TrxPatientClinicalDocument> BuildBaseQuery()
        {
            return _dbContext.Set<TrxPatientClinicalDocument>()
                .Include(x => x.Patient)
                .Include(x => x.Encounter)
                .Include(x => x.Queue)
                .Include(x => x.Assessment)
                .Include(x => x.Consultation)
                .Include(x => x.PatientDiagnosis)
                .Include(x => x.PatientProcedure)
                .Include(x => x.Doctor)
                .Include(x => x.ServiceUnit)
                .Include(x => x.Clinic)
                .Include(x => x.UploadedByUser)
                .Include(x => x.ReviewedByUser)
                .Include(x => x.VerifiedByUser)
                .Include(x => x.ApprovedByUser)
                .Include(x => x.ArchivedByUser)
                .Include(x => x.CancelledByUser)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<TrxPatientClinicalDocument> ApplyFilters(
            IQueryable<TrxPatientClinicalDocument> query,
            string? search,
            Guid? patientId,
            Guid? encounterId,
            Guid? queueId,
            Guid? assessmentId,
            Guid? consultationId,
            Guid? patientDiagnosisId,
            Guid? patientProcedureId,
            Guid? doctorId,
            Guid? serviceUnitId,
            Guid? clinicId,
            PatientClinicalDocumentType? documentType,
            PatientClinicalDocumentSource? documentSource,
            PatientClinicalDocumentStatus? documentStatus,
            PatientClinicalDocumentConfidentialityLevel? confidentialityLevel,
            bool? isConfidential,
            bool? isPatientVisible,
            bool? isShareable,
            bool? isExternalDocument,
            bool? isPartOfMedicalRecord,
            bool? isLegalDocument,
            bool? isNeedReview,
            bool? isReviewed,
            bool? isVerified,
            bool? isApproved,
            bool? isArchived,
            bool? isActive,
            DateTime? startDate,
            DateTime? endDate)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.ClinicalDocumentNumber.ToLower().Contains(keyword) ||
                    x.DocumentTitle.ToLower().Contains(keyword) ||
                    x.FileName.ToLower().Contains(keyword) ||
                    (x.DocumentCode != null && x.DocumentCode.ToLower().Contains(keyword)) ||
                    (x.DocumentCategoryName != null && x.DocumentCategoryName.ToLower().Contains(keyword)) ||
                    (x.ExternalDocumentNumber != null && x.ExternalDocumentNumber.ToLower().Contains(keyword)) ||
                    (x.ExternalProviderName != null && x.ExternalProviderName.ToLower().Contains(keyword)) ||
                    (x.ExternalDoctorName != null && x.ExternalDoctorName.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.FullName.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.MedicalRecordNumber.ToLower().Contains(keyword)) ||
                    (x.Encounter != null && x.Encounter.EncounterNumber.ToLower().Contains(keyword)) ||
                    (x.Consultation != null && x.Consultation.ConsultationNumber.ToLower().Contains(keyword)));
            }

            if (patientId.HasValue && patientId.Value != Guid.Empty) query = query.Where(x => x.PatientId == patientId.Value);
            if (encounterId.HasValue && encounterId.Value != Guid.Empty) query = query.Where(x => x.EncounterId == encounterId.Value);
            if (queueId.HasValue && queueId.Value != Guid.Empty) query = query.Where(x => x.QueueId == queueId.Value);
            if (assessmentId.HasValue && assessmentId.Value != Guid.Empty) query = query.Where(x => x.AssessmentId == assessmentId.Value);
            if (consultationId.HasValue && consultationId.Value != Guid.Empty) query = query.Where(x => x.ConsultationId == consultationId.Value);
            if (patientDiagnosisId.HasValue && patientDiagnosisId.Value != Guid.Empty) query = query.Where(x => x.PatientDiagnosisId == patientDiagnosisId.Value);
            if (patientProcedureId.HasValue && patientProcedureId.Value != Guid.Empty) query = query.Where(x => x.PatientProcedureId == patientProcedureId.Value);
            if (doctorId.HasValue && doctorId.Value != Guid.Empty) query = query.Where(x => x.DoctorId == doctorId.Value);
            if (serviceUnitId.HasValue && serviceUnitId.Value != Guid.Empty) query = query.Where(x => x.ServiceUnitId == serviceUnitId.Value);
            if (clinicId.HasValue && clinicId.Value != Guid.Empty) query = query.Where(x => x.ClinicId == clinicId.Value);
            if (documentType.HasValue) query = query.Where(x => x.DocumentType == documentType.Value);
            if (documentSource.HasValue) query = query.Where(x => x.DocumentSource == documentSource.Value);
            if (documentStatus.HasValue) query = query.Where(x => x.DocumentStatus == documentStatus.Value);
            if (confidentialityLevel.HasValue) query = query.Where(x => x.ConfidentialityLevel == confidentialityLevel.Value);
            if (isConfidential.HasValue) query = query.Where(x => x.IsConfidential == isConfidential.Value);
            if (isPatientVisible.HasValue) query = query.Where(x => x.IsPatientVisible == isPatientVisible.Value);
            if (isShareable.HasValue) query = query.Where(x => x.IsShareable == isShareable.Value);
            if (isExternalDocument.HasValue) query = query.Where(x => x.IsExternalDocument == isExternalDocument.Value);
            if (isPartOfMedicalRecord.HasValue) query = query.Where(x => x.IsPartOfMedicalRecord == isPartOfMedicalRecord.Value);
            if (isLegalDocument.HasValue) query = query.Where(x => x.IsLegalDocument == isLegalDocument.Value);
            if (isNeedReview.HasValue) query = query.Where(x => x.IsNeedReview == isNeedReview.Value);
            if (isReviewed.HasValue) query = query.Where(x => x.IsReviewed == isReviewed.Value);
            if (isVerified.HasValue) query = query.Where(x => x.IsVerified == isVerified.Value);
            if (isApproved.HasValue) query = query.Where(x => x.IsApproved == isApproved.Value);
            if (isArchived.HasValue) query = query.Where(x => x.IsArchived == isArchived.Value);
            if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);
            if (startDate.HasValue) query = query.Where(x => x.DocumentDateTime >= startDate.Value.Date);
            if (endDate.HasValue) query = query.Where(x => x.DocumentDateTime < endDate.Value.Date.AddDays(1));

            return query;
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateCreateRequestAsync(CreatePatientClinicalDocumentRequest request)
        {
            if (request.PatientId == Guid.Empty)
                return (false, "PatientId wajib diisi.");

            var patientExists = await _dbContext.Set<MstPatient>()
                .AnyAsync(x => x.Id == request.PatientId && !x.IsDelete);

            if (!patientExists)
                return (false, "Pasien tidak ditemukan.");

            return ValidateDocumentValues(
                request.DocumentTitle,
                request.FilePath,
                request.FileName,
                request.FileSizeBytes,
                request.PageCount,
                request.EffectiveStartDate,
                request.EffectiveEndDate,
                request.ExpiredDate
            );
        }

        private static (bool IsValid, string? ErrorMessage) ValidateUpdateRequest(UpdatePatientClinicalDocumentRequest request)
        {
            return ValidateDocumentValues(
                request.DocumentTitle,
                request.FilePath,
                request.FileName,
                request.FileSizeBytes,
                request.PageCount,
                request.EffectiveStartDate,
                request.EffectiveEndDate,
                request.ExpiredDate
            );
        }

        private static (bool IsValid, string? ErrorMessage) ValidateDocumentValues(
            string? documentTitle,
            string? filePath,
            string? fileName,
            long? fileSizeBytes,
            int? pageCount,
            DateTime? effectiveStartDate,
            DateTime? effectiveEndDate,
            DateTime? expiredDate)
        {
            if (string.IsNullOrWhiteSpace(documentTitle))
                return (false, "Judul dokumen wajib diisi.");

            if (string.IsNullOrWhiteSpace(filePath))
                return (false, "FilePath wajib diisi.");

            if (string.IsNullOrWhiteSpace(fileName))
                return (false, "FileName wajib diisi.");

            if (fileSizeBytes.HasValue && fileSizeBytes.Value < 0)
                return (false, "Ukuran file tidak boleh kurang dari 0.");

            if (pageCount.HasValue && pageCount.Value < 0)
                return (false, "Jumlah halaman tidak boleh kurang dari 0.");

            if (effectiveStartDate.HasValue && effectiveEndDate.HasValue && effectiveStartDate.Value.Date > effectiveEndDate.Value.Date)
                return (false, "Tanggal mulai berlaku tidak boleh lebih besar dari tanggal akhir berlaku.");

            if (effectiveStartDate.HasValue && expiredDate.HasValue && effectiveStartDate.Value.Date > expiredDate.Value.Date)
                return (false, "Tanggal mulai berlaku tidak boleh lebih besar dari tanggal expired.");

            return (true, null);
        }

        private async Task<ClinicalContextResult> ResolveClinicalContextAsync(
            Guid patientId,
            Guid? encounterId,
            Guid? queueId,
            Guid? assessmentId,
            Guid? consultationId,
            Guid? patientDiagnosisId,
            Guid? patientProcedureId,
            Guid? doctorId,
            Guid? serviceUnitId,
            Guid? clinicId)
        {
            var result = new ClinicalContextResult
            {
                EncounterId = NormalizeNullableGuid(encounterId),
                QueueId = NormalizeNullableGuid(queueId),
                AssessmentId = NormalizeNullableGuid(assessmentId),
                ConsultationId = NormalizeNullableGuid(consultationId),
                PatientDiagnosisId = NormalizeNullableGuid(patientDiagnosisId),
                PatientProcedureId = NormalizeNullableGuid(patientProcedureId),
                DoctorId = NormalizeNullableGuid(doctorId),
                ServiceUnitId = NormalizeNullableGuid(serviceUnitId),
                ClinicId = NormalizeNullableGuid(clinicId)
            };

            if (result.PatientProcedureId.HasValue)
            {
                var procedure = await _dbContext.Set<TrxPatientProcedure>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == result.PatientProcedureId.Value && !x.IsDelete);

                if (procedure == null)
                    return ClinicalContextResult.Fail("Tindakan pasien tidak ditemukan.");

                if (procedure.PatientId != patientId)
                    return ClinicalContextResult.Fail("Tindakan pasien tidak sesuai dengan pasien.");

                result.EncounterId = procedure.EncounterId;
                result.ConsultationId = procedure.ConsultationId;
                result.DoctorId = procedure.DoctorId;
                result.ServiceUnitId = procedure.ServiceUnitId;
                result.ClinicId = procedure.ClinicId;
            }

            if (result.PatientDiagnosisId.HasValue)
            {
                var diagnosis = await _dbContext.Set<TrxPatientDiagnosis>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == result.PatientDiagnosisId.Value && !x.IsDelete);

                if (diagnosis == null)
                    return ClinicalContextResult.Fail("Diagnosis pasien tidak ditemukan.");

                if (diagnosis.PatientId != patientId)
                    return ClinicalContextResult.Fail("Diagnosis pasien tidak sesuai dengan pasien.");

                result.EncounterId = diagnosis.EncounterId;
                result.ConsultationId = diagnosis.ConsultationId;
                result.DoctorId = diagnosis.DoctorId;
                result.ServiceUnitId = diagnosis.ServiceUnitId;
                result.ClinicId = diagnosis.ClinicId;
            }

            if (result.ConsultationId.HasValue)
            {
                var consultation = await _dbContext.Set<TrxDoctorConsultation>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == result.ConsultationId.Value && !x.IsDelete);

                if (consultation == null)
                    return ClinicalContextResult.Fail("Konsultasi dokter tidak ditemukan.");

                if (consultation.PatientId != patientId)
                    return ClinicalContextResult.Fail("Konsultasi dokter tidak sesuai dengan pasien.");

                result.EncounterId = consultation.EncounterId;
                result.QueueId = consultation.QueueId;
                result.AssessmentId = consultation.AssessmentId ?? result.AssessmentId;
                result.DoctorId = consultation.DoctorId;
                result.ServiceUnitId = consultation.ServiceUnitId;
                result.ClinicId = consultation.ClinicId;
            }

            if (result.AssessmentId.HasValue)
            {
                var assessment = await _dbContext.Set<TrxPatientAssessment>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == result.AssessmentId.Value && !x.IsDelete);

                if (assessment == null)
                    return ClinicalContextResult.Fail("Assessment pasien tidak ditemukan.");

                if (assessment.PatientId != patientId)
                    return ClinicalContextResult.Fail("Assessment pasien tidak sesuai dengan pasien.");

                result.EncounterId = assessment.EncounterId;
                result.QueueId = assessment.QueueId;
                result.DoctorId = assessment.DoctorId ?? result.DoctorId;
                result.ServiceUnitId = assessment.ServiceUnitId;
                result.ClinicId = assessment.ClinicId;
            }

            if (result.QueueId.HasValue)
            {
                var queue = await _dbContext.Set<TrxQueue>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == result.QueueId.Value && !x.IsDelete);

                if (queue == null)
                    return ClinicalContextResult.Fail("Queue pasien tidak ditemukan.");

                if (queue.PatientId != patientId)
                    return ClinicalContextResult.Fail("Queue pasien tidak sesuai dengan pasien.");

                result.EncounterId = queue.EncounterId;
                result.DoctorId = queue.DoctorId ?? result.DoctorId;
                result.ServiceUnitId = queue.ServiceUnitId;
                result.ClinicId = queue.ClinicId;
            }

            if (result.EncounterId.HasValue)
            {
                var encounter = await _dbContext.Set<TrxPatientEncounter>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == result.EncounterId.Value && !x.IsDelete);

                if (encounter == null)
                    return ClinicalContextResult.Fail("Encounter pasien tidak ditemukan.");

                if (encounter.PatientId != patientId)
                    return ClinicalContextResult.Fail("Encounter tidak sesuai dengan pasien.");
            }

            return result.Ok();
        }

        private async Task<string> GenerateClinicalDocumentNumberAsync(DateTime now)
        {
            var prefix = $"CDOC-{now:yyyyMMdd}";

            var countToday = await _dbContext.Set<TrxPatientClinicalDocument>()
                .CountAsync(x => x.ClinicalDocumentNumber.StartsWith(prefix));

            return $"{prefix}-{countToday + 1:0000}";
        }

        private async Task<int?> UpdateConsultationClinicalDocumentSummaryAsync(Guid? consultationId, Guid actorUserId, DateTime now)
        {
            if (!consultationId.HasValue || consultationId.Value == Guid.Empty)
                return null;

            var consultation = await _dbContext.Set<TrxDoctorConsultation>()
                .FirstOrDefaultAsync(x => x.Id == consultationId.Value && !x.IsDelete);

            if (consultation == null)
                return null;

            var count = await _dbContext.Set<TrxPatientClinicalDocument>()
                .CountAsync(x =>
                    !x.IsDelete &&
                    x.ConsultationId == consultationId.Value &&
                    x.IsActive &&
                    x.IsPartOfMedicalRecord &&
                    x.DocumentStatus != PatientClinicalDocumentStatus.Cancelled &&
                    x.DocumentStatus != PatientClinicalDocumentStatus.EnteredInError &&
                    x.DocumentStatus != PatientClinicalDocumentStatus.Archived);

            consultation.ClinicalDocumentCount = count;
            consultation.UpdateDateTime = now;
            consultation.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return count;
        }

        private static IQueryable<TrxPatientClinicalDocument> ApplySorting(
            IQueryable<TrxPatientClinicalDocument> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "documentDateTime").ToLowerInvariant() switch
            {
                "clinicaldocumentnumber" => isDesc ? query.OrderByDescending(x => x.ClinicalDocumentNumber) : query.OrderBy(x => x.ClinicalDocumentNumber),
                "documenttitle" => isDesc ? query.OrderByDescending(x => x.DocumentTitle) : query.OrderBy(x => x.DocumentTitle),
                "documenttype" => isDesc ? query.OrderByDescending(x => x.DocumentType) : query.OrderBy(x => x.DocumentType),
                "documentsource" => isDesc ? query.OrderByDescending(x => x.DocumentSource) : query.OrderBy(x => x.DocumentSource),
                "documentstatus" => isDesc ? query.OrderByDescending(x => x.DocumentStatus) : query.OrderBy(x => x.DocumentStatus),
                "confidentialitylevel" => isDesc ? query.OrderByDescending(x => x.ConfidentialityLevel) : query.OrderBy(x => x.ConfidentialityLevel),
                "isneedreview" => isDesc ? query.OrderByDescending(x => x.IsNeedReview) : query.OrderBy(x => x.IsNeedReview),
                "isreviewed" => isDesc ? query.OrderByDescending(x => x.IsReviewed) : query.OrderBy(x => x.IsReviewed),
                "isverified" => isDesc ? query.OrderByDescending(x => x.IsVerified) : query.OrderBy(x => x.IsVerified),
                "isapproved" => isDesc ? query.OrderByDescending(x => x.IsApproved) : query.OrderBy(x => x.IsApproved),
                "isarchived" => isDesc ? query.OrderByDescending(x => x.IsArchived) : query.OrderBy(x => x.IsArchived),
                "createdatetime" => isDesc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                _ => isDesc
                    ? query.OrderByDescending(x => x.DocumentDateTime).ThenByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.DocumentDateTime).ThenBy(x => x.CreateDateTime)
            };
        }

        private static PatientClinicalDocumentResponse ToResponse(TrxPatientClinicalDocument x)
        {
            return new PatientClinicalDocumentResponse
            {
                Id = x.Id,
                ClinicalDocumentNumber = x.ClinicalDocumentNumber,
                PatientId = x.PatientId,
                PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty,
                EncounterId = x.EncounterId,
                EncounterNumber = x.Encounter != null ? x.Encounter.EncounterNumber : null,
                QueueId = x.QueueId,
                QueueCode = x.Queue != null ? x.Queue.QueueCode : null,
                AssessmentId = x.AssessmentId,
                AssessmentNumber = x.Assessment != null ? x.Assessment.AssessmentNumber : null,
                ConsultationId = x.ConsultationId,
                ConsultationNumber = x.Consultation != null ? x.Consultation.ConsultationNumber : null,
                PatientDiagnosisId = x.PatientDiagnosisId,
                DiagnosisCode = x.PatientDiagnosis != null ? x.PatientDiagnosis.DiagnosisCode : null,
                DiagnosisName = x.PatientDiagnosis != null ? x.PatientDiagnosis.DiagnosisName : null,
                PatientProcedureId = x.PatientProcedureId,
                ProcedureCode = x.PatientProcedure != null ? x.PatientProcedure.ProcedureCodeSnapshot : null,
                ProcedureName = x.PatientProcedure != null ? x.PatientProcedure.ProcedureNameSnapshot : null,
                DoctorId = x.DoctorId,
                DoctorName = x.Doctor != null ? x.Doctor.FullName : null,
                ServiceUnitId = x.ServiceUnitId,
                ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null,
                ClinicId = x.ClinicId,
                ClinicName = x.Clinic != null ? x.Clinic.ClinicName : null,
                DocumentType = x.DocumentType,
                DocumentSource = x.DocumentSource,
                DocumentStatus = x.DocumentStatus,
                ConfidentialityLevel = x.ConfidentialityLevel,
                DocumentTitle = x.DocumentTitle,
                DocumentCode = x.DocumentCode,
                DocumentCategoryName = x.DocumentCategoryName,
                ExternalDocumentNumber = x.ExternalDocumentNumber,
                ExternalProviderName = x.ExternalProviderName,
                ExternalDoctorName = x.ExternalDoctorName,
                DocumentDateTime = x.DocumentDateTime,
                ReceivedDateTime = x.ReceivedDateTime,
                UploadedDateTime = x.UploadedDateTime,
                ExpiredDate = x.ExpiredDate,
                FilePath = x.FilePath,
                FileName = x.FileName,
                OriginalFileName = x.OriginalFileName,
                FileExtension = x.FileExtension,
                MimeType = x.MimeType,
                FileSizeBytes = x.FileSizeBytes,
                StorageProvider = x.StorageProvider,
                IsConfidential = x.IsConfidential,
                IsPatientVisible = x.IsPatientVisible,
                IsShareable = x.IsShareable,
                IsExternalDocument = x.IsExternalDocument,
                IsPartOfMedicalRecord = x.IsPartOfMedicalRecord,
                IsLegalDocument = x.IsLegalDocument,
                IsNeedReview = x.IsNeedReview,
                IsReviewed = x.IsReviewed,
                IsVerified = x.IsVerified,
                IsApproved = x.IsApproved,
                IsArchived = x.IsArchived,
                UploadedByUserId = x.UploadedByUserId,
                UploadedByUserName = x.UploadedByUser != null ? x.UploadedByUser.DisplayName : null,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime
            };
        }

        private static PatientClinicalDocumentDetailResponse ToDetailResponse(TrxPatientClinicalDocument x)
        {
            var response = new PatientClinicalDocumentDetailResponse
            {
                Id = x.Id,
                ClinicalDocumentNumber = x.ClinicalDocumentNumber,
                PatientId = x.PatientId,
                PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty,
                EncounterId = x.EncounterId,
                EncounterNumber = x.Encounter != null ? x.Encounter.EncounterNumber : null,
                QueueId = x.QueueId,
                QueueCode = x.Queue != null ? x.Queue.QueueCode : null,
                AssessmentId = x.AssessmentId,
                AssessmentNumber = x.Assessment != null ? x.Assessment.AssessmentNumber : null,
                ConsultationId = x.ConsultationId,
                ConsultationNumber = x.Consultation != null ? x.Consultation.ConsultationNumber : null,
                PatientDiagnosisId = x.PatientDiagnosisId,
                DiagnosisCode = x.PatientDiagnosis != null ? x.PatientDiagnosis.DiagnosisCode : null,
                DiagnosisName = x.PatientDiagnosis != null ? x.PatientDiagnosis.DiagnosisName : null,
                PatientProcedureId = x.PatientProcedureId,
                ProcedureCode = x.PatientProcedure != null ? x.PatientProcedure.ProcedureCodeSnapshot : null,
                ProcedureName = x.PatientProcedure != null ? x.PatientProcedure.ProcedureNameSnapshot : null,
                DoctorId = x.DoctorId,
                DoctorName = x.Doctor != null ? x.Doctor.FullName : null,
                ServiceUnitId = x.ServiceUnitId,
                ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null,
                ClinicId = x.ClinicId,
                ClinicName = x.Clinic != null ? x.Clinic.ClinicName : null,
                DocumentType = x.DocumentType,
                DocumentSource = x.DocumentSource,
                DocumentStatus = x.DocumentStatus,
                ConfidentialityLevel = x.ConfidentialityLevel,
                DocumentTitle = x.DocumentTitle,
                DocumentCode = x.DocumentCode,
                DocumentCategoryName = x.DocumentCategoryName,
                ExternalDocumentNumber = x.ExternalDocumentNumber,
                ExternalProviderName = x.ExternalProviderName,
                ExternalDoctorName = x.ExternalDoctorName,
                DocumentDateTime = x.DocumentDateTime,
                ReceivedDateTime = x.ReceivedDateTime,
                UploadedDateTime = x.UploadedDateTime,
                EffectiveStartDate = x.EffectiveStartDate,
                EffectiveEndDate = x.EffectiveEndDate,
                ExpiredDate = x.ExpiredDate,
                FilePath = x.FilePath,
                FileName = x.FileName,
                OriginalFileName = x.OriginalFileName,
                FileExtension = x.FileExtension,
                MimeType = x.MimeType,
                FileSizeBytes = x.FileSizeBytes,
                FileHash = x.FileHash,
                StorageProvider = x.StorageProvider,
                ThumbnailPath = x.ThumbnailPath,
                PreviewPath = x.PreviewPath,
                PageCount = x.PageCount,
                DocumentSummary = x.DocumentSummary,
                ClinicalFindingSummary = x.ClinicalFindingSummary,
                Impression = x.Impression,
                Recommendation = x.Recommendation,
                Keywords = x.Keywords,
                IsConfidential = x.IsConfidential,
                IsPatientVisible = x.IsPatientVisible,
                IsShareable = x.IsShareable,
                IsExternalDocument = x.IsExternalDocument,
                IsPartOfMedicalRecord = x.IsPartOfMedicalRecord,
                IsLegalDocument = x.IsLegalDocument,
                IsNeedReview = x.IsNeedReview,
                IsReviewed = x.IsReviewed,
                ReviewedAt = x.ReviewedAt,
                ReviewedByUserId = x.ReviewedByUserId,
                ReviewedByUserName = x.ReviewedByUser != null ? x.ReviewedByUser.DisplayName : null,
                ReviewNote = x.ReviewNote,
                IsVerified = x.IsVerified,
                VerifiedAt = x.VerifiedAt,
                VerifiedByUserId = x.VerifiedByUserId,
                VerifiedByUserName = x.VerifiedByUser != null ? x.VerifiedByUser.DisplayName : null,
                VerificationNote = x.VerificationNote,
                IsApproved = x.IsApproved,
                ApprovedAt = x.ApprovedAt,
                ApprovedByUserId = x.ApprovedByUserId,
                ApprovedByUserName = x.ApprovedByUser != null ? x.ApprovedByUser.DisplayName : null,
                ApprovalNote = x.ApprovalNote,
                UploadedByUserId = x.UploadedByUserId,
                UploadedByUserName = x.UploadedByUser != null ? x.UploadedByUser.DisplayName : null,
                IsArchived = x.IsArchived,
                ArchivedAt = x.ArchivedAt,
                ArchivedByUserId = x.ArchivedByUserId,
                ArchivedByUserName = x.ArchivedByUser != null ? x.ArchivedByUser.DisplayName : null,
                ArchiveReason = x.ArchiveReason,
                CancelledAt = x.CancelledAt,
                CancelledByUserId = x.CancelledByUserId,
                CancelledByUserName = x.CancelledByUser != null ? x.CancelledByUser.DisplayName : null,
                CancelReason = x.CancelReason,
                Notes = x.Notes,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime
            };

            return response;
        }

        private static PatientClinicalDocumentCreateResponse ToCreateUpdateResponse(TrxPatientClinicalDocument x, int? clinicalDocumentCount)
        {
            return new PatientClinicalDocumentCreateResponse
            {
                Id = x.Id,
                ClinicalDocumentNumber = x.ClinicalDocumentNumber,
                PatientId = x.PatientId,
                EncounterId = x.EncounterId,
                ConsultationId = x.ConsultationId,
                PatientDiagnosisId = x.PatientDiagnosisId,
                PatientProcedureId = x.PatientProcedureId,
                DocumentType = x.DocumentType,
                DocumentStatus = x.DocumentStatus,
                DocumentTitle = x.DocumentTitle,
                FileName = x.FileName,
                FilePath = x.FilePath,
                IsPartOfMedicalRecord = x.IsPartOfMedicalRecord,
                IsVerified = x.IsVerified,
                IsApproved = x.IsApproved,
                ClinicalDocumentCount = clinicalDocumentCount
            };
        }

        private static PatientClinicalDocumentUpdateResponse ToUpdateResponse(TrxPatientClinicalDocument x, int? clinicalDocumentCount)
        {
            return new PatientClinicalDocumentUpdateResponse
            {
                Id = x.Id,
                ClinicalDocumentNumber = x.ClinicalDocumentNumber,
                PatientId = x.PatientId,
                EncounterId = x.EncounterId,
                ConsultationId = x.ConsultationId,
                PatientDiagnosisId = x.PatientDiagnosisId,
                PatientProcedureId = x.PatientProcedureId,
                DocumentType = x.DocumentType,
                DocumentStatus = x.DocumentStatus,
                DocumentTitle = x.DocumentTitle,
                FileName = x.FileName,
                FilePath = x.FilePath,
                IsPartOfMedicalRecord = x.IsPartOfMedicalRecord,
                IsVerified = x.IsVerified,
                IsApproved = x.IsApproved,
                ClinicalDocumentCount = clinicalDocumentCount
            };
        }

        private static void NormalizeClinicalDocumentData(TrxPatientClinicalDocument entity)
        {
            if (entity.ConfidentialityLevel != PatientClinicalDocumentConfidentialityLevel.Normal)
                entity.IsConfidential = true;

            if (entity.IsConfidential && entity.ConfidentialityLevel == PatientClinicalDocumentConfidentialityLevel.Normal)
                entity.ConfidentialityLevel = PatientClinicalDocumentConfidentialityLevel.Restricted;

            if (entity.DocumentSource == PatientClinicalDocumentSource.ExternalHospital ||
                entity.DocumentSource == PatientClinicalDocumentSource.PatientProvided)
            {
                entity.IsExternalDocument = true;
            }

            if (entity.IsNeedReview)
                entity.IsReviewed = false;

            if (entity.IsApproved)
            {
                entity.IsVerified = true;
                entity.DocumentStatus = PatientClinicalDocumentStatus.Approved;
            }
            else if (entity.IsVerified && entity.DocumentStatus == PatientClinicalDocumentStatus.Uploaded)
            {
                entity.DocumentStatus = PatientClinicalDocumentStatus.Verified;
            }

            if (entity.DocumentStatus == PatientClinicalDocumentStatus.Approved)
            {
                entity.IsApproved = true;
                entity.IsVerified = true;
            }

            if (entity.DocumentStatus == PatientClinicalDocumentStatus.Verified)
                entity.IsVerified = true;

            if (entity.DocumentStatus == PatientClinicalDocumentStatus.Archived)
            {
                entity.IsArchived = true;
                entity.IsActive = false;
            }

            if (entity.DocumentStatus == PatientClinicalDocumentStatus.Cancelled ||
                entity.DocumentStatus == PatientClinicalDocumentStatus.EnteredInError)
            {
                entity.IsActive = false;
            }

            if (!entity.IsActive && entity.DocumentStatus == PatientClinicalDocumentStatus.Uploaded)
                entity.DocumentStatus = PatientClinicalDocumentStatus.Archived;
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            return (pageNumber, pageSize);
        }

        private static List<PatientClinicalDocumentEnumOptionResponse> BuildEnumOptions<TEnum>() where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Select(x => new PatientClinicalDocumentEnumOptionResponse
                {
                    Value = Convert.ToInt32(x),
                    Name = x.ToString(),
                    Label = SplitPascalCase(x.ToString())
                })
                .ToList();
        }

        private static string SplitPascalCase(string value)
        {
            return string.Concat(value.Select((x, i) =>
                i > 0 && char.IsUpper(x) ? " " + x : x.ToString()));
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static Guid? NormalizeNullableGuid(Guid? value)
        {
            if (!value.HasValue || value.Value == Guid.Empty)
                return null;

            return value.Value;
        }

        private static string? NormalizeFileExtension(string? fileExtension, string fileName)
        {
            var value = NormalizeNullableText(fileExtension);

            if (!string.IsNullOrWhiteSpace(value))
                return value.StartsWith(".") ? value.ToLowerInvariant() : "." + value.ToLowerInvariant();

            var dotIndex = fileName.LastIndexOf('.');
            if (dotIndex >= 0 && dotIndex < fileName.Length - 1)
                return fileName[dotIndex..].ToLowerInvariant();

            return null;
        }

        private Guid GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userId, out var id)
                ? id
                : Guid.Empty;
        }

        private class ClinicalContextResult
        {
            public bool IsValid { get; set; }
            public string? ErrorMessage { get; set; }
            public Guid? EncounterId { get; set; }
            public Guid? QueueId { get; set; }
            public Guid? AssessmentId { get; set; }
            public Guid? ConsultationId { get; set; }
            public Guid? PatientDiagnosisId { get; set; }
            public Guid? PatientProcedureId { get; set; }
            public Guid? DoctorId { get; set; }
            public Guid? ServiceUnitId { get; set; }
            public Guid? ClinicId { get; set; }

            public ClinicalContextResult Ok()
            {
                IsValid = true;
                return this;
            }

            public static ClinicalContextResult Fail(string errorMessage)
            {
                return new ClinicalContextResult
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
        }
    }
}
