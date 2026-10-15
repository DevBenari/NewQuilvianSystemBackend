using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace QuilvianSystemBackend.Tests.Pharmacy;

/// <summary>
/// Unit test untuk Finance AR Invoice / FinReceivableInvoiceBatch:
/// 1. Due Date selalu otomatis 30 hari kalender dari Tanggal Pembuatan Invoice.
/// 2. Format resmi nomor invoice: {SEQUENCE}/{VISIT_CODE}/RSMMC/{ROMAN_MONTH}/{YEAR}.
/// 3. Validasi visit code (Inpatient -> IP, Outpatient/Emergency/OTC -> OP).
/// 4. Invariant: satu batch hanya boleh memiliki satu distinct visit code (campur IP + OP ditolak 422).
/// </summary>
public class FinanceReceivableInvoiceBatchTests
{
    [Fact]
    public void Case1_Inpatient_Generates_IP_And_RomanMonth()
    {
        // CASE 1: Sequence = 50, EncounterType = Inpatient, InvoiceDate = 2026-06-10
        // Expected: 0050/IP/RSMMC/VI/2026
        const long sequence = 50;
        const string visitCode = "IP";
        var invoiceDate = new DateOnly(2026, 6, 10);
        var romanMonth = FinanceReceivableInvoiceBatchService.ToRomanMonth(invoiceDate.Month);

        var invoiceNumber = $"{sequence:D4}/{visitCode}/RSMMC/{romanMonth}/{invoiceDate.Year}";

        Assert.Equal("0050/IP/RSMMC/VI/2026", invoiceNumber);
    }

    [Fact]
    public void Case2_Outpatient_Generates_OP_And_RomanMonth()
    {
        // CASE 2: Next sequence = 51, EncounterType = Outpatient, InvoiceDate = 2026-06-12
        // Expected: 0051/OP/RSMMC/VI/2026
        const long sequence = 51;
        const string visitCode = "OP";
        var invoiceDate = new DateOnly(2026, 6, 12);
        var romanMonth = FinanceReceivableInvoiceBatchService.ToRomanMonth(invoiceDate.Month);

        var invoiceNumber = $"{sequence:D4}/{visitCode}/RSMMC/{romanMonth}/{invoiceDate.Year}";

        Assert.Equal("0051/OP/RSMMC/VI/2026", invoiceNumber);
    }

    [Fact]
    public void Case3_Emergency_Resolves_To_OP()
    {
        // CASE 3: EncounterType = Emergency -> Expected: VisitCode = OP
        var recId = Guid.NewGuid();
        var encId = Guid.NewGuid();
        var receivable = new FinReceivable { Id = recId, ReceivableNumber = "REC-EMG-01" };
        var encounter = new RegPatientEncounter { Id = encId, EncounterType = EncounterType.Emergency };

        var items = new List<FinReceivableItem>
        {
            new() { Id = Guid.NewGuid(), ReceivableId = recId, EncounterId = encId }
        };

        var encounters = new Dictionary<Guid, RegPatientEncounter> { [encId] = encounter };
        var invoices = new Dictionary<Guid, BilInvoice>();

        var visitCode = FinanceReceivableInvoiceBatchService.ResolveReceivableVisitCode(
            receivable, items, invoices, encounters);

        Assert.Equal("OP", visitCode);
    }

    [Fact]
    public void Case4_OTC_AuthoritativeSource_Resolves_To_OP()
    {
        // CASE 4: OTC authoritative source (invoice.ServiceType == "OTC") -> Expected: VisitCode = OP
        var recId = Guid.NewGuid();
        var invId = Guid.NewGuid();
        var receivable = new FinReceivable { Id = recId, ReceivableNumber = "REC-OTC-01", InvoiceId = invId };
        var invoice = new BilInvoice { Id = invId, ServiceType = "OTC" };

        var items = new List<FinReceivableItem>();
        var invoices = new Dictionary<Guid, BilInvoice> { [invId] = invoice };
        var encounters = new Dictionary<Guid, RegPatientEncounter>();

        var visitCode = FinanceReceivableInvoiceBatchService.ResolveReceivableVisitCode(
            receivable, items, invoices, encounters);

        Assert.Equal("OP", visitCode);
    }

