using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Security.Claims;

using ResponsePrescriptionPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs.PrescriptionResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/pharmacy-management/prescriptions")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_PHARMACY",
        moduleName: "Health Service Pharmacy",
        displayName: "Prescription",
        AreaName = "HealthServices",
        ControllerName = "Prescription",
        Description = "Header resep dokter dengan alur billing dan farmasi",
        SortOrder = 1
    )]
    [Tags("Health Services / Pharmacy Management / Prescription")]
    public class PrescriptionController : ControllerBase
    {
        private const string LogCategory = "HealthServices.Pharmacy";

        private readonly ApplicationDbContext _dbContext;
        private readonly EncounterInsuranceService _encounterInsuranceService;
        private readonly PrescriptionNumberService _prescriptionNumberService;
        private readonly PrescriptionSummaryService _prescriptionSummaryService;
        private readonly PrescriptionWorkflowService _prescriptionWorkflowService;
        private readonly LoggerService _loggerService;

        public PrescriptionController(
            ApplicationDbContext dbContext,
            EncounterInsuranceService encounterInsuranceService,
            PrescriptionNumberService prescriptionNumberService,
            PrescriptionSummaryService prescriptionSummaryService,
            PrescriptionWorkflowService prescriptionWorkflowService,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _encounterInsuranceService = encounterInsuranceService;
            _prescriptionNumberService = prescriptionNumberService;
            _prescriptionSummaryService = prescriptionSummaryService;
            _prescriptionWorkflowService = prescriptionWorkflowService;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<PrescriptionFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Prescription", Description = "Melihat metadata filter resep", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Prescription", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new PrescriptionFilterMetadataResponse
            {
                DefaultFilter = new PrescriptionDefaultFilterResponse(),
                SortOptions = new List<PrescriptionSortOptionResponse>
                {
                    new() { Value = "prescriptionDateTime", Label = "Tanggal resep" },
                    new() { Value = "prescriptionNumber", Label = "Nomor resep" },
                    new() { Value = "prescriptionStatus", Label = "Status dokter" },
                    new() { Value = "paymentStatus", Label = "Status pembayaran" },
                    new() { Value = "fulfillmentStatus", Label = "Status farmasi" },
                    new() { Value = "totalItemCount", Label = "Jumlah item" },
                    new() { Value = "totalPrice", Label = "Total harga" },
                    new() { Value = "patientPayAmount", Label = "Bayar pasien" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                PrescriptionStatusOptions = BuildEnumOptions<PrescriptionStatus>(),
                PaymentStatusOptions = BuildEnumOptions<PrescriptionPaymentStatus>(),
                FulfillmentStatusOptions = BuildEnumOptions<PrescriptionFulfillmentStatus>()
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "Prescription.GetFilterMetadata",
                "Mengambil metadata filter resep.",
                result);

            return Ok(ApiResponse<PrescriptionFilterMetadataResponse>.Ok(
                result,
                "Metadata filter resep berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponsePrescriptionPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Prescription", Description = "Melihat data resep", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Prescription", "Read")]
        public async Task<IActionResult> GetPrescriptions(
            [FromQuery] string? search,
            [FromQuery] Guid? encounterId,
            [FromQuery] Guid? consultationId,
            [FromQuery] Guid? patientId,
            [FromQuery] Guid? doctorId,
            [FromQuery] Guid? serviceUnitId,
            [FromQuery] Guid? clinicId,
            [FromQuery] PrescriptionStatus? prescriptionStatus,
            [FromQuery] PrescriptionPaymentStatus? paymentStatus,
            [FromQuery] PrescriptionFulfillmentStatus? fulfillmentStatus,
            [FromQuery] bool? isNeedApproval,
            [FromQuery] bool? isApproved,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? sortBy = "prescriptionDateTime",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = ApplyFilters(
                BuildBaseQuery().AsNoTracking(),
                search,
                encounterId,
                consultationId,
                patientId,
                doctorId,
                serviceUnitId,
                clinicId,
                prescriptionStatus,
                paymentStatus,
                fulfillmentStatus,
                isNeedApproval,
                isApproved,
                startDate,
                endDate);

            var totalData = await query.CountAsync(cancellationToken);
            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var result = new ResponsePrescriptionPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = entities.Select(ToResponse).ToList()
            };

            return Ok(ApiResponse<ResponsePrescriptionPagedResult>.Ok(
                result,
                "Data resep berhasil diambil."));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<PrescriptionOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Prescription", Description = "Melihat pilihan resep", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Prescription", "Read")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] Guid? encounterId,
            [FromQuery] Guid? consultationId,
            [FromQuery] Guid? patientId,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Set<TrxPrescription>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (onlyActive)
                query = query.Where(x => x.IsActive && !x.IsCancel && x.PrescriptionStatus != PrescriptionStatus.Cancelled);

            if (encounterId.HasValue && encounterId.Value != Guid.Empty)
                query = query.Where(x => x.EncounterId == encounterId.Value);
            if (consultationId.HasValue && consultationId.Value != Guid.Empty)
                query = query.Where(x => x.ConsultationId == consultationId.Value);
            if (patientId.HasValue && patientId.Value != Guid.Empty)
                query = query.Where(x => x.PatientId == patientId.Value);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x => x.PrescriptionNumber.ToLower().Contains(keyword));
            }

            var data = await query
                .OrderByDescending(x => x.PrescriptionDateTime)
                .Take(100)
                .Select(x => new PrescriptionOptionResponse
                {
                    Id = x.Id,
                    PrescriptionNumber = x.PrescriptionNumber,
                    ConsultationId = x.ConsultationId,
                    PrescriptionStatus = x.PrescriptionStatus,
                    PaymentStatus = x.PaymentStatus,
                    FulfillmentStatus = x.FulfillmentStatus,
                    PrescriptionDateTime = x.PrescriptionDateTime,
                    TotalItemCount = x.TotalItemCount,
                    TotalPrice = x.TotalPrice
                })
                .ToListAsync(cancellationToken);

            return Ok(ApiResponse<List<PrescriptionOptionResponse>>.Ok(
                data,
                "Data pilihan resep berhasil diambil."));
        }

        [HttpGet("active-by-consultation/{consultationId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PrescriptionDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Active Prescription", Description = "Melihat resep aktif berdasarkan konsultasi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Prescription", "Read")]
        public async Task<IActionResult> GetActiveByConsultation(
            Guid consultationId,
            CancellationToken cancellationToken = default)
        {
            var entity = await BuildBaseQuery()
                .AsNoTracking()
                .Where(x =>
                    x.ConsultationId == consultationId &&
                    x.IsActive &&
                    !x.IsCancel &&
                    x.PrescriptionStatus != PrescriptionStatus.Cancelled)
                .FirstOrDefaultAsync(cancellationToken);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Resep aktif untuk konsultasi ini tidak ditemukan."));

            return Ok(ApiResponse<PrescriptionDetailResponse>.Ok(ToDetailResponse(entity), "Resep aktif berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PrescriptionDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Prescription", Description = "Melihat detail resep", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Prescription", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await BuildBaseQuery().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Resep tidak ditemukan."));

            return Ok(ApiResponse<PrescriptionDetailResponse>.Ok(ToDetailResponse(entity), "Detail resep berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<PrescriptionCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Prescription", Description = "Membuat header resep", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("Prescription", "Create")]
        public async Task<IActionResult> CreatePrescription(
            [FromBody] CreatePrescriptionRequest request,
            CancellationToken cancellationToken = default)
        {
            var validation = await ValidateCreateRequestAsync(request, cancellationToken);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage ?? "Data resep tidak valid."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var consultation = await _dbContext.Set<TrxDoctorConsultation>()
                .AsNoTracking()
                .FirstAsync(x => x.Id == request.ConsultationId && x.EncounterId == request.EncounterId && !x.IsDelete, cancellationToken);

            var insuranceContext = await _encounterInsuranceService.GetContextAsync(
                request.EncounterId,
                request.PrescriptionDateTime ?? now,
                cancellationToken);

            if (!insuranceContext.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, insuranceContext.ErrorMessage ?? "Konteks pembayaran encounter tidak valid."));

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            var entity = new TrxPrescription
            {
                Id = Guid.NewGuid(),
                PrescriptionNumber = await _prescriptionNumberService.GenerateAsync(now, cancellationToken),
                EncounterId = consultation.EncounterId,
                ConsultationId = consultation.Id,
                PatientId = consultation.PatientId,
                DoctorId = consultation.DoctorId,
                ServiceUnitId = consultation.ServiceUnitId,
                ClinicId = consultation.ClinicId,
                PaymentSourceId = insuranceContext.PaymentSourceId,
                PatientInsuranceId = insuranceContext.PatientInsuranceId,
                InsuranceProviderId = insuranceContext.InsuranceProviderId,
                PaymentTypeSnapshot = insuranceContext.PaymentType,
                PatientClassNameSnapshot = insuranceContext.PatientClassName,
                PaymentSourceNameSnapshot = insuranceContext.PaymentSourceName,
                InsuranceProviderNameSnapshot = insuranceContext.InsuranceProviderName,
                PolicyNumberSnapshot = insuranceContext.PolicyNumber,
                BenefitPlanCodeSnapshot = insuranceContext.BenefitPlanCode,
                BenefitPlanNameSnapshot = insuranceContext.BenefitPlanName,
                PrescriptionStatus = PrescriptionStatus.Draft,
                PaymentStatus = PrescriptionPaymentStatus.NotBilled,
                FulfillmentStatus = PrescriptionFulfillmentStatus.WaitingForPayment,
                PrescriptionDateTime = request.PrescriptionDateTime ?? now,
                ClinicalNote = NormalizeNullableText(request.ClinicalNote),
                DoctorInstruction = NormalizeNullableText(request.DoctorInstruction),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<TrxPrescription>().Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var summary = await _prescriptionSummaryService.RebuildConsultationSummaryAsync(
                entity.ConsultationId,
                actorUserId,
                now,
                cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            var response = ToCreateResponse(entity, summary);

            await _loggerService.InfoAsync(LogCategory, "Prescription.CreatePrescription", "Membuat header resep dokter.", response);
            return Ok(ApiResponse<PrescriptionCreateResponse>.Ok(response, "Header resep berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PrescriptionUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Prescription", Description = "Mengubah header resep draft", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("Prescription", "Update")]
        public async Task<IActionResult> UpdatePrescription(Guid id, [FromBody] UpdatePrescriptionRequest request, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxPrescription>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Resep tidak ditemukan."));
            if (entity.PrescriptionStatus != PrescriptionStatus.Draft)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Hanya resep draft yang dapat diubah."));
            if (entity.PaymentStatus != PrescriptionPaymentStatus.NotBilled)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Resep yang sudah masuk billing tidak dapat diubah."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.PrescriptionDateTime = request.PrescriptionDateTime ?? entity.PrescriptionDateTime;
            entity.ClinicalNote = NormalizeNullableText(request.ClinicalNote);
            entity.DoctorInstruction = NormalizeNullableText(request.DoctorInstruction);
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            var summary = await _prescriptionSummaryService.RebuildConsultationSummaryAsync(entity.ConsultationId, actorUserId, now, cancellationToken);
            var response = ToUpdateResponse(entity, summary);
            await _loggerService.InfoAsync(LogCategory, "Prescription.UpdatePrescription", "Mengubah header resep draft.", response);
            return Ok(ApiResponse<PrescriptionUpdateResponse>.Ok(response, "Header resep berhasil diubah."));
        }

        // Resep tidak disubmit dari tab resep.
        // Finalisasi resep dilakukan otomatis melalui DoctorConsultationController.CompleteConsultation.

        [HttpPatch("{id:guid}/billing-generated")]
        [ProducesResponseType(typeof(ApiResponse<PrescriptionDetailResponse>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Mark Prescription Billing", Description = "Menandai billing resep sudah dibuat", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("Prescription", "Update")]
        public async Task<IActionResult> MarkBillingGenerated(Guid id, [FromBody] MarkPrescriptionBillingGeneratedRequest request, CancellationToken cancellationToken = default)
        {
            var entity = await GetWorkflowEntityAsync(id, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Resep tidak ditemukan."));

            var now = DateTime.UtcNow;
            var result = await _prescriptionWorkflowService.MarkBillingGeneratedAsync(entity, request.BillingId, GetCurrentUserId(), now, cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, result.ErrorMessage ?? "Billing resep tidak dapat diproses."));

            await _loggerService.InfoAsync(LogCategory, "Prescription.MarkBillingGenerated", "Menandai billing resep sudah dibuat.", new { id, request.BillingId });
            return Ok(ApiResponse<PrescriptionDetailResponse>.Ok(ToDetailResponse(await ReloadAsync(id, cancellationToken)), "Billing resep berhasil ditandai."));
        }

        [HttpPatch("{id:guid}/payment-paid")]
        [ProducesResponseType(typeof(ApiResponse<PrescriptionDetailResponse>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Mark Prescription Paid", Description = "Menandai pembayaran resep lunas", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("Prescription", "Update")]
        public Task<IActionResult> MarkPaid(Guid id, [FromBody] MarkPrescriptionPaymentCompletedRequest request, CancellationToken cancellationToken = default)
            => CompletePayment(id, request, "paid", cancellationToken);

        [HttpPatch("{id:guid}/insurance-approved")]
        [ProducesResponseType(typeof(ApiResponse<PrescriptionDetailResponse>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Approve Prescription Insurance", Description = "Menandai resep disetujui asuransi", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("Prescription", "Update")]
        public Task<IActionResult> MarkInsuranceApproved(Guid id, [FromBody] MarkPrescriptionPaymentCompletedRequest request, CancellationToken cancellationToken = default)
            => CompletePayment(id, request, "insurance", cancellationToken);

        [HttpPatch("{id:guid}/payment-waived")]
        [ProducesResponseType(typeof(ApiResponse<PrescriptionDetailResponse>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Waive Prescription Payment", Description = "Menandai pembayaran resep ditiadakan", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("Prescription", "Update")]
        public Task<IActionResult> MarkPaymentWaived(Guid id, [FromBody] MarkPrescriptionPaymentCompletedRequest request, CancellationToken cancellationToken = default)
            => CompletePayment(id, request, "waived", cancellationToken);

        [HttpPatch("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Cancel Prescription", Description = "Membatalkan resep", AccessType = AccessTypes.Update, SortOrder = 7)]
        [AccessPermission("Prescription", "Update")]
        public async Task<IActionResult> CancelPrescription(Guid id, [FromBody] CancelPrescriptionRequest request, CancellationToken cancellationToken = default)
        {
            var entity = await GetWorkflowEntityAsync(id, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Resep tidak ditemukan."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var result = await _prescriptionWorkflowService.CancelAsync(entity, request.CancelReason, actorUserId, now, cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, result.ErrorMessage ?? "Resep tidak dapat dibatalkan."));

            await _prescriptionSummaryService.RebuildConsultationSummaryAsync(entity.ConsultationId, actorUserId, now, cancellationToken);
            await _loggerService.InfoAsync(LogCategory, "Prescription.CancelPrescription", "Membatalkan resep.", new { id, request.CancelReason });
            return Ok(ApiResponse<object>.Ok(null, "Resep berhasil dibatalkan."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction("Delete", "Delete Prescription", Description = "Menghapus resep draft kosong", AccessType = AccessTypes.Delete, SortOrder = 8)]
        [AccessPermission("Prescription", "Delete")]
        public async Task<IActionResult> DeletePrescription(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxPrescription>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Resep tidak ditemukan."));
            if (!_prescriptionWorkflowService.CanDelete(entity))
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Hanya resep draft kosong yang belum masuk billing yang dapat dihapus."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.IsDelete = true;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.IsActive = false;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
            await _prescriptionSummaryService.RebuildConsultationSummaryAsync(entity.ConsultationId, actorUserId, now, cancellationToken);
            return Ok(ApiResponse<object>.Ok(null, "Resep draft berhasil dihapus."));
        }

        private async Task<IActionResult> CompletePayment(
            Guid id,
            MarkPrescriptionPaymentCompletedRequest request,
            string mode,
            CancellationToken cancellationToken)
        {
            var entity = await GetWorkflowEntityAsync(id, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Resep tidak ditemukan."));

            var now = request?.CompletedAt ?? DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            PrescriptionWorkflowResult result = mode switch
            {
                "insurance" => await _prescriptionWorkflowService.MarkInsuranceApprovedAsync(entity, actorUserId, now, cancellationToken),
                "waived" => await _prescriptionWorkflowService.MarkPaymentWaivedAsync(entity, actorUserId, now, cancellationToken),
                _ => await _prescriptionWorkflowService.MarkPaidAsync(entity, actorUserId, now, cancellationToken)
            };

            if (!result.IsSuccess)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, result.ErrorMessage ?? "Pembayaran resep tidak dapat diselesaikan."));

            await _loggerService.InfoAsync(LogCategory, "Prescription.CompletePayment", "Menyelesaikan pembayaran resep dan membuka proses farmasi.", new { id, mode, now });
            return Ok(ApiResponse<PrescriptionDetailResponse>.Ok(ToDetailResponse(await ReloadAsync(id, cancellationToken)), "Pembayaran resep selesai dan resep siap diproses farmasi."));
        }

        private IQueryable<TrxPrescription> BuildBaseQuery()
        {
            return _dbContext.Set<TrxPrescription>()
                .Include(x => x.Encounter)
                .Include(x => x.Consultation)
                .Include(x => x.Patient)
                .Include(x => x.Doctor)
                .Include(x => x.ServiceUnit)
                .Include(x => x.Clinic)
                .Include(x => x.PaymentSource)
                .Include(x => x.PatientInsurance)
                .Include(x => x.InsuranceProvider)
                .Include(x => x.SubmittedByUser)
                .Include(x => x.PaymentCompletedByUser)
                .Include(x => x.PharmacyVerifiedByUser)
                .Include(x => x.DispensedByUser)
                .Include(x => x.CancelledByUser)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<TrxPrescription> ApplyFilters(
            IQueryable<TrxPrescription> query,
            string? search,
            Guid? encounterId,
            Guid? consultationId,
            Guid? patientId,
            Guid? doctorId,
            Guid? serviceUnitId,
            Guid? clinicId,
            PrescriptionStatus? prescriptionStatus,
            PrescriptionPaymentStatus? paymentStatus,
            PrescriptionFulfillmentStatus? fulfillmentStatus,
            bool? isNeedApproval,
            bool? isApproved,
            DateTime? startDate,
            DateTime? endDate)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.PrescriptionNumber.ToLower().Contains(keyword) ||
                    (x.Encounter != null && x.Encounter.EncounterNumber.ToLower().Contains(keyword)) ||
                    (x.Consultation != null && x.Consultation.ConsultationNumber.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.FullName.ToLower().Contains(keyword)) ||
                    (x.Patient != null && x.Patient.MedicalRecordNumber.ToLower().Contains(keyword)) ||
                    (x.Doctor != null && x.Doctor.FullName.ToLower().Contains(keyword)));
            }

            if (encounterId.HasValue && encounterId.Value != Guid.Empty) query = query.Where(x => x.EncounterId == encounterId.Value);
            if (consultationId.HasValue && consultationId.Value != Guid.Empty) query = query.Where(x => x.ConsultationId == consultationId.Value);
            if (patientId.HasValue && patientId.Value != Guid.Empty) query = query.Where(x => x.PatientId == patientId.Value);
            if (doctorId.HasValue && doctorId.Value != Guid.Empty) query = query.Where(x => x.DoctorId == doctorId.Value);
            if (serviceUnitId.HasValue && serviceUnitId.Value != Guid.Empty) query = query.Where(x => x.ServiceUnitId == serviceUnitId.Value);
            if (clinicId.HasValue && clinicId.Value != Guid.Empty) query = query.Where(x => x.ClinicId == clinicId.Value);
            if (prescriptionStatus.HasValue) query = query.Where(x => x.PrescriptionStatus == prescriptionStatus.Value);
            if (paymentStatus.HasValue) query = query.Where(x => x.PaymentStatus == paymentStatus.Value);
            if (fulfillmentStatus.HasValue) query = query.Where(x => x.FulfillmentStatus == fulfillmentStatus.Value);
            if (isNeedApproval.HasValue) query = query.Where(x => x.IsNeedApproval == isNeedApproval.Value);
            if (isApproved.HasValue) query = query.Where(x => x.IsApproved == isApproved.Value);
            if (startDate.HasValue) query = query.Where(x => x.PrescriptionDateTime >= startDate.Value.Date);
            if (endDate.HasValue) query = query.Where(x => x.PrescriptionDateTime < endDate.Value.Date.AddDays(1));
            return query;
        }

        private static IQueryable<TrxPrescription> ApplySorting(IQueryable<TrxPrescription> query, string? sortBy, string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "prescriptionDateTime").ToLowerInvariant() switch
            {
                "prescriptionnumber" => isDesc ? query.OrderByDescending(x => x.PrescriptionNumber) : query.OrderBy(x => x.PrescriptionNumber),
                "prescriptionstatus" => isDesc ? query.OrderByDescending(x => x.PrescriptionStatus) : query.OrderBy(x => x.PrescriptionStatus),
                "paymentstatus" => isDesc ? query.OrderByDescending(x => x.PaymentStatus) : query.OrderBy(x => x.PaymentStatus),
                "fulfillmentstatus" => isDesc ? query.OrderByDescending(x => x.FulfillmentStatus) : query.OrderBy(x => x.FulfillmentStatus),
                "totalitemcount" => isDesc ? query.OrderByDescending(x => x.TotalItemCount) : query.OrderBy(x => x.TotalItemCount),
                "totalprice" => isDesc ? query.OrderByDescending(x => x.TotalPrice) : query.OrderBy(x => x.TotalPrice),
                "patientpayamount" => isDesc ? query.OrderByDescending(x => x.PatientPayAmount) : query.OrderBy(x => x.PatientPayAmount),
                "createdatetime" => isDesc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                _ => isDesc ? query.OrderByDescending(x => x.PrescriptionDateTime) : query.OrderBy(x => x.PrescriptionDateTime)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateCreateRequestAsync(CreatePrescriptionRequest request, CancellationToken cancellationToken)
        {
            var consultation = await _dbContext.Set<TrxDoctorConsultation>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.ConsultationId && x.EncounterId == request.EncounterId && !x.IsDelete, cancellationToken);
            if (consultation == null) return (false, "Konsultasi dokter tidak ditemukan atau tidak sesuai encounter.");
            if (consultation.ConsultationStatus == DoctorConsultationStatus.Completed) return (false, "Konsultasi yang sudah completed tidak dapat ditambahkan resep.");
            if (consultation.ConsultationStatus == DoctorConsultationStatus.Cancelled) return (false, "Konsultasi yang sudah cancelled tidak dapat ditambahkan resep.");

            var exists = await _dbContext.Set<TrxPrescription>().AsNoTracking().AnyAsync(x =>
                x.ConsultationId == request.ConsultationId && !x.IsDelete && !x.IsCancel && x.PrescriptionStatus != PrescriptionStatus.Cancelled,
                cancellationToken);
            return exists ? (false, "Konsultasi ini sudah memiliki resep aktif.") : (true, null);
        }

        private async Task<TrxPrescription?> GetWorkflowEntityAsync(Guid id, CancellationToken cancellationToken)
            => await _dbContext.Set<TrxPrescription>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

        private async Task<TrxPrescription> ReloadAsync(Guid id, CancellationToken cancellationToken)
            => await BuildBaseQuery().AsNoTracking().FirstAsync(x => x.Id == id, cancellationToken);

        private static PrescriptionResponse ToResponse(TrxPrescription x)
        {
            return new PrescriptionResponse
            {
                Id = x.Id,
                PrescriptionNumber = x.PrescriptionNumber,
                EncounterId = x.EncounterId,
                EncounterNumber = x.Encounter?.EncounterNumber ?? string.Empty,
                ConsultationId = x.ConsultationId,
                ConsultationNumber = x.Consultation?.ConsultationNumber ?? string.Empty,
                PatientId = x.PatientId,
                PatientName = x.Patient?.FullName ?? string.Empty,
                MedicalRecordNumber = x.Patient?.MedicalRecordNumber ?? string.Empty,
                DoctorId = x.DoctorId,
                DoctorName = x.Doctor?.FullName ?? string.Empty,
                ServiceUnitId = x.ServiceUnitId,
                ServiceUnitName = x.ServiceUnit?.ServiceUnitName ?? string.Empty,
                ClinicId = x.ClinicId,
                ClinicName = x.Clinic?.ClinicName,
                PaymentTypeSnapshot = x.PaymentTypeSnapshot,
                PaymentTypeName = GetDisplayName(x.PaymentTypeSnapshot),
                PaymentSourceNameSnapshot = x.PaymentSourceNameSnapshot,
                InsuranceProviderNameSnapshot = x.InsuranceProviderNameSnapshot,
                BenefitPlanNameSnapshot = x.BenefitPlanNameSnapshot,
                PatientClassNameSnapshot = x.PatientClassNameSnapshot,
                PrescriptionStatus = x.PrescriptionStatus,
                PaymentStatus = x.PaymentStatus,
                FulfillmentStatus = x.FulfillmentStatus,
                PrescriptionDateTime = x.PrescriptionDateTime,
                RegularItemCount = x.RegularItemCount,
                CompoundCount = x.CompoundCount,
                CompoundIngredientCount = x.CompoundIngredientCount,
                TotalItemCount = x.TotalItemCount,
                TotalPrice = x.TotalPrice,
                CoveredAmount = x.CoveredAmount,
                PatientPayAmount = x.PatientPayAmount,
                IsNeedApproval = x.IsNeedApproval,
                IsApproved = x.IsApproved,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime
            };
        }

        private static PrescriptionDetailResponse ToDetailResponse(TrxPrescription x)
        {
            var response = new PrescriptionDetailResponse
            {
                PaymentSourceId = x.PaymentSourceId,
                PatientInsuranceId = x.PatientInsuranceId,
                InsuranceProviderId = x.InsuranceProviderId,
                PolicyNumberSnapshot = x.PolicyNumberSnapshot,
                BenefitPlanCodeSnapshot = x.BenefitPlanCodeSnapshot,
                ClinicalNote = x.ClinicalNote,
                DoctorInstruction = x.DoctorInstruction,
                PharmacyNote = x.PharmacyNote,
                SubmittedAt = x.SubmittedAt,
                SubmittedByUserId = x.SubmittedByUserId,
                SubmittedByUserName = x.SubmittedByUser?.DisplayName,
                BillingId = x.BillingId,
                BillingGeneratedAt = x.BillingGeneratedAt,
                PaymentCompletedAt = x.PaymentCompletedAt,
                PaymentCompletedByUserId = x.PaymentCompletedByUserId,
                PaymentCompletedByUserName = x.PaymentCompletedByUser?.DisplayName,
                ReadyForPharmacyAt = x.ReadyForPharmacyAt,
                PharmacyQueueId = x.PharmacyQueueId,
                PharmacyQueuedAt = x.PharmacyQueuedAt,
                PharmacyVerifiedAt = x.PharmacyVerifiedAt,
                PharmacyVerifiedByUserId = x.PharmacyVerifiedByUserId,
                PharmacyVerifiedByUserName = x.PharmacyVerifiedByUser?.DisplayName,
                PreparationStartedAt = x.PreparationStartedAt,
                ReadyToDispenseAt = x.ReadyToDispenseAt,
                DispensedAt = x.DispensedAt,
                DispensedByUserId = x.DispensedByUserId,
                DispensedByUserName = x.DispensedByUser?.DisplayName,
                CancelledAt = x.CancelledAt,
                CancelledByUserId = x.CancelledByUserId,
                CancelledByUserName = x.CancelledByUser?.DisplayName,
                CancelReason = x.CancelReason
            };
            CopyBaseResponse(x, response);
            return response;
        }

        private static void CopyBaseResponse(TrxPrescription x, PrescriptionResponse response)
        {
            var b = ToResponse(x);
            foreach (var property in typeof(PrescriptionResponse).GetProperties().Where(p => p.CanRead && p.CanWrite))
                property.SetValue(response, property.GetValue(b));
        }

        private static PrescriptionCreateResponse ToCreateResponse(TrxPrescription x, PrescriptionSummaryResult summary)
            => new()
            {
                Id = x.Id,
                PrescriptionNumber = x.PrescriptionNumber,
                EncounterId = x.EncounterId,
                ConsultationId = x.ConsultationId,
                PrescriptionStatus = x.PrescriptionStatus,
                PaymentStatus = x.PaymentStatus,
                FulfillmentStatus = x.FulfillmentStatus,
                PrescriptionDateTime = x.PrescriptionDateTime,
                TotalItemCount = x.TotalItemCount,
                TotalPrice = x.TotalPrice,
                CoveredAmount = x.CoveredAmount,
                PatientPayAmount = x.PatientPayAmount,
                HasPrescription = summary.HasPrescription,
                PrescriptionCount = summary.PrescriptionCount,
                PrescriptionText = summary.PrescriptionText
            };

        private static PrescriptionUpdateResponse ToUpdateResponse(TrxPrescription x, PrescriptionSummaryResult summary)
            => new()
            {
                Id = x.Id,
                PrescriptionNumber = x.PrescriptionNumber,
                EncounterId = x.EncounterId,
                ConsultationId = x.ConsultationId,
                PrescriptionStatus = x.PrescriptionStatus,
                PaymentStatus = x.PaymentStatus,
                FulfillmentStatus = x.FulfillmentStatus,
                PrescriptionDateTime = x.PrescriptionDateTime,
                TotalItemCount = x.TotalItemCount,
                TotalPrice = x.TotalPrice,
                CoveredAmount = x.CoveredAmount,
                PatientPayAmount = x.PatientPayAmount,
                HasPrescription = summary.HasPrescription,
                PrescriptionCount = summary.PrescriptionCount,
                PrescriptionText = summary.PrescriptionText
            };

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 25;
            if (pageSize > 100) pageSize = 100;
            return (pageNumber, pageSize);
        }

        private static List<PrescriptionEnumOptionResponse> BuildEnumOptions<TEnum>() where TEnum : Enum
            => Enum.GetValues(typeof(TEnum)).Cast<TEnum>().Select(x => new PrescriptionEnumOptionResponse
            {
                Value = Convert.ToInt32(x),
                Name = x.ToString(),
                Label = GetDisplayName(x)
            }).ToList();

        private static string GetDisplayName<TEnum>(TEnum value) where TEnum : Enum
        {
            var member = typeof(TEnum).GetMember(value.ToString()).FirstOrDefault();
            return member?.GetCustomAttribute<DisplayAttribute>()?.Name ?? value.ToString();
        }

        private static string? NormalizeNullableText(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private Guid GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userId, out var id) ? id : Guid.Empty;
        }
    }
}
