using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.GeneralLedger.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.GeneralLedger.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Services;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.Services;
using System.Globalization;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.GeneralLedger.Services
{
    public partial class AccGeneralLedgerService
    {
        private const int BatasAkunDisebut = 5;

        private static readonly CultureInfo BudayaRupiah = CultureInfo.GetCultureInfo("id-ID");

        private sealed class RentangTahunBuku
        {
            public DateTime Awal { get; init; }

            public DateTime Akhir { get; init; }
        }

        private sealed class PosisiRasio
        {
            public DateTime Tanggal { get; init; }

            public decimal Aset { get; set; }

            public decimal Liabilitas { get; set; }

            public decimal Ekuitas { get; set; }

            public decimal AsetLancar { get; set; }

            public decimal Kas { get; set; }

            public decimal InvestasiJangkaPendek { get; set; }

            public decimal Piutang { get; set; }

            public decimal LiabilitasLancar { get; set; }

            public List<AkunLaporan> TanpaKlasifikasi { get; } = new();

            public List<AkunLaporan> TanpaSubKlasifikasi { get; } = new();

            public decimal Selisih => Aset - (Liabilitas + Ekuitas);

            public bool Seimbang => Selisih == 0m;
        }

        private sealed class LabaRasio
        {
            public decimal Pendapatan { get; init; }

            public decimal LabaBersih { get; init; }
        }

        private enum KebutuhanKlasifikasi
        {
            Tidak,
            Klasifikasi,
            SubKlasifikasi
        }

        private sealed class DefinisiRasio
        {
            public FinancialRatioCode Kode { get; init; }

            public string Nama { get; init; } = string.Empty;

            public string Rumus { get; init; } = string.Empty;

            public FinancialRatioUnit Satuan { get; init; }

            public KebutuhanKlasifikasi Klasifikasi { get; init; }

            public bool MemakaiNeraca { get; init; }

            public bool MemakaiSaldoAwal { get; init; }

            public bool CatatEkuitasNegatif { get; init; }

            public Func<PosisiRasio, PosisiRasio?, LabaRasio?, decimal?> Pembilang { get; init; } = (_, _, _) => null;

            public Func<PosisiRasio, PosisiRasio?, LabaRasio?, decimal?> Penyebut { get; init; } = (_, _, _) => null;

            public Func<PosisiRasio, decimal>? PenyebutPosisi { get; init; }
        }

        private static readonly IReadOnlyList<DefinisiRasio> DaftarRasio = new[]
        {
            new DefinisiRasio
            {
                Kode = FinancialRatioCode.CurrentRatio,
                Nama = "Current Ratio",
                Rumus = "Aset Lancar ÷ Liabilitas Lancar",
                Satuan = FinancialRatioUnit.Times,
                Klasifikasi = KebutuhanKlasifikasi.Klasifikasi,
                MemakaiNeraca = true,
                Pembilang = (p, _, _) => p.AsetLancar,
                Penyebut = (p, _, _) => p.LiabilitasLancar
            },
            new DefinisiRasio
            {
                Kode = FinancialRatioCode.QuickRatio,
                Nama = "Quick Ratio",
                Rumus = "(Kas dan Setara Kas + Investasi Jangka Pendek + Piutang) ÷ Liabilitas Lancar",
                Satuan = FinancialRatioUnit.Times,
                Klasifikasi = KebutuhanKlasifikasi.SubKlasifikasi,
                MemakaiNeraca = true,
                Pembilang = (p, _, _) => p.Kas + p.InvestasiJangkaPendek + p.Piutang,
                Penyebut = (p, _, _) => p.LiabilitasLancar
            },
            new DefinisiRasio
            {
                Kode = FinancialRatioCode.CashRatio,
                Nama = "Cash Ratio",
                Rumus = "Kas dan Setara Kas ÷ Liabilitas Lancar",
                Satuan = FinancialRatioUnit.Times,
                Klasifikasi = KebutuhanKlasifikasi.SubKlasifikasi,
                MemakaiNeraca = true,
                Pembilang = (p, _, _) => p.Kas,
                Penyebut = (p, _, _) => p.LiabilitasLancar
            },
            new DefinisiRasio
            {
                Kode = FinancialRatioCode.NetProfitMargin,
                Nama = "Net Profit Margin",
                Rumus = "Laba Bersih sejak awal tahun buku ÷ Total Pendapatan sejak awal tahun buku",
                Satuan = FinancialRatioUnit.Percent,
                Pembilang = (_, _, l) => l?.LabaBersih,
                Penyebut = (_, _, l) => l?.Pendapatan
            },
            new DefinisiRasio
            {
                Kode = FinancialRatioCode.ReturnOnAssets,
                Nama = "Return on Assets",
                Rumus = "Laba Bersih sejak awal tahun buku ÷ rata-rata Total Aset awal tahun buku dan akhir periode",
                Satuan = FinancialRatioUnit.Percent,
                MemakaiNeraca = true,
                MemakaiSaldoAwal = true,
                Pembilang = (_, _, l) => l?.LabaBersih,
                Penyebut = (p, a, _) => a is null ? null : (a.Aset + p.Aset) / 2m,
                PenyebutPosisi = p => p.Aset
            },
            new DefinisiRasio
            {
                Kode = FinancialRatioCode.ReturnOnEquity,
                Nama = "Return on Equity",
                Rumus = "Laba Bersih sejak awal tahun buku ÷ rata-rata Total Ekuitas awal tahun buku dan akhir periode",
                Satuan = FinancialRatioUnit.Percent,
                MemakaiNeraca = true,
                MemakaiSaldoAwal = true,
                CatatEkuitasNegatif = true,
                Pembilang = (_, _, l) => l?.LabaBersih,
                Penyebut = (p, a, _) => a is null ? null : (a.Ekuitas + p.Ekuitas) / 2m,
                PenyebutPosisi = p => p.Ekuitas
            },
            new DefinisiRasio
            {
                Kode = FinancialRatioCode.DebtToAssetRatio,
                Nama = "Debt to Asset Ratio",
                Rumus = "Total Liabilitas ÷ Total Aset",
                Satuan = FinancialRatioUnit.Percent,
                MemakaiNeraca = true,
                Pembilang = (p, _, _) => p.Liabilitas,
                Penyebut = (p, _, _) => p.Aset
            },
            new DefinisiRasio
            {
                Kode = FinancialRatioCode.DebtToEquityRatio,
                Nama = "Debt to Equity Ratio",
                Rumus = "Total Liabilitas ÷ Total Ekuitas",
                Satuan = FinancialRatioUnit.Times,
                MemakaiNeraca = true,
                CatatEkuitasNegatif = true,
                Pembilang = (p, _, _) => p.Liabilitas,
                Penyebut = (p, _, _) => p.Ekuitas
            }
        };

        public async Task<AccountingServiceResult<FinancialRatioResponse>> GetFinancialRatiosAsync(
            FinancialRatioQuery query,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard.PeriksaAsync<FinancialRatioResponse>(_db, ct);
            if (penjaga is not null) return penjaga;

            var siap = await SiapkanPeriodeLaporanAsync<FinancialRatioResponse>(
                query.LegalEntityId, query.PeriodCode, ct);
            if (siap.Gagal is not null) return siap.Gagal;

            var periode = siap.Periode!;
            var tahun = periode.FiscalYear;

            var periodeTigaTahun = await _db.Set<AccAccountingPeriod>()
                .AsNoTracking()
                .Where(x => !x.IsDelete
                            && x.LegalEntityId == query.LegalEntityId
                            && x.FiscalYear >= tahun - 2
                            && x.FiscalYear <= tahun)
                .ToListAsync(ct);

            RentangTahunBuku? Rentang(int t)
            {
                var daftar = periodeTigaTahun.Where(x => x.FiscalYear == t).ToList();
                if (daftar.Count == 0) return null;

                return new RentangTahunBuku
                {
                    Awal = daftar.Min(x => x.StartDate),
                    Akhir = daftar.Max(x => x.EndDate)
                };
            }

            var tahunIni = Rentang(tahun)!;
            var tahunLalu = Rentang(tahun - 1);
            var duaTahunLalu = Rentang(tahun - 2);

            var akun = await MuatAkunLaporanAsync(query.LegalEntityId, ct);

            var efektif = AccChartOfAccountClassification.Hitung(akun.Select(x => new AccChartOfAccountClassification.AkunKlasifikasi
            {
                Id = x.Id,
                ParentAccountId = x.ParentAccountId,
                AccountCode = x.AccountCode,
                AccountName = x.AccountName,
                AccountType = x.AccountType,
                IsPostable = x.IsPostable,
                ReportClassification = x.ReportClassification,
                CurrentAssetCategory = x.CurrentAssetCategory
            }));

            var idAkunNeraca = akun
                .Where(x => x.AccountType is AccountType.Asset or AccountType.Liability or AccountType.Equity)
                .Select(x => x.Id)
                .ToList();

            var idAkunLabaRugi = akun
                .Where(x => x.AccountType is AccountType.Revenue or AccountType.Expense)
                .Select(x => x.Id)
                .ToList();

            var dasar = BarisDisahkan(query.LegalEntityId);
            var barisNeraca = dasar.Where(x => idAkunNeraca.Contains(x.AccountId));
            var barisLabaRugi = dasar.Where(x => idAkunLabaRugi.Contains(x.AccountId));
            var barisLabaTanpaTutup = (await BarisTanpaTutupTahunAsync(query.LegalEntityId, ct))
                .Where(x => idAkunLabaRugi.Contains(x.AccountId));

            var posisiKini = await HitungPosisiRasioAsync(
                akun, efektif, barisNeraca, barisLabaRugi, tahunIni.Awal, periode.EndDate, ct);

            PosisiRasio? posisiAkhirLalu = null;
            PosisiRasio? posisiAkhirDuaLalu = null;
            LabaRasio? labaLalu = null;

            if (tahunLalu is not null)
            {
                posisiAkhirLalu = await HitungPosisiRasioAsync(
                    akun, efektif, barisNeraca, barisLabaRugi, tahunLalu.Awal, tahunLalu.Akhir, ct);
                labaLalu = await HitungLabaRasioAsync(akun, barisLabaTanpaTutup, tahunLalu.Awal, tahunLalu.Akhir, ct);
            }

            if (tahunLalu is not null && duaTahunLalu is not null)
            {
                posisiAkhirDuaLalu = await HitungPosisiRasioAsync(
                    akun, efektif, barisNeraca, barisLabaRugi, duaTahunLalu.Awal, duaTahunLalu.Akhir, ct);
            }

            var labaKini = await HitungLabaRasioAsync(akun, barisLabaTanpaTutup, tahunIni.Awal, periode.EndDate, ct);

            var alasanPembanding = tahunLalu is null
                ? $"Tahun buku {(tahun - 1).ToString(CultureInfo.InvariantCulture)} belum ada di sistem."
                : null;

            var alasanSaldoAwalKini = tahunLalu is null
                ? AlasanSaldoAwal(tahun)
                : null;

            var alasanSaldoAwalBanding = duaTahunLalu is null
                ? AlasanSaldoAwal(tahun - 1)
                : null;

            var rasio = DaftarRasio
                .Select(d => new FinancialRatioRowResponse
                {
                    Code = d.Kode,
                    Name = d.Nama,
                    Formula = d.Rumus,
                    Unit = d.Satuan,
                    Current = HitungNilaiRasio(d, posisiKini, posisiAkhirLalu, labaKini, alasanSaldoAwalKini),
                    Comparison = posisiAkhirLalu is null
                        ? new FinancialRatioValueResponse
                        {
                            Status = FinancialRatioStatus.Unavailable,
                            Reason = alasanPembanding
                        }
                        : HitungNilaiRasio(d, posisiAkhirLalu, posisiAkhirDuaLalu, labaLalu, alasanSaldoAwalBanding)
                })
                .ToList();

            var hasil = new FinancialRatioResponse
            {
                LegalEntityId = query.LegalEntityId,
                PeriodCode = periode.PeriodCode,
                PeriodName = NamaPeriode(periode),
                FiscalYear = tahun,
                AsOfDate = periode.EndDate,
                YearToDateStartDate = tahunIni.Awal,
                ComparisonAvailable = tahunLalu is not null,
                ComparisonAsOfDate = tahunLalu?.Akhir,
                ComparisonUnavailableReason = alasanPembanding,
                IsBalanced = posisiKini.Seimbang,
                ComparisonIsBalanced = posisiAkhirLalu?.Seimbang,
                ClassificationGaps = CelahPosisi(posisiKini),
                ComparisonClassificationGaps = posisiAkhirLalu is null
                    ? new List<FinancialStatementClassificationGapResponse>()
                    : CelahPosisi(posisiAkhirLalu),
                Ratios = rasio
            };

            return AccountingServiceResult<FinancialRatioResponse>.Ok(
                hasil, $"Rasio keuangan {hasil.PeriodName} berhasil diambil.");
        }

        private static string AlasanSaldoAwal(int tahun)
            => $"Saldo awal tahun buku {tahun.ToString(CultureInfo.InvariantCulture)} tidak tersedia: " +
               $"tahun buku {(tahun - 1).ToString(CultureInfo.InvariantCulture)} belum ada di sistem.";

        private async Task<PosisiRasio> HitungPosisiRasioAsync(
            IReadOnlyList<AkunLaporan> akun,
            IReadOnlyDictionary<Guid, AccChartOfAccountClassification.KlasifikasiEfektif> efektif,
            IQueryable<AccJournalLine> barisNeraca,
            IQueryable<AccJournalLine> barisLabaRugi,
            DateTime awalTahun,
            DateTime sampai,
            CancellationToken ct)
        {
            var jumlah = await JumlahPerAkunAsync(barisNeraca, null, sampai, ct);
            var laba = await HitungLabaBelumDipindahAsync(barisLabaRugi, awalTahun, sampai, ct);

            var posisi = new PosisiRasio { Tanggal = sampai };

            foreach (var a in akun)
            {
                if (a.AccountType is not (AccountType.Asset or AccountType.Liability or AccountType.Equity)) continue;

                var nilai = NilaiMenurutJenis(a.AccountType, jumlah, a.Id);

                if (a.AccountType == AccountType.Equity)
                {
                    posisi.Ekuitas += nilai;
                    continue;
                }

                if (a.AccountType == AccountType.Asset) posisi.Aset += nilai;
                else posisi.Liabilitas += nilai;

                if (nilai == 0m || !efektif.TryGetValue(a.Id, out var klasifikasi)) continue;

                if (klasifikasi.Gap == ClassificationGapKind.MissingReportClassification)
                {
                    posisi.TanpaKlasifikasi.Add(a);
                    continue;
                }

                if (klasifikasi.Gap == ClassificationGapKind.MissingCurrentAssetCategory)
                    posisi.TanpaSubKlasifikasi.Add(a);

                if (klasifikasi.ReportClassification != AccountReportClassification.Current) continue;

                if (a.AccountType == AccountType.Liability)
                {
                    posisi.LiabilitasLancar += nilai;
                    continue;
                }

                posisi.AsetLancar += nilai;

                switch (klasifikasi.CurrentAssetCategory)
                {
                    case CurrentAssetCategory.CashAndCashEquivalents:
                        posisi.Kas += nilai;
                        break;
                    case CurrentAssetCategory.ShortTermInvestment:
                        posisi.InvestasiJangkaPendek += nilai;
                        break;
                    case CurrentAssetCategory.Receivable:
                        posisi.Piutang += nilai;
                        break;
                }
            }

            posisi.Ekuitas += laba.TahunBerjalan + laba.TahunLalu;

            return posisi;
        }

        private async Task<LabaRasio> HitungLabaRasioAsync(
            IReadOnlyList<AkunLaporan> akun,
            IQueryable<AccJournalLine> barisLabaTanpaTutup,
            DateTime dari,
            DateTime sampai,
            CancellationToken ct)
        {
            var jumlah = await JumlahPerAkunAsync(barisLabaTanpaTutup, dari, sampai, ct);

            var pendapatan = akun
                .Where(x => x.AccountType == AccountType.Revenue)
                .Sum(x => NilaiMenurutJenis(x.AccountType, jumlah, x.Id));

            var beban = akun
                .Where(x => x.AccountType == AccountType.Expense)
                .Sum(x => NilaiMenurutJenis(x.AccountType, jumlah, x.Id));

            return new LabaRasio { Pendapatan = pendapatan, LabaBersih = pendapatan - beban };
        }

        private static FinancialRatioValueResponse HitungNilaiRasio(
            DefinisiRasio definisi,
            PosisiRasio posisi,
            PosisiRasio? posisiAwal,
            LabaRasio? laba,
            string? alasanSaldoAwal)
        {
            if (definisi.MemakaiSaldoAwal && posisiAwal is null)
            {
                return new FinancialRatioValueResponse
                {
                    Status = FinancialRatioStatus.Unavailable,
                    Numerator = definisi.Pembilang(posisi, posisiAwal, laba),
                    DenominatorClosing = definisi.PenyebutPosisi?.Invoke(posisi),
                    Reason = alasanSaldoAwal
                };
            }

            if (definisi.Klasifikasi != KebutuhanKlasifikasi.Tidak && posisi.TanpaKlasifikasi.Count > 0)
            {
                return new FinancialRatioValueResponse
                {
                    Status = FinancialRatioStatus.Unavailable,
                    Reason = $"Klasifikasi laporan belum lengkap pada {posisi.TanpaKlasifikasi.Count} akun bersaldo: " +
                             $"{DaftarAkunDisebut(posisi.TanpaKlasifikasi)}."
                };
            }

            if (definisi.Klasifikasi == KebutuhanKlasifikasi.SubKlasifikasi && posisi.TanpaSubKlasifikasi.Count > 0)
            {
                return new FinancialRatioValueResponse
                {
                    Status = FinancialRatioStatus.Unavailable,
                    Reason = $"Sub-klasifikasi aset lancar belum diisi pada {posisi.TanpaSubKlasifikasi.Count} akun bersaldo: " +
                             $"{DaftarAkunDisebut(posisi.TanpaSubKlasifikasi)}."
                };
            }

            var pembilang = definisi.Pembilang(posisi, posisiAwal, laba);
            var penyebut = definisi.Penyebut(posisi, posisiAwal, laba);

            var nilai = new FinancialRatioValueResponse
            {
                Numerator = pembilang,
                Denominator = penyebut,
                DenominatorOpening = definisi.MemakaiSaldoAwal ? definisi.PenyebutPosisi?.Invoke(posisiAwal!) : null,
                DenominatorClosing = definisi.MemakaiSaldoAwal ? definisi.PenyebutPosisi?.Invoke(posisi) : null,
                Note = definisi.CatatEkuitasNegatif && penyebut < 0m
                    ? "Ekuitas negatif; tafsirkan dengan hati-hati."
                    : null
            };

            var tidakSeimbang = !definisi.MemakaiNeraca
                ? null
                : !posisi.Seimbang
                    ? posisi
                    : definisi.MemakaiSaldoAwal && posisiAwal is not null && !posisiAwal.Seimbang
                        ? posisiAwal
                        : null;

            if (tidakSeimbang is not null)
            {
                nilai.Status = FinancialRatioStatus.Invalid;
                nilai.Reason = $"Neraca per {TanggalPanjang(tidakSeimbang.Tanggal)} tidak seimbang, " +
                               $"selisih {Rupiah(tidakSeimbang.Selisih)}. Rasio ini tidak dapat dipakai.";
                return nilai;
            }

            if (pembilang is null || penyebut is null || penyebut == 0m)
            {
                nilai.Status = FinancialRatioStatus.NotApplicable;
                nilai.Reason = "Penyebut bernilai nol.";
                return nilai;
            }

            var pengali = definisi.Satuan == FinancialRatioUnit.Percent ? 100m : 1m;

            nilai.Status = FinancialRatioStatus.Calculated;
            nilai.Value = Math.Round(pembilang.Value / penyebut.Value * pengali, 2, MidpointRounding.AwayFromZero);

            return nilai;
        }

        private static List<FinancialStatementClassificationGapResponse> CelahPosisi(PosisiRasio posisi)
        {
            return posisi.TanpaKlasifikasi
                .Select(x => (Akun: x, Jenis: ClassificationGapKind.MissingReportClassification))
                .Concat(posisi.TanpaSubKlasifikasi
                    .Select(x => (Akun: x, Jenis: ClassificationGapKind.MissingCurrentAssetCategory)))
                .OrderBy(x => x.Akun.AccountCode, StringComparer.Ordinal)
                .Select(x => new FinancialStatementClassificationGapResponse
                {
                    AccountId = x.Akun.Id,
                    AccountCode = x.Akun.AccountCode,
                    AccountName = x.Akun.AccountName,
                    AccountType = x.Akun.AccountType,
                    GapKind = x.Jenis
                })
                .ToList();
        }

        private static string DaftarAkunDisebut(IReadOnlyCollection<AkunLaporan> akun)
        {
            var disebut = akun
                .OrderBy(x => x.AccountCode, StringComparer.Ordinal)
                .Take(BatasAkunDisebut)
                .Select(x => $"{x.AccountCode} {x.AccountName}")
                .ToList();

            var sisa = akun.Count - disebut.Count;

            return sisa > 0
                ? $"{string.Join(", ", disebut)}, dan {sisa.ToString(CultureInfo.InvariantCulture)} akun lain"
                : string.Join(", ", disebut);
        }

        private static string TanggalPanjang(DateTime tanggal)
            => $"{tanggal.Day.ToString(CultureInfo.InvariantCulture)} {NamaBulan[tanggal.Month - 1]} " +
               tanggal.Year.ToString(CultureInfo.InvariantCulture);

        private static string Rupiah(decimal nilai)
            => $"Rp {Math.Abs(nilai).ToString("#,##0.##", BudayaRupiah)}";
    }
}
