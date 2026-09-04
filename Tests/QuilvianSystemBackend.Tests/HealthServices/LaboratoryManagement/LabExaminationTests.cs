using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using Xunit;

namespace QuilvianSystemBackend.Tests.HealthServices.LaboratoryManagement;

/// <summary>
/// Bukti untuk <c>BE-LAB-09</c> — entity pemeriksaan terpesan
/// (<c>FR-02.1</c>; <c>LAB-DEC-024</c>, <c>LAB-DEC-026</c>; BR-20, BR-22).
///
/// Yang dibuktikan di sini:
///   1. <c>AC-35</c> — satu wadah fisik menopang lebih dari satu pemeriksaan terpesan, dan
///      wadah itu tetap hanya memiliki <b>satu</b> barcode;
///   2. setiap pemeriksaan menyimpan salinan tarifnya sendiri, sehingga dua pemeriksaan pada
///      wadah yang sama dapat berbeda harga;
///   3. <c>AC-40</c> — penanda cito dan duplo berada pada baris pemeriksaan, dan
///      <b>tidak ada</b> kolom sejenis pada pesanan maupun wadah;
///   4. satu wadah tidak boleh menopang jenis pemeriksaan yang sama dua kali;
///   5. pemetaan tabel sesuai <c>erd/data-dictionary.md</c> bagian 3 dan 11.1;
///   6. <c>QBE-NAM-001</c> — entity ini bernama <c>LabExamination</c>, bukan
///      <c>TrxLabExamination</c>, beserta nama tabel yang sepaket dengannya.
/// </summary>
/// <remarks>
/// Dua jenis context dipakai dan keduanya disengaja.
///
/// Provider InMemory dipakai untuk bukti penyimpanan supaya pengujian berjalan tanpa database
/// mana pun; konsekuensinya index unik fisik tidak ditegakkan di sana.
///
/// Karena itu bukti index, nama tabel, tipe kolom, filter soft delete, dan perilaku hapus
/// dibaca dari model relasional Npgsql. Model itu dibangun sepenuhnya di memori — tidak ada
/// koneksi yang dibuka dan tidak ada perintah database yang dijalankan — sehingga bentuk
/// schema yang sebenarnya dapat diperiksa tanpa wewenang database.
///
/// Penolakan beserta kode statusnya adalah pekerjaan endpoint pada <c>BE-LAB-16</c>; yang
/// menjadi cakupan <c>BE-LAB-09</c> adalah struktur yang membuat penolakan itu dapat
/// ditegakkan.
/// </remarks>
public class LabExaminationTests
{
    // =====================================================================
    // 1. AC-35 — satu wadah menopang beberapa pemeriksaan
    // =====================================================================

    [Fact]
    public async Task AC35_SatuWadahMenopangTigaPemeriksaan_DenganSatuBarcodeSaja()
    {
        await using var context = CreateInMemoryContext();
        var (orderId, specimenId, hemoglobin, leukosit, trombosit) = await SeedAsync(context);

        context.LabExaminations.AddRange(
            Pemeriksaan(orderId, specimenId, hemoglobin, "LAB-HB", "Hemoglobin", 35_000m),
            Pemeriksaan(orderId, specimenId, leukosit, "LAB-WBC", "Leukosit", 30_000m),
            Pemeriksaan(orderId, specimenId, trombosit, "LAB-PLT", "Trombosit", 28_000m));

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var tersimpan = await context.LabExaminations
            .AsNoTracking()
            .Where(x => x.SpecimenId == specimenId)
            .ToListAsync();

        // Tiga pemeriksaan berdiri sebagai tiga baris terpisah di atas satu wadah.
        Assert.Equal(3, tersimpan.Count);
        Assert.All(tersimpan, x => Assert.Equal(specimenId, x.SpecimenId));
        Assert.All(tersimpan, x => Assert.Equal(orderId, x.LabOrderId));

        // Inti AC-35: wadahnya tetap satu, sehingga pasien hanya ditusuk sekali dan hanya
        // menerima satu barcode.
        var wadah = await context.LabSpecimens
            .AsNoTracking()
            .Where(x => x.LabOrderId == orderId)
            .ToListAsync();

        Assert.Single(wadah);
        Assert.Equal("BC-0001", wadah[0].SpecimenBarcode);
    }