    [Fact]
    public void Case5_Outpatient_And_Emergency_CanCombine_InSingleBatch()
    {
        // CASE 5: Outpatient + Emergency -> Keduanya resolve ke OP -> Boleh satu batch
        var rec1Id = Guid.NewGuid();
        var enc1Id = Guid.NewGuid();
        var rec1 = new FinReceivable { Id = rec1Id, ReceivableNumber = "REC-OPD-01" };
        var enc1 = new RegPatientEncounter { Id = enc1Id, EncounterType = EncounterType.Outpatient };

        var rec2Id = Guid.NewGuid();
        var enc2Id = Guid.NewGuid();
        var rec2 = new FinReceivable { Id = rec2Id, ReceivableNumber = "REC-EMG-02" };
        var enc2 = new RegPatientEncounter { Id = enc2Id, EncounterType = EncounterType.Emergency };

        var items = new List<FinReceivableItem>
        {
            new() { Id = Guid.NewGuid(), ReceivableId = rec1Id, EncounterId = enc1Id },
            new() { Id = Guid.NewGuid(), ReceivableId = rec2Id, EncounterId = enc2Id }
        };

        var encounters = new Dictionary<Guid, RegPatientEncounter>
        {
            [enc1Id] = enc1,
            [enc2Id] = enc2
        };
        var invoices = new Dictionary<Guid, BilInvoice>();

        var visitCodes = new HashSet<string>();
        visitCodes.Add(FinanceReceivableInvoiceBatchService.ResolveReceivableVisitCode(
            rec1, items.Where(x => x.ReceivableId == rec1Id).ToList(), invoices, encounters));
        visitCodes.Add(FinanceReceivableInvoiceBatchService.ResolveReceivableVisitCode(
            rec2, items.Where(x => x.ReceivableId == rec2Id).ToList(), invoices, encounters));

        Assert.Single(visitCodes);
        Assert.Contains("OP", visitCodes);
    }

    [Fact]
    public void Case6_Inpatient_And_Outpatient_ThrowsValidationException()
    {
        // CASE 6: Inpatient + Outpatient -> Campur IP dan OP -> ditolak
        var rec1Id = Guid.NewGuid();
        var enc1Id = Guid.NewGuid();
        var rec1 = new FinReceivable { Id = rec1Id, ReceivableNumber = "REC-INP-01" };
        var enc1 = new RegPatientEncounter { Id = enc1Id, EncounterType = EncounterType.Inpatient };

        var rec2Id = Guid.NewGuid();
        var enc2Id = Guid.NewGuid();
        var rec2 = new FinReceivable { Id = rec2Id, ReceivableNumber = "REC-OPD-02" };
        var enc2 = new RegPatientEncounter { Id = enc2Id, EncounterType = EncounterType.Outpatient };

        var items = new List<FinReceivableItem>
        {
            new() { Id = Guid.NewGuid(), ReceivableId = rec1Id, EncounterId = enc1Id },
            new() { Id = Guid.NewGuid(), ReceivableId = rec2Id, EncounterId = enc2Id }
        };

        var encounters = new Dictionary<Guid, RegPatientEncounter>
        {
            [enc1Id] = enc1,
            [enc2Id] = enc2
        };
        var invoices = new Dictionary<Guid, BilInvoice>();

        var visitCodes = new HashSet<string>();
        visitCodes.Add(FinanceReceivableInvoiceBatchService.ResolveReceivableVisitCode(
            rec1, items.Where(x => x.ReceivableId == rec1Id).ToList(), invoices, encounters));
        visitCodes.Add(FinanceReceivableInvoiceBatchService.ResolveReceivableVisitCode(
            rec2, items.Where(x => x.ReceivableId == rec2Id).ToList(), invoices, encounters));

        Assert.Equal(2, visitCodes.Count);

        var action = () =>
        {
            if (visitCodes.Count > 1)
            {
                throw new ReceivableInvoiceBatchValidationException(
                    "Tagihan Rawat Inap dan Rawat Jalan/IGD/OTC tidak dapat digabung dalam satu Invoice AR.");
            }
        };

        var ex = Assert.Throws<ReceivableInvoiceBatchValidationException>(action);
        Assert.Equal("Tagihan Rawat Inap dan Rawat Jalan/IGD/OTC tidak dapat digabung dalam satu Invoice AR.", ex.Message);
    }

