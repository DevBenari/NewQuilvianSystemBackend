using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Enums.HumanResource;
using System.Security.Claims;

namespace QuilvianSystemBackend.Tests.HumanResource;

public sealed class SetelanBawaan : ISchedulingSettingService
{
    public Task<RecruitmentResult<SchedulingSettingResponse>> GetSettingAsync(Guid hospitalSiteId, CancellationToken ct = default) =>
        Task.FromResult(RecruitmentResult<SchedulingSettingResponse>.Ok(new SchedulingSettingResponse()));

    public Task<RecruitmentResult<SchedulingSettingResponse>> UpdateSettingAsync(Guid hospitalSiteId, UpdateSchedulingSettingRequest request, ClaimsPrincipal user, CancellationToken ct = default) =>
        throw new NotSupportedException();
}

public sealed class KesiapanTiruan(params Guid[] gagal) : IHrdEmploymentReadinessReadService
{
    public Task<RecruitmentResult<SchedulingEligibilityResponse>> GetSchedulingEligibilityAsync(Guid workforceProfileId, DateOnly targetDate, CancellationToken ct = default) =>
        Task.FromResult(gagal.Contains(workforceProfileId)
            ? RecruitmentResult<SchedulingEligibilityResponse>.Fail("tidak terbaca", 503)
            : RecruitmentResult<SchedulingEligibilityResponse>.Ok(new SchedulingEligibilityResponse { WorkforceProfileId = workforceProfileId, TargetDate = targetDate }));
}

// BE-HRD-337 — contoh 35.9.2 dan AT-HRD-PK-11..14.
public sealed class RosterValidationTests : IDisposable
{
    private readonly TestDatabase _db = TestDatabase.Create();

    private readonly Guid _icu = Guid.NewGuid(), _hcu = Guid.NewGuid();
    private readonly Guid _perawat = Guid.NewGuid();
    private readonly Guid _kompIcu = Guid.NewGuid(), _kompTanpaData = Guid.NewGuid();
    private readonly Guid _pagi = Guid.NewGuid(), _malam = Guid.NewGuid();

    private readonly Guid _tono = Guid.NewGuid(), _dewi = Guid.NewGuid(), _ani = Guid.NewGuid(), _budi = Guid.NewGuid(),
        _citra = Guid.NewGuid(), _eko = Guid.NewGuid(), _fajar = Guid.NewGuid();

    private readonly Guid _periodeMaret = Guid.NewGuid(), _periodeAgustus = Guid.NewGuid(), _periodeFeb = Guid.NewGuid(),
        _periodeHcu = Guid.NewGuid();

    private Guid _shiftCitra11Feb;

