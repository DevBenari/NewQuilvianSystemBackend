using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services;
using QuilvianSystemBackend.Tests.OperatingRoom.Infrastructure;

namespace QuilvianSystemBackend.Tests.OperatingRoom;

/// <summary>
/// Regresi mesin status kasus operasi di bawah aturan klinis penuh (`BE-OPR-005/006/007`).
/// </summary>
/// <remarks>
/// Setiap kasus dibawa ke statusnya lewat service sungguhan, bukan dengan menulis status ke
/// basis data. Karena itu uji yang lulus di sini sekaligus membuktikan seluruh gerbang menuju
/// status itu memang dapat dilewati, dan uji yang gagal menunjuk gerbang yang rusak.
/// </remarks>
public class StateTransitionTests
{
    [Fact]
    public async Task Kasus_baru_berstatus_diminta()
    {
        using var harness = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(harness);
        var alur = new AlurOperasi(harness, seed);

        var kasusId = await alur.BuatKasusAsync();

        Assert.Equal(OprCaseStatus.Requested, await alur.StatusAsync(kasusId));
    }

    [Fact]
    public async Task Penjadwalan_membawa_kasus_ke_terjadwal()
    {
        using var harness = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(harness);
        var alur = new AlurOperasi(harness, seed);

        var kasusId = await alur.BuatKasusAsync();
        await alur.JadwalkanAsync(kasusId);

        Assert.Equal(OprCaseStatus.Scheduled, await alur.StatusAsync(kasusId));
    }

    [Fact]
    public async Task Kesiapan_menunggu_seluruh_syarat_sebelum_naik_ke_siap()
    {
        using var harness = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(harness);
        var alur = new AlurOperasi(harness, seed);

        var kasusId = await alur.BuatKasusAsync();
        await alur.JadwalkanAsync(kasusId);
        await alur.ChecklistSignInSelesaiAsync(kasusId);

        await alur.SignOffAsync(kasusId, OprReadinessRole.PrimarySurgeon);
        Assert.Equal(OprCaseStatus.Scheduled, await alur.StatusAsync(kasusId));

        await alur.SignOffAsync(kasusId, OprReadinessRole.Anesthesiologist);
        Assert.Equal(OprCaseStatus.Scheduled, await alur.StatusAsync(kasusId));

        await alur.SignOffAsync(kasusId, OprReadinessRole.Nurse);
        Assert.Equal(OprCaseStatus.Ready, await alur.StatusAsync(kasusId));
    }

