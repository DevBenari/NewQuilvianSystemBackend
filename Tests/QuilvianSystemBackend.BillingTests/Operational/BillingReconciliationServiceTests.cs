using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Enums;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Services;
using QuilvianSystemBackend.BillingTests.Infrastructure;
using Xunit;

namespace QuilvianSystemBackend.BillingTests.Operational
{
    /// <summary>
    /// Acceptance criteria RJ-BIL-BE-007 dan RJ-BIL-GATE-DEC-008 yang menuntut database
    /// sungguhan.
    ///
    /// Seluruh invariant di sini adalah invariant persistence: keunikan case, gerbang penutupan
    /// folio, dan status kanonik yang dibaca kembali dari baris tersimpan. Provider InMemory
    /// tidak dipakai karena tidak menegakkan unique index, sehingga pengujian keunikan case
    /// akan lulus secara semu.
    /// </summary>
    public sealed class BillingReconciliationServiceTests
        : IClassFixture<BillingTestDatabaseFixture>, IAsyncLifetime
    {
        private readonly BillingTestDatabaseFixture _fixture;
        private readonly List<EncounterSeed> _seeds = new();

        public BillingReconciliationServiceTests(BillingTestDatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            foreach (var seed in _seeds)
                await _fixture.CleanupEncounterAsync(seed);
        }

        // =================================================================
        // Acceptance criteria 1 — kehilangan jawaban tidak menggandakan tagihan
        // =================================================================

        /// <summary>
        /// Jawaban Billing hilang setelah tagihan sebenarnya sudah tercatat. Pencarian status
        /// kanonik harus menyatakan dengan tegas bahwa pengiriman ulang tidak aman, karena
        /// mengulang pengiriman atas hasil yang belum terverifikasi adalah cara paling umum
        /// melahirkan tagihan kedua.
        /// </summary>
        [Fact]
        public async Task JawabanHilang_PencarianStatusMenyatakanTidakAmanDiulang()
        {
            var konteks = await SiapkanKonteksAsync(BillingProcessingOutcome.OutcomeUnknown);

            await using var context = _fixture.CreateContext();
            var layanan = new BillingReconciliationService(context);

            var status = await layanan.GetProcessingStatusAsync(
                konteks.SourceContext,
                konteks.MilestoneFactId,
                konteks.MilestoneFactVersion,
                konteks.EffectType);

            Assert.True(status.Found);
            Assert.Equal(BillingProcessingOutcome.OutcomeUnknown, status.Outcome);
            Assert.False(status.SafeToRetryWithSameKey);
            Assert.Contains("tagihan ganda", status.Guidance);
        }

        /// <summary>
        /// Identitas yang belum pernah diproses justru aman dikirim ulang. Tanpa pembedaan ini,
        /// pengirim akan menahan diri pada keadaan yang sebenarnya menuntut pengiriman.
        /// </summary>
        [Fact]
        public async Task IdentitasBelumPernahDiproses_AmanDikirimUlang()
        {
            await using var context = _fixture.CreateContext();
            var layanan = new BillingReconciliationService(context);

            var status = await layanan.GetProcessingStatusAsync(
                BillingFolioService.InternalTestSourceContext,
                Guid.NewGuid(),
                1,
                BillingFolioService.InternalTestEffectType);

            Assert.False(status.Found);
            Assert.True(status.SafeToRetryWithSameKey);
        }

        [Fact]
        public async Task HasilBerhasil_TidakPerluDikirimUlang()
        {
            var konteks = await SiapkanKonteksAsync(BillingProcessingOutcome.Succeeded);

            await using var context = _fixture.CreateContext();
            var layanan = new BillingReconciliationService(context);

            var status = await layanan.GetProcessingStatusAsync(
                konteks.SourceContext,
                konteks.MilestoneFactId,
                konteks.MilestoneFactVersion,
                konteks.EffectType);

            Assert.False(status.SafeToRetryWithSameKey);
            Assert.Contains("tidak akan", status.Guidance);
        }

        [Fact]
        public async Task GangguanSementara_AmanDicobaUlangDenganKunciYangSama()
        {
            var konteks = await SiapkanKonteksAsync(BillingProcessingOutcome.TransientFailure);

            await using var context = _fixture.CreateContext();
            var layanan = new BillingReconciliationService(context);

            var status = await layanan.GetProcessingStatusAsync(
                konteks.SourceContext,
                konteks.MilestoneFactId,
                konteks.MilestoneFactVersion,
                konteks.EffectType);

            Assert.True(status.SafeToRetryWithSameKey);
        }

        // =================================================================
        // Acceptance criteria 2 — kegagalan terlihat, dan tidak berlipat
        // =================================================================

        [Fact]
        public async Task PemindaianMembukaCaseUntukHasilYangBelumPasti()
        {
            var konteks = await SiapkanKonteksAsync(BillingProcessingOutcome.OutcomeUnknown);

            await using var context = _fixture.CreateContext();
            var layanan = new BillingReconciliationService(context);

            var hasil = await layanan.ScanAsync(konteks.EncounterId, konteks.ActorUserId);

            Assert.Equal(1, hasil.CasesOpened);

            var dibuka = Assert.Single(hasil.OpenedCases);

            Assert.Equal(BillingReconciliationCaseType.OutcomeUnknown, dibuka.CaseType);
            Assert.Equal(BillingReconciliationCaseStatus.Open, dibuka.CaseStatus);
            Assert.Equal(konteks.EncounterId, dibuka.EncounterId);
            Assert.False(string.IsNullOrWhiteSpace(dibuka.NextAction));
        }

        /// <summary>
        /// Pemindaian ulang tidak boleh menumpuk case untuk masalah yang sama. Bila ini gagal,
        /// daftar case akan penuh salinan dan petugas kehilangan kepercayaan pada isinya.
        /// </summary>
        [Fact]
        public async Task PemindaianDiulang_TidakMenggandakanCase()
        {
            var konteks = await SiapkanKonteksAsync(BillingProcessingOutcome.OutcomeUnknown);

            await using var context = _fixture.CreateContext();
            var layanan = new BillingReconciliationService(context);

            var pertama = await layanan.ScanAsync(konteks.EncounterId, konteks.ActorUserId);
            var kedua = await layanan.ScanAsync(konteks.EncounterId, konteks.ActorUserId);

            Assert.Equal(1, pertama.CasesOpened);
            Assert.Equal(0, kedua.CasesOpened);
            Assert.Equal(1, kedua.CasesReused);

            await using var verify = _fixture.CreateContext();

            var jumlahCase = await verify.BilReconciliationCases
                .CountAsync(x => x.EncounterId == konteks.EncounterId && !x.IsDelete);

            Assert.Equal(1, jumlahCase);
        }

        [Fact]
        public async Task HasilBerhasil_TidakMembukaCaseApaPun()
        {
            var konteks = await SiapkanKonteksAsync(BillingProcessingOutcome.Succeeded);

            await using var context = _fixture.CreateContext();
            var layanan = new BillingReconciliationService(context);

            var hasil = await layanan.ScanAsync(konteks.EncounterId, konteks.ActorUserId);

            Assert.Equal(0, hasil.CasesOpened);
            Assert.Empty(hasil.OpenedCases);
        }

        [Fact]
        public async Task KegagalanSebagianKomponen_TerlihatSebagaiCaseTersendiri()
        {
            var konteks = await SiapkanKonteksAsync(BillingProcessingOutcome.PartialOutcome);

            await using var context = _fixture.CreateContext();
            var layanan = new BillingReconciliationService(context);

            await layanan.ScanAsync(konteks.EncounterId, konteks.ActorUserId);

            var daftar = await layanan.GetCasesAsync(konteks.EncounterId, null, null);

            var item = Assert.Single(daftar);

            Assert.Equal(BillingReconciliationCaseType.PartialComponentFailure, item.CaseType);
            Assert.Contains("komponen", item.NextAction, StringComparison.OrdinalIgnoreCase);
        }

        // =================================================================
        // Acceptance criteria 3 — folio tidak boleh ditutup sebelum selesai
        // =================================================================

        [Fact]
        public async Task FolioTidakBolehDitutupSelamaMasihAdaCaseTerbuka()
        {
            var konteks = await SiapkanKonteksAsync(BillingProcessingOutcome.OutcomeUnknown);

            await using var context = _fixture.CreateContext();
            var layanan = new BillingReconciliationService(context);

            await layanan.ScanAsync(konteks.EncounterId, konteks.ActorUserId);

            var kesiapan = await layanan.EvaluateClosureReadinessAsync(konteks.FolioId);

            Assert.Equal(BillingServiceResultKind.Success, kesiapan.Kind);
            Assert.False(kesiapan.Value!.CanClose);
            Assert.NotEmpty(kesiapan.Value.Blockers);
        }

        /// <summary>
        /// RJ-BIL-DEC-010: ambang materialitas bernilai nol, sehingga setiap kegagalan menahan
        /// penutupan folio. Nol adalah perilaku paling aman dan bukan angka karangan.
        /// </summary>
        [Fact]
        public async Task AmbangMaterialitasNol_MenahanSetiapKegagalan()
        {
            await using var arrange = _fixture.CreateContext();

            var kebijakan = await arrange.MstBillingReconciliationPolicies
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive)
                .ToListAsync();

            Assert.NotEmpty(kebijakan);
            Assert.All(kebijakan, x => Assert.Equal(0m, x.MaterialityThresholdAmount));
        }