    public RosterValidationTests()
    {
        using var ctx = _db.CreateContext();
        var legal = Guid.NewGuid();
        ctx.MstOrganizationUnits.AddRange(
            new MstOrganizationUnit { Id = _icu, LegalEntityId = legal, UnitCode = "ICU", UnitName = "ICU" },
            new MstOrganizationUnit { Id = _hcu, LegalEntityId = legal, UnitCode = "HCU", UnitName = "HCU" });
        ctx.MstPositions.Add(new MstPosition { Id = _perawat, DepartmentId = Guid.NewGuid(), PositionCode = "PRW", PositionName = "perawat" });
        ctx.MstCompetencies.AddRange(
            new MstCompetency { Id = _kompIcu, CompetencyCode = "ICU", CompetencyName = "ICU Mahir" },
            new MstCompetency { Id = _kompTanpaData, CompetencyCode = "ECMO", CompetencyName = "ECMO" });
        ctx.MstShifts.AddRange(
            new MstShift { Id = _pagi, ShiftCode = "P", ShiftName = "Pagi", StartTime = new TimeOnly(7, 0), EndTime = new TimeOnly(14, 0) },
            new MstShift { Id = _malam, ShiftCode = "M", ShiftName = "Malam", StartTime = new TimeOnly(21, 0), EndTime = new TimeOnly(7, 0), IsNightShift = true });

        foreach (var (id, nama) in new[] { (_tono, "Tono"), (_dewi, "Dewi"), (_ani, "Ani"), (_budi, "Budi"), (_citra, "Citra"), (_eko, "Eko"), (_fajar, "Fajar") })
        {
            ctx.MstWorkforceProfiles.Add(new MstWorkforceProfile { Id = id, ProfileCode = nama, DisplayName = nama, UserType = UserType.Employee, PrimaryPositionId = _perawat });
            // Semua pegawai berpenempatan utama di ICU sejak 2026, kecuali Fajar (HCU).
            ctx.WfpOrganizationAssignments.Add(new WfpOrganizationAssignment
            {
                WorkforceProfileId = id, OrganizationUnitId = id == _fajar ? _hcu : _icu, DepartmentId = Guid.NewGuid(), PositionId = _perawat,
                AssignmentType = "Primary", IsPrimary = true, EffectiveStartDate = Utc(2026, 1, 1), IsActive = true,
            });
        }

        // HRD-DEC-213: Tono ditempatkan sementara di HCU 1–14 Maret 2027.
        ctx.WfpOrganizationAssignments.Add(new WfpOrganizationAssignment
        {
            WorkforceProfileId = _tono, OrganizationUnitId = _hcu, DepartmentId = Guid.NewGuid(), PositionId = _perawat,
            AssignmentType = "Temporary", EffectiveStartDate = Utc(2027, 3, 1), EffectiveEndDate = Utc(2027, 3, 14), IsActive = true,
        });

        // HRD-DEC-201: Dewi tugas belajar 1 Agt 2027 – 31 Jul 2029; satu tugas belajar lain sudah dibatalkan.
        ctx.HrdStudyAssignments.AddRange(
            new HrdStudyAssignment { WorkforceProfileId = _dewi, DecreeNumber = "SK-1", InstitutionName = "UI", StartDate = new DateOnly(2027, 8, 1), EndDate = new DateOnly(2029, 7, 31), Status = "Active" },
            new HrdStudyAssignment { WorkforceProfileId = _ani, DecreeNumber = "SK-2", InstitutionName = "UGM", StartDate = new DateOnly(2027, 8, 1), EndDate = new DateOnly(2027, 8, 31), Status = "Cancelled" });

        // Standar ketenagaan ICU aktif: 3 perawat per shift; skill-mix 1 "ICU Mahir" per shift, dan 1 "ECMO" pada shift Malam (tanpa data kompetensi).
        var standar = new HrdStaffingStandard
        {
            StandardNumber = "STD-ICU-1", HospitalSiteId = Guid.NewGuid(), OrganizationUnitId = _icu, VersionNumber = 1,
            EffectiveStartDate = new DateOnly(2027, 1, 1), StandardStatus = StaffingStandardStatus.Active,
        };
        ctx.HrdStaffingStandards.Add(standar);
        ctx.Set<HrdStaffingStandardLine>().Add(new HrdStaffingStandardLine { StaffingStandardId = standar.Id, PositionId = _perawat, CalculationMethod = StaffingCalculationMethod.FixedMinimum, StaffPerShift = 3 });
        ctx.Set<HrdStaffingSkillMixRule>().AddRange(
            new HrdStaffingSkillMixRule { StaffingStandardId = standar.Id, CompetencyId = _kompIcu, MinimumQualifiedHeadcount = 1, IsMandatory = true },
            new HrdStaffingSkillMixRule { StaffingStandardId = standar.Id, CompetencyId = _kompTanpaData, ShiftId = _malam, MinimumQualifiedHeadcount = 1, IsMandatory = true });
        // Standar draf untuk HCU tidak dihitung.
        ctx.HrdStaffingStandards.Add(new HrdStaffingStandard
        {
            StandardNumber = "STD-HCU-0", HospitalSiteId = Guid.NewGuid(), OrganizationUnitId = _hcu, VersionNumber = 1,
            EffectiveStartDate = new DateOnly(2027, 1, 1), StandardStatus = StaffingStandardStatus.Draft,
        });

        // Citra "ICU Mahir" lulus; Ani pernah gagal.
        ctx.WfpCompetencyAssessments.AddRange(
            new WfpCompetencyAssessment { WorkforceProfileId = _citra, CompetencyId = _kompIcu, AssessmentDate = Utc(2026, 6, 1), CompetencyLevel = CompetencyLevel.Advanced, ResultStatus = CompetencyAssessmentResultStatus.Passed, IsActive = true },
            new WfpCompetencyAssessment { WorkforceProfileId = _ani, CompetencyId = _kompIcu, AssessmentDate = Utc(2026, 6, 1), CompetencyLevel = CompetencyLevel.Basic, ResultStatus = CompetencyAssessmentResultStatus.Failed, IsActive = true });

        Periode(ctx, _periodeMaret, _icu, new DateOnly(2027, 3, 1), new DateOnly(2027, 3, 31));
        Periode(ctx, _periodeAgustus, _icu, new DateOnly(2027, 8, 1), new DateOnly(2027, 8, 31));
        Periode(ctx, _periodeFeb, _icu, new DateOnly(2027, 2, 1), new DateOnly(2027, 2, 28));
        Periode(ctx, _periodeHcu, _hcu, new DateOnly(2027, 2, 1), new DateOnly(2027, 2, 28));

        Shift(ctx, _periodeMaret, _tono, _pagi, new DateOnly(2027, 3, 5));
        Shift(ctx, _periodeMaret, _tono, _pagi, new DateOnly(2027, 3, 15));
        Shift(ctx, _periodeAgustus, _dewi, _pagi, new DateOnly(2027, 8, 3));
        Shift(ctx, _periodeAgustus, _ani, _pagi, new DateOnly(2027, 8, 3));
        // Malam 10 Feb: hanya Ani dan Budi (AT-HRD-PK-13). Pagi 11 Feb: Citra, Ani, Budi (lengkap).
        Shift(ctx, _periodeFeb, _ani, _malam, new DateOnly(2027, 2, 10));
        Shift(ctx, _periodeFeb, _budi, _malam, new DateOnly(2027, 2, 10));
        _shiftCitra11Feb = Shift(ctx, _periodeFeb, _citra, _pagi, new DateOnly(2027, 2, 11));
        Shift(ctx, _periodeFeb, _ani, _pagi, new DateOnly(2027, 2, 12));
        Shift(ctx, _periodeFeb, _budi, _pagi, new DateOnly(2027, 2, 12));
        Shift(ctx, _periodeFeb, _ani, _pagi, new DateOnly(2027, 2, 11));
        Shift(ctx, _periodeFeb, _budi, _pagi, new DateOnly(2027, 2, 11));
        // Eko bekerja 7 hari Pagi berturut-turut 1–7 Feb (W2, batas 6).
        for (var d = 1; d <= 7; d++) Shift(ctx, _periodeFeb, _eko, _pagi, new DateOnly(2027, 2, d));
        // Unit HCU tanpa standar aktif (AT-HRD-PK-14).
        Shift(ctx, _periodeHcu, _fajar, _pagi, new DateOnly(2027, 2, 10));

        ctx.SaveChanges();
    }

