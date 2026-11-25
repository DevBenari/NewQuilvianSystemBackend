using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Services;
using QuilvianSystemBackend.Enums;

namespace QuilvianSystemBackend.Tests.HumanResource;

// BE-HRD-351 — regresi INT-HR-39 (integration-contract 1865): daftar dokter siaga Active untuk Laboratorium
// ("GET on-call-assignments/active-doctors") TIDAK berubah oleh amandemen A52 — catatan panggilan dan cadangan siaga
// (HrdOnCallCall, HrdOnCallBackup) tidak memengaruhi isinya (HRD-DEC-198, HRD-ASM-78).
public sealed class IntHr39RegressionTests : IDisposable
{
    private readonly TestDatabase _db = TestDatabase.Create();
    private static readonly DateTime Jam = new(2027, 2, 10, 15, 0, 0, DateTimeKind.Utc);

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task Dokter_siaga_aktif_tetap_sama_dengan_cadangan_dan_panggilan()
    {
        Guid andi, budi, cadangan, roster;
        using (var ctx = _db.CreateContext())
        {
            MstWorkforceProfile Profil(string n) => new() { Id = Guid.NewGuid(), ProfileCode = n, DisplayName = n, UserType = UserType.PermanentDoctor, IsActive = true };
            var pAndi = Profil("dr. Andi");
            var pBudi = Profil("dr. Budi");
            var pCadangan = Profil("dr. Cadangan");
            ctx.MstWorkforceProfiles.AddRange(pAndi, pBudi, pCadangan);
            (andi, budi, cadangan) = (pAndi.Id, pBudi.Id, pCadangan.Id);
            var unit = new MstOrganizationUnit { Id = Guid.NewGuid(), LegalEntityId = Guid.NewGuid(), UnitCode = "IGD", UnitName = "IGD" };
            ctx.MstOrganizationUnits.Add(unit);
            var tipe = new MstOnCallType { OnCallTypeCode = "OC", OnCallTypeName = "Siaga Dokter", IsActive = true, AnswerTimeoutMinutes = 10 };
            ctx.Set<MstOnCallType>().Add(tipe);
            roster = Guid.NewGuid();
            ctx.TrxRosterPeriods.Add(new TrxRosterPeriod { Id = roster, OrganizationUnitId = unit.Id, RosterPeriodCode = "R", RosterPeriodName = "R", RosterStatus = "Published", PeriodStartDate = new DateOnly(2027, 2, 1), PeriodEndDate = new DateOnly(2027, 2, 28), IsActive = true });

            TrxOnCallAssignment Siaga(Guid wf, string status, DateTime mulai) => new()
            { RosterPeriodId = roster, WorkforceProfileId = wf, OnCallTypeId = tipe.Id, StartAt = mulai, EndAt = mulai.AddHours(12), AssignmentStatus = status, IsActive = true };
            var aktif = Siaga(andi, "Active", Jam.AddHours(-2));
            ctx.TrxOnCallAssignments.AddRange(aktif, Siaga(budi, "Scheduled", Jam.AddHours(-2)), Siaga(andi, "Active", Jam.AddDays(1)));

            // Cadangan dan panggilan A52 pada penugasan aktif dr. Andi.
            ctx.Set<HrdOnCallBackup>().Add(new HrdOnCallBackup { OnCallAssignmentId = aktif.Id, WorkforceProfileId = cadangan, BackupSequence = 1, IsActive = true });
            ctx.Set<HrdOnCallCall>().Add(new HrdOnCallCall { OnCallAssignmentId = aktif.Id, CalledWorkforceProfileId = andi, CallingOrganizationUnitId = unit.Id, CallReason = "Pasien IGD", CalledAt = Jam.AddMinutes(-30), CallStatus = "Declined", DeclineReason = "Sedang operasi" });
            ctx.Set<HrdOnCallCall>().Add(new HrdOnCallCall { OnCallAssignmentId = aktif.Id, CalledWorkforceProfileId = cadangan, BackupSequence = 1, CallingOrganizationUnitId = unit.Id, CallReason = "Pasien IGD", CalledAt = Jam.AddMinutes(-25), CallStatus = "Accepted" });
            ctx.SaveChanges();
        }

        using var baca = _db.CreateContext();
        var svc = new OnCallAssignmentService(baca, NullLogger<OnCallAssignmentService>.Instance);
        var dokter = await svc.GetActiveOnCallDoctorIdsAsync(null, Jam);

        // Hanya dr. Andi (Active, dalam rentang); bukan dr. Budi (Scheduled), bukan cadangan, walaupun cadangan yang menerima panggilan.
        Assert.Equal(new[] { andi }, dokter);
        Assert.DoesNotContain(cadangan, dokter);
        Assert.DoesNotContain(budi, dokter);
    }
}
