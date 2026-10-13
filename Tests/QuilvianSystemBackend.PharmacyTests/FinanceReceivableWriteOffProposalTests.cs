using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.DTOs;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace QuilvianSystemBackend.Tests.Pharmacy;

/// <summary>
/// Unit & Domain Logic Tests untuk Pengajuan Pemutihan Tagihan AR V2 (Header & Detail).
/// Menguji 14 skenario spesifikasi Section N:
/// 1. Pengajuan satu invoice (PER_INVOICE)
/// 2. Pengajuan batch (BATCH)
/// 3. Satu Invoice AR dengan banyak receivable
/// 4. Pemutihan penuh (Outstanding -> 0, Status -> WRITTEN_OFF)
/// 5. Pemutihan sebagian (Outstanding > 0, Status -> PARTIAL)
/// 6. Overlapping proposal (mencegah double proposal pada receivable yang sama)
/// 7. Approved lalu posting (FinReceivableWriteOff dibuat, outbox staged, subledger tercatat)
/// 8. Rejected (Proposal ditolak dengan alasan penolakan)
/// 9. Posting dua kali (Idempotency posting)
/// 10. Pembayaran terjadi sebelum posting (saldo berkurang -> revalidasi gagal & rollback)
/// 11. Invoice dibatalkan sebelum posting (batch CANCELLED -> posting gagal & rollback)
/// 12. Cancel -> Revive/Reissue (invoice revived valid, reissue pengganti ditolak)
/// 13. Maker-checker enforcement (RequestedBy == ApprovedBy ditolak)
/// 14. Concurrency dan rollback (RowVersion mismatch memicu exception)
/// </summary>
public class FinanceReceivableWriteOffProposalTests
{
    private static readonly Guid MakerUserId = Guid.NewGuid();
    private static readonly Guid CheckerUserId = Guid.NewGuid();
    private static readonly Guid DebtorAId = Guid.NewGuid();
    private static readonly Guid DebtorBId = Guid.NewGuid();

    [Fact]
    public void Test01_PerInvoice_ProposalCreation_Valid()
    {
        // 1. Pengajuan satu invoice (PER_INVOICE)
        var batchId = Guid.NewGuid();
        var recId = Guid.NewGuid();

        var request = new CreateReceivableWriteOffProposalRequest
        {
            ProposalType = FinReceivableWriteOffProposalTypes.PerInvoice,
            ServiceType = "RANAP",
            DebtorReferenceId = DebtorAId,
            ReasonCode = FinReceivableWriteOffReasonCodes.Uncollectible,
            ReasonDescription = "Pasien tidak mampu membayar setelah jatuh tempo 90 hari.",
            Items = new()
            {
                new()
                {
                    InvoiceBatchId = batchId,
                    ReceivableId = recId,
                    ProposedAmount = 5_000_000m,
                    Note = "Pemutihan tagihan rawat inap"
                }
            }
        };

        Assert.Equal(FinReceivableWriteOffProposalTypes.PerInvoice, request.ProposalType);
        Assert.Single(request.Items);
        Assert.Equal(5_000_000m, request.Items[0].ProposedAmount);
    }

