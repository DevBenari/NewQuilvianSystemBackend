using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using Xunit;

namespace QuilvianSystemBackend.Tests.HealthServices.MasterData;

/// <summary>
/// Bukti untuk <c>BE-EXT-01</c> dan <c>BE-EXT-02</c> — dua perubahan Master Data yang diminta
/// Laboratorium lewat <c>LAB-REQ-001</c> (<c>LAB-DEC-035</c>, <c>LAB-DEC-036</c>).
///
/// Yang dibuktikan di sini:
///   1. <c>MstProcedure</c> memiliki kolom klasifikasi disiplin, boleh kosong, dan ber-index;
///   2. kolom itu <b>satu-satunya</b> tambahan Laboratorium — satuan hasil, batas nilai, dan
///      jenis wadah tetap tidak masuk ke sana;
///   3. `MstReferralInstitution` dan `MstReferralDoctor` ada, dokter tertaut ke instansinya
///      dengan `Restrict`, dan kode instansi unik;
///   4. keduanya benar-benar dapat disimpan dan dibaca kembali.
///
/// Pemeriksaan bentuk dilakukan atas model relasional Npgsql yang dibangun di memori: tidak ada
/// koneksi yang dibuka dan tidak ada perintah database yang dijalankan.
/// </summary>
public class ReferralMasterDataTests
{
    // =====================================================================
    // BE-EXT-01 — kolom disiplin pada MstProcedure
    // =====================================================================

    [Fact]
    public void MstProcedure_MemilikiKolomDisiplinYangBolehKosongDanDisimpanSebagaiInt()
    {
        using var context = CreateRelationalModelContext();

        var entityType = context.Model.FindEntityType(typeof(MstProcedure));

        Assert.NotNull(entityType);

        var kolom = entityType!.FindProperty(nameof(MstProcedure.LabDiscipline));

        Assert.NotNull(kolom);
        Assert.True(kolom!.IsNullable);
        Assert.Equal(typeof(int), kolom.GetProviderClrType());
    }

    [Fact]
    public void MstProcedure_MemilikiIndexDisiplinYangHanyaMemuatBarisBermakna()
    {
        using var context = CreateRelationalModelContext();

        var entityType = context.Model.FindEntityType(typeof(MstProcedure));

        var index = entityType!.GetIndexes().SingleOrDefault(x =>
            x.Properties.Count == 1 &&
            x.Properties[0].Name == nameof(MstProcedure.LabDiscipline));

        Assert.NotNull(index);

        // Tindakan non-laboratorium tidak punya disiplin, sehingga tidak perlu ikut ke index.
        Assert.Equal("\"LabDiscipline\" IS NOT NULL", index!.GetFilter());
    }

    /// <summary>
    /// <c>LAB-DEC-036</c> menyebutnya <b>satu-satunya</b> tambahan Laboratorium pada tabel milik
    /// Master Data. Atribut operasional — satuan hasil, batas nilai, jenis wadah — tetap berada
    /// di tabel milik Laboratorium, dan uji ini yang menahannya agar tidak menyelinap masuk.
    /// </summary>
    [Fact]
    public void MstProcedure_TidakBertambahAtributOperasionalLaboratorium()
    {
        var properti = typeof(MstProcedure).GetProperties().Select(x => x.Name).ToList();

        foreach (var terlarang in new[]
                 {
                     "Unit", "NormalLow", "NormalHigh", "CriticalLow", "CriticalHigh",
                     "SpecimenType", "ContainerType", "ResultForm", "CitoTurnaroundMinutes"
                 })
        {
            Assert.DoesNotContain(terlarang, properti);
        }

        Assert.Contains(nameof(MstProcedure.LabDiscipline), properti);
    }

    // =====================================================================
    // BE-EXT-02 — dua data induk perujuk
    // =====================================================================

    [Fact]
    public void KodeInstansiPerujuk_UnikDiAntaraBarisYangBelumDihapus()
    {
        using var context = CreateRelationalModelContext();

        var entityType = context.Model.FindEntityType(typeof(MstReferralInstitution));

        Assert.NotNull(entityType);
        Assert.Equal("MstReferralInstitution", entityType!.GetTableName());

        var index = entityType.GetIndexes().Single(x =>
            x.Properties.Count == 1 &&
            x.Properties[0].Name == nameof(MstReferralInstitution.InstitutionCode));

        Assert.True(index.IsUnique);
        Assert.Equal("\"IsDelete\" = false", index.GetFilter());
    }

