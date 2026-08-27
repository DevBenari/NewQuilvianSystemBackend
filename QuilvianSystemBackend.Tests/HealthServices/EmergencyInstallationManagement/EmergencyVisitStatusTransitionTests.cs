using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Tests.HealthServices.EmergencyInstallationManagement;

/// <summary>
/// Membuktikan bahwa penjaga transisi status kunjungan IGD mengikuti kontrak
/// <c>state-transition-matrix.md</c> versi <c>0.3.0</c> bagian 1, 1.1, dan 1.2.
/// </summary>
/// <remarks>
/// Task <c>BE-IGD-018</c>, requirement <c>FR-IGD-015</c>, uji <c>AT-IGD-089</c> sebagian.
/// Kontrak yang dikunci ber-hash
/// <c>a41efd8d9adc87e1cf1eec2a9397b3521fdc0ebf935ccf0a19a5aa975b6c7c75</c>, disetujui
/// Rizki Gunawan 24 Agustus 2026 lewat <c>IGD-DEC-093</c>.
///
/// <para>
/// Test ini murni perhitungan — tidak menyentuh database maupun HTTP. Context InMemory hanya
/// dibutuhkan karena konstruktor service memintanya.
/// </para>
/// </remarks>
public class EmergencyVisitStatusTransitionTests
{
    /// <summary>
    /// Salinan langsung tabel kontrak bagian 1. Setiap ✓ pada tabel muncul di sini; setiap —
    /// tidak muncul. Bila kontrak berubah, tabel ini yang pertama harus ikut berubah.
    /// </summary>
    private static readonly Dictionary<EmergencyVisitStatus, EmergencyVisitStatus[]> KontrakTransisiSah = new()
    {
        [EmergencyVisitStatus.Arrived] = new[]
        {
            EmergencyVisitStatus.WaitingForTriage,
            EmergencyVisitStatus.InTreatment,
            EmergencyVisitStatus.Cancelled,
        },
        [EmergencyVisitStatus.WaitingForTriage] = new[]
        {
            EmergencyVisitStatus.Triaged,
            EmergencyVisitStatus.InTreatment,
            EmergencyVisitStatus.Cancelled,
        },
        [EmergencyVisitStatus.Triaged] = new[]
        {
            EmergencyVisitStatus.InTreatment,
            EmergencyVisitStatus.UnderObservation,
            EmergencyVisitStatus.AwaitingDisposition,
            EmergencyVisitStatus.Cancelled,
        },
        [EmergencyVisitStatus.InTreatment] = new[]
        {
            EmergencyVisitStatus.UnderObservation,
            EmergencyVisitStatus.AwaitingDisposition,
            EmergencyVisitStatus.Cancelled,
        },
        [EmergencyVisitStatus.UnderObservation] = new[]
        {
            EmergencyVisitStatus.InTreatment,
            EmergencyVisitStatus.AwaitingDisposition,
            EmergencyVisitStatus.Cancelled,
        },
        [EmergencyVisitStatus.AwaitingDisposition] = new[]
        {
            EmergencyVisitStatus.Disposed,
            EmergencyVisitStatus.InTreatment,
            EmergencyVisitStatus.Cancelled,
        },
        [EmergencyVisitStatus.Disposed] = new[]
        {
            EmergencyVisitStatus.Completed,
        },
        [EmergencyVisitStatus.Completed] = Array.Empty<EmergencyVisitStatus>(),
        [EmergencyVisitStatus.Cancelled] = Array.Empty<EmergencyVisitStatus>(),
    };

    /// <summary>
    /// Seluruh sembilan kali sembilan sel tabel kontrak, satu baris data per sel.
    /// </summary>
    public static TheoryData<EmergencyVisitStatus, EmergencyVisitStatus, bool> SeluruhSelMatriks()
    {
        var data = new TheoryData<EmergencyVisitStatus, EmergencyVisitStatus, bool>();

        foreach (var dari in KontrakTransisiSah.Keys)
        {
            foreach (var ke in KontrakTransisiSah.Keys)
            {
                data.Add(dari, ke, DiharapkanSah(dari, ke));
            }
        }

        return data;
    }

    /// <summary>
    /// Diagonal tabel kontrak tergambar sebagai —, tetapi bagian 1.2 hanya menyebut
    /// <c>Completed</c> → <c>Completed</c> yang ditolak. Kode memperlakukan transisi ke status
    /// yang sama sebagai tindakan idempoten yang diterima, dan roadmap <c>BE-IGD-018</c>
    /// memutuskan mengikuti kode. Bila Product/Domain Owner menghendaki seluruh diagonal
    /// ditolak, itu perubahan kontrak — bukan perbaikan test ini.
    /// </summary>
    private static bool DiharapkanSah(EmergencyVisitStatus dari, EmergencyVisitStatus ke)
    {
        if (dari == ke)
            return dari != EmergencyVisitStatus.Completed;

        return KontrakTransisiSah[dari].Contains(ke);
    }