    [Fact]
    public async Task Tanpa_checklist_kasus_tidak_pernah_naik_ke_siap()
    {
        using var harness = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(harness);
        var alur = new AlurOperasi(harness, seed);

        var kasusId = await alur.BuatKasusAsync();
        await alur.JadwalkanAsync(kasusId);

        await alur.SignOffAsync(kasusId, OprReadinessRole.PrimarySurgeon);
        await alur.SignOffAsync(kasusId, OprReadinessRole.Anesthesiologist);
        await alur.SignOffAsync(kasusId, OprReadinessRole.Nurse);

        Assert.Equal(OprCaseStatus.Scheduled, await alur.StatusAsync(kasusId));

        var persiapan = await harness.PreparationService(OperatingRoomSeed.AkunBedah).GetAsync(kasusId);
        Assert.Contains(persiapan!.OutstandingRequirements,
            x => x.Contains("Checklist", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Operasi_hanya_dapat_dimulai_dari_status_siap()
    {
        using var harness = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(harness);
        var alur = new AlurOperasi(harness, seed);

        var kasusId = await alur.BuatKasusAsync();

        var galat = await Assert.ThrowsAsync<OperatingRoomConflictException>(
            () => alur.MulaiAsync(kasusId));
        Assert.Equal("InvalidStateTransition", galat.Code);

        await alur.JadwalkanAsync(kasusId);
        await alur.ChecklistSignInSelesaiAsync(kasusId);
        await alur.SignOffAsync(kasusId, OprReadinessRole.PrimarySurgeon);
        await alur.SignOffAsync(kasusId, OprReadinessRole.Anesthesiologist);
        await alur.SignOffAsync(kasusId, OprReadinessRole.Nurse);
        await alur.MulaiAsync(kasusId);

        Assert.Equal(OprCaseStatus.InProgress, await alur.StatusAsync(kasusId));
    }

    [Fact]
    public async Task Kasus_yang_sudah_dimulai_tidak_dapat_dibatalkan()
    {
        using var harness = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(harness);
        var alur = new AlurOperasi(harness, seed);

        var kasusId = await alur.KasusBerjalanAsync();
        var versi = await alur.VersiAsync(kasusId);

        var galat = await Assert.ThrowsAsync<OperatingRoomConflictException>(() =>
            harness.ExecutionService(OperatingRoomSeed.AkunBedah).CancelAsync(kasusId,
                new CancelOprCaseRequest
                {
                    Reason = "Uji pembatalan setelah mulai",
                    IdempotencyKey = alur.Kunci("BATAL"),
                    ExpectedVersion = versi
                }));

        Assert.Equal("InvalidStateTransition", galat.Code);
        Assert.Equal(OprCaseStatus.InProgress, await alur.StatusAsync(kasusId));
    }

    [Fact]
    public async Task Selesai_hanya_setelah_catatan_final_recovery_keluar_dan_serah_terima_diterima()
    {
        using var harness = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(harness);
        var alur = new AlurOperasi(harness, seed);

        var kasusId = await alur.KasusBerjalanAsync();

        // 1. Catatan operasi difinalisasi.
        await harness.ExecutionService(OperatingRoomSeed.AkunBedah).SaveRecordAsync(kasusId,
            new SaveOprExecutionRecordRequest
            {
                PreDiagnosis = "Pra uji", PostDiagnosis = "Pasca uji", Findings = "Temuan uji",
                Technique = "Teknik uji", PostPlan = "Rencana uji",
                Finalize = true, Outcome = OprCaseOutcome.Completed,
                IdempotencyKey = alur.Kunci("CATATAN"), ExpectedRecordVersion = 0
            });
        Assert.Equal(OprCaseStatus.InProgress, await alur.StatusAsync(kasusId));

        // 2. Pasien keluar dari recovery. Hanya dokter anestesi tim yang boleh memutuskan.
        var recovery = await harness.RecoveryService(OperatingRoomSeed.AkunAnestesi).SaveRecoveryAsync(kasusId,
            new SaveOprRecoveryRequest
            {
                ScoreSystem = "Aldrete", ScoreValue = 9,
                Status = OprRecoveryStatus.Released, Decision = OprRecoveryDecision.Inpatient,
                IdempotencyKey = alur.Kunci("RECOVERY"), ExpectedRecordVersion = 0
            });
        Assert.Equal(OprRecoveryStatus.Released, recovery.Status);
        Assert.Equal(OprCaseStatus.InProgress, await alur.StatusAsync(kasusId));

        // 3. Serah terima dikirim; kasus masih belum selesai sebelum diterima.
        var serahTerima = await harness.RecoveryService(OperatingRoomSeed.AkunAnestesi).CreateHandoverAsync(kasusId,
            new CreateOprHandoverRequest
            {
                DestinationUnitId = seed.UnitTujuanId,
                ConditionSummary = "Kondisi stabil",
                IdempotencyKey = alur.Kunci("SERAH")
            });
        Assert.Equal(OprCaseStatus.InProgress, await alur.StatusAsync(kasusId));

        // 4. Diterima unit tujuan.
        await harness.RecoveryService(OperatingRoomSeed.AkunPerawat).AcceptHandoverAsync(kasusId, serahTerima.Id,
            new AcceptOprHandoverRequest { Accept = true, IdempotencyKey = alur.Kunci("TERIMA") });

        Assert.Equal(OprCaseStatus.Completed, await alur.StatusAsync(kasusId));
    }

    [Fact]
    public async Task Catatan_operasi_yang_sudah_final_hanya_dapat_diperbaiki_lewat_addendum()
    {
        using var harness = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(harness);
        var alur = new AlurOperasi(harness, seed);

        var kasusId = await alur.KasusBerjalanAsync();
        var eksekusi = harness.ExecutionService(OperatingRoomSeed.AkunBedah);

        await eksekusi.SaveRecordAsync(kasusId, new SaveOprExecutionRecordRequest
        {
            PreDiagnosis = "Pra", PostDiagnosis = "Pasca", Findings = "Temuan",
            Technique = "Teknik", PostPlan = "Rencana",
            Finalize = true, Outcome = OprCaseOutcome.Completed,
            IdempotencyKey = alur.Kunci("FINAL"), ExpectedRecordVersion = 0
        });

        var galat = await Assert.ThrowsAsync<OperatingRoomUnprocessableException>(() =>
            harness.ExecutionService(OperatingRoomSeed.AkunBedah).SaveRecordAsync(kasusId,
                new SaveOprExecutionRecordRequest
                {
                    PreDiagnosis = "Pra diubah", PostDiagnosis = "Pasca", Findings = "Temuan",
                    Technique = "Teknik", PostPlan = "Rencana",
                    IdempotencyKey = alur.Kunci("UBAH"), ExpectedRecordVersion = 1
                }));

        Assert.Equal("OPR010", galat.Code);

        var addendum = await harness.ExecutionService(OperatingRoomSeed.AkunBedah).CreateAddendumAsync(kasusId,
            new CreateOprExecutionAddendumRequest
            {
                Content = "Koreksi perdarahan menjadi 150 ml",
                Reason = "Salah catat saat operasi",
                IdempotencyKey = alur.Kunci("ADD")
            });

        Assert.NotEqual(Guid.Empty, addendum.Id);
    }

    [Fact]
    public async Task Finalisasi_tanpa_hasil_operasi_ditolak()
    {
        using var harness = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(harness);
        var alur = new AlurOperasi(harness, seed);

        var kasusId = await alur.KasusBerjalanAsync();

        var galat = await Assert.ThrowsAsync<OperatingRoomUnprocessableException>(() =>
            harness.ExecutionService(OperatingRoomSeed.AkunBedah).SaveRecordAsync(kasusId,
                new SaveOprExecutionRecordRequest
                {
                    PreDiagnosis = "Pra", PostDiagnosis = "Pasca", Findings = "Temuan",
                    Technique = "Teknik", PostPlan = "Rencana",
                    Finalize = true, Outcome = null,
                    IdempotencyKey = alur.Kunci("TANPAHASIL"), ExpectedRecordVersion = 0
                }));

        Assert.Equal("OutcomeRequired", galat.Code);
    }
}
