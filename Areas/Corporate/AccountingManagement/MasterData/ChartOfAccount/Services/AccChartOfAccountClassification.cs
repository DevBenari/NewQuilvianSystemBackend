using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Services
{
    public static class AccChartOfAccountClassification
    {
        public sealed class AkunKlasifikasi
        {
            public Guid Id { get; set; }

            public Guid? ParentAccountId { get; set; }

            public string AccountCode { get; set; } = string.Empty;

            public string AccountName { get; set; } = string.Empty;

            public AccountType AccountType { get; set; }

            public bool IsPostable { get; set; }

            public AccountReportClassification? ReportClassification { get; set; }

            public CurrentAssetCategory? CurrentAssetCategory { get; set; }
        }

        public sealed class KlasifikasiEfektif
        {
            public AccountReportClassification? ReportClassification { get; set; }

            public Guid? ReportClassificationSourceAccountId { get; set; }

            public string? ReportClassificationSourceAccountCode { get; set; }

            public CurrentAssetCategory? CurrentAssetCategory { get; set; }

            public Guid? CurrentAssetCategorySourceAccountId { get; set; }

            public string? CurrentAssetCategorySourceAccountCode { get; set; }

            public ClassificationGapKind? Gap { get; set; }
        }

        public static bool BerlakuUntuk(AccountType jenis)
            => jenis is AccountType.Asset or AccountType.Liability;

        public static Task<List<AkunKlasifikasi>> MuatAsync(
            ApplicationDbContext db,
            Guid legalEntityId,
            CancellationToken ct = default)
        {
            return db.Set<AccChartOfAccount>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.LegalEntityId == legalEntityId)
                .Select(x => new AkunKlasifikasi
                {
                    Id = x.Id,
                    ParentAccountId = x.ParentAccountId,
                    AccountCode = x.AccountCode,
                    AccountName = x.AccountName,
                    AccountType = x.AccountType,
                    IsPostable = x.IsPostable,
                    ReportClassification = x.ReportClassification,
                    CurrentAssetCategory = x.CurrentAssetCategory
                })
                .ToListAsync(ct);
        }

        public static Dictionary<Guid, KlasifikasiEfektif> Hitung(IEnumerable<AkunKlasifikasi> akun)
        {
            var peta = akun.GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.First());

            return peta.Values.ToDictionary(x => x.Id, x => HitungSatu(x, peta));
        }

        public static KlasifikasiEfektif HitungSatu(
            AkunKlasifikasi akun,
            IReadOnlyDictionary<Guid, AkunKlasifikasi> peta)
        {
            var hasil = new KlasifikasiEfektif();

            if (!BerlakuUntuk(akun.AccountType)) return hasil;

            AkunKlasifikasi? sumberKlasifikasi = null;
            AkunKlasifikasi? sumberSub = null;
            var dikunjungi = new HashSet<Guid>();
            var penelusur = akun;

            while (penelusur is not null
                   && penelusur.AccountType == akun.AccountType
                   && dikunjungi.Add(penelusur.Id))
            {
                if (sumberKlasifikasi is null && penelusur.ReportClassification.HasValue)
                    sumberKlasifikasi = penelusur;

                if (sumberSub is null && penelusur.CurrentAssetCategory.HasValue)
                    sumberSub = penelusur;

                if (sumberKlasifikasi is not null && sumberSub is not null) break;

                penelusur = penelusur.ParentAccountId.HasValue
                    && peta.TryGetValue(penelusur.ParentAccountId.Value, out var induk)
                        ? induk
                        : null;
            }

            if (sumberKlasifikasi is not null)
            {
                hasil.ReportClassification = sumberKlasifikasi.ReportClassification;
                hasil.ReportClassificationSourceAccountId = sumberKlasifikasi.Id;
                hasil.ReportClassificationSourceAccountCode = sumberKlasifikasi.AccountCode;
            }

            var lancarAset = akun.AccountType == AccountType.Asset
                             && hasil.ReportClassification == AccountReportClassification.Current;

            if (lancarAset && sumberSub is not null)
            {
                hasil.CurrentAssetCategory = sumberSub.CurrentAssetCategory;
                hasil.CurrentAssetCategorySourceAccountId = sumberSub.Id;
                hasil.CurrentAssetCategorySourceAccountCode = sumberSub.AccountCode;
            }

            if (akun.IsPostable)
            {
                hasil.Gap = hasil.ReportClassification is null
                    ? ClassificationGapKind.MissingReportClassification
                    : lancarAset && hasil.CurrentAssetCategory is null
                        ? ClassificationGapKind.MissingCurrentAssetCategory
                        : null;
            }

            return hasil;
        }
    }
}
