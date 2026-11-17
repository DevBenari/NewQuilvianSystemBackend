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
    /// Layanan Siklus Kinerja, Guard Periode & Validasi Anti-Tumpang-Tindih (BE-HRD-257, PRF-B1, VR-PRF-01, VR-PRF-02)
    /// </summary>
    public class PerformanceCycleService : IPerformanceCycleService
    {
        private readonly IPerformanceUnitOfWork _uow;

        public PerformanceCycleService(IPerformanceUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task<List<PerformanceCycleDto>> GetCyclesAsync(CancellationToken cancellationToken = default)
        {
            var cycles = await _uow.PerformanceCycles
                .OrderByDescending(x => x.StartDate)
                .ToListAsync(cancellationToken);

            return cycles.Select(c => new PerformanceCycleDto
            {
                Id = c.Id,
                CycleCode = c.CycleCode,
                CycleName = c.CycleName,
                CycleType = c.CycleType,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                GoalSettingDeadline = c.GoalSettingDeadline ?? c.StartDate,
                SelfAssessmentDeadline = c.SelfAssessmentDeadline ?? c.StartDate,
                ManagerAssessmentDeadline = c.ManagerAssessmentDeadline ?? c.StartDate,
                CalibrationDeadline = c.CalibrationDeadline,
                Status = c.Status,
                Description = c.Description,
                IsActive = c.IsActive
            }).ToList();
        }

        public async Task<PerformanceCycleDto> GetCycleByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var c = await _uow.PerformanceCycles.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (c == null)
                throw new KeyNotFoundException($"Siklus evaluasi kinerja dengan ID {id} tidak ditemukan.");

            return new PerformanceCycleDto
            {
                Id = c.Id,
                CycleCode = c.CycleCode,
                CycleName = c.CycleName,
                CycleType = c.CycleType,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                GoalSettingDeadline = c.GoalSettingDeadline ?? c.StartDate,
                SelfAssessmentDeadline = c.SelfAssessmentDeadline ?? c.StartDate,
                ManagerAssessmentDeadline = c.ManagerAssessmentDeadline ?? c.StartDate,
                CalibrationDeadline = c.CalibrationDeadline,
                Status = c.Status,
                Description = c.Description,
                IsActive = c.IsActive
            };
        }

        public async Task<PerformanceCycleDto> CreateCycleAsync(CreatePerformanceCycleRequestDto dto, Guid createdBy, CancellationToken cancellationToken = default)
        {
            if (dto.StartDate >= dto.EndDate)
                throw new ArgumentException("Tanggal mulai siklus harus lebih awal dari tanggal selesai.");

            // PRF-B1 & VR-PRF-01: Larangan Overlapping Periode Kinerja Aktif
            var hasOverlap = await _uow.PerformanceCycles.AnyAsync(x =>
                x.CycleType == dto.CycleType &&
                x.Status != "CLOSED" &&
                ((dto.StartDate >= x.StartDate && dto.StartDate <= x.EndDate) ||
                 (dto.EndDate >= x.StartDate && dto.EndDate <= x.EndDate) ||
                 (dto.StartDate <= x.StartDate && dto.EndDate >= x.EndDate)),
                cancellationToken);

            if (hasOverlap)
            {
                throw new InvalidOperationException("Konflik jadwal: Masih terdapat siklus kinerja aktif pada rentang tanggal tersebut yang belum ditutup resmi (VR-PRF-01, PRF-B1).");
            }

            var cycle = new HrdPerformanceCycle
            {
                Id = Guid.NewGuid(),
                CycleCode = dto.CycleCode,
                CycleName = dto.CycleName,
                CycleType = dto.CycleType,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                GoalSettingDeadline = dto.GoalSettingDeadline,
                SelfAssessmentDeadline = dto.SelfAssessmentDeadline,
                ManagerAssessmentDeadline = dto.ManagerAssessmentDeadline,
                CalibrationDeadline = dto.CalibrationDeadline,
                Status = "DRAFT",
                Description = dto.Description,
                IsActive = true,
                CreateBy = createdBy,
                CreateDateTime = DateTime.UtcNow,
                UpdateBy = createdBy,
                UpdateDateTime = DateTime.UtcNow
            };

            await _uow.AddAsync(cycle, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            return await GetCycleByIdAsync(cycle.Id, cancellationToken);
        }

        public async Task<bool> TransitionCycleStatusAsync(Guid cycleId, string newStatus, Guid updatedBy, CancellationToken cancellationToken = default)
        {
            var cycle = await _uow.PerformanceCycles.FirstOrDefaultAsync(x => x.Id == cycleId, cancellationToken);
            if (cycle == null)
                throw new KeyNotFoundException($"Siklus kinerja ID {cycleId} tidak ditemukan.");

            // VR-PRF-02: Validasi State Transition
            var validTransitions = new Dictionary<string, List<string>>
            {
                { "DRAFT", new() { "GOAL_SETTING", "CANCELLED" } },
                { "GOAL_SETTING", new() { "ACTIVE_ASSESSMENT", "CANCELLED" } },
                { "ACTIVE_ASSESSMENT", new() { "CALIBRATION" } },
                { "CALIBRATION", new() { "CLOSED" } },
                { "CLOSED", new() }
            };

            if (validTransitions.ContainsKey(cycle.Status) && !validTransitions[cycle.Status].Contains(newStatus))
            {
                throw new InvalidOperationException($"Transisi siklus dari status '{cycle.Status}' ke '{newStatus}' tidak diizinkan oleh regulasi alur kerja (VR-PRF-02).");
            }

            cycle.Status = newStatus;
            cycle.UpdateBy = updatedBy;
            cycle.UpdateDateTime = DateTime.UtcNow;
            if (newStatus == "CLOSED")
            {
                cycle.IsActive = false;
            }

            _uow.Update(cycle);
            await _uow.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<List<KpiMetricCatalogDto>> GetKpiCatalogsAsync(string? category = null, string? targetProfession = null, CancellationToken cancellationToken = default)
        {
            var query = _uow.KpiMetricCatalogs.Where(x => x.IsActive);
            if (!string.IsNullOrEmpty(category))
                query = query.Where(x => x.Category == category);
            if (!string.IsNullOrEmpty(targetProfession) && targetProfession != "ALL")
                query = query.Where(x => x.TargetProfessionGroup == targetProfession || x.TargetProfessionGroup == "ALL");

            var metrics = await query.OrderBy(x => x.MetricCode).ToListAsync(cancellationToken);

            return metrics.Select(m => new KpiMetricCatalogDto
            {
                Id = m.Id,
                MetricCode = m.MetricCode,
                MetricName = m.MetricName,
                ComponentType = m.ComponentType,
                Category = m.Category,
                TargetProfessionGroup = m.TargetProfessionGroup,
                UnitOfMeasure = m.UnitOfMeasure,
                TargetPolarity = m.TargetPolarity,
                DefaultTargetValue = m.DefaultTargetValue ?? 0m,
                ScoringRubricJson = m.ScoringRubricJson,
                IsActive = m.IsActive
            }).ToList();
        }

        public async Task<KpiMetricCatalogDto> CreateKpiCatalogAsync(CreateKpiMetricCatalogRequestDto dto, Guid createdBy, CancellationToken cancellationToken = default)
        {
            var exists = await _uow.KpiMetricCatalogs.AnyAsync(x => x.MetricCode == dto.MetricCode, cancellationToken);
            if (exists)
                throw new InvalidOperationException($"Metrik dengan kode {dto.MetricCode} sudah terdaftar dalam katalog master.");

            var metric = new MstKpiMetricCatalog
            {
                Id = Guid.NewGuid(),
                MetricCode = dto.MetricCode,
                MetricName = dto.MetricName,
                ComponentType = dto.ComponentType,
                Category = dto.Category,
                TargetProfessionGroup = dto.TargetProfessionGroup,
                UnitOfMeasure = dto.UnitOfMeasure,
                TargetPolarity = dto.TargetPolarity,
                DefaultTargetValue = dto.DefaultTargetValue,
                ScoringRubricJson = dto.ScoringRubricJson,
                IsActive = dto.IsActive,
                CreateBy = createdBy,
                CreateDateTime = DateTime.UtcNow,
                UpdateBy = createdBy,
                UpdateDateTime = DateTime.UtcNow
            };

            await _uow.AddAsync(metric, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            return new KpiMetricCatalogDto
            {
                Id = metric.Id,
                MetricCode = metric.MetricCode,
                MetricName = metric.MetricName,
                ComponentType = metric.ComponentType,
                Category = metric.Category,
                TargetProfessionGroup = metric.TargetProfessionGroup,
                UnitOfMeasure = metric.UnitOfMeasure,
                TargetPolarity = metric.TargetPolarity,
                DefaultTargetValue = metric.DefaultTargetValue ?? 0m,
                ScoringRubricJson = metric.ScoringRubricJson,
                IsActive = metric.IsActive
            };
        }
    }
}
