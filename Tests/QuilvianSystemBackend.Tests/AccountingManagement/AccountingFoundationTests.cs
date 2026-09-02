using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.JournalType.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.Infrastructure;

namespace QuilvianSystemBackend.Tests.AccountingManagement
{
    /// <summary>
    /// Bukti acceptance untuk task `BE-ACC-001` — kerangka modul, enum, dan test harness.
    ///
    /// Task ini sengaja belum membuat satu pun tabel bisnis. Karena itu yang dibuktikan di sini
    /// ada dua macam: nilai enam enum sesuai contract `ACC-STATE-0.1`, dan batas task itu sendiri
    /// yaitu belum adanya entity persisted Accounting.
    ///
    /// Uji batas task terlihat tidak lazim, tetapi ada gunanya. `QBE-MOD-002` memblokir pembuatan
    /// entity operasional pertama sampai prefix modul terdaftar di registry kepemilikan. Test ini
    /// menjadi pagar otomatis: bila ada yang menambahkan entity Accounting sebelum pendaftaran
    /// itu turun, test gagal dan sebabnya terbaca langsung.
    /// </summary>
    public class AccountingFoundationTests
    {
        private const string NamespaceAccounting =
            "QuilvianSystemBackend.Areas.Corporate.AccountingManagement";

        /// <summary>
        /// Acceptance criteria 2 — `AccountType` bernilai persis seperti contract.
        /// </summary>
        [Fact]
        public void AccountType_BernilaiSesuaiContractState()
        {
            Assert.Equal(1, (int)AccountType.Asset);
            Assert.Equal(2, (int)AccountType.Liability);
            Assert.Equal(3, (int)AccountType.Equity);
            Assert.Equal(4, (int)AccountType.Revenue);
            Assert.Equal(5, (int)AccountType.Expense);
            Assert.Equal(5, Enum.GetValues<AccountType>().Length);
        }

        /// <summary>
        /// Acceptance criteria 2 — `NormalBalance` bernilai persis seperti contract.
        /// </summary>
        [Fact]
        public void NormalBalance_BernilaiSesuaiContractState()
        {
            Assert.Equal(1, (int)NormalBalance.Debit);
            Assert.Equal(2, (int)NormalBalance.Credit);
            Assert.Equal(2, Enum.GetValues<NormalBalance>().Length);
        }

        /// <summary>
        /// Acceptance criteria 2 — `JournalStatus` bernilai persis seperti contract.
        /// </summary>
        [Fact]
        public void JournalStatus_BernilaiSesuaiContractState()
        {
            Assert.Equal(1, (int)JournalStatus.Draft);
            Assert.Equal(2, (int)JournalStatus.PendingApproval);
            Assert.Equal(3, (int)JournalStatus.Approved);
            Assert.Equal(4, (int)JournalStatus.Posted);
            Assert.Equal(5, (int)JournalStatus.Rejected);
            Assert.Equal(5, Enum.GetValues<JournalStatus>().Length);
        }

        /// <summary>
        /// Acceptance criteria 2 — `JournalApprovalAction` bernilai persis seperti contract.
        /// </summary>
        [Fact]
        public void JournalApprovalAction_BernilaiSesuaiContractState()
        {
            Assert.Equal(1, (int)JournalApprovalAction.Submitted);
            Assert.Equal(2, (int)JournalApprovalAction.Approved);
            Assert.Equal(3, (int)JournalApprovalAction.Rejected);
            Assert.Equal(4, (int)JournalApprovalAction.Posted);
            Assert.Equal(5, (int)JournalApprovalAction.Reversed);
            Assert.Equal(5, Enum.GetValues<JournalApprovalAction>().Length);
        }

        /// <summary>
        /// Acceptance criteria 2 — `JournalCorrectionType` bernilai persis seperti contract.
        /// </summary>
        [Fact]
        public void JournalCorrectionType_BernilaiSesuaiContractState()
        {
            Assert.Equal(1, (int)JournalCorrectionType.FullReversal);
            Assert.Equal(2, (int)JournalCorrectionType.Adjustment);
            Assert.Equal(2, Enum.GetValues<JournalCorrectionType>().Length);
        }

