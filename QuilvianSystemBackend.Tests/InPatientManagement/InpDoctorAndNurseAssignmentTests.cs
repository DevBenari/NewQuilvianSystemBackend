using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;

namespace QuilvianSystemBackend.Tests.InPatientManagement;

/// <summary>
/// <c>BE-RWI-017</c> — sistem dapat menjawab siapa DPJP pada tanggal tertentu;
/// <c>BE-RWI-018</c> — perawat penanggung jawab tercatat, dan ketiadaannya tidak menahan
/// apa pun.
/// </summary>
/// <remarks>
/// <b>Bentuk berperiode adalah inti keduanya.</b> Menyimpan <c>CurrentDoctorId</c> sebagai
/// kolom pada episode membuat query lebih murah dan menghapus jawaban atas pertanyaan
/// "siapa yang berwenang pada 22 September" selamanya. Bentuk berperiode dikunci
/// `blueprint-manifest.md` bagian 8 butir 4.
/// </remarks>
public sealed class InpDoctorAndNurseAssignmentTests
{
    // =========================================================================
    // BE-RWI-017 — DPJP
    // =========================================================================

    /// <remarks>
    /// Skenario roadmap: dr. Andi 21–23 September, dr. Rina 23–25 September, dan pada
    /// 25 September sistem masih dapat menjawab siapa yang berwenang pada 22 September.
    /// </remarks>
    [Fact]
    public async Task Kriteria1_RiwayatBerperiodeMasihDapatMenjawabSiapaBerwenangPadaTanggalLampau()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");
        var rina = await world.AddDoctorAsync("dr. Rina");

        var episode = await world.OpenAndPlaceAsync(bed);

        // Penugasan pertama dimundurkan ke 21 September supaya periodenya dapat diperiksa.
        var pertama = await world.DbContext.Set<InpDoctorAssignment>()
            .FirstAsync(x => x.EpisodeId == episode.Id);

        pertama.StartDateTime = new DateTime(2026, 9, 21, 9, 15, 0, DateTimeKind.Utc);
        await world.DbContext.SaveChangesAsync();

        var alih = await world.EpisodeService.HandoverDoctorAsync(
            episode.Id,
            new HandoverDoctorRequest
            {
                DoctorId = rina.Id,
                HandoverReason = "dr. Andi cuti mulai 23 September."
            },
            InpatientEpisodeTestWorld.SupervisorUserId,
            actorIsWardHeadOrSupervisor: true);

        Assert.Equal(InpEpisodeOperationStatus.Success, alih.Status);

        // Waktu penutupan dan pembukaan disetel ke 23 September, meniru berjalannya hari.
        var seluruh = await world.DbContext.Set<InpDoctorAssignment>()
            .Where(x => x.EpisodeId == episode.Id)
            .OrderBy(x => x.SequenceNumber)
            .ToListAsync();

        Assert.Equal(2, seluruh.Count);

        var batas = new DateTime(2026, 9, 23, 8, 0, 0, DateTimeKind.Utc);
        seluruh[0].EndDateTime = batas;
        seluruh[1].StartDateTime = batas;
        await world.DbContext.SaveChangesAsync();

        var pada22September = await world.EpisodeService.GetDoctorAssignmentAtAsync(
            episode.Id,
            new DateTime(2026, 9, 22, 10, 0, 0, DateTimeKind.Utc));

        Assert.NotNull(pada22September);
        Assert.Equal(world.Doctor.Id, pada22September!.DoctorId);

        var pada24September = await world.EpisodeService.GetDoctorAssignmentAtAsync(
            episode.Id,
            new DateTime(2026, 9, 24, 10, 0, 0, DateTimeKind.Utc));

