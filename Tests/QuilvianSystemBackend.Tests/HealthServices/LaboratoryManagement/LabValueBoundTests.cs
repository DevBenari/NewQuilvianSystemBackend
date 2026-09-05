using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using Xunit;

namespace QuilvianSystemBackend.Tests.HealthServices.LaboratoryManagement;

/// <summary>
/// Bukti untuk <c>BE-LAB-02</c> — tabel batas nilai dan pilihan hasil
/// (<c>FR-03.1</c>, <c>FR-03.2</c>, <c>FR-03.6</c>; <c>LAB-DEC-006</c>, <c>LAB-DEC-018</c>,
/// <c>LAB-DEC-021</c>).
///
/// Yang dibuktikan di sini:
///   1. <c>AC-24</c> — tiga baris batas Hemoglobin (pria dewasa, wanita dewasa, anak)
///      tersimpan berdampingan untuk satu jenis pemeriksaan yang sama.
///   2. <c>VAL-21</c> — kombinasi pemeriksaan, jenis kelamin, dan kelompok umur dijaga unik
///      oleh index database, bukan hanya oleh pemeriksaan di service.
///   3. <c>AC-28</c> — batas berbentuk pilihan menyimpan daftar pilihan yang sah beserta
///      penanda di luar rujukan dan penanda kritis, dan kode pilihannya unik per batas.
///   4. <c>AC-25</c> — <c>MstProcedure</c> tidak bertambah satu pun kolom operasional
///      laboratorium akibat pekerjaan ini.
///   5. <c>AC-49</c> — kedua entity tinggal di modul Laboratorium, dan data induk global
///      (<c>MstProcedure</c>, <c>MstAgeCategory</c>) hanya ditunjuk, tidak disalin.
///   6. Pemetaan kedua tabel sesuai <c>erd/data-dictionary.md</c> bagian 5, 6, 11.2, dan 11.3.
/// </summary>
/// <remarks>
/// Dua jenis context dipakai dan keduanya disengaja.
///
/// Provider InMemory dipakai untuk bukti penyimpanan supaya pengujian dapat berjalan tanpa
/// database mana pun; konsekuensinya index unik fisik tidak ditegakkan di sana.
///
/// Karena itu bukti index, nama tabel, tipe kolom, filter soft delete, dan perilaku hapus
/// dibaca dari model relasional Npgsql. Model itu dibangun sepenuhnya di memori — tidak ada
/// koneksi yang dibuka dan tidak ada perintah database yang dijalankan — sehingga bentuk
/// schema yang sebenarnya dapat diperiksa tanpa wewenang database.
///
/// Penolakan <c>409</c> beserta pesan <c>VAL-21</c> adalah pekerjaan endpoint pengelolaan pada
/// <c>BE-LAB-04</c>; yang menjadi cakupan <c>BE-LAB-02</c> adalah constraint yang membuat
/// penolakan itu dapat ditegakkan.
/// </remarks>
public class LabValueBoundTests
{
    // =====================================================================
    // 1. AC-24 — tiga baris batas untuk satu jenis pemeriksaan
    // =====================================================================

    [Fact]
    public async Task TigaBarisBatasHemoglobin_TersimpanBerdampinganUntukSatuPemeriksaan()
    {
        await using var context = CreateInMemoryContext();
        var (procedureId, dewasaId, anakId) = await SeedMasterDataAsync(context);

        context.LabValueBounds.AddRange(
            new LabValueBound
            {
                ProcedureId = procedureId,
                ResultForm = LabResultForm.Numeric,
                Unit = "g/dL",
                GenderScope = LabGenderScope.Male,
                AgeCategoryId = dewasaId,
                NormalLow = 13.0m,
                NormalHigh = 17.0m,
                CriticalLow = 7.0m,
                CriticalHigh = 20.0m
            },
            new LabValueBound
            {
                ProcedureId = procedureId,
                ResultForm = LabResultForm.Numeric,
                Unit = "g/dL",
                GenderScope = LabGenderScope.Female,
                AgeCategoryId = dewasaId,
                NormalLow = 12.0m,
                NormalHigh = 15.0m,
                CriticalLow = 7.0m,
                CriticalHigh = 20.0m
            },
            new LabValueBound
            {
                ProcedureId = procedureId,
                ResultForm = LabResultForm.Numeric,
                Unit = "g/dL",
                GenderScope = LabGenderScope.All,
                AgeCategoryId = anakId,
                NormalLow = 11.0m,
                NormalHigh = 14.0m,
                CriticalLow = 6.0m,
                CriticalHigh = 18.0m
            });

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var tersimpan = await context.LabValueBounds
            .AsNoTracking()
            .Where(x => x.ProcedureId == procedureId)
            .ToListAsync();

        // Ketiganya berdiri sebagai baris terpisah pada satu jenis pemeriksaan yang sama.
        Assert.Equal(3, tersimpan.Count);
        Assert.All(tersimpan, x => Assert.Equal(procedureId, x.ProcedureId));
        Assert.All(tersimpan, x => Assert.True(x.IsActive));

        var pria = tersimpan.Single(x => x.GenderScope == LabGenderScope.Male);
        var wanita = tersimpan.Single(x => x.GenderScope == LabGenderScope.Female);
        var anak = tersimpan.Single(x => x.GenderScope == LabGenderScope.All);

        Assert.Equal(13.0m, pria.NormalLow);
        Assert.Equal(17.0m, pria.NormalHigh);
        Assert.Equal(dewasaId, pria.AgeCategoryId);
        Assert.Equal(12.0m, wanita.NormalLow);
        Assert.Equal(dewasaId, wanita.AgeCategoryId);
        Assert.Equal(anakId, anak.AgeCategoryId);
        Assert.Equal(6.0m, anak.CriticalLow);
    }

