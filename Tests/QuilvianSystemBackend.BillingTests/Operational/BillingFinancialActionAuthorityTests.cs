using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Constants;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Enums;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Services;
using Xunit;

namespace QuilvianSystemBackend.BillingTests.Operational
{
    /// <summary>
    /// Invariant <c>RJ-BIL-BE-006</c> yang dapat diuji tanpa database sama sekali.
    ///
    /// Isinya dua hal yang menentukan siapa boleh melakukan apa: penilaian risiko, dan pemisahan
    /// kewenangan per jenis tindakan. Keduanya murni dan tidak menyentuh baris apa pun, sehingga
    /// tidak ada alasan menjadikannya bergantung pada database — dan tidak ada alasan pula
    /// membiarkannya tidak teruji.
    /// </summary>
    public sealed class BillingFinancialActionAuthorityTests
    {
        // =================================================================
        // Penilaian risiko
        // =================================================================

        /// <summary>
        /// Koreksi lintas encounter selalu high-risk, berapa pun nominalnya — termasuk nol.
        /// Yang berbahaya bukan angkanya, melainkan berpindahnya tagihan ke kunjungan lain.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(999_999_999)]
        public void KoreksiLintasEncounter_SelaluHighRisk(decimal nominal)
        {
            var encounterAsal = Guid.NewGuid();

            var permintaan = new BilFinancialActionRequest
            {
                ActionType = BillingFinancialActionType.Adjustment,
                EncounterId = encounterAsal,
                TargetEncounterId = Guid.NewGuid(),
                RequestedAmount = nominal
            };

            var folio = new BilFolio { EncounterId = encounterAsal };

            Assert.Equal(
                BillingFinancialRiskLevel.HighRisk,
                BillingFinancialActionService.DetermineRiskLevel(permintaan, folio, null));
        }

        /// <summary>
        /// Menunjuk encounter tujuan yang sama dengan encounter asalnya bukan koreksi lintas
        /// encounter. Tanpa pembedaan ini, seluruh permintaan biasa akan ikut terangkat menjadi
        /// high-risk hanya karena field-nya kebetulan terisi.
        /// </summary>
        [Fact]
        public void EncounterTujuanSamaDenganAsal_BukanLintasEncounter()
        {
            var encounterId = Guid.NewGuid();

            var permintaan = new BilFinancialActionRequest
            {
                ActionType = BillingFinancialActionType.Adjustment,
                EncounterId = encounterId,
                TargetEncounterId = encounterId,
                RequestedAmount = 100m
            };

            var folio = new BilFolio { EncounterId = encounterId };

            Assert.Equal(
                BillingFinancialRiskLevel.Normal,
                BillingFinancialActionService.DetermineRiskLevel(permintaan, folio, null));
        }

        /// <summary>
        /// Membuka kembali folio yang sudah tertutup selalu high-risk. Folio tertutup adalah
        /// pernyataan bahwa tidak ada lagi uang yang belum jelas; membatalkan pernyataan itu
        /// tidak boleh menjadi pekerjaan satu orang.
        /// </summary>
        [Fact]
        public void MembukaKembaliFolioTertutup_SelaluHighRisk()
        {
            var permintaan = new BilFinancialActionRequest
            {
                ActionType = BillingFinancialActionType.FolioReopen,
                RequestedAmount = 0m
            };

            var folio = new BilFolio { Status = BillingFolioStatus.Closed };

            Assert.Equal(
                BillingFinancialRiskLevel.HighRisk,
                BillingFinancialActionService.DetermineRiskLevel(permintaan, folio, null));
        }

        /// <summary>
        /// Seluruh refund untuk sekarang high-risk.
        ///
        /// Ini keputusan fail-closed yang disengaja, bukan kelalaian. <c>RJ-BIL-GATE-DEC-006</c>
        /// menyebut <i>refund atas pembayaran yang sudah settled</i>, sedangkan keadaan settled
        /// belum ada di model mana pun — ia lahir bersama <c>RJ-BIL-BE-008</c>. Selama keadaan
        /// itu tidak dapat dipastikan, seluruh refund diperlakukan high-risk. Salah menganggap
        /// high-risk hanya menambah satu persetujuan; salah menganggap aman berarti uang keluar
        /// tanpa pengawasan.
        /// </summary>
        [Fact]
        public void SeluruhRefund_HighRiskSelamaKeadaanSettledBelumAda()
        {
            var permintaan = new BilFinancialActionRequest
            {
                ActionType = BillingFinancialActionType.Refund,
                RequestedAmount = 1m
            };

            Assert.Equal(
                BillingFinancialRiskLevel.HighRisk,
                BillingFinancialActionService.DetermineRiskLevel(permintaan, new BilFolio(), null));
        }

