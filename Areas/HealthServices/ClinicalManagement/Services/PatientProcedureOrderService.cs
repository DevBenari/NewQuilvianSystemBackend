using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services
{
    /// <summary>
    /// Service pengelola pesanan tindakan rawat inap, aturan penginput/DPJP (INV-DOK-17),
    /// serta pembatalan otomatis saat penutupan episode (RWI-DEC-143, BE-RWI-097).
    /// </summary>
    public class PatientProcedureOrderService
    {
        private const string LogCategory = "ClinicalManagement.PatientProcedureOrderService";

        private readonly ApplicationDbContext _dbContext;
        private readonly InpatientClinicalContextService _clinicalContextService;
        private readonly ILoggerService _loggerService;

        public PatientProcedureOrderService(
            ApplicationDbContext dbContext,
            InpatientClinicalContextService clinicalContextService,
            ILoggerService loggerService)
        {
            _dbContext = dbContext;
            _clinicalContextService = clinicalContextService;
            _loggerService = loggerService;
        }

        /// <summary>
        /// Membuat pesanan tindakan rawat inap oleh dokter atau perawat (FR-DOK-100 / BE-RWI-097).
        /// Perawat wajib menyebut dokter pemberi instruksi yang aktif bertugas atas pasien.
        /// </summary>
        public async Task<PatientProcedureOrderResult> CreateInpatientOrderAsync(
            CreateInpatientProcedureOrderRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (request.Quantity <= 0)
            {
                return PatientProcedureOrderResult.BadRequest("Quantity tindakan harus lebih dari 0.");
            }

            var idempotencyKey = request.IdempotencyKey?.Trim();
            if (!string.IsNullOrEmpty(idempotencyKey))
            {
                var existing = await _dbContext.Set<TrxPatientProcedure>()
                    .AsNoTracking()
                    .Include(x => x.Procedure)
                    .Include(x => x.Doctor)
                    .Include(x => x.InstructingDoctor)
                    .Include(x => x.OrderedByUser)
                    .FirstOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey && !x.IsDelete, cancellationToken);

                if (existing != null)
                {
                    return PatientProcedureOrderResult.Success(
                        MapToResponse(existing),
                        "Pesanan tindakan sudah tercatat sebelumnya dengan kunci permintaan yang sama.");
                }
            }

            var episode = await _dbContext.Set<InpEpisode>()
                .Include(x => x.Encounter)
                .FirstOrDefaultAsync(x => x.Id == request.InpEpisodeId && !x.IsDelete, cancellationToken);

            if (episode == null)
            {
                return PatientProcedureOrderResult.NotFound("Episode rawat inap tidak ditemukan.");
            }

            if (episode.EpisodeStatus == InpEpisodeStatus.Closed || episode.EpisodeStatus == InpEpisodeStatus.Cancelled)
            {
                return PatientProcedureOrderResult.UnprocessableEntity(
                    "Perawatan rawat inap pasien ini sudah ditutup, sehingga tindakan baru tidak dapat dicatat lagi.");
            }

            if (episode.EpisodeStatus == InpEpisodeStatus.Draft)
            {
                return PatientProcedureOrderResult.BadRequest(
                    "Perawatan rawat inap masih berstatus Draft; pasien belum teradmisi.");
            }

            var now = DateTime.UtcNow;
            var doctorId = await _clinicalContextService.ResolveActorDoctorIdAsync(actorUserId, cancellationToken);

            Guid orderDoctorId;
            Guid? instructingDoctorId = null;
            var verificationStatus = PatientProcedureInstructionVerificationStatus.NotRequired;

            if (doctorId.HasValue)
            {
                var isDoctorAssigned = await _clinicalContextService.IsDoctorAssignedAsync(
                    episode.Id, doctorId.Value, now, cancellationToken);

                if (!isDoctorAssigned)
                {
                    return PatientProcedureOrderResult.Forbidden(
                        "Dokter tidak memiliki penugasan aktif pada perawatan pasien ini.");
                }

                orderDoctorId = doctorId.Value;
                instructingDoctorId = null;
                verificationStatus = PatientProcedureInstructionVerificationStatus.NotRequired;
            }
            else
            {
                if (!request.InstructingDoctorId.HasValue || request.InstructingDoctorId.Value == Guid.Empty)
                {
                    return PatientProcedureOrderResult.BadRequest(
                        "Dokter pemberi instruksi wajib dipilih untuk pesanan tindakan oleh perawat.");
                }

                var isInstructingDoctorAssigned = await _clinicalContextService.IsDoctorAssignedAsync(
                    episode.Id, request.InstructingDoctorId.Value, now, cancellationToken);

                if (!isInstructingDoctorAssigned)
                {
                    return PatientProcedureOrderResult.Forbidden(
                        "Dokter pemberi instruksi yang dipilih tidak sedang bertugas atas pasien ini.");
                }

                orderDoctorId = request.InstructingDoctorId.Value;
                instructingDoctorId = request.InstructingDoctorId.Value;
                verificationStatus = PatientProcedureInstructionVerificationStatus.Pending;
            }

            var procedure = await _dbContext.Set<MstProcedure>()
                .FirstOrDefaultAsync(x => x.Id == request.ProcedureId && !x.IsDelete, cancellationToken);

            if (procedure == null)
            {
                return PatientProcedureOrderResult.NotFound("Master tindakan tidak ditemukan.");
            }

            var entity = new TrxPatientProcedure
            {
                Id = Guid.NewGuid(),
                EncounterId = episode.EncounterId,
                ConsultationId = null, // Dilonggarkan sejak R7 (BE-RWI-097)
                PatientId = episode.PatientId,
                DoctorId = orderDoctorId,
                ServiceUnitId = episode.ServiceUnitId,
                ClinicId = null,
                InpEpisodeId = episode.Id,
                PhysicianVisitId = null,
                IdempotencyKey = idempotencyKey,
                ProcedureId = procedure.Id,
                TariffId = null,
                InsuranceTariffId = null,
                InsuranceCoverageRuleId = null,
                ProcedureCodeSnapshot = procedure.ProcedureCode ?? string.Empty,
                ProcedureNameSnapshot = procedure.ProcedureName ?? string.Empty,
                ProcedureTypeSnapshot = procedure.ProcedureType,
                ProcedureMasterType = "Master",
                IsFromMasterProcedure = true,
                IsPrimaryProcedure = request.IsPrimaryProcedure,
                IsEmergencyProcedure = request.IsEmergencyProcedure,
                IsSurgeryRelated = false,
                IsPackageProcedure = false,
                ProcedureSource = PatientProcedureSource.DoctorOrder,
                ProcedureStatus = PatientProcedureStatus.Ordered,
                ProcedureDateTime = now,
                PlannedAt = now,
                Quantity = request.Quantity,
                UnitNameSnapshot = "Tindakan",
                UnitPrice = procedure.Price,
                TotalPrice = procedure.Price * request.Quantity,
                HospitalPriceSnapshot = procedure.Price,
                IsFreeOfCharge = false,
                IsBillable = true,
                IsCoveredByInsurance = false,
                CoverageStatus = "Unknown",
                CoveragePercent = 0,
                CoveredAmount = 0,
                PatientPayAmount = procedure.Price * request.Quantity,
                IsNeedApproval = false,
                IsApproved = false,
                IsExecuted = false,
                ClinicalNote = request.ClinicalReason,
                InstructionNote = request.InstructionNote,
                IsBillingGenerated = false,
                OrderedByUserId = actorUserId,
                InstructingDoctorId = instructingDoctorId,
                InstructionVerificationStatus = verificationStatus,
                CancelledByEpisodeClosure = false,
                IsActive = true,
                CreateBy = actorUserId,
                CreateDateTime = now
            };

            _dbContext.Set<TrxPatientProcedure>().Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "PatientProcedureOrderService.CreateInpatientOrderAsync",
                "Berhasil membuat pesanan tindakan rawat inap.",
                new
                {
                    entity.Id,
                    entity.InpEpisodeId,
                    entity.DoctorId,
                    entity.InstructingDoctorId,
                    VerificationStatus = entity.InstructionVerificationStatus.ToString(),
                    entity.OrderedByUserId
                });

            return PatientProcedureOrderResult.Success(
                MapToResponse(entity),
                "Pesanan tindakan rawat inap berhasil dibuat.");
        }

        /// <summary>
        /// Mengubah pesanan tindakan yang belum dilaksanakan (INV-DOK-17).
        /// Hanya penginput asli yang berhak mengubah.
        /// </summary>
        public async Task<PatientProcedureOrderResult> UpdateOrderAsync(
            Guid id,
            UpdatePatientProcedureRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxPatientProcedure>()
                .Include(x => x.Consultation)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
            {
                return PatientProcedureOrderResult.NotFound("Tindakan pasien tidak ditemukan.");
            }

            // INV-DOK-17: Pesanan tindakan yang belum dilaksanakan hanya diubah penginputnya.
            if (entity.OrderedByUserId.HasValue && entity.OrderedByUserId.Value != actorUserId)
            {
                return PatientProcedureOrderResult.Forbidden(
                    "Hanya penginput pesanan ini yang dapat mengubah pesanan.");
            }

            if (entity.InpEpisodeId.HasValue)
            {
                var episode = await _dbContext.Set<InpEpisode>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == entity.InpEpisodeId.Value && !x.IsDelete, cancellationToken);

                if (episode != null && (episode.EpisodeStatus == InpEpisodeStatus.Closed || episode.EpisodeStatus == InpEpisodeStatus.Cancelled))
                {
                    return PatientProcedureOrderResult.UnprocessableEntity(
                        "Perawatan rawat inap sudah ditutup; tindakan tidak dapat diubah.");
                }
            }

            if (entity.IsBillingGenerated)
            {
                return PatientProcedureOrderResult.BadRequest("Tindakan yang sudah masuk billing tidak dapat diubah.");
            }

            if (entity.IsExecuted)
            {
                return PatientProcedureOrderResult.BadRequest("Tindakan yang sudah dieksekusi tidak dapat diubah.");
            }

            if (entity.ProcedureStatus == PatientProcedureStatus.Cancelled)
            {
                return PatientProcedureOrderResult.BadRequest("Tindakan yang sudah dibatalkan tidak dapat diubah.");
            }

            if (request.Quantity <= 0)
            {
                return PatientProcedureOrderResult.BadRequest("Quantity tindakan harus lebih dari 0.");
            }

            var now = DateTime.UtcNow;
            entity.Quantity = request.Quantity;
            entity.TotalPrice = entity.UnitPrice * request.Quantity;
            entity.PatientPayAmount = entity.TotalPrice;
            entity.IsPrimaryProcedure = request.IsPrimaryProcedure;
            entity.IsEmergencyProcedure = request.IsEmergencyProcedure;
            entity.ClinicalNote = request.ClinicalNote;
            entity.InstructionNote = request.InstructionNote;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return PatientProcedureOrderResult.Success(
                MapToResponse(entity),
                "Pesanan tindakan berhasil diperbarui.");
        }

        /// <summary>
        /// Membatalkan pesanan tindakan (FR-DOK-102 / INV-DOK-17).
        /// Hanya penginput atau DPJP aktif pada episode rawat inap yang berwenang membatalkan.
        /// </summary>
        public async Task<PatientProcedureOrderResult> CancelOrderAsync(
            Guid id,
            string reason,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                return PatientProcedureOrderResult.BadRequest("Alasan pembatalan tindakan wajib diisi.");
            }

            var entity = await _dbContext.Set<TrxPatientProcedure>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
            {
                return PatientProcedureOrderResult.NotFound("Tindakan pasien tidak ditemukan.");
            }

            if (entity.IsBillingGenerated)
            {
                return PatientProcedureOrderResult.BadRequest(
                    "Tindakan yang sudah masuk billing tidak dapat dibatalkan dari modul klinis.");
            }

            if (entity.IsExecuted)
            {
                return PatientProcedureOrderResult.BadRequest("Tindakan yang sudah dieksekusi tidak dapat dibatalkan.");
            }

            if (entity.ProcedureStatus == PatientProcedureStatus.Cancelled)
            {
                return PatientProcedureOrderResult.Conflict("Tindakan sudah dibatalkan sebelumnya.");
            }

            // FR-DOK-102: Pesanan hanya dibatalkan oleh penginput atau DPJP aktif.
            if (entity.InpEpisodeId.HasValue && entity.OrderedByUserId.HasValue)
            {
                var isSubmitter = entity.OrderedByUserId.Value == actorUserId;
                var isDpjp = false;

                var doctorId = await _clinicalContextService.ResolveActorDoctorIdAsync(actorUserId, cancellationToken);
                if (doctorId.HasValue)
                {
                    isDpjp = await _clinicalContextService.IsDpjpAssignedAsync(
                        entity.InpEpisodeId.Value, doctorId.Value, DateTime.UtcNow, cancellationToken);
                }

                if (!isSubmitter && !isDpjp)
                {
                    return PatientProcedureOrderResult.Forbidden(
                        "Hanya penginput pesanan atau DPJP yang sedang bertugas yang dapat membatalkan pesanan ini.");
                }
            }

            var now = DateTime.UtcNow;
            entity.ProcedureStatus = PatientProcedureStatus.Cancelled;
            entity.CancelledAt = now;
            entity.CancelledByUserId = actorUserId;
            entity.CancelReason = reason.Trim();
            entity.IsActive = false;
            entity.IsCancel = true;
            entity.CancelDateTime = now;
            entity.CancelBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "PatientProcedureOrderService.CancelOrderAsync",
                "Pesanan tindakan berhasil dibatalkan.",
                new
                {
                    entity.Id,
                    entity.InpEpisodeId,
                    entity.CancelReason,
                    CancelledBy = actorUserId
                });

            return PatientProcedureOrderResult.Success(
                MapToResponse(entity),
                "Pesanan tindakan berhasil dibatalkan.");
        }

        /// <summary>
        /// Verifikasi instruksi pesanan tindakan oleh dokter pemberi instruksi (FR-DOK-104 / BE-RWI-098).
        /// </summary>
        public async Task<PatientProcedureOrderResult> VerifyInstructionAsync(
            Guid id,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxPatientProcedure>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
            {
                return PatientProcedureOrderResult.NotFound("Tindakan pasien tidak ditemukan.");
            }

            if (entity.InstructionVerificationStatus == PatientProcedureInstructionVerificationStatus.Verified)
            {
                return PatientProcedureOrderResult.Conflict("Instruksi pesanan tindakan sudah diverifikasi.");
            }

            if (!entity.InstructingDoctorId.HasValue)
            {
                return PatientProcedureOrderResult.BadRequest("Pesanan tindakan ini tidak memerlukan verifikasi instruksi.");
            }

            var doctorId = await _clinicalContextService.ResolveActorDoctorIdAsync(actorUserId, cancellationToken);
            if (!doctorId.HasValue || doctorId.Value != entity.InstructingDoctorId.Value)
            {
                return PatientProcedureOrderResult.Forbidden(
                    "Hanya dokter pemberi instruksi yang tercantum yang dapat memverifikasi pesanan ini.");
            }

            var now = DateTime.UtcNow;
            entity.InstructionVerificationStatus = PatientProcedureInstructionVerificationStatus.Verified;
            entity.InstructionVerifiedAt = now;
            entity.InstructionVerifiedByUserId = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return PatientProcedureOrderResult.Success(
                MapToResponse(entity),
                "Instruksi pesanan tindakan berhasil diverifikasi.");
        }

        /// <summary>
        /// Pembatalan otomatis seluruh pesanan tindakan tertunda saat penutupan episode (Langkah 5 / BE-RWI-083 / RWI-DEC-143).
        /// Dipanggil di dalam transaksi penutupan; tidak melakukan commit sendiri.
        /// </summary>
        public async Task<int> CancelPendingOrdersForClosureAsync(
            Guid episodeId,
            Guid closedByUserId,
            CancellationToken cancellationToken = default)
        {
            var pendingOrders = await _dbContext.Set<TrxPatientProcedure>()
                .Where(x => x.InpEpisodeId == episodeId &&
                            !x.IsDelete &&
                            !x.IsExecuted &&
                            !x.IsBillingGenerated &&
                            (x.ProcedureStatus == PatientProcedureStatus.Planned ||
                             x.ProcedureStatus == PatientProcedureStatus.Ordered))
                .ToListAsync(cancellationToken);

            var now = DateTime.UtcNow;
            foreach (var order in pendingOrders)
            {
                order.ProcedureStatus = PatientProcedureStatus.Cancelled;
                order.CancelledByEpisodeClosure = true;
                order.CancelReason = "Episode ditutup sebelum tindakan dilaksanakan.";
                order.CancelledAt = now;
                order.CancelledByUserId = closedByUserId;
                order.IsActive = false;
                order.IsCancel = true;
                order.CancelDateTime = now;
                order.CancelBy = closedByUserId;
                order.UpdateDateTime = now;
                order.UpdateBy = closedByUserId;
            }

            return pendingOrders.Count;
        }

        private static PatientProcedureResponse MapToResponse(TrxPatientProcedure entity)
        {
            return new PatientProcedureResponse
            {
                Id = entity.Id,
                EncounterId = entity.EncounterId,
                ConsultationId = entity.ConsultationId,
                PatientId = entity.PatientId,
                DoctorId = entity.DoctorId,
                ServiceUnitId = entity.ServiceUnitId,
                ClinicId = entity.ClinicId,
                ProcedureId = entity.ProcedureId,
                ProcedureCodeSnapshot = entity.ProcedureCodeSnapshot,
                ProcedureNameSnapshot = entity.ProcedureNameSnapshot,
                ProcedureTypeSnapshot = entity.ProcedureTypeSnapshot,
                ProcedureSource = entity.ProcedureSource,
                ProcedureStatus = entity.ProcedureStatus,
                ProcedureDateTime = entity.ProcedureDateTime,
                PlannedAt = entity.PlannedAt,
                Quantity = entity.Quantity,
                UnitPrice = entity.UnitPrice,
                TotalPrice = entity.TotalPrice,
                IsBillable = entity.IsBillable,
                IsExecuted = entity.IsExecuted,
                ExecutedAt = entity.ExecutedAt,
                PerformedAt = entity.PerformedAt,
                PerformedByUserId = entity.PerformedByUserId,
                IsBillingGenerated = entity.IsBillingGenerated,
                BillingGeneratedAt = entity.BillingGeneratedAt,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                UpdateDateTime = entity.UpdateDateTime,
                OrderedByUserId = entity.OrderedByUserId,
                InstructingDoctorId = entity.InstructingDoctorId,
                InstructionVerificationStatus = entity.InstructionVerificationStatus,
                InstructionVerifiedAt = entity.InstructionVerifiedAt,
                InstructionVerifiedByUserId = entity.InstructionVerifiedByUserId,
                CancelledByEpisodeClosure = entity.CancelledByEpisodeClosure,
                CanEdit = !entity.IsExecuted && !entity.IsBillingGenerated && entity.ProcedureStatus != PatientProcedureStatus.Cancelled,
                CanRemoveFromDraft = false
            };
        }
    }

    /// <summary>
    /// Hasil operasi pesanan tindakan dengan penanda status HTTP yang presisi.
    /// </summary>
    public class PatientProcedureOrderResult
    {
        public bool IsSuccess { get; private set; }
        public int StatusCode { get; private set; }
        public string Message { get; private set; } = string.Empty;
        public PatientProcedureResponse? Data { get; private set; }

        public static PatientProcedureOrderResult Success(PatientProcedureResponse data, string message) =>
            new() { IsSuccess = true, StatusCode = StatusCodes.Status200OK, Data = data, Message = message };

        public static PatientProcedureOrderResult BadRequest(string message) =>
            new() { IsSuccess = false, StatusCode = StatusCodes.Status400BadRequest, Message = message };

        public static PatientProcedureOrderResult Forbidden(string message) =>
            new() { IsSuccess = false, StatusCode = StatusCodes.Status403Forbidden, Message = message };

        public static PatientProcedureOrderResult NotFound(string message) =>
            new() { IsSuccess = false, StatusCode = StatusCodes.Status404NotFound, Message = message };

        public static PatientProcedureOrderResult Conflict(string message) =>
            new() { IsSuccess = false, StatusCode = StatusCodes.Status409Conflict, Message = message };

        public static PatientProcedureOrderResult UnprocessableEntity(string message) =>
            new() { IsSuccess = false, StatusCode = StatusCodes.Status422UnprocessableEntity, Message = message };
    }
}
