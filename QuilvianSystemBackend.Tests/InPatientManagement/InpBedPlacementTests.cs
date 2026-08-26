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
/// <c>BE-RWI-011</c> — pasien punya lokasi, dan tempat tidur ganda mustahil terjadi;
/// <c>BE-RWI-012</c> — satu pasien tidak pernah tercatat dirawat di dua tempat.
/// </summary>
/// <remarks>
/// <b>Batas pembuktian yang wajib dibaca sebelum mempercayai test ini.</b> Provider InMemory
/// tidak menegakkan unique index parsial dan tidak punya transaksi sungguhan, sehingga dua
/// pertahanan terpenting <c>INV-INP-02</c> — penguncian baris <c>MstBed</c> dan
/// <c>IX_InpBedPlacement_BedId_Active</c> — tidak dapat diuji di sini.
///
/// <para>
/// Yang dibuktikan test ini adalah lapis pemeriksaan di dalam kode: bahwa penempatan kedua
/// pada tempat tidur yang sama ditolak, bahwa keadaan tempat tidur diperiksa ulang saat
/// penempatan, dan bahwa kegagalan penyimpanan tidak menyisakan baris apa pun. Pembuktian
/// bahwa dua transaksi yang benar-benar bersamaan menghasilkan tepat satu baris penempatan
/// aktif harus dijalankan terhadap PostgreSQL sungguhan, dan dicatat pada laporan task.
/// </para>
/// </remarks>
public sealed class InpBedPlacementTests
{
    [Fact]
    public async Task Kriteria1_SetelahPenempatanSistemMenjawabSiapaMenempatiDanSejakJamBerapa()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync("Melati 3");
        var bed = await world.AddBedAsync(room, "3B");

        var sebelum = DateTime.UtcNow;
        var episode = await world.OpenAndPlaceAsync(bed);

        var placements = await world.BedOccupancyService.GetPlacementsByEpisodeAsync(episode.Id);

        var placement = Assert.Single(placements);

        Assert.True(placement.IsCurrent);
        Assert.Equal(bed.Id, placement.BedId);
        Assert.Equal("Melati 3", placement.RoomName);
        Assert.Equal(1, placement.SequenceNumber);

        // RWI-AC-147 — untuk jalur datang langsung dan poliklinik, waktu mulai penempatan
        // adalah waktu penempatan dibuat, bukan waktu yang menunggu kejadian lain.
        Assert.InRange(placement.StartDateTime, sebelum, DateTime.UtcNow);

        var tersimpan = await world.DbContext.Set<InpEpisode>()
            .AsNoTracking()
            .FirstAsync(x => x.Id == episode.Id);

        Assert.Equal(InpEpisodeStatus.Admitted, tersimpan.EpisodeStatus);
        Assert.NotNull(tersimpan.AdmittedAt);

        var bedSesudah = await world.DbContext.Set<MstBed>()
            .AsNoTracking()
            .FirstAsync(x => x.Id == bed.Id);

