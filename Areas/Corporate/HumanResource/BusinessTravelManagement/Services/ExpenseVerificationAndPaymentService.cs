using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.ExpenseManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Services
{
    public interface IExpenseVerificationAndPaymentService
    {
        Task<ExpenseClaimResponseDto> VerifyExpenseClaimAsync(Guid claimId, VerifyExpenseClaimDto dto, Guid currentUserId);
        Task<ExpenseClaimResponseDto> PayExpenseClaimAsync(Guid claimId, PayExpenseClaimDto dto, Guid currentUserId);
    }

    public class ExpenseVerificationAndPaymentService : IExpenseVerificationAndPaymentService
    {
        private readonly ApplicationDbContext _context;

        public ExpenseVerificationAndPaymentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ExpenseClaimResponseDto> VerifyExpenseClaimAsync(Guid claimId, VerifyExpenseClaimDto dto, Guid currentUserId)
        {
            var claim = await _context.TrxExpenseClaims
                .Include(c => c.WorkforceProfile)
                .FirstOrDefaultAsync(c => c.Id == claimId);

            if (claim == null) throw new KeyNotFoundException("Klaim pengeluaran operasional non-trip tidak ditemukan");

            var now = DateTime.UtcNow;
            claim.TotalApprovedAmount = dto.VerifiedAmount;
            claim.ClaimStatus = "Approved";
            claim.ApprovedAt = now;
            claim.ApprovedByUserId = currentUserId;
            claim.ApprovalNotes = string.IsNullOrWhiteSpace(dto.VerificationNotes) ? claim.ApprovalNotes : $"{claim.ApprovalNotes} [Verifikasi Finance: {dto.VerificationNotes}]";
            claim.UpdateBy = currentUserId;
            claim.UpdateDateTime = now;

            // Catat atau buat rekam verifikasi
            var verification = new TrxExpenseVerification
            {
                Id = Guid.NewGuid(),
                ExpenseClaimId = claim.Id,
                VerificationType = "Finance",
                VerificationStatus = "Verified",
                VerifiedAmount = dto.VerifiedAmount,
                VerificationNotes = dto.VerificationNotes,
                VerifiedByUserId = currentUserId,
                VerifiedAt = now,
                IsActive = true,
                CreateBy = currentUserId,
                CreateDateTime = now
            };

            _context.TrxExpenseVerifications.Add(verification);
            await _context.SaveChangesAsync();

            return new ExpenseClaimResponseDto
            {
                Id = claim.Id,
                ClaimNumber = claim.ClaimNumber,
                WorkforceProfileId = claim.WorkforceProfileId,
                EmployeeName = claim.WorkforceProfile?.DisplayName ?? "Pegawai Pemohon",
                ClaimTitle = claim.ClaimTitle,
                ClaimType = claim.ClaimType,
                ClaimDate = claim.ClaimDate,
                TotalClaimedAmount = claim.TotalClaimedAmount,
                TotalApprovedAmount = claim.TotalApprovedAmount,
                TotalPaidAmount = claim.TotalPaidAmount,
                CurrencyCode = claim.CurrencyCode,
                ClaimStatus = claim.ClaimStatus,
                CreatedAt = claim.CreateDateTime
            };
        }

        public async Task<ExpenseClaimResponseDto> PayExpenseClaimAsync(Guid claimId, PayExpenseClaimDto dto, Guid currentUserId)
        {
            if (string.IsNullOrWhiteSpace(dto.ReferenceNumber))
            {
                throw new InvalidOperationException("Nomor referensi pembayaran kasir / transfer bank wajib diisi (VR-TRV-21)");
            }

            var claim = await _context.TrxExpenseClaims
                .Include(c => c.WorkforceProfile)
                .FirstOrDefaultAsync(c => c.Id == claimId);

            if (claim == null) throw new KeyNotFoundException("Klaim pengeluaran operasional non-trip tidak ditemukan");

            if (claim.ClaimStatus != "Approved")
            {
                throw new InvalidOperationException($"Klaim berstatus '{claim.ClaimStatus}' belum dapat dicairkan. Wajib memperoleh persetujuan dan verifikasi Finance terlebih dahulu");
            }

            var now = DateTime.UtcNow;
            claim.ClaimStatus = "Paid";
            claim.TotalPaidAmount = claim.TotalApprovedAmount;
            claim.PaymentCompletedAt = now;
            claim.ApprovalNotes = $"{claim.ApprovalNotes} [Dibayarkan Kasir via {dto.PaymentMethod}, Ref: {dto.ReferenceNumber}]";
            claim.UpdateBy = currentUserId;
            claim.UpdateDateTime = now;

            // Catat transaksi pembayaran kasir
            var payment = new TrxExpensePayment
            {
                Id = Guid.NewGuid(),
                ExpenseClaimId = claim.Id,
                PaymentNumber = $"PAY/EXP/{now:yyyyMMdd}/{claim.ClaimNumber.Replace("/", "_")}",
                PaymentAmount = claim.TotalApprovedAmount,
                CurrencyCode = claim.CurrencyCode,
                PaymentReferenceNumber = dto.ReferenceNumber,
                PaymentMethodSnapshot = dto.PaymentMethod,
                PaymentStatus = "Paid",
                PaidAt = now,
                PaidByUserId = currentUserId,
                Notes = dto.Notes,
                IsActive = true,
                CreateBy = currentUserId,
                CreateDateTime = now
            };

            _context.TrxExpensePayments.Add(payment);
            await _context.SaveChangesAsync();

            return new ExpenseClaimResponseDto
            {
                Id = claim.Id,
                ClaimNumber = claim.ClaimNumber,
                WorkforceProfileId = claim.WorkforceProfileId,
                EmployeeName = claim.WorkforceProfile?.DisplayName ?? "Pegawai Pemohon",
                ClaimTitle = claim.ClaimTitle,
                ClaimType = claim.ClaimType,
                ClaimDate = claim.ClaimDate,
                TotalClaimedAmount = claim.TotalClaimedAmount,
                TotalApprovedAmount = claim.TotalApprovedAmount,
                TotalPaidAmount = claim.TotalPaidAmount,
                CurrencyCode = claim.CurrencyCode,
                ClaimStatus = claim.ClaimStatus,
                CreatedAt = claim.CreateDateTime
            };
        }
    }
}