    public void Dispose() => _db.Dispose();

    private static DateTime Utc(int y, int m, int d) => new(y, m, d, 0, 0, 0, DateTimeKind.Utc);

    private void Periode(QuilvianSystemBackend.Repositories.ApplicationDbContext ctx, Guid id, Guid unit, DateOnly mulai, DateOnly akhir) =>
        ctx.TrxRosterPeriods.Add(new TrxRosterPeriod
        { Id = id, OrganizationUnitId = unit, RosterPeriodCode = id.ToString("N")[..6], RosterPeriodName = "R", RosterStatus = "Draft", PeriodStartDate = mulai, PeriodEndDate = akhir, IsActive = true });

    private readonly Dictionary<(Guid, Guid), Guid> _rosterAssignments = new();

    private Guid Shift(QuilvianSystemBackend.Repositories.ApplicationDbContext ctx, Guid periode, Guid pegawai, Guid shift, DateOnly tanggal)
    {
        if (!_rosterAssignments.TryGetValue((periode, pegawai), out var ra))
        {
            ra = Guid.NewGuid();
            _rosterAssignments[(periode, pegawai)] = ra;
            ctx.TrxRosterAssignments.Add(new TrxRosterAssignment { Id = ra, RosterPeriodId = periode, WorkforceProfileId = pegawai, PositionId = _perawat });
        }

        var malam = shift == _malam;
        var mulai = tanggal.ToDateTime(malam ? new TimeOnly(14, 0) : new TimeOnly(0, 0), DateTimeKind.Utc);
        var id = Guid.NewGuid();
        ctx.TrxShiftAssignments.Add(new TrxShiftAssignment
        {
            Id = id, RosterAssignmentId = ra, WorkforceProfileId = pegawai, ShiftId = shift, ShiftDate = tanggal,
            ScheduledStartAt = mulai, ScheduledEndAt = mulai.AddHours(malam ? 10 : 7), IsNightShift = malam, IsActive = true,
        });
        return id;
    }

    private RosterValidationService Layanan(params Guid[] kesiapanGagal) =>
        new(_db.CreateContext(), new KesiapanTiruan(kesiapanGagal), new SetelanBawaan(), NullLogger<RosterValidationService>.Instance);

    private static List<RosterValidationConflictItemDto> Kode(RosterValidationResultResponse r, string kode) =>
        r.ConflictItems.Where(x => x.RuleCode == kode).ToList();

