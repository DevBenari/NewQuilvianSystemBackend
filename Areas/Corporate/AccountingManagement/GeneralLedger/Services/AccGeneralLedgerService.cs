using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.GeneralLedger.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using System.Globalization;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.GeneralLedger.Services
{
    /// <summary>
    /// Buku besar, saldo per akun, dan neraca saldo. Cakupan `BE-ACC-012`.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Tidak ada tabel buku besar.</b> Seluruh angka dihitung dari <see cref="AccJournalLine"/>
    /// sebagai sumber tunggal. Menyalin saldo ke tabel tersendiri berarti menciptakan sumber
    /// kebenaran kedua yang dapat menyimpang diam-diam dari jurnalnya.
    /// </para>
    /// <para>
    /// <b>Hanya jurnal berstatus <c>Posted</c>.</b> Draft, menunggu persetujuan, disetujui, dan
    /// ditolak tidak pernah ikut terhitung. Ini acceptance (2) dan sekaligus butir yang paling
    /// mudah terlewat: laporan akan tetap terlihat wajar walau salah, karena angkanya masuk akal.
    /// Penyaringnya karena itu dipusatkan pada satu tempat — <see cref="BarisDisahkan"/> — supaya
    /// tidak ada satu pun query yang lupa menerapkannya.
    /// </para>
    /// <para>
    /// Seluruh pembacaan memakai <c>AsNoTracking</c>: tidak ada satu pun jalur di service ini yang
    /// menulis.
    /// </para>
    /// </remarks>
    public class AccGeneralLedgerService
    {
        private readonly ApplicationDbContext _db;

        public AccGeneralLedgerService(ApplicationDbContext db)
        {
            _db = db;
        }

        private static readonly string[] NamaBulan =
        {
            "Januari", "Februari", "Maret", "April", "Mei", "Juni",
            "Juli", "Agustus", "September", "Oktober", "November", "Desember"
        };

        /// <summary>
        /// Baris jurnal yang sah masuk laporan: jurnalnya disahkan, milik badan hukum itu, dan
        /// baik baris maupun jurnalnya belum dihapus.
        /// </summary>
        /// <remarks>
        /// Satu-satunya pintu masuk data bagi seluruh endpoint di service ini. Acceptance (2) dan
        /// (5) keduanya ditegakkan di sini, sekali, bukan diulang di tiap query.
        /// </remarks>
        private IQueryable<AccJournalLine> BarisDisahkan(Guid legalEntityId)
        {
            return _db.Set<AccJournalLine>()
                .AsNoTracking()
                .Where(x => !x.IsDelete
                            && _db.Set<AccJournal>().Any(j => j.Id == x.JournalId
                                                              && !j.IsDelete
                                                              && j.LegalEntityId == legalEntityId
                                                              && j.JournalStatus == JournalStatus.Posted));
        }

        // ------------------------------------------------------------------
        // Mutasi buku besar
        // ------------------------------------------------------------------

        /// <remarks>
        /// <para>
        /// <b>Urutannya adalah bagian dari kebenaran, bukan kosmetik.</b> Saldo berjalan hanya
        /// bermakna bila urutannya tidak pernah berubah antar pemanggilan, sehingga urutannya
        /// dikunci pada <c>AccountingDate</c>, lalu <c>JournalNumber</c>, lalu <c>LineNumber</c> —
        /// acceptance (3) dan (4). Ketiganya bersama-sama unik, karena
        /// <c>(LegalEntityId, JournalNumber)</c> unik dan <c>(JournalId, LineNumber)</c> unik.
        /// Tidak ada field baru yang dikarang untuk pengurutan.
        /// </para>
        /// <para>
        /// <b>Saldo berjalan tetap benar lintas halaman.</b> Halaman kedua tidak dimulai dari nol:
        /// saldo dibuka dari mutasi sebelum rentang tanggal, ditambah seluruh baris yang dilewati
        /// halaman-halaman sebelumnya. Keduanya dihitung di database, bukan dengan menarik seluruh
        /// baris ke memori.
        /// </para>
        /// </remarks>
        public async Task<AccountingServiceResult<PagedResult<LedgerMovementResponse>>> GetMovementsAsync(
            LedgerMovementQuery query,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard
                .PeriksaAsync<PagedResult<LedgerMovementResponse>>(_db, ct);
            if (penjaga is not null) return penjaga;

            var rentang = PeriksaRentang<PagedResult<LedgerMovementResponse>>(query.DateFrom, query.DateTo);
            if (rentang is not null) return rentang;

            var akun = await _db.Set<AccChartOfAccount>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == query.AccountId && !x.IsDelete, ct);

            if (akun is null)
            {
                return AccountingServiceResult<PagedResult<LedgerMovementResponse>>.Fail(
                    StatusCodes.Status404NotFound, "Akun tidak ditemukan.");
            }

            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize is < 1 or > 200 ? 25 : query.PageSize;

            var dasar = BarisDisahkan(query.LegalEntityId).Where(x => x.AccountId == query.AccountId);

            // Saldo pembuka: seluruh mutasi sebelum rentang yang diminta.
            var saldoPembuka = 0m;

            if (query.DateFrom.HasValue)
            {
                var dari = query.DateFrom.Value.Date;

                saldoPembuka = await dasar
                    .Where(x => _db.Set<AccJournal>()
                        .Any(j => j.Id == x.JournalId && j.AccountingDate < dari))
                    .SumAsync(x => x.DebitAmount - x.CreditAmount, ct);
            }

            var dalamRentang = dasar;

            if (query.DateFrom.HasValue)
            {
                var dari = query.DateFrom.Value.Date;
                dalamRentang = dalamRentang.Where(x => _db.Set<AccJournal>()
                    .Any(j => j.Id == x.JournalId && j.AccountingDate >= dari));
            }

            if (query.DateTo.HasValue)
            {
                var sampai = query.DateTo.Value.Date;
                dalamRentang = dalamRentang.Where(x => _db.Set<AccJournal>()
                    .Any(j => j.Id == x.JournalId && j.AccountingDate <= sampai));
            }

            // Diproyeksikan lebih dahulu supaya pengurutan memakai kolom jurnal tanpa menarik
            // seluruh entity, dan supaya urutannya sama persis di setiap pemanggilan.
            var terproyeksi = dalamRentang.Select(x => new
            {
                x.Id,
                Jurnal = _db.Set<AccJournal>().First(j => j.Id == x.JournalId),
                x.LineNumber,
                x.Description,
                x.DebitAmount,
                x.CreditAmount
            });

            var terurut = terproyeksi
                .OrderBy(x => x.Jurnal.AccountingDate)
                .ThenBy(x => x.Jurnal.JournalNumber)
                .ThenBy(x => x.LineNumber);

            var total = await terurut.CountAsync(ct);
            var dilewati = (pageNumber - 1) * pageSize;

            // Saldo seluruh baris yang dilewati halaman sebelumnya — dihitung di database.
            var saldoSebelumHalaman = dilewati == 0
                ? 0m
                : await terurut.Take(dilewati).SumAsync(x => x.DebitAmount - x.CreditAmount, ct);

            var halaman = await terurut
                .Skip(dilewati)
                .Take(pageSize)
                .Select(x => new LedgerMovementResponse
                {
                    AccountingDate = x.Jurnal.AccountingDate,
                    JournalNumber = x.Jurnal.JournalNumber,
                    LineNumber = x.LineNumber,
                    Description = x.Description ?? x.Jurnal.Description,
                    DebitAmount = x.DebitAmount,
                    CreditAmount = x.CreditAmount
                })
                .ToListAsync(ct);

            var berjalan = saldoPembuka + saldoSebelumHalaman;

            foreach (var baris in halaman)
            {
                berjalan += baris.DebitAmount - baris.CreditAmount;
                baris.RunningBalance = berjalan;
            }

            var hasil = new PagedResult<LedgerMovementResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = total,
                TotalPage = pageSize == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize),
                Items = halaman
            };

            return AccountingServiceResult<PagedResult<LedgerMovementResponse>>.Ok(
                hasil, $"Mutasi buku besar akun {akun.AccountCode} berhasil diambil.");
        }

        // ------------------------------------------------------------------
        // Neraca saldo
        // ------------------------------------------------------------------

        /// <remarks>
        /// Acceptance (1). Keseimbangan neraca saldo bukan sesuatu yang dipaksakan di sini — ia
        /// <b>akibat</b> dari setiap jurnal yang disahkan wajib seimbang. Karena itu
        /// <c>IsBalanced</c> dihitung apa adanya dan tidak pernah dibulatkan agar terlihat
        /// seimbang: bila ia <c>false</c>, yang rusak adalah datanya, dan laporan wajib
        /// mengatakannya.
        /// </remarks>
        public async Task<AccountingServiceResult<TrialBalanceResponse>> GetTrialBalanceAsync(
            TrialBalanceQuery query,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard.PeriksaAsync<TrialBalanceResponse>(_db, ct);
            if (penjaga is not null) return penjaga;

            var kode = query.PeriodCode?.Trim() ?? string.Empty;

            var periode = await _db.Set<AccAccountingPeriod>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => !x.IsDelete
                                          && x.LegalEntityId == query.LegalEntityId
                                          && x.PeriodCode == kode, ct);

            if (periode is null)
            {
                return AccountingServiceResult<TrialBalanceResponse>.Fail(
                    StatusCodes.Status404NotFound, "Periode akuntansi tidak ditemukan.");
            }

            var dasar = BarisDisahkan(query.LegalEntityId);

            // Mutasi di dalam periode, dikelompokkan per akun.
            var mutasi = await dasar
                .Where(x => _db.Set<AccJournal>().Any(j => j.Id == x.JournalId
                                                          && j.AccountingDate >= periode.StartDate
                                                          && j.AccountingDate <= periode.EndDate))
                .GroupBy(x => x.AccountId)
                .Select(g => new
                {
                    AccountId = g.Key,
                    Debit = g.Sum(x => x.DebitAmount),
                    Kredit = g.Sum(x => x.CreditAmount)
                })
                .ToListAsync(ct);

            // Saldo pembuka: seluruh mutasi sebelum periode dimulai.
            var pembuka = await dasar
                .Where(x => _db.Set<AccJournal>().Any(j => j.Id == x.JournalId
                                                          && j.AccountingDate < periode.StartDate))
                .GroupBy(x => x.AccountId)
                .Select(g => new
                {
                    AccountId = g.Key,
                    Saldo = g.Sum(x => x.DebitAmount - x.CreditAmount)
                })
                .ToListAsync(ct);

            var petaPembuka = pembuka.ToDictionary(x => x.AccountId, x => x.Saldo);
            var petaMutasi = mutasi.ToDictionary(x => x.AccountId, x => (x.Debit, x.Kredit));

            var idAkun = petaPembuka.Keys.Union(petaMutasi.Keys).ToList();

            var akun = await _db.Set<AccChartOfAccount>()
                .AsNoTracking()
                .Where(x => idAkun.Contains(x.Id) && !x.IsDelete)
                .OrderBy(x => x.AccountCode)
                .Select(x => new { x.Id, x.AccountCode, x.AccountName })
                .ToListAsync(ct);

            var baris = new List<TrialBalanceRowResponse>();

            foreach (var a in akun)
            {
                petaPembuka.TryGetValue(a.Id, out var saldoPembuka);
                petaMutasi.TryGetValue(a.Id, out var m);

                baris.Add(new TrialBalanceRowResponse
                {
                    AccountId = a.Id,
                    AccountCode = a.AccountCode,
                    AccountName = a.AccountName,
                    OpeningBalance = saldoPembuka,
                    TotalDebit = m.Item1,
                    TotalCredit = m.Item2,
                    ClosingBalance = saldoPembuka + m.Item1 - m.Item2
                });
            }

            var totalDebit = baris.Sum(x => x.TotalDebit);
            var totalKredit = baris.Sum(x => x.TotalCredit);

            var hasil = new TrialBalanceResponse
            {
                PeriodCode = periode.PeriodCode,
                PeriodName = NamaPeriode(periode),
                Rows = baris,
                TotalDebit = totalDebit,
                TotalCredit = totalKredit,
                IsBalanced = totalDebit == totalKredit
            };

            return AccountingServiceResult<TrialBalanceResponse>.Ok(
                hasil, $"Neraca saldo {hasil.PeriodName} berhasil diambil.");
        }

        // ------------------------------------------------------------------
        // Saldo satu akun
        // ------------------------------------------------------------------

        public async Task<AccountingServiceResult<AccountBalanceResponse>> GetAccountBalanceAsync(
            Guid accountId,
            string? periodCode,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard.PeriksaAsync<AccountBalanceResponse>(_db, ct);
            if (penjaga is not null) return penjaga;

            var akun = await _db.Set<AccChartOfAccount>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == accountId && !x.IsDelete, ct);

            if (akun is null)
            {
                return AccountingServiceResult<AccountBalanceResponse>.Fail(
                    StatusCodes.Status404NotFound, "Akun tidak ditemukan.");
            }

            var kode = periodCode?.Trim() ?? string.Empty;

            // Badan hukum diturunkan dari akunnya, tidak dikirim pemanggil — dengan begitu saldo
            // dua badan hukum tidak mungkin tercampur lewat parameter yang keliru.
            var periode = await _db.Set<AccAccountingPeriod>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => !x.IsDelete
                                          && x.LegalEntityId == akun.LegalEntityId
                                          && x.PeriodCode == kode, ct);

            if (periode is null)
            {
                return AccountingServiceResult<AccountBalanceResponse>.Fail(
                    StatusCodes.Status404NotFound, "Periode akuntansi tidak ditemukan.");
            }

            var dasar = BarisDisahkan(akun.LegalEntityId).Where(x => x.AccountId == accountId);

            var saldoPembuka = await dasar
                .Where(x => _db.Set<AccJournal>().Any(j => j.Id == x.JournalId
                                                          && j.AccountingDate < periode.StartDate))
                .SumAsync(x => x.DebitAmount - x.CreditAmount, ct);

            var dalamPeriode = dasar.Where(x => _db.Set<AccJournal>()
                .Any(j => j.Id == x.JournalId
                          && j.AccountingDate >= periode.StartDate
                          && j.AccountingDate <= periode.EndDate));

            var debit = await dalamPeriode.SumAsync(x => x.DebitAmount, ct);
            var kredit = await dalamPeriode.SumAsync(x => x.CreditAmount, ct);

            var hasil = new AccountBalanceResponse
            {
                AccountId = akun.Id,
                AccountCode = akun.AccountCode,
                AccountName = akun.AccountName,
                PeriodCode = periode.PeriodCode,
                PeriodName = NamaPeriode(periode),
                OpeningBalance = saldoPembuka,
                TotalDebit = debit,
                TotalCredit = kredit,
                ClosingBalance = saldoPembuka + debit - kredit
            };

            return AccountingServiceResult<AccountBalanceResponse>.Ok(
                hasil, $"Saldo akun {akun.AccountCode} berhasil diambil.");
        }

        // ------------------------------------------------------------------
        // Pembantu
        // ------------------------------------------------------------------

        private static AccountingServiceResult<T>? PeriksaRentang<T>(DateTime? dari, DateTime? sampai)
        {
            if (dari.HasValue && sampai.HasValue && sampai.Value.Date < dari.Value.Date)
            {
                return AccountingServiceResult<T>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Tanggal akhir tidak boleh mendahului tanggal mulai.");
            }

            return null;
        }

        private static string NamaPeriode(AccAccountingPeriod periode)
            => $"{NamaBulan[periode.PeriodMonth - 1]} "
             + periode.FiscalYear.ToString(CultureInfo.InvariantCulture);
    }
}
