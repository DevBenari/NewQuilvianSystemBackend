using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QRCoder;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Repositories;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Services
{
    public class EmployeeDocumentIssuanceService : IEmployeeDocumentIssuanceService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IDocumentAutoNumberingService _numberingService;
        private readonly IPinRateLimiterService _rateLimiter;
        private readonly ILogger<EmployeeDocumentIssuanceService> _logger;

        private static readonly CultureInfo IdCulture = new("id-ID");

        public EmployeeDocumentIssuanceService(
            ApplicationDbContext dbContext,
            IDocumentAutoNumberingService numberingService,
            IPinRateLimiterService rateLimiter,
            ILogger<EmployeeDocumentIssuanceService> logger)
        {
            _dbContext = dbContext;
            _numberingService = numberingService;
            _rateLimiter = rateLimiter;
            _logger = logger;
        }

        #region Pengajuan Dokumen & Anti-Manual Guard (BE-HRD-200)

        public async Task<EmployeeDocumentIssuanceDto> RequestDocumentAsync(
            Guid workforceProfileId,
            SubmitDocumentRequestDto request,
            Guid requestedByUserId,
            CancellationToken cancellationToken = default)
        {
            var profile = await _dbContext.MstWorkforceProfiles
                .Include(x => x.Employee)
                .Include(x => x.PrimaryDepartment)
                .Include(x => x.PrimaryPosition)
                .Include(x => x.UserAccount)
                .FirstOrDefaultAsync(x => x.Id == workforceProfileId && !x.IsDelete, cancellationToken);

            if (profile == null)
            {
                throw new InvalidOperationException($"Profil pegawai dengan ID {workforceProfileId} tidak ditemukan.");
            }

            var docType = await _dbContext.MstEmployeeDocumentTypes
                .FirstOrDefaultAsync(x => x.Id == request.EmployeeDocumentTypeId && !x.IsDelete, cancellationToken);

            if (docType == null)
            {
                throw new InvalidOperationException($"Jenis dokumen kepegawaian dengan ID {request.EmployeeDocumentTypeId} tidak ditemukan.");
            }

            bool isFinancial = docType.IsFinancialSensitive ||
                               docType.DocumentTypeCode.Contains("SALARY", StringComparison.OrdinalIgnoreCase) ||
                               docType.DocumentTypeCode.Contains("INCOME", StringComparison.OrdinalIgnoreCase) ||
                               docType.DocumentTypeCode.Contains("KPR", StringComparison.OrdinalIgnoreCase) ||
                               docType.DocumentCategory.Contains("Finance", StringComparison.OrdinalIgnoreCase);

            decimal? salaryBruto = null;
            decimal? salaryNetto = null;
            Guid? payslipId = null;

            // INT-HR-56 & VR-HRS-06: Anti-Manual Guard Penarikan Gaji Otomatis dari Sub-10 Payroll
            if (isFinancial)
            {
                var targetEmployeeId = profile.Employee?.Id;

                var latestPayslip = await _dbContext.TrxPayrollPayslips
                    .Include(p => p.PayrollRunEmployee)
                    .Where(p => (p.PayrollRunEmployee.WorkforceProfileId == workforceProfileId ||
                                (targetEmployeeId.HasValue && p.PayrollRunEmployee.EmployeeId == targetEmployeeId.Value))
                             && (p.PayslipStatus == "Published" || p.PayslipStatus == "Finalized" || p.PayslipStatus == "Generated")
                             && !p.IsDelete)
                    .OrderByDescending(p => p.PeriodEndDateSnapshot)
                    .FirstOrDefaultAsync(cancellationToken);

                if (latestPayslip == null)
                {
                    _logger.LogWarning("[ANTI-MANUAL-GUARD] Penolakan pengajuan surat finansial untuk pegawai {ProfileCode}: Data slip gaji final Sub-10 tidak ditemukan (VR-HRS-06).", profile.ProfileCode);
                    throw new InvalidOperationException(
                        "Data slip gaji resmi Sub-10 Payroll belum tersedia untuk karyawan ini. " +
                        "Sesuai regulasi rumah sakit (VR-HRS-06), penerbitan Surat Keterangan Penghasilan/KPR dilarang menggunakan input manual dan hanya dapat diproses setelah slip gaji berstatus Finalized/Published diterbitkan.");
                }

                salaryBruto = latestPayslip.GrossPay;
                salaryNetto = latestPayslip.NetPay;
                payslipId = latestPayslip.Id;

                _logger.LogInformation("[INT-HR-56] Menarik otomatis slip gaji final ID {PayslipId} untuk {ProfileCode}. Bruto: {Bruto}, Netto: {Netto}",
                    payslipId, profile.ProfileCode, salaryBruto, salaryNetto);
            }

            var now = DateTime.UtcNow;
            var reqNumber = $"REQ-DOC-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
            var issuanceNumber = $"ISS-DOC-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";

            var docRequest = new TrxEmployeeDocumentRequest
            {
                Id = Guid.NewGuid(),
                EmployeeDocumentTypeId = docType.Id,
                RequestedByWorkforceProfileId = workforceProfileId,
                RequestedForWorkforceProfileId = request.RequestedForWorkforceProfileId ?? workforceProfileId,
                RequestedForEmployeeId = profile.Employee?.Id,
                RequestedByUserId = requestedByUserId,
                DocumentRequestNumber = reqNumber,
                RequestStatus = isFinancial ? "PendingCbApproval" : (docType.RequiresApproval ? "PendingAuthorization" : "Approved"),
                Purpose = request.Purpose,
                LanguageCode = request.LanguageCode,
                NumberOfCopies = request.NumberOfCopies,
                DeliveryMethod = request.DeliveryMethod ?? "Digital",
                RequestedAt = now,
                NeededByDate = request.NeededByDate,
                SubmittedAt = now,
                ProcessingNotes = request.AdditionalNotes,
                IsConfidential = isFinancial || docType.IsConfidential,
                CreateBy = requestedByUserId,
                CreateDateTime = now
            };

            _dbContext.TrxEmployeeDocumentRequests.Add(docRequest);

            var issuance = new TrxEmployeeDocumentIssuance
            {
                Id = Guid.NewGuid(),
                EmployeeDocumentRequestId = docRequest.Id,
                WorkforceProfileId = workforceProfileId,
                EmployeeId = profile.Employee?.Id,
                IssuedByUserId = requestedByUserId,
                IssuanceNumber = issuanceNumber,
                OfficialDocumentNumber = string.Empty, // Di-generate saat otorisasi resmi
                VersionNumber = 1,
                DocumentFileName = $"{docType.DocumentTypeCode}_{profile.ProfileCode}_{now:yyyyMMdd}.pdf",
                DocumentFilePath = string.Empty,
                ContentType = "application/pdf",
                IssuedAt = now,
                ValidFrom = now,
                ValidUntil = now.AddDays(docType.ValidityPeriodDays > 0 ? docType.ValidityPeriodDays : 90),
                IsFinancialSensitive = isFinancial,
                SalaryBrutoAmount = salaryBruto,
                SalaryNettoAmount = salaryNetto,
                PayrollPayslipId = payslipId,
                IssuanceStatus = isFinancial ? "PendingCbApproval" : (docType.RequiresApproval ? "PendingAuthorization" : "Approved"),
                CreateBy = requestedByUserId,
                CreateDateTime = now
            };

            _dbContext.TrxEmployeeDocumentIssuances.Add(issuance);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("[HRS-W4-REQ] Permohonan dokumen {DocType} berhasil diajukan dengan nomor {ReqNum}. Status: {Status}",
                docType.DocumentTypeCode, reqNumber, issuance.IssuanceStatus);

            return MapToDto(issuance, docRequest, profile, docType);
        }

        #endregion

        #region Tier 1 C&B Supervisor Approval (BE-HRD-201)

        public async Task<EmployeeDocumentIssuanceDto> CbSupervisorApproveAsync(
            Guid issuanceId,
            CbSupervisorApprovalDto request,
            Guid cbSupervisorUserId,
            CancellationToken cancellationToken = default)
        {
            var issuance = await _dbContext.TrxEmployeeDocumentIssuances
                .Include(x => x.EmployeeDocumentRequest)
                .Include(x => x.WorkforceProfile)
                    .ThenInclude(w => w!.UserAccount)
                .Include(x => x.WorkforceProfile)
                    .ThenInclude(w => w!.Employee)
                .Include(x => x.WorkforceProfile)
                    .ThenInclude(w => w!.PrimaryDepartment)
                .Include(x => x.WorkforceProfile)
                    .ThenInclude(w => w!.PrimaryPosition)
                .FirstOrDefaultAsync(x => x.Id == issuanceId && !x.IsDelete, cancellationToken);

            if (issuance == null)
            {
                throw new InvalidOperationException($"Penerbitan dokumen ID {issuanceId} tidak ditemukan.");
            }

            var docType = await _dbContext.MstEmployeeDocumentTypes
                .FirstOrDefaultAsync(x => x.Id == issuance.EmployeeDocumentRequest!.EmployeeDocumentTypeId, cancellationToken);

            // VR-HRS-08: Penegakan Anti-Self-Approval untuk Supervisor C&B
            if (issuance.WorkforceProfile != null &&
                (issuance.WorkforceProfile.UserAccount?.Id == cbSupervisorUserId || issuance.CreateBy == cbSupervisorUserId))
            {
                _logger.LogWarning("[ANTI-SELF-APPROVAL] Supervisor C&B {UserId} dilarang menyetujui dokumen sendiri (issuance ID {IssuanceId})",
                    cbSupervisorUserId, issuanceId);
                throw new InvalidOperationException(
                    "Pelanggaran Tata Kelola: Supervisor C&B dilarang keras menyetujui permohonan surat dinas untuk dirinya sendiri (VR-HRS-08 Anti-Self-Approval Enforced).");
            }

            if (issuance.IssuanceStatus != "PendingCbApproval")
            {
                throw new InvalidOperationException($"Status dokumen saat ini '{issuance.IssuanceStatus}', tidak dapat diproses persetujuan C&B.");
            }

            var now = DateTime.UtcNow;
            if (request.IsApproved)
            {
                issuance.IssuanceStatus = "CbApproved";
                issuance.EmployeeDocumentRequest!.RequestStatus = "CbApproved";
                issuance.EmployeeDocumentRequest.ProcessingNotes = $"[C&B Approved] {request.Notes}";
            }
            else
            {
                issuance.IssuanceStatus = "Rejected";
                issuance.EmployeeDocumentRequest!.RequestStatus = "Rejected";
                issuance.EmployeeDocumentRequest.ProcessingNotes = $"[C&B Rejected] {request.Notes}";
            }

            issuance.UpdateBy = cbSupervisorUserId;
            issuance.UpdateDateTime = now;
            issuance.EmployeeDocumentRequest.UpdateBy = cbSupervisorUserId;
            issuance.EmployeeDocumentRequest.UpdateDateTime = now;

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("[HRS-W4-CB] Verifikasi C&B untuk dokumen {IssuanceId} oleh {UserId}: {Decision}",
                issuanceId, cbSupervisorUserId, request.IsApproved ? "Approved" : "Rejected");

            return MapToDto(issuance, issuance.EmployeeDocumentRequest, issuance.WorkforceProfile, docType);
        }

        #endregion

        #region Tier 2 Final Authorization & Issuance (BE-HRD-201, BE-HRD-202, BE-HRD-203)

        public async Task<EmployeeDocumentIssuanceDto> FinalAuthorizeAndIssueAsync(
            Guid issuanceId,
            FinalAuthorizeAndIssueDto request,
            Guid authorizedSignerUserId,
            CancellationToken cancellationToken = default)
        {
            var issuance = await _dbContext.TrxEmployeeDocumentIssuances
                .Include(x => x.EmployeeDocumentRequest)
                .Include(x => x.WorkforceProfile)
                    .ThenInclude(w => w!.UserAccount)
                .Include(x => x.WorkforceProfile)
                    .ThenInclude(w => w!.Employee)
                .Include(x => x.WorkforceProfile)
                    .ThenInclude(w => w!.PrimaryDepartment)
                .Include(x => x.WorkforceProfile)
                    .ThenInclude(w => w!.PrimaryPosition)
                .FirstOrDefaultAsync(x => x.Id == issuanceId && !x.IsDelete, cancellationToken);

            if (issuance == null)
            {
                throw new InvalidOperationException($"Penerbitan dokumen ID {issuanceId} tidak ditemukan.");
            }

            var docType = await _dbContext.MstEmployeeDocumentTypes
                .FirstOrDefaultAsync(x => x.Id == issuance.EmployeeDocumentRequest!.EmployeeDocumentTypeId, cancellationToken);

            if (docType == null)
            {
                throw new InvalidOperationException("Master jenis dokumen kepegawaian tidak ditemukan.");
            }

            // VR-HRS-08: Penegakan Anti-Self-Approval untuk Pejabat Penandatangan Final
            if (issuance.WorkforceProfile != null &&
                (issuance.WorkforceProfile.UserAccount?.Id == authorizedSignerUserId || issuance.CreateBy == authorizedSignerUserId))
            {
                _logger.LogWarning("[ANTI-SELF-APPROVAL] Pejabat {UserId} dilarang mengotorisasi dokumen sendiri (issuance ID {IssuanceId})",
                    authorizedSignerUserId, issuanceId);
                throw new InvalidOperationException(
                    "Pelanggaran Tata Kelola: Pejabat penandatangan dilarang keras mengotorisasi atau menerbitkan surat dinas resmi untuk dirinya sendiri (VR-HRS-08 Anti-Self-Approval Enforced).");
            }

            // Validasi status alur persetujuan
            if (issuance.IsFinancialSensitive && issuance.IssuanceStatus != "CbApproved")
            {
                throw new InvalidOperationException(
                    $"Dokumen finansial wajib mendapatkan persetujuan C&B Supervisor terlebih dahulu (Status saat ini: {issuance.IssuanceStatus}) (VR-HRS-07).");
            }
            else if (!issuance.IsFinancialSensitive && issuance.IssuanceStatus != "PendingAuthorization" && issuance.IssuanceStatus != "Approved")
            {
                throw new InvalidOperationException(
                    $"Status dokumen saat ini '{issuance.IssuanceStatus}', tidak memenuhi syarat untuk otorisasi penerbitan final.");
            }

            var now = DateTime.UtcNow;

            // 1. Generate nomor naskah dinas resmi gapless (BE-HRD-194)
            var officialDocNumber = await _numberingService.GenerateOfficialDocumentNumberAsync(
                docType.Id, cancellationToken);

            // 2. Generator SHA-256 Verification Hash (BE-HRD-203)
            var rawHashSource = $"{issuance.IssuanceNumber}_{officialDocNumber}_{issuance.WorkforceProfileId}_{now.Ticks}_QUILVIAN_HRD_SALT";
            var verificationHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawHashSource))).ToLowerInvariant();
            var verificationUrl = $"https://simrs.quilvian.co.id/public-verify/hr-document?hash={verificationHash}";

            // 3. Generator QR Code PNG via QRCoder (BE-HRD-203)
            var qrCodeRelPath = GenerateAndSaveQrCode(verificationUrl, verificationHash);

            // 4. Pengelolaan PIN Keamanan Finansial 4-Digit (BE-HRD-203 & VR-HRS-09)
            string? rawSecurityPin = null;
            if (issuance.IsFinancialSensitive)
            {
                rawSecurityPin = !string.IsNullOrWhiteSpace(request.CustomSecurityPin)
                    ? request.CustomSecurityPin
                    : Random.Shared.Next(1000, 9999).ToString();

                var salt = Guid.NewGuid().ToString("N");
                var pinHash = HashPinWithSalt(rawSecurityPin, salt);
                issuance.SecurityPinHash = $"{salt}:{pinHash}";

                _logger.LogInformation("[FIN-PIN-GUARD] Generated 4-digit PIN hash for financial document {DocNumber}", officialDocNumber);
            }

            // 5. Mesin Rendering Naskah Dinas PDF berbasis Token Dinamis (BE-HRD-202)
            var renderedHtml = RenderDocumentTemplate(docType, issuance, officialDocNumber, verificationUrl, request.SignerName, request.SignerTitle, now);
            var (pdfFilePath, fileSizeBytes, fileChecksum) = SaveRenderedDocumentFile(renderedHtml, officialDocNumber);

            // INT-HR-66: Pengarsipan Abadi ke Repositori Digital E-Dossier Sub-02 (WfpDocument)
            if (issuance.WorkforceProfileId.HasValue)
            {
                var wfpDoc = new QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models.WfpDocument
                {
                    Id = Guid.NewGuid(),
                    WorkforceProfileId = issuance.WorkforceProfileId.Value,
                    DocumentType = docType.DocumentTypeCode,
                    DocumentName = docType.DocumentTypeName,
                    DocumentNumber = officialDocNumber,
                    IssueDate = now,
                    ExpiredDate = issuance.ValidUntil,
                    IssuingAuthority = "Rumah Sakit Quilvian Utama",
                    FilePath = pdfFilePath,
                    FileContentType = "application/pdf",
                    StoredFileName = Path.GetFileName(pdfFilePath),
                    OriginalFileName = issuance.DocumentFileName,
                    FileSizeBytes = fileSizeBytes,
                    FileChecksum = fileChecksum,
                    IsConfidential = issuance.IsFinancialSensitive,
                    IsVerified = true,
                    VerifiedAt = now,
                    VerifiedByUserId = authorizedSignerUserId,
                    VerificationNote = $"Pengarsipan otomatis E-Dossier Sub-02 (Nomor Dokumen: {officialDocNumber})",
                    IsActive = true,
                    CreateBy = authorizedSignerUserId,
                    CreateDateTime = now
                };
                _dbContext.WfpDocuments.Add(wfpDoc);
                issuance.EDossierDocumentId = wfpDoc.Id;
                issuance.WorkforceDocumentId = wfpDoc.Id;
            }

            // 6. Pembaruan entitas resmi
            issuance.OfficialDocumentNumber = officialDocNumber;
            issuance.DigitallySignedByUserId = authorizedSignerUserId;
            issuance.IsDigitallySigned = true;
            issuance.DigitallySignedAt = now;
            issuance.SignerName = request.SignerName;
            issuance.SignerTitle = request.SignerTitle;
            issuance.VerificationHash = verificationHash;
            issuance.QrCodeImagePath = qrCodeRelPath;
            issuance.PdfFilePath = pdfFilePath;
            issuance.FileSizeBytes = fileSizeBytes;
            issuance.FileChecksum = fileChecksum;
            issuance.IssuanceStatus = "Issued";
            issuance.DocumentSnapshotJson = renderedHtml;
            issuance.UpdateBy = authorizedSignerUserId;
            issuance.UpdateDateTime = now;

            issuance.EmployeeDocumentRequest!.RequestStatus = "Completed";
            issuance.EmployeeDocumentRequest.CompletedAt = now;
            issuance.EmployeeDocumentRequest.ProcessedByUserId = authorizedSignerUserId;
            issuance.EmployeeDocumentRequest.UpdateBy = authorizedSignerUserId;
            issuance.EmployeeDocumentRequest.UpdateDateTime = now;

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("[DOCUMENT-ISSUED] Surat naskah dinas resmi {DocNumber} berhasil diterbitkan & ditandatangani digital oleh {Signer}.",
                officialDocNumber, request.SignerName);

            var resultDto = MapToDto(issuance, issuance.EmployeeDocumentRequest, issuance.WorkforceProfile, docType);
            resultDto.GeneratedRawSecurityPin = rawSecurityPin;
            resultDto.VerificationUrl = verificationUrl;

            return resultDto;
        }

        #endregion

        #region Verifikasi Publik & Masking Finansial (BE-HRD-203)

        public async Task<PublicDocumentVerificationDto> VerifyPublicDocumentAsync(
            string verificationHash,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(verificationHash))
            {
                return new PublicDocumentVerificationDto
                {
                    IsValid = false,
                    VerificationStatusMessage = "Kode hash verifikasi naskah dinas tidak boleh kosong."
                };
            }

            var issuance = await _dbContext.TrxEmployeeDocumentIssuances
                .Include(x => x.EmployeeDocumentRequest)
                    .ThenInclude(r => r!.EmployeeDocumentType)
                .Include(x => x.WorkforceProfile)
                    .ThenInclude(w => w!.Employee)
                .FirstOrDefaultAsync(x => x.VerificationHash == verificationHash.Trim().ToLowerInvariant() && !x.IsDelete, cancellationToken);

            if (issuance == null)
            {
                return new PublicDocumentVerificationDto
                {
                    IsValid = false,
                    VerificationStatusMessage = "Dokumen tidak ditemukan atau kode verifikasi tidak terdaftar pada sistem Rumah Sakit Quilvian."
                };
            }

            if (issuance.IsRevoked)
            {
                return new PublicDocumentVerificationDto
                {
                    IsValid = false,
                    IssuanceNumber = issuance.IssuanceNumber,
                    OfficialDocumentNumber = issuance.OfficialDocumentNumber,
                    DocumentTypeName = issuance.EmployeeDocumentRequest?.EmployeeDocumentType?.DocumentTypeName ?? "Naskah Dinas SDM",
                    VerificationStatusMessage = $"PERINGATAN RESMI: Dokumen ini telah DICABUT/DIBATALKAN oleh Rumah Sakit Quilvian pada {issuance.RevokedAt:dd MMMM yyyy} berdasarkan Berita Acara Pencabutan (BAP) No. {issuance.RevocationBapNumber}. Alasan: {issuance.RevocationReason}. Segala bentuk penggunaan dokumen ini adalah TIDAK SAH secara hukum."
                };
            }

            var empName = issuance.WorkforceProfile?.DisplayName ?? "Pegawai RS Quilvian";
            var empNumber = issuance.WorkforceProfile?.Employee?.EmployeeNumber ?? issuance.WorkforceProfile?.ProfileCode ?? string.Empty;

            var maskedName = MaskString(empName);
            var maskedNumber = MaskString(empNumber);

            return new PublicDocumentVerificationDto
            {
                IsValid = true,
                VerificationStatusMessage = "Dokumen Sah & Terverifikasi Diterbitkan Resmi oleh Rumah Sakit Quilvian.",
                IssuanceNumber = issuance.IssuanceNumber,
                OfficialDocumentNumber = issuance.OfficialDocumentNumber,
                DocumentTypeName = issuance.EmployeeDocumentRequest?.EmployeeDocumentType?.DocumentTypeName ?? "Naskah Dinas SDM",
                EmployeeNameMasked = maskedName,
                EmployeeNumberMasked = maskedNumber,
                IssuedByHospital = "Rumah Sakit Quilvian Utama",
                IssuedAt = issuance.IssuedAt,
                ValidUntil = issuance.ValidUntil,
                SignerName = issuance.SignerName ?? "Pejabat SDM Berwenang",
                SignerTitle = issuance.SignerTitle ?? "Manajer Sumber Daya Manusia",
                IsFinancialSensitive = issuance.IsFinancialSensitive,
                IsFinancialDataMasked = issuance.IsFinancialSensitive,
                MaskedSalaryBruto = issuance.IsFinancialSensitive ? "Rp **.***.***" : null,
                MaskedSalaryNetto = issuance.IsFinancialSensitive ? "Rp **.***.***" : null,
                RequiresPinToUnlock = issuance.IsFinancialSensitive
            };
        }

        public async Task<FinancialDetailsUnlockedDto> UnlockFinancialDetailsAsync(
            UnlockFinancialDetailsRequestDto request,
            string? clientIp = null,
            CancellationToken cancellationToken = default)
        {
            var clientIpSafe = string.IsNullOrWhiteSpace(clientIp) ? "127.0.0.1" : clientIp;

            // VR-HRS-12: Proteksi Rate Limiting & Anti-Brute-Force PIN (Max 3 kali percobaan per 30 menit)
            var rateLimitCheck = _rateLimiter.CheckRateLimit(request.VerificationHash, clientIpSafe);
            if (rateLimitCheck.IsBlocked)
            {
                _logger.LogWarning("[RATE-LIMIT-LOCK] Percobaan akses PIN untuk hash {Hash} dari IP {Ip} ditolak: {Msg}",
                    request.VerificationHash, clientIpSafe, rateLimitCheck.BlockedMessage);
                throw new InvalidOperationException(rateLimitCheck.BlockedMessage);
            }

            var issuance = await _dbContext.TrxEmployeeDocumentIssuances
                .Include(x => x.WorkforceProfile)
                    .ThenInclude(w => w!.Employee)
                .FirstOrDefaultAsync(x => x.VerificationHash == request.VerificationHash.Trim().ToLowerInvariant() && !x.IsDelete, cancellationToken);

            if (issuance == null)
            {
                throw new InvalidOperationException("Dokumen tidak ditemukan.");
            }

            if (!issuance.IsFinancialSensitive)
            {
                throw new InvalidOperationException("Dokumen ini bukan dokumen finansial ber-PIN.");
            }

            if (string.IsNullOrWhiteSpace(issuance.SecurityPinHash) || !VerifyPin(request.SecurityPin, issuance.SecurityPinHash))
            {
                _rateLimiter.RecordFailedAttempt(request.VerificationHash, clientIpSafe);
                var remaining = rateLimitCheck.RemainingAttempts - 1;

                _logger.LogWarning("[PIN-INVALID] Gagal verifikasi PIN dokumen {DocNum} dari IP {Ip}. Sisa kesempatan: {Remaining}",
                    issuance.OfficialDocumentNumber, clientIpSafe, remaining);

                if (remaining <= 0)
                {
                    throw new InvalidOperationException(
                        "Akses pembukaan PIN finansial diblokir sementara selama 30 menit karena salah memasukkan PIN sebanyak 3 kali berturut-turut demi keamanan data finansial tenaga medis (VR-HRS-12).");
                }

                throw new InvalidOperationException(
                    $"PIN 4-digit keamanan finansial tidak valid atau tidak cocok. Sisa kesempatan mencoba: {remaining} kali sebelum terkunci 30 menit (VR-HRS-12).");
            }

            // PIN Benar: Reset rate limiter attempts
            _rateLimiter.ResetAttempts(request.VerificationHash, clientIpSafe);

            var bruto = issuance.SalaryBrutoAmount ?? 0m;
            var netto = issuance.SalaryNettoAmount ?? 0m;

            TrxPayrollPayslip? payslip = null;
            if (issuance.PayrollPayslipId.HasValue)
            {
                payslip = await _dbContext.TrxPayrollPayslips
                    .FirstOrDefaultAsync(p => p.Id == issuance.PayrollPayslipId.Value && !p.IsDelete, cancellationToken);
            }

            var periodStr = payslip != null
                ? $"{payslip.PeriodStartDateSnapshot:dd MMM yyyy} s.d. {payslip.PeriodEndDateSnapshot:dd MMM yyyy}"
                : "Periode Penggajian Terakhir";

            return new FinancialDetailsUnlockedDto
            {
                IsSuccess = true,
                Message = "PIN 4-Digit Terverifikasi. Rincian data finansial resmi berhasil dibuka.",
                OfficialDocumentNumber = issuance.OfficialDocumentNumber,
                EmployeeFullName = issuance.WorkforceProfile?.DisplayName ?? string.Empty,
                EmployeeNumber = issuance.WorkforceProfile?.Employee?.EmployeeNumber ?? issuance.WorkforceProfile?.ProfileCode ?? string.Empty,
                SalaryBrutoAmount = bruto,
                SalaryNettoAmount = netto,
                FormattedSalaryBruto = bruto.ToString("C0", IdCulture),
                FormattedSalaryNetto = netto.ToString("C0", IdCulture),
                Currency = "IDR",
                PayslipPeriodSnapshot = periodStr,
                VerifiedAtUtc = DateTime.UtcNow
            };
        }

        #endregion

        #region Download & Revocation

        public async Task<(byte[] FileBytes, string ContentType, string FileName)> DownloadIssuedDocumentPdfAsync(
            Guid issuanceId,
            Guid requestingUserId,
            bool isStaff,
            CancellationToken cancellationToken = default)
        {
            var issuance = await _dbContext.TrxEmployeeDocumentIssuances
                .Include(x => x.WorkforceProfile)
                    .ThenInclude(w => w!.UserAccount)
                .FirstOrDefaultAsync(x => x.Id == issuanceId && !x.IsDelete, cancellationToken);

            if (issuance == null)
            {
                throw new InvalidOperationException("Dokumen penerbitan tidak ditemukan.");
            }

            // Cek otorisasi download: hanya pemilik dokumen atau staf HR yang boleh mengunduh
            if (!isStaff && issuance.WorkforceProfile?.UserAccount?.Id != requestingUserId && issuance.IssuedByUserId != requestingUserId)
            {
                throw new UnauthorizedAccessException("Anda tidak memiliki wewenang untuk mengunduh dokumen kepegawaian ini.");
            }

            if (!string.IsNullOrWhiteSpace(issuance.PdfFilePath) && File.Exists(issuance.PdfFilePath))
            {
                var bytes = await File.ReadAllBytesAsync(issuance.PdfFilePath, cancellationToken);
                return (bytes, "application/pdf", issuance.DocumentFileName);
            }

            // Fallback jika file fisik belum ter-render: kembalikan snapshot HTML as bytes
            var snapshotContent = issuance.DocumentSnapshotJson ?? "<html><body><h3>Naskah Dinas Belum Siap</h3></body></html>";
            var fallbackBytes = Encoding.UTF8.GetBytes(snapshotContent);
            return (fallbackBytes, "text/html", $"{issuance.OfficialDocumentNumber}.html");
        }

        public async Task<EmployeeDocumentIssuanceDto> RevokeDocumentAsync(
            Guid issuanceId,
            RevokeDocumentRequestDto request,
            Guid revokingUserId,
            CancellationToken cancellationToken = default)
        {
            // VR-HRS-14: Pencabutan dokumen dinas wajib menyertakan nomor BAP dan alasan pembatalan
            if (string.IsNullOrWhiteSpace(request.RevocationReason))
            {
                throw new InvalidOperationException("Alasan pencabutan surat naskah dinas wajib diisi lengkap sesuai regulasi RS (VR-HRS-14).");
            }
            if (string.IsNullOrWhiteSpace(request.RevocationBapNumber))
            {
                throw new InvalidOperationException("Nomor Berita Acara Pencabutan (BAP) wajib dicantumkan dalam pembatalan surat dinas resmi (VR-HRS-14).");
            }

            var issuance = await _dbContext.TrxEmployeeDocumentIssuances
                .Include(x => x.EmployeeDocumentRequest)
                .Include(x => x.WorkforceProfile)
                    .ThenInclude(w => w!.Employee)
                .Include(x => x.WorkforceProfile)
                    .ThenInclude(w => w!.PrimaryDepartment)
                .Include(x => x.WorkforceProfile)
                    .ThenInclude(w => w!.PrimaryPosition)
                .FirstOrDefaultAsync(x => x.Id == issuanceId && !x.IsDelete, cancellationToken);

            if (issuance == null)
            {
                throw new InvalidOperationException("Dokumen penerbitan tidak ditemukan.");
            }

            var now = DateTime.UtcNow;
            issuance.IsRevoked = true;
            issuance.RevokedAt = now;
            issuance.RevokedByUserId = revokingUserId;
            issuance.RevocationReason = request.RevocationReason;
            issuance.RevocationBapNumber = request.RevocationBapNumber;
            issuance.IssuanceStatus = "Revoked";
            issuance.UpdateBy = revokingUserId;
            issuance.UpdateDateTime = now;

            // INT-HR-66: Non-aktifkan juga arsip E-Dossier Sub-02
            if (issuance.EDossierDocumentId.HasValue)
            {
                var wfpDoc = await _dbContext.WfpDocuments
                    .FirstOrDefaultAsync(d => d.Id == issuance.EDossierDocumentId.Value && !d.IsDelete, cancellationToken);

                if (wfpDoc != null)
                {
                    wfpDoc.IsActive = false;
                    wfpDoc.Description = $"[REVOKED] Dicabut resmi dengan BAP No: {request.RevocationBapNumber}. Alasan: {request.RevocationReason}";
                    wfpDoc.UpdateBy = revokingUserId;
                    wfpDoc.UpdateDateTime = now;
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogWarning("[DOCUMENT-REVOKED] Naskah dinas {DocNum} dicabut oleh {UserId}. BAP: {Bap}, Alasan: {Reason}",
                issuance.OfficialDocumentNumber, revokingUserId, request.RevocationBapNumber, request.RevocationReason);

            var docType = await _dbContext.MstEmployeeDocumentTypes
                .FirstOrDefaultAsync(x => x.Id == issuance.EmployeeDocumentRequest!.EmployeeDocumentTypeId, cancellationToken);

            return MapToDto(issuance, issuance.EmployeeDocumentRequest, issuance.WorkforceProfile, docType);
        }

        public async Task<EmployeeDocumentIssuanceDto> GetIssuanceDetailAsync(
            Guid issuanceId,
            CancellationToken cancellationToken = default)
        {
            var issuance = await _dbContext.TrxEmployeeDocumentIssuances
                .Include(x => x.EmployeeDocumentRequest)
                .Include(x => x.WorkforceProfile)
                    .ThenInclude(w => w!.Employee)
                .Include(x => x.WorkforceProfile)
                    .ThenInclude(w => w!.PrimaryDepartment)
                .Include(x => x.WorkforceProfile)
                    .ThenInclude(w => w!.PrimaryPosition)
                .FirstOrDefaultAsync(x => x.Id == issuanceId && !x.IsDelete, cancellationToken);

            if (issuance == null)
            {
                throw new InvalidOperationException($"Penerbitan dokumen ID {issuanceId} tidak ditemukan.");
            }

            var docType = await _dbContext.MstEmployeeDocumentTypes
                .FirstOrDefaultAsync(x => x.Id == issuance.EmployeeDocumentRequest!.EmployeeDocumentTypeId, cancellationToken);

            return MapToDto(issuance, issuance.EmployeeDocumentRequest, issuance.WorkforceProfile, docType);
        }

        public async Task<List<EmployeeDocumentIssuanceDto>> GetIssuancesQueueAsync(
            string? status = null,
            bool? isFinancialSensitive = null,
            Guid? workforceProfileId = null,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.TrxEmployeeDocumentIssuances
                .Include(x => x.EmployeeDocumentRequest)
                .Include(x => x.WorkforceProfile)
                    .ThenInclude(w => w!.Employee)
                .Include(x => x.WorkforceProfile)
                    .ThenInclude(w => w!.PrimaryDepartment)
                .Include(x => x.WorkforceProfile)
                    .ThenInclude(w => w!.PrimaryPosition)
                .Where(x => !x.IsDelete);

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(x => x.IssuanceStatus == status);
            }

            if (isFinancialSensitive.HasValue)
            {
                query = query.Where(x => x.IsFinancialSensitive == isFinancialSensitive.Value);
            }

            if (workforceProfileId.HasValue)
            {
                query = query.Where(x => x.WorkforceProfileId == workforceProfileId.Value);
            }

            var list = await query
                .OrderByDescending(x => x.IssuedAt)
                .ToListAsync(cancellationToken);

            var docTypeIds = list.Select(x => x.EmployeeDocumentRequest?.EmployeeDocumentTypeId ?? Guid.Empty).Distinct().ToList();
            var docTypes = await _dbContext.MstEmployeeDocumentTypes
                .Where(x => docTypeIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, cancellationToken);

            return list.Select(x =>
            {
                docTypes.TryGetValue(x.EmployeeDocumentRequest?.EmployeeDocumentTypeId ?? Guid.Empty, out var dt);
                return MapToDto(x, x.EmployeeDocumentRequest, x.WorkforceProfile, dt);
            }).ToList();
        }

        #endregion

        #region Helper Rendering, Hash, QR & Masking

        private static string GenerateAndSaveQrCode(string payload, string hash)
        {
            try
            {
                using var qrGen = new QRCodeGenerator();
                using var qrData = qrGen.CreateQrCode(payload, QRCodeGenerator.ECCLevel.M);
                using var qrCode = new PngByteQRCode(qrData);
                var qrBytes = qrCode.GetGraphic(20);

                var baseDir = Path.Combine(Directory.GetCurrentDirectory(), "Storage", "documents", "qrcodes");
                if (!Directory.Exists(baseDir))
                {
                    Directory.CreateDirectory(baseDir);
                }

                var fullPath = Path.Combine(baseDir, $"{hash}.png");
                File.WriteAllBytes(fullPath, qrBytes);

                return Path.Combine("Storage", "documents", "qrcodes", $"{hash}.png");
            }
            catch
            {
                return string.Empty;
            }
        }

        private static (string FilePath, long FileSize, string Checksum) SaveRenderedDocumentFile(string htmlContent, string officialDocNumber)
        {
            try
            {
                var safeDocNum = officialDocNumber.Replace('/', '_').Replace('\\', '_');
                var baseDir = Path.Combine(Directory.GetCurrentDirectory(), "Storage", "documents", "issued");
                if (!Directory.Exists(baseDir))
                {
                    Directory.CreateDirectory(baseDir);
                }

                var fullPath = Path.Combine(baseDir, $"{safeDocNum}.html");
                var bytes = Encoding.UTF8.GetBytes(htmlContent);
                File.WriteAllBytes(fullPath, bytes);

                var checksum = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
                return (fullPath, bytes.Length, checksum);
            }
            catch
            {
                return (string.Empty, 0, string.Empty);
            }
        }

        private static string RenderDocumentTemplate(
            MstEmployeeDocumentType docType,
            TrxEmployeeDocumentIssuance issuance,
            string officialDocNumber,
            string verificationUrl,
            string signerName,
            string signerTitle,
            DateTime issuedDate)
        {
            var template = docType.TemplateBodyHtml;
            if (string.IsNullOrWhiteSpace(template))
            {
                template = GetDefaultHospitalLetterTemplate(issuance.IsFinancialSensitive);
            }

            var empName = issuance.WorkforceProfile?.DisplayName ?? "Pegawai";
            var empCode = issuance.WorkforceProfile?.Employee?.EmployeeNumber ?? issuance.WorkforceProfile?.ProfileCode ?? "-";
            var posName = issuance.WorkforceProfile?.PrimaryPosition?.PositionName ?? "-";
            var deptName = issuance.WorkforceProfile?.PrimaryDepartment?.DepartmentName ?? "-";
            var purpose = issuance.EmployeeDocumentRequest?.Purpose ?? "Keperluan Administrasi Kepegawaian";
            var dateStr = issuedDate.ToString("dd MMMM yyyy", IdCulture);

            var brutoStr = issuance.SalaryBrutoAmount.HasValue
                ? issuance.SalaryBrutoAmount.Value.ToString("C0", IdCulture)
                : "Rp -";

            var nettoStr = issuance.SalaryNettoAmount.HasValue
                ? issuance.SalaryNettoAmount.Value.ToString("C0", IdCulture)
                : "Rp -";

            return template
                .Replace("[NOMOR_SURAT]", officialDocNumber)
                .Replace("[TANGGAL_SURAT]", dateStr)
                .Replace("[NAMA_LENGKAP]", empName)
                .Replace("[NIP]", empCode)
                .Replace("[JABATAN]", posName)
                .Replace("[UNIT_KERJA]", deptName)
                .Replace("[KEPERLUAN]", purpose)
                .Replace("[GAJI_BRUTO]", brutoStr)
                .Replace("[GAJI_NETTO]", nettoStr)
                .Replace("[NAMA_PENANDATANGAN]", signerName)
                .Replace("[JABATAN_PENANDATANGAN]", signerTitle)
                .Replace("[KOTA_RS]", "Jakarta")
                .Replace("[VERIFIKASI_URL]", verificationUrl);
        }

        private static string GetDefaultHospitalLetterTemplate(bool isFinancial)
        {
            var financialBlock = isFinancial
                ? @"<p>Menerangkan bahwa yang bersangkutan memiliki rincian penghasilan resmi terakhir sebagai berikut:</p>
                   <table style='width: 80%; margin: 15px auto; border-collapse: collapse; font-size: 14px;'>
                     <tr style='border-bottom: 1px solid #ddd;'><td style='padding: 6px;'>Penghasilan Bruto (Kotor)</td><td style='padding: 6px; font-weight: bold;'>[GAJI_BRUTO]</td></tr>
                     <tr style='border-bottom: 1px solid #ddd;'><td style='padding: 6px;'>Penghasilan Netto (Bawa Pulang)</td><td style='padding: 6px; font-weight: bold; color: #0284c7;'>[GAJI_NETTO]</td></tr>
                   </table>"
                : @"<p>Menerangkan bahwa yang bersangkutan adalah benar karyawan aktif di Rumah Sakit Quilvian Utama dengan reputasi dan kinerja yang baik.</p>";

            return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'/>
    <title>Naskah Dinas Resmi RS Quilvian</title>
    <style>
        body {{ font-family: 'Times New Roman', serif; margin: 40px; color: #1e293b; line-height: 1.6; }}
        .header {{ text-align: center; border-bottom: 3px double #0284c7; padding-bottom: 10px; margin-bottom: 25px; }}
        .header h2 {{ margin: 0; color: #0f172a; font-size: 22px; text-transform: uppercase; }}
        .header p {{ margin: 2px 0; font-size: 13px; color: #64748b; }}
        .doc-title {{ text-align: center; margin: 20px 0; }}
        .doc-title h3 {{ margin: 0; text-decoration: underline; font-size: 18px; }}
        .doc-title p {{ margin: 3px 0; font-size: 14px; font-weight: bold; }}
        .content {{ font-size: 14px; text-align: justify; }}
        .meta-table {{ width: 100%; margin: 15px 0 20px 30px; font-size: 14px; }}
        .meta-table td {{ padding: 4px 8px; vertical-align: top; }}
        .footer {{ margin-top: 40px; width: 100%; }}
        .signature-box {{ float: right; width: 280px; text-align: center; font-size: 14px; }}
        .verification {{ float: left; width: 250px; font-size: 11px; color: #64748b; border: 1px dashed #cbd5e1; padding: 8px; border-radius: 4px; }}
    </style>
</head>
<body>
    <div class='header'>
        <h2>Rumah Sakit Quilvian Utama</h2>
        <p>Jl. Kesehatan Medika Raya No. 42, Jakarta Pusat | Telp: (021) 555-0199 | info@quilvian.co.id</p>
    </div>
    <div class='doc-title'>
        <h3>SURAT KETERANGAN RESMI KEPEGAWAIAN</h3>
        <p>Nomor: [NOMOR_SURAT]</p>
    </div>
    <div class='content'>
        <p>Yang bertanda tangan di bawah ini, Manajemen Sumber Daya Manusia Rumah Sakit Quilvian Utama, dengan ini menerangkan bahwa:</p>
        <table class='meta-table'>
            <tr><td style='width: 200px;'>Nama Lengkap</td><td>: <strong>[NAMA_LENGKAP]</strong></td></tr>
            <tr><td>Nomor Induk Pegawai (NIP)</td><td>: [NIP]</td></tr>
            <tr><td>Jabatan</td><td>: [JABATAN]</td></tr>
            <tr><td>Unit Kerja / Departemen</td><td>: [UNIT_KERJA]</td></tr>
            <tr><td>Keperluan</td><td>: [KEPERLUAN]</td></tr>
        </table>
        {financialBlock}
        <p>Demikian surat keterangan ini kami buat dengan sebenar-benarnya untuk dipergunakan sebagaimana mestinya.</p>
    </div>
    <div class='footer'>
        <div class='verification'>
            <p style='margin: 0 0 4px 0; font-weight: bold;'>Verifikasi Keaslian Dokumen:</p>
            <p style='margin: 0;'>Pindai QR Code atau akses tautan resmi SIMRS Quilvian: [VERIFIKASI_URL]</p>
        </div>
        <div class='signature-box'>
            <p>[KOTA_RS], [TANGGAL_SURAT]<br/><strong>[JABATAN_PENANDATANGAN]</strong></p>
            <div style='height: 60px;'></div>
            <p style='text-decoration: underline; font-weight: bold; margin: 0;'>[NAMA_PENANDATANGAN]</p>
            <p style='margin: 0; font-size: 12px; color: #0284c7;'>[Telah Ditandatangani Secara Digital]</p>
        </div>
        <div style='clear: both;'></div>
    </div>
</body>
</html>";
        }

        private static string HashPinWithSalt(string pin, string salt)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes($"{salt}:{pin}:QUILVIAN_FIN_GUARD");
            return Convert.ToHexString(sha256.ComputeHash(bytes));
        }

        private static bool VerifyPin(string enteredPin, string storedHash)
        {
            if (string.IsNullOrWhiteSpace(storedHash) || !storedHash.Contains(':')) return false;
            var parts = storedHash.Split(':');
            if (parts.Length != 2) return false;
            var salt = parts[0];
            var expectedHash = parts[1];
            var computed = HashPinWithSalt(enteredPin, salt);
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(computed),
                Encoding.UTF8.GetBytes(expectedHash));
        }

        private static string MaskString(string input)
        {
            if (string.IsNullOrWhiteSpace(input) || input.Length <= 2) return "**";
            var first = input[0];
            var last = input[^1];
            var stars = new string('*', Math.Max(input.Length - 2, 3));
            return $"{first}{stars}{last}";
        }

        private static EmployeeDocumentIssuanceDto MapToDto(
            TrxEmployeeDocumentIssuance issuance,
            TrxEmployeeDocumentRequest? request,
            MstWorkforceProfile? profile,
            MstEmployeeDocumentType? docType)
        {
            return new EmployeeDocumentIssuanceDto
            {
                Id = issuance.Id,
                EmployeeDocumentRequestId = issuance.EmployeeDocumentRequestId,
                WorkforceProfileId = issuance.WorkforceProfileId ?? Guid.Empty,
                EmployeeName = profile?.DisplayName ?? string.Empty,
                EmployeeNumber = profile?.Employee?.EmployeeNumber ?? profile?.ProfileCode ?? string.Empty,
                DepartmentName = profile?.PrimaryDepartment?.DepartmentName ?? string.Empty,
                PositionName = profile?.PrimaryPosition?.PositionName ?? string.Empty,
                DocumentTypeId = docType?.Id ?? Guid.Empty,
                DocumentTypeName = docType?.DocumentTypeName ?? "Naskah Dinas",
                IssuanceNumber = issuance.IssuanceNumber,
                OfficialDocumentNumber = issuance.OfficialDocumentNumber,
                VersionNumber = issuance.VersionNumber,
                IssuanceStatus = issuance.IssuanceStatus,
                IssuedAt = issuance.IssuedAt,
                ValidFrom = issuance.ValidFrom,
                ValidUntil = issuance.ValidUntil,
                IsDigitallySigned = issuance.IsDigitallySigned,
                DigitallySignedAt = issuance.DigitallySignedAt,
                SignerName = issuance.SignerName,
                SignerTitle = issuance.SignerTitle,
                IsFinancialSensitive = issuance.IsFinancialSensitive,
                SalaryBrutoAmount = issuance.SalaryBrutoAmount,
                SalaryNettoAmount = issuance.SalaryNettoAmount,
                PayrollPayslipId = issuance.PayrollPayslipId,
                VerificationHash = issuance.VerificationHash,
                QrCodeImagePath = issuance.QrCodeImagePath,
                PdfFilePath = issuance.PdfFilePath,
                FileSizeBytes = issuance.FileSizeBytes,
                FileChecksum = issuance.FileChecksum,
                IsRevoked = issuance.IsRevoked,
                RevocationReason = issuance.RevocationReason
            };
        }

        #endregion
    }
}
