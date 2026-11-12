using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Options;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;

/// <summary>
/// Intake jasa medis yang sudah disahkan ke jalur PAYROLL (PAY-BE-07). Perhitungan dan aturan sharing jasa medis TETAP milik Medical
/// Fee; Payroll hanya mengonsumsi hasil sah untuk penerima payroll. Tepat SATU jalur bayar per hak jasa medis (SourceId):
/// PAYROLL (penghasilan variabel pada slip) atau FINANCE_AP (utang dokter di Finance), tidak pernah keduanya.
///
/// Pengamanan pembayaran ganda (berlapis):
/// 1. Kebijakan jalur PAYROLL dimatikan secara bawaan (NEEDS CONFIRMATION, [jalur honor dokter kontrak/nonpegawai]); klaim ditolak.
/// 2. Payroll hanya mengklaim hasil jasa yang BELUM memiliki catatan penyerahan Finance AP dalam status apa pun. Saat ini persetujuan
///    periode Medical Fee otomatis membuat penyerahan AP untuk setiap hasil jasa, sehingga klaim praktis tidak mungkin sampai Medical
///    Fee memutuskan penerima payroll tidak diserahkan ke AP (perubahan itu milik Medical Fee dan belum dilakukan).
/// 3. Klaim dicatat sebagai PrlPayrollVariableInput (SourceType = MdfServiceFee, SourceId = hasil jasa) dan dijaga indeks unik
///    database sehingga satu hasil jasa tidak dapat diklaim dua kali.
/// Perlakuan PPh 21 atas jasa medis BELUM dihitung otomatis (NEEDS CONFIRMATION).
/// </summary>
public sealed class PayrollMedicalFeeIntakeService
{
    public const string PolicyNote =
        "NEEDS CONFIRMATION: jalur PAYROLL untuk jasa medis belum disahkan. Selama nonaktif, seluruh jasa medis dibayar lewat Finance AP " +
        "(perilaku Medical Fee saat ini). Mengaktifkannya juga memerlukan perubahan di Medical Fee agar penerima payroll tidak diserahkan ke AP.";

    private const string LogCategory = "Corporate.PayrollManagement.MedicalFeeIntake";
    private const string PayeeDoctor = "Doctor";
    private const string TaxNote =
        "PPh 21 atas jasa medis belum dihitung otomatis (NEEDS CONFIRMATION); nominal masuk penghasilan variabel sebesar nilai akhir jasa kotor.";

    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;
    private readonly PayrollPolicyOptions _policy;

