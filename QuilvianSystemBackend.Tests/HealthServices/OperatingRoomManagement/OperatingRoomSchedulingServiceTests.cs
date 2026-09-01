using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Options;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services;
using QuilvianSystemBackend.Enums.HumanResource;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using Xunit;

namespace QuilvianSystemBackend.Tests.HealthServices.OperatingRoomManagement;

public class OperatingRoomSchedulingServiceTests
{
    [Fact]
    public async Task ScheduleAsync_ValidRequest_MovesCaseToScheduledWithRevisionOne()
    {
        await using var fixture = await TestFixture.CreateAsync();
        var result = await fixture.Service.ScheduleAsync(fixture.CaseId, fixture.ValidRequest("schedule-1"));

        Assert.Equal(OprCaseStatus.Scheduled, result.Status);
        Assert.Equal(1, result.Revision);
        Assert.Equal(15, result.BufferBeforeMinutes);
        Assert.Equal(30, result.BufferAfterMinutes);
        Assert.Equal(4, result.TeamMembers.Count);
        Assert.Contains("Reschedule", result.AvailableActions);
        Assert.Equal(1, await fixture.Context.OprSchedules.CountAsync(x => x.IsCurrent));
    }

    [Fact]
    public async Task ScheduleAsync_SameIdempotencyAndPayload_DoesNotCreateSecondRevision()
    {
        await using var fixture = await TestFixture.CreateAsync();
        var first = await fixture.Service.ScheduleAsync(fixture.CaseId, fixture.ValidRequest("same-key"));
        var second = await fixture.Service.ScheduleAsync(fixture.CaseId, fixture.ValidRequest("same-key"));

        Assert.Equal(first.Id, second.Id);
        Assert.Equal(1, await fixture.Context.OprSchedules.CountAsync());
    }

    [Fact]
    public async Task ScheduleAsync_SameIdempotencyWithDifferentPayload_RejectsOpr013()
    {
        await using var fixture = await TestFixture.CreateAsync();
        await fixture.Service.ScheduleAsync(fixture.CaseId, fixture.ValidRequest("reused-key"));
        var changed = fixture.ValidRequest("reused-key");
        changed.StartAt = changed.StartAt.AddHours(2);
        changed.EndAt = changed.EndAt.AddHours(2);

        var exception = await Assert.ThrowsAsync<OperatingRoomConflictException>(() =>
            fixture.Service.ScheduleAsync(fixture.CaseId, changed));

        Assert.Equal("OPR013", exception.Code);
    }

    [Fact]
    public async Task ScheduleAsync_StaleVersion_RejectsOpr012()
    {
        await using var fixture = await TestFixture.CreateAsync();
        var request = fixture.ValidRequest("stale-version");
        request.ExpectedVersion = 5;

        var exception = await Assert.ThrowsAsync<OperatingRoomConflictException>(() =>
            fixture.Service.ScheduleAsync(fixture.CaseId, request));

        Assert.Equal("OPR012", exception.Code);
    }

    [Fact]
    public async Task ScheduleAsync_RoomAlreadyBookedWithinBuffer_RejectsOpr003()
    {
        await using var fixture = await TestFixture.CreateAsync();
        await fixture.Service.ScheduleAsync(fixture.CaseId, fixture.ValidRequest("first-case"));

        // Kasus kedua memakai ruang yang sama dan mulai 20 menit setelah kasus pertama selesai.
        // Buffer pembersihan 30 menit membuat kedua jadwal tetap beririsan.
        var second = fixture.ValidRequest("second-case", caseOffsetMinutes: 80);
        second.TeamMembers = fixture.OtherTeam();

        var exception = await Assert.ThrowsAsync<OperatingRoomConflictException>(() =>
            fixture.Service.ScheduleAsync(fixture.SecondCaseId, second));

        Assert.Equal("OPR003", exception.Code);
    }

