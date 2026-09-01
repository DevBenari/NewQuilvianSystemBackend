using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Services.Security
{
    /// <summary>
    /// Menurunkan proyeksi otorisasi dari penempatan organisasi yang otoritatif.
    ///
    /// <c>WfpOrganizationAssignment</c> adalah sumber kebenaran; <c>AspNetUserOrganization</c>
    /// hanyalah proyeksinya dan sejak Phase A0 hanya boleh ditulis lewat service ini.
    ///
    /// Sebelum A0 keduanya tidak terhubung. Penempatan yang ditambahkan lewat API HR resmi tidak
    /// pernah melahirkan izin, penempatan yang dinonaktifkan tidak pernah mencabutnya, dan
    /// perpindahan departemen hanya membalik <c>IsPrimary</c> sehingga izin departemen lama
    /// menempel selamanya bersama yang baru.
    ///
    /// Dua hal yang sengaja tidak dilakukan di sini:
    /// <list type="bullet">
    /// <item>Baris warisan yang sumbernya tidak dapat dibuktikan tidak ditutup dan tidak ditebak
    /// sumbernya. Ia dipertahankan apa adanya dan dilaporkan sebagai legacy-unresolved.</item>
    /// <item><c>EffectiveEndDate</c> historis tidak pernah dikarang. Penutupan memakai
    /// <c>IsActive=false</c>, sehingga tidak ada tanggal palsu yang masuk ke sejarah.</item>
    /// </list>
    /// </summary>
    public sealed class OrganizationAuthorizationProjectionService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<OrganizationAuthorizationProjectionService> _logger;

        public OrganizationAuthorizationProjectionService(
            ApplicationDbContext dbContext,
            ILogger<OrganizationAuthorizationProjectionService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public sealed class ProjectionResult
        {
            public int Created { get; set; }
            public int Reactivated { get; set; }
            public int Updated { get; set; }
            public int Closed { get; set; }
            public int AdoptedLegacy { get; set; }
            public List<string> LegacyUnresolved { get; } = new();
            public List<string> AmbiguousLegacy { get; } = new();
            public bool DryRun { get; set; }

            public void Add(ProjectionResult other)
            {
                Created += other.Created;
                Reactivated += other.Reactivated;
                Updated += other.Updated;
                Closed += other.Closed;
                AdoptedLegacy += other.AdoptedLegacy;
                LegacyUnresolved.AddRange(other.LegacyUnresolved);
                AmbiguousLegacy.AddRange(other.AmbiguousLegacy);
            }
        }

        /// <summary>Predikat kelayakan penempatan. Satu-satunya definisi yang dipakai A0.</summary>
        public static bool IsAssignmentValid(WfpOrganizationAssignment assignment, DateTime nowUtc) =>
            !assignment.IsDelete &&
            !assignment.IsCancel &&
            assignment.IsActive &&
            assignment.EffectiveStartDate <= nowUtc &&
            (!assignment.EffectiveEndDate.HasValue || assignment.EffectiveEndDate.Value >= nowUtc);

        public Task<ProjectionResult> ReconcileWorkforceProfileAsync(
            Guid workforceProfileId,
            Guid actorUserId,
            bool dryRun = false,
            CancellationToken cancellationToken = default) =>
            ReconcileCoreAsync(workforceProfileId, actorUserId, dryRun, cancellationToken);

        public async Task<ProjectionResult> ReconcileAllAsync(
            Guid actorUserId,
            bool dryRun = false,
            CancellationToken cancellationToken = default)
        {
            var profileIds = await _dbContext.Users
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId.HasValue)
                .Select(x => x.WorkforceProfileId!.Value)
                .Distinct()
                .ToListAsync(cancellationToken);

            var total = new ProjectionResult { DryRun = dryRun };

            foreach (var profileId in profileIds)
            {
                total.Add(await ReconcileCoreAsync(profileId, actorUserId, dryRun, cancellationToken));
            }

            // Baris proyeksi milik akun tanpa profil tenaga kerja sama sekali tidak punya sumber
            // otoritatif. Ia tetap dipertahankan dan hanya dilaporkan.
            var orphanRows = await (
                from projection in _dbContext.ApplicationUserOrganizations.AsNoTracking()
                join user in _dbContext.Users.AsNoTracking() on projection.UserId equals user.Id
                where projection.IsActive
                      && !projection.IsDelete
                      && projection.SourceAssignmentId == null
                      && user.WorkforceProfileId == null
                select new { projection.Id, projection.UserId })
                .ToListAsync(cancellationToken);

            foreach (var row in orphanRows)
            {
                total.LegacyUnresolved.Add(
                    $"AspNetUserOrganization {row.Id} (user {row.UserId}): akun tidak terhubung profil tenaga kerja.");
            }

            _logger.LogInformation(
                "Rekonsiliasi proyeksi otorisasi selesai. dryRun={DryRun} dibuat={Created} diaktifkan={Reactivated} " +
                "diperbarui={Updated} ditutup={Closed} warisan-diadopsi={Adopted} warisan-tak-terpetakan={Unresolved} ambigu={Ambiguous}",
                dryRun, total.Created, total.Reactivated, total.Updated, total.Closed,
                total.AdoptedLegacy, total.LegacyUnresolved.Count, total.AmbiguousLegacy.Count);

            return total;
        }

        private async Task<ProjectionResult> ReconcileCoreAsync(
            Guid workforceProfileId,
            Guid actorUserId,
            bool dryRun,
            CancellationToken cancellationToken)
        {
            var result = new ProjectionResult { DryRun = dryRun };
            var now = DateTime.UtcNow;

            var userIds = await _dbContext.Users
                .Where(x => x.WorkforceProfileId == workforceProfileId)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

            if (userIds.Count == 0)
            {
                return result;
            }

            var assignments = await _dbContext.WfpOrganizationAssignments
                .Where(x => x.WorkforceProfileId == workforceProfileId)
                .ToListAsync(cancellationToken);

            var validAssignments = assignments
                .Where(x => IsAssignmentValid(x, now))
                .ToList();

            foreach (var userId in userIds)
            {
                var projections = await _dbContext.ApplicationUserOrganizations
                    .Where(x => x.UserId == userId)
                    .ToListAsync(cancellationToken);

                var bySource = projections
                    .Where(x => x.SourceAssignmentId.HasValue)
                    .GroupBy(x => x.SourceAssignmentId!.Value)
                    .ToDictionary(g => g.Key, g => g.First());

                var matchedSourceIds = new HashSet<Guid>();

                // Pada mode dry-run adopsi tidak dipersist, jadi baris yang sudah diputuskan
                // teradopsi dicatat di sini agar tidak ikut dilaporkan sebagai legacy-unresolved.
                var adoptedIds = new HashSet<Guid>();

                foreach (var assignment in validAssignments)
                {
                    matchedSourceIds.Add(assignment.Id);

                    if (bySource.TryGetValue(assignment.Id, out var existing))
                    {
                        var wasClosed = existing.IsDelete || !existing.IsActive;

                        if (!dryRun)
                        {
                            // Dihidupkan kembali hanya karena sumber otoritatifnya memang sah lagi,
                            // bukan karena ada update pada data pegawai.
                            existing.IsDelete = false;
                            existing.DeleteDateTime = null;
                            existing.DeleteBy = Guid.Empty;
                            existing.IsCancel = false;
                            existing.IsActive = true;
                            ApplyAssignment(existing, assignment, actorUserId, now);
                        }

                        if (wasClosed) result.Reactivated++; else result.Updated++;
                        continue;
                    }

                    // Baris warisan tanpa SourceAssignmentId boleh diadopsi hanya bila
                    // pemetaannya tunggal. Bila ada lebih dari satu kandidat, tidak ditebak.
                    var legacyCandidates = projections
                        .Where(x => x.SourceAssignmentId == null &&
                                    !x.IsDelete &&
                                    x.DepartmentId == assignment.DepartmentId &&
                                    x.PositionId == assignment.PositionId)
                        .ToList();

                    var assignmentsForSamePlacement = validAssignments
                        .Count(x => x.DepartmentId == assignment.DepartmentId &&
                                    x.PositionId == assignment.PositionId);

                    if (legacyCandidates.Count == 1 && assignmentsForSamePlacement == 1)
                    {
                        var adopted = legacyCandidates[0];

                        if (!dryRun)
                        {
                            adopted.SourceAssignmentId = assignment.Id;
                            adopted.IsActive = true;
                            ApplyAssignment(adopted, assignment, actorUserId, now);
                        }

                        bySource[assignment.Id] = adopted;


                        adoptedIds.Add(adopted.Id);
                        result.AdoptedLegacy++;
                        continue;
                    }

                    if (legacyCandidates.Count > 1 || assignmentsForSamePlacement > 1)
                    {
                        result.AmbiguousLegacy.Add(
                            $"user {userId} Departemen {assignment.DepartmentId} Posisi {assignment.PositionId}: " +
                            $"{legacyCandidates.Count} baris warisan untuk {assignmentsForSamePlacement} penempatan sah. " +
                            "SourceAssignmentId dibiarkan kosong.");
                    }

                    if (!dryRun)
                    {
                        var created = new ApplicationUserOrganization
                        {
                            Id = Guid.NewGuid(),
                            UserId = userId,
                            SourceAssignmentId = assignment.Id,
                            IsActive = true,
                            CreateDateTime = now,
                            CreateBy = actorUserId,
                            IsDelete = false,
                            IsCancel = false
                        };

                        ApplyAssignment(created, assignment, actorUserId, now);
                        _dbContext.ApplicationUserOrganizations.Add(created);
                    }

                    result.Created++;
                }

                // Proyeksi yang sumbernya sudah tidak sah lagi ditutup. EffectiveEndDate tidak
                // dikarang: penutupan cukup lewat IsActive.
                foreach (var projection in projections)
                {
                    if (!projection.SourceAssignmentId.HasValue) continue;
                    if (matchedSourceIds.Contains(projection.SourceAssignmentId.Value)) continue;
                    if (!projection.IsActive || projection.IsDelete) continue;

                    if (!dryRun)
                    {
                        projection.IsActive = false;
                        projection.IsPrimary = false;
                        projection.UpdateDateTime = now;
                        projection.UpdateBy = actorUserId;
                    }

                    result.Closed++;
                }

                foreach (var projection in projections)
                {
                    if (projection.SourceAssignmentId.HasValue) continue;
                    if (adoptedIds.Contains(projection.Id)) continue;
                    if (!projection.IsActive || projection.IsDelete) continue;

                    result.LegacyUnresolved.Add(
                        $"AspNetUserOrganization {projection.Id} (user {userId}, Departemen {projection.DepartmentId}, " +
                        $"Posisi {projection.PositionId}): tidak ada penempatan otoritatif yang cocok. Dipertahankan.");
                }
            }

            if (!dryRun)
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            return result;
        }

        private static void ApplyAssignment(
            ApplicationUserOrganization projection,
            WfpOrganizationAssignment assignment,
            Guid actorUserId,
            DateTime now)
        {
            projection.DepartmentId = assignment.DepartmentId;
            projection.PositionId = assignment.PositionId;
            projection.IsPrimary = assignment.IsPrimary;
            projection.EffectiveStartDate = DateTime.SpecifyKind(assignment.EffectiveStartDate, DateTimeKind.Utc);
            projection.EffectiveEndDate = assignment.EffectiveEndDate.HasValue
                ? DateTime.SpecifyKind(assignment.EffectiveEndDate.Value, DateTimeKind.Utc)
                : null;
            projection.Description = $"Diproyeksikan dari WfpOrganizationAssignment ({assignment.AssignmentType})";
            projection.UpdateDateTime = now;
            projection.UpdateBy = actorUserId;
        }
    }
}
