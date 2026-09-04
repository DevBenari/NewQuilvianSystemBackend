using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;

namespace QuilvianSystemBackend.Tests.InPatientManagement;

/// <summary>
/// <c>BE-RWI-015</c> — kapasitas isolasi terjaga dari dua arah, tanpa menahan pencatatan
/// klinis.
/// </summary>
/// <remarks>
/// <b>Kriteria 4 adalah yang paling mudah dikerjakan terbalik.</b> Menahan pencatatan klinis
/// demi menjaga aturan penempatan adalah urutan yang salah: fakta klinis dicatat lebih dulu,
/// lalu sistem menunjukkan bahwa penempatannya perlu dibetulkan. Daftar pantau adalah
/// pengganti penolakan, bukan pelengkapnya.
/// </remarks>
public sealed class InpIsolationCapacityTests
{
    /// <remarks>
    /// Kriteria 1 dan 2 wajib memeriksa <b>isi pesannya</b>, bukan hanya kode 422. Keduanya
    /// berkode sama tetapi artinya berlawanan: yang pertama berarti pasien perlu tempat tidur
    /// yang lebih ketat, yang kedua berarti pasien sedang memakai kapasitas yang dibutuhkan
    /// orang lain.
    /// </remarks>
    [Fact]
    public async Task Kriteria1Dan2_DuaPenolakanBerkodeSamaDenganArtiBerlawanan()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var kamarBiasa = await world.AddRoomAsync("Melati 3");
        var tempatTidurBiasa = await world.AddBedAsync(kamarBiasa, "3A");

        var kamarIsolasi = await world.AddRoomAsync("Isolasi 1");
        var tempatTidurIsolasi = await world.AddBedAsync(kamarIsolasi, "ISO-1", isIsolationBed: true);

        var butuhIsolasi = await world.OpenDraftEpisodeAsync();

        await world.EpisodeService.SetIsolationRequirementAsync(
            butuhIsolasi.Id,
            new SetIsolationRequirementRequest
            {
                RequiresIsolation = true,
                IsolationNote = "Kecurigaan tuberkulosis aktif."
            },
            InpatientEpisodeTestWorld.ActorUserId,
            actorDoctorId: null);

        // Kriteria 1 — butuh isolasi, tempat tidurnya bukan isolasi.
        var kriteria1 = await world.BedOccupancyService.PlacePatientAsync(
            new PlacePatientRequest { EpisodeId = butuhIsolasi.Id, BedId = tempatTidurBiasa.Id },
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.BusinessRuleRejected, kriteria1.Status);
        Assert.Equal(
            "Pasien ini membutuhkan isolasi, sehingga hanya dapat ditempatkan pada tempat " +
            "tidur isolasi.",
            kriteria1.Message);
        Assert.Contains(kriteria1.Failures, x => x.RuleNumber == 7);

        // Kriteria 2 — tidak butuh isolasi, tempat tidurnya isolasi.
        var tidakButuh = await world.AddPatientAsync("Ny. Sari", QuilvianSystemBackend.Enums.Gender.Female);
        var episodeBiasa = await world.OpenDraftEpisodeAsync(tidakButuh.Id);

        var kriteria2 = await world.BedOccupancyService.PlacePatientAsync(
            new PlacePatientRequest { EpisodeId = episodeBiasa.Id, BedId = tempatTidurIsolasi.Id },
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.BusinessRuleRejected, kriteria2.Status);
        Assert.Equal(
            "Tempat tidur isolasi hanya untuk pasien yang membutuhkan isolasi.",
            kriteria2.Message);
        Assert.Contains(kriteria2.Failures, x => x.RuleNumber == 8);