        /// <summary>
        /// Void dan reversal terhadap baris yang sudah <c>Recognized</c> high-risk, dengan alasan
        /// fail-closed yang sama seperti refund.
        /// </summary>
        [Theory]
        [InlineData(BillingFinancialActionType.Void)]
        [InlineData(BillingFinancialActionType.Reversal)]
        public void VoidDanReversalAtasBarisRecognized_HighRisk(BillingFinancialActionType jenis)
        {
            var permintaan = new BilFinancialActionRequest
            {
                ActionType = jenis,
                RequestedAmount = 1m
            };

            var baris = new BilChargeLine
            {
                CalculationStatus = BillingChargeCalculationStatus.Recognized
            };

            Assert.Equal(
                BillingFinancialRiskLevel.HighRisk,
                BillingFinancialActionService.DetermineRiskLevel(permintaan, new BilFolio(), baris));
        }

        /// <summary>
        /// Void terhadap baris yang baru diterima belum high-risk. Kalau semuanya dianggap
        /// high-risk, pembedaan itu berhenti bermakna dan persetujuan berubah menjadi formalitas
        /// yang dilewati orang tanpa membaca.
        /// </summary>
        [Fact]
        public void VoidAtasBarisBaruDiterima_Normal()
        {
            var permintaan = new BilFinancialActionRequest
            {
                ActionType = BillingFinancialActionType.Void,
                RequestedAmount = 1m
            };

            var baris = new BilChargeLine
            {
                CalculationStatus = BillingChargeCalculationStatus.Received
            };

            Assert.Equal(
                BillingFinancialRiskLevel.Normal,
                BillingFinancialActionService.DetermineRiskLevel(permintaan, new BilFolio(), baris));
        }

        // =================================================================
        // Sidik isi permintaan
        // =================================================================

        /// <summary>
        /// Isi yang sama menghasilkan sidik yang sama. Tanpa sifat ini, checker akan ditolak
        /// terus-menerus walaupun tidak ada yang berubah.
        /// </summary>
        [Fact]
        public void IsiSama_SidikSama()
        {
            var a = BuatPermintaanContoh();
            var b = BuatPermintaanContoh();

            Assert.Equal(
                BillingFinancialActionService.ComputeContentHash(a),
                BillingFinancialActionService.ComputeContentHash(b));
        }

        /// <summary>
        /// Setiap perubahan yang mengubah akibat finansial harus mengubah sidiknya. Inilah yang
        /// membuat <i>"checker material edit tidak dapat disetujui sebagai request lama"</i>
        /// dapat dibuktikan.
        /// </summary>
        [Fact]
        public void NominalBerubah_SidikBerubah()
        {
            var asli = BuatPermintaanContoh();
            var diubah = BuatPermintaanContoh();
            diubah.RequestedAmount += 1m;

            Assert.NotEqual(
                BillingFinancialActionService.ComputeContentHash(asli),
                BillingFinancialActionService.ComputeContentHash(diubah));
        }

        [Fact]
        public void AlasanBerubah_SidikBerubah()
        {
            var asli = BuatPermintaanContoh();
            var diubah = BuatPermintaanContoh();
            diubah.ReasonCode = "ALASAN_LAIN";

            Assert.NotEqual(
                BillingFinancialActionService.ComputeContentHash(asli),
                BillingFinancialActionService.ComputeContentHash(diubah));
        }

        [Fact]
        public void SasaranBarisTagihanBerubah_SidikBerubah()
        {
            var asli = BuatPermintaanContoh();
            var diubah = BuatPermintaanContoh();
            diubah.ChargeLineId = Guid.NewGuid();

            Assert.NotEqual(
                BillingFinancialActionService.ComputeContentHash(asli),
                BillingFinancialActionService.ComputeContentHash(diubah));
        }

