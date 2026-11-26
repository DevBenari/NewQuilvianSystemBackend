using System.Diagnostics;
using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Controllers;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Tests.HumanResource;

// BE-HRD-FIN-WFP-005: Pengujian lengkap Pengaturan Perencanaan SDM (daftar LEFT JOIN, status kelengkapan,
// validasi input, concurrency RowVersion, duplikasi unik, otorisasi Read/Update, dan cakupan rumah sakit).
public sealed class WorkforcePlanningSettingTests : IDisposable
{
    private readonly LocalSqliteTestDatabase _db = LocalSqliteTestDatabase.Create();

    private sealed class LocalSqliteModelCustomizer(ModelCustomizerDependencies dependencies) : RelationalModelCustomizer(dependencies)
    {
        private static readonly string[] PostgresOnlyTokens = ["num_nonnulls", "btrim", "::", " ~ ", " ~* ", " !~ ", " !~* "];

        public override void Customize(ModelBuilder modelBuilder, DbContext context)
        {
            base.Customize(modelBuilder, context);
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var constraintName in entityType.GetDeclaredCheckConstraints()
                    .Where(x => x.Sql != null && PostgresOnlyTokens.Any(t => x.Sql.Contains(t, StringComparison.OrdinalIgnoreCase)))
                    .Select(x => x.ModelName)
                    .ToList())
                {
                    entityType.RemoveCheckConstraint(constraintName);
                }

                foreach (var prop in entityType.GetDeclaredProperties())
                {
                    var defSql = prop.GetDefaultValueSql();
                    if (defSql != null && PostgresOnlyTokens.Any(t => defSql.Contains(t, StringComparison.OrdinalIgnoreCase)))
                        prop.SetDefaultValueSql(null);

                    var compSql = prop.GetComputedColumnSql();
                    if (compSql != null && PostgresOnlyTokens.Any(t => compSql.Contains(t, StringComparison.OrdinalIgnoreCase)))
                        prop.SetComputedColumnSql(null);

                    if ((prop.ClrType == typeof(DateTimeOffset) || prop.ClrType == typeof(DateTimeOffset?)) && prop.GetValueConverter() is null)
                        prop.SetValueConverter(new Microsoft.EntityFrameworkCore.Storage.ValueConversion.DateTimeOffsetToBinaryConverter());
                }

