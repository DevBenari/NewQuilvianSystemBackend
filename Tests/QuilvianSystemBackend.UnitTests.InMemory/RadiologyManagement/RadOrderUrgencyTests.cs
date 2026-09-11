using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Tests.RadiologyManagement;

/// <summary>
/// Penanda cito pada pesanan radiologi — <c>BE-RAD-12</c>, acceptance criteria AC-42,
/// <c>FR-RAD-064</c> dan <c>FR-RAD-065</c>, <c>RAD-DEC-013</c>.
///
/// <para>
/// <b>Task ini menambah kolom pada tabel yang sudah dipakai tiga modul lain</b> — Rawat Jalan,
/// IGD, dan Rawat Inap semuanya memesan pemeriksaan radiologi hari ini. Karena itu yang paling
/// banyak diuji di sini bukan fitur barunya, melainkan <b>bahwa tidak ada yang rusak</b>:
/// pemanggil yang tidak mengenal field baru tetap berhasil, pesanan lama tetap terbaca, dan
/// index yang sudah ada tidak hilang.
/// </para>
/// </summary>
public sealed class RadOrderUrgencyTests
{
    private static readonly Guid DokterPengirim =
        Guid.Parse("aaaaaaaa-1111-1111-1111-111111111111");

    /* ================================================================== *
     * FR-RAD-065 — pemanggil lama tetap berjalan
     * ================================================================== */

    [Fact]
    public async Task FR065_PemanggilYangTidakMengirimPenandaTetapBerhasil()
    {
        // Modul Rawat Jalan membuat pesanan tanpa mengenal field penanda cito sama sekali.
        await using var db = Konteks();
        var master = await SeedMasterAsync(db);

        var hasil = await Service(db, DokterPengirim).CreateAsync(new CreateRadOrderRequest
        {
            EncounterId = Guid.NewGuid(),
            ProcedureId = master.ProcedureId,
            ModalityId = master.ModalityId,
            // IsUrgent sengaja tidak diisi — inilah yang dilakukan pemanggil lama.
        });

        Assert.Equal(RadOperationResultKind.Success, hasil.Kind);
        Assert.False(hasil.Value!.IsUrgent);

        var tersimpan = await db.RadOrders.AsNoTracking().SingleAsync();
        Assert.False(tersimpan.IsUrgent);
    }

    [Fact]
    public async Task PesananYangTidakCitoTidakMeninggalkanJejakPenandaan()
    {
        // Kolom "siapa menandai cito" yang terisi pada pesanan yang tidak pernah ditandai cito
        // membuat jejaknya kehilangan arti: setiap baris terlihat seperti pernah diputuskan
        // seseorang.
        await using var db = Konteks();
        var master = await SeedMasterAsync(db);

        await Service(db, DokterPengirim).CreateAsync(Permintaan(master, cito: false));

        var tersimpan = await db.RadOrders.AsNoTracking().SingleAsync();

        Assert.False(tersimpan.IsUrgent);
        Assert.Null(tersimpan.UrgentMarkedByUserId);
        Assert.Null(tersimpan.UrgentMarkedAt);
    }

    [Fact]
    public async Task PesananLamaTerbacaSebagaiTidakCito()
    {
        // Baris yang ditulis sebelum kolom ini lahir — disimulasikan dengan menyimpan pesanan
        // tanpa menyentuh penandanya sama sekali.
        await using var db = Konteks();
        var master = await SeedMasterAsync(db);

        db.RadOrders.Add(new RadOrder
        {
            Id = Guid.NewGuid(),
            EncounterId = Guid.NewGuid(),
            ProcedureId = master.ProcedureId,
            ModalityId = master.ModalityId,
            OrderStatus = RadOrderStatus.Requested,
            CreateDateTime = DateTime.UtcNow,
        });

        await db.SaveChangesAsync();

        var tersimpan = await db.RadOrders.AsNoTracking().SingleAsync();

        Assert.False(tersimpan.IsUrgent);
        Assert.Null(tersimpan.UrgentMarkedByUserId);
        Assert.Null(tersimpan.UrgentMarkedAt);
    }

    /* ================================================================== *
     * AC-42 dan FR-RAD-064 — penanda cito tercatat pelakunya
     * ================================================================== */

