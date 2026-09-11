using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Services;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Reflection;
using System.Security.Claims;

namespace QuilvianSystemBackend.Tests.RadiologyManagement;

/// <summary>
/// Daftar kerja petugas per alat dan pengubahan penanda cito — <c>BE-RAD-13</c>,
/// acceptance criteria AC-36, AC-37, AC-39, dan AC-41; <c>FR-RAD-060</c> sampai
/// <c>FR-RAD-062</c>; <c>RAD-DEC-012</c> dan <c>RAD-DEC-013</c>.
///
/// <para>
/// <b>Janji terpenting slice ini adalah sesuatu yang tidak dibuat: tidak ada tabel daftar
/// kerja.</b> Daftar kerja dihitung dari pesanan dan study yang sudah ada. Sebuah tabel daftar
/// kerja akan memerlukan penyelarasan, dan penyelarasan yang tertinggal menghasilkan layar yang
/// menampilkan pekerjaan yang sudah dibatalkan — atau menyembunyikan pekerjaan yang benar-benar
/// menunggu.
/// </para>
/// </summary>
public sealed class RadWorklistTests
{
    private static readonly Guid Petugas = Guid.Parse("bbbbbbbb-2222-2222-2222-222222222222");
    private static readonly Guid DokterLain = Guid.Parse("cccccccc-3333-3333-3333-333333333333");

    /* ================================================================== *
     * AC-36 — hanya alat yang dipilih
     * ================================================================== */

    [Fact]
    public async Task AC36_DaftarKerjaHanyaMemuatPemeriksaanPadaAlatYangDipilih()
    {
        // Radiografer Tono bertugas di ruang CT-Scan. Pemeriksaan MRI dan USG hari itu tidak
        // muncul di layarnya.
        await using var db = Konteks();
        var ct = await SeedAlatAsync(db, "CT-13", "CT-Scan");
        var mri = await SeedAlatAsync(db, "MRI-13", "MRI");

        await SeedPesananAsync(db, ct);
        await SeedPesananAsync(db, ct);
        await SeedPesananAsync(db, mri);

        var hasil = await Service(db).GetWorklistAsync(ct.ModalityId, null, null);

        Assert.Equal(RadOperationResultKind.Success, hasil.Kind);
        Assert.Equal(2, hasil.Value!.Count);
        Assert.All(hasil.Value, x => Assert.Equal(ct.ModalityId, x.ModalityId));
    }

    [Fact]
    public async Task DaftarKerjaTanpaAlatDitolak()
    {
        // Daftar kerja tanpa alat bukan daftar kerja siapa pun — RAD-DEC-012.
        await using var db = Konteks();

        var hasil = await Service(db).GetWorklistAsync(null, null, null);

        Assert.Equal(RadOperationResultKind.Validation, hasil.Kind);
        Assert.Equal(RadErrorCodes.WorklistModalityRequired, hasil.ErrorCode);
        Assert.Equal(
            "Alat pencitraan wajib dipilih untuk membuka daftar kerja.",
            hasil.ErrorMessage);
    }

    [Fact]
    public async Task DaftarKerjaDenganAlatKosongJugaDitolak()
    {
        await using var db = Konteks();

        var hasil = await Service(db).GetWorklistAsync(Guid.Empty, null, null);

        Assert.Equal(RadOperationResultKind.Validation, hasil.Kind);
        Assert.Equal(RadErrorCodes.WorklistModalityRequired, hasil.ErrorCode);
    }

    [Fact]
    public async Task DaftarKerjaDapatDisaringPerKeadaanPesanan()
    {
        await using var db = Konteks();
        var ct = await SeedAlatAsync(db, "CT-13", "CT-Scan");

        await SeedPesananAsync(db, ct, status: RadOrderStatus.Requested);
        await SeedPesananAsync(db, ct, status: RadOrderStatus.InProgress);
        await SeedPesananAsync(db, ct, status: RadOrderStatus.Completed);

        var seluruhnya = await Service(db).GetWorklistAsync(ct.ModalityId, null, null);
        var berjalan = await Service(db)
            .GetWorklistAsync(ct.ModalityId, null, RadOrderStatus.InProgress);

        // Tanpa penyaring, pesanan yang sudah selesai tetap muncul — RAD-DEC-012 menyebut
        // "tujuh pemeriksaan: dua sudah selesai, satu sedang berjalan, empat menunggu".
        Assert.Equal(3, seluruhnya.Value!.Count);
        Assert.Equal(RadOrderStatus.InProgress.ToString(), Assert.Single(berjalan.Value!).OrderStatus);
    }

