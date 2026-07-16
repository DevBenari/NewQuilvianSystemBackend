using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services
{
    public class PrescriptionFinalCheckService
    {
        private readonly ApplicationDbContext _dbContext;
        public PrescriptionFinalCheckService(ApplicationDbContext dbContext) => _dbContext = dbContext;

        public async Task<PrescriptionFinalCheckResponse> CompleteAsync(Guid prescriptionId, CompletePrescriptionFinalCheckRequest request, Guid actorUserId, CancellationToken ct = default)
        {
            var prescription = await _dbContext.Set<TrxPrescription>()
                .FirstOrDefaultAsync(x => x.Id == prescriptionId && !x.IsDelete, ct)
                ?? throw new InvalidOperationException("Resep tidak ditemukan.");
            if (prescription.FulfillmentStatus != PrescriptionFulfillmentStatus.ReadyToDispense)
                throw new InvalidOperationException("Telaah obat akhir hanya dapat dilakukan setelah penyiapan selesai.");
            if (request.Items.Count == 0)
                throw new InvalidOperationException("Kriteria telaah obat akhir wajib diisi.");

            var now = DateTime.UtcNow;
            var finalCheck = await _dbContext.Set<TrxPrescriptionFinalCheck>()
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.PrescriptionId == prescriptionId && !x.IsDelete && x.IsActive, ct);
            if (finalCheck == null)
            {
                finalCheck = new TrxPrescriptionFinalCheck
                {
                    Id = Guid.NewGuid(), PrescriptionId = prescriptionId,
                    Status = PrescriptionFinalCheckStatus.InReview,
                    CheckedByUserId = actorUserId, StartedAt = now,
                    CreateDateTime = now, CreateBy = actorUserId, IsActive = true
                };
                _dbContext.Set<TrxPrescriptionFinalCheck>().Add(finalCheck);
            }
            else
            {
                foreach (var old in finalCheck.Items.Where(x => !x.IsDelete))
                {
                    old.IsDelete = true; old.IsActive = false; old.DeleteDateTime = now; old.DeleteBy = actorUserId;
                }
            }

            foreach (var input in request.Items)
            {
                finalCheck.Items.Add(new TrxPrescriptionFinalCheckItem
                {
                    Id = Guid.NewGuid(), PrescriptionFinalCheckId = finalCheck.Id,
                    CriterionCode = input.CriterionCode.Trim(),
                    CriterionName = input.CriterionName.Trim(), Result = input.Result,
                    Finding = Normalize(input.Finding), SortOrder = input.SortOrder,
                    CreateDateTime = now, CreateBy = actorUserId, IsActive = true
                });
            }

            var passed = request.Items.All(x => x.Result is PrescriptionReviewResult.Compliant or PrescriptionReviewResult.NotApplicable or PrescriptionReviewResult.WarningAccepted);
            finalCheck.Status = passed ? PrescriptionFinalCheckStatus.Passed : PrescriptionFinalCheckStatus.Failed;
            finalCheck.CheckedByUserId = actorUserId;
            finalCheck.CompletedAt = now;
            finalCheck.CheckNote = Normalize(request.CheckNote);
            finalCheck.UpdateDateTime = now;
            finalCheck.UpdateBy = actorUserId;
            if (passed)
                prescription.FulfillmentStatus = PrescriptionFulfillmentStatus.ReadyToDispense;
            prescription.UpdateDateTime = now;
            prescription.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(ct);
            return Map(finalCheck);
        }

        private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        private static PrescriptionFinalCheckResponse Map(TrxPrescriptionFinalCheck x) => new()
        {
            Id = x.Id, PrescriptionId = x.PrescriptionId, Status = x.Status,
            CheckedByUserId = x.CheckedByUserId, StartedAt = x.StartedAt,
            CompletedAt = x.CompletedAt, CheckNote = x.CheckNote,
            Items = x.Items.Where(i => !i.IsDelete && i.IsActive).OrderBy(i => i.SortOrder).Select(i => new PrescriptionFinalCheckItemResponse
            {
                Id = i.Id, CriterionCode = i.CriterionCode, CriterionName = i.CriterionName,
                Result = i.Result, Finding = i.Finding, SortOrder = i.SortOrder
            }).ToList()
        };
    }
}
