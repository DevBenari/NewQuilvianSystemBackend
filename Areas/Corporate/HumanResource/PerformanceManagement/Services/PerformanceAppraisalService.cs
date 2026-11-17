using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Services
{
    /// <summary>
    /// Layanan Domain Evaluasi Tiga Tingkat Berjenjang (Self -> Karu -> Kabid) & Formula Baku 60:40 (BE-HRD-259, PRF-B3, PRF-B4)
    /// </summary>
    public class PerformanceAppraisalService : IPerformanceAppraisalService
    {
        private readonly IPerformanceUnitOfWork _uow;

        public PerformanceAppraisalService(IPerformanceUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task<PerformanceAppraisalDto> GetMyAppraisalAsync(Guid workforceProfileId, Guid cycleId, CancellationToken cancellationToken = default)
        {
            var appraisal = await _uow.PerformanceAppraisals
                .FirstOrDefaultAsync(x => x.WorkforceProfileId == workforceProfileId && x.PerformanceCycleId == cycleId, cancellationToken);

            if (appraisal == null)
            {
                // Create draft if not exists
                appraisal = new HrdPerformanceAppraisal
                {
                    Id = Guid.NewGuid(),
                    PerformanceCycleId = cycleId,
                    WorkforceProfileId = workforceProfileId,
                    Status = "DRAFT",
                    CreateBy = workforceProfileId,
                    CreateDateTime = DateTime.UtcNow,
                    UpdateBy = workforceProfileId,
                    UpdateDateTime = DateTime.UtcNow
                };
                await _uow.AddAsync(appraisal, cancellationToken);
                await _uow.SaveChangesAsync(cancellationToken);
            }

            return await MapToDtoAsync(appraisal, cancellationToken);
        }

        public async Task<PerformanceAppraisalDto> SubmitSelfAppraisalAsync(Guid workforceProfileId, SubmitSelfAppraisalRequestDto dto, CancellationToken cancellationToken = default)
        {
            var appraisal = await _uow.PerformanceAppraisals
                .FirstOrDefaultAsync(x => x.WorkforceProfileId == workforceProfileId && x.PerformanceCycleId == dto.PerformanceCycleId, cancellationToken);

            if (appraisal == null)
            {
                appraisal = new HrdPerformanceAppraisal
                {
                    Id = Guid.NewGuid(),
                    PerformanceCycleId = dto.PerformanceCycleId,
                    WorkforceProfileId = workforceProfileId,
                    Status = "DRAFT",
                    CreateBy = workforceProfileId,
                    CreateDateTime = DateTime.UtcNow
                };
                await _uow.AddAsync(appraisal, cancellationToken);
            }

            if (appraisal.Status != "DRAFT" && appraisal.Status != "REVISION_REQUESTED")
            {
                throw new InvalidOperationException($"Pengisian evaluasi mandiri tidak dapat dilakukan saat lembar berstatus '{appraisal.Status}'.");
            }

            // Hapus detail sebelumnya jika ada
            var oldDetails = await _uow.PerformanceAppraisalDetails
                .Where(x => x.PerformanceAppraisalId == appraisal.Id)
                .ToListAsync(cancellationToken);
            foreach (var od in oldDetails)
            {
                _uow.SoftDelete(od, workforceProfileId);
            }

            decimal totalQuantSelfScore = 0m;
            decimal totalQuantWeight = 0m;
            decimal totalBehSelfScore = 0m;
            int behCount = 0;

            foreach (var item in dto.Details)
            {
                var catalog = await _uow.KpiMetricCatalogs.FirstOrDefaultAsync(x => x.Id == item.MetricCatalogId, cancellationToken);
                var weight = 10m; // Default weight per item

                // VR-PRF-22: Wajib bukti dukung untuk nilai ekstrem (Rating 5 atau Capaian > 120%)
                if (item.RatingScore >= 5.0m && string.IsNullOrWhiteSpace(item.EvidenceUrl))
                {
                    throw new ArgumentException("Penilaian mandiri dengan skor maksimal (5.0) wajib melampirkan URL dokumen bukti capaian yang sah (VR-PRF-22).");
                }

                var detail = new HrdPerformanceAppraisalDetail
                {
                    Id = Guid.NewGuid(),
                    PerformanceAppraisalId = appraisal.Id,
                    MetricCatalogId = item.MetricCatalogId,
                    WeightPercentage = weight,
                    TargetValue = catalog?.DefaultTargetValue ?? 100m,
                    ActualValue = item.ActualValue,
                    SelfRatingScore = item.RatingScore,
                    EvidenceUrl = item.EvidenceUrl,
                    Comments = item.Comments,
                    CreateBy = workforceProfileId,
                    CreateDateTime = DateTime.UtcNow,
                    UpdateBy = workforceProfileId,
                    UpdateDateTime = DateTime.UtcNow
                };
                await _uow.AddAsync(detail, cancellationToken);

                if (catalog?.ComponentType == "CORE_VALUES_BEHAVIOR")
                {
                    totalBehSelfScore += item.RatingScore;
                    behCount++;
                }
                else
                {
                    totalQuantSelfScore += item.RatingScore * (weight / 100m);
                    totalQuantWeight += weight;
                }
            }

            decimal quantScoreNormalized = totalQuantWeight > 0 ? (totalQuantSelfScore / totalQuantWeight) * 100m : 80m;
            decimal behScoreNormalized = behCount > 0 ? (totalBehSelfScore / (behCount * 5.0m)) * 100m : 80m;

            appraisal.SelfScoreQuantitative = Math.Round(quantScoreNormalized, 2);
            appraisal.SelfScoreBehavioral = Math.Round(behScoreNormalized, 2);
            appraisal.SelfOverallScore = Math.Round((0.60m * appraisal.SelfScoreQuantitative) + (0.40m * appraisal.SelfScoreBehavioral), 2);
            appraisal.SelfAssessmentDate = DateTime.UtcNow;
            appraisal.SelfNotes = dto.SelfNotes;
            appraisal.Status = "SELF_SUBMITTED"; // PRF-B3
            appraisal.UpdateBy = workforceProfileId;
            appraisal.UpdateDateTime = DateTime.UtcNow;

            _uow.Update(appraisal);
            await _uow.SaveChangesAsync(cancellationToken);

            return await MapToDtoAsync(appraisal, cancellationToken);
        }

        public async Task<List<PerformanceAppraisalDto>> GetSubordinateAppraisalsAsync(Guid managerId, Guid cycleId, CancellationToken cancellationToken = default)
        {
            var appraisals = await _uow.PerformanceAppraisals
                .Where(x => x.PerformanceCycleId == cycleId)
                .ToListAsync(cancellationToken);

            var result = new List<PerformanceAppraisalDto>();
            foreach (var app in appraisals)
            {
                result.Add(await MapToDtoAsync(app, cancellationToken));
            }
            return result;
        }

        public async Task<PerformanceAppraisalDto> SubmitManagerAppraisalAsync(Guid appraisalId, Guid managerId, SubmitManagerAppraisalRequestDto dto, CancellationToken cancellationToken = default)
        {
            var appraisal = await _uow.PerformanceAppraisals.FirstOrDefaultAsync(x => x.Id == appraisalId, cancellationToken);
            if (appraisal == null)
                throw new KeyNotFoundException($"Lembar evaluasi kinerja ID {appraisalId} tidak ditemukan.");

            // PRF-B3 & VR-PRF-04: Pegawai harus menuntaskan evaluasi mandiri terlebih dahulu
            if (appraisal.Status != "SELF_SUBMITTED" && appraisal.Status != "MANAGER_REVIEWED")
            {
                throw new InvalidOperationException("Evaluasi atasan hanya dapat dilakukan setelah pegawai menyerahkan evaluasi mandiri (VR-PRF-04, PRF-B3).");
            }

            var details = await _uow.PerformanceAppraisalDetails
                .Where(x => x.PerformanceAppraisalId == appraisalId)
                .ToListAsync(cancellationToken);

            decimal totalQuantScore = 0m;
            decimal totalQuantWeight = 0m;
            decimal totalBehScore = 0m;
            int behCount = 0;

            foreach (var updateItem in dto.Details)
            {
                var detail = details.FirstOrDefault(x => x.Id == updateItem.AppraisalDetailId);
                if (detail != null)
                {
                    detail.ManagerRatingScore = updateItem.ManagerRatingScore;
                    detail.Comments = updateItem.ManagerComments ?? detail.Comments;
                    detail.UpdateBy = managerId;
                    detail.UpdateDateTime = DateTime.UtcNow;
                    _uow.Update(detail);

                    var cat = await _uow.KpiMetricCatalogs.FirstOrDefaultAsync(x => x.Id == detail.MetricCatalogId, cancellationToken);
                    if (cat?.ComponentType == "CORE_VALUES_BEHAVIOR")
                    {
                        totalBehScore += updateItem.ManagerRatingScore;
                        behCount++;
                    }
                    else
                    {
                        totalQuantScore += updateItem.ManagerRatingScore * (detail.WeightPercentage / 100m);
                        totalQuantWeight += detail.WeightPercentage;
                    }
                }
            }

            // PRF-B4: Formula baku 60:40
            decimal quantNormalized = totalQuantWeight > 0 ? (totalQuantScore / totalQuantWeight) * 100m : 85m;
            decimal behNormalized = behCount > 0 ? (totalBehScore / (behCount * 5.0m)) * 100m : 85m;

            appraisal.ManagerScoreQuantitative = Math.Round(quantNormalized, 2);
            appraisal.ManagerScoreBehavioral = Math.Round(behNormalized, 2);
            appraisal.ManagerOverallScore = Math.Round((0.60m * appraisal.ManagerScoreQuantitative) + (0.40m * appraisal.ManagerScoreBehavioral), 2);
            appraisal.FinalScore = appraisal.ManagerOverallScore;
            appraisal.FinalGrade = DetermineGrade(appraisal.FinalScore);
            appraisal.ManagerId = managerId;
            appraisal.ManagerAssessmentDate = DateTime.UtcNow;
            appraisal.ManagerFeedbackNotes = dto.ManagerFeedbackNotes;
            appraisal.Status = "MANAGER_REVIEWED"; // PRF-B3
            appraisal.UpdateBy = managerId;
            appraisal.UpdateDateTime = DateTime.UtcNow;

            _uow.Update(appraisal);
            await _uow.SaveChangesAsync(cancellationToken);

            return await MapToDtoAsync(appraisal, cancellationToken);
        }

        public async Task<PerformanceAppraisalDto> ProcessSecondLineReviewAsync(Guid appraisalId, Guid secondLineId, ProcessSecondLineReviewDto dto, CancellationToken cancellationToken = default)
        {
            var appraisal = await _uow.PerformanceAppraisals.FirstOrDefaultAsync(x => x.Id == appraisalId, cancellationToken);
            if (appraisal == null)
                throw new KeyNotFoundException($"Lembar evaluasi kinerja ID {appraisalId} tidak ditemukan.");

            if (appraisal.Status != "MANAGER_REVIEWED")
            {
                throw new InvalidOperationException("Penelaahan atasan kedua (Kabid) hanya dapat dilakukan setelah atasan langsung (Karu) menyelesaikan penilaian resmi (PRF-B3).");
            }

            if (dto.Decision == "APPROVE")
            {
                appraisal.Status = "SECOND_LINE_APPROVED";
            }
            else
            {
                appraisal.Status = "REVISION_REQUESTED";
            }

            appraisal.SecondLineReviewerId = secondLineId;
            appraisal.SecondLineReviewDate = DateTime.UtcNow;
            appraisal.SecondLineReviewNotes = dto.SecondLineNotes;
            appraisal.UpdateBy = secondLineId;
            appraisal.UpdateDateTime = DateTime.UtcNow;

            _uow.Update(appraisal);
            await _uow.SaveChangesAsync(cancellationToken);

            return await MapToDtoAsync(appraisal, cancellationToken);
        }

        public async Task<PerformanceAppraisalDto> AcknowledgeAppraisalAsync(Guid appraisalId, Guid staffId, AcknowledgeAppraisalRequestDto dto, CancellationToken cancellationToken = default)
        {
            var appraisal = await _uow.PerformanceAppraisals.FirstOrDefaultAsync(x => x.Id == appraisalId, cancellationToken);
            if (appraisal == null)
                throw new KeyNotFoundException($"Lembar evaluasi kinerja ID {appraisalId} tidak ditemukan.");

            if (appraisal.Status != "RELEASED")
            {
                throw new InvalidOperationException("Konfirmasi penerimaan nilai oleh pegawai hanya dapat dilakukan setelah skor resmi dirilis pasca sidang kalibrasi.");
            }

            appraisal.IsAcknowledgedByEmployee = true;
            appraisal.EmployeeAcknowledgmentDate = DateTime.UtcNow;
            appraisal.EmployeeAcknowledgmentNotes = dto.EmployeeFeedbackNotes;
            appraisal.Status = "ACKNOWLEDGED";
            appraisal.UpdateBy = staffId;
            appraisal.UpdateDateTime = DateTime.UtcNow;

            _uow.Update(appraisal);
            await _uow.SaveChangesAsync(cancellationToken);

            return await MapToDtoAsync(appraisal, cancellationToken);
        }

        private static string DetermineGrade(decimal score)
        {
            if (score >= 90.0m) return "SANGAT_BAIK";
            if (score >= 75.0m) return "BAIK";
            if (score >= 65.0m) return "CUKUP";
            if (score >= 50.0m) return "KURANG";
            return "SANGAT_KURANG";
        }

        private async Task<PerformanceAppraisalDto> MapToDtoAsync(HrdPerformanceAppraisal app, CancellationToken cancellationToken)
        {
            var cycle = await _uow.PerformanceCycles.FirstOrDefaultAsync(x => x.Id == app.PerformanceCycleId, cancellationToken);
            var details = await _uow.PerformanceAppraisalDetails
                .Where(x => x.PerformanceAppraisalId == app.Id)
                .ToListAsync(cancellationToken);

            var metricIds = details.Select(d => d.MetricCatalogId).Distinct().ToList();
            var metrics = await _uow.KpiMetricCatalogs
                .Where(m => metricIds.Contains(m.Id))
                .ToDictionaryAsync(m => m.Id, cancellationToken);

            return new PerformanceAppraisalDto
            {
                Id = app.Id,
                PerformanceCycleId = app.PerformanceCycleId,
                CycleName = cycle?.CycleName ?? string.Empty,
                WorkforceProfileId = app.WorkforceProfileId,
                StaffName = $"Pegawai ({app.WorkforceProfileId.ToString()[..8]})",
                UnitName = "Instalasi Rawat Inap",
                Status = app.Status,
                SelfScoreQuantitative = app.SelfScoreQuantitative,
                SelfScoreBehavioral = app.SelfScoreBehavioral,
                SelfOverallScore = app.SelfOverallScore,
                ManagerScoreQuantitative = app.ManagerScoreQuantitative,
                ManagerScoreBehavioral = app.ManagerScoreBehavioral,
                ManagerOverallScore = app.ManagerOverallScore,
                CalibratedScore = app.CalibratedScore,
                FinalScore = app.FinalScore,
                FinalGrade = app.FinalGrade,
                HasDispute = app.HasDispute,
                ManagerFeedbackNotes = app.ManagerFeedbackNotes,
                SecondLineReviewNotes = app.SecondLineReviewNotes,
                EmployeeAcknowledgmentNotes = app.EmployeeAcknowledgmentNotes,
                Details = details.Select(d => new PerformanceAppraisalDetailDto
                {
                    Id = d.Id,
                    MetricCatalogId = d.MetricCatalogId,
                    MetricCode = metrics.TryGetValue(d.MetricCatalogId, out var mc) ? mc.MetricCode : string.Empty,
                    MetricName = metrics.TryGetValue(d.MetricCatalogId, out var mc2) ? mc2.MetricName : string.Empty,
                    ComponentType = metrics.TryGetValue(d.MetricCatalogId, out var mc3) ? mc3.ComponentType : string.Empty,
                    WeightPercentage = d.WeightPercentage,
                    TargetValue = d.TargetValue,
                    ActualValue = d.ActualValue,
                    SelfRatingScore = d.SelfRatingScore,
                    ManagerRatingScore = d.ManagerRatingScore,
                    WeightedScore = d.WeightedScore,
                    EvidenceUrl = d.EvidenceUrl,
                    Comments = d.Comments
                }).ToList()
            };
        }
    }
}
