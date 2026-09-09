using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Models;
using QuilvianSystemBackend.Tests.Infrastructure;
using Xunit;

namespace QuilvianSystemBackend.Tests.Platform;

/// <summary>
/// Bukti untuk <c>PLT-BE-002</c> — tabel pencacah deret nomor bersama
/// (<c>NumNumberSeries</c>), blueprint <c>PLT-BP-001</c> revisi 3, kontrak <c>v1</c>.
/// </summary>
/// <remarks>
/// <para>
/// Yang dibuktikan di sini adalah <b>bentuk tabelnya</b>, bukan perilaku alokasinya. Alokasi
/// dikerjakan <c>PLT-BE-003</c>, dan durabilitasnya hanya dapat dibuktikan di PostgreSQL
/// sungguhan lewat <c>PLT-BE-004</c>.
/// </para>
/// <para>
/// <b>Batas yang jujur.</b> SQLite dipakai supaya model relasional benar-benar dibangun —
/// index dan constraint ikut terbentuk, tidak seperti provider InMemory. Tetapi SQLite
/// <b>bukan</b> PostgreSQL: <c>pg_advisory_xact_lock</c> tidak ada di sini, sehingga antrean
/// alokasi dan durabilitas pencacah <b>tidak</b> teruji pada berkas ini. Keduanya milik
/// <c>PLT-BE-004</c>.
/// </para>
/// </remarks>
public class NumNumberSeriesSchemaTests
{
    private static readonly Guid ActorUserId = Guid.Parse("55555555-5555-5555-5555-555555555555");

    /// <summary>
    /// <c>AC-PLT-011</c> — pasangan penanda deret dan periode dijaga <b>unik di tingkat
    /// database</b>, bukan hanya oleh pemeriksaan aplikasi.
    /// </summary>
    /// <remarks>
    /// Inilah penjaga terakhir <c>INV-PLT-001</c>. Kunci penasihat menyerialkan urutan
    /// pengambilan nomor, tetapi ia hidup di dalam aplikasi; index ini hidup di database dan
    /// menahan jalur yang lupa mengunci — termasuk jalur yang belum ditulis hari ini.
    /// </remarks>
    [Fact]
    public void PasanganDeretDanPeriode_KemBar_DitolakDatabase()
    {
        using var db = TestDatabase.Create();

        using (var context = db.CreateContext())
        {
            context.NumNumberSeries.Add(Deret("BBK_BLOOD_ORDER", "GLOBAL", 1));
            context.SaveChanges();
        }

        using (var context = db.CreateContext())
        {
            context.NumNumberSeries.Add(Deret("BBK_BLOOD_ORDER", "GLOBAL", 2));

            Assert.ThrowsAny<DbUpdateException>(() => context.SaveChanges());
        }

        using (var context = db.CreateContext())
        {
            Assert.Equal(1, context.NumNumberSeries.Count());
        }
    }

    /// <summary>
    /// Penanda deret yang sama pada periode <b>berbeda</b> adalah keadaan sah — itulah cara
    /// kebijakan pengulangan bekerja bagi deret lama yang kelak dipindahkan.
    /// </summary>
    [Fact]
    public void DeretSama_PeriodeBerbeda_Diterima()
    {
        using var db = TestDatabase.Create();
        using var context = db.CreateContext();

        context.NumNumberSeries.Add(Deret("BIL_INVOICE", "20260908", 12, NumberSeriesResetPolicies.Daily));
        context.NumNumberSeries.Add(Deret("BIL_INVOICE", "20260909", 3, NumberSeriesResetPolicies.Daily));
        context.SaveChanges();

        Assert.Equal(2, context.NumNumberSeries.Count());
    }

