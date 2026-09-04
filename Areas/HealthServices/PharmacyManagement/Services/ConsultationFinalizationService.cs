using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Constants;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services
{
    public class ConsultationFinalizationService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ConsultationValidationService _validationService;
        private readonly PrescriptionAggregateService _prescriptionAggregateService;
        private readonly PrescriptionWorkflowService _prescriptionWorkflowService;
        private readonly ClinicalMilestoneFactProducer _clinicalMilestoneFactProducer;
        private readonly ClinicalDocumentIntegrityService _integrityService;

        public ConsultationFinalizationService(
            ApplicationDbContext dbContext,
            ConsultationValidationService validationService,
            PrescriptionAggregateService prescriptionAggregateService,
            PrescriptionWorkflowService prescriptionWorkflowService,
            ClinicalMilestoneFactProducer clinicalMilestoneFactProducer,
            ClinicalDocumentIntegrityService integrityService)
        {
            _dbContext = dbContext;
            _validationService = validationService;
            _prescriptionAggregateService = prescriptionAggregateService;
            _prescriptionWorkflowService = prescriptionWorkflowService;
            _clinicalMilestoneFactProducer = clinicalMilestoneFactProducer;
            _integrityService = integrityService;
        }

        /// <summary>
        /// Memvalidasi lalu menyelesaikan satu konsultasi dokter beserta resep yang menyertainya.
        /// </summary>
        /// <param name="consultationId">Konsultasi yang diselesaikan.</param>
        /// <param name="request">Isi akhir catatan beserta peringatan yang sudah dikonfirmasi.</param>
        /// <param name="actorUserId">Pengguna yang menekan tombol selesai.</param>
        /// <param name="cancellationToken">Token pembatalan permintaan.</param>
        /// <param name="signatureDeviceInfo">
        /// Perangkat yang dipakai menandatangani, diambil pemanggil dari permintaan HTTP —
        /// `BE-RWI-038`, `RM-DEC-021`. Tidak pernah dari kiriman klien: nilai yang dikirim
        /// klien dapat dipalsukan dan kehilangan makna sebagai bukti.
        /// </param>
        /// <param name="signatureIpAddress">Alamat jaringan penanda tangan.</param>
        public async Task<ConsultationFinalizationOperationResult> FinalizeAsync(
            Guid consultationId,
            FinalizeDoctorConsultationRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default,
            string? signatureDeviceInfo = null,
            string? signatureIpAddress = null)
        {
            // RJ-DOC-BE-001, kontrak RJ-DOC-COMPLETION-001@1.0.0 bagian 1.2. Actor selalu berasal
            // dari authentication context, tidak pernah dari payload. Bila klaimnya tidak dapat
            // dibaca, GetCurrentUserId mengembalikan Guid.Empty — dan konsultasi yang selesai
            // tanpa aktor yang dapat ditunjuk bukan audit trail, melainkan lubang audit.
            // Ditolak di boundary finalisasi, bukan disimpan sebagai aktor kosong.
            if (actorUserId == Guid.Empty)
                return ConsultationFinalizationOperationResult.Fail("Aktor penyelesaian konsultasi tidak dapat ditentukan.");

            var now = DateTime.UtcNow;
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            var consultation = await _dbContext.Set<TrxDoctorConsultation>()
                .Include(x => x.Queue)
                .Include(x => x.Encounter)
                .FirstOrDefaultAsync(x => x.Id == consultationId && !x.IsDelete, cancellationToken);

            if (consultation == null)
                return ConsultationFinalizationOperationResult.Fail("Konsultasi dokter tidak ditemukan.");

            if (request.ExpectedUpdatedAt.HasValue && consultation.UpdateDateTime.HasValue &&
                consultation.UpdateDateTime.Value.ToUniversalTime() != request.ExpectedUpdatedAt.Value.ToUniversalTime())
            {
                return ConsultationFinalizationOperationResult.Conflict("Data konsultasi telah berubah. Muat ulang sebelum menyelesaikan konsultasi.");
            }

            if (consultation.ConsultationStatus is DoctorConsultationStatus.Completed or DoctorConsultationStatus.Cancelled)
                return ConsultationFinalizationOperationResult.Fail("Status konsultasi tidak valid untuk finalisasi.");

            ApplyRequest(consultation, request);
            consultation.UpdateDateTime = now;
            consultation.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            var prescriptions = await _dbContext.Set<TrxPrescription>()
                .Where(x => x.ConsultationId == consultationId && !x.IsDelete && !x.IsCancel && x.IsActive)
                .ToListAsync(cancellationToken);

            foreach (var prescription in prescriptions.Where(x => x.PrescriptionStatus == PrescriptionStatus.Draft))
            {
                await _prescriptionAggregateService.RebuildAsync(prescription.Id, actorUserId, now, cancellationToken);
            }

            var validation = await _validationService.ValidateAsync(consultationId, cancellationToken);
            if (validation.ErrorCount > 0)
            {
                await transaction.RollbackAsync(cancellationToken);
                return ConsultationFinalizationOperationResult.ValidationFailed(validation);
            }

            var acknowledged = new HashSet<string>(request.AcknowledgedWarningKeys ?? new List<string>(), StringComparer.OrdinalIgnoreCase);
            var missingWarnings = validation.Sections.SelectMany(x => x.Issues)
                .Where(x => x.Severity == ConsultationValidationSeverity.Warning && !acknowledged.Contains(x.IssueKey))
                .ToList();

            if (missingWarnings.Count > 0)
            {
                await transaction.RollbackAsync(cancellationToken);
                return ConsultationFinalizationOperationResult.WarningAcknowledgementRequired(validation);
            }

            var finalizedPrescriptionCount = 0;
            var finalizedPrescriptions = new List<TrxPrescription>();
            foreach (var prescription in prescriptions.Where(x => x.PrescriptionStatus == PrescriptionStatus.Draft))
            {
                var workflow = await _prescriptionWorkflowService.FinalizeFromConsultationAsync(prescription, actorUserId, now, cancellationToken);
                if (!workflow.IsSuccess)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return ConsultationFinalizationOperationResult.Fail(workflow.ErrorMessage ?? "Resep gagal difinalkan.");
                }
                finalizedPrescriptions.Add(prescription);
                finalizedPrescriptionCount++;
            }

            consultation.ConsultationStatus = DoctorConsultationStatus.Completed;
            consultation.CompletedAt = now;
            consultation.CompletedByUserId = actorUserId;
            consultation.DoctorNote = MergeNote(consultation.DoctorNote, request.FinalizationNote);
            consultation.UpdateDateTime = now;
            consultation.UpdateBy = actorUserId;

            if (consultation.Queue != null)
            {
                consultation.Queue.QueueStatus = QueueStatus.Completed;
                consultation.Queue.ConsultationCompletedAt = now;
                consultation.Queue.CompletedAt = now;
                consultation.Queue.CompletedByUserId = actorUserId;
                consultation.Queue.UpdateDateTime = now;
                consultation.Queue.UpdateBy = actorUserId;
            }

            if (consultation.Encounter != null)
            {
                consultation.Encounter.EncounterStatus = EncounterStatus.ConsultationCompleted;
                consultation.Encounter.UpdateDateTime = now;
                consultation.Encounter.UpdateBy = actorUserId;
            }

            var finalizedProcedureCount = await _dbContext.Set<TrxPatientProcedure>()
                .CountAsync(x => x.ConsultationId == consultationId && !x.IsDelete && !x.IsCancel && x.IsActive, cancellationToken);

            // BE-RWI-038, RWI-AC-157. Catatan dokter yang selesai didaftarkan ke mesin keutuhan
            // rekam medis sebagai dokumen tertanda tangan, dengan penulis catatan sebagai
            // penanda tangannya.
            //
            // Pendaftaran ini berada DI DALAM transaksi finalisasi dengan sengaja. Bila ia
            // dipisah dan gagal, akan lahir catatan yang sudah final tetapi tidak punya baris
            // keutuhan — catatan yang tidak dapat disunting karena sudah selesai, sekaligus
            // tidak dapat dikoreksi karena mesin koreksi tidak mengenalnya. Itu persis keadaan
            // yang sedang ditutup task ini, jadi kegagalan pendaftaran WAJIB membatalkan
            // finalisasi.
            //
            // Penanda tangan adalah PENULIS catatan, bukan aktor yang menekan tombol selesai.
            // Keduanya biasanya orang yang sama, tetapi ketika berbeda, yang bertanggung jawab
            // atas isi catatan tetap penulisnya.
            var authorUserId = consultation.CreateBy != Guid.Empty ? consultation.CreateBy : actorUserId;

            try
            {
                await _integrityService.RegisterSignedAsync(
                    ClinicalDocumentKind.Consultation,
                    consultation.Id,
                    consultation.PatientId,
                    consultation.EncounterId,
                    authorUserId,
                    signatureDeviceInfo,
                    signatureIpAddress,
                    now,
                    cancellationToken);
            }
            catch (InvalidOperationException pendaftaranGagal)
            {
                // Kegagalannya dijawab sebagai penolakan permintaan, bukan kegagalan sistem,
                // supaya pengguna membaca sebabnya dan bukan layar galat kosong. Transaksi
                // dibatalkan lebih dulu, sehingga konsultasi tetap berstatus belum selesai.
                await transaction.RollbackAsync(cancellationToken);

                return ConsultationFinalizationOperationResult.Fail(
                    "Konsultasi tidak dapat diselesaikan karena pendaftaran catatan pada rekam " +
                    $"medis gagal: {pendaftaranGagal.Message}");
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            // RJ-BIL-BE-002. Milestone charge resep menurut RJ-BIL-DEC-002 adalah "resep
            // difinalkan bersama konsultasi dokter"; penyerahan obat adalah fulfillment dan
            // bukan pemicu charge.
            //
            // Penyerahan fakta dilakukan setelah commit. Konsultasi yang sudah sah tidak boleh
            // dibatalkan hanya karena Billing sedang tidak dapat dihubungi.
            var billingHandoffIssues = new List<string>();
            foreach (var prescription in finalizedPrescriptions)
            {
                var emission = await _clinicalMilestoneFactProducer.EmitChargeEligibilityAsync(
                    new ClinicalMilestoneFactRequest
                    {
                        SourceContext = BillingSourceContract.PrescriptionSourceContext,
                        SourceAggregateId = prescription.Id,
                        EffectType = BillingSourceContract.PrescriptionChargeEffectType,
                        EncounterId = prescription.EncounterId,
                        OccurredAt = now,
                        Quantity = prescription.TotalItemCount > 0 ? prescription.TotalItemCount : null,
                        Unit = prescription.TotalItemCount > 0 ? "ITEM" : null,
                        TariffSnapshot = BuildPrescriptionSnapshot(prescription),
                        CorrelationId = consultationId
                    },
                    actorUserId,
                    cancellationToken);

                if (!emission.IsClinicallySafe)
                    billingHandoffIssues.Add($"{prescription.PrescriptionNumber}: {emission.Code}");
            }

            return ConsultationFinalizationOperationResult.Success(new ConsultationFinalizationResponse
            {
                ConsultationId = consultationId,
                CompletedAt = now,
                CompletedByUserId = actorUserId,
                FinalizedPrescriptionCount = finalizedPrescriptionCount,
                FinalizedProcedureCount = finalizedProcedureCount,
                BillingHandoffIssues = billingHandoffIssues,
                Validation = validation
            });
        }

        /// <summary>
        /// Menyusun snapshot tarif klinis sebagai rujukan Billing.
        ///
        /// Sengaja hanya memuat harga kotor dan jumlah item. Pembagian tanggungan asuransi
        /// dan pasien tidak disertakan karena kepemilikan angka tersebut masih menjadi bahasan
        /// RJ-BIL-CONFLICT-001 dan cakupan RJ-BIL-BE-005.
        /// </summary>
        private static string BuildPrescriptionSnapshot(TrxPrescription prescription)
        {
            return JsonSerializer.Serialize(new
            {
                source = "ClinicalSnapshot",
                prescriptionNumber = prescription.PrescriptionNumber,
                totalPrice = prescription.TotalPrice,
                totalItemCount = prescription.TotalItemCount,
                regularItemCount = prescription.RegularItemCount,
                compoundCount = prescription.CompoundCount
            });
        }

        private static void ApplyRequest(TrxDoctorConsultation entity, FinalizeDoctorConsultationRequest request)
        {
            entity.ChiefComplaint = Normalize(request.ChiefComplaint) ?? entity.ChiefComplaint;
            entity.HistoryOfPresentIllness = Normalize(request.HistoryOfPresentIllness) ?? entity.HistoryOfPresentIllness;
            entity.PhysicalExamination = Normalize(request.PhysicalExamination) ?? entity.PhysicalExamination;
            entity.Subjective = Normalize(request.Subjective) ?? entity.Subjective;
            entity.Objective = Normalize(request.Objective) ?? entity.Objective;
            entity.Assessment = Normalize(request.Assessment) ?? entity.Assessment;
            entity.Plan = Normalize(request.Plan) ?? entity.Plan;
            entity.ProcedurePlan = Normalize(request.ProcedurePlan) ?? entity.ProcedurePlan;
            entity.PrescriptionPlan = Normalize(request.PrescriptionPlan) ?? entity.PrescriptionPlan;
            entity.SupportingExamPlan = Normalize(request.SupportingExamPlan) ?? entity.SupportingExamPlan;
            entity.ReferralPlan = Normalize(request.ReferralPlan) ?? entity.ReferralPlan;
            entity.EducationPlan = Normalize(request.EducationPlan) ?? entity.EducationPlan;
            entity.FollowUpDate = request.FollowUpDate ?? entity.FollowUpDate;
            entity.FollowUpNote = Normalize(request.FollowUpNote) ?? entity.FollowUpNote;
            entity.DoctorNote = Normalize(request.DoctorNote) ?? entity.DoctorNote;
        }

        private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        private static string? MergeNote(string? current, string? addition)
        {
            var note = Normalize(addition);
            if (note == null) return current;
            return string.IsNullOrWhiteSpace(current) ? note : $"{current}\nFinalisasi: {note}";
        }
    }

    public class ConsultationFinalizationOperationResult
    {
        public bool IsSuccess { get; private set; }
        public bool IsConflict { get; private set; }
        public bool RequiresWarningAcknowledgement { get; private set; }
        public string? ErrorMessage { get; private set; }
        public ConsultationFinalizationValidationResponse? Validation { get; private set; }
        public ConsultationFinalizationResponse? Data { get; private set; }

        public static ConsultationFinalizationOperationResult Success(ConsultationFinalizationResponse data) => new() { IsSuccess = true, Data = data, Validation = data.Validation };
        public static ConsultationFinalizationOperationResult Fail(string message) => new() { ErrorMessage = message };
        public static ConsultationFinalizationOperationResult Conflict(string message) => new() { IsConflict = true, ErrorMessage = message };
        public static ConsultationFinalizationOperationResult ValidationFailed(ConsultationFinalizationValidationResponse validation) => new() { ErrorMessage = "Konsultasi belum dapat diselesaikan.", Validation = validation };
        public static ConsultationFinalizationOperationResult WarningAcknowledgementRequired(ConsultationFinalizationValidationResponse validation) => new() { RequiresWarningAcknowledgement = true, ErrorMessage = "Konfirmasi seluruh peringatan sebelum menyelesaikan konsultasi.", Validation = validation };
    }
}