        /// <summary>
        /// Nomor revisi ikut dihitung, supaya revisi dengan isi yang kebetulan identik tetap
        /// memiliki sidik berbeda dan tidak dapat disetujui memakai persetujuan revisi sebelumnya.
        /// </summary>
        [Fact]
        public void NomorRevisiBerubah_SidikBerubah()
        {
            var asli = BuatPermintaanContoh();
            var diubah = BuatPermintaanContoh();
            diubah.RevisionNumber = 2;

            Assert.NotEqual(
                BillingFinancialActionService.ComputeContentHash(asli),
                BillingFinancialActionService.ComputeContentHash(diubah));
        }

        // =================================================================
        // Pemisahan kewenangan
        // =================================================================

        /// <summary>
        /// Mengajukan dan menyetujui adalah dua kemampuan berbeda untuk setiap jenis tindakan.
        /// Menyatukannya berarti seseorang yang boleh mengajukan otomatis boleh memutuskan.
        /// </summary>
        [Theory]
        [InlineData(BillingFinancialActionType.Void)]
        [InlineData(BillingFinancialActionType.Adjustment)]
        [InlineData(BillingFinancialActionType.Reversal)]
        [InlineData(BillingFinancialActionType.Refund)]
        [InlineData(BillingFinancialActionType.Waiver)]
        [InlineData(BillingFinancialActionType.WriteOff)]
        [InlineData(BillingFinancialActionType.ManualOverride)]
        [InlineData(BillingFinancialActionType.FolioReopen)]
        public void MengajukanDanMenyetujui_KemampuanBerbeda(BillingFinancialActionType jenis)
        {
            Assert.NotEqual(
                BillingFinancialCapabilities.CreateCapability(jenis),
                BillingFinancialCapabilities.ApproveCapability(jenis));
        }

        /// <summary>
        /// Tidak ada dua jenis tindakan yang berbagi nama kemampuan. Nama yang bertabrakan berarti
        /// memberi hak atas satu jenis diam-diam memberi hak atas jenis lain.
        /// </summary>
        [Fact]
        public void SetiapJenisPunyaNamaKemampuanSendiri()
        {
            var jenis = Enum.GetValues<BillingFinancialActionType>();

            var seluruhNama = jenis
                .Select(BillingFinancialCapabilities.CreateCapability)
                .Concat(jenis.Select(BillingFinancialCapabilities.ApproveCapability))
                .ToList();

            Assert.Equal(seluruhNama.Count, seluruhNama.Distinct().Count());
        }

        /// <summary>
        /// Menyetujui refund dan benar-benar mengeluarkan uangnya adalah dua kemampuan berbeda.
        /// <c>RJ-BIL-GATE-DEC-006</c> menuliskannya secara khusus hanya untuk refund, dan memang
        /// hanya refund yang mengeluarkan uang dari rumah sakit.
        /// </summary>
        [Fact]
        public void MenyetujuiRefundDanMenjalankannya_KemampuanBerbeda()
        {
            Assert.NotEqual(
                BillingFinancialCapabilities.ApproveCapability(BillingFinancialActionType.Refund),
                BillingFinancialCapabilities.ExecuteCapability(BillingFinancialActionType.Refund));

            Assert.Equal(
                "RefundExecute",
                BillingFinancialCapabilities.ExecuteCapability(BillingFinancialActionType.Refund));
        }

        /// <summary>
        /// Menutup folio dan membukanya kembali bukan kemampuan yang sama.
        /// </summary>
        [Fact]
        public void MenutupDanMembukaFolio_KemampuanBerbeda()
        {
            Assert.NotEqual(
                BillingFinancialCapabilities.FolioClose,
                BillingFinancialCapabilities.FolioReopen);
        }

        private static BilFinancialActionRequest BuatPermintaanContoh()
        {
            var folioId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var encounterId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var chargeLineId = Guid.Parse("33333333-3333-3333-3333-333333333333");

            return new BilFinancialActionRequest
            {
                ActionType = BillingFinancialActionType.Adjustment,
                FolioId = folioId,
                EncounterId = encounterId,
                ChargeLineId = chargeLineId,
                RequestedAmount = 150_000m,
                Currency = "IDR",
                ReasonCode = "KOREKSI_KUANTITAS",
                ReasonNote = "Kuantitas tercatat dua kali.",
                RevisionNumber = 1
            };
        }
    }
}
