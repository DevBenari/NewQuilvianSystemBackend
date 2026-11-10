using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Services
{
    /// <summary>
    /// Layanan bisnis standar kebutuhan tenaga kerja rumah sakit (BE-HRD-035).
    /// Memenuhi HRD-DEC-058, HRD-DEC-060, HRD-DEC-064 (T9), HRD-DEC-072, FR-HRD-092 s.d. FR-HRD-095, FR-HRD-097,
    /// VR-WFP-01 s.d. VR-WFP-07, VR-WFP-38, AT-HRD-D1-02, AT-HRD-D1-19, dan UAT-29 s.d. UAT-30.
    /// </summary>
    public sealed class HrdStaffingStandardService : IHrdStaffingStandardService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly NumberSeriesAllocator _numberAllocator;
        private readonly IHrdWorkforcePlanningWorkflowIntegrationService _workflowIntegrationService;
        private readonly IHrdStaffingRecommendationService _recommendationService;
        private readonly ILogger<HrdStaffingStandardService> _logger;

        public HrdStaffingStandardService(
            ApplicationDbContext dbContext,
            NumberSeriesAllocator numberAllocator,
            IHrdWorkforcePlanningWorkflowIntegrationService workflowIntegrationService,
            IHrdStaffingRecommendationService recommendationService,
            ILogger<HrdStaffingStandardService> logger)
        {
            _dbContext = dbContext;
            _numberAllocator = numberAllocator;
            _workflowIntegrationService = workflowIntegrationService;
            _recommendationService = recommendationService;
            _logger = logger;
        }

        public async Task<WorkforcePlanningResult<PagedResult<StaffingStandardListItemResponse>>> GetPagedListAsync(
            StaffingStandardPagedQuery query,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize < 1 ? 25 : Math.Min(query.PageSize, 100);

            // Batas cakupan Kepala Unit (VR-WFP-38)
            var scopedUnitIds = await GetScopedOrganizationUnitIdsAsync(currentUserId, cancellationToken);
            if (scopedUnitIds != null)
            {
                if (query.OrganizationUnitId.HasValue && !scopedUnitIds.Contains(query.OrganizationUnitId.Value))
                {
                    return WorkforcePlanningResult<PagedResult<StaffingStandardListItemResponse>>.Ok(new PagedResult<StaffingStandardListItemResponse>
                    {
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        TotalData = 0,
                        TotalPage = 0,
                        Items = new List<StaffingStandardListItemResponse>()
                    }, "Daftar standar kebutuhan tenaga berhasil diambil.");
                }
            }

            var baseQuery = _dbContext.HrdStaffingStandards.AsNoTracking()
                .Include(s => s.HospitalSite)
                .Include(s => s.OrganizationUnit)
                .Include(s => s.Lines)
                .Include(s => s.SkillMixRules)
                .Where(s => !s.IsDelete);

            if (scopedUnitIds != null)
            {
                baseQuery = baseQuery.Where(s => scopedUnitIds.Contains(s.OrganizationUnitId));
            }

            if (query.HospitalSiteId.HasValue && query.HospitalSiteId.Value != Guid.Empty)
            {
                baseQuery = baseQuery.Where(s => s.HospitalSiteId == query.HospitalSiteId.Value);
            }

            if (query.OrganizationUnitId.HasValue && query.OrganizationUnitId.Value != Guid.Empty)
            {
                baseQuery = baseQuery.Where(s => s.OrganizationUnitId == query.OrganizationUnitId.Value);
            }

            if (query.Status.HasValue)
            {
                baseQuery = baseQuery.Where(s => s.StandardStatus == query.Status.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim().ToLower();
                baseQuery = baseQuery.Where(s =>
                    s.StandardNumber.ToLower().Contains(search) ||
                    (s.OrganizationUnit != null && s.OrganizationUnit.UnitName.ToLower().Contains(search)) ||
                    (s.Justification != null && s.Justification.ToLower().Contains(search)));
            }

            var totalData = await baseQuery.CountAsync(cancellationToken);

            var standards = await baseQuery
                .OrderByDescending(s => s.EffectiveStartDate)
                .ThenByDescending(s => s.CreateDateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var userIds = new List<Guid>();
            foreach (var s in standards)
            {
                if (s.SubmittedByUserId.HasValue) userIds.Add(s.SubmittedByUserId.Value);
                if (s.ApprovedByUserId.HasValue) userIds.Add(s.ApprovedByUserId.Value);
            }

            var users = userIds.Count > 0
                ? await _dbContext.Users.AsNoTracking()
                    .Where(u => userIds.Contains(u.Id))
                    .ToDictionaryAsync(u => u.Id, u => !string.IsNullOrWhiteSpace(u.DisplayName) ? u.DisplayName : u.UserName ?? string.Empty, cancellationToken)
                : new Dictionary<Guid, string>();

            var items = standards.Select(s => new StaffingStandardListItemResponse
            {
                Id = s.Id,
                StandardNumber = s.StandardNumber,
                HospitalSiteId = s.HospitalSiteId,
                HospitalSiteName = s.HospitalSite?.SiteName,
                OrganizationUnitId = s.OrganizationUnitId,
                OrganizationUnitCode = s.OrganizationUnit?.UnitCode,
                OrganizationUnitName = s.OrganizationUnit?.UnitName,
                VersionNumber = s.VersionNumber,
                EffectiveStartDate = s.EffectiveStartDate,
                EffectiveEndDate = s.EffectiveEndDate,
                StandardStatus = s.StandardStatus,
                TotalLines = s.Lines.Count(l => !l.IsDelete),
                TotalSkillMixRules = s.SkillMixRules.Count(r => !r.IsDelete),
                SubmittedAt = s.SubmittedAt,
                SubmittedByUserName = s.SubmittedByUserId.HasValue && users.TryGetValue(s.SubmittedByUserId.Value, out var sUser) ? sUser : null,
                ApprovedAt = s.ApprovedAt,
                ApprovedByUserName = s.ApprovedByUserId.HasValue && users.TryGetValue(s.ApprovedByUserId.Value, out var aUser) ? aUser : null,
                ActivatedAt = s.ActivatedAt,
                CreatedAt = s.CreateDateTime,
                RowVersion = s.RowVersion
            }).ToList();

            var result = new PagedResult<StaffingStandardListItemResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return WorkforcePlanningResult<PagedResult<StaffingStandardListItemResponse>>.Ok(result, "Daftar standar kebutuhan tenaga berhasil diambil.");
        }

        public async Task<WorkforcePlanningResult<StaffingStandardResponse>> GetByIdAsync(
            Guid id,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            var standard = await _dbContext.HrdStaffingStandards.AsNoTracking()
                .Include(s => s.HospitalSite)
                .Include(s => s.OrganizationUnit)
                .Include(s => s.Lines)
                    .ThenInclude(l => l.Position)
                .Include(s => s.Lines)
                    .ThenInclude(l => l.Profession)
                .Include(s => s.SkillMixRules)
                    .ThenInclude(r => r.Shift)
                .Include(s => s.SkillMixRules)
                    .ThenInclude(r => r.Profession)
                .Include(s => s.SkillMixRules)
                    .ThenInclude(r => r.Competency)
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDelete, cancellationToken);

            if (standard == null)
            {
                return WorkforcePlanningResult<StaffingStandardResponse>.Fail(
                    StatusCodes.Status404NotFound, "Data tidak ditemukan.");
            }

            // Batas cakupan Kepala Unit (VR-WFP-38)
            var scopedUnitIds = await GetScopedOrganizationUnitIdsAsync(currentUserId, cancellationToken);
            if (scopedUnitIds != null && !scopedUnitIds.Contains(standard.OrganizationUnitId))
            {
                return WorkforcePlanningResult<StaffingStandardResponse>.Fail(
                    StatusCodes.Status404NotFound, "Data tidak ditemukan.");
            }

            var response = await MapToResponseAsync(standard, cancellationToken);
            return WorkforcePlanningResult<StaffingStandardResponse>.Ok(response, "Detail standar kebutuhan tenaga berhasil diambil.");
        }

        public async Task<WorkforcePlanningResult<StaffingStandardResponse>> GetEffectiveAsync(
            Guid organizationUnitId,
            DateOnly date,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            // Batas cakupan Kepala Unit (VR-WFP-38)
            var scopedUnitIds = await GetScopedOrganizationUnitIdsAsync(currentUserId, cancellationToken);
            if (scopedUnitIds != null && !scopedUnitIds.Contains(organizationUnitId))
            {
                return WorkforcePlanningResult<StaffingStandardResponse>.Fail(
                    StatusCodes.Status404NotFound, "Data tidak ditemukan.");
            }

            // Kriteria 6: Versi Draft/Submitted tidak pernah dikembalikan. Hanya versi Active yang berlaku pada tanggal acuan.
            var standard = await _dbContext.HrdStaffingStandards.AsNoTracking()
                .Include(s => s.HospitalSite)
                .Include(s => s.OrganizationUnit)
                .Include(s => s.Lines)
                    .ThenInclude(l => l.Position)
                .Include(s => s.Lines)
                    .ThenInclude(l => l.Profession)
                .Include(s => s.SkillMixRules)
                    .ThenInclude(r => r.Shift)
                .Include(s => s.SkillMixRules)
                    .ThenInclude(r => r.Profession)
                .Include(s => s.SkillMixRules)
                    .ThenInclude(r => r.Competency)
                .FirstOrDefaultAsync(s => s.OrganizationUnitId == organizationUnitId &&
                                          s.StandardStatus == StaffingStandardStatus.Active &&
                                          s.EffectiveStartDate <= date &&
                                          (!s.EffectiveEndDate.HasValue || s.EffectiveEndDate.Value >= date) &&
                                          !s.IsDelete, cancellationToken);

            if (standard == null)
            {
                return WorkforcePlanningResult<StaffingStandardResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Belum ada standar kebutuhan tenaga yang berlaku untuk unit ini pada tanggal tersebut.");
            }

            var response = await MapToResponseAsync(standard, cancellationToken);
            return WorkforcePlanningResult<StaffingStandardResponse>.Ok(
                response, "Standar kebutuhan tenaga yang berlaku berhasil diambil.");
        }

        public async Task<WorkforcePlanningResult<StaffingStandardResponse>> CreateDraftAsync(
            CreateStaffingStandardRequest request,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                return WorkforcePlanningResult<StaffingStandardResponse>.Fail(
                    StatusCodes.Status400BadRequest, "Data permintaan tidak boleh kosong.");
            }

            // Batas cakupan Kepala Unit (VR-WFP-38)
            var scopedUnitIds = await GetScopedOrganizationUnitIdsAsync(currentUserId, cancellationToken);
            if (scopedUnitIds != null && !scopedUnitIds.Contains(request.OrganizationUnitId))
            {
                return WorkforcePlanningResult<StaffingStandardResponse>.Fail(
                    StatusCodes.Status404NotFound, "Data tidak ditemukan.");
            }

            var unit = await _dbContext.MstOrganizationUnits.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == request.OrganizationUnitId && !u.IsDelete, cancellationToken);

            if (unit == null)
            {
                return WorkforcePlanningResult<StaffingStandardResponse>.Fail(
                    StatusCodes.Status400BadRequest, "Unit organisasi tidak ditemukan atau tidak aktif.");
            }

            // Kriteria 1: Draf kedua untuk unit yang masih punya draf/Returned → 409 VR-WFP-01
            var hasOpenDraft = await _dbContext.HrdStaffingStandards.AnyAsync(s =>
                s.OrganizationUnitId == request.OrganizationUnitId &&
                (s.StandardStatus == StaffingStandardStatus.Draft || s.StandardStatus == StaffingStandardStatus.Returned) &&
                !s.IsDelete, cancellationToken);

            if (hasOpenDraft)
            {
                return WorkforcePlanningResult<StaffingStandardResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Unit ini masih punya draf standar yang belum selesai. Lanjutkan draf itu atau batalkan lebih dulu.");
            }

            // Tentukan nomor versi berikutnya
            var latestVersion = await _dbContext.HrdStaffingStandards.AsNoTracking()
                .Where(s => s.OrganizationUnitId == request.OrganizationUnitId && !s.IsDelete)
                .MaxAsync(s => (int?)s.VersionNumber, cancellationToken) ?? 0;
            var nextVersion = latestVersion + 1;

            // Alokasikan nomor dokumen bisnis
            string standardNumber;
            try
            {
                standardNumber = await _numberAllocator.AllocateAsync(new NumberAllocationRequest(
                    SequenceKey: WorkforcePlanningValueConstants.NumberSeriesKeys.StaffingStandard,
                    Prefix: "STD",
                    ResetPolicy: NumberSeriesResetPolicies.Never,
                    SequenceDigits: 6,
                    ActorUserId: currentUserId,
                    Instant: DateTimeOffset.UtcNow), cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Alokasi nomor bisnis StaffingStandard gagal, memakai format darurat.");
                standardNumber = $"STD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..4].ToUpperInvariant()}";
            }

            // Tentukan ID rumah sakit
            var hospitalSiteId = unit.HospitalSiteId ?? Guid.Empty;
            if (hospitalSiteId == Guid.Empty)
            {
                var defaultSite = await _dbContext.MstHospitalSites.AsNoTracking()
                    .FirstOrDefaultAsync(s => !s.IsDelete, cancellationToken);
                if (defaultSite != null)
                {
                    hospitalSiteId = defaultSite.Id;
                }
            }

            var newStandard = new HrdStaffingStandard
            {
                Id = Guid.NewGuid(),
                StandardNumber = standardNumber,
                HospitalSiteId = hospitalSiteId,
                OrganizationUnitId = request.OrganizationUnitId,
                VersionNumber = nextVersion,
                EffectiveStartDate = request.EffectiveStartDate,
                EffectiveEndDate = null,
                StandardStatus = StaffingStandardStatus.Draft,
                RowVersion = Guid.NewGuid(),
                CreateDateTime = DateTime.UtcNow,
                CreateBy = currentUserId,
                IsDelete = false
            };

            // Kriteria 1: bila ada versi aktif, isinya disalin sebagai titik awal
            var activeStandard = await _dbContext.HrdStaffingStandards.AsNoTracking()
                .Include(s => s.Lines.Where(l => !l.IsDelete))
                .Include(s => s.SkillMixRules.Where(r => !r.IsDelete))
                .FirstOrDefaultAsync(s => s.OrganizationUnitId == request.OrganizationUnitId &&
                                          s.StandardStatus == StaffingStandardStatus.Active &&
                                          !s.IsDelete, cancellationToken);

            if (activeStandard != null)
            {
                foreach (var actLine in activeStandard.Lines)
                {
                    newStandard.Lines.Add(new HrdStaffingStandardLine
                    {
                        Id = Guid.NewGuid(),
                        StaffingStandardId = newStandard.Id,
                        PositionId = actLine.PositionId,
                        ProfessionId = actLine.ProfessionId,
                        CalculationMethod = actLine.CalculationMethod,
                        MinimumHeadcount = actLine.MinimumHeadcount,
                        RatioStaffQuantity = actLine.RatioStaffQuantity,
                        RatioWorkloadQuantity = actLine.RatioWorkloadQuantity,
                        WorkloadUnit = actLine.WorkloadUnit,
                        ShiftsPerDay = actLine.ShiftsPerDay,
                        StaffPerShift = actLine.StaffPerShift,
                        ServiceHoursPerDay = actLine.ServiceHoursPerDay,
                        ReliefFactor = actLine.ReliefFactor,
                        RoundingMethod = actLine.RoundingMethod,
                        CountsOutsourced = actLine.CountsOutsourced,
                        Notes = actLine.Notes,
                        CreateDateTime = DateTime.UtcNow,
                        CreateBy = currentUserId,
                        IsDelete = false
                    });
                }

                foreach (var actRule in activeStandard.SkillMixRules)
                {
                    newStandard.SkillMixRules.Add(new HrdStaffingSkillMixRule
                    {
                        Id = Guid.NewGuid(),
                        StaffingStandardId = newStandard.Id,
                        ShiftId = actRule.ShiftId,
                        ProfessionId = actRule.ProfessionId,
                        CompetencyId = actRule.CompetencyId,
                        MinimumCompetencyLevel = actRule.MinimumCompetencyLevel,
                        MinimumQualifiedHeadcount = actRule.MinimumQualifiedHeadcount,
                        IsMandatory = actRule.IsMandatory,
                        Notes = actRule.Notes,
                        CreateDateTime = DateTime.UtcNow,
                        CreateBy = currentUserId,
                        IsDelete = false
                    });
                }

                _logger.LogInformation(
                    "Draf standar baru {StandardNumber} menyalin {LineCount} baris dan {RuleCount} skill mix dari versi aktif {ActiveNumber}",
                    newStandard.StandardNumber,
                    newStandard.Lines.Count,
                    newStandard.SkillMixRules.Count,
                    activeStandard.StandardNumber);
            }

            _dbContext.HrdStaffingStandards.Add(newStandard);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Draf standar kebutuhan tenaga {StandardNumber} versi {VersionNumber} berhasil dibuat untuk unit {UnitId}",
                newStandard.StandardNumber,
                newStandard.VersionNumber,
                newStandard.OrganizationUnitId);

            var result = await GetByIdAsync(newStandard.Id, currentUserId, cancellationToken);
            return WorkforcePlanningResult<StaffingStandardResponse>.Ok(
                result.Data!,
                "Draf standar kebutuhan tenaga berhasil dibuat.",
                StatusCodes.Status201Created);
        }

        public async Task<WorkforcePlanningResult<StaffingStandardResponse>> UpdateDraftAsync(
            Guid id,
            UpdateStaffingStandardRequest request,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                return WorkforcePlanningResult<StaffingStandardResponse>.Fail(
                    StatusCodes.Status400BadRequest, "Data permintaan tidak boleh kosong.");
            }

            var standard = await _dbContext.HrdStaffingStandards
                .Include(s => s.Lines)
                .Include(s => s.SkillMixRules)
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDelete, cancellationToken);

            if (standard == null)
            {
                return WorkforcePlanningResult<StaffingStandardResponse>.Fail(
                    StatusCodes.Status404NotFound, "Data tidak ditemukan.");
            }

            // Batas cakupan Kepala Unit (VR-WFP-38)
            var scopedUnitIds = await GetScopedOrganizationUnitIdsAsync(currentUserId, cancellationToken);
            if (scopedUnitIds != null && !scopedUnitIds.Contains(standard.OrganizationUnitId))
            {
                return WorkforcePlanningResult<StaffingStandardResponse>.Fail(
                    StatusCodes.Status404NotFound, "Data tidak ditemukan.");
            }

            // Kriteria 5: Versi Active, Scheduled, Superseded tidak dapat diubah isinya
            if (standard.StandardStatus != StaffingStandardStatus.Draft &&
                standard.StandardStatus != StaffingStandardStatus.Returned)
            {
                return WorkforcePlanningResult<StaffingStandardResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Standar berstatus {standard.StandardStatus} tidak dapat diubah isinya.");
            }

            // Konkurensi
            if (standard.RowVersion != request.RowVersion)
            {
                return WorkforcePlanningResult<StaffingStandardResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Data standar telah diubah oleh pengguna lain. Silakan muat ulang halaman.");
            }

            // Kriteria 2: Metode Analisis Beban Kerja → 422 VR-WFP-02
            if (request.Lines.Any(l => l.CalculationMethod == StaffingCalculationMethod.WorkloadAnalysis))
            {
                return WorkforcePlanningResult<StaffingStandardResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    "Metode Analisis Beban Kerja belum tersedia pada rilis ini. Hitung di luar sistem lalu isi hasilnya sebagai jumlah tetap.");
            }

            // Kriteria 2: Faktor cadangan di bawah 1,00 → 400 VR-WFP-05
            if (request.Lines.Any(l => l.ReliefFactor.HasValue && l.ReliefFactor.Value < 1.00m))
            {
                return WorkforcePlanningResult<StaffingStandardResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Faktor cadangan paling kecil 1,00.");
            }

            // Kriteria 2: Jabatan ganda → 409 VR-WFP-07
            var duplicatePosition = request.Lines
                .GroupBy(l => l.PositionId)
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicatePosition != null)
            {
                var pos = await _dbContext.MstPositions.AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == duplicatePosition.Key, cancellationToken);
                var posName = pos?.PositionName ?? duplicatePosition.Key.ToString();

                return WorkforcePlanningResult<StaffingStandardResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    $"Jabatan {posName} sudah ada pada standar ini.");
            }

            standard.EffectiveStartDate = request.EffectiveStartDate;
            standard.Justification = request.Justification?.Trim();

            // Ganti baris jabatan
            _dbContext.HrdStaffingStandardLines.RemoveRange(standard.Lines);
            standard.Lines.Clear();

            foreach (var reqLine in request.Lines)
            {
                standard.Lines.Add(new HrdStaffingStandardLine
                {
                    Id = Guid.NewGuid(),
                    StaffingStandardId = standard.Id,
                    PositionId = reqLine.PositionId,
                    ProfessionId = reqLine.ProfessionId,
                    CalculationMethod = reqLine.CalculationMethod,
                    MinimumHeadcount = reqLine.MinimumHeadcount,
                    RatioStaffQuantity = reqLine.RatioStaffQuantity,
                    RatioWorkloadQuantity = reqLine.RatioWorkloadQuantity,
                    WorkloadUnit = reqLine.WorkloadUnit?.Trim(),
                    ShiftsPerDay = reqLine.ShiftsPerDay,
                    StaffPerShift = reqLine.StaffPerShift,
                    ServiceHoursPerDay = reqLine.ServiceHoursPerDay,
                    ReliefFactor = reqLine.ReliefFactor,
                    RoundingMethod = reqLine.RoundingMethod,
                    CountsOutsourced = reqLine.CountsOutsourced,
                    Notes = reqLine.Notes?.Trim(),
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = currentUserId,
                    IsDelete = false
                });
            }

            // Ganti aturan skill mix
            _dbContext.HrdStaffingSkillMixRules.RemoveRange(standard.SkillMixRules);
            standard.SkillMixRules.Clear();

            foreach (var reqRule in request.SkillMixRules)
            {
                standard.SkillMixRules.Add(new HrdStaffingSkillMixRule
                {
                    Id = Guid.NewGuid(),
                    StaffingStandardId = standard.Id,
                    ShiftId = reqRule.ShiftId,
                    ProfessionId = reqRule.ProfessionId,
                    CompetencyId = reqRule.CompetencyId,
                    MinimumCompetencyLevel = reqRule.MinimumCompetencyLevel?.Trim(),
                    MinimumQualifiedHeadcount = reqRule.MinimumQualifiedHeadcount,
                    IsMandatory = reqRule.IsMandatory,
                    Notes = reqRule.Notes?.Trim(),
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = currentUserId,
                    IsDelete = false
                });
            }

            standard.RowVersion = Guid.NewGuid();
            standard.UpdateDateTime = DateTime.UtcNow;
            standard.UpdateBy = currentUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Standar kebutuhan tenaga {StandardNumber} berhasil diperbarui dengan {LineCount} baris dan {RuleCount} skill mix",
                standard.StandardNumber,
                standard.Lines.Count,
                standard.SkillMixRules.Count);

            var result = await GetByIdAsync(standard.Id, currentUserId, cancellationToken);
            return WorkforcePlanningResult<StaffingStandardResponse>.Ok(
                result.Data!, "Standar kebutuhan tenaga berhasil diperbarui.");
        }

        public async Task<WorkforcePlanningResult<StaffingRecommendationResponse>> GetRecommendationAsync(
            Guid id,
            DateOnly periodStartDate,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            var standard = await _dbContext.HrdStaffingStandards.AsNoTracking()
                .Include(s => s.OrganizationUnit)
                .Include(s => s.Lines)
                    .ThenInclude(l => l.Position)
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDelete, cancellationToken);

            if (standard == null)
            {
                return WorkforcePlanningResult<StaffingRecommendationResponse>.Fail(
                    StatusCodes.Status404NotFound, "Data tidak ditemukan.");
            }

            // Batas cakupan Kepala Unit (VR-WFP-38)
            var scopedUnitIds = await GetScopedOrganizationUnitIdsAsync(currentUserId, cancellationToken);
            if (scopedUnitIds != null && !scopedUnitIds.Contains(standard.OrganizationUnitId))
            {
                return WorkforcePlanningResult<StaffingRecommendationResponse>.Fail(
                    StatusCodes.Status404NotFound, "Data tidak ditemukan.");
            }

            var recommendation = await _recommendationService.CalculateRecommendationAsync(
                standard, periodStartDate, cancellationToken);

            return WorkforcePlanningResult<StaffingRecommendationResponse>.Ok(
                recommendation, "Rekomendasi kebutuhan tenaga kerja berhasil dihitung.");
        }

        public async Task<WorkforcePlanningResult<StaffingStandardResponse>> SubmitAsync(
            Guid id,
            SubmitStaffingStandardRequest request,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                return WorkforcePlanningResult<StaffingStandardResponse>.Fail(
                    StatusCodes.Status400BadRequest, "Data permintaan pengajuan tidak boleh kosong.");
            }

            var standard = await _dbContext.HrdStaffingStandards
                .Include(s => s.Lines)
                    .ThenInclude(l => l.Position)
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDelete, cancellationToken);

            if (standard == null)
            {
                return WorkforcePlanningResult<StaffingStandardResponse>.Fail(
                    StatusCodes.Status404NotFound, "Data tidak ditemukan.");
            }

            // Batas cakupan Kepala Unit (VR-WFP-38)
            var scopedUnitIds = await GetScopedOrganizationUnitIdsAsync(currentUserId, cancellationToken);
            if (scopedUnitIds != null && !scopedUnitIds.Contains(standard.OrganizationUnitId))
            {
                return WorkforcePlanningResult<StaffingStandardResponse>.Fail(
                    StatusCodes.Status404NotFound, "Data tidak ditemukan.");
            }

            if (standard.StandardStatus != StaffingStandardStatus.Draft &&
                standard.StandardStatus != StaffingStandardStatus.Returned)
            {
                return WorkforcePlanningResult<StaffingStandardResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Standar berstatus {standard.StandardStatus} tidak dapat diajukan. Hanya status Draft atau Returned yang sah.");
            }

            if (standard.RowVersion != request.RowVersion)
            {
                return WorkforcePlanningResult<StaffingStandardResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Data standar telah diubah oleh pengguna lain.");
            }

            var activeLines = standard.Lines.Where(l => !l.IsDelete).ToList();
            if (activeLines.Count == 0)
            {
                return WorkforcePlanningResult<StaffingStandardResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    "Standar kebutuhan tenaga harus memiliki sekurang-kurangnya satu baris jabatan.");
            }

            // Kriteria 3: Tanggal berlaku di masa lalu → 422 VR-WFP-06
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (standard.EffectiveStartDate < today)
            {
                return WorkforcePlanningResult<StaffingStandardResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    "Tanggal berlaku standar tidak boleh di masa lalu.");
            }

            // Kriteria 3: Validasi kelengkapan per metode (VR-WFP-03 & VR-WFP-04)
            foreach (var line in activeLines)
            {
                var posName = line.Position?.PositionName ?? line.PositionId.ToString();

                // VR-WFP-04: Cara pembulatan belum dipilih (sistem tidak menetapkan nilai bawaan)
                if (line.RoundingMethod == 0)
                {
                    return WorkforcePlanningResult<StaffingStandardResponse>.Fail(
                        StatusCodes.Status422UnprocessableEntity,
                        "Faktor cadangan dan cara pembulatan wajib dipilih. Sistem tidak menetapkan nilai bawaan.");
                }

                switch (line.CalculationMethod)
                {
                    case StaffingCalculationMethod.FixedMinimum:
                        if (!line.MinimumHeadcount.HasValue || line.MinimumHeadcount.Value <= 0)
                        {
                            return WorkforcePlanningResult<StaffingStandardResponse>.Fail(
                                StatusCodes.Status422UnprocessableEntity,
                                $"Baris jabatan {posName} belum lengkap untuk metode Jumlah Tetap: angka minimum tenaga wajib diisi.");
                        }
                        break;

                    case StaffingCalculationMethod.WorkloadRatio:
                        // VR-WFP-04: Faktor cadangan kosong pada metode yang memerlukannya
                        if (!line.ReliefFactor.HasValue || line.ReliefFactor.Value < 1.00m)
                        {
                            return WorkforcePlanningResult<StaffingStandardResponse>.Fail(
                                StatusCodes.Status422UnprocessableEntity,
                                "Faktor cadangan dan cara pembulatan wajib dipilih. Sistem tidak menetapkan nilai bawaan.");
                        }

                        // VR-WFP-03: Rasio tanpa rasio, satuan beban, jumlah shift, atau faktor cadangan
                        if (!line.RatioStaffQuantity.HasValue || line.RatioStaffQuantity.Value <= 0 ||
                            !line.RatioWorkloadQuantity.HasValue || line.RatioWorkloadQuantity.Value <= 0 ||
                            string.IsNullOrWhiteSpace(line.WorkloadUnit) ||
                            !line.ShiftsPerDay.HasValue || line.ShiftsPerDay.Value <= 0)
                        {
                            return WorkforcePlanningResult<StaffingStandardResponse>.Fail(
                                StatusCodes.Status422UnprocessableEntity,
                                $"Baris jabatan {posName} belum lengkap untuk metode Rasio Beban Kerja: rasio, satuan beban, jumlah shift, atau faktor cadangan belum lengkap.");
                        }
                        break;

                    case StaffingCalculationMethod.ServiceCoverage:
                        // VR-WFP-04: Faktor cadangan kosong
                        if (!line.ReliefFactor.HasValue || line.ReliefFactor.Value < 1.00m)
                        {
                            return WorkforcePlanningResult<StaffingStandardResponse>.Fail(
                                StatusCodes.Status422UnprocessableEntity,
                                "Faktor cadangan dan cara pembulatan wajib dipilih. Sistem tidak menetapkan nilai bawaan.");
                        }

                        // VR-WFP-03: Cakupan layanan tanpa jumlah shift, orang per shift, atau faktor cadangan
                        if (!line.ShiftsPerDay.HasValue || line.ShiftsPerDay.Value <= 0 ||
                            !line.StaffPerShift.HasValue || line.StaffPerShift.Value <= 0)
                        {
                            return WorkforcePlanningResult<StaffingStandardResponse>.Fail(
                                StatusCodes.Status422UnprocessableEntity,
                                $"Baris jabatan {posName} belum lengkap untuk metode Cakupan Layanan: jumlah shift, orang per shift, atau faktor cadangan belum lengkap.");
                        }
                        break;

                    case StaffingCalculationMethod.WorkloadAnalysis:
                        return WorkforcePlanningResult<StaffingStandardResponse>.Fail(
                            StatusCodes.Status422UnprocessableEntity,
                            "Metode Analisis Beban Kerja belum tersedia pada rilis ini. Hitung di luar sistem lalu isi hasilnya sebagai jumlah tetap.");
                }
            }

            // Kriteria 3: Ajukan ke T9; menolak 400 bila penyetuju langkah Direktur tidak ditemukan
            var workflowResult = await _workflowIntegrationService.StartWorkflowAsync(new StartWorkforcePlanningWorkflowRequest
            {
                WorkflowDefinitionCode = "HRD_T9_STAFFING_STANDARD",
                ReferenceType = WorkforcePlanningValueConstants.WorkflowReferenceTypes.WorkforceStaffingStandard,
                ReferenceId = standard.Id,
                DocumentNumber = standard.StandardNumber,
                SubmitComment = request.Notes ?? "Pengajuan Standar Kebutuhan Tenaga",
                AutoSubmit = true
            }, cancellationToken);

            if (!workflowResult.Success)
            {
                _logger.LogWarning(
                    "Workflow T9 gagal dimulai untuk standar {StandardNumber}: {Message}",
                    standard.StandardNumber,
                    workflowResult.Message);

                return WorkforcePlanningResult<StaffingStandardResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    workflowResult.Message ?? "Penyetuju langkah Direktur tidak ditemukan pada matriks persetujuan (fail closed).");
            }

            standard.StandardStatus = StaffingStandardStatus.Submitted;
            standard.SubmittedAt = DateTime.UtcNow;
            standard.SubmittedByUserId = currentUserId;
            standard.WorkflowInstanceId = workflowResult.Data?.Id;
            standard.WorkflowDefinitionId = workflowResult.Data?.WorkflowDefinitionId;
            standard.RowVersion = Guid.NewGuid();
            standard.UpdateDateTime = DateTime.UtcNow;
            standard.UpdateBy = currentUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Standar kebutuhan tenaga {StandardNumber} berhasil diajukan (Submitted) ke workflow T9.",
                standard.StandardNumber);

            var result = await GetByIdAsync(standard.Id, currentUserId, cancellationToken);
            return WorkforcePlanningResult<StaffingStandardResponse>.Ok(
                result.Data!, "Standar kebutuhan tenaga berhasil diajukan ke workflow persetujuan T9.");
        }

        public async Task<WorkforcePlanningResult<StaffingStandardResponse>> CancelAsync(
            Guid id,
            CancelStaffingStandardRequest request,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                return WorkforcePlanningResult<StaffingStandardResponse>.Fail(
                    StatusCodes.Status400BadRequest, "Data permintaan pembatalan tidak boleh kosong.");
            }

            var standard = await _dbContext.HrdStaffingStandards
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDelete, cancellationToken);

            if (standard == null)
            {
                return WorkforcePlanningResult<StaffingStandardResponse>.Fail(
                    StatusCodes.Status404NotFound, "Data tidak ditemukan.");
            }

            // Batas cakupan Kepala Unit (VR-WFP-38)
            var scopedUnitIds = await GetScopedOrganizationUnitIdsAsync(currentUserId, cancellationToken);
            if (scopedUnitIds != null && !scopedUnitIds.Contains(standard.OrganizationUnitId))
            {
                return WorkforcePlanningResult<StaffingStandardResponse>.Fail(
                    StatusCodes.Status404NotFound, "Data tidak ditemukan.");
            }

            if (standard.StandardStatus != StaffingStandardStatus.Draft &&
                standard.StandardStatus != StaffingStandardStatus.Returned)
            {
                return WorkforcePlanningResult<StaffingStandardResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Standar berstatus {standard.StandardStatus} tidak dapat dibatalkan. Hanya draf atau versi yang dikembalikan yang dapat dibatalkan.");
            }

            if (standard.RowVersion != request.RowVersion)
            {
                return WorkforcePlanningResult<StaffingStandardResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Data standar telah diubah oleh pengguna lain.");
            }

            standard.StandardStatus = StaffingStandardStatus.Cancelled;
            standard.IsCancel = true;
            standard.CancelDateTime = DateTime.UtcNow;
            standard.CancelBy = currentUserId;
            standard.RowVersion = Guid.NewGuid();
            standard.UpdateDateTime = DateTime.UtcNow;
            standard.UpdateBy = currentUserId;

            if (!string.IsNullOrWhiteSpace(request.Reason))
            {
                standard.Justification = string.IsNullOrWhiteSpace(standard.Justification)
                    ? $"[Dibatalkan]: {request.Reason.Trim()}"
                    : $"{standard.Justification}\n[Dibatalkan]: {request.Reason.Trim()}";
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Standar kebutuhan tenaga {StandardNumber} berhasil dibatalkan.", standard.StandardNumber);

            var result = await GetByIdAsync(standard.Id, currentUserId, cancellationToken);
            return WorkforcePlanningResult<StaffingStandardResponse>.Ok(
                result.Data!, "Standar kebutuhan tenaga berhasil dibatalkan.");
        }

        public StaffingStandardFilterMetadataResponse GetFilterMetadata()
        {
            return new StaffingStandardFilterMetadataResponse
            {
                Statuses = Enum.GetValues<StaffingStandardStatus>()
                    .Select(e => new EnumOptionResponse
                    {
                        Value = (int)e,
                        Name = e.ToString(),
                        DisplayText = e switch
                        {
                            StaffingStandardStatus.Draft => "Draf",
                            StaffingStandardStatus.Submitted => "Diajukan",
                            StaffingStandardStatus.Returned => "Dikembalikan",
                            StaffingStandardStatus.Rejected => "Ditolak",
                            StaffingStandardStatus.Scheduled => "Dijadwalkan",
                            StaffingStandardStatus.Active => "Aktif",
                            StaffingStandardStatus.Superseded => "Digantikan",
                            StaffingStandardStatus.Cancelled => "Dibatalkan",
                            _ => e.ToString()
                        }
                    }).ToList(),

                CalculationMethods = Enum.GetValues<StaffingCalculationMethod>()
                    .Select(e => new EnumOptionResponse
                    {
                        Value = (int)e,
                        Name = e.ToString(),
                        DisplayText = e switch
                        {
                            StaffingCalculationMethod.FixedMinimum => "Jumlah Tetap",
                            StaffingCalculationMethod.WorkloadRatio => "Rasio Beban Kerja",
                            StaffingCalculationMethod.ServiceCoverage => "Cakupan Layanan",
                            StaffingCalculationMethod.WorkloadAnalysis => "Analisis Beban Kerja",
                            _ => e.ToString()
                        }
                    }).ToList(),

                RoundingMethods = Enum.GetValues<StaffingRoundingMethod>()
                    .Select(e => new EnumOptionResponse
                    {
                        Value = (int)e,
                        Name = e.ToString(),
                        DisplayText = e switch
                        {
                            StaffingRoundingMethod.Ceiling => "Bulatkan ke Atas (Ceiling)",
                            StaffingRoundingMethod.Floor => "Bulatkan ke Bawah (Floor)",
                            StaffingRoundingMethod.Nearest => "Bulatkan Terdekat (Nearest)",
                            StaffingRoundingMethod.NoRounding => "Tanpa Pembulatan",
                            _ => e.ToString()
                        }
                    }).ToList()
            };
        }

        private async Task<StaffingStandardResponse> MapToResponseAsync(
            HrdStaffingStandard standard,
            CancellationToken cancellationToken)
        {
            var userIds = new List<Guid>();
            if (standard.SubmittedByUserId.HasValue) userIds.Add(standard.SubmittedByUserId.Value);
            if (standard.ApprovedByUserId.HasValue) userIds.Add(standard.ApprovedByUserId.Value);

            var users = userIds.Count > 0
                ? await _dbContext.Users.AsNoTracking()
                    .Where(u => userIds.Contains(u.Id))
                    .ToDictionaryAsync(u => u.Id, u => !string.IsNullOrWhiteSpace(u.DisplayName) ? u.DisplayName : u.UserName ?? string.Empty, cancellationToken)
                : new Dictionary<Guid, string>();

            return new StaffingStandardResponse
            {
                Id = standard.Id,
                StandardNumber = standard.StandardNumber,
                HospitalSiteId = standard.HospitalSiteId,
                HospitalSiteName = standard.HospitalSite?.SiteName,
                OrganizationUnitId = standard.OrganizationUnitId,
                OrganizationUnitCode = standard.OrganizationUnit?.UnitCode,
                OrganizationUnitName = standard.OrganizationUnit?.UnitName,
                VersionNumber = standard.VersionNumber,
                EffectiveStartDate = standard.EffectiveStartDate,
                EffectiveEndDate = standard.EffectiveEndDate,
                StandardStatus = standard.StandardStatus,
                Justification = standard.Justification,
                SupersedesStandardId = standard.SupersedesStandardId,
                WorkflowInstanceId = standard.WorkflowInstanceId,
                SubmittedAt = standard.SubmittedAt,
                SubmittedByUserName = standard.SubmittedByUserId.HasValue && users.TryGetValue(standard.SubmittedByUserId.Value, out var sUser) ? sUser : null,
                ApprovedAt = standard.ApprovedAt,
                ApprovedByUserName = standard.ApprovedByUserId.HasValue && users.TryGetValue(standard.ApprovedByUserId.Value, out var aUser) ? aUser : null,
                ActivatedAt = standard.ActivatedAt,
                CreatedAt = standard.CreateDateTime,
                RowVersion = standard.RowVersion,
                Lines = standard.Lines.Where(l => !l.IsDelete).Select(l => new StaffingStandardLineResponse
                {
                    Id = l.Id,
                    PositionId = l.PositionId,
                    PositionCode = l.Position?.PositionCode,
                    PositionName = l.Position?.PositionName,
                    ProfessionId = l.ProfessionId,
                    ProfessionName = l.Profession?.ProfessionName,
                    CalculationMethod = l.CalculationMethod,
                    MinimumHeadcount = l.MinimumHeadcount,
                    RatioStaffQuantity = l.RatioStaffQuantity,
                    RatioWorkloadQuantity = l.RatioWorkloadQuantity,
                    WorkloadUnit = l.WorkloadUnit,
                    ShiftsPerDay = l.ShiftsPerDay,
                    StaffPerShift = l.StaffPerShift,
                    ServiceHoursPerDay = l.ServiceHoursPerDay,
                    ReliefFactor = l.ReliefFactor,
                    RoundingMethod = l.RoundingMethod,
                    CountsOutsourced = l.CountsOutsourced,
                    Notes = l.Notes
                }).ToList(),
                SkillMixRules = standard.SkillMixRules.Where(r => !r.IsDelete).Select(r => new StaffingSkillMixRuleResponse
                {
                    Id = r.Id,
                    ShiftId = r.ShiftId,
                    ShiftName = r.Shift?.ShiftName,
                    ProfessionId = r.ProfessionId,
                    ProfessionName = r.Profession?.ProfessionName,
                    CompetencyId = r.CompetencyId,
                    CompetencyCode = r.Competency?.CompetencyCode,
                    CompetencyName = r.Competency?.CompetencyName,
                    MinimumCompetencyLevel = r.MinimumCompetencyLevel,
                    MinimumQualifiedHeadcount = r.MinimumQualifiedHeadcount,
                    IsMandatory = r.IsMandatory,
                    Notes = r.Notes
                }).ToList()
            };
        }

        private async Task<HashSet<Guid>?> GetScopedOrganizationUnitIdsAsync(Guid userId, CancellationToken cancellationToken)
        {
            if (userId == Guid.Empty)
                return null;

            var user = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user == null || !user.WorkforceProfileId.HasValue)
                return null;

            var now = DateTime.UtcNow;

            var managerialAssignments = await _dbContext.WfpOrganizationAssignments
                .AsNoTracking()
                .Where(a => a.WorkforceProfileId == user.WorkforceProfileId.Value &&
                            a.IsActive && !a.IsDelete && !a.IsCancel &&
                            a.IsManagerialAssignment &&
                            a.EffectiveStartDate <= now &&
                            (!a.EffectiveEndDate.HasValue || a.EffectiveEndDate.Value >= now))
                .ToListAsync(cancellationToken);

            if (managerialAssignments.Count == 0)
            {
                return null;
            }

            var baseUnitIds = new HashSet<Guid>();
            foreach (var a in managerialAssignments)
            {
                if (a.OrganizationUnitId.HasValue && a.OrganizationUnitId.Value != Guid.Empty)
                {
                    baseUnitIds.Add(a.OrganizationUnitId.Value);
                }
                else if (a.DepartmentId != Guid.Empty)
                {
                    var mappedUnits = await _dbContext.MstOrganizationUnits
                        .AsNoTracking()
                        .Where(u => u.DepartmentId == a.DepartmentId && !u.IsDelete)
                        .Select(u => u.Id)
                        .ToListAsync(cancellationToken);

                    foreach (var mu in mappedUnits) baseUnitIds.Add(mu);
                }
            }

            if (baseUnitIds.Count == 0)
                return null;

            var allScopedUnits = new HashSet<Guid>(baseUnitIds);
            var currentLevel = baseUnitIds.ToList();

            while (currentLevel.Count > 0)
            {
                var childUnits = await _dbContext.MstOrganizationUnits
                    .AsNoTracking()
                    .Where(u => u.ParentOrganizationUnitId.HasValue &&
                                currentLevel.Contains(u.ParentOrganizationUnitId.Value) &&
                                !u.IsDelete)
                    .Select(u => u.Id)
                    .ToListAsync(cancellationToken);

                currentLevel = childUnits.Where(id => allScopedUnits.Add(id)).ToList();
            }

            return allScopedUnits;
        }
    }
}
