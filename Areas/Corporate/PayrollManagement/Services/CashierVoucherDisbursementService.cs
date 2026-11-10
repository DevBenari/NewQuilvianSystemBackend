using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Repositories;
using System.Security.Cryptography;
using System.Text;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Services
{
    /// <summary>
    /// Layanan penerbitan, verifikasi token SHA-256, dan kliring kasir RS 24/7 untuk voucher cito nakes (BE-HRD-172, BE-HRD-173, HRD-DEC-133, INT-HR-59, DED-B6, VR-DED-06, VR-DED-13, VR-DED-14).
    /// </summary>
    public class CashierVoucherDisbursementService : ICashierVoucherDisbursementService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<CashierVoucherDisbursementService> _logger;

        public CashierVoucherDisbursementService(
            ApplicationDbContext dbContext,
            ILogger<CashierVoucherDisbursementService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        private static string ComputeSha256(string raw)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
            return Convert.ToHexString(bytes).ToUpperInvariant();
        }

        public async Task<CashierVoucherResponseDto> IssueVoucherAsync(
            IssueCashierVoucherRequestDto request,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var claim = await _dbContext.TrxEmployeeRestitutionClaims
                .Include(c => c.Employee)
                .FirstOrDefaultAsync(c => c.Id == request.ClaimId && !c.IsDelete, cancellationToken);

            if (claim == null)
            {
                throw new KeyNotFoundException($"Klaim restitusi dengan ID {request.ClaimId} tidak ditemukan.");
            }

            if (claim.ClaimStatus != "Approved")
            {
                throw new InvalidOperationException($"Voucher kasir darurat hanya dapat diterbitkan untuk klaim yang sudah disahkan (Approved). Status saat ini: {claim.ClaimStatus}.");
            }

            if (!claim.IsCitoEmergency)
            {
                throw new InvalidOperationException("Voucher kasir 24/7 hanya diperuntukkan bagi klaim kategori darurat medis nakes cito (IsCitoEmergency = true). Klaim reguler akan disalurkan via payroll tgl 25.");
            }

            if (claim.ClaimStatus == "VoucherIssued" || claim.ClaimStatus == "DisbursedViaCashier")
            {
                throw new InvalidOperationException("Voucher telah diterbitkan atau dicairkan untuk klaim ini sebelumnya.");
            }

            // Pembangkitan nomor unik VCH-YYYYMM-XXXXXX
            var now = DateTime.UtcNow;
            string voucherNumber = $"VCH-{now:yyyyMM}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";

            // Pembangkitan cryptographic security token satu kali pakai (32 bytes hex)
            byte[] randomBytes = RandomNumberGenerator.GetBytes(16);
            string rawToken = Convert.ToHexString(randomBytes).ToUpperInvariant();
            string tokenHash = ComputeSha256(rawToken);

            decimal voucherAmount = claim.ApprovedAmount > 0 ? claim.ApprovedAmount : claim.ClaimAmount;

            var voucher = new TrxCashierDisbursementVoucher
            {
                Id = Guid.NewGuid(),
                VoucherNumber = voucherNumber,
                ClaimId = claim.Id,
                EmployeeId = claim.EmployeeId,
                VoucherAmount = voucherAmount,
                SecurityTokenHash = tokenHash,
                IssuedAt = now,
                ExpiredAt = now.AddHours(48), // Berlaku tepat 48 jam sejak terbit (VR-DED-14)
                VoucherStatus = "Issued",
                Notes = request.Notes,
                CreateDateTime = now,
                CreateBy = userId
            };

            await _dbContext.TrxCashierDisbursementVouchers.AddAsync(voucher, cancellationToken);

            // Perbarui status klaim menjadi VoucherIssued
            claim.ClaimStatus = "VoucherIssued";
            claim.UpdateDateTime = now;
            claim.UpdateBy = userId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Voucher pencairan kasir darurat cito {VoucherNumber} diterbitkan untuk klaim {ClaimNumber} senilai Rp {Amount}. Kedaluwarsa: {ExpiredAt}",
                voucherNumber, claim.ClaimNumber, voucherAmount, voucher.ExpiredAt);

            return new CashierVoucherResponseDto
            {
                Id = voucher.Id,
                VoucherNumber = voucher.VoucherNumber,
                ClaimId = voucher.ClaimId,
                ClaimNumber = claim.ClaimNumber,
                ClaimCategory = claim.ClaimCategory,
                EmployeeId = voucher.EmployeeId,
                EmployeeName = claim.Employee?.FullName,
                EmployeeNik = claim.Employee?.EmployeeNumber,
                VoucherAmount = voucher.VoucherAmount,
                SecurityToken = rawToken, // Diberikan kepada pemegang voucher
                SecurityTokenHash = voucher.SecurityTokenHash,
                IssuedAt = voucher.IssuedAt,
                ExpiredAt = voucher.ExpiredAt,
                VoucherStatus = voucher.VoucherStatus,
                IsExpired = false,
                IsExpiringSoonWarning = false,
                RemainingHours = 48.0,
                Message = "Voucher kasir darurat cito berhasil diterbitkan. Token keamanan satu kali pakai berlaku selama 48 jam untuk dicairkan di Loket Kasir RS 24/7."
            };
        }

        public async Task<CashierVoucherResponseDto?> GetVoucherByIdAsync(
            Guid voucherId,
            CancellationToken cancellationToken = default)
        {
            var voucher = await _dbContext.TrxCashierDisbursementVouchers
                .AsNoTracking()
                .Include(v => v.Claim)
                .Include(v => v.Employee)
                .Include(v => v.CashedByUser)
                .FirstOrDefaultAsync(v => v.Id == voucherId && !v.IsDelete, cancellationToken);

            if (voucher == null) return null;

            return MapToDto(voucher);
        }

        public async Task<List<CashierVoucherResponseDto>> GetVouchersAsync(
            CashierVoucherFilterDto filter,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.TrxCashierDisbursementVouchers
                .AsNoTracking()
                .Include(v => v.Claim)
                .Include(v => v.Employee)
                .Include(v => v.CashedByUser)
                .Where(v => !v.IsDelete);

            if (filter.EmployeeId.HasValue)
                query = query.Where(v => v.EmployeeId == filter.EmployeeId.Value);

            if (!string.IsNullOrWhiteSpace(filter.VoucherStatus))
                query = query.Where(v => v.VoucherStatus == filter.VoucherStatus);

            if (filter.StartDate.HasValue)
                query = query.Where(v => v.IssuedAt >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(v => v.IssuedAt <= filter.EndDate.Value);

            var list = await query.OrderByDescending(v => v.IssuedAt).ToListAsync(cancellationToken);
            return list.Select(MapToDto).ToList();
        }

        public async Task<CashierVoucherResponseDto> VerifyVoucherAsync(
            VerifyCashierVoucherRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var voucher = await _dbContext.TrxCashierDisbursementVouchers
                .Include(v => v.Claim)
                .Include(v => v.Employee)
                .Include(v => v.CashedByUser)
                .FirstOrDefaultAsync(v => v.VoucherNumber == request.VoucherNumber && !v.IsDelete, cancellationToken);

            if (voucher == null)
            {
                throw new KeyNotFoundException($"Voucher kasir dengan nomor '{request.VoucherNumber}' tidak ditemukan.");
            }

            string computedHash = ComputeSha256(request.SecurityToken);
            if (!string.Equals(computedHash, voucher.SecurityTokenHash, StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("Token otentikasi voucher tidak valid. Pencairan ditolak.");
            }

            var now = DateTime.UtcNow;
            if (now > voucher.ExpiredAt && voucher.VoucherStatus == "Issued")
            {
                voucher.VoucherStatus = "Expired";
                voucher.UpdateDateTime = now;
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            var dto = MapToDto(voucher);
            dto.SecurityToken = request.SecurityToken;
            dto.Message = dto.IsExpired
                ? "PERINGATAN: Voucher telah KEDALUWARSA (melebihi batas 48 jam) dan tidak dapat dicairkan."
                : voucher.VoucherStatus == "DisbursedViaCashier"
                    ? $"Voucher telah dicairkan pada {voucher.CashedAt:yyyy-MM-dd HH:mm} (Kuitansi: {voucher.CashierReceiptNumber})."
                    : "Voucher valid dan terverifikasi. Siap untuk proses kliring pencairan kasir.";

            return dto;
        }

        public async Task<CashierVoucherResponseDto> ClaimVoucherAtCashierAsync(
            ClaimCashierVoucherRequestDto request,
            Guid cashierUserId,
            CancellationToken cancellationToken = default)
        {
            var voucher = await _dbContext.TrxCashierDisbursementVouchers
                .Include(v => v.Claim)
                .Include(v => v.Employee)
                .FirstOrDefaultAsync(v => v.VoucherNumber == request.VoucherNumber && !v.IsDelete, cancellationToken);

            if (voucher == null)
            {
                throw new KeyNotFoundException($"Voucher kasir dengan nomor '{request.VoucherNumber}' tidak ditemukan.");
            }

            string computedHash = ComputeSha256(request.SecurityToken);
            if (!string.Equals(computedHash, voucher.SecurityTokenHash, StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("Token otentikasi voucher tidak cocok. Pencairan ditolak.");
            }

            var now = DateTime.UtcNow;
            if (now > voucher.ExpiredAt)
            {
                voucher.VoucherStatus = "Expired";
                voucher.UpdateDateTime = now;
                await _dbContext.SaveChangesAsync(cancellationToken);
                throw new InvalidOperationException("Pencairan ditolak: Voucher kasir telah kedaluwarsa (melebihi 48 jam sejak penerbitan).");
            }

            if (voucher.VoucherStatus == "DisbursedViaCashier")
            {
                throw new InvalidOperationException($"Pencairan ditolak: Voucher telah dicairkan sebelumnya pada {voucher.CashedAt:yyyy-MM-dd HH:mm} dengan kuitansi {voucher.CashierReceiptNumber}.");
            }

            if (voucher.VoucherStatus != "Issued")
            {
                throw new InvalidOperationException($"Pencairan ditolak: Status voucher saat ini '{voucher.VoucherStatus}' tidak memenuhi syarat pencairan.");
            }

            // Eksekusi Kliring Kasir RS 24/7 (INT-HR-59)
            voucher.VoucherStatus = "DisbursedViaCashier";
            voucher.CashedByUserId = cashierUserId;
            voucher.CashedAt = now;
            voucher.CashierReceiptNumber = request.CashierReceiptNumber;
            if (!string.IsNullOrWhiteSpace(request.Notes))
            {
                voucher.Notes = string.IsNullOrWhiteSpace(voucher.Notes)
                    ? request.Notes
                    : $"{voucher.Notes} | {request.Notes}";
            }
            voucher.UpdateDateTime = now;
            voucher.UpdateBy = cashierUserId;

            // Kunci mutlak status klaim menjadi DisbursedViaCashier (DED-B6, VR-DED-06)
            if (voucher.Claim != null)
            {
                voucher.Claim.ClaimStatus = "DisbursedViaCashier";
                voucher.Claim.UpdateDateTime = now;
                voucher.Claim.UpdateBy = cashierUserId;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Voucher kasir {VoucherNumber} berhasil dicairkan oleh kasir {CashierUserId} senilai Rp {Amount} (Kuitansi: {ReceiptNo}). Status klaim terkunci DisbursedViaCashier (DED-B6)",
                voucher.VoucherNumber, cashierUserId, voucher.VoucherAmount, request.CashierReceiptNumber);

            var response = MapToDto(voucher);
            response.SecurityToken = request.SecurityToken;
            response.Message = "Kliring kasir RS 24/7 berhasil diselesaikan. Uang tunai telah dicairkan dan status klaim terkunci permanen 'DisbursedViaCashier' (bebas dari injeksi penggajian bulanan).";
            return response;
        }

        private static CashierVoucherResponseDto MapToDto(TrxCashierDisbursementVoucher v)
        {
            var now = DateTime.UtcNow;
            double remainingHours = Math.Max(0.0, (v.ExpiredAt - now).TotalHours);
            bool isExpired = now > v.ExpiredAt || v.VoucherStatus == "Expired";
            // Peringatan dini DED-W3: Tersisa <= 12 jam (sudah > 36 jam) dan belum dicairkan
            bool isExpiringSoon = v.VoucherStatus == "Issued" && remainingHours > 0 && remainingHours <= 12.0;

            return new CashierVoucherResponseDto
            {
                Id = v.Id,
                VoucherNumber = v.VoucherNumber,
                ClaimId = v.ClaimId,
                ClaimNumber = v.Claim?.ClaimNumber ?? string.Empty,
                ClaimCategory = v.Claim?.ClaimCategory ?? string.Empty,
                EmployeeId = v.EmployeeId,
                EmployeeName = v.Employee?.FullName,
                EmployeeNik = v.Employee?.EmployeeNumber,
                VoucherAmount = v.VoucherAmount,
                SecurityTokenHash = v.SecurityTokenHash,
                IssuedAt = v.IssuedAt,
                ExpiredAt = v.ExpiredAt,
                VoucherStatus = v.VoucherStatus,
                CashedByUserId = v.CashedByUserId,
                CashedByName = v.CashedByUser?.UserName,
                CashedAt = v.CashedAt,
                CashierReceiptNumber = v.CashierReceiptNumber,
                Notes = v.Notes,
                IsExpired = isExpired,
                IsExpiringSoonWarning = isExpiringSoon,
                RemainingHours = Math.Round(remainingHours, 2)
            };
        }
    }
}
