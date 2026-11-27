using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Services
{
    public partial class AccChartOfAccountService
    {
        public const string NamaIndexKodeAkun = "IX_AccChartOfAccount_LegalEntityId_AccountCode";

        private const int PanjangKodeMaksimum = 20;

        private static readonly Regex PolaKodeAkar = new("^[1-5]-0000$", RegexOptions.CultureInvariant);

        private static readonly Regex PolaKodeGrup = new("^[1-5]-[1-9]000$", RegexOptions.CultureInvariant);

        public async Task<AccountingServiceResult<ChartOfAccountNextCodeResponse>> GetNextCodeAsync(
            ChartOfAccountNextCodeQuery query,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard.PeriksaAsync<ChartOfAccountNextCodeResponse>(_db, ct);
            if (penjaga is not null) return penjaga;

            var badanHukumAda = await _db.Set<MstLegalEntity>()
                .AnyAsync(x => x.Id == query.LegalEntityId && !x.IsDelete, ct);

            if (!badanHukumAda)
            {
                return AccountingServiceResult<ChartOfAccountNextCodeResponse>.Fail(
                    StatusCodes.Status400BadRequest, "Badan hukum tidak ditemukan.");
            }

            var induk = await PeriksaIndukAsync<ChartOfAccountNextCodeResponse>(
                query.ParentAccountId, query.LegalEntityId, idSaatIni: null, ct);
            if (induk.Gagal is not null) return induk.Gagal;

            if (induk.Akun is not null && await PunyaBarisJurnalDisahkanAsync(induk.Akun.Id, ct))
            {
                return AccountingServiceResult<ChartOfAccountNextCodeResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    $"Akun {induk.Akun.AccountCode} sudah memiliki transaksi, sehingga tidak dapat diberi akun turunan.");
            }

            var jenis = TentukanJenis<ChartOfAccountNextCodeResponse>(induk.Akun, query.AccountType);
            if (jenis.Gagal is not null) return jenis.Gagal;

            var tingkatAkun = induk.Akun is null ? TingkatMinimum : induk.Akun.AccountLevel + 1;

            var tingkat = PeriksaTingkatTerhadapInduk<ChartOfAccountNextCodeResponse>(
                induk.Akun, tingkatAkun, kedalamanTurunan: 0);
            if (tingkat is not null) return tingkat;

            var usulan = await SusunKodeAsync(query.LegalEntityId, induk.Akun, jenis.Jenis, ct);

            if (usulan.Galat is not null)
            {
                return AccountingServiceResult<ChartOfAccountNextCodeResponse>.Fail(
                    StatusCodes.Status409Conflict, usulan.Galat);
            }

            var warisan = induk.Akun is null
                ? null
                : AccChartOfAccountClassification
                    .Hitung(await AccChartOfAccountClassification.MuatAsync(_db, query.LegalEntityId, ct))
                    .GetValueOrDefault(induk.Akun.Id);

            return AccountingServiceResult<ChartOfAccountNextCodeResponse>.Ok(
                new ChartOfAccountNextCodeResponse
                {
                    InheritedReportClassification = warisan?.ReportClassification,
                    InheritedCurrentAssetCategory = warisan?.CurrentAssetCategory,
                    InheritedFromAccountCode = warisan?.ReportClassificationSourceAccountCode
                        ?? warisan?.CurrentAssetCategorySourceAccountCode,
                    AccountCode = usulan.Kode!,
                    AccountLevel = tingkatAkun,
                    AccountType = jenis.Jenis,
                    ParentAccountId = induk.Akun?.Id,
                    ParentAccountCode = induk.Akun?.AccountCode,
                    ParentAccountName = induk.Akun?.AccountName,
                    SuggestedNormalBalance = induk.Akun?.NormalBalance
                },
                "Usulan kode akun berhasil disusun.");
        }

        private static (AccountType Jenis, AccountingServiceResult<T>? Gagal) TentukanJenis<T>(
            AccChartOfAccount? induk,
            AccountType? diminta)
        {
            if (diminta.HasValue && !Enum.IsDefined(diminta.Value))
            {
                return (default, AccountingServiceResult<T>.Fail(
                    StatusCodes.Status400BadRequest, "Jenis akun tidak dikenal."));
            }

            if (induk is null)
            {
                return diminta.HasValue
                    ? (diminta.Value, null)
                    : (default, AccountingServiceResult<T>.Fail(
                        StatusCodes.Status400BadRequest, "Jenis akun wajib dipilih untuk akun tingkat 1."));
            }

            if (diminta.HasValue && diminta.Value != induk.AccountType)
                return (default, JenisBedaInduk<T>(induk));

            return (induk.AccountType, null);
        }

        private static AccountingServiceResult<T> JenisBedaInduk<T>(AccChartOfAccount induk)
            => AccountingServiceResult<T>.Fail(
                StatusCodes.Status409Conflict,
                $"Jenis akun harus sama dengan akun induk {induk.AccountCode} ({NamaJenis(induk.AccountType)}).");

        private static string NamaJenis(AccountType jenis) => jenis switch
        {
            AccountType.Asset => "Aset",
            AccountType.Liability => "Liabilitas",
            AccountType.Equity => "Ekuitas",
            AccountType.Revenue => "Pendapatan",
            AccountType.Expense => "Beban",
            _ => ((int)jenis).ToString(CultureInfo.InvariantCulture)
        };

        private async Task KunciPenomoranAsync(
            Guid legalEntityId,
            AccChartOfAccount? induk,
            AccountType jenis,
            CancellationToken ct)
        {
            if (!_db.Database.IsNpgsql()) return;

            var lingkup = induk is null || PolaKodeAkar.IsMatch(induk.AccountCode)
                ? $"JENIS_{(int)jenis}"
                : $"INDUK_{induk.Id:N}";

            await _db.Database.ExecuteSqlRawAsync(
                "SELECT pg_advisory_xact_lock(hashtext({0}));",
                [$"ACC_COA_KODE_{legalEntityId:N}_{lingkup}"],
                ct);
        }

        private async Task<(string? Kode, string? Galat)> SusunKodeAsync(
            Guid legalEntityId,
            AccChartOfAccount? induk,
            AccountType jenis,
            CancellationToken ct)
        {
            var kodeSebadan = await _db.Set<AccChartOfAccount>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.LegalEntityId == legalEntityId)
                .Select(x => x.AccountCode)
                .ToListAsync(ct);

            return UsulkanKode(induk?.AccountCode, jenis, kodeSebadan);
        }

        private static (string? Kode, string? Galat) UsulkanKode(
            string? kodeInduk,
            AccountType jenis,
            IReadOnlyCollection<string> kodeSebadan)
        {
            if (kodeInduk is null)
            {
                var akar = $"{((int)jenis).ToString(CultureInfo.InvariantCulture)}-0000";

                return kodeSebadan.Contains(akar)
                    ? (null, $"Akun tingkat 1 untuk jenis {NamaJenis(jenis)} sudah ada ({akar}). Tambahkan akun di bawahnya.")
                    : (akar, null);
            }

            if (PolaKodeAkar.IsMatch(kodeInduk))
            {
                var awalan = kodeInduk[..2];
                var terbesar = NomorTerbesar(kodeSebadan, $"^{Regex.Escape(awalan)}([1-9])000$");

                return terbesar >= 9
                    ? (null, KodePenuh(kodeInduk, $"{awalan}9000"))
                    : ($"{awalan}{(terbesar + 1).ToString(CultureInfo.InvariantCulture)}000", null);
            }

            if (PolaKodeGrup.IsMatch(kodeInduk))
            {
                var awalan = kodeInduk[..3];
                var terbesar = NomorTerbesar(kodeSebadan, $"^{Regex.Escape(awalan)}([0-9]{{3}})$");

                return terbesar >= 999
                    ? (null, KodePenuh(kodeInduk, $"{awalan}999"))
                    : ($"{awalan}{(terbesar + 1).ToString("000", CultureInfo.InvariantCulture)}", null);
            }

            var terbesarAnak = NomorTerbesar(kodeSebadan, $"^{Regex.Escape(kodeInduk)}\\.([0-9]{{2}})$");

            if (terbesarAnak >= 99) return (null, KodePenuh(kodeInduk, $"{kodeInduk}.99"));

            var kode = $"{kodeInduk}.{(terbesarAnak + 1).ToString("00", CultureInfo.InvariantCulture)}";

            return kode.Length > PanjangKodeMaksimum
                ? (null, $"Kode induk {kodeInduk} terlalu panjang untuk diberi turunan otomatis.")
                : (kode, null);
        }

        private static int NomorTerbesar(IEnumerable<string> kodeSebadan, string pola)
        {
            var regex = new Regex(pola, RegexOptions.CultureInvariant);

            return kodeSebadan
                .Select(x => regex.Match(x))
                .Where(x => x.Success)
                .Select(x => int.Parse(x.Groups[1].Value, CultureInfo.InvariantCulture))
                .DefaultIfEmpty(0)
                .Max();
        }

        private static string KodePenuh(string kodeInduk, string batas)
            => $"Nomor akun di bawah {kodeInduk} sudah penuh ({batas}). Gunakan induk lain.";

        private static bool MelanggarIndexKodeAkun(DbUpdateException exception)
        {
            var postgresException = exception.InnerException as PostgresException;

            return postgresException?.SqlState == PostgresErrorCodes.UniqueViolation
                   && string.Equals(
                       postgresException.ConstraintName,
                       NamaIndexKodeAkun,
                       StringComparison.Ordinal);
        }
    }
}