    private static EmergencyVisitService BuatService()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"igd-transition-tests-{Guid.NewGuid():N}")
            .Options;

        return new EmergencyVisitService(
            new ApplicationDbContext(options),
            new EmergencyDocumentNumberService());
    }

    private static TrxEmergencyVisit BuatKunjungan(
        EmergencyVisitStatus status,
        DateTime? updateDateTime = null,
        Guid? updateBy = null)
    {
        return new TrxEmergencyVisit
        {
            Id = Guid.NewGuid(),
            VisitStatus = status,
            UpdateDateTime = updateDateTime,
            UpdateBy = updateBy ?? Guid.Empty,
        };
    }

    [Theory]
    [MemberData(nameof(SeluruhSelMatriks))]
    public void CanTransition_MengikutiSeluruhSelTabelKontrak(
        EmergencyVisitStatus dari,
        EmergencyVisitStatus ke,
        bool diharapkanSah)
    {
        var service = BuatService();

        var hasil = service.CanTransition(dari, ke);

        Assert.Equal(diharapkanSah, hasil);
    }

    [Theory]
    [MemberData(nameof(SeluruhSelMatriks))]
    public void TryApplyVisitStatus_MengikutiSeluruhSelTabelKontrak(
        EmergencyVisitStatus dari,
        EmergencyVisitStatus ke,
        bool diharapkanSah)
    {
        var service = BuatService();
        var kunjungan = BuatKunjungan(dari);

        var diterima = service.TryApplyVisitStatus(
            kunjungan,
            ke,
            Guid.NewGuid(),
            DateTime.UtcNow,
            out var penolakan);

        Assert.Equal(diharapkanSah, diterima);

        if (diharapkanSah)
        {
            Assert.Null(penolakan);
            Assert.Equal(ke, kunjungan.VisitStatus);
        }
        else
        {
            Assert.NotNull(penolakan);
            Assert.Equal(dari, kunjungan.VisitStatus);
        }
    }

    [Fact]
    public void TryApplyVisitStatus_TransisiSah_MenulisStatusDanJejakAudit()
    {
        var service = BuatService();
        var kunjungan = BuatKunjungan(EmergencyVisitStatus.WaitingForTriage);
        var pelaku = Guid.NewGuid();
        var waktu = new DateTime(2026, 8, 26, 9, 15, 0, DateTimeKind.Utc);

        var diterima = service.TryApplyVisitStatus(
            kunjungan,
            EmergencyVisitStatus.Triaged,
            pelaku,
            waktu,
            out var penolakan);

        Assert.True(diterima);
        Assert.Null(penolakan);
        Assert.Equal(EmergencyVisitStatus.Triaged, kunjungan.VisitStatus);
        Assert.Equal(waktu, kunjungan.UpdateDateTime);
        Assert.Equal(pelaku, kunjungan.UpdateBy);
    }

    [Fact]
    public void TryApplyVisitStatus_TransisiDitolak_TidakMenyentuhApaPun()
    {
        var service = BuatService();
        var waktuLama = new DateTime(2026, 8, 20, 1, 0, 0, DateTimeKind.Utc);
        var pelakuLama = Guid.NewGuid();
        var kunjungan = BuatKunjungan(EmergencyVisitStatus.InTreatment, waktuLama, pelakuLama);

        // Penilaian ulang tidak boleh mengembalikan pasien yang sedang ditangani ke Triaged.
        var diterima = service.TryApplyVisitStatus(
            kunjungan,
            EmergencyVisitStatus.Triaged,
            Guid.NewGuid(),
            new DateTime(2026, 8, 26, 9, 15, 0, DateTimeKind.Utc),
            out var penolakan);

        Assert.False(diterima);
        Assert.NotNull(penolakan);
        Assert.Equal(EmergencyVisitStatus.InTreatment, kunjungan.VisitStatus);
        Assert.Equal(waktuLama, kunjungan.UpdateDateTime);
        Assert.Equal(pelakuLama, kunjungan.UpdateBy);
    }

    [Fact]
    public void TryApplyVisitStatus_KunjunganSelesai_TidakDapatDibukaKembali()
    {
        var service = BuatService();

        foreach (var target in KontrakTransisiSah.Keys)
        {
            var kunjungan = BuatKunjungan(EmergencyVisitStatus.Completed);

            var diterima = service.TryApplyVisitStatus(
                kunjungan,
                target,
                Guid.NewGuid(),
                DateTime.UtcNow,
                out _);

            Assert.False(diterima);
            Assert.Equal(EmergencyVisitStatus.Completed, kunjungan.VisitStatus);
        }
    }

    [Fact]
    public void TryApplyVisitStatus_StatusSama_DiterimaTetapiJejakAuditTidakBergerak()
    {
        var service = BuatService();
        var waktuLama = new DateTime(2026, 8, 20, 1, 0, 0, DateTimeKind.Utc);
        var pelakuLama = Guid.NewGuid();
        var kunjungan = BuatKunjungan(EmergencyVisitStatus.InTreatment, waktuLama, pelakuLama);

        var diterima = service.TryApplyVisitStatus(
            kunjungan,
            EmergencyVisitStatus.InTreatment,
            Guid.NewGuid(),
            new DateTime(2026, 8, 26, 9, 15, 0, DateTimeKind.Utc),
            out var penolakan);

        Assert.True(diterima);
        Assert.Null(penolakan);
        Assert.Equal(waktuLama, kunjungan.UpdateDateTime);
        Assert.Equal(pelakuLama, kunjungan.UpdateBy);
    }

    [Fact]
    public void TryApplyVisitStatus_PesanPenolakanMenyebutKeduaStatus()
    {
        var service = BuatService();
        var kunjungan = BuatKunjungan(EmergencyVisitStatus.Disposed);

        service.TryApplyVisitStatus(
            kunjungan,
            EmergencyVisitStatus.Triaged,
            Guid.NewGuid(),
            DateTime.UtcNow,
            out var penolakan);

        Assert.NotNull(penolakan);
        Assert.Contains(nameof(EmergencyVisitStatus.Disposed), penolakan);
        Assert.Contains(nameof(EmergencyVisitStatus.Triaged), penolakan);
    }

    [Fact]
    public void TryApplyVisitStatus_KunjunganNull_Ditolak()
    {
        var service = BuatService();

        Assert.Throws<ArgumentNullException>(() => service.TryApplyVisitStatus(
            null!,
            EmergencyVisitStatus.Triaged,
            Guid.NewGuid(),
            DateTime.UtcNow,
            out _));
    }
}
