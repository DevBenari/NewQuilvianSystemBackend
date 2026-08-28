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
    /// Acceptance criteria <c>RJ-BIL-BE-006</c> dan <c>RJ-BIL-GATE-DEC-006</c> yang menuntut
    /// database sungguhan.
    ///
    /// Tiga kriteria yang harus terbukti di sini: self-approval ditolak, menunggu persetujuan
    /// tidak mengubah keadaan finansial, dan penutupan folio ditolak selama rekonsiliasi belum
    /// selesai. Ketiganya invariant persistence — keunikan, keterurutan, dan keadaan yang dibaca
    /// kembali dari baris tersimpan — sehingga provider InMemory tidak dipakai: ia tidak
    /// menegakkan unique index, dan pengujian akan lulus secara semu.
    /// </summary>
    public sealed class BillingFinancialActionServiceTests
        : IClassFixture<BillingTestDatabaseFixture>, IAsyncLifetime
    {
        private readonly BillingTestDatabaseFixture _fixture;
        private readonly List<EncounterSeed> _seeds = new();
        private readonly List<Guid> _policyIds = new();

        public BillingFinancialActionServiceTests(BillingTestDatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            if (_policyIds.Count > 0)
            {
                await using var context = _fixture.CreateContext();

                await context.MstBillingApprovalPolicies
                    .Where(x => _policyIds.Contains(x.Id))
                    .ExecuteDeleteAsync();
            }

            foreach (var seed in _seeds)
                await _fixture.CleanupEncounterAsync(seed);
        }

        // =================================================================
        // Acceptance criteria 1 — self-approval ditolak
        // =================================================================

        /// <summary>
        /// Pengaju mencoba memutuskan permintaannya sendiri.
        ///
        /// Yang diuji bukan hanya pesan galatnya, melainkan bahwa <b>tidak ada apa pun yang
        /// terjadi</b>: status tetap menunggu keputusan, dan tidak ada satu baris persetujuan pun
        /// yang lahir. Penolakan yang tetap meninggalkan jejak persetujuan akan membuat laporan
        /// audit menyatakan permintaan itu pernah diputuskan.
        /// </summary>
        [Fact]
        public async Task PengajuMemutuskanPermintaannyaSendiri_Ditolak()
        {
            var konteks = await SiapkanFolioBersihAsync();
            var permintaan = await BuatDanAjukanVoidAsync(konteks);

            Assert.Equal(BillingFinancialActionStatus.PendingApproval, permintaan.Status);

            await using var context = _fixture.CreateContext();
            var layanan = new BillingFinancialActionService(context);

            var hasil = await layanan.DecideAsync(
                permintaan.Id,
                new DecideFinancialActionRequest
                {
                    Decision = BillingApprovalDecision.Approve,
                    DecisionNote = "Menyetujui permintaan sendiri."
                },
                konteks.Seed.ActorUserId);

            Assert.Equal(BillingServiceResultKind.Validation, hasil.Kind);
            Assert.Equal("BIL_SELF_APPROVAL_FORBIDDEN", hasil.ErrorCode);

            await using var verifikasi = _fixture.CreateContext();

            var tersimpan = await verifikasi.BilFinancialActionRequests
                .AsNoTracking()
                .FirstAsync(x => x.Id == permintaan.Id);

            Assert.Equal(BillingFinancialActionStatus.PendingApproval, tersimpan.Status);

            var jumlahPersetujuan = await verifikasi.BilFinancialApprovals
                .CountAsync(x => x.RequestId == permintaan.Id);

            Assert.Equal(0, jumlahPersetujuan);
        }

        /// <summary>
        /// Checker yang berbeda orang berhasil memutuskan permintaan yang sama.
        ///
        /// Test ini pasangan wajib dari yang di atas. Tanpa membuktikan jalur yang sah benar-benar
        /// bekerja, larangan self-approval bisa saja lulus hanya karena persetujuan memang tidak
        /// pernah berhasil bagi siapa pun.
        /// </summary>
        [Fact]
        public async Task CheckerBerbedaOrang_KeputusanTercatat()
        {
            var konteks = await SiapkanFolioBersihAsync();
            var permintaan = await BuatDanAjukanVoidAsync(konteks);
            var checkerUserId = Guid.NewGuid();

            await using var context = _fixture.CreateContext();
            var layanan = new BillingFinancialActionService(context);

            var hasil = await layanan.DecideAsync(
                permintaan.Id,
                new DecideFinancialActionRequest
                {
                    Decision = BillingApprovalDecision.Approve,
                    DecisionNote = "Alasan koreksi sudah sesuai bukti."
                },
                checkerUserId);

            Assert.Equal(BillingServiceResultKind.Success, hasil.Kind);
            Assert.Equal(BillingFinancialActionStatus.Approved, hasil.Value!.Status);

            var persetujuan = Assert.Single(hasil.Value.Approvals);
            Assert.Equal(checkerUserId, persetujuan.CheckerUserId);
            Assert.NotEqual(persetujuan.CheckerUserId, hasil.Value.MakerUserId);

            // Sidik isi yang disetujui dibekukan pada persetujuannya sendiri.
            Assert.Equal(hasil.Value.ContentHash, persetujuan.RequestContentHash);
        }

        /// <summary>
        /// Larangan self-approval berlaku untuk seluruh jenis keputusan, bukan hanya menyetujui.
        /// Pengaju yang boleh "menolak" permintaannya sendiri tetap merusak pemisahan tugas,
        /// karena ia dapat menutup permintaan sebelum orang lain sempat melihatnya.
        /// </summary>
        [Theory]
        [InlineData(BillingApprovalDecision.Reject)]
        [InlineData(BillingApprovalDecision.ReturnForRevision)]
        public async Task PengajuMenolakAtauMengembalikanPermintaannyaSendiri_Ditolak(
            BillingApprovalDecision keputusan)
        {
            var konteks = await SiapkanFolioBersihAsync();
            var permintaan = await BuatDanAjukanVoidAsync(konteks);

            await using var context = _fixture.CreateContext();
            var layanan = new BillingFinancialActionService(context);

            var hasil = await layanan.DecideAsync(
                permintaan.Id,
                new DecideFinancialActionRequest { Decision = keputusan },
                konteks.Seed.ActorUserId);

            Assert.Equal(BillingServiceResultKind.Validation, hasil.Kind);
            Assert.Equal("BIL_SELF_APPROVAL_FORBIDDEN", hasil.ErrorCode);
        }

        // =================================================================
        // Acceptance criteria 2 — menunggu persetujuan tidak mengubah keadaan
        // =================================================================

        /// <summary>
        /// Keadaan finansial baru berubah pada saat pelaksanaan, bukan pada saat pengajuan dan
        /// bukan pula pada saat persetujuan.
        ///
        /// Ketiga titik diperiksa berturut-turut dalam satu test, karena yang perlu dibuktikan
        /// adalah urutannya — bukan sekadar bahwa akhirnya berubah.
        /// </summary>
        [Fact]
        public async Task MenungguDanDisetujui_TidakMengubahTagihanSampaiDijalankan()
        {
            var konteks = await SiapkanFolioBersihAsync();
            var permintaan = await BuatDanAjukanVoidAsync(konteks);

            // Titik 1 — sesudah diajukan.
            await PastikanBarisTagihanBerstatusAsync(
                konteks.ChargeLineId, BillingChargeCalculationStatus.Recognized);

            var checkerUserId = Guid.NewGuid();

            await using (var context = _fixture.CreateContext())
            {
                var layanan = new BillingFinancialActionService(context);

                var keputusan = await layanan.DecideAsync(
                    permintaan.Id,
                    new DecideFinancialActionRequest { Decision = BillingApprovalDecision.Approve },
                    checkerUserId);

                Assert.Equal(BillingServiceResultKind.Success, keputusan.Kind);
            }

            // Titik 2 — sesudah disetujui, sebelum dijalankan.
            await PastikanBarisTagihanBerstatusAsync(
                konteks.ChargeLineId, BillingChargeCalculationStatus.Recognized);

            await using (var context = _fixture.CreateContext())
            {
                var layanan = new BillingFinancialActionService(context);

                var pelaksanaan = await layanan.ExecuteAsync(
                    permintaan.Id,
                    new ExecuteFinancialActionRequest { ExecutionNote = "Dijalankan setelah disetujui." },
                    konteks.Seed.ActorUserId);

                Assert.Equal(BillingServiceResultKind.Success, pelaksanaan.Kind);
                Assert.Equal(BillingFinancialActionStatus.Executed, pelaksanaan.Value!.Status);
            }

            // Titik 3 — sesudah dijalankan.
            await PastikanBarisTagihanBerstatusAsync(
                konteks.ChargeLineId, BillingChargeCalculationStatus.Voided);
        }

        /// <summary>
        /// Permintaan yang belum disetujui tidak dapat dijalankan, walaupun pemanggilnya adalah
        /// pengajunya sendiri dan permintaannya sah.
        /// </summary>
        [Fact]
        public async Task PermintaanBelumDisetujui_TidakDapatDijalankan()
        {
            var konteks = await SiapkanFolioBersihAsync();
            var permintaan = await BuatDanAjukanVoidAsync(konteks);

            await using var context = _fixture.CreateContext();
            var layanan = new BillingFinancialActionService(context);

            var hasil = await layanan.ExecuteAsync(
                permintaan.Id,
                new ExecuteFinancialActionRequest(),
                konteks.Seed.ActorUserId);

            Assert.Equal(BillingServiceResultKind.Conflict, hasil.Kind);
            Assert.Equal("BIL_ACTION_NOT_APPROVED", hasil.ErrorCode);

            await PastikanBarisTagihanBerstatusAsync(
                konteks.ChargeLineId, BillingChargeCalculationStatus.Recognized);
        }

        /// <summary>
        /// Menjalankan dua kali tidak menggandakan efeknya.
        ///
        /// <c>RJ-BIL-GATE-DEC-006</c>: <i>"Replayed approved request tidak menghasilkan duplicate
        /// refund/reversal/waiver/write-off/adjustment."</i> Yang diperiksa bukan hanya status
        /// akhirnya, melainkan bahwa waktu pelaksanaan tidak bergeser — sebab bergesernya waktu
        /// berarti pelaksanaan kedua benar-benar terjadi.
        /// </summary>
        [Fact]
        public async Task MenjalankanDuaKali_TidakMenggandakanEfek()
        {
            var konteks = await SiapkanFolioBersihAsync();
            var permintaan = await BuatDanAjukanVoidAsync(konteks);

            await SetujuiAsync(permintaan.Id, Guid.NewGuid());

            await using (var context = _fixture.CreateContext())
            {
                var layanan = new BillingFinancialActionService(context);

                var pertama = await layanan.ExecuteAsync(
                    permintaan.Id, new ExecuteFinancialActionRequest(), konteks.Seed.ActorUserId);

                Assert.Equal(BillingServiceResultKind.Success, pertama.Kind);
            }

            // Waktu pelaksanaan dibaca dari database, bukan dari nilai kembalian.
            //
            // Alasannya presisi: `DateTime` .NET berpresisi 100 nanodetik, sedangkan kolom
            // `timestamp with time zone` PostgreSQL berpresisi mikrodetik. Nilai kembalian
            // pemanggilan pertama masih berupa nilai di memori dengan presisi penuh, sementara
            // pemanggilan kedua membacanya kembali dari baris tersimpan. Membandingkan keduanya
            // akan memunculkan selisih dua tick yang tidak ada hubungannya dengan idempotensi.
            // Membandingkan nilai tersimpan dengan nilai tersimpan menghilangkan artefak itu
            // tanpa melonggarkan apa pun.
            var waktuPelaksanaanPertama = await WaktuPelaksanaanTersimpanAsync(permintaan.Id);

            await using (var context = _fixture.CreateContext())
            {
                var layanan = new BillingFinancialActionService(context);

                var kedua = await layanan.ExecuteAsync(
                    permintaan.Id, new ExecuteFinancialActionRequest(), konteks.Seed.ActorUserId);

                Assert.Equal(BillingServiceResultKind.Success, kedua.Kind);
            }

            var waktuPelaksanaanKedua = await WaktuPelaksanaanTersimpanAsync(permintaan.Id);

            Assert.Equal(waktuPelaksanaanPertama, waktuPelaksanaanKedua);

            await using var verifikasi = _fixture.CreateContext();

            var baris = await verifikasi.BilChargeLines
                .AsNoTracking()
                .FirstAsync(x => x.Id == konteks.ChargeLineId);

            // Satu kali kenaikan versi, bukan dua.
            Assert.Equal(konteks.ChargeLineVersionAwal + 1, baris.Version);
        }

        /// <summary>
        /// Keadaan sasaran berubah setelah persetujuan diberikan. Pelaksanaan harus berhenti dan
        /// menuntut penilaian ulang, bukan menjalankan keputusan atas keadaan yang sudah tidak
        /// berlaku.
        /// </summary>
        [Fact]
        public async Task SasaranBerubahSesudahDisetujui_MenuntutPenilaianUlang()
        {
            var konteks = await SiapkanFolioBersihAsync();
            var permintaan = await BuatDanAjukanVoidAsync(konteks);

            await SetujuiAsync(permintaan.Id, Guid.NewGuid());

            // Ada orang lain yang menyentuh baris tagihan di antara persetujuan dan pelaksanaan.
            await using (var mutasi = _fixture.CreateContext())
            {
                var baris = await mutasi.BilChargeLines.FirstAsync(x => x.Id == konteks.ChargeLineId);
                baris.Version += 1;
                await mutasi.SaveChangesAsync();
            }

            await using var context = _fixture.CreateContext();
            var layanan = new BillingFinancialActionService(context);

            var hasil = await layanan.ExecuteAsync(
                permintaan.Id, new ExecuteFinancialActionRequest(), konteks.Seed.ActorUserId);

            Assert.Equal(BillingServiceResultKind.Conflict, hasil.Kind);
            Assert.Equal("BIL_ACTION_REVALIDATION_REQUIRED", hasil.ErrorCode);

            await using var verifikasi = _fixture.CreateContext();

            var tersimpan = await verifikasi.BilFinancialActionRequests
                .AsNoTracking()
                .FirstAsync(x => x.Id == permintaan.Id);

            Assert.Equal(BillingFinancialActionStatus.RevalidationRequired, tersimpan.Status);
        }

        // =================================================================
        // Acceptance criteria 3 — penutupan folio ditolak saat masih ada yang menggantung
        // =================================================================

        /// <summary>
        /// Folio dengan reconciliation case yang masih terbuka tidak boleh ditutup.
        ///
        /// Inilah pertemuan <c>RJ-BIL-BE-006</c> dengan <c>RJ-BIL-BE-007</c>: gerbangnya milik
        /// task sebelumnya, keputusannya dilaksanakan di sini.
        /// </summary>
        [Fact]
        public async Task RekonsiliasiBelumSelesai_PenutupanFolioDitolak()
        {
            var konteks = await SiapkanFolioBersihAsync();

            await using (var mutasi = _fixture.CreateContext())
            {
                mutasi.BilReconciliationCases.Add(new BilReconciliationCase
                {
                    CaseNumber = $"RC-UJI-{Guid.NewGuid():N}"[..20],
                    CaseType = BillingReconciliationCaseType.OutcomeUnknown,
                    SourceContext = "UJI_BE006",
                    MilestoneFactId = Guid.NewGuid(),
                    MilestoneFactVersion = 1,
                    EffectType = "UJI_BE006_EFFECT",
                    EncounterId = konteks.Seed.EncounterId,
                    FolioId = konteks.FolioId,
                    ImpactAmount = 250_000m,
                    ImpactDescription = "Hasil pemrosesan belum dapat dipastikan.",
                    BlocksFolioClosure = true,
                    CaseStatus = BillingReconciliationCaseStatus.Open,
                    Priority = BillingReconciliationPriority.Normal,
                    DetectedAt = DateTime.UtcNow,
                    CreateBy = konteks.Seed.ActorUserId
                });

                await mutasi.SaveChangesAsync();
            }

            var statusSebelum = await StatusFolioAsync(konteks.FolioId);

            await using var context = _fixture.CreateContext();
            var layanan = BuatLayananPenutupan(context);

            var hasil = await layanan.CloseAsync(
                konteks.FolioId, new CloseFolioRequest(), konteks.Seed.ActorUserId);

            Assert.Equal(BillingServiceResultKind.Conflict, hasil.Kind);
            Assert.Equal("BIL_FOLIO_CLOSE_BLOCKED", hasil.ErrorCode);
            Assert.Contains("RECONCILIATION", hasil.ErrorMessage);

            // Penolakan tidak boleh menyisakan jejak pada folio. Yang dibandingkan adalah status
            // sebelum dan sesudah, bukan satu nilai yang ditebak di muka: penutupan yang ditolak
            // harus meninggalkan folio persis seperti semula, apa pun status semula itu.
            var statusSesudah = await StatusFolioAsync(konteks.FolioId);

            Assert.Equal(statusSebelum, statusSesudah);
            Assert.NotEqual(BillingFolioStatus.Closed, statusSesudah);
        }

        /// <summary>
        /// Permintaan tindakan finansial yang belum selesai juga menahan penutupan — termasuk
        /// yang sudah disetujui tetapi belum dijalankan, karena angka folio masih akan berubah.
        /// </summary>
        [Fact]
        public async Task PermintaanFinansialBelumSelesai_PenutupanFolioDitolak()
        {
            var konteks = await SiapkanFolioBersihAsync();
            var permintaan = await BuatDanAjukanVoidAsync(konteks);

            await SetujuiAsync(permintaan.Id, Guid.NewGuid());

            var statusSebelum = await StatusFolioAsync(konteks.FolioId);

            await using var context = _fixture.CreateContext();
            var layanan = BuatLayananPenutupan(context);

            var hasil = await layanan.CloseAsync(
                konteks.FolioId, new CloseFolioRequest(), konteks.Seed.ActorUserId);

            Assert.Equal(BillingServiceResultKind.Conflict, hasil.Kind);
            Assert.Contains("FINANCIAL_ACTION", hasil.ErrorMessage);

            var statusSesudah = await StatusFolioAsync(konteks.FolioId);

            Assert.Equal(statusSebelum, statusSesudah);
            Assert.NotEqual(BillingFolioStatus.Closed, statusSesudah);
        }

        /// <summary>
        /// Folio yang benar-benar bersih boleh ditutup, dan penutupannya meninggalkan bukti
        /// keadaan gerbang pada saat itu.
        /// </summary>
        [Fact]
        public async Task FolioBersih_DapatDitutupDanTercatatRiwayatnya()
        {
            var konteks = await SiapkanFolioBersihAsync();

            await using var context = _fixture.CreateContext();
            var layanan = BuatLayananPenutupan(context);

            var hasil = await layanan.CloseAsync(
                konteks.FolioId,
                new CloseFolioRequest { Note = "Seluruh tagihan sudah jelas." },
                konteks.Seed.ActorUserId);

            Assert.Equal(BillingServiceResultKind.Success, hasil.Kind);
            Assert.Equal(BillingFolioStatus.Closed, hasil.Value!.Status);

            var riwayat = await layanan.GetHistoryAsync(konteks.FolioId);

            var baris = Assert.Single(riwayat);
            Assert.Equal(BillingFolioClosureAction.Close, baris.Action);
            Assert.Equal(BillingFolioStatus.Closed, baris.NewStatus);
            Assert.False(string.IsNullOrWhiteSpace(baris.ClosureEvidence));
        }

        // =================================================================
        // Penutupan dan pembukaan kembali
        // =================================================================

        /// <summary>
        /// Membuka kembali folio tanpa permintaan yang disetujui ditolak.
        /// </summary>
        [Fact]
        public async Task MembukaKembaliTanpaPersetujuan_Ditolak()
        {
            var konteks = await SiapkanFolioBersihAsync();

            await using (var context = _fixture.CreateContext())
            {
                var penutupan = BuatLayananPenutupan(context);

                var tutup = await penutupan.CloseAsync(
                    konteks.FolioId, new CloseFolioRequest(), konteks.Seed.ActorUserId);

                Assert.Equal(BillingServiceResultKind.Success, tutup.Kind);
            }

            var permintaan = await BuatPermintaanAsync(
                konteks, BillingFinancialActionType.FolioReopen, chargeLineId: null, nominal: 0m);

            await AjukanAsync(permintaan.Id, konteks.Seed.ActorUserId);

            await using var buka = _fixture.CreateContext();
            var layanan = BuatLayananPenutupan(buka);

            var hasil = await layanan.ReopenAsync(
                konteks.FolioId,
                new ReopenFolioRequest { FinancialActionRequestId = permintaan.Id },
                konteks.Seed.ActorUserId);

            Assert.Equal(BillingServiceResultKind.Conflict, hasil.Kind);
            Assert.Equal("BIL_ACTION_NOT_APPROVED", hasil.ErrorCode);

            await PastikanFolioBerstatusAsync(konteks.FolioId, BillingFolioStatus.Closed);
        }

        /// <summary>
        /// Membuka kembali folio atas permintaan yang sudah disetujui berhasil, dan riwayat
        /// penutupan sebelumnya <b>tetap ada</b>.
        /// </summary>
        [Fact]
        public async Task MembukaKembaliDenganPersetujuan_RiwayatPenutupanTetapAda()
        {
            var konteks = await SiapkanFolioBersihAsync();

            await using (var context = _fixture.CreateContext())
            {
                var penutupan = BuatLayananPenutupan(context);
                await penutupan.CloseAsync(
                    konteks.FolioId, new CloseFolioRequest(), konteks.Seed.ActorUserId);
            }

            var permintaan = await BuatPermintaanAsync(
                konteks, BillingFinancialActionType.FolioReopen, chargeLineId: null, nominal: 0m);

            await AjukanAsync(permintaan.Id, konteks.Seed.ActorUserId);
            await SetujuiAsync(permintaan.Id, Guid.NewGuid());

            await using var buka = _fixture.CreateContext();
            var layanan = BuatLayananPenutupan(buka);

            var hasil = await layanan.ReopenAsync(
                konteks.FolioId,
                new ReopenFolioRequest
                {
                    FinancialActionRequestId = permintaan.Id,
                    Note = "Ada tagihan susulan yang sah."
                },
                konteks.Seed.ActorUserId);

            Assert.Equal(BillingServiceResultKind.Success, hasil.Kind);
            Assert.Equal(BillingFolioStatus.Open, hasil.Value!.Status);

            var riwayat = await layanan.GetHistoryAsync(konteks.FolioId);

            Assert.Equal(2, riwayat.Count);
            Assert.Equal(BillingFolioClosureAction.Close, riwayat[0].Action);
            Assert.Equal(BillingFolioClosureAction.Reopen, riwayat[1].Action);
            Assert.Equal(permintaan.Id, riwayat[1].FinancialActionRequestId);
        }

        // =================================================================
        // Kebijakan ambang
        // =================================================================

        /// <summary>
        /// Tanpa kebijakan ambang yang sah, tindakan yang bergantung ambang berhenti pada
        /// <c>BlockedByPolicyConfiguration</c>.
        ///
        /// Yang penting: ia <b>tidak</b> menjadi <c>Approved</c>, dan <b>tidak</b> pula gagal
        /// sebagai galat. Permintaannya tetap hidup menunggu Finance menetapkan kebijakannya.
        /// </summary>
        [Fact]
        public async Task TanpaKebijakanAmbang_PermintaanTertahanDanTidakDisetujui()
        {
            var konteks = await SiapkanFolioBersihAsync();

            var permintaan = await BuatPermintaanAsync(
                konteks, BillingFinancialActionType.Waiver, chargeLineId: null, nominal: 75_000m);

            var diajukan = await AjukanAsync(permintaan.Id, konteks.Seed.ActorUserId);

            Assert.Equal(
                BillingFinancialActionStatus.BlockedByPolicyConfiguration,
                diajukan.Status);

            Assert.NotEqual(BillingFinancialActionStatus.Approved, diajukan.Status);
            Assert.True(diajukan.RequiresApproval);
            Assert.False(string.IsNullOrWhiteSpace(diajukan.PolicyBlockReason));

            // Tertahan bukan berarti dapat dijalankan diam-diam.
            await using var context = _fixture.CreateContext();
            var layanan = new BillingFinancialActionService(context);

            var pelaksanaan = await layanan.ExecuteAsync(
                permintaan.Id, new ExecuteFinancialActionRequest(), konteks.Seed.ActorUserId);

            Assert.Equal(BillingServiceResultKind.Conflict, pelaksanaan.Kind);
            Assert.Equal("BIL_ACTION_NOT_APPROVED", pelaksanaan.ErrorCode);
        }

        /// <summary>
        /// Tindakan yang selalu high-risk tetap menunggu persetujuan walaupun kebijakan ambangnya
        /// belum ada — ia tidak ikut tertahan sebagai masalah konfigurasi, karena kewajiban
        /// persetujuannya tidak berasal dari kebijakan mana pun.
        /// </summary>
        [Fact]
        public async Task TindakanSelaluHighRisk_MenungguPersetujuanWalauKebijakanBelumAda()
        {
            var konteks = await SiapkanFolioBersihAsync();

            var permintaan = await BuatPermintaanAsync(
                konteks, BillingFinancialActionType.Refund, chargeLineId: null, nominal: 500_000m);

            var diajukan = await AjukanAsync(permintaan.Id, konteks.Seed.ActorUserId);

            Assert.Equal(BillingFinancialActionStatus.PendingApproval, diajukan.Status);
            Assert.Equal(BillingFinancialRiskLevel.HighRisk, diajukan.RiskLevel);
            Assert.True(diajukan.RequiresApproval);
        }

        /// <summary>
        /// Kebijakan yang menyatakan tidak perlu persetujuan <b>tidak</b> dapat melonggarkan
        /// tindakan yang selalu high-risk. Kebijakan boleh menambah kewajiban, tidak boleh
        /// mencabutnya.
        /// </summary>
        [Fact]
        public async Task KebijakanTanpaPersetujuan_TidakMelonggarkanTindakanHighRisk()
        {
            var konteks = await SiapkanFolioBersihAsync();

            await BuatKebijakanAsync(
                BillingFinancialActionType.Refund, memerlukanPersetujuan: false);

            var permintaan = await BuatPermintaanAsync(
                konteks, BillingFinancialActionType.Refund, chargeLineId: null, nominal: 10_000m);

            var diajukan = await AjukanAsync(permintaan.Id, konteks.Seed.ActorUserId);

            Assert.Equal(BillingFinancialActionStatus.PendingApproval, diajukan.Status);
            Assert.True(diajukan.RequiresApproval);
        }

        /// <summary>
        /// Permintaan yang sudah kedaluwarsa tidak dapat diputuskan. Kedaluwarsa bukan persetujuan.
        /// </summary>
        [Fact]
        public async Task PermintaanKedaluwarsa_TidakDapatDiputuskan()
        {
            var konteks = await SiapkanFolioBersihAsync();
            var permintaan = await BuatDanAjukanVoidAsync(konteks);

            await using (var mutasi = _fixture.CreateContext())
            {
                var baris = await mutasi.BilFinancialActionRequests
                    .FirstAsync(x => x.Id == permintaan.Id);

                baris.ExpiresAt = DateTime.UtcNow.AddMinutes(-1);
                await mutasi.SaveChangesAsync();
            }

            await using var context = _fixture.CreateContext();
            var layanan = new BillingFinancialActionService(context);

            var hasil = await layanan.DecideAsync(
                permintaan.Id,
                new DecideFinancialActionRequest { Decision = BillingApprovalDecision.Approve },
                Guid.NewGuid());

            Assert.Equal(BillingServiceResultKind.Conflict, hasil.Kind);
            Assert.Equal("BIL_ACTION_EXPIRED", hasil.ErrorCode);

            await using var verifikasi = _fixture.CreateContext();

            var tersimpan = await verifikasi.BilFinancialActionRequests
                .AsNoTracking()
                .FirstAsync(x => x.Id == permintaan.Id);

            Assert.Equal(BillingFinancialActionStatus.Expired, tersimpan.Status);
            Assert.NotEqual(BillingFinancialActionStatus.Approved, tersimpan.Status);
        }

        // =================================================================
        // Revisi, sidik isi, dan idempotensi
        // =================================================================

        /// <summary>
        /// Revisi menerbitkan permintaan baru dan membekukan yang lama. Nomor revisi naik, dan
        /// permintaan baru menunjuk pendahulunya.
        /// </summary>
        [Fact]
        public async Task Revisi_MenerbitkanPermintaanBaruDanMembekukanYangLama()
        {
            var konteks = await SiapkanFolioBersihAsync();
            var asli = await BuatDanAjukanVoidAsync(konteks);

            await using var context = _fixture.CreateContext();
            var layanan = new BillingFinancialActionService(context);

            var hasil = await layanan.ReviseAsync(
                asli.Id,
                new ReviseFinancialActionRequest
                {
                    ChargeLineId = konteks.ChargeLineId,
                    RequestedAmount = 123_456m,
                    ReasonCode = "KOREKSI_SETELAH_DIKEMBALIKAN",
                    ReasonNote = "Nominal diperbaiki sesuai catatan checker."
                },
                konteks.Seed.ActorUserId);

            Assert.Equal(BillingServiceResultKind.Success, hasil.Kind);
            Assert.Equal(2, hasil.Value!.RevisionNumber);
            Assert.Equal(asli.Id, hasil.Value.SupersedesRequestId);
            Assert.NotEqual(asli.ContentHash, hasil.Value.ContentHash);

            await using var verifikasi = _fixture.CreateContext();

            var lama = await verifikasi.BilFinancialActionRequests
                .AsNoTracking()
                .FirstAsync(x => x.Id == asli.Id);

            Assert.Equal(BillingFinancialActionStatus.Cancelled, lama.Status);

            // Isi permintaan lama tidak ikut berubah — ia dibekukan, bukan disunting.
            Assert.Equal(asli.RequestedAmount, lama.RequestedAmount);
            Assert.Equal(asli.ContentHash, lama.ContentHash);
        }

        /// <summary>
        /// Checker yang menyetujui berdasarkan isi lama ditolak ketika isinya sudah berbeda.
        /// </summary>
        [Fact]
        public async Task SidikIsiTidakCocok_KeputusanDitolak()
        {
            var konteks = await SiapkanFolioBersihAsync();
            var permintaan = await BuatDanAjukanVoidAsync(konteks);

            await using var context = _fixture.CreateContext();
            var layanan = new BillingFinancialActionService(context);

            var hasil = await layanan.DecideAsync(
                permintaan.Id,
                new DecideFinancialActionRequest
                {
                    Decision = BillingApprovalDecision.Approve,
                    ExpectedContentHash = new string('A', 64)
                },
                Guid.NewGuid());

            Assert.Equal(BillingServiceResultKind.Conflict, hasil.Kind);
            Assert.Equal("BIL_ACTION_CONTENT_CHANGED", hasil.ErrorCode);
        }

        /// <summary>
        /// Nominal yang disetujui tidak boleh melebihi yang diajukan. Checker boleh mengurangi,
        /// tidak boleh menambah — menambah berarti menyetujui sesuatu yang tidak pernah diajukan
        /// siapa pun.
        /// </summary>
        [Fact]
        public async Task NominalDisetujuiMelebihiDiajukan_Ditolak()
        {
            var konteks = await SiapkanFolioBersihAsync();
            var permintaan = await BuatDanAjukanVoidAsync(konteks);

            await using var context = _fixture.CreateContext();
            var layanan = new BillingFinancialActionService(context);

            var hasil = await layanan.DecideAsync(
                permintaan.Id,
                new DecideFinancialActionRequest
                {
                    Decision = BillingApprovalDecision.Approve,
                    ApprovedAmount = permintaan.RequestedAmount + 1m
                },
                Guid.NewGuid());

            Assert.Equal(BillingServiceResultKind.Validation, hasil.Kind);
            Assert.Equal("BIL_APPROVED_AMOUNT_EXCEEDS_REQUEST", hasil.ErrorCode);
        }

        /// <summary>
        /// Pengiriman ulang dengan kunci idempotensi yang sama mengembalikan permintaan yang
        /// sama, bukan permintaan kedua.
        /// </summary>
        [Fact]
        public async Task KunciIdempotensiSama_TidakMelahirkanPermintaanKedua()
        {
            var konteks = await SiapkanFolioBersihAsync();
            var kunci = $"BE006-UJI-{Guid.NewGuid():N}";

            var permintaan = new CreateFinancialActionRequest
            {
                ActionType = BillingFinancialActionType.Refund,
                FolioId = konteks.FolioId,
                RequestedAmount = 50_000m,
                ReasonCode = "PENGEMBALIAN_KELEBIHAN",
                IdempotencyKey = kunci
            };

            Guid pertamaId;

            await using (var context = _fixture.CreateContext())
            {
                var layanan = new BillingFinancialActionService(context);
                var hasil = await layanan.CreateAsync(permintaan, konteks.Seed.ActorUserId);

                Assert.Equal(BillingServiceResultKind.Success, hasil.Kind);
                pertamaId = hasil.Value!.Id;
            }

            await using (var context = _fixture.CreateContext())
            {
                var layanan = new BillingFinancialActionService(context);
                var hasil = await layanan.CreateAsync(permintaan, konteks.Seed.ActorUserId);

                Assert.Equal(BillingServiceResultKind.Success, hasil.Kind);
                Assert.Equal(pertamaId, hasil.Value!.Id);
            }

            await using var verifikasi = _fixture.CreateContext();

            var jumlah = await verifikasi.BilFinancialActionRequests
                .CountAsync(x => x.IdempotencyKey == kunci && !x.IsDelete);

            Assert.Equal(1, jumlah);
        }

        // =================================================================
        // Pembantu
        // =================================================================

        private sealed record KonteksUji(
            EncounterSeed Seed,
            Guid FolioId,
            Guid ChargeLineId,
            int ChargeLineVersionAwal);

        private BillingFolioClosureService BuatLayananPenutupan(
            QuilvianSystemBackend.Repositories.ApplicationDbContext context) =>
            new(context, new BillingReconciliationService(context));

        /// <summary>
        /// Menyiapkan folio yang benar-benar tidak punya penghalang: hasil pemrosesan sudah pasti,
        /// dan baris tagihannya sudah diakui. Tanpa ini, setiap test penutupan akan gagal karena
        /// alasan yang bukan sedang diuji.
        /// </summary>
        private async Task<KonteksUji> SiapkanFolioBersihAsync()
        {
            var seed = await _fixture.SeedEncounterAsync();
            _seeds.Add(seed);

            var milestoneFactId = Guid.NewGuid();

            await using (var context = _fixture.CreateContext())
            {
                var folioService = new BillingFolioService(context);

                var hasil = await folioService.RecognizeMilestoneAsync(
                    new RecognizeBillingMilestoneRequest
                    {
                        IdempotencyKey = $"FA-TEST-{Guid.NewGuid():N}",
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
            }

            await using var mutate = _fixture.CreateContext();

            var folio = await mutate.BilFolios
                .FirstAsync(x => x.EncounterId == seed.EncounterId && !x.IsDelete);

            var baris = await mutate.BilChargeLines
                .FirstAsync(x => x.FolioId == folio.Id && !x.IsDelete);

            baris.CalculationStatus = BillingChargeCalculationStatus.Recognized;

            var efek = await mutate.BilProcessingEffects
                .Where(x => x.MilestoneFactId == milestoneFactId && !x.IsDelete)
                .ToListAsync();

            foreach (var item in efek)
            {
                item.Outcome = BillingProcessingOutcome.Succeeded;
                item.ErrorCode = null;
                item.ErrorMessage = null;
            }

            await mutate.SaveChangesAsync();

            return new KonteksUji(seed, folio.Id, baris.Id, baris.Version);
        }

        private async Task<FinancialActionRequestResponse> BuatPermintaanAsync(
            KonteksUji konteks,
            BillingFinancialActionType jenis,
            Guid? chargeLineId,
            decimal nominal)
        {
            await using var context = _fixture.CreateContext();
            var layanan = new BillingFinancialActionService(context);

            var hasil = await layanan.CreateAsync(
                new CreateFinancialActionRequest
                {
                    ActionType = jenis,
                    FolioId = konteks.FolioId,
                    ChargeLineId = chargeLineId,
                    RequestedAmount = nominal,
                    ReasonCode = "UJI_BE006",
                    ReasonNote = $"Permintaan {jenis} untuk pengujian."
                },
                konteks.Seed.ActorUserId);

            Assert.Equal(BillingServiceResultKind.Success, hasil.Kind);

            return hasil.Value!;
        }

        private async Task<FinancialActionRequestResponse> AjukanAsync(Guid requestId, Guid makerUserId)
        {
            await using var context = _fixture.CreateContext();
            var layanan = new BillingFinancialActionService(context);

            var hasil = await layanan.SubmitAsync(requestId, makerUserId);

            Assert.Equal(BillingServiceResultKind.Success, hasil.Kind);

            return hasil.Value!;
        }

        private async Task<FinancialActionRequestResponse> BuatDanAjukanVoidAsync(KonteksUji konteks)
        {
            var permintaan = await BuatPermintaanAsync(
                konteks, BillingFinancialActionType.Void, konteks.ChargeLineId, 250_000m);

            return await AjukanAsync(permintaan.Id, konteks.Seed.ActorUserId);
        }

        private async Task SetujuiAsync(Guid requestId, Guid checkerUserId)
        {
            await using var context = _fixture.CreateContext();
            var layanan = new BillingFinancialActionService(context);

            var hasil = await layanan.DecideAsync(
                requestId,
                new DecideFinancialActionRequest { Decision = BillingApprovalDecision.Approve },
                checkerUserId);

            Assert.Equal(BillingServiceResultKind.Success, hasil.Kind);
        }

        private async Task BuatKebijakanAsync(
            BillingFinancialActionType jenis,
            bool memerlukanPersetujuan)
        {
            await using var context = _fixture.CreateContext();

            var kebijakan = new MstBillingApprovalPolicy
            {
                ActionType = jenis,
                PolicyCode = $"UJI-{jenis}-{Guid.NewGuid():N}"[..30],
                PolicyVersion = 1,
                EffectiveStartDate = DateTime.UtcNow.AddDays(-1),
                IsApproved = true,
                ApprovedAt = DateTime.UtcNow.AddDays(-1),
                RequiresApproval = memerlukanPersetujuan,
                ApprovalExpiryMinutes = 0,
                IsActive = true,
                Description = "Kebijakan pengujian RJ-BIL-BE-006."
            };

            context.MstBillingApprovalPolicies.Add(kebijakan);
            await context.SaveChangesAsync();

            _policyIds.Add(kebijakan.Id);
        }

        private async Task PastikanBarisTagihanBerstatusAsync(
            Guid chargeLineId,
            BillingChargeCalculationStatus diharapkan)
        {
            await using var context = _fixture.CreateContext();

            var baris = await context.BilChargeLines
                .AsNoTracking()
                .FirstAsync(x => x.Id == chargeLineId);

            Assert.Equal(diharapkan, baris.CalculationStatus);
        }

        private async Task PastikanFolioBerstatusAsync(
            Guid folioId,
            BillingFolioStatus diharapkan)
        {
            Assert.Equal(diharapkan, await StatusFolioAsync(folioId));
        }

        private async Task<DateTime?> WaktuPelaksanaanTersimpanAsync(Guid requestId)
        {
            await using var context = _fixture.CreateContext();

            return await context.BilFinancialActionRequests
                .AsNoTracking()
                .Where(x => x.Id == requestId)
                .Select(x => x.ExecutedAt)
                .FirstAsync();
        }

        private async Task<BillingFolioStatus> StatusFolioAsync(Guid folioId)
        {
            await using var context = _fixture.CreateContext();

            var folio = await context.BilFolios
                .AsNoTracking()
                .FirstAsync(x => x.Id == folioId);

            return folio.Status;
        }
    }
}
