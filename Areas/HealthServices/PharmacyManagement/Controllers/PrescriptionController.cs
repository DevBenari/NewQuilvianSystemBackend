using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Constants;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
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
        private readonly ClinicalMilestoneFactProducer _clinicalMilestoneFactProducer;
        private readonly LoggerService _loggerService;

        public PrescriptionController(
            ApplicationDbContext dbContext,
            EncounterInsuranceService encounterInsuranceService,
            PrescriptionNumberService prescriptionNumberService,
            PrescriptionSummaryService prescriptionSummaryService,
            PrescriptionWorkflowService prescriptionWorkflowService,
            ClinicalMilestoneFactProducer clinicalMilestoneFactProducer,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _encounterInsuranceService = encounterInsuranceService;
            _prescriptionNumberService = prescriptionNumberService;
            _prescriptionSummaryService = prescriptionSummaryService;
            _prescriptionWorkflowService = prescriptionWorkflowService;
            _clinicalMilestoneFactProducer = clinicalMilestoneFactProducer;
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
            // BE-RWI-050 kriteria 3. Kiriman ulang dengan kunci yang sama mengembalikan resep
            // yang sudah ada, bukan resep kedua. Diperiksa PALING AWAL supaya percobaan ulang
            // tidak menyentuh konteks pembayaran maupun penomoran resep sama sekali.
            var kunciPermintaan = NormalizeNullableText(request.IdempotencyKey);

            if (kunciPermintaan != null)
            {
                var sudahAda = await _dbContext.Set<TrxPrescription>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.IdempotencyKey == kunciPermintaan && !x.IsDelete,
                                         cancellationToken);

                if (sudahAda != null)
                {
                    var ringkasanUlang = await _prescriptionSummaryService.ReadConsultationSummaryAsync(
                        sudahAda.ConsultationId, cancellationToken);

                    return Ok(ApiResponse<PrescriptionCreateResponse>.Ok(
                        ToCreateResponse(sudahAda, ringkasanUlang),
                        "Resep sudah tercatat sebelumnya dengan kunci permintaan yang sama."));
                }
            }

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
                // BE-RWI-042 dan BE-RWI-043. Konteks perawatan diwarisi dari catatan dokternya,
                // bukan ditanyakan ulang: resep memang lahir dari satu catatan, dan mewarisinya
                // membuat resep tidak pernah menunjuk perawatan yang berbeda dari catatannya.
                InpEpisodeId = consultation.InpEpisodeId,
                // BE-RWI-050. Jenis resep dan kunci permintaan distempel saat resep lahir.
                PrescriptionOrderType = request.PrescriptionOrderType,
                IdempotencyKey = kunciPermintaan,
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

        /// <summary>
        /// Seluruh resep satu perawatan rawat inap beserta keadaan pemenuhannya.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>BE-RWI-050</c> kriteria 1, 2, dan 4; <c>api-contract.md</c> bagian 6. Pasien yang
        /// dirawat lima hari menerima resep setiap hari, dan seluruhnya terbaca di sini terurut
        /// waktu penulisan.
        /// </para>
        /// <para>
        /// <b>Keadaan pemenuhan hanya dibaca.</b> Tidak ada satu pun endpoint pada controller
        /// ini yang mengubahnya - <c>RUL-DOK-01</c>. Menandai obat sudah diserahkan adalah
        /// kewenangan petugas Farmasi lewat permukaannya sendiri.
        /// </para>
        /// </remarks>
        [HttpGet("episodes/{episodeId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ResponsePrescriptionPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Prescription", Description = "Melihat resep satu perawatan rawat inap beserta status pemenuhannya", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Prescription", "Read")]
        public async Task<IActionResult> GetByEpisode(
            Guid episodeId,
            [FromQuery] PrescriptionOrderType? orderType = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery()
                .AsNoTracking()
                .Where(x => x.InpEpisodeId == episodeId && !x.IsDelete);

            if (orderType.HasValue)
                query = query.Where(x => x.PrescriptionOrderType == orderType.Value);

            var totalData = await query.CountAsync(cancellationToken);

            var entities = await query
                .OrderBy(x => x.PrescriptionDateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var hasil = new ResponsePrescriptionPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = pageSize == 0 ? 0 : (int)Math.Ceiling(totalData / (double)pageSize),
                Items = entities.Select(ToResponse).ToList()
            };

            return Ok(ApiResponse<ResponsePrescriptionPagedResult>.Ok(
                hasil, "Resep perawatan rawat inap berhasil diambil."));
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

        // RJ-BIL-BE-002 / RJ-BIL-CONFLICT-006 keputusan author 1A.
        //
        // Empat endpoint berikut dihapus dari modul klinis:
        //
        //   PATCH {id}/billing-generated
        //   PATCH {id}/payment-paid
        //   PATCH {id}/insurance-approved
        //   PATCH {id}/payment-waived
        //
        // Keempatnya menetapkan status finansial canonical dengan hak akses klinis
        // Prescription : Update. Penelusuran repository dan frontend commit 29422c8
        // menemukan nol konsumen. Route dihapus seluruhnya, bukan sekadar disembunyikan
        // dari Swagger, agar tidak menyisakan jalur bypass kewenangan finansial.
        //
        // Status finansial resep kini hanya berasal dari Billing.

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

            // Pembatalan klinis sudah tersimpan. Billing diberi tahu setelahnya, sehingga
            // gangguan pada Billing tidak membatalkan keputusan klinis dokter.
            var emission = await _clinicalMilestoneFactProducer.EmitClinicalCancellationAsync(
                new ClinicalMilestoneFactRequest
                {
                    SourceContext = BillingSourceContract.PrescriptionSourceContext,
                    SourceAggregateId = entity.Id,
                    EffectType = BillingSourceContract.PrescriptionChargeEffectType,
                    EncounterId = entity.EncounterId,
                    OccurredAt = now,
                    CorrelationId = entity.ConsultationId
                },
                actorUserId,
                cancellationToken);

            await _loggerService.InfoAsync(LogCategory, "Prescription.CancelPrescription", "Membatalkan resep.", new
            {
                id,
                request.CancelReason,
                BillingHandoff = emission.Kind.ToString(),
                emission.MilestoneFactVersion
            });

            return Ok(ApiResponse<object>.Ok(
                new { BillingHandoff = emission.Kind.ToString() },
                emission.IsClinicallySafe
                    ? "Resep berhasil dibatalkan."
                    : "Resep berhasil dibatalkan, tetapi penyerahan fakta ke Billing memerlukan tinjauan."));
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

            // BE-RWI-043 / INT-DOK-02. Batas satu resep aktif per catatan tidak berlaku bagi
            // catatan yang menempel pada perawatan rawat inap: pasien yang dirawat berhari-hari
            // menerima resep sebanyak yang dibutuhkan - RWI-DEC-070, RWI-RULE-026 aturan 5.
            //
            // Penyaringnya adalah konteks perawatan pada catatan dokternya, bukan nama peran
            // maupun tipe kunjungan yang ditebak. Resep rawat jalan dan medical check-up tetap
            // ditolak dengan kalimat yang sama persis - INV-DOK-05, RWI-AC-143.
            if (consultation.InpEpisodeId.HasValue)
                return (true, null);

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
                // BE-RWI-050. Jenis resep dan penanda perawatan ikut dibaca, supaya layar
                // farmasi dapat menyaring obat pulang dan layar dokter dapat membaca seluruh
                // resep satu perawatan.
                PrescriptionOrderType = x.PrescriptionOrderType,
                InpEpisodeId = x.InpEpisodeId,
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
                InpEpisodeId = x.InpEpisodeId,
                PrescriptionOrderType = x.PrescriptionOrderType,
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
