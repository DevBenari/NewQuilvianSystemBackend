using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Services
{
    public class PlataranSehatSkpService : IPlataranSehatSkpService
    {
        private readonly ApplicationDbContext _context;

        public PlataranSehatSkpService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PlataranSehatSkpRecordDto> RecordSkpActivityAsync(RecordSkpActivityRequestDto request, string currentUserId)
        {
            var staff = await _context.MstWorkforceProfiles
                .FirstOrDefaultAsync(w => w.Id == request.WorkforceProfileId && !w.IsDelete);

            if (staff == null)
            {
                throw new KeyNotFoundException($"Profil tenaga kerja ID '{request.WorkforceProfileId}' tidak ditemukan.");
            }

            var category = request.SkpCategory?.Trim();
            if (string.IsNullOrWhiteSpace(category) ||
                (!category.Equals("Pembelajaran", StringComparison.OrdinalIgnoreCase) &&
                 !category.Equals("Pelayanan", StringComparison.OrdinalIgnoreCase) &&
                 !category.Equals("Pengabdian", StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("Kategori SKP wajib berupa 'Pembelajaran', 'Pelayanan', atau 'Pengabdian' sesuai regulasi Kemenkes (VR-TRN-23).");
            }

            var parsedUserGuid = Guid.TryParse(currentUserId, out var uGuid) ? uGuid : Guid.Empty;
            bool isVerifiedPlataran = !string.IsNullOrWhiteSpace(request.PlataranSehatCertificateId);

            var record = new HrdPlataranSehatSkpRecord
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = request.WorkforceProfileId,
                TrainingCertificateId = request.TrainingCertificateId,
                ActivityName = request.ActivityName,
                SkpCategory = category,
                SkpPoints = request.SkpPoints,
                ActivityDate = request.ActivityDate != default ? request.ActivityDate : DateTime.UtcNow,
                OrganizerName = request.OrganizerName ?? "Kemenkes / Fasilitas Pelayanan Kesehatan",
                PlataranSehatCertificateId = request.PlataranSehatCertificateId,
                IsVerifiedByPlataranSehat = isVerifiedPlataran,
                PlataranSehatVerifiedAt = isVerifiedPlataran ? DateTime.UtcNow : null,
                IsSyncedToOppe = false,
                EvidenceDocumentUrl = request.EvidenceDocumentUrl,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = parsedUserGuid
            };

            await _context.Set<HrdPlataranSehatSkpRecord>().AddAsync(record);
            await _context.SaveChangesAsync();

            return new PlataranSehatSkpRecordDto
            {
                Id = record.Id,
                WorkforceProfileId = record.WorkforceProfileId,
                StaffName = staff.DisplayName,
                TrainingCertificateId = record.TrainingCertificateId,
                ActivityName = record.ActivityName,
                SkpCategory = record.SkpCategory,
                SkpPoints = record.SkpPoints,
                ActivityDate = record.ActivityDate,
                OrganizerName = record.OrganizerName,
                PlataranSehatCertificateId = record.PlataranSehatCertificateId,
                IsVerifiedByPlataranSehat = record.IsVerifiedByPlataranSehat,
                PlataranSehatVerifiedAt = record.PlataranSehatVerifiedAt,
                IsSyncedToOppe = record.IsSyncedToOppe,
                OppeSyncedAt = record.OppeSyncedAt,
                EvidenceDocumentUrl = record.EvidenceDocumentUrl,
                CreatedAt = record.CreateDateTime
            };
        }

        public async Task<SkpPortfolioSummaryDto> GetStaffSkpPortfolioAsync(Guid workforceProfileId)
        {
            var staff = await _context.MstWorkforceProfiles
                .FirstOrDefaultAsync(w => w.Id == workforceProfileId && !w.IsDelete);

            if (staff == null)
            {
                throw new KeyNotFoundException($"Profil tenaga kerja ID '{workforceProfileId}' tidak ditemukan.");
            }

            var records = await _context.Set<HrdPlataranSehatSkpRecord>()
                .Where(r => r.WorkforceProfileId == workforceProfileId && !r.IsDelete)
                .OrderByDescending(r => r.ActivityDate)
                .ToListAsync();

            var totalEarned = records.Sum(r => r.SkpPoints);
            var pembEarned = records.Where(r => r.SkpCategory.Equals("Pembelajaran", StringComparison.OrdinalIgnoreCase)).Sum(r => r.SkpPoints);
            var pelEarned = records.Where(r => r.SkpCategory.Equals("Pelayanan", StringComparison.OrdinalIgnoreCase)).Sum(r => r.SkpPoints);
            var pengEarned = records.Where(r => r.SkpCategory.Equals("Pengabdian", StringComparison.OrdinalIgnoreCase)).Sum(r => r.SkpPoints);

            string statusText = totalEarned >= 50.00m
                ? "Target 50 SKP Kemenkes terpenuhi (100% siap untuk perpanjangan SIP/STR)."
                : $"Pencapaian SKP: {totalEarned:F2} dari target 50.00 SKP (Sisa kekurangan: {(50.00m - totalEarned):F2} SKP).";

            return new SkpPortfolioSummaryDto
            {
                WorkforceProfileId = staff.Id,
                StaffName = staff.DisplayName,
                TotalSkpTarget = 50.00m,
                TotalSkpEarned = totalEarned,
                SkpPembelajaranEarned = pembEarned,
                SkpPelayananEarned = pelEarned,
                SkpPengabdianEarned = pengEarned,
                TotalActivitiesCount = records.Count,
                VerifiedByPlataranSehatCount = records.Count(r => r.IsVerifiedByPlataranSehat),
                SyncedToOppeCount = records.Count(r => r.IsSyncedToOppe),
                StatusSummary = statusText,
                Records = records.Select(r => new PlataranSehatSkpRecordDto
                {
                    Id = r.Id,
                    WorkforceProfileId = r.WorkforceProfileId,
                    StaffName = staff.DisplayName,
                    TrainingCertificateId = r.TrainingCertificateId,
                    ActivityName = r.ActivityName,
                    SkpCategory = r.SkpCategory,
                    SkpPoints = r.SkpPoints,
                    ActivityDate = r.ActivityDate,
                    OrganizerName = r.OrganizerName,
                    PlataranSehatCertificateId = r.PlataranSehatCertificateId,
                    IsVerifiedByPlataranSehat = r.IsVerifiedByPlataranSehat,
                    PlataranSehatVerifiedAt = r.PlataranSehatVerifiedAt,
                    IsSyncedToOppe = r.IsSyncedToOppe,
                    OppeSyncedAt = r.OppeSyncedAt,
                    EvidenceDocumentUrl = r.EvidenceDocumentUrl,
                    CreatedAt = r.CreateDateTime
                }).ToList()
            };
        }

        public async Task<SyncOppeResponseDto> SyncStaffSkpToOppeAsync(Guid workforceProfileId, string currentUserId)
        {
            var staff = await _context.MstWorkforceProfiles
                .FirstOrDefaultAsync(w => w.Id == workforceProfileId && !w.IsDelete);

            if (staff == null)
            {
                throw new KeyNotFoundException($"Profil tenaga kerja ID '{workforceProfileId}' tidak ditemukan.");
            }

            var unSyncedRecords = await _context.Set<HrdPlataranSehatSkpRecord>()
                .Where(r => r.WorkforceProfileId == workforceProfileId && !r.IsSyncedToOppe && !r.IsDelete)
                .ToListAsync();

            var parsedUserGuid = Guid.TryParse(currentUserId, out var uGuid) ? uGuid : Guid.Empty;
            var now = DateTime.UtcNow;

            foreach (var record in unSyncedRecords)
            {
                record.IsSyncedToOppe = true;
                record.OppeSyncedAt = now;
                record.UpdateDateTime = now;
                record.UpdateBy = parsedUserGuid;
            }

            await _context.SaveChangesAsync();

            var totalPoints = unSyncedRecords.Sum(r => r.SkpPoints);

            return new SyncOppeResponseDto
            {
                WorkforceProfileId = staff.Id,
                StaffName = staff.DisplayName,
                SyncedRecordsCount = unSyncedRecords.Count,
                TotalSyncedSkpPoints = totalPoints,
                SyncedAt = now,
                Message = $"Berhasil menyinkronkan {unSyncedRecords.Count} rekaman SKP ({totalPoints:F2} SKP) ke modul evaluasi praktik profesional OPPE Sub-15 (INT-HR-78)."
            };
        }

        public async Task<PlataranSehatSkpRecordDto> VerifyPlataranSehatAsync(Guid skpRecordId, string currentUserId)
        {
            var record = await _context.Set<HrdPlataranSehatSkpRecord>()
                .Include(r => r.WorkforceProfile)
                .FirstOrDefaultAsync(r => r.Id == skpRecordId && !r.IsDelete);

            if (record == null)
            {
                throw new KeyNotFoundException($"Rekaman SKP ID '{skpRecordId}' tidak ditemukan.");
            }

            var parsedUserGuid = Guid.TryParse(currentUserId, out var uGuid) ? uGuid : Guid.Empty;
            record.IsVerifiedByPlataranSehat = true;
            record.PlataranSehatVerifiedAt = DateTime.UtcNow;
            record.UpdateDateTime = DateTime.UtcNow;
            record.UpdateBy = parsedUserGuid;

            await _context.SaveChangesAsync();

            return new PlataranSehatSkpRecordDto
            {
                Id = record.Id,
                WorkforceProfileId = record.WorkforceProfileId,
                StaffName = record.WorkforceProfile?.DisplayName,
                TrainingCertificateId = record.TrainingCertificateId,
                ActivityName = record.ActivityName,
                SkpCategory = record.SkpCategory,
                SkpPoints = record.SkpPoints,
                ActivityDate = record.ActivityDate,
                OrganizerName = record.OrganizerName,
                PlataranSehatCertificateId = record.PlataranSehatCertificateId,
                IsVerifiedByPlataranSehat = record.IsVerifiedByPlataranSehat,
                PlataranSehatVerifiedAt = record.PlataranSehatVerifiedAt,
                IsSyncedToOppe = record.IsSyncedToOppe,
                OppeSyncedAt = record.OppeSyncedAt,
                EvidenceDocumentUrl = record.EvidenceDocumentUrl,
                CreatedAt = record.CreateDateTime
            };
        }
    }
}
