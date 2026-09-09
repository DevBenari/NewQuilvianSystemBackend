using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Controllers;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Services;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.JournalType.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.Infrastructure;
using System.Reflection;

namespace QuilvianSystemBackend.Tests.AccountingManagement
{
    /// <summary>
    /// Bukti acceptance untuk <c>BE-ACC-P2-005</c> — daftar periksa penutupan periode.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Roadmap menuntut pembuktiannya lewat <b>test integrasi PostgreSQL</b>. Database test
    /// PostgreSQL tidak tersedia di lingkungan ini (<c>QUILVIAN_BILLING_TEST_DB</c> tidak diset,
    /// dan fixture-nya menolak berjalan tanpa itu), sehingga berkas ini membuktikan perilaku yang
    /// sama di atas SQLite dalam memori.
    /// </para>
    /// <para>
    /// Yang <b>tidak</b> dibuktikan berkas ini: perilaku khusus PostgreSQL. Untuk daftar periksa
    /// hal itu kecil risikonya — seluruh perhitungannya <c>COUNT</c> hanya-baca tanpa tipe
    /// khusus provider — tetapi tetap dicatat sebagai celah pada laporan task, bukan didiamkan.
    /// </para>
    /// <para>
    /// Seluruh data di bawah adalah data karangan.
    /// </para>
    /// </remarks>
    public class AccPeriodClosingChecklistTests
    {
        private static readonly Guid Pelaku = Guid.Parse("44444444-4444-4444-4444-444444444444");
        private static readonly Guid BadanHukum = Guid.Parse("55555555-5555-5555-5555-555555555555");
        private static readonly Guid PeriodeId = Guid.Parse("66666666-6666-6666-6666-666666666666");
        private static readonly Guid JenisJurnalId = Guid.Parse("77777777-7777-7777-7777-777777777777");

        /// <summary>
        /// Menyiapkan satu badan hukum bertanda default — <c>AccountingLegalEntityGuard</c>
        /// menuntut tepat satu — beserta satu periode <c>Open</c> dan satu jenis jurnal.
        /// </summary>
        private static async Task SiapkanAsync(TestDatabase database)
        {
            await using var db = database.CreateContext();

            db.Set<MstLegalEntity>().Add(new MstLegalEntity
            {
                Id = BadanHukum,
                LegalEntityCode = "RS-UJI",
                LegalEntityName = "Rumah Sakit Uji",
                IsDefault = true,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = Pelaku
            });

            db.Set<AccJournalType>().Add(new AccJournalType
            {
                Id = JenisJurnalId,
                JournalTypeCode = "JU",
                JournalTypeName = "Jurnal Umum",
                NumberPrefix = "JU",
                RequiresApproval = true,
                IsSystemType = false,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = Pelaku
            });

            db.Set<AccAccountingPeriod>().Add(new AccAccountingPeriod
            {
                Id = PeriodeId,
                LegalEntityId = BadanHukum,
                PeriodCode = "2026-09",
                FiscalYear = 2026,
                PeriodMonth = 9,
                StartDate = new DateTime(2026, 9, 1),
                EndDate = new DateTime(2026, 9, 30),
                PeriodStatus = AccountingPeriodStatus.Open,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = Pelaku
            });

            await db.SaveChangesAsync();
        }

        private static AccJournal Jurnal(string nomor, JournalStatus status, decimal debit = 100m, decimal kredit = 100m) => new()
        {
            Id = Guid.NewGuid(),
            LegalEntityId = BadanHukum,
            JournalNumber = nomor,
            JournalTypeId = JenisJurnalId,
            AccountingPeriodId = PeriodeId,
            AccountingDate = new DateTime(2026, 9, 15),
            Description = "Jurnal uji",
            JournalStatus = status,
            TotalDebit = debit,
            TotalCredit = kredit,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = Pelaku
        };

        /// <summary>Mengambil satu butir penghalang menurut kodenya.</summary>
        private static PeriodClosingBlockerResponse Penghalang(
            PeriodClosingChecklistResponse isi, string kode)
            => isi.Blockers.Single(x => x.Code == kode);

        // =====================================================================
        // Acceptance 1 — dihitung saat diminta, bukan disimpan
        // =====================================================================

