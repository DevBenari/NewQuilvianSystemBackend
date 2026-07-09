using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;
using System.Text;

using ResponsePatientIntegratedProgressNotePagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs.PatientIntegratedProgressNoteResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/clinical-management/patient-integrated-progress-notes")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_CLINICAL",
        moduleName: "Health Service Clinical",
        displayName: "Patient Integrated Progress Note",
        AreaName = "HealthServices",
        ControllerName = "PatientIntegratedProgressNote",
        Description = "Catatan Perkembangan Pasien Terintegrasi (CPPT)",
        SortOrder = 12
    )]
    [Tags("Health Services / Clinical Management / Patient Integrated Progress Note")]
    public class PatientIntegratedProgressNoteController : ControllerBase
    {
        private const string LogCategory = "HealthServices.Clinical";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public PatientIntegratedProgressNoteController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<PatientIntegratedProgressNoteFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Integrated Progress Note", Description = "Melihat metadata filter CPPT", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientIntegratedProgressNote", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new PatientIntegratedProgressNoteFilterMetadataResponse
            {
                DefaultFilter = new PatientIntegratedProgressNoteDefaultFilterResponse(),
                SortOptions = new List<PatientIntegratedProgressNoteSortOptionResponse>
                {
                    new() { Value = "noteDateTime", Label = "Tanggal dan jam catatan" },
                    new() { Value = "progressNoteNumber", Label = "Nomor CPPT" },
                    new() { Value = "professionType", Label = "Profesi" },
                    new() { Value = "sourceModule", Label = "Sumber modul" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                ProfessionOptions = BuildProfessionOptions(),
                SourceModuleOptions = BuildSourceModuleOptions()
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "PatientIntegratedProgressNote.GetFilterMetadata",
                "Mengambil metadata filter CPPT.",
                result
            );

            return Ok(ApiResponse<PatientIntegratedProgressNoteFilterMetadataResponse>.Ok(
                result,
                "Metadata filter CPPT berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponsePatientIntegratedProgressNotePagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Integrated Progress Note", Description = "Melihat data CPPT", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientIntegratedProgressNote", "Read")]
        public async Task<IActionResult> GetProgressNotes(
            [FromQuery] string? search,
            [FromQuery] Guid? patientId,
            [FromQuery] Guid? encounterId,
            [FromQuery] Guid? queueId,
            [FromQuery] Guid? consultationId,
            [FromQuery] Guid? assessmentId,
            [FromQuery] Guid? vitalSignId,
            [FromQuery] Guid? doctorId,
            [FromQuery] Guid? serviceUnitId,
            [FromQuery] Guid? clinicId,
            [FromQuery] Guid? providerUserId,
            [FromQuery] string? professionType,
            [FromQuery] string? sourceModule,
            [FromQuery] bool? isGeneratedFromSource,
            [FromQuery] bool? isActive,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? sortBy = "noteDateTime",
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
                consultationId,
                assessmentId,
                vitalSignId,
                doctorId,
                serviceUnitId,
                clinicId,
                providerUserId,
                professionType,
                sourceModule,
                isGeneratedFromSource,
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

            var result = new ResponsePatientIntegratedProgressNotePagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponsePatientIntegratedProgressNotePagedResult>.Ok(
                result,
                "Data CPPT berhasil diambil."
            ));
        }

        [HttpGet("timeline")]
        [ProducesResponseType(typeof(ApiResponse<List<PatientIntegratedProgressNoteTimelineResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Integrated Progress Note", Description = "Melihat timeline CPPT pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientIntegratedProgressNote", "Read")]
        public async Task<IActionResult> GetTimeline(
            [FromQuery] Guid? patientId,
            [FromQuery] Guid? encounterId,
            [FromQuery] string? professionType,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] bool includeCancelled = false,
            [FromQuery] int limit = 100)
        {
            if ((!patientId.HasValue || patientId.Value == Guid.Empty) &&
                (!encounterId.HasValue || encounterId.Value == Guid.Empty))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "PatientId atau EncounterId wajib diisi untuk timeline CPPT."
                ));
            }

            limit = limit <= 0 ? 100 : Math.Min(limit, 300);

            var query = BuildBaseQuery().AsNoTracking();

            if (!includeCancelled)
            {
                query = query.Where(x => !x.IsCancel && x.IsActive);
            }

            query = ApplyFilters(
                query,
                null,
                patientId,
                encounterId,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                professionType,
                null,
                null,
                null,
                startDate,
                endDate
            );

            var data = await query
                .OrderBy(x => x.NoteDateTime)
                .ThenBy(x => x.CreateDateTime)
                .Take(limit)
                .ToListAsync();

            var result = data.Select(ToTimelineResponse).ToList();

            return Ok(ApiResponse<List<PatientIntegratedProgressNoteTimelineResponse>>.Ok(
                result,
                "Timeline CPPT berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientIntegratedProgressNoteDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Patient Integrated Progress Note", Description = "Melihat detail CPPT", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientIntegratedProgressNote", "Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await BuildBaseQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "CPPT tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<PatientIntegratedProgressNoteDetailResponse>.Ok(
                ToDetailResponse(entity),
                "Detail CPPT berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<PatientIntegratedProgressNoteCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Patient Integrated Progress Note", Description = "Membuat CPPT", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("PatientIntegratedProgressNote", "Create")]
        public async Task<IActionResult> CreateProgressNote([FromBody] CreatePatientIntegratedProgressNoteRequest request)
        {
            var validation = await ValidateCreateRequestAsync(request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data CPPT tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var context = await ResolveClinicalContextAsync(
                request.PatientId,
                request.EncounterId,
                request.QueueId,
                request.ConsultationId,
                request.AssessmentId,
                request.VitalSignId,
                request.DoctorId,
                request.ServiceUnitId,
                request.ClinicId
            );

            if (!context.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    context.ErrorMessage ?? "Konteks klinis CPPT tidak valid."
                ));
            }

            var entity = new TrxPatientIntegratedProgressNote
            {
                Id = Guid.NewGuid(),
                ProgressNoteNumber = await GenerateProgressNoteNumberAsync(now),
                PatientId = request.PatientId,
                EncounterId = context.EncounterId,
                QueueId = context.QueueId,
                ConsultationId = context.ConsultationId,
                AssessmentId = context.AssessmentId,
                VitalSignId = context.VitalSignId,
                DoctorId = context.DoctorId,
                ServiceUnitId = context.ServiceUnitId,
                ClinicId = context.ClinicId,
                NoteDateTime = request.NoteDateTime ?? now,
                ProfessionType = NormalizeProfessionType(request.ProfessionType),
                ProfessionName = NormalizeNullableText(request.ProfessionName) ?? GetDefaultProfessionName(request.ProfessionType),
                ProviderUserId = request.ProviderUserId ?? actorUserId,
                ProviderDisplayNameSnapshot = NormalizeNullableText(request.ProviderDisplayNameSnapshot),
                ProviderRoleSnapshot = NormalizeNullableText(request.ProviderRoleSnapshot),
                ServiceUnitNameSnapshot = NormalizeNullableText(request.ServiceUnitNameSnapshot) ?? context.ServiceUnitNameSnapshot,
                LocationSnapshot = NormalizeNullableText(request.LocationSnapshot) ?? context.LocationSnapshot,
                SourceModule = NormalizeNullableText(request.SourceModule) ?? context.SourceModule,
                SourceReferenceId = request.SourceReferenceId ?? context.SourceReferenceId,
                SourceReferenceNumber = NormalizeNullableText(request.SourceReferenceNumber) ?? context.SourceReferenceNumber,
                SubjectiveSummary = NormalizeNullableText(request.SubjectiveSummary),
                ObjectiveSummary = NormalizeNullableText(request.ObjectiveSummary),
                AssessmentSummary = NormalizeNullableText(request.AssessmentSummary),
                PlanSummary = NormalizeNullableText(request.PlanSummary),
                Instruction = NormalizeNullableText(request.Instruction),
                Evaluation = NormalizeNullableText(request.Evaluation),
                PrivateNote = NormalizeNullableText(request.PrivateNote),
                IsGeneratedFromSource = request.IsGeneratedFromSource,
                IsReadOnlyGenerated = request.IsReadOnlyGenerated,
                IsActive = request.IsActive,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            entity.NoteText = NormalizeNullableText(request.NoteText) ?? BuildNoteText(entity);

            _dbContext.Set<TrxPatientIntegratedProgressNote>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var response = ToCreateUpdateResponse(entity);

            await _loggerService.InfoAsync(
                LogCategory,
                "PatientIntegratedProgressNote.CreateProgressNote",
                "Membuat CPPT pasien.",
                response
            );

            return Ok(ApiResponse<PatientIntegratedProgressNoteCreateResponse>.Ok(
                response,
                "CPPT berhasil dibuat."
            ));
        }

        [HttpPost("from-consultation/{consultationId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientIntegratedProgressNoteCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Create", "Create Patient Integrated Progress Note", Description = "Membuat CPPT dari SOAP dokter", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("PatientIntegratedProgressNote", "Create")]
        public async Task<IActionResult> CreateFromConsultation(Guid consultationId, [FromBody] CreatePatientIntegratedProgressNoteFromConsultationRequest request)
        {
            var consultation = await _dbContext.Set<TrxDoctorConsultation>()
                .Include(x => x.Patient)
                .Include(x => x.Queue)
                .Include(x => x.Doctor)
                .Include(x => x.ServiceUnit)
                .Include(x => x.Clinic)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == consultationId && !x.IsDelete);

            if (consultation == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Konsultasi dokter tidak ditemukan."
                ));
            }

            var existingGeneratedNote = await _dbContext.Set<TrxPatientIntegratedProgressNote>()
                .AnyAsync(x =>
                    x.SourceModule == "DoctorConsultation" &&
                    x.SourceReferenceId == consultationId &&
                    !x.IsDelete &&
                    !x.IsCancel);

            if (existingGeneratedNote)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "CPPT dari konsultasi dokter ini sudah pernah dibuat."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var draft = BuildRequestFromConsultation(consultation, request, actorUserId, now);

            var entity = new TrxPatientIntegratedProgressNote
            {
                Id = Guid.NewGuid(),
                ProgressNoteNumber = await GenerateProgressNoteNumberAsync(now),
                PatientId = draft.PatientId,
                EncounterId = draft.EncounterId,
                QueueId = draft.QueueId,
                ConsultationId = draft.ConsultationId,
                AssessmentId = draft.AssessmentId,
                DoctorId = draft.DoctorId,
                ServiceUnitId = draft.ServiceUnitId,
                ClinicId = draft.ClinicId,
                NoteDateTime = draft.NoteDateTime ?? now,
                ProfessionType = draft.ProfessionType,
                ProfessionName = draft.ProfessionName,
                ProviderUserId = draft.ProviderUserId,
                ProviderDisplayNameSnapshot = draft.ProviderDisplayNameSnapshot,
                ProviderRoleSnapshot = draft.ProviderRoleSnapshot,
                ServiceUnitNameSnapshot = draft.ServiceUnitNameSnapshot,
                LocationSnapshot = draft.LocationSnapshot,
                SourceModule = draft.SourceModule,
                SourceReferenceId = draft.SourceReferenceId,
                SourceReferenceNumber = draft.SourceReferenceNumber,
                SubjectiveSummary = draft.SubjectiveSummary,
                ObjectiveSummary = draft.ObjectiveSummary,
                AssessmentSummary = draft.AssessmentSummary,
                PlanSummary = draft.PlanSummary,
                Instruction = draft.Instruction,
                Evaluation = draft.Evaluation,
                NoteText = draft.NoteText,
                PrivateNote = draft.PrivateNote,
                IsGeneratedFromSource = true,
                IsReadOnlyGenerated = request.IsReadOnlyGenerated,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<TrxPatientIntegratedProgressNote>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var response = ToCreateUpdateResponse(entity);

            return Ok(ApiResponse<PatientIntegratedProgressNoteCreateResponse>.Ok(
                response,
                "CPPT dari SOAP dokter berhasil dibuat."
            ));
        }

        [HttpGet("draft-from-consultation/{consultationId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<CreatePatientIntegratedProgressNoteRequest>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Patient Integrated Progress Note", Description = "Membuat draft CPPT dari SOAP dokter tanpa menyimpan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientIntegratedProgressNote", "Read")]
        public async Task<IActionResult> BuildDraftFromConsultation(Guid consultationId)
        {
            var consultation = await _dbContext.Set<TrxDoctorConsultation>()
                .Include(x => x.Patient)
                .Include(x => x.Queue)
                .Include(x => x.Doctor)
                .Include(x => x.ServiceUnit)
                .Include(x => x.Clinic)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == consultationId && !x.IsDelete);

            if (consultation == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Konsultasi dokter tidak ditemukan."
                ));
            }

            var actorUserId = GetCurrentUserId();
            var result = BuildRequestFromConsultation(
                consultation,
                new CreatePatientIntegratedProgressNoteFromConsultationRequest(),
                actorUserId,
                DateTime.UtcNow
            );

            return Ok(ApiResponse<CreatePatientIntegratedProgressNoteRequest>.Ok(
                result,
                "Draft CPPT dari SOAP dokter berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientIntegratedProgressNoteUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Patient Integrated Progress Note", Description = "Mengubah CPPT", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("PatientIntegratedProgressNote", "Update")]
        public async Task<IActionResult> UpdateProgressNote(Guid id, [FromBody] UpdatePatientIntegratedProgressNoteRequest request)
        {
            var entity = await _dbContext.Set<TrxPatientIntegratedProgressNote>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "CPPT tidak ditemukan."
                ));
            }

            if (entity.IsCancel)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "CPPT yang sudah dibatalkan tidak dapat diubah."
                ));
            }

            if (entity.IsReadOnlyGenerated)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "CPPT hasil generate yang read-only tidak dapat diubah."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.NoteDateTime = request.NoteDateTime ?? entity.NoteDateTime;
            entity.ProfessionType = NormalizeProfessionType(request.ProfessionType);
            entity.ProfessionName = NormalizeNullableText(request.ProfessionName) ?? GetDefaultProfessionName(request.ProfessionType);
            entity.ProviderUserId = NormalizeNullableGuid(request.ProviderUserId) ?? entity.ProviderUserId;
            entity.ProviderDisplayNameSnapshot = NormalizeNullableText(request.ProviderDisplayNameSnapshot);
            entity.ProviderRoleSnapshot = NormalizeNullableText(request.ProviderRoleSnapshot);
            entity.ServiceUnitNameSnapshot = NormalizeNullableText(request.ServiceUnitNameSnapshot) ?? entity.ServiceUnitNameSnapshot;
            entity.LocationSnapshot = NormalizeNullableText(request.LocationSnapshot) ?? entity.LocationSnapshot;
            entity.SourceModule = NormalizeNullableText(request.SourceModule) ?? entity.SourceModule;
            entity.SourceReferenceId = NormalizeNullableGuid(request.SourceReferenceId) ?? entity.SourceReferenceId;
            entity.SourceReferenceNumber = NormalizeNullableText(request.SourceReferenceNumber) ?? entity.SourceReferenceNumber;
            entity.SubjectiveSummary = NormalizeNullableText(request.SubjectiveSummary);
            entity.ObjectiveSummary = NormalizeNullableText(request.ObjectiveSummary);
            entity.AssessmentSummary = NormalizeNullableText(request.AssessmentSummary);
            entity.PlanSummary = NormalizeNullableText(request.PlanSummary);
            entity.Instruction = NormalizeNullableText(request.Instruction);
            entity.Evaluation = NormalizeNullableText(request.Evaluation);
            entity.PrivateNote = NormalizeNullableText(request.PrivateNote);
            entity.NoteText = NormalizeNullableText(request.NoteText) ?? BuildNoteText(entity);
            entity.IsGeneratedFromSource = request.IsGeneratedFromSource;
            entity.IsReadOnlyGenerated = request.IsReadOnlyGenerated;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var response = ToUpdateResponse(entity);

            return Ok(ApiResponse<PatientIntegratedProgressNoteUpdateResponse>.Ok(
                response,
                "CPPT berhasil diubah."
            ));
        }

        [HttpPatch("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Cancel Patient Integrated Progress Note", Description = "Membatalkan CPPT", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("PatientIntegratedProgressNote", "Update")]
        public async Task<IActionResult> CancelProgressNote(Guid id, [FromBody] CancelPatientIntegratedProgressNoteRequest request)
        {
            var entity = await _dbContext.Set<TrxPatientIntegratedProgressNote>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "CPPT tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsActive = false;
            entity.IsCancel = true;
            entity.CancelledAt = now;
            entity.CancelledByUserId = actorUserId;
            entity.CancelReason = request.CancelReason.Trim();
            entity.CancelDateTime = now;
            entity.CancelBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "CPPT berhasil dibatalkan."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Patient Integrated Progress Note", Description = "Menghapus CPPT", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("PatientIntegratedProgressNote", "Delete")]
        public async Task<IActionResult> DeleteProgressNote(Guid id)
        {
            var entity = await _dbContext.Set<TrxPatientIntegratedProgressNote>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "CPPT tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsDelete = true;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.IsActive = false;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "CPPT berhasil dihapus."
            ));
        }

        private IQueryable<TrxPatientIntegratedProgressNote> BuildBaseQuery()
        {
            return _dbContext.Set<TrxPatientIntegratedProgressNote>()
                .Include(x => x.Patient)
                .Include(x => x.Encounter)
                .Include(x => x.Queue)
                .Include(x => x.Consultation)
                .Include(x => x.Assessment)
                .Include(x => x.VitalSign)
                .Include(x => x.Doctor)
                .Include(x => x.ServiceUnit)
                .Include(x => x.Clinic)
                .Include(x => x.ProviderUser)
                .Include(x => x.CancelledByUser)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<TrxPatientIntegratedProgressNote> ApplyFilters(
            IQueryable<TrxPatientIntegratedProgressNote> query,
            string? search,
            Guid? patientId,
            Guid? encounterId,
            Guid? queueId,
            Guid? consultationId,
            Guid? assessmentId,
            Guid? vitalSignId,
            Guid? doctorId,
            Guid? serviceUnitId,
            Guid? clinicId,
            Guid? providerUserId,
            string? professionType,
            string? sourceModule,
            bool? isGeneratedFromSource,
            bool? isActive,
            DateTime? startDate,
            DateTime? endDate)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.ProgressNoteNumber.ToLower().Contains(keyword) ||
                    x.ProfessionType.ToLower().Contains(keyword) ||
                    (x.ProfessionName != null && x.ProfessionName.ToLower().Contains(keyword)) ||
                    (x.ProviderDisplayNameSnapshot != null && x.ProviderDisplayNameSnapshot.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.FullName.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.MedicalRecordNumber.ToLower().Contains(keyword)) ||
                    (x.Encounter != null && x.Encounter.EncounterNumber.ToLower().Contains(keyword)) ||
                    (x.Queue != null && x.Queue.QueueCode.ToLower().Contains(keyword)) ||
                    (x.NoteText != null && x.NoteText.ToLower().Contains(keyword)) ||
                    (x.AssessmentSummary != null && x.AssessmentSummary.ToLower().Contains(keyword)) ||
                    (x.PlanSummary != null && x.PlanSummary.ToLower().Contains(keyword)));
            }

            if (patientId.HasValue && patientId.Value != Guid.Empty) query = query.Where(x => x.PatientId == patientId.Value);
            if (encounterId.HasValue && encounterId.Value != Guid.Empty) query = query.Where(x => x.EncounterId == encounterId.Value);
            if (queueId.HasValue && queueId.Value != Guid.Empty) query = query.Where(x => x.QueueId == queueId.Value);
            if (consultationId.HasValue && consultationId.Value != Guid.Empty) query = query.Where(x => x.ConsultationId == consultationId.Value);
            if (assessmentId.HasValue && assessmentId.Value != Guid.Empty) query = query.Where(x => x.AssessmentId == assessmentId.Value);
            if (vitalSignId.HasValue && vitalSignId.Value != Guid.Empty) query = query.Where(x => x.VitalSignId == vitalSignId.Value);
            if (doctorId.HasValue && doctorId.Value != Guid.Empty) query = query.Where(x => x.DoctorId == doctorId.Value);
            if (serviceUnitId.HasValue && serviceUnitId.Value != Guid.Empty) query = query.Where(x => x.ServiceUnitId == serviceUnitId.Value);
            if (clinicId.HasValue && clinicId.Value != Guid.Empty) query = query.Where(x => x.ClinicId == clinicId.Value);
            if (providerUserId.HasValue && providerUserId.Value != Guid.Empty) query = query.Where(x => x.ProviderUserId == providerUserId.Value);

            if (!string.IsNullOrWhiteSpace(professionType))
            {
                var normalizedProfession = NormalizeProfessionType(professionType);
                query = query.Where(x => x.ProfessionType == normalizedProfession);
            }

            if (!string.IsNullOrWhiteSpace(sourceModule))
            {
                var normalizedSource = sourceModule.Trim();
                query = query.Where(x => x.SourceModule == normalizedSource);
            }

            if (isGeneratedFromSource.HasValue) query = query.Where(x => x.IsGeneratedFromSource == isGeneratedFromSource.Value);
            if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);
            if (startDate.HasValue) query = query.Where(x => x.NoteDateTime >= startDate.Value.Date);
            if (endDate.HasValue) query = query.Where(x => x.NoteDateTime < endDate.Value.Date.AddDays(1));

            return query;
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateCreateRequestAsync(CreatePatientIntegratedProgressNoteRequest request)
        {
            if (request.PatientId == Guid.Empty)
                return (false, "PatientId wajib diisi.");

            if (string.IsNullOrWhiteSpace(request.ProfessionType))
                return (false, "ProfessionType wajib diisi.");

            var hasAnyNote =
                HasText(request.NoteText) ||
                HasText(request.SubjectiveSummary) ||
                HasText(request.ObjectiveSummary) ||
                HasText(request.AssessmentSummary) ||
                HasText(request.PlanSummary) ||
                HasText(request.Instruction) ||
                HasText(request.Evaluation);

            if (!hasAnyNote)
                return (false, "Minimal satu catatan CPPT wajib diisi.");

            var patientExists = await _dbContext.Set<MstPatient>()
                .AnyAsync(x => x.Id == request.PatientId && !x.IsDelete);

            if (!patientExists)
                return (false, "Pasien tidak ditemukan.");

            return (true, null);
        }

        private async Task<ClinicalContextResult> ResolveClinicalContextAsync(
            Guid patientId,
            Guid? encounterId,
            Guid? queueId,
            Guid? consultationId,
            Guid? assessmentId,
            Guid? vitalSignId,
            Guid? doctorId,
            Guid? serviceUnitId,
            Guid? clinicId)
        {
            var result = new ClinicalContextResult
            {
                EncounterId = NormalizeNullableGuid(encounterId),
                QueueId = NormalizeNullableGuid(queueId),
                ConsultationId = NormalizeNullableGuid(consultationId),
                AssessmentId = NormalizeNullableGuid(assessmentId),
                VitalSignId = NormalizeNullableGuid(vitalSignId),
                DoctorId = NormalizeNullableGuid(doctorId),
                ServiceUnitId = NormalizeNullableGuid(serviceUnitId),
                ClinicId = NormalizeNullableGuid(clinicId)
            };

            if (result.ConsultationId.HasValue)
            {
                var consultation = await _dbContext.Set<TrxDoctorConsultation>()
                    .Include(x => x.ServiceUnit)
                    .Include(x => x.Clinic)
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
                result.SourceModule = "DoctorConsultation";
                result.SourceReferenceId = consultation.Id;
                result.SourceReferenceNumber = consultation.ConsultationNumber;
                result.ServiceUnitNameSnapshot = consultation.ServiceUnit != null ? consultation.ServiceUnit.ServiceUnitName : null;
                result.LocationSnapshot = consultation.Clinic != null ? consultation.Clinic.ClinicName : null;

                return result.Ok();
            }

            if (result.VitalSignId.HasValue)
            {
                var vitalSign = await _dbContext.Set<TrxPatientVitalSign>()
                    .Include(x => x.ServiceUnit)
                    .Include(x => x.Clinic)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == result.VitalSignId.Value && !x.IsDelete);

                if (vitalSign == null)
                    return ClinicalContextResult.Fail("Tanda vital pasien tidak ditemukan.");

                if (vitalSign.PatientId != patientId)
                    return ClinicalContextResult.Fail("Tanda vital tidak sesuai dengan pasien.");

                result.EncounterId = vitalSign.EncounterId ?? result.EncounterId;
                result.QueueId = vitalSign.QueueId ?? result.QueueId;
                result.AssessmentId = vitalSign.AssessmentId ?? result.AssessmentId;
                result.ConsultationId = vitalSign.ConsultationId ?? result.ConsultationId;
                result.DoctorId = vitalSign.DoctorId ?? result.DoctorId;
                result.ServiceUnitId = vitalSign.ServiceUnitId ?? result.ServiceUnitId;
                result.ClinicId = vitalSign.ClinicId ?? result.ClinicId;
                result.SourceModule = "PatientVitalSign";
                result.SourceReferenceId = vitalSign.Id;
                result.SourceReferenceNumber = vitalSign.VitalSignRecordNumber;
                result.ServiceUnitNameSnapshot = vitalSign.ServiceUnit != null ? vitalSign.ServiceUnit.ServiceUnitName : null;
                result.LocationSnapshot = vitalSign.Clinic != null ? vitalSign.Clinic.ClinicName : vitalSign.ObservationLocation;

                return result.Ok();
            }

            if (result.AssessmentId.HasValue)
            {
                var assessment = await _dbContext.Set<TrxPatientAssessment>()
                    .Include(x => x.ServiceUnit)
                    .Include(x => x.Clinic)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == result.AssessmentId.Value && !x.IsDelete);

                if (assessment == null)
                    return ClinicalContextResult.Fail("Assessment pasien tidak ditemukan.");

                if (assessment.PatientId != patientId)
                    return ClinicalContextResult.Fail("Assessment tidak sesuai dengan pasien.");

                result.EncounterId = assessment.EncounterId;
                result.QueueId = assessment.QueueId;
                result.DoctorId = assessment.DoctorId ?? result.DoctorId;
                result.ServiceUnitId = assessment.ServiceUnitId;
                result.ClinicId = assessment.ClinicId;
                result.SourceModule = "PatientAssessment";
                result.SourceReferenceId = assessment.Id;
                result.SourceReferenceNumber = assessment.AssessmentNumber;
                result.ServiceUnitNameSnapshot = assessment.ServiceUnit != null ? assessment.ServiceUnit.ServiceUnitName : null;
                result.LocationSnapshot = assessment.Clinic != null ? assessment.Clinic.ClinicName : null;

                return result.Ok();
            }

            if (result.QueueId.HasValue)
            {
                var queue = await _dbContext.Set<TrxQueue>()
                    .Include(x => x.ServiceUnit)
                    .Include(x => x.Clinic)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == result.QueueId.Value && !x.IsDelete);

                if (queue == null)
                    return ClinicalContextResult.Fail("Antrean pasien tidak ditemukan.");

                if (queue.PatientId != patientId)
                    return ClinicalContextResult.Fail("Antrean tidak sesuai dengan pasien.");

                result.EncounterId = queue.EncounterId;
                result.DoctorId = queue.DoctorId ?? result.DoctorId;
                result.ServiceUnitId = queue.ServiceUnitId;
                result.ClinicId = queue.ClinicId;
                result.ServiceUnitNameSnapshot = queue.ServiceUnit != null ? queue.ServiceUnit.ServiceUnitName : null;
                result.LocationSnapshot = queue.Clinic != null ? queue.Clinic.ClinicName : null;

                return result.Ok();
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

        private async Task<string> GenerateProgressNoteNumberAsync(DateTime now)
        {
            var prefix = $"CPPT-{now:yyyyMMdd}";

            var countToday = await _dbContext.Set<TrxPatientIntegratedProgressNote>()
                .CountAsync(x => x.ProgressNoteNumber.StartsWith(prefix));

            return $"{prefix}-{countToday + 1:0000}";
        }

        private static CreatePatientIntegratedProgressNoteRequest BuildRequestFromConsultation(
            TrxDoctorConsultation consultation,
            CreatePatientIntegratedProgressNoteFromConsultationRequest request,
            Guid currentUserId,
            DateTime now)
        {
            var subjective = NormalizeNullableText(consultation.Subjective);
            var objective = NormalizeNullableText(consultation.Objective);
            var assessment = NormalizeNullableText(consultation.Assessment);
            var planItems = new[]
            {
                NormalizeNullableText(consultation.Plan),
                NormalizeNullableText(consultation.PrescriptionPlan),
                NormalizeNullableText(consultation.ProcedurePlan),
                NormalizeNullableText(consultation.EducationPlan),
                NormalizeNullableText(consultation.DoctorNote)
            }
            .Where(HasText)
            .ToList();

            var plan = planItems.Any()
                ? string.Join("\n\n", planItems)
                : null;

            var draft = new CreatePatientIntegratedProgressNoteRequest
            {
                PatientId = consultation.PatientId,
                EncounterId = consultation.EncounterId,
                QueueId = consultation.QueueId,
                ConsultationId = consultation.Id,
                AssessmentId = consultation.AssessmentId,
                DoctorId = consultation.DoctorId,
                ServiceUnitId = consultation.ServiceUnitId,
                ClinicId = consultation.ClinicId,
                NoteDateTime = request.NoteDateTime ?? now,
                ProfessionType = "Doctor",
                ProfessionName = "Dokter",
                ProviderUserId = request.UseCurrentUserAsProvider ? currentUserId : null,
                ProviderDisplayNameSnapshot = consultation.Doctor != null ? consultation.Doctor.FullName : null,
                ProviderRoleSnapshot = "Dokter Penanggung Jawab",
                ServiceUnitNameSnapshot = consultation.ServiceUnit != null ? consultation.ServiceUnit.ServiceUnitName : null,
                LocationSnapshot = consultation.Clinic != null ? consultation.Clinic.ClinicName : null,
                SourceModule = "DoctorConsultation",
                SourceReferenceId = consultation.Id,
                SourceReferenceNumber = consultation.ConsultationNumber,
                SubjectiveSummary = subjective,
                ObjectiveSummary = objective,
                AssessmentSummary = assessment,
                PlanSummary = plan,
                Instruction = NormalizeNullableText(request.AdditionalInstruction),
                Evaluation = NormalizeNullableText(request.Evaluation),
                PrivateNote = NormalizeNullableText(request.PrivateNote),
                IsGeneratedFromSource = true,
                IsReadOnlyGenerated = request.IsReadOnlyGenerated,
                IsActive = true
            };

            draft.NoteText = BuildNoteText(new TrxPatientIntegratedProgressNote
            {
                SubjectiveSummary = draft.SubjectiveSummary,
                ObjectiveSummary = draft.ObjectiveSummary,
                AssessmentSummary = draft.AssessmentSummary,
                PlanSummary = draft.PlanSummary,
                Instruction = draft.Instruction,
                Evaluation = draft.Evaluation
            });

            return draft;
        }

        private static string? BuildNoteText(TrxPatientIntegratedProgressNote note)
        {
            var builder = new StringBuilder();

            AppendLine(builder, "Keluhan utama", note.SubjectiveSummary);
            AppendLine(builder, "Pemeriksaan", note.ObjectiveSummary);
            AppendLine(builder, "Assessment", note.AssessmentSummary);
            AppendLine(builder, "Rencana", note.PlanSummary);
            AppendLine(builder, "Instruksi", note.Instruction);
            AppendLine(builder, "Evaluasi/Respon", note.Evaluation);

            var result = builder.ToString().Trim();
            return string.IsNullOrWhiteSpace(result) ? null : result;
        }

        private static void AppendLine(StringBuilder builder, string label, string? value)
        {
            if (!HasText(value)) return;
            builder.Append(label).Append(": ").AppendLine(value!.Trim());
        }

        private static IQueryable<TrxPatientIntegratedProgressNote> ApplySorting(
            IQueryable<TrxPatientIntegratedProgressNote> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "noteDateTime").ToLowerInvariant() switch
            {
                "progressnotenumber" => isDesc ? query.OrderByDescending(x => x.ProgressNoteNumber) : query.OrderBy(x => x.ProgressNoteNumber),
                "professiontype" => isDesc ? query.OrderByDescending(x => x.ProfessionType) : query.OrderBy(x => x.ProfessionType),
                "sourcemodule" => isDesc ? query.OrderByDescending(x => x.SourceModule) : query.OrderBy(x => x.SourceModule),
                "createdatetime" => isDesc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                _ => isDesc
                    ? query.OrderByDescending(x => x.NoteDateTime).ThenByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.NoteDateTime).ThenBy(x => x.CreateDateTime)
            };
        }

        private static PatientIntegratedProgressNoteResponse ToResponse(TrxPatientIntegratedProgressNote x)
        {
            return new PatientIntegratedProgressNoteResponse
            {
                Id = x.Id,
                ProgressNoteNumber = x.ProgressNoteNumber,
                PatientId = x.PatientId,
                PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty,
                EncounterId = x.EncounterId,
                EncounterNumber = x.Encounter != null ? x.Encounter.EncounterNumber : null,
                QueueId = x.QueueId,
                QueueCode = x.Queue != null ? x.Queue.QueueCode : null,
                ConsultationId = x.ConsultationId,
                ConsultationNumber = x.Consultation != null ? x.Consultation.ConsultationNumber : null,
                AssessmentId = x.AssessmentId,
                AssessmentNumber = x.Assessment != null ? x.Assessment.AssessmentNumber : null,
                VitalSignId = x.VitalSignId,
                VitalSignRecordNumber = x.VitalSign != null ? x.VitalSign.VitalSignRecordNumber : null,
                DoctorId = x.DoctorId,
                DoctorName = x.Doctor != null ? x.Doctor.FullName : null,
                ServiceUnitId = x.ServiceUnitId,
                ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : x.ServiceUnitNameSnapshot,
                ClinicId = x.ClinicId,
                ClinicName = x.Clinic != null ? x.Clinic.ClinicName : null,
                NoteDateTime = x.NoteDateTime,
                ProfessionType = x.ProfessionType,
                ProfessionName = x.ProfessionName,
                ProviderUserId = x.ProviderUserId,
                ProviderUserName = x.ProviderUser != null ? x.ProviderUser.DisplayName : null,
                ProviderDisplayNameSnapshot = x.ProviderDisplayNameSnapshot,
                ProviderRoleSnapshot = x.ProviderRoleSnapshot,
                ServiceUnitNameSnapshot = x.ServiceUnitNameSnapshot,
                LocationSnapshot = x.LocationSnapshot,
                SourceModule = x.SourceModule,
                SourceReferenceId = x.SourceReferenceId,
                SourceReferenceNumber = x.SourceReferenceNumber,
                NoteText = x.NoteText,
                IsGeneratedFromSource = x.IsGeneratedFromSource,
                IsReadOnlyGenerated = x.IsReadOnlyGenerated,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime
            };
        }

        private static PatientIntegratedProgressNoteDetailResponse ToDetailResponse(TrxPatientIntegratedProgressNote x)
        {
            var response = new PatientIntegratedProgressNoteDetailResponse
            {
                Id = x.Id,
                ProgressNoteNumber = x.ProgressNoteNumber,
                PatientId = x.PatientId,
                PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty,
                EncounterId = x.EncounterId,
                EncounterNumber = x.Encounter != null ? x.Encounter.EncounterNumber : null,
                QueueId = x.QueueId,
                QueueCode = x.Queue != null ? x.Queue.QueueCode : null,
                ConsultationId = x.ConsultationId,
                ConsultationNumber = x.Consultation != null ? x.Consultation.ConsultationNumber : null,
                AssessmentId = x.AssessmentId,
                AssessmentNumber = x.Assessment != null ? x.Assessment.AssessmentNumber : null,
                VitalSignId = x.VitalSignId,
                VitalSignRecordNumber = x.VitalSign != null ? x.VitalSign.VitalSignRecordNumber : null,
                DoctorId = x.DoctorId,
                DoctorName = x.Doctor != null ? x.Doctor.FullName : null,
                ServiceUnitId = x.ServiceUnitId,
                ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : x.ServiceUnitNameSnapshot,
                ClinicId = x.ClinicId,
                ClinicName = x.Clinic != null ? x.Clinic.ClinicName : null,
                NoteDateTime = x.NoteDateTime,
                ProfessionType = x.ProfessionType,
                ProfessionName = x.ProfessionName,
                ProviderUserId = x.ProviderUserId,
                ProviderUserName = x.ProviderUser != null ? x.ProviderUser.DisplayName : null,
                ProviderDisplayNameSnapshot = x.ProviderDisplayNameSnapshot,
                ProviderRoleSnapshot = x.ProviderRoleSnapshot,
                ServiceUnitNameSnapshot = x.ServiceUnitNameSnapshot,
                LocationSnapshot = x.LocationSnapshot,
                SourceModule = x.SourceModule,
                SourceReferenceId = x.SourceReferenceId,
                SourceReferenceNumber = x.SourceReferenceNumber,
                SubjectiveSummary = x.SubjectiveSummary,
                ObjectiveSummary = x.ObjectiveSummary,
                AssessmentSummary = x.AssessmentSummary,
                PlanSummary = x.PlanSummary,
                Instruction = x.Instruction,
                Evaluation = x.Evaluation,
                NoteText = x.NoteText,
                PrivateNote = x.PrivateNote,
                IsGeneratedFromSource = x.IsGeneratedFromSource,
                IsReadOnlyGenerated = x.IsReadOnlyGenerated,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime,
                CancelledAt = x.CancelledAt,
                CancelledByUserId = x.CancelledByUserId,
                CancelledByUserName = x.CancelledByUser != null ? x.CancelledByUser.DisplayName : null,
                CancelReason = x.CancelReason
            };

            return response;
        }

        private static PatientIntegratedProgressNoteTimelineResponse ToTimelineResponse(TrxPatientIntegratedProgressNote x)
        {
            return new PatientIntegratedProgressNoteTimelineResponse
            {
                Id = x.Id,
                ProgressNoteNumber = x.ProgressNoteNumber,
                NoteDateTime = x.NoteDateTime,
                ProfessionType = x.ProfessionType,
                ProfessionName = x.ProfessionName ?? GetDefaultProfessionName(x.ProfessionType),
                ProfessionTone = GetProfessionTone(x.ProfessionType),
                ProviderUserId = x.ProviderUserId,
                ProviderName = FirstNotEmpty(
                    x.ProviderDisplayNameSnapshot,
                    x.ProviderUser != null ? x.ProviderUser.DisplayName : null,
                    x.Doctor != null ? x.Doctor.FullName : null,
                    "-"),
                ProviderRole = FirstNotEmpty(x.ProviderRoleSnapshot, GetDefaultProfessionName(x.ProfessionType), "-"),
                ServiceUnitId = x.ServiceUnitId,
                ServiceUnitName = FirstNotEmpty(
                    x.ServiceUnitNameSnapshot,
                    x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null,
                    x.Clinic != null ? x.Clinic.ClinicName : null,
                    "-"),
                ClinicId = x.ClinicId,
                ClinicName = x.Clinic != null ? x.Clinic.ClinicName : null,
                SourceModule = x.SourceModule,
                SourceReferenceId = x.SourceReferenceId,
                SourceReferenceNumber = x.SourceReferenceNumber,
                NoteText = FirstNotEmpty(x.NoteText, BuildNoteText(x), "-"),
                IsGeneratedFromSource = x.IsGeneratedFromSource,
                IsReadOnlyGenerated = x.IsReadOnlyGenerated
            };
        }

        private static PatientIntegratedProgressNoteCreateResponse ToCreateUpdateResponse(TrxPatientIntegratedProgressNote x)
        {
            return new PatientIntegratedProgressNoteCreateResponse
            {
                Id = x.Id,
                ProgressNoteNumber = x.ProgressNoteNumber,
                PatientId = x.PatientId,
                EncounterId = x.EncounterId,
                QueueId = x.QueueId,
                ConsultationId = x.ConsultationId,
                NoteDateTime = x.NoteDateTime,
                ProfessionType = x.ProfessionType,
                ProfessionName = x.ProfessionName,
                SourceModule = x.SourceModule,
                SourceReferenceId = x.SourceReferenceId,
                IsGeneratedFromSource = x.IsGeneratedFromSource,
                IsReadOnlyGenerated = x.IsReadOnlyGenerated,
                IsActive = x.IsActive
            };
        }

        private static PatientIntegratedProgressNoteUpdateResponse ToUpdateResponse(TrxPatientIntegratedProgressNote x)
        {
            return new PatientIntegratedProgressNoteUpdateResponse
            {
                Id = x.Id,
                ProgressNoteNumber = x.ProgressNoteNumber,
                PatientId = x.PatientId,
                EncounterId = x.EncounterId,
                QueueId = x.QueueId,
                ConsultationId = x.ConsultationId,
                NoteDateTime = x.NoteDateTime,
                ProfessionType = x.ProfessionType,
                ProfessionName = x.ProfessionName,
                SourceModule = x.SourceModule,
                SourceReferenceId = x.SourceReferenceId,
                IsGeneratedFromSource = x.IsGeneratedFromSource,
                IsReadOnlyGenerated = x.IsReadOnlyGenerated,
                IsActive = x.IsActive
            };
        }

        private static List<PatientIntegratedProgressNoteProfessionOptionResponse> BuildProfessionOptions()
        {
            return new List<PatientIntegratedProgressNoteProfessionOptionResponse>
            {
                new() { Value = "Doctor", Label = "Dokter", Tone = "doctor" },
                new() { Value = "Nurse", Label = "Perawat", Tone = "nurse" },
                new() { Value = "Pharmacist", Label = "Farmasi", Tone = "pharmacy" },
                new() { Value = "Nutritionist", Label = "Gizi", Tone = "nutrition" },
                new() { Value = "Midwife", Label = "Bidan", Tone = "midwife" },
                new() { Value = "Physiotherapist", Label = "Fisioterapi", Tone = "physiotherapy" },
                new() { Value = "Laboratory", Label = "Laboratorium", Tone = "laboratory" },
                new() { Value = "Radiology", Label = "Radiologi", Tone = "radiology" },
                new() { Value = "Other", Label = "Lainnya", Tone = "neutral" }
            };
        }

        private static List<PatientIntegratedProgressNoteSourceOptionResponse> BuildSourceModuleOptions()
        {
            return new List<PatientIntegratedProgressNoteSourceOptionResponse>
            {
                new() { Value = "ManualEntry", Label = "Input Manual" },
                new() { Value = "DoctorConsultation", Label = "SOAP Dokter" },
                new() { Value = "PatientAssessment", Label = "Assessment Pasien" },
                new() { Value = "PatientVitalSign", Label = "Tanda Vital" },
                new() { Value = "Prescription", Label = "Resep" },
                new() { Value = "Procedure", Label = "Tindakan" },
                new() { Value = "Pharmacy", Label = "Farmasi" },
                new() { Value = "Nutrition", Label = "Gizi" }
            };
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            return (pageNumber, pageSize);
        }

        private static string NormalizeProfessionType(string? value)
        {
            var text = NormalizeNullableText(value) ?? "Other";
            var lowered = text.ToLowerInvariant();

            return lowered switch
            {
                "dokter" or "doctor" => "Doctor",
                "perawat" or "nurse" => "Nurse",
                "farmasi" or "pharmacist" or "pharmacy" => "Pharmacist",
                "gizi" or "nutrition" or "nutritionist" => "Nutritionist",
                "bidan" or "midwife" => "Midwife",
                "fisioterapi" or "physiotherapy" or "physiotherapist" => "Physiotherapist",
                "laboratorium" or "laboratory" => "Laboratory",
                "radiologi" or "radiology" => "Radiology",
                _ => text
            };
        }

        private static string GetDefaultProfessionName(string? professionType)
        {
            return NormalizeProfessionType(professionType) switch
            {
                "Doctor" => "Dokter",
                "Nurse" => "Perawat",
                "Pharmacist" => "Farmasi",
                "Nutritionist" => "Gizi",
                "Midwife" => "Bidan",
                "Physiotherapist" => "Fisioterapi",
                "Laboratory" => "Laboratorium",
                "Radiology" => "Radiologi",
                _ => "Lainnya"
            };
        }

        private static string GetProfessionTone(string? professionType)
        {
            return NormalizeProfessionType(professionType) switch
            {
                "Doctor" => "doctor",
                "Nurse" => "nurse",
                "Pharmacist" => "pharmacy",
                "Nutritionist" => "nutrition",
                "Midwife" => "midwife",
                "Physiotherapist" => "physiotherapy",
                "Laboratory" => "laboratory",
                "Radiology" => "radiology",
                _ => "neutral"
            };
        }

        private static bool HasText(string? value)
        {
            return !string.IsNullOrWhiteSpace(value);
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

        private static string FirstNotEmpty(params string?[] values)
        {
            foreach (var value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                    return value.Trim();
            }

            return string.Empty;
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
            public Guid? ConsultationId { get; set; }
            public Guid? AssessmentId { get; set; }
            public Guid? VitalSignId { get; set; }
            public Guid? DoctorId { get; set; }
            public Guid? ServiceUnitId { get; set; }
            public Guid? ClinicId { get; set; }
            public string? SourceModule { get; set; }
            public Guid? SourceReferenceId { get; set; }
            public string? SourceReferenceNumber { get; set; }
            public string? ServiceUnitNameSnapshot { get; set; }
            public string? LocationSnapshot { get; set; }

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