        [Fact]
        public async Task SetelahCaseDiselesaikan_FolioTidakLagiTertahanOlehCaseItu()
        {
            var konteks = await SiapkanKonteksAsync(BillingProcessingOutcome.PermanentFailure);

            await using var context = _fixture.CreateContext();
            var layanan = new BillingReconciliationService(context);

            var pindai = await layanan.ScanAsync(konteks.EncounterId, konteks.ActorUserId);
            var dibuka = Assert.Single(pindai.OpenedCases);

            var sebelum = await layanan.EvaluateClosureReadinessAsync(konteks.FolioId);
            Assert.False(sebelum.Value!.CanClose);

            var selesai = await layanan.ResolveAsync(
                dibuka.Id,
                new ResolveReconciliationCaseRequest
                {
                    ResolutionType = BillingReconciliationResolutionType.ConfirmedNotApplied,
                    ResolutionNote = "Sudah ditelusuri; tagihan memang belum pernah terbentuk."
                },
                konteks.ActorUserId);

            Assert.Equal(BillingServiceResultKind.Success, selesai.Kind);

            var sesudah = await layanan.EvaluateClosureReadinessAsync(konteks.FolioId);

            Assert.DoesNotContain(
                sesudah.Value!.Blockers,
                x => x.ReconciliationCaseId == dibuka.Id);
        }

