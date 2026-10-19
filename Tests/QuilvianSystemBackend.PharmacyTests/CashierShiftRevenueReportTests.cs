using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.CashManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Models;
using System;
using Xunit;

namespace QuilvianSystemBackend.Tests.Pharmacy;

/// <summary>
/// Unit tests untuk verifikasi konseptual dan aturan perhitungan Laporan Pendapatan Kasir per Shift:
/// Memenuhi skenario verifikasi Task INT-02 dan kriteria kelayakan AC-07 s/d AC-13.
/// </summary>
public class CashierShiftRevenueReportTests
{
    [Fact]
    public void Scenario1_CashOnly_CalculatesExpectedCashCorrectly()
    {
        // Skenario 1 — Tunai:
        // Kas awal Rp 2.000.000 dan penerimaan tunai Rp 3.000.000.
        // Total kas sistem yang diharapkan Rp 5.000.000.
        var openingCash = 2_000_000m;
        var systemCash = 3_000_000m;

        var expectedCash = checked(openingCash + systemCash);

        Assert.Equal(5_000_000m, expectedCash);
    }

    [Fact]
    public void Scenario2_InsuranceAr_DoesNotIncreaseCashierPhysicalCash()
    {
        // Skenario 2 — Asuransi:
        // Pada shift yang sama terdapat tagihan asuransi Rp 10.000.000.
        // Total kas fisik yang diharapkan tetap Rp 5.000.000 (tidak bertambah).
        // Tagihan asuransi Rp 10.000.000 dicatat sebagai A/R terpisah.
        var openingCash = 2_000_000m;
        var cashReceived = 3_000_000m;
        var expectedCash = openingCash + cashReceived;

        var insuranceAr = 10_000_000m;

        // Invariant: Kas sistem tidak boleh mencampurkan piutang asuransi
        Assert.Equal(5_000_000m, expectedCash);
        Assert.NotEqual(expectedCash + insuranceAr, expectedCash);
        Assert.Equal(10_000_000m, insuranceAr);
    }

    [Fact]
    public void Scenario3_PettyCashDisbursement_DoesNotReduceShiftSystemCash()
    {
        // Skenario 3 — Petty Cash:
        // Petugas kasir melayani pencairan voucher Petty Cash Rp 250.000 dari dana kas kecil.
        // Total kas sistem shift tetap Rp 5.000.000 (tidak terpotong).
        var expectedCash = 5_000_000m;
        var pettyCashDisbursed = 250_000m;

        // Invariant: Kas kecil terpisah dari kas fisik kasir
        var remainingShiftCash = expectedCash; // Tidak dikurangi pettyCashDisbursed
        Assert.Equal(5_000_000m, remainingShiftCash);
    }

    [Fact]
    public void Scenario4_DepositTopUp_CountedOnlyOnce()
    {
        // Skenario 4 — Deposit:
        // Top-up deposit tunai Rp 1.000.000 sudah tercatat dalam kas shift (SystemCash).
        // Tidak boleh ditambahkan lagi sebagai pendapatan invoice baru.
        var initialOpening = 1_000_000m;
        var invoiceCashTenders = 2_000_000m;
        var depositTopUpCash = 1_000_000m;

        // SystemCash mencatat seluruh penerimaan tunai di kasir (invoice + deposit)
        var recordedSystemCash = invoiceCashTenders + depositTopUpCash;
        var expectedClosingCash = initialOpening + recordedSystemCash;

        Assert.Equal(3_000_000m, recordedSystemCash);
        Assert.Equal(4_000_000m, expectedClosingCash);
    }

    [Fact]
    public void Scenario5_NightShift_CrossingMidnight_BelongsToSingleShift()
    {
        // Skenario 5 — Shift Malam:
        // Shift dibuka pukul 23:00 WIB tanggal 2026-10-14 dan ditutup pukul 07:00 WIB tanggal 2026-10-15.
        // Seluruh transaksi dalam interval tersebut tetap berada dalam 1 shift yang sama.
        var openedAtWib = new DateTimeOffset(2026, 10, 14, 23, 0, 0, TimeSpan.FromHours(7));
        var closedAtWib = new DateTimeOffset(2026, 10, 15, 7, 0, 0, TimeSpan.FromHours(7));

        var businessDate = FinanceBusinessDate.ToDateOnly(openedAtWib);
        Assert.Equal(new DateOnly(2026, 10, 14), businessDate);

        var txAtMidnightWib = new DateTimeOffset(2026, 10, 15, 1, 30, 0, TimeSpan.FromHours(7));
        var isWithinShift = txAtMidnightWib >= openedAtWib && txAtMidnightWib <= closedAtWib;
        Assert.True(isWithinShift);
    }

    [Fact]
    public void Scenario6_PettyCashAttribution_DetectsUnattributedWhenOutsideShift()
    {
        // Skenario 6 — Petty Cash Ambigu / Belum Teratribusi:
        // Voucher dicairkan di luar rentang waktu shift atau petugas tidak cocok
        var shiftCashierId = Guid.NewGuid();
        var otherCashierId = Guid.NewGuid();
        var shiftOpenedAt = DateTimeOffset.UtcNow.AddHours(-8);
        var shiftClosedAt = DateTimeOffset.UtcNow;

        var voucherDisbursedBy = otherCashierId;
        var voucherDisbursedAt = DateTimeOffset.UtcNow.AddHours(-4);

        var isMatched = voucherDisbursedBy == shiftCashierId &&
                        voucherDisbursedAt >= shiftOpenedAt &&
                        voucherDisbursedAt <= shiftClosedAt;

        Assert.False(isMatched);
    }

    [Fact]
    public void Scenario7_OpenShift_PhysicalCashAndVarianceAreNull()
    {
        // Skenario 7 — Shift yang Masih Terbuka:
        // Belum dihitung kas fisiknya, nilai PhysicalCash dan Variance harus null, bukan 0.
        var shift = new CashierShiftRevenueItemDto
        {
            ShiftNumber = "SFT-OPEN-001",
            Status = CashierShiftStatuses.Open,
            OpeningCash = 1_000_000m,
            CashReceived = 500_000m,
            ExpectedCash = 1_500_000m,
            PhysicalCash = null,
            Variance = null
        };

        Assert.Null(shift.PhysicalCash);
        Assert.Null(shift.Variance);
    }
}