        /// <summary>
        /// Skenario persis dari kolom Verifikasi roadmap: siapkan periode berisi 3 jurnal belum
        /// sah, panggil endpoint, sahkan satu, panggil lagi, pastikan angkanya turun.
        /// </summary>
        [Fact]
        public async Task TigaJurnalBelumSah_SahkanSatu_AngkanyaTurun()
        {
            using var database = TestDatabase.Create();
            await SiapkanAsync(database);

            Guid idYangDisahkan;
            await using (var db = database.CreateContext())
            {
                var satu = Jurnal("JU-001", JournalStatus.Draft);
                idYangDisahkan = satu.Id;

                db.Set<AccJournal>().AddRange(
                    satu,
                    Jurnal("JU-002", JournalStatus.PendingApproval),
                    Jurnal("JU-003", JournalStatus.Approved));

                await db.SaveChangesAsync();
            }

            // Panggilan pertama — tiga penghalang.
            int sebelum;
            await using (var db = database.CreateContext())
            {
                var hasil = await new AccPeriodClosingService(db).GetChecklistAsync(PeriodeId);

                Assert.True(hasil.Success);
                sebelum = Penghalang(hasil.Data!, AccPeriodClosingService.KodeJurnalBelumDisahkan).Count;
                Assert.Equal(3, sebelum);
                Assert.False(hasil.Data!.CanSubmitClosing);
            }

            // Satu jurnal disahkan di antara dua panggilan.
            await using (var db = database.CreateContext())
            {
                var jurnal = await db.Set<AccJournal>().SingleAsync(x => x.Id == idYangDisahkan);
                jurnal.JournalStatus = JournalStatus.Posted;
                await db.SaveChangesAsync();
            }

            // Panggilan kedua — angkanya WAJIB turun. Kalau hasilnya disimpan, angkanya tetap 3.
            await using (var db = database.CreateContext())
            {
                var hasil = await new AccPeriodClosingService(db).GetChecklistAsync(PeriodeId);

                var sesudah = Penghalang(hasil.Data!, AccPeriodClosingService.KodeJurnalBelumDisahkan).Count;
                Assert.Equal(2, sesudah);
                Assert.True(sesudah < sebelum);
            }
        }

        /// <summary>
        /// Periode tanpa jurnal belum sah dapat diajukan, dan seluruh penghalang bernilai nol.
        /// </summary>
        [Fact]
        public async Task PeriodeBersih_DapatDiajukan()
        {
            using var database = TestDatabase.Create();
            await SiapkanAsync(database);

            await using (var db = database.CreateContext())
            {
                db.Set<AccJournal>().Add(Jurnal("JU-010", JournalStatus.Posted));
                await db.SaveChangesAsync();
            }

            await using var konteks = database.CreateContext();
            var hasil = await new AccPeriodClosingService(konteks).GetChecklistAsync(PeriodeId);

            Assert.Equal(0, hasil.Data!.BlockingCount);
            Assert.True(hasil.Data.CanSubmitClosing);

            // Tetapi daftar periksanya BELUM lengkap, dan itu wajib terlihat.
            Assert.False(hasil.Data.IsComplete);
            Assert.True(hasil.Data.NotYetAvailableCount > 0);
        }

        // =====================================================================
        // Acceptance 2 — Draft, PendingApproval, Approved menahan; Posted tidak
        // =====================================================================

        /// <summary>
        /// Ketiga status belum sah terhitung, sedangkan <c>Posted</c> dan <c>Rejected</c> tidak.
        /// </summary>
        /// <remarks>
        /// <c>Rejected</c> adalah jebakannya. Menulis penghalang ini sebagai
        /// <c>JournalStatus != Posted</c> akan ikut menghitung jurnal yang pernah ditolak, dan
        /// periode yang sebenarnya bersih tidak akan pernah bisa ditutup — tanpa penjelasan apa
        /// pun bagi penggunanya.
        /// </remarks>
        [Fact]
        public async Task HanyaTigaStatusBelumSah_YangTerhitung()
        {
            using var database = TestDatabase.Create();
            await SiapkanAsync(database);

            await using (var db = database.CreateContext())
            {
                db.Set<AccJournal>().AddRange(
                    Jurnal("JU-101", JournalStatus.Draft),
                    Jurnal("JU-102", JournalStatus.PendingApproval),
                    Jurnal("JU-103", JournalStatus.Approved),
                    Jurnal("JU-104", JournalStatus.Posted),
                    Jurnal("JU-105", JournalStatus.Rejected));

                await db.SaveChangesAsync();
            }

            await using var konteks = database.CreateContext();
            var hasil = await new AccPeriodClosingService(konteks).GetChecklistAsync(PeriodeId);

            // Lima jurnal, tetapi hanya tiga yang menahan.
            Assert.Equal(3, Penghalang(hasil.Data!, AccPeriodClosingService.KodeJurnalBelumDisahkan).Count);
        }

        /// <summary>
        /// Jurnal periode lain tidak ikut terhitung.
        /// </summary>
        [Fact]
        public async Task JurnalPeriodeLain_TidakIkutTerhitung()
        {
            using var database = TestDatabase.Create();
            await SiapkanAsync(database);

            var periodeLain = Guid.NewGuid();
            await using (var db = database.CreateContext())
            {
                db.Set<AccAccountingPeriod>().Add(new AccAccountingPeriod
                {
                    Id = periodeLain,
                    LegalEntityId = BadanHukum,
                    PeriodCode = "2026-10",
                    FiscalYear = 2026,
                    PeriodMonth = 10,
                    StartDate = new DateTime(2026, 10, 1),
                    EndDate = new DateTime(2026, 10, 31),
                    PeriodStatus = AccountingPeriodStatus.Open,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = Pelaku
                });

                var lain = Jurnal("JU-201", JournalStatus.Draft);
                lain.AccountingPeriodId = periodeLain;

                db.Set<AccJournal>().AddRange(Jurnal("JU-200", JournalStatus.Draft), lain);
                await db.SaveChangesAsync();
            }

            await using var konteks = database.CreateContext();
            var hasil = await new AccPeriodClosingService(konteks).GetChecklistAsync(PeriodeId);

            Assert.Equal(1, Penghalang(hasil.Data!, AccPeriodClosingService.KodeJurnalBelumDisahkan).Count);
        }

