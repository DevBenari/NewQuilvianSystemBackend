using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Services
{
    /// <summary>
    /// Kontrak layanan distribusi dan proteksi slip gaji mandiri ESS ber-PIN sekunder 6-digit (VR-PAY-22 s.d. VR-PAY-24, BE-HRD-159, HRD-DEC-127).
    /// </summary>
    public interface IPayrollPayslipDistributionService
    {
        /// <summary>
        /// Mengambil daftar ringkasan slip gaji pegawai yang login pada portal ESS (hanya slip berstatus Published/Released).
        /// </summary>
        Task<List<SelfServicePayslipSummaryDto>> GetMyPayslipsAsync(Guid employeeId, int? year, CancellationToken cancellationToken = default);

        /// <summary>
        /// Memverifikasi autentikasi sekunder PIN 6-digit pegawai dengan penalti penguncian 30 menit setelah 3 kali gagal (VR-PAY-22, PAY-B6).
        /// </summary>
        Task<PayslipPinVerificationResultDto> VerifyPinAsync(Guid employeeId, string pin, CancellationToken cancellationToken = default);

        /// <summary>
        /// Mengambil rincian lengkap slip gaji setelah PIN sekunder terverifikasi secara sah.
        /// </summary>
        Task<SelfServicePayslipDetailDto> GetPayslipDetailAsync(Guid payslipId, Guid employeeId, string pinToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Mengunduh dokumen slip gaji resmi dalam format PDF ber-watermark dan ber-password terenkripsi.
        /// </summary>
        Task<(byte[] FileBytes, string FileName, string ContentType)> DownloadPayslipPdfAsync(Guid payslipId, Guid employeeId, string pinToken, CancellationToken cancellationToken = default);
    }
}
