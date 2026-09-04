using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Tests.InPatientManagement;

/// <summary>
/// <c>BE-RWI-006</c> beserta test regresi <c>BE-RWI-032</c> untuk jalur `MstBed` yang dipakai
/// modul tetangga.
/// </summary>
/// <remarks>
/// <para>
/// <c>BE-RWI-006</c> adalah <b>perubahan perilaku pada modul milik pihak lain</b>, bukan
/// penambahan fitur. Karena itu <c>RWI-DEC-051</c> mewajibkan test regresi dibawa bersama
/// perubahannya, dan berkas ini memuat keduanya: pembuktian aturan barunya, dan pembuktian
/// bahwa jalur lama yang masih diizinkan tidak ikut rusak.
/// </para>
/// <para>
/// <b>Cakupannya sengaja sempit.</b> <c>RWI-RISK-002</c> mencatat bahwa hari ini tidak ada test
/// yang menjaga jalur poliklinik, IGD, dan farmasi. Berkas ini menutup lubang itu <b>hanya</b>
/// untuk jalur `MstBed` yang benar-benar disentuh <c>BE-RWI-006</c> — tidak untuk seluruh modul
/// tetangga, dan tidak melebar diam-diam.
/// </para>
/// </remarks>
public sealed class BedAvailabilityRegressionTests
{
    private static readonly Guid ActorUserId = Guid.NewGuid();

    /// <summary>
    /// Keempat nilai yang <b>tetap</b> menjadi wewenang admin master data menurut
    /// <c>RWI-RULE-027</c> aturan 5, ditambah <c>Available</c> sebagai jalan kembalinya.
    /// </summary>
    public static TheoryData<BedStatus> NilaiYangTetapDiizinkan => new()
    {
        BedStatus.Cleaning,
        BedStatus.Maintenance,
        BedStatus.Blocked,
        BedStatus.Inactive,
        BedStatus.Available
    };

    /// <summary>Kedua nilai yang dicabut dari wewenang admin oleh aturan 4.</summary>
    public static TheoryData<BedStatus> NilaiYangDicabut => new()
    {
        BedStatus.Reserved,
        BedStatus.Occupied
    };

    // =====================================================================
    // BE-RWI-006 — aturan barunya
    // =====================================================================

    /// <summary>
    /// Kriteria 1. Pesannya diperiksa apa adanya, bukan hanya kode 422, karena kalimat itulah
    /// yang memberi tahu admin jalan keluarnya.
    /// </summary>
    [Theory]
    [MemberData(nameof(NilaiYangDicabut))]
    public async Task MengirimTerisiAtauDipesan_DitolakDenganPesanDariValidationMatrix(
        BedStatus status)
    {
        await using var dbContext = NewDbContext();
        var bed = await SeedBedAsync(dbContext);
        var controller = BuildController(dbContext);

        var hasil = await controller.UpdateBedAvailability(
            bed.Id,
            new UpdateBedAvailabilityRequest { BedStatus = status });

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(hasil);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, objectResult.StatusCode);

        var response = Assert.IsType<ApiResponse<object>>(objectResult.Value);

        Assert.Equal(
            "Status Terisi dan Dipesan hanya dapat diubah lewat modul Rawat Inap. " +
            "Untuk menutup tempat tidur sementara, pakai status Pembersihan, Perbaikan, atau Diblokir.",
            response.Message);