    [Fact]
    public void DokterPerujuk_TertautKeInstansinyaDenganRestrict()
    {
        using var context = CreateRelationalModelContext();

        var entityType = context.Model.FindEntityType(typeof(MstReferralDoctor));

        Assert.NotNull(entityType);
        Assert.Equal("MstReferralDoctor", entityType!.GetTableName());

        var relasi = Assert.Single(entityType.GetForeignKeys());

        Assert.Equal(typeof(MstReferralInstitution), relasi.PrincipalEntityType.ClrType);
        Assert.Equal(
            nameof(MstReferralDoctor.ReferralInstitutionId),
            relasi.Properties.Single().Name);

        // Instansi yang masih menaungi dokter tidak boleh terhapus; kunjungan lama yang
        // menunjuk dokter itu akan kehilangan asal-usulnya.
        Assert.Equal(DeleteBehavior.Restrict, relasi.DeleteBehavior);
    }

    /// <summary>
    /// Dokter perujuk sengaja terpisah dari data induk dokter rumah sakit. Uji ini menjaga
    /// pemisahan itu: ia tidak boleh punya jadwal, jasa medis, maupun penanda DPJP.
    /// </summary>
    [Fact]
    public void DokterPerujuk_TidakMemilikiAtributDokterInternal()
    {
        var properti = typeof(MstReferralDoctor).GetProperties().Select(x => x.Name).ToList();

        foreach (var terlarang in new[] { "ScheduleId", "DoctorScheduleId", "ServiceRuleId", "IsDpjp", "MedicalFee" })
            Assert.DoesNotContain(terlarang, properti);

        Assert.Contains(nameof(MstReferralDoctor.DoctorName), properti);
        Assert.Contains(nameof(MstReferralDoctor.ReferralInstitutionId), properti);
    }

    [Fact]
    public async Task InstansiDanDokterPerujuk_DapatDisimpanDanDibacaKembali()
    {
        await using var context = CreateInMemoryContext();

        var instansi = new MstReferralInstitution
        {
            Id = Guid.NewGuid(),
            InstitutionCode = "KLN-001",
            InstitutionName = "Klinik Sehat Sentosa",
            Address = "Jl. Merdeka 10",
            PhoneNumber = "0271-000000"
        };

        var dokter = new MstReferralDoctor
        {
            Id = Guid.NewGuid(),
            ReferralInstitutionId = instansi.Id,
            DoctorName = "dr. Rina Wijaya"
        };

        context.MstReferralInstitutions.Add(instansi);
        context.MstReferralDoctors.Add(dokter);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var tersimpan = await context.MstReferralDoctors
            .AsNoTracking()
            .Include(x => x.ReferralInstitution)
            .SingleAsync();

        Assert.Equal("dr. Rina Wijaya", tersimpan.DoctorName);
        Assert.True(tersimpan.IsActive);
        Assert.NotNull(tersimpan.ReferralInstitution);
        Assert.Equal("Klinik Sehat Sentosa", tersimpan.ReferralInstitution!.InstitutionName);
    }

    [Fact]
    public async Task JenisPemeriksaan_DapatDigolongkanKeDisiplinnya()
    {
        await using var context = CreateInMemoryContext();

        context.Set<MstProcedure>().AddRange(
            Procedure("LAB-HB", "Hemoglobin", LabDiscipline.ClinicalPathology),
            Procedure("LAB-PA", "Biopsi", LabDiscipline.AnatomicalPathology),
            Procedure("LAB-KUL", "Kultur darah", LabDiscipline.Microbiology),
            Procedure("RAD-TX", "Foto toraks", discipline: null, isLab: false));

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var mikro = await context.Set<MstProcedure>()
            .AsNoTracking()
            .Where(x => x.IsLaboratory && x.LabDiscipline == LabDiscipline.Microbiology)
            .ToListAsync();

        Assert.Equal("Kultur darah", Assert.Single(mikro).ProcedureName);

        // Tindakan non-laboratorium memang tidak bergolongan, dan itu bukan kesalahan data.
        var tanpaDisiplin = await context.Set<MstProcedure>()
            .AsNoTracking()
            .CountAsync(x => x.LabDiscipline == null);

        Assert.Equal(1, tanpaDisiplin);
    }

    // =====================================================================
    // Pembantu
    // =====================================================================

    private static MstProcedure Procedure(
        string kode, string nama, LabDiscipline? discipline, bool isLab = true) =>
        new()
        {
            Id = Guid.NewGuid(),
            ProcedureCode = kode,
            ProcedureName = nama,
            ProcedureType = isLab ? "Laboratory" : "Radiology",
            IsLaboratory = isLab,
            IsRadiology = !isLab,
            LabDiscipline = discipline,
            IsActive = true
        };

    private static ApplicationDbContext CreateRelationalModelContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=referral_master_model_only")
            .Options;

        return new ApplicationDbContext(options);
    }

    private static ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"referral-master-{Guid.NewGuid():N}")
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ApplicationDbContext(options);
    }
}
