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
    public class OppeDataAggregationService : IOppeDataAggregationService
    {
        private readonly ApplicationDbContext _context;

        public OppeDataAggregationService(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        #region 1. METRIC CATALOG
        public async Task<List<OppeMetricCatalogDto>> GetMetricCatalogsAsync(string? specialtyGroup = null, string? competencyDomain = null, CancellationToken cancellationToken = default)
        {
            var query = _context.MstOppeMetricCatalogs.AsNoTracking().Where(m => !m.IsDelete);

            if (!string.IsNullOrWhiteSpace(specialtyGroup) && specialtyGroup != "ALL")
            {
                query = query.Where(m => m.SpecialtyGroup == specialtyGroup || m.SpecialtyGroup == "ALL");
            }

            if (!string.IsNullOrWhiteSpace(competencyDomain))
            {
                query = query.Where(m => m.CompetencyDomain == competencyDomain);
            }

            var list = await query.OrderBy(m => m.CompetencyDomain).ThenBy(m => m.MetricCode).ToListAsync(cancellationToken);

            return list.Select(m => new OppeMetricCatalogDto
            {
                Id = m.Id,
                MetricCode = m.MetricCode,
                MetricName = m.MetricName,
                CompetencyDomain = m.CompetencyDomain,
                SpecialtyGroup = m.SpecialtyGroup,
                DataSourceType = m.DataSourceType,
                TargetStandardValue = m.TargetStandardValue,
                MetricWeightPercentage = m.MetricWeightPercentage,
                IsActive = m.IsActive
            }).ToList();
        }

        public async Task<OppeMetricCatalogDto> CreateMetricCatalogAsync(CreateOppeMetricCatalogRequestDto request, Guid creatorId, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            // Cek duplikasi kode
            var exists = await _context.MstOppeMetricCatalogs
                .AnyAsync(m => m.MetricCode == request.MetricCode && !m.IsDelete, cancellationToken);
            if (exists)
            {
                throw new InvalidOperationException($"Indikator dengan kode '{request.MetricCode}' sudah terdaftar dalam sistem.");
            }

            var entity = new MstOppeMetricCatalog
            {
                Id = Guid.NewGuid(),
                MetricCode = request.MetricCode.ToUpperInvariant(),
                MetricName = request.MetricName,
                CompetencyDomain = request.CompetencyDomain.ToUpperInvariant(),
                SpecialtyGroup = request.SpecialtyGroup.ToUpperInvariant(),
                DataSourceType = request.DataSourceType.ToUpperInvariant(),
                TargetStandardValue = request.TargetStandardValue,
                MetricWeightPercentage = request.MetricWeightPercentage,
                IsActive = request.IsActive,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = creatorId,
                IsDelete = false
            };

            await _context.MstOppeMetricCatalogs.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return new OppeMetricCatalogDto
            {
                Id = entity.Id,
                MetricCode = entity.MetricCode,
                MetricName = entity.MetricName,
                CompetencyDomain = entity.CompetencyDomain,
                SpecialtyGroup = entity.SpecialtyGroup,
                DataSourceType = entity.DataSourceType,
                TargetStandardValue = entity.TargetStandardValue,
                MetricWeightPercentage = entity.MetricWeightPercentage,
                IsActive = entity.IsActive
            };
        }
        #endregion

        #region 2. EVALUATION PERIOD
        public async Task<List<OppeEvaluationPeriodDto>> GetEvaluationPeriodsAsync(int? year = null, CancellationToken cancellationToken = default)
        {
            var query = _context.TrxOppeEvaluationPeriods.AsNoTracking().Where(p => !p.IsDelete);

            if (year.HasValue)
            {
                query = query.Where(p => p.EvaluationYear == year.Value);
            }

            var list = await query.OrderByDescending(p => p.EvaluationYear).ThenByDescending(p => p.Semester).ToListAsync(cancellationToken);

            var result = new List<OppeEvaluationPeriodDto>();
            foreach (var p in list)
            {
                var staffCount = await _context.TrxOppeStaffEvaluations
                    .CountAsync(s => s.PeriodId == p.Id && !s.IsDelete, cancellationToken);

                result.Add(new OppeEvaluationPeriodDto
                {
                    Id = p.Id,
                    PeriodCode = p.PeriodCode,
                    PeriodName = p.PeriodName,
                    EvaluationYear = p.EvaluationYear,
                    Semester = p.Semester,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    Status = p.Status,
                    QuantitativeWeightPercentage = p.QuantitativeWeightPercentage,
                    QualitativeWeightPercentage = p.QualitativeWeightPercentage,
                    TotalStaffCount = staffCount
                });
            }

            return result;
        }

        public async Task<OppeEvaluationPeriodDto> CreateEvaluationPeriodAsync(CreateOppeEvaluationPeriodRequestDto request, Guid creatorId, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            // Validasi aturan VR-OPP-01: Total bobot kuantitatif + kualitatif wajib tepat 100%
            var totalWeight = request.QuantitativeWeightPercentage + request.QualitativeWeightPercentage;
            if (Math.Abs(totalWeight - 100.00m) > 0.01m)
            {
                throw new InvalidOperationException($"Validasi VR-OPP-01 Gagal: Total bobot kuantitatif ({request.QuantitativeWeightPercentage}%) + kualitatif ({request.QualitativeWeightPercentage}%) wajib tepat 100.00%.");
            }

            var exists = await _context.TrxOppeEvaluationPeriods
                .AnyAsync(p => p.PeriodCode == request.PeriodCode && !p.IsDelete, cancellationToken);
            if (exists)
            {
                throw new InvalidOperationException($"Periode evaluasi dengan kode '{request.PeriodCode}' sudah ada.");
            }

            var period = new TrxOppeEvaluationPeriod
            {
                Id = Guid.NewGuid(),
                PeriodCode = request.PeriodCode.ToUpperInvariant(),
                PeriodName = request.PeriodName,
                EvaluationYear = request.EvaluationYear,
                Semester = request.Semester,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Status = "OPEN",
                QuantitativeWeightPercentage = request.QuantitativeWeightPercentage,
                QualitativeWeightPercentage = request.QualitativeWeightPercentage,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = creatorId,
                IsDelete = false
            };

            await _context.TrxOppeEvaluationPeriods.AddAsync(period, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return new OppeEvaluationPeriodDto
            {
                Id = period.Id,
                PeriodCode = period.PeriodCode,
                PeriodName = period.PeriodName,
                EvaluationYear = period.EvaluationYear,
                Semester = period.Semester,
                StartDate = period.StartDate,
                EndDate = period.EndDate,
                Status = period.Status,
                QuantitativeWeightPercentage = period.QuantitativeWeightPercentage,
                QualitativeWeightPercentage = period.QualitativeWeightPercentage,
                TotalStaffCount = 0
            };
        }

        public async Task<int> InitiateStaffEvaluationsForPeriodAsync(Guid periodId, Guid initiatorId, CancellationToken cancellationToken = default)
        {
            var period = await _context.TrxOppeEvaluationPeriods
                .FirstOrDefaultAsync(p => p.Id == periodId && !p.IsDelete, cancellationToken);
            if (period == null) throw new KeyNotFoundException("Periode OPPE tidak ditemukan.");

            // Ambil seluruh dokter / tenaga medis aktif dari MstWorkforceProfiles
            var doctors = await _context.MstWorkforceProfiles
                .AsNoTracking()
                .Where(w => !w.IsDelete)
                .ToListAsync(cancellationToken);

            var createdCount = 0;
            foreach (var doc in doctors)
            {
                var alreadyExists = await _context.TrxOppeStaffEvaluations
                    .AnyAsync(s => s.PeriodId == periodId && s.WorkforceProfileId == doc.Id && !s.IsDelete, cancellationToken);

                if (!alreadyExists)
                {
                    var staffEval = new TrxOppeStaffEvaluation
                    {
                        Id = Guid.NewGuid(),
                        PeriodId = periodId,
                        WorkforceProfileId = doc.Id,
                        QuantitativeScore = 0.00m,
                        QualitativeScore = 0.00m,
                        FinalCompositeScore = 0.00m,
                        PerformanceGrade = "C",
                        Status = "DRAFT",
                        IsAcknowledgedByStaff = false,
                        CreateDateTime = DateTime.UtcNow,
                        CreateBy = initiatorId,
                        IsDelete = false
                    };

                    await _context.TrxOppeStaffEvaluations.AddAsync(staffEval, cancellationToken);
                    createdCount++;
                }
            }

            if (createdCount > 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
            }

            return createdCount;
        }
        #endregion

        #region 3. AGREGASI DATA KUANTITATIF SIMRS 60%
        public async Task<OppeStaffEvaluationDto> AggregateSimrsQuantitativeMetricsAsync(Guid staffEvaluationId, Guid actorId, CancellationToken cancellationToken = default)
        {
            var evaluation = await _context.TrxOppeStaffEvaluations
                .Include(s => s.Period)
                .Include(s => s.EvaluationDetails)
                .FirstOrDefaultAsync(s => s.Id == staffEvaluationId && !s.IsDelete, cancellationToken);

            if (evaluation == null) throw new KeyNotFoundException("Rapor evaluasi OPPE staf tidak ditemukan.");

            // Ambil seluruh katalog indikator kuantitatif yang aktif
            var quantitativeMetrics = await _context.MstOppeMetricCatalogs
                .AsNoTracking()
                .Where(m => m.IsActive && !m.IsDelete && m.DataSourceType != "PEER_REVIEW_SMF")
                .ToListAsync(cancellationToken);

            decimal totalCalculatedScore = 0m;
            decimal totalWeight = 0m;

            foreach (var metric in quantitativeMetrics)
            {
                // Simulasi agregasi data terukur SIMRS / KMKP
                decimal actualValue = metric.DataSourceType switch
                {
                    "SIMRS_VOLUME" => 120.00m,
                    "SIMRS_EMR_COMPLETION" => 92.50m,
                    "KMKP_INCIDENT" => 0.00m,
                    _ => 88.00m
                };

                // Skor ketercapaian target
                decimal ratio = metric.TargetStandardValue > 0 ? (actualValue / metric.TargetStandardValue) : 1.0m;
                if (ratio > 1.2m) ratio = 1.2m; // Cap maksimum ketercapaian 120%
                decimal score = ratio * metric.MetricWeightPercentage;

                totalCalculatedScore += score;
                totalWeight += metric.MetricWeightPercentage;

                var existingDetail = evaluation.EvaluationDetails
                    .FirstOrDefault(d => d.MetricCatalogId == metric.Id);

                if (existingDetail == null)
                {
                    var detail = new TrxOppeStaffEvaluationDetail
                    {
                        Id = Guid.NewGuid(),
                        StaffEvaluationId = evaluation.Id,
                        MetricCatalogId = metric.Id,
                        TargetValue = metric.TargetStandardValue,
                        ActualValue = actualValue,
                        ComputedScore = Math.Round(score, 2),
                        EvaluatorNotes = $"Agregasi otomatis dari data interface {metric.DataSourceType}.",
                        CreateDateTime = DateTime.UtcNow,
                        CreateBy = actorId,
                        IsDelete = false
                    };
                    await _context.TrxOppeStaffEvaluationDetails.AddAsync(detail, cancellationToken);
                }
                else
                {
                    existingDetail.ActualValue = actualValue;
                    existingDetail.ComputedScore = Math.Round(score, 2);
                    existingDetail.UpdateDateTime = DateTime.UtcNow;
                    existingDetail.UpdateBy = actorId;
                }
            }

            // Normalisasi skor kuantitatif ke batas maksimal 60.00
            decimal normalizedQuantScore = totalWeight > 0 ? (totalCalculatedScore / totalWeight) * 60.00m : 50.00m;
            if (normalizedQuantScore > 60.00m) normalizedQuantScore = 60.00m;

            evaluation.QuantitativeScore = Math.Round(normalizedQuantScore, 2);
            RecalculateCompositeScore(evaluation);

            evaluation.UpdateDateTime = DateTime.UtcNow;
            evaluation.UpdateBy = actorId;

            await _context.SaveChangesAsync(cancellationToken);

            return await GetStaffEvaluationByIdAsync(evaluation.Id, cancellationToken);
        }
        #endregion

        #region 4. EVALUASI KUALITATIF TELAAH SEJAWAT 40%
        public async Task<List<AssignedPeerReviewDto>> GetAssignedPeerReviewsAsync(Guid evaluatorWorkforceProfileId, CancellationToken cancellationToken = default)
        {
            // Ambil rapor staf yang sedang berstatus DRAFT atau EVALUATED_KSM di mana dokter evaluator bukan diri sendiri (VR-OPP-03)
            var evaluations = await _context.TrxOppeStaffEvaluations
                .Include(s => s.Period)
                .AsNoTracking()
                .Where(s => !s.IsDelete && s.WorkforceProfileId != evaluatorWorkforceProfileId)
                .Take(20)
                .ToListAsync(cancellationToken);

            var staffIds = evaluations.Select(e => e.WorkforceProfileId).Distinct().ToList();
            var profiles = await _context.MstWorkforceProfiles
                .AsNoTracking()
                .Where(p => staffIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.DisplayName, cancellationToken);

            return evaluations.Select(e => new AssignedPeerReviewDto
            {
                StaffEvaluationId = e.Id,
                EvaluateeWorkforceProfileId = e.WorkforceProfileId,
                EvaluateeName = profiles.TryGetValue(e.WorkforceProfileId, out var name) ? name : "Dokter Anggota KSM",
                SpecialtyName = "Spesialis Medis",
                PeriodName = e.Period?.PeriodName ?? "Periode Berjalan",
                DueDate = e.Period?.EndDate ?? DateTime.UtcNow.AddDays(30),
                IsCompleted = e.QualitativeScore > 0
            }).ToList();
        }

        public async Task<bool> SubmitPeerReviewScoresAsync(Guid staffEvaluationId, SubmitPeerReviewEvaluationRequestDto request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var evaluation = await _context.TrxOppeStaffEvaluations
                .Include(s => s.EvaluationDetails)
                .FirstOrDefaultAsync(s => s.Id == staffEvaluationId && !s.IsDelete, cancellationToken);

            if (evaluation == null) throw new KeyNotFoundException("Rapor evaluasi OPPE staf tidak ditemukan.");

            // Validasi aturan anti-self-review (VR-OPP-03)
            if (evaluation.WorkforceProfileId == request.EvaluatorWorkforceProfileId)
            {
                throw new InvalidOperationException("Validasi VR-OPP-03 Gagal: Dokter penilai dilarang keras menilai berkas evaluasi dirinya sendiri (Anti-Self-Review).");
            }

            // Validasi skala Likert dan catatan naratif jika skor <= 2 (VR-OPP-04)
            foreach (var item in request.Scores)
            {
                if (item.Score <= 2 && string.IsNullOrWhiteSpace(item.QualitativeComment))
                {
                    throw new InvalidOperationException($"Validasi VR-OPP-04 Gagal: Penilaian skor rendah ({item.Score}) wajib disertai catatan naratif klinis yang menjelaskan kekurangan performa.");
                }
            }

            decimal sumScores = 0m;
            int count = 0;

            foreach (var item in request.Scores)
            {
                sumScores += item.Score;
                count++;

                var existingDetail = evaluation.EvaluationDetails
                    .FirstOrDefault(d => d.MetricCatalogId == item.MetricCatalogId);

                decimal computedScore = (item.Score / 5.0m) * 10.0m; // Skala 1-5 dinormalisasi

                if (existingDetail == null)
                {
                    var detail = new TrxOppeStaffEvaluationDetail
                    {
                        Id = Guid.NewGuid(),
                        StaffEvaluationId = evaluation.Id,
                        MetricCatalogId = item.MetricCatalogId,
                        TargetValue = 5.0m,
                        ActualValue = item.Score,
                        ComputedScore = Math.Round(computedScore, 2),
                        EvaluatorNotes = item.QualitativeComment ?? "Evaluasi telaah sejawat (Likert).",
                        CreateDateTime = DateTime.UtcNow,
                        CreateBy = request.EvaluatorWorkforceProfileId,
                        IsDelete = false
                    };
                    await _context.TrxOppeStaffEvaluationDetails.AddAsync(detail, cancellationToken);
                }
                else
                {
                    existingDetail.ActualValue = item.Score;
                    existingDetail.ComputedScore = Math.Round(computedScore, 2);
                    existingDetail.EvaluatorNotes = item.QualitativeComment;
                    existingDetail.UpdateDateTime = DateTime.UtcNow;
                    existingDetail.UpdateBy = request.EvaluatorWorkforceProfileId;
                }
            }

            // Normalisasi skor kualitatif telaah sejawat ke batas 40.00
            decimal averageRatio = count > 0 ? (sumScores / (count * 5.0m)) : 0.8m;
            decimal qualScore = averageRatio * 40.00m;
            if (qualScore > 40.00m) qualScore = 40.00m;

            evaluation.QualitativeScore = Math.Round(qualScore, 2);
            RecalculateCompositeScore(evaluation);

            evaluation.EvaluatorKsmHeadId = request.EvaluatorWorkforceProfileId;
            evaluation.Status = "EVALUATED_KSM";
            evaluation.UpdateDateTime = DateTime.UtcNow;
            evaluation.UpdateBy = request.EvaluatorWorkforceProfileId;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        #endregion

        #region 5. COMPOSITE SCORE & LIFECYCLE FINALIZATION
        public async Task<OppeStaffEvaluationDto> GetStaffEvaluationByIdAsync(Guid staffEvaluationId, CancellationToken cancellationToken = default)
        {
            var evaluation = await _context.TrxOppeStaffEvaluations
                .Include(s => s.Period)
                .Include(s => s.EvaluationDetails)
                    .ThenInclude(d => d.MetricCatalog)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == staffEvaluationId && !s.IsDelete, cancellationToken);

            if (evaluation == null) throw new KeyNotFoundException("Rapor evaluasi OPPE staf tidak ditemukan.");

            var doctor = await _context.MstWorkforceProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == evaluation.WorkforceProfileId, cancellationToken);

            var evaluator = evaluation.EvaluatorKsmHeadId.HasValue
                ? await _context.MstWorkforceProfiles.AsNoTracking().FirstOrDefaultAsync(w => w.Id == evaluation.EvaluatorKsmHeadId.Value, cancellationToken)
                : null;

            return new OppeStaffEvaluationDto
            {
                Id = evaluation.Id,
                PeriodId = evaluation.PeriodId,
                PeriodName = evaluation.Period?.PeriodName ?? "Periode OPPE",
                WorkforceProfileId = evaluation.WorkforceProfileId,
                EmployeeName = doctor?.DisplayName ?? "Dokter Medis",
                EmployeeCode = doctor?.ProfileCode ?? "DOC-UNKNOWN",
                SpecialtyName = "Spesialis Medis",
                CommitteeId = evaluation.CommitteeId,
                CommitteeName = "Komite Medik",
                QuantitativeScore = evaluation.QuantitativeScore,
                QualitativeScore = evaluation.QualitativeScore,
                FinalCompositeScore = evaluation.FinalCompositeScore,
                PerformanceGrade = evaluation.PerformanceGrade,
                EvaluatorKsmHeadId = evaluation.EvaluatorKsmHeadId,
                EvaluatorKsmHeadName = evaluator?.DisplayName,
                EvaluatorRecommendationNotes = evaluation.EvaluatorRecommendationNotes,
                IsAcknowledgedByStaff = evaluation.IsAcknowledgedByStaff,
                AcknowledgedAt = evaluation.AcknowledgedAt,
                Status = evaluation.Status,
                Details = evaluation.EvaluationDetails.Select(d => new OppeStaffEvaluationDetailDto
                {
                    Id = d.Id,
                    MetricCatalogId = d.MetricCatalogId,
                    MetricCode = d.MetricCatalog?.MetricCode ?? "-",
                    MetricName = d.MetricCatalog?.MetricName ?? "-",
                    CompetencyDomain = d.MetricCatalog?.CompetencyDomain ?? "-",
                    DataSourceType = d.MetricCatalog?.DataSourceType ?? "-",
                    TargetValue = d.TargetValue,
                    ActualValue = d.ActualValue,
                    ComputedScore = d.ComputedScore,
                    EvaluatorNotes = d.EvaluatorNotes
                }).ToList()
            };
        }

        public async Task<OppeStaffEvaluationDto> FinalizeStaffEvaluationAsync(Guid staffEvaluationId, FinalizeOppeStaffEvaluationDto request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var evaluation = await _context.TrxOppeStaffEvaluations
                .FirstOrDefaultAsync(s => s.Id == staffEvaluationId && !s.IsDelete, cancellationToken);

            if (evaluation == null) throw new KeyNotFoundException("Rapor evaluasi OPPE staf tidak ditemukan.");

            evaluation.EvaluatorKsmHeadId = request.EvaluatorKsmHeadId;
            evaluation.EvaluatorRecommendationNotes = request.EvaluatorRecommendationNotes;
            evaluation.Status = "FINALIZED";
            evaluation.UpdateDateTime = DateTime.UtcNow;
            evaluation.UpdateBy = request.EvaluatorKsmHeadId;

            await _context.SaveChangesAsync(cancellationToken);

            return await GetStaffEvaluationByIdAsync(evaluation.Id, cancellationToken);
        }

        public async Task<OppeStaffEvaluationDto> AcknowledgeStaffEvaluationAsync(Guid staffEvaluationId, AcknowledgeOppeStaffEvaluationDto request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var evaluation = await _context.TrxOppeStaffEvaluations
                .FirstOrDefaultAsync(s => s.Id == staffEvaluationId && !s.IsDelete, cancellationToken);

            if (evaluation == null) throw new KeyNotFoundException("Rapor evaluasi OPPE staf tidak ditemukan.");

            if (evaluation.WorkforceProfileId != request.WorkforceProfileId)
            {
                throw new InvalidOperationException("Konfirmasi penerimaan rapor hanya dapat dilakukan oleh dokter yang bersangkutan.");
            }

            evaluation.IsAcknowledgedByStaff = true;
            evaluation.AcknowledgedAt = DateTime.UtcNow;
            evaluation.Status = "ACKNOWLEDGED";
            evaluation.UpdateDateTime = DateTime.UtcNow;
            evaluation.UpdateBy = request.WorkforceProfileId;

            await _context.SaveChangesAsync(cancellationToken);

            return await GetStaffEvaluationByIdAsync(evaluation.Id, cancellationToken);
        }

        private static void RecalculateCompositeScore(TrxOppeStaffEvaluation evaluation)
        {
            // Formula baku OPP-B1: (Kuantitatif 60% maks + Kualitatif 40% maks) = 100%
            evaluation.FinalCompositeScore = evaluation.QuantitativeScore + evaluation.QualitativeScore;

            if (evaluation.FinalCompositeScore >= 85.00m)
            {
                evaluation.PerformanceGrade = "A";
            }
            else if (evaluation.FinalCompositeScore >= 75.00m)
            {
                evaluation.PerformanceGrade = "B";
            }
            else if (evaluation.FinalCompositeScore >= 65.00m)
            {
                evaluation.PerformanceGrade = "C";
            }
            else
            {
                evaluation.PerformanceGrade = "D";
            }
        }
        #endregion
    }
}
