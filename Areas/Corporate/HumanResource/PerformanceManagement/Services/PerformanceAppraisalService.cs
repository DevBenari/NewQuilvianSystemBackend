using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Services
{
    /// <summary>
    /// Layanan Domain Evaluasi Tiga Tingkat Berjenjang (Self -> Karu -> Kabid) & Formula Baku 60:40 (BE-HRD-259, PRF-B3, PRF-B4)
    /// </summary>
    public class PerformanceAppraisalService : IPerformanceAppraisalService
    {
        // Siklus yang dipakai bila klien tidak menyebut siklus: yang sedang dinilai lebih dulu, lalu kalibrasi, lalu penetapan sasaran.
        private static readonly string[] OpenCycleStatusPriority = { "ACTIVE_ASSESSMENT", "CALIBRATION", "GOAL_SETTING" };

        private readonly IPerformanceUnitOfWork _uow;
        private readonly ApplicationDbContext _context;

        public PerformanceAppraisalService(IPerformanceUnitOfWork uow, ApplicationDbContext context)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// MSL-02 (18 Nov 2026): profil pegawai ditentukan dari akun login oleh controller, siklus ditentukan server,
        /// dan GET tidak lagi membuat baris. Bila lembar belum ada, yang dikembalikan adalah draf belum tersimpan
        /// (Id kosong) berisi butir KPI aktif; lembar baru tersimpan saat pegawai mengirim evaluasi mandiri (POST self-submit).
        /// </summary>
        public async Task<PerformanceAppraisalDto> GetMyAppraisalAsync(Guid workforceProfileId, Guid? cycleId, CancellationToken cancellationToken = default)
        {
            var cycle = await ResolveCycleAsync(cycleId, cancellationToken)
                ?? throw new KeyNotFoundException("Belum ada siklus penilaian kinerja yang sedang berjalan. Penilaian dapat diisi setelah bagian SDM membuka siklus penilaian.");

            var appraisal = await _uow.PerformanceAppraisals
                .FirstOrDefaultAsync(x => x.WorkforceProfileId == workforceProfileId && x.PerformanceCycleId == cycle.Id, cancellationToken);

            if (appraisal != null)
            {
                return await MapToDtoAsync(appraisal, cancellationToken);
            }

            var catalogs = await _uow.KpiMetricCatalogs
                .Where(x => x.IsActive)
                .OrderBy(x => x.MetricCode)
                .ToListAsync(cancellationToken);
            var profileName = await GetProfileNameAsync(workforceProfileId, cancellationToken);

            return new PerformanceAppraisalDto
            {
                Id = Guid.Empty,
                PerformanceCycleId = cycle.Id,
                CycleName = cycle.CycleName,
                WorkforceProfileId = workforceProfileId,
                StaffName = profileName,
                Status = "DRAFT",
                Details = catalogs.Select(c => new PerformanceAppraisalDetailDto
                {
                    Id = Guid.Empty,
                    MetricCatalogId = c.Id,
                    MetricCode = c.MetricCode,
                    MetricName = c.MetricName,
                    ComponentType = c.ComponentType,
                    WeightPercentage = DefaultItemWeight,
                    TargetValue = c.DefaultTargetValue ?? 100m
                }).ToList()
            };
        }

        public async Task<PerformanceAppraisalDto> SubmitSelfAppraisalAsync(Guid workforceProfileId, SubmitSelfAppraisalRequestDto dto, CancellationToken cancellationToken = default)
        {
            var appraisal = await _uow.PerformanceAppraisals
                .FirstOrDefaultAsync(x => x.WorkforceProfileId == workforceProfileId && x.PerformanceCycleId == dto.PerformanceCycleId, cancellationToken);

            if (appraisal == null)
            {
                var cycle = await _uow.PerformanceCycles.FirstOrDefaultAsync(x => x.Id == dto.PerformanceCycleId, cancellationToken)
                    ?? throw new KeyNotFoundException("Siklus penilaian kinerja tidak ditemukan.");
                if (!OpenCycleStatusPriority.Contains(cycle.Status))
                {
                    throw new InvalidOperationException("Siklus penilaian ini sudah ditutup atau belum dibuka, sehingga evaluasi mandiri tidak dapat dikirim.");
                }

                // Lembar dibuat di aksi POST yang eksplisit, lengkap dengan atasan langsung dan atasan kedua dari penugasan atasan aktif.
                var firstLineManagerId = await ResolveActiveManagerAsync(workforceProfileId, cancellationToken)
                    ?? throw new InvalidOperationException("Atasan langsung Anda belum tercatat di data kepegawaian, sehingga penilaian belum bisa dikirim. Hubungi bagian SDM.");
                var secondLineManagerId = await ResolveActiveManagerAsync(firstLineManagerId, cancellationToken);

                appraisal = new HrdPerformanceAppraisal
                {
                    Id = Guid.NewGuid(),
                    PerformanceCycleId = dto.PerformanceCycleId,
                    WorkforceProfileId = workforceProfileId,
                    FirstLineManagerId = firstLineManagerId,
                    SecondLineManagerId = secondLineManagerId,
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
                var weight = DefaultItemWeight;

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

        public async Task<List<PerformanceAppraisalDto>> GetSubordinateAppraisalsAsync(Guid managerId, Guid? cycleId, CancellationToken cancellationToken = default)
        {
            var cycle = await ResolveCycleAsync(cycleId, cancellationToken);
            if (cycle == null)
            {
                return new List<PerformanceAppraisalDto>();
            }

            // MSL-02: hanya lembar bawahan langsung atau bawahan lini kedua milik atasan login.
            var appraisals = await _uow.PerformanceAppraisals
                .Where(x => x.PerformanceCycleId == cycle.Id && (x.FirstLineManagerId == managerId || x.SecondLineManagerId == managerId))
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
                throw new KeyNotFoundException("Lembar evaluasi kinerja tidak ditemukan.");

            if (appraisal.FirstLineManagerId != managerId)
            {
                throw new UnauthorizedAccessException("Lembar ini hanya dapat dinilai oleh atasan langsung pegawai yang bersangkutan.");
            }

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
                throw new KeyNotFoundException("Lembar evaluasi kinerja tidak ditemukan.");

            var expectedSecondLine = appraisal.SecondLineManagerId ?? await ResolveActiveManagerAsync(appraisal.FirstLineManagerId, cancellationToken);
            if (expectedSecondLine != secondLineId)
            {
                throw new UnauthorizedAccessException("Lembar ini hanya dapat ditelaah oleh atasan kedua (atasan dari atasan langsung) pegawai yang bersangkutan.");
            }

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
                throw new KeyNotFoundException("Lembar evaluasi kinerja tidak ditemukan.");

            if (appraisal.WorkforceProfileId != staffId)
            {
                throw new UnauthorizedAccessException("Hasil penilaian hanya dapat dikonfirmasi oleh pegawai pemilik lembar.");
            }

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
                StaffName = await GetProfileNameAsync(app.WorkforceProfileId, cancellationToken),
                UnitName = string.Empty,
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

        private const decimal DefaultItemWeight = 10m;

        private async Task<HrdPerformanceCycle?> ResolveCycleAsync(Guid? cycleId, CancellationToken cancellationToken)
        {
            if (cycleId.HasValue && cycleId.Value != Guid.Empty)
            {
                return await _uow.PerformanceCycles.FirstOrDefaultAsync(x => x.Id == cycleId.Value, cancellationToken);
            }

            var openCycles = await _uow.PerformanceCycles
                .Where(x => x.IsActive && OpenCycleStatusPriority.Contains(x.Status))
                .ToListAsync(cancellationToken);
            return openCycles
                .OrderBy(x => Array.IndexOf(OpenCycleStatusPriority, x.Status))
                .ThenByDescending(x => x.StartDate)
                .FirstOrDefault();
        }

        /// <summary>Atasan langsung aktif (MstWorkforceProfile.Id) dari penugasan atasan, atau null bila belum tercatat.</summary>
        private async Task<Guid?> ResolveActiveManagerAsync(Guid workforceProfileId, CancellationToken cancellationToken)
        {
            var today = DateTime.UtcNow.Date;
            return await _context.WfpManagerAssignments
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId &&
                            x.IsActive &&
                            !x.IsDelete &&
                            x.EffectiveStartDate <= today &&
                            (x.EffectiveEndDate == null || x.EffectiveEndDate >= today))
                .OrderByDescending(x => x.IsPrimaryManager)
                .ThenByDescending(x => x.EffectiveStartDate)
                .Select(x => (Guid?)x.ManagerWorkforceProfileId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        private async Task<string> GetProfileNameAsync(Guid workforceProfileId, CancellationToken cancellationToken)
        {
            var name = await _context.MstWorkforceProfiles
                .AsNoTracking()
                .Where(x => x.Id == workforceProfileId)
                .Select(x => x.DisplayName)
                .FirstOrDefaultAsync(cancellationToken);
            return string.IsNullOrWhiteSpace(name) ? "Pegawai" : name;
        }
    }
}
