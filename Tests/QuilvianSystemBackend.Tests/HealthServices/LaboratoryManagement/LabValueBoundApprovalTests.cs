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
/// Bukti untuk <c>BE-LAB-03</c> — riwayat dan pengajuan perubahan batas kritis
/// (<c>FR-03.4</c>, <c>FR-03.5</c>; <c>LAB-DEC-023</c>; <c>LAB-STATE-v1</c> r2 bagian 4).
///
/// Yang dibuktikan di sini:
///   1. <c>AC-34</c> — satu baris riwayat memuat kolom yang berubah, nilai lama, nilai baru,
///      pelaku, penyetuju, waktu, dan alasan.
///   2. <c>AC-33</c> — pengajuan perubahan batas kritis berstatus <c>Submitted</c> sementara
///      batas yang berlaku pada <c>LabValueBound</c> <b>tidak berubah sama sekali</b>.
///   3. Perubahan batas normal menerbitkan riwayat <b>tanpa</b> penyetuju, sedangkan perubahan
///      batas kritis menerbitkan riwayat <b>dengan</b> penyetuju.
///   4. Riwayat benar-benar permanen: mengubah baris yang sudah tersimpan ditolak lapisan
///      penyimpanan.
///   5. Penamaan kedua entity tidak memakai awalan <c>Trx</c> — risiko yang disebut roadmap
///      dan dilarang QBE-NAM-001 untuk kode baru.
///   6. Pemetaan kedua tabel sesuai <c>erd/data-dictionary.md</c> bagian 7, 8, 11.4, dan 11.5.
/// </summary>
/// <remarks>
/// Dua jenis context dipakai, sama seperti pada <c>LabValueBoundTests</c>.
///
/// Provider InMemory untuk bukti penyimpanan, supaya pengujian berjalan tanpa database mana pun.
/// Model relasional Npgsql — dibangun di memori, tanpa satu pun koneksi dibuka — untuk membaca
/// bentuk schema yang sebenarnya: nama tabel, tipe kolom, index, dan perilaku hapus.
///
/// Penegakan transisi status, larangan menyetujui pengajuan sendiri (<c>VAL-33</c>), penolakan
/// pengajuan kedua (<c>VAL-32</c>), dan kode status <c>403</c>/<c>409</c> adalah pekerjaan
/// endpoint pada <c>BE-LAB-05</c>. Yang menjadi cakupan <c>BE-LAB-03</c> adalah bentuk data yang
/// membuat seluruh penegakan itu mungkin.
/// </remarks>
public class LabValueBoundApprovalTests
{
    private static readonly Guid KepalaInstalasi = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid PenyetujuKlinis = Guid.Parse("22222222-2222-2222-2222-222222222222");

    // =====================================================================
    // 1. AC-33 — pengajuan tertahan, batas yang berlaku tidak berubah
    // =====================================================================

