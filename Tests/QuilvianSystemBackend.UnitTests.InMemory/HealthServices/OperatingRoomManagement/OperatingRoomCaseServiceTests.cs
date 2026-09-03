using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using Xunit;

namespace QuilvianSystemBackend.Tests.HealthServices.OperatingRoomManagement;

public class OperatingRoomCaseServiceTests
{
    [Fact]
    public async Task CreateAsync_ValidRequest_CreatesRequestedCaseAndHistory()
    {
        await using var fixture = await TestFixture.CreateAsync();
        var result = await fixture.Service.CreateAsync(fixture.ValidCreateRequest("create-1"));

        Assert.Equal(OprCaseStatus.Requested, result.Status);
        Assert.StartsWith("OPR-", result.CaseNumber);
        Assert.Single(result.Procedures);
        Assert.Contains("Schedule", result.AvailableActions);
        Assert.Equal(1, await fixture.Context.OprStatusHistories.CountAsync());
    }

    [Fact]
    public async Task CreateAsync_SameIdempotencyAndPayload_ReturnsExistingCase()
    {
        await using var fixture = await TestFixture.CreateAsync();
        var first = await fixture.Service.CreateAsync(fixture.ValidCreateRequest("same-key"));
        var second = await fixture.Service.CreateAsync(fixture.ValidCreateRequest("same-key"));

        Assert.Equal(first.Id, second.Id);
        Assert.Equal(1, await fixture.Context.OprCases.CountAsync());
    }

    [Fact]
    public async Task CreateAsync_SameIdempotencyWithDifferentPayload_RejectsOpr013()
    {
        await using var fixture = await TestFixture.CreateAsync();
        await fixture.Service.CreateAsync(fixture.ValidCreateRequest("reused-key"));
        var changed = fixture.ValidCreateRequest("reused-key");
        changed.EstimatedMinutes = 120;

        var exception = await Assert.ThrowsAsync<OperatingRoomConflictException>(() =>
            fixture.Service.CreateAsync(changed));

        Assert.Equal("OPR013", exception.Code);
    }

