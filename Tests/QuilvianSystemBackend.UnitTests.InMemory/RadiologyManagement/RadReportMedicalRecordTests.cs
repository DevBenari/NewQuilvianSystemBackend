using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
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
/// Penyajian hasil bacaan ke rekam medis — <c>BE-RAD-11</c>, acceptance criteria AC-19 dan
/// AC-21, <c>FR-RAD-030</c> dan <c>FR-RAD-032</c>, <c>RAD-DEC-006</c>, <c>RAD-INT-001</c>
/// bagian 2.
///
/// <para>
/// <b>Dua janji dibuktikan di sini, dan keduanya mudah dilanggar tanpa disadari.</b>
/// </para>
///
/// <list type="number">
/// <item><b>Rekam medis membaca, tidak menyalin.</b> Salinan yang lupa diperbarui membuat dokter
/// membaca bacaan yang sudah diralat, lalu mengambil keputusan pengobatan atasnya. Dibuktikan
/// dengan uji arsitektur — bukan dengan janji di dokumen.</item>
/// <item><b>Bacaan yang belum dirilis tidak tampil sama sekali.</b> Draf belum menjadi
/// pernyataan siapa pun; menampilkannya kepada dokter pengirim membuka jalan yang tidak dapat
/// ditutup kembali.</item>
/// </list>
/// </summary>
public sealed class RadReportMedicalRecordTests
{
    private static readonly Guid Radiolog = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid Residen = Guid.Parse("33333333-3333-3333-3333-333333333333");

    private const string IsiVersiSatu = "Tidak tampak perdarahan intrakranial.";
    private const string IsiVersiDua = "Tampak perdarahan kecil pada lobus temporalis kanan.";

    /* ================================================================== *
     * FR-RAD-032 — bacaan yang belum dirilis tidak tampil
     * ================================================================== */

    [Fact]
    public async Task FR032_BacaanBerstatusDraftedTidakMuncul()
    {
        await using var db = Konteks();
        var (_, kunjungan) = await DrafAsync(db);

        var hasil = await Service(db, Radiolog, radiolog: true).GetByEncounterAsync(kunjungan);

        Assert.Empty(hasil);
    }

    [Fact]
    public async Task FR032_BacaanYangSudahDisahkanTetapiBelumDirilisTidakMuncul()
    {
        // Keadaan yang paling mudah terlewat. Bacaan ini sudah diperiksa dokter radiolog,
        // sehingga terasa "sudah jadi" — tetapi belum dirilis, dan perilisan itulah tindakan
        // yang menyerahkannya kepada dokter pengirim.
        await using var db = Konteks();
        var (laporan, kunjungan) = await DrafAsync(db);
        await Service(db, Radiolog, radiolog: true).ValidateAsync(laporan);

        var hasil = await Service(db, Radiolog, radiolog: true).GetByEncounterAsync(kunjungan);

        Assert.Empty(hasil);
    }

    [Fact]
    public async Task BacaanYangSudahDirilisMuncul()
    {
        await using var db = Konteks();
        var (laporan, kunjungan) = await DrafAsync(db);
        await Service(db, Radiolog, radiolog: true).ValidateAsync(laporan);
        await Service(db, Radiolog, radiolog: true).ReleaseAsync(laporan);

        var hasil = await Service(db, Radiolog, radiolog: true).GetByEncounterAsync(kunjungan);

        var baris = Assert.Single(hasil);
        Assert.Equal(laporan, baris.Id);
        Assert.Equal(nameof(RadReportStatus.Released), baris.ReportStatus);
        Assert.NotNull(baris.FirstReleasedAt);
    }