        // =================================================================
        // Batas kewenangan — rekonsiliasi tidak memindahkan uang
        // =================================================================

        /// <summary>
        /// Inti batas kewenangan RJ-BIL-BE-007. Menutup sebuah case adalah pernyataan
        /// administratif bahwa masalahnya sudah ditelusuri. Ia tidak boleh menyentuh nilai
        /// tagihan maupun statusnya, karena akibat finansial adalah ranah RJ-BIL-GATE-DEC-006.
        /// </summary>
        [Fact]
        public async Task MenyelesaikanCase_TidakMengubahNilaiMaupunStatusTagihan()
        {
            var konteks = await SiapkanKonteksAsync(BillingProcessingOutcome.PermanentFailure);

            await using var arrange = _fixture.CreateContext();

            var sebelum = await arrange.BilChargeLines
                .AsNoTracking()
                .Where(x => x.FolioId == konteks.FolioId && !x.IsDelete)
                .Select(x => new { x.Id, x.GrossAmount, x.CalculationStatus })
                .ToListAsync();

            await using var context = _fixture.CreateContext();
            var layanan = new BillingReconciliationService(context);

            var pindai = await layanan.ScanAsync(konteks.EncounterId, konteks.ActorUserId);
            var dibuka = Assert.Single(pindai.OpenedCases);

            await layanan.ResolveAsync(
                dibuka.Id,
                new ResolveReconciliationCaseRequest
                {
                    ResolutionType = BillingReconciliationResolutionType.ManualFinancialAction,
                    ResolutionNote = "Perlu koreksi finansial melalui jalur persetujuan tersendiri."
                },
                konteks.ActorUserId);

            await using var verify = _fixture.CreateContext();

            var sesudah = await verify.BilChargeLines
                .AsNoTracking()
                .Where(x => x.FolioId == konteks.FolioId && !x.IsDelete)
                .Select(x => new { x.Id, x.GrossAmount, x.CalculationStatus })
                .ToListAsync();

            Assert.Equal(sebelum.Count, sesudah.Count);

            foreach (var baris in sebelum)
            {
                var pasangan = Assert.Single(sesudah, x => x.Id == baris.Id);

                Assert.Equal(baris.GrossAmount, pasangan.GrossAmount);
                Assert.Equal(baris.CalculationStatus, pasangan.CalculationStatus);
            }
        }

