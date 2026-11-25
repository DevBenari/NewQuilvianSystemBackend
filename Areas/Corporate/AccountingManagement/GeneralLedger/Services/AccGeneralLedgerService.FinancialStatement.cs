using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingPeriod.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.GeneralLedger.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.GeneralLedger.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.JournalType.Services;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.GeneralLedger.Services
{
    public partial class AccGeneralLedgerService
    {
        private const string PemberitahuanUnitBiayaLabaRugi =
            "Hanya baris jurnal yang mencatat unit biaya ini yang dihitung. Pendapatan dan beban yang dicatat tanpa unit biaya tidak ikut.";

        private const string NamaLabaTahunBerjalan = "Laba (Rugi) Tahun Berjalan";

        private const string NamaLabaTahunLaluBelumDitutup = "Laba (Rugi) Tahun Lalu Belum Ditutup";

        private static readonly Regex BentukKodePeriode =
            new(@"^\d{4}-(0[1-9]|1[0-2])$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private sealed class AkunLaporan
        {
            public Guid Id { get; init; }

            public Guid? ParentAccountId { get; init; }

            public string AccountCode { get; init; } = string.Empty;

            public string AccountName { get; init; } = string.Empty;

            public AccountType AccountType { get; init; }

            public bool IsActive { get; init; }
        }

        private sealed class LabaBelumDipindah
        {
            public decimal TahunBerjalan { get; init; }

            public decimal TahunLalu { get; init; }

            public List<int> TahunBelumDitutup { get; init; } = new();
        }

        private sealed class NilaiLaporan
        {
            public decimal Kini { get; set; }

            public decimal? Banding { get; set; }

            public decimal? KiniYtd { get; set; }

            public decimal? BandingYtd { get; set; }

            public bool SemuaNol => Kini == 0m
                                    && (Banding ?? 0m) == 0m
                                    && (KiniYtd ?? 0m) == 0m
                                    && (BandingYtd ?? 0m) == 0m;

            public static NilaiLaporan Kosong(bool adaBanding, bool adaYtd) => new()
            {
                Kini = 0m,
                Banding = adaBanding ? 0m : null,
                KiniYtd = adaYtd ? 0m : null,
                BandingYtd = adaBanding && adaYtd ? 0m : null
            };

            public void Tambah(NilaiLaporan lain)
            {
                Kini += lain.Kini;
                if (Banding.HasValue) Banding += lain.Banding ?? 0m;
                if (KiniYtd.HasValue) KiniYtd += lain.KiniYtd ?? 0m;
                if (BandingYtd.HasValue) BandingYtd += lain.BandingYtd ?? 0m;
            }

            public static NilaiLaporan Selisih(NilaiLaporan a, NilaiLaporan b) => new()
            {
                Kini = a.Kini - b.Kini,
                Banding = a.Banding.HasValue && b.Banding.HasValue ? a.Banding - b.Banding : null,
                KiniYtd = a.KiniYtd.HasValue && b.KiniYtd.HasValue ? a.KiniYtd - b.KiniYtd : null,
                BandingYtd = a.BandingYtd.HasValue && b.BandingYtd.HasValue ? a.BandingYtd - b.BandingYtd : null
            };

            public FinancialStatementAmountsResponse KeRespons() => new()
            {
                Current = Kini,
                Comparison = Banding,
                Variance = Banding.HasValue ? Kini - Banding.Value : null,
                CurrentYearToDate = KiniYtd,
                ComparisonYearToDate = BandingYtd,
                VarianceYearToDate = KiniYtd.HasValue && BandingYtd.HasValue
                    ? KiniYtd.Value - BandingYtd.Value
                    : null
            };
        }

        public async Task<AccountingServiceResult<IncomeStatementResponse>> GetIncomeStatementAsync(
            IncomeStatementQuery query,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard.PeriksaAsync<IncomeStatementResponse>(_db, ct);
            if (penjaga is not null) return penjaga;

            var siap = await SiapkanPeriodeLaporanAsync<IncomeStatementResponse>(
                query.LegalEntityId, query.PeriodCode, ct);
            if (siap.Gagal is not null) return siap.Gagal;

            var periode = siap.Periode!;

            MstCostCenter? unitBiaya = null;

            if (query.CostCenterId.HasValue)
            {
                unitBiaya = await CariUnitBiayaAsync(query.LegalEntityId, query.CostCenterId.Value, ct);

                if (unitBiaya is null)
                {
                    return AccountingServiceResult<IncomeStatementResponse>.Fail(
                        StatusCodes.Status400BadRequest,
                        "Unit biaya tidak ditemukan pada badan hukum ini.");
                }
            }

            var periodeDuaTahun = await _db.Set<AccAccountingPeriod>()
                .AsNoTracking()
                .Where(x => !x.IsDelete
                            && x.LegalEntityId == query.LegalEntityId
                            && (x.FiscalYear == periode.FiscalYear || x.FiscalYear == periode.FiscalYear - 1))
                .ToListAsync(ct);

            var awalTahun = periodeDuaTahun
                .Where(x => x.FiscalYear == periode.FiscalYear)
                .Min(x => x.StartDate);

            var pembanding = periodeDuaTahun
                .FirstOrDefault(x => x.FiscalYear == periode.FiscalYear - 1
                                     && x.PeriodMonth == periode.PeriodMonth);

            var akun = await MuatAkunLaporanAsync(query.LegalEntityId, ct);

            var idAkunLabaRugi = akun
                .Where(x => x.AccountType is AccountType.Revenue or AccountType.Expense)
                .Select(x => x.Id)
                .ToList();

            var dasar = (await BarisTanpaTutupTahunAsync(query.LegalEntityId, ct))
                .Where(x => idAkunLabaRugi.Contains(x.AccountId));

            if (unitBiaya is not null)
            {
                var idUnit = unitBiaya.Id;
                dasar = dasar.Where(x => x.CostCenterId == idUnit);
            }

            var kini = await JumlahPerAkunAsync(dasar, periode.StartDate, periode.EndDate, ct);
            var kiniYtd = await JumlahPerAkunAsync(dasar, awalTahun, periode.EndDate, ct);

            Dictionary<Guid, (decimal Debit, decimal Kredit)>? banding = null;
            Dictionary<Guid, (decimal Debit, decimal Kredit)>? bandingYtd = null;

            if (pembanding is not null)
            {
                var awalTahunPembanding = periodeDuaTahun
                    .Where(x => x.FiscalYear == pembanding.FiscalYear)
                    .Min(x => x.StartDate);

                banding = await JumlahPerAkunAsync(dasar, pembanding.StartDate, pembanding.EndDate, ct);
                bandingYtd = await JumlahPerAkunAsync(dasar, awalTahunPembanding, pembanding.EndDate, ct);
            }

            var nilaiAkun = new Dictionary<Guid, NilaiLaporan>();

            foreach (var a in akun.Where(x => x.AccountType is AccountType.Revenue or AccountType.Expense))
            {
                nilaiAkun[a.Id] = new NilaiLaporan
                {
                    Kini = NilaiMenurutJenis(a.AccountType, kini, a.Id),
                    Banding = banding is null ? null : NilaiMenurutJenis(a.AccountType, banding, a.Id),
                    KiniYtd = NilaiMenurutJenis(a.AccountType, kiniYtd, a.Id),
                    BandingYtd = bandingYtd is null ? null : NilaiMenurutJenis(a.AccountType, bandingYtd, a.Id)
                };
            }

            var adaBanding = pembanding is not null;

            var pendapatan = SusunBagian(
                AccountType.Revenue, akun, nilaiAkun, query.IncludeZeroAccounts, adaBanding, adaYtd: true);
            var beban = SusunBagian(
                AccountType.Expense, akun, nilaiAkun, query.IncludeZeroAccounts, adaBanding, adaYtd: true);

            var namaPeriodePembanding =
                $"{NamaBulan[periode.PeriodMonth - 1]} {(periode.FiscalYear - 1).ToString(CultureInfo.InvariantCulture)}";

            var hasil = new IncomeStatementResponse
            {
                LegalEntityId = query.LegalEntityId,
                PeriodCode = periode.PeriodCode,
                PeriodName = NamaPeriode(periode),
                FiscalYear = periode.FiscalYear,
                PeriodStartDate = periode.StartDate,
                PeriodEndDate = periode.EndDate,
                YearToDateStartDate = awalTahun,
                ComparisonAvailable = adaBanding,
                ComparisonPeriodCode = pembanding?.PeriodCode,
                ComparisonPeriodName = pembanding is null ? null : NamaPeriode(pembanding),
                ComparisonUnavailableReason = adaBanding
                    ? null
                    : $"Periode {namaPeriodePembanding} belum ada di sistem.",
                CostCenterId = unitBiaya?.Id,
                CostCenterCode = unitBiaya?.CostCenterCode,
                CostCenterName = unitBiaya?.CostCenterName,
                CostCenterNotice = unitBiaya is null ? null : PemberitahuanUnitBiayaLabaRugi,
                Sections = new List<FinancialStatementSectionResponse> { pendapatan.Bagian, beban.Bagian },
                NetIncome = NilaiLaporan.Selisih(pendapatan.Total, beban.Total).KeRespons()
            };

            return AccountingServiceResult<IncomeStatementResponse>.Ok(
                hasil, $"Laba rugi {hasil.PeriodName} berhasil diambil.");
        }

        public async Task<AccountingServiceResult<BalanceSheetResponse>> GetBalanceSheetAsync(
            BalanceSheetQuery query,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard.PeriksaAsync<BalanceSheetResponse>(_db, ct);
            if (penjaga is not null) return penjaga;

            var siap = await SiapkanPeriodeLaporanAsync<BalanceSheetResponse>(
                query.LegalEntityId, query.PeriodCode, ct);
            if (siap.Gagal is not null) return siap.Gagal;

            var periode = siap.Periode!;

            var periodeDuaTahun = await _db.Set<AccAccountingPeriod>()
                .AsNoTracking()
                .Where(x => !x.IsDelete
                            && x.LegalEntityId == query.LegalEntityId
                            && (x.FiscalYear == periode.FiscalYear || x.FiscalYear == periode.FiscalYear - 1))
                .ToListAsync(ct);

            var awalTahun = periodeDuaTahun
                .Where(x => x.FiscalYear == periode.FiscalYear)
                .Min(x => x.StartDate);

            var periodeTahunLalu = periodeDuaTahun
                .Where(x => x.FiscalYear == periode.FiscalYear - 1)
                .ToList();

            var adaBanding = periodeTahunLalu.Count > 0;
            DateTime? tanggalBanding = adaBanding ? periodeTahunLalu.Max(x => x.EndDate) : null;
            DateTime? awalTahunBanding = adaBanding ? periodeTahunLalu.Min(x => x.StartDate) : null;

            var akun = await MuatAkunLaporanAsync(query.LegalEntityId, ct);

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

            var kini = await JumlahPerAkunAsync(barisNeraca, null, periode.EndDate, ct);
            var banding = tanggalBanding.HasValue
                ? await JumlahPerAkunAsync(barisNeraca, null, tanggalBanding.Value, ct)
                : null;

            var labaKini = await HitungLabaBelumDipindahAsync(barisLabaRugi, awalTahun, periode.EndDate, ct);
            var labaBanding = tanggalBanding.HasValue && awalTahunBanding.HasValue
                ? await HitungLabaBelumDipindahAsync(barisLabaRugi, awalTahunBanding.Value, tanggalBanding.Value, ct)
                : null;

            var nilaiAkun = new Dictionary<Guid, NilaiLaporan>();

            foreach (var a in akun.Where(x => idAkunNeraca.Contains(x.Id)))
            {
                nilaiAkun[a.Id] = new NilaiLaporan
                {
                    Kini = NilaiMenurutJenis(a.AccountType, kini, a.Id),
                    Banding = banding is null ? null : NilaiMenurutJenis(a.AccountType, banding, a.Id)
                };
            }

            var aset = SusunBagian(
                AccountType.Asset, akun, nilaiAkun, query.IncludeZeroAccounts, adaBanding, adaYtd: false);
            var liabilitas = SusunBagian(
                AccountType.Liability, akun, nilaiAkun, query.IncludeZeroAccounts, adaBanding, adaYtd: false);
            var ekuitas = SusunBagian(
                AccountType.Equity, akun, nilaiAkun, query.IncludeZeroAccounts, adaBanding, adaYtd: false);

            TambahBarisLaba(
                ekuitas, NamaLabaTahunLaluBelumDitutup, labaKini.TahunLalu, labaBanding?.TahunLalu, wajibTampil: false);
            TambahBarisLaba(
                ekuitas, NamaLabaTahunBerjalan, labaKini.TahunBerjalan, labaBanding?.TahunBerjalan, wajibTampil: true);

            ekuitas.Bagian.Total = ekuitas.Total.KeRespons();

            var totalLiabilitasEkuitas = NilaiLaporan.Kosong(adaBanding, adaYtd: false);
            totalLiabilitasEkuitas.Tambah(liabilitas.Total);
            totalLiabilitasEkuitas.Tambah(ekuitas.Total);

            var selisih = NilaiLaporan.Selisih(aset.Total, totalLiabilitasEkuitas);

            var hasil = new BalanceSheetResponse
            {
                LegalEntityId = query.LegalEntityId,
                PeriodCode = periode.PeriodCode,
                PeriodName = NamaPeriode(periode),
                FiscalYear = periode.FiscalYear,
                AsOfDate = periode.EndDate,
                ComparisonAvailable = adaBanding,
                ComparisonAsOfDate = tanggalBanding,
                ComparisonUnavailableReason = adaBanding
                    ? null
                    : $"Tahun buku {(periode.FiscalYear - 1).ToString(CultureInfo.InvariantCulture)} belum ada di sistem.",
                Sections = new List<FinancialStatementSectionResponse>
                {
                    aset.Bagian,
                    liabilitas.Bagian,
                    ekuitas.Bagian
                },
                TotalLiabilitiesAndEquity = totalLiabilitasEkuitas.KeRespons(),
                IsBalanced = selisih.Kini == 0m,
                Difference = selisih.Kini,
                ComparisonIsBalanced = selisih.Banding.HasValue ? selisih.Banding.Value == 0m : null,
                ComparisonDifference = selisih.Banding,
                UnclosedFiscalYears = labaKini.TahunBelumDitutup
            };

            return AccountingServiceResult<BalanceSheetResponse>.Ok(
                hasil, $"Neraca {hasil.PeriodName} berhasil diambil.");
        }

        private async Task<LabaBelumDipindah> HitungLabaBelumDipindahAsync(
            IQueryable<AccJournalLine> barisLabaRugi,
            DateTime awalTahun,
            DateTime sampai,
            CancellationToken ct)
        {
            var tahunBerjalan = await barisLabaRugi
                .Where(x => _db.Set<AccJournal>().Any(j => j.Id == x.JournalId
                                                          && j.AccountingDate >= awalTahun
                                                          && j.AccountingDate <= sampai))
                .SumAsync(x => x.CreditAmount - x.DebitAmount, ct);

            var perTahunLalu = await barisLabaRugi
                .Join(
                    _db.Set<AccJournal>(),
                    x => x.JournalId,
                    j => j.Id,
                    (x, j) => new { x.DebitAmount, x.CreditAmount, j.AccountingDate, j.AccountingPeriodId })
                .Where(x => x.AccountingDate < awalTahun)
                .Join(
                    _db.Set<AccAccountingPeriod>(),
                    x => x.AccountingPeriodId,
                    p => p.Id,
                    (x, p) => new { p.FiscalYear, x.DebitAmount, x.CreditAmount })
                .GroupBy(x => x.FiscalYear)
                .Select(g => new
                {
                    FiscalYear = g.Key,
                    Nilai = g.Sum(x => x.CreditAmount - x.DebitAmount)
                })
                .ToListAsync(ct);

            return new LabaBelumDipindah
            {
                TahunBerjalan = tahunBerjalan,
                TahunLalu = perTahunLalu.Sum(x => x.Nilai),
                TahunBelumDitutup = perTahunLalu
                    .Where(x => x.Nilai != 0m)
                    .Select(x => x.FiscalYear)
                    .OrderBy(x => x)
                    .ToList()
            };
        }

        private static void TambahBarisLaba(
            (FinancialStatementSectionResponse Bagian, NilaiLaporan Total) ekuitas,
            string nama,
            decimal kini,
            decimal? banding,
            bool wajibTampil)
        {
            if (!wajibTampil && kini == 0m && (banding ?? 0m) == 0m) return;

            var nilai = new NilaiLaporan
            {
                Kini = kini,
                Banding = ekuitas.Total.Banding.HasValue ? banding ?? 0m : null
            };

            ekuitas.Bagian.Rows.Add(new FinancialStatementRowResponse
            {
                RowKind = FinancialStatementRowKind.Computed,
                AccountId = null,
                ParentAccountId = null,
                AccountCode = string.Empty,
                AccountName = nama,
                Depth = 0,
                IsActive = true,
                Amounts = nilai.KeRespons()
            });

            ekuitas.Total.Tambah(nilai);
        }

        private async Task<(AccountingServiceResult<T>? Gagal, AccAccountingPeriod? Periode)> SiapkanPeriodeLaporanAsync<T>(
            Guid legalEntityId,
            string? periodCode,
            CancellationToken ct)
        {
            var kode = periodCode?.Trim() ?? string.Empty;

            if (!BentukKodePeriode.IsMatch(kode))
            {
                return (AccountingServiceResult<T>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Periode wajib dipilih dengan bentuk tahun-bulan, contoh 2026-11."), null);
            }

            var periode = await _db.Set<AccAccountingPeriod>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => !x.IsDelete
                                          && x.LegalEntityId == legalEntityId
                                          && x.PeriodCode == kode, ct);

            if (periode is null)
            {
                return (AccountingServiceResult<T>.Fail(
                    StatusCodes.Status404NotFound, "Periode akuntansi tidak ditemukan."), null);
            }

            return (null, periode);
        }

        private Task<MstCostCenter?> CariUnitBiayaAsync(Guid legalEntityId, Guid costCenterId, CancellationToken ct)
        {
            return _db.Set<MstCostCenter>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == costCenterId
                                          && !x.IsDelete
                                          && x.LegalEntityId == legalEntityId, ct);
        }

        private Task<List<AkunLaporan>> MuatAkunLaporanAsync(Guid legalEntityId, CancellationToken ct)
        {
            return _db.Set<AccChartOfAccount>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.LegalEntityId == legalEntityId)
                .Select(x => new AkunLaporan
                {
                    Id = x.Id,
                    ParentAccountId = x.ParentAccountId,
                    AccountCode = x.AccountCode,
                    AccountName = x.AccountName,
                    AccountType = x.AccountType,
                    IsActive = x.IsActive
                })
                .ToListAsync(ct);
        }

        private async Task<IQueryable<AccJournalLine>> BarisTanpaTutupTahunAsync(Guid legalEntityId, CancellationToken ct)
        {
            var dasar = BarisDisahkan(legalEntityId);

            var jenisTutupTahun = await AccJournalTypeService.CariMenurutKodeAsync(_db, KodeJenisJurnalTutupTahun, ct);
            if (jenisTutupTahun is null) return dasar;

            var idJenis = jenisTutupTahun.Id;

            return dasar.Where(x => !_db.Set<AccJournal>().Any(j =>
                j.Id == x.JournalId
                && (j.JournalTypeId == idJenis
                    || (j.ReversalOfJournalId != null
                        && _db.Set<AccJournal>().Any(r => r.Id == j.ReversalOfJournalId
                                                          && r.JournalTypeId == idJenis)))));
        }

        private async Task<Dictionary<Guid, (decimal Debit, decimal Kredit)>> JumlahPerAkunAsync(
            IQueryable<AccJournalLine> baris,
            DateTime? dari,
            DateTime sampai,
            CancellationToken ct)
        {
            var dalamRentang = dari.HasValue
                ? baris.Where(x => _db.Set<AccJournal>().Any(j => j.Id == x.JournalId
                                                                 && j.AccountingDate >= dari.Value
                                                                 && j.AccountingDate <= sampai))
                : baris.Where(x => _db.Set<AccJournal>().Any(j => j.Id == x.JournalId
                                                                 && j.AccountingDate <= sampai));

            var jumlah = await dalamRentang
                .GroupBy(x => x.AccountId)
                .Select(g => new
                {
                    AccountId = g.Key,
                    Debit = g.Sum(x => x.DebitAmount),
                    Kredit = g.Sum(x => x.CreditAmount)
                })
                .ToListAsync(ct);

            return jumlah.ToDictionary(x => x.AccountId, x => (x.Debit, x.Kredit));
        }

        private static decimal NilaiMenurutJenis(
            AccountType jenis,
            IReadOnlyDictionary<Guid, (decimal Debit, decimal Kredit)> jumlah,
            Guid idAkun)
        {
            if (!jumlah.TryGetValue(idAkun, out var j)) return 0m;

            return jenis is AccountType.Asset or AccountType.Expense
                ? j.Debit - j.Kredit
                : j.Kredit - j.Debit;
        }

        private static string JudulBagian(AccountType jenis) => jenis switch
        {
            AccountType.Asset => "Aset",
            AccountType.Liability => "Liabilitas",
            AccountType.Equity => "Ekuitas",
            AccountType.Revenue => "Pendapatan",
            AccountType.Expense => "Beban",
            _ => jenis.ToString()
        };

        private static Guid? IndukTampilan(AkunLaporan akun, IReadOnlyDictionary<Guid, AkunLaporan> peta)
        {
            var dilalui = new HashSet<Guid> { akun.Id };
            var idInduk = akun.ParentAccountId;

            while (idInduk.HasValue && dilalui.Add(idInduk.Value) && peta.TryGetValue(idInduk.Value, out var induk))
            {
                if (induk.AccountType == akun.AccountType) return induk.Id;
                idInduk = induk.ParentAccountId;
            }

            return null;
        }

        private static (FinancialStatementSectionResponse Bagian, NilaiLaporan Total) SusunBagian(
            AccountType jenis,
            IReadOnlyList<AkunLaporan> semuaAkun,
            IReadOnlyDictionary<Guid, NilaiLaporan> nilaiAkun,
            bool sertakanNol,
            bool adaBanding,
            bool adaYtd)
        {
            var peta = semuaAkun.ToDictionary(x => x.Id);
            var anggota = semuaAkun.Where(x => x.AccountType == jenis).ToList();
            var induk = anggota.ToDictionary(x => x.Id, x => IndukTampilan(x, peta));

            Dictionary<Guid, List<AkunLaporan>> SusunAnak() => anggota
                .GroupBy(x => induk[x.Id] ?? Guid.Empty)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(x => x.AccountCode, StringComparer.Ordinal).ToList());

            var anak = SusunAnak();

            var terjangkau = new HashSet<Guid>();
            var antrean = new Queue<Guid>(new[] { Guid.Empty });

            while (antrean.Count > 0)
            {
                if (!anak.TryGetValue(antrean.Dequeue(), out var daftar)) continue;

                foreach (var a in daftar.Where(x => terjangkau.Add(x.Id)))
                    antrean.Enqueue(a.Id);
            }

            if (terjangkau.Count < anggota.Count)
            {
                foreach (var a in anggota.Where(x => !terjangkau.Contains(x.Id)))
                    induk[a.Id] = null;

                anak = SusunAnak();
            }

            var total = new Dictionary<Guid, NilaiLaporan>();

            NilaiLaporan Hitung(AkunLaporan a)
            {
                var n = NilaiLaporan.Kosong(adaBanding, adaYtd);

                if (nilaiAkun.TryGetValue(a.Id, out var sendiri)) n.Tambah(sendiri);

                if (anak.TryGetValue(a.Id, out var daftar))
                {
                    foreach (var c in daftar) n.Tambah(Hitung(c));
                }

                total[a.Id] = n;
                return n;
            }

            var akar = anak.TryGetValue(Guid.Empty, out var daftarAkar) ? daftarAkar : new List<AkunLaporan>();
            var totalBagian = NilaiLaporan.Kosong(adaBanding, adaYtd);

            foreach (var a in akar) totalBagian.Tambah(Hitung(a));

            var tampil = new Dictionary<Guid, bool>();

            bool Tampil(AkunLaporan a)
            {
                if (tampil.TryGetValue(a.Id, out var sudah)) return sudah;

                var hasilTampil = sertakanNol || !total[a.Id].SemuaNol;

                if (anak.TryGetValue(a.Id, out var daftar))
                {
                    foreach (var c in daftar)
                    {
                        if (Tampil(c)) hasilTampil = true;
                    }
                }

                tampil[a.Id] = hasilTampil;
                return hasilTampil;
            }

            var baris = new List<FinancialStatementRowResponse>();

            void Tulis(AkunLaporan a, int kedalaman)
            {
                if (!Tampil(a)) return;

                var punyaAnak = anak.TryGetValue(a.Id, out var daftar);

                baris.Add(new FinancialStatementRowResponse
                {
                    RowKind = punyaAnak ? FinancialStatementRowKind.Group : FinancialStatementRowKind.Account,
                    AccountId = a.Id,
                    ParentAccountId = induk[a.Id],
                    AccountCode = a.AccountCode,
                    AccountName = a.AccountName,
                    Depth = kedalaman,
                    IsActive = a.IsActive,
                    Amounts = total[a.Id].KeRespons()
                });

                if (!punyaAnak) return;

                foreach (var c in daftar!) Tulis(c, kedalaman + 1);
            }

            foreach (var a in akar) Tulis(a, 0);

            var bagian = new FinancialStatementSectionResponse
            {
                AccountType = jenis,
                Title = JudulBagian(jenis),
                Rows = baris,
                Total = totalBagian.KeRespons()
            };

            return (bagian, totalBagian);
        }
    }
}
