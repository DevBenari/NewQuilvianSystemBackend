using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Tests.RadiologyManagement;

/// <summary>
/// Metadata penyaring dan rekap untuk layar Radiologi.
///
/// Dua hal yang dijaga berkas ini. <b>Pertama</b>, metadata tidak boleh menjanjikan penyaring
/// yang tidak benar-benar diproses daftar — metadata yang berbohong menghasilkan tombol yang
/// terlihat bekerja tetapi tidak mengubah apa pun. <b>Kedua</b>, rekap study wajib menghitung
/// alat yang belum punya aturan keselamatan berlaku, karena gerbang bersifat fail-closed dan
/// alat semacam itu akan menolak seluruh pemeriksaannya.
/// </summary>
public sealed class RadiologyFilterAndSummaryTests
{
    /* ================================================================== *
     * Metadata pesanan
     * ================================================================== */

    [Fact]
    public void MetadataPesanan_MemuatSeluruhStatusBerlabelIndonesia()
    {
        using var db = Konteks();
        var metadata = OrderService(db).GetFilterMetadata();

        Assert.Equal(
            Enum.GetValues<RadOrderStatus>().Length,
            metadata.OrderStatuses.Count);

        Assert.All(metadata.OrderStatuses, x => Assert.False(string.IsNullOrWhiteSpace(x.Label)));

        // RAD-DEC-011 menetapkan Draft tidak dipakai. Nilainya tetap dikirim supaya angka
        // status lain tidak bergeser, tetapi labelnya wajib mengatakannya apa adanya.
        var draf = metadata.OrderStatuses.Single(x => x.Name == nameof(RadOrderStatus.Draft));
        Assert.Contains("tidak dipakai", draf.Label);
    }

    [Fact]
    public void MetadataPesanan_TidakMenjanjikanHalamanYangBelumAda()
    {
        using var db = Konteks();
        var metadata = OrderService(db).GetFilterMetadata();

        // Daftar pesanan belum memakai PagedResult. Selama itu belum ada, metadata tidak boleh
        // menawarkan pilihan jumlah baris kepada layar.
        Assert.DoesNotContain(
            metadata.QueryParameters,
            x => x.Name is "pageNumber" or "pageSize");

        // Sebaliknya, setiap parameter yang disebut memang dilayani daftar.
        Assert.Contains(metadata.QueryParameters, x => x.Name == "encounterId");
        Assert.Contains(metadata.QueryParameters, x => x.Name == "sortBy");
        Assert.Contains(metadata.QueryParameters, x => x.Name == "sortDirection");
    }

    [Fact]
    public async Task PengurutanYangDijanjikanMetadata_BenarBenarDiproses()
    {
        await using var db = Konteks();
        var master = await SeedMasterAsync(db);
        var kunjungan = Guid.NewGuid();

        var lama = Pesanan(kunjungan, RadOrderStatus.Completed, master, new DateTime(2026, 1, 1));
        var baru = Pesanan(kunjungan, RadOrderStatus.Requested, master, new DateTime(2026, 9, 1));
        db.RadOrders.AddRange(lama, baru);
        await db.SaveChangesAsync();

        var service = OrderService(db);
        var metadata = service.GetFilterMetadata();

        var menurun = await service.GetListAsync(kunjungan, "createDateTime", "desc");
        var menaik = await service.GetListAsync(kunjungan, "createDateTime", "asc");

        Assert.Contains(metadata.SortOptions, x => x.Value == "createDateTime");
        Assert.Equal(baru.Id, menurun.First().Id);
        Assert.Equal(lama.Id, menaik.First().Id);
    }

    [Fact]
    public async Task UrutanBawaanTidakBerubah()
    {
        // Menambah pengurutan tidak boleh mengubah jawaban bagi pemanggil yang sudah ada dan
        // tidak mengirim parameter apa pun.
        await using var db = Konteks();
        var master = await SeedMasterAsync(db);
        var kunjungan = Guid.NewGuid();

        var lama = Pesanan(kunjungan, RadOrderStatus.Completed, master, new DateTime(2026, 1, 1));
        var baru = Pesanan(kunjungan, RadOrderStatus.Requested, master, new DateTime(2026, 9, 1));
        db.RadOrders.AddRange(lama, baru);
        await db.SaveChangesAsync();

        var hasil = await OrderService(db).GetListAsync(kunjungan);

        Assert.Equal(baru.Id, hasil.First().Id);
    }

    /* ================================================================== *
     * Rekap pesanan
     * ================================================================== */