    /// <summary>
    /// Check constraint <c>CurrentValue &gt; 0</c>. Pencacah bernilai nol berarti deret belum
    /// pernah menerbitkan apa pun, dan barisnya semestinya belum ada.
    /// </summary>
    [Fact]
    public void PencacahNol_DitolakCheckConstraint()
    {
        using var db = TestDatabase.Create();
        using var context = db.CreateContext();

        context.NumNumberSeries.Add(Deret("BBK_PROVIDER_REQUEST", "GLOBAL", 0));

        Assert.ThrowsAny<DbUpdateException>(() => context.SaveChanges());
    }

    /// <summary>
    /// Check constraint <c>ResetPolicy IN (...)</c>. Kebijakan asing membuat periode pencacah
    /// tidak dapat dihitung, sehingga alokasi berikutnya akan memilih baris yang salah.
    /// </summary>
    [Fact]
    public void KebijakanPengulanganAsing_DitolakCheckConstraint()
    {
        using var db = TestDatabase.Create();
        using var context = db.CreateContext();

        context.NumNumberSeries.Add(Deret("BBK_PROCEDURE", "GLOBAL", 1, "WEEKLY"));

        Assert.ThrowsAny<DbUpdateException>(() => context.SaveChanges());
    }

    /// <summary>
    /// Keempat kebijakan sah benar-benar diterima database — penjaga agar daftar konstanta dan
    /// check constraint tidak berselisih diam-diam.
    /// </summary>
    [Theory]
    [InlineData(NumberSeriesResetPolicies.Never)]
    [InlineData(NumberSeriesResetPolicies.Yearly)]
    [InlineData(NumberSeriesResetPolicies.Monthly)]
    [InlineData(NumberSeriesResetPolicies.Daily)]
    public void KeempatKebijakanSah_Diterima(string kebijakan)
    {
        using var db = TestDatabase.Create();
        using var context = db.CreateContext();

        context.NumNumberSeries.Add(Deret($"UJI_{kebijakan}", "GLOBAL", 1, kebijakan));
        context.SaveChanges();

        Assert.Equal(1, context.NumNumberSeries.Count());
    }

    /// <summary>
    /// Daftar konstanta dan isi check constraint menyebut himpunan yang sama. Keduanya ditulis
    /// terpisah — satu di C#, satu di SQL — sehingga selisihnya perlu dijaga bukti, bukan
    /// ingatan.
    /// </summary>
    [Fact]
    public void DaftarKonstanta_BerisiTepatEmpatKebijakan()
    {
        Assert.Equal(4, NumberSeriesResetPolicies.All.Count);
        Assert.Contains(NumberSeriesResetPolicies.Never, NumberSeriesResetPolicies.All);
        Assert.Contains(NumberSeriesResetPolicies.Yearly, NumberSeriesResetPolicies.All);
        Assert.Contains(NumberSeriesResetPolicies.Monthly, NumberSeriesResetPolicies.All);
        Assert.Contains(NumberSeriesResetPolicies.Daily, NumberSeriesResetPolicies.All);
    }

    /// <summary>
    /// <c>FR-PLT-010</c> — tabel lahir kosong. Nol baris di-seed, dan itu disengaja: menyemai
    /// baris dengan nilai tebakan berisiko menerbitkan nomor yang sudah menempel pada catatan
    /// lain, sedangkan menyemainya dengan nol justru melanggar check constraint.
    /// </summary>
    [Fact]
    public void TabelLahirKosong_NolBarisDiSeed()
    {
        using var db = TestDatabase.Create();
        using var context = db.CreateContext();

        Assert.Equal(0, context.NumNumberSeries.Count());
    }

    private static NumNumberSeries Deret(
        string sequenceKey,
        string scopeKey,
        long currentValue,
        string resetPolicy = NumberSeriesResetPolicies.Never) => new()
    {
        Id = Guid.NewGuid(),
        SequenceKey = sequenceKey,
        ScopeKey = scopeKey,
        ResetPolicy = resetPolicy,
        CurrentValue = currentValue,
        LastAllocatedAt = DateTimeOffset.UtcNow,
        CreateDateTime = DateTime.UtcNow,
        CreateBy = ActorUserId
    };
}