    [Fact]
    public async Task AC42_PenandaCitoTercatatSiapaYangMenetapkannyaDanKapan()
    {
        // dr. Andi menandai pesanan cito. Sistem menyimpan nama dr. Andi dan waktunya.
        await using var db = Konteks();
        var master = await SeedMasterAsync(db);
        var sebelum = DateTime.UtcNow;

        var hasil = await Service(db, DokterPengirim).CreateAsync(Permintaan(master, cito: true));

        Assert.Equal(RadOperationResultKind.Success, hasil.Kind);
        Assert.True(hasil.Value!.IsUrgent);

        var tersimpan = await db.RadOrders.AsNoTracking().SingleAsync();

        Assert.True(tersimpan.IsUrgent);
        Assert.Equal(DokterPengirim, tersimpan.UrgentMarkedByUserId);
        Assert.NotNull(tersimpan.UrgentMarkedAt);
        Assert.InRange(tersimpan.UrgentMarkedAt!.Value, sebelum, DateTime.UtcNow);
    }

    [Fact]
    public async Task AC42_JejakPenandaanTerbacaPadaRincianPesanan()
    {
        await using var db = Konteks();
        var master = await SeedMasterAsync(db);

        var dibuat = await Service(db, DokterPengirim).CreateAsync(Permintaan(master, cito: true));

        var rincian = await Service(db, DokterPengirim).GetDetailAsync(dibuat.Value!.Id);

        Assert.NotNull(rincian);
        Assert.True(rincian!.IsUrgent);
        Assert.Equal(DokterPengirim, rincian.UrgentMarkedByUserId);
        Assert.NotNull(rincian.UrgentMarkedAt);
    }

    [Fact]
    public async Task PenandaCitoTerbacaPadaDaftarPesanan()
    {
        // Kriteria 40: penanda cito terlihat tanpa perlu membuka rincian pesanan. Yang
        // dibuktikan backend adalah field-nya ikut terbawa daftar; penyajiannya milik frontend.
        await using var db = Konteks();
        var master = await SeedMasterAsync(db);
        var kunjungan = Guid.NewGuid();

        var service = Service(db, DokterPengirim);
        await service.CreateAsync(Permintaan(master, cito: true, encounterId: kunjungan));
        await service.CreateAsync(Permintaan(master, cito: false, encounterId: kunjungan));

        var daftar = await service.GetListAsync(encounterId: kunjungan);

        Assert.Equal(2, daftar.Count);
        Assert.Single(daftar, x => x.IsUrgent);
        Assert.Single(daftar, x => !x.IsUrgent);
    }

    /* ================================================================== *
     * Bentuk index — menopang daftar kerja BE-RAD-13
     * ================================================================== */

    [Fact]
    public void IndexDaftarKerjaDideklarasikanDenganUrutanYangBenar()
    {
        // Urutan kolomnya menentukan. Daftar kerja bertanya "pekerjaan pada alat ini", lalu
        // "yang cito lebih dulu", lalu "yang statusnya masih berjalan". Index dengan urutan lain
        // memaksa database memindai seluruh pesanan alat tersebut.
        using var db = Konteks();

        var index = db.Model
            .FindEntityType(typeof(RadOrder))!
            .GetIndexes()
            .SingleOrDefault(x =>
                x.Properties.Count == 3 &&
                x.Properties[0].Name == nameof(RadOrder.ModalityId) &&
                x.Properties[1].Name == nameof(RadOrder.IsUrgent) &&
                x.Properties[2].Name == nameof(RadOrder.OrderStatus));

        Assert.NotNull(index);
        Assert.False(index!.IsUnique);
    }

    [Fact]
    public void IndexYangSudahAdaTidakHilang()
    {
        // Penambahan index baru adalah saat yang paling mudah untuk tanpa sengaja menghapus
        // index lama — dan kehilangan index tidak menghasilkan galat, hanya query yang pelan.
        using var db = Konteks();

        var index = db.Model.FindEntityType(typeof(RadOrder))!.GetIndexes().ToList();

        Assert.Contains(index, x =>
            x.Properties.Count == 1 &&
            x.Properties[0].Name == nameof(RadOrder.EncounterId));

        Assert.Contains(index, x =>
            x.Properties.Count == 1 &&
            x.Properties[0].Name == nameof(RadOrder.OrderStatus));

        Assert.Contains(index, x =>
            x.Properties.Count == 2 &&
            x.Properties[0].Name == nameof(RadOrder.ModalityId) &&
            x.Properties[1].Name == nameof(RadOrder.OrderStatus));

        Assert.Contains(index, x =>
            x.Properties.Count == 2 &&
            x.Properties[0].Name == nameof(RadOrder.InpEpisodeId) &&
            x.Properties[1].Name == nameof(RadOrder.CreateDateTime));
    }

