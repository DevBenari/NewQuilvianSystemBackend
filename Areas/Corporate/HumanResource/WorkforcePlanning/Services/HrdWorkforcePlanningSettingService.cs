using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Services
{
    /// <summary>
    /// Implementasi layanan pengaturan perencanaan tenaga kerja per rumah sakit.
    /// Memenuhi kriteria BE-HRD-031, HRD-DEC-061, HRD-DEC-071, VR-WFP-40, VR-WFP-41, NFR-010/NFR-011,
    /// serta perluasan BE-HRD-FIN-WFP-001 s.d. 004.
    /// </summary>
    public sealed class HrdWorkforcePlanningSettingService : IHrdWorkforcePlanningSettingService
    {
        private const int MaxChangeReasonLength = 500;
        private const string ForbiddenScopeMessage = "Anda tidak berwenang mengakses konfigurasi rumah sakit di luar cakupan akses Anda.";
        private const string ConcurrencyConflictMessage = "Data telah diubah oleh pengguna lain. Silakan muat ulang data terbaru.";

        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<HrdWorkforcePlanningSettingService> _logger;

        public HrdWorkforcePlanningSettingService(
            ApplicationDbContext dbContext,
            ILogger<HrdWorkforcePlanningSettingService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<WorkforcePlanningResult<WorkforcePlanningSettingListResponse>> GetListAsync(
            WorkforcePlanningSettingListQuery query,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            query ??= new WorkforcePlanningSettingListQuery();

            if (!TryNormalizeStatusFilter(query.ConfigurationStatus, out var normalizedStatusFilter))
            {
                return WorkforcePlanningResult<WorkforcePlanningSettingListResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Status kelengkapan konfigurasi tidak valid.");
            }

            var pageNumber = Math.Max(1, query.PageNumber);
            var pageSize = Math.Clamp(query.PageSize <= 0 ? 10 : query.PageSize, 1, 100);

            var scopedSiteIds = await GetScopedHospitalSiteIdsAsync(currentUserId, cancellationToken);
            if (query.HospitalSiteId.HasValue &&
                query.HospitalSiteId.Value != Guid.Empty &&
                scopedSiteIds != null &&
                !scopedSiteIds.Contains(query.HospitalSiteId.Value))
            {
                return WorkforcePlanningResult<WorkforcePlanningSettingListResponse>.Fail(
                    StatusCodes.Status403Forbidden,
                    ForbiddenScopeMessage);
            }

            var scopedSiteList = scopedSiteIds?.ToList();

            var permittedQuery =
                from site in _dbContext.MstHospitalSites.AsNoTracking()
                join setting in _dbContext.HrdWorkforcePlanningSettings.AsNoTracking().Where(s => !s.IsDelete)
                    on site.Id equals setting.HospitalSiteId into siteSettings
                from setting in siteSettings.DefaultIfEmpty()
                where !site.IsDelete && site.IsActive
                select new
                {
                    SiteId = site.Id,
                    site.SiteCode,
                    site.SiteName,
                    site.IsMainSite,
                    SettingId = setting != null ? (Guid?)setting.Id : null,
                    LongLeaveThresholdDays = setting != null ? setting.LongLeaveThresholdDays : null,
                    OverdueRequestThresholdDays = setting != null ? setting.OverdueRequestThresholdDays : null,
                    ChangeReason = setting != null ? setting.ChangeReason : null,
                    RowVersion = setting != null ? (Guid?)setting.RowVersion : null,
                    UpdateDateTime = setting != null ? setting.UpdateDateTime : null,
                    CreateDateTime = setting != null ? (DateTime?)setting.CreateDateTime : null,
                    UpdateBy = setting != null ? (Guid?)setting.UpdateBy : null,
                    CreateBy = setting != null ? (Guid?)setting.CreateBy : null
                };

            if (scopedSiteList != null)
            {
                permittedQuery = permittedQuery.Where(x => scopedSiteList.Contains(x.SiteId));
            }

            var summaryRow = await permittedQuery
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Total = g.Count(),
                    Complete = g.Sum(x => x.LongLeaveThresholdDays.HasValue && x.OverdueRequestThresholdDays.HasValue ? 1 : 0),
                    Incomplete = g.Sum(x =>
                        (x.LongLeaveThresholdDays.HasValue && !x.OverdueRequestThresholdDays.HasValue) ||
                        (!x.LongLeaveThresholdDays.HasValue && x.OverdueRequestThresholdDays.HasValue)
                        ? 1 : 0),
                    NotConfigured = g.Sum(x => !x.LongLeaveThresholdDays.HasValue && !x.OverdueRequestThresholdDays.HasValue ? 1 : 0)
                })
                .FirstOrDefaultAsync(cancellationToken);

            var filteredQuery = permittedQuery;

            if (query.HospitalSiteId.HasValue && query.HospitalSiteId.Value != Guid.Empty)
            {
                var targetSiteId = query.HospitalSiteId.Value;
                filteredQuery = filteredQuery.Where(x => x.SiteId == targetSiteId);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var term = query.Search.Trim().ToLower();
                filteredQuery = filteredQuery.Where(x =>
                    x.SiteName.ToLower().Contains(term) ||
                    x.SiteCode.ToLower().Contains(term));
            }

            if (normalizedStatusFilter == WorkforcePlanningSettingStatuses.Complete)
            {
                filteredQuery = filteredQuery.Where(x =>
                    x.LongLeaveThresholdDays.HasValue &&
                    x.OverdueRequestThresholdDays.HasValue);
            }
            else if (normalizedStatusFilter == WorkforcePlanningSettingStatuses.Incomplete)
            {
                filteredQuery = filteredQuery.Where(x =>
                    (x.LongLeaveThresholdDays.HasValue && !x.OverdueRequestThresholdDays.HasValue) ||
                    (!x.LongLeaveThresholdDays.HasValue && x.OverdueRequestThresholdDays.HasValue));
            }
            else if (normalizedStatusFilter == WorkforcePlanningSettingStatuses.NotConfigured)
            {
                filteredQuery = filteredQuery.Where(x =>
                    !x.LongLeaveThresholdDays.HasValue &&
                    !x.OverdueRequestThresholdDays.HasValue);
            }

            var totalData = await filteredQuery.CountAsync(cancellationToken);
            var totalPage = totalData == 0 ? 0 : (int)Math.Ceiling(totalData / (double)pageSize);

            var pagedRows = await filteredQuery
                .OrderByDescending(x => x.IsMainSite)
                .ThenBy(x => x.SiteName)
                .ThenBy(x => x.SiteId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var actorIds = pagedRows
                .Select(x => ResolveActorUserId(x.UpdateBy, x.CreateBy))
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();

            var actorNames = await ResolveUserDisplayNamesAsync(actorIds, cancellationToken);

            var items = pagedRows.Select(x =>
            {
                var actorId = ResolveActorUserId(x.UpdateBy, x.CreateBy);
                var updatedByName = actorId.HasValue && actorNames.TryGetValue(actorId.Value, out var name)
                    ? name
                    : null;

                return new WorkforcePlanningSettingResponse
                {
                    Id = x.SettingId,
                    HospitalSiteId = x.SiteId,
                    HospitalSiteCode = x.SiteCode,
                    HospitalSiteName = x.SiteName,
                    LongLeaveThresholdDays = x.LongLeaveThresholdDays,
                    OverdueRequestThresholdDays = x.OverdueRequestThresholdDays,
                    ConfigurationStatus = WorkforcePlanningSettingStatuses.Compute(
                        x.LongLeaveThresholdDays,
                        x.OverdueRequestThresholdDays),
                    ChangeReason = x.ChangeReason,
                    RowVersion = x.RowVersion,
                    UpdatedAt = x.SettingId.HasValue ? (x.UpdateDateTime ?? x.CreateDateTime) : null,
                    UpdatedBy = actorId,
                    UpdatedByName = updatedByName
                };
            }).ToList();

            var summary = new WorkforcePlanningSettingSummaryResponse
            {
                TotalHospitalSites = summaryRow?.Total ?? 0,
                CompleteCount = summaryRow?.Complete ?? 0,
                IncompleteCount = summaryRow?.Incomplete ?? 0,
                NotConfiguredCount = summaryRow?.NotConfigured ?? 0,
                IncompleteOrNotConfiguredCount = (summaryRow?.Incomplete ?? 0) + (summaryRow?.NotConfigured ?? 0)
            };

            return WorkforcePlanningResult<WorkforcePlanningSettingListResponse>.Ok(
                new WorkforcePlanningSettingListResponse
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = totalPage,
                    Items = items,
                    Summary = summary
                },
                "Daftar pengaturan perencanaan rumah sakit berhasil diambil.");
        }

        public Task<WorkforcePlanningResult<WorkforcePlanningSettingResponse>> GetByHospitalSiteIdAsync(
            Guid hospitalSiteId,
            CancellationToken cancellationToken = default) =>
            GetByHospitalSiteIdInternalAsync(hospitalSiteId, enforceScope: false, Guid.Empty, cancellationToken);

        public Task<WorkforcePlanningResult<WorkforcePlanningSettingResponse>> GetByHospitalSiteIdAsync(
            Guid hospitalSiteId,
            Guid currentUserId,
            CancellationToken cancellationToken = default) =>
            GetByHospitalSiteIdInternalAsync(hospitalSiteId, enforceScope: true, currentUserId, cancellationToken);

        private async Task<WorkforcePlanningResult<WorkforcePlanningSettingResponse>> GetByHospitalSiteIdInternalAsync(
            Guid hospitalSiteId,
            bool enforceScope,
            Guid currentUserId,
            CancellationToken cancellationToken)
        {
            if (hospitalSiteId == Guid.Empty)
            {
                return WorkforcePlanningResult<WorkforcePlanningSettingResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "ID rumah sakit wajib diisi.");
            }

            if (enforceScope)
            {
                var scopedSiteIds = await GetScopedHospitalSiteIdsAsync(currentUserId, cancellationToken);
                if (scopedSiteIds != null && !scopedSiteIds.Contains(hospitalSiteId))
                {
                    return WorkforcePlanningResult<WorkforcePlanningSettingResponse>.Fail(
                        StatusCodes.Status403Forbidden,
                        ForbiddenScopeMessage);
                }
            }

            var site = await _dbContext.MstHospitalSites
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == hospitalSiteId && !s.IsDelete, cancellationToken);

            if (site == null)
            {
                return WorkforcePlanningResult<WorkforcePlanningSettingResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Rumah sakit tidak ditemukan.");
            }

            var setting = await _dbContext.HrdWorkforcePlanningSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.HospitalSiteId == hospitalSiteId && !s.IsDelete, cancellationToken);

            if (setting == null)
            {
                // Kriteria 1: Rumah sakit yang belum punya baris mendapat kedua nilai kosong (null), bukan angka bawaan.
                return WorkforcePlanningResult<WorkforcePlanningSettingResponse>.Ok(new WorkforcePlanningSettingResponse
                {
                    Id = null,
                    HospitalSiteId = site.Id,
                    HospitalSiteCode = site.SiteCode,
                    HospitalSiteName = site.SiteName,
                    LongLeaveThresholdDays = null,
                    OverdueRequestThresholdDays = null,
                    ConfigurationStatus = WorkforcePlanningSettingStatuses.NotConfigured,
                    ChangeReason = null,
                    RowVersion = null,
                    UpdatedAt = null,
                    UpdatedBy = null,
                    UpdatedByName = null
                }, "Pengaturan rumah sakit berhasil diambil.");
            }

            var response = await BuildSettingResponseAsync(site, setting, cancellationToken);
            return WorkforcePlanningResult<WorkforcePlanningSettingResponse>.Ok(response, "Pengaturan rumah sakit berhasil diambil.");
        }

        public async Task<WorkforcePlanningResult<WorkforcePlanningSettingResponse>> UpdateAsync(
            UpdateWorkforcePlanningSettingRequest request,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            if (request.HospitalSiteId == Guid.Empty)
            {
                return WorkforcePlanningResult<WorkforcePlanningSettingResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "ID rumah sakit wajib diisi.");
            }

            // Kriteria 3 (VR-WFP-40): Alasan perubahan pengaturan tidak boleh kosong dan maksimal 500 karakter
            var trimmedReason = request.ChangeReason?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(trimmedReason))
            {
                return WorkforcePlanningResult<WorkforcePlanningSettingResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Tuliskan alasan perubahan pengaturan.");
            }

            if (trimmedReason.Length > MaxChangeReasonLength)
            {
                return WorkforcePlanningResult<WorkforcePlanningSettingResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Alasan perubahan pengaturan maksimal {MaxChangeReasonLength} karakter.");
            }

            // Kriteria 4 (VR-WFP-41): Batas cuti panjang atau umur permintaan < 1 atau > 365
            if ((request.LongLeaveThresholdDays.HasValue && (request.LongLeaveThresholdDays.Value < 1 || request.LongLeaveThresholdDays.Value > 365)) ||
                (request.OverdueRequestThresholdDays.HasValue && (request.OverdueRequestThresholdDays.Value < 1 || request.OverdueRequestThresholdDays.Value > 365)))
            {
                return WorkforcePlanningResult<WorkforcePlanningSettingResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Isi angka antara 1 dan 365 hari.");
            }

            var scopedSiteIds = await GetScopedHospitalSiteIdsAsync(currentUserId, cancellationToken);
            if (scopedSiteIds != null && !scopedSiteIds.Contains(request.HospitalSiteId))
            {
                return WorkforcePlanningResult<WorkforcePlanningSettingResponse>.Fail(
                    StatusCodes.Status403Forbidden,
                    ForbiddenScopeMessage);
            }

            var site = await _dbContext.MstHospitalSites
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == request.HospitalSiteId && !s.IsDelete, cancellationToken);

            if (site == null)
            {
                return WorkforcePlanningResult<WorkforcePlanningSettingResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Rumah sakit tidak ditemukan.");
            }

            var setting = await _dbContext.HrdWorkforcePlanningSettings
                .FirstOrDefaultAsync(s => s.HospitalSiteId == request.HospitalSiteId && !s.IsDelete, cancellationToken);

            if (setting != null)
            {
                // Kriteria 5: Optimistic concurrency check terhadap rowVersion
                if (!request.RowVersion.HasValue || request.RowVersion.Value == Guid.Empty || request.RowVersion.Value != setting.RowVersion)
                {
                    return WorkforcePlanningResult<WorkforcePlanningSettingResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        ConcurrencyConflictMessage);
                }

                setting.LongLeaveThresholdDays = request.LongLeaveThresholdDays;
                setting.OverdueRequestThresholdDays = request.OverdueRequestThresholdDays;
                setting.ChangeReason = trimmedReason;
                setting.RowVersion = Guid.NewGuid();
                setting.UpdateDateTime = DateTime.UtcNow;
                setting.UpdateBy = currentUserId;
            }
            else
            {
                if (request.RowVersion.HasValue && request.RowVersion.Value != Guid.Empty)
                {
                    return WorkforcePlanningResult<WorkforcePlanningSettingResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        ConcurrencyConflictMessage);
                }

                setting = new HrdWorkforcePlanningSetting
                {
                    Id = Guid.NewGuid(),
                    HospitalSiteId = request.HospitalSiteId,
                    LongLeaveThresholdDays = request.LongLeaveThresholdDays,
                    OverdueRequestThresholdDays = request.OverdueRequestThresholdDays,
                    ChangeReason = trimmedReason,
                    RowVersion = Guid.NewGuid(),
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = currentUserId,
                    IsDelete = false
                };

                _dbContext.HrdWorkforcePlanningSettings.Add(setting);
            }

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                return WorkforcePlanningResult<WorkforcePlanningSettingResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    ConcurrencyConflictMessage);
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                return WorkforcePlanningResult<WorkforcePlanningSettingResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    ConcurrencyConflictMessage);
            }

            // Kriteria 6: Audit trail sesuai permission-audit-matrix.md bagian 11.4.
            // Payload logger HANYA memuat EntityId, controller, action, dan status — TANPA kolom bertanda sensitif (alasan/biaya).
            _logger.LogInformation(
                "WorkforcePlanningSetting updated. EntityId: {EntityId}, Controller: {Controller}, Action: {Action}, Status: {Status}",
                setting.Id,
                "WorkforcePlanningSetting",
                "Update",
                "Success");

            var response = await BuildSettingResponseAsync(site, setting, cancellationToken);
            return WorkforcePlanningResult<WorkforcePlanningSettingResponse>.Ok(response, "Pengaturan perencanaan berhasil disimpan.");
        }

        private async Task<WorkforcePlanningSettingResponse> BuildSettingResponseAsync(
            MstHospitalSite site,
            HrdWorkforcePlanningSetting setting,
            CancellationToken cancellationToken)
        {
            var actorId = ResolveActorUserId(setting.UpdateBy, setting.CreateBy);
            string? updatedByName = null;
            if (actorId.HasValue)
            {
                var names = await ResolveUserDisplayNamesAsync(new[] { actorId.Value }, cancellationToken);
                names.TryGetValue(actorId.Value, out updatedByName);
            }

            return new WorkforcePlanningSettingResponse
            {
                Id = setting.Id,
                HospitalSiteId = setting.HospitalSiteId,
                HospitalSiteCode = site.SiteCode,
                HospitalSiteName = site.SiteName,
                LongLeaveThresholdDays = setting.LongLeaveThresholdDays,
                OverdueRequestThresholdDays = setting.OverdueRequestThresholdDays,
                ConfigurationStatus = WorkforcePlanningSettingStatuses.Compute(
                    setting.LongLeaveThresholdDays,
                    setting.OverdueRequestThresholdDays),
                ChangeReason = setting.ChangeReason,
                RowVersion = setting.RowVersion,
                UpdatedAt = setting.UpdateDateTime ?? setting.CreateDateTime,
                UpdatedBy = actorId,
                UpdatedByName = updatedByName
            };
        }

        private async Task<HashSet<Guid>?> GetScopedHospitalSiteIdsAsync(
            Guid currentUserId,
            CancellationToken cancellationToken)
        {
            if (currentUserId == Guid.Empty)
            {
                return null;
            }

            var user = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == currentUserId && u.IsActive, cancellationToken);

            if (user == null || user.UserType == UserType.SuperAdmin || !user.WorkforceProfileId.HasValue)
            {
                return null;
            }

            var nowUtc = DateTime.UtcNow;
            var managerialAssignments = await _dbContext.WfpOrganizationAssignments
                .AsNoTracking()
                .Where(a =>
                    a.WorkforceProfileId == user.WorkforceProfileId.Value &&
                    a.IsActive &&
                    !a.IsDelete &&
                    !a.IsCancel &&
                    a.IsManagerialAssignment &&
                    a.EffectiveStartDate <= nowUtc &&
                    (!a.EffectiveEndDate.HasValue || a.EffectiveEndDate.Value >= nowUtc))
                .Select(a => new { a.HospitalSiteId, a.OrganizationUnitId, a.DepartmentId })
                .ToListAsync(cancellationToken);

            // Selaras dengan layanan WorkforcePlanning lainnya: akun tanpa penempatan manajerial
            // (mis. admin SDM) memiliki cakupan penuh atas seluruh rumah sakit yang diizinkan oleh izin peran.
            if (managerialAssignments.Count == 0)
            {
                return null;
            }

            var scopedSiteIds = new HashSet<Guid>();
            var unitIdsToLookup = new List<Guid>();
            var departmentIdsToLookup = new List<Guid>();

            foreach (var assignment in managerialAssignments)
            {
                if (assignment.HospitalSiteId.HasValue && assignment.HospitalSiteId.Value != Guid.Empty)
                {
                    scopedSiteIds.Add(assignment.HospitalSiteId.Value);
                }

                if (assignment.OrganizationUnitId.HasValue && assignment.OrganizationUnitId.Value != Guid.Empty)
                {
                    unitIdsToLookup.Add(assignment.OrganizationUnitId.Value);
                }
                else if (assignment.DepartmentId != Guid.Empty)
                {
                    departmentIdsToLookup.Add(assignment.DepartmentId);
                }
            }

            if (unitIdsToLookup.Count > 0)
            {
                var distinctUnitIds = unitIdsToLookup.Distinct().ToList();
                var unitSiteIds = await _dbContext.MstOrganizationUnits
                    .AsNoTracking()
                    .Where(u => distinctUnitIds.Contains(u.Id) && !u.IsDelete && u.HospitalSiteId.HasValue)
                    .Select(u => u.HospitalSiteId!.Value)
                    .ToListAsync(cancellationToken);

                foreach (var siteId in unitSiteIds)
                {
                    scopedSiteIds.Add(siteId);
                }
            }

            if (departmentIdsToLookup.Count > 0)
            {
                var distinctDeptIds = departmentIdsToLookup.Distinct().ToList();
                var deptSiteIds = await _dbContext.MstOrganizationUnits
                    .AsNoTracking()
                    .Where(u => u.DepartmentId.HasValue && distinctDeptIds.Contains(u.DepartmentId.Value) && !u.IsDelete && u.HospitalSiteId.HasValue)
                    .Select(u => u.HospitalSiteId!.Value)
                    .ToListAsync(cancellationToken);

                foreach (var siteId in deptSiteIds)
                {
                    scopedSiteIds.Add(siteId);
                }
            }

            return scopedSiteIds;
        }

        private async Task<Dictionary<Guid, string>> ResolveUserDisplayNamesAsync(
            IReadOnlyCollection<Guid> userIds,
            CancellationToken cancellationToken)
        {
            if (userIds.Count == 0)
            {
                return new Dictionary<Guid, string>();
            }

            var users = await _dbContext.Users
                .AsNoTracking()
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.DisplayName, u.UserName, u.Email, u.UserCode })
                .ToListAsync(cancellationToken);

            var result = new Dictionary<Guid, string>(users.Count);
            foreach (var u in users)
            {
                var resolved = !string.IsNullOrWhiteSpace(u.DisplayName)
                    ? u.DisplayName.Trim()
                    : !string.IsNullOrWhiteSpace(u.UserName)
                        ? u.UserName.Trim()
                        : !string.IsNullOrWhiteSpace(u.Email)
                            ? u.Email.Trim()
                            : u.UserCode;

                if (!string.IsNullOrWhiteSpace(resolved))
                {
                    result[u.Id] = resolved;
                }
            }

            return result;
        }

        private static Guid? ResolveActorUserId(Guid? updateBy, Guid? createBy)
        {
            if (updateBy.HasValue && updateBy.Value != Guid.Empty)
            {
                return updateBy.Value;
            }

            if (createBy.HasValue && createBy.Value != Guid.Empty)
            {
                return createBy.Value;
            }

            return null;
        }

        private static bool TryNormalizeStatusFilter(string? rawStatus, out string? normalizedStatus)
        {
            if (string.IsNullOrWhiteSpace(rawStatus))
            {
                normalizedStatus = null;
                return true;
            }

            var compact = rawStatus.Trim().Replace(" ", string.Empty).Replace("_", string.Empty).Replace("-", string.Empty);
            if (string.Equals(compact, "Complete", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(compact, "Lengkap", StringComparison.OrdinalIgnoreCase))
            {
                normalizedStatus = WorkforcePlanningSettingStatuses.Complete;
                return true;
            }

            if (string.Equals(compact, "Incomplete", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(compact, "BelumLengkap", StringComparison.OrdinalIgnoreCase))
            {
                normalizedStatus = WorkforcePlanningSettingStatuses.Incomplete;
                return true;
            }

            if (string.Equals(compact, "NotConfigured", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(compact, "BelumDiatur", StringComparison.OrdinalIgnoreCase))
            {
                normalizedStatus = WorkforcePlanningSettingStatuses.NotConfigured;
                return true;
            }

            normalizedStatus = null;
            return false;
        }

        private static bool IsUniqueConstraintViolation(DbUpdateException exception)
        {
            var inner = exception.InnerException;
            if (inner == null)
            {
                return false;
            }

            var sqlState = inner.GetType().GetProperty("SqlState")?.GetValue(inner)?.ToString();
            if (string.Equals(sqlState, "23505", StringComparison.Ordinal))
            {
                return true;
            }

            var sqliteErrorCode = inner.GetType().GetProperty("SqliteErrorCode")?.GetValue(inner);
            if (sqliteErrorCode is int code && code == 19)
            {
                return true;
            }

            var message = inner.Message;
            return message.Contains("IX_HrdWorkforcePlanningSetting_Site", StringComparison.OrdinalIgnoreCase) ||
                   message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) ||
                   message.Contains("UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase);
        }
    }
}