        Assert.NotNull(pada24September);
        Assert.Equal(rina.Id, pada24September!.DoctorId);
    }

    /// <remarks>
    /// <c>INV-INP-03</c>. Pengalihan menutup penugasan lama dan membuka yang baru pada
    /// tindakan yang sama, sehingga tidak pernah ada dua DPJP aktif — dan tidak pernah ada
    /// satu saat pun episode tanpa DPJP.
    /// </remarks>
    [Fact]
    public async Task Kriteria2_SatuEpisodeAktifPunyaTepatSatuDpjpAktif()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");
        var rina = await world.AddDoctorAsync("dr. Rina");

        var episode = await world.OpenAndPlaceAsync(bed);

        await world.EpisodeService.HandoverDoctorAsync(
            episode.Id,
            new HandoverDoctorRequest
            {
                DoctorId = rina.Id,
                HandoverReason = "Pengalihan tanggung jawab pelayanan."
            },
            InpatientEpisodeTestWorld.SupervisorUserId,
            actorIsWardHeadOrSupervisor: true);

        var aktif = await world.DbContext.Set<InpDoctorAssignment>()
            .AsNoTracking()
            .CountAsync(x => x.EpisodeId == episode.Id && x.EndDateTime == null);

        Assert.Equal(1, aktif);

        var riwayat = await world.EpisodeService.GetDoctorAssignmentsAsync(episode.Id);

        Assert.Equal(2, riwayat.Count);
        Assert.False(riwayat[0].IsCurrent);
        Assert.True(riwayat[1].IsCurrent);
        Assert.Equal(rina.Id, riwayat[1].DoctorId);
        Assert.NotNull(riwayat[0].EndDateTime);
    }

    [Fact]
    public async Task Kriteria3_PengalihanTanpaAlasanDitolak400()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");
        var rina = await world.AddDoctorAsync("dr. Rina");

        var episode = await world.OpenAndPlaceAsync(bed);

        var hasil = await world.EpisodeService.HandoverDoctorAsync(
            episode.Id,
            new HandoverDoctorRequest { DoctorId = rina.Id, HandoverReason = "  -  " },
            InpatientEpisodeTestWorld.SupervisorUserId,
            actorIsWardHeadOrSupervisor: true);

        Assert.Equal(InpEpisodeOperationStatus.Invalid, hasil.Status);
        Assert.Contains("Alasan pengalihan DPJP wajib diisi", hasil.Message);
    }

    [Fact]
    public async Task Kriteria4_PengalihanHanyaOlehKepalaRuanganAtauSupervisor()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");
        var rina = await world.AddDoctorAsync("dr. Rina");

        var episode = await world.OpenAndPlaceAsync(bed);

        var hasil = await world.EpisodeService.HandoverDoctorAsync(
            episode.Id,
            new HandoverDoctorRequest
            {
                DoctorId = rina.Id,
                HandoverReason = "Pengalihan tanggung jawab pelayanan."
            },
            InpatientEpisodeTestWorld.ActorUserId,
            actorIsWardHeadOrSupervisor: false);

        Assert.Equal(InpEpisodeOperationStatus.Forbidden, hasil.Status);
        Assert.Contains("kepala ruangan atau supervisor", hasil.Message);
    }

    [Fact]
    public async Task MengalihkanKepadaDpjpYangSamaDitolak()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");

        var episode = await world.OpenAndPlaceAsync(bed);

        var hasil = await world.EpisodeService.HandoverDoctorAsync(
            episode.Id,
            new HandoverDoctorRequest
            {
                DoctorId = world.Doctor.Id,
                HandoverReason = "Salah tekan."
            },
            InpatientEpisodeTestWorld.SupervisorUserId,
            actorIsWardHeadOrSupervisor: true);

        Assert.Equal(InpEpisodeOperationStatus.BusinessRuleRejected, hasil.Status);
    }

    // =========================================================================
    // BE-RWI-018 — Perawat penanggung jawab
    // =========================================================================

    [Fact]
    public async Task Kriteria1Dan4_PenugasanMenutupPenugasanSebelumnyaDanRiwayatnyaTerbacaUrut()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");
        var sari = await world.AddEmployeeAsync("Ns. Sari");
        var wati = await world.AddEmployeeAsync("Ns. Wati");

        var episode = await world.OpenAndPlaceAsync(bed);

        await world.EpisodeService.AssignNurseAsync(
            episode.Id,
            new AssignNurseRequest { EmployeeId = sari.Id },
            InpatientEpisodeTestWorld.SupervisorUserId,
            actorIsWardHeadOrSupervisor: true);

        await world.EpisodeService.AssignNurseAsync(
            episode.Id,
            new AssignNurseRequest { EmployeeId = wati.Id },
            InpatientEpisodeTestWorld.SupervisorUserId,
            actorIsWardHeadOrSupervisor: true);

        var riwayat = await world.EpisodeService.GetNurseAssignmentsAsync(episode.Id);

        Assert.Equal(2, riwayat.Count);
        Assert.Equal(1, riwayat[0].SequenceNumber);
        Assert.Equal(sari.Id, riwayat[0].EmployeeId);
        Assert.False(riwayat[0].IsCurrent);
        Assert.NotNull(riwayat[0].EndDateTime);

        Assert.Equal(2, riwayat[1].SequenceNumber);
        Assert.Equal(wati.Id, riwayat[1].EmployeeId);
        Assert.True(riwayat[1].IsCurrent);
    }

    /// <remarks>
    /// Kriteria 2, dan ini yang paling mudah dikerjakan terbalik menjadi "wajib ada perawat
    /// sebelum episode aktif". <c>RWI-DEC-032</c> memilih <b>tidak menahan</b>, karena
    /// penugasan perawat sering menyusul beberapa menit setelah pasien tiba. Test ini
    /// membuktikan tiga tindakan besar semuanya tetap berhasil tanpa perawat.
    /// </remarks>
    [Fact]
    public async Task Kriteria2_PenempatanPerpindahanDanKeputusanPulangSemuanyaBerhasilTanpaPerawat()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync("Melati 3");
        var awal = await world.AddBedAsync(room, "3A");
        var tujuan = await world.AddBedAsync(room, "3B");

        var episode = await world.OpenAndPlaceAsync(awal);

        var pindah = await world.BedOccupancyService.TransferAsync(
            new TransferPatientRequest
            {
                EpisodeId = episode.Id,
                TargetBedId = tujuan.Id,
                TransferReason = "Permintaan keluarga, kamar lebih dekat pos perawat."
            },
            InpatientEpisodeTestWorld.SupervisorUserId,
            actorDoctorId: null);

        Assert.Equal(InpEpisodeOperationStatus.Success, pindah.Status);

        var pulang = await world.DischargeService.DecideDischargeAsync(
            episode.Id,
            new DecideDischargeRequest { DischargeType = (int)InpDischargeType.DoctorApproved },
            InpatientEpisodeTestWorld.ActorUserId,
            world.Doctor.Id);

        Assert.Equal(InpEpisodeOperationStatus.Success, pulang.Status);

        var perawat = await world.EpisodeService.GetNurseAssignmentsAsync(episode.Id);

        Assert.Empty(perawat);
    }

    /// <remarks>
    /// Kriteria 3. Endpoint daftar pantaunya — <c>GET /monitoring/unassigned-nurse-episodes</c>
    /// — milik <c>BE-RWI-029</c> dan belum dibuka, sehingga yang dibuktikan di sini adalah
    /// query-nya di tingkat service.
    /// </remarks>
    [Fact]
    public async Task Kriteria3_EpisodeTanpaPerawatMunculPadaDaftarPantauLaluHilangSetelahDitugaskan()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");
        var sari = await world.AddEmployeeAsync("Ns. Sari");

        var episode = await world.OpenAndPlaceAsync(bed);

        var sebelum = await world.CensusQueryService.GetUnassignedNurseEpisodesAsync();

        var butir = Assert.Single(sebelum);
        Assert.Equal(episode.Id, butir.EpisodeId);

        await world.EpisodeService.AssignNurseAsync(
            episode.Id,
            new AssignNurseRequest { EmployeeId = sari.Id },
            InpatientEpisodeTestWorld.SupervisorUserId,
            actorIsWardHeadOrSupervisor: true);

        var sesudah = await world.CensusQueryService.GetUnassignedNurseEpisodesAsync();

        Assert.Empty(sesudah);
    }

    [Fact]
    public async Task Kriteria5_PenugasanPerawatHanyaOlehKepalaRuanganAtauSupervisor()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");
        var sari = await world.AddEmployeeAsync("Ns. Sari");

        var episode = await world.OpenAndPlaceAsync(bed);

        var hasil = await world.EpisodeService.AssignNurseAsync(
            episode.Id,
            new AssignNurseRequest { EmployeeId = sari.Id },
            InpatientEpisodeTestWorld.ActorUserId,
            actorIsWardHeadOrSupervisor: false);

        Assert.Equal(InpEpisodeOperationStatus.Forbidden, hasil.Status);
    }
}
