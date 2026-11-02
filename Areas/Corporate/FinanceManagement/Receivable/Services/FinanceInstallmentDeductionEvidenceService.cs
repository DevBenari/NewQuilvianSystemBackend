using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Data;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;

/// <summary>
/// Bukti potong gaji per angsuran: HRD menyimpan, HRD/Finance membaca dari sumber yang sama.
///
/// BATAS TEGAS: service ini TIDAK PERNAH mengubah FinReceivable, FinReceivableInstallment, maupun buku mutasi. Saldo piutang
/// hanya berkurang lewat FinanceReceivablePayrollSyncService.ApplyResultCoreAsync (hasil payroll yang sah atau batch Finance
/// yang disahkan). Mengunggah atau memverifikasi bukti hanyalah catatan pendukung — status TERVERIFIKASI bukan pengakuan bayar.
///
/// Berkas memakai FinanceTransactionProofService yang sama dengan batch hasil potongan Finance (jenis LAPORAN-PENGGAJIAN)
/// sehingga validasi tipe/ukuran/jalur, penyimpanan di luar akar publik, dan audit unduhan tidak diduplikasi. Bukti yang
/// sudah dilampirkan pada batch Finance ikut dibaca (diturunkan, tidak disalin).
/// </summary>
public sealed class FinanceInstallmentDeductionEvidenceService
{
    private const string LogCategory = "Corporate.FinanceManagement.Receivable.InstallmentDeductionEvidence";

    private static readonly HashSet<string> ResultStatuses = new(StringComparer.Ordinal)
    {
        FinPayrollDeductionItemResultStatuses.Berhasil,
        FinPayrollDeductionItemResultStatuses.Sebagian,
        FinPayrollDeductionItemResultStatuses.Gagal
    };

    private readonly ApplicationDbContext _dbContext;
    private readonly FinanceTransactionProofService _proofService;
    private readonly LoggerService _loggerService;

    public FinanceInstallmentDeductionEvidenceService(
        ApplicationDbContext dbContext, FinanceTransactionProofService proofService, LoggerService loggerService)
    {
        _dbContext = dbContext;
        _proofService = proofService;
        _loggerService = loggerService;
    }

    // ------------------------------------------------------------------------------------
    // Baca
    // ------------------------------------------------------------------------------------