    [Fact]
    public void KetigaKolomBaruTerpetakanDenganKewajibanYangBenar()
    {
        using var db = Konteks();
        var entity = db.Model.FindEntityType(typeof(RadOrder))!;

        var penanda = entity.FindProperty(nameof(RadOrder.IsUrgent))!;
        var oleh = entity.FindProperty(nameof(RadOrder.UrgentMarkedByUserId))!;
        var kapan = entity.FindProperty(nameof(RadOrder.UrgentMarkedAt))!;

        // Penandanya wajib dan tidak pernah kosong; jejaknya boleh kosong karena pesanan yang
        // tidak cito memang tidak pernah ditandai siapa pun.
        Assert.False(penanda.IsNullable);
        Assert.True(oleh.IsNullable);
        Assert.True(kapan.IsNullable);
    }

    /* ================================================================== *
     * Tidak ada yang rusak pada tabel yang sudah dipakai modul lain
     * ================================================================== */

    [Fact]
    public void KolomLamaRadOrderTidakAdaYangHilang()
    {
        using var db = Konteks();

        var kolom = db.Model
            .FindEntityType(typeof(RadOrder))!
            .GetProperties()
            .Select(x => x.Name)
            .ToList();

        string[] wajibMasihAda =
        [
            nameof(RadOrder.EncounterId),
            nameof(RadOrder.ProcedureId),
            nameof(RadOrder.ModalityId),
            nameof(RadOrder.InpEpisodeId),
            nameof(RadOrder.OrderStatus),
            nameof(RadOrder.StatusBeforeHold),
            nameof(RadOrder.ClinicalIndication),
            nameof(RadOrder.RequestedAt),
            nameof(RadOrder.RequestedByUserId),
            nameof(RadOrder.ScheduledAt),
            nameof(RadOrder.CompletedAt),
            nameof(RadOrder.ClosureReason),
            nameof(RadOrder.Version),
        ];

        Assert.All(wajibMasihAda, x => Assert.Contains(x, kolom));
    }

    [Fact]
    public void PenandaCitoTidakDitambahkanKeStudy()
    {
        // Kriteria 41 berbunyi "penanda cito ikut terbawa ke seluruh study yang lahir dari
        // pesanan itu". Itu DITURUNKAN dari pesanannya, bukan disalin ke kolom baru pada study.
        // Menyalinnya akan menghasilkan dua sumber kebenaran yang dapat berselisih begitu
        // penanda pesanannya diubah.
        using var db = Konteks();

        var kolom = db.Model
            .FindEntityType(typeof(RadStudy))!
            .GetProperties()
            .Select(x => x.Name)
            .ToList();

        Assert.DoesNotContain(nameof(RadOrder.IsUrgent), kolom);
        Assert.DoesNotContain(nameof(RadOrder.UrgentMarkedByUserId), kolom);
    }

    /* ================================================================== *
     * Isi migration — dibaca dari berkasnya sendiri
     * ================================================================== */

    [Fact]
    public void MigrationHanyaMenyentuhTabelRadOrder()
    {
        // Migration yang diam-diam menyentuh tabel lain adalah cara paling cepat merusak modul
        // yang tidak ada hubungannya dengan task ini. Diperiksa dari berkasnya, bukan dari
        // ingatan penulisnya.
        var isi = IsiMigration();

        var tabel = System.Text.RegularExpressions.Regex
            .Matches(isi, "table: \"(?<nama>[A-Za-z]+)\"")
            .Select(x => x.Groups["nama"].Value)
            .Distinct()
            .ToList();

        Assert.Equal(new[] { nameof(RadOrder) }, tabel);
    }

