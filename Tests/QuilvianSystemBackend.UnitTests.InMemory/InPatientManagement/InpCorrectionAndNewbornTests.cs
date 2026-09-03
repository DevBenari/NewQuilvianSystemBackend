using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Enums;

namespace QuilvianSystemBackend.Tests.InPatientManagement;

/// <summary>
/// <c>BE-RWI-030</c> — kesalahan catatan dapat dibetulkan tanpa membongkar episode;
/// <c>BE-RWI-031</c> — bayi baru lahir punya episode sendiri di boks kamar ibunya.
/// </summary>
public sealed class InpCorrectionAndNewbornTests
{
    // =========================================================================
    // BE-RWI-030 — Sesi koreksi
    // =========================================================================

    [Fact]
    public async Task Kriteria1_HanyaSupervisorYangDapatMembukaSesiKoreksi()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();
        var episode = await BuatEpisodeTertutupAsync(world);

        var hasil = await world.EpisodeService.OpenCorrectionSessionAsync(
            episode.Id,
            new OpenCorrectionSessionRequest
            {
                OpenReason = "Diagnosis utama keliru, perlu dibetulkan."
            },
            InpatientEpisodeTestWorld.ActorUserId,
            actorIsSupervisor: false);

        Assert.Equal(InpEpisodeOperationStatus.Forbidden, hasil.Status);
        Assert.Equal("Hanya supervisor yang dapat membuka kembali episode.", hasil.Message);
    }

    /// <remarks>
    /// Kriteria 2 dan 3 sekaligus. <b>Sesi koreksi bukan status episode keenam.</b> Status
    /// tetap <c>Closed</c>, tempat tidur tidak dikembalikan, dan lama dirawat tidak bertambah
    /// — ketiganya diperiksa sebelum dan sesudah sesi dibuka.
    /// </remarks>
    [Fact]
    public async Task Kriteria2Dan3_StatusTetapClosedTempatTidurTidakKembaliDanLamaDirawatTidakBertambah()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");

        var episode = await world.BuildClosableEpisodeAsync(bed);

        await world.DischargeService.CloseEpisodeAsync(
            episode.Id,
            null,
            InpatientEpisodeTestWorld.ActorUserId);

        var sebelumSesi = await world.DbContext.Set<InpEpisode>()
            .AsNoTracking()
            .FirstAsync(x => x.Id == episode.Id);

        var bedSebelum = await world.DbContext.Set<MstBed>()
            .AsNoTracking()
            .FirstAsync(x => x.Id == bed.Id);

        var censusSebelum = await world.CensusQueryService.GetCensusAsync(new CensusQuery());

        var riwayatSebelum = await world.EpisodeService.GetStatusHistoryAsync(episode.Id);

        var buka = await world.EpisodeService.OpenCorrectionSessionAsync(
            episode.Id,
            new OpenCorrectionSessionRequest
            {
                OpenReason = "Diagnosis utama keliru, dibetulkan setelah hasil kultur keluar."
            },
            InpatientEpisodeTestWorld.SupervisorUserId,
            actorIsSupervisor: true);

        Assert.Equal(InpEpisodeOperationStatus.Success, buka.Status);

        var sesudahSesi = await world.DbContext.Set<InpEpisode>()
            .AsNoTracking()
            .FirstAsync(x => x.Id == episode.Id);

        // Kriteria 2 — status episode TETAP Closed.
        Assert.Equal(InpEpisodeStatus.Closed, sebelumSesi.EpisodeStatus);
        Assert.Equal(InpEpisodeStatus.Closed, sesudahSesi.EpisodeStatus);
        Assert.Equal(sebelumSesi.ClosedAt, sesudahSesi.ClosedAt);

        // Kriteria 3 — tempat tidur tidak dikembalikan kepada episode ini, dan pasien tidak
        // muncul kembali pada census, sehingga lama dirawat tidak bertambah.
        var bedSesudah = await world.DbContext.Set<MstBed>()
            .AsNoTracking()
            .FirstAsync(x => x.Id == bed.Id);

        Assert.Equal(bedSebelum.BedStatus, bedSesudah.BedStatus);
        Assert.Equal(BedStatus.Available, bedSesudah.BedStatus);

        var penempatanAktif = await world.DbContext.Set<InpBedPlacement>()
            .AsNoTracking()
            .CountAsync(x => x.EpisodeId == episode.Id && x.EndDateTime == null);

        Assert.Equal(0, penempatanAktif);

        var censusSesudah = await world.CensusQueryService.GetCensusAsync(new CensusQuery());

        Assert.Equal(censusSebelum.TotalData, censusSesudah.TotalData);
        Assert.Empty(censusSesudah.Items);

        // Tidak ada baris riwayat status baru — sesi koreksi bukan perpindahan status.
        var riwayatSesudah = await world.EpisodeService.GetStatusHistoryAsync(episode.Id);
        Assert.Equal(riwayatSebelum.Count, riwayatSesudah.Count);
    }

    [Fact]
    public async Task Kriteria4_SatuEpisodePunyaPalingBanyakSatuSesiTerbuka()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();
        var episode = await BuatEpisodeTertutupAsync(world);

        var pertama = await world.EpisodeService.OpenCorrectionSessionAsync(
            episode.Id,
            new OpenCorrectionSessionRequest { OpenReason = "Koreksi pertama." },
            InpatientEpisodeTestWorld.SupervisorUserId,
            actorIsSupervisor: true);

        Assert.Equal(InpEpisodeOperationStatus.Success, pertama.Status);

        var kedua = await world.EpisodeService.OpenCorrectionSessionAsync(
            episode.Id,
            new OpenCorrectionSessionRequest { OpenReason = "Koreksi kedua." },
            InpatientEpisodeTestWorld.SupervisorUserId,
            actorIsSupervisor: true);

        Assert.Equal(InpEpisodeOperationStatus.Conflict, kedua.Status);
        Assert.Equal(
            "Episode ini sedang dalam sesi koreksi yang belum ditutup.",
            kedua.Message);

        // Setelah sesi pertama ditutup, sesi berikutnya boleh dibuka.
        await world.EpisodeService.CloseCorrectionSessionAsync(
            episode.Id,
            pertama.SessionId!.Value,
            new CloseCorrectionSessionRequest { ChangedFieldSummary = "Diagnosis utama dibetulkan." },
            InpatientEpisodeTestWorld.SupervisorUserId,
            actorIsSupervisor: true);

        var ketiga = await world.EpisodeService.OpenCorrectionSessionAsync(
            episode.Id,
            new OpenCorrectionSessionRequest { OpenReason = "Koreksi lanjutan." },
            InpatientEpisodeTestWorld.SupervisorUserId,
            actorIsSupervisor: true);

        Assert.Equal(InpEpisodeOperationStatus.Success, ketiga.Status);
    }

    [Fact]
    public async Task Kriteria5_MenutupSesiMenyimpanDaftarPerubahannya()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();
        var episode = await BuatEpisodeTertutupAsync(world);

        var buka = await world.EpisodeService.OpenCorrectionSessionAsync(
            episode.Id,
            new OpenCorrectionSessionRequest { OpenReason = "Diagnosis utama keliru." },
            InpatientEpisodeTestWorld.SupervisorUserId,
            actorIsSupervisor: true);

        var tanpaDaftar = await world.EpisodeService.CloseCorrectionSessionAsync(
            episode.Id,
            buka.SessionId!.Value,
            new CloseCorrectionSessionRequest { ChangedFieldSummary = "   " },
            InpatientEpisodeTestWorld.SupervisorUserId,
            actorIsSupervisor: true);

        Assert.Equal(InpEpisodeOperationStatus.Invalid, tanpaDaftar.Status);
        Assert.Equal(
            "Tuliskan apa saja yang diubah sebelum menutup sesi koreksi.",
            tanpaDaftar.Message);

        var tutup = await world.EpisodeService.CloseCorrectionSessionAsync(
            episode.Id,
            buka.SessionId!.Value,
            new CloseCorrectionSessionRequest
            {
                ChangedFieldSummary = "Diagnosis utama diubah dari demam berdarah menjadi tifoid."
            },
            InpatientEpisodeTestWorld.SupervisorUserId,
            actorIsSupervisor: true);

        Assert.Equal(InpEpisodeOperationStatus.Success, tutup.Status);

        var sesi = await world.EpisodeService.GetCorrectionSessionAsync(
            episode.Id,
            buka.SessionId!.Value);

        Assert.NotNull(sesi);
        Assert.False(sesi!.IsOpen);
        Assert.NotNull(sesi.ClosedAt);
        Assert.Equal(InpatientEpisodeTestWorld.SupervisorUserId, sesi.ClosedByUserId);
        Assert.Equal(
            "Diagnosis utama diubah dari demam berdarah menjadi tifoid.",
            sesi.ChangedFieldSummary);
    }

    /// <remarks>
    /// Kriteria 6 menyambung ke <c>BE-RWI-022</c>: koreksi resume yang sudah ditandatangani
    /// menyimpan versi lamanya. Sampai task ini, sesi koreksi hanya dapat disisipkan langsung
    /// ke database uji; sekarang ia lahir lewat endpoint yang sebenarnya.
    /// </remarks>
    [Fact]
    public async Task Kriteria6_KoreksiResumeDiDalamSesiMenyimpanVersiLamanya()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();
        var episode = await BuatEpisodeTertutupAsync(world);

        var buka = await world.EpisodeService.OpenCorrectionSessionAsync(
            episode.Id,
            new OpenCorrectionSessionRequest { OpenReason = "Diagnosis utama keliru." },
            InpatientEpisodeTestWorld.SupervisorUserId,
            actorIsSupervisor: true);

        Assert.Equal(InpEpisodeOperationStatus.Success, buka.Status);

        var koreksi = await world.DischargeService.UpsertSummaryAsync(
            episode.Id,
            new UpsertDischargeSummaryRequest { PrimaryDiagnosisText = "Demam tifoid" },
            InpatientEpisodeTestWorld.SupervisorUserId,
            world.Doctor.Id,
            actorIsSupervisor: true);

        Assert.Equal(InpEpisodeOperationStatus.Success, koreksi.Status);

        var resume = await world.DischargeService.GetSummaryAsync(
            episode.Id,
            includeRevisions: true);

        Assert.NotNull(resume);
        Assert.Equal("Demam tifoid", resume!.PrimaryDiagnosisText);

        var versi = Assert.Single(resume.Revisions);

        Assert.Equal("Demam berdarah dengue", versi.PrimaryDiagnosisText);
        Assert.Equal(buka.SessionId, versi.CorrectionSessionId);
    }

    [Fact]
    public async Task SesiKoreksiHanyaUntukEpisodeYangSudahDitutup()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");

        var episode = await world.OpenAndPlaceAsync(bed);

        var hasil = await world.EpisodeService.OpenCorrectionSessionAsync(
            episode.Id,
            new OpenCorrectionSessionRequest { OpenReason = "Ada yang perlu dibetulkan." },
            InpatientEpisodeTestWorld.SupervisorUserId,
            actorIsSupervisor: true);

        Assert.Equal(InpEpisodeOperationStatus.BusinessRuleRejected, hasil.Status);
        Assert.Equal(
            "Sesi koreksi hanya untuk episode yang sudah ditutup.",
            hasil.Message);
    }

    // =========================================================================
    // BE-RWI-031 — Episode bayi
    // =========================================================================

    /// <remarks>
    /// Kriteria 1 dan 2: bayi mendapat episode dan kunjungan sendiri, ditempatkan di boks
    /// bertanda <c>IsForNewborn</c>, dan census menampilkan <b>dua baris</b> — ibu dan bayinya.
    /// </remarks>
    [Fact]
    public async Task Kriteria1Dan2_BayiPunyaEpisodeSendiriDanCensusMenampilkanDuaBaris()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync("Melati 3");
        var tempatTidurIbu = await world.AddBedAsync(room, "3A");
        var boksBayi = await world.AddBedAsync(room, "3A-Boks", isForNewborn: true);

        var episodeIbu = await world.OpenAndPlaceAsync(tempatTidurIbu);

        var bayi = await world.AddPatientAsync("Bayi Ny. Rina", Gender.Male);

        var admisiBayi = await world.EpisodeService.OpenAdmissionAsync(
            new OpenAdmissionRequest
            {
                PatientId = bayi.Id,
                ServiceUnitId = world.ServiceUnit.Id,
                PatientClassId = world.PatientClass.Id,
                DoctorId = world.Doctor.Id,
                MotherEpisodeId = episodeIbu.Id
            },
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.Success, admisiBayi.Status);

        var penempatanBayi = await world.BedOccupancyService.PlacePatientAsync(
            new PlacePatientRequest
            {
                EpisodeId = admisiBayi.Episode!.Id,
                BedId = boksBayi.Id
            },
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.Success, penempatanBayi.Status);

        // Bayi punya kunjungan sendiri, bukan menumpang kunjungan ibunya.
        Assert.NotEqual(episodeIbu.EncounterId, admisiBayi.Episode!.EncounterId);

        var census = await world.CensusQueryService.GetCensusAsync(new CensusQuery());

        Assert.Equal(2, census.TotalData);

        var barisIbu = census.Items.Single(x => x.EpisodeId == episodeIbu.Id);
        var barisBayi = census.Items.Single(x => x.EpisodeId == admisiBayi.Episode.Id);

        Assert.Null(barisIbu.MotherEpisodeId);
        Assert.False(barisIbu.IsNewbornBed);

        Assert.Equal(episodeIbu.Id, barisBayi.MotherEpisodeId);
        Assert.Equal(episodeIbu.EpisodeNumber, barisBayi.MotherEpisodeNumber);
        Assert.Equal("Ibu Rina", barisBayi.MotherPatientName);
        Assert.True(barisBayi.IsNewbornBed);
    }

    /// <remarks>
    /// <b>Kriteria 3 adalah yang paling mudah dikerjakan terbalik</b> menjadi "menutup ibu
    /// menutup bayinya". Bayi sering pulang pada hari yang berbeda dari ibunya, dan episode
    /// yang tertutup paksa akan menghapus hari rawat bayi dari tagihan.
    /// </remarks>
    [Fact]
    public async Task Kriteria3_MenutupEpisodeIbuTidakMenutupEpisodeBayiDanTidakMelepasBoksnya()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync("Melati 3");
        var tempatTidurIbu = await world.AddBedAsync(room, "3A");
        var boksBayi = await world.AddBedAsync(room, "3A-Boks", isForNewborn: true);

        var episodeIbu = await world.BuildClosableEpisodeAsync(tempatTidurIbu);

        var bayi = await world.AddPatientAsync("Bayi Ny. Rina", Gender.Male);

        var admisiBayi = await world.EpisodeService.OpenAdmissionAsync(
            new OpenAdmissionRequest
            {
                PatientId = bayi.Id,
                ServiceUnitId = world.ServiceUnit.Id,
                PatientClassId = world.PatientClass.Id,
                DoctorId = world.Doctor.Id,
                MotherEpisodeId = episodeIbu.Id
            },
            InpatientEpisodeTestWorld.ActorUserId);

        await world.BedOccupancyService.PlacePatientAsync(
            new PlacePatientRequest
            {
                EpisodeId = admisiBayi.Episode!.Id,
                BedId = boksBayi.Id
            },
            InpatientEpisodeTestWorld.ActorUserId);

        var tutupIbu = await world.DischargeService.CloseEpisodeAsync(
            episodeIbu.Id,
            null,
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.Success, tutupIbu.Status);

        var episodeBayi = await world.DbContext.Set<InpEpisode>()
            .AsNoTracking()
            .FirstAsync(x => x.Id == admisiBayi.Episode.Id);

        Assert.Equal(InpEpisodeStatus.Admitted, episodeBayi.EpisodeStatus);
        Assert.Null(episodeBayi.ClosedAt);

        var boksSesudah = await world.DbContext.Set<MstBed>()
            .AsNoTracking()
            .FirstAsync(x => x.Id == boksBayi.Id);

        Assert.Equal(BedStatus.Occupied, boksSesudah.BedStatus);

        var penempatanBayi = await world.DbContext.Set<InpBedPlacement>()
            .AsNoTracking()
            .FirstAsync(x => x.EpisodeId == admisiBayi.Episode.Id);

        Assert.Null(penempatanBayi.EndDateTime);

        // Bayinya tetap muncul pada census setelah ibunya pulang.
        var census = await world.CensusQueryService.GetCensusAsync(new CensusQuery());

        var satuSatunya = Assert.Single(census.Items);
        Assert.Equal(admisiBayi.Episode.Id, satuSatunya.EpisodeId);
    }

    [Fact]
    public async Task Kriteria4_SistemDapatMenjawabBayiSiapaYangAdaDiBoksKamarTertentu()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync("Melati 3");
        var tempatTidurIbu = await world.AddBedAsync(room, "3A");
        var boksBayi = await world.AddBedAsync(room, "3A-Boks", isForNewborn: true);

        var episodeIbu = await world.OpenAndPlaceAsync(tempatTidurIbu);

        var bayi = await world.AddPatientAsync("Bayi Ny. Rina", Gender.Male);

        var admisiBayi = await world.EpisodeService.OpenAdmissionAsync(
            new OpenAdmissionRequest
            {
                PatientId = bayi.Id,
                ServiceUnitId = world.ServiceUnit.Id,
                PatientClassId = world.PatientClass.Id,
                DoctorId = world.Doctor.Id,
                MotherEpisodeId = episodeIbu.Id
            },
            InpatientEpisodeTestWorld.ActorUserId);

        await world.BedOccupancyService.PlacePatientAsync(
            new PlacePatientRequest
            {
                EpisodeId = admisiBayi.Episode!.Id,
                BedId = boksBayi.Id
            },
            InpatientEpisodeTestWorld.ActorUserId);

        var penghuniBoks = await world.EpisodeService.GetNewbornOccupantsAsync(room.Id);

        var butir = Assert.Single(penghuniBoks);

        Assert.Equal("Bayi Ny. Rina", butir.PatientName);
        Assert.Equal(boksBayi.Id, butir.BedId);
        Assert.Equal(episodeIbu.EpisodeNumber, butir.MotherEpisodeNumber);
        Assert.Equal("Ibu Rina", butir.MotherPatientName);
    }

    /// <remarks>
    /// Kriteria 5, dan roadmap mewajibkan percobaan menunjuk episode pasien yang sama dibuktikan
    /// ditolak. Tanpa aturan itu, seorang pasien dapat tercatat sebagai ibu dari dirinya sendiri
    /// lewat dua episode berbeda.
    /// </remarks>
    [Fact]
    public async Task Kriteria5_RujukanEpisodeIbuBolehKosongTetapiTidakBolehMilikPasienYangSama()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        // Boleh kosong.
        var tanpaIbu = await world.OpenDraftEpisodeAsync();
        Assert.Null(tanpaIbu.MotherEpisodeId);

        // Tidak boleh milik pasien yang sama.
        var pasienYangSama = await world.EpisodeService.OpenAdmissionAsync(
            new OpenAdmissionRequest
            {
                PatientId = world.Patient.Id,
                ServiceUnitId = world.ServiceUnit.Id,
                PatientClassId = world.PatientClass.Id,
                DoctorId = world.Doctor.Id,
                MotherEpisodeId = tanpaIbu.Id
            },
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.BusinessRuleRejected, pasienYangSama.Status);
        Assert.Equal("Episode ibu harus milik pasien yang berbeda.", pasienYangSama.Message);

        // Tidak boleh menunjuk episode yang tidak ada.
        var bayi = await world.AddPatientAsync("Bayi Ny. Rina", Gender.Male);

        var tidakDitemukan = await world.EpisodeService.OpenAdmissionAsync(
            new OpenAdmissionRequest
            {
                PatientId = bayi.Id,
                ServiceUnitId = world.ServiceUnit.Id,
                PatientClassId = world.PatientClass.Id,
                DoctorId = world.Doctor.Id,
                MotherEpisodeId = Guid.NewGuid()
            },
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.BusinessRuleRejected, tidakDitemukan.Status);
        Assert.Equal("Episode ibu tidak ditemukan atau sudah selesai.", tidakDitemukan.Message);
    }

    [Fact]
    public async Task Kriteria5_EpisodeTidakDapatMenunjukDirinyaSendiriSebagaiEpisodeIbu()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var episode = await world.OpenDraftEpisodeAsync();

        var hasil = await world.EpisodeService.UpdateAdmissionAsync(
            episode.Id,
            new UpdateAdmissionRequest
            {
                ServiceUnitId = world.ServiceUnit.Id,
                PatientClassId = world.PatientClass.Id,
                MotherEpisodeId = episode.Id
            },
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.Invalid, hasil.Status);
        Assert.Equal(
            "Episode tidak dapat menunjuk dirinya sendiri sebagai episode ibu.",
            hasil.Message);
    }

    // =========================================================================
    // Pembantu
    // =========================================================================

    private static async Task<InpEpisode> BuatEpisodeTertutupAsync(
        InpatientEpisodeTestWorld world)
    {
        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, $"3-{Guid.NewGuid().ToString("N")[..3]}");

        var episode = await world.BuildClosableEpisodeAsync(bed);

        var tutup = await world.DischargeService.CloseEpisodeAsync(
            episode.Id,
            null,
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.Success, tutup.Status);

        return episode;
    }
}
