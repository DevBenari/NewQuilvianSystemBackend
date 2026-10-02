using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.Reconciliation.DTOs;
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

            // Acceptance (2) dan (3) sisi baris — Posted saja, dan dibatasi tanggal bila diminta.
            var barisQuery = _db.Set<AccJournalLine>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && idAkun.Contains(x.AccountId));

            var jurnalQuery = _db.Set<AccJournal>()
                .Where(j => !j.IsDelete && j.JournalStatus == JournalStatus.Posted);

            if (query.AsOfDate.HasValue)
            {
                // Termasuk tanggalnya sendiri — "saldo per 30 September" memuat 30 September.
                var batas = query.AsOfDate.Value.Date;
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

            var perAkun = ringkasan.ToDictionary(x => x.AccountId);

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
    }
}
