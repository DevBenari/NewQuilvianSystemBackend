using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using QuilvianSystemBackend.Enums;

namespace QuilvianSystemBackend.Tests.InPatientManagement;

/// <summary>
/// <c>BE-RWI-013</c> — kamar tidak pernah menjadi campur laki-laki dan perempuan.
/// </summary>
/// <remarks>
/// Aturan 4, 5, dan 6 Kelayakan Penempatan beserta dua pengecualian boks bayi.
///
/// <para>
/// <b>Aturan 6 diperiksa dari penghuni yang sedang ada</b>, bukan dari penanda pada
/// <c>MstRoom</c>. Penanda <c>IsForMale</c> dan <c>IsForFemale</c> bernilai benar secara
/// bawaan untuk setiap kamar, sehingga tidak dapat membedakan kamar yang boleh campur. Kolom
/// "boleh campur" ditolak tegas oleh <c>RWI-DEC-066</c>; menambahkannya bukan keputusan
/// pelaksana.
/// </para>
/// </remarks>
public sealed class InpPlacementGenderRuleTests
{
    [Fact]
    public async Task Kriteria1_PasienPerempuanKeTempatTidurHanyaLakiLakiDitolak422()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync("Melati 3");
        var bed = await world.AddBedAsync(room, "3A", isForMale: true, isForFemale: false);

        var episode = await world.OpenDraftEpisodeAsync();

