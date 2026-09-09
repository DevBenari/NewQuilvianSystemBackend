using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Enums;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Services;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.BillingTests.Infrastructure;
using QuilvianSystemBackend.Repositories;
using Xunit;
using System.Security.Claims;

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
    [Collection(PostgresIntegrationTestCollection.Name)]
    public sealed class LaboratorySpecimenLifecycleTests
        : IAsyncLifetime
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

            var jumlahFakta = await context.CliClinicalMilestoneFacts
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

            var collectorUserId = seed.ActorUserId;
            var verifierUserId = Guid.NewGuid();

            var collectorService =
                CreateSpecimenService(context, collectorUserId);

            var order = await SeedOrderAsync(
                context,
                seed,
                procedure.Id);

            var specimen = await SampaiDiterimaAsync(
                collectorService,
                order.Id);

            var verifierService =
                CreateSpecimenService(context, verifierUserId);

            var hasil = await verifierService.AcceptAsync(
                specimen.Id,
                new AcceptLabSpecimenRequest());

            Assert.NotNull(hasil.Handoff);
            var emission = hasil.Handoff!.Perwakilan;

            Assert.True(
                emission.Kind == ClinicalFactEmissionKind.Emitted,
                $"Expected Emitted, actual {emission.Kind}. " +
                $"Code={emission.Code ?? "<null>"}; " +
                $"Message={emission.Message ?? "<null>"}");
            Assert.Equal(1, hasil.Handoff.Perwakilan.MilestoneFactVersion);

            var examinationId = await context.LabExaminations
                .AsNoTracking()
                .Where(x =>
                    x.SpecimenId == specimen.Id &&
                    !x.IsDelete)
                .Select(x => x.Id)
                .SingleAsync();

            var barisTagihan = await context.BilChargeLines
                .Where(x =>
                    x.SourceContext == "Laboratory" &&
                    x.SourceItemId == examinationId)
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

            var collectorUserId = seed.ActorUserId;
            var verifierUserId = Guid.NewGuid();

            var collectorService =
                CreateSpecimenService(context, collectorUserId);

            var order = await SeedOrderAsync(
                context,
                seed,
                darah.Id);

            var spDarah = await SampaiDiterimaAsync(
                collectorService,
                order.Id,
                darah.Id);

            var spHati = await SampaiDiterimaAsync(
                collectorService,
                order.Id,
                hati.Id);

            var spUrin = await SampaiDiterimaAsync(
                collectorService,
                order.Id,
                urin.Id);

            var verifierService =
                CreateSpecimenService(context, verifierUserId);

            await verifierService.AcceptAsync(
                spDarah.Id,
                new AcceptLabSpecimenRequest());

            await verifierService.AcceptAsync(
                spHati.Id,
                new AcceptLabSpecimenRequest());

            await verifierService.RejectAsync(
                spUrin.Id,
                new RejectLabSpecimenRequest
                {
                    ReasonCode = "INSUFFICIENT_QUANTITY"
                });

            var examinationDarahId = await context.LabExaminations
                .AsNoTracking()
                .Where(x =>
                    x.SpecimenId == spDarah.Id &&
                    !x.IsDelete)
                .Select(x => x.Id)
                .SingleAsync();

            var examinationHatiId = await context.LabExaminations
                .AsNoTracking()
                .Where(x =>
                    x.SpecimenId == spHati.Id &&
                    !x.IsDelete)
                .Select(x => x.Id)
                .SingleAsync();

            var examinationUrinId = await context.LabExaminations
                .AsNoTracking()
                .Where(x =>
                    x.SpecimenId == spUrin.Id &&
                    !x.IsDelete)
                .Select(x => x.Id)
                .SingleAsync();

            var examinationIds = new[]
            {
                examinationDarahId,
                examinationHatiId,
                examinationUrinId
            };

            var idKomponenTertagih = await context.BilChargeLines
            .Where(x =>
                x.SourceContext == "Laboratory" &&
                x.SourceItemId.HasValue &&
                examinationIds.Contains(x.SourceItemId.Value))
            .Select(x => x.SourceItemId!.Value)
            .ToListAsync();

            Assert.Equal(2, idKomponenTertagih.Count);
            Assert.Contains(examinationDarahId, idKomponenTertagih);
            Assert.Contains(examinationHatiId, idKomponenTertagih);
            Assert.DoesNotContain(examinationUrinId, idKomponenTertagih);

            // Nilai rujukan yang diserahkan ke Billing berjumlah Rp350.000, bukan Rp450.000.
            //
            // Sejak BE-LAB-11 salinan tarif tidak lagi ada pada wadah, sehingga jumlahnya
            // diambil dari baris pemeriksaan yang ditopang wadah yang dinyatakan layak.
            var idWadahLayak = await context.LabSpecimens
                .Where(x => x.LabOrderId == order.Id && x.SpecimenStatus == LabSpecimenStatus.Accepted)
                .Select(x => x.Id)
                .ToListAsync();

            var totalRujukan = await context.LabExaminations
                .Where(x => idWadahLayak.Contains(x.SpecimenId) && !x.IsDelete)
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

            var adaFakta = await context.CliClinicalMilestoneFacts
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

            var galat = await Assert.ThrowsAsync<LabSpecimenValidationException>(() =>
                specimenService.RejectAsync(
                    specimen.Id,
                    new RejectLabSpecimenRequest
                    {
                        ReasonCode = "OTHER"
                    }));

            Assert.Contains(
                "membutuhkan keterangan tambahan",
                galat.Message);
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

            var galat = await Assert.ThrowsAsync<LabSpecimenValidationException>(() =>
                specimenService.RejectAsync(
                    specimen.Id,
                    new RejectLabSpecimenRequest
                    {
                        ReasonCode = "ALASAN_KARANGAN"
                    }));

            Assert.Contains(
                "tidak berlaku",
                galat.Message);
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

            var collectorUserId = seed.ActorUserId;
            var verifierUserId = Guid.NewGuid();

            var collectorService =
                CreateSpecimenService(context, collectorUserId);

            var order = await SeedOrderAsync(
                context,
                seed,
                procedure.Id);

            var specimen = await SampaiDiterimaAsync(
                collectorService,
                order.Id);

            var verifierService =
                CreateSpecimenService(context, verifierUserId);

            var pertama = await verifierService.AcceptAsync(
                specimen.Id,
                new AcceptLabSpecimenRequest());

            var kedua = await verifierService.AcceptAsync(
                specimen.Id,
                new AcceptLabSpecimenRequest());

            Assert.Equal(ClinicalFactEmissionKind.Emitted, pertama.Handoff!.Perwakilan.Kind);
            Assert.Equal(ClinicalFactEmissionKind.Replayed, kedua.Handoff!.Perwakilan.Kind);

            // Identitas fakta stabil: pengulangan tidak membuat revisi baru.
            Assert.Equal(pertama.Handoff.Perwakilan.MilestoneFactId, kedua.Handoff.Perwakilan.MilestoneFactId);
            Assert.Equal(1, kedua.Handoff.Perwakilan.MilestoneFactVersion);

            var examinationId = await context.LabExaminations
                .AsNoTracking()
                .Where(x =>
                    x.SpecimenId == specimen.Id &&
                    !x.IsDelete)
                .Select(x => x.Id)
                .SingleAsync();

            var jumlahBaris = await context.BilChargeLines
                .CountAsync(x =>
                    x.SourceContext == "Laboratory" &&
                    x.SourceItemId == examinationId);

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
            var procedure = await SeedLabProcedureAsync(
                "Darah lengkap",
                TarifDarahLengkap);

            await using var context = _fixture.CreateContext();

            var collectorUserId = seed.ActorUserId;
            var verifierUserId = Guid.NewGuid();

            var collectorService =
                CreateSpecimenService(
                    context,
                    collectorUserId);

            var order = await SeedOrderAsync(
                context,
                seed,
                procedure.Id);

            var asli = await SampaiDiterimaAsync(
                collectorService,
                order.Id);

            await collectorService.RejectAsync(
                asli.Id,
                new RejectLabSpecimenRequest
                {
                    ReasonCode = "COLLECTION_ISSUE"
                });

            var pengganti = await collectorService.RequestRecollectionAsync(
                asli.Id,
                new RequestLabRecollectionRequest
                {
                    Cause = LabRecollectionCause.InternalHospitalError
                });

            await collectorService.CollectAsync(
                pengganti.Specimen.Id,
                new CollectLabSpecimenRequest());

            await collectorService.ReceiveAsync(
                pengganti.Specimen.Id,
                new ReceiveLabSpecimenRequest());

            var verifierService =
                CreateSpecimenService(
                    context,
                    verifierUserId);

            var layak = await verifierService.AcceptAsync(
                pengganti.Specimen.Id,
                new AcceptLabSpecimenRequest());

            Assert.NotNull(layak.Handoff);

            Assert.Equal(
                ClinicalFactEmissionKind.Emitted,
                layak.Handoff!.Perwakilan.Kind);

            var examinationIds = await context.LabExaminations
                .AsNoTracking()
                .Where(x =>
                    (x.SpecimenId == asli.Id ||
                    x.SpecimenId == pengganti.Specimen.Id) &&
                    !x.IsDelete)
                .Select(x => x.Id)
                .ToListAsync();

            var jumlahBaris = await context.BilChargeLines
                .CountAsync(x =>
                    x.SourceContext == "Laboratory" &&
                    x.SourceItemId.HasValue &&
                    examinationIds.Contains(x.SourceItemId.Value));

            // Pemeriksaan hanya benar-benar dikerjakan satu kali,
            // sehingga tagihannya satu.
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

            var galat = await Assert.ThrowsAsync<LabSpecimenValidationException>(() =>
                specimenService.RequestRecollectionAsync(
                    asli.Id,
                    new RequestLabRecollectionRequest
                    {
                        Cause = LabRecollectionCause.PatientOrSpecimenCondition
                    }));

            Assert.Contains(
                "membutuhkan alasan tertulis",
                galat.Message);
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

            var collectorUserId = seed.ActorUserId;
            var verifierUserId = Guid.NewGuid();

            var collectorService =
                CreateSpecimenService(context, collectorUserId);

            var order = await SeedOrderAsync(
                context,
                seed,
                procedure.Id);

            var specimen = await SampaiDiterimaAsync(
                collectorService,
                order.Id);

            var verifierService =
                CreateSpecimenService(context, verifierUserId);

            var layak = await verifierService.AcceptAsync(
                specimen.Id,
                new AcceptLabSpecimenRequest());

            Assert.NotNull(layak.Handoff);
            Assert.Equal(
                ClinicalFactEmissionKind.Emitted,
                layak.Handoff!.Perwakilan.Kind);

            var idTagihanAsli =
                layak.Handoff.Perwakilan.MilestoneFactId;

            var batal = await verifierService.CancelAsync(
                specimen.Id,
                new CancelLabSpecimenRequest
                {
                    Reason = "Pasien pulang atas permintaan sendiri."
                });

            Assert.NotNull(batal.Handoff);
            Assert.Equal(ClinicalFactEmissionKind.Emitted, batal.Handoff!.Perwakilan.Kind);

            // Identitas fakta tetap sama; yang bertambah adalah versinya.
            Assert.Equal(idTagihanAsli, batal.Handoff.Perwakilan.MilestoneFactId);
            Assert.Equal(2, batal.Handoff.Perwakilan.MilestoneFactVersion);

            // Tagihan asli tidak dihapus. Laboratorium tidak memiliki kewenangan menghapusnya.
            var examinationId = await context.LabExaminations
                .AsNoTracking()
                .Where(x =>
                    x.SpecimenId == specimen.Id &&
                    !x.IsDelete)
                .Select(x => x.Id)
                .SingleAsync();

            var barisTagihan = await context.BilChargeLines
                .Where(x =>
                    x.SourceContext == "Laboratory" &&
                    x.SourceItemId == examinationId)
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
            var darah = await SeedLabProcedureAsync(
                "Darah lengkap",
                TarifDarahLengkap);

            var urin = await SeedLabProcedureAsync(
                "Urin lengkap",
                TarifUrinLengkap);

            await using var context = _fixture.CreateContext();

            var collectorUserId = seed.ActorUserId;
            var verifierUserId = Guid.NewGuid();

            // Petugas pertama: mengambil dan menerima sampel.
            var collectorService =
                CreateSpecimenService(
                    context,
                    collectorUserId);

            var order = await SeedOrderAsync(
                context,
                seed,
                darah.Id);

            var spLayak = await SampaiDiterimaAsync(
                collectorService,
                order.Id,
                darah.Id);

            var spBelum = await SampaiDiterimaAsync(
                collectorService,
                order.Id,
                urin.Id);

            // Petugas kedua: menetapkan sampel darah sebagai layak.
            var verifierService =
                CreateSpecimenService(
                    context,
                    verifierUserId);

            var layak = await verifierService.AcceptAsync(
                spLayak.Id,
                new AcceptLabSpecimenRequest());

            Assert.NotNull(layak.Handoff);

            Assert.Equal(
                ClinicalFactEmissionKind.Emitted,
                layak.Handoff!.Perwakilan.Kind);

            // Pembatalan order juga harus memiliki actor klinis yang valid.
            var orderService = CreateOrderService(
                context,
                verifierService,
                verifierUserId);

            var hasil = await orderService.CancelAsync(
                order.Id,
                new CancelLabSpecimenRequest
                {
                    Reason = "Pemeriksaan dibatalkan dokter."
                });

            // Hanya sampel yang sudah layak yang menghasilkan fakta pembatalan.
            Assert.Single(hasil.BillingHandoffs);

            Assert.Equal(
                2,
                hasil.BillingHandoffs[0].MilestoneFactVersion);

            var statusSampel = await context.LabSpecimens
                .AsNoTracking()
                .Where(x => x.LabOrderId == order.Id)
                .Select(x => x.SpecimenStatus)
                .ToListAsync();

            Assert.All(
                statusSampel,
                status => Assert.Equal(
                    LabSpecimenStatus.Cancelled,
                    status));

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
            var procedure = await SeedLabProcedureAsync(
                "Darah lengkap",
                TarifDarahLengkap);

            await using var setupContext = _fixture.CreateContext();

            var collectorUserId = seed.ActorUserId;
            var petugasAId = Guid.NewGuid();
            var petugasBId = Guid.NewGuid();

            var collectorService =
                CreateSpecimenService(
                    setupContext,
                    collectorUserId);

            var order = await SeedOrderAsync(
                setupContext,
                seed,
                procedure.Id);

            var specimen = await SampaiDiterimaAsync(
                collectorService,
                order.Id);

            // Dua context terpisah mewakili dua petugas yang membuka
            // specimen pada waktu yang sama.
            await using var contextA = _fixture.CreateContext();
            await using var contextB = _fixture.CreateContext();

            // PENTING:
            // kedua context harus membaca versi yang sama SEBELUM
            // salah satu petugas menyimpan perubahan.
            var snapshotA = await contextA.LabSpecimens
                .Include(x => x.LabOrder)
                .SingleAsync(x => x.Id == specimen.Id);

            var snapshotB = await contextB.LabSpecimens
                .Include(x => x.LabOrder)
                .SingleAsync(x => x.Id == specimen.Id);

            Assert.Equal(
                LabSpecimenStatus.Received,
                snapshotA.SpecimenStatus);

            Assert.Equal(
                LabSpecimenStatus.Received,
                snapshotB.SpecimenStatus);

            var layananA =
                CreateSpecimenService(
                    contextA,
                    petugasAId);

            var layananB =
                CreateSpecimenService(
                    contextB,
                    petugasBId);

            // Petugas A menyimpan lebih dulu.
            var hasilA = await layananA.AcceptAsync(
                specimen.Id,
                new AcceptLabSpecimenRequest());

            Assert.Equal(
                LabSpecimenStatus.Accepted,
                hasilA.Specimen.SpecimenStatus);

            // Context B masih memegang Version lama.
            // Ketika mencoba menyimpan perubahan, concurrency token
            // harus menolak update tersebut.
            var galat = await Assert.ThrowsAsync<LabConcurrencyException>(() =>
                layananB.RejectAsync(
                    specimen.Id,
                    new RejectLabSpecimenRequest
                    {
                        ReasonCode = "LABELING_ISSUE"
                    }));

            Assert.Contains(
                "diubah oleh petugas lain",
                galat.Message);
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

            var galat = await Assert.ThrowsAsync<LabSpecimenConflictException>(() =>
                specimenService.PlanAsync(
                    order.Id,
                    new PlanLabSpecimenRequest()));

            Assert.Contains(
                "sudah dibatalkan",
                galat.Message);
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
            var galat = await Assert.ThrowsAsync<LabSpecimenConflictException>(() =>
                specimenService.AcceptAsync(
                    planned.Specimen.Id,
                    new AcceptLabSpecimenRequest()));

            Assert.Contains(
                "belum tercatat tiba",
                galat.Message);
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

        private async Task<EncounterSeed> NewEncounterAsync()
        {
            var seed = await _fixture.SeedEncounterAsync();
            _seeds.Add(seed);
            return seed;
        }

        private LabSpecimenService CreateSpecimenService(ApplicationDbContext context) =>
            new(
                context,
                new ClinicalMilestoneFactProducer(
                    context,
                    new BillingFolioService(context),
                    BillingTestDatabaseFixture.CreateLoggerService()),
                new HttpContextAccessor(),
                BillingTestDatabaseFixture.CreateLoggerService());

        private LabSpecimenService CreateSpecimenService(
            ApplicationDbContext context,
            Guid actorUserId)
        {
            if (actorUserId == Guid.Empty)
                throw new ArgumentException(
                    "Actor integration test tidak boleh Guid.Empty.",
                    nameof(actorUserId));

            var identity = new ClaimsIdentity(
                new[]
                {
                    new Claim(
                        ClaimTypes.NameIdentifier,
                        actorUserId.ToString())
                },
                authenticationType: "IntegrationTest");

            var accessor = new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(identity)
                }
            };

            return new LabSpecimenService(
                context,
                new ClinicalMilestoneFactProducer(
                    context,
                    new BillingFolioService(context),
                    BillingTestDatabaseFixture.CreateLoggerService()),
                accessor,
                BillingTestDatabaseFixture.CreateLoggerService());
        }

        private static LabOrderService CreateOrderService(
            ApplicationDbContext context,
            LabSpecimenService specimenService) =>
            new(
                context,
                specimenService,
                new HttpContextAccessor(),
                BillingTestDatabaseFixture.CreateLoggerService());

        private static LabOrderService CreateOrderService(
            ApplicationDbContext context,
            LabSpecimenService specimenService,
            Guid actorUserId)
        {
            if (actorUserId == Guid.Empty)
                throw new ArgumentException(
                    "Actor integration test tidak boleh Guid.Empty.",
                    nameof(actorUserId));

            var identity = new ClaimsIdentity(
                new[]
                {
                    new Claim(
                        ClaimTypes.NameIdentifier,
                        actorUserId.ToString())
                },
                authenticationType: "IntegrationTest");

            var accessor = new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(identity)
                }
            };

            return new LabOrderService(
                context,
                specimenService,
                accessor,
                BillingTestDatabaseFixture.CreateLoggerService());
        }

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