    [Fact]
    public async Task HanyaBacaanYangDirilisYangIkutTerbawaKetikaKunjunganPunyaKeduanya()
    {
        // Satu kunjungan, dua pemeriksaan: satu bacaannya sudah dirilis, satu masih draf.
        // Yang muncul hanya satu.
        await using var db = Konteks();
        var kunjungan = Guid.NewGuid();

        var dirilis = await BacaanPadaKunjunganAsync(db, kunjungan);
        await Service(db, Radiolog, radiolog: true).ValidateAsync(dirilis);
        await Service(db, Radiolog, radiolog: true).ReleaseAsync(dirilis);

        var masihDraf = await BacaanPadaKunjunganAsync(db, kunjungan);

        var hasil = await Service(db, Radiolog, radiolog: true).GetByEncounterAsync(kunjungan);

        var baris = Assert.Single(hasil);
        Assert.Equal(dirilis, baris.Id);
        Assert.DoesNotContain(hasil, x => x.Id == masihDraf);
    }

    [Fact]
    public async Task DaftarKerjaRadiologiTetapMelihatDrafYangSamaLewatJalurLain()
    {
        // Penyaringan hanya berlaku pada jalur pembaca hasil. Petugas Radiologi yang perlu
        // melihat draf satu kunjungan memakai GET /?encounterId=…, dan di sana drafnya tetap
        // terlihat — kalau tidak, pekerjaan yang belum selesai justru menghilang dari orang
        // yang harus menyelesaikannya.
        await using var db = Konteks();
        var (_, kunjungan) = await DrafAsync(db);

        var rekamMedis = await Service(db, Radiolog, radiolog: true)
            .GetByEncounterAsync(kunjungan);

        var daftarKerja = await Service(db, Radiolog, radiolog: true)
            .GetPagedAsync(new RadReportPagedQuery { EncounterId = kunjungan });

        Assert.Empty(rekamMedis);
        Assert.Single(daftarKerja.Items);
    }

    /* ================================================================== *
     * AC-19 — koreksi langsung terlihat, tanpa penyalinan
     * ================================================================== */

    [Fact]
    public async Task AC19_SetelahKoreksiDirilis_RekamMedisLangsungMelihatVersiTerbaru()
    {
        // dr. Andi membaca pukul 08.00, koreksi dirilis pukul 09.00, dr. Andi membuka lagi
        // pukul 10.00 dan melihat versi koreksi — tanpa satu pun langkah penyalinan di antaranya.
        await using var db = Konteks();
        var (laporan, kunjungan) = await BacaanDirilisAsync(db);

        var pukulDelapan = await Service(db, Radiolog, radiolog: true)
            .GetByEncounterAsync(kunjungan);

        Assert.Equal(1, Assert.Single(pukulDelapan).CurrentVersionNumber);

        await RilisKoreksiAsync(db, laporan);

        var pukulSepuluh = await Service(db, Radiolog, radiolog: true)
            .GetByEncounterAsync(kunjungan);

        var baris = Assert.Single(pukulSepuluh);
        Assert.Equal(2, baris.CurrentVersionNumber);
        Assert.Equal(nameof(RadReportStatus.AmendmentReleased), baris.ReportStatus);

        // Isi versi terbaru terbaca lewat rincian, yang membaca tabel yang sama.
        var rincian = await Service(db, Radiolog, radiolog: true).GetByIdAsync(laporan);
        Assert.Equal(IsiVersiDua, rincian.Value!.CurrentVersion!.Impression);
    }

    [Fact]
    public async Task AC19_SelamaKoreksiDisusun_RekamMedisMasihMelihatVersiRilisSebelumnya()
    {
        // Jendela waktu antara "koreksi mulai ditulis" dan "koreksi dirilis". Yang berlaku bagi
        // dokter pengirim tetap versi 1, karena versi 2 belum diperiksa siapa pun.
        await using var db = Konteks();
        var (laporan, kunjungan) = await BacaanDirilisAsync(db);

        await Service(db, Radiolog, radiolog: true).CreateAmendmentAsync(laporan, Koreksi());

        var hasil = await Service(db, Radiolog, radiolog: true).GetByEncounterAsync(kunjungan);

        var baris = Assert.Single(hasil);
        Assert.Equal(1, baris.CurrentVersionNumber);

        var rincian = await Service(db, Radiolog, radiolog: true).GetByIdAsync(laporan);
        Assert.Equal(IsiVersiSatu, rincian.Value!.CurrentVersion!.Impression);
    }