    // AT-HRD-PK-11
    [Fact]
    public async Task B3_penempatan_sementara_di_unit_lain_mengalahkan_penempatan_utama()
    {
        var r = await Layanan().ValidateRosterPeriodAsync(_periodeMaret, Guid.NewGuid());
        var b3 = Assert.Single(Kode(r, "B3"));
        Assert.Equal(new DateOnly(2027, 3, 5), b3.ShiftDate);
        Assert.Equal("BLOCKING", b3.Severity);
        Assert.Equal("VR-PK-01 (B3): Tono ditempatkan di HCU 1–14 Maret 2027.", b3.Message);
        Assert.False(r.IsValidationPassed);

        // Shift 15 Maret tetap lolos dan status per shift tersimpan.
        await using var ctx = _db.CreateContext();
        var lolos = ctx.TrxShiftAssignments.Single(s => s.WorkforceProfileId == _tono && s.ShiftDate == new DateOnly(2027, 3, 15));
        Assert.True(lolos.IsValidationPassed);
        Assert.False(ctx.TrxShiftAssignments.Single(s => s.WorkforceProfileId == _tono && s.ShiftDate == new DateOnly(2027, 3, 5)).IsValidationPassed);
    }

    // AT-HRD-PK-12
    [Fact]
    public async Task B8_tugas_belajar_aktif_memblokir_yang_dibatalkan_tidak()
    {
        var r = await Layanan().ValidateRosterPeriodAsync(_periodeAgustus, Guid.NewGuid());
        var b8 = Assert.Single(Kode(r, "B8"));
        Assert.Equal(_dewi, b8.WorkforceProfileId);
        Assert.Equal("BLOCKING", b8.Severity);
        Assert.Equal("VR-PK-02 (B8): Dewi sedang tugas belajar 1 Agustus 2027 – 31 Juli 2029.", b8.Message);
    }

    // AT-HRD-PK-13
    [Fact]
    public async Task W6_dan_W7_peringatan_pada_slot_kurang_tenaga_dan_tanpa_kompetensi()
    {
        var r = await Layanan().ValidateRosterPeriodAsync(_periodeFeb, Guid.NewGuid());

        var w6 = Kode(r, "W6");
        Assert.Contains(w6, x => x.ShiftDate == new DateOnly(2027, 2, 10) && x.Message == "W6: Shift Malam 10-02-2027 di ICU berisi 2 perawat, kurang dari kuota 3 per shift.");
        Assert.DoesNotContain(w6, x => x.ShiftDate == new DateOnly(2027, 2, 11)); // Citra, Ani, Budi = 3
        Assert.All(w6, x => Assert.Equal("WARNING", x.Severity));
        Assert.All(w6, x => Assert.Equal(Guid.Empty, x.WorkforceProfileId));

        var w7 = Kode(r, "W7");
        Assert.Contains(w7, x => x.ShiftDate == new DateOnly(2027, 2, 12)); // Ani (gagal) + Budi
        Assert.DoesNotContain(w7, x => x.ShiftDate == new DateOnly(2027, 2, 11)); // Citra lulus

        // W7 ECMO pada shift Malam tidak dievaluasi: belum ada data kompetensi -> keterangan.
        Assert.Contains("Data kompetensi ECMO belum tersedia; komposisi keahlian (W7) tidak diperiksa", r.InfoMessages);
        Assert.DoesNotContain(w7, x => x.Message.Contains("ECMO"));

        // Peringatan ikut dihitung sehingga terbit membutuhkan alasan; tidak memblokir.
        Assert.Equal(r.ConflictItems.Count(x => x.Severity == "WARNING"), r.WarningConflicts);
        Assert.DoesNotContain(r.ConflictItems, x => x.RuleCode is "W6" or "W7" && x.Severity == "BLOCKING");
    }

    // AT-HRD-PK-14
    [Fact]
    public async Task Tanpa_standar_aktif_tidak_ada_W6_hanya_keterangan()
    {
        var r = await Layanan().ValidateRosterPeriodAsync(_periodeHcu, Guid.NewGuid());
        Assert.Empty(Kode(r, "W6"));
        Assert.Empty(Kode(r, "W7"));
        Assert.Equal(new[] { "Kuota unit belum ditetapkan" }, r.InfoMessages);
        Assert.Equal(0, r.WarningConflicts);
        Assert.True(r.IsValidationPassed);
    }

    [Fact]
    public async Task W2_kini_bercode_dan_muncul_sebagai_item()
    {
        var r = await Layanan().ValidateRosterPeriodAsync(_periodeFeb, Guid.NewGuid());
        var w2 = Assert.Single(Kode(r, "W2"));
        Assert.Equal(_eko, w2.WorkforceProfileId);
        Assert.Equal(new DateOnly(2027, 2, 7), w2.ShiftDate);
        Assert.Equal("WARNING", w2.Severity);
    }

