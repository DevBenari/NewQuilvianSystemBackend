using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Constants;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;

/// <summary>Status pembayaran gaji satu penerima beserta nomor instruksi yang menentukannya (kosong bila belum ada instruksi).</summary>
public sealed record PayrollSalaryPaymentState(string Status, string? PaymentNumber);

/// <summary>
/// Satu-satunya tempat aturan "status pembayaran gaji penerima" (PAY-BE-11). Dipakai daftar potongan piutang, rekonsiliasi potongan,
/// ringkasan pembayaran run, dan rekonsiliasi keuangan supaya keempatnya tidak pernah berbeda. Hanya membaca; tidak mengubah data,
/// tidak mengubah titik pengakuan potongan, dan tidak membayar apa pun.
/// </summary>
public static class PayrollSalaryPaymentStatusResolver
{
    private const string AppliedInputStatus = "Applied";

    /// <summary>
    /// Aturan berurutan atas status instruksi penerima: (1) ada Paid atau PartiallyReversed → LUNAS; (2) ada Scheduled atau
    /// Processing → MENUNGGU_BAYAR; (3) ada Reversed → DIBALIK; (4) ada Failed → GAGAL; (5) tidak ada satu pun: nominal pembayaran
    /// nol atau kurang → TIDAK_ADA_PEMBAYARAN, selain itu BELUM_DIKIRIM. Cancelled dan status lain diabaikan.
    /// </summary>
    public static string Resolve(decimal paymentAmount, IEnumerable<string?> paymentStatuses)
    {
        var statuses = (paymentStatuses ?? Enumerable.Empty<string?>())
            .Where(status => !string.IsNullOrWhiteSpace(status))
            .Select(status => status!.Trim())
            .ToList();

        bool Has(string status) => statuses.Any(s => string.Equals(s, status, StringComparison.OrdinalIgnoreCase));

        if (Has(PayrollPaymentStatuses.Paid) || Has(PayrollPaymentStatuses.PartiallyReversed)) return PayrollSalaryPaymentStatuses.Paid;
        if (Has(PayrollPaymentStatuses.Scheduled) || Has(PayrollPaymentStatuses.Processing)) return PayrollSalaryPaymentStatuses.Pending;
        if (Has(PayrollPaymentStatuses.Reversed)) return PayrollSalaryPaymentStatuses.Reversed;
        if (Has(PayrollPaymentStatuses.Failed)) return PayrollSalaryPaymentStatuses.Failed;
        return paymentAmount <= 0m ? PayrollSalaryPaymentStatuses.NothingToPay : PayrollSalaryPaymentStatuses.NotSent;
    }

    /// <summary>
    /// Membaca instruksi pembayaran seluruh penerima sekali (satu kueri) lalu menurunkan status tiap penerima. Penerima yang tidak
    /// punya instruksi tetap mendapat status (BELUM_DIKIRIM atau TIDAK_ADA_PEMBAYARAN menurut nominal pembayarannya).
    /// </summary>
    public static async Task<Dictionary<Guid, PayrollSalaryPaymentState>> LoadAsync(
        ApplicationDbContext dbContext,
        IReadOnlyCollection<Guid> runEmployeeIds,
        IReadOnlyDictionary<Guid, decimal> paymentAmountByRunEmployeeId,
        CancellationToken cancellationToken)
    {
        var result = new Dictionary<Guid, PayrollSalaryPaymentState>();
        var ids = runEmployeeIds.Distinct().ToList();
        if (ids.Count == 0) return result;

        var rows = await dbContext.PrlPayrollPayments.AsNoTracking()
            .Where(p => !p.IsDelete && ids.Contains(p.PayrollRunEmployeeId))
            .Select(p => new { p.PayrollRunEmployeeId, p.PaymentStatus, p.PaymentNumber, p.CreateDateTime })
            .ToListAsync(cancellationToken);
        var byEmployee = rows.GroupBy(r => r.PayrollRunEmployeeId).ToDictionary(g => g.Key, g => g.ToList());

        foreach (var id in ids)
        {
            paymentAmountByRunEmployeeId.TryGetValue(id, out var amount);
            var payments = byEmployee.TryGetValue(id, out var list) ? list : rows.Take(0).ToList();
            var status = Resolve(amount, payments.Select(p => (string?)p.PaymentStatus));

            // Nomor instruksi yang menentukan status: yang terbaru di antara instruksi yang statusnya sesuai.
            var determining = DeterminingPaymentStatuses(status);
            var number = payments
                .Where(p => determining.Any(s => string.Equals(s, p.PaymentStatus, StringComparison.OrdinalIgnoreCase)))
                .OrderByDescending(p => p.CreateDateTime)
                .Select(p => p.PaymentNumber)
                .FirstOrDefault();
            result[id] = new PayrollSalaryPaymentState(status, string.IsNullOrWhiteSpace(number) ? null : number);
        }

        return result;
    }

    /// <summary>
    /// Jumlah potongan piutang yang SUDAH dikonfirmasi ke Finance (masukan berstatus Applied) pada satu run, yang gaji
    /// penerimanya belum dibayar (<c>Pending</c>) atau gagal/dibalik (<c>Failed</c>). Satuannya masukan potongan, bukan penerima.
    /// </summary>
    public static async Task<(int Pending, int Failed)> CountConfirmedDeductionAttentionAsync(
        ApplicationDbContext dbContext, Guid payrollRunId, CancellationToken cancellationToken)
    {
        var inputs = await dbContext.PrlPayrollVariableInputs.AsNoTracking()
            .Where(x => !x.IsDelete
                && x.PayrollRunEmployee!.PayrollRunId == payrollRunId
                && x.SourceType == PayrollVariableInputSources.FinanceReceivableInstallment
                && x.SourceId != null
                && x.InputStatus == AppliedInputStatus)
            .Select(x => new { x.PayrollRunEmployeeId, x.PayrollRunEmployee!.PaymentAmount })
            .ToListAsync(cancellationToken);
        if (inputs.Count == 0) return (0, 0);

        var amounts = inputs
            .GroupBy(x => x.PayrollRunEmployeeId)
            .ToDictionary(g => g.Key, g => g.First().PaymentAmount);
        var states = await LoadAsync(dbContext, amounts.Keys.ToList(), amounts, cancellationToken);

        var pending = 0;
        var failed = 0;
        foreach (var input in inputs)
        {
            if (!states.TryGetValue(input.PayrollRunEmployeeId, out var state)) continue;
            if (PayrollSalaryPaymentStatuses.IsPendingAttention(state.Status)) pending++;
            else if (PayrollSalaryPaymentStatuses.IsFailedAttention(state.Status)) failed++;
        }

        return (pending, failed);
    }

    private static string[] DeterminingPaymentStatuses(string salaryStatus) => salaryStatus switch
    {
        PayrollSalaryPaymentStatuses.Paid => [PayrollPaymentStatuses.Paid, PayrollPaymentStatuses.PartiallyReversed],
        PayrollSalaryPaymentStatuses.Pending => [PayrollPaymentStatuses.Scheduled, PayrollPaymentStatuses.Processing],
        PayrollSalaryPaymentStatuses.Reversed => [PayrollPaymentStatuses.Reversed],
        PayrollSalaryPaymentStatuses.Failed => [PayrollPaymentStatuses.Failed],
        _ => []
    };
}
