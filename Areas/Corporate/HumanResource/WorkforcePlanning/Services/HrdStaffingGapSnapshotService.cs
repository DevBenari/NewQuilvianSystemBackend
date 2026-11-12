using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Services
{
    /// <summary>
    /// Layanan pencatatan dan pembacaan potret gap kebutuhan tenaga kerja (BE-HRD-039).
    /// Mengabadikan kondisi kesenjangan pada saat pengajuan dokumen secara tidak dapat diubah (immutable).
    /// </summary>
    public sealed class HrdStaffingGapSnapshotService : IHrdStaffingGapSnapshotService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IHrdStaffingGapCalculationService _gapCalculationService;

        public HrdStaffingGapSnapshotService(
            ApplicationDbContext dbContext,
            IHrdStaffingGapCalculationService gapCalculationService)
        {
            _dbContext = dbContext;
            _gapCalculationService = gapCalculationService;
        }

        public async Task<Guid> CaptureSubmissionSnapshotAsync(
            Guid hospitalSiteId,
            Guid? organizationUnitId,
            PositionFormationSourceType sourceDocumentType,
            Guid sourceDocumentId,
            DateOnly referenceDate,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            // Hitung live gap pada saat ini
            var liveGap = await _gapCalculationService.CalculateStaffingGapsAsync(
                new StaffingGapQuery
                {
                    HospitalSiteId = hospitalSiteId,
                    OrganizationUnitId = organizationUnitId,
                    AsOfDate = referenceDate
                },
                currentUserId,
                cancellationToken);

            var now = DateTime.UtcNow;
            var snapshot = new HrdStaffingGapSnapshot
            {
                Id = Guid.NewGuid(),
                SnapshotType = StaffingGapSnapshotType.Submission,
                HospitalSiteId = hospitalSiteId,
                ReferenceDate = referenceDate,
                SourceDocumentType = sourceDocumentType,
                SourceDocumentId = sourceDocumentId,
                TakenAt = now,
                CreateDateTime = now,
                CreateBy = currentUserId
            };

            foreach (var line in liveGap.Lines)
            {
                var snapshotLine = new HrdStaffingGapSnapshotLine
                {
                    Id = Guid.NewGuid(),
                    SnapshotId = snapshot.Id,
                    OrganizationUnitId = line.OrganizationUnitId,
                    PositionId = line.PositionId,
                    FormationQuantity = line.Formation,
                    FilledFte = line.Filled,
                    ToBeVacatedFte = line.WillBeVacant,
                    TemporarilyUnavailableFte = line.TemporaryUnavailable,
                    NotYetCompetentFte = line.NotCompetent,
                    OutsourcedFte = line.Outsourced,
                    ConcurrentAssignmentCount = line.SecondaryAssignments.Count,
                    PermanentGap = line.PermanentGap,
                    TemporaryGap = line.TemporaryGap,
                    CompetencyGap = line.CompetencyGap,
                    PlacementDataComplete = true,
                    LeaveDataComplete = line.TemporaryUnavailable.HasValue,
                    CompetencyDataComplete = line.NotCompetent.HasValue,
                    WorkforceTypeDataComplete = line.Outsourced.HasValue,
                    CreateDateTime = now,
                    CreateBy = currentUserId
                };

                snapshot.Lines.Add(snapshotLine);
            }

            _dbContext.HrdStaffingGapSnapshots.Add(snapshot);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return snapshot.Id;
        }

        public async Task<PagedResult<StaffingGapSnapshotListItemResponse>> GetPagedListAsync(
            StaffingGapSnapshotPagedQuery query,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            var dbQuery = _dbContext.HrdStaffingGapSnapshots
                .AsNoTracking()
                .Include(s => s.HospitalSite)
                .Include(s => s.Lines)
                .Where(s => !s.IsDelete);

            if (query.HospitalSiteId.HasValue)
            {
                dbQuery = dbQuery.Where(s => s.HospitalSiteId == query.HospitalSiteId.Value);
            }

            if (query.SnapshotType.HasValue)
            {
                dbQuery = dbQuery.Where(s => s.SnapshotType == query.SnapshotType.Value);
            }

            if (query.SourceDocumentType.HasValue)
            {
                dbQuery = dbQuery.Where(s => s.SourceDocumentType == query.SourceDocumentType.Value);
            }

            if (query.SourceDocumentId.HasValue)
            {
                dbQuery = dbQuery.Where(s => s.SourceDocumentId == query.SourceDocumentId.Value);
            }

            if (query.PeriodMonth.HasValue)
            {
                dbQuery = dbQuery.Where(s => s.PeriodMonth == query.PeriodMonth.Value);
            }

            var totalCount = await dbQuery.CountAsync(cancellationToken);

            var pageNumber = Math.Max(1, query.PageNumber);
            var pageSize = Math.Clamp(query.PageSize, 1, 100);

            var items = await dbQuery
                .OrderByDescending(s => s.TakenAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new StaffingGapSnapshotListItemResponse
                {
                    Id = s.Id,
                    SnapshotType = s.SnapshotType,
                    HospitalSiteId = s.HospitalSiteId,
                    HospitalSiteName = s.HospitalSite != null ? s.HospitalSite.SiteName : null,
                    ReferenceDate = s.ReferenceDate,
                    SourceDocumentType = s.SourceDocumentType,
                    SourceDocumentId = s.SourceDocumentId,
                    PeriodMonth = s.PeriodMonth,
                    TakenAt = s.TakenAt,
                    ReconstructedAt = s.ReconstructedAt,
                    TotalLines = s.Lines.Count(l => !l.IsDelete)
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<StaffingGapSnapshotListItemResponse>
            {
                Items = items,
                TotalData = totalCount,
                TotalPage = pageSize > 0 ? (int)Math.Ceiling((double)totalCount / pageSize) : 0,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<StaffingGapSnapshotResponse?> GetByIdAsync(
            Guid id,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            var snapshot = await _dbContext.HrdStaffingGapSnapshots
                .AsNoTracking()
                .Include(s => s.HospitalSite)
                .Include(s => s.Lines.Where(l => !l.IsDelete))
                    .ThenInclude(l => l.OrganizationUnit)
                .Include(s => s.Lines.Where(l => !l.IsDelete))
                    .ThenInclude(l => l.Position)
                .FirstOrDefaultAsync(s => !s.IsDelete && s.Id == id, cancellationToken);

            if (snapshot == null)
            {
                return null;
            }

            return new StaffingGapSnapshotResponse
            {
                Id = snapshot.Id,
                SnapshotType = snapshot.SnapshotType,
                HospitalSiteId = snapshot.HospitalSiteId,
                HospitalSiteName = snapshot.HospitalSite?.SiteName,
                ReferenceDate = snapshot.ReferenceDate,
                SourceDocumentType = snapshot.SourceDocumentType,
                SourceDocumentId = snapshot.SourceDocumentId,
                PeriodMonth = snapshot.PeriodMonth,
                TakenAt = snapshot.TakenAt,
                ReconstructedAt = snapshot.ReconstructedAt,
                Lines = snapshot.Lines
                    .Where(l => !l.IsDelete)
                    .Select(l => new StaffingGapSnapshotLineResponse
                    {
                        Id = l.Id,
                        OrganizationUnitId = l.OrganizationUnitId,
                        OrganizationUnitCode = l.OrganizationUnit?.UnitCode,
                        OrganizationUnitName = l.OrganizationUnit?.UnitName,
                        PositionId = l.PositionId,
                        PositionCode = l.Position?.PositionCode,
                        PositionName = l.Position?.PositionName,
                        FormationQuantity = l.FormationQuantity,
                        FilledFte = l.FilledFte,
                        ToBeVacatedFte = l.ToBeVacatedFte,
                        TemporarilyUnavailableFte = l.TemporarilyUnavailableFte,
                        NotYetCompetentFte = l.NotYetCompetentFte,
                        OutsourcedFte = l.OutsourcedFte,
                        ConcurrentAssignmentCount = l.ConcurrentAssignmentCount,
                        PermanentGap = l.PermanentGap,
                        TemporaryGap = l.TemporaryGap,
                        CompetencyGap = l.CompetencyGap,
                        PlacementDataComplete = l.PlacementDataComplete,
                        LeaveDataComplete = l.LeaveDataComplete,
                        CompetencyDataComplete = l.CompetencyDataComplete,
                        WorkforceTypeDataComplete = l.WorkforceTypeDataComplete
                    })
                    .ToList()
            };
        }

        public async Task<int> StepProcessMonthlySnapshotAsync(
            DateOnly operationalDate,
            SchedulerExecutionSummary summary,
            CancellationToken cancellationToken = default)
        {
            // Periksa apakah hari ini adalah hari terakhir bulan berjalan
            int daysInMonth = DateTime.DaysInMonth(operationalDate.Year, operationalDate.Month);
            if (operationalDate.Day != daysInMonth)
            {
                return 0; // Bukan akhir bulan
            }

            var periodMonth = new DateOnly(operationalDate.Year, operationalDate.Month, 1);
            var hospitalSites = await _dbContext.MstHospitalSites
                .AsNoTracking()
                .Where(h => !h.IsDelete)
                .Select(h => h.Id)
                .ToListAsync(cancellationToken);

            int createdCount = 0;
            var now = DateTime.UtcNow;

            foreach (var siteId in hospitalSites)
            {
                // Cek idempotensi: apakah snapshot Monthly bulan ini sudah ada
                bool exists = await _dbContext.HrdStaffingGapSnapshots
                    .AnyAsync(s => !s.IsDelete &&
                                   s.HospitalSiteId == siteId &&
                                   s.SnapshotType == StaffingGapSnapshotType.Monthly &&
                                   s.PeriodMonth == periodMonth,
                              cancellationToken);

                if (exists) continue;

                var liveGap = await _gapCalculationService.CalculateStaffingGapsAsync(
                    new StaffingGapQuery
                    {
                        HospitalSiteId = siteId,
                        AsOfDate = operationalDate
                    },
                    Guid.Empty,
                    cancellationToken);

                var snapshot = new HrdStaffingGapSnapshot
                {
                    Id = Guid.NewGuid(),
                    SnapshotType = StaffingGapSnapshotType.Monthly,
                    HospitalSiteId = siteId,
                    ReferenceDate = operationalDate,
                    PeriodMonth = periodMonth,
                    TakenAt = now,
                    CreateDateTime = now,
                    CreateBy = Guid.Empty
                };

                foreach (var line in liveGap.Lines)
                {
                    snapshot.Lines.Add(new HrdStaffingGapSnapshotLine
                    {
                        Id = Guid.NewGuid(),
                        SnapshotId = snapshot.Id,
                        OrganizationUnitId = line.OrganizationUnitId,
                        PositionId = line.PositionId,
                        FormationQuantity = line.Formation,
                        FilledFte = line.Filled,
                        ToBeVacatedFte = line.WillBeVacant,
                        TemporarilyUnavailableFte = line.TemporaryUnavailable,
                        NotYetCompetentFte = line.NotCompetent,
                        OutsourcedFte = line.Outsourced,
                        ConcurrentAssignmentCount = line.SecondaryAssignments.Count,
                        PermanentGap = line.PermanentGap,
                        TemporaryGap = line.TemporaryGap,
                        CompetencyGap = line.CompetencyGap,
                        PlacementDataComplete = true,
                        LeaveDataComplete = line.TemporaryUnavailable.HasValue,
                        CompetencyDataComplete = line.NotCompetent.HasValue,
                        WorkforceTypeDataComplete = line.Outsourced.HasValue,
                        CreateDateTime = now,
                        CreateBy = Guid.Empty
                    });
                }

                _dbContext.HrdStaffingGapSnapshots.Add(snapshot);
                createdCount++;
            }

            if (createdCount > 0)
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                var logMsg = $"Scheduler potret bulanan: {createdCount} potret bulanan berhasil disimpan untuk periode {periodMonth:yyyy-MM}.";
                summary.StepLogs.Add(logMsg);
            }

            return createdCount;
        }

        public async Task<Guid> ReconstructMonthlySnapshotAsync(
            Guid hospitalSiteId,
            DateOnly periodMonth,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            var firstDay = new DateOnly(periodMonth.Year, periodMonth.Month, 1);
            int days = DateTime.DaysInMonth(firstDay.Year, firstDay.Month);
            var lastDay = new DateOnly(firstDay.Year, firstDay.Month, days);

            var liveGap = await _gapCalculationService.CalculateStaffingGapsAsync(
                new StaffingGapQuery
                {
                    HospitalSiteId = hospitalSiteId,
                    AsOfDate = lastDay
                },
                currentUserId,
                cancellationToken);

            var now = DateTime.UtcNow;
            var snapshot = new HrdStaffingGapSnapshot
            {
                Id = Guid.NewGuid(),
                SnapshotType = StaffingGapSnapshotType.Monthly,
                HospitalSiteId = hospitalSiteId,
                ReferenceDate = lastDay,
                PeriodMonth = firstDay,
                TakenAt = now,
                ReconstructedAt = now, // Penanda rekonstruksi (kriteria BE-HRD-048)
                CreateDateTime = now,
                CreateBy = currentUserId
            };

            foreach (var line in liveGap.Lines)
            {
                snapshot.Lines.Add(new HrdStaffingGapSnapshotLine
                {
                    Id = Guid.NewGuid(),
                    SnapshotId = snapshot.Id,
                    OrganizationUnitId = line.OrganizationUnitId,
                    PositionId = line.PositionId,
                    FormationQuantity = line.Formation,
                    FilledFte = line.Filled,
                    ToBeVacatedFte = line.WillBeVacant,
                    TemporarilyUnavailableFte = line.TemporaryUnavailable,
                    NotYetCompetentFte = line.NotCompetent,
                    OutsourcedFte = line.Outsourced,
                    ConcurrentAssignmentCount = line.SecondaryAssignments.Count,
                    PermanentGap = line.PermanentGap,
                    TemporaryGap = line.TemporaryGap,
                    CompetencyGap = line.CompetencyGap,
                    PlacementDataComplete = true,
                    LeaveDataComplete = line.TemporaryUnavailable.HasValue,
                    CompetencyDataComplete = line.NotCompetent.HasValue,
                    WorkforceTypeDataComplete = line.Outsourced.HasValue,
                    CreateDateTime = now,
                    CreateBy = currentUserId
                });
            }

            _dbContext.HrdStaffingGapSnapshots.Add(snapshot);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return snapshot.Id;
        }
    }
}
