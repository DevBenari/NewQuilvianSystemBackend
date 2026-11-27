using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.Services;
using static QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Services.AccChartOfAccountClassification;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Services
{
    public partial class AccChartOfAccountService
    {
        private const int BatasKodeTurunanDisebut = 5;

        public sealed class PerubahanKlasifikasi
        {
            public Guid AccountId { get; set; }

            public string AccountCode { get; set; } = string.Empty;

            public AccountReportClassification? ReportClassificationBefore { get; set; }

            public CurrentAssetCategory? CurrentAssetCategoryBefore { get; set; }

            public AccountReportClassification? ReportClassificationAfter { get; set; }

            public CurrentAssetCategory? CurrentAssetCategoryAfter { get; set; }

            public bool Changed => ReportClassificationBefore != ReportClassificationAfter
                                   || CurrentAssetCategoryBefore != CurrentAssetCategoryAfter;
        }

        public PerubahanKlasifikasi? PerubahanKlasifikasiTerakhir { get; private set; }

        private static AccountingServiceResult<T>? PeriksaIsianKlasifikasi<T>(
            AccountType jenis,
            ChartOfAccountClassificationRequest? klasifikasi)
        {
            if (klasifikasi is null) return null;

            if ((klasifikasi.ReportClassification.HasValue && !Enum.IsDefined(klasifikasi.ReportClassification.Value))
                || (klasifikasi.CurrentAssetCategory.HasValue && !Enum.IsDefined(klasifikasi.CurrentAssetCategory.Value)))
            {
                return AccountingServiceResult<T>.Fail(
                    StatusCodes.Status400BadRequest, "Klasifikasi laporan tidak dikenal.");
            }

            if (klasifikasi.ReportClassification.HasValue && !BerlakuUntuk(jenis))
            {
                return AccountingServiceResult<T>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Klasifikasi Lancar/Tidak Lancar hanya untuk akun Aset dan Liabilitas.");
            }

            if (klasifikasi.CurrentAssetCategory.HasValue && jenis != AccountType.Asset)
            {
                return AccountingServiceResult<T>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Sub-klasifikasi aset lancar hanya untuk akun Aset.");
            }

            return null;
        }

        private async Task<AccountingServiceResult<T>?> PeriksaKonsistensiKlasifikasiAsync<T>(
            Guid legalEntityId,
            AkunKlasifikasi target,
            CancellationToken ct)
        {
            var semua = await MuatAsync(_db, legalEntityId, ct);
            semua.RemoveAll(x => x.Id == target.Id);
            semua.Add(target);

            var peta = semua.ToDictionary(x => x.Id);
            var efektifTarget = HitungSatu(target, peta);

            if (target.CurrentAssetCategory.HasValue
                && efektifTarget.ReportClassification != AccountReportClassification.Current)
            {
                var keadaan = efektifTarget.ReportClassification == AccountReportClassification.NonCurrent
                    ? "Tidak Lancar"
                    : "belum diisi";

                return AccountingServiceResult<T>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Sub-klasifikasi aset lancar hanya untuk akun berklasifikasi Lancar. Klasifikasi akun ini: {keadaan}.");
            }

            var anakPerInduk = semua
                .Where(x => x.ParentAccountId.HasValue)
                .GroupBy(x => x.ParentAccountId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            var bermasalah = new List<string>();
            var dikunjungi = new HashSet<Guid> { target.Id };
            var antrean = new Queue<Guid>();
            antrean.Enqueue(target.Id);

            while (antrean.Count > 0)
            {
                if (!anakPerInduk.TryGetValue(antrean.Dequeue(), out var anak)) continue;

                foreach (var turunan in anak.Where(x => dikunjungi.Add(x.Id)))
                {
                    antrean.Enqueue(turunan.Id);

                    if (turunan.CurrentAssetCategory.HasValue
                        && HitungSatu(turunan, peta).ReportClassification != AccountReportClassification.Current)
                    {
                        bermasalah.Add(turunan.AccountCode);
                    }
                }
            }

            if (bermasalah.Count == 0) return null;

            var disebut = bermasalah.OrderBy(x => x, StringComparer.Ordinal).Take(BatasKodeTurunanDisebut).ToList();
            var sisa = bermasalah.Count - disebut.Count;
            var daftar = string.Join(", ", disebut) + (sisa > 0 ? $" dan {sisa} akun lain" : string.Empty);

            return AccountingServiceResult<T>.Fail(
                StatusCodes.Status409Conflict,
                $"Klasifikasi tidak dapat diubah karena turunan {daftar} memakai sub-klasifikasi aset lancar. Ubah atau kosongkan sub-klasifikasi turunan lebih dahulu.");
        }

        private async Task KunciKlasifikasiAsync(Guid legalEntityId, CancellationToken ct)
        {
            if (!_db.Database.IsNpgsql()) return;

            await _db.Database.ExecuteSqlRawAsync(
                "SELECT pg_advisory_xact_lock(hashtext({0}));",
                [$"ACC_COA_KLASIFIKASI_{legalEntityId:N}"],
                ct);
        }

        private static void IsiKlasifikasi(ChartOfAccountListResponse baris, KlasifikasiEfektif? efektif)
        {
            if (efektif is null) return;

            baris.EffectiveReportClassification = efektif.ReportClassification;
            baris.EffectiveCurrentAssetCategory = efektif.CurrentAssetCategory;
            baris.ClassificationGap = efektif.Gap;

            if (baris is not ChartOfAccountDetailResponse rincian) return;

            rincian.ReportClassificationSourceAccountId = efektif.ReportClassificationSourceAccountId;
            rincian.ReportClassificationSourceAccountCode = efektif.ReportClassificationSourceAccountCode;
            rincian.CurrentAssetCategorySourceAccountId = efektif.CurrentAssetCategorySourceAccountId;
            rincian.CurrentAssetCategorySourceAccountCode = efektif.CurrentAssetCategorySourceAccountCode;
        }

        private async Task IsiKlasifikasiAsync(
            IReadOnlyCollection<ChartOfAccountListResponse> baris,
            CancellationToken ct)
        {
            foreach (var badanHukum in baris.Select(x => x.LegalEntityId).Distinct().ToList())
            {
                var efektif = Hitung(await MuatAsync(_db, badanHukum, ct));

                foreach (var item in baris.Where(x => x.LegalEntityId == badanHukum))
                    IsiKlasifikasi(item, efektif.GetValueOrDefault(item.Id));
            }
        }

        private static void IsiKlasifikasiPohon(
            ChartOfAccountTreeResponse simpul,
            AkunKlasifikasi sumber,
            KlasifikasiEfektif? efektif)
        {
            simpul.ReportClassification = sumber.ReportClassification;
            simpul.CurrentAssetCategory = sumber.CurrentAssetCategory;
            simpul.EffectiveReportClassification = efektif?.ReportClassification;
            simpul.EffectiveCurrentAssetCategory = efektif?.CurrentAssetCategory;
            simpul.ClassificationGap = efektif?.Gap;
        }
    }
}
