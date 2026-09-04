using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using QuilvianSystemBackend.Enums;

namespace QuilvianSystemBackend.Tests.InPatientManagement;

/// <summary>
/// <c>BE-RWI-016</c> — sistem dapat menjawab siapa dirawat, di mana, dan sudah berapa hari.
/// </summary>
public sealed class InpCensusTests
{
    /// <remarks>
    /// <b>Contoh berangka pada roadmap.</b> Tn. Budi masuk 21 September pukul 22:30 dan dibaca
    /// 22 September pukul 06:00. Selisih jamnya hanya 7,5 jam, tetapi tanggalnya berbeda,
    /// sehingga lama dirawat tercatat 1 hari — bukan 0.
    /// </remarks>
    [Theory]
    // Masuk malam, dibaca pagi keesokan harinya: selisih jam 7,5 — hasilnya 1, bukan 0.
    [InlineData("2026-09-21T22:30:00", "2026-09-22T06:00:00", 1)]
    // Masuk dan dibaca pada tanggal yang sama: hasilnya tetap 1, karena pasien memang dirawat
    // hari itu.
    [InlineData("2026-09-21T06:00:00", "2026-09-21T23:00:00", 1)]
    // Selisih jam kurang dari 24, tetapi tanggalnya berbeda: hasilnya 1.
    [InlineData("2026-09-21T06:00:00", "2026-09-22T05:00:00", 1)]
    // Bertambah pada pergantian tanggal, bukan setiap genap 24 jam.
    [InlineData("2026-09-21T23:00:00", "2026-09-23T01:00:00", 2)]
    [InlineData("2026-09-21T06:00:00", "2026-09-25T06:00:00", 4)]
    public void Kriteria2Dan3_LamaDirawatDihitungDariSelisihTanggalDenganHasilPalingSedikitSatu(
        string masuk,
        string dibaca,
        int diharapkan)
    {
        var hasil = InpCensusQueryService.CalculateLengthOfStayDays(
            DateTime.Parse(masuk),
            DateTime.Parse(dibaca));

        Assert.Equal(diharapkan, hasil);
    }

    /// <remarks>
    /// Lima episode berstatus berbeda, dan census memuat tepat dua. Status <c>Closed</c>
    /// disetel langsung karena endpoint penutupannya milik <c>BE-RWI-025</c>; jalur
    /// endpoint-nya diuji ulang pada task tersebut.
    /// </remarks>
    [Fact]
    public async Task Kriteria1_CensusMemuatAdmittedDanDischargePendingSaja()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync("Melati 3");
        var bedAdmitted = await world.AddBedAsync(room, "3A");
        var bedPending = await world.AddBedAsync(room, "3B");
        var bedClosed = await world.AddBedAsync(room, "3C");
        var bedCancelled = await world.AddBedAsync(room, "3D");

        // 1. Draft — tanpa penempatan.
        await world.OpenDraftEpisodeAsync();

        // 2. Admitted.
        var admitted = await world.AddPatientAsync("Ny. Admitted", Gender.Female);
        await world.OpenAndPlaceAsync(bedAdmitted, admitted.Id);

        // 3. DischargePending.
        var pending = await world.AddPatientAsync("Ny. Pending", Gender.Female);
        var episodePending = await world.OpenAndPlaceAsync(bedPending, pending.Id);

        await world.DischargeService.DecideDischargeAsync(
            episodePending.Id,
            new DecideDischargeRequest { DischargeType = (int)InpDischargeType.DoctorApproved },
            InpatientEpisodeTestWorld.ActorUserId,
            world.Doctor.Id);

        // 4. Closed.
        var closed = await world.AddPatientAsync("Ny. Closed", Gender.Female);
        var episodeClosed = await world.OpenAndPlaceAsync(bedClosed, closed.Id);

        var trackedClosed = await world.DbContext.Set<InpEpisode>()
            .FirstAsync(x => x.Id == episodeClosed.Id);
        trackedClosed.EpisodeStatus = InpEpisodeStatus.Closed;
        trackedClosed.ClosedAt = DateTime.UtcNow;
        await world.DbContext.SaveChangesAsync();

        // 5. Cancelled.
        var cancelled = await world.AddPatientAsync("Ny. Cancelled", Gender.Female);
        var episodeCancelled = await world.OpenAndPlaceAsync(bedCancelled, cancelled.Id);

        await world.EpisodeService.CancelAdmissionAsync(
            episodeCancelled.Id,
            new CancelAdmissionRequest { Reason = "Pasien pulang paksa." },
            InpatientEpisodeTestWorld.SupervisorUserId,
            actorIsSupervisorOrWardHead: true);

        var census = await world.CensusQueryService.GetCensusAsync(new CensusQuery());

