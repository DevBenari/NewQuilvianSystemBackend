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

using ResponseClinicalNoteAttachmentPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs.ClinicalNoteAttachmentResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/clinical-management/clinical-note-attachments")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_CLINICAL",
        moduleName: "Health Service Clinical",
        displayName: "Clinical Note Attachment",
        AreaName = "HealthServices",
        ControllerName = "ClinicalNoteAttachment",
        Description = "Lampiran catatan klinis pasien",
        SortOrder = 12
    )]
    [Tags("Health Services / Clinical Management / Clinical Note Attachment")]
    public class ClinicalNoteAttachmentController : ControllerBase
    {
        private const string LogCategory = "HealthServices.Clinical";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public ClinicalNoteAttachmentController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<ClinicalNoteAttachmentFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Clinical Note Attachment", Description = "Melihat metadata filter lampiran catatan klinis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ClinicalNoteAttachment", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new ClinicalNoteAttachmentFilterMetadataResponse
            {
                DefaultFilter = new ClinicalNoteAttachmentDefaultFilterResponse(),
                SortOptions = new List<ClinicalNoteAttachmentSortOptionResponse>
                {
                    new() { Value = "uploadedAt", Label = "Tanggal upload" },
                    new() { Value = "attachmentNumber", Label = "Nomor lampiran" },
                    new() { Value = "attachmentTitle", Label = "Judul lampiran" },
                    new() { Value = "attachmentType", Label = "Tipe lampiran" },
                    new() { Value = "attachmentContext", Label = "Konteks lampiran" },
                    new() { Value = "attachmentStatus", Label = "Status lampiran" },
                    new() { Value = "isReviewed", Label = "Sudah direview" },
                    new() { Value = "isVerified", Label = "Terverifikasi" },
                    new() { Value = "isArchived", Label = "Diarsipkan" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                AttachmentTypeOptions = BuildEnumOptions<ClinicalNoteAttachmentType>(),
                AttachmentContextOptions = BuildEnumOptions<ClinicalNoteAttachmentContext>(),
                AttachmentStatusOptions = BuildEnumOptions<ClinicalNoteAttachmentStatus>(),
                ConfidentialityLevelOptions = BuildEnumOptions<ClinicalNoteAttachmentConfidentialityLevel>(),
                BodySideOptions = BuildEnumOptions<ClinicalAttachmentBodySide>()
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "ClinicalNoteAttachment.GetFilterMetadata",
                "Mengambil metadata filter lampiran catatan klinis.",
                result
            );

            return Ok(ApiResponse<ClinicalNoteAttachmentFilterMetadataResponse>.Ok(
                result,
                "Metadata filter lampiran catatan klinis berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseClinicalNoteAttachmentPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Clinical Note Attachment", Description = "Melihat data lampiran catatan klinis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ClinicalNoteAttachment", "Read")]
        public async Task<IActionResult> GetAttachments(
            [FromQuery] string? search,
            [FromQuery] Guid? patientId,
            [FromQuery] Guid? encounterId,
            [FromQuery] Guid? queueId,
            [FromQuery] Guid? assessmentId,
            [FromQuery] Guid? consultationId,
            [FromQuery] Guid? patientDiagnosisId,
            [FromQuery] Guid? patientProcedureId,
            [FromQuery] Guid? clinicalDocumentId,
            [FromQuery] Guid? doctorId,
            [FromQuery] Guid? serviceUnitId,
            [FromQuery] Guid? clinicId,
            [FromQuery] Guid? relatedAttachmentId,
            [FromQuery] ClinicalNoteAttachmentType? attachmentType,
            [FromQuery] ClinicalNoteAttachmentContext? attachmentContext,
            [FromQuery] ClinicalNoteAttachmentStatus? attachmentStatus,
            [FromQuery] ClinicalNoteAttachmentConfidentialityLevel? confidentialityLevel,
            [FromQuery] ClinicalAttachmentBodySide? bodySide,
            [FromQuery] bool? isConfidential,
            [FromQuery] bool? isPatientVisible,
            [FromQuery] bool? isPartOfMedicalRecord,
            [FromQuery] bool? isClinicalMedia,
            [FromQuery] bool? isBeforeAfterComparison,
            [FromQuery] bool? isNeedReview,
            [FromQuery] bool? isReviewed,
            [FromQuery] bool? isVerified,
            [FromQuery] bool? isArchived,
            [FromQuery] bool? isActive,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? sortBy = "uploadedAt",
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
                clinicalDocumentId,
                doctorId,
                serviceUnitId,
                clinicId,
                relatedAttachmentId,
                attachmentType,
                attachmentContext,
                attachmentStatus,
                confidentialityLevel,
                bodySide,
                isConfidential,
                isPatientVisible,
                isPartOfMedicalRecord,
                isClinicalMedia,
                isBeforeAfterComparison,
                isNeedReview,
                isReviewed,
                isVerified,
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

            return Ok(ApiResponse<ResponseClinicalNoteAttachmentPagedResult>.Ok(
                new ResponseClinicalNoteAttachmentPagedResult
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = items
                },
                "Data lampiran catatan klinis berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<ClinicalNoteAttachmentOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Clinical Note Attachment", Description = "Melihat pilihan lampiran catatan klinis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ClinicalNoteAttachment", "Read")]
        public async Task<IActionResult> GetAttachmentOptions(
            [FromQuery] Guid? patientId,
            [FromQuery] Guid? encounterId,
            [FromQuery] Guid? consultationId,
            [FromQuery] Guid? patientDiagnosisId,
            [FromQuery] Guid? patientProcedureId,
            [FromQuery] ClinicalNoteAttachmentContext? attachmentContext,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null)
        {
            var query = _dbContext.Set<TrxClinicalNoteAttachment>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (onlyActive)
            {
                query = query.Where(x =>
                    x.IsActive &&
                    x.AttachmentStatus != ClinicalNoteAttachmentStatus.Cancelled &&
                    x.AttachmentStatus != ClinicalNoteAttachmentStatus.EnteredInError &&
                    x.AttachmentStatus != ClinicalNoteAttachmentStatus.Archived);
            }

            if (patientId.HasValue && patientId.Value != Guid.Empty) query = query.Where(x => x.PatientId == patientId.Value);
            if (encounterId.HasValue && encounterId.Value != Guid.Empty) query = query.Where(x => x.EncounterId == encounterId.Value);
            if (consultationId.HasValue && consultationId.Value != Guid.Empty) query = query.Where(x => x.ConsultationId == consultationId.Value);
            if (patientDiagnosisId.HasValue && patientDiagnosisId.Value != Guid.Empty) query = query.Where(x => x.PatientDiagnosisId == patientDiagnosisId.Value);
            if (patientProcedureId.HasValue && patientProcedureId.Value != Guid.Empty) query = query.Where(x => x.PatientProcedureId == patientProcedureId.Value);
            if (attachmentContext.HasValue) query = query.Where(x => x.AttachmentContext == attachmentContext.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.AttachmentNumber.ToLower().Contains(keyword) ||
                    x.AttachmentTitle.ToLower().Contains(keyword) ||
                    x.FileName.ToLower().Contains(keyword) ||
                    (x.OriginalFileName != null && x.OriginalFileName.ToLower().Contains(keyword)) ||
                    (x.AttachmentDescription != null && x.AttachmentDescription.ToLower().Contains(keyword)) ||
                    (x.BodySite != null && x.BodySite.ToLower().Contains(keyword)));
            }

            var data = await query
                .OrderByDescending(x => x.UploadedAt)
                .Take(100)
                .Select(x => new ClinicalNoteAttachmentOptionResponse
                {
                    Id = x.Id,
                    AttachmentNumber = x.AttachmentNumber,
                    PatientId = x.PatientId,
                    EncounterId = x.EncounterId,
                    ConsultationId = x.ConsultationId,
                    PatientDiagnosisId = x.PatientDiagnosisId,
                    PatientProcedureId = x.PatientProcedureId,
                    AttachmentType = x.AttachmentType,
                    AttachmentContext = x.AttachmentContext,
                    AttachmentStatus = x.AttachmentStatus,
                    AttachmentTitle = x.AttachmentTitle,
                    FileName = x.FileName,
                    MimeType = x.MimeType,
                    UploadedAt = x.UploadedAt,
                    IsReviewed = x.IsReviewed,
                    IsVerified = x.IsVerified,
                    IsArchived = x.IsArchived
                })
                .ToListAsync();

            return Ok(ApiResponse<List<ClinicalNoteAttachmentOptionResponse>>.Ok(
                data,
                "Data pilihan lampiran catatan klinis berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ClinicalNoteAttachmentDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Clinical Note Attachment", Description = "Melihat detail lampiran catatan klinis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ClinicalNoteAttachment", "Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await BuildBaseQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Lampiran catatan klinis tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<ClinicalNoteAttachmentDetailResponse>.Ok(
                ToDetailResponse(entity),
                "Detail lampiran catatan klinis berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ClinicalNoteAttachmentCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Clinical Note Attachment", Description = "Membuat lampiran catatan klinis", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("ClinicalNoteAttachment", "Create")]
        public async Task<IActionResult> CreateAttachment([FromBody] CreateClinicalNoteAttachmentRequest request)
        {
            var validation = await ValidateCreateRequestAsync(request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data lampiran catatan klinis tidak valid."
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
                request.ClinicalDocumentId,
                request.DoctorId,
                request.ServiceUnitId,
                request.ClinicId,
                request.RelatedAttachmentId
            );

            if (!context.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    context.ErrorMessage ?? "Konteks lampiran catatan klinis tidak valid."
                ));
            }

            var entity = new TrxClinicalNoteAttachment
            {
                Id = Guid.NewGuid(),
                AttachmentNumber = await GenerateAttachmentNumberAsync(now),
                PatientId = request.PatientId,
                EncounterId = context.EncounterId,
                QueueId = context.QueueId,
                AssessmentId = context.AssessmentId,
                ConsultationId = context.ConsultationId,
                PatientDiagnosisId = context.PatientDiagnosisId,
                PatientProcedureId = context.PatientProcedureId,
                ClinicalDocumentId = context.ClinicalDocumentId,
                DoctorId = context.DoctorId,
                ServiceUnitId = context.ServiceUnitId,
                ClinicId = context.ClinicId,

                AttachmentType = request.AttachmentType,
                AttachmentContext = request.AttachmentContext,
                AttachmentStatus = request.AttachmentStatus,
                ConfidentialityLevel = request.ConfidentialityLevel,
                AttachmentTitle = request.AttachmentTitle.Trim(),
                AttachmentCode = NormalizeNullableText(request.AttachmentCode),
                AttachmentCategoryName = NormalizeNullableText(request.AttachmentCategoryName),
                AttachmentDescription = NormalizeNullableText(request.AttachmentDescription),
                NoteSectionName = NormalizeNullableText(request.NoteSectionName),
                ClinicalNote = NormalizeNullableText(request.ClinicalNote),
                FindingNote = NormalizeNullableText(request.FindingNote),
                InterpretationNote = NormalizeNullableText(request.InterpretationNote),
                FollowUpNote = NormalizeNullableText(request.FollowUpNote),

                BodySite = NormalizeNullableText(request.BodySite),
                BodySide = request.BodySide,
                BodySiteDescription = NormalizeNullableText(request.BodySiteDescription),
                ViewPosition = NormalizeNullableText(request.ViewPosition),
                CapturedAt = request.CapturedAt,
                CapturedByUserId = NormalizeNullableGuid(request.CapturedByUserId),
                CaptureDeviceName = NormalizeNullableText(request.CaptureDeviceName),

                FilePath = request.FilePath.Trim(),
                FileName = request.FileName.Trim(),
                OriginalFileName = NormalizeNullableText(request.OriginalFileName),
                FileExtension = NormalizeNullableText(request.FileExtension),
                MimeType = NormalizeNullableText(request.MimeType),
                FileSizeBytes = request.FileSizeBytes,
                FileHash = NormalizeNullableText(request.FileHash),
                StorageProvider = NormalizeNullableText(request.StorageProvider),
                ThumbnailPath = NormalizeNullableText(request.ThumbnailPath),
                PreviewPath = NormalizeNullableText(request.PreviewPath),
                ImageWidth = request.ImageWidth,
                ImageHeight = request.ImageHeight,
                DurationSeconds = request.DurationSeconds,

                IsConfidential = request.IsConfidential,
                IsPatientVisible = request.IsPatientVisible,
                IsShareable = request.IsShareable,
                IsPartOfMedicalRecord = request.IsPartOfMedicalRecord,
                IsClinicalMedia = request.IsClinicalMedia,
                IsBeforeAfterComparison = request.IsBeforeAfterComparison,
                RelatedAttachmentId = context.RelatedAttachmentId,
                IsNeedReview = request.IsNeedReview,
                IsReviewed = request.IsReviewed,
                ReviewedAt = request.IsReviewed ? now : null,
                ReviewedByUserId = request.IsReviewed ? actorUserId : null,
                IsVerified = request.IsVerified,
                VerifiedAt = request.IsVerified ? now : null,
                VerifiedByUserId = request.IsVerified ? actorUserId : null,
                UploadedByUserId = actorUserId,
                UploadedAt = request.UploadedAt ?? now,
                Notes = NormalizeNullableText(request.Notes),

                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            NormalizeAttachmentData(entity);

            _dbContext.Set<TrxClinicalNoteAttachment>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var response = ToCreateUpdateResponse(entity);

            await _loggerService.InfoAsync(
                LogCategory,
                "ClinicalNoteAttachment.CreateAttachment",
                "Membuat lampiran catatan klinis.",
                response
            );

            return Ok(ApiResponse<ClinicalNoteAttachmentCreateResponse>.Ok(
                response,
                "Lampiran catatan klinis berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ClinicalNoteAttachmentUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Clinical Note Attachment", Description = "Mengubah lampiran catatan klinis", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("ClinicalNoteAttachment", "Update")]
        public async Task<IActionResult> UpdateAttachment(Guid id, [FromBody] UpdateClinicalNoteAttachmentRequest request)
        {
            var entity = await _dbContext.Set<TrxClinicalNoteAttachment>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Lampiran catatan klinis tidak ditemukan."
                ));
            }

            if (entity.AttachmentStatus == ClinicalNoteAttachmentStatus.Cancelled ||
                entity.AttachmentStatus == ClinicalNoteAttachmentStatus.EnteredInError)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Lampiran yang sudah cancelled atau entered in error tidak dapat diubah."
                ));
            }

            var validation = await ValidateUpdateRequestAsync(entity.PatientId, id, request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data lampiran catatan klinis tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.AttachmentType = request.AttachmentType;
            entity.AttachmentContext = request.AttachmentContext;
            entity.AttachmentStatus = request.AttachmentStatus;
            entity.ConfidentialityLevel = request.ConfidentialityLevel;
            entity.AttachmentTitle = request.AttachmentTitle.Trim();
            entity.AttachmentCode = NormalizeNullableText(request.AttachmentCode);
            entity.AttachmentCategoryName = NormalizeNullableText(request.AttachmentCategoryName);
            entity.AttachmentDescription = NormalizeNullableText(request.AttachmentDescription);
            entity.NoteSectionName = NormalizeNullableText(request.NoteSectionName);
            entity.ClinicalNote = NormalizeNullableText(request.ClinicalNote);
            entity.FindingNote = NormalizeNullableText(request.FindingNote);
            entity.InterpretationNote = NormalizeNullableText(request.InterpretationNote);
            entity.FollowUpNote = NormalizeNullableText(request.FollowUpNote);

            entity.BodySite = NormalizeNullableText(request.BodySite);
            entity.BodySide = request.BodySide;
            entity.BodySiteDescription = NormalizeNullableText(request.BodySiteDescription);
            entity.ViewPosition = NormalizeNullableText(request.ViewPosition);
            entity.CapturedAt = request.CapturedAt;
            entity.CapturedByUserId = NormalizeNullableGuid(request.CapturedByUserId);
            entity.CaptureDeviceName = NormalizeNullableText(request.CaptureDeviceName);

            entity.FilePath = request.FilePath.Trim();
            entity.FileName = request.FileName.Trim();
            entity.OriginalFileName = NormalizeNullableText(request.OriginalFileName);
            entity.FileExtension = NormalizeNullableText(request.FileExtension);
            entity.MimeType = NormalizeNullableText(request.MimeType);
            entity.FileSizeBytes = request.FileSizeBytes;
            entity.FileHash = NormalizeNullableText(request.FileHash);
            entity.StorageProvider = NormalizeNullableText(request.StorageProvider);
            entity.ThumbnailPath = NormalizeNullableText(request.ThumbnailPath);
            entity.PreviewPath = NormalizeNullableText(request.PreviewPath);
            entity.ImageWidth = request.ImageWidth;
            entity.ImageHeight = request.ImageHeight;
            entity.DurationSeconds = request.DurationSeconds;

            entity.IsConfidential = request.IsConfidential;
            entity.IsPatientVisible = request.IsPatientVisible;
            entity.IsShareable = request.IsShareable;
            entity.IsPartOfMedicalRecord = request.IsPartOfMedicalRecord;
            entity.IsClinicalMedia = request.IsClinicalMedia;
            entity.IsBeforeAfterComparison = request.IsBeforeAfterComparison;
            entity.RelatedAttachmentId = NormalizeNullableGuid(request.RelatedAttachmentId);
            entity.IsNeedReview = request.IsNeedReview;
            entity.Notes = NormalizeNullableText(request.Notes);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            NormalizeAttachmentData(entity);

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<ClinicalNoteAttachmentUpdateResponse>.Ok(
                ToUpdateResponse(entity),
                "Lampiran catatan klinis berhasil diubah."
            ));
        }

        [HttpPatch("{id:guid}/review")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Review Clinical Note Attachment", Description = "Review lampiran catatan klinis", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("ClinicalNoteAttachment", "Update")]
        public async Task<IActionResult> ReviewAttachment(Guid id, [FromBody] ReviewClinicalNoteAttachmentRequest request)
        {
            var entity = await _dbContext.Set<TrxClinicalNoteAttachment>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Lampiran catatan klinis tidak ditemukan."));

            if (entity.AttachmentStatus == ClinicalNoteAttachmentStatus.Cancelled ||
                entity.AttachmentStatus == ClinicalNoteAttachmentStatus.EnteredInError)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Lampiran tidak dapat direview."));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsReviewed = true;
            entity.ReviewedAt = now;
            entity.ReviewedByUserId = actorUserId;
            entity.ReviewNote = NormalizeNullableText(request.ReviewNote);
            entity.AttachmentStatus = ClinicalNoteAttachmentStatus.Reviewed;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(null, "Lampiran catatan klinis berhasil direview."));
        }

        [HttpPatch("{id:guid}/verify")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Verify Clinical Note Attachment", Description = "Verifikasi lampiran catatan klinis", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("ClinicalNoteAttachment", "Update")]
        public async Task<IActionResult> VerifyAttachment(Guid id, [FromBody] VerifyClinicalNoteAttachmentRequest request)
        {
            var entity = await _dbContext.Set<TrxClinicalNoteAttachment>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Lampiran catatan klinis tidak ditemukan."));

            if (entity.AttachmentStatus == ClinicalNoteAttachmentStatus.Cancelled ||
                entity.AttachmentStatus == ClinicalNoteAttachmentStatus.EnteredInError)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Lampiran tidak dapat diverifikasi."));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsVerified = true;
            entity.VerifiedAt = now;
            entity.VerifiedByUserId = actorUserId;
            entity.VerificationNote = NormalizeNullableText(request.VerificationNote);
            entity.AttachmentStatus = ClinicalNoteAttachmentStatus.Verified;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(null, "Lampiran catatan klinis berhasil diverifikasi."));
        }

        [HttpPatch("{id:guid}/archive")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Archive Clinical Note Attachment", Description = "Arsipkan lampiran catatan klinis", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("ClinicalNoteAttachment", "Update")]
        public async Task<IActionResult> ArchiveAttachment(Guid id, [FromBody] ArchiveClinicalNoteAttachmentRequest request)
        {
            var entity = await _dbContext.Set<TrxClinicalNoteAttachment>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Lampiran catatan klinis tidak ditemukan."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsArchived = true;
            entity.ArchivedAt = now;
            entity.ArchivedByUserId = actorUserId;
            entity.ArchiveReason = request.ArchiveReason.Trim();
            entity.AttachmentStatus = ClinicalNoteAttachmentStatus.Archived;
            entity.IsActive = false;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(null, "Lampiran catatan klinis berhasil diarsipkan."));
        }

        [HttpPatch("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Cancel Clinical Note Attachment", Description = "Membatalkan lampiran catatan klinis", AccessType = AccessTypes.Update, SortOrder = 7)]
        [AccessPermission("ClinicalNoteAttachment", "Update")]
        public async Task<IActionResult> CancelAttachment(Guid id, [FromBody] CancelClinicalNoteAttachmentRequest request)
        {
            var entity = await _dbContext.Set<TrxClinicalNoteAttachment>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Lampiran catatan klinis tidak ditemukan."));

            if (entity.AttachmentStatus == ClinicalNoteAttachmentStatus.Cancelled)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Lampiran catatan klinis sudah cancelled."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.AttachmentStatus = ClinicalNoteAttachmentStatus.Cancelled;
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

            return Ok(ApiResponse<object>.Ok(null, "Lampiran catatan klinis berhasil dibatalkan."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Clinical Note Attachment", Description = "Menghapus lampiran catatan klinis", AccessType = AccessTypes.Delete, SortOrder = 8)]
        [AccessPermission("ClinicalNoteAttachment", "Delete")]
        public async Task<IActionResult> DeleteAttachment(Guid id)
        {
            var entity = await _dbContext.Set<TrxClinicalNoteAttachment>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Lampiran catatan klinis tidak ditemukan."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsDelete = true;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.IsActive = false;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(null, "Lampiran catatan klinis berhasil dihapus."));
        }

        private IQueryable<TrxClinicalNoteAttachment> BuildBaseQuery()
        {
            return _dbContext.Set<TrxClinicalNoteAttachment>()
                .Include(x => x.Patient)
                .Include(x => x.Encounter)
                .Include(x => x.Queue)
                .Include(x => x.Assessment)
                .Include(x => x.Consultation)
                .Include(x => x.PatientDiagnosis)
                .Include(x => x.PatientProcedure)
                .Include(x => x.ClinicalDocument)
                .Include(x => x.Doctor)
                .Include(x => x.ServiceUnit)
                .Include(x => x.Clinic)
                .Include(x => x.RelatedAttachment)
                .Include(x => x.UploadedByUser)
                .Include(x => x.CapturedByUser)
                .Include(x => x.ReviewedByUser)
                .Include(x => x.VerifiedByUser)
                .Include(x => x.ArchivedByUser)
                .Include(x => x.CancelledByUser)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<TrxClinicalNoteAttachment> ApplyFilters(
            IQueryable<TrxClinicalNoteAttachment> query,
            string? search,
            Guid? patientId,
            Guid? encounterId,
            Guid? queueId,
            Guid? assessmentId,
            Guid? consultationId,
            Guid? patientDiagnosisId,
            Guid? patientProcedureId,
            Guid? clinicalDocumentId,
            Guid? doctorId,
            Guid? serviceUnitId,
            Guid? clinicId,
            Guid? relatedAttachmentId,
            ClinicalNoteAttachmentType? attachmentType,
            ClinicalNoteAttachmentContext? attachmentContext,
            ClinicalNoteAttachmentStatus? attachmentStatus,
            ClinicalNoteAttachmentConfidentialityLevel? confidentialityLevel,
            ClinicalAttachmentBodySide? bodySide,
            bool? isConfidential,
            bool? isPatientVisible,
            bool? isPartOfMedicalRecord,
            bool? isClinicalMedia,
            bool? isBeforeAfterComparison,
            bool? isNeedReview,
            bool? isReviewed,
            bool? isVerified,
            bool? isArchived,
            bool? isActive,
            DateTime? startDate,
            DateTime? endDate)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.AttachmentNumber.ToLower().Contains(keyword) ||
                    x.AttachmentTitle.ToLower().Contains(keyword) ||
                    x.FileName.ToLower().Contains(keyword) ||
                    (x.OriginalFileName != null && x.OriginalFileName.ToLower().Contains(keyword)) ||
                    (x.AttachmentDescription != null && x.AttachmentDescription.ToLower().Contains(keyword)) ||
                    (x.NoteSectionName != null && x.NoteSectionName.ToLower().Contains(keyword)) ||
                    (x.BodySite != null && x.BodySite.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.FullName.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.MedicalRecordNumber.ToLower().Contains(keyword)) ||
                    (x.Encounter != null && x.Encounter.EncounterNumber.ToLower().Contains(keyword)));
            }

            if (patientId.HasValue && patientId.Value != Guid.Empty) query = query.Where(x => x.PatientId == patientId.Value);
            if (encounterId.HasValue && encounterId.Value != Guid.Empty) query = query.Where(x => x.EncounterId == encounterId.Value);
            if (queueId.HasValue && queueId.Value != Guid.Empty) query = query.Where(x => x.QueueId == queueId.Value);
            if (assessmentId.HasValue && assessmentId.Value != Guid.Empty) query = query.Where(x => x.AssessmentId == assessmentId.Value);
            if (consultationId.HasValue && consultationId.Value != Guid.Empty) query = query.Where(x => x.ConsultationId == consultationId.Value);
            if (patientDiagnosisId.HasValue && patientDiagnosisId.Value != Guid.Empty) query = query.Where(x => x.PatientDiagnosisId == patientDiagnosisId.Value);
            if (patientProcedureId.HasValue && patientProcedureId.Value != Guid.Empty) query = query.Where(x => x.PatientProcedureId == patientProcedureId.Value);
            if (clinicalDocumentId.HasValue && clinicalDocumentId.Value != Guid.Empty) query = query.Where(x => x.ClinicalDocumentId == clinicalDocumentId.Value);
            if (doctorId.HasValue && doctorId.Value != Guid.Empty) query = query.Where(x => x.DoctorId == doctorId.Value);
            if (serviceUnitId.HasValue && serviceUnitId.Value != Guid.Empty) query = query.Where(x => x.ServiceUnitId == serviceUnitId.Value);
            if (clinicId.HasValue && clinicId.Value != Guid.Empty) query = query.Where(x => x.ClinicId == clinicId.Value);
            if (relatedAttachmentId.HasValue && relatedAttachmentId.Value != Guid.Empty) query = query.Where(x => x.RelatedAttachmentId == relatedAttachmentId.Value);
            if (attachmentType.HasValue) query = query.Where(x => x.AttachmentType == attachmentType.Value);
            if (attachmentContext.HasValue) query = query.Where(x => x.AttachmentContext == attachmentContext.Value);
            if (attachmentStatus.HasValue) query = query.Where(x => x.AttachmentStatus == attachmentStatus.Value);
            if (confidentialityLevel.HasValue) query = query.Where(x => x.ConfidentialityLevel == confidentialityLevel.Value);
            if (bodySide.HasValue) query = query.Where(x => x.BodySide == bodySide.Value);
            if (isConfidential.HasValue) query = query.Where(x => x.IsConfidential == isConfidential.Value);
            if (isPatientVisible.HasValue) query = query.Where(x => x.IsPatientVisible == isPatientVisible.Value);
            if (isPartOfMedicalRecord.HasValue) query = query.Where(x => x.IsPartOfMedicalRecord == isPartOfMedicalRecord.Value);
            if (isClinicalMedia.HasValue) query = query.Where(x => x.IsClinicalMedia == isClinicalMedia.Value);
            if (isBeforeAfterComparison.HasValue) query = query.Where(x => x.IsBeforeAfterComparison == isBeforeAfterComparison.Value);
            if (isNeedReview.HasValue) query = query.Where(x => x.IsNeedReview == isNeedReview.Value);
            if (isReviewed.HasValue) query = query.Where(x => x.IsReviewed == isReviewed.Value);
            if (isVerified.HasValue) query = query.Where(x => x.IsVerified == isVerified.Value);
            if (isArchived.HasValue) query = query.Where(x => x.IsArchived == isArchived.Value);
            if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);
            if (startDate.HasValue) query = query.Where(x => x.UploadedAt >= startDate.Value.Date);
            if (endDate.HasValue) query = query.Where(x => x.UploadedAt < endDate.Value.Date.AddDays(1));

            return query;
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateCreateRequestAsync(CreateClinicalNoteAttachmentRequest request)
        {
            if (request.PatientId == Guid.Empty)
                return (false, "PatientId wajib diisi.");

            if (string.IsNullOrWhiteSpace(request.AttachmentTitle))
                return (false, "Judul lampiran wajib diisi.");

            if (string.IsNullOrWhiteSpace(request.FilePath))
                return (false, "FilePath wajib diisi.");

            if (string.IsNullOrWhiteSpace(request.FileName))
                return (false, "FileName wajib diisi.");

            var patientExists = await _dbContext.Set<MstPatient>()
                .AnyAsync(x => x.Id == request.PatientId && !x.IsDelete);

            if (!patientExists)
                return (false, "Pasien tidak ditemukan.");

            return ValidateFileMetadata(
                request.FileSizeBytes,
                request.ImageWidth,
                request.ImageHeight,
                request.DurationSeconds
            );
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateUpdateRequestAsync(
            Guid patientId,
            Guid currentAttachmentId,
            UpdateClinicalNoteAttachmentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.AttachmentTitle))
                return (false, "Judul lampiran wajib diisi.");

            if (string.IsNullOrWhiteSpace(request.FilePath))
                return (false, "FilePath wajib diisi.");

            if (string.IsNullOrWhiteSpace(request.FileName))
                return (false, "FileName wajib diisi.");

            var fileValidation = ValidateFileMetadata(
                request.FileSizeBytes,
                request.ImageWidth,
                request.ImageHeight,
                request.DurationSeconds
            );

            if (!fileValidation.IsValid)
                return fileValidation;

            var relatedAttachmentId = NormalizeNullableGuid(request.RelatedAttachmentId);

            if (relatedAttachmentId.HasValue)
            {
                if (relatedAttachmentId.Value == currentAttachmentId)
                    return (false, "RelatedAttachmentId tidak boleh sama dengan attachment saat ini.");

                var relatedExists = await _dbContext.Set<TrxClinicalNoteAttachment>()
                    .AnyAsync(x =>
                        x.Id == relatedAttachmentId.Value &&
                        x.PatientId == patientId &&
                        !x.IsDelete);

                if (!relatedExists)
                    return (false, "Related attachment tidak ditemukan atau tidak sesuai pasien.");
            }

            return (true, null);
        }

        private static (bool IsValid, string? ErrorMessage) ValidateFileMetadata(
            long? fileSizeBytes,
            int? imageWidth,
            int? imageHeight,
            int? durationSeconds)
        {
            if (fileSizeBytes.HasValue && fileSizeBytes.Value < 0)
                return (false, "Ukuran file tidak valid.");

            if (imageWidth.HasValue && imageWidth.Value < 0)
                return (false, "ImageWidth tidak valid.");

            if (imageHeight.HasValue && imageHeight.Value < 0)
                return (false, "ImageHeight tidak valid.");

            if (durationSeconds.HasValue && durationSeconds.Value < 0)
                return (false, "DurationSeconds tidak valid.");

            return (true, null);
        }

        private async Task<ClinicalAttachmentContextResult> ResolveClinicalContextAsync(
            Guid patientId,
            Guid? encounterId,
            Guid? queueId,
            Guid? assessmentId,
            Guid? consultationId,
            Guid? patientDiagnosisId,
            Guid? patientProcedureId,
            Guid? clinicalDocumentId,
            Guid? doctorId,
            Guid? serviceUnitId,
            Guid? clinicId,
            Guid? relatedAttachmentId)
        {
            var result = new ClinicalAttachmentContextResult
            {
                EncounterId = NormalizeNullableGuid(encounterId),
                QueueId = NormalizeNullableGuid(queueId),
                AssessmentId = NormalizeNullableGuid(assessmentId),
                ConsultationId = NormalizeNullableGuid(consultationId),
                PatientDiagnosisId = NormalizeNullableGuid(patientDiagnosisId),
                PatientProcedureId = NormalizeNullableGuid(patientProcedureId),
                ClinicalDocumentId = NormalizeNullableGuid(clinicalDocumentId),
                DoctorId = NormalizeNullableGuid(doctorId),
                ServiceUnitId = NormalizeNullableGuid(serviceUnitId),
                ClinicId = NormalizeNullableGuid(clinicId),
                RelatedAttachmentId = NormalizeNullableGuid(relatedAttachmentId)
            };

            if (result.PatientProcedureId.HasValue)
            {
                var procedure = await _dbContext.Set<TrxPatientProcedure>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == result.PatientProcedureId.Value && !x.IsDelete);

                if (procedure == null)
                    return ClinicalAttachmentContextResult.Fail("Tindakan pasien tidak ditemukan.");

                if (procedure.PatientId != patientId)
                    return ClinicalAttachmentContextResult.Fail("Tindakan pasien tidak sesuai dengan pasien.");

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
                    return ClinicalAttachmentContextResult.Fail("Diagnosis pasien tidak ditemukan.");

                if (diagnosis.PatientId != patientId)
                    return ClinicalAttachmentContextResult.Fail("Diagnosis pasien tidak sesuai dengan pasien.");

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
                    return ClinicalAttachmentContextResult.Fail("Konsultasi dokter tidak ditemukan.");

                if (consultation.PatientId != patientId)
                    return ClinicalAttachmentContextResult.Fail("Konsultasi dokter tidak sesuai dengan pasien.");

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
                    return ClinicalAttachmentContextResult.Fail("Assessment pasien tidak ditemukan.");

                if (assessment.PatientId != patientId)
                    return ClinicalAttachmentContextResult.Fail("Assessment pasien tidak sesuai dengan pasien.");

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
                    return ClinicalAttachmentContextResult.Fail("Queue pasien tidak ditemukan.");

                if (queue.PatientId != patientId)
                    return ClinicalAttachmentContextResult.Fail("Queue tidak sesuai dengan pasien.");

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
                    return ClinicalAttachmentContextResult.Fail("Encounter pasien tidak ditemukan.");

                if (encounter.PatientId != patientId)
                    return ClinicalAttachmentContextResult.Fail("Encounter tidak sesuai dengan pasien.");
            }

            if (result.ClinicalDocumentId.HasValue)
            {
                var document = await _dbContext.Set<TrxPatientClinicalDocument>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == result.ClinicalDocumentId.Value && !x.IsDelete);

                if (document == null)
                    return ClinicalAttachmentContextResult.Fail("Dokumen klinis tidak ditemukan.");

                if (document.PatientId != patientId)
                    return ClinicalAttachmentContextResult.Fail("Dokumen klinis tidak sesuai dengan pasien.");
            }

            if (result.RelatedAttachmentId.HasValue)
            {
                var related = await _dbContext.Set<TrxClinicalNoteAttachment>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == result.RelatedAttachmentId.Value && !x.IsDelete);

                if (related == null)
                    return ClinicalAttachmentContextResult.Fail("Related attachment tidak ditemukan.");

                if (related.PatientId != patientId)
                    return ClinicalAttachmentContextResult.Fail("Related attachment tidak sesuai dengan pasien.");
            }

            return result.Ok();
        }

        private async Task<string> GenerateAttachmentNumberAsync(DateTime now)
        {
            var prefix = $"CNA-{now:yyyyMMdd}";

            var countToday = await _dbContext.Set<TrxClinicalNoteAttachment>()
                .CountAsync(x => x.AttachmentNumber.StartsWith(prefix));

            return $"{prefix}-{countToday + 1:0000}";
        }

        private static IQueryable<TrxClinicalNoteAttachment> ApplySorting(
            IQueryable<TrxClinicalNoteAttachment> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "uploadedAt").ToLowerInvariant() switch
            {
                "attachmentnumber" => isDesc ? query.OrderByDescending(x => x.AttachmentNumber) : query.OrderBy(x => x.AttachmentNumber),
                "attachmenttitle" => isDesc ? query.OrderByDescending(x => x.AttachmentTitle) : query.OrderBy(x => x.AttachmentTitle),
                "attachmenttype" => isDesc ? query.OrderByDescending(x => x.AttachmentType) : query.OrderBy(x => x.AttachmentType),
                "attachmentcontext" => isDesc ? query.OrderByDescending(x => x.AttachmentContext) : query.OrderBy(x => x.AttachmentContext),
                "attachmentstatus" => isDesc ? query.OrderByDescending(x => x.AttachmentStatus) : query.OrderBy(x => x.AttachmentStatus),
                "isreviewed" => isDesc ? query.OrderByDescending(x => x.IsReviewed) : query.OrderBy(x => x.IsReviewed),
                "isverified" => isDesc ? query.OrderByDescending(x => x.IsVerified) : query.OrderBy(x => x.IsVerified),
                "isarchived" => isDesc ? query.OrderByDescending(x => x.IsArchived) : query.OrderBy(x => x.IsArchived),
                "createdatetime" => isDesc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                _ => isDesc ? query.OrderByDescending(x => x.UploadedAt) : query.OrderBy(x => x.UploadedAt)
            };
        }

        private static ClinicalNoteAttachmentResponse ToResponse(TrxClinicalNoteAttachment x)
        {
            return new ClinicalNoteAttachmentResponse
            {
                Id = x.Id,
                AttachmentNumber = x.AttachmentNumber,
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
                PatientProcedureId = x.PatientProcedureId,
                ClinicalDocumentId = x.ClinicalDocumentId,
                ClinicalDocumentNumber = x.ClinicalDocument != null ? x.ClinicalDocument.ClinicalDocumentNumber : null,
                DoctorId = x.DoctorId,
                DoctorName = x.Doctor != null ? x.Doctor.FullName : null,
                ServiceUnitId = x.ServiceUnitId,
                ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null,
                ClinicId = x.ClinicId,
                ClinicName = x.Clinic != null ? x.Clinic.ClinicName : null,
                AttachmentType = x.AttachmentType,
                AttachmentContext = x.AttachmentContext,
                AttachmentStatus = x.AttachmentStatus,
                ConfidentialityLevel = x.ConfidentialityLevel,
                AttachmentTitle = x.AttachmentTitle,
                AttachmentCode = x.AttachmentCode,
                AttachmentCategoryName = x.AttachmentCategoryName,
                NoteSectionName = x.NoteSectionName,
                FilePath = x.FilePath,
                FileName = x.FileName,
                OriginalFileName = x.OriginalFileName,
                FileExtension = x.FileExtension,
                MimeType = x.MimeType,
                FileSizeBytes = x.FileSizeBytes,
                ThumbnailPath = x.ThumbnailPath,
                PreviewPath = x.PreviewPath,
                BodySite = x.BodySite,
                BodySide = x.BodySide,
                CapturedAt = x.CapturedAt,
                CapturedByUserId = x.CapturedByUserId,
                CapturedByUserName = x.CapturedByUser != null ? x.CapturedByUser.DisplayName : null,
                IsConfidential = x.IsConfidential,
                IsPatientVisible = x.IsPatientVisible,
                IsShareable = x.IsShareable,
                IsPartOfMedicalRecord = x.IsPartOfMedicalRecord,
                IsClinicalMedia = x.IsClinicalMedia,
                IsBeforeAfterComparison = x.IsBeforeAfterComparison,
                RelatedAttachmentId = x.RelatedAttachmentId,
                RelatedAttachmentNumber = x.RelatedAttachment != null ? x.RelatedAttachment.AttachmentNumber : null,
                IsNeedReview = x.IsNeedReview,
                IsReviewed = x.IsReviewed,
                ReviewedAt = x.ReviewedAt,
                ReviewedByUserId = x.ReviewedByUserId,
                ReviewedByUserName = x.ReviewedByUser != null ? x.ReviewedByUser.DisplayName : null,
                IsVerified = x.IsVerified,
                VerifiedAt = x.VerifiedAt,
                VerifiedByUserId = x.VerifiedByUserId,
                VerifiedByUserName = x.VerifiedByUser != null ? x.VerifiedByUser.DisplayName : null,
                UploadedByUserId = x.UploadedByUserId,
                UploadedByUserName = x.UploadedByUser != null ? x.UploadedByUser.DisplayName : null,
                UploadedAt = x.UploadedAt,
                IsArchived = x.IsArchived,
                ArchivedAt = x.ArchivedAt,
                ArchivedByUserId = x.ArchivedByUserId,
                ArchivedByUserName = x.ArchivedByUser != null ? x.ArchivedByUser.DisplayName : null,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime
            };
        }

        private static ClinicalNoteAttachmentDetailResponse ToDetailResponse(TrxClinicalNoteAttachment x)
        {
            var response = new ClinicalNoteAttachmentDetailResponse
            {
                Id = x.Id,
                AttachmentNumber = x.AttachmentNumber,
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
                PatientProcedureId = x.PatientProcedureId,
                ClinicalDocumentId = x.ClinicalDocumentId,
                ClinicalDocumentNumber = x.ClinicalDocument != null ? x.ClinicalDocument.ClinicalDocumentNumber : null,
                DoctorId = x.DoctorId,
                DoctorName = x.Doctor != null ? x.Doctor.FullName : null,
                ServiceUnitId = x.ServiceUnitId,
                ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null,
                ClinicId = x.ClinicId,
                ClinicName = x.Clinic != null ? x.Clinic.ClinicName : null,
                AttachmentType = x.AttachmentType,
                AttachmentContext = x.AttachmentContext,
                AttachmentStatus = x.AttachmentStatus,
                ConfidentialityLevel = x.ConfidentialityLevel,
                AttachmentTitle = x.AttachmentTitle,
                AttachmentCode = x.AttachmentCode,
                AttachmentCategoryName = x.AttachmentCategoryName,
                AttachmentDescription = x.AttachmentDescription,
                NoteSectionName = x.NoteSectionName,
                ClinicalNote = x.ClinicalNote,
                FindingNote = x.FindingNote,
                InterpretationNote = x.InterpretationNote,
                FollowUpNote = x.FollowUpNote,
                BodySite = x.BodySite,
                BodySide = x.BodySide,
                BodySiteDescription = x.BodySiteDescription,
                ViewPosition = x.ViewPosition,
                CapturedAt = x.CapturedAt,
                CapturedByUserId = x.CapturedByUserId,
                CapturedByUserName = x.CapturedByUser != null ? x.CapturedByUser.DisplayName : null,
                CaptureDeviceName = x.CaptureDeviceName,
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
                ImageWidth = x.ImageWidth,
                ImageHeight = x.ImageHeight,
                DurationSeconds = x.DurationSeconds,
                IsConfidential = x.IsConfidential,
                IsPatientVisible = x.IsPatientVisible,
                IsShareable = x.IsShareable,
                IsPartOfMedicalRecord = x.IsPartOfMedicalRecord,
                IsClinicalMedia = x.IsClinicalMedia,
                IsBeforeAfterComparison = x.IsBeforeAfterComparison,
                RelatedAttachmentId = x.RelatedAttachmentId,
                RelatedAttachmentNumber = x.RelatedAttachment != null ? x.RelatedAttachment.AttachmentNumber : null,
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
                UploadedByUserId = x.UploadedByUserId,
                UploadedByUserName = x.UploadedByUser != null ? x.UploadedByUser.DisplayName : null,
                UploadedAt = x.UploadedAt,
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

        private static ClinicalNoteAttachmentCreateResponse ToCreateUpdateResponse(TrxClinicalNoteAttachment x)
        {
            return new ClinicalNoteAttachmentCreateResponse
            {
                Id = x.Id,
                AttachmentNumber = x.AttachmentNumber,
                PatientId = x.PatientId,
                EncounterId = x.EncounterId,
                ConsultationId = x.ConsultationId,
                PatientDiagnosisId = x.PatientDiagnosisId,
                PatientProcedureId = x.PatientProcedureId,
                AttachmentType = x.AttachmentType,
                AttachmentContext = x.AttachmentContext,
                AttachmentStatus = x.AttachmentStatus,
                AttachmentTitle = x.AttachmentTitle,
                FileName = x.FileName,
                UploadedAt = x.UploadedAt,
                IsReviewed = x.IsReviewed,
                IsVerified = x.IsVerified,
                IsArchived = x.IsArchived
            };
        }

        private static ClinicalNoteAttachmentUpdateResponse ToUpdateResponse(TrxClinicalNoteAttachment x)
        {
            return new ClinicalNoteAttachmentUpdateResponse
            {
                Id = x.Id,
                AttachmentNumber = x.AttachmentNumber,
                PatientId = x.PatientId,
                EncounterId = x.EncounterId,
                ConsultationId = x.ConsultationId,
                PatientDiagnosisId = x.PatientDiagnosisId,
                PatientProcedureId = x.PatientProcedureId,
                AttachmentType = x.AttachmentType,
                AttachmentContext = x.AttachmentContext,
                AttachmentStatus = x.AttachmentStatus,
                AttachmentTitle = x.AttachmentTitle,
                FileName = x.FileName,
                UploadedAt = x.UploadedAt,
                IsReviewed = x.IsReviewed,
                IsVerified = x.IsVerified,
                IsArchived = x.IsArchived
            };
        }

        private static void NormalizeAttachmentData(TrxClinicalNoteAttachment entity)
        {
            if (entity.ConfidentialityLevel >= ClinicalNoteAttachmentConfidentialityLevel.Confidential)
                entity.IsConfidential = true;

            if (entity.IsReviewed && entity.AttachmentStatus == ClinicalNoteAttachmentStatus.Uploaded)
                entity.AttachmentStatus = ClinicalNoteAttachmentStatus.Reviewed;

            if (entity.IsVerified)
                entity.AttachmentStatus = ClinicalNoteAttachmentStatus.Verified;

            if (entity.IsArchived)
            {
                entity.AttachmentStatus = ClinicalNoteAttachmentStatus.Archived;
                entity.IsActive = false;
            }

            if (entity.AttachmentStatus == ClinicalNoteAttachmentStatus.Cancelled ||
                entity.AttachmentStatus == ClinicalNoteAttachmentStatus.EnteredInError ||
                entity.AttachmentStatus == ClinicalNoteAttachmentStatus.Archived)
            {
                entity.IsActive = false;
            }
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            return (pageNumber, pageSize);
        }

        private static List<ClinicalNoteAttachmentEnumOptionResponse> BuildEnumOptions<TEnum>() where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Select(x => new ClinicalNoteAttachmentEnumOptionResponse
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

        private Guid GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userId, out var id)
                ? id
                : Guid.Empty;
        }

        private class ClinicalAttachmentContextResult
        {
            public bool IsValid { get; set; }
            public string? ErrorMessage { get; set; }
            public Guid? EncounterId { get; set; }
            public Guid? QueueId { get; set; }
            public Guid? AssessmentId { get; set; }
            public Guid? ConsultationId { get; set; }
            public Guid? PatientDiagnosisId { get; set; }
            public Guid? PatientProcedureId { get; set; }
            public Guid? ClinicalDocumentId { get; set; }
            public Guid? DoctorId { get; set; }
            public Guid? ServiceUnitId { get; set; }
            public Guid? ClinicId { get; set; }
            public Guid? RelatedAttachmentId { get; set; }

            public ClinicalAttachmentContextResult Ok()
            {
                IsValid = true;
                return this;
            }

            public static ClinicalAttachmentContextResult Fail(string errorMessage)
            {
                return new ClinicalAttachmentContextResult
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
        }
    }
}
