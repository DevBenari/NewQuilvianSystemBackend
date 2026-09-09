using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Services
{
    /// <summary>
    /// Daur hidup periode akuntansi, `ACC-STATE-0.1` bagian 2.
    /// </summary>
    public class AccAccountingPeriodService
    {
        private readonly ApplicationDbContext _db;

        public AccAccountingPeriodService(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>Nama bulan Indonesia, dipakai menyusun nama periode yang dibaca pengguna.</summary>
        private static readonly string[] NamaBulan =
        {
            "Januari", "Februari", "Maret", "April", "Mei", "Juni",
            "Juli", "Agustus", "September", "Oktober", "November", "Desember"
        };

        private const int TahunBukuMinimum = 2000;
        private const int TahunBukuMaksimum = 2999;

        // ------------------------------------------------------------------
        // Baca
        // ------------------------------------------------------------------

        public async Task<AccountingServiceResult<PagedResult<AccountingPeriodResponse>>> GetPagedAsync(
            AccountingPeriodPagedQuery query,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard
                .PeriksaAsync<PagedResult<AccountingPeriodResponse>>(_db, ct);
            if (penjaga is not null) return penjaga;

            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize is < 1 or > 200 ? 25 : query.PageSize;

            IQueryable<AccAccountingPeriod> q = _db.Set<AccAccountingPeriod>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (query.LegalEntityId.HasValue)
                q = q.Where(x => x.LegalEntityId == query.LegalEntityId.Value);

            if (query.FiscalYear.HasValue)
                q = q.Where(x => x.FiscalYear == query.FiscalYear.Value);

            if (query.PeriodStatus.HasValue)
                q = q.Where(x => x.PeriodStatus == query.PeriodStatus.Value);

            var menurun = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            q = menurun
                ? q.OrderByDescending(x => x.FiscalYear).ThenByDescending(x => x.PeriodMonth)
                : q.OrderBy(x => x.FiscalYear).ThenBy(x => x.PeriodMonth);

            var total = await q.CountAsync(ct);

            var baris = await q
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return AccountingServiceResult<PagedResult<AccountingPeriodResponse>>.Ok(
                new PagedResult<AccountingPeriodResponse>
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = total,
                    TotalPage = (int)Math.Ceiling(total / (double)pageSize),
                    Items = baris.Select(Petakan).ToList()
                },
                "Daftar periode akuntansi berhasil diambil.");
        }

        /// <remarks>
        /// "Sedang berjalan" ditentukan dari tanggal hari ini yang jatuh di antara
        /// <c>StartDate</c> dan <c>EndDate</c> — bukan dari statusnya. Periode berjalan yang sudah
        /// ditutup tetap periode berjalan; layar perlu tahu itu supaya dapat menjelaskan kenapa
        /// jurnal ditolak.
        /// </remarks>
        public async Task<AccountingServiceResult<AccountingPeriodResponse>> GetCurrentAsync(
            Guid legalEntityId,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard.PeriksaAsync<AccountingPeriodResponse>(_db, ct);
            if (penjaga is not null) return penjaga;

            var hariIni = DateTime.UtcNow.Date;

            var periode = await _db.Set<AccAccountingPeriod>()
                .AsNoTracking()
                .Where(x => !x.IsDelete
                            && x.LegalEntityId == legalEntityId
                            && x.StartDate <= hariIni
                            && x.EndDate >= hariIni)
                .FirstOrDefaultAsync(ct);

            if (periode is null)
            {
                return AccountingServiceResult<AccountingPeriodResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Belum ada periode akuntansi yang mencakup tanggal hari ini. " +
                    "Bangkitkan periode tahun buku berjalan lebih dahulu.");
            }

            return AccountingServiceResult<AccountingPeriodResponse>.Ok(
                Petakan(periode), "Periode berjalan berhasil diambil.");
        }

        // ------------------------------------------------------------------
        // Bangkitkan
        // ------------------------------------------------------------------

        /// <remarks>
        /// Acceptance (1) dan (2). Dua belas periode dibangkitkan sekaligus dalam satu
        /// <c>SaveChanges</c>, sehingga tidak mungkin tersisa setengah jalan.
        ///
        /// Tahun kabisat ditangani <see cref="DateTime.DaysInMonth"/>, bukan didaftar manual —
        /// Februari 2028 otomatis berakhir pada tanggal 29.
        /// </remarks>
        public async Task<AccountingServiceResult<List<AccountingPeriodResponse>>> GenerateAsync(
            GenerateAccountingPeriodRequest request,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard
                .PeriksaAsync<List<AccountingPeriodResponse>>(_db, ct);
            if (penjaga is not null) return penjaga;

            if (request.FiscalYear is < TahunBukuMinimum or > TahunBukuMaksimum)
            {
                return AccountingServiceResult<List<AccountingPeriodResponse>>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Tahun buku harus antara {TahunBukuMinimum} sampai {TahunBukuMaksimum}.");
            }

            var badanHukumAda = await _db.Set<MstLegalEntity>()
                .AnyAsync(x => x.Id == request.LegalEntityId && !x.IsDelete, ct);

            if (!badanHukumAda)
            {
                return AccountingServiceResult<List<AccountingPeriodResponse>>.Fail(
                    StatusCodes.Status400BadRequest, "Badan hukum tidak ditemukan.");
            }

            var sudahAda = await _db.Set<AccAccountingPeriod>()
                .AnyAsync(x => !x.IsDelete
                               && x.LegalEntityId == request.LegalEntityId
                               && x.FiscalYear == request.FiscalYear, ct);

            if (sudahAda)
            {
                return AccountingServiceResult<List<AccountingPeriodResponse>>.Fail(
                    StatusCodes.Status409Conflict,
                    $"Periode tahun buku {request.FiscalYear} sudah pernah dibangkitkan untuk badan hukum ini.");
            }

            var sekarang = DateTime.UtcNow;
            var dibuat = new List<AccAccountingPeriod>(12);

            for (var bulan = 1; bulan <= 12; bulan++)
            {
                var hariTerakhir = DateTime.DaysInMonth(request.FiscalYear, bulan);

                dibuat.Add(new AccAccountingPeriod
                {
                    Id = Guid.NewGuid(),
                    LegalEntityId = request.LegalEntityId,
                    PeriodCode = $"{request.FiscalYear:D4}-{bulan:D2}",
                    FiscalYear = request.FiscalYear,
                    PeriodMonth = bulan,
                    StartDate = new DateTime(request.FiscalYear, bulan, 1),
                    EndDate = new DateTime(request.FiscalYear, bulan, hariTerakhir),
                    PeriodStatus = AccountingPeriodStatus.Open,
                    CreateDateTime = sekarang,
                    CreateBy = actorUserId
                });
            }

            _db.Set<AccAccountingPeriod>().AddRange(dibuat);
            await _db.SaveChangesAsync(ct);

            return AccountingServiceResult<List<AccountingPeriodResponse>>.Ok(
                dibuat.OrderBy(x => x.PeriodMonth).Select(Petakan).ToList(),
                $"Dua belas periode tahun buku {request.FiscalYear} berhasil dibangkitkan.",
                StatusCodes.Status201Created);
        }

        // ------------------------------------------------------------------
        // Tutup dan buka kembali
        // ------------------------------------------------------------------

        public async Task<AccountingServiceResult<AccountingPeriodResponse>> CloseAsync(
            Guid id,
            ClosePeriodRequest request,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard.PeriksaAsync<AccountingPeriodResponse>(_db, ct);
            if (penjaga is not null) return penjaga;

            var periode = await _db.Set<AccAccountingPeriod>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);

            if (periode is null) return TidakDitemukan();

            var tujuan = request.Permanent
                ? AccountingPeriodStatus.Closed
                : AccountingPeriodStatus.SoftClosed;

            var pelanggaran = PeriksaPerpindahanTutup(periode.PeriodStatus, tujuan, periode);
            if (pelanggaran is not null) return pelanggaran;

            periode.PeriodStatus = tujuan;
            periode.ClosedBy = actorUserId;
            periode.ClosedAt = DateTime.UtcNow;
            periode.LastReasonNote = string.IsNullOrWhiteSpace(request.Reason)
                ? periode.LastReasonNote
                : request.Reason.Trim();
            periode.UpdateDateTime = DateTime.UtcNow;
            periode.UpdateBy = actorUserId;

            await _db.SaveChangesAsync(ct);

            var sebutan = tujuan == AccountingPeriodStatus.Closed ? "tutup permanen" : "tutup sementara";

            return AccountingServiceResult<AccountingPeriodResponse>.Ok(
                Petakan(periode), $"Periode {NamaPeriode(periode)} berhasil diubah menjadi {sebutan}.");
        }

        /// <remarks>
        /// Acceptance (3) dan (4), dan inilah butir yang paling mudah salah di seluruh task ini.
        ///
        /// Periode <c>Closed</c> yang dibuka kembali menjadi <c>SoftClosed</c>, **bukan**
        /// <c>Open</c>. Mengembalikannya ke <c>Open</c> melanggar `ACC-DEC-028` dan membuka pintu
        /// bagi jurnal operasional baru masuk ke tahun buku yang sudah ditutup — yang seharusnya
        /// hanya menerima penyesuaian dan pembalikan.
        /// </remarks>
        public async Task<AccountingServiceResult<AccountingPeriodResponse>> ReopenAsync(
            Guid id,
            ReopenPeriodRequest request,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard.PeriksaAsync<AccountingPeriodResponse>(_db, ct);
            if (penjaga is not null) return penjaga;

            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                return AccountingServiceResult<AccountingPeriodResponse>.Fail(
                    StatusCodes.Status400BadRequest, "Alasan pembukaan kembali wajib diisi.");
            }

            var periode = await _db.Set<AccAccountingPeriod>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);

            if (periode is null) return TidakDitemukan();

            AccountingPeriodStatus tujuan;

            switch (periode.PeriodStatus)
            {
                case AccountingPeriodStatus.SoftClosed:
                    tujuan = AccountingPeriodStatus.Open;
                    break;

                // ACC-DEC-028 — tutup permanen kembali ke tutup sementara, bukan terbuka.
                case AccountingPeriodStatus.Closed:
                    tujuan = AccountingPeriodStatus.SoftClosed;
                    break;

                default:
                    return AccountingServiceResult<AccountingPeriodResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        $"Periode {NamaPeriode(periode)} masih terbuka, sehingga tidak dapat dibuka kembali.");
            }

            periode.PeriodStatus = tujuan;
            periode.ReopenedBy = actorUserId;
            periode.ReopenedAt = DateTime.UtcNow;
            periode.LastReasonNote = request.Reason.Trim();
            periode.UpdateDateTime = DateTime.UtcNow;
            periode.UpdateBy = actorUserId;

            await _db.SaveChangesAsync(ct);

            var sebutan = tujuan == AccountingPeriodStatus.Open ? "terbuka" : "tutup sementara";

            return AccountingServiceResult<AccountingPeriodResponse>.Ok(
                Petakan(periode),
                $"Periode {NamaPeriode(periode)} dibuka kembali menjadi {sebutan}.");
        }

        // ------------------------------------------------------------------
        // Dipakai bersama task lain
        // ------------------------------------------------------------------

        /// <summary>
        /// Menentukan apakah sebuah periode menerima jenis jurnal tertentu, sesuai
        /// `ACC-STATE-0.1` bagian 2.2.
        /// </summary>
        /// <remarks>
        /// Dibuat <c>public static</c> menerima <see cref="ApplicationDbContext"/> supaya
        /// `BE-ACC-010` dan `BE-ACC-011` memakainya tanpa registrasi DI baru — persis yang
        /// diminta cakupan roadmap `BE-ACC-009`.
        ///
        /// Mengembalikan pesan siap tampil bila ditolak, dan <c>null</c> bila diterima. Pesannya
        /// menyebut **nama periode**, bukan istilah teknis, sesuai ketentuan kontrak.
        /// </remarks>
        public static async Task<string?> AlasanPenolakanJenisJurnalAsync(
            ApplicationDbContext db,
            Guid accountingPeriodId,
            string journalTypeCode,
            CancellationToken ct = default)
        {
            var periode = await db.Set<AccAccountingPeriod>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == accountingPeriodId && !x.IsDelete, ct);

            if (periode is null) return "Periode akuntansi tidak ditemukan.";

            return AlasanPenolakanJenisJurnal(periode.PeriodStatus, NamaPeriode(periode), journalTypeCode);
        }

        /// <summary>
        /// Bentuk murni dari aturan yang sama, tanpa menyentuh database. Memudahkan
        /// pengujiannya dan memungkinkan pemanggil yang sudah memuat periodenya.
        /// </summary>
        public static string? AlasanPenolakanJenisJurnal(
            AccountingPeriodStatus status,
            string namaPeriode,
            string journalTypeCode)
        {
            var kode = journalTypeCode?.Trim().ToUpperInvariant() ?? string.Empty;

            return status switch
            {
                AccountingPeriodStatus.Open => null,

                // Tutup sementara hanya menerima penyesuaian dan pembalikan.
                AccountingPeriodStatus.SoftClosed => kode is "JP" or "JB"
                    ? null
                    : $"Periode {namaPeriode} sudah ditutup sementara. Hanya jurnal penyesuaian "
                      + "dan pembalikan yang masih dapat disahkan.",

                AccountingPeriodStatus.Closed =>
                    $"Periode {namaPeriode} sudah ditutup permanen dan tidak menerima jurnal apa pun.",

                _ => $"Status periode {namaPeriode} tidak dikenali."
            };
        }

        /// <summary>Jenis jurnal yang diterima sebuah status periode.</summary>
        public static List<string> JenisJurnalYangDiterima(AccountingPeriodStatus status) =>
            status switch
            {
                AccountingPeriodStatus.Open => new List<string> { "JU", "JP", "JB", "SA" },
                AccountingPeriodStatus.SoftClosed => new List<string> { "JP", "JB" },
                _ => new List<string>()
            };

        // ------------------------------------------------------------------
        // Pembantu
        // ------------------------------------------------------------------

        private static AccountingServiceResult<AccountingPeriodResponse> TidakDitemukan()
            => AccountingServiceResult<AccountingPeriodResponse>.Fail(
                StatusCodes.Status404NotFound, "Periode akuntansi tidak ditemukan.");

        private static AccountingServiceResult<AccountingPeriodResponse>? PeriksaPerpindahanTutup(
            AccountingPeriodStatus dari,
            AccountingPeriodStatus tujuan,
            AccAccountingPeriod periode)
        {
            if (dari == tujuan)
            {
                var sebutan = tujuan == AccountingPeriodStatus.Closed ? "tutup permanen" : "tutup sementara";
                return AccountingServiceResult<AccountingPeriodResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    $"Periode {NamaPeriode(periode)} sudah berstatus {sebutan}.");
            }

            // Satu-satunya perpindahan mundur yang mungkin: Closed hendak dijadikan SoftClosed
            // lewat endpoint tutup. Itu bukan penutupan, melainkan pembukaan kembali — dan ia
            // punya endpoint tersendiri yang mewajibkan alasan tertulis.
            if (dari == AccountingPeriodStatus.Closed && tujuan == AccountingPeriodStatus.SoftClosed)
            {
                return AccountingServiceResult<AccountingPeriodResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    $"Periode {NamaPeriode(periode)} sudah ditutup permanen. Pakai pembukaan kembali "
                    + "yang mewajibkan alasan tertulis.");
            }

            return null;
        }

        private static string NamaPeriode(AccAccountingPeriod periode)
            => $"{NamaBulan[periode.PeriodMonth - 1]} {periode.FiscalYear.ToString(CultureInfo.InvariantCulture)}";

        private static AccountingPeriodResponse Petakan(AccAccountingPeriod x) => new()
        {
            Id = x.Id,
            LegalEntityId = x.LegalEntityId,
            PeriodCode = x.PeriodCode,
            FiscalYear = x.FiscalYear,
            PeriodMonth = x.PeriodMonth,
            StartDate = x.StartDate,
            EndDate = x.EndDate,
            PeriodStatus = x.PeriodStatus,
            PeriodName = NamaPeriode(x),
            ClosedBy = x.ClosedBy,
            ClosedAt = x.ClosedAt,
            ReopenedBy = x.ReopenedBy,
            ReopenedAt = x.ReopenedAt,
            LastReasonNote = x.LastReasonNote,
            AcceptedJournalTypeCodes = JenisJurnalYangDiterima(x.PeriodStatus)
        };
    }
}