        var hasil = await world.BedOccupancyService.PlacePatientAsync(
            new PlacePatientRequest { EpisodeId = episode.Id, BedId = bed.Id },
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.BusinessRuleRejected, hasil.Status);
        Assert.Equal("Tempat tidur ini hanya untuk pasien laki-laki.", hasil.Message);
        Assert.Contains(hasil.Failures, x => x.RuleNumber == 4);
    }

    [Fact]
    public async Task Kriteria2_KamarYangSudahDihuniJenisKelaminBerbedaDitolak422DanPesannyaMenyebutNamaKamar()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync("Melati 3");
        var pertama = await world.AddBedAsync(room, "3A");
        var kedua = await world.AddBedAsync(room, "3B");

        // Ibu Rina — perempuan — menempati 3A lebih dulu.
        await world.OpenAndPlaceAsync(pertama);

        var budi = await world.AddPatientAsync("Tn. Budi", Gender.Male);
        var episodeBudi = await world.OpenDraftEpisodeAsync(budi.Id);

        var hasil = await world.BedOccupancyService.PlacePatientAsync(
            new PlacePatientRequest { EpisodeId = episodeBudi.Id, BedId = kedua.Id },
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.BusinessRuleRejected, hasil.Status);
        Assert.Contains("Melati 3", hasil.Message);
        Assert.Contains("perempuan", hasil.Message);
        Assert.Contains("laki-laki", hasil.Message);
        Assert.Contains(hasil.Failures, x => x.RuleNumber == 6);
    }

    /// <remarks>
    /// Aturannya menolak <b>pencampuran</b>, bukan menolak kamar berpenghuni. Bila kriteria
    /// ini dikerjakan terbalik, seluruh kamar berisi lebih dari satu tempat tidur menjadi
    /// tidak berguna sejak pasien pertama masuk.
    /// </remarks>
    [Fact]
    public async Task Kriteria3_PasienBerikutnyaBerjenisKelaminSamaDiterima()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync("Melati 3");
        var pertama = await world.AddBedAsync(room, "3A");
        var kedua = await world.AddBedAsync(room, "3B");

        await world.OpenAndPlaceAsync(pertama);

        var sari = await world.AddPatientAsync("Ny. Sari", Gender.Female);
        var episodeSari = await world.OpenDraftEpisodeAsync(sari.Id);

        var hasil = await world.BedOccupancyService.PlacePatientAsync(
            new PlacePatientRequest { EpisodeId = episodeSari.Id, BedId = kedua.Id },
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.Success, hasil.Status);
    }

    [Fact]
    public async Task Kriteria4_JenisKelaminBelumTercatatHanyaBolehKeTempatTidurNetralDiKamarKosong()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync("Anggrek 1");
        var netral = await world.AddBedAsync(room, "1A");
        var hanyaLakiLaki = await world.AddBedAsync(room, "1B", isForMale: true, isForFemale: false);
        var netralKedua = await world.AddBedAsync(room, "1C");

        var kamarKosong = await world.AddRoomAsync("Anggrek 2");
        var netralDiKamarKosong = await world.AddBedAsync(kamarKosong, "2A");

        var tanpaGender = await world.AddPatientAsync("Tn. X", null);
        var episodeSatu = await world.OpenDraftEpisodeAsync(tanpaGender.Id);

        // Gagal pada tempat tidurnya: kamarnya masih kosong, tetapi tempat tidurnya hanya
        // menerima laki-laki.
        var ditolakKarenaTempatTidur = await world.BedOccupancyService.PlacePatientAsync(
            new PlacePatientRequest { EpisodeId = episodeSatu.Id, BedId = hanyaLakiLaki.Id },
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(
            InpEpisodeOperationStatus.BusinessRuleRejected,
            ditolakKarenaTempatTidur.Status);
        Assert.Contains("Jenis kelamin pasien belum tercatat", ditolakKarenaTempatTidur.Message);
        Assert.Contains(ditolakKarenaTempatTidur.Failures, x => x.RuleNumber == 5);

        // Ibu Rina masuk lebih dulu, sehingga kamar Anggrek 1 tidak lagi kosong.
        await world.OpenAndPlaceAsync(netral);

        // Gagal pada kamarnya: tempat tidurnya netral, tetapi kamarnya sudah berpenghuni.
        var ditolakKarenaKamar = await world.BedOccupancyService.PlacePatientAsync(
            new PlacePatientRequest { EpisodeId = episodeSatu.Id, BedId = netralKedua.Id },
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.BusinessRuleRejected, ditolakKarenaKamar.Status);
        Assert.Contains("Jenis kelamin pasien belum tercatat", ditolakKarenaKamar.Message);
        Assert.Contains(ditolakKarenaKamar.Failures, x => x.RuleNumber == 5);

        // Keduanya terpenuhi — diterima.
        var diterima = await world.BedOccupancyService.PlacePatientAsync(
            new PlacePatientRequest { EpisodeId = episodeSatu.Id, BedId = netralDiKamarKosong.Id },
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.Success, diterima.Status);
    }

    /// <remarks>
    /// Kriteria 5 dan 6 sengaja berpasangan dalam satu test, supaya sifat <b>dua arah</b>
    /// pengecualian boks bayi terbukti: bayi laki-laki boleh menempati boks di kamar ibunya,
    /// dan keberadaannya di sana tidak menutup kamar bagi pasien lain.
    /// </remarks>
    [Fact]
    public async Task Kriteria5Dan6_BoksBayiDikecualikanDariKeduaSisiPemeriksaan()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync("Melati 3");
        var tempatTidurIbu = await world.AddBedAsync(room, "3A");
        var boksBayi = await world.AddBedAsync(room, "3A-Boks", isForNewborn: true);
        var tempatTidurKetiga = await world.AddBedAsync(room, "3B");

        // Ibu Rina — perempuan — menempati 3A.
        await world.OpenAndPlaceAsync(tempatTidurIbu);

        // Kriteria 5 — bayi laki-laki ke boks bayi di kamar ibunya berhasil, walaupun kamar
        // itu sedang dihuni pasien perempuan.
        var bayi = await world.AddPatientAsync("Bayi Ny. Rina", Gender.Male);
        var episodeBayi = await world.OpenDraftEpisodeAsync(bayi.Id);

        var penempatanBayi = await world.BedOccupancyService.PlacePatientAsync(
            new PlacePatientRequest { EpisodeId = episodeBayi.Id, BedId = boksBayi.Id },
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.Success, penempatanBayi.Status);

        // Kriteria 6 — penghuni boks bayi tidak dihitung saat memeriksa pencampuran, sehingga
        // pasien perempuan berikutnya tetap diterima di kamar yang sama.
        var sari = await world.AddPatientAsync("Ny. Sari", Gender.Female);
        var episodeSari = await world.OpenDraftEpisodeAsync(sari.Id);

        var penempatanSari = await world.BedOccupancyService.PlacePatientAsync(
            new PlacePatientRequest { EpisodeId = episodeSari.Id, BedId = tempatTidurKetiga.Id },
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.Success, penempatanSari.Status);
    }

    [Fact]
    public async Task Kriteria7_KamarBerisiSatuTempatTidurTidakPernahTersentuhAturanPencampuran()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync("VIP 1", capacity: 1);
        var bed = await world.AddBedAsync(room, "VIP1-A");

        var budi = await world.AddPatientAsync("Tn. Budi", Gender.Male);
        var episode = await world.OpenDraftEpisodeAsync(budi.Id);

        var hasil = await world.BedOccupancyService.PlacePatientAsync(
            new PlacePatientRequest { EpisodeId = episode.Id, BedId = bed.Id },
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.Success, hasil.Status);
        Assert.Empty(hasil.Failures);
    }

    /// <remarks>
    /// <b>Penyaring dan penolak wajib memberi jawaban yang sama.</b> Bila keduanya berbeda,
    /// petugas melihat tempat tidur yang tampak kosong lalu ditolak saat menekan simpan — dan
    /// ia tidak punya cara mengetahui tempat tidur mana yang sebenarnya boleh dipakai.
    /// </remarks>
    [Fact]
    public async Task HasilPencarianDanHasilPenolakanSelaluSama()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync("Melati 3");
        var netral = await world.AddBedAsync(room, "3A");
        var hanyaLakiLaki = await world.AddBedAsync(room, "3B", isForMale: true, isForFemale: false);
        var isolasi = await world.AddBedAsync(room, "3C", isIsolationBed: true);

        // Ibu Rina — perempuan, tidak membutuhkan isolasi.
        var episode = await world.OpenDraftEpisodeAsync();

        var pencarian = await world.BedOccupancyService.SearchAvailableBedsAsync(
            new AvailableBedQuery { EpisodeId = episode.Id });

        var ditawarkan = pencarian.Items.Select(x => x.BedId).ToList();

        Assert.Contains(netral.Id, ditawarkan);
        Assert.DoesNotContain(hanyaLakiLaki.Id, ditawarkan);
        Assert.DoesNotContain(isolasi.Id, ditawarkan);

        // Yang tidak ditawarkan memang benar-benar ditolak, dan yang ditawarkan memang
        // benar-benar diterima.
        var penolakanGender = await world.BedOccupancyService.PlacePatientAsync(
            new PlacePatientRequest { EpisodeId = episode.Id, BedId = hanyaLakiLaki.Id },
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.NotEqual(InpEpisodeOperationStatus.Success, penolakanGender.Status);

        var penolakanIsolasi = await world.BedOccupancyService.PlacePatientAsync(
            new PlacePatientRequest { EpisodeId = episode.Id, BedId = isolasi.Id },
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.NotEqual(InpEpisodeOperationStatus.Success, penolakanIsolasi.Status);

        var diterima = await world.BedOccupancyService.PlacePatientAsync(
            new PlacePatientRequest { EpisodeId = episode.Id, BedId = netral.Id },
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.Success, diterima.Status);
    }
}
