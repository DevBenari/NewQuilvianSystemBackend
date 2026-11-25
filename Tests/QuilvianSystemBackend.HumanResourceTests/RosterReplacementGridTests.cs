using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Tests.HumanResource;

// BE-HRD-450 — AT-HRD-PK-47 (HRD-DEC-237, kontrak v30 API 36.2): detail roster menempatkan shift di baris petugas yang
// memegangnya sekarang. Contoh desain: Pagi 16 Feb Indra diganti Raka.
public sealed class RosterReplacementGridTests : IDisposable
{
    private readonly TestDatabase _db = TestDatabase.Create();
    private readonly Guid _icu = Guid.NewGuid(), _pagi = Guid.NewGuid(), _periode = Guid.NewGuid(), _draf = Guid.NewGuid();
    private readonly Guid _indra = Guid.NewGuid(), _raka = Guid.NewGuid(), _tono = Guid.NewGuid(), _budi = Guid.NewGuid();
    private readonly Guid _raIndra = Guid.NewGuid(), _raBudi = Guid.NewGuid();
    private readonly Guid _indra16 = Guid.NewGuid(), _indra17 = Guid.NewGuid(), _budi18 = Guid.NewGuid();
    private readonly Guid _sari = Guid.NewGuid();
    private static readonly DateOnly T16 = new(2027, 2, 16), T17 = new(2027, 2, 17), T18 = new(2027, 2, 18), T19 = new(2027, 2, 19);

    public RosterReplacementGridTests()
    {
        using var ctx = _db.CreateContext();
        ctx.MstOrganizationUnits.Add(new MstOrganizationUnit { Id = _icu, LegalEntityId = Guid.NewGuid(), UnitCode = "ICU", UnitName = "ICU" });
        ctx.MstShifts.Add(new MstShift { Id = _pagi, ShiftCode = "PA", ShiftName = "Pagi", StartTime = new TimeOnly(7, 0), EndTime = new TimeOnly(14, 0), PaidWorkMinutes = 420, IsActive = true });
        foreach (var (id, kode, nama) in new[]
                 {
                     (_indra, "EMP-IND", "Indra Setiawan"), (_raka, "EMP-RAK", "Raka Firmansyah"),
                     (_tono, "EMP-TON", "Tono"), (_budi, "EMP-BUD", "Budi"),
                 })
        {
            ctx.MstWorkforceProfiles.Add(new MstWorkforceProfile { Id = id, ProfileCode = kode, DisplayName = nama, UserType = UserType.Employee, IsActive = true });
            ctx.WfpOrganizationAssignments.Add(new WfpOrganizationAssignment
            {
                WorkforceProfileId = id, OrganizationUnitId = _icu, DepartmentId = Guid.NewGuid(), PositionId = Guid.NewGuid(),
                AssignmentType = "Primary", EffectiveStartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsActive = true,
            });
        }

        ctx.Users.Add(new ApplicationUser { Id = _sari, UserName = "sari", UserCode = "SARI0450", DisplayName = "Sari", UserType = UserType.SuperAdmin, IsActive = true });

        foreach (var (id, status) in new[] { (_periode, "Published"), (_draf, "Draft") })
        {
            ctx.TrxRosterPeriods.Add(new TrxRosterPeriod
            {
                Id = id, OrganizationUnitId = _icu, RosterPeriodCode = "R450-" + status, RosterPeriodName = "ICU Feb " + status, RosterStatus = status,
                IsPublished = status == "Published", VersionNumber = 2, PeriodStartDate = new DateOnly(2027, 2, 1), PeriodEndDate = new DateOnly(2027, 2, 28),
                IsActive = true, TotalAssignmentCount = 2,
            });
        }

        ctx.TrxRosterAssignments.Add(new TrxRosterAssignment { Id = _raIndra, RosterPeriodId = _periode, WorkforceProfileId = _indra, AssignmentStatus = "Published", PlannedShiftCount = 2, IsActive = true });
        ctx.TrxRosterAssignments.Add(new TrxRosterAssignment { Id = _raBudi, RosterPeriodId = _periode, WorkforceProfileId = _budi, AssignmentStatus = "Published", PlannedShiftCount = 1, IsActive = true });
        ctx.TrxShiftAssignments.AddRange(
            Shift(_indra16, _raIndra, _indra, T16),
            Shift(_indra17, _raIndra, _indra, T17),
            Shift(_budi18, _raBudi, _budi, T18));
        ctx.SaveChanges();
    }

    public void Dispose() => _db.Dispose();

    private TrxShiftAssignment Shift(Guid id, Guid ra, Guid pegawai, DateOnly tanggal) => new()
    {
        Id = id, RosterAssignmentId = ra, WorkforceProfileId = pegawai, ShiftId = _pagi, ShiftDate = tanggal,
        ScheduledStartAt = DateTime.SpecifyKind(tanggal.ToDateTime(new TimeOnly(0, 0)), DateTimeKind.Utc),
        ScheduledEndAt = DateTime.SpecifyKind(tanggal.ToDateTime(new TimeOnly(7, 0)), DateTimeKind.Utc),
        PlannedWorkMinutes = 420, AssignmentStatus = "Published", IsActive = true,
    };