    [Fact]
    public async Task SetiapPemeriksaan_MenyimpanSalinanTarifnyaSendiri()
    {
        await using var context = CreateInMemoryContext();
        var (orderId, specimenId, hemoglobin, leukosit, _) = await SeedAsync(context);

        context.LabExaminations.AddRange(
            Pemeriksaan(orderId, specimenId, hemoglobin, "LAB-HB", "Hemoglobin", 35_000m),
            Pemeriksaan(orderId, specimenId, leukosit, "LAB-WBC", "Leukosit", 30_000m));

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var tersimpan = await context.LabExaminations
            .AsNoTracking()
            .OrderBy(x => x.ProcedureCodeSnapshot)
            .ToListAsync();

        // Dua pemeriksaan pada satu wadah yang sama berbeda harganya. Inilah sebabnya salinan
        // tarif tinggal di sini, bukan di wadah.
        Assert.Equal("LAB-HB", tersimpan[0].ProcedureCodeSnapshot);
        Assert.Equal(35_000m, tersimpan[0].UnitPriceSnapshot);
        Assert.Equal("LAB-WBC", tersimpan[1].ProcedureCodeSnapshot);
        Assert.Equal(30_000m, tersimpan[1].UnitPriceSnapshot);

        Assert.Equal("Hemoglobin", tersimpan[0].ProcedureNameSnapshot);
        Assert.All(tersimpan, x => Assert.NotNull(x.TariffId));
        Assert.All(tersimpan, x => Assert.NotNull(x.TariffCodeSnapshot));
    }

    [Fact]
    public async Task PemeriksaanBaru_LahirBerstatusOrderedDanBelumLayakTagih()
    {
        await using var context = CreateInMemoryContext();
        var (orderId, specimenId, hemoglobin, _, _) = await SeedAsync(context);

        context.LabExaminations.Add(
            Pemeriksaan(orderId, specimenId, hemoglobin, "LAB-HB", "Hemoglobin", 35_000m));

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var tersimpan = await context.LabExaminations.AsNoTracking().SingleAsync();

        Assert.Equal(LabExaminationStatus.Ordered, tersimpan.ExaminationStatus);

        // Kelayakan tagih baru terbentuk ketika wadah penopangnya dinyatakan layak; sampai itu
        // terjadi, waktunya kosong.
        Assert.Null(tersimpan.ChargeEligibleAt);
    }

    [Fact]
    public async Task NavigasiDuaArah_WadahDanPesananSamaSamaMengenaliPemeriksaannya()
    {
        await using var context = CreateInMemoryContext();
        var (orderId, specimenId, hemoglobin, leukosit, _) = await SeedAsync(context);

        context.LabExaminations.AddRange(
            Pemeriksaan(orderId, specimenId, hemoglobin, "LAB-HB", "Hemoglobin", 35_000m),
            Pemeriksaan(orderId, specimenId, leukosit, "LAB-WBC", "Leukosit", 30_000m));

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var pesanan = await context.LabOrders
            .AsNoTracking()
            .Include(x => x.Examinations)
            .SingleAsync(x => x.Id == orderId);

        var wadah = await context.LabSpecimens
            .AsNoTracking()
            .Include(x => x.Examinations)
            .SingleAsync(x => x.Id == specimenId);

        Assert.Equal(2, pesanan.Examinations.Count);
        Assert.Equal(2, wadah.Examinations.Count);
    }

    // =====================================================================
    // 2. AC-40 — kesegeraan dan duplo melekat pada pemeriksaan
    // =====================================================================

