using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Services
{
    /// <summary>
    /// Layanan pelunasan utang pinjaman pegawai pada pesangon Exit Clearance Sub-20 dan penerbitan statement defisit (BE-HRD-174, HRD-DEC-132, INT-HR-60, DED-B7, DED-W5, VR-DED-07, VR-DED-21).
    /// </summary>
    public interface IExitClearanceReceivableService
    {
        Task<ExitClearanceReceivableCalculationResultDto> CalculateExitClearanceDeductionAsync(
            ExitClearanceReceivableCalculationRequestDto request,
            CancellationToken cancellationToken = default);

        Task<ExitClearanceReceivableCalculationResultDto> ConfirmExitClearanceSettlementAsync(
            ConfirmExitClearanceSettlementRequestDto request,
            Guid userId,
            CancellationToken cancellationToken = default);
    }
}