    private RosterPeriodService Roster()
    {
        var ctx = _db.CreateContext();
        var kesiapan = new KesiapanTiruan();
        var scope = new HrdUnitScopeService(ctx, Pelaku.Izin(ctx), Pelaku.Sebagai(_sari));
        return new RosterPeriodService(ctx, new RosterValidationService(ctx, kesiapan, new SetelanBawaan(), NullLogger<RosterValidationService>.Instance),
            kesiapan, NullLogger<RosterPeriodService>.Instance, scope, new ShiftRevisionRecorder(ctx));
    }

    private ShiftReplacementService Ganti()
    {
        var ctx = _db.CreateContext();
        var kesiapan = new KesiapanAtur(Array.Empty<Guid>(), Array.Empty<Guid>());
        var scope = new HrdUnitScopeService(ctx, Pelaku.Izin(ctx), Pelaku.Sebagai(_sari));
        return new ShiftReplacementService(ctx, kesiapan, NullLogger<ShiftReplacementService>.Instance, Pengganti.Buat<IServiceProvider>(), scope,
            new RosterValidationService(ctx, kesiapan, new SetelanBawaan(), NullLogger<RosterValidationService>.Instance),
            new ShiftRevisionRecorder(ctx), new PemberitahuanTiruan(), new SetelanBawaan());
    }

    private async Task GantiPetugas(Guid shiftId, Guid pengganti, Guid harap)
    {
        var r = await Ganti().ExecuteDirectReplacementAsync(shiftId,
            new DirectShiftReplacementRequest { ReplacementEmployeeId = pengganti, ExpectedWorkforceProfileId = harap, ReasonCategory = "Sakit Mendadak", Notes = "Uji BE-HRD-450" },
            _sari);
        Assert.True(r.Success, r.Message);
    }

    private async Task<RosterPeriodDetailResponse> Detail()
    {
        var r = await Roster().GetByIdAsync(_periode);
        Assert.True(r.Success, r.Message);
        return r.Data!;
    }

    // AC1 + AC2: Raka tanpa penugasan roster mendapat baris pengganti-saja; baris Indra tetap ada tanpa 16 Feb.
    [Fact]
    public async Task Pengganti_tanpa_penugasan_tampil_di_baris_pengganti_saja_dan_baris_awal_tetap_ada()
    {
        await GantiPetugas(_indra16, _raka, _indra);

        var detail = await Detail();
        Assert.Equal(3, detail.Assignments.Count);

        var indra = Assert.Single(detail.Assignments, a => a.WorkforceProfileId == _indra);
        Assert.Equal(_raIndra, indra.Id);
        Assert.False(indra.IsReplacementOnly);
        var milikIndra = Assert.Single(indra.ShiftAssignments);
        Assert.Equal(_indra17, milikIndra.Id);
        Assert.False(milikIndra.IsReplacement);
        Assert.Null(milikIndra.OriginalWorkforceProfileId);
        Assert.Null(milikIndra.OriginalWorkforceDisplayName);
        Assert.Equal(_indra, milikIndra.WorkforceProfileId);
        Assert.Equal("Indra Setiawan", milikIndra.WorkforceDisplayName);

        var raka = detail.Assignments[^1];
        Assert.Equal(_raka, raka.WorkforceProfileId);
        Assert.True(raka.IsReplacementOnly);
        Assert.Equal(Guid.Empty, raka.Id);
        Assert.Equal(_periode, raka.RosterPeriodId);
        Assert.Equal("EMP-RAK", raka.WorkforceProfileCode);
        Assert.Equal("Raka Firmansyah", raka.WorkforceDisplayName);
        Assert.Equal("Published", raka.AssignmentStatus);
        Assert.Equal(1, raka.PlannedShiftCount);

        var pengganti = Assert.Single(raka.ShiftAssignments);
        Assert.Equal(_indra16, pengganti.Id);
        Assert.Equal(T16, pengganti.ShiftDate);
        Assert.Equal("Pagi", pengganti.ShiftName);
        Assert.True(pengganti.IsReplacement);
        Assert.Equal(_raka, pengganti.WorkforceProfileId);
        Assert.Equal("EMP-RAK", pengganti.WorkforceProfileCode);
        Assert.Equal("Raka Firmansyah", pengganti.WorkforceDisplayName);
        Assert.Equal(_indra, pengganti.OriginalWorkforceProfileId);
        Assert.Equal("Indra Setiawan", pengganti.OriginalWorkforceDisplayName);

        // Baris Budi tidak tersentuh.
        Assert.Equal(_budi18, Assert.Single(Assert.Single(detail.Assignments, a => a.WorkforceProfileId == _budi).ShiftAssignments).Id);
    }