    [Fact]
    public async Task KunjunganTanpaBacaanDirilisMengembalikanDaftarKosong()
    {
        // Daftar kosong, bukan galat. Membedakan "belum ada hasilnya" dari "modul sedang
        // bermasalah" adalah tanggung jawab layar — FR-RAD-031, FE-RAD-13.
        await using var db = Konteks();

        var hasil = await Service(db, Radiolog, radiolog: true)
            .GetByEncounterAsync(Guid.NewGuid());

        Assert.Empty(hasil);
    }

    /* ================================================================== *
     * Uji arsitektur — Definition of Done BE-RAD-11
     * ================================================================== */

    [Fact]
    public void TidakAdaEntityDiLuarRadiologiYangMenyimpanRujukanHasilBacaan()
    {
        // Bentuk pelanggaran yang paling mungkin terjadi: sebuah modul menambahkan kolom
        // RadReportId beserta salinan kesimpulannya "supaya layarnya cepat". Sejak saat itu ada
        // dua sumber kebenaran, dan yang kedua tidak pernah tahu kapan bacaannya dikoreksi.
        var pelanggar = new List<string>();
        var diperiksa = EntitasDiLuarRadiologi().ToList();

        // Penjaga terhadap lulus semu.
        Assert.True(
            diperiksa.Count > 100,
            $"Hanya {diperiksa.Count} entity yang terpindai; penyaringnya kemungkinan salah.");

        foreach (var entity in diperiksa)
        {
            foreach (var property in entity.GetProperties())
            {
                var namaProperti = property.Name;
                var namaTipe = property.PropertyType.Name;

                if (namaProperti.Contains("RadReport", StringComparison.Ordinal) ||
                    namaTipe.Contains("RadReport", StringComparison.Ordinal))
                {
                    pelanggar.Add(
                        $"{entity.FullName}.{namaProperti} ({namaTipe}) menyimpan rujukan ke " +
                        "hasil bacaan radiologi di luar modul pemiliknya.");
                }
            }
        }

        Assert.True(pelanggar.Count == 0, string.Join("\n", pelanggar));
    }

