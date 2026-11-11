using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Services
{
    public class DocumentAutoNumberingService : IDocumentAutoNumberingService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<DocumentAutoNumberingService> _logger;

        private static readonly string[] RomanMonths = 
        {
            "", "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X", "XI", "XII"
        };

        public DocumentAutoNumberingService(ApplicationDbContext dbContext, ILogger<DocumentAutoNumberingService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<string> GenerateOfficialDocumentNumberAsync(Guid documentTypeId, CancellationToken cancellationToken = default)
        {
            var executionStrategy = _dbContext.Database.CreateExecutionStrategy();

            return await executionStrategy.ExecuteAsync(async () =>
            {
                using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
                try
                {
                    MstEmployeeDocumentType? docType = null;

                    // Di database PostgreSQL, gunakan row-level locking "FOR UPDATE"
                    if (_dbContext.Database.IsNpgsql())
                    {
                        docType = await _dbContext.MstEmployeeDocumentTypes
                            .FromSqlRaw("SELECT * FROM public.\"MstEmployeeDocumentType\" WHERE \"Id\" = {0} FOR UPDATE", documentTypeId)
                            .FirstOrDefaultAsync(cancellationToken);
                    }
                    else
                    {
                        docType = await _dbContext.MstEmployeeDocumentTypes
                            .FirstOrDefaultAsync(x => x.Id == documentTypeId && !x.IsDelete, cancellationToken);
                    }

                    if (docType == null)
                    {
                        throw new InvalidOperationException($"Tipe dokumen dengan ID {documentTypeId} tidak ditemukan");
                    }

                    var now = DateTime.UtcNow;
                    var currentYear = now.Year;
                    var romanMonth = RomanMonths[now.Month];

                    // Reset nomor urut jika terjadi pergantian tahun kalender
                    if (docType.CurrentYear != currentYear)
                    {
                        docType.CurrentYear = currentYear;
                        docType.LastSequenceNumber = 0;
                    }

                    // Kenaikan nomor urut secara atomik
                    docType.LastSequenceNumber += 1;
                    var sequenceFormatted = docType.LastSequenceNumber.ToString("D5");

                    var pattern = string.IsNullOrWhiteSpace(docType.NumberingPattern)
                        ? "[KODE]/RSQ-SDM/[BULAN_ROMAWI]/[TAHUN]/[NO_URUT]"
                        : docType.NumberingPattern;

                    var officialNumber = pattern
                        .Replace("[KODE]", docType.DocumentTypeCode, StringComparison.OrdinalIgnoreCase)
                        .Replace("[BULAN_ROMAWI]", romanMonth, StringComparison.OrdinalIgnoreCase)
                        .Replace("[TAHUN]", currentYear.ToString(), StringComparison.OrdinalIgnoreCase)
                        .Replace("[NO_URUT]", sequenceFormatted, StringComparison.OrdinalIgnoreCase);

                    docType.UpdateDateTime = now;
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    _logger.LogInformation("[NUMBERING-HRS-01] Berhasil menerbitkan nomor surat resmi: {OfficialNumber} (DocType: {DocCode})",
                        officialNumber, docType.DocumentTypeCode);

                    return officialNumber;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    _logger.LogError(ex, "[NUMBERING-HRS-ERR] Gagal menerbitkan nomor surat resmi atomik untuk DocType ID: {DocTypeId}", documentTypeId);
                    throw;
                }
            });
        }

        public async Task<string> GenerateTicketNumberAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var prefix = $"TKT-{now:yyyyMM}-";

            // Hitung tiket yang sudah ada di bulan berjalan
            var count = await _dbContext.TrxHrServiceRequests
                .CountAsync(x => x.RequestNumber.StartsWith(prefix), cancellationToken);

            var nextSequence = count + 1;
            var ticketNumber = $"{prefix}{nextSequence:D5}";

            // Pengecekan tabrakan jika ada konkurensi paralel
            while (await _dbContext.TrxHrServiceRequests.AnyAsync(x => x.RequestNumber == ticketNumber, cancellationToken))
            {
                nextSequence++;
                ticketNumber = $"{prefix}{nextSequence:D5}";
            }

            return ticketNumber;
        }
    }
}