    [Fact]
    public async Task RekapPesanan_MenghitungPerStatusDanBebanYangBelumTersentuh()
    {
        await using var db = Konteks();
        var kunjungan = Guid.NewGuid();

        db.RadOrders.AddRange(
            Pesanan(kunjungan, RadOrderStatus.Requested),
            Pesanan(kunjungan, RadOrderStatus.Requested),
            Pesanan(kunjungan, RadOrderStatus.Accepted),
            Pesanan(kunjungan, RadOrderStatus.Scheduled),
            Pesanan(kunjungan, RadOrderStatus.Completed),
            Pesanan(kunjungan, RadOrderStatus.Cancelled));
        await db.SaveChangesAsync();

        var rekap = await OrderService(db).GetSummaryAsync();

        Assert.Equal(6, rekap.TotalPesanan);
        Assert.Equal(2, rekap.Diminta);
        Assert.Equal(1, rekap.Diterima);
        Assert.Equal(1, rekap.Dijadwalkan);
        Assert.Equal(1, rekap.Selesai);
        Assert.Equal(1, rekap.Dibatalkan);

        // Dua diminta, satu diterima, satu dijadwalkan.
        Assert.Equal(4, rekap.BelumDikerjakan);
    }

    [Fact]
    public async Task RekapPesanan_TidakMenghitungBarisYangSudahDihapus()
    {
        await using var db = Konteks();
        var kunjungan = Guid.NewGuid();

        var terhapus = Pesanan(kunjungan, RadOrderStatus.Requested);
        terhapus.IsDelete = true;

        db.RadOrders.AddRange(Pesanan(kunjungan, RadOrderStatus.Requested), terhapus);
        await db.SaveChangesAsync();

        var rekap = await OrderService(db).GetSummaryAsync();

        Assert.Equal(1, rekap.TotalPesanan);
    }

    /* ================================================================== *
     * Metadata dan rekap study
     * ================================================================== */

    [Fact]
    public void MetadataStudy_MemuatKeempatKeadaanAturanKeselamatan()
    {
        using var db = Konteks();
        var metadata = StudyService(db).GetFilterMetadata();

        Assert.Equal(4, metadata.SafetyRuleStatuses.Count);

        var berlaku = metadata.SafetyRuleStatuses
            .Single(x => x.Name == nameof(RadSafetyRuleStatus.Active));

        // Layar harus dapat membedakan mana yang benar-benar menahan pasien.
        Assert.Contains("dinilai gerbang", berlaku.Label);
    }

    [Fact]
    public async Task RekapStudy_MenghitungAlatYangBelumPunyaAturanBerlaku()
    {
        // Inilah angka yang paling penting pada rekap ini. Alat yang terhitung di sini akan
        // menolak seluruh pemeriksaannya, dan admin berhak tahu sebelum pasien dipanggil.
        await using var db = Konteks();

        var butir = new MstRadSafetyRequirement
        {
            Id = Guid.NewGuid(),
            RequirementCode = "PREGNANCY_SCREENING",
            RequirementName = "Skrining kehamilan",
            IsActive = true,
        };

        var siap = new MstRadModality
        {
            Id = Guid.NewGuid(),
            ModalityCode = "CT",
            ModalityName = "CT-Scan",
            IsActive = true,
        };

        var belumSiap = new MstRadModality
        {
            Id = Guid.NewGuid(),
            ModalityCode = "USG",
            ModalityName = "Ultrasonografi",
            IsActive = true,
        };

        db.MstRadSafetyRequirements.Add(butir);
        db.MstRadModalities.AddRange(siap, belumSiap);

        db.MstRadModalitySafetyRules.AddRange(
            AturanKeselamatan(siap.Id, butir.Id, RadSafetyRuleStatus.Active),

            // Draf tidak menyiapkan alat apa pun. Alat ini tetap terhitung belum siap.
            AturanKeselamatan(belumSiap.Id, butir.Id, RadSafetyRuleStatus.Draft));

        await db.SaveChangesAsync();

        var rekap = await StudyService(db).GetSummaryAsync();

        Assert.Equal(1, rekap.AlatTanpaAturanKeselamatanAktif);
    }

