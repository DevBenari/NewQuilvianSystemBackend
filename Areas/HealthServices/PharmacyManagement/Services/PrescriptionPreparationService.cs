using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services
{
    public class PrescriptionPreparationService
    {
        private readonly ApplicationDbContext _dbContext;
        public PrescriptionPreparationService(ApplicationDbContext dbContext) => _dbContext = dbContext;

        public async Task<PrescriptionPreparationResponse> StartAsync(Guid prescriptionId, Guid actorUserId, string? note, CancellationToken ct = default)
        {
            var prescription = await _dbContext.Set<TrxPrescription>()
                .FirstOrDefaultAsync(x => x.Id == prescriptionId && !x.IsDelete, ct)
                ?? throw new InvalidOperationException("Resep tidak ditemukan.");
            if (prescription.FulfillmentStatus != PrescriptionFulfillmentStatus.VerifiedByPharmacy)
                throw new InvalidOperationException("Penyiapan hanya dapat dimulai setelah telaah farmasi disetujui.");

            var current = await _dbContext.Set<TrxPrescriptionPreparation>()
                .Include(x => x.Items.Where(i => !i.IsDelete && i.IsActive))
                .FirstOrDefaultAsync(x => x.PrescriptionId == prescriptionId && !x.IsDelete && x.IsActive, ct);
            var now = DateTime.UtcNow;
            if (current == null)
            {
                current = new TrxPrescriptionPreparation
                {
                    Id = Guid.NewGuid(), PrescriptionId = prescriptionId,
                    Status = PrescriptionPreparationStatus.InPreparation,
                    PreparedByUserId = actorUserId, PreparationStartedAt = now,
                    PreparationNote = Normalize(note), CreateDateTime = now,
                    CreateBy = actorUserId, IsActive = true
                };
                _dbContext.Set<TrxPrescriptionPreparation>().Add(current);
            }
            else
            {
                current.Status = PrescriptionPreparationStatus.InPreparation;
                current.PreparedByUserId = actorUserId;
                current.PreparationStartedAt ??= now;
                current.PreparationNote = Normalize(note) ?? current.PreparationNote;
                current.UpdateDateTime = now;
                current.UpdateBy = actorUserId;
            }
            prescription.FulfillmentStatus = PrescriptionFulfillmentStatus.InPreparation;
            prescription.UpdateDateTime = now;
            prescription.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(ct);
            return Map(current);
        }

        public async Task<PrescriptionPreparationResponse> CompleteAsync(Guid prescriptionId, CompletePrescriptionPreparationRequest request, Guid actorUserId, CancellationToken ct = default)
        {
            var prescription = await _dbContext.Set<TrxPrescription>()
                .FirstOrDefaultAsync(x => x.Id == prescriptionId && !x.IsDelete, ct)
                ?? throw new InvalidOperationException("Resep tidak ditemukan.");
            var preparation = await _dbContext.Set<TrxPrescriptionPreparation>()
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.PrescriptionId == prescriptionId && !x.IsDelete && x.IsActive, ct)
                ?? throw new InvalidOperationException("Proses penyiapan belum dimulai.");
            if (preparation.Status != PrescriptionPreparationStatus.InPreparation)
                throw new InvalidOperationException("Status penyiapan tidak valid.");

            var now = DateTime.UtcNow;
            foreach (var old in preparation.Items.Where(x => !x.IsDelete))
            {
                old.IsDelete = true; old.IsActive = false; old.DeleteDateTime = now; old.DeleteBy = actorUserId;
            }
            foreach (var input in request.Items)
            {
                if (!input.PrescriptionItemId.HasValue && !input.PrescriptionCompoundItemId.HasValue)
                    throw new InvalidOperationException("Item penyiapan harus merujuk ke item resep reguler atau bahan racikan.");
                preparation.Items.Add(new TrxPrescriptionPreparationItem
                {
                    Id = Guid.NewGuid(), PrescriptionPreparationId = preparation.Id,
                    PrescriptionItemId = input.PrescriptionItemId,
                    PrescriptionCompoundItemId = input.PrescriptionCompoundItemId,
                    DrugId = input.DrugId, TheoreticalQuantity = input.TheoreticalQuantity,
                    ActualQuantity = input.ActualQuantity, WasteQuantity = input.WasteQuantity,
                    MeasurementId = input.MeasurementId,
                    MeasurementNameSnapshot = Normalize(input.MeasurementName),
                    BatchNumber = Normalize(input.BatchNumber), ExpiryDate = input.ExpiryDate,
                    Note = Normalize(input.Note), SortOrder = input.SortOrder,
                    CreateDateTime = now, CreateBy = actorUserId, IsActive = true
                });
            }
            preparation.Status = PrescriptionPreparationStatus.Prepared;
            preparation.PreparationCompletedAt = now;
            preparation.PreparationNote = Normalize(request.PreparationNote) ?? preparation.PreparationNote;
            preparation.UpdateDateTime = now;
            preparation.UpdateBy = actorUserId;
            prescription.FulfillmentStatus = PrescriptionFulfillmentStatus.ReadyToDispense;
            prescription.UpdateDateTime = now;
            prescription.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(ct);
            return Map(preparation);
        }

        private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        private static PrescriptionPreparationResponse Map(TrxPrescriptionPreparation x) => new()
        {
            Id = x.Id, PrescriptionId = x.PrescriptionId, Status = x.Status,
            PreparedByUserId = x.PreparedByUserId,
            PreparationStartedAt = x.PreparationStartedAt,
            PreparationCompletedAt = x.PreparationCompletedAt,
            PreparationNote = x.PreparationNote,
            Items = x.Items.Where(i => !i.IsDelete && i.IsActive).OrderBy(i => i.SortOrder).Select(i => new PrescriptionPreparationItemResponse
            {
                Id = i.Id, PrescriptionItemId = i.PrescriptionItemId,
                PrescriptionCompoundItemId = i.PrescriptionCompoundItemId,
                DrugId = i.DrugId, TheoreticalQuantity = i.TheoreticalQuantity,
                ActualQuantity = i.ActualQuantity, WasteQuantity = i.WasteQuantity,
                MeasurementId = i.MeasurementId, MeasurementName = i.MeasurementNameSnapshot,
                BatchNumber = i.BatchNumber, ExpiryDate = i.ExpiryDate,
                Note = i.Note, SortOrder = i.SortOrder
            }).ToList()
        };
    }
}