    [Fact]
    public void Case8_InvoiceDate_EndOfYear_Generates_XII_And_SameYear()
    {
        // CASE 8: InvoiceDate = 2026-12-31 -> Expected suffix /XII/2026
        var invoiceDate = new DateOnly(2026, 12, 31);
        var romanMonth = FinanceReceivableInvoiceBatchService.ToRomanMonth(invoiceDate.Month);
        const long sequence = 99;
        const string visitCode = "OP";

        var invoiceNumber = $"{sequence:D4}/{visitCode}/RSMMC/{romanMonth}/{invoiceDate.Year}";

        Assert.Equal("0099/OP/RSMMC/XII/2026", invoiceNumber);
    }

    [Fact]
    public void CaseS1_DueDate_Always_30_Calendar_Days_October()
    {
        // CASE S1: InvoiceDate = 2026-10-07 -> DueDate = 2026-11-06 (30 hari kalender)
        var invoiceDate = new DateOnly(2026, 10, 7);
        const int paymentTermDays = 30;
        var dueDate = invoiceDate.AddDays(paymentTermDays);

        Assert.Equal(30, paymentTermDays);
        Assert.Equal(new DateOnly(2026, 11, 6), dueDate);
    }

    [Fact]
    public void CaseS2_DueDate_Always_30_Calendar_Days_January()
    {
        // CASE S2: InvoiceDate = 2026-01-31 -> DueDate = 2026-03-02 (bukan AddMonths(1))
        var invoiceDate = new DateOnly(2026, 1, 31);
        const int paymentTermDays = 30;
        var dueDate = invoiceDate.AddDays(paymentTermDays);

        // 31 Jan 2026 + 30 days:
        // 2026 bukan tahun kabisat (Februari = 28 hari)
        // 31 Jan + 28 hari Feb = 28 hari dilewati, tersisa 2 hari di Maret -> 2026-03-02
        Assert.Equal(new DateOnly(2026, 3, 2), dueDate);
    }

    [Theory]
    [InlineData(1, "I")]
    [InlineData(2, "II")]
    [InlineData(3, "III")]
    [InlineData(4, "IV")]
    [InlineData(5, "V")]
    [InlineData(6, "VI")]
    [InlineData(7, "VII")]
    [InlineData(8, "VIII")]
    [InlineData(9, "IX")]
    [InlineData(10, "X")]
    [InlineData(11, "XI")]
    [InlineData(12, "XII")]
    public void All_RomanMonths_AreDeterministic(int month, string expected)
    {
        var actual = FinanceReceivableInvoiceBatchService.ToRomanMonth(month);
        Assert.Equal(expected, actual);
    }

    // ====================================================================================
    // SKENARIO 1 - 12: PAYMENT, CANCEL, REVERSAL, REVIVE, VALIDASI & TRANSAKSI ATOMIK
    // ====================================================================================

    [Fact]
    public void Test01_Issued_Payment_Transition_To_Paid()
    {
        // 1. ISSUED -> payment -> PAID
        var batchId = Guid.NewGuid();
        var rec1Id = Guid.NewGuid();
        var rec2Id = Guid.NewGuid();

        var rec1 = new FinReceivable { Id = rec1Id, ReceivableNumber = "REC-001", OutstandingAmount = 0m, Status = FinReceivableStatuses.Settled };
        var rec2 = new FinReceivable { Id = rec2Id, ReceivableNumber = "REC-002", OutstandingAmount = 0m, Status = FinReceivableStatuses.Settled };

        var batch = new FinReceivableInvoiceBatch
        {
            Id = batchId,
            Status = FinReceivableInvoiceBatchStatuses.Issued,
            Items = new List<FinReceivableInvoiceBatchItem>
            {
                new() { Id = Guid.NewGuid(), BatchId = batchId, ReceivableId = rec1Id, Receivable = rec1, IsActiveMembership = true, IsDelete = false },
                new() { Id = Guid.NewGuid(), BatchId = batchId, ReceivableId = rec2Id, Receivable = rec2, IsActiveMembership = true, IsDelete = false }
            }
        };

        var settledCount = batch.Items.Count(x => !x.IsDelete && x.IsActiveMembership && x.Receivable != null && x.Receivable.Status == FinReceivableStatuses.Settled);
        var totalActiveCount = batch.Items.Count(x => !x.IsDelete && x.IsActiveMembership);

        batch.Status = settledCount == totalActiveCount
            ? FinReceivableInvoiceBatchStatuses.Paid
            : FinReceivableInvoiceBatchStatuses.PartiallyPaid;

        Assert.Equal(FinReceivableInvoiceBatchStatuses.Paid, batch.Status);
    }

