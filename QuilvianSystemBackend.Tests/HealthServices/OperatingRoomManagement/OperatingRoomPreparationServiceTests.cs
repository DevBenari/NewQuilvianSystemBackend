using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using Xunit;

namespace QuilvianSystemBackend.Tests.HealthServices.OperatingRoomManagement;

public class OperatingRoomPreparationServiceTests
{
    [Fact]
    public async Task GetAsync_FreshScheduledCase_ListsOutstandingChecklistAndSignOffs()
    {
        await using var fixture = await TestFixture.CreateAsync();
        var result = await fixture.Service.GetAsync(fixture.CaseId);

        Assert.NotNull(result);
        Assert.Equal(OprCaseStatus.Scheduled, result!.Status);
        Assert.All(result.Consents, x => Assert.True(x.IsValid));
        Assert.Contains("Checklist verifikasi sebelum anestesi belum selesai.", result.OutstandingRequirements);
        Assert.Equal(3, result.OutstandingRequirements.Count(x => x.StartsWith("Sign-off")));
        Assert.False(result.IsEmergencyBypassActive);
    }

    [Fact]
    public async Task ThreeSignOffsAfterChecklist_MovesCaseToReadyExactlyOnce()
    {
        await using var fixture = await TestFixture.CreateAsync();
        await fixture.CompleteSignInChecklistAsync();
        var afterSecond = await fixture.SignOffAsync(OprReadinessRole.PrimarySurgeon, "sign-1");
        Assert.Equal(OprCaseStatus.Scheduled, afterSecond.Status);

        await fixture.SignOffAsync(OprReadinessRole.Anesthesiologist, "sign-2");
        var final = await fixture.SignOffAsync(OprReadinessRole.Nurse, "sign-3");

        Assert.Equal(OprCaseStatus.Ready, final.Status);
        Assert.Empty(final.OutstandingRequirements);
        Assert.Equal(3, final.SignOffs.Count);
        Assert.Equal(1, await fixture.Context.OprStatusHistories
            .CountAsync(x => x.ToStatus == OprCaseStatus.Ready && x.Action == "CompleteReadiness"));
    }

    [Fact]
    public async Task SignOffs_WithoutChecklist_DoesNotReachReady()
    {
        await using var fixture = await TestFixture.CreateAsync();
        await fixture.SignOffAsync(OprReadinessRole.PrimarySurgeon, "sign-1");
        await fixture.SignOffAsync(OprReadinessRole.Anesthesiologist, "sign-2");
        var result = await fixture.SignOffAsync(OprReadinessRole.Nurse, "sign-3");

        Assert.Equal(OprCaseStatus.Scheduled, result.Status);
        Assert.Contains("Checklist verifikasi sebelum anestesi belum selesai.", result.OutstandingRequirements);
    }

    [Fact]
    public async Task SignOffs_WithInvalidConsent_DoesNotReachReady()
    {
        await using var fixture = await TestFixture.CreateAsync();
        var consent = await fixture.Context.Set<TrxPatientConsent>()
            .FirstAsync(x => x.ConsentType == PatientConsentType.Anesthesia);
        consent.ConsentStatus = PatientConsentStatus.Withdrawn;
        await fixture.Context.SaveChangesAsync();

        await fixture.CompleteSignInChecklistAsync();
        await fixture.SignOffAsync(OprReadinessRole.PrimarySurgeon, "sign-1");
        await fixture.SignOffAsync(OprReadinessRole.Anesthesiologist, "sign-2");
        var result = await fixture.SignOffAsync(OprReadinessRole.Nurse, "sign-3");

        Assert.Equal(OprCaseStatus.Scheduled, result.Status);
        Assert.Contains("Consent tindakan anestesi belum sah.", result.OutstandingRequirements);
    }

    [Fact]
    public async Task SignOff_ByUserOutsideTeamRole_IsForbidden()
    {
        await using var fixture = await TestFixture.CreateAsync();
        fixture.ActAs(fixture.ScrubNurseUserId);

        await Assert.ThrowsAsync<OperatingRoomForbiddenException>(() =>
            fixture.Service.CreateSignOffAsync(fixture.CaseId, new CreateOprReadinessSignOffRequest
            {
                Role = OprReadinessRole.PrimarySurgeon,
                IdempotencyKey = "wrong-actor",
                ExpectedVersion = 0
            }));
    }