        [Fact]
        public async Task CaseYangSudahSelesai_TidakDapatDiselesaikanDuaKali()
        {
            var konteks = await SiapkanKonteksAsync(BillingProcessingOutcome.OutcomeUnknown);

            await using var context = _fixture.CreateContext();
            var layanan = new BillingReconciliationService(context);

            var pindai = await layanan.ScanAsync(konteks.EncounterId, konteks.ActorUserId);
            var dibuka = Assert.Single(pindai.OpenedCases);

            var permintaan = new ResolveReconciliationCaseRequest
            {
                ResolutionType = BillingReconciliationResolutionType.ConfirmedApplied,
                ResolutionNote = "Status kanonik menunjukkan tagihan sudah terbentuk."
            };

            var pertama = await layanan.ResolveAsync(dibuka.Id, permintaan, konteks.ActorUserId);
            var kedua = await layanan.ResolveAsync(dibuka.Id, permintaan, konteks.ActorUserId);

            Assert.Equal(BillingServiceResultKind.Success, pertama.Kind);
            Assert.Equal(BillingServiceResultKind.Conflict, kedua.Kind);
            Assert.Equal("BIL_RECON_CASE_CLOSED", kedua.ErrorCode);
        }

        [Fact]
        public async Task PenyelesaianTanpaCatatan_Ditolak()
        {
            var konteks = await SiapkanKonteksAsync(BillingProcessingOutcome.OutcomeUnknown);

            await using var context = _fixture.CreateContext();
            var layanan = new BillingReconciliationService(context);

            var pindai = await layanan.ScanAsync(konteks.EncounterId, konteks.ActorUserId);
            var dibuka = Assert.Single(pindai.OpenedCases);

            var hasil = await layanan.ResolveAsync(
                dibuka.Id,
                new ResolveReconciliationCaseRequest
                {
                    ResolutionType = BillingReconciliationResolutionType.NoFinancialImpact,
                    ResolutionNote = "   "
                },
                konteks.ActorUserId);

            Assert.Equal(BillingServiceResultKind.Validation, hasil.Kind);
        }

        // =================================================================
        // Kepemilikan dan laporan pemulihan
        // =================================================================

        [Fact]
        public async Task CaseLahirTanpaPemilik_DanPenugasanMenjadiTindakanTercatat()
        {
            var konteks = await SiapkanKonteksAsync(BillingProcessingOutcome.OutcomeUnknown);

            await using var context = _fixture.CreateContext();
            var layanan = new BillingReconciliationService(context);

            var pindai = await layanan.ScanAsync(konteks.EncounterId, konteks.ActorUserId);
            var dibuka = Assert.Single(pindai.OpenedCases);

            Assert.Null(dibuka.OwnerUserId);

            var ditugaskan = await layanan.AssignAsync(
                dibuka.Id,
                new AssignReconciliationCaseRequest
                {
                    OwnerUserId = konteks.ActorUserId,
                    NextAction = "Hubungi Billing untuk memastikan hasilnya."
                },
                konteks.ActorUserId);

            Assert.Equal(BillingServiceResultKind.Success, ditugaskan.Kind);
            Assert.Equal(konteks.ActorUserId, ditugaskan.Value!.OwnerUserId);
            Assert.NotNull(ditugaskan.Value.AssignedAt);
            Assert.Equal(BillingReconciliationCaseStatus.InProgress, ditugaskan.Value.CaseStatus);
        }

