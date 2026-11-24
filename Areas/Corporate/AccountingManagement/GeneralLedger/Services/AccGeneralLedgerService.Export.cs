using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.GeneralLedger.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using System.Globalization;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.GeneralLedger.Services
{
    public partial class AccGeneralLedgerService
    {
        private const int BatasBarisUnduhanBukuBesar = 50000;

        public async Task<AccountingServiceResult<LedgerExportFile>> BuildMovementCsvAsync(
            LedgerMovementQuery query,
            CancellationToken ct = default)
        {
            var siap = await SiapkanMutasiAsync<LedgerExportFile>(query, ct);
            if (siap.Gagal is not null) return siap.Gagal;

            var konteks = siap.Konteks!;

            var jumlah = await konteks.DalamRentang.CountAsync(ct);

            if (jumlah > BatasBarisUnduhanBukuBesar)
            {
                return AccountingServiceResult<LedgerExportFile>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    $"Mutasi terlalu banyak untuk diunduh sekaligus ({jumlah:N0} baris). Persempit rentang tanggal.");
            }

            var semua = await UrutkanMutasi(konteks.DalamRentang).ToListAsync(ct);
            var mutasi = await PetakanMutasiAsync(semua, konteks.SaldoPembuka, ct);

            var namaBadanHukum = await NamaBadanHukumAsync(query.LegalEntityId, ct);
            var dari = query.DateFrom.HasValue ? AccLedgerCsv.Tanggal(query.DateFrom.Value.Date) : "awal data";
            var sampai = query.DateTo.HasValue ? AccLedgerCsv.Tanggal(query.DateTo.Value.Date) : "akhir data";

            var kepala = new List<IReadOnlyList<string>>
            {
                AccLedgerCsv.BarisKepala("Laporan", "Buku Besar"),
                AccLedgerCsv.BarisKepala("Badan Hukum", namaBadanHukum),
                AccLedgerCsv.BarisKepala("Akun", $"{konteks.Akun.AccountCode} - {konteks.Akun.AccountName}"),
                AccLedgerCsv.BarisKepala("Rentang", $"{dari} s.d. {sampai}"),
                AccLedgerCsv.BarisKepala(
                    "Unit Biaya",
                    konteks.UnitBiaya is null
                        ? "Semua unit biaya"
                        : $"{konteks.UnitBiaya.CostCenterCode} - {konteks.UnitBiaya.CostCenterName}"),
                AccLedgerCsv.BarisKepala("Diunduh", AccLedgerCsv.WaktuUnduh())
            };

            if (konteks.UnitBiaya is not null)
            {
                kepala.Add(AccLedgerCsv.BarisKepala(
                    "Catatan",
                    "Saldo pada berkas ini hanya bagian akun yang mencatat unit biaya tersebut."));
            }

            var kolom = new[]
            {
                AccLedgerCsv.Teks("Tanggal"),
                AccLedgerCsv.Teks("No. Jurnal"),
                AccLedgerCsv.Teks("Jenis Jurnal"),
                AccLedgerCsv.Teks("Asal"),
                AccLedgerCsv.Teks("No. Dokumen"),
                AccLedgerCsv.Teks("Unit Biaya"),
                AccLedgerCsv.Teks("Keterangan"),
                AccLedgerCsv.Teks("Debit"),
                AccLedgerCsv.Teks("Kredit"),
                AccLedgerCsv.Teks("Saldo")
            };

            var isi = new List<IReadOnlyList<string>>
            {
                BarisRingkasMutasi("Saldo awal", null, null, konteks.SaldoPembuka)
            };

            foreach (var m in mutasi)
            {
                isi.Add(new[]
                {
                    AccLedgerCsv.Tanggal(m.AccountingDate),
                    AccLedgerCsv.Teks(m.JournalNumber),
                    AccLedgerCsv.Teks(m.JournalTypeCode),
                    AccLedgerCsv.Teks(m.SourceLabel),
                    AccLedgerCsv.Teks(m.DocumentNumber),
                    AccLedgerCsv.Teks(m.CostCenterCode is null ? null : $"{m.CostCenterCode} - {m.CostCenterName}"),
                    AccLedgerCsv.Teks(m.Description),
                    AccLedgerCsv.Angka(m.DebitAmount),
                    AccLedgerCsv.Angka(m.CreditAmount),
                    AccLedgerCsv.Angka(m.RunningBalance)
                });
            }

            var totalDebit = mutasi.Sum(x => x.DebitAmount);
            var totalKredit = mutasi.Sum(x => x.CreditAmount);

            isi.Add(BarisRingkasMutasi(
                "Saldo akhir",
                totalDebit,
                totalKredit,
                konteks.SaldoPembuka + totalDebit - totalKredit));

            var segmenDari = query.DateFrom.HasValue
                ? query.DateFrom.Value.ToString("yyyyMMdd", CultureInfo.InvariantCulture)
                : "awal";
            var segmenSampai = query.DateTo.HasValue
                ? query.DateTo.Value.ToString("yyyyMMdd", CultureInfo.InvariantCulture)
                : "akhir";

            var berkas = new LedgerExportFile
            {
                Content = AccLedgerCsv.Tulis(kepala, kolom, isi),
                FileName = $"buku-besar_{SegmenNamaBerkas(konteks.Akun.AccountCode)}_{segmenDari}-{segmenSampai}.csv",
                RowCount = mutasi.Count
            };

            return AccountingServiceResult<LedgerExportFile>.Ok(
                berkas, $"Buku besar akun {konteks.Akun.AccountCode} berhasil diunduh.");
        }

        private static IReadOnlyList<string> BarisRingkasMutasi(
            string label,
            decimal? debit,
            decimal? kredit,
            decimal saldo)
        {
            return new[]
            {
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                AccLedgerCsv.Teks(label),
                debit.HasValue ? AccLedgerCsv.Angka(debit.Value) : string.Empty,
                kredit.HasValue ? AccLedgerCsv.Angka(kredit.Value) : string.Empty,
                AccLedgerCsv.Angka(saldo)
            };
        }

        private async Task<string> NamaBadanHukumAsync(Guid legalEntityId, CancellationToken ct)
        {
            var nama = await _db.Set<MstLegalEntity>()
                .AsNoTracking()
                .Where(x => x.Id == legalEntityId)
                .Select(x => x.LegalEntityName)
                .FirstOrDefaultAsync(ct);

            return nama ?? string.Empty;
        }

        private static string SegmenNamaBerkas(string nilai)
        {
            var aman = new string(nilai
                .Select(c => char.IsLetterOrDigit(c) || c == '-' || c == '_' ? c : '-')
                .ToArray());

            return string.IsNullOrWhiteSpace(aman) ? "akun" : aman;
        }
    }
}