    [Fact]
    public async Task PengajuanPerubahanBatasKritis_TertahanSementaraBatasLamaTidakBerubah()
    {
        await using var context = CreateInMemoryContext();
        var boundId = await SeedKaliumAsync(context);

        // Kepala instalasi mengusulkan batas kritis atas Kalium naik dari 6,0 ke 8,0.
        context.LabValueBoundChangeRequests.Add(new LabValueBoundChangeRequest
        {
            ValueBoundId = boundId,
            ProposedCriticalHigh = 8.0m,
            RequestReason = "Peringatan nilai kritis dinilai terlalu sering muncul.",
            RequestedByUserId = KepalaInstalasi,
            RequestedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var pengajuan = await context.LabValueBoundChangeRequests.AsNoTracking().SingleAsync();

        Assert.Equal(LabBoundChangeStatus.Submitted, pengajuan.RequestStatus);
        Assert.Equal(8.0m, pengajuan.ProposedCriticalHigh);
        Assert.Equal(KepalaInstalasi, pengajuan.RequestedByUserId);

        // Belum diputuskan siapa pun.
        Assert.Null(pengajuan.DecidedByUserId);
        Assert.Null(pengajuan.DecidedAt);
        Assert.Null(pengajuan.DecisionNote);

        // Inti AC-33: batas yang berlaku sama sekali tidak bergerak.
        var batas = await context.LabValueBounds.AsNoTracking().SingleAsync(x => x.Id == boundId);

        Assert.Equal(6.0m, batas.CriticalHigh);
        Assert.Equal(2.5m, batas.CriticalLow);

        // Pasien dengan Kalium 7,2 mmol/L masih melewati batas kritis yang berlaku, karena
        // usulan 8,0 belum berlaku apa pun.
        Assert.True(7.2m > batas.CriticalHigh);
    }

    // =====================================================================
    // 2. AC-34 — riwayat memuat tujuh hal yang dituntut
    // =====================================================================

    [Fact]
    public async Task PerubahanBatasNormal_MenerbitkanSatuBarisRiwayatTanpaPenyetuju()
    {
        await using var context = CreateInMemoryContext();
        var boundId = await SeedKaliumAsync(context);
        var saat = DateTime.UtcNow;

        context.LabValueBoundHistories.Add(new LabValueBoundHistory
        {
            ValueBoundId = boundId,
            ChangedField = nameof(LabValueBound.NormalHigh),
            OldValue = "5,1",
            NewValue = "5,3",
            ActorUserId = KepalaInstalasi,
            ChangeReason = "Penyesuaian setelah penggantian alat.",
            OccurredAt = saat
        });

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var riwayat = await context.LabValueBoundHistories.AsNoTracking().SingleAsync();

        Assert.Equal(boundId, riwayat.ValueBoundId);
        Assert.Equal("NormalHigh", riwayat.ChangedField);
        Assert.Equal("5,1", riwayat.OldValue);
        Assert.Equal("5,3", riwayat.NewValue);
        Assert.Equal(KepalaInstalasi, riwayat.ActorUserId);
        Assert.Equal("Penyesuaian setelah penggantian alat.", riwayat.ChangeReason);
        Assert.Equal(saat, riwayat.OccurredAt);

        // Batas normal tidak menempuh persetujuan klinis, jadi penyetujunya memang kosong —
        // dan kekosongan itu sah, bukan data yang belum diisi.
        Assert.Null(riwayat.ApprovedByUserId);
    }

    [Fact]
    public async Task PerubahanBatasKritisYangDisetujui_MenerbitkanRiwayatBesertaPenyetujunya()
    {
        await using var context = CreateInMemoryContext();
        var boundId = await SeedKaliumAsync(context);

        context.LabValueBoundHistories.Add(new LabValueBoundHistory
        {
            ValueBoundId = boundId,
            ChangedField = nameof(LabValueBound.CriticalHigh),
            OldValue = "6,0",
            NewValue = "8,0",
            ActorUserId = KepalaInstalasi,
            ApprovedByUserId = PenyetujuKlinis,
            ChangeReason = "Disetujui pihak klinis setelah tinjauan.",
            OccurredAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var riwayat = await context.LabValueBoundHistories.AsNoTracking().SingleAsync();

        // Tujuh hal yang dituntut AC-34, lengkap dalam satu baris.
        Assert.Equal("CriticalHigh", riwayat.ChangedField);
        Assert.Equal("6,0", riwayat.OldValue);
        Assert.Equal("8,0", riwayat.NewValue);
        Assert.Equal(KepalaInstalasi, riwayat.ActorUserId);
        Assert.Equal(PenyetujuKlinis, riwayat.ApprovedByUserId);
        Assert.NotNull(riwayat.ChangeReason);
        Assert.NotEqual(default, riwayat.OccurredAt);

        // Pelaku dan penyetuju wajib dua orang berbeda; itulah gunanya dua kolom terpisah.
        Assert.NotEqual(riwayat.ActorUserId, riwayat.ApprovedByUserId!.Value);
    }

    [Fact]
    public async Task SatuBatasNilai_DapatMemilikiBeberapaBarisRiwayatBerurutanWaktu()
    {
        await using var context = CreateInMemoryContext();
        var boundId = await SeedKaliumAsync(context);
        var awal = DateTime.UtcNow;

        context.LabValueBoundHistories.AddRange(
            new LabValueBoundHistory
            {
                ValueBoundId = boundId,
                ChangedField = nameof(LabValueBound.Unit),
                OldValue = "mmol/L",
                NewValue = "mEq/L",
                ActorUserId = KepalaInstalasi,
                OccurredAt = awal
            },
            new LabValueBoundHistory
            {
                ValueBoundId = boundId,
                ChangedField = nameof(LabValueBound.CriticalHigh),
                OldValue = "6,0",
                NewValue = "8,0",
                ActorUserId = KepalaInstalasi,
                ApprovedByUserId = PenyetujuKlinis,
                OccurredAt = awal.AddMinutes(5)
            });

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var riwayat = await context.LabValueBoundHistories
            .AsNoTracking()
            .Where(x => x.ValueBoundId == boundId)
            .OrderBy(x => x.OccurredAt)
            .ToListAsync();

        Assert.Equal(2, riwayat.Count);
        Assert.Equal(new[] { "Unit", "CriticalHigh" }, riwayat.Select(x => x.ChangedField));

        // Hanya perubahan batas kritis yang punya penyetuju.
        Assert.Null(riwayat[0].ApprovedByUserId);
        Assert.Equal(PenyetujuKlinis, riwayat[1].ApprovedByUserId);
    }

    // =====================================================================
    // 3. Riwayat benar-benar permanen
    // =====================================================================

    [Fact]
    public async Task MengubahBarisRiwayatYangSudahTersimpan_Ditolak()
    {
        await using var context = CreateInMemoryContext();
        var boundId = await SeedKaliumAsync(context);

        var riwayat = new LabValueBoundHistory
        {
            ValueBoundId = boundId,
            ChangedField = nameof(LabValueBound.CriticalHigh),
            OldValue = "6,0",
            NewValue = "8,0",
            ActorUserId = KepalaInstalasi,
            ApprovedByUserId = PenyetujuKlinis,
            OccurredAt = DateTime.UtcNow
        };

        context.LabValueBoundHistories.Add(riwayat);
        await context.SaveChangesAsync();

        // Seseorang mencoba menghaluskan jejak: nilai lama diubah supaya kenaikannya tampak
        // wajar.
        riwayat.OldValue = "7,9";

        await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());

        context.ChangeTracker.Clear();

        var tersimpan = await context.LabValueBoundHistories.AsNoTracking().SingleAsync();

        Assert.Equal("6,0", tersimpan.OldValue);
    }

    // =====================================================================
    // 4. Penamaan — risiko yang disebut roadmap
    // =====================================================================

    [Fact]
    public void KeduaEntityBaru_TidakMemakaiAwalanTrx()
    {
        using var context = CreateRelationalModelContext();

        foreach (var clr in new[] { typeof(LabValueBoundChangeRequest), typeof(LabValueBoundHistory) })
        {
            Assert.StartsWith("Lab", clr.Name, StringComparison.Ordinal);
            Assert.DoesNotContain("Trx", clr.Name, StringComparison.Ordinal);

            var entityType = context.Model.FindEntityType(clr);

            Assert.NotNull(entityType);

            var tabel = entityType!.GetTableName();

            Assert.Equal(clr.Name, tabel);
            Assert.DoesNotContain("Trx", tabel!, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void LabBoundChangeStatus_MemuatEmpatStatusSesuaiStateMatrix()
    {
        Assert.Equal(4, Enum.GetValues<LabBoundChangeStatus>().Length);
        Assert.Equal(1, (int)LabBoundChangeStatus.Submitted);
        Assert.Equal(2, (int)LabBoundChangeStatus.Approved);
        Assert.Equal(3, (int)LabBoundChangeStatus.Rejected);
        Assert.Equal(4, (int)LabBoundChangeStatus.Withdrawn);

        // Status lahir sebuah pengajuan adalah Submitted.
        Assert.Equal(LabBoundChangeStatus.Submitted, new LabValueBoundChangeRequest().RequestStatus);
    }

    // =====================================================================
    // 5. Pemetaan sesuai kamus data
    // =====================================================================

    [Fact]
    public void LabValueBoundChangeRequest_TerpetakanSesuaiKamusData()
    {
        using var context = CreateRelationalModelContext();

        var entityType = context.Model.FindEntityType(typeof(LabValueBoundChangeRequest));

        Assert.NotNull(entityType);
        Assert.Equal("LabValueBoundChangeRequest", entityType!.GetTableName());
        Assert.Equal("public", entityType.GetSchema());

        Assert.Equal(typeof(int), entityType.FindProperty(nameof(LabValueBoundChangeRequest.RequestStatus))!.GetProviderClrType());
        Assert.Equal(500, entityType.FindProperty(nameof(LabValueBoundChangeRequest.ProposedCriticalOptionCodes))!.GetMaxLength());
        Assert.Equal(1000, entityType.FindProperty(nameof(LabValueBoundChangeRequest.RequestReason))!.GetMaxLength());
        Assert.Equal(1000, entityType.FindProperty(nameof(LabValueBoundChangeRequest.DecisionNote))!.GetMaxLength());
        Assert.False(entityType.FindProperty(nameof(LabValueBoundChangeRequest.RequestReason))!.IsNullable);

        foreach (var nama in new[]
        {
            nameof(LabValueBoundChangeRequest.ProposedCriticalLow),
            nameof(LabValueBoundChangeRequest.ProposedCriticalHigh)
        })
        {
            var property = entityType.FindProperty(nama);

            Assert.NotNull(property);
            Assert.True(property!.IsNullable);
            Assert.Equal(18, property.GetPrecision());
            Assert.Equal(4, property.GetScale());
        }

        // Pemutus dan waktunya kosong selama pengajuan belum diputuskan.
        Assert.True(entityType.FindProperty(nameof(LabValueBoundChangeRequest.DecidedByUserId))!.IsNullable);
        Assert.True(entityType.FindProperty(nameof(LabValueBoundChangeRequest.DecidedAt))!.IsNullable);

        // CAP-17 — perlindungan dua pemutus bertindak bersamaan.
        Assert.True(entityType.FindProperty(nameof(LabValueBoundChangeRequest.Version))!.IsConcurrencyToken);

        Assert.Contains(entityType.GetIndexes(), x =>
            x.Properties.Count == 1 && x.Properties[0].Name == nameof(LabValueBoundChangeRequest.ValueBoundId));
        Assert.Contains(entityType.GetIndexes(), x =>
            x.Properties.Count == 1 && x.Properties[0].Name == nameof(LabValueBoundChangeRequest.RequestStatus));

        var fk = entityType.GetForeignKeys()
            .Single(x => x.Properties.Single().Name == nameof(LabValueBoundChangeRequest.ValueBoundId));

        Assert.Equal(typeof(LabValueBound), fk.PrincipalEntityType.ClrType);
        Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior);
    }

    [Fact]
    public void LabValueBoundHistory_TerpetakanSesuaiKamusDataDanBerperilakuTolakUbah()
    {
        using var context = CreateRelationalModelContext();

        var entityType = context.Model.FindEntityType(typeof(LabValueBoundHistory));

        Assert.NotNull(entityType);
        Assert.Equal("LabValueBoundHistory", entityType!.GetTableName());
        Assert.Equal("public", entityType.GetSchema());

        Assert.Equal(100, entityType.FindProperty(nameof(LabValueBoundHistory.ChangedField))!.GetMaxLength());
        Assert.Equal(200, entityType.FindProperty(nameof(LabValueBoundHistory.OldValue))!.GetMaxLength());
        Assert.Equal(200, entityType.FindProperty(nameof(LabValueBoundHistory.NewValue))!.GetMaxLength());
        Assert.Equal(1000, entityType.FindProperty(nameof(LabValueBoundHistory.ChangeReason))!.GetMaxLength());
        Assert.False(entityType.FindProperty(nameof(LabValueBoundHistory.ChangedField))!.IsNullable);
        Assert.True(entityType.FindProperty(nameof(LabValueBoundHistory.ApprovedByUserId))!.IsNullable);

        // Seluruh kolom fakta menolak diubah sesudah tersimpan.
        foreach (var nama in new[]
        {
            nameof(LabValueBoundHistory.ValueBoundId),
            nameof(LabValueBoundHistory.ChangedField),
            nameof(LabValueBoundHistory.OldValue),
            nameof(LabValueBoundHistory.NewValue),
            nameof(LabValueBoundHistory.ActorUserId),
            nameof(LabValueBoundHistory.ApprovedByUserId),
            nameof(LabValueBoundHistory.ChangeReason),
            nameof(LabValueBoundHistory.OccurredAt)
        })
        {
            Assert.Equal(
                PropertySaveBehavior.Throw,
                entityType.FindProperty(nama)!.GetAfterSaveBehavior());
        }

        Assert.Contains(entityType.GetIndexes(), x =>
            x.Properties.Select(p => p.Name).SequenceEqual(new[]
            {
                nameof(LabValueBoundHistory.ValueBoundId),
                nameof(LabValueBoundHistory.OccurredAt)
            }));

        var fk = entityType.GetForeignKeys()
            .Single(x => x.Properties.Single().Name == nameof(LabValueBoundHistory.ValueBoundId));

        Assert.Equal(typeof(LabValueBound), fk.PrincipalEntityType.ClrType);
        Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior);
    }

    // =====================================================================
    // Pembantu
    // =====================================================================

    private static ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"lab-bound-approval-{Guid.NewGuid():N}")
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ApplicationDbContext(options);
    }

    private static ApplicationDbContext CreateRelationalModelContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=lab_bound_approval_model_only")
            .Options;

        return new ApplicationDbContext(options);
    }

    /// <summary>
    /// Menyiapkan satu batas nilai Kalium yang berlaku: normal 3,5–5,1 mmol/L, kritis 2,5–6,0.
    /// Angka ini diambil dari contoh BR-17 supaya bukti dapat dicocokkan dengan keputusannya.
    /// </summary>
    private static async Task<Guid> SeedKaliumAsync(ApplicationDbContext context)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];

        var procedure = new MstProcedure
        {
            Id = Guid.NewGuid(),
            ProcedureCode = $"LB-{suffix}",
            ProcedureName = "Kalium",
            ProcedureType = "Laboratory",
            IsLaboratory = true,
            IsActive = true
        };

        var batas = new LabValueBound
        {
            Id = Guid.NewGuid(),
            ProcedureId = procedure.Id,
            ResultForm = LabResultForm.Numeric,
            Unit = "mmol/L",
            GenderScope = LabGenderScope.All,
            NormalLow = 3.5m,
            NormalHigh = 5.1m,
            CriticalLow = 2.5m,
            CriticalHigh = 6.0m
        };

        context.Set<MstProcedure>().Add(procedure);
        context.LabValueBounds.Add(batas);

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        return batas.Id;
    }
}
