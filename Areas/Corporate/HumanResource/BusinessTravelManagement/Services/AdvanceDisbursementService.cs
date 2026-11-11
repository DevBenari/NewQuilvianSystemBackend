using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Services
{
    public interface IAdvanceDisbursementService
    {
        Task<TravelAdvanceResponseDto> DisburseAdvanceAsync(Guid advanceId, DisburseAdvancePaymentDto dto, Guid currentUserId);
    }

    public class AdvanceDisbursementService : IAdvanceDisbursementService
    {
        private readonly ApplicationDbContext _context;

        public AdvanceDisbursementService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TravelAdvanceResponseDto> DisburseAdvanceAsync(Guid advanceId, DisburseAdvancePaymentDto dto, Guid currentUserId)
        {
            if (string.IsNullOrWhiteSpace(dto.ReferenceNumber))
            {
                throw new InvalidOperationException("Nomor referensi bukti pencairan kasir / transfer bank wajib diisi (VR-TRV-21)");
            }

            var advance = await _context.TrxTravelAdvanceRequests
                .Include(a => a.BusinessTravelRequest)
                .FirstOrDefaultAsync(a => a.Id == advanceId);

            if (advance == null) throw new KeyNotFoundException("Permohonan uang muka tidak ditemukan");

            // VR-TRV-03: Wajib disetujui 2-pintu sebelum dicairkan
            if (advance.AdvanceStatus != "Approved")
            {
                throw new InvalidOperationException($"Uang muka berstatus '{advance.AdvanceStatus}' belum dapat dicairkan. Wajib memperoleh persetujuan Finance (Approved) terlebih dahulu (VR-TRV-03)");
            }

            var now = DateTime.UtcNow;

            advance.AdvanceStatus = "Paid";
            advance.PaidAmount = advance.ApprovedAmount;
            advance.PaidAt = now;
            advance.Notes = $"{advance.Notes} [Dicairkan Kasir via {dto.PaymentMethod}, Ref: {dto.ReferenceNumber}]";
            advance.UpdateBy = currentUserId;
            advance.UpdateDateTime = now;

            // Update status SPPD menjadi AdvancePaid
            if (advance.BusinessTravelRequest != null)
            {
                advance.BusinessTravelRequest.TravelStatus = "AdvancePaid";
                advance.BusinessTravelRequest.UpdateBy = currentUserId;
                advance.BusinessTravelRequest.UpdateDateTime = now;
            }

            // Update atau catat pembayaran kasir
            var payment = await _context.TrxTravelAdvancePayments
                .FirstOrDefaultAsync(p => p.TravelAdvanceRequestId == advance.Id && p.PaymentStatus == "Pending");

            if (payment != null)
            {
                payment.PaymentStatus = "Paid";
                payment.BankReferenceNumber = dto.ReferenceNumber;
                payment.FinanceReferenceNumber = dto.ReferenceNumber;
                payment.PaidAt = now;
                payment.ProcessedAt = now;
                payment.ProcessedByUserId = currentUserId;
                payment.UpdateBy = currentUserId;
                payment.UpdateDateTime = now;
            }

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
                PaidAt = advance.PaidAt,
                BankReferenceNumber = dto.ReferenceNumber,
                Notes = advance.Notes
            };
        }
    }
}
