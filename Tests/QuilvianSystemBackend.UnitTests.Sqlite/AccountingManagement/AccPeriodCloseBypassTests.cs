using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Tests.Infrastructure;

namespace QuilvianSystemBackend.Tests.AccountingManagement
{
    /// <summary>
    /// Menjaga bahwa <c>POST /{id}/close</c> <b>tidak lagi</b> dapat dipakai melewati persetujuan
    /// penutupan (<c>ACC-DEC-052</c>, <c>ACC-STATE-0.2</c> bagian 2).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Sampai MVP, endpoint tutup menerima <c>Open</c> menjadi <c>SoftClosed</c> maupun
    /// <c>Closed</c> secara langsung. Sesudah <c>BE-ACC-P2-006</c> berdiri, perilaku itu menjadi
    /// <b>jalan pintas</b>: siapa pun berhak <c>Close</c> dapat menutup periode tanpa dilihat
    /// orang kedua, dan seluruh mekanisme empat mata menjadi hiasan.
    /// </para>
    /// <para>
    /// Yang membuatnya mahal adalah tidak adanya error: periode tertutup rapi, angkanya final,
    /// dan tidak ada yang menandai bahwa tidak seorang pun pernah memeriksanya. Berkas ini
    /// adalah jaring yang mencegah perilaku itu kembali.
    /// </para>
    /// <para>
    /// Seluruh data di bawah adalah data karangan.
    /// </para>
    /// </remarks>
    public class AccPeriodCloseBypassTests
    {
        private static readonly Guid Pelaku = Guid.Parse("f0f0f0f0-0000-0000-0000-000000000001");
        private static readonly Guid BadanHukum = Guid.Parse("f1f1f1f1-0000-0000-0000-000000000002");
        private static readonly Guid PeriodeId = Guid.Parse("f2f2f2f2-0000-0000-0000-000000000003");

        private static async Task SiapkanAsync(TestDatabase database, AccountingPeriodStatus status)
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

            db.Set<AccAccountingPeriod>().Add(new AccAccountingPeriod
            {
                Id = PeriodeId,
                LegalEntityId = BadanHukum,
                PeriodCode = "2026-09",
                FiscalYear = 2026,
                PeriodMonth = 9,
                StartDate = new DateTime(2026, 9, 1),
                EndDate = new DateTime(2026, 9, 30),
                PeriodStatus = status,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = Pelaku
            });

            await db.SaveChangesAsync();
        }

        private static async Task<AccountingPeriodStatus> StatusAsync(TestDatabase database)
        {
            await using var db = database.CreateContext();
            return (await db.Set<AccAccountingPeriod>().SingleAsync(x => x.Id == PeriodeId)).PeriodStatus;
        }

        // =====================================================================
        // Jalan pintas ditutup
        // =====================================================================

        /// <summary>
        /// Periode <c>Open</c> tidak dapat ditutup langsung — baik tutup sementara maupun tutup
        /// permanen. Inilah jalan pintas yang dicabut.
        /// </summary>
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task PeriodeTerbuka_TidakDapatDitutupLangsung(bool permanen)
        {
            using var database = TestDatabase.Create();
            await SiapkanAsync(database, AccountingPeriodStatus.Open);

            await using (var db = database.CreateContext())
            {
                var hasil = await new AccAccountingPeriodService(db).CloseAsync(
                    PeriodeId,
                    new ClosePeriodRequest { Permanent = permanen },
                    Pelaku);

                Assert.False(hasil.Success);
                Assert.Equal(409, hasil.StatusCode);

                // Pesannya wajib menunjukkan jalan yang benar, bukan sekadar menolak.
                Assert.Contains("Ajukan penutupan", hasil.Message);
            }

            // Dan periodenya benar-benar tidak bergerak.
            Assert.Equal(AccountingPeriodStatus.Open, await StatusAsync(database));
        }