    [Fact]
    public async Task SignOff_SameRoleTwice_RejectsOpr006()
    {
        await using var fixture = await TestFixture.CreateAsync();
        await fixture.SignOffAsync(OprReadinessRole.PrimarySurgeon, "sign-1");
        fixture.ActAs(fixture.SurgeonUserId);
        var version = await fixture.CurrentVersionAsync();

        var exception = await Assert.ThrowsAsync<OperatingRoomConflictException>(() =>
            fixture.Service.CreateSignOffAsync(fixture.CaseId, new CreateOprReadinessSignOffRequest
            {
                Role = OprReadinessRole.PrimarySurgeon,
                IdempotencyKey = "sign-again",
                ExpectedVersion = version
            }));

        Assert.Equal("OPR006", exception.Code);
    }

    [Fact]
    public async Task SignOff_SameIdempotencyKey_IsRepeatable()
    {
        await using var fixture = await TestFixture.CreateAsync();
        var first = await fixture.SignOffAsync(OprReadinessRole.PrimarySurgeon, "repeat-key");
        fixture.ActAs(fixture.SurgeonUserId);
        var second = await fixture.Service.CreateSignOffAsync(fixture.CaseId, new CreateOprReadinessSignOffRequest
        {
            Role = OprReadinessRole.PrimarySurgeon, IdempotencyKey = "repeat-key", ExpectedVersion = 0
        });

        Assert.Equal(first.Version, second.Version);
        Assert.Equal(1, await fixture.Context.OprStatusHistories.CountAsync(x => x.Action == "ReadinessSignOff"));
    }

    [Fact]
    public async Task SignOff_StaleVersion_RejectsOpr012()
    {
        await using var fixture = await TestFixture.CreateAsync();
        fixture.ActAs(fixture.SurgeonUserId);

        var exception = await Assert.ThrowsAsync<OperatingRoomConflictException>(() =>
            fixture.Service.CreateSignOffAsync(fixture.CaseId, new CreateOprReadinessSignOffRequest
            {
                Role = OprReadinessRole.PrimarySurgeon, IdempotencyKey = "stale", ExpectedVersion = 9
            }));

        Assert.Equal("OPR012", exception.Code);
    }

    [Fact]
    public async Task SaveChecklist_MissingMandatoryItem_RejectsOpr006()
    {
        await using var fixture = await TestFixture.CreateAsync();
        var request = fixture.ChecklistRequest("incomplete", complete: true);
        request.Items[0].IsChecked = false;

        var exception = await Assert.ThrowsAsync<OperatingRoomUnprocessableException>(() =>
            fixture.Service.SaveChecklistAsync(fixture.CaseId, OprChecklistPhase.SignIn, request));

        Assert.Equal("OPR006", exception.Code);
    }

    [Fact]
    public async Task SaveChecklist_DraftThenComplete_KeepsSingleRevision()
    {
        await using var fixture = await TestFixture.CreateAsync();
        var draft = await fixture.Service.SaveChecklistAsync(fixture.CaseId, OprChecklistPhase.SignIn,
            fixture.ChecklistRequest("draft-1", complete: false));
        Assert.Equal(OprChecklistStatus.Draft, draft.Status);
        Assert.Equal(1, draft.Revision);

        var completed = await fixture.Service.SaveChecklistAsync(fixture.CaseId, OprChecklistPhase.SignIn,
            fixture.ChecklistRequest("draft-2", complete: true, expectedVersion: await fixture.CurrentVersionAsync()));

        Assert.Equal(OprChecklistStatus.Completed, completed.Status);
        Assert.Equal(1, completed.Revision);
        Assert.Equal(3, completed.Items.Count);
        Assert.Equal(1, await fixture.Context.OprSafetyChecklists.CountAsync());
    }

