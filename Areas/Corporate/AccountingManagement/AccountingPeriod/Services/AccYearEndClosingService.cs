using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.Configuration.Services;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.JournalType.Services;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Repositories;
using System.Globalization;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Services
{
    /// <summary>
    /// Tutup tahun: menghitung saldo pendapatan dan beban satu tahun buku, lalu menyusun jurnal
    /// penutup yang menolkan seluruhnya dan memindahkan selisihnya ke akun laba ditahan.
    /// Cakupan <c>BE-ACC-P2-010</c>, mewujudkan <c>ACC-DEC-053</c> dan <c>ACC-DEC-054</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Bahaya yang dijaga berkas ini adalah salah hitung yang tidak menimbulkan error.</b>
    /// Jurnal penutup yang angkanya keliru tetap seimbang, tetap dapat disetujui, dan tetap dapat
    /// disahkan — lalu terbawa ke tahun berikutnya sebagai saldo awal laba ditahan. Karena itu
    /// perhitungannya disusun sedemikian rupa sehingga <b>saldo tiap akun benar-benar menjadi
    /// nol</b>, bukan sekadar menghasilkan jurnal yang seimbang. Keseimbangan adalah akibat, bukan
    /// tujuan.
    /// </para>
    /// <para>
    /// <b>Tiga keputusan perhitungan yang mudah salah dan sengaja dicatat di sini.</b>
    /// </para>
    /// <para>
    /// <i>Pertama, saldonya dibatasi periode tahun buku yang ditutup.</i>
    /// <c>AccChartOfAccountService.HitungSaldoAsync</c> menghitung saldo <b>seumur hidup</b> akun.
    /// Memakainya apa adanya tampak menggoda — ia menjamin akunnya nol — tetapi salah, dan
    /// salahnya justru pada keadaan yang paling lazim: tutup tahun 2026 dikerjakan pada Maret
    /// 2027, ketika Januari dan Februari 2027 sudah berisi jurnal yang disahkan. Saldo seumur
    /// hidup akan menyapu pendapatan 2027 ke laba ditahan 2026 dan membuat akun 2027 bersaldo
    /// negatif — tanpa satu pun error. Karena itu yang dihitung hanyalah baris jurnal disahkan
    /// yang periodenya termasuk tahun buku itu, persis seperti bunyi <c>flowcharts/04-tutup-tahun.md</c>
    /// langkah 3: "baris jurnal disahkan sepanjang tahun".
    /// </para>
    /// <para>
    /// <i>Kedua, baris beban dipecah per unit biaya.</i> <c>ACC-DEC-019</c> mewajibkan baris
    /// berakun <c>Expense</c> menyebutkan unit biaya, dan syarat 7 <c>ACC-STATE-0.1</c> bagian
    /// 1.3 memeriksanya ulang saat pengajuan dan pengesahan. Satu baris gabungan per akun beban
    /// akan tertahan di sana, dan menebak satu unit biaya akan merusak laporan beban per unit
    /// biaya tanpa membuat jurnalnya timpang. Jumlah baris per akun tetap sama persis dengan
    /// saldo akun itu, sehingga penolkan per akun tidak berubah sedikit pun.
    /// </para>
    /// <para>
    /// <i>Ketiga, penilaian akun laba ditahan dipinjam dari <c>BE-ACC-P2-009</c>.</i>
    /// <see cref="AccAccountingConfigurationService.AmbilAkunLabaDitahanAsync"/> dipanggil apa
    /// adanya supaya tutup tahun dan layar pengaturan tidak pernah berbeda pendapat tentang akun
    /// mana yang berlaku.
    /// </para>
    /// <para>
    /// <b>Jurnalnya dibuat lewat <see cref="AccJournalService.CreateAsync"/>, bukan ditulis
    /// sendiri ke tabel.</b> Dengan begitu penomoran, penentuan periode, kesembilan syarat, dan
    /// seluruh validasi baris berlaku sama bagi jurnal penutup seperti bagi jurnal mana pun.
    /// Jurnal penutup adalah jurnal biasa (<c>ACC-STATE-0.2</c> bagian 4); menuliskannya lewat
    /// jalur sendiri berarti membuat jalur kedua yang aturannya diam-diam berbeda.
    /// </para>
    /// </remarks>
    public class AccYearEndClosingService
    {
        private readonly ApplicationDbContext _db;
        private readonly AccJournalService _journalService;

        public AccYearEndClosingService(ApplicationDbContext db, AccJournalService journalService)
        {
            _db = db;
            _journalService = journalService;
        }

        /// <summary>Kode jenis jurnal penutup tahun (<c>ACC-DEC-053</c>).</summary>
        private const string KodeJenisJurnal = "JT";

        // ------------------------------------------------------------------
        // Pratinjau
        // ------------------------------------------------------------------

        /// <summary>
        /// Menghitung dan menampilkan rencana jurnal penutup tanpa membuat apa pun.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Acceptance (1): tidak ada satu pun tulisan di sini.</b> Method ini tidak memanggil
        /// <c>SaveChangesAsync</c>, tidak membuka transaction, dan tidak menyentuh
        /// <c>AccNumberSeries</c> — nomor jurnal baru dialokasikan pada
        /// <see cref="GenerateAsync"/>. Dipanggil seratus kali pun, isi database tetap sama.
        /// </para>
        /// <para>
        /// <b>Yang sengaja TIDAK diperiksa di sini adalah jurnal penutup ganda.</b> Penolakan
        /// ganda adalah aturan langkah 5 pada <c>flowcharts/04-tutup-tahun.md</c> — langkah
        /// menyusun, bukan langkah memeriksa angka. Melihat kembali angka tahun yang jurnal
        /// penutupnya sudah tersusun adalah kebutuhan yang wajar, dan menolaknya tidak
        /// melindungi apa pun.
        /// </para>
        /// </remarks>
        public async Task<AccountingServiceResult<YearEndClosingPreviewResponse>> PreviewAsync(
            Guid legalEntityId,
            int fiscalYear,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard
                .PeriksaAsync<YearEndClosingPreviewResponse>(_db, ct);
            if (penjaga is not null) return penjaga;

            var rencana = await SusunRencanaAsync(legalEntityId, fiscalYear, ct);

            if (rencana.Gagal)
            {
                return AccountingServiceResult<YearEndClosingPreviewResponse>.Fail(
                    rencana.StatusCode, rencana.Pesan);
            }

            return AccountingServiceResult<YearEndClosingPreviewResponse>.Ok(
                Petakan(rencana, legalEntityId, fiscalYear),
                $"Pratinjau tutup tahun buku {Angka(fiscalYear)} berhasil dihitung. "
                + rencana.RingkasanLaba);
        }

        // ------------------------------------------------------------------
        // Penyusunan
        // ------------------------------------------------------------------

        /// <summary>
        /// Menyusun jurnal penutup tahun berstatus <c>Draft</c> berjenis <c>JT</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Perhitungannya <b>diulang di sini</b>, bukan diterima dari pratinjau. Pratinjau adalah
        /// tampilan, dan angka yang datang dari pemanggil bukan bukti apa pun: jurnal Desember
        /// dapat saja disahkan di antara pratinjau dibuka dan tombol Susun ditekan. Itu sebabnya
        /// <see cref="GenerateYearEndClosingRequest"/> hanya membawa badan hukum dan tahun buku,
        /// tanpa satu pun nominal.
        /// </para>
        /// <para>
        /// <b>Penjaga jurnal ganda dan pembuatan jurnal berada dalam satu transaction</b>, supaya
        /// keduanya tidak dapat disela penyimpanan lain dari koneksi yang sama. Perlu diketahui
        /// batasnya: tanpa unique index di database, dua permintaan <b>bersamaan</b> pada dua
        /// koneksi berbeda masih dapat lolos keduanya. Penutupnya adalah constraint database, dan
        /// itu menuntut migration — di luar wewenang task ini. Dicatat sebagai risiko tersisa,
        /// bukan didiamkan.
        /// </para>
        /// </remarks>
        public async Task<AccountingServiceResult<JournalDetailResponse>> GenerateAsync(
            GenerateYearEndClosingRequest request,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard
                .PeriksaAsync<JournalDetailResponse>(_db, ct);
            if (penjaga is not null) return penjaga;

            var jenis = await AccJournalTypeService.CariMenurutKodeAsync(_db, KodeJenisJurnal, ct);

            if (jenis is null)
            {
                return AccountingServiceResult<JournalDetailResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    $"Jenis jurnal {KodeJenisJurnal} belum ada pada master jenis jurnal. Jalankan "
                    + "pengisian data master jenis jurnal lebih dahulu.");
            }

            var rencana = await SusunRencanaAsync(request.LegalEntityId, request.FiscalYear, ct);

            if (rencana.Gagal)
            {
                return AccountingServiceResult<JournalDetailResponse>.Fail(
                    rencana.StatusCode, rencana.Pesan);
            }

            var transaksiSendiri = _db.Database.CurrentTransaction is null;
            var transaksi = transaksiSendiri
                ? await _db.Database.BeginTransactionAsync(ct)
                : null;

            try
            {
                var sudahAda = await CariJurnalPenutupAsync(
                    request.LegalEntityId, jenis.Id, rencana.IdPeriode, ct);

                if (sudahAda is not null)
                {
                    if (transaksi is not null) await transaksi.RollbackAsync(ct);

                    return AccountingServiceResult<JournalDetailResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        $"Jurnal penutup tahun buku {Angka(request.FiscalYear)} sudah pernah "
                        + $"disusun dengan nomor {sudahAda.JournalNumber} berstatus "
                        + $"{SebutanStatus(sudahAda.JournalStatus)}. Hapus atau balik jurnal itu "
                        + "lebih dahulu bila memang perlu disusun ulang.");
                }

                var keterangan = string.IsNullOrWhiteSpace(request.Description)
                    ? $"Jurnal penutup tahun buku {Angka(request.FiscalYear)}."
                    : request.Description.Trim();

                var permintaan = new CreateJournalRequest
                {
                    LegalEntityId = request.LegalEntityId,
                    JournalTypeId = jenis.Id,
                    AccountingDate = rencana.TanggalPenutupan,
                    Description = keterangan,
                    Lines = rencana.SeluruhBaris
                        .Select((x, urutan) => new CreateJournalLineRequest
                        {
                            LineNumber = urutan + 1,
                            AccountId = x.AccountId,
                            CostCenterId = x.CostCenterId,
                            Description = KeteranganBaris(x, request.FiscalYear),
                            DebitAmount = x.DebitAmount,
                            CreditAmount = x.CreditAmount
                        })
                        .ToList()
                };

                var hasil = await _journalService.CreateAsync(permintaan, actorUserId, null, ct);

                if (!hasil.Success)
                {
                    if (transaksi is not null) await transaksi.RollbackAsync(ct);
                    return hasil;
                }

                if (transaksi is not null) await transaksi.CommitAsync(ct);

                return AccountingServiceResult<JournalDetailResponse>.Ok(
                    hasil.Data,
                    $"Jurnal penutup tahun buku {Angka(request.FiscalYear)} berhasil disusun "
                    + $"sebagai draft. {rencana.RingkasanLaba} Jurnal ini masih harus diajukan, "
                    + "disetujui, dan disahkan seperti jurnal lainnya.",
                    hasil.StatusCode);
            }
            catch
            {
                if (transaksi is not null) await transaksi.RollbackAsync(ct);
                throw;
            }
            finally
            {
                if (transaksi is not null) await transaksi.DisposeAsync();
            }
        }

        // ------------------------------------------------------------------
        // Dipakai bersama task lain
        // ------------------------------------------------------------------

        /// <summary>
        /// Saldo tiap pasangan akun dan unit biaya sepanjang satu tahun buku, dihitung
        /// <b>hanya</b> dari baris jurnal yang jurnalnya berstatus <c>Posted</c>, dan hanya untuk
        /// akun <c>Revenue</c> serta <c>Expense</c> milik badan hukum itu.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Perjanjian tandanya sama persis dengan
        /// <c>AccChartOfAccountService.HitungSaldoAsync</c>: <c>Balance</c> adalah debit dikurangi
        /// kredit, sehingga positif berarti condong debit. Yang berbeda hanyalah cakupannya —
        /// di sana seumur hidup akun, di sini terbatas periode tahun buku yang ditutup. Alasan
        /// perbedaan itu ada pada catatan kelas ini.
        /// </para>
        /// <para>
        /// Baris bersaldo nol <b>ikut dikembalikan</b>. Pemanggil yang menyusun jurnal menyaringnya
        /// sendiri; pemanggil yang memverifikasi penolkan justru membutuhkannya, sebab akun yang
        /// sudah nol adalah bukti bahwa penutupan berhasil, bukan baris yang layak hilang.
        /// </para>
        /// <para>
        /// Dibuat <c>public static</c> menerima <see cref="ApplicationDbContext"/> mengikuti
        /// <c>02-backend-architecture.md</c> bagian 6, supaya <c>BE-ACC-P2-014</c> dan pengujian
        /// dapat memakainya tanpa registrasi DI baru.
        /// </para>
        /// </remarks>
        public static async Task<List<YearEndClosingPreviewLineResponse>> HitungSaldoTahunAsync(
            ApplicationDbContext db,
            Guid legalEntityId,
            IReadOnlyCollection<Guid> accountingPeriodIds,
            CancellationToken ct = default)
        {
            var idPeriode = accountingPeriodIds.ToList();

            if (idPeriode.Count == 0) return new List<YearEndClosingPreviewLineResponse>();

            var mentah = await (
                from baris in db.Set<AccJournalLine>().AsNoTracking()
                join akun in db.Set<AccChartOfAccount>().AsNoTracking() on baris.AccountId equals akun.Id
                join jurnal in db.Set<AccJournal>().AsNoTracking() on baris.JournalId equals jurnal.Id
                where !baris.IsDelete
                      && !akun.IsDelete
                      && !jurnal.IsDelete
                      && jurnal.JournalStatus == JournalStatus.Posted
                      && idPeriode.Contains(jurnal.AccountingPeriodId)
                      && akun.LegalEntityId == legalEntityId
                      && (akun.AccountType == AccountType.Revenue
                          || akun.AccountType == AccountType.Expense)
                group baris by new
                {
                    baris.AccountId,
                    akun.AccountCode,
                    akun.AccountName,
                    akun.AccountType,
                    baris.CostCenterId
                }
                into kelompok
                select new
                {
                    kelompok.Key.AccountId,
                    kelompok.Key.AccountCode,
                    kelompok.Key.AccountName,
                    kelompok.Key.AccountType,
                    kelompok.Key.CostCenterId,
                    Debit = kelompok.Sum(x => x.DebitAmount),
                    Kredit = kelompok.Sum(x => x.CreditAmount)
                }).ToListAsync(ct);

            var idUnitBiaya = mentah
                .Where(x => x.CostCenterId.HasValue)
                .Select(x => x.CostCenterId!.Value)
                .Distinct()
                .ToList();

            var namaUnitBiaya = idUnitBiaya.Count == 0
                ? new Dictionary<Guid, string>()
                : await db.Set<MstCostCenter>()
                    .AsNoTracking()
                    .Where(x => idUnitBiaya.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.CostCenterName, ct);

            return mentah
                .Select(x =>
                {
                    var saldo = x.Debit - x.Kredit;

                    return new YearEndClosingPreviewLineResponse
                    {
                        AccountId = x.AccountId,
                        AccountCode = x.AccountCode,
                        AccountName = x.AccountName,
                        AccountType = x.AccountType,
                        CostCenterId = x.CostCenterId,
                        CostCenterName = x.CostCenterId.HasValue
                            && namaUnitBiaya.TryGetValue(x.CostCenterId.Value, out var nama)
                                ? nama
                                : null,
                        Balance = saldo,

                        // Menolkan saldo berarti mencatat lawannya. Rumus ini sengaja tidak
                        // memandang jenis akun maupun saldo normalnya: akun pendapatan yang
                        // kebetulan bersaldo debit — misalnya karena retur melebihi pendapatan —
                        // tetap menjadi nol dengan rumus yang sama.
                        DebitAmount = saldo < 0m ? -saldo : 0m,
                        CreditAmount = saldo > 0m ? saldo : 0m
                    };
                })
                .OrderBy(x => x.AccountType == AccountType.Revenue ? 0 : 1)
                .ThenBy(x => x.AccountCode, StringComparer.Ordinal)
                .ThenBy(x => x.CostCenterName ?? string.Empty, StringComparer.Ordinal)
                .ThenBy(x => x.CostCenterId)
                .ToList();
        }

        // ------------------------------------------------------------------
        // Perhitungan bersama pratinjau dan penyusunan
        // ------------------------------------------------------------------

        /// <summary>
        /// Rencana jurnal penutup satu tahun buku, atau alasan penolakannya.
        /// </summary>
        /// <remarks>
        /// Pratinjau dan penyusunan memakai method yang <b>sama</b>, bukan dua perhitungan yang
        /// mirip. Pratinjau yang berbeda dari jurnal yang akhirnya tersusun tidak akan pernah
        /// tertangkap siapa pun — keduanya sama-sama seimbang.
        /// </remarks>
        private sealed class RencanaTutupTahun
        {
            public int StatusCode { get; init; }

            public string Pesan { get; init; } = string.Empty;

            public bool Gagal => StatusCode != 0;

            public List<Guid> IdPeriode { get; init; } = new();

            public int JumlahPeriode { get; init; }

            public DateTime TanggalPenutupan { get; init; }

            public AccChartOfAccount AkunLabaDitahan { get; init; } = null!;

            public List<YearEndClosingPreviewLineResponse> BarisPendapatan { get; init; } = new();

            public List<YearEndClosingPreviewLineResponse> BarisBeban { get; init; } = new();

            public YearEndClosingPreviewLineResponse? BarisLabaDitahan { get; init; }

            public decimal TotalPendapatan { get; init; }

            public decimal TotalBeban { get; init; }

            public decimal LabaBersih { get; init; }

            /// <summary>Seluruh baris jurnal penutup, sudah terurut siap diberi nomor baris.</summary>
            public IEnumerable<YearEndClosingPreviewLineResponse> SeluruhBaris
            {
                get
                {
                    foreach (var x in BarisPendapatan) yield return x;
                    foreach (var x in BarisBeban) yield return x;
                    if (BarisLabaDitahan is not null) yield return BarisLabaDitahan;
                }
            }

            public string RingkasanLaba => LabaBersih >= 0m
                ? $"Laba tahun itu Rp {LabaBersih:N0} dipindahkan ke akun laba ditahan."
                : $"Rugi tahun itu Rp {Math.Abs(LabaBersih):N0} dibebankan ke akun laba ditahan.";
        }

        private static RencanaTutupTahun Tolak(int statusCode, string pesan)
            => new() { StatusCode = statusCode, Pesan = pesan };

        private async Task<RencanaTutupTahun> SusunRencanaAsync(
            Guid legalEntityId,
            int fiscalYear,
            CancellationToken ct)
        {
            if (legalEntityId == Guid.Empty)
            {
                return Tolak(StatusCodes.Status400BadRequest, "Badan hukum wajib dipilih.");
            }

            if (fiscalYear < 2000 || fiscalYear > 2100)
            {
                return Tolak(
                    StatusCodes.Status400BadRequest,
                    "Tahun buku tidak masuk akal. Isi tahun antara 2000 dan 2100.");
            }

            // ------------------------------------------------------------------
            // Langkah 2 flowchart — seluruh periode tahun itu harus tertutup.
            // ------------------------------------------------------------------
            var periode = await _db.Set<AccAccountingPeriod>()
                .AsNoTracking()
                .Where(x => !x.IsDelete
                            && x.LegalEntityId == legalEntityId
                            && x.FiscalYear == fiscalYear)
                .OrderBy(x => x.PeriodMonth)
                .ToListAsync(ct);

            if (periode.Count == 0)
            {
                return Tolak(
                    StatusCodes.Status422UnprocessableEntity,
                    $"Belum ada periode akuntansi untuk tahun buku {Angka(fiscalYear)}. Minta "
                    + "administrator membangkitkan periode tahun buku ini lebih dahulu.");
            }

            // Yang menahan hanyalah Open dan PendingClosingApproval (`ACC-VALIDATION-0.6`
            // bagian 5). SoftClosed sudah cukup: masa tenggang tutup buku memang tempat jurnal
            // penutup tahun tinggal, dan menuntut Closed akan mengunci tutup tahun selamanya —
            // periode Closed tidak menerima jurnal apa pun, termasuk jurnal penutupnya sendiri.
            var belumTertutup = periode
                .Where(x => x.PeriodStatus is AccountingPeriodStatus.Open
                                           or AccountingPeriodStatus.PendingClosingApproval)
                .ToList();

            if (belumTertutup.Count > 0)
            {
                var daftar = string.Join(
                    ", ",
                    belumTertutup.Select(x =>
                        $"{AccAccountingPeriodService.NamaPeriode(x)} ({SebutanStatusPeriode(x.PeriodStatus)})"));

                return Tolak(
                    StatusCodes.Status409Conflict,
                    $"Masih ada {Angka(belumTertutup.Count)} periode tahun "
                    + $"{Angka(fiscalYear)} yang belum ditutup: {daftar}. Tutup periode-periode "
                    + "itu lebih dahulu sebelum menyusun jurnal penutup tahun.");
            }

            // Tanggal akuntansi jurnal penutup adalah tanggal terakhir periode terakhir tahun
            // buku itu. Diambil dari periodenya, bukan ditulis 31 Desember di kode, supaya tetap
            // benar bila tahun bukunya belum lengkap dua belas periode.
            var periodeTerakhir = periode[^1];

            // Periode tujuan harus benar-benar menerima jurnal JT. Aturannya dipinjam apa adanya
            // dari AccAccountingPeriodService supaya tutup tahun tidak punya tafsir kedua atas
            // status periode.
            //
            // Yang tersaring di sini hanyalah periode terakhir yang sudah TUTUP PERMANEN. Itu
            // keadaan yang sah tetapi buntu: periode Closed tidak menerima jurnal apa pun,
            // termasuk jurnal penutup tahunnya sendiri. Diperiksa sekarang, bukan dibiarkan
            // muncul jauh di dalam pembuatan jurnal — pratinjau yang tampak sempurna lalu ditolak
            // dengan alasan yang tidak menyebut periodenya adalah cara tercepat membuat orang
            // menyalahkan angkanya.
            var alasanPeriodeTujuan = AccAccountingPeriodService.AlasanPenolakanJenisJurnal(
                periodeTerakhir.PeriodStatus,
                AccAccountingPeriodService.NamaPeriode(periodeTerakhir),
                KodeJenisJurnal);

            if (alasanPeriodeTujuan is not null)
            {
                return Tolak(
                    StatusCodes.Status422UnprocessableEntity,
                    $"Jurnal penutup tahun buku {Angka(fiscalYear)} harus dicatat pada periode "
                    + $"{AccAccountingPeriodService.NamaPeriode(periodeTerakhir)}, dan periode itu "
                    + $"tidak menerimanya. {alasanPeriodeTujuan}");
            }

            // ------------------------------------------------------------------
            // Langkah 3 flowchart — akun laba ditahan wajib sudah ditetapkan.
            // ------------------------------------------------------------------
            var akunLabaDitahan = await AccAccountingConfigurationService
                .AmbilAkunLabaDitahanAsync(_db, legalEntityId, ct);

            if (akunLabaDitahan is null)
            {
                return Tolak(
                    StatusCodes.Status422UnprocessableEntity,
                    "Akun laba ditahan belum ditetapkan pada pengaturan akuntansi. Tetapkan "
                    + "akunnya lebih dahulu lewat Pengaturan Akuntansi.");
            }

            // Diperiksa ulang walaupun BE-ACC-P2-009 sudah memeriksanya saat ditetapkan: akun
            // dapat dinonaktifkan, diubah menjadi akun induk, atau dihapus SESUDAH ditetapkan.
            // Tanpa pemeriksaan ini penolakannya tetap terjadi, tetapi jauh di dalam validasi
            // baris jurnal, dengan kalimat "Baris ke-5" yang tidak menyebut pengaturan akuntansi
            // sama sekali.
            var cacatAkun = AlasanAkunLabaDitahanTidakLayak(akunLabaDitahan, legalEntityId);

            if (cacatAkun is not null)
            {
                return Tolak(StatusCodes.Status422UnprocessableEntity, cacatAkun);
            }

            // ------------------------------------------------------------------
            // Langkah 3 flowchart — saldo pendapatan dan beban.
            // ------------------------------------------------------------------
            var idPeriode = periode.Select(x => x.Id).ToList();

            var seluruhSaldo = await HitungSaldoTahunAsync(_db, legalEntityId, idPeriode, ct);

            var baris = seluruhSaldo.Where(x => x.Balance != 0m).ToList();

            if (baris.Count == 0)
            {
                return Tolak(
                    StatusCodes.Status422UnprocessableEntity,
                    $"Tidak ada saldo pendapatan maupun beban yang perlu ditutup pada tahun buku "
                    + $"{Angka(fiscalYear)}.");
            }

            var barisPendapatan = baris.Where(x => x.AccountType == AccountType.Revenue).ToList();
            var barisBeban = baris.Where(x => x.AccountType == AccountType.Expense).ToList();

            // Disajikan positif sebagaimana lazimnya dibaca: saldo normal pendapatan adalah
            // kredit, sehingga saldo debit-dikurangi-kreditnya negatif.
            var totalPendapatan = -barisPendapatan.Sum(x => x.Balance);
            var totalBeban = barisBeban.Sum(x => x.Balance);
            var labaBersih = totalPendapatan - totalBeban;

            // Baris laba ditahan menutup selisihnya. Bila labanya tepat nol — pendapatan dan
            // beban sama persis — barisnya tidak dibuat sama sekali: baris bernilai nol pada
            // kedua sisi ditolak validasi baris jurnal, dan jurnalnya memang sudah seimbang
            // tanpa baris itu.
            var barisLabaDitahan = labaBersih == 0m
                ? null
                : new YearEndClosingPreviewLineResponse
                {
                    AccountId = akunLabaDitahan.Id,
                    AccountCode = akunLabaDitahan.AccountCode,
                    AccountName = akunLabaDitahan.AccountName,
                    AccountType = akunLabaDitahan.AccountType,
                    CostCenterId = null,
                    CostCenterName = null,
                    Balance = 0m,
                    DebitAmount = labaBersih < 0m ? -labaBersih : 0m,
                    CreditAmount = labaBersih > 0m ? labaBersih : 0m
                };

            return new RencanaTutupTahun
            {
                IdPeriode = idPeriode,
                JumlahPeriode = periode.Count,
                TanggalPenutupan = periodeTerakhir.EndDate.Date,
                AkunLabaDitahan = akunLabaDitahan,
                BarisPendapatan = barisPendapatan,
                BarisBeban = barisBeban,
                BarisLabaDitahan = barisLabaDitahan,
                TotalPendapatan = totalPendapatan,
                TotalBeban = totalBeban,
                LabaBersih = labaBersih
            };
        }

        /// <remarks>
        /// Keempat penolakan memakai kalimat yang menyebut <b>pengaturan akuntansi</b>, bukan
        /// nomor baris jurnal, supaya petugas tahu layar mana yang harus dibuka.
        /// </remarks>
        private static string? AlasanAkunLabaDitahanTidakLayak(
            AccChartOfAccount akun,
            Guid legalEntityId)
        {
            if (akun.IsDelete)
            {
                return "Akun laba ditahan pada pengaturan akuntansi sudah dihapus dari daftar "
                     + "akun. Tetapkan akun laba ditahan yang lain lebih dahulu.";
            }

            if (akun.LegalEntityId != legalEntityId)
            {
                return $"Akun laba ditahan {akun.AccountCode} bukan milik badan hukum ini. "
                     + "Perbaiki pengaturan akuntansi lebih dahulu.";
            }

            if (akun.AccountType != AccountType.Equity)
            {
                return $"Akun laba ditahan {akun.AccountCode} bukan akun berjenis Ekuitas. "
                     + "Perbaiki pengaturan akuntansi lebih dahulu.";
            }

            if (!akun.IsPostable)
            {
                return $"Akun laba ditahan {akun.AccountCode} adalah akun induk dan tidak dapat "
                     + "menerima transaksi. Perbaiki pengaturan akuntansi lebih dahulu.";
            }

            return akun.IsActive
                ? null
                : $"Akun laba ditahan {akun.AccountCode} sudah tidak aktif. Aktifkan kembali "
                  + "akunnya, atau tunjuk akun laba ditahan yang lain pada pengaturan akuntansi.";
        }

        /// <summary>
        /// Jurnal penutup tahun yang sudah pernah disusun untuk tahun buku itu, atau
        /// <c>null</c>.
        /// </summary>
        /// <remarks>
        /// Jurnal yang <b>dihapus</b> tidak menahan penyusunan ulang — draft yang keliru memang
        /// dihapus lalu disusun ulang, dan itu jalur yang disebut <c>flowcharts/04-tutup-tahun.md</c>
        /// langkah 6. Selain itu, status apa pun menahan: draft yang masih menunggu, jurnal yang
        /// ditolak dan belum diperbaiki, maupun jurnal yang sudah disahkan. Satu tahun buku hanya
        /// boleh punya satu jurnal penutup.
        /// </remarks>
        private Task<YearEndClosingExistingJournal?> CariJurnalPenutupAsync(
            Guid legalEntityId,
            Guid journalTypeId,
            List<Guid> idPeriode,
            CancellationToken ct)
        {
            return _db.Set<AccJournal>()
                .AsNoTracking()
                .Where(x => !x.IsDelete
                            && x.LegalEntityId == legalEntityId
                            && x.JournalTypeId == journalTypeId
                            && idPeriode.Contains(x.AccountingPeriodId))
                .OrderBy(x => x.JournalNumber)
                .Select(x => new YearEndClosingExistingJournal
                {
                    Id = x.Id,
                    JournalNumber = x.JournalNumber,
                    JournalStatus = x.JournalStatus
                })
                .FirstOrDefaultAsync(ct);
        }

        // ------------------------------------------------------------------
        // Pemetaan dan pembantu
        // ------------------------------------------------------------------

        private static YearEndClosingPreviewResponse Petakan(
            RencanaTutupTahun rencana,
            Guid legalEntityId,
            int fiscalYear)
        {
            var seluruh = rencana.SeluruhBaris.ToList();

            return new YearEndClosingPreviewResponse
            {
                LegalEntityId = legalEntityId,
                FiscalYear = fiscalYear,
                EvaluatedAt = DateTime.UtcNow,
                ClosingDate = rencana.TanggalPenutupan,
                PeriodCount = rencana.JumlahPeriode,
                RetainedEarningsAccountId = rencana.AkunLabaDitahan.Id,
                RetainedEarningsAccountCode = rencana.AkunLabaDitahan.AccountCode,
                RetainedEarningsAccountName = rencana.AkunLabaDitahan.AccountName,
                RevenueLines = rencana.BarisPendapatan,
                ExpenseLines = rencana.BarisBeban,
                TotalRevenue = rencana.TotalPendapatan,
                TotalExpense = rencana.TotalBeban,
                NetIncome = rencana.LabaBersih,
                RetainedEarningsDebit = rencana.BarisLabaDitahan?.DebitAmount ?? 0m,
                RetainedEarningsCredit = rencana.BarisLabaDitahan?.CreditAmount ?? 0m,
                LineCount = seluruh.Count,
                TotalDebit = seluruh.Sum(x => x.DebitAmount),
                TotalCredit = seluruh.Sum(x => x.CreditAmount),
                IsBalanced = seluruh.Sum(x => x.DebitAmount) == seluruh.Sum(x => x.CreditAmount)
            };
        }

        private static string KeteranganBaris(
            YearEndClosingPreviewLineResponse baris,
            int fiscalYear)
        {
            var keterangan = baris.AccountType == AccountType.Equity
                ? $"Laba rugi tahun buku {Angka(fiscalYear)}"
                : $"Penutupan saldo {baris.AccountCode} tahun buku {Angka(fiscalYear)}";

            if (!string.IsNullOrWhiteSpace(baris.CostCenterName))
            {
                keterangan += $" — {baris.CostCenterName}";
            }

            return keterangan.Length > 500 ? keterangan[..500] : keterangan;
        }

        private static string SebutanStatus(JournalStatus status) => status switch
        {
            JournalStatus.Draft => "draft",
            JournalStatus.PendingApproval => "menunggu persetujuan",
            JournalStatus.Approved => "sudah disetujui",
            JournalStatus.Posted => "sudah disahkan",
            JournalStatus.Rejected => "ditolak",
            _ => status.ToString()
        };

        private static string SebutanStatusPeriode(AccountingPeriodStatus status) => status switch
        {
            AccountingPeriodStatus.Open => "masih terbuka",
            AccountingPeriodStatus.PendingClosingApproval => "menunggu persetujuan penutupan",
            AccountingPeriodStatus.SoftClosed => "tutup sementara",
            AccountingPeriodStatus.Closed => "tutup permanen",
            _ => status.ToString()
        };

        private static string Angka(int nilai)
            => nilai.ToString(CultureInfo.InvariantCulture);
    }
}