        // Penolakan tidak boleh menyisakan perubahan separuh jalan.
        var tersimpan = await dbContext.Set<MstBed>().AsNoTracking().SingleAsync(x => x.Id == bed.Id);
        Assert.Equal(BedStatus.Available, tersimpan.BedStatus);
    }

    /// <summary>
    /// Kriteria 3. Menutup tempat tidur yang masih ditempati tidak memindahkan pasien ke mana
    /// pun — ia hanya membuat salinan status berbohong.
    /// </summary>
    [Fact]
    public async Task TempatTidurYangSedangDitempati_TidakDapatDisetelMaintenance()
    {
        await using var dbContext = NewDbContext();
        var bed = await SeedBedAsync(dbContext, BedStatus.Occupied);
        await SeedPenempatanAktifAsync(dbContext, bed.Id);

        var controller = BuildController(dbContext);

        var hasil = await controller.UpdateBedAvailability(
            bed.Id,
            new UpdateBedAvailabilityRequest { BedStatus = BedStatus.Maintenance });

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(hasil);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, objectResult.StatusCode);

        var response = Assert.IsType<ApiResponse<object>>(objectResult.Value);
        Assert.Contains("sedang ditempati pasien rawat inap", response.Message);

        var tersimpan = await dbContext.Set<MstBed>().AsNoTracking().SingleAsync(x => x.Id == bed.Id);
        Assert.Equal(BedStatus.Occupied, tersimpan.BedStatus);
    }

    /// <summary>
    /// Penempatan yang <b>sudah berakhir</b> tidak menahan apa pun. Tanpa test ini, penjaga di
    /// atas mudah ditulis terlalu lebar dan tempat tidur yang sudah ditinggalkan pasien ikut
    /// terkunci selamanya.
    /// </summary>
    [Fact]
    public async Task PenempatanYangSudahBerakhir_TidakMenahanPerubahanStatus()
    {
        await using var dbContext = NewDbContext();
        var bed = await SeedBedAsync(dbContext);
        await SeedPenempatanAktifAsync(dbContext, bed.Id, akhirnya: DateTime.UtcNow.AddHours(-1));

        var controller = BuildController(dbContext);

        var hasil = await controller.UpdateBedAvailability(
            bed.Id,
            new UpdateBedAvailabilityRequest { BedStatus = BedStatus.Cleaning });

        Assert.IsType<OkObjectResult>(hasil);

        var tersimpan = await dbContext.Set<MstBed>().AsNoTracking().SingleAsync(x => x.Id == bed.Id);
        Assert.Equal(BedStatus.Cleaning, tersimpan.BedStatus);
    }

    // =====================================================================
    // BE-RWI-032 — regresi jalur lama
    // =====================================================================

    /// <summary>
    /// Kriteria 1 dan 3 <c>BE-RWI-032</c>. Test ini gagal bila perubahan `BedController`
    /// melebihi kesepakatan, yaitu bila ia mulai menolak nilai yang seharusnya masih
    /// diizinkan.
    /// </summary>
    [Theory]
    [MemberData(nameof(NilaiYangTetapDiizinkan))]
    public async Task LayarMasterTempatTidur_TetapBerfungsiUntukNilaiYangMasihDiizinkan(
        BedStatus status)
    {
        await using var dbContext = NewDbContext();
        var bed = await SeedBedAsync(dbContext);
        var controller = BuildController(dbContext);

        var hasil = await controller.UpdateBedAvailability(
            bed.Id,
            new UpdateBedAvailabilityRequest
            {
                BedStatus = status,
                Description = "Ditutup untuk perbaikan rutin."
            });

        var okResult = Assert.IsType<OkObjectResult>(hasil);

        // Kriteria 4: bentuk balasannya tidak berubah.
        var response = Assert.IsType<ApiResponse<BedUpdateResponse>>(okResult.Value);

        Assert.Equal("Status ketersediaan bed berhasil diperbarui.", response.Message);
        Assert.NotNull(response.Data);
        Assert.Equal(bed.Id, response.Data!.Id);
        Assert.Equal(status, response.Data.BedStatus);
        Assert.Equal(status.ToString(), response.Data.BedStatusName);

        var tersimpan = await dbContext.Set<MstBed>().AsNoTracking().SingleAsync(x => x.Id == bed.Id);
        Assert.Equal(status, tersimpan.BedStatus);
        Assert.Equal("Ditutup untuk perbaikan rutin.", tersimpan.Description);
    }

    /// <summary>
    /// Kriteria 2 <c>BE-RWI-032</c>. Modul tetangga memakai `MstBed` untuk <b>membacanya</b> —
    /// mencari tempat tidur suatu kamar, memeriksa penandanya — tanpa menyetel status.
    /// Jalur baca itu tidak boleh ikut terpengaruh.
    /// </summary>
    [Fact]
    public async Task JalurBacaMstBedOlehModulLain_TetapBerjalan()
    {
        await using var dbContext = NewDbContext();
        var bed = await SeedBedAsync(dbContext, BedStatus.Occupied);
        await SeedPenempatanAktifAsync(dbContext, bed.Id);

        var terbaca = await dbContext.Set<MstBed>()
            .AsNoTracking()
            .Where(x => x.RoomId == bed.RoomId && !x.IsDelete)
            .ToListAsync();

        Assert.Single(terbaca);
        Assert.Equal(BedStatus.Occupied, terbaca[0].BedStatus);
        Assert.True(terbaca[0].IsForMale);
        Assert.False(terbaca[0].IsIsolationBed);
        Assert.True(terbaca[0].IsReservable);
    }

    /// <summary>
    /// Kriteria 2 dan 4 <c>BE-RWI-032</c>. Jalur tulis `MstBed` yang <b>bukan</b>
    /// `/availability` tidak disentuh `BE-RWI-006` dan wajib tetap seperti semula, termasuk
    /// untuk tempat tidur yang sedang ditempati.
    /// </summary>
    [Fact]
    public async Task JalurTulisLainPadaMstBed_TidakIkutTerkunci()
    {
        await using var dbContext = NewDbContext();
        var bed = await SeedBedAsync(dbContext, BedStatus.Occupied);
        await SeedPenempatanAktifAsync(dbContext, bed.Id);

        bed.BedName = "Melati 3B — nama dibetulkan";
        bed.SortOrder = 7;
        await dbContext.SaveChangesAsync();

        var tersimpan = await dbContext.Set<MstBed>().AsNoTracking().SingleAsync(x => x.Id == bed.Id);

        Assert.Equal("Melati 3B — nama dibetulkan", tersimpan.BedName);
        Assert.Equal(7, tersimpan.SortOrder);
        Assert.Equal(BedStatus.Occupied, tersimpan.BedStatus);
    }

    /// <summary>
    /// Tempat tidur yang tidak ada tetap dijawab <c>404</c>, bukan <c>422</c>. Urutan
    /// pemeriksaan ini penting: menolak nilai lebih dulu akan menyembunyikan salah ketik id.
    /// </summary>
    [Fact]
    public async Task TempatTidurTidakDitemukan_TetapDijawab404SebelumPemeriksaanNilai()
    {
        await using var dbContext = NewDbContext();
        var controller = BuildController(dbContext);

        var hasil = await controller.UpdateBedAvailability(
            Guid.NewGuid(),
            new UpdateBedAvailabilityRequest { BedStatus = BedStatus.Reserved });

        var notFound = Assert.IsType<NotFoundObjectResult>(hasil);
        var response = Assert.IsType<ApiResponse<object>>(notFound.Value);

        Assert.Equal("Bed tidak ditemukan.", response.Message);
    }

    // =====================================================================
    // Perkakas
    // =====================================================================

    private static ApplicationDbContext NewDbContext()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"bed-availability-{Guid.NewGuid():N}")
            .Options);

    private static BedController BuildController(ApplicationDbContext dbContext)
    {
        var controller = new BedController(
            dbContext,
            new LoggerService(NullLogger<LoggerService>.Instance, new HttpContextAccessor()));

        var identity = new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, ActorUserId.ToString()) },
            "TestAuth");

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };

        return controller;
    }

    private static async Task<MstBed> SeedBedAsync(
        ApplicationDbContext dbContext,
        BedStatus status = BedStatus.Available)
    {
        var bed = new MstBed
        {
            Id = Guid.NewGuid(),
            RoomId = Guid.NewGuid(),
            BedCode = "BD-RSMMC-00042",
            BedName = "Melati 3B",
            BedNumber = "3B",
            BedStatus = status,
            IsActive = true
        };

        dbContext.Set<MstBed>().Add(bed);
        await dbContext.SaveChangesAsync();

        return bed;
    }

    private static async Task SeedPenempatanAktifAsync(
        ApplicationDbContext dbContext,
        Guid bedId,
        DateTime? akhirnya = null)
    {
        dbContext.Set<InpBedPlacement>().Add(new InpBedPlacement
        {
            Id = Guid.NewGuid(),
            BedId = bedId,
            EpisodeId = Guid.NewGuid(),
            StartDateTime = DateTime.UtcNow.AddHours(-3),
            EndDateTime = akhirnya
        });

        await dbContext.SaveChangesAsync();
    }
}