        /// <summary>
        /// Acceptance criteria 2 — `AccountingPeriodStatus` bernilai persis seperti contract.
        ///
        /// Urutannya bermakna, bukan sekadar penomoran: `SoftClosed` berada di antara `Open` dan
        /// `Closed` karena ia memang masa tenggang tutup buku.
        /// </summary>
        [Fact]
        public void AccountingPeriodStatus_BernilaiSesuaiContractState()
        {
            Assert.Equal(1, (int)AccountingPeriodStatus.Open);
            Assert.Equal(2, (int)AccountingPeriodStatus.SoftClosed);
            Assert.Equal(3, (int)AccountingPeriodStatus.Closed);
            Assert.Equal(3, Enum.GetValues<AccountingPeriodStatus>().Length);
        }

        /// <summary>
        /// Konvensi repository: setiap anggota enum punya `[Display(Name = ...)]`, supaya layar
        /// tidak perlu menerjemahkan sendiri nama statusnya.
        /// </summary>
        [Fact]
        public void SetiapAnggotaEnum_PunyaDisplayName()
        {
            Type[] enumAccounting =
            [
                typeof(AccountType),
                typeof(NormalBalance),
                typeof(JournalStatus),
                typeof(JournalApprovalAction),
                typeof(JournalCorrectionType),
                typeof(AccountingPeriodStatus)
            ];

            List<string> tanpaDisplay = [];

            foreach (Type tipe in enumAccounting)
            {
                foreach (string nama in Enum.GetNames(tipe))
                {
                    DisplayAttribute? display = tipe
                        .GetField(nama)!
                        .GetCustomAttribute<DisplayAttribute>();

                    if (string.IsNullOrWhiteSpace(display?.Name))
                    {
                        tanpaDisplay.Add($"{tipe.Name}.{nama}");
                    }
                }
            }

            Assert.True(
                tanpaDisplay.Count == 0,
                $"Anggota enum berikut belum punya Display Name: {string.Join(", ", tanpaDisplay)}");
        }

        /// <summary>
        /// Penjaga batas task, diperbarui pada `BE-ACC-005`.
        ///
        /// Riwayatnya: `BE-ACC-001` menuntut **nol** entity persisted; `BE-ACC-003` menaikkannya
        /// menjadi **dua**; `BE-ACC-004` menjadi **tiga**; `BE-ACC-005` menjadi **tujuh**.
        ///
        /// Tujuh ini menutup seluruh entity `MVP-0`. Sesudahnya yang tersisa hanya `BE-ACC-006`
        /// yang membuat migration, bukan entity. Jadi bila daftar ini bertambah lagi, hampir
        /// pasti ada entity Phase 2 yang masuk terlalu dini — misalnya kolom atau tabel
        /// integrasi Finance/Billing yang memang sengaja ditunda.
        ///
        /// Dibuktikan lewat refleksi, bukan lewat daftar berkas, supaya tetap berlaku walau
        /// berkasnya dipindah folder. Entity persisted di repository ini dikenali dari
        /// pewarisan <see cref="IdentityModel"/>.
        /// </summary>
        [Fact]
        public void ModulAccounting_HanyaMemilikiEntityCakupanBeAcc005()
        {
            const string akar = "QuilvianSystemBackend.Areas.Corporate.AccountingManagement";

            string[] entityDiizinkan =
            {
                akar + ".AccountingPeriod.Models.AccAccountingPeriod",
                akar + ".JournalManagement.Models.AccJournal",
                akar + ".JournalManagement.Models.AccJournalApproval",
                akar + ".JournalManagement.Models.AccJournalLine",
                akar + ".JournalManagement.Models.AccNumberSeries",
                akar + ".MasterData.ChartOfAccount.Models.AccChartOfAccount",
                akar + ".MasterData.JournalType.Models.AccJournalType"
            };

            List<string> entityDitemukan = typeof(AccountType).Assembly
                .GetTypes()
                .Where(t => t.Namespace is not null
                            && t.Namespace.StartsWith(NamespaceAccounting, StringComparison.Ordinal))
                .Where(t => typeof(IdentityModel).IsAssignableFrom(t) && t != typeof(IdentityModel))
                .Select(t => t.FullName!)
                .OrderBy(nama => nama, StringComparer.Ordinal)
                .ToList();

            List<string> diLuarCakupan = entityDitemukan
                .Except(entityDiizinkan, StringComparer.Ordinal)
                .ToList();

            Assert.True(
                diLuarCakupan.Count == 0,
                "Hanya tujuh entity cakupan MVP-0 yang boleh ada. Entity di luar daftar itu "
                + "berarti cakupan Phase 2 masuk terlalu dini. "
                + $"Ditemukan di luar cakupan: {string.Join(", ", diLuarCakupan)}");

            foreach (string wajibAda in entityDiizinkan)
            {
                Assert.Contains(wajibAda, entityDitemukan);
            }
        }