                foreach (var idx in entityType.GetDeclaredIndexes().Where(x => x.IsUnique && x.GetFilter() is not null).ToList())
                    idx.IsUnique = false;
            }
        }
    }

    private sealed class LocalSqliteTestDatabase : IDisposable
    {
        private readonly SqliteConnection _connection;
        private LocalSqliteTestDatabase(SqliteConnection connection) => _connection = connection;

        public static LocalSqliteTestDatabase Create()
        {
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();
            var db = new LocalSqliteTestDatabase(connection);
            using (var ctx = db.CreateContext()) ctx.Database.EnsureCreated();
            using (var pragma = connection.CreateCommand())
            {
                pragma.CommandText = "PRAGMA foreign_keys = OFF;";
                pragma.ExecuteNonQuery();
            }
            return db;
        }

        public ApplicationDbContext CreateContext()
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .ReplaceService<IModelCustomizer, LocalSqliteModelCustomizer>();
            return new ApplicationDbContext(builder.Options);
        }

        public void Dispose() => _connection.Dispose();
    }

    private readonly Guid _siteMmc = Guid.NewGuid();
    private readonly Guid _siteMdc = Guid.NewGuid();
    private readonly Guid _siteMhs = Guid.NewGuid();

    private readonly Guid _userHartini = Guid.NewGuid();
    private readonly Guid _userYudha = Guid.NewGuid();
    private readonly Guid _userManagerMmc = Guid.NewGuid();
    private readonly Guid _userEndang = Guid.NewGuid();

    private readonly Guid _profilHartini = Guid.NewGuid();
    private readonly Guid _profilYudha = Guid.NewGuid();
    private readonly Guid _profilManagerMmc = Guid.NewGuid();

    private readonly Guid _deptSdm = Guid.NewGuid();
    private readonly Guid _posKabagSdm = Guid.NewGuid();
    private readonly Guid _posStafSdm = Guid.NewGuid();
    private readonly Guid _posKarumMmc = Guid.NewGuid();

    public WorkforcePlanningSettingTests()
    {
        using var ctx = _db.CreateContext();

        ctx.MstHospitalSites.AddRange(
            new MstHospitalSite { Id = _siteMmc, SiteCode = "SITE-MMC-001", SiteName = "Metropolitan Medical Centre Hospital", IsMainSite = true, IsActive = true },
            new MstHospitalSite { Id = _siteMdc, SiteCode = "SITE-MDC-001", SiteName = "Metropolitan Diagnostic Centre", IsMainSite = false, IsActive = true },
            new MstHospitalSite { Id = _siteMhs, SiteCode = "SITE-MHS-001", SiteName = "Metropolitan Healthcare Services Head Office", IsMainSite = false, IsActive = true });

        var legalId = Guid.NewGuid();
        var unitMmc = Guid.NewGuid();
        var unitMdc = Guid.NewGuid();
        ctx.MstOrganizationUnits.AddRange(
            new MstOrganizationUnit { Id = unitMmc, LegalEntityId = legalId, HospitalSiteId = _siteMmc, UnitCode = "UNIT-MMC", UnitName = "Unit Layanan MMC", IsActive = true },
            new MstOrganizationUnit { Id = unitMdc, LegalEntityId = legalId, HospitalSiteId = _siteMdc, UnitCode = "UNIT-MDC", UnitName = "Unit Layanan MDC", IsActive = true });

        ctx.MstWorkforceProfiles.AddRange(
            new MstWorkforceProfile { Id = _profilHartini, ProfileCode = "WFP-01", DisplayName = "Hartini Sulistyowati, S.Psi., M.M.", UserType = UserType.Employee, IsActive = true },
            new MstWorkforceProfile { Id = _profilYudha, ProfileCode = "WFP-02", DisplayName = "Yudha Hermawan, S.E.", UserType = UserType.Employee, IsActive = true },
            new MstWorkforceProfile { Id = _profilManagerMmc, ProfileCode = "WFP-03", DisplayName = "Kepala Layanan MMC", UserType = UserType.Employee, IsActive = true });

        ctx.Users.AddRange(
            new ApplicationUser { Id = _userHartini, UserName = "hartini.sulistyowati@rsmmc.local", UserCode = "USR-01", DisplayName = "Hartini Sulistyowati, S.Psi., M.M.", UserType = UserType.Employee, WorkforceProfileId = _profilHartini, IsActive = true },
            new ApplicationUser { Id = _userYudha, UserName = "yudha.hermawan@rsmmc.local", UserCode = "USR-02", DisplayName = "Yudha Hermawan, S.E.", UserType = UserType.Employee, WorkforceProfileId = _profilYudha, IsActive = true },
            new ApplicationUser { Id = _userManagerMmc, UserName = "manager.mmc@rsmmc.local", UserCode = "USR-03", DisplayName = "Kepala Layanan MMC", UserType = UserType.Employee, WorkforceProfileId = _profilManagerMmc, IsActive = true },
            new ApplicationUser { Id = _userEndang, UserName = "endang.purwanti@rsmmc.local", UserCode = "USR-04", DisplayName = "Ns. Endang Purwanti, S.Kep.", UserType = UserType.Employee, IsActive = true });

        var startDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        ctx.WfpOrganizationAssignments.Add(new WfpOrganizationAssignment
        {
            WorkforceProfileId = _profilManagerMmc,
            HospitalSiteId = _siteMmc,
            OrganizationUnitId = unitMmc,
            DepartmentId = _deptSdm,
            PositionId = _posKarumMmc,
            AssignmentType = "Primary",
            IsPrimary = true,
            IsManagerialAssignment = true,
            EffectiveStartDate = startDate,
            IsActive = true
        });

        // Pengaturan awal MMC (Complete)
        ctx.HrdWorkforcePlanningSettings.Add(new HrdWorkforcePlanningSetting
        {
            Id = Guid.NewGuid(),
            HospitalSiteId = _siteMmc,
            LongLeaveThresholdDays = 30,
            OverdueRequestThresholdDays = 60,
            ChangeReason = "Penetapan awal kebijakan MMC",
            RowVersion = Guid.NewGuid(),
            CreateDateTime = new DateTime(2026, 11, 1, 8, 0, 0, DateTimeKind.Utc),
            CreateBy = _userHartini,
            UpdateDateTime = new DateTime(2026, 11, 2, 9, 30, 0, DateTimeKind.Utc),
            UpdateBy = _userHartini,
            IsDelete = false
        });

        // Pengaturan awal MDC (Incomplete: hanya cuti panjang terisi)
        ctx.HrdWorkforcePlanningSettings.Add(new HrdWorkforcePlanningSetting
        {
            Id = Guid.NewGuid(),
            HospitalSiteId = _siteMdc,
            LongLeaveThresholdDays = 45,
            OverdueRequestThresholdDays = null,
            ChangeReason = "Inisiasi parameter cuti panjang MDC",
            RowVersion = Guid.NewGuid(),
            CreateDateTime = new DateTime(2026, 11, 3, 7, 15, 0, DateTimeKind.Utc),
            CreateBy = _userYudha,
            IsDelete = false
        });

        // Daftarkan hak akses WorkforcePlanningSetting Read & Update
        var ctrl = new SysControllerAccess
        {
            ModuleId = Guid.NewGuid(),
            ControllerName = "WorkforcePlanningSetting",
            DisplayName = "Workforce Planning Setting"
        };
        var readAction = new SysActionAccess { ControllerAccessId = ctrl.Id, ActionName = "Read", DisplayName = "Read Workforce Planning Setting", AccessType = "Read" };
        var updateAction = new SysActionAccess { ControllerAccessId = ctrl.Id, ActionName = "Update", DisplayName = "Update Workforce Planning Setting", AccessType = "Update" };
        ctx.SysControllerAccesses.Add(ctrl);
        ctx.SysActionAccesses.AddRange(readAction, updateAction);

        ctx.ApplicationUserOrganizations.AddRange(
            new ApplicationUserOrganization { UserId = _userHartini, DepartmentId = _deptSdm, PositionId = _posKabagSdm, IsActive = true },
            new ApplicationUserOrganization { UserId = _userYudha, DepartmentId = _deptSdm, PositionId = _posStafSdm, IsActive = true });

        ctx.SysAccessPolicies.AddRange(
            new SysAccessPolicy { DepartmentId = _deptSdm, PositionId = _posKabagSdm, ControllerAccessId = ctrl.Id, ActionAccessId = readAction.Id, IsAllowed = true, IsActive = true },
            new SysAccessPolicy { DepartmentId = _deptSdm, PositionId = _posKabagSdm, ControllerAccessId = ctrl.Id, ActionAccessId = updateAction.Id, IsAllowed = true, IsActive = true },
            new SysAccessPolicy { DepartmentId = _deptSdm, PositionId = _posStafSdm, ControllerAccessId = ctrl.Id, ActionAccessId = readAction.Id, IsAllowed = true, IsActive = true });

        ctx.SaveChanges();
    }

    public void Dispose() => _db.Dispose();

    private HrdWorkforcePlanningSettingService CreateService(ApplicationDbContext ctx) =>
        new(ctx, NullLogger<HrdWorkforcePlanningSettingService>.Instance);

    [Fact]
    public async Task GetListAsync_melakukan_left_join_rumah_sakit_dan_menghitung_ringkasan_menyeluruh()
    {
        using var ctx = _db.CreateContext();
        var svc = CreateService(ctx);

        var res = await svc.GetListAsync(new WorkforcePlanningSettingListQuery { PageNumber = 1, PageSize = 10 }, _userHartini);

        Assert.True(res.Success, res.Message);
        Assert.NotNull(res.Data);
        Assert.Equal(3, res.Data!.TotalData);
        Assert.Equal(3, res.Data.Items.Count);

        Assert.Equal(3, res.Data.Summary.TotalHospitalSites);
        Assert.Equal(1, res.Data.Summary.CompleteCount);
        Assert.Equal(1, res.Data.Summary.IncompleteCount);
        Assert.Equal(1, res.Data.Summary.NotConfiguredCount);
        Assert.Equal(2, res.Data.Summary.IncompleteOrNotConfiguredCount);

        var mmc = res.Data.Items.Single(x => x.HospitalSiteId == _siteMmc);
        Assert.Equal(WorkforcePlanningSettingStatuses.Complete, mmc.ConfigurationStatus);
        Assert.Equal(30, mmc.LongLeaveThresholdDays);
        Assert.Equal(60, mmc.OverdueRequestThresholdDays);
        Assert.Equal("Hartini Sulistyowati, S.Psi., M.M.", mmc.UpdatedByName);
        Assert.Equal("Penetapan awal kebijakan MMC", mmc.ChangeReason);

        var mdc = res.Data.Items.Single(x => x.HospitalSiteId == _siteMdc);
        Assert.Equal(WorkforcePlanningSettingStatuses.Incomplete, mdc.ConfigurationStatus);
        Assert.Equal(45, mdc.LongLeaveThresholdDays);
        Assert.Null(mdc.OverdueRequestThresholdDays);
        Assert.Equal("Yudha Hermawan, S.E.", mdc.UpdatedByName);

        var mhs = res.Data.Items.Single(x => x.HospitalSiteId == _siteMhs);
        Assert.Null(mhs.Id);
        Assert.Equal(WorkforcePlanningSettingStatuses.NotConfigured, mhs.ConfigurationStatus);
        Assert.Null(mhs.LongLeaveThresholdDays);
        Assert.Null(mhs.OverdueRequestThresholdDays);
        Assert.Null(mhs.UpdatedAt);
        Assert.Null(mhs.UpdatedBy);
        Assert.Null(mhs.UpdatedByName);
    }

    [Fact]
    public async Task GetByHospitalSiteIdAsync_membedakan_sudah_diatur_dan_belum_diatur_tanpa_nilai_nol()
    {
        using var ctx = _db.CreateContext();
        var svc = CreateService(ctx);

        var configured = await svc.GetByHospitalSiteIdAsync(_siteMmc, _userHartini);
        Assert.True(configured.Success);
        Assert.Equal(WorkforcePlanningSettingStatuses.Complete, configured.Data!.ConfigurationStatus);
        Assert.Equal(30, configured.Data.LongLeaveThresholdDays);
        Assert.Equal(60, configured.Data.OverdueRequestThresholdDays);
        Assert.Equal("Hartini Sulistyowati, S.Psi., M.M.", configured.Data.UpdatedByName);

        var unconfigured = await svc.GetByHospitalSiteIdAsync(_siteMhs, _userHartini);
        Assert.True(unconfigured.Success);
        Assert.Null(unconfigured.Data!.Id);
        Assert.Equal(WorkforcePlanningSettingStatuses.NotConfigured, unconfigured.Data.ConfigurationStatus);
        Assert.Null(unconfigured.Data.LongLeaveThresholdDays);
        Assert.Null(unconfigured.Data.OverdueRequestThresholdDays);
        Assert.Null(unconfigured.Data.RowVersion);
    }

    [Fact]
    public async Task Put_awal_lalu_ubah_existing_dan_get_ulang_dari_konteks_baru_terbukti_persisten()
    {
        using (var ctxCreate = _db.CreateContext())
        {
            var svc = CreateService(ctxCreate);
            var created = await svc.UpdateAsync(new UpdateWorkforcePlanningSettingRequest
            {
                HospitalSiteId = _siteMhs,
                LongLeaveThresholdDays = 21,
                OverdueRequestThresholdDays = null,
                ChangeReason = "Uji konfigurasi awal MHS (hanya cuti panjang).",
                RowVersion = null
            }, _userHartini);

            Assert.True(created.Success, created.Message);
            Assert.NotNull(created.Data!.Id);
            Assert.Equal(WorkforcePlanningSettingStatuses.Incomplete, created.Data.ConfigurationStatus);
            Assert.Equal(21, created.Data.LongLeaveThresholdDays);
            Assert.Null(created.Data.OverdueRequestThresholdDays);
            Assert.Equal("Hartini Sulistyowati, S.Psi., M.M.", created.Data.UpdatedByName);

            // Ubah existing pada request berikutnya dengan RowVersion hasil create
            var updated = await svc.UpdateAsync(new UpdateWorkforcePlanningSettingRequest
            {
                HospitalSiteId = _siteMhs,
                LongLeaveThresholdDays = 28,
                OverdueRequestThresholdDays = 14,
                ChangeReason = "Melengkapi kedua parameter MHS untuk tahun 2027.",
                RowVersion = created.Data.RowVersion
            }, _userYudha);

            Assert.True(updated.Success, updated.Message);
            Assert.Equal(WorkforcePlanningSettingStatuses.Complete, updated.Data!.ConfigurationStatus);
            Assert.Equal(28, updated.Data.LongLeaveThresholdDays);
            Assert.Equal(14, updated.Data.OverdueRequestThresholdDays);
            Assert.Equal(_userYudha, updated.Data.UpdatedBy);
            Assert.Equal("Yudha Hermawan, S.E.", updated.Data.UpdatedByName);
            Assert.NotEqual(created.Data.RowVersion, updated.Data.RowVersion);
        }

        // Request GET baru dari DbContext terpisah membuktikan persistensi database nyata
        using (var ctxRead = _db.CreateContext())
        {
            var reread = await CreateService(ctxRead).GetByHospitalSiteIdAsync(_siteMhs, _userHartini);
            Assert.True(reread.Success, reread.Message);
            Assert.Equal(28, reread.Data!.LongLeaveThresholdDays);
            Assert.Equal(14, reread.Data.OverdueRequestThresholdDays);
            Assert.Equal(WorkforcePlanningSettingStatuses.Complete, reread.Data.ConfigurationStatus);
            Assert.Equal("Melengkapi kedua parameter MHS untuk tahun 2027.", reread.Data.ChangeReason);
            Assert.Equal("Yudha Hermawan, S.E.", reread.Data.UpdatedByName);
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Put_ditolak_400_ketika_alasan_kosong(string emptyReason)
    {
        using var ctx = _db.CreateContext();
        var svc = CreateService(ctx);

        var res = await svc.UpdateAsync(new UpdateWorkforcePlanningSettingRequest
        {
            HospitalSiteId = _siteMhs,
            LongLeaveThresholdDays = 30,
            OverdueRequestThresholdDays = 60,
            ChangeReason = emptyReason
        }, _userHartini);

        Assert.False(res.Success);
        Assert.Equal(StatusCodes.Status400BadRequest, res.StatusCode);
        Assert.Equal("Tuliskan alasan perubahan pengaturan.", res.Message);
    }

    [Theory]
    [InlineData(0, 60)]
    [InlineData(366, 60)]
    [InlineData(30, 0)]
    [InlineData(30, 366)]
    [InlineData(-5, 10)]
    public async Task Put_ditolak_400_ketika_parameter_hari_di_luar_rentang_1_sampai_365(int leaveDays, int overdueDays)
    {
        using var ctx = _db.CreateContext();
        var svc = CreateService(ctx);

        var res = await svc.UpdateAsync(new UpdateWorkforcePlanningSettingRequest
        {
            HospitalSiteId = _siteMhs,
            LongLeaveThresholdDays = leaveDays,
            OverdueRequestThresholdDays = overdueDays,
            ChangeReason = "Uji rentang di luar batas."
        }, _userHartini);

        Assert.False(res.Success);
        Assert.Equal(StatusCodes.Status400BadRequest, res.StatusCode);
        Assert.Equal("Isi angka antara 1 dan 365 hari.", res.Message);
    }

    [Fact]
    public async Task Put_ditolak_400_ketika_alasan_melebihi_500_karakter()
    {
        using var ctx = _db.CreateContext();
        var svc = CreateService(ctx);

        var res = await svc.UpdateAsync(new UpdateWorkforcePlanningSettingRequest
        {
            HospitalSiteId = _siteMhs,
            LongLeaveThresholdDays = 30,
            OverdueRequestThresholdDays = 60,
            ChangeReason = new string('A', 501)
        }, _userHartini);

        Assert.False(res.Success);
        Assert.Equal(StatusCodes.Status400BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task GetListAsync_filter_status_search_hospital_dan_pagination_menjaga_summary_total()
    {
        using var ctx = _db.CreateContext();
        var svc = CreateService(ctx);

        var searchDiagnostic = await svc.GetListAsync(new WorkforcePlanningSettingListQuery
        {
            Search = "Diagnostic"
        }, _userHartini);
        Assert.True(searchDiagnostic.Success);
        Assert.Equal(1, searchDiagnostic.Data!.TotalData);
        Assert.Equal(_siteMdc, searchDiagnostic.Data.Items.Single().HospitalSiteId);
        // Summary tetap menghitung seluruh RS (bukan 1 hasil filter)
        Assert.Equal(3, searchDiagnostic.Data.Summary.TotalHospitalSites);
        Assert.Equal(1, searchDiagnostic.Data.Summary.CompleteCount);
        Assert.Equal(2, searchDiagnostic.Data.Summary.IncompleteOrNotConfiguredCount);

        var filterIncomplete = await svc.GetListAsync(new WorkforcePlanningSettingListQuery
        {
            ConfigurationStatus = WorkforcePlanningSettingStatuses.Incomplete
        }, _userHartini);
        Assert.True(filterIncomplete.Success);
        Assert.Equal(1, filterIncomplete.Data!.TotalData);
        Assert.Equal(_siteMdc, filterIncomplete.Data.Items.Single().HospitalSiteId);

        var filterNotConfigured = await svc.GetListAsync(new WorkforcePlanningSettingListQuery
        {
            ConfigurationStatus = WorkforcePlanningSettingStatuses.NotConfigured
        }, _userHartini);
        Assert.True(filterNotConfigured.Success);
        Assert.Equal(1, filterNotConfigured.Data!.TotalData);
        Assert.Equal(_siteMhs, filterNotConfigured.Data.Items.Single().HospitalSiteId);

        var filterBySite = await svc.GetListAsync(new WorkforcePlanningSettingListQuery
        {
            HospitalSiteId = _siteMmc
        }, _userHartini);
        Assert.True(filterBySite.Success);
        Assert.Equal(1, filterBySite.Data!.TotalData);
        Assert.Equal(_siteMmc, filterBySite.Data.Items.Single().HospitalSiteId);

        var page1 = await svc.GetListAsync(new WorkforcePlanningSettingListQuery { PageNumber = 1, PageSize = 2 }, _userHartini);
        var page2 = await svc.GetListAsync(new WorkforcePlanningSettingListQuery { PageNumber = 2, PageSize = 2 }, _userHartini);
        Assert.Equal(2, page1.Data!.Items.Count);
        Assert.Single(page2.Data!.Items);
        Assert.Equal(2, page1.Data.TotalPage);
        Assert.Equal(3, page1.Data.Summary.TotalHospitalSites);
    }

    [Fact]
    public async Task Put_menghasilkan_409_pada_konflik_RowVersion_dan_duplikasi_indeks_unik()
    {
        Guid? originalVersion;
        using (var ctx0 = _db.CreateContext())
        {
            var svc0 = CreateService(ctx0);
            var cur = await svc0.GetByHospitalSiteIdAsync(_siteMmc, _userHartini);
            originalVersion = cur.Data!.RowVersion;

            // Pengguna 1 lebih dulu menyimpan dengan RowVersion awal -> sukses
            var win = await svc0.UpdateAsync(new UpdateWorkforcePlanningSettingRequest
            {
                HospitalSiteId = _siteMmc,
                LongLeaveThresholdDays = 35,
                OverdueRequestThresholdDays = 60,
                ChangeReason = "Pembaruan oleh pengguna 1.",
                RowVersion = originalVersion
            }, _userHartini);
            Assert.True(win.Success);

            // Pengguna 2 mengirim RowVersion basi -> 409 Conflict
            var conflict = await svc0.UpdateAsync(new UpdateWorkforcePlanningSettingRequest
            {
                HospitalSiteId = _siteMmc,
                LongLeaveThresholdDays = 40,
                OverdueRequestThresholdDays = 60,
                ChangeReason = "Pembaruan basi pengguna 2.",
                RowVersion = originalVersion
            }, _userYudha);
            Assert.False(conflict.Success);
            Assert.Equal(StatusCodes.Status409Conflict, conflict.StatusCode);
        }

        // Uji pencegahan duplikasi konfigurasi & penanganan UniqueConstraint (SQLite unique index simulasi IX_HrdWorkforcePlanningSetting_Site)
        using var ctxDup = _db.CreateContext();
        await ctxDup.Database.ExecuteSqlRawAsync(
            "CREATE UNIQUE INDEX IF NOT EXISTS IX_Uji_WfpSetting_Site ON \"HrdWorkforcePlanningSetting\" (\"HospitalSiteId\") WHERE \"IsDelete\" = 0;");

        // Pastikan baris ganda di level SQL akan tertangkap DbUpdateException -> 409 Conflict
        ctxDup.HrdWorkforcePlanningSettings.Add(new HrdWorkforcePlanningSetting
        {
            Id = Guid.NewGuid(),
            HospitalSiteId = _siteMmc,
            LongLeaveThresholdDays = 20,
            OverdueRequestThresholdDays = 20,
            ChangeReason = "Percobaan sisip ganda.",
            RowVersion = Guid.NewGuid(),
            CreateDateTime = DateTime.UtcNow,
            CreateBy = _userHartini,
            IsDelete = false
        });
        var dbEx = await Assert.ThrowsAsync<DbUpdateException>(() => ctxDup.SaveChangesAsync());
        Assert.NotNull(dbEx.InnerException);

        // Pastikan satu rumah sakit tetap hanya memiliki satu baris konfigurasi aktif via service UpdateAsync
        using var ctxClean = _db.CreateContext();
        var activeRows = await ctxClean.HrdWorkforcePlanningSettings
            .CountAsync(s => s.HospitalSiteId == _siteMmc && !s.IsDelete);
        Assert.Equal(1, activeRows);
    }

    [Fact]
    public void Controller_terdaftar_sah_di_PermissionRegistryValidator()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(new DiagnosticListener("uji"));
        services.AddSingleton<DiagnosticSource>(sp => sp.GetRequiredService<DiagnosticListener>());
        services.AddControllers().AddApplicationPart(typeof(HrdWorkforcePlanningSettingController).Assembly);
        using var provider = services.BuildServiceProvider();
        var actions = provider.GetRequiredService<IActionDescriptorCollectionProvider>();

        var result = new PermissionRegistryValidator(actions, NullLogger<PermissionRegistryValidator>.Instance)
            .ValidateAndReport(throwOnFailure: false);

        var findings = result.UnregisterableEndpoints
            .Concat(result.DuplicateResourceIdentities)
            .Concat(result.InvalidAccessTypes)
            .Concat(result.UnenforcedEndpoints)
            .Concat(result.NakedBusinessEndpoints)
            .Where(x => x.StartsWith("WorkforcePlanningSetting.", StringComparison.Ordinal) ||
                        x.Contains("'WorkforcePlanningSetting'", StringComparison.Ordinal))
            .ToList();

        Assert.True(findings.Count == 0, string.Join(Environment.NewLine, findings));

        var listMethod = typeof(HrdWorkforcePlanningSettingController).GetMethod(nameof(HrdWorkforcePlanningSettingController.GetList))!;
        var listPerm = listMethod.GetCustomAttribute<AccessPermissionAttribute>();
        Assert.NotNull(listPerm);
        Assert.Equal("WorkforcePlanningSetting", listPerm!.Arguments![0]);
        Assert.Equal("Read", listPerm.Arguments[1]);

        var updateMethod = typeof(HrdWorkforcePlanningSettingController).GetMethod(nameof(HrdWorkforcePlanningSettingController.Update))!;
        var updatePerm = updateMethod.GetCustomAttribute<AccessPermissionAttribute>();
        Assert.NotNull(updatePerm);
        Assert.Equal("WorkforcePlanningSetting", updatePerm!.Arguments![0]);
        Assert.Equal("Update", updatePerm.Arguments[1]);
    }

    [Fact]
    public async Task AccessPermissionService_membedakan_izin_Read_Update_dan_tanpa_akses()
    {
        using var ctx = _db.CreateContext();
        var permSvc = Pelaku.Izin(ctx);

        static ClaimsPrincipal ClaimsOf(Guid uid) =>
            new(new ClaimsIdentity(new[] { new Claim("user_id", uid.ToString()) }, "Test"));

        Assert.True(await permSvc.HasAccessAsync(ClaimsOf(_userHartini), "WorkforcePlanningSetting", "Read"));
        Assert.True(await permSvc.HasAccessAsync(ClaimsOf(_userHartini), "WorkforcePlanningSetting", "Update"));

        Assert.True(await permSvc.HasAccessAsync(ClaimsOf(_userYudha), "WorkforcePlanningSetting", "Read"));
        Assert.False(await permSvc.HasAccessAsync(ClaimsOf(_userYudha), "WorkforcePlanningSetting", "Update"));

        Assert.False(await permSvc.HasAccessAsync(ClaimsOf(_userEndang), "WorkforcePlanningSetting", "Read"));
        Assert.False(await permSvc.HasAccessAsync(ClaimsOf(_userEndang), "WorkforcePlanningSetting", "Update"));
    }

    [Fact]
    public async Task Pembatasan_akses_rumah_sakit_menolak_list_detail_dan_update_di_luar_cakupan()
    {
        using var ctx = _db.CreateContext();
        var svc = CreateService(ctx);

        // Akun manajerial MMC hanya boleh melihat & mengubah MMC, tidak melihat MDC maupun MHS
        var listRes = await svc.GetListAsync(new WorkforcePlanningSettingListQuery(), _userManagerMmc);
        Assert.True(listRes.Success, listRes.Message);
        Assert.Equal(1, listRes.Data!.TotalData);
        Assert.Single(listRes.Data.Items);
        Assert.Equal(_siteMmc, listRes.Data.Items[0].HospitalSiteId);
        Assert.Equal(1, listRes.Data.Summary.TotalHospitalSites);
        Assert.Equal(1, listRes.Data.Summary.CompleteCount);
        Assert.Equal(0, listRes.Data.Summary.IncompleteOrNotConfiguredCount);

        var filteredForbiddenList = await svc.GetListAsync(new WorkforcePlanningSettingListQuery { HospitalSiteId = _siteMdc }, _userManagerMmc);
        Assert.False(filteredForbiddenList.Success);
        Assert.Equal(StatusCodes.Status403Forbidden, filteredForbiddenList.StatusCode);

        var detailAllowed = await svc.GetByHospitalSiteIdAsync(_siteMmc, _userManagerMmc);
        Assert.True(detailAllowed.Success);

        var detailForbidden = await svc.GetByHospitalSiteIdAsync(_siteMdc, _userManagerMmc);
        Assert.False(detailForbidden.Success);
        Assert.Equal(StatusCodes.Status403Forbidden, detailForbidden.StatusCode);
        Assert.Null(detailForbidden.Data);

        var updateForbidden = await svc.UpdateAsync(new UpdateWorkforcePlanningSettingRequest
        {
            HospitalSiteId = _siteMhs,
            LongLeaveThresholdDays = 10,
            OverdueRequestThresholdDays = 10,
            ChangeReason = "Mencoba ubah RS di luar cakupan."
        }, _userManagerMmc);
        Assert.False(updateForbidden.Success);
        Assert.Equal(StatusCodes.Status403Forbidden, updateForbidden.StatusCode);
    }
}