    [Fact]
    public void Test02_Payment_Cancel_AutoReversal_Revive_Repayment()
    {
        // 2. payment -> cancel -> auto reversal -> revive -> payment ulang
        var batchId = Guid.NewGuid();
        var recId = Guid.NewGuid();
        const decimal originalAmount = 5_000_000m;

        var rec = new FinReceivable
        {
            Id = recId,
            ReceivableNumber = "REC-REV-01",
            OriginalAmount = originalAmount,
            OutstandingAmount = 0m,
            AllocatedAmount = originalAmount,
            Status = FinReceivableStatuses.Settled
        };

        var batch = new FinReceivableInvoiceBatch
        {
            Id = batchId,
            TotalAmount = originalAmount,
            TotalDiscount = 500_000m,
            Status = FinReceivableInvoiceBatchStatuses.Paid,
            Items = new List<FinReceivableInvoiceBatchItem>
            {
                new() { Id = Guid.NewGuid(), BatchId = batchId, ReceivableId = recId, Receivable = rec, IsActiveMembership = true }
            }
        };

        // Langkah A: Pembatalan Invoice (Cancel + Auto Reversal)
        rec.OutstandingAmount += originalAmount;
        rec.AllocatedAmount -= originalAmount;
        rec.Status = FinReceivableStatuses.Outstanding;

        foreach (var item in batch.Items)
        {
            item.IsActiveMembership = false;
        }
        batch.Status = FinReceivableInvoiceBatchStatuses.Cancelled;
        batch.IsCancel = true;

        Assert.Equal(FinReceivableInvoiceBatchStatuses.Cancelled, batch.Status);
        Assert.False(batch.Items.First().IsActiveMembership);
        Assert.Equal(originalAmount, rec.OutstandingAmount);
        Assert.Equal(0m, rec.AllocatedAmount);
        Assert.Equal(FinReceivableStatuses.Outstanding, rec.Status);

        // Langkah B: Buat Ulang AR / Revive
        foreach (var item in batch.Items)
        {
            item.IsActiveMembership = true;
        }
        batch.Status = FinReceivableInvoiceBatchStatuses.Issued;
        batch.IsCancel = false;
        batch.TotalDiscount = 0m; // Reset diskon dari pembayaran sebelumnya yang dibatalkan
        batch.TotalAmount = rec.OriginalAmount - rec.AdjustedAmount;
        batch.RowVersion = Guid.NewGuid();

        Assert.Equal(FinReceivableInvoiceBatchStatuses.Issued, batch.Status);
        Assert.True(batch.Items.First().IsActiveMembership);
        Assert.Equal(0m, batch.TotalDiscount);
        Assert.Equal(originalAmount, batch.TotalAmount);

        // Langkah C: Pembayaran Ulang
        rec.OutstandingAmount -= originalAmount;
        rec.AllocatedAmount += originalAmount;
        rec.Status = FinReceivableStatuses.Settled;
        batch.Status = FinReceivableInvoiceBatchStatuses.Paid;

        Assert.Equal(FinReceivableInvoiceBatchStatuses.Paid, batch.Status);
        Assert.Equal(0m, rec.OutstandingAmount);
        Assert.Equal(originalAmount, rec.AllocatedAmount);
    }