    [Fact]
    public void MigrationMenambahTigaKolomDenganPenandaBawaanTidakCito()
    {
        // Bawaan false inilah yang membuat seluruh pesanan lama menjadi tidak-cito begitu
        // migration dijalankan. Kalau kolomnya dibuat wajib tanpa bawaan, migration akan gagal
        // pada tabel yang sudah berisi data.
        var isi = IsiMigration();

        Assert.Contains("name: \"IsUrgent\"", isi);
        Assert.Contains("name: \"UrgentMarkedAt\"", isi);
        Assert.Contains("name: \"UrgentMarkedByUserId\"", isi);
        Assert.Contains("defaultValue: false", isi);

        // Jejak penandaan boleh kosong; penandanya sendiri tidak.
        Assert.Contains("nullable: false", isi);
        Assert.Contains("nullable: true", isi);
    }

    [Fact]
    public void MigrationDapatDimundurkanDenganUrutanYangBenar()
    {
        // Index dijatuhkan lebih dulu, baru kolomnya. Urutan sebaliknya gagal, karena index
        // masih menunjuk kolom yang hendak dihapus.
        var isi = IsiMigration();
        var down = isi[isi.IndexOf("void Down(", StringComparison.Ordinal)..];

        var posisiDropIndex = down.IndexOf("DropIndex", StringComparison.Ordinal);
        var posisiDropKolom = down.IndexOf("DropColumn", StringComparison.Ordinal);

        Assert.True(posisiDropIndex >= 0, "Down tidak menjatuhkan index.");
        Assert.True(posisiDropKolom >= 0, "Down tidak menjatuhkan kolom.");
        Assert.True(
            posisiDropIndex < posisiDropKolom,
            "Index harus dijatuhkan sebelum kolom yang ditunjuknya.");

        // Ketiga kolom dikembalikan, bukan sebagian.
        Assert.Equal(3, System.Text.RegularExpressions.Regex.Matches(down, "DropColumn").Count);
    }

    /* ================================================================== *
     * Perancah
     * ================================================================== */

    private const string NamaBerkasMigration = "20260911045053_AddRadOrderUrgency.cs";

    private static string IsiMigration()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);

        while (dir != null &&
               !File.Exists(Path.Combine(dir.FullName, "QuilvianSystemBackend.csproj")))
        {
            dir = dir.Parent;
        }

        Assert.NotNull(dir);

        var jalur = Path.Combine(dir!.FullName, "Migrations", NamaBerkasMigration);

        Assert.True(File.Exists(jalur), $"Migration {NamaBerkasMigration} tidak ditemukan.");

        return File.ReadAllText(jalur);
    }

    private static ApplicationDbContext Konteks()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"rad-order-urgency-{Guid.NewGuid():N}")
            .Options;

        return new ApplicationDbContext(options);
    }

    private static RadOrderService Service(ApplicationDbContext db, Guid actorUserId)
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, actorUserId.ToString()) },
            "Test"));

        var accessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext { User = principal },
        };

        return new RadOrderService(
            db,
            accessor,
            new LoggerService(NullLogger<LoggerService>.Instance, accessor));
    }

    /// <summary>
    /// Alat dan pemeriksaan yang benar-benar ada. Relasi keduanya wajib pada <c>RadOrder</c>,
    /// dan proyeksi daftar menggabungkannya sebagai inner join — pesanan yang menunjuk baris
    /// master yang tidak ada akan hilang dari daftar.
    /// </summary>
    private static async Task<(Guid ProcedureId, Guid ModalityId)> SeedMasterAsync(
        ApplicationDbContext db)
    {
        var pemeriksaan = new MstProcedure
        {
            Id = Guid.NewGuid(),
            ProcedureCode = "RD-CITO",
            ProcedureName = "CT Kepala Uji",
            ProcedureType = "Radiology",
            IsActive = true,
        };

        var alat = new MstRadModality
        {
            Id = Guid.NewGuid(),
            ModalityCode = "CT-CITO",
            ModalityName = "CT-Scan Uji",
            IsActive = true,
        };

        db.Add(pemeriksaan);
        db.MstRadModalities.Add(alat);
        await db.SaveChangesAsync();

        return (pemeriksaan.Id, alat.Id);
    }

    private static CreateRadOrderRequest Permintaan(
        (Guid ProcedureId, Guid ModalityId) master,
        bool cito,
        Guid? encounterId = null) => new()
        {
            EncounterId = encounterId ?? Guid.NewGuid(),
            ProcedureId = master.ProcedureId,
            ModalityId = master.ModalityId,
            IsUrgent = cito,
        };
}