        /// <summary>
        /// Acceptance criteria 1 — kolom, tipe, dan panjang `AccChartOfAccount` persis seperti
        /// kamus data bagian 1.
        ///
        /// Panjang teks diperiksa lewat <see cref="MaxLengthAttribute"/> supaya kegagalannya
        /// menunjuk kolom yang salah, bukan sekadar "build gagal".
        /// </summary>
        [Fact]
        public void AccChartOfAccount_SesuaiKamusData()
        {
            Assert.Equal(20, PanjangMaksimum<AccChartOfAccount>(nameof(AccChartOfAccount.AccountCode)));
            Assert.Equal(200, PanjangMaksimum<AccChartOfAccount>(nameof(AccChartOfAccount.AccountName)));
            Assert.Equal(500, PanjangMaksimum<AccChartOfAccount>(nameof(AccChartOfAccount.Description)));

            Assert.True(WajibDiisi<AccChartOfAccount>(nameof(AccChartOfAccount.LegalEntityId)));
            Assert.True(WajibDiisi<AccChartOfAccount>(nameof(AccChartOfAccount.AccountCode)));
            Assert.True(WajibDiisi<AccChartOfAccount>(nameof(AccChartOfAccount.AccountName)));

            // Akun tingkat pertama tidak punya induk, jadi kolomnya wajib boleh kosong.
            Assert.Equal(
                typeof(Guid?),
                typeof(AccChartOfAccount).GetProperty(nameof(AccChartOfAccount.ParentAccountId))!
                    .PropertyType);

            // NormalBalance berdiri sendiri, tidak diturunkan dari AccountType, supaya akun
            // kontra dapat ditangani.
            Assert.Equal(
                typeof(NormalBalance),
                typeof(AccChartOfAccount).GetProperty(nameof(AccChartOfAccount.NormalBalance))!
                    .PropertyType);

            AccChartOfAccount bawaan = new();
            Assert.Equal(1, bawaan.AccountLevel);
            Assert.False(bawaan.IsPostable);
            Assert.True(bawaan.IsActive);
        }

        /// <summary>
        /// Acceptance criteria 1 — `AccJournalType` persis seperti kamus data bagian 2, termasuk
        /// ketiadaan `LegalEntityId` yang memang disengaja.
        /// </summary>
        [Fact]
        public void AccJournalType_SesuaiKamusData()
        {
            Assert.Equal(10, PanjangMaksimum<AccJournalType>(nameof(AccJournalType.JournalTypeCode)));
            Assert.Equal(100, PanjangMaksimum<AccJournalType>(nameof(AccJournalType.JournalTypeName)));
            Assert.Equal(10, PanjangMaksimum<AccJournalType>(nameof(AccJournalType.NumberPrefix)));

            Assert.True(WajibDiisi<AccJournalType>(nameof(AccJournalType.JournalTypeCode)));
            Assert.True(WajibDiisi<AccJournalType>(nameof(AccJournalType.JournalTypeName)));
            Assert.True(WajibDiisi<AccJournalType>(nameof(AccJournalType.NumberPrefix)));

            // Jenis jurnal berlaku sama untuk semua badan hukum.
            Assert.Null(typeof(AccJournalType).GetProperty("LegalEntityId"));

            AccJournalType bawaan = new();
            Assert.True(bawaan.RequiresApproval);
            Assert.False(bawaan.IsSystemType);
            Assert.True(bawaan.IsActive);
        }