    public PayrollMedicalFeeIntakeService(ApplicationDbContext dbContext, LoggerService loggerService, IOptions<PayrollPolicyOptions> policyOptions)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
        _policy = policyOptions.Value;
    }

    private sealed record Payee(Guid WorkforceProfileId, string Number, string Name);

    // ------------------------------------------------------------------------------------
    // Kandidat dan posisi jalur (hanya baca)
    // ------------------------------------------------------------------------------------

    public async Task<PayrollMedicalFeeCandidateListResponse> GetCandidatesAsync(PayrollMedicalFeeQuery query, CancellationToken cancellationToken)
    {
        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var fees = _dbContext.MdfServiceFees.AsNoTracking()
            .Where(x => !x.IsDelete && (x.Status == MdfServiceFeeStatuses.Approved || x.Status == MdfServiceFeeStatuses.HandedOff));
        if (!string.IsNullOrWhiteSpace(query.PeriodCode))
            fees = fees.Where(x => x.Period!.PeriodCode == query.PeriodCode);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = $"%{query.Search.Trim()}%";
            fees = fees.Where(x => EF.Functions.ILike(x.FeeNumber, term));
        }

        // Posisi jalur ditentukan dari data lintas tabel; saring negara jalur pada sisi aplikasi untuk halaman yang diminta.
        var rows = await fees.Include(x => x.Period).Include(x => x.FinanceHandoff)
            .OrderByDescending(x => x.CalculatedAt).ThenBy(x => x.FeeNumber)
            .ToListAsync(cancellationToken);

        var feeIds = rows.Select(x => x.Id).ToList();
        var claims = (await _dbContext.PrlPayrollVariableInputs.AsNoTracking()
                .Include(x => x.PayrollRunEmployee).ThenInclude(e => e!.PayrollRun)
                .Where(x => !x.IsDelete && x.SourceType == PayrollVariableInputSources.MedicalServiceFee
                    && x.SourceId != null && feeIds.Contains(x.SourceId.Value)
                    && x.InputStatus != "Cancelled" && x.InputStatus != "Rejected")
                .ToListAsync(cancellationToken))
            .GroupBy(x => x.SourceId!.Value).ToDictionary(g => g.Key, g => g.First());

        var payees = await ResolvePayeesAsync(rows, cancellationToken);
        var payeeWorkforceIds = payees.Values.Select(v => v.WorkforceProfileId).Distinct().ToList();
        var payrollWorkforceIds = (await _dbContext.PrlPayrolls.AsNoTracking()
                .Where(p => !p.IsDelete && p.IsActive && p.IsPayrollEligible && p.PayrollStatus == "Active"
                    && payeeWorkforceIds.Contains(p.WorkforceProfileId))
                .Select(p => p.WorkforceProfileId).Distinct().ToListAsync(cancellationToken)).ToHashSet();

        var items = rows.Select(fee =>
        {
            payees.TryGetValue(fee.Id, out var payee);
            claims.TryGetValue(fee.Id, out var claim);
            var isPayrollPayee = payee is not null && payrollWorkforceIds.Contains(payee.WorkforceProfileId);
            var handoff = fee.FinanceHandoff;

            string state, explanation;
            if (claim is not null)
            {
                state = PayrollMedicalFeePathStates.PayrollClaimed;
                explanation = "Dibayar lewat slip gaji (jalur PAYROLL). Tidak boleh diserahkan ke Finance AP.";
            }
            else if (handoff is not null)
            {
                state = string.Equals(handoff.Status, MdfFinanceHandoffStatuses.Failed, StringComparison.OrdinalIgnoreCase)
                    ? PayrollMedicalFeePathStates.ApFailed : PayrollMedicalFeePathStates.ApActive;
                explanation = state == PayrollMedicalFeePathStates.ApFailed
                    ? "Penyerahan ke Finance AP ditolak. Memindahkan ke jalur PAYROLL perlu keputusan pemilik Medical Fee dan Finance."
                    : "Sudah diserahkan ke Finance AP (jalur FINANCE_AP). Payroll tidak boleh membayarnya lagi.";
            }
            else if (isPayrollPayee)
            {
                state = PayrollMedicalFeePathStates.NoPathYet;
                explanation = "Belum memiliki jalur bayar. Dapat diklaim jalur PAYROLL bila kebijakan diaktifkan.";
            }
            else
            {
                state = PayrollMedicalFeePathStates.NotPayrollPayee;
                explanation = "Penerima bukan penerima payroll; hanya dapat dibayar lewat Finance AP.";
            }

            return new PayrollMedicalFeeCandidateResponse
            {
                ServiceFeeId = fee.Id,
                FeeNumber = fee.FeeNumber,
                MedicalFeePeriodCode = fee.Period?.PeriodCode ?? string.Empty,
                PayeeType = fee.PayeeType,
                PayeeNumber = payee?.Number ?? string.Empty,
                PayeeName = payee?.Name ?? "Penerima belum teridentifikasi",
                IsPayrollPayee = isPayrollPayee,
                FeeStatus = fee.Status,
                FinalAmount = fee.FinalAmount,
                PathState = state,
                ApHandoffStatus = handoff?.Status,
                ClaimInputNumber = claim?.InputNumber,
                ClaimRunNumber = claim?.PayrollRunEmployee?.PayrollRun?.RunNumber,
                ClaimInputStatus = claim?.InputStatus,
                CanClaim = _policy.MedicalFeePayrollPathEnabled && state == PayrollMedicalFeePathStates.NoPathYet,
                Explanation = explanation
            };
        }).ToList();

        if (!string.IsNullOrWhiteSpace(query.PathState))
            items = items.Where(i => string.Equals(i.PathState, query.PathState, StringComparison.OrdinalIgnoreCase)).ToList();

        return new PayrollMedicalFeeCandidateListResponse
        {
            PayrollPathEnabled = _policy.MedicalFeePayrollPathEnabled,
            PolicyNote = PolicyNote,
            Candidates = new PagedResult<PayrollMedicalFeeCandidateResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = items.Count,
                TotalPage = (int)Math.Ceiling(items.Count / (double)pageSize),
                Items = items.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList()
            }
        };
    }

    /// <summary>Penerima per hasil jasa: Doctor = MstDoctor.Id, Workforce = MstWorkforceProfile.Id (rujukan polimorfik tanpa FK).</summary>
    private async Task<Dictionary<Guid, Payee>> ResolvePayeesAsync(List<MdfServiceFee> fees, CancellationToken cancellationToken)
    {
        var result = new Dictionary<Guid, Payee>();
        if (fees.Count == 0) return result;

        var doctorIds = fees.Where(f => f.PayeeType == PayeeDoctor).Select(f => f.PayeeReferenceId).Distinct().ToList();
        var workforceIds = fees.Where(f => f.PayeeType != PayeeDoctor).Select(f => f.PayeeReferenceId).Distinct().ToList();

        var doctors = await _dbContext.MstDoctors.AsNoTracking()
            .Where(d => !d.IsDelete && doctorIds.Contains(d.Id))
            .Select(d => new { d.Id, d.WorkforceProfileId, d.DoctorNumber, d.FullName })
            .ToDictionaryAsync(d => d.Id, cancellationToken);
        var employees = (await _dbContext.MstEmployees.AsNoTracking()
                .Where(e => !e.IsDelete && workforceIds.Contains(e.WorkforceProfileId))
                .Select(e => new { e.WorkforceProfileId, e.EmployeeNumber, e.FullName })
                .ToListAsync(cancellationToken))
            .GroupBy(e => e.WorkforceProfileId).ToDictionary(g => g.Key, g => g.First());
        var workforceDoctors = (await _dbContext.MstDoctors.AsNoTracking()
                .Where(d => !d.IsDelete && workforceIds.Contains(d.WorkforceProfileId))
                .Select(d => new { d.WorkforceProfileId, d.DoctorNumber, d.FullName })
                .ToListAsync(cancellationToken))
            .GroupBy(d => d.WorkforceProfileId).ToDictionary(g => g.Key, g => g.First());

        foreach (var fee in fees)
        {
            if (fee.PayeeType == PayeeDoctor)
            {
                if (doctors.TryGetValue(fee.PayeeReferenceId, out var d))
                    result[fee.Id] = new Payee(d.WorkforceProfileId, d.DoctorNumber, d.FullName);
            }
            else if (employees.TryGetValue(fee.PayeeReferenceId, out var e))
                result[fee.Id] = new Payee(fee.PayeeReferenceId, e.EmployeeNumber, e.FullName);
            else if (workforceDoctors.TryGetValue(fee.PayeeReferenceId, out var wd))
                result[fee.Id] = new Payee(fee.PayeeReferenceId, wd.DoctorNumber, wd.FullName);
        }
        return result;
    }

    // ------------------------------------------------------------------------------------
    // Klaim ke jalur PAYROLL
    // ------------------------------------------------------------------------------------

    public Task<PayrollMedicalFeeIntakeResponse> ClaimAsync(PayrollMedicalFeeIntakeRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        if (!_policy.MedicalFeePayrollPathEnabled)
            throw new PayrollPolicyNotConfirmedException(PolicyNote);

        return PayrollTransactionRunner.RunAsync(_dbContext, PayrollTransactionRunner.RunLockKey(request.PayrollRunId), async () =>
        {
            var run = await _dbContext.PrlPayrollRuns.SingleOrDefaultAsync(x => x.Id == request.PayrollRunId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Payroll run tidak ditemukan.");
            if (run.IsLocked || !PayrollRunStatuses.AcceptsInputs(run.RunStatus))
                throw new PayrollInvalidTransitionException($"Payroll run {run.RunNumber} berstatus {run.RunStatus} dan tidak lagi menerima input.");

            var component = await _dbContext.MstPayrollComponents.AsNoTracking()
                .FirstOrDefaultAsync(c => !c.IsDelete && c.IsActive && c.PayrollComponentCode == _policy.MedicalFeeComponentCode
                    && c.ComponentType == "Earning", cancellationToken)
                ?? throw new PayrollRunValidationException(
                    $"Komponen penggajian jasa medis dengan kode '{_policy.MedicalFeeComponentCode}' (tipe Earning, aktif) belum ada di Data Induk > Komponen Penggajian.");

            var ids = request.ServiceFeeIds.Distinct().ToList();
            var fees = await _dbContext.MdfServiceFees.AsNoTracking()
                .Include(x => x.Period).Include(x => x.FinanceHandoff)
                .Where(x => ids.Contains(x.Id) && !x.IsDelete)
                .ToDictionaryAsync(x => x.Id, cancellationToken);
            var existingClaims = (await _dbContext.PrlPayrollVariableInputs.AsNoTracking()
                    .Include(x => x.PayrollRunEmployee)
                    .Where(x => !x.IsDelete && x.SourceType == PayrollVariableInputSources.MedicalServiceFee
                        && x.SourceId != null && ids.Contains(x.SourceId.Value)
                        && x.InputStatus != "Cancelled" && x.InputStatus != "Rejected")
                    .ToListAsync(cancellationToken))
                .GroupBy(x => x.SourceId!.Value).ToDictionary(g => g.Key, g => g.First());
            var payees = await ResolvePayeesAsync(fees.Values.ToList(), cancellationToken);
            var runEmployees = await _dbContext.PrlPayrollRunEmployees
                .Where(x => x.PayrollRunId == run.Id && !x.IsDelete).ToListAsync(cancellationToken);

            var response = new PayrollMedicalFeeIntakeResponse { PayrollRunId = run.Id, RunNumber = run.RunNumber, TaxNote = TaxNote };
            var now = DateTime.UtcNow;
            foreach (var id in ids)
            {
                var result = new PayrollMedicalFeeIntakeItemResult { ServiceFeeId = id };
                response.Items.Add(result);

                if (!fees.TryGetValue(id, out var fee))
                { Reject(response, result, "Hasil jasa medis tidak ditemukan."); continue; }
                result.FeeNumber = fee.FeeNumber;

                if (existingClaims.TryGetValue(id, out var claim))
                {
                    if (claim.PayrollRunEmployee?.PayrollRunId == run.Id)
                    { result.Outcome = "Idempoten"; result.InputNumber = claim.InputNumber; result.Message = "Sudah diklaim pada run ini."; response.IdempotentCount++; }
                    else Reject(response, result, "Hasil jasa ini sudah diklaim pada payroll run lain (satu sumber hanya satu jalur aktif).");
                    continue;
                }
                if (fee.FinanceHandoff is not null)
                { Reject(response, result, $"Sudah memiliki catatan penyerahan Finance AP (status {fee.FinanceHandoff.Status}); jalur bayar tidak boleh ganda. Pemindahan jalur perlu keputusan pemilik."); continue; }
                if (!string.Equals(fee.Status, MdfServiceFeeStatuses.Approved, StringComparison.OrdinalIgnoreCase))
                { Reject(response, result, $"Hasil jasa berstatus {fee.Status}; hanya yang berstatus Approved dan belum diserahkan yang dapat diklaim."); continue; }
                if (!payees.TryGetValue(id, out var payee))
                { Reject(response, result, "Penerima jasa medis tidak ditemukan di HRD."); continue; }
                var runEmployee = runEmployees.FirstOrDefault(e => e.WorkforceProfileId == payee.WorkforceProfileId);
                if (runEmployee is null)
                { Reject(response, result, $"{payee.Name} bukan penerima pada payroll run ini (belum punya profil payroll eligible)."); continue; }
                if (fee.FinalAmount <= 0m)
                { Reject(response, result, "Nilai akhir jasa tidak positif."); continue; }

                var input = new PrlPayrollVariableInput
                {
                    PayrollRunEmployeeId = runEmployee.Id,
                    PayrollComponentId = component.Id,
                    InputNumber = $"MFI-{fee.FeeNumber}",
                    InputDate = DateOnly.FromDateTime(now),
                    InputType = "MedicalServiceFee",
                    InputStatus = "Approved",
                    CurrencyCode = runEmployee.CurrencyCode,
                    Quantity = 1m,
                    Rate = fee.FinalAmount,
                    Amount = fee.FinalAmount,
                    SourceType = PayrollVariableInputSources.MedicalServiceFee,
                    SourceId = fee.Id,
                    Notes = $"Jasa medis {fee.FeeNumber} periode {fee.Period?.PeriodCode}"
                        + (string.IsNullOrWhiteSpace(request.Notes) ? string.Empty : $" — {request.Notes.Trim()}"),
                    SubmittedAt = now,
                    SubmittedByUserId = actorUserId,
                    VerifiedAt = fee.ApprovedAt,
                    VerifiedByUserId = fee.ApprovedBy,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                };
                _dbContext.PrlPayrollVariableInputs.Add(input);
                result.Outcome = "Diklaim";
                result.InputNumber = input.InputNumber;
                result.Message = "Diklaim ke jalur PAYROLL; akan masuk penghasilan saat run dihitung.";
                response.ClaimedCount++;
                response.ClaimedAmount += fee.FinalAmount;
            }

            if (response.ClaimedCount > 0)
            {
                // Run yang sudah dihitung dimundurkan agar wajib dihitung ulang sebelum diajukan (penjaga hitung ulang).
                if (string.Equals(run.RunStatus, PayrollRunStatuses.Calculated, StringComparison.OrdinalIgnoreCase))
                    run.RunStatus = PayrollRunStatuses.CollectingInput;
                run.UpdateDateTime = now;
                run.UpdateBy = actorUserId;
                await _dbContext.SaveChangesAsync(cancellationToken);
                await _loggerService.AuditAsync(LogCategory, "MedicalFee.Claimed",
                    $"{response.ClaimedCount} hasil jasa medis (Rp {response.ClaimedAmount:N2}) diklaim jalur PAYROLL pada run {run.RunNumber}.",
                    new { run.Id, response.ClaimedCount, response.ClaimedAmount, ActorUserId = actorUserId });
            }
            return response;
        }, cancellationToken);
    }

    private static void Reject(PayrollMedicalFeeIntakeResponse response, PayrollMedicalFeeIntakeItemResult result, string message)
    {
        result.Outcome = "Ditolak";
        result.Message = message;
        response.RejectedCount++;
    }
}
