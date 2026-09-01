using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Tests.HealthServices.OperatingRoomManagement;

/// <summary>
/// Satu kasus operasi lengkap dengan jadwal berjalan, tim minimum, akun pengguna yang
/// terhubung ke tenaga, dan consent yang sah. Dipakai bersama oleh pengujian BE-OPR-006
/// sampai BE-OPR-011 agar penyiapan data tidak diulang di tiap berkas.
/// </summary>
internal sealed class OperatingRoomTestContext : IAsyncDisposable
{
    /// <summary>
    /// Aturan klinis dalam keadaan BERLAKU PENUH, yaitu keadaan bawaan sistem. Seluruh test
    /// modul Operasi memakai ini supaya yang diuji adalah perilaku sesungguhnya, bukan
    /// perilaku saat saklar pelepas aturan sedang menyala.
    /// </summary>
    public static QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Options
        .OperatingRoomRuleRelaxation StrictRules { get; } =
        new(new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build(),
            new Microsoft.Extensions.Hosting.Internal.HostingEnvironment
            {
                EnvironmentName = "Development"
            });

    private static readonly OprTeamRole[] TeamRoles =
    [
        OprTeamRole.PrimarySurgeon, OprTeamRole.Anesthesiologist,
        OprTeamRole.ScrubNurse, OprTeamRole.CirculatingNurse
    ];

    public required ApplicationDbContext Context { get; init; }
    public required MutableHttpContextAccessor Accessor { get; init; }
    public required LoggerService Logger { get; init; }
    public required Guid CaseId { get; init; }
    public required Guid ScheduleId { get; init; }
    public required Guid PatientId { get; init; }
    public required Guid EncounterId { get; init; }
    public required Guid ProcedureId { get; init; }
    public required Guid DoctorId { get; init; }
    public required Guid RoomId { get; init; }
    public required Guid DestinationUnitId { get; init; }
    public required Guid[] WorkforceIds { get; init; }
    public required Guid[] UserIds { get; init; }
    public required Guid OutsiderUserId { get; init; }

    public Guid SurgeonUserId => UserIds[0];
    public Guid AnesthesiologistUserId => UserIds[1];
    public Guid ScrubNurseUserId => UserIds[2];
    public Guid CirculatingNurseUserId => UserIds[3];

