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
/// <c>BE-RWI-019</c> — pasien dapat berpindah tanpa episode terputus.
/// </summary>
/// <remarks>
/// <b>Kesalahan yang paling mahal di modul ini adalah menulis daftar aturan kedua khusus
/// perpindahan.</b> Dua daftar akan berselisih dalam hitungan minggu, dan jalur perpindahan
/// justru yang paling sering dipakai petugas yang sedang terburu-buru. Karena itu ada satu
/// test khusus yang menjalankan ulang skenario penolakan jenis kelamin lewat jalur
/// perpindahan, dan memeriksa bahwa kalimatnya <b>sama persis</b>.
/// </remarks>
public sealed class InpBedTransferTests
{
    [Fact]
    public async Task Kriteria1_PerpindahanMenghasilkanDuaBarisPenempatanDanYangLamaDitutupDenganAlasanTransfer()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync("Melati 3");
        var awal = await world.AddBedAsync(room, "3A");
        var tujuan = await world.AddBedAsync(room, "3B");

        var episode = await world.OpenAndPlaceAsync(awal);

        var hasil = await world.BedOccupancyService.TransferAsync(
            new TransferPatientRequest
            {
                EpisodeId = episode.Id,
                TargetBedId = tujuan.Id,
                TransferReason = "Kondisi memburuk, perlu dekat pos perawat."
            },
            InpatientEpisodeTestWorld.SupervisorUserId,
            actorDoctorId: null);

        Assert.Equal(InpEpisodeOperationStatus.Success, hasil.Status);

        var riwayat = await world.BedOccupancyService.GetPlacementsByEpisodeAsync(episode.Id);

        Assert.Equal(2, riwayat.Count);

        Assert.Equal(awal.Id, riwayat[0].BedId);
        Assert.NotNull(riwayat[0].EndDateTime);
        Assert.Equal((int)InpBedPlacementEndReason.Transfer, riwayat[0].EndReason);
        Assert.False(riwayat[0].IsCurrent);

        Assert.Equal(tujuan.Id, riwayat[1].BedId);
        Assert.Null(riwayat[1].EndDateTime);
        Assert.True(riwayat[1].IsCurrent);

        var bedLama = await world.DbContext.Set<MstBed>().AsNoTracking().FirstAsync(x => x.Id == awal.Id);
        var bedBaru = await world.DbContext.Set<MstBed>().AsNoTracking().FirstAsync(x => x.Id == tujuan.Id);

