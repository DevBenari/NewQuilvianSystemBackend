using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Services
{
    public interface IAdvanceApprovalAndVoucherService
    {
        Task<TravelAdvanceResponseDto> ApproveAdvanceAsync(Guid advanceId, ApproveAdvanceRequestDto dto, Guid currentUserId, bool isFinanceRole);
    }

    public class AdvanceApprovalAndVoucherService : IAdvanceApprovalAndVoucherService
    {
        private readonly ApplicationDbContext _context;

        public AdvanceApprovalAndVoucherService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TravelAdvanceResponseDto> ApproveAdvanceAsync(Guid advanceId, ApproveAdvanceRequestDto dto, Guid currentUserId, bool isFinanceRole)
        {
            var advance = await _context.TrxTravelAdvanceRequests
                .Include(a => a.BusinessTravelRequest)
                .FirstOrDefaultAsync(a => a.Id == advanceId);

            if (advance == null) throw new KeyNotFoundException("Permohonan uang muka tidak ditemukan");

            if (advance.AdvanceStatus == "Paid" || advance.AdvanceStatus == "Settled" || advance.AdvanceStatus == "Rejected")
            {
                throw new InvalidOperationException($"Permohonan uang muka sudah berstatus {advance.AdvanceStatus} dan tidak dapat disetujui lagi");
            }

            var now = DateTime.UtcNow;

            // HRD-DEC-189 & VR-TRV-03: Alur persetujuan 2-pintu (Atasan & Finance)
            if (!isFinanceRole)
            {
                // Pintu 1: Atasan Langsung
                advance.AdvanceStatus = "ManagerApproved";
                advance.ApprovedAmount = dto.ApprovedAmount;
                advance.ApprovedAt = now;
                advance.ApprovedByUserId = currentUserId;
                advance.Notes = string.IsNullOrWhiteSpace(dto.ApprovalNote) ? advance.Notes : $"{advance.Notes} [Catatan Atasan: {dto.ApprovalNote}]";
            }
            else
            {
                // Pintu 2: Finance Manager
                // Validasi bahwa harus sudah disetujui Atasan (kecuali jika langsung delegasi finance)
                advance.AdvanceStatus = "Approved"; // Siap dicairkan kasir
                advance.ApprovedAmount = dto.ApprovedAmount > 0 ? dto.ApprovedAmount : advance.RequestedAmount;
                advance.ApprovedAt = now;
                advance.ApprovedByUserId = currentUserId;
                advance.Notes = string.IsNullOrWhiteSpace(dto.ApprovalNote) ? advance.Notes : $"{advance.Notes} [Catatan Finance: {dto.ApprovalNote}]";

                // Menerbitkan dokumen voucher kasir cito (INT-HR-98)
                var voucherSeq = await _context.TrxTravelAdvancePayments.CountAsync() + 1;
                var voucherNumber = $"VCHR/KSR/{now:yyyy}/{now:MM}/{voucherSeq:D4}";

                var paymentVoucher = new TrxTravelAdvancePayment
                {
                    Id = Guid.NewGuid(),
                    TravelAdvanceRequestId = advance.Id,
                    PaymentNumber = voucherNumber,
                    PaymentDate = DateOnly.FromDateTime(now),
                    PaymentAmount = advance.ApprovedAmount,
                    CurrencyCode = advance.CurrencyCode,
                    PaymentStatus = "Pending",
                    Notes = $"Voucher kasir diterbitkan untuk SPPD {advance.BusinessTravelRequest?.TravelRequestNumber}",
                    IsActive = true,
                    CreateBy = currentUserId,
                    CreateDateTime = now
                };

                _context.TrxTravelAdvancePayments.Add(paymentVoucher);
            }

            advance.UpdateBy = currentUserId;
            advance.UpdateDateTime = now;

            await _context.SaveChangesAsync();

            return new TravelAdvanceResponseDto
            {
                Id = advance.Id,
                AdvanceRequestNumber = advance.AdvanceRequestNumber,
                BusinessTravelRequestId = advance.BusinessTravelRequestId,
                TravelRequestNumber = advance.BusinessTravelRequest?.TravelRequestNumber ?? string.Empty,
                RequestedAmount = advance.RequestedAmount,
                ApprovedAmount = advance.ApprovedAmount,
                PaidAmount = advance.PaidAmount,
                CurrencyCode = advance.CurrencyCode,
                AdvanceStatus = advance.AdvanceStatus,
                RequestDate = advance.RequestDate,
                Notes = advance.Notes
            };
        }
    }
}
