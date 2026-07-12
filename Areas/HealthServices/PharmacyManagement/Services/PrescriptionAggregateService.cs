using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services
{
    public class PrescriptionAggregateService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly PrescriptionSummaryService _prescriptionSummaryService;

        public PrescriptionAggregateService(
            ApplicationDbContext dbContext,
            PrescriptionSummaryService prescriptionSummaryService)
        {
            _dbContext = dbContext;
            _prescriptionSummaryService = prescriptionSummaryService;
        }

        public async Task<PrescriptionAggregateResult> RebuildAsync(
            Guid prescriptionId,
            Guid actorUserId,
            DateTime now,
            CancellationToken cancellationToken = default)
        {
            var prescription = await _dbContext.Set<TrxPrescription>()
                .FirstAsync(
                    x => x.Id == prescriptionId && !x.IsDelete,
                    cancellationToken);

            var regularItems = await _dbContext.Set<TrxPrescriptionItem>()
                .AsNoTracking()
                .Where(x =>
                    x.PrescriptionId == prescriptionId &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.IsActive)
                .ToListAsync(cancellationToken);

            var compounds = await _dbContext.Set<TrxPrescriptionCompound>()
                .Where(x =>
                    x.PrescriptionId == prescriptionId &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.IsActive)
                .ToListAsync(cancellationToken);

            var compoundIds = compounds.Select(x => x.Id).ToList();

            var compoundItems = compoundIds.Count == 0
                ? new List<TrxPrescriptionCompoundItem>()
                : await _dbContext.Set<TrxPrescriptionCompoundItem>()
                    .AsNoTracking()
                    .Where(x =>
                        compoundIds.Contains(x.PrescriptionCompoundId) &&
                        !x.IsDelete &&
                        !x.IsCancel &&
                        x.IsActive)
                    .ToListAsync(cancellationToken);

            foreach (var compound in compounds)
            {
                var items = compoundItems
                    .Where(x => x.PrescriptionCompoundId == compound.Id)
                    .ToList();

                compound.IngredientCount = items.Count;
                compound.TotalPrice = RoundMoney(items.Sum(x => x.TotalPrice));
                compound.CoveredAmount = RoundMoney(items.Sum(x => x.CoveredAmount));
                compound.PatientPayAmount = RoundMoney(items.Sum(x => x.PatientPayAmount));
                compound.IsNeedApproval = items.Any(x => x.IsNeedApproval);
                compound.IsApproved = !compound.IsNeedApproval ||
                    items.Where(x => x.IsNeedApproval).All(x => x.IsApproved);
                compound.IsNeedGuaranteeLetter = items.Any(x => x.IsNeedGuaranteeLetter);
                compound.UpdateDateTime = now;
                compound.UpdateBy = actorUserId;
            }

            prescription.RegularItemCount = regularItems.Count;
            prescription.CompoundCount = compounds.Count;
            prescription.CompoundIngredientCount = compoundItems.Count;
            prescription.TotalItemCount = prescription.RegularItemCount + prescription.CompoundIngredientCount;
            prescription.TotalPrice = RoundMoney(
                regularItems.Sum(x => x.TotalPrice) +
                compoundItems.Sum(x => x.TotalPrice));
            prescription.CoveredAmount = RoundMoney(
                regularItems.Sum(x => x.CoveredAmount) +
                compoundItems.Sum(x => x.CoveredAmount));
            prescription.PatientPayAmount = RoundMoney(
                regularItems.Sum(x => x.PatientPayAmount) +
                compoundItems.Sum(x => x.PatientPayAmount));

            var allApprovalFlags = regularItems
                .Where(x => x.IsNeedApproval)
                .Select(x => x.IsApproved)
                .Concat(compoundItems
                    .Where(x => x.IsNeedApproval)
                    .Select(x => x.IsApproved))
                .ToList();

            prescription.IsNeedApproval = allApprovalFlags.Count > 0;
            prescription.IsApproved = !prescription.IsNeedApproval || allApprovalFlags.All(x => x);
            prescription.UpdateDateTime = now;
            prescription.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _prescriptionSummaryService.RebuildConsultationSummaryAsync(
                prescription.ConsultationId,
                actorUserId,
                now,
                cancellationToken);

            return new PrescriptionAggregateResult
            {
                PrescriptionId = prescription.Id,
                RegularItemCount = prescription.RegularItemCount,
                CompoundCount = prescription.CompoundCount,
                CompoundIngredientCount = prescription.CompoundIngredientCount,
                TotalItemCount = prescription.TotalItemCount,
                TotalPrice = prescription.TotalPrice,
                CoveredAmount = prescription.CoveredAmount,
                PatientPayAmount = prescription.PatientPayAmount,
                IsNeedApproval = prescription.IsNeedApproval,
                IsApproved = prescription.IsApproved
            };
        }

        public async Task EnsureEditableAsync(
            Guid prescriptionId,
            CancellationToken cancellationToken = default)
        {
            var prescription = await _dbContext.Set<TrxPrescription>()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == prescriptionId && !x.IsDelete,
                    cancellationToken);

            if (prescription == null)
                throw new InvalidOperationException("Resep tidak ditemukan.");

            if (prescription.IsCancel ||
                prescription.PrescriptionStatus == PrescriptionStatus.Cancelled)
            {
                throw new InvalidOperationException("Resep yang sudah dibatalkan tidak dapat diubah.");
            }

            if (prescription.PrescriptionStatus != PrescriptionStatus.Draft)
            {
                throw new InvalidOperationException(
                    "Item resep hanya dapat diubah ketika resep masih berstatus Draft.");
            }

            if (prescription.PaymentStatus != PrescriptionPaymentStatus.NotBilled)
            {
                throw new InvalidOperationException(
                    "Item resep tidak dapat diubah setelah proses billing dimulai.");
            }
        }

        private static decimal RoundMoney(decimal value)
        {
            return Math.Round(value, 2, MidpointRounding.AwayFromZero);
        }
    }

    public class PrescriptionAggregateResult
    {
        public Guid PrescriptionId { get; set; }
        public int RegularItemCount { get; set; }
        public int CompoundCount { get; set; }
        public int CompoundIngredientCount { get; set; }
        public int TotalItemCount { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal CoveredAmount { get; set; }
        public decimal PatientPayAmount { get; set; }
        public bool IsNeedApproval { get; set; }
        public bool IsApproved { get; set; }
    }
}
