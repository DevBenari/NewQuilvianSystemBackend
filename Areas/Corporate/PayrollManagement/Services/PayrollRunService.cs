using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Options;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Data;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;

/// <summary>
/// Siklus payroll run milik modul Payroll: buka (Draft) -> impor HRD (CollectingInput) -> hitung (Calculated) -> ajukan (Submitted)
/// -> sahkan (Approved, terkunci) -> kirim ke Finance (SentToFinance) -> hasil bank (PartiallyPaid/Paid) -> tutup (Closed).
///
/// Prinsip (keputusan Finance-HRD yang dipertahankan):
/// - Payroll pemilik run dan hasil potongan; Finance pemilik piutang dan jadwal angsuran. Finance tidak membuat run.
/// - Saat run dibuka dan dihitung, jadwal angsuran periode itu DITARIK dari Finance (dan hanya periode sejak peralihan).
/// - Potongan dihitung saat payroll dihitung, tetapi saldo piutang HANYA berkurang saat run DISAHKAN, atomik dalam satu
///   transaksi serializable: bila satu hasil gagal diterima Finance, pengesahan dibatalkan seluruhnya. Titik pengakuan penyelesaian
///   ini sengaja tidak diubah; apakah seharusnya menunggu konfirmasi pembayaran adalah NEEDS CONFIRMATION (pemilik Finance).
/// - Run yang sudah disahkan terkunci. Koreksi lewat pembalikan Finance dengan approval, bukan menghitung ulang.
///
/// Perhitungan gaji didelegasikan ke <see cref="PayrollCalculationService"/> (gaji pokok dari penetapan gaji disetujui dengan prorata,
/// tunjangan, lembur, kehadiran, input variabel, pajak/iuran dari tetapan profil). Cicilan piutang dipotong sesudahnya.
/// </summary>
public sealed class PayrollRunService
{
    // String kategori audit sengaja dipertahankan (bukan namespace) agar riwayat audit payroll run tidak terpecah.
    private const string LogCategory = "Corporate.HumanResource.PayrollManagement.PayrollRun";
    private const string RegularRunType = "Regular";
    private const string EmployeeCalculated = "Calculated";
    private const string EmployeeError = "Error";
    private const string EmployeeFinalized = "Finalized";
    private const string InstallmentComponentCode = "FINANCE_INSTALLMENT_DEDUCTION";

    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;
    private readonly FinanceReceivablePayrollSyncService _syncService;
    private readonly PayrollInstallmentDeductionService _deductionService;
    private readonly PayrollCalculationService _calculationService;
    private readonly PayrollProfileProvisioningService _provisioningService;
    private readonly PayrollPolicyOptions _policy;

