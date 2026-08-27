using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services
{
    public class PrescriptionWorkflowService
    {
        private readonly ApplicationDbContext _dbContext;

        public PrescriptionWorkflowService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PrescriptionWorkflowResult> FinalizeFromConsultationAsync(
            TrxPrescription entity,
            Guid actorUserId,
            DateTime now,
            CancellationToken cancellationToken = default)
        {
            if (entity.PrescriptionStatus != PrescriptionStatus.Draft)
                return PrescriptionWorkflowResult.Fail("Hanya resep draft yang dapat difinalkan bersama konsultasi.");

            if (entity.PaymentStatus != PrescriptionPaymentStatus.NotBilled)
                return PrescriptionWorkflowResult.Fail("Resep tidak dapat difinalkan karena proses billing sudah dimulai.");

            if (entity.FulfillmentStatus != PrescriptionFulfillmentStatus.WaitingForClinicalFinalization)
                return PrescriptionWorkflowResult.Fail("Status pemenuhan resep tidak valid untuk finalisasi klinis.");

            if (entity.TotalItemCount <= 0)
                return PrescriptionWorkflowResult.Fail("Resep belum memiliki item obat.");

            if (entity.IsNeedApproval && !entity.IsApproved)
                return PrescriptionWorkflowResult.Fail("Resep membutuhkan approval sebelum konsultasi diselesaikan.");

            entity.PrescriptionStatus = PrescriptionStatus.Submitted;
            entity.PaymentStatus = PrescriptionPaymentStatus.NotBilled;
            entity.FulfillmentStatus = PrescriptionFulfillmentStatus.WaitingForPayment;
            entity.SubmittedAt = now;
            entity.SubmittedByUserId = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return PrescriptionWorkflowResult.Ok();
        }

        public Task<PrescriptionWorkflowResult> SubmitAsync(
            TrxPrescription entity,
            Guid actorUserId,
            DateTime now,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(PrescriptionWorkflowResult.Fail(
                "Resep tidak diajukan secara terpisah. Selesaikan konsultasi dokter untuk memfinalkan resep."));
        }

        // RJ-BIL-BE-002 / RJ-BIL-CONFLICT-006 keputusan author 1A.
        //
        // MarkBillingGeneratedAsync, MarkPaidAsync, MarkInsuranceApprovedAsync,
        // MarkPaymentWaivedAsync, dan CompletePaymentAsync dihapus dari modul klinis.
        // Kelimanya menetapkan status finansial canonical dari kewenangan klinis
        // Prescription : Update, sehingga siapa pun yang boleh mengubah resep dapat
        // menyatakan resep lunas. Status finansial sekarang hanya berasal dari Billing.
        //
        // Modul klinis menyerahkan fakta melalui ClinicalMilestoneFactProducer, dan Billing
        // yang menentukan akibat finansialnya.

        public async Task<PrescriptionWorkflowResult> CancelAsync(
            TrxPrescription entity,
            string reason,
            Guid actorUserId,
            DateTime now,
            CancellationToken cancellationToken = default)
        {
            if (entity.PrescriptionStatus == PrescriptionStatus.Cancelled)
                return PrescriptionWorkflowResult.Fail("Resep sudah dibatalkan.");

            if (entity.FulfillmentStatus is
                PrescriptionFulfillmentStatus.QueuedAtPharmacy or
                PrescriptionFulfillmentStatus.VerifiedByPharmacy or
                PrescriptionFulfillmentStatus.InPreparation or
                PrescriptionFulfillmentStatus.ReadyToDispense or
                PrescriptionFulfillmentStatus.PartiallyDispensed or
                PrescriptionFulfillmentStatus.Dispensed)
            {
                return PrescriptionWorkflowResult.Fail("Resep yang sudah diproses farmasi tidak dapat dibatalkan dari modul dokter.");
            }

            // RJ-BIL-BE-002 / keputusan author 1B: pembatalan klinis bersifat otoritatif atas
            // status klinis dan status pemenuhan saja. PaymentStatus sengaja tidak disentuh —
            // pembatalan klinis bukan pembatalan finansial. Konsekuensi finansialnya ditentukan
            // Billing setelah menerima fakta pembatalan.
            entity.PrescriptionStatus = PrescriptionStatus.Cancelled;
            entity.FulfillmentStatus = PrescriptionFulfillmentStatus.Cancelled;
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
            return PrescriptionWorkflowResult.Ok();
        }

        public bool CanDelete(TrxPrescription entity)
        {
            return entity.PrescriptionStatus == PrescriptionStatus.Draft &&
                   entity.TotalItemCount == 0 &&
                   entity.PaymentStatus == PrescriptionPaymentStatus.NotBilled &&
                   entity.FulfillmentStatus == PrescriptionFulfillmentStatus.WaitingForClinicalFinalization;
        }

    }

    public class PrescriptionWorkflowResult
    {
        public bool IsSuccess { get; private set; }
        public string? ErrorMessage { get; private set; }

        public static PrescriptionWorkflowResult Ok() => new() { IsSuccess = true };

        public static PrescriptionWorkflowResult Fail(string errorMessage) => new()
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }
}