    /* ================================================================== *
     * AC-39 dan FR-RAD-062 — pesanan cito di urutan atas
     * ================================================================== */

    [Fact]
    public async Task AC39_SatuPesananCitoBeradaDiUrutanPertamaMendahuluiYangLebihTua()
    {
        // Empat pesanan rawat jalan pukul 08.00 sampai 10.00, dan satu pesanan cito dari IGD
        // pukul 10.30. Yang cito berada di urutan pertama.
        await using var db = Konteks();
        var ct = await SeedAlatAsync(db, "CT-13", "CT-Scan");

        await SeedPesananAsync(db, ct, pada: Pukul(8));
        await SeedPesananAsync(db, ct, pada: Pukul(9));
        await SeedPesananAsync(db, ct, pada: Pukul(9.5));
        await SeedPesananAsync(db, ct, pada: Pukul(10));
        var cito = await SeedPesananAsync(db, ct, pada: Pukul(10.5), cito: true);

        var hasil = await Service(db).GetWorklistAsync(ct.ModalityId, HariIni, null);

        Assert.Equal(5, hasil.Value!.Count);
        Assert.Equal(cito, hasil.Value[0].RadOrderId);
        Assert.True(hasil.Value[0].IsUrgent);
        Assert.All(hasil.Value.Skip(1), x => Assert.False(x.IsUrgent));
    }

    [Fact]
    public async Task AC39_DuaPesananCitoKeduanyaDiAtasDanDiurutkanMenurutWaktu()
    {
        await using var db = Konteks();
        var ct = await SeedAlatAsync(db, "CT-13", "CT-Scan");

        await SeedPesananAsync(db, ct, pada: Pukul(7));
        var citoSiang = await SeedPesananAsync(db, ct, pada: Pukul(13), cito: true);
        var citoPagi = await SeedPesananAsync(db, ct, pada: Pukul(9), cito: true);

        var hasil = await Service(db).GetWorklistAsync(ct.ModalityId, HariIni, null);

        // Keduanya di atas, dan di antara sesama cito yang lebih dulu dipesan dikerjakan lebih
        // dulu — bukan yang terakhir ditandai.
        Assert.Equal(citoPagi, hasil.Value![0].RadOrderId);
        Assert.Equal(citoSiang, hasil.Value[1].RadOrderId);
        Assert.False(hasil.Value[2].IsUrgent);
    }

    /* ================================================================== *
     * AC-41 — penanda cito terbaca pada baris study
     * ================================================================== */

    [Fact]
    public async Task AC41_PenandaCitoIkutTerbacaPadaBarisStudy()
    {
        await using var db = Konteks();
        var ct = await SeedAlatAsync(db, "CT-13", "CT-Scan");
        var pesanan = await SeedPesananAsync(db, ct, cito: true);

        await SeedStudyAsync(db, pesanan, ct, urutan: 1);
        await SeedStudyAsync(db, pesanan, ct, urutan: 2);

        var baris = Assert.Single((await Service(db)
            .GetWorklistAsync(ct.ModalityId, null, null)).Value!);

        Assert.Equal(2, baris.Studies.Count);
        Assert.All(baris.Studies, x => Assert.True(x.IsUrgent));
    }

    [Fact]
    public async Task AC41_PenandaStudyIkutBerubahKetikaPenandaPesanannyaDicabut()
    {
        // Inilah yang membuat penurunan lebih baik daripada penyalinan. Kalau penandanya
        // disalin ke kolom pada RadStudy, baris study akan tetap terbaca cito setelah penanda
        // pesanannya dicabut — dan daftar kerja akan mendahulukan pekerjaan yang sudah tidak
        // mendesak lagi.
        await using var db = Konteks();
        var ct = await SeedAlatAsync(db, "CT-13", "CT-Scan");
        var pesanan = await SeedPesananAsync(db, ct, cito: true);
        await SeedStudyAsync(db, pesanan, ct, urutan: 1);

        await Service(db).SetUrgencyAsync(pesanan, new RadOrderUrgencyRequest { IsUrgent = false });

        var baris = Assert.Single((await Service(db)
            .GetWorklistAsync(ct.ModalityId, null, null)).Value!);

        Assert.False(baris.IsUrgent);
        Assert.False(Assert.Single(baris.Studies).IsUrgent);
    }

