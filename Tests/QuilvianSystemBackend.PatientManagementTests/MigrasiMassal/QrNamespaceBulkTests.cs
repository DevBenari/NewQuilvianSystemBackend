using System.Text;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services;
using QuilvianSystemBackend.Tests.PatientManagement.Infrastructure;
using QuilvianSystemBackend.Tests.PatientManagement.MigrasiMassal.Infrastruktur;

namespace QuilvianSystemBackend.Tests.PatientManagement.MigrasiMassal;

/// <summary>
/// Penulis QR Pilot yang dipakai apa adanya dengan layout <c>patient-qrcodes/rsmmc-bulk</c>, dan pembersihan
/// yang hanya menghapus artefak berbukti kepemilikan — `PAT-MIG2-AC-17`, `18`, `30`, `43`; `PAT-DEC-024`,
/// `039`, `052`; kontrak B6.
/// </summary>
/// <remarks>
/// Pada urutan B4.3, langkah sesudah <c>CREATE_QR</c> hanya <c>COMMIT</c>, sehingga lewat service kegagalan
/// sesudah QR dibuat selalu berupa kasus J (QR dipertahankan, lihat <c>EksekusiMigrasiMassalTests</c>). Uji
/// ini membuktikan jalur pembersihan kasus I pada tingkat penulis QR dengan layout namespace bulk.
/// </remarks>
public class QrNamespaceBulkTests
{
    private const string Mrn = "00-12-00-05";

    [Fact]
    public void PathRencanaBulkDanPathHistoris_Terpisah()
    {
        using var h = new MigrasiMassalHarness();
        MigrasiMassalHarness.TulisFile(h.PathQrHistorisFisik(Mrn), "QR-HISTORIS");

        var probe = h.BuatProbe().Probe(Mrn);

        Assert.Equal("/uploads/patient-qrcodes/rsmmc-bulk/00-12-00-05/qrcode.png", h.BuatStoreQr().BuildPublicPath(Mrn));
        Assert.False(probe.PlannedExists);
        Assert.True(probe.HistoricalExists);
    }

    [Fact]
    public void CreateNew_DiPathBulk_QrHistorisTidakDisentuh()
    {
        using var h = new MigrasiMassalHarness();
        MigrasiMassalHarness.TulisFile(h.PathQrHistorisFisik(Mrn), "QR-HISTORIS");

        var hasil = h.BuatStoreQr().CreateNew(Mrn, "uji");

        Assert.False(hasil.IsConflict);
        Assert.Equal(h.PathQrBulkFisik(Mrn), hasil.Created!.PhysicalPath);
        Assert.Equal("QR-BARU:" + FormatQrPasien.Payload(Mrn), File.ReadAllText(h.PathQrBulkFisik(Mrn)));
        Assert.Equal("QR-HISTORIS", File.ReadAllText(h.PathQrHistorisFisik(Mrn)));
    }

    [Fact]
    public void CreateNew_BerkasBulkSudahAda_KonflikTanpaMenimpaAtauMengadopsi()
    {
        using var h = new MigrasiMassalHarness();
        MigrasiMassalHarness.TulisFile(h.PathQrBulkFisik(Mrn), "BERKAS-TAK-DIKENAL");

        var hasil = h.BuatStoreQr().CreateNew(Mrn, "uji");

        Assert.True(hasil.IsConflict);
        Assert.Null(hasil.Created);
        Assert.Equal("BERKAS-TAK-DIKENAL", File.ReadAllText(h.PathQrBulkFisik(Mrn)));
        Assert.Empty(h.PayloadDirender);
    }

    [Fact]
    public void Pembersihan_KepemilikanTerbukti_BerkasDanFolderBuatanSendiriDihapus()
    {
        using var h = new MigrasiMassalHarness();
        MigrasiMassalHarness.TulisFile(h.PathQrHistorisFisik(Mrn), "QR-HISTORIS");
        var store = h.BuatStoreQr();
        var dibuat = store.CreateNew(Mrn, "uji").Created!;

        var pembersihan = store.TryDeleteCreated(dibuat);

        Assert.Equal(RsmmcPilotQrCleanupOutcome.Deleted, pembersihan.Outcome);
        Assert.False(File.Exists(h.PathQrBulkFisik(Mrn)));
        Assert.False(Directory.Exists(Path.GetDirectoryName(h.PathQrBulkFisik(Mrn))));
        Assert.Equal("QR-HISTORIS", File.ReadAllText(h.PathQrHistorisFisik(Mrn)));
    }

    [Fact]
    public void Pembersihan_IsiBerubah_KepemilikanTidakTerbukti_BerkasDipertahankan()
    {
        using var h = new MigrasiMassalHarness();
        var store = h.BuatStoreQr();
        var dibuat = store.CreateNew(Mrn, "uji").Created!;

        // Panjang sama, isi berbeda: panjang saja bukan bukti kepemilikan.
        var pengganti = new string('X', (int)dibuat.Length);
        File.WriteAllText(h.PathQrBulkFisik(Mrn), pengganti, new UTF8Encoding(false));

        var pembersihan = store.TryDeleteCreated(dibuat);

        Assert.Equal(RsmmcPilotQrCleanupOutcome.OwnershipUnproven, pembersihan.Outcome);
        Assert.Equal(pengganti, File.ReadAllText(h.PathQrBulkFisik(Mrn)));
    }

    [Fact]
    public void Pembersihan_ArtefakBukanBuatanPemanggilanIni_TidakPernahDihapus()
    {
        using var h = new MigrasiMassalHarness();
        MigrasiMassalHarness.TulisFile(h.PathQrHistorisFisik(Mrn), "QR-HISTORIS");
        var historis = new RsmmcPilotCreatedQrArtifact(
            $"/uploads/patient-qrcodes/{Mrn}/qrcode.png",
            h.PathQrHistorisFisik(Mrn),
            Path.GetDirectoryName(h.PathQrHistorisFisik(Mrn))!,
            CreatedFolder: false,
            CreatedByThisInvocation: false,
            Length: "QR-HISTORIS".Length,
            Sha256: "tidak-relevan");

        var pembersihan = h.BuatStoreQr().TryDeleteCreated(historis);

        Assert.Equal(RsmmcPilotQrCleanupOutcome.NotCreatedByThisInvocation, pembersihan.Outcome);
        Assert.Equal("QR-HISTORIS", File.ReadAllText(h.PathQrHistorisFisik(Mrn)));
    }
}