    [Fact]
    public async Task BatasBerlakuSemuaUmur_MenyimpanKelompokUmurKosong()
    {
        await using var context = CreateInMemoryContext();
        var (procedureId, _, _) = await SeedMasterDataAsync(context);

        context.LabValueBounds.Add(new LabValueBound
        {
            ProcedureId = procedureId,
            ResultForm = LabResultForm.Numeric,
            Unit = "mmol/L",
            GenderScope = LabGenderScope.All,
            AgeCategoryId = null,
            NormalLow = 3.5m,
            NormalHigh = 5.1m,
            CriticalLow = 2.5m,
            CriticalHigh = 6.0m
        });

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var kalium = await context.LabValueBounds.AsNoTracking().SingleAsync();

        // Kosong berarti berlaku untuk semua umur; itu keadaan yang sah, bukan data hilang.
        Assert.Null(kalium.AgeCategoryId);
        Assert.Equal(LabGenderScope.All, kalium.GenderScope);
    }

    // =====================================================================
    // 2. VAL-21 — keunikan kombinasi ditegakkan database
    // =====================================================================

    [Fact]
    public void KombinasiPemeriksaanJenisKelaminKelompokUmur_UnikDiDatabase()
    {
        using var context = CreateRelationalModelContext();

        var entityType = context.Model.FindEntityType(typeof(LabValueBound));

        Assert.NotNull(entityType);

        var index = entityType!.GetIndexes().SingleOrDefault(x =>
            x.Properties.Select(p => p.Name).SequenceEqual(new[]
            {
                nameof(LabValueBound.ProcedureId),
                nameof(LabValueBound.GenderScope),
                nameof(LabValueBound.AgeCategoryId)
            }));

        Assert.NotNull(index);
        Assert.True(index!.IsUnique);
        Assert.Equal("IX_LabValueBound_Procedure_Gender_AgeCategory", index.GetDatabaseName());

        // Baris yang sudah dihapus tidak boleh menghalangi pembuatan baris baru untuk
        // kelompok pasien yang sama.
        Assert.Equal("\"IsDelete\" = false", index.GetFilter());

        // Catatan tentang NULLS NOT DISTINCT.
        //
        // Kelompok umur yang kosong berarti "berlaku untuk semua umur" — itu sebuah nilai,
        // bukan ketiadaan nilai. Tanpa NULLS NOT DISTINCT, PostgreSQL menganggap tiap NULL
        // berbeda, sehingga dua baris "semua umur" untuk kombinasi yang sama lolos dari index
        // unik. Setelan itu dipasang lewat AreNullsDistinct(false) pada LabValueBoundConfiguration.
        //
        // Setelan itu sengaja TIDAK diperiksa di sini. Npgsql menerapkannya pada tahap
        // finalisasi model relasional, sehingga ia tidak terbaca sebagai anotasi index pada
        // context.Model; memaksakannya di sini hanya menghasilkan pengujian yang menguji
        // pemahaman kita atas API, bukan menguji schemanya. Buktinya diambil di tempat yang
        // memang menentukan — database — dan dicatat pada laporan BE-LAB-02 bagian 10.1:
        // pg_indexes menampilkan NULLS NOT DISTINCT, pg_index.indnullsnotdistinct bernilai
        // True, dan percobaan menyimpan dua baris "semua umur" ditolak.
    }

