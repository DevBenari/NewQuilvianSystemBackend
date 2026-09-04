using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;

namespace QuilvianSystemBackend.Tests.InPatientManagement;

/// <summary>
/// <c>BE-RWI-014</c> — kebutuhan isolasi tercatat pada episode dengan pemiliknya jelas.
/// </summary>
/// <remarks>
/// <b>Inti task ini adalah <c>GUARD-INP-04</c>.</b> Mesin hak akses menjawab
/// <c>SetIsolation</c> dengan "boleh" untuk petugas admisi <b>dan</b> untuk dokter mana pun.
/// Yang membedakan keduanya adalah status episode dan siapa DPJP aktifnya. Bila penjaga itu
/// dilupakan, dokter jaga mana pun dapat mengubah keputusan pengendalian infeksi milik DPJP
/// lain, dan tidak ada satu pun kolom yang dapat membedakannya dari keputusan yang sah.
///
/// <para>
/// Kriteria 6 — peran di luar admisi dan dokter ditolak mesin hak akses sebelum service
/// dijalankan — <b>tidak</b> dibuktikan di sini, karena <c>AccessPermissionFilter</c> baru
/// berjalan pada permintaan HTTP sungguhan. Yang dijaga tanpa aplikasi berjalan adalah bahwa
/// endpoint-nya memang memakai butir <c>SetIsolation</c>, dan itu dijaga
/// <c>InpatientEpisodeControllerContractTests</c>.
/// </para>
/// </remarks>
public sealed class InpIsolationRequirementTests
{
    [Fact]
    public async Task Kriteria1_PetugasAdmisiMenyalakanSelagiDraftMenghasilkanCatatanAwal()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var episode = await world.OpenDraftEpisodeAsync();

        var hasil = await world.EpisodeService.SetIsolationRequirementAsync(
            episode.Id,
            new SetIsolationRequirementRequest
            {
                RequiresIsolation = true,
                IsolationNote = "Surat pengantar dokter menyebut kecurigaan tuberkulosis."
            },
            InpatientEpisodeTestWorld.ActorUserId,
            actorDoctorId: null);

        Assert.Equal(InpEpisodeOperationStatus.Success, hasil.Status);

        var tersimpan = await world.DbContext.Set<InpEpisode>()
            .AsNoTracking()
            .FirstAsync(x => x.Id == episode.Id);

