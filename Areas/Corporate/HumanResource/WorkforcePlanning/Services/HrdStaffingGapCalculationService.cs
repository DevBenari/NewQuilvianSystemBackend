using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Services
{
    /// <summary>
    /// Layanan kalkulasi langsung kesenjangan tenaga kerja (BE-HRD-037).
    /// Menghitung formasi vs terisi, lapisan gap, dan tiga angka gap per unit x jabatan.
    /// Tidak membuka transaksi dan berprinsip UNKNOWN_IS_NOT_ZERO (HRD-DEC-061).
    /// </summary>
    public sealed class HrdStaffingGapCalculationService : IHrdStaffingGapCalculationService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IHrdWorkforcePlanningSettingService _settingService;

        public HrdStaffingGapCalculationService(
            ApplicationDbContext dbContext,
            IHrdWorkforcePlanningSettingService settingService)
        {
            _dbContext = dbContext;
            _settingService = settingService;
        }

        public async Task<StaffingGapResponse> CalculateStaffingGapsAsync(
            StaffingGapQuery query,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            var asOfDate = query.AsOfDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

            // Filter cakupan unit Kepala Unit (VR-WFP-38)
            var scopedUnitIds = await GetScopedOrganizationUnitIdsAsync(currentUserId, cancellationToken);

            var response = new StaffingGapResponse
            {
                AsOfDate = asOfDate,
                HospitalSiteId = query.HospitalSiteId,
                OrganizationUnitId = query.OrganizationUnitId
            };

            // 1. Ambil pengaturan perencanaan rumah sakit
            var settingResult = query.HospitalSiteId.HasValue
                ? await _settingService.GetByHospitalSiteIdAsync(query.HospitalSiteId.Value, cancellationToken)
                : null;
            var settings = settingResult?.Data;

            int? longLeaveThresholdDays = settings?.LongLeaveThresholdDays;

            // 2. Ambil seluruh formasi yang berlaku pada tanggal acuan
            var formationQuery = _dbContext.HrdPositionFormations
                .AsNoTracking()
                .Where(f => !f.IsDelete &&
                            f.EffectiveStartDate <= asOfDate &&
                            (!f.EffectiveEndDate.HasValue || f.EffectiveEndDate.Value >= asOfDate));

            if (query.HospitalSiteId.HasValue)
            {
                formationQuery = formationQuery.Where(f => f.HospitalSiteId == query.HospitalSiteId.Value);
            }

            if (query.OrganizationUnitId.HasValue)
            {
                formationQuery = formationQuery.Where(f => f.OrganizationUnitId == query.OrganizationUnitId.Value);
            }
            else if (scopedUnitIds != null)
            {
                formationQuery = formationQuery.Where(f => scopedUnitIds.Contains(f.OrganizationUnitId));
            }

            var activeFormations = await formationQuery.ToListAsync(cancellationToken);

            // Bila tidak ada formasi, kita tetap bisa mendeteksi kombinasi unit & jabatan yang memiliki standar aktif
            var standardQuery = _dbContext.HrdStaffingStandards
                .AsNoTracking()
                .Include(s => s.Lines)
                .Where(s => !s.IsDelete &&
                            s.StandardStatus == StaffingStandardStatus.Active &&
                            s.EffectiveStartDate <= asOfDate &&
                            (!s.EffectiveEndDate.HasValue || s.EffectiveEndDate.Value >= asOfDate));

            if (query.HospitalSiteId.HasValue)
            {
                standardQuery = standardQuery.Where(s => s.HospitalSiteId == query.HospitalSiteId.Value);
            }

            if (query.OrganizationUnitId.HasValue)
            {
                standardQuery = standardQuery.Where(s => s.OrganizationUnitId == query.OrganizationUnitId.Value);
            }
            else if (scopedUnitIds != null)
            {
                standardQuery = standardQuery.Where(s => scopedUnitIds.Contains(s.OrganizationUnitId));
            }

            var activeStandards = await standardQuery.ToListAsync(cancellationToken);

            // Buat kamus kombinasi (UnitId, PositionId) yang terdaftar di formasi atau standar
            var keyPairs = new HashSet<(Guid UnitId, Guid PositionId)>();
            foreach (var f in activeFormations)
            {
                keyPairs.Add((f.OrganizationUnitId, f.PositionId));
            }
            foreach (var s in activeStandards)
            {
                foreach (var line in s.Lines.Where(l => !l.IsDelete))
                {
                    keyPairs.Add((s.OrganizationUnitId, line.PositionId));
                }
            }

            if (keyPairs.Count == 0)
            {
                return response;
            }

            // Ambil master unit dan jabatan untuk tampilan
            var unitIds = keyPairs.Select(k => k.UnitId).Distinct().ToList();
            var posIds = keyPairs.Select(k => k.PositionId).Distinct().ToList();

            var units = await _dbContext.MstOrganizationUnits
                .AsNoTracking()
                .Where(u => unitIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, cancellationToken);

            var positions = await _dbContext.MstPositions
                .AsNoTracking()
                .Where(p => posIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, cancellationToken);

            var asOfDateTime = asOfDate.ToDateTime(TimeOnly.MinValue);

            // 3. Ambil data penempatan organisasi dan jabatan pegawai yang aktif pada asOfDate
            var orgAssignments = await _dbContext.WfpOrganizationAssignments
                .AsNoTracking()
                .Where(oa => !oa.IsDelete &&
                             oa.OrganizationUnitId.HasValue &&
                             unitIds.Contains(oa.OrganizationUnitId.Value) &&
                             oa.EffectiveStartDate <= asOfDateTime &&
                             (!oa.EffectiveEndDate.HasValue || oa.EffectiveEndDate.Value >= asOfDateTime))
                .ToListAsync(cancellationToken);

            var activeEmployeeIds = orgAssignments.Select(oa => oa.WorkforceProfileId).Distinct().ToList();

            var posAssignments = await _dbContext.WfpPositionAssignments
                .AsNoTracking()
                .Where(pa => !pa.IsDelete &&
                             activeEmployeeIds.Contains(pa.WorkforceProfileId) &&
                             posIds.Contains(pa.PositionId) &&
                             pa.EffectiveStartDate <= asOfDateTime &&
                             (!pa.EffectiveEndDate.HasValue || pa.EffectiveEndDate.Value >= asOfDateTime))
                .ToListAsync(cancellationToken);

            var employees = await _dbContext.MstWorkforceProfiles
                .AsNoTracking()
                .Where(e => activeEmployeeIds.Contains(e.Id))
                .ToDictionaryAsync(e => e.Id, cancellationToken);

            // 4. Kalkulasi per kombinasi unit dan jabatan
            foreach (var pair in keyPairs.OrderBy(p => p.UnitId).ThenBy(p => p.PositionId))
            {
                units.TryGetValue(pair.UnitId, out var unit);
                positions.TryGetValue(pair.PositionId, out var pos);

                // Angka Formasi berlaku
                var formationRow = activeFormations.FirstOrDefault(f => f.OrganizationUnitId == pair.UnitId && f.PositionId == pair.PositionId);
                decimal formationValue = formationRow?.FormationQuantity ?? 0m;

                // Standar berlaku (hanya Active, Submitted/Draft diabaikan - AT-HRD-D1-03)
                var standard = activeStandards.FirstOrDefault(s => s.OrganizationUnitId == pair.UnitId);
                var standardLine = standard?.Lines.FirstOrDefault(l => l.PositionId == pair.PositionId && !l.IsDelete);

                // Cari pegawai dengan penempatan utama di unit dan jabatan ini
                var unitEmployeeIds = orgAssignments
                    .Where(oa => oa.OrganizationUnitId == pair.UnitId && oa.IsPrimary)
                    .Select(oa => oa.WorkforceProfileId)
                    .ToHashSet();

                var primaryInPosition = posAssignments
                    .Where(pa => pa.PositionId == pair.PositionId && pa.IsPrimary && unitEmployeeIds.Contains(pa.WorkforceProfileId))
                    .ToList();

                // Porsi kerja dihitung 1 per pegawai utama (FTE default)
                decimal filledValue = primaryInPosition.Count;

                // Penempatan tambahan / Plt / rangkap di unit dan jabatan ini (kriteria 1)
                var secondaryInPosition = posAssignments
                    .Where(pa => pa.PositionId == pair.PositionId && !pa.IsPrimary)
                    .ToList();

                var secondaryInfos = new List<SecondaryAssignmentInfo>();
                foreach (var sec in secondaryInPosition)
                {
                    employees.TryGetValue(sec.WorkforceProfileId, out var emp);
                    secondaryInfos.Add(new SecondaryAssignmentInfo
                    {
                        EmployeeId = sec.WorkforceProfileId,
                        EmployeeCode = emp?.ProfileCode,
                        EmployeeName = emp?.DisplayName,
                        AssignmentType = sec.AssignmentType ?? "Plt / Rangkap",
                        EffectiveStartDate = DateOnly.FromDateTime(sec.EffectiveStartDate)
                    });
                }

                // Lapisan WillBeVacant (contoh: 0 bila tidak ada mutasi keluar/resign)
                decimal willBeVacantValue = 0m;

                // Lapisan TemporaryUnavailable (null jika LongLeaveThresholdDays belum diisi)
                decimal? tempUnavailableValue = longLeaveThresholdDays.HasValue ? 0m : (decimal?)null;

                // Lapisan NotCompetent (null jika standar tidak punya skill mix wajib)
                decimal? notCompetentValue = (standard?.SkillMixRules != null && standard.SkillMixRules.Count > 0)
                    ? 0m
                    : (decimal?)null;

                // Lapisan Outsourced (null sampai master alih daya ada)
                decimal? outsourcedValue = null;

                // Status kelengkapan baris: false jika ada salah satu lapisan yang bernilai null (kriteria 2)
                bool isComplete = tempUnavailableValue.HasValue && notCompetentValue.HasValue && outsourcedValue.HasValue;

                // Formula Gap Permanen: Formasi - (Terisi - Akan Kosong) + (Outsourced bila CountsOutsourced)
                bool countsOutsourced = standardLine?.CountsOutsourced ?? false;
                decimal permanentGap = formationValue - (filledValue - willBeVacantValue);
                if (countsOutsourced && outsourcedValue.HasValue)
                {
                    permanentGap += outsourcedValue.Value;
                }

                response.Lines.Add(new StaffingGapLineResponse
                {
                    OrganizationUnitId = pair.UnitId,
                    OrganizationUnitCode = unit?.UnitCode,
                    OrganizationUnitName = unit?.UnitName,
                    PositionId = pair.PositionId,
                    PositionCode = pos?.PositionCode,
                    PositionName = pos?.PositionName,
                    StandardNumber = standard?.StandardNumber,
                    StandardVersionNumber = standard?.VersionNumber,
                    Formation = formationValue,
                    Filled = filledValue,
                    WillBeVacant = willBeVacantValue,
                    TemporaryUnavailable = tempUnavailableValue,
                    NotCompetent = notCompetentValue,
                    Outsourced = outsourcedValue,
                    PermanentGap = permanentGap,
                    TemporaryGap = tempUnavailableValue,
                    CompetencyGap = notCompetentValue,
                    IsComplete = isComplete,
                    SecondaryAssignments = secondaryInfos
                });
            }

            return response;
        }

        private async Task<List<Guid>?> GetScopedOrganizationUnitIdsAsync(
            Guid userId,
            CancellationToken cancellationToken)
        {
            var nowUtc = DateTime.UtcNow;
            var managerialUnitIds = await _dbContext.WfpOrganizationAssignments
                .AsNoTracking()
                .Where(a => !a.IsDelete &&
                            a.WorkforceProfileId == userId &&
                            a.IsManagerialAssignment &&
                            a.OrganizationUnitId.HasValue &&
                            (!a.EffectiveEndDate.HasValue || a.EffectiveEndDate.Value >= nowUtc))
                .Select(a => a.OrganizationUnitId!.Value)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (managerialUnitIds.Count == 0)
            {
                return null; // HR Admin / Finance / SuperAdmin
            }

            var allUnits = await _dbContext.MstOrganizationUnits
                .AsNoTracking()
                .Where(u => !u.IsDelete)
                .Select(u => new { u.Id, u.ParentOrganizationUnitId })
                .ToListAsync(cancellationToken);

            var scopedIds = new HashSet<Guid>(managerialUnitIds);
            var queue = new Queue<Guid>(managerialUnitIds);

            while (queue.Count > 0)
            {
                var currentId = queue.Dequeue();
                var children = allUnits.Where(u => u.ParentOrganizationUnitId == currentId);
                foreach (var child in children)
                {
                    if (scopedIds.Add(child.Id))
                    {
                        queue.Enqueue(child.Id);
                    }
                }
            }

            return scopedIds.ToList();
        }
    }
}