    public async Task<InstallmentDeductionEvidenceOverviewResponse> GetOverviewAsync(Guid planId, CancellationToken cancellationToken)
    {
        var plan = await _dbContext.FinReceivableInstallmentPlans.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == planId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Perjanjian angsuran tidak ditemukan.");

        var receivableNumber = await _dbContext.FinReceivables.AsNoTracking()
            .Where(x => x.Id == plan.ReceivableId)
            .Select(x => x.ReceivableNumber)
            .FirstOrDefaultAsync(cancellationToken);

        var installments = await _dbContext.FinReceivableInstallments.AsNoTracking()
            .Where(x => !x.IsDelete && x.PlanId == planId)
            .OrderBy(x => x.InstallmentNumber)
            .ToListAsync(cancellationToken);
        var installmentIds = installments.Select(x => x.Id).ToList();

        var hrdRows = installmentIds.Count == 0
            ? new List<FinReceivableInstallmentDeductionEvidence>()
            : await _dbContext.FinReceivableInstallmentDeductionEvidences.AsNoTracking()
                .Where(x => !x.IsDelete && installmentIds.Contains(x.InstallmentId))
                .OrderBy(x => x.UploadedAt)
                .ToListAsync(cancellationToken);

        // Bukti yang dilampirkan pada batch hasil potongan Finance (diajukan atau disahkan) untuk angsuran ini.
        var batchRows = installmentIds.Count == 0
            ? new List<BatchEvidenceRow>()
            : await (
                from item in _dbContext.FinPayrollDeductionBatchItems.AsNoTracking()
                join batch in _dbContext.FinPayrollDeductionBatches.AsNoTracking() on item.BatchId equals batch.Id
                where !item.IsDelete && !batch.IsDelete
                    && item.InstallmentId != null && installmentIds.Contains(item.InstallmentId.Value)
                    && item.ValidationStatus == FinPayrollDeductionItemValidationStatuses.Valid
                    && batch.ProofId != null
                    && (batch.Status == FinPayrollDeductionBatchStatuses.Diajukan || batch.Status == FinPayrollDeductionBatchStatuses.Disahkan)
                select new BatchEvidenceRow(
                    item.InstallmentId.Value, batch.ProofId.Value, batch.PayrollReportNumber, batch.PayrollPeriodId,
                    item.ResultStatus, item.DeductedAmount, batch.Status, batch.SubmittedBy, batch.SubmittedAt,
                    batch.ApprovedBy, batch.ApprovedAt)
            ).ToListAsync(cancellationToken);

        var proofIds = hrdRows.Select(x => x.ProofId).Concat(batchRows.Select(x => x.ProofId)).Distinct().ToList();
        var proofs = proofIds.Count == 0
            ? new Dictionary<Guid, FinTransactionProof>()
            : await _dbContext.FinTransactionProofs.AsNoTracking()
                .Where(x => proofIds.Contains(x.Id) && !x.IsDelete)
                .ToDictionaryAsync(x => x.Id, cancellationToken);

        var names = await GetUserNamesAsync(
            hrdRows.SelectMany(x => new Guid?[] { x.UploadedBy, x.VerifiedBy })
                .Concat(batchRows.SelectMany(x => new Guid?[] { x.SubmittedBy, x.ApprovedBy })),
            cancellationToken);

        var currentPeriod = CurrentPeriod();
        var planActive = plan.Status is FinReceivableInstallmentPlanStatuses.Disetujui or FinReceivableInstallmentPlanStatuses.Selesai;

        var response = new InstallmentDeductionEvidenceOverviewResponse
        {
            PlanId = plan.Id,
            PlanNumber = plan.PlanNumber,
            PlanStatus = plan.Status,
            ReceivableId = plan.ReceivableId,
            ReceivableNumber = receivableNumber
        };

        foreach (var installment in installments)
        {
            var evidence = new List<InstallmentDeductionEvidenceResponse>();

            foreach (var row in hrdRows.Where(x => x.InstallmentId == installment.Id))
            {
                proofs.TryGetValue(row.ProofId, out var proof);
                evidence.Add(new InstallmentDeductionEvidenceResponse
                {
                    Id = row.Id,
                    Source = InstallmentDeductionEvidenceSources.Hrd,
                    ProofId = row.ProofId,
                    DocumentNumber = row.DocumentNumber,
                    FileName = proof?.OriginalFileName ?? string.Empty,
                    MediaType = proof?.MediaType ?? string.Empty,
                    SizeBytes = proof?.SizeBytes ?? 0,
                    ResultStatus = row.ResultStatus,
                    DeductedAmount = row.DeductedAmount,
                    DeductionDate = row.DeductionDate,
                    PayrollPeriodId = row.PayrollPeriodId,
                    PayrollRunId = row.PayrollRunId,
                    Notes = row.Notes,
                    Status = row.Status,
                    UploadedByName = names.GetValueOrDefault(row.UploadedBy),
                    UploadedAt = row.UploadedAt,
                    VerifiedByName = row.VerifiedBy.HasValue ? names.GetValueOrDefault(row.VerifiedBy.Value) : null,
                    VerifiedAt = row.VerifiedAt,
                    VerificationNotes = row.VerificationNotes,
                    RowVersion = row.RowVersion
                });
            }

            foreach (var row in batchRows.Where(x => x.InstallmentId == installment.Id))
            {
                proofs.TryGetValue(row.ProofId, out var proof);
                var approved = row.BatchStatus == FinPayrollDeductionBatchStatuses.Disahkan;
                evidence.Add(new InstallmentDeductionEvidenceResponse
                {
                    Id = null,
                    Source = InstallmentDeductionEvidenceSources.FinanceBatch,
                    ProofId = row.ProofId,
                    DocumentNumber = row.PayrollReportNumber ?? string.Empty,
                    FileName = proof?.OriginalFileName ?? string.Empty,
                    MediaType = proof?.MediaType ?? string.Empty,
                    SizeBytes = proof?.SizeBytes ?? 0,
                    ResultStatus = row.ResultStatus,
                    DeductedAmount = row.DeductedAmount,
                    PayrollPeriodId = row.PayrollPeriodId,
                    // Batch yang disahkan Finance = laporan penggajian telah diperiksa dan hasilnya diterapkan.
                    Status = approved ? FinInstallmentDeductionEvidenceStatuses.Terverifikasi : FinInstallmentDeductionEvidenceStatuses.Diunggah,
                    UploadedByName = row.SubmittedBy.HasValue ? names.GetValueOrDefault(row.SubmittedBy.Value) : null,
                    UploadedAt = row.SubmittedAt,
                    VerifiedByName = approved && row.ApprovedBy.HasValue ? names.GetValueOrDefault(row.ApprovedBy.Value) : null,
                    VerifiedAt = approved ? row.ApprovedAt : null
                });
            }

            var cancelled = installment.Status == FinReceivableInstallmentStatuses.Dibatalkan;
            response.Installments.Add(new InstallmentDeductionEvidenceInstallmentResponse
            {
                InstallmentId = installment.Id,
                InstallmentNumber = installment.InstallmentNumber,
                DeductionPeriod = installment.DeductionPeriod,
                InstallmentStatus = installment.Status,
                ScheduledAmount = installment.ScheduledAmount,
                CarriedOverAmount = installment.CarriedOverAmount,
                PaidAmount = installment.PaidAmount,
                OutstandingAmount = installment.OutstandingAmount,
                LastResultAt = installment.LastResultAt,
                PayrollResultStatus = DerivePayrollResult(installment),
                EvidenceStatus = DeriveEvidenceStatus(evidence),
                CanUploadEvidence = planActive && !cancelled
                    && string.CompareOrdinal(installment.DeductionPeriod, currentPeriod) <= 0,
                Evidence = evidence
            });
        }

        return response;
    }

