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
/// <c>BE-RWI-010</c> — tempat tidur dapat dicari dan dipesan, dan pemesanan gugur sendiri.
/// </summary>
/// <remarks>
/// <b>Yang tidak dapat dibuktikan provider InMemory.</b> Unique index parsial
/// <c>IX_InpBedReservation_BedId_Active</c> tidak ditegakkan di sini, dan penguncian baris
/// <c>MstBed</c> juga tidak dijalankan karena <c>FOR UPDATE</c> hanya berlaku pada penyedia
/// relasional. Yang dibuktikan test ini adalah bahwa <b>kode</b> menolak keadaan yang memang
/// harus ditolak. Bahwa database sendiri menolak baris kembar adalah pembuktian terpisah
/// terhadap PostgreSQL sungguhan, dan dicatat pada laporan task.
/// </remarks>
public sealed class InpBedReservationTests
{
    [Fact]
    public async Task Kriteria1_TempatTidurYangSudahDipesanTidakMunculPadaPencarian()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");
        var otherBed = await world.AddBedAsync(room, "3B");

        var episode = await world.OpenDraftEpisodeAsync();

        var sebelum = await world.BedOccupancyService.SearchAvailableBedsAsync(
            new AvailableBedQuery());

        Assert.Equal(2, sebelum.TotalData);

        var reserve = await world.BedOccupancyService.ReserveBedAsync(
            new ReserveBedRequest { EpisodeId = episode.Id, BedId = bed.Id },
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.Success, reserve.Status);

        // Tanpa episodeId, pencarian membaca tempat tidur yang tidak dipegang siapa pun.
        var sesudah = await world.BedOccupancyService.SearchAvailableBedsAsync(
            new AvailableBedQuery());