    [Fact]
    public async Task CreateAsync_WithoutExactlyOnePrimary_RejectsOpr001()
    {
        await using var fixture = await TestFixture.CreateAsync();
        var request = fixture.ValidCreateRequest("missing-primary");
        request.Procedures[0].IsPrimary = false;

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => fixture.Service.CreateAsync(request));
        Assert.Equal("Pilih satu tindakan utama.", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_ProcedureUsedByActiveCase_RejectsOpr002()
    {
        await using var fixture = await TestFixture.CreateAsync();
        await fixture.Service.CreateAsync(fixture.ValidCreateRequest("first-case"));

        var exception = await Assert.ThrowsAsync<OperatingRoomConflictException>(() =>
            fixture.Service.CreateAsync(fixture.ValidCreateRequest("second-case")));

        Assert.Equal("OPR002", exception.Code);
    }

    [Fact]
    public async Task UpdateAsync_ValidRequest_RaisesVersionAndRecordsHistory()
    {
        await using var fixture = await TestFixture.CreateAsync();
        var created = await fixture.Service.CreateAsync(fixture.ValidCreateRequest("create-before-valid-update"));

        var request = fixture.ValidUpdateRequest("valid-update");
        request.ExpectedVersion = created.Version;
        var updated = await fixture.Service.UpdateAsync(created.Id, request);

        Assert.Equal(created.Version + 1, updated.Version);
        Assert.Equal("Indikasi diperbarui", updated.Indication);
        Assert.Equal(90, updated.EstimatedMinutes);
        Assert.Single(updated.Procedures);
        Assert.Equal(1, await fixture.Context.OprCaseProcedures.CountAsync(x => !x.IsDelete));
        Assert.Equal(2, await fixture.Context.OprStatusHistories.CountAsync());
    }

    [Fact]
    public async Task UpdateAsync_StaleVersion_RejectsOpr012()
    {
        await using var fixture = await TestFixture.CreateAsync();
        var created = await fixture.Service.CreateAsync(fixture.ValidCreateRequest("create-before-update"));
        var request = fixture.ValidUpdateRequest("stale-update");
        request.ExpectedVersion = created.Version + 1;

        var exception = await Assert.ThrowsAsync<OperatingRoomConflictException>(() =>
            fixture.Service.UpdateAsync(created.Id, request));

        Assert.Equal("OPR012", exception.Code);
    }

    private sealed class TestFixture : IAsyncDisposable
    {
        public required ApplicationDbContext Context { get; init; }
        public required OperatingRoomCaseService Service { get; init; }
        public required Guid PatientId { get; init; }
        public required Guid EncounterId { get; init; }
        public required Guid DoctorId { get; init; }
        public required Guid ProcedureId { get; init; }

        public static async Task<TestFixture> CreateAsync()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase($"operating-room-case-{Guid.NewGuid()}").Options;
            var context = new ApplicationDbContext(options);
            var patientId = Guid.NewGuid();
            var encounterId = Guid.NewGuid();
            var doctorId = Guid.NewGuid();
            var procedureId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            context.MstPatients.Add(new MstPatient { Id = patientId, FullName = "Pasien Uji", PatientCode = "P001", MedicalRecordNumber = "MR001" });
            context.TrxPatientEncounters.Add(new TrxPatientEncounter { Id = encounterId, PatientId = patientId, EncounterNumber = "E001" });
            context.MstDoctors.Add(new MstDoctor { Id = doctorId, FullName = "Dokter Uji", DoctorCode = "D001", DoctorNumber = "DN001", IsActive = true });
            context.TrxPatientProcedures.Add(new TrxPatientProcedure
            {
                Id = procedureId, EncounterId = encounterId, PatientId = patientId, DoctorId = doctorId,
                ProcedureCodeSnapshot = "OP001", ProcedureNameSnapshot = "Operasi Uji",
                IsSurgeryRelated = true, IsActive = true
            });
            await context.SaveChangesAsync();

            var httpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity([
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim("doctor_id", doctorId.ToString())
                ], "Test"))
            };
            var accessor = new FixedHttpContextAccessor { HttpContext = httpContext };
            var logger = new LoggerService(NullLogger<LoggerService>.Instance, accessor);

            return new TestFixture
            {
                Context = context,
                Service = new OperatingRoomCaseService(context, accessor, logger),
                PatientId = patientId,
                EncounterId = encounterId,
                DoctorId = doctorId,
                ProcedureId = procedureId
            };
        }

        public CreateOprCaseRequest ValidCreateRequest(string key) => new()
        {
            PatientId = PatientId, EncounterId = EncounterId, RequesterDoctorId = DoctorId,
            PrimarySurgeonId = DoctorId, CaseType = OprCaseType.Elective, Priority = OprPriority.Routine,
            Indication = "Indikasi uji", EstimatedMinutes = 60, IdempotencyKey = key,
            Procedures = [new OprCaseProcedureRequest { PatientProcedureId = ProcedureId, IsPrimary = true }]
        };

        public UpdateOprCaseRequest ValidUpdateRequest(string key) => new()
        {
            RequesterDoctorId = DoctorId, PrimarySurgeonId = DoctorId, CaseType = OprCaseType.Elective,
            Priority = OprPriority.Routine, Indication = "Indikasi diperbarui", EstimatedMinutes = 90,
            IdempotencyKey = key,
            Procedures = [new OprCaseProcedureRequest { PatientProcedureId = ProcedureId, IsPrimary = true }]
        };

        public ValueTask DisposeAsync() => Context.DisposeAsync();
    }

    private sealed class FixedHttpContextAccessor : IHttpContextAccessor
    {
        public HttpContext? HttpContext { get; set; }
    }
}