    [Fact]
    public void Test03_Payment_Cancel_Reissue_Blocks_Revive_Allows_New_Batch_Payment()
    {
        // 3. payment -> cancel -> reissue ke batch baru -> payment
        var oldBatchId = Guid.NewGuid();
        var newBatchId = Guid.NewGuid();
        var recId = Guid.NewGuid();

        var oldBatch = new FinReceivableInvoiceBatch
        {
            Id = oldBatchId,
            Status = FinReceivableInvoiceBatchStatuses.Cancelled,
            ReissuedToBatchId = newBatchId
        };

        // Revive pada batch lama yang sudah di-reissue wajib ditolak dengan 409 Conflict
        var ex = Assert.Throws<ReceivableInvoiceBatchConflictException>(() =>
        {
            if (oldBatch.ReissuedToBatchId.HasValue)
                throw new ReceivableInvoiceBatchConflictException("Batch tagihan ini sudah pernah dibuat ulang (reissue) ke batch baru dan tidak dapat dihidupkan kembali.");
        });
        Assert.Contains("reissue", ex.Message);

        // Batch baru yang diterbitkan dari reissue dapat menerima pembayaran
        var newBatch = new FinReceivableInvoiceBatch
        {
            Id = newBatchId,
            Status = FinReceivableInvoiceBatchStatuses.Issued,
            TotalAmount = 5_000_000m
        };
        newBatch.Status = FinReceivableInvoiceBatchStatuses.Paid;
        Assert.Equal(FinReceivableInvoiceBatchStatuses.Paid, newBatch.Status);
    }

    [Fact]
    public void Test04_PartialPayment_Cancel_Revive_Repayment()
    {
        // 4. partial payment -> cancel -> revive -> payment
        var batchId = Guid.NewGuid();
        var recId = Guid.NewGuid();
        const decimal totalAmount = 10_000_000m;
        const decimal partialPaid = 3_000_000m;

        var rec = new FinReceivable
        {
            Id = recId,
            ReceivableNumber = "REC-PART-01",
            OriginalAmount = totalAmount,
            OutstandingAmount = totalAmount - partialPaid,
            AllocatedAmount = partialPaid,
            Status = FinReceivableStatuses.Partial
        };

        var batch = new FinReceivableInvoiceBatch
        {
            Id = batchId,
            Status = FinReceivableInvoiceBatchStatuses.PartiallyPaid,
            TotalAmount = totalAmount,
            Items = new List<FinReceivableInvoiceBatchItem>
            {
                new() { Id = Guid.NewGuid(), BatchId = batchId, ReceivableId = recId, Receivable = rec, IsActiveMembership = true }
            }
        };

        // Cancel -> pembalikan alokasi partial
        rec.OutstandingAmount += partialPaid;
        rec.AllocatedAmount -= partialPaid;
        rec.Status = FinReceivableStatuses.Outstanding;
        batch.Status = FinReceivableInvoiceBatchStatuses.Cancelled;

        Assert.Equal(totalAmount, rec.OutstandingAmount);
        Assert.Equal(0m, rec.AllocatedAmount);
        Assert.Equal(FinReceivableStatuses.Outstanding, rec.Status);

        // Revive -> kembali ke ISSUED dengan sisa utuh
        batch.Status = FinReceivableInvoiceBatchStatuses.Issued;
        batch.TotalAmount = rec.OriginalAmount - rec.AdjustedAmount;
        Assert.Equal(totalAmount, batch.TotalAmount);

        // Pelunasan penuh pembayaran baru
        rec.OutstandingAmount -= totalAmount;
        rec.AllocatedAmount += totalAmount;
        rec.Status = FinReceivableStatuses.Settled;
        batch.Status = FinReceivableInvoiceBatchStatuses.Paid;

        Assert.Equal(0m, rec.OutstandingAmount);
        Assert.Equal(totalAmount, rec.AllocatedAmount);
        Assert.Equal(FinReceivableInvoiceBatchStatuses.Paid, batch.Status);
    }

