using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;
using QuilvianSystemBackend.Repositories;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services
{
    /// <summary>
    /// Service penanganan surat keterangan sakit dokter ber-SIP dan masking privasi diagnosa medis K3RS
    /// (HRD-DEC-112, LEV-B3, LEV-W3, VR-LEV-06, VR-LEV-07, VR-LEV-08, VR-LEV-09).
    /// </summary>
    public class MedicalCertificateVerificationService
    {
        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB
        private static readonly string[] AllowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png" };
        private const string MaskedDiagnosisPlaceholder = "[DIAGNOSIS_PROTECTED_CONFIDENTIAL]";

        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<MedicalCertificateVerificationService> _logger;

        public MedicalCertificateVerificationService(
            ApplicationDbContext dbContext,
            ILogger<MedicalCertificateVerificationService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Validasi kewajiban surat dokter: Cuti sakit 1 hari diizinkan tanpa surat dokter;
        /// Cuti sakit >= 2 hari kerja wajib mengunggah surat dokter ber-SIP (LEV-B3, VR-LEV-06).
        /// </summary>
        public (bool IsValid, string? ErrorMessage) ValidateMedicalCertificateRequirement(
            decimal requestedDays,
            bool isSickLeave,
            bool hasMedicalAttachment)
        {
            if (!isSickLeave)
            {
                return (true, null);
            }

            if (requestedDays >= 2.0m && !hasMedicalAttachment)
            {
                return (false, "Cuti sakit 2 hari kerja atau lebih wajib melampirkan berkas Surat Keterangan Sakit resmi dokter (VR-LEV-06, LEV-B3).");
            }

            return (true, null);
        }

        /// <summary>
        /// Mengunggah dan mengaitkan berkas surat keterangan sakit dokter ke permohonan cuti.
        /// </summary>
        public async Task<LeaveBalanceLedgerServiceResult<MedicalCertificateDetailDto>> UploadAndAttachCertificateAsync(
            UploadMedicalCertificateRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                return LeaveBalanceLedgerServiceResult<MedicalCertificateDetailDto>.Fail(StatusCodes.Status400BadRequest, "Request tidak boleh kosong.");
            }

            var leaveRequest = await _dbContext.Set<WfpLeaveRequest>()
                .FirstOrDefaultAsync(r => r.Id == request.LeaveRequestId && !r.IsDelete, cancellationToken);

            if (leaveRequest == null)
            {
                return LeaveBalanceLedgerServiceResult<MedicalCertificateDetailDto>.Fail(StatusCodes.Status404NotFound, "Permohonan cuti tidak ditemukan.");
            }

            // Validasi format file dan ukuran (VR-LEV-07)
            var extension = Path.GetExtension(request.OriginalFileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                return LeaveBalanceLedgerServiceResult<MedicalCertificateDetailDto>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Format berkas tidak didukung. Harap unggah berkas bertipe PDF, JPG, JPEG, atau PNG (VR-LEV-07).");
            }

            if (request.FileSizeBytes > MaxFileSizeBytes)
            {
                return LeaveBalanceLedgerServiceResult<MedicalCertificateDetailDto>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Ukuran berkas melebihi batas maksimal 5MB (VR-LEV-07).");
            }

            // Validasi kelengkapan metadata dokter dan faskes (VR-LEV-08)
            if (string.IsNullOrWhiteSpace(request.DoctorSipNumber) || string.IsNullOrWhiteSpace(request.FaskesName))
            {
                return LeaveBalanceLedgerServiceResult<MedicalCertificateDetailDto>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Nama faskes penerbit dan nomor Surat Izin Praktik (SIP) dokter pemeriksa wajib diisi (VR-LEV-08).");
            }

            var attachment = new TrxLeaveRequestAttachment
            {
                Id = Guid.NewGuid(),
                LeaveRequestId = request.LeaveRequestId,
                AttachmentType = "MedicalCertificate",
                OriginalFileName = request.OriginalFileName,
                FilePath = request.FilePath,
                ContentType = request.ContentType ?? "application/pdf",
                FileSizeBytes = request.FileSizeBytes,
                IsRequiredDocument = true,
                VerificationStatus = "Pending",
                IsMedicalCertificate = true,
                IsConfidentialMedical = true,
                DoctorName = request.DoctorName,
                DoctorSipNumber = request.DoctorSipNumber,
                FaskesName = request.FaskesName,
                MedicalDiagnosisIcd10 = request.MedicalDiagnosisIcd10,
                IsActive = true,
                CreateBy = actorUserId,
                CreateDateTime = DateTime.UtcNow
            };

            await _dbContext.Set<TrxLeaveRequestAttachment>().AddAsync(attachment, cancellationToken);

            // Sinkronkan metadata ke permohonan cuti
            leaveRequest.DoctorNoteFaskesName = request.FaskesName;
            leaveRequest.DoctorSipNumber = request.DoctorSipNumber;
            leaveRequest.IsDoctorNoteVerified = false;
            leaveRequest.UpdateBy = actorUserId;
            leaveRequest.UpdateDateTime = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Surat sakit dokter berhasil dilampirkan pada cuti {RequestId} oleh user {UserId}.", leaveRequest.Id, actorUserId);

            return LeaveBalanceLedgerServiceResult<MedicalCertificateDetailDto>.Ok(
                new MedicalCertificateDetailDto
                {
                    AttachmentId = attachment.Id,
                    LeaveRequestId = leaveRequest.Id,
                    RequestNumber = leaveRequest.RequestNumber,
                    FaskesName = attachment.FaskesName ?? string.Empty,
                    DoctorName = attachment.DoctorName ?? string.Empty,
                    DoctorSipNumber = attachment.DoctorSipNumber ?? string.Empty,
                    MedicalDiagnosis = MaskedDiagnosisPlaceholder,
                    IsDiagnosisMasked = true,
                    VerificationStatus = attachment.VerificationStatus,
                    OriginalFileName = attachment.OriginalFileName,
                    FilePath = attachment.FilePath
                },
                "Berkas surat keterangan sakit berhasil diunggah dan disimpan dengan proteksi privasi.");
        }

        /// <summary>
        /// Mengambil rincian berkas surat sakit dengan masking otomatis diagnosa klinis (LEV-W3, VR-LEV-09).
        /// Hanya petugas K3RS / HR Medis yang diizinkan melihat diagnosa ICD-10 asli.
        /// </summary>
        public async Task<LeaveBalanceLedgerServiceResult<MedicalCertificateDetailDto>> GetCertificateDetailAsync(
            Guid leaveRequestId,
            bool isAuthorizedK3RsOrAdmin,
            CancellationToken cancellationToken = default)
        {
            var attachment = await _dbContext.Set<TrxLeaveRequestAttachment>()
                .Include(a => a.LeaveRequest)
                .AsNoTracking()
                .Where(a => a.LeaveRequestId == leaveRequestId && a.IsMedicalCertificate && !a.IsDelete)
                .OrderByDescending(a => a.CreateDateTime)
                .FirstOrDefaultAsync(cancellationToken);

            if (attachment == null)
            {
                return LeaveBalanceLedgerServiceResult<MedicalCertificateDetailDto>.Fail(
                    StatusCodes.Status404NotFound,
                    "Surat keterangan sakit tidak ditemukan untuk permohonan cuti ini.");
            }

            string diagnosisDisplay;
            bool isMasked;

            if (isAuthorizedK3RsOrAdmin)
            {
                diagnosisDisplay = !string.IsNullOrWhiteSpace(attachment.MedicalDiagnosisIcd10)
                    ? attachment.MedicalDiagnosisIcd10
                    : "Tidak ada diagnosa spesifik";
                isMasked = false;
            }
            else
            {
                // Masking diagnosa dari atasan langsung (VR-LEV-09, LEV-W3)
                diagnosisDisplay = MaskedDiagnosisPlaceholder;
                isMasked = true;
            }

            return LeaveBalanceLedgerServiceResult<MedicalCertificateDetailDto>.Ok(
                new MedicalCertificateDetailDto
                {
                    AttachmentId = attachment.Id,
                    LeaveRequestId = attachment.LeaveRequestId,
                    RequestNumber = attachment.LeaveRequest?.RequestNumber ?? string.Empty,
                    FaskesName = attachment.FaskesName ?? string.Empty,
                    DoctorName = attachment.DoctorName ?? string.Empty,
                    DoctorSipNumber = attachment.DoctorSipNumber ?? string.Empty,
                    MedicalDiagnosis = diagnosisDisplay,
                    IsDiagnosisMasked = isMasked,
                    VerificationStatus = attachment.VerificationStatus,
                    VerificationNotes = attachment.VerificationNotes,
                    VerifiedAt = attachment.VerifiedAt,
                    OriginalFileName = attachment.OriginalFileName,
                    FilePath = attachment.FilePath
                },
                "Rincian surat keterangan dokter berhasil diambil.");
        }

        /// <summary>
        /// Verifikasi keabsahan surat dokter oleh Tim K3RS / HR Medis (HRD-DEC-112).
        /// </summary>
        public async Task<LeaveBalanceLedgerServiceResult<MedicalCertificateDetailDto>> VerifyCertificateAsync(
            Guid attachmentId,
            Guid k3rsUserId,
            VerifyMedicalCertificateCommand command,
            CancellationToken cancellationToken = default)
        {
            if (command == null)
            {
                return LeaveBalanceLedgerServiceResult<MedicalCertificateDetailDto>.Fail(StatusCodes.Status400BadRequest, "Command tidak boleh kosong.");
            }

            var attachment = await _dbContext.Set<TrxLeaveRequestAttachment>()
                .Include(a => a.LeaveRequest)
                .FirstOrDefaultAsync(a => a.Id == attachmentId && !a.IsDelete, cancellationToken);

            if (attachment == null)
            {
                return LeaveBalanceLedgerServiceResult<MedicalCertificateDetailDto>.Fail(StatusCodes.Status404NotFound, "Berkas lampiran medis tidak ditemukan.");
            }

            attachment.VerificationStatus = command.IsApproved ? "Verified" : "Rejected";
            attachment.VerificationNotes = command.VerificationNotes;
            attachment.VerifiedByUserId = k3rsUserId;
            attachment.VerifiedAt = DateTime.UtcNow;
            attachment.UpdateBy = k3rsUserId;
            attachment.UpdateDateTime = DateTime.UtcNow;

            if (attachment.LeaveRequest != null)
            {
                attachment.LeaveRequest.IsDoctorNoteVerified = command.IsApproved;
                attachment.LeaveRequest.UpdateBy = k3rsUserId;
                attachment.LeaveRequest.UpdateDateTime = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Berkas medis {AttachmentId} diverifikasi dengan status {Status} oleh K3RS {UserId}.",
                attachmentId, attachment.VerificationStatus, k3rsUserId);

            return LeaveBalanceLedgerServiceResult<MedicalCertificateDetailDto>.Ok(
                new MedicalCertificateDetailDto
                {
                    AttachmentId = attachment.Id,
                    LeaveRequestId = attachment.LeaveRequestId,
                    RequestNumber = attachment.LeaveRequest?.RequestNumber ?? string.Empty,
                    FaskesName = attachment.FaskesName ?? string.Empty,
                    DoctorName = attachment.DoctorName ?? string.Empty,
                    DoctorSipNumber = attachment.DoctorSipNumber ?? string.Empty,
                    MedicalDiagnosis = attachment.MedicalDiagnosisIcd10 ?? string.Empty,
                    IsDiagnosisMasked = false,
                    VerificationStatus = attachment.VerificationStatus,
                    VerificationNotes = attachment.VerificationNotes,
                    VerifiedAt = attachment.VerifiedAt,
                    OriginalFileName = attachment.OriginalFileName,
                    FilePath = attachment.FilePath
                },
                command.IsApproved
                    ? "Surat keterangan sakit berhasil diverifikasi sah oleh Tim K3RS."
                    : "Surat keterangan sakit ditolak oleh Tim K3RS.");
        }
    }
}
