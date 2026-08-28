using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Enums;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Services;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalBillingIntegration.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalBillingIntegration.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalBillingIntegration.Services;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.BillingTests.Infrastructure;
using QuilvianSystemBackend.Repositories;
using Xunit;

namespace QuilvianSystemBackend.BillingTests.Laboratory
{
    /// <summary>
    /// Acceptance criteria RJ-BIL-BE-003 beserta keputusan author RJ-BIL-OQ-008 sampai OQ-011.
    ///
    /// Skenario yang dibuktikan:
    ///   1. Requested, Collected, dan Received tidak membentuk tagihan pemeriksaan.
    ///   2. Accepted membentuk tepat satu fakta kelayakan tagih.
    ///   3. Rejected menghasilkan nol tagihan pemeriksaan.
    ///   4. Satu pesanan tiga komponen: dua layak dan satu ditolak menagih Rp350.000, bukan
    ///      Rp450.000 dan bukan nol.
    ///   5. Penetapan layak yang diulang tidak menggandakan tagihan.
    ///   6. Pengambilan ulang mempertahankan sampel yang ditolak beserta tautan sebabnya.
    ///   7. Pengambilan ulang karena kesalahan internal tidak menambah tagihan kedua.
    ///   8. Pembatalan setelah layak tidak menghapus tagihan dan memakai revisi baru.
    ///   9. Pembatalan sebelum layak tidak menghasilkan koreksi finansial apa pun.
    ///  10. Perubahan bersamaan atas sampel yang sama ditolak salah satunya.
    /// </summary>
    public sealed class LaboratorySpecimenLifecycleTests
        : IClassFixture<BillingTestDatabaseFixture>, IAsyncLifetime
    {
        private const decimal TarifDarahLengkap = 200_000m;
        private const decimal TarifFungsiHati = 150_000m;
        private const decimal TarifUrinLengkap = 100_000m;

        private readonly BillingTestDatabaseFixture _fixture;
        private readonly List<EncounterSeed> _seeds = new();
        private readonly List<Guid> _procedureIds = new();
        private readonly List<Guid> _tariffIds = new();
        private readonly List<Guid> _tariffCategoryIds = new();

        public LaboratorySpecimenLifecycleTests(BillingTestDatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            foreach (var seed in _seeds)
                await _fixture.CleanupEncounterAsync(seed);

            // Master data dihapus setelah seluruh transaksi, karena FK-nya Restrict.
            await using var context = _fixture.CreateContext();

            await context.Set<MstTariff>()
                .Where(x => _tariffIds.Contains(x.Id))
                .ExecuteDeleteAsync();

            await context.Set<MstProcedure>()
                .Where(x => _procedureIds.Contains(x.Id))
                .ExecuteDeleteAsync();

            await context.Set<MstTariffCategory>()
                .Where(x => _tariffCategoryIds.Contains(x.Id))
                .ExecuteDeleteAsync();
        }

        // =====================================================================
        // Skenario 1 — Requested, Collected, dan Received belum membentuk tagihan
        // =====================================================================

        [Fact]
        public async Task SebelumDinyatakanLayak_TidakAdaTagihanYangTerbentuk()
        {
            var seed = await NewEncounterAsync();
            var procedure = await SeedLabProcedureAsync("Darah lengkap", TarifDarahLengkap);

            await using var context = _fixture.CreateContext();
            var specimenService = CreateSpecimenService(context);
            var order = await SeedOrderAsync(context, seed, procedure.Id);

            var planned = await specimenService.PlanAsync(order.Id, new PlanLabSpecimenRequest());
            await specimenService.CollectAsync(planned.Specimen.Id, new CollectLabSpecimenRequest());
            await specimenService.ReceiveAsync(planned.Specimen.Id, new ReceiveLabSpecimenRequest());

            var jumlahFakta = await context.BilClinicalMilestoneFacts
                .CountAsync(x => x.EncounterId == seed.EncounterId);

            var jumlahFolio = await context.BilFolios
                .CountAsync(x => x.EncounterId == seed.EncounterId);

            Assert.Equal(0, jumlahFakta);
            Assert.Equal(0, jumlahFolio);
        }

        // =====================================================================
        // Skenario 2 — Accepted membentuk tepat satu fakta kelayakan tagih
        // =====================================================================

        [Fact]
        public async Task PenetapanLayak_MembentukTepatSatuFaktaDanSatuBarisTagihan()
        {
            var seed = await NewEncounterAsync();
            var procedure = await SeedLabProcedureAsync("Darah lengkap", TarifDarahLengkap);

            await using var context = _fixture.CreateContext();
            var specimenService = CreateSpecimenService(context);
            var order = await SeedOrderAsync(context, seed, procedure.Id);

            var specimen = await SampaiDiterimaAsync(specimenService, order.Id);
            var hasil = await specimenService.AcceptAsync(specimen.Id, new AcceptLabSpecimenRequest());

            Assert.NotNull(hasil.Handoff);
            Assert.Equal(ClinicalFactEmissionKind.Emitted, hasil.Handoff!.Kind);
            Assert.Equal(1, hasil.Handoff.MilestoneFactVersion);

            var barisTagihan = await context.BilChargeLines
                .Where(x => x.SourceContext == "Laboratory" && x.SourceItemId == specimen.Id)
                .ToListAsync();

            Assert.Single(barisTagihan);

            // Nilai finansialnya belum final. Yang dibuktikan di sini adalah terbentuknya
            // tepat satu baris, bukan besaran yang sudah disetujui siapa pun.
            Assert.Equal(
                BillingChargeCalculationStatus.PendingFinancialReview,
                barisTagihan[0].CalculationStatus);
        }

        // =====================================================================
        // Skenario 3 dan 4 — Rp450.000 dengan satu komponen ditolak menagih Rp350.000
        // =====================================================================

        [Fact]
        public async Task DuaKomponenLayakSatuDitolak_MenagihTigaRatusLimaPuluhRibu()
        {
            var seed = await NewEncounterAsync();
            var darah = await SeedLabProcedureAsync("Darah lengkap", TarifDarahLengkap);
            var hati = await SeedLabProcedureAsync("Fungsi hati", TarifFungsiHati);
            var urin = await SeedLabProcedureAsync("Urin lengkap", TarifUrinLengkap);

            await using var context = _fixture.CreateContext();
            var specimenService = CreateSpecimenService(context);
            var order = await SeedOrderAsync(context, seed, darah.Id);

            var spDarah = await SampaiDiterimaAsync(specimenService, order.Id, darah.Id);
            var spHati = await SampaiDiterimaAsync(specimenService, order.Id, hati.Id);
            var spUrin = await SampaiDiterimaAsync(specimenService, order.Id, urin.Id);

            await specimenService.AcceptAsync(spDarah.Id, new AcceptLabSpecimenRequest());
            await specimenService.AcceptAsync(spHati.Id, new AcceptLabSpecimenRequest());

            await specimenService.RejectAsync(spUrin.Id, new RejectLabSpecimenRequest
            {
                ReasonCode = "INSUFFICIENT_QUANTITY"
            });

            var idKomponenTertagih = await context.BilChargeLines
                .Where(x => x.SourceContext == "Laboratory")
                .Where(x => x.SourceItemId == spDarah.Id ||
                            x.SourceItemId == spHati.Id ||
                            x.SourceItemId == spUrin.Id)
                .Select(x => x.SourceItemId)
                .ToListAsync();

            // Dua komponen yang dikerjakan membentuk baris tagihan; komponen yang ditolak tidak.
            Assert.Equal(2, idKomponenTertagih.Count);
            Assert.Contains(spDarah.Id, idKomponenTertagih);
            Assert.Contains(spHati.Id, idKomponenTertagih);
            Assert.DoesNotContain(spUrin.Id, idKomponenTertagih);

            // Nilai rujukan yang diserahkan ke Billing berjumlah Rp350.000, bukan Rp450.000.
            var totalRujukan = await context.LabSpecimens
                .Where(x => x.LabOrderId == order.Id && x.SpecimenStatus == LabSpecimenStatus.Accepted)
                .SumAsync(x => x.UnitPriceSnapshot ?? 0m);

            Assert.Equal(350_000m, totalRujukan);
        }

        [Fact]
        public async Task SampelDitolak_TidakMenerbitkanFaktaApaPun()
        {
            var seed = await NewEncounterAsync();
            var procedure = await SeedLabProcedureAsync("Urin lengkap", TarifUrinLengkap);

            await using var context = _fixture.CreateContext();
            var specimenService = CreateSpecimenService(context);
            var order = await SeedOrderAsync(context, seed, procedure.Id);

            var specimen = await SampaiDiterimaAsync(specimenService, order.Id);

            var hasil = await specimenService.RejectAsync(specimen.Id, new RejectLabSpecimenRequest
            {
                ReasonCode = "SPECIMEN_INTEGRITY_OR_QUALITY_ISSUE",
                Note = "Sampel hemolisis."
            });

            Assert.Null(hasil.Handoff);
            Assert.Equal(LabSpecimenStatus.Rejected, hasil.Specimen.SpecimenStatus);

            var adaFakta = await context.BilClinicalMilestoneFacts
                .AnyAsync(x => x.SourceItemId == specimen.Id);

            Assert.False(adaFakta);
        }

        [Fact]
        public async Task AlasanPenolakanOther_WajibDisertaiCatatan()
        {
            var seed = await NewEncounterAsync();
            var procedure = await SeedLabProcedureAsync("Urin lengkap", TarifUrinLengkap);

            await using var context = _fixture.CreateContext();
            var specimenService = CreateSpecimenService(context);
            var order = await SeedOrderAsync(context, seed, procedure.Id);

            var specimen = await SampaiDiterimaAsync(specimenService, order.Id);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                specimenService.RejectAsync(specimen.Id, new RejectLabSpecimenRequest
                {
                    ReasonCode = "OTHER"
                }));
        }