    // AC3: Ganti Petugas berikutnya memakai petugas sekarang (Raka); memakai petugas awal ditolak 409 (VR-PK-37).
    [Fact]
    public async Task Ganti_berikutnya_dari_sel_pengganti_memakai_petugas_sekarang()
    {
        await GantiPetugas(_indra16, _raka, _indra);

        var salah = await Ganti().ExecuteDirectReplacementAsync(_indra16,
            new DirectShiftReplacementRequest { ReplacementEmployeeId = _tono, ExpectedWorkforceProfileId = _indra, ReasonCategory = "Sakit Mendadak", Notes = "Data lama" },
            _sari);
        Assert.False(salah.Success);
        Assert.Equal(409, salah.StatusCode);

        await GantiPetugas(_indra16, _tono, _raka);

        var detail = await Detail();
        Assert.DoesNotContain(detail.Assignments, a => a.WorkforceProfileId == _raka);
        var tono = Assert.Single(detail.Assignments, a => a.WorkforceProfileId == _tono);
        Assert.True(tono.IsReplacementOnly);
        var shift = Assert.Single(tono.ShiftAssignments);
        Assert.Equal(_indra16, shift.Id);
        Assert.True(shift.IsReplacement);
        Assert.Equal(_indra, shift.OriginalWorkforceProfileId); // petugas pertama tetap Indra
        Assert.Equal("Indra Setiawan", shift.OriginalWorkforceDisplayName);
    }

    // Pengganti yang sudah punya baris di roster: shift masuk ke barisnya sendiri, tanpa baris tambahan.
    [Fact]
    public async Task Pengganti_yang_punya_penugasan_menerima_shift_di_barisnya_sendiri()
    {
        await GantiPetugas(_budi18, _indra, _budi);

        var detail = await Detail();
        Assert.Equal(2, detail.Assignments.Count);
        Assert.DoesNotContain(detail.Assignments, a => a.IsReplacementOnly);

        var budi = Assert.Single(detail.Assignments, a => a.WorkforceProfileId == _budi);
        Assert.Empty(budi.ShiftAssignments);

        var indra = Assert.Single(detail.Assignments, a => a.WorkforceProfileId == _indra);
        Assert.Equal(new[] { T16, T17, T18 }, indra.ShiftAssignments.Select(s => s.ShiftDate));
        var dariBudi = indra.ShiftAssignments.Single(s => s.Id == _budi18);
        Assert.True(dariBudi.IsReplacement);
        Assert.Equal(_budi, dariBudi.OriginalWorkforceProfileId);
        Assert.Equal("Budi", dariBudi.OriginalWorkforceDisplayName);
        Assert.All(indra.ShiftAssignments.Where(s => s.Id != _budi18), s => Assert.False(s.IsReplacement));
    }

    // Dikembalikan ke petugas awal: bukan pengganti lagi dan kembali ke baris awal.
    [Fact]
    public async Task Kembali_ke_petugas_awal_tidak_ditandai_pengganti()
    {
        await GantiPetugas(_indra16, _raka, _indra);
        await GantiPetugas(_indra16, _indra, _raka);

        var detail = await Detail();
        Assert.Equal(2, detail.Assignments.Count);
        var indra = Assert.Single(detail.Assignments, a => a.WorkforceProfileId == _indra);
        Assert.Equal(new[] { _indra16, _indra17 }, indra.ShiftAssignments.Select(s => s.Id));
        Assert.All(indra.ShiftAssignments, s =>
        {
            Assert.False(s.IsReplacement);
            Assert.Null(s.OriginalWorkforceProfileId);
        });
    }

    // Shift nonaktif (dihapus dari roster) tidak tampil — perilaku yang ada dipertahankan.
    [Fact]
    public async Task Shift_nonaktif_tidak_tampil()
    {
        using (var ctx = _db.CreateContext())
        {
            var s = ctx.TrxShiftAssignments.Single(x => x.Id == _indra17);
            s.IsActive = false;
            s.AssignmentStatus = "Cancelled";
            ctx.SaveChanges();
        }

        var indra = Assert.Single((await Detail()).Assignments, a => a.WorkforceProfileId == _indra);
        Assert.Equal(_indra16, Assert.Single(indra.ShiftAssignments).Id);
    }

    // Jalur tambah penugasan (roster draf) dan tambah shift (roster terbit) mengisi ShiftName dan petugas pada butir shift.
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Respons_tambah_mengisi_nama_shift_dan_petugas(bool terbit)
    {
        var req = new CreateRosterAssignmentRequest
        {
            WorkforceProfileId = _tono,
            ReasonCategory = "Kekurangan tenaga",
            ReasonNote = "Uji BE-HRD-450",
            Shifts = new List<RosterShiftItemRequest> { new() { ShiftDate = T19, ShiftId = _pagi } },
        };

        var r = await Roster().AddAssignmentAsync(terbit ? _periode : _draf, req, _sari);
        Assert.True(r.Success, r.Message);

        var butir = Assert.Single(r.Data!.ShiftAssignments);
        Assert.Equal("Pagi", butir.ShiftName);
        Assert.Equal(_tono, butir.WorkforceProfileId);
        Assert.Equal("EMP-TON", butir.WorkforceProfileCode);
        Assert.Equal("Tono", butir.WorkforceDisplayName);
        Assert.False(butir.IsReplacement);
    }
}