        [Fact]
        public async Task LaporanPemulihan_MenampilkanCaseBelumSelesaiBesertaPemilikDanTindakan()
        {
            var konteks = await SiapkanKonteksAsync(BillingProcessingOutcome.OutcomeUnknown);

            await using var context = _fixture.CreateContext();
            var layanan = new BillingReconciliationService(context);

            await layanan.ScanAsync(konteks.EncounterId, konteks.ActorUserId);

            var laporan = await layanan.GetRecoveryReportAsync(konteks.EncounterId);

            Assert.Equal(1, laporan.UnresolvedCaseCount);
            Assert.Equal(1, laporan.UnassignedCaseCount);
            Assert.Contains(konteks.EncounterId, laporan.AffectedEncounterIds);
            Assert.Contains(konteks.FolioId, laporan.AffectedFolioIds);
            Assert.NotEmpty(laporan.OutcomeCounts);

            var kasus = Assert.Single(laporan.UnresolvedCases);
            Assert.False(string.IsNullOrWhiteSpace(kasus.NextAction));
        }

        // =================================================================
        // Pembantu
        // =================================================================

        private sealed record KonteksUji(
            Guid EncounterId,
            Guid FolioId,
            Guid ActorUserId,
            string SourceContext,
            Guid MilestoneFactId,
            int MilestoneFactVersion,
            string EffectType);

        /// <summary>
        /// Membentuk satu folio beserta tagihannya melalui jalur yang sebenarnya, lalu menandai
        /// hasil pemrosesannya sesuai keadaan yang hendak diuji.
        ///
        /// Folio sengaja dibentuk lewat <see cref="BillingFolioService"/>, bukan disisipkan
        /// langsung, agar barisnya benar-benar sama dengan yang dihasilkan produksi.
        /// </summary>
        private async Task<KonteksUji> SiapkanKonteksAsync(BillingProcessingOutcome outcome)
        {
            var seed = await _fixture.SeedEncounterAsync();
            _seeds.Add(seed);

            var milestoneFactId = Guid.NewGuid();

            await using var context = _fixture.CreateContext();
            var folioService = new BillingFolioService(context);

            var hasil = await folioService.RecognizeMilestoneAsync(
                new RecognizeBillingMilestoneRequest
                {
                    IdempotencyKey = $"RC-TEST-{Guid.NewGuid():N}",
                    MilestoneFactId = milestoneFactId,
                    MilestoneFactVersion = 1,
                    EncounterId = seed.EncounterId,
                    SourceContext = BillingFolioService.InternalTestSourceContext,
                    SourceAggregateId = Guid.NewGuid(),
                    SourceItemId = Guid.NewGuid(),
                    EffectType = BillingFolioService.InternalTestEffectType,
                    OccurredAt = DateTime.UtcNow,
                    Quantity = 1m,
                    Unit = "EA"
                },
                seed.ActorUserId);

            Assert.Equal(BillingServiceResultKind.Success, hasil.Kind);

            await using var mutate = _fixture.CreateContext();

            var folioId = await mutate.BilFolios
                .Where(x => x.EncounterId == seed.EncounterId && !x.IsDelete)
                .Select(x => x.Id)
                .FirstAsync();

            var effect = await mutate.BilProcessingEffects
                .Where(x => x.MilestoneFactId == milestoneFactId && !x.IsDelete)
                .FirstAsync();

            effect.Outcome = outcome;
            effect.ErrorCode = outcome == BillingProcessingOutcome.Succeeded ? null : "BIL_TEST_SIMULATED";
            effect.ErrorMessage = outcome == BillingProcessingOutcome.Succeeded
                ? null
                : $"Keadaan {outcome} disimulasikan untuk pengujian rekonsiliasi.";

            await mutate.SaveChangesAsync();

            return new KonteksUji(
                seed.EncounterId,
                folioId,
                seed.ActorUserId,
                effect.SourceContext,
                effect.MilestoneFactId,
                effect.MilestoneFactVersion,
                effect.EffectType);
        }
    }
}
