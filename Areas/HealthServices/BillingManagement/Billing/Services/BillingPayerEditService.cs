using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Data;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services
{
    /// <summary>
    /// Orkestrator layar edit tagihan, perbandingan payer kandidat, dan eksekusi ganti penanggung kunjungan
    /// (BE-BKC-047, MPY-DES-016, BIL-API-1.0).
    /// Mengelola gerbang keamanan kelayakan edit, transaksi serializable, idempotensi perintah,
    /// reset otomatis penanggung per baris biaya yang tidak lagi sah, kalkulasi ulang, dan jejak audit tak terhapus.
    /// </summary>
    public sealed class BillingPayerEditService
    {
        private const string LogCategory = "HealthServices.BillingManagement.Billing";

        private readonly ApplicationDbContext _dbContext;
        private readonly BillingCalculationService _calculationService;
        private readonly EncounterPaymentSourceService _encounterPaymentSourceService;
        private readonly LoggerService _loggerService;

        public BillingPayerEditService(
            ApplicationDbContext dbContext,
            BillingCalculationService calculationService,
            EncounterPaymentSourceService encounterPaymentSourceService,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _calculationService = calculationService;
            _encounterPaymentSourceService = encounterPaymentSourceService;
            _loggerService = loggerService;
        }

        /// <summary>
        /// Memuat seluruh bahan layar Edit Tagihan dalam satu panggilan gabungan (GET /{id}/edit-context, MPY-DES-015, BIL-API-1.0).
        /// Murni membaca data (100% read-only, AsNoTracking), tidak membuat versi kalkulasi baru dan tidak mengubah state.
        /// </summary>
        public async Task<InvoiceEditContextResponse> GetEditContextAsync(
            Guid invoiceId,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var invoice = await _dbContext.BilInvoices.AsNoTracking()
                .Include(x => x.Items).ThenInclude(x => x.Category)
                .FirstOrDefaultAsync(x => x.Id == invoiceId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Invoice Billing tidak ditemukan.");

            var encounter = await _dbContext.RegPatientEncounters.AsNoTracking()
                .Include(x => x.PaymentSource).ThenInclude(x => x!.InsuranceProvider)
                .Include(x => x.PaymentSource).ThenInclude(x => x!.CompanyGuarantor)
                .Include(x => x.PaymentSource).ThenInclude(x => x!.PatientInsurance)
                .Include(x => x.PaymentSource).ThenInclude(x => x!.PatientCompanyGuarantor)
                .FirstOrDefaultAsync(x => x.Id == invoice.EncounterId && !x.IsDelete && !x.IsCancel, cancellationToken)
                ?? throw new KeyNotFoundException("Data kunjungan tidak ditemukan.");

            var patient = await _dbContext.MstPatients.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == encounter.PatientId && !x.IsDelete, cancellationToken);

            var calculation = await _calculationService.PreviewCalculationAsync(invoice.Id, actorUserId, cancellationToken);

            // 1. Current Payer
            var currentPaymentSource = encounter.PaymentSource
                ?? await _dbContext.RegPatientEncounterGuarantors.AsNoTracking()
                    .Include(x => x.InsuranceProvider)
                    .Include(x => x.CompanyGuarantor)
                    .Include(x => x.PatientInsurance)
                    .Include(x => x.PatientCompanyGuarantor)
                    .FirstOrDefaultAsync(x => x.EncounterId == encounter.Id && x.IsActive && !x.IsDelete, cancellationToken);

            var currentPayer = MapCurrentPayer(currentPaymentSource, encounter);

            // 2. Available Payer Options
            var availablePayerOptions = await BuildAvailablePayerOptionsAsync(encounter, cancellationToken);

            // 3. Active items and assignments
            var activeItems = invoice.Items
                .Where(x => x.Status == BillingInvoiceItemStatuses.Active && !x.IsDelete)
                .OrderBy(x => x.CreateDateTime)
                .ToList();

            var itemIds = activeItems.Select(x => x.Id).ToList();

            var existingAssignments = await _dbContext.BilInvoiceItemPayerAssignments.AsNoTracking()
                .Where(x => itemIds.Contains(x.InvoiceItemId) && x.IsActive && !x.IsDelete)
                .ToDictionaryAsync(x => x.InvoiceItemId, cancellationToken);

            var itemAssignments = activeItems.Select(item =>
            {
                if (existingAssignments.TryGetValue(item.Id, out var assignment))
                {
                    return new ItemPayerAssignmentResponse
                    {
                        InvoiceItemId = item.Id,
                        ItemName = item.DescriptionSnapshot,
                        PayerKind = assignment.PayerKind,
                        AssignmentSource = assignment.AssignmentSource,
                        Amount = item.Quantity * item.UnitPrice
                    };
                }
                return new ItemPayerAssignmentResponse
                {
                    InvoiceItemId = item.Id,
                    ItemName = item.DescriptionSnapshot,
                    PayerKind = currentPayer.PaymentType,
                    AssignmentSource = "AUTO",
                    Amount = item.Quantity * item.UnitPrice
                };
            }).ToList();

            // 4. Drug items and dispositions
            var drugItems = activeItems
                .Where(x => (x.Category != null && x.Category.IsPharmacy) || x.SourceDomain == "PHARMACY")
                .ToList();

            var drugItemIds = drugItems.Select(x => x.Id).ToList();

            var existingDispositions = await _dbContext.BilInvoiceItemBillingDispositions.AsNoTracking()
                .Where(x => drugItemIds.Contains(x.InvoiceItemId) && x.IsActive && !x.IsDelete)
                .ToDictionaryAsync(x => x.InvoiceItemId, cancellationToken);

            var drugDispositions = drugItems.Select(item =>
            {
                if (existingDispositions.TryGetValue(item.Id, out var disposition))
                {
                    return new DrugBillingDispositionItemResponse
                    {
                        InvoiceItemId = item.Id,
                        DrugName = item.DescriptionSnapshot,
                        Disposition = disposition.Disposition,
                        DecisionSource = disposition.DecisionSource,
                        Amount = item.Quantity * item.UnitPrice
                    };
                }
                return new DrugBillingDispositionItemResponse
                {
                    InvoiceItemId = item.Id,
                    DrugName = item.DescriptionSnapshot,
                    Disposition = "INCLUDED",
                    DecisionSource = "AUTO",
                    Amount = item.Quantity * item.UnitPrice
                };
            }).ToList();

            var isRanap = string.Equals(invoice.ServiceType, "RANAP", StringComparison.OrdinalIgnoreCase);
            var eligibleDrugItemIds = isRanap ? new List<Guid>() : drugItemIds;

            // 5. Capabilities and block reasons
            var isOpen = invoice.Status == BillingInvoiceStatuses.Open;
            var paidAmount = await _dbContext.BilPaymentAllocations.AsNoTracking()
                .Where(x => x.TargetType == BillingAllocationTargetTypes.Invoice && x.TargetId == invoice.Id && !x.IsDelete)
                .SumAsync(x => (decimal?)(x.ReversesAllocationId.HasValue ? -x.Amount : x.Amount), cancellationToken) ?? 0;
            var hasPaid = paidAmount > 0;

            var capabilities = new InvoiceEditCapabilitiesResponse
            {
                CanEditPaymentSource = isOpen && !hasPaid,
                PaymentSourceBlockReason = !isOpen
                    ? "Tagihan ini sudah difinalisasi sehingga tidak dapat diubah lagi."
                    : hasPaid
                        ? "Tagihan ini sudah menerima pembayaran. Perubahan penjamin atau penanggung memerlukan proses pembalikan, bukan pengeditan."
                        : null,
                CanEditItemPayer = isOpen && !hasPaid && activeItems.Count > 0,
                ItemPayerBlockReason = !isOpen
                    ? "Tagihan ini sudah difinalisasi sehingga tidak dapat diubah lagi."
                    : hasPaid
                        ? "Tagihan ini sudah menerima pembayaran. Perubahan penjamin atau penanggung memerlukan proses pembalikan, bukan pengeditan."
                        : activeItems.Count == 0
                            ? "Tagihan ini belum memiliki baris biaya."
                            : null,
                CanEditDrugBilling = isOpen && !hasPaid && !isRanap && eligibleDrugItemIds.Count > 0,
                DrugBillingBlockReason = !isOpen
                    ? "Tagihan ini sudah difinalisasi sehingga tidak dapat diubah lagi."
                    : hasPaid
                        ? "Tagihan ini sudah menerima pembayaran. Perubahan penjamin atau penanggung memerlukan proses pembalikan, bukan pengeditan."
                        : isRanap
                            ? "Penebusan obat tidak dapat diubah untuk kunjungan rawat inap."
                            : eligibleDrugItemIds.Count == 0
                                ? "Tagihan ini tidak memiliki item obat yang dapat diatur penebusannya."
                                : null
            };

            return new InvoiceEditContextResponse
            {
                Invoice = new InvoiceEditHeaderResponse
                {
                    Id = invoice.Id,
                    InvoiceNumber = invoice.InvoiceNumber,
                    Status = invoice.Status,
                    ServiceType = invoice.ServiceType,
                    RowVersion = invoice.RowVersion,
                    PatientId = encounter.PatientId,
                    PatientName = patient?.FullName ?? string.Empty,
                    MedicalRecordNumber = patient?.MedicalRecordNumber ?? string.Empty,
                    EncounterId = encounter.Id,
                    EncounterNumber = encounter.EncounterNumber,
                    EncounterDate = encounter.EncounterDate
                },
                Calculation = calculation,
                CurrentPayer = currentPayer,
                AvailablePayerOptions = availablePayerOptions,
                ItemPayerAssignments = itemAssignments,
                DrugBillingDisposition = drugDispositions,
                EligibleDrugInvoiceItemIds = eligibleDrugItemIds,
                Capabilities = capabilities
            };
        }

        /// <summary>
        /// Menghitung pratinjau perbandingan perhitungan antara payer yang sedang berlaku dengan payer kandidat
        /// (POST /{id}/payer-comparison-preview, MPY-DES-005, BIL-API-1.0).
        /// Murni membaca data (100% read-only, AsNoTracking), tidak membuat versi perhitungan baru,
        /// tidak menyentuh kunjungan, dan tidak meninggalkan jejak apa pun.
        /// </summary>
        public async Task<PayerComparisonPreviewResponse> PreviewPayerComparisonAsync(
            Guid invoiceId,
            PayerComparisonPreviewRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var invoice = await _dbContext.BilInvoices.AsNoTracking()
                .Include(x => x.Items).ThenInclude(x => x.Category)
                .FirstOrDefaultAsync(x => x.Id == invoiceId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Invoice Billing tidak ditemukan.");

            var encounter = await _dbContext.RegPatientEncounters.AsNoTracking()
                .Include(x => x.PaymentSource)
                .FirstOrDefaultAsync(x => x.Id == invoice.EncounterId && !x.IsDelete && !x.IsCancel, cancellationToken)
                ?? throw new KeyNotFoundException("Data kunjungan tidak ditemukan.");

            // 1. Current calculation
            var currentCalc = await _calculationService.PreviewCalculationAsync(invoice.Id, actorUserId, cancellationToken);

            var currentPaymentSource = encounter.PaymentSource
                ?? await _dbContext.RegPatientEncounterGuarantors.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.EncounterId == encounter.Id && x.IsActive && !x.IsDelete, cancellationToken);

            var currentPayer = MapCurrentPayer(currentPaymentSource, encounter);

            var currentSummary = new PayerComparisonSummaryResponse
            {
                PayerKind = currentPayer.PaymentType,
                PayerName = currentPayer.PaymentSourceName ?? currentPayer.PaymentType,
                TotalAmount = Money(currentCalc.PatientAmount + currentCalc.PrimaryAmount + currentCalc.ExcessAmount + currentCalc.UnresolvedCoverageAmount),
                PrimaryAmount = currentCalc.PrimaryAmount,
                PatientAmount = currentCalc.PatientAmount,
                ExcessAmount = currentCalc.ExcessAmount,
                NonBillableResidualAmount = currentCalc.Breakdown?.Coverage?.NonBillableResidualAmount ?? 0,
                DataAnomalyAmount = currentCalc.Breakdown?.Coverage?.DataAnomalyAmount ?? 0
            };

            // 2. Validate candidate request shape and business rules
            var validation = await ValidateCandidatePayerAsync(encounter, currentPaymentSource, request, cancellationToken);

            var warnings = new List<string>();
            var perItemComparison = new List<PayerComparisonItemResponse>();
            var candidateSummary = new PayerComparisonSummaryResponse();

            if (!validation.CanApply)
            {
                // Bila kandidat tidak layak, kembalikan pratinjau dengan penanda CanApply = false dan alasannya
                return new PayerComparisonPreviewResponse
                {
                    Current = currentSummary,
                    Candidate = candidateSummary,
                    PerItemComparison = perItemComparison,
                    Warnings = warnings,
                    CanApply = false,
                    BlockReason = validation.BlockReason
                };
            }

            // 3. Candidate calculation
            var candidateContext = new CandidatePayerContext(
                validation.CandidatePaymentType,
                request.CandidatePaymentMethodId,
                request.CandidatePatientInsuranceId,
                request.CandidatePatientCompanyGuarantorId);

            var candidateCalc = await _calculationService.PreviewCandidateCalculationAsync(
                invoice.Id, candidateContext, actorUserId, cancellationToken);

            candidateSummary = new PayerComparisonSummaryResponse
            {
                PayerKind = validation.CandidatePayerKind,
                PayerName = validation.CandidatePayerName,
                TotalAmount = Money(candidateCalc.PatientAmount + candidateCalc.PrimaryAmount + candidateCalc.ExcessAmount + candidateCalc.UnresolvedCoverageAmount),
                PrimaryAmount = candidateCalc.PrimaryAmount,
                PatientAmount = candidateCalc.PatientAmount,
                ExcessAmount = candidateCalc.ExcessAmount,
                NonBillableResidualAmount = candidateCalc.Breakdown?.Coverage?.NonBillableResidualAmount ?? 0,
                DataAnomalyAmount = candidateCalc.Breakdown?.Coverage?.DataAnomalyAmount ?? 0
            };

            // 4. Per item comparison
            var currentItemMap = currentCalc.Breakdown.Items.ToDictionary(x => x.InvoiceItemId);
            var candidateItemMap = candidateCalc.Breakdown.Items.ToDictionary(x => x.InvoiceItemId);
            var invoiceItems = invoice.Items.Where(x => x.Status == BillingInvoiceItemStatuses.Active && !x.IsDelete).ToList();

            foreach (var item in invoiceItems)
            {
                currentItemMap.TryGetValue(item.Id, out var curItem);
                candidateItemMap.TryGetValue(item.Id, out var candItem);

                decimal curCovered = curItem != null ? curItem.ItemPrimaryAmount + curItem.TaxPrimaryAmount : 0;
                decimal curPatient = curItem != null
                    ? (curItem.NetAmount - curItem.ItemPrimaryAmount - curItem.ItemUnresolvedAmount) + (curItem.TaxAmount - curItem.TaxPrimaryAmount - curItem.TaxUnresolvedAmount)
                    : item.Quantity * item.UnitPrice;

                decimal candCovered = candItem != null ? candItem.ItemPrimaryAmount + candItem.TaxPrimaryAmount : 0;
                decimal candPatient = candItem != null
                    ? (candItem.NetAmount - candItem.ItemPrimaryAmount - candItem.ItemUnresolvedAmount) + (candItem.TaxAmount - candItem.TaxPrimaryAmount - candItem.TaxUnresolvedAmount)
                    : item.Quantity * item.UnitPrice;

                perItemComparison.Add(new PayerComparisonItemResponse
                {
                    InvoiceItemId = item.Id,
                    ItemName = item.DescriptionSnapshot,
                    GrossAmount = item.Quantity * item.UnitPrice,
                    CurrentCovered = Money(curCovered),
                    CurrentPatient = Money(curPatient),
                    CandidateCovered = Money(candCovered),
                    CandidatePatient = Money(candPatient),
                    Difference = Money(curPatient - candPatient)
                });
            }

            if (candidateCalc.Breakdown?.Coverage?.AnomalyMessages != null)
            {
                foreach (var msg in candidateCalc.Breakdown.Coverage.AnomalyMessages)
                {
                    if (!string.IsNullOrWhiteSpace(msg) && !warnings.Contains(msg))
                    {
                        warnings.Add(msg);
                    }
                }
            }

            return new PayerComparisonPreviewResponse
            {
                Current = currentSummary,
                Candidate = candidateSummary,
                PerItemComparison = perItemComparison,
                Warnings = warnings,
                CanApply = true,
                BlockReason = null
            };
        }

        /// <summary>
        /// Mengganti sumber pembayaran kunjungan yang berlaku secara atomik di dalam satu transaksi serializable
        /// (PUT /{id}/payment-source, MPY-DES-001, MPY-DES-003, MPY-DES-009, MPY-DES-016, BIL-API-1.0).
        /// Menjaga seluruh gerbang validasi BIL-VAL-059 sampai BIL-VAL-074, mereset penanggung per baris biaya yang tidak
        /// lagi sah, menghitung ulang tagihan, dan mencatat jejak audit tak terhapus.
        /// </summary>
        public async Task<InvoiceEditResultResponse> SwitchPaymentSourceAsync(
            Guid invoiceId,
            SwitchPaymentSourceRequest request,
            Guid actorUserId,
            string? idempotencyKey,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            // BIL-VAL-063: Idempotency-Key tidak dikirim
            if (string.IsNullOrWhiteSpace(idempotencyKey))
            {
                throw new BillingPayerEditBadRequestException("Permintaan tidak lengkap. Muat ulang halaman lalu coba lagi.");
            }

            // BIL-VAL-062: Alasan kosong
            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                throw new BillingPayerEditBadRequestException("Alasan perubahan wajib diisi.");
            }

            // BIL-VAL-061: Versi baris tagihan yang dikirim berbeda dari yang tersimpan
            if (!Guid.TryParse(request.ExpectedRowVersion, out var expectedRowVersionGuid))
            {
                throw new BillingPayerEditConflictException("Data tagihan telah berubah sejak layar ini dibuka. Muat ulang data sebelum menyimpan kembali.");
            }

            IDbContextTransaction? transaction = null;
            try
            {
                if (_dbContext.Database.IsRelational())
                {
                    transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
                    await AcquireLockAsync($"BIL_CALCULATION_{invoiceId:N}", cancellationToken);
                }

                // Cek Idempotency Replay
                var priorCommand = await _dbContext.BilInvoicePayerChangeCommands.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey, cancellationToken);

                if (priorCommand is not null)
                {
                    if (priorCommand.InvoiceId != invoiceId ||
                        !string.Equals(priorCommand.NewPayerKind, request.PaymentType, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new BillingPayerEditConflictException("Permintaan yang sama memiliki isi berbeda; gunakan permintaan baru.");
                    }

                    var replayInvoice = await _dbContext.BilInvoices.AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == invoiceId && !x.IsDelete, cancellationToken);
                    var replayCalculation = await _calculationService.PreviewCalculationAsync(invoiceId, actorUserId, cancellationToken);

                    if (transaction is not null) await transaction.CommitAsync(cancellationToken);

                    return new InvoiceEditResultResponse
                    {
                        Calculation = replayCalculation,
                        RowVersion = replayInvoice?.RowVersion ?? Guid.Empty,
                        ResetAssignmentCount = priorCommand.ResetAssignmentCount,
                        Warnings = ["Permintaan ini sudah pernah diproses sebelumnya (idempotent replay)."]
                    };
                }

                // Validasi Tagihan
                var invoice = await _dbContext.BilInvoices
                    .FirstOrDefaultAsync(x => x.Id == invoiceId && !x.IsDelete, cancellationToken)
                    ?? throw new KeyNotFoundException("Invoice Billing tidak ditemukan.");

                if (_dbContext.Database.IsRelational())
                {
                    await AcquireLockAsync($"BIL_ENCOUNTER_{invoice.EncounterId:N}", cancellationToken);
                }

                // BIL-VAL-059: Tagihan tidak berstatus OPEN
                if (invoice.Status != BillingInvoiceStatuses.Open)
                {
                    throw new BillingPayerEditConflictException("Tagihan ini sudah difinalisasi sehingga tidak dapat diubah lagi.");
                }

                // BIL-VAL-060: Sudah ada pembayaran berhasil pada tagihan ini
                var paidAmount = await _dbContext.BilPaymentAllocations.AsNoTracking()
                    .Where(x => x.TargetType == BillingAllocationTargetTypes.Invoice && x.TargetId == invoice.Id && !x.IsDelete)
                    .SumAsync(x => (decimal?)(x.ReversesAllocationId.HasValue ? -x.Amount : x.Amount), cancellationToken) ?? 0;

                if (paidAmount > 0)
                {
                    throw new BillingPayerEditConflictException("Tagihan ini sudah menerima pembayaran. Perubahan penjamin atau penanggung memerlukan proses pembalikan, bukan pengeditan.");
                }

                // BIL-VAL-061: Versi baris tagihan yang dikirim berbeda dari yang tersimpan
                if (invoice.RowVersion != expectedRowVersionGuid)
                {
                    throw new BillingPayerEditConflictException("Data tagihan telah berubah sejak layar ini dibuka. Muat ulang data sebelum menyimpan kembali.");
                }

                var encounter = await _dbContext.RegPatientEncounters
                    .FirstOrDefaultAsync(x => x.Id == invoice.EncounterId && !x.IsDelete && !x.IsCancel, cancellationToken)
                    ?? throw new KeyNotFoundException("Data kunjungan tidak ditemukan.");

                // Ambil id versi kalkulasi sebelumnya untuk snapshot audit
                var previousCalcVersion = await _dbContext.BilCalculationVersions.AsNoTracking()
                    .Where(x => x.InvoiceId == invoiceId && !x.IsDelete)
                    .OrderByDescending(x => x.VersionNo)
                    .FirstOrDefaultAsync(cancellationToken);
                var previousCalcVersionId = previousCalcVersion?.Id;

                // BIL-VAL-064: paymentType bernilai di luar CASH/INSURANCE/COMPANY_GUARANTOR
                EncounterPaymentType targetPaymentType;
                if (string.Equals(request.PaymentType, "CASH", StringComparison.OrdinalIgnoreCase))
                    targetPaymentType = EncounterPaymentType.Cash;
                else if (string.Equals(request.PaymentType, "INSURANCE", StringComparison.OrdinalIgnoreCase))
                    targetPaymentType = EncounterPaymentType.Insurance;
                else if (string.Equals(request.PaymentType, "COMPANY_GUARANTOR", StringComparison.OrdinalIgnoreCase))
                    targetPaymentType = EncounterPaymentType.CompanyGuarantor;
                else
                    throw new BillingPayerEditBadRequestException("Jenis pembayaran tidak dikenali.");

                // Panggil EncounterPaymentSourceService di dalam transaksi yang sama (MPY-ENC-PAYER-001)
                var switchCommand = new EncounterPaymentSourceSwitchCommand(
                    EncounterId: invoice.EncounterId,
                    PaymentType: targetPaymentType,
                    PaymentMethodId: request.PaymentMethodId,
                    PatientInsuranceId: request.PatientInsuranceId,
                    PatientCompanyGuarantorId: request.PatientCompanyGuarantorId,
                    ServiceDate: encounter.EncounterDate,
                    Reason: request.Reason,
                    ActorUserId: actorUserId);

                var switchResult = await _encounterPaymentSourceService.SwitchPaymentSourceAsync(switchCommand, cancellationToken);
                if (!switchResult.Success)
                {
                    switch (switchResult.StatusCode)
                    {
                        case 400: throw new BillingPayerEditBadRequestException(switchResult.ErrorMessage!);
                        case 404: throw new KeyNotFoundException(switchResult.ErrorMessage!);
                        case 409: throw new BillingPayerEditConflictException(switchResult.ErrorMessage!);
                        case 422:
                        default:
                            throw new BillingPayerEditValidationException(switchResult.ErrorMessage!);
                    }
                }

                // Reset penanggung per baris biaya yang tidak lagi sah (MPY-DES-009, FR-BKC-072)
                var activeItemAssignments = await (
                    from a in _dbContext.BilInvoiceItemPayerAssignments
                    join item in _dbContext.BilInvoiceItems on a.InvoiceItemId equals item.Id
                    where item.InvoiceId == invoiceId && a.IsActive && !a.IsDelete && !item.IsDelete
                    select a)
                    .ToListAsync(cancellationToken);

                int resetCount = 0;
                var warnings = new List<string>();

                foreach (var assignment in activeItemAssignments)
                {
                    bool isObsolete = false;
                    if (targetPaymentType == EncounterPaymentType.Cash)
                    {
                        if (assignment.PayerKind != "CASH") isObsolete = true;
                    }
                    else if (targetPaymentType == EncounterPaymentType.Insurance)
                    {
                        if (assignment.PayerKind == "COMPANY_GUARANTOR") isObsolete = true;
                    }
                    else if (targetPaymentType == EncounterPaymentType.CompanyGuarantor)
                    {
                        if (assignment.PayerKind == "INSURANCE") isObsolete = true;
                    }

                    if (isObsolete)
                    {
                        assignment.IsActive = false;
                        assignment.UpdateBy = actorUserId;
                        assignment.UpdateDateTime = DateTime.UtcNow;

                        var resetAssignment = new BilInvoiceItemPayerAssignment
                        {
                            InvoiceItemId = assignment.InvoiceItemId,
                            PayerKind = "CASH",
                            AssignmentSource = "AUTO",
                            Reason = $"Direset otomatis menjadi Pribadi karena penanggung kunjungan diganti menjadi {switchResult.NewPayerName ?? request.PaymentType}.",
                            IsActive = true,
                            CreateBy = actorUserId,
                            CreateDateTime = DateTime.UtcNow
                        };
                        _dbContext.BilInvoiceItemPayerAssignments.Add(resetAssignment);
                        resetCount++;
                    }
                }

                if (resetCount > 0)
                {
                    warnings.Add($"{resetCount} baris biaya dikembalikan menjadi tanggungan pasien karena penanggung sebelumnya tidak lagi berlaku.");
                }

                // Simpan perubahan encounter guarantor dan reset assignment sebelum memicu recalculate
                await _dbContext.SaveChangesAsync(cancellationToken);

                // Hitung ulang tagihan secara atomik
                var recalcResult = await _calculationService.RecalculateAsync(
                    invoice.Id,
                    new RecalculateInvoiceRequest
                    {
                        ExpectedRowVersion = invoice.RowVersion,
                        Reason = $"Perhitungan ulang otomatis akibat ganti payer ke {switchResult.NewPayerName ?? request.PaymentType}. Alasan: {request.Reason}"
                    },
                    actorUserId,
                    cancellationToken);

                // Catat jejak perintah ganti payer pada BilInvoicePayerChangeCommand (MPY-DES-003, FR-BKC-073)
                var changeCommand = new BilInvoicePayerChangeCommand
                {
                    InvoiceId = invoice.Id,
                    EncounterId = invoice.EncounterId,
                    PreviousPayerKind = switchResult.OldPaymentType ?? "CASH",
                    NewPayerKind = switchResult.NewPaymentType ?? request.PaymentType,
                    PreviousPayerNameSnapshot = switchResult.OldPayerName,
                    NewPayerNameSnapshot = switchResult.NewPayerName,
                    PreviousCalculationVersionId = previousCalcVersionId,
                    NewCalculationVersionId = recalcResult.Id,
                    ResetAssignmentCount = resetCount,
                    Reason = request.Reason,
                    IdempotencyKey = idempotencyKey,
                    CorrelationId = request.CorrelationId,
                    CausationId = request.CausationId,
                    CreateBy = actorUserId,
                    CreateDateTime = DateTime.UtcNow
                };
                _dbContext.BilInvoicePayerChangeCommands.Add(changeCommand);
                await _dbContext.SaveChangesAsync(cancellationToken);

                // Commit transaksi keseluruhan
                if (transaction is not null)
                {
                    await transaction.CommitAsync(cancellationToken);
                }

                await _loggerService.AuditAsync(LogCategory, "BillingInvoice.SwitchPaymentSource", "Penanggung kunjungan berhasil diganti dan tagihan dihitung ulang.", new
                {
                    InvoiceId = invoice.Id,
                    EncounterId = invoice.EncounterId,
                    PreviousPayer = switchResult.OldPayerName,
                    NewPayer = switchResult.NewPayerName,
                    ResetCount = resetCount,
                    NewCalculationVersion = recalcResult.VersionNo,
                    recalcResult.PatientAmount,
                    recalcResult.PrimaryAmount,
                    IdempotencyKey = idempotencyKey
                });

                return new InvoiceEditResultResponse
                {
                    Calculation = recalcResult,
                    RowVersion = recalcResult.InvoiceRowVersion,
                    ResetAssignmentCount = resetCount,
                    Warnings = warnings
                };
            }
            catch
            {
                if (transaction is not null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                }
                throw;
            }
        }

        /// <summary>
        /// Mengubah penanggung beberapa baris biaya sekaligus dan menghitung ulang tagihan secara atomik
        /// (BE-BKC-048, MPY-DES-008, MPY-DEC-004, BIL-API-1.0).
        /// Menerapkan matriks validasi BIL-VAL-059 s/d BIL-VAL-063, BIL-VAL-075 s/d BIL-VAL-080.
        /// Penulisan penanggung dilakukan secara nonaktifkan-lalu-sisipkan (append-only).
        /// </summary>
        public async Task<InvoiceEditResultResponse> UpdateItemPayerAssignmentsAsync(
            Guid invoiceId,
            UpdateItemPayerAssignmentsRequest request,
            Guid actorUserId,
            string? idempotencyKey,
            CancellationToken cancellationToken = default)
        {
            // BIL-VAL-063: Header Idempotency-Key tidak disertakan pada perintah PUT
            if (string.IsNullOrWhiteSpace(idempotencyKey))
            {
                throw new BillingPayerEditBadRequestException("Header Idempotency-Key wajib disertakan pada permintaan ini.");
            }

            // BIL-VAL-062: Alasan kosong pada permintaan yang mewajibkan
            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                throw new BillingPayerEditBadRequestException("Alasan perubahan wajib diisi.");
            }

            if (string.IsNullOrWhiteSpace(request.ExpectedRowVersion) || !Guid.TryParse(request.ExpectedRowVersion, out var expectedRowVersionGuid))
            {
                throw new BillingPayerEditBadRequestException("ExpectedRowVersion tidak valid.");
            }

            // BIL-VAL-079: Daftar penanggung yang dikirim kosong
            if (request.Assignments == null || request.Assignments.Count == 0)
            {
                throw new BillingPayerEditBadRequestException("Tidak ada perubahan penanggung yang dikirim.");
            }

            // BIL-VAL-080: Satu baris biaya muncul lebih dari sekali pada permintaan yang sama
            var duplicateGroup = request.Assignments.GroupBy(x => x.InvoiceItemId).FirstOrDefault(g => g.Count() > 1);
            if (duplicateGroup != null)
            {
                throw new BillingPayerEditBadRequestException("Ada baris biaya yang dikirim lebih dari satu kali.");
            }

            // Validasi jenis penanggung pada tiap item yang dikirim
            foreach (var a in request.Assignments)
            {
                var kind = a.PayerKind?.Trim().ToUpperInvariant();
                if (kind != "CASH" && kind != "INSURANCE" && kind != "COMPANY_GUARANTOR")
                {
                    throw new BillingPayerEditBadRequestException("Jenis pembayaran tidak dikenali.");
                }
            }

            IDbContextTransaction? transaction = null;
            try
            {
                if (_dbContext.Database.IsRelational() && _dbContext.Database.CurrentTransaction is null)
                {
                    transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
                }

                if (_dbContext.Database.IsRelational())
                {
                    await AcquireLockAsync($"BIL_CALCULATION_{invoiceId:N}", cancellationToken);
                }

                // Idempotency: periksa apakah permintaan dengan Idempotency-Key yang sama sudah pernah dieksekusi
                var priorCommand = await _dbContext.BilInvoicePayerChangeCommands.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey, cancellationToken);
                if (priorCommand != null)
                {
                    if (priorCommand.InvoiceId != invoiceId)
                    {
                        throw new BillingPayerEditConflictException("Permintaan yang sama memiliki isi berbeda; gunakan permintaan baru.");
                    }

                    var replayInvoice = await _dbContext.BilInvoices.AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == invoiceId && !x.IsDelete, cancellationToken);
                    var replayCalculation = await _calculationService.PreviewCalculationAsync(invoiceId, actorUserId, cancellationToken);

                    if (transaction is not null) await transaction.CommitAsync(cancellationToken);

                    return new InvoiceEditResultResponse
                    {
                        Calculation = replayCalculation,
                        RowVersion = replayInvoice?.RowVersion ?? Guid.Empty,
                        ResetAssignmentCount = 0,
                        Warnings = ["Permintaan ini sudah pernah diproses sebelumnya (idempotent replay)."]
                    };
                }

                // Validasi Tagihan
                var invoice = await _dbContext.BilInvoices
                    .Include(x => x.Items)
                    .FirstOrDefaultAsync(x => x.Id == invoiceId && !x.IsDelete, cancellationToken)
                    ?? throw new KeyNotFoundException("Invoice Billing tidak ditemukan.");

                if (_dbContext.Database.IsRelational())
                {
                    await AcquireLockAsync($"BIL_ENCOUNTER_{invoice.EncounterId:N}", cancellationToken);
                }

                // BIL-VAL-059: Tagihan tidak berstatus OPEN
                if (invoice.Status != BillingInvoiceStatuses.Open)
                {
                    throw new BillingPayerEditConflictException("Tagihan ini sudah difinalisasi sehingga tidak dapat diubah lagi.");
                }

                // BIL-VAL-060: Sudah ada pembayaran berhasil pada tagihan ini
                var paidAmount = await _dbContext.BilPaymentAllocations.AsNoTracking()
                    .Where(x => x.TargetType == BillingAllocationTargetTypes.Invoice && x.TargetId == invoice.Id && !x.IsDelete)
                    .SumAsync(x => (decimal?)(x.ReversesAllocationId.HasValue ? -x.Amount : x.Amount), cancellationToken) ?? 0;

                if (paidAmount > 0)
                {
                    throw new BillingPayerEditConflictException("Tagihan ini sudah menerima pembayaran. Perubahan penjamin atau penanggung memerlukan proses pembalikan, bukan pengeditan.");
                }

                // BIL-VAL-061: Versi baris tagihan yang dikirim berbeda dari yang tersimpan
                if (invoice.RowVersion != expectedRowVersionGuid)
                {
                    throw new BillingPayerEditConflictException("Data tagihan telah berubah sejak layar ini dibuka. Muat ulang data sebelum menyimpan kembali.");
                }

                // Validasi baris biaya terhadap tagihan
                var invoiceItemsById = invoice.Items
                    .Where(x => !x.IsDelete)
                    .ToDictionary(x => x.Id);

                foreach (var a in request.Assignments)
                {
                    // BIL-VAL-075: Baris biaya yang dikirim bukan milik tagihan ini
                    if (!invoiceItemsById.TryGetValue(a.InvoiceItemId, out var item))
                    {
                        throw new BillingPayerEditValidationException("Ada baris biaya yang tidak terdaftar pada tagihan ini.");
                    }

                    // BIL-VAL-076: Baris biaya berstatus dibatalkan
                    if (item.Status == BillingInvoiceItemStatuses.Voided)
                    {
                        throw new BillingPayerEditValidationException("Baris biaya yang sudah dibatalkan tidak dapat diubah penanggungnya.");
                    }
                }

                // Validasi penanggung item terhadap jenis penjamin kunjungan yang aktif
                var encounter = await _dbContext.RegPatientEncounters.AsNoTracking()
                    .Include(x => x.PaymentSource)
                    .FirstOrDefaultAsync(x => x.Id == invoice.EncounterId && !x.IsDelete && !x.IsCancel, cancellationToken)
                    ?? throw new KeyNotFoundException("Data kunjungan tidak ditemukan.");

                var currentPaymentSource = encounter.PaymentSource
                    ?? await _dbContext.RegPatientEncounterGuarantors.AsNoTracking()
                        .FirstOrDefaultAsync(x => x.EncounterId == encounter.Id && x.IsActive && !x.IsDelete, cancellationToken);

                var effectivePaymentType = currentPaymentSource?.PaymentType ?? encounter.PaymentType;

                bool hasInsurance = request.Assignments.Any(x => string.Equals(x.PayerKind, "INSURANCE", StringComparison.OrdinalIgnoreCase));
                bool hasCompany = request.Assignments.Any(x => string.Equals(x.PayerKind, "COMPANY_GUARANTOR", StringComparison.OrdinalIgnoreCase));

                // BIL-VAL-077: Penanggung INSURANCE dipilih padahal kunjungan tidak berpayer asuransi
                if (hasInsurance && effectivePaymentType != EncounterPaymentType.Insurance)
                {
                    throw new BillingPayerEditValidationException("Kunjungan ini tidak memakai asuransi, sehingga baris biaya tidak dapat ditanggung asuransi.");
                }

                // BIL-VAL-078: Penanggung COMPANY_GUARANTOR dipilih padahal kunjungan tidak berpenjamin perusahaan
                if (hasCompany && effectivePaymentType != EncounterPaymentType.CompanyGuarantor)
                {
                    throw new BillingPayerEditValidationException("Kunjungan ini tidak memakai penjamin perusahaan, sehingga baris biaya tidak dapat ditanggung penjamin.");
                }

                // Catat versi kalkulasi sebelumnya untuk snapshot audit
                var previousCalcVersion = await _dbContext.BilCalculationVersions.AsNoTracking()
                    .Where(x => x.InvoiceId == invoiceId && !x.IsDelete)
                    .OrderByDescending(x => x.VersionNo)
                    .FirstOrDefaultAsync(cancellationToken);
                var previousCalcVersionId = previousCalcVersion?.Id;

                // Penulisan penanggung baris: nonaktifkan-lalu-sisipkan (append-only mutation)
                var targetItemIds = request.Assignments.Select(x => x.InvoiceItemId).ToList();
                var existingActiveAssignments = await _dbContext.BilInvoiceItemPayerAssignments
                    .Where(x => targetItemIds.Contains(x.InvoiceItemId) && x.IsActive && !x.IsDelete)
                    .ToListAsync(cancellationToken);

                var now = DateTime.UtcNow;
                if (existingActiveAssignments.Count > 0)
                {
                    foreach (var existing in existingActiveAssignments)
                    {
                        existing.IsActive = false;
                        existing.UpdateBy = actorUserId;
                        existing.UpdateDateTime = now;
                    }
                    // Simpan deaktifasi terlebih dahulu untuk memastikan integritas filtered unique index
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }

                foreach (var a in request.Assignments)
                {
                    var normalizedKind = a.PayerKind.Trim().ToUpperInvariant();
                    var newAssignment = new BilInvoiceItemPayerAssignment
                    {
                        InvoiceItemId = a.InvoiceItemId,
                        EncounterGuarantorId = (normalizedKind == "CASH") ? null : currentPaymentSource?.Id,
                        PayerKind = normalizedKind,
                        AssignmentSource = "MANUAL",
                        Reason = request.Reason,
                        IsActive = true,
                        CreateBy = actorUserId,
                        CreateDateTime = now
                    };
                    _dbContext.BilInvoiceItemPayerAssignments.Add(newAssignment);
                }
                await _dbContext.SaveChangesAsync(cancellationToken);

                // Hitung ulang tagihan secara atomik
                var recalcResult = await _calculationService.RecalculateAsync(
                    invoice.Id,
                    new RecalculateInvoiceRequest
                    {
                        ExpectedRowVersion = invoice.RowVersion,
                        Reason = $"Perhitungan ulang otomatis akibat perubahan penanggung baris biaya. Alasan: {request.Reason}"
                    },
                    actorUserId,
                    cancellationToken);

                // Catat jejak perintah pada BilInvoicePayerChangeCommand (MPY-DES-003, FR-BKC-074)
                var changeCommand = new BilInvoicePayerChangeCommand
                {
                    InvoiceId = invoice.Id,
                    EncounterId = invoice.EncounterId,
                    PreviousPayerKind = "ITEM_ASSIGNMENT",
                    NewPayerKind = "ITEM_ASSIGNMENT",
                    PreviousPayerNameSnapshot = $"Update {request.Assignments.Count} item assignments",
                    NewPayerNameSnapshot = $"Update {request.Assignments.Count} item assignments",
                    PreviousCalculationVersionId = previousCalcVersionId,
                    NewCalculationVersionId = recalcResult.Id,
                    ResetAssignmentCount = 0,
                    Reason = request.Reason,
                    IdempotencyKey = idempotencyKey,
                    CorrelationId = request.CorrelationId,
                    CausationId = request.CausationId,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                };
                _dbContext.BilInvoicePayerChangeCommands.Add(changeCommand);
                await _dbContext.SaveChangesAsync(cancellationToken);

                if (transaction is not null)
                {
                    await transaction.CommitAsync(cancellationToken);
                }

                await _loggerService.AuditAsync(LogCategory, "BillingInvoice.UpdateItemPayerAssignments", "Penanggung baris biaya berhasil diperbarui dan tagihan dihitung ulang.", new
                {
                    InvoiceId = invoice.Id,
                    EncounterId = invoice.EncounterId,
                    UpdatedItemCount = request.Assignments.Count,
                    NewCalculationVersion = recalcResult.VersionNo,
                    recalcResult.PatientAmount,
                    recalcResult.PrimaryAmount,
                    IdempotencyKey = idempotencyKey
                });

                return new InvoiceEditResultResponse
                {
                    Calculation = recalcResult,
                    RowVersion = recalcResult.InvoiceRowVersion,
                    ResetAssignmentCount = 0,
                    Warnings = []
                };
            }
            catch
            {
                if (transaction is not null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                }
                throw;
            }
        }

        /// <summary>
        /// Mengatur disposisi penebusan obat tagihan (ALL_REDEEMED, PARTIAL_REDEEMED, NOT_REDEEMED)
        /// dan menghitung ulang tagihan secara atomik (BE-BKC-049, MPY-DES-010, MPY-DEC-009, BIL-API-1.0).
        /// Menerapkan matriks validasi BIL-VAL-059 s/d BIL-VAL-063, BIL-VAL-081 s/d BIL-VAL-086.
        /// Penulisan disposisi dilakukan secara nonaktifkan-lalu-sisipkan (append-only) ke BilInvoiceItemBillingDisposition.
        /// Data penyerahan obat milik Farmasi (PhmDrugUsage) sama sekali tidak disentuh (MPY-DEC-009, BIL-AT-096).
        /// </summary>
        public async Task<InvoiceEditResultResponse> UpdateDrugBillingDispositionAsync(
            Guid invoiceId,
            UpdateDrugBillingDispositionRequest request,
            Guid actorUserId,
            string? idempotencyKey,
            CancellationToken cancellationToken = default)
        {
            // BIL-VAL-063: Header Idempotency-Key tidak disertakan pada perintah PUT
            if (string.IsNullOrWhiteSpace(idempotencyKey))
            {
                throw new BillingPayerEditBadRequestException("Header Idempotency-Key wajib disertakan pada permintaan ini.");
            }

            // BIL-VAL-062: Alasan kosong pada permintaan yang mewajibkan
            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                throw new BillingPayerEditBadRequestException("Alasan perubahan wajib diisi.");
            }

            if (string.IsNullOrWhiteSpace(request.ExpectedRowVersion) || !Guid.TryParse(request.ExpectedRowVersion, out var expectedRowVersionGuid))
            {
                throw new BillingPayerEditBadRequestException("ExpectedRowVersion tidak valid.");
            }

            var mode = request.Mode?.Trim().ToUpperInvariant();
            if (mode != "ALL_REDEEMED" && mode != "PARTIAL_REDEEMED" && mode != "NOT_REDEEMED")
            {
                throw new BillingPayerEditBadRequestException("Mode penebusan obat tidak dikenali.");
            }

            // BIL-VAL-083: Mode PARTIAL_REDEEMED tetapi daftar baris yang ditebus kosong
            if (mode == "PARTIAL_REDEEMED" && (request.IncludedInvoiceItemIds == null || request.IncludedInvoiceItemIds.Count == 0))
            {
                throw new BillingPayerEditBadRequestException("Pilih baris obat yang ditebus, atau pilih Tidak Ditebus untuk seluruhnya.");
            }

            // BIL-VAL-084: Mode ALL_REDEEMED atau NOT_REDEEMED tetapi daftar baris tetap dikirim
            if ((mode == "ALL_REDEEMED" || mode == "NOT_REDEEMED") && (request.IncludedInvoiceItemIds != null && request.IncludedInvoiceItemIds.Count > 0))
            {
                throw new BillingPayerEditBadRequestException("Daftar baris hanya dipakai pada penebusan sebagian.");
            }

            // Cek duplikasi item id pada IncludedInvoiceItemIds
            if (request.IncludedInvoiceItemIds != null && request.IncludedInvoiceItemIds.GroupBy(x => x).Any(g => g.Count() > 1))
            {
                throw new BillingPayerEditBadRequestException("Ada baris biaya yang dikirim lebih dari satu kali.");
            }

            IDbContextTransaction? transaction = null;
            try
            {
                if (_dbContext.Database.IsRelational() && _dbContext.Database.CurrentTransaction is null)
                {
                    transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
                }

                if (_dbContext.Database.IsRelational())
                {
                    await AcquireLockAsync($"BIL_CALCULATION_{invoiceId:N}", cancellationToken);
                }

                // Idempotency: periksa apakah permintaan dengan Idempotency-Key yang sama sudah pernah dieksekusi
                var priorCommand = await _dbContext.BilInvoicePayerChangeCommands.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey, cancellationToken);
                if (priorCommand != null)
                {
                    if (priorCommand.InvoiceId != invoiceId)
                    {
                        throw new BillingPayerEditConflictException("Permintaan yang sama memiliki isi berbeda; gunakan permintaan baru.");
                    }

                    var replayInvoice = await _dbContext.BilInvoices.AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == invoiceId && !x.IsDelete, cancellationToken);
                    var replayCalculation = await _calculationService.PreviewCalculationAsync(invoiceId, actorUserId, cancellationToken);

                    if (transaction is not null) await transaction.CommitAsync(cancellationToken);

                    return new InvoiceEditResultResponse
                    {
                        Calculation = replayCalculation,
                        RowVersion = replayInvoice?.RowVersion ?? Guid.Empty,
                        ResetAssignmentCount = 0,
                        Warnings = ["Permintaan ini sudah pernah diproses sebelumnya (idempotent replay)."]
                    };
                }

                // Validasi Tagihan
                var invoice = await _dbContext.BilInvoices
                    .Include(x => x.Items).ThenInclude(x => x.Category)
                    .FirstOrDefaultAsync(x => x.Id == invoiceId && !x.IsDelete, cancellationToken)
                    ?? throw new KeyNotFoundException("Invoice Billing tidak ditemukan.");

                if (_dbContext.Database.IsRelational())
                {
                    await AcquireLockAsync($"BIL_ENCOUNTER_{invoice.EncounterId:N}", cancellationToken);
                }

                // BIL-VAL-059: Tagihan tidak berstatus OPEN
                if (invoice.Status != BillingInvoiceStatuses.Open)
                {
                    throw new BillingPayerEditConflictException("Tagihan ini sudah difinalisasi sehingga tidak dapat diubah lagi.");
                }

                // BIL-VAL-060: Sudah ada pembayaran berhasil pada tagihan ini
                var paidAmount = await _dbContext.BilPaymentAllocations.AsNoTracking()
                    .Where(x => x.TargetType == BillingAllocationTargetTypes.Invoice && x.TargetId == invoice.Id && !x.IsDelete)
                    .SumAsync(x => (decimal?)(x.ReversesAllocationId.HasValue ? -x.Amount : x.Amount), cancellationToken) ?? 0;

                if (paidAmount > 0)
                {
                    throw new BillingPayerEditConflictException("Tagihan ini sudah menerima pembayaran. Perubahan penjamin atau penanggung memerlukan proses pembalikan, bukan pengeditan.");
                }

                // BIL-VAL-061: Versi baris tagihan yang dikirim berbeda dari yang tersimpan
                if (invoice.RowVersion != expectedRowVersionGuid)
                {
                    throw new BillingPayerEditConflictException("Data tagihan telah berubah sejak layar ini dibuka. Muat ulang data sebelum menyimpan kembali.");
                }

                // BIL-VAL-081: Kunjungan berjenis rawat inap
                if (string.Equals(invoice.ServiceType, AdministrationFeeServiceTypes.Ranap, StringComparison.OrdinalIgnoreCase))
                {
                    throw new BillingPayerEditValidationException("Penebusan obat tidak dapat diubah untuk kunjungan rawat inap.");
                }

                // Daftar item obat yang layak pada tagihan ini
                var activeItems = invoice.Items
                    .Where(x => !x.IsDelete && x.Status != BillingInvoiceItemStatuses.Voided)
                    .ToList();

                var eligibleDrugItems = activeItems
                    .Where(x => (x.Category != null && x.Category.IsPharmacy) || x.SourceDomain == "PHARMACY")
                    .ToList();

                // BIL-VAL-086: Tagihan tidak memiliki satu pun baris obat yang layak
                if (eligibleDrugItems.Count == 0)
                {
                    throw new BillingPayerEditValidationException("Tagihan ini tidak memiliki item obat yang dapat diatur penebusannya.");
                }

                var eligibleDrugItemMap = eligibleDrugItems.ToDictionary(x => x.Id);
                var allInvoiceItemsMap = invoice.Items.Where(x => !x.IsDelete).ToDictionary(x => x.Id);

                if (mode == "PARTIAL_REDEEMED")
                {
                    foreach (var itemId in request.IncludedInvoiceItemIds)
                    {
                        // Cek apakah item ada di tagihan
                        if (!allInvoiceItemsMap.TryGetValue(itemId, out var item))
                        {
                            throw new BillingPayerEditValidationException("Ada baris obat yang tidak dapat diatur penebusannya pada tagihan ini."); // BIL-VAL-085
                        }

                        // BIL-VAL-082: Baris yang dikirim bukan item obat
                        bool isDrug = (item.Category != null && item.Category.IsPharmacy) || item.SourceDomain == "PHARMACY";
                        if (!isDrug)
                        {
                            throw new BillingPayerEditValidationException("Hanya baris obat yang dapat diatur penebusannya.");
                        }

                        // BIL-VAL-085: Item dibatalkan / tidak termasuk obat yang layak
                        if (item.Status == BillingInvoiceItemStatuses.Voided || !eligibleDrugItemMap.ContainsKey(itemId))
                        {
                            throw new BillingPayerEditValidationException("Ada baris obat yang tidak dapat diatur penebusannya pada tagihan ini.");
                        }
                    }
                }

                // Catat versi kalkulasi sebelumnya untuk snapshot audit
                var previousCalcVersion = await _dbContext.BilCalculationVersions.AsNoTracking()
                    .Where(x => x.InvoiceId == invoiceId && !x.IsDelete)
                    .OrderByDescending(x => x.VersionNo)
                    .FirstOrDefaultAsync(cancellationToken);
                var previousCalcVersionId = previousCalcVersion?.Id;

                // Penulisan disposisi: nonaktifkan-lalu-sisipkan (append-only mutation)
                var drugItemIds = eligibleDrugItems.Select(x => x.Id).ToList();
                var existingActiveDispositions = await _dbContext.BilInvoiceItemBillingDispositions
                    .Where(x => drugItemIds.Contains(x.InvoiceItemId) && x.IsActive && !x.IsDelete)
                    .ToListAsync(cancellationToken);

                var now = DateTime.UtcNow;
                if (existingActiveDispositions.Count > 0)
                {
                    foreach (var existing in existingActiveDispositions)
                    {
                        existing.IsActive = false;
                        existing.UpdateBy = actorUserId;
                        existing.UpdateDateTime = now;
                    }
                    // Simpan deaktifasi terlebih dahulu untuk memastikan integritas filtered unique index
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }

                var includedSet = (mode == "PARTIAL_REDEEMED")
                    ? new HashSet<Guid>(request.IncludedInvoiceItemIds)
                    : [];

                foreach (var drugItem in eligibleDrugItems)
                {
                    string targetDisposition;
                    if (mode == "ALL_REDEEMED")
                    {
                        targetDisposition = "INCLUDED";
                    }
                    else if (mode == "NOT_REDEEMED")
                    {
                        targetDisposition = "EXCLUDED";
                    }
                    else // PARTIAL_REDEEMED
                    {
                        targetDisposition = includedSet.Contains(drugItem.Id) ? "INCLUDED" : "EXCLUDED";
                    }

                    var newDisposition = new BilInvoiceItemBillingDisposition
                    {
                        InvoiceItemId = drugItem.Id,
                        Disposition = targetDisposition,
                        DecisionSource = "MANUAL",
                        Reason = request.Reason,
                        IsActive = true,
                        CreateBy = actorUserId,
                        CreateDateTime = now
                    };
                    _dbContext.BilInvoiceItemBillingDispositions.Add(newDisposition);
                }
                await _dbContext.SaveChangesAsync(cancellationToken);

                // Hitung ulang tagihan secara atomik
                var recalcResult = await _calculationService.RecalculateAsync(
                    invoice.Id,
                    new RecalculateInvoiceRequest
                    {
                        ExpectedRowVersion = invoice.RowVersion,
                        Reason = $"Perhitungan ulang otomatis akibat perubahan disposisi obat ({mode}). Alasan: {request.Reason}"
                    },
                    actorUserId,
                    cancellationToken);

                // Catat jejak perintah pada BilInvoicePayerChangeCommand (MPY-DES-003, FR-BKC-078)
                var changeCommand = new BilInvoicePayerChangeCommand
                {
                    InvoiceId = invoice.Id,
                    EncounterId = invoice.EncounterId,
                    PreviousPayerKind = "DRUG_DISPOSITION",
                    NewPayerKind = $"DRUG_{mode}",
                    PreviousPayerNameSnapshot = $"Drug Disposition Mode: {mode}",
                    NewPayerNameSnapshot = $"Drug Disposition Mode: {mode}",
                    PreviousCalculationVersionId = previousCalcVersionId,
                    NewCalculationVersionId = recalcResult.Id,
                    ResetAssignmentCount = 0,
                    Reason = request.Reason,
                    IdempotencyKey = idempotencyKey,
                    CorrelationId = request.CorrelationId,
                    CausationId = request.CausationId,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                };
                _dbContext.BilInvoicePayerChangeCommands.Add(changeCommand);
                await _dbContext.SaveChangesAsync(cancellationToken);

                if (transaction is not null)
                {
                    await transaction.CommitAsync(cancellationToken);
                }

                await _loggerService.AuditAsync(LogCategory, "BillingInvoice.UpdateDrugBillingDisposition", "Disposisi penebusan obat berhasil diperbarui dan tagihan dihitung ulang.", new
                {
                    InvoiceId = invoice.Id,
                    EncounterId = invoice.EncounterId,
                    Mode = mode,
                    EligibleDrugCount = eligibleDrugItems.Count,
                    IncludedCount = (mode == "ALL_REDEEMED") ? eligibleDrugItems.Count : (mode == "NOT_REDEEMED" ? 0 : request.IncludedInvoiceItemIds.Count),
                    NewCalculationVersion = recalcResult.VersionNo,
                    recalcResult.PatientAmount,
                    recalcResult.PrimaryAmount,
                    IdempotencyKey = idempotencyKey
                });

                return new InvoiceEditResultResponse
                {
                    Calculation = recalcResult,
                    RowVersion = recalcResult.InvoiceRowVersion,
                    ResetAssignmentCount = 0,
                    Warnings = []
                };
            }
            catch
            {
                if (transaction is not null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                }
                throw;
            }
        }

        // =========================================================================
        // PRIVATE HELPER METHODS
        // =========================================================================

        private static CurrentPayerResponse MapCurrentPayer(RegPatientEncounterGuarantor? source, RegPatientEncounter encounter)
        {
            if (source is null || source.PaymentType == EncounterPaymentType.Cash)
            {
                return new CurrentPayerResponse
                {
                    PaymentType = "CASH",
                    PaymentSourceName = "Tunai",
                    CardNumberMasked = null,
                    EffectiveStartDate = null,
                    EffectiveEndDate = null,
                    IsEligible = true,
                    IsPolicyActive = true,
                    PatientInsuranceId = null,
                    PatientCompanyGuarantorId = null,
                    PaymentMethodId = source?.PaymentMethodId ?? encounter.PaymentMethodId,
                    InsuranceProviderId = null,
                    CompanyGuarantorId = null
                };
            }

            if (source.PaymentType == EncounterPaymentType.Insurance)
            {
                return new CurrentPayerResponse
                {
                    PaymentType = "INSURANCE",
                    PaymentSourceName = source.InsuranceProvider?.InsuranceProviderName ?? source.PlanNameSnapshot ?? "Asuransi",
                    CardNumberMasked = MaskCardNumber(source.CardNumberSnapshot),
                    EffectiveStartDate = source.PatientInsurance?.EffectiveStartDate,
                    EffectiveEndDate = source.PatientInsurance?.EffectiveEndDate,
                    IsEligible = source.PatientInsurance?.IsEligible ?? true,
                    IsPolicyActive = source.PatientInsurance?.IsActive ?? false,
                    PatientInsuranceId = source.PatientInsuranceId,
                    PatientCompanyGuarantorId = null,
                    PaymentMethodId = null,
                    InsuranceProviderId = source.InsuranceProviderId,
                    CompanyGuarantorId = null
                };
            }

            // CompanyGuarantor
            return new CurrentPayerResponse
            {
                PaymentType = "COMPANY_GUARANTOR",
                PaymentSourceName = source.CompanyGuarantor?.CompanyGuarantorName ?? source.CompanyGuarantorCodeSnapshot ?? "Penjamin Perusahaan",
                CardNumberMasked = MaskCardNumber(source.EmployeeNumberSnapshot),
                EffectiveStartDate = source.PatientCompanyGuarantor?.EffectiveStartDate,
                EffectiveEndDate = source.PatientCompanyGuarantor?.EffectiveEndDate,
                IsEligible = source.PatientCompanyGuarantor?.IsEligible ?? true,
                IsPolicyActive = source.PatientCompanyGuarantor?.IsActive ?? false,
                PatientInsuranceId = null,
                PatientCompanyGuarantorId = source.PatientCompanyGuarantorId,
                PaymentMethodId = null,
                InsuranceProviderId = null,
                CompanyGuarantorId = source.CompanyGuarantorId
            };
        }

        private async Task<List<AvailablePayerOptionResponse>> BuildAvailablePayerOptionsAsync(
            RegPatientEncounter encounter,
            CancellationToken cancellationToken)
        {
            var options = new List<AvailablePayerOptionResponse>
            {
                new AvailablePayerOptionResponse
                {
                    PayerType = "CASH",
                    PayerName = "Tunai",
                    IsActive = true,
                    IsEligible = true,
                    IsCurrentlyAvailable = true,
                    ReasonIfNotAvailable = null
                }
            };

            // Kartu Asuransi Pasien
            var patientInsurances = await _dbContext.MstPatientInsurances.AsNoTracking()
                .Include(x => x.InsuranceProvider)
                .Where(x => x.PatientId == encounter.PatientId && !x.IsDelete)
                .OrderBy(x => x.PolicyNumber)
                .ToListAsync(cancellationToken);

            foreach (var ins in patientInsurances)
            {
                var isDateValid = (!ins.EffectiveStartDate.HasValue || encounter.EncounterDate.Date >= ins.EffectiveStartDate.Value.Date) &&
                                  (!ins.EffectiveEndDate.HasValue || encounter.EncounterDate.Date <= ins.EffectiveEndDate.Value.Date);
                var isProviderActive = ins.InsuranceProvider != null && ins.InsuranceProvider.IsActive && !ins.InsuranceProvider.IsDelete &&
                                      (!ins.InsuranceProvider.ContractEndDate.HasValue || encounter.EncounterDate.Date <= ins.InsuranceProvider.ContractEndDate.Value.Date);
                var isAvailable = ins.IsActive && ins.IsEligible && isDateValid && isProviderActive;

                string? reason = null;
                if (!isProviderActive)
                    reason = "Kerja sama dengan perusahaan asuransi ini sudah berakhir pada tanggal pelayanan.";
                else if (!ins.IsActive)
                    reason = "Kartu penjamin yang dipilih sudah tidak berlaku. Perbarui data penjamin pasien di Registrasi.";
                else if (!isDateValid)
                    reason = "Kartu penjamin ini tidak berlaku pada tanggal pelayanan. Pilih kartu lain atau perbarui masa berlakunya di Registrasi.";
                else if (!ins.IsEligible)
                    reason = "Kartu penjamin ini belum dinyatakan layak. Periksa kelayakannya di Registrasi sebelum dipakai.";

                options.Add(new AvailablePayerOptionResponse
                {
                    PayerType = "INSURANCE",
                    PayerName = ins.InsuranceProvider?.InsuranceProviderName ?? ins.PlanName ?? "Asuransi",
                    PatientInsuranceId = ins.Id,
                    CardNumberMasked = MaskCardNumber(ins.CardNumber),
                    PolicyNumber = ins.PolicyNumber,
                    BenefitPlanCode = ins.BenefitPlanCode,
                    BenefitPlanName = ins.PlanName,
                    EffectiveStartDate = ins.EffectiveStartDate,
                    EffectiveEndDate = ins.EffectiveEndDate,
                    IsActive = ins.IsActive,
                    IsEligible = ins.IsEligible,
                    IsCurrentlyAvailable = isAvailable,
                    ReasonIfNotAvailable = reason
                });
            }

            // Kartu Penjamin Perusahaan Pasien
            var patientGuarantors = await _dbContext.MstPatientCompanyGuarantors.AsNoTracking()
                .Include(x => x.CompanyGuarantor)
                .Where(x => x.PatientId == encounter.PatientId && !x.IsDelete)
                .OrderBy(x => x.EmployeeNumber)
                .ToListAsync(cancellationToken);

            foreach (var gua in patientGuarantors)
            {
                var isDateValid = (!gua.EffectiveStartDate.HasValue || encounter.EncounterDate.Date >= gua.EffectiveStartDate.Value.Date) &&
                                  (!gua.EffectiveEndDate.HasValue || encounter.EncounterDate.Date <= gua.EffectiveEndDate.Value.Date);
                var isCompanyActive = gua.CompanyGuarantor != null && gua.CompanyGuarantor.IsActive && !gua.CompanyGuarantor.IsDelete &&
                                      (!gua.CompanyGuarantor.ContractEndDate.HasValue || encounter.EncounterDate.Date <= gua.CompanyGuarantor.ContractEndDate.Value.Date);
                var isAvailable = gua.IsActive && gua.IsEligible && isDateValid && isCompanyActive;

                string? reason = null;
                if (!isCompanyActive)
                    reason = "Kerja sama dengan perusahaan penjamin ini sudah berakhir pada tanggal pelayanan.";
                else if (!gua.IsActive)
                    reason = "Kartu penjamin yang dipilih sudah tidak berlaku. Perbarui data penjamin pasien di Registrasi.";
                else if (!isDateValid)
                    reason = "Kartu penjamin ini tidak berlaku pada tanggal pelayanan. Pilih kartu lain atau perbarui masa berlakunya di Registrasi.";
                else if (!gua.IsEligible)
                    reason = "Kartu penjamin ini belum dinyatakan layak. Periksa kelayakannya di Registrasi sebelum dipakai.";

                options.Add(new AvailablePayerOptionResponse
                {
                    PayerType = "COMPANY_GUARANTOR",
                    PayerName = gua.CompanyGuarantor?.CompanyGuarantorName ?? gua.BenefitPlanName ?? "Penjamin Perusahaan",
                    PatientCompanyGuarantorId = gua.Id,
                    CardNumberMasked = MaskCardNumber(gua.EmployeeNumber),
                    PolicyNumber = gua.CompanyGuarantor?.CompanyGuarantorCode,
                    EmployeeNumber = gua.EmployeeNumber,
                    BenefitPlanCode = gua.BenefitPlanCode,
                    BenefitPlanName = gua.BenefitPlanName,
                    EffectiveStartDate = gua.EffectiveStartDate,
                    EffectiveEndDate = gua.EffectiveEndDate,
                    IsActive = gua.IsActive,
                    IsEligible = gua.IsEligible,
                    IsCurrentlyAvailable = isAvailable,
                    ReasonIfNotAvailable = reason
                });
            }

            return options;
        }

        private async Task<(bool CanApply, string? BlockReason, EncounterPaymentType CandidatePaymentType, string CandidatePayerKind, string CandidatePayerName)>
            ValidateCandidatePayerAsync(
                RegPatientEncounter encounter,
                RegPatientEncounterGuarantor? currentSource,
                PayerComparisonPreviewRequest request,
                CancellationToken cancellationToken)
        {
            EncounterPaymentType targetType;
            if (string.Equals(request.CandidatePaymentType, "CASH", StringComparison.OrdinalIgnoreCase))
                targetType = EncounterPaymentType.Cash;
            else if (string.Equals(request.CandidatePaymentType, "INSURANCE", StringComparison.OrdinalIgnoreCase))
                targetType = EncounterPaymentType.Insurance;
            else if (string.Equals(request.CandidatePaymentType, "COMPANY_GUARANTOR", StringComparison.OrdinalIgnoreCase))
                targetType = EncounterPaymentType.CompanyGuarantor;
            else
                throw new BillingPayerEditBadRequestException("Jenis pembayaran tidak dikenali."); // BIL-VAL-064

            // BIL-VAL-065: paymentType = CASH tetapi ada id kartu asuransi atau kartu perusahaan yang ikut dikirim
            if (targetType == EncounterPaymentType.Cash &&
                (request.CandidatePatientInsuranceId.HasValue || request.CandidatePatientCompanyGuarantorId.HasValue))
            {
                throw new BillingPayerEditBadRequestException("Pembayaran tunai tidak boleh disertai kartu penjamin.");
            }

            // BIL-VAL-066: paymentType = INSURANCE tetapi kartu asuransi tidak dikirim, atau justru kartu perusahaan yang dikirim
            if (targetType == EncounterPaymentType.Insurance &&
                (!request.CandidatePatientInsuranceId.HasValue || request.CandidatePatientCompanyGuarantorId.HasValue))
            {
                throw new BillingPayerEditBadRequestException("Pilih kartu asuransi yang akan dipakai.");
            }

            // BIL-VAL-067: paymentType = COMPANY_GUARANTOR tetapi kartu penjamin perusahaan tidak dikirim, atau justru kartu asuransi yang dikirim
            if (targetType == EncounterPaymentType.CompanyGuarantor &&
                (!request.CandidatePatientCompanyGuarantorId.HasValue || request.CandidatePatientInsuranceId.HasValue))
            {
                throw new BillingPayerEditBadRequestException("Pilih kartu penjamin perusahaan yang akan dipakai.");
            }

            if (targetType == EncounterPaymentType.Cash)
            {
                // BIL-VAL-073: Payer kandidat sama persis dengan payer yang sedang berlaku
                if (currentSource is null || currentSource.PaymentType == EncounterPaymentType.Cash)
                {
                    return (false, "Penjamin yang dipilih sama dengan yang sedang dipakai. Tidak ada yang perlu diubah.", targetType, "CASH", "Tunai");
                }
                return (true, null, targetType, "CASH", "Tunai");
            }

            if (targetType == EncounterPaymentType.Insurance)
            {
                var card = await _dbContext.MstPatientInsurances.AsNoTracking()
                    .Include(x => x.InsuranceProvider)
                    .FirstOrDefaultAsync(x => x.Id == request.CandidatePatientInsuranceId!.Value, cancellationToken);

                // BIL-VAL-068: Kartu yang dipilih bukan milik pasien pada kunjungan ini
                if (card is null || card.PatientId != encounter.PatientId)
                {
                    return (false, "Kartu penjamin yang dipilih bukan milik pasien ini.", targetType, "INSURANCE", "Asuransi");
                }

                // BIL-VAL-069: Kartu yang dipilih sudah tidak aktif atau sudah ditandai terhapus
                if (!card.IsActive || card.IsDelete)
                {
                    return (false, "Kartu penjamin yang dipilih sudah tidak berlaku. Perbarui data penjamin pasien di Registrasi.", targetType, "INSURANCE", card.InsuranceProvider?.InsuranceProviderName ?? card.PlanName ?? "Asuransi");
                }

                // BIL-VAL-070: Tanggal layanan berada di luar masa berlaku kartu
                if ((card.EffectiveStartDate.HasValue && encounter.EncounterDate.Date < card.EffectiveStartDate.Value.Date) ||
                    (card.EffectiveEndDate.HasValue && encounter.EncounterDate.Date > card.EffectiveEndDate.Value.Date))
                {
                    return (false, "Kartu penjamin ini tidak berlaku pada tanggal pelayanan. Pilih kartu lain atau perbarui masa berlakunya di Registrasi.", targetType, "INSURANCE", card.InsuranceProvider?.InsuranceProviderName ?? card.PlanName ?? "Asuransi");
                }

                // BIL-VAL-071: Kartu belum dinyatakan layak dipakai
                if (!card.IsEligible)
                {
                    return (false, "Kartu penjamin ini belum dinyatakan layak. Periksa kelayakannya di Registrasi sebelum dipakai.", targetType, "INSURANCE", card.InsuranceProvider?.InsuranceProviderName ?? card.PlanName ?? "Asuransi");
                }

                // BIL-VAL-072: Perusahaan asuransi pada kartu sudah tidak aktif atau kontraknya sudah berakhir
                if (card.InsuranceProvider is null || !card.InsuranceProvider.IsActive || card.InsuranceProvider.IsDelete ||
                    (card.InsuranceProvider.ContractEndDate.HasValue && encounter.EncounterDate.Date > card.InsuranceProvider.ContractEndDate.Value.Date))
                {
                    return (false, "Kerja sama dengan perusahaan asuransi ini sudah berakhir pada tanggal pelayanan.", targetType, "INSURANCE", card.InsuranceProvider?.InsuranceProviderName ?? card.PlanName ?? "Asuransi");
                }

                // BIL-VAL-073: Payer kandidat sama persis dengan payer yang sedang berlaku
                if (currentSource != null && currentSource.PaymentType == EncounterPaymentType.Insurance &&
                    currentSource.PatientInsuranceId == card.Id)
                {
                    return (false, "Penjamin yang dipilih sama dengan yang sedang dipakai. Tidak ada yang perlu diubah.", targetType, "INSURANCE", card.InsuranceProvider.InsuranceProviderName);
                }

                return (true, null, targetType, "INSURANCE", card.InsuranceProvider.InsuranceProviderName);
            }

            // CompanyGuarantor
            var compCard = await _dbContext.MstPatientCompanyGuarantors.AsNoTracking()
                .Include(x => x.CompanyGuarantor)
                .FirstOrDefaultAsync(x => x.Id == request.CandidatePatientCompanyGuarantorId!.Value, cancellationToken);

            // BIL-VAL-068: Kartu yang dipilih bukan milik pasien pada kunjungan ini
            if (compCard is null || compCard.PatientId != encounter.PatientId)
            {
                return (false, "Kartu penjamin yang dipilih bukan milik pasien ini.", targetType, "COMPANY_GUARANTOR", "Penjamin Perusahaan");
            }

            // BIL-VAL-069: Kartu yang dipilih sudah tidak aktif atau sudah ditandai terhapus
            if (!compCard.IsActive || compCard.IsDelete)
            {
                return (false, "Kartu penjamin yang dipilih sudah tidak berlaku. Perbarui data penjamin pasien di Registrasi.", targetType, "COMPANY_GUARANTOR", compCard.CompanyGuarantor?.CompanyGuarantorName ?? compCard.BenefitPlanName ?? "Penjamin Perusahaan");
            }

            // BIL-VAL-070: Tanggal layanan berada di luar masa berlaku kartu
            if ((compCard.EffectiveStartDate.HasValue && encounter.EncounterDate.Date < compCard.EffectiveStartDate.Value.Date) ||
                (compCard.EffectiveEndDate.HasValue && encounter.EncounterDate.Date > compCard.EffectiveEndDate.Value.Date))
            {
                return (false, "Kartu penjamin ini tidak berlaku pada tanggal pelayanan. Pilih kartu lain atau perbarui masa berlakunya di Registrasi.", targetType, "COMPANY_GUARANTOR", compCard.CompanyGuarantor?.CompanyGuarantorName ?? compCard.BenefitPlanName ?? "Penjamin Perusahaan");
            }

            // BIL-VAL-071: Kartu belum dinyatakan layak dipakai
            if (!compCard.IsEligible)
            {
                return (false, "Kartu penjamin ini belum dinyatakan layak. Periksa kelayakannya di Registrasi sebelum dipakai.", targetType, "COMPANY_GUARANTOR", compCard.CompanyGuarantor?.CompanyGuarantorName ?? compCard.BenefitPlanName ?? "Penjamin Perusahaan");
            }

            // BIL-VAL-072: Perusahaan penjamin sudah tidak aktif atau kontraknya sudah berakhir
            if (compCard.CompanyGuarantor is null || !compCard.CompanyGuarantor.IsActive || compCard.CompanyGuarantor.IsDelete ||
                (compCard.CompanyGuarantor.ContractEndDate.HasValue && encounter.EncounterDate.Date > compCard.CompanyGuarantor.ContractEndDate.Value.Date))
            {
                return (false, "Kerja sama dengan perusahaan penjamin ini sudah berakhir pada tanggal pelayanan.", targetType, "COMPANY_GUARANTOR", compCard.CompanyGuarantor?.CompanyGuarantorName ?? compCard.BenefitPlanName ?? "Penjamin Perusahaan");
            }

            // BIL-VAL-073: Payer kandidat sama persis dengan payer yang sedang berlaku
            if (currentSource != null && currentSource.PaymentType == EncounterPaymentType.CompanyGuarantor &&
                currentSource.PatientCompanyGuarantorId == compCard.Id)
            {
                return (false, "Penjamin yang dipilih sama dengan yang sedang dipakai. Tidak ada yang perlu diubah.", targetType, "COMPANY_GUARANTOR", compCard.CompanyGuarantor.CompanyGuarantorName);
            }

            return (true, null, targetType, "COMPANY_GUARANTOR", compCard.CompanyGuarantor.CompanyGuarantorName);
        }

        private static string? MaskCardNumber(string? cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber)) return cardNumber;
            var trimmed = cardNumber.Trim();
            if (trimmed.Length <= 4) return new string('*', trimmed.Length);
            return string.Concat(new string('*', trimmed.Length - 4), trimmed.AsSpan(trimmed.Length - 4));
        }

        private static decimal Money(decimal value) => decimal.Round(value, 2, MidpointRounding.AwayFromZero);

        private async Task AcquireLockAsync(string key, CancellationToken cancellationToken) =>
            await _dbContext.Database.ExecuteSqlRawAsync(
                "SELECT pg_advisory_xact_lock(hashtext({0}));", [key], cancellationToken);
    }

    // =========================================================================
    // DOMAIN EXCEPTIONS
    // =========================================================================

    public sealed class BillingPayerEditBadRequestException(string message) : Exception(message);
    public sealed class BillingPayerEditValidationException(string message) : Exception(message);
    public sealed class BillingPayerEditConflictException(string message) : Exception(message);
}