    [Fact]
    public async Task AC40_SatuPesananMemuatPemeriksaanCitoDanBiasaSekaligus()
    {
        await using var context = CreateInMemoryContext();
        var (orderId, specimenId, elektrolit, lipid, _) = await SeedAsync(context);

        var segera = Pemeriksaan(orderId, specimenId, elektrolit, "LAB-NA", "Natrium", 40_000m);
        segera.Urgency = LabExaminationUrgency.Cito;
        segera.UrgencyMarkedAt = new DateTime(2026, 9, 3, 8, 0, 0, DateTimeKind.Utc);
        segera.UrgencyMarkedByUserId = DokterPemesan;

        var menunggu = Pemeriksaan(orderId, specimenId, lipid, "LAB-LIP", "Profil Lipid", 120_000m);

        context.LabExaminations.AddRange(segera, menunggu);

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var tersimpan = await context.LabExaminations
            .AsNoTracking()
            .Where(x => x.LabOrderId == orderId)
            .ToListAsync();

        var cito = tersimpan.Single(x => x.Urgency == LabExaminationUrgency.Cito);
        var biasa = tersimpan.Single(x => x.Urgency == LabExaminationUrgency.Routine);

        // Inti AC-40 dan LAB-DEC-026: dua tingkat kesegeraan berdampingan dalam satu pesanan.
        // Bila kesegeraan disimpan di tingkat pesanan, profil lipid ikut diperlakukan cito dan
        // menenggelamkan natrium yang benar-benar mendesak.
        Assert.Equal("LAB-NA", cito.ProcedureCodeSnapshot);
        Assert.Equal("LAB-LIP", biasa.ProcedureCodeSnapshot);

        // Penandaan menyimpan jejak siapa dan kapan.
        Assert.Equal(DokterPemesan, cito.UrgencyMarkedByUserId);
        Assert.Equal(new DateTime(2026, 9, 3, 8, 0, 0, DateTimeKind.Utc), cito.UrgencyMarkedAt);

        // Pemeriksaan biasa tidak pernah ditandai, sehingga jejaknya kosong.
        Assert.Null(biasa.UrgencyMarkedAt);
        Assert.Null(biasa.UrgencyMarkedByUserId);
    }

    [Fact]
    public async Task PenandaDuplo_TersimpanPadaBarisPemeriksaan()
    {
        await using var context = CreateInMemoryContext();
        var (orderId, specimenId, hemoglobin, leukosit, _) = await SeedAsync(context);

        var ganda = Pemeriksaan(orderId, specimenId, hemoglobin, "LAB-HB", "Hemoglobin", 35_000m);
        ganda.IsDuplo = true;

        var tunggal = Pemeriksaan(orderId, specimenId, leukosit, "LAB-WBC", "Leukosit", 30_000m);

        context.LabExaminations.AddRange(ganda, tunggal);

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var tersimpan = await context.LabExaminations.AsNoTracking().ToListAsync();

        Assert.True(tersimpan.Single(x => x.ProcedureCodeSnapshot == "LAB-HB").IsDuplo);
        Assert.False(tersimpan.Single(x => x.ProcedureCodeSnapshot == "LAB-WBC").IsDuplo);
    }

    /// <summary>
    /// Bukti bahwa kesegeraan dan duplo memang <b>tidak ada</b> di tempat lain. Tanpa ini,
    /// penegakan <c>AC-40</c> bergantung pada kesepakatan yang tidak terperiksa: cukup satu
    /// orang menambahkan kolom cito pada pesanan, dan sistem punya dua sumber kebenaran.
    /// </summary>
    [Fact]
    public void AC40_PesananDanWadah_TidakPunyaKolomKesegeraanMaupunDuplo()
    {
        using var context = CreateRelationalModelContext();

        var order = context.Model.FindEntityType(typeof(LabOrder));
        var specimen = context.Model.FindEntityType(typeof(LabSpecimen));
        var examination = context.Model.FindEntityType(typeof(LabExamination));

        Assert.NotNull(order);
        Assert.NotNull(specimen);
        Assert.NotNull(examination);

        foreach (var kolom in new[] { "Urgency", "UrgencyMarkedAt", "UrgencyMarkedByUserId", "IsDuplo" })
        {
            Assert.Null(order!.FindProperty(kolom));
            Assert.Null(specimen!.FindProperty(kolom));
            Assert.NotNull(examination!.FindProperty(kolom));
        }
    }