    [Fact]
    public async Task PesananTanpaStudyTetapMuncul()
    {
        // Pesanan yang belum direncanakan sama sekali justru yang paling perlu dikerjakan.
        await using var db = Konteks();
        var ct = await SeedAlatAsync(db, "CT-13", "CT-Scan");
        await SeedPesananAsync(db, ct);

        var baris = Assert.Single((await Service(db)
            .GetWorklistAsync(ct.ModalityId, null, null)).Value!);

        Assert.Empty(baris.Studies);
    }

    /* ================================================================== *
     * FR-RAD-061 — daftar kerja tidak menyimpan apa pun
     * ================================================================== */

    [Fact]
    public async Task FR061_PesananYangDibatalkanLangsungHilangTanpaPenyelarasan()
    {
        await using var db = Konteks();
        var ct = await SeedAlatAsync(db, "CT-13", "CT-Scan");
        var pesanan = await SeedPesananAsync(db, ct);

        Assert.Single((await Service(db).GetWorklistAsync(ct.ModalityId, null, null)).Value!);

        // Ditandai terhapus langsung pada tabelnya, tanpa memanggil apa pun yang "menyegarkan"
        // daftar kerja — karena memang tidak ada yang perlu disegarkan.
        var baris = await db.RadOrders.SingleAsync(x => x.Id == pesanan);
        baris.IsDelete = true;
        await db.SaveChangesAsync();

        Assert.Empty((await Service(db).GetWorklistAsync(ct.ModalityId, null, null)).Value!);
    }

    /* ================================================================== *
     * Hari kerja dihitung menurut Waktu Indonesia Barat
     * ================================================================== */

    [Fact]
    public async Task PekerjaanShiftPagiMasukKeHariKerjaYangBenar()
    {
        // Pukul 06.30 WIB berjalan pada pukul 23.30 UTC HARI SEBELUMNYA. Memakai tanggal UTC
        // akan menyembunyikan pekerjaan ini dari petugas yang baru masuk shift pagi.
        await using var db = Konteks();
        var ct = await SeedAlatAsync(db, "CT-13", "CT-Scan");

        var hari = new DateTime(2026, 9, 10, 0, 0, 0, DateTimeKind.Unspecified);
        var pagiWib = new DateTime(2026, 9, 9, 23, 30, 0, DateTimeKind.Utc); // 06.30 WIB, 10 Sep

        var pesanan = await SeedPesananAsync(db, ct, pada: pagiWib);

        var hasil = await Service(db).GetWorklistAsync(ct.ModalityId, hari, null);

        Assert.Equal(pesanan, Assert.Single(hasil.Value!).RadOrderId);
    }

    [Fact]
    public async Task PekerjaanHariLainTidakIkutTerbawa()
    {
        await using var db = Konteks();
        var ct = await SeedAlatAsync(db, "CT-13", "CT-Scan");

        var hari = new DateTime(2026, 9, 10, 0, 0, 0, DateTimeKind.Unspecified);

        await SeedPesananAsync(db, ct, pada: new DateTime(2026, 9, 10, 3, 0, 0, DateTimeKind.Utc));
        await SeedPesananAsync(db, ct, pada: new DateTime(2026, 9, 11, 3, 0, 0, DateTimeKind.Utc));

        var hasil = await Service(db).GetWorklistAsync(ct.ModalityId, hari, null);

        Assert.Single(hasil.Value!);
    }

    [Fact]
    public async Task JadwalMendahuluiWaktuPemesananDalamMenentukanHariKerja()
    {
        // Pesanan dibuat kemarin untuk dikerjakan hari ini. Ia milik daftar kerja hari ini.
        await using var db = Konteks();
        var ct = await SeedAlatAsync(db, "CT-13", "CT-Scan");

        var hari = new DateTime(2026, 9, 10, 0, 0, 0, DateTimeKind.Unspecified);

        var pesanan = new RadOrder
        {
            Id = Guid.NewGuid(),
            EncounterId = Guid.NewGuid(),
            ProcedureId = ct.ProcedureId,
            ModalityId = ct.ModalityId,
            OrderStatus = RadOrderStatus.Scheduled,
            RequestedAt = new DateTime(2026, 9, 9, 3, 0, 0, DateTimeKind.Utc),
            ScheduledAt = new DateTime(2026, 9, 10, 3, 0, 0, DateTimeKind.Utc),
            CreateDateTime = new DateTime(2026, 9, 9, 3, 0, 0, DateTimeKind.Utc),
        };

        db.RadOrders.Add(pesanan);
        await db.SaveChangesAsync();

        var hasil = await Service(db).GetWorklistAsync(ct.ModalityId, hari, null);

        Assert.Equal(pesanan.Id, Assert.Single(hasil.Value!).RadOrderId);
        Assert.Equal(pesanan.ScheduledAt, hasil.Value![0].WorkAt);
    }