    /// <summary>
    /// Membuka berkas bukti HANYA bila bukti itu memang terhubung ke perjanjian ini (penghubung HRD atau batch Finance).
    /// ProofId milik transaksi lain ditolak 404 — endpoint ini bukan jalan pintas membaca bukti sembarang.
    /// </summary>
    public async Task<(Stream Stream, string MediaType, string FileName)> OpenFileAsync(
        Guid planId, Guid proofId, Guid actorUserId, CancellationToken cancellationToken)
    {
        var linkedByHrd = await _dbContext.FinReceivableInstallmentDeductionEvidences.AsNoTracking()
            .AnyAsync(x => !x.IsDelete && x.PlanId == planId && x.ProofId == proofId, cancellationToken);

        var linked = linkedByHrd || await (
            from item in _dbContext.FinPayrollDeductionBatchItems.AsNoTracking()
            join batch in _dbContext.FinPayrollDeductionBatches.AsNoTracking() on item.BatchId equals batch.Id
            join installment in _dbContext.FinReceivableInstallments.AsNoTracking() on item.InstallmentId equals (Guid?)installment.Id
            where !item.IsDelete && !batch.IsDelete && installment.PlanId == planId && batch.ProofId == proofId
            select item.Id
        ).AnyAsync(cancellationToken);

        if (!linked) throw new KeyNotFoundException("Bukti tidak ditemukan pada perjanjian ini.");

        return await _proofService.DownloadAsync(proofId, actorUserId, cancellationToken);
    }

    // ------------------------------------------------------------------------------------
    // Simpan bukti (HRD)
    // ------------------------------------------------------------------------------------

