using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services
{
    /// <summary>
    /// Cakupan unit organisasi pengguna yang sedang login (HRD-DEC-194, 206, 208; desain A52 35.9.1).
    ///
    /// Cakupan kelola = unit tempat pengguna memegang WfpOrganizationAssignment berjabatan
    /// (IsManagerialAssignment) yang berlaku hari ini. Cakupan baca = cakupan kelola beserta
    /// seluruh unit turunannya. Penanda RosterPeriod : ManageAllUnits memberi cakupan kelola semua
    /// unit; penanda AttendanceReport : ReadAllUnits memberi cakupan baca semua unit.
    ///
    /// Gagal tertutup: tanpa login, tanpa profil pegawai, atau tanpa penempatan berjabatan, cakupan
    /// kosong. Data yang tidak bertautan unit tidak termasuk cakupan mana pun selain semua unit.
    /// </summary>
    public class HrdUnitScopeService
    {
        public const string ManageAllUnitsResource = "RosterPeriod";
        public const string ManageAllUnitsAction = "ManageAllUnits";
        public const string ReadAllUnitsResource = "AttendanceReport";
        public const string ReadAllUnitsAction = "ReadAllUnits";

        private readonly ApplicationDbContext _context;
        private readonly AccessPermissionService _accessPermissionService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private HrdUnitScope? _manageScope;
        private HrdUnitScope? _readScope;

        public HrdUnitScopeService(
            ApplicationDbContext context,
            AccessPermissionService accessPermissionService,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _accessPermissionService = accessPermissionService ?? throw new ArgumentNullException(nameof(accessPermissionService));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public async Task<HrdUnitScope> GetManageScopeAsync(CancellationToken cancellationToken = default)
        {
            if (_manageScope != null)
            {
                return _manageScope;
            }

            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                return _manageScope = HrdUnitScope.Empty;
            }

            if (await _accessPermissionService.HasAccessAsync(user, ManageAllUnitsResource, ManageAllUnitsAction))
            {
                return _manageScope = HrdUnitScope.AllUnits;
            }

            var unitIds = await GetManagerialUnitIdsAsync(user, cancellationToken);
            return _manageScope = HrdUnitScope.ForUnits(unitIds);
        }

        public async Task<HrdUnitScope> GetReadScopeAsync(CancellationToken cancellationToken = default)
        {
            if (_readScope != null)
            {
                return _readScope;
            }

            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                return _readScope = HrdUnitScope.Empty;
            }

            // Penanda ReadAllUnits baru terdaftar bersama AttendanceReportController (BE-HRD-349).
            // Sebelum itu pemeriksaan ini hanya bernilai benar bagi superadmin.
            if (await _accessPermissionService.HasAccessAsync(user, ReadAllUnitsResource, ReadAllUnitsAction))
            {
                return _readScope = HrdUnitScope.AllUnits;
            }

            var manageScope = await GetManageScopeAsync(cancellationToken);
            if (manageScope.IsAllUnits)
            {
                return _readScope = HrdUnitScope.AllUnits;
            }

            if (manageScope.UnitIds.Count == 0)
            {
                return _readScope = HrdUnitScope.Empty;
            }

            var units = await _context.MstOrganizationUnits
                .AsNoTracking()
                .Where(x => !x.IsDelete)
                .Select(x => new { x.Id, x.ParentOrganizationUnitId })
                .ToListAsync(cancellationToken);

            var childrenByParent = units
                .Where(x => x.ParentOrganizationUnitId.HasValue)
                .GroupBy(x => x.ParentOrganizationUnitId!.Value)
                .ToDictionary(x => x.Key, x => x.Select(c => c.Id).ToList());

            var readable = new HashSet<Guid>(manageScope.UnitIds);
            var pending = new Queue<Guid>(manageScope.UnitIds);
            while (pending.Count > 0)
            {
                var parentId = pending.Dequeue();
                if (!childrenByParent.TryGetValue(parentId, out var children))
                {
                    continue;
                }

                foreach (var childId in children)
                {
                    // HashSet sekaligus menjaga dari data induk yang melingkar.
                    if (readable.Add(childId))
                    {
                        pending.Enqueue(childId);
                    }
                }
            }

            return _readScope = HrdUnitScope.ForUnits(readable);
        }

        private async Task<IReadOnlyCollection<Guid>> GetManagerialUnitIdsAsync(
            ClaimsPrincipal user,
            CancellationToken cancellationToken)
        {
            var userIdText = user.FindFirstValue("user_id") ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdText, out var userId) || userId == Guid.Empty)
            {
                return Array.Empty<Guid>();
            }

            var workforceProfileId = await _context.Users
                .AsNoTracking()
                .Where(x => x.Id == userId && x.IsActive)
                .Select(x => x.WorkforceProfileId)
                .FirstOrDefaultAsync(cancellationToken);

            if (!workforceProfileId.HasValue)
            {
                return Array.Empty<Guid>();
            }

            // Tanggal efektif HR disimpan sebagai tanggal kalender pada tengah malam UTC; dibandingkan
            // dengan tanggal operasional WIB hari ini.
            var today = DateTime.SpecifyKind(AppDateTimeHelper.OperationalDate().Date, DateTimeKind.Utc);

            return await _context.WfpOrganizationAssignments
                .AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId.Value &&
                    x.IsManagerialAssignment &&
                    x.IsActive &&
                    !x.IsDelete &&
                    x.OrganizationUnitId.HasValue &&
                    x.EffectiveStartDate <= today &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= today))
                .Select(x => x.OrganizationUnitId!.Value)
                .Distinct()
                .ToListAsync(cancellationToken);
        }
    }

    public sealed class HrdUnitScope
    {
        public static readonly HrdUnitScope Empty = new(false, new HashSet<Guid>());
        public static readonly HrdUnitScope AllUnits = new(true, new HashSet<Guid>());

        private HrdUnitScope(bool isAllUnits, IReadOnlySet<Guid> unitIds)
        {
            IsAllUnits = isAllUnits;
            UnitIds = unitIds;
        }

        public bool IsAllUnits { get; }

        /// <summary>Kosong bila <see cref="IsAllUnits"/> bernilai benar.</summary>
        public IReadOnlySet<Guid> UnitIds { get; }

        public static HrdUnitScope ForUnits(IEnumerable<Guid> unitIds) =>
            new(false, new HashSet<Guid>(unitIds));

        /// <summary>
        /// Benar bila unit termasuk cakupan. Data tanpa unit hanya termasuk cakupan semua unit.
        /// </summary>
        public bool Contains(Guid? organizationUnitId) =>
            IsAllUnits || (organizationUnitId.HasValue && UnitIds.Contains(organizationUnitId.Value));
    }
}
