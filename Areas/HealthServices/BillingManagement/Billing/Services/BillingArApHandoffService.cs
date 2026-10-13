using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Dtos;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Data;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

/// <summary>
/// Billing hanya merekam dan mengekspos fakta handoff AR/AP secara idempotent; belum ada
/// konsumen AR/AP nyata di repository ini, sehingga tidak ada pengiriman aktif yang dibangun.
/// </summary>
public sealed class BillingArApHandoffService
{
    private const string LogCategory = "HealthServices.BillingManagement.Billing";
    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;
    private readonly EmployeeBenefitBalanceService? _balanceService;

    public BillingArApHandoffService(
        ApplicationDbContext dbContext,
        LoggerService loggerService,
        EmployeeBenefitBalanceService? balanceService = null)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
        _balanceService = balanceService;
    }

    /// <summary>
    /// Menambahkan entity handoff ke context yang sama tanpa membuka transaksi sendiri;
    /// pemanggil (BillingFinalizationService) yang memegang batas transaksi dan SaveChanges.
    /// </summary>
    public async Task StageHandoffsForFinalizationAsync(
        BilInvoice invoice,
        BilCalculationVersion calculation,
        BilFinalizationRecord finalizationRecord,
        decimal outstandingAtFinalization,
        bool isDepartureException,
        Guid actorUserId,
        CancellationToken cancellationToken,
        EmployeeBenefitSplitCalculationResult? employeeBenefitSplit = null)
    {
        var now = DateTimeOffset.UtcNow;

        if (isDepartureException && outstandingAtFinalization > 0)
        {
            _dbContext.BilArHandoffs.Add(new BilArHandoff
            {
                InvoiceId = invoice.Id,
                Invoice = invoice,
                FinalizationRecordId = finalizationRecord.Id,
                FinalizationRecord = finalizationRecord,
                DebtorType = BillingArDebtorTypes.PatientGuarantor,
                Amount = outstandingAtFinalization,
                DueDate = invoice.InvoiceDate,
                Status = BillingHandoffStatuses.Created,
                HandoffKey = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid(),
                CausationId = finalizationRecord.CorrelationId,
                CreatedAt = now,
                RowVersion = Guid.NewGuid(),
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            });
        }

        if (employeeBenefitSplit != null && employeeBenefitSplit.IsEmployeeBenefitVisit)
        {
            // BE-FIN-107, FIN-DEC-183, FIN-DEC-210: Pembagian dua porsi tagihan penjamin internal.
            // 1. Baris PAYER untuk porsi RS Benefit (P = min(C, S)). Baris nol tidak diterbitkan.
            if (employeeBenefitSplit.PayerAmount > 0)
            {
                var payerHandoffKey = employeeBenefitSplit.PayerHandoffKey ?? Guid.NewGuid();
                employeeBenefitSplit.PayerHandoffKey = payerHandoffKey;

                _dbContext.BilArHandoffs.Add(new BilArHandoff
                {
                    InvoiceId = invoice.Id,
                    Invoice = invoice,
                    FinalizationRecordId = finalizationRecord.Id,
                    FinalizationRecord = finalizationRecord,
                    DebtorType = BillingArDebtorTypes.Payer,
                    DebtorReferenceId = employeeBenefitSplit.CompanyGuarantorId,
                    Amount = employeeBenefitSplit.PayerAmount,
                    DueDate = invoice.InvoiceDate,
                    Status = BillingHandoffStatuses.Created,
                    HandoffKey = payerHandoffKey,
                    CorrelationId = Guid.NewGuid(),
                    CausationId = finalizationRecord.CorrelationId,
                    CreatedAt = now,
                    RowVersion = Guid.NewGuid(),
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = actorUserId
                });
            }

            // 2. Baris EMPLOYEE_BENEFIT untuk porsi kelebihan plafon (E = C - P) ditambah pilihan utang pegawai (D).
            // Baris nol tidak diterbitkan.
            if (employeeBenefitSplit.EmployeeReceivableAmount > 0)
            {
                _dbContext.BilArHandoffs.Add(new BilArHandoff
                {
                    InvoiceId = invoice.Id,
                    Invoice = invoice,
                    FinalizationRecordId = finalizationRecord.Id,
                    FinalizationRecord = finalizationRecord,
                    DebtorType = BillingArDebtorTypes.EmployeeBenefit,
                    DebtorReferenceId = null,
                    BenefitOwnerId = employeeBenefitSplit.EmployeeId,
                    BenefitRelationship = employeeBenefitSplit.RelationshipCode,
                    DeferredPatientAmount = employeeBenefitSplit.ChoiceAmount,
                    DeferredConsentGivenBy = employeeBenefitSplit.ConsentGivenBy,
                    Amount = employeeBenefitSplit.EmployeeReceivableAmount,
                    DueDate = invoice.InvoiceDate,
                    Status = BillingHandoffStatuses.Created,
                    HandoffKey = Guid.NewGuid(),
                    CorrelationId = Guid.NewGuid(),
                    CausationId = finalizationRecord.CorrelationId,
                    CreatedAt = now,
                    RowVersion = Guid.NewGuid(),
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = actorUserId
                });
            }
        }
        else
        {
            var payerAmount = calculation.PrimaryAmount + calculation.ExcessAmount;
            if (payerAmount > 0)
            {
                // FIN-CAP-069: penjamin kunjungan dipilih memakai urutan yang sama dengan
                // BillingDepositService — penjamin yang sudah dibatalkan dibuang, lalu penjamin utama
                // diutamakan dan sisanya mengikuti Priority. Tanpa urutan ini, kunjungan dengan lebih
                // dari satu penjamin memulangkan baris yang berbeda-beda setiap kali dijalankan.
                var guarantor = await _dbContext.RegPatientEncounterGuarantors.AsNoTracking()
                    .Where(x => x.EncounterId == invoice.EncounterId && x.IsActive && !x.IsDelete && !x.IsCancel)
                    .OrderByDescending(x => x.IsPrimary)
                    .ThenBy(x => x.Priority)
                    .FirstOrDefaultAsync(cancellationToken);
                _dbContext.BilArHandoffs.Add(new BilArHandoff
                {
                    InvoiceId = invoice.Id,
                    Invoice = invoice,
                    FinalizationRecordId = finalizationRecord.Id,
                    FinalizationRecord = finalizationRecord,
                    DebtorType = BillingArDebtorTypes.Payer,
                    // FIN-CAP-069: kunjungan yang dijamin PERUSAHAAN tidak punya InsuranceProviderId,
                    // sehingga sebelumnya piutangnya lahir tanpa identitas debitur dan tidak dapat
                    // dikelompokkan maupun digabung ke Batch Tagihan. Urutan InsuranceProviderId lalu
                    // CompanyGuarantorId mengikuti BillingDepositService, dan Finance sudah memetakan
                    // nilai ini ke kedua master saat menampilkan nama penjamin.
                    DebtorReferenceId = guarantor?.InsuranceProviderId ?? guarantor?.CompanyGuarantorId,
                    Amount = payerAmount,
                    DueDate = invoice.InvoiceDate,
                    Status = BillingHandoffStatuses.Created,
                    HandoffKey = Guid.NewGuid(),
                    CorrelationId = Guid.NewGuid(),
                    CausationId = finalizationRecord.CorrelationId,
                    CreatedAt = now,
                    RowVersion = Guid.NewGuid(),
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = actorUserId
                });
            }
        }

        var doctorId = await _dbContext.RegPatientEncounters.AsNoTracking()
            .Where(x => x.Id == invoice.EncounterId)
            .Select(x => x.DoctorId)
            .FirstOrDefaultAsync(cancellationToken);
        if (doctorId.HasValue)
        {
            var grossDoctorShare = await _dbContext.BilInvoiceItems.AsNoTracking()
                .Where(x => x.InvoiceId == invoice.Id
                    && x.Status == BillingInvoiceItemStatuses.Active && !x.IsDelete)
                .SumAsync(x => (decimal?)x.DoctorShare, cancellationToken) ?? 0;
            var doctorDiscountTotal = await _dbContext.BilDiscountApplications.AsNoTracking()
                .Where(x => x.InvoiceId == invoice.Id && x.DiscountType == DiscountPolicyValues.Doctor
                    && x.ApprovalStatus == BillingDiscountApprovalStatuses.Approved && !x.IsDelete)
                .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0;
            var apAmount = Math.Max(grossDoctorShare - doctorDiscountTotal, 0);
            if (apAmount > 0)
            {
                var isReady = !isDepartureException && payerAmount == 0;
                _dbContext.BilApHandoffs.Add(new BilApHandoff
                {
                    InvoiceId = invoice.Id,
                    Invoice = invoice,
                    FinalizationRecordId = finalizationRecord.Id,
                    FinalizationRecord = finalizationRecord,
                    DoctorId = doctorId.Value,
                    Amount = apAmount,
                    ReadinessStatus = isReady
                        ? BillingApReadinessStatuses.Ready
                        : BillingApReadinessStatuses.NotReady,
                    Status = BillingHandoffStatuses.Created,
                    HandoffKey = Guid.NewGuid(),
                    CorrelationId = Guid.NewGuid(),
                    CausationId = finalizationRecord.CorrelationId,
                    CreatedAt = now,
                    ReadyAt = isReady ? now : null,
                    RowVersion = Guid.NewGuid(),
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = actorUserId
                });
            }
        }
    }

    /// <summary>
    /// Dipanggil setelah adjustment/write-off diposting; hanya membuat correction bila invoice
    /// sudah FINAL atau CLOSED dan memiliki AR handoff yang ada. Idempotent per source
    /// (at-least-once safe). BKC-DES-035/BKC-DEC-101: CLOSED diterima sejak invoice lunas kini
    /// dapat berpindah otomatis ke CLOSED (BKC-DEC-100) - tanpa ini, koreksi yang diposting
    /// SESUDAH tagihan lunas (kasus paling umum) akan gagal tercatat diam-diam, persis lubang
    /// yang didokumentasikan pada temuan 2 September 2026. OPEN dan SETTLED_BY_WRITE_OFF tetap
    /// ditolak - belum pernah ada handoff AR untuk OPEN, dan piutang SETTLED_BY_WRITE_OFF sudah
    /// dihapusbukukan (pertanyaan yang berbeda, belum diputuskan).
    /// </summary>
    public async Task RecordCorrectionIfLinkedAsync(
        Guid invoiceId,
        Guid? sourceAdjustmentId,
        Guid? sourceWriteOffCaseId,
        string direction,
        decimal amount,
        string reason,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var invoice = await _dbContext.BilInvoices.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == invoiceId && !x.IsDelete, cancellationToken);
        if (invoice is null
            || (invoice.Status != BillingInvoiceStatuses.Final
                && invoice.Status != BillingInvoiceStatuses.Closed))
            return;

        var arHandoff = await _dbContext.BilArHandoffs.AsNoTracking()
            .Where(x => x.InvoiceId == invoiceId && !x.IsDelete)
            .OrderBy(x => x.DebtorType == BillingArDebtorTypes.PatientGuarantor ? 0 : 1)
            .FirstOrDefaultAsync(cancellationToken);
        if (arHandoff is null) return;

        var sourceKey = sourceAdjustmentId ?? sourceWriteOffCaseId;
        if (!sourceKey.HasValue) return;
        IDbContextTransaction? transaction = null;

        try
        {
            if (_dbContext.Database.IsRelational())
            {
                transaction = await _dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable, cancellationToken);
                await AcquireLockAsync($"BIL_HANDOFF_CORRECTION_{sourceKey.Value:N}", cancellationToken);
            }

            var exists = await _dbContext.BilHandoffAdjustments.AsNoTracking().AnyAsync(
                x => (sourceAdjustmentId.HasValue && x.SourceAdjustmentId == sourceAdjustmentId)
                    || (sourceWriteOffCaseId.HasValue && x.SourceWriteOffCaseId == sourceWriteOffCaseId),
                cancellationToken);
            if (exists)
            {
                if (transaction is not null) await transaction.CommitAsync(cancellationToken);
                return;
            }

            var now = DateTimeOffset.UtcNow;
            var correction = new BilHandoffAdjustment
            {
                ArHandoffId = arHandoff.Id,
                SourceAdjustmentId = sourceAdjustmentId,
                SourceWriteOffCaseId = sourceWriteOffCaseId,
                Direction = direction,
                Amount = amount,
                Reason = reason,
                CorrelationId = Guid.NewGuid(),
                CausationId = sourceKey.Value,
                CreatedAt = now,
                RowVersion = Guid.NewGuid(),
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };
            _dbContext.BilHandoffAdjustments.Add(correction);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // BE-FIN-108: Kembalikan plafon benefit internal di HR bila tagihan dikoreksi (FIN-DEC-202, FIN-DEC-205, R.3.3)
            if (_balanceService is not null)
            {
                var payerHandoff = await _dbContext.BilArHandoffs.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.InvoiceId == invoiceId
                        && x.DebtorType == BillingArDebtorTypes.Payer
                        && !x.IsDelete, cancellationToken);

                if (payerHandoff is not null)
                {
                    if (string.Equals(direction, BillingAdjustmentDirections.Credit, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(direction, "CANCEL", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(direction, "REVERSAL", StringComparison.OrdinalIgnoreCase))
                    {
                        await _balanceService.ReturnAsync(new BenefitBalanceReturnRequest
                        {
                            SourceHandoffKey = payerHandoff.HandoffKey,
                            Reason = reason,
                            ActorUserId = actorUserId
                        }, cancellationToken);
                    }
                }
            }

            if (transaction is not null) await transaction.CommitAsync(cancellationToken);
            await AuditCorrectionAsync(correction, actorUserId);
        }
        catch (DbUpdateException)
        {
            // Correction sudah tercatat oleh percobaan lain (at-least-once); aman diabaikan.
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
        }
        catch
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }

    public async Task<HandoffStatusResponse> GetHandoffStatusAsync(
        Guid finalizationRecordId,
        CancellationToken cancellationToken)
    {
        var record = await _dbContext.BilFinalizationRecords.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == finalizationRecordId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Finalization record tidak ditemukan.");

        var arHandoffs = await _dbContext.BilArHandoffs.AsNoTracking()
            .Where(x => x.FinalizationRecordId == finalizationRecordId && !x.IsDelete)
            .ToListAsync(cancellationToken);
        var apHandoffs = await _dbContext.BilApHandoffs.AsNoTracking()
            .Where(x => x.FinalizationRecordId == finalizationRecordId && !x.IsDelete)
            .ToListAsync(cancellationToken);
        var arIds = arHandoffs.Select(x => x.Id).ToHashSet();
        var apIds = apHandoffs.Select(x => x.Id).ToHashSet();
        var adjustments = await _dbContext.BilHandoffAdjustments.AsNoTracking()
            .Where(x => !x.IsDelete)
            .ToListAsync(cancellationToken);
        var linkedAdjustments = adjustments.Where(x =>
            (x.ArHandoffId.HasValue && arIds.Contains(x.ArHandoffId.Value))
            || (x.ApHandoffId.HasValue && apIds.Contains(x.ApHandoffId.Value)))
            .ToList();

        return new HandoffStatusResponse
        {
            InvoiceId = record.InvoiceId,
            FinalizationRecordId = record.Id,
            ArHandoffs = arHandoffs.Select(MapAr).ToList(),
            ApHandoffs = apHandoffs.Select(MapAp).ToList(),
            Adjustments = linkedAdjustments.Select(MapAdjustment).ToList()
        };
    }

    public async Task<HandoffStatusResponse> GetHandoffStatusByInvoiceAsync(
        Guid invoiceId,
        CancellationToken cancellationToken)
    {
        var record = await _dbContext.BilFinalizationRecords.AsNoTracking()
            .Where(x => x.InvoiceId == invoiceId && !x.IsDelete)
            .OrderByDescending(x => x.FinalizedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (record is null)
        {
            throw new KeyNotFoundException($"Catatan finalisasi untuk invoice '{invoiceId}' tidak ditemukan.");
        }

        return await GetHandoffStatusAsync(record.Id, cancellationToken);
    }

    private Task AcquireLockAsync(string key, CancellationToken cancellationToken) =>
        _dbContext.Database.ExecuteSqlRawAsync(
            "SELECT pg_advisory_xact_lock(hashtext({0}));", [key], cancellationToken);

    private Task AuditCorrectionAsync(BilHandoffAdjustment correction, Guid actorUserId) =>
        _loggerService.AuditAsync(
            LogCategory,
            "BillingHandoff.Correction",
            "Koreksi post-final ditautkan ke handoff AR/AP tanpa memutasi posting asal.",
            new
            {
                HandoffAdjustmentId = correction.Id,
                correction.ArHandoffId,
                correction.ApHandoffId,
                correction.SourceAdjustmentId,
                correction.SourceWriteOffCaseId,
                correction.Direction,
                correction.Amount,
                correction.CorrelationId,
                ActorUserId = actorUserId
            });

    private static ArHandoffResponse MapAr(BilArHandoff handoff) => new()
    {
        Id = handoff.Id,
        DebtorType = handoff.DebtorType,
        DebtorReferenceId = handoff.DebtorReferenceId,
        Amount = handoff.Amount,
        DueDate = handoff.DueDate,
        Status = handoff.Status,
        HandoffKey = handoff.HandoffKey,
        BenefitOwnerId = handoff.BenefitOwnerId,
        BenefitRelationship = handoff.BenefitRelationship,
        DeferredPatientAmount = handoff.DeferredPatientAmount,
        DeferredConsentGivenBy = handoff.DeferredConsentGivenBy,
        CreatedAt = handoff.CreatedAt,
        AcknowledgedAt = handoff.AcknowledgedAt
    };

    private static ApHandoffResponse MapAp(BilApHandoff handoff) => new()
    {
        Id = handoff.Id,
        DoctorId = handoff.DoctorId,
        Amount = handoff.Amount,
        ReadinessStatus = handoff.ReadinessStatus,
        Status = handoff.Status,
        HandoffKey = handoff.HandoffKey,
        CreatedAt = handoff.CreatedAt,
        ReadyAt = handoff.ReadyAt,
        AcknowledgedAt = handoff.AcknowledgedAt
    };

    private static HandoffAdjustmentResponse MapAdjustment(BilHandoffAdjustment adjustment) => new()
    {
        Id = adjustment.Id,
        Direction = adjustment.Direction,
        Amount = adjustment.Amount,
        Reason = adjustment.Reason,
        CreatedAt = adjustment.CreatedAt
    };

    /// <summary>
    /// Mengembalikan plafon benefit karyawan di HR untuk invoice tertentu (BE-FIN-108, R.3.3, AC 3).
    /// Dipanggil saat koreksi, pembatalan invoice, atau penerbitan serah terima pembalik.
    /// Idempoten: bila sudah pernah dikembalikan atau tidak ada mutasi pemakaian, operasi aman.
    /// </summary>
    public async Task<BenefitBalanceReturnResult?> ReturnBenefitBalanceAsync(
        Guid invoiceId,
        string? reason,
        Guid? actorUserId,
        CancellationToken cancellationToken)
    {
        if (_balanceService is null) return null;

        var payerHandoff = await _dbContext.BilArHandoffs.AsNoTracking()
            .FirstOrDefaultAsync(x => x.InvoiceId == invoiceId
                && x.DebtorType == BillingArDebtorTypes.Payer
                && !x.IsDelete, cancellationToken);
        if (payerHandoff is null) return null;

        if (_dbContext.Database.CurrentTransaction is not null)
        {
            return await _balanceService.ReturnAsync(new BenefitBalanceReturnRequest
            {
                SourceHandoffKey = payerHandoff.HandoffKey,
                Reason = reason,
                ActorUserId = actorUserId
            }, cancellationToken);
        }

        IDbContextTransaction? transaction = null;
        try
        {
            if (_dbContext.Database.IsRelational())
            {
                transaction = await _dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable, cancellationToken);
            }

            var result = await _balanceService.ReturnAsync(new BenefitBalanceReturnRequest
            {
                SourceHandoffKey = payerHandoff.HandoffKey,
                Reason = reason,
                ActorUserId = actorUserId
            }, cancellationToken);

            if (transaction is not null) await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }
}