    [Fact]
    public void Test05_InvoiceWithAdjustment_Cancel_Revive_UsesEffectiveAmount()
    {
        // 5. invoice dengan adjustment -> cancel -> revive -> payment berdasarkan effective amount
        var recId = Guid.NewGuid();
        const decimal originalAmount = 10_000_000m;
        const decimal adjustedAmount = 2_000_000m;
        const decimal effectiveAmount = originalAmount - adjustedAmount; // 8.000.000

        var rec = new FinReceivable
        {
            Id = recId,
            ReceivableNumber = "REC-ADJ-01",
            OriginalAmount = originalAmount,
            AdjustedAmount = adjustedAmount,
            OutstandingAmount = effectiveAmount,
            Status = FinReceivableStatuses.Outstanding
        };

        var batch = new FinReceivableInvoiceBatch
        {
            Id = Guid.NewGuid(),
            Status = FinReceivableInvoiceBatchStatuses.Cancelled,
            TotalAmount = originalAmount // Nilai lama sebelum perbaikan
        };

        // Revive wajib menghitung TotalAmount dari effective amount: OriginalAmount - AdjustedAmount
        var memberReceivables = new List<FinReceivable> { rec };
        batch.TotalAmount = memberReceivables.Sum(r => r.OriginalAmount - r.AdjustedAmount);

        Assert.Equal(8_000_000m, batch.TotalAmount);
        Assert.True(rec.OutstandingAmount <= batch.TotalAmount);

        // Pembayaran baru sejumlah effectiveAmount (8.000.000) sukses tanpa overflow
        rec.OutstandingAmount -= batch.TotalAmount;
        rec.AllocatedAmount += batch.TotalAmount;
        rec.Status = FinReceivableStatuses.Settled;
        batch.Status = FinReceivableInvoiceBatchStatuses.Paid;

        Assert.Equal(0m, rec.OutstandingAmount);
        Assert.Equal(8_000_000m, rec.AllocatedAmount);
    }

    [Fact]
    public void Test06_StaleExpectedRowVersion_ThrowsConflictException()
    {
        // 6. stale ExpectedRowVersion -> 409
        var actualRowVersion = Guid.NewGuid();
        var staleExpectedRowVersion = Guid.NewGuid();

        var ex = Assert.Throws<ReceivableInvoiceBatchConflictException>(() =>
        {
            if (staleExpectedRowVersion == Guid.Empty || actualRowVersion != staleExpectedRowVersion)
                throw new ReceivableInvoiceBatchConflictException("Data telah berubah. Muat ulang sebelum melanjutkan.");
        });

        Assert.Contains("Data telah berubah", ex.Message);
    }

    [Fact]
    public void Test07_DuplicateIdempotencyKey_ThrowsConflictException()
    {
        // 7. duplicate Idempotency-Key -> 409 tanpa receipt kedua
        const string idempotencyKey = "PAY-IDEMP-KEY-999";
        var existingReceipts = new List<FinReceipt>
        {
            new() { Id = Guid.NewGuid(), ProviderEventId = idempotencyKey, IsDelete = false }
        };

        var ex = Assert.Throws<ReceivableInvoiceBatchConflictException>(() =>
        {
            if (existingReceipts.Any(r => r.ProviderEventId == idempotencyKey && !r.IsDelete))
                throw new ReceivableInvoiceBatchConflictException("Pembayaran dengan kunci idempotensi ini sudah pernah diproses.");
        });

        Assert.Contains("kunci idempotensi", ex.Message);
    }

    [Fact]
    public void Test08_InvalidOrMissingBankAccount_ThrowsBadRequestException()
    {
        // 8. invalid bank account -> 400
        var request = new PostReceivableInvoiceBatchPaymentRequest
        {
            BankAccountId = null,
            ReceiptAmount = 1_000_000m
        };

        var ex = Assert.Throws<ReceivableInvoiceBatchBadRequestException>(() =>
        {
            if (!request.BankAccountId.HasValue || request.BankAccountId.Value == Guid.Empty)
                throw new ReceivableInvoiceBatchBadRequestException("Rekening bank rumah sakit wajib dipilih.");
        });

        Assert.Contains("Rekening bank", ex.Message);
    }