        /// <summary>
        /// Acceptance criteria 1 dan 2 — bentuk tabel dan relasi dibaca dari model EF Core yang
        /// benar-benar terbentuk, bukan dari berkas configuration-nya.
        ///
        /// Ini pemeriksaan yang sesungguhnya: nama tabel, unique index, dan `DeleteBehavior`
        /// hanya dapat dipastikan setelah EF menggabungkan entity dengan configuration-nya.
        /// Memakai penyedia in-memory, jadi **tidak** menyentuh database mana pun.
        /// </summary>
        [Fact]
        public void ModelEfCore_MembentukTabelDanRelasiSesuaiKontrak()
        {
            using TestDatabase basisUji = TestDatabase.Create();
            using ApplicationDbContext db = basisUji.CreateContext();
            IModel model = db.Model;

            IEntityType coa = model.FindEntityType(typeof(AccChartOfAccount))!;
            IEntityType jenisJurnal = model.FindEntityType(typeof(AccJournalType))!;

            Assert.Equal("AccChartOfAccount", coa.GetTableName());
            Assert.Equal("AccJournalType", jenisJurnal.GetTableName());

            // Kode akun unik per badan hukum (ACC-DEC-037), bukan unik global.
            Assert.Contains(
                coa.GetIndexes(),
                i => i.IsUnique
                     && i.Properties.Select(p => p.Name).SequenceEqual(
                         new[] { "LegalEntityId", "AccountCode" }));

            // Kode jenis jurnal unik global.
            Assert.Contains(
                jenisJurnal.GetIndexes(),
                i => i.IsUnique
                     && i.Properties.Select(p => p.Name).SequenceEqual(new[] { "JournalTypeCode" }));

            // Acceptance criteria 2 — seluruh relasi memakai Restrict.
            foreach (IForeignKey fk in coa.GetForeignKeys())
            {
                Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior);
            }

            // Relasi induk-anak menunjuk tabel yang sama.
            Assert.Contains(
                coa.GetForeignKeys(),
                fk => fk.PrincipalEntityType.ClrType == typeof(AccChartOfAccount)
                      && fk.Properties.Select(p => p.Name).SequenceEqual(new[] { "ParentAccountId" }));

            // Enum disimpan sebagai int, bukan sebagai teks.
            Assert.Equal(
                typeof(int),
                coa.FindProperty(nameof(AccChartOfAccount.AccountType))!.GetProviderClrType());
            Assert.Equal(
                typeof(int),
                coa.FindProperty(nameof(AccChartOfAccount.NormalBalance))!.GetProviderClrType());

            // Roadmap BE-ACC-003 melarang kolom RequiresCostCenter; kewajiban Cost Center
            // diturunkan dari AccountType == Expense, bukan disimpan.
            Assert.Null(coa.FindProperty("RequiresCostCenter"));
        }

        /// <summary>
        /// Acceptance criteria 2 `BE-ACC-004` — `PeriodCode` berpanjang 7, ditambah pemeriksaan
        /// kolom lain terhadap kamus data bagian 3.
        ///
        /// Panjang 7 bukan angka sembarangan: ia persis muat untuk bentuk <c>2026-09</c>, yaitu
        /// empat digit tahun, satu tanda hubung, dan dua digit bulan. Kelebihan satu karakter pun
        /// akan membuka peluang bentuk lain yang tidak dikehendaki `ACC-DEC-013`.
        /// </summary>
        [Fact]
        public void AccAccountingPeriod_SesuaiKamusData()
        {
            Assert.Equal(7, PanjangMaksimum<AccAccountingPeriod>(
                nameof(AccAccountingPeriod.PeriodCode)));
            Assert.Equal(500, PanjangMaksimum<AccAccountingPeriod>(
                nameof(AccAccountingPeriod.LastReasonNote)));

            Assert.True(WajibDiisi<AccAccountingPeriod>(
                nameof(AccAccountingPeriod.LegalEntityId)));
            Assert.True(WajibDiisi<AccAccountingPeriod>(
                nameof(AccAccountingPeriod.PeriodCode)));

            // Panjang 7 tepat memuat bentuk yang diwajibkan ACC-DEC-013.
            Assert.Equal(7, "2026-09".Length);

            // Empat kolom jejak penutupan boleh kosong: periode baru belum pernah ditutup
            // maupun dibuka kembali.
            Assert.Equal(typeof(Guid?), TipeProperti<AccAccountingPeriod>(
                nameof(AccAccountingPeriod.ClosedBy)));
            Assert.Equal(typeof(DateTime?), TipeProperti<AccAccountingPeriod>(
                nameof(AccAccountingPeriod.ClosedAt)));
            Assert.Equal(typeof(Guid?), TipeProperti<AccAccountingPeriod>(
                nameof(AccAccountingPeriod.ReopenedBy)));
            Assert.Equal(typeof(DateTime?), TipeProperti<AccAccountingPeriod>(
                nameof(AccAccountingPeriod.ReopenedAt)));

            // Periode baru selalu lahir dalam keadaan terbuka.
            AccAccountingPeriod bawaan = new();
            Assert.Equal(AccountingPeriodStatus.Open, bawaan.PeriodStatus);
        }

