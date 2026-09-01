using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Services;
using Xunit;

namespace QuilvianSystemBackend.BillingTests.Radiology;

/// <summary>
/// Penilaian gerbang keselamatan radiologi — <c>RJ-BIL-BE-004</c> acceptance criteria 1.
///
/// Seluruh test di berkas ini murni dan tidak menyentuh database. Aturan yang diuji di sini
/// adalah satu-satunya hal yang berdiri antara sebuah permintaan dan penyinaran seorang pasien,
/// sehingga ia diuji langsung tanpa perancah apa pun yang dapat menutupi kesalahannya.
/// </summary>
public class RadiologySafetyGateTests
{
    private static MstRadSafetyRequirement Butir(string kode, Guid? id = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        RequirementCode = kode,
        RequirementName = kode,
    };

    private static MstRadModalitySafetyRule Aturan(
        MstRadSafetyRequirement butir,
        bool wajib = true,
        int versi = 1) => new()
    {
        Id = Guid.NewGuid(),
        ModalityId = Guid.NewGuid(),
        SafetyRequirementId = butir.Id,
        SafetyRequirement = butir,
        IsMandatory = wajib,
        RuleVersion = versi,
        IsActive = true,
        EffectiveFrom = DateTime.UtcNow.AddDays(-1),
    };

    private static RadStudySafetyCheck Jawaban(
        MstRadSafetyRequirement butir,
        RadSafetyCheckState keadaan) => new()
    {
        Id = Guid.NewGuid(),
        RadStudyId = Guid.NewGuid(),
        SafetyRequirementId = butir.Id,
        RequirementCodeSnapshot = butir.RequirementCode,
        RequirementNameSnapshot = butir.RequirementName,
        CheckState = keadaan,
    };

    /* ------------------------------------------------------------------ *
     * Fail-closed: kebijakan yang belum ada menolak, bukan meloloskan
     * ------------------------------------------------------------------ */

    [Fact]
    public void TanpaSatuPunAturan_AcquisitionDitolak()
    {
        // RJ-BIL-DEC-014 menuntut perilaku fail-closed. Tidak adanya aturan berarti belum ada
        // yang menetapkan apa yang aman — bukan berarti semuanya aman.
        var hasil = RadSafetyGateEvaluator.Evaluate(
            Array.Empty<MstRadModalitySafetyRule>(),
            Array.Empty<RadStudySafetyCheck>());

        Assert.False(hasil.PolicyConfigured);
        Assert.False(hasil.Cleared);
    }

    [Fact]
    public void TanpaAturan_PesanMenunjukPadaAdmin_BukanPadaPetugas()
    {
        // Petugas tidak dapat memperbaiki kebijakan yang belum ditetapkan. Pesan yang
        // menyalahkan mereka hanya akan mendorong pencarian jalan pintas.
        var hasil = RadSafetyGateEvaluator.Evaluate(
            Array.Empty<MstRadModalitySafetyRule>(),
            Array.Empty<RadStudySafetyCheck>());

        var pesan = RadSafetyGateEvaluator.DescribeBlockage(hasil);

        Assert.Contains("belum ditetapkan", pesan);
        Assert.Contains("admin", pesan);
    }

    [Fact]
    public void AturanKosongTidakSamaDenganAturanTerpenuhi()
    {
        var butir = Butir("PREGNANCY_SCREENING");

        var tanpaAturan = RadSafetyGateEvaluator.Evaluate(
            Array.Empty<MstRadModalitySafetyRule>(),
            Array.Empty<RadStudySafetyCheck>());

        var denganAturanTerpenuhi = RadSafetyGateEvaluator.Evaluate(
            new[] { Aturan(butir) },
            new[] { Jawaban(butir, RadSafetyCheckState.Passed) });

        Assert.False(tanpaAturan.Cleared);
        Assert.True(denganAturanTerpenuhi.Cleared);
    }

    /* ------------------------------------------------------------------ *
     * Butir wajib
     * ------------------------------------------------------------------ */

    [Fact]
    public void ButirWajibYangBelumDijawab_Menahan()
    {
        var butir = Butir("METAL_IMPLANT_SCREENING");

        var hasil = RadSafetyGateEvaluator.Evaluate(
            new[] { Aturan(butir) },
            new[] { Jawaban(butir, RadSafetyCheckState.Pending) });

        Assert.True(hasil.PolicyConfigured);
        Assert.False(hasil.Cleared);
        Assert.Contains("METAL_IMPLANT_SCREENING", hasil.PendingMandatoryCodes);
        Assert.Empty(hasil.FailedMandatoryCodes);
    }

    [Fact]
    public void ButirWajibYangDijawabGagal_Menahan()
    {
        var butir = Butir("CONTRAST_ALLERGY");

        var hasil = RadSafetyGateEvaluator.Evaluate(
            new[] { Aturan(butir) },
            new[] { Jawaban(butir, RadSafetyCheckState.Failed) });

        Assert.False(hasil.Cleared);
        Assert.Contains("CONTRAST_ALLERGY", hasil.FailedMandatoryCodes);
        Assert.Empty(hasil.PendingMandatoryCodes);
    }

    [Fact]
    public void ButirWajibTanpaBarisJawaban_DianggapBelumDijawab_BukanLolos()
    {
        // Ketiadaan jawaban bukan jawaban. Bila baris pemeriksaannya hilang — karena data
        // rusak, atau karena aturan ditambahkan setelah study dibuat — gerbangnya harus tetap
        // menahan, bukan terbuka karena kebetulan tidak ada yang menolak.
        var butir = Butir("SEDATION_FASTING");

        var hasil = RadSafetyGateEvaluator.Evaluate(
            new[] { Aturan(butir) },
            Array.Empty<RadStudySafetyCheck>());

        Assert.False(hasil.Cleared);
        Assert.Contains("SEDATION_FASTING", hasil.PendingMandatoryCodes);
    }