    /* ================================================================== *
     * PUT /{id}/urgency
     * ================================================================== */

    [Fact]
    public async Task PenandaCitoDapatDipasangSetelahPesananDibuat()
    {
        await using var db = Konteks();
        var ct = await SeedAlatAsync(db, "CT-13", "CT-Scan");
        var pesanan = await SeedPesananAsync(db, ct);

        var hasil = await Service(db, DokterLain)
            .SetUrgencyAsync(pesanan, new RadOrderUrgencyRequest { IsUrgent = true });

        Assert.Equal(RadOperationResultKind.Success, hasil.Kind);
        Assert.True(hasil.Value!.IsUrgent);
        Assert.Equal(DokterLain, hasil.Value.UrgentMarkedByUserId);
        Assert.NotNull(hasil.Value.UrgentMarkedAt);
    }

    [Fact]
    public async Task MencabutPenandaCitoMengosongkanJejakPadaKolomnya()
    {
        await using var db = Konteks();
        var ct = await SeedAlatAsync(db, "CT-13", "CT-Scan");
        var pesanan = await SeedPesananAsync(db, ct, cito: true);

        var hasil = await Service(db, DokterLain)
            .SetUrgencyAsync(pesanan, new RadOrderUrgencyRequest { IsUrgent = false });

        Assert.False(hasil.Value!.IsUrgent);
        Assert.Null(hasil.Value.UrgentMarkedByUserId);
        Assert.Null(hasil.Value.UrgentMarkedAt);
    }

    [Fact]
    public async Task SetiapPerubahanPenandaCitoMasukRiwayatBesertaPelakunya()
    {
        // Kolom hanya menyimpan keadaan sekarang. Pertanyaan "siapa yang MENCABUT penanda cito
        // pasien ini" sama pentingnya dengan "siapa yang memasangnya", dan hanya riwayat yang
        // dapat menjawabnya.
        await using var db = Konteks();
        var ct = await SeedAlatAsync(db, "CT-13", "CT-Scan");
        var pesanan = await SeedPesananAsync(db, ct);

        await Service(db, Petugas).SetUrgencyAsync(pesanan, new RadOrderUrgencyRequest { IsUrgent = true });
        await Service(db, DokterLain).SetUrgencyAsync(pesanan, new RadOrderUrgencyRequest { IsUrgent = false });

        var riwayat = await db.RadTransitionHistories
            .AsNoTracking()
            .Where(x => x.Action == "Order.Urgency")
            .OrderBy(x => x.OccurredAt)
            .ToListAsync();

        Assert.Equal(2, riwayat.Count);

        Assert.Equal("NotUrgent", riwayat[0].FromStatus);
        Assert.Equal("Urgent", riwayat[0].ToStatus);
        Assert.Equal(Petugas, riwayat[0].ActorUserId);

        Assert.Equal("Urgent", riwayat[1].FromStatus);
        Assert.Equal("NotUrgent", riwayat[1].ToStatus);
        Assert.Equal(DokterLain, riwayat[1].ActorUserId);
    }

    [Fact]
    public async Task MenetapkanPenandaKeNilaiYangSamaTidakMeninggalkanJejakBaru()
    {
        await using var db = Konteks();
        var ct = await SeedAlatAsync(db, "CT-13", "CT-Scan");
        var pesanan = await SeedPesananAsync(db, ct, cito: true);

        var hasil = await Service(db, DokterLain)
            .SetUrgencyAsync(pesanan, new RadOrderUrgencyRequest { IsUrgent = true });

        Assert.Equal(RadOperationResultKind.Success, hasil.Kind);
        Assert.Empty(await db.RadTransitionHistories
            .Where(x => x.Action == "Order.Urgency")
            .ToListAsync());
    }