        [Fact]
        public async Task AlasanPenolakanTidakDikenal_Ditolak()
        {
            var seed = await NewEncounterAsync();
            var procedure = await SeedLabProcedureAsync("Urin lengkap", TarifUrinLengkap);

            await using var context = _fixture.CreateContext();
            var specimenService = CreateSpecimenService(context);
            var order = await SeedOrderAsync(context, seed, procedure.Id);

            var specimen = await SampaiDiterimaAsync(specimenService, order.Id);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                specimenService.RejectAsync(specimen.Id, new RejectLabSpecimenRequest
                {
                    ReasonCode = "ALASAN_KARANGAN"
                }));
        }

        // =====================================================================
        // Skenario 5 — Penetapan layak yang diulang tidak menggandakan tagihan
        // =====================================================================

        [Fact]
        public async Task PenetapanLayakDiulang_TidakMenggandakanTagihan()
        {
            var seed = await NewEncounterAsync();
            var procedure = await SeedLabProcedureAsync("Darah lengkap", TarifDarahLengkap);

            await using var context = _fixture.CreateContext();
            var specimenService = CreateSpecimenService(context);
            var order = await SeedOrderAsync(context, seed, procedure.Id);

            var specimen = await SampaiDiterimaAsync(specimenService, order.Id);

            var pertama = await specimenService.AcceptAsync(specimen.Id, new AcceptLabSpecimenRequest());
            var kedua = await specimenService.AcceptAsync(specimen.Id, new AcceptLabSpecimenRequest());

            Assert.Equal(ClinicalFactEmissionKind.Emitted, pertama.Handoff!.Kind);
            Assert.Equal(ClinicalFactEmissionKind.Replayed, kedua.Handoff!.Kind);

            // Identitas fakta stabil: pengulangan tidak membuat revisi baru.
            Assert.Equal(pertama.Handoff.MilestoneFactId, kedua.Handoff.MilestoneFactId);
            Assert.Equal(1, kedua.Handoff.MilestoneFactVersion);

            var jumlahBaris = await context.BilChargeLines
                .CountAsync(x => x.SourceContext == "Laboratory" && x.SourceItemId == specimen.Id);

            Assert.Equal(1, jumlahBaris);
        }

        // =====================================================================
        // Skenario 6 dan 7 — Pengambilan ulang
        // =====================================================================

        [Fact]
        public async Task PengambilanUlang_MempertahankanSampelDitolakDanTautanSebabnya()
        {
            var seed = await NewEncounterAsync();
            var procedure = await SeedLabProcedureAsync("Darah lengkap", TarifDarahLengkap);

            await using var context = _fixture.CreateContext();
            var specimenService = CreateSpecimenService(context);
            var order = await SeedOrderAsync(context, seed, procedure.Id);

            var asli = await SampaiDiterimaAsync(specimenService, order.Id);

            await specimenService.RejectAsync(asli.Id, new RejectLabSpecimenRequest
            {
                ReasonCode = "LABELING_ISSUE"
            });

            var pengganti = await specimenService.RequestRecollectionAsync(
                asli.Id,
                new RequestLabRecollectionRequest { Cause = LabRecollectionCause.InternalHospitalError });

            var sampelAsli = await context.LabSpecimens.AsNoTracking()
                .FirstAsync(x => x.Id == asli.Id);

            // Sampel yang ditolak tidak dihapus dan alasan penolakannya tidak ditimpa.
            Assert.Equal(LabSpecimenStatus.RecollectionRequired, sampelAsli.SpecimenStatus);
            Assert.Equal("LABELING_ISSUE", sampelAsli.RejectionReasonCode);

            // Sampel pengganti punya identitas dan barcode baru, dengan tautan sebab yang tetap.
            Assert.NotEqual(asli.Id, pengganti.Specimen.Id);
            Assert.NotEqual(asli.SpecimenBarcode, pengganti.Specimen.SpecimenBarcode);
            Assert.Equal(asli.Id, pengganti.Specimen.SupersededSpecimenId);
            Assert.Equal(LabSpecimenStatus.Planned, pengganti.Specimen.SpecimenStatus);

            // Riwayat penolakan tetap terbaca setelah pengambilan ulang.
            var riwayat = await context.LabTransitionHistories.AsNoTracking()
                .Where(x => x.LabSpecimenId == asli.Id)
                .Select(x => x.Action)
                .ToListAsync();

            Assert.Contains("Specimen.Reject", riwayat);
            Assert.Contains("Specimen.RequestRecollection", riwayat);
        }

        [Fact]
        public async Task PengambilanUlangKesalahanInternal_HanyaMenghasilkanSatuTagihan()
        {
            var seed = await NewEncounterAsync();
            var procedure = await SeedLabProcedureAsync("Darah lengkap", TarifDarahLengkap);

            await using var context = _fixture.CreateContext();
            var specimenService = CreateSpecimenService(context);
            var order = await SeedOrderAsync(context, seed, procedure.Id);

            var asli = await SampaiDiterimaAsync(specimenService, order.Id);

            await specimenService.RejectAsync(asli.Id, new RejectLabSpecimenRequest
            {
                ReasonCode = "COLLECTION_ISSUE"
            });

            var pengganti = await specimenService.RequestRecollectionAsync(
                asli.Id,
                new RequestLabRecollectionRequest { Cause = LabRecollectionCause.InternalHospitalError });

            await specimenService.CollectAsync(pengganti.Specimen.Id, new CollectLabSpecimenRequest());
            await specimenService.ReceiveAsync(pengganti.Specimen.Id, new ReceiveLabSpecimenRequest());
            await specimenService.AcceptAsync(pengganti.Specimen.Id, new AcceptLabSpecimenRequest());

            var jumlahBaris = await context.BilChargeLines
                .CountAsync(x => x.SourceContext == "Laboratory" &&
                                 (x.SourceItemId == asli.Id || x.SourceItemId == pengganti.Specimen.Id));

            // Pemeriksaan hanya benar-benar dikerjakan satu kali, sehingga tagihannya satu.
            // Percobaan yang gagal karena kesalahan rumah sakit tidak menambah tanggungan pasien.
            Assert.Equal(1, jumlahBaris);
        }

        [Fact]
        public async Task PengambilanUlangSebabEksternal_WajibMenyertakanAlasan()
        {
            var seed = await NewEncounterAsync();
            var procedure = await SeedLabProcedureAsync("Darah lengkap", TarifDarahLengkap);

            await using var context = _fixture.CreateContext();
            var specimenService = CreateSpecimenService(context);
            var order = await SeedOrderAsync(context, seed, procedure.Id);

            var asli = await SampaiDiterimaAsync(specimenService, order.Id);

            await specimenService.RejectAsync(asli.Id, new RejectLabSpecimenRequest
            {
                ReasonCode = "INSUFFICIENT_QUANTITY"
            });

            await Assert.ThrowsAsync<ArgumentException>(() =>
                specimenService.RequestRecollectionAsync(
                    asli.Id,
                    new RequestLabRecollectionRequest
                    {
                        Cause = LabRecollectionCause.PatientOrSpecimenCondition
                    }));
        }

        // =====================================================================
        // Skenario 8 dan 9 — Pembatalan klinis
        // =====================================================================

        [Fact]
        public async Task PembatalanSetelahLayak_TidakMenghapusTagihanDanMemakaiRevisiBaru()
        {
            var seed = await NewEncounterAsync();
            var procedure = await SeedLabProcedureAsync("Darah lengkap", TarifDarahLengkap);

            await using var context = _fixture.CreateContext();
            var specimenService = CreateSpecimenService(context);
            var order = await SeedOrderAsync(context, seed, procedure.Id);

            var specimen = await SampaiDiterimaAsync(specimenService, order.Id);
            var layak = await specimenService.AcceptAsync(specimen.Id, new AcceptLabSpecimenRequest());

            var idTagihanAsli = layak.Handoff!.MilestoneFactId;

            var batal = await specimenService.CancelAsync(
                specimen.Id,
                new CancelLabSpecimenRequest { Reason = "Pasien pulang atas permintaan sendiri." });

            Assert.NotNull(batal.Handoff);
            Assert.Equal(ClinicalFactEmissionKind.Emitted, batal.Handoff!.Kind);

            // Identitas fakta tetap sama; yang bertambah adalah versinya.
            Assert.Equal(idTagihanAsli, batal.Handoff.MilestoneFactId);
            Assert.Equal(2, batal.Handoff.MilestoneFactVersion);

            // Tagihan asli tidak dihapus. Laboratorium tidak memiliki kewenangan menghapusnya.
            var barisTagihan = await context.BilChargeLines
                .Where(x => x.SourceContext == "Laboratory" && x.SourceItemId == specimen.Id)
                .ToListAsync();

            Assert.Single(barisTagihan);
        }

        [Fact]
        public async Task PembatalanSebelumLayak_TidakMenghasilkanKoreksiFinansial()
        {
            var seed = await NewEncounterAsync();
            var procedure = await SeedLabProcedureAsync("Darah lengkap", TarifDarahLengkap);

            await using var context = _fixture.CreateContext();
            var specimenService = CreateSpecimenService(context);
            var order = await SeedOrderAsync(context, seed, procedure.Id);

            var specimen = await SampaiDiterimaAsync(specimenService, order.Id);

            var batal = await specimenService.CancelAsync(specimen.Id, new CancelLabSpecimenRequest());

            // Tidak pernah ada tagihan, sehingga tidak ada apa pun yang perlu dikoreksi.
            Assert.Null(batal.Handoff);

            var jumlahBaris = await context.BilChargeLines
                .CountAsync(x => x.SourceContext == "Laboratory" && x.SourceItemId == specimen.Id);

            Assert.Equal(0, jumlahBaris);
        }

        [Fact]
        public async Task PembatalanPesanan_MembatalkanSampelDanMenerbitkanKoreksiUntukYangSudahLayak()
        {
            var seed = await NewEncounterAsync();
            var darah = await SeedLabProcedureAsync("Darah lengkap", TarifDarahLengkap);
            var urin = await SeedLabProcedureAsync("Urin lengkap", TarifUrinLengkap);

            await using var context = _fixture.CreateContext();
            var specimenService = CreateSpecimenService(context);
            var orderService = CreateOrderService(context, specimenService);
            var order = await SeedOrderAsync(context, seed, darah.Id);

            var spLayak = await SampaiDiterimaAsync(specimenService, order.Id, darah.Id);
            var spBelum = await SampaiDiterimaAsync(specimenService, order.Id, urin.Id);

            await specimenService.AcceptAsync(spLayak.Id, new AcceptLabSpecimenRequest());

            var hasil = await orderService.CancelAsync(
                order.Id,
                new CancelLabSpecimenRequest { Reason = "Pemeriksaan dibatalkan dokter." });

            // Hanya sampel yang sudah layak yang menghasilkan fakta pembatalan.
            Assert.Single(hasil.BillingHandoffs);
            Assert.Equal(2, hasil.BillingHandoffs[0].MilestoneFactVersion);

            var statusSampel = await context.LabSpecimens.AsNoTracking()
                .Where(x => x.LabOrderId == order.Id)
                .Select(x => x.SpecimenStatus)
                .ToListAsync();

            Assert.All(statusSampel, status => Assert.Equal(LabSpecimenStatus.Cancelled, status));
            Assert.Equal(2, statusSampel.Count);
            Assert.Equal("Cancelled", hasil.Order.OrderStatus);

            _ = spBelum;
        }

        // =====================================================================
        // Skenario 10 — Konkurensi
        // =====================================================================

        [Fact]
        public async Task DuaPetugasMenetapkanLayakBersamaan_SalahSatuDitolak()
        {
            var seed = await NewEncounterAsync();
            var procedure = await SeedLabProcedureAsync("Darah lengkap", TarifDarahLengkap);

            await using var context = _fixture.CreateContext();
            var specimenService = CreateSpecimenService(context);
            var order = await SeedOrderAsync(context, seed, procedure.Id);

            var specimen = await SampaiDiterimaAsync(specimenService, order.Id);

            // Dua context terpisah mewakili dua petugas yang membuka sampel yang sama.
            await using var contextA = _fixture.CreateContext();
            await using var contextB = _fixture.CreateContext();

            var layananA = CreateSpecimenService(contextA);
            var layananB = CreateSpecimenService(contextB);

            // Petugas kedua membuka layarnya lebih dulu, sehingga context-nya sudah memegang
            // baris berversi lama. Tanpa langkah ini konflik versi tidak pernah terbentuk:
            // service memuat ulang sampel di dalam pemanggilannya, sehingga petugas kedua akan
            // membaca keadaan terbaru dan yang menolak adalah penjaga status, bukan penjaga
            // versi. Yang hendak dibuktikan test ini justru penjaga versinya.
            var terbukaDiLayarPetugasKedua = await contextB.LabSpecimens
                .FirstOrDefaultAsync(x => x.Id == specimen.Id);

            Assert.NotNull(terbukaDiLayarPetugasKedua);
            var versiSaatDibuka = terbukaDiLayarPetugasKedua!.Version;

            // Petugas pertama menyimpan lebih dulu dan menaikkan versi baris.
            await layananA.AcceptAsync(specimen.Id, new AcceptLabSpecimenRequest());

            // Petugas kedua menyimpan sambil memegang versi lama, sehingga harus ditolak.
            var galat = await Assert.ThrowsAsync<LabConcurrencyException>(() =>
                layananB.AcceptAsync(specimen.Id, new AcceptLabSpecimenRequest()));

            Assert.Contains("diubah oleh petugas lain", galat.Message);

            // Bagian terpenting: keputusan ganda tidak boleh menghasilkan tagihan ganda.
            await using var verify = _fixture.CreateContext();

            var barisTagihan = await verify.BilChargeLines
                .Where(x => x.SourceContext == "Laboratory" && x.SourceItemId == specimen.Id)
                .ToListAsync();

            Assert.Single(barisTagihan);

            var tersimpan = await verify.LabSpecimens.FirstAsync(x => x.Id == specimen.Id);

            Assert.Equal(LabSpecimenStatus.Accepted, tersimpan.SpecimenStatus);
            Assert.True(
                tersimpan.Version > versiSaatDibuka,
                "Versi baris seharusnya naik setelah petugas pertama menyimpan.");
        }

        // =====================================================================
        // Batas kewenangan
        // =====================================================================

        [Fact]
        public async Task PesananYangSudahDibatalkan_TidakDapatMenerimaSampelBaru()
        {
            var seed = await NewEncounterAsync();
            var procedure = await SeedLabProcedureAsync("Darah lengkap", TarifDarahLengkap);

            await using var context = _fixture.CreateContext();
            var specimenService = CreateSpecimenService(context);
            var orderService = CreateOrderService(context, specimenService);
            var order = await SeedOrderAsync(context, seed, procedure.Id);

            await orderService.CancelAsync(order.Id);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                specimenService.PlanAsync(order.Id, new PlanLabSpecimenRequest()));
        }

        [Fact]
        public async Task PenetapanLayakTanpaMelaluiPenerimaan_Ditolak()
        {
            var seed = await NewEncounterAsync();
            var procedure = await SeedLabProcedureAsync("Darah lengkap", TarifDarahLengkap);

            await using var context = _fixture.CreateContext();
            var specimenService = CreateSpecimenService(context);
            var order = await SeedOrderAsync(context, seed, procedure.Id);

            var planned = await specimenService.PlanAsync(order.Id, new PlanLabSpecimenRequest());

            // Sampel yang baru direncanakan belum pernah sampai di laboratorium.
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                specimenService.AcceptAsync(planned.Specimen.Id, new AcceptLabSpecimenRequest()));
        }

        [Fact]
        public async Task ProcedureBukanLaboratorium_TidakDapatDipakaiSebagaiKomponen()
        {
            var seed = await NewEncounterAsync();
            var lab = await SeedLabProcedureAsync("Darah lengkap", TarifDarahLengkap);
            var bukanLab = await SeedLabProcedureAsync("Tindakan umum", 50_000m, isLaboratory: false);

            await using var context = _fixture.CreateContext();
            var specimenService = CreateSpecimenService(context);
            var order = await SeedOrderAsync(context, seed, lab.Id);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                specimenService.PlanAsync(order.Id, new PlanLabSpecimenRequest
                {
                    ProcedureId = bukanLab.Id
                }));
        }

        [Fact]
        public async Task BarcodeSampel_UnikDanTidakMemuatIdentitasPasien()
        {
            var seed = await NewEncounterAsync();
            var procedure = await SeedLabProcedureAsync("Darah lengkap", TarifDarahLengkap);

            await using var context = _fixture.CreateContext();
            var specimenService = CreateSpecimenService(context);
            var order = await SeedOrderAsync(context, seed, procedure.Id);

            var pertama = await specimenService.PlanAsync(order.Id, new PlanLabSpecimenRequest());
            var kedua = await specimenService.PlanAsync(order.Id, new PlanLabSpecimenRequest());

            Assert.NotEqual(pertama.Specimen.SpecimenBarcode, kedua.Specimen.SpecimenBarcode);
            Assert.Matches("^LSP-[0-9A-F]{32}$", pertama.Specimen.SpecimenBarcode);

            var pasien = await context.MstPatients.AsNoTracking()
                .FirstAsync(x => x.Id == seed.PatientId);

            // Barcode tidak boleh dapat dipakai membaca identitas pasien.
            Assert.DoesNotContain(pasien.MedicalRecordNumber, pertama.Specimen.SpecimenBarcode);
            Assert.DoesNotContain(pasien.PatientCode, pertama.Specimen.SpecimenBarcode);
            Assert.DoesNotContain(
                seed.EncounterId.ToString("N"),
                pertama.Specimen.SpecimenBarcode,
                StringComparison.OrdinalIgnoreCase);
        }

        // =====================================================================
        // Pembantu
        // =====================================================================

        /// <summary>
        /// Identitas petugas yang dipakai seluruh service pada test ini.
        ///
        /// Nilainya diambil dari encounter yang baru saja dibuat, sehingga merupakan pengguna
        /// yang benar-benar ada di database. Tanpa ini, service mengambil identitas dari klaim
        /// pada HTTP context yang kosong, mendapat <c>Guid.Empty</c>, dan seluruh penyerahan
        /// fakta ke Billing ditolak sebagai <c>CLIN_FACT_ACTOR_INVALID</c> sebelum satu pun
        /// perilaku domain sempat diuji.
        /// </summary>
        private Guid _actorUserId = Guid.NewGuid();

        private async Task<EncounterSeed> NewEncounterAsync()
        {
            var seed = await _fixture.SeedEncounterAsync();
            _seeds.Add(seed);
            _actorUserId = seed.ActorUserId;
            return seed;
        }

        private LabSpecimenService CreateSpecimenService(ApplicationDbContext context) =>
            new(
                context,
                new ClinicalMilestoneFactProducer(
                    context,
                    new BillingFolioService(context),
                    BillingTestDatabaseFixture.CreateLoggerService()),
                BillingTestDatabaseFixture.CreateHttpContextAccessor(_actorUserId),
                BillingTestDatabaseFixture.CreateLoggerService());

        private LabOrderService CreateOrderService(
            ApplicationDbContext context,
            LabSpecimenService specimenService) =>
            new(
                context,
                specimenService,
                BillingTestDatabaseFixture.CreateHttpContextAccessor(_actorUserId),
                BillingTestDatabaseFixture.CreateLoggerService());

        private async Task<MstProcedure> SeedLabProcedureAsync(
            string nama,
            decimal tarif,
            bool isLaboratory = true)
        {
            await using var context = _fixture.CreateContext();

            var suffix = Guid.NewGuid().ToString("N")[..10];

            var category = new MstTariffCategory
            {
                Id = Guid.NewGuid(),
                TariffCategoryCode = $"TC{suffix}",
                TariffCategoryName = $"Kategori Test {suffix}"
            };

            var procedure = new MstProcedure
            {
                Id = Guid.NewGuid(),
                ProcedureCode = $"LB{suffix}",
                ProcedureName = nama,
                ProcedureType = isLaboratory ? "Laboratory" : "General",
                IsLaboratory = isLaboratory,
                IsActive = true
            };

            var tariff = new MstTariff
            {
                Id = Guid.NewGuid(),
                TariffCode = $"TR{suffix}",
                TariffName = $"Tarif {nama}",
                TariffCategoryId = category.Id,
                ProcedureId = procedure.Id,
                NormalPrice = tarif
            };

            context.Set<MstTariffCategory>().Add(category);
            context.Set<MstProcedure>().Add(procedure);
            await context.SaveChangesAsync();

            context.Set<MstTariff>().Add(tariff);
            await context.SaveChangesAsync();

            _tariffCategoryIds.Add(category.Id);
            _procedureIds.Add(procedure.Id);
            _tariffIds.Add(tariff.Id);

            return procedure;
        }

        private static async Task<LabOrder> SeedOrderAsync(
            ApplicationDbContext context,
            EncounterSeed seed,
            Guid procedureId)
        {
            var order = new LabOrder
            {
                Id = Guid.NewGuid(),
                EncounterId = seed.EncounterId,
                ProcedureId = procedureId,
                OrderStatus = LabOrderStatus.Requested,
                RequestedAt = DateTime.UtcNow,
                RequestedByUserId = seed.ActorUserId,
                CreateBy = seed.ActorUserId
            };

            context.LabOrders.Add(order);
            await context.SaveChangesAsync();

            return order;
        }

        /// <summary>
        /// Membawa satu sampel baru sampai berstatus Received, yaitu tepat satu langkah
        /// sebelum milestone kelayakan tagih.
        /// </summary>
        private static async Task<LabSpecimen> SampaiDiterimaAsync(
            LabSpecimenService service,
            Guid labOrderId,
            Guid? procedureId = null)
        {
            var planned = await service.PlanAsync(
                labOrderId,
                new PlanLabSpecimenRequest { ProcedureId = procedureId });

            await service.CollectAsync(planned.Specimen.Id, new CollectLabSpecimenRequest());
            var received = await service.ReceiveAsync(planned.Specimen.Id, new ReceiveLabSpecimenRequest());

            return received.Specimen;
        }
    }
}