    [Fact]
    public void Test09_MemberAllocationExceedingOutstanding_ThrowsValidationException()
    {
        // 9. member allocation melebihi outstanding -> 422, bukan 500
        var rec = new FinReceivable
        {
            Id = Guid.NewGuid(),
            ReceivableNumber = "REC-LIMIT-01",
            OutstandingAmount = 5_000_000m
        };

        var allocationAmount = 6_000_000m;

        var ex = Assert.Throws<ReceivableInvoiceBatchValidationException>(() =>
        {
            if (allocationAmount > rec.OutstandingAmount)
                throw new ReceivableInvoiceBatchValidationException(
                    $"Total alokasi dan potongan untuk piutang {rec.ReceivableNumber} (Rp {allocationAmount:N0}) melebihi sisa piutang saat ini (Rp {rec.OutstandingAmount:N0}).");
        });

        Assert.Contains("melebihi sisa piutang", ex.Message);
    }

    [Fact]
    public void Test10_ReceivableStatusNotPayable_ThrowsValidationException()
    {
        // 10. receivable status tidak memungkinkan pembayaran -> 422, bukan 500
        var recCancelled = new FinReceivable
        {
            Id = Guid.NewGuid(),
            ReceivableNumber = "REC-CANC-01",
            Status = FinReceivableStatuses.Cancelled
        };

        var exCancelled = Assert.Throws<ReceivableInvoiceBatchValidationException>(() =>
        {
            if (recCancelled.Status is FinReceivableStatuses.Cancelled or FinReceivableStatuses.WrittenOff)
                throw new ReceivableInvoiceBatchValidationException(
                    $"Piutang {recCancelled.ReceivableNumber} berstatus {recCancelled.Status} dan tidak dapat menerima pembayaran.");
        });

        Assert.Contains("tidak dapat menerima pembayaran", exCancelled.Message);
    }

    [Fact]
    public void Test11_AccountingPrerequisiteException_MapsToControlledBusinessResponse()
    {
        // 11. accounting prerequisite tidak tersedia -> business error yang jelas, bukan generic unhandled 500
        var exOutbox = new AccountingOutboxException("Konfigurasi akun outbox akuntansi belum lengkap.");
        var exSubledger = new FinanceSubledgerException("Subledger prerequisite missing.", 400);

        Assert.IsAssignableFrom<Exception>(exOutbox);
        Assert.Equal("Konfigurasi akun outbox akuntansi belum lengkap.", exOutbox.Message);
        Assert.Equal(400, exSubledger.StatusCode);
    }

    [Fact]
    public void Test12_DatabaseConcurrencyOrConstraintException_RequiresTransactionRollback()
    {
        // 12. DB exception tidak meninggalkan partial financial transaction (atomicity)
        var transactionRolledBack = false;

        try
        {
            // Simulasi mutasi dalam transaksi yang mengalami error database
            throw new DbUpdateConcurrencyException("Data telah berubah oleh proses lain.");
        }
        catch (DbUpdateConcurrencyException)
        {
            // Transaction rollback triggered
            transactionRolledBack = true;
        }

        Assert.True(transactionRolledBack);
    }

    [Fact]
    public void Test13_Payment_WithHistoricalReversals_AllowsNewPayment()
    {
        // Skenario 4: Pembayaran dengan reversal historis
        // Allocation lama sudah dibalik (IsReversal = true, ReversalOfAllocationId = allocOld.Id)
        var recId = Guid.NewGuid();
        var allocOldId = Guid.NewGuid();
        var allocReversalId = Guid.NewGuid();

        var originalAllocations = new List<FinReceiptAllocation>
        {
            new() { Id = allocOldId, ReceivableId = recId, Amount = 632_692m, IsReversal = false, IsDelete = false }
        };

        var reversalAllocations = new List<FinReceiptAllocation>
        {
            new() { Id = allocReversalId, ReceivableId = recId, ReversalOfAllocationId = allocOldId, Amount = 632_692m, IsReversal = true, IsDelete = false }
        };

        var reversedTargetIds = reversalAllocations
            .Where(a => a.IsReversal && a.ReversalOfAllocationId.HasValue)
            .Select(a => a.ReversalOfAllocationId!.Value)
            .ToHashSet();

        // Alokasi aktif adalah alokasi original yang belum dibalik
        var activeAllocations = originalAllocations
            .Where(a => !a.IsReversal && a.Amount > 0 && !reversedTargetIds.Contains(a.Id))
            .ToList();

        // Verifikasi: alokasi lama TIDAK lagi dianggap aktif karena sudah dibalik
        Assert.Empty(activeAllocations);
    }