    public PayrollRunService(
        ApplicationDbContext dbContext,
        LoggerService loggerService,
        FinanceReceivablePayrollSyncService syncService,
        PayrollInstallmentDeductionService deductionService,
        PayrollCalculationService calculationService,
        PayrollProfileProvisioningService provisioningService,
        IOptions<PayrollPolicyOptions> policyOptions)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
        _syncService = syncService;
        _deductionService = deductionService;
        _calculationService = calculationService;
        _provisioningService = provisioningService;
        _policy = policyOptions.Value;
    }

    /// <summary>Warnings bertipe opsional supaya JSON lama (tanpa Warnings) tetap terbaca.</summary>
    /// <summary>
    /// PAY-BE-FIX-030: jejak pengajuan untuk review dengan PPh sementara. Tersimpan pada ValidationSummaryJson (tanpa migration) selama run
    /// berstatus Submitted; dihapus saat run dikembalikan dan tertimpa saat dihitung ulang. Jejak abadinya ada pada PrlPayrollApproval
    /// (aksi SubmitForReview) dan audit log.
    /// </summary>
    private sealed record ReviewOnlyMarker(
        DateTime SubmittedAt, Guid SubmittedByUserId, string Reason, int AffectedEmployeeCount, decimal TotalProvisionalTax,
        string? PolicyVersion, List<int> MissingTaxMonths, string BlockCode);

    /// <summary>
    /// PAY-BE-FIX-031: metadata klasifikasi penahan yang ditulis saat hitung (tanpa migration). <c>OtherBlockerCount</c> = penahan yang BUKAN
    /// histori PPh belum lengkap; dihitung dari kode penahan terstruktur mesin pajak, bukan dari pencocokan teks. Run yang dihitung sebelum
    /// metadata ini ada (null) tidak dapat dinilai kelayakan review-nya dan harus dihitung ulang.
    /// </summary>
    private sealed record TaxReviewMeta(int Version, int TaxHistoryBlockerCount, int OtherBlockerCount);

    /// <summary>
    /// PAY-BE-FIX-032: jejak pengesahan bersyarat (ADMINISTRATIF). Tersimpan pada ValidationSummaryJson selama run Submitted; run tetap
    /// Submitted, tidak terkunci, tidak dikonfirmasi ke Finance, dan tidak dibayar. Dilepas saat run dikembalikan; jejak abadinya ada pada
    /// PrlPayrollApproval (aksi ApproveConditionally) dan audit log.
    /// </summary>
    private sealed record ConditionalApprovalMarker(
        string Reference, DateTime ApprovedAt, Guid ApprovedByUserId, string Reason, int AffectedEmployeeCount, decimal ConfirmedTaxTotal,
        decimal ProvisionalTaxTotal, string? PolicyVersion, List<int> MissingTaxMonths, bool FollowUpRequired, string ReconciliationStatus);

    /// <summary>Warnings, ReviewOnly, TaxReview, dan ConditionalApproval bertipe opsional supaya JSON lama (tanpa kunci itu) tetap terbaca.</summary>
    private sealed record ValidationSummary(
        List<string> Blockers, List<string> SyncNotes, List<string>? Warnings = null, ReviewOnlyMarker? ReviewOnly = null,
        TaxReviewMeta? TaxReview = null, ConditionalApprovalMarker? ConditionalApproval = null);

    // ------------------------------------------------------------------------------------
    // Baca
    // ------------------------------------------------------------------------------------

    public async Task<PagedResult<PayrollRunListItemResponse>> GetPagedAsync(PayrollRunQuery request, CancellationToken cancellationToken)
    {
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = _dbContext.PrlPayrollRuns.AsNoTracking().Include(x => x.PayrollPeriod).Where(x => !x.IsDelete);
        if (request.PayrollPeriodId.HasValue) query = query.Where(x => x.PayrollPeriodId == request.PayrollPeriodId.Value);
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var statuses = request.Status.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (statuses.Length == 1) query = query.Where(x => x.RunStatus == statuses[0]);
            else if (statuses.Length > 1) query = query.Where(x => statuses.Contains(x.RunStatus));
        }
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(x => EF.Functions.ILike(x.RunNumber, $"%{term}%"));
        }

        var total = await query.CountAsync(cancellationToken);
        var runs = await query.OrderByDescending(x => x.CreateDateTime)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return new PagedResult<PayrollRunListItemResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)pageSize),
            Items = runs.Select(r => MapListItem(r, new PayrollRunListItemResponse())).ToList()
        };
    }

    public async Task<PayrollRunDetailResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var run = await _dbContext.PrlPayrollRuns.AsNoTracking()
            .Include(x => x.PayrollPeriod)
            .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Payroll run tidak ditemukan.");

        var employees = await _dbContext.PrlPayrollRunEmployees.AsNoTracking()
            .Include(x => x.Components)
            .Include(x => x.VariableInputs)
            .Where(x => x.PayrollRunId == id && !x.IsDelete)
            .OrderBy(x => x.EmployeeNameSnapshot)
            .ToListAsync(cancellationToken);

        var detail = MapListItem(run, new PayrollRunDetailResponse());
        detail.Notes = run.Notes;
        detail.PeriodStartDate = run.PeriodStartDateSnapshot;
        detail.PeriodEndDate = run.PeriodEndDateSnapshot;
        detail.PaymentDate = run.PaymentDateSnapshot;
        detail.PolicyVersion = ReadPolicyVersion(run.PolicySnapshotJson);
        // Penahan Finance Handoff produksi (dipisahkan dari urusan BPJS): run yang belum dihitung dianggap ditahan karena run baru ditandai saat dihitung.
        detail.FinanceHandoffHoldReason = PayrollFinanceHandoffGuard.ReasonOf(run.PolicySnapshotJson, run.CalculatedAt.HasValue);
        detail.IsFinanceHandoffHeld = detail.FinanceHandoffHoldReason is not null;

        // Indikator "perlu dihitung ulang": hanya relevan selama run belum disahkan (snapshot run yang disahkan terkunci).
        if (run.CalculatedAt.HasValue && !run.IsLocked && !PayrollRunStatuses.IsFinalized(run.RunStatus))
        {
            detail.RecalculationReasons = await FindChangedCompensationSourcesAsync(run, cancellationToken);
            detail.NeedsRecalculation = detail.RecalculationReasons.Count > 0;
        }

        var validation = ReadValidation(run.ValidationSummaryJson);
        detail.Blockers = validation.Blockers;
        detail.InstallmentSyncNotes = validation.SyncNotes;
        detail.Warnings = validation.Warnings ?? new List<string>();

        var reversals = await _dbContext.PrlPayrollReversals.AsNoTracking()
            .Where(x => x.PayrollRunId == id && !x.IsDelete)
            .OrderByDescending(x => x.CreateDateTime)
            .ToListAsync(cancellationToken);

        var employeeLines = employees.ToDictionary(e => e.Id, e => PayrollInstallmentDeductionService.ReadLine(e));
        var confirmedIds = employeeLines.Values.Where(l => l is { IsConfirmedToFinance: true }).Select(l => l!.InstallmentId).Distinct().ToList();
        var financeDeducted = confirmedIds.Count == 0
            ? new Dictionary<Guid, decimal>()
            : await _dbContext.FinReceivableInstallments.AsNoTracking()
                .Where(x => confirmedIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.PaidAmount, cancellationToken);
        foreach (var line in employeeLines.Values)
        {
            if (line is not null && financeDeducted.TryGetValue(line.InstallmentId, out var paid)) line.FinanceDeductedAmount = paid;
        }

        detail.Employees = employees.Select(e => new PayrollRunEmployeeResponse
        {
            Id = e.Id,
            WorkforceProfileId = e.WorkforceProfileId,
            EmployeeNumber = e.EmployeeNumberSnapshot ?? string.Empty,
            EmployeeName = e.EmployeeNameSnapshot,
            EmployeePayrollStatus = e.EmployeePayrollStatus,
            BaseSalary = e.BaseSalary,
            TotalRecurringEarning = e.TotalRecurringEarning,
            TotalVariableEarning = e.TotalVariableEarning,
            TotalOvertimeAmount = e.TotalOvertimeAmount,
            TotalAttendanceAllowance = e.TotalAttendanceAllowance,
            GrossPay = e.GrossPay,
            TotalTax = e.TotalTax,
            TotalEmployeeInsuranceContribution = e.TotalEmployeeInsuranceContribution,
            TotalDeduction = e.TotalDeduction,
            NetPay = e.NetPay,
            ValidationMessage = e.ValidationResultJson,
            Installment = employeeLines[e.Id],
            ReversedAmount = reversals
                .Where(r => r.PayrollRunEmployeeId == e.Id && r.ReversalStatus == PayrollReversalStatuses.Approved)
                .Sum(r => r.ReversalAmount),
            HasPendingReversal = reversals.Any(r => r.PayrollRunEmployeeId == e.Id && r.ReversalStatus == PayrollReversalStatuses.Requested),
            ProvisionalTax = ProvisionalTaxOf(e, ReadSnapshot<PayrollTaxSnapshot>(e.TaxSnapshotJson)),
            TaxFinalityStatus = e.TaxSnapshotJson is null ? string.Empty : ClassifyEmployeeTaxStatus(ReadSnapshot<PayrollTaxSnapshot>(e.TaxSnapshotJson)),
            OfficialTaxAmount = OfficialTaxOf(ReadSnapshot<PayrollTaxSnapshot>(e.TaxSnapshotJson))
        }).ToList();

        var employeeById = employees.ToDictionary(e => e.Id);
        detail.Reversals = reversals.Select(r =>
        {
            employeeById.TryGetValue(r.PayrollRunEmployeeId ?? Guid.Empty, out var owner);
            return new PayrollReversalResponse
            {
                Id = r.Id,
                ReversalNumber = r.ReversalNumber,
                PayrollRunEmployeeId = r.PayrollRunEmployeeId,
                EmployeeName = owner?.EmployeeNameSnapshot ?? string.Empty,
                EmployeeNumber = owner?.EmployeeNumberSnapshot ?? string.Empty,
                ReversalType = r.ReversalType,
                ReversalStatus = r.ReversalStatus,
                ReversalAmount = r.ReversalAmount,
                Reason = r.ReversalReason,
                Notes = r.Notes,
                RequestedAt = r.CreateDateTime,
                PostedAt = r.PostedAt
            };
        }).ToList();

        var lines = detail.Employees.Where(e => e.Installment is not null).Select(e => e.Installment!).ToList();
        detail.InstallmentCount = lines.Count;
        detail.TotalInstallmentDue = lines.Sum(l => l.DueAmount);
        detail.TotalInstallmentPlanned = lines.Sum(l => l.PlannedDeduction ?? 0m);

        var payments = await _dbContext.PrlPayrollPayments.AsNoTracking()
            .Where(x => x.PayrollRunId == id && !x.IsDelete)
            .Select(x => new { x.PaymentStatus, x.PaymentAmount, x.NetPaidAmount })
            .ToListAsync(cancellationToken);
        if (payments.Count > 0)
        {
            var live = payments.Where(p => PayrollPaymentStatuses.IsLive(p.PaymentStatus)).ToList();
            detail.PaymentSummary = new PayrollRunPaymentSummaryResponse
            {
                InstructionCount = live.Count,
                ScheduledCount = live.Count(p => p.PaymentStatus == PayrollPaymentStatuses.Scheduled || p.PaymentStatus == PayrollPaymentStatuses.Processing),
                PaidCount = live.Count(p => p.PaymentStatus == PayrollPaymentStatuses.Paid || p.PaymentStatus == PayrollPaymentStatuses.PartiallyReversed),
                FailedCount = payments.Count(p => p.PaymentStatus == PayrollPaymentStatuses.Failed),
                TotalInstructionAmount = live.Sum(p => p.PaymentAmount),
                TotalPaidAmount = live.Sum(p => p.NetPaidAmount)
            };

            // PAY-BE-11: potongan piutang yang sudah dikonfirmasi saat run disahkan, tetapi gaji penerimanya belum lunas atau gagal.
            var deductionAttention = await PayrollSalaryPaymentStatusResolver.CountConfirmedDeductionAttentionAsync(_dbContext, id, cancellationToken);
            detail.PaymentSummary.ConfirmedDeductionPaymentPendingCount = deductionAttention.Pending;
            detail.PaymentSummary.ConfirmedDeductionPaymentFailedCount = deductionAttention.Failed;
        }

        // PAY-BE-FIX-030: PPh sementara dan kelayakan pengajuan review. Sumber kebenarannya snapshot pajak penerima (kode penahan
        // terstruktur), bukan pencocokan teks pesan penahan.
        var taxReview = EvaluateTaxReview(validation, employees);
        detail.TaxReviewState = taxReview.State;
        detail.ProvisionalTax = BuildProvisionalSummary(employees);
        if (detail.ProvisionalTax is not null)
            detail.ProvisionalTax.ConfirmedTaxTotal = employees
                .Select(e => OfficialTaxOf(ReadSnapshot<PayrollTaxSnapshot>(e.TaxSnapshotJson)))
                .Where(amount => amount.HasValue)
                .Sum(amount => amount!.Value);
        if (validation.ReviewOnly is { } marker)
        {
            detail.ReviewOnlySubmission = new PayrollRunReviewOnlyResponse
            {
                SubmittedAt = marker.SubmittedAt,
                SubmittedByUserId = marker.SubmittedByUserId,
                Reason = marker.Reason,
                AffectedEmployeeCount = marker.AffectedEmployeeCount,
                TotalProvisionalTax = marker.TotalProvisionalTax,
                PolicyVersion = marker.PolicyVersion,
                MissingTaxMonths = marker.MissingTaxMonths,
                BlockCode = marker.BlockCode,
                TaxFinalStatus = PayrollTaxFinality.NotFinal
            };
        }

        detail.DecisionBlockReasons = await BuildDecisionBlockReasonsAsync(run, detail, taxReview, forReview: false, cancellationToken);
        if (string.Equals(run.RunStatus, PayrollRunStatuses.Calculated, StringComparison.OrdinalIgnoreCase) && taxReview.State != TaxReviewNotApplicable)
        {
            // Aturan kelayakan SAMA dengan SubmitForReviewAsync: EvaluateTaxReview + alasan non-pajak yang sama + estimasi tersedia.
            var reviewReasons = await BuildDecisionBlockReasonsAsync(run, detail, taxReview, forReview: true, cancellationToken);
            var codes = new List<string>();
            var taxReasonCount = 0;
            if (taxReview.State == TaxReviewRecalculationRequired) { codes.Add(PayrollRunReviewBlockCodes.RecalculationRequired); taxReasonCount++; }
            if (taxReview.State == TaxReviewBlocked) { codes.Add(PayrollRunReviewBlockCodes.OtherBlockers); taxReasonCount++; }
            if (taxReview.State == TaxReviewEligible)
            {
                var unavailable = UnavailableEstimateReasons(employees);
                if (unavailable.Count > 0)
                {
                    reviewReasons.AddRange(unavailable);
                    codes.Add(PayrollRunReviewBlockCodes.EstimateUnavailable);
                    taxReasonCount += unavailable.Count;
                }
            }

            if (reviewReasons.Count > taxReasonCount) codes.Add(PayrollRunReviewBlockCodes.OtherReasons);
            detail.SubmitForReviewBlockReasons = reviewReasons;
            detail.SubmitForReviewBlockCodes = codes;
            detail.ReviewRecalculationRequired = taxReview.State == TaxReviewRecalculationRequired && !run.IsLocked;
            detail.CanSubmitForReview = taxReview.State == TaxReviewEligible && reviewReasons.Count == 0;
        }

        // PAY-BE-FIX-032: pengesahan bersyarat (administratif). Aturan kelayakan yang sama dengan ApproveConditionallyAsync.
        if (validation.ConditionalApproval is { } conditional)
        {
            detail.TaxApprovalStatus = "CONDITIONAL";
            detail.ConditionalApproval = new PayrollRunConditionalApprovalResponse
            {
                Reference = conditional.Reference,
                ApprovedAt = conditional.ApprovedAt,
                ApprovedByUserId = conditional.ApprovedByUserId,
                Reason = conditional.Reason,
                AffectedEmployeeCount = conditional.AffectedEmployeeCount,
                ConfirmedTaxTotal = conditional.ConfirmedTaxTotal,
                ProvisionalTaxTotal = conditional.ProvisionalTaxTotal,
                PolicyVersion = conditional.PolicyVersion,
                MissingTaxMonths = conditional.MissingTaxMonths,
                FollowUpRequired = conditional.FollowUpRequired,
                ReconciliationStatus = conditional.ReconciliationStatus
            };
        }

        if (string.Equals(run.RunStatus, PayrollRunStatuses.Submitted, StringComparison.OrdinalIgnoreCase))
        {
            detail.ApproveConditionallyBlockReasons = BuildConditionalApprovalReasons(
                run, validation, taxReview, employees, detail.NeedsRecalculation, detail.RecalculationReasons);
            detail.CanApproveConditionally = detail.ApproveConditionallyBlockReasons.Count == 0;
        }

        detail.CanApproveFinal = string.Equals(run.RunStatus, PayrollRunStatuses.Submitted, StringComparison.OrdinalIgnoreCase)
            && detail.DecisionBlockReasons.Count == 0 && !detail.IsTaxProvisional;
        ApplyApprovalNextStep(run, detail, validation, taxReview);
        detail.AvailableActions = BuildAvailableActions(run, detail);
        return detail;
    }

    // ------------------------------------------------------------------------------------
    // PAY-BE-FIX-030: PPh sementara (review-only). Estimasi TIDAK PERNAH menjadi nominal PPh resmi.
    // ------------------------------------------------------------------------------------

    private const string TaxReviewNotApplicable = "NotApplicable";
    private const string TaxReviewRecalculationRequired = "RecalculationRequired";
    private const string TaxReviewBlocked = "Blocked";
    private const string TaxReviewEligible = "Eligible";

    private const string TaxReviewRecalculationMessage =
        "Payroll Run dihitung sebelum fitur PPh sementara tersedia. Hitung ulang Payroll Run untuk menyiapkan estimasi PPh dan pengajuan review.";

    /// <summary>Hasil penilaian kelayakan review-only: satu-satunya sumber untuk GET detail, availableActions, dan SubmitForReviewAsync.</summary>
    private sealed record TaxReviewEvaluation(string State, int OtherBlockerCount, List<string> AffectedEmployees);

    /// <summary>
    /// PAY-BE-FIX-031: menilai kelayakan pengajuan review-only dari data terstruktur, tanpa pencocokan teks pesan:
    /// - snapshot pajak penerima: <c>IsBlocking</c> + <c>BlockCode</c> TAX_YTD_HISTORY_INCOMPLETE (hasil mesin pajak saat hitung);
    /// - metadata klasifikasi run (<see cref="TaxReviewMeta"/>): jumlah penahan lain, ditulis saat hitung dari kode yang sama.
    /// Snapshot lama (blocking tanpa BlockCode, tetapi dengan bukti terstruktur masa pajak terakhir dan bulan histori yang hilang) atau run
    /// tanpa metadata dinilai RecalculationRequired: tidak ditebak dan tidak dianggap lolos. Penahan lain membuat state Blocked.
    /// </summary>
    private static TaxReviewEvaluation EvaluateTaxReview(ValidationSummary validation, IReadOnlyCollection<PrlPayrollRunEmployee> employees)
    {
        var classified = new List<string>();
        var legacy = new List<string>();
        foreach (var employee in employees)
        {
            var snapshot = ReadSnapshot<PayrollTaxSnapshot>(employee.TaxSnapshotJson);
            if (IsTaxHistoryBlocked(snapshot)) classified.Add(employee.EmployeeNameSnapshot);
            else if (snapshot is { IsBlocking: true, BlockCode: null, IsLastTaxPeriod: true } && MissingMonthsOf(snapshot).Count > 0)
                legacy.Add(employee.EmployeeNameSnapshot);
        }

        if (classified.Count == 0 && legacy.Count == 0) return new TaxReviewEvaluation(TaxReviewNotApplicable, 0, new List<string>());
        if (legacy.Count > 0 || validation.TaxReview is null)
            return new TaxReviewEvaluation(TaxReviewRecalculationRequired, 0, legacy.Concat(classified).Distinct().ToList());
        if (validation.TaxReview.OtherBlockerCount > 0)
            return new TaxReviewEvaluation(TaxReviewBlocked, validation.TaxReview.OtherBlockerCount, classified);
        return new TaxReviewEvaluation(TaxReviewEligible, 0, classified);
    }

    /// <summary>Alasan spesifik per penerima terdampak yang estimasinya tidak tersedia (data/kebijakan yang kurang), bukan pesan generik.</summary>
    private static List<string> UnavailableEstimateReasons(IEnumerable<PrlPayrollRunEmployee> employees)
    {
        var reasons = new List<string>();
        foreach (var employee in employees)
        {
            var provisional = ProvisionalTaxOf(employee, ReadSnapshot<PayrollTaxSnapshot>(employee.TaxSnapshotJson));
            if (provisional is { IsAvailable: false })
                reasons.Add($"{employee.EmployeeNameSnapshot}: estimasi PPh sementara tidak tersedia. {provisional.UnavailableReason}".TrimEnd());
        }

        return reasons;
    }

    /// <summary>
    /// PAY-BE-FIX-032: status PPh satu penerima dari snapshot hasil hitung. Tidak ada snapshot atau hasil tidak sah = INCOMPLETE_NO_ESTIMATE
    /// (BUKAN Rp0). Rp0 hanya FINAL_VALID bila hasilnya VALID_ZERO.
    /// </summary>
    private static string ClassifyEmployeeTaxStatus(PayrollTaxSnapshot? snapshot)
    {
        if (snapshot is null) return PayrollEmployeeTaxStatuses.IncompleteNoEstimate;
        if (IsTaxHistoryBlocked(snapshot))
            return snapshot.Provisional is { IsAvailable: true, Amount: not null }
                ? PayrollEmployeeTaxStatuses.ProvisionalEstimate
                : PayrollEmployeeTaxStatuses.IncompleteNoEstimate;
        if (snapshot.IsBlocking) return PayrollEmployeeTaxStatuses.IncompleteNoEstimate;
        return snapshot.ResultStatus switch
        {
            PayrollComponentResultStatuses.Exempt or PayrollComponentResultStatuses.NotApplicable => PayrollEmployeeTaxStatuses.NotApplicable,
            PayrollComponentResultStatuses.Calculated or PayrollComponentResultStatuses.ValidZero => PayrollEmployeeTaxStatuses.FinalValid,
            _ => PayrollEmployeeTaxStatuses.IncompleteNoEstimate
        };
    }

    /// <summary>PPh resmi penerima bila FINAL_VALID (termasuk 0 yang sah); null selain itu.</summary>
    private static decimal? OfficialTaxOf(PayrollTaxSnapshot? snapshot) =>
        snapshot is not null && ClassifyEmployeeTaxStatus(snapshot) == PayrollEmployeeTaxStatuses.FinalValid
            ? snapshot.EmployeeAmount + snapshot.EmployerBorneAmount
            : null;

    /// <summary>
    /// PAY-BE-FIX-032: alasan run belum dapat disahkan bersyarat; kosong = layak. Hanya run Submitted yang diajukan review-only, dengan
    /// penilaian terstruktur Eligible (satu-satunya penahan histori PPh), tanpa pegawai bermasalah, sumber kompensasi masih sah, dan estimasi
    /// tersedia bagi seluruh penerima terdampak. Tanpa pencocokan teks pesan.
    /// </summary>
    private static List<string> BuildConditionalApprovalReasons(
        PrlPayrollRun run, ValidationSummary validation, TaxReviewEvaluation taxReview, IReadOnlyCollection<PrlPayrollRunEmployee> employees,
        bool needsRecalculation, IReadOnlyCollection<string> recalculationReasons)
    {
        var reasons = new List<string>();
        if (!string.Equals(run.RunStatus, PayrollRunStatuses.Submitted, StringComparison.OrdinalIgnoreCase))
        {
            reasons.Add($"Hanya payroll run berstatus Submitted yang dapat disahkan bersyarat (saat ini {run.RunStatus}).");
            return reasons;
        }

        if (validation.ConditionalApproval is not null)
        {
            reasons.Add("Payroll run sudah disahkan bersyarat.");
            return reasons;
        }

        if (validation.ReviewOnly is null)
            reasons.Add("Pengesahan bersyarat hanya untuk payroll run yang diajukan untuk review dengan PPh sementara.");

        switch (taxReview.State)
        {
            case TaxReviewNotApplicable:
                reasons.Add("Payroll run tidak memiliki penahan histori PPh; gunakan pengesahan normal.");
                break;
            case TaxReviewRecalculationRequired:
                reasons.Add(TaxReviewRecalculationMessage);
                break;
            case TaxReviewBlocked:
                reasons.Add($"Terdapat {taxReview.OtherBlockerCount} penahan payroll selain histori PPh yang belum lengkap; selesaikan lebih dulu.");
                break;
            case TaxReviewEligible:
                reasons.AddRange(UnavailableEstimateReasons(employees));
                break;
        }

        if (run.ErrorEmployeeCount > 0) reasons.Add($"{run.ErrorEmployeeCount} penerima memiliki hasil perhitungan bermasalah.");
        if (run.ProcessedEmployeeCount <= 0) reasons.Add("Belum ada penerima yang terhitung.");
        if (needsRecalculation)
            reasons.Add("Data sumber berubah setelah perhitungan terakhir (" + string.Join("; ", recalculationReasons) + "); hitung ulang payroll run.");
        return reasons;
    }

    private static bool IsTaxHistoryBlocked(PayrollTaxSnapshot? snapshot) =>
        snapshot is { IsBlocking: true, BlockCode: PayrollTaxBlockCodes.YtdHistoryIncomplete };

    /// <summary>Teks penahan (berawalan nama penerima, sama dengan daftar Blockers) yang diklasifikasikan histori PPh belum lengkap.</summary>
    private static List<string> TaxHistoryBlockerTexts(IEnumerable<PrlPayrollRunEmployee> employees)
    {
        var texts = new List<string>();
        foreach (var employee in employees)
        {
            var snapshot = ReadSnapshot<PayrollTaxSnapshot>(employee.TaxSnapshotJson);
            if (!IsTaxHistoryBlocked(snapshot)) continue;
            texts.AddRange(snapshot!.Issues.Select(issue => $"{employee.EmployeeNameSnapshot}: {issue}"));
        }

        return texts;
    }

    private static List<int> ReadMonths(object? value)
    {
        var months = new List<int>();
        if (value is JsonElement { ValueKind: JsonValueKind.Array } array)
        {
            foreach (var item in array.EnumerateArray())
                if (item.ValueKind == JsonValueKind.Number && item.TryGetInt32(out var month)) months.Add(month);
        }
        else if (value is IEnumerable<int> list)
        {
            months.AddRange(list);
        }

        return months.Distinct().OrderBy(m => m).ToList();
    }

    private static List<int> MissingMonthsOf(PayrollTaxSnapshot snapshot) =>
        snapshot.Detail is not null && snapshot.Detail.TryGetValue("ytdMissingMonths", out var value) ? ReadMonths(value) : new List<int>();

    /// <summary>PPh sementara satu penerima; null bila penahannya bukan histori PPh belum lengkap.</summary>
    private static PayrollRunProvisionalTaxResponse? ProvisionalTaxOf(PrlPayrollRunEmployee employee, PayrollTaxSnapshot? snapshot)
    {
        if (!IsTaxHistoryBlocked(snapshot)) return null;
        var estimate = snapshot!.Provisional;
        var months = MissingMonthsOf(snapshot);
        if (estimate is null || !estimate.IsAvailable || !estimate.Amount.HasValue)
        {
            return new PayrollRunProvisionalTaxResponse
            {
                IsAvailable = false,
                OfficialNetPay = employee.NetPay,
                Method = estimate?.Method ?? PayrollProvisionalTaxEstimate.MethodTerMonthlyProxy,
                Treatment = estimate?.Treatment ?? TaxTreatments.None,
                PolicyVersion = estimate?.PolicyVersion ?? snapshot.PolicyVersion,
                PolicyLegalBasis = estimate?.PolicyLegalBasis ?? snapshot.PolicyLegalBasis,
                MissingTaxMonths = months,
                TaxFinalStatus = PayrollTaxFinality.NotFinal,
                Explanation = PayrollProvisionalTaxEstimate.ExplanationText,
                UnavailableReason = estimate?.UnavailableReason
                    ?? "Run dihitung sebelum estimasi PPh sementara tersedia; hitung ulang payroll run."
            };
        }

        // Gaji bersih resmi dihitung dengan PPh resmi = 0 (penahan). Estimasi hanya mengurangkan PPh sementara bila dipotong dari pegawai.
        var deduct = estimate.Treatment == TaxTreatments.EmployeeDeduction ? estimate.Amount.Value : 0m;
        return new PayrollRunProvisionalTaxResponse
        {
            IsAvailable = true,
            ProvisionalTaxAmount = estimate.Amount,
            EstimatedNetPay = employee.NetPay - deduct,
            OfficialNetPay = employee.NetPay,
            Method = estimate.Method,
            Treatment = estimate.Treatment,
            PolicyVersion = estimate.PolicyVersion,
            PolicyLegalBasis = estimate.PolicyLegalBasis,
            BasisGross = estimate.BasisGross,
            TerCategory = estimate.TerCategory,
            TerRatePercent = estimate.TerRatePercent,
            NoNpwpSurchargePercent = estimate.NoNpwpSurchargePercent,
            MissingTaxMonths = months,
            TaxFinalStatus = PayrollTaxFinality.NotFinal,
            Explanation = estimate.Explanation
        };
    }

    private static PayrollRunProvisionalTaxSummaryResponse? BuildProvisionalSummary(IEnumerable<PrlPayrollRunEmployee> employees)
    {
        var rows = employees
            .Select(e => (Employee: e, Snapshot: ReadSnapshot<PayrollTaxSnapshot>(e.TaxSnapshotJson)))
            .Where(x => IsTaxHistoryBlocked(x.Snapshot))
            .ToList();
        if (rows.Count == 0) return null;

        var items = rows.Select(x => ProvisionalTaxOf(x.Employee, x.Snapshot)!).ToList();
        var available = items.Where(x => x.IsAvailable).ToList();
        return new PayrollRunProvisionalTaxSummaryResponse
        {
            BlockCode = PayrollTaxBlockCodes.YtdHistoryIncomplete,
            AffectedEmployeeCount = rows.Count,
            TaxBlockerCount = rows.Sum(x => x.Snapshot!.Issues.Count),
            EstimateAvailableCount = available.Count,
            EstimateUnavailableCount = items.Count - available.Count,
            TotalProvisionalTax = available.Sum(x => x.ProvisionalTaxAmount ?? 0m),
            TotalEstimatedNetPay = available.Sum(x => x.EstimatedNetPay ?? 0m),
            Method = PayrollProvisionalTaxEstimate.MethodTerMonthlyProxy,
            PolicyVersions = items.Select(x => x.PolicyVersion).Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x!).Distinct().ToList(),
            MissingTaxMonths = items.SelectMany(x => x.MissingTaxMonths).Distinct().OrderBy(m => m).ToList(),
            TaxFinalStatus = PayrollTaxFinality.NotFinal,
            Explanation = PayrollProvisionalTaxEstimate.ExplanationText,
            NetPayNotice = "Gaji bersih pada penerima terdampak masih ESTIMASI selama PPh 21 belum final; nominal resmi baru ada setelah histori PPh dilengkapi dan payroll dihitung ulang."
        };
    }

    /// <summary>
    /// Alasan run belum dapat diajukan (Calculated) atau disahkan (Submitted). Mencerminkan syarat yang diperiksa ulang oleh
    /// <see cref="SubmitAsync"/> dan <see cref="ApproveAsync"/> (penahan, pegawai bermasalah, belum ada yang terhitung, sumber kompensasi
    /// berubah, dan, khusus pengajuan, penghasilan variabel). Murni baca; tidak mengubah status, nominal, ataupun hasil pajak.
    /// </summary>
    private async Task<List<string>> BuildDecisionBlockReasonsAsync(
        PrlPayrollRun run, PayrollRunDetailResponse detail, TaxReviewEvaluation taxReview, bool forReview, CancellationToken cancellationToken)
    {
        var reasons = new List<string>();
        var calculated = string.Equals(run.RunStatus, PayrollRunStatuses.Calculated, StringComparison.OrdinalIgnoreCase);
        var submitted = string.Equals(run.RunStatus, PayrollRunStatuses.Submitted, StringComparison.OrdinalIgnoreCase);
        if (!calculated && !submitted) return reasons;

        // PAY-BE-FIX-030: run yang diajukan dengan PPh sementara tidak boleh disahkan sampai dikembalikan, dihitung ulang, dan diajukan ulang.
        if (submitted && detail.IsTaxProvisional) reasons.Add(TaxNotFinalApproveMessage);

        // Mode review hanya mengabaikan penahan yang terklasifikasi histori PPh belum lengkap (kode terstruktur + metadata hitung);
        // penahan lain tetap menahan, dan run yang tidak dapat dinilai (snapshot lama) wajib dihitung ulang.
        if (forReview)
        {
            if (taxReview.State == TaxReviewRecalculationRequired)
            {
                var names = taxReview.AffectedEmployees.Count > 0 ? $" Penerima terdampak: {string.Join(", ", taxReview.AffectedEmployees)}." : string.Empty;
                reasons.Add(TaxReviewRecalculationMessage + names);
            }
            else if (taxReview.State == TaxReviewBlocked)
            {
                reasons.Add($"Terdapat {taxReview.OtherBlockerCount} penahan payroll selain histori PPh yang belum lengkap; selesaikan lebih dulu.");
            }
        }
        else if (detail.Blockers.Count > 0)
        {
            reasons.Add($"Terdapat {detail.Blockers.Count} penahan payroll (termasuk perhitungan PPh yang belum valid bila ada); lihat daftar penahan.");
        }
        AddRunQualityReasons(reasons, run, detail);

        // Penghasilan variabel hanya menahan pengajuan; pengesahan tidak memeriksanya lagi (lihat ApproveAsync).
        if (calculated)
        {
            var pending = await ManualInputsOfRun(run.Id)
                .CountAsync(x => x.InputStatus == PayrollManualInputStatuses.Submitted, cancellationToken);
            if (pending > 0)
                reasons.Add($"Masih ada {pending} penghasilan variabel yang menunggu keputusan; setujui atau tolak lebih dulu.");

            if (run.CalculatedAt.HasValue)
            {
                var calculatedAt = run.CalculatedAt.Value;
                var changedAfterCalculation = await ManualInputsOfRun(run.Id)
                    .AnyAsync(x => (x.UpdateDateTime ?? x.CreateDateTime) > calculatedAt, cancellationToken);
                if (changedAfterCalculation)
                    reasons.Add("Penghasilan variabel berubah setelah perhitungan terakhir; hitung ulang payroll run.");
            }
        }

        return reasons;
    }

    /// <summary>Alasan kualitas data run yang BUKAN histori PPh: penerima bermasalah, belum terhitung, sumber kompensasi berubah. Satu sumber untuk pengesahan normal dan lanjutan.</summary>
    private static void AddRunQualityReasons(List<string> reasons, PrlPayrollRun run, PayrollRunDetailResponse detail)
    {
        if (run.ErrorEmployeeCount > 0)
            reasons.Add($"{run.ErrorEmployeeCount} penerima memiliki hasil perhitungan bermasalah.");
        if (run.ProcessedEmployeeCount <= 0)
            reasons.Add("Belum ada penerima yang terhitung.");
        if (detail.NeedsRecalculation)
            reasons.Add("Data sumber berubah setelah perhitungan terakhir (" + string.Join("; ", detail.RecalculationReasons) + "); hitung ulang payroll run.");
    }

    private static readonly string[] TaxResolutionSteps =
    {
        "Lengkapi Saldo Awal PPh (Januari-November) sampai disetujui, ATAU tetapkan nominal PPh manual yang telah dihitung dari dokumen yang sah dan disetujui pada profil pajak pegawai (metode Nominal Manual Disetujui).",
        "Kembalikan Payroll Run ke tahap hitung (riwayat pengajuan dan pengesahan bersyarat tetap tersimpan).",
        "Hitung ulang Payroll Run agar PPh dihitung dari data yang sah; estimasi tidak pernah menjadi PPh resmi.",
        "Ajukan kembali secara normal, lalu sahkan. Pengaju dan pengesah boleh pengguna yang sama selama keduanya memegang izin Ajukan dan Sahkan.",
    };

    /// <summary>
    /// PAY-BE-FIX-033: langkah berikutnya menuju pengesahan FINAL untuk run Submitted. Murni informasi (tanpa mengubah data): endpoint
    /// approve tetap memeriksa EnsureTaxFinalAsync, EnsureReadyToDecide, dan sumber kompensasi. Run yang sudah disahkan bersyarat tidak
    /// pernah ditawarkan pengesahan bersyarat kedua; ia mendapat aksi "ContinueApproval" (panduan penyelesaian), bukan Approve.
    /// </summary>
    private static void ApplyApprovalNextStep(
        PrlPayrollRun run, PayrollRunDetailResponse detail, ValidationSummary validation, TaxReviewEvaluation taxReview)
    {
        detail.CanContinueApproval = false;
        detail.ApprovalNextStep = PayrollRunApprovalNextSteps.None;
        detail.TaxResolutionRequired = false;
        detail.ContinueApprovalBlockReasons = new List<string>();
        detail.ContinueApprovalSteps = new List<string>();
        if (!string.Equals(run.RunStatus, PayrollRunStatuses.Submitted, StringComparison.OrdinalIgnoreCase)) return;

        detail.TaxResolutionRequired = detail.IsTaxProvisional;
        if (detail.CanApproveFinal)
        {
            detail.ApprovalNextStep = PayrollRunApprovalNextSteps.ApproveFinal;
            return;
        }

        detail.ContinueApprovalBlockReasons = detail.DecisionBlockReasons.ToList();
        if (detail.IsTaxProvisional)
        {
            // PAY-BE-FIX-034: histori PPh yang belum lengkap adalah TINDAK LANJUT finalisasi, bukan penahan persetujuan administratif.
            // Penahan nyata disusun dari data terstruktur (klasifikasi hitung + kualitas run), bukan dari pencocokan teks pesan.
            var realBlockers = new List<string>();
            if (taxReview.State == TaxReviewRecalculationRequired) realBlockers.Add(TaxReviewRecalculationMessage);
            else if (taxReview.State == TaxReviewBlocked)
                realBlockers.Add($"Terdapat {taxReview.OtherBlockerCount} penahan payroll selain histori PPh yang belum lengkap; selesaikan lebih dulu.");
            AddRunQualityReasons(realBlockers, run, detail);
            detail.ContinueApprovalBlockReasons = realBlockers;

            if (validation.ConditionalApproval is not null)
            {
                detail.CanContinueApproval = true;
                detail.ApprovalNextStep = realBlockers.Count > 0
                    ? PayrollRunApprovalNextSteps.ResolveOtherBlockers
                    : PayrollRunApprovalNextSteps.ResolveTaxData;
            }
            else
            {
                detail.ApprovalNextStep = detail.CanApproveConditionally
                    ? PayrollRunApprovalNextSteps.ApproveConditionally
                    : realBlockers.Count > 0 ? PayrollRunApprovalNextSteps.ResolveOtherBlockers : PayrollRunApprovalNextSteps.ResolveTaxData;
            }

            detail.ContinueApprovalSteps = TaxResolutionSteps.ToList();
            return;
        }

        detail.ApprovalNextStep = PayrollRunApprovalNextSteps.ResolveOtherBlockers;
    }

    /// <summary>
    /// Aksi yang boleh dijalankan menurut status run. Hanya bantuan tampilan: setiap endpoint aksi memeriksa ulang status dan
    /// kewenangan di backend. Izin Akses Role per aksi diperiksa terpisah oleh [AccessPermission].
    /// </summary>
    private static List<string> BuildAvailableActions(PrlPayrollRun run, PayrollRunDetailResponse detail)
    {
        var actions = new List<string>();
        var status = run.RunStatus;
        // Sama dengan syarat SubmitAsync/ApproveAsync: tidak ada alasan menahan (penahan, pegawai bermasalah, belum terhitung, sumber
        // kompensasi berubah, penghasilan variabel belum beres). Hanya ditetapkan untuk status Calculated dan Submitted.
        var ready = detail.DecisionBlockReasons.Count == 0;

        if (!run.IsLocked && PayrollRunStatuses.AcceptsInputs(status))
        {
            actions.Add("ImportHrd");
            actions.Add("Calculate");
        }
        if (string.Equals(status, PayrollRunStatuses.Calculated, StringComparison.OrdinalIgnoreCase) && ready) actions.Add("Submit");
        // PAY-BE-FIX-030: pengajuan review-only hanya ditawarkan bila backend menyatakan layak; tidak pernah bersamaan dengan Submit normal.
        if (string.Equals(status, PayrollRunStatuses.Calculated, StringComparison.OrdinalIgnoreCase) && !ready && detail.CanSubmitForReview)
            actions.Add("SubmitForReview");
        if (string.Equals(status, PayrollRunStatuses.Submitted, StringComparison.OrdinalIgnoreCase))
        {
            actions.Add("Return");
            // Approve tidak pernah ditawarkan selama PPh sementara (izin Approve dan kesiapan bisnis adalah dua syarat terpisah).
            if (ready && !detail.IsTaxProvisional) actions.Add("Approve");
            // PAY-BE-FIX-032: pengesahan bersyarat (administratif) hanya bila backend menyatakan layak; bukan pengganti Approve.
            if (detail.CanApproveConditionally) actions.Add("ApproveConditionally");
            // PAY-BE-FIX-033: run yang sudah disahkan bersyarat mendapat jalur lanjutan (panduan penyelesaian). BUKAN Approve dan BUKAN
            // pengesahan bersyarat kedua; tidak mengubah data apa pun.
            if (detail.CanContinueApproval) actions.Add("ContinueApproval");
        }
        if (!run.IsLocked && !PayrollRunStatuses.IsFinalized(status)
            && !string.Equals(status, PayrollRunStatuses.Cancelled, StringComparison.OrdinalIgnoreCase)) actions.Add("Cancel");
        if (PayrollRunStatuses.AllowsFinanceHandoff(status) && !detail.IsFinanceHandoffHeld) actions.Add("FinanceHandoff");
        if (PayrollRunStatuses.AllowsDeductionReversal(status)) actions.Add("RequestReversal");
        if (PayrollRunStatuses.AllowsPayslipGeneration(status)) actions.Add("GeneratePayslips");
        if (string.Equals(status, PayrollRunStatuses.Paid, StringComparison.OrdinalIgnoreCase)) actions.Add("Close");
        return actions;
    }

    private static string? ReadPolicyVersion(string? policySnapshotJson)
    {
        if (string.IsNullOrWhiteSpace(policySnapshotJson)) return null;
        try
        {
            using var document = JsonDocument.Parse(policySnapshotJson);
            return document.RootElement.TryGetProperty("policyVersion", out var value) && value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>Ringkasan jumlah run per status. Hanya jumlah; tidak ada nominal gaji.</summary>
    public async Task<PayrollRunSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var rows = await _dbContext.PrlPayrollRuns.AsNoTracking()
            .Where(x => !x.IsDelete)
            .GroupBy(x => x.RunStatus)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);
        var byStatus = rows.ToDictionary(r => r.Status, r => r.Count);
        int Count(params string[] statuses) => statuses.Sum(s => byStatus.GetValueOrDefault(s));

        return new PayrollRunSummaryResponse
        {
            TotalRuns = rows.Sum(r => r.Count),
            AwaitingActionCount = Count(PayrollRunStatuses.Draft, PayrollRunStatuses.CollectingInput,
                PayrollRunStatuses.Calculated, PayrollRunStatuses.Submitted),
            InPaymentCount = Count(PayrollRunStatuses.SentToFinance, PayrollRunStatuses.PartiallyPaid),
            CountByStatus = byStatus
        };
    }

    public static PayrollRunFilterMetadataResponse GetFilterMetadata() => new()
    {
        Statuses = new List<string>
        {
            PayrollRunStatuses.Draft, PayrollRunStatuses.CollectingInput, PayrollRunStatuses.Calculated, PayrollRunStatuses.Submitted,
            PayrollRunStatuses.Approved, PayrollRunStatuses.SentToFinance, PayrollRunStatuses.PartiallyPaid, PayrollRunStatuses.Paid,
            PayrollRunStatuses.Closed, PayrollRunStatuses.Cancelled, PayrollRunStatuses.Reversed
        },
        RunTypes = new List<string> { RegularRunType },
        DefaultPageSize = 20,
        PageSizeOptions = new List<int> { 10, 20, 50, 100 }
    };

    /// <summary>Periode bulanan aktif terbaru, untuk memilih periode pada layar Payroll Run.</summary>
    public async Task<List<PayrollRunPeriodOptionResponse>> GetPeriodOptionsAsync(CancellationToken cancellationToken)
    {
        var periods = await _dbContext.MstPayrollPeriods.AsNoTracking()
            .Where(x => !x.IsDelete && x.PeriodType == "Monthly")
            .OrderByDescending(x => x.FiscalYear).ThenByDescending(x => x.PeriodNumber)
            .Take(36)
            .ToListAsync(cancellationToken);
        return periods.Select(x => new PayrollRunPeriodOptionResponse
        {
            Id = x.Id,
            Code = x.PayrollPeriodCode,
            Name = x.PayrollPeriodName,
            DeductionPeriod = $"{x.FiscalYear:D4}-{x.PeriodNumber:D2}",
            IsLocked = x.IsLocked,
            IsActive = x.IsActive
        }).ToList();
    }

    /// <summary>
    /// Ringkasan satu periode untuk HRD: karyawan yang punya angsuran pada periode itu, jumlah sesuai perjanjian Finance, dan status
    /// pemrosesannya. Tanpa payroll run, periode berstatus "Menunggu Payroll Run" (tidak ada run palsu dari Finance).
    /// </summary>
    public async Task<PayrollRunPeriodOverviewResponse> GetPeriodOverviewAsync(Guid payrollPeriodId, CancellationToken cancellationToken)
    {
        var period = await _dbContext.MstPayrollPeriods.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == payrollPeriodId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Periode payroll tidak ditemukan.");
        var code = $"{period.FiscalYear:D4}-{period.PeriodNumber:D2}";

        var cutover = await _dbContext.MstPayrollDeductionCutovers.AsNoTracking()
            .FirstOrDefaultAsync(x => x.IsActive && !x.IsDelete, cancellationToken);
        var isAutomatic = cutover is null || string.CompareOrdinal(code, cutover.AutomaticFromPeriod) >= 0;

        var run = await _dbContext.PrlPayrollRuns.AsNoTracking()
            .Where(x => !x.IsDelete && x.PayrollPeriodId == period.Id && x.RunType == RegularRunType
                && x.RunStatus != PayrollRunStatuses.Cancelled && x.RunStatus != PayrollRunStatuses.Reversed)
            .OrderByDescending(x => x.CreateDateTime)
            .FirstOrDefaultAsync(cancellationToken);

        var overview = new PayrollRunPeriodOverviewResponse
        {
            PayrollPeriodId = period.Id,
            PeriodCode = period.PayrollPeriodCode,
            PeriodName = period.PayrollPeriodName,
            DeductionPeriod = code,
            IsLocked = period.IsLocked,
            AutomaticFromPeriod = cutover?.AutomaticFromPeriod,
            IsAutomaticPeriod = isAutomatic,
            RunId = run?.Id,
            RunNumber = run?.RunNumber,
            RunStatus = run?.RunStatus,
            PeriodStatus = !isAutomatic
                ? PayrollPeriodProcessStatuses.ManualFinance
                : run?.RunStatus ?? PayrollPeriodProcessStatuses.AwaitingPayrollRun
        };

        var installments = await _dbContext.FinReceivableInstallments.AsNoTracking()
            .Include(x => x.Plan)
            .Where(x => !x.IsDelete && x.DeductionPeriod == code
                && x.Status != FinReceivableInstallmentStatuses.Dibatalkan
                && x.Plan != null && !x.Plan.IsDelete
                && (x.Plan.Status == FinReceivableInstallmentPlanStatuses.Disetujui
                    || x.Plan.Status == FinReceivableInstallmentPlanStatuses.Selesai))
            .OrderBy(x => x.Plan!.PlanNumber).ThenBy(x => x.InstallmentNumber)
            .ToListAsync(cancellationToken);
        if (installments.Count == 0) return overview;

        var receivableIds = installments.Select(x => x.Plan!.ReceivableId).Distinct().ToList();
        var receivables = await _dbContext.FinReceivables.AsNoTracking()
            .Where(r => receivableIds.Contains(r.Id))
            .Select(r => new { r.Id, r.ReceivableNumber, r.BenefitOwnerId, r.BenefitOwnerWorkforceProfileId })
            .ToDictionaryAsync(r => r.Id, cancellationToken);

        var ownerIds = receivables.Values.Where(r => r.BenefitOwnerId.HasValue).Select(r => r.BenefitOwnerId!.Value).Distinct().ToList();
        var workforceIds = receivables.Values.Where(r => r.BenefitOwnerWorkforceProfileId.HasValue)
            .Select(r => r.BenefitOwnerWorkforceProfileId!.Value).Distinct().ToList();
        var employees = await _dbContext.MstEmployees.AsNoTracking()
            .Where(x => !x.IsDelete && (ownerIds.Contains(x.Id) || workforceIds.Contains(x.WorkforceProfileId)))
            .Select(x => new { x.Id, x.WorkforceProfileId, x.EmployeeNumber, x.FullName })
            .ToListAsync(cancellationToken);
        var doctors = await _dbContext.MstDoctors.AsNoTracking()
            .Where(x => !x.IsDelete && workforceIds.Contains(x.WorkforceProfileId))
            .Select(x => new { x.WorkforceProfileId, x.DoctorNumber, x.FullName })
            .ToListAsync(cancellationToken);

        // Rencana/hasil potongan dibaca dari baris pegawai pada run (bila ada); tidak dihitung ulang di sini.
        var lines = new Dictionary<Guid, PayrollRunInstallmentLineResponse>();
        if (run is not null)
        {
            var runEmployees = await _dbContext.PrlPayrollRunEmployees.AsNoTracking()
                .Include(x => x.Components).Include(x => x.VariableInputs)
                .Where(x => x.PayrollRunId == run.Id && !x.IsDelete)
                .ToListAsync(cancellationToken);
            foreach (var runEmployee in runEmployees)
            {
                var line = PayrollInstallmentDeductionService.ReadLine(runEmployee);
                if (line is not null) lines[line.InstallmentId] = line;
            }
        }

        foreach (var installment in installments)
        {
            receivables.TryGetValue(installment.Plan!.ReceivableId, out var receivable);
            string name = "Pemilik belum teridentifikasi", number = string.Empty;
            var employee = employees.FirstOrDefault(e =>
                (receivable?.BenefitOwnerId.HasValue == true && e.Id == receivable.BenefitOwnerId!.Value)
                || (receivable?.BenefitOwnerWorkforceProfileId.HasValue == true && e.WorkforceProfileId == receivable.BenefitOwnerWorkforceProfileId!.Value));
            if (employee is not null) { name = employee.FullName; number = employee.EmployeeNumber; }
            else if (receivable?.BenefitOwnerWorkforceProfileId.HasValue == true)
            {
                var doctor = doctors.FirstOrDefault(d => d.WorkforceProfileId == receivable.BenefitOwnerWorkforceProfileId!.Value);
                if (doctor is not null) { name = doctor.FullName; number = doctor.DoctorNumber; }
            }

            lines.TryGetValue(installment.Id, out var line);
            string process;
            if (!isAutomatic) process = PayrollInstallmentProcessStatuses.ManualFinance;
            else if (run is null) process = PayrollInstallmentProcessStatuses.AwaitingPayrollRun;
            else if (line is null) process = PayrollInstallmentProcessStatuses.NotInRun;
            else if (line.IsConfirmedToFinance) process = PayrollInstallmentProcessStatuses.Confirmed;
            else if (!line.IsCalculated) process = PayrollInstallmentProcessStatuses.NotCalculated;
            else process = PayrollInstallmentProcessStatuses.Planned;

            overview.Items.Add(new PayrollPeriodInstallmentResponse
            {
                InstallmentId = installment.Id,
                PlanNumber = installment.Plan.PlanNumber,
                ReceivableNumber = receivable?.ReceivableNumber ?? string.Empty,
                EmployeeName = name,
                EmployeeNumber = number,
                InstallmentNumber = installment.InstallmentNumber,
                ScheduledAmount = installment.ScheduledAmount,
                CarriedOverAmount = installment.CarriedOverAmount,
                DueAmount = installment.ScheduledAmount + installment.CarriedOverAmount,
                PaidAmount = installment.PaidAmount,
                InstallmentStatus = installment.Status,
                PlannedDeduction = line?.PlannedDeduction,
                PlannedStatus = line?.PlannedStatus,
                ProcessStatus = process
            });
        }

        overview.EmployeeCount = overview.Items.Select(x => x.EmployeeNumber + x.EmployeeName).Distinct().Count();
        overview.InstallmentCount = overview.Items.Count;
        overview.TotalDueAmount = overview.Items.Sum(x => x.DueAmount);
        overview.TotalPlannedDeduction = overview.Items.Sum(x => x.PlannedDeduction ?? 0m);
        return overview;
    }

    // ------------------------------------------------------------------------------------
    // Buka run
    // ------------------------------------------------------------------------------------

    public async Task<PayrollRunDetailResponse> CreateAsync(CreatePayrollRunRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var period = await _dbContext.MstPayrollPeriods.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == request.PayrollPeriodId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Periode payroll tidak ditemukan.");
        if (!period.IsActive) throw new PayrollRunValidationException("Periode payroll tidak aktif.");
        if (period.IsLocked) throw new PayrollRunValidationException("Periode payroll sudah terkunci.");

        var runId = await InTransactionAsync($"HR_PAYROLL_RUN_PERIOD_{period.Id:N}", async () =>
        {
            var exists = await _dbContext.PrlPayrollRuns.AnyAsync(x => !x.IsDelete
                && x.PayrollPeriodId == period.Id && x.RunType == RegularRunType
                && x.RunStatus != PayrollRunStatuses.Cancelled && x.RunStatus != PayrollRunStatuses.Reversed, cancellationToken);
            if (exists) throw new PayrollRunConflictException("Sudah ada payroll run Regular yang aktif untuk periode ini.");

            // Profil payroll pegawai dan dokter HRD yang layak disiapkan otomatis (tanpa impor CSV) sebelum populasi run dibentuk dari
            // profil payroll aktif. Dilakukan sebelum run ditambahkan ke konteks supaya penyimpanan di dalamnya hanya memuat profil baru.
            // Tanggal periode berkolom `date` dan terbaca Kind=Unspecified; kolom snapshot run berjenis timestamptz hanya menerima UTC.
            var periodStart = PayrollDateNormalizer.ToUtcDate(period.StartDate);
            var periodEnd = PayrollDateNormalizer.ToUtcDate(period.EndDate);
            var provision = await _provisioningService.EnsureProfilesAsync(periodStart, periodEnd, actorUserId, "PayrollRun.Create", cancellationToken);

            var run = new PrlPayrollRun
            {
                PayrollPeriodId = period.Id,
                LegalEntityId = period.LegalEntityId,
                HospitalSiteId = period.HospitalSiteId,
                RunNumber = $"PR-{period.FiscalYear}{period.PeriodNumber:D2}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}",
                RunType = RegularRunType,
                RunStatus = PayrollRunStatuses.Draft,
                CurrencyCode = "IDR",
                PeriodStartDateSnapshot = periodStart,
                PeriodEndDateSnapshot = periodEnd,
                AttendanceCutoffDateSnapshot = PayrollDateNormalizer.ToUtcDate(period.AttendanceCutoffDate),
                VariableInputCutoffDateSnapshot = PayrollDateNormalizer.ToUtcDate(period.VariableInputCutoffDate),
                PaymentDateSnapshot = PayrollDateNormalizer.ToUtcDate(period.PaymentDate),
                Notes = request.Notes,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };
            _dbContext.PrlPayrollRuns.Add(run);

            var seed = await SeedEmployeesAsync(run, period, actorUserId, cancellationToken);
            run.TotalEmployeeCount = seed.Added;

            // Baris run dan pegawai harus tersimpan dulu agar jadwal angsuran dapat ditautkan ke baris pegawai yang sah.
            await _dbContext.SaveChangesAsync(cancellationToken);

            var sync = await _syncService.SyncScheduledForRunAsync(period, actorUserId, cancellationToken);
            var notes = sync.SkipReasons.Distinct().ToList();
            if (seed.SkippedNoIdentity > 0)
                notes.Add($"{seed.SkippedNoIdentity} profil payroll aktif dilewati karena identitas pegawai/dokter tidak ditemukan.");
            AddProvisioningNotes(notes, provision);
            run.ValidationSummaryJson = JsonSerializer.Serialize(new ValidationSummary(new List<string>(), notes));

            await _dbContext.SaveChangesAsync(cancellationToken);
            await _loggerService.AuditAsync(LogCategory, "PayrollRun.Created",
                $"Payroll run {run.RunNumber} dibuka: {run.TotalEmployeeCount} pegawai, {sync.SyncedCount} angsuran ditarik dari Finance.",
                new { run.Id, run.RunNumber, PeriodId = period.Id, sync.SyncedCount, sync.SkippedCount, ActorUserId = actorUserId });
            return run.Id;
        }, cancellationToken);

        return await GetByIdAsync(runId, cancellationToken);
    }

    private sealed record PayrollSeedResult(int Added, int SkippedNoIdentity, int Repaired);

    /// <summary>
    /// Nama departemen dan jabatan penempatan organisasi untuk snapshot baris pegawai. Sumbernya sama dengan Profil Penggajian
    /// (PayrollCompensationService): penempatan yang tertaut pada profil payroll (<c>OrganizationAssignmentId</c>) lebih dulu, lalu
    /// penempatan primer aktif yang berlaku pada <paramref name="asOf"/>. Murni baca; nilai kosong bila HRD belum mengisinya.
    /// </summary>
    private async Task<Dictionary<Guid, (string? Department, string? Position)>> ResolveOrganizationNamesAsync(
        IReadOnlyDictionary<Guid, Guid?> assignmentIdByWorkforce, DateTime asOf, CancellationToken cancellationToken)
    {
        var workforceIds = assignmentIdByWorkforce.Keys.ToList();
        var linkedIds = assignmentIdByWorkforce.Values.Where(x => x.HasValue).Select(x => x!.Value).ToList();
        if (workforceIds.Count == 0) return new();

        var rows = await _dbContext.WfpOrganizationAssignments.AsNoTracking()
            .Where(x => !x.IsDelete && x.IsActive && workforceIds.Contains(x.WorkforceProfileId)
                && (linkedIds.Contains(x.Id)
                    || (x.IsPrimary && x.EffectiveStartDate <= asOf && (x.EffectiveEndDate == null || x.EffectiveEndDate >= asOf))))
            .Select(x => new
            {
                x.Id,
                x.WorkforceProfileId,
                x.EffectiveStartDate,
                DepartmentName = x.Department != null ? x.Department.DepartmentName : null,
                PositionName = x.Position != null ? x.Position.PositionName : null
            })
            .ToListAsync(cancellationToken);

        var result = new Dictionary<Guid, (string? Department, string? Position)>();
        foreach (var group in rows.GroupBy(x => x.WorkforceProfileId))
        {
            assignmentIdByWorkforce.TryGetValue(group.Key, out var linked);
            var pick = (linked.HasValue ? group.FirstOrDefault(x => x.Id == linked.Value) : null)
                ?? group.OrderByDescending(x => x.EffectiveStartDate).First();
            result[group.Key] = (pick.DepartmentName, pick.PositionName);
        }
        return result;
    }

    /// <summary>Catatan hasil penyiapan profil otomatis untuk ringkasan validasi run. Tanpa nama atau nominal.</summary>
    private static void AddProvisioningNotes(List<string> target, PayrollProvisioningResult provision)
    {
        if (provision.Created > 0)
            target.Add($"{provision.Created} profil payroll disiapkan otomatis dari data HRD. Tetapan PPh dan iuran pada profil itu masih nol (belum ditetapkan).");
        if (provision.SkippedIncompleteHrd > 0)
            target.Add($"{provision.SkippedIncompleteHrd} pegawai/dokter aktif belum masuk run karena penetapan gaji utama atau rekening payroll belum ada di HRD (lihat Kelengkapan Data Payroll).");
    }

    /// <summary>
    /// Melengkapi baris pegawai dari profil payroll aktif dan eligible yang berlaku pada periode. Idempoten: pegawai yang sudah ada
    /// pada run ini TIDAK digandakan (indeks unik run+pegawai juga menjaganya), dan baris yang dibuat jalur lain tanpa profil
    /// (mis. restitusi) dilengkapi dengan profil payroll aktifnya. Dipanggil saat membuka dan saat menghitung run, sehingga run
    /// penampung tidak pernah disahkan dengan populasi yang tidak lengkap.
    /// </summary>
    private async Task<PayrollSeedResult> SeedEmployeesAsync(PrlPayrollRun run, MstPayrollPeriod period, Guid actorUserId, CancellationToken cancellationToken)
    {
        // EffectiveStartDate/EndDate profil berkolom timestamptz: pembandingnya wajib UTC (tanggal periode terbaca Unspecified dari kolom date).
        var periodStart = PayrollDateNormalizer.ToUtcDate(period.StartDate);
        var periodEnd = PayrollDateNormalizer.ToUtcDate(period.EndDate);
        var profiles = await _dbContext.PrlPayrolls.AsNoTracking()
            .Where(x => !x.IsDelete && x.IsActive && x.IsPayrollEligible && x.PayrollStatus == "Active"
                && (x.EffectiveStartDate == null || x.EffectiveStartDate <= periodEnd)
                && (x.EffectiveEndDate == null || x.EffectiveEndDate >= periodStart))
            .OrderByDescending(x => x.EffectiveStartDate)
            .ToListAsync(cancellationToken);

        var chosen = profiles.GroupBy(x => x.WorkforceProfileId).Select(g => g.First()).ToList();
        var workforceIds = chosen.Select(x => x.WorkforceProfileId).ToList();

        var employees = await _dbContext.MstEmployees.AsNoTracking()
            .Where(x => !x.IsDelete && workforceIds.Contains(x.WorkforceProfileId))
            .Select(x => new { x.WorkforceProfileId, x.Id, x.EmployeeNumber, x.FullName })
            .ToListAsync(cancellationToken);
        var doctors = await _dbContext.MstDoctors.AsNoTracking()
            .Where(x => !x.IsDelete && workforceIds.Contains(x.WorkforceProfileId))
            .Select(x => new { x.WorkforceProfileId, x.DoctorNumber, x.FullName })
            .ToListAsync(cancellationToken);
        var employeeMap = employees.GroupBy(x => x.WorkforceProfileId).ToDictionary(g => g.Key, g => g.First());
        var doctorMap = doctors.GroupBy(x => x.WorkforceProfileId).ToDictionary(g => g.Key, g => g.First());

        var existingRows = await _dbContext.PrlPayrollRunEmployees
            .Where(x => x.PayrollRunId == run.Id && !x.IsDelete)
            .ToListAsync(cancellationToken);
        var existingByWorkforce = existingRows.GroupBy(x => x.WorkforceProfileId).ToDictionary(g => g.Key, g => g.First());

        // Snapshot Departemen/Jabatan untuk baris baru, dan untuk baris lama yang masih kosong selama belum dibekukan.
        var organizationNames = await ResolveOrganizationNamesAsync(
            chosen.ToDictionary(x => x.WorkforceProfileId, x => x.OrganizationAssignmentId), periodEnd, cancellationToken);

        var skipped = 0;
        var added = 0;
        var repaired = 0;
        foreach (var profile in chosen)
        {
            organizationNames.TryGetValue(profile.WorkforceProfileId, out var organization);
            if (existingByWorkforce.TryGetValue(profile.WorkforceProfileId, out var existingRow))
            {
                if (!existingRow.IsSnapshotFrozen)
                {
                    var filled = false;
                    if (string.IsNullOrWhiteSpace(existingRow.DepartmentNameSnapshot) && !string.IsNullOrWhiteSpace(organization.Department))
                    {
                        existingRow.DepartmentNameSnapshot = organization.Department;
                        filled = true;
                    }
                    if (string.IsNullOrWhiteSpace(existingRow.PositionNameSnapshot) && !string.IsNullOrWhiteSpace(organization.Position))
                    {
                        existingRow.PositionNameSnapshot = organization.Position;
                        filled = true;
                    }
                    if (filled)
                    {
                        existingRow.UpdateDateTime = DateTime.UtcNow;
                        existingRow.UpdateBy = actorUserId;
                    }
                }

                if (!existingRow.PayrollProfileId.HasValue)
                {
                    existingRow.OrganizationAssignmentId = profile.OrganizationAssignmentId;
                    existingRow.PayrollProfileId = profile.Id;
                    existingRow.SalaryAssignmentId = profile.SalaryAssignmentId;
                    existingRow.BankAccountId = profile.BankAccountId;
                    existingRow.CostCenterId = profile.CostCenterId;
                    existingRow.SalaryStructureId = profile.SalaryStructureId;
                    existingRow.SalaryGradeId = profile.SalaryGradeId;
                    existingRow.UpdateDateTime = DateTime.UtcNow;
                    existingRow.UpdateBy = actorUserId;
                    repaired++;
                }
                continue;
            }

            Guid? employeeId = null;
            string number, name;
            if (employeeMap.TryGetValue(profile.WorkforceProfileId, out var emp)) { employeeId = emp.Id; number = emp.EmployeeNumber; name = emp.FullName; }
            else if (doctorMap.TryGetValue(profile.WorkforceProfileId, out var doc)) { number = doc.DoctorNumber; name = doc.FullName; }
            else { skipped++; continue; }
            added++;

            _dbContext.PrlPayrollRunEmployees.Add(new PrlPayrollRunEmployee
            {
                PayrollRunId = run.Id,
                WorkforceProfileId = profile.WorkforceProfileId,
                EmployeeId = employeeId,
                OrganizationAssignmentId = profile.OrganizationAssignmentId,
                PayrollProfileId = profile.Id,
                SalaryAssignmentId = profile.SalaryAssignmentId,
                BankAccountId = profile.BankAccountId,
                CostCenterId = profile.CostCenterId,
                SalaryStructureId = profile.SalaryStructureId,
                SalaryGradeId = profile.SalaryGradeId,
                EmployeePayrollStatus = "Pending",
                CurrencyCode = string.IsNullOrWhiteSpace(profile.CurrencyCode) ? "IDR" : profile.CurrencyCode,
                EmployeeNumberSnapshot = number,
                EmployeeNameSnapshot = name,
                DepartmentNameSnapshot = organization.Department,
                PositionNameSnapshot = organization.Position,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            });
        }
        return new PayrollSeedResult(added, skipped, repaired);
    }

    // ------------------------------------------------------------------------------------
    // Hitung
    // ------------------------------------------------------------------------------------

    public async Task<PayrollRunDetailResponse> CalculateAsync(Guid id, string? expectedRowVersion, Guid actorUserId, CancellationToken cancellationToken)
    {
        await InTransactionAsync($"HR_PAYROLL_RUN_{id:N}", async () =>
        {
            var run = await LoadRunAsync(id, cancellationToken);
            PayrollVersionToken.EnsureMatches(expectedRowVersion, run.CreateDateTime, run.UpdateDateTime, "Payroll run");
            if (run.IsLocked || !PayrollRunStatuses.AcceptsInputs(run.RunStatus))
                throw new PayrollRunValidationException($"Payroll run {run.RunNumber} berstatus {run.RunStatus} dan tidak dapat dihitung ulang.");
            if (run.PayrollPeriod is null || run.PayrollPeriod.IsLocked)
                throw new PayrollRunValidationException("Periode payroll sudah terkunci.");

            run.CalculationStartedAt = DateTime.UtcNow;

            // Pegawai/dokter HRD yang baru memenuhi syarat dibuatkan profil payroll otomatis (hanya membuat yang belum ada; profil yang
            // sudah ada tidak disentuh). Hanya terjangkau oleh run yang masih menerima input (status diperiksa di atas), sehingga run
            // yang sudah disahkan dan snapshot-nya tidak berubah.
            var provision = await _provisioningService.EnsureProfilesAsync(run.PayrollPeriod.StartDate, run.PayrollPeriod.EndDate, actorUserId, "PayrollRun.Calculate", cancellationToken);

            // Populasi dilengkapi lebih dulu (pegawai baru yang eligible, baris penampung dari restitusi) supaya angsuran dan input
            // tertaut ke baris pegawai yang sah dan run tidak disahkan dengan populasi yang tidak lengkap.
            var seed = await SeedEmployeesAsync(run, run.PayrollPeriod, actorUserId, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Jadwal angsuran terbaru (mis. perjanjian baru disetujui setelah run dibuka) ditarik lagi; idempoten.
            var sync = await _syncService.SyncScheduledForRunAsync(run.PayrollPeriod, actorUserId, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Gross-to-net per penerima (gaji pokok prorata, tunjangan, lembur, kehadiran, input variabel, pajak/iuran).
            var calculation = await _calculationService.CalculateRunAsync(run, run.PayrollPeriod, actorUserId, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var blockers = new List<string>(calculation.Blockers);
            var warnings = new List<string>(calculation.Warnings);
            if (seed.SkippedNoIdentity > 0)
                warnings.Add($"{seed.SkippedNoIdentity} profil payroll aktif dilewati karena identitas pegawai/dokter tidak ditemukan.");
            AddProvisioningNotes(warnings, provision);

            // Potongan cicilan dihitung setelah gaji bruto dan potongan wajib ada. Belum menyentuh Finance.
            var deductions = await _deductionService.CalculateRunAsync(run, actorUserId, cancellationToken);
            blockers.AddRange(deductions.Blockers);
            // Batas potongan bawaan yang belum disahkan dan urutan alokasi antar angsuran terlihat sebagai peringatan, tidak diam-diam.
            warnings.AddRange(deductions.Warnings);

            var employees = await _dbContext.PrlPayrollRunEmployees
                .Where(x => x.PayrollRunId == run.Id && !x.IsDelete)
                .ToListAsync(cancellationToken);
            foreach (var employee in employees)
            {
                // Jumlah yang akan dibayarkan = gaji bersih setelah cicilan; penerima bermasalah tidak membawa nominal bayar.
                employee.PaymentAmount = employee.EmployeePayrollStatus == EmployeeCalculated ? employee.NetPay : 0m;
            }

            run.TotalEmployeeCount = employees.Count;
            run.ProcessedEmployeeCount = employees.Count(x => x.EmployeePayrollStatus == EmployeeCalculated);
            run.ErrorEmployeeCount = employees.Count(x => x.EmployeePayrollStatus == EmployeeError) + deductions.Blockers.Count;
            run.TotalBaseSalary = employees.Sum(x => x.BaseSalary);
            run.TotalEarning = employees.Sum(x => x.GrossPay);
            run.TotalGrossPay = employees.Sum(x => x.GrossPay);
            run.TotalTax = employees.Sum(x => x.TotalTax);
            run.TotalEmployeeContribution = employees.Sum(x => x.TotalEmployeeInsuranceContribution);
            run.TotalEmployerContribution = employees.Sum(x => x.TotalEmployerInsuranceContribution);
            run.TotalDeduction = employees.Sum(x => x.TotalDeduction);
            run.TotalNetPay = employees.Sum(x => x.NetPay);
            run.RunStatus = PayrollRunStatuses.Calculated;
            run.CalculatedAt = DateTime.UtcNow;
            run.CalculatedByUserId = actorUserId;
            run.PolicySnapshotJson = _calculationService.BuildPolicySnapshotJson();
            run.ValidationSummaryJson = JsonSerializer.Serialize(new ValidationSummary(
                blockers.Distinct().ToList(), sync.SkipReasons.Distinct().ToList(), warnings.Distinct().ToList(),
                ReviewOnly: null,
                TaxReview: new TaxReviewMeta(1, calculation.TaxHistoryBlockerCount, Math.Max(0, blockers.Count - calculation.TaxHistoryBlockerCount))));
            run.UpdateDateTime = DateTime.UtcNow;
            run.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            await _loggerService.AuditAsync(LogCategory, "PayrollRun.Calculated",
                $"Payroll run {run.RunNumber} dihitung: {run.ProcessedEmployeeCount} pegawai, {deductions.TotalInstallmentsProcessed} potongan cicilan direncanakan, {blockers.Count} penahan.",
                new { run.Id, deductions.TotalDeductedAmount, Blockers = blockers.Count, ActorUserId = actorUserId });
            return run.Id;
        }, cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    // ------------------------------------------------------------------------------------
    // Ajukan, kembalikan, sahkan, batalkan
    // ------------------------------------------------------------------------------------

    public async Task<PayrollRunDetailResponse> SubmitAsync(Guid id, string? expectedRowVersion, Guid actorUserId, CancellationToken cancellationToken)
    {
        await InTransactionAsync($"HR_PAYROLL_RUN_{id:N}", async () =>
        {
            var run = await LoadRunAsync(id, cancellationToken);
            PayrollVersionToken.EnsureMatches(expectedRowVersion, run.CreateDateTime, run.UpdateDateTime, "Payroll run");
            if (run.RunStatus != PayrollRunStatuses.Calculated)
                throw new PayrollRunValidationException($"Hanya payroll run berstatus Calculated yang dapat diajukan (saat ini {run.RunStatus}).");
            await EnsureManualInputsSettledAsync(run, cancellationToken);
            await EnsureCompensationSourcesSettledAsync(run, cancellationToken);
            EnsureReadyToDecide(run);

            run.RunStatus = PayrollRunStatuses.Submitted;
            run.SubmittedAt = DateTime.UtcNow;
            run.SubmittedByUserId = actorUserId;
            run.UpdateDateTime = DateTime.UtcNow;
            run.UpdateBy = actorUserId;
            AddApproval(run, actorUserId, "Submit", "Submitted", null);

            await _dbContext.SaveChangesAsync(cancellationToken);
            await _loggerService.AuditAsync(LogCategory, "PayrollRun.Submitted", $"Payroll run {run.RunNumber} diajukan untuk review.",
                new { run.Id, ActorUserId = actorUserId });
            return run.Id;
        }, cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    /// <summary>Pesan penolakan pengesahan untuk run dengan PPh sementara.</summary>
    private const string TaxNotFinalApproveMessage =
        "Payroll Run sudah diajukan untuk review, tetapi belum dapat disahkan karena perhitungan PPh 21 belum final.";

    /// <summary>
    /// PAY-BE-FIX-030: pengajuan untuk REVIEW dengan PPh sementara. Bukan Submit dengan validasi dimatikan: status, input manual, sumber
    /// kompensasi, pegawai bermasalah, dan SEMUA penahan selain histori PPh yang belum lengkap tetap diperiksa ulang di sini. Hasil pajak
    /// resmi, TotalTax, NetPay, snapshot, dan Finance tidak disentuh. Run tetap tidak dapat disahkan (ApproveAsync menolak).
    /// </summary>
    public async Task<PayrollRunDetailResponse> SubmitForReviewAsync(
        Guid id, string? expectedRowVersion, string? reason, bool acknowledgedProvisionalTax, Guid actorUserId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new PayrollRunValidationException("Alasan pengajuan untuk review dengan PPh sementara wajib diisi.");
        if (!acknowledgedProvisionalTax)
            throw new PayrollRunValidationException("Pengajuan untuk review wajib disertai pernyataan bahwa PPh masih sementara dan belum final.");

        await InTransactionAsync($"HR_PAYROLL_RUN_{id:N}", async () =>
        {
            var run = await LoadRunAsync(id, cancellationToken);
            PayrollVersionToken.EnsureMatches(expectedRowVersion, run.CreateDateTime, run.UpdateDateTime, "Payroll run");
            if (run.RunStatus != PayrollRunStatuses.Calculated)
                throw new PayrollRunValidationException($"Hanya payroll run berstatus Calculated yang dapat diajukan (saat ini {run.RunStatus}).");
            if (run.IsLocked) throw new PayrollRunValidationException("Payroll run terkunci dan tidak dapat diajukan.");
            await EnsureManualInputsSettledAsync(run, cancellationToken);
            await EnsureCompensationSourcesSettledAsync(run, cancellationToken);

            if (run.ErrorEmployeeCount > 0 || run.ProcessedEmployeeCount <= 0)
                throw new PayrollRunValidationException("Payroll run belum siap: masih ada pegawai bermasalah atau belum ada pegawai yang terhitung.");

            var employees = await _dbContext.PrlPayrollRunEmployees.AsNoTracking()
                .Where(x => x.PayrollRunId == run.Id && !x.IsDelete).ToListAsync(cancellationToken);
            // Aturan kelayakan SAMA dengan GET detail dan availableActions (EvaluateTaxReview + UnavailableEstimateReasons).
            var validation = ReadValidation(run.ValidationSummaryJson);
            var taxReview = EvaluateTaxReview(validation, employees);
            switch (taxReview.State)
            {
                case TaxReviewNotApplicable:
                    throw new PayrollRunValidationException("Payroll run tidak memiliki penahan histori PPh; gunakan pengajuan normal.");
                case TaxReviewRecalculationRequired:
                    throw new PayrollRunValidationException($"{PayrollRunReviewBlockCodes.RecalculationRequired}: {TaxReviewRecalculationMessage}");
                case TaxReviewBlocked:
                    // Daftar teks hanya untuk tampilan galat; keputusan sudah diambil dari metadata terstruktur.
                    var otherTexts = validation.Blockers.Except(TaxHistoryBlockerTexts(employees)).Take(3).ToList();
                    throw new PayrollRunValidationException(
                        $"{PayrollRunReviewBlockCodes.OtherBlockers}: Payroll run masih memiliki {taxReview.OtherBlockerCount} penahan selain histori PPh yang belum lengkap"
                        + (otherTexts.Count > 0 ? $": {string.Join("; ", otherTexts)}." : "."));
            }

            var unavailable = UnavailableEstimateReasons(employees);
            if (unavailable.Count > 0)
                throw new PayrollRunValidationException(
                    $"{PayrollRunReviewBlockCodes.EstimateUnavailable}: pengajuan untuk review memerlukan estimasi PPh sementara yang dapat ditelusuri bagi seluruh penerima terdampak. {string.Join(" ", unavailable)}");

            var summary = BuildProvisionalSummary(employees)!;

            var now = DateTime.UtcNow;
            var trimmedReason = reason.Trim();
            run.ValidationSummaryJson = JsonSerializer.Serialize(validation with
            {
                ReviewOnly = new ReviewOnlyMarker(
                    now, actorUserId, trimmedReason, summary.AffectedEmployeeCount, summary.TotalProvisionalTax,
                    string.Join(", ", summary.PolicyVersions), summary.MissingTaxMonths, PayrollTaxBlockCodes.YtdHistoryIncomplete)
            });
            run.RunStatus = PayrollRunStatuses.Submitted;
            run.SubmittedAt = now;
            run.SubmittedByUserId = actorUserId;
            run.UpdateDateTime = now;
            run.UpdateBy = actorUserId;
            var comment = $"PPh sementara (BELUM FINAL, hanya review): {trimmedReason}";
            AddApproval(run, actorUserId, "SubmitForReview", "Submitted", comment.Length <= 2000 ? comment : comment[..2000]);

            await _dbContext.SaveChangesAsync(cancellationToken);
            await _loggerService.AuditAsync(LogCategory, "PayrollRun.SubmittedForReview",
                $"Payroll run {run.RunNumber} diajukan untuk review dengan PPh sementara (belum final): {summary.AffectedEmployeeCount} penerima terdampak, "
                + $"estimasi Rp {summary.TotalProvisionalTax:N2}, bulan histori belum lengkap {string.Join(", ", summary.MissingTaxMonths)}.",
                new
                {
                    run.Id, ActorUserId = actorUserId, BlockCode = PayrollTaxBlockCodes.YtdHistoryIncomplete, summary.AffectedEmployeeCount,
                    summary.TotalProvisionalTax, summary.MissingTaxMonths, PolicyVersions = summary.PolicyVersions, Reason = trimmedReason
                });
            return run.Id;
        }, cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    /// <summary>
    /// PAY-BE-FIX-032: pengesahan BERSYARAT (administratif; PPh BELUM FINAL). Bukan Approve dengan validasi dimatikan dan bukan status
    /// Approved: RunStatus tetap Submitted, run tidak dikunci, snapshot tidak dibekukan, potongan cicilan TIDAK dikonfirmasi ke Finance
    /// (ConfirmRunAsync tidak dipanggil), tidak ada instruksi pembayaran, slip, atau laporan PPh resmi, dan nominal PPh resmi tidak berubah.
    /// Yang tercatat hanya persetujuan beserta alasan, aktor, ringkasan PPh valid vs estimasi, dan kewajiban tindak lanjut. Pengesahan final
    /// tetap lewat jalur normal (kembalikan, hitung ulang dengan histori lengkap, ajukan, sahkan). Idempoten: pengesahan berulang tidak
    /// menambah jejak.
    /// </summary>
    public async Task<PayrollRunDetailResponse> ApproveConditionallyAsync(
        Guid id, string? expectedRowVersion, string? reason, bool acknowledgedProvisionalTax, Guid actorUserId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new PayrollRunValidationException("Alasan pengesahan bersyarat wajib diisi.");
        if (!acknowledgedProvisionalTax)
            throw new PayrollRunValidationException(
                "Pengesahan bersyarat wajib disertai pernyataan bahwa sebagian PPh belum final dan pengesahan ini tidak mengesahkan nominal pajak, posting Finance, atau pembayaran.");

        await InTransactionAsync($"HR_PAYROLL_RUN_{id:N}", async () =>
        {
            var run = await LoadRunAsync(id, cancellationToken);
            var validation = ReadValidation(run.ValidationSummaryJson);
            // Idempoten: sudah disahkan bersyarat pada siklus ini -> kembalikan apa adanya (tanpa jejak ganda).
            if (validation.ConditionalApproval is not null && run.RunStatus == PayrollRunStatuses.Submitted) return run.Id;

            PayrollVersionToken.EnsureMatches(expectedRowVersion, run.CreateDateTime, run.UpdateDateTime, "Payroll run");
            var employees = await _dbContext.PrlPayrollRunEmployees.AsNoTracking()
                .Where(x => x.PayrollRunId == run.Id && !x.IsDelete).ToListAsync(cancellationToken);
            var taxReview = EvaluateTaxReview(validation, employees);
            var recalculationReasons = await FindChangedCompensationSourcesAsync(run, cancellationToken);
            var blockReasons = BuildConditionalApprovalReasons(run, validation, taxReview, employees, recalculationReasons.Count > 0, recalculationReasons);
            if (blockReasons.Count > 0)
                throw new PayrollRunValidationException(string.Join(" ", blockReasons));

            var summary = BuildProvisionalSummary(employees)!;
            var confirmedTotal = employees
                .Select(e => OfficialTaxOf(ReadSnapshot<PayrollTaxSnapshot>(e.TaxSnapshotJson)))
                .Where(amount => amount.HasValue)
                .Sum(amount => amount!.Value);

            var now = DateTime.UtcNow;
            var trimmedReason = reason.Trim();
            var priorCount = await _dbContext.PrlPayrollApprovals.AsNoTracking()
                .CountAsync(x => x.PayrollRunId == run.Id && x.ActionType == "ApproveConditionally", cancellationToken);
            var reference = $"CA-{run.RunNumber}-{priorCount + 1}";
            var marker = new ConditionalApprovalMarker(
                reference, now, actorUserId, trimmedReason, summary.AffectedEmployeeCount, confirmedTotal, summary.TotalProvisionalTax,
                string.Join(", ", summary.PolicyVersions), summary.MissingTaxMonths, FollowUpRequired: true, "PENDING_HISTORY_COMPLETION");

            run.ValidationSummaryJson = JsonSerializer.Serialize(validation with { ConditionalApproval = marker });
            run.UpdateDateTime = now;
            run.UpdateBy = actorUserId;
            var comment = $"Pengesahan bersyarat {reference} (PPh BELUM FINAL, administratif): {trimmedReason}";
            AddApproval(run, actorUserId, "ApproveConditionally", "Conditional", comment.Length <= 2000 ? comment : comment[..2000]);

            await _dbContext.SaveChangesAsync(cancellationToken);
            await _loggerService.AuditAsync(LogCategory, "PayrollRun.ApprovedConditionally",
                $"Payroll run {run.RunNumber} disahkan bersyarat ({reference}; PPh belum final, administratif): {summary.AffectedEmployeeCount} penerima terdampak, "
                + $"PPh valid Rp {confirmedTotal:N2}, estimasi Rp {summary.TotalProvisionalTax:N2}, bulan histori belum lengkap {string.Join(", ", summary.MissingTaxMonths)}. "
                + "Tidak ada pengunci, konfirmasi Finance, atau pembayaran.",
                new
                {
                    run.Id, ActorUserId = actorUserId, Reference = reference, summary.AffectedEmployeeCount, ConfirmedTaxTotal = confirmedTotal,
                    summary.TotalProvisionalTax, summary.MissingTaxMonths, PolicyVersions = summary.PolicyVersions, Reason = trimmedReason
                });
            return run.Id;
        }, cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    /// <summary>
    /// PAY-BE-FIX-032: ringkasan PPh run untuk Finance. BACA-SAJA dan informatif; tidak menulis ke Finance, tidak ada pengiriman, tidak ada
    /// posting. Nominal resmi hanya untuk penerima FINAL_VALID; estimasi dipisahkan dan selalu BELUM FINAL; nilai yang tidak diketahui null.
    /// </summary>
    public async Task<PayrollRunTaxSummaryResponse> GetTaxSummaryAsync(Guid id, CancellationToken cancellationToken)
    {
        var run = await _dbContext.PrlPayrollRuns.AsNoTracking().Include(x => x.PayrollPeriod)
            .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Payroll run tidak ditemukan.");
        var employees = await _dbContext.PrlPayrollRunEmployees.AsNoTracking()
            .Where(x => x.PayrollRunId == id && !x.IsDelete).OrderBy(x => x.EmployeeNameSnapshot).ToListAsync(cancellationToken);
        var validation = ReadValidation(run.ValidationSummaryJson);

        var rows = employees.Select(e =>
        {
            var snapshot = ReadSnapshot<PayrollTaxSnapshot>(e.TaxSnapshotJson);
            var status = ClassifyEmployeeTaxStatus(snapshot);
            var provisional = snapshot?.Provisional;
            var showEstimate = status == PayrollEmployeeTaxStatuses.ProvisionalEstimate && provisional?.Amount is not null;
            return new PayrollRunTaxSummaryEmployeeResponse
            {
                WorkforceProfileId = e.WorkforceProfileId,
                EmployeeId = e.EmployeeId,
                EmployeeNumber = e.EmployeeNumberSnapshot ?? string.Empty,
                EmployeeName = e.EmployeeNameSnapshot,
                TaxYear = snapshot?.TaxYear,
                TaxMonth = snapshot?.TaxMonth,
                TaxArticle = snapshot?.Article,
                CalculationMode = snapshot?.CalculationMode,
                TaxFinalityStatus = status,
                ResultStatus = snapshot?.ResultStatus,
                ConfirmedTaxAmount = OfficialTaxOf(snapshot),
                ProvisionalTaxAmount = showEstimate ? provisional!.Amount : null,
                ProvisionalMethod = showEstimate ? provisional!.Method : null,
                PolicyVersion = snapshot?.PolicyVersion,
                MissingTaxMonths = snapshot is null ? new List<int>() : MissingMonthsOf(snapshot),
                Explanation = status switch
                {
                    PayrollEmployeeTaxStatuses.ProvisionalEstimate => PayrollProvisionalTaxEstimate.ExplanationText,
                    PayrollEmployeeTaxStatuses.IncompleteNoEstimate => "PPh belum dapat dihitung secara sah dan tidak ada estimasi; bukan Rp0.",
                    _ => null
                }
            };
        }).ToList();

        var allFinal = rows.Count > 0 && rows.All(r => r.TaxFinalityStatus is PayrollEmployeeTaxStatuses.FinalValid or PayrollEmployeeTaxStatuses.NotApplicable);
        var finalized = PayrollRunStatuses.IsFinalized(run.RunStatus) && validation.ReviewOnly is null && allFinal;
        var affected = rows.Where(r => r.TaxFinalityStatus is PayrollEmployeeTaxStatuses.ProvisionalEstimate or PayrollEmployeeTaxStatuses.IncompleteNoEstimate).ToList();
        return new PayrollRunTaxSummaryResponse
        {
            RunId = run.Id,
            RunNumber = run.RunNumber,
            PeriodCode = run.PayrollPeriod?.PayrollPeriodCode ?? string.Empty,
            PeriodName = run.PayrollPeriod?.PayrollPeriodName ?? string.Empty,
            RunStatus = run.RunStatus,
            TaxFinalStatus = finalized ? PayrollTaxFinality.Final : PayrollTaxFinality.NotFinal,
            IsPostingEligible = false,
            UsageNotice =
                "Informasi PPh Payroll untuk Finance. Bukan jurnal, bukan utang pajak, dan bukan instruksi pemotongan; estimasi tidak pernah menjadi kewajiban final. "
                + "Posting kewajiban PPh memerlukan kontrak Finance/Accounting yang belum disepakati; belum ada consumer Finance yang terverifikasi.",
            ConditionalApprovalReference = validation.ConditionalApproval?.Reference,
            ReconciliationStatus = validation.ConditionalApproval?.ReconciliationStatus ?? (affected.Count > 0 ? "PENDING_HISTORY_COMPLETION" : "NOT_REQUIRED"),
            AffectedEmployeeCount = affected.Count,
            ConfirmedTaxTotal = rows.Sum(r => r.ConfirmedTaxAmount ?? 0m),
            ProvisionalTaxTotal = rows.Sum(r => r.ProvisionalTaxAmount ?? 0m),
            PolicyVersions = rows.Select(r => r.PolicyVersion).Where(v => !string.IsNullOrWhiteSpace(v)).Select(v => v!).Distinct().ToList(),
            MissingTaxMonths = rows.SelectMany(r => r.MissingTaxMonths).Distinct().OrderBy(m => m).ToList(),
            Employees = rows
        };
    }

    /// <summary>
    /// Menolak pengesahan bila run masih memuat PPh sementara: penanda review-only, atau snapshot penerima yang masih terklasifikasi
    /// histori PPh belum lengkap / memuat estimasi. Dibaca dari data tersimpan, bukan dari klien.
    /// </summary>
    private async Task EnsureTaxFinalAsync(PrlPayrollRun run, CancellationToken cancellationToken)
    {
        var provisional = ReadValidation(run.ValidationSummaryJson).ReviewOnly is not null;
        if (!provisional)
        {
            var snapshots = await _dbContext.PrlPayrollRunEmployees.AsNoTracking()
                .Where(x => x.PayrollRunId == run.Id && !x.IsDelete)
                .Select(x => x.TaxSnapshotJson)
                .ToListAsync(cancellationToken);
            provisional = snapshots
                .Select(json => ReadSnapshot<PayrollTaxSnapshot>(json))
                .Any(snapshot => IsTaxHistoryBlocked(snapshot) || snapshot?.Provisional is not null);
        }

        if (provisional) throw new PayrollRunValidationException(TaxNotFinalApproveMessage);
    }

    /// <summary>Mengembalikan run yang diajukan ke tahap hitung (ditolak reviewer). Belum ada efek ke Finance pada tahap ini.</summary>
    public async Task<PayrollRunDetailResponse> ReturnAsync(Guid id, string? reason, string? expectedRowVersion, Guid actorUserId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(reason)) throw new PayrollRunValidationException("Alasan pengembalian wajib diisi.");

        await InTransactionAsync($"HR_PAYROLL_RUN_{id:N}", async () =>
        {
            var run = await LoadRunAsync(id, cancellationToken);
            PayrollVersionToken.EnsureMatches(expectedRowVersion, run.CreateDateTime, run.UpdateDateTime, "Payroll run");
            if (run.RunStatus != PayrollRunStatuses.Submitted)
                throw new PayrollRunValidationException($"Hanya payroll run berstatus Submitted yang dapat dikembalikan (saat ini {run.RunStatus}).");

            // PAY-BE-FIX-030: penanda review-only dilepas saat run kembali ke Calculated; jejaknya tetap ada pada riwayat persetujuan
            // (SubmitForReview dan Return) dan audit. Run yang kembali belum final: ia harus dihitung ulang sebelum diajukan lagi.
            var returnedValidation = ReadValidation(run.ValidationSummaryJson);
            if (returnedValidation.ReviewOnly is not null || returnedValidation.ConditionalApproval is not null)
                run.ValidationSummaryJson = JsonSerializer.Serialize(returnedValidation with { ReviewOnly = null, ConditionalApproval = null });

            run.RunStatus = PayrollRunStatuses.Calculated;
            run.SubmittedAt = null;
            run.SubmittedByUserId = null;
            run.UpdateDateTime = DateTime.UtcNow;
            run.UpdateBy = actorUserId;
            AddApproval(run, actorUserId, "Return", "Rejected", reason.Trim());

            await _dbContext.SaveChangesAsync(cancellationToken);
            await _loggerService.AuditAsync(LogCategory, "PayrollRun.Returned", $"Payroll run {run.RunNumber} dikembalikan ke tahap hitung: {reason.Trim()}",
                new { run.Id, ActorUserId = actorUserId });
            return run.Id;
        }, cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    /// <summary>
    /// Mengesahkan run. Satu transaksi serializable: hasil potongan cicilan dikonfirmasi ke Finance (saldo piutang berkurang) dan run
    /// dikunci bersamaan. Bila satu hasil gagal diterima Finance, seluruh pengesahan dibatalkan. Idempoten: run yang sudah Approved
    /// dikembalikan apa adanya, tanpa mengirim ulang hasil.
    /// </summary>
    public async Task<PayrollRunDetailResponse> ApproveAsync(Guid id, string? expectedRowVersion, Guid actorUserId, CancellationToken cancellationToken)
    {
        await InTransactionAsync($"HR_PAYROLL_RUN_{id:N}", async () =>
        {
            var run = await LoadRunAsync(id, cancellationToken);
            // Idempoten: run yang sudah disahkan (atau sudah melewati pengesahan) dikembalikan apa adanya, tanpa mengirim ulang hasil.
            if (run.ApprovedAt.HasValue && PayrollRunStatuses.IsFinalized(run.RunStatus)) return run.Id;
            PayrollVersionToken.EnsureMatches(expectedRowVersion, run.CreateDateTime, run.UpdateDateTime, "Payroll run");
            if (run.RunStatus != PayrollRunStatuses.Submitted)
                throw new PayrollRunValidationException($"Hanya payroll run berstatus Submitted yang dapat disahkan (saat ini {run.RunStatus}).");
            // Keputusan pemilik proses (24 Nov 2026): kewenangan mengajukan dan mengesahkan ditentukan Role Management (izin PayrollRun:Submit
            // dan PayrollRun:Approve, ditegakkan [AccessPermission] pada controller), BUKAN keharusan dua pengguna berbeda. Pengaju yang juga
            // memegang izin Approve boleh mengesahkan run yang sama; pengajuan dan pengesahan tetap dua transaksi terpisah dengan jejak
            // SubmittedByUserId/SubmittedAt dan ApprovedByUserId/ApprovedAt serta dua baris riwayat (Submit dan Approve) masing-masing.
            // PAY-BE-FIX-030: run dengan PPh sementara (review-only) TIDAK PERNAH disahkan. Dicek lebih dulu dan terpisah dari penahan umum,
            // sehingga pesannya jelas dan tidak bergantung pada tombol di klien.
            await EnsureTaxFinalAsync(run, cancellationToken);
            EnsureReadyToDecide(run);
            // Sumber kompensasi bisa berubah antara pengajuan dan pengesahan: hasil yang basi tidak boleh dikunci menjadi snapshot final.
            await EnsureCompensationSourcesSettledAsync(run, cancellationToken);

            var confirmed = await _deductionService.ConfirmRunAsync(run, actorUserId, cancellationToken);

            var now = DateTime.UtcNow;
            var employees = await _dbContext.PrlPayrollRunEmployees
                .Where(x => x.PayrollRunId == run.Id && !x.IsDelete).ToListAsync(cancellationToken);
            foreach (var employee in employees)
            {
                employee.PaymentAmount = employee.NetPay;
                employee.EmployeePayrollStatus = EmployeeFinalized;
                employee.IsFinalized = true;
                employee.FinalizedAt = now;
                employee.FinalizedByUserId = actorUserId;
                employee.IsSnapshotFrozen = true;
                employee.SnapshotFrozenAt = now;
                employee.SnapshotFrozenByUserId = actorUserId;
            }

            // Input manual yang disetujui menjadi Applied di transaksi pengesahan yang sama (PAY-BE-20, state-transition-matrix bagian 3).
            var appliedManualInputs = await MoveManualInputsAsync(run.Id,
                new[] { PayrollManualInputStatuses.Approved }, PayrollManualInputStatuses.Applied, reason: null, actorUserId, now, cancellationToken);

            run.RunStatus = PayrollRunStatuses.Approved;
            run.ApprovedAt = now;
            run.ApprovedByUserId = actorUserId;
            run.IsLocked = true;
            run.LockedAt = now;
            run.LockedByUserId = actorUserId;
            run.UpdateDateTime = now;
            run.UpdateBy = actorUserId;
            AddApproval(run, actorUserId, "Approve", "Approved", null);

            await _dbContext.SaveChangesAsync(cancellationToken);
            await _loggerService.AuditAsync(LogCategory, "PayrollRun.Approved",
                $"Payroll run {run.RunNumber} disahkan; {confirmed.TotalInstallmentsProcessed} hasil potongan cicilan (Rp {confirmed.TotalDeductedAmount:N2}) dikonfirmasi ke Finance.",
                new { run.Id, confirmed.TotalInstallmentsProcessed, confirmed.TotalDeductedAmount, AppliedManualInputs = appliedManualInputs, ActorUserId = actorUserId });
            return run.Id;
        }, cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    /// <summary>Membatalkan run SEBELUM disahkan. Run yang sudah disahkan tidak dibatalkan; koreksinya lewat pembalikan.</summary>
    public async Task<PayrollRunDetailResponse> CancelAsync(Guid id, string? reason, string? expectedRowVersion, Guid actorUserId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(reason)) throw new PayrollRunValidationException("Alasan pembatalan wajib diisi.");

        await InTransactionAsync($"HR_PAYROLL_RUN_{id:N}", async () =>
        {
            var run = await LoadRunAsync(id, cancellationToken);
            PayrollVersionToken.EnsureMatches(expectedRowVersion, run.CreateDateTime, run.UpdateDateTime, "Payroll run");
            if (PayrollRunStatuses.IsFinalized(run.RunStatus) || run.IsLocked)
                throw new PayrollRunValidationException("Payroll run yang sudah disahkan tidak dapat dibatalkan; gunakan pembalikan dengan approval.");
            if (run.RunStatus == PayrollRunStatuses.Cancelled) return run.Id;

            // Input manual yang belum Applied ikut dibatalkan di transaksi yang sama (PAY-BE-20). Rejected tetap Rejected (final).
            var cancelledManualInputs = await MoveManualInputsAsync(run.Id,
                new[] { PayrollManualInputStatuses.Draft, PayrollManualInputStatuses.Submitted, PayrollManualInputStatuses.Approved },
                PayrollManualInputStatuses.Cancelled, ManualInputRunCancelledReason, actorUserId, DateTime.UtcNow, cancellationToken);

            run.RunStatus = PayrollRunStatuses.Cancelled;
            run.Notes = string.IsNullOrWhiteSpace(run.Notes) ? $"Dibatalkan: {reason.Trim()}" : $"{run.Notes}\nDibatalkan: {reason.Trim()}";
            run.UpdateDateTime = DateTime.UtcNow;
            run.UpdateBy = actorUserId;
            AddApproval(run, actorUserId, "Cancel", "Cancelled", reason.Trim());

            await _dbContext.SaveChangesAsync(cancellationToken);
            await _loggerService.AuditAsync(LogCategory, "PayrollRun.Cancelled", $"Payroll run {run.RunNumber} dibatalkan: {reason.Trim()}",
                new { run.Id, CancelledManualInputs = cancelledManualInputs, ActorUserId = actorUserId });
            return run.Id;
        }, cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    // ------------------------------------------------------------------------------------
    // Pembalikan potongan cicilan atas run yang sudah disahkan
    //
    // Koreksi sesudah pengesahan TIDAK menjalankan ulang potongan dan TIDAK mengubah slip yang sudah dibekukan. Pembalikan diajukan
    // (dengan alasan), disetujui, lalu Finance memulihkan saldo piutang lewat jalur koreksi bertanda "dari payroll run" dengan mutasi
    // PEMBALIKAN-POTONGAN-GAJI. Pengaju boleh sama dengan penyetuju; approval bertingkat menyusul. Pengembalian dana kepada pegawai
    // (restitusi lewat slip bulan berikutnya) bukan bagian langkah ini.
    // ------------------------------------------------------------------------------------

    public async Task<PayrollRunDetailResponse> RequestReversalAsync(
        Guid runId, Guid runEmployeeId, RequestPayrollReversalRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request?.Reason)) throw new PayrollRunValidationException("Alasan pembalikan wajib diisi.");

        await InTransactionAsync($"HR_PAYROLL_RUN_{runId:N}", async () =>
        {
            var run = await LoadRunAsync(runId, cancellationToken);
            if (!PayrollRunStatuses.AllowsDeductionReversal(run.RunStatus))
                throw new PayrollRunValidationException("Pembalikan hanya untuk payroll run yang sudah disahkan (belum ditutup atau dibatalkan). Run yang belum disahkan cukup dikembalikan atau dihitung ulang.");

            var employee = await _dbContext.PrlPayrollRunEmployees
                .Include(x => x.Components).Include(x => x.VariableInputs)
                .SingleOrDefaultAsync(x => x.Id == runEmployeeId && x.PayrollRunId == runId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Pegawai tidak ditemukan pada payroll run ini.");

            var line = PayrollInstallmentDeductionService.ReadLine(employee);
            if (line is null || !line.IsConfirmedToFinance)
                throw new PayrollRunValidationException("Pegawai ini tidak memiliki potongan cicilan yang sudah dikonfirmasi ke Finance pada run ini.");

            var installment = await _dbContext.FinReceivableInstallments.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == line.InstallmentId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Angsuran Finance tidak ditemukan.");

            var pending = await _dbContext.PrlPayrollReversals.AnyAsync(x => !x.IsDelete
                && x.PayrollRunEmployeeId == runEmployeeId && x.ReversalStatus == PayrollReversalStatuses.Requested, cancellationToken);
            if (pending) throw new PayrollRunConflictException("Pembalikan untuk pegawai ini masih menunggu persetujuan.");

            var deducted = installment.PaidAmount;
            if (deducted <= 0m) throw new PayrollRunValidationException("Tidak ada potongan terpotong yang dapat dibalik.");
            var amount = request!.Amount ?? deducted;
            if (amount <= 0m || amount > deducted)
                throw new PayrollRunValidationException($"Nominal pembalikan harus lebih dari nol dan tidak melebihi nominal terpotong saat ini (Rp {deducted:N0}).");

            var reversal = new PrlPayrollReversal
            {
                PayrollRunId = run.Id,
                PayrollRunEmployeeId = employee.Id,
                ReversalNumber = $"REV-{run.RunNumber}-{Guid.NewGuid().ToString("N")[..4].ToUpperInvariant()}",
                ReversalType = amount == deducted ? "Full" : "Partial",
                ReversalStatus = PayrollReversalStatuses.Requested,
                CurrencyCode = "IDR",
                ReversalAmount = amount,
                ReversalReason = request.Reason.Trim(),
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };
            _dbContext.PrlPayrollReversals.Add(reversal);
            AddApproval(run, actorUserId, "ReversalRequest", "Requested", $"{reversal.ReversalNumber}: {reversal.ReversalReason}");

            await _dbContext.SaveChangesAsync(cancellationToken);
            await _loggerService.AuditAsync(LogCategory, "PayrollRun.ReversalRequested",
                $"Pembalikan potongan {reversal.ReversalNumber} (Rp {amount:N2}) diajukan untuk {employee.EmployeeNameSnapshot} pada run {run.RunNumber}.",
                new { run.Id, ReversalId = reversal.Id, amount, ActorUserId = actorUserId });
            return reversal.Id;
        }, cancellationToken);

        return await GetByIdAsync(runId, cancellationToken);
    }

    /// <summary>
    /// Menyetujui pembalikan: Finance memulihkan saldo piutang (satu transaksi serializable dengan kunci yang sama dengan penerima hasil
    /// Finance). Idempoten: pembalikan yang sudah Approved dikembalikan apa adanya tanpa memulihkan dua kali.
    /// </summary>
    public async Task<PayrollRunDetailResponse> ApproveReversalAsync(Guid runId, Guid reversalId, Guid actorUserId, CancellationToken cancellationToken)
    {
        await InTransactionAsync($"HR_PAYROLL_RUN_{runId:N}", async () =>
        {
            var run = await LoadRunAsync(runId, cancellationToken);
            var reversal = await _dbContext.PrlPayrollReversals
                .SingleOrDefaultAsync(x => x.Id == reversalId && x.PayrollRunId == runId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Pembalikan tidak ditemukan pada payroll run ini.");
            if (reversal.ReversalStatus == PayrollReversalStatuses.Approved) return reversal.Id;
            if (reversal.ReversalStatus != PayrollReversalStatuses.Requested)
                throw new PayrollRunValidationException($"Pembalikan berstatus {reversal.ReversalStatus} dan tidak dapat disetujui.");

            var employee = await _dbContext.PrlPayrollRunEmployees
                .Include(x => x.Components).Include(x => x.VariableInputs)
                .SingleOrDefaultAsync(x => x.Id == reversal.PayrollRunEmployeeId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Pegawai pada pembalikan ini tidak ditemukan.");
            var line = PayrollInstallmentDeductionService.ReadLine(employee)
                ?? throw new PayrollRunValidationException("Potongan cicilan pegawai ini tidak ditemukan pada run.");

            if (_dbContext.Database.IsRelational())
            {
                await _dbContext.Database.ExecuteSqlRawAsync(
                    "SELECT pg_advisory_xact_lock(hashtext({0}));", [$"FIN_INSTALLMENT_PAYROLL_RESULT_{line.InstallmentId:N}"], cancellationToken);
            }

            var current = await _dbContext.FinReceivableInstallments.AsNoTracking()
                .Where(x => x.Id == line.InstallmentId && !x.IsDelete).Select(x => (decimal?)x.PaidAmount).SingleOrDefaultAsync(cancellationToken)
                ?? throw new KeyNotFoundException("Angsuran Finance tidak ditemukan.");
            var newAmount = current - reversal.ReversalAmount;
            if (newAmount < 0m)
                throw new PayrollRunValidationException($"Nominal terpotong saat ini (Rp {current:N0}) lebih kecil dari nominal pembalikan (Rp {reversal.ReversalAmount:N0}).");

            await _syncService.ApplyCorrectionCoreAsync(
                line.InstallmentId,
                newAmount,
                newAmount <= 0m ? PayrollResultStatuses.Gagal : PayrollResultStatuses.Sebagian,
                actorUserId,
                $"Pembalikan {reversal.ReversalNumber} run {run.RunNumber}: {reversal.ReversalReason}",
                cancellationToken,
                fromPayrollRun: true);

            var now = DateTime.UtcNow;
            reversal.ReversalStatus = PayrollReversalStatuses.Approved;
            reversal.ReversedAt = now;
            reversal.ReversedByUserId = actorUserId;
            reversal.PostedAt = now;
            reversal.PostedByUserId = actorUserId;
            reversal.UpdateDateTime = now;
            reversal.UpdateBy = actorUserId;
            AddApproval(run, actorUserId, "ReversalApprove", "Approved", reversal.ReversalNumber);

            await _dbContext.SaveChangesAsync(cancellationToken);
            await _loggerService.AuditAsync(LogCategory, "PayrollRun.ReversalApproved",
                $"Pembalikan potongan {reversal.ReversalNumber} (Rp {reversal.ReversalAmount:N2}) disetujui; saldo piutang dipulihkan di Finance.",
                new { run.Id, ReversalId = reversal.Id, reversal.ReversalAmount, ActorUserId = actorUserId });
            return reversal.Id;
        }, cancellationToken);

        return await GetByIdAsync(runId, cancellationToken);
    }

    public async Task<PayrollRunDetailResponse> RejectReversalAsync(
        Guid runId, Guid reversalId, string? reason, Guid actorUserId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(reason)) throw new PayrollRunValidationException("Alasan penolakan wajib diisi.");

        await InTransactionAsync($"HR_PAYROLL_RUN_{runId:N}", async () =>
        {
            var run = await LoadRunAsync(runId, cancellationToken);
            var reversal = await _dbContext.PrlPayrollReversals
                .SingleOrDefaultAsync(x => x.Id == reversalId && x.PayrollRunId == runId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Pembalikan tidak ditemukan pada payroll run ini.");
            if (reversal.ReversalStatus != PayrollReversalStatuses.Requested)
                throw new PayrollRunValidationException($"Pembalikan berstatus {reversal.ReversalStatus} dan tidak dapat ditolak.");

            reversal.ReversalStatus = PayrollReversalStatuses.Rejected;
            reversal.Notes = $"Ditolak: {reason.Trim()}";
            reversal.UpdateDateTime = DateTime.UtcNow;
            reversal.UpdateBy = actorUserId;
            AddApproval(run, actorUserId, "ReversalReject", "Rejected", $"{reversal.ReversalNumber}: {reason.Trim()}");

            await _dbContext.SaveChangesAsync(cancellationToken);
            await _loggerService.AuditAsync(LogCategory, "PayrollRun.ReversalRejected",
                $"Pembalikan potongan {reversal.ReversalNumber} ditolak: {reason.Trim()}", new { run.Id, ReversalId = reversal.Id, ActorUserId = actorUserId });
            return reversal.Id;
        }, cancellationToken);

        return await GetByIdAsync(runId, cancellationToken);
    }

    // ------------------------------------------------------------------------------------
    // Riwayat status, rincian penerima, dan penutupan
    // ------------------------------------------------------------------------------------

    /// <summary>
    /// Riwayat perpindahan status run (standar transaksi 5.4): diturunkan dari jejak persetujuan (<c>PrlPayrollApproval</c>) yang
    /// ditulis setiap aksi. Nama aktor dipetakan dari pengguna terautentikasi; tidak ada data yang diubah.
    /// </summary>
    public async Task<List<PayrollRunStatusHistoryItemResponse>> GetStatusHistoryAsync(Guid id, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.PrlPayrollRuns.AsNoTracking().AnyAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
        if (!exists) throw new KeyNotFoundException("Payroll run tidak ditemukan.");

        var rows = await _dbContext.PrlPayrollApprovals.AsNoTracking()
            .Where(x => x.PayrollRunId == id && !x.IsDelete)
            .OrderBy(x => x.CreateDateTime)
            .ToListAsync(cancellationToken);

        var actorIds = rows.Where(r => r.ActualActionByUserId.HasValue).Select(r => r.ActualActionByUserId!.Value).Distinct().ToList();
        var names = actorIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await _dbContext.Users.AsNoTracking()
                .Where(u => actorIds.Contains(u.Id))
                .Select(u => new { u.Id, u.DisplayName })
                .ToDictionaryAsync(u => u.Id, u => u.DisplayName, cancellationToken);

        return rows.Select(r => new PayrollRunStatusHistoryItemResponse
        {
            OccurredAt = r.ActionAt ?? r.CreateDateTime,
            Action = r.ActionType ?? string.Empty,
            Status = r.ApprovalStatus,
            ActorUserId = r.ActualActionByUserId,
            ActorName = r.ActualActionByUserId.HasValue && names.TryGetValue(r.ActualActionByUserId.Value, out var name) ? name : null,
            Comments = r.Comments
        }).ToList();
    }

    private const int MaxOvertimePageSize = 200;

    /// <summary>
    /// PAY-BE-23 (PGJ-C1, PRL-DEC-42): rekap lembur terverifikasi HR yang diimpor ke run, per pegawai per tanggal. Murni baca.
    /// Saringan baris sama dengan yang dipakai mesin hitung (aktif dan tidak terhapus), sehingga yang tampil adalah yang dihitung.
    /// Run tanpa lembur menghasilkan daftar kosong.
    /// </summary>
    public async Task<PagedResult<PayrollRunOvertimeInputResponse>> GetOvertimeInputsAsync(
        Guid id, PayrollRunOvertimeInputQuery query, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.PrlPayrollRuns.AsNoTracking().AnyAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
        if (!exists) throw new KeyNotFoundException("Payroll run tidak ditemukan.");

        var source = _dbContext.PrlPayrollOvertimeInputs.AsNoTracking()
            .Where(x => !x.IsDelete && x.IsActive
                && x.PayrollRunEmployee != null
                && !x.PayrollRunEmployee.IsDelete
                && x.PayrollRunEmployee.PayrollRunId == id);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            source = source.Where(x =>
                x.PayrollRunEmployee!.EmployeeNameSnapshot.ToLower().Contains(term)
                || (x.PayrollRunEmployee.EmployeeNumberSnapshot != null && x.PayrollRunEmployee.EmployeeNumberSnapshot.ToLower().Contains(term)));
        }

        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = Math.Clamp(query.PageSize, 1, MaxOvertimePageSize);
        var total = await source.CountAsync(cancellationToken);

        var items = await source
            .OrderBy(x => x.PayrollRunEmployee!.EmployeeNameSnapshot)
            .ThenBy(x => x.OvertimeDate)
            .ThenBy(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new PayrollRunOvertimeInputResponse
            {
                Id = x.Id,
                PayrollRunEmployeeId = x.PayrollRunEmployeeId,
                EmployeeNumber = x.PayrollRunEmployee!.EmployeeNumberSnapshot ?? string.Empty,
                EmployeeName = x.PayrollRunEmployee.EmployeeNameSnapshot,
                OvertimeDate = x.OvertimeDate,
                VerifiedMinutes = x.VerifiedMinutes,
                RateMultiplier = x.RateMultiplier,
                HourlyRate = x.HourlyRate,
                OvertimeAmount = x.OvertimeAmount
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<PayrollRunOvertimeInputResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)pageSize),
            Items = items
        };
    }

    /// <summary>
    /// Rincian satu penerima (popup "Rincian Penghasilan"): status hitung, gross-to-net, pajak dan iuran, kehadiran, SELURUH cicilan, dan
    /// komponen. MURNI BACA: tidak menghitung, tidak menyimpan, dan tidak menyentuh Finance. Rekening disamarkan; NPWP, nomor piutang,
    /// serta jalur dan isi berkas perjanjian tidak dikirim.
    ///
    /// Empat keadaan dibedakan lewat <c>CalculationState</c>: belum dihitung (angka BUKAN hasil, termasuk nol), hasil hitung sah (nol
    /// yang muncul adalah hasil sah), hasil gagal (angka tidak sah; alasan di <c>ValidationIssues</c>), dan snapshot beku (run disahkan).
    /// </summary>
    public async Task<PayrollRunEmployeeDetailResponse> GetEmployeeDetailAsync(Guid runId, Guid runEmployeeId, CancellationToken cancellationToken)
    {
        var employee = await _dbContext.PrlPayrollRunEmployees.AsNoTracking()
            .Include(x => x.Components)
            .Include(x => x.VariableInputs)
            .Include(x => x.PayrollRun).ThenInclude(r => r!.PayrollPeriod)
            .SingleOrDefaultAsync(x => x.Id == runEmployeeId && x.PayrollRunId == runId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Pegawai tidak ditemukan pada payroll run ini.");
        var run = employee.PayrollRun ?? throw new KeyNotFoundException("Payroll run tidak ditemukan.");

        var components = employee.Components.Where(c => !c.IsDelete).OrderBy(c => c.SortOrder).ThenBy(c => c.ComponentCodeSnapshot).ToList();

        // Seluruh angsuran penerima dibaca dengan satu join (bukan N+1); `Installment` lama tetap berisi angsuran pertama.
        var installmentIds = employee.VariableInputs
            .Where(x => x.SourceType == InstallmentSourceType && !x.IsDelete && x.SourceId.HasValue)
            .Select(x => x.SourceId!.Value)
            .Distinct()
            .ToList();
        var facts = await _deductionService.LoadInstallmentFactsAsync(installmentIds, cancellationToken);
        var installments = PayrollInstallmentDeductionService.ReadLines(employee, facts);

        // ---- Status hitung ----
        var attempted = run.CalculatedAt.HasValue;
        var finalized = employee.IsSnapshotFrozen || string.Equals(employee.EmployeePayrollStatus, EmployeeFinalized, StringComparison.OrdinalIgnoreCase);
        var failed = attempted && !finalized && string.Equals(employee.EmployeePayrollStatus, EmployeeError, StringComparison.OrdinalIgnoreCase);
        var succeeded = finalized || (attempted && string.Equals(employee.EmployeePayrollStatus, EmployeeCalculated, StringComparison.OrdinalIgnoreCase));

        var result = ReadEmployeeResult(employee.CalculationResultJson);
        var validation = ReadValidation(run.ValidationSummaryJson);
        var prefix = $"{employee.EmployeeNameSnapshot}: ";
        List<string> ForEmployee(IEnumerable<string>? source) =>
            (source ?? Enumerable.Empty<string>())
                .Where(x => x.StartsWith(prefix, StringComparison.Ordinal))
                .Select(x => x[prefix.Length..])
                .ToList();

        var issues = new List<string>(result.Errors);
        if (issues.Count == 0 && failed && !string.IsNullOrWhiteSpace(employee.ValidationResultJson)) issues.Add(employee.ValidationResultJson.Trim());
        issues.AddRange(ForEmployee(validation.Blockers));
        var warnings = new List<string>(result.Warnings);
        warnings.AddRange(ForEmployee(validation.Warnings));

        // ---- PPh dan iuran: dibaca dari snapshot hasil hitung (TaxSnapshotJson dan InsuranceSnapshotJson), bukan dari perbandingan nominal
        // nol. Status konfigurasi, status hasil, dan alasannya ditetapkan saat hitung oleh PayrollTaxInsuranceRules; GET hanya membaca. ----
        var taxSnapshot = ReadSnapshot<PayrollTaxSnapshot>(employee.TaxSnapshotJson);
        var insuranceSnapshot = ReadSnapshot<PayrollInsuranceSnapshot>(employee.InsuranceSnapshotJson);
        var taxIssues = new List<string>();
        if (succeeded)
        {
            if (taxSnapshot is null || insuranceSnapshot is null)
            {
                // Run lama (dihitung sebelum snapshot ada): statusnya tidak dapat disimpulkan dari nominal. Run yang sudah dibekukan
                // tidak dituduh; hitung ulang hanya relevan selama run masih menerima input.
                if (!finalized)
                    taxIssues.Add("Run dihitung sebelum snapshot konfigurasi PPh dan iuran tersedia; hitung ulang untuk mendapatkan status konfigurasinya.");
            }
            else
            {
                // Hanya konfigurasi PPh yang menentukan kelengkapan. Konfigurasi iuran/asuransi (termasuk BPJS pada snapshot lama) tidak
                // pernah menjadi syarat kelengkapan data payroll.
                if (taxSnapshot.ConfigurationStatus != PayrollConfigurationStatuses.Configured)
                    taxIssues.AddRange(taxSnapshot.Issues.Count > 0 ? taxSnapshot.Issues : new List<string> { "Konfigurasi PPh belum lengkap." });
            }
        }

        var state = finalized ? "SnapshotFrozen"
            : failed ? "Error"
            : !succeeded ? "NotCalculated"
            : taxIssues.Count > 0 ? "DataIncomplete"
            : "Calculated";

        // ---- Ringkasan dari komponen hasil hitung (sumber rincian yang dapat ditelusuri) ----
        decimal SumOf(params string[] codes) => components.Where(c => codes.Contains(c.ComponentCodeSnapshot)).Sum(c => c.Amount);
        var installmentCodes = new[] { InstallmentComponentCode, "LOAN_HOSPITAL_AR" };
        var attendanceDeduction = SumOf(PayrollEngineComponentCodes.AttendanceDeduction);
        var unpaidLeave = SumOf(PayrollEngineComponentCodes.UnpaidLeave);
        var installmentDeduction = SumOf(installmentCodes);
        var otherDeduction = components
            .Where(c => string.Equals(c.ComponentTypeSnapshot, "Deduction", StringComparison.OrdinalIgnoreCase)
                && c.ComponentCodeSnapshot != PayrollEngineComponentCodes.AttendanceDeduction
                && c.ComponentCodeSnapshot != PayrollEngineComponentCodes.UnpaidLeave
                && !installmentCodes.Contains(c.ComponentCodeSnapshot))
            .Sum(c => c.Amount);

        // ---- Konsistensi agregat terhadap komponen penyusunnya (hanya untuk hasil yang sah) ----
        var consistency = new List<string>();
        if (succeeded)
        {
            var earningSum = components.Where(c => string.Equals(c.ComponentTypeSnapshot, "Earning", StringComparison.OrdinalIgnoreCase)).Sum(c => c.Amount);
            var deductionSum = components
                .Where(c => c.ComponentTypeSnapshot is "Deduction" or "Tax" or "Insurance")
                .Sum(c => c.Amount);
            if (Math.Abs(employee.GrossPay - earningSum) > 0.01m)
                consistency.Add($"Gaji bruto (Rp {employee.GrossPay:N2}) tidak sama dengan jumlah komponen penghasilan (Rp {earningSum:N2}).");
            if (Math.Abs(employee.TotalDeduction - deductionSum) > 0.01m)
                consistency.Add($"Total potongan (Rp {employee.TotalDeduction:N2}) tidak sama dengan jumlah komponen potongan (Rp {deductionSum:N2}).");
            if (Math.Abs(employee.NetPay - (employee.GrossPay - employee.TotalDeduction)) > 0.01m)
                consistency.Add($"Gaji bersih (Rp {employee.NetPay:N2}) tidak sama dengan gaji bruto dikurangi total potongan (Rp {employee.GrossPay - employee.TotalDeduction:N2}).");
        }

        var period = run.PayrollPeriod;
        var account = employee.BankAccountNumberSnapshot;

        // Baris yang dibuat sebelum snapshot organisasi diisi: dibaca dari penempatan organisasi saat ini (hanya untuk tampilan, tidak
        // disimpan). Baris yang sudah dibekukan tidak pernah diganti dengan data terkini.
        var departmentName = employee.DepartmentNameSnapshot;
        var positionName = employee.PositionNameSnapshot;
        if (!employee.IsSnapshotFrozen && (string.IsNullOrWhiteSpace(departmentName) || string.IsNullOrWhiteSpace(positionName)))
        {
            var asOf = period is null ? DateTime.UtcNow : PayrollDateNormalizer.ToUtcDate(period.EndDate);
            var names = await ResolveOrganizationNamesAsync(
                new Dictionary<Guid, Guid?> { [employee.WorkforceProfileId] = employee.OrganizationAssignmentId }, asOf, cancellationToken);
            if (names.TryGetValue(employee.WorkforceProfileId, out var live))
            {
                if (string.IsNullOrWhiteSpace(departmentName)) departmentName = live.Department;
                if (string.IsNullOrWhiteSpace(positionName)) positionName = live.Position;
            }
        }

        return new PayrollRunEmployeeDetailResponse
        {
            Id = employee.Id,
            WorkforceProfileId = employee.WorkforceProfileId,
            EmployeeNumber = employee.EmployeeNumberSnapshot ?? string.Empty,
            EmployeeName = employee.EmployeeNameSnapshot,
            EmployeePayrollStatus = employee.EmployeePayrollStatus,
            BaseSalary = employee.BaseSalary,
            TotalRecurringEarning = employee.TotalRecurringEarning,
            TotalVariableEarning = employee.TotalVariableEarning,
            TotalOvertimeAmount = employee.TotalOvertimeAmount,
            TotalAttendanceAllowance = employee.TotalAttendanceAllowance,
            GrossPay = employee.GrossPay,
            TotalTax = employee.TotalTax,
            TotalEmployeeInsuranceContribution = employee.TotalEmployeeInsuranceContribution,
            TotalDeduction = employee.TotalDeduction,
            NetPay = employee.NetPay,
            ValidationMessage = employee.ValidationResultJson,
            Installment = installments.FirstOrDefault(),
            ScheduledWorkMinutes = employee.ScheduledWorkMinutes,
            ActualWorkMinutes = employee.ActualWorkMinutes,
            LateMinutes = employee.LateMinutes,
            OvertimeMinutes = employee.OvertimeMinutes,
            PaidLeaveDays = employee.PaidLeaveDays,
            UnpaidLeaveDays = employee.UnpaidLeaveDays,
            AbsentDays = employee.AbsentDays,
            BankName = employee.BankNameSnapshot,
            BankAccountMasked = MaskAccount(account),
            TaxStatus = employee.TaxStatusSnapshot,
            IsSnapshotFrozen = employee.IsSnapshotFrozen,
            Components = components
                .Select(c => new PayrollRunComponentLineResponse
                {
                    Id = c.Id,
                    ComponentCode = c.ComponentCodeSnapshot,
                    ComponentName = c.ComponentNameSnapshot,
                    ComponentType = c.ComponentTypeSnapshot,
                    SourceType = c.SourceType,
                    Quantity = c.Quantity,
                    Amount = c.Amount,
                    IsProrated = c.IsProrated,
                    IsManual = c.IsManual,
                    Formula = c.FormulaSnapshot,
                    SortOrder = c.SortOrder,
                    SourceId = c.SourceId,
                    CalculationMethod = c.CalculationMethodSnapshot
                }).ToList(),

            RunId = run.Id,
            RunNumber = run.RunNumber,
            RunStatus = run.RunStatus,
            IsRunLocked = run.IsLocked,
            DepartmentName = departmentName,
            PositionName = positionName,
            PayrollPeriodCode = period?.PayrollPeriodCode ?? string.Empty,
            PayrollPeriodName = period?.PayrollPeriodName ?? string.Empty,
            PeriodStartDate = period?.StartDate,
            PeriodEndDate = period?.EndDate,

            CalculationState = state,
            IsCalculated = succeeded,
            CalculationFailed = failed,
            CalculatedAt = attempted ? run.CalculatedAt : null,
            SnapshotFrozenAt = employee.SnapshotFrozenAt,
            ValidationIssues = issues.Distinct().ToList(),
            CalculationWarnings = warnings.Distinct().ToList(),
            ConsistencyIssues = consistency,

            TotalFixedAllowance = SumOf(PayrollEngineComponentCodes.StructureAllowance, PayrollEngineComponentCodes.FixedAllowance, PayrollEngineComponentCodes.WorkforceAllowance),
            TotalTransportAllowance = SumOf(PayrollEngineComponentCodes.TransportAllowance),
            TotalAttendanceDeduction = attendanceDeduction,
            TotalUnpaidLeaveDeduction = unpaidLeave,
            TotalInstallmentDeduction = installmentDeduction,
            TotalOtherDeduction = otherDeduction,
            TotalEmployerInsuranceContribution = employee.TotalEmployerInsuranceContribution,

            ProvisionalTax = ProvisionalTaxOf(employee, taxSnapshot),
            TaxCalculationSource = taxSnapshot is null ? TaxCalculationSourceText : DescribeTaxSource(taxSnapshot.Source),
            // Kompatibel dengan klien lama: Configured hanya bila konfigurasi PPh dan iuran HRD lengkap; selain itu NotConfigured.
            TaxCalculationStatus = !succeeded ? "NotCalculated" : taxIssues.Count > 0 ? "NotConfigured" : "Configured",
            TaxConfigurationIssues = taxIssues,
            Tax = taxSnapshot is null ? null : new PayrollRunTaxResultResponse
            {
                Source = taxSnapshot.Source,
                ConfigurationStatus = taxSnapshot.ConfigurationStatus,
                ResultStatus = taxSnapshot.ResultStatus,
                Article = taxSnapshot.Article,
                RecipientCategory = taxSnapshot.RecipientCategory,
                CalculationMode = taxSnapshot.CalculationMode,
                TaxMethod = taxSnapshot.TaxMethod,
                PtkpStatus = taxSnapshot.PtkpStatus,
                PolicyVersion = taxSnapshot.PolicyVersion,
                Treatment = taxSnapshot.Treatment,
                EmployeeAmount = taxSnapshot.EmployeeAmount,
                EmployerBorneAmount = taxSnapshot.EmployerBorneAmount,
                EffectiveStartDate = taxSnapshot.EffectiveStartDate,
                EffectiveEndDate = taxSnapshot.EffectiveEndDate,
                DecisionBasis = taxSnapshot.DecisionBasis,
                ExemptionReason = taxSnapshot.ExemptionReason,
                Issues = taxSnapshot.Issues,
                Warnings = taxSnapshot.Warnings,
                PolicyId = taxSnapshot.PolicyId,
                PolicyLegalBasis = taxSnapshot.PolicyLegalBasis,
                ManualAmountApprovalStatus = taxSnapshot.ManualAmountApprovalStatus,
                TaxYear = taxSnapshot.TaxYear,
                TaxMonth = taxSnapshot.TaxMonth,
                IsLastTaxPeriod = taxSnapshot.IsLastTaxPeriod,
                TaxableGross = taxSnapshot.TaxableGross,
                EmployerPremium = taxSnapshot.EmployerPremium,
                EmployeePension = taxSnapshot.EmployeePension,
                Detail = taxSnapshot.Detail,
                BlockCode = taxSnapshot.BlockCode,
                Provisional = IsTaxHistoryBlocked(taxSnapshot) ? ProvisionalTaxOf(employee, taxSnapshot) : null
            },
            Insurance = insuranceSnapshot is null ? null : new PayrollRunInsuranceResultResponse
            {
                Source = insuranceSnapshot.Source,
                ConfigurationStatus = insuranceSnapshot.ConfigurationStatus,
                ResultStatus = insuranceSnapshot.ResultStatus,
                ParticipationStatus = insuranceSnapshot.ParticipationStatus,
                Programs = insuranceSnapshot.Programs,
                PolicyVersion = insuranceSnapshot.PolicyVersion,
                PolicyId = insuranceSnapshot.PolicyId,
                PolicyLegalBasis = insuranceSnapshot.PolicyLegalBasis,
                JkkRiskClass = insuranceSnapshot.JkkRiskClass,
                WageBase = insuranceSnapshot.WageBase,
                WageBaseItems = (insuranceSnapshot.WageBaseItems ?? new List<PayrollWageBaseItem>())
                    .Select(i => new PayrollRunWageBaseItemResponse
                    {
                        Source = i.Source,
                        Name = i.Name,
                        Amount = i.Amount,
                        Classification = i.Classification,
                        Reason = i.Reason
                    })
                    .ToList(),
                EmployeeTotal = insuranceSnapshot.EmployeeTotal,
                EmployerTotal = insuranceSnapshot.EmployerTotal,
                Lines = insuranceSnapshot.Lines
                    .Select(l => new PayrollRunInsuranceLineResponse
                    {
                        Code = l.Code,
                        Program = l.Program,
                        PaidBy = l.PaidBy,
                        BaseAmount = l.BaseAmount,
                        Rate = l.Rate,
                        Amount = l.Amount
                    })
                    .ToList(),
                Issues = insuranceSnapshot.Issues,
                Warnings = insuranceSnapshot.Warnings
            },
            FinanceHandoffHoldReason = PayrollFinanceHandoffGuard.ReasonOf(run.PolicySnapshotJson, run.CalculatedAt.HasValue),
            IsFinanceHandoffHeld = PayrollFinanceHandoffGuard.ReasonOf(run.PolicySnapshotJson, run.CalculatedAt.HasValue) is not null,

            Installments = installments
        };
    }

    private const string InstallmentSourceType = "FinanceReceivableInstallment";

    /// <summary>
    /// Sumber PPh dan iuran bila run belum punya snapshot konfigurasi. Sumber resminya adalah konfigurasi HRD (Profil Pajak dan Profil
    /// Iuran); skema otomatis PPh belum tersedia karena belum ada kebijakan tarif berversi. Kontribusi pemberi kerja tidak mengurangi
    /// gaji bersih.
    /// </summary>
    private const string TaxCalculationSourceText = "Konfigurasi HRD (Profil Pajak dan Profil Iuran); belum ada hasil hitung dengan snapshot konfigurasi";

    private static readonly JsonSerializerOptions SnapshotJsonOptions = new() { PropertyNameCaseInsensitive = true };

    private static T? ReadSnapshot<T>(string? json) where T : class
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try { return JsonSerializer.Deserialize<T>(json, SnapshotJsonOptions); }
        catch (JsonException) { return null; }
    }

    private static string DescribeTaxSource(string source) => source switch
    {
        PayrollTaxInsuranceSources.HrdProfile => "Konfigurasi HRD (Profil Pajak) dan kebijakan tarif PPh berversi",
        PayrollTaxInsuranceSources.LegacyProfileAmount => "Tetapan lama profil payroll (cadangan; konfigurasi HRD belum lengkap)",
        _ => "Belum ada sumber PPh yang sah"
    };

    /// <summary>Galat dan peringatan per penerima dari hasil hitung terakhir (CalculationResultJson). JSON lama tanpa daftar terbaca kosong.</summary>
    private static (List<string> Errors, List<string> Warnings) ReadEmployeeResult(string? json)
    {
        var errors = new List<string>();
        var warnings = new List<string>();
        if (string.IsNullOrWhiteSpace(json)) return (errors, warnings);

        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Object) return (errors, warnings);
            AddStrings(document.RootElement, "errors", errors);
            AddStrings(document.RootElement, "warnings", warnings);
        }
        catch (JsonException)
        {
            // JSON rusak tidak menggagalkan pembacaan rincian; daftarnya dibiarkan kosong.
        }

        return (errors, warnings);

        static void AddStrings(JsonElement root, string name, List<string> target)
        {
            if (!root.TryGetProperty(name, out var array) || array.ValueKind != JsonValueKind.Array) return;
            foreach (var item in array.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(item.GetString())) target.Add(item.GetString()!);
            }
        }
    }

    /// <summary>
    /// Menutup run yang sudah lunas dibayar. Syarat: status Paid dan tidak ada pembalikan yang masih menunggu keputusan.
    /// Closed tidak menghapus apa pun; ia hanya menandai run selesai sehingga tidak ada aksi pembayaran lagi.
    /// </summary>
    public async Task<PayrollRunDetailResponse> CloseAsync(Guid id, string? expectedRowVersion, Guid actorUserId, CancellationToken cancellationToken)
    {
        await InTransactionAsync($"HR_PAYROLL_RUN_{id:N}", async () =>
        {
            var run = await LoadRunAsync(id, cancellationToken);
            if (string.Equals(run.RunStatus, PayrollRunStatuses.Closed, StringComparison.OrdinalIgnoreCase)) return run.Id;
            PayrollVersionToken.EnsureMatches(expectedRowVersion, run.CreateDateTime, run.UpdateDateTime, "Payroll run");
            if (!string.Equals(run.RunStatus, PayrollRunStatuses.Paid, StringComparison.OrdinalIgnoreCase))
                throw new PayrollInvalidTransitionException($"Hanya payroll run berstatus Paid yang dapat ditutup (saat ini {run.RunStatus}).");

            var pendingReversal = await _dbContext.PrlPayrollReversals.AnyAsync(x => !x.IsDelete
                && x.PayrollRunId == run.Id && x.ReversalStatus == PayrollReversalStatuses.Requested, cancellationToken);
            if (pendingReversal)
                throw new PayrollInvalidTransitionException("Masih ada pembalikan yang menunggu keputusan; selesaikan sebelum menutup run.");

            var now = DateTime.UtcNow;
            run.RunStatus = PayrollRunStatuses.Closed;
            run.ClosedAt = now;
            run.ClosedByUserId = actorUserId;
            run.UpdateDateTime = now;
            run.UpdateBy = actorUserId;
            AddApproval(run, actorUserId, "Close", "Approved", null);

            await _dbContext.SaveChangesAsync(cancellationToken);
            await _loggerService.AuditAsync(LogCategory, "PayrollRun.Closed", $"Payroll run {run.RunNumber} ditutup.",
                new { run.Id, ActorUserId = actorUserId });
            return run.Id;
        }, cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    private static string? MaskAccount(string? account)
    {
        if (string.IsNullOrWhiteSpace(account)) return null;
        var digits = account.Trim();
        return digits.Length <= 4 ? new string('*', digits.Length) : new string('*', digits.Length - 4) + digits[^4..];
    }

    // ------------------------------------------------------------------------------------
    // Pembantu
    // ------------------------------------------------------------------------------------

    private async Task<PrlPayrollRun> LoadRunAsync(Guid id, CancellationToken cancellationToken) =>
        await _dbContext.PrlPayrollRuns.Include(x => x.PayrollPeriod)
            .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
        ?? throw new KeyNotFoundException("Payroll run tidak ditemukan.");

    /// <summary>Run siap diajukan/disahkan hanya bila tidak ada penahan dan ada pegawai yang terhitung.</summary>
    private static void EnsureReadyToDecide(PrlPayrollRun run)
    {
        var blockers = ReadValidation(run.ValidationSummaryJson).Blockers;
        if (blockers.Count > 0)
            throw new PayrollRunValidationException($"Payroll run memiliki {blockers.Count} penahan: {string.Join("; ", blockers.Take(3))}.");
        if (run.ErrorEmployeeCount > 0 || run.ProcessedEmployeeCount <= 0)
            throw new PayrollRunValidationException("Payroll run belum siap: masih ada pegawai bermasalah atau belum ada pegawai yang terhitung.");
    }

    /// <summary>Alasan sistem pada input manual yang ikut dibatalkan karena run-nya dibatalkan (state-transition-matrix bagian 1).</summary>
    private const string ManualInputRunCancelledReason = "Run dibatalkan";

    private IQueryable<PrlPayrollVariableInput> ManualInputsOfRun(Guid runId) =>
        _dbContext.PrlPayrollVariableInputs.Where(x =>
            !x.IsDelete
            && x.SourceType == PayrollVariableInputSources.ManualInput
            && x.PayrollRunEmployee != null
            && x.PayrollRunEmployee.PayrollRunId == runId);

    /// <summary>
    /// Penjaga pengajuan run (PAY-BE-20): VI-10 tidak boleh ada input manual Submitted; VI-11 tidak boleh ada input manual yang dibuat
    /// atau diubah sesudah perhitungan terakhir. Keduanya 409.
    /// </summary>
    private async Task EnsureManualInputsSettledAsync(PrlPayrollRun run, CancellationToken cancellationToken)
    {
        var pending = await ManualInputsOfRun(run.Id)
            .CountAsync(x => x.InputStatus == PayrollManualInputStatuses.Submitted, cancellationToken);
        if (pending > 0)
            throw new PayrollRunConflictException(
                $"Masih ada {pending} penghasilan variabel yang menunggu keputusan. Setujui atau tolak dulu.");

        if (run.CalculatedAt.HasValue)
        {
            var calculatedAt = run.CalculatedAt.Value;
            var changedAfterCalculation = await ManualInputsOfRun(run.Id)
                .AnyAsync(x => (x.UpdateDateTime ?? x.CreateDateTime) > calculatedAt, cancellationToken);
            if (changedAfterCalculation)
                throw new PayrollRunConflictException("Penghasilan variabel berubah setelah perhitungan terakhir. Hitung ulang run dulu.");
        }
    }

    /// <summary>
    /// Penjaga pengajuan dan pengesahan run (PAY-BE-FIX-008, UAT-14): bila penetapan gaji, tunjangan struktur, tunjangan khusus, atau tunjangan
    /// transport milik penerima run ini berubah setelah perhitungan terakhir, hasil run sudah basi dan tidak boleh diajukan atau disahkan
    /// (409) sampai dihitung ulang. Pola yang sama dengan VI-11 untuk input manual.
    /// </summary>
    private async Task EnsureCompensationSourcesSettledAsync(PrlPayrollRun run, CancellationToken cancellationToken)
    {
        var reasons = await FindChangedCompensationSourcesAsync(run, cancellationToken);
        if (reasons.Count > 0)
            throw new PayrollRunConflictException(
                "Sumber kompensasi berubah setelah perhitungan terakhir (" + string.Join("; ", reasons) + "). Hitung ulang run dulu.");
    }

    private async Task<List<string>> FindChangedCompensationSourcesAsync(PrlPayrollRun run, CancellationToken cancellationToken)
    {
        var reasons = new List<string>();
        if (!run.CalculatedAt.HasValue) return reasons;
        var calculatedAt = run.CalculatedAt.Value;

        var workforceIds = await _dbContext.PrlPayrollRunEmployees.AsNoTracking()
            .Where(x => x.PayrollRunId == run.Id && !x.IsDelete)
            .Select(x => x.WorkforceProfileId)
            .Distinct()
            .ToListAsync(cancellationToken);
        if (workforceIds.Count == 0) return reasons;

        var periodStart = PayrollDateNormalizer.ToUtcDate(run.PeriodStartDateSnapshot);
        var periodEnd = PayrollDateNormalizer.ToUtcDate(run.PeriodEndDateSnapshot);

        // Penetapan gaji: dibuat, diubah, dihapus, atau disetujui setelah hitung dan masa berlakunya menyentuh periode (baris yang dihapus
        // atau dinonaktifkan ikut dihitung: ia bisa saja yang tadinya dipakai).
        if (await _dbContext.WfpSalaryAssignments.AsNoTracking().AnyAsync(a =>
                workforceIds.Contains(a.WorkforceProfileId)
                && a.EffectiveStartDate <= periodEnd && (a.EffectiveEndDate == null || a.EffectiveEndDate >= periodStart)
                && ((a.UpdateDateTime ?? a.CreateDateTime) > calculatedAt || (a.ApprovedAt != null && a.ApprovedAt > calculatedAt)),
                cancellationToken))
            reasons.Add("penetapan gaji");

        // Tunjangan struktur: baris milik struktur yang dipakai penerima pada periode ini.
        var structureIds = await _dbContext.WfpSalaryAssignments.AsNoTracking()
            .Where(a => !a.IsDelete && workforceIds.Contains(a.WorkforceProfileId)
                && a.EffectiveStartDate <= periodEnd && (a.EffectiveEndDate == null || a.EffectiveEndDate >= periodStart))
            .Select(a => a.SalaryStructureId)
            .Distinct()
            .ToListAsync(cancellationToken);
        if (structureIds.Count > 0 && await _dbContext.MstSalaryStructureAllowances.AsNoTracking().AnyAsync(l =>
                structureIds.Contains(l.SalaryStructureId) && (l.UpdateDateTime ?? l.CreateDateTime) > calculatedAt, cancellationToken))
            reasons.Add("tunjangan Struktur Gaji");

        if (await _dbContext.PrlWorkforceAllowances.AsNoTracking().AnyAsync(w =>
                workforceIds.Contains(w.WorkforceProfileId)
                && w.EffectiveStartDate <= periodEnd && (w.EffectiveEndDate == null || w.EffectiveEndDate >= periodStart)
                && ((w.UpdateDateTime ?? w.CreateDateTime) > calculatedAt || (w.DecidedAt != null && w.DecidedAt > calculatedAt)),
                cancellationToken))
            reasons.Add("tunjangan khusus pegawai");

        if (await _dbContext.PrlTransportAllowances.AsNoTracking().AnyAsync(t =>
                workforceIds.Contains(t.WorkforceProfileId)
                && (t.EffectiveStartDate == null || t.EffectiveStartDate <= periodEnd)
                && (t.EffectiveEndDate == null || t.EffectiveEndDate >= periodStart)
                && (t.UpdateDateTime ?? t.CreateDateTime) > calculatedAt,
                cancellationToken))
            reasons.Add("tunjangan transportasi");

        return reasons;
    }

    /// <summary>Memindahkan status input manual run oleh sistem, di dalam transaksi pemanggil. Mengembalikan jumlah input yang dipindah.</summary>
    private async Task<int> MoveManualInputsAsync(
        Guid runId, string[] fromStatuses, string toStatus, string? reason, Guid actorUserId, DateTime now, CancellationToken cancellationToken)
    {
        var inputs = await ManualInputsOfRun(runId)
            .Where(x => fromStatuses.Contains(x.InputStatus))
            .ToListAsync(cancellationToken);

        foreach (var input in inputs)
        {
            input.InputStatus = toStatus;
            if (toStatus == PayrollManualInputStatuses.Cancelled)
            {
                input.IsCancel = true;
                input.CancelDateTime = now;
                input.CancelBy = actorUserId;
                input.DecisionReason = reason;
            }
            input.UpdateDateTime = now;
            input.UpdateBy = actorUserId;
        }

        return inputs.Count;
    }

    private void AddApproval(PrlPayrollRun run, Guid actorUserId, string actionType, string status, string? comments) =>
        _dbContext.PrlPayrollApprovals.Add(new PrlPayrollApproval
        {
            PayrollRunId = run.Id,
            ApprovalLevel = "Payroll",
            ApprovalStatus = status,
            ActionType = actionType,
            ActionAt = DateTime.UtcNow,
            ActualActionByUserId = actorUserId,
            Comments = comments,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = actorUserId
        });

    private static ValidationSummary ReadValidation(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new ValidationSummary(new List<string>(), new List<string>());
        try { return JsonSerializer.Deserialize<ValidationSummary>(json) ?? new ValidationSummary(new List<string>(), new List<string>()); }
        catch (JsonException) { return new ValidationSummary(new List<string>(), new List<string>()); }
    }

    private static T MapListItem<T>(PrlPayrollRun run, T target) where T : PayrollRunListItemResponse
    {
        target.Id = run.Id;
        target.RunNumber = run.RunNumber;
        target.PayrollPeriodId = run.PayrollPeriodId;
        target.PeriodCode = run.PayrollPeriod?.PayrollPeriodCode ?? string.Empty;
        target.PeriodName = run.PayrollPeriod?.PayrollPeriodName ?? string.Empty;
        target.RunType = run.RunType;
        target.RunStatus = run.RunStatus;
        target.IsLocked = run.IsLocked;
        target.TotalEmployeeCount = run.TotalEmployeeCount;
        target.ErrorEmployeeCount = run.ErrorEmployeeCount;
        target.TotalGrossPay = run.TotalGrossPay;
        target.TotalDeduction = run.TotalDeduction;
        target.TotalNetPay = run.TotalNetPay;
        target.CalculatedAt = run.CalculatedAt;
        target.SubmittedAt = run.SubmittedAt;
        target.ApprovedAt = run.ApprovedAt;
        // PAY-BE-FIX-030: penanda PPh belum final dari jejak pengajuan review-only (hanya ada selama run Submitted).
        var mappedValidation = ReadValidation(run.ValidationSummaryJson);
        if (mappedValidation.ConditionalApproval is { } mappedConditional)
        {
            target.IsConditionallyApproved = true;
            target.ConditionalApprovalReference = mappedConditional.Reference;
        }

        if (mappedValidation.ReviewOnly is { } reviewOnly)
        {
            target.IsTaxProvisional = true;
            target.TaxFinalStatus = PayrollTaxFinality.NotFinal;
            target.ProvisionalTaxTotal = reviewOnly.TotalProvisionalTax;
        }

        // PAY-BE-FIX-034: tahap persetujuan eksplisit, diturunkan dari data tersimpan (tanpa kolom/status baru).
        target.ApprovalStage = string.Equals(run.RunStatus, PayrollRunStatuses.Submitted, StringComparison.OrdinalIgnoreCase)
            ? (mappedValidation.ConditionalApproval is not null
                ? PayrollRunApprovalStages.AdministrativeApprovedTaxPending
                : PayrollRunApprovalStages.AwaitingDecision)
            : PayrollRunStatuses.IsFinalized(run.RunStatus) ? PayrollRunApprovalStages.FinalApproved : PayrollRunApprovalStages.None;

        target.RowVersion = PayrollVersionToken.From(run.CreateDateTime, run.UpdateDateTime);
        return target;
    }

    /// <summary>Satu transaksi serializable dengan advisory lock per kunci; rollback penuh bila pekerjaan gagal.</summary>
    private async Task<T> InTransactionAsync<T>(string lockKey, Func<Task<T>> work, CancellationToken cancellationToken)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            if (_dbContext.Database.IsRelational())
            {
                transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
                await _dbContext.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(hashtext({0}));", [lockKey], cancellationToken);
            }

            var result = await work();
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
