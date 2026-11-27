using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingEvent.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.GeneralLedger.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.GeneralLedger.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.JournalType.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.RecurringJournal.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.GeneralLedger.Services
{
    public partial class AccGeneralLedgerService
    {
        private const string KodeJenisJurnalTutupTahun = "JT";

        private sealed class KonteksMutasi
        {
            public AccChartOfAccount Akun { get; init; } = null!;

            public MstCostCenter? UnitBiaya { get; init; }

            public IQueryable<AccJournalLine> DalamRentang { get; init; } = null!;

            public decimal SaldoPembuka { get; init; }
        }

        private sealed class BarisMutasi
        {
            public Guid JournalId { get; init; }

            public DateTime AccountingDate { get; init; }

            public string JournalNumber { get; init; } = string.Empty;

            public string JournalDescription { get; init; } = string.Empty;

            public string? DocumentNumber { get; init; }

            public Guid JournalTypeId { get; init; }

            public Guid? ReversalOfJournalId { get; init; }

            public int LineNumber { get; init; }

            public string? LineDescription { get; init; }

            public decimal DebitAmount { get; init; }

            public decimal CreditAmount { get; init; }

            public Guid? CostCenterId { get; init; }
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
            var siap = await SiapkanMutasiAsync<PagedResult<LedgerMovementResponse>>(query, ct);
            if (siap.Gagal is not null) return siap.Gagal;

            var konteks = siap.Konteks!;

            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize is < 1 or > 200 ? 25 : query.PageSize;

            // Diproyeksikan lebih dahulu supaya pengurutan memakai kolom jurnal tanpa menarik
            // seluruh entity, dan supaya urutannya sama persis di setiap pemanggilan.
            var terurut = UrutkanMutasi(konteks.DalamRentang);

            var total = await terurut.CountAsync(ct);
            var dilewati = (pageNumber - 1) * pageSize;

            // Saldo seluruh baris yang dilewati halaman sebelumnya — dihitung di database.
            var saldoSebelumHalaman = dilewati == 0
                ? 0m
                : await terurut.Take(dilewati).SumAsync(x => x.DebitAmount - x.CreditAmount, ct);

            var halaman = await terurut
                .Skip(dilewati)
                .Take(pageSize)
                .ToListAsync(ct);

            var items = await PetakanMutasiAsync(halaman, konteks.SaldoPembuka + saldoSebelumHalaman, ct);

            var hasil = new PagedResult<LedgerMovementResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = total,
                TotalPage = pageSize == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize),
                Items = items
            };

            return AccountingServiceResult<PagedResult<LedgerMovementResponse>>.Ok(
                hasil, $"Mutasi buku besar akun {konteks.Akun.AccountCode} berhasil diambil.");
        }

        public async Task<AccountingServiceResult<LedgerMovementSummaryResponse>> GetMovementSummaryAsync(
            LedgerMovementQuery query,
            CancellationToken ct = default)
        {
            var siap = await SiapkanMutasiAsync<LedgerMovementSummaryResponse>(query, ct);
            if (siap.Gagal is not null) return siap.Gagal;

            var konteks = siap.Konteks!;

            var totalDebit = await konteks.DalamRentang.SumAsync(x => x.DebitAmount, ct);
            var totalKredit = await konteks.DalamRentang.SumAsync(x => x.CreditAmount, ct);
            var jumlahBaris = await konteks.DalamRentang.CountAsync(ct);

            var hasil = new LedgerMovementSummaryResponse
            {
                AccountId = konteks.Akun.Id,
                AccountCode = konteks.Akun.AccountCode,
                AccountName = konteks.Akun.AccountName,
                CostCenterId = konteks.UnitBiaya?.Id,
                CostCenterCode = konteks.UnitBiaya?.CostCenterCode,
                CostCenterName = konteks.UnitBiaya?.CostCenterName,
                OpeningBalance = konteks.SaldoPembuka,
                TotalDebit = totalDebit,
                TotalCredit = totalKredit,
                ClosingBalance = konteks.SaldoPembuka + totalDebit - totalKredit,
                LineCount = jumlahBaris
            };

            return AccountingServiceResult<LedgerMovementSummaryResponse>.Ok(
                hasil, $"Ringkasan buku besar akun {konteks.Akun.AccountCode} berhasil diambil.");
        }

        private async Task<(AccountingServiceResult<T>? Gagal, KonteksMutasi? Konteks)> SiapkanMutasiAsync<T>(
            LedgerMovementQuery query,
            CancellationToken ct)
        {
            var penjaga = await AccountingLegalEntityGuard.PeriksaAsync<T>(_db, ct);
            if (penjaga is not null) return (penjaga, null);

            var rentang = PeriksaRentang<T>(query.DateFrom, query.DateTo);
            if (rentang is not null) return (rentang, null);

            var akun = await _db.Set<AccChartOfAccount>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == query.AccountId && !x.IsDelete, ct);

            if (akun is null)
            {
                return (AccountingServiceResult<T>.Fail(
                    StatusCodes.Status404NotFound, "Akun tidak ditemukan."), null);
            }

            MstCostCenter? unitBiaya = null;

            if (query.CostCenterId.HasValue)
            {
                var idUnit = query.CostCenterId.Value;

                unitBiaya = await _db.Set<MstCostCenter>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == idUnit
                                              && !x.IsDelete
                                              && x.LegalEntityId == query.LegalEntityId, ct);

                if (unitBiaya is null)
                {
                    return (AccountingServiceResult<T>.Fail(
                        StatusCodes.Status400BadRequest,
                        "Unit biaya tidak ditemukan pada badan hukum ini."), null);
                }
            }

            var dasar = BarisDisahkan(query.LegalEntityId).Where(x => x.AccountId == query.AccountId);

            if (unitBiaya is not null)
            {
                var idUnitTerpilih = unitBiaya.Id;
                dasar = dasar.Where(x => x.CostCenterId == idUnitTerpilih);
            }

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

            return (null, new KonteksMutasi
            {
                Akun = akun,
                UnitBiaya = unitBiaya,
                DalamRentang = dalamRentang,
                SaldoPembuka = saldoPembuka
            });
        }

        private IQueryable<BarisMutasi> UrutkanMutasi(IQueryable<AccJournalLine> baris)
        {
            return baris
                .Join(
                    _db.Set<AccJournal>(),
                    x => x.JournalId,
                    j => j.Id,
                    (x, j) => new BarisMutasi
                    {
                        JournalId = j.Id,
                        AccountingDate = j.AccountingDate,
                        JournalNumber = j.JournalNumber,
                        JournalDescription = j.Description,
                        DocumentNumber = j.DocumentNumber,
                        JournalTypeId = j.JournalTypeId,
                        ReversalOfJournalId = j.ReversalOfJournalId,
                        LineNumber = x.LineNumber,
                        LineDescription = x.Description,
                        DebitAmount = x.DebitAmount,
                        CreditAmount = x.CreditAmount,
                        CostCenterId = x.CostCenterId
                    })
                .OrderBy(x => x.AccountingDate)
                .ThenBy(x => x.JournalNumber)
                .ThenBy(x => x.LineNumber);
        }

        private async Task<List<LedgerMovementResponse>> PetakanMutasiAsync(
            List<BarisMutasi> baris,
            decimal saldoAwal,
            CancellationToken ct)
        {
            if (baris.Count == 0) return new List<LedgerMovementResponse>();

            var idJurnal = baris.Select(x => x.JournalId).Distinct().ToList();
            var idJenis = baris.Select(x => x.JournalTypeId).Distinct().ToList();
            var idDikoreksi = baris
                .Where(x => x.ReversalOfJournalId.HasValue)
                .Select(x => x.ReversalOfJournalId!.Value)
                .Distinct()
                .ToList();
            var idUnit = baris
                .Where(x => x.CostCenterId.HasValue)
                .Select(x => x.CostCenterId!.Value)
                .Distinct()
                .ToList();

            var kodeJenis = await _db.Set<AccJournalType>()
                .AsNoTracking()
                .Where(x => idJenis.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.JournalTypeCode, ct);

            var kejadian = (await _db.Set<AccAccountingEvent>()
                    .AsNoTracking()
                    .Where(x => !x.IsDelete && x.JournalId != null && idJurnal.Contains(x.JournalId.Value))
                    .Select(x => new { JournalId = x.JournalId!.Value, x.Id, x.SourceModule, x.SourceTransactionId })
                    .ToListAsync(ct))
                .GroupBy(x => x.JournalId)
                .ToDictionary(g => g.Key, g => g.First());

            var penerbitan = (await _db.Set<AccRecurringJournalRun>()
                    .AsNoTracking()
                    .Where(x => !x.IsDelete && idJurnal.Contains(x.JournalId))
                    .Select(x => new
                    {
                        x.JournalId,
                        x.TemplateId,
                        TemplateName = x.Template != null ? x.Template.TemplateName : string.Empty
                    })
                    .ToListAsync(ct))
                .GroupBy(x => x.JournalId)
                .ToDictionary(g => g.Key, g => g.First());

            var nomorDikoreksi = idDikoreksi.Count == 0
                ? new Dictionary<Guid, string>()
                : await _db.Set<AccJournal>()
                    .AsNoTracking()
                    .Where(x => idDikoreksi.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.JournalNumber, ct);

            var unitBiaya = idUnit.Count == 0
                ? new Dictionary<Guid, (string Kode, string Nama)>()
                : (await _db.Set<MstCostCenter>()
                        .AsNoTracking()
                        .Where(x => idUnit.Contains(x.Id))
                        .Select(x => new { x.Id, x.CostCenterCode, x.CostCenterName })
                        .ToListAsync(ct))
                    .ToDictionary(x => x.Id, x => (Kode: x.CostCenterCode, Nama: x.CostCenterName));

            var hasil = new List<LedgerMovementResponse>(baris.Count);
            var berjalan = saldoAwal;

            foreach (var b in baris)
            {
                berjalan += b.DebitAmount - b.CreditAmount;

                var respons = new LedgerMovementResponse
                {
                    AccountingDate = b.AccountingDate,
                    JournalNumber = b.JournalNumber,
                    LineNumber = b.LineNumber,
                    Description = b.LineDescription ?? b.JournalDescription,
                    DebitAmount = b.DebitAmount,
                    CreditAmount = b.CreditAmount,
                    RunningBalance = berjalan,
                    JournalId = b.JournalId,
                    JournalTypeCode = kodeJenis.GetValueOrDefault(b.JournalTypeId, string.Empty),
                    DocumentNumber = b.DocumentNumber,
                    CostCenterId = b.CostCenterId
                };

                if (b.CostCenterId.HasValue && unitBiaya.TryGetValue(b.CostCenterId.Value, out var unit))
                {
                    respons.CostCenterCode = unit.Kode;
                    respons.CostCenterName = unit.Nama;
                }

                if (b.ReversalOfJournalId.HasValue)
                {
                    respons.SourceKind = LedgerSourceKind.Correction;
                    respons.ReversalOfJournalId = b.ReversalOfJournalId;
                    respons.SourceLabel = nomorDikoreksi.TryGetValue(b.ReversalOfJournalId.Value, out var nomor)
                        ? $"Koreksi · {nomor}"
                        : "Koreksi";
                }
                else if (string.Equals(respons.JournalTypeCode, KodeJenisJurnalTutupTahun, StringComparison.OrdinalIgnoreCase))
                {
                    respons.SourceKind = LedgerSourceKind.YearEnd;
                    respons.SourceLabel = "Tutup Tahun";
                }
                else if (kejadian.TryGetValue(b.JournalId, out var asalKejadian))
                {
                    respons.SourceKind = LedgerSourceKind.AccountingEvent;
                    respons.AccountingEventId = asalKejadian.Id;
                    respons.SourceLabel = $"{asalKejadian.SourceModule} · {asalKejadian.SourceTransactionId}";
                }
                else if (penerbitan.TryGetValue(b.JournalId, out var asalBerulang))
                {
                    respons.SourceKind = LedgerSourceKind.RecurringJournal;
                    respons.RecurringTemplateId = asalBerulang.TemplateId;
                    respons.SourceLabel = $"Jurnal Berulang · {asalBerulang.TemplateName}";
                }
                else
                {
                    respons.SourceKind = LedgerSourceKind.Manual;
                    respons.SourceLabel = string.IsNullOrWhiteSpace(b.DocumentNumber)
                        ? "Manual"
                        : $"Manual · {b.DocumentNumber}";
                }

                hasil.Add(respons);
            }

            return hasil;
        }
    }
}
