using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Services
{
    /// <summary>
    /// Layanan penerbitan, verifikasi kriptografi SHA-256, dan kliring loket kasir RS 24/7 untuk voucher darurat cito nakes (BE-HRD-172, BE-HRD-173, HRD-DEC-133, INT-HR-59, DED-B6, VR-DED-06, VR-DED-13, VR-DED-14).
    /// </summary>
    public interface ICashierVoucherDisbursementService
    {
        Task<CashierVoucherResponseDto> IssueVoucherAsync(IssueCashierVoucherRequestDto request, Guid userId, CancellationToken cancellationToken = default);
        Task<CashierVoucherResponseDto?> GetVoucherByIdAsync(Guid voucherId, CancellationToken cancellationToken = default);
        Task<List<CashierVoucherResponseDto>> GetVouchersAsync(CashierVoucherFilterDto filter, CancellationToken cancellationToken = default);
        Task<CashierVoucherResponseDto> VerifyVoucherAsync(VerifyCashierVoucherRequestDto request, CancellationToken cancellationToken = default);
        Task<CashierVoucherResponseDto> ClaimVoucherAtCashierAsync(ClaimCashierVoucherRequestDto request, Guid cashierUserId, CancellationToken cancellationToken = default);
    }
}