    public async Task<InstallmentDeductionEvidenceOverviewResponse> UploadAsync(
        Guid planId, Guid installmentId, UploadInstallmentDeductionEvidenceRequest request, Guid actorUserId,
        CancellationToken cancellationToken)
    {
        if (actorUserId == Guid.Empty)
            throw new InstallmentPlanValidationException("Identitas pengguna yang mengunggah bukti tidak dapat ditentukan.");

        var documentNumber = request.DocumentNumber?.Trim() ?? string.Empty;
        if (documentNumber.Length == 0)
            throw new InstallmentPlanValidationException("Nomor laporan penggajian wajib diisi.");

        var resultStatus = request.ResultStatus?.Trim().ToUpperInvariant() ?? string.Empty;
        if (!ResultStatuses.Contains(resultStatus))
            throw new InstallmentPlanValidationException("Hasil potongan harus BERHASIL, SEBAGIAN, atau GAGAL.");

        var hasFile = request.File is { Length: > 0 };
        var hasExisting = request.ExistingProofId.HasValue && request.ExistingProofId.Value != Guid.Empty;
        if (hasFile == hasExisting)
            throw new InstallmentPlanValidationException("Lampirkan tepat satu: berkas bukti baru atau rujukan laporan penggajian yang sudah tersimpan.");

        if (request.DeductionDate.HasValue && request.DeductionDate.Value > DateTimeOffset.UtcNow.AddDays(1))
            throw new InstallmentPlanValidationException("Tanggal pemotongan tidak boleh di masa depan.");

        var notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        if (resultStatus == FinPayrollDeductionItemResultStatuses.Gagal && notes is null)
            throw new InstallmentPlanValidationException("Potongan yang gagal wajib disertai alasan yang tertera pada dokumen.");

        // Validasi terhadap data terkini SEBELUM berkas ditulis, supaya permintaan yang pasti ditolak tidak meninggalkan berkas.
        var (plan, installment, receivable) = await LoadScopeAsync(planId, installmentId, cancellationToken);
        EnsureCanAttach(plan, installment, request, resultStatus);

        if (hasExisting)
        {
            var existingProofId = request.ExistingProofId!.Value;
            var existing = await _dbContext.FinTransactionProofs.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == existingProofId && !x.IsDelete, cancellationToken);
            if (existing is null || existing.ProofType != FinTransactionProofTypes.LaporanPenggajian)
                throw new InstallmentPlanValidationException("Rujukan laporan penggajian tidak valid.");
        }

        Guid proofId;
        if (hasFile)
        {
            // Validasi isi/ukuran/jalur berkas dimiliki FinanceTransactionProofService; pengecualiannya dipetakan controller.
            var proof = await _proofService.UploadAsync(request.File, FinTransactionProofTypes.LaporanPenggajian, actorUserId, cancellationToken);
            proofId = proof.Id;
        }
        else
        {
            proofId = request.ExistingProofId!.Value;
        }

