using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Tests.Security;

/// <summary>
/// Proyeksi otorisasi dari penempatan organisasi otoritatif (Phase A0).
///
/// Audit menemukan bahwa <c>WfpOrganizationAssignment</c> dan <c>AspNetUserOrganization</c> sama
/// sekali tidak terhubung: menambah penempatan lewat API HR resmi tidak pernah melahirkan izin,
/// menonaktifkannya tidak pernah mencabut izin, dan perpindahan departemen hanya membalik
/// <c>IsPrimary</c> sehingga izin departemen lama menempel selamanya.
///
/// Catatan keterbatasan: provider InMemory tidak menegakkan unique index, jadi test di sini
/// membuktikan perilaku service, bukan perilaku index. Index diverifikasi lewat migration dan
/// pemeriksaan skema pada database.
/// </summary>
public sealed class OrganizationAuthorizationProjectionTests
{
    private static readonly Guid Actor = Guid.NewGuid();

    private static ApplicationDbContext NewContext() =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"projection-{Guid.NewGuid():N}")
            .Options);

    private static OrganizationAuthorizationProjectionService NewService(ApplicationDbContext db) =>
        new(db, NullLogger<OrganizationAuthorizationProjectionService>.Instance);

    private static async Task<(Guid UserId, Guid ProfileId)> SeedUserAsync(ApplicationDbContext db)
    {
        var profileId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = $"user-{Guid.NewGuid():N}",
            NormalizedUserName = $"USER-{Guid.NewGuid():N}",
            UserCode = "PRJ-USER",
            DisplayName = "Projection User",
            UserType = UserType.Employee,
            IsActive = true,
            WorkforceProfileId = profileId,
            SecurityStamp = Guid.NewGuid().ToString("N")
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();
        return (user.Id, profileId);
    }

    private static WfpOrganizationAssignment Assignment(
        Guid profileId,
        Guid departmentId,
        Guid positionId,
        string assignmentType = "Primary",
        bool isPrimary = false,
        bool isActive = true,
        bool isDelete = false,
        bool isCancel = false,
        int startDaysOffset = -30,
        int? endDaysOffset = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            WorkforceProfileId = profileId,
            DepartmentId = departmentId,
            PositionId = positionId,
            AssignmentType = assignmentType,
            IsPrimary = isPrimary,
            IsActive = isActive,
            IsDelete = isDelete,
            IsCancel = isCancel,
            EffectiveStartDate = DateTime.UtcNow.AddDays(startDaysOffset),
            EffectiveEndDate = endDaysOffset.HasValue ? DateTime.UtcNow.AddDays(endDaysOffset.Value) : null
        };

    private static Task<List<ApplicationUserOrganization>> ActiveProjectionsAsync(
        ApplicationDbContext db, Guid userId) =>
        db.ApplicationUserOrganizations
            .Where(x => x.UserId == userId && x.IsActive && !x.IsDelete)
            .ToListAsync();

    // ---------------------------------------------------------------- 6, 7, 8

    [Fact]
    public async Task SingleAssignmentProducesOneProjection()
    {
        await using var db = NewContext();
        var (userId, profileId) = await SeedUserAsync(db);
        var assignment = Assignment(profileId, Guid.NewGuid(), Guid.NewGuid(), isPrimary: true);
        db.WfpOrganizationAssignments.Add(assignment);
        await db.SaveChangesAsync();

        await NewService(db).ReconcileWorkforceProfileAsync(profileId, Actor);

        var projections = await ActiveProjectionsAsync(db, userId);
        Assert.Single(projections);
        Assert.Equal(assignment.Id, projections[0].SourceAssignmentId);
        Assert.Equal(assignment.DepartmentId, projections[0].DepartmentId);
    }

    [Fact]
    public async Task MultipleActiveAssignmentsAllProject_UnionSemantics()
    {
        await using var db = NewContext();
        var (userId, profileId) = await SeedUserAsync(db);

        db.WfpOrganizationAssignments.AddRange(
            Assignment(profileId, Guid.NewGuid(), Guid.NewGuid(), "Primary", isPrimary: true),
            Assignment(profileId, Guid.NewGuid(), Guid.NewGuid(), "Secondary"));
        await db.SaveChangesAsync();

        await NewService(db).ReconcileWorkforceProfileAsync(profileId, Actor);

        var projections = await ActiveProjectionsAsync(db, userId);
        Assert.Equal(2, projections.Count);
        Assert.Equal(2, projections.Select(x => x.SourceAssignmentId).Distinct().Count());
    }

    /// <summary>Test 9 sampai 13: seluruh AssignmentType ikut, tipe bukan filter otorisasi.</summary>
    [Theory]
    [InlineData("Primary")]
    [InlineData("Secondary")]
    [InlineData("Acting")]
    [InlineData("Temporary")]
    [InlineData("Project")]
    [InlineData("Functional")]
    public async Task EveryAssignmentTypeParticipates(string assignmentType)
    {
        await using var db = NewContext();
        var (userId, profileId) = await SeedUserAsync(db);
        db.WfpOrganizationAssignments.Add(
            Assignment(profileId, Guid.NewGuid(), Guid.NewGuid(), assignmentType));
        await db.SaveChangesAsync();

        await NewService(db).ReconcileWorkforceProfileAsync(profileId, Actor);

        Assert.Single(await ActiveProjectionsAsync(db, userId));
    }

    /// <summary>Test 14: IsPrimary=false tidak dengan sendirinya mencabut akses yang sah.</summary>
    [Fact]
    public async Task NonPrimaryAssignmentStillGrantsProjection()
    {
        await using var db = NewContext();
        var (userId, profileId) = await SeedUserAsync(db);
        db.WfpOrganizationAssignments.Add(
            Assignment(profileId, Guid.NewGuid(), Guid.NewGuid(), "Secondary", isPrimary: false));
        await db.SaveChangesAsync();

        await NewService(db).ReconcileWorkforceProfileAsync(profileId, Actor);

        var projections = await ActiveProjectionsAsync(db, userId);
        Assert.Single(projections);
        Assert.False(projections[0].IsPrimary);
    }

    // ---------------------------------------------------------------- 15 sampai 19

    [Theory]
    [InlineData("cancelled")]
    [InlineData("deleted")]
    [InlineData("inactive")]
    [InlineData("future")]
    [InlineData("expired")]
    public async Task InvalidAssignmentNeverProjects(string flavour)
    {
        await using var db = NewContext();
        var (userId, profileId) = await SeedUserAsync(db);

        var assignment = flavour switch
        {
            "cancelled" => Assignment(profileId, Guid.NewGuid(), Guid.NewGuid(), isCancel: true),
            "deleted" => Assignment(profileId, Guid.NewGuid(), Guid.NewGuid(), isDelete: true),
            "inactive" => Assignment(profileId, Guid.NewGuid(), Guid.NewGuid(), isActive: false),
            "future" => Assignment(profileId, Guid.NewGuid(), Guid.NewGuid(), startDaysOffset: 30),
            "expired" => Assignment(profileId, Guid.NewGuid(), Guid.NewGuid(), startDaysOffset: -60, endDaysOffset: -10),
            _ => throw new ArgumentOutOfRangeException(nameof(flavour))
        };

        db.WfpOrganizationAssignments.Add(assignment);
        await db.SaveChangesAsync();

        await NewService(db).ReconcileWorkforceProfileAsync(profileId, Actor);

        Assert.Empty(await ActiveProjectionsAsync(db, userId));
    }

    // ---------------------------------------------------------------- 20, 21, 22

    /// <summary>Test 20: perpindahan departemen mencabut proyeksi lama, bukan menumpuknya.</summary>
    [Fact]
    public async Task DepartmentTransferRevokesStaleProjection()
    {
        await using var db = NewContext();
        var (userId, profileId) = await SeedUserAsync(db);

        var oldAssignment = Assignment(profileId, Guid.NewGuid(), Guid.NewGuid(), isPrimary: true);
        db.WfpOrganizationAssignments.Add(oldAssignment);
        await db.SaveChangesAsync();
        await NewService(db).ReconcileWorkforceProfileAsync(profileId, Actor);

        Assert.Single(await ActiveProjectionsAsync(db, userId));

        // Pindah departemen: penempatan lama ditutup HR, penempatan baru dibuat.
        oldAssignment.IsActive = false;
        var newAssignment = Assignment(profileId, Guid.NewGuid(), Guid.NewGuid(), isPrimary: true);
        db.WfpOrganizationAssignments.Add(newAssignment);
        await db.SaveChangesAsync();

        await NewService(db).ReconcileWorkforceProfileAsync(profileId, Actor);

        var projections = await ActiveProjectionsAsync(db, userId);
        Assert.Single(projections);
        Assert.Equal(newAssignment.Id, projections[0].SourceAssignmentId);
    }

    /// <summary>Test 21: mutasi lewat jalur resmi memperbarui proyeksi otorisasi.</summary>
    [Fact]
    public async Task DeactivatingAssignmentRevokesProjection()
    {
        await using var db = NewContext();
        var (userId, profileId) = await SeedUserAsync(db);

        var assignment = Assignment(profileId, Guid.NewGuid(), Guid.NewGuid(), "Acting");
        db.WfpOrganizationAssignments.Add(assignment);
        await db.SaveChangesAsync();
        await NewService(db).ReconcileWorkforceProfileAsync(profileId, Actor);
        Assert.Single(await ActiveProjectionsAsync(db, userId));

        assignment.IsActive = false;
        await db.SaveChangesAsync();
        await NewService(db).ReconcileWorkforceProfileAsync(profileId, Actor);

        Assert.Empty(await ActiveProjectionsAsync(db, userId));
    }

    /// <summary>Test 22: proyeksi tidak dihidupkan kembali oleh penempatan yang tidak sah.</summary>
    [Fact]
    public async Task ReconcileDoesNotResurrectInvalidAssignment()
    {
        await using var db = NewContext();
        var (userId, profileId) = await SeedUserAsync(db);

        var assignment = Assignment(profileId, Guid.NewGuid(), Guid.NewGuid());
        db.WfpOrganizationAssignments.Add(assignment);
        await db.SaveChangesAsync();
        await NewService(db).ReconcileWorkforceProfileAsync(profileId, Actor);

        assignment.IsCancel = true;
        await db.SaveChangesAsync();
        await NewService(db).ReconcileWorkforceProfileAsync(profileId, Actor);
        Assert.Empty(await ActiveProjectionsAsync(db, userId));

        // Dijalankan berkali-kali pun tidak boleh menghidupkannya lagi.
        await NewService(db).ReconcileWorkforceProfileAsync(profileId, Actor);
        Assert.Empty(await ActiveProjectionsAsync(db, userId));
    }

    /// <summary>Penutupan tidak pernah mengarang EffectiveEndDate historis.</summary>
    [Fact]
    public async Task ClosingProjectionDoesNotInventEffectiveEndDate()
    {
        await using var db = NewContext();
        var (userId, profileId) = await SeedUserAsync(db);

        var assignment = Assignment(profileId, Guid.NewGuid(), Guid.NewGuid());
        db.WfpOrganizationAssignments.Add(assignment);
        await db.SaveChangesAsync();
        await NewService(db).ReconcileWorkforceProfileAsync(profileId, Actor);

        assignment.IsActive = false;
        await db.SaveChangesAsync();
        await NewService(db).ReconcileWorkforceProfileAsync(profileId, Actor);

        var closed = await db.ApplicationUserOrganizations.SingleAsync(x => x.UserId == userId);
        Assert.False(closed.IsActive);
        Assert.Null(closed.EffectiveEndDate);
        Assert.False(closed.IsDelete);
    }

    // ---------------------------------------------------------------- legacy

    /// <summary>
    /// Baris warisan yang sumbernya dapat dibuktikan tunggal diadopsi; SourceAssignmentId diisi
    /// dari sumber yang benar, bukan ditebak.
    /// </summary>
    [Fact]
    public async Task LegacyRowWithProvableSourceIsAdopted()
    {
        await using var db = NewContext();
        var (userId, profileId) = await SeedUserAsync(db);

        var departmentId = Guid.NewGuid();
        var positionId = Guid.NewGuid();
        var assignment = Assignment(profileId, departmentId, positionId, isPrimary: true);
        db.WfpOrganizationAssignments.Add(assignment);

        var legacy = new ApplicationUserOrganization
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            DepartmentId = departmentId,
            PositionId = positionId,
            IsActive = true,
            IsDelete = false,
            SourceAssignmentId = null
        };
        db.ApplicationUserOrganizations.Add(legacy);
        await db.SaveChangesAsync();

        var result = await NewService(db).ReconcileWorkforceProfileAsync(profileId, Actor);

        Assert.Equal(1, result.AdoptedLegacy);
        Assert.Equal(0, result.Created);

        var adopted = await db.ApplicationUserOrganizations.SingleAsync(x => x.Id == legacy.Id);
        Assert.Equal(assignment.Id, adopted.SourceAssignmentId);
    }

    /// <summary>
    /// Baris warisan tanpa sumber yang dapat dibuktikan dipertahankan apa adanya, tidak ditutup,
    /// dan SourceAssignmentId-nya tidak dikarang.
    /// </summary>
    [Fact]
    public async Task LegacyRowWithoutProvableSourceIsPreservedAndReported()
    {
        await using var db = NewContext();
        var (userId, profileId) = await SeedUserAsync(db);

        var legacy = new ApplicationUserOrganization
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            DepartmentId = Guid.NewGuid(),
            PositionId = Guid.NewGuid(),
            IsActive = true,
            IsDelete = false,
            SourceAssignmentId = null
        };
        db.ApplicationUserOrganizations.Add(legacy);
        await db.SaveChangesAsync();

        var result = await NewService(db).ReconcileWorkforceProfileAsync(profileId, Actor);

        Assert.Single(result.LegacyUnresolved);
        Assert.Equal(0, result.Closed);

        var preserved = await db.ApplicationUserOrganizations.SingleAsync(x => x.Id == legacy.Id);
        Assert.True(preserved.IsActive);
        Assert.False(preserved.IsDelete);
        Assert.Null(preserved.SourceAssignmentId);
    }

    /// <summary>Mode dry-run melaporkan selisih tanpa menulis apa pun.</summary>
    [Fact]
    public async Task DryRunReportsWithoutWriting()
    {
        await using var db = NewContext();
        var (userId, profileId) = await SeedUserAsync(db);
        db.WfpOrganizationAssignments.Add(Assignment(profileId, Guid.NewGuid(), Guid.NewGuid()));
        await db.SaveChangesAsync();

        var result = await NewService(db).ReconcileWorkforceProfileAsync(profileId, Actor, dryRun: true);

        Assert.Equal(1, result.Created);
        Assert.Empty(await ActiveProjectionsAsync(db, userId));
    }
}
