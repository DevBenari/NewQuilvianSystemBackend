using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.CashManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;

/// <summary>
/// Layanan kalkulasi posisi saldo subledger per tanggal dan selisih kas operasional (BE-FIN-067, FIN-DES-081, FIN-DEC-114, FIN-DEC-125).
/// Menjadi satu-satunya sumbu kalkulasi posisi saldo Finance per tanggal secara murni berbasis saldo awal cutover ditambah buku mutasi bertanggal.
/// Invariant: Nol pembacaan OutstandingAmount atau ClosingBalance sebagai jawaban posisi saldo.
/// </summary>
public sealed class FinanceSubledgerBalanceCalculator
{
    private static readonly Regex AccountingPeriodRegex = new(@"^\d{4}-(0[1-9]|1[0-2])$", RegexOptions.Compiled);
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<FinanceSubledgerBalanceCalculator> _logger;

    public FinanceSubledgerBalanceCalculator(
        ApplicationDbContext dbContext,
        ILogger<FinanceSubledgerBalanceCalculator> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Menghitung posisi seluruh kelompok saldo subledger dan rincian segmennya pada tanggal tertentu sejak cutover (FIN-DES-081, FIN-VAL-170).
    /// </summary>
    public async Task<SubledgerPositionResponse> CalculatePositionAsync(
        DateOnly asOfDate,
        CancellationToken cancellationToken = default)
    {
        var cutoverDate = await GetCutoverDateAsync(cancellationToken);

        // FIN-VAL-170: Menolak tanggal yang diminta lebih awal daripada CutoverDate
        if (asOfDate < cutoverDate)
        {
            throw new FinanceSubledgerValidationException(
                "Posisi saldo sebelum tanggal cutover tidak dapat dihitung karena buku mutasi belum berjalan pada tanggal itu.");
        }

        // 1. Ambil seluruh saldo awal cutover aktif
        var openingBalances = await _dbContext.FinOpeningBalances
            .AsNoTracking()
            .Where(x => !x.IsDelete)
            .ToListAsync(cancellationToken);

        // 2. Kelompok KAS-KASIR (FIN-DES-081)
        // Posisi = Saldo Awal Cutover + sum(IN) - sum(OUT) sampai dengan asOfDate (mengecualikan mutasi SALDO-AWAL agar tidak dobel)
        var openingKasKasir = openingBalances
            .FirstOrDefault(x => x.BalanceGroup == FinSubledgerBalanceGroups.KasKasir)?.Amount ?? 0.00m;

        var cashMovements = await _dbContext.FinCashMovements
            .AsNoTracking()
            .Where(x => !x.IsDelete && x.BusinessDate <= asOfDate && x.MovementType != FinCashMovementTypes.SaldoAwal)
            .ToListAsync(cancellationToken);

        var totalCashIn = cashMovements
            .Where(x => x.Direction == FinCashMovementDirections.In)
            .Sum(x => x.Amount);

        var totalCashOut = cashMovements
            .Where(x => x.Direction == FinCashMovementDirections.Out)
            .Sum(x => x.Amount);

        var kasKasirPosition = openingKasKasir + totalCashIn - totalCashOut;

        var kasKasirGroup = new SubledgerGroupPositionItem
        {
            BalanceGroup = FinSubledgerBalanceGroups.KasKasir,
            GroupName = "Kas Kasir",
            OpeningAmount = openingKasKasir,
            TotalIn = totalCashIn,
            TotalOut = totalCashOut,
            TotalMovementAmount = totalCashIn - totalCashOut,
            CalculatedPosition = kasKasirPosition,
            Segments = new List<SubledgerSegmentPositionItem>()
        };

        // 3. Kelompok KAS-KECIL
        var openingKasKecil = openingBalances
            .FirstOrDefault(x => x.BalanceGroup == FinSubledgerBalanceGroups.KasKecil)?.Amount ?? 0.00m;

        var kasKecilGroup = new SubledgerGroupPositionItem
        {
            BalanceGroup = FinSubledgerBalanceGroups.KasKecil,
            GroupName = "Kas Kecil",
            OpeningAmount = openingKasKecil,
            TotalIn = 0.00m,
            TotalOut = 0.00m,
            TotalMovementAmount = 0.00m,
            CalculatedPosition = openingKasKecil,
            Segments = new List<SubledgerSegmentPositionItem>()
        };

        // 4. Kelompok PIUTANG (FIN-DES-079, FIN-DEC-123)
        // Murni dari penjumlahan Amount buku mutasi FinReceivableMovement bertanggal <= asOfDate (nol baca OutstandingAmount)
        var arMovements = await _dbContext.FinReceivableMovements
            .AsNoTracking()
            .Include(x => x.Receivable)
            .Where(x => !x.IsDelete && x.BusinessDate <= asOfDate)
            .ToListAsync(cancellationToken);

        var totalArMovement = arMovements.Sum(x => x.Amount);

        var segmentPayer = arMovements
            .Where(x => x.Receivable?.DebtorType == FinReceivableDebtorTypes.Payer)
            .Sum(x => x.Amount);

        var segmentPatient = arMovements
            .Where(x => x.Receivable?.DebtorType == FinReceivableDebtorTypes.PatientGuarantor)
            .Sum(x => x.Amount);

        var segmentEmployee = arMovements
            .Where(x => x.Receivable?.DebtorType == FinReceivableDebtorTypes.EmployeeBenefit)
            .Sum(x => x.Amount);

        var piutangGroup = new SubledgerGroupPositionItem
        {
            BalanceGroup = FinSubledgerBalanceGroups.Piutang,
            GroupName = "Piutang Usaha / Pasien & Penjamin",
            OpeningAmount = 0.00m,
            TotalIn = 0.00m,
            TotalOut = 0.00m,
            TotalMovementAmount = totalArMovement,
            CalculatedPosition = totalArMovement,
            Segments = new List<SubledgerSegmentPositionItem>
            {
                new() { SegmentKey = FinReceivableDebtorTypes.Payer, SegmentName = "Penjamin / Asuransi", Amount = segmentPayer },
                new() { SegmentKey = FinReceivableDebtorTypes.PatientGuarantor, SegmentName = "Penjamin Pasien Pribadi", Amount = segmentPatient },
                new() { SegmentKey = FinReceivableDebtorTypes.EmployeeBenefit, SegmentName = "Manfaat Karyawan", Amount = segmentEmployee }
            }
        };

        // 5. Kelompok UTANG-SUPPLIER (FIN-DES-079, FIN-DEC-123)
        // Murni dari penjumlahan Amount buku mutasi FinSupplierPayableMovement bertanggal <= asOfDate (nol baca OutstandingAmount)
        var apMovements = await _dbContext.FinSupplierPayableMovements
            .AsNoTracking()
            .Where(x => !x.IsDelete && x.BusinessDate <= asOfDate)
            .ToListAsync(cancellationToken);

        var totalApMovement = apMovements.Sum(x => x.Amount);

        var utangSupplierGroup = new SubledgerGroupPositionItem
        {
            BalanceGroup = FinSubledgerBalanceGroups.UtangSupplier,
            GroupName = "Utang Usaha / Supplier",
            OpeningAmount = 0.00m,
            TotalIn = 0.00m,
            TotalOut = 0.00m,
            TotalMovementAmount = totalApMovement,
            CalculatedPosition = totalApMovement,
            Segments = new List<SubledgerSegmentPositionItem>()
        };

        // 6. Kelompok UTANG-JASA-MEDIS (FIN-DES-091, FIN-DEC-122)
        // Belum ada buku mutasi dan penulis saldo, bernilai 0.00m
        var utangJasaMedisGroup = new SubledgerGroupPositionItem
        {
            BalanceGroup = FinSubledgerBalanceGroups.UtangJasaMedis,
            GroupName = "Utang Jasa Medis",
            OpeningAmount = 0.00m,
            TotalIn = 0.00m,
            TotalOut = 0.00m,
            TotalMovementAmount = 0.00m,
            CalculatedPosition = 0.00m,
            Segments = new List<SubledgerSegmentPositionItem>
            {
                new() { SegmentKey = FinMedicalServicePayeeTypes.Doctor, SegmentName = "Dokter", Amount = 0.00m },
                new() { SegmentKey = FinMedicalServicePayeeTypes.Nurse, SegmentName = "Perawat", Amount = 0.00m },
                new() { SegmentKey = FinMedicalServicePayeeTypes.OtherPractitioner, SegmentName = "Tenaga Medis Lainnya", Amount = 0.00m }
            }
        };

        var groups = new List<SubledgerGroupPositionItem>
        {
            kasKasirGroup,
            kasKecilGroup,
            piutangGroup,
            utangSupplierGroup,
            utangJasaMedisGroup
        };

        var totalPosition = groups.Sum(x => x.CalculatedPosition);

        _logger.LogInformation(
            "Posisi subledger per {AsOfDate} berhasil dihitung. Cutover: {CutoverDate}, Total: {TotalPosition}",
            asOfDate, cutoverDate, totalPosition);

        return new SubledgerPositionResponse
        {
            AsOfDate = asOfDate,
            CutoverDate = cutoverDate,
            CalculatedAt = DateTimeOffset.UtcNow,
            TotalPosition = totalPosition,
            Groups = groups
        };
    }

    /// <summary>
    /// Menghitung perbandingan selisih rekap kas harian terhadap posisi kas terhitung pada satu periode akuntansi (FIN-DEC-125, FIN-VAL-170).
    /// </summary>
    public async Task<CashVarianceResponse> CalculateCashVarianceAsync(
        string accountingPeriodCode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(accountingPeriodCode) || !AccountingPeriodRegex.IsMatch(accountingPeriodCode))
        {
            throw new FinanceSubledgerBadRequestException("Kode periode harus berbentuk YYYY-MM, contoh: 2026-09.");
        }

        var periodParts = accountingPeriodCode.Split('-');
        var periodYear = int.Parse(periodParts[0]);
        var periodMonth = int.Parse(periodParts[1]);
        var periodStartDate = new DateOnly(periodYear, periodMonth, 1);
        var periodEndDate = FinanceBusinessDate.GetPeriodEndDate(periodYear, periodMonth);

        var cutoverDate = await GetCutoverDateAsync(cancellationToken);

        // FIN-VAL-170: Menolak jika akhir periode sebelum CutoverDate
        if (periodEndDate < cutoverDate)
        {
            throw new FinanceSubledgerValidationException(
                "Posisi saldo sebelum tanggal cutover tidak dapat dihitung karena buku mutasi belum berjalan pada tanggal itu.");
        }

        // 1. Hitung posisi Kas Kasir terhitung pada PeriodEndDate
        var openingKasKasir = await _dbContext.FinOpeningBalances
            .AsNoTracking()
            .Where(x => !x.IsDelete && x.BalanceGroup == FinSubledgerBalanceGroups.KasKasir)
            .Select(x => x.Amount)
            .FirstOrDefaultAsync(cancellationToken);

        var cashMovementsUpToPeriod = await _dbContext.FinCashMovements
            .AsNoTracking()
            .Where(x => !x.IsDelete && x.BusinessDate <= periodEndDate && x.MovementType != FinCashMovementTypes.SaldoAwal)
            .ToListAsync(cancellationToken);

        var totalIn = cashMovementsUpToPeriod
            .Where(x => x.Direction == FinCashMovementDirections.In)
            .Sum(x => x.Amount);

        var totalOut = cashMovementsUpToPeriod
            .Where(x => x.Direction == FinCashMovementDirections.Out)
            .Sum(x => x.Amount);

        var calculatedCashPosition = openingKasKasir + totalIn - totalOut;

        // 2. Ambil rekapitulasi kas harian terakhir pada rentang periode terkait
        var lastDailySnapshot = await _dbContext.FinDailyCashSnapshots
            .AsNoTracking()
            .Where(x => !x.IsDelete && x.CashDate >= periodStartDate && x.CashDate <= periodEndDate)
            .OrderByDescending(x => x.CashDate)
            .FirstOrDefaultAsync(cancellationToken);

        var dailyCashClosingBalance = lastDailySnapshot?.ClosingBalance ?? 0.00m;
        var dailyCashSnapshotDate = lastDailySnapshot?.CashDate;
        var dailyCashSnapshotStatus = lastDailySnapshot?.Status;

        var varianceAmount = dailyCashClosingBalance - calculatedCashPosition;

        // 3. Ambil mutasi kas periode terkait yang menjelaskan perubahan kas
        var explainingMovements = await _dbContext.FinCashMovements
            .AsNoTracking()
            .Where(x => !x.IsDelete && x.BusinessDate >= periodStartDate && x.BusinessDate <= periodEndDate && x.MovementType != FinCashMovementTypes.SaldoAwal)
            .OrderBy(x => x.BusinessDate)
            .ThenBy(x => x.OccurredAt)
            .Select(x => new CashMovementExplainingItem
            {
                Id = x.Id,
                MovementType = x.MovementType,
                Direction = x.Direction,
                Amount = x.Amount,
                BusinessDate = x.BusinessDate,
                OccurredAt = x.OccurredAt,
                SourceReferenceType = x.SourceReferenceType,
                SourceReferenceId = x.SourceReferenceId,
                PaymentMethodCode = x.PaymentMethodCode,
                CashierShiftId = x.CashierShiftId,
                Notes = x.Notes
            })
            .ToListAsync(cancellationToken);

        _logger.LogInformation(
            "Selisih kas periode {Period} dihitung. DailyClosing: {DailyClosing}, Calculated: {Calculated}, Variance: {Variance}",
            accountingPeriodCode, dailyCashClosingBalance, calculatedCashPosition, varianceAmount);

        return new CashVarianceResponse
        {
            AccountingPeriodCode = accountingPeriodCode,
            PeriodStartDate = periodStartDate,
            PeriodEndDate = periodEndDate,
            CutoverDate = cutoverDate,
            CalculatedCashPosition = calculatedCashPosition,
            DailyCashClosingBalance = dailyCashClosingBalance,
            DailyCashSnapshotDate = dailyCashSnapshotDate,
            DailyCashSnapshotStatus = dailyCashSnapshotStatus,
            VarianceAmount = varianceAmount,
            HasVariance = varianceAmount != 0.00m,
            ExplainingMovements = explainingMovements
        };
    }

    /// <summary>
    /// Memulangkan CutoverDate efektif dari konfigurasi FinOpeningBalance yang tersimpan.
    /// </summary>
    private async Task<DateOnly> GetCutoverDateAsync(CancellationToken cancellationToken)
    {
        var cutoverDate = await _dbContext.FinOpeningBalances
            .AsNoTracking()
            .Where(x => !x.IsDelete)
            .OrderBy(x => x.CutoverDate)
            .Select(x => (DateOnly?)x.CutoverDate)
            .FirstOrDefaultAsync(cancellationToken);

        // Jika belum ada data cutover tercatat, gunakan hari pertama bulan berjalan menurut kalender WIB
        return cutoverDate ?? new DateOnly(FinanceBusinessDate.Today().Year, FinanceBusinessDate.Today().Month, 1);
    }
}
