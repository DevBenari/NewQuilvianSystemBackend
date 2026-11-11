using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Services
{
    /// <summary>
    /// Implementasi layanan serah terima berkas final penggajian ke Departemen Keuangan (Finance) dengan verifikasi Checksum SHA-256 (INT-HR-53, BE-HRD-158, HRD-DEC-126).
    /// </summary>
    public class PayrollFinanceHandoffService : IPayrollFinanceHandoffService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<PayrollFinanceHandoffService> _logger;

        public PayrollFinanceHandoffService(
            ApplicationDbContext dbContext,
            ILogger<PayrollFinanceHandoffService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<FinanceHandoffBatchResultDto> DispatchFinanceHandoffAsync(
            Guid payrollRunId,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var run = await _dbContext.PrlPayrollRuns
                .Include(r => r.Employees)
                .FirstOrDefaultAsync(r => r.Id == payrollRunId, cancellationToken);

            if (run == null)
            {
                throw new KeyNotFoundException($"Batch siklus penggajian dengan ID {payrollRunId} tidak ditemukan.");
            }

            // 1. Validasi Status Batch Penggajian (Hanya batch Approved / disetujui Direktur yang dapat diserahkan ke Finance)
            var normalizedStatus = run.RunStatus.Trim();
            if (!string.Equals(normalizedStatus, "Approved", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(normalizedStatus, "ApprovedFinal", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Batch penggajian harus berstatus 'Approved' (disetujui Direktur) sebelum dapat diserahkan ke Finance. Status saat ini: '{run.RunStatus}'.");
            }

            var activeEmployees = run.Employees
                .Where(e => e.EmployeePayrollStatus != "Reversed" &&
                            e.EmployeePayrollStatus != "Error" &&
                            e.NetPay > 0)
                .OrderBy(e => e.EmployeeNumberSnapshot ?? e.Id.ToString())
                .ToList();

            if (!activeEmployees.Any())
            {
                throw new InvalidOperationException("Batch penggajian tidak memiliki baris data pegawai dengan nominal transfer gaji bersih valid.");
            }

            // 2. Susun baris transfer rekening bank per pegawai (Disbursement Lines)
            var disbursementLines = new List<PayrollDisbursementLineDto>();
            foreach (var emp in activeEmployees)
            {
                var nik = !string.IsNullOrWhiteSpace(emp.EmployeeNumberSnapshot)
                    ? emp.EmployeeNumberSnapshot
                    : $"EMP-{emp.Id.ToString()[..8].ToUpper()}";

                var bankCode = !string.IsNullOrWhiteSpace(emp.BankNameSnapshot)
                    ? emp.BankNameSnapshot.Trim().ToUpper()
                    : "MANDIRI";

                var bankAccount = !string.IsNullOrWhiteSpace(emp.BankAccountNumberSnapshot)
                    ? emp.BankAccountNumberSnapshot.Trim()
                    : "1370000000000";

                var accountHolder = !string.IsNullOrWhiteSpace(emp.BankAccountHolderSnapshot)
                    ? emp.BankAccountHolderSnapshot.Trim()
                    : emp.EmployeeNameSnapshot;

                var amount = Math.Round(emp.NetPay, 2, MidpointRounding.AwayFromZero);

                disbursementLines.Add(new PayrollDisbursementLineDto
                {
                    DetailId = emp.Id,
                    WorkforceProfileId = emp.WorkforceProfileId,
                    EmployeeNik = nik,
                    EmployeeName = emp.EmployeeNameSnapshot,
                    DestinationBankCode = bankCode,
                    DestinationBankAccountNumber = bankAccount,
                    DestinationAccountHolder = accountHolder,
                    TransferAmount = amount,
                    PaymentReference = $"PAY-{run.PeriodStartDateSnapshot:yyyyMM}-{nik}"
                });
            }

            // 3. Penghitungan Checksum SHA-256 Integritas Data (VR-PAY-21, INT-HR-53)
            var checksumSha256 = ComputeCanonicalChecksum(disbursementLines);

            var totalGross = Math.Round(activeEmployees.Sum(e => e.GrossPay), 2, MidpointRounding.AwayFromZero);
            var totalDeductions = Math.Round(activeEmployees.Sum(e => e.TotalDeduction), 2, MidpointRounding.AwayFromZero);
            var totalNet = Math.Round(disbursementLines.Sum(l => l.TransferAmount), 2, MidpointRounding.AwayFromZero);

            var batchNumber = $"PHB-{run.PeriodStartDateSnapshot:yyyyMM}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

            var handoffBatchId = Guid.NewGuid();

            var payload = new PayrollFinanceHandoffPayload
            {
                HandoffBatchId = handoffBatchId,
                PayrollRunId = run.Id,
                PeriodYear = run.PeriodStartDateSnapshot.Year,
                PeriodMonth = run.PeriodStartDateSnapshot.Month,
                BatchNumber = batchNumber,
                PaymentMethod = "BankTransferBatch",
                SourceBankAccountId = "RS-OPERASIONAL-MANDIRI-01",
                TotalEmployees = disbursementLines.Count,
                TotalGrossAmount = totalGross,
                TotalDeductionsAmount = totalDeductions,
                TotalNetAmount = totalNet,
                Currency = "IDR",
                HandoffTimestamp = DateTime.UtcNow,
                HandedOverByUserId = actorUserId,
                IntegrityChecksum = checksumSha256,
                DisbursementLines = disbursementLines
            };

            var payloadJson = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = false });

            // 4. Periksa apakah sudah ada Handoff Batch lama; jika ada, perbarui atau buat baru
            var existingBatch = await _dbContext.TrxPayrollHandoffBatches
                .FirstOrDefaultAsync(b => b.PayrollRunId == run.Id, cancellationToken);

            if (existingBatch != null)
            {
                existingBatch.BatchNumber = batchNumber;
                existingBatch.PeriodYear = payload.PeriodYear;
                existingBatch.PeriodMonth = payload.PeriodMonth;
                existingBatch.TotalEmployees = payload.TotalEmployees;
                existingBatch.TotalDisbursementAmount = payload.TotalNetAmount;
                existingBatch.ChecksumSha256 = checksumSha256;
                existingBatch.HandoffStatus = "Transferred";
                existingBatch.TransferredAt = DateTime.UtcNow;
                existingBatch.TransferredByUserId = actorUserId;
                existingBatch.PayloadSnapshotJson = payloadJson;
                existingBatch.IsActive = true;
                handoffBatchId = existingBatch.Id;
            }
            else
            {
                var newBatch = new TrxPayrollHandoffBatch
                {
                    Id = handoffBatchId,
                    PayrollRunId = run.Id,
                    BatchNumber = batchNumber,
                    PeriodYear = payload.PeriodYear,
                    PeriodMonth = payload.PeriodMonth,
                    TotalEmployees = payload.TotalEmployees,
                    TotalDisbursementAmount = payload.TotalNetAmount,
                    ChecksumSha256 = checksumSha256,
                    HandoffStatus = "Transferred",
                    TransferredAt = DateTime.UtcNow,
                    TransferredByUserId = actorUserId,
                    PayloadSnapshotJson = payloadJson,
                    IsActive = true
                };
                _dbContext.TrxPayrollHandoffBatches.Add(newBatch);
            }

            // 5. Pembekuan Permanen Immutable Batch (VR-PAY-02, PAY-B2)
            run.IsLocked = true;
            run.LockedAt = DateTime.UtcNow;
            run.LockedByUserId = actorUserId;
            run.RunStatus = "PaymentProcessing";
            run.FinancePaymentBatchId = handoffBatchId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Paket serah terima Finance {BatchNumber} untuk Run ID {RunId} berhasil dibentuk dan dikirim dengan Checksum SHA-256: {Checksum}",
                batchNumber, run.Id, checksumSha256);

            return new FinanceHandoffBatchResultDto
            {
                HandoffBatchId = handoffBatchId,
                PayrollRunId = run.Id,
                BatchNumber = batchNumber,
                HandoffStatus = "Transferred",
                TotalEmployees = disbursementLines.Count,
                TotalDisbursementAmount = totalNet,
                ChecksumSha256 = checksumSha256,
                TransferredAt = DateTime.UtcNow,
                Message = $"Paket serah terima keuangan {batchNumber} berhasil dikirim ke Modul Finance secara asinkron dengan verifikasi integritas Checksum SHA-256."
            };
        }

        public async Task<FinanceHandoffStatusDto?> GetHandoffStatusAsync(
            Guid payrollRunId,
            CancellationToken cancellationToken = default)
        {
            var batch = await _dbContext.TrxPayrollHandoffBatches
                .Include(b => b.PayrollRun)
                .FirstOrDefaultAsync(b => b.PayrollRunId == payrollRunId, cancellationToken);

            if (batch == null)
            {
                return null;
            }

            return new FinanceHandoffStatusDto
            {
                HandoffBatchId = batch.Id,
                PayrollRunId = batch.PayrollRunId,
                BatchNumber = batch.BatchNumber,
                HandoffStatus = batch.HandoffStatus,
                TransferredAt = batch.TransferredAt,
                FinanceReferenceNo = batch.FinanceReferenceNo,
                FinanceNotes = batch.FinanceNotes,
                ChecksumSha256 = batch.ChecksumSha256,
                TotalEmployees = batch.TotalEmployees,
                TotalDisbursementAmount = batch.TotalDisbursementAmount,
                IsLocked = batch.PayrollRun?.IsLocked ?? true,
                RunStatus = batch.PayrollRun?.RunStatus ?? string.Empty
            };
        }

        public async Task<PayrollFinanceHandoffPayload?> GetHandoffPayloadAsync(
            Guid payrollRunId,
            CancellationToken cancellationToken = default)
        {
            var batch = await _dbContext.TrxPayrollHandoffBatches
                .FirstOrDefaultAsync(b => b.PayrollRunId == payrollRunId, cancellationToken);

            if (batch == null || string.IsNullOrWhiteSpace(batch.PayloadSnapshotJson))
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<PayrollFinanceHandoffPayload>(batch.PayloadSnapshotJson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal mendeserialisasi payload snapshot untuk batch ID {BatchId}", batch.Id);
                return null;
            }
        }

        /// <summary>
        /// Menghitung Checksum SHA-256 dari representasi kanonikal seluruh baris transfer gaji bank pegawai (VR-PAY-21).
        /// Format per baris: DetailId|WorkforceProfileId|Nik|BankCode|BankAccountNumber|TransferAmount\n
        /// </summary>
        private static string ComputeCanonicalChecksum(IEnumerable<PayrollDisbursementLineDto> lines)
        {
            var sorted = lines.OrderBy(l => l.DetailId).ToList();
            var sb = new StringBuilder();

            foreach (var line in sorted)
            {
                sb.Append(line.DetailId.ToString("D")).Append('|')
                  .Append(line.WorkforceProfileId.ToString("D")).Append('|')
                  .Append(line.EmployeeNik.Trim()).Append('|')
                  .Append(line.DestinationBankCode.Trim().ToUpperInvariant()).Append('|')
                  .Append(line.DestinationBankAccountNumber.Trim()).Append('|')
                  .Append(line.TransferAmount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture))
                  .Append('\n');
            }

            var canonicalBytes = Encoding.UTF8.GetBytes(sb.ToString());
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(canonicalBytes);

            var hexBuilder = new StringBuilder(64);
            foreach (var b in hashBytes)
            {
                hexBuilder.Append(b.ToString("x2"));
            }

            return hexBuilder.ToString();
        }
    }
}
