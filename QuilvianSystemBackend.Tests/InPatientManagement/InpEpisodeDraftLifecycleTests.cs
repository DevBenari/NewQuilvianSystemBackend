using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;

namespace QuilvianSystemBackend.Tests.InPatientManagement;

/// <summary>
/// Keenam acceptance criteria <c>BE-RWI-008</c>: isian admisi dibetulkan, admisi dibatalkan,
/// dan admisi yang ditinggalkan gugur sendiri saat dibaca.
/// </summary>
public sealed class InpEpisodeDraftLifecycleTests
{
    // Kriteria 1 — mengubah isian episode yang bukan Draft ditolak.
    [Fact]
    public async Task Kriteria1_MengubahIsianEpisodeYangBukanDraftDitolak()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();
        await using var db = world.DbContext;

        var episodeId = await OpenAdmissionAsync(world);

        // Episode dinaikkan ke Admitted lewat basis data karena penempatan pasien baru lahir
        // pada BE-RWI-011. Yang diuji di sini adalah penolakannya, bukan cara menaikkannya.
        await SetStatusDirectlyAsync(db, episodeId, InpEpisodeStatus.Admitted);

        var hasil = await world.EpisodeService.UpdateAdmissionAsync(
            episodeId,
            world.BuildUpdateAdmissionRequest(),
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.Conflict, hasil.Status);
        Assert.Equal(
            "Isian admisi hanya dapat diubah selama pasien belum ditempatkan.",
            hasil.Message);
    }

    [Fact]
    public async Task MengubahIsianEpisodeDraftBerhasil()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();
        await using var db = world.DbContext;

        var episodeId = await OpenAdmissionAsync(world);

        var kelasBaru = new MstPatientClass
        {
            Id = Guid.NewGuid(),
            PatientClassCode = "KELAS-2",
            PatientClassName = "Kelas 2",
            IsForInpatient = true,
            IsActive = true,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = InpatientEpisodeTestWorld.ActorUserId
        };

        db.Set<MstPatientClass>().Add(kelasBaru);
        await db.SaveChangesAsync();

        var hasil = await world.EpisodeService.UpdateAdmissionAsync(
            episodeId,
            world.BuildUpdateAdmissionRequest(kelasBaru.Id),
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.Success, hasil.Status);

        var episode = await ReadEpisodeAsync(db, episodeId);

        Assert.Equal(kelasBaru.Id, episode.PatientClassId);
        Assert.Equal(InpatientEpisodeTestWorld.ActorUserId, episode.UpdateBy);

        // Membetulkan isian bukan perpindahan status, jadi ia tidak menambah baris riwayat.
        Assert.Equal(1, await db.Set<InpStatusHistory>().AsNoTracking().CountAsync());
    }

    // Kriteria 2 — pembatalan berhasil dan melepas pemesanan serta penempatan dalam satu
    // tindakan utuh.
    [Fact]
    public async Task Kriteria2_PembatalanDraftBerhasilDanMelepasPemesananSertaPenempatan()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();
        await using var db = world.DbContext;

        var episodeId = await OpenAdmissionAsync(world);
        var bedId = Guid.NewGuid();

        db.Set<InpBedReservation>().Add(new InpBedReservation
        {
            Id = Guid.NewGuid(),
            EpisodeId = episodeId,
            BedId = bedId,
            ReservedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(2),
            ReservationStatus = InpBedReservationStatus.Active,
            ReservedByUserId = InpatientEpisodeTestWorld.ActorUserId,
            IsActive = true,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = InpatientEpisodeTestWorld.ActorUserId
        });

        db.Set<InpBedPlacement>().Add(new InpBedPlacement
        {
            Id = Guid.NewGuid(),
            EpisodeId = episodeId,
            BedId = bedId,
            RoomId = Guid.NewGuid(),
            ServiceUnitId = world.ServiceUnit.Id,
            PatientClassId = world.PatientClass.Id,
            SequenceNumber = 1,
            StartDateTime = DateTime.UtcNow,
            EndDateTime = null,
            PlacedByUserId = InpatientEpisodeTestWorld.ActorUserId,
            IsActive = true,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = InpatientEpisodeTestWorld.ActorUserId
        });

        await db.SaveChangesAsync();

        var hasil = await world.EpisodeService.CancelAdmissionAsync(
            episodeId,
            new CancelAdmissionRequest { Reason = "Pasien membatalkan rencana rawat inap." },
            InpatientEpisodeTestWorld.ActorUserId,
            actorIsSupervisorOrWardHead: false);

        Assert.Equal(InpEpisodeOperationStatus.Success, hasil.Status);

        var episode = await ReadEpisodeAsync(db, episodeId);

        Assert.Equal(InpEpisodeStatus.Cancelled, episode.EpisodeStatus);
        Assert.Equal("Pasien membatalkan rencana rawat inap.", episode.CancelReason);
        Assert.True(episode.IsCancel);

        // Pelepasan tempat tidur adalah BAGIAN dari pembatalan, bukan langkah terpisah yang
        // dikerjakan petugas sesudahnya.
        var pemesanan = await db.Set<InpBedReservation>().AsNoTracking().SingleAsync();
        Assert.Equal(InpBedReservationStatus.Cancelled, pemesanan.ReservationStatus);
        Assert.NotNull(pemesanan.ReleasedAt);

        var penempatan = await db.Set<InpBedPlacement>().AsNoTracking().SingleAsync();
        Assert.NotNull(penempatan.EndDateTime);
        Assert.Equal(InpBedPlacementEndReason.AdmissionCancelled, penempatan.EndReason);
    }

    [Fact]
    public async Task PembatalanTanpaAlasanDitolak()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();
        await using var db = world.DbContext;

        var episodeId = await OpenAdmissionAsync(world);

        var hasil = await world.EpisodeService.CancelAdmissionAsync(
            episodeId,
            new CancelAdmissionRequest { Reason = "   " },
            InpatientEpisodeTestWorld.ActorUserId,
            actorIsSupervisorOrWardHead: false);

        Assert.Equal(InpEpisodeOperationStatus.Invalid, hasil.Status);
    }

    [Fact]
    public async Task PembatalanDenganAlasanYangHanyaTandaBacaDitolak()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();
        await using var db = world.DbContext;

        var episodeId = await OpenAdmissionAsync(world);

        // RWI-AC-008: "..." bukan alasan. Ia hanya membuat kolom alasan terlihat terisi.
        var hasil = await world.EpisodeService.CancelAdmissionAsync(
            episodeId,
            new CancelAdmissionRequest { Reason = "... --- ???" },
            InpatientEpisodeTestWorld.ActorUserId,
            actorIsSupervisorOrWardHead: false);

        Assert.Equal(InpEpisodeOperationStatus.Invalid, hasil.Status);

        Assert.Equal(
            InpEpisodeStatus.Draft,
            (await ReadEpisodeAsync(db, episodeId)).EpisodeStatus);
    }

    // Kriteria 3 — pembatalan setelah Admitted hanya oleh supervisor atau kepala ruangan.
    [Fact]
    public async Task Kriteria3_PembatalanEpisodeAdmittedOlehPeranLainDitolak403()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();
        await using var db = world.DbContext;

        var episodeId = await OpenAdmissionAsync(world);
        await SetStatusDirectlyAsync(db, episodeId, InpEpisodeStatus.Admitted);

        var hasil = await world.EpisodeService.CancelAdmissionAsync(
            episodeId,
            new CancelAdmissionRequest { Reason = "Salah pilih pasien saat admisi." },
            InpatientEpisodeTestWorld.ActorUserId,
            actorIsSupervisorOrWardHead: false);

        Assert.Equal(InpEpisodeOperationStatus.Forbidden, hasil.Status);

        Assert.Equal(
            InpEpisodeStatus.Admitted,
            (await ReadEpisodeAsync(db, episodeId)).EpisodeStatus);
    }

    [Fact]
    public async Task Kriteria3_PembatalanEpisodeAdmittedOlehSupervisorBerhasil()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();
        await using var db = world.DbContext;

        var episodeId = await OpenAdmissionAsync(world);
        await SetStatusDirectlyAsync(db, episodeId, InpEpisodeStatus.Admitted);

        var hasil = await world.EpisodeService.CancelAdmissionAsync(
            episodeId,
            new CancelAdmissionRequest { Reason = "Salah pilih pasien saat admisi." },
            InpatientEpisodeTestWorld.SupervisorUserId,
            actorIsSupervisorOrWardHead: true);

        Assert.Equal(InpEpisodeOperationStatus.Success, hasil.Status);

        var episode = await ReadEpisodeAsync(db, episodeId);

        Assert.Equal(InpEpisodeStatus.Cancelled, episode.EpisodeStatus);

        var riwayatTerakhir = await db.Set<InpStatusHistory>()
            .AsNoTracking()
            .OrderByDescending(x => x.SequenceNumber)
            .FirstAsync();

        Assert.Equal(InpEpisodeService.ActionCancelAdmission, riwayatTerakhir.ActionType);
        Assert.Equal(InpEpisodeStatus.Admitted, riwayatTerakhir.FromStatus);
        Assert.Equal(InpEpisodeStatus.Cancelled, riwayatTerakhir.ToStatus);
        Assert.Equal(InpStatusChangeActorType.User, riwayatTerakhir.ActorType);
        Assert.Equal(InpatientEpisodeTestWorld.SupervisorUserId, riwayatTerakhir.ChangedByUserId);
        Assert.Equal(2, riwayatTerakhir.SequenceNumber);
    }

    // Kriteria 4 — Draft yang ditinggalkan terbaca Cancelled, tanpa program penjadwal.
    [Fact]
    public async Task Kriteria4_DuaPembacaanPadaWaktuBerbedaMembuktikanTidakAdaPenjadwal()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync(draftEpisodeExpiryHours: 24);
        await using var db = world.DbContext;

        var episodeId = await OpenAdmissionAsync(world);

        // Pembacaan pertama: episode baru saja dibuka, jadi masih Draft.
        var pembacaanPertama = await world.EpisodeService.GetEpisodeAsync(episodeId);

        Assert.Equal(InpEpisodeStatus.Draft, pembacaanPertama.Episode!.EpisodeStatus);

        // Waktu dimajukan dengan memundurkan jejak sentuhan terakhir. Tidak ada satu pun
        // proses latar belakang yang dijalankan di antara dua pembacaan ini.
        await BackdateEpisodeAsync(db, episodeId, hoursAgo: 48);

        var pembacaanKedua = await world.EpisodeService.GetEpisodeAsync(episodeId);

        Assert.Equal(InpEpisodeStatus.Cancelled, pembacaanKedua.Episode!.EpisodeStatus);

        var episode = await ReadEpisodeAsync(db, episodeId);

        Assert.Equal(InpEpisodeService.SystemExpiryReason, episode.CancelReason);
    }

    [Fact]
    public async Task Kriteria4_KedaluwarsaDitulisSebagaiTindakanSistem()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync(draftEpisodeExpiryHours: 24);
        await using var db = world.DbContext;

        var episodeId = await OpenAdmissionAsync(world);
        await BackdateEpisodeAsync(db, episodeId, hoursAgo: 48);

        await world.EpisodeService.GetEpisodeAsync(episodeId);

        var riwayatTerakhir = await db.Set<InpStatusHistory>()
            .AsNoTracking()
            .OrderByDescending(x => x.SequenceNumber)
            .FirstAsync();

        // Perubahan yang dihitung saat pembacaan ditulis sebagai dilakukan SISTEM, bukan
        // dilakukan orang. Tanpa penanda ini, laporan pengecualian akan menuduh petugas yang
        // kebetulan membuka layar.
        Assert.Equal(InpStatusChangeActorType.System, riwayatTerakhir.ActorType);
        Assert.Null(riwayatTerakhir.ChangedByUserId);
        Assert.Equal(InpEpisodeService.ActionExpireDraft, riwayatTerakhir.ActionType);
    }

    [Fact]
    public async Task Kriteria4_EpisodeYangSudahGugurTidakDapatDiubahLagi()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync(draftEpisodeExpiryHours: 24);
        await using var db = world.DbContext;

        var episodeId = await OpenAdmissionAsync(world);
        await BackdateEpisodeAsync(db, episodeId, hoursAgo: 48);

        var hasil = await world.EpisodeService.UpdateAdmissionAsync(
            episodeId,
            world.BuildUpdateAdmissionRequest(),
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.Conflict, hasil.Status);
    }

    // Kriteria 5 — kunjungan yang ikut lahir bersama episode ikut dibatalkan.
    [Fact]
    public async Task Kriteria5_KunjunganYangLahirBersamaEpisodeIkutDibatalkan()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();
        await using var db = world.DbContext;

        var episodeId = await OpenAdmissionAsync(world, encounterId: null);

        await world.EpisodeService.CancelAdmissionAsync(
            episodeId,
            new CancelAdmissionRequest { Reason = "Pasien batal datang." },
            InpatientEpisodeTestWorld.ActorUserId,
            actorIsSupervisorOrWardHead: false);

        var encounter = await db.Set<TrxPatientEncounter>().AsNoTracking().SingleAsync();

        // Supaya ia tidak muncul sebagai kunjungan rawat inap yang benar-benar terjadi pada
        // laporan kunjungan.
        Assert.Equal(EncounterStatus.Cancelled, encounter.EncounterStatus);
        Assert.NotNull(encounter.CancelledAt);
        Assert.True(encounter.IsCancel);
    }

    [Fact]
    public async Task Kriteria5_KunjunganYangDitunjukPetugasTidakIkutDibatalkan()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();
        await using var db = world.DbContext;

        var encounter = await world.AddEncounterAsync();
        var episodeId = await OpenAdmissionAsync(world, encounter.Id);

        await world.EpisodeService.CancelAdmissionAsync(
            episodeId,
            new CancelAdmissionRequest { Reason = "Pasien batal datang." },
            InpatientEpisodeTestWorld.ActorUserId,
            actorIsSupervisorOrWardHead: false);

        var tersimpan = await db.Set<TrxPatientEncounter>()
            .AsNoTracking()
            .SingleAsync(x => x.Id == encounter.Id);

        // Kunjungan milik alur pendaftaran tidak pernah dibatalkan modul Rawat Inap.
        Assert.Equal(EncounterStatus.Registered, tersimpan.EncounterStatus);
        Assert.False(tersimpan.IsCancel);
    }

    [Fact]
    public async Task Kriteria5_KunjunganIkutDibatalkanSaatEpisodeGugurSendiri()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync(draftEpisodeExpiryHours: 24);
        await using var db = world.DbContext;

        var episodeId = await OpenAdmissionAsync(world, encounterId: null);
        await BackdateEpisodeAsync(db, episodeId, hoursAgo: 48);

        await world.EpisodeService.GetEpisodeAsync(episodeId);

        var encounter = await db.Set<TrxPatientEncounter>().AsNoTracking().SingleAsync();

        Assert.Equal(EncounterStatus.Cancelled, encounter.EncounterStatus);
    }

    // Kriteria 6 — batas jamnya dapat diubah admin dan berlaku pada pembacaan berikutnya.
    [Fact]
    public async Task Kriteria6_BatasJamYangDiubahAdminBerlakuPadaPembacaanBerikutnya()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync(draftEpisodeExpiryHours: 72);
        await using var db = world.DbContext;

        var episodeId = await OpenAdmissionAsync(world);
        await BackdateEpisodeAsync(db, episodeId, hoursAgo: 48);

        // Batas 72 jam: umur 48 jam belum melewatinya, jadi episode masih Draft.
        var sebelum = await world.EpisodeService.GetEpisodeAsync(episodeId);

        Assert.Equal(InpEpisodeStatus.Draft, sebelum.Episode!.EpisodeStatus);

        var setting = await db.Set<MstInpatientSetting>().SingleAsync();
        setting.DraftEpisodeExpiryHours = 24;
        setting.UpdateDateTime = DateTime.UtcNow;
        await db.SaveChangesAsync();

        // Pembacaan berikutnya memakai angka baru, tanpa aplikasi dinyalakan ulang.
        var sesudah = await world.EpisodeService.GetEpisodeAsync(episodeId);

        Assert.Equal(InpEpisodeStatus.Cancelled, sesudah.Episode!.EpisodeStatus);
    }

    // Batas status akhir.
    [Fact]
    public async Task EpisodeYangSudahDibatalkanTidakDapatDibatalkanLagi()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();
        await using var db = world.DbContext;

        var episodeId = await OpenAdmissionAsync(world);

        await world.EpisodeService.CancelAdmissionAsync(
            episodeId,
            new CancelAdmissionRequest { Reason = "Pasien batal datang." },
            InpatientEpisodeTestWorld.ActorUserId,
            actorIsSupervisorOrWardHead: false);

        var kedua = await world.EpisodeService.CancelAdmissionAsync(
            episodeId,
            new CancelAdmissionRequest { Reason = "Dicoba lagi." },
            InpatientEpisodeTestWorld.ActorUserId,
            actorIsSupervisorOrWardHead: false);

        Assert.Equal(InpEpisodeOperationStatus.Conflict, kedua.Status);
        Assert.Equal(
            "Admisi ini sudah dibatalkan dan tidak dapat dilanjutkan.",
            kedua.Message);
    }

    [Fact]
    public async Task EpisodeYangSudahDiputuskanPulangTidakDapatDibatalkan()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();
        await using var db = world.DbContext;

        var episodeId = await OpenAdmissionAsync(world);
        await SetStatusDirectlyAsync(db, episodeId, InpEpisodeStatus.DischargePending);

        var hasil = await world.EpisodeService.CancelAdmissionAsync(
            episodeId,
            new CancelAdmissionRequest { Reason = "Dicoba membatalkan." },
            InpatientEpisodeTestWorld.SupervisorUserId,
            actorIsSupervisorOrWardHead: true);

        Assert.Equal(InpEpisodeOperationStatus.BusinessRuleRejected, hasil.Status);
        Assert.Equal(
            "Episode yang sudah diputuskan pulang tidak dapat dibatalkan.",
            hasil.Message);
    }

    // ---------------------------------------------------------------------
    // Pembantu
    // ---------------------------------------------------------------------

    private static async Task<Guid> OpenAdmissionAsync(
        InpatientEpisodeTestWorld world,
        Guid? encounterId = null)
    {
        var hasil = await world.EpisodeService.OpenAdmissionAsync(
            world.BuildOpenAdmissionRequest(encounterId),
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.Success, hasil.Status);

        return hasil.Episode!.Id;
    }

    private static async Task<InpEpisode> ReadEpisodeAsync(
        QuilvianSystemBackend.Repositories.ApplicationDbContext db,
        Guid episodeId)
    {
        return await db.Set<InpEpisode>()
            .AsNoTracking()
            .SingleAsync(x => x.Id == episodeId);
    }

    /// <summary>
    /// Memundurkan jejak sentuhan terakhir episode, supaya batas kedaluwarsanya terlewati
    /// tanpa test perlu menunggu.
    /// </summary>
    private static async Task BackdateEpisodeAsync(
        QuilvianSystemBackend.Repositories.ApplicationDbContext db,
        Guid episodeId,
        int hoursAgo)
    {
        var episode = await db.Set<InpEpisode>().SingleAsync(x => x.Id == episodeId);
        var backdatedAt = DateTime.UtcNow.AddHours(-hoursAgo);

        episode.CreateDateTime = backdatedAt;
        episode.UpdateDateTime = null;

        await db.SaveChangesAsync();

        db.ChangeTracker.Clear();
    }

    private static async Task SetStatusDirectlyAsync(
        QuilvianSystemBackend.Repositories.ApplicationDbContext db,
        Guid episodeId,
        InpEpisodeStatus status)
    {
        var episode = await db.Set<InpEpisode>().SingleAsync(x => x.Id == episodeId);

        episode.EpisodeStatus = status;

        await db.SaveChangesAsync();

        db.ChangeTracker.Clear();
    }
}