    [Fact]
    public async Task SaveChecklist_AfterCompletion_CreatesNewRevision()
    {
        await using var fixture = await TestFixture.CreateAsync();
        await fixture.CompleteSignInChecklistAsync();

        var revised = await fixture.Service.SaveChecklistAsync(fixture.CaseId, OprChecklistPhase.SignIn,
            fixture.ChecklistRequest("revise", complete: false, expectedVersion: await fixture.CurrentVersionAsync()));

        Assert.Equal(2, revised.Revision);
        Assert.Equal(OprChecklistStatus.Draft, revised.Status);
        Assert.Equal(2, await fixture.Context.OprSafetyChecklists.CountAsync());
    }

    [Fact]
    public async Task SaveChecklist_TimeOutPhaseWhileScheduled_RejectsInvalidStateTransition()
    {
        await using var fixture = await TestFixture.CreateAsync();

        var exception = await Assert.ThrowsAsync<OperatingRoomConflictException>(() =>
            fixture.Service.SaveChecklistAsync(fixture.CaseId, OprChecklistPhase.TimeOut,
                fixture.ChecklistRequest("timeout-early", complete: false)));

        Assert.Equal("InvalidStateTransition", exception.Code);
    }

    [Fact]
    public async Task EmergencyBypass_OnElectiveCase_RejectsOpr007()
    {
        await using var fixture = await TestFixture.CreateAsync();

        var exception = await Assert.ThrowsAsync<OperatingRoomUnprocessableException>(() =>
            fixture.Service.CreateEmergencyBypassAsync(fixture.CaseId, new CreateOprEmergencyBypassRequest
            {
                Reason = "Pasien tidak stabil.",
                ResponsibleUserId = fixture.SurgeonUserId,
                IdempotencyKey = "bypass-elective",
                ExpectedVersion = 0
            }));

        Assert.Equal("OPR007", exception.Code);
    }

    [Fact]
    public async Task EmergencyBypass_WithoutResponsibleUser_RejectsOpr007()
    {
        await using var fixture = await TestFixture.CreateAsync(emergency: true);

        var exception = await Assert.ThrowsAsync<OperatingRoomUnprocessableException>(() =>
            fixture.Service.CreateEmergencyBypassAsync(fixture.CaseId, new CreateOprEmergencyBypassRequest
            {
                Reason = "Pasien tidak stabil.",
                ResponsibleUserId = Guid.Empty,
                IdempotencyKey = "bypass-no-owner",
                ExpectedVersion = 0
            }));

        Assert.Equal("OPR007", exception.Code);
    }

    [Fact]
    public async Task EmergencyBypass_WaivesConsentAndChecklistButNotSignOffs()
    {
        await using var fixture = await TestFixture.CreateAsync(emergency: true, withConsents: false);
        var afterBypass = await fixture.Service.CreateEmergencyBypassAsync(fixture.CaseId, new CreateOprEmergencyBypassRequest
        {
            Reason = "Perdarahan aktif, operasi tidak dapat ditunda.",
            ResponsibleUserId = fixture.SurgeonUserId,
            IdempotencyKey = "bypass-ok",
            ExpectedVersion = 0
        });

        Assert.True(afterBypass.IsEmergencyBypassActive);
        Assert.Equal(OprCaseStatus.Scheduled, afterBypass.Status);
        Assert.DoesNotContain(afterBypass.OutstandingRequirements, x => x.StartsWith("Consent"));
        Assert.DoesNotContain(afterBypass.OutstandingRequirements, x => x.StartsWith("Checklist"));
        Assert.Equal(3, afterBypass.OutstandingRequirements.Count);

        await fixture.SignOffAsync(OprReadinessRole.PrimarySurgeon, "e-sign-1");
        await fixture.SignOffAsync(OprReadinessRole.Anesthesiologist, "e-sign-2");
        var final = await fixture.SignOffAsync(OprReadinessRole.Nurse, "e-sign-3");

        Assert.Equal(OprCaseStatus.Ready, final.Status);
    }