    [Fact]
    public void LabValueBound_TidakPunyaKolomUrutanTampilYangDipersistensi()
    {
        using var context = CreateRelationalModelContext();

        var entityType = context.Model.FindEntityType(typeof(LabValueBound));

        Assert.NotNull(entityType);

        // QBE-ENT-003 melarang kolom presentasi yang dipersistensi untuk kode baru. Urutan
        // tampil baris batas dapat diturunkan dari datanya sendiri — jenis kelamin dan kelompok
        // umur — sehingga tidak perlu disimpan.
        Assert.Null(entityType!.FindProperty("SortOrder"));

        // Bandingkan dengan LabValueOption: di sana urutan menyatakan tingkatan skala ordinal
        // hasil (Negatif, +1, +2, +3, +4), dan itu isi bisnis, bukan tampilan.
        var option = context.Model.FindEntityType(typeof(LabValueOption));

        Assert.NotNull(option);
        Assert.NotNull(option!.FindProperty(nameof(LabValueOption.SortOrder)));
    }

    // =====================================================================
    // 3. AC-28 — daftar pilihan yang sah
    // =====================================================================

    [Fact]
    public async Task BatasBentukPilihan_MenyimpanDaftarPilihanBesertaPenandanya()
    {
        await using var context = CreateInMemoryContext();
        var (procedureId, _, _) = await SeedMasterDataAsync(context);

        var proteinUrin = new LabValueBound
        {
            ProcedureId = procedureId,
            ResultForm = LabResultForm.Choice,
            GenderScope = LabGenderScope.All
        };

        proteinUrin.Options.Add(new LabValueOption { OptionCode = "NEG", OptionName = "Negatif", SortOrder = 0 });
        proteinUrin.Options.Add(new LabValueOption { OptionCode = "P1", OptionName = "+1", IsOutOfReference = true, SortOrder = 1 });
        proteinUrin.Options.Add(new LabValueOption { OptionCode = "P2", OptionName = "+2", IsOutOfReference = true, SortOrder = 2 });
        proteinUrin.Options.Add(new LabValueOption { OptionCode = "P3", OptionName = "+3", IsOutOfReference = true, IsCritical = true, SortOrder = 3 });
        proteinUrin.Options.Add(new LabValueOption { OptionCode = "P4", OptionName = "+4", IsOutOfReference = true, IsCritical = true, SortOrder = 4 });

        context.LabValueBounds.Add(proteinUrin);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var tersimpan = await context.LabValueBounds
            .AsNoTracking()
            .Include(x => x.Options)
            .SingleAsync();

        Assert.Equal(LabResultForm.Choice, tersimpan.ResultForm);

        // Bentuk pilihan tidak memakai satuan maupun batas angka.
        Assert.Null(tersimpan.Unit);
        Assert.Null(tersimpan.NormalLow);
        Assert.Null(tersimpan.CriticalHigh);

        var pilihan = tersimpan.Options.OrderBy(x => x.SortOrder).ToList();

        Assert.Equal(5, pilihan.Count);
        Assert.Equal(new[] { "Negatif", "+1", "+2", "+3", "+4" }, pilihan.Select(x => x.OptionName));

        // Golongan darah dan tes kehamilan berbentuk pilihan tanpa nilai kritis; karena itu
        // "di luar rujukan" dan "kritis" adalah dua penanda terpisah.
        Assert.Equal(new[] { "P3", "P4" }, pilihan.Where(x => x.IsCritical).Select(x => x.OptionCode));
        Assert.Equal(4, pilihan.Count(x => x.IsOutOfReference));
        Assert.False(pilihan.Single(x => x.OptionCode == "NEG").IsOutOfReference);
    }

    [Fact]
    public void KodePilihan_UnikDalamSatuBatasNilai()
    {
        using var context = CreateRelationalModelContext();

        var entityType = context.Model.FindEntityType(typeof(LabValueOption));

        Assert.NotNull(entityType);

        var index = entityType!.GetIndexes().SingleOrDefault(x =>
            x.Properties.Select(p => p.Name).SequenceEqual(new[]
            {
                nameof(LabValueOption.ValueBoundId),
                nameof(LabValueOption.OptionCode)
            }));

        Assert.NotNull(index);
        Assert.True(index!.IsUnique);
        Assert.Equal("IX_LabValueOption_ValueBoundId_OptionCode", index.GetDatabaseName());
        Assert.Equal("\"IsDelete\" = false", index.GetFilter());
    }