        var workforceProfileId = receivable.BenefitOwnerWorkforceProfileId;
        if (workforceProfileId is null && receivable.BenefitOwnerId.HasValue)
        {
            workforceProfileId = await _dbContext.MstEmployees.AsNoTracking()
                .Where(e => e.Id == receivable.BenefitOwnerId.Value)
                .Select(e => (Guid?)e.WorkforceProfileId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);
            await AcquireLockAsync($"FIN_INSTALLMENT_EVIDENCE_{installmentId:N}", cancellationToken);

            // Pemeriksaan ulang di bawah kunci: perjanjian bisa dibatalkan atau angsuran berubah sejak validasi awal.
            var (freshPlan, freshInstallment, _) = await LoadScopeAsync(planId, installmentId, cancellationToken);
            EnsureCanAttach(freshPlan, freshInstallment, request, resultStatus);

            var duplicate = await _dbContext.FinReceivableInstallmentDeductionEvidences.AsNoTracking()
                .AnyAsync(x => !x.IsDelete && x.InstallmentId == installmentId && x.DocumentNumber == documentNumber
                    && x.Status != FinInstallmentDeductionEvidenceStatuses.Ditolak, cancellationToken);
            if (duplicate)
                throw new InstallmentPlanConflictException(
                    $"Bukti dengan nomor laporan {documentNumber} sudah tersimpan untuk angsuran ini.");

            var now = DateTimeOffset.UtcNow;
            var evidence = new FinReceivableInstallmentDeductionEvidence
            {
                InstallmentId = installmentId,
                PlanId = planId,
                ReceivableId = receivable.Id,
                WorkforceProfileId = workforceProfileId,
                DeductionPeriod = installment.DeductionPeriod,
                PayrollPeriodId = request.PayrollPeriodId,
                PayrollRunId = request.PayrollRunId,
                DocumentNumber = documentNumber,
                ProofId = proofId,
                ResultStatus = resultStatus,
                DeductedAmount = resultStatus == FinPayrollDeductionItemResultStatuses.Gagal ? 0m : request.DeductedAmount,
                DeductionDate = request.DeductionDate,
                Notes = notes,
                Status = FinInstallmentDeductionEvidenceStatuses.Diunggah,
                UploadedBy = actorUserId,
                UploadedAt = now,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };
            _dbContext.FinReceivableInstallmentDeductionEvidences.Add(evidence);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception)
            {
                // Unique index terfilter (angsuran + nomor laporan aktif) menangkap unggah ganda yang lolos pemeriksaan di atas.
                throw new InstallmentPlanConflictException(
                    $"Bukti dengan nomor laporan {documentNumber} sudah tersimpan untuk angsuran ini.", exception);
            }