    [Theory]
    [InlineData(RadOrderStatus.Completed)]
    [InlineData(RadOrderStatus.Cancelled)]
    [InlineData(RadOrderStatus.Rejected)]
    public async Task PenandaCitoPesananYangSudahSelesaiTidakDapatDiubah(RadOrderStatus status)
    {
        await using var db = Konteks();
        var ct = await SeedAlatAsync(db, "CT-13", "CT-Scan");
        var pesanan = await SeedPesananAsync(db, ct, status: status);

        var hasil = await Service(db, DokterLain)
            .SetUrgencyAsync(pesanan, new RadOrderUrgencyRequest { IsUrgent = true });

        Assert.Equal(RadOperationResultKind.Conflict, hasil.Kind);
        Assert.Equal(RadErrorCodes.UrgencyNotChangeable, hasil.ErrorCode);

        var tersimpan = await db.RadOrders.AsNoTracking().SingleAsync();
        Assert.False(tersimpan.IsUrgent);
    }

    [Fact]
    public async Task PenandaCitoPesananYangTidakAdaDitolak()
    {
        await using var db = Konteks();

        var hasil = await Service(db, DokterLain)
            .SetUrgencyAsync(Guid.NewGuid(), new RadOrderUrgencyRequest { IsUrgent = true });

        Assert.Equal(RadOperationResultKind.NotFound, hasil.Kind);
        Assert.Equal(RadErrorCodes.OrderNotFound, hasil.ErrorCode);
    }

    /* ================================================================== *
     * AC-37 — uji arsitektur: tidak ada tabel daftar kerja
     * ================================================================== */

    [Fact]
    public void AC37_TidakAdaTabelDaftarKerjaPadaModelManaPun()
    {
        // Bentuk pelanggaran yang paling mungkin: seseorang menambahkan tabel RadWorklist
        // "supaya layarnya cepat". Sejak saat itu daftar kerja perlu diselaraskan, dan
        // penyelarasan yang tertinggal menampilkan pekerjaan yang sudah dibatalkan.
        var pelanggar = typeof(ApplicationDbContext).Assembly
            .GetTypes()
            .Where(x => x.IsClass && !x.IsAbstract)
            .Where(x => typeof(IdentityModel).IsAssignableFrom(x))
            .Where(x =>
                x.Name.Contains("Worklist", StringComparison.OrdinalIgnoreCase) ||
                x.Name.Contains("WorkList", StringComparison.Ordinal))
            .Select(x => x.FullName!)
            .ToList();

        Assert.True(
            pelanggar.Count == 0,
            "Ada entity bertema daftar kerja:\n" + string.Join("\n", pelanggar));
    }

    [Fact]
    public void AC37_DaftarKerjaHanyaMenyentuhRadOrderDanRadStudy()
    {
        // Definition of Done BE-RAD-13, dibaca dari source methodnya sendiri.
        var source = SourceRadOrderService();
        var awal = source.IndexOf("GetWorklistAsync", StringComparison.Ordinal);

        Assert.True(awal > 0, "Method GetWorklistAsync tidak ditemukan pada source.");

        var akhir = source.IndexOf("SetUrgencyAsync", StringComparison.Ordinal);
        Assert.True(akhir > awal, "Batas akhir method tidak ditemukan.");

        var isi = source[awal..akhir];

        // Satu-satunya DbSet yang boleh disentuh.
        var dbSet = System.Text.RegularExpressions.Regex
            .Matches(isi, @"_dbContext\.(?<nama>[A-Za-z]+)")
            .Select(x => x.Groups["nama"].Value)
            .Distinct()
            .ToList();

        Assert.Equal(new[] { "RadOrders" }, dbSet);

        // Study dicapai lewat navigasi milik pesanannya, bukan lewat tabel lain.
        Assert.Contains("x.Studies", isi);
    }

    [Fact]
    public void EndpointDaftarKerjaDanPenandaCitoSesuaiKontrak()
    {
        var endpoint = typeof(RadOrderController)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(x => !x.IsSpecialName)
            .ToList();

        var worklist = Assert.Single(endpoint, x => x.Name == "GetWorklist");
        var urgency = Assert.Single(endpoint, x => x.Name == "SetUrgency");

        Assert.Equal(
            "worklist",
            worklist.GetCustomAttribute<Microsoft.AspNetCore.Mvc.HttpGetAttribute>()!.Template);

        Assert.Equal(
            "{id:guid}/urgency",
            urgency.GetCustomAttribute<Microsoft.AspNetCore.Mvc.HttpPutAttribute>()!.Template);

        Assert.Equal(("RadOrder", "Read"), HakAkses(worklist));
        Assert.Equal(("RadOrder", "Update"), HakAkses(urgency));
    }

