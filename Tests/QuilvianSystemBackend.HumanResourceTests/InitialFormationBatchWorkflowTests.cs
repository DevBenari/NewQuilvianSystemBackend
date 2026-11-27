using System.Data.Common;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Services;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Shared.HumanResource.Services;

namespace QuilvianSystemBackend.Tests.HumanResource;

// BE-HRD-FIN-WFP-008..011: Pengujian ujung-ke-ujung Batch Formasi Awal dengan Workflow T10 (3 tingkat:
// HR Manager -> Finance -> Direktur Utama), penolakan false-success tanpa workflow yatim, pemulihan
// batch Submitted terputus ke Draft / rekonsiliasi, idempotensi callback, dan pembentukan formasi 2027-01-01.
public sealed class InitialFormationBatchWorkflowTests : IDisposable
{
    private sealed class LocalSqliteDb : IDisposable
    {
        private readonly SqliteConnection _connection;
        private LocalSqliteDb(SqliteConnection connection) => _connection = connection;

        public static LocalSqliteDb Create()
        {
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();
            connection.CreateFunction<string?, string?>("btrim", s => s?.Trim());
            connection.CreateFunction<string?, string?, string?>("btrim", (s, chars) =>
                s is null ? null : string.IsNullOrEmpty(chars) ? s.Trim() : s.Trim(chars.ToCharArray()));
            var db = new LocalSqliteDb(connection);
            using (var ctx = db.CreateContext()) ctx.Database.EnsureCreated();
            using (var pragma = connection.CreateCommand())
            {
                pragma.CommandText = "PRAGMA foreign_keys = OFF;";
                pragma.ExecuteNonQuery();
            }
            return db;
        }

        public ApplicationDbContext CreateContext(params IInterceptor[] interceptors)
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .ReplaceService<IModelCustomizer, ModelUjiSqlite>();
            if (interceptors.Length > 0) builder.AddInterceptors(interceptors);
            return new ApplicationDbContext(builder.Options);
        }

