using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Enums;

namespace QuilvianSystemBackend.Tests.InPatientManagement;

/// <summary>
/// <c>BE-RWI-009</c> — daftar dan detail episode dapat dibaca dan disaring.
/// </summary>
/// <remarks>
/// Yang dijaga di sini adalah lima acceptance criteria task tersebut, ditambah satu batas
/// arsitektur: lokasi pasien selalu dibaca dari <c>InpBedPlacement</c>, tidak pernah dari
/// kolom pada episode.
///
/// <para>
/// Kriteria 5 — penolakan 403 tanpa hak akses — <b>tidak</b> dibuktikan di sini. Hak akses
/// repository ini bekerja lewat <c>AccessPermissionFilter</c> yang baru berjalan ketika
/// permintaan HTTP sungguhan masuk. Yang dapat dijaga tanpa aplikasi berjalan adalah bahwa
/// setiap endpoint memang diberi atributnya, dan itu dijaga
/// <c>InpatientEpisodeControllerContractTests</c>.
/// </para>
/// </remarks>
public sealed class InpEpisodeReadTests
{
    [Fact]
    public async Task Kriteria1_DaftarDapatDisaringUnitLayananStatusRentangTanggalDanNamaPasien()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var budi = await world.AddPatientAsync("Tn. Budi", Gender.Male);

        await world.OpenDraftEpisodeAsync();
        await world.OpenDraftEpisodeAsync(budi.Id);

        var byName = await world.EpisodeService.GetEpisodeListAsync(
            new InpatientEpisodeListQuery { Search = "budi" });

        Assert.Single(byName.Items);
        Assert.Equal("Tn. Budi", byName.Items[0].PatientName);

        var byServiceUnit = await world.EpisodeService.GetEpisodeListAsync(
            new InpatientEpisodeListQuery { ServiceUnitId = world.ServiceUnit.Id });

        Assert.Equal(2, byServiceUnit.TotalData);

        var byOtherServiceUnit = await world.EpisodeService.GetEpisodeListAsync(
            new InpatientEpisodeListQuery { ServiceUnitId = Guid.NewGuid() });

        Assert.Empty(byOtherServiceUnit.Items);

        var byStatus = await world.EpisodeService.GetEpisodeListAsync(
            new InpatientEpisodeListQuery { EpisodeStatus = (int)InpEpisodeStatus.Admitted });

        Assert.Empty(byStatus.Items);

        var byDateRange = await world.EpisodeService.GetEpisodeListAsync(
            new InpatientEpisodeListQuery
            {
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow
            });

        Assert.Equal(2, byDateRange.TotalData);

        var byPastDateRange = await world.EpisodeService.GetEpisodeListAsync(
            new InpatientEpisodeListQuery
            {
                StartDate = DateTime.UtcNow.AddDays(-10),
                EndDate = DateTime.UtcNow.AddDays(-5)
            });

