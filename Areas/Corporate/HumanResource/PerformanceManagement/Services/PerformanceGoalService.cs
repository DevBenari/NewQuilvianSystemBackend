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
    /// Layanan Domain Cascading Sasaran Mutu Unit ke Individu & Validasi Bobot 100% (BE-HRD-258, PRF-B2, VR-PRF-02, VR-PRF-04)
    /// </summary>
    public class PerformanceGoalService : IPerformanceGoalService
    {
        private readonly IPerformanceUnitOfWork _uow;

        public PerformanceGoalService(IPerformanceUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task<List<PerformanceGoalDto>> GetGoalsByStaffAsync(Guid staffId, Guid cycleId, CancellationToken cancellationToken = default)
        {
            var goals = await _uow.PerformanceGoals
                .Where(x => x.WorkforceProfileId == staffId && x.PerformanceCycleId == cycleId)
                .ToListAsync(cancellationToken);

            var catalogIds = goals.Select(g => g.MetricCatalogId).Distinct().ToList();
            var catalogs = await _uow.KpiMetricCatalogs
                .Where(c => catalogIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, cancellationToken);

            return goals.Select(g => new PerformanceGoalDto
            {
                Id = g.Id,
                PerformanceCycleId = g.PerformanceCycleId,
                WorkforceProfileId = g.WorkforceProfileId,
                MetricCatalogId = g.MetricCatalogId,
                MetricCode = catalogs.TryGetValue(g.MetricCatalogId, out var cat) ? cat.MetricCode : string.Empty,
                MetricName = catalogs.TryGetValue(g.MetricCatalogId, out var c2) ? c2.MetricName : string.Empty,
                WeightPercentage = g.WeightPercentage,
                TargetValue = g.TargetValue,
                TargetPolarity = g.TargetPolarity,
                ApprovedByManagerId = g.ApprovedByManagerId,
                IsLocked = g.IsLocked
            }).ToList();
        }

        public async Task<List<PerformanceGoalDto>> CascadeGoalsAsync(CascadePerformanceGoalsRequestDto dto, Guid managerId, CancellationToken cancellationToken = default)
        {
            if (dto.Goals == null || dto.Goals.Count < 3 || dto.Goals.Count > 10)
            {
                throw new ArgumentException("Jumlah butir sasaran kinerja individu harus antara 3 sampai 10 butir (VR-PRF-02).");
            }

            // PRF-B2 & VR-PRF-04: Locking Rule - Akumulasi Bobot Tepat 100.00%
            var totalWeight = dto.Goals.Sum(x => x.WeightPercentage);
            if (Math.Abs(totalWeight - 100.00m) > 0.01m)
            {
                throw new InvalidOperationException($"Akumulasi total bobot target sasaran kerja harus tepat 100.00%. Total saat ini: {totalWeight:F2}% (VR-PRF-04, PRF-B2).");
            }

            // Validasi apakah target sudah terkunci
            var existingGoals = await _uow.PerformanceGoals
                .Where(x => x.WorkforceProfileId == dto.WorkforceProfileId && x.PerformanceCycleId == dto.PerformanceCycleId)
                .ToListAsync(cancellationToken);

            if (existingGoals.Any(x => x.IsLocked))
            {
                throw new InvalidOperationException("Sasaran kerja individu untuk periode ini sudah dikunci dan disetujui atasan, tidak dapat diubah sepihak (PRF-B2).");
            }

            // Soft-delete target lama jika ada (belum dikunci)
            foreach (var g in existingGoals)
            {
                _uow.SoftDelete(g, managerId);
            }

            // Tambahkan sasaran kerja baru
            var newGoals = new List<HrdPerformanceGoal>();
            foreach (var item in dto.Goals)
            {
                var goal = new HrdPerformanceGoal
                {
                    Id = Guid.NewGuid(),
                    PerformanceCycleId = dto.PerformanceCycleId,
                    WorkforceProfileId = dto.WorkforceProfileId,
                    MetricCatalogId = item.MetricCatalogId,
                    WeightPercentage = item.WeightPercentage,
                    TargetValue = item.TargetValue,
                    TargetPolarity = item.TargetPolarity,
                    ApprovedByManagerId = managerId,
                    IsLocked = false,
                    CreateBy = managerId,
                    CreateDateTime = DateTime.UtcNow,
                    UpdateBy = managerId,
                    UpdateDateTime = DateTime.UtcNow
                };
                await _uow.AddAsync(goal, cancellationToken);
                newGoals.Add(goal);
            }

            await _uow.SaveChangesAsync(cancellationToken);
            return await GetGoalsByStaffAsync(dto.WorkforceProfileId, dto.PerformanceCycleId, cancellationToken);
        }

        public async Task<bool> ApproveGoalsAsync(Guid cycleId, Guid staffId, Guid managerId, CancellationToken cancellationToken = default)
        {
            var goals = await _uow.PerformanceGoals
                .Where(x => x.WorkforceProfileId == staffId && x.PerformanceCycleId == cycleId)
                .ToListAsync(cancellationToken);

            if (!goals.Any())
                throw new KeyNotFoundException("Belum ada target sasaran kerja yang diajukan untuk staf ini.");

            var totalWeight = goals.Sum(x => x.WeightPercentage);
            if (Math.Abs(totalWeight - 100.00m) > 0.01m)
            {
                throw new InvalidOperationException($"Persetujuan target ditolak: total bobot saat ini {totalWeight:F2}% belum memenuhi tepat 100.00% (VR-PRF-04).");
            }

            foreach (var g in goals)
            {
                g.ApprovedByManagerId = managerId;
                g.IsLocked = true;
                g.UpdateBy = managerId;
                g.UpdateDateTime = DateTime.UtcNow;
                _uow.Update(g);
            }

            await _uow.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
