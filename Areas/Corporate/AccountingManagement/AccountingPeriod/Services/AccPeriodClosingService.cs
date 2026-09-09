using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.Services;
using QuilvianSystemBackend.Repositories;
using System.Globalization;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Services
{
    /// <summary>
    /// Penutupan periode: daftar periksanya, dan — mulai <c>BE-ACC-P2-006</c> — pengajuan,
    /// persetujuan, serta penolakannya.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Dipisahkan dari <see cref="AccAccountingPeriodService"/> karena keduanya menjawab
    /// pertanyaan yang berbeda. Service periode mengurus daur hidup periode itu sendiri —
    /// membangkitkan, menutup, membuka kembali. Service ini mengurus <b>proses persetujuan
    /// penutupan</b> yang melibatkan dua orang, dan ia membaca jurnal serta kejadian keuangan,
    /// yaitu data milik submodule lain.
    /// </para>
    /// <para>
    /// <b>Nol hasil hitungan disimpan.</b> Seluruh angka pada daftar periksa dihitung ulang
    /// setiap kali diminta. Menyimpannya akan membuat penutupan ditolak berdasarkan keadaan yang
    /// sudah berubah — dan yang lebih buruk, membuatnya <i>diterima</i> berdasarkan keadaan yang
    /// sudah berubah.
    /// </para>
    /// </remarks>
    public class AccPeriodClosingService
    {
        private readonly ApplicationDbContext _db;

        public AccPeriodClosingService(ApplicationDbContext db)
        {
            _db = db;
        }

        private static readonly string[] NamaBulan =
        {
            "Januari", "Februari", "Maret", "April", "Mei", "Juni",
            "Juli", "Agustus", "September", "Oktober", "November", "Desember"
        };

        /// <summary>
        /// Status jurnal yang dihitung sebagai <b>belum disahkan</b>, dan karena itu menahan
        /// penutupan (<c>ACC-DEC-051</c>).
        /// </summary>
        /// <remarks>
        /// Sengaja ditulis sebagai daftar tegas, <b>bukan</b> sebagai <c>!= Posted</c>. Jurnal
        /// <c>Rejected</c> juga bukan <c>Posted</c>, tetapi ia sudah selesai urusannya dan tidak
        /// menahan apa pun. Menuliskannya sebagai negasi akan diam-diam menahan penutupan setiap
        /// kali ada satu jurnal yang pernah ditolak di bulan itu.
        /// </remarks>
        private static readonly JournalStatus[] StatusBelumDisahkan =
        {
            JournalStatus.Draft,
            JournalStatus.PendingApproval,
            JournalStatus.Approved
        };

        // ------------------------------------------------------------------
        // Kode butir daftar periksa
        // ------------------------------------------------------------------

        public const string KodeJurnalBelumDisahkan = "UNPOSTED_JOURNALS";
        public const string KodeKejadianGagal = "FAILED_EVENTS";
        public const string KodeShiftKasirBelumTutup = "OPEN_CASH_SHIFTS";

        public const string KodeJurnalBelumSeimbang = "UNBALANCED_JOURNALS";
        public const string KodeKejadianTertahan = "HELD_EVENTS";
        public const string KodeIntegrasiBelumCocok = "INTEGRATION_MISMATCH";
        public const string KodePenyusutanBelumJalan = "DEPRECIATION_NOT_RUN";
        public const string KodeSaldoAkunSementara = "SUSPENSE_ACCOUNT_BALANCE";
        public const string KodeSelisihSaldoAwalAkhir = "OPENING_CLOSING_MISMATCH";

        private const string MenungguKotakMasukKejadian =
            "Kotak masuk kejadian keuangan belum berdiri; pemeriksaan ini menyusul pada gelombang P2-1.";

        // ------------------------------------------------------------------
        // Daftar periksa
        // ------------------------------------------------------------------

        /// <summary>
        /// Menghitung daftar periksa penutupan sebuah periode.
        /// </summary>
        public async Task<AccountingServiceResult<PeriodClosingChecklistResponse>> GetChecklistAsync(
            Guid accountingPeriodId,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard
                .PeriksaAsync<PeriodClosingChecklistResponse>(_db, ct);
            if (penjaga is not null) return penjaga;

            var periode = await _db.Set<AccAccountingPeriod>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == accountingPeriodId && !x.IsDelete, ct);

            if (periode is null)
            {
                return AccountingServiceResult<PeriodClosingChecklistResponse>.Fail(
                    StatusCodes.Status404NotFound, "Periode akuntansi tidak ditemukan.");
            }

            var jurnalBelumDisahkan = await HitungJurnalBelumDisahkanAsync(accountingPeriodId, ct);
            var jurnalBelumSeimbang = await HitungJurnalBelumSeimbangAsync(accountingPeriodId, ct);

            var penghalang = new List<PeriodClosingBlockerResponse>
            {
                // Penghalang 1 — satu-satunya yang sudah dapat diperiksa sepenuhnya.
                Butir(
                    KodeJurnalBelumDisahkan,
                    "Jurnal belum disahkan",
                    jurnalBelumDisahkan > 0
                        ? $"Masih ada {jurnalBelumDisahkan} jurnal yang belum disahkan."
                        : "Seluruh jurnal periode ini sudah disahkan.",
                    jurnalBelumDisahkan,
                    menahan: true),

                // Penghalang 2 — ACC-DEC-051.
                ButirBelumTersedia(
                    KodeKejadianGagal,
                    "Kejadian keuangan gagal",
                    "Belum dapat diperiksa: kotak masuk kejadian keuangan belum berdiri.",
                    menahan: true),

                // Penghalang 3 — ACC-DEC-065. Tempatnya disediakan sekarang supaya bentuk
                // respons tidak berubah saat penghalang ini diaktifkan.
                ButirBelumTersedia(
                    KodeShiftKasirBelumTutup,
                    "Shift kasir belum ditutup",
                    "Belum dapat diperiksa: kejadian CASH_SHIFT_CLOSED belum mengalir.",
                    menahan: true)
            };

            var peringatan = new List<PeriodClosingBlockerResponse>
            {
                Butir(
                    KodeJurnalBelumSeimbang,
                    "Jurnal belum seimbang",
                    jurnalBelumSeimbang > 0
                        ? $"Ada {jurnalBelumSeimbang} jurnal draft yang total debit dan kreditnya belum sama."
                        : "Tidak ada jurnal yang belum seimbang.",
                    jurnalBelumSeimbang,
                    menahan: false),

                ButirBelumTersedia(
                    KodeKejadianTertahan,
                    "Kejadian keuangan tertahan",
                    "Belum dapat diperiksa: kotak masuk kejadian keuangan belum berdiri.",
                    menahan: false),

                ButirBelumTersedia(
                    KodeIntegrasiBelumCocok,
                    "Integrasi belum cocok",
                    "Belum dapat diperiksa: penyambungan ke modul lain belum berdiri.",
                    menahan: false),

                ButirBelumTersedia(
                    KodePenyusutanBelumJalan,
                    "Penyusutan belum dijalankan",
                    "Belum dapat diperiksa: penjadwal jurnal berulang belum berdiri (BE-ACC-P2-008).",
                    menahan: false,
                    alasan: "Penjadwal jurnal berulang belum berdiri; pemeriksaan ini menyusul pada BE-ACC-P2-008."),

                ButirBelumTersedia(
                    KodeSaldoAkunSementara,
                    "Saldo tertinggal di akun sementara",
                    "Belum dapat diperiksa: penandaan akun sementara belum diputuskan.",
                    menahan: false,
                    alasan: "Belum ada penanda akun sementara pada daftar akun; menunggu keputusan owner."),

                ButirBelumTersedia(
                    KodeSelisihSaldoAwalAkhir,
                    "Selisih saldo awal dan saldo akhir",
                    "Belum dapat diperiksa: pembandingan saldo antarperiode belum berdiri.",
                    menahan: false,
                    alasan: "Pembandingan saldo penutup periode sebelumnya dengan saldo awal periode ini belum dirancang.")
            };

            var penghalangAktif = penghalang
                .Count(x => x.State == PeriodChecklistItemState.Evaluated && x.Count > 0);

            var peringatanAktif = peringatan
                .Count(x => x.State == PeriodChecklistItemState.Evaluated && x.Count > 0);

            var belumTersedia = penghalang.Concat(peringatan)
                .Count(x => x.State == PeriodChecklistItemState.NotYetAvailable);

            var isi = new PeriodClosingChecklistResponse
            {
                AccountingPeriodId = periode.Id,
                PeriodName = NamaPeriode(periode),
                PeriodStatus = periode.PeriodStatus,

                // Bukti bahwa angka ini baru saja dihitung, bukan diambil dari simpanan.
                EvaluatedAt = DateTime.UtcNow,

                CanSubmitClosing = penghalangAktif == 0
                                   && periode.PeriodStatus == AccountingPeriodStatus.Open,
                IsComplete = belumTersedia == 0,
                BlockingCount = penghalangAktif,
                WarningCount = peringatanAktif,
                NotYetAvailableCount = belumTersedia,
                Blockers = penghalang,
                Warnings = peringatan
            };

            var pesan = penghalangAktif > 0
                ? $"Periode {isi.PeriodName} belum dapat diajukan: {penghalangAktif} penghalang masih terbuka."
                : isi.IsComplete
                    ? $"Periode {isi.PeriodName} siap diajukan untuk penutupan."
                    : $"Periode {isi.PeriodName} tidak memiliki penghalang yang dapat diperiksa saat ini, "
                      + $"tetapi {belumTersedia} pemeriksaan belum dapat dijalankan.";

            return AccountingServiceResult<PeriodClosingChecklistResponse>.Ok(isi, pesan);
        }

        // ------------------------------------------------------------------
        // Ajukan, setujui, tolak — BE-ACC-P2-006
        // ------------------------------------------------------------------

        /// <summary>
        /// Mengajukan penutupan periode. Periode berpindah <c>Open</c> ke
        /// <c>PendingClosingApproval</c>.
        /// </summary>
        /// <remarks>
        /// Penghalang dihitung ulang <b>di sini</b>, bukan dipercaya dari daftar periksa yang
        /// dilihat pengguna beberapa menit lalu. Perhitungannya memakai
        /// <see cref="HitungJurnalBelumDisahkanAsync(ApplicationDbContext, Guid, CancellationToken)"/>
        /// yang sama persis dengan yang dipakai daftar periksa, supaya keduanya tidak mungkin
        /// berselisih.
        /// </remarks>
        public async Task<AccountingServiceResult<PeriodClosingApprovalResponse>> SubmitClosingAsync(
            Guid accountingPeriodId,
            SubmitPeriodClosingRequest request,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard
                .PeriksaAsync<PeriodClosingApprovalResponse>(_db, ct);
            if (penjaga is not null) return penjaga;

            var periode = await _db.Set<AccAccountingPeriod>()
                .FirstOrDefaultAsync(x => x.Id == accountingPeriodId && !x.IsDelete, ct);

            if (periode is null) return TidakDitemukan();

            if (periode.PeriodStatus != AccountingPeriodStatus.Open)
            {
                return Gagal(
                    StatusCodes.Status409Conflict,
                    $"Periode {NamaPeriode(periode)} tidak dalam keadaan terbuka.");
            }

            // Acceptance (1) — penghalang menahan pengajuan.
            var jurnalBelumDisahkan = await HitungJurnalBelumDisahkanAsync(_db, accountingPeriodId, ct);
            if (jurnalBelumDisahkan > 0)
            {
                return Gagal(
                    StatusCodes.Status409Conflict,
                    $"Masih ada {jurnalBelumDisahkan} jurnal yang belum disahkan.");
            }

            var riwayat = await CatatTindakanAsync(
                periode, PeriodClosingAction.Submitted, actorUserId, request.Note, ct);

            periode.PeriodStatus = AccountingPeriodStatus.PendingClosingApproval;
            periode.ClosingSubmittedBy = actorUserId;
            periode.ClosingSubmittedAt = riwayat.ActionAt;
            periode.UpdateDateTime = DateTime.UtcNow;
            periode.UpdateBy = actorUserId;

            await _db.SaveChangesAsync(ct);

            return AccountingServiceResult<PeriodClosingApprovalResponse>.Ok(
                Petakan(riwayat),
                $"Penutupan periode {NamaPeriode(periode)} berhasil diajukan dan menunggu persetujuan.");
        }

        /// <summary>
        /// Menyetujui penutupan. Periode berpindah <c>PendingClosingApproval</c> ke
        /// <c>SoftClosed</c>.
        /// </summary>
        /// <remarks>
        /// <b>Prinsip empat mata ditegakkan di sini</b> (<c>ACC-DEC-052</c>, meneruskan
        /// <c>ACC-DEC-016</c>): penyetuju tidak boleh orang yang mengajukan. Diperiksa terhadap
        /// <c>ClosingSubmittedBy</c> yang tersimpan, bukan terhadap isian permintaan — kalau
        /// dibaca dari permintaan, siapa pun dapat mengaku bukan pengaju.
        /// </remarks>
        public async Task<AccountingServiceResult<PeriodClosingApprovalResponse>> ApproveClosingAsync(
            Guid accountingPeriodId,
            ApprovePeriodClosingRequest request,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard
                .PeriksaAsync<PeriodClosingApprovalResponse>(_db, ct);
            if (penjaga is not null) return penjaga;

            var periode = await _db.Set<AccAccountingPeriod>()
                .FirstOrDefaultAsync(x => x.Id == accountingPeriodId && !x.IsDelete, ct);

            if (periode is null) return TidakDitemukan();

            if (periode.PeriodStatus != AccountingPeriodStatus.PendingClosingApproval)
            {
                return Gagal(
                    StatusCodes.Status409Conflict,
                    $"Periode {NamaPeriode(periode)} tidak sedang menunggu persetujuan penutupan.");
            }

            // Acceptance (2) — penyetuju bukan pengaju.
            if (periode.ClosingSubmittedBy == actorUserId)
            {
                return Gagal(
                    StatusCodes.Status403Forbidden,
                    "Penutupan tidak dapat disetujui oleh orang yang mengajukannya.");
            }

            var riwayat = await CatatTindakanAsync(
                periode, PeriodClosingAction.Approved, actorUserId, request.Note, ct);

            periode.PeriodStatus = AccountingPeriodStatus.SoftClosed;
            periode.ClosedBy = actorUserId;
            periode.ClosedAt = riwayat.ActionAt;
            periode.UpdateDateTime = DateTime.UtcNow;
            periode.UpdateBy = actorUserId;

            await _db.SaveChangesAsync(ct);

            return AccountingServiceResult<PeriodClosingApprovalResponse>.Ok(
                Petakan(riwayat),
                $"Penutupan periode {NamaPeriode(periode)} disetujui; periode menjadi tutup sementara.");
        }

        /// <summary>
        /// Menolak penutupan. Periode <b>kembali</b> ke <c>Open</c>, dan alasan tertulis wajib.
        /// </summary>
        /// <remarks>
        /// Pengembalian ke <c>Open</c>, bukan ke keadaan menggantung, disengaja: periode yang
        /// ditolak harus dapat diperbaiki lalu diajukan ulang. <c>ClosingSubmittedBy</c>
        /// dikosongkan supaya putaran pengajuan berikutnya dinilai dari nol — kalau tidak,
        /// pengaju lama akan terus terhitung sebagai pengaju walau yang mengajukan ulang orang
        /// lain.
        /// </remarks>
        public async Task<AccountingServiceResult<PeriodClosingApprovalResponse>> RejectClosingAsync(
            Guid accountingPeriodId,
            RejectPeriodClosingRequest request,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard
                .PeriksaAsync<PeriodClosingApprovalResponse>(_db, ct);
            if (penjaga is not null) return penjaga;

            // Acceptance (3) — alasan wajib. Diperiksa lebih dahulu supaya permintaan tanpa
            // alasan ditolak 400 tanpa menyentuh apa pun.
            var alasan = request.Reason?.Trim();
            if (string.IsNullOrWhiteSpace(alasan))
            {
                return Gagal(StatusCodes.Status400BadRequest, "Alasan penolakan wajib diisi.");
            }

            var periode = await _db.Set<AccAccountingPeriod>()
                .FirstOrDefaultAsync(x => x.Id == accountingPeriodId && !x.IsDelete, ct);

            if (periode is null) return TidakDitemukan();

            if (periode.PeriodStatus != AccountingPeriodStatus.PendingClosingApproval)
            {
                return Gagal(
                    StatusCodes.Status409Conflict,
                    $"Periode {NamaPeriode(periode)} tidak sedang menunggu persetujuan penutupan.");
            }

            var riwayat = await CatatTindakanAsync(
                periode, PeriodClosingAction.Rejected, actorUserId, alasan, ct);

            periode.PeriodStatus = AccountingPeriodStatus.Open;
            periode.ClosingSubmittedBy = null;
            periode.ClosingSubmittedAt = null;
            periode.LastReasonNote = alasan;
            periode.UpdateDateTime = DateTime.UtcNow;
            periode.UpdateBy = actorUserId;

            await _db.SaveChangesAsync(ct);

            return AccountingServiceResult<PeriodClosingApprovalResponse>.Ok(
                Petakan(riwayat),
                $"Penutupan periode {NamaPeriode(periode)} ditolak; periode kembali terbuka.");
        }

        /// <summary>
        /// Riwayat penutupan sebuah periode, urut menurut nomor tindakan.
        /// </summary>
        /// <remarks>
        /// Periode yang ditutup sebelum Phase 2 berdiri mengembalikan daftar kosong, dan itu
        /// <b>benar</b> — mereka ditutup ketika aturan persetujuannya memang belum ada
        /// (ACC-STATE-0.2 catatan kompatibilitas). Daftar kosong bukan tanda data janggal.
        /// </remarks>
        public async Task<AccountingServiceResult<List<PeriodClosingApprovalResponse>>> GetClosingHistoryAsync(
            Guid accountingPeriodId,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard
                .PeriksaAsync<List<PeriodClosingApprovalResponse>>(_db, ct);
            if (penjaga is not null) return penjaga;

            var ada = await _db.Set<AccAccountingPeriod>()
                .AnyAsync(x => x.Id == accountingPeriodId && !x.IsDelete, ct);

            if (!ada)
            {
                return AccountingServiceResult<List<PeriodClosingApprovalResponse>>.Fail(
                    StatusCodes.Status404NotFound, "Periode akuntansi tidak ditemukan.");
            }

            var isi = await _db.Set<AccPeriodClosingApproval>()
                .AsNoTracking()
                .Where(x => x.AccountingPeriodId == accountingPeriodId && !x.IsDelete)
                .OrderBy(x => x.ActionSequence)
                .Select(x => new PeriodClosingApprovalResponse
                {
                    Id = x.Id,
                    AccountingPeriodId = x.AccountingPeriodId,
                    ActionSequence = x.ActionSequence,
                    Action = x.Action,
                    ActionBy = x.ActionBy,
                    ActionAt = x.ActionAt,
                    ActionNote = x.ActionNote
                })
                .ToListAsync(ct);

            return AccountingServiceResult<List<PeriodClosingApprovalResponse>>.Ok(
                isi,
                isi.Count == 0
                    ? "Periode ini belum memiliki riwayat penutupan."
                    : $"{isi.Count} riwayat penutupan ditemukan.");
        }

        /// <summary>
        /// Menulis satu baris riwayat penutupan dengan nomor urut berikutnya.
        /// </summary>
        /// <remarks>
        /// Nomor urut dihitung dari baris yang sudah ada, lalu dijaga unique index
        /// <c>(AccountingPeriodId, ActionSequence)</c> di database. Dua pengajuan bersamaan
        /// menghasilkan nomor yang sama, dan yang kedua ditolak database — bukan saling menimpa.
        /// </remarks>
        private async Task<AccPeriodClosingApproval> CatatTindakanAsync(
            AccAccountingPeriod periode,
            PeriodClosingAction tindakan,
            Guid actorUserId,
            string? catatan,
            CancellationToken ct)
        {
            var nomorTerakhir = await _db.Set<AccPeriodClosingApproval>()
                .Where(x => x.AccountingPeriodId == periode.Id && !x.IsDelete)
                .Select(x => (int?)x.ActionSequence)
                .MaxAsync(ct) ?? 0;

            var sekarang = DateTime.UtcNow;

            var baris = new AccPeriodClosingApproval
            {
                Id = Guid.NewGuid(),
                AccountingPeriodId = periode.Id,
                ActionSequence = nomorTerakhir + 1,
                Action = tindakan,
                ActionBy = actorUserId,
                ActionAt = sekarang,
                ActionNote = string.IsNullOrWhiteSpace(catatan) ? null : catatan.Trim(),
                CreateDateTime = sekarang,
                CreateBy = actorUserId
            };

            _db.Set<AccPeriodClosingApproval>().Add(baris);

            return baris;
        }

        private static AccountingServiceResult<PeriodClosingApprovalResponse> TidakDitemukan()
            => AccountingServiceResult<PeriodClosingApprovalResponse>.Fail(
                StatusCodes.Status404NotFound, "Periode akuntansi tidak ditemukan.");

        private static AccountingServiceResult<PeriodClosingApprovalResponse> Gagal(
            int kode, string pesan)
            => AccountingServiceResult<PeriodClosingApprovalResponse>.Fail(kode, pesan);

        private static PeriodClosingApprovalResponse Petakan(AccPeriodClosingApproval x) => new()
        {
            Id = x.Id,
            AccountingPeriodId = x.AccountingPeriodId,
            ActionSequence = x.ActionSequence,
            Action = x.Action,
            ActionBy = x.ActionBy,
            ActionAt = x.ActionAt,
            ActionNote = x.ActionNote
        };

        // ------------------------------------------------------------------
        // Dipakai bersama BE-ACC-P2-006
        // ------------------------------------------------------------------

        /// <summary>
        /// Menghitung jurnal periode ini yang belum disahkan. Dibuat <c>public static</c> supaya
        /// jalur pengajuan penutupan <c>BE-ACC-P2-006</c> memakai perhitungan yang sama persis
        /// dengan yang ditampilkan daftar periksa, tanpa registrasi DI baru.
        /// </summary>
        /// <remarks>
        /// Kalau kedua jalur menghitung sendiri-sendiri, cepat atau lambat keduanya berselisih —
        /// dan pengguna akan melihat daftar periksa bersih tetapi pengajuannya ditolak `409`.
        /// </remarks>
        public static Task<int> HitungJurnalBelumDisahkanAsync(
            ApplicationDbContext db,
            Guid accountingPeriodId,
            CancellationToken ct = default)
            => db.Set<AccJournal>()
                .AsNoTracking()
                .CountAsync(x => !x.IsDelete
                                 && x.AccountingPeriodId == accountingPeriodId
                                 && StatusBelumDisahkan.Contains(x.JournalStatus), ct);

        // ------------------------------------------------------------------
        // Pembantu
        // ------------------------------------------------------------------

        private Task<int> HitungJurnalBelumDisahkanAsync(Guid periodeId, CancellationToken ct)
            => HitungJurnalBelumDisahkanAsync(_db, periodeId, ct);

        /// <remarks>
        /// Hanya jurnal <c>Draft</c> yang mungkin belum seimbang — <c>ACC-DEC-025</c> sudah
        /// melarang jurnal tidak seimbang diajukan maupun disahkan. Karena itu butir ini menjadi
        /// peringatan, bukan penghalang: yang belum seimbang pasti masih draft, dan draft sudah
        /// ditahan penghalang pertama.
        /// </remarks>
        private Task<int> HitungJurnalBelumSeimbangAsync(Guid periodeId, CancellationToken ct)
            => _db.Set<AccJournal>()
                .AsNoTracking()
                .CountAsync(x => !x.IsDelete
                                 && x.AccountingPeriodId == periodeId
                                 && x.JournalStatus == JournalStatus.Draft
                                 && x.TotalDebit != x.TotalCredit, ct);

        private static PeriodClosingBlockerResponse Butir(
            string kode,
            string judul,
            string pesan,
            int jumlah,
            bool menahan) => new()
            {
                Code = kode,
                Title = judul,
                Message = pesan,
                Count = jumlah,
                IsBlocking = menahan && jumlah > 0,
                State = PeriodChecklistItemState.Evaluated
            };

        private static PeriodClosingBlockerResponse ButirBelumTersedia(
            string kode,
            string judul,
            string pesan,
            bool menahan,
            string? alasan = null) => new()
            {
                Code = kode,
                Title = judul,
                Message = pesan,

                // Nol, dan sengaja TIDAK menahan apa pun. Butir yang tidak dapat diperiksa tidak
                // boleh menahan penutupan atas dasar tebakan; yang wajib dilakukan adalah
                // menyatakannya terang-terangan lewat State dan IsComplete.
                Count = 0,
                IsBlocking = false,
                State = PeriodChecklistItemState.NotYetAvailable,
                UnavailableReason = alasan ?? MenungguKotakMasukKejadian
            };

        private static string NamaPeriode(AccAccountingPeriod periode)
            => $"{NamaBulan[periode.PeriodMonth - 1]} "
               + periode.FiscalYear.ToString(CultureInfo.InvariantCulture);
    }
}