    [Fact]
    public async Task ScheduleAsync_TeamMemberBookedInOtherRoom_RejectsOpr003()
    {
        await using var fixture = await TestFixture.CreateAsync();
        await fixture.Service.ScheduleAsync(fixture.CaseId, fixture.ValidRequest("room-a"));

        // Ruang berbeda, waktu sama, tetapi memakai anggota tim yang sama.
        var second = fixture.ValidRequest("room-b");
        second.RoomId = fixture.SecondRoomId;

        var exception = await Assert.ThrowsAsync<OperatingRoomConflictException>(() =>
            fixture.Service.ScheduleAsync(fixture.ThirdCaseId, second));

        Assert.Equal("OPR003", exception.Code);
    }

    [Fact]
    public async Task ScheduleAsync_OutsideBufferWindow_Succeeds()
    {
        await using var fixture = await TestFixture.CreateAsync();
        await fixture.Service.ScheduleAsync(fixture.CaseId, fixture.ValidRequest("early-case"));

        // Mulai 3 jam setelah kasus pertama selesai; jauh di luar buffer 30 menit.
        var second = fixture.ValidRequest("late-case", caseOffsetMinutes: 240);
        second.TeamMembers = fixture.OtherTeam();
        var result = await fixture.Service.ScheduleAsync(fixture.SecondCaseId, second);

        Assert.Equal(OprCaseStatus.Scheduled, result.Status);
    }

    [Fact]
    public async Task ScheduleAsync_MissingRequiredRole_RejectsOpr004()
    {
        await using var fixture = await TestFixture.CreateAsync();
        var request = fixture.ValidRequest("incomplete-team");
        request.TeamMembers.RemoveAll(x => x.Role == OprTeamRole.CirculatingNurse);

        var exception = await Assert.ThrowsAsync<OperatingRoomUnprocessableException>(() =>
            fixture.Service.ScheduleAsync(fixture.CaseId, request));

        Assert.Equal("OPR004", exception.Code);
    }

    [Fact]
    public async Task ScheduleAsync_InactiveWorkforce_RejectsOpr005()
    {
        await using var fixture = await TestFixture.CreateAsync();
        var nurse = await fixture.Context.Set<MstWorkforceProfile>()
            .FirstAsync(x => x.Id == fixture.ScrubNurseWorkforceId);
        nurse.IsActive = false;
        await fixture.Context.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<OperatingRoomUnprocessableException>(() =>
            fixture.Service.ScheduleAsync(fixture.CaseId, fixture.ValidRequest("inactive-nurse")));

        Assert.Equal("OPR005", exception.Code);
    }

    [Fact]
    public async Task ScheduleAsync_CredentialBlocked_RejectsOpr005()
    {
        await using var fixture = await TestFixture.CreateAsync();
        fixture.Context.WfpClinicalPrivileges.Add(new WfpClinicalPrivilege
        {
            WorkforceProfileId = fixture.SurgeonWorkforceId,
            PrivilegeCode = "BEDAH-01",
            PrivilegeName = "Bedah Umum",
            PrivilegeStatus = ClinicalPrivilegeStatus.Active,
            IsSchedulingBlocked = true,
            EffectiveStartDate = DateTime.UtcNow.AddYears(-1)
        });
        await fixture.Context.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<OperatingRoomUnprocessableException>(() =>
            fixture.Service.ScheduleAsync(fixture.CaseId, fixture.ValidRequest("blocked-credential")));

        Assert.Equal("OPR005", exception.Code);
    }

    [Fact]
    public async Task ScheduleAsync_WithoutCredentialData_MarksTeamNotAvailableAndStillSchedules()
    {
        await using var fixture = await TestFixture.CreateAsync();
        var result = await fixture.Service.ScheduleAsync(fixture.CaseId, fixture.ValidRequest("unresolved-credential"));

        Assert.All(result.TeamMembers, x =>
            Assert.Equal(OprCredentialCheckStatus.NotAvailable, x.CredentialCheckStatus));
    }