        Assert.Equal(BedStatus.Available, bedLama.BedStatus);
        Assert.Equal(BedStatus.Occupied, bedBaru.BedStatus);
    }

    /// <remarks>
    /// <c>INV-INP-07</c>. Bila pembukaan penempatan baru gagal, penempatan lama <b>tidak jadi
    /// ditutup</b>. Tidak pernah ada satu saat pun pasien tercatat tanpa tempat tidur.
    /// </remarks>
    [Fact]
    public async Task Kriteria2_BilaPembukaanPenempatanBaruGagalPasienTetapDiTempatSemula()
    {
        var databaseName = $"inpatient-transfer-fail-{Guid.NewGuid():N}";

        var world = await InpatientEpisodeTestWorld.CreateAsync(
            dbContext: IsolatedInpatientDbContextFactory.Create(databaseName));

        var room = await world.AddRoomAsync("Melati 3");
        var awal = await world.AddBedAsync(room, "3A");
        var tujuan = await world.AddBedAsync(room, "3B");

        var episode = await world.OpenAndPlaceAsync(awal);

        await using var failing = IsolatedInpatientDbContextFactory.CreateFailingSave(databaseName);

        var failingWorld = InpatientEpisodeTestWorld.Build(
            failing,
            world.Patient,
            world.Doctor,
            world.ServiceUnit,
            world.PatientClass);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            failingWorld.BedOccupancyService.TransferAsync(
                new TransferPatientRequest
                {
                    EpisodeId = episode.Id,
                    TargetBedId = tujuan.Id,
                    TransferReason = "Kondisi memburuk, perlu dekat pos perawat."
                },
                InpatientEpisodeTestWorld.SupervisorUserId,
                actorDoctorId: null));

        await using var pembaca = IsolatedInpatientDbContextFactory.Create(databaseName);

        var placements = await pembaca.Set<InpBedPlacement>()
            .Where(x => x.EpisodeId == episode.Id)
            .ToListAsync();

        var satuSatunya = Assert.Single(placements);

        Assert.Equal(awal.Id, satuSatunya.BedId);
        Assert.Null(satuSatunya.EndDateTime);
    }

    /// <remarks>
    /// <c>RWI-DEC-013</c>. Kelas yang ditagihkan mengikuti kamar yang ditempati, dan riwayat
    /// penempatanlah yang menyimpannya — bukan kolom pada episode. Tanpa itu, penagihan pasien
    /// yang pindah kelas di tengah perawatan tidak dapat dihitung.
    /// </remarks>
    [Fact]
    public async Task Kriteria3_KelasYangDitagihkanMengikutiKamarTujuan()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var kelasSatu = new MstPatientClass
        {
            Id = Guid.NewGuid(),
            PatientClassCode = "KELAS-1-VIP",
            PatientClassName = "Kelas 1",
            ClassLevel = 1,
            IsForInpatient = true,
            IsActive = true,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = InpatientEpisodeTestWorld.ActorUserId
        };

        world.DbContext.Set<MstPatientClass>().Add(kelasSatu);
        await world.DbContext.SaveChangesAsync();

        var kamarKelasDua = await world.AddRoomAsync("Melati 3", world.PatientClass.Id);
        var kamarKelasSatu = await world.AddRoomAsync("Anggrek 1", kelasSatu.Id);

        var awal = await world.AddBedAsync(kamarKelasDua, "3A");
        var tujuan = await world.AddBedAsync(kamarKelasSatu, "1A");

        var episode = await world.OpenAndPlaceAsync(awal);

        await world.BedOccupancyService.TransferAsync(
            new TransferPatientRequest
            {
                EpisodeId = episode.Id,
                TargetBedId = tujuan.Id,
                TransferReason = "Naik kelas atas permintaan keluarga."
            },
            InpatientEpisodeTestWorld.SupervisorUserId,
            actorDoctorId: null);

        var riwayat = await world.BedOccupancyService.GetPlacementsByEpisodeAsync(episode.Id);

        Assert.Equal(world.PatientClass.Id, riwayat[0].PatientClassId);
        Assert.Equal(kelasSatu.Id, riwayat[1].PatientClassId);

        // Kolom kelas pada episode tetap merekam pilihan saat admisi dibuka, sehingga jejak
        // kelas awal tidak hilang.
        var tersimpan = await world.DbContext.Set<InpEpisode>()
            .AsNoTracking()
            .FirstAsync(x => x.Id == episode.Id);

        Assert.Equal(world.PatientClass.Id, tersimpan.PatientClassId);
    }

    /// <remarks>
    /// <c>GUARD-INP-01</c>. Berlaku <b>hanya</b> untuk pemohon berperan dokter; kepala
    /// ruangan, perawat pelaksana, dan supervisor tetap boleh memindahkan tanpa menjadi DPJP —
    /// <c>RWI-DEC-012</c> yang tidak dicabut.
    /// </remarks>
    [Fact]
    public async Task Kriteria4_DokterYangBukanDpjpAktifDitolak403SementaraKepalaRuanganTetapBoleh()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync("Melati 3");
        var awal = await world.AddBedAsync(room, "3A");
        var tujuan = await world.AddBedAsync(room, "3B");
        var dokterJaga = await world.AddDoctorAsync("dr. Rina");

        var episode = await world.OpenAndPlaceAsync(awal);

        var olehDokterLain = await world.BedOccupancyService.TransferAsync(
            new TransferPatientRequest
            {
                EpisodeId = episode.Id,
                TargetBedId = tujuan.Id,
                TransferReason = "Menurut saya perlu pindah."
            },
            InpatientEpisodeTestWorld.SupervisorUserId,
            actorDoctorId: dokterJaga.Id);

        Assert.Equal(InpEpisodeOperationStatus.Forbidden, olehDokterLain.Status);
        Assert.Contains("Hanya DPJP episode ini yang dapat memindahkan pasien", olehDokterLain.Message);

        // Tidak ada kolom keterangan apa pun pada permintaan perpindahan yang dapat dipakai
        // melewati penjaga ini.
        var kolom = typeof(TransferPatientRequest).GetProperties().Select(x => x.Name).ToList();

        Assert.Equal(3, kolom.Count);
        Assert.Contains("EpisodeId", kolom);
        Assert.Contains("TargetBedId", kolom);
        Assert.Contains("TransferReason", kolom);

        // Kepala ruangan — bukan dokter — tetap boleh.
        var olehKepalaRuangan = await world.BedOccupancyService.TransferAsync(
            new TransferPatientRequest
            {
                EpisodeId = episode.Id,
                TargetBedId = tujuan.Id,
                TransferReason = "Kondisi memburuk, perlu dekat pos perawat."
            },
            InpatientEpisodeTestWorld.SupervisorUserId,
            actorDoctorId: null);

        Assert.Equal(InpEpisodeOperationStatus.Success, olehKepalaRuangan.Status);
    }

    [Fact]
    public async Task Kriteria5_PerpindahanTanpaAlasanMedisDitolak400()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync("Melati 3");
        var awal = await world.AddBedAsync(room, "3A");
        var tujuan = await world.AddBedAsync(room, "3B");

        var episode = await world.OpenAndPlaceAsync(awal);

        var hasil = await world.BedOccupancyService.TransferAsync(
            new TransferPatientRequest
            {
                EpisodeId = episode.Id,
                TargetBedId = tujuan.Id,
                TransferReason = "  -  "
            },
            InpatientEpisodeTestWorld.SupervisorUserId,
            actorDoctorId: null);

        Assert.Equal(InpEpisodeOperationStatus.Invalid, hasil.Status);
        Assert.Equal("Alasan perpindahan wajib diisi.", hasil.Message);
    }

    /// <remarks>
    /// Menjalankan ulang skenario <c>UAT-29</c> lewat jalur perpindahan. Kode dan kalimat
    /// penolakannya wajib <b>sama persis</b> dengan jalur penempatan — itulah bukti bahwa di
    /// seluruh source hanya ada satu daftar aturan.
    /// </remarks>
    [Fact]
    public async Task Kriteria6_PenolakanJenisKelaminLewatPerpindahanSamaPersisDenganLewatPenempatan()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var kamarAsal = await world.AddRoomAsync("Anggrek 1");
        var kamarTujuan = await world.AddRoomAsync("Melati 3");

        var tempatAsal = await world.AddBedAsync(kamarAsal, "1A");
        var tempatTujuan = await world.AddBedAsync(kamarTujuan, "3B");
        var tempatPenghuni = await world.AddBedAsync(kamarTujuan, "3A");

        // Ibu Rina — perempuan — sudah menghuni kamar Melati 3.
        await world.OpenAndPlaceAsync(tempatPenghuni);

        var budi = await world.AddPatientAsync("Tn. Budi", Gender.Male);
        var episodeBudi = await world.OpenAndPlaceAsync(tempatAsal, budi.Id);

        var lewatPerpindahan = await world.BedOccupancyService.TransferAsync(
            new TransferPatientRequest
            {
                EpisodeId = episodeBudi.Id,
                TargetBedId = tempatTujuan.Id,
                TransferReason = "Permintaan keluarga."
            },
            InpatientEpisodeTestWorld.SupervisorUserId,
            actorDoctorId: null);

        // Jalur penempatan pada pasien laki-laki lain, ke kamar yang sama.
        var joko = await world.AddPatientAsync("Tn. Joko", Gender.Male);
        var episodeJoko = await world.OpenDraftEpisodeAsync(joko.Id);

        var lewatPenempatan = await world.BedOccupancyService.PlacePatientAsync(
            new PlacePatientRequest { EpisodeId = episodeJoko.Id, BedId = tempatTujuan.Id },
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.BusinessRuleRejected, lewatPerpindahan.Status);
        Assert.Equal(lewatPenempatan.Status, lewatPerpindahan.Status);
        Assert.Equal(lewatPenempatan.Message, lewatPerpindahan.Message);
        Assert.Equal(
            lewatPenempatan.Failures.Select(x => x.Code).OrderBy(x => x),
            lewatPerpindahan.Failures.Select(x => x.Code).OrderBy(x => x));
    }

    [Fact]
    public async Task PerpindahanKeTempatTidurYangSamaDitolak400()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");

        var episode = await world.OpenAndPlaceAsync(bed);

        var hasil = await world.BedOccupancyService.TransferAsync(
            new TransferPatientRequest
            {
                EpisodeId = episode.Id,
                TargetBedId = bed.Id,
                TransferReason = "Salah tekan."
            },
            InpatientEpisodeTestWorld.SupervisorUserId,
            actorDoctorId: null);

        Assert.Equal(InpEpisodeOperationStatus.Invalid, hasil.Status);
        Assert.Equal("Tempat tidur tujuan sama dengan tempat tidur saat ini.", hasil.Message);
    }

    [Fact]
    public async Task PasienYangSudahDiputuskanPulangTidakDapatDipindahkan()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var awal = await world.AddBedAsync(room, "3A");
        var tujuan = await world.AddBedAsync(room, "3B");

        var episode = await world.OpenAndPlaceAsync(awal);

        await world.DischargeService.DecideDischargeAsync(
            episode.Id,
            new DecideDischargeRequest { DischargeType = (int)InpDischargeType.DoctorApproved },
            InpatientEpisodeTestWorld.ActorUserId,
            world.Doctor.Id);

        var hasil = await world.BedOccupancyService.TransferAsync(
            new TransferPatientRequest
            {
                EpisodeId = episode.Id,
                TargetBedId = tujuan.Id,
                TransferReason = "Permintaan keluarga."
            },
            InpatientEpisodeTestWorld.SupervisorUserId,
            actorDoctorId: null);

        Assert.Equal(InpEpisodeOperationStatus.BusinessRuleRejected, hasil.Status);
        Assert.Equal(
            "Pasien sudah diputuskan boleh pulang, sehingga tidak dapat dipindahkan lagi.",
            hasil.Message);
    }
}
