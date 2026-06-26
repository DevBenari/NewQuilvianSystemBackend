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

using ResponsePatientVitalSignPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs.PatientVitalSignResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/clinical-management/patient-vital-signs")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_CLINICAL",
        moduleName: "Health Service Clinical",
        displayName: "Patient Vital Sign",
        AreaName = "HealthServices",
        ControllerName = "PatientVitalSign",
        Description = "Catatan observasi tanda vital pasien",
        SortOrder = 8
    )]
    [Tags("Health Services / Clinical Management / Patient Vital Sign")]
    public class PatientVitalSignController : ControllerBase
    {
        private const string LogCategory = "HealthServices.Clinical";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public PatientVitalSignController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<PatientVitalSignFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Vital Sign", Description = "Melihat metadata filter tanda vital pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientVitalSign", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new PatientVitalSignFilterMetadataResponse
            {
                DefaultFilter = new PatientVitalSignDefaultFilterResponse(),
                SortOptions = new List<PatientVitalSignSortOptionResponse>
                {
                    new() { Value = "observationDateTime", Label = "Waktu observasi" },
                    new() { Value = "vitalSignRecordNumber", Label = "Nomor catatan" },
                    new() { Value = "vitalSignSource", Label = "Sumber data" },
                    new() { Value = "vitalSignStatus", Label = "Status catatan" },
                    new() { Value = "bloodPressureSystolic", Label = "Sistolik" },
                    new() { Value = "pulseRate", Label = "Nadi" },
                    new() { Value = "temperature", Label = "Suhu" },
                    new() { Value = "oxygenSaturation", Label = "SpO2" },
                    new() { Value = "earlyWarningScore", Label = "EWS" },
                    new() { Value = "ewsRiskLevel", Label = "Level risiko EWS" },
                    new() { Value = "isAbnormal", Label = "Abnormal" },
                    new() { Value = "isCritical", Label = "Kritis" },
                    new() { Value = "isVerified", Label = "Terverifikasi" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                VitalSignSourceOptions = BuildEnumOptions<PatientVitalSignSource>(),
                VitalSignStatusOptions = BuildEnumOptions<PatientVitalSignStatus>(),
                PatientPositionOptions = BuildEnumOptions<PatientPosition>(),
                OxygenSupportTypeOptions = BuildEnumOptions<OxygenSupportType>(),
                ConsciousnessStatusOptions = BuildEnumOptions<ConsciousnessStatus>(),
                MapStatusOptions = BuildEnumOptions<MapStatus>(),
                EwsRiskLevelOptions = BuildEnumOptions<EwsRiskLevel>()
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "PatientVitalSign.GetFilterMetadata",
                "Mengambil metadata filter tanda vital pasien.",
                result
            );

            return Ok(ApiResponse<PatientVitalSignFilterMetadataResponse>.Ok(
                result,
                "Metadata filter tanda vital pasien berhasil diambil."
            ));
        }

        [HttpGet("critical-alerts")]
        [ProducesResponseType(typeof(ApiResponse<List<PatientVitalSignAlertResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Vital Sign", Description = "Melihat alert tanda vital kritis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientVitalSign", "Read")]
        public async Task<IActionResult> GetCriticalAlerts(
            [FromQuery] Guid? patientId,
            [FromQuery] Guid? encounterId,
            [FromQuery] bool onlyNeedDoctorNotification = true)
        {
            var query = BuildBaseQuery()
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    x.VitalSignStatus != PatientVitalSignStatus.Cancelled &&
                    x.VitalSignStatus != PatientVitalSignStatus.EnteredInError &&
                    (x.IsCritical || x.IsAbnormal || x.NeedDoctorNotification));

            if (patientId.HasValue && patientId.Value != Guid.Empty)
                query = query.Where(x => x.PatientId == patientId.Value);

            if (encounterId.HasValue && encounterId.Value != Guid.Empty)
                query = query.Where(x => x.EncounterId == encounterId.Value);

            if (onlyNeedDoctorNotification)
                query = query.Where(x => x.NeedDoctorNotification);

            var data = await query
                .OrderByDescending(x => x.IsCritical)
                .ThenByDescending(x => x.EwsRiskLevel)
                .ThenByDescending(x => x.ObservationDateTime)
                .Take(100)
                .Select(x => new PatientVitalSignAlertResponse
                {
                    Id = x.Id,
                    VitalSignRecordNumber = x.VitalSignRecordNumber,
                    PatientId = x.PatientId,
                    PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                    MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty,
                    EncounterId = x.EncounterId,
                    ConsultationId = x.ConsultationId,
                    ObservationDateTime = x.ObservationDateTime,
                    BloodPressureSystolic = x.BloodPressureSystolic,
                    BloodPressureDiastolic = x.BloodPressureDiastolic,
                    PulseRate = x.PulseRate,
                    RespiratoryRate = x.RespiratoryRate,
                    Temperature = x.Temperature,
                    OxygenSaturation = x.OxygenSaturation,
                    EarlyWarningScore = x.EarlyWarningScore,
                    EwsRiskLevel = x.EwsRiskLevel,
                    IsAbnormal = x.IsAbnormal,
                    IsCritical = x.IsCritical,
                    NeedDoctorNotification = x.NeedDoctorNotification,
                    EwsMonitoringRecommendation = x.EwsMonitoringRecommendation,
                    ClinicalNote = x.ClinicalNote
                })
                .ToListAsync();

            return Ok(ApiResponse<List<PatientVitalSignAlertResponse>>.Ok(
                data,
                "Alert tanda vital pasien berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponsePatientVitalSignPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Vital Sign", Description = "Melihat data tanda vital pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientVitalSign", "Read")]
        public async Task<IActionResult> GetVitalSigns(
            [FromQuery] string? search,
            [FromQuery] Guid? patientId,
            [FromQuery] Guid? encounterId,
            [FromQuery] Guid? queueId,
            [FromQuery] Guid? assessmentId,
            [FromQuery] Guid? consultationId,
            [FromQuery] Guid? doctorId,
            [FromQuery] Guid? serviceUnitId,
            [FromQuery] Guid? clinicId,
            [FromQuery] PatientVitalSignSource? vitalSignSource,
            [FromQuery] PatientVitalSignStatus? vitalSignStatus,
            [FromQuery] PatientPosition? patientPosition,
            [FromQuery] ConsciousnessStatus? consciousnessStatus,
            [FromQuery] MapStatus? mapStatus,
            [FromQuery] EwsRiskLevel? ewsRiskLevel,
            [FromQuery] bool? isUsingOxygen,
            [FromQuery] bool? hasPain,
            [FromQuery] bool? isAbnormal,
            [FromQuery] bool? isCritical,
            [FromQuery] bool? needDoctorNotification,
            [FromQuery] bool? isVerified,
            [FromQuery] bool? isActive,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? sortBy = "observationDateTime",
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
                doctorId,
                serviceUnitId,
                clinicId,
                vitalSignSource,
                vitalSignStatus,
                patientPosition,
                consciousnessStatus,
                mapStatus,
                ewsRiskLevel,
                isUsingOxygen,
                hasPain,
                isAbnormal,
                isCritical,
                needDoctorNotification,
                isVerified,
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

            var result = new ResponsePatientVitalSignPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponsePatientVitalSignPagedResult>.Ok(
                result,
                "Data tanda vital pasien berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<PatientVitalSignOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Vital Sign", Description = "Melihat pilihan tanda vital pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientVitalSign", "Read")]
        public async Task<IActionResult> GetVitalSignOptions(
            [FromQuery] Guid? patientId,
            [FromQuery] Guid? encounterId,
            [FromQuery] Guid? consultationId,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null)
        {
            var query = _dbContext.Set<TrxPatientVitalSign>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (onlyActive)
                query = query.Where(x => x.IsActive && x.VitalSignStatus != PatientVitalSignStatus.Cancelled && x.VitalSignStatus != PatientVitalSignStatus.EnteredInError);

            if (patientId.HasValue && patientId.Value != Guid.Empty)
                query = query.Where(x => x.PatientId == patientId.Value);

            if (encounterId.HasValue && encounterId.Value != Guid.Empty)
                query = query.Where(x => x.EncounterId == encounterId.Value);

            if (consultationId.HasValue && consultationId.Value != Guid.Empty)
                query = query.Where(x => x.ConsultationId == consultationId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.VitalSignRecordNumber.ToLower().Contains(keyword) ||
                    (x.ObservationLocation != null && x.ObservationLocation.ToLower().Contains(keyword)) ||
                    (x.MeasurementMethod != null && x.MeasurementMethod.ToLower().Contains(keyword)) ||
                    (x.ClinicalNote != null && x.ClinicalNote.ToLower().Contains(keyword)));
            }

            var data = await query
                .OrderByDescending(x => x.ObservationDateTime)
                .Take(100)
                .Select(x => new PatientVitalSignOptionResponse
                {
                    Id = x.Id,
                    VitalSignRecordNumber = x.VitalSignRecordNumber,
                    PatientId = x.PatientId,
                    ObservationDateTime = x.ObservationDateTime,
                    VitalSignSource = x.VitalSignSource,
                    VitalSignStatus = x.VitalSignStatus,
                    BloodPressureSystolic = x.BloodPressureSystolic,
                    BloodPressureDiastolic = x.BloodPressureDiastolic,
                    PulseRate = x.PulseRate,
                    RespiratoryRate = x.RespiratoryRate,
                    Temperature = x.Temperature,
                    OxygenSaturation = x.OxygenSaturation,
                    EarlyWarningScore = x.EarlyWarningScore,
                    EwsRiskLevel = x.EwsRiskLevel,
                    IsAbnormal = x.IsAbnormal,
                    IsCritical = x.IsCritical,
                    IsVerified = x.IsVerified
                })
                .ToListAsync();

            return Ok(ApiResponse<List<PatientVitalSignOptionResponse>>.Ok(
                data,
                "Data pilihan tanda vital pasien berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientVitalSignDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Patient Vital Sign", Description = "Melihat detail tanda vital pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientVitalSign", "Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await BuildBaseQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Tanda vital pasien tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<PatientVitalSignDetailResponse>.Ok(
                ToDetailResponse(entity),
                "Detail tanda vital pasien berhasil diambil."
            ));
        }

        [HttpGet("active-by-encounter/{encounterId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientVitalSignDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Patient Vital Sign", Description = "Melihat draft/record tanda vital aktif berdasarkan encounter", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientVitalSign", "Read")]
        public async Task<IActionResult> GetActiveByEncounter(Guid encounterId, [FromQuery] Guid? queueId = null)
        {
            var query = BuildBaseQuery()
                .AsNoTracking()
                .Where(x =>
                    x.EncounterId == encounterId &&
                    !x.IsDelete &&
                    x.IsActive &&
                    x.VitalSignStatus != PatientVitalSignStatus.Cancelled &&
                    x.VitalSignStatus != PatientVitalSignStatus.EnteredInError);

            if (queueId.HasValue && queueId.Value != Guid.Empty)
            {
                query = query.Where(x => x.QueueId == queueId.Value);
            }

            var entity = await query
                .OrderByDescending(x => x.VitalSignStatus == PatientVitalSignStatus.Draft)
                .ThenByDescending(x => x.UpdateDateTime ?? x.CreateDateTime)
                .ThenByDescending(x => x.ObservationDateTime)
                .FirstOrDefaultAsync();

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Draft/catatan tanda vital aktif untuk encounter ini tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<PatientVitalSignDetailResponse>.Ok(
                ToDetailResponse(entity),
                "Draft/catatan tanda vital aktif berhasil diambil."
            ));
        }

        [HttpGet("active-by-queue/{queueId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientVitalSignDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Patient Vital Sign", Description = "Melihat draft/record tanda vital aktif berdasarkan antrean", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientVitalSign", "Read")]
        public async Task<IActionResult> GetActiveByQueue(Guid queueId)
        {
            var entity = await BuildBaseQuery()
                .AsNoTracking()
                .Where(x =>
                    x.QueueId == queueId &&
                    !x.IsDelete &&
                    x.IsActive &&
                    x.VitalSignStatus != PatientVitalSignStatus.Cancelled &&
                    x.VitalSignStatus != PatientVitalSignStatus.EnteredInError)
                .OrderByDescending(x => x.VitalSignStatus == PatientVitalSignStatus.Draft)
                .ThenByDescending(x => x.UpdateDateTime ?? x.CreateDateTime)
                .ThenByDescending(x => x.ObservationDateTime)
                .FirstOrDefaultAsync();

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Draft/catatan tanda vital aktif untuk antrean ini tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<PatientVitalSignDetailResponse>.Ok(
                ToDetailResponse(entity),
                "Draft/catatan tanda vital aktif berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<PatientVitalSignCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Patient Vital Sign", Description = "Membuat data tanda vital pasien", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("PatientVitalSign", "Create")]
        public async Task<IActionResult> CreateVitalSign([FromBody] CreatePatientVitalSignRequest request)
        {
            var validation = await ValidateCreateRequestAsync(request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data tanda vital pasien tidak valid."
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

            var calculated = CalculateVitalSignValues(request);

            var entity = new TrxPatientVitalSign
            {
                Id = Guid.NewGuid(),
                VitalSignRecordNumber = await GenerateVitalSignRecordNumberAsync(now),
                PatientId = request.PatientId,
                EncounterId = context.EncounterId,
                QueueId = context.QueueId,
                AssessmentId = context.AssessmentId,
                ConsultationId = context.ConsultationId,
                DoctorId = context.DoctorId,
                ServiceUnitId = context.ServiceUnitId,
                ClinicId = context.ClinicId,
                ObservationDateTime = request.ObservationDateTime ?? now,
                VitalSignSource = request.VitalSignSource,
                VitalSignStatus = request.IsVerified ? PatientVitalSignStatus.Verified : request.VitalSignStatus,
                ObservedByUserId = actorUserId,
                ObservationLocation = NormalizeNullableText(request.ObservationLocation),
                PatientPosition = request.PatientPosition,
                MeasurementMethod = NormalizeNullableText(request.MeasurementMethod),
                DeviceName = NormalizeNullableText(request.DeviceName),
                DeviceSerialNumber = NormalizeNullableText(request.DeviceSerialNumber),
                BloodPressureSystolic = request.BloodPressureSystolic,
                BloodPressureDiastolic = request.BloodPressureDiastolic,
                MeanArterialPressure = calculated.MeanArterialPressure,
                MapStatus = calculated.MapStatus,
                BloodPressureLocation = NormalizeNullableText(request.BloodPressureLocation),
                PulseRate = request.PulseRate,
                IsPulseReadable = request.IsPulseReadable,
                IsPulseRegular = request.IsPulseRegular,
                PulseRhythmNote = NormalizeNullableText(request.PulseRhythmNote),
                RespiratoryRate = request.RespiratoryRate,
                Temperature = request.Temperature,
                TemperatureRoute = NormalizeNullableText(request.TemperatureRoute),
                OxygenSaturation = request.OxygenSaturation,
                IsUsingOxygen = request.IsUsingOxygen,
                OxygenSupportType = request.OxygenSupportType,
                OxygenFlowRate = request.OxygenFlowRate,
                OxygenSupportNote = NormalizeNullableText(request.OxygenSupportNote),
                Weight = request.Weight,
                Height = request.Height,
                BMI = calculated.BMI,
                WeightMeasurementNote = NormalizeNullableText(request.WeightMeasurementNote),
                ConsciousnessStatus = request.ConsciousnessStatus,
                GcsEye = request.GcsEye,
                GcsVerbal = request.GcsVerbal,
                GcsMotor = request.GcsMotor,
                GcsTotal = calculated.GcsTotal,
                NeurologicalNote = NormalizeNullableText(request.NeurologicalNote),
                HasPain = request.HasPain,
                PainScale = request.PainScale,
                PainLocation = NormalizeNullableText(request.PainLocation),
                PainNote = NormalizeNullableText(request.PainNote),
                EarlyWarningScore = calculated.EarlyWarningScore,
                EwsRiskLevel = calculated.EwsRiskLevel,
                EwsMonitoringRecommendation = calculated.EwsMonitoringRecommendation,
                IsAbnormal = calculated.IsAbnormal,
                IsCritical = calculated.IsCritical,
                NeedDoctorNotification = request.NeedDoctorNotification || calculated.IsCritical || calculated.EwsRiskLevel == EwsRiskLevel.High || calculated.EwsRiskLevel == EwsRiskLevel.Critical,
                IsVerified = request.IsVerified,
                VerifiedAt = request.IsVerified ? now : null,
                VerifiedByUserId = request.IsVerified ? actorUserId : null,
                ClinicalNote = NormalizeNullableText(request.ClinicalNote),
                Notes = NormalizeNullableText(request.Notes),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            NormalizeVitalSignData(entity);

            _dbContext.Set<TrxPatientVitalSign>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var response = ToCreateUpdateResponse(entity);

            await _loggerService.InfoAsync(
                LogCategory,
                "PatientVitalSign.CreateVitalSign",
                "Membuat catatan tanda vital pasien.",
                response
            );

            return Ok(ApiResponse<PatientVitalSignCreateResponse>.Ok(
                response,
                "Tanda vital pasien berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientVitalSignUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Patient Vital Sign", Description = "Mengubah data tanda vital pasien", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("PatientVitalSign", "Update")]
        public async Task<IActionResult> UpdateVitalSign(Guid id, [FromBody] UpdatePatientVitalSignRequest request)
        {
            var entity = await _dbContext.Set<TrxPatientVitalSign>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Tanda vital pasien tidak ditemukan."
                ));
            }

            if (entity.VitalSignStatus == PatientVitalSignStatus.Cancelled ||
                entity.VitalSignStatus == PatientVitalSignStatus.EnteredInError)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Tanda vital yang sudah cancelled atau entered in error tidak dapat diubah."
                ));
            }

            var validation = ValidateUpdateRequest(request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data tanda vital pasien tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var calculated = CalculateVitalSignValues(request);

            entity.ObservationDateTime = request.ObservationDateTime ?? entity.ObservationDateTime;
            entity.VitalSignSource = request.VitalSignSource;
            // Untuk kebutuhan autosave screening perawat, status Draft harus tetap bisa disimpan.
            // Jika user mengirim Draft, record tetap Draft agar bisa dilanjutkan setelah refresh/mati listrik.
            entity.VitalSignStatus = request.VitalSignStatus;
            entity.ObservationLocation = NormalizeNullableText(request.ObservationLocation);
            entity.PatientPosition = request.PatientPosition;
            entity.MeasurementMethod = NormalizeNullableText(request.MeasurementMethod);
            entity.DeviceName = NormalizeNullableText(request.DeviceName);
            entity.DeviceSerialNumber = NormalizeNullableText(request.DeviceSerialNumber);
            entity.BloodPressureSystolic = request.BloodPressureSystolic;
            entity.BloodPressureDiastolic = request.BloodPressureDiastolic;
            entity.MeanArterialPressure = calculated.MeanArterialPressure;
            entity.MapStatus = calculated.MapStatus;
            entity.BloodPressureLocation = NormalizeNullableText(request.BloodPressureLocation);
            entity.PulseRate = request.PulseRate;
            entity.IsPulseReadable = request.IsPulseReadable;
            entity.IsPulseRegular = request.IsPulseRegular;
            entity.PulseRhythmNote = NormalizeNullableText(request.PulseRhythmNote);
            entity.RespiratoryRate = request.RespiratoryRate;
            entity.Temperature = request.Temperature;
            entity.TemperatureRoute = NormalizeNullableText(request.TemperatureRoute);
            entity.OxygenSaturation = request.OxygenSaturation;
            entity.IsUsingOxygen = request.IsUsingOxygen;
            entity.OxygenSupportType = request.OxygenSupportType;
            entity.OxygenFlowRate = request.OxygenFlowRate;
            entity.OxygenSupportNote = NormalizeNullableText(request.OxygenSupportNote);
            entity.Weight = request.Weight;
            entity.Height = request.Height;
            entity.BMI = calculated.BMI;
            entity.WeightMeasurementNote = NormalizeNullableText(request.WeightMeasurementNote);
            entity.ConsciousnessStatus = request.ConsciousnessStatus;
            entity.GcsEye = request.GcsEye;
            entity.GcsVerbal = request.GcsVerbal;
            entity.GcsMotor = request.GcsMotor;
            entity.GcsTotal = calculated.GcsTotal;
            entity.NeurologicalNote = NormalizeNullableText(request.NeurologicalNote);
            entity.HasPain = request.HasPain;
            entity.PainScale = request.PainScale;
            entity.PainLocation = NormalizeNullableText(request.PainLocation);
            entity.PainNote = NormalizeNullableText(request.PainNote);
            entity.EarlyWarningScore = calculated.EarlyWarningScore;
            entity.EwsRiskLevel = calculated.EwsRiskLevel;
            entity.EwsMonitoringRecommendation = calculated.EwsMonitoringRecommendation;
            entity.IsAbnormal = calculated.IsAbnormal;
            entity.IsCritical = calculated.IsCritical;
            entity.NeedDoctorNotification = request.NeedDoctorNotification || calculated.IsCritical || calculated.EwsRiskLevel == EwsRiskLevel.High || calculated.EwsRiskLevel == EwsRiskLevel.Critical;
            entity.ClinicalNote = NormalizeNullableText(request.ClinicalNote);
            entity.Notes = NormalizeNullableText(request.Notes);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            NormalizeVitalSignData(entity);

            await _dbContext.SaveChangesAsync();

            var response = ToUpdateResponse(entity);

            return Ok(ApiResponse<PatientVitalSignUpdateResponse>.Ok(
                response,
                "Tanda vital pasien berhasil diubah."
            ));
        }

        [HttpPatch("{id:guid}/verify")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Verify Patient Vital Sign", Description = "Verifikasi tanda vital pasien", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("PatientVitalSign", "Update")]
        public async Task<IActionResult> VerifyVitalSign(Guid id, [FromBody] VerifyPatientVitalSignRequest request)
        {
            var entity = await _dbContext.Set<TrxPatientVitalSign>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Tanda vital pasien tidak ditemukan."
                ));
            }

            if (entity.VitalSignStatus == PatientVitalSignStatus.Cancelled ||
                entity.VitalSignStatus == PatientVitalSignStatus.EnteredInError)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Tanda vital yang sudah cancelled atau entered in error tidak dapat diverifikasi."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsVerified = true;
            entity.VerifiedAt = now;
            entity.VerifiedByUserId = actorUserId;
            entity.VitalSignStatus = PatientVitalSignStatus.Verified;

            if (!string.IsNullOrWhiteSpace(request.ClinicalNote))
                entity.ClinicalNote = request.ClinicalNote.Trim();

            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Tanda vital pasien berhasil diverifikasi."
            ));
        }

        [HttpPatch("{id:guid}/notify-doctor")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Notify Doctor Patient Vital Sign", Description = "Menandai dokter sudah diberi notifikasi tanda vital", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("PatientVitalSign", "Update")]
        public async Task<IActionResult> NotifyDoctor(Guid id, [FromBody] NotifyDoctorPatientVitalSignRequest request)
        {
            var entity = await _dbContext.Set<TrxPatientVitalSign>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Tanda vital pasien tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.NeedDoctorNotification = false;
            entity.DoctorNotifiedAt = now;
            entity.DoctorNotifiedByUserId = actorUserId;
            entity.DoctorNotificationNote = NormalizeNullableText(request.DoctorNotificationNote);
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Notifikasi dokter untuk tanda vital pasien berhasil dicatat."
            ));
        }

        [HttpPatch("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Cancel Patient Vital Sign", Description = "Membatalkan catatan tanda vital pasien", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("PatientVitalSign", "Update")]
        public async Task<IActionResult> CancelVitalSign(Guid id, [FromBody] CancelPatientVitalSignRequest request)
        {
            var entity = await _dbContext.Set<TrxPatientVitalSign>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Tanda vital pasien tidak ditemukan."
                ));
            }

            if (entity.VitalSignStatus == PatientVitalSignStatus.Cancelled)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Tanda vital pasien sudah cancelled."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.VitalSignStatus = PatientVitalSignStatus.Cancelled;
            entity.IsActive = false;
            entity.NeedDoctorNotification = false;
            entity.CancelledAt = now;
            entity.CancelledByUserId = actorUserId;
            entity.CancelReason = request.CancelReason.Trim();
            entity.IsCancel = true;
            entity.CancelDateTime = now;
            entity.CancelBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Tanda vital pasien berhasil dibatalkan."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Patient Vital Sign", Description = "Menghapus data tanda vital pasien", AccessType = AccessTypes.Delete, SortOrder = 7)]
        [AccessPermission("PatientVitalSign", "Delete")]
        public async Task<IActionResult> DeleteVitalSign(Guid id)
        {
            var entity = await _dbContext.Set<TrxPatientVitalSign>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Tanda vital pasien tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsDelete = true;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.IsActive = false;
            entity.NeedDoctorNotification = false;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Tanda vital pasien berhasil dihapus."
            ));
        }

        private IQueryable<TrxPatientVitalSign> BuildBaseQuery()
        {
            return _dbContext.Set<TrxPatientVitalSign>()
                .Include(x => x.Patient)
                .Include(x => x.Encounter)
                .Include(x => x.Queue)
                .Include(x => x.Assessment)
                .Include(x => x.Consultation)
                .Include(x => x.Doctor)
                .Include(x => x.ServiceUnit)
                .Include(x => x.Clinic)
                .Include(x => x.ObservedByUser)
                .Include(x => x.DoctorNotifiedByUser)
                .Include(x => x.VerifiedByUser)
                .Include(x => x.CancelledByUser)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<TrxPatientVitalSign> ApplyFilters(
            IQueryable<TrxPatientVitalSign> query,
            string? search,
            Guid? patientId,
            Guid? encounterId,
            Guid? queueId,
            Guid? assessmentId,
            Guid? consultationId,
            Guid? doctorId,
            Guid? serviceUnitId,
            Guid? clinicId,
            PatientVitalSignSource? vitalSignSource,
            PatientVitalSignStatus? vitalSignStatus,
            PatientPosition? patientPosition,
            ConsciousnessStatus? consciousnessStatus,
            MapStatus? mapStatus,
            EwsRiskLevel? ewsRiskLevel,
            bool? isUsingOxygen,
            bool? hasPain,
            bool? isAbnormal,
            bool? isCritical,
            bool? needDoctorNotification,
            bool? isVerified,
            bool? isActive,
            DateTime? startDate,
            DateTime? endDate)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.VitalSignRecordNumber.ToLower().Contains(keyword) ||
                    (x.Patient != null && x.Patient.FullName.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.MedicalRecordNumber.ToLower().Contains(keyword)) ||
                    (x.Encounter != null && x.Encounter.EncounterNumber.ToLower().Contains(keyword)) ||
                    (x.Queue != null && x.Queue.QueueCode.ToLower().Contains(keyword)) ||
                    (x.ObservationLocation != null && x.ObservationLocation.ToLower().Contains(keyword)) ||
                    (x.MeasurementMethod != null && x.MeasurementMethod.ToLower().Contains(keyword)) ||
                    (x.ClinicalNote != null && x.ClinicalNote.ToLower().Contains(keyword)));
            }

            if (patientId.HasValue && patientId.Value != Guid.Empty) query = query.Where(x => x.PatientId == patientId.Value);
            if (encounterId.HasValue && encounterId.Value != Guid.Empty) query = query.Where(x => x.EncounterId == encounterId.Value);
            if (queueId.HasValue && queueId.Value != Guid.Empty) query = query.Where(x => x.QueueId == queueId.Value);
            if (assessmentId.HasValue && assessmentId.Value != Guid.Empty) query = query.Where(x => x.AssessmentId == assessmentId.Value);
            if (consultationId.HasValue && consultationId.Value != Guid.Empty) query = query.Where(x => x.ConsultationId == consultationId.Value);
            if (doctorId.HasValue && doctorId.Value != Guid.Empty) query = query.Where(x => x.DoctorId == doctorId.Value);
            if (serviceUnitId.HasValue && serviceUnitId.Value != Guid.Empty) query = query.Where(x => x.ServiceUnitId == serviceUnitId.Value);
            if (clinicId.HasValue && clinicId.Value != Guid.Empty) query = query.Where(x => x.ClinicId == clinicId.Value);
            if (vitalSignSource.HasValue) query = query.Where(x => x.VitalSignSource == vitalSignSource.Value);
            if (vitalSignStatus.HasValue) query = query.Where(x => x.VitalSignStatus == vitalSignStatus.Value);
            if (patientPosition.HasValue) query = query.Where(x => x.PatientPosition == patientPosition.Value);
            if (consciousnessStatus.HasValue) query = query.Where(x => x.ConsciousnessStatus == consciousnessStatus.Value);
            if (mapStatus.HasValue) query = query.Where(x => x.MapStatus == mapStatus.Value);
            if (ewsRiskLevel.HasValue) query = query.Where(x => x.EwsRiskLevel == ewsRiskLevel.Value);
            if (isUsingOxygen.HasValue) query = query.Where(x => x.IsUsingOxygen == isUsingOxygen.Value);
            if (hasPain.HasValue) query = query.Where(x => x.HasPain == hasPain.Value);
            if (isAbnormal.HasValue) query = query.Where(x => x.IsAbnormal == isAbnormal.Value);
            if (isCritical.HasValue) query = query.Where(x => x.IsCritical == isCritical.Value);
            if (needDoctorNotification.HasValue) query = query.Where(x => x.NeedDoctorNotification == needDoctorNotification.Value);
            if (isVerified.HasValue) query = query.Where(x => x.IsVerified == isVerified.Value);
            if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);
            if (startDate.HasValue) query = query.Where(x => x.ObservationDateTime >= startDate.Value.Date);
            if (endDate.HasValue) query = query.Where(x => x.ObservationDateTime < endDate.Value.Date.AddDays(1));

            return query;
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateCreateRequestAsync(CreatePatientVitalSignRequest request)
        {
            if (request.PatientId == Guid.Empty)
                return (false, "PatientId wajib diisi.");

            var patientExists = await _dbContext.Set<MstPatient>()
                .AnyAsync(x => x.Id == request.PatientId && !x.IsDelete);

            if (!patientExists)
                return (false, "Pasien tidak ditemukan.");

            return ValidateMeasurementValues(request);
        }

        private static (bool IsValid, string? ErrorMessage) ValidateUpdateRequest(UpdatePatientVitalSignRequest request)
        {
            return ValidateMeasurementValues(request);
        }

        private static (bool IsValid, string? ErrorMessage) ValidateMeasurementValues(CreatePatientVitalSignRequest request)
        {
            return ValidateMeasurementValuesCore(
                request.BloodPressureSystolic,
                request.BloodPressureDiastolic,
                request.PulseRate,
                request.RespiratoryRate,
                request.Temperature,
                request.OxygenSaturation,
                request.OxygenFlowRate,
                request.Weight,
                request.Height,
                request.GcsEye,
                request.GcsVerbal,
                request.GcsMotor,
                request.HasPain,
                request.PainScale);
        }

        private static (bool IsValid, string? ErrorMessage) ValidateMeasurementValues(UpdatePatientVitalSignRequest request)
        {
            return ValidateMeasurementValuesCore(
                request.BloodPressureSystolic,
                request.BloodPressureDiastolic,
                request.PulseRate,
                request.RespiratoryRate,
                request.Temperature,
                request.OxygenSaturation,
                request.OxygenFlowRate,
                request.Weight,
                request.Height,
                request.GcsEye,
                request.GcsVerbal,
                request.GcsMotor,
                request.HasPain,
                request.PainScale);
        }

        private static (bool IsValid, string? ErrorMessage) ValidateMeasurementValuesCore(
            int? systolic,
            int? diastolic,
            int? pulseRate,
            int? respiratoryRate,
            decimal? temperature,
            decimal? oxygenSaturation,
            decimal? oxygenFlowRate,
            decimal? weight,
            decimal? height,
            int? gcsEye,
            int? gcsVerbal,
            int? gcsMotor,
            bool hasPain,
            int? painScale)
        {
            if (systolic.HasValue && (systolic.Value < 40 || systolic.Value > 300))
                return (false, "Tekanan darah sistolik harus berada pada rentang 40-300 mmHg.");

            if (diastolic.HasValue && (diastolic.Value < 20 || diastolic.Value > 200))
                return (false, "Tekanan darah diastolik harus berada pada rentang 20-200 mmHg.");

            if (systolic.HasValue && diastolic.HasValue && systolic.Value <= diastolic.Value)
                return (false, "Tekanan darah sistolik harus lebih besar dari diastolik.");

            if (pulseRate.HasValue && (pulseRate.Value < 20 || pulseRate.Value > 250))
                return (false, "Nadi harus berada pada rentang 20-250 kali/menit.");

            if (respiratoryRate.HasValue && (respiratoryRate.Value < 5 || respiratoryRate.Value > 80))
                return (false, "Respiratory rate harus berada pada rentang 5-80 kali/menit.");

            if (temperature.HasValue && (temperature.Value < 25 || temperature.Value > 45))
                return (false, "Suhu tubuh harus berada pada rentang 25-45 derajat Celcius.");

            if (oxygenSaturation.HasValue && (oxygenSaturation.Value < 0 || oxygenSaturation.Value > 100))
                return (false, "Saturasi oksigen harus berada pada rentang 0-100 persen.");

            if (oxygenFlowRate.HasValue && oxygenFlowRate.Value < 0)
                return (false, "Oxygen flow rate tidak boleh negatif.");

            if (weight.HasValue && weight.Value <= 0)
                return (false, "Berat badan harus lebih dari 0.");

            if (height.HasValue && height.Value <= 0)
                return (false, "Tinggi badan harus lebih dari 0.");

            if (gcsEye.HasValue && (gcsEye.Value < 1 || gcsEye.Value > 4))
                return (false, "GCS Eye harus berada pada rentang 1-4.");

            if (gcsVerbal.HasValue && (gcsVerbal.Value < 1 || gcsVerbal.Value > 5))
                return (false, "GCS Verbal harus berada pada rentang 1-5.");

            if (gcsMotor.HasValue && (gcsMotor.Value < 1 || gcsMotor.Value > 6))
                return (false, "GCS Motor harus berada pada rentang 1-6.");

            if (hasPain && (!painScale.HasValue || painScale.Value < 0 || painScale.Value > 10))
                return (false, "Skala nyeri wajib diisi pada rentang 0-10 jika pasien memiliki nyeri.");

            if (!hasPain && painScale.HasValue && (painScale.Value < 0 || painScale.Value > 10))
                return (false, "Skala nyeri harus berada pada rentang 0-10.");

            return (true, null);
        }

        private async Task<ClinicalContextResult> ResolveClinicalContextAsync(
            Guid patientId,
            Guid? encounterId,
            Guid? queueId,
            Guid? assessmentId,
            Guid? consultationId,
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
                DoctorId = NormalizeNullableGuid(doctorId),
                ServiceUnitId = NormalizeNullableGuid(serviceUnitId),
                ClinicId = NormalizeNullableGuid(clinicId)
            };

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

                return result.Ok();
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

                return result.Ok();
            }

            if (result.QueueId.HasValue)
            {
                var queue = await _dbContext.Set<TrxQueue>()
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

        private async Task<string> GenerateVitalSignRecordNumberAsync(DateTime now)
        {
            var prefix = $"VTS-{now:yyyyMMdd}";

            var countToday = await _dbContext.Set<TrxPatientVitalSign>()
                .CountAsync(x => x.VitalSignRecordNumber.StartsWith(prefix));

            return $"{prefix}-{countToday + 1:0000}";
        }

        private static CalculatedVitalSignValue CalculateVitalSignValues(CreatePatientVitalSignRequest request)
        {
            return CalculateVitalSignValuesCore(
                request.Weight,
                request.Height,
                request.BloodPressureSystolic,
                request.BloodPressureDiastolic,
                request.RespiratoryRate,
                request.OxygenSaturation,
                request.Temperature,
                request.PulseRate,
                request.ConsciousnessStatus,
                request.GcsEye,
                request.GcsVerbal,
                request.GcsMotor);
        }

        private static CalculatedVitalSignValue CalculateVitalSignValues(UpdatePatientVitalSignRequest request)
        {
            return CalculateVitalSignValuesCore(
                request.Weight,
                request.Height,
                request.BloodPressureSystolic,
                request.BloodPressureDiastolic,
                request.RespiratoryRate,
                request.OxygenSaturation,
                request.Temperature,
                request.PulseRate,
                request.ConsciousnessStatus,
                request.GcsEye,
                request.GcsVerbal,
                request.GcsMotor);
        }

        private static CalculatedVitalSignValue CalculateVitalSignValuesCore(
            decimal? weight,
            decimal? height,
            int? systolic,
            int? diastolic,
            int? respiratoryRate,
            decimal? oxygenSaturation,
            decimal? temperature,
            int? pulseRate,
            ConsciousnessStatus consciousnessStatus,
            int? gcsEye,
            int? gcsVerbal,
            int? gcsMotor)
        {
            var bmi = CalculateBmi(weight, height);
            var map = CalculateMap(systolic, diastolic);
            var mapStatus = CalculateMapStatus(map);
            var gcsTotal = CalculateGcsTotal(gcsEye, gcsVerbal, gcsMotor);
            var ewsScore = CalculateEwsScore(respiratoryRate, oxygenSaturation, temperature, systolic, pulseRate, consciousnessStatus);
            var ewsRiskLevel = CalculateEwsRiskLevel(ewsScore);
            var ewsMonitoringRecommendation = GetEwsMonitoringRecommendation(ewsRiskLevel, ewsScore);
            var isCritical = CalculateIsCritical(systolic, diastolic, pulseRate, respiratoryRate, temperature, oxygenSaturation, gcsTotal, ewsRiskLevel);
            var isAbnormal = isCritical || CalculateIsAbnormal(systolic, diastolic, pulseRate, respiratoryRate, temperature, oxygenSaturation, gcsTotal, mapStatus, ewsRiskLevel);

            return new CalculatedVitalSignValue
            {
                BMI = bmi,
                MeanArterialPressure = map,
                MapStatus = mapStatus,
                GcsTotal = gcsTotal,
                EarlyWarningScore = ewsScore,
                EwsRiskLevel = ewsRiskLevel,
                EwsMonitoringRecommendation = ewsMonitoringRecommendation,
                IsAbnormal = isAbnormal,
                IsCritical = isCritical
            };
        }

        private static decimal? CalculateBmi(decimal? weightKg, decimal? heightCm)
        {
            if (!weightKg.HasValue || !heightCm.HasValue || heightCm.Value <= 0)
                return null;

            var heightMeter = heightCm.Value / 100;
            var bmi = weightKg.Value / (heightMeter * heightMeter);

            return Math.Round(bmi, 2);
        }

        private static decimal? CalculateMap(int? systolic, int? diastolic)
        {
            if (!systolic.HasValue || !diastolic.HasValue)
                return null;

            var map = diastolic.Value + ((systolic.Value - diastolic.Value) / 3m);

            return Math.Round(map, 2);
        }

        private static MapStatus CalculateMapStatus(decimal? map)
        {
            if (!map.HasValue)
                return MapStatus.Unknown;

            if (map.Value < 60)
                return MapStatus.Hypotension;

            if (map.Value > 100)
                return MapStatus.Hypertension;

            return MapStatus.Normal;
        }

        private static int? CalculateGcsTotal(int? eye, int? verbal, int? motor)
        {
            if (!eye.HasValue && !verbal.HasValue && !motor.HasValue)
                return null;

            if (!eye.HasValue || !verbal.HasValue || !motor.HasValue)
                return null;

            return eye.Value + verbal.Value + motor.Value;
        }

        private static int? CalculateEwsScore(
            int? respiratoryRate,
            decimal? oxygenSaturation,
            decimal? temperature,
            int? systolicBloodPressure,
            int? pulseRate,
            ConsciousnessStatus consciousnessStatus)
        {
            var hasAnyValue =
                respiratoryRate.HasValue ||
                oxygenSaturation.HasValue ||
                temperature.HasValue ||
                systolicBloodPressure.HasValue ||
                pulseRate.HasValue ||
                consciousnessStatus != ConsciousnessStatus.Unknown;

            if (!hasAnyValue)
                return null;

            var score = 0;

            if (respiratoryRate.HasValue)
            {
                if (respiratoryRate.Value <= 8) score += 3;
                else if (respiratoryRate.Value <= 11) score += 1;
                else if (respiratoryRate.Value <= 20) score += 0;
                else if (respiratoryRate.Value <= 24) score += 2;
                else score += 3;
            }

            if (oxygenSaturation.HasValue)
            {
                if (oxygenSaturation.Value <= 91) score += 3;
                else if (oxygenSaturation.Value <= 93) score += 2;
                else if (oxygenSaturation.Value <= 95) score += 1;
                else score += 0;
            }

            if (temperature.HasValue)
            {
                if (temperature.Value <= 35.0m) score += 3;
                else if (temperature.Value <= 36.0m) score += 1;
                else if (temperature.Value <= 38.0m) score += 0;
                else if (temperature.Value <= 39.0m) score += 1;
                else score += 2;
            }

            if (systolicBloodPressure.HasValue)
            {
                if (systolicBloodPressure.Value <= 90) score += 3;
                else if (systolicBloodPressure.Value <= 100) score += 2;
                else if (systolicBloodPressure.Value <= 110) score += 1;
                else if (systolicBloodPressure.Value <= 219) score += 0;
                else score += 3;
            }

            if (pulseRate.HasValue)
            {
                if (pulseRate.Value <= 40) score += 3;
                else if (pulseRate.Value <= 50) score += 1;
                else if (pulseRate.Value <= 90) score += 0;
                else if (pulseRate.Value <= 110) score += 1;
                else if (pulseRate.Value <= 130) score += 2;
                else score += 3;
            }

            if (consciousnessStatus != ConsciousnessStatus.Unknown &&
                consciousnessStatus != ConsciousnessStatus.ComposMentis)
            {
                score += 3;
            }

            return score;
        }

        private static EwsRiskLevel CalculateEwsRiskLevel(int? ewsScore)
        {
            if (!ewsScore.HasValue)
                return EwsRiskLevel.Unknown;

            if (ewsScore.Value >= 7)
                return EwsRiskLevel.Critical;

            if (ewsScore.Value >= 5)
                return EwsRiskLevel.High;

            if (ewsScore.Value >= 3)
                return EwsRiskLevel.Medium;

            return EwsRiskLevel.Low;
        }

        private static string? GetEwsMonitoringRecommendation(EwsRiskLevel riskLevel, int? ewsScore)
        {
            if (!ewsScore.HasValue)
                return null;

            return riskLevel switch
            {
                EwsRiskLevel.Low => "Monitoring rutin sesuai kondisi klinis pasien.",
                EwsRiskLevel.Medium => "Monitoring ulang tanda vital dan evaluasi klinis berkala.",
                EwsRiskLevel.High => "Monitoring lebih sering dan informasikan dokter penanggung jawab.",
                EwsRiskLevel.Critical => "Pemantauan terus menerus tanda-tanda vital, pertimbangkan eskalasi klinis segera.",
                _ => null
            };
        }

        private static bool CalculateIsCritical(
            int? systolic,
            int? diastolic,
            int? pulseRate,
            int? respiratoryRate,
            decimal? temperature,
            decimal? oxygenSaturation,
            int? gcsTotal,
            EwsRiskLevel ewsRiskLevel)
        {
            return
                ewsRiskLevel == EwsRiskLevel.Critical ||
                (systolic.HasValue && (systolic.Value <= 90 || systolic.Value >= 220)) ||
                (diastolic.HasValue && diastolic.Value >= 120) ||
                (pulseRate.HasValue && (pulseRate.Value <= 40 || pulseRate.Value >= 131)) ||
                (respiratoryRate.HasValue && (respiratoryRate.Value <= 8 || respiratoryRate.Value >= 25)) ||
                (temperature.HasValue && (temperature.Value <= 35 || temperature.Value >= 40)) ||
                (oxygenSaturation.HasValue && oxygenSaturation.Value <= 91) ||
                (gcsTotal.HasValue && gcsTotal.Value <= 8);
        }

        private static bool CalculateIsAbnormal(
            int? systolic,
            int? diastolic,
            int? pulseRate,
            int? respiratoryRate,
            decimal? temperature,
            decimal? oxygenSaturation,
            int? gcsTotal,
            MapStatus mapStatus,
            EwsRiskLevel ewsRiskLevel)
        {
            return
                ewsRiskLevel == EwsRiskLevel.Medium ||
                ewsRiskLevel == EwsRiskLevel.High ||
                mapStatus == MapStatus.Hypotension ||
                mapStatus == MapStatus.Hypertension ||
                (systolic.HasValue && (systolic.Value < 100 || systolic.Value > 180)) ||
                (diastolic.HasValue && (diastolic.Value < 60 || diastolic.Value > 110)) ||
                (pulseRate.HasValue && (pulseRate.Value < 50 || pulseRate.Value > 110)) ||
                (respiratoryRate.HasValue && (respiratoryRate.Value < 12 || respiratoryRate.Value > 24)) ||
                (temperature.HasValue && (temperature.Value < 36 || temperature.Value > 38)) ||
                (oxygenSaturation.HasValue && oxygenSaturation.Value < 95) ||
                (gcsTotal.HasValue && gcsTotal.Value < 15);
        }

        private static void NormalizeVitalSignData(TrxPatientVitalSign entity)
        {
            if (!entity.IsActive ||
                entity.VitalSignStatus == PatientVitalSignStatus.Cancelled ||
                entity.VitalSignStatus == PatientVitalSignStatus.EnteredInError)
            {
                entity.NeedDoctorNotification = false;
            }

            if (!entity.HasPain)
            {
                entity.PainScale = null;
                entity.PainLocation = null;
                entity.PainNote = null;
            }

            if (!entity.IsUsingOxygen)
            {
                entity.OxygenSupportType = OxygenSupportType.None;
                entity.OxygenFlowRate = null;
                entity.OxygenSupportNote = null;
            }
        }

        private static IQueryable<TrxPatientVitalSign> ApplySorting(
            IQueryable<TrxPatientVitalSign> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "observationDateTime").ToLowerInvariant() switch
            {
                "vitalsignrecordnumber" => isDesc ? query.OrderByDescending(x => x.VitalSignRecordNumber) : query.OrderBy(x => x.VitalSignRecordNumber),
                "vitalsignsource" => isDesc ? query.OrderByDescending(x => x.VitalSignSource) : query.OrderBy(x => x.VitalSignSource),
                "vitalsignstatus" => isDesc ? query.OrderByDescending(x => x.VitalSignStatus) : query.OrderBy(x => x.VitalSignStatus),
                "bloodpressuresystolic" => isDesc ? query.OrderByDescending(x => x.BloodPressureSystolic) : query.OrderBy(x => x.BloodPressureSystolic),
                "pulserate" => isDesc ? query.OrderByDescending(x => x.PulseRate) : query.OrderBy(x => x.PulseRate),
                "temperature" => isDesc ? query.OrderByDescending(x => x.Temperature) : query.OrderBy(x => x.Temperature),
                "oxygensaturation" => isDesc ? query.OrderByDescending(x => x.OxygenSaturation) : query.OrderBy(x => x.OxygenSaturation),
                "earlywarningscore" => isDesc ? query.OrderByDescending(x => x.EarlyWarningScore) : query.OrderBy(x => x.EarlyWarningScore),
                "ewsrisklevel" => isDesc ? query.OrderByDescending(x => x.EwsRiskLevel) : query.OrderBy(x => x.EwsRiskLevel),
                "isabnormal" => isDesc ? query.OrderByDescending(x => x.IsAbnormal) : query.OrderBy(x => x.IsAbnormal),
                "iscritical" => isDesc ? query.OrderByDescending(x => x.IsCritical) : query.OrderBy(x => x.IsCritical),
                "isverified" => isDesc ? query.OrderByDescending(x => x.IsVerified) : query.OrderBy(x => x.IsVerified),
                "createdatetime" => isDesc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                _ => isDesc
                    ? query.OrderByDescending(x => x.ObservationDateTime).ThenByDescending(x => x.IsCritical).ThenByDescending(x => x.IsAbnormal)
                    : query.OrderBy(x => x.ObservationDateTime).ThenByDescending(x => x.IsCritical).ThenByDescending(x => x.IsAbnormal)
            };
        }

        private static PatientVitalSignResponse ToResponse(TrxPatientVitalSign x)
        {
            return new PatientVitalSignResponse
            {
                Id = x.Id,
                VitalSignRecordNumber = x.VitalSignRecordNumber,
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
                DoctorId = x.DoctorId,
                DoctorName = x.Doctor != null ? x.Doctor.FullName : null,
                ServiceUnitId = x.ServiceUnitId,
                ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null,
                ClinicId = x.ClinicId,
                ClinicName = x.Clinic != null ? x.Clinic.ClinicName : null,
                ObservationDateTime = x.ObservationDateTime,
                VitalSignSource = x.VitalSignSource,
                VitalSignStatus = x.VitalSignStatus,
                ObservedByUserId = x.ObservedByUserId,
                ObservedByUserName = x.ObservedByUser != null ? x.ObservedByUser.DisplayName : null,
                ObservationLocation = x.ObservationLocation,
                PatientPosition = x.PatientPosition,
                MeasurementMethod = x.MeasurementMethod,
                BloodPressureSystolic = x.BloodPressureSystolic,
                BloodPressureDiastolic = x.BloodPressureDiastolic,
                MeanArterialPressure = x.MeanArterialPressure,
                MapStatus = x.MapStatus,
                PulseRate = x.PulseRate,
                IsPulseReadable = x.IsPulseReadable,
                IsPulseRegular = x.IsPulseRegular,
                RespiratoryRate = x.RespiratoryRate,
                Temperature = x.Temperature,
                OxygenSaturation = x.OxygenSaturation,
                IsUsingOxygen = x.IsUsingOxygen,
                OxygenSupportType = x.OxygenSupportType,
                OxygenFlowRate = x.OxygenFlowRate,
                Weight = x.Weight,
                Height = x.Height,
                BMI = x.BMI,
                ConsciousnessStatus = x.ConsciousnessStatus,
                GcsTotal = x.GcsTotal,
                HasPain = x.HasPain,
                PainScale = x.PainScale,
                EarlyWarningScore = x.EarlyWarningScore,
                EwsRiskLevel = x.EwsRiskLevel,
                IsAbnormal = x.IsAbnormal,
                IsCritical = x.IsCritical,
                NeedDoctorNotification = x.NeedDoctorNotification,
                DoctorNotifiedAt = x.DoctorNotifiedAt,
                IsVerified = x.IsVerified,
                VerifiedAt = x.VerifiedAt,
                VerifiedByUserId = x.VerifiedByUserId,
                VerifiedByUserName = x.VerifiedByUser != null ? x.VerifiedByUser.DisplayName : null,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime
            };
        }

        private static PatientVitalSignDetailResponse ToDetailResponse(TrxPatientVitalSign x)
        {
            return new PatientVitalSignDetailResponse
            {
                Id = x.Id,
                VitalSignRecordNumber = x.VitalSignRecordNumber,
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
                DoctorId = x.DoctorId,
                DoctorName = x.Doctor != null ? x.Doctor.FullName : null,
                ServiceUnitId = x.ServiceUnitId,
                ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null,
                ClinicId = x.ClinicId,
                ClinicName = x.Clinic != null ? x.Clinic.ClinicName : null,
                ObservationDateTime = x.ObservationDateTime,
                VitalSignSource = x.VitalSignSource,
                VitalSignStatus = x.VitalSignStatus,
                ObservedByUserId = x.ObservedByUserId,
                ObservedByUserName = x.ObservedByUser != null ? x.ObservedByUser.DisplayName : null,
                ObservationLocation = x.ObservationLocation,
                PatientPosition = x.PatientPosition,
                MeasurementMethod = x.MeasurementMethod,
                DeviceName = x.DeviceName,
                DeviceSerialNumber = x.DeviceSerialNumber,
                BloodPressureSystolic = x.BloodPressureSystolic,
                BloodPressureDiastolic = x.BloodPressureDiastolic,
                MeanArterialPressure = x.MeanArterialPressure,
                MapStatus = x.MapStatus,
                BloodPressureLocation = x.BloodPressureLocation,
                PulseRate = x.PulseRate,
                IsPulseReadable = x.IsPulseReadable,
                IsPulseRegular = x.IsPulseRegular,
                PulseRhythmNote = x.PulseRhythmNote,
                RespiratoryRate = x.RespiratoryRate,
                Temperature = x.Temperature,
                TemperatureRoute = x.TemperatureRoute,
                OxygenSaturation = x.OxygenSaturation,
                IsUsingOxygen = x.IsUsingOxygen,
                OxygenSupportType = x.OxygenSupportType,
                OxygenFlowRate = x.OxygenFlowRate,
                OxygenSupportNote = x.OxygenSupportNote,
                Weight = x.Weight,
                Height = x.Height,
                BMI = x.BMI,
                WeightMeasurementNote = x.WeightMeasurementNote,
                ConsciousnessStatus = x.ConsciousnessStatus,
                GcsEye = x.GcsEye,
                GcsVerbal = x.GcsVerbal,
                GcsMotor = x.GcsMotor,
                GcsTotal = x.GcsTotal,
                NeurologicalNote = x.NeurologicalNote,
                HasPain = x.HasPain,
                PainScale = x.PainScale,
                PainLocation = x.PainLocation,
                PainNote = x.PainNote,
                EarlyWarningScore = x.EarlyWarningScore,
                EwsRiskLevel = x.EwsRiskLevel,
                EwsMonitoringRecommendation = x.EwsMonitoringRecommendation,
                IsAbnormal = x.IsAbnormal,
                IsCritical = x.IsCritical,
                NeedDoctorNotification = x.NeedDoctorNotification,
                DoctorNotifiedAt = x.DoctorNotifiedAt,
                DoctorNotifiedByUserId = x.DoctorNotifiedByUserId,
                DoctorNotifiedByUserName = x.DoctorNotifiedByUser != null ? x.DoctorNotifiedByUser.DisplayName : null,
                DoctorNotificationNote = x.DoctorNotificationNote,
                IsVerified = x.IsVerified,
                VerifiedAt = x.VerifiedAt,
                VerifiedByUserId = x.VerifiedByUserId,
                VerifiedByUserName = x.VerifiedByUser != null ? x.VerifiedByUser.DisplayName : null,
                ClinicalNote = x.ClinicalNote,
                Notes = x.Notes,
                CancelledAt = x.CancelledAt,
                CancelledByUserId = x.CancelledByUserId,
                CancelledByUserName = x.CancelledByUser != null ? x.CancelledByUser.DisplayName : null,
                CancelReason = x.CancelReason,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime
            };
        }

        private static PatientVitalSignCreateResponse ToCreateUpdateResponse(TrxPatientVitalSign x)
        {
            return new PatientVitalSignCreateResponse
            {
                Id = x.Id,
                VitalSignRecordNumber = x.VitalSignRecordNumber,
                PatientId = x.PatientId,
                EncounterId = x.EncounterId,
                QueueId = x.QueueId,
                AssessmentId = x.AssessmentId,
                ConsultationId = x.ConsultationId,
                ObservationDateTime = x.ObservationDateTime,
                VitalSignSource = x.VitalSignSource,
                VitalSignStatus = x.VitalSignStatus,
                BMI = x.BMI,
                MeanArterialPressure = x.MeanArterialPressure,
                MapStatus = x.MapStatus,
                GcsTotal = x.GcsTotal,
                EarlyWarningScore = x.EarlyWarningScore,
                EwsRiskLevel = x.EwsRiskLevel,
                IsAbnormal = x.IsAbnormal,
                IsCritical = x.IsCritical,
                NeedDoctorNotification = x.NeedDoctorNotification,
                IsVerified = x.IsVerified
            };
        }

        private static PatientVitalSignUpdateResponse ToUpdateResponse(TrxPatientVitalSign x)
        {
            return new PatientVitalSignUpdateResponse
            {
                Id = x.Id,
                VitalSignRecordNumber = x.VitalSignRecordNumber,
                PatientId = x.PatientId,
                EncounterId = x.EncounterId,
                QueueId = x.QueueId,
                AssessmentId = x.AssessmentId,
                ConsultationId = x.ConsultationId,
                ObservationDateTime = x.ObservationDateTime,
                VitalSignSource = x.VitalSignSource,
                VitalSignStatus = x.VitalSignStatus,
                BMI = x.BMI,
                MeanArterialPressure = x.MeanArterialPressure,
                MapStatus = x.MapStatus,
                GcsTotal = x.GcsTotal,
                EarlyWarningScore = x.EarlyWarningScore,
                EwsRiskLevel = x.EwsRiskLevel,
                IsAbnormal = x.IsAbnormal,
                IsCritical = x.IsCritical,
                NeedDoctorNotification = x.NeedDoctorNotification,
                IsVerified = x.IsVerified
            };
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            return (pageNumber, pageSize);
        }

        private static List<PatientVitalSignEnumOptionResponse> BuildEnumOptions<TEnum>() where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Select(x => new PatientVitalSignEnumOptionResponse
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

        private class ClinicalContextResult
        {
            public bool IsValid { get; set; }
            public string? ErrorMessage { get; set; }
            public Guid? EncounterId { get; set; }
            public Guid? QueueId { get; set; }
            public Guid? AssessmentId { get; set; }
            public Guid? ConsultationId { get; set; }
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

        private class CalculatedVitalSignValue
        {
            public decimal? BMI { get; set; }
            public decimal? MeanArterialPressure { get; set; }
            public MapStatus MapStatus { get; set; }
            public int? GcsTotal { get; set; }
            public int? EarlyWarningScore { get; set; }
            public EwsRiskLevel EwsRiskLevel { get; set; }
            public string? EwsMonitoringRecommendation { get; set; }
            public bool IsAbnormal { get; set; }
            public bool IsCritical { get; set; }
        }
    }
}