    // =====================================================================
    // 4. AC-25 — MstProcedure tidak bertambah kolom operasional laboratorium
    // =====================================================================

    [Fact]
    public void MstProcedure_TidakBertambahKolomOperasionalLaboratorium()
    {
        using var context = CreateRelationalModelContext();

        var mstProcedure = context.Model.FindEntityType(typeof(MstProcedure));

        Assert.NotNull(mstProcedure);

        var kolom = mstProcedure!.GetProperties()
            .Select(x => x.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Seluruh atribut operasional laboratorium wajib tinggal di tabel milik Laboratorium.
        // LAB-DEC-036 mengamandemen AC-25 dengan tepat satu pengecualian — kolom klasifikasi
        // disiplin, yang merupakan cakupan BE-EXT-01 milik master-data — sehingga daftar
        // terlarang di bawah sengaja tidak menyebutnya.
        string[] terlarang =
        {
            "Unit", "ResultForm", "NormalLow", "NormalHigh", "CriticalLow", "CriticalHigh",
            "GenderScope", "AgeCategoryId", "CitoTurnaroundMinutes", "SpecimenType",
            "ValueBoundId", "ReferenceRange"
        };

        foreach (var nama in terlarang)
        {
            Assert.DoesNotContain(nama, kolom);
        }

        // Dan atribut itu memang ada — di tabel milik Laboratorium, bukan hilang begitu saja.
        var labValueBound = context.Model.FindEntityType(typeof(LabValueBound));

        Assert.NotNull(labValueBound);

        var kolomLab = labValueBound!.GetProperties()
            .Select(x => x.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.Contains(nameof(LabValueBound.Unit), kolomLab);
        Assert.Contains(nameof(LabValueBound.NormalLow), kolomLab);
        Assert.Contains(nameof(LabValueBound.CriticalHigh), kolomLab);
        Assert.Contains(nameof(LabValueBound.CitoTurnaroundMinutes), kolomLab);
    }

    // =====================================================================
    // 5. AC-49 — data induk khusus Laboratorium tinggal di modul Laboratorium
    // =====================================================================

    [Fact]
    public void KeduaEntity_TinggalDiModulLaboratoriumDanHanyaMenunjukDataIndukGlobal()
    {
        using var context = CreateRelationalModelContext();

        const string modul = "QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models";

        Assert.Equal(modul, typeof(LabValueBound).Namespace);
        Assert.Equal(modul, typeof(LabValueOption).Namespace);

        var entityType = context.Model.FindEntityType(typeof(LabValueBound));

        Assert.NotNull(entityType);

        // MstProcedure dan MstAgeCategory tetap milik master-data; Laboratorium menunjuk ke
        // sana dan tidak menyalinnya ke dalam modulnya.
        var procedureFk = entityType!.GetForeignKeys()
            .Single(x => x.Properties.Single().Name == nameof(LabValueBound.ProcedureId));

        Assert.Equal(typeof(MstProcedure), procedureFk.PrincipalEntityType.ClrType);
        Assert.Equal(DeleteBehavior.Restrict, procedureFk.DeleteBehavior);

        var ageCategoryFk = entityType.GetForeignKeys()
            .Single(x => x.Properties.Single().Name == nameof(LabValueBound.AgeCategoryId));

        Assert.Equal(typeof(MstAgeCategory), ageCategoryFk.PrincipalEntityType.ClrType);
        Assert.Equal(DeleteBehavior.Restrict, ageCategoryFk.DeleteBehavior);
    }

    // =====================================================================
    // 6. Pemetaan sesuai kamus data
    // =====================================================================

    [Fact]
    public void LabValueBound_TerpetakanSesuaiKamusData()
    {
        using var context = CreateRelationalModelContext();

        var entityType = context.Model.FindEntityType(typeof(LabValueBound));

        Assert.NotNull(entityType);
        Assert.Equal("LabValueBound", entityType!.GetTableName());
        Assert.Equal("public", entityType.GetSchema());

        // Enum disimpan sebagai int, mengikuti seluruh enum modul Laboratorium.
        Assert.Equal(typeof(int), entityType.FindProperty(nameof(LabValueBound.ResultForm))!.GetProviderClrType());
        Assert.Equal(typeof(int), entityType.FindProperty(nameof(LabValueBound.GenderScope))!.GetProviderClrType());

        Assert.Equal(20, entityType.FindProperty(nameof(LabValueBound.Unit))!.GetMaxLength());

        foreach (var nama in new[]
        {
            nameof(LabValueBound.NormalLow),
            nameof(LabValueBound.NormalHigh),
            nameof(LabValueBound.CriticalLow),
            nameof(LabValueBound.CriticalHigh)
        })
        {
            var property = entityType.FindProperty(nama);

            Assert.NotNull(property);
            Assert.True(property!.IsNullable);
            Assert.Equal(18, property.GetPrecision());
            Assert.Equal(4, property.GetScale());
        }

        Assert.False(entityType.FindProperty(nameof(LabValueBound.ProcedureId))!.IsNullable);
        Assert.True(entityType.FindProperty(nameof(LabValueBound.AgeCategoryId))!.IsNullable);
        Assert.True(entityType.FindProperty(nameof(LabValueBound.CitoTurnaroundMinutes))!.IsNullable);
    }

    [Fact]
    public void LabValueOption_TerpetakanSesuaiKamusDataDanIkutTerhapusBersamaInduknya()
    {
        using var context = CreateRelationalModelContext();

        var entityType = context.Model.FindEntityType(typeof(LabValueOption));

        Assert.NotNull(entityType);
        Assert.Equal("LabValueOption", entityType!.GetTableName());
        Assert.Equal("public", entityType.GetSchema());

        Assert.Equal(20, entityType.FindProperty(nameof(LabValueOption.OptionCode))!.GetMaxLength());
        Assert.Equal(100, entityType.FindProperty(nameof(LabValueOption.OptionName))!.GetMaxLength());
        Assert.False(entityType.FindProperty(nameof(LabValueOption.OptionCode))!.IsNullable);
        Assert.False(entityType.FindProperty(nameof(LabValueOption.OptionName))!.IsNullable);

        // Cascade dipakai di sini karena pilihan tidak punya makna tanpa batas nilai induknya.
        var fk = entityType.GetForeignKeys()
            .Single(x => x.Properties.Single().Name == nameof(LabValueOption.ValueBoundId));

        Assert.Equal(typeof(LabValueBound), fk.PrincipalEntityType.ClrType);
        Assert.Equal(DeleteBehavior.Cascade, fk.DeleteBehavior);
    }

    [Fact]
    public void EnumBentukHasilDanPembatasJenisKelamin_MemuatNilaiYangDiputuskan()
    {
        // LAB-DEC-021: tepat dua bentuk hasil.
        Assert.Equal(2, Enum.GetValues<LabResultForm>().Length);
        Assert.Equal(1, (int)LabResultForm.Numeric);
        Assert.Equal(2, (int)LabResultForm.Choice);

        // BR-14: semua, pria, atau wanita.
        Assert.Equal(3, Enum.GetValues<LabGenderScope>().Length);
        Assert.Equal(1, (int)LabGenderScope.All);
        Assert.Equal(2, (int)LabGenderScope.Male);
        Assert.Equal(3, (int)LabGenderScope.Female);
    }

    // =====================================================================
    // Pembantu
    // =====================================================================

    private static ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"lab-value-bound-{Guid.NewGuid():N}")
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
            .UseNpgsql("Host=localhost;Database=lab_value_bound_model_only")
            .Options;

        return new ApplicationDbContext(options);
    }

    private static async Task<(Guid ProcedureId, Guid DewasaId, Guid AnakId)> SeedMasterDataAsync(
        ApplicationDbContext context)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];

        var procedure = new MstProcedure
        {
            Id = Guid.NewGuid(),
            ProcedureCode = $"LB-{suffix}",
            ProcedureName = "Hemoglobin",
            ProcedureType = "Laboratory",
            IsLaboratory = true,
            IsActive = true
        };

        var dewasa = new MstAgeCategory
        {
            Id = Guid.NewGuid(),
            AgeCategoryCode = $"ADT-{suffix}",
            AgeCategoryName = "Dewasa",
            MinAgeDays = 6570,
            IsActive = true
        };

        var anak = new MstAgeCategory
        {
            Id = Guid.NewGuid(),
            AgeCategoryCode = $"CHD-{suffix}",
            AgeCategoryName = "Anak",
            MinAgeDays = 0,
            MaxAgeDays = 6569,
            IsActive = true
        };

        context.Set<MstProcedure>().Add(procedure);
        context.Set<MstAgeCategory>().AddRange(dewasa, anak);

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        return (procedure.Id, dewasa.Id, anak.Id);
    }
}