    [Fact]
    public async Task ScheduleAsync_LeadIsNotPrimarySurgeonOfCase_RejectsOpr004()
    {
        await using var fixture = await TestFixture.CreateAsync();
        var request = fixture.ValidRequest("wrong-lead");
        request.TeamMembers.Single(x => x.IsLead).WorkforceId = fixture.AnesthesiologistWorkforceId;
        request.TeamMembers.RemoveAll(x => x.Role == OprTeamRole.Anesthesiologist);
        request.TeamMembers.Add(new OprTeamMemberRequest
        {
            WorkforceId = fixture.SurgeonWorkforceId, Role = OprTeamRole.Anesthesiologist
        });

        var exception = await Assert.ThrowsAsync<OperatingRoomUnprocessableException>(() =>
            fixture.Service.ScheduleAsync(fixture.CaseId, request));

        Assert.Equal("OPR004", exception.Code);
    }

    [Fact]
    public async Task ScheduleAsync_RevisionWithoutChangeReason_RejectsWithArgumentException()
    {
        await using var fixture = await TestFixture.CreateAsync();
        var first = await fixture.Service.ScheduleAsync(fixture.CaseId, fixture.ValidRequest("revision-base"));

        var revision = fixture.ValidRequest("revision-no-reason", caseOffsetMinutes: 300);
        revision.ExpectedVersion = first.Version;

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            fixture.Service.ScheduleAsync(fixture.CaseId, revision));
        Assert.Equal("Alasan perubahan jadwal wajib diisi.", exception.Message);
    }

    [Fact]
    public async Task ScheduleAsync_RevisionWithReason_KeepsHistoryAndRaisesRevision()
    {
        await using var fixture = await TestFixture.CreateAsync();
        var first = await fixture.Service.ScheduleAsync(fixture.CaseId, fixture.ValidRequest("revision-first"));

        var revision = fixture.ValidRequest("revision-second", caseOffsetMinutes: 300);
        revision.ExpectedVersion = first.Version;
        revision.ChangeReason = "Ruang dipakai kasus darurat.";
        var result = await fixture.Service.ScheduleAsync(fixture.CaseId, revision);

        Assert.Equal(2, result.Revision);
        Assert.Equal(2, await fixture.Context.OprSchedules.CountAsync());
        Assert.Equal(1, await fixture.Context.OprSchedules.CountAsync(x => x.IsCurrent));
        Assert.Equal(8, await fixture.Context.OprTeamMembers.CountAsync());
        Assert.Equal(4, await fixture.Context.OprTeamMembers.CountAsync(x => x.IsCurrent));
    }

    [Fact]
    public async Task GetScheduleHistoryAsync_AfterRevision_ReturnsAllRevisionsNewestFirst()
    {
        await using var fixture = await TestFixture.CreateAsync();
        var first = await fixture.Service.ScheduleAsync(fixture.CaseId, fixture.ValidRequest("history-first"));

        var revision = fixture.ValidRequest("history-second", caseOffsetMinutes: 300);
        revision.ExpectedVersion = first.Version;
        revision.ChangeReason = "Ruang dipakai kasus darurat.";
        await fixture.Service.ScheduleAsync(fixture.CaseId, revision);

        var history = await fixture.Service.GetScheduleHistoryAsync(fixture.CaseId);

        Assert.Equal(2, history.Count);
        Assert.Equal([2, 1], history.Select(x => x.Revision).ToArray());
        Assert.True(history[0].IsCurrent);
        Assert.False(history[1].IsCurrent);
        Assert.Equal("Ruang dipakai kasus darurat.", history[0].ChangeReason);
        Assert.All(history, x => Assert.Equal(4, x.TeamMembers.Count));
    }

    [Fact]
    public async Task GetScheduleHistoryAsync_CaseWithoutSchedule_ReturnsEmptyList()
    {
        await using var fixture = await TestFixture.CreateAsync();

        var history = await fixture.Service.GetScheduleHistoryAsync(fixture.CaseId);

        Assert.Empty(history);
    }

    [Fact]
    public async Task GetScheduleHistoryAsync_UnknownCase_ReturnsEmptyList()
    {
        await using var fixture = await TestFixture.CreateAsync();

        var history = await fixture.Service.GetScheduleHistoryAsync(Guid.NewGuid());

        Assert.Empty(history);
    }

    [Fact]
    public async Task PostponeAsync_FromScheduled_ClearsCurrentPlanAndAllowsReschedule()
    {
        await using var fixture = await TestFixture.CreateAsync();
        var scheduled = await fixture.Service.ScheduleAsync(fixture.CaseId, fixture.ValidRequest("before-postpone"));

        var postponed = await fixture.Service.PostponeAsync(fixture.CaseId, new PostponeOprCaseRequest
        {
            Reason = "Pasien demam pada hari operasi.",
            ConfirmedByDoctorId = fixture.DoctorId,
            IdempotencyKey = "postpone-1",
            ExpectedVersion = scheduled.Version
        });

        Assert.Equal(OprCaseStatus.Postponed, postponed.Status);
        Assert.Equal(["Reschedule"], postponed.AvailableActions);
        Assert.Equal(0, await fixture.Context.OprSchedules.CountAsync(x => x.IsCurrent));

        var reschedule = fixture.ValidRequest("after-postpone", caseOffsetMinutes: 1440);
        reschedule.ExpectedVersion = postponed.Version;
        reschedule.ChangeReason = "Dijadwalkan ulang setelah pasien pulih.";
        var result = await fixture.Service.ScheduleAsync(fixture.CaseId, reschedule);

        Assert.Equal(OprCaseStatus.Scheduled, result.Status);
        Assert.Equal(2, result.Revision);
    }

    [Fact]
    public async Task PostponeAsync_WithoutReason_RejectsAsUnprocessable()
    {
        await using var fixture = await TestFixture.CreateAsync();

        var exception = await Assert.ThrowsAsync<OperatingRoomUnprocessableException>(() =>
            fixture.Service.PostponeAsync(fixture.CaseId, new PostponeOprCaseRequest
            {
                Reason = "   ",
                ConfirmedByDoctorId = fixture.DoctorId,
                IdempotencyKey = "postpone-no-reason"
            }));

        Assert.Equal("MissingPostponeReason", exception.Code);
    }

    [Fact]
    public async Task PostponeAsync_SameIdempotencyKey_IsRepeatable()
    {
        await using var fixture = await TestFixture.CreateAsync();
        var request = new PostponeOprCaseRequest
        {
            Reason = "Alat sterilisasi rusak.",
            ConfirmedByDoctorId = fixture.DoctorId,
            IdempotencyKey = "postpone-twice"
        };

        var first = await fixture.Service.PostponeAsync(fixture.CaseId, request);
        var second = await fixture.Service.PostponeAsync(fixture.CaseId, request);

        Assert.Equal(first.Version, second.Version);
        Assert.Equal(1, await fixture.Context.OprStatusHistories.CountAsync(x => x.Action == "Postpone"));
    }

    [Fact]
    public async Task PostponeAsync_OnPostponedCase_RejectsInvalidStateTransition()
    {
        await using var fixture = await TestFixture.CreateAsync();
        var postponed = await fixture.Service.PostponeAsync(fixture.CaseId, new PostponeOprCaseRequest
        {
            Reason = "Pasien belum puasa.",
            ConfirmedByDoctorId = fixture.DoctorId,
            IdempotencyKey = "postpone-once"
        });

        var exception = await Assert.ThrowsAsync<OperatingRoomConflictException>(() =>
            fixture.Service.PostponeAsync(fixture.CaseId, new PostponeOprCaseRequest
            {
                Reason = "Alasan lain.",
                ConfirmedByDoctorId = fixture.DoctorId,
                IdempotencyKey = "postpone-again",
                ExpectedVersion = postponed.Version
            }));

        Assert.Equal("InvalidStateTransition", exception.Code);
    }

    [Fact]
    public async Task ScheduleAsync_NonOperatingRoom_RejectsWithArgumentException()
    {
        await using var fixture = await TestFixture.CreateAsync();
        var request = fixture.ValidRequest("wrong-room");
        request.RoomId = fixture.InpatientRoomId;

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            fixture.Service.ScheduleAsync(fixture.CaseId, request));
        Assert.Equal("Ruang operasi tidak ditemukan atau tidak aktif.", exception.Message);
    }

    private sealed class TestFixture : IAsyncDisposable
    {
        private static readonly DateTime BaseStart = new(2026, 9, 1, 8, 0, 0, DateTimeKind.Utc);

        public required ApplicationDbContext Context { get; init; }
        public required OperatingRoomSchedulingService Service { get; init; }
        public required Guid CaseId { get; init; }
        public required Guid SecondCaseId { get; init; }
        public required Guid ThirdCaseId { get; init; }
        public required Guid RoomId { get; init; }
        public required Guid SecondRoomId { get; init; }
        public required Guid InpatientRoomId { get; init; }
        public required Guid DoctorId { get; init; }
        public required Guid SurgeonWorkforceId { get; init; }
        public required Guid AnesthesiologistWorkforceId { get; init; }
        public required Guid ScrubNurseWorkforceId { get; init; }
        public required Guid CirculatingNurseWorkforceId { get; init; }
        public required Guid SpareSurgeonWorkforceId { get; init; }
        public required Guid SpareAnesthesiologistWorkforceId { get; init; }
        public required Guid SpareScrubNurseWorkforceId { get; init; }
        public required Guid SpareCirculatingNurseWorkforceId { get; init; }

        public static async Task<TestFixture> CreateAsync()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase($"operating-room-schedule-{Guid.NewGuid()}").Options;
            var context = new ApplicationDbContext(options);

            var userId = Guid.NewGuid();
            var doctorId = Guid.NewGuid();
            var surgeonWorkforceId = Guid.NewGuid();
            var workforceIds = new List<Guid> { surgeonWorkforceId };
            for (var i = 0; i < 7; i++) workforceIds.Add(Guid.NewGuid());

            foreach (var (id, index) in workforceIds.Select((id, index) => (id, index)))
                context.Set<MstWorkforceProfile>().Add(new MstWorkforceProfile
                {
                    Id = id, ProfileCode = $"WF{index:D3}", DisplayName = $"Tenaga {index + 1}", IsActive = true
                });

            var spareDoctorId = Guid.NewGuid();
            context.Set<MstDoctor>().AddRange(
                new MstDoctor
                {
                    Id = doctorId, WorkforceProfileId = surgeonWorkforceId, FullName = "Dokter Bedah Uji",
                    DoctorCode = "D001", DoctorNumber = "DN001", IsActive = true
                },
                new MstDoctor
                {
                    Id = spareDoctorId, WorkforceProfileId = workforceIds[4], FullName = "Dokter Bedah Kedua",
                    DoctorCode = "D002", DoctorNumber = "DN002", IsActive = true
                });

            var roomId = Guid.NewGuid();
            var secondRoomId = Guid.NewGuid();
            var inpatientRoomId = Guid.NewGuid();
            context.Set<MstRoom>().AddRange(
                new MstRoom { Id = roomId, ServiceUnitId = Guid.NewGuid(), RoomCode = "OK1", RoomName = "OK 1", RoomType = RoomType.OperatingRoom, IsActive = true },
                new MstRoom { Id = secondRoomId, ServiceUnitId = Guid.NewGuid(), RoomCode = "OK2", RoomName = "OK 2", RoomType = RoomType.OperatingRoom, IsActive = true },
                new MstRoom { Id = inpatientRoomId, ServiceUnitId = Guid.NewGuid(), RoomCode = "RI1", RoomName = "Melati 1", RoomType = RoomType.InpatientRoom, IsActive = true });

            var caseId = Guid.NewGuid();
            var secondCaseId = Guid.NewGuid();
            var thirdCaseId = Guid.NewGuid();
            context.OprCases.AddRange(
                NewCase(caseId, "OPR-001", doctorId),
                // Kasus kedua dipakai untuk menguji benturan ruang, sehingga dokter bedahnya
                // berbeda agar timnya boleh berbeda pula.
                NewCase(secondCaseId, "OPR-002", spareDoctorId),
                // Kasus ketiga memakai dokter bedah yang sama untuk menguji benturan anggota tim.
                NewCase(thirdCaseId, "OPR-003", doctorId));
            await context.SaveChangesAsync();

            var httpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity([
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                ], "Test"))
            };
            var accessor = new FixedHttpContextAccessor { HttpContext = httpContext };
            var logger = new LoggerService(NullLogger<LoggerService>.Instance, accessor);

            return new TestFixture
            {
                Context = context,
                Service = new OperatingRoomSchedulingService(context, accessor, logger,
                    new OperatingRoomCredentialResolver(context),
                    Options.Create(new OperatingRoomSchedulingOptions()), OperatingRoomTestContext.StrictRules),
                CaseId = caseId,
                SecondCaseId = secondCaseId,
                ThirdCaseId = thirdCaseId,
                RoomId = roomId,
                SecondRoomId = secondRoomId,
                InpatientRoomId = inpatientRoomId,
                DoctorId = doctorId,
                SurgeonWorkforceId = surgeonWorkforceId,
                AnesthesiologistWorkforceId = workforceIds[1],
                ScrubNurseWorkforceId = workforceIds[2],
                CirculatingNurseWorkforceId = workforceIds[3],
                SpareSurgeonWorkforceId = workforceIds[4],
                SpareAnesthesiologistWorkforceId = workforceIds[5],
                SpareScrubNurseWorkforceId = workforceIds[6],
                SpareCirculatingNurseWorkforceId = workforceIds[7]
            };
        }

        private static OprCase NewCase(Guid id, string caseNumber, Guid doctorId) => new()
        {
            Id = id, CaseNumber = caseNumber, PatientId = Guid.NewGuid(), EncounterId = Guid.NewGuid(),
            RequesterDoctorId = doctorId, PrimarySurgeonId = doctorId, CaseType = OprCaseType.Elective,
            Priority = OprPriority.Routine, Status = OprCaseStatus.Requested, Indication = "Indikasi uji",
            EstimatedMinutes = 60, RequestedAt = DateTime.UtcNow, Version = 0
        };

        /// <summary>Permintaan jadwal 60 menit yang lolos seluruh validasi dasar.</summary>
        public ScheduleOprCaseRequest ValidRequest(string key, int caseOffsetMinutes = 0) => new()
        {
            RoomId = RoomId,
            StartAt = BaseStart.AddMinutes(caseOffsetMinutes),
            EndAt = BaseStart.AddMinutes(caseOffsetMinutes + 60),
            TeamMembers =
            [
                new OprTeamMemberRequest { WorkforceId = SurgeonWorkforceId, Role = OprTeamRole.PrimarySurgeon, IsLead = true },
                new OprTeamMemberRequest { WorkforceId = AnesthesiologistWorkforceId, Role = OprTeamRole.Anesthesiologist },
                new OprTeamMemberRequest { WorkforceId = ScrubNurseWorkforceId, Role = OprTeamRole.ScrubNurse },
                new OprTeamMemberRequest { WorkforceId = CirculatingNurseWorkforceId, Role = OprTeamRole.CirculatingNurse }
            ],
            IdempotencyKey = key,
            ExpectedVersion = 0
        };

        /// <summary>Tim pengganti tanpa satu pun anggota yang sama, untuk menguji benturan ruang saja.</summary>
        public List<OprTeamMemberRequest> OtherTeam() =>
        [
            new() { WorkforceId = SpareSurgeonWorkforceId, Role = OprTeamRole.PrimarySurgeon, IsLead = true },
            new() { WorkforceId = SpareAnesthesiologistWorkforceId, Role = OprTeamRole.Anesthesiologist },
            new() { WorkforceId = SpareScrubNurseWorkforceId, Role = OprTeamRole.ScrubNurse },
            new() { WorkforceId = SpareCirculatingNurseWorkforceId, Role = OprTeamRole.CirculatingNurse }
        ];

        public ValueTask DisposeAsync() => Context.DisposeAsync();
    }

    private sealed class FixedHttpContextAccessor : IHttpContextAccessor
    {
        public HttpContext? HttpContext { get; set; }
    }
}