        Assert.Equal(BedStatus.Occupied, bedSesudah.BedStatus);
    }

    /// <remarks>
    /// Lapis pemeriksaan di dalam kode. Lapis sesungguhnya — penguncian baris ditambah unique
    /// index parsial — hanya dapat diuji terhadap PostgreSQL.
    /// </remarks>
    [Fact]
    public async Task Kriteria2_PenempatanKeduaPadaTempatTidurYangSamaDitolak409DanHanyaSatuBarisAktifTersimpan()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");

        var budi = await world.AddPatientAsync("Tn. Budi", Gender.Male);

        await world.OpenAndPlaceAsync(bed);

        var episodeKedua = await world.OpenDraftEpisodeAsync(budi.Id);

        var kedua = await world.BedOccupancyService.PlacePatientAsync(
            new PlacePatientRequest { EpisodeId = episodeKedua.Id, BedId = bed.Id },
            InpatientEpisodeTestWorld.SupervisorUserId);

        Assert.Equal(InpEpisodeOperationStatus.Conflict, kedua.Status);
        Assert.Contains("sudah ditempati pasien lain", kedua.Message);

        var aktif = await world.DbContext.Set<InpBedPlacement>()
            .AsNoTracking()
            .CountAsync(x => x.BedId == bed.Id && x.EndDateTime == null);

        Assert.Equal(1, aktif);
    }

    /// <remarks>
    /// Kriteria 3 dan 5 sekaligus. Penyimpanan digagalkan di tengah jalan, dan yang dibuktikan
    /// adalah bahwa tidak ada satu pun baris tersisa: tidak ada penempatan, episode tetap
    /// <c>Draft</c>, dan isian admisinya utuh.
    /// </remarks>
    [Fact]
    public async Task Kriteria3_KegagalanPenyimpananTidakMenyisakanPenempatanDanEpisodeTetapDraft()
    {
        var databaseName = $"inpatient-placement-fail-{Guid.NewGuid():N}";

        var world = await InpatientEpisodeTestWorld.CreateAsync(
            dbContext: IsolatedInpatientDbContextFactory.Create(databaseName));

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");
        var episode = await world.OpenDraftEpisodeAsync();

        await using var failing = IsolatedInpatientDbContextFactory.CreateFailingSave(databaseName);

        var failingWorld = InpatientEpisodeTestWorld.Build(
            failing,
            world.Patient,
            world.Doctor,
            world.ServiceUnit,
            world.PatientClass);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            failingWorld.BedOccupancyService.PlacePatientAsync(
                new PlacePatientRequest { EpisodeId = episode.Id, BedId = bed.Id },
                InpatientEpisodeTestWorld.ActorUserId));

        await using var pembaca = IsolatedInpatientDbContextFactory.Create(databaseName);

        Assert.Empty(await pembaca.Set<InpBedPlacement>().ToListAsync());

        var tersimpan = await pembaca.Set<InpEpisode>().FirstAsync(x => x.Id == episode.Id);

        Assert.Equal(InpEpisodeStatus.Draft, tersimpan.EpisodeStatus);
        Assert.Null(tersimpan.AdmittedAt);

        // Isian admisi tetap utuh: unit layanan, kelas perawatan, dan nomor episodenya tidak
        // berubah sedikit pun oleh penolakan tadi.
        Assert.Equal(world.ServiceUnit.Id, tersimpan.ServiceUnitId);
        Assert.Equal(world.PatientClass.Id, tersimpan.PatientClassId);
        Assert.False(string.IsNullOrWhiteSpace(tersimpan.EpisodeNumber));
    }

    /// <remarks>
    /// Pemesanan sudah dibuat saat tempat tidur masih layak, lalu keadaannya berubah sebelum
    /// pasien datang. Bila keadaan tidak diperiksa ulang saat penempatan, pasien ditempatkan
    /// di tempat tidur yang sedang diperbaiki.
    /// </remarks>
    [Fact]
    public async Task Kriteria4_KeadaanTempatTidurDiperiksaUlangSaatPenempatanBukanHanyaSaatPemesanan()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");
        var episode = await world.OpenDraftEpisodeAsync();

        var reserve = await world.BedOccupancyService.ReserveBedAsync(
            new ReserveBedRequest { EpisodeId = episode.Id, BedId = bed.Id },
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.Success, reserve.Status);

        var tracked = await world.DbContext.Set<MstBed>().FirstAsync(x => x.Id == bed.Id);
        tracked.BedStatus = BedStatus.Maintenance;
        await world.DbContext.SaveChangesAsync();

        var place = await world.BedOccupancyService.PlacePatientAsync(
            new PlacePatientRequest { EpisodeId = episode.Id, BedId = bed.Id },
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.BusinessRuleRejected, place.Status);
        Assert.Contains("Perbaikan", place.Message);
    }

    /// <remarks>
    /// Pesan penolakannya menyebut kode tempat tidur dan menyatakan isian admisi tetap
    /// tersimpan. Tanpa kalimat kedua itu, petugas cenderung mengulang admisi dari awal —
    /// dan admisi kedua itulah yang kemudian menjadi episode ganda.
    /// </remarks>
    [Fact]
    public async Task Kriteria5_PenolakanTidakMenghapusIsianAdmisiDanPesannyaMengatakannya()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");

        await world.OpenAndPlaceAsync(bed);

        var budi = await world.AddPatientAsync("Tn. Budi", Gender.Male);
        var episode = await world.OpenDraftEpisodeAsync(budi.Id);

        var hasil = await world.BedOccupancyService.PlacePatientAsync(
            new PlacePatientRequest { EpisodeId = episode.Id, BedId = bed.Id },
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.Conflict, hasil.Status);
        Assert.Contains(bed.BedCode, hasil.Message);
        Assert.Contains("isian admisi Anda tetap tersimpan", hasil.Message);

        var tersimpan = await world.DbContext.Set<InpEpisode>()
            .AsNoTracking()
            .FirstAsync(x => x.Id == episode.Id);

        Assert.Equal(InpEpisodeStatus.Draft, tersimpan.EpisodeStatus);
        Assert.Equal(world.ServiceUnit.Id, tersimpan.ServiceUnitId);
    }

    [Fact]
    public async Task Kriteria6_PemesananMilikEpisodeIniYangMasihBerlakuDipakaiBukanDitolak()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");
        var episode = await world.OpenDraftEpisodeAsync();

        var reserve = await world.BedOccupancyService.ReserveBedAsync(
            new ReserveBedRequest { EpisodeId = episode.Id, BedId = bed.Id },
            InpatientEpisodeTestWorld.ActorUserId);

        var place = await world.BedOccupancyService.PlacePatientAsync(
            new PlacePatientRequest { EpisodeId = episode.Id, BedId = bed.Id },
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.Success, place.Status);

        var reservation = await world.DbContext.Set<InpBedReservation>()
            .AsNoTracking()
            .FirstAsync(x => x.Id == reserve.ReservationId!.Value);

        Assert.Equal(InpBedReservationStatus.Consumed, reservation.ReservationStatus);
        Assert.NotNull(reservation.ReleasedAt);
    }

    /// <remarks>
    /// <c>RWI-RULE-015</c>. Pemesanan Sdri. Wati pukul 09:15 gugur pukul 11:15; Ny. Sari baru
    /// sampai kamar pukul 11:40. Karena tempat tidurnya masih kosong, penempatan tetap
    /// berhasil dan tidak ada peringatan apa pun.
    /// </remarks>
    [Fact]
    public async Task PemesananYangSudahGugurTidakMenghalangiPenempatan()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");
        var episode = await world.OpenDraftEpisodeAsync();

        var reserve = await world.BedOccupancyService.ReserveBedAsync(
            new ReserveBedRequest { EpisodeId = episode.Id, BedId = bed.Id },
            InpatientEpisodeTestWorld.ActorUserId);

        var reservation = await world.DbContext.Set<InpBedReservation>()
            .FirstAsync(x => x.Id == reserve.ReservationId!.Value);

        reservation.ExpiresAt = DateTime.UtcNow.AddMinutes(-25);
        await world.DbContext.SaveChangesAsync();

        var place = await world.BedOccupancyService.PlacePatientAsync(
            new PlacePatientRequest { EpisodeId = episode.Id, BedId = bed.Id },
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.Success, place.Status);
    }

    [Fact]
    public async Task MenempatkanEpisodeYangSudahDitempatkanDitolak409()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var pertama = await world.AddBedAsync(room, "3A");
        var kedua = await world.AddBedAsync(room, "3B");

        var episode = await world.OpenAndPlaceAsync(pertama);

        var hasil = await world.BedOccupancyService.PlacePatientAsync(
            new PlacePatientRequest { EpisodeId = episode.Id, BedId = kedua.Id },
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.Conflict, hasil.Status);
        Assert.Equal("Pasien sudah ditempatkan sebelumnya.", hasil.Message);
    }

    // =========================================================================
    // BE-RWI-012 — INV-INP-10, satu pasien satu episode yang hadir
    // =========================================================================

    [Fact]
    public async Task InvInp10Kriteria1_MenempatkanPasienYangSudahDirawatDitolak409DenganNomorEpisodeDanLokasi()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync("Melati 3");
        var pertama = await world.AddBedAsync(room, "3B");
        var kedua = await world.AddBedAsync(room, "3C");

        var episodeLama = await world.OpenAndPlaceAsync(pertama);

        var episodeBaru = await world.OpenDraftEpisodeAsync();

        var hasil = await world.BedOccupancyService.PlacePatientAsync(
            new PlacePatientRequest { EpisodeId = episodeBaru.Id, BedId = kedua.Id },
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.Conflict, hasil.Status);
        Assert.Contains(episodeLama.EpisodeNumber, hasil.Message);
        Assert.Contains("Melati 3", hasil.Message);
        Assert.Contains("3B", hasil.Message);
        Assert.Contains("pakai perpindahan, bukan admisi baru", hasil.Message);
    }

    /// <remarks>
    /// Kebalikan kriteria 1, dan sama pentingnya. Admisi <c>Draft</c> ganda <b>bukan</b>
    /// penolakan: pasien belum tentu ada di ruangan, dan menolaknya akan menyandera pekerjaan
    /// yang sah. Yang muncul hanya peringatan.
    /// </remarks>
    [Fact]
    public async Task InvInp10Kriteria2_MembukaAdmisiUntukPasienYangPunyaDraftLainTetapBerhasilDenganPeringatan()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        await world.OpenDraftEpisodeAsync();

        var hasil = await world.EpisodeService.OpenAdmissionAsync(
            world.BuildOpenAdmissionRequest(),
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.Success, hasil.Status);
        Assert.Single(hasil.Warnings);
        Assert.Contains("admisi lain yang sedang disiapkan", hasil.Warnings[0]);
    }

    /// <remarks>
    /// Kriteria 3 dan 4 sengaja ditulis berpasangan dalam satu berkas, karena batas di antara
    /// keduanya adalah inti aturan ini: yang pertama mencegah data ganda, yang kedua mencegah
    /// pasien tertahan oleh urusan administrasi. Menguji salah satunya saja menghasilkan rasa
    /// aman yang palsu.
    /// </remarks>
    [Fact]
    public async Task InvInp10Kriteria3Dan4_BatasnyaKepergianFisikBukanPenutupanEpisode()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync("Melati 3");
        var lama = await world.AddBedAsync(room, "3B");
        var baru = await world.AddBedAsync(room, "3C");

        var episodeLama = await world.OpenAndPlaceAsync(lama);

        var decide = await world.DischargeService.DecideDischargeAsync(
            episodeLama.Id,
            new DecideDischargeRequest { DischargeType = (int)InpDischargeType.DoctorApproved },
            InpatientEpisodeTestWorld.ActorUserId,
            world.Doctor.Id);

        Assert.Equal(InpEpisodeOperationStatus.Success, decide.Status);

        // Kriteria 3 — kepergian BELUM dicatat, pasien masih di ruangan.
        var episodeBaru = await world.OpenDraftEpisodeAsync();

        var ditolak = await world.BedOccupancyService.PlacePatientAsync(
            new PlacePatientRequest { EpisodeId = episodeBaru.Id, BedId = baru.Id },
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.Conflict, ditolak.Status);
        Assert.Contains(episodeLama.EpisodeNumber, ditolak.Message);

        // Kriteria 4 — kepergian SUDAH dicatat. Endpoint pencatatannya milik BE-RWI-027, jadi
        // kolomnya disetel langsung di sini. Jalur endpoint-nya diuji ulang pada task itu.
        var tracked = await world.DbContext.Set<InpEpisode>()
            .FirstAsync(x => x.Id == episodeLama.Id);

        tracked.PhysicallyLeftAt = DateTime.UtcNow;
        tracked.PhysicallyLeftByUserId = InpatientEpisodeTestWorld.ActorUserId;
        await world.DbContext.SaveChangesAsync();

        var diterima = await world.BedOccupancyService.PlacePatientAsync(
            new PlacePatientRequest { EpisodeId = episodeBaru.Id, BedId = baru.Id },
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.Success, diterima.Status);
    }

    /// <remarks>
    /// Pembatalan admisi mengembalikan salinan status tempat tidur. Celah ini terbuka sejak
    /// <c>BE-RWI-008</c> dan ditutup <c>BE-RWI-011</c>; tanpa penutupan itu, tempat tidur
    /// pasien yang admisinya batal tetap terlihat terisi pada layar master.
    /// </remarks>
    [Fact]
    public async Task PembatalanEpisodeMengembalikanSalinanStatusTempatTidur()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");

        var episode = await world.OpenAndPlaceAsync(bed);

        var cancel = await world.EpisodeService.CancelAdmissionAsync(
            episode.Id,
            new CancelAdmissionRequest { Reason = "Pasien pulang paksa sebelum perawatan dimulai." },
            InpatientEpisodeTestWorld.SupervisorUserId,
            actorIsSupervisorOrWardHead: true);

        Assert.Equal(InpEpisodeOperationStatus.Success, cancel.Status);

        var bedSesudah = await world.DbContext.Set<MstBed>()
            .AsNoTracking()
            .FirstAsync(x => x.Id == bed.Id);

        Assert.Equal(BedStatus.Available, bedSesudah.BedStatus);
    }
}
