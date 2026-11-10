using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Services
{
    /// <summary>
    /// Kontrak layanan penerima webhook callback rekonsiliasi transfer bank dari Modul Keuangan (INT-HR-54, BE-HRD-159, HRD-DEC-126).
    /// </summary>
    public interface IPayrollDisbursementReconciliationService
    {
        /// <summary>
        /// Memproses callback konfirmasi pembayaran bank dari Finance: mengubah status batch ke Completed dan memicu pelepasan slip gaji digital ESS (TrxPayrollPayslip).
        /// </summary>
        Task<FinanceConfirmationResultDto> ConfirmPaymentAsync(FinancePaymentConfirmationCommand command, CancellationToken cancellationToken = default);

        /// <summary>
        /// Memproses callback penolakan pencairan gaji dari Finance: memvalidasi alasan minimal 15 karakter, melakukan rollback terkendali ke NeedCorrection, dan membuka gembok batch.
        /// </summary>
        Task<FinanceRejectionResultDto> RejectPaymentAsync(FinanceRejectionCallbackCommand command, CancellationToken cancellationToken = default);
    }
}
