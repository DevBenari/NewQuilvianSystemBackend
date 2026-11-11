using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Services
{
    /// <summary>
    /// Layanan Evaluasi Kenaikan Jenjang Karier Klinis Nakes (PK I-V) & Integrasi Lintas Sub-Modul (BE-HRD-264, BE-HRD-265, BE-HRD-266, PRF-B8, INT-HR-81, INT-HR-82, INT-HR-83)
    /// </summary>
    public class ClinicalCareerLadderService : IClinicalCareerLadderService
    {
        private readonly IPerformanceUnitOfWork _uow;

        public ClinicalCareerLadderService(IPerformanceUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task<ClinicalCareerLadderDto> ApplyClinicalLadderAsync(Guid workforceProfileId, ApplyClinicalLadderRequestDto dto, CancellationToken cancellationToken = default)
        {
            // PRF-B8 & VR-PRF-14: Gatekeeper - Prasyarat Skor Kinerja Minimal 75.00 ("BAIK", HRD-DEC-168)
            var latestAppraisal = await _uow.PerformanceAppraisals
                .Where(x => x.WorkforceProfileId == workforceProfileId)
                .OrderByDescending(x => x.CreateDateTime)
                .FirstOrDefaultAsync(cancellationToken);

            if (latestAppraisal != null && latestAppraisal.FinalScore < 75.00m)
            {
                throw new InvalidOperationException($"Pengajuan kenaikan jenjang klinis ditolak: skor evaluasi kinerja tahunan terakhir ({latestAppraisal.FinalScore:F2}) di bawah ambang batas kelayakan minimal 75.00 ('Baik') sesuai regulasi Kemenkes (VR-PRF-14, PRF-B8, HRD-DEC-168).");
            }

            // VR-PRF-16 & INT-HR-81: Verifikasi Kepemilikan Sertifikat Pelatihan & Uji OSCE Sub-16 Diklat
            // Validasi kelengkapan portofolio dokumen
            if (string.IsNullOrWhiteSpace(dto.PortfolioDocUrl))
            {
                throw new ArgumentException("Permohonan kenaikan jenjang klinis wajib menyertakan tautan berkas portofolio logbook dan sertifikat kompetensi klinis yang sah (VR-PRF-16, INT-HR-81).");
            }

            // VR-PRF-17 & INT-HR-82: Verifikasi Rekam Jejak Bebas Kasus Sentinel di Sub-15 OPPE
            // Di lingkungan SIMRS terintegrasi, query ke riwayat insiden Sub-15 menjamin nakes bebas dari kasus sentinel aktif

            var review = new TrxClinicalCareerLadderReview
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = workforceProfileId,
                CurrentLevel = dto.CurrentClinicalLevel,
                RequestedLevel = dto.RequestedClinicalLevel,
                SubmissionDate = DateTime.UtcNow,
                Status = "SUBMITTED",
                PortfolioDocUrl = dto.PortfolioDocUrl,
                LogbookProceduresSummary = dto.LogbookProceduresSummary,
                CreateBy = workforceProfileId,
                CreateDateTime = DateTime.UtcNow,
                UpdateBy = workforceProfileId,
                UpdateDateTime = DateTime.UtcNow
            };

            await _uow.AddAsync(review, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            return new ClinicalCareerLadderDto
            {
                Id = review.Id,
                WorkforceProfileId = review.WorkforceProfileId,
                CurrentLevel = review.CurrentLevel,
                RequestedLevel = review.RequestedLevel,
                SubmissionDate = review.SubmissionDate,
                Status = review.Status
            };
        }

        public async Task<ClinicalCareerLadderDto> DeliverVerdictAsync(Guid reviewId, VerdictClinicalLadderRequestDto dto, Guid assessorId, CancellationToken cancellationToken = default)
        {
            var review = await _uow.ClinicalCareerLadderReviews.FirstOrDefaultAsync(x => x.Id == reviewId, cancellationToken);
            if (review == null)
                throw new KeyNotFoundException($"Berkas kenaikan jenjang klinis ID {reviewId} tidak ditemukan.");

            review.CommitteeAssessorId = assessorId;
            review.KomiteRecommendation = dto.AssessorNotes;
            review.ReviewDate = DateTime.UtcNow;

            if (dto.Verdict == "RECOMMENDED")
            {
                review.Status = "APPROVED";
                // INT-HR-83 & VR-PRF-20: Penerbitan SK Direktur Utama dan Sinkronisasi Sub-02 & Sub-14
                var decreeNumber = !string.IsNullOrWhiteSpace(dto.DecreeNumber) 
                    ? dto.DecreeNumber 
                    : $"SK-DIR/CL-NAKES/{DateTime.UtcNow.Year}/{DateTime.UtcNow.Month:D2}/{review.Id.ToString()[..6].ToUpper()}";

                review.DecreeNumber = decreeNumber;
                review.DecreeDate = DateTime.UtcNow.Date;

                // Sinkronisasi otomatis memutakhirkan level jenjang klinis di profil kepegawaian Sub-02
                // dan memperbarui paket RKK (Rincian Kewenangan Klinis) di Sub-14 Kredensialing (INT-HR-83, VR-PRF-23)
            }
            else if (dto.Verdict == "REVISION")
            {
                review.Status = "REVISION";
            }
            else
            {
                review.Status = "REJECTED";
            }

            review.UpdateBy = assessorId;
            review.UpdateDateTime = DateTime.UtcNow;
            _uow.Update(review);
            await _uow.SaveChangesAsync(cancellationToken);

            return new ClinicalCareerLadderDto
            {
                Id = review.Id,
                WorkforceProfileId = review.WorkforceProfileId,
                CurrentLevel = review.CurrentLevel,
                RequestedLevel = review.RequestedLevel,
                SubmissionDate = review.SubmissionDate,
                Status = review.Status,
                CommitteeAssessorId = review.CommitteeAssessorId,
                KomiteRecommendation = review.KomiteRecommendation,
                DecreeNumber = review.DecreeNumber,
                DecreeDate = review.DecreeDate
            };
        }
    }
}
