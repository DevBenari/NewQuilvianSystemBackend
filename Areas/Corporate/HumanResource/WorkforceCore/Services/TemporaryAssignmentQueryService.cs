using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services
{
    /// <summary>
    /// Daftar baca penempatan sementara lintas pegawai (HRD-DEC-240, desain 38.9.4, kontrak v30 API 36.6, INT-HR-117).
    ///
    /// Sumber: WfpOrganizationAssignment ber-AssignmentType Temporary, aktif, tidak dihapus. Baris tampil bila unit tujuan
    /// atau unit penempatan utama pegawai pada tanggal mulai berada dalam cakupan baca pengguna. Pencatatan dan perubahan
    /// tetap di Profil Pegawai 360 (WfpOrganizationAssignmentController).
    /// </summary>
    public class TemporaryAssignmentQueryService
    {
        public const string TemporaryAssignmentType = "Temporary";
        public const string Upcoming = "Upcoming";
        public const string Ongoing = "Ongoing";
        public const string Ended = "Ended";

        private const string ForbiddenUnitMessage = "Anda tidak berwenang melihat data unit ini";
        private const int MaxPageSize = 100;

        private static readonly (string Value, string Label)[] TimeStatuses =
        {
            (Upcoming, "Akan Datang"),
            (Ongoing, "Berlangsung"),
            (Ended, "Selesai")
        };

        private static readonly int[] PageSizes = { 10, 25, 50, 100 };

        private readonly ApplicationDbContext _context;
        private readonly HrdUnitScopeService _unitScopeService;

        public TemporaryAssignmentQueryService(ApplicationDbContext context, HrdUnitScopeService unitScopeService)
        {
            _context = context;
            _unitScopeService = unitScopeService;
        }

        public TemporaryAssignmentFilterMetadataResponse GetFilterMetadata() => new()
        {
            TimeStatusOptions = TimeStatuses.Select(x => new TemporaryAssignmentOptionResponse { Value = x.Value, Label = x.Label }).ToList(),
            PageSizeOptions = PageSizes.ToList()
        };

        public async Task<TemporaryAssignmentQueryResult<PagedResult<TemporaryAssignmentResponse>>> GetPagedAsync(
            TemporaryAssignmentPagedQuery query,
            CancellationToken cancellationToken = default)
        {
            var timeStatus = string.IsNullOrWhiteSpace(query.TimeStatus) ? null : query.TimeStatus.Trim();
            if (timeStatus != null && TimeStatuses.All(x => !string.Equals(x.Value, timeStatus, StringComparison.OrdinalIgnoreCase)))
            {
                return Fail(StatusCodes.Status400BadRequest, "Status waktu harus Upcoming, Ongoing, atau Ended.");
            }

            if (query.FromDate.HasValue && query.ToDate.HasValue && query.FromDate.Value > query.ToDate.Value)
            {
                return Fail(StatusCodes.Status400BadRequest, "Tanggal awal tidak boleh melewati tanggal akhir.");
            }

            // VR-PK-27: filter unit di luar cakupan baca ditolak, bukan dikosongkan diam-diam.
            var scope = await _unitScopeService.GetReadScopeAsync(cancellationToken);
            if (query.OrganizationUnitId.HasValue && !scope.Contains(query.OrganizationUnitId))
            {
                return Fail(StatusCodes.Status403Forbidden, ForbiddenUnitMessage);
            }

            var today = DateOnly.FromDateTime(AppDateTimeHelper.OperationalDate());
            var rows = BuildRows();

            if (!scope.IsAllUnits)
            {
                var unitIds = scope.UnitIds.Select(x => (Guid?)x).ToList();
                rows = rows.Where(x => unitIds.Contains(x.DestinationUnitId) || unitIds.Contains(x.OriginUnitId));
            }

            if (query.OrganizationUnitId.HasValue)
            {
                var unitId = query.OrganizationUnitId.Value;
                rows = rows.Where(x => x.DestinationUnitId == unitId || x.OriginUnitId == unitId);
            }

            // Tanggal efektif HR disimpan sebagai tanggal kalender pada tengah malam UTC.
            if (query.FromDate.HasValue)
            {
                var from = AtMidnightUtc(query.FromDate.Value);
                rows = rows.Where(x => x.EffectiveEndDate == null || x.EffectiveEndDate >= from);
            }

            if (query.ToDate.HasValue)
            {
                var to = AtMidnightUtc(query.ToDate.Value);
                rows = rows.Where(x => x.EffectiveStartDate <= to);
            }

            if (timeStatus != null)
            {
                var todayAt = AtMidnightUtc(today);
                rows = timeStatus.ToLowerInvariant() switch
                {
                    "upcoming" => rows.Where(x => x.EffectiveStartDate > todayAt),
                    "ended" => rows.Where(x => x.EffectiveEndDate != null && x.EffectiveEndDate < todayAt),
                    _ => rows.Where(x => x.EffectiveStartDate <= todayAt && (x.EffectiveEndDate == null || x.EffectiveEndDate >= todayAt))
                };
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var keyword = query.Search.Trim().ToLower();
                rows = rows.Where(x =>
                    x.EmployeeName.ToLower().Contains(keyword) ||
                    x.ProfileCode.ToLower().Contains(keyword) ||
                    (x.AssignmentNumber != null && x.AssignmentNumber.ToLower().Contains(keyword)));
            }

            var pageNumber = Math.Max(query.PageNumber, 1);
            var pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);
            var totalData = await rows.CountAsync(cancellationToken);
            var page = await rows
                .OrderByDescending(x => x.EffectiveStartDate)
                .ThenBy(x => x.EmployeeName)
                .ThenBy(x => x.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return TemporaryAssignmentQueryResult<PagedResult<TemporaryAssignmentResponse>>.Ok(
                new PagedResult<TemporaryAssignmentResponse>
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = page.Select(x => ToResponse(x, today)).ToList()
                },
                "Daftar penempatan sementara berhasil diambil.");
        }

        // Unit asal = penempatan utama (IsPrimary, bukan Temporary) yang berlaku pada tanggal mulai penempatan sementara;
        // bila ada lebih dari satu, yang mulainya paling akhir. Dihitung di basis data agar cakupan dan paginasi tetap di SQL.
        private IQueryable<TemporaryAssignmentRow> BuildRows()
        {
            var primaries = _context.WfpOrganizationAssignments.Where(p =>
                p.IsActive &&
                !p.IsDelete &&
                p.IsPrimary &&
                p.AssignmentType != TemporaryAssignmentType &&
                p.OrganizationUnitId != null);

            return _context.WfpOrganizationAssignments
                .AsNoTracking()
                .Where(x => x.AssignmentType == TemporaryAssignmentType && x.IsActive && !x.IsDelete)
                .Select(x => new TemporaryAssignmentRow
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    ProfileCode = x.WorkforceProfile != null ? x.WorkforceProfile.ProfileCode : string.Empty,
                    EmployeeName = x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : string.Empty,
                    DestinationUnitId = x.OrganizationUnitId,
                    DestinationUnitName = x.OrganizationUnit != null ? x.OrganizationUnit.UnitName : null,
                    OriginUnitId = primaries
                        .Where(p => p.WorkforceProfileId == x.WorkforceProfileId &&
                                    p.EffectiveStartDate <= x.EffectiveStartDate &&
                                    (p.EffectiveEndDate == null || p.EffectiveEndDate >= x.EffectiveStartDate))
                        .OrderByDescending(p => p.EffectiveStartDate)
                        .Select(p => p.OrganizationUnitId)
                        .FirstOrDefault(),
                    OriginUnitName = primaries
                        .Where(p => p.WorkforceProfileId == x.WorkforceProfileId &&
                                    p.EffectiveStartDate <= x.EffectiveStartDate &&
                                    (p.EffectiveEndDate == null || p.EffectiveEndDate >= x.EffectiveStartDate))
                        .OrderByDescending(p => p.EffectiveStartDate)
                        .Select(p => p.OrganizationUnit != null ? p.OrganizationUnit.UnitName : null)
                        .FirstOrDefault(),
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    AssignmentNumber = x.AssignmentNumber,
                    Description = x.Description
                });
        }

        private static TemporaryAssignmentResponse ToResponse(TemporaryAssignmentRow x, DateOnly today)
        {
            var start = DateOnly.FromDateTime(x.EffectiveStartDate);
            DateOnly? end = x.EffectiveEndDate.HasValue ? DateOnly.FromDateTime(x.EffectiveEndDate.Value) : null;
            var status = start > today ? Upcoming : end.HasValue && end.Value < today ? Ended : Ongoing;

            return new TemporaryAssignmentResponse
            {
                Id = x.Id,
                WorkforceProfileId = x.WorkforceProfileId,
                ProfileCode = x.ProfileCode,
                EmployeeName = x.EmployeeName,
                OriginUnitName = x.OriginUnitName,
                DestinationUnitId = x.DestinationUnitId,
                DestinationUnitName = x.DestinationUnitName,
                EffectiveStartDate = start,
                EffectiveEndDate = end,
                TimeStatus = status,
                TimeStatusLabel = TimeStatuses.Single(s => s.Value == status).Label,
                AssignmentNumber = x.AssignmentNumber,
                Description = x.Description
            };
        }

        private static DateTime AtMidnightUtc(DateOnly date) =>
            DateTime.SpecifyKind(date.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);

        private static TemporaryAssignmentQueryResult<PagedResult<TemporaryAssignmentResponse>> Fail(int statusCode, string message) =>
            TemporaryAssignmentQueryResult<PagedResult<TemporaryAssignmentResponse>>.Fail(statusCode, message);

        private sealed class TemporaryAssignmentRow
        {
            public Guid Id { get; init; }
            public Guid WorkforceProfileId { get; init; }
            public string ProfileCode { get; init; } = string.Empty;
            public string EmployeeName { get; init; } = string.Empty;
            public Guid? DestinationUnitId { get; init; }
            public string? DestinationUnitName { get; init; }
            public Guid? OriginUnitId { get; init; }
            public string? OriginUnitName { get; init; }
            public DateTime EffectiveStartDate { get; init; }
            public DateTime? EffectiveEndDate { get; init; }
            public string? AssignmentNumber { get; init; }
            public string? Description { get; init; }
        }
    }
}
