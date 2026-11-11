using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Models;
using QuilvianSystemBackend.Repositories;
using System.Security.Cryptography;
using System.Text;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Services
{
    public class TrainingCertificationService : ITrainingCertificationService
    {
        private readonly ApplicationDbContext _context;

        public TrainingCertificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TrainingCertificateDto> IssueCertificateAsync(IssueCertificateRequestDto request, string currentUserId)
        {
            var participant = await _context.TrxTrainingParticipants
                .Include(p => p.TrainingSession)
                .Include(p => p.WorkforceProfile)
                .FirstOrDefaultAsync(p => p.Id == request.TrainingParticipantId && !p.IsDelete);

            if (participant == null)
            {
                throw new KeyNotFoundException($"Peserta pelatihan dengan ID '{request.TrainingParticipantId}' tidak ditemukan.");
            }

            // Pastikan peserta telah lulus asesmen (post-test atau OSCE)
            var hasPassed = await _context.TrxTrainingAssessments
                .AnyAsync(a => a.TrainingSessionId == participant.TrainingSessionId &&
                               a.WorkforceProfileId == participant.WorkforceProfileId &&
                               a.IsPassed &&
                               !a.IsDelete);

            if (!hasPassed && participant.ParticipantStatus != "PassedExam" && participant.ParticipantStatus != "PassedOsce")
            {
                throw new InvalidOperationException($"Penerbitan sertifikat ditolak: Peserta {participant.WorkforceProfile?.DisplayName} belum memenuhi syarat kelulusan ujian / OSCE.");
            }

            var parsedUserGuid = Guid.TryParse(currentUserId, out var uGuid) ? uGuid : Guid.Empty;
            var currentYear = DateTime.UtcNow.Year;

            // Generate nomor seri sertifikat: CERT-{TypeCode}-{Year}-{Sequence:D6}
            var typeCode = !string.IsNullOrWhiteSpace(participant.TrainingSession?.SessionCode)
                ? participant.TrainingSession.SessionCode.Split('-')[0]
                : "TRN";

            var currentYearCount = await _context.TrxTrainingCertificates
                .CountAsync(c => c.IssuedDate.Year == currentYear);

            var sequence = currentYearCount + 1;
            var certNumber = $"CERT-{typeCode}-{currentYear}-{sequence:D6}";

            // Hitung checksum sertifikat
            var rawChecksumInput = $"{certNumber}:{participant.WorkforceProfileId}:{DateTime.UtcNow:O}";
            string checksumHex;
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawChecksumInput));
                checksumHex = BitConverter.ToString(hashBytes).Replace("-", "").ToUpperInvariant();
            }

            var filePath = !string.IsNullOrWhiteSpace(request.CertificateFilePath)
                ? request.CertificateFilePath
                : $"/storage/certificates/{currentYear}/{certNumber}.pdf";

            var verificationUrl = $"https://hospital.quilvian.id/verify-cert?no={certNumber}&hash={checksumHex.Substring(0, 8)}";

            // 1. Buat atau update TrxTrainingResult
            var trainingResult = await _context.TrxTrainingResults
                .FirstOrDefaultAsync(r => r.TrainingParticipantId == participant.Id && !r.IsDelete);

            if (trainingResult == null)
            {
                trainingResult = new TrxTrainingResult
                {
                    Id = Guid.NewGuid(),
                    TrainingParticipantId = participant.Id,
                    TrainingSessionId = participant.TrainingSessionId,
                    WorkforceProfileId = participant.WorkforceProfileId,
                    ResultDate = DateTime.UtcNow,
                    ResultStatus = "Passed",
                    FinalScore = participant.FinalScore ?? 85.00m,
                    AttendancePercentage = participant.AttendancePercentage,
                    IsPassed = true,
                    IsCompleted = true,
                    CreditPointEarned = request.CreditPointEarned,
                    Notes = $"Lulus diklat & evaluasi. Sertifikat diterbitkan dengan nomor: {certNumber}",
                    IsActive = true,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = parsedUserGuid
                };
                await _context.TrxTrainingResults.AddAsync(trainingResult);
            }
            else
            {
                trainingResult.ResultStatus = "Passed";
                trainingResult.IsPassed = true;
                trainingResult.IsCompleted = true;
                trainingResult.CreditPointEarned = request.CreditPointEarned;
                trainingResult.UpdateDateTime = DateTime.UtcNow;
                trainingResult.UpdateBy = parsedUserGuid;
            }

            // 2. Sinkronisasi ke portofolio e-Dossier Sub-02 Workforce Profile (WfpTrainingRecord, VR-TRN-19)
            var trainingRecord = new WfpTrainingRecord
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = participant.WorkforceProfileId,
                TrainingParticipantId = participant.Id,
                TrainingType = "InternalHospital",
                TrainingName = participant.TrainingSession?.SessionName ?? "Pelatihan Kompetensi Rumah Sakit",
                Organizer = request.IssuerName ?? "RS Quilvian Learning & Development Center",
                Location = participant.TrainingSession?.Venue ?? "RS Quilvian",
                StartDate = participant.TrainingSession?.StartDateTime ?? DateTime.UtcNow,
                EndDate = participant.TrainingSession?.EndDateTime ?? DateTime.UtcNow,
                CertificateNumber = certNumber,
                CreditPoint = request.CreditPointEarned,
                FilePath = filePath,
                FileContentType = "application/pdf",
                IsVerified = true,
                VerifiedAt = DateTime.UtcNow,
                VerifiedByUserId = parsedUserGuid,
                IsMandatory = participant.IsMandatory,
                IsActive = true,
                Description = $"Sinkronisasi otomatis e-Dossier dari Diklat Sesi {participant.TrainingSession?.SessionCode}",
                CreateDateTime = DateTime.UtcNow,
                CreateBy = parsedUserGuid
            };
            await _context.WfpTrainingRecords.AddAsync(trainingRecord);

            // 3. Buat entitas TrxTrainingCertificate
            var certificate = new TrxTrainingCertificate
            {
                Id = Guid.NewGuid(),
                TrainingParticipantId = participant.Id,
                TrainingResultId = trainingResult.Id,
                WorkforceProfileId = participant.WorkforceProfileId,
                CertificationTypeId = request.CertificationTypeId,
                TrainingRecordId = trainingRecord.Id,
                CertificateNumber = certNumber,
                IssuedDate = DateTime.UtcNow,
                ExpiredDate = DateTime.UtcNow.AddYears(request.ValidityYears > 0 ? request.ValidityYears : 3),
                IssuerName = request.IssuerName ?? "RS Quilvian Learning & Development Center",
                CertificateFilePath = filePath,
                FileContentType = "application/pdf",
                FileChecksum = checksumHex,
                VerificationUrl = verificationUrl,
                IsVerified = true,
                VerifiedByUserId = parsedUserGuid,
                VerifiedAt = DateTime.UtcNow,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = parsedUserGuid
            };
            await _context.TrxTrainingCertificates.AddAsync(certificate);

            // Perbarui status peserta menjadi Certified
            participant.ParticipantStatus = "Certified";
            participant.CompletedAt = DateTime.UtcNow;
            participant.UpdateDateTime = DateTime.UtcNow;
            participant.UpdateBy = parsedUserGuid;

            await _context.SaveChangesAsync();

            return new TrainingCertificateDto
            {
                Id = certificate.Id,
                TrainingParticipantId = certificate.TrainingParticipantId,
                WorkforceProfileId = certificate.WorkforceProfileId,
                StaffName = participant.WorkforceProfile?.DisplayName,
                TrainingSessionName = participant.TrainingSession?.SessionName ?? string.Empty,
                CertificateNumber = certificate.CertificateNumber,
                IssuedDate = certificate.IssuedDate,
                ExpiredDate = certificate.ExpiredDate,
                IssuerName = certificate.IssuerName,
                CertificateFilePath = certificate.CertificateFilePath,
                FileChecksum = certificate.FileChecksum,
                VerificationUrl = certificate.VerificationUrl,
                IsVerified = certificate.IsVerified,
                TrainingRecordId = certificate.TrainingRecordId,
                IsSyncedToEdossier = true,
                CreatedAt = certificate.CreateDateTime
            };
        }

        public async Task<TrainingCertificateDto?> GetCertificateByNumberAsync(string certificateNumber)
        {
            var cert = await _context.TrxTrainingCertificates
                .Include(c => c.WorkforceProfile)
                .Include(c => c.TrainingParticipant)
                    .ThenInclude(p => p != null ? p.TrainingSession : null)
                .FirstOrDefaultAsync(c => c.CertificateNumber == certificateNumber && !c.IsDelete);

            if (cert == null) return null;

            return new TrainingCertificateDto
            {
                Id = cert.Id,
                TrainingParticipantId = cert.TrainingParticipantId,
                WorkforceProfileId = cert.WorkforceProfileId,
                StaffName = cert.WorkforceProfile?.DisplayName,
                TrainingSessionName = cert.TrainingParticipant?.TrainingSession?.SessionName ?? string.Empty,
                CertificateNumber = cert.CertificateNumber,
                IssuedDate = cert.IssuedDate,
                ExpiredDate = cert.ExpiredDate,
                IssuerName = cert.IssuerName,
                CertificateFilePath = cert.CertificateFilePath,
                FileChecksum = cert.FileChecksum,
                VerificationUrl = cert.VerificationUrl,
                IsVerified = cert.IsVerified,
                TrainingRecordId = cert.TrainingRecordId,
                IsSyncedToEdossier = cert.TrainingRecordId.HasValue,
                CreatedAt = cert.CreateDateTime
            };
        }

        public async Task<List<TrainingCertificateDto>> GetCertificatesByParticipantAsync(Guid participantId)
        {
            var certs = await _context.TrxTrainingCertificates
                .Include(c => c.WorkforceProfile)
                .Include(c => c.TrainingParticipant)
                    .ThenInclude(p => p != null ? p.TrainingSession : null)
                .Where(c => c.TrainingParticipantId == participantId && !c.IsDelete)
                .OrderByDescending(c => c.IssuedDate)
                .ToListAsync();

            return certs.Select(c => new TrainingCertificateDto
            {
                Id = c.Id,
                TrainingParticipantId = c.TrainingParticipantId,
                WorkforceProfileId = c.WorkforceProfileId,
                StaffName = c.WorkforceProfile?.DisplayName,
                TrainingSessionName = c.TrainingParticipant?.TrainingSession?.SessionName ?? string.Empty,
                CertificateNumber = c.CertificateNumber,
                IssuedDate = c.IssuedDate,
                ExpiredDate = c.ExpiredDate,
                IssuerName = c.IssuerName,
                CertificateFilePath = c.CertificateFilePath,
                FileChecksum = c.FileChecksum,
                VerificationUrl = c.VerificationUrl,
                IsVerified = c.IsVerified,
                TrainingRecordId = c.TrainingRecordId,
                IsSyncedToEdossier = c.TrainingRecordId.HasValue,
                CreatedAt = c.CreateDateTime
            }).ToList();
        }

        public async Task<List<TrainingCertificateDto>> GetCertificatesByStaffAsync(Guid workforceProfileId)
        {
            var certs = await _context.TrxTrainingCertificates
                .Include(c => c.WorkforceProfile)
                .Include(c => c.TrainingParticipant)
                    .ThenInclude(p => p != null ? p.TrainingSession : null)
                .Where(c => c.WorkforceProfileId == workforceProfileId && !c.IsDelete)
                .OrderByDescending(c => c.IssuedDate)
                .ToListAsync();

            return certs.Select(c => new TrainingCertificateDto
            {
                Id = c.Id,
                TrainingParticipantId = c.TrainingParticipantId,
                WorkforceProfileId = c.WorkforceProfileId,
                StaffName = c.WorkforceProfile?.DisplayName,
                TrainingSessionName = c.TrainingParticipant?.TrainingSession?.SessionName ?? string.Empty,
                CertificateNumber = c.CertificateNumber,
                IssuedDate = c.IssuedDate,
                ExpiredDate = c.ExpiredDate,
                IssuerName = c.IssuerName,
                CertificateFilePath = c.CertificateFilePath,
                FileChecksum = c.FileChecksum,
                VerificationUrl = c.VerificationUrl,
                IsVerified = c.IsVerified,
                TrainingRecordId = c.TrainingRecordId,
                IsSyncedToEdossier = c.TrainingRecordId.HasValue,
                CreatedAt = c.CreateDateTime
            }).ToList();
        }
    }
}
