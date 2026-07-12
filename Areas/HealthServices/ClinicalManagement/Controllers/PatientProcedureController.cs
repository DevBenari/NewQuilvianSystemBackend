using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponsePatientProcedurePagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs.PatientProcedureResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/clinical-management/patient-procedures")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_CLINICAL",
        moduleName: "Health Service Clinical",
        displayName: "Patient Procedure",
        AreaName = "HealthServices",
        ControllerName = "PatientProcedure",
        Description = "Tindakan pasien berdasarkan konsultasi dokter",
        SortOrder = 4
    )]
    [Tags("Health Services / Clinical Management / Patient Procedure")]
    public class PatientProcedureController : ControllerBase
    {
        private const string LogCategory = "HealthServices.Clinical";

        private readonly ApplicationDbContext _dbContext;
        private readonly EncounterInsuranceService _encounterInsuranceService;
        private readonly InsuranceCoverageService _insuranceCoverageService;
        private readonly LoggerService _loggerService;

        public PatientProcedureController(
            ApplicationDbContext dbContext,
            EncounterInsuranceService encounterInsuranceService,
            InsuranceCoverageService insuranceCoverageService,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _encounterInsuranceService = encounterInsuranceService;
            _insuranceCoverageService = insuranceCoverageService;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<PatientProcedureFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Procedure", Description = "Melihat metadata filter tindakan pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientProcedure", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new PatientProcedureFilterMetadataResponse
            {
                DefaultFilter = new PatientProcedureDefaultFilterResponse(),
                SortOptions = new List<PatientProcedureSortOptionResponse>
                {
                    new() { Value = "procedureDateTime", Label = "Tanggal tindakan" },
                    new() { Value = "procedureCode", Label = "Kode tindakan" },
                    new() { Value = "procedureName", Label = "Nama tindakan" },
                    new() { Value = "procedureStatus", Label = "Status tindakan" },
                    new() { Value = "totalPrice", Label = "Total harga" },
                    new() { Value = "patientPayAmount", Label = "Bayar pasien" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                ProcedureSourceOptions = BuildEnumOptions<PatientProcedureSource>(),
                ProcedureStatusOptions = BuildEnumOptions<PatientProcedureStatus>()
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "PatientProcedure.GetFilterMetadata",
                "Mengambil metadata filter tindakan pasien.",
                result
            );

            return Ok(ApiResponse<PatientProcedureFilterMetadataResponse>.Ok(
                result,
                "Metadata filter tindakan pasien berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponsePatientProcedurePagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Procedure", Description = "Melihat data tindakan pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientProcedure", "Read")]
        public async Task<IActionResult> GetProcedures(
            [FromQuery] string? search,
            [FromQuery] Guid? encounterId,
            [FromQuery] Guid? consultationId,
            [FromQuery] Guid? patientId,
            [FromQuery] Guid? doctorId,
            [FromQuery] Guid? serviceUnitId,
            [FromQuery] Guid? clinicId,
            [FromQuery] Guid? procedureId,
            [FromQuery] PatientProcedureStatus? procedureStatus,
            [FromQuery] bool? isPrimaryProcedure,
            [FromQuery] bool? isEmergencyProcedure,
            [FromQuery] bool? isBillable,
            [FromQuery] bool? isFreeOfCharge,
            [FromQuery] bool? isCoveredByInsurance,
            [FromQuery] bool? isNeedApproval,
            [FromQuery] bool? isApproved,
            [FromQuery] bool? isExecuted,
            [FromQuery] bool? isBillingGenerated,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? sortBy = "procedureDateTime",
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
                encounterId,
                consultationId,
                patientId,
                doctorId,
                serviceUnitId,
                clinicId,
                procedureId,
                procedureStatus,
                isPrimaryProcedure,
                isEmergencyProcedure,
                isBillable,
                isFreeOfCharge,
                isCoveredByInsurance,
                isNeedApproval,
                isApproved,
                isExecuted,
                isBillingGenerated,
                startDate,
                endDate
            );

            var totalData = await query.CountAsync();

            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = entities.Select(ToResponse).ToList();

            return Ok(ApiResponse<ResponsePatientProcedurePagedResult>.Ok(
                new ResponsePatientProcedurePagedResult
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = items
                },
                "Data tindakan pasien berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<PatientProcedureOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Patient Procedure", Description = "Melihat pilihan tindakan pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientProcedure", "Read")]
        public async Task<IActionResult> GetProcedureOptions(
            [FromQuery] Guid? consultationId,
            [FromQuery] Guid? encounterId,
            [FromQuery] Guid? patientId,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null)
        {
            var query = _dbContext.Set<TrxPatientProcedure>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (onlyActive)
                query = query.Where(x => x.IsActive && x.ProcedureStatus != PatientProcedureStatus.Cancelled);

            if (consultationId.HasValue && consultationId.Value != Guid.Empty)
                query = query.Where(x => x.ConsultationId == consultationId.Value);

            if (encounterId.HasValue && encounterId.Value != Guid.Empty)
                query = query.Where(x => x.EncounterId == encounterId.Value);

            if (patientId.HasValue && patientId.Value != Guid.Empty)
                query = query.Where(x => x.PatientId == patientId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.ProcedureCodeSnapshot.ToLower().Contains(keyword) ||
                    x.ProcedureNameSnapshot.ToLower().Contains(keyword));
            }

            var data = await query
                .OrderByDescending(x => x.IsPrimaryProcedure)
                .ThenBy(x => x.ProcedureDateTime)
                .ThenBy(x => x.ProcedureCodeSnapshot)
                .Take(100)
                .Select(x => new PatientProcedureOptionResponse
                {
                    Id = x.Id,
                    ProcedureId = x.ProcedureId,
                    ProcedureCodeSnapshot = x.ProcedureCodeSnapshot,
                    ProcedureNameSnapshot = x.ProcedureNameSnapshot,
                    ProcedureStatus = x.ProcedureStatus,
                    IsPrimaryProcedure = x.IsPrimaryProcedure,
                    IsExecuted = x.IsExecuted,
                    IsBillable = x.IsBillable
                })
                .ToListAsync();

            return Ok(ApiResponse<List<PatientProcedureOptionResponse>>.Ok(
                data,
                "Data pilihan tindakan pasien berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientProcedureDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Patient Procedure", Description = "Melihat detail tindakan pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PatientProcedure", "Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await BuildBaseQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Tindakan pasien tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<PatientProcedureDetailResponse>.Ok(
                ToDetailResponse(entity),
                "Detail tindakan pasien berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<PatientProcedureCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Patient Procedure", Description = "Membuat tindakan pasien", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("PatientProcedure", "Create")]
        public async Task<IActionResult> CreateProcedure([FromBody] CreatePatientProcedureRequest request)
        {
            var validation = await ValidateCreateRequestAsync(request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data tindakan pasien tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var consultation = await _dbContext.Set<TrxDoctorConsultation>()
                .FirstAsync(x =>
                    x.Id == request.ConsultationId &&
                    x.EncounterId == request.EncounterId &&
                    !x.IsDelete);

            var procedure = await _dbContext.Set<MstProcedure>()
                .AsNoTracking()
                .FirstAsync(x => x.Id == request.ProcedureId && !x.IsDelete);

            var serviceDate = request.ProcedureDateTime ?? now;

            var insuranceContext = await _encounterInsuranceService.GetContextAsync(
                request.EncounterId,
                serviceDate);

            if (!insuranceContext.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    insuranceContext.ErrorMessage ?? "Konteks pembayaran encounter tidak valid."
                ));
            }

            var pricing = await _insuranceCoverageService.ResolveProcedureAsync(
                request.EncounterId,
                request.ProcedureId,
                request.Quantity,
                serviceDate);

            if (!pricing.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    pricing.ErrorMessage ?? "Tarif atau coverage tindakan tidak dapat ditentukan."
                ));
            }

            MstInsuranceTariff? insuranceTariffSnapshot = null;

            if (pricing.InsuranceTariffId.HasValue)
            {
                insuranceTariffSnapshot = await _dbContext.Set<MstInsuranceTariff>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.Id == pricing.InsuranceTariffId.Value &&
                        !x.IsDelete);
            }

            var needApproval = pricing.IsNeedApproval || procedure.IsNeedApproval;

            if (request.ExecuteImmediately && needApproval)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Tindakan membutuhkan approval dan tidak dapat langsung dieksekusi."
                ));
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            if (request.IsPrimaryProcedure)
            {
                await ClearPrimaryProcedureAsync(request.ConsultationId, actorUserId, now);
            }

            var entity = new TrxPatientProcedure
            {
                Id = Guid.NewGuid(),
                EncounterId = consultation.EncounterId,
                ConsultationId = consultation.Id,
                PatientId = consultation.PatientId,
                DoctorId = consultation.DoctorId,
                ServiceUnitId = consultation.ServiceUnitId,
                ClinicId = consultation.ClinicId,
                ProcedureId = procedure.Id,
                TariffId = pricing.TariffId,
                InsuranceTariffId = pricing.InsuranceTariffId,
                InsuranceCoverageRuleId = pricing.InsuranceCoverageRuleId,

                ProcedureCodeSnapshot = procedure.ProcedureCode,
                ProcedureNameSnapshot = procedure.ProcedureName,
                ProcedureTypeSnapshot = procedure.ProcedureType,
                ProcedureCategoryNameSnapshot = procedure.ProcedureCategoryName,
                ProcedureMasterType = "Master",
                IsFromMasterProcedure = true,
                IsPrimaryProcedure = request.IsPrimaryProcedure,
                IsEmergencyProcedure = request.IsEmergencyProcedure,
                IsSurgeryRelated = request.IsSurgeryRelated,
                IsPackageProcedure = request.IsPackageProcedure,

                PatientTypeSnapshot = null,
                PaymentTypeSnapshot = insuranceContext.PaymentType.ToString(),
                PatientClassNameSnapshot = insuranceContext.PatientClassName,
                InsuranceProviderNameSnapshot = insuranceContext.InsuranceProviderName,
                BenefitPlanNameSnapshot = insuranceContext.BenefitPlanName,
                InsuranceTariffCodeSnapshot = insuranceTariffSnapshot?.InsuranceTariffCode,
                InsuranceTariffNameSnapshot = insuranceTariffSnapshot?.InsuranceTariffName,

                ProcedureSource = request.ProcedureSource,
                ProcedureStatus = request.ExecuteImmediately
                    ? PatientProcedureStatus.Completed
                    : PatientProcedureStatus.Planned,
                ProcedureDateTime = request.ProcedureDateTime ?? now,
                PlannedAt = request.PlannedAt ?? now,
                ScheduledAt = request.ScheduledAt,
                StartedAt = request.ExecuteImmediately ? now : null,
                CompletedAt = request.ExecuteImmediately ? now : null,

                Quantity = pricing.Quantity,
                UnitNameSnapshot = NormalizeNullableText(request.UnitNameSnapshot),
                UnitPrice = pricing.UnitPrice,
                TotalPrice = pricing.TotalPrice,
                HospitalPriceSnapshot = pricing.HospitalUnitPrice,
                InsuranceContractPrice = pricing.ContractUnitPrice,

                IsFreeOfCharge = request.IsFreeOfCharge,
                FreeOfChargeReason = NormalizeNullableText(request.FreeOfChargeReason),
                IsBillable = !request.IsFreeOfCharge,

                IsCoveredByInsurance = pricing.IsCovered,
                CoverageStatus = pricing.CoverageStatus,
                CoveragePercent = pricing.CoveragePercent,
                CoveredAmount = pricing.CoveredAmount,
                PatientPayAmount = pricing.PatientPayAmount,
                CoverageNote = pricing.CoverageNote,

                IsNeedApproval = needApproval,
                IsApproved = false,

                IsExecuted = request.ExecuteImmediately,
                ExecutedAt = request.ExecuteImmediately ? now : null,
                ExecutedByUserId = request.ExecuteImmediately ? actorUserId : null,
                PerformedAt = request.ExecuteImmediately ? now : null,
                PerformedByUserId = request.ExecuteImmediately ? actorUserId : null,

                ClinicalNote = NormalizeNullableText(request.ClinicalNote),
                InstructionNote = NormalizeNullableText(request.InstructionNote),
                DispositionNote = NormalizeNullableText(request.DispositionNote),
                ComplicationNote = NormalizeNullableText(request.ComplicationNote),
                FollowUpInstruction = NormalizeNullableText(request.FollowUpInstruction),

                IsBillingGenerated = false,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            NormalizeProcedureData(entity);

            _dbContext.Set<TrxPatientProcedure>().Add(entity);

            await _dbContext.SaveChangesAsync();

            var summary = await UpdateConsultationProcedureSummaryAsync(
                consultation.Id,
                actorUserId,
                now
            );

            await transaction.CommitAsync();

            var response = ToCreateResponse(entity, summary);

            await _loggerService.InfoAsync(
                LogCategory,
                "PatientProcedure.CreateProcedure",
                "Membuat tindakan pasien.",
                response
            );

            return Ok(ApiResponse<PatientProcedureCreateResponse>.Ok(
                response,
                "Tindakan pasien berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PatientProcedureUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Patient Procedure", Description = "Mengubah tindakan pasien", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("PatientProcedure", "Update")]
        public async Task<IActionResult> UpdateProcedure(Guid id, [FromBody] UpdatePatientProcedureRequest request)
        {
            var entity = await _dbContext.Set<TrxPatientProcedure>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Tindakan pasien tidak ditemukan."
                ));
            }

            if (entity.IsBillingGenerated)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Tindakan yang sudah masuk billing tidak dapat diubah."
                ));
            }

            if (entity.IsExecuted)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Tindakan yang sudah dieksekusi tidak dapat diubah. Gunakan pembatalan bila diperlukan."
                ));
            }

            if (entity.ProcedureStatus == PatientProcedureStatus.Cancelled)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Tindakan yang sudah cancelled tidak dapat diubah."
                ));
            }

            var procedure = await _dbContext.Set<MstProcedure>()
                .AsNoTracking()
                .FirstAsync(x => x.Id == entity.ProcedureId && !x.IsDelete);

            var serviceDate = request.ProcedureDateTime ?? entity.ProcedureDateTime;

            var insuranceContext = await _encounterInsuranceService.GetContextAsync(
                entity.EncounterId,
                serviceDate);

            if (!insuranceContext.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    insuranceContext.ErrorMessage ?? "Konteks pembayaran encounter tidak valid."
                ));
            }

            var pricing = await _insuranceCoverageService.ResolveProcedureAsync(
                entity.EncounterId,
                entity.ProcedureId,
                request.Quantity,
                serviceDate);

            if (!pricing.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    pricing.ErrorMessage ?? "Tarif atau coverage tindakan tidak dapat ditentukan."
                ));
            }

            MstInsuranceTariff? insuranceTariffSnapshot = null;

            if (pricing.InsuranceTariffId.HasValue)
            {
                insuranceTariffSnapshot = await _dbContext.Set<MstInsuranceTariff>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.Id == pricing.InsuranceTariffId.Value &&
                        !x.IsDelete);
            }

            var needApproval = pricing.IsNeedApproval || procedure.IsNeedApproval;

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            if (request.IsPrimaryProcedure)
            {
                await ClearPrimaryProcedureAsync(entity.ConsultationId, actorUserId, now, entity.Id);
            }

            entity.TariffId = pricing.TariffId;
            entity.InsuranceTariffId = pricing.InsuranceTariffId;
            entity.InsuranceCoverageRuleId = pricing.InsuranceCoverageRuleId;

            entity.PaymentTypeSnapshot = insuranceContext.PaymentType.ToString();
            entity.PatientClassNameSnapshot = insuranceContext.PatientClassName;
            entity.InsuranceProviderNameSnapshot = insuranceContext.InsuranceProviderName;
            entity.BenefitPlanNameSnapshot = insuranceContext.BenefitPlanName;
            entity.InsuranceTariffCodeSnapshot = insuranceTariffSnapshot?.InsuranceTariffCode;
            entity.InsuranceTariffNameSnapshot = insuranceTariffSnapshot?.InsuranceTariffName;

            entity.ProcedureSource = request.ProcedureSource;
            entity.ProcedureDateTime = request.ProcedureDateTime ?? entity.ProcedureDateTime;
            entity.PlannedAt = request.PlannedAt ?? entity.PlannedAt;
            entity.ScheduledAt = request.ScheduledAt;

            entity.IsPrimaryProcedure = request.IsPrimaryProcedure;
            entity.IsEmergencyProcedure = request.IsEmergencyProcedure;
            entity.IsSurgeryRelated = request.IsSurgeryRelated;
            entity.IsPackageProcedure = request.IsPackageProcedure;

            entity.Quantity = pricing.Quantity;
            entity.UnitNameSnapshot = NormalizeNullableText(request.UnitNameSnapshot);
            entity.UnitPrice = pricing.UnitPrice;
            entity.TotalPrice = pricing.TotalPrice;
            entity.HospitalPriceSnapshot = pricing.HospitalUnitPrice;
            entity.InsuranceContractPrice = pricing.ContractUnitPrice;

            entity.IsFreeOfCharge = request.IsFreeOfCharge;
            entity.FreeOfChargeReason = NormalizeNullableText(request.FreeOfChargeReason);
            entity.IsBillable = !request.IsFreeOfCharge;

            entity.IsCoveredByInsurance = pricing.IsCovered;
            entity.CoverageStatus = pricing.CoverageStatus;
            entity.CoveragePercent = pricing.CoveragePercent;
            entity.CoveredAmount = pricing.CoveredAmount;
            entity.PatientPayAmount = pricing.PatientPayAmount;
            entity.CoverageNote = pricing.CoverageNote;

            entity.IsNeedApproval = needApproval;

            entity.ClinicalNote = NormalizeNullableText(request.ClinicalNote);
            entity.ResultNote = NormalizeNullableText(request.ResultNote);
            entity.InstructionNote = NormalizeNullableText(request.InstructionNote);
            entity.DispositionNote = NormalizeNullableText(request.DispositionNote);
            entity.ComplicationNote = NormalizeNullableText(request.ComplicationNote);
            entity.FollowUpInstruction = NormalizeNullableText(request.FollowUpInstruction);

            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            NormalizeProcedureData(entity);

            await _dbContext.SaveChangesAsync();

            var summary = await UpdateConsultationProcedureSummaryAsync(
                entity.ConsultationId,
                actorUserId,
                now
            );

            await transaction.CommitAsync();

            var response = ToUpdateResponse(entity, summary);

            return Ok(ApiResponse<PatientProcedureUpdateResponse>.Ok(
                response,
                "Tindakan pasien berhasil diubah."
            ));
        }

        [HttpPatch("{id:guid}/approve")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Approve Patient Procedure", Description = "Approve tindakan pasien", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("PatientProcedure", "Update")]
        public async Task<IActionResult> ApproveProcedure(Guid id, [FromBody] ApprovePatientProcedureRequest request)
        {
            var entity = await _dbContext.Set<TrxPatientProcedure>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Tindakan pasien tidak ditemukan."
                ));
            }

            if (!entity.IsNeedApproval)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Tindakan ini tidak membutuhkan approval."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsApproved = true;
            entity.ApprovedAt = now;
            entity.ApprovedByUserId = actorUserId;
            entity.ApprovalNote = NormalizeNullableText(request.ApprovalNote);
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Tindakan pasien berhasil disetujui."
            ));
        }

        [HttpPatch("{id:guid}/execute")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Execute Patient Procedure", Description = "Eksekusi tindakan pasien", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("PatientProcedure", "Update")]
        public async Task<IActionResult> ExecuteProcedure(Guid id, [FromBody] ExecutePatientProcedureRequest request)
        {
            var entity = await _dbContext.Set<TrxPatientProcedure>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Tindakan pasien tidak ditemukan."
                ));
            }

            if (entity.ProcedureStatus == PatientProcedureStatus.Cancelled)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Tindakan yang sudah cancelled tidak dapat dieksekusi."
                ));
            }

            if (entity.IsNeedApproval && !entity.IsApproved)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Tindakan membutuhkan approval sebelum dieksekusi."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.ProcedureStatus = PatientProcedureStatus.Completed;
            entity.IsExecuted = true;
            entity.ExecutedAt = now;
            entity.ExecutedByUserId = actorUserId;
            entity.PerformedAt = request.PerformedAt ?? now;
            entity.PerformedByUserId = actorUserId;
            entity.StartedAt ??= now;
            entity.CompletedAt = now;
            entity.ResultNote = NormalizeNullableText(request.ResultNote) ?? entity.ResultNote;
            entity.DispositionNote = NormalizeNullableText(request.DispositionNote) ?? entity.DispositionNote;
            entity.ComplicationNote = NormalizeNullableText(request.ComplicationNote) ?? entity.ComplicationNote;
            entity.FollowUpInstruction = NormalizeNullableText(request.FollowUpInstruction) ?? entity.FollowUpInstruction;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Tindakan pasien berhasil dieksekusi."
            ));
        }

        [HttpPatch("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Cancel Patient Procedure", Description = "Membatalkan tindakan pasien", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("PatientProcedure", "Update")]
        public async Task<IActionResult> CancelProcedure(Guid id, [FromBody] CancelPatientProcedureRequest request)
        {
            var entity = await _dbContext.Set<TrxPatientProcedure>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Tindakan pasien tidak ditemukan."
                ));
            }

            if (entity.IsBillingGenerated)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Tindakan yang sudah masuk billing tidak dapat dibatalkan dari modul klinis."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            entity.ProcedureStatus = PatientProcedureStatus.Cancelled;
            entity.CancelledAt = now;
            entity.CancelledByUserId = actorUserId;
            entity.CancelReason = request.CancelReason.Trim();
            entity.IsActive = false;
            entity.IsCancel = true;
            entity.CancelDateTime = now;
            entity.CancelBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            await UpdateConsultationProcedureSummaryAsync(
                entity.ConsultationId,
                actorUserId,
                now
            );

            await transaction.CommitAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Tindakan pasien berhasil dibatalkan."
            ));
        }

        private IQueryable<TrxPatientProcedure> BuildBaseQuery()
        {
            return _dbContext.Set<TrxPatientProcedure>()
                .Include(x => x.Encounter)
                .Include(x => x.Consultation)
                .Include(x => x.Patient)
                .Include(x => x.Doctor)
                .Include(x => x.ServiceUnit)
                .Include(x => x.Clinic)
                .Include(x => x.Procedure)
                .Include(x => x.Tariff)
                .Include(x => x.InsuranceTariff)
                .Include(x => x.InsuranceCoverageRule)
                .Include(x => x.ApprovedByUser)
                .Include(x => x.ExecutedByUser)
                .Include(x => x.PerformedByUser)
                .Include(x => x.CancelledByUser)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<TrxPatientProcedure> ApplyFilters(
            IQueryable<TrxPatientProcedure> query,
            string? search,
            Guid? encounterId,
            Guid? consultationId,
            Guid? patientId,
            Guid? doctorId,
            Guid? serviceUnitId,
            Guid? clinicId,
            Guid? procedureId,
            PatientProcedureStatus? procedureStatus,
            bool? isPrimaryProcedure,
            bool? isEmergencyProcedure,
            bool? isBillable,
            bool? isFreeOfCharge,
            bool? isCoveredByInsurance,
            bool? isNeedApproval,
            bool? isApproved,
            bool? isExecuted,
            bool? isBillingGenerated,
            DateTime? startDate,
            DateTime? endDate)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.ProcedureCodeSnapshot.ToLower().Contains(keyword) ||
                    x.ProcedureNameSnapshot.ToLower().Contains(keyword) ||
                    (x.ProcedureTypeSnapshot != null && x.ProcedureTypeSnapshot.ToLower().Contains(keyword)) ||
                    (x.ProcedureCategoryNameSnapshot != null && x.ProcedureCategoryNameSnapshot.ToLower().Contains(keyword)) ||
                    (x.Encounter != null && x.Encounter.EncounterNumber.ToLower().Contains(keyword)) ||
                    (x.Consultation != null && x.Consultation.ConsultationNumber.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.FullName.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.MedicalRecordNumber.ToLower().Contains(keyword)) ||
                    (x.Doctor != null && x.Doctor.FullName.ToLower().Contains(keyword)));
            }

            if (encounterId.HasValue && encounterId.Value != Guid.Empty)
                query = query.Where(x => x.EncounterId == encounterId.Value);

            if (consultationId.HasValue && consultationId.Value != Guid.Empty)
                query = query.Where(x => x.ConsultationId == consultationId.Value);

            if (patientId.HasValue && patientId.Value != Guid.Empty)
                query = query.Where(x => x.PatientId == patientId.Value);

            if (doctorId.HasValue && doctorId.Value != Guid.Empty)
                query = query.Where(x => x.DoctorId == doctorId.Value);

            if (serviceUnitId.HasValue && serviceUnitId.Value != Guid.Empty)
                query = query.Where(x => x.ServiceUnitId == serviceUnitId.Value);

            if (clinicId.HasValue && clinicId.Value != Guid.Empty)
                query = query.Where(x => x.ClinicId == clinicId.Value);

            if (procedureId.HasValue && procedureId.Value != Guid.Empty)
                query = query.Where(x => x.ProcedureId == procedureId.Value);

            if (procedureStatus.HasValue)
                query = query.Where(x => x.ProcedureStatus == procedureStatus.Value);

            if (isPrimaryProcedure.HasValue)
                query = query.Where(x => x.IsPrimaryProcedure == isPrimaryProcedure.Value);

            if (isEmergencyProcedure.HasValue)
                query = query.Where(x => x.IsEmergencyProcedure == isEmergencyProcedure.Value);

            if (isBillable.HasValue)
                query = query.Where(x => x.IsBillable == isBillable.Value);

            if (isFreeOfCharge.HasValue)
                query = query.Where(x => x.IsFreeOfCharge == isFreeOfCharge.Value);

            if (isCoveredByInsurance.HasValue)
                query = query.Where(x => x.IsCoveredByInsurance == isCoveredByInsurance.Value);

            if (isNeedApproval.HasValue)
                query = query.Where(x => x.IsNeedApproval == isNeedApproval.Value);

            if (isApproved.HasValue)
                query = query.Where(x => x.IsApproved == isApproved.Value);

            if (isExecuted.HasValue)
                query = query.Where(x => x.IsExecuted == isExecuted.Value);

            if (isBillingGenerated.HasValue)
                query = query.Where(x => x.IsBillingGenerated == isBillingGenerated.Value);

            if (startDate.HasValue)
                query = query.Where(x => x.ProcedureDateTime >= startDate.Value.Date);

            if (endDate.HasValue)
                query = query.Where(x => x.ProcedureDateTime < endDate.Value.Date.AddDays(1));

            return query;
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateCreateRequestAsync(
            CreatePatientProcedureRequest request)
        {
            var consultation = await _dbContext.Set<TrxDoctorConsultation>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == request.ConsultationId &&
                    x.EncounterId == request.EncounterId &&
                    !x.IsDelete);

            if (consultation == null)
                return (false, "Konsultasi dokter tidak ditemukan atau tidak sesuai encounter.");

            if (consultation.ConsultationStatus == DoctorConsultationStatus.Completed)
                return (false, "Konsultasi yang sudah completed tidak dapat ditambahkan tindakan.");

            if (consultation.ConsultationStatus == DoctorConsultationStatus.Cancelled)
                return (false, "Konsultasi yang sudah cancelled tidak dapat ditambahkan tindakan.");

            var procedureExists = await _dbContext.Set<MstProcedure>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == request.ProcedureId &&
                    x.IsActive &&
                    !x.IsDelete);

            if (!procedureExists)
                return (false, "Master tindakan tidak ditemukan atau tidak aktif.");

            if (request.Quantity <= 0)
                return (false, "Quantity tindakan harus lebih dari 0.");

            if (request.IsFreeOfCharge && string.IsNullOrWhiteSpace(request.FreeOfChargeReason))
                return (false, "Alasan FOC wajib diisi.");

            return (true, null);
        }

        private async Task ClearPrimaryProcedureAsync(
            Guid consultationId,
            Guid actorUserId,
            DateTime now,
            Guid? exceptProcedureId = null)
        {
            var query = _dbContext.Set<TrxPatientProcedure>()
                .Where(x =>
                    x.ConsultationId == consultationId &&
                    x.IsPrimaryProcedure &&
                    !x.IsDelete &&
                    x.ProcedureStatus != PatientProcedureStatus.Cancelled);

            if (exceptProcedureId.HasValue)
                query = query.Where(x => x.Id != exceptProcedureId.Value);

            var existingPrimaryProcedures = await query.ToListAsync();

            foreach (var procedure in existingPrimaryProcedures)
            {
                procedure.IsPrimaryProcedure = false;
                procedure.UpdateDateTime = now;
                procedure.UpdateBy = actorUserId;
            }
        }

        private async Task<ProcedureSummaryResult> UpdateConsultationProcedureSummaryAsync(
            Guid consultationId,
            Guid actorUserId,
            DateTime now)
        {
            var consultation = await _dbContext.Set<TrxDoctorConsultation>()
                .FirstAsync(x => x.Id == consultationId && !x.IsDelete);

            var procedures = await _dbContext.Set<TrxPatientProcedure>()
                .AsNoTracking()
                .Where(x =>
                    x.ConsultationId == consultationId &&
                    !x.IsDelete &&
                    x.ProcedureStatus != PatientProcedureStatus.Cancelled)
                .OrderByDescending(x => x.IsPrimaryProcedure)
                .ThenBy(x => x.ProcedureDateTime)
                .ToListAsync();

            var procedureText = procedures.Count == 0
                ? null
                : string.Join("; ", procedures.Select(x => $"{x.ProcedureCodeSnapshot} - {x.ProcedureNameSnapshot}"));

            consultation.ProcedureText = procedureText;
            consultation.ProcedureCount = procedures.Count;
            consultation.HasProcedure = procedures.Count > 0;
            consultation.UpdateDateTime = now;
            consultation.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return new ProcedureSummaryResult
            {
                ProcedureText = procedureText,
                ProcedureCount = procedures.Count,
                HasProcedure = procedures.Count > 0
            };
        }

        private static void NormalizeProcedureData(TrxPatientProcedure entity)
        {
            if (entity.Quantity <= 0)
                entity.Quantity = 1;

            entity.TotalPrice = Math.Round(entity.UnitPrice * entity.Quantity, 2);

            if (entity.IsFreeOfCharge)
            {
                entity.IsBillable = false;
                entity.IsCoveredByInsurance = false;
                entity.CoverageStatus = "FOC";
                entity.CoveragePercent = 0;
                entity.CoveredAmount = 0;
                entity.PatientPayAmount = 0;
                entity.IsNeedApproval = false;
                entity.IsApproved = false;
            }

            if (!entity.IsNeedApproval)
            {
                entity.IsApproved = false;
                entity.ApprovedAt = null;
                entity.ApprovedByUserId = null;
                entity.ApprovalNote = null;
            }
        }


        private static IQueryable<TrxPatientProcedure> ApplySorting(
            IQueryable<TrxPatientProcedure> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "procedureDateTime").ToLowerInvariant() switch
            {
                "procedurecode" => isDesc ? query.OrderByDescending(x => x.ProcedureCodeSnapshot) : query.OrderBy(x => x.ProcedureCodeSnapshot),
                "procedurename" => isDesc ? query.OrderByDescending(x => x.ProcedureNameSnapshot) : query.OrderBy(x => x.ProcedureNameSnapshot),
                "procedurestatus" => isDesc ? query.OrderByDescending(x => x.ProcedureStatus) : query.OrderBy(x => x.ProcedureStatus),
                "totalprice" => isDesc ? query.OrderByDescending(x => x.TotalPrice) : query.OrderBy(x => x.TotalPrice),
                "patientpayamount" => isDesc ? query.OrderByDescending(x => x.PatientPayAmount) : query.OrderBy(x => x.PatientPayAmount),
                "createdatetime" => isDesc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                _ => isDesc ? query.OrderByDescending(x => x.ProcedureDateTime) : query.OrderBy(x => x.ProcedureDateTime)
            };
        }

        private static PatientProcedureResponse ToResponse(TrxPatientProcedure x)
        {
            return new PatientProcedureResponse
            {
                Id = x.Id,
                EncounterId = x.EncounterId,
                EncounterNumber = x.Encounter != null ? x.Encounter.EncounterNumber : string.Empty,
                ConsultationId = x.ConsultationId,
                ConsultationNumber = x.Consultation != null ? x.Consultation.ConsultationNumber : string.Empty,
                PatientId = x.PatientId,
                PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty,
                DoctorId = x.DoctorId,
                DoctorName = x.Doctor != null ? x.Doctor.FullName : string.Empty,
                ServiceUnitId = x.ServiceUnitId,
                ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null,
                ClinicId = x.ClinicId,
                ClinicName = x.Clinic != null ? x.Clinic.ClinicName : null,
                ProcedureId = x.ProcedureId,
                TariffId = x.TariffId,
                InsuranceTariffId = x.InsuranceTariffId,
                InsuranceCoverageRuleId = x.InsuranceCoverageRuleId,
                ProcedureCodeSnapshot = x.ProcedureCodeSnapshot,
                ProcedureNameSnapshot = x.ProcedureNameSnapshot,
                ProcedureTypeSnapshot = x.ProcedureTypeSnapshot,
                ProcedureCategoryNameSnapshot = x.ProcedureCategoryNameSnapshot,
                ProcedureMasterType = x.ProcedureMasterType,
                IsFromMasterProcedure = x.IsFromMasterProcedure,
                IsPrimaryProcedure = x.IsPrimaryProcedure,
                IsEmergencyProcedure = x.IsEmergencyProcedure,
                IsSurgeryRelated = x.IsSurgeryRelated,
                IsPackageProcedure = x.IsPackageProcedure,
                ProcedureSource = x.ProcedureSource,
                ProcedureStatus = x.ProcedureStatus,
                ProcedureDateTime = x.ProcedureDateTime,
                PlannedAt = x.PlannedAt,
                ScheduledAt = x.ScheduledAt,
                StartedAt = x.StartedAt,
                CompletedAt = x.CompletedAt,
                Quantity = x.Quantity,
                UnitNameSnapshot = x.UnitNameSnapshot,
                UnitPrice = x.UnitPrice,
                TotalPrice = x.TotalPrice,
                HospitalPriceSnapshot = x.HospitalPriceSnapshot,
                InsuranceContractPrice = x.InsuranceContractPrice,
                IsFreeOfCharge = x.IsFreeOfCharge,
                IsBillable = x.IsBillable,
                IsCoveredByInsurance = x.IsCoveredByInsurance,
                CoverageStatus = x.CoverageStatus,
                CoveragePercent = x.CoveragePercent,
                CoveredAmount = x.CoveredAmount,
                PatientPayAmount = x.PatientPayAmount,
                IsNeedApproval = x.IsNeedApproval,
                IsApproved = x.IsApproved,
                IsExecuted = x.IsExecuted,
                ExecutedAt = x.ExecutedAt,
                PerformedAt = x.PerformedAt,
                PerformedByUserId = x.PerformedByUserId,
                PerformedByUserName = x.PerformedByUser != null ? x.PerformedByUser.DisplayName : null,
                IsBillingGenerated = x.IsBillingGenerated,
                BillingGeneratedAt = x.BillingGeneratedAt,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime
            };
        }

        private static PatientProcedureDetailResponse ToDetailResponse(TrxPatientProcedure x)
        {
            var response = new PatientProcedureDetailResponse
            {
                PatientTypeSnapshot = x.PatientTypeSnapshot,
                PaymentTypeSnapshot = x.PaymentTypeSnapshot,
                PatientClassNameSnapshot = x.PatientClassNameSnapshot,
                InsuranceProviderNameSnapshot = x.InsuranceProviderNameSnapshot,
                BenefitPlanNameSnapshot = x.BenefitPlanNameSnapshot,
                InsuranceTariffCodeSnapshot = x.InsuranceTariffCodeSnapshot,
                InsuranceTariffNameSnapshot = x.InsuranceTariffNameSnapshot,
                FreeOfChargeReason = x.FreeOfChargeReason,
                CoverageNote = x.CoverageNote,
                ApprovedAt = x.ApprovedAt,
                ApprovedByUserId = x.ApprovedByUserId,
                ApprovedByUserName = x.ApprovedByUser != null ? x.ApprovedByUser.DisplayName : null,
                ApprovalNote = x.ApprovalNote,
                ExecutedByUserId = x.ExecutedByUserId,
                ExecutedByUserName = x.ExecutedByUser != null ? x.ExecutedByUser.DisplayName : null,
                ClinicalNote = x.ClinicalNote,
                ResultNote = x.ResultNote,
                InstructionNote = x.InstructionNote,
                DispositionNote = x.DispositionNote,
                ComplicationNote = x.ComplicationNote,
                FollowUpInstruction = x.FollowUpInstruction,
                BillingItemId = x.BillingItemId,
                CancelledAt = x.CancelledAt,
                CancelledByUserId = x.CancelledByUserId,
                CancelledByUserName = x.CancelledByUser != null ? x.CancelledByUser.DisplayName : null,
                CancelReason = x.CancelReason
            };

            CopyBaseResponse(x, response);

            return response;
        }

        private static void CopyBaseResponse(TrxPatientProcedure x, PatientProcedureResponse response)
        {
            response.Id = x.Id;
            response.EncounterId = x.EncounterId;
            response.EncounterNumber = x.Encounter != null ? x.Encounter.EncounterNumber : string.Empty;
            response.ConsultationId = x.ConsultationId;
            response.ConsultationNumber = x.Consultation != null ? x.Consultation.ConsultationNumber : string.Empty;
            response.PatientId = x.PatientId;
            response.PatientName = x.Patient != null ? x.Patient.FullName : string.Empty;
            response.MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty;
            response.DoctorId = x.DoctorId;
            response.DoctorName = x.Doctor != null ? x.Doctor.FullName : string.Empty;
            response.ServiceUnitId = x.ServiceUnitId;
            response.ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null;
            response.ClinicId = x.ClinicId;
            response.ClinicName = x.Clinic != null ? x.Clinic.ClinicName : null;
            response.ProcedureId = x.ProcedureId;
            response.TariffId = x.TariffId;
            response.InsuranceTariffId = x.InsuranceTariffId;
            response.InsuranceCoverageRuleId = x.InsuranceCoverageRuleId;
            response.ProcedureCodeSnapshot = x.ProcedureCodeSnapshot;
            response.ProcedureNameSnapshot = x.ProcedureNameSnapshot;
            response.ProcedureTypeSnapshot = x.ProcedureTypeSnapshot;
            response.ProcedureCategoryNameSnapshot = x.ProcedureCategoryNameSnapshot;
            response.ProcedureMasterType = x.ProcedureMasterType;
            response.IsFromMasterProcedure = x.IsFromMasterProcedure;
            response.IsPrimaryProcedure = x.IsPrimaryProcedure;
            response.IsEmergencyProcedure = x.IsEmergencyProcedure;
            response.IsSurgeryRelated = x.IsSurgeryRelated;
            response.IsPackageProcedure = x.IsPackageProcedure;
            response.ProcedureSource = x.ProcedureSource;
            response.ProcedureStatus = x.ProcedureStatus;
            response.ProcedureDateTime = x.ProcedureDateTime;
            response.PlannedAt = x.PlannedAt;
            response.ScheduledAt = x.ScheduledAt;
            response.StartedAt = x.StartedAt;
            response.CompletedAt = x.CompletedAt;
            response.Quantity = x.Quantity;
            response.UnitNameSnapshot = x.UnitNameSnapshot;
            response.UnitPrice = x.UnitPrice;
            response.TotalPrice = x.TotalPrice;
            response.HospitalPriceSnapshot = x.HospitalPriceSnapshot;
            response.InsuranceContractPrice = x.InsuranceContractPrice;
            response.IsFreeOfCharge = x.IsFreeOfCharge;
            response.IsBillable = x.IsBillable;
            response.IsCoveredByInsurance = x.IsCoveredByInsurance;
            response.CoverageStatus = x.CoverageStatus;
            response.CoveragePercent = x.CoveragePercent;
            response.CoveredAmount = x.CoveredAmount;
            response.PatientPayAmount = x.PatientPayAmount;
            response.IsNeedApproval = x.IsNeedApproval;
            response.IsApproved = x.IsApproved;
            response.IsExecuted = x.IsExecuted;
            response.ExecutedAt = x.ExecutedAt;
            response.PerformedAt = x.PerformedAt;
            response.PerformedByUserId = x.PerformedByUserId;
            response.PerformedByUserName = x.PerformedByUser != null ? x.PerformedByUser.DisplayName : null;
            response.IsBillingGenerated = x.IsBillingGenerated;
            response.BillingGeneratedAt = x.BillingGeneratedAt;
            response.IsActive = x.IsActive;
            response.CreateDateTime = x.CreateDateTime;
        }

        private static PatientProcedureCreateResponse ToCreateResponse(
            TrxPatientProcedure x,
            ProcedureSummaryResult summary)
        {
            return new PatientProcedureCreateResponse
            {
                Id = x.Id,
                EncounterId = x.EncounterId,
                ConsultationId = x.ConsultationId,
                ProcedureId = x.ProcedureId,
                TariffId = x.TariffId,
                InsuranceTariffId = x.InsuranceTariffId,
                InsuranceCoverageRuleId = x.InsuranceCoverageRuleId,
                ProcedureCodeSnapshot = x.ProcedureCodeSnapshot,
                ProcedureNameSnapshot = x.ProcedureNameSnapshot,
                ProcedureMasterType = x.ProcedureMasterType,
                IsFromMasterProcedure = x.IsFromMasterProcedure,
                IsPrimaryProcedure = x.IsPrimaryProcedure,
                IsEmergencyProcedure = x.IsEmergencyProcedure,
                ProcedureStatus = x.ProcedureStatus,
                ProcedureDateTime = x.ProcedureDateTime,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice,
                TotalPrice = x.TotalPrice,
                CoveredAmount = x.CoveredAmount,
                PatientPayAmount = x.PatientPayAmount,
                IsFreeOfCharge = x.IsFreeOfCharge,
                IsBillable = x.IsBillable,
                IsCoveredByInsurance = x.IsCoveredByInsurance,
                CoverageStatus = x.CoverageStatus,
                IsNeedApproval = x.IsNeedApproval,
                IsExecuted = x.IsExecuted,
                ProcedureCount = summary.ProcedureCount,
                HasProcedure = summary.HasProcedure,
                ProcedureText = summary.ProcedureText
            };
        }

        private static PatientProcedureUpdateResponse ToUpdateResponse(
            TrxPatientProcedure x,
            ProcedureSummaryResult summary)
        {
            return new PatientProcedureUpdateResponse
            {
                Id = x.Id,
                EncounterId = x.EncounterId,
                ConsultationId = x.ConsultationId,
                ProcedureId = x.ProcedureId,
                TariffId = x.TariffId,
                InsuranceTariffId = x.InsuranceTariffId,
                InsuranceCoverageRuleId = x.InsuranceCoverageRuleId,
                ProcedureCodeSnapshot = x.ProcedureCodeSnapshot,
                ProcedureNameSnapshot = x.ProcedureNameSnapshot,
                ProcedureMasterType = x.ProcedureMasterType,
                IsFromMasterProcedure = x.IsFromMasterProcedure,
                IsPrimaryProcedure = x.IsPrimaryProcedure,
                IsEmergencyProcedure = x.IsEmergencyProcedure,
                ProcedureStatus = x.ProcedureStatus,
                ProcedureDateTime = x.ProcedureDateTime,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice,
                TotalPrice = x.TotalPrice,
                CoveredAmount = x.CoveredAmount,
                PatientPayAmount = x.PatientPayAmount,
                IsFreeOfCharge = x.IsFreeOfCharge,
                IsBillable = x.IsBillable,
                IsCoveredByInsurance = x.IsCoveredByInsurance,
                CoverageStatus = x.CoverageStatus,
                IsNeedApproval = x.IsNeedApproval,
                IsExecuted = x.IsExecuted,
                ProcedureCount = summary.ProcedureCount,
                HasProcedure = summary.HasProcedure,
                ProcedureText = summary.ProcedureText
            };
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            return (pageNumber, pageSize);
        }

        private static List<PatientProcedureEnumOptionResponse> BuildEnumOptions<TEnum>() where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Select(x => new PatientProcedureEnumOptionResponse
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

        private Guid GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userId, out var id)
                ? id
                : Guid.Empty;
        }

        private class ProcedureSummaryResult
        {
            public string? ProcedureText { get; set; }
            public int ProcedureCount { get; set; }
            public bool HasProcedure { get; set; }
        }

    }
}