    [Fact]
    public async Task B6_gagal_tertutup_tetap_berlaku()
    {
        var r = await Layanan(_dewi).ValidateRosterPeriodAsync(_periodeAgustus, Guid.NewGuid());
        Assert.Contains(r.ConflictItems, x => x.RuleCode == "B6" && x.WorkforceProfileId == _dewi);
    }

    // ---------- ValidateSingleShiftAsync ----------

    private RosterSingleShiftValidationRequest Calon(Guid periode, Guid pegawai, Guid shift, DateOnly tanggal, Guid? menggantikan = null)
    {
        var malam = shift == _malam;
        var mulai = tanggal.ToDateTime(malam ? new TimeOnly(14, 0) : new TimeOnly(0, 0), DateTimeKind.Utc);
        return new RosterSingleShiftValidationRequest
        {
            RosterPeriodId = periode, ShiftAssignmentId = menggantikan, WorkforceProfileId = pegawai, ShiftId = shift, ShiftDate = tanggal,
            ScheduledStartAt = mulai, ScheduledEndAt = mulai.AddHours(malam ? 10 : 7), IsNightShift = malam,
        };
    }

    [Fact]
    public async Task Satu_shift_B3_dan_B8_sama_dengan_terbit()
    {
        var tono5 = await Layanan().ValidateSingleShiftAsync(Calon(_periodeMaret, _tono, _pagi, new DateOnly(2027, 3, 6)));
        Assert.False(tono5!.IsPassed);
        Assert.Contains(tono5.Items, x => x.RuleCode == "B3" && x.Message == "VR-PK-01 (B3): Tono ditempatkan di HCU 1–14 Maret 2027.");

        var tono16 = await Layanan().ValidateSingleShiftAsync(Calon(_periodeMaret, _tono, _pagi, new DateOnly(2027, 3, 16)));
        Assert.DoesNotContain(tono16!.Items, x => x.Severity == "BLOCKING");

        var dewi = await Layanan().ValidateSingleShiftAsync(Calon(_periodeAgustus, _dewi, _pagi, new DateOnly(2027, 8, 10)));
        Assert.Contains(dewi!.Items, x => x.RuleCode == "B8");
    }

    [Fact]
    public async Task Satu_shift_ganti_petugas_menilai_komposisi_slot_tanpa_petugas_lama()
    {
        // Pagi 11 Feb: Citra (ICU Mahir) diganti Eko. Slot tetap 3 orang (W6 aman) tetapi tanpa kompetensi (W7).
        var r = await Layanan().ValidateSingleShiftAsync(Calon(_periodeFeb, _eko, _pagi, new DateOnly(2027, 2, 11), _shiftCitra11Feb));
        Assert.DoesNotContain(r!.Items, x => x.RuleCode == "W6");
        var w7 = Assert.Single(r.Items, x => x.RuleCode == "W7");
        Assert.Equal(_shiftCitra11Feb, w7.ShiftAssignmentId);

        // Eko sudah bekerja 1–7 Feb; menambah 8 Feb memperpanjang rangkaian -> W2 pada kandidat.
        var tambah = await Layanan().ValidateSingleShiftAsync(Calon(_periodeFeb, _eko, _pagi, new DateOnly(2027, 2, 8)));
        Assert.Contains(tambah!.Items, x => x.RuleCode == "W2" && x.ShiftDate == new DateOnly(2027, 2, 8));

        // B1: Ani sudah Pagi 11 Feb; menempatkan Ani lagi di Pagi 11 Feb (shift baru) bertabrakan.
        var ganda = await Layanan().ValidateSingleShiftAsync(Calon(_periodeFeb, _ani, _pagi, new DateOnly(2027, 2, 11)));
        Assert.Contains(ganda!.Items, x => x.RuleCode == "B1");

        // Periode tidak ada -> null.
        Assert.Null(await Layanan().ValidateSingleShiftAsync(Calon(Guid.NewGuid(), _eko, _pagi, new DateOnly(2027, 2, 11))));
    }

    [Fact]
    public async Task Satu_shift_tidak_menyimpan_apa_pun()
    {
        await Layanan().ValidateSingleShiftAsync(Calon(_periodeMaret, _tono, _pagi, new DateOnly(2027, 3, 6)));
        await using var ctx = _db.CreateContext();
        Assert.Null(ctx.TrxRosterPeriods.Single(p => p.Id == _periodeMaret).ValidatedAt);
        Assert.All(ctx.TrxShiftAssignments.Where(s => s.RosterAssignment!.RosterPeriodId == _periodeMaret).ToList(), s => Assert.Null(s.ValidatedAt));
    }
}