        Assert.Empty(byPastDateRange.Items);
    }

    /// <remarks>
    /// Ini batas arsitektur yang paling mudah dilanggar. Menyimpan "lokasi terakhir" sebagai
    /// kolom pada episode membuat query lebih murah, dan sejak saat itu lokasi pada layar
    /// dapat berbeda dari catatan penempatan tanpa ada yang menyadarinya.
    /// </remarks>
    [Fact]
    public async Task Kriteria2_DetailMenampilkanDpjpAktifPerawatAktifDanLokasiDariCatatanPenempatan()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync("Melati 3");
        var bed = await world.AddBedAsync(room, "3B");
        var perawat = await world.AddEmployeeAsync("Ns. Sari");

        var episode = await world.OpenAndPlaceAsync(bed);

        await world.EpisodeService.AssignNurseAsync(
            episode.Id,
            new AssignNurseRequest { EmployeeId = perawat.Id },
            InpatientEpisodeTestWorld.SupervisorUserId,
            actorIsWardHeadOrSupervisor: true);

        var detail = await world.EpisodeService.GetEpisodeDetailAsync(episode.Id);

        Assert.NotNull(detail);
        Assert.NotNull(detail!.ActiveDoctor);
        Assert.Equal(world.Doctor.Id, detail.ActiveDoctor!.DoctorId);

        Assert.NotNull(detail.ActiveNurse);
        Assert.Equal(perawat.Id, detail.ActiveNurse!.EmployeeId);

        Assert.NotNull(detail.CurrentLocation);
        Assert.Equal(bed.Id, detail.CurrentLocation!.BedId);
        Assert.Equal("Melati 3", detail.CurrentLocation.RoomName);

        // Bukti bahwa lokasinya benar-benar dibaca dari catatan penempatan: menutup baris
        // penempatan membuat lokasinya hilang, tanpa satu pun kolom pada episode disentuh.
        var placement = await world.DbContext.Set<InpBedPlacement>()
            .FirstAsync(x => x.EpisodeId == episode.Id && x.EndDateTime == null);

        placement.EndDateTime = DateTime.UtcNow;
        placement.EndReason = InpBedPlacementEndReason.PatientDeparted;
        await world.DbContext.SaveChangesAsync();

        var afterRelease = await world.EpisodeService.GetEpisodeDetailAsync(episode.Id);

        Assert.NotNull(afterRelease);
        Assert.Null(afterRelease!.CurrentLocation);
    }

    [Fact]
    public async Task Kriteria3_RingkasanMenghitungJumlahPerStatus()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");
        var budi = await world.AddPatientAsync("Tn. Budi", Gender.Male);

        await world.OpenDraftEpisodeAsync();
        await world.OpenAndPlaceAsync(bed, budi.Id);

        var summary = await world.EpisodeService.GetEpisodeSummaryAsync(
            new InpatientEpisodeListQuery());

        Assert.Equal(2, summary.TotalAll);
        Assert.Equal(5, summary.ByStatus.Count);

        Assert.Equal(
            1,
            summary.ByStatus.Single(x => x.EpisodeStatus == (int)InpEpisodeStatus.Draft).Total);

        Assert.Equal(
            1,
            summary.ByStatus.Single(x => x.EpisodeStatus == (int)InpEpisodeStatus.Admitted).Total);

        // Status yang tidak punya satu pun episode tetap muncul dengan nilai nol, supaya layar
        // tidak perlu menebak status mana yang hilang dari jawaban.
        Assert.Equal(
            0,
            summary.ByStatus.Single(x => x.EpisodeStatus == (int)InpEpisodeStatus.Closed).Total);
    }

    /// <remarks>
    /// Kewajiban privasi, bukan preferensi. Daftar episode dibaca setiap peran yang punya
    /// <c>InpatientEpisode : Read</c> — termasuk kasir. Bila catatan admisi dan keterangan
    /// kebutuhan isolasi ikut pada daftar, seluruh peran itu ikut membacanya.
    /// </remarks>
    [Fact]
    public async Task Kriteria4_KolomSensitifTidakIkutPadaDaftarTetapiAdaPadaDetail()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

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

        var list = await world.EpisodeService.GetEpisodeListAsync(new InpatientEpisodeListQuery());

        var item = Assert.Single(list.Items);

        // Nilai benar/salahnya boleh tampil; alasan klinisnya tidak.
        Assert.True(item.RequiresIsolation);

        var listProperties = typeof(InpatientEpisodeListItemResponse)
            .GetProperties()
            .Select(x => x.Name)
            .ToList();

        Assert.DoesNotContain("Notes", listProperties);
        Assert.DoesNotContain("IsolationNote", listProperties);

        var detail = await world.EpisodeService.GetEpisodeDetailAsync(episode.Id);

        Assert.NotNull(detail);
        Assert.Equal("Kecurigaan tuberkulosis aktif.", detail!.IsolationNote);
    }

    /// <remarks>
    /// Konsekuensi <c>RWI-DEC-030</c>: kedaluwarsa dihitung saat dibaca. Bila endpoint daftar
    /// membaca tabel tanpa menjalankan perhitungan itu, layar menampilkan admisi yang
    /// sesungguhnya sudah gugur, dan petugas mencoba melanjutkannya lalu ditolak tanpa
    /// mengerti kenapa.
    /// </remarks>
    [Fact]
    public async Task DaftarMenjalankanPerhitunganKedaluwarsaSebelumMembaca()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync(draftEpisodeExpiryHours: 24);

        var episode = await world.OpenDraftEpisodeAsync();

        var tracked = await world.DbContext.Set<InpEpisode>().FirstAsync(x => x.Id == episode.Id);
        tracked.CreateDateTime = DateTime.UtcNow.AddHours(-25);
        tracked.UpdateDateTime = null;
        await world.DbContext.SaveChangesAsync();

        var list = await world.EpisodeService.GetEpisodeListAsync(new InpatientEpisodeListQuery());

        var item = Assert.Single(list.Items);

        Assert.Equal((int)InpEpisodeStatus.Cancelled, item.EpisodeStatus);
    }

    [Fact]
    public async Task MetadataPenyaringHanyaMenawarkanUnitDanKelasYangBerlakuUntukRawatInap()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var metadata = await world.EpisodeService.GetFilterMetadataAsync();

        var serviceUnit = Assert.Single(metadata.ServiceUnitOptions);
        Assert.Equal(world.ServiceUnit.Id.ToString(), serviceUnit.Value);

        var patientClass = Assert.Single(metadata.PatientClassOptions);
        Assert.Equal(world.PatientClass.Id.ToString(), patientClass.Value);

        Assert.Equal(5, metadata.EpisodeStatusOptions.Count);
        Assert.Contains("asc", metadata.SortDirections);
        Assert.Contains("desc", metadata.SortDirections);
    }

    [Fact]
    public async Task DaftarBertingkatMemakaiBentukPaginationYangSama()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        for (var i = 0; i < 3; i++)
        {
            var patient = await world.AddPatientAsync($"Pasien {i}", Gender.Female);
            await world.OpenDraftEpisodeAsync(patient.Id);
        }

        var page = await world.EpisodeService.GetEpisodeListAsync(
            new InpatientEpisodeListQuery { PageNumber = 1, PageSize = 2 });

        Assert.Equal(1, page.PageNumber);
        Assert.Equal(2, page.PageSize);
        Assert.Equal(3, page.TotalData);
        Assert.Equal(2, page.TotalPage);
        Assert.Equal(2, page.Items.Count);
    }
}
