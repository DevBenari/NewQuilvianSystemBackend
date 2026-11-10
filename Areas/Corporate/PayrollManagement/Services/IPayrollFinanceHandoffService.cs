using System;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Services
{
    /// <summary>
    /// Kontrak layanan serah terima berkas final penggajian ke Bagian Keuangan (Finance) dengan jaminan integritas SHA-256 (INT-HR-53, BE-HRD-158, HRD-DEC-126).
    /// </summary>
    public interface IPayrollFinanceHandoffService
    {
        /// <summary>
        /// Membentuk paket serah terima keuangan, menghitung Checksum SHA-256 kanonikal, mengunci batch menjadi immutable permanen, dan mengirimkan berkas serah terima ke Finance.
        /// </summary>
        Task<FinanceHandoffBatchResultDto> DispatchFinanceHandoffAsync(Guid payrollRunId, Guid actorUserId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Mengambil status serah terima berkas di Finance untuk batch penggajian terkait.
        /// </summary>
        Task<FinanceHandoffStatusDto?> GetHandoffStatusAsync(Guid payrollRunId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Mengambil payload lengkap berkas serah terima keuangan beserta rincian transfer bank pegawai.
        /// </summary>
        Task<PayrollFinanceHandoffPayload?> GetHandoffPayloadAsync(Guid payrollRunId, CancellationToken cancellationToken = default);
    }
}
