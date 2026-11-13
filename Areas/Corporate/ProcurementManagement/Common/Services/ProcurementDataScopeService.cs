using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Services
{
    /// <summary>
    /// Cakupan unit yang boleh dilihat seorang pengguna pada satu jenis dokumen Pengadaan.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Bukan izin aksi, melainkan penyaring data. Contoh: pengguna IGD yang memegang
    /// <c>ProcurementRequest : Read</c> hanya melihat PR milik IGD. Pengguna yang juga memegang
    /// <c>ProcurementRequest : ReadAllUnits</c> (mis. Tim Pengadaan) melihat PR semua unit.
    /// </para>
    /// <para>
    /// "Unit" adalah <c>MstDepartment</c>, diambil dari penempatan pengguna
    /// (<c>AspNetUserOrganization</c>). Kelayakan penempatan disamakan persis dengan jalur
    /// otorisasi <see cref="AccessPermissionService"/>: aktif, tidak dihapus, tidak dibatalkan,
    /// dan masa berlakunya mencakup saat ini. Penempatan sekunder ikut dihitung. Pengguna tanpa
    /// penempatan yang sah mendapat cakupan kosong, sehingga tidak melihat dokumen unit mana pun.
    /// </para>
    /// <para>
    /// Aksi lintas unit ditanyakan per resource dokumen (PBJ-DEC-036), sehingga Keuangan dapat
    /// diberi semua pagu tanpa melihat semua PR.
    /// </para>
    /// </remarks>
    public sealed class ProcurementDataScopeService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly AccessPermissionService _accessPermissionService;

        public ProcurementDataScopeService(
            ApplicationDbContext dbContext,
            AccessPermissionService accessPermissionService)
        {
            _dbContext = dbContext;
            _accessPermissionService = accessPermissionService;
        }

        /// <summary>
        /// Menentukan cakupan pengguna untuk satu resource dokumen, misalnya
        /// <see cref="ProcurementPermissions.PurchaseRequestResource"/>.
        /// </summary>
        public async Task<ProcurementDataScope> ResolveAsync(
            ClaimsPrincipal user,
            string documentResource,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(documentResource))
            {
                throw new ArgumentException("Resource dokumen wajib diisi.", nameof(documentResource));
            }

            if (user.Identity?.IsAuthenticated != true || !TryGetUserId(user, out var userId))
            {
                return ProcurementDataScope.None;
            }

            if (await _accessPermissionService.HasAccessAsync(
                    user,
                    documentResource,
                    ProcurementPermissions.ReadAllUnitsAction))
            {
                return ProcurementDataScope.AllUnits(userId);
            }

            var departmentIds = await GetPlacementDepartmentIdsAsync(userId, cancellationToken);
            return ProcurementDataScope.Departments(userId, departmentIds);
        }

        /// <summary>
        /// Departemen tempat pengguna punya penempatan yang sah hari ini, tanpa memandang izin lintas
        /// unit. Dipakai untuk aturan "pengusul harus ditempatkan di unit dokumen" (PBJ-VAL-010):
        /// Tim Pengadaan boleh <b>melihat</b> PR semua unit, tetapi hanya boleh <b>membuat</b> PR untuk
        /// unit penempatannya sendiri.
        /// </summary>
        public async Task<IReadOnlyList<Guid>> GetPlacementDepartmentIdsAsync(Guid userId, CancellationToken cancellationToken)
        {
            if (userId == Guid.Empty)
            {
                return Array.Empty<Guid>();
            }

            var now = DateTime.UtcNow;

            return await _dbContext.ApplicationUserOrganizations
                .AsNoTracking()
                .Where(x =>
                    x.UserId == userId &&
                    x.IsActive &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value <= now) &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= now))
                .Select(x => x.DepartmentId)
                .Distinct()
                .ToListAsync(cancellationToken);
        }

        /// <summary>Membaca ID pengguna dari klaim; <see cref="Guid.Empty"/> bila tidak ada.</summary>
        public static Guid GetUserId(ClaimsPrincipal user) =>
            TryGetUserId(user, out var userId) ? userId : Guid.Empty;

        private static bool TryGetUserId(ClaimsPrincipal user, out Guid userId)
        {
            var userIdText =
                user.FindFirstValue("user_id") ??
                user.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userIdText, out userId) && userId != Guid.Empty;
        }
    }

    /// <summary>
    /// Hasil <see cref="ProcurementDataScopeService.ResolveAsync"/>. Dipakai service dokumen
    /// untuk menyaring query daftar dan menjawab akses detail (di luar cakupan → 404).
    /// </summary>
    public sealed class ProcurementDataScope
    {
        private ProcurementDataScope(Guid userId, bool isAllUnits, IReadOnlySet<Guid> departmentIds)
        {
            UserId = userId;
            IsAllUnits = isAllUnits;
            DepartmentIds = departmentIds;
        }

        /// <summary>Pengguna tidak terautentikasi atau tidak dikenali: tidak melihat apa pun.</summary>
        public static ProcurementDataScope None { get; } =
            new(Guid.Empty, false, new HashSet<Guid>());

        public static ProcurementDataScope AllUnits(Guid userId) =>
            new(userId, true, new HashSet<Guid>());

        public static ProcurementDataScope Departments(Guid userId, IEnumerable<Guid> departmentIds) =>
            new(userId, false, new HashSet<Guid>(departmentIds.Where(x => x != Guid.Empty)));

        public Guid UserId { get; }

        public bool IsAllUnits { get; }

        /// <summary>Departemen yang boleh dilihat bila <see cref="IsAllUnits"/> bernilai false.</summary>
        public IReadOnlySet<Guid> DepartmentIds { get; }

        /// <summary>Tidak ada unit yang boleh dilihat.</summary>
        public bool IsEmpty => !IsAllUnits && DepartmentIds.Count == 0;

        public bool CanSee(Guid departmentId) =>
            IsAllUnits || DepartmentIds.Contains(departmentId);
    }
}