    [Fact]
    public void TidakAdaModulLainYangMenyebutHasilBacaanRadiologiPadaSourcenya()
    {
        // Bentuk "pencarian tipe" yang diminta matriks uji penerimaan, dijalankan atas source
        // sungguhan. Pemeriksaan reflection saja tidak cukup: sebuah modul dapat menyalin isi
        // bacaan ke kolom bernama netral, dan yang menandainya adalah penyebutan nama tipenya
        // di dalam kode.
        var diperiksa = BerkasSourceDi("Areas")
            .Where(x => !x.Contains(
                Path.Combine("HealthServices", "RadiologyManagement"),
                StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Penjaga terhadap lulus semu. Uji pemindaian yang kehilangan jalur sourcenya akan
        // memeriksa nol berkas lalu melaporkan "tidak ada pelanggaran" selamanya — bentuk
        // kegagalan yang paling berbahaya, karena ia terlihat seperti keberhasilan.
        Assert.True(
            diperiksa.Count > 100,
            $"Hanya {diperiksa.Count} berkas yang terpindai; jalur sourcenya kemungkinan salah.");

        var pelanggar = diperiksa
            .Where(x => File.ReadAllText(x).Contains("RadReport", StringComparison.Ordinal))
            .Select(JalurPendek)
            .ToList();

        Assert.True(
            pelanggar.Count == 0,
            "Berkas di luar modul Radiologi menyebut hasil bacaan:\n" +
            string.Join("\n", pelanggar));
    }

    [Fact]
    public void AC21_AlurHasilBacaanTidakMenyentuhSlotDokumenRekamMedis()
    {
        // RAD-INT-001 bagian 2: slot PatientClinicalDocumentSource.Radiology tetap dipakai,
        // tetapi HANYA untuk berkas unggahan dari luar — misalnya hasil foto yang dibawa pasien
        // dari rumah sakit lain. Hasil bacaan yang lahir di modul ini tidak pernah mengisinya.
        //
        // Kalau alur internal ikut mengisinya, rekam medis akan memuat dua jenis baris yang
        // terlihat sama tetapi berperilaku berbeda: satu yang ikut berubah ketika bacaan
        // dikoreksi, satu yang tidak.
        var diperiksa = BerkasSourceDi(
            Path.Combine("Areas", "HealthServices", "RadiologyManagement")).ToList();

        // Penjaga terhadap lulus semu, sama seperti uji di atas.
        Assert.True(
            diperiksa.Count > 10,
            $"Hanya {diperiksa.Count} berkas modul Radiologi yang terpindai; jalurnya " +
            "kemungkinan salah.");

        var pelanggar = diperiksa
            .Where(x => File.ReadAllText(x)
                .Contains("PatientClinicalDocument", StringComparison.Ordinal))
            .Select(JalurPendek)
            .ToList();

        Assert.True(
            pelanggar.Count == 0,
            "Berkas modul Radiologi menyentuh slot dokumen rekam medis:\n" +
            string.Join("\n", pelanggar));
    }

    [Fact]
    public void ServiceHasilBacaanTidakBergantungPadaModulLainSelainInfrastrukturBersama()
    {
        // Ketergantungan adalah tanda paling awal sebuah modul mulai menulis ke wilayah modul
        // lain. Yang boleh dipegang service ini hanya infrastruktur bersama: DbContext, sesi,
        // logger, dan alokator nomor miliknya sendiri.
        var diizinkan = new[]
        {
            nameof(ApplicationDbContext),
            nameof(IHttpContextAccessor),
            "AccessPermissionService",
            nameof(LoggerService),
            nameof(RadReportNumberService),
        };

        var parameter = typeof(RadReportService)
            .GetConstructors()
            .Single()
            .GetParameters()
            .Select(x => x.ParameterType.Name)
            .ToList();

        Assert.All(parameter, x => Assert.Contains(x, diizinkan));
    }

    /* ================================================================== *
     * Perancah
     * ================================================================== */

    private static IEnumerable<Type> EntitasDiLuarRadiologi() =>
        typeof(ApplicationDbContext).Assembly
            .GetTypes()
            .Where(x => x.IsClass && !x.IsAbstract)
            .Where(x => typeof(IdentityModel).IsAssignableFrom(x))
            .Where(x => x.Namespace != null &&
                        !x.Namespace.Contains("RadiologyManagement", StringComparison.Ordinal));

    private static string AkarRepository()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);

        while (dir != null &&
               !File.Exists(Path.Combine(dir.FullName, "QuilvianSystemBackend.csproj")))
        {
            dir = dir.Parent;
        }

        Assert.NotNull(dir);

        return dir!.FullName;
    }

    private static IEnumerable<string> BerkasSourceDi(string jalurRelatif) =>
        Directory.EnumerateFiles(
            Path.Combine(AkarRepository(), jalurRelatif),
            "*.cs",
            SearchOption.AllDirectories);

    private static string JalurPendek(string jalur) =>
        Path.GetRelativePath(AkarRepository(), jalur);