        /// <summary>
        /// Acceptance criteria 1 `BE-ACC-004` — ketiga nilai status tersimpan sebagai integer,
        /// bukan sebagai teks.
        ///
        /// Diperiksa dari model EF Core yang benar-benar terbentuk, sehingga membuktikan
        /// <c>HasConversion&lt;int&gt;()</c> pada configuration memang berlaku.
        /// </summary>
        [Fact]
        public void AccAccountingPeriod_MenyimpanTigaStatusSebagaiInteger()
        {
            using TestDatabase basisUji = TestDatabase.Create();
            using ApplicationDbContext db = basisUji.CreateContext();

            IEntityType periode = db.Model.FindEntityType(typeof(AccAccountingPeriod))!;

            Assert.Equal("AccAccountingPeriod", periode.GetTableName());

            IProperty status = periode.FindProperty(nameof(AccAccountingPeriod.PeriodStatus))!;
            Assert.Equal(typeof(int), status.GetProviderClrType());

            // Tepat tiga status, dengan nilai persis seperti ACC-DEC-012.
            Assert.Equal(3, Enum.GetValues<AccountingPeriodStatus>().Length);
            Assert.Equal(1, (int)AccountingPeriodStatus.Open);
            Assert.Equal(2, (int)AccountingPeriodStatus.SoftClosed);
            Assert.Equal(3, (int)AccountingPeriodStatus.Closed);

            // Satu badan hukum hanya boleh punya satu periode per kode.
            Assert.Contains(
                periode.GetIndexes(),
                i => i.IsUnique
                     && i.Properties.Select(p => p.Name).SequenceEqual(
                         new[] { "LegalEntityId", "PeriodCode" }));

            // Seluruh relasi memakai Restrict — periode adalah kerangka pembukaan yang tidak
            // boleh ikut terhapus bersama induknya.
            foreach (IForeignKey fk in periode.GetForeignKeys())
            {
                Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior);
            }