        Assert.Equal(2, census.TotalData);
        Assert.Contains(census.Items, x => x.PatientName == "Ny. Admitted");
        Assert.Contains(census.Items, x => x.PatientName == "Ny. Pending");
    }

    /// <remarks>
    /// Kolom kepergian fisik disetel langsung karena endpoint pencatatannya milik
    /// <c>BE-RWI-027</c>. Jalur endpoint-nya diuji ulang pada task tersebut.
    /// </remarks>
    [Fact]
    public async Task Kriteria4_PasienYangKepergiannyaSudahDicatatTidakMunculPadaCensus()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");

        var episode = await world.OpenAndPlaceAsync(bed);

        await world.DischargeService.DecideDischargeAsync(
            episode.Id,
            new DecideDischargeRequest { DischargeType = (int)InpDischargeType.DoctorApproved },
            InpatientEpisodeTestWorld.ActorUserId,
            world.Doctor.Id);

        var sebelum = await world.CensusQueryService.GetCensusAsync(new CensusQuery());
        Assert.Single(sebelum.Items);

        var tracked = await world.DbContext.Set<InpEpisode>().FirstAsync(x => x.Id == episode.Id);
        tracked.PhysicallyLeftAt = DateTime.UtcNow;
        tracked.PhysicallyLeftByUserId = InpatientEpisodeTestWorld.ActorUserId;
        await world.DbContext.SaveChangesAsync();

        var sesudah = await world.CensusQueryService.GetCensusAsync(new CensusQuery());
        Assert.Empty(sesudah.Items);
    }

    [Fact]
    public async Task Kriteria5_RingkasanMenghitungPerUnitLayananDanPerKelas()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync("Melati 3");
        var bedSatu = await world.AddBedAsync(room, "3A");
        var bedDua = await world.AddBedAsync(room, "3B");

        await world.OpenAndPlaceAsync(bedSatu);

        var sari = await world.AddPatientAsync("Ny. Sari", Gender.Female);
        await world.OpenAndPlaceAsync(bedDua, sari.Id);

        var ringkasan = await world.CensusQueryService.GetCensusSummaryAsync(new CensusQuery());

        Assert.Equal(2, ringkasan.TotalPatient);
        Assert.Equal(0, ringkasan.TotalRequiringIsolation);

        var unit = Assert.Single(ringkasan.ByServiceUnit);
        Assert.Equal(world.ServiceUnit.Id, unit.Id);
        Assert.Equal(2, unit.Total);

        var kelas = Assert.Single(ringkasan.ByPatientClass);
        Assert.Equal(world.PatientClass.Id, kelas.Id);
        Assert.Equal(2, kelas.Total);
    }

    [Fact]
    public async Task CensusMenampilkanLokasiDpjpDanPerawatPenanggungJawab()
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

        var census = await world.CensusQueryService.GetCensusAsync(new CensusQuery());

        var butir = Assert.Single(census.Items);

        Assert.Equal("Melati 3", butir.RoomName);
        Assert.Equal("3B", butir.BedName);
        Assert.Equal("dr. Andi", butir.DoctorName);
        Assert.Equal("Ns. Sari", butir.NurseName);
        Assert.True(butir.LengthOfStayDays >= 1);
    }

    [Fact]
    public async Task CensusDapatDisaringUnitLayananKamarDanKebutuhanIsolasi()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var melati = await world.AddRoomAsync("Melati 3");
        var isolasi = await world.AddRoomAsync("Isolasi 1");

        var bedMelati = await world.AddBedAsync(melati, "3A");
        var bedIsolasi = await world.AddBedAsync(isolasi, "ISO-1", isIsolationBed: true);

        await world.OpenAndPlaceAsync(bedMelati);

        var sari = await world.AddPatientAsync("Ny. Sari", Gender.Female);
        var episodeIsolasi = await world.OpenDraftEpisodeAsync(sari.Id);

        await world.EpisodeService.SetIsolationRequirementAsync(
            episodeIsolasi.Id,
            new SetIsolationRequirementRequest
            {
                RequiresIsolation = true,
                IsolationNote = "Kecurigaan tuberkulosis aktif."
            },
            InpatientEpisodeTestWorld.ActorUserId,
            actorDoctorId: null);

        await world.BedOccupancyService.PlacePatientAsync(
            new PlacePatientRequest { EpisodeId = episodeIsolasi.Id, BedId = bedIsolasi.Id },
            InpatientEpisodeTestWorld.ActorUserId);

        var seluruhnya = await world.CensusQueryService.GetCensusAsync(new CensusQuery());
        Assert.Equal(2, seluruhnya.TotalData);

        var perKamar = await world.CensusQueryService.GetCensusAsync(
            new CensusQuery { RoomId = isolasi.Id });

        var butir = Assert.Single(perKamar.Items);
        Assert.Equal("Ny. Sari", butir.PatientName);

        var perIsolasi = await world.CensusQueryService.GetCensusAsync(
            new CensusQuery { RequiresIsolation = true });

        Assert.Single(perIsolasi.Items);

        var perUnitLain = await world.CensusQueryService.GetCensusAsync(
            new CensusQuery { ServiceUnitId = Guid.NewGuid() });

        Assert.Empty(perUnitLain.Items);
    }

    /// <remarks>
    /// Census tidak pernah memuat isi klinis. Ia dibaca hampir seluruh peran ruangan, dan
    /// diagnosis maupun keterangan kebutuhan isolasi tidak boleh bocor lewat sini.
    /// </remarks>
    [Fact]
    public void CensusTidakMemuatKolomKlinis()
    {
        var properties = typeof(CensusItemResponse)
            .GetProperties()
            .Select(x => x.Name)
            .ToList();

        Assert.DoesNotContain("IsolationNote", properties);
        Assert.DoesNotContain("Notes", properties);
        Assert.DoesNotContain("PrimaryDiagnosisText", properties);
        Assert.DoesNotContain("ClinicalSummary", properties);
    }
}