    /* ================================================================== *
     * Perancah
     * ================================================================== */

    private static readonly DateTime HariIni =
        new(2026, 9, 10, 0, 0, 0, DateTimeKind.Unspecified);

    /// <summary>Pukul sekian Waktu Indonesia Barat pada <see cref="HariIni"/>, dalam UTC.</summary>
    private static DateTime Pukul(double jamWib) =>
        new DateTime(2026, 9, 10, 0, 0, 0, DateTimeKind.Utc)
            .AddHours(jamWib - 7);

    private static ApplicationDbContext Konteks()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"rad-worklist-{Guid.NewGuid():N}")
            .Options;

        return new ApplicationDbContext(options);
    }

    private static RadOrderService Service(ApplicationDbContext db, Guid? actorUserId = null)
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, (actorUserId ?? Petugas).ToString()) },
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

    private static async Task<(Guid ProcedureId, Guid ModalityId)> SeedAlatAsync(
        ApplicationDbContext db,
        string kodeAlat,
        string namaAlat)
    {
        var pemeriksaan = new MstProcedure
        {
            Id = Guid.NewGuid(),
            ProcedureCode = $"PR-{kodeAlat}",
            ProcedureName = $"Pemeriksaan {namaAlat}",
            ProcedureType = "Radiology",
            IsActive = true,
        };

        var alat = new MstRadModality
        {
            Id = Guid.NewGuid(),
            ModalityCode = kodeAlat,
            ModalityName = namaAlat,
            IsActive = true,
        };

        db.Add(pemeriksaan);
        db.MstRadModalities.Add(alat);
        await db.SaveChangesAsync();

        return (pemeriksaan.Id, alat.Id);
    }

    private static async Task<Guid> SeedPesananAsync(
        ApplicationDbContext db,
        (Guid ProcedureId, Guid ModalityId) alat,
        RadOrderStatus status = RadOrderStatus.Requested,
        bool cito = false,
        DateTime? pada = null)
    {
        var waktu = pada ?? DateTime.UtcNow;

        var pesanan = new RadOrder
        {
            Id = Guid.NewGuid(),
            EncounterId = Guid.NewGuid(),
            ProcedureId = alat.ProcedureId,
            ModalityId = alat.ModalityId,
            OrderStatus = status,
            RequestedAt = waktu,
            CreateDateTime = waktu,
            IsUrgent = cito,
            UrgentMarkedByUserId = cito ? Petugas : null,
            UrgentMarkedAt = cito ? waktu : null,
        };

        db.RadOrders.Add(pesanan);
        await db.SaveChangesAsync();

        return pesanan.Id;
    }

    private static async Task SeedStudyAsync(
        ApplicationDbContext db,
        Guid radOrderId,
        (Guid ProcedureId, Guid ModalityId) alat,
        int urutan)
    {
        db.RadStudies.Add(new RadStudy
        {
            Id = Guid.NewGuid(),
            RadOrderId = radOrderId,
            EncounterId = Guid.NewGuid(),
            ProcedureId = alat.ProcedureId,
            ModalityId = alat.ModalityId,
            StudySequence = urutan,
            StudyNumber = $"RAD-{radOrderId:N}-{urutan:D3}".ToUpperInvariant(),
            StudyStatus = RadStudyStatus.Planned,
        });

        await db.SaveChangesAsync();
    }

    private static (string Resource, string Action) HakAkses(MethodInfo endpoint)
    {
        var permission = endpoint
            .GetCustomAttributes()
            .Single(x => x.GetType().Name == "AccessPermissionAttribute");

        var arguments = (object[])permission
            .GetType()
            .GetProperty("Arguments")!
            .GetValue(permission)!;

        return ((string)arguments[0], (string)arguments[1]);
    }

    private static string SourceRadOrderService()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);

        while (dir != null &&
               !File.Exists(Path.Combine(dir.FullName, "QuilvianSystemBackend.csproj")))
        {
            dir = dir.Parent;
        }

        Assert.NotNull(dir);

        var jalur = Path.Combine(
            dir!.FullName,
            "Areas", "HealthServices", "RadiologyManagement", "Services", "RadOrderService.cs");

        Assert.True(File.Exists(jalur), "RadOrderService.cs tidak ditemukan.");

        return File.ReadAllText(jalur);
    }
}
