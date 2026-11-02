using System.Globalization;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Tests.PatientManagement.MigrasiMassal.Infrastruktur;

namespace QuilvianSystemBackend.Tests.PatientManagement.MigrasiMassal;

/// <summary>
/// Log dan audit kontrak `1.3.0` B11 — `PAT-MIG2-AC-23`; `PAT-DEC-025`, `PAT-DEC-049`. Seluruh keluaran
/// <c>LoggerService</c> ditangkap, termasuk <i>scope</i> dan galat lengkap.
/// </summary>
public class LogTanpaDataPribadiTests
{
    [Fact]
    public async Task SeluruhJalur_LogTanpaNamaNikTeleponAlamatAtauTanggalLahir()
    {
        using var h = new MigrasiMassalHarness();

        await h.JalankanAsync(true);
        await h.JalankanAsync(false, 3);

        h.Kendali.KunciDipegangSesiLain.Add((int)h.PidManifest(3));
        await h.JalankanAsync(false, 25);
        h.Kendali.KunciDipegangSesiLain.Clear();

        h.GagalRender = true;
        await h.JalankanAsync(false, 25);
        h.GagalRender = false;

        h.Profil = h.Profil with { ApprovedPatientBaseline = h.Profil.ApprovedPatientBaseline + 100 };
        await h.JalankanAsync(false, 25);

        h.EnvironmentName = "Production";
        await h.JalankanAsync(true);

        var log = h.Log.SeluruhTeks;
        Assert.NotEmpty(log);

        for (var i = 0; i < 25; i++)
        {
            var preview = h.PreviewManifest(i);
            var lahir = preview.BirthDate!.Value;

            Assert.DoesNotContain(preview.FullName!, log);
            Assert.DoesNotContain(preview.Address!, log);
            Assert.DoesNotContain(MigrasiMassalHarness.NikUntuk(i), log);
            Assert.DoesNotContain(MigrasiMassalHarness.TeleponUntuk(i), log);

            if (preview.LegacyNormalizedNik is { } nikLegacy)
            {
                Assert.DoesNotContain(nikLegacy, log);
            }

            foreach (var format in new[] { "yyyy-MM-dd", "dd/MM/yyyy", "MM/dd/yyyy", "d/M/yyyy", "dd-MM-yyyy" })
            {
                Assert.DoesNotContain(lahir.ToString(format, CultureInfo.InvariantCulture), log);
            }
        }

        Assert.DoesNotContain("Nama Rahasia", log);
        Assert.DoesNotContain("Alamat Rahasia", log);
    }

    [Fact]
    public async Task PasienMigrated_LogInfoMemuatIsiMinimumB11()
    {
        using var h = new MigrasiMassalHarness();

        var data = (await h.JalankanAsync(false, 1)).Data!;
        var item = data.Items[0];
        var mrn = MigrasiMassalHarness.MrnUntuk(0);

        var entri = Assert.Single(h.Log.Entri, x => x.Level == LogLevel.Information && x.Teks.Contains("berhasil untuk satu pasien", StringComparison.Ordinal));
        Assert.Contains($"batchId={MigrasiMassalHarness.BatchKanonik:D}", entri.Teks);
        Assert.Contains("legacyPid=-667992", entri.Teks);
        Assert.Contains($"patientId={item.PatientId:D}", entri.Teks);
        Assert.Contains($"patientCode={MigrasiMassalHarness.KodeUntuk(0)}", entri.Teks);
        Assert.Contains($"medicalRecordNumber={mrn}", entri.Teks);
        Assert.Contains($"qrCodePath={MigrasiMassalHarness.PathQrBulkPublik(mrn)}", entri.Teks);
        Assert.Contains("status=MIGRATED", entri.Teks);
        Assert.Contains($"actorUserId={MigrasiMassalHarness.AktorUji:D}", entri.Teks);
    }

    [Fact]
    public async Task KonflikDanGagal_LevelPeringatanDanError_DenganJenisKonflikDanTahap()
    {
        using var h = new MigrasiMassalHarness();

        h.Kendali.KunciDipegangSesiLain.Add((int)h.PidManifest(0));
        await h.JalankanAsync(false, 25);
        h.Kendali.KunciDipegangSesiLain.Clear();

        h.GagalRender = true;
        await h.JalankanAsync(false, 25);

        Assert.Contains(h.Log.Entri, x => x.Level == LogLevel.Warning && x.Teks.Contains("conflictType=CONCURRENT_RUN", StringComparison.Ordinal));
        Assert.Contains(h.Log.Entri, x => x.Level == LogLevel.Error && x.Teks.Contains("failureStage=CREATE_QR", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Simulasi_LogInfoMemuatRingkasanDanSidikJariManifest()
    {
        using var h = new MigrasiMassalHarness();

        await h.JalankanAsync(true);

        var entri = Assert.Single(h.Log.Entri, x => x.Level == LogLevel.Information);
        Assert.Contains("manifestSha256=c43207eb73f1f244db13ff930bca1c22e2e4ae779018e5cd48e26f628a5a79ad", entri.Teks);
        Assert.Contains("ready=25", entri.Teks);
        Assert.Contains($"actorUserId={MigrasiMassalHarness.AktorUji:D}", entri.Teks);
    }
}