        var tersisa = Assert.Single(sesudah.Items);
        Assert.Equal(otherBed.Id, tersisa.BedId);
    }

    /// <remarks>
    /// <b>Contoh berangka pada roadmap.</b> Pemesanan pukul 09:15 dengan batas 2 jam masih
    /// mengunci pada pembacaan 11:14 dan sudah bebas pada pembacaan 11:16 — <b>tanpa</b>
    /// program penjadwal. Yang menggugurkannya adalah pembacaan kedua itu sendiri.
    /// </remarks>
    [Fact]
    public async Task Kriteria2_DuaPembacaanPadaWaktuBerbedaMembuktikanTidakAdaPenjadwal()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");
        var episode = await world.OpenDraftEpisodeAsync();

        var reserve = await world.BedOccupancyService.ReserveBedAsync(
            new ReserveBedRequest { EpisodeId = episode.Id, BedId = bed.Id },
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.Success, reserve.Status);

        // Pembacaan pertama: masih terkunci.
        var pembacaanPertama = await world.BedOccupancyService.SearchAvailableBedsAsync(
            new AvailableBedQuery());

        Assert.Empty(pembacaanPertama.Items);

        // Waktu gugurnya dimundurkan, meniru berjalannya waktu. Tidak ada satu pun proses
        // yang dijalankan di antara kedua pembacaan.
        var reservation = await world.DbContext.Set<InpBedReservation>()
            .FirstAsync(x => x.Id == reserve.ReservationId!.Value);

        reservation.ExpiresAt = DateTime.UtcNow.AddMinutes(-1);
        await world.DbContext.SaveChangesAsync();

        // Pembacaan kedua: sudah bebas, dan pemesanannya sendiri berubah menjadi Expired.
        var pembacaanKedua = await world.BedOccupancyService.SearchAvailableBedsAsync(
            new AvailableBedQuery());

        Assert.Single(pembacaanKedua.Items);

        var sesudah = await world.DbContext.Set<InpBedReservation>()
            .AsNoTracking()
            .FirstAsync(x => x.Id == reservation.Id);

        Assert.Equal(InpBedReservationStatus.Expired, sesudah.ReservationStatus);
        Assert.NotNull(sesudah.ReleasedAt);
    }

    [Fact]
    public async Task Kriteria3_BatasWaktuYangDiubahAdminDipakaiPemesananBerikutnya()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");
        var episode = await world.OpenDraftEpisodeAsync();

        var setting = await world.DbContext.Set<MstInpatientSetting>().FirstAsync();
        setting.BedReservationMinutes = 30;
        await world.DbContext.SaveChangesAsync();

        var sebelum = DateTime.UtcNow;

        var reserve = await world.BedOccupancyService.ReserveBedAsync(
            new ReserveBedRequest { EpisodeId = episode.Id, BedId = bed.Id },
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.Success, reserve.Status);

        var reservation = await world.DbContext.Set<InpBedReservation>()
            .AsNoTracking()
            .FirstAsync(x => x.Id == reserve.ReservationId!.Value);

        var selisihMenit = (reservation.ExpiresAt - sebelum).TotalMinutes;

        Assert.InRange(selisihMenit, 29, 31);
    }

    [Fact]
    public async Task Kriteria4_MemesanTempatTidurYangSudahDipesanEpisodeLainDitolak409()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");

        var episodeSatu = await world.OpenDraftEpisodeAsync();

        var budi = await world.AddPatientAsync("Tn. Budi", Gender.Male);
        var episodeDua = await world.OpenDraftEpisodeAsync(budi.Id);

        await world.BedOccupancyService.ReserveBedAsync(
            new ReserveBedRequest { EpisodeId = episodeSatu.Id, BedId = bed.Id },
            InpatientEpisodeTestWorld.ActorUserId);

        var kedua = await world.BedOccupancyService.ReserveBedAsync(
            new ReserveBedRequest { EpisodeId = episodeDua.Id, BedId = bed.Id },
            InpatientEpisodeTestWorld.SupervisorUserId);

        Assert.Equal(InpEpisodeOperationStatus.Conflict, kedua.Status);
        Assert.Equal("Tempat tidur ini sudah dipesan untuk pasien lain.", kedua.Message);
        Assert.Contains(kedua.Failures, x => x.RuleNumber == 2 && x.StatusCode == 409);
    }

    /// <remarks>
    /// Pesannya wajib menyebut keadaan tempat tidurnya. Penolakan yang hanya berbunyi "tempat
    /// tidur tidak dapat dipakai" memaksa petugas menebak apakah ia perlu menunggu pembersihan
    /// selesai, atau memilih tempat tidur lain karena yang ini sedang diperbaiki.
    /// </remarks>
    [Fact]
    public async Task Kriteria5_MemesanTempatTidurBerstatusPerbaikanDitolak422DenganPesanYangMenyebutKeadaannya()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A", bedStatus: BedStatus.Maintenance);
        var episode = await world.OpenDraftEpisodeAsync();

        var result = await world.BedOccupancyService.ReserveBedAsync(
            new ReserveBedRequest { EpisodeId = episode.Id, BedId = bed.Id },
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.BusinessRuleRejected, result.Status);
        Assert.Equal(
            "Tempat tidur sedang tidak dapat dipakai. Keadaan saat ini: Perbaikan.",
            result.Message);
        Assert.Contains(result.Failures, x => x.RuleNumber == 1);
    }

    [Fact]
    public async Task Kriteria6_PapanKetersediaanMengelompokkanPerUnitLayananDanKamar()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var melati = await world.AddRoomAsync("Melati 3");
        var anggrek = await world.AddRoomAsync("Anggrek 1");

        var terisi = await world.AddBedAsync(melati, "3A");
        await world.AddBedAsync(melati, "3B");
        await world.AddBedAsync(anggrek, "1A", bedStatus: BedStatus.Maintenance);

        await world.OpenAndPlaceAsync(terisi);

        var board = await world.BedOccupancyService.GetBedBoardAsync(null);

        var unit = Assert.Single(board.ServiceUnits);
        Assert.Equal(world.ServiceUnit.Id, unit.ServiceUnitId);
        Assert.Equal(2, unit.Rooms.Count);

        Assert.Equal(3, board.TotalBed);
        Assert.Equal(1, board.TotalOccupied);
        Assert.Equal(1, board.TotalAvailable);
        Assert.Equal(1, board.TotalUnavailable);

        var kamarMelati = unit.Rooms.Single(x => x.RoomName == "Melati 3");
        var tempatTidurTerisi = kamarMelati.Beds.Single(x => x.BedId == terisi.Id);

        Assert.True(tempatTidurTerisi.IsOccupied);
        Assert.Equal("Ibu Rina", tempatTidurTerisi.PatientName);
    }

    /// <remarks>
    /// Satu episode hanya boleh memegang satu pemesanan. Tanpa aturan ini, satu petugas dapat
    /// mengunci tiga tempat tidur sekaligus "untuk berjaga-jaga", dan ketiganya menghilang
    /// dari pencarian petugas lain selama dua jam.
    /// </remarks>
    [Fact]
    public async Task SatuEpisodeHanyaBolehPunyaSatuPemesananAktif()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var pertama = await world.AddBedAsync(room, "3A");
        var kedua = await world.AddBedAsync(room, "3B");

        var episode = await world.OpenDraftEpisodeAsync();

        await world.BedOccupancyService.ReserveBedAsync(
            new ReserveBedRequest { EpisodeId = episode.Id, BedId = pertama.Id },
            InpatientEpisodeTestWorld.ActorUserId);

        var hasil = await world.BedOccupancyService.ReserveBedAsync(
            new ReserveBedRequest { EpisodeId = episode.Id, BedId = kedua.Id },
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.Conflict, hasil.Status);
        Assert.Equal(
            "Episode ini sudah memesan tempat tidur lain. Batalkan dulu pemesanan sebelumnya.",
            hasil.Message);
    }

    [Fact]
    public async Task PemesananHanyaBolehUntukEpisodeYangMasihDraft()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var terisi = await world.AddBedAsync(room, "3A");
        var lain = await world.AddBedAsync(room, "3B");

        var episode = await world.OpenAndPlaceAsync(terisi);

        var hasil = await world.BedOccupancyService.ReserveBedAsync(
            new ReserveBedRequest { EpisodeId = episode.Id, BedId = lain.Id },
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.BusinessRuleRejected, hasil.Status);
        Assert.Equal(
            "Pemesanan tempat tidur hanya dapat dilakukan sebelum pasien ditempatkan.",
            hasil.Message);
    }

    [Fact]
    public async Task MembatalkanPemesananMengembalikanSalinanStatusTempatTidur()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");
        var episode = await world.OpenDraftEpisodeAsync();

        var reserve = await world.BedOccupancyService.ReserveBedAsync(
            new ReserveBedRequest { EpisodeId = episode.Id, BedId = bed.Id },
            InpatientEpisodeTestWorld.ActorUserId);

        var setelahDipesan = await world.DbContext.Set<MstBed>()
            .AsNoTracking()
            .FirstAsync(x => x.Id == bed.Id);

        Assert.Equal(BedStatus.Reserved, setelahDipesan.BedStatus);

        var cancel = await world.BedOccupancyService.CancelReservationAsync(
            reserve.ReservationId!.Value,
            new CancelReservationRequest { Reason = "Pasien tidak jadi datang." },
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.Success, cancel.Status);

        var setelahDibatalkan = await world.DbContext.Set<MstBed>()
            .AsNoTracking()
            .FirstAsync(x => x.Id == bed.Id);

        Assert.Equal(BedStatus.Available, setelahDibatalkan.BedStatus);
    }

    /// <remarks>
    /// Salinan status tempat tidur yang sedang <c>Maintenance</c> tidak boleh ditimpa modul
    /// ini. Bila ditimpa, tempat tidur yang sedang diperbaiki kembali muncul sebagai siap
    /// pakai — dan pasien berikutnya ditempatkan di sana.
    /// </remarks>
    [Fact]
    public async Task SalinanStatusTidakPernahMenimpaKeadaanYangMerupakanWewenangAdmin()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");
        var episode = await world.OpenDraftEpisodeAsync();

        var reserve = await world.BedOccupancyService.ReserveBedAsync(
            new ReserveBedRequest { EpisodeId = episode.Id, BedId = bed.Id },
            InpatientEpisodeTestWorld.ActorUserId);

        // Admin menutup tempat tidurnya selagi pemesanan masih berjalan.
        var tracked = await world.DbContext.Set<MstBed>().FirstAsync(x => x.Id == bed.Id);
        tracked.BedStatus = BedStatus.Maintenance;
        await world.DbContext.SaveChangesAsync();

        await world.BedOccupancyService.CancelReservationAsync(
            reserve.ReservationId!.Value,
            null,
            InpatientEpisodeTestWorld.ActorUserId);

        var sesudah = await world.DbContext.Set<MstBed>()
            .AsNoTracking()
            .FirstAsync(x => x.Id == bed.Id);

        Assert.Equal(BedStatus.Maintenance, sesudah.BedStatus);
    }
}
