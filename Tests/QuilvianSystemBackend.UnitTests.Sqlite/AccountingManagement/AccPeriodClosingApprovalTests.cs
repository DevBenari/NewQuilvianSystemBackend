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
using QuilvianSystemBackend.Tests.Infrastructure;
using System.Reflection;

namespace QuilvianSystemBackend.Tests.AccountingManagement
{
    /// <summary>
    /// Bukti acceptance untuk <c>BE-ACC-P2-006</c> — ajukan, setujui, dan tolak penutupan periode.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Dua acceptance yang roadmap tandai paling penting diuji terpisah dan diberi penjelasan
    /// tersendiri: <b>(2) penyetuju bukan pengaju</b>, dan <b>(4) periode yang sudah tertutup
    /// sebelum Phase 2 tetap sah tanpa riwayat persetujuan</b>.
    /// </para>
    /// <para>
    /// Roadmap menuntut pembuktian lewat test integrasi PostgreSQL. Database test PostgreSQL
    /// tidak tersedia di lingkungan ini, sehingga berkas ini membuktikannya di atas SQLite —
    /// dicatat sebagai celah pada laporan task, bukan didiamkan.
    /// </para>
    /// <para>
    /// Seluruh data di bawah adalah data karangan.
    /// </para>
    /// </remarks>
    public class AccPeriodClosingApprovalTests
    {
        private static readonly Guid Manajer = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001");
        private static readonly Guid Direktur = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000002");
        private static readonly Guid BadanHukum = Guid.Parse("cccccccc-0000-0000-0000-000000000003");
        private static readonly Guid PeriodeId = Guid.Parse("dddddddd-0000-0000-0000-000000000004");
        private static readonly Guid JenisJurnalId = Guid.Parse("eeeeeeee-0000-0000-0000-000000000005");