    [Fact]
    public async Task BypassedChecklist_CompletedLater_RecordsStableTimestamp()
    {
        await using var fixture = await TestFixture.CreateAsync(emergency: true, withConsents: false);
        await fixture.Service.CreateEmergencyBypassAsync(fixture.CaseId, new CreateOprEmergencyBypassRequest
        {
            Reason = "Perdarahan aktif.",
            ResponsibleUserId = fixture.SurgeonUserId,
            IdempotencyKey = "bypass-then-complete",
            ExpectedVersion = 0
        });

        var completed = await fixture.Service.SaveChecklistAsync(fixture.CaseId, OprChecklistPhase.SignIn,
            fixture.ChecklistRequest("complete-after-stable", complete: true,
                expectedVersion: await fixture.CurrentVersionAsync()));

        Assert.True(completed.IsEmergencyBypass);
        Assert.Equal(OprChecklistStatus.Completed, completed.Status);
        Assert.NotNull(completed.CompletedAfterStableAt);
    }

    private sealed class TestFixture : IAsyncDisposable
    {
        public required ApplicationDbContext Context { get; init; }
        public required OperatingRoomPreparationService Service { get; init; }
        public required MutableHttpContextAccessor Accessor { get; init; }
        public required Guid CaseId { get; init; }
        public required Guid SurgeonUserId { get; init; }
        public required Guid AnesthesiologistUserId { get; init; }
        public required Guid ScrubNurseUserId { get; init; }

        public static async Task<TestFixture> CreateAsync(bool emergency = false, bool withConsents = true)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase($"operating-room-preparation-{Guid.NewGuid()}").Options;
            var context = new ApplicationDbContext(options);

            var patientId = Guid.NewGuid();
            var encounterId = Guid.NewGuid();
            var procedureId = Guid.NewGuid();
            var doctorId = Guid.NewGuid();
            var caseId = Guid.NewGuid();
            var scheduleId = Guid.NewGuid();

            var roles = new[]
            {
                OprTeamRole.PrimarySurgeon, OprTeamRole.Anesthesiologist,
                OprTeamRole.ScrubNurse, OprTeamRole.CirculatingNurse
            };
            var workforceIds = roles.Select(_ => Guid.NewGuid()).ToArray();
            var userIds = roles.Select(_ => Guid.NewGuid()).ToArray();

            for (var i = 0; i < roles.Length; i++)
            {
                context.Set<MstWorkforceProfile>().Add(new MstWorkforceProfile
                {
                    Id = workforceIds[i], ProfileCode = $"WF{i:D3}", DisplayName = $"Tenaga {i + 1}", IsActive = true
                });
                context.Users.Add(new ApplicationUser
                {
                    Id = userIds[i], UserName = $"user{i}", NormalizedUserName = $"USER{i}",
                    UserCode = $"U{i:D3}", DisplayName = $"Pengguna {i + 1}", WorkforceProfileId = workforceIds[i]
                });
            }

            context.OprCases.Add(new OprCase
            {
                Id = caseId, CaseNumber = "OPR-PREP-001", PatientId = patientId, EncounterId = encounterId,
                RequesterDoctorId = doctorId, PrimarySurgeonId = doctorId,
                CaseType = emergency ? OprCaseType.Emergency : OprCaseType.Elective,
                Priority = emergency ? OprPriority.Emergency : OprPriority.Routine,
                Status = OprCaseStatus.Scheduled, Indication = "Indikasi uji", EstimatedMinutes = 60,
                RequestedAt = DateTime.UtcNow, Version = 0
            });
            context.OprCaseProcedures.Add(new OprCaseProcedure
            {
                OprCaseId = caseId, PatientProcedureId = procedureId, IsPrimary = true, Sequence = 1
            });
            context.OprSchedules.Add(new OprSchedule
            {
                Id = scheduleId, OprCaseId = caseId, RoomId = Guid.NewGuid(),
                StartAt = DateTime.UtcNow.AddHours(4), EndAt = DateTime.UtcNow.AddHours(5),
                BufferBeforeMinutes = 15, BufferAfterMinutes = 30, Revision = 1, IsCurrent = true,
                ChangedByUserId = userIds[0]
            });
            for (var i = 0; i < roles.Length; i++)
                context.OprTeamMembers.Add(new OprTeamMember
                {
                    OprCaseId = caseId, ScheduleId = scheduleId, WorkforceId = workforceIds[i],
                    Role = roles[i], IsLead = i == 0, IsCurrent = true,
                    CredentialCheckStatus = OprCredentialCheckStatus.NotAvailable
                });