            // BE-ACC-005 belum dikerjakan, jadi belum boleh ada relasi ke jurnal.
            Assert.Null(periode.FindNavigation("Journals"));
        }

        /// <summary>
        /// Acceptance criteria 1 `BE-ACC-005` — seluruh kolom nilai memakai `decimal(18,2)`.
        ///
        /// Ini pemeriksaan yang paling berbahaya bila dilewatkan. Salah presisi pada kolom uang
        /// tidak membuat build gagal maupun test lain merah; ia baru terlihat sebagai selisih
        /// beberapa rupiah pada neraca, berbulan-bulan kemudian, dan sangat mahal ditelusuri.
        /// `NFR-008` menandainya sebagai risiko langsung.
        /// </summary>
        [Fact]
        public void SeluruhKolomNilai_MemakaiDecimal18Koma2()
        {
            using TestDatabase basisUji = TestDatabase.Create();
            using ApplicationDbContext db = basisUji.CreateContext();

            (Type entity, string kolom)[] kolomNilai =
            {
                (typeof(AccJournal), nameof(AccJournal.TotalDebit)),
                (typeof(AccJournal), nameof(AccJournal.TotalCredit)),
                (typeof(AccJournalLine), nameof(AccJournalLine.DebitAmount)),
                (typeof(AccJournalLine), nameof(AccJournalLine.CreditAmount))
            };

            foreach ((Type entity, string kolom) in kolomNilai)
            {
                IProperty properti = db.Model.FindEntityType(entity)!.FindProperty(kolom)!;

                Assert.Equal(typeof(decimal), properti.ClrType);
                Assert.Equal(18, properti.GetPrecision());
                Assert.Equal(2, properti.GetScale());
            }
        }

        /// <summary>
        /// Acceptance criteria 2 dan 3 `BE-ACC-005` — tiga unique index yang diminta, ditambah
        /// foreign key ke `MstCostCenter` yang wajib ada dan wajib boleh kosong.
        /// </summary>
        [Fact]
        public void JurnalDanAlokatorNomor_MemenuhiIndexDanRelasiKontrak()
        {
            using TestDatabase basisUji = TestDatabase.Create();
            using ApplicationDbContext db = basisUji.CreateContext();

            IEntityType jurnal = db.Model.FindEntityType(typeof(AccJournal))!;
            IEntityType baris = db.Model.FindEntityType(typeof(AccJournalLine))!;
            IEntityType riwayat = db.Model.FindEntityType(typeof(AccJournalApproval))!;
            IEntityType deret = db.Model.FindEntityType(typeof(AccNumberSeries))!;

            Assert.Equal("AccJournal", jurnal.GetTableName());
            Assert.Equal("AccJournalLine", baris.GetTableName());
            Assert.Equal("AccJournalApproval", riwayat.GetTableName());
            Assert.Equal("AccNumberSeries", deret.GetTableName());

            // Acceptance 2 — tiga unique index.
            Assert.Contains(jurnal.GetIndexes(), i => i.IsUnique
                && i.Properties.Select(p => p.Name)
                    .SequenceEqual(new[] { "LegalEntityId", "JournalNumber" }));
            Assert.Contains(baris.GetIndexes(), i => i.IsUnique
                && i.Properties.Select(p => p.Name)
                    .SequenceEqual(new[] { "JournalId", "LineNumber" }));
            Assert.Contains(deret.GetIndexes(), i => i.IsUnique
                && i.Properties.Select(p => p.Name)
                    .SequenceEqual(new[] { "SequenceKey", "ScopeKey" }));

            // Acceptance 3 — FK ke MstCostCenter ada, dan boleh kosong.
            IForeignKey fkCostCenter = Assert.Single(
                baris.GetForeignKeys(),
                fk => fk.PrincipalEntityType.ClrType == typeof(MstCostCenter));
            Assert.False(fkCostCenter.IsRequired);
            Assert.Equal(DeleteBehavior.Restrict, fkCostCenter.DeleteBehavior);
        }

        /// <summary>
        /// Perilaku hapus sengaja berbeda antar relasi, dan perbedaannya bermakna.
        ///
        /// Baris jurnal ikut terhapus bersama jurnalnya karena tidak punya makna sendiri.
        /// Riwayat persetujuan justru <b>tidak boleh</b> ikut terhapus, karena ia bukti audit.
        /// Kalau keduanya tertukar, jejak persetujuan bisa lenyap tanpa ada yang menyadari.
        /// </summary>
        [Fact]
        public void PerilakuHapus_CascadeHanyaPadaBarisJurnal()
        {
            using TestDatabase basisUji = TestDatabase.Create();
            using ApplicationDbContext db = basisUji.CreateContext();

            IEntityType baris = db.Model.FindEntityType(typeof(AccJournalLine))!;
            IEntityType riwayat = db.Model.FindEntityType(typeof(AccJournalApproval))!;
            IEntityType jurnal = db.Model.FindEntityType(typeof(AccJournal))!;

            IForeignKey barisKeJurnal = Assert.Single(
                baris.GetForeignKeys(),
                fk => fk.PrincipalEntityType.ClrType == typeof(AccJournal));
            Assert.Equal(DeleteBehavior.Cascade, barisKeJurnal.DeleteBehavior);

            IForeignKey riwayatKeJurnal = Assert.Single(
                riwayat.GetForeignKeys(),
                fk => fk.PrincipalEntityType.ClrType == typeof(AccJournal));
            Assert.Equal(DeleteBehavior.Restrict, riwayatKeJurnal.DeleteBehavior);

            // Seluruh relasi jurnal memakai Restrict, termasuk pembalikan ke dirinya sendiri.
            foreach (IForeignKey fk in jurnal.GetForeignKeys())
            {
                Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior);
            }

            Assert.Contains(
                jurnal.GetForeignKeys(),
                fk => fk.PrincipalEntityType.ClrType == typeof(AccJournal)
                      && fk.Properties.Select(p => p.Name)
                          .SequenceEqual(new[] { "ReversalOfJournalId" }));
        }

        /// <summary>
        /// Tiga kolom sengaja tidak dibuat, dan ketiadaannya adalah keputusan — bukan kelalaian.
        ///
        /// `SourceDomain` dan `SourceTransactionId` baru berguna saat ada jurnal otomatis, dan
        /// MVP memang tidak punya satu pun (`ACC-DEC-009`). `CurrencyCode` tidak diperlukan
        /// karena MVP hanya IDR (`ACC-DEC-020`).
        ///
        /// Test ini menjaga keputusan itu: menambahkan salah satunya "sekalian saja" akan
        /// membuatnya gagal, sehingga penambahannya harus lewat keputusan sadar.
        /// </summary>
        [Fact]
        public void AccJournal_TidakMemilikiKolomYangSengajaDitunda()
        {
            Assert.Null(typeof(AccJournal).GetProperty("SourceDomain"));
            Assert.Null(typeof(AccJournal).GetProperty("SourceTransactionId"));
            Assert.Null(typeof(AccJournal).GetProperty("CurrencyCode"));

            // Baris jurnal tidak membawa LegalEntityId sendiri (ACC-DEC-037) — badan hukumnya
            // diturunkan dari akun yang ditunjuk.
            Assert.Null(typeof(AccJournalLine).GetProperty("LegalEntityId"));
        }

        private static Type TipeProperti<T>(string namaProperti) =>
            typeof(T).GetProperty(namaProperti)!.PropertyType;

        private static int PanjangMaksimum<T>(string namaProperti) =>
            typeof(T).GetProperty(namaProperti)!
                .GetCustomAttribute<MaxLengthAttribute>()!.Length;

        private static bool WajibDiisi<T>(string namaProperti) =>
            typeof(T).GetProperty(namaProperti)!
                .GetCustomAttribute<RequiredAttribute>() is not null;

        /// <summary>
        /// Acceptance criteria 3 — folder controller memakai bentuk jamak `Controllers`.
        ///
        /// Bentuk tunggal `Controller` yang ada di modul IGD adalah utang teknis dan tidak boleh
        /// ditiru modul baru. Diperiksa pada filesystem karena di situlah pemeriksa kesesuaian
        /// QBE juga melihat.
        /// </summary>
        [Fact]
        public void FolderModulAccounting_MemakaiKonvensiRepository()
        {
            DirectoryInfo akar = CariAkarRepository();
            string modul = Path.Combine(
                akar.FullName, "Areas", "Corporate", "AccountingManagement");

            Assert.True(Directory.Exists(modul), $"Folder modul tidak ditemukan: {modul}");

            List<string> folderControllerTunggal = Directory
                .GetDirectories(modul, "Controller", SearchOption.AllDirectories)
                .ToList();

            Assert.True(
                folderControllerTunggal.Count == 0,
                "Folder controller wajib memakai bentuk jamak `Controllers`. "
                + $"Ditemukan bentuk tunggal: {string.Join(", ", folderControllerTunggal)}");

            Assert.NotEmpty(
                Directory.GetDirectories(modul, "Controllers", SearchOption.AllDirectories));
        }

        /// <summary>
        /// Menelusuri folder ke atas sampai menemukan solution, supaya test tidak bergantung pada
        /// kedalaman folder keluaran build.
        /// </summary>
        private static DirectoryInfo CariAkarRepository()
        {
            DirectoryInfo? kandidat = new(AppContext.BaseDirectory);

            while (kandidat is not null
                   && !File.Exists(Path.Combine(kandidat.FullName, "QuilvianSystemBackend.sln")))
            {
                kandidat = kandidat.Parent;
            }

            Assert.NotNull(kandidat);
            return kandidat!;
        }
    }
}
