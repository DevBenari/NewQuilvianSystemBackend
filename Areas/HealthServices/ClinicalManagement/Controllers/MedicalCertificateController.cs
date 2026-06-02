using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponseMedicalCertificatePagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs.MedicalCertificateResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/clinical-management/medical-certificates")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_CLINICAL",
        moduleName: "Health Service Clinical",
        displayName: "Medical Certificate",
        AreaName = "HealthServices",
        ControllerName = "MedicalCertificate",
        Description = "Surat keterangan medis pasien",
        SortOrder = 11
    )]
    [Tags("Health Services / Clinical Management / Medical Certificate")]
    public class MedicalCertificateController : ControllerBase
    {
        private const string LogCategory = "HealthServices.Clinical";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public MedicalCertificateController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<MedicalCertificateFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Medical Certificate", Description = "Melihat metadata filter surat medis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicalCertificate", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new MedicalCertificateFilterMetadataResponse
            {
                DefaultFilter = new MedicalCertificateDefaultFilterResponse(),
                SortOptions = new List<MedicalCertificateSortOptionResponse>
                {
                    new() { Value = "certificateDateTime", Label = "Tanggal surat" },
                    new() { Value = "medicalCertificateNumber", Label = "Nomor surat" },
                    new() { Value = "certificateTitle", Label = "Judul surat" },
                    new() { Value = "certificateType", Label = "Jenis surat" },
                    new() { Value = "certificateStatus", Label = "Status surat" },
                    new() { Value = "issuedAt", Label = "Tanggal terbit" },
                    new() { Value = "expiredDate", Label = "Tanggal kedaluwarsa" },
                    new() { Value = "isIssued", Label = "Sudah diterbitkan" },
                    new() { Value = "isVerified", Label = "Terverifikasi" },
                    new() { Value = "isApproved", Label = "Disetujui" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                CertificateTypeOptions = BuildEnumOptions<MedicalCertificateType>(),
                CertificateStatusOptions = BuildEnumOptions<MedicalCertificateStatus>(),
                RecipientTypeOptions = BuildEnumOptions<MedicalCertificateRecipientType>(),
                DeliveryMethodOptions = BuildEnumOptions<MedicalCertificateDeliveryMethod>(),
                FitnessStatusOptions = BuildEnumOptions<MedicalFitnessStatus>()
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "MedicalCertificate.GetFilterMetadata",
                "Mengambil metadata filter surat medis.",
                result
            );

            return Ok(ApiResponse<MedicalCertificateFilterMetadataResponse>.Ok(
                result,
                "Metadata filter surat medis berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseMedicalCertificatePagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Medical Certificate", Description = "Melihat data surat medis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicalCertificate", "Read")]
        public async Task<IActionResult> GetMedicalCertificates(
            [FromQuery] string? search,
            [FromQuery] Guid? patientId,
            [FromQuery] Guid? encounterId,
            [FromQuery] Guid? queueId,
            [FromQuery] Guid? assessmentId,
            [FromQuery] Guid? consultationId,
            [FromQuery] Guid? patientDiagnosisId,
            [FromQuery] Guid? diagnosisId,
            [FromQuery] Guid? clinicalDocumentId,
            [FromQuery] Guid? doctorId,
            [FromQuery] Guid? serviceUnitId,
            [FromQuery] Guid? clinicId,
            [FromQuery] MedicalCertificateType? certificateType,
            [FromQuery] MedicalCertificateStatus? certificateStatus,
            [FromQuery] MedicalCertificateRecipientType? recipientType,
            [FromQuery] MedicalCertificateDeliveryMethod? deliveryMethod,
            [FromQuery] MedicalFitnessStatus? fitnessStatus,
            [FromQuery] bool? isIssued,
            [FromQuery] bool? isVerified,
            [FromQuery] bool? isApproved,
            [FromQuery] bool? isRejected,
            [FromQuery] bool? isRevoked,
            [FromQuery] bool? isActive,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] DateTime? effectiveDate,
            [FromQuery] string? sortBy = "certificateDateTime",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery().AsNoTracking();

            query = ApplyFilters(query, search, patientId, encounterId, queueId, assessmentId, consultationId,
                patientDiagnosisId, diagnosisId, clinicalDocumentId, doctorId, serviceUnitId, clinicId,
                certificateType, certificateStatus, recipientType, deliveryMethod, fitnessStatus, isIssued,
                isVerified, isApproved, isRejected, isRevoked, isActive, startDate, endDate, effectiveDate);

            var totalData = await query.CountAsync();

            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new ResponseMedicalCertificatePagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = entities.Select(ToResponse).ToList()
            };

            return Ok(ApiResponse<ResponseMedicalCertificatePagedResult>.Ok(result, "Data surat medis berhasil diambil."));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<MedicalCertificateOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Medical Certificate", Description = "Melihat pilihan surat medis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicalCertificate", "Read")]
        public async Task<IActionResult> GetMedicalCertificateOptions(
            [FromQuery] Guid? patientId,
            [FromQuery] Guid? encounterId,
            [FromQuery] Guid? consultationId,
            [FromQuery] MedicalCertificateType? certificateType,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null)
        {
            var query = _dbContext.Set<TrxMedicalCertificate>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (onlyActive)
            {
                query = query.Where(x =>
                    x.IsActive &&
                    x.CertificateStatus != MedicalCertificateStatus.Cancelled &&
                    x.CertificateStatus != MedicalCertificateStatus.EnteredInError &&
                    x.CertificateStatus != MedicalCertificateStatus.Revoked);
            }

            if (patientId.HasValue && patientId.Value != Guid.Empty) query = query.Where(x => x.PatientId == patientId.Value);
            if (encounterId.HasValue && encounterId.Value != Guid.Empty) query = query.Where(x => x.EncounterId == encounterId.Value);
            if (consultationId.HasValue && consultationId.Value != Guid.Empty) query = query.Where(x => x.ConsultationId == consultationId.Value);
            if (certificateType.HasValue) query = query.Where(x => x.CertificateType == certificateType.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.MedicalCertificateNumber.ToLower().Contains(keyword) ||
                    x.CertificateTitle.ToLower().Contains(keyword) ||
                    (x.DiagnosisNameSnapshot != null && x.DiagnosisNameSnapshot.ToLower().Contains(keyword)) ||
                    (x.RecipientName != null && x.RecipientName.ToLower().Contains(keyword)));
            }

            var data = await query
                .OrderByDescending(x => x.CertificateDateTime)
                .Take(100)
                .Select(x => new MedicalCertificateOptionResponse
                {
                    Id = x.Id,
                    MedicalCertificateNumber = x.MedicalCertificateNumber,
                    PatientId = x.PatientId,
                    EncounterId = x.EncounterId,
                    ConsultationId = x.ConsultationId,
                    CertificateType = x.CertificateType,
                    CertificateStatus = x.CertificateStatus,
                    CertificateTitle = x.CertificateTitle,
                    CertificateDateTime = x.CertificateDateTime,
                    IssuedAt = x.IssuedAt,
                    ExpiredDate = x.ExpiredDate,
                    IsIssued = x.IsIssued,
                    IsVerified = x.IsVerified,
                    IsApproved = x.IsApproved
                })
                .ToListAsync();

            return Ok(ApiResponse<List<MedicalCertificateOptionResponse>>.Ok(data, "Data pilihan surat medis berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<MedicalCertificateDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Medical Certificate", Description = "Melihat detail surat medis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicalCertificate", "Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await BuildBaseQuery().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Surat medis tidak ditemukan."));

            return Ok(ApiResponse<MedicalCertificateDetailResponse>.Ok(ToDetailResponse(entity), "Detail surat medis berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<MedicalCertificateCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Medical Certificate", Description = "Membuat surat medis", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("MedicalCertificate", "Create")]
        public async Task<IActionResult> CreateMedicalCertificate([FromBody] CreateMedicalCertificateRequest request)
        {
            var validation = await ValidateCreateRequestAsync(request);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage ?? "Data surat medis tidak valid."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var context = await ResolveClinicalContextAsync(
                request.PatientId,
                request.EncounterId,
                request.QueueId,
                request.AssessmentId,
                request.ConsultationId,
                request.PatientDiagnosisId,
                request.ClinicalDocumentId,
                request.DoctorId,
                request.ServiceUnitId,
                request.ClinicId);

            if (!context.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, context.ErrorMessage ?? "Konteks surat medis tidak valid."));

            var diagnosisSnapshot = await BuildDiagnosisSnapshotAsync(
                context.PatientDiagnosisId,
                request.DiagnosisId,
                request.DiagnosisCodeSnapshot,
                request.DiagnosisNameSnapshot,
                request.DiagnosisMasterType,
                request.IcdVersion);

            if (!diagnosisSnapshot.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, diagnosisSnapshot.ErrorMessage ?? "Diagnosis surat medis tidak valid."));

            var entity = new TrxMedicalCertificate
            {
                Id = Guid.NewGuid(),
                MedicalCertificateNumber = await GenerateMedicalCertificateNumberAsync(now),
                PatientId = request.PatientId,
                EncounterId = context.EncounterId,
                QueueId = context.QueueId,
                AssessmentId = context.AssessmentId,
                ConsultationId = context.ConsultationId,
                PatientDiagnosisId = context.PatientDiagnosisId,
                DiagnosisId = diagnosisSnapshot.DiagnosisId,
                ClinicalDocumentId = context.ClinicalDocumentId,
                DoctorId = context.DoctorId,
                ServiceUnitId = context.ServiceUnitId,
                ClinicId = context.ClinicId,
                CertificateType = request.CertificateType,
                CertificateStatus = request.CertificateStatus,
                RecipientType = request.RecipientType,
                DeliveryMethod = request.DeliveryMethod,
                CertificateTitle = request.CertificateTitle.Trim(),
                CertificateCode = NormalizeNullableText(request.CertificateCode),
                CertificateCategoryName = NormalizeNullableText(request.CertificateCategoryName),
                CertificatePurpose = NormalizeNullableText(request.CertificatePurpose),
                DiagnosisCodeSnapshot = diagnosisSnapshot.DiagnosisCodeSnapshot,
                DiagnosisNameSnapshot = diagnosisSnapshot.DiagnosisNameSnapshot,
                DiagnosisMasterType = diagnosisSnapshot.DiagnosisMasterType,
                IcdVersion = diagnosisSnapshot.IcdVersion,
                ClinicalSummary = NormalizeNullableText(request.ClinicalSummary),
                MedicalRecommendation = NormalizeNullableText(request.MedicalRecommendation),
                CertificateStatement = NormalizeNullableText(request.CertificateStatement),
                AdditionalStatement = NormalizeNullableText(request.AdditionalStatement),
                RestrictionNote = NormalizeNullableText(request.RestrictionNote),
                FollowUpInstruction = NormalizeNullableText(request.FollowUpInstruction),
                SickLeaveStartDate = request.SickLeaveStartDate,
                SickLeaveEndDate = request.SickLeaveEndDate,
                SickLeaveDays = CalculateSickLeaveDays(request.SickLeaveStartDate, request.SickLeaveEndDate, request.SickLeaveDays),
                SickLeaveReason = NormalizeNullableText(request.SickLeaveReason),
                ControlDate = request.ControlDate,
                ControlClinicName = NormalizeNullableText(request.ControlClinicName),
                ReferralDate = request.ReferralDate,
                ReferralToProviderName = NormalizeNullableText(request.ReferralToProviderName),
                ReferralToDepartmentName = NormalizeNullableText(request.ReferralToDepartmentName),
                ReferralReason = NormalizeNullableText(request.ReferralReason),
                AdmissionDate = request.AdmissionDate,
                DischargeDate = request.DischargeDate,
                DeathDateTime = request.DeathDateTime,
                CauseOfDeath = NormalizeNullableText(request.CauseOfDeath),
                FitnessStatus = request.FitnessStatus,
                FitnessAssessmentNote = NormalizeNullableText(request.FitnessAssessmentNote),
                WorkRestrictionNote = NormalizeNullableText(request.WorkRestrictionNote),
                RequestedByName = NormalizeNullableText(request.RequestedByName),
                RequestedByRelationship = NormalizeNullableText(request.RequestedByRelationship),
                RecipientName = NormalizeNullableText(request.RecipientName),
                RecipientInstitutionName = NormalizeNullableText(request.RecipientInstitutionName),
                RecipientAddress = NormalizeNullableText(request.RecipientAddress),
                CertificateDateTime = request.CertificateDateTime ?? now,
                IssuedAt = request.IsIssued ? request.IssuedAt ?? now : request.IssuedAt,
                IssuedByDoctorId = NormalizeNullableGuid(request.IssuedByDoctorId) ?? context.DoctorId,
                IssuedByUserId = request.IsIssued ? actorUserId : NormalizeNullableGuid(request.IssuedByUserId),
                IssuePlace = NormalizeNullableText(request.IssuePlace),
                EffectiveStartDate = request.EffectiveStartDate,
                EffectiveEndDate = request.EffectiveEndDate,
                ExpiredDate = request.ExpiredDate,
                CertificateFilePath = NormalizeNullableText(request.CertificateFilePath),
                CertificateFileName = NormalizeNullableText(request.CertificateFileName),
                CertificateMimeType = NormalizeNullableText(request.CertificateMimeType),
                CertificateFileSizeBytes = request.CertificateFileSizeBytes,
                CertificateFileHash = NormalizeNullableText(request.CertificateFileHash),
                QrCodePath = NormalizeNullableText(request.QrCodePath),
                VerificationCode = NormalizeNullableText(request.VerificationCode),
                VerificationUrl = NormalizeNullableText(request.VerificationUrl),
                IsIssued = request.IsIssued,
                IsVerified = request.IsVerified,
                VerifiedAt = request.IsVerified ? now : null,
                VerifiedByUserId = request.IsVerified ? actorUserId : null,
                IsApproved = request.IsApproved,
                ApprovedAt = request.IsApproved ? now : null,
                ApprovedByUserId = request.IsApproved ? actorUserId : null,
                Notes = NormalizeNullableText(request.Notes),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            NormalizeMedicalCertificateData(entity);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            _dbContext.Set<TrxMedicalCertificate>().Add(entity);
            await _dbContext.SaveChangesAsync();

            if (entity.ConsultationId.HasValue)
                await UpdateConsultationMedicalCertificateCountAsync(entity.ConsultationId.Value, actorUserId, now);

            await transaction.CommitAsync();

            var response = ToCreateUpdateResponse(entity);

            await _loggerService.InfoAsync(LogCategory, "MedicalCertificate.CreateMedicalCertificate", "Membuat surat medis.", response);

            return Ok(ApiResponse<MedicalCertificateCreateResponse>.Ok(response, "Surat medis berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<MedicalCertificateUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Medical Certificate", Description = "Mengubah surat medis", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("MedicalCertificate", "Update")]
        public async Task<IActionResult> UpdateMedicalCertificate(Guid id, [FromBody] UpdateMedicalCertificateRequest request)
        {
            var entity = await _dbContext.Set<TrxMedicalCertificate>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Surat medis tidak ditemukan."));

            if (entity.CertificateStatus == MedicalCertificateStatus.Cancelled || entity.CertificateStatus == MedicalCertificateStatus.EnteredInError || entity.IsRevoked)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Surat medis yang sudah cancelled, entered in error, atau revoked tidak dapat diubah."));

            var validation = ValidateUpdateRequest(request);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage ?? "Data surat medis tidak valid."));

            var diagnosisSnapshot = await BuildDiagnosisSnapshotAsync(
                entity.PatientDiagnosisId,
                request.DiagnosisId,
                request.DiagnosisCodeSnapshot,
                request.DiagnosisNameSnapshot,
                request.DiagnosisMasterType,
                request.IcdVersion);

            if (!diagnosisSnapshot.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, diagnosisSnapshot.ErrorMessage ?? "Diagnosis surat medis tidak valid."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.CertificateType = request.CertificateType;
            entity.CertificateStatus = request.CertificateStatus;
            entity.RecipientType = request.RecipientType;
            entity.DeliveryMethod = request.DeliveryMethod;
            entity.CertificateTitle = request.CertificateTitle.Trim();
            entity.CertificateCode = NormalizeNullableText(request.CertificateCode);
            entity.CertificateCategoryName = NormalizeNullableText(request.CertificateCategoryName);
            entity.CertificatePurpose = NormalizeNullableText(request.CertificatePurpose);
            entity.DiagnosisId = diagnosisSnapshot.DiagnosisId;
            entity.DiagnosisCodeSnapshot = diagnosisSnapshot.DiagnosisCodeSnapshot;
            entity.DiagnosisNameSnapshot = diagnosisSnapshot.DiagnosisNameSnapshot;
            entity.DiagnosisMasterType = diagnosisSnapshot.DiagnosisMasterType;
            entity.IcdVersion = diagnosisSnapshot.IcdVersion;
            entity.ClinicalSummary = NormalizeNullableText(request.ClinicalSummary);
            entity.MedicalRecommendation = NormalizeNullableText(request.MedicalRecommendation);
            entity.CertificateStatement = NormalizeNullableText(request.CertificateStatement);
            entity.AdditionalStatement = NormalizeNullableText(request.AdditionalStatement);
            entity.RestrictionNote = NormalizeNullableText(request.RestrictionNote);
            entity.FollowUpInstruction = NormalizeNullableText(request.FollowUpInstruction);
            entity.SickLeaveStartDate = request.SickLeaveStartDate;
            entity.SickLeaveEndDate = request.SickLeaveEndDate;
            entity.SickLeaveDays = CalculateSickLeaveDays(request.SickLeaveStartDate, request.SickLeaveEndDate, request.SickLeaveDays);
            entity.SickLeaveReason = NormalizeNullableText(request.SickLeaveReason);
            entity.ControlDate = request.ControlDate;
            entity.ControlClinicName = NormalizeNullableText(request.ControlClinicName);
            entity.ReferralDate = request.ReferralDate;
            entity.ReferralToProviderName = NormalizeNullableText(request.ReferralToProviderName);
            entity.ReferralToDepartmentName = NormalizeNullableText(request.ReferralToDepartmentName);
            entity.ReferralReason = NormalizeNullableText(request.ReferralReason);
            entity.AdmissionDate = request.AdmissionDate;
            entity.DischargeDate = request.DischargeDate;
            entity.DeathDateTime = request.DeathDateTime;
            entity.CauseOfDeath = NormalizeNullableText(request.CauseOfDeath);
            entity.FitnessStatus = request.FitnessStatus;
            entity.FitnessAssessmentNote = NormalizeNullableText(request.FitnessAssessmentNote);
            entity.WorkRestrictionNote = NormalizeNullableText(request.WorkRestrictionNote);
            entity.RequestedByName = NormalizeNullableText(request.RequestedByName);
            entity.RequestedByRelationship = NormalizeNullableText(request.RequestedByRelationship);
            entity.RecipientName = NormalizeNullableText(request.RecipientName);
            entity.RecipientInstitutionName = NormalizeNullableText(request.RecipientInstitutionName);
            entity.RecipientAddress = NormalizeNullableText(request.RecipientAddress);
            entity.IssuedByDoctorId = NormalizeNullableGuid(request.IssuedByDoctorId) ?? entity.IssuedByDoctorId;
            entity.IssuedByUserId = NormalizeNullableGuid(request.IssuedByUserId) ?? entity.IssuedByUserId;
            entity.IssuePlace = NormalizeNullableText(request.IssuePlace);
            entity.EffectiveStartDate = request.EffectiveStartDate;
            entity.EffectiveEndDate = request.EffectiveEndDate;
            entity.ExpiredDate = request.ExpiredDate;
            entity.CertificateFilePath = NormalizeNullableText(request.CertificateFilePath);
            entity.CertificateFileName = NormalizeNullableText(request.CertificateFileName);
            entity.CertificateMimeType = NormalizeNullableText(request.CertificateMimeType);
            entity.CertificateFileSizeBytes = request.CertificateFileSizeBytes;
            entity.CertificateFileHash = NormalizeNullableText(request.CertificateFileHash);
            entity.QrCodePath = NormalizeNullableText(request.QrCodePath);
            entity.VerificationCode = NormalizeNullableText(request.VerificationCode);
            entity.VerificationUrl = NormalizeNullableText(request.VerificationUrl);
            entity.Notes = NormalizeNullableText(request.Notes);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            NormalizeMedicalCertificateData(entity);
            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<MedicalCertificateUpdateResponse>.Ok(ToUpdateResponse(entity), "Surat medis berhasil diubah."));
        }

        [HttpPatch("{id:guid}/issue")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Issue Medical Certificate", Description = "Menerbitkan surat medis", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("MedicalCertificate", "Update")]
        public async Task<IActionResult> IssueMedicalCertificate(Guid id, [FromBody] IssueMedicalCertificateRequest request)
        {
            var entity = await _dbContext.Set<TrxMedicalCertificate>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Surat medis tidak ditemukan."));
            if (entity.CertificateStatus == MedicalCertificateStatus.Cancelled || entity.CertificateStatus == MedicalCertificateStatus.EnteredInError || entity.IsRevoked)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Surat medis tidak dapat diterbitkan."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.IsIssued = true;
            entity.IssuedAt = now;
            entity.IssuedByDoctorId = NormalizeNullableGuid(request.IssuedByDoctorId) ?? entity.DoctorId ?? entity.IssuedByDoctorId;
            entity.IssuedByUserId = actorUserId;
            entity.IssuePlace = NormalizeNullableText(request.IssuePlace) ?? entity.IssuePlace;
            entity.CertificateStatus = MedicalCertificateStatus.Issued;
            entity.CertificateFilePath = NormalizeNullableText(request.CertificateFilePath) ?? entity.CertificateFilePath;
            entity.CertificateFileName = NormalizeNullableText(request.CertificateFileName) ?? entity.CertificateFileName;
            entity.CertificateMimeType = NormalizeNullableText(request.CertificateMimeType) ?? entity.CertificateMimeType;
            entity.CertificateFileSizeBytes = request.CertificateFileSizeBytes ?? entity.CertificateFileSizeBytes;
            entity.CertificateFileHash = NormalizeNullableText(request.CertificateFileHash) ?? entity.CertificateFileHash;
            entity.QrCodePath = NormalizeNullableText(request.QrCodePath) ?? entity.QrCodePath;
            entity.VerificationCode = NormalizeNullableText(request.VerificationCode) ?? entity.VerificationCode;
            entity.VerificationUrl = NormalizeNullableText(request.VerificationUrl) ?? entity.VerificationUrl;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(null, "Surat medis berhasil diterbitkan."));
        }

        [HttpPatch("{id:guid}/verify")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Verify Medical Certificate", Description = "Verifikasi surat medis", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("MedicalCertificate", "Update")]
        public async Task<IActionResult> VerifyMedicalCertificate(Guid id, [FromBody] VerifyMedicalCertificateRequest request)
        {
            var entity = await _dbContext.Set<TrxMedicalCertificate>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Surat medis tidak ditemukan."));
            if (entity.CertificateStatus == MedicalCertificateStatus.Cancelled || entity.CertificateStatus == MedicalCertificateStatus.EnteredInError || entity.IsRevoked)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Surat medis tidak dapat diverifikasi."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.IsVerified = true;
            entity.VerifiedAt = now;
            entity.VerifiedByUserId = actorUserId;
            entity.VerificationNote = NormalizeNullableText(request.VerificationNote);
            entity.CertificateStatus = MedicalCertificateStatus.Verified;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(null, "Surat medis berhasil diverifikasi."));
        }

        [HttpPatch("{id:guid}/approve")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Approve Medical Certificate", Description = "Menyetujui surat medis", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("MedicalCertificate", "Update")]
        public async Task<IActionResult> ApproveMedicalCertificate(Guid id, [FromBody] ApproveMedicalCertificateRequest request)
        {
            var entity = await _dbContext.Set<TrxMedicalCertificate>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Surat medis tidak ditemukan."));
            if (entity.CertificateStatus == MedicalCertificateStatus.Cancelled || entity.CertificateStatus == MedicalCertificateStatus.EnteredInError || entity.IsRevoked)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Surat medis tidak dapat disetujui."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.IsApproved = true;
            entity.ApprovedAt = now;
            entity.ApprovedByUserId = actorUserId;
            entity.ApprovalNote = NormalizeNullableText(request.ApprovalNote);
            entity.IsRejected = false;
            entity.RejectedAt = null;
            entity.RejectedByUserId = null;
            entity.RejectionReason = null;
            entity.CertificateStatus = MedicalCertificateStatus.Approved;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(null, "Surat medis berhasil disetujui."));
        }

        [HttpPatch("{id:guid}/reject")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Reject Medical Certificate", Description = "Menolak surat medis", AccessType = AccessTypes.Update, SortOrder = 7)]
        [AccessPermission("MedicalCertificate", "Update")]
        public async Task<IActionResult> RejectMedicalCertificate(Guid id, [FromBody] RejectMedicalCertificateRequest request)
        {
            var entity = await _dbContext.Set<TrxMedicalCertificate>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Surat medis tidak ditemukan."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.IsRejected = true;
            entity.RejectedAt = now;
            entity.RejectedByUserId = actorUserId;
            entity.RejectionReason = request.RejectionReason.Trim();
            entity.IsApproved = false;
            entity.ApprovedAt = null;
            entity.ApprovedByUserId = null;
            entity.ApprovalNote = null;
            entity.CertificateStatus = MedicalCertificateStatus.Rejected;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(null, "Surat medis berhasil ditolak."));
        }

        [HttpPatch("{id:guid}/revoke")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Revoke Medical Certificate", Description = "Mencabut surat medis", AccessType = AccessTypes.Update, SortOrder = 8)]
        [AccessPermission("MedicalCertificate", "Update")]
        public async Task<IActionResult> RevokeMedicalCertificate(Guid id, [FromBody] RevokeMedicalCertificateRequest request)
        {
            var entity = await _dbContext.Set<TrxMedicalCertificate>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Surat medis tidak ditemukan."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.IsRevoked = true;
            entity.RevokedAt = now;
            entity.RevokedByUserId = actorUserId;
            entity.RevocationReason = request.RevocationReason.Trim();
            entity.CertificateStatus = MedicalCertificateStatus.Revoked;
            entity.IsActive = false;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(null, "Surat medis berhasil dicabut."));
        }

        [HttpPatch("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Cancel Medical Certificate", Description = "Membatalkan surat medis", AccessType = AccessTypes.Update, SortOrder = 9)]
        [AccessPermission("MedicalCertificate", "Update")]
        public async Task<IActionResult> CancelMedicalCertificate(Guid id, [FromBody] CancelMedicalCertificateRequest request)
        {
            var entity = await _dbContext.Set<TrxMedicalCertificate>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Surat medis tidak ditemukan."));
            if (entity.CertificateStatus == MedicalCertificateStatus.Cancelled)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Surat medis sudah cancelled."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            entity.CertificateStatus = MedicalCertificateStatus.Cancelled;
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
            if (entity.ConsultationId.HasValue)
                await UpdateConsultationMedicalCertificateCountAsync(entity.ConsultationId.Value, actorUserId, now);
            await transaction.CommitAsync();
            return Ok(ApiResponse<object>.Ok(null, "Surat medis berhasil dibatalkan."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Medical Certificate", Description = "Menghapus surat medis", AccessType = AccessTypes.Delete, SortOrder = 10)]
        [AccessPermission("MedicalCertificate", "Delete")]
        public async Task<IActionResult> DeleteMedicalCertificate(Guid id)
        {
            var entity = await _dbContext.Set<TrxMedicalCertificate>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null) return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Surat medis tidak ditemukan."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            entity.IsDelete = true;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.IsActive = false;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync();
            if (entity.ConsultationId.HasValue)
                await UpdateConsultationMedicalCertificateCountAsync(entity.ConsultationId.Value, actorUserId, now);
            await transaction.CommitAsync();
            return Ok(ApiResponse<object>.Ok(null, "Surat medis berhasil dihapus."));
        }

        private IQueryable<TrxMedicalCertificate> BuildBaseQuery()
        {
            return _dbContext.Set<TrxMedicalCertificate>()
                .Include(x => x.Patient)
                .Include(x => x.Encounter)
                .Include(x => x.Queue)
                .Include(x => x.Assessment)
                .Include(x => x.Consultation)
                .Include(x => x.PatientDiagnosis)
                .Include(x => x.ClinicalDocument)
                .Include(x => x.Doctor)
                .Include(x => x.IssuedByDoctor)
                .Include(x => x.ServiceUnit)
                .Include(x => x.Clinic)
                .Include(x => x.IssuedByUser)
                .Include(x => x.VerifiedByUser)
                .Include(x => x.ApprovedByUser)
                .Include(x => x.RejectedByUser)
                .Include(x => x.RevokedByUser)
                .Include(x => x.CancelledByUser)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<TrxMedicalCertificate> ApplyFilters(
            IQueryable<TrxMedicalCertificate> query,
            string? search,
            Guid? patientId,
            Guid? encounterId,
            Guid? queueId,
            Guid? assessmentId,
            Guid? consultationId,
            Guid? patientDiagnosisId,
            Guid? diagnosisId,
            Guid? clinicalDocumentId,
            Guid? doctorId,
            Guid? serviceUnitId,
            Guid? clinicId,
            MedicalCertificateType? certificateType,
            MedicalCertificateStatus? certificateStatus,
            MedicalCertificateRecipientType? recipientType,
            MedicalCertificateDeliveryMethod? deliveryMethod,
            MedicalFitnessStatus? fitnessStatus,
            bool? isIssued,
            bool? isVerified,
            bool? isApproved,
            bool? isRejected,
            bool? isRevoked,
            bool? isActive,
            DateTime? startDate,
            DateTime? endDate,
            DateTime? effectiveDate)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.MedicalCertificateNumber.ToLower().Contains(keyword) ||
                    x.CertificateTitle.ToLower().Contains(keyword) ||
                    (x.CertificateCode != null && x.CertificateCode.ToLower().Contains(keyword)) ||
                    (x.CertificatePurpose != null && x.CertificatePurpose.ToLower().Contains(keyword)) ||
                    (x.DiagnosisCodeSnapshot != null && x.DiagnosisCodeSnapshot.ToLower().Contains(keyword)) ||
                    (x.DiagnosisNameSnapshot != null && x.DiagnosisNameSnapshot.ToLower().Contains(keyword)) ||
                    (x.RecipientName != null && x.RecipientName.ToLower().Contains(keyword)) ||
                    (x.RecipientInstitutionName != null && x.RecipientInstitutionName.ToLower().Contains(keyword)) ||
                    (x.VerificationCode != null && x.VerificationCode.ToLower().Contains(keyword)) ||
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
            if (diagnosisId.HasValue && diagnosisId.Value != Guid.Empty) query = query.Where(x => x.DiagnosisId == diagnosisId.Value);
            if (clinicalDocumentId.HasValue && clinicalDocumentId.Value != Guid.Empty) query = query.Where(x => x.ClinicalDocumentId == clinicalDocumentId.Value);
            if (doctorId.HasValue && doctorId.Value != Guid.Empty) query = query.Where(x => x.DoctorId == doctorId.Value);
            if (serviceUnitId.HasValue && serviceUnitId.Value != Guid.Empty) query = query.Where(x => x.ServiceUnitId == serviceUnitId.Value);
            if (clinicId.HasValue && clinicId.Value != Guid.Empty) query = query.Where(x => x.ClinicId == clinicId.Value);
            if (certificateType.HasValue) query = query.Where(x => x.CertificateType == certificateType.Value);
            if (certificateStatus.HasValue) query = query.Where(x => x.CertificateStatus == certificateStatus.Value);
            if (recipientType.HasValue) query = query.Where(x => x.RecipientType == recipientType.Value);
            if (deliveryMethod.HasValue) query = query.Where(x => x.DeliveryMethod == deliveryMethod.Value);
            if (fitnessStatus.HasValue) query = query.Where(x => x.FitnessStatus == fitnessStatus.Value);
            if (isIssued.HasValue) query = query.Where(x => x.IsIssued == isIssued.Value);
            if (isVerified.HasValue) query = query.Where(x => x.IsVerified == isVerified.Value);
            if (isApproved.HasValue) query = query.Where(x => x.IsApproved == isApproved.Value);
            if (isRejected.HasValue) query = query.Where(x => x.IsRejected == isRejected.Value);
            if (isRevoked.HasValue) query = query.Where(x => x.IsRevoked == isRevoked.Value);
            if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);
            if (startDate.HasValue) query = query.Where(x => x.CertificateDateTime >= startDate.Value.Date);
            if (endDate.HasValue) query = query.Where(x => x.CertificateDateTime < endDate.Value.Date.AddDays(1));

            if (effectiveDate.HasValue)
            {
                var selectedDate = effectiveDate.Value.Date;
                query = query.Where(x =>
                    (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value.Date <= selectedDate) &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= selectedDate) &&
                    (!x.ExpiredDate.HasValue || x.ExpiredDate.Value.Date >= selectedDate));
            }

            return query;
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateCreateRequestAsync(CreateMedicalCertificateRequest request)
        {
            if (request.PatientId == Guid.Empty) return (false, "PatientId wajib diisi.");
            if (string.IsNullOrWhiteSpace(request.CertificateTitle)) return (false, "Judul surat medis wajib diisi.");

            var patientExists = await _dbContext.Set<MstPatient>().AnyAsync(x => x.Id == request.PatientId && !x.IsDelete);
            if (!patientExists) return (false, "Pasien tidak ditemukan.");

            return ValidateDateAndFileValues(request.SickLeaveStartDate, request.SickLeaveEndDate,
                request.AdmissionDate, request.DischargeDate, request.EffectiveStartDate,
                request.EffectiveEndDate, request.CertificateFileSizeBytes);
        }

        private static (bool IsValid, string? ErrorMessage) ValidateUpdateRequest(UpdateMedicalCertificateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.CertificateTitle)) return (false, "Judul surat medis wajib diisi.");
            return ValidateDateAndFileValues(request.SickLeaveStartDate, request.SickLeaveEndDate,
                request.AdmissionDate, request.DischargeDate, request.EffectiveStartDate,
                request.EffectiveEndDate, request.CertificateFileSizeBytes);
        }

        private static (bool IsValid, string? ErrorMessage) ValidateDateAndFileValues(
            DateTime? sickLeaveStartDate,
            DateTime? sickLeaveEndDate,
            DateTime? admissionDate,
            DateTime? dischargeDate,
            DateTime? effectiveStartDate,
            DateTime? effectiveEndDate,
            long? fileSizeBytes)
        {
            if (sickLeaveStartDate.HasValue && sickLeaveEndDate.HasValue && sickLeaveStartDate.Value.Date > sickLeaveEndDate.Value.Date)
                return (false, "Tanggal mulai surat sakit tidak boleh lebih besar dari tanggal selesai surat sakit.");

            if (admissionDate.HasValue && dischargeDate.HasValue && admissionDate.Value.Date > dischargeDate.Value.Date)
                return (false, "Tanggal masuk rawat inap tidak boleh lebih besar dari tanggal keluar.");

            if (effectiveStartDate.HasValue && effectiveEndDate.HasValue && effectiveStartDate.Value.Date > effectiveEndDate.Value.Date)
                return (false, "Tanggal mulai berlaku tidak boleh lebih besar dari tanggal akhir berlaku.");

            if (fileSizeBytes.HasValue && fileSizeBytes.Value < 0)
                return (false, "Ukuran file surat medis tidak valid.");

            return (true, null);
        }

        private async Task<MedicalCertificateContextResult> ResolveClinicalContextAsync(
            Guid patientId,
            Guid? encounterId,
            Guid? queueId,
            Guid? assessmentId,
            Guid? consultationId,
            Guid? patientDiagnosisId,
            Guid? clinicalDocumentId,
            Guid? doctorId,
            Guid? serviceUnitId,
            Guid? clinicId)
        {
            var result = new MedicalCertificateContextResult
            {
                EncounterId = NormalizeNullableGuid(encounterId),
                QueueId = NormalizeNullableGuid(queueId),
                AssessmentId = NormalizeNullableGuid(assessmentId),
                ConsultationId = NormalizeNullableGuid(consultationId),
                PatientDiagnosisId = NormalizeNullableGuid(patientDiagnosisId),
                ClinicalDocumentId = NormalizeNullableGuid(clinicalDocumentId),
                DoctorId = NormalizeNullableGuid(doctorId),
                ServiceUnitId = NormalizeNullableGuid(serviceUnitId),
                ClinicId = NormalizeNullableGuid(clinicId)
            };

            if (result.PatientDiagnosisId.HasValue)
            {
                var patientDiagnosis = await _dbContext.Set<TrxPatientDiagnosis>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == result.PatientDiagnosisId.Value && !x.IsDelete);

                if (patientDiagnosis == null) return MedicalCertificateContextResult.Fail("Diagnosis pasien tidak ditemukan.");
                if (patientDiagnosis.PatientId != patientId) return MedicalCertificateContextResult.Fail("Diagnosis pasien tidak sesuai dengan pasien.");

                result.EncounterId = patientDiagnosis.EncounterId;
                result.ConsultationId = patientDiagnosis.ConsultationId;
                result.DoctorId = patientDiagnosis.DoctorId;
                result.ServiceUnitId = patientDiagnosis.ServiceUnitId;
                result.ClinicId = patientDiagnosis.ClinicId;
                return result.Ok();
            }

            if (result.ConsultationId.HasValue)
            {
                var consultation = await _dbContext.Set<TrxDoctorConsultation>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == result.ConsultationId.Value && !x.IsDelete);

                if (consultation == null) return MedicalCertificateContextResult.Fail("Konsultasi dokter tidak ditemukan.");
                if (consultation.PatientId != patientId) return MedicalCertificateContextResult.Fail("Konsultasi dokter tidak sesuai dengan pasien.");

                result.EncounterId = consultation.EncounterId;
                result.QueueId = consultation.QueueId;
                result.AssessmentId = consultation.AssessmentId ?? result.AssessmentId;
                result.DoctorId = consultation.DoctorId;
                result.ServiceUnitId = consultation.ServiceUnitId;
                result.ClinicId = consultation.ClinicId;
                return result.Ok();
            }

            if (result.AssessmentId.HasValue)
            {
                var assessment = await _dbContext.Set<TrxPatientAssessment>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == result.AssessmentId.Value && !x.IsDelete);

                if (assessment == null) return MedicalCertificateContextResult.Fail("Assessment pasien tidak ditemukan.");
                if (assessment.PatientId != patientId) return MedicalCertificateContextResult.Fail("Assessment pasien tidak sesuai dengan pasien.");

                result.EncounterId = assessment.EncounterId;
                result.QueueId = assessment.QueueId;
                result.DoctorId = assessment.DoctorId ?? result.DoctorId;
                result.ServiceUnitId = assessment.ServiceUnitId;
                result.ClinicId = assessment.ClinicId;
                return result.Ok();
            }

            if (result.QueueId.HasValue)
            {
                var queue = await _dbContext.Set<TrxQueue>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == result.QueueId.Value && !x.IsDelete);

                if (queue == null) return MedicalCertificateContextResult.Fail("Queue pasien tidak ditemukan.");
                if (queue.PatientId != patientId) return MedicalCertificateContextResult.Fail("Queue tidak sesuai dengan pasien.");

                result.EncounterId = queue.EncounterId;
                result.DoctorId = queue.DoctorId ?? result.DoctorId;
                result.ServiceUnitId = queue.ServiceUnitId;
                result.ClinicId = queue.ClinicId;
                return result.Ok();
            }

            if (result.EncounterId.HasValue)
            {
                var encounter = await _dbContext.Set<TrxPatientEncounter>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == result.EncounterId.Value && !x.IsDelete);

                if (encounter == null) return MedicalCertificateContextResult.Fail("Encounter pasien tidak ditemukan.");
                if (encounter.PatientId != patientId) return MedicalCertificateContextResult.Fail("Encounter tidak sesuai dengan pasien.");
            }

            if (result.ClinicalDocumentId.HasValue)
            {
                var document = await _dbContext.Set<TrxPatientClinicalDocument>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == result.ClinicalDocumentId.Value && !x.IsDelete);

                if (document == null) return MedicalCertificateContextResult.Fail("Dokumen klinis tidak ditemukan.");
                if (document.PatientId != patientId) return MedicalCertificateContextResult.Fail("Dokumen klinis tidak sesuai dengan pasien.");
            }

            return result.Ok();
        }

        private async Task<DiagnosisSnapshotResult> BuildDiagnosisSnapshotAsync(
            Guid? patientDiagnosisId,
            Guid? diagnosisId,
            string? diagnosisCodeSnapshot,
            string? diagnosisNameSnapshot,
            string? diagnosisMasterType,
            string? icdVersion)
        {
            var result = new DiagnosisSnapshotResult
            {
                DiagnosisId = NormalizeNullableGuid(diagnosisId),
                DiagnosisCodeSnapshot = NormalizeNullableText(diagnosisCodeSnapshot),
                DiagnosisNameSnapshot = NormalizeNullableText(diagnosisNameSnapshot),
                DiagnosisMasterType = NormalizeNullableText(diagnosisMasterType) ?? "Manual",
                IcdVersion = NormalizeNullableText(icdVersion),
                IsValid = true
            };

            if (patientDiagnosisId.HasValue && patientDiagnosisId.Value != Guid.Empty)
            {
                var patientDiagnosis = await _dbContext.Set<TrxPatientDiagnosis>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == patientDiagnosisId.Value && !x.IsDelete);

                if (patientDiagnosis == null) return DiagnosisSnapshotResult.Fail("Diagnosis pasien tidak ditemukan.");

                result.DiagnosisId = patientDiagnosis.DiagnosisId;
                result.DiagnosisCodeSnapshot = patientDiagnosis.DiagnosisCode;
                result.DiagnosisNameSnapshot = patientDiagnosis.DiagnosisName;
                result.DiagnosisMasterType = patientDiagnosis.DiagnosisMasterType;
                result.IcdVersion = patientDiagnosis.IcdVersion;
                return result;
            }

            if (result.DiagnosisId.HasValue)
            {
                var diagnosisExists = await _dbContext.Set<MstDiagnosis>()
                    .AnyAsync(x => x.Id == result.DiagnosisId.Value && !x.IsDelete);

                if (!diagnosisExists) return DiagnosisSnapshotResult.Fail("Master diagnosis tidak ditemukan.");
            }

            return result;
        }

        private async Task<string> GenerateMedicalCertificateNumberAsync(DateTime now)
        {
            var prefix = $"MCF-{now:yyyyMMdd}";
            var countToday = await _dbContext.Set<TrxMedicalCertificate>()
                .CountAsync(x => x.MedicalCertificateNumber.StartsWith(prefix));
            return $"{prefix}-{countToday + 1:0000}";
        }

        private async Task UpdateConsultationMedicalCertificateCountAsync(Guid consultationId, Guid actorUserId, DateTime now)
        {
            var consultation = await _dbContext.Set<TrxDoctorConsultation>()
                .FirstOrDefaultAsync(x => x.Id == consultationId && !x.IsDelete);

            if (consultation == null) return;

            var certificateCount = await _dbContext.Set<TrxMedicalCertificate>()
                .CountAsync(x =>
                    !x.IsDelete &&
                    x.ConsultationId == consultationId &&
                    x.CertificateStatus != MedicalCertificateStatus.Cancelled &&
                    x.CertificateStatus != MedicalCertificateStatus.EnteredInError &&
                    x.CertificateStatus != MedicalCertificateStatus.Revoked);

            consultation.MedicalCertificateCount = certificateCount;
            consultation.UpdateDateTime = now;
            consultation.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync();
        }

        private static int? CalculateSickLeaveDays(DateTime? sickLeaveStartDate, DateTime? sickLeaveEndDate, int? sickLeaveDays)
        {
            if (sickLeaveDays.HasValue && sickLeaveDays.Value > 0) return sickLeaveDays.Value;
            if (!sickLeaveStartDate.HasValue || !sickLeaveEndDate.HasValue) return sickLeaveDays;
            return (sickLeaveEndDate.Value.Date - sickLeaveStartDate.Value.Date).Days + 1;
        }

        private static IQueryable<TrxMedicalCertificate> ApplySorting(IQueryable<TrxMedicalCertificate> query, string? sortBy, string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "certificateDateTime").ToLowerInvariant() switch
            {
                "medicalcertificatenumber" => isDesc ? query.OrderByDescending(x => x.MedicalCertificateNumber) : query.OrderBy(x => x.MedicalCertificateNumber),
                "certificatetitle" => isDesc ? query.OrderByDescending(x => x.CertificateTitle) : query.OrderBy(x => x.CertificateTitle),
                "certificatetype" => isDesc ? query.OrderByDescending(x => x.CertificateType) : query.OrderBy(x => x.CertificateType),
                "certificatestatus" => isDesc ? query.OrderByDescending(x => x.CertificateStatus) : query.OrderBy(x => x.CertificateStatus),
                "issuedat" => isDesc ? query.OrderByDescending(x => x.IssuedAt) : query.OrderBy(x => x.IssuedAt),
                "expireddate" => isDesc ? query.OrderByDescending(x => x.ExpiredDate) : query.OrderBy(x => x.ExpiredDate),
                "isissued" => isDesc ? query.OrderByDescending(x => x.IsIssued) : query.OrderBy(x => x.IsIssued),
                "isverified" => isDesc ? query.OrderByDescending(x => x.IsVerified) : query.OrderBy(x => x.IsVerified),
                "isapproved" => isDesc ? query.OrderByDescending(x => x.IsApproved) : query.OrderBy(x => x.IsApproved),
                "createdatetime" => isDesc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                _ => isDesc ? query.OrderByDescending(x => x.CertificateDateTime) : query.OrderBy(x => x.CertificateDateTime)
            };
        }

        private static MedicalCertificateResponse ToResponse(TrxMedicalCertificate x)
        {
            return new MedicalCertificateResponse
            {
                Id = x.Id,
                MedicalCertificateNumber = x.MedicalCertificateNumber,
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
                DiagnosisId = x.DiagnosisId,
                ClinicalDocumentId = x.ClinicalDocumentId,
                ClinicalDocumentNumber = x.ClinicalDocument != null ? x.ClinicalDocument.ClinicalDocumentNumber : null,
                DoctorId = x.DoctorId,
                DoctorName = x.Doctor != null ? x.Doctor.FullName : null,
                ServiceUnitId = x.ServiceUnitId,
                ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null,
                ClinicId = x.ClinicId,
                ClinicName = x.Clinic != null ? x.Clinic.ClinicName : null,
                CertificateType = x.CertificateType,
                CertificateStatus = x.CertificateStatus,
                RecipientType = x.RecipientType,
                DeliveryMethod = x.DeliveryMethod,
                CertificateTitle = x.CertificateTitle,
                CertificateCode = x.CertificateCode,
                CertificateCategoryName = x.CertificateCategoryName,
                CertificatePurpose = x.CertificatePurpose,
                DiagnosisCodeSnapshot = x.DiagnosisCodeSnapshot,
                DiagnosisNameSnapshot = x.DiagnosisNameSnapshot,
                DiagnosisMasterType = x.DiagnosisMasterType,
                CertificateDateTime = x.CertificateDateTime,
                IssuedAt = x.IssuedAt,
                IssuedByDoctorId = x.IssuedByDoctorId,
                IssuedByDoctorName = x.IssuedByDoctor != null ? x.IssuedByDoctor.FullName : null,
                IssuedByUserId = x.IssuedByUserId,
                IssuedByUserName = x.IssuedByUser != null ? x.IssuedByUser.DisplayName : null,
                SickLeaveStartDate = x.SickLeaveStartDate,
                SickLeaveEndDate = x.SickLeaveEndDate,
                SickLeaveDays = x.SickLeaveDays,
                ControlDate = x.ControlDate,
                ReferralDate = x.ReferralDate,
                ExpiredDate = x.ExpiredDate,
                FitnessStatus = x.FitnessStatus,
                IsIssued = x.IsIssued,
                IsVerified = x.IsVerified,
                VerifiedAt = x.VerifiedAt,
                VerifiedByUserId = x.VerifiedByUserId,
                VerifiedByUserName = x.VerifiedByUser != null ? x.VerifiedByUser.DisplayName : null,
                IsApproved = x.IsApproved,
                ApprovedAt = x.ApprovedAt,
                ApprovedByUserId = x.ApprovedByUserId,
                ApprovedByUserName = x.ApprovedByUser != null ? x.ApprovedByUser.DisplayName : null,
                IsRejected = x.IsRejected,
                IsRevoked = x.IsRevoked,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime
            };
        }

        private static MedicalCertificateDetailResponse ToDetailResponse(TrxMedicalCertificate x)
        {
            var r = new MedicalCertificateDetailResponse
            {
                Id = x.Id,
                MedicalCertificateNumber = x.MedicalCertificateNumber,
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
                DiagnosisId = x.DiagnosisId,
                ClinicalDocumentId = x.ClinicalDocumentId,
                ClinicalDocumentNumber = x.ClinicalDocument != null ? x.ClinicalDocument.ClinicalDocumentNumber : null,
                DoctorId = x.DoctorId,
                DoctorName = x.Doctor != null ? x.Doctor.FullName : null,
                ServiceUnitId = x.ServiceUnitId,
                ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null,
                ClinicId = x.ClinicId,
                ClinicName = x.Clinic != null ? x.Clinic.ClinicName : null,
                CertificateType = x.CertificateType,
                CertificateStatus = x.CertificateStatus,
                RecipientType = x.RecipientType,
                DeliveryMethod = x.DeliveryMethod,
                CertificateTitle = x.CertificateTitle,
                CertificateCode = x.CertificateCode,
                CertificateCategoryName = x.CertificateCategoryName,
                CertificatePurpose = x.CertificatePurpose,
                DiagnosisCodeSnapshot = x.DiagnosisCodeSnapshot,
                DiagnosisNameSnapshot = x.DiagnosisNameSnapshot,
                DiagnosisMasterType = x.DiagnosisMasterType,
                IcdVersion = x.IcdVersion,
                ClinicalSummary = x.ClinicalSummary,
                MedicalRecommendation = x.MedicalRecommendation,
                CertificateStatement = x.CertificateStatement,
                AdditionalStatement = x.AdditionalStatement,
                RestrictionNote = x.RestrictionNote,
                FollowUpInstruction = x.FollowUpInstruction,
                SickLeaveStartDate = x.SickLeaveStartDate,
                SickLeaveEndDate = x.SickLeaveEndDate,
                SickLeaveDays = x.SickLeaveDays,
                SickLeaveReason = x.SickLeaveReason,
                ControlDate = x.ControlDate,
                ControlClinicName = x.ControlClinicName,
                ReferralDate = x.ReferralDate,
                ReferralToProviderName = x.ReferralToProviderName,
                ReferralToDepartmentName = x.ReferralToDepartmentName,
                ReferralReason = x.ReferralReason,
                AdmissionDate = x.AdmissionDate,
                DischargeDate = x.DischargeDate,
                DeathDateTime = x.DeathDateTime,
                CauseOfDeath = x.CauseOfDeath,
                FitnessStatus = x.FitnessStatus,
                FitnessAssessmentNote = x.FitnessAssessmentNote,
                WorkRestrictionNote = x.WorkRestrictionNote,
                RequestedByName = x.RequestedByName,
                RequestedByRelationship = x.RequestedByRelationship,
                RecipientName = x.RecipientName,
                RecipientInstitutionName = x.RecipientInstitutionName,
                RecipientAddress = x.RecipientAddress,
                CertificateDateTime = x.CertificateDateTime,
                IssuedAt = x.IssuedAt,
                IssuedByDoctorId = x.IssuedByDoctorId,
                IssuedByDoctorName = x.IssuedByDoctor != null ? x.IssuedByDoctor.FullName : null,
                IssuedByUserId = x.IssuedByUserId,
                IssuedByUserName = x.IssuedByUser != null ? x.IssuedByUser.DisplayName : null,
                IssuePlace = x.IssuePlace,
                EffectiveStartDate = x.EffectiveStartDate,
                EffectiveEndDate = x.EffectiveEndDate,
                ExpiredDate = x.ExpiredDate,
                CertificateFilePath = x.CertificateFilePath,
                CertificateFileName = x.CertificateFileName,
                CertificateMimeType = x.CertificateMimeType,
                CertificateFileSizeBytes = x.CertificateFileSizeBytes,
                CertificateFileHash = x.CertificateFileHash,
                QrCodePath = x.QrCodePath,
                VerificationCode = x.VerificationCode,
                VerificationUrl = x.VerificationUrl,
                IsIssued = x.IsIssued,
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
                IsRejected = x.IsRejected,
                RejectedAt = x.RejectedAt,
                RejectedByUserId = x.RejectedByUserId,
                RejectedByUserName = x.RejectedByUser != null ? x.RejectedByUser.DisplayName : null,
                RejectionReason = x.RejectionReason,
                IsRevoked = x.IsRevoked,
                RevokedAt = x.RevokedAt,
                RevokedByUserId = x.RevokedByUserId,
                RevokedByUserName = x.RevokedByUser != null ? x.RevokedByUser.DisplayName : null,
                RevocationReason = x.RevocationReason,
                CancelledAt = x.CancelledAt,
                CancelledByUserId = x.CancelledByUserId,
                CancelledByUserName = x.CancelledByUser != null ? x.CancelledByUser.DisplayName : null,
                CancelReason = x.CancelReason,
                Notes = x.Notes,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime
            };

            return r;
        }

        private static MedicalCertificateCreateResponse ToCreateUpdateResponse(TrxMedicalCertificate x)
        {
            return new MedicalCertificateCreateResponse
            {
                Id = x.Id,
                MedicalCertificateNumber = x.MedicalCertificateNumber,
                PatientId = x.PatientId,
                EncounterId = x.EncounterId,
                ConsultationId = x.ConsultationId,
                CertificateType = x.CertificateType,
                CertificateStatus = x.CertificateStatus,
                CertificateTitle = x.CertificateTitle,
                CertificateDateTime = x.CertificateDateTime,
                IssuedAt = x.IssuedAt,
                IsIssued = x.IsIssued,
                IsVerified = x.IsVerified,
                IsApproved = x.IsApproved
            };
        }

        private static MedicalCertificateUpdateResponse ToUpdateResponse(TrxMedicalCertificate x)
        {
            return new MedicalCertificateUpdateResponse
            {
                Id = x.Id,
                MedicalCertificateNumber = x.MedicalCertificateNumber,
                PatientId = x.PatientId,
                EncounterId = x.EncounterId,
                ConsultationId = x.ConsultationId,
                CertificateType = x.CertificateType,
                CertificateStatus = x.CertificateStatus,
                CertificateTitle = x.CertificateTitle,
                CertificateDateTime = x.CertificateDateTime,
                IssuedAt = x.IssuedAt,
                IsIssued = x.IsIssued,
                IsVerified = x.IsVerified,
                IsApproved = x.IsApproved
            };
        }

        private static void NormalizeMedicalCertificateData(TrxMedicalCertificate entity)
        {
            if (entity.IsApproved)
            {
                entity.IsRejected = false;
                entity.RejectedAt = null;
                entity.RejectedByUserId = null;
                entity.RejectionReason = null;
                entity.CertificateStatus = MedicalCertificateStatus.Approved;
            }

            if (entity.IsVerified && entity.CertificateStatus == MedicalCertificateStatus.Issued)
                entity.CertificateStatus = MedicalCertificateStatus.Verified;

            if (entity.IsIssued && entity.CertificateStatus == MedicalCertificateStatus.Draft)
                entity.CertificateStatus = MedicalCertificateStatus.Issued;

            if (entity.ExpiredDate.HasValue &&
                entity.ExpiredDate.Value.Date < DateTime.UtcNow.Date &&
                entity.CertificateStatus != MedicalCertificateStatus.Cancelled &&
                entity.CertificateStatus != MedicalCertificateStatus.EnteredInError &&
                entity.CertificateStatus != MedicalCertificateStatus.Revoked)
            {
                entity.IsActive = false;
            }

            if (entity.IsRevoked ||
                entity.CertificateStatus == MedicalCertificateStatus.Cancelled ||
                entity.CertificateStatus == MedicalCertificateStatus.EnteredInError ||
                entity.CertificateStatus == MedicalCertificateStatus.Revoked)
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

        private static List<MedicalCertificateEnumOptionResponse> BuildEnumOptions<TEnum>() where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Select(x => new MedicalCertificateEnumOptionResponse
                {
                    Value = Convert.ToInt32(x),
                    Name = x.ToString(),
                    Label = SplitPascalCase(x.ToString())
                })
                .ToList();
        }

        private static string SplitPascalCase(string value)
        {
            return string.Concat(value.Select((x, i) => i > 0 && char.IsUpper(x) ? " " + x : x.ToString()));
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static Guid? NormalizeNullableGuid(Guid? value)
        {
            if (!value.HasValue || value.Value == Guid.Empty) return null;
            return value.Value;
        }

        private Guid GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userId, out var id) ? id : Guid.Empty;
        }

        private class MedicalCertificateContextResult
        {
            public bool IsValid { get; set; }
            public string? ErrorMessage { get; set; }
            public Guid? EncounterId { get; set; }
            public Guid? QueueId { get; set; }
            public Guid? AssessmentId { get; set; }
            public Guid? ConsultationId { get; set; }
            public Guid? PatientDiagnosisId { get; set; }
            public Guid? ClinicalDocumentId { get; set; }
            public Guid? DoctorId { get; set; }
            public Guid? ServiceUnitId { get; set; }
            public Guid? ClinicId { get; set; }

            public MedicalCertificateContextResult Ok()
            {
                IsValid = true;
                return this;
            }

            public static MedicalCertificateContextResult Fail(string errorMessage)
            {
                return new MedicalCertificateContextResult { IsValid = false, ErrorMessage = errorMessage };
            }
        }

        private class DiagnosisSnapshotResult
        {
            public bool IsValid { get; set; }
            public string? ErrorMessage { get; set; }
            public Guid? DiagnosisId { get; set; }
            public string? DiagnosisCodeSnapshot { get; set; }
            public string? DiagnosisNameSnapshot { get; set; }
            public string DiagnosisMasterType { get; set; } = "Manual";
            public string? IcdVersion { get; set; }

            public static DiagnosisSnapshotResult Fail(string errorMessage)
            {
                return new DiagnosisSnapshotResult { IsValid = false, ErrorMessage = errorMessage };
            }
        }
    }
}