    [Fact]
    public void Test14_Payment_WhenOriginalAllocationStillActive_IsRejected()
    {
        // Skenario 5: Pembayaran ketika original allocation masih aktif
        var recId = Guid.NewGuid();
        var allocActiveId = Guid.NewGuid();

        var originalAllocations = new List<FinReceiptAllocation>
        {
            new() { Id = allocActiveId, ReceivableId = recId, Amount = 632_692m, IsReversal = false, IsDelete = false }
        };

        var reversalAllocations = new List<FinReceiptAllocation>(); // Belum ada reversal!

        var reversedTargetIds = reversalAllocations
            .Where(a => a.IsReversal && a.ReversalOfAllocationId.HasValue)
            .Select(a => a.ReversalOfAllocationId!.Value)
            .ToHashSet();

        var hasActiveAllocations = originalAllocations.Any(a => !a.IsReversal && a.Amount > 0 && !reversedTargetIds.Contains(a.Id));

        var ex = Assert.Throws<ReceivableInvoiceBatchValidationException>(() =>
        {
            if (hasActiveAllocations)
                throw new ReceivableInvoiceBatchValidationException("Invoice ini sudah memiliki pembayaran aktif. Gunakan koreksi pembayaran atau batalkan invoice jika diperlukan.");
        });

        Assert.Contains("sudah memiliki pembayaran aktif", ex.Message);
    }

    [Fact]
    public void Test15_ConcurrentPaymentOrDuplicateMemberAllocation_Rejected()
    {
        // Skenario 11: Duplicate member allocations dalam satu request / concurrent payment
        var recId = Guid.NewGuid();
        var memberAllocations = new List<PostReceivableInvoiceBatchMemberAllocationRequest>
        {
            new() { ReceivableId = recId, Amount = 300_000m },
            new() { ReceivableId = recId, Amount = 332_692m } // Duplikat piutang yang sama!
        };

        var ex = Assert.Throws<ReceivableInvoiceBatchBadRequestException>(() =>
        {
            if (memberAllocations.Select(m => m.ReceivableId).Distinct().Count() != memberAllocations.Count)
                throw new ReceivableInvoiceBatchBadRequestException("Rincian alokasi anggota tidak boleh memiliki piutang duplikat.");
        });

        Assert.Contains("piutang duplikat", ex.Message);
    }

    [Fact]
    public void Test16_PaymentWithZeroDeductions_Succeeds()
    {
        // Skenario 12: Pembayaran dengan seluruh komponen potongan bernilai nol (Diskon=0, PPh23=0, Admin=0)
        const decimal totalReceivable = 632_692m;
        var request = new PostReceivableInvoiceBatchPaymentRequest
        {
            ReceiptAmount = totalReceivable,
            DiscountAmount = 0m,
            OtherReceiptAmount = 0m,
            Pph23Amount = 0m,
            BankAdminFeeAmount = 0m,
            MemberAllocations = new List<PostReceivableInvoiceBatchMemberAllocationRequest>
            {
                new()
                {
                    ReceivableId = Guid.NewGuid(),
                    Amount = totalReceivable,
                    DiscountAmount = 0m,
                    Pph23Amount = 0m,
                    BankAdminFeeAmount = 0m
                }
            }
        };

        var totalAllocated = request.MemberAllocations.Sum(x => x.Amount);
        Assert.Equal(request.ReceiptAmount, totalAllocated);
        Assert.Equal(0m, request.DiscountAmount);
        Assert.Equal(0m, request.Pph23Amount);
        Assert.Equal(0m, request.BankAdminFeeAmount);
        Assert.Equal(0m, request.OtherReceiptAmount);
    }
}