    [Fact]
    public async Task RekapStudy_MemisahkanYangMenungguGerbangKeselamatan()
    {
        await using var db = Konteks();
        var pesanan = Guid.NewGuid();

        db.RadStudies.AddRange(
            Study(pesanan, RadStudyStatus.Planned, 1),
            Study(pesanan, RadStudyStatus.PatientVerified, 2),
            Study(pesanan, RadStudyStatus.PatientVerified, 3),
            Study(pesanan, RadStudyStatus.SafetyCleared, 4),
            Study(pesanan, RadStudyStatus.QualityAccepted, 5));
        await db.SaveChangesAsync();

        var rekap = await StudyService(db).GetSummaryAsync();

        Assert.Equal(5, rekap.TotalStudy);
        Assert.Equal(1, rekap.Direncanakan);
        Assert.Equal(2, rekap.IdentitasTerverifikasi);
        Assert.Equal(1, rekap.LolosGerbangKeselamatan);
        Assert.Equal(1, rekap.MutuDiterima);

        // Identitas sudah benar bukan berarti pemeriksaannya sudah aman dijalankan.
        Assert.Equal(2, rekap.MenungguGerbangKeselamatan);
    }

    /* ================================================================== *
     * Perancah
     * ================================================================== */

    private static ApplicationDbContext Konteks()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"rad-filter-summary-{Guid.NewGuid():N}")
            .Options;

        return new ApplicationDbContext(options);
    }

    private static HttpContextAccessor Accessor() => new()
    {
        HttpContext = new DefaultHttpContext(),
    };

    private static LoggerService Logger(HttpContextAccessor accessor) =>
        new(NullLogger<LoggerService>.Instance, accessor);

    private static RadOrderService OrderService(ApplicationDbContext db)
    {
        var accessor = Accessor();

        return new RadOrderService(db, accessor, Logger(accessor));
    }

    private static RadStudyService StudyService(ApplicationDbContext db)
    {
        var accessor = Accessor();
        var logger = Logger(accessor);

        return new RadStudyService(
            db,
            new ClinicalMilestoneFactProducer(db, new BillingFolioService(db), logger),
            accessor,
            logger);
    }

    /// <summary>
    /// Alat dan pemeriksaan yang benar-benar ada.
    ///
    /// Relasi <c>Procedure</c> dan <c>Modality</c> pada <c>RadOrder</c> bersifat wajib,
    /// sehingga proyeksi daftar menggabungkannya sebagai inner join. Pesanan yang menunjuk
    /// baris master yang tidak ada akan hilang dari daftar — itu perilaku query yang benar,
    /// dan perancah uji harus mengikutinya alih-alih menyiasatinya.
    /// </summary>
    private static async Task<(Guid ProcedureId, Guid ModalityId)> SeedMasterAsync(
        ApplicationDbContext db)
    {
        var pemeriksaan = new MstProcedure
        {
            Id = Guid.NewGuid(),
            ProcedureCode = "RD-TEST",
            ProcedureName = "Pemeriksaan Radiologi Uji",
            ProcedureType = "Radiology",
            IsActive = true,
        };

        var alat = new MstRadModality
        {
            Id = Guid.NewGuid(),
            ModalityCode = "CT-TEST",
            ModalityName = "CT-Scan Uji",
            IsActive = true,
        };

        db.Add(pemeriksaan);
        db.MstRadModalities.Add(alat);
        await db.SaveChangesAsync();

        return (pemeriksaan.Id, alat.Id);
    }

    private static RadOrder Pesanan(
        Guid encounterId,
        RadOrderStatus status,
        (Guid ProcedureId, Guid ModalityId)? master = null,
        DateTime? dibuat = null) => new()
    {
        Id = Guid.NewGuid(),
        EncounterId = encounterId,
        ProcedureId = master?.ProcedureId ?? Guid.NewGuid(),
        ModalityId = master?.ModalityId ?? Guid.NewGuid(),
        OrderStatus = status,
        CreateDateTime = dibuat ?? DateTime.UtcNow,
    };

    private static RadStudy Study(Guid radOrderId, RadStudyStatus status, int urutan) => new()
    {
        Id = Guid.NewGuid(),
        RadOrderId = radOrderId,
        EncounterId = Guid.NewGuid(),
        ProcedureId = Guid.NewGuid(),
        ModalityId = Guid.NewGuid(),
        StudySequence = urutan,
        StudyNumber = $"RAD-TEST-{urutan:D3}",
        StudyStatus = status,
    };

    private static MstRadModalitySafetyRule AturanKeselamatan(
        Guid modalityId,
        Guid requirementId,
        RadSafetyRuleStatus status) => new()
    {
        Id = Guid.NewGuid(),
        ModalityId = modalityId,
        SafetyRequirementId = requirementId,
        IsMandatory = true,
        RuleStatus = status,
        IsActive = status == RadSafetyRuleStatus.Active,
        RuleVersion = 1,
        EffectiveFrom = DateTime.UtcNow.AddDays(-1),
    };
}