    // =====================================================================
    // 3. Keunikan wadah dan jenis pemeriksaan
    // =====================================================================

    [Fact]
    public void SatuWadah_TidakBolehMenopangJenisPemeriksaanYangSamaDuaKali()
    {
        using var context = CreateRelationalModelContext();

        var entityType = context.Model.FindEntityType(typeof(LabExamination));

        Assert.NotNull(entityType);

        var index = entityType!.GetIndexes().SingleOrDefault(x =>
            x.Properties.Select(p => p.Name).SequenceEqual(new[]
            {
                nameof(LabExamination.SpecimenId),
                nameof(LabExamination.ProcedureId)
            }));

        Assert.NotNull(index);
        Assert.True(index!.IsUnique);
        Assert.Equal("IX_LabExamination_SpecimenId_ProcedureId", index.GetDatabaseName());

        // Baris yang sudah dihapus tidak boleh menghalangi pemesanan ulang jenis pemeriksaan
        // yang sama pada wadah itu.
        Assert.Equal("\"IsDelete\" = false", index.GetFilter());
    }

    [Fact]
    public void EmpatIndexPencarian_TerpasangSesuaiKamusData()
    {
        using var context = CreateRelationalModelContext();

        var entityType = context.Model.FindEntityType(typeof(LabExamination));

        Assert.NotNull(entityType);

        foreach (var kolom in new[]
        {
            nameof(LabExamination.LabOrderId),
            nameof(LabExamination.ExaminationStatus),
            nameof(LabExamination.ChargeEligibleAt),
            nameof(LabExamination.Urgency)
        })
        {
            var index = entityType!.GetIndexes().SingleOrDefault(x =>
                x.Properties.Count == 1 && x.Properties[0].Name == kolom);

            Assert.True(index != null, $"Index untuk {kolom} tidak ditemukan.");
            Assert.False(index!.IsUnique);
        }
    }

    // =====================================================================
    // 4. Bentuk tabel dan penamaan
    // =====================================================================

    /// <summary>
    /// <c>QBE-NAM-001</c> melarang awalan <c>Trx</c> untuk kode baru, dan rancangan roadmap
    /// revision 1 sempat keliru menamainya <c>TrxLabExamination</c> sebelum dikoreksi. Nama
    /// entity, berkas, configuration, DbSet, dan tabel adalah satu paket.
    /// </summary>
    [Fact]
    public void PenamaanEntityDanTabel_MengikutiPrefixLabYangTerdaftar()
    {
        using var context = CreateRelationalModelContext();

        var entityType = context.Model.FindEntityType(typeof(LabExamination));

        Assert.NotNull(entityType);

        Assert.Equal("LabExamination", typeof(LabExamination).Name);
        Assert.StartsWith("Lab", typeof(LabExamination).Name, StringComparison.Ordinal);
        Assert.DoesNotContain("Trx", typeof(LabExamination).Name, StringComparison.Ordinal);

        // Nama tabel tunggal PascalCase, sama persis dengan nama entity, pada schema public.
        Assert.Equal("LabExamination", entityType!.GetTableName());
        Assert.Equal("public", entityType.GetSchema());

        // DbSet adalah bentuk jamak dari nama entity.
        Assert.NotNull(typeof(ApplicationDbContext).GetProperty("LabExaminations"));
    }

