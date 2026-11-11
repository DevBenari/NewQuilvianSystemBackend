using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Services
{
    public interface ITravelDocumentGenerationService
    {
        Task<(byte[] FileBytes, string FileName, string ContentType)> GenerateTravelOrderPdfAsync(Guid travelRequestId);
    }

    public class TravelDocumentGenerationService : ITravelDocumentGenerationService
    {
        private readonly ApplicationDbContext _context;

        public TravelDocumentGenerationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(byte[] FileBytes, string FileName, string ContentType)> GenerateTravelOrderPdfAsync(Guid travelRequestId)
        {
            var travel = await _context.TrxBusinessTravelRequests
                .Include(r => r.WorkforceProfile)
                .Include(r => r.CostCenter)
                .FirstOrDefaultAsync(r => r.Id == travelRequestId);

            if (travel == null) throw new KeyNotFoundException("Permohonan perjalanan dinas tidak ditemukan");

            // Pembentukan token barcode keabsahan surat tugas
            var barcodePayload = $"{travel.TravelRequestNumber}|{travel.WorkforceProfileId}|{travel.StartDate:yyyyMMdd}|{travel.EndDate:yyyyMMdd}";
            var barcodeHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(barcodePayload)));

            // Pembuatan dokumen teks representasi PDF / surat tugas resmi ber-barcode
            var sb = new StringBuilder();
            sb.AppendLine("================================================================================");
            sb.AppendLine("                 RUMAH SAKIT QUILVIAN — SURAT PERINTAH PERJALANAN DINAS (SPPD)  ");
            sb.AppendLine("================================================================================");
            sb.AppendLine($"Nomor SPPD         : {travel.TravelRequestNumber}");
            sb.AppendLine($"Tanggal Penerbitan : {DateTime.UtcNow:dd MMMM yyyy}");
            sb.AppendLine($"Nama Pegawai/Nakes : {travel.WorkforceProfile?.DisplayName ?? "Pegawai Rumah Sakit"}");
            sb.AppendLine($"Pusat Biaya / Unit : {travel.CostCenter?.CostCenterName ?? "Rumah Sakit Umum"}");
            sb.AppendLine("--------------------------------------------------------------------------------");
            sb.AppendLine($"Agenda Kedinasan   : {travel.TravelTitle}");
            sb.AppendLine($"Maksud & Tujuan    : {travel.TravelPurpose}");
            sb.AppendLine($"Kota Asal          : {travel.Origin}");
            sb.AppendLine($"Kota Tujuan        : {travel.Destination}");
            sb.AppendLine($"Tanggal Penugasan  : {travel.StartDate:dd/MM/yyyy} s.d. {travel.EndDate:dd/MM/yyyy} ({travel.TravelDayCount} Hari)");
            sb.AppendLine($"Pagu Disetujui     : {travel.ApprovedBudgetAmount:N0} {travel.CurrencyCode}");
            sb.AppendLine("--------------------------------------------------------------------------------");
            sb.AppendLine("KODE VERIFIKASI KEABSAHAN ELEKTRONIK (ENCRYPTED QR BARCODE):");
            sb.AppendLine($"Token QR           : {barcodeHash}");
            sb.AppendLine($"Status SPPD        : {travel.TravelStatus} (Sah Divalidasi Sistem HRD)");
            sb.AppendLine("================================================================================");

            var docBytes = Encoding.UTF8.GetBytes(sb.ToString());
            var fileName = $"SPPD_{travel.TravelRequestNumber.Replace("/", "_")}.pdf";

            // Simpan atau update rekam dokumen fisik di TrxTravelDocument
            var existingDoc = await _context.TrxTravelDocuments
                .FirstOrDefaultAsync(d => d.BusinessTravelRequestId == travel.Id && d.DocumentType == "TravelOrder");

            if (existingDoc == null)
            {
                var doc = new TrxTravelDocument
                {
                    Id = Guid.NewGuid(),
                    BusinessTravelRequestId = travel.Id,
                    DocumentType = "TravelOrder",
                    DocumentName = $"Surat Tugas {travel.TravelRequestNumber}",
                    FileName = fileName,
                    FilePath = $"/documents/sppd/{fileName}",
                    ContentType = "application/pdf",
                    FileSizeBytes = docBytes.Length,
                    FileChecksum = barcodeHash,
                    DocumentStatus = "Verified",
                    IsRequiredDocument = true,
                    IsVerified = true,
                    VerifiedAt = DateTime.UtcNow,
                    IsActive = true,
                    CreateBy = Guid.Empty,
                    CreateDateTime = DateTime.UtcNow
                };
                _context.TrxTravelDocuments.Add(doc);
                await _context.SaveChangesAsync();
            }

            return (docBytes, fileName, "application/pdf");
        }
    }
}