            await CommitAsync(transaction, cancellationToken);
            await AuditAsync("Evidence.Uploaded", planId, installmentId, evidence.Id, actorUserId);
        }
        catch
        {
            await RollbackAsync(transaction);
            throw;
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }

        return await GetOverviewAsync(planId, cancellationToken);
    }

    // ------------------------------------------------------------------------------------
    // Verifikasi / tolak bukti
    // ------------------------------------------------------------------------------------

    public Task<InstallmentDeductionEvidenceOverviewResponse> VerifyAsync(
        Guid planId, Guid evidenceId, VerifyInstallmentDeductionEvidenceRequest request, Guid actorUserId,
        CancellationToken cancellationToken) =>
        DecideAsync(planId, evidenceId, verify: true, request.RowVersion, request.Notes, actorUserId, cancellationToken);

    public Task<InstallmentDeductionEvidenceOverviewResponse> RejectAsync(
        Guid planId, Guid evidenceId, RejectInstallmentDeductionEvidenceRequest request, Guid actorUserId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new InstallmentPlanValidationException("Alasan penolakan bukti wajib diisi.");
        return DecideAsync(planId, evidenceId, verify: false, request.RowVersion, request.Reason, actorUserId, cancellationToken);
    }

    private async Task<InstallmentDeductionEvidenceOverviewResponse> DecideAsync(
        Guid planId, Guid evidenceId, bool verify, Guid? rowVersion, string? notes, Guid actorUserId,
        CancellationToken cancellationToken)
    {
        if (actorUserId == Guid.Empty)
            throw new InstallmentPlanValidationException("Identitas pengguna yang memverifikasi tidak dapat ditentukan.");

        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);
            await AcquireLockAsync($"FIN_INSTALLMENT_EVIDENCE_DECISION_{evidenceId:N}", cancellationToken);

            var evidence = await _dbContext.FinReceivableInstallmentDeductionEvidences
                .SingleOrDefaultAsync(x => x.Id == evidenceId && x.PlanId == planId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Bukti potong gaji tidak ditemukan pada perjanjian ini.");

            if (evidence.Status != FinInstallmentDeductionEvidenceStatuses.Diunggah)
                throw new InstallmentPlanConflictException(
                    $"Bukti berstatus {evidence.Status} sehingga tidak dapat diputuskan lagi. Hanya bukti BUKTI_DIUNGGAH yang dapat diverifikasi atau ditolak.");

            if (rowVersion.HasValue && rowVersion.Value != evidence.RowVersion)
                throw new InstallmentPlanConflictException(
                    "Bukti telah diperbarui oleh pengguna lain sejak Anda membukanya. Muat ulang sebelum memutuskan.");

            evidence.Status = verify ? FinInstallmentDeductionEvidenceStatuses.Terverifikasi : FinInstallmentDeductionEvidenceStatuses.Ditolak;
            evidence.VerifiedBy = actorUserId;
            evidence.VerifiedAt = DateTimeOffset.UtcNow;
            evidence.VerificationNotes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
            evidence.UpdateDateTime = DateTime.UtcNow;
            evidence.UpdateBy = actorUserId;
            evidence.RowVersion = Guid.NewGuid();

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException exception)
            {
                throw new InstallmentPlanConflictException(
                    "Bukti telah diperbarui oleh pengguna lain. Muat ulang sebelum mencoba lagi.", exception);
            }

            await CommitAsync(transaction, cancellationToken);
            await AuditAsync(verify ? "Evidence.Verified" : "Evidence.Rejected", planId, evidence.InstallmentId, evidence.Id, actorUserId);
        }
        catch
        {
            await RollbackAsync(transaction);
            throw;
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }

        return await GetOverviewAsync(planId, cancellationToken);
    }

    // ------------------------------------------------------------------------------------
    // Infrastruktur
    // ------------------------------------------------------------------------------------

    private async Task<(FinReceivableInstallmentPlan Plan, FinReceivableInstallment Installment, FinReceivable Receivable)> LoadScopeAsync(
        Guid planId, Guid installmentId, CancellationToken cancellationToken)
    {
        var plan = await _dbContext.FinReceivableInstallmentPlans.AsNoTracking().SingleOrDefaultAsync(x => x.Id == planId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Perjanjian angsuran tidak ditemukan.");

        var installment = await _dbContext.FinReceivableInstallments.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == installmentId && x.PlanId == planId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Angsuran tidak ditemukan pada perjanjian ini.");

        var receivable = await _dbContext.FinReceivables.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == plan.ReceivableId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Kartu piutang pemilik perjanjian ini tidak ditemukan.");

        return (plan, installment, receivable);
    }

    private static void EnsureCanAttach(
        FinReceivableInstallmentPlan plan, FinReceivableInstallment installment,
        UploadInstallmentDeductionEvidenceRequest request, string resultStatus)
    {
        if (plan.Status is not (FinReceivableInstallmentPlanStatuses.Disetujui or FinReceivableInstallmentPlanStatuses.Selesai))
            throw new InstallmentPlanConflictException(
                $"Perjanjian berstatus {plan.Status}; bukti potong gaji hanya untuk perjanjian yang disetujui.");

        if (installment.Status == FinReceivableInstallmentStatuses.Dibatalkan)
            throw new InstallmentPlanConflictException("Angsuran ini dibatalkan sehingga tidak dapat menerima bukti potong gaji.");

        if (string.CompareOrdinal(installment.DeductionPeriod, CurrentPeriod()) > 0)
            throw new InstallmentPlanValidationException(
                $"Periode potong gaji {installment.DeductionPeriod} belum berjalan; bukti belum dapat disimpan.");

        var due = installment.ScheduledAmount + installment.CarriedOverAmount;
        if (resultStatus == FinPayrollDeductionItemResultStatuses.Gagal)
        {
            if (request.DeductedAmount != 0m)
                throw new InstallmentPlanValidationException("Potongan yang gagal harus bernominal nol.");
        }
        else
        {
            if (request.DeductedAmount <= 0m)
                throw new InstallmentPlanValidationException("Nominal terpotong harus lebih dari nol untuk hasil BERHASIL atau SEBAGIAN.");
            if (request.DeductedAmount > due)
                throw new InstallmentPlanValidationException(
                    $"Nominal pada dokumen ({request.DeductedAmount:N2}) melebihi kewajiban angsuran periode ini ({due:N2}).");
        }
    }

    private static string DerivePayrollResult(FinReceivableInstallment installment) => installment.Status switch
    {
        FinReceivableInstallmentStatuses.Terbayar => InstallmentPayrollResultStates.Success,
        FinReceivableInstallmentStatuses.TerbayarSebagian => InstallmentPayrollResultStates.Partial,
        FinReceivableInstallmentStatuses.Tertunggak => InstallmentPayrollResultStates.Failed,
        FinReceivableInstallmentStatuses.Dibatalkan => InstallmentPayrollResultStates.Cancelled,
        _ => InstallmentPayrollResultStates.NoResult
    };

    /// <summary>Status terkuat dari bukti yang tidak ditolak: TERVERIFIKASI &gt; BUKTI_DIUNGGAH; hanya ditolak -&gt; DITOLAK.</summary>
    private static string DeriveEvidenceStatus(IReadOnlyCollection<InstallmentDeductionEvidenceResponse> evidence)
    {
        if (evidence.Any(x => x.Status == FinInstallmentDeductionEvidenceStatuses.Terverifikasi))
            return FinInstallmentDeductionEvidenceStatuses.Terverifikasi;
        if (evidence.Any(x => x.Status == FinInstallmentDeductionEvidenceStatuses.Diunggah))
            return FinInstallmentDeductionEvidenceStatuses.Diunggah;
        if (evidence.Count > 0) return FinInstallmentDeductionEvidenceStatuses.Ditolak;
        return FinInstallmentDeductionEvidenceStatuses.None;
    }

    private static string CurrentPeriod()
    {
        var today = FinanceBusinessDate.Today();
        return $"{today.Year:D4}-{today.Month:D2}";
    }

    private async Task<Dictionary<Guid, string?>> GetUserNamesAsync(IEnumerable<Guid?> userIds, CancellationToken cancellationToken)
    {
        var ids = userIds.Where(x => x.HasValue && x.Value != Guid.Empty).Select(x => x!.Value).Distinct().ToList();
        if (ids.Count == 0) return new Dictionary<Guid, string?>();

        return await _dbContext.Users.AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .Select(x => new { x.Id, Name = x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode })
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);
    }

    private async Task<IDbContextTransaction?> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        if (!_dbContext.Database.IsRelational()) return null;
        return await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
    }

    private Task AcquireLockAsync(string key, CancellationToken cancellationToken) =>
        _dbContext.Database.IsRelational()
            ? _dbContext.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(hashtext({0}));", [key], cancellationToken)
            : Task.CompletedTask;

    private static Task CommitAsync(IDbContextTransaction? transaction, CancellationToken cancellationToken) =>
        transaction is null ? Task.CompletedTask : transaction.CommitAsync(cancellationToken);

    private static Task RollbackAsync(IDbContextTransaction? transaction) =>
        transaction is null ? Task.CompletedTask : transaction.RollbackAsync(CancellationToken.None);

    private Task AuditAsync(string action, Guid planId, Guid installmentId, Guid evidenceId, Guid actorUserId) =>
        _loggerService.AuditAsync(LogCategory, $"InstallmentDeductionEvidence.{action}",
            $"Bukti potong gaji angsuran dicatat. PlanId={planId} InstallmentId={installmentId} EvidenceId={evidenceId}",
            new { PlanId = planId, InstallmentId = installmentId, EvidenceId = evidenceId, ActorUserId = actorUserId });

    private sealed record BatchEvidenceRow(
        Guid InstallmentId, Guid ProofId, string? PayrollReportNumber, Guid PayrollPeriodId, string ResultStatus,
        decimal DeductedAmount, string BatchStatus, Guid? SubmittedBy, DateTimeOffset? SubmittedAt,
        Guid? ApprovedBy, DateTimeOffset? ApprovedAt);
}