    [Fact]
    public void BentukKolom_SesuaiKamusDataBagian11Satu()
    {
        using var context = CreateRelationalModelContext();

        var entityType = context.Model.FindEntityType(typeof(LabExamination));

        Assert.NotNull(entityType);

        Assert.Equal(50, entityType!.FindProperty(nameof(LabExamination.ProcedureCodeSnapshot))!.GetMaxLength());
        Assert.Equal(200, entityType.FindProperty(nameof(LabExamination.ProcedureNameSnapshot))!.GetMaxLength());
        Assert.Equal(50, entityType.FindProperty(nameof(LabExamination.TariffCodeSnapshot))!.GetMaxLength());

        var harga = entityType.FindProperty(nameof(LabExamination.UnitPriceSnapshot))!;
        Assert.Equal(18, harga.GetPrecision());
        Assert.Equal(2, harga.GetScale());

        // Kedua enum disimpan sebagai int.
        Assert.Equal(typeof(int), entityType.FindProperty(nameof(LabExamination.ExaminationStatus))!.GetProviderClrType());
        Assert.Equal(typeof(int), entityType.FindProperty(nameof(LabExamination.Urgency))!.GetProviderClrType());

        // Token konkurensi, mengikuti pola LabSpecimen.
        Assert.True(entityType.FindProperty(nameof(LabExamination.Version))!.IsConcurrencyToken);

        // Kolom wajib.
        Assert.False(entityType.FindProperty(nameof(LabExamination.LabOrderId))!.IsNullable);
        Assert.False(entityType.FindProperty(nameof(LabExamination.SpecimenId))!.IsNullable);
        Assert.False(entityType.FindProperty(nameof(LabExamination.ProcedureId))!.IsNullable);
        Assert.False(entityType.FindProperty(nameof(LabExamination.IsDuplo))!.IsNullable);

        // Kolom yang boleh kosong.
        Assert.True(entityType.FindProperty(nameof(LabExamination.TariffId))!.IsNullable);
        Assert.True(entityType.FindProperty(nameof(LabExamination.ChargeEligibleAt))!.IsNullable);
        Assert.True(entityType.FindProperty(nameof(LabExamination.UrgencyMarkedAt))!.IsNullable);
    }

    [Fact]
    public void KetigaRelasi_MemakaiRestrictAgarTautanTagihanTidakPutus()
    {
        using var context = CreateRelationalModelContext();

        var entityType = context.Model.FindEntityType(typeof(LabExamination));

        Assert.NotNull(entityType);

        var relasi = entityType!.GetForeignKeys().ToList();

        // Tepat tiga foreign key: pesanan, wadah, dan jenis pemeriksaan.
        Assert.Equal(3, relasi.Count);
        Assert.All(relasi, x => Assert.Equal(DeleteBehavior.Restrict, x.DeleteBehavior));

        Assert.Contains(relasi, x =>
            x.PrincipalEntityType.ClrType == typeof(LabOrder) &&
            x.Properties.Single().Name == nameof(LabExamination.LabOrderId));

        Assert.Contains(relasi, x =>
            x.PrincipalEntityType.ClrType == typeof(LabSpecimen) &&
            x.Properties.Single().Name == nameof(LabExamination.SpecimenId));

        Assert.Contains(relasi, x =>
            x.PrincipalEntityType.ClrType == typeof(MstProcedure) &&
            x.Properties.Single().Name == nameof(LabExamination.ProcedureId));

        // TariffId sengaja tanpa foreign key, mengikuti LabSpecimen: tarif adalah salinan
        // bukti nilai saat kejadian, bukan tautan hidup ke data induk.
        Assert.DoesNotContain(relasi, x =>
            x.Properties.Single().Name == nameof(LabExamination.TariffId));
    }