        Assert.True(tersimpan.RequiresIsolation);
        Assert.Equal(InpIsolationSource.AdmissionRecord, tersimpan.IsolationSource);
        Assert.Equal(InpatientEpisodeTestWorld.ActorUserId, tersimpan.IsolationSetByUserId);
        Assert.Null(tersimpan.IsolationSetByDoctorId);
        Assert.NotNull(tersimpan.IsolationSetAt);
    }

    [Fact]
    public async Task Kriteria2_DpjpAktifMengubahSetelahAdmittedMenghasilkanKeputusanKlinis()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");

        var episode = await world.OpenAndPlaceAsync(bed);

        var hasil = await world.EpisodeService.SetIsolationRequirementAsync(
            episode.Id,
            new SetIsolationRequirementRequest
            {
                RequiresIsolation = true,
                IsolationNote = "Hasil pemeriksaan menunjukkan perlunya isolasi."
            },
            InpatientEpisodeTestWorld.ActorUserId,
            actorDoctorId: world.Doctor.Id);

        Assert.Equal(InpEpisodeOperationStatus.Success, hasil.Status);

        var tersimpan = await world.DbContext.Set<InpEpisode>()
            .AsNoTracking()
            .FirstAsync(x => x.Id == episode.Id);

        Assert.Equal(InpIsolationSource.ClinicalDecision, tersimpan.IsolationSource);
        Assert.Equal(world.Doctor.Id, tersimpan.IsolationSetByDoctorId);
    }

    /// <remarks>
    /// Kriteria 3 dan 4 sengaja berpasangan dalam satu test, supaya terlihat bahwa yang
    /// membedakan diterima dan ditolak adalah <b>status episode</b> beserta hubungan dokter
    /// dengan pasien itu, bukan sekadar peran penggunanya.
    /// </remarks>
    [Fact]
    public async Task Kriteria3Dan4_SetelahAdmittedHanyaDpjpAktifYangBoleh()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");
        var dokterJaga = await world.AddDoctorAsync("dr. Rina");

        var episode = await world.OpenAndPlaceAsync(bed);

        // Kriteria 3 — dokter yang bukan DPJP aktif.
        var dokterLain = await world.EpisodeService.SetIsolationRequirementAsync(
            episode.Id,
            new SetIsolationRequirementRequest
            {
                RequiresIsolation = true,
                IsolationNote = "Menurut saya perlu isolasi."
            },
            InpatientEpisodeTestWorld.SupervisorUserId,
            actorDoctorId: dokterJaga.Id);

        Assert.Equal(InpEpisodeOperationStatus.Forbidden, dokterLain.Status);
        Assert.Contains("hanya DPJP episode ini", dokterLain.Message);

        // Kriteria 4 — petugas admisi setelah episode berjalan. Wewenangnya berhenti di
        // Draft; ia tidak berlaku selamanya.
        var petugasAdmisi = await world.EpisodeService.SetIsolationRequirementAsync(
            episode.Id,
            new SetIsolationRequirementRequest
            {
                RequiresIsolation = true,
                IsolationNote = "Katanya perlu isolasi."
            },
            InpatientEpisodeTestWorld.ActorUserId,
            actorDoctorId: null);

        Assert.Equal(InpEpisodeOperationStatus.Forbidden, petugasAdmisi.Status);

        var tersimpan = await world.DbContext.Set<InpEpisode>()
            .AsNoTracking()
            .FirstAsync(x => x.Id == episode.Id);

        Assert.False(tersimpan.RequiresIsolation);
    }

    [Fact]
    public async Task Kriteria5_MenyalakanTanpaKeteranganDitolak400()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var episode = await world.OpenDraftEpisodeAsync();

        var hasil = await world.EpisodeService.SetIsolationRequirementAsync(
            episode.Id,
            new SetIsolationRequirementRequest { RequiresIsolation = true, IsolationNote = "   " },
            InpatientEpisodeTestWorld.ActorUserId,
            actorDoctorId: null);

        Assert.Equal(InpEpisodeOperationStatus.Invalid, hasil.Status);
        Assert.Equal("Tuliskan alasan atau keterangan kebutuhan isolasi.", hasil.Message);
    }

    /// <remarks>
    /// Mencabut kebutuhan isolasi tidak mewajibkan keterangan. Yang wajib diberi alasan adalah
    /// menyalakannya, karena itulah yang membatasi ke mana pasien boleh ditempatkan.
    /// </remarks>
    [Fact]
    public async Task MencabutKebutuhanIsolasiTidakMewajibkanKeterangan()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var episode = await world.OpenDraftEpisodeAsync();

        await world.EpisodeService.SetIsolationRequirementAsync(
            episode.Id,
            new SetIsolationRequirementRequest
            {
                RequiresIsolation = true,
                IsolationNote = "Kecurigaan tuberkulosis."
            },
            InpatientEpisodeTestWorld.ActorUserId,
            actorDoctorId: null);

        var hasil = await world.EpisodeService.SetIsolationRequirementAsync(
            episode.Id,
            new SetIsolationRequirementRequest { RequiresIsolation = false },
            InpatientEpisodeTestWorld.ActorUserId,
            actorDoctorId: null);

        Assert.Equal(InpEpisodeOperationStatus.Success, hasil.Status);

        var tersimpan = await world.DbContext.Set<InpEpisode>()
            .AsNoTracking()
            .FirstAsync(x => x.Id == episode.Id);

        Assert.False(tersimpan.RequiresIsolation);
        Assert.Null(tersimpan.IsolationNote);
    }

    [Fact]
    public async Task EpisodeYangSudahDibatalkanTidakDapatDiubahKebutuhanIsolasinya()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var episode = await world.OpenDraftEpisodeAsync();

        await world.EpisodeService.CancelAdmissionAsync(
            episode.Id,
            new CancelAdmissionRequest { Reason = "Pasien membatalkan rencana rawat inap." },
            InpatientEpisodeTestWorld.ActorUserId,
            actorIsSupervisorOrWardHead: false);

        var hasil = await world.EpisodeService.SetIsolationRequirementAsync(
            episode.Id,
            new SetIsolationRequirementRequest
            {
                RequiresIsolation = true,
                IsolationNote = "Kecurigaan tuberkulosis."
            },
            InpatientEpisodeTestWorld.ActorUserId,
            actorDoctorId: null);

        Assert.Equal(InpEpisodeOperationStatus.Conflict, hasil.Status);
    }
}
