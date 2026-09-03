using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;

namespace QuilvianSystemBackend.Tests.InPatientManagement;

/// <summary>
/// Keenam acceptance criteria <c>BE-RWI-007</c>, ditambah pembuktian bahwa kegagalan di tengah
/// penyimpanan tidak menyisakan episode maupun baris riwayat.
/// </summary>
public sealed class InpEpisodeOpenAdmissionTests
{
    // Kriteria 1 — episode lahir Draft dengan nomor berawalan dari master.
    [Fact]
    public async Task Kriteria1_EpisodeLahirDraftDenganNomorBerawalanDariMaster()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync(episodeNumberPrefix: "RWI");
        await using var db = world.DbContext;

        var hasil = await world.EpisodeService.OpenAdmissionAsync(
            world.BuildOpenAdmissionRequest(),
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.Success, hasil.Status);

        var episode = await db.Set<InpEpisode>().AsNoTracking().SingleAsync();

        Assert.Equal(InpEpisodeStatus.Draft, episode.EpisodeStatus);

        // Awalan dibaca dari MstInpatientSetting.EpisodeNumberPrefix, bukan huruf yang
        // ditanam di kode. Rumah sakit yang memakai awalan lain cukup mengubah satu baris
        // master.
        Assert.StartsWith("RWI-", episode.EpisodeNumber);
    }

    [Fact]
    public async Task Kriteria1_DpjpPertamaDitetapkanSejakDetikPertama()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();
        await using var db = world.DbContext;

        await world.EpisodeService.OpenAdmissionAsync(
            world.BuildOpenAdmissionRequest(),
            InpatientEpisodeTestWorld.ActorUserId);

        var penugasan = await db.Set<InpDoctorAssignment>().AsNoTracking().SingleAsync();

        Assert.Equal(world.Doctor.Id, penugasan.DoctorId);
        Assert.Equal(1, penugasan.SequenceNumber);
        Assert.Null(penugasan.EndDateTime);
    }

    // Kriteria 2 — membuka admisi tanpa DPJP ditolak 400; INV-INP-03 tidak pernah dilanggar.
    [Fact]
    public async Task Kriteria2_TanpaDpjpDitolakDanTidakAdaEpisodeYangLahir()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();
        await using var db = world.DbContext;

        var request = world.BuildOpenAdmissionRequest();
        request.DoctorId = Guid.Empty;

        var hasil = await world.EpisodeService.OpenAdmissionAsync(
            request,
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.Invalid, hasil.Status);
        Assert.Equal("Dokter penanggung jawab belum dipilih.", hasil.Message);

        // INV-INP-03 dijaga dengan tidak melahirkan episodenya sama sekali. Episode tanpa
        // DPJP tidak pernah ada, walau sesaat.
        Assert.Empty(await db.Set<InpEpisode>().AsNoTracking().ToListAsync());
    }

    // Kriteria 3 — kunjungan yang sudah punya episode ditolak 409; INV-INP-04 dijaga.
    [Fact]
    public async Task Kriteria3_KunjunganYangSudahPunyaEpisodeDitolak()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();
        await using var db = world.DbContext;

        var encounter = await world.AddEncounterAsync();

        var pertama = await world.EpisodeService.OpenAdmissionAsync(
            world.BuildOpenAdmissionRequest(encounter.Id),
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.Success, pertama.Status);

        var kedua = await world.EpisodeService.OpenAdmissionAsync(
            world.BuildOpenAdmissionRequest(encounter.Id),
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.Conflict, kedua.Status);
        Assert.Equal("Kunjungan ini sudah punya episode rawat inap.", kedua.Message);

        Assert.Single(await db.Set<InpEpisode>().AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task KunjunganBukanRawatInapDitolakDenganPesanAturanBisnis()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();
        await using var db = world.DbContext;

        var encounter = await world.AddEncounterAsync(EncounterType.Outpatient);

        var hasil = await world.EpisodeService.OpenAdmissionAsync(
            world.BuildOpenAdmissionRequest(encounter.Id),
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.BusinessRuleRejected, hasil.Status);
        Assert.Equal("Kunjungan yang dipilih bukan kunjungan rawat inap.", hasil.Message);
    }

    // Kriteria 4 — pasien datang langsung mendapat kunjungan rawat inap secara otomatis.
    [Fact]
    public async Task Kriteria4_PasienDatangLangsungMendapatKunjunganRawatInapOtomatis()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();
        await using var db = world.DbContext;

        var hasil = await world.EpisodeService.OpenAdmissionAsync(
            world.BuildOpenAdmissionRequest(encounterId: null),
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.Success, hasil.Status);

        var encounter = await db.Set<TrxPatientEncounter>().AsNoTracking().SingleAsync();

        Assert.Equal(EncounterType.Inpatient, encounter.EncounterType);
        Assert.Equal(world.Patient.Id, encounter.PatientId);

        // Kelas pasien TIDAK dipaksa menjadi RAWAT JALAN. Pemaksaan itu hanya berlaku pada
        // kunjungan bertipe Outpatient di PatientEncounterController.
        Assert.Equal(world.PatientClass.Id, encounter.PatientClassId);

        var episode = await db.Set<InpEpisode>().AsNoTracking().SingleAsync();

        Assert.Equal(encounter.Id, episode.EncounterId);
    }

    [Fact]
    public async Task Kriteria4_ProvenanceKunjunganTercatatPadaRiwayatStatusPertama()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();
        await using var db = world.DbContext;

        await world.EpisodeService.OpenAdmissionAsync(
            world.BuildOpenAdmissionRequest(encounterId: null),
            InpatientEpisodeTestWorld.ActorUserId);

        var riwayat = await db.Set<InpStatusHistory>().AsNoTracking().SingleAsync();

        // Penanda inilah yang kelak menentukan apakah kunjungan boleh ikut dibatalkan.
        Assert.Equal(InpEpisodeService.ActionOpenAdmissionWithEncounter, riwayat.ActionType);
    }

    [Fact]
    public async Task KunjunganYangDitunjukPetugasTidakDitandaiSebagaiBuatanAdmisi()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();
        await using var db = world.DbContext;

        var encounter = await world.AddEncounterAsync();

        await world.EpisodeService.OpenAdmissionAsync(
            world.BuildOpenAdmissionRequest(encounter.Id),
            InpatientEpisodeTestWorld.ActorUserId);

        var riwayat = await db.Set<InpStatusHistory>().AsNoTracking().SingleAsync();

        Assert.Equal(InpEpisodeService.ActionOpenAdmission, riwayat.ActionType);
    }

    // Kriteria 5 — setiap perubahan status menulis satu baris riwayat.
    [Fact]
    public async Task Kriteria5_KelahiranEpisodeMeninggalkanTepatSatuBarisRiwayat()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();
        await using var db = world.DbContext;

        await world.EpisodeService.OpenAdmissionAsync(
            world.BuildOpenAdmissionRequest(),
            InpatientEpisodeTestWorld.ActorUserId);

        var riwayat = await db.Set<InpStatusHistory>().AsNoTracking().SingleAsync();

        Assert.Null(riwayat.FromStatus);
        Assert.Equal(InpEpisodeStatus.Draft, riwayat.ToStatus);
        Assert.Equal(1, riwayat.SequenceNumber);
        Assert.Equal(InpStatusChangeActorType.User, riwayat.ActorType);
        Assert.Equal(InpatientEpisodeTestWorld.ActorUserId, riwayat.ChangedByUserId);
    }

    /// <summary>
    /// Pembuktian bahwa episode dan baris riwayatnya berada di dalam SATU penyimpanan.
    /// </summary>
    /// <remarks>
    /// Provider InMemory tidak punya transaksi sungguhan, sehingga yang dibuktikan di sini
    /// adalah sifat yang membuat transaksinya bekerja: tidak ada penyimpanan antara. Ketika
    /// penyimpanan gagal, nol episode dan nol baris riwayat tertinggal — bukan satu tanpa
    /// yang lain. Bahwa PostgreSQL benar-benar mengembalikan perubahan saat transaksi
    /// digagalkan dibuktikan terpisah terhadap database sungguhan.
    /// </remarks>
    [Fact]
    public async Task Kriteria5_KegagalanDiTengahTidakMenyisakanEpisodeMaupunRiwayat()
    {
        var databaseName = $"inpatient-tests-{Guid.NewGuid():N}";

        var world = await InpatientEpisodeTestWorld.CreateAsync(
            dbContext: IsolatedInpatientDbContextFactory.Create(databaseName));

        await using (world.DbContext)
        {
            await using var failing = IsolatedInpatientDbContextFactory
                .CreateFailingSave(databaseName);

            var failingWorld = InpatientEpisodeTestWorld.Build(
                failing,
                world.Patient,
                world.Doctor,
                world.ServiceUnit,
                world.PatientClass);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => failingWorld.EpisodeService.OpenAdmissionAsync(
                    failingWorld.BuildOpenAdmissionRequest(),
                    InpatientEpisodeTestWorld.ActorUserId));

            Assert.Equal(1, failing.SaveAttempts);
        }

        await using var pembaca = IsolatedInpatientDbContextFactory.Create(databaseName);

        Assert.Empty(await pembaca.Set<InpEpisode>().AsNoTracking().ToListAsync());
        Assert.Empty(await pembaca.Set<InpStatusHistory>().AsNoTracking().ToListAsync());
        Assert.Empty(await pembaca.Set<InpDoctorAssignment>().AsNoTracking().ToListAsync());
        Assert.Empty(await pembaca.Set<TrxPatientEncounter>().AsNoTracking().ToListAsync());
    }

    // Kriteria 6 — admisi Draft ganda BERHASIL disertai peringatan, bukan ditolak.
    [Fact]
    public async Task Kriteria6_AdmisiDraftGandaBerhasilDisertaiPeringatan()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();
        await using var db = world.DbContext;

        var pertama = await world.EpisodeService.OpenAdmissionAsync(
            world.BuildOpenAdmissionRequest(),
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.Success, pertama.Status);
        Assert.Empty(pertama.Warnings);

        var kedua = await world.EpisodeService.OpenAdmissionAsync(
            world.BuildOpenAdmissionRequest(),
            InpatientEpisodeTestWorld.ActorUserId);

        // Ini yang membedakannya dari penolakan: petugas yang memutuskan, bukan sistem.
        Assert.Equal(InpEpisodeOperationStatus.Success, kedua.Status);
        Assert.Single(kedua.Warnings);
        Assert.Contains("admisi lain yang sedang disiapkan", kedua.Warnings[0]);

        Assert.Equal(2, await db.Set<InpEpisode>().AsNoTracking().CountAsync());
    }

    // Validation matrix bagian 1 — sisa aturannya.
    [Fact]
    public async Task PasienKosongDitolak()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();
        await using var db = world.DbContext;

        var request = world.BuildOpenAdmissionRequest();
        request.PatientId = Guid.Empty;

        var hasil = await world.EpisodeService.OpenAdmissionAsync(
            request,
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.Invalid, hasil.Status);
        Assert.Equal("Pasien belum dipilih.", hasil.Message);
    }

    [Fact]
    public async Task UnitLayananBukanRawatInapDitolak()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();
        await using var db = world.DbContext;

        var poliklinik = new MstServiceUnit
        {
            Id = Guid.NewGuid(),
            ServiceUnitCode = "POLI-DALAM",
            ServiceUnitName = "Poliklinik Penyakit Dalam",
            ServiceUnitType = QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums
                .ServiceUnitType.Outpatient,
            IsActive = true,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = InpatientEpisodeTestWorld.ActorUserId
        };

        db.Set<MstServiceUnit>().Add(poliklinik);
        await db.SaveChangesAsync();

        var request = world.BuildOpenAdmissionRequest();
        request.ServiceUnitId = poliklinik.Id;

        var hasil = await world.EpisodeService.OpenAdmissionAsync(
            request,
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.BusinessRuleRejected, hasil.Status);
        Assert.Equal("Unit layanan yang dipilih bukan unit rawat inap.", hasil.Message);
    }

    [Fact]
    public async Task KelasPerawatanYangTidakBerlakuUntukRawatInapDitolak()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();
        await using var db = world.DbContext;

        var kelasRawatJalan = new MstPatientClass
        {
            Id = Guid.NewGuid(),
            PatientClassCode = "RAWAT-JALAN",
            PatientClassName = "Rawat Jalan",
            IsForInpatient = false,
            IsActive = true,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = InpatientEpisodeTestWorld.ActorUserId
        };

        db.Set<MstPatientClass>().Add(kelasRawatJalan);
        await db.SaveChangesAsync();

        var request = world.BuildOpenAdmissionRequest();
        request.PatientClassId = kelasRawatJalan.Id;

        var hasil = await world.EpisodeService.OpenAdmissionAsync(
            request,
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.BusinessRuleRejected, hasil.Status);
        Assert.Equal(
            "Kelas perawatan yang dipilih tidak berlaku untuk rawat inap.",
            hasil.Message);
    }
}