    /// <summary>
    /// Slice hasil masih tertahan <c>LAB-SIGN-001</c>, dan akibat finansial milik Billing.
    /// Keduanya tidak boleh menyelinap masuk lewat kolom yang "sekalian dibuat".
    /// </summary>
    [Fact]
    public void TidakAdaKolomHasilMaupunKolomFinansial()
    {
        using var context = CreateRelationalModelContext();

        var entityType = context.Model.FindEntityType(typeof(LabExamination));

        Assert.NotNull(entityType);

        foreach (var terlarang in new[]
        {
            "ResultValue", "ResultText", "ResultAt", "ValidatedAt", "ReleasedAt",
            "IsPaid", "PaidAt", "SettlementId", "InvoiceId", "RefundAt"
        })
        {
            Assert.True(
                entityType!.FindProperty(terlarang) == null,
                $"Kolom {terlarang} tidak boleh ada pada LabExamination.");
        }
    }

    // =====================================================================
    // Pembantu
    // =====================================================================

    private static readonly Guid DokterPemesan = Guid.Parse("66666666-6666-6666-6666-666666666666");

    private static LabExamination Pemeriksaan(
        Guid labOrderId,
        Guid specimenId,
        Guid procedureId,
        string procedureCode,
        string procedureName,
        decimal unitPrice) =>
        new()
        {
            LabOrderId = labOrderId,
            SpecimenId = specimenId,
            ProcedureId = procedureId,
            ProcedureCodeSnapshot = procedureCode,
            ProcedureNameSnapshot = procedureName,
            TariffId = Guid.NewGuid(),
            TariffCodeSnapshot = $"TRF-{procedureCode}",
            UnitPriceSnapshot = unitPrice
        };

    private static ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"lab-examination-{Guid.NewGuid():N}")
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ApplicationDbContext(options);
    }

    /// <summary>
    /// Membangun model relasional Npgsql tanpa membuka koneksi apa pun. Dipakai hanya untuk
    /// membaca bentuk schema — nama tabel, tipe kolom, index, filter, dan perilaku hapus —
    /// yang tidak dapat dibaca dari provider InMemory.
    /// </summary>
    private static ApplicationDbContext CreateRelationalModelContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=lab_examination_model_only")
            .Options;

        return new ApplicationDbContext(options);
    }

    /// <summary>
    /// Satu pesanan, satu wadah darah ungu, dan lima jenis pemeriksaan yang dapat ditopang
    /// wadah itu.
    /// </summary>
    private static async Task<(Guid OrderId, Guid SpecimenId, Guid A, Guid B, Guid C)> SeedAsync(
        ApplicationDbContext context)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];

        var procedures = new List<MstProcedure>();

        foreach (var (kode, nama) in new[]
        {
            ("HB", "Hemoglobin"), ("WBC", "Leukosit"), ("PLT", "Trombosit"),
            ("NA", "Natrium"), ("LIP", "Profil Lipid")
        })
        {
            procedures.Add(new MstProcedure
            {
                Id = Guid.NewGuid(),
                ProcedureCode = $"{kode}-{suffix}",
                ProcedureName = nama,
                ProcedureType = "Laboratory",
                IsLaboratory = true,
                IsActive = true
            });
        }

        var order = new LabOrder
        {
            Id = Guid.NewGuid(),
            EncounterId = Guid.NewGuid(),
            ProcedureId = procedures[0].Id,
            Discipline = LabDiscipline.ClinicalPathology,
            OrderStatus = LabOrderStatus.Requested
        };

        var specimen = new LabSpecimen
        {
            Id = Guid.NewGuid(),
            LabOrderId = order.Id,
            SpecimenBarcode = "BC-0001",
            SpecimenSequence = 1,
            SpecimenStatus = LabSpecimenStatus.Planned
        };

        context.Set<MstProcedure>().AddRange(procedures);
        context.LabOrders.Add(order);
        context.LabSpecimens.Add(specimen);

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        return (order.Id, specimen.Id, procedures[0].Id, procedures[1].Id, procedures[2].Id);
    }
}
