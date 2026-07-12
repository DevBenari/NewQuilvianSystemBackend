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

        public async Task<PrescriptionWorkflowResult> MarkBillingGeneratedAsync(
            TrxPrescription entity,
            Guid? billingId,
            Guid actorUserId,
            DateTime now,
            CancellationToken cancellationToken = default)
        {
            if (entity.PrescriptionStatus != PrescriptionStatus.Submitted)
                return PrescriptionWorkflowResult.Fail("Billing hanya dapat dibuat untuk resep yang sudah difinalkan bersama konsultasi.");

            if (entity.PaymentStatus != PrescriptionPaymentStatus.NotBilled &&
                entity.PaymentStatus != PrescriptionPaymentStatus.BillingGenerated)
            {
                return PrescriptionWorkflowResult.Fail("Status pembayaran resep tidak valid untuk pembuatan billing.");
            }

            entity.BillingId = billingId ?? entity.BillingId;
            entity.PaymentStatus = PrescriptionPaymentStatus.WaitingForPayment;
            entity.BillingGeneratedAt ??= now;
            entity.FulfillmentStatus = PrescriptionFulfillmentStatus.WaitingForPayment;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return PrescriptionWorkflowResult.Ok();
        }

        public Task<PrescriptionWorkflowResult> MarkPaidAsync(TrxPrescription entity, Guid actorUserId, DateTime now, CancellationToken cancellationToken = default)
            => CompletePaymentAsync(entity, PrescriptionPaymentStatus.Paid, actorUserId, now, cancellationToken);

        public Task<PrescriptionWorkflowResult> MarkInsuranceApprovedAsync(TrxPrescription entity, Guid actorUserId, DateTime now, CancellationToken cancellationToken = default)
            => CompletePaymentAsync(entity, PrescriptionPaymentStatus.InsuranceApproved, actorUserId, now, cancellationToken);

        public Task<PrescriptionWorkflowResult> MarkPaymentWaivedAsync(TrxPrescription entity, Guid actorUserId, DateTime now, CancellationToken cancellationToken = default)
            => CompletePaymentAsync(entity, PrescriptionPaymentStatus.PaymentWaived, actorUserId, now, cancellationToken);

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

            entity.PrescriptionStatus = PrescriptionStatus.Cancelled;
            entity.PaymentStatus = PrescriptionPaymentStatus.Cancelled;
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

        private async Task<PrescriptionWorkflowResult> CompletePaymentAsync(
            TrxPrescription entity,
            PrescriptionPaymentStatus completedStatus,
            Guid actorUserId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            if (entity.PrescriptionStatus != PrescriptionStatus.Submitted)
                return PrescriptionWorkflowResult.Fail("Pembayaran hanya dapat diselesaikan untuk resep yang sudah difinalkan.");

            if (entity.PaymentStatus is PrescriptionPaymentStatus.Paid or PrescriptionPaymentStatus.InsuranceApproved or PrescriptionPaymentStatus.PaymentWaived)
                return PrescriptionWorkflowResult.Fail("Pembayaran resep sudah diselesaikan.");

            if (entity.PaymentStatus == PrescriptionPaymentStatus.Cancelled)
                return PrescriptionWorkflowResult.Fail("Pembayaran resep sudah dibatalkan.");

            entity.PaymentStatus = completedStatus;
            entity.PaymentCompletedAt = now;
            entity.PaymentCompletedByUserId = actorUserId;
            entity.FulfillmentStatus = PrescriptionFulfillmentStatus.ReadyForPharmacy;
            entity.ReadyForPharmacyAt = now;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return PrescriptionWorkflowResult.Ok();
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