        Assert.NotEqual(kriteria1.Message, kriteria2.Message);
    }

    [Fact]
    public async Task Kriteria3_PasienButuhIsolasiKeTempatTidurIsolasiBerhasil()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var kamarIsolasi = await world.AddRoomAsync("Isolasi 1");
        var tempatTidurIsolasi = await world.AddBedAsync(kamarIsolasi, "ISO-1", isIsolationBed: true);

        var episode = await world.OpenDraftEpisodeAsync();

        await world.EpisodeService.SetIsolationRequirementAsync(
            episode.Id,
            new SetIsolationRequirementRequest
            {
                RequiresIsolation = true,
                IsolationNote = "Kecurigaan tuberkulosis aktif."
            },
            InpatientEpisodeTestWorld.ActorUserId,
            actorDoctorId: null);

        var hasil = await world.BedOccupancyService.PlacePatientAsync(
            new PlacePatientRequest { EpisodeId = episode.Id, BedId = tempatTidurIsolasi.Id },
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.Success, hasil.Status);
    }

    /// <remarks>
    /// Kriteria 4 dan 5 berpasangan: pencatatan klinis diterima walaupun penempatannya menjadi
    /// tidak sesuai, episodenya muncul pada daftar pantau, lalu hilang dari daftar begitu
    /// penempatannya dibetulkan.
    /// </remarks>
    [Fact]
    public async Task Kriteria4Dan5_PencatatanKlinisTidakDitahanDanDaftarPantauMengikutiPembetulannya()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var kamarBiasa = await world.AddRoomAsync("Melati 3");
        var tempatTidurBiasa = await world.AddBedAsync(kamarBiasa, "3A");

        var kamarIsolasi = await world.AddRoomAsync("Isolasi 1");
        var tempatTidurIsolasi = await world.AddBedAsync(kamarIsolasi, "ISO-1", isIsolationBed: true);

        var episode = await world.OpenAndPlaceAsync(tempatTidurBiasa);

        var kosongDiAwal = await world.CensusQueryService.GetIsolationMismatchAsync(
            new IsolationMismatchQuery());

        Assert.Empty(kosongDiAwal.Items);

        // Kriteria 4 — kondisi klinis berubah di tengah perawatan. Pencatatannya DITERIMA,
        // bukan ditolak, walaupun pasien sedang berada di tempat tidur biasa.
        var pencatatan = await world.EpisodeService.SetIsolationRequirementAsync(
            episode.Id,
            new SetIsolationRequirementRequest
            {
                RequiresIsolation = true,
                IsolationNote = "Hasil laboratorium keluar, pasien perlu isolasi."
            },
            InpatientEpisodeTestWorld.ActorUserId,
            actorDoctorId: world.Doctor.Id);

        Assert.Equal(InpEpisodeOperationStatus.Success, pencatatan.Status);

        var setelahPencatatan = await world.CensusQueryService.GetIsolationMismatchAsync(
            new IsolationMismatchQuery());

        var butir = Assert.Single(setelahPencatatan.Items);
        Assert.Equal(episode.Id, butir.EpisodeId);
        Assert.Equal("NeedsIsolationBed", butir.MismatchKind);

        // Kriteria 5 — setelah dipindahkan ke tempat tidur isolasi, episodenya hilang dari
        // daftar pantau.
        var pindah = await world.BedOccupancyService.TransferAsync(
            new TransferPatientRequest
            {
                EpisodeId = episode.Id,
                TargetBedId = tempatTidurIsolasi.Id,
                TransferReason = "Memenuhi kebutuhan isolasi pasien."
            },
            InpatientEpisodeTestWorld.SupervisorUserId,
            actorDoctorId: null);

        Assert.Equal(InpEpisodeOperationStatus.Success, pindah.Status);

        var setelahDipindahkan = await world.CensusQueryService.GetIsolationMismatchAsync(
            new IsolationMismatchQuery());

        Assert.Empty(setelahDipindahkan.Items);
    }

    /// <remarks>
    /// Arah sebaliknya, dan sama pentingnya: kapasitas isolasi yang terpakai pasien yang sudah
    /// tidak membutuhkannya adalah tempat tidur yang tidak tersedia bagi pasien yang
    /// membutuhkannya.
    /// </remarks>
    [Fact]
    public async Task Kriteria6_MematikanKebutuhanIsolasiSaatDiTempatTidurIsolasiMemunculkanDaftarPantau()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var kamarIsolasi = await world.AddRoomAsync("Isolasi 1");
        var tempatTidurIsolasi = await world.AddBedAsync(kamarIsolasi, "ISO-1", isIsolationBed: true);

        var episode = await world.OpenDraftEpisodeAsync();

        await world.EpisodeService.SetIsolationRequirementAsync(
            episode.Id,
            new SetIsolationRequirementRequest
            {
                RequiresIsolation = true,
                IsolationNote = "Kecurigaan tuberkulosis aktif."
            },
            InpatientEpisodeTestWorld.ActorUserId,
            actorDoctorId: null);

        await world.BedOccupancyService.PlacePatientAsync(
            new PlacePatientRequest { EpisodeId = episode.Id, BedId = tempatTidurIsolasi.Id },
            InpatientEpisodeTestWorld.ActorUserId);

        var sebelum = await world.CensusQueryService.GetIsolationMismatchAsync(
            new IsolationMismatchQuery());

        Assert.Empty(sebelum.Items);

        var pencabutan = await world.EpisodeService.SetIsolationRequirementAsync(
            episode.Id,
            new SetIsolationRequirementRequest { RequiresIsolation = false },
            InpatientEpisodeTestWorld.ActorUserId,
            actorDoctorId: world.Doctor.Id);

        Assert.Equal(InpEpisodeOperationStatus.Success, pencabutan.Status);

        var sesudah = await world.CensusQueryService.GetIsolationMismatchAsync(
            new IsolationMismatchQuery());

        var butir = Assert.Single(sesudah.Items);
        Assert.Equal("OccupiesIsolationBed", butir.MismatchKind);
        Assert.Contains("tidak membutuhkan isolasi", butir.MismatchMessage);
    }

    [Fact]
    public async Task Kriteria7_DaftarPantauYangKosongMengembalikanDaftarKosongBukanGalat()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var hasil = await world.CensusQueryService.GetIsolationMismatchAsync(
            new IsolationMismatchQuery());

        Assert.NotNull(hasil);
        Assert.Empty(hasil.Items);
        Assert.Equal(0, hasil.TotalData);
        Assert.Equal(0, hasil.TotalPage);
    }
}