        /// <summary>
        /// Periode yang sedang menunggu persetujuan juga tidak dapat ditutup lewat endpoint ini —
        /// jalur keluarnya adalah menyetujui atau menolak.
        /// </summary>
        [Fact]
        public async Task PeriodeMenungguPersetujuan_TidakDapatDitutupLangsung()
        {
            using var database = TestDatabase.Create();
            await SiapkanAsync(database, AccountingPeriodStatus.PendingClosingApproval);

            await using (var db = database.CreateContext())
            {
                var hasil = await new AccAccountingPeriodService(db).CloseAsync(
                    PeriodeId, new ClosePeriodRequest { Permanent = false }, Pelaku);

                Assert.False(hasil.Success);
                Assert.Equal(409, hasil.StatusCode);
                Assert.Contains("menunggu persetujuan", hasil.Message);
            }

            Assert.Equal(AccountingPeriodStatus.PendingClosingApproval, await StatusAsync(database));
        }

        // =====================================================================
        // Yang tetap sah
        // =====================================================================

        /// <summary>
        /// Penutupan permanen dari <c>SoftClosed</c> <b>tetap berjalan</b>. Perbaikan ini
        /// mempersempit, bukan mematikan endpoint tutup.
        /// </summary>
        [Fact]
        public async Task TutupPermanenDariTutupSementara_TetapBerjalan()
        {
            using var database = TestDatabase.Create();
            await SiapkanAsync(database, AccountingPeriodStatus.SoftClosed);

            await using (var db = database.CreateContext())
            {
                var hasil = await new AccAccountingPeriodService(db).CloseAsync(
                    PeriodeId,
                    new ClosePeriodRequest { Permanent = true, Reason = "Laporan September sudah terbit." },
                    Pelaku);

                Assert.True(hasil.Success);
            }

            Assert.Equal(AccountingPeriodStatus.Closed, await StatusAsync(database));
        }

        /// <summary>
        /// Periode yang sudah tutup permanen tetap tidak dapat disentuh endpoint ini.
        /// </summary>
        [Fact]
        public async Task PeriodeTutupPermanen_TetapDitolak()
        {
            using var database = TestDatabase.Create();
            await SiapkanAsync(database, AccountingPeriodStatus.Closed);

            await using var db = database.CreateContext();
            var hasil = await new AccAccountingPeriodService(db).CloseAsync(
                PeriodeId, new ClosePeriodRequest { Permanent = false }, Pelaku);

            Assert.False(hasil.Success);
            Assert.Equal(409, hasil.StatusCode);
            Assert.Contains("pembukaan kembali", hasil.Message);
        }

        // =====================================================================
        // Satu-satunya jalan menuju SoftClosed adalah lewat persetujuan
        // =====================================================================

        /// <summary>
        /// Membuktikan jalannya utuh: periode terbuka hanya dapat menjadi tutup sementara lewat
        /// pengajuan dan persetujuan dua orang berbeda.
        /// </summary>
        [Fact]
        public async Task SatuSatunyaJalanKeTutupSementara_LewatPersetujuanDuaOrang()
        {
            var pengaju = Guid.Parse("f3f3f3f3-0000-0000-0000-000000000004");
            var penyetuju = Guid.Parse("f4f4f4f4-0000-0000-0000-000000000005");

            using var database = TestDatabase.Create();
            await SiapkanAsync(database, AccountingPeriodStatus.Open);

            // Jalan pintas tertutup.
            await using (var db = database.CreateContext())
            {
                var pintas = await new AccAccountingPeriodService(db).CloseAsync(
                    PeriodeId, new ClosePeriodRequest { Permanent = false }, pengaju);

                Assert.False(pintas.Success);
            }

            // Jalan yang benar terbuka.
            await using (var db = database.CreateContext())
            {
                var ajukan = await new AccPeriodClosingService(db)
                    .SubmitClosingAsync(PeriodeId, new SubmitPeriodClosingRequest(), pengaju);

                Assert.True(ajukan.Success);
            }

            await using (var db = database.CreateContext())
            {
                var setujui = await new AccPeriodClosingService(db)
                    .ApproveClosingAsync(PeriodeId, new ApprovePeriodClosingRequest(), penyetuju);

                Assert.True(setujui.Success);
            }

            Assert.Equal(AccountingPeriodStatus.SoftClosed, await StatusAsync(database));
        }
    }
}