        public void Dispose() => _connection.Dispose();
    }

    private readonly LocalSqliteDb _db = LocalSqliteDb.Create();
    private readonly Guid _legalId = Guid.NewGuid();
    private readonly Guid _hospitalSiteId = Guid.NewGuid();
    private readonly Guid _unitId = Guid.NewGuid();
    private readonly Guid _positionPerawatId = Guid.NewGuid();
    private readonly Guid _deptSdmId = Guid.NewGuid();
    private readonly Guid _deptFinanceId = Guid.NewGuid();
    private readonly Guid _deptDireksiId = Guid.NewGuid();
    private readonly Guid _posStaffSdmId = Guid.NewGuid();
    private readonly Guid _posHrManagerId = Guid.NewGuid();
    private readonly Guid _posFinanceManagerId = Guid.NewGuid();
    private readonly Guid _posDirekturUtamaId = Guid.NewGuid();
    private readonly Guid _defT10Id = Guid.NewGuid();
    private readonly Guid _userSuperAdminNoProfileId = Guid.NewGuid();

    private readonly Dictionary<string, (Guid Wf, Guid User)> _actors = new();

    private sealed class PostgresKeSqliteInterceptor : DbCommandInterceptor
    {
        private static bool Adapt(DbCommand c)
        {
            if (c.CommandText.Contains("pg_advisory_xact_lock", StringComparison.OrdinalIgnoreCase))
            {
                c.CommandText = "SELECT 1;";
                return true;
            }

            if (c.CommandText.Contains("FOR UPDATE", StringComparison.Ordinal))
            {
                c.CommandText = c.CommandText
                    .Replace("public.\"", "\"", StringComparison.Ordinal)
                    .Replace("FOR UPDATE", string.Empty, StringComparison.Ordinal);
            }

            return false;
        }

        public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
        {
            Adapt(command);
            return result;
        }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
        {
            Adapt(command);
            return ValueTask.FromResult(result);
        }

        public override InterceptionResult<int> NonQueryExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
        {
            Adapt(command);
            return result;
        }

        public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            Adapt(command);
            return ValueTask.FromResult(result);
        }
    }

    private sealed class SqliteContextFactory(LocalSqliteDb db) : IDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext() => db.CreateContext();
        public Task<ApplicationDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) => Task.FromResult(db.CreateContext());
    }

    public InitialFormationBatchWorkflowTests()
    {
        using var ctx = _db.CreateContext();

        ctx.MstLegalEntities.Add(new MstLegalEntity
        {
            Id = _legalId,
            LegalEntityCode = "LE-MMC",
            LegalEntityName = "PT Metropolitan Medical Centre",
            IsActive = true
        });

        ctx.MstHospitalSites.Add(new MstHospitalSite
        {
            Id = _hospitalSiteId,
            LegalEntityId = _legalId,
            SiteCode = "SITE-MMC-001",
            SiteName = "Metropolitan Medical Centre Hospital",
            IsMainSite = true,
            IsActive = true
        });

        ctx.MstOrganizationUnits.Add(new MstOrganizationUnit
        {
            Id = _unitId,
            LegalEntityId = _legalId,
            HospitalSiteId = _hospitalSiteId,
            UnitCode = "ORG-IRNA",
            UnitName = "Instalasi Rawat Inap",
            IsActive = true
        });

        ctx.MstDepartments.AddRange(
            new MstDepartment { Id = _deptSdmId, DepartmentCode = "DPT-HR", DepartmentName = "Human Resource", IsActive = true },
            new MstDepartment { Id = _deptFinanceId, DepartmentCode = "DPT-FIN", DepartmentName = "Finance", IsActive = true },
            new MstDepartment { Id = _deptDireksiId, DepartmentCode = "DPT-DIR", DepartmentName = "Direksi", IsActive = true });

        ctx.MstPositions.AddRange(
            new MstPosition { Id = _positionPerawatId, PositionCode = "POS-KEP-004", PositionName = "Perawat Rawat Inap", IsActive = true },
            new MstPosition { Id = _posStaffSdmId, PositionCode = "POS000001", PositionName = "Staff HR", IsActive = true },
            new MstPosition { Id = _posHrManagerId, PositionCode = "POS000002", PositionName = "Manajer HR", IsActive = true },
            new MstPosition { Id = _posFinanceManagerId, PositionCode = "POS000004", PositionName = "Manajer Finance", IsActive = true },
            new MstPosition { Id = _posDirekturUtamaId, PositionCode = "POS-DIR-001", PositionName = "Direktur Utama", IsActive = true });

        var effective = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var people = new[]
        {
            ("PemohonHR", "Anisa Rahmawati", _deptSdmId, _posStaffSdmId, false),
            ("HrManager", "Budi Prakoso", _deptSdmId, _posHrManagerId, true),
            ("Finance", "dr. Fajar Rama", _deptFinanceId, _posFinanceManagerId, true),
            ("Dirut", "dr. Direktur Utama", _deptDireksiId, _posDirekturUtamaId, true),
        };

        foreach (var (key, display, deptId, posId, isMgr) in people)
        {
            var wf = Guid.NewGuid();
            var userId = Guid.NewGuid();
            _actors[key] = (wf, userId);

            ctx.MstWorkforceProfiles.Add(new MstWorkforceProfile
            {
                Id = wf,
                ProfileCode = "WFP-" + key.ToUpperInvariant(),
                DisplayName = display,
                PrimaryDepartmentId = deptId,
                PrimaryPositionId = posId,
                UserType = UserType.Employee,
                IsActive = true
            });

            ctx.Users.Add(new ApplicationUser
            {
                Id = userId,
                UserName = key.ToLowerInvariant() + "@rsmmc.local",
                UserCode = ("U" + key.ToUpperInvariant()).PadRight(8, '0')[..8],
                DisplayName = display,
                UserType = UserType.Employee,
                WorkforceProfileId = wf,
                IsActive = true
            });

            ctx.WfpOrganizationAssignments.Add(new WfpOrganizationAssignment
            {
                WorkforceProfileId = wf,
                LegalEntityId = _legalId,
                HospitalSiteId = _hospitalSiteId,
                OrganizationUnitId = _unitId,
                DepartmentId = deptId,
                PositionId = posId,
                AssignmentType = "Primary",
                IsPrimary = true,
                IsManagerialAssignment = isMgr,
                EffectiveStartDate = effective,
                IsActive = true
            });
        }

        // Akun tanpa workforce profile (simulasi SuperAdmin tanpa penempatan organisasi).
        ctx.Users.Add(new ApplicationUser
        {
            Id = _userSuperAdminNoProfileId,
            UserName = "superadmin.tanpaprofil@rsmmc.local",
            UserCode = "SA000001",
            DisplayName = "SuperAdmin",
            UserType = UserType.SuperAdmin,
            WorkforceProfileId = null,
            IsActive = true
        });

        ctx.Set<MstWorkflowDefinition>().Add(new MstWorkflowDefinition
        {
            Id = _defT10Id,
            WorkflowCode = WorkforcePlanningValueConstants.WorkflowDefinitionCodes.ManpowerPlanAndInitialFormation,
            WorkflowName = "Persetujuan Rencana Formasi T10",
            RequestType = "ManpowerPlan",
            WorkflowCategory = "HumanResource",
            Version = 1,
            WorkflowStatus = "Active",
            IsDefault = true,
            IsActive = true
        });

        ctx.Set<MstWorkflowStep>().AddRange(
            new MstWorkflowStep
            {
                Id = Guid.NewGuid(),
                WorkflowDefinitionId = _defT10Id,
                StepCode = "T10_HR_MANAGER",
                StepName = "Persetujuan HR Manager",
                StepOrder = 1,
                StepType = "Approval",
                ApprovalMode = "Any",
                ApproverSourceType = WorkflowValueConstants.ApproverSource.Position,
                ApproverPositionId = _posHrManagerId,
                AllowSelfApproval = false,
                EscalationAfterHours = 48,
                IsActive = true
            },
            new MstWorkflowStep
            {
                Id = Guid.NewGuid(),
                WorkflowDefinitionId = _defT10Id,
                StepCode = "T10_FINANCE",
                StepName = "Persetujuan Finance",
                StepOrder = 2,
                StepType = "Approval",
                ApprovalMode = "Any",
                ApproverSourceType = WorkflowValueConstants.ApproverSource.Position,
                ApproverPositionId = _posFinanceManagerId,
                AllowSelfApproval = false,
                EscalationAfterHours = 48,
                IsActive = true
            },
            new MstWorkflowStep
            {
                Id = Guid.NewGuid(),
                WorkflowDefinitionId = _defT10Id,
                StepCode = "T10_DIREKTUR_UTAMA",
                StepName = "Persetujuan Direktur Utama",
                StepOrder = 3,
                StepType = "Approval",
                ApprovalMode = "Any",
                ApproverSourceType = WorkflowValueConstants.ApproverSource.Position,
                ApproverPositionId = _posDirekturUtamaId,
                AllowSelfApproval = false,
                EscalationAfterHours = 72,
                IsActive = true
            });

        ctx.SaveChanges();
    }

    public void Dispose() => _db.Dispose();

    private ApplicationDbContext Konteks() => _db.CreateContext(new PostgresKeSqliteInterceptor());

    private ServiceProvider Layanan(ApplicationDbContext ctx, Guid actorUserId)
    {
        var services = new ServiceCollection();
        services.AddSingleton(ctx);
        services.AddSingleton<IHrdPositionFormationService>(new HrdPositionFormationService(ctx, NullLogger<HrdPositionFormationService>.Instance));
        services.AddSingleton<IWorkforcePlanningDecisionApplier>(sp =>
            new HrdInitialFormationBatchDecisionApplier(
                ctx,
                sp.GetRequiredService<IHrdPositionFormationService>(),
                NullLogger<HrdInitialFormationBatchDecisionApplier>.Instance));
        services.AddSingleton<IHrdWorkforcePlanningWorkflowIntegrationService>(sp =>
            new HrdWorkforcePlanningWorkflowIntegrationService(
                ctx,
                sp,
                sp.GetServices<IWorkforcePlanningDecisionApplier>(),
                NullLogger<HrdWorkforcePlanningWorkflowIntegrationService>.Instance));
        services.AddSingleton(sp =>
            new WorkflowReferenceLifecycleService(
                ctx,
                null!, null!, null!, null!, null!, null!, null!, null!, null!, null!,
                sp.GetRequiredService<IHrdWorkforcePlanningWorkflowIntegrationService>(),
                NullLogger<WorkflowReferenceLifecycleService>.Instance,
                Array.Empty<IWorkflowReferenceLifecycleHandler>()));
        services.AddSingleton(sp =>
        {
            var http = Pelaku.Sebagai(actorUserId);
            var userManager = new UserManager<ApplicationUser>(
                new TanpaPeranStore(ctx),
                Options.Create(new IdentityOptions()),
                new PasswordHasher<ApplicationUser>(),
                Array.Empty<IUserValidator<ApplicationUser>>(),
                Array.Empty<IPasswordValidator<ApplicationUser>>(),
                new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(),
                null!,
                NullLogger<UserManager<ApplicationUser>>.Instance);
            return new WorkflowService(
                ctx,
                new HumanResourceContextService(ctx, http, userManager),
                userManager,
                http,
                sp.GetRequiredService<WorkflowReferenceLifecycleService>());
        });
        services.AddSingleton(sp =>
            new HrdInitialFormationBatchService(
                ctx,
                new NumberSeriesAllocator(new SqliteContextFactory(_db)),
                sp.GetRequiredService<IHrdWorkforcePlanningWorkflowIntegrationService>(),
                NullLogger<HrdInitialFormationBatchService>.Instance));
        return services.BuildServiceProvider();
    }

    private CreateInitialFormationBatchRequest PayloadBatch(string skRef = "UAT/SK-FORMASI/001/2027") => new()
    {
        HospitalSiteId = _hospitalSiteId,
        SourceDocumentReference = skRef,
        Notes = "Pengisian formasi awal UAT",
        Rows =
        {
            new InitialFormationBatchRowRequest
            {
                OrganizationUnitId = _unitId,
                PositionId = _positionPerawatId,
                FormationQuantity = 10m,
                EffectiveStartDate = new DateOnly(2027, 1, 1)
            }
        }
    };

    private async Task AttachDummySkAsync(Guid batchId)
    {
        await using var ctx = Konteks();
        var batch = await ctx.HrdInitialFormationBatches.SingleAsync(b => b.Id == batchId);
        batch.SourceDocumentPath = $"/uploads/sk-{batchId:N}.pdf";
        batch.SourceDocumentFileName = "SK_Formasi_Awal_2027_Khusus_UAT.pdf";
        batch.RowVersion = Guid.NewGuid();
        await ctx.SaveChangesAsync();
    }

    private Guid? OpenAssignmentForUser(Guid workflowInstanceId, Guid userId)
    {
        using var ctx = _db.CreateContext();
        return ctx.TrxWorkflowApproverAssignments.AsNoTracking()
            .Where(a => a.WorkflowInstanceId == workflowInstanceId &&
                        a.AssignedApproverUserId == userId &&
                        a.IsCurrentAssignment &&
                        a.AssignmentStatus != WorkflowValueConstants.AssignmentStatus.Approved &&
                        a.AssignmentStatus != WorkflowValueConstants.AssignmentStatus.Completed &&
                        a.AssignmentStatus != WorkflowValueConstants.AssignmentStatus.Cancelled &&
                        a.AssignmentStatus != WorkflowValueConstants.AssignmentStatus.Rejected)
            .Select(a => (Guid?)a.Id)
            .FirstOrDefault();
    }

    [Fact]
    public async Task Ajukan_T10_dan_setujui_tiga_tingkat_membentuk_formasi_2027_01_01_secara_idempoten()
    {
        Guid batchId;
        Guid workflowId;
        Guid rowVersion;

        await using (var ctx = Konteks())
        await using (var sp = Layanan(ctx, _actors["PemohonHR"].User))
        {
            var svc = sp.GetRequiredService<HrdInitialFormationBatchService>();
            var created = await svc.CreateDraftAsync(PayloadBatch(), _actors["PemohonHR"].User);
            Assert.True(created.Success, created.Message);
            batchId = created.Data!.Id;
        }

        await AttachDummySkAsync(batchId);

        await using (var ctxGet = Konteks())
        await using (var spGet = Layanan(ctxGet, _actors["PemohonHR"].User))
        {
            var detail = await spGet.GetRequiredService<HrdInitialFormationBatchService>().GetByIdAsync(batchId);
            rowVersion = detail!.RowVersion;
        }

        // Submit -> Workflow T10 tercipta & berada di Step 1 (HR Manager)
        await using (var ctxSub = Konteks())
        await using (var spSub = Layanan(ctxSub, _actors["PemohonHR"].User))
        {
            var svc = spSub.GetRequiredService<HrdInitialFormationBatchService>();
            var submitted = await svc.SubmitAsync(batchId, new SubmitInitialFormationBatchRequest { RowVersion = rowVersion }, _actors["PemohonHR"].User);
            Assert.True(submitted.Success, submitted.Message);
            Assert.Equal(InitialFormationBatchStatus.Submitted, submitted.Data!.BatchStatus);
            Assert.NotNull(submitted.Data.WorkflowInstanceId);
            Assert.Equal(_defT10Id, submitted.Data.WorkflowDefinitionId);
            Assert.False(submitted.Data.HasWorkflowIntegrityIssue);
            Assert.Equal(1, submitted.Data.CurrentStepOrder);
            Assert.Equal("Persetujuan HR Manager", submitted.Data.CurrentStepName);
            workflowId = submitted.Data.WorkflowInstanceId!.Value;
        }

        // Step 1: HR Manager approve -> Step 2 (Finance), formasi belum terbentuk
        await using (var ctxS1 = Konteks())
        await using (var spS1 = Layanan(ctxS1, _actors["HrManager"].User))
        {
            var assignId = OpenAssignmentForUser(workflowId, _actors["HrManager"].User);
            Assert.NotNull(assignId);
            Assert.Null(OpenAssignmentForUser(workflowId, _actors["Finance"].User));
            Assert.Null(OpenAssignmentForUser(workflowId, _actors["Dirut"].User));

            var res = await spS1.GetRequiredService<WorkflowService>().ApproveAsync(workflowId, assignId!.Value, new WorkflowApproveRequest { Comment = "Disetujui HR Manager" });
            Assert.True(res.Success, res.Message);
            Assert.Equal(WorkflowValueConstants.WorkflowStatus.InProgress, res.Data!.WorkflowStatus);
            Assert.Equal(2, res.Data.CurrentStepOrder);
            Assert.Equal(3, await ctxS1.TrxWorkflowInstances.AsNoTracking().Where(w => w.Id == workflowId).Select(w => w.TotalSteps).SingleAsync());
            Assert.Equal(0, await ctxS1.HrdPositionFormations.CountAsync(f => f.SourceDocumentId == batchId));
        }

        // Step 2: Finance approve -> Step 3 (Direktur Utama), formasi belum terbentuk
        await using (var ctxS2 = Konteks())
        await using (var spS2 = Layanan(ctxS2, _actors["Finance"].User))
        {
            var assignId = OpenAssignmentForUser(workflowId, _actors["Finance"].User);
            Assert.NotNull(assignId);

            var res = await spS2.GetRequiredService<WorkflowService>().ApproveAsync(workflowId, assignId!.Value, new WorkflowApproveRequest { Comment = "Disetujui Finance" });
            Assert.True(res.Success, res.Message);
            Assert.Equal(WorkflowValueConstants.WorkflowStatus.InProgress, res.Data!.WorkflowStatus);
            Assert.Equal(3, res.Data.CurrentStepOrder);
            Assert.Equal(0, await ctxS2.HrdPositionFormations.CountAsync(f => f.SourceDocumentId == batchId));
        }

        // Step 3: Direktur Utama approve -> Completed -> Applied + CreatedFormationId terisi (10 FTE, 2027-01-01)
        await using (var ctxS3 = Konteks())
        await using (var spS3 = Layanan(ctxS3, _actors["Dirut"].User))
        {
            var assignId = OpenAssignmentForUser(workflowId, _actors["Dirut"].User);
            Assert.NotNull(assignId);

            var res = await spS3.GetRequiredService<WorkflowService>().ApproveAsync(workflowId, assignId!.Value, new WorkflowApproveRequest { Comment = "Disahkan Direktur Utama" });
            Assert.True(res.Success, res.Message);
            Assert.Equal(WorkflowValueConstants.WorkflowStatus.Completed, res.Data!.WorkflowStatus);
        }

        await using (var ctxAfter = Konteks())
        await using (var spAfter = Layanan(ctxAfter, _actors["PemohonHR"].User))
        {
            var detail = await spAfter.GetRequiredService<HrdInitialFormationBatchService>().GetByIdAsync(batchId);
            Assert.NotNull(detail);
            Assert.Equal(InitialFormationBatchStatus.Applied, detail!.BatchStatus);
            Assert.NotNull(detail.ApprovedAt);
            Assert.NotNull(detail.AppliedAt);
            var row = Assert.Single(detail.Rows);
            Assert.NotNull(row.CreatedFormationId);

            var formation = await ctxAfter.HrdPositionFormations.AsNoTracking().SingleAsync(f => f.Id == row.CreatedFormationId!.Value);
            Assert.Equal(10m, formation.FormationQuantity);
            Assert.Equal(new DateOnly(2027, 1, 1), formation.EffectiveStartDate);
            Assert.Equal(1, formation.VersionNumber);
            Assert.Equal(PositionFormationSourceType.InitialBatch, formation.SourceType);

            // Idempotensi callback persetujuan berulang tidak membentuk formasi kedua
            var wfInstance = await ctxAfter.TrxWorkflowInstances.SingleAsync(w => w.Id == workflowId);
            var syncRepeat = await spAfter.GetRequiredService<IHrdWorkforcePlanningWorkflowIntegrationService>()
                .SynchronizeAsync(wfInstance, _actors["Dirut"].User);
            Assert.True(syncRepeat.IsHandled);
            Assert.False(syncRepeat.StatusChanged);
            Assert.Equal(1, await ctxAfter.HrdPositionFormations.CountAsync(f => f.SourceDocumentId == batchId && !f.IsDelete));
        }
    }

    [Fact]
    public async Task Gagal_pengajuan_tidak_menyimpan_Submitted_dan_tidak_meninggalkan_workflow_yatim()
    {
        Guid batchNoProfileId;
        Guid batchStepMissingId;

        // 1) Uji akun tanpa workforce profile -> gagal sebelum status berubah
        await using (var ctx = Konteks())
        await using (var sp = Layanan(ctx, _actors["PemohonHR"].User))
        {
            var c1 = await sp.GetRequiredService<HrdInitialFormationBatchService>().CreateDraftAsync(PayloadBatch("UAT/SK-FAIL/01"), _actors["PemohonHR"].User);
            Assert.True(c1.Success, c1.Message);
            batchNoProfileId = c1.Data!.Id;
        }

        await AttachDummySkAsync(batchNoProfileId);

        await using (var ctxNoProf = Konteks())
        await using (var spNoProf = Layanan(ctxNoProf, _userSuperAdminNoProfileId))
        {
            var svc = spNoProf.GetRequiredService<HrdInitialFormationBatchService>();
            var current = await svc.GetByIdAsync(batchNoProfileId);
            var failRes = await svc.SubmitAsync(batchNoProfileId, new SubmitInitialFormationBatchRequest { RowVersion = current!.RowVersion }, _userSuperAdminNoProfileId);
            Assert.False(failRes.Success);
            Assert.Equal(400, failRes.StatusCode);

            var reloaded = await svc.GetByIdAsync(batchNoProfileId);
            Assert.Equal(InitialFormationBatchStatus.Draft, reloaded!.BatchStatus);
            Assert.Null(reloaded.SubmittedAt);
            Assert.Null(reloaded.WorkflowInstanceId);
            Assert.Equal(0, await ctxNoProf.TrxWorkflowInstances.CountAsync(w => !w.IsDelete && w.ReferenceId == batchNoProfileId));
        }

        // Batalkan batchNoProfileId agar tidak bentrok VR-WFP-11 dengan skenario uji berikutnya
        await using (var ctxClean = Konteks())
        {
            var b = await ctxClean.HrdInitialFormationBatches.Include(x => x.Rows).SingleAsync(x => x.Id == batchNoProfileId);
            b.IsDelete = true;
            foreach (var r in b.Rows) r.IsDelete = true;
            await ctxClean.SaveChangesAsync();
        }

        await using (var ctx2 = Konteks())
        await using (var sp2 = Layanan(ctx2, _actors["PemohonHR"].User))
        {
            var c2 = await sp2.GetRequiredService<HrdInitialFormationBatchService>().CreateDraftAsync(PayloadBatch("UAT/SK-FAIL/02"), _actors["PemohonHR"].User);
            Assert.True(c2.Success, c2.Message);
            batchStepMissingId = c2.Data!.Id;
        }

        await AttachDummySkAsync(batchStepMissingId);

        // 2) Nonaktifkan step T10 -> gagal tertutup dan tidak ada orphan Draft workflow
        await using (var ctxDeact = Konteks())
        {
            var steps = await ctxDeact.Set<MstWorkflowStep>().Where(s => s.WorkflowDefinitionId == _defT10Id).ToListAsync();
            foreach (var s in steps) s.IsActive = false;
            await ctxDeact.SaveChangesAsync();
        }

        await using (var ctxStepFail = Konteks())
        await using (var spStepFail = Layanan(ctxStepFail, _actors["PemohonHR"].User))
        {
            var svc = spStepFail.GetRequiredService<HrdInitialFormationBatchService>();
            var current = await svc.GetByIdAsync(batchStepMissingId);
            var failRes = await svc.SubmitAsync(batchStepMissingId, new SubmitInitialFormationBatchRequest { RowVersion = current!.RowVersion }, _actors["PemohonHR"].User);
            Assert.False(failRes.Success);
            Assert.Equal(400, failRes.StatusCode);

            var reloaded = await svc.GetByIdAsync(batchStepMissingId);
            Assert.Equal(InitialFormationBatchStatus.Draft, reloaded!.BatchStatus);
            Assert.Null(reloaded.SubmittedAt);
            Assert.Null(reloaded.WorkflowInstanceId);
            Assert.Equal(0, await ctxStepFail.TrxWorkflowInstances.CountAsync(w => !w.IsDelete && w.ReferenceId == batchStepMissingId));
        }
    }

    [Fact]
    public async Task Pemulihan_batch_Submitted_tanpa_workflow_mengembalikan_ke_Draft_lalu_berhasil_diajukan()
    {
        var stuckBatchId = new Guid("3089fe12-7ddb-4500-8047-6e1d98e05355");
        var initialRowVersion = Guid.NewGuid();

        await using (var ctxSeed = Konteks())
        {
            ctxSeed.HrdInitialFormationBatches.Add(new HrdInitialFormationBatch
            {
                Id = stuckBatchId,
                BatchNumber = "IFB-000001",
                HospitalSiteId = _hospitalSiteId,
                SourceDocumentReference = "UAT/SK-FORMASI/001/2027",
                SourceDocumentPath = "/uploads/sk_uat.pdf",
                SourceDocumentFileName = "SK_Formasi_Awal_2027_Khusus_UAT.pdf",
                BatchStatus = InitialFormationBatchStatus.Submitted,
                TotalRows = 1,
                ValidRows = 1,
                InvalidRows = 0,
                WorkflowInstanceId = null,
                WorkflowDefinitionId = null,
                SubmittedAt = DateTime.UtcNow.AddHours(-2),
                SubmittedByUserId = _userSuperAdminNoProfileId,
                RowVersion = initialRowVersion,
                CreateDateTime = DateTime.UtcNow.AddHours(-2),
                CreateBy = _userSuperAdminNoProfileId,
                Rows =
                {
                    new HrdInitialFormationBatchRow
                    {
                        Id = Guid.NewGuid(),
                        BatchId = stuckBatchId,
                        OrganizationUnitId = _unitId,
                        PositionId = _positionPerawatId,
                        FormationQuantity = 10m,
                        EffectiveStartDate = new DateOnly(2027, 1, 1),
                        IsValid = true,
                        CreatedFormationId = null
                    }
                }
            });
            await ctxSeed.SaveChangesAsync();
        }

        Guid draftRowVersion;
        await using (var ctxRec = Konteks())
        await using (var spRec = Layanan(ctxRec, _actors["PemohonHR"].User))
        {
            var svc = spRec.GetRequiredService<HrdInitialFormationBatchService>();
            var before = await svc.GetByIdAsync(stuckBatchId);
            Assert.True(before!.HasWorkflowIntegrityIssue);

            var recovered = await svc.RecoverAsync(
                stuckBatchId,
                new RecoverInitialFormationBatchRequest
                {
                    Reason = "Pemulihan batch IFB-000001 yang berstatus Submitted tanpa WorkflowInstanceId.",
                    RowVersion = initialRowVersion
                },
                _actors["PemohonHR"].User);

            Assert.True(recovered.Success, recovered.Message);
            Assert.Equal(InitialFormationBatchStatus.Draft, recovered.Data!.BatchStatus);
            Assert.Equal("IFB-000001", recovered.Data.BatchNumber);
            Assert.Equal("SK_Formasi_Awal_2027_Khusus_UAT.pdf", recovered.Data.SourceDocumentFileName);
            Assert.Single(recovered.Data.Rows);
            Assert.Null(recovered.Data.SubmittedAt);
            Assert.Null(recovered.Data.WorkflowInstanceId);
            Assert.False(recovered.Data.HasWorkflowIntegrityIssue);
            Assert.Contains("Pemulihan ke Draft", recovered.Data.Notes);
            draftRowVersion = recovered.Data.RowVersion;
        }

        // Setelah pulih, batch yang sama IFB-000001 bisa langsung diajukan ke T10
        await using (var ctxResub = Konteks())
        await using (var spResub = Layanan(ctxResub, _actors["PemohonHR"].User))
        {
            var svc = spResub.GetRequiredService<HrdInitialFormationBatchService>();
            var submitted = await svc.SubmitAsync(
                stuckBatchId,
                new SubmitInitialFormationBatchRequest { RowVersion = draftRowVersion },
                _actors["PemohonHR"].User);

            Assert.True(submitted.Success, submitted.Message);
            Assert.Equal("IFB-000001", submitted.Data!.BatchNumber);
            Assert.Equal(InitialFormationBatchStatus.Submitted, submitted.Data.BatchStatus);
            Assert.NotNull(submitted.Data.WorkflowInstanceId);
            Assert.NotNull(submitted.Data.WorkflowNumber);
        }
    }

    [Fact]
    public async Task Pengembalian_revisi_dan_rekonsiliasi_workflow_berjalan_aman()
    {
        Guid batchId;
        Guid wfId;

        await using (var ctx = Konteks())
        await using (var sp = Layanan(ctx, _actors["PemohonHR"].User))
        {
            var svc = sp.GetRequiredService<HrdInitialFormationBatchService>();
            var created = await svc.CreateDraftAsync(PayloadBatch("UAT/SK-REV/01"), _actors["PemohonHR"].User);
            batchId = created.Data!.Id;
        }

        await AttachDummySkAsync(batchId);

        await using (var ctxSub = Konteks())
        await using (var spSub = Layanan(ctxSub, _actors["PemohonHR"].User))
        {
            var svc = spSub.GetRequiredService<HrdInitialFormationBatchService>();
            var cur = await svc.GetByIdAsync(batchId);
            var sub = await svc.SubmitAsync(batchId, new SubmitInitialFormationBatchRequest { RowVersion = cur!.RowVersion }, _actors["PemohonHR"].User);
            Assert.True(sub.Success, sub.Message);
            wfId = sub.Data!.WorkflowInstanceId!.Value;
        }

        // Simulasikan WorkflowInstanceId di batch sempat kosong (terputus) -> RecoverAsync merekonsiliasi tanpa duplikasi workflow
        Guid disconnectedRowVersion;
        await using (var ctxDisc = Konteks())
        {
            var b = await ctxDisc.HrdInitialFormationBatches.SingleAsync(x => x.Id == batchId);
            b.WorkflowInstanceId = null;
            b.RowVersion = Guid.NewGuid();
            disconnectedRowVersion = b.RowVersion;
            await ctxDisc.SaveChangesAsync();
        }

        await using (var ctxReconcile = Konteks())
        await using (var spReconcile = Layanan(ctxReconcile, _actors["PemohonHR"].User))
        {
            var svc = spReconcile.GetRequiredService<HrdInitialFormationBatchService>();
            var rec = await svc.RecoverAsync(
                batchId,
                new RecoverInitialFormationBatchRequest
                {
                    Reason = "Rekonsiliasi referensi workflow T10 yang sudah ada di database.",
                    RowVersion = disconnectedRowVersion
                },
                _actors["PemohonHR"].User);

            Assert.True(rec.Success, rec.Message);
            Assert.Equal(wfId, rec.Data!.WorkflowInstanceId);
            Assert.Equal(InitialFormationBatchStatus.Submitted, rec.Data.BatchStatus);
            Assert.Equal(1, await ctxReconcile.TrxWorkflowInstances.CountAsync(w => !w.IsDelete && w.ReferenceId == batchId));
        }

        // HR Manager minta revisi -> Batch status menjadi Returned, tidak ada formasi terbentuk
        await using (var ctxRev = Konteks())
        await using (var spRev = Layanan(ctxRev, _actors["HrManager"].User))
        {
            var assignId = OpenAssignmentForUser(wfId, _actors["HrManager"].User);
            var res = await spRev.GetRequiredService<WorkflowService>().RequestRevisionAsync(
                wfId,
                assignId!.Value,
                new WorkflowRequestRevisionRequest { Comment = "Lengkapi keterangan SK formasi." });
            Assert.True(res.Success, res.Message);
        }

        await using (var ctxCheckRet = Konteks())
        await using (var spCheckRet = Layanan(ctxCheckRet, _actors["PemohonHR"].User))
        {
            var svc = spCheckRet.GetRequiredService<HrdInitialFormationBatchService>();
            var retDetail = await svc.GetByIdAsync(batchId);
            Assert.Equal(InitialFormationBatchStatus.Returned, retDetail!.BatchStatus);
            Assert.Equal(0, await ctxCheckRet.HrdPositionFormations.CountAsync(f => f.SourceDocumentId == batchId));

            // Ajukan ulang menggunakan instance yang sama
            var resub = await svc.SubmitAsync(batchId, new SubmitInitialFormationBatchRequest { RowVersion = retDetail.RowVersion, Notes = "Sudah dilengkapi" }, _actors["PemohonHR"].User);
            Assert.True(resub.Success, resub.Message);
            Assert.Equal(wfId, resub.Data!.WorkflowInstanceId);
            Assert.Equal(InitialFormationBatchStatus.Submitted, resub.Data.BatchStatus);
        }
    }
}