    private static ApplicationDbContext Konteks()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"rad-report-mr-{Guid.NewGuid():N}")
            .Options;

        return new ApplicationDbContext(options);
    }

    private sealed class BacaanDenganKewenangan(
        ApplicationDbContext dbContext,
        IHttpContextAccessor httpContextAccessor,
        LoggerService loggerService,
        RadReportNumberService reportNumberService,
        bool radiolog)
        : RadReportService(dbContext, httpContextAccessor, null!, loggerService, reportNumberService)
    {
        protected override Task<bool> HasRadiologistAuthorityAsync(CancellationToken cancellationToken) =>
            Task.FromResult(radiolog);
    }

    private static RadReportService Service(
        ApplicationDbContext db,
        Guid actorUserId,
        bool radiolog = false)
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, actorUserId.ToString()) },
            "Test"));

        var accessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext { User = principal },
        };

        return new BacaanDenganKewenangan(
            db,
            accessor,
            new LoggerService(NullLogger<LoggerService>.Instance, accessor),
            new RadReportNumberService(),
            radiolog);
    }

    private static async Task<Guid> SeedStudyAsync(ApplicationDbContext db, Guid encounterId)
    {
        var order = new RadOrder
        {
            Id = Guid.NewGuid(),
            EncounterId = encounterId,
            ProcedureId = Guid.NewGuid(),
            ModalityId = Guid.NewGuid(),
            OrderStatus = RadOrderStatus.InProgress,
        };

        var study = new RadStudy
        {
            Id = Guid.NewGuid(),
            RadOrderId = order.Id,
            EncounterId = encounterId,
            ProcedureId = order.ProcedureId,
            ModalityId = order.ModalityId,
            StudySequence = 1,
            StudyNumber = $"RAD-{order.Id:N}-001".ToUpperInvariant(),
            StudyStatus = RadStudyStatus.QualityAccepted,
            IsUsable = true,
        };

        db.RadOrders.Add(order);
        db.RadStudies.Add(study);
        await db.SaveChangesAsync();

        return study.Id;
    }

    /// <summary>Satu bacaan berstatus draf pada kunjungan yang diberikan.</summary>
    private static async Task<Guid> BacaanPadaKunjunganAsync(
        ApplicationDbContext db,
        Guid encounterId)
    {
        var study = await SeedStudyAsync(db, encounterId);

        var draf = await Service(db, Residen).CreateDraftAsync(
            study,
            new CreateRadReportDraftRequest
            {
                AuthorRole = RadReportAuthorRole.Resident,
                Impression = IsiVersiSatu,
            });

        Assert.Equal(RadOperationResultKind.Success, draf.Kind);

        return draf.Value!.Id;
    }

    /// <summary>Satu bacaan berstatus draf beserta kunjungannya.</summary>
    private static async Task<(Guid ReportId, Guid EncounterId)> DrafAsync(ApplicationDbContext db)
    {
        var kunjungan = Guid.NewGuid();

        return (await BacaanPadaKunjunganAsync(db, kunjungan), kunjungan);
    }

    /// <summary>Satu bacaan yang sudah disahkan dan dirilis beserta kunjungannya.</summary>
    private static async Task<(Guid ReportId, Guid EncounterId)> BacaanDirilisAsync(
        ApplicationDbContext db)
    {
        var (laporan, kunjungan) = await DrafAsync(db);

        await Service(db, Radiolog, radiolog: true).ValidateAsync(laporan);
        await Service(db, Radiolog, radiolog: true).ReleaseAsync(laporan);

        return (laporan, kunjungan);
    }

    private static CreateRadReportAmendmentRequest Koreksi() => new()
    {
        AmendmentReason = "Peninjauan ulang menemukan perdarahan yang terlewat.",
        Impression = IsiVersiDua,
    };

    private static async Task RilisKoreksiAsync(ApplicationDbContext db, Guid reportId)
    {
        var service = Service(db, Radiolog, radiolog: true);

        var draf = await service.CreateAmendmentAsync(reportId, Koreksi());
        Assert.Equal(RadOperationResultKind.Success, draf.Kind);

        await service.ValidateAsync(reportId);
        await service.ReleaseAsync(reportId);
    }
}