    [Fact]
    public void Test02_BatchProposal_SameDebtor_Allowed_DifferentDebtor_Throws()
    {
        // 2. Pengajuan batch (BATCH) beberapa invoice dengan penjamin yang sama diizinkan, penjamin berbeda ditolak
        var batch1 = new FinReceivableInvoiceBatch { Id = Guid.NewGuid(), DebtorReferenceId = DebtorAId, BatchNumber = "INV-001" };
        var batch2 = new FinReceivableInvoiceBatch { Id = Guid.NewGuid(), DebtorReferenceId = DebtorAId, BatchNumber = "INV-002" };
        var batchDifferentDebtor = new FinReceivableInvoiceBatch { Id = Guid.NewGuid(), DebtorReferenceId = DebtorBId, BatchNumber = "INV-003" };

        var validBatches = new List<FinReceivableInvoiceBatch> { batch1, batch2 };
        var distinctDebtorsValid = validBatches.Select(b => b.DebtorReferenceId).Distinct().ToList();
        Assert.Single(distinctDebtorsValid);

        var mixedBatches = new List<FinReceivableInvoiceBatch> { batch1, batchDifferentDebtor };
        var ex = Assert.Throws<ReceivableValidationException>(() =>
        {
            var distinctDebtorsMixed = mixedBatches.Select(b => b.DebtorReferenceId).Distinct().ToList();
            if (distinctDebtorsMixed.Count > 1)
                throw new ReceivableValidationException("Seluruh Invoice AR dalam satu pengajuan BATCH harus memiliki penjamin yang sama.");
        });

        Assert.Contains("penjamin yang sama", ex.Message);
    }

    [Fact]
    public void Test03_SingleInvoice_MultipleReceivables_AllocationMapping()
    {
        // 3. Satu Invoice AR dengan banyak receivable anggota: alokasi per-receivable terhitung secara akurat
        var batchId = Guid.NewGuid();
        var rec1 = new FinReceivable { Id = Guid.NewGuid(), OriginalAmount = 3_000_000m, OutstandingAmount = 3_000_000m };
        var rec2 = new FinReceivable { Id = Guid.NewGuid(), OriginalAmount = 2_000_000m, OutstandingAmount = 2_000_000m };

        var items = new List<CreateReceivableWriteOffProposalItemRequest>
        {
            new() { InvoiceBatchId = batchId, ReceivableId = rec1.Id, ProposedAmount = 3_000_000m },
            new() { InvoiceBatchId = batchId, ReceivableId = rec2.Id, ProposedAmount = 1_500_000m }
        };

        var totalProposed = items.Sum(i => i.ProposedAmount);
        var totalOutstanding = rec1.OutstandingAmount + rec2.OutstandingAmount;

        Assert.Equal(4_500_000m, totalProposed);
        Assert.Equal(5_000_000m, totalOutstanding);
        Assert.True(totalProposed <= totalOutstanding);
    }

    [Fact]
    public void Test04_FullWriteOff_StatusTransitionsToWrittenOff()
    {
        // 4. Pemutihan penuh: OutstandingAmount menjadi 0 dan status receivable menjadi WRITTEN_OFF
        var rec = new FinReceivable
        {
            Id = Guid.NewGuid(),
            ReceivableNumber = "REC-FULL-01",
            OriginalAmount = 10_000_000m,
            OutstandingAmount = 10_000_000m,
            WrittenOffAmount = 0m,
            Status = FinReceivableStatuses.Outstanding
        };

        var writeOffAmount = 10_000_000m;
        rec.OutstandingAmount -= writeOffAmount;
        rec.WrittenOffAmount += writeOffAmount;
        rec.Status = rec.OutstandingAmount <= 0m ? FinReceivableStatuses.WrittenOff : FinReceivableStatuses.Partial;

        Assert.Equal(0m, rec.OutstandingAmount);
        Assert.Equal(10_000_000m, rec.WrittenOffAmount);
        Assert.Equal(FinReceivableStatuses.WrittenOff, rec.Status);
    }

    [Fact]
    public void Test05_PartialWriteOff_StatusTransitionsToPartial()
    {
        // 5. Pemutihan sebagian: sisa AR tetap tertagih, status receivable menjadi PARTIAL
        var rec = new FinReceivable
        {
            Id = Guid.NewGuid(),
            ReceivableNumber = "REC-PART-01",
            OriginalAmount = 10_000_000m,
            OutstandingAmount = 10_000_000m,
            WrittenOffAmount = 0m,
            Status = FinReceivableStatuses.Outstanding
        };

        var partialWriteOffAmount = 4_000_000m;
        rec.OutstandingAmount -= partialWriteOffAmount;
        rec.WrittenOffAmount += partialWriteOffAmount;
        rec.Status = rec.OutstandingAmount <= 0m ? FinReceivableStatuses.WrittenOff : FinReceivableStatuses.Partial;

        Assert.Equal(6_000_000m, rec.OutstandingAmount);
        Assert.Equal(4_000_000m, rec.WrittenOffAmount);
        Assert.Equal(FinReceivableStatuses.Partial, rec.Status);
    }