        // =====================================================================
        // Acceptance 3 — kejadian Tertahan adalah PERINGATAN, bukan penghalang
        // =====================================================================

        [Fact]
        public async Task KejadianTertahan_AdaSebagaiPeringatan_BukanPenghalang()
        {
            using var database = TestDatabase.Create();
            await SiapkanAsync(database);

            await using var konteks = database.CreateContext();
            var isi = (await new AccPeriodClosingService(konteks).GetChecklistAsync(PeriodeId)).Data!;

            // Ada di daftar peringatan.
            var tertahan = isi.Warnings
                .Single(x => x.Code == AccPeriodClosingService.KodeKejadianTertahan);

            Assert.False(tertahan.IsBlocking);

            // Dan TIDAK ada di daftar penghalang.
            Assert.DoesNotContain(isi.Blockers,
                x => x.Code == AccPeriodClosingService.KodeKejadianTertahan);

            // Sebaliknya, kejadian GAGAL memang penghalang — inilah pasangan yang mudah tertukar.
            Assert.Contains(isi.Blockers, x => x.Code == AccPeriodClosingService.KodeKejadianGagal);
        }

        // =====================================================================
        // Acceptance 5 — tempat penghalang ketiga disediakan, tetapi kosong
        // =====================================================================

        /// <summary>
        /// Penghalang shift kasir (<c>ACC-DEC-065</c>) sudah punya tempatnya pada respons,
        /// bernilai kosong, dan menyatakan terang-terangan bahwa ia belum dapat diperiksa.
        /// </summary>
        [Fact]
        public async Task PenghalangKetiga_AdaTempatnya_TetapiBelumDapatDiperiksa()
        {
            using var database = TestDatabase.Create();
            await SiapkanAsync(database);

            await using var konteks = database.CreateContext();
            var isi = (await new AccPeriodClosingService(konteks).GetChecklistAsync(PeriodeId)).Data!;

            // Ketiga penghalang ACC-DEC-051 + ACC-DEC-065 selalu hadir.
            Assert.Equal(3, isi.Blockers.Count);

            var shift = isi.Blockers
                .Single(x => x.Code == AccPeriodClosingService.KodeShiftKasirBelumTutup);

            Assert.Equal(PeriodChecklistItemState.NotYetAvailable, shift.State);
            Assert.Equal(0, shift.Count);
            Assert.False(shift.IsBlocking);
            Assert.False(string.IsNullOrWhiteSpace(shift.UnavailableReason));
        }

        /// <summary>
        /// Butir yang belum dapat diperiksa tidak boleh terbaca sebagai "aman".
        /// </summary>
        [Fact]
        public async Task ButirBelumTersedia_TidakTerbacaSebagaiAman()
        {
            using var database = TestDatabase.Create();
            await SiapkanAsync(database);

            await using var konteks = database.CreateContext();
            var isi = (await new AccPeriodClosingService(konteks).GetChecklistAsync(PeriodeId)).Data!;

            var belumTersedia = isi.Blockers.Concat(isi.Warnings)
                .Where(x => x.State == PeriodChecklistItemState.NotYetAvailable)
                .ToList();

            Assert.NotEmpty(belumTersedia);
            Assert.All(belumTersedia, x => Assert.False(string.IsNullOrWhiteSpace(x.UnavailableReason)));
            Assert.Equal(belumTersedia.Count, isi.NotYetAvailableCount);
            Assert.False(isi.IsComplete);
        }

        // =====================================================================
        // Acceptance 4 — hak akses terpasang dan cocok dengan AccessController
        // =====================================================================

        /// <summary>
        /// Endpoint membawa <c>[AccessPermission]</c>, dan argumen pertamanya <b>sama persis</b>
        /// dengan <c>ControllerName</c> pada <c>[AccessController]</c>.
        /// </summary>
        /// <remarks>
        /// Kalau keduanya berselisih, hasilnya <c>403</c> permanen yang tidak dapat diperbaiki
        /// dari layar Akses Role — kegagalan yang tampak seperti masalah hak akses pengguna,
        /// padahal letaknya di kode.
        /// </remarks>
        [Fact]
        public void Endpoint_MembawaHakAksesYangCocokDenganAccessController()
        {
            var controller = typeof(AccountingPeriodController);

            var namaController = controller.GetCustomAttribute<AccessControllerAttribute>()!.ControllerName;

            var method = controller.GetMethod(nameof(AccountingPeriodController.GetClosingChecklist))!;

            var aksi = method.GetCustomAttribute<AccessActionAttribute>();
            var izin = method.GetCustomAttribute<AccessPermissionAttribute>();

            Assert.NotNull(aksi);
            Assert.NotNull(izin);

            var argumen = izin!.Arguments!;
            Assert.Equal(namaController, (string)argumen[0]);
            Assert.Equal("Read", (string)argumen[1]);
        }
    }
}