        private static async Task SiapkanAsync(
            TestDatabase database,
            AccountingPeriodStatus status = AccountingPeriodStatus.Open)
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
                CreateBy = Manajer
            });

            db.Set<AccJournalType>().Add(new AccJournalType
            {
                Id = JenisJurnalId,
                JournalTypeCode = "JU",
                JournalTypeName = "Jurnal Umum",
                NumberPrefix = "JU",
                RequiresApproval = true,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = Manajer
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
                CreateBy = Manajer
            });

            await db.SaveChangesAsync();
        }

        private static AccJournal Jurnal(string nomor, JournalStatus status) => new()
        {
            Id = Guid.NewGuid(),
            LegalEntityId = BadanHukum,
            JournalNumber = nomor,
            JournalTypeId = JenisJurnalId,
            AccountingPeriodId = PeriodeId,
            AccountingDate = new DateTime(2026, 9, 15),
            Description = "Jurnal uji",
            JournalStatus = status,
            TotalDebit = 100m,
            TotalCredit = 100m,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = Manajer
        };

        /// <summary>Mengajukan penutupan sebagai Manajer, dan memastikan berhasil.</summary>
        private static async Task AjukanAsync(TestDatabase database)
        {
            await using var db = database.CreateContext();
            var hasil = await new AccPeriodClosingService(db)
                .SubmitClosingAsync(PeriodeId, new SubmitPeriodClosingRequest(), Manajer);

            Assert.True(hasil.Success);
        }

        private static async Task<AccountingPeriodStatus> StatusPeriodeAsync(TestDatabase database)
        {
            await using var db = database.CreateContext();
            return (await db.Set<AccAccountingPeriod>().SingleAsync(x => x.Id == PeriodeId)).PeriodStatus;
        }

        // =====================================================================
        // Acceptance 1 — pengajuan ditolak 409 selama masih ada penghalang
        // =====================================================================

        [Fact]
        public async Task MasihAdaJurnalBelumSah_PengajuanDitolak409()
        {
            using var database = TestDatabase.Create();
            await SiapkanAsync(database);

            await using (var db = database.CreateContext())
            {
                db.Set<AccJournal>().Add(Jurnal("JU-001", JournalStatus.Draft));
                await db.SaveChangesAsync();
            }

            await using (var db = database.CreateContext())
            {
                var hasil = await new AccPeriodClosingService(db)
                    .SubmitClosingAsync(PeriodeId, new SubmitPeriodClosingRequest(), Manajer);

                Assert.False(hasil.Success);
                Assert.Equal(409, hasil.StatusCode);
                Assert.Contains("belum disahkan", hasil.Message);
            }

            // Periode TIDAK bergeser statusnya.
            Assert.Equal(AccountingPeriodStatus.Open, await StatusPeriodeAsync(database));
        }

        [Fact]
        public async Task PeriodeBersih_PengajuanBerhasil_DanStatusMenungguPersetujuan()
        {
            using var database = TestDatabase.Create();
            await SiapkanAsync(database);

            await using (var db = database.CreateContext())
            {
                db.Set<AccJournal>().Add(Jurnal("JU-010", JournalStatus.Posted));
                await db.SaveChangesAsync();
            }

            await using (var db = database.CreateContext())
            {
                var hasil = await new AccPeriodClosingService(db)
                    .SubmitClosingAsync(PeriodeId, new SubmitPeriodClosingRequest { Note = "Siap tutup" }, Manajer);

                Assert.True(hasil.Success);
                Assert.Equal(1, hasil.Data!.ActionSequence);
                Assert.Equal(PeriodClosingAction.Submitted, hasil.Data.Action);
                Assert.Equal(Manajer, hasil.Data.ActionBy);
            }

            Assert.Equal(AccountingPeriodStatus.PendingClosingApproval, await StatusPeriodeAsync(database));

            // Pengaju tercatat pada periode — inilah dasar prinsip empat mata.
            await using var pemeriksa = database.CreateContext();
            var periode = await pemeriksa.Set<AccAccountingPeriod>().SingleAsync(x => x.Id == PeriodeId);
            Assert.Equal(Manajer, periode.ClosingSubmittedBy);
            Assert.NotNull(periode.ClosingSubmittedAt);
        }

        // =====================================================================
        // Acceptance 2 — penyetuju sama dengan pengaju ditolak 403
        // =====================================================================

        /// <summary>
        /// Prinsip empat mata: orang yang mengajukan tidak boleh menyetujui pengajuannya sendiri.
        /// </summary>
        /// <remarks>
        /// Ini acceptance yang roadmap tandai paling penting. Bila lolos, seluruh gunanya
        /// persetujuan penutupan hilang — satu orang dapat menyatakan angka bulanan final tanpa
        /// pernah dilihat siapa pun, dan tidak ada error apa pun yang menandainya.
        /// </remarks>
        [Fact]
        public async Task PenyetujuSamaDenganPengaju_Ditolak403()
        {
            using var database = TestDatabase.Create();
            await SiapkanAsync(database);
            await AjukanAsync(database);

            await using (var db = database.CreateContext())
            {
                var hasil = await new AccPeriodClosingService(db)
                    .ApproveClosingAsync(PeriodeId, new ApprovePeriodClosingRequest(), Manajer);

                Assert.False(hasil.Success);
                Assert.Equal(403, hasil.StatusCode);
                Assert.Contains("mengajukannya", hasil.Message);
            }

            // Periode tetap menunggu; tidak diam-diam tertutup.
            Assert.Equal(AccountingPeriodStatus.PendingClosingApproval, await StatusPeriodeAsync(database));
        }

        [Fact]
        public async Task PenyetujuOrangLain_Diterima_DanPeriodeMenjadiTutupSementara()
        {
            using var database = TestDatabase.Create();
            await SiapkanAsync(database);
            await AjukanAsync(database);

            await using (var db = database.CreateContext())
            {
                var hasil = await new AccPeriodClosingService(db)
                    .ApproveClosingAsync(PeriodeId, new ApprovePeriodClosingRequest(), Direktur);

                Assert.True(hasil.Success);
                Assert.Equal(2, hasil.Data!.ActionSequence);
                Assert.Equal(PeriodClosingAction.Approved, hasil.Data.Action);
                Assert.Equal(Direktur, hasil.Data.ActionBy);
            }

            // SoftClosed, BUKAN Closed — tutup permanen hanya dari SoftClosed.
            Assert.Equal(AccountingPeriodStatus.SoftClosed, await StatusPeriodeAsync(database));
        }

        // =====================================================================
        // Acceptance 3 — penolakan tanpa alasan ditolak 400; periode kembali Open
        // =====================================================================

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task PenolakanTanpaAlasan_Ditolak400(string? alasan)
        {
            using var database = TestDatabase.Create();
            await SiapkanAsync(database);
            await AjukanAsync(database);

            await using (var db = database.CreateContext())
            {
                var hasil = await new AccPeriodClosingService(db)
                    .RejectClosingAsync(PeriodeId, new RejectPeriodClosingRequest { Reason = alasan }, Direktur);

                Assert.False(hasil.Success);
                Assert.Equal(400, hasil.StatusCode);
            }

            // Nol perubahan — periode masih menunggu persetujuan.
            Assert.Equal(AccountingPeriodStatus.PendingClosingApproval, await StatusPeriodeAsync(database));

            await using var pemeriksa = database.CreateContext();
            Assert.Equal(1, await pemeriksa.Set<AccPeriodClosingApproval>().CountAsync());
        }

        [Fact]
        public async Task PenolakanBeralasan_PeriodeKembaliTerbuka()
        {
            using var database = TestDatabase.Create();
            await SiapkanAsync(database);
            await AjukanAsync(database);

            await using (var db = database.CreateContext())
            {
                var hasil = await new AccPeriodClosingService(db).RejectClosingAsync(
                    PeriodeId,
                    new RejectPeriodClosingRequest { Reason = "Penyusutan September belum dijurnal." },
                    Direktur);

                Assert.True(hasil.Success);
                Assert.Equal(PeriodClosingAction.Rejected, hasil.Data!.Action);
                Assert.Equal("Penyusutan September belum dijurnal.", hasil.Data.ActionNote);
            }

            Assert.Equal(AccountingPeriodStatus.Open, await StatusPeriodeAsync(database));

            // Pengaju dikosongkan supaya putaran berikutnya dinilai dari nol.
            await using var pemeriksa = database.CreateContext();
            var periode = await pemeriksa.Set<AccAccountingPeriod>().SingleAsync(x => x.Id == PeriodeId);
            Assert.Null(periode.ClosingSubmittedBy);
            Assert.Null(periode.ClosingSubmittedAt);
        }

        /// <summary>
        /// Sesudah ditolak, periode dapat diajukan ulang — dan riwayatnya bertambah, tidak
        /// menimpa.
        /// </summary>
        [Fact]
        public async Task SesudahDitolak_DapatDiajukanUlang_RiwayatBertambah()
        {
            using var database = TestDatabase.Create();
            await SiapkanAsync(database);
            await AjukanAsync(database);

            await using (var db = database.CreateContext())
            {
                await new AccPeriodClosingService(db).RejectClosingAsync(
                    PeriodeId, new RejectPeriodClosingRequest { Reason = "Perbaiki dulu." }, Direktur);
            }

            await AjukanAsync(database);

            await using var pemeriksa = database.CreateContext();
            var riwayat = await pemeriksa.Set<AccPeriodClosingApproval>()
                .OrderBy(x => x.ActionSequence)
                .ToListAsync();

            Assert.Equal(3, riwayat.Count);
            Assert.Equal(new[] { 1, 2, 3 }, riwayat.Select(x => x.ActionSequence));
            Assert.Equal(
                new[] { PeriodClosingAction.Submitted, PeriodClosingAction.Rejected, PeriodClosingAction.Submitted },
                riwayat.Select(x => x.Action));
        }

        // =====================================================================
        // Acceptance 4 — periode SoftClosed sebelum Phase 2 tetap sah tanpa riwayat
        // =====================================================================

        /// <summary>
        /// Periode yang sudah <c>SoftClosed</c> sejak sebelum Phase 2 <b>tidak punya</b> riwayat
        /// persetujuan, dan itu benar.
        /// </summary>
        /// <remarks>
        /// Roadmap menandai ini sebagai acceptance yang wajib diuji. Bahayanya bukan error yang
        /// terlihat, melainkan sistem yang memperlakukan periode lama sebagai data janggal —
        /// misalnya menolak membacanya, atau menandainya perlu diperbaiki. Mereka ditutup ketika
        /// aturan persetujuannya memang belum ada.
        /// </remarks>
        [Fact]
        public async Task PeriodeSoftClosedSebelumPhase2_TetapSahTanpaRiwayat()
        {
            using var database = TestDatabase.Create();
            await SiapkanAsync(database, AccountingPeriodStatus.SoftClosed);

            await using var db = database.CreateContext();
            var layanan = new AccPeriodClosingService(db);

            // Nol baris riwayat, dan itu bukan error.
            var riwayat = await layanan.GetClosingHistoryAsync(PeriodeId);
            Assert.True(riwayat.Success);
            Assert.Empty(riwayat.Data!);

            // Kedua kolom Phase 2 kosong, dan periodenya tetap terbaca.
            var periode = await db.Set<AccAccountingPeriod>().SingleAsync(x => x.Id == PeriodeId);
            Assert.Null(periode.ClosingSubmittedBy);
            Assert.Null(periode.ClosingSubmittedAt);
            Assert.Equal(AccountingPeriodStatus.SoftClosed, periode.PeriodStatus);

            // Daftar periksanya pun tetap dapat dihitung tanpa meledak.
            var daftar = await layanan.GetChecklistAsync(PeriodeId);
            Assert.True(daftar.Success);
            Assert.Equal(AccountingPeriodStatus.SoftClosed, daftar.Data!.PeriodStatus);

            // Tetapi ia tidak dapat diajukan lagi — sudah bukan Open.
            Assert.False(daftar.Data.CanSubmitClosing);
        }

        /// <summary>
        /// Periode yang sudah tertutup tidak dapat diajukan penutupannya.
        /// </summary>
        [Fact]
        public async Task PeriodeSudahTertutup_PengajuanDitolak409()
        {
            using var database = TestDatabase.Create();
            await SiapkanAsync(database, AccountingPeriodStatus.SoftClosed);

            await using var db = database.CreateContext();
            var hasil = await new AccPeriodClosingService(db)
                .SubmitClosingAsync(PeriodeId, new SubmitPeriodClosingRequest(), Manajer);

            Assert.False(hasil.Success);
            Assert.Equal(409, hasil.StatusCode);
        }

        /// <summary>
        /// Menyetujui periode yang tidak sedang menunggu ditolak <c>409</c>, bukan diam-diam
        /// diterima.
        /// </summary>
        [Fact]
        public async Task MenyetujuiPeriodeYangTidakMenunggu_Ditolak409()
        {
            using var database = TestDatabase.Create();
            await SiapkanAsync(database);

            await using var db = database.CreateContext();
            var hasil = await new AccPeriodClosingService(db)
                .ApproveClosingAsync(PeriodeId, new ApprovePeriodClosingRequest(), Direktur);

            Assert.False(hasil.Success);
            Assert.Equal(409, hasil.StatusCode);
        }

        // =====================================================================
        // Acceptance 5 — ketiga endpoint membawa [AccessPermission] yang benar
        // =====================================================================

        [Theory]
        [InlineData(nameof(AccountingPeriodController.SubmitClosing), "Close")]
        [InlineData(nameof(AccountingPeriodController.ApproveClosing), "Approve")]
        [InlineData(nameof(AccountingPeriodController.RejectClosing), "Approve")]
        public void KetigaEndpoint_MembawaHakAksesYangBenar(string namaMethod, string aksiDiharapkan)
        {
            var controller = typeof(AccountingPeriodController);
            var namaController = controller.GetCustomAttribute<AccessControllerAttribute>()!.ControllerName;

            var method = controller.GetMethod(namaMethod)!;

            var aksi = method.GetCustomAttribute<AccessActionAttribute>();
            var izin = method.GetCustomAttribute<AccessPermissionAttribute>();

            Assert.NotNull(aksi);
            Assert.NotNull(izin);

            var argumen = izin!.Arguments!;

            // Argumen ke-1 wajib sama persis dengan ControllerName, kalau tidak hasilnya 403
            // permanen yang tidak dapat diperbaiki dari layar Akses Role.
            Assert.Equal(namaController, (string)argumen[0]);
            Assert.Equal(aksiDiharapkan, (string)argumen[1]);
        }

        /// <summary>
        /// Menyetujui memakai hak akses <c>Approve</c> yang <b>terpisah</b> dari <c>Close</c>.
        /// </summary>
        /// <remarks>
        /// Pemisahan inilah yang membuat prinsip empat mata dapat ditegakkan dari layar Akses
        /// Role: peran yang boleh mengajukan tidak perlu diberi hak menyetujui.
        /// </remarks>
        [Fact]
        public void HakAksesMenyetujui_TerpisahDariMengajukan()
        {
            var controller = typeof(AccountingPeriodController);

            string Aksi(string nama) => (string)controller.GetMethod(nama)!
                .GetCustomAttribute<AccessPermissionAttribute>()!.Arguments![1];

            Assert.NotEqual(
                Aksi(nameof(AccountingPeriodController.SubmitClosing)),
                Aksi(nameof(AccountingPeriodController.ApproveClosing)));
        }
    }
}