    [Fact]
    public void SeluruhButirWajibTerpenuhi_Meloloskan()
    {
        var butirA = Butir("PATIENT_IDENTITY");
        var butirB = Butir("PREGNANCY_SCREENING");

        var hasil = RadSafetyGateEvaluator.Evaluate(
            new[] { Aturan(butirA), Aturan(butirB) },
            new[]
            {
                Jawaban(butirA, RadSafetyCheckState.Passed),
                Jawaban(butirB, RadSafetyCheckState.Passed),
            });

        Assert.True(hasil.Cleared);
        Assert.Empty(hasil.PendingMandatoryCodes);
        Assert.Empty(hasil.FailedMandatoryCodes);
    }

    [Fact]
    public void TidakBerlaku_IkutMenuntaskan_TetapiBukanSinonimLolos()
    {
        // Skrining kehamilan pada pasien laki-laki tidak dapat dijawab "aman"; ia memang tidak
        // berlaku. Keduanya sama-sama meloloskan, tetapi jejaknya harus dapat dibedakan.
        var butir = Butir("PREGNANCY_SCREENING");

        var hasil = RadSafetyGateEvaluator.Evaluate(
            new[] { Aturan(butir) },
            new[] { Jawaban(butir, RadSafetyCheckState.NotApplicable) });

        Assert.True(hasil.Cleared);
        Assert.True(RadSafetyGateEvaluator.IsSettled(RadSafetyCheckState.NotApplicable));
        Assert.True(RadSafetyGateEvaluator.IsSettled(RadSafetyCheckState.Passed));
        Assert.NotEqual(RadSafetyCheckState.Passed, RadSafetyCheckState.NotApplicable);
    }

    /* ------------------------------------------------------------------ *
     * Butir tidak wajib
     * ------------------------------------------------------------------ */

    [Fact]
    public void ButirTidakWajibYangGagal_TidakMenahan()
    {
        // Menjadikannya pemblokir akan menghapus perbedaan antara wajib dan tidak wajib, dan
        // mendorong admin menandai semuanya tidak wajib supaya pekerjaan tetap berjalan.
        var wajib = Butir("PATIENT_IDENTITY");
        var opsional = Butir("PRIOR_STUDY_COMPARISON");

        var hasil = RadSafetyGateEvaluator.Evaluate(
            new[] { Aturan(wajib), Aturan(opsional, wajib: false) },
            new[]
            {
                Jawaban(wajib, RadSafetyCheckState.Passed),
                Jawaban(opsional, RadSafetyCheckState.Failed),
            });

        Assert.True(hasil.Cleared);
        Assert.Empty(hasil.FailedMandatoryCodes);
    }

    [Fact]
    public void ButirTidakWajibYangBelumDijawab_TidakMenahan()
    {
        var wajib = Butir("PATIENT_IDENTITY");
        var opsional = Butir("PRIOR_STUDY_COMPARISON");

        var hasil = RadSafetyGateEvaluator.Evaluate(
            new[] { Aturan(wajib), Aturan(opsional, wajib: false) },
            new[] { Jawaban(wajib, RadSafetyCheckState.Passed) });

        Assert.True(hasil.Cleared);
    }

    /* ------------------------------------------------------------------ *
     * Versi aturan dan pesan
     * ------------------------------------------------------------------ */

    [Fact]
    public void VersiAturanYangDibekukanAdalahYangTertinggi()
    {
        var butirA = Butir("PATIENT_IDENTITY");
        var butirB = Butir("PREGNANCY_SCREENING");

        var hasil = RadSafetyGateEvaluator.Evaluate(
            new[] { Aturan(butirA, versi: 2), Aturan(butirB, versi: 5) },
            new[]
            {
                Jawaban(butirA, RadSafetyCheckState.Passed),
                Jawaban(butirB, RadSafetyCheckState.Passed),
            });

        Assert.Equal(5, hasil.RuleVersion);
    }

    [Fact]
    public void PesanPenahanMenyebutButirYangMenahan()
    {
        // Petugas yang tahu butir mana yang kurang dapat menyelesaikannya. Petugas yang hanya
        // diberi tahu "tidak boleh" akan mencari jalan lain.
        var belum = Butir("SEDATION_FASTING");
        var gagal = Butir("CONTRAST_ALLERGY");

        var hasil = RadSafetyGateEvaluator.Evaluate(
            new[] { Aturan(belum), Aturan(gagal) },
            new[]
            {
                Jawaban(belum, RadSafetyCheckState.Pending),
                Jawaban(gagal, RadSafetyCheckState.Failed),
            });

        var pesan = RadSafetyGateEvaluator.DescribeBlockage(hasil);

        Assert.Contains("SEDATION_FASTING", pesan);
        Assert.Contains("CONTRAST_ALLERGY", pesan);
        Assert.Contains("belum dijawab", pesan);
        Assert.Contains("tidak aman", pesan);
    }

    [Fact]
    public void BarisJawabanYangSudahDihapus_Diabaikan()
    {
        var butir = Butir("PATIENT_IDENTITY");

        var terhapus = Jawaban(butir, RadSafetyCheckState.Passed);
        terhapus.IsDelete = true;

        var hasil = RadSafetyGateEvaluator.Evaluate(
            new[] { Aturan(butir) },
            new[] { terhapus });

        // Jawaban yang sudah dihapus tidak boleh meloloskan gerbang.
        Assert.False(hasil.Cleared);
        Assert.Contains("PATIENT_IDENTITY", hasil.PendingMandatoryCodes);
    }
}
