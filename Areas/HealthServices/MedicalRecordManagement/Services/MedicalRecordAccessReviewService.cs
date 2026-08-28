using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services
{
    /// <summary>
    /// Melayani tinjauan jejak akses oleh unit rekam medis (RM-DEC-005).
    ///
    /// Inilah yang membuat jejak akses berguna alih-alih sekadar menumpuk. Tanpa tinjauan,
    /// mencatat siapa membuka apa hanya menghasilkan tabel yang tidak pernah dibaca.
    ///
    /// SATU-SATUNYA PERUBAHAN YANG DIIZINKAN pada jejak adalah menandainya sudah ditinjau. Isi
    /// jejak tidak dapat diubah, dan barisnya tidak dapat dihapus — jejak yang dapat diubah
    /// bukan jejak.
    /// </summary>
    public class MedicalRecordAccessReviewService
    {
        private readonly ApplicationDbContext _dbContext;

        public MedicalRecordAccessReviewService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Menandai satu jejak akses sudah ditinjau.
        /// </summary>
        public async Task<(IntegrityGuardResult Result, TrxMedicalRecordAccessLog? Log)> MarkReviewedAsync(
            Guid accessLogId,
            Guid reviewerUserId,
            string reviewNote,
            DateTime nowUtc,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(reviewNote))
            {
                return (IntegrityGuardResult.Denied(
                    StatusCodes.Status400BadRequest,
                    "Catatan tinjauan wajib diisi."), null);
            }

            var jejak = await _dbContext.Set<TrxMedicalRecordAccessLog>()
                .FirstOrDefaultAsync(x => x.Id == accessLogId, cancellationToken);

            if (jejak == null)
            {
                return (IntegrityGuardResult.Denied(
                    StatusCodes.Status404NotFound, "Jejak akses tidak ditemukan."), null);
            }

            // Akses yang memang tidak perlu ditinjau tidak boleh ditandai. Bila diizinkan,
            // angka pada laporan tinjauan menjadi tidak bermakna.
            if (!jejak.IsFlaggedForReview)
            {
                return (IntegrityGuardResult.Denied(
                    StatusCodes.Status400BadRequest,
                    "Akses ini tidak memerlukan tinjauan."), null);
            }

            if (jejak.ReviewedAt.HasValue)
            {
                return (IntegrityGuardResult.Denied(
                    StatusCodes.Status400BadRequest,
                    "Akses ini sudah ditinjau."), null);
            }

            jejak.ReviewedAt = nowUtc;
            jejak.ReviewedByUserId = reviewerUserId;
            jejak.ReviewNote = reviewNote.Trim()[..Math.Min(reviewNote.Trim().Length, 500)];
            jejak.UpdateDateTime = nowUtc;
            jejak.UpdateBy = reviewerUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return (IntegrityGuardResult.Allowed(), jejak);
        }

        /// <summary>
        /// Rekap jumlah akses dalam satu rentang waktu.
        ///
        /// Angka ini yang memberi tahu apakah aturan akses bekerja sebagaimana dimaksud. Bila
        /// hampir seluruh akses berjenis beralasan, berarti definisi pasien rawatan terlalu
        /// sempit dan justru menghambat pelayanan — dan itu perlu ditinjau ulang, bukan
        /// dibiarkan.
        /// </summary>
        public async Task<MedicalRecordAccessSummaryResponse> SummaryAsync(
            DateTime startDate,
            DateTime endDate,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Set<TrxMedicalRecordAccessLog>()
                .AsNoTracking()
                .Where(x => x.AccessedAt >= startDate && x.AccessedAt <= endDate);

            return new MedicalRecordAccessSummaryResponse
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalAkses = await query.CountAsync(cancellationToken),
                AksesRawatan = await query.CountAsync(
                    x => x.AccessType == MedicalRecordAccessType.RoutineCare, cancellationToken),
                AksesBeralasan = await query.CountAsync(
                    x => x.AccessType == MedicalRecordAccessType.ReasonedAccess, cancellationToken),
                AksesCatatanPribadi = await query.CountAsync(
                    x => x.AccessScope == MedicalRecordAccessScope.PrivateNote, cancellationToken),
                PerluDitinjau = await query.CountAsync(
                    x => x.IsFlaggedForReview, cancellationToken),
                SudahDitinjau = await query.CountAsync(
                    x => x.IsFlaggedForReview && x.ReviewedAt != null, cancellationToken),
                BelumDitinjau = await query.CountAsync(
                    x => x.IsFlaggedForReview && x.ReviewedAt == null, cancellationToken),
                JumlahPenggunaBerbeda = await query
                    .Select(x => x.UserId).Distinct().CountAsync(cancellationToken),
                JumlahPasienBerbeda = await query
                    .Select(x => x.PatientId).Distinct().CountAsync(cancellationToken)
            };
        }
    }
}
