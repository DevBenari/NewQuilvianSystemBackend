using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.BillingIntake.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.BillingIntake.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Constants;
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
    private readonly EmployeeBenefitEligibilityService? _eligibilityService;
    private readonly FinanceBillingIntakeService? _intakeService;

    public BillingArApHandoffService(
        ApplicationDbContext dbContext,
        LoggerService loggerService,
        EmployeeBenefitBalanceService? balanceService = null,
        EmployeeBenefitEligibilityService? eligibilityService = null,
        FinanceBillingIntakeService? intakeService = null)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
        _balanceService = balanceService;
        _eligibilityService = eligibilityService;
        _intakeService = intakeService;
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
        EmployeeBenefitSplitCalculationResult? employeeBenefitSplit = null,
        bool isPatientArDeferred = false,
        DateTimeOffset? patientArDueDate = null,
        string? patientArConsentGivenBy = null,
        string? patientArConsentNotes = null)
    {
        var now = DateTimeOffset.UtcNow;

        if ((isDepartureException || isPatientArDeferred) && outstandingAtFinalization > 0)
        {
            _dbContext.BilArHandoffs.Add(new BilArHandoff
            {
                InvoiceId = invoice.Id,
                Invoice = invoice,
                FinalizationRecordId = finalizationRecord.Id,
                FinalizationRecord = finalizationRecord,
                DebtorType = BillingArDebtorTypes.PatientGuarantor,
                Amount = outstandingAtFinalization,
                DueDate = patientArDueDate ?? invoice.InvoiceDate,
                DeferredConsentGivenBy = patientArConsentGivenBy,
                DeferredConsentNotes = patientArConsentNotes,
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

        decimal payerAmount;
        if (employeeBenefitSplit != null && employeeBenefitSplit.IsEmployeeBenefitVisit)
        {
            payerAmount = employeeBenefitSplit.PayerAmount;
            // BE-FIN-107, FIN-DEC-183, FIN-DEC-210: Pembagian dua porsi tagihan penjamin internal.
            // 1. Baris PAYER untuk porsi RS Benefit (P = min(C, S)). Baris nol tidak diterbitkan.
            if (payerAmount > 0)
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
                    // Biaya yang ditanggung RS sendiri (penjamin internal), bukan piutang pihak luar: ditutup lewat Penutupan Benefit.
                    IsInternalBenefit = true,
                    Amount = payerAmount,
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

            // 2. Baris EMPLOYEE_BENEFIT untuk porsi tagihan yang secara eksplisit ditangguhkan menjadi utang pegawai.
            // Baris nol tidak diterbitkan (Section 3.4, 8).
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
                    // BenefitOwnerId tetap bermakna MstEmployee.Id (kosong untuk dokter tanpa data karyawan);
                    // identitas canonical pemilik ada di BenefitOwnerWorkforceProfileId.
                    BenefitOwnerId = employeeBenefitSplit.EmployeeId,
                    BenefitOwnerWorkforceProfileId = employeeBenefitSplit.WorkforceProfileId,
                    BenefitRelationship = employeeBenefitSplit.RelationshipCode,
                    DeferredPatientAmount = employeeBenefitSplit.EmployeeDeferredAmount > 0 ? employeeBenefitSplit.EmployeeDeferredAmount : employeeBenefitSplit.ChoiceAmount,
                    DeferredConsentGivenBy = employeeBenefitSplit.ConsentGivenBy,
                    DeferredConsentNotes = employeeBenefitSplit.ConsentNotes,
                    FinancingChoice = employeeBenefitSplit.FinancingChoice,
                    EmployeeArOrigin = employeeBenefitSplit.EmployeeArOrigin,
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
            payerAmount = calculation.PrimaryAmount + calculation.ExcessAmount;
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
        BenefitOwnerWorkforceProfileId = handoff.BenefitOwnerWorkforceProfileId,
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

    /// <summary>
    /// Mengoreksi pemilik manfaat salah orang pada tagihan yang sudah final lewat Berita Acara Koreksi resmi
    /// (BE-FIN-109; FIN-DEC-173, FIN-DEC-187, FIN-DEC-195, FIN-DEC-200; FR-FIN-231..234; INT-BIL-FIN-002 §P.6).
    /// Menerbitkan serah terima pembalik (BilHandoffAdjustment), mengembalikan plafon pegawai A (KembalikanPlafon),
    /// memotong plafon pegawai B (PakaiPlafon), dan menerbitkan serah terima pengganti (BilArHandoff) atas pegawai B.
    /// Selesai dalam satu transaksi database serializable dan mengonsumsi intake Finance secara sistematis tanpa
    /// manipulasi manual kartu debitur.
    /// </summary>
    public async Task<CorrectBenefitDebtorResponse> CorrectBenefitDebtorAsync(
        Guid invoiceId,
        CorrectBenefitDebtorRequest request,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Validasi input Berita Acara Koreksi
        if (string.IsNullOrWhiteSpace(request.OfficialReportNumber))
            throw new ArgumentException("Nomor Berita Acara Koreksi wajib diisi.");
        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new ArgumentException("Alasan koreksi wajib diisi.");
        var hasCorrectProfile = request.CorrectWorkforceProfileId.HasValue && request.CorrectWorkforceProfileId.Value != Guid.Empty;
        if (request.CorrectEmployeeId == Guid.Empty && !hasCorrectProfile)
            throw new ArgumentException("Pegawai atau dokter pengganti yang benar wajib dipilih.");

        // 1. Validasi keberadaan dan status invoice
        var invoice = await _dbContext.BilInvoices.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == invoiceId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException($"Invoice '{invoiceId}' tidak ditemukan.");

        // AC 4: Sebelum tagihan final, koreksi dilakukan lewat alur 'Ganti Penanggung' existing tanpa Berita Acara
        if (invoice.Status == BillingInvoiceStatuses.Open)
        {
            throw new BillingFinalizationValidationException(
                "Tagihan belum difinalisasi. Koreksi penanggung sebelum tagihan final dilakukan langsung melalui menu Ganti Penanggung tanpa Berita Acara.");
        }

        if (invoice.Status != BillingInvoiceStatuses.Final && invoice.Status != BillingInvoiceStatuses.Closed)
        {
            throw new BillingFinalizationValidationException(
                $"Tagihan berstatus '{invoice.Status}' tidak dapat dikoreksi penanggungnya.");
        }

        // 2. Cari serah terima AR yang sudah ada pada invoice ini
        var handoffs = await _dbContext.BilArHandoffs
            .Where(x => x.InvoiceId == invoiceId && !x.IsDelete)
            .ToListAsync(cancellationToken);

        var oldBenefitHandoff = handoffs.FirstOrDefault(x => x.DebtorType == BillingArDebtorTypes.EmployeeBenefit);
        var payerHandoff = handoffs.FirstOrDefault(x => x.DebtorType == BillingArDebtorTypes.Payer);

        if (oldBenefitHandoff is null && payerHandoff is null)
        {
            throw new BillingFinalizationValidationException(
                "Tagihan ini tidak memiliki serah terima piutang manfaat pegawai atau penjamin internal yang dapat dikoreksi.");
        }

        // Pemilik pengganti diresolusi lewat WorkforceProfileId (karyawan atau dokter tetap); EmployeeId lama
        // diterjemahkan, dan bila keduanya dikirim harus orang yang sama.
        var ownerResolver = _eligibilityService?.OwnerResolver ?? new WorkforceBenefitOwnerResolver(_dbContext);
        var newOwnerResolution = await ownerResolver.ResolveRequestAsync(
            request.CorrectWorkforceProfileId,
            request.CorrectEmployeeId == Guid.Empty ? null : request.CorrectEmployeeId,
            cancellationToken);
        if (!newOwnerResolution.IsResolved)
        {
            throw new BillingFinalizationValidationException(
                newOwnerResolution.Message ?? "Pegawai pengganti tidak ditemukan di HR atau tidak aktif.");
        }
        var newOwner = newOwnerResolution.Owner!;

        // Cegah koreksi ke orang yang sama bila tidak ada pergantian apa pun
        var sameAsCurrentOwner = oldBenefitHandoff is not null
            && (oldBenefitHandoff.BenefitOwnerWorkforceProfileId == newOwner.WorkforceProfileId
                || (oldBenefitHandoff.BenefitOwnerWorkforceProfileId is null
                    && newOwner.EmployeeId.HasValue
                    && oldBenefitHandoff.BenefitOwnerId == newOwner.EmployeeId));
        if (sameAsCurrentOwner && !request.CorrectFamilyMemberId.HasValue)
        {
            throw new BillingFinalizationValidationException(
                "Pegawai pengganti sama dengan pegawai pemilik manfaat saat ini. Tidak ada perubahan yang dilakukan.");
        }

        // 3. Validasi kelayakan pemilik baru B di HR
        string newRelationshipCode = BenefitRelationshipCodes.Self;
        string? newEmployeeName = null;
        if (_eligibilityService is not null)
        {
            var eligibility = await _eligibilityService.CheckAsync(new BenefitEligibilityRequest
            {
                WorkforceProfileId = newOwner.WorkforceProfileId,
                FamilyMemberId = request.CorrectFamilyMemberId,
                ServiceDate = invoice.InvoiceDate?.Date ?? invoice.VisitDate.Date
            }, cancellationToken);

            if (!eligibility.IsEligible)
            {
                throw new BillingFinalizationValidationException(
                    $"Pegawai pengganti tidak layak menerima fasilitas benefit: {eligibility.Message}");
            }

            newRelationshipCode = eligibility.RelationshipCode ?? BenefitRelationshipCodes.Self;
            newEmployeeName = eligibility.StaffName ?? eligibility.EmployeeName;
        }
        else
        {
            newEmployeeName = newOwner.StaffName;
        }

        // 4. Hitung porsi cakupan aturan RS (C) dan pembagian untuk pegawai B
        var oldEmployeeAmount = oldBenefitHandoff?.Amount ?? 0m;
        var oldDeferredChoice = oldBenefitHandoff?.DeferredPatientAmount ?? 0m;
        var oldPayerAmount = payerHandoff?.Amount ?? 0m;
        var oldExcess = Math.Max(oldEmployeeAmount - oldDeferredChoice, 0m);
        var coveredByRuleC = oldPayerAmount + oldExcess;

        decimal remainingAllowanceB = 0m;
        string? personKindB = null;
        Guid? personIdB = null;

        if (_balanceService is not null)
        {
            var balanceB = await _balanceService.ReadAsync(new BenefitBalanceReadRequest
            {
                WorkforceProfileId = newOwner.WorkforceProfileId,
                FamilyMemberId = request.CorrectFamilyMemberId,
                AsOfDate = invoice.InvoiceDate?.Date ?? invoice.VisitDate.Date
            }, cancellationToken);

            if (!balanceB.IsUsable)
            {
                throw new BillingFinalizationValidationException(
                    $"Plafon benefit pegawai pengganti tidak dapat digunakan: {balanceB.Message}");
            }

            remainingAllowanceB = balanceB.RemainingAmount;
            personKindB = balanceB.BenefitPersonKind;
            personIdB = balanceB.BenefitPersonId;
            if (!string.IsNullOrEmpty(balanceB.RelationshipCode))
            {
                newRelationshipCode = balanceB.RelationshipCode;
            }
        }

        var newPayerAmount = Math.Min(coveredByRuleC, remainingAllowanceB);
        var newExcess = Math.Max(coveredByRuleC - newPayerAmount, 0m);
        var newChoice = oldDeferredChoice; // Pilihan utang terbawa dari tagihan
        var newEmployeeAmount = newExcess + newChoice;

        IDbContextTransaction? transaction = null;
        var sharedCorrelationId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        decimal returnedAllowanceAmount = 0m;
        decimal usedAllowanceAmount = 0m;
        BilHandoffAdjustment? adjustment = null;
        BilArHandoff? newHandoff = null;

        try
        {
            if (_dbContext.Database.IsRelational())
            {
                transaction = await _dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable, cancellationToken);
                await AcquireLockAsync($"BIL_BENEFIT_CORRECTION_{invoiceId:N}", cancellationToken);
            }

            // A. Pemulihan Plafon Pegawai A (KembalikanPlafon / ReturnAsync)
            if (payerHandoff is not null && _balanceService is not null)
            {
                var returnResult = await _balanceService.ReturnAsync(new BenefitBalanceReturnRequest
                {
                    SourceHandoffKey = payerHandoff.HandoffKey,
                    Reason = $"Berita Acara Koreksi No. {request.OfficialReportNumber}: Pengembalian plafon pegawai salah",
                    ActorUserId = actorUserId
                }, cancellationToken);

                if (returnResult.Result == BenefitReturnResults.Returned)
                {
                    returnedAllowanceAmount = returnResult.Amount;
                }
            }

            // B. Pemakaian Plafon Pegawai B (PakaiPlafon / UseAsync)
            Guid? newPayerHandoffKey = null;
            if (newPayerAmount > 0 && _balanceService is not null && personKindB is not null && personIdB.HasValue)
            {
                newPayerHandoffKey = Guid.NewGuid();
                var useResult = await _balanceService.UseAsync(new BenefitBalanceUseRequest
                {
                    SourceHandoffKey = newPayerHandoffKey.Value,
                    InvoiceId = invoice.Id,
                    BenefitPersonKind = personKindB,
                    BenefitPersonId = personIdB.Value,
                    Amount = newPayerAmount,
                    ActorUserId = actorUserId
                }, cancellationToken);

                if (useResult.Result == BenefitUseResults.Insufficient)
                {
                    throw new BillingFinalizationValidationException(
                        $"Sisa plafon pegawai pengganti tidak mencukupi untuk porsi RS Benefit: {useResult.Message}");
                }

                usedAllowanceAmount = newPayerAmount;

                // Perbarui HandoffKey dan nominal pada baris PAYER bila ada
                if (payerHandoff is not null)
                {
                    payerHandoff.HandoffKey = newPayerHandoffKey.Value;
                    payerHandoff.Amount = newPayerAmount;
                    payerHandoff.RowVersion = Guid.NewGuid();
                }
            }

            // C. Penerbitan Serah Terima Pembalik (BilHandoffAdjustment)
            if (oldBenefitHandoff is not null)
            {
                adjustment = new BilHandoffAdjustment
                {
                    ArHandoffId = oldBenefitHandoff.Id,
                    Direction = BillingAdjustmentDirections.Credit,
                    Amount = oldBenefitHandoff.Amount,
                    Reason = $"Berita Acara Koreksi No. {request.OfficialReportNumber}: {request.Reason}",
                    CorrelationId = sharedCorrelationId,
                    CausationId = oldBenefitHandoff.Id,
                    CreatedAt = now,
                    RowVersion = Guid.NewGuid(),
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = actorUserId
                };
                _dbContext.BilHandoffAdjustments.Add(adjustment);
            }

            // D. Penerbitan Serah Terima Pengganti (BilArHandoff) atas pegawai B
            var finalizationRecordId = oldBenefitHandoff?.FinalizationRecordId
                ?? payerHandoff?.FinalizationRecordId
                ?? Guid.Empty;

            newHandoff = new BilArHandoff
            {
                InvoiceId = invoice.Id,
                FinalizationRecordId = finalizationRecordId,
                DebtorType = BillingArDebtorTypes.EmployeeBenefit,
                DebtorReferenceId = null,
                BenefitOwnerId = newOwner.EmployeeId,
                BenefitOwnerWorkforceProfileId = newOwner.WorkforceProfileId,
                BenefitRelationship = newRelationshipCode,
                DeferredPatientAmount = newChoice,
                EmployeeArOrigin = oldBenefitHandoff?.EmployeeArOrigin,
                DeferredConsentGivenBy = request.ConsentGivenBy ?? oldBenefitHandoff?.DeferredConsentGivenBy,
                Amount = newEmployeeAmount,
                DueDate = oldBenefitHandoff?.DueDate ?? payerHandoff?.DueDate ?? invoice.InvoiceDate,
                Status = BillingHandoffStatuses.Created,
                HandoffKey = Guid.NewGuid(),
                CorrelationId = sharedCorrelationId,
                CausationId = adjustment?.Id ?? Guid.NewGuid(),
                CreatedAt = now,
                RowVersion = Guid.NewGuid(),
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };
            _dbContext.BilArHandoffs.Add(newHandoff);

            await _dbContext.SaveChangesAsync(cancellationToken);

            if (transaction is not null)
            {
                await transaction.CommitAsync(cancellationToken);
            }

            await AuditBenefitCorrectionAsync(
                invoiceId,
                oldBenefitHandoff?.BenefitOwnerWorkforceProfileId ?? oldBenefitHandoff?.BenefitOwnerId,
                newOwner.WorkforceProfileId,
                request.OfficialReportNumber,
                sharedCorrelationId,
                actorUserId);

            // E. Integrasi Intake Finance: sinkronisasi dan pengolahan otomatis jika service tersedia
            if (_intakeService is not null && adjustment is not null)
            {
                try
                {
                    await _intakeService.SyncNewFactsAsync(actorUserId, cancellationToken);
                    var intake = await _dbContext.FinBillingHandoffIntakes
                        .FirstOrDefaultAsync(x => x.HandoffType == FinBillingHandoffTypes.Adjustment
                            && x.SourceHandoffId == adjustment.Id
                            && x.Status == FinBillingHandoffIntakeStatuses.New
                            && !x.IsDelete, cancellationToken);

                    if (intake is not null)
                    {
                        await _intakeService.ProcessAsync(intake.Id, actorUserId, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    await _loggerService.WarningAsync(
                        LogCategory,
                        "BenefitCorrection.IntakeSync",
                        $"Sinkronisasi otomatis intake Finance setelah Berita Acara Koreksi gagal: {ex.Message}");
                }
            }

            return new CorrectBenefitDebtorResponse
            {
                Success = true,
                InvoiceId = invoice.Id,
                OldHandoffId = oldBenefitHandoff?.Id ?? Guid.Empty,
                AdjustmentId = adjustment?.Id,
                NewHandoffId = newHandoff.Id,
                CorrelationId = sharedCorrelationId,
                OldBenefitOwnerId = oldBenefitHandoff?.BenefitOwnerId,
                NewBenefitOwnerId = newOwner.EmployeeId ?? Guid.Empty,
                OldBenefitOwnerWorkforceProfileId = oldBenefitHandoff?.BenefitOwnerWorkforceProfileId,
                NewBenefitOwnerWorkforceProfileId = newOwner.WorkforceProfileId,
                NewBenefitOwnerName = newEmployeeName,
                NewRelationshipCode = newRelationshipCode,
                OldEmployeeAmount = oldEmployeeAmount,
                NewEmployeeAmount = newEmployeeAmount,
                ReturnedAllowanceAmount = returnedAllowanceAmount,
                UsedAllowanceAmount = usedAllowanceAmount,
                OfficialReportNumber = request.OfficialReportNumber,
                Message = $"Koreksi pemilik manfaat berhasil diproses dengan Berita Acara No. {request.OfficialReportNumber}."
            };
        }
        catch
        {
            if (transaction is not null)
            {
                await transaction.RollbackAsync(CancellationToken.None);
            }
            throw;
        }
        finally
        {
            if (transaction is not null)
            {
                await transaction.DisposeAsync();
            }
        }
    }

    private Task AuditBenefitCorrectionAsync(
        Guid invoiceId,
        Guid? oldEmployeeId,
        Guid newEmployeeId,
        string officialReportNumber,
        Guid correlationId,
        Guid actorUserId) =>
        _loggerService.AuditAsync(
            LogCategory,
            "BillingHandoff.BenefitCorrection",
            $"Berita Acara Koreksi salah orang ({officialReportNumber}): pemilik {oldEmployeeId} digantikan pemilik {newEmployeeId}.",
            new
            {
                InvoiceId = invoiceId,
                OldEmployeeId = oldEmployeeId,
                NewEmployeeId = newEmployeeId,
                OfficialReportNumber = officialReportNumber,
                CorrelationId = correlationId,
                ActorUserId = actorUserId
            });
}

