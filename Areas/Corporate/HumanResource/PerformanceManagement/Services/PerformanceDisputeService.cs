using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Services
{
    /// <summary>
    /// Layanan Tata Kelola Sanggah Banding Nilai Kinerja Pegawai SLA 7/14 Hari (BE-HRD-267, VR-PRF-12, VR-PRF-13)
    /// </summary>
    public class PerformanceDisputeService : IPerformanceDisputeService
    {
        private readonly IPerformanceUnitOfWork _uow;

        public PerformanceDisputeService(IPerformanceUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task<AppraisalDisputeDto> SubmitDisputeAsync(SubmitAppraisalDisputeRequestDto dto, Guid staffId, CancellationToken cancellationToken = default)
        {
            var appraisal = await _uow.PerformanceAppraisals.FirstOrDefaultAsync(x => x.Id == dto.AppraisalId, cancellationToken);
            if (appraisal == null)
                throw new KeyNotFoundException($"Lembar evaluasi kinerja ID {dto.AppraisalId} tidak ditemukan.");

            if (appraisal.WorkforceProfileId != staffId)
                throw new UnauthorizedAccessException("Pegawai hanya dapat mengajukan sanggah banding atas nilai evaluasi kinerja miliknya sendiri.");

            // VR-PRF-12: Batas Waktu Pengajuan Banding Maksimal 7 Hari Kalender Pasca Rilis
            if (appraisal.PublishedDate.HasValue)
            {
                var daysSincePublished = (DateTime.UtcNow.Date - appraisal.PublishedDate.Value.Date).TotalDays;
                if (daysSincePublished > 7)
                {
                    throw new InvalidOperationException($"Pengajuan sanggah banding ditolak: batas waktu pengajuan (maksimal 7 hari kalender pasca rilis nilai) telah berakhir ({daysSincePublished:F0} hari berlalu, VR-PRF-12, HRD-DEC-169).");
                }
            }

            var dispute = new HrdPerformanceAppraisalDispute
            {
                Id = Guid.NewGuid(),
                PerformanceAppraisalId = appraisal.Id,
                WorkforceProfileId = staffId,
                DisputeReason = dto.DisputeReason,
                CounterEvidenceUrl = dto.CounterEvidenceUrl,
                Status = "SUBMITTED",
                SubmissionDate = DateTime.UtcNow,
                CreateBy = staffId,
                CreateDateTime = DateTime.UtcNow,
                UpdateBy = staffId,
                UpdateDateTime = DateTime.UtcNow
            };

            appraisal.HasDispute = true;
            appraisal.Status = "DISPUTED";
            appraisal.UpdateBy = staffId;
            appraisal.UpdateDateTime = DateTime.UtcNow;

            await _uow.AddAsync(dispute, cancellationToken);
            _uow.Update(appraisal);
            await _uow.SaveChangesAsync(cancellationToken);

            return new AppraisalDisputeDto
            {
                Id = dispute.Id,
                AppraisalId = dispute.PerformanceAppraisalId,
                WorkforceProfileId = dispute.WorkforceProfileId,
                DisputeReason = dispute.DisputeReason,
                CounterEvidenceUrl = dispute.CounterEvidenceUrl,
                Status = dispute.Status
            };
        }

        public async Task<AppraisalDisputeDto> AdjudicateDisputeAsync(Guid disputeId, AdjudicateDisputeRequestDto dto, Guid directorId, CancellationToken cancellationToken = default)
        {
            var dispute = await _uow.PerformanceAppraisalDisputes.FirstOrDefaultAsync(x => x.Id == disputeId, cancellationToken);
            if (dispute == null)
                throw new KeyNotFoundException($"Berkas sanggah banding ID {disputeId} tidak ditemukan.");

            var appraisal = await _uow.PerformanceAppraisals.FirstOrDefaultAsync(x => x.Id == dispute.PerformanceAppraisalId, cancellationToken);
            if (appraisal == null)
                throw new KeyNotFoundException($"Lembar evaluasi kinerja terkait tidak ditemukan.");

            // VR-PRF-13: SLA Majelis Banding Maksimal 14 Hari Kalender
            var daysSinceSubmitted = (DateTime.UtcNow.Date - dispute.SubmissionDate.Date).TotalDays;
            if (daysSinceSubmitted > 14)
            {
                // Tetap diproses tetapi diberi catatan pelanggaran SLA
                dispute.InvestigationNotes = $"[CATATAN SLA: Putusan terbit H+{daysSinceSubmitted:F0} hari kalender] " + dto.ResolutionNotes;
            }
            else
            {
                dispute.InvestigationNotes = dto.ResolutionNotes;
            }

            dispute.AdjudicatedById = directorId;
            dispute.AdjudicatedDateTime = DateTime.UtcNow;

            if (dto.ResolutionDecision == "REVISED" && dto.NewFinalScore.HasValue)
            {
                dispute.Status = "ADJUDICATED";
                dispute.FinalResolutionScore = dto.NewFinalScore.Value;

                appraisal.FinalScore = dto.NewFinalScore.Value;
                appraisal.FinalGrade = DetermineGrade(dto.NewFinalScore.Value);
                appraisal.Status = "RELEASED";
                appraisal.HasDispute = false;
            }
            else if (dto.ResolutionDecision == "UPHELD")
            {
                dispute.Status = "ADJUDICATED";
                appraisal.Status = "RELEASED";
                appraisal.HasDispute = false;
            }
            else
            {
                dispute.Status = "REJECTED";
                appraisal.Status = "RELEASED";
                appraisal.HasDispute = false;
            }

            dispute.UpdateBy = directorId;
            dispute.UpdateDateTime = DateTime.UtcNow;
            appraisal.UpdateBy = directorId;
            appraisal.UpdateDateTime = DateTime.UtcNow;

            _uow.Update(dispute);
            _uow.Update(appraisal);
            await _uow.SaveChangesAsync(cancellationToken);

            return new AppraisalDisputeDto
            {
                Id = dispute.Id,
                AppraisalId = dispute.PerformanceAppraisalId,
                WorkforceProfileId = dispute.WorkforceProfileId,
                DisputeReason = dispute.DisputeReason,
                CounterEvidenceUrl = dispute.CounterEvidenceUrl,
                Status = dispute.Status,
                InvestigationNotes = dispute.InvestigationNotes,
                FinalResolutionScore = dispute.FinalResolutionScore,
                AdjudicatedById = dispute.AdjudicatedById,
                AdjudicatedDateTime = dispute.AdjudicatedDateTime
            };
        }

        private static string DetermineGrade(decimal score)
        {
            if (score >= 90.0m) return "SANGAT_BAIK";
            if (score >= 75.0m) return "BAIK";
            if (score >= 65.0m) return "CUKUP";
            if (score >= 50.0m) return "KURANG";
            return "SANGAT_KURANG";
        }
    }
}
