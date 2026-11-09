using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Services
{
    /// <summary>
    /// Layanan sinkronisasi snapshot jadwal cicilan piutang/pinjaman pegawai aktif dari Modul Keuangan (BE-HRD-163, INT-HR-57, HRD-DEC-129, VR-DED-17).
    /// </summary>
    public interface IFinanceDeductionSyncService
    {
        Task<FinanceSyncResultDto> SyncFinanceReceivablesAsync(int periodYear, int periodMonth, List<FinanceReceivableSyncItemDto>? mockPayload, Guid userId, CancellationToken cancellationToken = default);
        Task<List<DeductionScheduleResponseDto>> GetActiveSchedulesForEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default);
        Task<List<DeductionScheduleResponseDto>> GetSchedulesAsync(DeductionScheduleFilterDto filter, CancellationToken cancellationToken = default);
        Task<bool> LockCurrentMonthSchedulesAsync(int periodYear, int periodMonth, Guid userId, CancellationToken cancellationToken = default);
    }
}