            if (withConsents)
                foreach (var type in new[] { PatientConsentType.Surgery, PatientConsentType.Anesthesia })
                    context.Set<TrxPatientConsent>().Add(new TrxPatientConsent
                    {
                        PatientId = patientId, EncounterId = encounterId, PatientProcedureId = procedureId,
                        ConsentNumber = $"CNS-{type}", ConsentTitle = $"Persetujuan {type}",
                        ConsentType = type, ConsentStatus = PatientConsentStatus.Signed,
                        SignedAt = DateTime.UtcNow.AddHours(-2)
                    });

            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var accessor = new MutableHttpContextAccessor();
            accessor.SetUser(userIds[0]);
            var logger = new LoggerService(NullLogger<LoggerService>.Instance, accessor);

            return new TestFixture
            {
                Context = context,
                Service = new OperatingRoomPreparationService(context, accessor, logger, OperatingRoomTestContext.StrictRules),
                Accessor = accessor,
                CaseId = caseId,
                SurgeonUserId = userIds[0],
                AnesthesiologistUserId = userIds[1],
                ScrubNurseUserId = userIds[2]
            };
        }

        public void ActAs(Guid userId) => Accessor.SetUser(userId);

        public Task<int> CurrentVersionAsync() => Context.OprCases.AsNoTracking()
            .Where(x => x.Id == CaseId).Select(x => x.Version).FirstAsync();

        /// <summary>Checklist tiga item; dua di antaranya wajib dan sudah tercentang.</summary>
        public SaveOprChecklistRequest ChecklistRequest(string key, bool complete, int expectedVersion = 0) => new()
        {
            TemplateVersion = "WHO-SSC-2009",
            Complete = complete,
            IdempotencyKey = key,
            ExpectedVersion = expectedVersion,
            Items =
            [
                new OprChecklistItemRequest { Code = "IDENTITY", Label = "Identitas pasien terkonfirmasi", IsMandatory = true, IsChecked = true },
                new OprChecklistItemRequest { Code = "SITE", Label = "Lokasi tindakan ditandai", IsMandatory = true, IsChecked = true },
                new OprChecklistItemRequest { Code = "ALLERGY", Label = "Riwayat alergi ditinjau", IsMandatory = false, IsChecked = true }
            ]
        };

        public async Task CompleteSignInChecklistAsync()
        {
            var currentUser = Accessor.CurrentUserId;
            await Service.SaveChecklistAsync(CaseId, OprChecklistPhase.SignIn,
                ChecklistRequest($"checklist-{Guid.NewGuid():N}", complete: true,
                    expectedVersion: await CurrentVersionAsync()));
            ActAs(currentUser);
        }

        public async Task<OprPreparationResponse> SignOffAsync(OprReadinessRole role, string key)
        {
            ActAs(role switch
            {
                OprReadinessRole.PrimarySurgeon => SurgeonUserId,
                OprReadinessRole.Anesthesiologist => AnesthesiologistUserId,
                _ => ScrubNurseUserId
            });
            return await Service.CreateSignOffAsync(CaseId, new CreateOprReadinessSignOffRequest
            {
                Role = role, IdempotencyKey = key, ExpectedVersion = await CurrentVersionAsync()
            });
        }

        public ValueTask DisposeAsync() => Context.DisposeAsync();
    }

    private sealed class MutableHttpContextAccessor : IHttpContextAccessor
    {
        public Guid CurrentUserId { get; private set; }
        public HttpContext? HttpContext { get; set; }

        public void SetUser(Guid userId)
        {
            CurrentUserId = userId;
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity([
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                ], "Test"))
            };
        }
    }
}