    public static async Task<OperatingRoomTestContext> CreateAsync(
        OprCaseStatus status = OprCaseStatus.Ready, bool emergency = false, bool withConsents = true)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"operating-room-{Guid.NewGuid()}").Options;
        var context = new ApplicationDbContext(options);

        var patientId = Guid.NewGuid();
        var encounterId = Guid.NewGuid();
        var procedureId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var caseId = Guid.NewGuid();
        var scheduleId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var destinationUnitId = Guid.NewGuid();
        var outsiderUserId = Guid.NewGuid();

        var workforceIds = TeamRoles.Select(_ => Guid.NewGuid()).ToArray();
        var userIds = TeamRoles.Select(_ => Guid.NewGuid()).ToArray();

        for (var i = 0; i < TeamRoles.Length; i++)
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

        // Pengguna di luar tim, dipakai untuk membuktikan penolakan kewenangan.
        var outsiderWorkforceId = Guid.NewGuid();
        context.Set<MstWorkforceProfile>().Add(new MstWorkforceProfile
        {
            Id = outsiderWorkforceId, ProfileCode = "WF999", DisplayName = "Tenaga Luar", IsActive = true
        });
        context.Users.Add(new ApplicationUser
        {
            Id = outsiderUserId, UserName = "outsider", NormalizedUserName = "OUTSIDER",
            UserCode = "U999", DisplayName = "Pengguna Luar", WorkforceProfileId = outsiderWorkforceId
        });

        context.Set<MstDoctor>().Add(new MstDoctor
        {
            Id = doctorId, WorkforceProfileId = workforceIds[0], FullName = "Dokter Bedah Uji",
            DoctorCode = "D001", DoctorNumber = "DN001", IsActive = true
        });
        context.Set<MstPatient>().Add(new MstPatient
        {
            Id = patientId, FullName = "Pasien Uji", PatientCode = "P001", MedicalRecordNumber = "MR001"
        });
        context.Set<MstRoom>().Add(new MstRoom
        {
            Id = roomId, ServiceUnitId = Guid.NewGuid(), RoomCode = "OK1", RoomName = "OK 1",
            RoomType = RoomType.OperatingRoom, IsActive = true
        });
        context.Set<MstServiceUnit>().Add(new MstServiceUnit
        {
            Id = destinationUnitId, ServiceUnitCode = "RI", ServiceUnitName = "Rawat Inap", IsActive = true
        });

        context.OprCases.Add(new OprCase
        {
            Id = caseId, CaseNumber = "OPR-TEST-001", PatientId = patientId, EncounterId = encounterId,
            RequesterDoctorId = doctorId, PrimarySurgeonId = doctorId,
            CaseType = emergency ? OprCaseType.Emergency : OprCaseType.Elective,
            Priority = emergency ? OprPriority.Emergency : OprPriority.Routine,
            Status = status, Indication = "Indikasi uji", EstimatedMinutes = 60,
            RequestedAt = DateTime.UtcNow, Version = 0
        });
        context.OprCaseProcedures.Add(new OprCaseProcedure
        {
            OprCaseId = caseId, PatientProcedureId = procedureId, IsPrimary = true, Sequence = 1
        });
        context.OprSchedules.Add(new OprSchedule
        {
            Id = scheduleId, OprCaseId = caseId, RoomId = roomId,
            StartAt = DateTime.UtcNow.AddHours(1), EndAt = DateTime.UtcNow.AddHours(2),
            BufferBeforeMinutes = 15, BufferAfterMinutes = 30, Revision = 1, IsCurrent = true,
            ChangedByUserId = userIds[0]
        });
        for (var i = 0; i < TeamRoles.Length; i++)
            context.OprTeamMembers.Add(new OprTeamMember
            {
                OprCaseId = caseId, ScheduleId = scheduleId, WorkforceId = workforceIds[i],
                Role = TeamRoles[i], IsLead = i == 0, IsCurrent = true,
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

        // Kasus yang sudah berjalan membawa catatan operasi draft seperti hasil `Start`.
        if (status is OprCaseStatus.InProgress)
            context.OprExecutionRecords.Add(new OprExecutionRecord
            {
                OprCaseId = caseId, Status = OprRecordStatus.Draft, StartedAt = DateTime.UtcNow.AddMinutes(-30),
                Version = 0
            });

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var accessor = new MutableHttpContextAccessor();
        accessor.SetUser(userIds[0]);

        return new OperatingRoomTestContext
        {
            Context = context,
            Accessor = accessor,
            Logger = new LoggerService(NullLogger<LoggerService>.Instance, accessor),
            CaseId = caseId,
            ScheduleId = scheduleId,
            PatientId = patientId,
            EncounterId = encounterId,
            ProcedureId = procedureId,
            DoctorId = doctorId,
            RoomId = roomId,
            DestinationUnitId = destinationUnitId,
            WorkforceIds = workforceIds,
            UserIds = userIds,
            OutsiderUserId = outsiderUserId
        };
    }

    public void ActAs(Guid userId) => Accessor.SetUser(userId);

    public Task<int> CurrentVersionAsync() => Context.OprCases.AsNoTracking()
        .Where(x => x.Id == CaseId).Select(x => x.Version).FirstAsync();

    public Task<OprCaseStatus> CurrentStatusAsync() => Context.OprCases.AsNoTracking()
        .Where(x => x.Id == CaseId).Select(x => x.Status).FirstAsync();

    public Task<int> RecordVersionAsync() => Context.OprExecutionRecords.AsNoTracking()
        .Where(x => x.OprCaseId == CaseId).Select(x => x.Version).FirstAsync();

    public ValueTask DisposeAsync() => Context.DisposeAsync();
}

internal sealed class MutableHttpContextAccessor : IHttpContextAccessor
{
    public Guid CurrentUserId { get; private set; }
    public HttpContext? HttpContext { get; set; }

    public void SetUser(Guid userId, Guid? doctorId = null)
    {
        CurrentUserId = userId;
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId.ToString()) };
        if (doctorId.HasValue) claims.Add(new Claim("doctor_id", doctorId.Value.ToString()));
        HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"))
        };
    }
}
