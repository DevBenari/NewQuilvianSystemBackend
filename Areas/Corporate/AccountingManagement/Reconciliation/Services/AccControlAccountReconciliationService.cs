using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Services;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.Reconciliation.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.Reconciliation.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.Reconciliation.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.Reconciliation.Services
{
    /// <summary>
    /// Sisi buku besar dari rekonsiliasi control account (<c>ACC-DEC-066</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Control account adalah akun yang saldonya <b>wajib sama</b> dengan catatan rincinya —
    /// Kas Kasir dengan setoran kasir, Piutang dengan rincian piutang pasien. Rekonsiliasi
    /// membandingkan keduanya, dan service ini menyediakan separuhnya: angka dari buku besar.
    /// </para>
    /// <para>
    /// Separuh lainnya, saldo subledger, adalah <c>BE-ACC-P2-014</c> yang masih terblokir
    /// <c>DEC-ACC-P2-011</c> — belum diputuskan dari mana Accounting memperolehnya, mengingat
    /// <c>ACC-DEC-061</c> melarangnya membaca tabel Finance maupun Billing. Sisi buku besar
    /// sepenuhnya milik Accounting dan tidak menunggu keputusan itu.
    /// </para>
    /// </remarks>
    public class AccControlAccountReconciliationService
    {
        private readonly ApplicationDbContext _db;

        public AccControlAccountReconciliationService(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Menghitung saldo buku besar seluruh control account sebuah badan hukum.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Hanya baris jurnal berstatus <c>Posted</c> yang dihitung.</b> Jurnal `Draft`,
        /// `PendingApproval`, dan `Approved` belum menjadi transaksi. Ikut menghitungnya
        /// menghasilkan saldo yang <b>tidak akan pernah cocok</b> dengan subledger, dan
        /// selisihnya akan disalahartikan sebagai cacat data — orang akan mencari kesalahan di
        /// tempat yang tidak ada kesalahannya.
        /// </para>
        /// <para>
        /// <b>Kenapa satu query pengelompokan, bukan panggilan
        /// <c>AccChartOfAccountService.HitungSaldoAsync</c> per akun.</b> Kartu roadmap menyebut
        /// pemakaian ulang fungsi itu apa adanya, dan angkanya memang wajib sama persis — tetapi
        /// memanggilnya per akun berarti satu query per control account, dan laporan ini justru
        /// dibuat untuk membaca banyak akun sekaligus. Fungsi itu juga tidak menerima batas
        /// tanggal, sedangkan acceptance (3) menuntutnya. Karena itu perhitungannya ditulis
        /// sebagai satu query pengelompokan dengan rumus yang sama, dan kesamaan angkanya
        /// <b>diuji</b> terhadap <c>HitungSaldoAsync</c>.
        /// </para>
        /// </remarks>
        public async Task<AccountingServiceResult<ControlAccountBalanceReportResponse>> GetGlBalancesAsync(
            ControlAccountBalanceQuery query,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard
                .PeriksaAsync<ControlAccountBalanceReportResponse>(_db, ct);
            if (penjaga is not null) return penjaga;

            if (query.LegalEntityId == Guid.Empty)
            {
                return AccountingServiceResult<ControlAccountBalanceReportResponse>.Fail(
                    StatusCodes.Status400BadRequest, "Badan hukum wajib disebutkan.");
            }

            // Acceptance (1) dan (3) sisi akun — hanya control account, hanya badan hukum ini.
            var akun = await _db.Set<AccChartOfAccount>()
                .AsNoTracking()
                .Where(x => !x.IsDelete
                            && x.IsControlAccount
                            && x.LegalEntityId == query.LegalEntityId)
                .OrderBy(x => x.AccountCode)
                .Select(x => new
                {
                    x.Id,
                    x.AccountCode,
                    x.AccountName,
                    x.AccountType,
                    x.NormalBalance
                })
                .ToListAsync(ct);

            if (akun.Count == 0)
            {
                return AccountingServiceResult<ControlAccountBalanceReportResponse>.Ok(
                    new ControlAccountBalanceReportResponse
                    {
                        LegalEntityId = query.LegalEntityId,
                        AsOfDate = query.AsOfDate,
                        EvaluatedAt = DateTime.UtcNow
                    },
                    "Belum ada akun yang ditandai sebagai control account pada badan hukum ini.");
            }

            var idAkun = akun.Select(x => x.Id).ToList();

            var perAkun = await HitungSaldoBukuBesarAsync(_db, idAkun, query.AsOfDate, ct);

            var isi = new ControlAccountBalanceReportResponse
            {
                LegalEntityId = query.LegalEntityId,
                AsOfDate = query.AsOfDate,
                EvaluatedAt = DateTime.UtcNow,
                AccountCount = akun.Count
            };

            foreach (var a in akun)
            {
                perAkun.TryGetValue(a.Id, out var angka);

                var debit = angka?.TotalDebit ?? 0m;
                var kredit = angka?.TotalCredit ?? 0m;
                var saldo = debit - kredit;

                isi.Accounts.Add(new ControlAccountBalanceResponse
                {
                    AccountId = a.Id,
                    AccountCode = a.AccountCode,
                    AccountName = a.AccountName,
                    AccountType = a.AccountType,
                    NormalBalance = a.NormalBalance,
                    TotalDebit = debit,
                    TotalCredit = kredit,
                    Balance = saldo,
                    BalanceInNormalBalance = a.NormalBalance == NormalBalance.Credit ? -saldo : saldo,
                    PostedLineCount = angka?.LineCount ?? 0
                });

                isi.TotalDebit += debit;
                isi.TotalCredit += kredit;
            }

            return AccountingServiceResult<ControlAccountBalanceReportResponse>.Ok(
                isi, $"Saldo {akun.Count} control account berhasil dihitung.");
        }

        public async Task<AccountingServiceResult<SubledgerComparisonReportResponse>> GetSubledgerComparisonAsync(
            SubledgerComparisonQuery query,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard
                .PeriksaAsync<SubledgerComparisonReportResponse>(_db, ct);
            if (penjaga is not null) return penjaga;

            if (query.AccountingPeriodId == Guid.Empty)
            {
                return AccountingServiceResult<SubledgerComparisonReportResponse>.Fail(
                    StatusCodes.Status400BadRequest, "Periode akuntansi wajib disebutkan.");
            }

            var periode = await _db.Set<AccAccountingPeriod>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == query.AccountingPeriodId && !x.IsDelete, ct);

            if (periode is null)
            {
                return AccountingServiceResult<SubledgerComparisonReportResponse>.Fail(
                    StatusCodes.Status404NotFound, "Periode akuntansi tidak ditemukan.");
            }

            var isi = await HitungRekonsiliasiSubledgerAsync(_db, periode, ct);

            var pesan = isi.AccountCount == 0
                ? "Belum ada akun yang ditandai sebagai control account pada badan hukum ini."
                : $"Rekonsiliasi saldo subledger periode {isi.PeriodName} berhasil dihitung.";

            return AccountingServiceResult<SubledgerComparisonReportResponse>.Ok(isi, pesan);
        }

        public static async Task<SubledgerComparisonReportResponse> HitungRekonsiliasiSubledgerAsync(
            ApplicationDbContext db,
            AccAccountingPeriod periode,
            CancellationToken ct = default)
        {
            var badanHukum = periode.LegalEntityId;
            var akhirPeriode = periode.EndDate.Date;

            var titikMulai = await db.Set<AccAccountingPeriod>()
                .AsNoTracking()
                .Where(p => !p.IsDelete
                            && p.LegalEntityId == badanHukum
                            && db.Set<AccSubledgerBalance>().Any(s => !s.IsDelete
                                                                      && s.LegalEntityId == badanHukum
                                                                      && s.AccountingPeriodId == p.Id))
                .OrderBy(p => p.StartDate)
                .Select(p => new { p.StartDate, p.PeriodCode })
                .FirstOrDefaultAsync(ct);

            var berlaku = titikMulai is not null && periode.StartDate.Date >= titikMulai.StartDate.Date;

            var saldoSubledger = await db.Set<AccSubledgerBalance>()
                .AsNoTracking()
                .Where(x => !x.IsDelete
                            && x.LegalEntityId == badanHukum
                            && x.AccountingPeriodId == periode.Id)
                .Select(x => new
                {
                    x.ChartOfAccountId,
                    x.Balance,
                    x.AsOfDate,
                    x.SourceVersionNumber,
                    x.AccountingEventId,
                    EventNumber = x.AccountingEvent != null ? x.AccountingEvent.EventNumber : null,
                    DicatatPada = x.UpdateDateTime ?? x.CreateDateTime
                })
                .ToListAsync(ct);

            var saldoPerAkun = saldoSubledger
                .GroupBy(x => x.ChartOfAccountId)
                .ToDictionary(g => g.Key, g => g.First());

            var idAkunBersaldo = saldoPerAkun.Keys.ToList();

            var akun = await db.Set<AccChartOfAccount>()
                .AsNoTracking()
                .Where(x => !x.IsDelete
                            && x.LegalEntityId == badanHukum
                            && (x.IsControlAccount || idAkunBersaldo.Contains(x.Id)))
                .OrderBy(x => x.AccountCode)
                .Select(x => new
                {
                    x.Id,
                    x.AccountCode,
                    x.AccountName,
                    x.AccountType,
                    x.NormalBalance,
                    x.IsActive,
                    x.IsPostable,
                    x.IsControlAccount
                })
                .ToListAsync(ct);

            var bukuBesar = await HitungSaldoBukuBesarAsync(
                db, akun.Select(x => x.Id).ToList(), akhirPeriode, ct);

            var isi = new SubledgerComparisonReportResponse
            {
                LegalEntityId = badanHukum,
                AccountingPeriodId = periode.Id,
                PeriodCode = periode.PeriodCode,
                PeriodName = AccAccountingPeriodService.NamaPeriode(periode),
                PeriodStatus = periode.PeriodStatus,
                PeriodEndDate = akhirPeriode,
                EvaluatedAt = DateTime.UtcNow,
                StartPeriodCode = titikMulai?.PeriodCode,
                AccountCount = akun.Count
            };

            var menahanBelumDiterima = 0;
            var menahanCutOff = 0;
            var menahanBerselisih = 0;

            foreach (var a in akun)
            {
                bukuBesar.TryGetValue(a.Id, out var angka);

                var saldoMentah = (angka?.TotalDebit ?? 0m) - (angka?.TotalCredit ?? 0m);
                var saldoBukuBesar = a.NormalBalance == NormalBalance.Credit ? -saldoMentah : saldoMentah;

                var wajib = a.IsControlAccount
                            && a.IsPostable
                            && (a.IsActive || saldoBukuBesar != 0m);

                saldoPerAkun.TryGetValue(a.Id, out var saldo);

                SubledgerReconciliationItemStatus keadaan;
                if (saldo is null)
                {
                    keadaan = SubledgerReconciliationItemStatus.BelumDiterima;
                }
                else if (saldo.AsOfDate.Date != akhirPeriode)
                {
                    keadaan = SubledgerReconciliationItemStatus.CutOffBukanAkhirPeriode;
                }
                else if (saldo.Balance == saldoBukuBesar)
                {
                    keadaan = SubledgerReconciliationItemStatus.Cocok;
                }
                else
                {
                    keadaan = SubledgerReconciliationItemStatus.Berselisih;
                }

                var dapatDibandingkan = keadaan == SubledgerReconciliationItemStatus.Cocok
                                        || keadaan == SubledgerReconciliationItemStatus.Berselisih;

                var menahan = berlaku && (wajib
                    ? keadaan != SubledgerReconciliationItemStatus.Cocok
                    : keadaan == SubledgerReconciliationItemStatus.Berselisih);

                isi.Accounts.Add(new SubledgerComparisonAccountResponse
                {
                    AccountId = a.Id,
                    AccountCode = a.AccountCode,
                    AccountName = a.AccountName,
                    AccountType = a.AccountType,
                    NormalBalance = a.NormalBalance,
                    IsActive = a.IsActive,
                    IsPostable = a.IsPostable,
                    IsRequired = wajib,
                    GlBalance = saldoBukuBesar,
                    PostedLineCount = angka?.LineCount ?? 0,
                    SubledgerBalance = saldo?.Balance,
                    SubledgerAsOfDate = saldo?.AsOfDate.Date,
                    SubledgerVersionNumber = saldo?.SourceVersionNumber,
                    SubledgerAccountingEventId = saldo?.AccountingEventId,
                    SubledgerEventNumber = saldo?.EventNumber,
                    SubledgerRecordedAt = saldo?.DicatatPada,
                    Difference = dapatDibandingkan ? saldoBukuBesar - saldo!.Balance : null,
                    ItemStatus = keadaan,
                    IsBlocking = menahan
                });

                if (wajib) isi.RequiredAccountCount++;

                switch (keadaan)
                {
                    case SubledgerReconciliationItemStatus.Cocok:
                        isi.MatchedCount++;
                        break;
                    case SubledgerReconciliationItemStatus.Berselisih:
                        isi.DifferenceCount++;
                        if (menahan) menahanBerselisih++;
                        break;
                    case SubledgerReconciliationItemStatus.BelumDiterima:
                        isi.NotReceivedCount++;
                        if (menahan) menahanBelumDiterima++;
                        break;
                    case SubledgerReconciliationItemStatus.CutOffBukanAkhirPeriode:
                        isi.CutOffMismatchCount++;
                        if (menahan) menahanCutOff++;
                        break;
                }

                if (menahan) isi.BlockingCount++;
            }

            isi.ReconciliationState = !berlaku
                ? SubledgerReconciliationState.BelumBerlaku
                : isi.BlockingCount == 0
                    ? SubledgerReconciliationState.Bersih
                    : SubledgerReconciliationState.BelumBersih;

            isi.StateMessage = isi.ReconciliationState switch
            {
                SubledgerReconciliationState.BelumBerlaku =>
                    "Belum dapat diperiksa: rekonsiliasi saldo subledger belum berlaku untuk periode ini.",
                SubledgerReconciliationState.Bersih =>
                    "Seluruh control account cocok dengan saldo subledger.",
                _ => RincianBelumBersih(menahanBelumDiterima, menahanCutOff, menahanBerselisih)
            };

            return isi;
        }

        public static string AlasanRekonsiliasiBelumBerlaku(SubledgerComparisonReportResponse rekonsiliasi)
            => string.IsNullOrWhiteSpace(rekonsiliasi.StartPeriodCode)
                ? "Finance belum pernah mengirim saldo subledger untuk badan hukum ini."
                : $"Rekonsiliasi saldo subledger berlaku mulai periode {rekonsiliasi.StartPeriodCode}.";

        private static string RincianBelumBersih(int belumDiterima, int cutOff, int berselisih)
        {
            var bagian = new List<(int Jumlah, string Keterangan)>
            {
                (belumDiterima, "belum menerima saldo subledger"),
                (cutOff, "saldonya bertanggal cut-off bukan akhir periode"),
                (berselisih, "berselisih")
            }
            .Where(x => x.Jumlah > 0)
            .Select((x, urutan) => urutan == 0
                ? $"{x.Jumlah} control account {x.Keterangan}"
                : $"{x.Jumlah} {x.Keterangan}");

            return string.Join(", ", bagian) + ".";
        }

        private static async Task<Dictionary<Guid, RingkasanBukuBesar>> HitungSaldoBukuBesarAsync(
            ApplicationDbContext db,
            List<Guid> idAkun,
            DateTime? batasTanggal,
            CancellationToken ct)
        {
            if (idAkun.Count == 0) return new Dictionary<Guid, RingkasanBukuBesar>();

            // Acceptance (2) dan (3) sisi baris — Posted saja, dan dibatasi tanggal bila diminta.
            var barisQuery = db.Set<AccJournalLine>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && idAkun.Contains(x.AccountId));

            var jurnalQuery = db.Set<AccJournal>()
                .Where(j => !j.IsDelete && j.JournalStatus == JournalStatus.Posted);

            if (batasTanggal.HasValue)
            {
                // Termasuk tanggalnya sendiri — "saldo per 30 September" memuat 30 September.
                var batas = batasTanggal.Value.Date;
                jurnalQuery = jurnalQuery.Where(j => j.AccountingDate <= batas);
            }

            var ringkasan = await barisQuery
                .Where(x => jurnalQuery.Any(j => j.Id == x.JournalId))
                .GroupBy(x => x.AccountId)
                .Select(g => new
                {
                    AccountId = g.Key,
                    TotalDebit = g.Sum(x => x.DebitAmount),
                    TotalCredit = g.Sum(x => x.CreditAmount),
                    LineCount = g.Count()
                })
                .ToListAsync(ct);

            return ringkasan.ToDictionary(
                x => x.AccountId,
                x => new RingkasanBukuBesar(x.TotalDebit, x.TotalCredit, x.LineCount));
        }

        private sealed record RingkasanBukuBesar(decimal TotalDebit, decimal TotalCredit, int LineCount);
    }
}