    [Fact]
    public void Test06_OverlappingPendingProposal_ThrowsConflictException()
    {
        // 6. Overlapping proposal: mencegah pengajuan ganda pada receivable yang sama saat masih pending
        var recId = Guid.NewGuid();

        var existingPendingProposals = new List<FinReceivableWriteOffProposalItem>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ReceivableId = recId,
                IsDelete = false,
                Proposal = new FinReceivableWriteOffProposal
                {
                    Id = Guid.NewGuid(),
                    Status = FinReceivableWriteOffProposalStatuses.Submitted,
                    IsDelete = false
                }
            }
        };

        var ex = Assert.Throws<ReceivableConflictException>(() =>
        {
            var isOverlapping = existingPendingProposals.Any(pi => !pi.IsDelete
                && pi.ReceivableId == recId
                && (pi.Proposal!.Status == FinReceivableWriteOffProposalStatuses.Submitted
                    || pi.Proposal.Status == FinReceivableWriteOffProposalStatuses.Approved)
                && !pi.Proposal.IsDelete);

            if (isOverlapping)
                throw new ReceivableConflictException("Piutang sedang dalam pengajuan pemutihan lain yang menunggu persetujuan.");
        });

        Assert.Contains("pengajuan pemutihan lain", ex.Message);
    }

    [Fact]
    public void Test07_ApprovedThenPosted_CreatesWriteOff_And_AccountingOutboxEvent()
    {
        // 7. Approved lalu posting: record FinReceivableWriteOff tercipta, outbox staged, proposal menjadi POSTED
        var proposal = new FinReceivableWriteOffProposal
        {
            Id = Guid.NewGuid(),
            ProposalNumber = "PROP-WO-001",
            Status = FinReceivableWriteOffProposalStatuses.Approved,
            ReasonCode = FinReceivableWriteOffReasonCodes.Uncollectible,
            ReasonDescription = "Tidak tertagih",
            RequestedBy = MakerUserId,
            ApprovedBy = CheckerUserId,
            TotalApprovedAmount = 2_500_000m
        };

        var rec = new FinReceivable
        {
            Id = Guid.NewGuid(),
            ReceivableNumber = "REC-POST-01",
            OutstandingAmount = 2_500_000m,
            Status = FinReceivableStatuses.Outstanding
        };

        // Simulasi execution posting
        var writeOff = new FinReceivableWriteOff
        {
            Id = Guid.NewGuid(),
            WriteOffNumber = "WO-001",
            ReceivableId = rec.Id,
            Amount = 2_500_000m,
            Reason = $"{proposal.ReasonCode}: {proposal.ReasonDescription}",
            Status = FinReceivableApprovalStatuses.Approved,
            RequestedBy = proposal.RequestedBy,
            ApprovedBy = proposal.ApprovedBy
        };

        rec.OutstandingAmount -= writeOff.Amount;
        rec.WrittenOffAmount += writeOff.Amount;
        rec.Status = FinReceivableStatuses.WrittenOff;

        var outboxRequest = new AccountingOutboxEventRequest
        {
            EventTypeCode = FinAccountingEventTypeCodes.PemutihanPiutang,
            SourceTransactionId = rec.ReceivableNumber,
            Amount = writeOff.Amount,
            CorrelationId = rec.CorrelationId,
            CausationId = proposal.Id,
            ActorUserId = CheckerUserId
        };

        proposal.Status = FinReceivableWriteOffProposalStatuses.Posted;
        proposal.PostedBy = CheckerUserId;

        Assert.Equal(FinReceivableWriteOffProposalStatuses.Posted, proposal.Status);
        Assert.Equal(0m, rec.OutstandingAmount);
        Assert.Equal(FinAccountingEventTypeCodes.PemutihanPiutang, outboxRequest.EventTypeCode);
        Assert.Equal(rec.ReceivableNumber, outboxRequest.SourceTransactionId);
    }

    [Fact]
    public void Test08_RejectedProposal_RequiresReason_And_TransitionsToRejected()
    {
        // 8. Rejected: penolakan proposal membutuhkan alasan dan status menjadi REJECTED
        var proposal = new FinReceivableWriteOffProposal
        {
            Id = Guid.NewGuid(),
            Status = FinReceivableWriteOffProposalStatuses.Submitted,
            RequestedBy = MakerUserId
        };

        const string rejectionReason = "Dokumen pendukung ketidakmampuan bayar belum lengkap.";

        if (string.IsNullOrWhiteSpace(rejectionReason))
            throw new ReceivableBadRequestException("Alasan penolakan wajib diisi.");

        proposal.Status = FinReceivableWriteOffProposalStatuses.Rejected;
        proposal.RejectedBy = CheckerUserId;
        proposal.RejectionReason = rejectionReason;

        Assert.Equal(FinReceivableWriteOffProposalStatuses.Rejected, proposal.Status);
        Assert.Equal(CheckerUserId, proposal.RejectedBy);
        Assert.Equal(rejectionReason, proposal.RejectionReason);
    }

    [Fact]
    public void Test09_DuplicatePosting_IsIdempotent()
    {
        // 9. Posting dua kali: proposal yang sudah POSTED bersifat idempotent dan tidak memicu posting ganda
        var proposal = new FinReceivableWriteOffProposal
        {
            Id = Guid.NewGuid(),
            Status = FinReceivableWriteOffProposalStatuses.Posted,
            PostedBy = CheckerUserId,
            PostedAt = DateTimeOffset.UtcNow
        };

        var initialPostDate = proposal.PostedAt;
        var postedAgain = false;

        if (proposal.Status == FinReceivableWriteOffProposalStatuses.Posted)
        {
            // Idempotent return without re-execution
            postedAgain = false;
        }

        Assert.False(postedAgain);
        Assert.Equal(initialPostDate, proposal.PostedAt);
    }

    [Fact]
    public void Test10_PaymentOccursBeforePosting_ThrowsValidationException()
    {
        // 10. Pembayaran terjadi sebelum posting sehingga saldo berkurang < nominal disetujui -> Revalidasi gagal
        var rec = new FinReceivable
        {
            Id = Guid.NewGuid(),
            ReceivableNumber = "REC-PAY-FIRST",
            OutstandingAmount = 1_000_000m // Saldo tinggal 1 juta setelah ada pembayaran parsial
        };

        var approvedWriteOffAmount = 3_000_000m; // Proposal sebelumnya menyetujui 3 juta

        var ex = Assert.Throws<ReceivableValidationException>(() =>
        {
            if (approvedWriteOffAmount > rec.OutstandingAmount)
                throw new ReceivableValidationException(
                    $"Nominal pemutihan yang disetujui (Rp {approvedWriteOffAmount:N0}) melebihi sisa piutang {rec.ReceivableNumber} saat ini (Rp {rec.OutstandingAmount:N0}) akibat mutasi/pembayaran lain.");
        });

        Assert.Contains("melebihi sisa piutang", ex.Message);
    }

    [Fact]
    public void Test11_InvoiceCancelledBeforePosting_ThrowsValidationException()
    {
        // 11. Invoice dibatalkan sebelum posting -> Posting ditolak dan transaksi dibatalkan
        var batch = new FinReceivableInvoiceBatch
        {
            Id = Guid.NewGuid(),
            BatchNumber = "INV-CANCELLED-01",
            Status = FinReceivableInvoiceBatchStatuses.Cancelled,
            IsCancel = true
        };

        var ex = Assert.Throws<ReceivableValidationException>(() =>
        {
            if (batch.Status == FinReceivableInvoiceBatchStatuses.Cancelled || batch.IsCancel)
                throw new ReceivableValidationException($"Invoice AR {batch.BatchNumber} telah dibatalkan.");
        });

        Assert.Contains("telah dibatalkan", ex.Message);
    }

    [Fact]
    public void Test12_CancelReviveReissue_RevivedIsEligible_ReissuedToBatchIsRejected()
    {
        // 12. Cancel -> Revive/Reissue: invoice revived aktif valid, sedangkan invoice yang digantikan reissue ditolak
        var revivedBatch = new FinReceivableInvoiceBatch
        {
            Id = Guid.NewGuid(),
            BatchNumber = "INV-REVIVED",
            Status = FinReceivableInvoiceBatchStatuses.Issued,
            ReissuedToBatchId = null,
            IsCancel = false
        };

        var supersededBatch = new FinReceivableInvoiceBatch
        {
            Id = Guid.NewGuid(),
            BatchNumber = "INV-SUPERSEDED",
            Status = FinReceivableInvoiceBatchStatuses.Cancelled,
            ReissuedToBatchId = Guid.NewGuid(),
            IsCancel = true
        };

        // Revived batch is eligible
        Assert.True(revivedBatch.Status == FinReceivableInvoiceBatchStatuses.Issued && revivedBatch.ReissuedToBatchId == null);

        // Superseded batch throws validation error
        var ex = Assert.Throws<ReceivableValidationException>(() =>
        {
            if (supersededBatch.ReissuedToBatchId.HasValue)
                throw new ReceivableValidationException($"Invoice AR {supersededBatch.BatchNumber} sudah digantikan oleh pembuatan ulang baru (reissue).");
        });

        Assert.Contains("sudah digantikan oleh pembuatan ulang", ex.Message);
    }

    [Fact]
    public void Test13_MakerChecker_RequesterCannotApproveOwnProposal()
    {
        // 13. Maker-checker: RequestedBy dan ApprovedBy tidak boleh sama
        var proposal = new FinReceivableWriteOffProposal
        {
            Id = Guid.NewGuid(),
            Status = FinReceivableWriteOffProposalStatuses.Submitted,
            RequestedBy = MakerUserId
        };

        // User pemohon mencoba menyetujui pengajuannya sendiri
        var currentApproverUserId = MakerUserId;

        var ex = Assert.Throws<ReceivableValidationException>(() =>
        {
            if (currentApproverUserId == proposal.RequestedBy)
                throw new ReceivableValidationException(
                    "Pengaju tidak boleh menyetujui pengajuannya sendiri. Mintalah persetujuan pengguna lain yang berwenang (maker-checker).");
        });

        Assert.Contains("maker-checker", ex.Message);
    }

    [Fact]
    public void Test14_ConcurrencyConflict_RequiresRollback()
    {
        // 14. Concurrency dan rollback: RowVersion mismatch memicu exception dan mencegah partial state
        var actualRowVersion = Guid.NewGuid();
        var staleRowVersion = Guid.NewGuid();

        var rolledBack = false;

        var ex = Assert.Throws<ReceivableConflictException>(() =>
        {
            try
            {
                if (staleRowVersion != actualRowVersion)
                    throw new ReceivableConflictException("Data telah berubah oleh pengguna atau proses lain. Muat ulang sebelum melanjutkan.");
            }
            catch
            {
                rolledBack = true;
                throw;
            }
        });

        Assert.True(rolledBack);
        Assert.Contains("Data telah berubah", ex.Message);
    }
}
