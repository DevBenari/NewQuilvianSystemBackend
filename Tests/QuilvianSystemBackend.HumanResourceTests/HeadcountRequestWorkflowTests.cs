using System.Data.Common;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Services;
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

// PLAN-REPAIR-009 (M1–M4): uji integrasi alur Headcount Request (T11/T12) pada mesin workflow nyata
// (WorkflowService + HrdHeadcountRequestService + HrdHeadcountRequestDecisionApplier + JobRequisitionService).
public sealed class HeadcountRequestWorkflowTests : IDisposable
{
    private readonly TestDatabase _db = TestDatabase.Create();
    private readonly Guid _hospitalSiteId = Guid.NewGuid();
    private readonly Guid _unitId = Guid.NewGuid();
    private readonly Guid _positionId = Guid.NewGuid();
    private readonly Guid _salaryGradeId = Guid.NewGuid();
    private readonly Guid _jabatanSdm = Guid.NewGuid();
    private readonly Dictionary<string, (Guid Wf, Guid User)> _actors = new();
    private readonly Guid _defT11Id = Guid.NewGuid();
    private readonly Guid _defT12Id = Guid.NewGuid();

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

        public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand c, CommandEventData e, InterceptionResult<DbDataReader> r)
        {
            Adapt(c);
            return r;
        }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand c, CommandEventData e, InterceptionResult<DbDataReader> r, CancellationToken t = default)
        {
            Adapt(c);
            return ValueTask.FromResult(r);
        }

        public override InterceptionResult<int> NonQueryExecuting(DbCommand c, CommandEventData e, InterceptionResult<int> r)
        {
            Adapt(c);
            return r;
        }

        public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand c, CommandEventData e, InterceptionResult<int> r, CancellationToken t = default)
        {
            Adapt(c);
            return ValueTask.FromResult(r);
        }
    }

    private sealed class SqliteContextFactory(TestDatabase db) : IDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext() => db.CreateContext(new PostgresKeSqliteInterceptor());
    }

    public HeadcountRequestWorkflowTests()
    {
        using var ctx = _db.CreateContext();
        var start = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        ctx.MstHospitalSites.Add(new MstHospitalSite
        {
            Id = _hospitalSiteId,
            SiteCode = "RS-UJI",
            SiteName = "RS MMC Uji",
            IsActive = true
        });
        ctx.MstOrganizationUnits.Add(new MstOrganizationUnit
        {
            Id = _unitId,
            UnitCode = "ICU",
            UnitName = "Unit Perawatan Intensif (ICU)",
            IsActive = true
        });
        ctx.MstPositions.Add(new MstPosition
        {
            Id = _positionId,
            PositionCode = "PRW-ICU",
            PositionName = "Perawat Pelaksana ICU",
            IsActive = true
        });
        ctx.MstSalaryGrades.Add(new MstSalaryGrade
        {
            Id = _salaryGradeId,
            SalaryGradeCode = "GRD-03",
            SalaryGradeName = "Grade Perawat Madya",
            CurrencyCode = "IDR",
            MinimumSalary = 6000000m,
            MidpointSalary = 7500000m,
            MaximumSalary = 9000000m,
            EffectiveStartDate = start,
            IsActive = true
        });

        foreach (var (nama, jabatan) in new[] { ("Yudha", Guid.NewGuid()), ("Hartini", _jabatanSdm) })
        {
            var wf = Guid.NewGuid();
            var user = Guid.NewGuid();
            _actors[nama] = (wf, user);
            ctx.MstWorkforceProfiles.Add(new MstWorkforceProfile
            {
                Id = wf,
                ProfileCode = "P-" + nama,
                DisplayName = nama,
                UserType = UserType.Employee,
                IsActive = true
            });
            ctx.Users.Add(new ApplicationUser
            {
                Id = user,
                UserName = nama,
                UserCode = nama.ToUpperInvariant().PadRight(8, 'X')[..8],
                DisplayName = nama,
                UserType = UserType.Employee,
                WorkforceProfileId = wf,
                IsActive = true
            });
            ctx.WfpOrganizationAssignments.Add(new WfpOrganizationAssignment
            {
                WorkforceProfileId = wf,
                HospitalSiteId = _hospitalSiteId,
                OrganizationUnitId = _unitId,
                DepartmentId = Guid.NewGuid(),
                PositionId = jabatan,
                AssignmentType = "Primary",
                IsPrimary = true,
                EffectiveStartDate = start,
                IsActive = true
            });
        }

        foreach (var (id, kode, jenis) in new[]
        {
            (_defT11Id, "T11", "HeadcountRequestWithinFormation"),
            (_defT12Id, "T12", "HeadcountRequestBeyondFormation")
        })
        {
            ctx.MstWorkflowDefinitions.Add(new MstWorkflowDefinition
            {
                Id = id,
                WorkflowCode = kode,
                WorkflowName = "Uji " + kode,
                RequestType = jenis,
                WorkflowCategory = "WorkforcePlanning",
                Version = 1,
                WorkflowStatus = "Active",
                IsActive = true,
                AllowRequesterWithdraw = true,
                AllowRequesterCancel = true
            });
            ctx.Set<MstWorkflowStep>().Add(new MstWorkflowStep
            {
                WorkflowDefinitionId = id,
                StepCode = kode + "_SDM",
                StepName = "Kepala Bagian SDM",
                StepOrder = 1,
                StepType = WorkflowValueConstants.StepType.Approval,
                ApprovalMode = WorkflowValueConstants.ApprovalMode.Any,
                RequiredApprovalCount = 1,
                ApproverSourceType = WorkflowValueConstants.ApproverSource.Position,
                ApproverPositionId = _jabatanSdm,
                IsRequired = true,
                AllowSelfApproval = false,
                IsActive = true
            });
        }

        ctx.SaveChanges();
    }

    public void Dispose() => _db.Dispose();

    private ApplicationDbContext Konteks() => _db.CreateContext(new PostgresKeSqliteInterceptor());

    private static ClaimsPrincipal Principal(Guid userId) =>
        new(new ClaimsIdentity(new[] { new Claim("user_id", userId.ToString()) }, "Uji"));

    private ServiceProvider Layanan(ApplicationDbContext ctx, Guid actorUserId)
    {
        var services = new ServiceCollection();
        services.AddSingleton(ctx);
        services.AddSingleton<IHrdWorkforcePlanningSettingService>(new HrdWorkforcePlanningSettingService(ctx, NullLogger<HrdWorkforcePlanningSettingService>.Instance));
        services.AddSingleton<IHrdWorkforcePlanningCostService>(new HrdWorkforcePlanningCostService(ctx, NullLogger<HrdWorkforcePlanningCostService>.Instance));
        services.AddSingleton<IHrdStaffingGapCalculationService>(sp =>
            new HrdStaffingGapCalculationService(ctx, sp.GetRequiredService<IHrdWorkforcePlanningSettingService>()));
        services.AddSingleton<IHrdStaffingGapSnapshotService>(sp =>
            new HrdStaffingGapSnapshotService(ctx, sp.GetRequiredService<IHrdStaffingGapCalculationService>()));
        services.AddSingleton<IHrdPositionFormationService>(new HrdPositionFormationService(ctx, NullLogger<HrdPositionFormationService>.Instance));
        services.AddSingleton<IWorkforcePlanningDecisionApplier>(sp =>
            new HrdHeadcountRequestDecisionApplier(ctx, sp.GetRequiredService<IHrdPositionFormationService>(), NullLogger<HrdHeadcountRequestDecisionApplier>.Instance));
        services.AddSingleton<IHrdWorkforcePlanningWorkflowIntegrationService>(sp =>
            new HrdWorkforcePlanningWorkflowIntegrationService(
                ctx,
                sp,
                sp.GetServices<IWorkforcePlanningDecisionApplier>(),
                NullLogger<HrdWorkforcePlanningWorkflowIntegrationService>.Instance));
        services.AddSingleton(sp =>
            new WorkflowReferenceLifecycleService(
                ctx,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
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
            new HrdHeadcountRequestService(
                ctx,
                sp.GetRequiredService<IHrdWorkforcePlanningCostService>(),
                sp.GetRequiredService<IHrdStaffingGapSnapshotService>(),
                sp.GetRequiredService<IHrdWorkforcePlanningWorkflowIntegrationService>(),
                new NumberSeriesAllocator(new SqliteContextFactory(_db))));
        services.AddSingleton(_ => new JobRequisitionService(ctx));
        return services.BuildServiceProvider();
    }

    private CreateHeadcountRequest PayloadHeadcount(
        HeadcountRequestType type = HeadcountRequestType.AdditionalBeyondFormation,
        int externalSeats = 2,
        int internalSeats = 0)
    {
        var seats = new List<HeadcountSeatRequest>();
        var seatNo = 1;
        for (var i = 0; i < externalSeats; i++)
        {
            seats.Add(new HeadcountSeatRequest { SeatNumber = seatNo++, SeatFte = 1.00m, FulfilmentSource = HeadcountFulfilmentSource.External });
        }
        for (var i = 0; i < internalSeats; i++)
        {
            seats.Add(new HeadcountSeatRequest { SeatNumber = seatNo++, SeatFte = 1.00m, FulfilmentSource = HeadcountFulfilmentSource.Internal });
        }

        return new CreateHeadcountRequest
        {
            HospitalSiteId = _hospitalSiteId,
            OrganizationUnitId = _unitId,
            PositionId = _positionId,
            SalaryGradeId = _salaryGradeId,
            RequestType = type,
            RequiredStartDate = new DateOnly(2026, 12, 1),
            Justification = "Penambahan tenaga perawatan ICU untuk ruang baru.",
            ExternalSeatCount = externalSeats,
            InternalSeatCount = internalSeats,
            Seats = seats
        };
    }

    private Guid? TugasHartiniTerbuka(Guid workflowId)
    {
        using var ctx = _db.CreateContext();
        return ctx.TrxWorkflowApproverAssignments.AsNoTracking()
            .Where(x => x.WorkflowInstanceId == workflowId &&
                        x.AssignedApproverUserId == _actors["Hartini"].User &&
                        x.IsCurrentAssignment &&
                        x.AssignmentStatus != WorkflowValueConstants.AssignmentStatus.Approved &&
                        x.AssignmentStatus != WorkflowValueConstants.AssignmentStatus.Completed &&
                        x.AssignmentStatus != WorkflowValueConstants.AssignmentStatus.Cancelled &&
                        x.AssignmentStatus != WorkflowValueConstants.AssignmentStatus.Rejected)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefault();
    }

    [Fact]
    public async Task Ajukan_T12_dan_setujui_mengubah_status_ke_ReadyForRecruitment_dan_muncul_di_EligibleForRequisitionOnly()
    {
        Guid headcountId;
        Guid workflowId;

        await using (var ctx = Konteks())
        await using (var sp = Layanan(ctx, _actors["Yudha"].User))
        {
            var svc = sp.GetRequiredService<HrdHeadcountRequestService>();
            var created = await svc.CreateDraftAsync(PayloadHeadcount(HeadcountRequestType.AdditionalBeyondFormation, externalSeats: 2), Principal(_actors["Yudha"].User));
            Assert.True(created.Success, created.Message);
            headcountId = created.Data!.Id;

            var submitted = await svc.SubmitAsync(headcountId, new SubmitHeadcountRequestDto { Note = "Mohon persetujuan." }, Principal(_actors["Yudha"].User));
            Assert.True(submitted.Success, submitted.Message);
            Assert.Equal(HeadcountRequestStatus.Submitted, submitted.Data!.RequestStatus);
            Assert.NotNull(submitted.Data.WorkflowInstanceId);
            workflowId = submitted.Data.WorkflowInstanceId!.Value;
        }

        // Pastikan RequestContext memuat summary dan tautan dokumen
        await using (var ctxCheck = Konteks())
        {
            var wf = await ctxCheck.TrxWorkflowInstances.AsNoTracking().SingleAsync(w => w.Id == workflowId);
            Assert.Contains("\"summary\"", wf.RequestContextJson);
            Assert.Contains("Perawat Pelaksana ICU", wf.RequestContextJson);
            Assert.Contains(headcountId.ToString(), wf.RequestContextJson);
        }

        // Hartini menyetujui -> WorkflowStatus menjadi Completed -> Applier mengubah ke ReadyForRecruitment
        await using (var ctxApprove = Konteks())
        await using (var spApprove = Layanan(ctxApprove, _actors["Hartini"].User))
        {
            var tugas = TugasHartiniTerbuka(workflowId);
            Assert.NotNull(tugas);
            var approved = await spApprove.GetRequiredService<WorkflowService>().ApproveAsync(workflowId, tugas!.Value, new WorkflowApproveRequest { Comment = "Disetujui." });
            Assert.True(approved.Success, approved.Message);
            Assert.Equal(WorkflowValueConstants.WorkflowStatus.Completed, approved.Data!.WorkflowStatus);
        }

        // Cek status headcount, formasi baru, filter EligibleForRequisitionOnly, pencarian nama jabatan, dan alokasi kursi requisition
        await using (var ctxAfter = Konteks())
        await using (var spAfter = Layanan(ctxAfter, _actors["Yudha"].User))
        {
            var hcSvc = spAfter.GetRequiredService<HrdHeadcountRequestService>();
            var reqSvc = spAfter.GetRequiredService<JobRequisitionService>();

            var detail = await hcSvc.GetByIdAsync(headcountId, Principal(_actors["Yudha"].User));
            Assert.True(detail.Success);
            Assert.Equal(HeadcountRequestStatus.ReadyForRecruitment, detail.Data!.RequestStatus);
            Assert.NotNull(detail.Data.ReadyForRecruitmentAt);

            // Search berdasarkan nama jabatan + filter EligibleForRequisitionOnly
            var optionsBefore = await hcSvc.GetPagedListAsync(
                new HeadcountRequestPagedQuery { EligibleForRequisitionOnly = true, Search = "Perawat" },
                Principal(_actors["Yudha"].User));
            Assert.True(optionsBefore.Success);
            var item = Assert.Single(optionsBefore.Data!.Items);
            Assert.Equal(2, item.ExternalSeatCount);
            Assert.Equal(2, item.RemainingExternalSeats);

            // Buat Requisition pertama untuk 1 kursi -> sisa 1 kursi -> masih tampil
            var r1 = await reqSvc.CreateAsync(new CreateJobRequisitionRequest
            {
                HeadcountRequestId = headcountId,
                HospitalSiteId = _hospitalSiteId,
                OrganizationUnitId = _unitId,
                PositionId = _positionId,
                RequestedSeats = 1
            }, Principal(_actors["Yudha"].User));
            Assert.True(r1.Success, r1.Message);

            var optionsPartial = await hcSvc.GetPagedListAsync(
                new HeadcountRequestPagedQuery { EligibleForRequisitionOnly = true },
                Principal(_actors["Yudha"].User));
            Assert.Equal(1, Assert.Single(optionsPartial.Data!.Items).RemainingExternalSeats);

            // Buat Requisition kedua untuk 1 kursi sisa -> kuota habis -> tidak lagi tampil
            var r2 = await reqSvc.CreateAsync(new CreateJobRequisitionRequest
            {
                HeadcountRequestId = headcountId,
                HospitalSiteId = _hospitalSiteId,
                OrganizationUnitId = _unitId,
                PositionId = _positionId,
                RequestedSeats = 1
            }, Principal(_actors["Yudha"].User));
            Assert.True(r2.Success, r2.Message);

            var optionsAfter = await hcSvc.GetPagedListAsync(
                new HeadcountRequestPagedQuery { EligibleForRequisitionOnly = true },
                Principal(_actors["Yudha"].User));
            Assert.Empty(optionsAfter.Data!.Items);
        }
    }

    [Fact]
    public async Task Permintaan_hanya_kursi_internal_tidak_muncul_di_EligibleForRequisitionOnly()
    {
        Guid headcountId;
        Guid workflowId;

        await using (var ctx = Konteks())
        await using (var sp = Layanan(ctx, _actors["Yudha"].User))
        {
            var svc = sp.GetRequiredService<HrdHeadcountRequestService>();
            var created = await svc.CreateDraftAsync(PayloadHeadcount(HeadcountRequestType.AdditionalBeyondFormation, externalSeats: 0, internalSeats: 1), Principal(_actors["Yudha"].User));
            Assert.True(created.Success, created.Message);
            headcountId = created.Data!.Id;
            var submitted = await svc.SubmitAsync(headcountId, new SubmitHeadcountRequestDto(), Principal(_actors["Yudha"].User));
            Assert.True(submitted.Success, submitted.Message);
            workflowId = submitted.Data!.WorkflowInstanceId!.Value;
        }

        await using (var ctxApprove = Konteks())
        await using (var spApprove = Layanan(ctxApprove, _actors["Hartini"].User))
        {
            var tugas = TugasHartiniTerbuka(workflowId);
            Assert.True((await spApprove.GetRequiredService<WorkflowService>().ApproveAsync(workflowId, tugas!.Value, new WorkflowApproveRequest())).Success);
        }

        await using (var ctxAfter = Konteks())
        await using (var spAfter = Layanan(ctxAfter, _actors["Yudha"].User))
        {
            var svc = spAfter.GetRequiredService<HrdHeadcountRequestService>();
            var allReady = await svc.GetPagedListAsync(new HeadcountRequestPagedQuery { Status = HeadcountRequestStatus.ReadyForRecruitment }, Principal(_actors["Yudha"].User));
            Assert.Single(allReady.Data!.Items);

            var eligible = await svc.GetPagedListAsync(new HeadcountRequestPagedQuery { EligibleForRequisitionOnly = true }, Principal(_actors["Yudha"].User));
            Assert.Empty(eligible.Data!.Items);
        }
    }

    [Fact]
    public async Task Minta_revisi_menjadi_Returned_lalu_ajukan_ulang_memakai_instance_sama_dan_lulus()
    {
        await using (var ctxSeed = Konteks())
        {
            ctxSeed.HrdPositionFormations.Add(new HrdPositionFormation
            {
                Id = Guid.NewGuid(),
                HospitalSiteId = _hospitalSiteId,
                OrganizationUnitId = _unitId,
                PositionId = _positionId,
                VersionNumber = 1,
                FormationQuantity = 5m,
                EffectiveStartDate = new DateOnly(2026, 1, 1),
                SourceType = PositionFormationSourceType.InitialBatch,
                SourceDocumentId = Guid.NewGuid(),
                SourceDocumentNumber = "IFB-001",
                ApprovedByUserId = _actors["Hartini"].User,
                ApprovedAt = DateTime.UtcNow
            });
            await ctxSeed.SaveChangesAsync();
        }

        Guid headcountId;
        Guid workflowId;

        await using (var ctx = Konteks())
        await using (var sp = Layanan(ctx, _actors["Yudha"].User))
        {
            var svc = sp.GetRequiredService<HrdHeadcountRequestService>();
            var created = await svc.CreateDraftAsync(PayloadHeadcount(HeadcountRequestType.AdditionalWithinFormation, externalSeats: 1), Principal(_actors["Yudha"].User));
            Assert.True(created.Success, created.Message);
            headcountId = created.Data!.Id;
            var submitted = await svc.SubmitAsync(headcountId, new SubmitHeadcountRequestDto(), Principal(_actors["Yudha"].User));
            Assert.True(submitted.Success, submitted.Message);
            workflowId = submitted.Data!.WorkflowInstanceId!.Value;
        }

        await using (var ctxRev = Konteks())
        await using (var spRev = Layanan(ctxRev, _actors["Hartini"].User))
        {
            var tugas = TugasHartiniTerbuka(workflowId);
            var rev = await spRev.GetRequiredService<WorkflowService>().RequestRevisionAsync(workflowId, tugas!.Value, new WorkflowRequestRevisionRequest { Comment = "Perjelas alasan." });
            Assert.True(rev.Success, rev.Message);
        }

        await using (var ctxResub = Konteks())
        await using (var spResub = Layanan(ctxResub, _actors["Yudha"].User))
        {
            var svc = spResub.GetRequiredService<HrdHeadcountRequestService>();
            var before = await svc.GetByIdAsync(headcountId, Principal(_actors["Yudha"].User));
            Assert.Equal(HeadcountRequestStatus.Returned, before.Data!.RequestStatus);

            var resubmitted = await svc.SubmitAsync(headcountId, new SubmitHeadcountRequestDto { Note = "Sudah diperjelas." }, Principal(_actors["Yudha"].User));
            Assert.True(resubmitted.Success, resubmitted.Message);
            Assert.Equal(HeadcountRequestStatus.Submitted, resubmitted.Data!.RequestStatus);
            Assert.Equal(workflowId, resubmitted.Data.WorkflowInstanceId);
        }

        await using (var ctxApprove = Konteks())
        await using (var spApprove = Layanan(ctxApprove, _actors["Hartini"].User))
        {
            var tugas = TugasHartiniTerbuka(workflowId);
            Assert.NotNull(tugas);
            Assert.True((await spApprove.GetRequiredService<WorkflowService>().ApproveAsync(workflowId, tugas!.Value, new WorkflowApproveRequest())).Success);
        }

        await using (var ctxFinal = Konteks())
        {
            var doc = await ctxFinal.HrdHeadcountRequests.AsNoTracking().SingleAsync(r => r.Id == headcountId);
            Assert.Equal(HeadcountRequestStatus.ReadyForRecruitment, doc.RequestStatus);
        }
    }

    [Fact]
    public async Task Tarik_kembali_menarik_instance_workflow_dan_gagal_tertutup_bila_step_kosong()
    {
        Guid hcWithdrawId;
        Guid wfWithdrawId;

        await using (var ctx = Konteks())
        await using (var sp = Layanan(ctx, _actors["Yudha"].User))
        {
            var svc = sp.GetRequiredService<HrdHeadcountRequestService>();
            var created = await svc.CreateDraftAsync(PayloadHeadcount(HeadcountRequestType.AdditionalBeyondFormation, externalSeats: 1), Principal(_actors["Yudha"].User));
            hcWithdrawId = created.Data!.Id;
            var submitted = await svc.SubmitAsync(hcWithdrawId, new SubmitHeadcountRequestDto(), Principal(_actors["Yudha"].User));
            Assert.True(submitted.Success, submitted.Message);
            wfWithdrawId = submitted.Data!.WorkflowInstanceId!.Value;

            var withdrawn = await svc.WithdrawAsync(hcWithdrawId, new WithdrawHeadcountRequestDto { Reason = "Kebutuhan dibatalkan unit." }, Principal(_actors["Yudha"].User));
            Assert.True(withdrawn.Success, withdrawn.Message);
            Assert.Equal(HeadcountRequestStatus.Withdrawn, withdrawn.Data!.RequestStatus);
        }

        await using (var ctxCheckWf = Konteks())
        {
            var wf = await ctxCheckWf.TrxWorkflowInstances.AsNoTracking().SingleAsync(w => w.Id == wfWithdrawId);
            Assert.Equal(WorkflowValueConstants.WorkflowStatus.Withdrawn, wf.WorkflowStatus);
        }

        // Nonaktifkan step T12 lalu uji gagal tertutup: status tetap Draft dan snapshot terhapus lunak
        await using (var ctxDeactivate = Konteks())
        {
            var step = await ctxDeactivate.Set<MstWorkflowStep>().SingleAsync(s => s.WorkflowDefinitionId == _defT12Id);
            step.IsActive = false;
            await ctxDeactivate.SaveChangesAsync();
        }

        await using (var ctxFail = Konteks())
        await using (var spFail = Layanan(ctxFail, _actors["Yudha"].User))
        {
            var svc = spFail.GetRequiredService<HrdHeadcountRequestService>();
            var created = await svc.CreateDraftAsync(PayloadHeadcount(HeadcountRequestType.AdditionalBeyondFormation, externalSeats: 1), Principal(_actors["Yudha"].User));
            var failed = await svc.SubmitAsync(created.Data!.Id, new SubmitHeadcountRequestDto(), Principal(_actors["Yudha"].User));
            Assert.False(failed.Success);
            Assert.Equal(400, failed.StatusCode);

            var reloaded = await svc.GetByIdAsync(created.Data.Id, Principal(_actors["Yudha"].User));
            Assert.Equal(HeadcountRequestStatus.Draft, reloaded.Data!.RequestStatus);
            var activeSnapshots = await ctxFail.HrdStaffingGapSnapshots.CountAsync(s => s.SourceDocumentId == created.Data.Id && !s.IsDelete);
            Assert.Equal(0, activeSnapshots);
        }
    }
}
