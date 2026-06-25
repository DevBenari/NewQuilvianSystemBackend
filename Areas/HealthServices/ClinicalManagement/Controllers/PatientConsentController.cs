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
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponsePatientConsentPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs.PatientConsentResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/clinical-management/patient-consents")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_CLINICAL",
        moduleName: "Health Service Clinical",
        displayName: "Patient Consent",
        AreaName = "HealthServices",
        ControllerName = "PatientConsent",
        Description = "Informed consent dan persetujuan tindakan medis pasien",
        SortOrder = 10
    )]
    [Tags("Health Services / Clinical Management / Patient Consent")]
    public class PatientConsentController : ControllerBase
    {
        private const string LogCategory = "HealthServices.Clinical";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public PatientConsentController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<PatientConsentFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Consent", Description = "Melihat metadata filter consent pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientConsent", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new PatientConsentFilterMetadataResponse
            {
                DefaultFilter = new PatientConsentDefaultFilterResponse(),
                SortOptions = new List<PatientConsentSortOptionResponse>
                {
                    new() { Value = "consentDateTime", Label = "Tanggal consent" },
                    new() { Value = "consentNumber", Label = "Nomor consent" },
                    new() { Value = "consentTitle", Label = "Judul consent" },
                    new() { Value = "consentType", Label = "Tipe consent" },
                    new() { Value = "consentStatus", Label = "Status consent" },
                    new() { Value = "consentMethod", Label = "Metode consent" },
                    new() { Value = "signerName", Label = "Nama penanda tangan" },
                    new() { Value = "signedAt", Label = "Tanggal tanda tangan" },
                    new() { Value = "expiredDate", Label = "Tanggal kedaluwarsa" },
                    new() { Value = "isVerified", Label = "Terverifikasi" },
                    new() { Value = "isApproved", Label = "Disetujui" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                ConsentTypeOptions = BuildEnumOptions<PatientConsentType>(),
                ConsentStatusOptions = BuildEnumOptions<PatientConsentStatus>(),
                ConsentMethodOptions = BuildEnumOptions<PatientConsentMethod>(),
                SignerTypeOptions = BuildEnumOptions<PatientConsentSignerType>()
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "PatientConsent.GetFilterMetadata",
                "Mengambil metadata filter consent pasien.",
                result
            );

            return Ok(ApiResponse<PatientConsentFilterMetadataResponse>.Ok(
                result,
                "Metadata filter consent pasien berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponsePatientConsentPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Consent", Description = "Melihat data consent pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientConsent", "Read")]
        public async Task<IActionResult> GetConsents(
            [FromQuery] string? search,
            [FromQuery] Guid? patientId,
            [FromQuery] Guid? encounterId,
            [FromQuery] Guid? queueId,
            [FromQuery] Guid? assessmentId,
            [FromQuery] Guid? consultationId,
            [FromQuery] Guid? patientProcedureId,
            [FromQuery] Guid? clinicalDocumentId,
            [FromQuery] Guid? doctorId,
            [FromQuery] Guid? serviceUnitId,
            [FromQuery] Guid? clinicId,
            [FromQuery] PatientConsentType? consentType,
            [FromQuery] PatientConsentStatus? consentStatus,
            [FromQuery] PatientConsentMethod? consentMethod,
            [FromQuery] PatientConsentSignerType? signerType,
            [FromQuery] bool? isPatientAgreed,
            [FromQuery] bool? isEmergencyConsent,
            [FromQuery] bool? isHighRiskConsent,
            [FromQuery] bool? isVerified,
            [FromQuery] bool? isApproved,
            [FromQuery] bool? isRejected,
            [FromQuery] bool? isWithdrawn,
            [FromQuery] bool? isActive,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? sortBy = "consentDateTime",
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
                patientProcedureId,
                clinicalDocumentId,
                doctorId,
                serviceUnitId,
                clinicId,
                consentType,
                consentStatus,
                consentMethod,
                signerType,
                isPatientAgreed,
                isEmergencyConsent,
                isHighRiskConsent,
                isVerified,
                isApproved,
                isRejected,
                isWithdrawn,
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

            return Ok(ApiResponse<ResponsePatientConsentPagedResult>.Ok(
                new ResponsePatientConsentPagedResult
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = items
                },
                "Data consent pasien berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<PatientConsentOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Consent", Description = "Melihat pilihan consent pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientConsent", "Read")]
        public async Task<IActionResult> GetConsentOptions(
            [FromQuery] Guid? patientId,
            [FromQuery] Guid? encounterId,
            [FromQuery] Guid? consultationId,
            [FromQuery] Guid? patientProcedureId,
            [FromQuery] PatientConsentType? consentType,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null)
        {
            var query = _dbContext.Set<TrxPatientConsent>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (onlyActive)
            {
                query = query.Where(x =>
                    x.IsActive &&
                    x.ConsentStatus != PatientConsentStatus.Cancelled &&
                    x.ConsentStatus != PatientConsentStatus.EnteredInError &&
                    x.ConsentStatus != PatientConsentStatus.Withdrawn);
            }

            if (patientId.HasValue && patientId.Value != Guid.Empty)
                query = query.Where(x => x.PatientId == patientId.Value);

            if (encounterId.HasValue && encounterId.Value != Guid.Empty)
                query = query.Where(x => x.EncounterId == encounterId.Value);

            if (consultationId.HasValue && consultationId.Value != Guid.Empty)
                query = query.Where(x => x.ConsultationId == consultationId.Value);

            if (patientProcedureId.HasValue && patientProcedureId.Value != Guid.Empty)
                query = query.Where(x => x.PatientProcedureId == patientProcedureId.Value);

            if (consentType.HasValue)
                query = query.Where(x => x.ConsentType == consentType.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.ConsentNumber.ToLower().Contains(keyword) ||
                    x.ConsentTitle.ToLower().Contains(keyword) ||
                    x.SignerName.ToLower().Contains(keyword) ||
                    (x.ProcedureNameSnapshot != null && x.ProcedureNameSnapshot.ToLower().Contains(keyword)));
            }

            var data = await query
                .OrderByDescending(x => x.ConsentDateTime)
                .Take(100)
                .Select(x => new PatientConsentOptionResponse
                {
                    Id = x.Id,
                    ConsentNumber = x.ConsentNumber,
                    PatientId = x.PatientId,
                    EncounterId = x.EncounterId,
                    ConsultationId = x.ConsultationId,
                    PatientProcedureId = x.PatientProcedureId,
                    ConsentType = x.ConsentType,
                    ConsentStatus = x.ConsentStatus,
                    ConsentMethod = x.ConsentMethod,
                    ConsentTitle = x.ConsentTitle,
                    SignerName = x.SignerName,
                    ConsentDateTime = x.ConsentDateTime,
                    SignedAt = x.SignedAt,
                    IsPatientAgreed = x.IsPatientAgreed,
                    IsVerified = x.IsVerified,
                    IsApproved = x.IsApproved
                })
                .ToListAsync();

            return Ok(ApiResponse<List<PatientConsentOptionResponse>>.Ok(
                data,
                "Data pilihan consent pasien berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientConsentDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Patient Consent", Description = "Melihat detail consent pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientConsent", "Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await BuildBaseQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Consent pasien tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<PatientConsentDetailResponse>.Ok(
                ToDetailResponse(entity),
                "Detail consent pasien berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<PatientConsentCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Patient Consent", Description = "Membuat data consent pasien", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("PatientConsent", "Create")]
        public async Task<IActionResult> CreateConsent([FromBody] CreatePatientConsentRequest request)
        {
            var validation = await ValidateCreateRequestAsync(request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data consent pasien tidak valid."
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
                request.PatientProcedureId,
                request.ClinicalDocumentId,
                request.DoctorId,
                request.ServiceUnitId,
                request.ClinicId
            );

            if (!context.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    context.ErrorMessage ?? "Konteks consent tidak valid."
                ));
            }

            var procedureSnapshot = await BuildProcedureSnapshotAsync(
                context.PatientProcedureId,
                request.ProcedureCodeSnapshot,
                request.ProcedureNameSnapshot,
                request.ProcedureTypeSnapshot,
                request.PlannedProcedureDateTime
            );

            var entity = new TrxPatientConsent
            {
                Id = Guid.NewGuid(),
                ConsentNumber = await GenerateConsentNumberAsync(now),
                PatientId = request.PatientId,
                EncounterId = context.EncounterId,
                QueueId = context.QueueId,
                AssessmentId = context.AssessmentId,
                ConsultationId = context.ConsultationId,
                PatientProcedureId = context.PatientProcedureId,
                ClinicalDocumentId = context.ClinicalDocumentId,
                DoctorId = context.DoctorId,
                ServiceUnitId = context.ServiceUnitId,
                ClinicId = context.ClinicId,

                ConsentType = request.ConsentType,
                ConsentStatus = request.ConsentStatus,
                ConsentMethod = request.ConsentMethod,
                ConsentTitle = request.ConsentTitle.Trim(),
                ConsentCode = NormalizeNullableText(request.ConsentCode),
                ConsentCategoryName = NormalizeNullableText(request.ConsentCategoryName),
                ConsentDescription = NormalizeNullableText(request.ConsentDescription),

                ProcedureCodeSnapshot = procedureSnapshot.ProcedureCodeSnapshot,
                ProcedureNameSnapshot = procedureSnapshot.ProcedureNameSnapshot,
                ProcedureTypeSnapshot = procedureSnapshot.ProcedureTypeSnapshot,
                PlannedProcedureDateTime = procedureSnapshot.PlannedProcedureDateTime,
                ProcedureLocation = NormalizeNullableText(request.ProcedureLocation),

                DiagnosisExplanation = NormalizeNullableText(request.DiagnosisExplanation),
                ProcedureExplanation = NormalizeNullableText(request.ProcedureExplanation),
                BenefitExplanation = NormalizeNullableText(request.BenefitExplanation),
                RiskExplanation = NormalizeNullableText(request.RiskExplanation),
                AlternativeExplanation = NormalizeNullableText(request.AlternativeExplanation),
                ConsequenceExplanation = NormalizeNullableText(request.ConsequenceExplanation),
                PatientQuestionNote = NormalizeNullableText(request.PatientQuestionNote),
                IsDiagnosisExplained = request.IsDiagnosisExplained,
                IsProcedureExplained = request.IsProcedureExplained,
                IsRiskExplained = request.IsRiskExplained,
                IsAlternativeExplained = request.IsAlternativeExplained,
                IsPatientUnderstood = request.IsPatientUnderstood,
                IsPatientAgreed = request.IsPatientAgreed,
                IsEmergencyConsent = request.IsEmergencyConsent,
                IsHighRiskConsent = request.IsHighRiskConsent,
                IsLegalDocument = request.IsLegalDocument,
                IsPartOfMedicalRecord = request.IsPartOfMedicalRecord,

                SignerType = request.SignerType,
                SignerName = request.SignerName.Trim(),
                SignerRelationship = NormalizeNullableText(request.SignerRelationship),
                SignerIdentityType = NormalizeNullableText(request.SignerIdentityType),
                SignerIdentityNumber = NormalizeNullableText(request.SignerIdentityNumber),
                SignerPhoneNumber = NormalizeNullableText(request.SignerPhoneNumber),
                SignerAddress = NormalizeNullableText(request.SignerAddress),
                IsSignerPatient = request.IsSignerPatient,
                IsSignerLegalRepresentative = request.IsSignerLegalRepresentative,

                ExplainedByDoctorId = NormalizeNullableGuid(request.ExplainedByDoctorId),
                ExplainedByUserId = NormalizeNullableGuid(request.ExplainedByUserId),
                ExplainedAt = request.ExplainedAt,
                WitnessName = NormalizeNullableText(request.WitnessName),
                WitnessRelationship = NormalizeNullableText(request.WitnessRelationship),
                WitnessByUserId = NormalizeNullableGuid(request.WitnessByUserId),

                SignedAt = request.SignedAt,
                PatientSignaturePath = NormalizeNullableText(request.PatientSignaturePath),
                SignerSignaturePath = NormalizeNullableText(request.SignerSignaturePath),
                DoctorSignaturePath = NormalizeNullableText(request.DoctorSignaturePath),
                WitnessSignaturePath = NormalizeNullableText(request.WitnessSignaturePath),
                ConsentFilePath = NormalizeNullableText(request.ConsentFilePath),
                ConsentFileName = NormalizeNullableText(request.ConsentFileName),
                ConsentMimeType = NormalizeNullableText(request.ConsentMimeType),
                ConsentFileSizeBytes = request.ConsentFileSizeBytes,
                ConsentFileHash = NormalizeNullableText(request.ConsentFileHash),

                ConsentDateTime = request.ConsentDateTime ?? now,
                EffectiveStartDate = request.EffectiveStartDate,
                EffectiveEndDate = request.EffectiveEndDate,
                ExpiredDate = request.ExpiredDate,

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

            NormalizeConsentData(entity);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            _dbContext.Set<TrxPatientConsent>().Add(entity);
            await _dbContext.SaveChangesAsync();

            if (entity.ConsultationId.HasValue)
                await UpdateConsultationConsentCountAsync(entity.ConsultationId.Value, actorUserId, now);

            await transaction.CommitAsync();

            var response = ToCreateUpdateResponse(entity);

            await _loggerService.InfoAsync(
                LogCategory,
                "PatientConsent.CreateConsent",
                "Membuat consent pasien.",
                response
            );

            return Ok(ApiResponse<PatientConsentCreateResponse>.Ok(
                response,
                "Consent pasien berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientConsentUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Patient Consent", Description = "Mengubah data consent pasien", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("PatientConsent", "Update")]
        public async Task<IActionResult> UpdateConsent(Guid id, [FromBody] UpdatePatientConsentRequest request)
        {
            var entity = await _dbContext.Set<TrxPatientConsent>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Consent pasien tidak ditemukan."
                ));
            }

            if (entity.ConsentStatus == PatientConsentStatus.Cancelled ||
                entity.ConsentStatus == PatientConsentStatus.EnteredInError ||
                entity.IsWithdrawn)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Consent yang sudah cancelled, entered in error, atau withdrawn tidak dapat diubah."
                ));
            }

            var validation = ValidateUpdateRequest(request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data consent pasien tidak valid."
                ));
            }

            var procedureSnapshot = await BuildProcedureSnapshotAsync(
                entity.PatientProcedureId,
                request.ProcedureCodeSnapshot,
                request.ProcedureNameSnapshot,
                request.ProcedureTypeSnapshot,
                request.PlannedProcedureDateTime
            );

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.ConsentType = request.ConsentType;
            entity.ConsentStatus = request.ConsentStatus;
            entity.ConsentMethod = request.ConsentMethod;
            entity.ConsentTitle = request.ConsentTitle.Trim();
            entity.ConsentCode = NormalizeNullableText(request.ConsentCode);
            entity.ConsentCategoryName = NormalizeNullableText(request.ConsentCategoryName);
            entity.ConsentDescription = NormalizeNullableText(request.ConsentDescription);

            entity.ProcedureCodeSnapshot = procedureSnapshot.ProcedureCodeSnapshot;
            entity.ProcedureNameSnapshot = procedureSnapshot.ProcedureNameSnapshot;
            entity.ProcedureTypeSnapshot = procedureSnapshot.ProcedureTypeSnapshot;
            entity.PlannedProcedureDateTime = procedureSnapshot.PlannedProcedureDateTime;
            entity.ProcedureLocation = NormalizeNullableText(request.ProcedureLocation);

            entity.DiagnosisExplanation = NormalizeNullableText(request.DiagnosisExplanation);
            entity.ProcedureExplanation = NormalizeNullableText(request.ProcedureExplanation);
            entity.BenefitExplanation = NormalizeNullableText(request.BenefitExplanation);
            entity.RiskExplanation = NormalizeNullableText(request.RiskExplanation);
            entity.AlternativeExplanation = NormalizeNullableText(request.AlternativeExplanation);
            entity.ConsequenceExplanation = NormalizeNullableText(request.ConsequenceExplanation);
            entity.PatientQuestionNote = NormalizeNullableText(request.PatientQuestionNote);

            entity.IsDiagnosisExplained = request.IsDiagnosisExplained;
            entity.IsProcedureExplained = request.IsProcedureExplained;
            entity.IsRiskExplained = request.IsRiskExplained;
            entity.IsAlternativeExplained = request.IsAlternativeExplained;
            entity.IsPatientUnderstood = request.IsPatientUnderstood;
            entity.IsPatientAgreed = request.IsPatientAgreed;
            entity.IsEmergencyConsent = request.IsEmergencyConsent;
            entity.IsHighRiskConsent = request.IsHighRiskConsent;
            entity.IsLegalDocument = request.IsLegalDocument;
            entity.IsPartOfMedicalRecord = request.IsPartOfMedicalRecord;

            entity.SignerType = request.SignerType;
            entity.SignerName = request.SignerName.Trim();
            entity.SignerRelationship = NormalizeNullableText(request.SignerRelationship);
            entity.SignerIdentityType = NormalizeNullableText(request.SignerIdentityType);
            entity.SignerIdentityNumber = NormalizeNullableText(request.SignerIdentityNumber);
            entity.SignerPhoneNumber = NormalizeNullableText(request.SignerPhoneNumber);
            entity.SignerAddress = NormalizeNullableText(request.SignerAddress);
            entity.IsSignerPatient = request.IsSignerPatient;
            entity.IsSignerLegalRepresentative = request.IsSignerLegalRepresentative;

            entity.ExplainedByDoctorId = NormalizeNullableGuid(request.ExplainedByDoctorId);
            entity.ExplainedByUserId = NormalizeNullableGuid(request.ExplainedByUserId);
            entity.ExplainedAt = request.ExplainedAt;
            entity.WitnessName = NormalizeNullableText(request.WitnessName);
            entity.WitnessRelationship = NormalizeNullableText(request.WitnessRelationship);
            entity.WitnessByUserId = NormalizeNullableGuid(request.WitnessByUserId);

            entity.SignedAt = request.SignedAt;
            entity.PatientSignaturePath = NormalizeNullableText(request.PatientSignaturePath);
            entity.SignerSignaturePath = NormalizeNullableText(request.SignerSignaturePath);
            entity.DoctorSignaturePath = NormalizeNullableText(request.DoctorSignaturePath);
            entity.WitnessSignaturePath = NormalizeNullableText(request.WitnessSignaturePath);
            entity.ConsentFilePath = NormalizeNullableText(request.ConsentFilePath);
            entity.ConsentFileName = NormalizeNullableText(request.ConsentFileName);
            entity.ConsentMimeType = NormalizeNullableText(request.ConsentMimeType);
            entity.ConsentFileSizeBytes = request.ConsentFileSizeBytes;
            entity.ConsentFileHash = NormalizeNullableText(request.ConsentFileHash);
            entity.EffectiveStartDate = request.EffectiveStartDate;
            entity.EffectiveEndDate = request.EffectiveEndDate;
            entity.ExpiredDate = request.ExpiredDate;
            entity.Notes = NormalizeNullableText(request.Notes);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            NormalizeConsentData(entity);

            await _dbContext.SaveChangesAsync();

            var response = ToUpdateResponse(entity);

            return Ok(ApiResponse<PatientConsentUpdateResponse>.Ok(
                response,
                "Consent pasien berhasil diubah."
            ));
        }

        [HttpPatch("{id:guid}/sign")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Sign Patient Consent", Description = "Menandatangani consent pasien", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("PatientConsent", "Update")]
        public async Task<IActionResult> SignConsent(Guid id, [FromBody] SignPatientConsentRequest request)
        {
            var entity = await _dbContext.Set<TrxPatientConsent>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Consent pasien tidak ditemukan."));

            if (entity.ConsentStatus == PatientConsentStatus.Cancelled ||
                entity.ConsentStatus == PatientConsentStatus.EnteredInError ||
                entity.IsWithdrawn)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Consent tidak dapat ditandatangani."));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsPatientAgreed = request.IsPatientAgreed;
            entity.SignedAt = now;
            entity.ConsentStatus = request.IsPatientAgreed
                ? PatientConsentStatus.Signed
                : PatientConsentStatus.PendingSignature;

            entity.PatientSignaturePath = NormalizeNullableText(request.PatientSignaturePath) ?? entity.PatientSignaturePath;
            entity.SignerSignaturePath = NormalizeNullableText(request.SignerSignaturePath) ?? entity.SignerSignaturePath;
            entity.DoctorSignaturePath = NormalizeNullableText(request.DoctorSignaturePath) ?? entity.DoctorSignaturePath;
            entity.WitnessSignaturePath = NormalizeNullableText(request.WitnessSignaturePath) ?? entity.WitnessSignaturePath;
            entity.ConsentFilePath = NormalizeNullableText(request.ConsentFilePath) ?? entity.ConsentFilePath;
            entity.ConsentFileName = NormalizeNullableText(request.ConsentFileName) ?? entity.ConsentFileName;
            entity.ConsentMimeType = NormalizeNullableText(request.ConsentMimeType) ?? entity.ConsentMimeType;
            entity.ConsentFileSizeBytes = request.ConsentFileSizeBytes ?? entity.ConsentFileSizeBytes;
            entity.ConsentFileHash = NormalizeNullableText(request.ConsentFileHash) ?? entity.ConsentFileHash;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(null, "Consent pasien berhasil ditandatangani."));
        }

        [HttpPatch("{id:guid}/verify")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Verify Patient Consent", Description = "Verifikasi consent pasien", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("PatientConsent", "Update")]
        public async Task<IActionResult> VerifyConsent(Guid id, [FromBody] VerifyPatientConsentRequest request)
        {
            var entity = await _dbContext.Set<TrxPatientConsent>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Consent pasien tidak ditemukan."));

            if (entity.ConsentStatus == PatientConsentStatus.Cancelled ||
                entity.ConsentStatus == PatientConsentStatus.EnteredInError ||
                entity.IsWithdrawn)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Consent tidak dapat diverifikasi."));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsVerified = true;
            entity.VerifiedAt = now;
            entity.VerifiedByUserId = actorUserId;
            entity.VerificationNote = NormalizeNullableText(request.VerificationNote);
            entity.ConsentStatus = PatientConsentStatus.Verified;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(null, "Consent pasien berhasil diverifikasi."));
        }

        [HttpPatch("{id:guid}/approve")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Approve Patient Consent", Description = "Menyetujui consent pasien", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("PatientConsent", "Update")]
        public async Task<IActionResult> ApproveConsent(Guid id, [FromBody] ApprovePatientConsentRequest request)
        {
            var entity = await _dbContext.Set<TrxPatientConsent>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Consent pasien tidak ditemukan."));

            if (entity.ConsentStatus == PatientConsentStatus.Cancelled ||
                entity.ConsentStatus == PatientConsentStatus.EnteredInError ||
                entity.IsWithdrawn)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Consent tidak dapat disetujui."));
            }

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
            entity.ConsentStatus = PatientConsentStatus.Approved;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(null, "Consent pasien berhasil disetujui."));
        }

        [HttpPatch("{id:guid}/reject")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Reject Patient Consent", Description = "Menolak consent pasien", AccessType = AccessTypes.Update, SortOrder = 7)]
        [AccessPermission("PatientConsent", "Update")]
        public async Task<IActionResult> RejectConsent(Guid id, [FromBody] RejectPatientConsentRequest request)
        {
            var entity = await _dbContext.Set<TrxPatientConsent>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Consent pasien tidak ditemukan."));

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
            entity.ConsentStatus = PatientConsentStatus.Rejected;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(null, "Consent pasien berhasil ditolak."));
        }

        [HttpPatch("{id:guid}/withdraw")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Withdraw Patient Consent", Description = "Menarik consent pasien", AccessType = AccessTypes.Update, SortOrder = 8)]
        [AccessPermission("PatientConsent", "Update")]
        public async Task<IActionResult> WithdrawConsent(Guid id, [FromBody] WithdrawPatientConsentRequest request)
        {
            var entity = await _dbContext.Set<TrxPatientConsent>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Consent pasien tidak ditemukan."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsWithdrawn = true;
            entity.WithdrawnAt = now;
            entity.WithdrawnByUserId = actorUserId;
            entity.WithdrawalReason = request.WithdrawalReason.Trim();
            entity.ConsentStatus = PatientConsentStatus.Withdrawn;
            entity.IsActive = false;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(null, "Consent pasien berhasil ditarik."));
        }

        [HttpPatch("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Cancel Patient Consent", Description = "Membatalkan consent pasien", AccessType = AccessTypes.Update, SortOrder = 9)]
        [AccessPermission("PatientConsent", "Update")]
        public async Task<IActionResult> CancelConsent(Guid id, [FromBody] CancelPatientConsentRequest request)
        {
            var entity = await _dbContext.Set<TrxPatientConsent>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Consent pasien tidak ditemukan."));

            if (entity.ConsentStatus == PatientConsentStatus.Cancelled)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Consent pasien sudah cancelled."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            entity.ConsentStatus = PatientConsentStatus.Cancelled;
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
                await UpdateConsultationConsentCountAsync(entity.ConsultationId.Value, actorUserId, now);

            await transaction.CommitAsync();

            return Ok(ApiResponse<object>.Ok(null, "Consent pasien berhasil dibatalkan."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Patient Consent", Description = "Menghapus consent pasien", AccessType = AccessTypes.Delete, SortOrder = 10)]
        [AccessPermission("PatientConsent", "Delete")]
        public async Task<IActionResult> DeleteConsent(Guid id)
        {
            var entity = await _dbContext.Set<TrxPatientConsent>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Consent pasien tidak ditemukan."));

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
                await UpdateConsultationConsentCountAsync(entity.ConsultationId.Value, actorUserId, now);

            await transaction.CommitAsync();

            return Ok(ApiResponse<object>.Ok(null, "Consent pasien berhasil dihapus."));
        }

        private IQueryable<TrxPatientConsent> BuildBaseQuery()
        {
            return _dbContext.Set<TrxPatientConsent>()
                .Include(x => x.Patient)
                .Include(x => x.Encounter)
                .Include(x => x.Queue)
                .Include(x => x.Assessment)
                .Include(x => x.Consultation)
                .Include(x => x.PatientProcedure)
                .Include(x => x.ClinicalDocument)
                .Include(x => x.Doctor)
                .Include(x => x.ExplainedByDoctor)
                .Include(x => x.ServiceUnit)
                .Include(x => x.Clinic)
                .Include(x => x.ExplainedByUser)
                .Include(x => x.WitnessByUser)
                .Include(x => x.VerifiedByUser)
                .Include(x => x.ApprovedByUser)
                .Include(x => x.RejectedByUser)
                .Include(x => x.WithdrawnByUser)
                .Include(x => x.CancelledByUser)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<TrxPatientConsent> ApplyFilters(
            IQueryable<TrxPatientConsent> query,
            string? search,
            Guid? patientId,
            Guid? encounterId,
            Guid? queueId,
            Guid? assessmentId,
            Guid? consultationId,
            Guid? patientProcedureId,
            Guid? clinicalDocumentId,
            Guid? doctorId,
            Guid? serviceUnitId,
            Guid? clinicId,
            PatientConsentType? consentType,
            PatientConsentStatus? consentStatus,
            PatientConsentMethod? consentMethod,
            PatientConsentSignerType? signerType,
            bool? isPatientAgreed,
            bool? isEmergencyConsent,
            bool? isHighRiskConsent,
            bool? isVerified,
            bool? isApproved,
            bool? isRejected,
            bool? isWithdrawn,
            bool? isActive,
            DateTime? startDate,
            DateTime? endDate)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.ConsentNumber.ToLower().Contains(keyword) ||
                    x.ConsentTitle.ToLower().Contains(keyword) ||
                    x.SignerName.ToLower().Contains(keyword) ||
                    (x.ConsentCode != null && x.ConsentCode.ToLower().Contains(keyword)) ||
                    (x.ProcedureNameSnapshot != null && x.ProcedureNameSnapshot.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.FullName.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.MedicalRecordNumber.ToLower().Contains(keyword)) ||
                    (x.Encounter != null && x.Encounter.EncounterNumber.ToLower().Contains(keyword)));
            }

            if (patientId.HasValue && patientId.Value != Guid.Empty) query = query.Where(x => x.PatientId == patientId.Value);
            if (encounterId.HasValue && encounterId.Value != Guid.Empty) query = query.Where(x => x.EncounterId == encounterId.Value);
            if (queueId.HasValue && queueId.Value != Guid.Empty) query = query.Where(x => x.QueueId == queueId.Value);
            if (assessmentId.HasValue && assessmentId.Value != Guid.Empty) query = query.Where(x => x.AssessmentId == assessmentId.Value);
            if (consultationId.HasValue && consultationId.Value != Guid.Empty) query = query.Where(x => x.ConsultationId == consultationId.Value);
            if (patientProcedureId.HasValue && patientProcedureId.Value != Guid.Empty) query = query.Where(x => x.PatientProcedureId == patientProcedureId.Value);
            if (clinicalDocumentId.HasValue && clinicalDocumentId.Value != Guid.Empty) query = query.Where(x => x.ClinicalDocumentId == clinicalDocumentId.Value);
            if (doctorId.HasValue && doctorId.Value != Guid.Empty) query = query.Where(x => x.DoctorId == doctorId.Value);
            if (serviceUnitId.HasValue && serviceUnitId.Value != Guid.Empty) query = query.Where(x => x.ServiceUnitId == serviceUnitId.Value);
            if (clinicId.HasValue && clinicId.Value != Guid.Empty) query = query.Where(x => x.ClinicId == clinicId.Value);
            if (consentType.HasValue) query = query.Where(x => x.ConsentType == consentType.Value);
            if (consentStatus.HasValue) query = query.Where(x => x.ConsentStatus == consentStatus.Value);
            if (consentMethod.HasValue) query = query.Where(x => x.ConsentMethod == consentMethod.Value);
            if (signerType.HasValue) query = query.Where(x => x.SignerType == signerType.Value);
            if (isPatientAgreed.HasValue) query = query.Where(x => x.IsPatientAgreed == isPatientAgreed.Value);
            if (isEmergencyConsent.HasValue) query = query.Where(x => x.IsEmergencyConsent == isEmergencyConsent.Value);
            if (isHighRiskConsent.HasValue) query = query.Where(x => x.IsHighRiskConsent == isHighRiskConsent.Value);
            if (isVerified.HasValue) query = query.Where(x => x.IsVerified == isVerified.Value);
            if (isApproved.HasValue) query = query.Where(x => x.IsApproved == isApproved.Value);
            if (isRejected.HasValue) query = query.Where(x => x.IsRejected == isRejected.Value);
            if (isWithdrawn.HasValue) query = query.Where(x => x.IsWithdrawn == isWithdrawn.Value);
            if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);
            if (startDate.HasValue) query = query.Where(x => x.ConsentDateTime >= startDate.Value.Date);
            if (endDate.HasValue) query = query.Where(x => x.ConsentDateTime < endDate.Value.Date.AddDays(1));

            return query;
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateCreateRequestAsync(CreatePatientConsentRequest request)
        {
            if (request.PatientId == Guid.Empty)
                return (false, "PatientId wajib diisi.");

            if (string.IsNullOrWhiteSpace(request.ConsentTitle))
                return (false, "Judul consent wajib diisi.");

            if (string.IsNullOrWhiteSpace(request.SignerName))
                return (false, "Nama penanda tangan wajib diisi.");

            var patientExists = await _dbContext.Set<MstPatient>()
                .AnyAsync(x => x.Id == request.PatientId && !x.IsDelete);

            if (!patientExists)
                return (false, "Pasien tidak ditemukan.");

            if (request.EffectiveStartDate.HasValue &&
                request.EffectiveEndDate.HasValue &&
                request.EffectiveStartDate.Value.Date > request.EffectiveEndDate.Value.Date)
            {
                return (false, "Tanggal mulai berlaku tidak boleh lebih besar dari tanggal akhir berlaku.");
            }

            if (request.ConsentFileSizeBytes.HasValue && request.ConsentFileSizeBytes.Value < 0)
                return (false, "Ukuran file consent tidak valid.");

            return (true, null);
        }

        private static (bool IsValid, string? ErrorMessage) ValidateUpdateRequest(UpdatePatientConsentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ConsentTitle))
                return (false, "Judul consent wajib diisi.");

            if (string.IsNullOrWhiteSpace(request.SignerName))
                return (false, "Nama penanda tangan wajib diisi.");

            if (request.EffectiveStartDate.HasValue &&
                request.EffectiveEndDate.HasValue &&
                request.EffectiveStartDate.Value.Date > request.EffectiveEndDate.Value.Date)
            {
                return (false, "Tanggal mulai berlaku tidak boleh lebih besar dari tanggal akhir berlaku.");
            }

            if (request.ConsentFileSizeBytes.HasValue && request.ConsentFileSizeBytes.Value < 0)
                return (false, "Ukuran file consent tidak valid.");

            return (true, null);
        }

        private async Task<ConsentContextResult> ResolveClinicalContextAsync(
            Guid patientId,
            Guid? encounterId,
            Guid? queueId,
            Guid? assessmentId,
            Guid? consultationId,
            Guid? patientProcedureId,
            Guid? clinicalDocumentId,
            Guid? doctorId,
            Guid? serviceUnitId,
            Guid? clinicId)
        {
            var result = new ConsentContextResult
            {
                EncounterId = NormalizeNullableGuid(encounterId),
                QueueId = NormalizeNullableGuid(queueId),
                AssessmentId = NormalizeNullableGuid(assessmentId),
                ConsultationId = NormalizeNullableGuid(consultationId),
                PatientProcedureId = NormalizeNullableGuid(patientProcedureId),
                ClinicalDocumentId = NormalizeNullableGuid(clinicalDocumentId),
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
                    return ConsentContextResult.Fail("Tindakan pasien tidak ditemukan.");

                if (procedure.PatientId != patientId)
                    return ConsentContextResult.Fail("Tindakan pasien tidak sesuai dengan pasien.");

                result.EncounterId = procedure.EncounterId;
                result.ConsultationId = procedure.ConsultationId;
                result.DoctorId = procedure.DoctorId;
                result.ServiceUnitId = procedure.ServiceUnitId;
                result.ClinicId = procedure.ClinicId;

                return result.Ok();
            }

            if (result.ConsultationId.HasValue)
            {
                var consultation = await _dbContext.Set<TrxDoctorConsultation>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == result.ConsultationId.Value && !x.IsDelete);

                if (consultation == null)
                    return ConsentContextResult.Fail("Konsultasi dokter tidak ditemukan.");

                if (consultation.PatientId != patientId)
                    return ConsentContextResult.Fail("Konsultasi dokter tidak sesuai dengan pasien.");

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

                if (assessment == null)
                    return ConsentContextResult.Fail("Assessment pasien tidak ditemukan.");

                if (assessment.PatientId != patientId)
                    return ConsentContextResult.Fail("Assessment pasien tidak sesuai dengan pasien.");

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

                if (queue == null)
                    return ConsentContextResult.Fail("Queue pasien tidak ditemukan.");

                if (queue.PatientId != patientId)
                    return ConsentContextResult.Fail("Queue tidak sesuai dengan pasien.");

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

                if (encounter == null)
                    return ConsentContextResult.Fail("Encounter pasien tidak ditemukan.");

                if (encounter.PatientId != patientId)
                    return ConsentContextResult.Fail("Encounter tidak sesuai dengan pasien.");
            }

            if (result.ClinicalDocumentId.HasValue)
            {
                var document = await _dbContext.Set<TrxPatientClinicalDocument>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == result.ClinicalDocumentId.Value && !x.IsDelete);

                if (document == null)
                    return ConsentContextResult.Fail("Dokumen klinis tidak ditemukan.");

                if (document.PatientId != patientId)
                    return ConsentContextResult.Fail("Dokumen klinis tidak sesuai dengan pasien.");
            }

            return result.Ok();
        }

        private async Task<ProcedureSnapshotResult> BuildProcedureSnapshotAsync(
            Guid? patientProcedureId,
            string? procedureCodeSnapshot,
            string? procedureNameSnapshot,
            string? procedureTypeSnapshot,
            DateTime? plannedProcedureDateTime)
        {
            var result = new ProcedureSnapshotResult
            {
                ProcedureCodeSnapshot = NormalizeNullableText(procedureCodeSnapshot),
                ProcedureNameSnapshot = NormalizeNullableText(procedureNameSnapshot),
                ProcedureTypeSnapshot = NormalizeNullableText(procedureTypeSnapshot),
                PlannedProcedureDateTime = plannedProcedureDateTime
            };

            if (!patientProcedureId.HasValue || patientProcedureId.Value == Guid.Empty)
                return result;

            var procedure = await _dbContext.Set<TrxPatientProcedure>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == patientProcedureId.Value && !x.IsDelete);

            if (procedure == null)
                return result;

            result.ProcedureCodeSnapshot = procedure.ProcedureCodeSnapshot;
            result.ProcedureNameSnapshot = procedure.ProcedureNameSnapshot;
            result.ProcedureTypeSnapshot = procedure.ProcedureTypeSnapshot;
            result.PlannedProcedureDateTime = procedure.ScheduledAt ?? procedure.PlannedAt ?? procedure.ProcedureDateTime;

            return result;
        }

        private async Task<string> GenerateConsentNumberAsync(DateTime now)
        {
            var prefix = $"CNS-{now:yyyyMMdd}";

            var countToday = await _dbContext.Set<TrxPatientConsent>()
                .CountAsync(x => x.ConsentNumber.StartsWith(prefix));

            return $"{prefix}-{countToday + 1:0000}";
        }

        private async Task UpdateConsultationConsentCountAsync(Guid consultationId, Guid actorUserId, DateTime now)
        {
            var consultation = await _dbContext.Set<TrxDoctorConsultation>()
                .FirstOrDefaultAsync(x => x.Id == consultationId && !x.IsDelete);

            if (consultation == null)
                return;

            var consentCount = await _dbContext.Set<TrxPatientConsent>()
                .CountAsync(x =>
                    !x.IsDelete &&
                    x.ConsultationId == consultationId &&
                    x.ConsentStatus != PatientConsentStatus.Cancelled &&
                    x.ConsentStatus != PatientConsentStatus.EnteredInError);

            consultation.ConsentCount = consentCount;
            consultation.UpdateDateTime = now;
            consultation.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();
        }

        private static IQueryable<TrxPatientConsent> ApplySorting(
            IQueryable<TrxPatientConsent> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "consentDateTime").ToLowerInvariant() switch
            {
                "consentnumber" => isDesc ? query.OrderByDescending(x => x.ConsentNumber) : query.OrderBy(x => x.ConsentNumber),
                "consenttitle" => isDesc ? query.OrderByDescending(x => x.ConsentTitle) : query.OrderBy(x => x.ConsentTitle),
                "consenttype" => isDesc ? query.OrderByDescending(x => x.ConsentType) : query.OrderBy(x => x.ConsentType),
                "consentstatus" => isDesc ? query.OrderByDescending(x => x.ConsentStatus) : query.OrderBy(x => x.ConsentStatus),
                "consentmethod" => isDesc ? query.OrderByDescending(x => x.ConsentMethod) : query.OrderBy(x => x.ConsentMethod),
                "signername" => isDesc ? query.OrderByDescending(x => x.SignerName) : query.OrderBy(x => x.SignerName),
                "signedat" => isDesc ? query.OrderByDescending(x => x.SignedAt) : query.OrderBy(x => x.SignedAt),
                "expireddate" => isDesc ? query.OrderByDescending(x => x.ExpiredDate) : query.OrderBy(x => x.ExpiredDate),
                "isverified" => isDesc ? query.OrderByDescending(x => x.IsVerified) : query.OrderBy(x => x.IsVerified),
                "isapproved" => isDesc ? query.OrderByDescending(x => x.IsApproved) : query.OrderBy(x => x.IsApproved),
                "createdatetime" => isDesc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                _ => isDesc
                    ? query.OrderByDescending(x => x.ConsentDateTime)
                    : query.OrderBy(x => x.ConsentDateTime)
            };
        }

        private static PatientConsentResponse ToResponse(TrxPatientConsent x)
        {
            return new PatientConsentResponse
            {
                Id = x.Id,
                ConsentNumber = x.ConsentNumber,
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
                PatientProcedureId = x.PatientProcedureId,
                ProcedureNameSnapshot = x.ProcedureNameSnapshot,
                ClinicalDocumentId = x.ClinicalDocumentId,
                ClinicalDocumentNumber = x.ClinicalDocument != null ? x.ClinicalDocument.ClinicalDocumentNumber : null,
                DoctorId = x.DoctorId,
                DoctorName = x.Doctor != null ? x.Doctor.FullName : null,
                ServiceUnitId = x.ServiceUnitId,
                ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null,
                ClinicId = x.ClinicId,
                ClinicName = x.Clinic != null ? x.Clinic.ClinicName : null,
                ConsentType = x.ConsentType,
                ConsentStatus = x.ConsentStatus,
                ConsentMethod = x.ConsentMethod,
                ConsentTitle = x.ConsentTitle,
                ConsentCode = x.ConsentCode,
                ConsentCategoryName = x.ConsentCategoryName,
                SignerType = x.SignerType,
                SignerName = x.SignerName,
                SignerRelationship = x.SignerRelationship,
                IsSignerPatient = x.IsSignerPatient,
                IsSignerLegalRepresentative = x.IsSignerLegalRepresentative,
                IsPatientAgreed = x.IsPatientAgreed,
                IsEmergencyConsent = x.IsEmergencyConsent,
                IsHighRiskConsent = x.IsHighRiskConsent,
                IsLegalDocument = x.IsLegalDocument,
                IsPartOfMedicalRecord = x.IsPartOfMedicalRecord,
                ConsentDateTime = x.ConsentDateTime,
                SignedAt = x.SignedAt,
                ExpiredDate = x.ExpiredDate,
                IsVerified = x.IsVerified,
                VerifiedAt = x.VerifiedAt,
                VerifiedByUserId = x.VerifiedByUserId,
                VerifiedByUserName = x.VerifiedByUser != null ? x.VerifiedByUser.DisplayName : null,
                IsApproved = x.IsApproved,
                ApprovedAt = x.ApprovedAt,
                ApprovedByUserId = x.ApprovedByUserId,
                ApprovedByUserName = x.ApprovedByUser != null ? x.ApprovedByUser.DisplayName : null,
                IsRejected = x.IsRejected,
                IsWithdrawn = x.IsWithdrawn,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime
            };
        }

        private static PatientConsentDetailResponse ToDetailResponse(TrxPatientConsent x)
        {
            var response = new PatientConsentDetailResponse
            {
                Id = x.Id,
                ConsentNumber = x.ConsentNumber,
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
                PatientProcedureId = x.PatientProcedureId,
                ProcedureNameSnapshot = x.ProcedureNameSnapshot,
                ClinicalDocumentId = x.ClinicalDocumentId,
                ClinicalDocumentNumber = x.ClinicalDocument != null ? x.ClinicalDocument.ClinicalDocumentNumber : null,
                DoctorId = x.DoctorId,
                DoctorName = x.Doctor != null ? x.Doctor.FullName : null,
                ServiceUnitId = x.ServiceUnitId,
                ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null,
                ClinicId = x.ClinicId,
                ClinicName = x.Clinic != null ? x.Clinic.ClinicName : null,
                ConsentType = x.ConsentType,
                ConsentStatus = x.ConsentStatus,
                ConsentMethod = x.ConsentMethod,
                ConsentTitle = x.ConsentTitle,
                ConsentCode = x.ConsentCode,
                ConsentCategoryName = x.ConsentCategoryName,
                ConsentDescription = x.ConsentDescription,
                ProcedureCodeSnapshot = x.ProcedureCodeSnapshot,
                ProcedureTypeSnapshot = x.ProcedureTypeSnapshot,
                PlannedProcedureDateTime = x.PlannedProcedureDateTime,
                ProcedureLocation = x.ProcedureLocation,
                DiagnosisExplanation = x.DiagnosisExplanation,
                ProcedureExplanation = x.ProcedureExplanation,
                BenefitExplanation = x.BenefitExplanation,
                RiskExplanation = x.RiskExplanation,
                AlternativeExplanation = x.AlternativeExplanation,
                ConsequenceExplanation = x.ConsequenceExplanation,
                PatientQuestionNote = x.PatientQuestionNote,
                IsDiagnosisExplained = x.IsDiagnosisExplained,
                IsProcedureExplained = x.IsProcedureExplained,
                IsRiskExplained = x.IsRiskExplained,
                IsAlternativeExplained = x.IsAlternativeExplained,
                IsPatientUnderstood = x.IsPatientUnderstood,
                IsPatientAgreed = x.IsPatientAgreed,
                IsEmergencyConsent = x.IsEmergencyConsent,
                IsHighRiskConsent = x.IsHighRiskConsent,
                IsLegalDocument = x.IsLegalDocument,
                IsPartOfMedicalRecord = x.IsPartOfMedicalRecord,
                SignerType = x.SignerType,
                SignerName = x.SignerName,
                SignerRelationship = x.SignerRelationship,
                SignerIdentityType = x.SignerIdentityType,
                SignerIdentityNumber = x.SignerIdentityNumber,
                SignerPhoneNumber = x.SignerPhoneNumber,
                SignerAddress = x.SignerAddress,
                IsSignerPatient = x.IsSignerPatient,
                IsSignerLegalRepresentative = x.IsSignerLegalRepresentative,
                ExplainedByDoctorId = x.ExplainedByDoctorId,
                ExplainedByDoctorName = x.ExplainedByDoctor != null ? x.ExplainedByDoctor.FullName : null,
                ExplainedByUserId = x.ExplainedByUserId,
                ExplainedByUserName = x.ExplainedByUser != null ? x.ExplainedByUser.DisplayName : null,
                ExplainedAt = x.ExplainedAt,
                WitnessName = x.WitnessName,
                WitnessRelationship = x.WitnessRelationship,
                WitnessByUserId = x.WitnessByUserId,
                WitnessByUserName = x.WitnessByUser != null ? x.WitnessByUser.DisplayName : null,
                SignedAt = x.SignedAt,
                PatientSignaturePath = x.PatientSignaturePath,
                SignerSignaturePath = x.SignerSignaturePath,
                DoctorSignaturePath = x.DoctorSignaturePath,
                WitnessSignaturePath = x.WitnessSignaturePath,
                ConsentFilePath = x.ConsentFilePath,
                ConsentFileName = x.ConsentFileName,
                ConsentMimeType = x.ConsentMimeType,
                ConsentFileSizeBytes = x.ConsentFileSizeBytes,
                ConsentFileHash = x.ConsentFileHash,
                ConsentDateTime = x.ConsentDateTime,
                EffectiveStartDate = x.EffectiveStartDate,
                EffectiveEndDate = x.EffectiveEndDate,
                ExpiredDate = x.ExpiredDate,
                IsVerified = x.IsVerified,
                VerifiedAt = x.VerifiedAt,
                VerifiedByUserId = x.VerifiedByUserId,
                VerifiedByUserName = x.VerifiedByUser != null ? x.VerifiedByUser.DisplayName : null,
                IsApproved = x.IsApproved,
                ApprovedAt = x.ApprovedAt,
                ApprovedByUserId = x.ApprovedByUserId,
                ApprovedByUserName = x.ApprovedByUser != null ? x.ApprovedByUser.DisplayName : null,
                VerificationNote = x.VerificationNote,
                ApprovalNote = x.ApprovalNote,
                IsRejected = x.IsRejected,
                RejectedAt = x.RejectedAt,
                RejectedByUserId = x.RejectedByUserId,
                RejectedByUserName = x.RejectedByUser != null ? x.RejectedByUser.DisplayName : null,
                RejectionReason = x.RejectionReason,
                IsWithdrawn = x.IsWithdrawn,
                WithdrawnAt = x.WithdrawnAt,
                WithdrawnByUserId = x.WithdrawnByUserId,
                WithdrawnByUserName = x.WithdrawnByUser != null ? x.WithdrawnByUser.DisplayName : null,
                WithdrawalReason = x.WithdrawalReason,
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

        private static PatientConsentCreateResponse ToCreateUpdateResponse(TrxPatientConsent x)
        {
            return new PatientConsentCreateResponse
            {
                Id = x.Id,
                ConsentNumber = x.ConsentNumber,
                PatientId = x.PatientId,
                EncounterId = x.EncounterId,
                ConsultationId = x.ConsultationId,
                PatientProcedureId = x.PatientProcedureId,
                ClinicalDocumentId = x.ClinicalDocumentId,
                ConsentType = x.ConsentType,
                ConsentStatus = x.ConsentStatus,
                ConsentMethod = x.ConsentMethod,
                ConsentTitle = x.ConsentTitle,
                SignerName = x.SignerName,
                IsPatientAgreed = x.IsPatientAgreed,
                ConsentDateTime = x.ConsentDateTime,
                SignedAt = x.SignedAt,
                IsVerified = x.IsVerified,
                IsApproved = x.IsApproved
            };
        }

        private static PatientConsentUpdateResponse ToUpdateResponse(TrxPatientConsent x)
        {
            return new PatientConsentUpdateResponse
            {
                Id = x.Id,
                ConsentNumber = x.ConsentNumber,
                PatientId = x.PatientId,
                EncounterId = x.EncounterId,
                ConsultationId = x.ConsultationId,
                PatientProcedureId = x.PatientProcedureId,
                ClinicalDocumentId = x.ClinicalDocumentId,
                ConsentType = x.ConsentType,
                ConsentStatus = x.ConsentStatus,
                ConsentMethod = x.ConsentMethod,
                ConsentTitle = x.ConsentTitle,
                SignerName = x.SignerName,
                IsPatientAgreed = x.IsPatientAgreed,
                ConsentDateTime = x.ConsentDateTime,
                SignedAt = x.SignedAt,
                IsVerified = x.IsVerified,
                IsApproved = x.IsApproved
            };
        }

        private static void NormalizeConsentData(TrxPatientConsent entity)
        {
            if (entity.IsApproved)
            {
                entity.IsRejected = false;
                entity.RejectedAt = null;
                entity.RejectedByUserId = null;
                entity.RejectionReason = null;
                entity.ConsentStatus = PatientConsentStatus.Approved;
            }

            if (entity.IsVerified && entity.ConsentStatus == PatientConsentStatus.Signed)
                entity.ConsentStatus = PatientConsentStatus.Verified;

            if (entity.SignedAt.HasValue && entity.IsPatientAgreed && entity.ConsentStatus == PatientConsentStatus.Draft)
                entity.ConsentStatus = PatientConsentStatus.Signed;

            if (entity.ExpiredDate.HasValue &&
                entity.ExpiredDate.Value.Date < AppDateTimeHelper.OperationalDate() &&
                entity.ConsentStatus != PatientConsentStatus.Cancelled &&
                entity.ConsentStatus != PatientConsentStatus.EnteredInError &&
                entity.ConsentStatus != PatientConsentStatus.Withdrawn)
            {
                entity.ConsentStatus = PatientConsentStatus.Expired;
                entity.IsActive = false;
            }

            if (entity.IsWithdrawn ||
                entity.ConsentStatus == PatientConsentStatus.Cancelled ||
                entity.ConsentStatus == PatientConsentStatus.EnteredInError ||
                entity.ConsentStatus == PatientConsentStatus.Expired)
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

        private static List<PatientConsentEnumOptionResponse> BuildEnumOptions<TEnum>() where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Select(x => new PatientConsentEnumOptionResponse
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

        private class ConsentContextResult
        {
            public bool IsValid { get; set; }
            public string? ErrorMessage { get; set; }
            public Guid? EncounterId { get; set; }
            public Guid? QueueId { get; set; }
            public Guid? AssessmentId { get; set; }
            public Guid? ConsultationId { get; set; }
            public Guid? PatientProcedureId { get; set; }
            public Guid? ClinicalDocumentId { get; set; }
            public Guid? DoctorId { get; set; }
            public Guid? ServiceUnitId { get; set; }
            public Guid? ClinicId { get; set; }

            public ConsentContextResult Ok()
            {
                IsValid = true;
                return this;
            }

            public static ConsentContextResult Fail(string errorMessage)
            {
                return new ConsentContextResult
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
        }

        private class ProcedureSnapshotResult
        {
            public string? ProcedureCodeSnapshot { get; set; }
            public string? ProcedureNameSnapshot { get; set; }
            public string? ProcedureTypeSnapshot { get; set; }
            public DateTime? PlannedProcedureDateTime { get; set; }
        }
    }
}
