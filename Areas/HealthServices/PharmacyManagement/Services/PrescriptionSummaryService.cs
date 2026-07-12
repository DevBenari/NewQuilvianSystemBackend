using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services
{
    public class PrescriptionSummaryService
    {
        private readonly ApplicationDbContext _dbContext;

        public PrescriptionSummaryService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PrescriptionSummaryResult> RebuildConsultationSummaryAsync(
            Guid consultationId,
            Guid actorUserId,
            DateTime now,
            CancellationToken cancellationToken = default)
        {
            var consultation = await _dbContext.Set<TrxDoctorConsultation>()
                .FirstAsync(
                    x => x.Id == consultationId && !x.IsDelete,
                    cancellationToken);

            var prescriptions = await _dbContext.Set<TrxPrescription>()
                .AsNoTracking()
                .Where(x =>
                    x.ConsultationId == consultationId &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.PrescriptionStatus != PrescriptionStatus.Cancelled)
                .OrderBy(x => x.PrescriptionDateTime)
                .ToListAsync(cancellationToken);

            var prescriptionText = prescriptions.Count == 0
                ? null
                : string.Join("; ", prescriptions.Select(x =>
                    x.TotalItemCount > 0
                        ? $"{x.PrescriptionNumber} ({x.TotalItemCount} item)"
                        : x.PrescriptionNumber));

            consultation.PrescriptionText = prescriptionText;
            consultation.PrescriptionCount = prescriptions.Count;
            consultation.HasPrescription = prescriptions.Count > 0;
            consultation.UpdateDateTime = now;
            consultation.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new PrescriptionSummaryResult
            {
                PrescriptionText = prescriptionText,
                PrescriptionCount = prescriptions.Count,
                HasPrescription = prescriptions.Count > 0
            };
        }
    }

    public class PrescriptionSummaryResult
    {
        public string? PrescriptionText { get; set; }
        public int PrescriptionCount { get; set; }
        public bool HasPrescription { get; set; }
    }
}
